# [PY_ARTIFACTS_API_XLSXWRITER]

`xlsxwriter` mints the constant-memory streaming XLSX/XLSM writer for the artifacts spreadsheet rail: one `Workbook` root opens a path or stream, mints `Worksheet`/`Format`/`Chart`/`Chartsheet` objects, emits Office Open XML row-major, and packages a zip64-capable container at `close`. Under `constant_memory` each completed row flushes to a temp file while only the active row stays resident, so the writer holds O(1) row memory and never re-implements the OOXML/zip64 packaging it owns; it writes only, and reading or editing an existing workbook routes to the ingest owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xlsxwriter`
- package: `xlsxwriter` (`BSD-2-Clause`)
- module: `xlsxwriter`
- abi: pure-Python, no native build
- rail: spreadsheet — write-only streaming XLSX/XLSM authoring feeding the artifacts download and export owners

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook root and minted objects

`Workbook` is the sole top-level export and the only directly-constructed object; `add_worksheet`/`add_chartsheet`/`add_format`/`add_chart` mint the rest. `xlsxwriter.exceptions` roots every fault under `XlsxWriterException`, branching to `XlsxInputError` (data/name/range) and `XlsxFileError` (zip/image/theme).

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------------ |
|  [01]   | `xlsxwriter.Workbook`                       | writer root   | open/own the XLSX container and mint sheets, formats, charts  |
|  [02]   | `xlsxwriter.worksheet.Worksheet`            | sheet writer  | row-major cell emission plus tables/validation/charts         |
|  [03]   | `xlsxwriter.chartsheet.Chartsheet`          | chart sheet   | a sheet holding a single full-page chart                      |
|  [04]   | `xlsxwriter.format.Format`                  | cell style    | reusable number/font/fill/border/align style object           |
|  [05]   | `xlsxwriter.chart.Chart`                    | chart builder | `add_series`/`set_*_axis`/`combine` bound to worksheet ranges |
|  [06]   | `xlsxwriter.exceptions.XlsxWriterException` | error root    | base of every xlsxwriter fault                                |
|  [07]   | `xlsxwriter.exceptions.XlsxInputError`      | error branch  | data/name/range faults                                        |
|  [08]   | `xlsxwriter.exceptions.XlsxFileError`       | error branch  | zip/image/theme faults                                        |

- [07]-[XLSX_INPUT_ERROR]: `DuplicateWorksheetName` `InvalidWorksheetName` `DuplicateTableName` `OverlappingRange` `EmptyChartSeries`
- [08]-[XLSX_FILE_ERROR]: `FileCreateError` `FileSizeError` `UndefinedImageSize` `UnsupportedImageFormat` `ThemeFileError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbook` lifecycle and minting

`Workbook` opens on a path or stream, mints the write-path objects, and `close` triggers the single serialization. A mutator returns `Literal[0, -1]` (0 applied, -1 rejected), a void setter returns `None`, and `filename` is `str | IO[AnyStr] | os.PathLike | None`.

- `Workbook` `options` keys: streaming `constant_memory` `in_memory` `tmpdir` `use_zip64`; coercion `strings_to_numbers` `strings_to_formulas` `strings_to_urls` `nan_inf_to_errors` `default_date_format` `remove_timezone` `date_1904` `max_url_length`.

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Workbook(filename=None, options=None)`                          | open a workbook on a path or stream                                  |
|  [02]   | `add_worksheet(name=None, worksheet_class=None) -> Worksheet`    | mint a data sheet (auto-named when `name` omitted)                   |
|  [03]   | `add_chartsheet(name=None, chartsheet_class=None) -> Chartsheet` | mint a single-chart full-page sheet                                  |
|  [04]   | `add_format(properties=None) -> Format`                          | mint a reusable cell-style object                                    |
|  [05]   | `add_chart(options) -> Chart \| None`                            | mint a chart by `{'type','subtype'}`; insert via `insert_chart`      |
|  [06]   | `define_name(name, formula)`                                     | register a workbook- or sheet-scoped defined name                    |
|  [07]   | `add_vba_project(vba_project, is_stream=False)`                  | embed a `vbaProject.bin` (extracted by `vba_extract.py`) for `.xlsm` |
|  [08]   | `set_properties(properties)`                                     | set core document properties (title/author/subject/keywords)         |
|  [09]   | `set_custom_property(name, value, property_type=None)`           | add one typed custom document property                               |
|  [10]   | `get_worksheet_by_name(name) -> Worksheet \| None`               | polymorphic lookup of a minted sheet by name                         |
|  [11]   | `worksheets() -> List[Worksheet]`                                | enumerate minted sheets in tab order                                 |
|  [12]   | `set_calc_mode(mode, calc_id=None)`                              | recalc policy                                                        |
|  [13]   | `read_only_recommended()`                                        | flag the file read-only on open                                      |
|  [14]   | `add_signed_vba_project(vba_project, signature, …)`              | embed a signed `vbaProject.bin` + signature for a trusted `.xlsm`    |
|  [15]   | `set_size(width, height)` / `set_tab_ratio(tab_ratio)`           | window pixel size / sheet-tab-to-scrollbar ratio                     |
|  [16]   | `use_zip64()`                                                    | enable zip64 at runtime, lifting the 4 GiB ceiling                   |
|  [17]   | `get_default_url_format() -> Format`                             | the implicit hyperlink `Format` `write_url` applies                  |
|  [18]   | `close()`                                                        | flush, package, and write the `.xlsx`/`.xlsm` container              |

