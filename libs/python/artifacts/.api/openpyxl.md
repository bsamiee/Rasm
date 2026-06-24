# [PY_ARTIFACTS_API_OPENPYXL]

`openpyxl` supplies the read+write `.xlsx`/`.xlsm` workbook surface for the artifacts office rail: a `Workbook` root with `load_workbook` read policy and `write_only`/`read_only` streaming mirrors, a `Worksheet` grid with addressed/iter/insert/move access, the `styles` named-style family, the `chart`/`formatting.rule` authoring families, worksheet `Table`/`DataValidation`/pivot features, the `formula.Translator`/`Tokenizer` formula engine, `CellRichText` inline runs, and `utils` coordinate/dataframe converters. The package owner composes `Workbook`, `load_workbook`, `Worksheet`, and the feature families into the office owner; it never re-implements the OOXML parse, the SpreadsheetML packaging, or the formula reference-translation openpyxl already owns, and routes write-only bulk authoring to `xlsxwriter` only when constant-memory streaming beats openpyxl's own `write_only` mode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openpyxl`
- package: `openpyxl`
- import: `openpyxl`
- owner: `artifacts`
- rail: office
- installed: `3.1.5` reflected via reflection on cp315 (Python 3.15)
- license: MIT; pure Python (`openpyxl-3.1.5-py2.py3-none-any.whl`), single hard dependency `et-xmlfile` (MIT, lazy-XML write); optional acceleration flags `openpyxl.LXML` (lxml-backed serialization), `openpyxl.NUMPY` (numpy date/dataframe support), `openpyxl.DEFUSEDXML` (hardened parse) are runtime-resolved booleans, not install gates; no compiled extension, installs clean on cp315
- entry points: none (library only)
- capability: `.xlsx`/`.xlsm` read+write, `read_only`/`write_only` streaming modes for large workbooks, formulas/values with `data_only` cached-value read and `formula.Translator` reference shifting, named + inline (`CellRichText`) styles, number formats, conditional formatting (cell-is/color-scale/data-bar/icon-set/formula rules), charts (bar/line/scatter/pie/area/radar/bubble/surface/stock + 3D), images, worksheet `Table` objects with `TableStyleInfo`, data validation, pivot tables, defined names/named ranges, freeze panes, merge regions, auto-filter, and dataframe interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and sheet roots
- rail: office

`Workbook` is the single root; its mode is a constructor/loader flag, never a parallel type — `write_only=True` yields `WriteOnlyWorksheet` (append-only streaming), `load_workbook(read_only=True)` yields `ReadOnlyWorksheet` (lazy cell parse). `Cell` carries value/formula/style; `WriteOnlyCell` is the styled streaming counterpart.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `Workbook`                              | workbook root     | sheets, named styles, defined names, save; `write_only`/`iso_dates` ctor flags |
|  [02]   | `worksheet.worksheet.Worksheet`         | sheet grid        | cell/row/col access, dimensions, freeze panes, merge, tables, validation, charts |
|  [03]   | `worksheet._read_only.ReadOnlyWorksheet`| read-stream grid  | lazy cell iteration under `load_workbook(read_only=True)`          |
|  [04]   | `worksheet._write_only.WriteOnlyWorksheet` | write-stream grid | append-only row streaming under `Workbook(write_only=True)`     |
|  [05]   | `cell.cell.Cell`                        | cell              | value/formula/data-type/style/comment/hyperlink/number-format      |
|  [06]   | `cell.rich_text.CellRichText`           | rich-text cell    | list of `TextBlock(InlineFont, str)` inline runs in one cell       |
|  [07]   | `worksheet.table.Table`                 | worksheet table   | structured `ref` table with `TableStyleInfo` and column metadata   |
|  [08]   | `worksheet.datavalidation.DataValidation` | validation rule | list/whole/decimal/date/custom validation over a `sqref` region    |
|  [09]   | `chartsheet.Chartsheet`                 | chart sheet       | full-sheet chart                                                   |
|  [10]   | `workbook.defined_name.DefinedName`     | named range/formula | workbook- or sheet-scoped name resolving to a range or formula   |

