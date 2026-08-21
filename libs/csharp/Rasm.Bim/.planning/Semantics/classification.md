# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the seam `Classification` value an `Object` node carries: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/ETIM/IfcClassification), plus a `Project` row whose whole identity arrives as composition data, each row ONE reader answering its `SystemIdentity` (title, version-free `Stem`, hosted version, compiled code shape) out of the composition-supplied `BsddPins` policy, the stem and version deriving the versioned `DictionaryUri` the live resolution and the egress `Location` use, validating a raw code against the system's shape and LOWERING it onto the library-neutral seam `Classification` value — the full-identity factory `(System, Code, Edition, Source, EditionDate, Title)`: the `(System, Code, Edition)` edition-scoped IDENTITY plus the equality-excluded resolved-annotation bundle. The seam owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (no roster, no dictionary, no regex); this page is the downstream Bim projector the seam names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto the seam value at ingest and re-authoring that value at egress. A classification is the seam `Classification` VALUE on an `Object` node: `Relations/relation#EDGE_ALGEBRA` is explicit that classification is a value on the node, NOT an edge (the neutral `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the retired `(GlobalId, system, code)` triple bound to a second stored `BimElement` record is GONE — a query reads `node.Classification.System`/`Code`, never a stringly-keyed lookup against a second element store.

