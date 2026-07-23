# [PY_ARTIFACTS_API_OPENPYXL]

`openpyxl` owns read+write `.xlsx`/`.xlsm`/`.xltx` for the artifacts office rail: one `Workbook` root whose `write_only`/`read_only` flags select streaming mirrors, a `Worksheet` grid, and the style, chart, conditional-format, table, validation, pivot, page-setup, and `formula.Translator` families over full OOXML fidelity. `openpyxl` never re-implements the OOXML parse, SpreadsheetML packaging, or formula reference-translation it already owns; the document emit owner lowers its `TableNode` grid through this surface and the detect gate routes spreadsheet ingest here, constant-memory bulk write routing to `xlsxwriter` and value-only fast read to `python-calamine`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openpyxl`
- package: `openpyxl` (MIT)
- module: `openpyxl`
- abi: pure-Python, no native build
- rail: office — OOXML spreadsheet authoring and fidelity-preserving ingest feeding the document owners

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook and sheet roots

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [CAPABILITY]                                                        |
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
- `styles`: `Font` `PatternFill` `GradientFill` `Border` `Side` `Alignment` `Protection` `Color` `NamedStyle`
- `chart`: `BarChart` `LineChart` `ScatterChart` `PieChart` `AreaChart` `RadarChart` `BubbleChart` `SurfaceChart` `StockChart` `BarChart3D` `LineChart3D` `PieChart3D` `AreaChart3D` `SurfaceChart3D` `Reference` `Series`
- `formatting.rule`: `CellIsRule` `ColorScaleRule` `DataBarRule` `IconSetRule` `FormulaRule` `Rule`
- `utils`: `get_column_letter` `column_index_from_string` `range_boundaries` `coordinate_to_tuple` `absolute_coordinate` `quote_sheetname` `rows_from_range` `cols_from_range`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `styles`                                | style module       | style-component value objects + `NamedStyle`                       |
|  [02]   | `styles.differential.DifferentialStyle` | delta style        | partial-style overlay a `Rule` applies on match (font/fill/border) |
|  [03]   | `chart`                                 | chart module       | the full chart family + `Reference`/`Series`                       |
|  [04]   | `formatting.rule`                       | conditional module | conditional-rule kinds, each lowering to a `DifferentialStyle`     |
|  [05]   | `formula.translate.Translator`          | formula engine     | shift refs on formula copy; `translate_range` for a range          |
|  [06]   | `formula.tokenizer.Tokenizer`           | formula lexer      | tokenize a formula into `Token` operands/operators/functions       |
|  [07]   | `worksheet.formula.ArrayFormula`        | array formula      | spilled/dynamic array formula on a `ref`                           |
|  [08]   | `worksheet.formula.DataTableFormula`    | table formula      | one/two-variable what-if data table on a `ref`                     |
|  [09]   | `worksheet.table.TableStyleInfo`        | table style        | banded-row/column table styling applied to a `Table`               |
|  [10]   | `worksheet.filters.AutoFilter`          | filter axis        | `auto_filter` `add_filter_column`/`add_sort_condition`             |
|  [11]   | `worksheet.page.PrintPageSetup`         | page setup         | orientation/paper/fit-to-pages/scale                               |
|  [12]   | `worksheet.page.PageMargins`            | page margins       | print-margin geometry                                              |
|  [13]   | `utils`                                 | coordinate utils   | A1/coordinate + range converters                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open, build, and save
- `load_workbook` read policy: `read_only`, `data_only` (cached values vs formula text), `keep_vba` (`.xlsm` macros), `keep_links`, `rich_text` (inline `CellRichText`)

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Workbook(write_only=False, iso_dates=False)`                      | build a workbook; `write_only=True` streams        |
|  [02]   | `load_workbook(filename, read_only, data_only, …) -> Workbook`     | open an existing workbook under the read policy    |
|  [03]   | `Workbook.active -> Worksheet`                                     | the active sheet                                   |
|  [04]   | `create_sheet(title=None, index=None) -> Worksheet`                | add a sheet at an optional index                   |
|  [05]   | `copy_worksheet(from_worksheet) -> Worksheet`                      | duplicate a sheet (in-file only)                   |
|  [06]   | `create_chartsheet(title=None, index=None) -> Chartsheet`          | add a full-sheet chart sheet                       |
|  [07]   | `remove(worksheet)`                                                | delete a sheet                                     |
|  [08]   | `move_sheet(sheet, offset=0)`                                      | reorder a sheet by signed offset                   |
|  [09]   | `add_named_style(style)`                                           | register a reusable `NamedStyle`                   |
|  [10]   | `create_named_range(name, worksheet=None, value=None, scope=None)` | register a `DefinedName` range                     |
|  [11]   | `defined_names -> DefinedNameDict`                                 | workbook-scoped name to `DefinedName` resolution   |
|  [12]   | `properties -> DocumentProperties`                                 | descriptive metadata; the `exchange/metadata` seam |
|  [13]   | `security -> WorkbookProtection`                                   | workbook structure/window lock (`egress#PROTECT`)  |
|  [14]   | `sheetnames -> list[str]` / `Workbook[title] -> Worksheet`         | sheet lookup by title (no `get_sheet_by_name`)     |
|  [15]   | `iso_dates` / `epoch` / `template`                                 | ISO date toggle; `1900`/`1904` epoch; `.xltx` flag |
|  [16]   | `save(filename)`                                                   | serialize the workbook to a path or stream         |
|  [17]   | `close()`                                                          | release `read_only`/`write_only` file handles      |

