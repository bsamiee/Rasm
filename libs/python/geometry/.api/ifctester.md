# [PY_GEOMETRY_API_IFCTESTER]

`ifctester` supplies IDS (Information Delivery Specification) authoring, parsing, and validation against IFC models: it exposes `Ids`/`Specification` document models, a facet family (`Entity`, `Attribute`, `Classification`, `Property`, `Material`, `PartOf`) with `Restriction` value constraints, a per-facet `Result` family, and a reporter family (`Console`, `Txt`, `Json`, `Html`, `Ods`, `OdsSummary`, `Bcf`) emitting both a structured `Results` TypedDict graph and file artifacts. `Ids.validate` mutates each `Specification` in place to a tri-state `status` plus `applicable_entities`/`passed_entities`/`failed_entities` sets; the consumer reads those richer sets, not just the boolean verdict. It rides the `ifcopenshell` worker lane (`0.8.5`; depends `ifcopenshell`, `xmlschema`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifctester`
- package: `ifctester`
- import: `import ifctester` (re-exports `open`); the document/facet/reporter families import from `ifctester.ids`, `ifctester.facet`, `ifctester.reporter`
- owner: `geometry`
- rail: ifc-companion / ids-validation
- installed: `0.8.5`
- entry points: none (library only; ships a `__main__` CLI and a `webapp` subpackage not consumed here)
- capability: parse/author/serialize buildingSMART IDS XML, validate every `Specification` against an `ifcopenshell.file` to a tri-state per-spec `status` plus per-spec entity sets, and report the result as a structured `Results` graph (JSON), rendered HTML, ODS/ODS-summary spreadsheets, coloured console/text, or a BCF issue archive

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document family
- rail: ids-validation

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [RAIL]                                                                                 |
| :-----: | :------------------------- | :--------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `Ids`                      | document root    | container for `specifications`; carries the `info` metadata dict                       |
|  [02]   | `Specification`            | requirement unit | applicability + requirement facets; mutated in place by `validate`                     |
|  [03]   | `Restriction`              | value constraint | `(options=None, base="string")` pattern/enumeration/bounds value filter                |
|  [04]   | `Cardinality`              | usage literal    | `Literal["required","optional","prohibited"]` facet/usage clause                       |
|  [05]   | `IdsXmlValidationError`    | parse failure    | raised by `open`/`from_string` when XSD validation fails (wraps `xmlschema`)           |
|  [06]   | `XMLSchemaValidationError` | parse failure    | underlying `xmlschema` error `IdsXmlValidationError` wraps (external, not `ifctester`) |

[PUBLIC_TYPE_SCOPE]: facet family (`ifctester.facet`)
- rail: ids-validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                                                          |
| :-----: | :--------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `Facet`          | base facet      | shared `filter`/`to_string`/`to_ids_value`/`get_usage`/`asdict` |
|  [02]   | `Entity`         | entity facet    | IFC entity name + predefined type filter                        |
|  [03]   | `Attribute`      | attribute facet | entity attribute name + value filter                            |
|  [04]   | `Classification` | class facet     | classification system + reference filter                        |
|  [05]   | `Property`       | property facet  | pset name + property name + value + dataType filter             |
|  [06]   | `Material`       | material facet  | material name + URI filter                                      |
|  [07]   | `PartOf`         | partof facet    | spatial containment + relation filter                           |

[PUBLIC_TYPE_SCOPE]: per-facet result family (`ifctester.facet`)
- rail: ids-validation

`Result(is_pass, reason=None)` is the base carrier — `is_pass: bool`, `reason: dict | None`, `__bool__` returns `is_pass`. Each facet subclass overrides `to_string()` to render its `reason` dict.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                                      |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `Result`               | match result     | `is_pass`/`reason` per-facet carrier                        |
|  [02]   | `EntityResult`         | entity result    | entity facet match result                                   |
|  [03]   | `AttributeResult`      | attribute result | attribute facet match result                                |
|  [04]   | `PropertyResult`       | property result  | property facet match result                                 |
|  [05]   | `ClassificationResult` | class result     | classification facet match result                           |
|  [06]   | `MaterialResult`       | material result  | material facet match result                                 |
|  [07]   | `PartOfResult`         | partof result    | partof facet match result                                   |
|  [08]   | `FacetFailure`         | failure carrier  | `TypedDict` failed-entity + reason the `Bcf` reporter folds |

[PUBLIC_TYPE_SCOPE]: structured report graph (`ifctester.reporter`)
- rail: ids-validation output

`Json`-family `report()` returns a `Results` `TypedDict` read field-by-field, not a stringified blob. Every level carries a `total_<scope>`/`_pass`/`_fail` count triple and a `percent_<scope>_pass` (`int | "N/A"`); every level also carries a `status`; the fields below are the distinctive graph edges.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [FIELDS]                                                                                   |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Results`              | report root     | `status: bool`, the three count triples, `specifications: list[ResultsSpecification]`      |
|  [02]   | `ResultsSpecification` | per-spec result | `name`/`description`/`is_skipped`/`is_ifc_version`, `cardinality`, applicable/requirements |
|  [03]   | `ResultsRequirement`   | per-req result  | `facet_type`/`metadata`/`label`/`value`, `passed`/`failed_entities: list[ResultsEntity]`   |
|  [04]   | `ResultsEntity`        | per-entity row  | `reason`, `element`, `class`, `predefined_type`, `name`, `global_id`, `id`, `tag`          |

[PUBLIC_TYPE_SCOPE]: reporter family (`ifctester.reporter`)
- rail: ids-validation output

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [OUTPUT]                                                                                              |
| :-----: | :----------- | :--------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Reporter`   | abstract base    | `__init__(ids)`; base `report(self, ids)`/`to_string()`/`write()` are NO-OP stubs subclasses override |
|  [02]   | `Console`    | console reporter | `report() -> None` then `to_string()`/`to_file(path)`; `(ids, use_colour=True)`                       |
|  [03]   | `Txt`        | text reporter    | `Console` subclass with colour disabled                                                               |
|  [04]   | `Json`       | JSON reporter    | `report() -> Results` then `to_string() -> str`/`to_file(path)`; `(ids, hide_skipped=False)`          |
|  [05]   | `Html`       | HTML reporter    | `Json` subclass; `to_file(path)` renders a browser report                                             |
|  [06]   | `Ods`        | ODS reporter     | `Json` subclass; `to_file(path)` writes a spreadsheet                                                 |
|  [07]   | `OdsSummary` | ODS reporter     | `Json` subclass; `to_file(path)` writes a summary spreadsheet                                         |
|  [08]   | `Bcf`        | BCF reporter     | `Json` subclass; `to_file(path)` writes a BCF issue archive of failures                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IDS document I/O (`ifctester.ids`)
- rail: ids-validation

`Ids(title="Untitled", copyright=None, version=None, description=None, author=None, date=None, purpose=None, milestone=None)` ctor folds metadata into `Ids.info`.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `ids.open(filepath: str, validate: bool=False) -> Ids`   | loader         | parse IDS XML file; `validate=True` raises on XSD fail |
|  [02]   | `ids.from_string(xml: str, validate: bool=False) -> Ids` | loader         | parse IDS XML string                                   |
|  [03]   | `Ids(title="Untitled", ...)`                             | constructor    | new IDS document (full ctor in lead)                   |
|  [04]   | `Ids.to_string() -> str`                                 | serializer     | serialize to IDS XML string                            |
|  [05]   | `Ids.to_xml(filepath="output.xml") -> bool`              | serializer     | write IDS XML to file, return XSD-valid flag           |
|  [06]   | `Ids.parse(data: dict) -> Ids`                           | mutator        | populate `info`/`specifications` from `xmlschema` dict |
|  [07]   | `Ids.asdict() -> dict`                                   | projector      | the IDS dict the XSD encoder consumes                  |

[ENTRYPOINT_SCOPE]: validation (`ifctester.ids`)
- rail: ids-validation

`Specification(name="Unnamed", minOccurs=0, maxOccurs="unbounded", ifcVersion=["IFC2X3","IFC4","IFC4X3_ADD2"], identifier=None, description=None, instructions=None)` ctor holds the applicability/requirement facet lists.

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `Ids.validate(ifc_file, should_filter_version=False, filepath=None) -> None` | validator      | reset, version-check, validate specs   |
|  [02]   | `Specification.validate(ifc_file, should_filter_version=False) -> None`      | validator      | validate one spec (no `filepath`)      |
|  [03]   | `Specification(name="Unnamed", ...)`                                         | constructor    | new specification (full ctor in lead)  |
|  [04]   | `Specification.check_ifc_version(ifc_file) -> bool`                          | version check  | set `is_ifc_version` from schema match |
|  [05]   | `Specification.get_usage() -> Cardinality` / `set_usage(usage)`              | cardinality    | derive/set `minOccurs`/`maxOccurs`     |
|  [06]   | `Specification.reset_status() -> None`                                       | mutator        | clear `status` and the entity sets     |

[VALIDATION_RESULT_STATE]: after `Ids.validate`, each `Specification` carries `status: bool | None` (tri-state: `True` pass, `False` fail, `None` skipped/not-applicable — NOT a plain bool), `is_ifc_version: bool | None`, `applicable_entities: list[entity_instance]` (the matched applicability set), `passed_entities: set[entity_instance]`, and `failed_entities: set[entity_instance]`. Consumer reads the entity sets for per-element evidence, not only the spec-level `status`.

[ENTRYPOINT_SCOPE]: facet construction (`ifctester.facet`)
- rail: ids-validation

Every facet constructor ends `..., cardinality="required", instructions=None` (`Entity`/`PartOf` also take `predefinedType`); `Restriction(options=None, base="string")` and the `Facet` methods stand apart.

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `Entity(name="IFCWALL", predefinedType=None, ...)`                          | constructor    | entity facet                         |
|  [02]   | `Attribute(name="Name", value=None, ...)`                                   | constructor    | attribute facet                      |
|  [03]   | `Classification(value=None, system=None, uri=None, ...)`                    | constructor    | classification facet                 |
|  [04]   | `Property(propertySet, baseName, value, dataType, uri, ...)`                | constructor    | property facet                       |
|  [05]   | `Material(value=None, uri=None, ...)`                                       | constructor    | material facet                       |
|  [06]   | `PartOf(name, predefinedType, relation, ...)`                               | constructor    | partof facet                         |
|  [07]   | `Restriction(options=None, base="string")`                                  | constructor    | value restriction                    |
|  [08]   | `Facet.filter(ifc_file, elements) -> list[entity_instance]`                 | method         | filter elements by facet predicate   |
|  [09]   | `Facet.to_string(clause_type, specification=None, requirement=None) -> str` | method         | render the facet clause as IDS prose |

[ENTRYPOINT_SCOPE]: reporter output (`ifctester.reporter`)
- rail: ids-validation output

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `Reporter(ids: Ids)`                   | constructor    | bind reporter to the validated IDS document                     |
|  [02]   | `Json(ids).report() -> Results`        | collector      | collect the structured result graph in memory                   |
|  [03]   | `Console(ids).report() -> None`        | collector      | render the coloured console buffer (then `to_string`/`to_file`) |
|  [04]   | `<reporter>.to_string() -> str`        | serializer     | the in-memory string product (Json/Console/Html)                |
|  [05]   | `<reporter>.to_file(filepath) -> None` | sink           | write the report artifact (Json/Html/Ods/OdsSummary/Bcf/Txt)    |

Subclass output is `report()` (no `ids` arg) then `to_string()`/`to_file(filepath)`; the base `Reporter.write(filepath)` and `report(self, ids)` are no-op stubs and are NOT the output path.

## [04]-[IMPLEMENTATION_LAW]

[IDS_TOPOLOGY]:
- import: `import ifctester.ids` / `import ifctester.reporter` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: `Ids` contains a `specifications: list[Specification]`; each `Specification` holds `applicability: list[Facet]` and `requirements: list[Facet]`; `ids.open`/`from_string` parse via `xmlschema` against the bundled `ids.xsd`, raising `IdsXmlValidationError` (wrapping `XMLSchemaValidationError`) under `validate=True`.
- validation axis: `Ids.validate` clears the `get_pset`/`get_psets` caches, then for each spec runs `reset_status()` -> `check_ifc_version()` -> `validate()`, mutating the tri-state `status` and the `applicable_entities`/`passed_entities`/`failed_entities` sets in place. `Facet.filter` predicate is the single matching engine each facet shares; a hand-rolled entity filter is the deleted form.
- usage axis: `Cardinality` (`"required"`/`"optional"`/`"prohibited"`) maps to `minOccurs`/`maxOccurs` through `get_usage`/`set_usage`; `Restriction.base` (`"string"`/`"decimal"`/`"integer"`/`"boolean"`) controls value coercion.
- reporter axis: each reporter binds the validated `Ids`, runs `report()` (the `Json` family returning the `Results` graph, `Console`/`Txt` rendering a buffer), then `to_string()`/`to_file(path)`; `Json` subclasses (`Html`/`Ods`/`OdsSummary`/`Bcf`) override `to_file` for their artifact, never the base `write` stub.
- evidence: each validation captures the per-spec tri-state `status`, the passing/failing entity-set sizes, and the `Results` totals as an IDS receipt.
- boundary: `ifctester` owns IDS parse/author/validate; a custom IDS XML parser or a manual entity filter duplicating `Facet.filter` is the deleted form; IFC parse stays `ifcopenshell`, clash stays `ifcclash`, BCF clash-issue authoring stays `bcf-client`.

[INTEGRATION_STACK]:
- `geometry:ifc/analysis.md#ANALYSIS` `IDS` arm composes `ifctester.ids.open(spec_path)` -> `document.validate(model)` -> folds each `Specification.status` into a `compliance` `AnalysisRow` (`1.0`/`0.0` verdict, `0`/`1` failure count). Tri-state `status is None` (skipped) is distinct from `False` (failed) — the consumer collapses `None` to skip rather than fail; the per-requirement passing fraction sharpens through `failed_entities`/`passed_entities` set sizes, the richer evidence the boolean verdict alone drops.
- `Json(ids).report() -> Results` graph STACKS with the graduation evidence ledger: the `Results.percent_checks_pass` and `ResultsSpecification.total_applicable_fail` feed a per-check failing fraction the `AnalysisResult.evidence` fold keys, a sharper residual than the binary spec verdict.
- `Bcf` reporter is NOT the clash-issue path: IDS-side BCF export is `Bcf(ids).to_file(path)` of validation failures, while clash-issue authoring composes `ifcclash` + `bcf-client` (`geometry:ifc/analysis.md` `BCF` arm) — the two BCF producers are disjoint and the IDS arm does not author a clash topic.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifctester`
- Owns: IDS document authoring/parsing/serialization, facet-based IFC compliance validation to a tri-state per-spec `status` plus per-spec entity sets, and structured report export (the `Results` graph plus HTML/ODS/BCF/console artifacts)
- Accept: an `ifcopenshell.file` as the target model for `Ids.validate`/`Specification.validate`; an IDS XML path/string for the loaders
- Reject: a custom IDS XML parser or manual entity filtering that duplicates `Facet.filter`; a `Specification.validate(..., filepath=)` call (only `Ids.validate` carries `filepath`); a `Reporter.write`/`report(ids)` output path (subclasses use `report()` then `to_string`/`to_file`); a plain-bool `status` read that conflates the `None` skipped verdict with `False`

[CAPTURE_GAP]:
- members: all member signatures, the tri-state `status`/entity-set attributes, the `Results`/`ResultsSpecification`/`ResultsRequirement`/`ResultsEntity` TypedDict graph, the reporter `report()`/`to_string()`/`to_file()` output contract, and the `Restriction.base`/`Cardinality` literals are confirmed against the `0.8.5` source; only the per-`Property` requirement passing-fraction shape (read off `failed_entities`/`passed_entities` set sizes) is a live-run sharpen the analysis owner unlocks
