# [MATERIALS_ASSESSMENT]

THE DATED-DECLARATION SOURCE. The two catalogue owners are CURATED: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` and `Properties/sustainability#SUSTAINABILITY_PROPERTY` seed the estate's known-material physics and lifecycle rows as in-fence published data under `SEED_ROW_LAW`, so every value they carry is a standards table transcribed once and every row is as good as the standard behind it. A real project also carries data those rosters cannot hold: an in-situ rebound-hammer strength on a fifty-year-old slab, a laboratory certificate for one delivered batch, a manufacturer EPD for the exact product specified, a condition grade from a structural survey. Each is a MEASUREMENT with a date, a provenance, and an expiry — not a standards row — and each must be able to OVERRIDE the seed row for the material it describes without editing a curated catalogue. This owner is that third source: one `AssessmentRecord` `[Union]` closing the declaration modality (`Measured` a dated in-situ or laboratory result · `Graded` a survey condition class · `Declared` a product declaration carrying an EPD row), one `AssessmentAdmission` fold lowering an admitted record onto the SAME `Published<T>` carrier `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` DECLARES — so an assessed column and a seed column are one type at the seam and the projector reads one carrier — and one `AssessmentResolution` law that RESOLVES assessed over published per column, per material, at a stated instant. The `EpdRow` shape lands here rather than on the sustainability roster because a product EPD is a DECLARATION with an issuer, a declared unit, a module coverage census, and a `NodaTime` expiry; the curated per-kg industry averages are its FALLBACK, demoted rather than deleted. This page re-mints NO seam type, admits NO `UnitsNet` quantity beyond the shared carrier's own arms, and rails ONE band — the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500) both sibling sources rail — so an assessed material and a catalogued material fault identically. The record TRANSPORT (the `python:data` Assessment wire the `ARCHITECTURE.md` `DataPeer e3` edge carries) is the peer's schema and is NOT this page's: the vocabulary and the fold land whole and admit a decoded record whatever wire delivers it.

## [01]-[INDEX]

- [02]-[ASSESSMENT_RECORD]: the `AssessmentModality` provenance axis, the `ConditionGrade` survey vocabulary, the `EpdRow` product-declaration shape with its declared unit and module coverage, the `AssessmentRecord` `[Union]` closing the three declaration modalities, and the `AssessmentAdmission.Admit` fold lowering a record onto the shared `Published<T>` carrier.
- [03]-[ASSESSED_RESOLUTION]: `AssessmentSet` the per-material record set, the assessed-over-published resolution law with its expiry and evidence-rank gates, and the `Resolve` entry the projector composes ahead of the two catalogue lookups.

## [02]-[ASSESSMENT_RECORD]