The round-trip is BIDIRECTIONAL across three entries: `Classify` lowers a validated raw code at authoring ingress, `Ingest` resolves an imported `IfcClassificationReference` back to the seam value at import ingress (the inverse of `Author`, the leg the migration source never had), and `Author` re-authors a node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress — the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node, which REPLACES the retired `Rasm.Materials` `MaterialPropertyWire.Classification` half (a material carries no classification; the `Object` node does): the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands a substance's standard `(system, code)` as the bound element's `Object`-node `Classification` value through its `MaterialBinding` egress, so the element classification this owner round-trips is the one the unified Component projection authored, never a material-wire field. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping carries the FULL `ClassContract.v1` surface: the class-scoped constraint surface (IFC `DataType` + Pset placement, `ValueKind`, allowed values, XSD `Pattern`, numeric `Bounds`, the SI base-dimension exponents the seam `Dimension` carries + `Units`, `Status`) that feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template, the `Review/validation#IDS_FACETS` Classification + Property facets, and the seam `Properties/quantity#MEASURE_VALUE` UnitsNet coercion directly, PLUS the relation set (`classRelations`/`reverseClassRelations` → typed `BsddRelation` rows whose `IsEqualTo`/`IsSynonymOf` edges the `BsddFederation` receipt closes through the shared QuikGraph substrate into cross-standard equivalence — `Translate` lowers an OmniClass code onto its Uniclass peer, a capability unreachable from any code string), the AUTHORITATIVE containment (`parentClassReference`/`hierarchy`/`childClassReferences` → `BsddRef` — the parent a MasterFormat/Uniformat code string does not encode), and the supersession surface (`status`/`replacedObjectCodes`/`replacingObjectCodes`/`deprecationExplanation` — `BsddClass.Admit` refuses to certify a NEW code onto an `Inactive` class, carrying the replacing code in the fault). `BsddResolution.Certify` is the dictionary-certified authoring lowering (`Resolve` → `Admit` → `Classify`, the resolved class's own code and title carried onto the minted value) and `BsddResolution.Search` resolves a concept label or IFC entity to candidate codes (`api/Class/Search/v1`) — so a new standard is one `ClassificationSystem` dictionary-identity row shared across `classification`, `properties`, and `validation`. The typed `Model/faults#FAULT_BAND` `BimFault` cases lift BARE onto the `Fin` rail (band 2600 owns the generated `Code`; no `.ToError()` hop), each carrying the kernel `Op` operation context the caller threads.

## [01]-[INDEX]

- [02]-[CLASSIFICATION_AXIS]: `BsddPins` the composition-supplied hosted-version policy; `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary stem, version pin reader, code-shape policy); `Classify(code, title, key)` gating a raw code on the row's shape and handing it to the seam's ONE railed `Classification.Of` admission at authoring ingress; `Ingest(reference, pins, key)` resolving an imported `IfcClassificationReference` back onto a seam value at import ingress; and `Author(db, related, classification, pins)` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` over the shared `Semantics/composition#EGRESS` `EmitMemo` dictionary-source memo (the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes).
- [03]-[BSDD_RESOLUTION]: the live bSDD dictionary resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, projecting the FULL `ClassContract.v1` surface — the constraint surface (`ValueKind`/`AllowedValues`/`Pattern`/`Bounds`/`SiDimension`/`Units`), the relation set (`BsddRelation` forward + reverse), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`) — degrading to the row's local code-shape policy ONLY when the endpoint was unreached; `BsddWire` the `[Mapper]` boundary transcription owning the rail-free wire rows; `BsddResolution.Certify` the supersession-gated authoring lowering, `BsddResolution.Search` the `api/Class/Search/v1` concept-to-code resolution carrying each hit's server-order `Rank`, and `BsddFederation` the QuikGraph connected-component equivalence receipt whose `Translate` lowers a code across standards onto the seam value; feeding the `Semantics/properties#PROPERTY_TEMPLATES` template, the IDS facets, and the seam `MeasureValue` coercion; the `Suggest` graph-scale enrichment fold returning ranked `ClassificationSuggestion` candidates per unclassified element and target system.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row ONE `Func<BsddPins, SystemIdentity>` reader answering the four facts a dictionary address and a code gate need — the display `Title`, the version-free `Stem` (the ingest prefix identity a foreign-edition `Location` still matches), the hosted `Version`, and the compiled code shape — the stem and version deriving the versioned `DictionaryUri` (the bSDD identifier scheme is `{org}/{dictionary}/{version}`; a versionless class URI does not resolve, so the version-bearing row IS the live-lane admission and an unhosted system reads a blank version so its live leg degrades by construction); `SystemIdentity` that four-fact carrier and `BsddPins` the ONE policy record supplying the hosted version tokens plus the `Custom` project-system identity the `Project` row reads WHOLE, so a durable standard freezes its identity at declaration while a registry-republished version and a client asset-code scheme alike stay values a composition overrides. The projector lowers a resolved standard code onto the seam `Classification/classification#CLASSIFICATION_AXIS` `Classification` value the `Object` node carries and authors it back at egress. The seam `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the seam defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the seam value, never re-declaring a classification value-object beside the seam.
- Entry: `ClassificationSystem.Classify(string code, Option<string> title, BsddPins pins, Op key)` validates the raw code against the system's code-shape regex and lowers it onto a seam `Classification(Key, code, "", None, None, title)` value — the `title` the resolved concept name where a caller HAS one (the `Certify` lane carries the dictionary class's own) and `None` on the bare authoring path, so one entry spans both without a mode flag; `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Unmapped`, the typed case lifting BARE off `key`, no `.ToError()` hop); `ClassificationSystem.Ingest(IfcClassificationReference reference, BsddPins pins, Op key)` is the import-ingress inverse resolving the standard system off the reference's `ReferencedSource` root dictionary title, the root's `Specification` dictionary-URI prefix, OR its `Location` identifier-URI prefix against the roster (the code off `Identification` or the trailing `Location` segment, the resolved `Title` off the reference's own `Name`), returning `Fin<Option<Classification>>` the `Projection/semantic#SEMANTIC_PROJECTOR` ingress accumulates onto the `Graph/element#NODE_MODEL` `Object` node's `Classifications` set (IFC admits MULTIPLE `IfcRelAssociatesClassification` per object) — `Succ(None)` for an unrostered source or a code-free reference so a foreign system rides the `Projection/relations#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering, `Fail` for the seam's own blank-token refusal a collapsed `None` masks as a foreign system; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification, BsddPins pins)` is the egress entry the `Emit` composes per `Object` node — `None` for the `IfcSystem.Key` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system's edition-scoped dictionary, `Identification` the code, `Location` the identifier URI, and `Name` the seam `Title` — the members `Ingest` reads back, so the full identity survives the egress leg.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the actual shape enforcement the migration source's decorative `ClassificationCode` never applied) and, on a match, hands the pair to the seam's ONE railed admission `Classification.Of(Key, code, key, title: title)` — the `[ComplexValueObject]` entry normalizing the system token, trimming the code, and accumulating its two frozen blank-token gates under the caller's `Op`, the edition-unspecified `Edition ""` and the `Source`/`EditionDate` annotations absent on every ingress path since neither a raw code nor a dictionary class carries a publisher edition — so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30", "")` and a code either gate rejects faults rather than lowering a malformed value; the containment hierarchy is the bSDD `Parent`/`Ancestry`/`Children` evidence the `[03]-[BSDD_RESOLUTION]` owner resolves, never a seam-side code-shape derivation. `Ingest` walks `RootSource` to the root `IfcClassification` ONCE: it matches a roster row by the dictionary `Name` equality, the root's `Specification` dictionary-URI through the shared `ByUri` version-free `Stem` prefix (the URI `Author` itself stamps, so the round-trip self-resolves even when a re-export strips per-reference `Location`s), or the reference `Location` prefix (a `Location` minted under ANY dictionary edition — a foreign 2015-edition reference, a future `ifc/4.6` class URI — still resolves its row), reads the code off `CodeOf` (`TailCode`-unescaping the `Location` trailing segment when the `Identification` is absent), and lowers the EDITION-SCOPED annotation bundle off that SAME root — the `Edition` token (IDENTITY on the seam), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed) — while `TitleOf` reads the reference's OWN `IfcClassificationReference.Name` (distinct from the root dictionary `Name`) as the resolved concept `Title`, so the imported leg lands the FULL identity (edition + publisher + date + title) at the only path that can populate it rather than a perpetually edition-blank, title-`None` reference. `Author` resolves the row through the generated `TryGet` and authors the reference with the URI-escaped `Location` (`ClassUri`) so an OmniClass/MasterFormat code carrying spaces round-trips, the reference `Name` carrying the seam `Title` and the `(db, system, edition)`-keyed dictionary memo re-stamping the value's `Edition`/`Source`/`EditionDate` — the egress leg of the same identity `Ingest` lowers; the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row whose reader closes over its frozen `SystemIdentity`; a dictionary the public bSDD starts hosting is one version column on `BsddPins` plus the row's reader, never a shape edit; a PROJECT-OWN system is a `SystemIdentity` value on `BsddPins.Custom` with no roster edit at all, the `Project` row already reading it; a registry that re-publishes a dictionary version is one value the composition passes, never a durable-page edit; the seam `Classification` value-object absorbs any `(system, code)` pair with no seam edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type, never a parallel classification value-object beside the seam, and never a per-direction resolver (one `Classify`/`Ingest`/`Author` triad spans the whole round-trip).
- Boundary: the classification systems are ONE keyed axis (`ClassificationSystem` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the classification VALUE is the seam `Classification` `[ComplexValueObject]` and a Bim `Classification`/`ClassificationCode`/`ClassificationRef` value-object is the deleted form — the seam owns the typed pair, this page owns the standard-systems roster and lowers onto it, so the type name `ClassificationSystem` never collides with the seam `Classification`; `Classification.Of` is the seam's ONE admission and every mint on this page composes it under the caller's `Op` — a throwing `Classification.Create`/`TryCreate`/`Validate` spelling is the deleted form, and so is a seam-side containment or crosswalk read (`Parent`, `Within`, `Ancestors`, `TranslateTo`), the authoritative hierarchy and the cross-standard equivalence both being `[03]-[BSDD_RESOLUTION]` evidence; the hosted VERSION tokens and the project-own system identity are composition-supplied `BsddPins` values, and a version literal or a client asset-code scheme frozen into a durable roster row is the deleted form — a token the registry re-publishes rots into a `404` the offline degrade then masks as unreachability, so it lives at the one overridable value whose defaults serve an unconfigured composition; the `Classify(BimElement element, …)` binding to a `BimElement.GlobalId` is GONE (the `BimElement`/`BimModel` are retired, the consumer element being the `Graph/element#ELEMENT_GRAPH` `Bake` fold) — a classification is the seam value on the `Object` node, never a `(GlobalId, system, code)` triple keyed to a second element record; classification is a VALUE on the `Object` node and NOT an edge (the seam `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the egress reads the node `Classification` value and a classification-association `Relationship` case is the deleted form; the code shape is the row's regex validated once at `Classify`, never a per-call regex at the call site; every comparison against a system key runs `OrdinalIgnoreCase` — the key space the row's own `[KeyMemberEqualityComparer]` declares — so a raw `!=`/`==` on a system token is the deleted form that reads `"IFC"` and `"ifc"` as different systems, and a bare `"ifc"` literal where `IfcSystem.Key` names the same token is the deleted form; the typed `BimFault` lifts BARE off the threaded `Op key` and a `.ToError()` hop or a single-string fault ctor is the named defect this owner closes (the band owns the generated `Code`); the bSDD dictionary is the authoritative live source for the class-to-property constraint surface resolved through the dictionary URI, never a hardcoded code-to-property table that duplicates and drifts from it; the per-system `CodeShape` regex is BOTH the cheap LOCAL shape gate `Classify` admits a raw authoring code through (no network round-trip) AND the offline degradation `BsddResolution.LocalShape` falls back to, never that drifting constraint table; the classification round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`.api/api-geometrygym-ifc`) consumed as settled vocabulary, the egress carrying `Identification` + `Location` (+ the `Name` concept title the seam `Classification.Title` round-trips) so the import `Ingest` reconstructs the seam value losslessly, never re-minting a classification mapping; the db-scoped dictionary memo composes the `Semantics/composition#EGRESS` `EmitMemo` owner and a second `ConditionalWeakTable` declared here is the deleted duplicate; the egress reads the seam `Object` node `Classification`, NOT a Materials `MaterialPropertyWire.Classification` carrier (retired), the material-wire classification half having moved to this element-classification egress.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using System.Text.RegularExpressions;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm.Bim;
using Rasm.Bim.Projection;
using Rasm.Element.Classification;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries

namespace Rasm.Bim.Semantics;

// --- [TYPES] ------------------------------------------------------------------------------
// ONE carrier rather than four row columns because the Custom row resolves every fact from composition data, so a
// per-fact reader would multiply each row's shape by the number of facts a project system supplies. The Shape
// initializer compiles ONCE per value (a record property initializer reads the primary-ctor parameter), so a policy
// value built at composition carries a compiled NonBacktracking matcher rather than parsing per call.
public sealed record SystemIdentity(string Title, string Stem, string Version, string Pattern) {
    public static readonly SystemIdentity Unclaimed = new("", "", "", @"^(?!)$");   // matches nothing: an unconfigured project system admits no code

    public Regex Shape { get; } = new(Pattern, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);
}

// The bSDD identifier scheme is {org}/{dictionary}/{version} and a versionless class URI does not resolve, so a
// live-hosted row must pin a version — yet a version is a LIVE registry fact re-published on the registry's own
// cadence, and a token frozen into a durable roster rots into a 404 the offline degrade then masks as plain
// unreachability. Pinning them here keeps that rot at ONE overridable value whose defaults serve an unconfigured
// composition. Unclaimed's shape admits nothing, so an unconfigured composition never silently classifies onto it.
public sealed record BsddPins(string Ifc, string Etim, SystemIdentity Custom) {
    public static readonly BsddPins Default = new(Ifc: "4.3", Etim: "10.0", Custom: SystemIdentity.Unclaimed);
}

// The type name ClassificationSystem is distinct from the seam Classification so the seam value-object stays the one
// canonical classification; this page owns only the roster and the bSDD lane.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClassificationSystem {
    // ifc/etim are hosted on the public bSDD; uniclass2015/omniclass/masterformat/uniformat are unhosted, so their
    // blank version degrades the live leg to the local shape BY CONSTRUCTION and hosting later is one BsddPins column.
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

    // The reader closes over ONE SystemIdentity compiled at type init rather than per resolution. A Uniclass table
    // prefix is two letters with the second upper OR lower (Ss/Pr/EF/SL/TE), so [A-Z][A-Za-z] admits every real
    // prefix, and NonBacktracking exposes no backtracking surface to hostile input.
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

    // Two gates in series and neither is redundant: this row's regex judges the code SHAPE its own standard declares,
    // and Classification.Of judges the blank tokens the seam freezes. Both paths are edition-UNSPECIFIED, because
    // neither a raw code nor a dictionary class carries a publisher edition; that is an Ingest concern.
    public Fin<Classification> Classify(string code, Option<string> title, BsddPins pins, Op key) =>
        CodeShape(pins).IsMatch(code.Trim())
            ? Classification.Of(Key, code, key, title: title)
            : Fin.Fail<Classification>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-code-reject", Key, code })));

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

    // Specification is the IFC4X3 rename of the retired dictionary-level Location and is the very URI Author stamps,
    // so a re-export that strips per-reference Locations still self-resolves and a third-party export carrying only
    // the dictionary-level URI resolves too. The EDITION-SCOPED bundle lowers off that SAME root
    // (IfcClassification.Source/Edition/EditionDate decompile-confirmed, .api/api-geometrygym-ifc row 08), because
    // Edition is IDENTITY on the seam (§EDITION_SCOPING) and this is the only ingest path that can populate it.
    // The return is THREE-state: Succ(None) an unrostered source or a code-free reference, Fail the seam's own
    // blank-token refusal — which a collapsed Succ(None) would have masked as "foreign system".
    public static Fin<Option<Classification>> Ingest(IfcClassificationReference reference, BsddPins pins, Op key) {
        IfcClassification? dictionary = RootSource(reference);
        return (Optional(Items.FirstOrDefault(row => row.Title(pins) is { Length: > 0 } title && string.Equals(title, PropertyLowering.Stated(dictionary?.Name).IfNone(""), StringComparison.OrdinalIgnoreCase)))
            | ByUri(PropertyLowering.Stated(dictionary?.Specification).IfNone(""), pins)
            | ByUri(PropertyLowering.Stated(reference.Location).IfNone(""), pins))
            .Bind(system => Optional(CodeOf(reference)).Filter(static code => code.Length > 0).Map(code => (system, code)))
            .Match(
                Some: found => Classification.Of(
                    found.system.Key, found.code, key,
                    edition: PropertyLowering.Stated(dictionary?.Edition).IfNone("").Trim(),
                    source: SourceOf(dictionary),
                    editionDate: EditionDateOf(dictionary),
                    title: TitleOf(reference)).Map(Some),
                None: static () => Fin.Succ(Option<Classification>.None));
    }

    // A nested IfcClassificationReference (the IFC hierarchical-classification pattern) points its ReferencedSource at
    // a PARENT reference, not the dictionary, so a flat `as IfcClassification` reads "" on a nested ref and silently
    // misses the dictionary. The depth bound makes a malformed cyclic chain terminate rather than spin.
    static IfcClassification? RootSource(IfcClassificationReference reference) {
        IfcClassificationReferenceSelect? source = reference.ReferencedSource;
        for (int depth = 0; source is IfcClassificationReference parent && depth < 32; depth++) {
            source = parent.ReferencedSource;
        }
        return source as IfcClassification;
    }

    // The GeometryGym unset-date sentinel is DateTime.MinValue, so an undated dictionary must lower None rather than a
    // spurious 0001-01-01.
    static Option<LocalDate> EditionDateOf(IfcClassification? dictionary) =>
        dictionary is { EditionDate: var date } && date > System.DateTime.MinValue
            ? Some(LocalDate.FromDateTime(date))
            : None;

    static Option<string> SourceOf(IfcClassification? dictionary) =>
        PropertyLowering.Stated(dictionary?.Source).Map(static source => source.Trim());

    static string CodeOf(IfcClassificationReference reference) =>
        (PropertyLowering.Stated(reference.Identification)
            | PropertyLowering.Stated(reference.Location).Map(TailCode)).IfNone("");

    // IfcClassificationReference.Name is the classified concept's resolved name (decompile-confirmed `public string
    // Name` on the GeometryGym IfcExternalReference base — .api/api-geometrygym-ifc row 07), DISTINCT from the root
    // dictionary Name; this is the only ingest path that can populate the seam Title at all.
    static Option<string> TitleOf(IfcClassificationReference reference) =>
        PropertyLowering.Stated(reference.Name).Map(static name => name.Trim());

    // The key carries EDITION because an IfcClassification IS an edition-scoped dictionary and the seam Edition is
    // IDENTITY: one shared entity strips the edition and forks every re-ingested content key. So N objects under one
    // Uniclass edition author ONE dictionary entity, and a 2015- and a 2023-edition value author two.
    static readonly EmitMemo<(string System, string Edition), IfcClassification> Sources = new();

    // The Edition/Source/EditionDate setters are decompile-confirmed, so Ingest reads back the SAME identity Author
    // lowered; an edition-unspecified value authors the bare dictionary.
    static IfcClassification Source(DatabaseIfc db, ClassificationSystem row, Classification classification, BsddPins pins) =>
        Sources.Of(db, (row.Key, classification.Edition), _ => {
            var dictionary = new IfcClassification(db, row.Title(pins)) { Specification = row.DictionaryUri(pins), Edition = classification.Edition };
            classification.Source.IfSome(publisher => dictionary.Source = publisher);
            classification.EditionDate.IfSome(date => dictionary.EditionDate = date.ToDateTimeUnspecified());
            return dictionary;
        });

    // The system compare runs OrdinalIgnoreCase because that IS the roster's declared key space — a raw != reads "IFC"
    // and "ifc" as different systems and authors a duplicate entity-type reference. The IfcRelAssociatesClassification
    // (IfcClassificationSelect, IfcDefinitionSelect) + IfcClassificationReference(db){ReferencedSource,Identification,
    // Location,Name} + IfcClassification(db, name){Specification,Edition} ctor surface is decompile-confirmed
    // (.api/api-geometrygym-ifc rows 07/08; every stamped member a settable attribute).
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

- Owner: `BsddResolution` the live bSDD dictionary resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the endpoint was UNREACHED so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence carrying the FULL `ClassContract.v1` surface — the class-scoped constraint surface the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve` template, the IDS facets, and the seam `MeasureValue` coercion read, the forward + reverse relation set (`BsddRelation`), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows — the parent a MasterFormat/Uniformat code string does not encode and the seam carries no derivation for, the dictionary being the ONLY source that states it), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`); `BsddWire` the `[Mapper]` boundary transcription owning the rail-free wire-row crossings; `BsddFederation` the cross-standard equivalence receipt over the relation graph.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key)` resolves the full dictionary-class evidence over Compute's transport — `Fin<T>` returns the resolved `BsddClass` on a live hit; an UNREACHED endpoint degrades to the row's local code-shape policy APPLIED (`LocalShape` — a shape-passing code admits as property-free `Active` evidence, a shape-reject faults `BimFault.Refused` with `BimReason.Unmapped` with the same `classification-code-reject` detail `Classify` mints; unreachability itself never faults), while a REACHED endpoint whose body the decoder rejected retains the port's exact `Error` — the two are distinct states on the port's own return, never inferred from one collapsed failure. `BsddResolution.Certify(system, code, port, pins, token, key)` is the dictionary-certified authoring lowering composing `Resolve` → `BsddClass.Admit` → `Classify`, so a NEW code never lands on an `Inactive`/superseded class AND the certified value carries the dictionary's OWN code and concept title rather than discarding the evidence it just paid a round-trip for (offline the shape-gated degrade admits and the row's local shape gates alone — certification tightens when the dictionary answers, never blocks when it cannot). `BsddResolution.Search(text, scope, relatedIfcEntity, port, pins, token, key)` resolves a concept label or IFC entity to candidate `BsddHit` codes over `api/Class/Search/v1`, each hit carrying its server-order `Rank` — authoring-time, never the ingest path, no offline concept index exists, so an unreached endpoint faults `BimFault.Refused` with `BimReason.Codec` while a port failure retains its exact `Error`. `BsddResolution.Suggest(ElementGraph graph, Seq<ClassificationSystem> targets, BsddPort port, BsddPins pins, CancellationToken token, Op key)` is the graph-scale ENRICHMENT fold — every entity-type-classified occurrence with no classification in a target system resolves ranked candidate codes once per distinct QUERY key through the same `Search` wire (server order the rank, the roster the admission), returning per-element `ClassificationSuggestion` rows an authoring surface confirms in bulk through `Certify` — the as-received unclassified model gains its mandated Uniclass/OmniClass coverage without per-element hand lookup, the difference between a checker and an authoring assistant. `BsddFederation.Of(classes)` folds resolved evidence into the equivalence closure; `Translate(classification, target, pins, key)` lowers a code across standards onto the seam value over `Classification.Of`, returning `Fin<Seq<Classification>>` so a peer URI the seam refuses rails instead of vanishing into the same empty sequence an unrostered source yields; `Equivalent`/`EquivalentSet` answer the pairwise and set queries. EVERY live hop crosses the one `Fetch` member under the declared `RedrivePolicy Registry` — `RedrivePolicy.Of(Schedule.exponential | Schedule.maxDelay, bound)` composed through kernel `Redrive.Run` — so a transient registry refusal re-drives on the kernel curve instead of degrading a reachable dictionary to the offline shape gate.
- Auto: `Resolve` builds the class request from `ClassificationSystem.ClassUri(code, pins)` with `IncludeClassProperties`/`IncludeClassRelations`/`IncludeReverseRelations`/`IncludeChildClassReferences` (hierarchy and `parentClassReference` arrive by default), issues it over the injected `BsddPort` (the Compute transport seam, ONE generic `Fetch<TWire>` — the page owns the resource query, the transport owns base URL, headers, the STJ camelCase decode, and the reached-versus-unreached discrimination), and projects the wire `BsddClassResponse` into the `BsddClass` evidence through `BsddClass.Of`: each `ClassProperty` projects through `Property` into a `BsddProperty` carrying the IFC `DataType`/Pset placement (the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template), the `ValueKind`/`AllowedValues`/`Pattern`/`Bounds` value constraints (the `Review/validation#IDS_FACETS` `ValueConstraint`), the seam `Dimension` built directly from the wire's SI exponent columns + `Units` (the seam `Properties/quantity#DIMENSION` owner's own generated factory, never a page-local exponent record beside it), and the `Status` (the IDS admission gate); each `ClassRelation` projects through `RelationOf` into a typed `BsddRelation`; the containment pointers project through the `BsddWire` mapper's generated `Ref` transcriptions. `BsddFederation.Of` folds the `IsEqualTo`/`IsSynonymOf` rows (forward AND reverse — an inbound equivalence declared by the OTHER dictionary counts) into a transient `UndirectedGraph<string, SEquatableEdge<string>>` ONE edge per relation, labels it through `AlgorithmExtensions.ConnectedComponents`, and projects each component whole onto every member URI — equivalence under a symmetric relation IS connected-component membership, so the linear component pass replaces the quadratic transitive-closure edge set and its per-source `GroupBy` re-projection; the QuikGraph fold is transient inside `Of`, the receipt carries data only. A transport miss degrades to `LocalShape` so a new standard becomes a dictionary-identity row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference. `Suggest` groups ONLY on the axes the query carries — the entity-class code and the search text — because the predefined token never reaches the request, and a group key carrying it split one query into N identical round-trips; the fold threads the caller's `CancellationToken` into every `Fetch`, so the abort grain is one in-flight request boundary and a request already dispatched runs to its own completion.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_TEMPLATES` owner (`PropertyKey.Resolve(cls, predefined, schema, scope, Option<BsddClass>)` unioning the `BsddClass.Properties` dictionary rows OVER the offline `Xbim.Properties` catalogue floor under the caller's `TemplateScope` definition set, dictionary-wins), the IDS Classification + Property facets (the class URI is the facet value; `Pattern`/`Bounds`/`AllowedValues` narrow into an `Xbim.InformationSpecifications` `ValueConstraint`; `Status` gates admission), and the seam `MeasureValue` (a dimensioned property's `SiDimension` IS the seam `Dimension` the `UnitsNet` `BaseDimensions` projection reads); the `BsddFederation` receipt is the cross-standard translation evidence a multi-standard deliverable reads (`Translate` an OmniClass-classified model onto its Uniclass peers), and `Ancestry` is the authoritative inheritance chain a facet `partOf`/rollup read walks; each `BsddHit.Rank` is the server's own result ordinal, so a consumer re-ranks against the relevance the registry expressed rather than against a score the wire does not publish — so a new dictionary is one identity row across all consumers.
- Law: requiredness is a CAPABILITY, not a flag — `BsddProperty.Traits` is a `CapabilitySet<TemplateTrait>` over the kernel `Domain/validation#CAPABILITY` carrier, `Declared` holding whenever a source answered at all and `Required` joining it when that source demanded the property, so the EMPTY set is the third state (the offline buildingSMART floor states no requiredness column) a `bool` asserted "optional" over and an `Option<bool>` carried only by wrapping one. A read of the axis folds out for absence through `Admits`, which the kernel carrier declares lawful; the `Require` refusal twin has no site on these two pages, because the only presence consumer is `Semantics/properties#TEMPLATE_AUDIT`, whose product is a `TemplateVerdict` row rather than a fault. The bSDD WIRE keeps its `bool IsRequired` column verbatim — the `additionalProperties:false` contract declares it, and re-shaping a wire member to match a domain carrier forks the transcription.
- Law: the live lane's re-drive is the kernel's, not a hand loop — one `RedrivePolicy` value on this owner and one `Fetch` member every resource crosses, so the page declares CADENCE and BOUND and classifies nothing: `Redrive.Settle` reads the `Expected` case's own `Retriability`, so a Terminal registry refusal never re-drives. The port's `Fin` lane lifts ONTO the `IO` rail before `Redrive` sees it, because a `Fin.Fail` carried inside a lifted `IO` is a successful `IO` holding a failure the curve never re-drives.
- Packages: Rasm.Element, LanguageExt.Core, QuikGraph, Thinktecture.Runtime.Extensions, Generator.Equals, Riok.Mapperly, Rasm, NodaTime
- Growth: a new bSDD dictionary is one `ClassificationSystem` row (stem + pin reader + shape) plus its `BsddPins` column; the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; a new dictionary-declared constraint is one read field on the `BsddClassResponse.ClassProperty` wire projected through `Property`, a new relation kind one `BsddRelationKind` member the closure filter reads, a new resource one query builder on `BsddResolution` (never a port member); a new rail-free wire row is one `BsddWire` partial, never a hand-rolled transcription beside it; a newly declared template axis is one `TemplateTrait` row with no signature edit anywhere; a lane whose registry publishes a different cadence is the one `RedrivePolicy` value, never a second retry owner — never a parallel evidence record, never a per-system classifier, and never a `Rasm.Persistence` reference; federation growth is more resolved evidence folded into `Of`, zero type edits.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreached-endpoint degradation only; the port returns a THREE-state outcome (decoded body / unreached endpoint / undecodable body) and collapsing the last two into one failure is the deleted form that masks a contract drift as an offline miss and silently substitutes property-free local evidence for a class the registry answered for; the `BsddClass.Of` projection reads ONLY the fields the `.api/api-bsdd` catalog enumerates (the wire is `additionalProperties:false`, so an unexpected member signals contract drift, not a capability) and a field absent from the catalog is a phantom — the search wire publishes NO numeric relevance, so the hit's server-order ORDINAL is the rank column and a fabricated score is the deleted form; the SI exponent vector is the seam `Properties/quantity#DIMENSION` `Dimension` built through its own generated factory, and a page-local seven-int exponent record beside it is the deleted duplicate that forced every consumer to re-project one concept twice; the class-level constraint (`ClassProperty.AllowedValues`/`Min*`/`Max*`/`Pattern`) is read, never silently the property master, so a class that narrows an enumeration is honored; the cross-standard equivalence closure folds the `IsEqualTo`/`IsSynonymOf` relations through the shared `QuikGraph` substrate the folder admits — the relation is SYMMETRIC, so the container is undirected and the answer is `ConnectedComponents`, where a directed double-edge fold plus `ComputeTransitiveClosure` materialized the whole quadratic reachability edge set to recover a partition the linear pass yields directly; a hand-rolled BFS/union-find over a `Map<>` adjacency is the named rejected form (`.api/api-quikgraph`); the authoritative containment (`parentClassReference`/`hierarchy`) is read for a code that does NOT encode its parent (MasterFormat/Uniformat), and re-deriving containment from the code string where the dictionary states it is the rejected form; supersession gates authoring — `BsddClass.Admit` refuses a NEW code onto an `Inactive` class carrying the `ReplacedBy` code, and silently authoring a superseded class is the named defect; a `Certify` that resolves a class and then discards its code and concept title is the deleted form — the round-trip is paid, so the certified value carries the dictionary's own identity; the rail-free wire rows transcribe through the ONE `BsddWire` `[Mapper]` and a hand-written `RefOf`/`AllowedValue` projection beside it is the deleted form, while every crossing that carries a `Fin` admission (`ValueKindOf`/`StatusOf`/`RelationOf`/`BoundsOf`) stays hand-written by law — Mapperly transcribes shape, never a lane; that same mapper owns the ONE wire absence admission — `Text` for a nullable string, `Rows` for a nullable array — so a `?? ""` or `?? []` at any projection below it is the deleted duplicate, and `Rows` is registered `Default = false` because a GENERIC mapping Mapperly may choose is the RMG001 form the folder refuses; the live fetch rides the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport injected as `BsddPort` (ONE generic `Fetch<TWire>` the page parameterizes by resource — a per-resource port member is the rejected form) and a transport minted here is the named seam violation; the port carries the caller's `CancellationToken` and the abort grain is DECLARED — one in-flight request boundary, a dispatched request running to its own completion — so a graph-scale fold that cannot be abandoned between round-trips is the deleted form and an unqualified cancellable claim over the request itself is the overclaim; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the enrichment fold groups on the QUERY axes ALONE and a group key carrying an axis the request never sends is the deleted form that multiplies one round-trip by the cardinality of a token the server never sees; a bare system-token literal where `IfcSystem.Key` names the same value is the deleted form, and every system comparison runs `OrdinalIgnoreCase` per the roster's declared key space; the resolution degrades to the local policy on an unreached endpoint so INGEST never blocks (faulting on unreachability itself is the named defect) while the degraded verdict IS the row's shape gate — a shape-rejected code faults `BimFault.Refused` with `BimReason.Unmapped`, never a fabricated `Active` evidence the dictionary did not answer — and `Search` is authoring-only, so an unreached endpoint faults `BimFault.Refused` with `BimReason.Codec` because no offline concept-to-code resolution exists; port failures retain the exact `Error` because the compact Bim boundary axis declares no bSDD wrapper.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using QuikGraph;                                      // the shared graph substrate the BsddFederation equivalence fold runs on (never a hand-rolled BFS)
using QuikGraph.Algorithms;
using Riok.Mapperly.Abstractions;
using Rasm.Domain;                                    // RedrivePolicy/Redrive/Schedule — the kernel re-drive law the REST lane composes
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

