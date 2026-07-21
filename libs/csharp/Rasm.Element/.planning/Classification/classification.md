# [ELEMENT_CLASSIFICATION]

The two neutral cross-cutting axes every node and assessment keys on: the generic `Classification` `[ComplexValueObject]` (the `(System, Code, Edition)` edition-scoped IDENTITY plus the projector-resolved `Source`/`EditionDate`/`Title` annotation bundle) an `Graph/element#NODE_MODEL` `Object` node carries, and the one `Discipline` `[SmartEnum<string>]` — the sixteen-row AEC analysis vocabulary spanning structural mechanics (`Structural`/`Seismic`/`Wind`/`Dynamic`), building physics (`Thermal`/`Hygrothermal`/`Energy`/`Daylight`/`Acoustic`/`Fire`/`Circulation`/`Water`), and the lifecycle set (`Durability`/`Circularity`/`Environmental`/`Cost`) — a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case and an `Assessment/assessment#ASSESSMENT_NODE` node key on. Both are LIBRARY-NEUTRAL: `Classification` is a `(system, code, edition)` IDENTITY plus an equality-excluded resolved-annotation bundle (`Source` publisher, `EditionDate`, human `Title`) — NO standard-systems roster, NO bSDD dictionary, NO code-shape regex, NO `IfcClass` entity-class enum — the `Rasm.Bim` `Semantics/classification` projector owning the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved `(system, code, edition, source, editionDate, title)` onto a seam `Classification` at ingest so a consumer reads `element.Classification.Code` AND its human `Title` in one hop without learning the IFC schema or re-querying bSDD; an object's co-applied standard references (Uniclass + OmniClass together) ride the node's `Classifications` `Seq<Classification>`, the entity-class pair staying the primary `Classification` the IFC reads key on.

Edition is identity, the resolved annotations are not. A classification code's MEANING is edition-scoped — `IfcClassification` carries `Source`/`Edition`/`EditionDate` and a publisher re-editions a code's definition without changing its token, so `("uniclass2015", "Ss_25_10")` and the same token under a later edition are DISTINCT concepts a re-ingest must not collide; `Edition` therefore joins the `(System, Code)` identity (ordinal-ignore-case, blank for an edition-unspecified reference), while the `Source` publisher, the `EditionDate`, and the human `Title` are equality-/hash-/diagnostic-excluded `Option`-carried annotations a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record. Identity stays annotation-stable: two references to one `(system, code, edition)` triple with different resolved titles or publisher strings are EQUAL and content-address identically, so a node's `Graph/element#NODE_MODEL` content key never forks on a re-resolved name.