[PUBLIC_TYPE_SCOPE]: style, chart, formatting, and formula families
- rail: office

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :-------------------------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `styles`                          | style module       | `Font`/`PatternFill`/`GradientFill`/`Border`/`Side`/`Alignment`/`Protection`/`Color`/`NamedStyle` |
|  [02]   | `chart`                           | chart module       | `BarChart`/`LineChart`/`ScatterChart`/`PieChart`/`AreaChart`/`RadarChart`/`BubbleChart`/`SurfaceChart`/`StockChart` (+ `3D` variants), `Reference`, `Series` |
|  [03]   | `formatting.rule`                 | conditional module | `CellIsRule`/`ColorScaleRule`/`DataBarRule`/`IconSetRule`/`FormulaRule`/`Rule` + `DifferentialStyle` |
|  [04]   | `formula.translate.Translator`    | formula engine     | shift cell references when copying a formula across a range      |
|  [05]   | `formula.tokenizer.Tokenizer`     | formula lexer      | tokenize a formula string into operands/operators/functions      |
|  [06]   | `worksheet.formula.ArrayFormula` / `DataTableFormula`  | array/table formula      | `ArrayFormula(ref)` dynamic/spilled array; `DataTableFormula` one/two-variable what-if data table bound to a `ref` |
|  [07]   | `worksheet.table.TableStyleInfo`  | table style        | banded-row/column table styling applied to a `Table`             |
|  [08]   | `utils`                           | coordinate utils   | `get_column_letter`/`column_index_from_string`/`range_boundaries`/`coordinate_to_tuple`/`absolute_coordinate`/`quote_sheetname`/`rows_from_range`/`cols_from_range` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open, build, and save
- rail: office

