# [MATERIALS_ASSESSMENT]

THE DATED-DECLARATION SOURCE. The two catalogue owners are CURATED: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` and `Properties/sustainability#SUSTAINABILITY_PROPERTY` seed the estate's known-material physics and lifecycle rows as in-fence published data under `SEED_ROW_LAW`, so every value they carry is as good as the standard behind it. A real project also carries data those rosters cannot hold: an in-situ rebound-hammer strength on a fifty-year-old slab, a laboratory certificate for one delivered batch, a manufacturer EPD for the exact product specified, a condition grade from a structural survey. Each is a MEASUREMENT with a date, a provenance, and an expiry rather than a standards row, and each must OVERRIDE the seed row for the material it describes without editing a curated catalogue. This owner is that third source: one `AssessmentRecord` `[Union]` closing the declaration modality, one `AssessmentAdmission` fold lowering an admitted record onto the SAME `Published<T>` carrier its engineering sibling declares, and one `AssessmentResolution` law resolving assessed over published per column, per material, at a stated instant. This page reads the two catalogue owners and writes NO catalogue row, so a curated roster stays curated and an assessment never mutates a standards table.

The `EpdRow` shape lands here rather than on the sustainability roster because a product EPD is a DECLARATION with an issuer, a declared unit, a module coverage census, and an expiry; the curated industry averages are its FALLBACK, demoted rather than deleted. The page re-mints NO seam type, admits NO `UnitsNet` quantity beyond the shared carrier's own arms, and rails ONE band — the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500) both sibling sources rail — so an assessed material and a catalogued material fault identically. Record TRANSPORT is the corpus `tests/contracts/MANIFEST.md` `[02.26]-[DECLARATION_RECORD]` DOMAIN contract — `python:data` `impact/declaration.md` the one producer, this page's `DeclarationWire.Decode` the committed consumer leg — so a declaration arrives as canonical JSON keyed to a `MaterialId`, lowers onto `EpdRow` and `AssessmentRecord.Declared`, and reaches `AssessmentSet.Of` unchanged. The peer impact wire stays impact-only by its own charter; the declaration contract carries what that frame structurally cannot — identity, dates, and the presence-censused cell map.

## [01]-[INDEX]

- [02]-[ASSESSMENT_RECORD]: the `AssessmentModality` provenance axis, the `ConditionGrade` survey vocabulary, the `AssessedProperty` axis with the landing lens each row owns, the `DeclarationUnit`/`EpdStandard`/`DeclarationSubtype` contract-token rosters, the `DeclaredImpacts` closed declaration-granularity family, the `EpdRow` product-declaration shape, the `AssessmentRecord` `[Union]` closing the three declaration modalities, the `AssessedIdentity` shared record identity, the `AssessmentAdmission.Admit` fold, and the `DeclarationWire` decode leg with its source-generated reader and its `DeclarationMap` transcription.
- [03]-[ASSESSED_RESOLUTION]: `AssessmentSet` the per-material record set, the assessed-over-published resolution law with its expiry and evidence-rank gates, and the `Resolve` entry the projector composes ahead of the two catalogue lookups.

## [02]-[ASSESSMENT_RECORD]

