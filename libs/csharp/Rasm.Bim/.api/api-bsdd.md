# [RASM_BIM_API_BSDD]

The buildingSMART Data Dictionary (bSDD) is the live RESTful authority for standard classification systems (IFC, Uniclass, OmniClass, ETIM, …) and their class-to-property mappings. It is NOT a NuGet package: `Rasm.Bim` consumes it as a hand-thin read-only HTTP client — `BsddPort.Fetch` issues a `GET` over the injected `Compute/Runtime/channels` transport, content-negotiates `application/json`, and projects the wire `ClassContract.v1` into the seam-owned `BsddClass`/`BsddProperty` evidence that `Semantics/classification#BSDD_RESOLUTION` returns and `Semantics/properties#PROPERTY_TEMPLATES` mines for `PropertyKey` templates. This catalog records the exact base URL, the URI scheme, the GET resources with their query parameters, and the JSON response contracts so the `BsddClassResponse` projection matches the real wire and no phantom field is read.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: bSDD `Dictionaries API`
- kind: doc (hand-thin read-only REST client over the Compute transport `HttpClient`; no NuGet assembly, no managed members)
- service: buildingSMART Data Dictionary — `Dictionaries API`, OpenAPI 3.0.1, official version `v1`
- base-url: `https://api.bsdd.buildingsmart.org/` (official release); `https://test.bsdd.buildingsmart.org/` is the unsupported TEST environment, never shown to users
- identifier-uri: `https://identifier.buildingsmart.org/uri/{organization}/{dictionary}/{version}/class/{code}` is the canonical class identity passed AS the `Uri` query argument to `api/Class/v1`; navigating the identifier host directly for JSON is DEPRECATED (extra hop + uncontrolled API version) — always call `api/Class/vX`/`api/Property/vX`
- license: MIT (© 2020 buildingSMART International) — commercial-safe; data redistribution governed per-dictionary by `DictionaryContract.v1.license`/`licenseUrl`
- auth: the `Class`/`Property`/`Dictionary`/`Search`/reference GET resources are UNSECURED (a desktop client calls them with no token); only upload/private-content methods require Azure AD B2C (MSAL) — out of the Bim read path
- transport-headers: send `Accept: application/json` (the resource also negotiates `application/xml`, `text/turtle`, `application/rdf+xml`) and an `X-User-Agent` / `User-Agent` of `"application/version"` (e.g. `Rasm.Bim/<ver>`) so usage is attributable
- graphql: a POST GraphQL endpoint exists (`/graphqls` secured, `/graphql` test) — NOT used by the Bim read path; the typed REST resources own resolution
- rail: classification

## [02]-[ENDPOINTS]

[ENDPOINT_SCOPE]: class + property resolution (the Bim read path)
- rail: classification
- method: `GET` for every resource below; query parameters are URL-encoded; array parameters repeat the key

| [INDEX] | [RESOURCE]                   | [REQUIRED]            | [OPTIONAL]                                                                                   | [RESPONSE]                            |
| :-----: | :--------------------------- | :-------------------- | :------------------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `api/Class/v1`               | `Uri`                 | `IncludeClassProperties` `IncludeChildClassReferences` `IncludeClassRelations` `IncludeReverseRelations` `ReverseRelationDictionaryUris[]` `languageCode` | `ClassContract.v1`                    |
|  [02]   | `api/Class/Properties/v1`    | `ClassUri`            | `PropertySet` `PropertyCode` `SearchText` `Offset` `Limit` `languageCode`                    | `ClassPropertiesContract.v1` (paged)  |
|  [03]   | `api/Class/Relations/v1`     | `ClassUri` `GetReverseRelations` | `SearchText` `Offset` `Limit` `languageCode`                                      | `ClassRelationsContract.v1` (paged)   |
|  [04]   | `api/Class/Search/v1`        | `SearchText`          | `DictionaryUris[]` `RelatedIfcEntities[]` `Offset` `Limit`                                   | `ClassSearchResponseContract.v1`      |
|  [05]   | `api/SearchInDictionary/v1`  | `DictionaryUri`       | `SearchText` `LanguageCode` `RelatedIfcEntity` `Offset` `Limit`                              | `SearchInDictionaryResponseContract.v1` |
|  [06]   | `api/TextSearch/v2`          | `SearchText`          | `TypeFilter` `DictionaryUris[]` `OnlyLatestVersion` `OnlyVerified` `IncludeInactive` `IncludePreview` `IncludeSearchDescriptions` `Offset` `Limit` | `TextSearchResponseContract.v2`       |
|  [07]   | `api/Property/v5`            | `uri`                 | `languageCode`                                                                               | `PropertyContract.v5`                 |
|  [08]   | `api/PropertyValue/v2`       | `uri`                 | `languageCode`                                                                               | `PropertyValueContract.v4`            |

