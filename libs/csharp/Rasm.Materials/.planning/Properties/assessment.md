# [MATERIALS_ASSESSMENT]

THE DATED-DECLARATION SOURCE. The two catalogue owners are CURATED: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` and `Properties/sustainability#SUSTAINABILITY_PROPERTY` seed the estate's known-material physics and lifecycle rows as in-fence published data under `SEED_ROW_LAW`, so every value they carry is as good as the standard behind it. A real project also carries data those rosters cannot hold: an in-situ rebound-hammer strength on a fifty-year-old slab, a laboratory certificate for one delivered batch, a manufacturer EPD for the exact product specified, a condition grade from a structural survey. Each is a MEASUREMENT with a date, a provenance, and an expiry rather than a standards row, and each must OVERRIDE the seed row for the material it describes without editing a curated catalogue. This owner is that third source: one `AssessmentRecord` `[Union]` closing the declaration modality, one `AssessmentAdmission` fold lowering an admitted record onto the SAME `Published<T>` carrier its engineering sibling declares, and one `AssessmentResolution` law resolving assessed over published per column, per material, at a stated instant. This page reads the two catalogue owners and writes NO catalogue row, so a curated roster stays curated and an assessment never mutates a standards table.

The `EpdRow` shape lands here rather than on the sustainability roster because a product EPD is a DECLARATION with an issuer, a declared unit, a module coverage census, and an expiry; the curated industry averages are its FALLBACK, demoted rather than deleted. The page re-mints NO seam type, admits NO `UnitsNet` quantity beyond the shared carrier's own arms, and rails ONE band — the seam `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (2500) both sibling sources rail — so an assessed material and a catalogued material fault identically. Record TRANSPORT is the corpus `tests/contracts/manifest.json` `declaration-record` DOMAIN contract — `python:data` `impact/declaration.md` the one producer, this page's `DeclarationWire.Decode` the committed consumer leg — so a declaration arrives as protobuf binary, parses under the declared size and recursion ceiling, validates from its generated descriptor, lowers onto `EpdRow` and `AssessmentRecord.Declared`, and reaches `AssessmentSet.Of` unchanged. The peer impact wire stays impact-only by its own charter; the declaration contract carries what that frame structurally cannot — identity, dates, and the presence-censused cell sequence.

## [01]-[INDEX]

- [02]-[ASSESSMENT_RECORD]: the `AssessmentModality` provenance axis, the `ConditionGrade` survey vocabulary, the `AssessedProperty` axis with the landing lens each row owns, the generated declaration vocabularies with their domain projections, the `DeclaredImpacts` closed declaration-granularity family, the `EpdRow` product-declaration shape, the `AssessmentRecord` `[Union]` closing the three declaration modalities, the `AssessedIdentity` shared record identity, the `AssessmentAdmission.Admit` fold, and the `DeclarationWire` protobuf-binary admission.
- [03]-[ASSESSED_RESOLUTION]: `AssessmentSet` the per-material record set, the assessed-over-published resolution law with its expiry and evidence-rank gates, and the `Resolve` entry the projector composes ahead of the two catalogue lookups.

## [02]-[ASSESSMENT_RECORD]