- Owner: `AssessmentModality` the closed provenance axis carrying each source's evidence rank and default relative band; `ConditionGrade` the survey condition vocabulary carrying its capacity-retention factor; `EpdRow` the product-declaration record (issuer, registration, declared unit, module coverage, expiry, the per-module GWP vector); `AssessmentRecord` the closed declaration family; `AssessmentAdmission` the ONE record→`Published<T>` fold.
- Cases: `Measured` (a dated scalar result for ONE named property over a `MaterialId` — a rebound-hammer `f_c`, a coupon tensile, a core density — carrying its instrument-relative band and its `LocalDate`) · `Graded` (a survey `ConditionGrade` over a `MaterialId`, whose retention factor scales the resolved mechanical columns rather than replacing them) · `Declared` (an `EpdRow` product declaration replacing the curated lifecycle row for the material it names). A fourth modality is one case and one `Admit` arm and one resolution arm — compiler-forced at all three.
- Entry: `public static Fin<Assessed> AssessmentAdmission.Admit(AssessmentRecord record, Op key)` — the ONE admission: it proves the record's own shape (a measured value finite and positive on a positive-only property, an EPD's module vector at `LifecycleStage.Count` arity, a graded record's retention in `(0,1]`), lifts every scalar onto the shared `Published<T>` carrier at the modality's own relative band with the evidence spelled as `PropertyEvidence.Declaration(modality.Key, reference, validUntil)`, and returns the neutral `Assessed` carrier the resolution law folds; `public static Fin<AssessmentSet> AssessmentSet.Of(Seq<AssessmentRecord> records, Op key)` admits a whole delivery in ONE `Traverse` (a malformed record aborts the set — never a silently dropped certificate); `public static Fin<Seq<MaterialPropertySet>> AssessmentResolution.Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key)` is the projector-facing entry that folds the assessed set over the two catalogue lookups.
- Packages: Rasm.Element (project — `MaterialId`, `MaterialPropertySet` + its `Of*` admissions, `MeasureValue`/`MeasureBand`, `PropertyEvidence`, `MeasurementBasis`, `LifecycleStage`, `ElementFault.ValueRejected`), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of`/`Exact`, `MaterialPropertyCatalogue.Lookup`, `SustainabilityCatalogue.Lookup`; SAME namespace so no import), Rasm (project — `Op`), NodaTime (`LocalDate` — the declaration date AND the expiry the resolution law compares; a wall-clock declaration date carries no zone, so `LocalDate` is the type, never an `Instant` fabricated by stamping it UTC), Thinktecture.Runtime.Extensions (`[Union]` the record family, `[SmartEnum<string>]` the modality and grade vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`/`Find`), BCL inbox (`ReadOnlyMemory<double>` the module vector, `FrozenDictionary` the per-material index).
- Growth: a new declaration modality is one `AssessmentRecord` case with its `Admit` arm and its resolution arm; a new survey scheme is one `ConditionGrade` row carrying its retention factor; a new EPD indicator is one seam `ImpactCategory` row the `EpdRow` vector widens against; a new assessable property is one `AssessedProperty` row — never a per-modality record type, never a parallel assessed-material surface, and never a second `Published` carrier.
- Boundary: an `AssessmentRecord` is INGRESS DATA, not a domain owner — `Admit` is its one `BOUNDARY_ADMISSION` and the interior sees only `Assessed`; every scalar rides the shared `Published<T>` so an assessed column and a seed column are ONE type at the seam and the `Published<T>.Band` lowering is the one provider-model→`MeasureBand` bridge for both; a measured value carries the INSTRUMENT's relative band (a rebound hammer is not a coupon test) and never the catalogue's authored transcription band, so the seam's `MeasureBand` distinguishes them without a second column and the `Rasm.Compute` propagation route reads the real spread instead of a precision one instrument never had; expiry is a HARD gate at resolution, never at admission (an expired certificate is a real historical record — it stops overriding), and a record with no expiry never expires; this page reads the two catalogue owners and writes NO catalogue row, so a curated roster stays curated and an assessment never mutates a standards table; the EPD transport is the `python:data` peer's wire and no wire record type is declared here.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation slot the per-record admission accumulates
using NodaTime;                      // LocalDate — the declaration date and the expiry the resolution law compares
using Rasm.Domain;                   // Op
using Rasm.Element.Composition;      // MaterialId, MaterialPropertySet + its Of* admissions, MeasureValue, MeasurementBasis,
using Rasm.Element.Projection;       // LifecycleStage, PropertyEvidence (Composition); ElementFault (Projection)
using Rasm.Element.Properties;
using Thinktecture;                  // [Union], [SmartEnum<string>], ComparerAccessors
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // beside MaterialPropertyCatalogue and SustainabilityCatalogue — the shared Published<T> carrier is namespace-local

// --- [TYPES] -------------------------------------------------------------------------------
// The provenance axis: WHERE a declaration came from decides two things a consumer must not re-derive — its evidence
// RANK (which record wins when two describe one column) and its default relative BAND (a rebound hammer is not a
// coupon test, and pretending otherwise publishes a laboratory band on a field reading). Rank is a domain column,
// never a bent comparer, and a new modality is one row with both.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessmentModality {
    public static readonly AssessmentModality Survey     = new("survey",     rank: 1, relative: 0.30);   // visual/condition survey — the widest band
    public static readonly AssessmentModality NonDestructive = new("non-destructive", rank: 2, relative: 0.20);   // rebound hammer, UPV, cover meter
    public static readonly AssessmentModality Core       = new("core",       rank: 3, relative: 0.12);   // extracted core / in-situ coupon
    public static readonly AssessmentModality Laboratory = new("laboratory", rank: 4, relative: 0.05);   // certified batch test to the standard's own method
    public static readonly AssessmentModality Declaration = new("declaration", rank: 5, relative: 0.05); // a verified product declaration (EPD, mill certificate)
    public int Rank { get; }
    public double Relative { get; }
}