// Declared holds whenever a source answered at all and Required joins it when that source demanded the property, so
// the EMPTY set is the third state: the offline buildingSMART floor states no requiredness column, and a `false` there
// asserted "optional" on a silent source's behalf, which made a presence audit read every unstated property as
// satisfied. A second declared axis lands as one row with no signature moving.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TemplateTrait : ICapability<TemplateTrait> {
    public static readonly TemplateTrait Declared = new("declared", rank: 0);
    public static readonly TemplateTrait Required = new("required", rank: 1);

    public int Rank { get; }

    private TemplateTrait(string key, int rank) : this(key) => Rank = rank;
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
// A class-level bound may be STRICTER than the property master's (.api/api-bsdd min*/max*).
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

// Rank is the hit's ZERO-BASED position in the response's own classes[] array. The search wire publishes NO numeric
// relevance column (the contract is additionalProperties:false and carries none), so the server's ORDER is the whole
// relevance signal it expresses and a fabricated score would be a phantom the catalog refutes.
public readonly record struct BsddHit(ClassificationSystem System, string Code, string Name, string Uri, Seq<string> RelatedIfcEntities, int Rank);

// Each accepted candidate lowers through Certify, so the confirmation path IS the standing dictionary-certified
// authoring lowering and never a second admission. Candidates stay in the server's Rank order.
public readonly record struct ClassificationSuggestion(NodeId Element, ClassificationSystem System, Seq<BsddHit> Candidates);

