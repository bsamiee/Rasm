# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the seam `Classification` value an `Object` node carries: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/ETIM/IfcClassification), each row carrying its bSDD dictionary identity (the version-free `Stem` + the hosted `Version` token deriving the versioned `DictionaryUri` the live resolution and the egress `Location` use) and its code-shape policy, validating a raw code against the system's shape and LOWERING it onto the library-neutral seam `Classification` value — the SIX-member factory `(System, Code, Edition, Source, EditionDate, Title)`: the `(System, Code, Edition)` edition-scoped IDENTITY plus the equality-excluded resolved-annotation bundle. The seam owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (no roster, no dictionary, no regex); this page is the downstream Bim projector the seam names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto the seam value at ingest and re-authoring that value at egress. A classification is the seam `Classification` VALUE on an `Object` node: `Relations/relation#EDGE_ALGEBRA` is explicit that classification is a value on the node, NOT an edge (the neutral `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the retired `(GlobalId, system, code)` triple bound to a second stored `BimElement` record is GONE — a query reads `node.Classification.System`/`Code`, never a stringly-keyed lookup against a second element store.

The round-trip is BIDIRECTIONAL across three entries: `Classify` lowers a validated raw code at authoring ingress, `Ingest` resolves an imported `IfcClassificationReference` back to the seam value at import ingress (the inverse of `Author`, the leg the migration source never had), and `Author` re-authors a node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress — the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node, which REPLACES the retired `Rasm.Materials` `MaterialPropertyWire.Classification` half (a material carries no classification; the `Object` node does): the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands a substance's standard `(system, code)` as the bound element's `Object`-node `Classification` value through its `MaterialBinding` egress, so the element classification this owner round-trips is the one the unified Component projection authored, never a material-wire field. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping carries the FULL `ClassContract.v1` surface: the class-scoped constraint surface (IFC `DataType` + Pset placement, `ValueKind`, allowed values, XSD `Pattern`, numeric `Bounds`, the seven SI exponents + `Units`, `Status`) that feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template, the `Review/validation#IDS_FACETS` Classification + Property facets, and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion directly, PLUS the relation set (`classRelations`/`reverseClassRelations` → typed `BsddRelation` rows whose `IsEqualTo`/`IsSynonymOf` edges the `BsddFederation` receipt closes through the shared QuikGraph substrate into cross-standard equivalence — `Translate` lowers an OmniClass code onto its Uniclass peer, a capability unreachable from any code string), the AUTHORITATIVE containment (`parentClassReference`/`hierarchy`/`childClassReferences` → `BsddRef` — the parent a MasterFormat/Uniformat code string does not encode), and the supersession surface (`status`/`replacedObjectCodes`/`replacingObjectCodes`/`deprecationExplanation` — `BsddClass.Admit` refuses to certify a NEW code onto an `Inactive` class, carrying the replacing code in the fault). `BsddResolution.Certify` is the dictionary-certified authoring lowering (`Resolve` → `Admit` → `Classify`) and `BsddResolution.Search` resolves a concept label or IFC entity to candidate codes (`api/Class/Search/v1`) — so a new standard is one `ClassificationSystem` dictionary-identity row shared across `classification`, `properties`, and `validation`. The typed `Model/faults#FAULT_BAND` `BimFault` cases lift BARE onto the `Fin` rail (band 2600 IS the `Expected` `Code`; no `.ToError()` hop), each carrying the kernel `Op` operation context the caller threads.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary URI, code-shape policy); `Classify(code, key)` lowering a validated raw code onto a seam `Classification` value at authoring ingress; `Ingest(reference)` resolving an imported `IfcClassificationReference` back onto a seam value at import ingress; and `Author(db, related, classification)` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` (the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes).
- [02]-[BSDD_RESOLUTION]: the live bSDD dictionary resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, projecting the FULL `ClassContract.v1` surface — the constraint surface (`ValueKind`/`AllowedValues`/`Pattern`/`Bounds`/`Exponents`/`Units`), the relation set (`BsddRelation` forward + reverse), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`) — degrading to the row's local code-shape policy; `BsddResolution.Certify` the supersession-gated authoring lowering, `BsddResolution.Search` the `api/Class/Search/v1` concept-to-code resolution, and `BsddFederation` the QuikGraph transitive-closure equivalence receipt whose `Translate` lowers a code across standards onto the seam value; feeding the `Semantics/properties#PROPERTY_TEMPLATES` template, the IDS facets, and the seam `MeasureValue` coercion.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row carrying the system title, the bSDD dictionary identity — the version-free `Stem` (the ingest prefix identity a foreign-edition `Location` still matches) plus the hosted `Version` token deriving the versioned `DictionaryUri` (the bSDD identifier scheme is `{org}/{dictionary}/{version}`; a versionless class URI does not resolve, so the version-bearing row IS the live-lane admission and an unhosted system carries `Version ""` whose live leg degrades by construction) — and the code-shape regex policy; the projector that lowers a resolved standard code onto the seam `Classification/classification#CLASSIFICATION_AXIS` `Classification` value the `Object` node carries and authors it back at egress. The seam `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the seam defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the seam value, never re-declaring a classification value-object beside the seam.
- Entry: `ClassificationSystem.Classify(string code, Op key)` validates the raw code against the system's code-shape regex and lowers it onto a seam `Classification(Key, code)` value — `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`, the typed case lifting BARE off `key`, no `.ToError()` hop); `ClassificationSystem.Ingest(IfcClassificationReference reference)` is the import-ingress inverse resolving the standard system off the reference's `ReferencedSource` root dictionary title, the root's `Specification` dictionary-URI prefix, OR its `Location` identifier-URI prefix against the roster (the code off `Identification` or the trailing `Location` segment, the resolved `Title` off the reference's own `Name`), returning `Option<Classification>` the `Projection/semantic#SEMANTIC_PROJECTOR` ingress accumulates onto the `Graph/element#NODE_MODEL` `Object` node's `Classifications` set (IFC admits MULTIPLE `IfcRelAssociatesClassification` per object) — `None` for an unrostered source so a foreign system rides the `Projection/relations#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification)` is the egress entry the `Emit` composes per `Object` node — `None` for the `"ifc"` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system's edition-scoped dictionary, `Identification` the code, `Location` the identifier URI, and `Name` the seam `Title` — the members `Ingest` reads back, so the six-member value survives the egress leg.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the actual shape enforcement the migration source's decorative `ClassificationCode` never applied) and, on a match, mints the seam `Classification.Create(Key, code, "", None, None, None)` (the author path carries no resolved edition/publisher/title; the seam `[ComplexValueObject]` SIX-member factory `(System, Code, Edition, Source, EditionDate, Title)` normalizing the system token and trimming the code, the edition-unspecified `Edition ""` and the `Source`/`EditionDate`/`Title` `Option` annotation bundle `None`), so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30", "")` whose seam `Parent`/`Within` projections derive the containment hierarchy; a code the shape rejects faults rather than lowering a malformed value. `Ingest` walks `RootSource` to the root `IfcClassification` ONCE: it matches a roster row by the dictionary `Name` equality, the root's `Specification` dictionary-URI through the shared `ByUri` version-free `Stem` prefix (the URI `Author` itself stamps, so the round-trip self-resolves even when a re-export strips per-reference `Location`s), or the reference `Location` prefix (a `Location` minted under ANY dictionary edition — a foreign 2015-edition reference, a future `ifc/4.6` class URI — still resolves its row), reads the code off `CodeOf` (`TailCode`-unescaping the `Location` trailing segment when the `Identification` is absent), and lowers the EDITION-SCOPED annotation bundle off that SAME root — the `Edition` token (IDENTITY on the seam), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed) — while `TitleOf` reads the reference's OWN `IfcClassificationReference.Name` (distinct from the root dictionary `Name`) as the resolved concept `Title`, so the imported leg lands the FULL six-member identity (edition + publisher + date + title) at the only path that can populate it rather than a perpetually edition-blank, title-`None` reference. `Author` resolves the row through the generated `TryGet` and authors the reference with the URI-escaped `Location` (`ClassUri`) so an OmniClass/MasterFormat code carrying spaces round-trips, the reference `Name` carrying the seam `Title` and the per-`(db, system, edition)` memoized dictionary re-stamping the value's `Edition`/`Source`/`EditionDate` — the egress leg of the same six-member identity `Ingest` lowers; the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row carrying its bSDD dictionary stem, version token, and code-shape regex; a dictionary the public bSDD starts hosting is one `Version` token flip (the ETIM row is the live-hosted exemplar beside IFC); the seam `Classification` value-object absorbs any `(system, code)` pair with no seam edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type, never a parallel classification value-object beside the seam, and never a per-direction resolver (one `Classify`/`Ingest`/`Author` triad spans the whole round-trip).
- Boundary: the classification systems are ONE keyed axis (`ClassificationSystem` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the classification VALUE is the seam `Classification` `[ComplexValueObject]` and a Bim `Classification`/`ClassificationCode`/`ClassificationRef` value-object is the deleted form — the seam owns the typed pair, this page owns the standard-systems roster and lowers onto it, so the type name `ClassificationSystem` never collides with the seam `Classification`; the `Classify(BimElement element, …)` binding to a `BimElement.GlobalId` is GONE (the `BimElement`/`BimModel` are retired, the consumer element being the `Graph/element#ELEMENT_GRAPH` `Bake` fold) — a classification is the seam value on the `Object` node, never a `(GlobalId, system, code)` triple keyed to a second element record; classification is a VALUE on the `Object` node and NOT an edge (the seam `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the egress reads the node `Classification` value and a classification-association `Relationship` case is the deleted form; the code shape is the row's regex validated once at `Classify`, never a per-call regex at the call site; the typed `BimFault` lifts BARE off the threaded `Op key` and a `.ToError()` hop or a single-string fault ctor is the named defect this owner closes (the band IS the `Expected` `Code`); the bSDD dictionary is the authoritative live source for the class-to-property constraint surface resolved through the dictionary URI, never a hardcoded code-to-property table that duplicates and drifts from it; the per-system `CodeShape` regex is BOTH the cheap LOCAL shape gate `Classify` admits a raw authoring code through (no network round-trip) AND the offline degradation `BsddResolution.LocalShape` falls back to, never that drifting constraint table; the classification round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`.api/api-geometrygym-ifc`) consumed as settled vocabulary, the egress carrying `Identification` + `Location` (+ the `Name` concept title the seam `Classification.Title` round-trips) so the import `Ingest` reconstructs the seam value losslessly, never re-minting a classification mapping; the egress reads the seam `Object` node `Classification`, NOT a Materials `MaterialPropertyWire.Classification` carrier (retired), the material-wire classification half having moved to this element-classification egress.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;                                       // LocalDate — the seam Classification.EditionDate annotation the Ingest leg lowers off IfcClassification.EditionDate
using QuikGraph;                                      // the shared graph substrate the BsddFederation equivalence closure folds through (never a hand-rolled BFS)
using QuikGraph.Algorithms;
using Rasm.Bim;
using Rasm.Element.Classification;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries

namespace Rasm.Bim.Semantics;

// --- [MODELS] -----------------------------------------------------------------------------
// The standard classification-systems roster lowering onto the seam Classification value-object. The type name
// ClassificationSystem is distinct from the seam Classification so the seam value-object is the one canonical
// classification; this page owns only the roster (dictionary URI + code shape) and the bSDD lane. Classification is a
// VALUE on the Object node (Relations/relation#EDGE_ALGEBRA — never an edge), so this projector lowers a resolved code
// onto that value at ingest (Classify for a raw code, Ingest for an imported IfcClassificationReference) and re-authors
// the node's standard value back onto IfcRelAssociatesClassification at egress (Author).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClassificationSystem {
    // The dictionary identity is (Stem, Version): the bSDD identifier scheme is {org}/{dictionary}/{version} and a
    // VERSIONLESS class URI does not resolve, so a live-hosted row (ifc/etim) pins the hosted version token while an
    // unhosted row (uniclass2015/omniclass/masterformat/uniformat — not on the public bSDD) carries "" and its live
    // leg degrades to the local shape BY CONSTRUCTION; hosting later is one Version token flip, never a shape edit.
    public static readonly ClassificationSystem Uniclass2015 = new("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", "", Shape(@"^[A-Z][A-Za-z]_\d{2}(_\d{2}){0,3}$"));
    public static readonly ClassificationSystem OmniClass     = new("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", "", Shape(@"^\d{2}-\d{2}( \d{2}){2,3}$"));
    public static readonly ClassificationSystem MasterFormat  = new("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", "", Shape(@"^\d{2} \d{2} \d{2}(\.\d{2})?$"));
    public static readonly ClassificationSystem Uniformat     = new("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", "", Shape(@"^[A-Z]\d{4}$"));
    public static readonly ClassificationSystem Etim          = new("etim", "ETIM", "https://identifier.buildingsmart.org/uri/etim/etim", "10.0", Shape(@"^EC\d{6}$"));
    public static readonly ClassificationSystem IfcSystem     = new("ifc", "IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", "4.3", Shape(@"^Ifc[A-Za-z]+$"));

    public string Title { get; }
    public string Stem { get; }
    public string Version { get; }
    public Regex CodeShape { get; }

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape the IfcRelKind roster also takes).
    private ClassificationSystem(string key, string title, string stem, string version, Regex codeShape) : this(key) =>
        (Title, Stem, Version, CodeShape) = (title, stem, version, codeShape);

    // The versioned request/egress identity: the live api/Class/v1 lane and the authored Location/Specification carry
    // it; the version-free Stem stays the ingest prefix identity so the two never drift (one row value, one derivation).
    public string DictionaryUri => Version.Length > 0 ? $"{Stem}/{Version}" : Stem;

    // The code-shape matcher is a compiled NonBacktracking row policy: a Uniclass table prefix is two letters with the
    // second upper OR lower (Ss/Pr/EF/SL/TE), so [A-Z][A-Za-z] admits every real prefix; a new standard is one Shape(...)
    // row, never a per-call-site Regex parse and never a hostile-input backtracking surface.
    static Regex Shape(string pattern) => new(pattern, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);

    // INGRESS (raw code): validate against the system's code shape and lower onto a seam Classification value the Object
    // node carries — the actual shape enforcement, then the seam factory. The author path is edition-UNSPECIFIED (Edition
    // "" — a raw authoring code carries no resolved publisher/edition; the bSDD/IfcClassification annotations are an Ingest
    // concern), so the seam six-member [ComplexValueObject] Create takes the edition-unspecified "" and a None
    // Source/EditionDate/Title annotation bundle ("", None, None, None). The typed UnmappedClass case lifts BARE off the
    // caller's Op (band 2600 IS the Expected Code; no .ToError() hop) on a shape reject.
    public Fin<Classification> Classify(string code, Op key) =>
        CodeShape.IsMatch(code.Trim())
            ? Fin.Succ(Classification.Create(Key, code, "", None, None, None))
            : Fin.Fail<Classification>(new BimFault.UnmappedClass(key, $"classification-code-reject:{Key}:{code}"));

    // The dictionary class URI the bSDD resolution fetches AND the IfcClassificationReference.Location the egress writes:
    // the dictionary URI plus the URI-escaped code, so an OmniClass/MasterFormat code carrying spaces (e.g. "23-13 35 00")
    // produces a valid request/round-trip URI Ingest can unescape back to the code.
    public string ClassUri(string code) => $"{DictionaryUri}/class/{System.Uri.EscapeDataString(code.Trim())}";

    // The roster resolver every URI-shaped read shares (Ingest Location, Search hit dictionary, federation Translate):
    // prefix on the version-FREE Stem, LONGEST stem winning so a nested-stem row (one dictionary extending a sibling's
    // URI space) resolves by specificity, never by declaration order.
    internal static Option<ClassificationSystem> ByUri(string uri) =>
        uri is { Length: > 0 }
            ? Optional(Items.Where(row => uri.StartsWith(row.Stem, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(static row => row.Stem.Length).FirstOrDefault())
            : None;

    // The identifier-URI code authority: the trailing segment unescaped ({...}/class/{code} — the inverse of ClassUri).
    internal static string TailCode(string uri) => System.Uri.UnescapeDataString(uri[(uri.LastIndexOf('/') + 1)..]);

    // INGRESS (imported IFC): an IfcClassificationReference -> the seam standard Classification the projector lands on the
    // Object node value (the inverse of Author, the leg the migration source never had). The root IfcClassification
    // dictionary is walked ONCE: the system resolves off its Name, its Specification dictionary-URI (the IFC4X3 rename of
    // the [Obsolete] Location — the very URI Author stamps, so a re-export that strips per-reference Locations still
    // self-resolves, and a third-party export carrying only the dictionary-level URI resolves too), OR the reference's
    // Location identifier-URI prefix against the roster, and the EDITION-SCOPED annotation bundle the seam Classification
    // carries (the publisher Source, the
    // string Edition, the EditionDate revision date — IfcClassification.Source/Edition/EditionDate decompile-confirmed,
    // .api/api-geometrygym-ifc row 08) lowers off that SAME root so the imported leg lands the full six-member identity,
    // never a perpetually-edition-blank reference. The dictionary Edition is IDENTITY on the seam (a re-editioned re-ingest
    // must not collide §EDITION_SCOPING), so reading it off the root is the only ingest path that can populate it. None for
    // an unrostered source so a foreign system rides the Generic passthrough rather than a wrong lowering, and a blank code
    // yields None rather than throwing through the seam Create.
    public static Option<Classification> Ingest(IfcClassificationReference reference) {
        IfcClassification? dictionary = RootSource(reference);
        return (Optional(Items.FirstOrDefault(row => string.Equals(row.Title, dictionary?.Name ?? "", StringComparison.OrdinalIgnoreCase)))
            | ByUri(dictionary?.Specification ?? "")
            | ByUri(reference.Location ?? ""))
            .Bind(system =>
                CodeOf(reference) is { Length: > 0 } code
                    ? Some(Classification.Create(
                        system.Key, code,
                        dictionary?.Edition?.Trim() ?? "",                          // the edition token (IDENTITY) off the root dictionary, "" when unspecified
                        SourceOf(dictionary),                                       // the publisher annotation (equality-excluded), None when unset
                        EditionDateOf(dictionary),                                  // the revision date annotation (equality-excluded)
                        TitleOf(reference)))                                        // the resolved concept title off the reference's own Name
                    : None);
    }

    // Walk ReferencedSource (an IfcClassificationReferenceSelect = IfcClassification | IfcClassificationReference) up the
    // hierarchy to the root dictionary; the depth bound makes a malformed cyclic chain terminate rather than spin. A nested
    // IfcClassificationReference (the IFC hierarchical-classification pattern) points its ReferencedSource at a PARENT
    // reference, not the dictionary, so a flat `as IfcClassification` would read "" on a nested ref and silently miss the
    // dictionary — the walk is what lets Ingest read the dictionary Name/Specification AND its Source/Edition/EditionDate.
    static IfcClassification? RootSource(IfcClassificationReference reference) {
        IfcClassificationReferenceSelect? source = reference.ReferencedSource;
        for (int depth = 0; source is IfcClassificationReference parent && depth < 32; depth++) {
            source = parent.ReferencedSource;
        }
        return source as IfcClassification;
    }

    // The IfcClassification.EditionDate -> the seam Option<LocalDate> annotation: the GeometryGym DateTime sentinel is
    // DateTime.MinValue (an unset edition date), mapped to None so an undated dictionary lowers a date-free annotation
    // rather than a spurious 0001-01-01; a real date lowers through NodaTime LocalDate.FromDateTime.
    static Option<LocalDate> EditionDateOf(IfcClassification? dictionary) =>
        dictionary is { EditionDate: var date } && date > System.DateTime.MinValue
            ? Some(LocalDate.FromDateTime(date))
            : None;

    // The IfcClassification.Source publisher -> the seam Option<string> annotation: an unset/blank source lowers None
    // rather than a Some("") sentinel (the seam no-sentinel law the Title/EditionDate annotations already hold), so a
    // publisher-free dictionary lands a source-free Classification the equality/content key never forks on.
    static Option<string> SourceOf(IfcClassification? dictionary) =>
        dictionary?.Source is { Length: > 0 } source ? Some(source.Trim()) : None;

    static string CodeOf(IfcClassificationReference reference) =>
        reference.Identification is { Length: > 0 } id ? id
        : reference.Location is { Length: > 0 } loc ? TailCode(loc)
        : "";

    // The human title the seam Classification.Title carries at ingest: IfcClassificationReference.Name (the
    // IfcExternalReference.Name label, decompile-confirmed `public string Name` on the GeometryGym base —
    // .api/api-geometrygym-ifc row 07), the classified concept's resolved name (e.g. "Cavity external wall systems");
    // None for a blank Name so an untitled reference lowers a title-free Classification rather than an empty-string
    // annotation that would read as a present-but-blank title downstream. Without this the seam Title is dead at the
    // ONLY ingest path that can populate it — the resolved-name read would be perpetually None at IFC import.
    static Option<string> TitleOf(IfcClassificationReference reference) =>
        reference.Name is { Length: > 0 } name ? Some(name.Trim()) : None;

    // The db-scoped dictionary-source memo: ONE IfcClassification per (db, system, EDITION) is shared across every Author
    // call, so an egress that classifies N objects under one Uniclass edition authors ONE dictionary entity, not N
    // duplicates — while a 2015-edition and a 2023-edition value author TWO (seam Edition is IDENTITY, and an
    // IfcClassification IS an edition-scoped dictionary; one shared entity would strip the edition and fork every
    // re-ingested content key). Keyed by the emit DatabaseIfc so the cache is emit-scoped and GC-collected with the
    // database; the emit is db-serial (DatabaseIfc is single-threaded), the dictionary guards reentry.
    static readonly ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<(string System, string Edition), IfcClassification>> Sources = new();

    // The minted dictionary re-stamps the value's edition-scoped bundle — Edition (identity), the Source publisher, the
    // EditionDate revision date (Edition/Source/EditionDate setters decompile-confirmed) — so Ingest reads back the SAME
    // six-member identity Author lowered; an edition-unspecified value ("" + None bundle) authors the bare dictionary.
    static IfcClassification Source(DatabaseIfc db, ClassificationSystem row, Classification classification) =>
        Sources.GetValue(db, static _ => new ConcurrentDictionary<(string, string), IfcClassification>())
            .GetOrAdd((row.Key, classification.Edition), _ => {
                var dictionary = new IfcClassification(db, row.Title) { Specification = row.DictionaryUri, Edition = classification.Edition };
                classification.Source.IfSome(publisher => dictionary.Source = publisher);
                classification.EditionDate.IfSome(date => dictionary.EditionDate = date.ToDateTimeUnspecified());
                return dictionary;
            });

    // EGRESS: the Object node's standard Classification value -> the IfcRelAssociatesClassification/IfcClassificationReference
    // the Projection/egress#IFC_EGRESS Emit composes per Object node — the element-classification egress that REPLACES the
    // retired Materials material-wire classification half (a material carries no classification, the Object node does). None
    // for the "ifc" entity-type code (the IfcClass the object author already resolved via IfcClass.Resolve) or an unrostered
    // system, so only a real standard system authors a reference; the per-node reference carries Identification (the code) +
    // Location (the identifier URI the Ingest reads back) + Name (the seam Title, the concept label TitleOf reads back) +
    // the SHARED ReferencedSource (the one IfcClassification per (system, edition) the Source memo resolves), so the model
    // carries one dictionary entity per system edition and the FULL six-member seam identity survives the egress leg.
    // The IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect) + IfcClassificationReference(db)
    // {ReferencedSource,Identification,Location,Name} + IfcClassification(db, name){Specification,Edition} ctor surface is
    // decompile-confirmed (.api/api-geometrygym-ifc rows 07/08; every stamped member a settable IfcExternalReference/
    // IfcClassification attribute).
    public static Option<IfcRelAssociatesClassification> Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification) =>
        classification.System != IfcSystem.Key && TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            ? Some(new IfcRelAssociatesClassification(
                new IfcClassificationReference(db) {
                    ReferencedSource = Source(db, row, classification),
                    Identification   = classification.Code,
                    Location         = row.ClassUri(classification.Code),
                    Name             = classification.Title.IfNone(""),
                },
                related))
            : None;
}
```

## [03]-[BSDD_RESOLUTION]

- Owner: `BsddResolution` the live bSDD dictionary resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the service is unreachable so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence carrying the FULL `ClassContract.v1` surface — the class-scoped constraint surface the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve` template, the IDS facets, and the seam `MeasureValue` coercion read, the forward + reverse relation set (`BsddRelation`), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows — the parent a MasterFormat/Uniformat code string does not encode, where the seam `Classification.Parent` code-shape derivation is structurally blind), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`); `BsddFederation` the cross-standard equivalence receipt over the relation graph.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port, Op key)` resolves the full dictionary-class evidence over Compute's transport — `Fin<T>` returns the resolved `BsddClass` on a live hit, and on a service-unreachable miss degrades to the row's local code-shape policy APPLIED (`LocalShape` — a shape-passing code admits as property-free `Active` evidence, a shape-reject faults `BimFault.UnmappedClass` with the same `classification-code-reject` detail `Classify` mints; unreachability itself never faults); a malformed published-class shape routes `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifting BARE off `key`. `BsddResolution.Certify(system, code, port, key)` is the dictionary-certified authoring lowering composing `Resolve` → `BsddClass.Admit` → `Classify`, so a NEW code never lands on an `Inactive`/superseded class (offline the shape-gated degrade admits and the row's local shape gates alone — certification tightens when the dictionary answers, never blocks when it cannot). `BsddResolution.Search(text, scope, relatedIfcEntity, port, key)` resolves a concept label or IFC entity to candidate `BsddHit` codes over `api/Class/Search/v1` — authoring-time, never the ingest path, no offline concept index exists, so the transport failure FAULTS `BimFault.CodecReject` lifted BARE off `key` (never a `LocalShape` degrade, never a raw provider error crossing the entry). `BsddFederation.Of(classes)` folds resolved evidence into the equivalence closure; `Translate(classification, target)` lowers a code across standards onto the seam value; `Equivalent`/`EquivalentSet` answer the pairwise and set queries.
- Auto: `Resolve` builds the class request from `ClassificationSystem.ClassUri(code)` with `IncludeClassProperties`/`IncludeClassRelations`/`IncludeReverseRelations`/`IncludeChildClassReferences` (hierarchy and `parentClassReference` arrive by default), issues it over the injected `BsddPort` (the Compute transport seam, ONE generic `Fetch<TWire>` — the page owns the resource query, the transport owns base URL, headers, and the STJ camelCase decode), and projects the wire `BsddClassResponse` into the `BsddClass` evidence through `BsddClass.Of`: each `ClassProperty` projects through `Property` into a `BsddProperty` carrying the IFC `DataType`/Pset placement (the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template), the `ValueKind`/`AllowedValues`/`Pattern`/`Bounds` value constraints (the `Review/validation#IDS_FACETS` `ValueConstraint`), the seven SI `Exponents` + `Units` (the seam `Properties/quantity#DIMENSION` `Dimension`/`MeasureValue` UnitsNet coercion), and the `Status` (the IDS admission gate); each `ClassRelation` projects through `RelationOf` into a typed `BsddRelation` (an unparseable kind or blank URI drops the row — the closure never sees an unaddressable edge); the containment pointers project into `BsddRef` rows. `BsddFederation.Of` folds the `IsEqualTo`/`IsSynonymOf` rows (forward AND reverse — an inbound equivalence declared by the OTHER dictionary counts) into a transient `AdjacencyGraph<string, SEdge<string>>` both directions per edge, computes `ComputeTransitiveClosure`, and projects the closure into the pure `Equivalence` map — the QuikGraph fold is transient inside `Of`, the receipt carries data only. A transport miss degrades to `LocalShape` so a new standard becomes a dictionary-identity row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_TEMPLATES` owner (`PropertyKey.Resolve(cls, schema, dictionary)` unioning the `BsddClass.Properties` dictionary rows OVER the offline `Xbim.Properties` catalogue floor, dictionary-wins), the IDS Classification + Property facets (the class URI is the facet value; `Pattern`/`Bounds`/`AllowedValues` narrow into an `Xbim.InformationSpecifications` `ValueConstraint`; `Status` gates admission), and the seam `MeasureValue` (a dimensioned property's `Exponents` resolve the `UnitsNet` `BaseDimensions`); the `BsddFederation` receipt is the cross-standard translation evidence a multi-standard deliverable reads (`Translate` an OmniClass-classified model onto its Uniclass peers), and `Ancestry` is the authoritative inheritance chain a facet `partOf`/rollup read walks — so a new dictionary is one identity row across all consumers.
- Packages: Rasm.Element, LanguageExt.Core, QuikGraph, Rasm
- Growth: a new bSDD dictionary is one `ClassificationSystem` row (stem + version + shape); the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; a new dictionary-declared constraint is one read field on the `BsddClassResponse.ClassProperty` wire projected through `Property`, a new relation kind one `BsddRelationKind` member the closure filter reads, a new resource one query builder on `BsddResolution` (never a port member) — never a parallel evidence record, never a per-system classifier, and never a `Rasm.Persistence` reference; federation growth is more resolved evidence folded into `Of`, zero type edits.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreachable-degradation fallback only; the `BsddClass.Of` projection reads ONLY the fields the `.api/api-bsdd` catalog enumerates (the wire is `additionalProperties:false`, so an unexpected member signals contract drift, not a capability) and a field absent from the catalog is a phantom; the class-level constraint (`ClassProperty.AllowedValues`/`Min*`/`Max*`/`Pattern`) is read, never silently the property master, so a class that narrows an enumeration is honored; the cross-standard equivalence closure folds the `IsEqualTo`/`IsSynonymOf` relations through the shared `QuikGraph` `ComputeTransitiveClosure` substrate the folder admits — a hand-rolled BFS/union-find over a `Map<>` adjacency is the named rejected form (`.api/api-quikgraph`); the authoritative containment (`parentClassReference`/`hierarchy`) is read for a code that does NOT encode its parent (MasterFormat/Uniformat), and re-deriving containment from the code string where the dictionary states it is the rejected form; supersession gates authoring — `BsddClass.Admit` refuses a NEW code onto an `Inactive` class carrying the `ReplacedBy` code, and silently authoring a superseded class is the named defect; the live fetch rides the `csharp:Compute/Runtime/transport#TRANSPORT_AXIS` transport injected as `BsddPort` (ONE generic `Fetch<TWire>` the page parameterizes by resource — a per-resource port member is the rejected form) and a transport minted here is the named seam violation; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the resolution degrades to the local policy on a service miss so INGEST never blocks on the service (faulting on unreachability itself is the named defect) while the degraded verdict IS the row's shape gate — a shape-rejected code faults `BimFault.UnmappedClass`, never a fabricated `Active` evidence the dictionary did not answer — and only a malformed published-class shape with no `Code`/`Uri` faults `BimFault.CodecReject` lifting BARE off `key`, while `Search` is authoring-only and its transport failure faults `BimFault.CodecReject` (`bsdd-search-unreachable`) because no offline concept-to-code resolution exists — a raw Compute transport error crossing this AEC-domain entry unwrapped is the named boundary defect.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The bSDD constraint vocabulary the dictionary supplies per property: the value kind selects the seam PropertyValue arm,
// the status gates IDS admission. ClassPropertyContract.v1 constrains both (.api/api-bsdd rows 02/03).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BsddValueKind {
    public static readonly BsddValueKind Single = new("Single");
    public static readonly BsddValueKind Range = new("Range");
    public static readonly BsddValueKind List = new("List");
    public static readonly BsddValueKind Complex = new("Complex");
    public static readonly BsddValueKind ComplexList = new("ComplexList");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BsddStatus {
    public static readonly BsddStatus Active = new("Active");
    public static readonly BsddStatus Preview = new("Preview");
    public static readonly BsddStatus Inactive = new("Inactive");
}

// The six-kind class-relation vocabulary (ClassRelationContract.v1.relationType, catalog order): the IsEqualTo/IsSynonymOf
// pair feeds the BsddFederation equivalence closure; IsParentOf/IsChildOf/HasPart carry taxonomy and composition context.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BsddRelationKind {
    public static readonly BsddRelationKind HasReference = new("HasReference");
    public static readonly BsddRelationKind IsEqualTo = new("IsEqualTo");
    public static readonly BsddRelationKind IsSynonymOf = new("IsSynonymOf");
    public static readonly BsddRelationKind IsParentOf = new("IsParentOf");
    public static readonly BsddRelationKind IsChildOf = new("IsChildOf");
    public static readonly BsddRelationKind HasPart = new("HasPart");
}

