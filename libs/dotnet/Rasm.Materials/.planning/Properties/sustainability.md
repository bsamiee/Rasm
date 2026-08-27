# [MATERIALS_SUSTAINABILITY]

THE SUSTAINABILITY-AND-UNIT-COST SOURCE. One `SustainabilityCatalogue` keys cradle-to-grave impact, unit-cost basis, and BIM classification per `MaterialId` in exact roster parity with its engineering sibling, and one `Lower` turns a published row into the contract's `Environmental` and `Cost` cases. A material is a FULL LIFECYCLE OBJECT — embodied carbon and cost basis ride as cases over one `MaterialId` and the BIM classification leaves through the `Classification` egress, never an `EcoMaterial`/`CostMaterial`/`ClassifiedMaterial` surface. The whole-building takeoff and cost rollup are `Rasm.Compute`'s; this page holds the per-material SOURCE alone. Impact values are TRANSCRIBED and unit costs are ESTIMATED, and the two never wear one provenance.

The lifecycle family is contract-owned. `Environmental` carries a `MeasurementBasis` declared unit and the FULL EN 15804+A2 `(ImpactCategory × LifecycleStage)` matrix stored row-major flat — the cradle-to-gate GWP a DERIVED read of the `(GwpTotal, A1A3)` cell, never a double-stored headline scalar — the recycled and end-of-life fractions, EPD provenance riding the case `Evidence` as `PropertyEvidence.Declaration`, and the intrinsic `IndicatorAt`/`WholeLife`/`Gwp`/`StageAt`/`WholeLifeGwp` folds; `Cost` carries the supply, install, and lifecycle per-unit columns over the contract `Currency` and `MeasurementBasis`. Classification is NOT a case but the contract's generic `Classification` `[ComplexValueObject]` the `Projection/component#COMPONENT_SUBGRAPH` `Capture` threads onto the bound element's `Object` node, which `Rasm.Bim` re-emits onto `IfcRelAssociatesClassification`. `Lower` embeds the carbon-only per-module vector into the full matrix through the contract `Environmental.CarbonMatrix` builder and passes it through the `OfEnvironmental` band gate, never a `MeasureValue`/`QuantityRow` mint — CO2e and currency are DOMAIN BASES, not SI dimensions. The per-material set so projected IS the `Environmental`/`Cost` analysis input `Rasm.Compute` reads off the `Material` node. Every lowering fault fails the contract `ElementFault.ValueRejected` (band 2500); the page re-mints NO contract type, mints NO `MaterialFault`, and admits NO `UnitsNet` quantity.

## [01]-[INDEX]

- [02]-[SUSTAINABILITY_PROPERTY]: the `SustainabilityRow` published-data ingress shape, the hoisted `EcoProfile` industry-average anchors and `Uniclass` code anchors, the `SustainabilityCatalogue` registered-row database with its roster-parity census, the `Lower` lowering into the contract `Environmental`/`Cost` cases, the `Classification` egress lifting the row's `(system, code)` to a contract `Classification` value-object, and the memoized `Lookup` the projector composes with the engineering catalogue.

## [02]-[SUSTAINABILITY_PROPERTY]