[ENTRYPOINT_SCOPE]: sheet cell, row, structure, and feature access
- `values_only` on `iter_rows`/`iter_cols` selects raw-value vs `Cell`-object iteration; feature attach (`add_chart`/`add_image`/`add_table`/`add_data_validation`/`add_pivot`) takes a minted object

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `cell(row, column, value=None) -> Cell`                                   | addressed cell read/write                           |
|  [02]   | `append(iterable)`                                                        | append a row (the write-only streaming op)          |
|  [03]   | `iter_rows(min_row, max_row, min_col, max_col, values_only=False)`        | row iteration (cells or raw values)                 |
|  [04]   | `iter_cols(min_col, max_col, min_row, max_row, values_only=False)`        | column iteration (cells or raw values)              |
|  [05]   | `merge_cells(range_string \| start_row, …)` / `unmerge_cells(…)`          | merge / split a region                              |
|  [06]   | `move_range(cell_range, rows=0, cols=0, translate=False)`                 | shift a range, `translate=True` re-anchors formulas |
|  [07]   | `insert_rows` / `insert_cols` / `delete_rows` / `delete_cols`             | structural row/column edits (`idx, amount=1`)       |
|  [08]   | `column_dimensions[letter].width` / `row_dimensions[idx].height`          | per-column width / per-row height + outline level   |
|  [09]   | `add_chart(chart, anchor=None)`                                           | embed a chart at an anchor                          |
|  [10]   | `add_image(img, anchor=None)`                                             | embed an image                                      |
|  [11]   | `add_table(table)`                                                        | register a structured `Table` object                |
|  [12]   | `add_data_validation(data_validation)`                                    | attach a `DataValidation` rule                      |
|  [13]   | `add_pivot(pivot)`                                                        | attach a pivot-table definition                     |
|  [14]   | `freeze_panes = "A2"`                                                     | freeze rows/columns above-left of a cell            |
|  [15]   | `auto_filter -> AutoFilter` (`.add_filter_column`, `.add_sort_condition`) | filterable + sortable header range                  |
|  [16]   | `conditional_formatting.add(range_string, cfRule)`                        | attach a conditional-format rule                    |
|  [17]   | `protection -> SheetProtection` (`.sheet`, `.password`)                   | per-sheet lock complementing `egress#PROTECT`       |

