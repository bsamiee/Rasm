# [PY_GEOMETRY_API_IFCTESTER]

`ifctester` owns buildingSMART IDS authoring, parsing, and validation: it serializes IDS XML into `Ids`/`Specification` models, validates each specification against an `ifcopenshell.file` to a tri-state per-spec `status` and per-spec entity sets, and reports the outcome as a `Results` graph or a rendered artifact. `Ids.validate` mutates each `Specification` in place, so the consumer reads the `applicable`/`passed`/`failed` entity sets for per-element evidence, never the boolean verdict alone; IFC parse stays `ifcopenshell`, feeding the geometry ifc-analysis rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifctester`
- package: `ifctester` (LGPL-3.0-or-later, IfcOpenShell ecosystem)
- module: `import ifctester` re-exports `open`; the document/facet/reporter families import from `ifctester.ids`, `ifctester.facet`, `ifctester.reporter`
- owner: `geometry`
- rail: ifc-companion / ids-validation
- depends: `ifcopenshell`, `xmlschema`
- capability: parse/author/serialize IDS XML, validate every `Specification` against an `ifcopenshell.file` to a tri-state per-spec `status` and per-spec entity sets, and report the result as a `Results` graph, rendered HTML, ODS/summary spreadsheets, coloured console/text, or a BCF issue archive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document family (`ifctester.ids`)

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [CAPABILITY]                                                            |
| :-----: | :------------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `Ids`                      | document root    | container for `specifications`; carries the `info` metadata dict        |
|  [02]   | `Specification`            | requirement unit | applicability + requirement facets; mutated in place by `validate`      |
|  [03]   | `Restriction`              | value constraint | `(options=None, base="string")` pattern/enumeration/bounds value filter |
|  [04]   | `Cardinality`              | usage literal    | `Literal["required","optional","prohibited"]` facet/usage clause        |
|  [05]   | `IdsXmlValidationError`    | parse failure    | raised by `open`/`from_string` on XSD failure, wrapping `xmlschema`     |
|  [06]   | `XMLSchemaValidationError` | parse failure    | the external `xmlschema` error `IdsXmlValidationError` wraps            |

[PUBLIC_TYPE_SCOPE]: facet family (`ifctester.facet`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [CAPABILITY]                                                    |
| :-----: | :--------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `Facet`          | base facet      | shared `filter`/`to_string`/`to_ids_value`/`get_usage`/`asdict` |
|  [02]   | `Entity`         | entity facet    | IFC entity name + predefined type filter                        |
|  [03]   | `Attribute`      | attribute facet | entity attribute name + value filter                            |
|  [04]   | `Classification` | class facet     | classification system + reference filter                        |
|  [05]   | `Property`       | property facet  | pset name + property name + value + dataType filter             |
|  [06]   | `Material`       | material facet  | material name + URI filter                                      |
|  [07]   | `PartOf`         | partof facet    | spatial containment + relation filter                           |

[PUBLIC_TYPE_SCOPE]: per-facet result family (`ifctester.facet`)

`Result(is_pass, reason=None)` is the base carrier — `is_pass: bool`, `reason: dict | None`, `__bool__` returns `is_pass`; each facet subclass overrides `to_string()` to render its `reason` dict. `FacetFailure` is the `TypedDict` failed-entity+reason carrier the `Bcf` reporter folds.
- [RESULT_SUBCLASS]: `EntityResult` `AttributeResult` `PropertyResult` `ClassificationResult` `MaterialResult` `PartOfResult`

[PUBLIC_TYPE_SCOPE]: structured report graph (`ifctester.reporter`)

`Json`-family `report()` returns a `Results` `TypedDict` read field-by-field, not a stringified blob. Every level carries a `total_<scope>`/`_pass`/`_fail` count triple, a `percent_<scope>_pass` (`int | "N/A"`), and a `status`; the fields below are the distinctive graph edges.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                               |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Results`              | report root     | `status: bool`, the three count triples, `specifications: list[ResultsSpecification]`      |
|  [02]   | `ResultsSpecification` | per-spec result | `name`/`description`/`is_skipped`/`is_ifc_version`, `cardinality`, applicable/requirements |
|  [03]   | `ResultsRequirement`   | per-req result  | `facet_type`/`metadata`/`label`/`value`, `passed`/`failed_entities: list[ResultsEntity]`   |
|  [04]   | `ResultsEntity`        | per-entity row  | `reason`, `element`, `class`, `predefined_type`, `name`, `global_id`, `id`, `tag`          |

