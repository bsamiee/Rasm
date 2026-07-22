# [RASM_BIM_API_BSDD]

bSDD — the buildingSMART Data Dictionary — is the live REST authority for standard classification systems (IFC, Uniclass, OmniClass, ETIM) and their class-to-property mappings, consumed as a hand-thin read-only HTTP client over the injected Compute transport. `BsddPort.Fetch` issues one identifier-URI `GET`, content-negotiates `application/json`, and projects the wire `ClassContract.v1` into the seam-owned `BsddClass`/`BsddProperty` evidence that classification resolution returns and the property templates mine for `PropertyKey` seeds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: bSDD `Dictionaries API`
- service: buildingSMART Data Dictionary `Dictionaries API` — live REST, OpenAPI `v1`
- base-url: `https://api.bsdd.buildingsmart.org/`
- identifier-uri: `https://identifier.buildingsmart.org/uri/{organization}/{dictionary}/{version}/class/{code}` — the canonical class identity, passed AS the `Uri` query argument to `api/Class/vX`/`api/Property/vX`
- license: MIT (© 2020 buildingSMART International); per-dictionary data redistribution governs at `DictionaryContract.v1.license`/`licenseUrl`
- auth: the `Class`/`Property`/`Dictionary`/`Search`/reference `GET` resources are token-free; upload and private-content methods require Azure AD B2C (MSAL), outside the read path
- transport-headers: `Accept: application/json` (the resource also serves `application/xml`, `text/turtle`, `application/rdf+xml`); an `X-User-Agent`/`User-Agent` of `"application/version"` attributes usage
- rail: classification

## [02]-[ENDPOINTS]

[ENDPOINT_SCOPE]: class + property resolution (the Bim read path)

Every resource is `GET`; query parameters URL-encode and an array parameter repeats its key.

| [INDEX] | [RESOURCE]                  | [REQUIRED]                       | [RESPONSE]                              |
| :-----: | :-------------------------- | :------------------------------- | :-------------------------------------- |
|  [01]   | `api/Class/v1`              | `Uri`                            | `ClassContract.v1`                      |
|  [02]   | `api/Class/Properties/v1`   | `ClassUri`                       | `ClassPropertiesContract.v1` (paged)    |
|  [03]   | `api/Class/Relations/v1`    | `ClassUri` `GetReverseRelations` | `ClassRelationsContract.v1` (paged)     |
|  [04]   | `api/Class/Search/v1`       | `SearchText`                     | `ClassSearchResponseContract.v1`        |
|  [05]   | `api/SearchInDictionary/v1` | `DictionaryUri`                  | `SearchInDictionaryResponseContract.v1` |
|  [06]   | `api/TextSearch/v2`         | `SearchText`                     | `TextSearchResponseContract.v2`         |
|  [07]   | `api/Property/v5`           | `uri`                            | `PropertyContract.v5`                   |
|  [08]   | `api/PropertyValue/v2`      | `uri`                            | `PropertyValueContract.v4`              |

Optional query parameters, keyed to the rows above:
- [01]: `IncludeClassProperties` `IncludeChildClassReferences` `IncludeClassRelations` `IncludeReverseRelations` `ReverseRelationDictionaryUris[]` `languageCode`.
- [02]: `PropertySet` `PropertyCode` `SearchText` `Offset` `Limit` `languageCode`.
- [03]: `SearchText` `Offset` `Limit` `languageCode`.
- [04]: `DictionaryUris[]` `RelatedIfcEntities[]` `Offset` `Limit`.
- [05]: `SearchText` `LanguageCode` `RelatedIfcEntity` `Offset` `Limit`.
- [06]: `TypeFilter` `DictionaryUris[]` `OnlyLatestVersion` `OnlyVerified` `IncludeInactive` `IncludePreview` `IncludeSearchDescriptions` `Offset` `Limit`.
- [07]-[08]: `languageCode`.

