# [MATERIALS_ASSESSMENT]

THE DATED-DECLARATION SOURCE. The two catalogue owners are CURATED: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` and `Properties/sustainability#SUSTAINABILITY_PROPERTY` seed the estate's known-material physics and lifecycle rows as in-fence published data under `SEED_ROW_LAW`, so every value they carry is as good as the standard behind it. A real project also carries data those rosters cannot hold: an in-situ rebound-hammer strength on a fifty-year-old slab, a laboratory certificate for one delivered batch, a manufacturer EPD for the exact product specified, a condition grade from a structural survey. Each is a MEASUREMENT with a date, a provenance, and an expiry rather than a standards row, and each must OVERRIDE the seed row for the material it describes without editing a curated catalogue. This owner is that third source: one `AssessmentRecord` `[Union]` closing the declaration modality, one `AssessmentAdmission` fold lowering an admitted record onto the SAME `Published<T>` carrier its engineering sibling declares, and one `AssessmentResolution` law resolving assessed over published per column, per material, at a stated instant. This page reads the two catalogue owners and writes NO catalogue row, so a curated roster stays curated and an assessment never mutates a standards table.

The `EpdRow` shape lands here rather than on the sustainability roster because a product EPD is a DECLARATION with an issuer, a declared unit, a module coverage census, and an expiry; the curated industry averages are its FALLBACK, demoted rather than deleted. The page re-mints NO seam type, admits NO `UnitsNet` quantity beyond the shared carrier's own arms, and rails ONE band — the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500) both sibling sources rail — so an assessed material and a catalogued material fault identically. Record TRANSPORT is the corpus `tests/contracts/MANIFEST.md` `[02.26]-[DECLARATION_RECORD]` DOMAIN contract — `python:data` `impact/declaration.md` the one producer, this page's `DeclarationWire.Decode` the committed consumer leg — so a declaration arrives as canonical JSON keyed to a `MaterialId`, lowers onto `EpdRow` and `AssessmentRecord.Declared`, and reaches `AssessmentSet.Of` unchanged. The peer impact wire stays impact-only by its own charter; the declaration contract carries what that frame structurally cannot — identity, dates, and the presence-censused cell map.

## [01]-[INDEX]

- [02]-[ASSESSMENT_RECORD]: the `AssessmentModality` provenance axis, the `ConditionGrade` survey vocabulary, the `AssessedProperty` axis with the landing lens each row owns, the `EpdRow` product-declaration shape, the `AssessmentRecord` `[Union]` closing the three declaration modalities, the `AssessedIdentity` shared record identity, the `AssessmentAdmission.Admit` fold, and the `DeclarationWire` corpus-contract decode leg.
- [03]-[ASSESSED_RESOLUTION]: `AssessmentSet` the per-material record set, the assessed-over-published resolution law with its expiry and evidence-rank gates, and the `Resolve` entry the projector composes ahead of the two catalogue lookups.

## [02]-[ASSESSMENT_RECORD]

