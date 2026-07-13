# [PY_ARTIFACTS_API_OPENPYXL]

`openpyxl` supplies the read+write `.xlsx`/`.xlsm` workbook surface for the artifacts office rail: a `Workbook` root with `load_workbook` read policy and `write_only`/`read_only` streaming mirrors, a `Worksheet` grid with addressed/iter/insert/move access, the `styles` named-style family, the `chart`/`formatting.rule` authoring families, worksheet `Table`/`DataValidation`/pivot features, the print-production page-setup family (`page_setup`/`page_margins`/`print_area`/`print_title_rows`), the `properties`/`security` descriptive-metadata and protection axes, the `formula.Translator`/`Tokenizer` formula engine, `CellRichText` inline runs, and `utils` coordinate/dataframe converters. The package owner composes `Workbook`, `load_workbook`, `Worksheet`, and the feature families into `folder:document/emit#DOCUMENT` (the `XLSX` `_xlsx_in_memory` arm lowering the one `document/model#NODE` `TableNode` grid through `CellValue.write_openpyxl`) and `folder:exchange/detect#DETECT` routes the `MediaClass.SPREADSHEET` ingest gate here; it never re-implements the OOXML parse, the SpreadsheetML packaging, or the formula reference-translation openpyxl already owns, routes write-only bulk authoring to `folder:.api/xlsxwriter#SPREADSHEET_STREAMING_ROW` only when constant-memory streaming beats openpyxl's own `write_only` mode, and routes bleeding-fast cell-grid READ of OOXML/binary Excel to `folder:.api/python-calamine#OFFICE_INGEST_XLSX` (the `document/lens#LENS` `XLSX_READ` arm) where openpyxl's lazy parse loses to the Rust reader.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openpyxl`
- package: `openpyxl`
- import: `openpyxl`
- owner: `artifacts`
- rail: office
- installed: `3.1.5`
- license: `MIT` (LICENCE.rst — MIT/Expat); pure-Python, no native build, cp315-resident and ungated in the manifest
- entry points: none (library only; import-only via `from openpyxl import Workbook, load_workbook`)
- capability: `.xlsx`/`.xlsm`/`.xltx`/`.xltm` read+write, `read_only`/`write_only` streaming modes for large workbooks, formulas/values with `data_only` cached-value read and `formula.Translator` reference shifting, named + inline (`CellRichText`) styles, number formats, conditional formatting (cell-is/color-scale/data-bar/icon-set/formula rules over `DifferentialStyle`), the full chart family (bar/line/scatter/pie/area/radar/bubble/surface/stock + 3D variants), images, worksheet `Table` objects with `TableStyleInfo`, data validation, pivot-table definitions, defined names/named ranges, array + data-table formulas, freeze panes, merge/unmerge regions, auto-filter with sort, column/row dimensions, print-production page setup (orientation/paper/fit-to-pages/margins/print-area/repeat-title-rows), workbook + sheet protection, document core properties, and pandas/polars dataframe interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and sheet roots
- rail: office

`Workbook` is the single root; its mode is a constructor/loader flag, never a parallel type — `write_only=True` yields `WriteOnlyWorksheet` (append-only streaming), `load_workbook(read_only=True)` yields `ReadOnlyWorksheet` (lazy cell parse). `Cell` carries value/formula/style; `WriteOnlyCell` is the styled streaming counterpart.

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]      | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------- | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `Workbook`                                 | workbook root       | sheets, named styles, defined names, save; `write_only` ctor flag   |
|  [02]   | `worksheet.worksheet.Worksheet`            | sheet grid          | cell/row/col, dims, freeze, merge, tables, validation, charts       |
|  [03]   | `worksheet._read_only.ReadOnlyWorksheet`   | read-stream grid    | lazy cell iteration under `load_workbook(read_only=True)`           |
|  [04]   | `worksheet._write_only.WriteOnlyWorksheet` | write-stream grid   | append-only row streaming under `Workbook(write_only=True)`         |
|  [05]   | `cell.cell.Cell`                           | cell                | value/formula/data-type/style/comment/hyperlink/number-format       |
|  [06]   | `cell.rich_text.CellRichText`              | rich-text cell      | list of `TextBlock(InlineFont, str)` inline runs in one cell        |
|  [07]   | `worksheet.table.Table`                    | worksheet table     | structured `ref` table with `TableStyleInfo` and column metadata    |
|  [08]   | `worksheet.datavalidation.DataValidation`  | validation rule     | list/whole/decimal/date/custom validation over a `sqref` region     |
|  [09]   | `chartsheet.Chartsheet`                    | chart sheet         | full-sheet chart                                                    |
|  [10]   | `workbook.defined_name.DefinedName`        | named range/formula | workbook- or sheet-scoped name resolving to a range or formula      |
|  [11]   | `cell.read_only.ReadOnlyCell`              | read-stream cell    | value-only lazy cell under `read_only=True` (no style write)        |
|  [12]   | `cell.cell.WriteOnlyCell`                  | write-stream cell   | styled cell for a `WriteOnlyWorksheet` (the streamed-style carrier) |
|  [13]   | `packaging.core.DocumentProperties`        | core metadata       | `Workbook.properties` metadata seam (title/creator/…)               |
|  [14]   | `workbook.protection.WorkbookProtection`   | workbook lock       | `Workbook.security` structure/window lock + password hash           |
|  [15]   | `worksheet.protection.SheetProtection`     | sheet lock          | `Worksheet.protection` per-sheet lock + allowances                  |

[PUBLIC_TYPE_SCOPE]: style, chart, formatting, and formula families
- rail: office

Family members:
- `styles`: `Font`/`PatternFill`/`GradientFill`/`Border`/`Side`/`Alignment`/`Protection`/`Color`/`NamedStyle`
- `chart`: `BarChart`/`LineChart`/`ScatterChart`/`PieChart`/`AreaChart`/`RadarChart`/`BubbleChart`/`SurfaceChart`/`StockChart` (+ `BarChart3D`/`LineChart3D`/`PieChart3D`/`AreaChart3D`/`SurfaceChart3D`), `Reference`, `Series`
- `formatting.rule`: `CellIsRule`/`ColorScaleRule`/`DataBarRule`/`IconSetRule`/`FormulaRule`/`Rule`
- `worksheet.formula`: `ArrayFormula(ref, text=None)`, `DataTableFormula(ref, ca, dt2D, dtr, r1, r2, del1, del2)`
- `utils`: `get_column_letter`/`column_index_from_string`/`range_boundaries`/`coordinate_to_tuple`/`absolute_coordinate`/`quote_sheetname`/`rows_from_range`/`cols_from_range`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `styles`                                | style module       | style-component value objects + `NamedStyle`                       |
|  [02]   | `styles.differential.DifferentialStyle` | delta style        | partial-style overlay a `Rule` applies on match (font/fill/border) |
|  [03]   | `chart`                                 | chart module       | the full chart family + `Reference`/`Series`                       |
|  [04]   | `formatting.rule`                       | conditional module | conditional-rule kinds, each → a `DifferentialStyle`               |
|  [05]   | `formula.translate.Translator`          | formula engine     | shift refs on formula copy; `translate_range` for a range          |
|  [06]   | `formula.tokenizer.Tokenizer`           | formula lexer      | tokenize a formula into `Token` operands/operators/functions       |
|  [07]   | `worksheet.formula.ArrayFormula`        | array formula      | spilled/dynamic array formula on a `ref`                           |
|  [08]   | `worksheet.formula.DataTableFormula`    | table formula      | one/two-variable what-if data table on a `ref`                     |
|  [09]   | `worksheet.table.TableStyleInfo`        | table style        | banded-row/column table styling applied to a `Table`               |
|  [10]   | `worksheet.filters.AutoFilter`          | filter axis        | `auto_filter` `add_filter_column`/`add_sort_condition`             |
|  [11]   | `worksheet.page.PrintPageSetup`         | page setup         | orientation/paper/fit-to-pages/scale                               |
|  [12]   | `worksheet.page.PageMargins`            | page margins       | print-margin geometry (`PageMargins`)                              |
|  [13]   | `utils`                                 | coordinate utils   | A1/coordinate + range converters                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open, build, and save
- rail: office

`load_workbook` carries the read policy (streaming, cached-value, macro-preserve, link, rich-text); `Workbook` carries the build policy (streaming, ISO-date). Named styles and defined names register on the workbook once and bind by reference.

Call shapes by index:
- [01]: `Workbook` — `Workbook(write_only=False, iso_dates=False)`
- [02]: `load_workbook` — `load_workbook(filename, read_only=False, keep_vba=False, data_only=False, keep_links=True, rich_text=False)`
- [03]: `Workbook.active` — property -> `Worksheet`
- [04]: `Workbook.create_sheet` — `create_sheet(title=None, index=None)` -> `Worksheet`
- [05]: `Workbook.copy_worksheet` — `copy_worksheet(from_worksheet)` -> `Worksheet`
- [06]: `Workbook.create_chartsheet` — `create_chartsheet(title=None, index=None)` -> `Chartsheet`
- [07]: `Workbook.remove` — `remove(worksheet)`
- [08]: `Workbook.move_sheet` — `move_sheet(sheet, offset=0)`
- [09]: `Workbook.add_named_style` — `add_named_style(style)`
- [10]: `Workbook.create_named_range` — `create_named_range(name, worksheet=None, value=None, scope=None)`
- [11]: `Workbook.defined_names` — mapping property (`DefinedNameDict`)
- [12]: `Workbook.properties` — property -> `DocumentProperties`
- [13]: `Workbook.security` — property -> `WorkbookProtection` (`Workbook.security = WorkbookProtection(workbookPassword=..., lockStructure=True)`)
- [14]: `Workbook.sheetnames` / `Workbook[title]` — property -> `list[str]` / `__getitem__(title) -> Worksheet`
- [15]: `Workbook.iso_dates` / `epoch` / `template` — properties
- [16]: `Workbook.save` — `save(filename)`
- [17]: `Workbook.close` — `close()`

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                          |
| :-----: | :------------------------------------------ | :---------------------------------------------------- |
|  [01]   | `Workbook`                                  | build a workbook (write-only streaming mode optional) |
|  [02]   | `load_workbook`                             | open an existing workbook with read policy            |
|  [03]   | `Workbook.active`                           | the active sheet                                      |
|  [04]   | `Workbook.create_sheet`                     | add a sheet at an optional index                      |
|  [05]   | `Workbook.copy_worksheet`                   | duplicate a sheet (in-file only)                      |
|  [06]   | `Workbook.create_chartsheet`                | add a full-sheet chart sheet                          |
|  [07]   | `Workbook.remove`                           | delete a sheet (the inverse of `create_sheet`)        |
|  [08]   | `Workbook.move_sheet`                       | reorder a sheet by signed offset                      |
|  [09]   | `Workbook.add_named_style`                  | register a reusable `NamedStyle`                      |
|  [10]   | `Workbook.create_named_range`               | register a `DefinedName` range                        |
|  [11]   | `Workbook.defined_names`                    | workbook-scoped name -> `DefinedName` resolution      |
|  [12]   | `Workbook.properties`                       | descriptive metadata; the `exchange/metadata` seam    |
|  [13]   | `Workbook.security`                         | workbook structure/window lock (`egress#PROTECT`)     |
|  [14]   | `Workbook.sheetnames` / `Workbook[title]`   | sheet lookup by title (no `get_sheet_by_name` verb)   |
|  [15]   | `Workbook.iso_dates` / `epoch` / `template` | ISO date toggle; `1900`/`1904` epoch; `.xltx` flag    |
|  [16]   | `Workbook.save`                             | serialize the workbook to a path or stream            |
|  [17]   | `Workbook.close`                            | release `read_only`/`write_only` file handles         |