// --- [MODELS] -----------------------------------------------------------------------------
// The seven SI base-dimension exponents (L M T I Θ N J) the bSDD property declares (.api/api-bsdd dimension*): the seam
// Properties/quantity#DIMENSION Dimension.Create(...) and the UnitsNet BaseDimensions 7-vector project from these so a
// measured property coerces to its SI base rather than a stringly unit suffix. Dimensioned iff any exponent is non-zero
// (the bitwise OR is non-zero for any non-zero exponent, positive or negative).
public readonly record struct SiExponents(int Length, int Mass, int Time, int Current, int Temperature, int Amount, int LuminousIntensity) {
    public bool IsDimensioned => (Length | Mass | Time | Current | Temperature | Amount | LuminousIntensity) != 0;
}

// The class-level numeric value constraint (may be stricter than the property master) — .api/api-bsdd min*/max*; the IDS
// facet narrows these into an Xbim ValueConstraint range and the seam PropertyValue.Bounded arm reads the same bounds.
public readonly record struct BsddBounds(Option<double> MinInclusive, Option<double> MaxInclusive, Option<double> MinExclusive, Option<double> MaxExclusive);

// A class-narrowed allowed value (the enumeration the IDS facet validates against and the seam Enumerated arm carries) —
// .api/api-bsdd ClassPropertyValueContract.v1.
public sealed record BsddAllowedValue(string Value, string Code, string Description, string Uri);