- Owner: `AssessmentModality` the closed provenance axis carrying each source's evidence rank and default relative band; `ConditionGrade` the survey condition vocabulary carrying its capacity-retention factor; `AssessedProperty` the assessable-column axis carrying its `QuantityRow` and its landing lens; `EpdRow` the product-declaration record; `AssessmentRecord` the closed declaration family; `AssessedIdentity` the identity every admitted record carries; `Assessed` the `[Union]` closing the three ADMITTED evidence shapes; `AssessmentAdmission` the ONE record→`Assessed` fold.
- Cases: `Measured` (a dated scalar result for ONE named property over a `MaterialId` — a rebound-hammer `f_c`, a coupon tensile, a core density — carrying its instrument-relative band and its `LocalDate`) · `Graded` (a survey `ConditionGrade` whose retention factor scales the resolved mechanical columns rather than replacing them) · `Declared` (an `EpdRow` product declaration replacing the curated lifecycle row for the material it names). A fourth modality is one case, one `Admit` arm, and one resolution arm — compiler-forced at all three.
- Law: A VOCABULARY ROW OWNS ITS OWN LANDING. Each `AssessedProperty` carries the lens that seats its measured column onto the seam case that owns it, so a new assessable property either declares where it lands or does not compile. A resolution that discriminated the landing centrally — one property routed to the thermal case and every other to the mechanical one — silently landed each new row in whichever branch the condition defaulted to, publishing a measured column on a case that does not own it, and the defect was invisible because the fold still type-checked.
- Law: A DECLARATION'S PROVENANCE DECIDES ITS SPREAD. The modality row carries both the evidence RANK that resolves a contest between two records and the default relative BAND the admitted value wears, so a rebound-hammer reading and a certified coupon are distinguishable at the seam `MeasureBand` without a second column and the `Rasm.Compute` propagation route reads the real spread instead of a precision no instrument had. Rank is a domain column, never a bent comparer.
- Entry: `public static Fin<Assessed> AssessmentAdmission.Admit(AssessmentRecord record, Op key)` — the ONE admission: it proves the record's own shape (a measured value finite and positive, an EPD's module vector and coverage census at `LifecycleStage.Count` arity, its resource fractions — where declared — in the unit interval), lifts every scalar onto the shared `Published<T>` carrier at the modality's own band with the evidence spelled `PropertyEvidence.Declaration`, and returns the neutral `Assessed` carrier the resolution law folds. `public static Fin<AssessmentSet> AssessmentSet.Of(Seq<AssessmentRecord> records, Op key)` admits a whole delivery in ONE `Traverse`, so a malformed record ABORTS the set rather than being silently dropped — a dropped assessment is a design that silently reverts to catalogue data. `public static Fin<AssessmentRecord> DeclarationWire.Decode(ReadOnlyMemory<byte> record, Op key)` is the corpus-contract transport admission: it BANDS the contract's fifteen declaration modules onto the six-band `LifecycleStage` axis by summing declared cells (a band covers when any member declares), constructs the full `Matrix` ONLY when every core indicator covers every band — a partial declaration rides the `GwpTotal` vector + census arm, the fabricated-zero matrix being the refused form — maps the declared-unit token onto the seam `MeasurementBasis` roster with any unmapped token refusing typed, and hands the decoded row to the SAME `Admit` gate an in-process record crosses, re-implementing no admission.
- Packages: Rasm.Element (project — `MaterialId`, `MaterialPropertySet` + its `Of*` admissions, `MeasureValue`/`MeasureBand`, `PropertyEvidence`, `MeasurementBasis`, `LifecycleStage`, `ImpactCategory`, `ElementFault.ValueRejected`), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of`, the two catalogue `Lookup` entries; SAME namespace so no import), Rasm.Materials.Component (project — `QuantityRow` and its `OfNative` railed mint), Rasm (project — `Op`), NodaTime (`LocalDate` — the declaration date AND the expiry the resolution law compares; a wall-clock declaration date carries no zone, so `LocalDate` is the type, never an `Instant` fabricated by stamping it UTC), Thinktecture.Runtime.Extensions (`[Union]` the record families, `[SmartEnum<string>]` the modality, grade, and property vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`), BCL inbox (`ReadOnlyMemory<double>`, `ImmutableArray<double>`, `FrozenDictionary`).
- Growth: a new declaration modality is one `AssessmentRecord` case with its `Admit` arm and its resolution arm; a new survey scheme is one `ConditionGrade` row carrying its retention factor; a new EN 15804+A2 indicator is one seam `ImpactCategory` row the declaration's own matrix widens against; a new assessable property is one `AssessedProperty` row carrying its `QuantityRow` and its lens — never a per-modality record type, never a parallel assessed-material surface, and never a second `Published` carrier.
- Boundary: an `AssessmentRecord` is INGRESS DATA rather than a domain owner — `Admit` is its one `BOUNDARY_ADMISSION` and the interior sees only `Assessed`. The five identity columns every admitted shape carries ride ONE `AssessedIdentity` value passed whole, so a sixth identity column widens in one place and no case signature moves; the shape this replaces re-declared all five positionally on each of three cases, spending fifteen slots on five facts and making a transposition between two same-typed columns a compile-clean defect. Every scalar rides the shared `Published<T>`, so an assessed column and a seed column are ONE type at the seam and the `Published<T>.Band` lowering is the one provider-model→`MeasureBand` bridge for both. Expiry is a HARD gate at RESOLUTION and never at admission — an expired certificate is a real historical record that stops overriding — and a record with no expiry never expires.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;              // JsonDocument — the corpus declaration-record decode leg's one reader
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation slot the per-record admission accumulates
using NodaTime;                      // LocalDate — the declaration date and the expiry the resolution law compares
using Rasm.Domain;                   // Op
using Rasm.Element.Composition;      // MaterialId, MaterialPropertySet + its Of* admissions, MeasureValue, MeasurementBasis,
using Rasm.Element.Projection;       // LifecycleStage, ImpactCategory, PropertyEvidence (Composition); ElementFault (Projection)
using Rasm.Element.Properties;
using Rasm.Materials.Component;      // QuantityRow — the one typed-mint owner every assessed column mints through
using Thinktecture;                  // [Union], [SmartEnum<string>], ComparerAccessors
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;   // beside MaterialPropertyCatalogue and SustainabilityCatalogue — the shared Published<T> carrier is namespace-local

