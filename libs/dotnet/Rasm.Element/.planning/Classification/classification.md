# [ELEMENT_CLASSIFICATION]

`Classification` and `Discipline` are the two neutral cross-cutting axes every node and assessment keys on: `Classification` `[ComplexValueObject]` carries the `(System, Code, Edition)` identity with a projector-resolved annotation bundle an `Graph/element#NODE_MODEL` `Object` node holds, and `Discipline` `[SmartEnum<string>]` closes the AEC analysis vocabulary a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case and an `Assessment/assessment#ASSESSMENT_NODE` node key on.

Edition is identity; resolved annotations are not. `Edition` joins the `(System, Code)` identity ordinal-ignore-case, blank for an edition-unspecified reference, because a publisher re-editions a code's MEANING under one unchanged token, so `("uniclass2015", "Ss_25_10")` under two editions names two DISTINCT concepts a re-ingest holds apart. `Source`, `EditionDate`, and `Title` stay equality-, hash-, and diagnostic-excluded `Option` annotations a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record.

`Discipline` is the single discriminant the typed property, the assessment payload, and the `Rasm.Compute` analysis route all share — `MaterialPropertySet.Thermal` maps to `Discipline.Thermal`, and a new analysis discipline is one row. Both axes admit through the contract's `Fin<T>` result (`Classification.Of`, `key.AcceptValidated<Discipline>`), with blank or unknown tokens refused by the kernel admission bridge so neither is representable downstream.

## [01]-[INDEX]