[ENDPOINT_SCOPE]: dictionary enumeration + reference data
- rail: classification

| [INDEX] | [RESOURCE]                    | [REQUIRED] | [OPTIONAL]                                                                          | [RESPONSE]                                  |
| :-----: | :---------------------------- | :--------- | :--------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `api/Dictionary/v1`           | —          | `Uri` `IncludeTestDictionaries` `Offset` `Limit`                                    | `DictionaryResponseContract.v1`             |
|  [02]   | `api/Dictionary/v1/Classes`   | `Uri`      | `UseNestedClasses` `ClassType` `SearchText` `RelatedIfcEntity` `RelatedIfcEntities[]` `Offset` `Limit` `languageCode` | `DictionaryClassesResponseContract.v1`      |
|  [03]   | `api/Dictionary/v1/Properties`| `Uri`      | `SearchText` `Offset` `Limit` `languageCode`                                        | `DictionaryPropertiesResponseContract.v1`   |
|  [04]   | `api/Unit/v1`                 | —          | —                                                                                  | `UnitContract.v1[]`                         |
|  [05]   | `api/Country/v1`              | —          | —                                                                                  | `CountryContract.v1[]`                      |
|  [06]   | `api/Language/v1`             | —          | —                                                                                  | `LanguageContract.v1[]`                     |
|  [07]   | `api/ReferenceDocument/v1`    | —          | —                                                                                  | `ReferenceDocumentContract.v1[]`            |
|  [08]   | `api/Health`                  | —          | —                                                                                  | `200 OK` liveness probe (no body)           |

[PARAMETER_LAW]:
- `Uri`/`ClassUri`/`uri` carry the FULL identifier URI (`{DictionaryUri}/class/{code}` for a class; `{DictionaryUri}/prop/{code}` for a property); the API resolves it server-side — this is the single addressing scheme `BsddPort.Fetch` builds.
- Paginated resources return `totalCount`/`offset`/`count`; the default and max `Limit` is `1000`, dropping to `100` once an `Offset` is set — drive the loop off `offset + count < totalCount`.
- `languageCode` is case-sensitive; a class text missing in the requested language falls back to the dictionary's `defaultLanguageCode`.
- `IncludeClassProperties=true` is mandatory on `api/Class/v1` for the property-template fold; without it `classProperties` is omitted and only the class header resolves.

## [03]-[RESPONSE_CONTRACTS]

[CONTRACT_SCOPE]: class detail + property + relation shapes
- rail: classification

| [INDEX] | [SYMBOL]                       | [SHAPE]                                                                                                  |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `ClassContract.v1`             | the class header + nested rollups; required `code`/`name`/`uri`/`status`/`activationDateUtc`/`versionDateUtc` |
|  [02]   | `ClassPropertyContract.v1`     | a class-scoped property binding (required `name`); the Pset placement + value constraints                |
|  [03]   | `ClassPropertyItemContract.v1` | the paginated `api/Class/Properties/v1` item — same fields as `ClassPropertyContract.v1`                 |
|  [04]   | `ClassPropertyValueContract.v1`| an allowed value: `value` (required) + `uri`/`code`/`description`/`sortNumber`                           |
|  [05]   | `ClassReferenceContract.v1`    | a parent/child class pointer: `uri` (required) + `name`/`code`                                           |
|  [06]   | `ClassRelationContract.v1`     | a forward relation: `relationType` + `relatedClassUri` (required) + `relatedClassName`/`fraction`/`uri` |
|  [07]   | `HierarchyItemContract.v1`     | an ancestor row to root: `level`/`name`/`code`/`uri`                                                     |