// --- [TYPES] -------------------------------------------------------------------------------
// The provenance axis: WHERE a declaration came from decides its evidence RANK and its default relative BAND, and a
// new modality is one row carrying both.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessmentModality {
    public static readonly AssessmentModality Survey         = new("survey",         rank: 1, relative: 0.30);   // visual/condition survey — the widest band
    public static readonly AssessmentModality NonDestructive = new("non-destructive", rank: 2, relative: 0.20);   // rebound hammer, UPV, cover meter
    public static readonly AssessmentModality Core           = new("core",           rank: 3, relative: 0.12);   // extracted core / in-situ coupon
    public static readonly AssessmentModality Laboratory     = new("laboratory",     rank: 4, relative: 0.05);   // certified batch test to the standard's own method
    public static readonly AssessmentModality Declaration    = new("declaration",    rank: 5, relative: 0.05);   // a verified product declaration (EPD, mill certificate)
    public int Rank { get; }
    public double Relative { get; }
}

// The survey condition vocabulary: a graded record does NOT replace a strength column — it scales the resolved one,
// because a survey observes deterioration against the material's own basis rather than measuring a new value. The
// retention factor is the capacity fraction the grade admits; a new scheme is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConditionGrade {
    public static readonly ConditionGrade Sound        = new("sound",        retention: 1.00);
    public static readonly ConditionGrade Fair         = new("fair",         retention: 0.90);
    public static readonly ConditionGrade Deteriorated = new("deteriorated", retention: 0.70);
    public static readonly ConditionGrade Severe       = new("severe",       retention: 0.45);
    public double Retention { get; }
}

// The LANDING LENS an assessable property owns: it seats an admitted measure onto the seam case that owns the
// column and answers the sets unchanged where the material publishes no such case.
public delegate Fin<Seq<MaterialPropertySet>> PropertyLanding(
    Seq<MaterialPropertySet> sets, MeasureValue measure, PropertyEvidence evidence, Op key);

