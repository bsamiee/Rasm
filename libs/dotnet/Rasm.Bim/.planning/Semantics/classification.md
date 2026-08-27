# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the shared `Classification` value an `Object` node carries: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/ETIM/IfcClassification), plus a `Project` row whose whole identity arrives as composition data, each row ONE reader answering its `SystemIdentity` (title, version-free `Stem`, hosted version, compiled code shape) out of the composition-supplied `BsddPins` policy, the stem and version deriving the versioned `DictionaryUri` the live resolution and the egress `Location` use, validating a raw code against the system's shape and LOWERING it onto the library-neutral shared `Classification` value — the full-identity factory `(System, Code, Edition, Source, EditionDate, Title)`: the `(System, Code, Edition)` edition-scoped IDENTITY plus the equality-excluded resolved-annotation bundle. The contract owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (no roster, no dictionary, no regex); this page is the downstream Bim projector the contract names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto the shared value at ingest and re-authoring that value at egress. A classification is the shared `Classification` VALUE on an `Object` node: `Relations/relation#EDGE_ALGEBRA` is explicit that classification is a value on the node, NOT an edge (the neutral `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the retired `(GlobalId, system, code)` triple bound to a second stored `BimElement` record is GONE — a query reads `node.Classification.System`/`Code`, never a stringly-keyed lookup against a second element store.