`ClassContract.v1` fields: `dictionaryUri`, `code`, `name`, `uri`, `status` (`Preview`|`Active`|`Inactive`), `classType` (`Class`|`Material`|`GroupOfProperties`|`AlternativeUse`), `referenceCode`, `definition`, `description`, `documentReference`, `creatorLanguageCode`, `countriesOfUse[]`, `countryOfOrigin`, `subdivisionsOfUse[]`, `synonyms[]`, `relatedIfcEntityNames[]` (version-independent IFC entity bindings), `replacedObjectCodes[]`/`replacingObjectCodes[]`, `deprecationExplanation`, `uid`, `visualRepresentationUri`, `revisionNumber`/`revisionDateUtc`, `versionNumber`/`versionDateUtc`, `activationDateUtc`/`deActivationDateUtc`, `parentClassReference` (`ClassReferenceContract.v1`), `classProperties[]` (`ClassPropertyContract.v1`), `classRelations[]` (`ClassRelationContract.v1`), `childClassReferences[]`, `reverseClassRelations[]`, `hierarchy[]` (`HierarchyItemContract.v1`).

`ClassPropertyContract.v1` fields: `name` (required), `propertyCode` (code within the property's own dictionary), `propertyUri`, `uri`, `propertySet` (the IFC Pset placement), `propertyDictionaryName`/`propertyDictionaryUri`, `propertyStatus`, `description`, `definition`, `example`, `symbol`, `dataType` (`Boolean`|`Character`|`Date`|`Enumeration`|`Integer`|`Real`|`String`|`Time`), `propertyValueKind` (`Single`|`Range`|`List`|`Complex`|`ComplexList`), `predefinedValue` (the single fixed value when the class fixes one), `allowedValues[]` (`ClassPropertyValueContract.v1` — the class-level-narrowed enumeration), `isRequired`, `isWritable`, `isDynamic`, `dynamicParameterPropertyCodes[]`, `physicalQuantity`, `units[]`, `qudtCodes[]`, `dimension` (the SI-exponent string `"L M T I Θ N J"`), `dimensionLength`/`dimensionMass`/`dimensionTime`/`dimensionElectricCurrent`/`dimensionThermodynamicTemperature`/`dimensionAmountOfSubstance`/`dimensionLuminousIntensity` (the seven exponents as `int`), `pattern` (XSD regex), `minInclusive`/`maxInclusive`/`minExclusive`/`maxExclusive` (`double` — class-level narrowing that may be stricter than the property-level bound). `ClassRelationContract.v1.relationType` ∈ `HasReference`|`IsEqualTo`|`IsSynonymOf`|`IsParentOf`|`IsChildOf`|`HasPart`.

[CONTRACT_SCOPE]: dictionary + search + reference shapes
- rail: classification

| [INDEX] | [SYMBOL]                                | [SHAPE]                                                                                              |
| :-----: | :-------------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `DictionaryResponseContract.v1`         | paged `{ totalCount, offset, count, dictionaries[] }` over `DictionaryContract.v1`                  |
|  [02]   | `DictionaryContract.v1`                 | dictionary header: `code`/`uri`/`name`/`version`/`organizationCodeOwner`/`organizationNameOwner`/`defaultLanguageCode` + flags |
|  [03]   | `ClassSearchResponseContract.v1`        | paged `{ totalCount, offset, count, classes[] }` over `ClassSearchResponseClassContract.v1`         |
|  [04]   | `SearchInDictionaryResponseContract.v1` | paged `{ totalCount, offset, count, dictionary }` (one `DictionarySearchResultContract.v1`)         |
|  [05]   | `TextSearchResponseContract.v2`         | paged `{ totalCount, offset, count, classes[], dictionaries[], properties[] }` (combined paging)    |
|  [06]   | `PropertyContract.v5`                   | the property master record (dictionary-level metadata + `allowedValues[]` + `propertyRelations[]`)  |
|  [07]   | `UnitContract.v1`                       | `code`/`name`/`symbol`/`qudtUri`                                                                    |
|  [08]   | `CountryContract.v1` / `LanguageContract.v1` / `ReferenceDocumentContract.v1` | `{code,name}` / `{isoCode,name}` / `{title,name,date}`        |

`DictionaryContract.v1` fields: `code`, `uri`, `name`, `version`, `organizationCodeOwner`, `organizationNameOwner`, `defaultLanguageCode`, `isLatestVersion`, `isVerified`, `isPrivate`, `status`, `license`, `licenseUrl`, `qualityAssuranceProcedure`/`qualityAssuranceProcedureUrl`, `changeRequestEmail`, `moreInfoUrl`, `releaseDate`, `lastUpdatedUtc`, `availableLanguages[]` (`{code,name}`). `ClassSearchResponseClassContract.v1` fields: `dictionaryUri`, `dictionaryName`, `name`, `referenceCode`, `uri`, `classType`, `description`, `parentClassName`, `relatedIfcEntityNames[]`. `TextSearchResponseClassContract.v2` adds `code`; `TextSearchResponseDictionaryContract.v2` carries `uri`/`code`/`name`/`version`/`organizationName`/`status`/`languages[]`/`isLatestVersion`/`isVerified`; `TextSearchResponsePropertyContract.v2` carries `dictionaryUri`/`uri`/`code`/`name`/`description`. `PropertyContract.v5` mirrors the `ClassPropertyContract.v1` value fields (`dataType`/`propertyValueKind`/`allowedValues[]` as `PropertyValueContract.v5`/`units[]`/`qudtCodes[]`/`dimension*`/`pattern`/`min*`/`max*`) plus dictionary-master metadata (`code`/`uri`/`status`/`versionDateUtc`/`connectedPropertyCodes[]`/`methodOfMeasurement`/`textFormat`/`propertyRelations[]`).

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the hand-thin client — `BsddPort.Fetch` request build + `BsddClass.Of` projection
- rail: classification
- surface-root: `Semantics/classification#BSDD_RESOLUTION` (`BsddPort`/`BsddResolution`/`BsddClass`/`BsddProperty`)

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                                                 |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `classUri = $"{Classification.DictionaryUri}/class/{code}"`              | the identifier URI is the `Uri` query argument — the one addressing scheme  |
|  [02]   | `GET {BsddApiUrl}/api/Class/v1?Uri={classUri}&IncludeClassProperties=true[&languageCode=]` | resolve a class header + its property bindings in one round trip |
|  [03]   | `Accept: application/json`, `X-User-Agent: Rasm.Bim/<ver>`               | content-negotiate JSON; attribute usage                                     |
|  [04]   | project `ClassContract.v1` → `BsddClass(code, name, classType, definition, uri, properties)` | `BsddClass.Of` reads only the verified header fields              |
|  [05]   | per `ClassPropertyContract.v1` → `BsddProperty(propertyCode ?? code, name, dataType, propertySet, predefinedValue, isRequired)` | the property-template seed |
|  [06]   | transport miss → `LocalShape` (the row's code-shape regex)              | degrade, never fault — ingest never blocks on the dictionary                |
|  [07]   | `GET api/Class/Search/v1?SearchText=&DictionaryUris=&RelatedIfcEntities=` | resolve a code from a label/IFC-entity search (authoring, not ingest)       |
|  [08]   | `GET api/Dictionary/v1[?Uri=]`                                           | enumerate dictionaries / pin a single dictionary version row                |

## [05]-[IMPLEMENTATION_LAW]

[BSDD_TOPOLOGY]:
- The class identity IS the identifier URI (`{org}/{dictionary}/{version}/class/{code}`); every resource is addressed by that URI, so `BsddPort.Fetch(string dictionaryClassUri)` carries exactly one argument and the API resolves the org/dictionary/version/code split server-side. A second hardcoded code-shape table that re-derives the class is the rejected form — the dictionary URI row is the single source.
- `api/Class/v1` with `IncludeClassProperties=true` returns the class header AND its `classProperties[]` in one round trip; a per-property second call to `api/Class/Properties/v1` is only for the paginated long tail (`> 1000` properties) or a `PropertySet`/`SearchText` filter — the default ingest path is the single class call.
- `ClassPropertyContract.v1` is the CLASS-scoped binding: its `allowedValues`/`min*`/`max*`/`pattern` may be STRICTER than the property-master (`PropertyContract.v5`) defaults; the seam reads the class-level constraint, never silently the master, so a class that narrows an enumeration is honored.
- A class fixes a value through `predefinedValue` (single fixed value) or `propertyValueKind` (`Single`/`Range`/`List`/`Complex`/`ComplexList`) with `allowedValues[]`; `BsddProperty` carries `predefinedValue` and the value kind so the `PropertyKey` template knows fixed-vs-selectable without re-fetching.
- `status` (`Preview`/`Active`/`Inactive`) gates admission: an `Inactive`/`Preview` class or property is resolvable but not authoritative — the seam keeps the status on the evidence and the IDS facet decides whether a preview binding is accepted.

[LOCAL_ADMISSION]:
- `BsddResolution.Resolve` issues the request over the INJECTED `Compute/Runtime/channels#TRANSPORT_AXIS` transport bound as `BsddPort`; a transport (`HttpClient`/socket) minted inside `Rasm.Bim` is the named seam violation — `Rasm.Bim` is AEC-domain and depends strictly upward, so the live leg rides Compute.
- The response projection reads ONLY the fields enumerated here; a field absent from this catalog is a phantom and must not be deserialized. The wire is `additionalProperties: false`, so an unexpected member signals a contract drift, not a new capability to read.
- A transport miss degrades to the row's local code-shape policy (`LocalShape`) — never a fault; only a MALFORMED published-class shape the dictionary returns routes `Model/faults#FAULT_BAND` `BimFault.CodecReject`. The unreachable service is a degrade; a corrupt response is a fault.
- Memoization keyed by the dictionary class URI rides Compute's transport cache, never a `Rasm.Persistence` reference; a durable classification cache is the calling app-platform's concern at the seam.

[STACKING]:
- `Semantics/properties#PROPERTY_TEMPLATES` (`api-geometrygym-ifc.md` Pset placement): `ClassPropertyContract.v1.propertySet` names the IFC Pset, `propertyCode` is the `PropertyKey`, `dataType`/`propertyValueKind`/`predefinedValue`/`allowedValues` drive the typed `PropertyValue` template, and `isRequired` marks the IDS requirement — one bSDD class call seeds the whole property-template roster a hardcoded Pset table would otherwise duplicate.
- `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`): the property `dimension` string (`"L M T I Θ N J"`) plus the seven `dimension*` exponents map to a `BaseDimensions` 7-vector, and `units[]`/`qudtCodes[]` select the source unit, so a measured property coerces to the SI-base `MeasureValue` through the UnitsNet quantity algebra rather than a stringly unit suffix — `Quantity.GetQuantitiesWithBaseDimensions(BaseDimensions)` resolves the quantity family from the dimension vector.
- `Review/validation` IDS facets (`api-ids-lib.md`, `api-xbim-informationspecifications.md`): the class-to-property mapping feeds BOTH the IDS Classification facet (the class URI is the facet value) and the property facet, and the bSDD `pattern`/`min*`/`max*`/`allowedValues` narrow directly into the `Xbim.InformationSpecifications` `ValueConstraint` — the same `BsddClass` evidence serves classification, properties, and validation, so a new dictionary is one URI row across all three.
- `GeometryGymIFC_Core` classification round-trip (`api-geometrygym-ifc.md`): the resolved `ClassContract.v1.uri` is the `IfcClassificationReference.Location`, the `code` is its `Identification`, and `relatedIfcEntityNames[]` aligns the bSDD class to the `IfcClass` entity — the binding re-authors through `IfcRelAssociatesClassification` on export with no re-resolution.

[RAIL_LAW]:
- Service: bSDD `Dictionaries API` (`https://api.bsdd.buildingsmart.org/`, MIT, OpenAPI `v1`)
- Owns: the live standard-classification dictionary, class detail (`ClassContract.v1`), the class-to-property mapping (`ClassPropertyContract.v1`), property masters (`PropertyContract.v5`), value enumerations, dictionary enumeration, and the unit/country/language/reference reference data — addressed by the identifier URI scheme.
- Accept: a single identifier-URI `GET` over the injected Compute transport, `application/json` negotiated, projected into the seam-owned `BsddClass`/`BsddProperty` evidence through `BsddClass.Of`; the class-level (not master) constraint; degrade-to-`LocalShape` on an unreachable service.
- Reject: a `Rasm.Bim`-minted transport; reading a field absent from this catalog; a second hardcoded code-shape table that drifts from the dictionary; a fault on service-unreachable degradation; the deprecated `identifier.buildingsmart.org` JSON hop for system-to-system calls; the secured GraphQL/upload methods in the read path.
