# [ELEMENT_CLASSIFICATION]

`Classification` and `Discipline` are the two neutral cross-cutting axes every node and assessment keys on: `Classification` `[ComplexValueObject]` carries the `(System, Code, Edition)` identity with a projector-resolved annotation bundle an `Graph/element#NODE_MODEL` `Object` node holds, and `Discipline` `[SmartEnum<string>]` closes the AEC analysis vocabulary a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case and an `Assessment/assessment#ASSESSMENT_NODE` node key on.

Edition is identity; resolved annotations are not. `Edition` joins the `(System, Code)` identity ordinal-ignore-case, blank for an edition-unspecified reference, because a publisher re-editions a code's MEANING under one unchanged token, so `("uniclass2015", "Ss_25_10")` under two editions names two DISTINCT concepts a re-ingest holds apart. `Source`, `EditionDate`, and `Title` stay equality-, hash-, and diagnostic-excluded `Option` annotations a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record.

`Discipline` is the single discriminant the typed property, the assessment payload, and the `Rasm.Compute` analysis route all share — `MaterialPropertySet.Thermal` maps to `Discipline.Thermal`, and a new analysis discipline is one row. Both axes admit through the seam's `Fin<T>` rail (`Classification.Of`, `Discipline.Parse`) railing `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`, so a blank pair or an unknown discipline token is unrepresentable downstream.

## [01]-[INDEX]