// The survey condition vocabulary: a graded record does NOT replace a strength column — it scales the resolved one,
// because a survey observes deterioration against the material's own basis rather than measuring a new value. The
// retention factor is the capacity fraction the grade admits; a new scheme is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConditionGrade {
    public static readonly ConditionGrade Sound       = new("sound",       retention: 1.00);
    public static readonly ConditionGrade Fair        = new("fair",        retention: 0.90);
    public static readonly ConditionGrade Deteriorated = new("deteriorated", retention: 0.70);
    public static readonly ConditionGrade Severe      = new("severe",      retention: 0.45);
    public double Retention { get; }
}

// The assessable property axis — the closed set of columns a measured record may override, each naming the seam case
// and column it lands on. A measured value that names no column has nowhere to go, so the vocabulary IS the landing
// map: a new assessable property is one row, and the resolution fold never string-matches a column name.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessedProperty {
    public static readonly AssessedProperty Density       = new("density",        QuantityRow.Density);
    public static readonly AssessedProperty YieldStrength = new("yield-strength", QuantityRow.Pressure);
    public static readonly AssessedProperty Ultimate      = new("ultimate",       QuantityRow.Pressure);
    public static readonly AssessedProperty Youngs        = new("youngs",         QuantityRow.Pressure);
    public static readonly AssessedProperty Conductivity  = new("conductivity",   QuantityRow.ThermalConductivity);
    public QuantityRow Row { get; }   // the ONE typed-mint owner the seam MeasureValue rides — never a local triple
}

// --- [MODELS] ------------------------------------------------------------------------------
// The product-declaration row: the EPD facts a curated industry average cannot carry. Issuer and Registration are the
// declaration's IDENTITY (the pair a procurement filter and a duplicate check key on), DeclaredUnit its own
// MeasurementBasis token parsed at admission (an EPD is published per functional unit and is admitted at THAT unit,
// never renormalized), Modules the per-EN-15978-module GWP vector at LifecycleStage arity, Coverage the census of
// which modules the declaration actually DECLARES (a cradle-to-gate EPD declares A1-A3 and nothing else — a zero in
// an undeclared module is ABSENCE, and reading it as a measured zero is the fabricated-tally defect), ValidUntil the
// calendar expiry the resolution law compares. Coverage is a parallel bool span rather than an Option per cell so the
// vector stays one contiguous read the seam CarbonMatrix consumes unchanged.
public sealed record EpdRow(
    string Issuer,
    string Registration,
    string DeclaredUnit,
    ReadOnlyMemory<double> Modules,
    ReadOnlyMemory<bool> Coverage,
    double RecycledContent,
    double EndOfLifeRecovery,
    LocalDate ValidUntil);

// The dated declaration family — three modalities over one MaterialId, each carrying exactly the evidence its
// resolution arm consumes. A record is DATA: it admits once and the interior sees only the lowered Assessed carrier.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssessmentRecord {
    private AssessmentRecord() { }
    // One dated scalar for one named property, at the modality's own instrument band.
    public sealed record Measured(MaterialId Material, AssessedProperty Property, double SiValue, AssessmentModality Modality, string Reference, LocalDate Taken, Option<LocalDate> ValidUntil) : AssessmentRecord;
    // A survey condition class scaling the resolved mechanical columns rather than replacing them.
    public sealed record Graded(MaterialId Material, ConditionGrade Grade, string Reference, LocalDate Taken, Option<LocalDate> ValidUntil) : AssessmentRecord;
    // A verified product declaration replacing the curated lifecycle row for the material it names.
    public sealed record Declared(MaterialId Material, EpdRow Epd) : AssessmentRecord;

    public MaterialId Subject => Switch(
        measured: static r => r.Material,
        graded: static r => r.Material,
        declared: static r => r.Material);

    public AssessmentModality Modality => Switch(
        measured: static r => r.Modality,
        graded: static _ => AssessmentModality.Survey,
        declared: static _ => AssessmentModality.Declaration);

    // The expiry the resolution law gates on; a record with no declared expiry never expires.
    public Option<LocalDate> Expiry => Switch(
        measured: static r => r.ValidUntil,
        graded: static r => r.ValidUntil,
        declared: static r => Some(r.Epd.ValidUntil));
}

