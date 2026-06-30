# [MATERIALS_SUSTAINABILITY]

THE SUSTAINABILITY-AND-PROCUREMENT SOURCE. The lifecycle-and-procurement property family is SEAM-owned: the `Rasm.Element` `MaterialPropertySet` `[Union]` carries `Environmental` (a `MeasurementBasis` declared unit + the per-EN-15978-module `StageGwp` vector of raw `kgCO2e` magnitudes over the seam `LifecycleStage` band exactly as the seam `Acoustic` case bands its absorption spectrum — the cradle-to-gate GWP a DERIVED read of the A1-A3 module, never a double-stored headline scalar — the EPD/recycled/end-of-life provenance with the intrinsic `WholeLifeGwp` projection fold) and `Cost` (the supply/install/lifecycle columns over the seam `Currency` band and `MeasurementBasis`) — classification is NOT a `MaterialPropertySet` case but a generic `Classification` `[ComplexValueObject]` (an opaque `System` token + `Code`) the `Projection/material#MATERIAL_SUBGRAPH` `Capture` threads onto the bound element's `Object` node (an Object-node VALUE per `Rasm.Element/Relations/relation`, never a `Material`-node field nor an edge) and `Rasm.Bim` re-emits onto `IfcRelAssociatesClassification` off that Object node (§4B) — so the prior Materials-owned `Environmental`/`Cost` `MaterialProperty` partial cases and the lifecycle aggregation folds are RETIRED, ROUTED into the seam. This owner is now the Materials SOURCE: one `SustainabilityCatalogue` is the registered-row database of cradle-to-gate EPD / cost / classification data per `MaterialId`, the `Lower` lowering coerces a row into the seam `Environmental`/`Cost` cases (passing the raw `StageGwp` vector the seam `OfEnvironmental` guards FINITE via `AllFinite` — CO2e is a domain basis, not an SI dimension, so the modules are raw `kgCO2e` magnitudes never forced through `MeasureValue.Of`/`MassUnit` — at the row's own declared `MeasurementBasis` and the cost columns at theirs, the recycled/end-of-life fractions guarded to `[0,1]` by the seam admission), and the per-material set so projected IS the `Environmental`/`Cost` analysis input `Rasm.Compute` reads off the `Material` node directly (the `Properties/properties#ASSESSMENT_INPUT` Materials-authored marshaller is retired). A material is a FULL LIFECYCLE OBJECT, not only a physics-and-shade carrier: a wall material carries its embodied carbon and its cost as `MaterialPropertySet` cases over one `MaterialId` the projector lowers into the seam `Material` node, plus its BIM classification as a generic `Classification` value-object the `Capture` threads onto the bound element's `Object` node (never onto the `Material` node), never a `EcoMaterial`/`CostMaterial`/`ClassifiedMaterial` surface; the whole-building embodied-carbon takeoff and the cost-of-construction rollup are the multi-ply `AssemblyAggregator` (the `AggregateEnvironmental`/`AggregateCost` folds) RELOCATED to `Rasm.Compute` — the seam carries the per-material INPUT, never the assembly aggregation. The page composes the seam (`MaterialPropertySet.Environmental`/`Cost`, `LifecycleStage`/`Currency`/`MeasurementBasis`, the generic `Classification` value-object), the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` admission pattern, and the seam `ElementFault.ValueRejected` (band 2500) rail every lowering fault lifts on unchanged; it re-mints NO seam type, mints NO `MaterialFault`, and admits NO `UnitsNet` quantity — the `StageGwp` modules, the recycled/end-of-life fractions, and the cost columns are raw domain scalars the seam guards (`AllFinite`/`[0,1]`/non-negative) at the lowering edge.

## [01]-[INDEX]