// The assessable-property axis. Each row carries BOTH the typed-mint owner its SI magnitude crosses and the lens
// that seats it, so the resolution fold never discriminates on which property it holds — the row already decided.
// The seat helpers resolve at first use, so declaring them on the resolution owner below is legal and keeps the
// case-rebuild shape with the law that owns it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessedProperty {
    public static readonly AssessedProperty Density = new("density", QuantityRow.Density,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatMechanical(sets, evidence, key, density: measure));
    public static readonly AssessedProperty YieldStrength = new("yield-strength", QuantityRow.Pressure,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatMechanical(sets, evidence, key, yieldStrength: measure));
    public static readonly AssessedProperty Ultimate = new("ultimate", QuantityRow.Pressure,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatMechanical(sets, evidence, key, ultimate: measure));
    public static readonly AssessedProperty Youngs = new("youngs", QuantityRow.Pressure,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatMechanical(sets, evidence, key, youngs: measure));
    public static readonly AssessedProperty Conductivity = new("conductivity", QuantityRow.ThermalConductivity,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatThermal(sets, evidence, key, measure));
    public QuantityRow Row { get; }         // the ONE typed-mint owner the seam MeasureValue rides — never a local triple
    public PropertyLanding Landing { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The product-declaration row: the EPD facts a curated industry average cannot carry. Issuer and Registration are
// the declaration's IDENTITY (the pair a procurement filter and a duplicate check key on), DeclaredUnit its own
// MeasurementBasis token parsed at admission (an EPD is published per functional unit and is admitted at THAT
// unit, never renormalized), Modules the per-EN-15978-module GWP vector, Coverage the census of which modules the
// declaration actually DECLARES, Matrix the full thirteen-indicator declaration where a producer publishes one,
// ValidUntil the calendar expiry the resolution law compares. A zero in an undeclared module is ABSENCE, and
// reading it as a measured zero is the fabricated-tally defect the census exists to prevent. Coverage is a
// parallel bool span rather than an Option per cell so the vector stays one contiguous read. The two resource
// fractions are Option — scenario data many declarations omit, absence never a fabricated fraction — mirroring
// the seam OfEnvironmental's own Option pair rather than forcing a zero the producer never declared.
public sealed record EpdRow(
    string Issuer,
    string Registration,
    string DeclaredUnit,
    ReadOnlyMemory<double> Modules,
    ReadOnlyMemory<bool> Coverage,
    Option<ImmutableArray<double>> Matrix,
    Option<double> RecycledContent,
    Option<double> EndOfLifeRecovery,
    LocalDate Issued,          // the declaration's own issue date — the tie key two revisions of one registration differ on
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

// The identity every admitted record carries, minted ONCE at admission and passed whole. Taken rides here because
// the tie law needs it on every shape: two records of equal rank are separated by which was DECLARED later, and an
// EXPIRY is not that date — two certificates issued a decade apart carry one expiry as easily as not, and a record
// with no expiry has none to compare at all, which is exactly the population an expiry-keyed tiebreak sorted to
// the bottom.
public readonly record struct AssessedIdentity(
    MaterialId Material,
    AssessmentModality Modality,
    PropertyEvidence Evidence,
    LocalDate Taken,
    Option<LocalDate> Expiry);

// The ADMITTED record as a CLOSED UNION over the three EVIDENCE SHAPES, each carrying exactly the columns its own
// resolution arm consumes. The three-optional-slot carrier this replaces made a record with two slots present and a
// record with none equally representable, so the resolution had to re-discriminate on IsSome probes over a fact the
// admission had already decided — and its fallthrough arm silently applied nothing to a record that admitted
// cleanly.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Assessed {
    private Assessed(AssessedIdentity identity) => Identity = identity;
    public AssessedIdentity Identity { get; }

    public sealed record Column(AssessedIdentity Identity, AssessedProperty Property, Published<double> Value) : Assessed(Identity);

    public sealed record Retention(AssessedIdentity Identity, double Factor) : Assessed(Identity);

    public sealed record Lifecycle(
        AssessedIdentity Identity, MeasurementBasis Basis,
        ReadOnlyMemory<double> Modules, ReadOnlyMemory<bool> Coverage, Option<ImmutableArray<double>> Matrix,
        Option<Published<double>> Recycled, Option<Published<double>> Recovery) : Assessed(Identity);

    // The AXIS a record claims — the resolution's exclusion key, so a lower-ranked record cannot re-claim what a
    // higher-ranked one already took. A column claims its OWN property, so two measured records over two different
    // properties BOTH apply while two over one property do not; retention and lifecycle each claim one axis whole.
    public string Axis => Switch(
        column:    static r => $"column:{r.Property.Key}",
        retention: static _ => "retention",
        lifecycle: static _ => "lifecycle");
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssessmentAdmission {
    // The ONE record admission: prove the record's own shape, then lower every scalar onto the shared Published<T>
    // carrier at the MODALITY's relative band. The three arms accumulate nothing across each other (a record is one
    // modality), but each arm's own independent columns accumulate applicatively so a malformed EPD reports its
    // arity AND its fraction faults together.
    public static Fin<Assessed> Admit(AssessmentRecord record, Op key) => record.Switch(
        state: key,
        measured: static (k, r) => double.IsFinite(r.SiValue) && r.SiValue > 0.0
            ? Fin.Succ(Column(Identity(r.Material, r.Modality, r.Reference, r.Taken, r.ValidUntil), r.Property, r.SiValue))
            : ElementFault.ValueRejected(k, $"<assessment-measured-nonpositive:{r.Material.Value}:{r.Property.Key}:{r.SiValue:R}>"),
        graded: static (k, r) => Fin.Succ<Assessed>(new Assessed.Retention(
            Identity(r.Material, AssessmentModality.Survey, r.Reference, r.Taken, r.ValidUntil), r.Grade.Retention)),
        declared: static (k, r) => Declared(r.Material, r.Epd, k));

    // The EPD arm's own admission, holding the SAME two environmental dispositions the curated roster holds: the
    // carbon vector and the full indicator matrix are MUTUALLY EXCLUSIVE, since a row carrying both declares its
    // GWP row twice and no rule picks a winner that is not a guess about which the producer meant. A carbon vector
    // and its coverage census must both stand at LifecycleStage arity — a short vector would be silently
    // zero-padded downstream, publishing undeclared modules as measured zeros. The declared unit must parse to a
    // seam MeasurementBasis (a malformed declared_unit is a fault, never a silent default), and the two resource
    // fractions must sit in the unit interval. The independent checks accumulate, so a bad delivery reports every
    // defect in one Fin.Fail.
    static Fin<Assessed> Declared(MaterialId material, EpdRow epd, Op key) =>
        epd.Matrix.IsSome && !epd.Modules.IsEmpty
            ? ElementFault.ValueRejected(key, $"<epd-declares-vector-and-matrix:{epd.Registration}>")
            : (guard(epd.Matrix.IsSome || (epd.Modules.Length == LifecycleStage.Count && epd.Coverage.Length == LifecycleStage.Count),
                   ElementFault.ValueRejected(key, $"<epd-module-arity:{epd.Registration}:{epd.Modules.Length}:{epd.Coverage.Length}:expected={LifecycleStage.Count}>")).ToValidation(),
               guard(epd.Matrix.ForAll(static matrix => matrix.Length == MaterialPropertySet.Environmental.MatrixArity),
                   ElementFault.ValueRejected(key, $"<epd-matrix-arity:{epd.Registration}:expected={MaterialPropertySet.Environmental.MatrixArity}>")).ToValidation(),
               MeasurementBasis.Parse(epd.DeclaredUnit, key).ToValidation(),
               guard(epd.RecycledContent.ForAll(static f => f is >= 0.0 and <= 1.0)
                       && epd.EndOfLifeRecovery.ForAll(static f => f is >= 0.0 and <= 1.0),
                   ElementFault.ValueRejected(key, $"<epd-fraction-out-of-unit:{epd.Registration}>")).ToValidation())
            // Taken for a declaration is its ISSUE date, which the EPD row carries beside its expiry: two revisions
            // of one registration are separated by issue and never by an expiry both may share.
            .Apply((_, _, basis, _) => Lifecycle(
                new AssessedIdentity(material, AssessmentModality.Declaration, EpdEvidence(epd), epd.Issued, Some(epd.ValidUntil)),
                basis, epd))
            .As()
            .ToFin();

    // The identity mint: the evidence, the band, and the tie key are all decided HERE, so no later arm re-derives
    // provenance and no record carries two spellings of one fact.
    static AssessedIdentity Identity(MaterialId material, AssessmentModality modality, string reference, LocalDate taken, Option<LocalDate> validUntil) =>
        new(material, modality, Evidence(modality, reference, validUntil), taken, validUntil);

    static Assessed Column(AssessedIdentity identity, AssessedProperty property, double si) =>
        new Assessed.Column(identity, property, Published.Of(si, identity.Modality.Relative, identity.Evidence));

    static Assessed Lifecycle(AssessedIdentity identity, MeasurementBasis basis, EpdRow epd) =>
        new Assessed.Lifecycle(
            identity, basis, epd.Modules, epd.Coverage, epd.Matrix,
            epd.RecycledContent.Map(f => Published.Of(f, identity.Modality.Relative, identity.Evidence)),
            epd.EndOfLifeRecovery.Map(f => Published.Of(f, identity.Modality.Relative, identity.Evidence)));

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

// --- [DECLARATION_WIRE]
// The corpus `declaration-record` decode leg — the ONE transport admission. The contract speaks declaration
// granularity (thirteen core indicators × fifteen EN 15978 modules, KEY PRESENCE the coverage census); this leg
// BANDS onto the six-band LifecycleStage axis by summing declared cells (a band covers when any member declares —
// additive impacts sum, absence never fabricates a zero), constructs the full Matrix ONLY when every core
// indicator covers every band (a partial declaration rides the GwpTotal vector + census arm), and maps the
// declared-unit token onto the seam MeasurementBasis roster, any unmapped token (m, l, t, m2r1) refusing typed —
// the seam basis roster is the law, never a silent per-kg default. The decoded row crosses the SAME Admit gate an
// in-process record crosses; this leg re-implements no admission.
public static class DeclarationWire {
    static readonly FrozenDictionary<string, string> Basis = new Dictionary<string, string> {
        ["kg"] = "per-kg", ["m2"] = "per-m2", ["m3"] = "per-m3", ["pcs"] = "per-item",
    }.ToFrozenDictionary();

    // Fifteen contract modules band by MEMBERSHIP rows, so a roster change is a row edit, never a re-derivation.
    static readonly FrozenDictionary<string, LifecycleStage> Band = new Dictionary<string, LifecycleStage> {
        ["a1-a3"] = LifecycleStage.A1A3, ["a4"] = LifecycleStage.A4, ["a5"] = LifecycleStage.A5,
        ["b1"] = LifecycleStage.B, ["b2"] = LifecycleStage.B, ["b3"] = LifecycleStage.B, ["b4"] = LifecycleStage.B,
        ["b5"] = LifecycleStage.B, ["b6"] = LifecycleStage.B, ["b7"] = LifecycleStage.B,
        ["c1"] = LifecycleStage.C, ["c2"] = LifecycleStage.C, ["c3"] = LifecycleStage.C, ["c4"] = LifecycleStage.C,
        ["d"] = LifecycleStage.D,
    }.ToFrozenDictionary();

    public static Fin<AssessmentRecord> Decode(ReadOnlyMemory<byte> record, Op key) {
        using JsonDocument document = JsonDocument.Parse(record);
        JsonElement root = document.RootElement;
        return
            from unit in Basis.TryGetValue(root.GetProperty("declared_unit").GetString() ?? "", out string? basis)
                ? Fin.Succ(basis)
                : ElementFault.ValueRejected(key, $"<declaration-unit-unmapped:{root.GetProperty("declared_unit").GetString()}>")
            from issued in Date(root, "issued", key)
            from validUntil in Date(root, "valid_until", key)
            let source = root.GetProperty("source")
            let banded = Banded(root.GetProperty("indicators"))
            select (AssessmentRecord)new AssessmentRecord.Declared(
                MaterialId.Of(root.GetProperty("material_key").GetString() ?? ""),
                new EpdRow(
                    Issuer: root.GetProperty("issuer").GetString() ?? "",
                    Registration: root.GetProperty("registration").GetString() ?? "",
                    DeclaredUnit: unit,
                    Modules: banded.Matrix.IsSome ? ReadOnlyMemory<double>.Empty : banded.Gwp,
                    Coverage: banded.Matrix.IsSome ? ReadOnlyMemory<bool>.Empty : banded.Covered,
                    Matrix: banded.Matrix,
                    RecycledContent: Fraction(root, "recycled_content"),
                    EndOfLifeRecovery: Fraction(root, "end_of_life_recovery"),
                    Issued: issued,
                    ValidUntil: validUntil));
    }

    static Fin<LocalDate> Date(JsonElement root, string field, Op key);
    // LocalDatePattern.Iso over the contract's ISO date string — a malformed date refuses typed, never a default.

    static Option<double> Fraction(JsonElement root, string field);
    // An OMITTED contract key is Option.None — presence is the declaration, exactly as the cell census reads.

    static (ReadOnlyMemory<double> Gwp, ReadOnlyMemory<bool> Covered, Option<ImmutableArray<double>> Matrix) Banded(JsonElement indicators);
    // Per indicator: sum declared cells into each LifecycleStage band via the Band rows, cover a band on any
    // member's presence. The gwp-total row fills the vector + census; the Matrix constructs (row-major, indicator
    // × stage) only when all thirteen indicator rows cover all six bands — otherwise Option.None and the vector
    // arm carries the declaration exactly as EpdRow's own vector-XOR-matrix admission expects.
}
```

## [03]-[ASSESSED_RESOLUTION]

- Owner: `AssessmentSet` the admitted per-material record index; `AssessmentResolution` the assessed-over-published law with its per-axis `ReplaceColumn`/`ScaleMechanical`/`ReplaceEnvironmental` arms over one shared `Rebuild` re-admission, the two `Seat` lenses the property vocabulary binds, and the `Resolve` entry the projector composes.
- Cases: three resolution arms over the `Assessed` `[Union]`'s own three cases — `Column` replaces the catalogue column it names, `Retention` scales the resolved mechanical strength columns, `Lifecycle` replaces the curated `Environmental` case under its own coverage census. The `Cost` case is untouched: an EPD declares environmental impact and never a unit price. `Apply` is the generated total `Switch`, so a fourth evidence shape is compiler-forced rather than silently no-op.
- Law: RANK DECIDES, IT DOES NOT MERELY ORDER. Ranking is `AssessmentModality.Rank` first and the later `Taken` declaration date second, both compared as `LocalDate` in its own calendar with no BCL crossing and no sentinel, and `Overlay` threads a CLAIMED-AXIS set so a record whose axis a higher-ranked record already took applies NOTHING. Without that set the last record to touch an axis won whatever its rank, so a rebound hammer beat a laboratory certificate whenever it happened to sort after it. A graded record therefore never wins an axis a measured record holds — it applies after.
- Law: ASSESSED BEATS PUBLISHED ONLY WHILE LIVE. A record whose expiry has passed the stated instant is excluded at SELECTION, so an expired mill certificate stops overriding without being deleted and a resolution at an earlier instant still honours it. The expiry day is INCLUSIVE: a certificate valid until a date still overrides ON that date and lapses the day after, which is how an issuer prints a validity period. The resolution instant is a CALLER input and never a system clock, so a resolution is replayable and two runs at one instant agree.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key)` — the ONE projector-facing resolution: it reads the two memoized catalogues (`MaterialPropertyCatalogue.Lookup` REQUIRED, `SustainabilityCatalogue.Lookup` optional-by-design), selects the LIVE records for the material at the stated instant, folds the winning record per axis by evidence rank, and returns the merged set. Both catalogue reads are frozen-dictionary reads and an empty assessed set short-circuits the overlay whole, so a project carrying no assessments reads exactly what the seed pages publish at the cost of two lookups.
- Boundary: this page never writes a catalogue row. A measured column REPLACES its named column by RE-ADMITTING through the same `Of*` family the catalogue used, so an assessed value crosses the identical band guard and an assessed material is never a less-admitted one; the record's evidence replaces the case's whole evidence, because the case now speaks for a measurement and leaving the catalogue's transcription evidence on it would attribute a field reading to a standards table. A material carrying no case for the named column is returned UNCHANGED rather than having one minted — a measured conductivity on a material the catalogue publishes no thermal case for is a column with no set to join, and inventing one would publish a thermal case whose other columns nothing measured. A graded record SCALES rather than replaces because a survey observes deterioration against the material's own basis: a `Severe` grade on a C30/37 slab yields `0.45 × f_ck` rather than a new characteristic strength, and modulus, yield, and ultimate scale while DENSITY and the two dimensionless ratios do not — deterioration does not change what a material weighs, and scaling Poisson's ratio is dimensionally meaningless. An EPD's UNDECLARED modules stay absent under its coverage census and fall back to the curated industry-average cell, which is the whole reason the curated rows are DEMOTED to fallback rather than deleted.

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
                admitted.GroupBy(static a => a.Identity.Material).ToFrozenDictionary(static g => g.Key, static g => toSeq(g))));

    // The LIVE selection at an instant, RANKED. Crossing to the BCL to compare two calendar dates is the phantom
    // this deletes: ToDateTimeUnspecified invents a midnight instant with no zone and a DateTime.MinValue sentinel
    // for the absent case, which sorted every record carrying no expiry beneath every record carrying one — and the
    // whole reason Taken exists is that expiry was never the tie key. ForAll over the absent expiry is what makes a
    // record with none unconditionally live rather than unconditionally stale.
    public Seq<Assessed> Live(MaterialId material, LocalDate at) =>
        ByMaterial.TryGetValue(material, out Seq<Assessed> records)
            ? toSeq(records.Filter(a => a.Identity.Expiry.ForAll(until => until >= at))
                .OrderByDescending(static a => a.Identity.Modality.Rank)
                .ThenByDescending(static a => a.Identity.Taken))
            : Seq<Assessed>();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class AssessmentResolution {
    // The ONE resolution: read both memoized catalogues, then let the live assessed records override per axis. The
    // engineering lookup is REQUIRED (a known structural material owes engineering properties), the lifecycle
    // lookup optional-by-design.
    public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key) =>
        from engineering in MaterialPropertyCatalogue.Lookup(id, key)
        from lifecycle in SustainabilityCatalogue.Lookup(id, key)
        from resolved in Overlay(engineering + lifecycle, assessed.Live(id, at), key)
        select resolved;

    // The per-axis fold, threading a CLAIMED-AXIS set beside the accumulating property sets. Live hands the records
    // in rank order, so the first record to reach an axis is the winner by construction and every later one is
    // skipped. The empty selection returns the published sets untouched — the overwhelming case, and the one the
    // advertised zero-cost assessed path rests on.
    static Fin<Seq<MaterialPropertySet>> Overlay(Seq<MaterialPropertySet> published, Seq<Assessed> live, Op key) =>
        live.IsEmpty
            ? Fin.Succ(published)
            : live.Fold(Fin.Succ((Sets: published, Claimed: Set<string>.Empty)), (state, record) => state.Bind(carried =>
                    carried.Claimed.Contains(record.Axis)
                        ? Fin.Succ(carried)
                        : Apply(carried.Sets, record, key).Map(sets => (Sets: sets, Claimed: carried.Claimed.Add(record.Axis)))))
                .Map(static carried => carried.Sets);

    // The generated TOTAL Switch over the admitted union: a fourth evidence shape is compiler-forced here, where
    // the property-probe switch it replaces carried a fallthrough arm that applied NOTHING — so a new shape landed
    // as a silent no-op rather than as a build break.
    static Fin<Seq<MaterialPropertySet>> Apply(Seq<MaterialPropertySet> sets, Assessed record, Op key) =>
        record.Switch(
            state: (Sets: sets, Key: key),
            column:    static (s, r) => ReplaceColumn(s.Sets, r, s.Key),
            retention: static (s, r) => ScaleMechanical(s.Sets, r, s.Key),
            lifecycle: static (s, r) => ReplaceEnvironmental(s.Sets, r, s.Key));

    // --- [PER_AXIS_RESOLUTION]
    // The measured column mints through the property row's OWN QuantityRow and lands through the property row's OWN
    // lens, so this arm holds no knowledge of which seam case owns which column and a new row cannot land in the
    // wrong one.
    static Fin<Seq<MaterialPropertySet>> ReplaceColumn(Seq<MaterialPropertySet> sets, Assessed.Column record, Op key) =>
        record.Property.Row.OfNative(record.Value.Central)
            .Bind(measure => record.Property.Landing(sets, measure, record.Identity.Evidence, key));

    // The two SEAT lenses the property vocabulary binds. Each takes its replacements as named optionals over the
    // case it owns, so a row declares WHICH column it seats at its own declaration site and the seat itself carries
    // no property discriminant at all.
    internal static Fin<Seq<MaterialPropertySet>> SeatMechanical(
        Seq<MaterialPropertySet> sets, PropertyEvidence evidence, Op key,
        Option<MeasureValue> density = default, Option<MeasureValue> youngs = default,
        Option<MeasureValue> yieldStrength = default, Option<MeasureValue> ultimate = default) =>
        Rebuild(sets, static set => set as MaterialPropertySet.Mechanical, mechanical =>
            MaterialPropertySet.OfMechanical(
                density.IfNone(mechanical.Density),
                youngs.IfNone(mechanical.YoungsModulus),
                yieldStrength.IfNone(mechanical.YieldStrength),
                ultimate.IfNone(mechanical.UltimateStrength),
                mechanical.PoissonsRatio, mechanical.ThermalExpansionPerK, key, evidence), key);

    internal static Fin<Seq<MaterialPropertySet>> SeatThermal(
        Seq<MaterialPropertySet> sets, PropertyEvidence evidence, Op key, MeasureValue conductivity) =>
        Rebuild(sets, static set => set as MaterialPropertySet.Thermal, thermal =>
            MaterialPropertySet.OfThermal(
                conductivity, thermal.SpecificHeat, thermal.UValue, thermal.VapourResistanceFactor,
                key, evidence, thermal.ConductivityCurve), key);

    // A graded record scales on the SI magnitude and re-mints through the same QuantityRow, so the scaled value
    // carries the same dimension and content-key shape as the value it replaced.
    static Fin<Seq<MaterialPropertySet>> ScaleMechanical(Seq<MaterialPropertySet> sets, Assessed.Retention record, Op key) =>
        Rebuild(sets, static set => set as MaterialPropertySet.Mechanical, mechanical =>
            from modulus in QuantityRow.Pressure.OfNative(mechanical.YoungsModulus.Si * record.Factor)
            from proof in QuantityRow.Pressure.OfNative(mechanical.YieldStrength.Si * record.Factor)
            from ultimate in QuantityRow.Pressure.OfNative(mechanical.UltimateStrength.Si * record.Factor)
            from scaled in MaterialPropertySet.OfMechanical(
                mechanical.Density, modulus, proof, ultimate,
                mechanical.PoissonsRatio, mechanical.ThermalExpansionPerK, key, record.Identity.Evidence)
            select scaled, key);

    // A declared EPD REPLACES the curated Environmental case at ITS OWN declared unit — never renormalized, because
    // an EPD is published per functional unit and a per-m2 membrane declaration is not a per-m3 one.
    static Fin<Seq<MaterialPropertySet>> ReplaceEnvironmental(Seq<MaterialPropertySet> sets, Assessed.Lifecycle record, Op key) =>
        MaterialPropertySet.OfEnvironmental(
                record.Basis,
                Impacts(sets.Choose(static set => set as MaterialPropertySet.Environmental).Head, record),
                record.Recycled.Map(static p => p.Central), record.Recovery.Map(static p => p.Central), key, record.Identity.Evidence)
            .Map(environmental => sets.Filter(static set => set is not MaterialPropertySet.Environmental).Add(environmental));

    // The impact matrix a declaration lands, under the SAME three dispositions the curated roster's own lowering
    // takes. A full thirteen-indicator declaration passes STRAIGHT through — CarbonMatrix is the partial-EPD
    // embedding and running it over an already-complete matrix would zero every indicator but GWP. A carbon row
    // whose census covers EVERY module already carries its own complete row and reads no curated cell, so the merge
    // is what a PARTIAL census earns; folding a full census through the curated row makes a complete declaration
    // depend on a fallback it can never reach.
    static ImmutableArray<double> Impacts(Option<MaterialPropertySet.Environmental> curated, Assessed.Lifecycle record) =>
        record.Matrix.IfNone(() => MaterialPropertySet.Environmental.CarbonMatrix(
            record.Coverage.Span.IndexOf(false) < 0 ? record.Modules : Merged(curated, record)));

    // The per-module carbon row the coverage census decides, ordered by the stage's OWN Index so the vector aligns
    // with the row-major matrix offset the seam CarbonMatrix writes — never the roster's declaration order. A
    // covered module reads its declared cell, an uncovered one the curated industry average, and an uncurated
    // material its own declared zero, which is the honest floor when no curated cell exists to fall back to.
    static ReadOnlyMemory<double> Merged(Option<MaterialPropertySet.Environmental> curated, Assessed.Lifecycle record) =>
        LifecycleStage.Items
            .OrderBy(static stage => stage.Index)
            .Select(stage => record.Coverage.Span[stage.Index]
                ? record.Modules.Span[stage.Index]
                : curated.Map(row => row.StageAt(stage)).IfNone(0.0))
            .ToArray()
            .AsMemory();

    // The ONE case-replacement shape every arm shares: find the case, rebuild it through its own seam admission,
    // and seat the rebuilt case where the old one stood. A material carrying no such case returns UNCHANGED.
    static Fin<Seq<MaterialPropertySet>> Rebuild<TCase>(
        Seq<MaterialPropertySet> sets, Func<MaterialPropertySet, TCase?> select,
        Func<TCase, Fin<MaterialPropertySet>> rebuild, Op key) where TCase : MaterialPropertySet =>
        sets.Choose(set => Optional(select(set))).Head.Match(
            Some: held => rebuild(held).Map(replaced => sets.Filter(set => !ReferenceEquals(set, held)).Add(replaced)),
            None: () => Fin.Succ(sets));
}
```

## [04]-[RESEARCH]

(none)
