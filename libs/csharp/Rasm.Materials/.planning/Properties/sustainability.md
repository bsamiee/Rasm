# [MATERIALS_SUSTAINABILITY]

THE SUSTAINABILITY-AND-UNIT-COST SOURCE. The lifecycle-and-unit-cost property family is SEAM-owned: the `Rasm.Element` `MaterialPropertySet` `[Union]` carries `Environmental` (a `MeasurementBasis` declared unit + the FULL EN 15804+A2 `(ImpactCategory × LifecycleStage)` impact matrix stored row-major flat — the cradle-to-gate GWP a DERIVED read of the `(GwpTotal, A1A3)` cell, never a double-stored headline scalar — the recycled/end-of-life resource fractions, EPD provenance riding the case `Evidence` as `PropertyEvidence.Declaration("epd", id, validUntil)`, with the intrinsic `IndicatorAt`/`WholeLife`/`Gwp`/`StageAt`/`WholeLifeGwp`/`StageGwp` projection folds) and `Cost` (the supply/install/lifecycle per-unit columns over the seam `Currency` band and `MeasurementBasis`) — classification is NOT a `MaterialPropertySet` case but the seam's generic `Classification` `[ComplexValueObject]` the `Projection/component#COMPONENT_SUBGRAPH` `Capture` threads onto the bound element's `Object` node (an Object-node VALUE per `Rasm.Element/Relations/relation`, never a `Material`-node field nor an edge) and `Rasm.Bim` re-emits onto `IfcRelAssociatesClassification` — so the prior Materials-owned `Environmental`/`Cost` `MaterialProperty` partial cases and the lifecycle aggregation folds are RETIRED, ROUTED into the seam. This owner is the second Materials SOURCE: it COMPOSES the shared `Published<T>` ingress carrier `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` DECLARES (the double-family `IUncertainty<double>` arm carrying the raw kgCO2e/fraction/cost magnitudes with their `PropertyEvidence` — the prior `ScalarDatum`/`SustainabilityUncertainty`/`CostDatum` scaffolds collapsed onto it), and one `SustainabilityCatalogue`, the registered-row database of cradle-to-grave EPD / cost-basis / classification data per `MaterialId` in EXACT roster parity with the engineering catalogue. The `Lower` lowering coerces a row into the seam cases — the carbon-only per-module vector embedding into the FULL impact matrix through the seam `MaterialPropertySet.Environmental.CarbonMatrix` builder, then through the seam `OfEnvironmental` band gate (`MatrixArity` + `AllFinite`, the fractions `[0,1]`, the basis the EPD's own declared `MeasurementBasis`) — never a `MeasureValue`/`QuantityRow` mint, CO2e and currency being DOMAIN BASES not SI dimensions — and the unit-cost columns through `OfCost` at their declared basis. The per-material set so projected IS the `Environmental`/`Cost` analysis input `Rasm.Compute` reads off the `Material` node directly. A material is a FULL LIFECYCLE OBJECT: embodied carbon and cost basis ride as `MaterialPropertySet` cases over one `MaterialId`, the BIM classification as the `Classification` egress — never an `EcoMaterial`/`CostMaterial`/`ClassifiedMaterial` surface; the whole-building takeoff and cost rollup (`AggregateEnvironmental`/`AggregateCost`) are RELOCATED to `Rasm.Compute`. The page composes the seam (`MaterialPropertySet.OfEnvironmental`/`OfCost`, `Environmental.CarbonMatrix`/`MatrixArity`, `LifecycleStage`/`ImpactCategory`/`Currency`/`MeasurementBasis`, the generic `Classification` + `Classification.Of`) and rails the seam `ElementFault.ValueRejected` (band 2500) on every lowering fault; it re-mints NO seam type, mints NO `MaterialFault`, and admits NO `UnitsNet` quantity.

## [01]-[INDEX]

- [02]-[SUSTAINABILITY_PROPERTY]: the `SustainabilityRow` published-data ingress shape composing the shared `Published<double>` carrier, the `SustainabilityCatalogue` registered-row database (the FULL EN/ASTM/CSA structural-and-envelope EPD roster), the `Lower` lowering into the seam `Environmental`/`Cost` cases, the `Classification` egress lifting the row's `(system, code)` to a seam `Classification` value-object, and the `Lookup` the projector composes with the engineering catalogue.
- [03]-[RESEARCH]: the seam lifecycle contract, module/basis law, relocation, and roster-parity registers.

## [02]-[SUSTAINABILITY_PROPERTY]

- Owner: `SustainabilityRow` the published-data ingress record over the shared `Published<double>` carrier (declared by `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`, composed here); `CostDatum` the currency/basis cost group; `SustainabilityCatalogue` the registered-row database; `Lower` the row→seam-case lowering; `Classification` the Object-node egress; the EN 15978 / EN 15804+A2 / ISO 14040 lifecycle-and-unit-cost SOURCE.
- Cases: one `SustainabilityRow` shape — the environmental (the per-EN-15978-module `StageGwp` carbon vector as `Published<double>` entries sharing the EPD `Declaration` evidence, + recycled/end-of-life fractions), optional cost (supply/install/lifecycle over a currency + measurement basis), and optional classification (system + code) published columns; `Lower` produces a `Seq<MaterialPropertySet>` of the seam `Environmental`/`Cost` cases, each over a `MaterialId`, never a property subtype; the row's classification `(system, code)` is NOT lowered to a property case — it leaves through the `Classification` egress `Rasm.Bim` associates via `IfcRelAssociatesClassification`. The roster keys the SAME `MaterialId` set the engineering catalogue rosters (FULL_ROSTER parity), grown by row to any registered grade.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key)` — GUARDS the `StageGwp` vector to the seam `LifecycleStage.Count` arity FIRST (a wrong-length vector rails `ElementFault.ValueRejected` at the lowering edge rather than the seam `CarbonMatrix` silently short-writing it — admission-once, fail-closed), parses the row's native `EnvironmentalBasis` through `MeasurementBasis.Parse`, EMBEDS the carbon-only central vector into the FULL `(ImpactCategory × LifecycleStage)` matrix through the seam `Environmental.CarbonMatrix` builder (the `GwpTotal` indicator row at its offset, every other indicator row zeroed — the partial-EPD invariant), passes it through `MaterialPropertySet.OfEnvironmental(basis, matrix, recycled, recovery, key, row.Evidence)` (the seam re-guards `MatrixArity`/`AllFinite`/`[0,1]` internally; EPD identity + `LocalDate` expiry ride the evidence, never per-case columns), and the optional cost through `Currency.Parse` + `MeasurementBasis.Parse` (independent tokens, accumulated applicatively) + `OfCost` (each column finite and non-negative at the seam) — the environmental and cost groups are INDEPENDENT and ACCUMULATE (`ToValidation` slots, tuple `Apply`, one `ToFin`), so a bad `declared_unit` and a bad currency fault together; the `MaterialId`-keyed `Lookup(id, key)` resolves the row then lowers, returning `Fin.Succ(empty)` for an unregistered id (lifecycle data is declared-or-absent — the asymmetric dual of the REQUIRED engineering `Lookup`); `Classification(id, key)` resolves the row's `(system, code)` through the edition-unspecified `Classification.Of`, `None` when row or material absent, and rides the `Projection/component#COMPONENT_SUBGRAPH` `MaterialBinding` to the bound element's Object node.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfEnvironmental`/`OfCost`, the seam-owned `Environmental.CarbonMatrix` carbon-row → matrix builder + `MatrixArity`, `LifecycleStage`/`ImpactCategory` the EN 15804+A2 matrix bands, `Currency`/`MeasurementBasis`, `PropertyEvidence.Declaration`, the generic `Classification` + `Classification.Of`, `ElementFault.ValueRejected`, `MaterialId`), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of`/`Vector`/`Centrals`, SAME namespace so no import), Rasm (project — `Op`), NodaTime (`LocalDate` the EPD validity expiry), LanguageExt.Core (`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`, `ImmutableArray<T>`, the `double[]` ingress vector). NO `UnitsNet` (CO2e and currency are domain bases, not SI dimensions — nothing coerces through `MeasureValue`), NO `QuantityRow` (the typed-mint mandate is the engineering catalogue's; a `StageGwp`/cost magnitude is basis-relative, not a dimensioned quantity), NO `MaterialFault` (every fault is the seam `ElementFault`).
- Growth: a new EN 15804+A2 indicator is one seam `ImpactCategory` row (the matrix widens by one indicator row; a carbon-only catalogue row zeroes it through `CarbonMatrix`); a FULL-matrix EPD (all thirteen indicators published) is one optional matrix column beside `StageGwp` that `Lower` passes straight to `OfEnvironmental`, `CarbonMatrix` bypassed; a new EN 15978 module is one seam `LifecycleStage` row; a new currency is one opaque ISO 4217 token the row supplies (the seam `Currency` is shape-validated alpha-3, roster-owned by the `Rasm.Bim` NodaMoney algebra); a new classification system is one opaque `System` token; a new declared basis is one seam `MeasurementBasis` row; a new known material is one `Rows` entry — never a per-discipline material type, never a parallel Eco/Cost surface. The catalogue grows by row, the lifecycle vocabulary by seam band.
- Boundary: `SustainabilityRow` is the published-DATA ingress, NOT a parallel domain union — the seam `Environmental`/`Cost` are the one typed carriers, `Lower` the `BOUNDARY_ADMISSION`; each `StageGwp` module is a raw kgCO2e-per-basis-unit magnitude declared at the row's OWN `EnvironmentalBasis` (the EPD's native `declared_unit` — a per-kg steel EPD stays `PerKg`, a per-m² membrane `PerM2`, never force-normalized to a curated `PerM3`), and the `Rasm.Compute` `AggregateEnvironmental` scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold and the EC3 ingress use; a negative module is VALID biogenic-sequestration or avoided-burden carbon (the timber A1-A3 credit, the metal D credit), never rejected — the seam guards FINITE alone on the matrix cells; the fractions pass raw and the seam guards `[0,1]` once (re-minting a `UnitInterval` here would diverge from the one admission owner); the cost columns ride the seam `Currency` (shape-validated ISO 4217 alpha-3) and their basis; the seam `Environmental` case is the FULL impact MATRIX, the cradle-to-gate `Gwp` a DERIVED `(GwpTotal, A1A3)` read and the cradle-to-grave total the seam `WholeLifeGwp` fold (the seam owns the intrinsic folds exactly as it owns `StcWeighted`); EPD identity/expiry ride `PropertyEvidence.Declaration` — the prior row-level `Epd`/`ValidUntilYear` columns are DELETED as a double-store of the evidence concept; the lowered cases land on the seam `Material` node the projector authors and `Rasm.Bim` reads `Pset_EnvironmentalImpactValues`/`Pset_ConstructionCosts`/`IfcClassificationReference` off the seam graph — no Materials wire carrier; the multi-ply rollups are `Rasm.Compute`'s.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation slot the applicative environmental×cost join accumulates
using NodaTime;                      // LocalDate — the EPD validity expiry PropertyEvidence.Declaration carries
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, MaterialPropertySet (Environmental|Cost), Environmental.CarbonMatrix,
                                     // LifecycleStage, ImpactCategory, Currency, MeasurementBasis, PropertyEvidence,
                                     // Classification (the seam (system, code) value-object the classification column resolves to),
                                     // ElementFault (the seam value-admission band 2500 every lowering fault lifts on)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // beside MaterialPropertyCatalogue — the shared Published<T> carrier is namespace-local