The round-trip is BIDIRECTIONAL across three entries: `Classify` lowers a validated raw code at authoring ingress, `Ingest` resolves an imported `IfcClassificationReference` back to the shared value at import ingress (the inverse of `Author`), and `Author` re-authors a node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress — the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node, which REPLACES the retired `Rasm.Materials` `MaterialPropertyWire.Classification` half (a material carries no classification; the `Object` node does): the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands a substance's standard `(system, code)` as the bound element's `Object`-node `Classification` value through its `MaterialBinding` egress, so the element classification this owner round-trips is the one the unified Component projection authored, never a material-wire field. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping carries the FULL `ClassContract.v1` surface: the class-scoped constraint surface (IFC `DataType` + Pset placement, `ValueKind`, allowed values, XSD `Pattern`, numeric `Bounds`, the SI base-dimension exponents the shared `Dimension` carries + `Units`, `Status`) that feeds the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template, the `Review/validation#IDS_FACETS` Classification + Property facets, and the shared `Properties/quantity#MEASURE_VALUE` UnitsNet coercion directly, PLUS the relation set (`classRelations`/`reverseClassRelations` → typed `BsddRelation` rows whose `IsEqualTo`/`IsSynonymOf` edges `BsddFederation` closes through the shared QuikGraph substrate into cross-standard equivalence — `Translate` lowers an OmniClass code onto its Uniclass peer, a capability unreachable from any code string), the AUTHORITATIVE containment (`parentClassReference`/`hierarchy`/`childClassReferences` → `BsddRef` — the parent a MasterFormat/Uniformat code string does not encode), and the supersession surface (`status`/`replacedObjectCodes`/`replacingObjectCodes`/`deprecationExplanation` — `BsddClass.Admit` refuses to certify a NEW code onto an `Inactive` class, carrying the replacing code in the fault). `BsddResolution.Certify` is the dictionary-certified authoring lowering (`Resolve` → `Admit` → `Classify`, the resolved class's own code and title carried onto the minted value) and `BsddResolution.Search` resolves a concept label or IFC entity to candidate codes (`api/Class/Search/v1`) — so a new standard is one `ClassificationSystem` dictionary-identity row shared across `classification`, `properties`, and `validation`. The typed `Model/faults#FAULT_BAND` `BimFault` cases lift BARE onto the `Fin` result (band 2600 owns the generated `Code`; no `.ToError()` hop), each carrying the kernel `Op` operation context the caller threads.

## [01]-[INDEX]

- [02]-[CLASSIFICATION_AXIS]: `BsddPins` the composition-supplied hosted-version policy; `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary stem, version pin reader, code-shape policy); `Classify(code, title, key)` gating a raw code on the row's shape and handing it to the contract's ONE result-returning `Classification.Of` admission at authoring ingress; `Ingest(reference, pins, key)` resolving an imported `IfcClassificationReference` back onto a shared value at import ingress; and `Author(db, related, classification, pins)` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` over the shared `Semantics/composition#EGRESS` `EmitMemo` dictionary-source memo (the element-classification egress the `Projection/egress#IFC_EGRESS` `Emit` composes).
- [03]-[BSDD_RESOLUTION]: the live bSDD dictionary resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, projecting the FULL `ClassContract.v1` surface — the constraint surface (`ValueKind`/`AllowedValues`/`Pattern`/`Bounds`/`SiDimension`/`Units`), the relation set (`BsddRelation` forward + reverse), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`) — degrading to the row's local code-shape policy ONLY when the endpoint was unreached; `BsddWire` the `[Mapper]` boundary transcription owning the result-free wire rows; `BsddResolution.Certify` the supersession-gated authoring lowering, `BsddResolution.Search` the `api/Class/Search/v1` concept-to-code resolution carrying each hit's server-order `Rank`, and `BsddFederation` the QuikGraph connected-component equivalence classes whose `Translate` lowers a code across standards onto the shared value; feeding the `Semantics/properties#PROPERTY_TEMPLATES` template, the IDS facets, and the shared `MeasureValue` coercion; the `Suggest` graph-scale enrichment fold returning ranked `ClassificationSuggestion` candidates per unclassified element and target system.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row ONE `Func<BsddPins, SystemIdentity>` reader answering the four facts a dictionary address and a code gate need — the display `Title`, the version-free `Stem` (the ingest prefix identity a foreign-edition `Location` still matches), the hosted `Version`, and the compiled code shape — the stem and version deriving the versioned `DictionaryUri` (the bSDD identifier scheme is `{org}/{dictionary}/{version}`; a versionless class URI does not resolve, so the version-bearing row IS the live-lane admission and an unhosted system reads a blank version so its live leg degrades by construction); `SystemIdentity` that four-fact carrier and `BsddPins` the ONE policy record supplying the hosted version tokens plus the `Custom` project-system identity the `Project` row reads WHOLE, so a durable standard freezes its identity at declaration while a registry-republished version and a client asset-code scheme alike stay values a composition overrides. The projector lowers a resolved standard code onto the shared `Classification/classification#CLASSIFICATION_AXIS` `Classification` value the `Object` node carries and authors it back at egress. The shared `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the contract defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the shared value, never re-declaring a classification value-object beside the contract.
- Entry: `ClassificationSystem.Classify(string code, Option<string> title, BsddPins pins)` validates the raw code against the system's code-shape regex and lowers it onto a shared `Classification(Key, code, "", None, None, title)` value — the `title` the resolved concept name where a caller HAS one (the `Certify` lane carries the dictionary class's own) and `None` on the bare authoring path, so one entry spans both without a mode flag; `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Unmapped`, the typed case lifting BARE off `key`, no `.ToError()` hop); `ClassificationSystem.Ingest(IfcClassificationReference reference, BsddPins pins)` is the import-ingress inverse resolving the standard system off the reference's `ReferencedSource` root dictionary title, the root's `Specification` dictionary-URI prefix, OR its `Location` identifier-URI prefix against the roster (the code off `Identification` or the trailing `Location` segment, the resolved `Title` off the reference's own `Name`), returning `Fin<Option<Classification>>` the `Projection/semantic#SEMANTIC_PROJECTOR` ingress accumulates onto the `Graph/element#NODE_MODEL` `Object` node's `Classifications` set (IFC admits MULTIPLE `IfcRelAssociatesClassification` per object) — `Succ(None)` for an unrostered source or a code-free reference so a foreign system rides the `Projection/relations#RELATION_ALGEBRA` `Generic` passthrough rather than a wrong lowering, `Fail` for the contract's own blank-token refusal a collapsed `None` masks as a foreign system; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification, BsddPins pins)` is the egress entry the `Emit` composes per `Object` node — `None` for the `IfcSystem.Key` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system's edition-scoped dictionary, `Identification` the code, `Location` the identifier URI, and `Name` the shared `Title` — the members `Ingest` reads back, so the full identity survives the egress leg.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the shape enforcement a decorative code type never applies) and, on a match, hands the pair to the contract's ONE result-returning admission `Classification.Of(Key, code, key, title: title)` — the `[ComplexValueObject]` entry normalizing the system token, trimming the code, and accumulating its two frozen blank-token gates under the caller's the edition-unspecified `Edition ""` and the `Source`/`EditionDate` annotations absent on every ingress path since neither a raw code nor a dictionary class carries a publisher edition — so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30", "")` and a code either gate rejects faults rather than lowering a malformed value; the containment hierarchy is the bSDD `Parent`/`Ancestry`/`Children` evidence the `[03]-[BSDD_RESOLUTION]` owner resolves, never a contract-side code-shape derivation. `Ingest` walks `RootSource` to the root `IfcClassification` ONCE: it matches a roster row by the dictionary `Name` equality, the root's `Specification` dictionary-URI through the shared `ByUri` version-free `Stem` prefix (the URI `Author` itself stamps, so the round-trip self-resolves even when a re-export strips per-reference `Location`s), or the reference `Location` prefix (a `Location` minted under ANY dictionary edition — a foreign 2015-edition reference, a future `ifc/4.6` class URI — still resolves its row), reads the code off `CodeOf` (`TailCode`-unescaping the `Location` trailing segment when the `Identification` is absent), and lowers the EDITION-SCOPED annotation bundle off that SAME root — the `Edition` token (IDENTITY on the contract), the `Source` publisher, and the `EditionDate` revision date (`IfcClassification.Edition`/`Source`/`EditionDate` decompile-confirmed) — while `TitleOf` reads the reference's OWN `IfcClassificationReference.Name` (distinct from the root dictionary `Name`) as the resolved concept `Title`, so the imported leg lands the FULL identity (edition + publisher + date + title) at the only path that can populate it rather than a perpetually edition-blank, title-`None` reference. `Author` resolves the row through the generated `TryGet` and authors the reference with the URI-escaped `Location` (`ClassUri`) so an OmniClass/MasterFormat code carrying spaces round-trips, the reference `Name` carrying the shared `Title` and the `(db, system, edition)`-keyed dictionary memo re-stamping the value's `Edition`/`Source`/`EditionDate` — the egress leg of the same identity `Ingest` lowers; the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row whose reader closes over its frozen `SystemIdentity`; a dictionary the public bSDD starts hosting is one version column on `BsddPins` plus the row's reader, never a shape edit; a PROJECT-OWN system is a `SystemIdentity` value on `BsddPins.Custom` with no roster edit at all, the `Project` row already reading it; a registry that re-publishes a dictionary version is one value the composition passes, never a durable-page edit; the shared `Classification` value-object absorbs any `(system, code)` pair with no contract edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type, never a parallel classification value-object beside the contract, and never a per-direction resolver (one `Classify`/`Ingest`/`Author` triad spans the whole round-trip).
- Boundary: the classification systems are ONE keyed axis (`ClassificationSystem` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the classification VALUE is the shared `Classification` `[ComplexValueObject]` and a Bim `Classification`/`ClassificationCode`/`ClassificationRef` value-object is the deleted form — the contract owns the typed pair, this page owns the standard-systems roster and lowers onto it, so the type name `ClassificationSystem` never collides with the shared `Classification`; `Classification.Of` is the contract's ONE admission and every mint on this page composes it — a throwing `Classification.Create`/`TryCreate`/`Validate` spelling is the deleted form, and so is a contract-side containment or crosswalk read (`Parent`, `Within`, `Ancestors`, `TranslateTo`), the authoritative hierarchy and the cross-standard equivalence both being `[03]-[BSDD_RESOLUTION]` evidence; the hosted VERSION tokens and the project-own system identity are composition-supplied `BsddPins` values, and a version literal or a client asset-code scheme frozen into a durable roster row is the deleted form — a token the registry re-publishes rots into a `404` the offline degrade then masks as unreachability, so it lives at the one overridable value whose defaults serve an unconfigured composition; the `Classify(BimElement element, …)` binding to a `BimElement.GlobalId` is GONE (the `BimElement`/`BimModel` are retired, the consumer element being the `Graph/element#ELEMENT_GRAPH` `Bake` fold) — a classification is the shared value on the `Object` node, never a `(GlobalId, system, code)` triple keyed to a second element record; classification is a VALUE on the `Object` node and NOT an edge (the shared `Associate` edge carries a `Material`/`Appearance` resource, never a classification), so the egress reads the node `Classification` value and a classification-association `Relationship` case is the deleted form; the code shape is the row's regex validated once at `Classify`, never a per-call regex at the call site; every comparison against a system key runs `OrdinalIgnoreCase` — the key space the row's own `[KeyMemberEqualityComparer]` declares — so a raw `!=`/`==` on a system token is the deleted form that reads `"IFC"` and `"ifc"` as different systems, and a bare `"ifc"` literal where `IfcSystem.Key` names the same token is the deleted form; the typed `BimFault` lifts BARE off the threaded `Op key` and a `.ToError()` hop or a single-string fault ctor is the named defect this owner closes (the band owns the generated `Code`); the bSDD dictionary is the authoritative live source for the class-to-property constraint surface resolved through the dictionary URI, never a hardcoded code-to-property table that duplicates and drifts from it; the per-system `CodeShape` regex is BOTH the cheap LOCAL shape gate `Classify` admits a raw authoring code through (no network round-trip) AND the offline degradation `BsddResolution.LocalShape` falls back to, never that drifting constraint table; the classification round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`.api/api-geometrygym-ifc`) consumed as settled vocabulary, the egress carrying `Identification` + `Location` (+ the `Name` concept title the shared `Classification.Title` round-trips) so the import `Ingest` reconstructs the shared value losslessly, never re-minting a classification mapping; the db-scoped dictionary memo composes the `Semantics/composition#EGRESS` `EmitMemo` owner and a second `ConditionalWeakTable` declared here is the deleted duplicate; the egress reads the shared `Object` node `Classification`, NOT a Materials `MaterialPropertyWire.Classification` carrier (retired), the material-wire classification half having moved to this element-classification egress.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

namespace Rasm.Bim.Semantics;

// --- [TYPES] ---------------------------------------------------------------------------
public sealed record SystemIdentity(string Title, string Stem, string Version, string Pattern) {
    public static readonly SystemIdentity Unclaimed = new("", "", "", @"^(?!)$");

    public Regex Shape { get; } = new(Pattern, RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);
}

public sealed record BsddPins(string Ifc, string Etim, SystemIdentity Custom) {
    public static readonly BsddPins Default = new(Ifc: "4.3", Etim: "10.0", Custom: SystemIdentity.Unclaimed);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClassificationSystem {
    public static readonly ClassificationSystem Uniclass2015 = Standard("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", @"^[A-Z][A-Za-z]_\d{2}(_\d{2}){0,3}$");
    public static readonly ClassificationSystem OmniClass     = Standard("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", @"^\d{2}-\d{2}( \d{2}){2,3}$");
    public static readonly ClassificationSystem MasterFormat  = Standard("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", @"^\d{2} \d{2} \d{2}(\.\d{2})?$");
    public static readonly ClassificationSystem Uniformat     = Standard("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", @"^[A-Z]\d{4}$");
    public static readonly ClassificationSystem Etim          = new("etim", static pins => new SystemIdentity("ETIM", "https://identifier.buildingsmart.org/uri/etim/etim", pins.Etim, @"^EC\d{6}$"));
    public static readonly ClassificationSystem IfcSystem     = new("ifc", static pins => new SystemIdentity("IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", pins.Ifc, @"^Ifc[A-Za-z]+$"));
    public static readonly ClassificationSystem Project       = new("project", static pins => pins.Custom);

    public Func<BsddPins, SystemIdentity> Resolve { get; }

    private ClassificationSystem(string key, Func<BsddPins, SystemIdentity> resolve) : this(key) => Resolve = resolve;

    static ClassificationSystem Standard(string key, string title, string stem, string pattern) {
        var identity = new SystemIdentity(title, stem, "", pattern);
        return new ClassificationSystem(_ => identity);
    }

    public string Title(BsddPins pins) => Resolve(pins).Title;

    public string Stem(BsddPins pins) => Resolve(pins).Stem;

    public Regex CodeShape(BsddPins pins) => Resolve(pins).Shape;

    public string DictionaryUri(BsddPins pins) =>
        Resolve(pins) switch { { Version.Length: > 0 } hosted => $"{hosted.Stem}/{hosted.Version}", var bare => bare.Stem };

    public bool Hosted(BsddPins pins) => Resolve(pins).Version.Length > 0;

    public Fin<Classification> Classify(string code, Option<string> title, BsddPins pins) =>
        CodeShape(pins).IsMatch(code.Trim())
            ? Classification.Of(Key, code, title: title)
            : Fin.Fail<Classification>(new BimFault.Refused(BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-code-reject", Key, code })));

    public string ClassUri(string code, BsddPins pins) => $"{DictionaryUri(pins)}/class/{System.Uri.EscapeDataString(code.Trim())}";

    internal static Option<ClassificationSystem> ByUri(string uri, BsddPins pins) =>
        uri is { Length: > 0 }
            ? Optional(Items.Select(row => (Row: row, Stem: row.Stem(pins)))
                .Where(row => row.Stem.Length > 0 && uri.StartsWith(row.Stem, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(static row => row.Stem.Length).Select(static row => row.Row).FirstOrDefault())
            : None;

    internal static string TailCode(string uri) => System.Uri.UnescapeDataString(uri[(uri.LastIndexOf('/') + 1)..]);

    public static Fin<Option<Classification>> Ingest(IfcClassificationReference reference, BsddPins pins) {
        IfcClassification? dictionary = RootSource(reference);
        return (Optional(Items.FirstOrDefault(row => row.Title(pins) is { Length: > 0 } title && string.Equals(title, PropertyLowering.Stated(dictionary?.Name).IfNone(""), StringComparison.OrdinalIgnoreCase)))
            | ByUri(PropertyLowering.Stated(dictionary?.Specification).IfNone(""), pins)
            | ByUri(PropertyLowering.Stated(reference.Location).IfNone(""), pins))
            .Bind(system => Optional(CodeOf(reference)).Filter(static code => code.Length > 0).Map(code => (system, code)))
            .TraverseM(found => Classification.Of(found.code,
                    edition: PropertyLowering.Stated(dictionary?.Edition).IfNone("").Trim(),
                    source: SourceOf(dictionary),
                    editionDate: EditionDateOf(dictionary),
                    title: TitleOf(reference)))
            .As();
    }

    static IfcClassification? RootSource(IfcClassificationReference reference) {
        IfcClassificationReferenceSelect? source = reference.ReferencedSource;
        for (int depth = 0; source is IfcClassificationReference parent && depth < 32; depth++) {
            source = parent.ReferencedSource;
        }
        return source as IfcClassification;
    }

    static Option<LocalDate> EditionDateOf(IfcClassification? dictionary) =>
        dictionary is { EditionDate: var date } && date > System.DateTime.MinValue
            ? Some(LocalDate.FromDateTime(date))
            : None;

    static Option<string> SourceOf(IfcClassification? dictionary) =>
        PropertyLowering.Stated(dictionary?.Source).Map(static source => source.Trim());

    static string CodeOf(IfcClassificationReference reference) =>
        (PropertyLowering.Stated(reference.Identification)
            | PropertyLowering.Stated(reference.Location).Map(TailCode)).IfNone("");

    static Option<string> TitleOf(IfcClassificationReference reference) =>
        PropertyLowering.Stated(reference.Name).Map(static name => name.Trim());

    static readonly EmitMemo<(string System, string Edition), IfcClassification> Sources = new();

    static IfcClassification Source(DatabaseIfc db, ClassificationSystem row, Classification classification, BsddPins pins) =>
        Sources.Of(db, (row.Key, classification.Edition), _ => {
            var dictionary = new IfcClassification(db, row.Title(pins)) { Specification = row.DictionaryUri(pins), Edition = classification.Edition };
            classification.Source.IfSome(publisher => dictionary.Source = publisher);
            classification.EditionDate.IfSome(date => dictionary.EditionDate = date.ToDateTimeUnspecified());
            return dictionary;
        });

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

- Owner: `BsddResolution` the live bSDD dictionary resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the endpoint was UNREACHED so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence carrying the FULL `ClassContract.v1` surface — the class-scoped constraint surface the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey.Resolve` template, the IDS facets, and the shared `MeasureValue` coercion read, the forward + reverse relation set (`BsddRelation`), the authoritative containment (`Parent`/`Ancestry`/`Children` `BsddRef` rows — the parent a MasterFormat/Uniformat code string does not encode and the contract carries no derivation for, the dictionary being the ONLY source that states it), and the supersession surface (`Status`/`Replaces`/`ReplacedBy`/`Deprecation`); `BsddWire` the `[Mapper]` boundary transcription owning the result-free wire-row crossings; `BsddFederation` the cross-standard equivalence classes over the relation graph.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token)` resolves the full dictionary-class evidence over Compute's transport — `Fin<T>` returns the resolved `BsddClass` on a live hit; an UNREACHED endpoint degrades to the row's local code-shape policy APPLIED (`LocalShape` — a shape-passing code admits as property-free `Active` evidence, a shape-reject faults `BimFault.Refused` with `BimReason.Unmapped` with the same `classification-code-reject` detail `Classify` mints; unreachability itself never faults), while a REACHED endpoint whose body the decoder rejected retains the port's exact `Error` — the two are distinct states on the port's own return, never inferred from one collapsed failure. `BsddResolution.Certify(system, code, port, pins, token)` is the dictionary-certified authoring lowering composing `Resolve` → `BsddClass.Admit` → `Classify`, so a NEW code never lands on an `Inactive`/superseded class AND the certified value carries the dictionary's OWN code and concept title rather than discarding the evidence it just paid a round-trip for (offline the shape-gated degrade admits and the row's local shape gates alone — certification tightens when the dictionary answers, never blocks when it cannot). `BsddResolution.Search(text, scope, relatedIfcEntity, port, pins, token)` resolves a concept label or IFC entity to candidate `BsddHit` codes over `api/Class/Search/v1`, each hit carrying its server-order `Rank` — authoring-time, never the ingest path, no offline concept index exists, so an unreached endpoint faults `BimFault.Refused` with `BimReason.Codec` while a port failure retains its exact `Error`. `BsddResolution.Suggest(ElementGraph graph, Seq<ClassificationSystem> targets, BsddPort port, BsddPins pins, CancellationToken token)` is the graph-scale ENRICHMENT fold — every entity-type-classified occurrence with no classification in a target system resolves ranked candidate codes once per distinct QUERY key through the same `Search` wire (server order the rank, the roster the admission), returning per-element `ClassificationSuggestion` rows an authoring surface confirms in bulk through `Certify` — the as-received unclassified model gains its mandated Uniclass/OmniClass coverage without per-element hand lookup, the difference between a checker and an authoring assistant. `BsddFederation.Of(classes)` folds resolved evidence into the equivalence closure; `Translate(classification, target, pins)` lowers a code across standards onto the shared value over `Classification.Of`, returning `Fin<Seq<Classification>>` so a peer URI the contract refuses returns instead of vanishing into the same empty sequence an unrostered source yields; `Equivalent`/`EquivalentSet` answer the pairwise and set queries. EVERY live hop crosses the one `Fetch` member under the declared `RedrivePolicy Registry` — `RedrivePolicy.Of(Schedule.exponential | Schedule.maxDelay, bound)` composed through kernel `Redrive.Run` — so a transient registry refusal re-drives on the kernel curve instead of degrading a reachable dictionary to the offline shape gate.
- Auto: `Resolve` builds the class request from `ClassificationSystem.ClassUri(code, pins)` with `IncludeClassProperties`/`IncludeClassRelations`/`IncludeReverseRelations`/`IncludeChildClassReferences` (hierarchy and `parentClassReference` arrive by default), issues it over the injected `BsddPort` (the Compute transport boundary, ONE generic `Fetch<TWire>` — the page owns the resource query, the transport owns base URL, headers, the STJ camelCase decode, and the reached-versus-unreached discrimination), and projects the wire `BsddClassResponse` into the `BsddClass` evidence through `BsddClass.Of`: each `ClassProperty` projects through `Property` into a `BsddProperty` carrying the IFC `DataType`/Pset placement (the `Semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` template), the `ValueKind`/`AllowedValues`/`Pattern`/`Bounds` value constraints (the `Review/validation#IDS_FACETS` `ValueConstraint`), the shared `Dimension` built directly from the wire's SI exponent columns + `Units` (the shared `Properties/quantity#DIMENSION` owner's own generated factory, never a page-local exponent record beside it), and the `Status` (the IDS admission gate); each `ClassRelation` projects through `RelationOf` into a typed `BsddRelation`; the containment pointers project through the `BsddWire` mapper's generated `Ref` transcriptions. `BsddFederation.Of` folds the `IsEqualTo`/`IsSynonymOf` rows (forward AND reverse — an inbound equivalence declared by the OTHER dictionary counts) into a transient `UndirectedGraph<string, SEquatableEdge<string>>` ONE edge per relation, labels it through `AlgorithmExtensions.ConnectedComponents`, and projects each component whole onto every member URI — equivalence under a symmetric relation IS connected-component membership, so the linear component pass replaces the quadratic transitive-closure edge set and its per-source `GroupBy` re-projection; the QuikGraph fold is transient inside `Of`, `BsddFederation` carries data only. A transport miss degrades to `LocalShape` so a new standard becomes a dictionary-identity row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference. `Suggest` groups ONLY on the axes the query carries — the entity-class code and the search text — because the predefined token never reaches the request, and a group key carrying it split one query into N identical round-trips; the fold threads the caller's `CancellationToken` into every `Fetch`, so the abort grain is one in-flight request boundary and a request already dispatched runs to its own completion.
- Output: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_TEMPLATES` owner (`PropertyKey.Resolve(cls, predefined, schema, scope, Option<BsddClass>)` unioning the `BsddClass.Properties` dictionary rows OVER the offline `Xbim.Properties` catalogue floor under the caller's `TemplateScope` definition set, dictionary-wins), the IDS Classification + Property facets (the class URI is the facet value; `Pattern`/`Bounds`/`AllowedValues` narrow into an `Xbim.InformationSpecifications` `ValueConstraint`; `Status` gates admission), and the shared `MeasureValue` (a dimensioned property's `SiDimension` IS the shared `Dimension` the `UnitsNet` `BaseDimensions` projection reads); `BsddFederation` is the cross-standard translation evidence a multi-standard deliverable reads (`Translate` an OmniClass-classified model onto its Uniclass peers), and `Ancestry` is the authoritative inheritance chain a facet `partOf`/rollup read walks; each `BsddHit.Rank` is the server's own result ordinal, so a consumer re-ranks against the relevance the registry expressed rather than against a score the wire does not publish — so a new dictionary is one identity row across all consumers.
- Law: requiredness is a CAPABILITY, not a flag — `BsddProperty.Traits` is a `CapabilitySet<TemplateTrait>` over the kernel `Domain/validation#CAPABILITY` carrier, `Declared` holding whenever a source answered at all and `Required` joining it when that source demanded the property, so the EMPTY set is the third state (the offline buildingSMART floor states no requiredness column) a `bool` asserted "optional" over and an `Option<bool>` carried only by wrapping one. A read of the axis folds out for absence through `Admits`, which the kernel carrier declares lawful; the `Require` refusal twin has no site on these two pages, because the only presence consumer is `Semantics/properties#TEMPLATE_AUDIT`, whose product is a `TemplateVerdict` row rather than a fault. The bSDD WIRE keeps its `bool IsRequired` column verbatim — the `additionalProperties:false` contract declares it, and re-shaping a wire member to match a domain carrier forks the transcription.
- Law: the live lane's re-drive is the kernel's, not a hand loop — one `RedrivePolicy` value on this owner and one `Fetch` member every resource crosses, so the page declares CADENCE and BOUND and classifies nothing: `Redrive.Settle` reads the `Expected` case's own `Retriability`, so a Terminal registry refusal never re-drives. The port's `Fin` lane lifts ONTO the `IO` result before `Redrive` sees it, because a `Fin.Fail` carried inside a lifted `IO` is a successful `IO` holding a failure the curve never re-drives.
- Packages: Rasm.Element, LanguageExt.Core, QuikGraph, Thinktecture.Runtime.Extensions, Generator.Equals, Riok.Mapperly, Rasm, NodaTime
- Growth: a new bSDD dictionary is one `ClassificationSystem` row (stem + pin reader + shape) plus its `BsddPins` column; the live lookup is the same `BsddPort` transport boundary; the degradation is the row's local code-shape policy; a new dictionary-declared constraint is one read field on the `BsddClassResponse.ClassProperty` wire projected through `Property`, a new relation kind one `BsddRelationKind` member the closure filter reads, a new resource one query builder on `BsddResolution` (never a port member); a new result-free wire row is one `BsddWire` partial, never a hand-rolled transcription beside it; a newly declared template axis is one `TemplateTrait` row with no signature edit anywhere; a lane whose registry publishes a different cadence is the one `RedrivePolicy` value, never a second retry owner — never a parallel evidence record, never a per-system classifier, and never a `Rasm.Persistence` reference; federation growth is more resolved evidence folded into `Of`, zero type edits.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreached-endpoint degradation only; the port returns a THREE-state outcome (decoded body / unreached endpoint / undecodable body) and collapsing the last two into one failure is the deleted form that masks a contract drift as an offline miss and silently substitutes property-free local evidence for a class the registry answered for; the `BsddClass.Of` projection reads ONLY the fields the `.api/api-bsdd` catalog enumerates (the wire is `additionalProperties:false`, so an unexpected member signals contract drift, not a capability) and a field absent from the catalog is a phantom — the search wire publishes NO numeric relevance, so the hit's server-order ORDINAL is the rank column and a fabricated score is the deleted form; the SI exponent vector is the shared `Properties/quantity#DIMENSION` `Dimension` built through its own generated factory, and a page-local seven-int exponent record beside it is the deleted duplicate that forced every consumer to re-project one concept twice; the class-level constraint (`ClassProperty.AllowedValues`/`Min*`/`Max*`/`Pattern`) is read, never silently the property master, so a class that narrows an enumeration is honored; the cross-standard equivalence closure folds the `IsEqualTo`/`IsSynonymOf` relations through the shared `QuikGraph` substrate the folder admits — the relation is SYMMETRIC, so the container is undirected and the answer is `ConnectedComponents`, where a directed double-edge fold plus `ComputeTransitiveClosure` materialized the whole quadratic reachability edge set to recover a partition the linear pass yields directly; a hand-rolled BFS/union-find over a `Map<>` adjacency is the named rejected form (`libs/dotnet/.api/api-quikgraph.md`); the authoritative containment (`parentClassReference`/`hierarchy`) is read for a code that does NOT encode its parent (MasterFormat/Uniformat), and re-deriving containment from the code string where the dictionary states it is the rejected form; supersession gates authoring — `BsddClass.Admit` refuses a NEW code onto an `Inactive` class carrying the `ReplacedBy` code, and silently authoring a superseded class is the named defect; a `Certify` that resolves a class and then discards its code and concept title is the deleted form — the round-trip is paid, so the certified value carries the dictionary's own identity; the result-free wire rows transcribe through the ONE `BsddWire` `[Mapper]` and a hand-written `RefOf`/`AllowedValue` projection beside it is the deleted form, while every crossing that carries a `Fin` admission (`ValueKindOf`/`StatusOf`/`RelationOf`/`BoundsOf`) stays hand-written by law — Mapperly transcribes shape, never a lane; that same mapper owns the ONE wire absence admission — `Text` for a nullable string, `Rows` for a nullable array — so a `?? ""` or `?? []` at any projection below it is the deleted duplicate, and `Rows` is registered `Default = false` because a GENERIC mapping Mapperly may choose is the RMG001 form the folder refuses; the live fetch rides the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport injected as `BsddPort` (ONE generic `Fetch<TWire>` the page parameterizes by resource — a per-resource port member is the rejected form) and a transport minted here is the named contract violation; the port carries the caller's `CancellationToken` and the abort grain is DECLARED — one in-flight request boundary, a dispatched request running to its own completion — so a graph-scale fold that cannot be abandoned between round-trips is the deleted form and an unqualified cancellable claim over the request itself is the overclaim; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the boundary, never a `Rasm.Persistence` reference; the enrichment fold groups on the QUERY axes ALONE and a group key carrying an axis the request never sends is the deleted form that multiplies one round-trip by the cardinality of a token the server never sees; a bare system-token literal where `IfcSystem.Key` names the same value is the deleted form, and every system comparison runs `OrdinalIgnoreCase` per the roster's declared key space; the resolution degrades to the local policy on an unreached endpoint so INGEST never blocks (faulting on unreachability itself is the named defect) while the degraded verdict IS the row's shape gate — a shape-rejected code faults `BimFault.Refused` with `BimReason.Unmapped`, never a fabricated `Active` evidence the dictionary did not answer — and `Search` is authoring-only, so an unreached endpoint faults `BimFault.Refused` with `BimReason.Codec` because no offline concept-to-code resolution exists; port failures retain the exact `Error` because the compact Bim boundary axis declares no bSDD wrapper.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Generator.Equals;
using QuikGraph;
using QuikGraph.Algorithms;
using Riok.Mapperly.Abstractions;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Properties;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TemplateTrait : ICapability<TemplateTrait> {
    public static readonly TemplateTrait Declared = new("declared", rank: 0);
    public static readonly TemplateTrait Required = new("required", rank: 1);

    public int Rank { get; }

    private TemplateTrait(string key, int rank) : this(key) => Rank = rank;
}

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

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BsddBounds(Option<double> MinInclusive, Option<double> MaxInclusive, Option<double> MinExclusive, Option<double> MaxExclusive);

public sealed record BsddAllowedValue(string Value, string Code, string Description, string Uri);

public readonly record struct BsddRelation(BsddRelationKind Kind, string RelatedUri, string RelatedName, Option<double> Fraction);

public readonly record struct BsddRef(string Uri, string Name, string Code);

public readonly record struct BsddHit(ClassificationSystem System, string Code, string Name, string Uri, Seq<string> RelatedIfcEntities, int Rank);

public readonly record struct ClassificationSuggestion(NodeId Element, ClassificationSystem System, Seq<BsddHit> Candidates);

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
    public static Fin<BsddClass> Of(BsddClassResponse response) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-class-malformed", response.Uri })))
            : from status in StatusOf(response.Status)
              from properties in BsddWire.Rows(response.ClassProperties).TraverseM(p => Property(p)).As()
              from relations in BsddWire.Rows(response.ClassRelations).TraverseM(r => RelationOf(r)).As()
              from reverse in BsddWire.Rows(response.ReverseClassRelations).TraverseM(r => RelationOf(r)).As()
              select new BsddClass(
                  response.Code, response.Name, BsddWire.Text(response.ClassType), BsddWire.Text(response.Definition), response.Uri,
                  properties, status, BsddWire.Rows(response.RelatedIfcEntityNames), relations, reverse,
                  Optional(response.ParentClassReference).Map(BsddWire.Ref),
                  toSeq(BsddWire.Rows(response.Hierarchy).OrderBy(static item => item.Level)).Map(BsddWire.Ref),
                  BsddWire.Rows(response.ChildClassReferences).Map(BsddWire.Ref),
                  BsddWire.Rows(response.ReplacedObjectCodes),
                  BsddWire.Rows(response.ReplacingObjectCodes),
                  Optional(response.DeprecationExplanation).Filter(static s => s.Length > 0));

    public Fin<BsddClass> Admit() =>
        Status != BsddStatus.Inactive
            ? Fin.Succ(this)
            : Fin.Fail<BsddClass>(new BimFault.Refused(BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-superseded", Code, ReplacedBy.Head.IfNone("") })));

    static Fin<BsddProperty> Property(BsddClassResponse.ClassProperty p) =>
        from kind in ValueKindOf(p.PropertyValueKind)
        from status in StatusOf(p.PropertyStatus)
        select new BsddProperty(
            Optional(p.PropertyCode).Filter(static c => c.Length > 0).IfNone(p.Name), p.Name,
            BsddWire.Text(p.DataType), BsddWire.Text(p.PropertySet), BsddWire.Text(p.PredefinedValue),
            TraitsOf(p), kind, status, BsddWire.Rows(p.AllowedValues).Map(BsddWire.Allowed),
            Optional(p.Pattern).Filter(static s => s.Length > 0), BoundsOf(p), DimensionOf(p), BsddWire.Rows(p.Units));

    static CapabilitySet<TemplateTrait> TraitsOf(BsddClassResponse.ClassProperty p) =>
        p.IsRequired
            ? CapabilitySet<TemplateTrait>.Of(TemplateTrait.Declared, TemplateTrait.Required)
            : CapabilitySet<TemplateTrait>.Of(TemplateTrait.Declared);

    static Option<BsddBounds> BoundsOf(BsddClassResponse.ClassProperty p) =>
        p is { MinInclusive: null, MaxInclusive: null, MinExclusive: null, MaxExclusive: null }
            ? None
            : Some(new BsddBounds(Optional(p.MinInclusive), Optional(p.MaxInclusive), Optional(p.MinExclusive), Optional(p.MaxExclusive)));

    static Option<Dimension> DimensionOf(BsddClassResponse.ClassProperty p) =>
        Dimension.Create(
            BsddWire.Exponent(p.DimensionLength), BsddWire.Exponent(p.DimensionMass), BsddWire.Exponent(p.DimensionTime),
            BsddWire.Exponent(p.DimensionElectricCurrent), BsddWire.Exponent(p.DimensionThermodynamicTemperature),
            BsddWire.Exponent(p.DimensionAmountOfSubstance), BsddWire.Exponent(p.DimensionLuminousIntensity))
        is var dimension && dimension != Dimension.Dimensionless ? Some(dimension) : None;

    static Fin<BsddRelation> RelationOf(BsddClassResponse.ClassRelation relation) =>
        relation.RelatedClassUri is not { Length: > 0 }
            ? Fin.Fail<BsddRelation>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-relation-uri-missing" })))
            : BsddRelationKind.TryGet(relation.RelationType, out BsddRelationKind? kind) && kind is { } resolved
                ? Fin.Succ(new BsddRelation(resolved, relation.RelatedClassUri, BsddWire.Text(relation.RelatedClassName), Optional(relation.Fraction)))
                : Fin.Fail<BsddRelation>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "relation-kind", relation.RelationType })));

    static Fin<BsddValueKind> ValueKindOf(string? kind) =>
        BsddValueKind.TryGet(BsddWire.Text(kind), out BsddValueKind? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddValueKind>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "value-kind", kind })));

    static Fin<BsddStatus> StatusOf(string? status) =>
        BsddStatus.TryGet(BsddWire.Text(status), out BsddStatus? parsed) && parsed is { } value
            ? Fin.Succ(value)
            : Fin.Fail<BsddStatus>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-token-unmapped", "status", status })));
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class BsddWire {
    public static partial BsddRef Ref(BsddClassResponse.ClassReference reference);
    public static partial BsddRef Ref(BsddClassResponse.HierarchyItem item);
    public static partial BsddAllowedValue Allowed(BsddClassResponse.AllowedValue value);

    [UserMapping]
    internal static string Text(string? value) => value ?? "";

    [UserMapping]
    internal static int Exponent(int? value) => value ?? 0;

    [UserMapping(Default = false)]
    internal static Seq<TWire> Rows<TWire>(TWire[]? values) => values is null ? Seq<TWire>() : toSeq(values);
}