- Owner: `SustainabilityRow` the published-data ingress record; `EcoProfile` the shared industry-average anchor a family of grades references; `CostDatum` the currency/basis cost group; `SustainabilityCatalogue` the registered-row database; `Lower` the row→contract-case lowering; `Classification` the Object-node egress.
- Cases: one `SustainabilityRow` shape — the environmental columns (the per-EN-15978-module `StageGwp` carbon vector as raw centrals, or the full thirteen-indicator `Matrix` where a producer publishes one, with the recycled and end-of-life fractions), optional cost (supply/install/lifecycle over a currency and measurement basis), and optional classification (system + code); `Lower` produces a `Seq<MaterialPropertySet>` of the contract `Environmental`/`Cost` cases, each over a `MaterialId`, never a property subtype. The classification `(system, code)` is NOT lowered to a property case — it leaves through the `Classification` egress.
- Law: A ROSTER CELL NAMING A CLOSED CONTRACT ROW IS THAT ROW, NEVER ITS SPELLING. `MeasurementBasis` is a genuinely closed four-set at the contract, so `EnvironmentalBasis` and the cost basis are `MeasurementBasis` VALUES on the row and a mistyped basis is a compile error rather than a type-init lowering fault that silently drops the material from the memoized catalogue. `Currency` is the contract's declared OPEN ISO 4217 value object, so its token stays a string admitted through `key.AcceptValidated<Currency>` at the lowering edge — the one genuine token admission this page owns. The Uniclass product codes ride NAMED anchors for the same reason the eco-profiles do: one code serves a whole family, so a corrected NBS table entry is a one-line edit and a transposed digit among a hundred inline pairs is unspellable.
- Law: PROVENANCE IS PER GROUP, AND THE TWO GROUPS NEVER SHARE ONE. The impact vectors are TRANSCRIBED producer declarations carrying `PropertyEvidence.Declaration` with the EPD's own identity and expiry; the unit-cost triples are AUTHORED planning estimates carrying the `estimate` evidence class naming their basis, so a cost report can never cite a standard for a figure no standard publishes and a takeoff reading the contract evidence tells the two apart without a second column. `Ökobaudat` is the settled acquisition route for pending product declarations: the one source clearing full-matrix coverage and licence together — EN 15804+A2 with all thirteen indicators enforced at admission, `ND` marked explicitly, bulk XML and CSV, and a licence granting free redistribution of unmodified data under attribution. A carbon-first registry whose non-GWP fields are advisory cannot fill the `Matrix` column, and a licence forbidding storage forbids a catalogue outright. Admitted values carry VERBATIM per that licence, `ND` models as ABSENCE and never as zero, and a generic dataset admits discriminated by its own subtype.
- Law: FULL_ROSTER PARITY IS DERIVED, NEVER ASSERTED (folder `RULINGS [02]`). The two catalogues are hand-maintained tables over one substance vocabulary, so their symmetric difference is computed at type init and a non-empty one throws with the divergent ids named. Both directions count: an engineering id with no lifecycle row makes `Lookup` answer an empty set for a material the module believes it prices, and a lifecycle row for no substance prices a material nothing can build. A type-init census has NO caller to fail onto — the fault precedes every `Op` a result could carry — so it breaks loudly at first touch exactly as the vendor factories break at their own derivation boundary, and this is the one throw the page admits.
- Law: A COLUMN NO CONSUMER READS IS NOT A COLUMN. The row's magnitudes are RAW centrals: the contract `OfEnvironmental`/`OfCost` take doubles, so a per-column uncertainty carrier is allocated at the row constructor and unwrapped by the very next expression. The same reasoning already deleted the per-module GWP band, and it binds the two resource fractions and the three cost columns identically. A DECLARED estimate spread re-enters the day a BANDED `Environmental`/`Cost` case lands at the contract — as one column beside the vector, never as a wrap-and-discard.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row)` — ONE applicative join over four INDEPENDENT columns: the vector-XOR-matrix exclusivity gate, the `LifecycleStage.Items.Count` carbon-vector arity gate (a wrong-length vector fails at the lowering edge rather than being silently short-written by the contract `CarbonMatrix`), the environmental lowering (a carbon-only vector embedded into the full `(ImpactCategory × LifecycleStage)` matrix, or a full declaration passed straight through, landed via `OfEnvironmental` at the row's own basis), and the optional cost lowering over its parsed `Currency`. A row with a bad arity AND a bad currency faults BOTH in one `ManyErrors` — the guard ladder this replaces reported the first and hid the rest. `Lookup(id)` reads the memoized lowered catalogue and returns `Fin.Succ(empty)` for an unregistered id — lifecycle data is declared-or-absent, the asymmetric dual of the REQUIRED engineering `Lookup`. `Classification(id)` resolves the row's pair through the edition-unspecified `Classification.Of` and rides the `MaterialBinding` to the bound element's Object node.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfEnvironmental`/`OfCost`, the contract-owned `Environmental.CarbonMatrix` builder + `MatrixArity`, `LifecycleStage`/`ImpactCategory` the EN 15804+A2 matrix bands, `Currency`, `MeasurementBasis`, `PropertyEvidence.Of`/`PropertyEvidence.Declaration`, `EvidenceGrade`, the generic `Classification` + `Classification.Of`, `ElementFault.ValueRejected`, `MaterialId`), Rasm.Materials.Properties (project-local — the engineering roster the parity census reads, SAME namespace so no import), Rasm, NodaTime (`LocalDate` the EPD validity expiry), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Validation<Error,_>`), BCL inbox (`FrozenDictionary`, `Lazy<T>`, `ImmutableArray<T>`, the `double[]` ingress vector). NO `UnitsNet` (CO2e and currency are domain bases, not SI dimensions), NO `QuantityRow` (a `StageGwp` or cost magnitude is basis-relative, not a dimensioned quantity), NO `MaterialFault` (every fault is the contract `ElementFault`).
- Growth: a new EN 15804+A2 indicator is one contract `ImpactCategory` row and a new EN 15978 module one contract `LifecycleStage` row; a FULL-matrix declaration is the `Matrix` column `Lower` passes straight to `OfEnvironmental` with `CarbonMatrix` bypassed; a new declared basis is one contract `MeasurementBasis` row, a new classification code one `Uniclass` anchor, a new currency one opaque token the row supplies. A new known material is one `Rows` entry naming its `EcoProfile` anchor, its cost triple, and its code anchor. The ANCHOR is the growth axis that matters at scale: an eco-profile prices MASS PER DECLARED UNIT and therefore serves a whole family of grades, so a corrected industry figure is one anchor edit rather than a twenty-three-row sweep whose one missed row is a silent divergence, and `EcoProfile.At` is the ONE parameterized re-anchor for a family whose A1-A3 scales with a per-row quantity while every downstream module holds.
- Boundary: `SustainabilityRow` is the published-DATA ingress, NOT a parallel domain union — the contract `Environmental`/`Cost` are the one typed carriers and `Lower` the `BOUNDARY_ADMISSION`, so the row stays `internal` and `Lookup` answering the ADMITTED set is the whole public surface. Each `StageGwp` module is a raw kgCO2e-per-basis-unit magnitude declared at the row's OWN basis: a per-kg steel EPD stays `PerKg`, a per-m² membrane `PerM2`, never force-normalized to a curated `PerM3`, and `Rasm.Compute` `AggregateEnvironmental` scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold uses. A negative module is VALID biogenic-sequestration or avoided-burden carbon — the timber A1-A3 credit, the metal D credit — and the contract guards FINITE alone on matrix cells; the fractions pass raw under the contract's one `[0,1]` gate, re-minting a `UnitInterval` here diverging from the one admission owner. The contract `Environmental` case is the FULL impact MATRIX and owns its intrinsic folds, so the cradle-to-gate `Gwp` is a DERIVED `(GwpTotal, A1A3)` read and the cradle-to-grave total the `WholeLifeGwp` fold — a headline scalar column double-stores what the matrix already carries, exactly as a row-level `Epd`/`ValidUntilYear` pair double-stores the `Declaration` evidence. The lowered cases land on the contract `Material` node the projector authors and `Rasm.Bim` reads `Pset_EnvironmentalImpactValues`/`Pset_ConstructionCosts`/`IfcClassificationReference` off that graph — no Materials wire carrier, and the multi-ply rollups are `Rasm.Compute`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record CostDatum(
    string Currency,
    MeasurementBasis Basis,
    double Supply,
    double Install,
    double Lifecycle);

internal readonly record struct EcoProfile(ImmutableArray<double> Stages, string Epd, int ValidUntilYear, double Recycled, double Recovery) {
    public EcoProfile At(double a1a3, string epd) => this with { Stages = [a1a3, .. Stages.AsSpan()[1..]], Epd = epd };
}

