# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the seam `Classification` value an `Object` node carries: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/ETIM/IfcClassification), plus a `Project` row whose whole identity arrives as composition data, each row ONE reader answering its `SystemIdentity` (title, version-free `Stem`, hosted version, compiled code shape) out of the composition-supplied `BsddPins` policy, the stem and version deriving the versioned `DictionaryUri` the live resolution and the egress `Location` use, validating a raw code against the system's shape and LOWERING it onto the library-neutral seam `Classification` value — the full-identity factory `(System, Code, Edition, Source, EditionDate, Title)`: the `(System, Code, Edition)` edition-scoped IDENTITY plus the equality-excluded resolved-annotation bundle. The seam owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (no roster, no dictionary, no regex); this page is the downstream Bim projector the seam names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto the seam value at ingest and re-authoring that value at egress. A classification is the seam `Classification` VALUE on an `Object` node: `Relations/relation#EDGE_ALGEBRA` is explicit that classification is a value on the node, NOT an edge (the neutral `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the retired `(GlobalId, system, code)` triple bound to a second stored `BimElement` record is GONE — a query reads `node.Classification.System`/`Code`, never a stringly-keyed lookup against a second element store.

The round-trip is BIDIRECTIONAL across three entries: `Classify` lowers a validated raw code at authoring ingress, `Ingest` resolves an imported `IfcClassificationReference` back to the seam value at import ingress (the inverse of `Author`, the leg the migration source never had), and `Author` re-authors a node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress — the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node, which REPLACES the retired `Rasm.Materials` `MaterialPropertyWire.Classification` half (a material carries no classification; the `Object` node does): the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands a substance's standard `(system, code)` as the bound element's `Object`-node `Classification` value through its `MaterialBinding` egress, so the element classification this owner round-trips is the one the unified Component projection authored, never a material-wire field. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping carries the FULL `ClassContract.v1` surface: the class-scoped constraint surface (IFC `DataType` + Pset placement, `ValueKind`, allowed values, XSD `Pattern`, numeric `Bounds`, the SI base-dimension exponents the seam `Dimension` carries + `Units`, `Status`) that feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template, the `Review/validation#IDS_FACETS` Classification + Property facets, and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion directly, PLUS the relation set (`classRelations`/`reverseClassRelations` → typed `BsddRelation` rows whose `IsEqualTo`/`IsSynonymOf` edges the `BsddFederation` receipt closes through the shared QuikGraph substrate into cross-standard equivalence — `Translate` lowers an OmniClass code onto its Uniclass peer, a capability unreachable from any code string), the AUTHORITATIVE containment (`parentClassReference`/`hierarchy`/`childClassReferences` → `BsddRef` — the parent a MasterFormat/Uniformat code string does not encode), and the supersession surface (`status`/`replacedObjectCodes`/`replacingObjectCodes`/`deprecationExplanation` — `BsddClass.Admit` refuses to certify a NEW code onto an `Inactive` class, carrying the replacing code in the fault). `BsddResolution.Certify` is the dictionary-certified authoring lowering (`Resolve` → `Admit` → `Classify`, the resolved class's own code and title carried onto the minted value) and `BsddResolution.Search` resolves a concept label or IFC entity to candidate codes (`api/Class/Search/v1`) — so a new standard is one `ClassificationSystem` dictionary-identity row shared across `classification`, `properties`, and `validation`. The typed `Model/faults#FAULT_BAND` `BimFault` cases lift BARE onto the `Fin` rail (band 2600 IS the `Expected` `Code`; no `.ToError()` hop), each carrying the kernel `Op` operation context the caller threads.

## [01]-[INDEX]

- [02]-[CLASSIFICATION_AXIS]: `BsddPins` the composition-supplied hosted-version policy; `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary stem, version pin reader, code-shape policy); `Classify(code, title, key)` lowering a validated raw code onto a seam `Classification` value at authoring ingress; `Ingest(reference)` resolving an imported `IfcClassificationReference` back onto a seam value at import ingress; and `Author(db, related, classification, pins)` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` over the shared `Semantics/composition#EGRESS` `EmitMemo` dictionary-source memo (the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes).
- [03]-[BSDD_RESOLUTION]: the live bSDD dictionary resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, projecting the FULL `ClassContract.v1` surface — the constraint surface (`ValueKind`/`AllowedValues`/`Pattern`/`Bounds`/`SiDimension`/`Units`), the relation set (`BsddRelation` forward + reverse), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`) — degrading to the row's local code-shape policy ONLY when the endpoint was unreached; `BsddWire` the `[Mapper]` boundary transcription owning the rail-free wire rows; `BsddResolution.Certify` the supersession-gated authoring lowering, `BsddResolution.Search` the `api/Class/Search/v1` concept-to-code resolution carrying each hit's server-order `Rank`, and `BsddFederation` the QuikGraph connected-component equivalence receipt whose `Translate` lowers a code across standards onto the seam value; feeding the `Semantics/properties#PROPERTY_TEMPLATES` template, the IDS facets, and the seam `MeasureValue` coercion; the `Suggest` graph-scale enrichment fold returning ranked `ClassificationSuggestion` candidates per unclassified element and target system.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row ONE `Func<BsddPins, SystemIdentity>` reader answering the four facts a dictionary address and a code gate need — the display `Title`, the version-free `Stem` (the ingest prefix identity a foreign-edition `Location` still matches), the hosted `Version`, and the compiled code shape — the stem and version deriving the versioned `DictionaryUri` (the bSDD identifier scheme is `{org}/{dictionary}/{version}`; a versionless class URI does not resolve, so the version-bearing row IS the live-lane admission and an unhosted system reads a blank version so its live leg degrades by construction); `SystemIdentity` that four-fact carrier and `BsddPins` the ONE policy record supplying the hosted version tokens plus the `Custom` project-system identity the `Project` row reads WHOLE, so a durable standard freezes its identity at declaration while a registry-republished version and a client asset-code scheme alike stay values a composition overrides. The projector lowers a resolved standard code onto the seam `Classification/classification#CLASSIFICATION_AXIS` `Classification` value the `Object` node carries and authors it back at egress. The seam `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the seam defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the seam value, never re-declaring a classification value-object beside the seam.
- Entry: `ClassificationSystem.Classify(string code, Option<string> title, BsddPins pins, Op key)` validates the raw code against the system's code-shape regex and lowers it onto a seam `Classification(Key, code, "", None, None, title)` value — the `title` the resolved concept name where a caller HAS one (the `Certify` lane carries the dictionary class's own) and `None` on the bare authoring path, so one entry spans both without a mode flag; `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`, the typed case lifting BARE off `key`, no `.ToError()` hop); `ClassificationSystem.Ingest(IfcClassificationReference reference, BsddPins pins)` is the import-ingress inverse resolving the standard system off the reference's `ReferencedSource` root dictionary title, the root's `Specification` dictionary-URI prefix, OR its `Location` identifier-URI prefix against the roster (the code off `Identification` or the trailing `Location` segment, the resolved `Title` off the reference's own `Name`), returning `Option<Classification>` the `Projection/semantic#SEMANTIC_PROJECTOR` ingress accumulates onto the `Graph/element#NODE_MODEL` `Object` node's `Classifications` set (IFC admits MULTIPLE `IfcRelAssociatesClassification` per object) — `None` for an unrostered source so a foreign system rides the `Projection/relations#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification, BsddPins pins)` is the egress entry the `Emit` composes per `Object` node — `None` for the `IfcSystem.Key` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system's edition-scoped dictionary, `Identification` the code, `Location` the identifier URI, and `Name` the seam `Title` — the members `Ingest` reads back, so the full identity survives the egress leg.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the actual shape enforcement the migration source's decorative `ClassificationCode` never applied) and, on a match, mints the seam `Classification.Create(Key, code, "", None, None, title)` (the seam `[ComplexValueObject]` factory normalizing the system token and trimming the code, the edition-unspecified `Edition ""` and the `Source`/`EditionDate` annotations `None` on every ingress path since neither a raw code nor a dictionary class carries a publisher edition), so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30", "")` whose seam `Parent`/`Within` projections derive the containment hierarchy; a code the shape rejects faults rather than lowering a malformed value. `Ingest` walks `RootSource` to the root `IfcClassification` ONCE: it matches a roster row by the dictionary `Name` equality, the root's `Specification` dictionary-URI through the shared `ByUri` version-free `Stem` prefix (the URI `Author` itself stamps, so the round-trip self-resolves even when a re-export strips per-reference `Location`s), or the reference `Location` prefix (a `Location` minted under ANY dictionary edition — a foreign 2015-edition reference, a future `ifc/4.6` class URI — still resolves its row), reads the code off `CodeOf` (`TailCode`-unescaping the `Location` trailing segment when the `Identification` is absent), and lowers the EDITION-SCOPED annotation bundle off that SAME root — the `Edition` token (IDENTITY on the seam), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed) — while `TitleOf` reads the reference's OWN `IfcClassificationReference.Name` (distinct from the root dictionary `Name`) as the resolved concept `Title`, so the imported leg lands the FULL identity (edition + publisher + date + title) at the only path that can populate it rather than a perpetually edition-blank, title-`None` reference. `Author` resolves the row through the generated `TryGet` and authors the reference with the URI-escaped `Location` (`ClassUri`) so an OmniClass/MasterFormat code carrying spaces round-trips, the reference `Name` carrying the seam `Title` and the `(db, system, edition)`-keyed dictionary memo re-stamping the value's `Edition`/`Source`/`EditionDate` — the egress leg of the same identity `Ingest` lowers; the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row whose reader closes over its frozen `SystemIdentity`; a dictionary the public bSDD starts hosting is one version column on `BsddPins` plus the row's reader, never a shape edit; a PROJECT-OWN system is a `SystemIdentity` value on `BsddPins.Custom` with no roster edit at all, the `Project` row already reading it; a registry that re-publishes a dictionary version is one value the composition passes, never a durable-page edit; the seam `Classification` value-object absorbs any `(system, code)` pair with no seam edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type, never a parallel classification value-object beside the seam, and never a per-direction resolver (one `Classify`/`Ingest`/`Author` triad spans the whole round-trip).
- Boundary: the classification systems are ONE keyed axis (`ClassificationSystem` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the classification VALUE is the seam `Classification` `[ComplexValueObject]` and a Bim `Classification`/`ClassificationCode`/`ClassificationRef` value-object is the deleted form — the seam owns the typed pair, this page owns the standard-systems roster and lowers onto it, so the type name `ClassificationSystem` never collides with the seam `Classification`; the hosted VERSION tokens and the project-own system identity are composition-supplied `BsddPins` values, and a version literal or a client asset-code scheme frozen into a durable roster row is the deleted form — a token the registry re-publishes rots into a `404` the offline degrade then masks as unreachability, so it lives at the one overridable value whose defaults serve an unconfigured composition; the `Classify(BimElement element, …)` binding to a `BimElement.GlobalId` is GONE (the `BimElement`/`BimModel` are retired, the consumer element being the `Graph/element#ELEMENT_GRAPH` `Bake` fold) — a classification is the seam value on the `Object` node, never a `(GlobalId, system, code)` triple keyed to a second element record; classification is a VALUE on the `Object` node and NOT an edge (the seam `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the egress reads the node `Classification` value and a classification-association `Relationship` case is the deleted form; the code shape is the row's regex validated once at `Classify`, never a per-call regex at the call site; every comparison against a system key runs `OrdinalIgnoreCase` — the key space the row's own `[KeyMemberEqualityComparer]` declares — so a raw `!=`/`==` on a system token is the deleted form that reads `"IFC"` and `"ifc"` as different systems, and a bare `"ifc"` literal where `IfcSystem.Key` names the same token is the deleted form; the typed `BimFault` lifts BARE off the threaded `Op key` and a `.ToError()` hop or a single-string fault ctor is the named defect this owner closes (the band IS the `Expected` `Code`); the bSDD dictionary is the authoritative live source for the class-to-property constraint surface resolved through the dictionary URI, never a hardcoded code-to-property table that duplicates and drifts from it; the per-system `CodeShape` regex is BOTH the cheap LOCAL shape gate `Classify` admits a raw authoring code through (no network round-trip) AND the offline degradation `BsddResolution.LocalShape` falls back to, never that drifting constraint table; the classification round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`.api/api-geometrygym-ifc`) consumed as settled vocabulary, the egress carrying `Identification` + `Location` (+ the `Name` concept title the seam `Classification.Title` round-trips) so the import `Ingest` reconstructs the seam value losslessly, never re-minting a classification mapping; the db-scoped dictionary memo composes the `Semantics/composition#EGRESS` `EmitMemo` owner and a second `ConditionalWeakTable` declared here is the deleted duplicate; the egress reads the seam `Object` node `Classification`, NOT a Materials `MaterialPropertyWire.Classification` carrier (retired), the material-wire classification half having moved to this element-classification egress.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using System.Text.RegularExpressions;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;                                       // LocalDate — the seam Classification.EditionDate annotation the Ingest leg lowers off IfcClassification.EditionDate
using Rasm.Bim;
using Rasm.Element.Classification;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries

namespace Rasm.Bim.Semantics;

// --- [TYPES] ------------------------------------------------------------------------------
// One classification system's RESOLVED identity — the four facts a roster row needs to address a dictionary and gate a
// code: the display title, the version-free URI stem, the hosted version (blank = unhosted), and the compiled code
// shape. It is ONE carrier rather than three row columns because the Custom row resolves all four from composition
// data, so a per-fact reader would multiply every row's shape by the number of facts a project system supplies.
// The Shape initializer compiles ONCE per value (a record property initializer reads the primary-ctor parameter), so
// a policy value built at composition carries a compiled NonBacktracking matcher rather than parsing per call.
public sealed record SystemIdentity(string Title, string Stem, string Version, string Pattern) {
    public static readonly SystemIdentity Unclaimed = new("", "", "", @"^(?!)$");   // matches nothing: an unconfigured project system admits no code

    public Regex Shape { get; } = new(Pattern, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);
}

// The composition-supplied bSDD policy: the hosted dictionary VERSIONS plus the project's OWN classification system.
// The bSDD identifier scheme is {org}/{dictionary}/{version} and a versionless class URI does not resolve, so a
// live-hosted row must pin a version — yet a version is a LIVE registry fact re-published on the registry's own
// cadence, and a token frozen into a durable roster rots into a 404 the offline degrade then masks as plain
// unreachability. Pinning them here keeps that rot at ONE overridable value: an app resolves current pins from its own
// configuration and passes the record, and the defaults serve an unconfigured composition. A row with no hosted
// dictionary reads blank and its live leg degrades to the local code shape by construction; a dictionary the public
// bSDD starts hosting is one column here plus its row's reader, never a roster edit.
// Custom is the same mechanism carried one step further: a project's in-house system (a client asset-code scheme, a
// contractor work-breakdown code) is COMPOSITION DATA — a title, a URI space, and a shape — not a durable roster row,
// so the roster names the standards it can name and every project system rides this one value. Unclaimed is the
// unconfigured default whose shape admits nothing, so an unconfigured composition never silently classifies onto it.
public sealed record BsddPins(string Ifc, string Etim, SystemIdentity Custom) {
    public static readonly BsddPins Default = new(Ifc: "4.3", Etim: "10.0", Custom: SystemIdentity.Unclaimed);
}

// The standard classification-systems roster lowering onto the seam Classification value-object. The type name
// ClassificationSystem is distinct from the seam Classification so the seam value-object is the one canonical
// classification; this page owns only the roster (dictionary stem + pin reader + code shape) and the bSDD lane.
// Classification is a VALUE on the Object node (Relations/relation#EDGE_ALGEBRA — never an edge), so this projector
// lowers a resolved code onto that value at ingest (Classify for a raw code, Ingest for an imported
// IfcClassificationReference) and re-authors the node's standard value back onto IfcRelAssociatesClassification at
// egress (Author).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClassificationSystem {
    // Every row is ONE reader into the caller's BsddPins policy answering the row's SystemIdentity. A durable standard
    // closes over its own frozen identity and reads only the hosted version off the policy (ifc/etim hosted on the
    // public bSDD; uniclass2015/omniclass/masterformat/uniformat unhosted, blank version, degrading to the local shape
    // BY CONSTRUCTION — hosting later is one BsddPins column, never a roster edit). The Project row reads its WHOLE
    // identity off the policy, which is what makes a client asset-code scheme composition data rather than a durable
    // row nobody outside one project can use.
    public static readonly ClassificationSystem Uniclass2015 = Standard("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", @"^[A-Z][A-Za-z]_\d{2}(_\d{2}){0,3}$");
    public static readonly ClassificationSystem OmniClass     = Standard("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", @"^\d{2}-\d{2}( \d{2}){2,3}$");
    public static readonly ClassificationSystem MasterFormat  = Standard("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", @"^\d{2} \d{2} \d{2}(\.\d{2})?$");
    public static readonly ClassificationSystem Uniformat     = Standard("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", @"^[A-Z]\d{4}$");
    public static readonly ClassificationSystem Etim          = new("etim", static pins => new SystemIdentity("ETIM", "https://identifier.buildingsmart.org/uri/etim/etim", pins.Etim, @"^EC\d{6}$"));
    public static readonly ClassificationSystem IfcSystem     = new("ifc", static pins => new SystemIdentity("IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", pins.Ifc, @"^Ifc[A-Za-z]+$"));
    public static readonly ClassificationSystem Project       = new("project", static pins => pins.Custom);

    public Func<BsddPins, SystemIdentity> Resolve { get; }

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape the IfcRelKind roster also takes).
    private ClassificationSystem(string key, Func<BsddPins, SystemIdentity> resolve) : this(key) => Resolve = resolve;

    // A durable standard's identity is frozen at declaration, so its reader closes over ONE SystemIdentity value —
    // compiled once at type init rather than per resolution. The code shape is a compiled NonBacktracking row policy:
    // a Uniclass table prefix is two letters with the second upper OR lower (Ss/Pr/EF/SL/TE), so [A-Z][A-Za-z] admits
    // every real prefix, and no call site ever parses a Regex or exposes a backtracking surface to hostile input.
    static ClassificationSystem Standard(string key, string title, string stem, string pattern) {
        var identity = new SystemIdentity(title, stem, "", pattern);
        return new ClassificationSystem(key, _ => identity);
    }

    public string Title(BsddPins pins) => Resolve(pins).Title;

    public string Stem(BsddPins pins) => Resolve(pins).Stem;

    public Regex CodeShape(BsddPins pins) => Resolve(pins).Shape;

    // The versioned request/egress identity: the live api/Class/v1 lane and the authored Location/Specification carry
    // it; the version-free Stem stays the ingest prefix identity so the two never drift (one row value, one derivation).
    public string DictionaryUri(BsddPins pins) =>
        Resolve(pins) switch { { Version.Length: > 0 } hosted => $"{hosted.Stem}/{hosted.Version}", var bare => bare.Stem };

    // A row whose live leg can resolve at all — the search scope default and the hosted-roster probe. Reading it off
    // the version keeps "hosted" one fact rather than a second boolean column to keep in step.
    public bool Hosted(BsddPins pins) => Resolve(pins).Version.Length > 0;

    // INGRESS (raw code): validate against the system's code shape and lower onto a seam Classification value the Object
    // node carries — the actual shape enforcement, then the seam factory. `title` is the resolved concept name where the
    // CALLER holds one: the Certify lane passes the dictionary class's own Name, the bare authoring path passes None, so
    // one entry spans both and no arity or mode flag forks it. Both paths are edition-UNSPECIFIED (Edition "" with a
    // None Source/EditionDate bundle — neither a raw code nor a dictionary class carries a publisher edition; that is an
    // Ingest concern). The typed UnmappedClass case lifts BARE off the caller's Op (band 2600 IS the Expected Code; no
    // .ToError() hop) on a shape reject.
    public Fin<Classification> Classify(string code, Option<string> title, BsddPins pins, Op key) =>
        CodeShape(pins).IsMatch(code.Trim())
            ? Fin.Succ(Classification.Create(Key, code, "", None, None, title))
            : Fin.Fail<Classification>(new BimFault.UnmappedClass(key, $"classification-code-reject:{Key}:{code}"));

    // The dictionary class URI the bSDD resolution fetches AND the IfcClassificationReference.Location the egress writes:
    // the dictionary URI plus the URI-escaped code, so an OmniClass/MasterFormat code carrying spaces (e.g. "23-13 35 00")
    // produces a valid request/round-trip URI Ingest can unescape back to the code.
    public string ClassUri(string code, BsddPins pins) => $"{DictionaryUri(pins)}/class/{System.Uri.EscapeDataString(code.Trim())}";

    // The roster resolver every URI-shaped read shares (Ingest Location, Search hit dictionary, federation Translate):
    // prefix on the version-FREE Stem, LONGEST stem winning so a nested-stem row (one dictionary extending a sibling's
    // URI space) resolves by specificity, never by declaration order — and version-free, so a pin the registry moves
    // never breaks an ingest that already matched.
    internal static Option<ClassificationSystem> ByUri(string uri, BsddPins pins) =>
        uri is { Length: > 0 }
            ? Optional(Items.Select(row => (Row: row, Stem: row.Stem(pins)))
                .Where(row => row.Stem.Length > 0 && uri.StartsWith(row.Stem, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(static row => row.Stem.Length).Select(static row => row.Row).FirstOrDefault())
            : None;

    // The identifier-URI code authority: the trailing segment unescaped ({...}/class/{code} — the inverse of ClassUri).
    internal static string TailCode(string uri) => System.Uri.UnescapeDataString(uri[(uri.LastIndexOf('/') + 1)..]);

    // INGRESS (imported IFC): an IfcClassificationReference -> the seam standard Classification the projector lands on the
    // Object node value (the inverse of Author, the leg the migration source never had). The root IfcClassification
    // dictionary is walked ONCE: the system resolves off its Name, its Specification dictionary-URI (the IFC4X3 rename of
    // the retired dictionary-level Location — the very URI Author stamps, so a re-export that strips per-reference Locations still
    // self-resolves, and a third-party export carrying only the dictionary-level URI resolves too), OR the reference's
    // Location identifier-URI prefix against the roster, and the EDITION-SCOPED annotation bundle the seam Classification
    // carries (the publisher Source, the string Edition, the EditionDate revision date — IfcClassification.Source/Edition/
    // EditionDate decompile-confirmed, .api/api-geometrygym-ifc row 08) lowers off that SAME root so the imported leg
    // lands the full identity, never a perpetually-edition-blank reference. The dictionary Edition is IDENTITY on the seam
    // (a re-editioned re-ingest must not collide §EDITION_SCOPING), so reading it off the root is the only ingest path
    // that can populate it. None for an unrostered source so a foreign system rides the Generic passthrough rather than a
    // wrong lowering, and a blank code yields None rather than throwing through the seam Create.
    public static Option<Classification> Ingest(IfcClassificationReference reference, BsddPins pins) {
        IfcClassification? dictionary = RootSource(reference);
        return (Optional(Items.FirstOrDefault(row => row.Title(pins) is { Length: > 0 } title && string.Equals(title, dictionary?.Name ?? "", StringComparison.OrdinalIgnoreCase)))
            | ByUri(dictionary?.Specification ?? "", pins)
            | ByUri(reference.Location ?? "", pins))
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

    // The db-scoped dictionary-source memo over the SHARED Semantics/composition#EGRESS EmitMemo owner: ONE
    // IfcClassification per (db, system, EDITION) across every Author call, so an egress that classifies N objects
    // under one Uniclass edition authors ONE dictionary entity, not N duplicates — while a 2015-edition and a
    // 2023-edition value author TWO (seam Edition is IDENTITY, and an IfcClassification IS an edition-scoped
    // dictionary; one shared entity would strip the edition and fork every re-ingested content key).
    static readonly EmitMemo<(string System, string Edition), IfcClassification> Sources = new();

    // The minted dictionary re-stamps the value's edition-scoped bundle — Edition (identity), the Source publisher, the
    // EditionDate revision date (Edition/Source/EditionDate setters decompile-confirmed) — so Ingest reads back the SAME
    // identity Author lowered; an edition-unspecified value ("" + None bundle) authors the bare dictionary.
    static IfcClassification Source(DatabaseIfc db, ClassificationSystem row, Classification classification, BsddPins pins) =>
        Sources.Of(db, (row.Key, classification.Edition), _ => {
            var dictionary = new IfcClassification(db, row.Title(pins)) { Specification = row.DictionaryUri(pins), Edition = classification.Edition };
            classification.Source.IfSome(publisher => dictionary.Source = publisher);
            classification.EditionDate.IfSome(date => dictionary.EditionDate = date.ToDateTimeUnspecified());
            return dictionary;
        });

    // EGRESS: the Object node's standard Classification value -> the IfcRelAssociatesClassification/IfcClassificationReference
    // the Projection/egress#IFC_EGRESS Emit composes per Object node — the element-classification egress that REPLACES the
    // retired Materials material-wire classification half (a material carries no classification, the Object node does). None
    // for the IfcSystem entity-type code (the IfcClass the object author already resolved via IfcClass.Resolve) or an
    // unrostered system, so only a real standard system authors a reference. The system compare runs OrdinalIgnoreCase
    // because that IS the roster's declared key space — a raw != reads "IFC" and "ifc" as different systems and authors a
    // duplicate entity-type reference. The per-node reference carries Identification (the code) + Location (the identifier
    // URI the Ingest reads back) + Name (the seam Title, the concept label TitleOf reads back) + the SHARED
    // ReferencedSource (the one IfcClassification per (system, edition) the Source memo resolves), so the model carries one
    // dictionary entity per system edition and the FULL seam identity survives the egress leg.
    // The IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect) + IfcClassificationReference(db)
    // {ReferencedSource,Identification,Location,Name} + IfcClassification(db, name){Specification,Edition} ctor surface is
    // decompile-confirmed (.api/api-geometrygym-ifc rows 07/08; every stamped member a settable IfcExternalReference/
    // IfcClassification attribute).
    public static Option<IfcRelAssociatesClassification> Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification, BsddPins pins) =>
        !string.Equals(classification.System, IfcSystem.Key, StringComparison.OrdinalIgnoreCase)
        && TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            ? Some(new IfcRelAssociatesClassification(
                new IfcClassificationReference(db) {
                    ReferencedSource = Source(db, row, classification, pins),
                    Identification   = classification.Code,
                    Location         = row.ClassUri(classification.Code, pins),
                    Name             = classification.Title.IfNone(""),
                },
                related))
            : None;
}
```

## [03]-[BSDD_RESOLUTION]

- Owner: `BsddResolution` the live bSDD dictionary resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the endpoint was UNREACHED so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence carrying the FULL `ClassContract.v1` surface — the class-scoped constraint surface the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve` template, the IDS facets, and the seam `MeasureValue` coercion read, the forward + reverse relation set (`BsddRelation`), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows — the parent a MasterFormat/Uniformat code string does not encode, where the seam `Classification.Parent` code-shape derivation is structurally blind), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`); `BsddWire` the `[Mapper]` boundary transcription owning the rail-free wire-row crossings; `BsddFederation` the cross-standard equivalence receipt over the relation graph.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key)` resolves the full dictionary-class evidence over Compute's transport — `Fin<T>` returns the resolved `BsddClass` on a live hit; an UNREACHED endpoint degrades to the row's local code-shape policy APPLIED (`LocalShape` — a shape-passing code admits as property-free `Active` evidence, a shape-reject faults `BimFault.UnmappedClass` with the same `classification-code-reject` detail `Classify` mints; unreachability itself never faults), while a REACHED endpoint whose body the decoder rejected faults `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifting BARE off `key` — the two are distinct states on the port's own return, never inferred from one collapsed failure. `BsddResolution.Certify(system, code, port, pins, token, key)` is the dictionary-certified authoring lowering composing `Resolve` → `BsddClass.Admit` → `Classify`, so a NEW code never lands on an `Inactive`/superseded class AND the certified value carries the dictionary's OWN code and concept title rather than discarding the evidence it just paid a round-trip for (offline the shape-gated degrade admits and the row's local shape gates alone — certification tightens when the dictionary answers, never blocks when it cannot). `BsddResolution.Search(text, scope, relatedIfcEntity, port, pins, token, key)` resolves a concept label or IFC entity to candidate `BsddHit` codes over `api/Class/Search/v1`, each hit carrying its server-order `Rank` — authoring-time, never the ingest path, no offline concept index exists, so BOTH an unreached endpoint and an undecodable body FAULT `BimFault.CodecReject` lifted BARE off `key`. `BsddResolution.Suggest(ElementGraph graph, Seq<ClassificationSystem> targets, BsddPort port, BsddPins pins, CancellationToken token, Op key)` is the graph-scale ENRICHMENT fold — every entity-type-classified occurrence with no classification in a target system resolves ranked candidate codes once per distinct QUERY key through the same `Search` wire (server order the rank, the roster the admission), returning per-element `ClassificationSuggestion` rows an authoring surface confirms in bulk through `Certify` — the as-received unclassified model gains its mandated Uniclass/OmniClass coverage without per-element hand lookup, the difference between a checker and an authoring assistant. `BsddFederation.Of(classes)` folds resolved evidence into the equivalence closure; `Translate(classification, target, pins)` lowers a code across standards onto the seam value; `Equivalent`/`EquivalentSet` answer the pairwise and set queries.
- Auto: `Resolve` builds the class request from `ClassificationSystem.ClassUri(code, pins)` with `IncludeClassProperties`/`IncludeClassRelations`/`IncludeReverseRelations`/`IncludeChildClassReferences` (hierarchy and `parentClassReference` arrive by default), issues it over the injected `BsddPort` (the Compute transport seam, ONE generic `Fetch<TWire>` — the page owns the resource query, the transport owns base URL, headers, the STJ camelCase decode, and the reached-versus-unreached discrimination), and projects the wire `BsddClassResponse` into the `BsddClass` evidence through `BsddClass.Of`: each `ClassProperty` projects through `Property` into a `BsddProperty` carrying the IFC `DataType`/Pset placement (the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template), the `ValueKind`/`AllowedValues`/`Pattern`/`Bounds` value constraints (the `Review/validation#IDS_FACETS` `ValueConstraint`), the seam `Dimension` built directly from the wire's SI exponent columns + `Units` (the seam `Properties/quantity#DIMENSION` owner's own generated factory, never a page-local exponent record beside it), and the `Status` (the IDS admission gate); each `ClassRelation` projects through `RelationOf` into a typed `BsddRelation`; the containment pointers project through the `BsddWire` mapper's generated `Ref` transcriptions. `BsddFederation.Of` folds the `IsEqualTo`/`IsSynonymOf` rows (forward AND reverse — an inbound equivalence declared by the OTHER dictionary counts) into a transient `UndirectedGraph<string, SEquatableEdge<string>>` ONE edge per relation, labels it through `AlgorithmExtensions.ConnectedComponents`, and projects each component whole onto every member URI — equivalence under a symmetric relation IS connected-component membership, so the linear component pass replaces the quadratic transitive-closure edge set and its per-source `GroupBy` re-projection; the QuikGraph fold is transient inside `Of`, the receipt carries data only. A transport miss degrades to `LocalShape` so a new standard becomes a dictionary-identity row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference. `Suggest` groups ONLY on the axes the query carries — the entity-class code and the search text — because the predefined token never reaches the request, and a group key carrying it split one query into N identical round-trips; the fold threads the caller's `CancellationToken` into every `Fetch`, so the abort grain is one in-flight request boundary and a request already dispatched runs to its own completion.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_TEMPLATES` owner (`PropertyKey.Resolve(cls, predefined, schema, scope, Option<BsddClass>)` unioning the `BsddClass.Properties` dictionary rows OVER the offline `Xbim.Properties` catalogue floor under the caller's `TemplateScope` definition set, dictionary-wins), the IDS Classification + Property facets (the class URI is the facet value; `Pattern`/`Bounds`/`AllowedValues` narrow into an `Xbim.InformationSpecifications` `ValueConstraint`; `Status` gates admission), and the seam `MeasureValue` (a dimensioned property's `SiDimension` IS the seam `Dimension` the `UnitsNet` `BaseDimensions` projection reads); the `BsddFederation` receipt is the cross-standard translation evidence a multi-standard deliverable reads (`Translate` an OmniClass-classified model onto its Uniclass peers), and `Ancestry` is the authoritative inheritance chain a facet `partOf`/rollup read walks; each `BsddHit.Rank` is the server's own result ordinal, so a consumer re-ranks against the relevance the registry expressed rather than against a score the wire does not publish — so a new dictionary is one identity row across all consumers.
- Packages: Rasm.Element, LanguageExt.Core, QuikGraph, Thinktecture.Runtime.Extensions, Generator.Equals, Riok.Mapperly, Rasm
- Growth: a new bSDD dictionary is one `ClassificationSystem` row (stem + pin reader + shape) plus its `BsddPins` column; the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; a new dictionary-declared constraint is one read field on the `BsddClassResponse.ClassProperty` wire projected through `Property`, a new relation kind one `BsddRelationKind` member the closure filter reads, a new resource one query builder on `BsddResolution` (never a port member); a new rail-free wire row is one `BsddWire` partial, never a hand-rolled transcription beside it — never a parallel evidence record, never a per-system classifier, and never a `Rasm.Persistence` reference; federation growth is more resolved evidence folded into `Of`, zero type edits.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreached-endpoint degradation only; the port returns a THREE-state outcome (decoded body / unreached endpoint / undecodable body) and collapsing the last two into one failure is the deleted form that masks a contract drift as an offline miss and silently substitutes property-free local evidence for a class the registry answered for; the `BsddClass.Of` projection reads ONLY the fields the `.api/api-bsdd` catalog enumerates (the wire is `additionalProperties:false`, so an unexpected member signals contract drift, not a capability) and a field absent from the catalog is a phantom — the search wire publishes NO numeric relevance, so the hit's server-order ORDINAL is the rank column and a fabricated score is the deleted form; the SI exponent vector is the seam `Properties/quantity#DIMENSION` `Dimension` built through its own generated factory, and a page-local seven-int exponent record beside it is the deleted duplicate that forced every consumer to re-project one concept twice; the class-level constraint (`ClassProperty.AllowedValues`/`Min*`/`Max*`/`Pattern`) is read, never silently the property master, so a class that narrows an enumeration is honored; the cross-standard equivalence closure folds the `IsEqualTo`/`IsSynonymOf` relations through the shared `QuikGraph` substrate the folder admits — the relation is SYMMETRIC, so the container is undirected and the answer is `ConnectedComponents`, where a directed double-edge fold plus `ComputeTransitiveClosure` materialized the whole quadratic reachability edge set to recover a partition the linear pass yields directly; a hand-rolled BFS/union-find over a `Map<>` adjacency is the named rejected form (`.api/api-quikgraph`); the authoritative containment (`parentClassReference`/`hierarchy`) is read for a code that does NOT encode its parent (MasterFormat/Uniformat), and re-deriving containment from the code string where the dictionary states it is the rejected form; supersession gates authoring — `BsddClass.Admit` refuses a NEW code onto an `Inactive` class carrying the `ReplacedBy` code, and silently authoring a superseded class is the named defect; a `Certify` that resolves a class and then discards its code and concept title is the deleted form — the round-trip is paid, so the certified value carries the dictionary's own identity; the rail-free wire rows transcribe through the ONE `BsddWire` `[Mapper]` and a hand-written `RefOf`/`AllowedValue` projection beside it is the deleted form, while every crossing that carries a `Fin` admission (`ValueKindOf`/`StatusOf`/`RelationOf`/`BoundsOf`) stays hand-written by law — Mapperly transcribes shape, never a lane; the live fetch rides the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport injected as `BsddPort` (ONE generic `Fetch<TWire>` the page parameterizes by resource — a per-resource port member is the rejected form) and a transport minted here is the named seam violation; the port carries the caller's `CancellationToken` and the abort grain is DECLARED — one in-flight request boundary, a dispatched request running to its own completion — so a graph-scale fold that cannot be abandoned between round-trips is the deleted form and an unqualified cancellable claim over the request itself is the overclaim; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the enrichment fold groups on the QUERY axes ALONE and a group key carrying an axis the request never sends is the deleted form that multiplies one round-trip by the cardinality of a token the server never sees; a bare system-token literal where `IfcSystem.Key` names the same value is the deleted form, and every system comparison runs `OrdinalIgnoreCase` per the roster's declared key space; the resolution degrades to the local policy on an unreached endpoint so INGEST never blocks (faulting on unreachability itself is the named defect) while the degraded verdict IS the row's shape gate — a shape-rejected code faults `BimFault.UnmappedClass`, never a fabricated `Active` evidence the dictionary did not answer — and `Search` is authoring-only, so BOTH its failure states fault `BimFault.CodecReject` because no offline concept-to-code resolution exists; a raw Compute transport error crossing this AEC-domain entry unwrapped is the named boundary defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using QuikGraph;                                      // the shared graph substrate the BsddFederation equivalence fold runs on (never a hand-rolled BFS)
using QuikGraph.Algorithms;
using Riok.Mapperly.Abstractions;
using Rasm.Element.Graph;                             // ElementGraph/NodeId/ObjectKind — the Suggest enrichment fold's graph read
using Rasm.Element.Properties;                        // the seam Dimension the bSDD exponent columns build directly

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

// The class-relation vocabulary (ClassRelationContract.v1.relationType, catalog order): the IsEqualTo/IsSynonymOf
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
// (ClassSearchResponseClassContract.v1 — the URI tail only the fallback when a dictionary omits it), the
// RelatedIfcEntities aligning the hit to the Model/elements#IFC_CLASS IfcClass entity, and Rank — the hit's ZERO-BASED
// position in the response's own classes[] array. The search wire publishes NO numeric relevance column (the contract is
// additionalProperties:false and carries none), so the server's ORDER is the whole relevance signal it expresses and the
// ordinal is the honest carrier of it: a consumer merging hits across dictionaries or re-ranking against its own priors
// needs the server's opinion as data, and a fabricated score would be a phantom the catalog refutes.
public readonly record struct BsddHit(ClassificationSystem System, string Code, string Name, string Uri, Seq<string> RelatedIfcEntities, int Rank);

// One enrichment row per (unclassified element, target system): the ranked candidate codes an authoring surface
// confirms in bulk — each accepted candidate lowers through Certify, so the confirmation path IS the standing
// dictionary-certified authoring lowering, never a second admission. Candidates stay in the server's Rank order.
public readonly record struct ClassificationSuggestion(NodeId Element, ClassificationSystem System, Seq<BsddHit> Candidates);

// The resolved bSDD property evidence carrying the FULL class-scoped constraint surface (.api/api-bsdd ClassPropertyContract.v1):
// the IFC DataType + Pset placement the Semantics/properties#PROPERTY_TEMPLATES PropertyKey threads into the seam PropertyValue,
// the ValueKind + AllowedValues + Pattern + Bounds the Review/validation#IDS_FACETS facet narrows into a ValueConstraint,
// the SiDimension + Units the seam MeasureValue UnitsNet coercion reads, and the Status the IDS admission gate honors.
// SiDimension is the SEAM Dimension itself, built once here from the wire's exponent columns — the dictionary's seven
// integers and the seam's dimension are ONE concept, so a page-local exponent record would make every consumer project
// it twice. ValueKind and Status are REQUIRED head members on two grounds: a [SmartEnum] row is a static readonly
// field, not a constant, so it cannot be an optional-parameter default at all; and a defaulted Active is exactly the
// fabricated evidence this owner's boundary refuses — an unanswered status must be stated by its caller, never
// assumed. The trailing CONSTRAINT members default empty, so the offline degrade and the Semantics/properties anchor
// construction stay valid on the head alone.
// The collection members are SERVER-ORDERED sets, not sequences: the registry may reorder allowed values or units
// between reads, so an ordered comparison would report a difference the dictionary never made.
[Equatable]
public sealed partial record BsddProperty(
    string Code, string Name, string DataType, string PropertySet, string PredefinedValue, bool IsRequired,
    BsddValueKind ValueKind,
    BsddStatus Status,
    [property: UnorderedEquality] Seq<BsddAllowedValue> AllowedValues = default,
    Option<string> Pattern = default,
    Option<BsddBounds> Bounds = default,
    Option<Dimension> SiDimension = default,
    [property: UnorderedEquality] Seq<string> Units = default);

// The authoritative classification evidence the classification/properties/validation owners share. RelatedIfcEntities
// aligns the bSDD class to the Model/elements#IFC_CLASS IfcClass entity; Status gates IDS admission; the relation set
// feeds the BsddFederation closure; Parent/Ancestry/Children carry the authoritative containment; Replaces/ReplacedBy/
// Deprecation carry the supersession the Admit gate reads. The trailing members default empty so the LocalShape degrade
// and the Semantics/properties anchor construction stay valid.
// Equality is STRUCTURAL and collection-aware: every server-ordered member compares as a multiset, because the registry
// reorders them freely between reads and an ordered comparison would report drift the dictionary never made — except
// Ancestry, which is ORDER-BEARING by construction (the wire level sorts it root-first, so a reordered chain IS a
// different inheritance path and must compare unequal).
[Equatable]
public sealed partial record BsddClass(
    string Code, string Name, string ClassType, string Definition, string Uri,
    [property: UnorderedEquality] Seq<BsddProperty> Properties,
    BsddStatus Status,
    [property: UnorderedEquality] Seq<string> RelatedIfcEntities = default,
    [property: UnorderedEquality] Seq<BsddRelation> Relations = default,
    [property: UnorderedEquality] Seq<BsddRelation> ReverseRelations = default,
    Option<BsddRef> Parent = default,
    [property: OrderedEquality] Seq<BsddRef> Ancestry = default,
    [property: UnorderedEquality] Seq<BsddRef> Children = default,
    [property: UnorderedEquality] Seq<string> Replaces = default,
    [property: UnorderedEquality] Seq<string> ReplacedBy = default,
    Option<string> Deprecation = default) {
    // The response -> evidence projection: a live hit returning a class with no Code/Uri is INVALID published data, not an
    // offline dictionary miss, so it faults BimFault.CodecReject BARE off the caller's Op (surfaced, never masked as a
    // LocalShape miss). The unreached-endpoint degradation is Resolve's concern; this projection only judges payload shape.
    public static Fin<BsddClass> Of(BsddClassResponse response, Op key) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.CodecReject(key, $"bsdd-class-malformed:{response.Uri}"))
            : from status in StatusOf(response.Status, key)
              from properties in toSeq(response.ClassProperties ?? []).TraverseM(p => Property(p, key)).As()
              from relations in toSeq(response.ClassRelations ?? []).TraverseM(r => RelationOf(r, key)).As()
              from reverse in toSeq(response.ReverseClassRelations ?? []).TraverseM(r => RelationOf(r, key)).As()
              select new BsddClass(
                  response.Code, response.Name, response.ClassType ?? "", response.Definition ?? "", response.Uri,
                  properties, status, toSeq(response.RelatedIfcEntityNames ?? []), relations, reverse,
                  Optional(response.ParentClassReference).Map(BsddWire.Ref),
                  toSeq((response.Hierarchy ?? []).OrderBy(static item => item.Level)).Map(BsddWire.Ref),
                  toSeq(response.ChildClassReferences ?? []).Map(BsddWire.Ref),
                  toSeq(response.ReplacedObjectCodes ?? []),
                  toSeq(response.ReplacingObjectCodes ?? []),
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
    // (AllowedValues/Pattern/Bounds), the dimension (the seam Dimension off the wire's exponent columns), and the Status —
    // every field grounded in .api/api-bsdd, none fabricated. The two admissions are why this stays hand-written: a
    // generated map cannot carry a Fin lane.
    static Fin<BsddProperty> Property(BsddClassResponse.ClassProperty p, Op key) =>
        from kind in ValueKindOf(p.PropertyValueKind, key)
        from status in StatusOf(p.PropertyStatus, key)
        select new BsddProperty(
            p.PropertyCode ?? p.Name, p.Name, p.DataType ?? "", p.PropertySet ?? "", p.PredefinedValue ?? "", p.IsRequired,
            kind, status, toSeq(p.AllowedValues ?? []).Map(BsddWire.Allowed),
            Optional(p.Pattern).Filter(static s => s.Length > 0), BoundsOf(p), DimensionOf(p), toSeq(p.Units ?? []));

    static Option<BsddBounds> BoundsOf(BsddClassResponse.ClassProperty p) =>
        p is { MinInclusive: null, MaxInclusive: null, MinExclusive: null, MaxExclusive: null }
            ? None
            : Some(new BsddBounds(Optional(p.MinInclusive), Optional(p.MaxInclusive), Optional(p.MinExclusive), Optional(p.MaxExclusive)));

    // The dictionary's SI base-dimension exponents build the SEAM Dimension directly through its own generated
    // Dimension.Create factory — one concept, one type. A dimensionless result (every exponent zero) is the undimensioned
    // property, so it lowers None and the seam MeasureValue coercion never fires on a plain count or label.
    static Option<Dimension> DimensionOf(BsddClassResponse.ClassProperty p) =>
        Dimension.Create(
            p.DimensionLength ?? 0, p.DimensionMass ?? 0, p.DimensionTime ?? 0, p.DimensionElectricCurrent ?? 0,
            p.DimensionThermodynamicTemperature ?? 0, p.DimensionAmountOfSubstance ?? 0, p.DimensionLuminousIntensity ?? 0)
        is var dimension && dimension != Dimension.Dimensionless ? Some(dimension) : None;

    // Each ClassRelationContract -> the typed row: an unparseable relationType or a blank relatedClassUri faults so the
    // federation closure never sees an unaddressable edge.
    static Fin<BsddRelation> RelationOf(BsddClassResponse.ClassRelation relation, Op key) =>
        relation.RelatedClassUri is not { Length: > 0 }
            ? Fin.Fail<BsddRelation>(new BimFault.CodecReject(key, "bsdd-relation-uri-missing"))
            : BsddRelationKind.TryGet(relation.RelationType, out BsddRelationKind? kind) && kind is { } resolved
                ? Fin.Succ(new BsddRelation(resolved, relation.RelatedClassUri, relation.RelatedClassName ?? "", Optional(relation.Fraction)))
                : Fin.Fail<BsddRelation>(new BimFault.CodecReject(key, $"bsdd-relation-kind-unmapped:{relation.RelationType}"));

    static Fin<BsddValueKind> ValueKindOf(string? kind, Op key) =>
        BsddValueKind.TryGet(kind ?? "", out BsddValueKind? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddValueKind>(new BimFault.CodecReject(key, $"bsdd-value-kind-unmapped:{kind}"));

    static Fin<BsddStatus> StatusOf(string? status, Op key) =>
        BsddStatus.TryGet(status ?? "", out BsddStatus? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddStatus>(new BimFault.CodecReject(key, $"bsdd-status-unmapped:{status}"));
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The RAIL-FREE wire crossings, generated rather than hand-transcribed: a containment pointer and an allowed value are
// pure column maps with no admission, no fault, and no lookup, which is exactly the boundary Riok.Mapperly owns. The
// two Ref overloads discriminate on the wire shape (a ClassReference carries its Uri required, a HierarchyItem
// optional), and the one Text carrier absorbs every nullable wire string. Everything on this page that carries a Fin —
// ValueKindOf, StatusOf, RelationOf, BoundsOf, DimensionOf — stays hand-written: Mapperly transcribes shape, never a lane.
[Mapper]
public static partial class BsddWire {
    public static partial BsddRef Ref(BsddClassResponse.ClassReference reference);
    public static partial BsddRef Ref(BsddClassResponse.HierarchyItem item);
    public static partial BsddAllowedValue Allowed(BsddClassResponse.AllowedValue value);

    [UserMapping]
    internal static string Text(string? value) => value ?? "";
}

// The cross-standard equivalence receipt: the IsEqualTo/IsSynonymOf relation rows (forward AND reverse — an inbound
// equivalence declared by the OTHER dictionary counts) fold into a transient UndirectedGraph<string,
// SEquatableEdge<string>> and partition through the shared QuikGraph ConnectedComponents pass (.api/api-quikgraph — a
// hand-rolled BFS/union-find is the named rejected form). Equivalence under a SYMMETRIC relation IS connected-component
// membership, so the linear label pass yields the partition the quadratic ComputeTransitiveClosure edge set had to be
// re-grouped to recover — and the undirected container makes the per-relation reverse edge unnecessary at the same time.
// The graph never escapes Of: the receipt carries only the Equivalence map (each URI to its WHOLE component, ordinal
// sorted and self-inclusive, so a set read is one lookup) and the uri->label Names the Translate title reads.
public sealed record BsddFederation(Map<string, Seq<string>> Equivalence, Map<string, string> Names) {
    // The transient QuikGraph fold is the named statement seam — the mutable graph container is the platform surface.
    // ONE pass over the relation rows yields BOTH receipt inputs: each class's forward+reverse rows are concatenated,
    // filtered, and read once for its equivalence EDGE and its related-URI LABEL together. The retired second fold
    // re-walked the same concatenation for the names alone, so every relation row was allocated and scanned twice and
    // the two passes could disagree about which rows they had seen.
    public static BsddFederation Of(Seq<BsddClass> classes) {
        var (edges, names) = classes.Fold(
            (Edges: Seq<SEquatableEdge<string>>(), Names: Map<string, string>()),
            static (acc, cls) => (cls.Relations + cls.ReverseRelations).Fold(
                (acc.Edges, Names: acc.Names.AddOrUpdate(cls.Uri, cls.Name)),
                (carry, r) => (
                    r.Kind == BsddRelationKind.IsEqualTo || r.Kind == BsddRelationKind.IsSynonymOf
                        ? carry.Edges.Add(new SEquatableEdge<string>(cls.Uri, r.RelatedUri))
                        : carry.Edges,
                    r.RelatedName.Length > 0 ? carry.Names.AddOrUpdate(r.RelatedUri, r.RelatedName) : carry.Names)));
        UndirectedGraph<string, SEquatableEdge<string>> graph = edges.ToUndirectedGraph<string, SEquatableEdge<string>>(allowParallelEdges: false);
        var labels = new Dictionary<string, int>();
        graph.ConnectedComponents(labels);
        Map<int, Seq<string>> members = toSeq(labels).Fold(Map<int, Seq<string>>(),
            static (acc, row) => acc.AddOrUpdate(row.Value, Some: existing => existing.Add(row.Key), None: () => Seq(row.Key)));
        return new(
            toSeq(labels).Fold(Map<string, Seq<string>>(), (acc, row) =>
                acc.AddOrUpdate(row.Key, toSeq(members.Find(row.Value).IfNone(Seq(row.Key)).Distinct().OrderBy(identity)))),
            names);
    }

    // The component IS the equivalence set, self included — a URI the closure never saw is its own singleton.
    public Seq<string> EquivalentSet(string classUri) => Equivalence.Find(classUri).IfNone(Seq(classUri));

    public bool Equivalent(string classUriA, string classUriB) =>
        string.Equals(classUriA, classUriB, StringComparison.OrdinalIgnoreCase) || EquivalentSet(classUriA).Contains(classUriB);

    // The cross-standard lowering: an OmniClass-classified value lowers onto its Uniclass peer as a seam value — the
    // source row's ClassUri addresses the closure, the peers under the target's version-free Stem win, and the
    // translated value is edition-unspecified with the resolved concept Title when the closure names it. Empty when the
    // component holds no peer under the target stem (never a wrong lowering), empty for an unrostered source system.
    public Seq<Classification> Translate(Classification classification, ClassificationSystem target, BsddPins pins) =>
        ClassificationSystem.TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            && target.Stem(pins) is { Length: > 0 } stem
            ? toSeq(EquivalentSet(row.ClassUri(classification.Code, pins))
                .Filter(uri => uri.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(identity)
                .Select(uri => Classification.Create(target.Key, ClassificationSystem.TailCode(uri), "", None, None, Names.Find(uri))))
            : Seq<Classification>();
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The bSDD api/Class/v1 wire contract (.api/api-bsdd ClassContract.v1/ClassPropertyContract.v1/ClassRelationContract.v1/
// ClassReferenceContract.v1/HierarchyItemContract.v1): the projection reads ONLY these fields (additionalProperties:false),
// each PascalCase member binding the camelCase wire through the transport's STJ naming policy. The nullable members are
// the optional wire fields — a class fixes a value through PredefinedValue or PropertyValueKind + AllowedValues, narrows
// numerically through Min*/Max*, declares its dimension through the Dimension* exponents, states its relations
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
// resolves the roster row. The contract publishes NO relevance column — the classes[] ORDER is the server's whole
// expression of relevance, which is why the hit carries its ordinal rather than a score.
public sealed record BsddSearchResponse(int TotalCount, int Offset, int Count, SearchClass[]? Classes) {
    public sealed record SearchClass(
        string? DictionaryUri, string? DictionaryName, string Name, string? ReferenceCode, string Uri,
        string? ClassType, string? Description, string? ParentClassName, string[]? RelatedIfcEntityNames);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The Compute transport seam every live bSDD GET rides — ONE generic Fetch the page parameterizes by resource (path +
// query); the transport owns the base URL, the Accept/X-User-Agent headers, and the STJ camelCase decode of TWire. A
// per-resource port member or a transport minted inside Rasm.Bim is the named seam violation
// (Rasm.Compute/Runtime/transport#TRANSPORT_AXIS).
// The return is THREE-state and the page discriminates all three: Succ(Some) is a reached endpoint whose body decoded,
// Succ(None) is an UNREACHED endpoint (the only genuine offline miss, the one state Resolve degrades on), and Fail is a
// reached endpoint whose body did NOT decode — a contract fault the page surfaces. Collapsing the last two into one
// failure is what let a schema drift substitute property-free local evidence for a class the registry answered for.
// The token rides the request so a graph-scale fold can be abandoned BETWEEN round-trips; an already-dispatched request
// runs to its own completion, which is the whole abort grain this seam publishes.
public interface BsddPort {
    Fin<Option<TWire>> Fetch<TWire>(string resource, CancellationToken token);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BsddResolution {
    // Three outcomes, kept distinct at the port rather than inferred from one collapsed failure: an UNREACHED endpoint
    // degrades to the row's local code-shape policy — the gate applied, never a fault for unreachability itself — a
    // reached-but-undecodable body surfaces CodecReject, and a decoded body's own payload verdict is BsddClass.Of's.
    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        port.Fetch<BsddClassResponse>(ClassResource(system.ClassUri(code, pins)), token)
            .MapFail(error => new BimFault.CodecReject(key, $"bsdd-class-undecodable:{error.Message}"))
            .Bind(reached => reached.Match(
                Some: response => BsddClass.Of(response, key),
                None: () => LocalShape(system, code, pins, key)));

    // The dictionary-certified authoring lowering: Resolve -> Admit -> Classify, so a NEW code never lands on an
    // Inactive/superseded class. The admitted class's OWN Code and Name ride onto the minted value — the round-trip is
    // already paid, so discarding the registry's canonical code casing and its concept title would leave the certified
    // value indistinguishable from a bare local one. Offline the shape-gated LocalShape degrade admits (Status defaults
    // Active, Code echoes the caller's) so the row's local shape gates alone — certification tightens when the
    // dictionary answers, never blocks when it cannot.
    public static Fin<Classification> Certify(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        Resolve(system, code, port, pins, token, key)
            .Bind(cls => cls.Admit(key))
            .Bind(admitted => system.Classify(admitted.Code, Optional(admitted.Name).Filter(static n => n.Length > 0), pins, key));

    // Authoring-time concept-to-code resolution (api/Class/Search/v1): a label ("external wall") or an IFC entity scope
    // resolves to candidate BsddHit codes across the scoped dictionaries (the hosted roster rows when scope is empty),
    // each carrying its zero-based position in the response as the server's own relevance rank. No offline concept index
    // exists, so BOTH failure states FAULT typed — an unreached endpoint and an undecodable body alike land CodecReject
    // lifted BARE off key (never a LocalShape degrade, never a raw provider error crossing the entry) — and a hit whose
    // dictionary is unrostered drops (the roster IS the admission; an unrostered code cannot lower onto the seam).
    public static Fin<Seq<BsddHit>> Search(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        port.Fetch<BsddSearchResponse>(SearchResource(text, scope, relatedIfcEntity, pins), token)
            .MapFail(error => new BimFault.CodecReject(key, $"bsdd-search-undecodable:{error.Message}"))
            .Bind(reached => reached.ToFin(new BimFault.CodecReject(key, $"bsdd-search-unreachable:{text}")))
            // The instance indexed Map is VALUE-FIRST (the Seq module's `map` transposes) — the ordinal is the rank.
            .Map(response => toSeq(response.Classes ?? []).Map((hit, index) => HitOf(hit, index, pins)).Somes());

    // The two resource builders the page owns (the transport owns base URL/headers/decode): the class request opts into
    // the relation/children rollups (hierarchy and parentClassReference arrive by default); the search request repeats
    // the DictionaryUris/RelatedIfcEntities keys per the bSDD array-parameter law, an empty scope pinning the HOSTED
    // roster rows server-side — the roster IS the admission, so the page limit is never spent on unrostered
    // dictionaries HitOf would drop anyway.
    static string ClassResource(string classUri) =>
        $"api/Class/v1?Uri={System.Uri.EscapeDataString(classUri)}&IncludeClassProperties=true&IncludeClassRelations=true&IncludeReverseRelations=true&IncludeChildClassReferences=true";

    static string SearchResource(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPins pins) =>
        string.Concat(
            $"api/Class/Search/v1?SearchText={System.Uri.EscapeDataString(text.Trim())}",
            (scope.IsEmpty ? toSeq(ClassificationSystem.Items).Filter(row => row.Hosted(pins)) : scope)
                .Fold("", (acc, row) => $"{acc}&DictionaryUris={System.Uri.EscapeDataString(row.DictionaryUri(pins))}"),
            relatedIfcEntity.Match(Some: static entity => $"&RelatedIfcEntities={System.Uri.EscapeDataString(entity)}", None: static () => ""));

    // The graph-scale ENRICHMENT fold: every entity-type-classified occurrence carrying NO classification in a target
    // system resolves ranked candidate codes — the bSDD IFC-entity alignment widened from the single authoring lookup
    // into the bulk suggestion pass an authoring surface confirms row by row through Certify, so the package produces
    // classifications instead of only validating them.
    // The fold is CLASS-FIRST and refines by name only where it must. One query per entity CLASS is the coarse pass
    // (the entity code is both the search text and the RelatedIfcEntities scope), and a class whose coarse hit set
    // DECIDES — at most one candidate per target — needs nothing more: every occurrence of that class takes the same
    // answer, which is the ordinary shape of a model whose walls all classify alike. Only an AMBIGUOUS class (a target
    // the coarse pass left holding several candidates) re-queries per distinct element NAME, spending the extra
    // round-trips exactly where the name is the discriminant the class was not. Keying every group on the name up
    // front spent a round-trip per naming variant a model happens to carry, and the predefined token never reaches the
    // wire at all, so keying on it split one query into as many identical round-trips as the model had tokens.
    // Server ORDER is the rank (the hit's own ordinal), the roster the admission (HitOf drops an unrostered
    // dictionary), and the transport failure faults by the Search law — enrichment is authoring-time, never the ingest
    // path. The token threads every Fetch, so the fold abandons between round-trips.
    public static Fin<Seq<ClassificationSuggestion>> Suggest(ElementGraph graph, Seq<ClassificationSystem> targets, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        toSeq(graph.ObjectNodes
            .Filter(o => string.Equals(o.Classification.System, ClassificationSystem.IfcSystem.Key, StringComparison.OrdinalIgnoreCase)
                && o.Kind == ObjectKind.Occurrence
                // ANY unclassified target admits the occurrence — a ForAll gate drops a partially classified element
                // whole, so the ordinary as-received model carrying Uniclass but no OmniClass gets suggested nothing.
                && targets.Exists(target => !o.Classifications.Exists(c => string.Equals(c.System, target.Key, StringComparison.OrdinalIgnoreCase))))
            .GroupBy(static o => o.Classification.Code))
            .TraverseM(byClass => Search(byClass.Key, targets, Some(byClass.Key), port, pins, token, key)
                .Bind(coarse => Decisive(coarse, targets)
                    ? Fin.Succ(Rows(toSeq(byClass), targets, coarse))
                    : toSeq(byClass.GroupBy(static o => o.Name is { Length: > 0 } name ? name : o.Classification.Code))
                        .TraverseM(byName => Search(byName.Key, targets, Some(byClass.Key), port, pins, token, key)
                            .Map(refined => Rows(toSeq(byName), targets, refined)))
                        .As()
                        .Map(static grouped => grouped.Flatten())))
            .As()
            .Map(static rows => rows.Flatten().Filter(static row => !row.Candidates.IsEmpty).ToSeq());

    // The coarse pass DECIDES when no target is left holding a choice: the class alone answered, so the name adds
    // nothing and the refinement round-trips are not spent. A target with zero hits is decided too — the class simply
    // has no peer there, and a name query cannot invent one under the same entity scope.
    static bool Decisive(Seq<BsddHit> coarse, Seq<ClassificationSystem> targets) =>
        targets.ForAll(target => coarse.Count(hit => hit.System == target) <= 1);

    // One suggestion row per (element, still-unclassified target) over a resolved hit set — shared by both passes so
    // the coarse and refined legs cannot drift on which targets an element still needs.
    static Seq<ClassificationSuggestion> Rows(Seq<Node.Object> elements, Seq<ClassificationSystem> targets, Seq<BsddHit> hits) =>
        elements.Bind(o => targets
            .Filter(target => !o.Classifications.Exists(c => string.Equals(c.System, target.Key, StringComparison.OrdinalIgnoreCase)))
            .Map(target => new ClassificationSuggestion(o.Id, target, hits.Filter(hit => hit.System == target))));

    // The hit's code authority is the wire's own referenceCode (ClassSearchResponseClassContract.v1 carries it —
    // .api/api-bsdd); the identifier-URI tail is the FALLBACK for a dictionary that omits it, never the primary read.
    // The index is the response array position — the server's relevance order preserved as data.
    static Option<BsddHit> HitOf(BsddSearchResponse.SearchClass hit, int index, BsddPins pins) =>
        hit.Uri is { Length: > 0 }
            ? ClassificationSystem.ByUri(hit.DictionaryUri ?? hit.Uri, pins)
                .Map(system => new BsddHit(
                    system,
                    hit.ReferenceCode is { Length: > 0 } code ? code : ClassificationSystem.TailCode(hit.Uri),
                    hit.Name, hit.Uri, toSeq(hit.RelatedIfcEntityNames ?? []), index))
            : None;

    // The offline degrade IS the row's code-shape policy applied: a shape-passing code admits as property-free Active
    // evidence, a shape-reject faults the SAME UnmappedClass detail Classify mints (one local policy, one fault) —
    // never a fabricated Active evidence for a garbage code the dictionary could not have answered for. The evidence
    // Uri is stem-only because an unreached dictionary pins no version to build a class URI against.
    // The construction stops at Status: every trailing BsddClass member is a LanguageExt Seq or Option whose DEFAULT
    // IS THE EMPTY VALUE, so the degrade states the head it can answer and the absent constraint/relation/containment/
    // supersession surfaces read empty by construction — spelling Seq<T>() into a trailing slot restates the language's
    // own default and invites a reader to infer the omitted slots mean something else.
    static Fin<BsddClass> LocalShape(ClassificationSystem system, string code, BsddPins pins, Op key) =>
        system.Resolve(pins) is var row && row.Shape.IsMatch(code.Trim())
            // ClassType stays EMPTY: it is a four-token registry vocabulary (Class|Material|GroupOfProperties|
            // AlternativeUse) and no dictionary answered, so a stamped "Class" is indistinguishable to a consumer
            // branching on it from a resolved one. Status is stated explicitly — the degrade's own declared policy,
            // never an assumed default.
            ? Fin.Succ(new BsddClass(code, $"{row.Title}:{code}", "", "", $"{row.Stem}/class/{System.Uri.EscapeDataString(code.Trim())}", Seq<BsddProperty>(), BsddStatus.Active))
            : Fin.Fail<BsddClass>(new BimFault.UnmappedClass(key, $"classification-code-reject:{system.Key}:{code}"));
}
```

## [04]-[RESEARCH]

(none)
