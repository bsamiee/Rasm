# [PY_ARTIFACTS_API_OPENPYXL]

`openpyxl` supplies the Excel `.xlsx`/`.xlsm` workbook surface for the artifacts office rail: a `Workbook` root, a `load_workbook` reader, a `Worksheet` grid with cell/row access, and the styles/chart/formatting submodules that drive workbook read/write, streaming read/write modes, named styles, charts, and conditional formatting against the OOXML spreadsheet format. The package owner composes `Workbook`, `load_workbook`, and `Worksheet` into the office owner; it never re-implements OOXML parsing openpyxl already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openpyxl`
- package: `openpyxl`
- import: `openpyxl`
- owner: `artifacts`
- rail: office
- installed: `3.1.5` reflected via `python -c "import openpyxl"` on cp315
- entry points: none (library only)
- capability: `.xlsx`/`.xlsm` read/write, read-only and write-only streaming modes, cell formulas and values, named styles, number formats, conditional formatting, charts, images, pivot tables, data validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and sheet roots
- rail: office

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Workbook`                      | workbook root      | a workbook with sheets, named styles, and save                   |
|  [02]   | `worksheet.worksheet.Worksheet` | sheet grid         | cell/row access, dimensions, freeze panes, formatting            |
|  [03]   | `cell.cell.Cell`                | cell               | a single cell value/formula/style                                |
|  [04]   | `chartsheet.Chartsheet`         | chart sheet        | a full-sheet chart                                               |
|  [05]   | `styles`                        | style module       | `Font`/`Fill`/`Border`/`Alignment`/`NamedStyle`                  |
|  [06]   | `chart`                         | chart module       | `BarChart`/`LineChart`/`ScatterChart`/`PieChart` and `Reference` |
|  [07]   | `formatting`                    | conditional module | conditional-formatting rules                                     |
|  [08]   | `utils`                         | coordinate utils   | column-letter/coordinate conversion                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open, build, and save
- rail: office

Workbook rows carry streaming, ISO-date, macro, formula-data, link, rich-text, sheet-order, style, and save-target policy.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                 | [CAPABILITY]                                          |
| :-----: | :------------------------- | :--------------------------- | :---------------------------------------------------- |
|  [01]   | `Workbook`                 | workbook construction policy | build a workbook (write-only streaming mode optional) |
|  [02]   | `load_workbook`            | filename plus load policy    | open an existing workbook                             |
|  [03]   | `Workbook.active`          | active-sheet property        | the active sheet                                      |
|  [04]   | `Workbook.create_sheet`    | title plus insertion index   | add a sheet                                           |
|  [05]   | `Workbook.copy_worksheet`  | source worksheet             | duplicate a sheet                                     |
|  [06]   | `Workbook.add_named_style` | named style value            | register a named style                                |
|  [07]   | `Workbook.save`            | filename target              | serialize the workbook                                |

[ENTRYPOINT_SCOPE]: sheet cell, row, and feature access
- rail: office

Worksheet rows carry address, range, values-only, merge, anchor, pane, and conditional-format policy.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]            | [CAPABILITY]           |
| :-----: | :------------------------------------- | :---------------------- | :--------------------- |
|  [01]   | `Worksheet.cell`                       | row/column plus value   | addressed cell access  |
|  [02]   | `Worksheet.append`                     | iterable row            | append a row           |
|  [03]   | `Worksheet.iter_rows`                  | range plus value policy | row iteration          |
|  [04]   | `Worksheet.merge_cells`                | range or coordinate box | merge a region         |
|  [05]   | `Worksheet.add_chart`                  | chart plus anchor       | embed a chart          |
|  [06]   | `Worksheet.add_image`                  | image plus anchor       | embed an image         |
|  [07]   | `Worksheet.freeze_panes`               | freeze-pane property    | freeze-pane property   |
|  [08]   | `Worksheet.conditional_formatting.add` | range plus rule         | add a conditional rule |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_XLSX]:
- import: `import openpyxl` at boundary scope only; module-level import is banned by the manifest import policy.
- workbook axis: one `Workbook` owns sheets and save; `write_only=True` (write streaming) and `load_workbook(read_only=True)` (read streaming) are mode rows for large workbooks, never parallel workbook types.
- cell axis: `Worksheet.cell`/`append`/`iter_rows` are the grid access rows; `values_only` selects value vs cell-object iteration — a call row, never separate functions.
- styling axis: the `styles` module (`Font`/`Fill`/`NamedStyle`) and conditional-formatting rules attach to cells/sheets; styles are reusable named rows, never per-cell duplication.
- chart axis: the `chart` module's chart types plus `Reference` and `add_chart` are the embed surface; the chart kind is a row.
- evidence: each workbook op captures sheet count, populated cell count, style count, chart count, and output byte length as an office receipt.
- boundary: openpyxl owns `.xlsx`; Word routes to `python-docx`, PowerPoint to `python-pptx`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `openpyxl`
- Owns: `.xlsx`/`.xlsm` read/write, streaming modes, formulas/values, named styles, conditional formatting, charts, images, data validation
- Accept: spreadsheet authoring and ingestion feeding the office and export-bundle owners
- Reject: wrapper-renames of `cell`/`save`; a per-cell style rebuild where named styles exist; a CSV fallback where rich `.xlsx` is admitted; identity minting the runtime owns