[ENTRYPOINT_SCOPE]: sheet cell, row, structure, and feature access
- rail: office

One `Worksheet` owns addressed access (`cell`), bulk iteration (`iter_rows`/`iter_cols` with `values_only`), append-streaming (`append`), structural edits (`insert`/`delete`/`move_range`), and feature attach (`add_chart`/`add_image`/`add_table`/`add_data_validation`/`add_pivot`). `values_only` is a call flag, never a separate function.

Call shapes by index:
- [01]: `Worksheet.cell` — `cell(row, column, value=None)` -> `Cell`
- [02]: `Worksheet.append` — `append(iterable)`
- [03]: `Worksheet.iter_rows` — `iter_rows(min_row=None, max_row=None, min_col=None, max_col=None, values_only=False)`
- [04]: `Worksheet.iter_cols` — `iter_cols(min_col=None, max_col=None, min_row=None, max_row=None, values_only=False)`
- [05]: `Worksheet.merge_cells` / `unmerge_cells` — `merge_cells(range_string=None, start_row=None, start_column=None, end_row=None, end_column=None)` / `unmerge_cells(...)`
- [06]: `Worksheet.move_range` — `move_range(cell_range, rows=0, cols=0, translate=False)`
- [07]: `Worksheet.insert_rows` — `insert_rows(idx, amount=1)` / `insert_cols` / `delete_rows` / `delete_cols`
- [08]: `Worksheet.column_dimensions` / `row_dimensions` — `column_dimensions['A'].width = ...` / `row_dimensions[1].height = ...` (`DimensionHolder` mappings)
- [09]: `Worksheet.add_chart` — `add_chart(chart, anchor=None)`
- [10]: `Worksheet.add_image` — `add_image(img, anchor=None)`
- [11]: `Worksheet.add_table` — `add_table(table)`
- [12]: `Worksheet.add_data_validation` — `add_data_validation(data_validation)`
- [13]: `Worksheet.add_pivot` — `add_pivot(pivot)`
- [14]: `Worksheet.freeze_panes` — property (coordinate or cell, e.g. `"A2"`)
- [15]: `Worksheet.auto_filter` — property -> `AutoFilter`; `auto_filter.ref = "A1:D99"`, `.add_filter_column(col, vals)`, `.add_sort_condition(ref)`
- [16]: `Worksheet.conditional_formatting.add` — `add(range_string, cfRule)` (the `conditional_formatting` instance is a `ConditionalFormattingList`)
- [17]: `Worksheet.protection` — property -> `SheetProtection`; `protection.sheet = True`, `protection.password = ...`

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                                                                          |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Worksheet.cell`                                 | addressed cell read/write                                                             |
|  [02]   | `Worksheet.append`                               | append a row (the write-only streaming op)                                            |
|  [03]   | `Worksheet.iter_rows`                            | row iteration (cells or raw values)                                                   |
|  [04]   | `Worksheet.iter_cols`                            | column iteration (cells or raw values)                                                |
|  [05]   | `Worksheet.merge_cells` / `unmerge_cells`        | merge / split a region                                                                |
|  [06]   | `Worksheet.move_range`                           | shift a range, optionally re-translating formulas                                     |
|  [07]   | `Worksheet.insert_rows`                          | structural row/column edits                                                           |
|  [08]   | `Worksheet.column_dimensions` / `row_dimensions` | per-column width / per-row height + hidden/outline level                              |
|  [09]   | `Worksheet.add_chart`                            | embed a chart at an anchor                                                            |
|  [10]   | `Worksheet.add_image`                            | embed an image                                                                        |
|  [11]   | `Worksheet.add_table`                            | register a structured `Table` object                                                  |
|  [12]   | `Worksheet.add_data_validation`                  | attach a `DataValidation` rule                                                        |
|  [13]   | `Worksheet.add_pivot`                            | attach a pivot table definition                                                       |
|  [14]   | `Worksheet.freeze_panes`                         | freeze rows/columns above-left of a cell                                              |
|  [15]   | `Worksheet.auto_filter`                          | filterable + sortable header range (an `AutoFilter` object, never an `.add()` method) |
|  [16]   | `Worksheet.conditional_formatting.add`           | attach a conditional-format rule                                                      |
|  [17]   | `Worksheet.protection`                           | per-sheet lock the workbook `egress#PROTECT` seal complements                         |

