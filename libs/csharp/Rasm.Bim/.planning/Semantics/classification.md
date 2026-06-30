# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the seam `Classification` value an `Object` node carries: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/IfcClassification), each row carrying its bSDD dictionary URI and its code-shape policy, validating a raw code against the system's shape and LOWERING it onto the library-neutral seam `Classification(system, code)` value. The seam owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (`System` + `Code`, no roster, no dictionary, no regex); this page is the downstream Bim projector the seam names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto the seam value at ingest and re-authoring that value at egress. A classification is the seam `Classification` VALUE on an `Object` node: `Relations/relation#EDGE_ALGEBRA` is explicit that classification is a value on the node, NOT an edge (the neutral `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the retired `(GlobalId, system, code)` triple bound to a second stored `BimElement` record is GONE — a query reads `node.Classification.System`/`Code`, never a stringly-keyed lookup against a second element store.

The round-trip is BIDIRECTIONAL across three entries: `Classify` lowers a validated raw code at authoring ingress, `Ingest` resolves an imported `IfcClassificationReference` back to the seam value at import ingress (the inverse of `Author`, the leg the migration source never had), and `Author` re-authors a node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress — the element-classification egress the `Projection/semantic#IFC_EGRESS` `Emit` composes per `Object` node, which REPLACES the retired `Rasm.Materials` `MaterialPropertyWire.Classification` half (a material carries no classification; the `Object` node does): the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands a substance's standard `(system, code)` as the bound element's `Object`-node `Classification` value through its `MaterialBinding` egress, so the element classification this owner round-trips is the one the unified Component projection authored, never a material-wire field. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping carries the FULL class-scoped constraint surface (IFC `DataType` + Pset placement, `ValueKind`, allowed values, XSD `Pattern`, numeric `Bounds`, the seven SI exponents + `Units`, `Status`) that feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template, the `Review/validation#IDS_FACETS` Classification + Property facets, and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion directly, so a new standard is one `ClassificationSystem` dictionary-URI row shared across `classification`, `properties`, and `validation`. The typed `Model/faults#FAULT_BAND` `BimFault` cases lift BARE onto the `Fin` rail (band 2600 IS the `Expected` `Code`; no `.ToError()` hop), each carrying the kernel `Op` operation context the caller threads.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary URI, code-shape policy); `Classify(code, key)` lowering a validated raw code onto a seam `Classification` value at authoring ingress; `Ingest(reference)` resolving an imported `IfcClassificationReference` back onto a seam value at import ingress; and `Author(db, related, classification)` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` (the element-classification egress the `Projection/semantic#IFC_EGRESS` `Emit` composes).
- [02]-[BSDD_RESOLUTION]: the live bSDD dictionary class/property resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, projecting the FULL `BsddClassResponse` constraint surface (`ValueKind`/`AllowedValues`/`Pattern`/`Bounds`/`Exponents`/`Units`/`Status`/`RelatedIfcEntities`), degrading to the row's local code-shape policy, feeding the `Semantics/properties#PROPERTY_TEMPLATES` template, the IDS facets, and the seam `MeasureValue` coercion.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row carrying the system title, the bSDD dictionary URI, and the code-shape regex policy; the projector that lowers a resolved standard code onto the seam `Classification/classification#CLASSIFICATION_AXIS` `Classification` value the `Object` node carries and authors it back at egress. The seam `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the seam defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the seam value, never re-declaring a classification value-object beside the seam.
- Entry: `ClassificationSystem.Classify(string code, Op key)` validates the raw code against the system's code-shape regex and lowers it onto a seam `Classification(Key, code)` value — `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`, the typed case lifting BARE off `key`, no `.ToError()` hop); `ClassificationSystem.Ingest(IfcClassificationReference reference)` is the import-ingress inverse resolving the standard system off the reference's `ReferencedSource` dictionary title OR its `Location` identifier-URI prefix against the roster (the code off `Identification` or the trailing `Location` segment, the resolved `Title` off the reference's own `Name`), returning `Option<Classification>` the `Projection/semantic#SEMANTIC_PROJECTOR` ingress accumulates onto the `Graph/element#NODE_MODEL` `Object` node's `Classifications` set (IFC admits MULTIPLE `IfcRelAssociatesClassification` per object) — `None` for an unrostered source so a foreign system rides the `Projection/semantic#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification)` is the egress entry the `Emit` composes per `Object` node — `None` for the `"ifc"` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system, `Identification` the code, and `Location` the identifier URI the `Ingest` reads back.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the actual shape enforcement the migration source's decorative `ClassificationCode` never applied) and, on a match, mints the seam `Classification.Create(Key, code, "", "", None, None)` (the author path carries no resolved edition/publisher/title; the seam `[ComplexValueObject]` SIX-member factory `(System, Code, Edition, Source, EditionDate, Title)` normalizing the system token and trimming the code, the edition-unspecified `Edition ""` and the `Source`/`EditionDate`/`Title` annotation bundle empty), so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30", "")` whose seam `Parent`/`Within` projections derive the containment hierarchy; a code the shape rejects faults rather than lowering a malformed value. `Ingest` walks `RootSource` to the root `IfcClassification` ONCE: it matches a roster row by the dictionary `Name` equality or the `Location` dictionary-URI prefix, reads the code off `CodeOf` (`URI`-unescaping the `Location` trailing segment when the `Identification` is absent), and lowers the EDITION-SCOPED annotation bundle off that SAME root — the `Edition` token (IDENTITY on the seam), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed) — while `TitleOf` reads the reference's OWN `IfcClassificationReference.Name` (distinct from the root dictionary `Name`) as the resolved concept `Title`, so the imported leg lands the FULL six-member identity (edition + publisher + date + title) at the only path that can populate it rather than a perpetually edition-blank, title-`None` reference. `Author` resolves the row through the generated `TryGet` and authors the reference with the URI-escaped `Location` (`ClassUri`) so an OmniClass/MasterFormat code carrying spaces round-trips; the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row carrying its bSDD dictionary URI and code-shape regex; the seam `Classification` value-object absorbs any `(system, code)` pair with no seam edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type, never a parallel classification value-object beside the seam, and never a per-direction resolver (one `Classify`/`Ingest`/`Author` triad spans the whole round-trip).
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
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The standard classification-systems roster lowering onto the seam Classification value-object. The type name
// ClassificationSystem is distinct from the seam Classification so the seam value-object is the one canonical
// classification; this page owns only the roster (dictionary URI + code shape) and the bSDD lane. Classification is a
// VALUE on the Object node (Relations/relation#EDGE_ALGEBRA — never an edge), so this projector lowers a resolved code
// onto that value at ingest (Classify for a raw code, Ingest for an imported IfcClassificationReference) and re-authors
// the node's standard value back onto IfcRelAssociatesClassification at egress (Author).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClassificationSystem {
    public static readonly ClassificationSystem Uniclass2015 = new("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", Shape(@"^[A-Z][A-Za-z]_\d{2}(_\d{2}){0,3}$"));
    public static readonly ClassificationSystem OmniClass     = new("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", Shape(@"^\d{2}-\d{2}( \d{2}){2,3}$"));
    public static readonly ClassificationSystem MasterFormat  = new("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", Shape(@"^\d{2} \d{2} \d{2}(\.\d{2})?$"));
    public static readonly ClassificationSystem Uniformat     = new("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", Shape(@"^[A-Z]\d{4}$"));
    public static readonly ClassificationSystem IfcSystem     = new("ifc", "IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", Shape(@"^Ifc[A-Za-z]+$"));

    public string Title { get; }
    public string DictionaryUri { get; }
    public Regex CodeShape { get; }

    // The code-shape matcher is a compiled NonBacktracking row policy: a Uniclass table prefix is two letters with the
    // second upper OR lower (Ss/Pr/EF/SL/TE), so [A-Z][A-Za-z] admits every real prefix; a new standard is one Shape(...)
    // row, never a per-call-site Regex parse and never a hostile-input backtracking surface.
    static Regex Shape(string pattern) => new(pattern, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);

    // INGRESS (raw code): validate against the system's code shape and lower onto a seam Classification value the Object
    // node carries — the actual shape enforcement, then the seam factory. The author path is edition-UNSPECIFIED (Edition
    // "" — a raw authoring code carries no resolved publisher/edition; the bSDD/IfcClassification annotations are an Ingest
    // concern), so the seam six-member [ComplexValueObject] Create takes ("", "", None, None) for the
    // Source/Edition/EditionDate/Title annotation bundle. The typed UnmappedClass case lifts BARE off the caller's Op
    // (band 2600 IS the Expected Code; no .ToError() hop) on a shape reject.
    public Fin<Classification> Classify(string code, Op key) =>
        CodeShape.IsMatch(code.Trim())
            ? Fin.Succ(Classification.Create(Key, code, "", "", None, None))
            : Fin.Fail<Classification>(new BimFault.UnmappedClass(key, $"classification-code-reject:{Key}:{code}"));

    // The dictionary class URI the bSDD resolution fetches AND the IfcClassificationReference.Location the egress writes:
    // the dictionary URI plus the URI-escaped code, so an OmniClass/MasterFormat code carrying spaces (e.g. "23-13 35 00")
    // produces a valid request/round-trip URI Ingest can unescape back to the code.
    public string ClassUri(string code) => $"{DictionaryUri}/class/{System.Uri.EscapeDataString(code.Trim())}";

    // INGRESS (imported IFC): an IfcClassificationReference -> the seam standard Classification the projector lands on the
    // Object node value (the inverse of Author, the leg the migration source never had). The root IfcClassification
    // dictionary is walked ONCE: the system resolves off its Name OR the reference's Location identifier-URI prefix against
    // the roster, and the EDITION-SCOPED annotation bundle the seam Classification carries (the publisher Source, the
    // string Edition, the EditionDate revision date — IfcClassification.Source/Edition/EditionDate decompile-confirmed,
    // .api/api-geometrygym-ifc row 08) lowers off that SAME root so the imported leg lands the full six-member identity,
    // never a perpetually-edition-blank reference. The dictionary Edition is IDENTITY on the seam (a re-editioned re-ingest
    // must not collide §EDITION_SCOPING), so reading it off the root is the only ingest path that can populate it. None for
    // an unrostered source so a foreign system rides the Generic passthrough rather than a wrong lowering, and a blank code
    // yields None rather than throwing through the seam Create.
    public static Option<Classification> Ingest(IfcClassificationReference reference) {
        IfcClassification? dictionary = RootSource(reference);
        string location = reference.Location ?? "";
        return Optional(Items.FirstOrDefault(row =>
            string.Equals(row.Title, dictionary?.Name ?? "", StringComparison.OrdinalIgnoreCase)
            || (location.Length > 0 && location.StartsWith(row.DictionaryUri, StringComparison.OrdinalIgnoreCase))))
            .Bind(system =>
                CodeOf(reference) is { Length: > 0 } code
                    ? Some(Classification.Create(
                        system.Key, code,
                        dictionary?.Edition?.Trim() ?? "",                          // the edition token (IDENTITY) off the root dictionary, "" when unspecified
                        dictionary?.Source ?? "",                                   // the publisher annotation (equality-excluded)
                        EditionDateOf(dictionary),                                  // the revision date annotation (equality-excluded)
                        TitleOf(reference)))                                        // the resolved concept title off the reference's own Name
                    : None);
    }

    // Walk ReferencedSource (an IfcClassificationReferenceSelect = IfcClassification | IfcClassificationReference) up the
    // hierarchy to the root dictionary; the depth bound makes a malformed cyclic chain terminate rather than spin. A nested
    // IfcClassificationReference (the IFC hierarchical-classification pattern) points its ReferencedSource at a PARENT
    // reference, not the dictionary, so a flat `as IfcClassification` would read "" on a nested ref and silently miss the
    // dictionary — the walk is what lets Ingest read the dictionary Name AND its Source/Edition/EditionDate annotations.
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

    static string CodeOf(IfcClassificationReference reference) =>
        reference.Identification is { Length: > 0 } id ? id
        : reference.Location is { Length: > 0 } loc ? System.Uri.UnescapeDataString(loc[(loc.LastIndexOf('/') + 1)..])
        : "";

    // The human title the seam Classification.Title carries at ingest: IfcClassificationReference.Name (the
    // IfcExternalReference.Name label, decompile-confirmed `public string Name` on the GeometryGym base —
    // .api/api-geometrygym-ifc row 07), the classified concept's resolved name (e.g. "Cavity external wall systems");
    // None for a blank Name so an untitled reference lowers a title-free Classification rather than an empty-string
    // annotation that would read as a present-but-blank title downstream. Without this the seam Title is dead at the
    // ONLY ingest path that can populate it — the resolved-name read would be perpetually None at IFC import.
    static Option<string> TitleOf(IfcClassificationReference reference) =>
        reference.Name is { Length: > 0 } name ? Some(name.Trim()) : None;

    // The db-scoped dictionary-source memo: ONE IfcClassification per (db, system) is shared across every Author call, so an
    // egress that classifies N objects under Uniclass authors ONE IfcClassification dictionary entity, not N duplicates (a
    // fresh source per node was the bloat). Keyed by the emit DatabaseIfc so the cache is emit-scoped and GC-collected with
    // the database (a per-Author db scan would be O(model) each call); the ConcurrentDictionary makes resolve-or-create atomic.
    static readonly ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<string, IfcClassification>> Sources = new();

    static IfcClassification Source(DatabaseIfc db, ClassificationSystem row) =>
        Sources.GetValue(db, static _ => new ConcurrentDictionary<string, IfcClassification>(StringComparer.Ordinal))
            .GetOrAdd(row.Key, _ => new IfcClassification(db, row.Title) { Specification = row.DictionaryUri });

    // EGRESS: the Object node's standard Classification value -> the IfcRelAssociatesClassification/IfcClassificationReference
    // the Projection/semantic#IFC_EGRESS Emit composes per Object node — the element-classification egress that REPLACES the
    // retired Materials material-wire classification half (a material carries no classification, the Object node does). None
    // for the "ifc" entity-type code (the IfcClass the object author already resolved via IfcClass.Resolve) or an unrostered
    // system, so only a real standard system authors a reference; the per-node reference carries Identification (the code) +
    // Location (the identifier URI the Ingest reads back) + the SHARED ReferencedSource (the one IfcClassification per system
    // the Source memo resolves, its Specification the dictionary URI), so the model carries one dictionary entity per system.
    // The IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect) + IfcClassificationReference(db)
    // {ReferencedSource,Identification,Location} + IfcClassification(db, name){Specification} ctor surface is decompile-confirmed
    // (.api/api-geometrygym-ifc rows 07/08; Specification settable, Location the IfcExternalReference attribute).
    public static Option<IfcRelAssociatesClassification> Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification) =>
        classification.System != IfcSystem.Key && TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            ? Some(new IfcRelAssociatesClassification(
                new IfcClassificationReference(db) {
                    ReferencedSource = Source(db, row),
                    Identification   = classification.Code,
                    Location         = row.ClassUri(classification.Code),
                },
                related))
            : None;
}
```

## [03]-[BSDD_RESOLUTION]

- Owner: `BsddResolution` the live bSDD dictionary class/property resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the service is unreachable so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence carrying the FULL class-scoped constraint surface the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve` template, the IDS facets, and the seam `MeasureValue` coercion read.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port, Op key)` resolves the dictionary class shape and the bSDD class-to-property mapping over Compute's transport — `Fin<T>` returns the resolved `BsddClass` evidence on a live hit, and on a service-unreachable miss degrades to the row's local code-shape policy (`LocalShape`, never a fault on degradation); a malformed published-class shape the dictionary returns routes `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifting BARE off `key`.
- Auto: `Resolve` builds the dictionary-class request URI from `ClassificationSystem.ClassUri(code)`, issues it over the injected `BsddPort` (the Compute transport seam), and projects the wire `BsddClassResponse` (the C# projection of the bSDD `ClassContract.v1`: `Code`/`Name`/`ClassType`/`Uri`/`Definition`/`Status`/`RelatedIfcEntityNames` + `ClassProperties`) into the `BsddClass` evidence through `BsddClass.Of`, each `ClassProperty` projecting through `Property` into a `BsddProperty` carrying the IFC `DataType`/Pset placement (the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template), the `ValueKind`/`AllowedValues`/`Pattern`/`Bounds` value constraints (the `Review/validation#IDS_FACETS` `ValueConstraint`), the seven SI `Exponents` + `Units` (the seam `Properties/quantity#DIMENSION` `Dimension`/`MeasureValue` UnitsNet coercion), and the `Status` (the IDS admission gate); a transport miss degrades to `LocalShape` so a new standard becomes a dictionary-URI row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_TEMPLATES` owner (`PropertyKey.Resolve(cls, schema, dictionary)` unioning the `BsddClass.Properties` dictionary rows OVER the offline `Xbim.Properties` catalogue floor, dictionary-wins), the IDS Classification + Property facets (the class URI is the facet value; `Pattern`/`Bounds`/`AllowedValues` narrow into an `Xbim.InformationSpecifications` `ValueConstraint`; `Status` gates admission), and the seam `MeasureValue` (a dimensioned property's `Exponents` resolve the `UnitsNet` `BaseDimensions`), so a new dictionary is one URI row across all three.
- Packages: Rasm.Element, LanguageExt.Core, Rasm
- Growth: a new bSDD dictionary is one `ClassificationSystem` row carrying its `DictionaryUri`; the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; a new dictionary-declared constraint is one read field on the `BsddClassResponse.ClassProperty` wire projected through `Property`, never a parallel evidence record; never a per-system classifier and never a `Rasm.Persistence` reference.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreachable-degradation fallback only; the `BsddClass.Of` projection reads ONLY the fields the `.api/api-bsdd` catalog enumerates (the wire is `additionalProperties:false`, so an unexpected member signals contract drift, not a capability) and a field absent from the catalog is a phantom; the class-level constraint (`ClassProperty.AllowedValues`/`Min*`/`Max*`/`Pattern`) is read, never silently the property master, so a class that narrows an enumeration is honored; the live fetch rides the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport injected as `BsddPort` and a transport minted here is the named seam violation; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the resolution degrades to the local policy on a service miss so ingest never blocks, and a fault on degradation is the named defect — only a malformed published-class shape (no `Code`/`Uri`) faults `BimFault.CodecReject`, lifting BARE off `key`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The bSDD constraint vocabulary the dictionary supplies per property: the value kind selects the seam PropertyValue arm,
// the status gates IDS admission. ClassPropertyContract.v1 constrains both (.api/api-bsdd rows 02/03).
public enum BsddValueKind : byte { Single = 0, Range = 1, List = 2, Complex = 3, ComplexList = 4 }
public enum BsddStatus : byte { Active = 0, Preview = 1, Inactive = 2 }

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
// aligns the bSDD class to the Model/elements#IFC_CLASS IfcClass entity; Status gates IDS admission.
public sealed record BsddClass(
    string Code, string Name, string ClassType, string Definition, string Uri, Seq<BsddProperty> Properties,
    BsddStatus Status = BsddStatus.Active, Seq<string> RelatedIfcEntities = default) {
    // The response -> evidence projection: a live hit returning a class with no Code/Uri is INVALID published data, not an
    // offline dictionary miss, so it faults BimFault.CodecReject BARE off the caller's Op (surfaced, never masked as a
    // LocalShape miss). The transport-unreachable degradation is Resolve's concern; this projection only judges payload shape.
    public static Fin<BsddClass> Of(BsddClassResponse response, Op key) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.CodecReject(key, $"bsdd-class-malformed:{response.Uri}"))
            : Fin.Succ(new BsddClass(
                response.Code, response.Name, response.ClassType ?? "", response.Definition ?? "", response.Uri,
                (response.ClassProperties ?? []).ToSeq().Map(Property),
                StatusOf(response.Status),
                (response.RelatedIfcEntityNames ?? []).ToSeq()));

    // Each ClassPropertyContract -> the rich BsddProperty: the code is propertyCode (the wire's class-property code; bSDD
    // carries NO bare `code` on a class-property — `name` is the only required member), falling back to the name when a
    // dictionary omits propertyCode; the typing (DataType/ValueKind/PredefinedValue), the constraint surface
    // (AllowedValues/Pattern/Bounds), the dimension (the seven Exponents + Units), and the Status — every field grounded
    // in .api/api-bsdd, none fabricated.
    static BsddProperty Property(BsddClassResponse.ClassProperty p) => new(
        p.PropertyCode ?? p.Name, p.Name, p.DataType ?? "", p.PropertySet ?? "", p.PredefinedValue ?? "", p.IsRequired,
        ValueKindOf(p.PropertyValueKind),
        (p.AllowedValues ?? []).ToSeq().Map(static v => new BsddAllowedValue(v.Value, v.Code ?? "", v.Description ?? "", v.Uri ?? "")),
        Optional(p.Pattern).Filter(static s => s.Length > 0),
        BoundsOf(p),
        ExponentsOf(p),
        (p.Units ?? []).ToSeq(),
        StatusOf(p.PropertyStatus));

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

    static BsddValueKind ValueKindOf(string? kind) => Enum.TryParse(kind, ignoreCase: true, out BsddValueKind parsed) ? parsed : BsddValueKind.Single;
    static BsddStatus StatusOf(string? status) => Enum.TryParse(status, ignoreCase: true, out BsddStatus parsed) ? parsed : BsddStatus.Active;
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The bSDD api/Class/v1 wire contract (.api/api-bsdd ClassContract.v1/ClassPropertyContract.v1): the projection reads ONLY
// these fields (additionalProperties:false), each PascalCase member binding the camelCase wire through the transport's STJ
// naming policy. The nullable members are the optional wire fields — a class fixes a value through PredefinedValue or
// PropertyValueKind + AllowedValues, narrows numerically through Min*/Max*, and declares its dimension through the seven
// Dimension* exponents.
public sealed record BsddClassResponse(
    string Code, string Name, string Uri, string? ClassType, string? Definition, string? Status,
    string[]? RelatedIfcEntityNames, ClassProperty[]? ClassProperties) {
    public sealed record ClassProperty(
        string Name, string? PropertyCode, string? DataType, string? PropertySet, string? PredefinedValue,
        bool IsRequired, string? PropertyValueKind, string? PropertyStatus, string? Pattern,
        double? MinInclusive, double? MaxInclusive, double? MinExclusive, double? MaxExclusive,
        int? DimensionLength, int? DimensionMass, int? DimensionTime, int? DimensionElectricCurrent,
        int? DimensionThermodynamicTemperature, int? DimensionAmountOfSubstance, int? DimensionLuminousIntensity,
        string[]? Units, AllowedValue[]? AllowedValues);
    public sealed record AllowedValue(string Value, string? Code, string? Description, string? Uri);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The Compute transport seam the live bSDD GET rides — a transport minted inside Rasm.Bim is the named seam violation
// (csharp:Compute/Runtime/channels#TRANSPORT_AXIS), so the live leg is injected as BsddPort.
public interface BsddPort {
    Fin<BsddClassResponse> Fetch(string dictionaryClassUri);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BsddResolution {
    // Two failure axes, kept distinct: a TRANSPORT failure (the dictionary unreachable — Fetch fails) degrades to the row's
    // local code-shape policy so ingest never blocks, while a live hit carrying malformed data surfaces BsddClass.Of's
    // CodecReject instead of being masked as an offline miss. The Fetch failure is the only genuine transport miss; the
    // response projection owns the payload verdict.
    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port, Op key) =>
        port.Fetch(system.ClassUri(code)).Match(
            Succ: response => BsddClass.Of(response, key),
            Fail: _ => Fin.Succ(LocalShape(system, code)));

    static BsddClass LocalShape(ClassificationSystem system, string code) =>
        new(code, $"{system.Title}:{code}", "Class", "", system.ClassUri(code), Seq<BsddProperty>());
}
```

## [04]-[RESEARCH]

- [CLASSIFICATION_LOWERING]: the standard-systems roster lowering onto the seam `Classification` grounds against `ELEMENT-REBUILD-PLAN.md` §4B (the `Object` node carries a generic `Classification(system, code)` value-object, NOT `IfcClass`) and the seam `Classification/classification#CLASSIFICATION_AXIS` boundary ("a downstream `Rasm.Bim` projector owns the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto a seam `Classification` value at ingest") — so `ClassificationSystem.Classify` validates the code shape and mints the seam `Classification.Create(system, code, "", "", None, None)` (the SIX-member `(System, Code, Edition, Source, EditionDate, Title)` factory; the author/`Classify` path carries no resolved edition/publisher/title so the trailing four are `"", "", None, None`, `Ingest` lowering the root `IfcClassification.Edition`/`Source`/`EditionDate` plus the `IfcClassificationReference.Name` concept title for the imported leg), the seam owning the `Parent`/`Within` containment projections and this page owning only the roster. Classification is a VALUE on the `Object` node and NEVER an edge: the seam `Relations/relation#EDGE_ALGEBRA` is explicit ("classification is a generic value ON the `Object` node, NOT an edge … the seam carries no classification-association relationship", the `Associate` edge carrying a `Material`/`Appearance` resource), so the egress reads `node.Classification` and authors `IfcRelAssociatesClassification` from the value, never from an edge. The fault shape grounds against `Model/faults#FAULT_BAND` (`BimFault` `Expected`-derived, band 2600 IS the `Code`, the typed case lifting BARE off the threaded `Op key` with no `.ToError()` hop — the exact `Classify` `classification-code-reject` and `BsddClass.Of` `bsdd-class-malformed` routings faults.md enumerates) and the sibling `Model/elements#IFC_CLASS` `IfcClass.Resolve(string, Op key)` idiom; the type rename `Classification`→`ClassificationSystem` resolves the migration-source name collision with the seam value-object, and the `BimElement` binding (`ClassificationRef`/`ClassificationCode`) is retired with the `BimElement`/`BimModel` element records per §2/§4B.
- [BSDD_SERVICE_CONTRACT]: the bSDD RESTful dictionary class/property lookup response shape is GROUNDED against the published buildingSMART bSDD `Dictionaries API` contract (`.api/api-bsdd`) — `api/Class/v1` with `IncludeClassProperties=true` returns the `ClassContract.v1` (`code`/`name`/`uri`/`status`/`classType`/`definition`/`relatedIfcEntityNames[]` plus the `classProperties[]`), each `ClassPropertyContract.v1` carrying `name`/`propertyCode`/`propertySet`/`dataType`/`propertyValueKind`/`predefinedValue`/`isRequired`/`propertyStatus` PLUS the FULL class-scoped constraint surface (`allowedValues[]`, `pattern`, `minInclusive`/`maxInclusive`/`minExclusive`/`maxExclusive`, the seven `dimensionLength`/`dimensionMass`/… exponents, `units[]`) — so `BsddClass.Of` projects the rich `BsddProperty` matching the real wire and feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve(cls, schema, dictionary)` template (the dictionary rows unioned OVER the offline `Xbim.Properties` catalogue floor), the `Review/validation#IDS_FACETS` Classification + Property facets (the bSDD `pattern`/`min*`/`max*`/`allowedValues` narrow directly into the `Xbim.InformationSpecifications` `ValueConstraint`, `status` gates admission), and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion (the seven `Exponents` resolve the `UnitsNet` `BaseDimensions` 7-vector, `units[]` the source unit) directly — the naive six-field slice the migration source read is the deleted form. The LIVE-WIRE leg stays gated on the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport alignment (the `BsddPort` binding rides the injected Compute transport, never a transport minted here), and the in-process degradation to the row's local code-shape policy (`LocalShape`) is the verified settled fallback so ingest never blocks.
- [CLASSIFICATION_ROUNDTRIP]: the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip member spellings confirm against the GeometryGym entity surface (`.api/api-geometrygym-ifc` rows 07/08; `assay api` decompile) so the round-trip is BIDIRECTIONAL: `ClassificationSystem.Author` constructs `new IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect)` over an `IfcClassificationReference(db)` whose `ReferencedSource` is `new IfcClassification(db, Title){Specification}`, `Identification` the code, and `Location` the identifier URI (`IfcExternalReference.Location`/`Identification` decompile-confirmed settable, `IfcClassification.Specification`/`Name` decompile-confirmed) — the egress the `Projection/semantic#IFC_EGRESS` `Emit` composes per `Object` node, subsuming the retired `Rasm.Materials` `MaterialPropertyWire.Classification` material-wire half (`ELEMENT-REBUILD-PLAN.md` §6); `ClassificationSystem.Ingest` is the inverse the migration source never had — it walks the `IfcClassificationReference.ReferencedSource` `IfcClassificationReferenceSelect` chain past nested references to the root `IfcClassification` ONCE, resolves the standard system off that root's `Name` and the `Location` prefix against the roster, reads the code off `Identification`/`Location`, AND lowers the EDITION-SCOPED annotation bundle off the SAME root — the `Edition` token (seam IDENTITY), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed, the `DateTime.MinValue` sentinel folding to a `None` `EditionDate` via NodaTime `LocalDate.FromDateTime`) — so the imported leg lands the FULL six-member seam `Classification` value on the `Object` node rather than an edition-blank, title-`None` reference; the `IfcClassificationReference.ReferencedSource`/`Location`/`Identification`/`Name` + `IfcClassification.Name`/`Edition`/`Source`/`EditionDate` spellings decompile-confirmed (rows 07/08); an unrostered source resolves to `None` so a foreign classification rides the `Projection/semantic#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering.