public sealed record BsddFederation(Map<string, Seq<string>> Equivalence, Map<string, string> Names) {
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

    public Seq<string> EquivalentSet(string classUri) => Equivalence.Find(classUri).IfNone(Seq(classUri));

    public bool Equivalent(string classUriA, string classUriB) =>
        string.Equals(classUriA, classUriB, StringComparison.OrdinalIgnoreCase) || EquivalentSet(classUriA).Contains(classUriB);

    public Fin<Seq<Classification>> Translate(Classification classification, ClassificationSystem target, BsddPins pins) =>
        ClassificationSystem.TryGet(classification.System, out ClassificationSystem? system) && system is { } row
            && target.Stem(pins) is { Length: > 0 } stem
            ? toSeq(EquivalentSet(row.ClassUri(classification.Code, pins))
                .Filter(uri => uri.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(identity))
                .TraverseM(uri => Classification.Of(ClassificationSystem.TailCode(uri), title: Names.Find(uri))).As()
            : Fin.Succ(Seq<Classification>());
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
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

public sealed record BsddSearchResponse(int TotalCount, int Offset, int Count, SearchClass[]? Classes) {
    public sealed record SearchClass(
        string? DictionaryUri, string? DictionaryName, string Name, string? ReferenceCode, string Uri,
        string? ClassType, string? Description, string? ParentClassName, string[]? RelatedIfcEntityNames);
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface BsddPort {
    Fin<Option<TWire>> Fetch<TWire>(string resource, CancellationToken token);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BsddResolution {
    static readonly RedrivePolicy Registry = RedrivePolicy.Of(
        law: Schedule.exponential(Duration.FromMilliseconds(250)) | Schedule.maxDelay(Duration.FromSeconds(4)), bound: 3);

    static Fin<Option<TWire>> Fetch<TWire>(BsddPort port, string resource, CancellationToken token) =>
        Try.lift(() => Redrive.Run(
            policy: Registry,
            work: IO.lift(() => Try.lift(() => port.Fetch<TWire>(resource, token)).Run().Bind(static inner => inner))).Run()).Run().Bind(static inner => inner);

    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token) =>
        Fetch<BsddClassResponse>(port, ClassResource(system.ClassUri(code, pins)), token)
            .Bind(reached => reached.Match(
                Some: response => BsddClass.Of(response),
                None: () => LocalShape(system, code, pins)));

    public static Fin<Classification> Certify(ClassificationSystem system, string code, BsddPort port, BsddPins pins, CancellationToken token) =>
        Resolve(system, code, port, pins, token)
            .Bind(cls => cls.Admit())
            .Bind(admitted => system.Classify(admitted.Code, Optional(admitted.Name).Filter(static n => n.Length > 0), pins));

    public static Fin<Seq<BsddHit>> Search(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPort port, BsddPins pins, CancellationToken token) =>
        Fetch<BsddSearchResponse>(port, SearchResource(text, scope, relatedIfcEntity, pins), token)
            .Bind(reached => reached.ToFin(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "bsdd-search-unreachable", text }))))
            .Map(response => BsddWire.Rows(response.Classes).Map((hit, index) => HitOf(hit, index, pins)).Somes());