[ENDPOINT_SCOPE]: dictionary enumeration + reference data

| [INDEX] | [RESOURCE]                     | [REQUIRED] | [RESPONSE]                                |
| :-----: | :----------------------------- | :--------- | :---------------------------------------- |
|  [01]   | `api/Dictionary/v1`            | —          | `DictionaryResponseContract.v1`           |
|  [02]   | `api/Dictionary/v1/Classes`    | `Uri`      | `DictionaryClassesResponseContract.v1`    |
|  [03]   | `api/Dictionary/v1/Properties` | `Uri`      | `DictionaryPropertiesResponseContract.v1` |
|  [04]   | `api/Unit/v1`                  | —          | `UnitContract.v1[]`                       |
|  [05]   | `api/Country/v1`               | —          | `CountryContract.v1[]`                    |
|  [06]   | `api/Language/v1`              | —          | `LanguageContract.v1[]`                   |
|  [07]   | `api/ReferenceDocument/v1`     | —          | `ReferenceDocumentContract.v1[]`          |
|  [08]   | `api/Health`                   | —          | `200 OK` liveness probe (no body)         |

Optional query parameters, keyed to the rows above:
- [01]: `Uri` `IncludeTestDictionaries` `Offset` `Limit`.
- [02]: `UseNestedClasses` `ClassType` `SearchText` `RelatedIfcEntity` `RelatedIfcEntities[]` `Offset` `Limit` `languageCode`.
- [03]: `SearchText` `Offset` `Limit` `languageCode`.
- [04]-[08]: none.

[PARAMETER_LAW]:
- `Uri`/`ClassUri`/`uri` carry the FULL identifier URI (`{DictionaryUri}/class/{code}` for a class, `{DictionaryUri}/prop/{code}` for a property); the API resolves it server-side, so `BsddPort.Fetch` builds one addressing scheme.
- Paginated resources return `totalCount`/`offset`/`count`; `Limit` defaults and maxes at `1000`, dropping to `100` once an `Offset` is set — drive the loop off `offset + count < totalCount`.
- `languageCode` is case-sensitive; a class text missing in the requested language falls back to the dictionary's `defaultLanguageCode`.
- `IncludeClassProperties=true` is mandatory on `api/Class/v1` for the property-template fold; absent it, `classProperties` is omitted and only the class header resolves.

## [03]-[RESPONSE_CONTRACTS]

[CONTRACT_SCOPE]: class detail + property + relation shapes

| [INDEX] | [SYMBOL]                        | [SHAPE]                                                                                               |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `ClassContract.v1`              | class header + rollups; required `code`/`name`/`uri`/`status`/`activationDateUtc`/`versionDateUtc`    |
|  [02]   | `ClassPropertyContract.v1`      | class-scoped property binding (required `name`); Pset placement + value constraints                   |
|  [03]   | `ClassPropertyItemContract.v1`  | paginated `api/Class/Properties/v1` item — same fields as `ClassPropertyContract.v1`                  |
|  [04]   | `ClassPropertyValueContract.v1` | an allowed value: `value` (required) + `uri`/`code`/`description`/`sortNumber`                        |
|  [05]   | `ClassReferenceContract.v1`     | a parent/child class pointer: `uri` (required) + `name`/`code`                                        |
|  [06]   | `ClassRelationContract.v1`      | forward relation: `relationType` + `relatedClassUri` (required) + `relatedClassName`/`fraction`/`uri` |
|  [07]   | `HierarchyItemContract.v1`      | an ancestor row to root: `level`/`name`/`code`/`uri`                                                  |