- Owner: `AssessmentModality` the closed provenance axis carrying each source's evidence rank, seam grade, and default relative band; `ConditionGrade` the survey condition vocabulary carrying its capacity-retention factor; `AssessedProperty` the assessable-column axis carrying its `QuantityRow` and its landing lens; `DeclarationUnit`/`EpdStandard`/`DeclarationSubtype` the contract's own frozen token rosters, pure vocabulary over one railed crossing; `DeclaredImpacts` the closed two-case family a declaration's granularity IS; `EpdRow` the product-declaration record; `AssessmentRecord` the closed declaration family; `AssessedIdentity` the identity every admitted record carries; `Assessed` the `[Union]` closing the three ADMITTED evidence shapes; `AssessmentAdmission` the ONE record→`Assessed` fold; `DeclarationWire` the transport leg over its `DeclarationRecordWire` reader shape and the `DeclarationMap` `[Mapper]`.
- Cases: `Measured` (a dated scalar result for ONE named property over a `MaterialId` — a rebound-hammer `f_c`, a coupon tensile, a core density — carrying its instrument-relative band and its `LocalDate`) · `Graded` (a survey `ConditionGrade` whose retention factor scales the resolved mechanical columns rather than replacing them) · `Declared` (an `EpdRow` product declaration replacing the curated lifecycle row for the material it names). A fourth modality is one case, one `Admit` arm, and one resolution arm — compiler-forced at all three.
- Law: A VOCABULARY ROW OWNS ITS OWN LANDING. Each `AssessedProperty` carries the lens that seats its measured column onto the seam case that owns it, so a new assessable property either declares where it lands or does not compile. A resolution that discriminated the landing centrally — one property routed to the thermal case and every other to the mechanical one — silently landed each new row in whichever branch the condition defaulted to, publishing a measured column on a case that does not own it, and the defect was invisible because the fold still type-checked.
- Law: A DECLARATION'S PROVENANCE DECIDES ITS SPREAD AND ITS GRADE. The modality row carries the evidence RANK that resolves a contest between two records, the default relative BAND the admitted value wears, and the seam `EvidenceGrade` the minted `PropertyEvidence` carries, so a rebound-hammer reading and a certified coupon are distinguishable at the seam `MeasureBand` without a second column and the `Rasm.Compute` propagation route reads the real spread instead of a precision no instrument had. A declaration is the one modality whose grade its OWN row cannot decide: representativeness does, so `DeclarationSubtype` carries the grade for the declared arm and an industry-average declaration enters at `EvidenceGrade.Catalogue` rather than wearing a product-specific attribution. Rank is a domain column, never a bent comparer.
- Law: THE TRANSPORT ADMITS ONCE, AND THE INTERIOR NEVER READS A DOCUMENT. `DeclarationWire.Decode` runs a source-generated `System.Text.Json` reader over the contract shape whose every required key is a `required` member, so a missing key refuses AT THE READER and no coalesced empty string re-authors an identity, a unit, or a registration the producer never sent. The whole read-and-transcribe funnels through ONE `Op.Catch`, so a malformed document and an unparsable ISO date park as typed refusals instead of escaping the `Fin` signature. Contract tokens stay TEXT on the ingress row and cross their rosters at `Admit` alone through ONE kernel-`AcceptValidated` crossing, so a delivery reports every bad token together rather than aborting on the first and no roster re-spells a keyed lookup the kernel owns.
- Entry: `AssessmentAdmission.Admit(record, key)` is the ONE admission — it proves each shape's own columns through the shared `Projection/fault#ADMISSION_SLOTS` slots over kernel `Band` rows, lifts every scalar onto `Published<T>` at the modality's own band with the evidence its row's grade names, and returns the neutral `Assessed` the resolution law folds. `AssessmentSet.Of(records, key)` admits a whole delivery in ONE `Traverse`, so a malformed record ABORTS the set rather than being dropped — a dropped assessment silently reverts to catalogue data. `DeclarationWire.Decode(record, key)` is the corpus-contract transport: the reader admits the payload once into `DeclarationRecordWire`, one banding fold sums the contract's fifteen modules onto the six-band `LifecycleStage` axis and PICKS the `DeclaredImpacts` arm its census earns, the `DeclarationMap` `[Mapper]` transcribes both onto `EpdRow`, and the decoded row crosses the SAME `Admit` gate an in-process record crosses.
- Packages: Rasm.Element (project — `MaterialId`, `MaterialPropertySet` + its `Of*` admissions, `MeasureValue`/`MeasureBand`, `PropertyEvidence.Of`, `EvidenceGrade`, `MeasurementBasis`, `LifecycleStage`, `ImpactCategory.Parse`, `ElementFault.ValueRejected`, and the `Projection/fault#ADMISSION_SLOTS` `Gate`/`Accumulate`/`Optional` slots this folder's first composer reaches), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of`, the two catalogue `Lookup` entries; SAME namespace so no import), Rasm.Materials.Component (project — `QuantityRow` + its `OfNative` railed mint), Rasm (project — `Op` + the `Op.Catch` boundary trap, and the kernel `Band` rows the scalar slots read), Riok.Mapperly (the ONE inbound `[Mapper]`, generator-only), NodaTime (`LocalDate` the declaration date and the expiry — a wall-clock date carries no zone, so an `Instant` stamped UTC is the deleted form — and `LocalDatePattern.Iso`), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Validation<Error,_>`/`Traverse`), BCL inbox (`System.Text.Json` source generation, `ReadOnlyMemory<double>`, `ImmutableArray<double>`, `FrozenDictionary`).
- Growth: a new declaration modality is one `AssessmentRecord` case with its `Admit` and resolution arms; a new survey scheme is one `ConditionGrade` row carrying its retention factor; a new EN 15804+A2 indicator is one seam `ImpactCategory` row the declaration's matrix widens against; a new assessable property is one `AssessedProperty` row carrying its `QuantityRow` and its lens, and a new `Seat` lens only where it reaches a seam case no existing lens rebuilds; a new contract token is one row on its owning roster, gaining its seam basis or matrix arity or declaring it has none; a new declaration GRANULARITY is one `DeclaredImpacts` case the generated Switch compiler-forces at fold, arity law, and resolution alike; a new contract COLUMN is one `DeclarationRecordWire` member the mapper's diagnostic forces onto `EpdRow` or an ignore row excuses. Never a per-modality record type, never a parallel assessed-material surface, never a second `Published` carrier or document reader.
- Boundary: an `AssessmentRecord` is INGRESS DATA, not a domain owner — `Admit` is its one `BOUNDARY_ADMISSION` and the interior sees only `Assessed`, so `EpdRow` holds the contract's tokens VERBATIM as text and every roster crossing happens at that gate. Every scalar rides `Published<T>`, so an assessed column and a seed column are ONE type at the seam. Expiry is a HARD gate at RESOLUTION and never at admission — an expired certificate is a historical record that stops overriding — and a record with no expiry never expires. `Attested` and `Run` stay absent: the contract declares neither, and filling either attributes a review nobody performed. The assessable axis is CARVED, not thin: a row needs a `QuantityRow`, so the seam's fractional-exponent columns — carbonation rate mm/sqrt-year, the ageing exponent — are unassessable, sqrt-time being inexpressible in the integer dimension vector; a durability survey assesses the chloride diffusivity and the seat carries those two untouched.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Text;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Materials.Component;
using Rasm.Numerics;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;

// --- [TYPES] -------------------------------------------------------------------------------
// Rank and Grade are DIFFERENT axes on purpose — Rank orders assessments against each other inside this page's
// resolution, Grade is the estate-wide attributable ladder ValueBag.Merge and PropertyEvidence.Citable read — so
// neither is derivable from the other and collapsing them would let a seam precedence decide an in-situ contest.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessmentModality {
    public static readonly AssessmentModality Survey         = new("survey",         rank: 1, relative: 0.30, grade: EvidenceGrade.Measured);   // visual/condition survey — the widest band
    public static readonly AssessmentModality NonDestructive = new("non-destructive", rank: 2, relative: 0.20, grade: EvidenceGrade.Measured);   // rebound hammer, UPV, cover meter
    public static readonly AssessmentModality Core           = new("core",           rank: 3, relative: 0.12, grade: EvidenceGrade.Measured);   // extracted core / in-situ coupon
    public static readonly AssessmentModality Laboratory     = new("laboratory",     rank: 4, relative: 0.05, grade: EvidenceGrade.Measured);   // certified batch test to the standard's own method
    // This row's grade is what a declaration with no stated class wears; a classed one reads DeclarationSubtype.Grade.
    public static readonly AssessmentModality Declaration    = new("declaration",    rank: 5, relative: 0.05, grade: EvidenceGrade.Import);
    public int Rank { get; }
    public double Relative { get; }
    public EvidenceGrade Grade { get; }
}

