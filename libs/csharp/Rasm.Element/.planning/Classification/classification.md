# [ELEMENT_CLASSIFICATION]

The two neutral cross-cutting axes every node and assessment keys on: the generic `Classification` `[ComplexValueObject]` (`System` + `Code` identity plus the projector-resolved `Title`) an `Graph/element#NODE_MODEL` `Object` node carries, and the one `Discipline` `[SmartEnum<string>]` (`Structural`/`Thermal`/`Energy`/`Acoustic`/`Fire`/`Environmental`/`Cost`) a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case and an `Assessment/assessment#ASSESSMENT_NODE` node key on. Both are LIBRARY-NEUTRAL: `Classification` is a `(system, code)` IDENTITY plus an equality-excluded resolved `Title` annotation — NO standard-systems roster, NO bSDD dictionary, NO code-shape regex, NO `IfcClass` entity-class enum — the `Rasm.Bim` `Semantics/classification` projector owning the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved `(system, code, title)` onto a seam `Classification` at ingest so a consumer reads `element.Classification.Code` AND its human `Title` in one hop without learning the IFC schema or re-querying bSDD; an object's co-applied standard references (Uniclass + OmniClass together) ride the node's `Classifications` `Seq<Classification>`, the entity-class pair staying the primary `Classification` the IFC reads key on.

