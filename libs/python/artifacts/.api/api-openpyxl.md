# [PY_ARTIFACTS_API_OPENPYXL]

`openpyxl` supplies the Excel `.xlsx`/`.xlsm` workbook surface for the artifacts office rail: a `Workbook` root, a `load_workbook` reader, a `Worksheet` grid with cell/row access, and the styles/chart/formatting submodules that drive workbook read/write, streaming read/write modes, named styles, charts, and conditional formatting against the OOXML spreadsheet format. The package owner composes `Workbook`, `load_workbook`, and `Worksheet` into the office owner; it never re-implements OOXML parsing openpyxl already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openpyxl`
- package: `openpyxl`
- import: `openpyxl`
- owner: `artifacts`
- rail: office
- installed: `3.1.5` reflected via `python -c "import openpyxl"` on cp315
- entry points: none (library only)
- capability: `.xlsx`/`.xlsm` read/write, read-only and write-only streaming modes, cell formulas and values, named styles, number formats, conditional formatting, charts, images, pivot tables, data validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and sheet roots
- rail: office

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `Workbook` | workbook root | a workbook with sheets, named styles, and save |
| [2] | `worksheet.worksheet.Worksheet` | sheet grid | cell/row access, dimensions, freeze panes, formatting |
| [3] | `cell.cell.Cell` | cell | a single cell value/formula/style |
| [4] | `chartsheet.Chartsheet` | chart sheet | a full-sheet chart |
| [5] | `styles` | style module | `Font`/`Fill`/`Border`/`Alignment`/`NamedStyle` |
| [6] | `chart` | chart module | `BarChart`/`LineChart`/`ScatterChart`/`PieChart` and `Reference` |
| [7] | `formatting` | conditional module | conditional-formatting rules |
| [8] | `utils` | coordinate utils | column-letter/coordinate conversion |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open, build, and save
- rail: office

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `Workbook` | `Workbook(write_only=False, iso_dates=False)` | build a workbook (write-only streaming mode optional) |
| [2] | `load_workbook` | `load_workbook(filename, read_only=False, keep_vba=False, data_only=False, keep_links=True, rich_text=False) -> Workbook` | open an existing workbook |
| [3] | `Workbook.active` | `active -> Worksheet` | the active sheet |
| [4] | `Workbook.create_sheet` | `create_sheet(title=None, index=None) -> Worksheet` | add a sheet |
| [5] | `Workbook.copy_worksheet` | `copy_worksheet(from_worksheet) -> Worksheet` | duplicate a sheet |
| [6] | `Workbook.add_named_style` | `add_named_style(style: NamedStyle) -> None` | register a named style |
| [7] | `Workbook.save` | `save(filename) -> None` | serialize the workbook |

[ENTRYPOINT_SCOPE]: sheet cell, row, and feature access
- rail: office

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `Worksheet.cell` | `cell(row: int, column: int, value=None) -> Cell` | addressed cell access |
| [2] | `Worksheet.append` | `append(iterable) -> None` | append a row |
| [3] | `Worksheet.iter_rows` | `iter_rows(min_row=None, max_row=None, min_col=None, max_col=None, values_only=False) -> Iterator` | row iteration |
| [4] | `Worksheet.merge_cells` | `merge_cells(range_string=None, start_row=None, start_column=None, end_row=None, end_column=None) -> None` | merge a region |
| [5] | `Worksheet.add_chart` | `add_chart(chart, anchor=None) -> None` | embed a chart |
| [6] | `Worksheet.add_image` | `add_image(img, anchor=None) -> None` | embed an image |
| [7] | `Worksheet.freeze_panes` | `freeze_panes` | freeze-pane property |
| [8] | `Worksheet.conditional_formatting.add` | `add(range_string, cfRule) -> None` | add a conditional rule |

## [4]-[IMPLEMENTATION_LAW]

[OFFICE_XLSX]:
- import: `import openpyxl` at boundary scope only; module-level import is banned by the manifest import policy.
- workbook axis: one `Workbook` owns sheets and save; `write_only=True` (write streaming) and `load_workbook(read_only=True)` (read streaming) are mode rows for large workbooks, never parallel workbook types.
- cell axis: `Worksheet.cell`/`append`/`iter_rows` are the grid access rows; `values_only` selects value vs cell-object iteration — a call row, never separate functions.
- styling axis: the `styles` module (`Font`/`Fill`/`NamedStyle`) and conditional-formatting rules attach to cells/sheets; styles are reusable named rows, never per-cell duplication.
- chart axis: the `chart` module's chart types plus `Reference` and `add_chart` are the embed surface; the chart kind is a row.
- evidence: each workbook op captures sheet count, populated cell count, style count, chart count, and output byte length as an office receipt.
- boundary: openpyxl owns `.xlsx`; Word routes to `python-docx`, PowerPoint to `python-pptx`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `openpyxl`
- Owns: `.xlsx`/`.xlsm` read/write, streaming modes, formulas/values, named styles, conditional formatting, charts, images, data validation
- Accept: spreadsheet authoring and ingestion feeding the office and export-bundle owners
- Reject: wrapper-renames of `cell`/`save`; a per-cell style rebuild where named styles exist; a CSV fallback where rich `.xlsx` is admitted; identity minting the runtime owns