[ENTRYPOINT_SCOPE]: style, chart, formatting, formula, and coordinate authoring
- rail: office

Styles mint once and share by reference; conditional-format and chart objects are kind rows; `formula.Translator` shifts references when a formula is copied; `utils` converts between A1 coordinates and zero/one-based tuples for boundary mapping.

Call shapes by index:
- [01]: `styles.NamedStyle` — `NamedStyle(name='Normal', font=None, fill=None, border=None, alignment=None, number_format=None, protection=None, builtinId=None)`
- [02]: `styles.Font` / `PatternFill` / `Border` / `Side` / `Alignment` / `Protection` — constructor each
- [03]: `chart.Reference` — `Reference(worksheet=None, min_col=None, min_row=None, max_col=None, max_row=None, range_string=None)`
- [04]: `chart.<Kind>Chart` — `BarChart()` / `LineChart()` / `ScatterChart()` / ... with `.add_data(ref)`/`.set_categories(ref)`
- [05]: `formatting.rule.ColorScaleRule` — `ColorScaleRule(start_type, start_value, start_color, end_type, end_value, end_color)`
- [06]: `formatting.rule.CellIsRule` / `DataBarRule` / `IconSetRule` / `FormulaRule` — constructor each
- [07]: `formula.translate.Translator` — `Translator(formula, origin)` -> `.translate_formula(dest)`; `Translator.translate_range(range_str, rdelta, cdelta)`
- [08]: `worksheet.formula.ArrayFormula` / `DataTableFormula` — `ArrayFormula(ref, text=None)` / `DataTableFormula(ref, ca, dt2D, dtr, r1, r2, del1, del2)`
- [09]: `utils.get_column_letter` — `get_column_letter(idx)` / `column_index_from_string(letter)` / `range_boundaries(range_string)` / `coordinate_to_tuple(coord)` / `absolute_coordinate(coord)` / `quote_sheetname(name)`
- [10]: `utils.dataframe.dataframe_to_rows` — `dataframe_to_rows(df, index=True, header=True)`

