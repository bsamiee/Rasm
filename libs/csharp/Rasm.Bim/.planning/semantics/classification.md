# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification PROJECTOR over the `Rasm.Element` seam `Classification`: one `ClassificationSystem` `[SmartEnum<string>]` standard-systems vocabulary (Uniclass/OmniClass/MasterFormat/Uniformat/IfcClassification) each row carrying its bSDD dictionary URI and its code-shape policy, and `ClassificationSystem.Classify` validating a raw code against the system's shape and LOWERING it onto a seam `Classification(system, code)` value. The seam owns the generic `Classification/classification#CLASSIFICATION_AXIS` `Classification` `[ComplexValueObject]` (`System` + `Code`, library-neutral, no roster, no dictionary, no regex); this page is the downstream Bim projector the seam names — it owns the standard-systems roster, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, and lowers a resolved code onto the seam value at ingest. The stringly `Classification.Classify(BimElement element, ...)` binding and the `ClassificationRef`/`ClassificationCode` records the retired `BimElement` carried are GONE: a classification is the seam `Classification` value an `Object` node carries (the projector setting it) or the neutral `Projection/semantic#RELATION_ALGEBRA` `Associate` edge an `IfcRelAssociatesClassification` rides, never a `(GlobalId, system, code)` triple bound to a second stored element record. The bSDD resolution stays HERE — the live `BsddClass`/`BsddProperty` dictionary mapping feeds the `Semantics/properties#PROPERTY_SETS` `PropertyKey` template and the `Review/validation#IDS_FACETS` Classification facet directly, so a new standard is one `ClassificationSystem` dictionary-URI row shared across `classification`, `properties`, and `validation`. The page is HOST-NEUTRAL; the type name `ClassificationSystem` is distinct from the seam `Classification` so the seam value-object is the one canonical classification crossing a seam signature. The round-trip is BIDIRECTIONAL: `ClassificationSystem.Classify` lowers a validated code onto the seam value at ingress, and `ClassificationSystem.Author` is the egress the `Projection/semantic#IFC_EGRESS` `Emit` composes per `Object` node — authoring the standard classification onto `IfcRelAssociatesClassification`/`IfcClassificationReference`. This element-classification egress is the home for the `IfcClassificationReference` half the retired `Rasm.Materials` `MaterialPropertyWire.Classification` carrier used to author: a material carries no classification on the seam, the ELEMENT does (the `Object` node `Classification`), so the standard classification reads off the seam graph, never a Materials wire.

## [01]-[INDEX]

- [01]-[CLASSIFICATION_AXIS]: `ClassificationSystem` `[SmartEnum<string>]` the standard-systems vocabulary (system title, bSDD dictionary URI, code-shape policy), `ClassificationSystem.Classify` lowering a validated code onto a seam `Classification` value at ingress, and `ClassificationSystem.Author` the egress re-authoring an `Object` node's standard `Classification` onto `IfcRelAssociatesClassification`/`IfcClassificationReference` (the element-classification egress the `Projection/semantic#IFC_EGRESS` `Emit` composes).
- [02]-[BSDD_RESOLUTION]: the live bSDD dictionary class/property resolution (`BsddClass`/`BsddProperty`/`BsddPort`/`BsddResolution`) over Compute's transport, degrading to the row's local code-shape policy, feeding the `Semantics/properties#PROPERTY_SETS` template and the IDS Classification facet.

## [02]-[CLASSIFICATION_AXIS]

