# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification axis bound to the live buildingSMART Data Dictionary (bSDD): one `Classification` keyed vocabulary, one `ClassificationCode` value-object, and the `ClassificationRef` binding round-tripped through `IfcRelAssociatesClassification`/`IfcClassificationReference`. Each system row resolves a bSDD dictionary URI rather than only a hardcoded code-shape table, so a new standard is one dictionary-URI row and the bSDD class-to-property mapping feeds the `properties/property-sets#PROPERTY_SETS` owner directly.

## [1]-[INDEX]

- [2]-[CLASSIFICATION_AXIS]: `Classification` keyed systems axis, `ClassificationCode` value-object, `ClassificationRef` binding, and the local code-shape policy.
- [3]-[BSDD_RESOLUTION]: the live bSDD dictionary class/property resolution over Compute's transport, degrading to the row's local code-shape policy.

## [2]-[CLASSIFICATION_AXIS]

- Owner: `Classification` `[SmartEnum<string>]` the standard classification-systems axis keyed on the system identifier, each row carrying the system title, the bSDD dictionary URI, and the code-shape policy; `ClassificationCode` the `[ValueObject<string>]` code-within-a-system carrying its parent-code derivation; `ClassificationRef` the binding record tying a `ClassificationCode` to a `BimElement` GlobalId, projected from and round-tripped through `IfcRelAssociatesClassification`/`IfcClassificationReference`.
- Entry: `Classification.Classify(BimElement element, Classification system, string code)` validates the code against the system's code shape and resolves it against the bSDD dictionary, then binds it to the element — `Fin<T>` aborts on a code-shape mismatch (`faults#FAULT_BAND` `BimFault.UnmappedClass`) or an unresolved dictionary class (`BimFault.CodecReject` on a service-unreachable degrade), each lowered with `.ToError()`; classification projection from the `IfcSemanticModel` reads `IfcRelAssociatesClassification` at ingest.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new classification standard is one `Classification` row carrying its bSDD dictionary URI and code-shape policy; a new code-derivation rule is one column on `ClassificationCode`; the bSDD lookup is the same dictionary that drives the IDS classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type.
- Boundary: the classification systems are ONE keyed axis (`Classification` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the code shape is the row's policy validated once at `Classify`, never a per-call regex; the bSDD dictionary is the authoritative live source resolved through the dictionary URI, never a second hardcoded code-shape table that drifts from the dictionary; the classification binding round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`exchange/format-axis#FORMAT_AXIS` packages), consumed as settled vocabulary; ingest projects the `exchange/import-rail#IMPORT_RAIL` `IfcSemanticModel.ClassificationRow` family, which `model/elements#ELEMENT_MODEL` folds into the typed `BimElement.Classifications` `ClassificationRef` binding the `query/element-set#ELEMENT_SET` `ByClassification` arm reads, never re-minting a classification mapping or a stringly-keyed property lookup.

```csharp signature
[ValueObject<string>]
public sealed partial class ClassificationCode {
    static partial void NormalizeValidate(ref string value) => value = value.Trim();

    public Option<ClassificationCode> Parent =>
        Value.LastIndexOfAny(['_', ' ', '-']) is var cut and > 0
            ? Some(ClassificationCode.Create(Value[..cut]))
            : Option<ClassificationCode>.None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class Classification {
    public static readonly Classification Uniclass2015 = new("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015", @"^[A-Z][a-z]_\d{2}(_\d{2}){0,3}$");
    public static readonly Classification OmniClass = new("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass", @"^\d{2}-\d{2} \d{2} \d{2}$");
    public static readonly Classification MasterFormat = new("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat", @"^\d{2} \d{2} \d{2}$");
    public static readonly Classification Uniformat = new("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat", @"^[A-Z]\d{4}$");
    public static readonly Classification IfcClassification = new("ifc", "IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc", @"^Ifc[A-Za-z]+$");

    public string Title { get; }
    public string DictionaryUri { get; }
    public string CodeShape { get; }

    public Fin<ClassificationRef> Classify(BimElement element, string code) =>
        ClassificationCode.TryCreate(code).ToFin(new BimFault.UnmappedClass($"classification-code-reject:{Key}:{code}").ToError())
            .Map(value => new ClassificationRef(element.GlobalId, this, value, $"{DictionaryUri}/class/{value.Value}"));
}

public sealed record ClassificationRef(string ElementGlobalId, Classification System, ClassificationCode Code, string DictionaryClassUri);
```

## [3]-[BSDD_RESOLUTION]

- Owner: `BsddResolution` the live bSDD dictionary class/property resolution over Compute's transport, keyed on the `Classification.DictionaryUri` and the code, degrading to the row's local code-shape policy when the service is unreachable so ingest never blocks on the dictionary.
- Entry: `BsddResolution.Resolve(Classification system, ClassificationCode code, BsddPort port)` resolves the dictionary class shape and the bSDD class-to-property mapping over Compute's transport — `Fin<T>` returns the resolved `BsddClass` evidence on a live hit, and on a service-unreachable miss degrades to the row's local code-shape policy validating the code shape in-process (never a fault on degradation, the local policy is the fallback) so `Classification.Classify` composes the resolution after the local `ClassificationCode.TryCreate` admission; a malformed published-class shape the dictionary returns routes `faults#FAULT_BAND` `BimFault.CodecReject` lowered with `.ToError()`.
- Auto: `Resolve` builds the dictionary-class request URI from `Classification.DictionaryUri` plus the code, issues it over the injected `BsddPort` (the Compute transport seam), and projects the bSDD `BsddClassResponse` (`Code`/`Name`/`ClassType`/`Uri`/`Definition` + `ClassProperties`) into the `BsddClass` evidence through `BsddClass.Of`, each `ClassProperty` (`PropertyCode`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired`) feeding the `properties/property-sets#PROPERTY_SETS` `PropertyKey` template; a transport miss degrades to `LocalShape` validating the code against the row's local code-shape regex so a new standard becomes a dictionary-URI row, not a hardcoded code-shape table that drifts; the memoization keyed by dictionary URI rides Compute's transport, never a `Rasm.Persistence` reference.
- Receipt: the `BsddClass` is the authoritative classification evidence shared by `classification`, `properties`, and `validation`; the bSDD class-to-property mapping feeds the `properties/property-sets#PROPERTY_SETS` owner and the IDS Classification facet directly so a new dictionary is one URI row across all three.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new bSDD dictionary is one `Classification` row carrying its `DictionaryUri`; the live lookup is the same `BsddPort` transport seam; the degradation is the row's local code-shape policy; never a per-system classifier and never a `Rasm.Persistence` reference.
- Boundary: the bSDD dictionary is the authoritative live source resolved through the dictionary URI — a second hardcoded code-shape table that drifts from the dictionary is the rejected form, the local code-shape policy is the unreachable-degradation fallback only; the live fetch rides the `csharp:Compute/remote/channels#TRANSPORT_AXIS` transport injected as `BsddPort` and a transport minted here is the named seam violation; `Rasm.Bim` is AEC-domain and depends strictly upward, so the memoization rides Compute's transport and a durable cache is the calling app-platform's concern at the seam, never a `Rasm.Persistence` reference; the resolution degrades to the local policy on a service miss so ingest never blocks on the dictionary, and a fault on degradation is the named defect — only a malformed published-class shape faults.

```csharp signature
public sealed record BsddClass(string Code, string Name, string ClassType, string Definition, string Uri, Seq<BsddProperty> Properties) {
    public static BsddClass Of(BsddClassResponse response) =>
        new(response.Code, response.Name, response.ClassType, response.Definition ?? "", response.Uri,
            (response.ClassProperties ?? []).ToSeq().Map(static p =>
                new BsddProperty(p.PropertyCode ?? p.Code, p.Name, p.DataType ?? "", p.PropertySet ?? "", p.PredefinedValue ?? "", p.IsRequired)));
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
    public static Fin<BsddClass> Resolve(Classification system, ClassificationCode code, BsddPort port) {
        string classUri = $"{system.DictionaryUri}/class/{code.Value}";
        return port.Fetch(classUri).Map(BsddClass.Of).BindFail(_ => Fin.Succ(LocalShape(system, code)));
    }

    static BsddClass LocalShape(Classification system, ClassificationCode code) =>
        new(code.Value, $"{system.Title}:{code.Value}", "Class", "", $"{system.DictionaryUri}/class/{code.Value}", Seq<BsddProperty>());
}
```

## [4]-[RESEARCH]

- [BSDD_SERVICE_CONTRACT]: the bSDD RESTful dictionary class/property lookup response shape is GROUNDED against the published buildingSMART bSDD API contract — the `GET /api/Class/v1` resource returns the `BsddClassResponse` (`Code`/`Name`/`ClassType`/`Uri`/`Definition` plus the `ClassProperties` array), each `ClassProperty` carrying `Code`/`Name`/`PropertyCode`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired` (the `ClassType` constrained to `Class`/`Material`/`GroupOfProperties`/`AlternativeUse`, the `PropertySet` naming the IFC Pset placement), the `GET /api/Dictionary/v1` resource enumerating available dictionaries, and `GET /api/SearchInDictionary/v1` the class search — so the `BsddPort.Fetch` response projection into `BsddClass` via `BsddClass.Of` matches the real wire members. The LIVE-WIRE leg (the in-process transport that issues the request and streams the response) stays gated on the `csharp:Compute/remote/channels#TRANSPORT_AXIS` transport alignment — the `BsddPort` transport binding rides the injected Compute transport at the composition edge, never a transport minted here — and the in-process degradation to the row's local code-shape policy (`LocalShape`) is the verified settled present-tense fallback so ingest never blocks on the unreachable service.
- [CLASSIFICATION_ROUNDTRIP]: the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip member spellings confirm against the GeometryGym entity surface so a classified element re-authors its classification association on export; the import-side extract reads `RelatedObjects`, `RelatingClassification`, `ReferencedSource`, `Identification`, and `Location`, whose spellings confirm against the GeometryGym `IfcClassificationReference`/`IfcExternalReference` surface before the `exchange/import-rail#IMPORT_RAIL` `ClassificationRow` projection is final.