// A typed class-relation row (.api/api-bsdd ClassRelationContract.v1: relationType + relatedClassUri required,
// relatedClassName/fraction optional — Fraction the HasPart mixture share). Forward and reverse rows share this shape.
public readonly record struct BsddRelation(BsddRelationKind Kind, string RelatedUri, string RelatedName, Option<double> Fraction);

// A containment pointer (.api/api-bsdd ClassReferenceContract.v1 / HierarchyItemContract.v1): the parent, ancestry, and
// child rows — the AUTHORITATIVE containment a MasterFormat/Uniformat code string does not encode.
public readonly record struct BsddRef(string Uri, string Name, string Code);

// One authoring-time search hit: the roster row the hit's dictionary resolves to, the wire's own referenceCode
// (ClassSearchResponseClassContract.v1 — the URI tail only the fallback when a dictionary omits it), and the
// RelatedIfcEntities aligning the hit to the Model/elements#IFC_CLASS IfcClass entity.
public readonly record struct BsddHit(ClassificationSystem System, string Code, string Name, string Uri, Seq<string> RelatedIfcEntities);

// The resolved bSDD property evidence carrying the FULL class-scoped constraint surface (.api/api-bsdd ClassPropertyContract.v1):
// the IFC DataType + Pset placement the Semantics/properties#PROPERTY_TEMPLATES PropertyKey threads into the seam PropertyValue,
// the ValueKind + AllowedValues + Pattern + Bounds the Review/validation#IDS_FACETS facet narrows into a ValueConstraint,
// the seven SI Exponents + Units the seam MeasureValue UnitsNet coercion reads, and the Status the IDS admission gate
// honors. The well-known static PropertyKey anchors construct the six-arg head; the dictionary enriches the rest, so the
// trailing constraint members default empty and the Semantics/properties anchor construction stays valid.
public sealed record BsddProperty(
    string Code, string Name, string DataType, string PropertySet, string PredefinedValue, bool IsRequired,
    BsddValueKind ValueKind = BsddValueKind.Single,
    Seq<BsddAllowedValue> AllowedValues = default,
    Option<string> Pattern = default,
    Option<BsddBounds> Bounds = default,
    Option<SiExponents> Exponents = default,
    Seq<string> Units = default,
    BsddStatus Status = BsddStatus.Active);