// A survey observes deterioration against the material's own basis rather than measuring a new value; the retention
// factor is the capacity fraction the grade admits.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConditionGrade {
    public static readonly ConditionGrade Sound        = new("sound",        retention: 1.00);
    public static readonly ConditionGrade Fair         = new("fair",         retention: 0.90);
    public static readonly ConditionGrade Deteriorated = new("deteriorated", retention: 0.70);
    public static readonly ConditionGrade Severe       = new("severe",       retention: 0.45);
    public double Retention { get; }
}

public delegate Fin<Seq<MaterialPropertySet>> PropertyLanding(
    Seq<MaterialPropertySet> sets, MeasureValue measure, PropertyEvidence evidence, Op key);

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
    // The fib Model Code D_RCM a chloride-profile survey returns — the durability column an assessment measures most
    // often after strength, and the one seam Durability column the MeasureValue axis can carry at all.
    public static readonly AssessedProperty ChlorideDiffusion = new("chloride-diffusion", QuantityRow.ChlorideDiffusivity,
        static (sets, measure, evidence, key) => AssessmentResolution.SeatDurability(sets, evidence, key, measure));
    public QuantityRow Row { get; }         // the ONE typed-mint owner the seam MeasureValue rides — never a local triple
    public PropertyLanding Landing { get; }
}

// --- [CONTRACT_TOKENS]
// The declaration contract's three closed token rosters, declared WHOLE rather than as the subset this consumer can
// seat: a token the contract does not name and a contract token this seam cannot express are different failures, and
// a lookup table holding only the mappable rows reports them as one. The keys ARE the frozen contract spellings.
// `DeclarationUnit` names the DECLARATION-CONTRACT altitude — the unit roster THIS seam's published declaration
// record admits (`t`, `l`, `pcs`, `m2r1` among them) — against the `Rasm.Compute` `Analysis/lifecycle#EC3_BOUNDARY`
// `DeclaredUnit`, the openEPD REST provider's own roster (`item`, `use`, `MJ`, `MPa`, `W`, `kgCO2e`, `t * km`,
// `m2 * RSI`). Neither key set contains the other and no mirror law joins them, so the two rosters carry two names
// and one spelling would invite a provider token crossing the contract rail unrejected.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeclarationUnit {
    public static readonly DeclarationUnit Kilogram        = new("kg",   Some(MeasurementBasis.PerKg));
    public static readonly DeclarationUnit Tonne           = new("t",    Option<MeasurementBasis>.None);
    public static readonly DeclarationUnit Metre           = new("m",    Option<MeasurementBasis>.None);
    public static readonly DeclarationUnit SquareMetre     = new("m2",   Some(MeasurementBasis.PerM2));
    public static readonly DeclarationUnit CubicMetre      = new("m3",   Some(MeasurementBasis.PerM3));
    public static readonly DeclarationUnit Litre           = new("l",    Option<MeasurementBasis>.None);
    public static readonly DeclarationUnit Piece           = new("pcs",  Some(MeasurementBasis.PerItem));
    public static readonly DeclarationUnit SquareMetreYear = new("m2r1", Option<MeasurementBasis>.None);
    // The seam basis this unit lands on, ABSENT where the four-basis closure holds no counterpart — never a per-kg
    // default, which would renormalize a declaration the contract publishes per running metre.
    public Option<MeasurementBasis> Basis { get; }
}