- [02]-[CLASSIFICATION_AXIS]: `Classification` the `[ComplexValueObject]` (the `(System, Code, Edition)` identity under split comparer policy, the equality-excluded `Source`/`EditionDate`/`Title` annotation bundle), the railed `Of` admission, the relation-typed `ClassificationCrosswalk` data row, and the `ClassificationsIn`/`ClassificationIn`/`TranslateTo`/`Parents`/`Ancestors`/`Within` reads over projector-supplied mappings.
- [03]-[DISCIPLINE_AXIS]: `Discipline` the `[SmartEnum<string>]` analysis vocabulary keyed on a wire token with its `Title`/`Group`/`Physical` columns, the railed `Parse` admission, and the `In(group)` roster read — the one axis the typed property, the assessment, and the analysis route share.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `Classification` the `[ComplexValueObject]` carrying the `System` token (the standard identifier — `"uniclass2015"`, `"omniclass"`, `"masterformat"`, a free lowercased string the seam never enumerates), the `Code` (the code-within-the-system, trimmed), the `Edition` (the standard's edition the code is scoped to — `""` for an edition-unspecified reference), and the equality-excluded annotation bundle (`Source` the `Option<string>` publisher, `EditionDate` the `Option<LocalDate>` revision date, `Title` the projector-resolved `Option<string>` concept name — e.g. `"Cavity external wall systems"`, each `None` for an author-minted or hierarchy-derived code) — the generic triple an `Graph/element#NODE_MODEL` `Object` node carries as its primary (entity-class-keying) `Classification`, the additional standard-system references riding the node's `Classifications` `Seq<Classification>` (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object); identity is the `(System, Code, Edition)` triple under split comparer policy (ordinal `System`, ordinal-ignore-case `Code`/`Edition`), the `Source`/`EditionDate`/`Title` an equality-/hash-/diagnostic-excluded annotation a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record.
- Entry: `Classification.Of(system, code, key)` is the ONE railed admission — every trailing member optional, so the edition-unspecified author path (`Edition` defaulting `""`, the annotations `None`) and the edition-scoped projector form are one entry discriminating on supplied members. `ClassificationsIn(system)` reads every reference under one standard and `ClassificationIn(system, edition)` the single UNAMBIGUOUS one (`None` for an absent or multi-edition match, never an order-dependent head), `TranslateTo` returns every equivalent target, and `Parents`/`Ancestors`/`Within` traverse only explicit `Broader`/`Narrower` rows.
- Auto: the `[ComplexValueObject]` generates structural equality + hashing over ONLY the marked `(System, Code, Edition)` members under the split comparer policy. `ClassificationRelation` is row data: `Equivalent` drives translation, `Broader`/`Narrower` drive hierarchy, and `Related` preserves non-hierarchical dictionary links. `Parents`/`Ancestors` carry a seen set, so malformed cyclic mapping data terminates without inventing an ancestry.
- Receipt: a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` carries the primary `Classification` AND the `Classifications` set flat. `Graph/element#NODE_MODEL` `Node.Object.AllClassifications` closes that storage split at its own owner, and these reads translate across systems without choosing an order-dependent head and roll up only through projector-supplied authoritative relations.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError<TError>]`, `[MemberEqualityComparer<TAccessor, TMember>]`, `[SmartEnum<string>]`), LanguageExt.Core (`Option`/`Seq`/`Fin`/`HashSet`), NodaTime (`LocalDate`), `Rasm` (the kernel `Op` op-key).
- Growth: a new standard system or edition is data; a resolved annotation is one equality-excluded member; a new mapping is one `ClassificationCrosswalk` row carrying its `ClassificationRelation`. Code punctuation never implies hierarchy because MasterFormat, Uniformat, and external dictionaries do not share one syntax.
- Boundary: `Classification` is ONE generic value-object — a per-system type or an `IfcClass`-style entity-class roster on the seam is the deleted form (the `IfcClass` roster and the `PredefinedType` valid-set are `Rasm.Bim`'s IFC-schema concern, never lowered into the seam); the `System` is an opaque token the seam never validates against a roster, the projector validating the code shape and resolving the bSDD class + `Source`/`Edition`/`Title` at ingest before lowering; identity is the `(System, Code, Edition)` triple — `Edition` is IDENTITY because a publisher re-editions a code's MEANING under one token (a `Source`/`EditionDate`/`Title` difference, by contrast, never fragments a node's content key), so the `Graph/element#NODE_MODEL` `ToCanonicalBytes` writes `System`/`Code`/`Edition` (for the primary `Classification` AND each deterministically-ordered member of the `Classifications` set, never the annotation bundle) and identity stays annotation-stable across runtimes; the `Object` node carries the typed triple with the `Classifications` set so a query matches a code (`Within` for a branch, `ClassificationIn` for a standard, over the co-applied set) rather than a stringly-keyed property lookup — a free `string` classification field on a node is the named defect; the `Code` parent-derivation is a pure projection over the admitted code, never a stored parent edge or a per-call regex; the `IfcClassificationReference.Location` dictionary URI is NOT a seam member — it is fully derivable from `(System, Code)` through the projector's roster, so lowering it as a stored annotation duplicates the roster.

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

    // Of is the ONE seam-rail admission a Rasm.Bim projector or author path takes: an author path supplies the
    // (system, code, key) triple alone (Edition "", annotations None), and the projector leg also lowers the resolved
    // (edition, source, date, title) off the IfcClassification / bSDD record — one entry discriminating on supplied
    // members, never an edition-arity overload pair. A blank pair rails ElementFault.ValueRejected re-keyed to the
    // CALLER's Op so the operation context survives (the keyless ValidateFactoryArguments fault re-stamps here).
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

        // ClassificationIn is the single-reference by-system read a one-edition-per-system model takes — Some ONLY
        // when the match is UNAMBIGUOUS (exactly one reference under the system/edition filter), never an
        // order-dependent head. Editions carry NO semantic rank here: an edition is an OPAQUE token (the
        // §EDITION_SCOPING law), so a lexical OrderByDescending claiming "the newest" is the deleted illusory form
        // ("9" sorts after "10", "2015" after "9" — lexical string order is NOT edition recency, and the seam owns no
        // edition calendar). A multi-edition model reads every reference through ClassificationsIn and the consumer
        // that KNOWS its edition scheme ranks them; None when the standard was never applied.
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

- Owner: `Discipline` the `[SmartEnum<string>]` analysis-discipline vocabulary keyed on a stable lowercase token, each row carrying its `Title` display name, its `DisciplineGroup` band, and its `Physical` flag (a measured-physics discipline a `Rasm.Compute` solver computes versus a procurement/lifecycle one a catalog/EPD supplies) — the single discriminant a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case maps to, an `Assessment/assessment#ASSESSMENT_NODE` node keys on, and the `Rasm.Compute` analysis route selects by; each row names its governing standard on its own declaration.
- Entry: the declaration list IS the vocabulary; `Discipline.Parse(token, key)` admits a wire/route token on the seam `Fin<T>` rail (railing `ElementFault.ValueRejected` on an unknown token — the seam-consistent admission the sibling `Composition/material#MATERIAL_PROPERTY` `FireRating.Parse`/`Currency.Parse` share), `Get`/`TryGet` resolve a row for a trusted token, `In(group)` reads one `DisciplineGroup`'s rows (the structural-mechanics, building-physics, and lifecycle bands a dispatcher or a UI facet filters by, off the row's own `Group` column rather than a re-derived name test), `Items` enumerates the roster, and the `[SmartEnum<string>]` key codec round-trips the token at the wire so an `Assessment` node persists `Discipline.Energy` as `"energy"` and re-admits it.
- Auto: `Discipline` dispatches through the generated total `Switch` (a consumer routes on the row, never a `string` compare); the `Physical` column is the analysis-dispatch axis the `Rasm.Compute` route reads (solver-input versus catalog-lookup) rather than a per-discipline branch; `Parse` reuses the generated zero-allocation key lookup so admission and dispatch share one vocabulary and a non-standard token is a railed rejection, never a silent miss.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`), `Rasm` (the kernel `Op` op-key).
- Growth: a new analysis discipline is one `Discipline` row carrying its token, title, and physical flag — a blast/glare discipline lands as one row the `MaterialPropertySet`/`Assessment`/analysis route all read; never a parallel discipline enum per consumer and never a `bool IsThermal` flag set the implementation re-derives.
- Boundary: `Discipline` is the ONE analysis axis — a `StructuralDiscipline`/`ThermalDiscipline` parallel enum or a per-consumer discipline string is the deleted form; the rows are the closed roster BOTH consumers key into: the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` `[Union]` maps its eleven cases onto ten rows (`Mechanical` and `Orthotropic` share `Structural`; `Damping` keys `Dynamic`; `Optical` keys `Energy`; `Hygrothermal`/`Durability` their namesake rows) and the `Assessment/assessment#ASSESSMENT_NODE` payload keys on ANY row; a row with no intrinsic single-material property — the driven-load physics rows (`Seismic`/`Wind`/`Daylight`/`Circulation`/`Water`) and the case-less `Circularity` lifecycle row — reads `None` from `MaterialPropertyAccess.ForDiscipline` BY CONSTRUCTION (the `Find` over the case→discipline map finds no case), never a gap; the case-to-discipline correspondence is owned ONCE by `MaterialPropertySet.Discipline` (`Composition/material#MATERIAL_PROPERTY`), this axis carrying only the neutral row and never re-stating the map, so a future material-borne discipline (a `Water` absorption case, a `Circularity` disassembly case) is one `MaterialPropertySet` case carrying its existing row — zero edits here; the `Physical` column distinguishes a solver-input discipline from a catalog-lookup one and the `Group` column bands the roster (`In(group)` reading one band), so the analysis dispatcher and every facet route by column, never an `if discipline == Structural` chain; the row carries NO aggregation/solver-route policy and NO governing-standard roster — the standards above are the rows' real-space justification, the route roster (`"iso-6946-u"`, `"en1998-response"`) staying `Rasm.Compute`'s, so a column encoding "how this discipline aggregates across plies" or "which standard solves it" is a strata leak the seam refuses, the axis staying the pure neutral routing vocabulary.

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
    public static readonly Discipline Structural = new("structural", "Structural", DisciplineGroup.StructuralMechanics, physical: true); // static/FEA
    public static readonly Discipline Seismic = new("seismic", "Seismic", DisciplineGroup.StructuralMechanics, physical: true); // EN 1998 / ASCE 7 action
    public static readonly Discipline Wind = new("wind", "Wind", DisciplineGroup.StructuralMechanics, physical: true); // EN 1991-1-4 / ASCE 7 load
    public static readonly Discipline Dynamic = new("dynamic", "Dynamic", DisciplineGroup.StructuralMechanics, physical: true); // EN 1998-1 response-spectrum, ISO 10137 / EN 1990-A1.4.4 vibration serviceability — the row MaterialPropertySet.Damping keys
    public static readonly Discipline Thermal = new("thermal", "Thermal", DisciplineGroup.BuildingPhysics, physical: true); // ISO 6946
    public static readonly Discipline Hygrothermal = new("hygrothermal", "Hygrothermal", DisciplineGroup.BuildingPhysics, physical: true); // EN 15026 / EN 13788 moisture
    public static readonly Discipline Energy = new("energy", "Energy", DisciplineGroup.BuildingPhysics, physical: true); // whole-building simulation
    public static readonly Discipline Daylight = new("daylight", "Daylight", DisciplineGroup.BuildingPhysics, physical: true); // EN 17037 / LM-83
    public static readonly Discipline Acoustic = new("acoustic", "Acoustic", DisciplineGroup.BuildingPhysics, physical: true); // ISO 12354
    public static readonly Discipline Fire = new("fire", "Fire", DisciplineGroup.BuildingPhysics, physical: true); // EN 199x-1-2
    public static readonly Discipline Circulation = new("circulation", "Circulation", DisciplineGroup.BuildingPhysics, physical: true); // IBC Ch.10 / EN egress life-safety
    public static readonly Discipline Water = new("water", "Water", DisciplineGroup.BuildingPhysics, physical: true); // EN 806 demand/drainage
    public static readonly Discipline Durability = new("durability", "Durability", DisciplineGroup.Lifecycle, physical: false); // ISO 15686 service life
    public static readonly Discipline Circularity = new("circularity", "Circularity", DisciplineGroup.Lifecycle, physical: false); // ISO 20887 disassembly/reuse, EN 15804 module D
    public static readonly Discipline Environmental = new("environmental", "Environmental", DisciplineGroup.Lifecycle, physical: false); // EN 15978 LCA
    public static readonly Discipline Cost = new("cost", "Cost", DisciplineGroup.Lifecycle, physical: false);

    public string Title { get; }
    public DisciplineGroup Group { get; }
    // Physical is the analysis-dispatch axis Rasm.Compute routes on: true for a solver-computed measured-physics
    // discipline, false for a catalog/factor-method lifecycle one — read as a column, never re-derived per consumer.
    public bool Physical { get; }

    // Parse is the seam-rail admission a wire/route token takes — the FireRating.Parse/Currency.Parse-consistent form
    // railing ElementFault.ValueRejected on an unknown token; Get/TryGet stay the trusted-token resolvers.
    public static Fin<Discipline> Parse(string token, Op key) =>
        TryGet(token, out Discipline? discipline) && discipline is { } row ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<discipline-unknown:{token}>");

    public static Seq<Discipline> In(DisciplineGroup group) => toSeq(Items).Filter(discipline => discipline.Group == group);
}
```

## [04]-[IMPLEMENTATION_LAW]

- [CLASSIFICATION_NEUTRALITY]: `Classification` carries the `(system, code, edition)` IDENTITY with the projector-resolved `Source`/`EditionDate`/`Title` annotations, and the standard-systems roster, bSDD dictionary URI, code-shape regex, `IfcClass` enum, `PredefinedType` valid-set, and `IfcRelAssociatesClassification` round-trip all stay the `Rasm.Bim` `Semantics/classification` projector's, which lowers a bSDD-resolved reference at ingest so an `Object` node reads triple and name in one hop.
- [ANNOTATION_STABILITY]: identity is the marked `(System, Code, Edition)` triple alone, so the resolved annotation bundle is equality- and hash-excluded and `Graph/element#NODE_MODEL` `ToCanonicalBytes` writes `System`+`Code`+`Edition` only — a node's content key stays stable whether or not name, publisher, and date resolved.
- [EDITION_SCOPING]: `Edition` is identity because a code's MEANING is edition-scoped — a publisher re-editions a code's definition without changing its token, so `("uniclass2015","Ss_25_10","2015")` and that token under a later edition are DISTINCT concepts a re-ingest holds apart. Hierarchy never derives from punctuation: `Parents`/`Ancestors`/`Within` consume explicit edition-scoped `Broader`/`Narrower` crosswalk rows, because a MasterFormat or Uniformat token encodes no authoritative parent.
- [DISCIPLINE_COVERAGE]: `Discipline` rows close the real AEC analysis space across three `DisciplineGroup` bands — structural mechanics, building physics, lifecycle — each row naming its governing standard on its own declaration, so the `Rasm.Compute` analysis route, the typed material property, and the assessment receipt share one axis and a new discipline is one row.
- [DISCIPLINE_MAPPING]: `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` owns the case→discipline map alone — `Mechanical`/`Orthotropic` both `Structural`, `Damping` the `Dynamic` row it seats in the roster, `Optical` the `Energy` row because a glazing's solar-optical constants ARE the material-borne energy-simulation input — and a row with no intrinsic single-material property reads `None` from `ForDiscipline` by construction, never a per-row exception.
- [DISCIPLINE_DISPATCH]: `Physical` separates a measured-physics discipline a solver computes from a catalog/factor-method lifecycle one, so a dispatcher routes by the column rather than a per-discipline branch, and the per-discipline aggregation route stays a `Rasm.Compute` concern the seam never encodes as a row column.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