- Owner: `AssessmentModality` the closed provenance axis carrying each source's evidence rank, seam grade, and default relative band; `ConditionGrade` the survey condition vocabulary carrying its capacity-retention factor; `AssessedProperty` the assessable-column axis carrying its `QuantityRow` and its landing lens; the generated `DeclaredUnit`/`Standard`/`Subtype` enums the contract vocabularies and `DeclarationProfile` their domain-only basis, arity, and grade projection; `DeclaredImpacts` the closed two-case family a declaration's granularity IS; `EpdRow` the product-declaration record; `AssessmentRecord` the closed declaration family; `AssessedIdentity` the identity every admitted record carries; `Assessed` the `[Union]` closing the three ADMITTED evidence shapes; `AssessmentAdmission` the ONE record→`Assessed` fold; `DeclarationWire` the protobuf-binary transport leg over the generated `DeclarationRecord`.
- Cases: `Measured` (a dated scalar result for ONE named property over a `MaterialId` — a rebound-hammer `f_c`, a coupon tensile, a core density — carrying its instrument-relative band and its `LocalDate`) · `Graded` (a survey `ConditionGrade` whose retention factor scales the resolved mechanical columns rather than replacing them) · `Declared` (an `EpdRow` product declaration replacing the curated lifecycle row for the material it names). A fourth modality is one case, one `Admit` arm, and one resolution arm — compiler-forced at all three.
- Law: A VOCABULARY ROW OWNS ITS OWN LANDING. Each `AssessedProperty` carries the lens that seats its measured column onto the seam case that owns it, so a new assessable property either declares where it lands or does not compile. A resolution that discriminated the landing centrally — one property routed to the thermal case and every other to the mechanical one — silently landed each new row in whichever branch the condition defaulted to, publishing a measured column on a case that does not own it, and the defect was invisible because the fold still type-checked.
- Law: A DECLARATION'S PROVENANCE DECIDES ITS SPREAD AND ITS GRADE. The modality row carries the evidence RANK that resolves a contest between two records, the default relative BAND the admitted value wears, and the seam `EvidenceGrade` the minted `PropertyEvidence` carries, so a rebound-hammer reading and a certified coupon are distinguishable at the seam `MeasureBand` without a second column and the `Rasm.Compute` propagation route reads the real spread instead of a precision no instrument had. A declaration is the one modality whose grade its OWN row cannot decide: the generated `Subtype` value projects representativeness once, so an industry-average declaration enters at `EvidenceGrade.Catalogue` rather than wearing a product-specific attribution. Rank is a domain column, never a bent comparer.
- Law: THE TRANSPORT ADMITS ONCE, AND THE INTERIOR NEVER READS A DOCUMENT. `DeclarationWire.Decode` parses protobuf binary through `CodedInputStream.CreateWithLimits`, then the app spine's neutral `WireAdmission.Admit` descriptor evaluator admits the generated message before a domain projection runs. The whole read and date projection funnels through ONE `Op.Catch`, so malformed protobuf, invalid contract fields, and invalid calendar values park as typed refusals instead of escaping the `Fin` signature. No C# record, JSON context, enum roster, parser, validator, or per-field check restates the descriptor.
- Entry: `AssessmentAdmission.Admit(record, key)` is the ONE domain admission — it proves each in-process shape's own columns through the shared `Projection/fault#ADMISSION_SLOTS` slots over kernel `Band` rows, lifts every scalar onto `Published<T>` at the modality's own band with the evidence its row's grade names, and returns the neutral `Assessed` the resolution law folds. `AssessmentSet.Of(records, key)` admits a whole delivery in ONE `Traverse`, so a malformed record ABORTS the set rather than being dropped — a dropped assessment silently reverts to catalogue data. `DeclarationWire.Decode(record, key)` is the corpus-contract transport: bounded binary parse and generated-rule admission run once, one banding fold sums the contract's fifteen modules onto the six-band `LifecycleStage` axis and PICKS the `DeclaredImpacts` arm its census earns, one direct projection creates `EpdRow`, and the decoded row crosses the SAME `Admit` gate an in-process record crosses.
- Packages: Rasm.Contracts (project — generated `DeclarationRecord`, `ImpactCell`, and enum vocabularies), Rasm.AppHost (project — neutral `WireAdmission.Admit` descriptor evaluation and `WireBoundary`), Google.Protobuf (`CodedInputStream.CreateWithLimits` and generated parser), CommunityToolkit.HighPerformance (`ReadOnlyMemory<byte>.AsStream` bridge to the shared stream door), NodaTime.Serialization.Protobuf (`Date.ToLocalDate`), Rasm.Element (project — `MaterialId`, `MaterialPropertySet` + its `Of*` admissions, `MeasureValue`/`MeasureBand`, `PropertyEvidence.Of`, `EvidenceGrade`, `MeasurementBasis`, `LifecycleStage`, `ImpactCategory`, `ElementFault.ValueRejected`, and the `Projection/fault#ADMISSION_SLOTS` `Gate`/`Accumulate`/`Optional` slots this folder's first composer reaches), Rasm.Materials.Properties (project-local — the shared `Published<T>` carrier + `Published.Of`, the two catalogue `Lookup` entries; SAME namespace so no import), Rasm.Materials.Component (project — `QuantityRow` + its `OfNative` railed mint), Rasm (project — `Op` + the `Op.Catch` boundary trap, and the kernel `Band` rows the scalar slots read), NodaTime (`LocalDate` the declaration date and the expiry), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Validation<Error,_>`/`Traverse`), BCL inbox (`ReadOnlyMemory<double>`, `ImmutableArray<double>`, `FrozenDictionary`).
- Growth: a new declaration modality is one `AssessmentRecord` case with its `Admit` and resolution arms; a new survey scheme is one `ConditionGrade` row carrying its retention factor; a new EN 15804+A2 indicator is one corpus enum member and one seam `ImpactCategory` row at the matching ordinal; a new assessable property is one `AssessedProperty` row carrying its `QuantityRow` and its lens, and a new `Seat` lens only where it reaches a seam case no existing lens rebuilds; a new contract enum member gains only a domain projection when this consumer can seat it; a new declaration GRANULARITY is one `DeclaredImpacts` case the generated Switch compiler-forces at fold, arity law, and resolution alike; a new contract COLUMN is consumed only when an interior decision reads it. Never a per-modality record type, never a parallel assessed-material surface, never a second `Published` carrier or document reader.
- Boundary: an `AssessmentRecord` is INGRESS DATA, not a domain owner — `Admit` is its one `BOUNDARY_ADMISSION` and the interior sees only `Assessed`; generated contract values cross once onto `EpdRow`, and no generated message survives that projection. Every scalar rides `Published<T>`, so an assessed column and a seed column are ONE type at the seam. Expiry is a HARD gate at RESOLUTION and never at admission — an expired certificate is a historical record that stops overriding — and a record with no expiry never expires. `Attested` and `Run` stay absent: the contract declares neither, and filling either attributes a review nobody performed. The assessable axis is CARVED, not thin: a row needs a `QuantityRow`, so the seam's fractional-exponent columns — carbonation rate mm/sqrt-year, the ageing exponent — are unassessable, sqrt-time being inexpressible in the integer dimension vector; a durability survey assesses the chloride diffusivity and the seat carries those two untouched.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Serialization.Protobuf;
using Rasm.AppHost.Runtime;
using Rasm.Contracts.Declaration;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Materials.Component;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Properties;

public sealed record WireLimits(int SizeLimit, int RecursionLimit) {
    public static readonly WireLimits Declaration = new(4 << 20, 100);
}

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
    // This row's grade is what a declaration with no stated class wears; a classed one reads DeclarationProfile.Grade.
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

// --- [CONTRACT_PROJECTION] -----------------------------------------------------------------
// The generated enums own the contract rosters. This projection declares only what the Materials domain adds:
// which declared units its four-basis seam can seat, which revision owns the A2 matrix, and attribution grade.
public static class DeclarationProfile {
    public static Option<MeasurementBasis> Basis(DeclaredUnit unit) => unit switch {
        DeclaredUnit.Kg => Some(MeasurementBasis.PerKg),
        DeclaredUnit.M2 => Some(MeasurementBasis.PerM2),
        DeclaredUnit.M3 => Some(MeasurementBasis.PerM3),
        DeclaredUnit.Pcs => Some(MeasurementBasis.PerItem),
        _ => None,
    };

    public static Option<int> MatrixArity(Standard standard) => standard == Standard.En15804A2
        ? Some(MaterialPropertySet.Environmental.MatrixArity)
        : None;

    public static EvidenceGrade Grade(Subtype subtype) => subtype is Subtype.Specific or Subtype.Representative
        ? EvidenceGrade.Import
        : EvidenceGrade.Catalogue;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The registry provenance an ingested declaration carries and an in-process one does not: the registry-native pair is
// the key two registries republishing ONE declaration differ on, which the programme pair cannot separate. The
// registry's own revision string is NOT carried — Issued is the revision discriminant the tie law already reads and
// Uuid the identity a join uses, so a version column would be a third spelling nothing consults. The generated source
// message projects once into these two domain columns; no peer schema or serialization annotation sits beside it.
public sealed record DeclarationOrigin(Registry Registry, string Uuid);

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
// generated contract values projected directly into the domain (an EPD is published per functional unit and is
// admitted at THAT unit, never renormalized), Impacts the declared family whole, ValidUntil the calendar
// expiry the resolution law compares. The two resource fractions are Option — scenario data many declarations omit,
// absence never a fabricated fraction — mirroring the seam OfEnvironmental's own Option pair rather than forcing a
// zero the producer never declared. The declared PRODUCT NAME is deliberately not mirrored: it is a display fact this
// consumer joins on nowhere, and a column no arm reads is decorative.
public sealed record EpdRow(
    string Issuer,
    string Registration,
    DeclaredUnit DeclaredUnit,
    Standard Standard,
    Subtype Subtype,
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
    // through no arity at all. The generated enums already crossed the descriptor gate before this domain projection.
    static Fin<Assessed> Declared(MaterialId material, EpdRow epd, Op key) =>
        (Arity(epd, key),
         DeclarationProfile.Basis(epd.DeclaredUnit).Match(
             Some: static basis => Success<Error, MeasurementBasis>(basis),
             None: () => Fail<Error, MeasurementBasis>(
                 new ElementFault.ValueRejected(key, $"<declaration-unit-unseated:{epd.DeclaredUnit}>"))),
         AdmissionSlots.Gate(material.Value.Length > 0, key, $"<epd-material-blank:{epd.Reference}>"),
         AdmissionSlots.Optional(epd.RecycledContent, Band.Unit, "epd-recycled-content", key),
         AdmissionSlots.Optional(epd.EndOfLifeRecovery, Band.Unit, "epd-end-of-life-recovery", key))
            .Apply((_, basis, _, recycled, recovery) => Lifecycle(
                new AssessedIdentity(material, AssessmentModality.Declaration, EpdEvidence(epd), epd.Issued, Some(epd.ValidUntil)),
                basis, epd, recycled, recovery))
            .As()
            .ToFin();

    static Validation<Error, Unit> Arity(EpdRow epd, Op key) => epd.Impacts.Switch(
        carbon: c => AdmissionSlots.Gate(
            c.Modules.Length == LifecycleStage.Count && c.Coverage.Length == LifecycleStage.Count, key,
            $"<epd-module-arity:{epd.Reference}:{c.Modules.Length}:{c.Coverage.Length}:expected={LifecycleStage.Count}>"),
        full: f => DeclarationProfile.MatrixArity(epd.Standard).Match(
            Some: arity => AdmissionSlots.Gate(f.Matrix.Length == arity, key,
                $"<epd-matrix-arity:{epd.Reference}:{f.Matrix.Length}:expected={arity}>"),
            None: () => Fail<Error, Unit>(new ElementFault.ValueRejected(key,
                $"<epd-matrix-under-standard:{epd.Reference}:{epd.Standard}>"))));

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
    static PropertyEvidence EpdEvidence(EpdRow epd) =>
        PropertyEvidence.Of("epd", DeclarationProfile.Grade(epd.Subtype), Some(epd.Reference), Some(epd.ValidUntil));
}

// --- [DECLARATION_WIRE] --------------------------------------------------------------------
public static class DeclarationWire {
    public static Fin<AssessmentRecord> Decode(ReadOnlyMemory<byte> record, Op key) =>
        key.Catch(() => Fin.Succ(DeclarationRecord.Parser.ParseFrom(
                CodedInputStream.CreateWithLimits(
                    record.AsStream(), WireLimits.Declaration.SizeLimit, WireLimits.Declaration.RecursionLimit))))
            .Bind(admitted => WireAdmission.Admit(admitted, WireBoundary.InboundPayload, key))
            .Map(admitted => (AssessmentRecord)new AssessmentRecord.Declared(
                MaterialId.Of(admitted.MaterialKey), ToEpd(admitted, Banded(admitted.Cells))));

    static EpdRow ToEpd(DeclarationRecord wire, DeclaredImpacts impacts) => new(
        wire.Issuer,
        wire.Registration,
        wire.DeclaredUnit,
        wire.Standard,
        wire.Subtype,
        Some(new DeclarationOrigin(wire.Source.Registry, wire.Source.Uuid)),
        impacts,
        wire.HasRecycledContent ? Some(wire.RecycledContent) : None,
        wire.HasEndOfLifeRecovery ? Some(wire.EndOfLifeRecovery) : None,
        wire.Issued.ToLocalDate(),
        wire.ValidUntil.ToLocalDate());

    // The generated enum ordinals are the schema's canonical indicator order. Module ordinals collapse the fifteen
    // EN 15978 cells onto the seam's six lifecycle bands; no token roster or string parser stands beside the schema.
    static DeclaredImpacts Banded(IEnumerable<ImpactCell> cells) {
        double[] matrix = new double[MaterialPropertySet.Environmental.MatrixArity];
        bool[] covered = new bool[matrix.Length];
        foreach (ImpactCell cell in cells) {
            int category = (int)cell.Category - 1;
            int stage = Band(cell.Stage);
            int at = (category * LifecycleStage.Count) + stage;
            matrix[at] += cell.Value;
            covered[at] = true;
        }
        return covered.AsSpan().IndexOf(false) < 0
            ? new DeclaredImpacts.Full([.. matrix])
            : new DeclaredImpacts.Carbon(
                matrix.AsMemory(0, LifecycleStage.Count), covered.AsMemory(0, LifecycleStage.Count));
    }

    static int Band(Module stage) => (int)stage switch {
        1 => 0,
        2 => 1,
        3 => 2,
        >= 4 and <= 10 => 3,
        >= 11 and <= 14 => 4,
        15 => 5,
        _ => throw new UnreachableException(),
    };
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
