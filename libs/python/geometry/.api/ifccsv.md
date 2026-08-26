# [PY_GEOMETRY_API_IFCCSV]

`ifccsv` owns bidirectional IFC-to-tabular exchange over the `ifcopenshell` model: `IfcCsv().export` writes a selector-scoped element set to CSV/ODS/XLSX or a Pandas `DataFrame`, and `IfcCsv().Import` re-applies an edited table's attribute and Pset cells back onto the model. Column values resolve through `ifcopenshell.util.selector.get_element_value` and write through `set_element_value`, so an attribute string is the same `Pset.Property`/`type.` selector-path grammar the `IfcSelector` gate admits.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tabular export and re-import

Export consumes a model, an `IfcSelector.filter`-scoped element iterable, an attribute list, an output path, and a `FILE_FORMAT`; import consumes a model and a table path, resolving the format from the extension.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------- | :-------------------------- | :---------------------------------------------- |
|  [01]   | `IfcCsv().export(model, elements, attributes)` | model, elements, attributes | resolve columns; write the selected set         |
|  [02]   | `IfcCsv().export_pd()`                         | none (drains export state)  | return the selected set as a Pandas `DataFrame` |
|  [03]   | `IfcCsv().Import(model, table)`                | model + table path          | re-apply an edited table's cells onto the model |

- `IfcCsv().export`: `include_global_id=True` prepends the `GlobalId` column that keys re-import; `format="pd"` returns the frame, every other format writes to `output` and returns `None`.

## [03]-[IMPLEMENTATION_LAW]

[TABULAR_EXCHANGE_TOPOLOGY]:
- export axis: `IfcCsv().export(model, elements, attributes, format=<FILE_FORMAT>, output=<path>)` resolves each attribute through `util.selector.get_element_value`, so the attribute vocabulary is the selector-path grammar (`Pset.Property`, `type.Attribute`). `export` expands no wildcard of its own: `get_wildcard_attributes` is a separate member reading the object's `ifc_file`, which `export` binds only on entry, so a caller wanting `Pset.*` binds the model and expands the roster ahead of the call. `export` also inserts the `include_global_id` key column into the caller's own `attributes` list, so each call hands it a fresh one. Its element set arrives `IfcSelector.filter`-scoped, so a malformed selector faults before `export` runs; `format="pd"` returns the `DataFrame`, `csv`/`ods`/`xlsx` write to `output`, and `format=None` runs the resolve half alone, leaving the grid and resolved headers on the object with no writer opened.
- import axis: `IfcCsv().Import(model, table)` dispatches on the table extension to `import_csv`/`import_xlsx`/`import_ods` and returns on an extension it does not know, so an unadmitted suffix reads as a clean zero-row run; each path converges on `process_row`, which keys the row on its `GlobalId` via `model.by_guid`, drops any column whose key lowercases to contain `count` or `material`, and writes every remaining cell through `util.selector.set_element_value` — `ifccsv`'s own write path. The transaction law fencing those writes is the one `ifc/authoring.md` legislates, so a re-import driven inside `begin_transaction`/`undo`/`end_transaction` unwinds whole rather than persisting half a table, and attribute and Pset edits round-trip onto the ownership-history transaction. `Import` returns `None` and prints a `by_guid` miss to stdout, publishing no count a caller can read back.
- lifecycle stacking: `ifc/costing#LIFECYCLE` owns integration: its `LifecyclePhase.EXPORT` arm threads a selector through the `IfcSelector.filter` gate, hands the filtered set, the expanded column vocabulary, and a `TableFormat` token to `IfcCsv().export` under `format=None`, so the grid lands on the object and the durable spreadsheet write belongs to `python:data/spatial`; its `LifecyclePhase.IMPORT` arm drives `IfcCsv().Import` inside that transaction fence and censuses the run through one `process_row` override. A new format is one `TableFormat` member with its `export_*` writer; a new column is one selector-path attribute string.
- evidence: both directions name their loss on the SAME closed law roster rather than counting it — a blind `count`/`material` column the write never reaches, a null substitution, an empty substitution, a `by_guid` miss the provider prints and publishes no count for, and a table row narrower than the resolved header roster the per-index write truncates. Each occurrence carries its subject, its column, and the substituted spelling; `FidelityLog` keeps the per-law census on the lifecycle result.
- boundary: `ifccsv` owns IFC-to-tabular export and table-to-IFC re-import over the `ifcopenshell` model; element selection stays the shared `IfcSelector` gate; attribute read stays `util.selector.get_element_value` and write stays `set_element_value`; the durable spreadsheet write defers to `python:data/spatial`, binding the writer call without holding a file handle across the boundary.

[CAPTURE_GAP]:
- members: the `IfcCsv.export`/`export_csv`/`export_ods`/`export_xlsx`/`export_pd`/`Import`/`import_csv`/`import_pd`/`process_row` surface, the `FILE_FORMAT` literal, and the `export`/`Import` default kwargs verify by source read against the single-module distribution — the `ifcopenshell` C extension does not build on darwin/python3.15, so `ifccsv.py` confirms by read, never runtime import