// The authoritative classification evidence the classification/properties/validation owners share. RelatedIfcEntities
// aligns the bSDD class to the Model/elements#IFC_CLASS IfcClass entity; Status gates IDS admission; the relation set
// feeds the BsddFederation closure; Parent/Ancestry/Children carry the authoritative containment (Ancestry ordered by
// the wire level, root at the head); Replaces/ReplacedBy/Deprecation carry the supersession the Admit gate reads. The
// trailing members default empty so the LocalShape degrade and the Semantics/properties anchor construction stay valid.
public sealed record BsddClass(
    string Code, string Name, string ClassType, string Definition, string Uri, Seq<BsddProperty> Properties,
    BsddStatus Status = BsddStatus.Active, Seq<string> RelatedIfcEntities = default,
    Seq<BsddRelation> Relations = default, Seq<BsddRelation> ReverseRelations = default,
    Option<BsddRef> Parent = default, Seq<BsddRef> Ancestry = default, Seq<BsddRef> Children = default,
    Seq<string> Replaces = default, Seq<string> ReplacedBy = default, Option<string> Deprecation = default) {
    // The response -> evidence projection: a live hit returning a class with no Code/Uri is INVALID published data, not an
    // offline dictionary miss, so it faults BimFault.CodecReject BARE off the caller's Op (surfaced, never masked as a
    // LocalShape miss). The transport-unreachable degradation is Resolve's concern; this projection only judges payload shape.
    public static Fin<BsddClass> Of(BsddClassResponse response, Op key) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.CodecReject(key, $"bsdd-class-malformed:{response.Uri}"))
            : from status in StatusOf(response.Status, key)
              from properties in (response.ClassProperties ?? []).ToSeq().TraverseM(p => Property(p, key)).As()
              from relations in (response.ClassRelations ?? []).ToSeq().TraverseM(r => RelationOf(r, key)).As()
              from reverse in (response.ReverseClassRelations ?? []).ToSeq().TraverseM(r => RelationOf(r, key)).As()
              select new BsddClass(
                  response.Code, response.Name, response.ClassType ?? "", response.Definition ?? "", response.Uri,
                  properties, status, (response.RelatedIfcEntityNames ?? []).ToSeq(), relations, reverse,
                  Optional(response.ParentClassReference).Map(RefOf),
                  (response.Hierarchy ?? []).ToSeq().OrderBy(static item => item.Level).ToSeq().Map(RefOf),
                  (response.ChildClassReferences ?? []).ToSeq().Map(RefOf),
                  (response.ReplacedObjectCodes ?? []).ToSeq(),
                  (response.ReplacingObjectCodes ?? []).ToSeq(),
                  Optional(response.DeprecationExplanation).Filter(static s => s.Length > 0));

    // The supersession gate Certify composes: an Inactive class never certifies a NEW authoring code — the fault carries
    // the replacing code so the caller re-authors onto the successor. Preview stays admissible here; the IDS facet owns
    // the preview-acceptance policy on the evidence Status.
    public Fin<BsddClass> Admit(Op key) =>
        Status != BsddStatus.Inactive
            ? Fin.Succ(this)
            : Fin.Fail<BsddClass>(new BimFault.UnmappedClass(key, $"classification-superseded:{Code}:{ReplacedBy.Head.IfNone("")}"));

    // Each ClassPropertyContract -> the rich BsddProperty: the code is propertyCode (the wire's class-property code; bSDD
    // carries NO bare `code` on a class-property — `name` is the only required member), falling back to the name when a
    // dictionary omits propertyCode; the typing (DataType/ValueKind/PredefinedValue), the constraint surface
    // (AllowedValues/Pattern/Bounds), the dimension (the seven Exponents + Units), and the Status — every field grounded
    // in .api/api-bsdd, none fabricated.
    static Fin<BsddProperty> Property(BsddClassResponse.ClassProperty p, Op key) =>
        from kind in ValueKindOf(p.PropertyValueKind, key)
        from status in StatusOf(p.PropertyStatus, key)
        select new BsddProperty(
            p.PropertyCode ?? p.Name, p.Name, p.DataType ?? "", p.PropertySet ?? "", p.PredefinedValue ?? "", p.IsRequired,
            kind, (p.AllowedValues ?? []).ToSeq().Map(static v => new BsddAllowedValue(v.Value, v.Code ?? "", v.Description ?? "", v.Uri ?? "")),
            Optional(p.Pattern).Filter(static s => s.Length > 0), BoundsOf(p), ExponentsOf(p), (p.Units ?? []).ToSeq(), status);

    static Option<BsddBounds> BoundsOf(BsddClassResponse.ClassProperty p) =>
        p is { MinInclusive: null, MaxInclusive: null, MinExclusive: null, MaxExclusive: null }
            ? None
            : Some(new BsddBounds(Optional(p.MinInclusive), Optional(p.MaxInclusive), Optional(p.MinExclusive), Optional(p.MaxExclusive)));

    static Option<SiExponents> ExponentsOf(BsddClassResponse.ClassProperty p) {
        var exponents = new SiExponents(
            p.DimensionLength ?? 0, p.DimensionMass ?? 0, p.DimensionTime ?? 0, p.DimensionElectricCurrent ?? 0,
            p.DimensionThermodynamicTemperature ?? 0, p.DimensionAmountOfSubstance ?? 0, p.DimensionLuminousIntensity ?? 0);
        return exponents.IsDimensioned ? Some(exponents) : None;
    }

    // Each ClassRelationContract -> the typed row: an unparseable relationType or a blank relatedClassUri DROPS the row
    // (Somes-filtered) so the federation closure never sees an unaddressable edge — a dictionary minting a new relation
    // kind degrades to context loss on that row, never a fault on the whole class.
    static Fin<BsddRelation> RelationOf(BsddClassResponse.ClassRelation relation, Op key) =>
        relation.RelatedClassUri is not { Length: > 0 }
            ? FinFail<BsddRelation>(new BimFault.CodecReject(key, "bsdd-relation-uri-missing"))
            : BsddRelationKind.TryGet(relation.RelationType, out BsddRelationKind? kind) && kind is { } resolved
                ? FinSucc(new BsddRelation(resolved, relation.RelatedClassUri, relation.RelatedClassName ?? "", Optional(relation.Fraction)))
                : FinFail<BsddRelation>(new BimFault.CodecReject(key, $"bsdd-relation-kind-unmapped:{relation.RelationType}"));

    static BsddRef RefOf(BsddClassResponse.ClassReference reference) => new(reference.Uri, reference.Name ?? "", reference.Code ?? "");
    static BsddRef RefOf(BsddClassResponse.HierarchyItem item) => new(item.Uri ?? "", item.Name ?? "", item.Code ?? "");

    static Fin<BsddValueKind> ValueKindOf(string? kind, Op key) =>
        BsddValueKind.TryGet(kind ?? "", out BsddValueKind? parsed) && parsed is { } value
            ? FinSucc(value)
            : FinFail<BsddValueKind>(new BimFault.CodecReject(key, $"bsdd-value-kind-unmapped:{kind}"));

    static Fin<BsddStatus> StatusOf(string? status, Op key) =>
        BsddStatus.TryGet(status ?? "", out BsddStatus? parsed) && parsed is { } value
            ? FinSucc(value)
            : FinFail<BsddStatus>(new BimFault.CodecReject(key, $"bsdd-status-unmapped:{status}"));
}