The seam never re-opens the IFC-schema strata: the `IfcClass` roster and the `PredefinedType` valid-set are a `Rasm.Bim` egress concern validated at `Emit` (`Graph/element#NODE_MODEL`), not an axis here. `Discipline` is the single discriminant the typed property, the assessment payload, and the `Rasm.Compute` analysis route all share: a `MaterialPropertySet.Thermal` maps to `Discipline.Thermal`, an `Assessment` keyed `Discipline.Energy` carries an energy-simulation receipt, and a future analysis discipline is one row — never a parallel enum per consumer, never a `bool IsThermal` the implementation re-derives. Both admit through the seam's `Fin<T>` rail (`Classification.Of`, `Discipline.Parse`) railing `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`, the closed-vocabulary identity making a blank pair or an unknown discipline token unrepresentable downstream.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: the `Classification` `[ComplexValueObject]` (`System` + `Code` identity under split comparer policy, the equality-excluded resolved `Title`), the railed `Of` admission, the derived `Parent`/`Ancestors`/`Root` hierarchy, and the `Within` containment predicate an `Object` node carries (as its primary `Classification` plus the `Classifications` `Seq` of co-applied standard-system references).
- [02]-[DISCIPLINE_AXIS]: the `Discipline` `[SmartEnum<string>]` seven-row analysis vocabulary keyed on a wire token with its `Title`/`Physical` columns and the railed `Parse` admission — the one axis the typed property, the assessment, and the analysis route share.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `Classification` the `[ComplexValueObject]` carrying the `System` token (the standard identifier — `"uniclass2015"`, `"omniclass"`, `"masterformat"`, a free lowercased string the seam never enumerates), the `Code` (the code-within-the-system, trimmed), and the projector-resolved human `Title` (the classified concept's name, e.g. `"Cavity external wall systems"` — `None` for an author-minted or hierarchy-derived code) the generic pair an `Graph/element#NODE_MODEL` `Object` node carries as its primary (entity-class-keying) `Classification`, the additional standard-system references riding the node's `Classifications` `Seq<Classification>` (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object); identity is the `(System, Code)` pair under split comparer policy (ordinal `System`, ordinal-ignore-case `Code`), `Title` an equality-/hash-/diagnostic-excluded annotation a `Rasm.Bim` projector lowers from the resolved bSDD / `IfcClassificationReference` record.
- Entry: `Classification.Of(system, code, key)` admits the pair through the generated `[ComplexValueObject]` factory on the seam `Fin<T>` rail — both members trimmed (the `System` lowercased), a blank system or code railing `ElementFault.ValueRejected` re-keyed to the caller's `Op`; the projector path passes the resolved name (`Of(system, code, key, Some(title))`), the author path omits it (`Title` defaulting `None`). `Create`/`TryCreate`/`Validate` carry the same `[ValidationError<ElementFault>]` admission for the rare non-railed caller, and `Parent`/`Ancestors`/`Root`/`Within` are the derived hierarchy reads.
- Auto: the `[ComplexValueObject]` generates structural equality + hashing over ONLY the marked `(System, Code)` members under the split comparer policy (so two references to one code with different resolved titles stay equal, the unmarked `Title` left out of equality, hashing, and diagnostic text), and the `ValidateFactoryArguments` hook trims/lowercases by `ref` and rejects a blank pair into an `ElementFault`; `Parent` derives the containing code by trimming the last separator segment over a `SearchValues<char>` span scan, `Ancestors` unfolds the full parent chain by recursion, `Root` resolves the top-level branch, and `Within` tests system-equal SEPARATOR-BOUNDED prefix containment (a sibling sharing a raw textual prefix never false-matches, so `Within` agrees exactly with `Ancestors`) — every hierarchy read a pure projection over the admitted code, never a stored parent edge, a per-call `char[]` allocation, or a per-call regex.
- Receipt: a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` carries the primary `Classification` AND the `Classifications` set flat — `element.Classification.Code` keys the IFC entity class (the one every `Rasm.Bim` query/egress/diff reads), the `element.Classifications` set carries the additional standard-system references (a Uniclass AND an OmniClass code co-applied to one object, the cardinality `IfcRelAssociatesClassification` admits) each reading its human `Title` without a bSDD round-trip, `element.Classifications.Exists(c => c.Within(branch))` selects every element under a classification branch, and a matched `c.Ancestors`/`Root` rolls a quantity/cost report up the classification tree — "has it all" including EVERY co-applied classification and its resolved name in one read, never a stranded code a consumer must re-resolve nor a co-applied standard silently dropped to a single field.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ValidationError<TError>]`, `[MemberEqualityComparer<TAccessor, TMember>]`, `ComparerAccessors.StringOrdinal`/`StringOrdinalIgnoreCase`), LanguageExt.Core (`Option`/`Seq`/`Fin`), System.Buffers (`SearchValues<char>`), `Rasm` (the kernel `Op` op-key).
- Growth: a new standard system is one `System` token a projector supplies (the seam never grows a row); a new code-derivation rule is one projection on `Classification` (the `Parent`/`Ancestors`/`Root`/`Within` family); a resolved annotation a consumer wants flat (a definition URI, an edition date) is one equality-excluded member the projector lowers; never a per-system `Uniclass`/`OmniClass` value-object and never a standard-systems roster, bSDD dictionary, or code-shape policy on the seam — those are the downstream projector's, the seam carrying the typed pair plus the resolved name.
- Boundary: `Classification` is ONE generic value-object — a per-system type or an `IfcClass`-style entity-class roster on the seam is the deleted form (the `IfcClass` roster and the `PredefinedType` valid-set are `Rasm.Bim`'s IFC-schema concern, never lowered into the seam); the `System` is an opaque token the seam never validates against a roster, the projector validating the code shape and resolving the bSDD class + `Title` at ingest before lowering; identity is the `(System, Code)` pair so a resolved-`Title` difference never fragments a node's content key (the `Graph/element#NODE_MODEL` `ToCanonicalBytes` writes only `System`/`Code` — for the primary `Classification` AND each deterministically-ordered member of the `Classifications` set, never `Title` — so identity stays title-stable across runtimes) and the `Object` node carries the typed `Classification` plus the `Classifications` set so a query matches a pair (`Within` for a branch over the co-applied set) rather than a stringly-keyed property lookup — a free `string` classification field on a node is the named defect; the `Code` parent-derivation is a pure projection over the admitted code, never a stored parent edge or a per-call regex.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ElementFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Classification {
    // Identity is the (System, Code) pair under SPLIT comparer policy (ordinal System — already lowercased at
    // admission — and ordinal-ignore-case Code); marking these two opts equality IN to them, so the unmarked
    // Title is materialized but excluded from equality/hashing/diagnostics. Title is the Rasm.Bim-resolved human
    // name lowered from the bSDD/IfcClassificationReference record, so identity (and the System+Code-only
    // Graph/element#NODE_MODEL ToCanonicalBytes) stays title-stable while a consumer still reads the name flat.
    [MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    public string System { get; }
    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string Code { get; }
    public Option<string> Title { get; }

    // The neutral hierarchy separators a code's parent prefix is trimmed at — a system-agnostic heuristic
    // (Uniclass `_`, OmniClass `-`/` `, MasterFormat ` `), the seam never learning a per-system scheme.
    private static readonly SearchValues<char> Separators = SearchValues.Create("_-. ");

    static partial void ValidateFactoryArguments(ref ElementFault? validationError, ref string system, ref string code, ref Option<string> title) {
        if (string.IsNullOrWhiteSpace(system) || string.IsNullOrWhiteSpace(code)) {
            validationError = ElementFault.ValueRejected(Op.Of(name: nameof(Classification)), $"classification requires a non-blank system and code; got '{system}':'{code}'");
            return;
        }
        system = system.Trim().ToLowerInvariant();
        code = code.Trim();
        title = title.Map(static t => t.Trim());
    }

    // The seam-rail admission a Rasm.Bim projector / author path takes: the projector supplies the resolved Title,
    // the author omits it (None); a blank pair rails ElementFault.ValueRejected re-keyed to the CALLER's Op so the
    // operation context survives (the keyless ValidateFactoryArguments fault is re-stamped here).
    public static Fin<Classification> Of(string system, string code, Op key, Option<string> title = default) =>
        Validate(system, code, title, out Classification value) is { } fault
            ? ElementFault.ValueRejected(key, fault.Message)
            : Fin.Succ(value);

    // Containment derived from the code, never a stored parent edge: the parent is the prefix up to the last
    // separator (infallible — a positive cut yields a non-empty valid prefix; the derived branch drops Title);
    // a flat code (no separator, or a leading one) has no parent.
    public Option<Classification> Parent =>
        Code.AsSpan().LastIndexOfAny(Separators) is var cut and > 0
            ? Some(Create(System, Code[..cut], None))
            : None;

    // The full parent chain nearest-first ([Ss_25_10, Ss_25, Ss] for Ss_25_10_30) a classification-tree
    // quantity/cost rollup folds over — a pure unfold of Parent, never a stored ancestry.
    public Seq<Classification> Ancestors =>
        Parent.Match(Some: static p => Seq1(p) + p.Ancestors, None: static () => Seq<Classification>());

    // The top-level branch (self for a flat code) a system-root grouping keys on.
    public Classification Root => Parent.Match(Some: static p => p.Root, None: () => this);

    // System-equal hierarchy containment (reflexive): a code is Within a branch iff the branch IS the code OR the branch
    // is a SEPARATOR-BOUNDED prefix of it — the SAME boundary Parent/Ancestors cut at — so Within(branch) holds for exactly
    // {branch} ∪ its descendants and a sibling sharing a raw textual prefix (Ss_25_100 under Ss_25_10) never false-matches;
    // a query selecting an element subtree tests one predicate rather than walking the ancestry. A bare StartsWith is the
    // deleted form because it over-matches every code that merely begins with the branch text, breaking the c.Within(b) ⟺
    // b ∈ {c} ∪ c.Ancestors equivalence the hierarchy derivation guarantees.
    public bool Within(Classification branch) =>
        string.Equals(System, branch.System, StringComparison.Ordinal)
        && (Code.Equals(branch.Code, StringComparison.OrdinalIgnoreCase)
            || (Code.Length > branch.Code.Length
                && Code.StartsWith(branch.Code, StringComparison.OrdinalIgnoreCase)
                && Separators.Contains(Code[branch.Code.Length])));
}
```

## [03]-[DISCIPLINE_AXIS]

- Owner: `Discipline` the `[SmartEnum<string>]` seven-row analysis-discipline vocabulary keyed on a stable lowercase token, each row carrying its `Title` display name and its `Physical` flag (a measured-physics discipline a `Rasm.Compute` solver computes versus a procurement/lifecycle one a catalog/EPD supplies) — the single discriminant a `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` case maps to, an `Assessment/assessment#ASSESSMENT_NODE` node keys on, and the `Rasm.Compute` analysis route selects by.
- Entry: the declaration list IS the vocabulary; `Discipline.Parse(token, key)` admits a wire/route token on the seam `Fin<T>` rail (railing `ElementFault.ValueRejected` on an unknown token — the seam-consistent admission the sibling `Composition/material#MATERIAL_PROPERTY` `FireRating.Parse`/`Currency.Parse` share), `Get`/`TryGet` resolve a row for a trusted token, `Items` enumerates the seven, and the `[SmartEnum<string>]` key codec round-trips the token at the wire so an `Assessment` node persists `Discipline.Energy` as `"energy"` and re-admits it.
- Auto: `Discipline` dispatches through the generated total `Switch` (a consumer routes on the row, never a `string` compare); the `Physical` column is the analysis-dispatch axis the `Rasm.Compute` route reads (solver-input versus catalog-lookup) rather than a per-discipline branch; `Parse` reuses the generated zero-allocation key lookup so admission and dispatch share one vocabulary and a non-standard token is a railed rejection, never a silent miss.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`), `Rasm` (the kernel `Op` op-key).
- Growth: a new analysis discipline is one `Discipline` row carrying its token, title, and physical flag — a damping/seismic/daylight discipline lands as one row the `MaterialPropertySet`/`Assessment`/analysis route all read; never a parallel discipline enum per consumer and never a `bool IsThermal` flag set the implementation re-derives.
- Boundary: `Discipline` is the ONE analysis axis — a `StructuralDiscipline`/`ThermalDiscipline` parallel enum or a per-consumer discipline string is the deleted form; the seven rows are the closed set, total over BOTH the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` `[Union]` (six cases each map to a discipline; `MaterialPropertyAccess.ForDiscipline` returns `None` for `Energy`, which has no intrinsic single-material property — a whole-element simulation, not a material constant) AND the `Assessment/assessment#ASSESSMENT_NODE` payload (any discipline, including `Energy`), so the property family and the assessment family share one axis without a parallel vocabulary; the case-to-discipline correspondence is owned ONCE by `MaterialPropertySet.Discipline` (`Composition/material#MATERIAL_PROPERTY`), this axis carrying only the neutral row and never re-stating the map; the `Physical` column distinguishes a solver-input discipline from a catalog-lookup one so the analysis dispatcher routes by the column, never an `if discipline == Structural` chain.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Discipline {
    public static readonly Discipline Structural = new("structural", "Structural", physical: true);
    public static readonly Discipline Thermal = new("thermal", "Thermal", physical: true);
    public static readonly Discipline Energy = new("energy", "Energy", physical: true);
    public static readonly Discipline Acoustic = new("acoustic", "Acoustic", physical: true);
    public static readonly Discipline Fire = new("fire", "Fire", physical: true);
    public static readonly Discipline Environmental = new("environmental", "Environmental", physical: false);
    public static readonly Discipline Cost = new("cost", "Cost", physical: false);

    public string Title { get; }
    // The analysis-dispatch axis Rasm.Compute routes on: a solver-computed measured-physics discipline (Structural
    // FEA, ISO 6946 thermal, EnergyPlus, ISO 12354 acoustic, EN fire) versus a catalog/EPD-supplied procurement-
    // lifecycle one (EN 15978 environmental, cost) — read as a column, never re-derived per consumer.
    public bool Physical { get; }

    // The seam-rail admission a wire/route token takes — the FireRating.Parse/Currency.Parse-consistent form
    // railing ElementFault.ValueRejected on an unknown token; Get/TryGet stay the trusted-token resolvers.
    public static Fin<Discipline> Parse(string token, Op key) =>
        TryGet(token, out Discipline? discipline) && discipline is { } row ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<discipline-unknown:{token}>");
}
```

## [04]-[RESEARCH]

- [CLASSIFICATION_NEUTRALITY]: the seam `Classification` carries the `(system, code)` IDENTITY plus the projector-resolved `Title` — the standard-systems roster (`Uniclass2015`/`OmniClass`/`MasterFormat`/`Uniformat`), the bSDD dictionary URI, the code-shape regex, and the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip stay the `Rasm.Bim` `Semantics/classification` projector's, which resolves a code against the live bSDD service and lowers the resolved `(system, code, title)` onto a seam `Classification` at ingest; identity is the marked `(System, Code)` pair so the resolved `Title` is an equality-/hash-excluded annotation (a node's content key, written `System`+`Code` only by `Graph/element#NODE_MODEL` `ToCanonicalBytes`, stays stable whether or not the name resolved), the seam never re-opening the IFC-schema strata — the `IfcClass` entity-class enum and the `PredefinedType` valid-set are a Bim egress concern validated at `Emit`, never an axis here, and the `Object` node carries the generic pair plus the human name a consumer reads in one hop.
- [DISCIPLINE_TOTALITY]: the seven `Discipline` rows are total over both the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet` `[Union]` (six cases, one per physical/lifecycle discipline) and the `Assessment/assessment#ASSESSMENT_NODE` payload (any discipline, including `Energy`), so the analysis route the `Rasm.Compute` solver selects, the typed material property, and the assessment receipt share one axis; `Energy` is the discipline carried only on an `Assessment` (a whole-element simulation has no intrinsic single-material property, so `ForDiscipline(Energy)` is `None` by design, not a gap), and the `Physical` column separates a measured-physics discipline (a solver input) from a procurement/lifecycle one (a catalog lookup) so the dispatcher routes by the column rather than a per-discipline branch — the case-to-discipline map staying `MaterialPropertySet`'s single owner.