// SiDimension IS the seam Dimension, built from the wire's exponent columns, because the dictionary's seven integers
// and the seam's dimension are ONE concept. Traits, ValueKind and Status are REQUIRED head members on two grounds: a
// [SmartEnum] row is a static readonly field, not a constant, so it cannot be an optional-parameter default at all;
// and a defaulted Active is exactly the fabricated evidence this owner's boundary refuses. The registry reorders the
// collection members freely between reads, so each compares as a SET and an ordered comparison reports drift the
// dictionary never made.
[Equatable]
public sealed partial record BsddProperty(
    string Code, string Name, string DataType, string PropertySet, string PredefinedValue,
    CapabilitySet<TemplateTrait> Traits,
    BsddValueKind ValueKind,
    BsddStatus Status,
    [property: UnorderedEquality] Seq<BsddAllowedValue> AllowedValues = default,
    Option<string> Pattern = default,
    Option<BsddBounds> Bounds = default,
    Option<Dimension> SiDimension = default,
    [property: UnorderedEquality] Seq<string> Units = default);

// Every server-ordered member compares as a MULTISET because the registry reorders them freely between reads and an
// ordered comparison would report drift the dictionary never made — except Ancestry, which is ORDER-BEARING by
// construction (the wire level sorts it root-first, so a reordered chain IS a different inheritance path).
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
    // A live hit returning a class with no Code/Uri is INVALID published data, not an offline dictionary miss, so it
    // faults rather than being masked as a LocalShape miss. The unreached-endpoint degradation is Resolve's concern;
    // this projection judges payload shape alone.
    public static Fin<BsddClass> Of(BsddClassResponse response, Op key) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-class-malformed", response.Uri })))
            : from status in StatusOf(response.Status, key)
              from properties in BsddWire.Rows(response.ClassProperties).TraverseM(p => Property(p, key)).As()
              from relations in BsddWire.Rows(response.ClassRelations).TraverseM(r => RelationOf(r, key)).As()
              from reverse in BsddWire.Rows(response.ReverseClassRelations).TraverseM(r => RelationOf(r, key)).As()
              select new BsddClass(
                  response.Code, response.Name, BsddWire.Text(response.ClassType), BsddWire.Text(response.Definition), response.Uri,
                  properties, status, BsddWire.Rows(response.RelatedIfcEntityNames), relations, reverse,
                  Optional(response.ParentClassReference).Map(BsddWire.Ref),
                  BsddWire.Rows(response.Hierarchy).OrderBy(static item => item.Level).Map(BsddWire.Ref).ToSeq(),
                  BsddWire.Rows(response.ChildClassReferences).Map(BsddWire.Ref),
                  BsddWire.Rows(response.ReplacedObjectCodes),
                  BsddWire.Rows(response.ReplacingObjectCodes),
                  Optional(response.DeprecationExplanation).Filter(static s => s.Length > 0));

    // The fault carries the replacing code so the caller re-authors onto the successor. Preview stays admissible here;
    // the IDS facet owns the preview-acceptance policy on the evidence Status.
    public Fin<BsddClass> Admit(Op key) =>
        Status != BsddStatus.Inactive
            ? Fin.Succ(this)
            : Fin.Fail<BsddClass>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-superseded", Code, ReplacedBy.Head.IfNone("") })));

    // bSDD carries NO bare `code` on a class-property — `name` is the only required member — so propertyCode is the
    // code authority and the name is its fallback. The two Fin admissions are why this stays hand-written: a generated
    // map cannot carry a lane.
    static Fin<BsddProperty> Property(BsddClassResponse.ClassProperty p, Op key) =>
        from kind in ValueKindOf(p.PropertyValueKind, key)
        from status in StatusOf(p.PropertyStatus, key)
        select new BsddProperty(
            Optional(p.PropertyCode).Filter(static c => c.Length > 0).IfNone(p.Name), p.Name,
            BsddWire.Text(p.DataType), BsddWire.Text(p.PropertySet), BsddWire.Text(p.PredefinedValue),
            TraitsOf(p), kind, status, BsddWire.Rows(p.AllowedValues).Map(BsddWire.Allowed),
            Optional(p.Pattern).Filter(static s => s.Length > 0), BoundsOf(p), DimensionOf(p), BsddWire.Rows(p.Units));

    // A dictionary that answered the class request STATED a verdict, so Declared always holds here and the EMPTY set
    // stays reserved for a source that never spoke — the offline buildingSMART floor, whose PropertyDef publishes no
    // requiredness column at all.
    static CapabilitySet<TemplateTrait> TraitsOf(BsddClassResponse.ClassProperty p) =>
        p.IsRequired
            ? CapabilitySet<TemplateTrait>.Of(TemplateTrait.Declared, TemplateTrait.Required)
            : CapabilitySet<TemplateTrait>.Of(TemplateTrait.Declared);

    static Option<BsddBounds> BoundsOf(BsddClassResponse.ClassProperty p) =>
        p is { MinInclusive: null, MaxInclusive: null, MinExclusive: null, MaxExclusive: null }
            ? None
            : Some(new BsddBounds(Optional(p.MinInclusive), Optional(p.MaxInclusive), Optional(p.MinExclusive), Optional(p.MaxExclusive)));

    // A dimensionless result (every exponent zero) IS the undimensioned property, so it lowers None and the seam
    // MeasureValue coercion never fires on a plain count or label.
    static Option<Dimension> DimensionOf(BsddClassResponse.ClassProperty p) =>
        Dimension.Create(
            BsddWire.Exponent(p.DimensionLength), BsddWire.Exponent(p.DimensionMass), BsddWire.Exponent(p.DimensionTime),
            BsddWire.Exponent(p.DimensionElectricCurrent), BsddWire.Exponent(p.DimensionThermodynamicTemperature),
            BsddWire.Exponent(p.DimensionAmountOfSubstance), BsddWire.Exponent(p.DimensionLuminousIntensity))
        is var dimension && dimension != Dimension.Dimensionless ? Some(dimension) : None;

    // Each ClassRelationContract -> the typed row: an unparseable relationType or a blank relatedClassUri faults so the
    // federation closure never sees an unaddressable edge.
    static Fin<BsddRelation> RelationOf(BsddClassResponse.ClassRelation relation, Op key) =>
        relation.RelatedClassUri is not { Length: > 0 }
            ? Fin.Fail<BsddRelation>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-relation-uri-missing" })))
            : BsddRelationKind.TryGet(relation.RelationType, out BsddRelationKind? kind) && kind is { } resolved
                ? Fin.Succ(new BsddRelation(resolved, relation.RelatedClassUri, BsddWire.Text(relation.RelatedClassName), Optional(relation.Fraction)))
                : Fin.Fail<BsddRelation>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "relation-kind", relation.RelationType })));

    static Fin<BsddValueKind> ValueKindOf(string? kind, Op key) =>
        BsddValueKind.TryGet(BsddWire.Text(kind), out BsddValueKind? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddValueKind>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "value-kind", kind })));

    static Fin<BsddStatus> StatusOf(string? status, Op key) =>
        BsddStatus.TryGet(BsddWire.Text(status), out BsddStatus? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddStatus>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "status", status })));
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// A containment pointer and an allowed value are pure column maps with no admission, no fault, and no lookup, which is
// exactly the boundary Riok.Mapperly owns; the two Ref overloads discriminate on the wire shape (a ClassReference
// carries its Uri required, a HierarchyItem optional). Everything carrying a Fin stays hand-written — Mapperly
// transcribes shape, never a lane.
// Text, Exponent and Rows are the ONE bSDD wire admission: the contract is additionalProperties:false and declares
// every optional column nullable, so each absence crosses here exactly once and a `?? ""` or `?? []` below is the
// deleted duplicate. Rows is registered Default = false because a GENERIC mapping Mapperly may choose is the RMG001
// form the folder refuses, so the array crossings name it explicitly.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class BsddWire {
    public static partial BsddRef Ref(BsddClassResponse.ClassReference reference);
    public static partial BsddRef Ref(BsddClassResponse.HierarchyItem item);
    public static partial BsddAllowedValue Allowed(BsddClassResponse.AllowedValue value);

    [UserMapping]
    internal static string Text(string? value) => value ?? "";

    // An unstated SI exponent IS zero by the signature's own algebra — the dictionary omits the column for a dimension
    // the property does not carry, so the absence and the zero are one fact, not a substituted default.
    [UserMapping]
    internal static int Exponent(int? value) => value ?? 0;

    [UserMapping(Default = false)]
    internal static Seq<TWire> Rows<TWire>(TWire[]? values) => values is null ? Seq<TWire>() : toSeq(values);
}