// The cross-standard equivalence receipt: the IsEqualTo/IsSynonymOf relation rows (forward AND reverse — an inbound
// equivalence declared by the OTHER dictionary counts) fold BOTH directions per edge into a transient
// AdjacencyGraph<string, SEdge<string>>, close under ComputeTransitiveClosure through the shared QuikGraph substrate
// (.api/api-quikgraph — a hand-rolled BFS/union-find is the named rejected form), and project to pure data: the graph
// never escapes Of, the receipt carries only the Equivalence map and the uri->label Names the Translate title reads.
public sealed record BsddFederation(Map<string, Seq<string>> Equivalence, Map<string, string> Names) {
    // The transient QuikGraph fold is the named statement seam — the mutable graph container is the platform surface.
    public static BsddFederation Of(Seq<BsddClass> classes) {
        var graph = new AdjacencyGraph<string, SEdge<string>>(allowParallelEdges: false);
        classes.Bind(static cls => (cls.Relations + cls.ReverseRelations)
                .Filter(static r => r.Kind == BsddRelationKind.IsEqualTo || r.Kind == BsddRelationKind.IsSynonymOf)
                .Bind(r => Seq(new SEdge<string>(cls.Uri, r.RelatedUri), new SEdge<string>(r.RelatedUri, cls.Uri))))
            .Iter(edge => graph.AddVerticesAndEdge(edge));
        var closure = graph.ComputeTransitiveClosure(static (source, target) => new SEdge<string>(source, target));
        return new(
            toMap(closure.Edges.Where(static edge => edge.Source != edge.Target)
                .GroupBy(static edge => edge.Source)
                .Select(static group => (group.Key, group.Select(static edge => edge.Target).ToSeq()))),
            classes.Fold(default(Map<string, string>), static (acc, cls) =>
                (cls.Relations + cls.ReverseRelations)
                    .Filter(static r => r.RelatedName.Length > 0)
                    .Fold(acc.AddOrUpdate(cls.Uri, cls.Name), static (names, r) => names.AddOrUpdate(r.RelatedUri, r.RelatedName))));
    }