- Owner: `ClassificationSystem` the `[SmartEnum<string>]` standard classification-systems axis keyed on the system identifier, each row carrying the system title, the bSDD dictionary URI, and the code-shape regex policy; the projector that lowers a resolved standard code onto the seam `Classification/classification#CLASSIFICATION_AXIS` `Classification` value-object the `Object` node carries. The seam `Classification` is the library-neutral `(system, code)` pair; this page is the standard-systems authority the seam defers to — it validates the code shape and resolves the bSDD dictionary class, then lowers onto the seam value at ingress and authors the seam value back onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at egress, never re-declaring a classification value-object.
- Entry: `ClassificationSystem.Classify(string code)` validates the raw code against the system's code-shape regex and lowers it onto a seam `Classification(Key, code)` value — `Fin<T>` aborts on a code-shape mismatch (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`; the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes the success onto the seam `Object` node's `Classification` (for the standard-system stamp), and the import-side projection reads `IfcRelAssociatesClassification`/`IfcClassificationReference` at ingest; `ClassificationSystem.Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification)` is the egress entry the `Emit` composes per `Object` node — returning `None` for the `"ifc"` entity-type code (the `IfcClass` the object author already resolved) or an unrostered system, and otherwise authoring the `IfcRelAssociatesClassification` over an `IfcClassificationReference` whose `ReferencedSource` is the resolved system and `Identification` the code.
- Auto: `Classify` matches the trimmed code against the row's `CodeShape` regex (the actual shape enforcement the migration source's decorative `ClassificationCode` never applied) and, on a match, mints the seam `Classification.Create(Key, code)` (the seam `[ComplexValueObject]` factory normalizing the system token and trimming the code), so a `"Ss_25_10_30"` lowers onto a `Classification("uniclass2015", "Ss_25_10_30")` whose seam `Parent`/`Within` projections derive the containment hierarchy; a code the shape rejects faults `BimFault.UnmappedClass` rather than lowering a malformed value, and the bSDD class-to-property mapping resolves separately through the `[03]-[BSDD_RESOLUTION]` owner.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new classification standard is one `ClassificationSystem` row carrying its bSDD dictionary URI and code-shape regex; the seam `Classification` value-object absorbs any `(system, code)` pair with no seam edit; the bSDD lookup is the same dictionary that drives the IDS Classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type and never a parallel classification value-object beside the seam.
- Boundary: the classification systems are ONE keyed axis (`ClassificationSystem` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the classification VALUE is the seam `Classification` `[ComplexValueObject]` and a Bim `Classification`/`ClassificationCode`/`ClassificationRef` value-object is the deleted form — the seam owns the typed pair, this page owns the standard-systems roster and lowers onto it, so the type name `ClassificationSystem` never collides with the seam `Classification`; the `Classify(BimElement element, ...)` binding to a `BimElement.GlobalId` is GONE (the `BimElement`/`BimModel` are retired, the consumer element being the `Graph/element#ELEMENT_GRAPH` `Bake` fold) — a classification is the seam value on the `Object` node or the neutral `Associate` edge, never a `(GlobalId, system, code)` triple keyed to a second element record; the code shape is the row's regex validated once at `Classify`, never a per-call regex at the call site; the bSDD dictionary is the authoritative live source resolved through the dictionary URI, never a second hardcoded code-shape table that drifts from the dictionary (the regex is the unreachable-degradation fallback only); the classification round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`.api/api-geometrygym-ifc`) consumed as settled vocabulary, the import reading the entity onto the `Object` node `Classification` value and `ClassificationSystem.Author` re-authoring that value at egress, never re-minting a classification mapping or a stringly-keyed property lookup; the classification is the `Object` node `Classification` VALUE (`Relations/relation#EDGE_ALGEBRA` — classification is a value on the node, not an edge), so the egress reads it off the `Object` node, NOT a Materials `MaterialPropertyWire.Classification` carrier (retired), the material-wire classification half having moved to this element-classification egress.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text.RegularExpressions;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The standard classification-systems roster lowering onto the seam Classification value-object. The type
// name ClassificationSystem is distinct from the seam Classification so the seam value-object is the one
// canonical classification; this page owns only the roster (dictionary URI + code shape) and the bSDD lane.
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class ClassificationSystem {
    public static readonly ClassificationSystem Uniclass2015 = new("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", @"^[A-Z][a-z]_\d{2}(_\d{2}){0,3}$");
    public static readonly ClassificationSystem OmniClass     = new("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", @"^\d{2}-\d{2} \d{2} \d{2}$");
    public static readonly ClassificationSystem MasterFormat  = new("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", @"^\d{2} \d{2} \d{2}$");
    public static readonly ClassificationSystem Uniformat     = new("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", @"^[A-Z]\d{4}$");
    public static readonly ClassificationSystem IfcSystem     = new("ifc", "IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", @"^Ifc[A-Za-z]+$");

    public string Title { get; }
    public string DictionaryUri { get; }
    public string CodeShape { get; }

    // Validate the raw code against the system's code shape and lower onto a seam Classification value the
    // Object node carries (or the Associate edge rides) — the actual shape enforcement, then the seam factory.
    public Fin<Classification> Classify(string code) =>
        Regex.IsMatch(code.Trim(), CodeShape)
            ? Fin.Succ(Classification.Create(Key, code.Trim()))
            : Fin.Fail<Classification>(new BimFault.UnmappedClass($"classification-code-reject:{Key}:{code}").ToError());

    // The dictionary class URI the bSDD resolution fetches: the system's dictionary URI plus the URI-escaped code,
    // so an OmniClass/MasterFormat code carrying spaces (e.g. "23-13 35 00") produces a valid bSDD request URI.
    public string ClassUri(string code) => $"{DictionaryUri}/class/{System.Uri.EscapeDataString(code.Trim())}";

    // EGRESS: the element's standard Classification -> the IfcRelAssociatesClassification/IfcClassificationReference
    // the Projection/semantic#IFC_EGRESS Emit composes per Object node — this is the element-classification egress
    // that REPLACES the retired material-wire classification half (the Materials MaterialPropertyWire.Classification
    // case is gone; a material carries no classification, the element does, on the Object node Classification).
    // None for the "ifc" entity-type code (the IfcClass the object author already resolved via IfcClass.Resolve) or
    // an unrostered system, so only a real standard system authors a reference; the IfcRelAssociatesClassification
    // (IfcClassificationSelect, IfcDefinitionSelect) + IfcClassificationReference(db){ReferencedSource,Identification}
    // + IfcClassification(db, source) ctor surface is decompile-confirmed (.api/api-geometrygym-ifc rows 07/08).
    public static Option<IfcRelAssociatesClassification> Author(DatabaseIfc db, IfcDefinitionSelect related, Classification classification) =>
        classification.System == IfcSystem.Key
            ? Option<IfcRelAssociatesClassification>.None
            : TryGet(classification.System, out ClassificationSystem? row) && row is { } resolved
                ? Some(new IfcRelAssociatesClassification(
                    new IfcClassificationReference(db) { ReferencedSource = new IfcClassification(db, resolved.Title), Identification = classification.Code },
                    related))
                : Option<IfcRelAssociatesClassification>.None;
}
```

## [03]-[BSDD_RESOLUTION]

- Owner: `BsddResolution` the live bSDD dictionary class/property resolution over Compute's transport keyed on the `ClassificationSystem.DictionaryUri` and the code, degrading to the row's local code-shape policy when the service is unreachable so ingest never blocks on the dictionary; `BsddClass`/`BsddProperty` the resolved evidence the `Semantics/properties#PROPERTY_SETS` `PropertyKey.Resolve` template and the IDS Classification facet read.
- Entry: `BsddResolution.Resolve(ClassificationSystem system, string code, BsddPort port)` resolves the dictionary class shape and the bSDD class-to-property mapping over Compute's transport — `Fin<T>` returns the resolved `BsddClass` evidence on a live hit, and on a service-unreachable miss degrades to the row's local code-shape policy (`LocalShape`, never a fault on degradation, the local policy is the fallback); a malformed published-class shape the dictionary returns routes `Model/faults#FAULT_BAND` `BimFault.CodecReject` lowered with `.ToError()`.
- Auto: `Resolve` builds the dictionary-class request URI from `ClassificationSystem.ClassUri(code)`, issues it over the injected `BsddPort` (the Compute transport seam), and projects the bSDD `BsddClassResponse` (`Code`/`Name`/`ClassType`/`Uri`/`Definition` + `ClassProperties`) into the `BsddClass` evidence through `BsddClass.Of`, each `ClassProperty` (`PropertyCode`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired`) feeding the `Semantics/properties#PROPERTY_SETS` `PropertyKey` template; a transport miss degrades to `LocalShape` so a new standard becomes a dictionary-URI row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `Semantics/properties#PROPERTY_SETS` owner (`PropertyKey.Resolve(IfcDomain, BsddClass)` reading `BsddClass.Properties`) and the IDS Classification facet directly so a new dictionary is one URI row across all three.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new bSDD dictionary is one `ClassificationSystem` row carrying its `DictionaryUri`; the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; never a per-system classifier and never a `Rasm.Persistence` reference.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy being the unreachable-degradation fallback only; the live fetch rides the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport injected as `BsddPort` and a transport minted here is the named seam violation; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the resolution degrades to the local policy on a service miss so ingest never blocks on the dictionary, and a fault on degradation is the named defect — only a malformed published-class shape faults.

```csharp signature
public sealed record BsddClass(string Code, string Name, string ClassType, string Definition, string Uri, Seq<BsddProperty> Properties) {
    // The response -> evidence projection: a live hit that returns a class with no Code/Uri is INVALID published data,
    // not an offline dictionary miss, so it faults BimFault.CodecReject (surfaced, never masked as a LocalShape miss).
    // The transport-unreachable degradation is the caller's (Resolve) concern; this projection only judges payload shape.
    public static Fin<BsddClass> Of(BsddClassResponse response) =>
        string.IsNullOrWhiteSpace(response.Code) || string.IsNullOrWhiteSpace(response.Uri)
            ? Fin.Fail<BsddClass>(new BimFault.CodecReject($"bsdd-class-malformed:{response.Uri}").ToError())
            : Fin.Succ(new BsddClass(response.Code, response.Name, response.ClassType, response.Definition ?? "", response.Uri,
                (response.ClassProperties ?? []).ToSeq().Map(static p =>
                    new BsddProperty(p.PropertyCode ?? p.Code, p.Name, p.DataType ?? "", p.PropertySet ?? "", p.PredefinedValue ?? "", p.IsRequired))));
}

public sealed record BsddProperty(string Code, string Name, string DataType, string PropertySet, string PredefinedValue, bool IsRequired);

public sealed record BsddClassResponse(
    string Code, string Name, string ClassType, string Uri, string? Definition, BsddClassResponse.ClassProperty[]? ClassProperties) {
    public sealed record ClassProperty(string Code, string Name, string? PropertyCode, string? DataType, string? PropertySet, string? PredefinedValue, bool IsRequired);
}

public interface BsddPort {
    Fin<BsddClassResponse> Fetch(string dictionaryClassUri);
}

public static class BsddResolution {
    // Two failure axes, kept distinct: a TRANSPORT failure (the dictionary unreachable — Fetch fails) degrades to the
    // row's local code-shape policy so ingest never blocks, while a live hit carrying invalid/malformed data surfaces
    // BsddClass.Of's CodecReject instead of being silently masked as an offline miss. The Fetch failure is the only
    // genuine transport miss; the response projection owns the payload verdict.
    public static Fin<BsddClass> Resolve(ClassificationSystem system, string code, BsddPort port) =>
        port.Fetch(system.ClassUri(code)).Match(
            Succ: BsddClass.Of,
            Fail: _ => Fin.Succ(LocalShape(system, code)));

    static BsddClass LocalShape(ClassificationSystem system, string code) =>
        new(code, $"{system.Title}:{code}", "Class", "", system.ClassUri(code), Seq<BsddProperty>());
}
```

## [04]-[RESEARCH]

- [CLASSIFICATION_LOWERING]: the standard-systems roster lowering onto the seam `Classification` grounds against `ELEMENT-REBUILD-PLAN.md` §4B (the `Object` node carries a generic `Classification(system, code)` value-object, NOT `IfcClass`) and the seam `Classification/classification#CLASSIFICATION_AXIS` boundary ("a downstream `Rasm.Bim` projector owns the standard-systems vocabulary, the bSDD resolution, and the `IfcRelAssociatesClassification` round-trip, lowering a resolved code onto a seam `Classification` value at ingest") — so `ClassificationSystem.Classify` validates the code shape and mints the seam `Classification.Create(system, code)`, the seam owning the `Parent`/`Within` containment projections and this page owning only the roster; the type rename `Classification`→`ClassificationSystem` resolves the migration-source name collision with the seam value-object, and the `BimElement` binding (`ClassificationRef`/`ClassificationCode`) is retired with the `BimElement`/`BimModel` element records per §2/§4B.
- [BSDD_SERVICE_CONTRACT]: the bSDD RESTful dictionary class/property lookup response shape is GROUNDED against the published buildingSMART bSDD API contract — the `GET /api/Class/v1` resource returns the `BsddClassResponse` (`Code`/`Name`/`ClassType`/`Uri`/`Definition` plus the `ClassProperties` array), each `ClassProperty` carrying `Code`/`Name`/`PropertyCode`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired` (the `ClassType` constrained to `Class`/`Material`/`GroupOfProperties`/`AlternativeUse`, the `PropertySet` naming the IFC Pset placement) — so the `BsddPort.Fetch` response projection into `BsddClass` via `BsddClass.Of` matches the real wire members and feeds the `Semantics/properties#PROPERTY_SETS` `PropertyKey.Resolve(IfcDomain, BsddClass)` template directly. The LIVE-WIRE leg stays gated on the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport alignment (the `BsddPort` binding rides the injected Compute transport at the composition edge, never a transport minted here), and the in-process degradation to the row's local code-shape policy (`LocalShape`) is the verified settled fallback so ingest never blocks on the unreachable service.
- [CLASSIFICATION_ROUNDTRIP]: the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip member spellings confirm against the GeometryGym entity surface (`.api/api-geometrygym-ifc`) so a classified element re-authors its classification association on export; the import-side projection reads `RelatedObjects`, `RelatingClassification`, `ReferencedSource`, `Identification`, and `Location`, whose spellings confirm against the GeometryGym `IfcClassificationReference`/`IfcExternalReference` surface, the import landing the standard classification onto the seam `Object` node `Classification` value, never re-binding to a retired `BimElement.Classifications` column; the egress `ClassificationSystem.Author` is the inverse — the `Projection/semantic#IFC_EGRESS` `Emit` reads each `Object` node's `Classification` value and re-authors the `IfcRelAssociatesClassification`/`IfcClassificationReference` through the `IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect)` + `IfcClassificationReference(db){ReferencedSource, Identification}` + `IfcClassification(db, source)` ctor surface (decompile-confirmed), this element-classification egress subsuming the retired `Rasm.Materials` `MaterialPropertyWire.Classification` material-wire half (`ELEMENT-REBUILD-PLAN.md` §6 `Rasm.Materials` ripple — the material/property/classification egress reads the seam graph, never a Materials wire).
