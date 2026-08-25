# [BIM_COST_ESTIMATE]

The host-neutral 5D/6D cost-carbon-and-resource projection over the `Rasm.Element` seam graph: one `CostItem` line joining an `IfcCostValue` applied rate to the takeoff `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` the line resolves at projection from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Element.Quantities` bag (or the cost line's own `IfcCostItem.CostQuantities`), one `ConstructionResource` record discriminated by the `ResourceKind` `[SmartEnum<string>]` over all six `IfcConstructionResource` modalities, and the `CostSchedule.Rollup` `Money`-fold that folds the resolved `(quantity x rate)` lines into one schedule total — one fold, never enumerated per-resource arms. The priced scalar is the `NodaMoney` `Money` `readonly struct` over a `decimal Amount` and an ISO 4217 `Currency`, never a `(double Amount, string Currency)` pair anywhere on the page: a `CostItem` value is `Money`, a unit rate is `Money / decimal` over the typed `CostValue.UnitBasis` denominator, the quantity x rate join is `Money * (decimal)quantity.Si` admitted only after the basis dimension is proved against the takeoff's, a composite IFC `IfcCostValue.Components` rate folds through the `IfcArithmeticOperatorEnum` operator into one `Money` (a non-monetary `IfcRatioMeasure` component lifting as the currencyless scalar factor), the `CostSchedule.Rollup` is a `Money` sum over the additive operator (`Money.AdditiveIdentity` the no-currency anchor) carrying the lossless per-currency `ByCurrency` subtotal partition BEFORE any convert — a genuinely mixed-currency estimate aggregates without a complete FX table — a cross-currency total composes `NodaMoney.Exchange.ExchangeRate.Convert` over the per-line-matched `Seq<ExchangeRate>` fx table (both rate legs matched, never a single-rate assumption), a lump-sum or contingency allocation across packages is the lossless `MoneyExtensions.Split` penny distribution rather than a remainder-losing `total / n` multiply, and the EARNED-VALUE metrics (BCWS/BCWP/ACWP plus the derived `CV`/`SV`) are `Money` while every RATIO-DERIVED read — `CPI`/`SPI`/`TCPI` the dimensionless `Money / Money → decimal` ratio and `EAC`/`VAC`/`ETC` the `Money` it forecasts — is an `Option` whose `None` IS the zero-denominator case, so an unspent schedule reads NO cost index rather than the unity a dashboard renders as on-plan, and the sign reads the native `Money.IsNegative` predicates — never a `(double, string Currency)` carrier. The whole estimate folds under one `NodaMoney.Context.MoneyContext` rounding/precision policy — the `CostMoney.Context` banker's-rounding value the ONE `CostMoney.Install` composition-root `DefaultThreadContext` seat binds before any projection mints a `Money` — so a single rounding rule governs every line rather than a `MidpointRounding` argument threaded through every construction, and the rule stays recoverable from that declaration and from any `Money.Context` the estimate carries.

The estimate is a VIEW of the federated `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a second quantity or schedule source: each `CostItem` reads its takeoff from the one seam `QuantitySet` bag the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation` already derived from the kernel geometry the `Node.Object` references by content key, joins to the priced elements through the `IfcRelAssignsToControl.RelatedObjects` cost-control set resolved against the seam graph by the `Node.Object.ExternalId` (the 1:1 IFC `GlobalId` projection attribute [H6]), and binds the resource it consumes to the `Planning/schedule#SCHEDULE` `ConstructionTask` activity network through `IfcRelAssignsToProcess` rather than re-modeling a parallel schedule — so a wall's net-volume takeoff, its concrete unit rate, and the labor crew that places it carry one cost line while the seam keeps the element's content-keyed geometry and the schedule keeps the activity's calendar. The estimate is HOST-NEUTRAL — it reads the in-process GeometryGym cost graph and joins to the seam by stable `GlobalId`/`NodeId`, never a RhinoCommon type — and the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, never a second selection surface. The `(QuantityKey, ResourceKey)` seam `ContentAddress` pair `CostSchedule.Identity` derives over the resolved `(globalIds, quantity, rate)` line triples is the reference the cross-libs `dotnet:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads the schedule by at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING`, and the `Rasm.AppUi/Charts` estimate report renders the same rollup — Bim PRODUCES the cost source, never re-pricing it downstream. A foreign IFC amount enters through the railed `CostMoney.Of((decimal)amount, iso4217, key)` boundary, which gates the code through `CurrencyInfo.TryFromCode` before construction and raises `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec` on a miss; an `IfcMonetaryMeasure` prices in the PROJECT currency (the `IfcContext.UnitsInContext` `IfcMonetaryUnit` resolved once at projection onto `CostSchedule.Currency` — the IFC law for monetary measures and the ONE currency source, `IfcCostValue.UnitBasis` being the measure denominator alone), and every cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600 owns the generated `Code`), never a `.ToError()` hop, a new fault family, or a thrown currency exception in domain logic.

## [01]-[INDEX]

- [02]-[ESTIMATE]: `CostItem` line (the summed `Values` rate set x ONE resolved `MeasureValue` takeoff by `GlobalId`), `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities, the `CostValue` rate carrier over the `NodaMoney` `Money`/`Currency`/`ExchangeRate` algebra with the `IfcCostValue.Components` aggregation fold, the `CostScheduleKind`/`ResourceKind`/`CostCategory` `[SmartEnum]` partitions, the `CostSchedule` record with its resolved reporting `Currency` + `Status`, its RAILED `Fin<CostRollup>` `Money`-fold `Rollup` (per-currency `ByCurrency` losslessly, the fx-table repriced `Total`), lossless `Apportion` `Split`, and CANONICALLY-ORDERED `(QuantityKey, ResourceKey)` identity, the `CostMoney` boundary lift + `Reprice` fx-match + the `Install`-seated `MoneyContext` policy, and the `CostProjection.Project` fold from the GeometryGym `IfcCostSchedule` surface over the seam graph.
- [03]-[EARNED_VALUE]: the `ChangeOrder` priced-revision record over a baseline `CostSchedule`, the `Contingency` `Money` reserve column its over-draw rejects rather than floors, and the currency-railed `CostSchedule.EarnedValue` fold (BCWS/BCWP/SV as `Money`, ACWP/CV as `Option<Money>` absent where no cost tier reports, the ratio-derived `CPI`/`SPI`/`TCPI` as `Option<decimal>` and `EAC`/`VAC`/`ETC` as `Option<Money>`) joining the three-tier actual-percent election (scan-observed evidence, the `Planning/schedule#SCHEDULE` `ConstructionTask` authored progress, the actual-interval fraction) and the two-tier ACWP evidence ladder (recorded accounting actuals, the resource-side `Spent`) to the self-contained priced lines.
- [04]-[CARBON]: the `CarbonEstimate.Rollup` 6D embodied-carbon fold — the material-true takeoff joined to the seam `MaterialPropertySet.Environmental` per-`LifecycleStage` GwpTotal vector over a query-algebra selection — `CarbonLine`/`CarbonGap`/`CarbonRollup` the stage-banded rollup with EPD provenance and counted coverage gaps.

## [02]-[ESTIMATE]