| [INDEX] | [SURFACE]                                             | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `styles.NamedStyle`                                   | a reusable named style registered on the workbook          |
|  [02]   | `styles.Font`/`PatternFill`/… (list above)            | style-component value objects for `Cell.font`/`.fill`/…    |
|  [03]   | `chart.Reference`                                     | a data range for a chart series                            |
|  [04]   | `chart.<Kind>Chart`                                   | a chart of one kind (the kind is a row)                    |
|  [05]   | `formatting.rule.ColorScaleRule`                      | a 2/3-color-scale conditional rule                         |
|  [06]   | `formatting.rule.CellIsRule`/… (list above)           | the conditional-rule kinds (each a row)                    |
|  [07]   | `formula.translate.Translator`                        | shift a formula's refs to `dest` or a range by delta       |
|  [08]   | `worksheet.formula.ArrayFormula` / `DataTableFormula` | spilled array formula or one/two-var what-if data table    |
|  [09]   | `utils.get_column_letter`                             | A1 <-> tuple coordinate conversion at the boundary         |
|  [10]   | `utils.dataframe.dataframe_to_rows`                   | stream a pandas/polars-compatible frame into `append` rows |

[ENTRYPOINT_SCOPE]: page-setup, print, and document metadata
- rail: office

The print-production + drawing-sheet plane sits on the sheet as state setters, never a parallel "page" object: `page_setup`/`page_margins`/`print_area`/`print_title_rows` make one workbook serve screen and ISO 5457 plotted output, and `Workbook.properties` is the descriptive-metadata seam the `exchange/metadata#METADATA` owner reads off an emitted workbook.