[ClassContract.v1]: `dictionaryUri` `code` `name` `uri` `status`(`Preview`|`Active`|`Inactive`) `classType`(`Class`|`Material`|`GroupOfProperties`|`AlternativeUse`) `referenceCode` `definition` `description` `documentReference` `creatorLanguageCode` `countriesOfUse[]` `countryOfOrigin` `subdivisionsOfUse[]` `synonyms[]` `relatedIfcEntityNames[]` `replacedObjectCodes[]` `replacingObjectCodes[]` `deprecationExplanation` `uid` `visualRepresentationUri` `revisionNumber` `revisionDateUtc` `versionNumber` `versionDateUtc` `activationDateUtc` `deActivationDateUtc` `parentClassReference`(`ClassReferenceContract.v1`) `classProperties[]`(`ClassPropertyContract.v1`) `classRelations[]`(`ClassRelationContract.v1`) `childClassReferences[]` `reverseClassRelations[]` `hierarchy[]`(`HierarchyItemContract.v1`).

[ClassPropertyContract.v1]: `name`(required) `propertyCode` `propertyUri` `uri` `propertySet` `propertyDictionaryName` `propertyDictionaryUri` `propertyStatus` `description` `definition` `example` `symbol` `dataType`(`Boolean`|`Character`|`Date`|`Enumeration`|`Integer`|`Real`|`String`|`Time`) `propertyValueKind`(`Single`|`Range`|`List`|`Complex`|`ComplexList`) `predefinedValue` `allowedValues[]`(`ClassPropertyValueContract.v1`) `isRequired` `isWritable` `isDynamic` `dynamicParameterPropertyCodes[]` `physicalQuantity` `units[]` `qudtCodes[]` `dimension`(`"L M T I Θ N J"`) `dimensionLength` `dimensionMass` `dimensionTime` `dimensionElectricCurrent` `dimensionThermodynamicTemperature` `dimensionAmountOfSubstance` `dimensionLuminousIntensity`(seven `int` exponents) `pattern`(XSD regex) `minInclusive` `maxInclusive` `minExclusive` `maxExclusive`(`double`).

[ClassRelationContract.v1.relationType]: `HasReference`|`IsEqualTo`|`IsSynonymOf`|`IsParentOf`|`IsChildOf`|`HasPart`.

[CONTRACT_SCOPE]: dictionary + search + reference shapes; every paged response wraps `{ totalCount, offset, count, <items> }`

| [INDEX] | [SYMBOL]                                | [SHAPE]                                                                               |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `DictionaryResponseContract.v1`         | paged `dictionaries[]` over `DictionaryContract.v1`                                   |
|  [02]   | `DictionaryContract.v1`                 | dictionary header (`code`/`uri`/`name`/`version`/owner/`defaultLanguageCode` + flags) |
|  [03]   | `ClassSearchResponseContract.v1`        | paged `classes[]` over `ClassSearchResponseClassContract.v1`                          |
|  [04]   | `SearchInDictionaryResponseContract.v1` | paged `dictionary` (`DictionarySearchResultContract.v1`)                              |
|  [05]   | `TextSearchResponseContract.v2`         | paged combined `classes[]`/`dictionaries[]`/`properties[]`                            |
|  [06]   | `PropertyContract.v5`                   | property master (dictionary metadata + `allowedValues[]` + `propertyRelations[]`)     |
|  [07]   | `UnitContract.v1`                       | `code`/`name`/`symbol`/`qudtUri`                                                      |
|  [08]   | `CountryContract.v1`                    | `{code,name}`                                                                         |
|  [09]   | `LanguageContract.v1`                   | `{isoCode,name}`                                                                      |
|  [10]   | `ReferenceDocumentContract.v1`          | `{title,name,date}`                                                                   |

[DictionaryContract.v1]: `code` `uri` `name` `version` `organizationCodeOwner` `organizationNameOwner` `defaultLanguageCode` `isLatestVersion` `isVerified` `isPrivate` `status` `license` `licenseUrl` `qualityAssuranceProcedure` `qualityAssuranceProcedureUrl` `changeRequestEmail` `moreInfoUrl` `releaseDate` `lastUpdatedUtc` `availableLanguages[]`(`{code,name}`).

