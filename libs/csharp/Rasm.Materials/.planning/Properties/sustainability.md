# [MATERIALS_SUSTAINABILITY]

THE SUSTAINABILITY-AND-UNIT-COST SOURCE. This owner holds the estate's lifecycle data: one `SustainabilityCatalogue` keying cradle-to-grave impact, unit-cost basis, and BIM classification per `MaterialId` in exact roster parity with its engineering sibling, and one `Lower` lowering that turns a published row into the seam's `Environmental` and `Cost` cases. A material is a FULL LIFECYCLE OBJECT — embodied carbon and cost basis ride as cases over one `MaterialId` and the BIM classification leaves as the `Classification` egress, never an `EcoMaterial`/`CostMaterial`/`ClassifiedMaterial` surface. The boundary is exact: the whole-building takeoff and cost rollup are `Rasm.Compute`'s, this page holding the per-material SOURCE alone. Impact values are TRANSCRIBED and unit costs are ESTIMATED, and the two never wear one provenance.

The lifecycle family is seam-owned: `Environmental` carries a `MeasurementBasis` declared unit plus the FULL EN 15804+A2 `(ImpactCategory × LifecycleStage)` matrix stored row-major flat — the cradle-to-gate GWP a DERIVED read of the `(GwpTotal, A1A3)` cell, never a double-stored headline scalar — the recycled and end-of-life fractions, EPD provenance riding the case `Evidence` as `PropertyEvidence.Declaration`, and the intrinsic `IndicatorAt`/`WholeLife`/`Gwp`/`StageAt`/`WholeLifeGwp` folds; `Cost` carries the supply, install, and lifecycle per-unit columns over the seam `Currency` and `MeasurementBasis`. Classification is NOT a case but the seam's generic `Classification` `[ComplexValueObject]` the `Projection/component#COMPONENT_SUBGRAPH` `Capture` threads onto the bound element's `Object` node, which `Rasm.Bim` re-emits onto `IfcRelAssociatesClassification`. This page COMPOSES the shared `Published<T>` ingress carrier `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` DECLARES, over its `IUncertainty<double>` arm; `Lower` embeds the carbon-only per-module vector into the full matrix through the seam `Environmental.CarbonMatrix` builder and passes it through the `OfEnvironmental` band gate, never a `MeasureValue`/`QuantityRow` mint, CO2e and currency being DOMAIN BASES rather than SI dimensions. The per-material set so projected IS the `Environmental`/`Cost` analysis input `Rasm.Compute` reads off the `Material` node. Every lowering fault rails the seam `ElementFault.ValueRejected` (band 2500); the page re-mints NO seam type, mints NO `MaterialFault`, and admits NO `UnitsNet` quantity.

## [01]-[INDEX]

- [02]-[SUSTAINABILITY_PROPERTY]: the `SustainabilityRow` published-data ingress shape composing the shared `Published<double>` carrier, the hoisted `EcoProfile` industry-average anchors, the `SustainabilityCatalogue` registered-row database with its roster-parity census, the `Lower` lowering into the seam `Environmental`/`Cost` cases, the `Classification` egress lifting the row's `(system, code)` to a seam `Classification` value-object, and the memoized `Lookup` the projector composes with the engineering catalogue.

## [02]-[SUSTAINABILITY_PROPERTY]