// The EPD standard revision, carrying the seam matrix arity ITS OWN indicator roster fills. A2 declares the thirteen
// A2 indicators the seam matrix axis is built from; A1's roster is a different characterization set that fills no
// seam matrix at all, so it declares NO arity and the full-matrix arm is unreachable for it BY THE COLUMN. The arity
// gate and the revision gate are therefore ONE read — the identity compare a two-row bool column would have needed
// never appears.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EpdStandard {
    public static readonly EpdStandard En15804A1 = new("en15804a1", Option<int>.None);
    public static readonly EpdStandard En15804A2 = new("en15804a2", Some(MaterialPropertySet.Environmental.MatrixArity));
    public Option<int> MatrixArity { get; }
}

// Representativeness decides ATTRIBUTABILITY: a specific-product declaration names an accountable product, while a
// generic or industry declaration is the same industry average the curated roster already publishes and enters at the
// catalogue grade, so an average never outranks a curated row merely by arriving as a declaration.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeclarationSubtype {
    public static readonly DeclarationSubtype Specific       = new("specific",       EvidenceGrade.Import);
    public static readonly DeclarationSubtype Representative = new("representative", EvidenceGrade.Import);
    public static readonly DeclarationSubtype Industry       = new("industry",       EvidenceGrade.Catalogue);
    public static readonly DeclarationSubtype Generic        = new("generic",        EvidenceGrade.Catalogue);
    public EvidenceGrade Grade { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The registry provenance an ingested declaration carries and an in-process one does not: the registry-native pair is
// the key two registries republishing ONE declaration differ on, which the programme pair cannot separate. The
// registry's own revision string is NOT carried — Issued is the revision discriminant the tie law already reads and
// Uuid the identity a join uses, so a version column would be a third spelling nothing consults. Two plain columns is
// what lets the contract's `source` object deserialize STRAIGHT into this record with no wire twin beside it.
public sealed record DeclarationOrigin(string Registry, string Uuid);

// WHAT a producer declared, as a CLOSED TWO-CASE FAMILY rather than a vector-and-matrix pair guarded against holding
// both. The carbon arm carries the per-EN-15978-module GWP vector beside the census of which modules the declaration
// actually DECLARES — a zero in an undeclared module is ABSENCE, and reading it as a measured zero is the
// fabricated-tally defect the census exists to prevent, while a parallel bool span rather than an Option per cell
// keeps the vector one contiguous read. The full arm carries the whole thirteen-indicator declaration. The
// vector-XOR-matrix invariant every earlier shape re-guarded at admission is now UNREPRESENTABLE: a row declaring its
// GWP twice cannot be built, so no rule has to guess which the producer meant, and a third declaration granularity
// lands as one case the generated Switch compiler-forces at every reader.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeclaredImpacts {
    private DeclaredImpacts() { }
    public sealed record Carbon(ReadOnlyMemory<double> Modules, ReadOnlyMemory<bool> Coverage) : DeclaredImpacts;
    public sealed record Full(ImmutableArray<double> Matrix) : DeclaredImpacts;
}

