# [BIM_CLASSIFICATION_SYSTEMS]

The standard-systems classification axis bound to the live buildingSMART Data Dictionary (bSDD): one `Classification` keyed vocabulary, one `ClassificationCode` value-object, and the `ClassificationRef` binding round-tripped through `IfcRelAssociatesClassification`/`IfcClassificationReference`. Each system row resolves a bSDD dictionary URI rather than only a hardcoded code-shape table, so a new standard is one dictionary-URI row and the bSDD class-to-property mapping feeds the `properties/property-sets#PROPERTY_SETS` owner directly.

## [1]-[INDEX]

- [2]-[CLASSIFICATION_AXIS]: `Classification` keyed systems axis, `ClassificationCode` value-object, `ClassificationRef` binding, and the bSDD dictionary resolution.

## [2]-[CLASSIFICATION_AXIS]

- Owner: `Classification` `[SmartEnum<string>]` the standard classification-systems axis keyed on the system identifier, each row carrying the system title, the bSDD dictionary URI, and the code-shape policy; `ClassificationCode` the `[ValueObject<string>]` code-within-a-system carrying its parent-code derivation; `ClassificationRef` the binding record tying a `ClassificationCode` to a `BimElement` GlobalId, projected from and round-tripped through `IfcRelAssociatesClassification`/`IfcClassificationReference`.
- Entry: `Classification.Classify(BimElement element, Classification system, string code)` validates the code against the system's code shape and resolves it against the bSDD dictionary, then binds it to the element — `Fin<T>` aborts on a code-shape mismatch or an unresolved dictionary class; classification projection from the `IfcSemanticModel` reads `IfcRelAssociatesClassification` at ingest.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new classification standard is one `Classification` row carrying its bSDD dictionary URI and code-shape policy; a new code-derivation rule is one column on `ClassificationCode`; the bSDD lookup is the same dictionary that drives the IDS classification facet and the bSDD-referenced property definitions, so a new dictionary is one URI row shared across `classification`, `properties`, and `validation`; never a per-system classifier type.
- Boundary: the classification systems are ONE keyed axis (`Classification` SmartEnum) and a per-system `UniclassClassifier`/`OmniClassClassifier` type is the deleted form; the code shape is the row's policy validated once at `Classify`, never a per-call regex; the bSDD dictionary is the authoritative live source resolved through the dictionary URI, never a second hardcoded code-shape table that drifts from the dictionary; the classification binding round-trips through the `IfcRelAssociatesClassification`/`IfcClassificationReference` entities owned at the GeometryGym surface (`exchange/interchange#FORMAT_AXIS` packages), consumed as settled vocabulary; ingest projects the `exchange/interchange#IMPORT_RAIL` `IfcSemanticModel.ClassificationRow` family, which `model/elements#ELEMENT_MODEL` folds into the typed `BimElement.Classifications` `ClassificationRef` binding the `query/element-set#ELEMENT_SET` `ByClassification` arm reads, never re-minting a classification mapping or a stringly-keyed property lookup.

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
    public static readonly Classification Uniclass2015 = new("uniclass2015", "Uniclass 2015", "https://identifier.buildingsmart.org/uri/uniclass2015");
    public static readonly Classification OmniClass = new("omniclass", "OmniClass", "https://identifier.buildingsmart.org/uri/omniclass");
    public static readonly Classification MasterFormat = new("masterformat", "MasterFormat", "https://identifier.buildingsmart.org/uri/masterformat");
    public static readonly Classification Uniformat = new("uniformat", "Uniformat", "https://identifier.buildingsmart.org/uri/uniformat");
    public static readonly Classification IfcClassification = new("ifc", "IfcClassification", "https://identifier.buildingsmart.org/uri/buildingsmart/ifc");

    public string Title { get; }
    public string DictionaryUri { get; }

    public Fin<ClassificationRef> Classify(BimElement element, string code) =>
        ClassificationCode.TryCreate(code).ToFin(new BimFault.ModelRejected($"<classification-code-reject:{Key}:{code}>"))
            .Map(value => new ClassificationRef(element.GlobalId, this, value, $"{DictionaryUri}/class/{value.Value}"));
}

public sealed record ClassificationRef(string ElementGlobalId, Classification System, ClassificationCode Code, string DictionaryClassUri);
```

## [3]-[RESEARCH]

- [BSDD_RESOLUTION]: the bSDD RESTful/GraphQL dictionary class and property lookup — the dictionary-URI-to-class resolution, the published class shape validation, and the class-to-property mapping that feeds `properties/property-sets#PROPERTY_SETS` — grounds against the live bSDD service contract (ISO 12006-3/23386/23387, 300+ dictionaries); the `DictionaryUri`/`DictionaryClassUri` row members confirm against the bSDD identifier scheme and the resolution rides the `csharp:Compute/remote#TRANSPORT_AXIS` transport for the live dictionary fetch.
- [CLASSIFICATION_ROUNDTRIP]: the `IfcRelAssociatesClassification`/`IfcClassificationReference` round-trip member spellings confirm against the GeometryGym entity surface so a classified element re-authors its classification association on export; the import-side extract reads `RelatedObjects`, `RelatingClassification`, `ReferencedSource`, `Identification`, and `Location`, whose spellings confirm against the GeometryGym `IfcClassificationReference`/`IfcExternalReference` surface before the `exchange/interchange#IMPORT_RAIL` `ClassificationRow` projection is final.