internal sealed record SustainabilityRow(
    MeasurementBasis EnvironmentalBasis,
    ReadOnlyMemory<double> StageGwp,
    Option<ImmutableArray<double>> Matrix,
    double Recycled,
    double Recovery,
    Option<CostDatum> Cost,
    Option<(string System, string Code)> Classification,
    PropertyEvidence Evidence) {

    internal static readonly PropertyEvidence CostEstimate =
        PropertyEvidence.Of("estimate", EvidenceGrade.User, Some("regional-contractor-unit-rate"));

    public SustainabilityRow(
        MeasurementBasis environmentalBasis,
        EcoProfile profile,
        Option<(string Currency, MeasurementBasis Basis, double Supply, double Install, double Lifecycle)> cost,
        Option<(string System, string Code)> classification)
        : this(environmentalBasis, [.. profile.Stages], profile.Epd, profile.ValidUntilYear, profile.Recycled, profile.Recovery, cost, classification) { }

    public SustainabilityRow(
        MeasurementBasis environmentalBasis,
        double[] stageGwp,
        string epd,
        int validUntilYear,
        double recycledContent,
        double endOfLifeRecovery,
        Option<(string Currency, MeasurementBasis Basis, double Supply, double Install, double Lifecycle)> cost,
        Option<(string System, string Code)> classification)
        : this(
            environmentalBasis,
            stageGwp.AsMemory(),
            Option<ImmutableArray<double>>.None,
            recycledContent,
            endOfLifeRecovery,
            cost.Map(static c => new CostDatum(c.Currency, c.Basis, c.Supply, c.Install, c.Lifecycle)),
            classification,
            Declared(epd, validUntilYear)) { }

    static PropertyEvidence Declared(string epd, int validUntilYear) =>
        PropertyEvidence.Declaration("epd", epd, new LocalDate(validUntilYear, 12, 31));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SustainabilityCatalogue {
    static readonly Validation<Error, Seq<MaterialPropertySet>> NoCost = Success<Error, Seq<MaterialPropertySet>>(Seq<MaterialPropertySet>());

    static SustainabilityCatalogue() {
        string[] diverged = [
            .. Rows.Keys.Except(MaterialPropertyCatalogue.Rows.Keys)
                .Concat(MaterialPropertyCatalogue.Rows.Keys.Except(Rows.Keys))
                .Select(static id => id.ToValue())
                .Order(StringComparer.Ordinal)];
        if (diverged.Length > 0) {
            throw new InvalidOperationException($"<full-roster-divergence:{string.Join(',', diverged)}>");
        }
    }

    // --- [TABLES]
    static Option<(string System, string Code)> Uniclass(string code) => Some(("uniclass-2015", code));
    static readonly Option<(string System, string Code)> NoCode = Option<(string System, string Code)>.None;

    static readonly Option<(string System, string Code)> SteelSection = Uniclass("Pr_20_85_08_11");
    static readonly Option<(string System, string Code)> StainlessSection = Uniclass("Pr_20_85_08_83");
    static readonly Option<(string System, string Code)> AluminiumSection = Uniclass("Pr_20_85_08_02");
    static readonly Option<(string System, string Code)> ReinforcementBar = Uniclass("Pr_20_29_72");
    static readonly Option<(string System, string Code)> ConcreteMix = Uniclass("Pr_20_85_08_15");
    static readonly Option<(string System, string Code)> BlockworkUnit = Uniclass("Pr_20_93_52_01");
    static readonly Option<(string System, string Code)> ClayUnit = Uniclass("Pr_20_93_52_15");
    static readonly Option<(string System, string Code)> SilicateUnit = Uniclass("Pr_20_93_52_12");
    static readonly Option<(string System, string Code)> AacUnit = Uniclass("Pr_20_93_52_05");
    static readonly Option<(string System, string Code)> StoneSlab = Uniclass("Pr_20_93_52_56");
    static readonly Option<(string System, string Code)> SoftwoodSection = Uniclass("Pr_20_85_08_81");
    static readonly Option<(string System, string Code)> HardwoodSection = Uniclass("Pr_20_85_08_36");
    static readonly Option<(string System, string Code)> GlulamSection = Uniclass("Pr_20_85_08_33");
    static readonly Option<(string System, string Code)> PlywoodBoard = Uniclass("Pr_20_85_08_65");
    static readonly Option<(string System, string Code)> OsbBoard = Uniclass("Pr_20_85_08_67");
    static readonly Option<(string System, string Code)> GlassPane = Uniclass("Pr_25_71_33");
    static readonly Option<(string System, string Code)> InsulationProduct = Uniclass("Pr_25_71_70");
    static readonly Option<(string System, string Code)> Plasterboard = Uniclass("Pr_25_71_35_65");
    static readonly Option<(string System, string Code)> CementBoard = Uniclass("Pr_25_71_50");
    static readonly Option<(string System, string Code)> RoofMembrane = Uniclass("Pr_25_57_25");
    static readonly Option<(string System, string Code)> BreatherMembrane = Uniclass("Pr_25_57_10");
    static readonly Option<(string System, string Code)> VapourFilm = Uniclass("Pr_25_57_51");
    static readonly Option<(string System, string Code)> BitumenSheet = Uniclass("Pr_25_57_08");
    static readonly Option<(string System, string Code)> PipeProduct = Uniclass("Pr_65_52_63");
    static readonly Option<(string System, string Code)> TileFinish = Uniclass("Pr_35_93_96");
    static readonly Option<(string System, string Code)> ResilientFloor = Uniclass("Pr_35_57_71");
    static readonly Option<(string System, string Code)> CarpetFinish = Uniclass("Pr_35_57_11");
    static readonly Option<(string System, string Code)> CeilingTile = Uniclass("Pr_35_93_13");

    static readonly EcoProfile WorldSteelSection = new([1.55, 0.013, 0.020, 0.0, 0.018, -0.30], "WorldSteel-Sections-EU", 2030, 0.73, 0.90);
    static readonly EcoProfile AiscHotRolled     = new([1.22, 0.032, 0.020, 0.0, 0.018, -0.28], "AISC-HotRolled-NA", 2027, 0.93, 0.98);
    static readonly EcoProfile ArcelorRebar      = new([0.818, 0.032, 0.039, 0.0, 0.013, -0.30], "ArcelorMittal-Rebar-EU", 2027, 0.90, 0.90);
    static readonly EcoProfile CrsiRebar         = new([0.760, 0.032, 0.039, 0.0, 0.013, -0.30], "CRSI-Rebar-NA", 2028, 0.97, 0.90);
    static readonly EcoProfile SawnSoftwood      = new([-734.0, 12.4, 2.5, 0.0, 775.0, -191.0], "Sawn-Softwood-EU", 2027, 0.00, 1.00);
    static readonly EcoProfile KilnHardwood      = new([-848.0, 13.0, 3.0, 0.0, 848.0, -210.0], "Kiln-Dried-Hardwood-EU", 2027, 0.00, 1.00);
    static readonly EcoProfile HasslacherGlulam  = new([-608.0, 12.0, 2.5, 0.0, 754.0, -410.0], "HASSLACHER-Glulam", 2026, 0.00, 1.00);
    static readonly EcoProfile ReadyMixBase      = new([0.0, 2.1, 12.7, -12.8, 13.1, -9.1], "ICE-v3-EC3-ReadyMix", 2028, 0.00, 0.90);
    static readonly EcoProfile ConcreteBlock     = new([94.8, 5.0, 2.0, 0.0, 19.7, -14.7], "Belgard-ConcreteBlock", 2029, 0.10, 0.95);
    static readonly EcoProfile EuroFloatGlass    = new([0.983, 0.050, 0.030, 0.0, 0.038, -0.256], "EUROFLOAT-AGC", 2029, 0.37, 1.00);
    static readonly EcoProfile Stalatube304      = new([1.61, 0.108, 0.050, 0.0, 0.022, -0.183], "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00);
    static readonly EcoProfile Stalatube316      = new([1.83, 0.077, 0.050, 0.0, 0.022, -0.181], "Stalatube-1.4404", 2028, 0.75, 1.00);
    static readonly EcoProfile PvcPipe    = new([2.76, 0.0, 0.0, 0.0, 2.258, -0.596], "OBD-SewerPipe-PVC-d3f0a22a", 2026, 0.00, 0.00);
    static readonly EcoProfile HdgCoil    = new([2.30, 0.0, 0.0, 0.0, 0.0243, -1.602], "IBU-HDG-Coil-0899b471", 2030, 0.00, 0.99);
    static readonly EcoProfile HdgSheet   = new([2.52, 0.0, 0.0, 0.0, 0.0316, -1.767], "OBD-HDG-ColdRolledSheet-d1e98488", 2029, 0.00, 0.90);
    static readonly EcoProfile Fastener   = new([3.46, 0.0, 0.0, 0.0, 0.0050, -1.369], "OBD-GalvanizedScrews-889e2819", 2026, 0.00, 0.90);
    static readonly EcoProfile Strand     = new([2.91, 0.0, 0.0, 0.0, 0.0054, -1.496], "OBD-DrawnWire-Strand-231073e3", 2028, 0.00, 0.90);
    static readonly EcoProfile CopperTube = new([2.25, 0.0, 0.0, 0.0, 0.0333, -0.629], "OBD-CopperTubes-HME-afb0f967", 2029, 0.00, 0.95);

    internal static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row) =>
        (AdmissionSlots.Gate(
             row.Matrix.IsNone || row.StageGwp.IsEmpty,
             new ElementFault.ValueRejected("<environmental-declares-vector-and-matrix>")),
         AdmissionSlots.Gate(
             row.Matrix.IsSome || row.StageGwp.Length == LifecycleStage.Items.Count,
             new ElementFault.ValueRejected($"<stage-gwp-arity:{row.StageGwp.Length}:expected={LifecycleStage.Items.Count}>")),
         MaterialPropertySet.OfEnvironmental(
                 row.EnvironmentalBasis,
                 row.Matrix.IfNone(() => MaterialPropertySet.Environmental.CarbonMatrix(row.StageGwp)),
                 row.Recycled, row.Recovery, row.Evidence)
             .ToValidation(),
         row.Cost.Match(
             None: static () => NoCost,
             Some: c => FactoryBridge.Accept<Currency>(c.Currency)
                 .Bind(currency => MaterialPropertySet.OfCost(
                     c.Basis, currency, c.Supply, c.Install, c.Lifecycle, SustainabilityRow.CostEstimate))
                 .Map(static priced => Seq(priced))
                 .ToValidation()))
        .Apply(static (_, _, environmental, cost) => Seq(environmental) + cost).As()
        .ToFin();

    internal static readonly FrozenDictionary<MaterialId, SustainabilityRow> Rows = new (MaterialId Id, SustainabilityRow Row)[] {
        (MaterialId.Create("steel.s235"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s275"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 0.98, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s355"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 1.05, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s420"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 1.15, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s450"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 1.20, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s460"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 1.25, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.s690"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 1.55, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("metal.steel"),  new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("metal.iron"),   new(MeasurementBasis.PerKg, new[] { 0.213, 0.057, 0.058, 0.0, 0.054, -0.036 }, "Furnes-DuctileCastIron", 2030, 0.85, 0.95, Some(("USD", MeasurementBasis.PerKg, 1.20, 0.60, 0.12)), SteelSection)),
        (MaterialId.Create("iron.cast"),    new(MeasurementBasis.PerKg, new[] { 0.621, 0.0, 0.0, 0.0, 0.0, 0.0 }, "OBD-SML-CastIron-7c18fec9", 2028, 0.85, 0.95, Some(("USD", MeasurementBasis.PerKg, 2.60, 1.40, 0.15)), PipeProduct)),
        (MaterialId.Create("iron.ductile"), new(MeasurementBasis.PerKg, new[] { 1.39, 0.0, 0.0, 0.0, 0.557, 0.000138 }, "EADIPS-FGR-DuctileIron-aa089b30", 2029, 0.85, 0.95, Some(("USD", MeasurementBasis.PerKg, 2.20, 1.20, 0.15)), PipeProduct)),
        (MaterialId.Create("steel.a36"),    new(MeasurementBasis.PerKg, AiscHotRolled, Some(("USD", MeasurementBasis.PerKg, 0.90, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.a992"),   new(MeasurementBasis.PerKg, AiscHotRolled, Some(("USD", MeasurementBasis.PerKg, 1.00, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.a572"),   new(MeasurementBasis.PerKg, AiscHotRolled, Some(("USD", MeasurementBasis.PerKg, 1.00, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.g33"),           new(MeasurementBasis.PerKg, HdgSheet, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.g50"),           new(MeasurementBasis.PerKg, HdgSheet, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-4_6"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-4_8"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-5_6"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-5_8"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-6_8"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-8_8"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-10_9"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-12_9"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-gr2"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-gr5"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-gr8"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-a325"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.fastener-a490"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 0.95, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.a500"), new(MeasurementBasis.PerKg, AiscHotRolled, Some(("USD", MeasurementBasis.PerKg, 1.35, 0.60, 0.10)), SteelSection)),
        (MaterialId.Create("steel.a53"),  new(MeasurementBasis.PerKg, AiscHotRolled, Some(("USD", MeasurementBasis.PerKg, 1.30, 0.60, 0.10)), SteelSection)),
        (MaterialId.Create("steel.a653"), new(MeasurementBasis.PerKg, HdgCoil, Some(("USD", MeasurementBasis.PerKg, 1.10, 0.55, 0.10)), SteelSection)),
        (MaterialId.Create("steel.galvanized"), new(MeasurementBasis.PerKg, HdgCoil, Some(("USD", MeasurementBasis.PerKg, 1.40, 0.80, 0.10)), SteelSection)),
        (MaterialId.Create("steel.e60"),  new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.e70"),  new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 3.40, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.e80"),  new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 4.60, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.e90"),  new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 5.10, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.e100"), new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 5.80, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.e110"), new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 6.40, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.sd1"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 2.40, 0.85, 0.05)), NoCode)),
        (MaterialId.Create("steel.sd2"),   new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 2.30, 0.85, 0.05)), NoCode)),
        (MaterialId.Create("steel.sd3"),   new(MeasurementBasis.PerKg, Stalatube304, Some(("USD", MeasurementBasis.PerKg, 6.80, 0.85, 0.05)), NoCode)),
        (MaterialId.Create("steel.aws-a"), new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 2.30, 0.85, 0.05)), NoCode)),
        (MaterialId.Create("steel.aws-b"), new(MeasurementBasis.PerKg, WorldSteelSection, Some(("USD", MeasurementBasis.PerKg, 2.40, 0.85, 0.05)), NoCode)),
        (MaterialId.Create("steel.fastener-nail"),  new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 1.80, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.fastener-dowel"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 1.60, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.fastener-rivet"), new(MeasurementBasis.PerKg, Fastener, Some(("USD", MeasurementBasis.PerKg, 2.20, 0.00, 0.00)), NoCode)),
        (MaterialId.Create("steel.strand-1725"), new(MeasurementBasis.PerKg, Strand, Some(("USD", MeasurementBasis.PerKg, 1.90, 0.55, 0.06)), ReinforcementBar)),
        (MaterialId.Create("steel.strand-1860"), new(MeasurementBasis.PerKg, Strand, Some(("USD", MeasurementBasis.PerKg, 1.95, 0.55, 0.06)), ReinforcementBar)),
        (MaterialId.Create("steel.y1860s7"),     new(MeasurementBasis.PerKg, Strand, Some(("USD", MeasurementBasis.PerKg, 2.05, 0.55, 0.06)), ReinforcementBar)),
        (MaterialId.Create("adhesive.epoxy"),              new(MeasurementBasis.PerKg, new[] { 8.04, 0.0, 0.0, 0.0, 1.710, -0.496 }, "OBD-ReactiveResin-Epoxy-5916a356", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 18.0, 2.50, 0.00)), NoCode)),
        (MaterialId.Create("adhesive.methacrylate"),       new(MeasurementBasis.PerKg, new[] { 4.87, 0.0, 0.0, 0.0, 1.975, -0.617 }, "OBD-ReactiveResin-MMA-374ca550", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 22.0, 2.50, 0.00)), NoCode)),
        (MaterialId.Create("adhesive.polyurethane"),       new(MeasurementBasis.PerKg, new[] { 4.70, 0.0, 0.0, 0.0, 1.468, -0.406 }, "OBD-ReactiveResin-PU-e23fcf3f", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 14.0, 2.50, 0.00)), NoCode)),
        (MaterialId.Create("sealant.silicone-structural"), new(MeasurementBasis.PerKg, new[] { 9.55, 0.0, 0.0, 0.0, 1.521, -0.474 }, "OBD-SiliconeSealing-c5a154b1", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 26.0, 3.50, 0.40)), NoCode)),
        (MaterialId.Create("steel.1.4301"), new(MeasurementBasis.PerKg, Stalatube304, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.1.4307"), new(MeasurementBasis.PerKg, Stalatube304, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.1.4401"), new(MeasurementBasis.PerKg, Stalatube316, Some(("USD", MeasurementBasis.PerKg, 3.80, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.1.4404"), new(MeasurementBasis.PerKg, Stalatube316, Some(("USD", MeasurementBasis.PerKg, 3.80, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.1.4571"), new(MeasurementBasis.PerKg, new[] { 1.83, 0.080, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Austenitic", 2028, 0.75, 1.00, Some(("USD", MeasurementBasis.PerKg, 4.60, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.1.4462"), new(MeasurementBasis.PerKg, new[] { 3.18, 0.096, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Duplex-2205", 2028, 0.75, 1.00, Some(("USD", MeasurementBasis.PerKg, 4.20, 0.65, 0.12)), StainlessSection)),
        (MaterialId.Create("steel.b450a"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.80, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.b450c"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.80, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.b500a"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.80, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.b500b"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.80, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.b500c"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.82, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.b550b"),  new(MeasurementBasis.PerKg, ArcelorRebar, Some(("USD", MeasurementBasis.PerKg, 0.82, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr40"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.78, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr60"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.80, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr75"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.84, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr80"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.88, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr60w"),  new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.86, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.gr80w"),  new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.92, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.400w"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.84, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("steel.500w"),   new(MeasurementBasis.PerKg, CrsiRebar, Some(("USD", MeasurementBasis.PerKg, 0.90, 0.45, 0.05)), ReinforcementBar)),
        (MaterialId.Create("concrete.c12_15"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(180.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 105.0, 90.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c16_20"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(195.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 110.0, 92.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c20_25"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(210.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 115.0, 94.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c25_30"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(235.0, "Interbeton-EN206-C25"), Some(("USD", MeasurementBasis.PerM3, 120.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c30_37"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(236.0, "Interbeton-EN206-C30"), Some(("USD", MeasurementBasis.PerM3, 130.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c35_45"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(260.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 142.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c40_50"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(285.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 155.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c45_55"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(310.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 168.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c50_60"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(335.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", MeasurementBasis.PerM3, 182.0, 98.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c55_67"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(360.0, "Arup-EC-Scheme-HS"), Some(("USD", MeasurementBasis.PerM3, 198.0, 100.0, 9.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c60_75"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(380.0, "Arup-EC-Scheme-HS"), Some(("USD", MeasurementBasis.PerM3, 215.0, 100.0, 9.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c70_85"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(410.0, "Arup-EC-Scheme-HS"), Some(("USD", MeasurementBasis.PerM3, 235.0, 105.0, 9.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c80_95"),  new(MeasurementBasis.PerM3, ReadyMixBase.At(430.0, "Arup-EC-Scheme-HS"), Some(("USD", MeasurementBasis.PerM3, 255.0, 105.0, 9.0)), ConcreteMix)),
        (MaterialId.Create("concrete.c90_105"), new(MeasurementBasis.PerM3, ReadyMixBase.At(450.0, "Arup-EC-Scheme-HS"), Some(("USD", MeasurementBasis.PerM3, 280.0, 110.0, 9.0)), ConcreteMix)),
        (MaterialId.Create("concrete.lc"),      new(MeasurementBasis.PerM3, ReadyMixBase.At(220.0, "ICE-LightweightConc"), Some(("USD", MeasurementBasis.PerM3, 165.0, 95.0, 8.0)), ConcreteMix)),
        (MaterialId.Create("concrete.cmu"),     new(MeasurementBasis.PerM3, ConcreteBlock, Some(("USD", MeasurementBasis.PerM3, 150.0, 140.0, 10.0)), BlockworkUnit)),
        (MaterialId.Create("timber.c14"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 420.0, 160.0, 28.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c16"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 430.0, 160.0, 28.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c18"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 440.0, 160.0, 28.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c20"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 450.0, 160.0, 28.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c22"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 460.0, 160.0, 28.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c24"),   new(MeasurementBasis.PerM3, SawnSoftwood.At(-734.0, "Moelven-Holmen-C24"), Some(("USD", MeasurementBasis.PerM3, 470.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c27"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 490.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c30"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 510.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c35"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 540.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c40"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 580.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c45"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 620.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.c50"),   new(MeasurementBasis.PerM3, SawnSoftwood, Some(("USD", MeasurementBasis.PerM3, 660.0, 165.0, 30.0)), SoftwoodSection)),
        (MaterialId.Create("timber.d18"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 700.0, 180.0, 35.0)), HardwoodSection)),
        (MaterialId.Create("timber.d24"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 760.0, 180.0, 35.0)), HardwoodSection)),
        (MaterialId.Create("timber.d27"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 800.0, 182.0, 36.0)), HardwoodSection)),
        (MaterialId.Create("timber.d30"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 850.0, 185.0, 38.0)), HardwoodSection)),
        (MaterialId.Create("timber.d35"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 940.0, 185.0, 38.0)), HardwoodSection)),
        (MaterialId.Create("timber.d40"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1050.0, 190.0, 40.0)), HardwoodSection)),
        (MaterialId.Create("timber.d45"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1120.0, 192.0, 41.0)), HardwoodSection)),
        (MaterialId.Create("timber.d50"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1200.0, 195.0, 42.0)), HardwoodSection)),
        (MaterialId.Create("timber.d55"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1300.0, 198.0, 43.0)), HardwoodSection)),
        (MaterialId.Create("timber.d60"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1400.0, 200.0, 45.0)), HardwoodSection)),
        (MaterialId.Create("timber.d65"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1520.0, 205.0, 46.0)), HardwoodSection)),
        (MaterialId.Create("timber.d70"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1650.0, 210.0, 48.0)), HardwoodSection)),
        (MaterialId.Create("timber.d75"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1780.0, 215.0, 50.0)), HardwoodSection)),
        (MaterialId.Create("timber.d80"),   new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1920.0, 220.0, 52.0)), HardwoodSection)),
        (MaterialId.Create("wood.oak"),     new(MeasurementBasis.PerM3, KilnHardwood, Some(("USD", MeasurementBasis.PerM3, 1200.0, 280.0, 60.0)), HardwoodSection)),
        (MaterialId.Create("timber.gl20h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 820.0, 235.0, 44.0)), GlulamSection)),
        (MaterialId.Create("timber.gl20c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 790.0, 235.0, 44.0)), GlulamSection)),
        (MaterialId.Create("timber.gl22h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 850.0, 238.0, 45.0)), GlulamSection)),
        (MaterialId.Create("timber.gl22c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 815.0, 238.0, 45.0)), GlulamSection)),
        (MaterialId.Create("timber.gl24h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 880.0, 240.0, 45.0)), GlulamSection)),
        (MaterialId.Create("timber.gl24c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 840.0, 240.0, 45.0)), GlulamSection)),
        (MaterialId.Create("timber.gl26h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 900.0, 245.0, 46.0)), GlulamSection)),
        (MaterialId.Create("timber.gl26c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 860.0, 245.0, 46.0)), GlulamSection)),
        (MaterialId.Create("timber.gl28h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 920.0, 250.0, 47.0)), GlulamSection)),
        (MaterialId.Create("timber.gl28c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 880.0, 250.0, 47.0)), GlulamSection)),
        (MaterialId.Create("timber.gl30h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 960.0, 255.0, 48.0)), GlulamSection)),
        (MaterialId.Create("timber.gl30c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 920.0, 255.0, 48.0)), GlulamSection)),
        (MaterialId.Create("timber.gl32h"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 1000.0, 260.0, 50.0)), GlulamSection)),
        (MaterialId.Create("timber.gl32c"), new(MeasurementBasis.PerM3, HasslacherGlulam, Some(("USD", MeasurementBasis.PerM3, 960.0, 260.0, 50.0)), GlulamSection)),
        (MaterialId.Create("aluminium.6082t6"), new(MeasurementBasis.PerKg, new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6082", 2027, 0.50, 0.95, Some(("USD", MeasurementBasis.PerKg, 3.40, 0.90, 0.20)), AluminiumSection)),
        (MaterialId.Create("aluminium.6061t6"), new(MeasurementBasis.PerKg, new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6061", 2027, 0.50, 0.95, Some(("USD", MeasurementBasis.PerKg, 3.40, 0.90, 0.20)), AluminiumSection)),
        (MaterialId.Create("aluminium.1350"),   new(MeasurementBasis.PerKg, new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Rod-1350",     2027, 0.50, 0.95, Some(("USD", MeasurementBasis.PerKg, 3.40, 0.90, 0.20)), AluminiumSection)),
        (MaterialId.Create("aluminium.6063t5"), new(MeasurementBasis.PerKg, new[] { 5.50, 0.050, 0.050, 0.0, 0.033, -1.70 }, "Pandolfo-Al-6063", 2027, 0.50, 0.95, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.90, 0.20)), AluminiumSection)),
        (MaterialId.Create("aluminium.6063t6"), new(MeasurementBasis.PerKg, new[] { 5.50, 0.050, 0.050, 0.0, 0.033, -1.70 }, "Pandolfo-Al-6063", 2027, 0.50, 0.95, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.90, 0.20)), AluminiumSection)),
        (MaterialId.Create("aluminium.5083"),   new(MeasurementBasis.PerKg, new[] { 8.50, 0.050, 0.050, 0.0, 0.050, -4.50 }, "EU-Al-Plate-5083", 2027, 0.35, 0.95, Some(("USD", MeasurementBasis.PerKg, 4.10, 0.95, 0.22)), AluminiumSection)),
        (MaterialId.Create("copper.c12200"), new(MeasurementBasis.PerKg, CopperTube, Some(("USD", MeasurementBasis.PerKg, 11.0, 4.00, 0.30)), PipeProduct)),
        (MaterialId.Create("masonry.clay"),  new(MeasurementBasis.PerM3, new[] { 320.0, 25.6, 2.6, 0.0, 25.6, -16.4 }, "Wienerberger-ClayBrick", 2030, 0.00, 0.90, Some(("USD", MeasurementBasis.PerM3, 280.0, 220.0, 15.0)), ClayUnit)),
        (MaterialId.Create("masonry.calciumsilicate"), new(MeasurementBasis.PerM3, new[] { 221.0, 13.6, 6.05, -94.5, 49.16, -7.98 }, "Xella-Silka-CS", 2031, 0.00, 0.90, Some(("USD", MeasurementBasis.PerM3, 240.0, 200.0, 14.0)), SilicateUnit)),
        (MaterialId.Create("masonry.aac"),   new(MeasurementBasis.PerM3, new[] { 160.0, 0.078, 1.0, -36.4, 9.31, -1.03 }, "Xella-Ytong-AAC", 2030, 0.00, 0.90, Some(("USD", MeasurementBasis.PerM3, 190.0, 160.0, 12.0)), AacUnit)),
        (MaterialId.Create("masonry.aggregate"), new(MeasurementBasis.PerM3, ConcreteBlock, Some(("USD", MeasurementBasis.PerM3, 160.0, 150.0, 11.0)), BlockworkUnit)),
        (MaterialId.Create("stone.marble"),  new(MeasurementBasis.PerM3, new[] { 500.0, 60.0, 30.0, 0.0, 60.0, -17.5 }, "EU-Marble-Slab", 2030, 0.00, 1.00, Some(("USD", MeasurementBasis.PerM3, 950.0, 320.0, 40.0)), StoneSlab)),
        (MaterialId.Create("stone.granite"), new(MeasurementBasis.PerM3, new[] { 95.0, 59.4, 96.7, 0.0, 41.4, -4.2 }, "IST-Granite-Slab", 2030, 0.00, 0.90, Some(("USD", MeasurementBasis.PerM3, 880.0, 300.0, 38.0)), StoneSlab)),
        (MaterialId.Create("glass.float"),  new(MeasurementBasis.PerKg, EuroFloatGlass, Some(("USD", MeasurementBasis.PerKg, 1.80, 0.70, 0.10)), GlassPane)),
        (MaterialId.Create("glass.crown"),  new(MeasurementBasis.PerKg, EuroFloatGlass, Some(("USD", MeasurementBasis.PerKg, 1.85, 0.70, 0.10)), GlassPane)),
        (MaterialId.Create("glass.flint"),  new(MeasurementBasis.PerKg, new[] { 1.74, 0.050, 0.030, 0.0, 0.038, -0.20 }, "SCHOTT-Borosilicate", 2029, 0.30, 1.00, Some(("USD", MeasurementBasis.PerKg, 4.50, 0.90, 0.12)), GlassPane)),
        (MaterialId.Create("insulation.glasswool"), new(MeasurementBasis.PerKg, new[] { 1.30, 0.10, 0.12, 0.0, 0.19, -0.04 }, "Knauf-GlassWool", 2029, 0.30, 0.00, Some(("USD", MeasurementBasis.PerKg, 1.15, 0.60, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.stonewool"), new(MeasurementBasis.PerKg, new[] { 1.40, 0.10, 0.12, 0.0, 0.19, -0.04 }, "MineralWool-EU", 2029, 0.25, 0.00, Some(("USD", MeasurementBasis.PerKg, 1.25, 0.60, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.eps"),  new(MeasurementBasis.PerKg, new[] { 2.23, 0.028, 0.002, 0.0, 2.79, -0.57 }, "BEWI-EPS-80", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 2.10, 0.55, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.xps"),  new(MeasurementBasis.PerKg, new[] { 3.30, 0.030, 0.010, 0.0, 2.80, -0.50 }, "XPS-Foam-EU", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 2.60, 0.55, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.pir"),  new(MeasurementBasis.PerKg, new[] { 2.68, 0.085, 0.278, 0.0, 2.34, -0.65 }, "UNILIN-PIR", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 3.10, 0.55, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.pur"),  new(MeasurementBasis.PerKg, new[] { 3.40, 0.080, 0.200, 0.0, 2.40, -0.60 }, "PUR-Foam-EU", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 3.20, 0.55, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.phenolic"), new(MeasurementBasis.PerKg, new[] { 2.42, 0.050, 0.100, 0.0, 2.60, -0.85 }, "Kingspan-Kooltherm-Phenolic", 2030, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 3.40, 0.55, 0.05)), InsulationProduct)),
        (MaterialId.Create("insulation.woodfibre"), new(MeasurementBasis.PerKg, new[] { -1.20, 0.080, 0.050, 0.0, 1.60, -0.20 }, "WoodFibre-EU", 2029, 0.00, 1.00, Some(("USD", MeasurementBasis.PerKg, 2.40, 0.60, 0.05)), InsulationProduct)),
        (MaterialId.Create("gypsum.board"), new(MeasurementBasis.PerKg, new[] { 0.226, 0.0162, 0.0238, 0.0, 0.0162, -0.0187 }, "Knauf-White-Plasterboard", 2030, 0.10, 0.10, Some(("USD", MeasurementBasis.PerKg, 0.45, 0.50, 0.05)), Plasterboard)),
        (MaterialId.Create("cement.board"), new(MeasurementBasis.PerM3, new[] { 350.0, 15.0, 10.0, 0.0, 15.0, -5.0 }, "Etex-FibreCement-Board", 2029, 0.05, 0.10, Some(("USD", MeasurementBasis.PerM3, 480.0, 260.0, 18.0)), CementBoard)),
        (MaterialId.Create("wood.plywood"), new(MeasurementBasis.PerM3, new[] { -800.0, 13.0, 3.0, 0.0, 810.0, -200.0 }, "Softwood-Plywood-EU", 2027, 0.00, 1.00, Some(("USD", MeasurementBasis.PerM3, 560.0, 175.0, 32.0)), PlywoodBoard)),
        (MaterialId.Create("wood.osb"),     new(MeasurementBasis.PerM3, new[] { -982.0, 21.6, 3.0, 0.0, 1193.0, -210.0 }, "CORRIM-OSB-NA", 2027, 0.00, 1.00, Some(("USD", MeasurementBasis.PerM3, 430.0, 170.0, 30.0)), OsbBoard)),
        (MaterialId.Create("membrane.epdm"), new(MeasurementBasis.PerM2, new[] { 5.98, 0.0467, 0.487, 0.0, 5.781, -4.56 }, "alwitra-EVALASTIC-EPDM", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 14.0, 9.0, 1.5)), RoofMembrane)),
        (MaterialId.Create("membrane.pvc"),  new(MeasurementBasis.PerM2, new[] { 6.50, 0.050, 0.400, 0.0, 5.50, -1.00 }, "MRPI-Flagon-PVC", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 12.0, 9.0, 1.5)), RoofMembrane)),
        (MaterialId.Create("membrane.tpo"),  new(MeasurementBasis.PerM2, new[] { 3.69, 0.16, 0.0, 0.0, 0.018, 0.0 }, "Elevate-UltraPly-TPO-EPD770", 2030, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 11.0, 9.0, 1.5)), RoofMembrane)),
        (MaterialId.Create("membrane.wrap"), new(MeasurementBasis.PerM2, new[] { 0.281, 0.0060, 0.0018, 0.0, 0.182, 0.0 }, "IBU-Tyvek-Monolayer60-b38b2ec7", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 1.50, 1.00, 0.10)), BreatherMembrane)),
        (MaterialId.Create("membrane.pe"),   new(MeasurementBasis.PerM2, new[] { 0.450, 0.0, 0.0, 0.0, 0.556, -0.245 }, "OBD-DampInsulationPE-6869f7c1", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 0.60, 0.80, 0.05)), VapourFilm)),
        (MaterialId.Create("membrane.sbs"),  new(MeasurementBasis.PerM2, new[] { 5.796, 0.0, 0.0, 0.0, 0.209, 0.0 }, "OBD-BitumenPYE-PV200S5-c984526a", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 7.00, 10.0, 1.0)), BitumenSheet)),
        (MaterialId.Create("pipe.pvc"),  new(MeasurementBasis.PerKg, PvcPipe, Some(("USD", MeasurementBasis.PerKg, 2.20, 1.50, 0.10)), PipeProduct)),
        (MaterialId.Create("pipe.cpvc"), new(MeasurementBasis.PerKg, PvcPipe, Some(("USD", MeasurementBasis.PerKg, 3.50, 1.80, 0.10)), PipeProduct)),
        (MaterialId.Create("pipe.pex"),  new(MeasurementBasis.PerKg, new[] { 2.93, 0.0, 0.0, 0.0, 3.769, -1.398 }, "OBD-PEX-DrinkingWater-eb2f1734", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 4.00, 1.80, 0.10)), PipeProduct)),
        (MaterialId.Create("pipe.hdpe"), new(MeasurementBasis.PerKg, new[] { 2.32, 0.0, 0.0, 0.0, 3.455, -1.524 }, "OBD-SewerPipe-PEHD-db7d83d8", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 2.50, 1.50, 0.10)), PipeProduct)),
        (MaterialId.Create("ceramic.tile"),       new(MeasurementBasis.PerM2, new[] { 19.55, 0.0, 0.0, 0.0, 0.254, -0.029 }, "OBD-Stoneware-Glazed-5618689c", 2026, 0.00, 0.90, Some(("USD", MeasurementBasis.PerM2, 35.0, 60.0, 2.0)), TileFinish)),
        (MaterialId.Create("flooring.resilient"), new(MeasurementBasis.PerM2, new[] { 8.185, 0.0, 0.0, 0.0, 8.127, -1.805 }, "OBD-PVCFloor-fde18fdc", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 30.0, 15.0, 2.0)), ResilientFloor)),
        (MaterialId.Create("flooring.carpet"),    new(MeasurementBasis.PerM2, new[] { 16.04, 0.0, 0.0, 0.0, 7.364, -1.965 }, "OBD-CarpetTile-PA6-2fbd1f22", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 28.0, 8.0, 3.0)), CarpetFinish)),
        (MaterialId.Create("ceiling.mineral"),    new(MeasurementBasis.PerM2, new[] { 15.84, 0.0, 0.0, 0.0, 1.098, 0.0 }, "Zentia-Bioguard-ANF-1d522341", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 12.0, 10.0, 2.0)), CeilingTile)),
        (MaterialId.Create("coating.paint"),      new(MeasurementBasis.PerKg, new[] { 2.154, 0.0, 0.0116, 0.0, 0.0209, -0.004 }, "OBD-DispersionPaint-8c5e949d", 2026, 0.00, 0.00, Some(("USD", MeasurementBasis.PerM2, 3.00, 12.0, 1.5)), NoCode)),
        (MaterialId.Create("fireproofing.sfrm"),        new(MeasurementBasis.PerKg, new[] { 0.42, 0.0, 0.0, 0.0, 0.0, 0.0 }, "Isolatek-CAFCO300-SmartEPD", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 1.20, 2.00, 0.10)), NoCode)),
        (MaterialId.Create("fireproofing.intumescent"), new(MeasurementBasis.PerKg, new[] { 2.16, 0.0, 0.0, 0.0, 1.304, -0.293 }, "IBU-Hensotherm-410KS-RHG20240229", 2029, 0.00, 0.00, Some(("USD", MeasurementBasis.PerKg, 8.00, 4.00, 0.20)), NoCode)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    static readonly Lazy<FrozenDictionary<MaterialId, Seq<MaterialPropertySet>>> Lowered =
        new(static () => Rows
                .Select(static entry => (entry.Key, Sets: Lower(entry.Value, LowerKey)))
                .Where(static entry => entry.Sets.IsSucc)
                .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Sets.ThrowIfFail()),
            LazyThreadSafetyMode.ExecutionAndPublication);

    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id) =>
        Lowered.Value.TryGetValue(id, out Seq<MaterialPropertySet> lowered)
            ? Fin.Succ(lowered)
            : Rows.TryGetValue(id, out SustainabilityRow? row)
                ? Lower(row!)
                : Fin.Succ(Seq<MaterialPropertySet>());

    public static Fin<Option<Classification>> Classification(MaterialId id) =>
        Rows.TryGetValue(id, out SustainabilityRow? row)
            ? row!.Classification
                .TraverseM(c => global::Rasm.Element.Classification.Classification.Of(c.System, c.Code))
                .As()
            : Fin.Succ(Option<Classification>.None);
}
```

## [03]-[RESEARCH]

(none)