// The product-declaration row: the EPD facts a curated industry average cannot carry. Issuer and Registration are the
// declaration's PROGRAMME identity (the pair a procurement filter keys on), DeclaredUnit / Standard / Subtype the
// contract's own tokens held VERBATIM as ingress text and crossed at Admit (an EPD is published per functional unit
// and is admitted at THAT unit, never renormalized), Impacts the declared family whole, ValidUntil the calendar
// expiry the resolution law compares. The two resource fractions are Option — scenario data many declarations omit,
// absence never a fabricated fraction — mirroring the seam OfEnvironmental's own Option pair rather than forcing a
// zero the producer never declared. The declared PRODUCT NAME is deliberately not mirrored: it is a display fact this
// consumer joins on nowhere, and a column no arm reads is decorative.
public sealed record EpdRow(
    string Issuer,
    string Registration,
    string DeclaredUnit,
    string Standard,
    string Subtype,
    Option<DeclarationOrigin> Origin,
    DeclaredImpacts Impacts,
    Option<double> RecycledContent,
    Option<double> EndOfLifeRecovery,
    LocalDate Issued,          // the declaration's own issue date — the tie key two revisions of one registration differ on
    LocalDate ValidUntil) {
    public string Reference => Origin.Match(
        Some: static origin => $"{origin.Registry}:{origin.Uuid}",
        None: () => $"{Issuer}:{Registration}");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssessmentRecord {
    private AssessmentRecord() { }
    public sealed record Measured(MaterialId Material, AssessedProperty Property, double SiValue, AssessmentModality Modality, string Reference, LocalDate Taken, Option<LocalDate> ValidUntil) : AssessmentRecord;
    public sealed record Graded(MaterialId Material, ConditionGrade Grade, string Reference, LocalDate Taken, Option<LocalDate> ValidUntil) : AssessmentRecord;
    public sealed record Declared(MaterialId Material, EpdRow Epd) : AssessmentRecord;

    public MaterialId Subject => Switch(
        measured: static r => r.Material,
        graded: static r => r.Material,
        declared: static r => r.Material);

    public AssessmentModality Modality => Switch(
        measured: static r => r.Modality,
        graded: static _ => AssessmentModality.Survey,
        declared: static _ => AssessmentModality.Declaration);

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
        AssessedIdentity Identity, MeasurementBasis Basis, DeclaredImpacts Impacts,
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
    // The three arms accumulate nothing across each other (a record is one modality), but each arm's own independent
    // columns accumulate applicatively so a malformed EPD reports its arity, its tokens, AND its fractions together.
    // Band.Positive IS the finite-and-strictly-positive row: a hand double.IsFinite beside it re-spells the kernel.
    public static Fin<Assessed> Admit(AssessmentRecord record, Op key) => record.Switch(
        state: key,
        measured: static (k, r) => AdmissionSlots
            .Gate(Band.Positive.Admits(r.SiValue), k,
                $"<assessment-measured-nonpositive:{r.Material.Value}:{r.Property.Key}:{r.SiValue:R}>")
            .Map(_ => Column(Identity(r.Material, r.Modality, r.Reference, r.Taken, r.ValidUntil), r.Property, r.SiValue))
            .As().ToFin(),
        graded: static (k, r) => Fin.Succ<Assessed>(new Assessed.Retention(
            Identity(r.Material, AssessmentModality.Survey, r.Reference, r.Taken, r.ValidUntil), r.Grade.Retention)),
        declared: static (k, r) => Declared(r.Material, r.Epd, k));

    // The EPD arm's own admission. The vector-XOR-matrix guard every earlier shape spent a slot on is GONE — the
    // DeclaredImpacts union makes the both-declared state unbuildable — so what survives is per-arm ARITY, and each
    // arm's expectation is read off a ROW rather than restated: the carbon arm owes LifecycleStage arity on vector and
    // census alike (a short vector zero-pads downstream, publishing undeclared modules as measured zeros), the full
    // arm owes exactly the arity ITS OWN standard revision declares, so an A1 declaration reaches the matrix arm
    // through no arity at all. The three contract tokens cross their rosters HERE and nowhere else.
    static Fin<Assessed> Declared(MaterialId material, EpdRow epd, Op key) =>
        (Token<EpdStandard>(epd.Standard, "standard", key).Bind(standard => Arity(epd, standard, key)),
         Token<DeclarationUnit>(epd.DeclaredUnit, "unit", key).Bind(unit => unit.Basis.Match(
             Some: static basis => Success<Error, MeasurementBasis>(basis),
             None: () => Fail<Error, MeasurementBasis>(
                 new ElementFault.ValueRejected(key, $"<declaration-unit-unseated:{epd.DeclaredUnit}>")))),
         Token<DeclarationSubtype>(epd.Subtype, "subtype", key),
         AdmissionSlots.Gate(material.Value.Length > 0, key, $"<epd-material-blank:{epd.Reference}>"),
         AdmissionSlots.Optional(epd.RecycledContent, Band.Unit, "epd-recycled-content", key),
         AdmissionSlots.Optional(epd.EndOfLifeRecovery, Band.Unit, "epd-end-of-life-recovery", key))
            .Apply((_, basis, subtype, _, recycled, recovery) => Lifecycle(
                new AssessedIdentity(material, AssessmentModality.Declaration, EpdEvidence(epd, subtype), epd.Issued, Some(epd.ValidUntil)),
                basis, epd, recycled, recovery))
            .As()
            .ToFin();

    // ONE railed crossing for every contract roster: the kernel `Op.AcceptValidated` bridge runs the GENERATED keyed
    // lookup — no page re-spells a TryGet ladder the kernel already owns — and the refusal re-keys onto the seam band
    // this page rails, so an off-roster token carries band 2500 and its [DETAIL_GRAMMAR] token like every sibling
    // refusal. Three near-identical Parse bodies collapsed here; a fourth roster costs one call, not one method.
    static Validation<Error, T> Token<T>(string token, string axis, Op key)
        where T : IObjectFactory<T, string, ValidationError> =>
        key.AcceptValidated<T>(token).Match(
            Succ: static row => Success<Error, T>(row),
            Fail: _ => Fail<Error, T>(new ElementFault.ValueRejected(key, $"<declaration-{axis}-unknown:{token}>")));

    static Validation<Error, Unit> Arity(EpdRow epd, EpdStandard standard, Op key) => epd.Impacts.Switch(
        carbon: c => AdmissionSlots.Gate(
            c.Modules.Length == LifecycleStage.Count && c.Coverage.Length == LifecycleStage.Count, key,
            $"<epd-module-arity:{epd.Reference}:{c.Modules.Length}:{c.Coverage.Length}:expected={LifecycleStage.Count}>"),
        full: f => standard.MatrixArity.Match(
            Some: arity => AdmissionSlots.Gate(f.Matrix.Length == arity, key,
                $"<epd-matrix-arity:{epd.Reference}:{f.Matrix.Length}:expected={arity}>"),
            None: () => Fail<Error, Unit>(new ElementFault.ValueRejected(key,
                $"<epd-matrix-under-standard:{epd.Reference}:{standard.Key}>"))));

    // The identity mint: the evidence, the band, and the tie key are all decided HERE, so no later arm re-derives
    // provenance and no record carries two spellings of one fact. ONE PropertyEvidence.Of threads the expiry Option
    // straight through — the present/absent Match this replaces spelled the same fact twice and reached a positional
    // constructor the seam has since closed.
    static AssessedIdentity Identity(MaterialId material, AssessmentModality modality, string reference, LocalDate taken, Option<LocalDate> validUntil) =>
        new(material, modality,
            PropertyEvidence.Of(modality.Key, modality.Grade, Some(reference), validUntil),
            taken, validUntil);

    static Assessed Column(AssessedIdentity identity, AssessedProperty property, double si) =>
        new Assessed.Column(identity, property, Published.Of(si, identity.Modality.Relative, identity.Evidence));

    static Assessed Lifecycle(AssessedIdentity identity, MeasurementBasis basis, EpdRow epd, Option<double> recycled, Option<double> recovery) =>
        new Assessed.Lifecycle(
            identity, basis, epd.Impacts,
            recycled.Map(f => Published.Of(f, identity.Modality.Relative, identity.Evidence)),
            recovery.Map(f => Published.Of(f, identity.Modality.Relative, identity.Evidence)));

    // Provenance is SINGLE-stored on the seam evidence exactly as the sibling catalogues store theirs: the modality
    // key is the source, the registry-native or programme reference the identity, the expiry the LocalDate — never a
    // parallel per-record provenance column the seam would have to carry twice. The general Of carries the grade the
    // SUBTYPE row decides, where the sibling roster's PropertyEvidence.Declaration shorthand fixes it at Import for
    // rows whose representativeness the curator already settled.
    static PropertyEvidence EpdEvidence(EpdRow epd, DeclarationSubtype subtype) =>
        PropertyEvidence.Of("epd", subtype.Grade, Some(epd.Reference), Some(epd.ValidUntil));
}

// --- [DECLARATION_WIRE]
// The contract shape, member-for-member. The two fractions are the contract's only optional cells and cross as
// `double?`, absent meaning absent. `product` is the one contract key deliberately unmirrored (a display fact no
// arm joins on). No declared-quantity member exists: the contract admits a declaration at quantity ONE —
// the producer refuses any other amount at ingest — so every cell is already per one DeclaredUnit and this decoder
// rescales nothing; a qty column here would invite a renormalization the wire law forecloses at its source.
public sealed record DeclarationRecordWire {
    public required string MaterialKey { get; init; }
    public required string Issuer { get; init; }
    public required string Registration { get; init; }
    public required string DeclaredUnit { get; init; }
    public required string Standard { get; init; }
    public required string Subtype { get; init; }
    public required string Issued { get; init; }
    public required string ValidUntil { get; init; }
    public required Dictionary<string, Dictionary<string, double>> Indicators { get; init; }
    public required DeclarationOrigin Source { get; init; }
    public double? RecycledContent { get; init; }
    public double? EndOfLifeRecovery { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(DeclarationRecordWire))]
internal sealed partial class DeclarationJson : JsonSerializerContext;

// The ONE [Mapper] on this seam: a pure SHAPE transcription with no domain decision inside it. Its mapping is
// READER-FREE, so RMG020 keeps source-side force and the method's two ignore rows are the whole authored inventory. No
// conversion knob is re-spelled here — the folder's assembly MapperDefaults owns EnabledConversions, and this seam is
// inbound-only, so LanguageExt's throwing explicit Option<T> cast is unreachable and excluding it would be hygiene.
[Mapper]
public static partial class DeclarationMap {
    // The material key lands on the union case, not on the row; the indicator block reaches the row already folded, so
    // the whole impact declaration crosses as ONE member instead of three parallel columns.
    [MapperIgnoreSource(nameof(DeclarationRecordWire.MaterialKey))]
    [MapperIgnoreSource(nameof(DeclarationRecordWire.Indicators))]
    [MapProperty(nameof(impacts), nameof(EpdRow.Impacts))]
    [MapProperty(nameof(DeclarationRecordWire.Source), nameof(EpdRow.Origin), Use = nameof(Provenance))]
    [MapProperty(nameof(DeclarationRecordWire.Issued), nameof(EpdRow.Issued), Use = nameof(Iso))]
    [MapProperty(nameof(DeclarationRecordWire.ValidUntil), nameof(EpdRow.ValidUntil), Use = nameof(Iso))]
    public static partial EpdRow ToEpd(DeclarationRecordWire wire, DeclaredImpacts impacts);

    // Per-TYPE non-generic carriers, never a generic T? -> Option<T> (RMG001 refuses it wholesale).
    [UserMapping] static Option<double> Fraction(double? value) => Optional(value);
    [UserMapping] static Option<DeclarationOrigin> Provenance(DeclarationOrigin source) => Some(source);

    // The contract declares `format: date`, so a value this pattern cannot read is a CONTRACT breach, not a domain
    // fault: the raise lands in Decode's one Op.Catch beside the reader's own, and no arm invents a default date.
    [UserMapping] static LocalDate Iso(string text) => LocalDatePattern.Iso.Parse(text).GetValueOrThrow();
}

public static class DeclarationWire {
    static readonly FrozenDictionary<string, LifecycleStage> Bands = new Dictionary<string, LifecycleStage> {
        ["a1-a3"] = LifecycleStage.A1A3, ["a4"] = LifecycleStage.A4, ["a5"] = LifecycleStage.A5,
        ["b1"] = LifecycleStage.B, ["b2"] = LifecycleStage.B, ["b3"] = LifecycleStage.B, ["b4"] = LifecycleStage.B,
        ["b5"] = LifecycleStage.B, ["b6"] = LifecycleStage.B, ["b7"] = LifecycleStage.B,
        ["c1"] = LifecycleStage.C, ["c2"] = LifecycleStage.C, ["c3"] = LifecycleStage.C, ["c4"] = LifecycleStage.C,
        ["d"] = LifecycleStage.D,
    }.ToFrozenDictionary();

    public static Fin<AssessmentRecord> Decode(ReadOnlyMemory<byte> record, Op key) =>
        key.Catch(() =>
            from wire in Read(record, key)
            from impacts in Banded(wire.Indicators, key)
            select (AssessmentRecord)new AssessmentRecord.Declared(
                MaterialId.Of(wire.MaterialKey), DeclarationMap.ToEpd(wire, impacts)));

    static Fin<DeclarationRecordWire> Read(ReadOnlyMemory<byte> record, Op key) =>
        Optional(JsonSerializer.Deserialize(record.Span, DeclarationJson.Default.DeclarationRecordWire))
            .Match(
                Some: static wire => Fin.Succ(wire),
                None: () => new ElementFault.ValueRejected(key, "<declaration-record-null>"));

    static Fin<DeclaredImpacts> Banded(Dictionary<string, Dictionary<string, double>> indicators, Op key);
    // Per indicator: resolve the row token through the seam ImpactCategory.Parse and each module token through the
    // Bands rows — an unknown token on either axis refuses typed rather than being dropped into a silent zero — then
    // sum declared cells into each LifecycleStage band and cover a band on any member's presence. Complete coverage
    // across all thirteen indicator rows and all six bands yields the Full arm (row-major, indicator × stage); any
    // partial census yields the Carbon arm off the gwp-total row alone. The fold PICKS an arm rather than filling two
    // slots, so the exclusivity it used to owe an admission gate is now the shape of its own return type.
}
```

## [03]-[ASSESSED_RESOLUTION]

- Owner: `AssessmentSet` the admitted per-material record index; `AssessmentResolution` the assessed-over-published law with its per-axis `ReplaceColumn`/`ScaleMechanical`/`ReplaceEnvironmental` arms over one shared `Rebuild` re-admission, the three `Seat` lenses the property vocabulary binds, and the `Resolve` entry the projector composes.
- Cases: three resolution arms over the `Assessed` `[Union]`'s own three cases — `Column` replaces the catalogue column it names, `Retention` scales the resolved mechanical strength columns, `Lifecycle` replaces the curated `Environmental` case on the `DeclaredImpacts` generated Switch — the full arm passes straight through, the carbon arm merges under its coverage census. The `Cost` case is untouched: an EPD declares environmental impact and never a unit price. `Apply` is the generated total `Switch`, so a fourth evidence shape is compiler-forced rather than silently no-op.
- Law: RANK DECIDES, IT DOES NOT MERELY ORDER. Ranking is `AssessmentModality.Rank` first and the later `Taken` declaration date second, both compared as `LocalDate` in its own calendar with no BCL crossing and no sentinel, and `Overlay` threads a CLAIMED-AXIS set so a record whose axis a higher-ranked record already took applies NOTHING. Without that set the last record to touch an axis won whatever its rank, so a rebound hammer beat a laboratory certificate whenever it happened to sort after it. A graded record therefore never wins an axis a measured record holds — it applies after.
- Law: ASSESSED BEATS PUBLISHED ONLY WHILE LIVE. A record whose expiry has passed the stated instant is excluded at SELECTION, so an expired mill certificate stops overriding without being deleted and a resolution at an earlier instant still honours it. The expiry day is INCLUSIVE: a certificate valid until a date still overrides ON that date and lapses the day after, which is how an issuer prints a validity period. The resolution instant is a CALLER input and never a system clock, so a resolution is replayable and two runs at one instant agree.
- Entry: `public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key)` — the ONE projector-facing resolution: it reads the two memoized catalogues (`MaterialPropertyCatalogue.Lookup` REQUIRED, `SustainabilityCatalogue.Lookup` optional-by-design), selects the LIVE records for the material at the stated instant, folds the winning record per axis by evidence rank, and returns the merged set. Both catalogue reads are frozen-dictionary reads and an empty assessed set short-circuits the overlay whole, so a project carrying no assessments reads exactly what the seed pages publish at the cost of two lookups.
- Boundary: this page never writes a catalogue row. A measured column REPLACES its named column by RE-ADMITTING through the same `Of*` family the catalogue used, so an assessed value crosses the identical band guard; the record's evidence replaces the case's whole evidence, because leaving the catalogue's transcription evidence on it attributes a field reading to a standards table. A material carrying no case for the named column returns UNCHANGED — inventing a thermal case for a measured conductivity publishes columns nothing measured. A graded record SCALES rather than replaces: modulus, yield, and ultimate scale while DENSITY and the two dimensionless ratios do not, since deterioration does not change what a material weighs and scaling Poisson's ratio is dimensionally meaningless. An EPD's UNDECLARED modules stay absent under its coverage census and fall back to the curated industry-average cell — the whole reason the curated rows are DEMOTED to fallback rather than deleted.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record AssessmentSet(FrozenDictionary<MaterialId, Seq<Assessed>> ByMaterial) {
    public static readonly AssessmentSet Empty = new(FrozenDictionary<MaterialId, Seq<Assessed>>.Empty);

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
    public static Fin<Seq<MaterialPropertySet>> Resolve(MaterialId id, AssessmentSet assessed, LocalDate at, Op key) =>
        from engineering in MaterialPropertyCatalogue.Lookup(id, key)
        from lifecycle in SustainabilityCatalogue.Lookup(id, key)
        from resolved in Overlay(engineering + lifecycle, assessed.Live(id, at), key)
        select resolved;

    // Live hands the records in rank order, so the first record to reach an axis is the winner by construction.
    static Fin<Seq<MaterialPropertySet>> Overlay(Seq<MaterialPropertySet> published, Seq<Assessed> live, Op key) =>
        live.IsEmpty
            ? Fin.Succ(published)
            : live.Fold(Fin.Succ((Sets: published, Claimed: Set<string>.Empty)), (state, record) => state.Bind(carried =>
                    carried.Claimed.Contains(record.Axis)
                        ? Fin.Succ(carried)
                        : Apply(carried.Sets, record, key).Map(sets => (Sets: sets, Claimed: carried.Claimed.Add(record.Axis)))))
                .Map(static carried => carried.Sets);

    // The property-probe switch this replaces carried a fallthrough arm that applied NOTHING, so a new shape landed
    // as a silent no-op rather than as a build break.
    static Fin<Seq<MaterialPropertySet>> Apply(Seq<MaterialPropertySet> sets, Assessed record, Op key) =>
        record.Switch(
            state: (Sets: sets, Key: key),
            column:    static (s, r) => ReplaceColumn(s.Sets, r, s.Key),
            retention: static (s, r) => ScaleMechanical(s.Sets, r, s.Key),
            lifecycle: static (s, r) => ReplaceEnvironmental(s.Sets, r, s.Key));

    // --- [PER_AXIS_RESOLUTION]
    static Fin<Seq<MaterialPropertySet>> ReplaceColumn(Seq<MaterialPropertySet> sets, Assessed.Column record, Op key) =>
        record.Property.Row.OfNative(record.Value.Central)
            .Bind(measure => record.Property.Landing(sets, measure, record.Identity.Evidence, key));

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

    // The fib Model Code service-life case. OfDurability declares ONE raw-double arity — the chloride column mints on
    // its L2T-1 signature INSIDE that admission — so the seat hands the admitted SI magnitude across rather than
    // re-minting a MeasureValue the factory would mint again.
    internal static Fin<Seq<MaterialPropertySet>> SeatDurability(
        Seq<MaterialPropertySet> sets, PropertyEvidence evidence, Op key, MeasureValue chlorideDiffusion) =>
        Rebuild(sets, static set => set as MaterialPropertySet.Durability, durability =>
            MaterialPropertySet.OfDurability(
                durability.CarbonationRateMmPerSqrtYear, chlorideDiffusion.Si, durability.AgeingExponent,
                key, evidence), key);

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

    static Fin<Seq<MaterialPropertySet>> ReplaceEnvironmental(Seq<MaterialPropertySet> sets, Assessed.Lifecycle record, Op key) =>
        MaterialPropertySet.OfEnvironmental(
                record.Basis,
                Impacts(sets.Choose(static set => set as MaterialPropertySet.Environmental).Head, record),
                record.Recycled.Map(static p => p.Central), record.Recovery.Map(static p => p.Central), key, record.Identity.Evidence)
            .Map(environmental => sets.Filter(static set => set is not MaterialPropertySet.Environmental).Add(environmental));

    // The impact matrix a declaration lands, on the DECLARED family's own generated Switch. A full thirteen-indicator
    // declaration passes STRAIGHT through — CarbonMatrix is the partial-EPD embedding and running it over an
    // already-complete matrix would zero every indicator but GWP. A carbon row whose census covers EVERY module
    // already carries its own complete row and reads no curated cell, so the merge is what a PARTIAL census earns;
    // folding a full census through the curated row makes a complete declaration depend on a fallback it can never
    // reach. The disposition ladder an Option probe used to spell is now two total arms plus one census test.
    static ImmutableArray<double> Impacts(Option<MaterialPropertySet.Environmental> curated, Assessed.Lifecycle record) =>
        record.Impacts.Switch(
            full: static f => f.Matrix,
            carbon: c => MaterialPropertySet.Environmental.CarbonMatrix(
                c.Coverage.Span.IndexOf(false) < 0 ? c.Modules : Merged(curated, c)));

    // The per-module carbon row the coverage census decides, ordered by the stage's OWN Index so the vector aligns
    // with the row-major matrix offset the seam CarbonMatrix writes — never the roster's declaration order. A
    // covered module reads its declared cell, an uncovered one the curated industry average, and an uncurated
    // material its own declared zero, which is the honest floor when no curated cell exists to fall back to.
    static ReadOnlyMemory<double> Merged(Option<MaterialPropertySet.Environmental> curated, DeclaredImpacts.Carbon declared) =>
        LifecycleStage.Items
            .OrderBy(static stage => stage.Index)
            .Select(stage => declared.Coverage.Span[stage.Index]
                ? declared.Modules.Span[stage.Index]
                : curated.Map(row => row.StageAt(stage)).IfNone(0.0))
            .ToArray()
            .AsMemory();

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