Call shapes by index:
- [01]: `Worksheet.page_setup` — `page_setup.orientation='landscape'`, `.paperSize=PAPERSIZE_A4`, `.fitToWidth=1`, `.fitToHeight=0`, `.scale=100` (`sheet_properties.pageSetUpPr.fitToPage=True` gates fit)
- [02]: `Worksheet.page_margins` — `page_margins = PageMargins(left=0.7, right=0.7, top=0.75, bottom=0.75, header=0.3, footer=0.3)`
- [03]: `Worksheet.print_area` — `print_area = 'A1:D99'`
- [04]: `Worksheet.print_title_rows` / `print_title_cols` — `print_title_rows = '1:1'` / `print_title_cols = 'A:A'`
- [05]: `Worksheet.sheet_properties.tabColor` — `sheet_properties.tabColor = 'FF0000'`
- [06]: `Worksheet.oddHeader` / `oddFooter` — `oddHeader.center.text = '&P of &N'`, `.left`/`.right` `HeaderFooterItem`
- [07]: `Workbook.properties` — `properties.title=`, `.creator=`, `.subject=`, `.keywords=`, `.category=`, `.created=`, `.lastPrinted=`

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------- |
|  [01]   | `Worksheet.page_setup`                            | orientation / paper / fit-to-pages / scale for plotted output  |
|  [02]   | `Worksheet.page_margins`                          | print-margin geometry in inches                                |
|  [03]   | `Worksheet.print_area`                            | bound the printable range                                      |
|  [04]   | `Worksheet.print_title_rows` / `print_title_cols` | repeat title rows/cols on every printed page (ISO 5457 banner) |
|  [05]   | `Worksheet.sheet_properties.tabColor`             | sheet-tab color                                                |
|  [06]   | `Worksheet.oddHeader` / `oddFooter`               | header/footer with `&P`/`&N`/`&D` field codes                  |
|  [07]   | `Workbook.properties`                             | core OOXML descriptive metadata (the `exchange/metadata` seam) |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_XLSX]:
- import: `import openpyxl` (or targeted `from openpyxl import Workbook, load_workbook`, `from openpyxl.styles import NamedStyle, Font`, `from openpyxl.chart import BarChart, Reference`, `from openpyxl.formatting.rule import ColorScaleRule`) at boundary scope only; module-level import is banned by the manifest import policy.
- workbook axis: one `Workbook` owns the sheet lifecycle (`create_sheet`/`copy_worksheet`/`create_chartsheet`/`remove`/`move_sheet`), named styles, defined names, and save; `Workbook(write_only=True)` (append-only write streaming yielding `WriteOnlyWorksheet`) and `load_workbook(read_only=True)` (lazy read streaming yielding `ReadOnlyWorksheet`) are mode rows for large workbooks, never parallel workbook types; `data_only=True` reads the last cached values instead of formula text, `keep_vba=True` round-trips `.xlsm` macros, `rich_text=True` reads inline `CellRichText`.
- cell axis: `Worksheet.cell`/`append`/`iter_rows`/`iter_cols` are the grid access rows; `values_only` selects raw-value vs `Cell`-object iteration — a call flag, never separate functions; structural `insert_rows`/`delete_cols`/`move_range` shift the grid in place, with `translate=True` re-anchoring moved formulas; `merge_cells`/`unmerge_cells` own merge regions and `column_dimensions[letter].width`/`row_dimensions[idx].height` own per-column/row geometry — never an open-coded width heuristic.
- styling axis: the `styles` module (`Font`/`PatternFill`/`GradientFill`/`Border`/`Side`/`Alignment`/`Protection`/`NamedStyle`) mints reusable value objects assigned by reference to `Cell.font`/`.fill`/...; `NamedStyle` registers once via `add_named_style` and is referenced by name, never per-cell duplication; `CellRichText`/`TextBlock`/`InlineFont` carry mixed-style runs inside one cell.
- formula axis: formula strings assign to `Cell.value`; `formula.Translator(formula, origin).translate_formula(dest)` (or `.translate_range(range_str, rdelta, cdelta)` for a whole range) shifts relative references when a formula is copied, never a manual offset rewrite; `ArrayFormula(ref, text)` carries spilled/dynamic arrays and `DataTableFormula(ref, ...)` carries one/two-variable what-if data tables; `formula.Tokenizer` lexes a formula for inspection.
- feature axis: `chart.<Kind>Chart` + `Reference`/`Series` + `add_chart`, `worksheet.table.Table` + `TableStyleInfo` + `add_table`, `DataValidation` + `add_data_validation`, conditional `formatting.rule.*Rule` + `conditional_formatting.add`, and `add_pivot` are the feature-attach rows; the chart/rule/validation kind is a row, never a per-feature sheet type. Conditional rules lower to one `styles.differential.DifferentialStyle`; the chart kind discriminates `BarChart`/`LineChart`/.../`*3D`, never a chart subclass per axis.
- print axis: `page_setup` (orientation/`paperSize`/`fitToWidth`/`fitToHeight`/`scale`, gated on `sheet_properties.pageSetUpPr.fitToPage`), `page_margins` (`PageMargins`), `print_area`, `print_title_rows`/`print_title_cols`, and `oddHeader`/`oddFooter` `&P`/`&N` field codes are sheet-state setters, never a parallel page object — the same workbook serves screen and the ISO 5457 plotted drawing-sheet the brief's pub/print + drawing plane demands.
- metadata axis: `Workbook.properties` (`DocumentProperties` title/creator/subject/keywords/category/created/lastPrinted) is the descriptive-metadata write the `folder:../exchange/metadata#METADATA` owner reads back off an emitted workbook; `Workbook.security`/`Worksheet.protection` (`WorkbookProtection`/`SheetProtection`) are the in-package structure/sheet lock the `folder:document/egress#PROTECT` `msoffcrypto` Office-seal complements (lock != encrypt — protection is metadata, the agile-container seal is the egress finisher).
- interchange axis: `utils.dataframe.dataframe_to_rows` streams a pandas/polars-compatible frame into `append`; `utils.get_column_letter`/`column_index_from_string`/`range_boundaries`/`coordinate_to_tuple`/`quote_sheetname` are the A1<->tuple boundary converters — internal code uses these, never an open-coded base-26 column math (the emit arm already binds `from openpyxl.utils import get_column_letter` for the `_HEADER_RANGE` `define_name`).
- integration (BOTH .api tiers stacked into ONE rail): the `folder:document/emit#DOCUMENT` `XLSX` arm `_xlsx_in_memory` lowers the one `folder:document/model#NODE` `TableNode` grid — `openpyxl.Workbook(write_only=True, iso_dates=True)` -> `create_sheet(spec.sheet or None)` -> per-row `sheet.append([value.write_openpyxl() for value in row])` -> `freeze_panes = "A2"` -> `save(BytesIO())` — where each `CellValue` (a `msgspec.Struct`, `libs/python/.api/msgspec.md`) projects through `write_openpyxl()` returning the typed scalar (`iso_dates=True` lets `append` serialize tz-aware `datetime`/`date` natively, never a `str()` coercion that loses the cell type); the resulting bytes thread onto one `EmitFact` carrier (`copy.replace`) the `@receipted` weave drains, and the producer mints exactly the existing `folder:../core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case off the `Backend.kind=OFFICE` discriminant — never a sibling receipt shape. A row count/sheet extent/streaming-mode `opentelemetry` span (`libs/python/.api/opentelemetry-api.md`) stamps the boundary; the conditional-format/number-format/validation options derive from the SAME `msgspec`/`pydantic` (`libs/python/.api/pydantic.md`) column schema that drives the writes, never hand-built twice; admission rejects the bad `EmitPayload` shape through a `pydantic` `TypeAdapter` and an arm raise converts to the runtime `BoundaryFault` via `async_boundary` (`expression`-rail `Result`, `libs/python/.api/expression.md`), exactly as the sibling office arms route. The READ side splits by speed: `folder:exchange/detect#DETECT` routes `MediaClass.SPREADSHEET` here for a styled/feature-preserving `load_workbook(read_only=True, data_only=True)` walk, while the `document/lens#LENS` `XLSX_READ` arm routes a pure cell-grid harvest to `folder:.api/python-calamine#OFFICE_INGEST_XLSX` (Rust, faster) — openpyxl reads when style/chart/validation/defined-name fidelity matters, calamine when only the values do.
- evidence: each workbook op captures sheet count, populated cell extent, named-style count, defined-name count, chart/table/validation/pivot counts, streaming mode (`write_only`/`read_only`/in-RAM), and output byte length onto the `EmitFact`/office receipt carrier — never re-derived off the bytes by a second reader.
- boundary: openpyxl owns `.xlsx`/`.xlsm`/`.xltx` read+write including `read_only`/`write_only` streaming, the style/chart/validation/defined-name/page-setup fidelity those modes preserve, and the `properties`/`security` metadata+lock axes; constant-memory bulk authoring at near-O(1) row memory routes to `folder:.api/xlsxwriter#SPREADSHEET_STREAMING_ROW` (write-only, no read) when the dataset exceeds openpyxl's `write_only` budget; bleeding-fast value-only Excel READ routes to `folder:.api/python-calamine#OFFICE_INGEST_XLSX`; ODF spreadsheets route to `folder:.api/odfpy#OFFICE_ODF`; Word to `folder:.api/python-docx`, PowerPoint to `folder:.api/python-pptx`; encrypted ECMA-376 workbooks decrypt through `folder:.api/msoffcrypto-tool` (or the `document/egress#PROTECT` unlock arm) before `load_workbook`; the document content TREE and its receipt/content-key are the runtime's, never re-minted here; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `openpyxl`
- Owns: `.xlsx`/`.xlsm`/`.xltx` read+write, `read_only`/`write_only` streaming modes, formulas/values with cached-value read and reference-translating formula engine, named + inline-rich styles over `DifferentialStyle`, conditional formatting, the full chart family (+3D), images, worksheet tables, data validation, pivot-table definitions, defined names, array/data-table formulas, print-production page setup, workbook+sheet protection, document core properties, and pandas/polars dataframe interchange
- Accept: style/chart/validation/defined-name-fidelity spreadsheet authoring + ingestion feeding the `document/emit`/`exchange/detect` office and export-bundle owners, including decrypted workbooks from `msoffcrypto-tool`, print-ready sheets via the page-setup family, and the `TableNode`-grid lowering through `CellValue.write_openpyxl`
- Reject: wrapper-renames of `cell`/`save`/`load_workbook`/`create_sheet`; a per-cell style rebuild where `NamedStyle` exists; an open-coded column-letter or formula-offset math where `utils`/`formula.Translator` own it; a `str()` coercion at the append site where `iso_dates=True` + `write_openpyxl` lands the typed cell; a `get_sheet_by_name` verb where `Workbook[title]` discriminates; a CSV fallback where rich `.xlsx` is admitted; a parallel workbook type per streaming mode; reaching for `xlsxwriter` for anything but read-free constant-memory bulk streaming, or staying on openpyxl for a pure value-only Excel read `python-calamine` reads faster; a sibling receipt shape where `ArtifactReceipt.Office` is the case; identity/content-key minting the runtime owns