- `set_custom_property`: `value` typed by `property_type` in `'bool'/'date'/'number'/'number_int'/'text'`.
- `set_calc_mode`: `mode` in `'auto'/'manual'/'auto_except_tables'`.

[ENTRYPOINT_SCOPE]: `Worksheet` row-major writes

`write` discriminates the value type and routes to the typed writers; `write_row`/`write_column` take a sequence with one shared `cell_format`. A positional cell is `(row, col, …)`, `<range>` abbreviates `(first_row, first_col, last_row, last_col)`, `cell_format` defaults `None`, and the typed writers return a `Literal[0, -1, …]` status.

| [INDEX] | [SURFACE]                                                                     | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `write(row, col, *args)`                                                      | type-dispatching write (str/number/bool/date/url) |
|  [02]   | `write_row(row, col, data, cell_format=None)`                                 | write a sequence across one row                   |
|  [03]   | `write_column(row, col, data, cell_format=None)`                              | write a sequence down one column                  |
|  [04]   | `write_string(row, col, string, cell_format=None)`                            | write a string cell                               |
|  [05]   | `write_number(row, col, number, cell_format=None)`                            | write a numeric cell                              |
|  [06]   | `write_boolean(row, col, boolean, cell_format=None)`                          | write a boolean cell                              |
|  [07]   | `write_datetime(row, col, date, cell_format=None)`                            | write a date/time cell (number-formatted)         |
|  [08]   | `write_formula(row, col, formula, cell_format=None, value=0)`                 | write a formula with cached value                 |
|  [09]   | `write_blank(row, col, blank, cell_format=None)`                              | write a formatted empty cell                      |
|  [10]   | `write_url(row, col, url, cell_format=None, string=None, tip=None)`           | write a hyperlink (external/internal/mailto)      |
|  [11]   | `write_rich_string(row, col, *args)`                                          | multi-format string; `Format`/text segments       |
|  [12]   | `write_array_formula(<range>, formula, value=0)`                              | write a CSE array formula over a range            |
|  [13]   | `write_dynamic_array_formula(<range>, formula, value=0)`                      | Excel 365 spilling dynamic-array formula          |
|  [14]   | `set_column(first_col, last_col, width=None, cell_format=None, options=None)` | set column width/default format/visibility        |
|  [15]   | `set_row(row, height=None, cell_format=None, options=None)`                   | set row height/default format                     |
|  [16]   | `freeze_panes(row, col, top_row=None, left_col=None, pane_type=0)`            | freeze header rows/columns                        |

[ENTRYPOINT_SCOPE]: `Worksheet` structured-feature family

Higher-level Excel features layer over cells; `options` is `Dict[str, Any] | None`, `<range>` opens `(first_row, first_col, last_row, last_col)`, and each surface returns a `Literal[0, -1, …]` code.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                             |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `conditional_format(<range>, options)`                 | cell/3-color-scale/data-bar/icon-set/formula conditional styling         |
|  [02]   | `data_validation(<range>, options)`                    | dropdown lists, ranges, input/error messages                             |
|  [03]   | `add_table(<range>, options=None)`                     | banded structured table with header/totals (no `constant_memory`)        |
|  [04]   | `add_sparkline(row, col, options)`                     | inline line/column/win-loss sparkline                                    |
|  [05]   | `autofilter(<range>)`                                  | declare a filterable header range                                        |
|  [06]   | `filter_column(col, criteria)`                         | apply a filter expression to one autofilter column                       |
|  [07]   | `merge_range(<range>, data, cell_format=None)`         | merge a range to one formatted value; returns the `write` code           |
|  [08]   | `insert_chart(row, col, chart, options=None)`          | embed an `add_chart` object at an anchor                                 |
|  [09]   | `insert_image(row, col, source, options=None)`         | embed a PNG/JPEG/GIF/BMP from path or in-memory stream                   |
|  [10]   | `insert_textbox(row, col, text, options=None)`         | place a floating text box                                                |
|  [11]   | `write_comment(row, col, comment, options=None)`       | attach a cell comment/note                                               |
|  [12]   | `protect(password="", options=None)`                   | sheet protection with feature allowances                                 |
|  [13]   | `add_write_handler(user_type, user_function)`          | type->writer hook so `write` dispatches a custom value type              |
|  [14]   | `embed_image(row, col, source, options=None)`          | in-cell Excel-365 `IMAGE()` vs floating `insert_image`                   |
|  [15]   | `insert_checkbox(row, col, boolean, cell_format=None)` | a boolean checkbox cell (Excel-365)                                      |
|  [16]   | `filter_column_list(col, filters)`                     | multi-value ("in" set) autofilter, the list mirror of `filter_column`    |
|  [17]   | `ignore_errors(options=None)`                          | suppress green-triangle error markers over a range (e.g. number-as-text) |
|  [18]   | `autofit(max_width=1790)`                              | size every column to its widest written cell at `close` (post-write)     |