[ClassSearchResponseClassContract.v1]: `dictionaryUri` `dictionaryName` `name` `referenceCode` `uri` `classType` `description` `parentClassName` `relatedIfcEntityNames[]`.

[TextSearchResponse*.v2]: `TextSearchResponseClassContract.v2` adds `code`; `TextSearchResponseDictionaryContract.v2` carries `uri`/`code`/`name`/`version`/`organizationName`/`status`/`languages[]`/`isLatestVersion`/`isVerified`; `TextSearchResponsePropertyContract.v2` carries `dictionaryUri`/`uri`/`code`/`name`/`description`.

[PropertyContract.v5]: mirrors the `ClassPropertyContract.v1` value fields (`dataType`/`propertyValueKind`/`allowedValues[]` as `PropertyValueContract.v5`/`units[]`/`qudtCodes[]`/`dimension*`/`pattern`/`min*`/`max*`) with dictionary-master metadata `code`/`uri`/`status`/`versionDateUtc`/`connectedPropertyCodes[]`/`methodOfMeasurement`/`textFormat`/`propertyRelations[]`.

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the hand-thin client — `BsddPort.Fetch` request build + `BsddClass.Of` projection; `{classUri}` = `{DictionaryUri}/class/{code}`
- surface-root: `Semantics/classification#BSDD_RESOLUTION` (`BsddPort`/`BsddResolution`/`BsddClass`/`BsddProperty`)

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `api/Class/v1?Uri={classUri}&IncludeClassProperties=true` | class header + property bindings, one round trip            |
|  [02]   | `ClassContract.v1` → `BsddClass`                          | full verified projection — see `[BSDD_CLASS]`               |
|  [03]   | `ClassPropertyContract.v1` → `BsddProperty`               | constraint-surface template seed — see `[BSDD_PROPERTY]`    |
|  [04]   | transport miss → `LocalShape`                             | degrade to the code-shape policy, never fault               |
|  [05]   | `api/Class/Search/v1?SearchText=…`                        | resolve a code from a label / IFC-entity search (authoring) |
|  [06]   | `api/Dictionary/v1[?Uri=]`                                | enumerate dictionaries / pin one version                    |

[BSDD_CLASS]: `BsddClass(code, name, classType, definition, uri, properties, status, relatedIfcEntities, relations, reverseRelations, parent, ancestry, children, replaces, replacedBy, deprecation)` — `Status` gates IDS admission, `Relations`/`ReverseRelations` feed the `BsddFederation` closure, `Parent`/`Ancestry`/`Children` the containment, `Replaces`/`ReplacedBy`/`Deprecation` the supersession the `Admit` gate reads.
[BSDD_PROPERTY]: `BsddProperty(propertyCode ?? code, name, dataType, propertySet, predefinedValue, isRequired, valueKind, allowedValues, pattern, bounds, exponents, units, status)` — `AllowedValues`/`Pattern`/`Bounds`/`SiExponents`/`Units` feed the `Semantics/properties` templates and IDS value constraints.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Class identity IS the identifier URI (`{org}/{dictionary}/{version}/class/{code}`); every resource is addressed by that URI, so `BsddPort.Fetch(string dictionaryClassUri)` carries one argument and the API resolves the org/dictionary/version/code split server-side.
- `api/Class/v1` with `IncludeClassProperties=true` returns the class header AND its `classProperties[]` in one round trip; a second `api/Class/Properties/v1` call serves only the paginated long tail (`> 1000` properties) or a `PropertySet`/`SearchText` filter, so the default ingest is the single class call.
- `ClassPropertyContract.v1` is the CLASS-scoped binding: its `allowedValues`/`min*`/`max*`/`pattern` may be STRICTER than the `PropertyContract.v5` master defaults, and the seam reads the class-level constraint, so a class that narrows an enumeration is honored.
- A class fixes a value through `predefinedValue` (single fixed value) or `propertyValueKind` (`Single`/`Range`/`List`/`Complex`/`ComplexList`) with `allowedValues[]`; `BsddProperty` carries both, so a `PropertyKey` template knows fixed-vs-selectable without re-fetching.
- `status` (`Preview`/`Active`/`Inactive`) gates admission: an `Inactive`/`Preview` class or property resolves but is not authoritative, so the seam keeps the status on the evidence and the IDS facet decides whether a preview binding is accepted.