[ENTRYPOINT_SCOPE]: style, chart, formatting, formula, and coordinate authoring
- `ColorScaleRule(start_type, start_value, start_color, end_type, end_value, end_color)` is the color-scale ctor; each `formatting.rule` kind lowers to one `DifferentialStyle`; `formula.Translator(formula, origin)` shifts refs via `.translate_formula(dest)` / `.translate_range(range_str, rdelta, cdelta)`

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `NamedStyle(name, font, fill, border, alignment, number_format, protection, builtinId)` | reusable named style                    |
|  [02]   | `styles.Font` / `PatternFill` / `Border` / `Side` / `Alignment` / `Protection`          | style-component value objects           |
|  [03]   | `chart.Reference(worksheet, min_col, min_row, max_col, max_row, range_string)`          | a data range for a chart series         |
|  [04]   | `chart.<Kind>Chart()` (`.add_data`, `.set_categories`)                                  | a chart of one kind (the kind is a row) |
|  [05]   | `ColorScaleRule` / `CellIsRule` / `DataBarRule` / `IconSetRule` / `FormulaRule`         | conditional-rule kinds (each a row)     |
|  [06]   | `formula.translate.Translator(formula, origin)`                                         | shift a formula's refs on copy          |
|  [07]   | `worksheet.formula.ArrayFormula(ref, text=None)`                                        | spilled/dynamic array formula           |
|  [08]   | `worksheet.formula.DataTableFormula(ref, ca, dt2D, dtr, r1, r2, del1, del2)`            | one/two-variable what-if data table     |
|  [09]   | `utils.get_column_letter(idx)` / `column_index_from_string` / `range_boundaries`        | A1<->tuple coordinate conversion        |
|  [10]   | `utils.dataframe.dataframe_to_rows(df, index=True, header=True)`                        | stream a frame into `append` rows       |

[ENTRYPOINT_SCOPE]: page-setup, print, and document metadata
- print setters live on the sheet, never a parallel page object; `page_setup` carries `.orientation`/`.paperSize`/`.fitToWidth`/`.fitToHeight`/`.scale` (gated on `sheet_properties.pageSetUpPr.fitToPage`); `Workbook.properties` carries `.title`/`.creator`/`.subject`/`.keywords`/`.category`/`.created`/`.lastPrinted`

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `Worksheet.page_setup`                                                    | orientation / paper / fit / scale for plotting |
|  [02]   | `page_margins = PageMargins(left, right, top, bottom, header, footer)`    | print-margin geometry in inches                |
|  [03]   | `print_area = 'A1:D99'`                                                   | bound the printable range                      |
|  [04]   | `print_title_rows = '1:1'` / `print_title_cols = 'A:A'`                   | repeat title rows/cols (ISO 5457 banner)       |
|  [05]   | `sheet_properties.tabColor`                                               | sheet-tab color                                |
|  [06]   | `oddHeader` / `oddFooter` (`.left`/`.center`/`.right` `HeaderFooterItem`) | header/footer with `&P`/`&N`/`&D` field codes  |
|  [07]   | `Workbook.properties`                                                     | core OOXML descriptive metadata                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- workbook: one `Workbook` owns sheet lifecycle, named styles, defined names, and save; `write_only`/`read_only` are mode flags yielding `WriteOnlyWorksheet`/`ReadOnlyWorksheet`, never parallel workbook types; `data_only` reads cached values, `keep_vba` round-trips `.xlsm`, `rich_text` reads inline `CellRichText`.
- grid: `values_only` selects raw-value vs `Cell`-object iteration, a call flag never a separate function; `insert_rows`/`delete_cols`/`move_range(translate=True)` shift the grid in place; `column_dimensions[letter].width`/`row_dimensions[idx].height` own geometry, never an open-coded width heuristic.
- style: `styles` value objects assign by reference to `Cell.font`/`.fill`; `NamedStyle` registers once via `add_named_style` and binds by name, never per-cell duplication; `CellRichText`/`TextBlock`/`InlineFont` carry mixed-style runs in one cell.
- formula: `formula.Translator(...).translate_formula`/`.translate_range` shifts relative refs on copy, never a manual offset rewrite; `ArrayFormula` carries spilled/dynamic arrays and `DataTableFormula` one/two-variable what-if tables; `Tokenizer` lexes for inspection.
- feature: the chart/rule/validation KIND is a row, never a per-feature sheet type; conditional rules lower to one `styles.differential.DifferentialStyle`; the chart kind discriminates `BarChart`…`*3D`, never a subclass per axis.
- print: `page_setup`/`page_margins`/`print_area`/`print_title_rows`/`oddHeader` are sheet-state setters, never a parallel page object — one workbook serves screen and the ISO 5457 plotted drawing-sheet.
- metadata: `Workbook.properties` is the descriptive-metadata write; `Workbook.security`/`Worksheet.protection` are the in-package structure/sheet lock (lock != encrypt — the agile-container seal is the egress finisher).
- interchange: `utils` A1<->tuple converters and `dataframe_to_rows` are the boundary; internal code never open-codes base-26 column math.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): each `CellValue` `msgspec.Struct` projects through `write_openpyxl()` to the typed scalar the emit arm's `append` lands, and `iso_dates=True` serializes tz-aware `datetime`/`date` natively.
- `pydantic`(`.api/pydantic.md`): admission rejects a bad `EmitPayload` through a `TypeAdapter`, and the conditional-format/number-format/validation options derive from the same column schema that drives the writes, never hand-built twice.
- `expression`(`.api/expression.md`): an arm raise converts to the runtime `BoundaryFault` via `async_boundary` on the `Result` rail, exactly as the sibling office arms route.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): one span stamps sheet/cell extent, style/name/feature counts, streaming mode, and output byte length onto the `EmitFact` carrier, never re-derived off the bytes by a second reader.
- `xlsxwriter`(`.api/xlsxwriter.md#SPREADSHEET_STREAMING_ROW`): constant-memory bulk authoring (write-only, no read) when a dataset exceeds openpyxl's `write_only` budget.
- `python-calamine`(`.api/python-calamine.md#OFFICE_INGEST_XLSX`): value-only fast Excel read (the `document/lens#LENS` `XLSX_READ` arm); openpyxl reads when style/chart/validation/defined-name fidelity matters, calamine when only the values do.
- `msoffcrypto-tool`(`.api/msoffcrypto-tool.md`): encrypted ECMA-376 workbooks decrypt (or route through the `document/egress#PROTECT` unlock arm) before `load_workbook`; ODF routes to `odfpy`, Word to `python-docx`, PowerPoint to `python-pptx`.
- `document/emit#DOCUMENT` `XLSX` arm: `_xlsx_in_memory` lowers the `document/model#NODE` `TableNode` grid — `Workbook(write_only=True, iso_dates=True)` -> `create_sheet` -> per-row `append([value.write_openpyxl() for value in row])` -> `freeze_panes = "A2"` -> `save(BytesIO())`; bytes thread onto one `EmitFact` (`copy.replace`) the `@receipted` weave drains, and the producer mints the `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case off the `Backend.kind=OFFICE` discriminant. Read side: `exchange/detect#DETECT` routes `MediaClass.SPREADSHEET` to a `load_workbook(read_only=True, data_only=True)` walk.