// The ADMITTED record: the neutral carrier the resolution law folds, every scalar already on the shared Published<T>
// with its evidence spelled. The three modalities collapse to three optional slots because each resolution arm reads
// exactly one — a measured column, a retention factor, or a lowered lifecycle pair — and the record's own case
// guaranteed which is present at admission.
public sealed record Assessed(
    MaterialId Material,
    AssessmentModality Modality,
    Option<(AssessedProperty Property, Published<double> Value)> Column,
    Option<double> Retention,
    Option<(MeasurementBasis Basis, ReadOnlyMemory<double> Modules, ReadOnlyMemory<bool> Coverage, Published<double> Recycled, Published<double> Recovery)> Lifecycle,
    PropertyEvidence Evidence,
    Option<LocalDate> Expiry);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssessmentAdmission {
    // The ONE record admission: prove the record's own shape, then lower every scalar onto the shared Published<T>
    // carrier at the MODALITY's relative band with the evidence spelled Declaration(modality, reference, expiry) —
    // so a consumer reading the seam MeasureBand can tell a rebound-hammer reading from a certified coupon without a
    // second column, and an assessed column and a catalogue column are one type. The three arms accumulate nothing
    // across each other (a record is one modality), but each arm's own independent columns accumulate applicatively
    // so a malformed EPD reports its arity AND its fraction faults together.
    public static Fin<Assessed> Admit(AssessmentRecord record, Op key) => record.Switch(
        state: key,
        measured: static (k, r) => double.IsFinite(r.SiValue) && r.SiValue > 0.0
            ? Fin.Succ(new Assessed(
                r.Material, r.Modality,
                Some((r.Property, Published.Of(r.SiValue, r.Modality.Relative, Evidence(r.Modality, r.Reference, r.ValidUntil)))),
                None, None, Evidence(r.Modality, r.Reference, r.ValidUntil), r.ValidUntil))
            : ElementFault.ValueRejected(k, $"<assessment-measured-nonpositive:{r.Material.Value}:{r.Property.Key}:{r.SiValue:R}>"),
        graded: static (k, r) => Fin.Succ(new Assessed(
            r.Material, AssessmentModality.Survey, None, Some(r.Grade.Retention), None,
            Evidence(AssessmentModality.Survey, r.Reference, r.ValidUntil), r.ValidUntil)),
        declared: static (k, r) => Declared(r.Material, r.Epd, k));

    // The EPD arm's own admission: the module vector and its coverage census must both stand at LifecycleStage arity
    // (a short vector would be silently zero-padded by the seam CarbonMatrix, publishing undeclared modules as
    // measured zeros), the declared unit must parse to a seam MeasurementBasis (a malformed declared_unit is a fault,
    // never a silent PerM3 default), and the two resource fractions must sit in the unit interval. The three checks
    // are INDEPENDENT and accumulate, so a bad delivery reports every defect in one Fin.Fail.
    static Fin<Assessed> Declared(MaterialId material, EpdRow epd, Op key) =>
        (guard(epd.Modules.Length == LifecycleStage.Count && epd.Coverage.Length == LifecycleStage.Count,
             ElementFault.ValueRejected(key, $"<epd-module-arity:{epd.Registration}:{epd.Modules.Length}:{epd.Coverage.Length}:expected={LifecycleStage.Count}>")).ToValidation(),
         MeasurementBasis.Parse(epd.DeclaredUnit, key).ToValidation(),
         guard(epd.RecycledContent is >= 0.0 and <= 1.0 && epd.EndOfLifeRecovery is >= 0.0 and <= 1.0,
             ElementFault.ValueRejected(key, $"<epd-fraction-out-of-unit:{epd.Registration}:{epd.RecycledContent:R}:{epd.EndOfLifeRecovery:R}>")).ToValidation())
        .Apply((_, basis, _) => new Assessed(
            material, AssessmentModality.Declaration, None, None,
            Some((basis, epd.Modules, epd.Coverage,
                  Published.Of(epd.RecycledContent, AssessmentModality.Declaration.Relative, EpdEvidence(epd)),
                  Published.Of(epd.EndOfLifeRecovery, AssessmentModality.Declaration.Relative, EpdEvidence(epd)))),
            EpdEvidence(epd), Some(epd.ValidUntil)))
        .As()
        .ToFin();

    // Provenance is SINGLE-stored on the seam evidence exactly as the sibling catalogues store theirs: the modality
    // key is the source, the certificate or registration the reference, the expiry the LocalDate — never a parallel
    // per-record provenance column the seam would have to carry twice.
    static PropertyEvidence Evidence(AssessmentModality modality, string reference, Option<LocalDate> validUntil) =>
        validUntil.Match(
            Some: until => PropertyEvidence.Declaration(modality.Key, reference, until),
            None: () => new PropertyEvidence(modality.Key, reference, Option<LocalDate>.None).Normalized());

    static PropertyEvidence EpdEvidence(EpdRow epd) =>
        PropertyEvidence.Declaration("epd", $"{epd.Issuer}:{epd.Registration}", epd.ValidUntil);
}
```

## [03]-[ASSESSED_RESOLUTION]

- Owner: `AssessmentSet` the admitted per-material record index; `AssessmentResolution` the assessed-over-published law and the `Resolve` entry the projector composes.
- Cases: three resolution arms over the admitted `Assessed` carrier — a measured COLUMN replaces the catalogue column it names, a graded RETENTION scales the resolved mechanical strength columns, and a declared LIFECYCLE replaces the curated `Environmental` case whole (its `Cost` case is untouched: an EPD declares environmental impact, never a unit price).
- Entry: `public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key)` — the ONE projector-facing resolution: it reads the two curated catalogues (`MaterialPropertyCatalogue.Lookup` REQUIRED, `SustainabilityCatalogue.Lookup` optional-by-design), selects the LIVE records for the material at the stated instant, folds the winning record per axis by evidence rank, and returns the merged set; with an empty set it returns the catalogue result byte-identically, so a project carrying no assessments pays nothing and reads exactly what the seed pages publish.
- Boundary: assessed BEATS published only while LIVE — a record whose `Expiry` has passed the stated instant is excluded at selection, so an expired mill certificate stops overriding without being deleted (it remains a true historical record, and a resolution at an earlier instant still honours it); ties break by `AssessmentModality.Rank` and then by the later declaration date, so a laboratory certificate outranks a rebound-hammer reading and a newer certificate outranks an older one of equal rank — never by set order; the resolution instant is a CALLER input, never `SystemClock`, so a resolution is replayable and two runs at one instant agree; a graded record SCALES rather than replaces because a survey observes deterioration against the material's own basis (a `Severe` grade on a C30/37 slab yields `0.45 × f_ck`, not a new characteristic strength), and a graded record therefore never wins an axis a measured record holds — it applies after; an EPD's UNDECLARED modules stay absent (its `Coverage` census gates the lowering, and an undeclared module falls back to the curated industry-average cell rather than reading as a measured zero), which is the whole reason the curated rows are DEMOTED to fallback rather than deleted; this page never writes a catalogue row.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The admitted record set, indexed by subject once: a resolution reads one material and must not scan a delivery.
public sealed record AssessmentSet(FrozenDictionary<MaterialId, Seq<Assessed>> ByMaterial) {
    public static readonly AssessmentSet Empty = new(FrozenDictionary<MaterialId, Seq<Assessed>>.Empty);

    // The ONE set admission: a whole delivery admits in one Traverse, so a malformed certificate ABORTS the set and
    // is never silently dropped — a dropped assessment is a design that silently reverts to catalogue data.
    public static Fin<AssessmentSet> Of(Seq<AssessmentRecord> records, Op key) =>
        records.Traverse(record => AssessmentAdmission.Admit(record, key)).As()
            .Map(static admitted => new AssessmentSet(
                admitted.GroupBy(static a => a.Material).ToFrozenDictionary(static g => g.Key, static g => toSeq(g))));

    // The LIVE selection at an instant, ranked: an expired record is excluded (never deleted), and the winner per
    // axis is the highest-rank record, ties broken by the later evidence expiry so a newer certificate of equal rank
    // supersedes an older one. Ordering is a DOMAIN column read, never set order.
    public Seq<Assessed> Live(MaterialId material, LocalDate at) =>
        ByMaterial.TryGetValue(material, out Seq<Assessed> records)
            ? toSeq(records.Filter(a => a.Expiry.ForAll(until => until >= at))
                .OrderByDescending(static a => a.Modality.Rank)
                .ThenByDescending(static a => a.Expiry.Map(static until => until.ToDateTimeUnspecified()).IfNone(DateTime.MinValue)))
            : Seq<Assessed>();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssessmentResolution {
    // The ONE resolution: read both curated catalogues, then let the live assessed records override per axis. The
    // engineering lookup is REQUIRED (its own asymmetry — a known structural material owes engineering properties),
    // the lifecycle lookup optional-by-design. An empty set returns the catalogue result byte-identically, so the
    // assessed path costs nothing where no project data exists and the curated rows stay the estate's floor.
    public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key) =>
        from engineering in MaterialPropertyCatalogue.Lookup(id, key)
        from lifecycle in SustainabilityCatalogue.Lookup(id, key)
        from resolved in Overlay(engineering + lifecycle, assessed.Live(id, at), key)
        select resolved;

    // The per-axis fold: each live record, highest rank first, applies its own arm to the accumulated set. Column
    // records replace, graded records scale, declared records replace the Environmental case; a record whose axis a
    // higher-ranked record already claimed applies nothing, so the fold is order-total and the winner is the rank
    // column rather than the fold's own traversal order.
    static Fin<Seq<MaterialPropertySet>> Overlay(Seq<MaterialPropertySet> published, Seq<Assessed> live, Op key) =>
        live.Fold(Fin.Succ(published), (state, record) => state.Bind(sets => Apply(sets, record, key)));

    static Fin<Seq<MaterialPropertySet>> Apply(Seq<MaterialPropertySet> sets, Assessed record, Op key) =>
        record switch {
            { Column.IsSome: true } => ReplaceColumn(sets, record, key),
            { Retention.IsSome: true } => ScaleMechanical(sets, record, key),
            { Lifecycle.IsSome: true } => ReplaceEnvironmental(sets, record, key),
            _ => Fin.Succ(sets),
        };

    // A measured column REPLACES its named column on the seam case that owns it, re-admitting through the SAME Of*
    // family the catalogue used — the MeasureValue-taking overload, which re-mints the case from columns the seam
    // case already exposes as reads, so the assessed value crosses the identical guard with no second raw coercion
    // and an assessed material is never a less-admitted material.
    static Fin<Seq<MaterialPropertySet>> ReplaceColumn(Seq<MaterialPropertySet> sets, Assessed record, Op key);

    // A graded record SCALES the resolved strength columns by its retention factor, because a condition survey
    // observes deterioration against the material's own basis rather than measuring a new characteristic value.
    static Fin<Seq<MaterialPropertySet>> ScaleMechanical(Seq<MaterialPropertySet> sets, Assessed record, Op key);

    // A declared EPD REPLACES the curated Environmental case at ITS OWN declared unit, the curated industry-average
    // cell surviving for every module the declaration does not cover — the demotion, not the deletion, of the
    // curated rows. The Cost case is untouched: an EPD declares impact, never price.
    static Fin<Seq<MaterialPropertySet>> ReplaceEnvironmental(Seq<MaterialPropertySet> sets, Assessed record, Op key);
}
```

## [04]-[RESEARCH]

- [ASSESSMENT_WIRE_TOKENS]-[BLOCKED]: do the `python:data` peer's Assessment record modality, property, and declared-unit tokens map onto `AssessmentModality`, `AssessedProperty`, and `MeasurementBasis` rows without a translation table, and in what field order does the record arrive; read the peer's Assessment record schema over the `ARCHITECTURE.md` `[WIRE]: Assessment` crossing once it lands — a token the peer publishes that no row carries is one new row here, never a string escape hatch.