`load_workbook` carries the read policy (streaming, cached-value, macro-preserve, link, rich-text); `Workbook` carries the build policy (streaming, ISO-date). Named styles and defined names register on the workbook once and bind by reference.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                          |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Workbook`                 | `Workbook(write_only=False, iso_dates=False)`                                                             | build a workbook (write-only streaming mode optional) |
|  [02]   | `load_workbook`            | `load_workbook(filename, read_only=False, keep_vba=False, data_only=False, keep_links=True, rich_text=False)` | open an existing workbook with read policy         |
|  [03]   | `Workbook.active`          | property -> `Worksheet`                                                                                    | the active sheet                                      |
|  [04]   | `Workbook.create_sheet`    | `create_sheet(title=None, index=None)` -> `Worksheet`                                                     | add a sheet at an optional index                      |
|  [05]   | `Workbook.copy_worksheet`  | `copy_worksheet(from_worksheet)` -> `Worksheet`                                                            | duplicate a sheet (in-file only)                      |
|  [06]   | `Workbook.create_chartsheet` | `create_chartsheet(title=None, index=None)` -> `Chartsheet`                                              | add a full-sheet chart sheet                           |
|  [07]   | `Workbook.remove`          | `remove(worksheet)`                                                                                        | delete a sheet (the inverse of `create_sheet`)        |
|  [08]   | `Workbook.move_sheet`      | `move_sheet(sheet, offset=0)`                                                                              | reorder a sheet by signed offset                      |
|  [09]   | `Workbook.add_named_style` | `add_named_style(style)`                                                                                   | register a reusable `NamedStyle`                      |
|  [10]   | `Workbook.create_named_range` | `create_named_range(name, worksheet=None, value=None, scope=None)`                                     | register a `DefinedName` range                        |
|  [11]   | `Workbook.defined_names`   | mapping property                                                                                          | workbook-scoped name -> `DefinedName` resolution      |
|  [12]   | `Workbook.save`            | `save(filename)`                                                                                          | serialize the workbook to a path or stream            |
|  [13]   | `Workbook.close`           | `close()`                                                                                                 | release `read_only`/`write_only` file handles         |

[ENTRYPOINT_SCOPE]: sheet cell, row, structure, and feature access
- rail: office

One `Worksheet` owns addressed access (`cell`), bulk iteration (`iter_rows`/`iter_cols` with `values_only`), append-streaming (`append`), structural edits (`insert`/`delete`/`move_range`), and feature attach (`add_chart`/`add_image`/`add_table`/`add_data_validation`/`add_pivot`). `values_only` is a call flag, never a separate function.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                                                          | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `Worksheet.cell`                       | `cell(row, column, value=None)` -> `Cell`                                             | addressed cell read/write                 |
|  [02]   | `Worksheet.append`                     | `append(iterable)`                                                                    | append a row (the write-only streaming op) |
|  [03]   | `Worksheet.iter_rows`                  | `iter_rows(min_row=None, max_row=None, min_col=None, max_col=None, values_only=False)`| row iteration (cells or raw values)       |
|  [04]   | `Worksheet.iter_cols`                  | `iter_cols(min_col=None, max_col=None, min_row=None, max_row=None, values_only=False)`| column iteration (cells or raw values)    |
|  [05]   | `Worksheet.merge_cells`                | `merge_cells(range_string=None, start_row=None, start_column=None, end_row=None, end_column=None)` | merge a region            |
|  [06]   | `Worksheet.move_range`                 | `move_range(cell_range, rows=0, cols=0, translate=False)`                             | shift a range, optionally re-translating formulas |
|  [07]   | `Worksheet.insert_rows`                | `insert_rows(idx, amount=1)` / `insert_cols` / `delete_rows` / `delete_cols`          | structural row/column edits               |
|  [08]   | `Worksheet.add_chart`                  | `add_chart(chart, anchor=None)`                                                       | embed a chart at an anchor                |
|  [09]   | `Worksheet.add_image`                  | `add_image(img, anchor=None)`                                                         | embed an image                            |
|  [10]   | `Worksheet.add_table`                  | `add_table(table)`                                                                    | register a structured `Table` object      |
|  [11]   | `Worksheet.add_data_validation`        | `add_data_validation(data_validation)`                                                | attach a `DataValidation` rule            |
|  [12]   | `Worksheet.add_pivot`                  | `add_pivot(pivot)`                                                                    | attach a pivot table definition           |
|  [13]   | `Worksheet.freeze_panes`               | property (coordinate or cell)                                                         | freeze rows/columns above-left of a cell  |
|  [14]   | `Worksheet.conditional_formatting.add` | `add(range_string, cfRule)` (the `conditional_formatting` instance is a `ConditionalFormattingList`) | attach a conditional-format rule          |

[ENTRYPOINT_SCOPE]: style, chart, formatting, formula, and coordinate authoring
- rail: office

Styles mint once and share by reference; conditional-format and chart objects are kind rows; `formula.Translator` shifts references when a formula is copied; `utils` converts between A1 coordinates and zero/one-based tuples for boundary mapping.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                            | [CAPABILITY]                                       |
| :-----: | :------------------------------ | :-------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `styles.NamedStyle`             | `NamedStyle(name='Normal', font=None, fill=None, border=None, alignment=None, number_format=None, protection=None, builtinId=None)` | a reusable named style registered on the workbook |
|  [02]   | `styles.Font` / `PatternFill` / `Border` / `Side` / `Alignment` / `Protection` | constructor each                                            | the style-component value objects assigned to `Cell.font`/`.fill`/... |
|  [03]   | `chart.Reference`               | `Reference(worksheet=None, min_col=None, min_row=None, max_col=None, max_row=None, range_string=None)` | a data range for a chart series             |
|  [04]   | `chart.<Kind>Chart`             | `BarChart()` / `LineChart()` / `ScatterChart()` / ... with `.add_data(ref)`/`.set_categories(ref)` | a chart of one kind (the kind is a row)     |
|  [05]   | `formatting.rule.ColorScaleRule`| `ColorScaleRule(start_type, start_value, start_color, end_type, end_value, end_color)`  | a 2/3-color-scale conditional rule                 |
|  [06]   | `formatting.rule.CellIsRule` / `DataBarRule` / `IconSetRule` / `FormulaRule` | constructor each                                           | the conditional-rule kinds (each a row)            |
|  [07]   | `formula.translate.Translator`  | `Translator(formula, origin)` -> `.translate_formula(dest)`; `Translator.translate_range(range_str, rdelta, cdelta)` | shift a formula's references to `dest`, or a whole range string by a row/col delta |
|  [08]   | `worksheet.formula.ArrayFormula` / `DataTableFormula` | `ArrayFormula(ref, text=None)` / `DataTableFormula(ref, ca, dt2D, dtr, r1, r2, del1, del2)` | a spilled/dynamic array formula, or a one/two-variable what-if data table, bound to `ref` |
|  [09]   | `utils.get_column_letter`       | `get_column_letter(idx)` / `column_index_from_string(letter)` / `range_boundaries(range_string)` / `coordinate_to_tuple(coord)` / `absolute_coordinate(coord)` | A1 <-> tuple coordinate conversion at the boundary |
|  [10]   | `utils.dataframe.dataframe_to_rows` | `dataframe_to_rows(df, index=True, header=True)`                                    | stream a pandas/polars-compatible frame into `append` rows |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_XLSX]:
- import: `import openpyxl` (or targeted `from openpyxl import Workbook, load_workbook`, `from openpyxl.styles import NamedStyle, Font`, `from openpyxl.chart import BarChart, Reference`, `from openpyxl.formatting.rule import ColorScaleRule`) at boundary scope only; module-level import is banned by the manifest import policy.
- workbook axis: one `Workbook` owns the sheet lifecycle (`create_sheet`/`copy_worksheet`/`create_chartsheet`/`remove`/`move_sheet`), named styles, defined names, and save; `Workbook(write_only=True)` (append-only write streaming yielding `WriteOnlyWorksheet`) and `load_workbook(read_only=True)` (lazy read streaming yielding `ReadOnlyWorksheet`) are mode rows for large workbooks, never parallel workbook types; `data_only=True` reads the last cached values instead of formula text, `keep_vba=True` round-trips `.xlsm` macros, `rich_text=True` reads inline `CellRichText`.
- cell axis: `Worksheet.cell`/`append`/`iter_rows`/`iter_cols` are the grid access rows; `values_only` selects raw-value vs `Cell`-object iteration — a call flag, never separate functions; structural `insert_rows`/`delete_cols`/`move_range` shift the grid in place, with `translate=True` re-anchoring moved formulas.
- styling axis: the `styles` module (`Font`/`PatternFill`/`GradientFill`/`Border`/`Side`/`Alignment`/`Protection`/`NamedStyle`) mints reusable value objects assigned by reference to `Cell.font`/`.fill`/...; `NamedStyle` registers once via `add_named_style` and is referenced by name, never per-cell duplication; `CellRichText`/`TextBlock`/`InlineFont` carry mixed-style runs inside one cell.
- formula axis: formula strings assign to `Cell.value`; `formula.Translator(formula, origin).translate_formula(dest)` (or `.translate_range(range_str, rdelta, cdelta)` for a whole range) shifts relative references when a formula is copied, never a manual offset rewrite; `ArrayFormula(ref, text)` carries spilled/dynamic arrays and `DataTableFormula(ref, ...)` carries one/two-variable what-if data tables; `formula.Tokenizer` lexes a formula for inspection.
- feature axis: `chart.<Kind>Chart` + `Reference`/`Series` + `add_chart`, `worksheet.table.Table` + `TableStyleInfo` + `add_table`, `DataValidation` + `add_data_validation`, conditional `formatting.rule.*Rule` + `conditional_formatting.add`, and `add_pivot` are the feature-attach rows; the chart/rule/validation kind is a row, never a per-feature sheet type.
- interchange axis: `utils.dataframe.dataframe_to_rows` streams a frame into `append`; `utils.get_column_letter`/`column_index_from_string`/`range_boundaries`/`coordinate_to_tuple`/`quote_sheetname` are the A1<->tuple boundary converters — internal code uses these, never an open-coded base-26 column math.
- evidence: each workbook op captures sheet count, populated cell count, named-style count, defined-name count, chart/table/validation/pivot counts, streaming mode, and output byte length as an office receipt.
- boundary: openpyxl owns `.xlsx`/`.xlsm` read+write including `read_only`/`write_only` streaming; constant-memory bulk authoring at near-O(1) row memory routes to `xlsxwriter` (write-only, no read) when the dataset exceeds openpyxl's `write_only` budget; ODF spreadsheets route to `odfpy`; Word to `python-docx`, PowerPoint to `python-pptx`; encrypted ECMA-376 workbooks decrypt through `msoffcrypto-tool` before `load_workbook`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `openpyxl`
- Owns: `.xlsx`/`.xlsm` read+write, `read_only`/`write_only` streaming modes, formulas/values with cached-value read and reference-translating formula engine, named + inline-rich styles, conditional formatting, the full chart family, images, worksheet tables, data validation, pivot tables, defined names, and dataframe interchange
- Accept: spreadsheet authoring and ingestion feeding the office and export-bundle owners, including decrypted workbooks from `msoffcrypto-tool`
- Reject: wrapper-renames of `cell`/`save`/`load_workbook`; a per-cell style rebuild where `NamedStyle` exists; an open-coded column-letter or formula-offset math where `utils`/`formula.Translator` own it; a CSV fallback where rich `.xlsx` is admitted; a parallel workbook type per streaming mode; reaching for `xlsxwriter` for anything but read-free constant-memory bulk streaming; identity minting the runtime owns