[STACKING]:
- `Semantics/properties#PROPERTY_TEMPLATES`: `ClassPropertyContract.v1.propertySet` names the IFC Pset, `propertyCode` is the `PropertyKey`, `dataType`/`propertyValueKind`/`predefinedValue`/`allowedValues` drive the typed `PropertyValue` template, and `isRequired` marks the IDS requirement — one class call seeds the whole property-template roster.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): the `dimension` string (`"L M T I Θ N J"`) and the seven `dimension*` exponents map to a `BaseDimensions` 7-vector, and `units[]`/`qudtCodes[]` select the source unit, so a measured property coerces to the SI-base `MeasureValue` through `Quantity.GetQuantitiesWithBaseDimensions(BaseDimensions)`.
- IDS facets (`api-ids-lib.md`, `api-xbim-informationspecifications.md`): the class-to-property mapping feeds both the IDS Classification facet (the class URI is the facet value) and the property facet, and the bSDD `pattern`/`min*`/`max*`/`allowedValues` narrow directly into the `Xbim.InformationSpecifications` `ValueConstraint`, so one `BsddClass` serves classification, properties, and validation.
- `GeometryGymIFC_Core`(`api-geometrygym-ifc.md`): the resolved `ClassContract.v1.uri` is the `IfcClassificationReference.Location`, the `code` its `Identification`, and `relatedIfcEntityNames[]` aligns the bSDD class to the `IfcClass` entity, re-authored through `IfcRelAssociatesClassification` on export with no re-resolution.

[LOCAL_ADMISSION]:
- `BsddResolution.Resolve` issues the request over the INJECTED `Compute/Runtime/transport#TRANSPORT_AXIS` transport bound as `BsddPort`; `Rasm.Bim` is AEC-domain and depends strictly upward, so the live leg rides Compute and a transport minted inside `Rasm.Bim` is the named seam violation.
- `BsddClass.Of` reads ONLY the fields enumerated here; the wire is `additionalProperties: false`, so an unexpected member signals contract drift, not a new capability, and a field absent from this catalog is a phantom the seam never deserializes.
- A transport miss degrades to the row's local code-shape policy (`LocalShape`); only a MALFORMED published-class shape routes `Model/faults#FAULT_BAND` `BimFault.CodecReject` — an unreachable service is a degrade, a corrupt response is a fault.
- Memoization keyed by the dictionary class URI rides Compute's transport cache; a durable classification cache is the calling app-platform's concern at the seam.

[RAIL_LAW]:
- Service: bSDD `Dictionaries API` (`https://api.bsdd.buildingsmart.org/`, MIT, OpenAPI `v1`)
- Owns: the live standard-classification dictionary — class detail (`ClassContract.v1`), the class-to-property mapping (`ClassPropertyContract.v1`), property masters (`PropertyContract.v5`), value enumerations, dictionary enumeration, and the unit/country/language/reference data — addressed by the identifier-URI scheme.
- Accept: a single identifier-URI `GET` over the injected Compute transport, `application/json` negotiated, projected into the seam-owned `BsddClass`/`BsddProperty` through `BsddClass.Of`; the class-level constraint over the master; degrade-to-`LocalShape` on an unreachable service.
- Reject: a `Rasm.Bim`-minted transport; reading a field absent from this catalog; a second hardcoded code-shape table drifting from the dictionary; a fault on service-unreachable degradation.