[LOCAL_ADMISSION]:
- openpyxl is the owner when style/chart/validation/defined-name fidelity or feature-preserving read matters; value-only read defers to `python-calamine`, constant-memory bulk write to `xlsxwriter`.

[RAIL_LAW]:
- Package: `openpyxl`
- Owns: `.xlsx`/`.xlsm`/`.xltx` read+write with `read_only`/`write_only` streaming, cached-value read and the reference-translating formula engine, named + inline-rich styles over `DifferentialStyle`, conditional formatting, the full chart family, images, worksheet tables, data validation, pivot definitions, defined names, array/data-table formulas, print-production page setup, workbook+sheet protection, core document properties, and dataframe interchange
- Accept: fidelity-preserving spreadsheet authoring + ingestion feeding the `document/emit`/`exchange/detect` owners, decrypted workbooks from `msoffcrypto-tool`, print-ready sheets via the page-setup family, and the `TableNode`-grid lowering through `CellValue.write_openpyxl`
- Reject: wrapper-renames of `cell`/`save`/`load_workbook`/`create_sheet`; a per-cell style rebuild where `NamedStyle` exists; open-coded column-letter or formula-offset math where `utils`/`formula.Translator` own it; a `str()` coercion at the append site where `iso_dates=True` + `write_openpyxl` lands the typed cell; a `get_sheet_by_name` verb where `Workbook[title]` discriminates; a CSV fallback where rich `.xlsx` is admitted; a parallel workbook type per streaming mode; `xlsxwriter` for anything but read-free constant-memory bulk, or openpyxl for a value-only read `python-calamine` reads faster; a sibling receipt shape where `ArtifactReceipt.Office` is the case; identity/content-key minting the runtime owns