- Owner: `EstimateStage` the projection lane's OWN closed stage roster projecting the `Model/observability#HOOK_RAIL` `StageMark` onto the `rasm.bim.planning.progress` observe point; `CostSchedule` the single host-neutral 5D cost-and-resource record carrying the `CostScheduleKind` discriminant, the resolved reporting `Currency` (the project `IfcMonetaryUnit`, else the first priced value's — resolved ONCE at projection, never a head-of-items implicit per fold), the optional `Status` approval state (the GG `Staus` member), the self-contained priced `CostItem` line set (each line carrying its resolved takeoff `MeasureValue`), the `ConstructionResource` resource set, the `Contingency` reserve, and the `(QuantityKey, ResourceKey)` content-key identity the cross-libs cost catalog reads it by, with the railed `Fin<CostRollup>` `Money`-fold `Rollup` schedule-total and the lossless `Apportion` lump-sum distribution; `CostItem` the single priced line record joining the `IfcCostItem.CostValues` `Seq<CostValue>` applied-rate SET (bSI sums an item's cost values — a head-only read is the deleted form; same-currency within one line admitted at projection) to the ONE resolved `MeasureValue Quantity` (the seam takeoff or the explicit `IfcCostItem.CostQuantities`) by the priced element `GlobalId` set, carrying the optional `ResourceGlobalId` it consumes (the `IfcConstructionResource` the item's own `Controls` set assigns, routed OFF the priced set so a resource-controlling line never false-faults the element join) and the `ParentGlobalId` nesting reference the `IfcCostItem` `Nests` hierarchy declares — its `ValueOf` the pure `Σ Rate * (decimal)Quantity.Si` fold needing no graph; `ConstructionResource` the ONE record discriminated by the `ResourceKind` `[SmartEnum<string>]` over the six `IfcConstructionResource` modalities (`Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`), carrying its `MeasureValue BaseQuantity`, an optional `Money BaseCost` from `IfcConstructionResource.BaseCosts`, the optional `Skill` (the `IfcLaborResourceTypeEnum`/`IfcCrewResourceTypeEnum` `PredefinedType` descriptor), the optional consumed `Material` (read through the resource's `HasAssociations` `IfcRelAssociatesMaterial`), the optional `TaskGlobalId` it resources through `IfcRelAssignsToProcess`, and the optional `Completion` ratio (`IfcResourceTime.Completion` off the inherited `Usage` — the resource-side actual-progress axis feeding the `Spent` incurred read) — never a per-subtype class family; `CostValue` the applied-rate record carrying its `Money` value (the fold of the `IfcCostValue` direct amount OR its `Components` tree through the `ArithmeticOperator`), its `UnitBasis` typed per-unit `MeasureValue` denominator, and its `CostCategory` discriminant, the per-basis rate the native `Money / decimal` divide over that denominator's SI magnitude; `CostScheduleKind`/`ResourceKind`/`CostCategory` the cost-schedule-kind / resource-modality / cost-category `[SmartEnum<string>]` vocabularies; `CostMoney` the boundary lift folding a foreign IFC `(double amount, string iso4217)` into a typed `Money` after public `CurrencyInfo.TryFromCode` admission, raising `BimFault.Refused` with `BimReason.Codec` on a miss, the ONE `Reprice` fx-table repricing owner (both rate legs matched) `Rollup` and `EarnedValue` compose, plus the `MoneyContext` rounding policy; `CostProjection` the static fold over the GeometryGym `IfcCostSchedule` surface and the seam `ElementGraph`.
- Cases: `ConstructionResource` is ONE record whose `ResourceKind` row discriminates the modality — `Labor` (`IfcLaborResource`, `Skill` = `IfcLaborResourceTypeEnum` `PredefinedType`, `BaseQuantity` crew man-hours), `Material` (`IfcConstructionMaterialResource`, `Material` = the consumed material name through `HasAssociations`, `BaseQuantity` consumed volume/mass), `Equipment` (`IfcConstructionEquipmentResource`, `BaseQuantity` plant-hours), `Crew` (`IfcCrewResource`, `Skill` = `IfcCrewResourceTypeEnum`), `Product` (`IfcConstructionProductResource`, `Material` = the product), `Subcontract` (`IfcSubContractResource`) — each carrying the optional `BaseCost` and `TaskGlobalId`, a seventh modality being one `ResourceKind` row with zero new surface (6 modalities, the `[SmartEnum]` partition the discriminant); the `CostValue` value object carries its `Money` `Applied` rate (the `IfcCostValue.AppliedValue` `IfcMonetaryMeasure.Measure` lifted into one `Money` under the project `IfcMonetaryUnit` currency, or the `IfcAppliedValue.Components` sub-value tree folded through the `IfcArithmeticOperatorEnum` — never a `double` amount beside a bare currency string), its `UnitBasis` typed per-unit `MeasureValue` denominator (the dimensionless `1` for a unit rate, the SI-coerced `IfcCostValue.UnitBasis` `IfcMeasureWithUnit` for a per-basis rate), and its `CostCategory` (`Material`/`Labour`/`Equipment`/`Overhead`/`Subcontract`/`Preliminaries`/`Contingency`/`NotDefined` over the `IfcCostValue.Category` string) — the line rate the native `Applied / UnitBasis` (`Money / decimal → Money`) before multiplying the takeoff quantity; the `CostScheduleKind` rows `Budget`/`CostPlan`/`Estimate`/`Tender`/`PricedBillOfQuantities`/`UnpricedBillOfQuantities`/`ScheduleOfRates`/`UserDefined`/`NotDefined` (9) each frozen over the verified `IfcCostScheduleTypeEnum` member, the `ResourceKind` rows `Labor`/`Material`/`Equipment`/`Crew`/`Product`/`Subcontract`/`NotDefined` (7 — a `UserDefined` row is unreachable dead data because `Of(GetType().Name)` only ever yields the six subtype keys, `NotDefined` the sole `IfNone` fallback) each frozen with its `IfcDomain`, and the `CostCategory` rows (8) over the IFC cost category string.
- Law: the rounding regime is a property of every `Money` VALUE, not of a fold — the ctor stamps `MoneyContext.CurrentContext` into the value's own flag bits, `money.Context` reads it back, and `Add`/`Subtract`/`Remainder` THROW `MoneyContextMismatchException` across two contexts — so the ONE `CostMoney.Install` composition-root `DefaultThreadContext` seat governs every mint on the page and a fold-local `CreateScope` is the deleted form: it forks the context space between the lines projection minted outside it and the amounts `Reprice` mints inside it, and `ExchangeRate.Convert` (which takes no context argument and can only carry the ambient one) puts that fork on the reprice path of every cross-currency estimate.
- Entry: `CostProjection.Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key, Option<BimRail> rail = default)` folds one GeometryGym cost schedule into one self-contained `CostSchedule` — materializing the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcCostItem` set PLUS each item's transitive `IsNestedBy` sub-item tree once (the nested BoQ child lines a `Controls`-only read drops, deduped by `GlobalId`), building the `ExternalId → NodeId` index over `graph.ObjectNodes` once, resolving the project reporting `Currency` once (`MonetaryOf` over `IfcContext.UnitsInContext`, railed through `CostMoney.Of`), folding each cost item onto a `CostItem` line that resolves its WHOLE `CostValues` applied-rate set (the `Components` tree included; mixed real currencies within one item railing `cost-value-currency-mixed` at admission so `ValueOf` stays total), resolves the priced element `GlobalId` set against the index (`BimFault.Refused` with `BimReason.DanglingReference` BARE on a priced `GlobalId` the seam graph never declares as a `Node.Object.ExternalId`), and resolves the line takeoff `MeasureValue` (the explicit `IfcCostItem.CostQuantities` when present, else the dominant base quantity off the priced elements' seam `Bake`d `Element.Quantities`, else a unit lump-sum), and folding the resource set onto `ConstructionResource` rows discriminated by `ResourceKind.Of(resource.GetType().Name)` (a non-construction-resource entity filtered before the traverse rather than aborting the whole schedule; a resource `BaseCosts` currency fault railing `BimFault.Refused` with `BimReason.Codec` typed rather than silently dropping the cost); `CostProjection.ProjectAll` lifts every cost schedule in a federated graph onto the `Seq<CostSchedule>` the catalog reads, `CostSchedule.Rollup(Op key, Seq<ExchangeRate> fx = default)` folds the resolved per-value lines into the `Fin<CostRollup>` schedule total — the `ByCurrency` partition aggregating each value's NATIVE currency losslessly before any convert, the `Total` repricing through the `CostMoney.Reprice` fx-table match (railing `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec` on a foreign value carrying no matching rate, never a thrown `Money + Money` mismatch in domain logic) — and `CostSchedule.Apportion(Money lumpSum)` distributes a lump-sum (overhead, contingency) across the lines by their value-weight ratios through the lossless `MoneyExtensions.Split` (a zero-weight line set splits evenly — the lump is never dropped).
- Auto: `Project` reads the `IfcCostSchedule` runtime graph and folds it into the self-contained schedule — the `ItemsOf` projection materializes the schedule's `Controls` `IfcRelAssignsToControl` controlled `IfcCostItem` set plus each item's transitive `IsNestedBy` sub-item closure once (the `NestedItems` recursion mirroring the schedule WBS flatten, deduped by `GlobalId`; a parent line prices only its OWN authored values, a leaf-authored BoQ summing exactly); `ValuesOf` folds each item's WHOLE `CostValues` LIST onto the `Seq<CostValue>` line set (`AmountOf` reading the `IfcMonetaryMeasure.Measure` into one `Money` under the resolved MODEL currency — the project `IfcMonetaryUnit` is the ONE currency source and a `UnitBasis.UnitComponent` currency read is the deleted form, since the basis answers what a rate is PER and never what it is IN — OR lifting a non-monetary `IfcMeasureValue` leaf (an `IfcRatioMeasure` overhead factor) as the currencyless scalar, OR recursively folding the `IfcAppliedValue.Components` sub-value tree through the `IfcArithmeticOperatorEnum` `ADD`/`SUBTRACT`/`MULTIPLY`/`DIVIDE` operator, the `IfcCostValue.UnitBasis` lifting through `BasisOf` onto the typed `MeasureValue UnitBasis` — its `ValueComponent`'s own measure-type name resolving dimension and coercion axis through the owned `PropertyLowering.MeasureDimensions` table and its `UnitComponent` overriding the project regime, so the denominator arrives SI-coerced rather than in the model's declared units the SI takeoff cannot meet — the `IfcCostValue.Category` onto `CostCategory`); `QuantityOf` resolves the line takeoff once at projection (the explicit `IfcCostItem.CostQuantities` `IfcPhysicalSimpleQuantity` set decoded through the owned `Projection/value#PROPERTY_LOWERING` `PropertyLowering.Measure`, else `graph.Bake(node, key)` over each priced element and the dominant `MeasureValue` by `Dimension` rank `Volume ≻ Area ≻ Length ≻ Mass ≻ Duration` summed through `MeasureValue.Sum`, else the `Dimensionless` unit lump-sum), so the line value is the pure `Σ CostValue.Rate * (decimal)Quantity.Si` fold and the schedule needs no graph after projection; `CostItemOf` splits the item's `Controls` related set — the `IfcConstructionResource` head onto `ResourceGlobalId`, the `IfcProduct`s onto the priced set (a related `IfcProcess`/`IfcActor` is neither: the line prices as a unit lump-sum with no element join, never a false `Refused/BimReason.DanglingReference` abort) — and threads the `IfcCostItem` `Nests.RelatingObject.GlobalId` parent onto `ParentGlobalId` so a bill-of-quantities tree folds onto the flat line set with its nesting preserved; `ResourceOf` is ONE fold reading each `IfcConstructionResource` by runtime subtype onto a `ConstructionResource` row — `ResourceKind.Of(GetType().Name)` the discriminant, `MeasureOf(resource.BaseQuantity)` the `MeasureValue BaseQuantity`, the first `IfcConstructionResource.BaseCosts` `IfcAppliedValue` onto the optional `Money BaseCost`, the `IfcLaborResource`/`IfcCrewResource` `PredefinedType` onto `Skill`, the `IfcConstructionMaterialResource`/`IfcConstructionProductResource` associated `IfcMaterial.Name` onto `Material`, and the `OperatesOn`/`HasAssignments` `IfcRelAssignsToProcess.RelatingProcess` `IfcProcess.GlobalId` onto `TaskGlobalId` so the resource binds the `Planning/schedule#SCHEDULE` `ConstructionTask` it resources; `CostSchedule.Rollup` flattens the lines to per-value `(category, native)` pairs, partitions the NATIVE amounts by `Currency.Code` losslessly (`ByCurrency` — the mixed-currency aggregation no FX table gates), reprices each through `CostMoney.Reprice` into the schedule `Currency` (`ExchangeRate.Convert` on the fx-table rate matched on BOTH legs, railing `BimFault.Refused` with `BimReason.Codec` when no matching rate exists rather than throwing a `Money + Money` mismatch) then sums them into the `Fin<CostRollup>` schedule total through the additive operator, partitions the total by `CostCategory` per VALUE and the resource cost (`BaseCost * BaseQuantity`, repriced through the SAME `CostMoney.Reprice` owner so a foreign-currency resource rails typed rather than throwing inside the partition) by `ResourceKind` through one `Fold`; `CostSchedule.Identity` derives the `(QuantityKey, ResourceKey)` seam `ContentAddress` pair through `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` fold (length-prefixed strings under per-run `Ordinal` counts) across the CANONICALLY-ORDERED priced line `(globalIds, quantity.Si, values)` triples and resource `(GlobalId, kind, baseQuantity.Si)` rows (the line set by `GlobalId`, each priced-id and resource set sorted ordinally, the per-line value list LIST-ordered — STEP LISTs are stable across re-parse) so the key is invariant to the unstable `IfcSet` iteration order a re-parse yields and the catalog re-reads only a genuinely changed estimate.
- Output: the `Seq<CostSchedule>` is the cost evidence the cross-libs `dotnet:TELEMETRY_LAKE_ANALYTICS` 5D cost catalog reads by the `(QuantityKey, ResourceKey)` reference at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and the `Rasm.AppUi/Charts` estimate report renders, the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` arm selects the quantified element set the estimate prices, and the `CostRollup` schedule total with its `ByCurrency` native-amount partition (the lossless mixed-currency read no FX table gates) and its `CostCategory`/`ResourceKind` partitions is the 5D estimate evidence; the priced line carries its resolved takeoff, applied rate, and resourced task on one self-contained record joining the seam quantity and the schedule activity by reference, never a second quantity or schedule store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new governance checkpoint is one `EstimateStage` row whose declared fraction the observe point reads with no arithmetic elsewhere; a new cost-schedule kind is one `CostScheduleKind` row reading the next `IfcCostScheduleTypeEnum` member; a new construction-resource modality is one `ResourceKind` row with zero new surface (the ONE `ConstructionResource` record absorbs it on the discriminant — never a new union arm or class); a new cost category is one `CostCategory` row; a composite rate rides the existing `IfcAppliedValue.Components` fold and a second cost value on a line is one `CostValues` LIST entry riding the existing `ValuesOf` traverse; a new per-line binding is one column on `CostItem`; a new convertible currency pair is one `ExchangeRate` row in the `Rollup`/`EarnedValue` fx table and a currency needing no convert reads the lossless `ByCurrency` partition; a regional or custom currency is one `CurrencyInfo.TryAdd(CurrencyInfo)` registration; a new rounding rule (cash-denomination, exact) is one `CostMoney.Context` `MoneyContext` policy (`CashDenominationRounding`/`NoRounding`) the same `Install` seat binds, never a `MidpointRounding` argument; a new cost schedule rides the existing `ProjectAll` fold on one row; never a per-resource-type cost record, never a parallel `LaborCost`/`MaterialCost` class family, never a `GetLaborCost`/`GetByCategory` operation family, never a hand-rolled `MonetaryAmount` `(double, string)` carrier beside `Money`, and never a second takeoff or schedule source.
- Boundary: `CostSchedule` is ONE record discriminated by the `CostScheduleKind` row, and `ConstructionResource` is ONE record discriminated by the `ResourceKind` `[SmartEnum]` over all six IFC modalities — a `Labor`/`Material`/`Equipment` `[Union]` slicing only three of six subtypes, a `LaborResource`/`MaterialResource` class family, or three sibling factory methods is the deleted form (the collapse to one record keyed by the smart enum removes the repeated `Switch` accessors and covers every modality), mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`; the priced scalar is the `NodaMoney` `Money` `readonly struct` and the priced DENOMINATOR is the seam `MeasureValue`, so a bare-`decimal` unit basis whose unit the projection discarded, an uncoerced native-unit basis divided into an SI takeoff, a hand-rolled `MonetaryAmount` `(double, string)` record, a `double` cost-arithmetic helper, a naive `total / n` allocation where `MoneyExtensions.Split` is lossless, a stringly currency field validated by hand where the `Money(decimal, string)` ctor resolves the ISO 4217 registry, a thrown `InvalidCurrencyException` in domain code instead of the railed `CostMoney.Of`, and a second rounding policy threaded as a `MidpointRounding` argument or opened as a fold-local `MoneyContext.CreateScope` where the one `Install`-seated `CostMoney.Context` governs are the RETIRED forms; the estimate is a VIEW of the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` — the priced quantity is the seam `QuantitySet` `MeasureValue` takeoff joined by `Node.Object.ExternalId` (or the explicit `IfcCostItem.CostQuantities`), a re-derived parallel takeoff or a re-tessellation in this owner being the named seam violation, and the resourced schedule is the `Planning/schedule#SCHEDULE` `ConstructionTask` network joined by `TaskGlobalId`, a re-modeled cost-side schedule being the named seam violation; the retired `BimModel`/`BimElement` collection is GONE — a `federated.Elements` scan over a second stored element record is the deleted form, the cost reading the seam graph the `Bake` fold derives the consumer `Element` from; the GeometryGym `IfcCostSchedule`/`IfcCostItem`/`IfcCostValue`/`IfcAppliedValue`/`IfcConstructionResource` and its six subtypes / `IfcRelAssignsToControl`/`IfcRelAssignsToProcess`/`IfcRelAssociatesMaterial` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 8-16) is consumed as settled vocabulary and a hand-rolled cost reader is the deleted form, the `IfcPhysicalSimpleQuantity`->`MeasureValue` decode composing the owned `Projection/value#PROPERTY_LOWERING` `PropertyLowering.Measure` (one decode owner) rather than a duplicate dimension/value switch; the `NodaMoney` `Money`/`Currency` cross the `Exchange/wire` boundary through `MoneyJsonConverter`/`CurrencyJsonConverter` or the integer `ToMinorUnits` form and never leak past the cost owner; the quantified-element selection is the `Model/query#ELEMENT_SET` `ByProperty(Qto_*)` predicate and a parallel cost-element selection arm is the no-second-selection-surface reject; the `(QuantityKey, ResourceKey)` identity is a typed seam `ContentAddress` pair derived through `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` fold over canonically-ordered line/resource rows (invariant to the unstable `IfcSet` iteration order), the ONE codec and the ONE kernel seed-zero hasher the `Review/diff#MODEL_DIFF` `ElementFingerprint`, the `Review/versioning#VERSION_GRAPH` `CommitKey`, and the `Review/validation#IDS_FACETS` `FacetKey` all state their keys in — a hand-rolled `XxHash128`/`Encoding.UTF8` string-join preimage, whose delimiter choice can forge an equality between two decompositions of one concatenation, minting a second identity scheme for the catalog join, and storing the raw `UInt128` that erases the one content-key type (the `.Value` unwrap `Review/versioning#VERSION_GRAPH` already deleted) are the named defects; the `CostSchedule.Rollup` total is ONE RAILED `Money` fold over the resolved per-value lines AND the repriced resource costs (a foreign value or resource `BaseCost` lacking a matching `ExchangeRate` in the fx TABLE lifting `BimFault.Refused` with `BimReason.Codec` BARE, never a thrown `Money + Money` mismatch in domain logic or inside a partition; a single-rate `Option<ExchangeRate>` parameter that cannot reprice a three-currency estimate is the deleted form, as is a base-leg-only rate match converting into a third currency, as is an unrepriced `ByResourceKind` partition whose accumulator can throw), never enumerated per-resource arms; the reporting currency is the resolved `CostSchedule.Currency` field and a head-of-items implicit derived per fold is the deleted form; a cost rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop or a bare `Fin.Fail` without the typed case is the named defect the rebuilt `Model/faults#FAULT_BAND` band closes.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using NodaMoney;
using NodaMoney.Context;
using NodaMoney.Exchange;
using Rasm.Bim.Model;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;
using BimRail = Rasm.Domain.HookRail<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Planning;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class EstimateStage {
    public static readonly EstimateStage Indexed   = new(done: 0.00, witness: "index");
    public static readonly EstimateStage Priced    = new(done: 0.10, witness: "price");
    public static readonly EstimateStage Resourced = new(done: 0.80, witness: "resource");
    public static readonly EstimateStage Settled   = new(done: 1.00, witness: "settle");

    public double Done { get; }
    public string Witness { get; }

    public StageMark Mark => new(Done, Witness);

    public Unit Beat(Option<BimRail> rail, Op key) =>
        rail.IfSome(live => ignore(live.Fire(BimPoint.PlanningProgress, new BimFact.Progress(key, ProgressLane.Planning, Mark), key)));
}

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

    public static Option<CostScheduleKind> TryGet(string key) =>
        TryGet(key, out CostScheduleKind? row) && row is { } hit ? Some(hit) : None;

    public static CostScheduleKind Of(IfcCostScheduleTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

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
    public static readonly ResourceKind NotDefined   = new("NOTDEFINED", IfcDomain.Architecture);

    public IfcDomain Domain { get; }

    public static Option<ResourceKind> TryGet(string key) =>
        TryGet(key, out ResourceKind? row) && row is { } hit ? Some(hit) : None;

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

    public static Option<CostCategory> TryGet(string key) =>
        TryGet(key, out CostCategory? row) && row is { } hit ? Some(hit) : None;

    public static CostCategory Of(string? category) =>
        Optional(category).Map(static text => text.Trim()).Bind(TryGet).IfNone(NotDefined);
}

// --- [MODELS] --------------------------------------------------------------------------
public static class CostMoney {
    public static readonly MoneyContext Context = MoneyContext.Create(static options => {
        options.RoundingStrategy = new StandardRounding();
        options.EnforceZeroCurrencyMatching = false;
    }, name: "rasm-cost");

    public static MoneyContext Install() => MoneyContext.DefaultThreadContext = Context;

    public static Fin<Money> Of(decimal amount, string iso4217, Op key) =>
        iso4217.Trim() is { Length: > 0 } code
            ? CurrencyInfo.TryFromCode(code, out var currency)
                ? Fin.Succ(new Money(amount, (Currency)currency))
                : Fin.Fail<Money>(new BimFault.Refused(key, BimScope.Planning, BimReason.Codec,
                    string.Join(':', new object?[] { "cost-currency", "unknown", code })))
            : Fin.Succ(new Money(amount, Currency.NoCurrency));

    public static Fin<Money> Reprice(Money value, Currency report, Seq<ExchangeRate> fx, Op key) =>
        value.Currency == report || value.Currency == Currency.NoCurrency
            ? Fin.Succ(value)
            : fx.Find(rate => rate.BaseCurrency == value.Currency && rate.QuoteCurrency == report).Match(
                Some: rate => Fin.Succ(rate.Convert(value)),
                None: () => Fin.Fail<Money>(new BimFault.Refused(key, BimScope.Planning, BimReason.Codec, string.Join(':', new object?[] { "cost-currency", "unconvertible", value.Currency.Code, report.Code }))));
}

public sealed record CostValue(Money Applied, MeasureValue UnitBasis, CostCategory Category) {
    public Money Rate => Applied / (decimal)UnitBasis.Si;
}

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

    public Money Spent => Completion.Map(c => Cost * (decimal)c).IfNone(Money.AdditiveIdentity);
}