- [02]-[CLASSIFICATION_AXIS]: `Classification` the `[ComplexValueObject]` (the `(System, Code, Edition)` identity under split comparer policy, the equality-excluded `Source`/`EditionDate`/`Title` annotation bundle) and the result-returning `Of` admission — the ONE door (`SkipFactoryMethods` emits no throwing twin).
- [03]-[DISCIPLINE_AXIS]: `Discipline` the `[SmartEnum<string>]` analysis vocabulary keyed on a wire token, with the result-returning `Parse` admission — the one axis the typed property, the assessment, and the analysis route share.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `Classification` the `[ComplexValueObject]` carrying the `System` token (the standard identifier — `"uniclass2015"`, `"omniclass"`, `"masterformat"`, a free lowercased string the contract never enumerates), the `Code` (the code-within-the-system, trimmed), the `Edition` (the standard's edition the code is scoped to — `""` for an edition-unspecified reference), and the equality-excluded annotation bundle (`Source` the `Option<string>` publisher, `EditionDate` the `Option<LocalDate>` revision date, `Title` the projector-resolved `Option<string>` concept name — e.g. `"Cavity external wall systems"`, each `None` for an author-minted or hierarchy-derived code) — the generic triple an `Graph/element#NODE_MODEL` `Object` node carries as its primary (entity-class-keying) `Classification`, the additional standard-system references riding the node's `Classifications` `Seq<Classification>` (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object); identity is the `(System, Code, Edition)` triple under split comparer policy (ordinal `System`, ordinal-ignore-case `Code`/`Edition`), the `Source`/`EditionDate`/`Title` an equality-/hash-/diagnostic-excluded annotation a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassification` record.
- Entry: `Classification.Of(system, code)` is the ONE result-returning admission — every trailing member optional, so the edition-unspecified author path (`Edition` defaulting `""`, the annotations `None`) and the edition-scoped projector form are one entry discriminating on supplied members, its two blank gates accumulating; `SkipFactoryMethods` emits no throwing `Create`/`TryCreate`/`Validate` beside it, so a consumer holds exactly one door.
- Auto: the `[ComplexValueObject]` generates structural equality + hashing over ONLY the marked `(System, Code, Edition)` members under the split comparer policy, and `SkipFactoryMethods` withholds the throwing `Create`/`TryCreate`/`Validate` trio, so `Of`'s accumulated blank gates are the only admission and the generated private ctor is reachable from inside the type alone.
- Output: a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` carries the primary `Classification` AND the `Classifications` set flat. `Graph/element#NODE_MODEL` `Node.Object.AllClassifications` closes that storage split at its own owner, and these reads translate across systems without choosing an order-dependent head and roll up only through projector-supplied authoritative relations.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError]`, `[MemberEqualityComparer<TAccessor, TMember>]`, `[SmartEnum<string>]`), LanguageExt.Core (`Option`/`Seq`/`Fin` + the `Validation<Error,_>` slots the admission accumulates), NodaTime (`LocalDate`).
- Growth: a new standard system or edition is data; a resolved annotation is one equality-excluded member. Hierarchy, translation, and dictionary links are `Rasm.Bim`'s bSDD-resolved concern (`Semantics/classification` `BsddRef` `Parent`/`Ancestry`/`Children`) — a contract-side crosswalk module is the deleted form, and a branch query rides `Query/predicate#ELEMENT_PREDICATE` `ByClassification` carrying a Bim-resolved closure. Code punctuation never implies hierarchy because MasterFormat, Uniformat, and external dictionaries do not share one syntax.
- Boundary: `Classification` is ONE generic value-object — a per-system type or an `IfcClass`-style entity-class roster on the contract is the deleted form (the `IfcClass` roster and the `PredefinedType` valid-set are `Rasm.Bim`'s IFC-schema concern, never lowered into the contract); the `System` is an opaque token the contract never validates against a roster, the projector validating the code shape and resolving the bSDD class + `Source`/`Edition`/`Title` at ingest before lowering; identity is the `(System, Code, Edition)` triple — `Edition` is IDENTITY because a publisher re-editions a code's MEANING under one token (a `Source`/`EditionDate`/`Title` difference, by contrast, never fragments a node's content key), so the `Graph/element#NODE_MODEL` `CanonicalBytes` projection writes `System`/`Code`/`Edition` (for the primary `Classification` AND each deterministically-ordered member of the `Classifications` set, never the annotation bundle) and identity stays annotation-stable across runtimes; the `Object` node carries the typed triple with the `Classifications` set so a query matches a code (the `Query/predicate#ELEMENT_PREDICATE` `ByClassification` leaf over the co-applied set, its branch closure Bim-resolved) rather than a stringly-keyed property lookup — a free `string` classification field on a node is the named defect; the `Code` parent-derivation is a pure projection over the admitted code, never a stored parent edge or a per-call regex; the `IfcClassificationReference.Location` dictionary URI is NOT a shared member — it is fully derivable from `(System, Code)` through the projector's roster, so lowering it as a stored annotation duplicates the roster.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Classification;

// --- [TYPES] ---------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Classification {
    [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    public string System { get; }
    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string Code { get; }
    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string Edition { get; }
    public Option<string> Source { get; }
    public Option<LocalDate> EditionDate { get; }
    public Option<string> Title { get; }

    public Option<Classification> Admitted => System is { Length: > 0 } ? Some(this) : None;

    public static Fin<Classification> Of(
        string system, string code, string edition = "",
        Option<string> source = default, Option<LocalDate> editionDate = default, Option<string> title = default) =>
        Accumulate(Seq(
            Gate(!string.IsNullOrWhiteSpace(system), "classification-system",
                static (label, op) => (Error)new KernelFault.InvalidValue(label, "not be blank")),
            Gate(!string.IsNullOrWhiteSpace(code), "classification-code", key,
                static (label, op) => (Error)new KernelFault.InvalidValue(label, "not be blank"))))
         .Map(_ => new Classification(
            system.Trim().ToLowerInvariant(), code.Trim(), edition.Trim(),
            source.Map(static value => value.Trim()), editionDate, title.Map(static value => value.Trim())))
         .ToFin();
}

```

## [03]-[DISCIPLINE_AXIS]

- Owner: `Discipline` the `[SmartEnum<string>]` analysis-discipline vocabulary keyed on a stable lowercase token — the single discriminant a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case maps to, an `Assessment/assessment#ASSESSMENT_NODE` node keys on, and the `Rasm.Compute` analysis route selects by; each row names its governing standard on its own declaration comment (the provenance answer), and the roster carries NO display, banding, or dispatch columns — 51 declared facts with zero readers were the deleted form, and the Compute dispatch axis lands as its own escalation (E-E16) naming the column it then needs.
- Entry: the declaration list IS the vocabulary; a wire/route token admits through the kernel `FactoryBridge.Accept<Discipline>(token)` bridge, `Get`/`TryGet` resolve a row for a trusted token, `Items` enumerates the roster, and the `[SmartEnum<string>]` key codec round-trips the token at the wire so an `Assessment` node persists `Discipline.Energy` as `"energy"` and re-admits it.
- Auto: `Discipline` dispatches through the generated total `Switch` (a consumer routes on the row, never a `string` compare); the kernel admission bridge reuses the generated zero-allocation key lookup so admission and dispatch share one vocabulary and a non-standard token is a result-returning rejection, never a silent miss.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`).
- Growth: a new analysis discipline is one `Discipline` row carrying its token, title, and physical flag the `MaterialPropertySet`/`Assessment`/analysis route all read — the `Electrical` row is that arm executed; blast mints no row (no consumer exists anywhere in the corpus) and glare mints no row (glare evaluation is the `Daylight` row's own analysis route, the python runtime's imageless-annual-glare recipe keying `Daylight`); never a parallel discipline enum per consumer and never a `bool IsThermal` flag set the implementation re-derives.
- Boundary: `Discipline` is the ONE analysis axis — a `StructuralDiscipline`/`ThermalDiscipline` parallel enum or a per-consumer discipline string is the deleted form; the rows are the closed roster BOTH consumers key into: the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` `[Union]` maps its twelve cases onto eleven rows (`Mechanical` and `Orthotropic` share `Structural`; `Damping` keys `Dynamic`; `Optical` keys `Energy`; `Hygrothermal`/`Durability`/`Electrical` their namesake rows) and the `Assessment/assessment#ASSESSMENT_NODE` payload keys on ANY row; the case-to-discipline correspondence is owned ONCE by `MaterialPropertySet.Discipline` (`Composition/material#MATERIAL_PROPERTY`), this axis carrying only the neutral row and never re-stating the map, so a future material-borne discipline (a `Water` absorption case, a `Circularity` disassembly case) is one `MaterialPropertySet` case carrying its existing row — zero edits here; the row carries NO aggregation/solver-route policy, NO display/banding columns, and NO governing-standard roster — the standards ride each row's declaration comment as provenance, the route roster (`"iso-6946-u"`, `"en1998-response"`) staying `Rasm.Compute`'s, so a column encoding "how this discipline aggregates across plies" or "which standard solves it" is a strata leak the contract refuses, the axis staying the pure neutral routing vocabulary.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Discipline {
    public static readonly Discipline Structural = new("structural");
    public static readonly Discipline Seismic = new("seismic");
    public static readonly Discipline Wind = new("wind");
    public static readonly Discipline Dynamic = new("dynamic");
    public static readonly Discipline Thermal = new("thermal");
    public static readonly Discipline Hygrothermal = new("hygrothermal");
    public static readonly Discipline Energy = new("energy");
    public static readonly Discipline Daylight = new("daylight");
    public static readonly Discipline Acoustic = new("acoustic");
    public static readonly Discipline Fire = new("fire");
    public static readonly Discipline Circulation = new("circulation");
    public static readonly Discipline Water = new("water");
    public static readonly Discipline Electrical = new("electrical");
    public static readonly Discipline Durability = new("durability");
    public static readonly Discipline Circularity = new("circularity");
    public static readonly Discipline Environmental = new("environmental");
    public static readonly Discipline Cost = new("cost");

}
```

## [04]-[IMPLEMENTATION_LAW]

- [CLASSIFICATION_NEUTRALITY]: `Classification` carries the `(system, code, edition)` IDENTITY with the projector-resolved `Source`/`EditionDate`/`Title` annotations, and the standard-systems roster, bSDD dictionary URI, code-shape regex, `IfcClass` enum, `PredefinedType` valid-set, and `IfcRelAssociatesClassification` round-trip all stay the `Rasm.Bim` `Semantics/classification` projector's, which lowers a bSDD-resolved reference at ingest so an `Object` node reads triple and name in one hop.
- [ANNOTATION_STABILITY]: identity is the marked `(System, Code, Edition)` triple alone, so the resolved annotation bundle is equality- and hash-excluded and the `Graph/element#NODE_MODEL` `CanonicalBytes` projection writes `System`+`Code`+`Edition` only — a node's content key stays stable whether or not name, publisher, and date resolved.
- [EDITION_SCOPING]: `Edition` is identity because a code's MEANING is edition-scoped — a publisher re-editions a code's definition without changing its token, so `("uniclass2015","Ss_25_10","2015")` and that token under a later edition are DISTINCT concepts a re-ingest holds apart. Hierarchy never derives from punctuation, and the contract derives NO hierarchy at all: ancestry is `Rasm.Bim`'s bSDD-resolved concern, a branch query carrying the resolved closure through `Query/predicate#ELEMENT_PREDICATE` `ByClassification`.
- [DISCIPLINE_COVERAGE]: `Discipline` rows close the real AEC analysis space — structural mechanics through building physics to lifecycle, declaration-ordered by that grouping — each row naming its governing standard on its own declaration comment, so the `Rasm.Compute` analysis route, the typed material property, and the assessment payload share one axis and a new discipline is one row.
- [DISCIPLINE_MAPPING]: `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` owns the case→discipline map alone — `Mechanical`/`Orthotropic` both `Structural`, `Damping` the `Dynamic` row it seats in the roster, `Optical` the `Energy` row because a glazing's solar-optical constants ARE the material-borne energy-simulation input — and a row with no intrinsic single-material property simply owns no case in that map — never a per-row exception.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