// --- [MODELS] ------------------------------------------------------------------------------
// The published lifecycle/unit-cost data for one material — the ingress row Lower lowers into the seam
// Environmental/Cost cases, its (system, code) the Classification egress lifts. A flat DATA record over the
// SHARED Published<double> carrier properties.md declares (the ScalarDatum/SustainabilityUncertainty/CostDatum
// scaffolds are the deleted duplicates). NO headline GWP scalar: the cradle-to-gate A1-A3 value IS the
// StageGwp[A1A3] entry (the seam derives Gwp over the matrix — DERIVED_LOGIC). NO Epd/ValidUntilYear columns:
// the Declaration evidence carries identity + expiry ONCE (the prior columns double-stored it). BASIS-NATIVE:
// the StageGwp magnitudes are kgCO2e PER the row's own declared_unit token exactly as the EPD publishes them.
public sealed record CostDatum(
    string Currency,
    string Basis,
    Published<double> Supply,
    Published<double> Install,
    Published<double> Lifecycle);

public sealed record SustainabilityRow(
    string EnvironmentalBasis,       // the EPD's native declared_unit token parsed to the seam MeasurementBasis
    ImmutableArray<Published<double>> StageGwp,
    Published<double> Recycled,
    Published<double> Recovery,
    Option<CostDatum> Cost,
    Option<(string System, string Code)> Classification,
    PropertyEvidence Evidence) {

    // The EPD confidence profile (POLICY_VALUES — the deleted SustainabilityUncertainty struct's rows as
    // dependency-free anchors): module GWP ±15%, resource fractions ±10%, unit costs ±20/25/25%.
    const double GwpConfidence = 0.15;
    const double FractionConfidence = 0.10;
    const double SupplyConfidence = 0.20;
    const double InstallConfidence = 0.25;
    const double LifecycleConfidence = 0.25;

    public SustainabilityRow(
        string environmentalBasis,
        double[] stageGwp,
        string epd,
        int validUntilYear,
        double recycledContent,
        double endOfLifeRecovery,
        Option<(string Currency, string Basis, double Supply, double Install, double Lifecycle)> cost,
        Option<(string System, string Code)> classification)
        : this(
            environmentalBasis,
            Published.Vector(stageGwp, GwpConfidence, Declared(epd, validUntilYear)),
            Published.Of(recycledContent, FractionConfidence, Declared(epd, validUntilYear)),
            Published.Of(endOfLifeRecovery, FractionConfidence, Declared(epd, validUntilYear)),
            cost.Map(c => new CostDatum(
                c.Currency,
                c.Basis,
                Published.Of(c.Supply, SupplyConfidence, PropertyEvidence.Catalogue),
                Published.Of(c.Install, InstallConfidence, PropertyEvidence.Catalogue),
                Published.Of(c.Lifecycle, LifecycleConfidence, PropertyEvidence.Catalogue))),
            classification,
            Declared(epd, validUntilYear)) { }

    static PropertyEvidence Declared(string epd, int validUntilYear) =>
        PropertyEvidence.Declaration("epd", epd, new LocalDate(validUntilYear, 12, 31));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SustainabilityCatalogue {
    // The empty cost slot a cost-less row contributes to the applicative join.
    static readonly Validation<Error, Seq<MaterialPropertySet>> NoCost = Success<Error, Seq<MaterialPropertySet>>(Seq<MaterialPropertySet>());

    // Lowers a published row into the seam Environmental/Cost cases: the arity guard FIRST (a wrong-length
    // vector rails HERE, never silently short-written by the seam CarbonMatrix Math.Min — admission-once is
    // fail-closed); the ENVIRONMENTAL group binds its one dependency chain — the native basis token through
    // MeasurementBasis.Parse (a malformed declared_unit is a fault, not a silent default), the carbon-only
    // central vector embedded into the FULL (ImpactCategory × LifecycleStage) matrix through the seam-owned
    // CarbonMatrix builder (GwpTotal row at its offset, every other EN 15804+A2 indicator row ZERO — the
    // partial-EPD invariant), lowered through the OfEnvironmental band gate AT THAT BASIS (the seam re-guards
    // MatrixArity/AllFinite and the fractions [0,1]; the A1A3 cell IS the cradle-to-gate Gwp the seam derives —
    // no headline scalar passed; the Declaration evidence carries EPD identity + expiry); the COST group parses
    // its independent currency/basis tokens applicatively then binds OfCost. The two groups are INDEPENDENT and
    // ACCUMULATE — a bad declared_unit AND a bad currency fault together in one Fin.Fail (ManyErrors), never
    // first-fault-only. Classification is NOT lowered (not a Discipline-keyed physics): the row's pair leaves
    // through the Classification egress below.
    public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key) =>
        row.StageGwp.Length != LifecycleStage.Count
            ? ElementFault.ValueRejected(key, $"<stage-gwp-arity:{row.StageGwp.Length}:expected={LifecycleStage.Count}>")
            : (MeasurementBasis.Parse(row.EnvironmentalBasis, key)
                   .Bind(basis => MaterialPropertySet.OfEnvironmental(
                       basis,
                       MaterialPropertySet.Environmental.CarbonMatrix(Published.Centrals(row.StageGwp)),
                       row.Recycled.Central, row.Recovery.Central, key, row.Evidence))
                   .ToValidation(),
               row.Cost.Match(
                   None: static () => NoCost,
                   Some: c => (Currency.Parse(c.Currency, key).ToValidation(),
                               MeasurementBasis.Parse(c.Basis, key).ToValidation())
                       .Apply(static (currency, costBasis) => (currency, costBasis)).As()
                       .Bind(x => MaterialPropertySet.OfCost(x.currency, x.costBasis, c.Supply.Central, c.Install.Central, c.Lifecycle.Central, key, c.Supply.Evidence)
                           .Map(static priced => Seq(priced))
                           .ToValidation())))
              .Apply(static (environmental, cost) => Seq(environmental) + cost).As()
              .ToFin();

    // The curated reference catalogue covers the SAME structural-and-envelope domain the engineering
    // Properties/properties#MATERIAL_PROPERTY_CATALOGUE rosters, keyed by the SAME canonical MaterialId in
    // EXACT parity (FULL_ROSTER — a registered grade resolves a mechanical/thermal/fire row AND this lifecycle
    // row), each row storing the EPD AS PUBLISHED at its native declared_unit — NO curation normalization to a
    // single PerM3 basis (a "× density at curation" rewrite drops a per-area or per-item EPD): the per-kg
    // metals/glass/gypsum/insulation stay PerKg, the per-m3 cast/cut/fired materials PerM3, the membranes
    // PerM2. The Compute AggregateEnvironmental scales each ply by the basis-matching element quantity (PerKg
    // -> volume×density, PerM3 -> volume, PerM2 -> face area), so a per-kg and a per-m3 declaration fold
    // through the one basis-aware DeclaredQuantity path. The StageGwp vector is the EN 15978 module GWP-total
    // [A1A3, A4, A5, B, C, D] in LifecycleStage order; the cradle-to-gate GWP is the [A1A3] cell the seam Gwp
    // accessor derives — NO headline scalar column. The figures are published EN 15804+A2 EPD / ICE v3 / EC3
    // industry-average magnitudes per the declared unit: a metal's negative D is the recycling avoided-burden
    // credit; a timber's negative A1-A3 is biogenic sequestration and its positive C the end-of-life combustion
    // release; a calcium-silicate/AAC unit's negative B1 is the in-service carbonation re-uptake.
    // GWP TRACKS MASS NOT GRADE: every carbon-steel grade S235..S690 (+ s450, the metal.steel alias) shares the
    // WorldSteel section eco-profile row, the AISC a36/a992/a572 grades the North-American EAF hot-rolled
    // sections profile, every EN/ASTM/CSA rebar grade its industry-average rebar EPD, every stainless grade its
    // EN 10088 family EPD, every concrete strength class its cement-content-scaled ICE/EC3 value (cmu the
    // concrete-block EPD), every EN 338 sawn grade the shared sawn-timber EPD, every EN 14080 glulam grade the
    // shared glulam EPD, and the glass pane substances (crown the float profile, flint the borosilicate EPD) —
    // the strength class drives the mechanical row (properties.md), the mass-per-declared-unit this carbon row.
    // The roster grows by ROW; a material with no declared EPD omits a row and Lookup returns the empty
    // lifecycle set, never a fault. Cost columns are USD unit-cost estimates at the matching basis; Uniclass
    // 2015 Pr_ codes are the BIM classification the Classification egress lifts.
    public static readonly FrozenDictionary<MaterialId, SustainabilityRow> Rows = new (MaterialId Id, SustainabilityRow Row)[] {
        // --- carbon structural steel (EN 10025; per-kg; WorldSteel Europe sections eco-profile shared S235..S690 — GWP tracks mass not grade; EAF recycled, negative D avoided-burden)
        (MaterialId.Of("steel.s235"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s275"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.98, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s355"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 1.05, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s420"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 1.15, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // steel.s450 — the EN 10025-2 S450 grade (properties.md parity; the Component/steel SteelGrade.S450 substance): the shared WorldSteel section profile
        (MaterialId.Of("steel.s450"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 1.20, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s460"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 1.25, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s690"),   new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 1.55, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // generic structural-steel alias (properties.md metal.steel — the unspecified-grade S235 baseline): shares the WorldSteel section eco-profile
        (MaterialId.Of("metal.steel"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // generic cast/ductile-iron (properties.md metal.iron — EN-GJS-400 casting baseline the joint/weld family keys): Furnes NEPD-9786-9710 ductile cast-iron EPD per-tonne -> per-kg, 100% remeltable
        (MaterialId.Of("metal.iron"),   new("per-kg", new[] { 0.213, 0.057, 0.058, 0.0, 0.054, -0.036 }, "Furnes-DuctileCastIron", 2030, 0.85, 0.95, Some(("USD", "per-kg", 1.20, 0.60, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- AISC structural steel (properties.md steel.a36/a992/a572 — the Component/steel SteelGrade substances; per-kg; North-American EAF hot-rolled sections industry average, ~93% recycled)
        (MaterialId.Of("steel.a36"),    new("per-kg", new[] { 1.22, 0.032, 0.020, 0.0, 0.018, -0.28 }, "AISC-HotRolled-NA", 2027, 0.93, 0.98, Some(("USD", "per-kg", 0.90, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a992"),   new("per-kg", new[] { 1.22, 0.032, 0.020, 0.0, 0.018, -0.28 }, "AISC-HotRolled-NA", 2027, 0.93, 0.98, Some(("USD", "per-kg", 1.00, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a572"),   new("per-kg", new[] { 1.22, 0.032, 0.020, 0.0, 0.018, -0.28 }, "AISC-HotRolled-NA", 2027, 0.93, 0.98, Some(("USD", "per-kg", 1.00, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- cold-formed sheet + fasteners (properties.md steel.g33/g50 + steel.fastener-* — the connector Gauge /
        //     fastener Grade SubstanceId rows; FULL_ROSTER parity). Carbon-steel products, GWP tracks mass not grade.
        //     R1 PENDING a cold-formed-coil / high-strength-fastener producer EPD: these carry the generic WorldSteel
        //     carbon-steel vector as the grade-agnostic placeholder (the metal.steel treatment), never a fabricated
        //     product-specific value — a real cold-formed/galvanized-sheet or fastener EPD supersedes on admission.
        (MaterialId.Of("steel.g33"),           new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.g50"),           new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-4_6"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-4_8"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-5_6"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-5_8"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-6_8"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-8_8"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-10_9"), new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-12_9"), new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr2"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr5"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr8"),  new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-a325"), new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-a490"), new("per-kg", new[] { 1.55, 0.013, 0.020, 0.0, 0.018, -0.30 }, "WorldSteel-Sections-EU", 2030, 0.73, 0.90, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- stainless steel (EN 10088; per-kg; Stalatube/Outokumpu/Aperam EPDs; austenitic 1.4301..1.4571, duplex 1.4462; ~100% effective recycling)
        (MaterialId.Of("steel.1.4301"), new("per-kg", new[] { 1.61, 0.108, 0.050, 0.0, 0.022, -0.183 }, "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4307"), new("per-kg", new[] { 1.61, 0.108, 0.050, 0.0, 0.022, -0.183 }, "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4401"), new("per-kg", new[] { 1.83, 0.077, 0.050, 0.0, 0.022, -0.181 }, "Stalatube-1.4404", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.80, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4404"), new("per-kg", new[] { 1.83, 0.077, 0.050, 0.0, 0.022, -0.181 }, "Stalatube-1.4404", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.80, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4571"), new("per-kg", new[] { 1.83, 0.080, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Austenitic", 2028, 0.75, 1.00, Some(("USD", "per-kg", 4.60, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4462"), new("per-kg", new[] { 3.18, 0.096, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Duplex-2205", 2028, 0.75, 1.00, Some(("USD", "per-kg", 4.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        // --- reinforcing steel (EN 10080; per-kg; ArcelorMittal/CARES EAF rebar shared across the six-grade EnRebarGrade set — ductility class drives properties.md not mass-GWP)
        (MaterialId.Of("steel.b450a"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b450c"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500a"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500b"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500c"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.82, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b550b"),  new("per-kg", new[] { 0.818, 0.032, 0.039, 0.0, 0.013, -0.30 }, "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90, Some(("USD", "per-kg", 0.82, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        // ASTM A615 (steel.gr40..gr80) + A706 weldable (gr60w/gr80w) + CSA G30.18 (400w/500w): North-American EAF rebar shares the CRSI/ASTM industry-average rebar family (GWP tracks mass not grade)
        (MaterialId.Of("steel.gr40"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.78, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr60"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr75"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.84, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr80"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.88, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr60w"),  new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.86, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr80w"),  new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.92, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.400w"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.84, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.500w"),   new("per-kg", new[] { 0.760, 0.032, 0.039, 0.0, 0.013, -0.30 }, "CRSI-Rebar-NA", 2028, 0.97, 0.90, Some(("USD", "per-kg", 0.90, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        // --- concrete (EN 1992/EN 206 strength classes; per-m3; ICE v3 / EC3 / EN 206 ready-mix EPD; A1-A3 scales with cement content, B1 carbonation re-uptake, ~25% GGBS baseline)
        (MaterialId.Of("concrete.c12_15"),  new("per-m3", new[] { 180.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 105.0, 90.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c16_20"),  new("per-m3", new[] { 195.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 110.0, 92.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c20_25"),  new("per-m3", new[] { 210.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 115.0, 94.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c25_30"),  new("per-m3", new[] { 235.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Interbeton-EN206-C25", 2028, 0.00, 0.90, Some(("USD", "per-m3", 120.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c30_37"),  new("per-m3", new[] { 236.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Interbeton-EN206-C30", 2028, 0.00, 0.90, Some(("USD", "per-m3", 130.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c35_45"),  new("per-m3", new[] { 260.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 142.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c40_50"),  new("per-m3", new[] { 285.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 155.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c45_55"),  new("per-m3", new[] { 310.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 168.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c50_60"),  new("per-m3", new[] { 335.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90, Some(("USD", "per-m3", 182.0, 98.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c55_67"),  new("per-m3", new[] { 360.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Arup-EC-Scheme-HS", 2028, 0.00, 0.90, Some(("USD", "per-m3", 198.0, 100.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c60_75"),  new("per-m3", new[] { 380.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Arup-EC-Scheme-HS", 2028, 0.00, 0.90, Some(("USD", "per-m3", 215.0, 100.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c70_85"),  new("per-m3", new[] { 410.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Arup-EC-Scheme-HS", 2028, 0.00, 0.90, Some(("USD", "per-m3", 235.0, 105.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c80_95"),  new("per-m3", new[] { 430.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Arup-EC-Scheme-HS", 2028, 0.00, 0.90, Some(("USD", "per-m3", 255.0, 105.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c90_105"), new("per-m3", new[] { 450.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "Arup-EC-Scheme-HS", 2028, 0.00, 0.90, Some(("USD", "per-m3", 280.0, 110.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.lc"),      new("per-m3", new[] { 220.0, 2.1, 12.7, -12.8, 13.1, -9.1 }, "ICE-LightweightConc", 2028, 0.00, 0.90, Some(("USD", "per-m3", 165.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        // concrete.cmu — the CMU block-concrete substance (properties.md parity; the Component/cmu SubstanceId): the concrete-block EPD family the masonry.aggregate unit shares
        (MaterialId.Of("concrete.cmu"),     new("per-m3", new[] { 94.8, 5.0, 2.0, 0.0, 19.7, -14.7 }, "Belgard-ConcreteBlock", 2029, 0.10, 0.95, Some(("USD", "per-m3", 150.0, 140.0, 10.0)), Some(("uniclass-2015", "Pr_20_93_52_01")))),
        // --- sawn structural timber (EN 338; per-m3; Holmen/Moelven/Stora Enso sawn-softwood EPD shared C14..C50, hardwood D18..D80; negative A1-A3 biogenic, positive C combustion)
        (MaterialId.Of("timber.c14"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 420.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c16"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 430.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c18"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 440.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c20"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 450.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c22"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 460.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c24"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Moelven-Holmen-C24", 2027, 0.00, 1.00, Some(("USD", "per-m3", 470.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c27"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 490.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c30"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 510.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c35"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 540.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c40"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 580.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c45"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 620.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c50"),   new("per-m3", new[] { -734.0, 12.4, 2.5, 0.0, 775.0, -191.0 }, "Sawn-Softwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 660.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.d18"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 700.0, 180.0, 35.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d24"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 760.0, 180.0, 35.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d27"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 800.0, 182.0, 36.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d30"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 850.0, 185.0, 38.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d35"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 940.0, 185.0, 38.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d40"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1050.0, 190.0, 40.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d45"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1120.0, 192.0, 41.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d50"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1200.0, 195.0, 42.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d55"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1300.0, 198.0, 43.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d60"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1400.0, 200.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d65"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1520.0, 205.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d70"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1650.0, 210.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d75"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1780.0, 215.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d80"),   new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1920.0, 220.0, 52.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("wood.oak"),     new("per-m3", new[] { -848.0, 13.0, 3.0, 0.0, 848.0, -210.0 }, "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 1200.0, 280.0, 60.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),  // named European oak hardwood (~EN 338 D30), the seam/sibling-referenced id
        // --- glued-laminated timber (EN 14080; per-m3; HASSLACHER EN 15804+A2 glulam EPD shared across the full GL20h..GL32h / GL20c..GL32c set — GWP tracks mass not strength class)
        (MaterialId.Of("timber.gl20h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 820.0, 235.0, 44.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl20c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 790.0, 235.0, 44.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl22h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 850.0, 238.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl22c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 815.0, 238.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl24h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 880.0, 240.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl24c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 840.0, 240.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl26h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 900.0, 245.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl26c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 860.0, 245.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl28h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 920.0, 250.0, 47.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl28c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 880.0, 250.0, 47.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl30h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 960.0, 255.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl30c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 920.0, 255.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl32h"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 1000.0, 260.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl32c"), new("per-m3", new[] { -608.0, 12.0, 2.5, 0.0, 754.0, -410.0 }, "HASSLACHER-Glulam", 2026, 0.00, 1.00, Some(("USD", "per-m3", 960.0, 260.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        // --- wrought aluminium (EN 1999/EN 573; per-kg; European profile/extrusion EPD; energy-intensive primary, ~95% effective recycling, large negative D; GWP tracks mass not alloy)
        (MaterialId.Of("aluminium.6082t6"), new("per-kg", new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6082", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.40, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.6061t6"), new("per-kg", new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6061", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.40, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.6063t5"), new("per-kg", new[] { 5.50, 0.050, 0.050, 0.0, 0.033, -1.70 }, "Pandolfo-Al-6063", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.20, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.5083"),   new("per-kg", new[] { 8.50, 0.050, 0.050, 0.0, 0.050, -4.50 }, "EU-Al-Plate-5083", 2027, 0.35, 0.95, Some(("USD", "per-kg", 4.10, 0.95, 0.22)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        // --- masonry units (EN 771; per-m3; Wienerberger/Xella EPD; fired clay kiln carbon, calcium-silicate/AAC carbonation B1 credit)
        (MaterialId.Of("masonry.clay"),  new("per-m3", new[] { 320.0, 25.6, 2.6, 0.0, 25.6, -16.4 }, "Wienerberger-ClayBrick", 2030, 0.00, 0.90, Some(("USD", "per-m3", 280.0, 220.0, 15.0)), Some(("uniclass-2015", "Pr_20_93_52_15")))),
        (MaterialId.Of("masonry.calciumsilicate"), new("per-m3", new[] { 221.0, 13.6, 6.05, -94.5, 49.16, -7.98 }, "Xella-Silka-CS", 2031, 0.00, 0.90, Some(("USD", "per-m3", 240.0, 200.0, 14.0)), Some(("uniclass-2015", "Pr_20_93_52_12")))),
        (MaterialId.Of("masonry.aac"),   new("per-m3", new[] { 160.0, 0.078, 1.0, -36.4, 9.31, -1.03 }, "Xella-Ytong-AAC", 2030, 0.00, 0.90, Some(("USD", "per-m3", 190.0, 160.0, 12.0)), Some(("uniclass-2015", "Pr_20_93_52_05")))),
        (MaterialId.Of("masonry.aggregate"), new("per-m3", new[] { 94.8, 5.0, 2.0, 0.0, 19.7, -14.7 }, "Belgard-ConcreteBlock", 2029, 0.10, 0.95, Some(("USD", "per-m3", 160.0, 150.0, 11.0)), Some(("uniclass-2015", "Pr_20_93_52_01")))),
        // --- dimension stone (EN 771-6 natural stone; per-m3; A4/A5 dominate for imported slab)
        (MaterialId.Of("stone.marble"),  new("per-m3", new[] { 500.0, 60.0, 30.0, 0.0, 60.0, -17.5 }, "EU-Marble-Slab", 2030, 0.00, 1.00, Some(("USD", "per-m3", 950.0, 320.0, 40.0)), Some(("uniclass-2015", "Pr_20_93_52_56")))),
        (MaterialId.Of("stone.granite"), new("per-m3", new[] { 95.0, 59.4, 96.7, 0.0, 41.4, -4.2 }, "IST-Granite-Slab", 2030, 0.00, 0.90, Some(("USD", "per-m3", 880.0, 300.0, 38.0)), Some(("uniclass-2015", "Pr_20_93_52_56")))),
        // --- glazing (EN 572 float soda-lime + EN 1748-1 borosilicate; per-kg; Glas Trösch EUROFLOAT / AGC / SCHOTT EPD;
        //     glass.crown/glass.flint the Component/glazing pane SubstanceIds — crown the float profile, flint the fire-glass borosilicate)
        (MaterialId.Of("glass.float"),  new("per-kg", new[] { 0.983, 0.050, 0.030, 0.0, 0.038, -0.256 }, "EUROFLOAT-AGC", 2029, 0.37, 1.00, Some(("USD", "per-kg", 1.80, 0.70, 0.10)), Some(("uniclass-2015", "Pr_25_71_33")))),
        (MaterialId.Of("glass.crown"),  new("per-kg", new[] { 0.983, 0.050, 0.030, 0.0, 0.038, -0.256 }, "EUROFLOAT-AGC", 2029, 0.37, 1.00, Some(("USD", "per-kg", 1.85, 0.70, 0.10)), Some(("uniclass-2015", "Pr_25_71_33")))),
        (MaterialId.Of("glass.flint"),  new("per-kg", new[] { 1.74, 0.050, 0.030, 0.0, 0.038, -0.20 }, "SCHOTT-Borosilicate", 2029, 0.30, 1.00, Some(("USD", "per-kg", 4.50, 0.90, 0.12)), Some(("uniclass-2015", "Pr_25_71_33")))),
        // --- insulation (EN 13162-13167; per-kg; Knauf/BEWI/UNILIN EPD; mineral wool low A1-A3, petrochemical foams higher, wood-fibre biogenic)
        (MaterialId.Of("insulation.glasswool"), new("per-kg", new[] { 1.30, 0.10, 0.12, 0.0, 0.19, -0.04 }, "Knauf-GlassWool", 2029, 0.30, 0.00, Some(("USD", "per-kg", 1.15, 0.60, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.stonewool"), new("per-kg", new[] { 1.40, 0.10, 0.12, 0.0, 0.19, -0.04 }, "MineralWool-EU", 2029, 0.25, 0.00, Some(("USD", "per-kg", 1.25, 0.60, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.eps"),  new("per-kg", new[] { 2.23, 0.028, 0.002, 0.0, 2.79, -0.57 }, "BEWI-EPS-80", 2029, 0.00, 0.00, Some(("USD", "per-kg", 2.10, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.xps"),  new("per-kg", new[] { 3.30, 0.030, 0.010, 0.0, 2.80, -0.50 }, "XPS-Foam-EU", 2029, 0.00, 0.00, Some(("USD", "per-kg", 2.60, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.pir"),  new("per-kg", new[] { 2.68, 0.085, 0.278, 0.0, 2.34, -0.65 }, "UNILIN-PIR", 2026, 0.00, 0.00, Some(("USD", "per-kg", 3.10, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.pur"),  new("per-kg", new[] { 3.40, 0.080, 0.200, 0.0, 2.40, -0.60 }, "PUR-Foam-EU", 2029, 0.00, 0.00, Some(("USD", "per-kg", 3.20, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        (MaterialId.Of("insulation.phenolic"), new("per-kg", new[] { 2.42, 0.050, 0.100, 0.0, 2.60, -0.85 }, "Kingspan-Kooltherm-Phenolic", 2030, 0.00, 0.00, Some(("USD", "per-kg", 3.40, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),  // EN 15804+A2 K5 EPD 9.37 kgCO2e/m2 @100mm / 3.87 kg/m2 -> per-kg
        (MaterialId.Of("insulation.woodfibre"), new("per-kg", new[] { -1.20, 0.080, 0.050, 0.0, 1.60, -0.20 }, "WoodFibre-EU", 2029, 0.00, 1.00, Some(("USD", "per-kg", 2.40, 0.60, 0.05)), Some(("uniclass-2015", "Pr_25_71_70")))),
        // --- gypsum board (EN 520; per-kg; Knauf plasterboard EPD; recyclable)
        (MaterialId.Of("gypsum.board"), new("per-kg", new[] { 0.226, 0.0162, 0.0238, 0.0, 0.0162, -0.0187 }, "Knauf-White-Plasterboard", 2030, 0.10, 0.10, Some(("USD", "per-kg", 0.45, 0.50, 0.05)), Some(("uniclass-2015", "Pr_25_71_35_65")))),
        // --- sheet-goods board SUBSTANCES (the Component/panel#PANEL_FAMILY PanelKind.SubstanceId substances — DUAL-KEYING
        //     parity with properties.md). fibre-cement (ASTM C1325; per-m3; Etex/James-Hardie EN 15804+A2 EPD ~350 kgCO2e/m3,
        //     portland-cement A1-A3, mineral so NO biogenic — the sign profile matches masonry not timber)
        (MaterialId.Of("cement.board"), new("per-m3", new[] { 350.0, 15.0, 10.0, 0.0, 15.0, -5.0 }, "Etex-FibreCement-Board", 2029, 0.05, 0.10, Some(("USD", "per-m3", 480.0, 260.0, 18.0)), Some(("uniclass-2015", "Pr_25_71_50")))),
        // wood structural panels (EN 13986/EN 300; per-m3; CORRIM/AWC/EPD softwood-panel biogenic-inclusive GWP-total —
        // negative A1-A3 sequestration, positive C combustion release, exactly the timber-row sign convention)
        (MaterialId.Of("wood.plywood"), new("per-m3", new[] { -800.0, 13.0, 3.0, 0.0, 810.0, -200.0 }, "Softwood-Plywood-EU", 2027, 0.00, 1.00, Some(("USD", "per-m3", 560.0, 175.0, 32.0)), Some(("uniclass-2015", "Pr_20_85_08_65")))),
        (MaterialId.Of("wood.osb"),     new("per-m3", new[] { -982.0, 21.6, 3.0, 0.0, 1193.0, -210.0 }, "CORRIM-OSB-NA", 2027, 0.00, 1.00, Some(("USD", "per-m3", 430.0, 170.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_67")))),
        // --- roofing membranes (per-m2; alwitra/MRPI single-ply EPD; declared per coverage area)
        (MaterialId.Of("membrane.epdm"), new("per-m2", new[] { 5.98, 0.0467, 0.487, 0.0, 5.781, -4.56 }, "alwitra-EVALASTIC-EPDM", 2029, 0.00, 0.00, Some(("USD", "per-m2", 14.0, 9.0, 1.5)), Some(("uniclass-2015", "Pr_25_57_25")))),
        (MaterialId.Of("membrane.pvc"),  new("per-m2", new[] { 6.50, 0.050, 0.400, 0.0, 5.50, -1.00 }, "MRPI-Flagon-PVC", 2029, 0.00, 0.00, Some(("USD", "per-m2", 12.0, 9.0, 1.5)), Some(("uniclass-2015", "Pr_25_57_25")))),
        (MaterialId.Of("membrane.tpo"),  new("per-m2", new[] { 5.80, 0.050, 0.400, 0.0, 5.50, -1.00 }, "SinglePly-TPO-EU", 2029, 0.00, 0.00, Some(("USD", "per-m2", 11.0, 9.0, 1.5)), Some(("uniclass-2015", "Pr_25_57_25")))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);   // seam MaterialId generated equality (ordinal-ignore-case) keys the table

    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out SustainabilityRow? row)
            ? Lower(row!, key)
            : Fin.Succ(Seq<MaterialPropertySet>());   // a material with no declared sustainability data carries no lifecycle case (not a fault)

    // The classification EGRESS the row's (system, code) column resolves to — the Projection/component#COMPONENT_SUBGRAPH
    // Capture composes this beside Lookup so the material classification (steel's Uniclass Pr_20_85_08_11, concrete's
    // Pr_20_85_08_15) leaves the catalogue as a seam Classification value-object rather than dying on the row: Lower lowers
    // ONLY the Discipline-keyed Environmental/Cost physics, so without this resolution the row's classification column has
    // no consumer. The resolved value rides the Projection/component#COMPONENT_PROJECTOR MaterialBinding to the bound
    // element's Object-node Classifications set (classification is an Object-node VALUE per Rasm.Element/Relations/relation,
    // NOT a Material-node field and NOT an edge), the SAME set Rasm.Bim's Semantics/classification ReauthorClassifications
    // re-emits onto IfcRelAssociatesClassification. The pair admits through the seam Classification.Of (the SAME admission
    // Rasm.Bim's bSDD path takes; Title None here — the bSDD title resolves at Bim ingress, the catalogue carrying only the
    // (system, code) identity), railing ElementFault.ValueRejected on a blank pair; an unregistered material or a row with
    // no classification returns None (declared-or-absent, the Lookup-symmetric shape).
    public static Fin<Option<Classification>> Classification(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out SustainabilityRow? row)
            ? row!.Classification.Match(
                Some: c => global::Rasm.Element.Classification.Of(c.System, c.Code, key).Map(Some),
                None: () => Fin.Succ(Option<Classification>.None))
            : Fin.Succ(Option<Classification>.None);
}
```

## [03]-[RESEARCH]

- [SEAM_OWNS_LIFECYCLE_UNION]: the lifecycle-and-unit-cost family is the seam `MaterialPropertySet.Environmental`/`Cost` (keyed to the seam `Discipline`), so the prior Materials-owned partial cases, the inline `WholeLifeGwp` projection, and the `LifecycleStage`/`Currency`/`MeasurementBasis` band declarations are RETIRED, ROUTED into the seam. This owner keeps only the SOURCE: the `SustainabilityCatalogue` database, the `Lower` lowering, and the `Classification` egress. SEAM CONTRACT (Rasm.Element side; this folder consumes): `MaterialPropertySet.OfEnvironmental(MeasurementBasis basis, ImmutableArray<double> impacts, double recycledContent, double endOfLifeRecovery, Op key, PropertyEvidence evidence = default)` — `impacts` the FULL EN 15804+A2 `(ImpactCategory × LifecycleStage)` row-major flat matrix of arity `Environmental.MatrixArity` (`ImpactCategory.Count × LifecycleStage.Count`, the 13-indicator × 6-module core set), a carbon-only row reaching it through the seam-owned `Environmental.CarbonMatrix(ReadOnlyMemory<double> stageGwp)` builder (the `GwpTotal` row embedded, every other indicator row zeroed — the partial-EPD invariant), the accumulating `Matrix`/`Guarded` admission slots re-guarding arity-then-finite and the fractions `[0,1]`, EPD identity + expiry riding `evidence` as `PropertyEvidence.Declaration("epd", registrationNumber, validUntil)` (the seam deleted its per-case `Epd`/`ValidUntilYear` columns as an evidence double-store — this row's former columns deleted for the same reason) — plus the intrinsic `IndicatorAt(category, stage)` cell read, the `WholeLife(category)` cross-stage fold, and the carbon-keyed `Gwp` (the DERIVED `(GwpTotal, A1A3)` read — the seam carries no stored headline scalar and `OfEnvironmental` takes no `gwpKgCo2e` arg), `StageAt`, `WholeLifeGwp`, and the `StageGwp` per-module row projection the `Rasm.Compute` carbon fold reads one-hop; `MaterialPropertySet.OfCost(Currency, MeasurementBasis, double supply, double install, double lifecycle, Op key, PropertyEvidence evidence = default)` guards each column finite and non-negative; `LifecycleStage` (`[SmartEnum<int>]`, A1A3/A4/A5/B/C/D, `Count` an Items-derived accessor) + `ImpactCategory` (`[SmartEnum<int>]`, the GwpTotal/GwpFossil/GwpBiogenic/GwpLuluc/ODP/AP/EP×3/POCP/ADP×2/WDP thirteen, `Parse` by wire Name); `Currency` (`[ValueObject<string>]` shape-validated ISO 4217 alpha-3, `Parse` railing on malformed — the roster check is the `Rasm.Bim` NodaMoney algebra's, an OPEN token) / `MeasurementBasis` (`[SmartEnum<string>]`, the CLOSED `per-kg`/`per-m2`/`per-m3`/`per-item` four + `Parse`); and `Classification.Of(string system, string code, Op key)` — the edition-unspecified rail overload lifting the row's pair to the seam `(System, Code, Edition)` value-object (`Edition`/`Source`/`EditionDate`/`Title` defaulting empty — bSDD resolves annotations at Bim ingress). The stage vector / fractions / costs pass as RAW centrals the seam guards internally; classification is NOT a case (not a `Discipline`-keyed physics) — it leaves through the `Classification` egress onto the bound element's Object node.
- [SHARED_PUBLISHED_COMPOSED]: this page COMPOSES the `Published<T>` ingress carrier `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` DECLARES (same `Rasm.Materials.Properties` namespace, no import) — the double-family arm `Published.Of(double, relative, evidence)` carries the recycled/recovery fractions and the cost columns, `Published.Vector` the per-module `StageGwp` entries sharing the EPD `Declaration` evidence, `Published.Centrals` the central-vector read `CarbonMatrix` consumes; the prior `ScalarDatum`/`SustainabilityUncertainty` scaffolds and the local `ScalarOf`/`Scalars` helpers are DELETED, and `CostDatum` is REBUILT as the currency/basis column group over `Published<double>` (no uncertainty scaffold of its own — the shared carrier owns the band). The EPD confidence profile (±15% module GWP, ±10% fractions, ±20/25/25% costs) survives as named `const` policy anchors on the row ctor. GUARD (a QuantityRow false-positive): `Lower` stays RAW-scalar into `OfEnvironmental`/`OfCost` — a StageGwp/recycled/cost magnitude is a domain-basis value (kgCO2e per `MeasurementBasis` unit, ISO 4217 currency per unit), never a UnitsNet `IQuantity`, so NO `MeasureValue`/`QuantityRow` mint exists on this page (the typed-mint mandate is the engineering catalogue's alone), and CO2e is NEVER a `MassUnit.Kilogram` coercion.
- [EN_15978_LIFECYCLE_MODULES]: the EN 15978 modules seed the seam `LifecycleStage` band: A1-A3 (product, the cradle-to-gate boundary), A4 (transport), A5 (construction-installation), B1-B7 (use — the calcium-silicate/AAC carbonation re-uptake), C1-C4 (end-of-life), D (benefits/loads beyond the boundary). `SustainabilityRow.StageGwp` is the module-level GWP-total in `[A1A3, A4, A5, B, C, D]` order (the seam `LifecycleStage.Index` order), the seam `CarbonMatrix` embedding it as the `GwpTotal` indicator row; the cradle-to-gate `Gwp` is the `[A1A3]` cell, the cradle-to-grave `WholeLifeGwp` the all-stage fold — both the seam's intrinsic projections (this owner reads, never re-authors). A glulam row's negative A1-A3 (`timber.gl24h` -608 kgCO2e/m³) is biogenic sequestration, its positive C (+754) the combustion release, its negative D (-410) the energy-substitution credit — the vector carries the full module sign profile, never a single positive scalar.
- [ISO_14040_DECLARED_UNIT]: an EPD is declared per functional unit, and this catalogue stores each row AT ITS NATIVE `declared_unit` — the `EnvironmentalBasis` token `Lower` parses to the seam `MeasurementBasis`, the magnitudes kgCO2e per that unit exactly as published: per-kg metals/glass/gypsum/insulation, per-m³ cast/cut/fired materials (concrete, timber, masonry, stone, boards), per-m² membranes, with NO `× density` rewrite to a curated `PerM3` (a per-area or per-kg EPD admits only at its native basis). The baked catalogue and the live `Rasm.Compute` EC3 ingress (tagging the openEPD `declared_unit`) land identically basis-tagged, and the RELOCATED `AggregateEnvironmental` scales each ply through the SAME basis-aware `DeclaredQuantity` owner the cost fold uses (`PerKg` → volume×density, `PerM2` → face area, `PerM3` → volume, `PerItem` → unit) — one basis-aware path, no basis branch.
- [ASSEMBLY_AGGREGATOR_RELOCATED]: the multi-ply lifecycle folds `AggregateEnvironmental` (per-module `Σ(value_i · quantity_i)`, the cradle-to-grave total, the embodied-carbon intensity) and `AggregateCost` (supply/install/lifecycle × quantities, the currency-mismatch guard) are RELOCATED to `Rasm.Compute` with the engineering `AssemblyAggregator` — the seam carries the per-material input, Compute folds the assignment plies and writes the `Assessment.Result` node back. The prior `AssemblyLifecycle`/`AssemblyCost`/`PlyQuantity` receipts and `AddScaled`/`AccumulateCost` kernels leave this folder. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` reading the seam `MaterialComposition` plies + the per-material `Environmental`/`Cost` inputs (`env.StageGwp` the seam's one-hop carbon-row projection).
- [IFC_PSET_ENVIRONMENTAL]: the seam `Environmental` case maps to IFC 4.3 `Pset_EnvironmentalImpactValues` (`Carbon`/`WholeLifeCarbon`/`RecycledContent`), `Cost` to `Pset_ConstructionCosts` (`SupplyCost`/`InstallationCost`/`LifeCycleCost` over an `IfcMonetaryUnit`), and the classification `(system, code)` to `IfcClassificationReference` (`ReferencedSource`/`Identification`) associated via `IfcRelAssociatesClassification` — `Rasm.Bim` reads the projected seam `Material` node and emits from the seam graph; the Pset member-name mapping is `Rasm.Bim`'s side, the computation this side, never a `MaterialProperty` type crossing a boundary.
- [FULL_ROSTER_DEPTH]: the `Rows` table is the FULL EN/ASTM/CSA structural-and-envelope EPD roster in EXACT `MaterialId` parity with the engineering catalogue — EVERY id that catalogue rosters resolves a lifecycle row here (no engineering-only orphan): the carbon steels `s235`–`s690` + `s450` + the `metal.steel`/`metal.iron` aliases, the AISC `a36`/`a992`/`a572` North-American EAF hot-rolled profile, every rebar grade (EN 10080, ASTM A615/A706, CSA G30.18), the EN 10088 stainless family, every concrete strength class + `lc` + the `concrete.cmu` block substance, the full EN 338 C/D series + `wood.oak`, the full EN 14080 glulam set, the EN 1999 alloys, EN 771 masonry + stone, the glass substances (`glass.float`/`glass.crown` the EUROFLOAT-AGC float profile, `glass.flint` the SCHOTT borosilicate fire-glass EPD), the EN 13162-13167 insulation set, EN 520 gypsum, the sheet-goods board substances, and the EN 13956 membranes. GWP TRACKS MASS PER DECLARED UNIT, NOT STRENGTH CLASS: S235 and S690 share the WorldSteel sections profile (the carbon is in the mass, not the yield grade), the ASTM/CSA rebar grades the CRSI/ArcelorMittal EAF family, the sawn/glulam grades their shared timber EPDs, `glass.crown` the float profile (the pane substance is soda-lime float) — while the concrete classes DIVERGE because cement content scales A1-A3 with strength. Every figure is a published EN 15804+A2 EPD / ICE v3 / EC3 industry-average magnitude per the declared unit (the `Declaration` evidence names the source family and expiry); a grade with no declared EPD omits a row and `Lookup` returns the empty lifecycle set, never an invented number.