public sealed record CostItem(
    string GlobalId,
    string Name,
    Seq<CostValue> Values,
    MeasureValue Quantity,
    Seq<string> PricedGlobalIds,
    Option<string> ResourceGlobalId,
    Option<string> ParentGlobalId) {
    public Money ValueOf() => Values.Fold(Money.AdditiveIdentity, (total, value) => total + value.Rate * (decimal)Quantity.Si);
}

public sealed record CostRollup(
    Money Total,
    Map<string, Money> ByCurrency,
    Map<string, Money> ByCategory,
    Map<string, Money> ByResourceKind);

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

    public (ContentAddress QuantityKey, ContentAddress ResourceKey) Identity => (
        ContentAddress.Of(Items, 0.0, static (items, writer) =>
            toSeq(items.OrderBy(static i => i.GlobalId, StringComparer.Ordinal))
                .Fold(writer.Ordinal(items.Count), static (w, item) => item.Values
                    .Fold(toSeq(item.PricedGlobalIds.OrderBy(static g => g, StringComparer.Ordinal))
                        .Fold(w.String(item.GlobalId).Ordinal(item.PricedGlobalIds.Count),
                            static (run, priced) => run.String(priced))
                        .Double(item.Quantity.Si).Ordinal(item.Values.Count),
                        static (run, value) => run.String(value.Rate.Amount.ToString()).String(value.Rate.Currency.Code).String(value.Category.Key)))),
        ContentAddress.Of(Resources, 0.0, static (resources, writer) =>
            toSeq(resources.OrderBy(static r => r.GlobalId, StringComparer.Ordinal))
                .Fold(writer.Ordinal(resources.Count),
                    static (w, resource) => w.String(resource.GlobalId).String(resource.Kind.Key).Double(resource.BaseQuantity.Si))));

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

    public Seq<(CostItem Line, Money Share)> Apportion(Money lumpSum) =>
        Items.IsEmpty
            ? Seq<(CostItem Line, Money Share)>()
            : Items.Map(Weight).ToArray() is var weights && weights.Any(static w => w > 0)
                ? Items.Zip(toSeq(lumpSum.Split(weights)), static (line, share) => (Line: line, Share: share))
                : Items.Zip(toSeq(lumpSum.Split(Items.Count)), static (line, share) => (Line: line, Share: share));

    static int Weight(CostItem line) =>
        (int)Math.Clamp(Math.Round(line.ValueOf().Amount * 100m, MidpointRounding.AwayFromZero), 0m, int.MaxValue);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CostProjection {
    static Option<string> TextOf(string? value) =>
        Optional(value).Map(static text => text.Trim()).Filter(static text => text.Length > 0);

    public static Fin<CostSchedule> Project(IfcCostSchedule schedule, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key, Option<BimRail> rail = default) {
        ignore(EstimateStage.Indexed.Beat(rail, key));
        var index = graph.ObjectNodes.Fold(Map<string, NodeId>(),
            static (map, o) => o.ExternalId.Match(Some: id => map.AddOrUpdate(id, o.Id), None: () => map));
        UnitScheme scale = IfcUnits.SchemeOf(schedule.Database);
        return MonetaryOf(schedule, key)
            .Bind(model =>
                from pricing in Opened(EstimateStage.Priced, rail, key)
                from items in ItemsOf(schedule).TraverseM(item => CostItemOf(item, index, graph, model, scale, key)).As()
                from resourcing in Opened(EstimateStage.Resourced, rail, key)
                from rows in resources
                    .Filter(static r => ResourceKind.Of(r.GetType().Name) != ResourceKind.NotDefined)
                    .TraverseM(r => ResourceOf(r, model, key)).As()
                from settling in Opened(EstimateStage.Settled, rail, key)
                select new CostSchedule(
                    schedule.GlobalId,
                    CostScheduleKind.Of(schedule.PredefinedType),
                    TextOf(schedule.Name).IfNone(""),
                    model == Currency.NoCurrency
                        ? items.Bind(static i => i.Values).Map(static v => v.Applied.Currency)
                            .Filter(static c => c != Currency.NoCurrency).Head.IfNone(Currency.NoCurrency)
                        : model,
                    Optional(schedule.Staus).Filter(static s => s.Length > 0),
                    items,
                    rows));
    }

    public static Fin<Seq<CostSchedule>> ProjectAll(Seq<IfcCostSchedule> schedules, Seq<IfcConstructionResource> resources, ElementGraph graph, Op key, Option<BimRail> rail = default) =>
        schedules.TraverseM(schedule => Project(schedule, resources, graph, key, rail)).As();

    static Fin<Unit> Opened(EstimateStage stage, Option<BimRail> rail, Op key) => Fin.Succ(stage.Beat(rail, key));

    static Fin<Currency> MonetaryOf(IfcCostSchedule schedule, Op key) =>
        Optional(schedule.Database?.Project?.UnitsInContext)
            .Bind(static units => Optional(units.Units.OfType<IfcMonetaryUnit>().FirstOrDefault()))
            .Match(
                Some: unit => CostMoney.Of(0m, unit.Currency, key).Map(static zero => zero.Currency),
                None: () => Fin.Succ(Currency.NoCurrency));

    static Seq<IfcCostItem> ItemsOf(IfcCostSchedule schedule) =>
        toSeq(schedule.Controls
            .SelectMany(static rel => rel.RelatedObjects.OfType<IfcCostItem>())
            .SelectMany(NestedItems)
            .DistinctBy(static item => item.GlobalId));

    static Seq<IfcCostItem> NestedItems(IfcCostItem item) =>
        Seq(item) + toSeq(item.IsNestedBy
            .SelectMany(static rel => rel.RelatedObjects.OfType<IfcCostItem>()))
            .Bind(NestedItems);

    static Fin<CostItem> CostItemOf(IfcCostItem item, Map<string, NodeId> index, ElementGraph graph, Currency model, UnitScheme scale, Op key) {
        var related = toSeq(item.Controls.SelectMany(static rel => rel.RelatedObjects));
        var priced = toSeq(related.OfType<IfcProduct>()
            .Select(static o => o.GlobalId)
            .Where(static id => id.Length > 0));
        var resource = Optional(related.OfType<IfcConstructionResource>()
            .Select(static r => r.GlobalId)
            .FirstOrDefault(static id => id.Length > 0));
        return priced.Find(id => !index.ContainsKey(id)).Match(
            Some: id => Fin.Fail<CostItem>(new BimFault.Refused(key, BimScope.Planning, BimReason.DanglingReference, string.Join(':', new object?[] { "cost-priced-miss", id }))),
            None: () =>
                from values in ValuesOf(item, model, scale, key)
                from same in guard(
                    values.Map(static v => v.Applied.Currency).Filter(static c => c != Currency.NoCurrency).Distinct().Count <= 1,
                    new BimFault.Refused(key, BimScope.Planning, BimReason.Codec, string.Join(':', new object?[] { "cost-currency", "value-mixed", item.GlobalId })))
                from quantity in QuantityOf(item, priced.Choose(index.Find), graph, scale, key)
                from basis in guard(
                    values.ForAll(v => v.UnitBasis.Dimension == Dimension.Dimensionless || v.UnitBasis.Dimension == quantity.Dimension),
                    new BimFault.Refused(key, BimScope.Planning, BimReason.Codec, string.Join(':', new object?[] { "cost-basis-dimension", item.GlobalId, quantity.Dimension.SiSymbol.IfNone("si-unrostered") })))
                select new CostItem(item.GlobalId, TextOf(item.Name).IfNone(""), values, quantity, priced, resource,
                    Optional(item.Nests?.RelatingObject?.GlobalId)));
    }

    static Fin<Seq<CostValue>> ValuesOf(IfcCostItem item, Currency model, UnitScheme scale, Op key) =>
        item.CostValues.AsIterable().ToSeq()
            .TraverseM(value =>
                from applied in AmountOf(value, model, key)
                from basis in BasisOf(value, scale, key)
                select new CostValue(applied, basis, CostCategory.Of(value.Category)))
            .As();

    static Fin<Money> AmountOf(IfcAppliedValue value, Currency model, Op key) =>
        value.AppliedValue switch {
            IfcMonetaryMeasure monetary => Fin.Succ(new Money((decimal)monetary.Measure, model)),
            IfcMeasureValue measure => Fin.Succ(new Money((decimal)measure.Measure, Currency.NoCurrency)),
            _ => value.Components.AsIterable().ToSeq() is { IsEmpty: false } components
                ? components.TraverseM(c => AmountOf(c, model, key)).As()
                    .Bind(parts => parts.Map(static p => p.Currency).Filter(static c => c != Currency.NoCurrency).Distinct().Count <= 1
                        ? Fin.Succ(Aggregate(value.ArithmeticOperator, parts))
                        : Fin.Fail<Money>(new BimFault.Refused(key, BimScope.Planning, BimReason.Codec, string.Join(':', new object?[] { "cost-currency", "component-mixed" }))))
                : Fin.Succ(new Money(0m, Currency.NoCurrency)),
        };

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

    static readonly Fin<MeasureValue> UnitRate = MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, 1d);

    static Fin<MeasureValue> BasisOf(IfcAppliedValue value, UnitScheme scale, Op key) =>
        value.UnitBasis?.ValueComponent is IfcMeasureValue basis
        && basis.Measure > 0d
        && PropertyLowering.MeasureDimensions.TryGetValue(basis.GetType().Name, out Dimension row)
            ? MeasureValue.OfSi(QuantityType.Create(basis.GetType().Name), row,
                PropertyLowering.Coerce(scale, basis.Measure, basis.GetType().Name, row, value.UnitBasis.UnitComponent), key: key)
            : UnitRate;

    static Fin<MeasureValue> QuantityOf(IfcCostItem item, Seq<NodeId> priced, ElementGraph graph, UnitScheme scale, Op key) =>
        Measures(item.CostQuantities.AsIterable().ToSeq(), scale, key).Bind(explicitQuantities =>
            explicitQuantities.IsEmpty
                ? priced.TraverseM(id => graph.Bake(id, key)).As()
                    .Bind(elements => Dominant(elements.Bind(static e => e.Quantities).Bind(static b => b.Values.Values.ToSeq()), key))
                : Dominant(explicitQuantities, key));

    static readonly Seq<Dimension> PricingRank =
        Seq(Dimension.VolumeDim, Dimension.AreaDim, Dimension.LengthDim, Dimension.MassDim, Dimension.DurationDim);

    static Fin<MeasureValue> Dominant(Seq<MeasureValue> measures, Op key) =>
        PricingRank.Choose(d => measures.Filter(m => m.Dimension == d) is { IsEmpty: false } same ? Some(same) : None)
            .Head
            .Match(Some: same => MeasureValue.Sum(same, key), None: () => MeasureValue.OfSi(Dimension.Dimensionless, 1d, key));

    static Fin<Seq<MeasureValue>> Measures(Seq<IfcPhysicalQuantity> quantities, UnitScheme scale, Op key) =>
        quantities.Choose(static quantity => quantity as IfcPhysicalSimpleQuantity)
            .TraverseM(simple => PropertyLowering.Measure(simple, scale, key))
            .As();

    static Fin<ConstructionResource> ResourceOf(IfcConstructionResource resource, Currency model, Op key) =>
        BaseCostOf(resource, model, key).Map(baseCost => new ConstructionResource(
            resource.GlobalId, TextOf(resource.Name).IfNone(""), ResourceKind.Of(resource.GetType().Name),
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

- Owner: `ChangeOrder` the priced-revision record carrying the baseline `CostSchedule` `GlobalId`, the priced `CostItem` delta set (added/modified/removed lines against the baseline), the `ChangeOrderStatus` `[SmartEnum<string>]` approval state, and the revision `Instant`; `Contingency` a `CostCategory.Contingency` `Money` reserve carried on `CostSchedule` a drawdown decrements through the native `Money` subtraction RAILED on currency AND on sufficiency (a foreign-currency draw and a draw exceeding the reserve both fault typed, never a thrown mismatch and never a floor-at-zero that reads an uncovered draw as a satisfied one); `EarnedValueReport` the typed report carrying BCWS (planned value) and BCWP (earned value) as `Money` beside ACWP (actual cost) as `Option<Money>` — actual cost is EVIDENCE, absent where no tier reports it — with the derived `SV = BCWP − BCWS` as `Money` and `CV = BCWP − ACWP` as `Option<Money>`, their `Money.IsNegative` sign reads ARE the `BehindSchedule` predicate and the `Option<bool> OverBudget`, beside the RATIO-DERIVED family every one of whose members is an `Option` — the cost-performance index `CPI = BCWP/ACWP`, schedule-performance index `SPI = BCWP/BCWS`, and to-complete index `TCPI = (BAC − BCWP)/(BAC − ACWP)` as `Option<decimal>` over the dimensionless `Money / Money → decimal` ratio, the estimate-at-completion `EAC = BAC/CPI`, variance-at-completion `VAC = BAC − EAC`, and estimate-to-complete `ETC = EAC − ACWP` as `Option<Money>` — because a zero denominator means the index has NO evidence, and unity is the one reading a dashboard renders as exactly-on-plan; never a `(double, string Currency)` carrier (the currency rides each `Money`) and never a hand-written `< 0` on the raw `decimal`; `CostSchedule.EarnedValue` the currency-RAILED, task-join-TOTAL fold joining three progress tiers and two cost tiers to the self-contained priced lines at a status `Instant`, never a generic ledger — the `observed` scan-verification feed (`Planning/progress#PROGRESS_EVIDENCE` `ProgressEvidence.Observed` keyed by `TaskGlobalId`, its present measurements alone) and the external `actuals` incurred-cost feed entering as the two caller-supplied `Map`s, the `Planning/schedule#SCHEDULE` `ConstructionTask` authored progress and the resource-side `ConstructionResource.Spent` as the schedule-owned middle tiers, and the interval-derived fraction as the progress floor with NO cost floor beneath the two cost tiers.
- Entry: `CostSchedule.EarnedValue(ScheduleNetwork network, Instant statusDate, Op key, Map<string, Money> actuals = default, Map<string, double> observed = default, Seq<ExchangeRate> fx = default)` folds the `Fin<EarnedValueReport>` at a status date — each `CostItem` joins its priced element set to the `Planning/schedule#SCHEDULE` `ConstructionTask` that assigns it (by the `TaskAssignment` `GlobalId` membership, the FIRST priced element carrying an assignment — a head-only read starving a line whose leading element is unassigned is the deleted form), reads the task's planned percent-complete (the fraction of the task's scheduled `Interval` elapsed at `statusDate`) and elects its actual percent-complete over three ORDERED tiers — the `observed` scan-verified fraction the `Planning/progress#PROGRESS_EVIDENCE` report supplies per `TaskGlobalId`, else the task's AUTHORED progress (the `IfcTaskTime.Completion` ratio the schedule owns, `1.0` on a `Completed` status), else the actual-`Interval` fraction — and contributes `line.ValueOf() × plannedPercent` to BCWS and `× actualPercent` to BCWP, while ACWP elects over its own two EVIDENCE tiers: the line's recorded incurred cost-to-date the `actuals` map supplies per `CostItem.GlobalId` (the EVM cost axis an accounting/Persistence feed produces), else the `ConstructionResource.Spent` of the resource the line's `ResourceGlobalId` names in the same `CostSchedule.Resources` set (available with no second feed, and skipped when that resource authored no `Completion` so a resource carrying no incurred evidence never reads a false zero) — so with `actuals` supplied CPI is a TRUE cost index independent of SPI, with only the resource tier reachable it is a resource-declared cost index, and with neither the schedule has NO actual cost and reports none; the report partitions BCWS/BCWP `Money` over the line set, accrues ACWP as an absence-propagating `Option<Money>`, derives `SV` total and `CV` through that absence, and derives the CPI/SPI/TCPI/EAC/VAC/ETC family as `Option`s absent at a zero or absent denominator; the fold is TOTAL on the task join (a line whose assigning task the network never declares still contributes its budget to BAC and its elected spend to ACWP, never a fault) and RAILED on currency — every line budget, recorded actual, and resource `Spent` reprices to the schedule `Currency` through `CostMoney.Reprice`, so a mixed-currency schedule faults typed instead of the accumulator `Money + Money` THROWING mid-fold, the exception-in-domain defect the prior total-claim prose hid. `ChangeOrder.Apply(CostSchedule baseline)` folds the priced delta set onto the baseline producing the revised `CostSchedule` (the delta lines added/superseding/removing the baseline lines by `GlobalId`) so a revision is the existing `CostItem`/`CostValue` algebra applied against a baseline, never a parallel revision store, and `CostSchedule.Drawdown(Money draw, Op key)` decrements the `Contingency` `Money` reserve through the `Fin<CostSchedule>` fold railed on BOTH currency and sufficiency — a draw the reserve cannot cover lifts `BimFault.Refused` with `BimReason.Rejected` (`cost-contingency-overdraw`) BARE rather than clamping to a zero remainder a caller then reads as a covered allocation.
- Auto: `EarnedValue` reads each `CostItem.ValueOf()` budgeted line value repriced through `CostMoney.Reprice` (the pure `Quantity × Σrate` in the schedule `Currency`, the `BAC` budget-at-completion summing the line set as `Money`), indexes the schedule's own `Resources` by `GlobalId` once so the resource tier costs no second pass, resolves each line's assigning `ConstructionTask` through the `network.Assignments` `TaskAssignment` join over the first assigned priced element (`PricedGlobalIds.Choose(taskByElement.Find).Head`), computes the task planned percent through the one `Fraction` clamped interval law over the scheduled `Interval` and elects the actual percent through the `Option` alternative chain `observed.Find(task.GlobalId) | authored | actual-interval fraction` (`0.0` when every tier is absent), and folds `BCWS += budget × planned`, `BCWP += budget × actual`, and accrues `ACWP` from the elected `recorded | spent` incurred cost, absent where neither tier resolves — every tier repriced through the same `CostMoney.Reprice` owner, the `Money` accumulators summed through the additive operator and the ACWP accumulator through the absence-propagating `Accrue`; the report derives every ratio through the ONE `Ratio` guard answering `None` at a zero denominator — `CPI = BCWP/ACWP`, `SPI = BCWP/BCWS`, `TCPI = (BAC − BCWP)/(BAC − ACWP)` (the `Money / Money → decimal` ratio) — and forecasts `EAC = BAC/CPI` off a NON-ZERO `CPI` alone (the `Money / decimal → Money` divide; a zero index means work was spent with nothing earned and no finite completion cost exists), `VAC = BAC − EAC` and the estimate-to-complete `EAC − ACWP` riding that same `Option`, while the `CV`/`SV` `Money` variances stay total — one `Fold` over the `(line, task)` join, never enumerated per-line arms; `ChangeOrder.Apply` folds the delta set onto the baseline line map keyed by `CostItem.GlobalId` (an added line inserts, a modified line supersedes, a removed line drops) so the revised schedule re-rolls through the existing `Rollup` fold.
- Output: the `EarnedValueReport` is the typed 5D cost-performance evidence the `Rasm.AppUi/Charts` `EarnedValue/ChangeOrder` report renders — `OverBudget` reads `Money.IsNegative` off the `Option<Money>` `Cv` and is itself absent until a cost tier reports, `BehindSchedule` reads `Money.IsNegative(Sv)`, an `Eac` exceeding `BAC` reads forecast-overrun and a `Tcpi` above one the demanded remaining efficiency — each read through its `Option`, so a dashboard renders "no index" where the denominator carried no evidence instead of a fabricated on-plan row — and `Vac` the cost variance at completion as `Option<Money>`; the `ChangeOrder` revision audit reads the baseline-to-revised line delta and the `ChangeOrderStatus` approval state, and the `Contingency` drawdown reads the remaining `Money` reserve — each carried on the one tracked `CostSchedule`, never a second cost-performance store.
- Packages: GeometryGymIFC_Core, NodaMoney, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new earned-value metric (a weighted-EAC formula variant `EAC = ACWP + (BAC − BCWP)/(CPI × SPI)`, a schedule-adjusted forecast) is one derived scalar on `EarnedValueReport` over the same BCWS/BCWP/ACWP fold the realized `CV`/`SV`/`ETC`/`TCPI` reads already prove; a new change-order status is one `ChangeOrderStatus` row; a new contingency-allocation rule rides the `CostSchedule.Apportion` `Split`; a new progress source is one rung on the existing `Option` alternative chain (a survey feed keyed by `TaskGlobalId` enters as the `observed` map with zero new surface) and a recorded actual-cost feed rides the existing `actuals` map keyed by `CostItem.GlobalId`; never a per-metric report record, never a parallel revision or contingency store, never a re-derived progress source, never a second actual-cost store, and never a `(double, string)` metric carrier.
- Boundary: the actual-percent election is a FIXED three-tier precedence — scan-`observed` evidence outranks the authored claim because dispute-grade physical measurement beats a self-reported percent, the authored `PercentComplete` (and its `Completed`-status `1.0`) outranks the interval fraction because a stated ratio beats an elapsed-time inference, and the fraction is the floor — so a caller-selected tier flag, a tier order that lets an authored percent shadow supplied evidence, and an averaging of two tiers are each the deleted form; the ACWP ladder is the matching fixed precedence over EVIDENCE ALONE — recorded accounting actuals, then the resource-side `ConstructionResource.Spent` joined by `CostItem.ResourceGlobalId` over the schedule's OWN `Resources` set, and then ABSENCE — and reading `Spent` off a resource with no authored `Completion` (an additive-identity zero standing in for real incurred cost) is the named defect that join's `Completion` filter closes; the schedule-duration proxy beneath those tiers is the DELETED form, the ASSERTED-VALUE defect in its purest shape — its overrun ratio defaults to 1 on a task with no actual interval, so ACWP lands exactly on BCWP, CPI lands exactly on 1, and EAC lands exactly on BAC, manufacturing the perfectly-on-budget reading out of the one input state carrying no cost evidence at all; the earned-value join reads the `Planning/schedule#SCHEDULE` `ConstructionTask` actual/scheduled `Interval` progress by `GlobalId` and re-deriving progress in this owner is the named seam violation — the schedule owns the activity network and its actual interval, `Planning/progress#PROGRESS_EVIDENCE` owns the measured one, and the cost owner reads the percent-complete each implies; the measured metrics are `Money` (BCWS/BCWP/SV) or `Option<Money>` on the axes whose evidence is itself absent at no cost tier (ACWP/CV) and the dimensionless `decimal` `Money / Money` ratio (CPI/SPI/TCPI), the sign reads the native `Money.IsNegative` predicates, and a `(double Bac, …, string Currency)` carrier or a hand-written `< 0` on the raw `decimal` is the deleted form mirroring the no-`(double, string)`-money law of `[2]-[ESTIMATE]`; every ratio-derived read carries its zero-denominator case as a typed `Option` absence and the guarded UNITY substitute is the named deleted form — a `CPI` of 1 on a schedule that earned value against zero recorded spend, an `SPI` of 1 on an unplanned window, a `TCPI` of 1 where the actuals already consumed the whole budget, and an `EAC` of `BAC` where the earned value is zero each report exactly-on-plan out of no evidence at all, the ASSERTED-VALUE defect a divide-by-zero guard mints while looking like arithmetic hygiene, and one `Ratio` guard owns the whole family so a new index inherits the law rather than restating it; the `ChangeOrder` delta is a priced revision against a baseline `CostSchedule` reusing the existing `CostItem`/`CostValue`/`Money` algebra and a parallel `CostRevision` class family or a second revision store is the deleted form; the `Contingency` is a `CostCategory.Contingency` `Money` reserve on the one `CostSchedule`, never a parallel reserve store, and a draw the reserve cannot cover REJECTS typed — the floor-at-zero remainder is the deleted form because a clamped reserve reports an uncovered allocation as a satisfied one and erases exactly the overrun a contingency exists to expose; the `EarnedValueReport` is the typed report and a generic ledger is the named defect; the fold joins the cost line to the schedule task by the `TaskAssignment` `GlobalId` membership, its actual percent reads a SUPPLIED tier before any interval re-derivation (re-deriving progress a capture measured or the schedule authored is the named seam violation), and a parallel cost-side schedule is the named seam violation; the fold is TOTAL on the task join and RAILED on currency — a bare `EarnedValueReport` return whose accumulator `Money + Money` can THROW on a mixed-currency schedule is the deleted form (the prose claimed total, the code threw — the exception-in-domain defect), and a vestigial rail claiming an abort the code never performs remains equally deleted.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using LanguageExt;
using NodaMoney;
using NodaMoney.Exchange;
using NodaTime;
using Rasm.Bim.Model;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Planning;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ChangeOrderStatus {
    public static readonly ChangeOrderStatus Proposed = new("PROPOSED");
    public static readonly ChangeOrderStatus Submitted = new("SUBMITTED");
    public static readonly ChangeOrderStatus Approved  = new("APPROVED");
    public static readonly ChangeOrderStatus Rejected  = new("REJECTED");
    public static readonly ChangeOrderStatus Void      = new("VOID");

    public static Option<ChangeOrderStatus> TryGet(string key) =>
        TryGet(key, out ChangeOrderStatus? row) && row is { } hit ? Some(hit) : None;

    public static ChangeOrderStatus Of(string status) => TryGet(status.Trim().ToUpperInvariant()).IfNone(Proposed);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Contingency(Money Reserve) {
    public static readonly Contingency None = new(Money.AdditiveIdentity);
    public Fin<Contingency> Drawdown(Money draw, Op key) =>
        draw.Currency != Reserve.Currency && Reserve.Currency != Currency.NoCurrency && draw.Currency != Currency.NoCurrency
            ? Fin.Fail<Contingency>(new BimFault.Refused(key, BimScope.Planning, BimReason.Codec, string.Join(':', new object?[] { "cost-currency", "contingency-draw", draw.Currency.Code, Reserve.Currency.Code })))
            : Reserve - draw is var net && !Money.IsNegative(net)
                ? Fin.Succ(new Contingency(net))
                : Fin.Fail<Contingency>(new BimFault.Refused(key, BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "cost-contingency-overdraw", draw.Amount.ToString(CultureInfo.InvariantCulture), Reserve.Amount.ToString(CultureInfo.InvariantCulture) })));
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

public readonly record struct EarnedValueReport(
    Money Bac, Money Bcws, Money Bcwp, Option<Money> Acwp,
    Option<decimal> Cpi, Option<decimal> Spi, Option<Money> Eac, Option<Money> Vac) {
    public Option<Money> Cv => Acwp.Map(spent => Bcwp - spent);
    public Money Sv => Bcwp - Bcws;
    public Option<Money> Etc => Eac.Bind(eac => Acwp.Map(spent => eac - spent));
    public Option<decimal> Tcpi => Acwp.Bind(spent => Ratio(Bac - Bcwp, Bac - spent));
    public Option<bool> OverBudget => Cv.Map(static cv => Money.IsNegative(cv));
    public bool BehindSchedule => Money.IsNegative(Sv);

    static Option<decimal> Ratio(Money numerator, Money denominator) =>
        denominator.Amount == 0m ? None : Some(numerator / denominator);

    public static EarnedValueReport Of(Money bac, Money bcws, Money bcwp, Option<Money> acwp) {
        Option<decimal> cpi = acwp.Bind(spent => Ratio(bcwp, spent));
        Option<Money> eac = cpi.Filter(static index => index != 0m).Map(index => bac / index);
        return new(bac, bcws, bcwp, acwp, cpi, Ratio(bcwp, bcws), eac, eac.Map(forecast => bac - forecast));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CostPerformance {
    public static Fin<EarnedValueReport> EarnedValue(this CostSchedule schedule, ScheduleNetwork network, Instant statusDate, Op key, Map<string, Money> actuals = default, Map<string, double> observed = default, Seq<ExchangeRate> fx = default) {
        var taskByElement = network.Assignments
            .Bind(a => a.ElementGlobalIds.Map(id => (Element: id, a.TaskGlobalId)))
            .Fold(Map<string, string>(), static (m, row) => m.AddOrUpdate(row.Element, row.TaskGlobalId));
        var taskById = network.Tasks.Fold(Map<string, ConstructionTask>(), static (m, t) => m.AddOrUpdate(t.GlobalId, t));
        var resourceById = schedule.Resources.Fold(Map<string, ConstructionResource>(), static (m, r) => m.AddOrUpdate(r.GlobalId, r));
        return schedule.Items
            .TraverseM(item => Line(item, taskByElement, taskById, resourceById, actuals, observed, statusDate, schedule.Currency, fx, key))
            .As()
            .Map(lines => lines.Fold(
                (Bac: Money.AdditiveIdentity, Bcws: Money.AdditiveIdentity, Bcwp: Money.AdditiveIdentity, Acwp: Option<Money>.None),
                static (acc, l) => (acc.Bac + l.Budget, acc.Bcws + l.Bcws, acc.Bcwp + l.Bcwp, Accrue(acc.Acwp, l.Acwp))))
            .Map(static t => EarnedValueReport.Of(t.Bac, t.Bcws, t.Bcwp, t.Acwp));
    }

    static Option<Money> Accrue(Option<Money> carried, Option<Money> line) =>
        carried.Match(
            Some: held => Some(held + line.IfNone(Money.AdditiveIdentity)),
            None: () => line);

    static Fin<(Money Budget, Money Bcws, Money Bcwp, Option<Money> Acwp)> Line(
        CostItem item, Map<string, string> taskByElement, Map<string, ConstructionTask> taskById,
        Map<string, ConstructionResource> resourceById, Map<string, Money> actuals, Map<string, double> observed,
        Instant statusDate, Currency report, Seq<ExchangeRate> fx, Op key) =>
        from budget in CostMoney.Reprice(item.ValueOf(), report, fx, key)
        from recorded in actuals.Find(item.GlobalId).Match(
            Some: money => CostMoney.Reprice(money, report, fx, key).Map(Some),
            None: () => Fin.Succ(Option<Money>.None))
        from spent in item.ResourceGlobalId.Bind(resourceById.Find).Filter(static r => r.Completion.IsSome).Match(
            Some: resource => CostMoney.Reprice(resource.Spent, report, fx, key).Map(Some),
            None: () => Fin.Succ(Option<Money>.None))
        select item.PricedGlobalIds.Choose(taskByElement.Find).Head
            .Bind(taskById.Find)
            .Match(
                Some: task => {
                    decimal planned = (decimal)Fraction(task.Scheduled, statusDate);
                    decimal actual = (decimal)(observed.Find(task.GlobalId)
                        | (task.Status == TaskStatus.Completed ? Some(1d) : task.PercentComplete)
                        | task.Actual.Map(a => Fraction(a, statusDate))).IfNone(0d);
                    return (Budget: budget, Bcws: budget * planned, Bcwp: budget * actual, Acwp: recorded | spent);
                },
                None: () => (Budget: budget, Bcws: Money.AdditiveIdentity, Bcwp: Money.AdditiveIdentity, Acwp: recorded | spent));

    public static double Fraction(Interval interval, Instant statusDate) =>
        interval.Duration.TotalDays <= 0d ? (statusDate >= interval.End ? 1d : 0d)
        : Math.Clamp((statusDate - interval.Start).TotalDays / interval.Duration.TotalDays, 0d, 1d);
}
```

## [04]-[CARBON]

- Owner: `CarbonEstimate` the 6D embodied-carbon rollup — the carbon peer of the 5D estimate on the SAME rollup shape: the `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation.Decompose` material-true takeoff joined to the Materials-authored seam `MaterialPropertySet.Environmental` per-`LifecycleStage` GwpTotal vector, folded per element → per stage → model total with EN 15978 stage discipline and EPD provenance; `CarbonLine` the per-(element, material) evidence row, `CarbonGap` the typed absence row (no environmental set, missing basis evidence, unresolved geometry — counted, never silently zero), `CarbonRollup` the stage-banded rollup.
- Entry: `CarbonEstimate.Rollup(ElementGraph graph, ElementQuery scope, Func<Node.Object, CapabilitySet<MassKind>, Option<MeasureBundle>> measures, Op key)` folds the selection — the scope IS the query algebra, so per-zone carbon is `Rollup` over the `Model/zones#ZONE_GRAPH` member set and per-model carbon the whole-graph selection — through each element's `Bake` reads (`element.Materials`/`element.Section`) into `Decompose`, the resolver taking the `QuantityDerivation.Demand(element.Materials)` composition-implied mass domains so a profile-set ply's swept LENGTH and a layer-set ply's VOLUME arrive on ONE bundle, each per-material volume share priced against its OWN material's `Environmental` case through a basis-resolved seam `MeasureValue` — never a bare double: a `PerM3` basis reads the share itself, a `PerKg` basis mints the MASS through the SAME material's declared `Mechanical` density (`share.Multiply(density)` re-stamped `QuantityType.Mass`, one material and one scalar product — the multi-ply element WEIGHT aggregate stays `Rasm.Compute`'s frozen boundary), a `PerM2` basis mints the AREA by dividing the share back through the summed ply thickness the `Decompose` `LayerSet` split it by (`share.Divide(thickness)` re-stamped `QuantityType.Area`), and a `PerItem` basis, a non-layered composition under `PerM2`, or a material with no environmental set lands a counted `CarbonGap`; independent share faults accumulate through `Validation<Error, T>` and lower once to `Error.Many`.
- Auto: each line prices the six-stage vector in one pass — `kgCO2e(stage) = Environmental.StageAt(stage) × basis-resolved quantity` — so A1-A3 through D band every line, `ByStage` folds the stage columns across lines, `Total` is the whole-life sum, and the line keeps its `PropertyEvidence` EPD provenance (registration + validity riding the case's base `Evidence`) so a statutory report cites its declarations; declared-property aggregation exactly like the 5D `Rollup` and the systems `Demand` — never a simulation, and the assembly-ply carbon scaling stays the `Rasm.Compute` aggregator's altitude.
- Output: the `CarbonRollup` is the 6D deliverable evidence — per-element lines for the hotspot view, the A1-D stage bands for the EN 15978 report, the model total for the statutory threshold, and the gap rows for the data-coverage verdict a reviewer reads FIRST — consumed by the `Rasm.AppUi/Charts` carbon dashboard beside the cost schedule it mirrors.
- Packages: Rasm.Element (the seam `Environmental`/`LifecycleStage`/`MeasurementBasis`), Rasm (the kernel `Analysis/measure` `MeasureBundle` carrier and the `Domain/validation` `CapabilitySet` demand column), Rasm.Bim (the `Semantics/properties#BASE_QUANTITIES` `Decompose` takeoff beside its `Demand` ceiling), LanguageExt.Core.
- Growth: a new impact indicator is the seam matrix's — a per-indicator rollup re-keys `IndicatorAt(category, stage)` through one parameter, never a sibling estimator; a new basis modality is one `Priced` arm; a carbon budget/target comparison is one column on `CarbonRollup`; never a second carbon owner and never a per-discipline carbon type.
- Boundary: the fold prices DECLARED data — the takeoff share from `Decompose`, the factor from the material's own `Environmental` case, the density from the SAME material's `Mechanical` case (a cross-material or element-level mass aggregate is the `Rasm.Compute` `AssemblyAggregator`'s frozen boundary, so a `PerKg` material with no declared density is a GAP row, never a fabricated mass), and the thickness from the SAME composition's own `LayerSet` plies (a `PerItem` declaration prices a COUNT no volumetric takeoff carries and stays the honest gap, never a fabricated unit count); every basis-resolved quantity is a dimensioned seam `MeasureValue` minted through the seam algebra and a bare-`double` mass or area is the deleted form; geometry enters as the injected kernel `MeasureBundle` resolver the owning fold's `Demand` ceiling parameterizes — the same port shape `Derive` takes, never an in-owner geometry evaluation, and never a hand-listed kind roster beside `QuantityDerivation.Demand`; a gap is COUNTED evidence (the coverage question a carbon reviewer asks first) and dropping an un-assessable line silently is the deleted form; the per-stage banding is the seam `LifecycleStage` vocabulary and a Bim-local stage enum is the deleted parallel roster.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Analysis;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Planning;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CarbonLine(
    NodeId Element, MaterialId Material, MeasureValue Share,
    Map<LifecycleStage, double> KgCo2eByStage, PropertyEvidence Evidence) {
    public double WholeLife => KgCo2eByStage.Values.Fold(0.0, static (total, kg) => total + kg);
}

public sealed record CarbonGap(NodeId Element, Option<MaterialId> Material, string Cause);

public sealed record CarbonRollup(Seq<CarbonLine> Lines, Map<LifecycleStage, double> ByStage, double TotalKgCo2e, Seq<CarbonGap> Gaps) {
    public static CarbonRollup Of(Seq<CarbonLine> lines, Seq<CarbonGap> gaps) {
        Map<LifecycleStage, double> byStage = lines.Fold(Map<LifecycleStage, double>(), static (acc, line) =>
            line.KgCo2eByStage.AsIterable().Fold(acc, static (m, row) => m.AddOrUpdate(row.Key, held => held + row.Value, () => row.Value)));
        return new CarbonRollup(lines, byStage, byStage.Values.Fold(0.0, static (total, kg) => total + kg), gaps);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CarbonEstimate {
    public static Fin<CarbonRollup> Rollup(ElementGraph graph, ElementQuery scope, Func<Node.Object, CapabilitySet<MassKind>, Option<MeasureBundle>> measures, Op key) =>
        scope.Objects
            .TraverseM(obj => graph.Bake(obj.Id, key).Bind(element =>
                measures(obj, QuantityDerivation.Demand(element.Materials)).Match(
                    None: () => Fin.Succ((Lines: Seq<CarbonLine>(), Gaps: Seq(new CarbonGap(obj.Id, None, "geometry-unresolved")))),
                    Some: bundle => QuantityDerivation.Decompose(bundle, element.Materials, element.Section, key)
                        .Bind(shares => shares.AsIterable()
                            .Traverse(share => Priced(obj.Id, element.Materials, share.Key, share.Value, key).ToValidation()).As().ToFin()
                            .Map(priced => priced.Fold(
                                (Lines: Seq<CarbonLine>(), Gaps: Seq<CarbonGap>()),
                                static (acc, outcome) => outcome.Match(
                                    Left:  gap => acc with { Gaps = acc.Gaps.Add(gap) },
                                    Right: line => acc with { Lines = acc.Lines.Add(line) })))))))
            .As()
            .Map(static rows => CarbonRollup.Of(rows.Bind(static r => r.Lines), rows.Bind(static r => r.Gaps)));

    static Fin<Either<CarbonGap, CarbonLine>> Priced(NodeId element, Seq<BakedMaterial> materials, MaterialId material, MeasureValue share, Op key) =>
        materials.Find(baked => baked.Material.MaterialKey == material).Match(
            None: () => Fin.Succ(Left<CarbonGap, CarbonLine>(new CarbonGap(element, Some(material), "material-unresolved"))),
            Some: baked => baked.Material.Properties
                .Choose(static p => p is MaterialPropertySet.Environmental e ? Some(e) : Option<MaterialPropertySet.Environmental>.None)
                .Head
                .Match(
                    None: () => Fin.Succ(Left<CarbonGap, CarbonLine>(new CarbonGap(element, Some(material), "environmental-unset"))),
                    Some: environmental => Quantity(environmental.Basis, material, share, baked, key).Map(quantity => quantity.Match<Either<CarbonGap, CarbonLine>>(
                        None: () => new CarbonGap(element, Some(material), $"basis-unresolvable:{environmental.Basis.Key}"),
                        Some: admitted => new CarbonLine(
                            element, material, share,
                            toSeq(LifecycleStage.Items).Fold(Map<LifecycleStage, double>(), (band, stage) =>
                                band.Add(stage, environmental.StageAt(stage) * admitted.Si)),
                            environmental.Evidence)))));

    static Fin<Option<MeasureValue>> Quantity(MeasurementBasis basis, MaterialId material, MeasureValue share, BakedMaterial baked, Op key) =>
        basis.Switch(
            state: (Material: material, Share: share, Baked: baked, Key: key),
            perM3:   static (s, _) => Fin.Succ(Some(s.Share)),
            perKg:   static (s, _) => s.Baked.Material.Properties
                .Choose(static p => p is MaterialPropertySet.Mechanical m ? Some(m.Density) : Option<MeasureValue>.None)
                .Head
                .Match(
                    Some: density => s.Share.Multiply(density, s.Key).Bind(mass => mass.WithType(QuantityType.Mass, s.Key)).Map(Some),
                    None: static () => Fin.Succ(Option<MeasureValue>.None)),
            perM2:   static (s, _) => Thickness(s.Baked, s.Material, s.Key)
                .Bind(thickness => thickness.Match(
                    Some: admitted => s.Share.Divide(admitted, s.Key).Bind(area => area.WithType(QuantityType.Area, s.Key)).Map(Some),
                    None: static () => Fin.Succ(Option<MeasureValue>.None))),
            perItem: static (_, _) => Fin.Succ(Option<MeasureValue>.None));

    static Fin<Option<MeasureValue>> Thickness(BakedMaterial baked, MaterialId material, Op key) =>
        baked.Material.Composition.Switch(
            state: material,
            single:         static (_, _) => Seq<MeasureValue>(),
            profileSet:     static (_, _) => Seq<MeasureValue>(),
            constituentSet: static (_, _) => Seq<MeasureValue>(),
            layerSet: static (id, set) => set.Layers.Filter(layer => layer.Material == id).Map(static layer => layer.Thickness))
        is { IsEmpty: false } plies ? MeasureValue.Sum(plies, key).Map(Some) : Fin.Succ(Option<MeasureValue>.None);
}
```

## [05]-[RESEARCH]

(none)