- [01]-[SUSTAINABILITY_PROPERTY]: the `SustainabilityRow` published-data ingress shape, the `SustainabilityCatalogue` registered-row database, the `Lower` lowering into the seam `Environmental`/`Cost` cases (passing each EPD's native declared `MeasurementBasis` and the raw per-module `StageGwp` magnitudes the seam guards FINITE, over the `LifecycleStage`/`Currency`/`MeasurementBasis` bands), the `Classification` egress lifting the row's `(system, code)` to a seam `Classification` value-object `Rasm.Bim` associates (NOT a lowered property case), and the `Lookup` the projector composes with the engineering catalogue.

## [02]-[SUSTAINABILITY_PROPERTY]

- Owner: `SustainabilityRow` the published-data ingress record; `SustainabilityCatalogue` the registered-row database; `Lower` the row→seam-case lowering; the EN 15978 / ISO 14040 lifecycle-and-procurement source.
- Cases: one `SustainabilityRow` shape — the environmental (cradle-to-gate GWP + the per-EN-15978-module stage vector + EPD/recycled/end-of-life provenance), optional cost (supply/install/lifecycle over a currency + measurement basis), and optional classification (system + code) published columns; the `Lower` lowering produces a `Seq<MaterialPropertySet>` of the seam `Environmental`/`Cost` cases, each a `MaterialPropertySet` over a `MaterialId`, never a property subtype; the row's optional classification `(system, code)` is NOT lowered to a property case — it rides as data `Rasm.Bim` associates via `IfcRelAssociatesClassification`.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key)` — the published-row lowering passing the row's native declared `EnvironmentalBasis` and the per-module `StageGwp` vector through the seam `MaterialPropertySet.OfEnvironmental` band gate (the `LifecycleStage` arity, each module a raw `kgCO2e`-per-basis-unit magnitude guarded FINITE via `AllFinite` and the recycled/end-of-life fractions guarded `[0,1]` — never a `MeasureValue.Of`/`MassUnit` coercion, CO2e being a domain basis not an SI dimension — the cradle-to-gate `Gwp` derived from the A1A3 module, no headline scalar passed) and the optional cost columns through the seam `MaterialPropertySet.OfCost` (the `Currency`/`MeasurementBasis` bands, each column non-negative), producing only the seam `Environmental`/`Cost` cases — the row's optional classification `(system, code)` is NOT lowered to a property case but is the EGRESS the sibling `Classification(MaterialId id, Op key)` resolution lifts to a seam `Classification` value-object the `Projection/material#MATERIAL_SUBGRAPH` `Capture` composes and threads onto the `MaterialBinding` so the bound element's `Object` node carries it (classification is an Object-node VALUE per `Rasm.Element/Relations/relation`, the seam generic `Classification` value-object, §4B), `Rasm.Bim` re-emitting it onto `IfcRelAssociatesClassification` off that Object node, so the catalogue's classification column has a real consumer rather than dying on the row; `Fin<T>` aborts on a non-finite, out-of-`[0,1]`, or unknown-basis column (the seam admission's `ElementFault.ValueRejected` lifts unchanged); the `MaterialId`-keyed `Lookup(MaterialId id, Op key)` resolves the catalogue row then lowers (and `Classification` resolves the row's pair through `Classification.Of`, `None` when the row or material is absent), and the `Projection/material#MATERIAL_SUBGRAPH` `Capture` composes the lowered set with the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` engineering set for the full `MaterialSpec.Properties` and the resolved `Classification` onto the `MaterialBinding` for the bound element's Object-node classification.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfEnvironmental`/`OfCost` + the `Environmental`/`Cost` cases, `LifecycleStage`/`Currency`/`MeasurementBasis`, the generic `Classification` value-object + `Classification.Of`, `ElementFault.ValueRejected`, `MaterialId`), Rasm (project — the kernel `Op` op-key), LanguageExt.Core (`Fin`/`Seq`/`Seq1`/`Option`), BCL inbox (`FrozenDictionary`/`ReadOnlyMemory<double>`). NO `UnitsNet` (CO2e and currency are domain bases, not SI dimensions, so the page coerces nothing through `MeasureValue`), NO `MaterialFault` (every lowering and classification fault is the seam `ElementFault`).
- Growth: a new EN 15978 module is one seam `LifecycleStage` row (the `StageGwp` carrier widens by data, the relocated `Rasm.Compute` aggregation fold re-reads the new stage index); a new currency is one opaque ISO 4217 token the row supplies (the seam `Currency` never grows a roster row), a new classification system one opaque `System` token a `Rasm.Bim` projector supplies (the seam `Classification` never grows a row either), a new procurement column one seam `Cost` column; a new declared basis is one seam `MeasurementBasis` row the EPD's `declared_unit` resolves to; a new known material is one `SustainabilityCatalogue.Rows` entry — a `MaterialId` key and a `SustainabilityRow` value — never a per-discipline material type, never a parallel Eco/Cost surface. The catalogue grows by row, the lifecycle vocabulary by seam band.
- Boundary: `SustainabilityRow` is the published-DATA ingress (the raw EPD/cost-plan/classification values), NOT a parallel domain union — the seam `MaterialPropertySet.Environmental`/`Cost` are the one typed carriers (classification is NOT a `MaterialPropertySet` case — it rides the row as data `Rasm.Bim` associates), `Lower` the `BOUNDARY_ADMISSION` that lowers the raw row into the seam cases once; each `StageGwp` module is a raw `kgCO2e`-per-basis-unit magnitude the seam `OfEnvironmental` guards FINITE via `AllFinite`, declared at the row's OWN `EnvironmentalBasis` (the EPD's native `declared_unit` — a per-kg steel EPD stays `PerKg`, a per-m² membrane EPD stays `PerM2`) NOT forced to one curated `PerM3` at ingress — the curated rows below carry their native basis and the `Rasm.Compute` `AggregateEnvironmental` scales each ply by the basis-matching element quantity through its basis-aware `DeclaredQuantity` derivation (the SAME basis axis the `Cost` case carries and the EC3 ingress tags), so the per-m² membrane EPD a forced-`PerM3` normalization would have dropped admits first-class — and CO2e is a domain basis not an SI dimension so a module is NEVER a `MassUnit.Kilogram` `MeasureValue` coercion; the cost columns ride the seam `Currency` band (a closed ISO 4217 `[SmartEnum<string>]` so a non-standard currency is a row never a free string); the seam `Environmental` case is a banded vector NOT a scalar carbon number — the `StageGwp` is a fixed-length `ReadOnlyMemory<double>` over the seam `LifecycleStage` EN 15978 module centres (the SAME carrier shape the seam `Acoustic` case admits) so the cradle-to-gate `Gwp` is a DERIVED read of the A1-A3 module (NOT a double-stored headline scalar) and the cradle-to-grave total the seam `WholeLifeGwp` projection fold (the seam owns the intrinsic single-material fold exactly as it owns `StcWeighted`, this owner reads never re-authors); the `RecycledContent`/`EndOfLifeRecovery` pass as raw `double` the seam `OfEnvironmental` guards to `[0,1]` once (the seam carries no kernel `UnitInterval` arg — re-minting the fraction here would diverge from the seam signature, the one admission owner); a non-finite stage GWP (`AllFinite`), an out-of-`[0,1]` fraction, an unknown basis token, or a negative cost rails the seam admission fault `ElementFault.ValueRejected` (band 2500), never a clamped sentinel — a negative module is VALID biogenic-sequestration carbon (the `wood.oak` A1-A3 credit), never rejected; the lowered cases lower into the seam `Material` node the `Projection/material#MATERIAL_PROJECTOR` authors and `Rasm.Bim` reads `Pset_EnvironmentalImpactValues` / `Pset_ConstructionCosts` / `IfcClassificationReference` from the seam graph — no Materials wire carrier, the Pset member-name mapping the `Rasm.Bim` side, the lifecycle computation this side; the multi-ply `AggregateEnvironmental`/`AggregateCost` rollups are `Rasm.Compute`'s (the seam carries the per-material `Environmental`/`Cost` input, never the assembly fold).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, MaterialPropertySet (Environmental|Cost), LifecycleStage, Currency, MeasurementBasis,
                                     // Classification (the seam (system, code) value-object the catalogue's classification column resolves to),
                                     // ElementFault (the seam value-admission band 2500 every lowering fault lifts on)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // the property-catalogue folder owner — SustainabilityCatalogue lives here beside MaterialPropertyCatalogue, the Projection/material projector imports it as Rasm.Materials.Properties

// --- [MODELS] ------------------------------------------------------------------------------
// The published lifecycle/procurement data for one material — the ingress row Lower lowers into the seam
// Environmental/Cost property cases, and whose (system, code) the sibling Classification resolution lifts to a seam
// Classification value-object Rasm.Bim associates (NOT a property case — classification is not a Discipline-keyed
// physics). A flat DATA record (the raw EPD/cost-plan values), NOT a parallel union.
// NO headline GWP scalar: the cradle-to-gate A1-A3 value IS StageGwp[LifecycleStage.A1A3.Index] (the seam Environmental
// case derives Gwp => StageAt(A1A3) over the StageGwp vector), so a row carries the per-EN-15978-module vector ALONE — a
// double-stored A1-A3 (a headline scalar beside the vector module) is the DERIVED_LOGIC defect that admitted divergent
// values with no enforced equality, collapsed here to the one StageGwp source the seam projects.
// BASIS-NATIVE: a row carries the EPD's OWN declared unit in EnvironmentalBasis (the published declared_unit token) — the
// StageGwp magnitudes are kgCO2e PER that basis unit, stored as the EPD publishes them, never force-normalized to one
// curated PerM3 at ingress (the prior "× density at curation" rewrite a per-m² membrane EPD could not survive). The seam
// Environmental case is basis-aware exactly as Cost is, so the Rasm.Compute AggregateEnvironmental scales each ply by the
// basis-matching element quantity — the SAME path the live EC3 ingress feeds, so a baked and an EC3-resolved declaration
// fold identically with no basis branch.
public sealed record SustainabilityRow(
    string EnvironmentalBasis,       // the EPD's native declared_unit token (per-kg/per-m2/per-m3/per-item) parsed to the seam MeasurementBasis
    double[] StageGwp,               // per EN 15978 module, kgCO2e per the EnvironmentalBasis unit, seam LifecycleStage.Count entries; [A1A3.Index] IS the cradle-to-gate GWP
    string Epd,
    int ValidUntilYear,
    double RecycledContent,
    double EndOfLifeRecovery,
    Option<(string Currency, string Basis, double Supply, double Install, double Lifecycle)> Cost,
    Option<(string System, string Code)> Classification);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SustainabilityCatalogue {
    // Lowers a published row into the seam Environmental/Cost cases: the row's native EnvironmentalBasis token parses to a
    // seam MeasurementBasis (the SAME Parse a cost basis takes), then the stage vector lowers through the seam Environmental.Of
    // band gate AT THAT BASIS (its A1A3 module IS the cradle-to-gate GWP the seam Gwp accessor derives — no headline scalar
    // passed), and the cost through the seam Cost.Of currency/basis bands. The stage vector and the recycled/end-of-life
    // fractions pass as RAW values — the seam OfEnvironmental guards each stage module FINITE (AllFinite, a raw kgCO2e
    // magnitude — CO2e is a domain basis not an SI dimension, never a MassUnit.Kilogram MeasureValue coercion) and the
    // fractions [0,1] internally. An unknown basis token rails ElementFault.ValueRejected through MeasurementBasis.Parse
    // before the Environmental case constructs, so a malformed declared_unit is a fault not a silent default. Classification
    // is NOT lowered to a property case (it is not a Discipline-keyed material physics): the row carries the (system, code)
    // pair and Rasm.Bim associates it via IfcRelAssociatesClassification (the seam generic Classification value-object, §4B)
    // — so this lowering produces only the seam Environmental and Cost cases.
    public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key) =>
        from basis in MeasurementBasis.Parse(row.EnvironmentalBasis, key)
        from environmental in MaterialPropertySet.OfEnvironmental(basis, row.StageGwp.AsMemory(), row.RecycledContent, row.EndOfLifeRecovery, row.Epd, row.ValidUntilYear, key)
        from cost in row.Cost.Match(
            None: () => Fin.Succ(Seq<MaterialPropertySet>()),
            Some: c => from currency in Currency.Parse(c.Currency, key)
                       from costBasis in MeasurementBasis.Parse(c.Basis, key)
                       from priced in MaterialPropertySet.OfCost(currency, costBasis, c.Supply, c.Install, c.Lifecycle, key)
                       select Seq1(priced))
        select Seq1(environmental) + cost;

    // The curated reference catalogue covers the SAME structural-materials domain the engineering Properties/properties#
    // MATERIAL_PROPERTY_CATALOGUE rosters (steel/concrete/timber/aluminium/masonry/glass/insulation/gypsum/membrane) keyed
    // by the SAME canonical MaterialId, each row storing the EPD AS PUBLISHED at its native declared_unit — NO curation
    // normalization to a single PerM3 basis (the prior "× density at curation" rewrite that silently dropped a per-area or
    // per-item EPD): the per-kg metals/insulation/glass/gypsum/brick stay PerKg, the per-m3 cast/cut materials (concrete,
    // timber, stone) PerM3, the membrane PerM2 (its EPD declares per coverage area). The Compute AggregateEnvironmental
    // scales each ply by the basis-matching element quantity (PerKg -> volume×density, PerM3 -> volume, PerM2 -> face area),
    // so a per-kg and a per-m3 declaration fold through the one basis-aware DeclaredQuantity path. The cradle-to-gate GWP is
    // the A1A3 module the seam Gwp accessor derives — NO headline scalar column; both Environmental and Cost are basis-aware.
    // The figures are published EN 15978 / openEPD A1-A3 cradle-to-gate magnitudes (per the declared unit), the C/D modules
    // the end-of-life release/recovery (timber's negative A1-A3 the biogenic-carbon credit, its positive B/C the combustion
    // release; recycled steel's near-zero D the avoided-burden credit). The roster grows by ROW to any registered material;
    // a material with no declared EPD simply omits a row and Lookup returns the empty lifecycle set, never a fault.
    public static readonly FrozenDictionary<MaterialId, SustainabilityRow> Rows = new (MaterialId Id, SustainabilityRow Row)[] {
        // --- structural steel (EN 10025 sections; per-kg WorldSteel/EPD; high recycled content + end-of-life recovery)
        (MaterialId.Of("steel.s235"),   new("per-kg", new[] { 1.13, 0.040, 0.030, 0.0, 0.040, -0.430 }, "EPD-STEEL-S235", 2030, 0.90, 0.95,
            Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_93_52")))),
        (MaterialId.Of("steel.s355"),   new("per-kg", new[] { 1.16, 0.040, 0.030, 0.0, 0.040, -0.430 }, "EPD-STEEL-S355", 2030, 0.90, 0.95,
            Some(("USD", "per-kg", 1.05, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_93_52")))),
        (MaterialId.Of("steel.b500"),   new("per-kg", new[] { 0.75, 0.035, 0.025, 0.0, 0.035, -0.380 }, "EPD-REBAR-B500", 2029, 0.97, 0.70,
            Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("metal.iron"),   new("per-kg", new[] { 1.90, 0.040, 0.030, 0.0, 0.040, -0.420 }, "EPD-IRON-0001", 2030, 0.95, 0.90,
            Some(("USD", "per-kg", 0.85, 0.40, 0.10)), Some(("uniclass-2015", "Pr_20_93_52")))),
        // --- concrete (EN 206; per-m3; A1-A3 rises with cement content; minimal recycled, low recovery)
        (MaterialId.Of("concrete.c25"), new("per-m3", new[] { 245.0, 9.0, 6.0, 0.0, 8.0, -7.0 }, "EPD-CONC-C25", 2029, 0.10, 0.30,
            Some(("USD", "per-m3", 120.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_31_04")))),
        (MaterialId.Of("concrete.c30"), new("per-m3", new[] { 268.0, 9.0, 6.0, 0.0, 8.0, -7.0 }, "EPD-CONC-C30", 2029, 0.10, 0.30,
            Some(("USD", "per-m3", 130.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_31_04")))),
        (MaterialId.Of("concrete.c40"), new("per-m3", new[] { 312.0, 9.0, 6.0, 0.0, 8.0, -7.0 }, "EPD-CONC-C40", 2029, 0.10, 0.30,
            Some(("USD", "per-m3", 155.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_31_04")))),
        // --- structural timber (EN 14080; per-m3; A1-A3 negative biogenic sequestration, B/C combustion release)
        (MaterialId.Of("timber.c24"),   new("per-m3", new[] { -660.0, 38.0, 22.0, 0.0, 720.0, -44.0 }, "EPD-TIMB-C24", 2031, 0.00, 0.80,
            Some(("USD", "per-m3", 520.0, 180.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_85")))),
        (MaterialId.Of("timber.gl24h"), new("per-m3", new[] { -630.0, 35.0, 64.0, 0.0, 705.0, -48.0 }, "EPD-GLULAM-GL24H", 2031, 0.00, 0.80,
            Some(("USD", "per-m3", 880.0, 240.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_36")))),
        (MaterialId.Of("wood.oak"),     new("per-m3", new[] { -630.0, 35.0, 21.0, 0.0, 182.0, -42.0 }, "EPD-OAK-0014", 2031, 0.00, 0.85,
            Some(("USD", "per-m3", 1200.0, 280.0, 60.0)), Some(("uniclass-2015", "Pr_20_85_85")))),
        // --- aluminium (EN 573 6061-T6; per-kg; energy-intensive primary, very high recyclability)
        (MaterialId.Of("aluminium.6061t6"), new("per-kg", new[] { 8.10, 0.090, 0.040, 0.0, 0.050, -1.700 }, "EPD-ALU-6061T6", 2030, 0.35, 0.95,
            Some(("USD", "per-kg", 3.40, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_93_01")))),
        // --- masonry (EN 771; per-m3; fired clay carries kiln carbon, AAC autoclave lower)
        (MaterialId.Of("masonry.clay"), new("per-m3", new[] { 432.0, 22.0, 14.0, 0.0, 12.0, 0.0 }, "EPD-BRICK-CLAY", 2028, 0.05, 0.40,
            Some(("USD", "per-m3", 280.0, 220.0, 15.0)), Some(("uniclass-2015", "Pr_20_76_12")))),
        (MaterialId.Of("masonry.aac"),  new("per-m3", new[] { 165.0, 14.0, 9.0, 0.0, 9.0, 0.0 }, "EPD-AAC-0003", 2028, 0.05, 0.30,
            Some(("USD", "per-m3", 190.0, 160.0, 12.0)), Some(("uniclass-2015", "Pr_20_76_03")))),
        (MaterialId.Of("stone.marble"), new("per-m3", new[] { 297.0, 27.0, 14.0, 0.0, 14.0, 0.0 }, "EPD-MARB-0007", 2029, 0.05, 0.20,
            Some(("USD", "per-m3", 950.0, 320.0, 40.0)), Some(("uniclass-2015", "Pr_20_85_52")))),
        // --- glazing (EN 572 float glass; per-kg)
        (MaterialId.Of("glass.float"),  new("per-kg", new[] { 1.24, 0.060, 0.030, 0.0, 0.040, -0.090 }, "EPD-GLASS-FLOAT", 2029, 0.25, 0.30,
            Some(("USD", "per-kg", 1.80, 0.70, 0.10)), Some(("uniclass-2015", "Pr_20_76_30")))),
        // --- insulation (EN 13162/13163/13164/13165; per-kg; mineral wool A1, petrochemical foams higher A1-A3)
        (MaterialId.Of("insulation.mineralwool"), new("per-kg", new[] { 1.28, 0.050, 0.030, 0.0, 0.030, 0.020 }, "EPD-MW-0011", 2030, 0.30, 0.10,
            Some(("USD", "per-kg", 1.20, 0.60, 0.05)), Some(("uniclass-2015", "Pr_25_71_55")))),
        (MaterialId.Of("insulation.eps"),  new("per-kg", new[] { 3.29, 0.060, 0.040, 0.0, 0.040, -0.700 }, "EPD-EPS-0006", 2030, 0.05, 0.20,
            Some(("USD", "per-kg", 2.10, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_31")))),
        (MaterialId.Of("insulation.xps"),  new("per-kg", new[] { 3.42, 0.060, 0.040, 0.0, 0.040, -0.700 }, "EPD-XPS-0009", 2030, 0.05, 0.20,
            Some(("USD", "per-kg", 2.60, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_97")))),
        (MaterialId.Of("insulation.pir"),  new("per-kg", new[] { 4.03, 0.070, 0.050, 0.0, 0.050, -0.900 }, "EPD-PIR-0004", 2030, 0.05, 0.15,
            Some(("USD", "per-kg", 3.10, 0.55, 0.05)), Some(("uniclass-2015", "Pr_25_71_67")))),
        // --- gypsum board (EN 520; per-kg; recyclable plasterboard)
        (MaterialId.Of("gypsum.board"), new("per-kg", new[] { 0.30, 0.030, 0.020, 0.0, 0.020, -0.010 }, "EPD-GYP-0002", 2028, 0.20, 0.40,
            Some(("USD", "per-kg", 0.45, 0.50, 0.05)), Some(("uniclass-2015", "Pr_25_57_06")))),
        // --- membrane (EPDM roofing; per-m2; declared per coverage area)
        (MaterialId.Of("membrane.epdm"), new("per-m2", new[] { 6.80, 0.120, 0.080, 0.0, 0.090, -0.300 }, "EPD-EPDM-0005", 2029, 0.05, 0.10,
            Some(("USD", "per-m2", 14.0, 9.0, 1.5)), Some(("uniclass-2015", "Pr_25_57_25")))),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);   // seam MaterialId generated equality (ordinal-ignore-case) keys the table

    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out SustainabilityRow? row)
            ? Lower(row!, key)
            : Fin.Succ(Seq<MaterialPropertySet>());   // a material with no declared sustainability data carries no lifecycle case (not a fault)

    // The classification EGRESS the row's (system, code) column resolves to — the Projection/material#MATERIAL_SUBGRAPH
    // Capture composes this beside Lookup so the material classification (steel's Uniclass Pr_20_93_52, concrete's
    // Pr_20_31_04) leaves the catalogue as a seam Classification value-object rather than dying on the row: Lower lowers
    // ONLY the Discipline-keyed Environmental/Cost physics, so without this resolution the row's classification column has
    // no consumer. The resolved value rides the Projection/material#MATERIAL_PROJECTOR MaterialBinding to the bound
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

- [SEAM_OWNS_LIFECYCLE_UNION]: the lifecycle-and-procurement family is the seam `MaterialPropertySet.Environmental`/`Cost` (keyed to the seam `Discipline` Environmental/Cost), so the prior Materials-owned partial cases, the `Environmental.WholeLifeGwp` inline projection, and the `LifecycleStage`/`Currency`/`MeasurementBasis` band declarations are RETIRED and ROUTED into the seam. This owner keeps only the Materials SOURCE: the `SustainabilityCatalogue` published-row database, the `Lower` lowering, and the `Classification` egress. SEAM CONTRACT (Rasm.Element side; this folder consumes): `MaterialPropertySet.OfEnvironmental(MeasurementBasis basis, ReadOnlyMemory<double> stageGwp, double recycled, double endOfLife, string epd, int valid, Op)` + the intrinsic `WholeLifeGwp`/`StageAt`/`Gwp` accessors (the cradle-to-gate `Gwp` a DERIVED read `StageAt(A1A3)` over the vector, NOT a stored headline scalar — the seam carries no `GlobalWarmingPotential` field and `OfEnvironmental` takes no `gwpKgCo2e` arg), `MaterialPropertySet.OfCost(Currency, MeasurementBasis, double supply, double install, double lifecycle, Op)`, `LifecycleStage` (the EN 15978 A1-A3/A4/A5/B/C/D band), `Currency`/`MeasurementBasis` (`Parse` + the closed rows), and `Classification.Of(string system, string code, Op key, Option<string> title)` (the seam generic `(system, code)` value-object the `Classification` egress lifts the row's pair to, `title` omitted — bSDD resolves it at Bim ingress). The stage vector / recycled / end-of-life pass as RAW values the seam coerces and guards internally. Material classification is NOT a `MaterialPropertySet` case (it is not a `Discipline`-keyed physics) — the row's `(system, code)` leaves through the `Classification` egress as a seam `Classification` value-object the `Projection/material#MATERIAL_SUBGRAPH` `Capture` composes and threads onto the `MaterialBinding`, the bound element's Object node carrying it and `Rasm.Bim` re-emitting it onto `IfcRelAssociatesClassification` off that Object node (§4B — classification is an Object-node VALUE, never a `Material`-node field nor an edge), never a column that dies on the row.
- [EN_15978_LIFECYCLE_MODULES]: the EN 15978 lifecycle-stage modules seed the seam `LifecycleStage` band: A1-A3 (product, the cradle-to-gate boundary the `Gwp` carries), A4 (transport to site), A5 (construction-installation), B1-B7 (use/maintenance/operational), C1-C4 (end-of-life), D (benefits/loads beyond the system boundary). The per-module `StageGwp` vector is the EN 15978 module-level GWP an EPD declares; the cradle-to-gate `Gwp` is the A1-A3 sum, the cradle-to-grave `WholeLifeGwp` the all-module sum — both the seam's intrinsic projection folds over the banded carrier (the seam owns them as it owns the acoustic `StcWeighted`, this owner reads). The `wood.oak` row's negative A1-A3 module is the biogenic-carbon sequestration an EPD declares (the timber stores carbon cradle-to-gate), the positive C/D modules the end-of-life release — the banded vector carries the full module sign profile, never a single positive scalar.
- [ISO_14040_DECLARED_UNIT]: the embodied-carbon GWP is a raw `kgCO2e` magnitude on the case's `MeasurementBasis` (kgCO2e per the basis unit) — the seam `Environmental` case carries a `MeasurementBasis` like `Cost`, and CO2-equivalence is a domain basis not an SI dimension, so a stage module is NEVER forced through `MeasureValue.Of`/`UnitsNet.Mass`; the seam guards it FINITE (`AllFinite`) alone. A published EPD is declared per functional unit (per-kg/per-m²/per-m³/per-item), and this catalogue stores each row AT ITS NATIVE `declared_unit` — the `SustainabilityRow.EnvironmentalBasis` token the `Lower` lowering parses to the seam `MeasurementBasis`, the `StageGwp` magnitudes kgCO2e per that unit exactly as the EPD publishes them (a per-kg steel EPD stays per-kg, a per-m³ stone EPD per-m³), with NO `× density` rewrite to one curated `PerM3` at ingress that a per-area or per-item EPD could not survive. The baked catalogue and the live `Rasm.Compute` EC3 ingress (which tags an EPD's native `declared_unit` basis from the openEPD payload) therefore land identically basis-tagged, and the RELOCATED `AggregateEnvironmental` fold scales each ply by the basis-matching element quantity through the SAME `DeclaredQuantity` owner the cost fold uses (PerKg → volume×density, PerM2 → face area, PerM3 → volume, PerItem → unit), so a baked declaration and an EC3-resolved declaration fold through one basis-aware path with no basis branch (the per-m² membrane EPD a per-m³-only normalization dropped is now first-class).
- [ASSEMBLY_AGGREGATOR_RELOCATED]: the multi-ply lifecycle folds `AggregateEnvironmental` (the per-module GWP `Σ(value_i · quantity_i)` over the plies, the cradle-to-grave total, the embodied-carbon intensity) and `AggregateCost` (the supply/install/lifecycle sum over the plies × quantities, the currency-mismatch guard) are RELOCATED to `Rasm.Compute` together with the engineering `AssemblyAggregator` — the seam carries the per-material `Environmental`/`Cost` input, Compute folds the assignment plies and writes the `Assessment.Result` node back. The prior `AssemblyLifecycle`/`AssemblyCost`/`PlyQuantity` receipts and the `AddScaled`/`AccumulateCost` kernels leave this folder entirely. Ripple counterpart: `Rasm.Compute` `[ASSEMBLY_AGGREGATOR]` (the embodied-carbon/cost rollup reading the seam `MaterialComposition` plies + the per-material `Environmental`/`Cost` inputs).
- [IFC_PSET_ENVIRONMENTAL]: the seam `Environmental` case maps to IFC 4.3 `Pset_EnvironmentalImpactValues` (`Carbon`/`WholeLifeCarbon`/`RecycledContent`/`ExpectedServiceLife`), the `Cost` case to `Pset_ConstructionCosts` (`SupplyCost`/`InstallationCost`/`LifeCycleCost` over an `IfcMonetaryUnit`), and the row's classification `(system, code)` to `IfcClassificationReference` (`ReferencedSource` the system, `Identification` the code) — associated via `IfcRelAssociatesClassification`, not a `MaterialPropertySet` case. `Rasm.Bim` reads the projected seam `Material` node's property set and emits the Psets from the seam graph — the Pset member-name mapping the `Rasm.Bim` side, the embodied-carbon/cost/classification computation this side, the seam aligning by the seam graph, never a `MaterialProperty` type crossing a boundary.