- Owner: `SustainabilityRow` the published-data ingress record over the shared `Published<double>` carrier (declared by `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`, composed here); `EcoProfile` the shared industry-average anchor a family of grades references; `CostDatum` the currency/basis cost group; `SustainabilityCatalogue` the registered-row database; `Lower` the row→seam-case lowering; `Classification` the Object-node egress.
- Cases: one `SustainabilityRow` shape — the environmental columns (the per-EN-15978-module `StageGwp` carbon vector as raw centrals, or the full thirteen-indicator `Matrix` where a producer publishes one, plus the recycled and end-of-life fractions), optional cost (supply/install/lifecycle over a currency and measurement basis), and optional classification (system + code); `Lower` produces a `Seq<MaterialPropertySet>` of the seam `Environmental`/`Cost` cases, each over a `MaterialId`, never a property subtype. The classification `(system, code)` is NOT lowered to a property case — it leaves through the `Classification` egress.
- Law: PROVENANCE IS PER COLUMN. The impact vectors are TRANSCRIBED producer declarations and carry `PropertyEvidence.Declaration` with the EPD's own identity and expiry; the unit-cost triples are AUTHORED planning estimates and carry the `estimate` evidence class naming their basis, so a cost report can never cite a standard for a figure no standard publishes and a takeoff reading the seam evidence tells the two apart without a second column. `Ökobaudat` is the settled acquisition route for the pending product declarations: it is the one source clearing full-matrix coverage and licence together — EN 15804+A2 with all thirteen indicators enforced at admission, `ND` marked explicitly, bulk XML and CSV, and a licence granting free redistribution of unmodified data under attribution. A carbon-first registry whose non-GWP fields are advisory cannot fill the `Matrix` column, and a licence forbidding storage forbids a catalogue outright, which is what a catalogue is. Admitted values carry VERBATIM per that licence, `ND` models as ABSENCE and never as zero, and a generic dataset admits discriminated by its own subtype.
- Law: FULL_ROSTER PARITY IS DERIVED, NEVER ASSERTED. The two catalogues are hand-maintained tables over one substance vocabulary, so their symmetric difference is computed at type init and a non-empty one throws with the divergent ids named. Both directions count: an engineering id with no lifecycle row makes `Lookup` answer an empty set for a material the estate believes it prices, and a lifecycle row for no substance prices a material nothing can build. There is no caller to rail a curation defect onto, so it breaks loudly at first touch exactly as the vendor factories break at their own derivation boundary.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key)` — refuses a row declaring BOTH the carbon vector and a full matrix, GUARDS a carbon vector to the seam `LifecycleStage.Count` arity (a wrong-length vector rails at the lowering edge rather than being silently short-written by the seam `CarbonMatrix`), parses the native `EnvironmentalBasis` through `MeasurementBasis.Parse`, embeds a carbon-only vector into the full `(ImpactCategory × LifecycleStage)` matrix or passes a full declaration straight through, and lands it via `OfEnvironmental` at that basis; the optional cost parses its independent currency and basis tokens applicatively then binds `OfCost`. The two groups are INDEPENDENT and ACCUMULATE, so a bad `declared_unit` and a bad currency fault together in one `ManyErrors`. `Lookup(id, key)` reads the memoized lowered catalogue and returns `Fin.Succ(empty)` for an unregistered id — lifecycle data is declared-or-absent, the asymmetric dual of the REQUIRED engineering `Lookup`. `Classification(id, key)` resolves the row's pair through the edition-unspecified `Classification.Of` and rides the `MaterialBinding` to the bound element's Object node.
- Packages: Rasm.Element (project — `MaterialPropertySet.OfEnvironmental`/`OfCost`, the seam-owned `Environmental.CarbonMatrix` builder + `MatrixArity`, `LifecycleStage`/`ImpactCategory` the EN 15804+A2 matrix bands, `Currency`/`MeasurementBasis`, `PropertyEvidence`/`PropertyEvidence.Declaration`, the generic `Classification` + `Classification.Of`, `ElementFault.ValueRejected`, `MaterialId`), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of` and the engineering roster the parity census reads, SAME namespace so no import), Rasm (project — `Op`), NodaTime (`LocalDate` the EPD validity expiry), LanguageExt.Core (`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`, `Lazy<T>`, `ImmutableArray<T>`, the `double[]` ingress vector). NO `UnitsNet` (CO2e and currency are domain bases, not SI dimensions), NO `QuantityRow` (a `StageGwp` or cost magnitude is basis-relative, not a dimensioned quantity), NO `MaterialFault` (every fault is the seam `ElementFault`).
- Growth: a new EN 15804+A2 indicator is one seam `ImpactCategory` row and a new EN 15978 module one seam `LifecycleStage` row; a FULL-matrix declaration is the `Matrix` column `Lower` passes straight to `OfEnvironmental` with `CarbonMatrix` bypassed; a new currency, classification system, or declared basis is one opaque token the row supplies. A new known material is one `Rows` entry naming its `EcoProfile` anchor plus its own cost triple and classification pair. The ANCHOR is the growth axis that matters at scale: an eco-profile prices MASS PER DECLARED UNIT and therefore serves a whole family of grades, so a corrected industry figure is one anchor edit rather than a twenty-three-row sweep whose one missed row is a silent divergence, and `EcoProfile.At` is the ONE parameterized re-anchor for a family whose A1-A3 scales with a per-row quantity — a concrete class with its cement content, a grade whose EPD names a specific producer — while every downstream module holds.
- Boundary: `SustainabilityRow` is the published-DATA ingress, NOT a parallel domain union — the seam `Environmental`/`Cost` are the one typed carriers and `Lower` the `BOUNDARY_ADMISSION`, so the row stays `internal` and `Lookup` answering the ADMITTED set is the whole public surface. Each `StageGwp` module is a raw kgCO2e-per-basis-unit magnitude declared at the row's OWN `EnvironmentalBasis`: a per-kg steel EPD stays `PerKg`, a per-m² membrane `PerM2`, never force-normalized to a curated `PerM3`, and `Rasm.Compute` `AggregateEnvironmental` scales each ply by the basis-matching element quantity through the SAME basis-aware `DeclaredQuantity` derivation the cost fold uses. A negative module is VALID biogenic-sequestration or avoided-burden carbon — the timber A1-A3 credit, the metal D credit — and the seam guards FINITE alone on matrix cells; the fractions pass raw under the seam's one `[0,1]` gate, re-minting a `UnitInterval` here diverging from the one admission owner. The seam `Environmental` case is the FULL impact MATRIX and owns its own intrinsic folds, so the cradle-to-gate `Gwp` is a DERIVED `(GwpTotal, A1A3)` read and the cradle-to-grave total the `WholeLifeGwp` fold — a headline scalar column would double-store what the matrix already carries, exactly as a row-level `Epd`/`ValidUntilYear` pair would double-store the `Declaration` evidence. The lowered cases land on the seam `Material` node the projector authors and `Rasm.Bim` reads `Pset_EnvironmentalImpactValues`/`Pset_ConstructionCosts`/`IfcClassificationReference` off that graph — no Materials wire carrier, and the multi-ply rollups are `Rasm.Compute`'s.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Threading;              // LazyThreadSafetyMode — the lowered-catalogue publication mode
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation slot the applicative environmental×cost join accumulates
using NodaTime;                      // LocalDate — the EPD validity expiry PropertyEvidence.Declaration carries
using Rasm.Domain;                   // Op
using Rasm.Element.Classification;   // Classification — the seam value-object the classification column resolves to
using Rasm.Element.Composition;      // MaterialId, MaterialPropertySet (Environmental|Cost), Environmental.CarbonMatrix,
using Rasm.Element.Projection;       // LifecycleStage, ImpactCategory, Currency, MeasurementBasis, PropertyEvidence (Composition); FaultBand (Projection)
                                     // ElementFault (the seam value-admission band 2500 every lowering fault lifts on)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // beside MaterialPropertyCatalogue — the shared Published<T> carrier is namespace-local