[PUBLIC_TYPE_SCOPE]: reporter family (`ifctester.reporter`)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [CAPABILITY]                                                                                          |
| :-----: | :----------- | :--------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Reporter`   | abstract base    | `__init__(ids)`; base `report(self, ids)`/`to_string()`/`write()` are NO-OP stubs subclasses override |
|  [02]   | `Console`    | console reporter | `(ids, use_colour=True)`; `report() -> None` then `to_string()`/`to_file(path)`                       |
|  [03]   | `Txt`        | text reporter    | `Console` subclass with colour disabled                                                               |
|  [04]   | `Json`       | JSON reporter    | `(ids, hide_skipped=False)`; `report() -> Results` then `to_string() -> str`/`to_file(path)`          |
|  [05]   | `Html`       | HTML reporter    | `Json` subclass; `to_file(path)` renders a browser report                                             |
|  [06]   | `Ods`        | ODS reporter     | `Json` subclass; `to_file(path)` writes a spreadsheet                                                 |
|  [07]   | `OdsSummary` | ODS reporter     | `Json` subclass; `to_file(path)` writes a summary spreadsheet                                         |
|  [08]   | `Bcf`        | BCF reporter     | `Json` subclass; `to_file(path)` writes a BCF issue archive of failures                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IDS document I/O (`ifctester.ids`)

`Ids(title="Untitled", copyright=None, version=None, description=None, author=None, date=None, purpose=None, milestone=None)` folds metadata into `Ids.info`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `ids.open(filepath: str, validate: bool=False) -> Ids`   | static   | parse IDS XML file; `validate=True` raises on XSD fail |
|  [02]   | `ids.from_string(xml: str, validate: bool=False) -> Ids` | static   | parse IDS XML string                                   |
|  [03]   | `Ids(title="Untitled", ...)`                             | ctor     | new IDS document (full ctor in lead)                   |
|  [04]   | `Ids.to_string() -> str`                                 | instance | serialize to IDS XML string                            |
|  [05]   | `Ids.to_xml(filepath="output.xml") -> bool`              | instance | write IDS XML to file, return XSD-valid flag           |
|  [06]   | `Ids.parse(data: dict) -> Ids`                           | instance | populate `info`/`specifications` from `xmlschema` dict |
|  [07]   | `Ids.asdict() -> dict`                                   | instance | the IDS dict the XSD encoder consumes                  |

[ENTRYPOINT_SCOPE]: validation (`ifctester.ids`)

`Specification(name="Unnamed", minOccurs=0, maxOccurs="unbounded", ifcVersion=["IFC2X3","IFC4","IFC4X3_ADD2"], identifier=None, description=None, instructions=None)` holds the applicability/requirement facet lists.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Ids.validate(ifc_file, should_filter_version=False, filepath=None) -> None` | instance | reset, version-check, validate specs   |
|  [02]   | `Specification.validate(ifc_file, should_filter_version=False) -> None`      | instance | validate one spec (no `filepath`)      |
|  [03]   | `Specification(name="Unnamed", ...)`                                         | ctor     | new specification (full ctor in lead)  |
|  [04]   | `Specification.check_ifc_version(ifc_file) -> bool`                          | instance | set `is_ifc_version` from schema match |
|  [05]   | `Specification.get_usage() -> Cardinality` / `set_usage(usage)`              | instance | derive/set `minOccurs`/`maxOccurs`     |
|  [06]   | `Specification.reset_status() -> None`                                       | instance | clear `status` and the entity sets     |

[VALIDATION_RESULT_STATE]: after `Ids.validate` each `Specification` carries `status: bool | None` (`True` pass / `False` fail / `None` skipped — tri-state, not a plain bool), `is_ifc_version: bool | None`, `applicable_entities: list` (matched applicability), and `passed_entities`/`failed_entities: set`.

[ENTRYPOINT_SCOPE]: facet construction (`ifctester.facet`)

