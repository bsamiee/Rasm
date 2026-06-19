# [PY_GEOMETRY_API_IFCTESTER]

`ifctester` supplies IDS (Information Delivery Specification) authoring, parsing, and validation against IFC models: it exposes `Ids`/`Specification` document models, a facet family (`Entity`, `Attribute`, `Classification`, `Property`, `Material`, `PartOf`) with `Restriction` value constraints, and a reporter family (`Console`, `Html`, `Json`, `Bcf`, `Ods`, `Txt`) for structured compliance output.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifctester`
- package: `ifctester`
- module: `ifctester`
- asset: runtime library
- rail: ifc-companion / ids-validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document family
- rail: ids-validation

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------------- | :--------------- | :------------------------------------- |
|  [01]   | `Ids`                      | document root    | container for specifications           |
|  [02]   | `Specification`            | requirement unit | applicability + requirement facets     |
|  [03]   | `Restriction`              | value constraint | pattern/enumeration/range value filter |
|  [04]   | `IdsXmlValidationError`    | parse failure    | XSD schema validation error            |
|  [05]   | `XMLSchemaValidationError` | parse failure    | XML schema validation error            |

[PUBLIC_TYPE_SCOPE]: facet family
- rail: ids-validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                                   |
| :-----: | :--------------- | :-------------- | :--------------------------------------- |
|  [01]   | `Facet`          | base facet      | shared filter/match interface            |
|  [02]   | `Entity`         | entity facet    | IFC entity name + predefined type filter |
|  [03]   | `Attribute`      | attribute facet | entity attribute name + value filter     |
|  [04]   | `Classification` | class facet     | classification system + reference filter |
|  [05]   | `Property`       | property facet  | pset name + property name + value filter |
|  [06]   | `Material`       | material facet  | material name + URI filter               |
|  [07]   | `PartOf`         | partof facet    | spatial containment + relation filter    |

[PUBLIC_TYPE_SCOPE]: result family
- rail: ids-validation

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                            |
| :-----: | :--------------------- | :--------------- | :-------------------------------- |
|  [01]   | `Result`               | match result     | per-entity facet result carrier   |
|  [02]   | `EntityResult`         | entity result    | entity facet match result         |
|  [03]   | `AttributeResult`      | attribute result | attribute facet match result      |
|  [04]   | `PropertyResult`       | property result  | property facet match result       |
|  [05]   | `ClassificationResult` | class result     | classification facet match result |
|  [06]   | `MaterialResult`       | material result  | material facet match result       |
|  [07]   | `PartOfResult`         | partof result    | partof facet match result         |
|  [08]   | `FacetFailure`         | failure carrier  | failed entity + reason            |

[PUBLIC_TYPE_SCOPE]: reporter family
- rail: ids-validation output

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :----------- | :--------------- | :----------------------------------- |
|  [01]   | `Reporter`   | base reporter    | shared `report`/`write` interface    |
|  [02]   | `Console`    | console reporter | coloured terminal output             |
|  [03]   | `Html`       | HTML reporter    | browser-renderable validation report |
|  [04]   | `Json`       | JSON reporter    | machine-readable JSON output         |
|  [05]   | `Bcf`        | BCF reporter     | BCF issue export                     |
|  [06]   | `Ods`        | ODS reporter     | spreadsheet output                   |
|  [07]   | `OdsSummary` | ODS reporter     | summary spreadsheet output           |
|  [08]   | `Txt`        | text reporter    | plain text output                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IDS document I/O
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :------------------------------ |
|  [01]   | `open(filepath: str, validate: bool=False) -> Ids`                              | loader         | parse IDS XML file              |
|  [02]   | `from_string(xml: str, validate: bool=False) -> Ids`                            | loader         | parse IDS XML string            |
|  [03]   | `Ids(title, copyright, version, description, author, date, purpose, milestone)` | constructor    | new IDS document                |
|  [04]   | `Ids.to_xml() -> ...`                                                           | serializer     | serialize to XML bytes/string   |
|  [05]   | `Ids.to_string() -> str`                                                        | serializer     | serialize to XML string         |
|  [06]   | `Ids.parse(...)`                                                                | mutator        | parse and attach specifications |

[ENTRYPOINT_SCOPE]: validation
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                                      | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `Ids.validate(ifc_file, should_filter_version=False, filepath=None) -> None`                   | validator      | validate all specs against IFC model |
|  [02]   | `Specification.validate(ifc_file, should_filter_version=False) -> None`                        | validator      | validate one specification           |
|  [03]   | `Specification(name, minOccurs, maxOccurs, ifcVersion, identifier, description, instructions)` | constructor    | new specification                    |
|  [04]   | `Specification.check_ifc_version(ifc_file) -> bool`                                            | version check  | filter by IFC schema version         |
|  [05]   | `Specification.get_usage() / set_usage(...)`                                                   | cardinality    | applicability/requirement clause     |
|  [06]   | `Specification.reset_status()`                                                                 | mutator        | clear prior validation results       |

[ENTRYPOINT_SCOPE]: facet construction
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `Entity(name='IFCWALL', predefinedType=None, instructions=None)`                    | constructor    | entity facet                 |
|  [02]   | `Attribute(name='Name', value=None, cardinality='required', instructions=None)`     | constructor    | attribute facet              |
|  [03]   | `Classification(value, system, uri, cardinality='required', instructions=None)`     | constructor    | classification facet         |
|  [04]   | `Property(propertySet, baseName, value, dataType, uri, cardinality, instructions)`  | constructor    | property facet               |
|  [05]   | `Material(value, uri, cardinality='required', instructions=None)`                   | constructor    | material facet               |
|  [06]   | `PartOf(name, predefinedType, relation, cardinality='required', instructions=None)` | constructor    | partof facet                 |
|  [07]   | `Restriction(options=None, base='string')`                                          | constructor    | value restriction            |
|  [08]   | `Facet.filter(ifc_file, elements) -> list`                                          | method         | filter IFC entities by facet |

[ENTRYPOINT_SCOPE]: reporter output
- rail: ids-validation output

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------- | :------------- | :---------------------------- |
|  [01]   | `Reporter(ids: Ids)`               | constructor    | bind reporter to IDS document |
|  [02]   | `Reporter.report() -> ...`         | method         | collect validation results    |
|  [03]   | `Reporter.write(filepath) -> None` | method         | write report to file          |
|  [04]   | `Html.to_file(filepath) -> None`   | method         | write HTML report file        |
|  [05]   | `Console.print() -> None`          | method         | print report to stdout        |

## [04]-[IMPLEMENTATION_LAW]

[IDS_TOPOLOGY]:
- subpackages: `ids`, `facet`, `reporter`, `webapp`
- `Ids` contains `Specification` list; each `Specification` holds applicability facets and requirement facets
- `Cardinality` literal type: `'required'`, `'optional'`, `'prohibited'`
- `Restriction.base` controls value matching: `'string'`, `'decimal'`, `'integer'`, `'boolean'`
- reporter subclasses each override `report()` and `write()`; `Html` additionally provides `to_file()`

[LOCAL_ADMISSION]:
- Import top-level I/O from `ifctester.ids`: `open`, `from_string`, `Ids`, `Specification`, `Restriction`
- Import facet types from `ifctester.facet`: `Entity`, `Attribute`, `Classification`, `Property`, `Material`, `PartOf`
- Import reporters from `ifctester.reporter`: `Reporter`, `Console`, `Html`, `Json`, `Bcf`, `Ods`, `Txt`
- `Ids.validate` mutates `Specification` result state in place; call reporters after `validate`

[RAIL_LAW]:
- Package: `ifctester`
- Owns: IDS document authoring, facet-based IFC compliance validation, structured report export
- Accept: `ifcopenshell.file` as the target model for all validation calls
- Reject: custom IDS XML parsers or manual entity filtering that duplicates facet logic