// Equivalence under a SYMMETRIC relation IS connected-component membership, so the linear label pass yields the
// partition the quadratic ComputeTransitiveClosure edge set had to be re-grouped to recover, and the undirected
// container makes the per-relation reverse edge unnecessary at the same time (.api/api-quikgraph — a hand-rolled
// BFS/union-find is the named rejected form). Reverse rows still fold in: an inbound equivalence is declared by the
// OTHER dictionary. The graph never escapes Of; the receipt carries data alone.
public sealed record BsddFederation(Map<string, Seq<string>> Equivalence, Map<string, string> Names) {
    // ONE pass yields BOTH receipt inputs — each class's forward+reverse rows are read once for its equivalence EDGE
    // and its related-URI LABEL together — where the retired second fold re-walked the same concatenation for the
    // names alone and the two passes could disagree about which rows they had seen.
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

    // An EMPTY sequence is the honest answer for an unrostered source system or a component holding no peer under the
    // target stem, and never a wrong lowering; a peer URI whose tail segment is empty is the seam's OWN refusal, so
    // the leg RAILS rather than folding a malformed code into that same emptiness.
    public Fin<Seq<Classification>> Translate(Classification classification, ClassificationSystem target, BsddPins pins, Op key) =>
        ClassificationSystem.TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            && target.Stem(pins) is { Length: > 0 } stem
            ? toSeq(EquivalentSet(row.ClassUri(classification.Code, pins))
                .Filter(uri => uri.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(identity))
                .TraverseM(uri => Classification.Of(
                    target.Key, ClassificationSystem.TailCode(uri), key, title: Names.Find(uri))).As()
            : Fin.Succ(Seq<Classification>());
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The bSDD api/Class/v1 wire contract (.api/api-bsdd ClassContract.v1 and its four member contracts): the projection
// reads ONLY these fields (additionalProperties:false), each PascalCase member binding the camelCase wire through the
// transport's STJ naming policy, and every nullable member is an OPTIONAL wire field the BsddWire admission owns.
// IsRequired stays a bare bool HERE because the contract declares it one; re-shaping a wire member to match the
// domain's own CapabilitySet carrier forks the transcription.
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
// The return is THREE-state and the page discriminates all three: Succ(Some) a reached endpoint whose body decoded,
// Succ(None) an UNREACHED endpoint (the only genuine offline miss, the one state Resolve degrades on), Fail a reached
// endpoint whose body did NOT decode. Collapsing the last two into one failure is what let a schema drift substitute
// property-free local evidence for a class the registry answered for. The token rides the request, so the abort grain
// is BETWEEN round-trips and an already-dispatched request runs to its own completion.
public interface BsddPort {
    Fin<Option<TWire>> Fetch<TWire>(string resource, CancellationToken token);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BsddResolution {
    // Every hop is a public-registry round-trip that answers a 429 or a gateway blip as readily as a class, and the
    // retired lane carried NO retry rail: one transient refusal degraded a REACHABLE dictionary to the offline shape
    // gate. Cadence and bound are the only facts this page owns — Redrive.Settle reads the Expected case's OWN
    // Retriability, so no predicate is spelled here.
    static readonly RedrivePolicy Registry = RedrivePolicy.Of(
        law: Schedule.exponential(Duration.FromMilliseconds(250)) | Schedule.maxDelay(Duration.FromSeconds(4)), bound: 3);

    // The ONE round-trip every resource crosses, so the policy is declared once and no entry spells an attempt loop.
    // The port's Fin lane lifts ONTO the IO rail before Redrive sees it, with the exact captured Error retained.
    static Fin<Option<TWire>> Fetch<TWire>(BsddPort port, string resource, CancellationToken token, Op key) =>
        key.Catch(() => Redrive.Run(
            policy: Registry,
            work: IO.lift(() => key.Catch(() => port.Fetch<TWire>(resource, token), token))).Run(), token);

    // Three outcomes, kept distinct at the port rather than inferred from one collapsed failure: an UNREACHED endpoint
    // degrades to the row's local code-shape policy — the gate applied, never a fault for unreachability itself — a
    // reached-but-undecodable body surfaces Refused/BimReason.Codec, and a decoded body's own payload verdict is BsddClass.Of's.
    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        Fetch<BsddClassResponse>(port, ClassResource(system.ClassUri(code, pins)), token, key)
            .Bind(reached => reached.Match(
                Some: response => BsddClass.Of(response, key),
                None: () => LocalShape(system, code, pins, key)));

    // The admitted class's OWN Code and Name ride onto the minted value: the round-trip is already paid, so discarding
    // the registry's canonical casing and its concept title would leave a certified value indistinguishable from a
    // bare local one. Offline the LocalShape degrade admits, so certification TIGHTENS when the dictionary answers and
    // never blocks when it cannot.
    public static Fin<Classification> Certify(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        Resolve(system, code, port, pins, token, key)
            .Bind(cls => cls.Admit(key))
            .Bind(admitted => system.Classify(admitted.Code, Optional(admitted.Name).Filter(static n => n.Length > 0), pins, key));

    // No offline concept index exists, so BOTH failure states FAULT typed — an unreached endpoint and an undecodable
    // body alike land Refused/BimReason.Codec, never a LocalShape degrade. A hit whose dictionary is unrostered drops: the roster
    // IS the admission, and an unrostered code cannot lower onto the seam.
    public static Fin<Seq<BsddHit>> Search(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        Fetch<BsddSearchResponse>(port, SearchResource(text, scope, relatedIfcEntity, pins), token, key)
            .Bind(reached => reached.ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-search-unreachable", text }))))
            // The instance indexed Map is VALUE-FIRST (the Seq module's `map` transposes) — the ordinal is the rank.
            .Map(response => BsddWire.Rows(response.Classes).Map((hit, index) => HitOf(hit, index, pins)).Somes());

    // Hierarchy and parentClassReference arrive by default, so the class request opts into the relation/children
    // rollups alone. The search request REPEATS the DictionaryUris/RelatedIfcEntities keys per the bSDD
    // array-parameter law, and an empty scope pins the HOSTED roster rows server-side so the page limit is never spent
    // on unrostered dictionaries HitOf would drop anyway.
    static string ClassResource(string classUri) =>
        $"api/Class/v1?Uri={System.Uri.EscapeDataString(classUri)}&IncludeClassProperties=true&IncludeClassRelations=true&IncludeReverseRelations=true&IncludeChildClassReferences=true";

    static string SearchResource(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPins pins) =>
        string.Concat(
            $"api/Class/Search/v1?SearchText={System.Uri.EscapeDataString(text.Trim())}",
            (scope.IsEmpty ? toSeq(ClassificationSystem.Items).Filter(row => row.Hosted(pins)) : scope)
                .Fold("", (acc, row) => $"{acc}&DictionaryUris={System.Uri.EscapeDataString(row.DictionaryUri(pins))}"),
            relatedIfcEntity.Match(Some: static entity => $"&RelatedIfcEntities={System.Uri.EscapeDataString(entity)}", None: static () => ""));

    // CLASS-FIRST, refining by name only where it must: one query per entity CLASS is the coarse pass (the entity code
    // is both the search text and the RelatedIfcEntities scope), and a class whose coarse hit set DECIDES needs
    // nothing more, which is the ordinary shape of a model whose walls all classify alike. Only an AMBIGUOUS class
    // re-queries per distinct element NAME, spending the extra round-trips exactly where the name is the discriminant
    // the class was not — keying every group on the name up front spent a round-trip per naming variant a model
    // happens to carry, and the predefined token never reaches the wire at all, so keying on it split one query into
    // as many identical round-trips as the model had tokens.
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
            ? ClassificationSystem.ByUri(Optional(hit.DictionaryUri).Filter(static u => u.Length > 0).IfNone(hit.Uri), pins)
                .Map(system => new BsddHit(
                    system,
                    Optional(hit.ReferenceCode).Filter(static c => c.Length > 0).IfNone(() => ClassificationSystem.TailCode(hit.Uri)),
                    hit.Name, hit.Uri, BsddWire.Rows(hit.RelatedIfcEntityNames), index))
            : None;

    // A shape-reject faults the SAME Refused/BimReason.Unmapped detail Classify mints — one local policy, one fault — never a
    // fabricated Active evidence for a garbage code the dictionary could not have answered for. The evidence Uri is
    // stem-only because an unreached dictionary pins no version to build a class URI against. The construction stops
    // at Status: every trailing member's DEFAULT IS its empty value, and spelling one out restates the language's own
    // default while inviting a reader to infer the omitted slots mean something else.
    static Fin<BsddClass> LocalShape(ClassificationSystem system, string code, BsddPins pins, Op key) =>
        system.Resolve(pins) is var row && row.Shape.IsMatch(code.Trim())
            // ClassType stays EMPTY: it is a four-token registry vocabulary (Class|Material|GroupOfProperties|
            // AlternativeUse) and no dictionary answered, so a stamped "Class" is indistinguishable to a consumer
            // branching on it from a resolved one. Status is stated explicitly — the degrade's own declared policy,
            // never an assumed default.
            ? Fin.Succ(new BsddClass(code, $"{row.Title}:{code}", "", "", $"{row.Stem}/class/{System.Uri.EscapeDataString(code.Trim())}", Seq<BsddProperty>(), BsddStatus.Active))
            : Fin.Fail<BsddClass>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-code-reject", system.Key, code })));
}
```

## [04]-[RESEARCH]

(none)