// --- [MODELS] ------------------------------------------------------------------------------
// The unit-cost group. Its three columns are the ONE part of a row nothing published: a regional contractor rate
// is a genuine design input a takeoff consumes, and it is not a transcription, so each column rides the estimate
// evidence class rather than the producer Declaration the impact vector carries. The relative spread is then the
// ESTIMATE's own declared confidence — an authored planning band that says so — instead of a transcription band
// implying a source that could be checked.
internal sealed record CostDatum(
    string Currency,
    string Basis,
    Published<double> Supply,
    Published<double> Install,
    Published<double> Lifecycle);

// The shared industry-average eco-profile: the six-module GWP vector, the EPD identity and validity year the
// Declaration evidence carries, and the recycled/recovery pair. GWP TRACKS MASS PER DECLARED UNIT, so every grade
// whose mass one profile prices REFERENCES this row rather than re-spelling the vector — the WorldSteel section
// profile alone was inline on twenty-three rows, where one corrected industry figure meant twenty-three edits and any
// missed row a silent divergence. Stage(a1a3) is the ONE parameterized read for a family whose A1-A3 scales with a
// per-row quantity (a concrete class with its cement content) while every downstream module holds.
// Stages is an ImmutableArray: a shared anchor referenced by a whole family of grades handed every reference a
// mutable array whose elements any holder could rewrite, and one such write re-prices every grade pointing at
// the row — the exact silent divergence the shared anchor exists to prevent. The immutable carrier also gives
// the At re-anchoring a value-typed copy rather than an aliasing slice.
internal readonly record struct EcoProfile(ImmutableArray<double> Stages, string Epd, int ValidUntilYear, double Recycled, double Recovery) {
    // The ONE parameterized read: a family whose A1-A3 scales with a per-row quantity (a concrete class with its
    // cement content) or whose EPD names a specific producer keeps every downstream module of the shared profile.
    public EcoProfile At(double a1a3, string epd) => this with { Stages = [a1a3, .. Stages.AsSpan()[1..]], Epd = epd };
}