    static string ClassResource(string classUri) =>
        $"api/Class/v1?Uri={System.Uri.EscapeDataString(classUri)}&IncludeClassProperties=true&IncludeClassRelations=true&IncludeReverseRelations=true&IncludeChildClassReferences=true";

    static string SearchResource(string text, Seq<ClassificationSystem> scope, Option<string> relatedIfcEntity, BsddPins pins) =>
        string.Concat(
            $"api/Class/Search/v1?SearchText={System.Uri.EscapeDataString(text.Trim())}",
            (scope.IsEmpty ? toSeq(ClassificationSystem.Items).Filter(row => row.Hosted(pins)) : scope)
                .Fold("", (acc, row) => $"{acc}&DictionaryUris={System.Uri.EscapeDataString(row.DictionaryUri(pins))}"),
            relatedIfcEntity.Match(Some: static entity => $"&RelatedIfcEntities={System.Uri.EscapeDataString(entity)}", None: static () => ""));

    public static Fin<Seq<ClassificationSuggestion>> Suggest(ElementGraph graph, Seq<ClassificationSystem> targets, BsddPort port, BsddPins pins, CancellationToken token) =>
        toSeq(graph.ObjectNodes
            .Filter(o => string.Equals(o.Classification.System, ClassificationSystem.IfcSystem.Key, StringComparison.OrdinalIgnoreCase)
                && o.Kind == ObjectKind.Occurrence
                && targets.Exists(target => !o.Classifications.Exists(c => string.Equals(c.System, target.Key, StringComparison.OrdinalIgnoreCase))))
            .GroupBy(static o => o.Classification.Code))
            .TraverseM(byClass => Search(targets, port, pins, token)
                .Bind(coarse => Decisive(coarse, targets)
                    ? Fin.Succ(Rows(toSeq(byClass), targets, coarse))
                    : toSeq(byClass.GroupBy(static o => o.Name is { Length: > 0 } name ? name : o.Classification.Code))
                        .TraverseM(byName => Search(targets, port, pins, token)
                            .Map(refined => Rows(toSeq(byName), targets, refined)))
                        .As()
                        .Map(static grouped => grouped.Flatten())))
            .As()
            .Map(static rows => toSeq(rows.Flatten()).Filter(static row => !row.Candidates.IsEmpty));

