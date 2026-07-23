# [PY_GEOMETRY_API_IFCCSV]

`ifccsv` owns bidirectional IFC-to-tabular exchange over the `ifcopenshell` model: `IfcCsv().export` writes a selector-scoped element set to CSV/ODS/XLSX or a Pandas `DataFrame`, and `IfcCsv().Import` re-applies an edited table's attribute and Pset cells back onto the model. Column values resolve through `ifcopenshell.util.selector.get_element_value` and write through `set_element_value`, so an attribute string is the same `Pset.Property`/`type.` selector-path grammar the `IfcSelector` gate admits.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifccsv`
- package: `ifccsv`
- import: `import ifccsv`
- owner: `geometry`
- rail: ifc-lifecycle / tabular-exchange
- license: LGPL-3.0-or-later (IfcOpenShell-ecosystem)
- entry points: none (single-module library; `python -m ifccsv` is an argparse CLI, not a `console_scripts` row)
- capability: `export` writes the selected set to CSV/ODS/XLSX or returns a Pandas `DataFrame`; `Import` re-applies an edited table's cells through `util.selector.set_element_value`; ODS needs `odfpy`, XLSX and Pandas need `openpyxl`/`pandas`, imported under `try` guards

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exchange object (`ifccsv.IfcCsv`)
- `ifccsv.FILE_FORMAT` is the closed output-format literal: `csv`, `ods`, `xlsx`, `pd`.
- One `IfcCsv()` instance is stateful: `export` populates `self.headers`/`self.results`/`self.dataframe` and the `export_*` writers drain that state, so a re-export reuses one object.

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

Result-shaping helpers `group_results`/`summarise_results`/`sort_results`/`format_results`/`get_wildcard_attributes` run inside `export` from its `groups`/`summaries`/`sort`/`formatting` spec dicts, not as standalone entrypoints. `import_xlsx`/`import_ods` decode the workbook to a `DataFrame` and delegate to `import_pd`; `process_row` is the per-row `set_element_value` writer both import paths converge on, keyed on the row's `GlobalId` first column via `model.by_guid`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tabular export and re-import

Export consumes a model, an `IfcSelector.filter`-scoped element iterable, an attribute list, an output path, and a `FILE_FORMAT`; import consumes a model and a table path, resolving the format from the extension.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------- | :-------------------------- | :---------------------------------------------- |
|  [01]   | `IfcCsv().export(model, elements, attributes)` | model, elements, attributes | resolve columns; write the selected set         |
|  [02]   | `IfcCsv().export_pd()`                         | none (drains export state)  | return the selected set as a Pandas `DataFrame` |
|  [03]   | `IfcCsv().Import(model, table)`                | model + table path          | re-apply an edited table's cells onto the model |

- `IfcCsv().export`: `include_global_id=True` prepends the `GlobalId` column that keys re-import; `format="pd"` returns the frame, every other format writes to `output` and returns `None`.

## [04]-[IMPLEMENTATION_LAW]

[TABULAR_EXCHANGE_TOPOLOGY]:
- export axis: `IfcCsv().export(model, elements, attributes, format=<FILE_FORMAT>, output=<path>)` resolves each attribute through `util.selector.get_element_value`, so the attribute vocabulary is the selector-path grammar (`Pset.Property`, `type.Attribute`, wildcard `Pset.*` via `get_wildcard_attributes`). Its element set arrives `IfcSelector.filter`-scoped, so a malformed selector faults before `export` runs; `format="pd"` returns the `DataFrame`, `csv`/`ods`/`xlsx` write to `output`.
- import axis: `IfcCsv().Import(model, table)` dispatches on the table extension to `import_csv`/`import_xlsx`/`import_ods`, each converging on `process_row`, which keys the row on its `GlobalId` via `model.by_guid` and writes every other cell through `util.selector.set_element_value` — the authoring rail `ifc/authoring.md` owns, so attribute and Pset edits round-trip onto the ownership-history transaction.
- lifecycle stacking: `ifc/costing.md#LIFECYCLE` owns integration: its `LifecyclePhase.EXCHANGE` arm threads a selector through the `IfcSelector.filter` gate, hands the filtered set, attribute list, and a `FILE_FORMAT` token to `IfcCsv().export`, and binds the durable spreadsheet write onto the receipt for `python:data/spatial`; its re-import arm drives `IfcCsv().Import`. A new format is one `FILE_FORMAT` member with its `export_*` writer; a new column is one selector-path attribute string.
- evidence: each export captures the element count, the attribute columns resolved, and the null/empty substitution counts; each re-import captures the rows applied, the rows skipped (`by_guid` miss), and the cells written; the receipt keys the empty-row fraction as the exchange residual the graduation leg folds against the caller ceiling.
- boundary: `ifccsv` owns IFC-to-tabular export and table-to-IFC re-import over the `ifcopenshell` model; element selection stays the shared `IfcSelector` gate; attribute read stays `util.selector.get_element_value` and write stays `set_element_value`; the durable spreadsheet write defers to `python:data/spatial`, binding the writer call without holding a file handle across the seam.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifccsv`
- Owns: bidirectional IFC-to-tabular exchange — `IfcCsv().export` (CSV/ODS/XLSX/Pandas) and `IfcCsv().Import` (re-apply an edited table onto the model)
- Accept: an `ifcopenshell.file` plus an `IfcSelector.filter`-scoped element set and a selector-path attribute list for export, or a model plus an edited table path for re-import
- Reject: a hand-rolled `csv.writer` fold over `util.element.get_psets` where `export` owns column resolution; a bespoke `by_guid`-keyed property-set mutation where `Import` routes cells through `util.selector.set_element_value`; a raw `util.selector.filter_elements` call where `IfcSelector` is the validated gate

[CAPTURE_GAP]:
- members: the `IfcCsv.export`/`export_csv`/`export_ods`/`export_xlsx`/`export_pd`/`Import`/`import_csv`/`import_pd`/`process_row` surface, the `FILE_FORMAT` literal, and the `export`/`Import` default kwargs verify by source read against the single-module distribution — the `ifcopenshell` C extension does not build on darwin/python3.15, so `ifccsv.py` confirms by read, never runtime import