Facet constructors carry a `cardinality="required", instructions=None` tail — except `Entity`, which ends at `instructions=None`; `Entity`/`PartOf` also carry `predefinedType`. `Restriction` and the `Facet` methods stand apart.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Entity(name="IFCWALL", predefinedType=None, instructions=None)`            | ctor     | entity facet                         |
|  [02]   | `Attribute(name="Name", value=None, ...)`                                   | ctor     | attribute facet                      |
|  [03]   | `Classification(value=None, system=None, uri=None, ...)`                    | ctor     | classification facet                 |
|  [04]   | `Property(propertySet, baseName, value, dataType, uri, ...)`                | ctor     | property facet                       |
|  [05]   | `Material(value=None, uri=None, ...)`                                       | ctor     | material facet                       |
|  [06]   | `PartOf(name, predefinedType, relation, ...)`                               | ctor     | partof facet                         |
|  [07]   | `Restriction(options=None, base="string")`                                  | ctor     | value restriction                    |
|  [08]   | `Facet.filter(ifc_file, elements) -> list[entity_instance]`                 | instance | filter elements by facet predicate   |
|  [09]   | `Facet.to_string(clause_type, specification=None, requirement=None) -> str` | instance | render the facet clause as IDS prose |

[ENTRYPOINT_SCOPE]: reporter output (`ifctester.reporter`)

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `Reporter(ids: Ids)`                   | ctor     | bind reporter to the validated IDS document                     |
|  [02]   | `Json(ids).report() -> Results`        | instance | collect the structured result graph in memory                   |
|  [03]   | `Console(ids).report() -> None`        | instance | render the coloured console buffer (then `to_string`/`to_file`) |
|  [04]   | `<reporter>.to_string() -> str`        | instance | the in-memory string product (Json/Console/Html)                |
|  [05]   | `<reporter>.to_file(filepath) -> None` | instance | write the report artifact (Json/Html/Ods/OdsSummary/Bcf/Txt)    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- document axis: `Ids` holds `specifications: list[Specification]`; each `Specification` holds `applicability`/`requirements: list[Facet]`; `ids.open`/`from_string` parse via `xmlschema` against the bundled `ids.xsd`, raising `IdsXmlValidationError` (wrapping `XMLSchemaValidationError`) under `validate=True`.
- validation axis: `Ids.validate` clears the `get_pset`/`get_psets` caches, then per spec runs `reset_status()` -> `check_ifc_version()` -> `validate()`, mutating the tri-state `status` and the entity sets in place; `Facet.filter` is the single match engine every facet shares.
- usage axis: `Cardinality` maps to `minOccurs`/`maxOccurs` through `get_usage`/`set_usage`; `Restriction.base` (`"string"`/`"decimal"`/`"integer"`/`"boolean"`) drives value coercion.
- reporter axis: each reporter binds the validated `Ids`, runs `report()` (the `Json` family returning the `Results` graph, `Console`/`Txt` a buffer), then `to_string()`/`to_file(path)`; `Json` subclasses override `to_file` for their artifact.
- evidence: each validation captures the per-spec tri-state `status`, the passing/failing entity-set sizes, and the `Results` totals as an IDS receipt.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `Ids.validate`/`Specification.validate` take an `ifcopenshell.file` target, and `Facet.filter(ifc_file, elements)` runs the match over its `entity_instance` elements — the single engine, never a hand-rolled entity filter.
- `geometry:ifc/analysis.md#ANALYSIS` IDS arm: `ids.open(spec)` -> `Ids.validate(model)` -> fold each `Specification.status` into a `compliance` `AnalysisRow`; `status is None` (skipped) collapses to skip, distinct from `False`, and `passed_entities`/`failed_entities` set sizes with `Json(ids).report()` `Results.percent_checks_pass` and `ResultsSpecification.total_applicable_fail` sharpen the per-check failing fraction the boolean verdict drops, keying `AnalysisResult.evidence`.
- `bcf-client`(`.api/bcf-client.md`) / `ifcclash`(`.api/ifcclash.md`): IDS-side BCF export is `Bcf(ids).to_file(path)` over validation failures, disjoint from clash-issue authoring (`ifcclash` + `bcf-client`, `analysis.md` `BCF` arm); the IDS arm authors no clash topic.

[LOCAL_ADMISSION]:
- `ifctester` owns IDS parse/author/validate; IFC parse stays `ifcopenshell`, clash stays `ifcclash`, BCF clash-issue authoring stays `bcf-client`. A custom IDS XML parser or a manual entity filter duplicating `Facet.filter` is the deleted form.

[RAIL_LAW]:
- Package: `ifctester`
- Owns: IDS document authoring/parsing/serialization, facet-based IFC compliance validation to a tri-state per-spec `status` and per-spec entity sets, and structured report export (the `Results` graph with HTML/ODS/BCF/console artifacts)
- Accept: an `ifcopenshell.file` target for `Ids.validate`/`Specification.validate`; an IDS XML path/string for the loaders
- Reject: a hand-rolled IDS parser or entity filter duplicating `Facet.filter`; a `Specification.validate(..., filepath=)` call (only `Ids.validate` carries `filepath`); a `Reporter.write`/`report(ids)` output path (subclasses use `report()` then `to_string`/`to_file`); a plain-bool `status` read conflating `None` (skipped) with `False`