    static bool Decisive(Seq<BsddHit> coarse, Seq<ClassificationSystem> targets) =>
        targets.ForAll(target => coarse.Count(hit => hit.System == target) <= 1);

    static Seq<ClassificationSuggestion> Rows(Seq<Node.Object> elements, Seq<ClassificationSystem> targets, Seq<BsddHit> hits) =>
        elements.Bind(o => targets
            .Filter(target => !o.Classifications.Exists(c => string.Equals(c.System, target.Key, StringComparison.OrdinalIgnoreCase)))
            .Map(target => new ClassificationSuggestion(o.Id, target, hits.Filter(hit => hit.System == target))));

    static Option<BsddHit> HitOf(BsddSearchResponse.SearchClass hit, int index, BsddPins pins) =>
        hit.Uri is { Length: > 0 }
            ? ClassificationSystem.ByUri(Optional(hit.DictionaryUri).Filter(static u => u.Length > 0).IfNone(hit.Uri), pins)
                .Map(system => new BsddHit(
                    system,
                    Optional(hit.ReferenceCode).Filter(static c => c.Length > 0).IfNone(() => ClassificationSystem.TailCode(hit.Uri)),
                    hit.Name, hit.Uri, BsddWire.Rows(hit.RelatedIfcEntityNames), index))
            : None;

    static Fin<BsddClass> LocalShape(ClassificationSystem system, string code, BsddPins pins) =>
        system.Resolve(pins) is var row && row.Shape.IsMatch(code.Trim())
            ? Fin.Succ(new BsddClass(code, $"{row.Title}:{code}", "", "", $"{row.Stem}/class/{System.Uri.EscapeDataString(code.Trim())}", Seq<BsddProperty>(), BsddStatus.Active))
            : Fin.Fail<BsddClass>(new BimFault.Refused(BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "classification-code-reject", system.Key, code })));
}
```

## [04]-[RESEARCH]

(none)
