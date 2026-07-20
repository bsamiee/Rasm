# [PY_GEOMETRY_API_IFCCSV]

`ifccsv` supplies the IFC-to-tabular round-trip surface for the geometry ifc-lifecycle rail: one `IfcCsv` object exports a selected element set to CSV/ODS/XLSX or a Pandas `DataFrame` (`export`), and re-imports an edited table back onto the model (`Import`), writing attribute and Pset edits through `ifcopenshell.util.selector.set_element_value` — the same selector grammar `IfcSelector` gates. It rides the `ifcopenshell` worker lane (single-module pure-Python distribution over the `ifcopenshell` core, same `0.8.5` ecosystem band as `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff`), so the lifecycle owner composes `IfcCsv().export`/`IfcCsv().Import` directly rather than hand-rolling a `csv.writer` fold over `util.element.get_psets`. Column values resolve through `ifcopenshell.util.selector.get_element_value`, so an attribute string is the same `Pset.Property`/`type.` selector-path vocabulary the `IfcSelector` filter admits — never a per-class getter family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifccsv`
- package: `ifccsv`
- import: `import ifccsv`
- owner: `geometry`
- rail: ifc-lifecycle / tabular-exchange
- installed: `0.8.5`
- license: LGPL-3.0-or-later (the IfcOpenShell-ecosystem license)
- entry points: none (single-module library; the `python -m ifccsv` argparse block is the CLI, not a `console_scripts` row)
- capability: bidirectional IFC-to-spreadsheet exchange — `IfcCsv().export(model, elements, attributes, format, output)` writes a selected element set to CSV/ODS/XLSX or returns a Pandas `DataFrame`, and `IfcCsv().Import(model, table)` re-applies an edited table's attribute and Pset cells onto the model through `ifcopenshell.util.selector.set_element_value`; ODS support needs `odfpy`, XLSX and Pandas support need `openpyxl`/`pandas`, all imported under `try` guards inside the module

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exchange object (`ifccsv.IfcCsv`)
- rail: tabular-exchange
- `ifccsv.FILE_FORMAT` literal is the closed output-format vocabulary: `csv`, `ods`, `xlsx`, `pd`.
- One `IfcCsv()` instance is stateful — `export` populates `self.headers`/`self.results`/`self.dataframe`, and the `export_*` writers drain that state; a re-export reuses one object rather than a fresh construction.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]     | [CAPABILITY]                                                                               |
| :-----: | :------------------- | :----------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `IfcCsv`             | exchange object    | stateful export/import object holding `headers`, `results`, `dataframe`                    |
|  [02]   | `IfcCsv.export`      | export kernel      | resolve `attributes` per element and dispatch on `format` to the matching writer           |
|  [03]   | `IfcCsv.export_csv`  | CSV writer         | `(output, delimiter=None) -> None` writes the drained result grid to a CSV path            |
|  [04]   | `IfcCsv.export_ods`  | ODS writer         | `(output, should_preserve_existing=False) -> None` writes an ODS workbook via `odfpy`      |
|  [05]   | `IfcCsv.export_xlsx` | XLSX writer        | `(output, should_preserve_existing=False) -> None` writes an XLSX workbook via `openpyxl`  |
|  [06]   | `IfcCsv.export_pd`   | DataFrame writer   | `() -> pd.DataFrame` materializes the result grid as a Pandas frame                        |
|  [07]   | `IfcCsv.Import`      | import dispatcher  | `(model, table, attributes=None, …) -> None` dispatches on the table's file extension      |
|  [08]   | `IfcCsv.import_csv`  | CSV importer       | read a CSV table and apply each row onto the model via `process_row`                       |
|  [09]   | `IfcCsv.import_pd`   | DataFrame importer | apply an in-memory `DataFrame`'s rows onto the model (the `import_xlsx`/`import_ods` core) |
|  [10]   | `IfcCsv.FILE_FORMAT` | format literal     | the closed output/extension literal (`csv`/`ods`/`xlsx`/`pd`)                              |

Result-shaping helpers `group_results`/`summarise_results`/`sort_results`/`format_results`/`get_wildcard_attributes` run inside `export` from its `groups`/`summaries`/`sort`/`formatting` spec dicts — they are the post-selection aggregation surface, not standalone entrypoints. `import_xlsx`/`import_ods` decode the workbook to a `DataFrame` and delegate to `import_pd`; `process_row` is the per-row `set_element_value` writer both import paths converge on, keyed on the row's `GlobalId` first column via `model.by_guid`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tabular export and re-import
- rail: tabular-exchange

Export consumes a model, an already-filtered element iterable, an attribute list, an output path, and a `FILE_FORMAT`; import consumes a model and a table path, resolving the format from the extension. That element set is the `IfcSelector.filter` product, never a raw `util.selector.filter_elements` call inside this owner.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------- | :-------------------------- | :---------------------------------------------- |
|  [01]   | `IfcCsv().export(model, elements, attributes)` | model, elements, attributes | resolve columns; write the selected set         |
|  [02]   | `IfcCsv().export_pd()`                         | none (drains export state)  | return the selected set as a Pandas `DataFrame` |
|  [03]   | `IfcCsv().Import(model, table)`                | model + table path          | re-apply an edited table's cells onto the model |

`export` signature: `(ifc_file, elements, attributes, headers=None, output=None, format=None, should_preserve_existing=False, include_global_id=True, delimiter=",", null="-", empty="", bool_true="YES", bool_false="NO", concat=", ", sort=None, groups=None, summaries=None, formatting=None)`. `include_global_id=True` prepends the `GlobalId` column that keys re-import; `format="pd"` returns the frame, every other format writes to `output` and returns `None`. `Import` signature: `(ifc_file, table, attributes=None, delimiter=",", null="-", empty="", bool_true="YES", bool_false="NO", concat=", ")`.

## [04]-[IMPLEMENTATION_LAW]

[TABULAR_EXCHANGE_TOPOLOGY]:
- import: `import ifccsv` at boundary scope only; module-level import is banned by the manifest import policy, and the ecosystem sibling imports function-local under `# noqa: PLC0415`.
- export axis: `IfcCsv().export(model, elements, attributes, format=<FILE_FORMAT>, output=<path>)` resolves each `attribute` against each element through `ifcopenshell.util.selector.get_element_value`, so the attribute vocabulary is the selector-path grammar (`Pset.Property`, `type.Attribute`, wildcard `Pset.*` expanded by `get_wildcard_attributes`) — never a hand-rolled `get_psets` fold. Its element set arrives already scoped by the `IfcSelector.filter` validated gate, so a malformed selector is a `BoundaryFault` at admission before `export` runs. `format="pd"` returns the `DataFrame` for in-process handoff; `csv`/`ods`/`xlsx` write to `output`.
- import axis: `IfcCsv().Import(model, table)` dispatches on the table extension to `import_csv`/`import_xlsx`/`import_ods`, each converging on `process_row`, which keys the row on its `GlobalId` first column via `model.by_guid` and writes every other cell back through `ifcopenshell.util.selector.set_element_value` — the same authoring rail `ifc/authoring.md` owns, so attribute and Pset edits round-trip onto the ownership-history transaction, never a bespoke property-set mutation.
- lifecycle stacking: `ifc/costing.md#LIFECYCLE` is the integration owner — a `LifecyclePhase.EXCHANGE` arm threads its element selector through the shared `IfcSelector.filter` gate (the lark grammar), hands the filtered set, the attribute list, and a closed export-format token to `IfcCsv().export`, and binds the durable spreadsheet write onto the receipt subject for the `python:data/spatial` boundary to drive — never a throwaway `tempfile` write the run discards. Its re-import arm consumes an edited table token and drives `IfcCsv().Import`, folding the changed-cell count into typed lifecycle rows and graduating the mutated model's SPF bytes home like the `PATCH` arm. A new export format is one `FILE_FORMAT` literal member with its `export_*` writer; a new column is one selector-path attribute string — zero new surface.
- evidence: each export captures the element count, the attribute columns resolved, and the null/empty substitution counts; each re-import captures the rows applied, the rows skipped (`by_guid` miss), and the cells written; the lifecycle receipt keys the empty-row fraction (a non-empty selected set producing no table rows is a degenerate run) as the exchange residual the graduation leg folds against the caller ceiling.
- boundary: `ifccsv` owns IFC-to-tabular export and table-to-IFC re-import over the `ifcopenshell` model; element selection stays the shared `IfcSelector` gate (never a direct `util.selector.filter_elements` call here); attribute read stays `util.selector.get_element_value` and attribute write stays `util.selector.set_element_value`; the durable spreadsheet write defers to `python:data/spatial` (this owner binds the writer call, never holds a file handle across the seam); quantity take-off stays `ifc5d`; cost rollup stays `ifcopenshell.api.cost`; clash stays `ifcclash`; IDS stays `ifctester`; schedule stays `ifc4d`; recipe patch stays `ifcpatch`; revision diff stays `ifcdiff`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifccsv`
- Owns: bidirectional IFC-to-tabular exchange — `IfcCsv().export` (CSV/ODS/XLSX/Pandas) and `IfcCsv().Import` (re-apply an edited table onto the model)
- Accept: an `ifcopenshell.file` plus an `IfcSelector.filter`-scoped element set and a selector-path attribute list for export, or a model plus an edited table path for re-import, feeding the ifc-lifecycle owner
- Reject: a hand-rolled `csv.writer` fold over `util.element.get_psets` where `IfcCsv().export` owns the column resolution; a bespoke `by_guid`-keyed property-set mutation loop where `IfcCsv().Import` routes cells through `util.selector.set_element_value`; a raw `util.selector.filter_elements` call inside this owner where `IfcSelector` is the validated gate

[CAPTURE_GAP]:
- members: the `IfcCsv.export`/`export_csv`/`export_ods`/`export_xlsx`/`export_pd` export surface and the `IfcCsv.Import`/`import_csv`/`import_pd`/`process_row` import surface confirm by source introspection against the installed single-module distribution before any fence transcribes them (the `ifcopenshell` C extension does not build on darwin/python3.15, so the pure-Python `ifccsv.py` module verifies by read, never runtime import); the `FILE_FORMAT` literal set, the `export`/`Import` default kwargs, and the `util.selector.get_element_value`/`set_element_value` column codec are the source-confirmed members