- `add_write_handler`: `user_function` is `Callable[[Worksheet, int, int, Any, Format \| None], Any]` — the `(worksheet, row, col, value, cell_format)` shape `write` dispatches for a registered value type (`numpy.datetime64`, a `polars`/`pandas` scalar, `decimal.Decimal`, a value object).

[ENTRYPOINT_SCOPE]: `Worksheet` page setup and print layout

Print layout rides sheet-state setters available under `constant_memory`; a setter returns `None` and `print_area` returns `Literal[0, -1]`.

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `set_landscape()` / `set_portrait()`                                 | page orientation                                                |
|  [02]   | `set_paper(paper_size)`                                              | paper size code (`9`=A4, `1`=Letter, ...)                       |
|  [03]   | `fit_to_pages(width, height)`                                        | scale print to W×H pages (`fit_to_pages(1, 0)` = one page wide) |
|  [04]   | `set_margins(left=0.7, right=0.7, top=0.75, bottom=0.75)`            | print margins in inches                                         |
|  [05]   | `set_header(header='', options=None, margin=None)` / `set_footer(…)` | header/footer with `&P`/`&N`/`&D` field codes                   |
|  [06]   | `print_area(first_row, first_col, last_row, last_col)`               | bound the printable range                                       |
|  [07]   | `repeat_rows(first_row, last_row=None)` / `repeat_columns(…)`        | repeat title rows/cols on every printed page                    |
|  [08]   | `set_zoom(zoom=100)` / `set_tab_color(color)`                        | screen zoom / sheet-tab color                                   |
|  [09]   | `set_default_row(height=None, hide_unused_rows=False)`               | default row height; hide trailing empty rows                    |

[ENTRYPOINT_SCOPE]: `Chart` builder

`add_chart({'type','subtype'})` mints a `Chart` (`type` in `column`/`bar`/`line`/`area`/`pie`/`doughnut`/`scatter`/`stock`/`radar`); series bind worksheet ranges by `=Sheet1!$A$1:$A$10` strings and `insert_chart` (or `Chartsheet.set_chart`) anchors it. `options` is `Dict[str, Any]` and setters return `None`.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `add_series(options=None)`                                    | one data series bound to worksheet ranges              |
|  [02]   | `set_x_axis(options)` / `set_y_axis(options)`                 | primary axis name/range/number format/gridlines/log    |
|  [03]   | `set_x2_axis(options)` / `set_y2_axis(options)`               | secondary axes a `combine`d series binds to (Pareto)   |
|  [04]   | `set_title(options=None)`                                     | chart title and font                                   |
|  [05]   | `set_legend(options)`                                         | legend position/visibility/font                        |
|  [06]   | `set_style(style_id=2)`                                       | apply one of the built-in Excel chart styles           |
|  [07]   | `set_size(options=None)`                                      | chart pixel dimensions and scaling                     |
|  [08]   | `set_plotarea(options)` / `set_chartarea(options)`            | plot-area / chart-area fill, border, gradient          |
|  [09]   | `set_table(options=None)`                                     | render a data table beneath the plot                   |
|  [10]   | `combine(chart=None)`                                         | overlay a second chart type on a shared (or `y2`) axis |
|  [11]   | `set_high_low_lines`, `set_drop_lines`, `set_up_down_bars`    | stock/line connectors and up-down bars                 |
|  [12]   | `show_blanks_as`, `show_na_as_empty_cell`, `show_hidden_data` | gap/zero/connect policy for blank & `#N/A` cells       |

- `add_series`: `options` carry `values` `categories` `name` `line` `fill` `trendline` `data_labels` `y2_axis`.

[ENTRYPOINT_SCOPE]: `Format` style minting

`add_format` mints a `Format`; its `set_*` family (or the equivalent `add_format({...})` dict) configures number format, font, alignment, fill, border, rotation, indent, and protection. `set_align` alignment is a `Literal['left','center','right','top','vcenter', …]` and every setter returns `None`.