// INTERNAL for the same reason its engineering peer is: this is published DATA before admission, and Lower is
// where a raw kgCO2e magnitude becomes a seam-gated matrix cell.
//
// The two environmental columns are MUTUALLY EXCLUSIVE and neither subsumes the other. The carbon-only vector is
// the PARTIAL-EPD shape, and widening it to thirteen indicator rows would force every carbon-only row to declare
// twelve zeroes it never measured — absence stated as measurement, which is the one defect the whole coverage
// discipline exists to prevent. A row carrying both would declare its GWP row twice and no rule picks a winner
// that is not a guess about which the producer meant, so Lower refuses the pair rather than resolving it.
internal sealed record SustainabilityRow(
    string EnvironmentalBasis,       // the EPD's native declared_unit token parsed to the seam MeasurementBasis
    ReadOnlyMemory<double> StageGwp, // RAW per-module centrals beside the row-level Evidence — the partial-EPD carbon-only shape
    Option<ImmutableArray<double>> Matrix, // the FULL thirteen-indicator declaration, when a producer publishes one
    Published<double> Recycled,
    Published<double> Recovery,
    Option<CostDatum> Cost,
    Option<(string System, string Code)> Classification,
    PropertyEvidence Evidence) {

    // The confidence profile (POLICY_VALUES): declared resource fractions ±10%, and the three ESTIMATE spreads a
    // planning unit rate carries — supply the tightest, install and lifecycle wider because both track labour and
    // duration a rate sheet cannot fix. There is NO module-GWP band: the seam's Environmental case takes the flat
    // impact MATRIX and re-guards arity then finiteness itself, so a per-module band was allocated at the row ctor
    // and unwrapped by the very next expression. It re-enters the day a BANDED Environmental case lands on the
    // seam, as one column beside the vector rather than a wrap-and-discard.
    const double FractionConfidence = 0.10;
    const double SupplyEstimateSpread = 0.20;
    const double InstallEstimateSpread = 0.25;
    const double LifecycleEstimateSpread = 0.25;

    // The ESTIMATE provenance class. The seam's evidence Source column is the provenance axis every Properties
    // owner already keys on — "vendor" for a standards table, "epd" for a producer declaration, a modality key for
    // an assessment — so an authored planning figure joins that vocabulary as its own class and needs no parallel
    // column. It carries no expiry: an estimate does not lapse, it is superseded.
    static readonly PropertyEvidence CostEstimate =
        new PropertyEvidence("estimate", "regional-contractor-unit-rate", Option<LocalDate>.None).Normalized();

    // The ROW ctor: a row spells its basis, its shared eco-profile anchor, and the two columns that genuinely vary
    // per grade — its own cost triple and its classification pair.
    public SustainabilityRow(
        string environmentalBasis,
        EcoProfile profile,
        Option<(string Currency, string Basis, double Supply, double Install, double Lifecycle)> cost,
        Option<(string System, string Code)> classification)
        : this(environmentalBasis, [.. profile.Stages], profile.Epd, profile.ValidUntilYear, profile.Recycled, profile.Recovery, cost, classification) { }

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
            stageGwp.AsMemory(),
            Option<ImmutableArray<double>>.None,
            Published.Of(recycledContent, FractionConfidence, Declared(epd, validUntilYear)),
            Published.Of(endOfLifeRecovery, FractionConfidence, Declared(epd, validUntilYear)),
            cost.Map(c => new CostDatum(
                c.Currency,
                c.Basis,
                Published.Of(c.Supply, SupplyEstimateSpread, CostEstimate),
                Published.Of(c.Install, InstallEstimateSpread, CostEstimate),
                Published.Of(c.Lifecycle, LifecycleEstimateSpread, CostEstimate))),
            classification,
            Declared(epd, validUntilYear)) { }

    static PropertyEvidence Declared(string epd, int validUntilYear) =>
        PropertyEvidence.Declaration("epd", epd, new LocalDate(validUntilYear, 12, 31));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SustainabilityCatalogue {
    // The empty cost slot a cost-less row contributes to the applicative join.
    static readonly Validation<Error, Seq<MaterialPropertySet>> NoCost = Success<Error, Seq<MaterialPropertySet>>(Seq<MaterialPropertySet>());

    // The memo fold's own op: the once-per-process lowering is a CATALOGUE event with no caller behind it.
    static readonly Op LowerKey = Op.Of(name: "sustainability-catalogue-lower");

    // FULL_ROSTER PARITY, PROVED. Two hand-maintained tables over one substance vocabulary drift silently, and the
    // drift is INVISIBLE at the seam: an engineering id with no lifecycle row answers an empty set that reads as
    // "this material declares nothing" rather than "the roster lost a row". The symmetric difference therefore runs
    // at type init, where the explicit static constructor also pins initialization order against beforefieldinit —
    // a curation defect has no caller to rail onto and must break at first touch, naming every divergent id at once
    // so one pass repairs the whole drift.
    static SustainabilityCatalogue() {
        string[] diverged = [
            .. Rows.Keys.Except(MaterialPropertyCatalogue.Rows.Keys)
                .Concat(MaterialPropertyCatalogue.Rows.Keys.Except(Rows.Keys))
                .Select(static id => id.Value)
                .Order(StringComparer.Ordinal)];
        if (diverged.Length > 0) {
            throw new InvalidOperationException($"<full-roster-divergence:{string.Join(',', diverged)}>");
        }
    }

    // --- [TABLES]
    // The shared INDUSTRY-AVERAGE profiles, hoisted above Rows exactly as the engineering roster hoists FireA1/ZSteel:
    // an eco-profile is a datum shared by every grade whose MASS it prices, so the six-module vector, the EPD identity,
    // the validity year, and the recycled/recovery pair travel as ONE anchor and a corrected industry figure is a
    // one-line edit rather than a twenty-three-row sweep. Each row then spells the anchor plus its OWN cost triple and
    // classification pair — the two columns that genuinely vary per grade.
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
    static readonly EcoProfile StructuralPolymer = new([4.20, 0.060, 0.050, 0.0, 2.60, -0.40], "Generic-StructuralPolymer", 2029, 0.00, 0.00);

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
    internal static Fin<Seq<MaterialPropertySet>> Lower(SustainabilityRow row, Op key) =>
        // A row declaring BOTH the carbon-only vector and a full matrix declares its GWP row twice, and no rule
        // picks a winner that is not a guess about which the producer meant — so the pair refuses rather than
        // resolves, and a full-matrix row leaves StageGwp empty.
        row.Matrix.IsSome && !row.StageGwp.IsEmpty
            ? ElementFault.ValueRejected(key, "<environmental-declares-vector-and-matrix>")
            : row.Matrix.IsNone && row.StageGwp.Length != LifecycleStage.Count
            ? ElementFault.ValueRejected(key, $"<stage-gwp-arity:{row.StageGwp.Length}:expected={LifecycleStage.Count}>")
            : (MeasurementBasis.Parse(row.EnvironmentalBasis, key)
                   .Bind(basis => MaterialPropertySet.OfEnvironmental(
                       basis,
                       // A full declaration passes STRAIGHT through: CarbonMatrix is the partial-EPD embedding
                       // and running it over an already-complete matrix would zero every indicator but GWP.
                       row.Matrix.IfNone(() => MaterialPropertySet.Environmental.CarbonMatrix(row.StageGwp)),
                       row.Recycled.Central, row.Recovery.Central, key, row.Evidence))
                   .ToValidation(),
               row.Cost.Match(
                   None: static () => NoCost,
                   Some: c => (Currency.Parse(c.Currency, key).ToValidation(),
                               MeasurementBasis.Parse(c.Basis, key).ToValidation())
                       .Apply(static (currency, costBasis) => (currency, costBasis)).As()
                       .Bind(x => MaterialPropertySet.OfCost(x.costBasis, x.currency, c.Supply.Central, c.Install.Central, c.Lifecycle.Central, key, c.Supply.Evidence)
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
    // The roster grows by ROW; a material with no declared EPD omits a row and Lookup returns the empty lifecycle
    // set, never a fault. Cost columns are USD unit-rate ESTIMATES at the matching basis and carry the estimate
    // evidence class that says so; Uniclass 2015 Pr_ codes are the BIM classification the Classification egress
    // lifts. An `R1 PENDING` note marks a row still pricing a FAMILY placeholder rather than its own product
    // declaration — each closes against a specific dataset with no row edit beyond its anchor.
    internal static readonly FrozenDictionary<MaterialId, SustainabilityRow> Rows = new (MaterialId Id, SustainabilityRow Row)[] {
        // --- carbon structural steel (EN 10025; per-kg; WorldSteel Europe sections eco-profile shared S235..S690 — GWP tracks mass not grade; EAF recycled, negative D avoided-burden)
        (MaterialId.Of("steel.s235"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s275"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.98, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s355"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.05, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s420"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.15, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // steel.s450 — the EN 10025-2 S450 grade (properties.md parity; the Component/steel SteelGrade.S450 substance): the shared WorldSteel section profile
        (MaterialId.Of("steel.s450"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.20, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s460"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.25, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.s690"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.55, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // generic structural-steel alias (properties.md metal.steel — the unspecified-grade S235 baseline): shares the WorldSteel section eco-profile
        (MaterialId.Of("metal.steel"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // generic cast/ductile-iron (properties.md metal.iron — EN-GJS-400 casting baseline the joint/weld family keys): Furnes NEPD-9786-9710 ductile cast-iron EPD per-tonne -> per-kg, 100% remeltable
        (MaterialId.Of("metal.iron"),   new("per-kg", new[] { 0.213, 0.057, 0.058, 0.0, 0.054, -0.036 }, "Furnes-DuctileCastIron", 2030, 0.85, 0.95, Some(("USD", "per-kg", 1.20, 0.60, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- AISC structural steel (properties.md steel.a36/a992/a572 — the Component/steel SteelGrade substances; per-kg; North-American EAF hot-rolled sections industry average, ~93% recycled)
        (MaterialId.Of("steel.a36"),    new("per-kg", AiscHotRolled, Some(("USD", "per-kg", 0.90, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a992"),   new("per-kg", AiscHotRolled, Some(("USD", "per-kg", 1.00, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a572"),   new("per-kg", AiscHotRolled, Some(("USD", "per-kg", 1.00, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- cold-formed sheet + fasteners (properties.md steel.g33/g50 + steel.fastener-* — the connector Gauge /
        //     fastener Grade SubstanceId rows; FULL_ROSTER parity). Carbon-steel products, GWP tracks mass not grade.
        //     R1 PENDING a cold-formed-coil / high-strength-fastener producer EPD: these carry the generic WorldSteel
        //     carbon-steel vector as the grade-agnostic placeholder (the metal.steel treatment), never a fabricated
        //     product-specific value — a real cold-formed/galvanized-sheet or fastener EPD supersedes on admission.
        (MaterialId.Of("steel.g33"),           new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.g50"),           new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-4_6"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-4_8"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-5_6"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-5_8"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-6_8"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-8_8"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-10_9"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-12_9"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr2"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr5"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-gr8"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-a325"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.fastener-a490"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 0.95, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- hollow-section, pipe, and sheet steels (properties.md steel.a500/a53/a653 — the Component/steel SteelGrade
        //     substances the GradeOf policy selects; per-kg). A500/A53 are North-American EAF hot-formed products and
        //     share the AISC sections profile; A653 galvanized sheet takes the generic WorldSteel carbon vector under
        //     the same R1 gate the cold-formed g33/g50 rows carry, since no galvanized-coil producer EPD is admitted.
        (MaterialId.Of("steel.a500"), new("per-kg", AiscHotRolled, Some(("USD", "per-kg", 1.35, 0.60, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a53"),  new("per-kg", AiscHotRolled, Some(("USD", "per-kg", 1.30, 0.60, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        (MaterialId.Of("steel.a653"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.10, 0.55, 0.10)), Some(("uniclass-2015", "Pr_20_85_08_11")))),
        // --- weld filler metal (properties.md steel.e60..steel.e110 — the Component/joint ElectrodeClass substances;
        //     per-kg). R1 PENDING an AWS consumable-producer EPD: the deposited metal is carbon or low-alloy steel, so
        //     these carry the grade-agnostic WorldSteel carbon vector as the placeholder the fastener rows already
        //     take, never a fabricated consumable-specific figure. No classification: a filler metal is a consumable,
        //     not a Uniclass product an IfcRelAssociatesClassification would carry.
        (MaterialId.Of("steel.e60"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 3.20, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.e70"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 3.40, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.e80"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 4.60, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.e90"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 5.10, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.e100"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 5.80, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.e110"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 6.40, 0.00, 0.00)), None)),
        // --- headed shear studs (properties.md steel.sd1/sd2/sd3/aws-a/aws-b — the Component/joint StudGrade
        //     substances; per-kg). The carbon grades take the WorldSteel vector under the same R1 gate; SD3 is
        //     X5CrNi18-10 austenitic stainless and takes the Stalatube austenitic profile its own substance row already
        //     keys, so the stud and the stainless family price one eco-profile.
        (MaterialId.Of("steel.sd1"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 2.40, 0.85, 0.05)), None)),
        (MaterialId.Of("steel.sd2"),   new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 2.30, 0.85, 0.05)), None)),
        (MaterialId.Of("steel.sd3"),   new("per-kg", new[] { 1.61, 0.108, 0.050, 0.0, 0.022, -0.183 }, "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00, Some(("USD", "per-kg", 6.80, 0.85, 0.05)), None)),
        (MaterialId.Of("steel.aws-a"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 2.30, 0.85, 0.05)), None)),
        (MaterialId.Of("steel.aws-b"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 2.40, 0.85, 0.05)), None)),
        // --- plain-shank fastener stock (properties.md steel.fastener-nail/-dowel/-rivet — the Component/fastener
        //     StockRow.Plain substances; per-kg). Same R1 gate and same WorldSteel placeholder as the threaded
        //     fastener-* rows above, so the whole fastener estate prices one carbon vector until a product EPD lands.
        (MaterialId.Of("steel.fastener-nail"),  new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.80, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.fastener-dowel"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 1.60, 0.00, 0.00)), None)),
        (MaterialId.Of("steel.fastener-rivet"), new("per-kg", WorldSteelSection, Some(("USD", "per-kg", 2.20, 0.00, 0.00)), None)),
        // --- prestressing strand (properties.md steel.strand-1725/-1860/y1860s7 — the Component/reinforcement
        //     StrandRow substances; per-kg). R1 PENDING a seven-wire-strand producer EPD: drawn strand is an EAF
        //     long-product line, so the three rows share the ArcelorMittal EAF profile the rebar family carries and
        //     the rebar Uniclass code, never a fabricated strand-specific figure. Cold-drawing adds real process
        //     energy the shared vector under-prices; the R1 row states that.
        (MaterialId.Of("steel.strand-1725"), new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 1.90, 0.55, 0.06)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.strand-1860"), new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 1.95, 0.55, 0.06)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.y1860s7"),     new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 2.05, 0.55, 0.06)), Some(("uniclass-2015", "Pr_20_29_72")))),
        // --- structural adhesives and sealant (properties.md adhesive.epoxy/methacrylate/polyurethane +
        //     sealant.silicone-structural — the Component/joint AdhesiveClass substances; per-kg). R1 PENDING a
        //     structural-adhesive producer EPD: the four share the generic thermoset-polymer vector (fossil-feedstock
        //     A1-A3, incineration-dominated C, no recycling credit), never a fabricated chemistry-specific figure.
        (MaterialId.Of("adhesive.epoxy"),              new("per-kg", StructuralPolymer, Some(("USD", "per-kg", 18.0, 2.50, 0.00)), None)),
        (MaterialId.Of("adhesive.methacrylate"),       new("per-kg", StructuralPolymer, Some(("USD", "per-kg", 22.0, 2.50, 0.00)), None)),
        (MaterialId.Of("adhesive.polyurethane"),       new("per-kg", StructuralPolymer, Some(("USD", "per-kg", 14.0, 2.50, 0.00)), None)),
        (MaterialId.Of("sealant.silicone-structural"), new("per-kg", StructuralPolymer, Some(("USD", "per-kg", 26.0, 3.50, 0.40)), None)),
        // --- stainless steel (EN 10088; per-kg; Stalatube/Outokumpu/Aperam EPDs; austenitic 1.4301..1.4571, duplex 1.4462; ~100% effective recycling)
        (MaterialId.Of("steel.1.4301"), new("per-kg", new[] { 1.61, 0.108, 0.050, 0.0, 0.022, -0.183 }, "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4307"), new("per-kg", new[] { 1.61, 0.108, 0.050, 0.0, 0.022, -0.183 }, "Stalatube-1.4301-1.4307", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4401"), new("per-kg", new[] { 1.83, 0.077, 0.050, 0.0, 0.022, -0.181 }, "Stalatube-1.4404", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.80, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4404"), new("per-kg", new[] { 1.83, 0.077, 0.050, 0.0, 0.022, -0.181 }, "Stalatube-1.4404", 2028, 0.75, 1.00, Some(("USD", "per-kg", 3.80, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4571"), new("per-kg", new[] { 1.83, 0.080, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Austenitic", 2028, 0.75, 1.00, Some(("USD", "per-kg", 4.60, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        (MaterialId.Of("steel.1.4462"), new("per-kg", new[] { 3.18, 0.096, 0.050, 0.0, 0.046, -0.114 }, "Outokumpu-Duplex-2205", 2028, 0.75, 1.00, Some(("USD", "per-kg", 4.20, 0.65, 0.12)), Some(("uniclass-2015", "Pr_20_85_08_83")))),
        // --- reinforcing steel (EN 10080; per-kg; ArcelorMittal/CARES EAF rebar shared across the six-grade EnRebarGrade set — ductility class drives properties.md not mass-GWP)
        (MaterialId.Of("steel.b450a"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b450c"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500a"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500b"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b500c"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.82, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.b550b"),  new("per-kg", ArcelorRebar, Some(("USD", "per-kg", 0.82, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        // ASTM A615 (steel.gr40..gr80) + A706 weldable (gr60w/gr80w) + CSA G30.18 (400w/500w): North-American EAF rebar shares the CRSI/ASTM industry-average rebar family (GWP tracks mass not grade)
        (MaterialId.Of("steel.gr40"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.78, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr60"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.80, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr75"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.84, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr80"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.88, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr60w"),  new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.86, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.gr80w"),  new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.92, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.400w"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.84, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        (MaterialId.Of("steel.500w"),   new("per-kg", CrsiRebar, Some(("USD", "per-kg", 0.90, 0.45, 0.05)), Some(("uniclass-2015", "Pr_20_29_72")))),
        // --- concrete (EN 1992/EN 206 strength classes; per-m3; ICE v3 / EC3 / EN 206 ready-mix EPD; A1-A3 scales with cement content, B1 carbonation re-uptake, ~25% GGBS baseline)
        (MaterialId.Of("concrete.c12_15"),  new("per-m3", ReadyMixBase.At(180.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 105.0, 90.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c16_20"),  new("per-m3", ReadyMixBase.At(195.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 110.0, 92.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c20_25"),  new("per-m3", ReadyMixBase.At(210.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 115.0, 94.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c25_30"),  new("per-m3", ReadyMixBase.At(235.0, "Interbeton-EN206-C25"), Some(("USD", "per-m3", 120.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c30_37"),  new("per-m3", ReadyMixBase.At(236.0, "Interbeton-EN206-C30"), Some(("USD", "per-m3", 130.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c35_45"),  new("per-m3", ReadyMixBase.At(260.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 142.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c40_50"),  new("per-m3", ReadyMixBase.At(285.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 155.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c45_55"),  new("per-m3", ReadyMixBase.At(310.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 168.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c50_60"),  new("per-m3", ReadyMixBase.At(335.0, "ICE-v3-EC3-ReadyMix"), Some(("USD", "per-m3", 182.0, 98.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c55_67"),  new("per-m3", ReadyMixBase.At(360.0, "Arup-EC-Scheme-HS"), Some(("USD", "per-m3", 198.0, 100.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c60_75"),  new("per-m3", ReadyMixBase.At(380.0, "Arup-EC-Scheme-HS"), Some(("USD", "per-m3", 215.0, 100.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c70_85"),  new("per-m3", ReadyMixBase.At(410.0, "Arup-EC-Scheme-HS"), Some(("USD", "per-m3", 235.0, 105.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c80_95"),  new("per-m3", ReadyMixBase.At(430.0, "Arup-EC-Scheme-HS"), Some(("USD", "per-m3", 255.0, 105.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.c90_105"), new("per-m3", ReadyMixBase.At(450.0, "Arup-EC-Scheme-HS"), Some(("USD", "per-m3", 280.0, 110.0, 9.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        (MaterialId.Of("concrete.lc"),      new("per-m3", ReadyMixBase.At(220.0, "ICE-LightweightConc"), Some(("USD", "per-m3", 165.0, 95.0, 8.0)), Some(("uniclass-2015", "Pr_20_85_08_15")))),
        // concrete.cmu — the CMU block-concrete substance (properties.md parity; the Component/cmu SubstanceId): the concrete-block EPD family the masonry.aggregate unit shares
        (MaterialId.Of("concrete.cmu"),     new("per-m3", ConcreteBlock, Some(("USD", "per-m3", 150.0, 140.0, 10.0)), Some(("uniclass-2015", "Pr_20_93_52_01")))),
        // --- sawn structural timber (EN 338; per-m3; Holmen/Moelven/Stora Enso sawn-softwood EPD shared C14..C50, hardwood D18..D80; negative A1-A3 biogenic, positive C combustion)
        (MaterialId.Of("timber.c14"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 420.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c16"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 430.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c18"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 440.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c20"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 450.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c22"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 460.0, 160.0, 28.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c24"),   new("per-m3", SawnSoftwood.At(-734.0, "Moelven-Holmen-C24"), Some(("USD", "per-m3", 470.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c27"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 490.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c30"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 510.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c35"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 540.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c40"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 580.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c45"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 620.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.c50"),   new("per-m3", SawnSoftwood, Some(("USD", "per-m3", 660.0, 165.0, 30.0)), Some(("uniclass-2015", "Pr_20_85_08_81")))),
        (MaterialId.Of("timber.d18"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 700.0, 180.0, 35.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d24"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 760.0, 180.0, 35.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d27"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 800.0, 182.0, 36.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d30"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 850.0, 185.0, 38.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d35"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 940.0, 185.0, 38.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d40"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1050.0, 190.0, 40.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d45"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1120.0, 192.0, 41.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d50"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1200.0, 195.0, 42.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d55"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1300.0, 198.0, 43.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d60"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1400.0, 200.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d65"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1520.0, 205.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d70"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1650.0, 210.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d75"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1780.0, 215.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("timber.d80"),   new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1920.0, 220.0, 52.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),
        (MaterialId.Of("wood.oak"),     new("per-m3", KilnHardwood, Some(("USD", "per-m3", 1200.0, 280.0, 60.0)), Some(("uniclass-2015", "Pr_20_85_08_36")))),  // named European oak hardwood (~EN 338 D30), the seam/sibling-referenced id
        // --- glued-laminated timber (EN 14080; per-m3; HASSLACHER EN 15804+A2 glulam EPD shared across the full GL20h..GL32h / GL20c..GL32c set — GWP tracks mass not strength class)
        (MaterialId.Of("timber.gl20h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 820.0, 235.0, 44.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl20c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 790.0, 235.0, 44.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl22h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 850.0, 238.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl22c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 815.0, 238.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl24h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 880.0, 240.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl24c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 840.0, 240.0, 45.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl26h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 900.0, 245.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl26c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 860.0, 245.0, 46.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl28h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 920.0, 250.0, 47.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl28c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 880.0, 250.0, 47.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl30h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 960.0, 255.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl30c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 920.0, 255.0, 48.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl32h"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 1000.0, 260.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        (MaterialId.Of("timber.gl32c"), new("per-m3", HasslacherGlulam, Some(("USD", "per-m3", 960.0, 260.0, 50.0)), Some(("uniclass-2015", "Pr_20_85_08_33")))),
        // --- wrought aluminium (EN 1999/EN 573; per-kg; European profile/extrusion EPD; energy-intensive primary, ~95% effective recycling, large negative D; GWP tracks mass not alloy)
        (MaterialId.Of("aluminium.6082t6"), new("per-kg", new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6082", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.40, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.6061t6"), new("per-kg", new[] { 5.73, 0.050, 0.068, 0.0, 0.051, -3.09 }, "EU-Al-Profile-6061", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.40, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.6063t5"), new("per-kg", new[] { 5.50, 0.050, 0.050, 0.0, 0.033, -1.70 }, "Pandolfo-Al-6063", 2027, 0.50, 0.95, Some(("USD", "per-kg", 3.20, 0.90, 0.20)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        (MaterialId.Of("aluminium.5083"),   new("per-kg", new[] { 8.50, 0.050, 0.050, 0.0, 0.050, -4.50 }, "EU-Al-Plate-5083", 2027, 0.35, 0.95, Some(("USD", "per-kg", 4.10, 0.95, 0.22)), Some(("uniclass-2015", "Pr_20_85_08_02")))),
        // --- masonry units (EN 771; per-m3; Wienerberger/Xella EPD; fired clay kiln carbon, calcium-silicate/AAC carbonation B1 credit)
        (MaterialId.Of("masonry.clay"),  new("per-m3", new[] { 320.0, 25.6, 2.6, 0.0, 25.6, -16.4 }, "Wienerberger-ClayBrick", 2030, 0.00, 0.90, Some(("USD", "per-m3", 280.0, 220.0, 15.0)), Some(("uniclass-2015", "Pr_20_93_52_15")))),
        (MaterialId.Of("masonry.calciumsilicate"), new("per-m3", new[] { 221.0, 13.6, 6.05, -94.5, 49.16, -7.98 }, "Xella-Silka-CS", 2031, 0.00, 0.90, Some(("USD", "per-m3", 240.0, 200.0, 14.0)), Some(("uniclass-2015", "Pr_20_93_52_12")))),
        (MaterialId.Of("masonry.aac"),   new("per-m3", new[] { 160.0, 0.078, 1.0, -36.4, 9.31, -1.03 }, "Xella-Ytong-AAC", 2030, 0.00, 0.90, Some(("USD", "per-m3", 190.0, 160.0, 12.0)), Some(("uniclass-2015", "Pr_20_93_52_05")))),
        (MaterialId.Of("masonry.aggregate"), new("per-m3", ConcreteBlock, Some(("USD", "per-m3", 160.0, 150.0, 11.0)), Some(("uniclass-2015", "Pr_20_93_52_01")))),
        // --- dimension stone (EN 771-6 natural stone; per-m3; A4/A5 dominate for imported slab)
        (MaterialId.Of("stone.marble"),  new("per-m3", new[] { 500.0, 60.0, 30.0, 0.0, 60.0, -17.5 }, "EU-Marble-Slab", 2030, 0.00, 1.00, Some(("USD", "per-m3", 950.0, 320.0, 40.0)), Some(("uniclass-2015", "Pr_20_93_52_56")))),
        (MaterialId.Of("stone.granite"), new("per-m3", new[] { 95.0, 59.4, 96.7, 0.0, 41.4, -4.2 }, "IST-Granite-Slab", 2030, 0.00, 0.90, Some(("USD", "per-m3", 880.0, 300.0, 38.0)), Some(("uniclass-2015", "Pr_20_93_52_56")))),
        // --- glazing (EN 572 float soda-lime + EN 1748-1 borosilicate; per-kg; Glas Trösch EUROFLOAT / AGC / SCHOTT EPD;
        //     glass.crown/glass.flint the Component/glazing pane SubstanceIds — crown the float profile, flint the fire-glass borosilicate)
        (MaterialId.Of("glass.float"),  new("per-kg", EuroFloatGlass, Some(("USD", "per-kg", 1.80, 0.70, 0.10)), Some(("uniclass-2015", "Pr_25_71_33")))),
        (MaterialId.Of("glass.crown"),  new("per-kg", EuroFloatGlass, Some(("USD", "per-kg", 1.85, 0.70, 0.10)), Some(("uniclass-2015", "Pr_25_71_33")))),
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

    // The LOWERED catalogue, frozen at first access. Lowering parses two tokens and builds a full
    // (ImpactCategory × LifecycleStage) matrix per row, so a projector resolving a thousand elements previously
    // rebuilt that matrix a thousand times over a table that cannot change. Only the rows that LOWER memoize: a
    // curation defect is not a hot path, so a failing row re-derives at the caller's key with its whole ManyErrors
    // set rather than a summary re-stamped from a frozen cell.
    static readonly Lazy<FrozenDictionary<MaterialId, Seq<MaterialPropertySet>>> Lowered =
        new(static () => Rows
                .Select(static entry => (entry.Key, Sets: Lower(entry.Value, LowerKey)))
                .Where(static entry => entry.Sets.IsSucc)
                .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Sets.IfFail(Seq<MaterialPropertySet>())),
            LazyThreadSafetyMode.ExecutionAndPublication);

    // A material with no declared sustainability data carries no lifecycle case and is NOT a fault — the
    // declared-or-absent shape the engineering catalogue's REQUIRED Lookup is the dual of.
    public static Fin<Seq<MaterialPropertySet>> Lookup(MaterialId id, Op key) =>
        Lowered.Value.TryGetValue(id, out Seq<MaterialPropertySet> lowered)
            ? Fin.Succ(lowered)
            : Rows.TryGetValue(id, out SustainabilityRow? row)
                ? Lower(row!, key)
                : Fin.Succ(Seq<MaterialPropertySet>());

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
                Some: c => global::Rasm.Element.Classification.Classification.Of(c.System, c.Code, key).Map(Some),
                None: () => Fin.Succ(Option<Classification>.None))
            : Fin.Succ(Option<Classification>.None);
}
```

## [03]-[RESEARCH]

(none)