The seam never re-opens the IFC-schema strata: the `IfcClass` roster and the `PredefinedType` valid-set are a `Rasm.Bim` egress concern validated at `Emit` (`Graph/element#NODE_MODEL`), not an axis here. `Discipline` is the single discriminant the typed property, the assessment payload, and the `Rasm.Compute` analysis route all share: a `MaterialPropertySet.Thermal` maps to `Discipline.Thermal`, an `Assessment` keyed `Discipline.Energy` carries an energy-simulation receipt, and a future analysis discipline is one row — never a parallel enum per consumer, never a `bool IsThermal` the implementation re-derives. Both admit through the seam's `Fin<T>` rail (`Classification.Of`, `Discipline.Parse`) railing `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`, the closed-vocabulary identity making a blank pair or an unknown discipline token unrepresentable downstream.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: the `Classification` `[ComplexValueObject]` (the `(System, Code, Edition)` identity under split comparer policy, the equality-excluded `Source`/`EditionDate`/`Title` annotation bundle), the railed `Of` admission, the relation-typed `ClassificationCrosswalk` data row, and the `AllClassifications`/`ClassificationsIn`/`ClassificationIn`/`TranslateTo`/`Parents`/`Ancestors`/`Within` reads over projector-supplied mappings.
- [02]-[DISCIPLINE_AXIS]: the `Discipline` `[SmartEnum<string>]` sixteen-row analysis vocabulary keyed on a wire token with its `Title`/`Physical` columns and the railed `Parse` admission — the one axis the typed property, the assessment, and the analysis route share.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `Classification` the `[ComplexValueObject]` carrying the `System` token (the standard identifier — `"uniclass2015"`, `"omniclass"`, `"masterformat"`, a free lowercased string the seam never enumerates), the `Code` (the code-within-the-system, trimmed), the `Edition` (the standard's edition the code is scoped to — `""` for an edition-unspecified reference), and the equality-excluded annotation bundle (`Source` the `Option<string>` publisher, `EditionDate` the `Option<LocalDate>` revision date, `Title` the projector-resolved `Option<string>` concept name — e.g. `"Cavity external wall systems"`, each `None` for an author-minted or hierarchy-derived code) — the generic triple an `Graph/element#NODE_MODEL` `Object` node carries as its primary (entity-class-keying) `Classification`, the additional standard-system references riding the node's `Classifications` `Seq<Classification>` (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object); identity is the `(System, Code, Edition)` triple under split comparer policy (ordinal `System`, ordinal-ignore-case `Code`/`Edition`), the `Source`/`EditionDate`/`Title` an equality-/hash-/diagnostic-excluded annotation a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record.
- Entry: `Classification.Of(system, code, key)` is the ONE railed admission — every trailing member optional, so the edition-unspecified author path (`Edition` defaulting `""`, the annotations `None`) and the edition-scoped projector form are one entry discriminating on supplied members. `AllClassifications` closes the primary-plus-secondary storage split before any by-system read; `TranslateTo` returns every equivalent target, and `Parents`/`Ancestors`/`Within` traverse only explicit `Broader`/`Narrower` rows.
- Auto: the `[ComplexValueObject]` generates structural equality + hashing over ONLY the marked `(System, Code, Edition)` members under the split comparer policy. `ClassificationRelation` is row data: `Equivalent` drives translation, `Broader`/`Narrower` drive hierarchy, and `Related` preserves non-hierarchical dictionary links. The traversal carries a seen set, so malformed cyclic mapping data terminates without inventing an ancestry.
- Receipt: a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` carries the primary `Classification` AND the `Classifications` set flat. The corresponding `Node.Object.AllClassifications` read includes both storage locations, translates across systems without choosing an order-dependent head, and rolls up only through projector-supplied authoritative relations.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError<TError>]`, `[MemberEqualityComparer<TAccessor, TMember>]`, `[SmartEnum<string>]`), LanguageExt.Core (`Option`/`Seq`/`Fin`/`HashSet`), NodaTime (`LocalDate`), `Rasm` (the kernel `Op` op-key).
- Growth: a new standard system or edition is data; a resolved annotation is one equality-excluded member; a new mapping is one `ClassificationCrosswalk` row carrying its `ClassificationRelation`. Code punctuation never implies hierarchy because MasterFormat, Uniformat, and external dictionaries do not share one syntax.
- Boundary: `Classification` is ONE generic value-object — a per-system type or an `IfcClass`-style entity-class roster on the seam is the deleted form (the `IfcClass` roster and the `PredefinedType` valid-set are `Rasm.Bim`'s IFC-schema concern, never lowered into the seam); the `System` is an opaque token the seam never validates against a roster, the projector validating the code shape and resolving the bSDD class + `Source`/`Edition`/`Title` at ingest before lowering; identity is the `(System, Code, Edition)` triple — `Edition` is IDENTITY because a publisher re-editions a code's MEANING under one token (a `Source`/`EditionDate`/`Title` difference, by contrast, never fragments a node's content key), so the `Graph/element#NODE_MODEL` `ToCanonicalBytes` writes `System`/`Code`/`Edition` (for the primary `Classification` AND each deterministically-ordered member of the `Classifications` set, never the annotation bundle) and identity stays annotation-stable across runtimes; the `Object` node carries the typed triple plus the `Classifications` set so a query matches a code (`Within` for a branch, `ClassificationIn` for a standard, over the co-applied set) rather than a stringly-keyed property lookup — a free `string` classification field on a node is the named defect; the `Code` parent-derivation is a pure projection over the admitted code, never a stored parent edge or a per-call regex; the `IfcClassificationReference.Location` dictionary URI is NOT a seam member — it is fully derivable from `(System, Code)` through the projector's roster, so lowering it as a stored annotation duplicates the roster.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Runtime.InteropServices;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Classification;

// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ElementFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Classification {
    // Identity is the (System, Code, Edition) TRIPLE under SPLIT comparer policy (ordinal System — already lowercased
    // at admission — and ordinal-ignore-case Code/Edition); marking these three opts equality IN to them, so the
    // unmarked Source/EditionDate/Title are materialized but excluded from equality/hashing/diagnostics. Edition is
    // IDENTITY because a publisher re-editions a code's MEANING under one token (IfcClassification.Edition,
    // decompile-confirmed settable) — ("uniclass2015","Ss_25_10","") and ("uniclass2015","Ss_25_10","2") are distinct
    // concepts a re-ingest must not collide; "" is the edition-unspecified default the common author path mints. The
    // Source publisher, the EditionDate, and the Title are the Rasm.Bim-resolved annotations lowered from the bSDD /
    // IfcClassification record, so identity (and the System+Code+Edition-only Graph/element#NODE_MODEL ToCanonicalBytes)
    // stays annotation-stable while a consumer still reads the name/publisher/date flat.
    [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    public string System { get; }
    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string Code { get; }
    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string Edition { get; }
    public Option<string> Source { get; }
    public Option<LocalDate> EditionDate { get; }
    public Option<string> Title { get; }

    static partial void ValidateFactoryArguments(
        ref ElementFault? validationError, ref string system, ref string code, ref string edition,
        ref Option<string> source, ref Option<LocalDate> editionDate, ref Option<string> title) {
        if (string.IsNullOrWhiteSpace(system) || string.IsNullOrWhiteSpace(code)) {
            validationError = ElementFault.ValueRejected(Op.Of(name: nameof(Classification)), $"classification requires a non-blank system and code; got '{system}':'{code}'");
            return;
        }
        system = system.Trim().ToLowerInvariant();
        code = code.Trim();
        edition = edition.Trim();
        source = source.Map(static s => s.Trim());
        title = title.Map(static t => t.Trim());
    }

    // The ONE seam-rail admission a Rasm.Bim projector / author path takes: the common author path supplies the
    // (system, code, key) triple alone (Edition "", the annotations None), the projector leg additionally lowers
    // the resolved (edition, source, date, title) off the IfcClassification / bSDD record — one entry, the supplied
    // members the discriminant, never an edition-arity overload pair. A blank pair rails ElementFault.ValueRejected
    // re-keyed to the CALLER's Op so the operation context survives (the keyless ValidateFactoryArguments fault is
    // re-stamped here).
    public static Fin<Classification> Of(
        string system, string code, Op key, string edition = "",
        Option<string> source = default, Option<LocalDate> editionDate = default, Option<string> title = default) =>
        Validate(system, code, edition, source, editionDate, title, out Classification value) is { } fault
            ? ElementFault.ValueRejected(key, fault.Message)
            : Fin.Succ(value);

}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ClassificationRelation {
 public static readonly ClassificationRelation Equivalent = new("equivalent");
 public static readonly ClassificationRelation Broader = new("broader");
 public static readonly ClassificationRelation Narrower = new("narrower");
 public static readonly ClassificationRelation Related = new("related");
}

public readonly record struct ClassificationCrosswalk(
 Classification From, Classification To, ClassificationRelation Relation, Option<string> Source);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ClassificationOps {
    extension(Node.Object node) {
        public Seq<Classification> AllClassifications =>
            (Seq(node.Classification) + node.Classifications)
                .Fold(Seq<Classification>(), static (unique, classification) =>
                    unique.Contains(classification) ? unique : unique.Add(classification));
    }

    extension(Seq<Classification> classifications) {
        // Every reference under ONE standard — a Uniclass-keyed cost rollup over a model also carrying OmniClass reads
        // ClassificationsIn("uniclass2015") without a stringly-keyed scan, and a model carrying TWO editions of one
        // system (a re-editioned re-ingest) reads BOTH (the editions are distinct identities, §EDITION_SCOPING). The
        // query `system` is normalized ONCE per query through the SAME Trim().ToLowerInvariant() the
        // ValidateFactoryArguments admission applies and compared Ordinal — the policy the (System) identity comparer
        // (ComparerAccessors.StringOrdinal) holds — so the read and the stored identity agree exactly, never a
        // stored-Ordinal-vs-query-IgnoreCase drift nor a per-element re-normalization allocation inside the filter.
        public Seq<Classification> ClassificationsIn(string system) {
            string normalized = system.Trim().ToLowerInvariant();
            return classifications.Filter(c => string.Equals(c.System, normalized, StringComparison.Ordinal));
        }

        // The single-reference by-system read the common one-edition-per-system model takes — Some ONLY when the
        // match is UNAMBIGUOUS (exactly one reference under the system/edition filter), never an order-dependent head.
        // The seam does NOT semantically rank editions: an edition is an OPAQUE token (the §EDITION_SCOPING law), so
        // a lexical OrderByDescending claiming "the newest" is the deleted illusory form ("9" sorts after "10", "2015"
        // after "9" — lexical string order is NOT edition recency, and the seam owns no edition calendar). A
        // multi-edition model reads every reference through ClassificationsIn and the consumer that KNOWS its edition
        // scheme ranks them; None when the standard was never applied.
        public Option<Classification> ClassificationIn(string system, Option<string> edition = default) {
            Seq<Classification> matches = classifications.ClassificationsIn(system)
                .Filter(classification => edition.ForAll(value => string.Equals(classification.Edition, value.Trim(), StringComparison.OrdinalIgnoreCase)));
            return matches.Count == 1 ? matches.Head : None;
        }

        // Equivalence is symmetric; translation reads either endpoint and deduplicates identity-equal targets.
        public Seq<Classification> TranslateTo(string system, Seq<ClassificationCrosswalk> crosswalk) {
            string normalized = system.Trim().ToLowerInvariant();
            Seq<Classification> candidates = classifications.ClassificationsIn(normalized)
                + classifications.Bind(source => crosswalk.Choose(row =>
                    row.Relation != ClassificationRelation.Equivalent
                        ? None
                        : row.From == source && string.Equals(row.To.System, normalized, StringComparison.Ordinal)
                            ? Some(row.To)
                            : row.To == source && string.Equals(row.From.System, normalized, StringComparison.Ordinal)
                                ? Some(row.From)
                                : None));
            return candidates.Fold(Seq<Classification>(), static (translated, candidate) =>
                translated.Contains(candidate) ? translated : translated.Add(candidate));
        }
    }

    extension(Classification classification) {
        public Seq<Classification> Parents(Seq<ClassificationCrosswalk> crosswalk) =>
            crosswalk.Choose(row => row.From == classification && row.Relation == ClassificationRelation.Broader
                ? Some(row.To)
                : row.To == classification && row.Relation == ClassificationRelation.Narrower
                    ? Some(row.From)
                    : None);

        public Seq<Classification> Ancestors(Seq<ClassificationCrosswalk> crosswalk) =>
            Expand(classification, crosswalk, HashSet(classification));

        public bool Within(Classification branch, Seq<ClassificationCrosswalk> crosswalk) =>
            classification == branch || classification.Ancestors(crosswalk).Contains(branch);
    }

    private static Seq<Classification> Expand(
        Classification classification,
        Seq<ClassificationCrosswalk> crosswalk,
        HashSet<Classification> seen) =>
        classification.Parents(crosswalk)
            .Fold((Seen: seen, Result: Seq<Classification>()), (state, parent) => Visit(parent, crosswalk, state))
            .Result;

    private static (HashSet<Classification> Seen, Seq<Classification> Result) Visit(
        Classification classification,
        Seq<ClassificationCrosswalk> crosswalk,
        (HashSet<Classification> Seen, Seq<Classification> Result) state) =>
        state.Seen.Contains(classification)
            ? state
            : classification.Parents(crosswalk).Fold(
                (state.Seen.Add(classification), state.Result.Add(classification)),
                (next, parent) => Visit(parent, crosswalk, next));
}
```

## [03]-[DISCIPLINE_AXIS]

- Owner: `Discipline` the `[SmartEnum<string>]` sixteen-row analysis-discipline vocabulary keyed on a stable lowercase token, each row carrying its `Title` display name and its `Physical` flag (a measured-physics discipline a `Rasm.Compute` solver computes versus a procurement/lifecycle one a catalog/EPD supplies) — the single discriminant a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case maps to, an `Assessment/assessment#ASSESSMENT_NODE` node keys on, and the `Rasm.Compute` analysis route selects by. The roster spans the real AEC analysis space: structural mechanics (`Structural` static/FEA, `Seismic` EN 1998/ASCE 7 action, `Wind` EN 1991-1-4/ASCE 7 load, `Dynamic` EN 1998-1 response-spectrum/ISO 10137 vibration serviceability — the row the `MaterialPropertySet.Damping` case keys), building physics (`Thermal` ISO 6946, `Hygrothermal` EN 15026/EN 13788 moisture, `Energy` whole-building simulation, `Daylight` EN 17037/LM-83, `Acoustic` ISO 12354, `Fire` EN 199x-1-2, `Circulation` IBC Ch.10/EN egress life-safety, `Water` EN 806 demand/drainage), and lifecycle (`Durability` ISO 15686 service life, `Circularity` ISO 20887 disassembly/reuse, `Environmental` EN 15978 LCA, `Cost`).
- Entry: the declaration list IS the vocabulary; `Discipline.Parse(token, key)` admits a wire/route token on the seam `Fin<T>` rail (railing `ElementFault.ValueRejected` on an unknown token — the seam-consistent admission the sibling `Composition/material#MATERIAL_PROPERTY` `FireRating.Parse`/`Currency.Parse` share), `Get`/`TryGet` resolve a row for a trusted token, `Items` enumerates the sixteen, and the `[SmartEnum<string>]` key codec round-trips the token at the wire so an `Assessment` node persists `Discipline.Energy` as `"energy"` and re-admits it.
- Auto: `Discipline` dispatches through the generated total `Switch` (a consumer routes on the row, never a `string` compare); the `Physical` column is the analysis-dispatch axis the `Rasm.Compute` route reads (solver-input versus catalog-lookup) rather than a per-discipline branch; `Parse` reuses the generated zero-allocation key lookup so admission and dispatch share one vocabulary and a non-standard token is a railed rejection, never a silent miss.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`), `Rasm` (the kernel `Op` op-key).
- Growth: a new analysis discipline is one `Discipline` row carrying its token, title, and physical flag — a blast/glare discipline lands as one row the `MaterialPropertySet`/`Assessment`/analysis route all read; never a parallel discipline enum per consumer and never a `bool IsThermal` flag set the implementation re-derives.
- Boundary: `Discipline` is the ONE analysis axis — a `StructuralDiscipline`/`ThermalDiscipline` parallel enum or a per-consumer discipline string is the deleted form; the sixteen rows are the closed roster BOTH consumers key into: the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` `[Union]` maps its eleven cases onto ten rows (`Mechanical` and `Orthotropic` share `Structural`; `Damping` keys `Dynamic`; `Optical` keys `Energy`; `Hygrothermal`/`Durability` their namesake rows) and the `Assessment/assessment#ASSESSMENT_NODE` payload keys on ANY row; a row with no intrinsic single-material property — the driven-load physics rows (`Seismic`/`Wind`/`Daylight`/`Water`) and the case-less `Circularity` lifecycle row — reads `None` from `MaterialPropertyAccess.ForDiscipline` BY CONSTRUCTION (the `Find` over the case→discipline map finds no case), never a gap; the case-to-discipline correspondence is owned ONCE by `MaterialPropertySet.Discipline` (`Composition/material#MATERIAL_PROPERTY`), this axis carrying only the neutral row and never re-stating the map, so a future material-borne discipline (a `Water` absorption case, a `Circularity` disassembly case) is one `MaterialPropertySet` case carrying its existing row — zero edits here; the `Physical` column distinguishes a solver-input discipline from a catalog-lookup one so the analysis dispatcher routes by the column, never an `if discipline == Structural` chain; the row carries NO aggregation/solver-route policy and NO governing-standard roster — the standards above are the rows' real-space justification, the route roster (`"iso-6946-u"`, `"en1998-response"`) staying `Rasm.Compute`'s, so a column encoding "how this discipline aggregates across plies" or "which standard solves it" is a strata leak the seam refuses, the axis staying the pure neutral routing vocabulary.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Declaration order groups the roster semantically (structural mechanics → building physics → lifecycle);
// wire identity is the KEY token and the Graph/element#NODE_MODEL Material projection orders by Key ordinal,
// so declaration order is content-key-inert and a new row lands in its group with zero key drift.
[SmartEnum<string>]
public sealed partial class DisciplineGroup {
    public static readonly DisciplineGroup StructuralMechanics = new("structural-mechanics");
    public static readonly DisciplineGroup BuildingPhysics = new("building-physics");
    public static readonly DisciplineGroup Lifecycle = new("lifecycle");
}

[SmartEnum<string>]
public sealed partial class Discipline {
    public static readonly Discipline Structural = new("structural", "Structural", DisciplineGroup.StructuralMechanics, physical: true);
    public static readonly Discipline Seismic = new("seismic", "Seismic", DisciplineGroup.StructuralMechanics, physical: true);
    public static readonly Discipline Wind = new("wind", "Wind", DisciplineGroup.StructuralMechanics, physical: true);
    public static readonly Discipline Dynamic = new("dynamic", "Dynamic", DisciplineGroup.StructuralMechanics, physical: true);
    public static readonly Discipline Thermal = new("thermal", "Thermal", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Hygrothermal = new("hygrothermal", "Hygrothermal", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Energy = new("energy", "Energy", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Daylight = new("daylight", "Daylight", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Acoustic = new("acoustic", "Acoustic", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Fire = new("fire", "Fire", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Circulation = new("circulation", "Circulation", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Water = new("water", "Water", DisciplineGroup.BuildingPhysics, physical: true);
    public static readonly Discipline Durability = new("durability", "Durability", DisciplineGroup.Lifecycle, physical: false);
    public static readonly Discipline Circularity = new("circularity", "Circularity", DisciplineGroup.Lifecycle, physical: false);
    public static readonly Discipline Environmental = new("environmental", "Environmental", DisciplineGroup.Lifecycle, physical: false);
    public static readonly Discipline Cost = new("cost", "Cost", DisciplineGroup.Lifecycle, physical: false);

    public string Title { get; }
    public DisciplineGroup Group { get; }
    // The analysis-dispatch axis Rasm.Compute routes on: a solver-computed measured-physics discipline (FEA,
    // EN 1998 seismic action, EN 1991-1-4 wind, EN 1998-1/ISO 10137 dynamic response, ISO 6946 thermal,
    // EN 15026 hygrothermal, EnergyPlus, EN 17037 daylight, ISO 12354 acoustic, EN fire, IBC Ch.10/EN egress
    // circulation, EN 806 water) versus
    // a catalog/factor-method lifecycle one (ISO 15686 durability, ISO 20887 circularity, EN 15978
    // environmental, cost) — read as a column, never re-derived per consumer.
    public bool Physical { get; }

    // The seam-rail admission a wire/route token takes — the FireRating.Parse/Currency.Parse-consistent form
    // railing ElementFault.ValueRejected on an unknown token; Get/TryGet stay the trusted-token resolvers.
    public static Fin<Discipline> Parse(string token, Op key) =>
        TryGet(token, out Discipline? discipline) && discipline is { } row ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<discipline-unknown:{token}>");

    public static Seq<Discipline> In(DisciplineGroup group) => Items.Filter(discipline => discipline.Group == group).ToSeq();
}
```

## [04]-[IMPLEMENTATION_LAW]

- [CLASSIFICATION_NEUTRALITY]: the seam `Classification` carries the `(system, code, edition)` IDENTITY plus the projector-resolved `Source`/`EditionDate`/`Title` annotations — the standard-systems roster (`Uniclass2015`/`OmniClass`/`MasterFormat`/`Uniformat`), the bSDD dictionary URI, the code-shape regex, and the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip stay the `Rasm.Bim` `Semantics/classification` projector's, which resolves a code against the live bSDD service and lowers the resolved `(system, code, edition, source, editionDate, title)` onto a seam `Classification` at ingest; identity is the marked `(System, Code, Edition)` triple so the resolved annotation bundle is equality-/hash-excluded (a node's content key, written `System`+`Code`+`Edition` only by `Graph/element#NODE_MODEL` `ToCanonicalBytes`, stays stable whether or not the name/publisher/date resolved), the seam never re-opening the IFC-schema strata — the `IfcClass` entity-class enum and the `PredefinedType` valid-set are a Bim egress concern validated at `Emit`, never an axis here, and the `Object` node carries the generic triple plus the human name a consumer reads in one hop.
- [EDITION_SCOPING]: `Edition` is identity because a code's MEANING is edition-scoped — `IfcClassification` carries a first-class `Source`/`Edition`/`EditionDate` and a publisher re-editions a code's definition without changing its token, so `("uniclass2015","Ss_25_10","2015")` and the same token under a later edition are DISTINCT concepts a re-ingest must hold apart. Hierarchy never derives from punctuation: `Parents`/`Ancestors`/`Within` consume explicit edition-scoped `Broader`/`Narrower` crosswalk rows, because a MasterFormat or Uniformat token does not encode its authoritative parent.
- [DISCIPLINE_COVERAGE]: the sixteen `Discipline` rows close the real AEC analysis space — structural mechanics (`Structural`, `Seismic` EN 1998/ASCE 7, `Wind` EN 1991-1-4/ASCE 7, `Dynamic` EN 1998-1 response-spectrum + ISO 10137/EN 1990-A1.4.4 vibration serviceability), building physics (`Thermal` ISO 6946, `Hygrothermal` EN 15026/EN 13788, `Energy`, `Daylight` EN 17037/LM-83, `Acoustic` ISO 12354, `Fire` EN 199x-1-2, `Circulation` IBC Ch.10/EN egress + life-safety, `Water` EN 806), lifecycle (`Durability` ISO 15686, `Circularity` ISO 20887/EN 15804 module D, `Environmental` EN 15978, `Cost`) — so the analysis route the `Rasm.Compute` solver selects, the typed material property, and the assessment receipt share one axis; the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case→discipline map covers eleven cases onto ten rows (`Mechanical`/`Orthotropic` both `Structural`; `Damping` the `Dynamic` row — the consumer contract that seats `Dynamic` in the roster; `Optical` the `Energy` row — a glazing's solar-optical constants ARE the material-borne energy-simulation input; `Hygrothermal`/`Durability` their namesake rows), and every row without an intrinsic single-material property — the driven-load physics rows (`Seismic`/`Wind`/`Daylight`/`Circulation`/`Water`, whose inputs are geometry, site, occupancy, and systems rather than a material constant) and the case-less `Circularity` row — reads `None` from `ForDiscipline` by construction (no case maps to them), never a per-row exception; the `Physical` column separates a measured-physics discipline (a solver input) from a catalog/factor-method lifecycle one so the dispatcher routes by the column rather than a per-discipline branch — the case-to-discipline map staying `MaterialPropertySet`'s single owner, and the per-discipline aggregation route a `Rasm.Compute` concern the seam never encodes as a row column.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