    public Seq<string> EquivalentSet(string classUri) =>
        Equivalence.Find(classUri).IfNone(Seq<string>()).Add(classUri).Distinct().OrderBy(identity).ToSeq();

    public bool Equivalent(string classUriA, string classUriB) =>
        string.Equals(classUriA, classUriB, StringComparison.OrdinalIgnoreCase) || EquivalentSet(classUriA).Contains(classUriB);

    // The cross-standard lowering: an OmniClass-classified value lowers onto its Uniclass peer as a seam value — the
    // source row's ClassUri addresses the closure, the FIRST peer under the target's version-free Stem wins, and the
    // translated value is edition-unspecified with the resolved concept Title when the closure names it. None when the
    // closure holds no peer under the target stem (never a wrong lowering), None for an unrostered source system.
    public Seq<Classification> Translate(Classification classification, ClassificationSystem target) =>
        ClassificationSystem.TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            ? EquivalentSet(row.ClassUri(classification.Code))
                .Filter(uri => uri.StartsWith(target.Stem, StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(identity)
                .Map(uri => Classification.Create(target.Key, ClassificationSystem.TailCode(uri), "", None, None, Names.Find(uri))).ToSeq()
            : Seq<Classification>();
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The bSDD api/Class/v1 wire contract (.api/api-bsdd ClassContract.v1/ClassPropertyContract.v1/ClassRelationContract.v1/
// ClassReferenceContract.v1/HierarchyItemContract.v1): the projection reads ONLY these fields (additionalProperties:false),
// each PascalCase member binding the camelCase wire through the transport's STJ naming policy. The nullable members are
// the optional wire fields — a class fixes a value through PredefinedValue or PropertyValueKind + AllowedValues, narrows
// numerically through Min*/Max*, declares its dimension through the seven Dimension* exponents, states its relations
// through ClassRelations/ReverseClassRelations, its containment through ParentClassReference/ChildClassReferences/
// Hierarchy, and its supersession through ReplacedObjectCodes/ReplacingObjectCodes/DeprecationExplanation.
public sealed record BsddClassResponse(
    string Code, string Name, string Uri, string? ClassType, string? Definition, string? Status,
    string[]? RelatedIfcEntityNames, ClassProperty[]? ClassProperties,
    ClassRelation[]? ClassRelations, ClassRelation[]? ReverseClassRelations,
    ClassReference? ParentClassReference, ClassReference[]? ChildClassReferences, HierarchyItem[]? Hierarchy,
    string[]? ReplacedObjectCodes, string[]? ReplacingObjectCodes, string? DeprecationExplanation) {
    public sealed record ClassProperty(
        string Name, string? PropertyCode, string? DataType, string? PropertySet, string? PredefinedValue,
        bool IsRequired, string? PropertyValueKind, string? PropertyStatus, string? Pattern,
        double? MinInclusive, double? MaxInclusive, double? MinExclusive, double? MaxExclusive,
        int? DimensionLength, int? DimensionMass, int? DimensionTime, int? DimensionElectricCurrent,
        int? DimensionThermodynamicTemperature, int? DimensionAmountOfSubstance, int? DimensionLuminousIntensity,
        string[]? Units, AllowedValue[]? AllowedValues);
    public sealed record AllowedValue(string Value, string? Code, string? Description, string? Uri);
    public sealed record ClassRelation(string RelationType, string RelatedClassUri, string? RelatedClassName, double? Fraction, string? Uri);
    public sealed record ClassReference(string Uri, string? Name, string? Code);
    public sealed record HierarchyItem(int Level, string? Name, string? Code, string? Uri);
}

// The bSDD api/Class/Search/v1 wire contract (.api/api-bsdd ClassSearchResponseContract.v1, paged): the hit's
// referenceCode is the code authority (the identifier-URI tail the omitted-code fallback) and the dictionaryUri
// resolves the roster row.
public sealed record BsddSearchResponse(int TotalCount, int Offset, int Count, SearchClass[]? Classes) {
    public sealed record SearchClass(
        string? DictionaryUri, string? DictionaryName, string Name, string? ReferenceCode, string Uri,
        string? ClassType, string? Description, string? ParentClassName, string[]? RelatedIfcEntityNames);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The Compute transport seam every live bSDD GET rides — ONE generic Fetch the page parameterizes by resource (path +
// query); the transport owns the base URL, the Accept/X-User-Agent headers, and the STJ camelCase decode of TWire. A
// per-resource port member or a transport minted inside Rasm.Bim is the named seam violation
// (csharp:Compute/Runtime/transport#TRANSPORT_AXIS).
public interface BsddPort {
    Fin<TWire> Fetch<TWire>(string resource);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BsddResolution {
    // Two failure axes, kept distinct: a TRANSPORT failure (the dictionary unreachable — Fetch fails) degrades to the row's
    // local code-shape policy — the gate applied, never a fault for unreachability itself — while a live hit carrying
    // malformed data surfaces BsddClass.Of's CodecReject instead of being masked as an offline miss. The Fetch failure is
    // the only genuine transport miss; the response projection owns the payload verdict.
    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port, Op key) =>
        port.Fetch<BsddClassResponse>(ClassResource(system.ClassUri(code))).Match(
            Succ: response => BsddClass.Of(response, key),
            Fail: _ => LocalShape(system, code, key));

    // The dictionary-certified authoring lowering: Resolve -> Admit -> Classify, so a NEW code never lands on an
    // Inactive/superseded class. Offline the shape-gated LocalShape degrade admits (Status defaults Active) so the row's
    // local shape gates alone — certification tightens when the dictionary answers, never blocks when it cannot.
    public static Fin<Classification> Certify(ClassificationSystem system, string code, BsddPort port, Op key) =>
        Resolve(system, code, port, key)
            .Bind(cls => cls.Admit(key))
            .Bind(_ => system.Classify(code, key));

    // Authoring-time concept-to-code resolution (api/Class/Search/v1): a label ("external wall") or an IFC entity scope
    // resolves to candidate BsddHit codes across the scoped dictionaries (the hosted roster rows when scope is empty). No
    // offline concept index exists, so the transport failure FAULTS typed — CodecReject lifted BARE off key with the
    // transport detail preserved (never a LocalShape degrade, never a raw provider error crossing the entry; the MapFail
    // lambda closes over key so it is not static) — and a hit whose dictionary is unrostered drops (the roster IS the
    // admission; an unrostered code cannot lower onto the seam).
    public static Fin<Seq<BsddHit>> Search(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPort port, Op key) =>
        port.Fetch<BsddSearchResponse>(SearchResource(text, scope, relatedIfcEntity))
            .MapFail(error => new BimFault.CodecReject(key, $"bsdd-search-unreachable:{error.Message}"))
            .Map(response => (response.Classes ?? []).ToSeq().Map(HitOf).Somes());

    // The two resource builders the page owns (the transport owns base URL/headers/decode): the class request opts into
    // the relation/children rollups (hierarchy and parentClassReference arrive by default); the search request repeats
    // the DictionaryUris/RelatedIfcEntities keys per the bSDD array-parameter law, an empty scope pinning the HOSTED
    // roster rows server-side — the roster IS the admission, so the page limit is never spent on unrostered
    // dictionaries HitOf would drop anyway.
    static string ClassResource(string classUri) =>
        $"api/Class/v1?Uri={System.Uri.EscapeDataString(classUri)}&IncludeClassProperties=true&IncludeClassRelations=true&IncludeReverseRelations=true&IncludeChildClassReferences=true";

    static string SearchResource(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity) =>
        string.Concat(
            $"api/Class/Search/v1?SearchText={System.Uri.EscapeDataString(text.Trim())}",
            (scope.IsEmpty ? ClassificationSystem.Items.ToSeq().Filter(static row => row.Version.Length > 0) : scope)
                .Fold("", static (acc, row) => $"{acc}&DictionaryUris={System.Uri.EscapeDataString(row.DictionaryUri)}"),
            relatedIfcEntity.Match(Some: static entity => $"&RelatedIfcEntities={System.Uri.EscapeDataString(entity)}", None: static () => ""));

    // The hit's code authority is the wire's own referenceCode (ClassSearchResponseClassContract.v1 carries it —
    // .api/api-bsdd); the identifier-URI tail is the FALLBACK for a dictionary that omits it, never the primary read.
    static Option<BsddHit> HitOf(BsddSearchResponse.SearchClass hit) =>
        hit.Uri is { Length: > 0 }
            ? ClassificationSystem.ByUri(hit.DictionaryUri ?? hit.Uri)
                .Map(system => new BsddHit(
                    system,
                    hit.ReferenceCode is { Length: > 0 } code ? code : ClassificationSystem.TailCode(hit.Uri),
                    hit.Name, hit.Uri, (hit.RelatedIfcEntityNames ?? []).ToSeq()))
            : None;

    // The offline degrade IS the row's code-shape policy applied: a shape-passing code admits as property-free Active
    // evidence, a shape-reject faults the SAME UnmappedClass detail Classify mints (one local policy, one fault) —
    // never a fabricated Active evidence for a garbage code the dictionary could not have answered for.
    static Fin<BsddClass> LocalShape(ClassificationSystem system, string code, Op key) =>
        system.CodeShape.IsMatch(code.Trim())
            ? Fin.Succ(new BsddClass(code, $"{system.Title}:{code}", "Class", "", system.ClassUri(code), Seq<BsddProperty>()))
            : Fin.Fail<BsddClass>(new BimFault.UnmappedClass(key, $"classification-code-reject:{system.Key}:{code}"));
}
```

## [04]-[RESEARCH]

- [CLASSIFICATION_LOWERING]: the standard-systems roster lowering onto the seam `Classification` grounds against `ELEMENT-REBUILD-PLAN.md` §4B (the `Object` node carries a generic `Classification(system, code)` value-object, NOT `IfcClass`) and the seam `Classification/classification#CLASSIFICATION_AXIS` boundary ("a downstream `Rasm.Bim` projector owns the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto a seam `Classification` value at ingest") — so `ClassificationSystem.Classify` validates the code shape and mints the seam `Classification.Create(system, code, "", None, None, None)` (the SIX-member `(System, Code, Edition, Source, EditionDate, Title)` factory; the author/`Classify` path carries no resolved edition/publisher/title so the trailing four are the edition-unspecified `""` then a `None` `Source`/`EditionDate`/`Title` bundle, `Ingest` lowering the root `IfcClassification.Edition`/`Source`/`EditionDate` plus the `IfcClassificationReference.Name` concept title for the imported leg), the seam owning the `Parent`/`Within` containment projections and this page owning only the roster. Classification is a VALUE on the `Object` node and NEVER an edge: the seam `Relations/relation#EDGE_ALGEBRA` is explicit ("classification is a generic value ON the `Object` node, NOT an edge … the seam carries no classification-association relationship", the `Associate` edge carrying a `Material`/`Appearance` resource), so the egress reads `node.Classification` and authors `IfcRelAssociatesClassification` from the value, never from an edge. The fault shape grounds against `Model/faults#FAULT_BAND` (`BimFault` `Expected`-derived, band 2600 IS the `Code`, the typed case lifting BARE off the threaded `Op key` with no `.ToError()` hop — the exact `Classify` `classification-code-reject` and `BsddClass.Of` `bsdd-class-malformed` routings faults.md enumerates) and the sibling `Model/elements#IFC_CLASS` `IfcClass.Resolve(string, Op key)` idiom; the type rename `Classification`→`ClassificationSystem` resolves the migration-source name collision with the seam value-object, and the `BimElement` binding (`ClassificationRef`/`ClassificationCode`) is retired with the `BimElement`/`BimModel` element records per §2/§4B.
- [BSDD_SERVICE_CONTRACT]: the bSDD RESTful dictionary class/property lookup response shape is GROUNDED against the published buildingSMART bSDD `Dictionaries API` contract (`.api/api-bsdd`) — `api/Class/v1` with `IncludeClassProperties=true` returns the `ClassContract.v1` (`code`/`name`/`uri`/`status`/`classType`/`definition`/`relatedIfcEntityNames[]` plus the `classProperties[]`), each `ClassPropertyContract.v1` carrying `name`/`propertyCode`/`propertySet`/`dataType`/`propertyValueKind`/`predefinedValue`/`isRequired`/`propertyStatus` PLUS the FULL class-scoped constraint surface (`allowedValues[]`, `pattern`, `minInclusive`/`maxInclusive`/`minExclusive`/`maxExclusive`, the seven `dimensionLength`/`dimensionMass`/… exponents, `units[]`) — so `BsddClass.Of` projects the rich `BsddProperty` matching the real wire and feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve(cls, schema, dictionary)` template (the dictionary rows unioned OVER the offline `Xbim.Properties` catalogue floor), the `Review/validation#IDS_FACETS` Classification + Property facets (the bSDD `pattern`/`min*`/`max*`/`allowedValues` narrow directly into the `Xbim.InformationSpecifications` `ValueConstraint`, `status` gates admission), and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion (the seven `Exponents` resolve the `UnitsNet` `BaseDimensions` 7-vector, `units[]` the source unit) directly — the naive six-field slice the migration source read is the deleted form. The relation/containment/supersession surface is likewise catalog-grounded: `classRelations[]`/`reverseClassRelations[]` are `ClassRelationContract.v1` rows (`relationType` ∈ `HasReference`/`IsEqualTo`/`IsSynonymOf`/`IsParentOf`/`IsChildOf`/`HasPart`, `relatedClassUri` required, `relatedClassName`/`fraction` optional — `IncludeClassRelations`/`IncludeReverseRelations` opt them in), `parentClassReference`/`childClassReferences[]` are `ClassReferenceContract.v1` pointers (`uri` required + `name`/`code`), `hierarchy[]` is the `HierarchyItemContract.v1` ancestor-to-root chain (`level`/`name`/`code`/`uri` — `Ancestry` orders by `level`, root at the head; `hierarchy` and `parentClassReference` arrive by default, no `Include*` toggle), and `replacedObjectCodes[]`/`replacingObjectCodes[]`/`deprecationExplanation` carry the supersession `BsddClass.Admit` gates on; `api/Class/Search/v1` (`SearchText` required, `DictionaryUris[]`/`RelatedIfcEntities[]` repeating array keys) returns the paged `ClassSearchResponseContract.v1` whose `classes[]` rows carry `dictionaryUri`/`dictionaryName`/`name`/`referenceCode`/`uri`/`classType`/`parentClassName`/`relatedIfcEntityNames[]` — `referenceCode` is the hit's code authority and the identifier-URI tail (`TailCode`) the omitted-code fallback, the version-free `Stem` prefix its roster resolution. The equivalence closure folds through the decompile-verified `QuikGraph.Algorithms` `ComputeTransitiveClosure(IEdgeListGraph<TVertex,TEdge>, Func<TVertex,TVertex,TEdge>)` → `BidirectionalGraph` extension over an `AdjacencyGraph<string, SEdge<string>>` (`.api/api-quikgraph`). The LIVE-WIRE leg stays gated on the `csharp:Compute/Runtime/transport#TRANSPORT_AXIS` transport alignment (the `BsddPort` binding rides the injected Compute transport, never a transport minted here), and the in-process degradation to the row's local code-shape policy (`LocalShape`) is the verified settled fallback so ingest never blocks.
- [CLASSIFICATION_ROUNDTRIP]: the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip member spellings confirm against the GeometryGym entity surface (`.api/api-geometrygym-ifc` rows 07/08; `assay api` decompile) so the round-trip is BIDIRECTIONAL: `ClassificationSystem.Author` constructs `new IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect)` over an `IfcClassificationReference(db)` whose `ReferencedSource` is the per-`(db, system, edition)` memoized `new IfcClassification(db, Title){Specification, Edition}` re-stamping the value's `Source`/`EditionDate`, `Identification` the code, `Location` the identifier URI, and `Name` the seam `Title` (`IfcExternalReference.Location`/`Identification`/`Name` and `IfcClassification.Specification`/`Edition`/`Source`/`EditionDate` decompile-confirmed settable; `LocalDate.ToDateTimeUnspecified` the NodaTime outbound) — the egress the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node, subsuming the retired `Rasm.Materials` `MaterialPropertyWire.Classification` material-wire half (`ELEMENT-REBUILD-PLAN.md` §6); `ClassificationSystem.Ingest` is the inverse the migration source never had — it walks the `IfcClassificationReference.ReferencedSource` `IfcClassificationReferenceSelect` chain past nested references to the root `IfcClassification` ONCE, resolves the standard system off that root's `Name`, the root's `Specification` dictionary-URI prefix (`IfcClassification.Specification` decompile-confirmed get/set, the IFC4X3 rename of the `[Obsolete]` `Location` — the SAME URI `Author` stamps, so the round-trip self-resolves when a re-export strips per-reference `Location`s and a third-party export carrying only the dictionary-level URI resolves too), and the reference `Location` prefix against the roster, reads the code off `Identification`/`Location`, AND lowers the EDITION-SCOPED annotation bundle off the SAME root — the `Edition` token (seam IDENTITY), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed, the `DateTime.MinValue` sentinel folding to a `None` `EditionDate` via NodaTime `LocalDate.FromDateTime`) — so the imported leg lands the FULL six-member seam `Classification` value on the `Object` node rather than an edition-blank, title-`None` reference; the `IfcClassificationReference.ReferencedSource`/`Location`/`Identification`/`Name` + `IfcClassification.Name`/`Edition`/`Source`/`EditionDate` spellings decompile-confirmed (rows 07/08); an unrostered source resolves to `None` so a foreign classification rides the `Projection/relations#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering.