| [INDEX] | [SURFACE]                    | [CAPABILITY]                      |
| :-----: | :--------------------------- | :-------------------------------- |
|  [01]   | `set_num_format(num_format)` | apply an Excel number-format code |
|  [02]   | `set_bold(bold=True)`        | toggle bold font                  |
|  [03]   | `set_align(alignment)`       | horizontal/vertical alignment     |
|  [04]   | `set_bg_color(bg_color)`     | set cell fill color               |
|  [05]   | `set_border(style=1)`        | set the cell border style         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- streaming: `constant_memory=True` flushes each completed row to `tmpdir` and holds only the active row for O(1) row memory; `in_memory=True` keeps the spill in RAM when no disk path exists; `use_zip64=True` lifts the 4 GiB archive ceiling. Streaming is a policy row on `options`, never a parallel writer type.
- order: cells write in strict row-major order and a row seals when a higher row is touched; `set_row` height/format applies before the row's cells, and a write to a sealed row is dropped — the row index is the discriminant.
- write: `write` is the single type-dispatching entry over str/number/bool/datetime/url; `write_row`/`write_column` are bulk-sequence rows and `write_string`/`write_number`/`write_datetime` force one type. `add_write_handler` registers a domain value type so `write` dispatches it natively, foreclosing an `isinstance` ladder or a `str()` coercion at the call site.
- style: `add_format` mints one `Format` shared by reference across cells and sheets as a reused style-table entry, never a per-cell allocation; `get_default_url_format` returns the implicit hyperlink style so link formatting extends the same shared `Format`.
- feature: structured features, embedded objects, and page-setup setters are options-dict-or-scalar rows on `Worksheet`, never parallel sheet types; `embed_image` writes an in-cell Excel-365 image scaling with the cell where `insert_image` floats over the grid. `constant_memory` permits every feature except `add_table`, whose flush cannot back-patch the table XML, and `autofit` runs post-write before `close`.
- chart: `add_chart` mints the `Chart`, `combine` overlays a second type binding the overlaid series to `set_y2_axis`/`set_x2_axis` via the series `'y2_axis': True` option, and the chart emits after the data rows it plots. Data tables, connector lines, and blank/`#N/A` policy are options rows, never a chart subclass.
- lifecycle: `close` is the single serialization trigger — it flushes pending rows, packages worksheets, shared strings, styles, charts, and metadata into the zip container, and writes the `.xlsx`/`.xlsm` once; the workbook is invalid after `close`.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): the conditional-format/data-validation options and each column's number-format derive from the same column `Struct` that drives the writes, never hand-built twice.
- `pydantic`(`.api/pydantic.md`): a column schema admits the frame through a `TypeAdapter` before the write path lands typed cells.
- `polars`(`.api/polars.md`): a frame streams through `write_row` under `constant_memory` sharing one `add_format` number/date style across the column block, its dtype scalars registered once through `add_write_handler`.
- `segno`(`.api/segno.md`) / `vl-convert-python`(`.api/vl-convert-python.md`): a QR or chart PNG anchors through `insert_image`/`embed_image`.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): one span stamps sheet count, row/column extent, `constant_memory` flag, and output byte length onto the spreadsheet receipt.
- `openpyxl`(`.api/openpyxl.md`): openpyxl authors fidelity-preserving workbooks and defers to xlsxwriter for read-free constant-memory bulk exceeding its `write_only` budget; a `BytesIO` passed as `filename` returns the in-memory buffer the artifacts download owner serves.

[LOCAL_ADMISSION]:
- xlsxwriter owns write-only XLSX/XLSM generation — large datasets under `constant_memory`, custom value-type cells, print-ready reports via the page-setup family, and charts plotting written ranges — while reading or editing an existing workbook routes to the ingest owners. Each workbook seals a receipt of target path/stream, sheet count, row/column extent, streaming mode, zip64 flag, and output byte length.

[RAIL_LAW]:
- Package: `xlsxwriter`
- Owns: constant-memory streaming XLSX/XLSM authoring — `Workbook` lifecycle, `Worksheet` row-major writes and structured features, the `add_write_handler` type-dispatch hook, page-setup layout, embedded charts/images/textboxes with secondary axes, `Format` style minting, and OOXML/zip64 packaging at `close`
- Accept: write-only generation feeding the artifacts spreadsheet, download, and export owners, custom value-type cells via `add_write_handler`, and macro embedding via `add_vba_project`/`add_signed_vba_project`
- Reject: a wrapper-rename of `Workbook`/`add_worksheet`/`write`; a hand-rolled OOXML or zip packager; an in-RAM row matrix where `constant_memory` streams; an `isinstance` ladder or `str()` coercion where `add_write_handler` registers the type; a parallel writer type per cell value or feature; `add_table` under `constant_memory`; reading an existing workbook
