# [PY_GEOMETRY_API_IFCTESTER]

`ifctester` supplies IDS (Information Delivery Specification) authoring, parsing, and validation against IFC models: it exposes `Ids`/`Specification` document models, a facet family (`Entity`, `Attribute`, `Classification`, `Property`, `Material`, `PartOf`) with `Restriction` value constraints, and a reporter family (`Console`, `Html`, `Json`, `Bcf`, `Ods`, `Txt`) for structured compliance output.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifctester`
- package: `ifctester`
- module: `ifctester`
- asset: runtime library
- rail: ifc-companion / ids-validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document family
- rail: ids-validation

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------------- | :--------------- | :------------------------------------- |
|   [1]   | `Ids`                      | document root    | container for specifications           |
|   [2]   | `Specification`            | requirement unit | applicability + requirement facets     |
|   [3]   | `Restriction`              | value constraint | pattern/enumeration/range value filter |
|   [4]   | `IdsXmlValidationError`    | parse failure    | XSD schema validation error            |
|   [5]   | `XMLSchemaValidationError` | parse failure    | XML schema validation error            |

[PUBLIC_TYPE_SCOPE]: facet family
- rail: ids-validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                                   |
| :-----: | :--------------- | :-------------- | :--------------------------------------- |
|   [1]   | `Facet`          | base facet      | shared filter/match interface            |
|   [2]   | `Entity`         | entity facet    | IFC entity name + predefined type filter |
|   [3]   | `Attribute`      | attribute facet | entity attribute name + value filter     |
|   [4]   | `Classification` | class facet     | classification system + reference filter |
|   [5]   | `Property`       | property facet  | pset name + property name + value filter |
|   [6]   | `Material`       | material facet  | material name + URI filter               |
|   [7]   | `PartOf`         | partof facet    | spatial containment + relation filter    |

[PUBLIC_TYPE_SCOPE]: result family
- rail: ids-validation

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                            |
| :-----: | :--------------------- | :--------------- | :-------------------------------- |
|   [1]   | `Result`               | match result     | per-entity facet result carrier   |
|   [2]   | `EntityResult`         | entity result    | entity facet match result         |
|   [3]   | `AttributeResult`      | attribute result | attribute facet match result      |
|   [4]   | `PropertyResult`       | property result  | property facet match result       |
|   [5]   | `ClassificationResult` | class result     | classification facet match result |
|   [6]   | `MaterialResult`       | material result  | material facet match result       |
|   [7]   | `PartOfResult`         | partof result    | partof facet match result         |
|   [8]   | `FacetFailure`         | failure carrier  | failed entity + reason            |

[PUBLIC_TYPE_SCOPE]: reporter family
- rail: ids-validation output

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :----------- | :--------------- | :----------------------------------- |
|   [1]   | `Reporter`   | base reporter    | shared `report`/`write` interface    |
|   [2]   | `Console`    | console reporter | coloured terminal output             |
|   [3]   | `Html`       | HTML reporter    | browser-renderable validation report |
|   [4]   | `Json`       | JSON reporter    | machine-readable JSON output         |
|   [5]   | `Bcf`        | BCF reporter     | BCF issue export                     |
|   [6]   | `Ods`        | ODS reporter     | spreadsheet output                   |
|   [7]   | `OdsSummary` | ODS reporter     | summary spreadsheet output           |
|   [8]   | `Txt`        | text reporter    | plain text output                    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IDS document I/O
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :------------------------------ |
|   [1]   | `open(filepath: str, validate: bool=False) -> Ids`                              | loader         | parse IDS XML file              |
|   [2]   | `from_string(xml: str, validate: bool=False) -> Ids`                            | loader         | parse IDS XML string            |
|   [3]   | `Ids(title, copyright, version, description, author, date, purpose, milestone)` | constructor    | new IDS document                |
|   [4]   | `Ids.to_xml() -> ...`                                                           | serializer     | serialize to XML bytes/string   |
|   [5]   | `Ids.to_string() -> str`                                                        | serializer     | serialize to XML string         |
|   [6]   | `Ids.parse(...)`                                                                | mutator        | parse and attach specifications |

[ENTRYPOINT_SCOPE]: validation
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                                      | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Ids.validate(ifc_file, should_filter_version=False, filepath=None) -> None`                   | validator      | validate all specs against IFC model |
|   [2]   | `Specification.validate(ifc_file, should_filter_version=False) -> None`                        | validator      | validate one specification           |
|   [3]   | `Specification(name, minOccurs, maxOccurs, ifcVersion, identifier, description, instructions)` | constructor    | new specification                    |
|   [4]   | `Specification.check_ifc_version(ifc_file) -> bool`                                            | version check  | filter by IFC schema version         |
|   [5]   | `Specification.get_usage() / set_usage(...)`                                                   | cardinality    | applicability/requirement clause     |
|   [6]   | `Specification.reset_status()`                                                                 | mutator        | clear prior validation results       |

[ENTRYPOINT_SCOPE]: facet construction
- rail: ids-validation

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `Entity(name='IFCWALL', predefinedType=None, instructions=None)`                    | constructor    | entity facet                 |
|   [2]   | `Attribute(name='Name', value=None, cardinality='required', instructions=None)`     | constructor    | attribute facet              |
|   [3]   | `Classification(value, system, uri, cardinality='required', instructions=None)`     | constructor    | classification facet         |
|   [4]   | `Property(propertySet, baseName, value, dataType, uri, cardinality, instructions)`  | constructor    | property facet               |
|   [5]   | `Material(value, uri, cardinality='required', instructions=None)`                   | constructor    | material facet               |
|   [6]   | `PartOf(name, predefinedType, relation, cardinality='required', instructions=None)` | constructor    | partof facet                 |
|   [7]   | `Restriction(options=None, base='string')`                                          | constructor    | value restriction            |
|   [8]   | `Facet.filter(ifc_file, elements) -> list`                                          | method         | filter IFC entities by facet |

[ENTRYPOINT_SCOPE]: reporter output
- rail: ids-validation output

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------- | :------------- | :---------------------------- |
|   [1]   | `Reporter(ids: Ids)`               | constructor    | bind reporter to IDS document |
|   [2]   | `Reporter.report() -> ...`         | method         | collect validation results    |
|   [3]   | `Reporter.write(filepath) -> None` | method         | write report to file          |
|   [4]   | `Html.to_file(filepath) -> None`   | method         | write HTML report file        |
|   [5]   | `Console.print() -> None`          | method         | print report to stdout        |

## [4]-[IMPLEMENTATION_LAW]

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
