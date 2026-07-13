# [PY_ARTIFACTS_API_XLSXWRITER]

`xlsxwriter` supplies the constant-memory streaming XLSX writer for the artifacts spreadsheet rail: a single `Workbook` root that opens a file or stream, mints `Worksheet` and `Format` objects, and serializes Office Open XML rows directly to a zip container at `close`. The package owner composes `Workbook(constant_memory=True)`, `Worksheet.write_row`, and `Workbook.close` into the `XLSX_STREAMING_ROW` path; it removes any in-RAM row matrix because `constant_memory` flushes each completed row to a temp file and holds only the active row, and it never re-implements the OOXML/zip64 packaging xlsxwriter already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xlsxwriter`
- package: `xlsxwriter` (PyPI distribution `XlsxWriter`)
- import: `xlsxwriter`
- owner: `artifacts`
- rail: spreadsheet
- installed: `3.2.9`
- license: `BSD-2-Clause`
- entry points: console script `vba_extract.py` (extracts a `vbaProject.bin` for `Workbook.add_vba_project`); library use is import-only via the single top-level `Workbook` class
- capability: constant-memory streaming XLSX authoring — `Workbook` opens a path or file-like stream, `add_worksheet`/`add_format`/`add_chart`/`add_chartsheet` mint sheets, styles, and charts, `Worksheet.write*` plus the `add_write_handler` type-extension hook and the conditional-format/data-validation/autofilter/table/sparkline/image/in-cell-image/checkbox/page-setup family emit content row-major, and `close` packages worksheets, shared strings, styles, charts, and metadata into a zip64-capable `.xlsx` with O(1) row memory under `constant_memory`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook root and minted objects
- rail: spreadsheet

`Workbook` is the sole top-level export and the only object constructed directly; `Worksheet`, `Chartsheet`, `Format`, and `Chart` are minted by `add_worksheet`/`add_chartsheet`/`add_format`/`add_chart` and never instantiated by the consumer. The `exceptions` module roots every input/file failure under `XlsxWriterException`, branching into `XlsxInputError` (data/name/range) and `XlsxFileError` (zip/image/theme).

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------------ |
|  [01]   | `xlsxwriter.Workbook`                       | writer root   | open/own the XLSX container and mint sheets, formats, charts  |
|  [02]   | `xlsxwriter.worksheet.Worksheet`            | sheet writer  | row-major cell emission plus tables/validation/charts         |
|  [03]   | `xlsxwriter.chartsheet.Chartsheet`          | chart sheet   | a sheet holding a single full-page chart                      |
|  [04]   | `xlsxwriter.format.Format`                  | cell style    | reusable number/font/fill/border/align style object           |
|  [05]   | `xlsxwriter.chart.Chart`                    | chart builder | `add_chart`-minted chart; `add_series`/`set_*_axis`/`combine` |
|  [06]   | `xlsxwriter.exceptions.XlsxWriterException` | error root    | base of every xlsxwriter failure                              |
|  [07]   | `xlsxwriter.exceptions.XlsxInputError`      | error branch  | data/name/range faults                                        |
|  [08]   | `xlsxwriter.exceptions.XlsxFileError`       | error branch  | zip/image/theme faults                                        |

- [07]-[XLSX_INPUT_ERROR]: `DuplicateWorksheetName`/`InvalidWorksheetName`/`DuplicateTableName`/`OverlappingRange`/`EmptyChartSeries`.
- [08]-[XLSX_FILE_ERROR]: `FileCreateError`/`FileSizeError`/`UndefinedImageSize`/`UnsupportedImageFormat`/`ThemeFileError`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbook` lifecycle and minting
- rail: spreadsheet

The constructor `options` dict carries the streaming and coercion policy: `constant_memory=True` flushes each completed row to `tmpdir` and holds only the active row; `in_memory=True` keeps the temp file in RAM when a disk path is unavailable; `tmpdir` redirects the spill directory; `use_zip64=True` lifts the 4 GiB archive ceiling; `strings_to_numbers`/`strings_to_formulas`/`strings_to_urls` govern `write` auto-coercion; `default_date_format` sets the implicit datetime format; `remove_timezone` strips tz from datetimes; `nan_inf_to_errors` maps NaN/Inf to Excel error codes; `max_url_length` bounds hyperlink length; `date_1904` selects the Mac epoch. `add_worksheet`/`add_chartsheet`/`add_format`/`add_chart` mint the objects consumed by the write path, and `close` is the single serialization trigger. The `[SURFACE]` cell carries the full call over the `Workbook` scope: `filename` is `str \| IO[AnyStr] \| os.PathLike \| None`, a mutator returns `Literal[0, -1]` (0 applied, -1 rejected) and a void setter returns `None`; `set_custom_property` value is `bool \| datetime \| int \| float \| Decimal \| Fraction \| Any` typed by `property_type` in `'bool'/'date'/'number'/'number_int'/'text'`, `set_calc_mode` mode is `'auto'/'manual'/'auto_except_tables'`, and `add_signed_vba_project` carries `project_is_stream=False, signature_is_stream=False`.

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `Workbook(filename=None, options=None)`                          | open a workbook on a path or stream                               |
|  [02]   | `add_worksheet(name=None, worksheet_class=None) -> Worksheet`    | mint a data sheet (auto-named when `name` omitted)                |
|  [03]   | `add_chartsheet(name=None, chartsheet_class=None) -> Chartsheet` | mint a single-chart full-page sheet                               |
|  [04]   | `add_format(properties=None) -> Format`                          | mint a reusable cell-style object                                 |
|  [05]   | `add_chart(options) -> Chart \| None`                            | mint a chart by `{'type','subtype'}`; insert via `insert_chart`   |
|  [06]   | `define_name(name, formula)`                                     | register a workbook- or sheet-scoped defined name                 |
|  [07]   | `add_vba_project(vba_project, is_stream=False)`                  | embed an extracted `vbaProject.bin` for a macro `.xlsm`           |
|  [08]   | `set_properties(properties)`                                     | set core document properties (title/author/subject/keywords)      |
|  [09]   | `set_custom_property(name, value, property_type=None)`           | add one typed custom document property                            |
|  [10]   | `get_worksheet_by_name(name) -> Worksheet \| None`               | polymorphic lookup of a minted sheet by name                      |
|  [11]   | `worksheets() -> List[Worksheet]`                                | enumerate minted sheets in tab order                              |
|  [12]   | `set_calc_mode(mode, calc_id=None)`                              | recalc policy                                                     |
|  [13]   | `read_only_recommended()`                                        | flag the file read-only on open                                   |
|  [14]   | `add_signed_vba_project(vba_project, signature, …)`              | embed a signed `vbaProject.bin` + signature for a trusted `.xlsm` |
|  [15]   | `set_size(width, height)` / `set_tab_ratio(tab_ratio)`           | window pixel size / sheet-tab-to-scrollbar ratio                  |
|  [16]   | `use_zip64()`                                                    | enable zip64 at runtime, lifting the 4 GiB ceiling                |
|  [17]   | `get_default_url_format() -> Format`                             | the implicit hyperlink `Format` `write_url` applies               |
|  [18]   | `close()`                                                        | flush, package, and write the `.xlsx`/`.xlsm` container           |

[ENTRYPOINT_SCOPE]: `Worksheet` row-major writes
- rail: spreadsheet

Under `constant_memory` cells are written in strict row-major order; a row is sealed when the next-higher row is touched, and `set_row` height/format applies before the row's cells. `write` discriminates the value type and dispatches to the typed writers; `write_row`/`write_column` accept a sequence and an optional shared `cell_format`. The `[SURFACE]` cell carries the full call over the `Worksheet` scope: a positional cell is `(row, col, …)`, `<range>` abbreviates the block `(first_row, first_col, last_row, last_col)`, `cell_format` is `Format \| None = None`, and the typed writers return a `Literal[0, -1, …]` status code.

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
- rail: spreadsheet

These are the higher-level Excel features layered over cells. `add_table`, `add_sparkline`, `data_validation`, and `conditional_format` accept an `options` dict carrying the full feature parameters. Under `constant_memory` these remain available except `add_table`, which the streaming flush cannot back-patch. The `[SURFACE]` cell drops the `Worksheet.` scope; `options` is `Dict[str, Any] \| None = None`, `<range>` opens the block `(first_row, first_col, last_row, last_col)`, and each surface returns a `Literal[0, -1, …]` code.

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

- [13]-[WRITE_HANDLER]: `user_function` is `Callable[[Worksheet, int, int, Any, Format \| None], Any]` — the `(worksheet, row, col, value, cell_format)` shape `write` invokes to natively dispatch a custom value type (a `numpy.datetime64`, a `polars`/`pandas` scalar, a `decimal.Decimal`), never a per-type call site.

[ENTRYPOINT_SCOPE]: `Worksheet` page setup and print layout
- rail: spreadsheet

A printable report artifact (the export-bundle target) sets orientation, paper, fit-to-pages, margins, header/footer, print area, and repeated title rows on the sheet. These are sheet-state setters, available under `constant_memory` (they touch sheet metadata, not back-patched cell XML), never a parallel "page" object. The `[SURFACE]` cell drops the `Worksheet.` scope; a setter returns `None` and `print_area` returns `Literal[0, -1]`.

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
- rail: spreadsheet — `add_chart({'type': ..., 'subtype': ...})` mints the object; `insert_chart` anchors it

`Chart` is minted by `Workbook.add_chart` with `type` in `column`/`bar`/`line`/`area`/`pie`/`doughnut`/`scatter`/`stock`/`radar`. Series reference worksheet ranges by `=Sheet1!$A$1:$A$10` strings; `combine` overlays a second chart type (column+line Pareto). A `Chartsheet` from `add_chartsheet` hosts a single full-page chart via its own `set_chart`. The `[SURFACE]` cell drops the `Chart.` scope; `options` is a `Dict[str, Any]` and setters return `None`; the connector setters take `options=None` and the blank-policy setters a single `option` string.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `add_series(options=None)`                                    | one data series bound to worksheet ranges              |
|  [02]   | `set_x_axis(options)` / `set_y_axis(options)`                 | primary axis name/range/number format/gridlines/log    |
|  [03]   | `set_x2_axis(options)` / `set_y2_axis(options)`               | secondary axes a `combine`d series binds to (Pareto)   |
|  [04]   | `set_title(options=None)`                                     | chart title and font                                   |
|  [05]   | `set_legend(options)`                                         | legend position/visibility/font                        |
|  [06]   | `set_style(style_id=2)`                                       | apply one of the 48 built-in Excel chart styles        |
|  [07]   | `set_size(options=None)`                                      | chart pixel dimensions and scaling                     |
|  [08]   | `set_plotarea(options)` / `set_chartarea(options)`            | plot-area / chart-area fill, border, gradient          |
|  [09]   | `set_table(options=None)`                                     | render a data table beneath the plot                   |
|  [10]   | `combine(chart=None)`                                         | overlay a second chart type on a shared (or `y2`) axis |
|  [11]   | `set_high_low_lines`, `set_drop_lines`, `set_up_down_bars`    | stock/line connectors and up-down bars                 |
|  [12]   | `show_blanks_as`, `show_na_as_empty_cell`, `show_hidden_data` | gap/zero/connect policy for blank & `#N/A` cells       |

- [01]-[SERIES]: `add_series` options carry `values`/`categories`/`name`/`line`/`fill`/`trendline`/`data_labels`/`y2_axis`.

[ENTRYPOINT_SCOPE]: `Format` style minting
- rail: spreadsheet

`Format` is constructed by `add_format`; its `set_*` family configures number format, font, alignment, fill, border, rotation, indent, and protection. Equivalent properties pass as the `add_format({...})` dict in one call. The style object is shared by reference across cells and sheets to keep the format table small. The `[SURFACE]` cell drops the `Format.` scope; `set_align` alignment is a `Literal['left','center','right','top','vcenter', …]` and every setter returns `None`.

| [INDEX] | [SURFACE]                    | [CAPABILITY]                      |
| :-----: | :--------------------------- | :-------------------------------- |
|  [01]   | `set_num_format(num_format)` | apply an Excel number-format code |
|  [02]   | `set_bold(bold=True)`        | toggle bold font                  |
|  [03]   | `set_align(alignment)`       | horizontal/vertical alignment     |
|  [04]   | `set_bg_color(bg_color)`     | set cell fill color               |
|  [05]   | `set_border(style=1)`        | set the cell border style         |

## [04]-[IMPLEMENTATION_LAW]

[SPREADSHEET_STREAMING_ROW]:
- import: `import xlsxwriter` at boundary scope only; module-level import is banned by the manifest import policy. Construct via `xlsxwriter.Workbook(target, {"constant_memory": True})`.
- memory axis: `constant_memory=True` is the streaming row of the `options` dict — it flushes each completed row to a `tmpdir` temp file and holds only the active row, giving O(1) row memory; `in_memory=True` keeps the temp data in RAM when no writable disk path exists; `use_zip64=True` lifts the 4 GiB archive ceiling. Streaming is a policy row, never a parallel writer type.
- order axis: under `constant_memory` cells are written in strict row-major order and a row is sealed when a higher row is touched; `set_row` height/format is applied before the row's cells are written. Out-of-order writes to a sealed row are silently dropped — the row index is the load-bearing discriminant.
- write axis: `Worksheet.write` is the single type-dispatching entry; it discriminates str/number/bool/datetime/url and routes to the typed writers. `write_row`/`write_column` are the bulk-sequence rows with one shared `cell_format`; typed `write_string`/`write_number`/`write_datetime` are forced-type rows, never a per-type sheet object. `add_write_handler(user_type, fn)` is the polymorphic extension seam: register a domain value type (a `numpy.datetime64`/`numpy.float64`, a `polars`/`pandas` cell, a `decimal.Decimal`, a `Length`-style value object) so `write` natively dispatches it — never an `if isinstance(...)` ladder at the call site or a `str(value)` coercion that loses the typed cell.
- style axis: `Format` is minted once by `add_format` and shared by reference across cells and sheets; its `set_*` setters (or the `add_format({...})` property dict) are the style rows. The format object is a reused style table entry, never a per-cell style allocation. `Workbook.get_default_url_format()` returns the implicit hyperlink style so link formatting extends the same shared `Format` rather than re-minting one.
- feature axis: structured Excel features (`conditional_format`, `data_validation`, `add_table`, `add_sparkline`, `autofilter`/`filter_column`/`filter_column_list`, `merge_range`, `ignore_errors`), embedded objects (`insert_chart`, `insert_image`/`embed_image`, `insert_textbox`, `insert_checkbox`, `write_comment`), and the page-setup setters (`set_landscape`/`set_paper`/`fit_to_pages`/`set_margins`/`set_header`/`set_footer`/`print_area`/`repeat_rows`) are options-dict-or-scalar rows on `Worksheet`, never parallel sheet types. `embed_image` writes an in-cell Excel-365 image that scales with the cell where `insert_image` floats over the grid. `constant_memory` permits all of these except `add_table`, whose streaming flush cannot back-patch the table XML — guard a table emission on the non-streaming path; `autofit` is likewise a post-write pass that needs the streamed widths, so call it before `close` on the materialized sheet.
- chart axis: `Workbook.add_chart({'type','subtype'})` mints a `Chart`; series bind worksheet ranges by A1 string, `combine` overlays a second type, and `Worksheet.insert_chart` (or `Chartsheet.set_chart`) anchors it. A Pareto/dual-scale chart binds the overlaid series to `set_y2_axis`/`set_x2_axis` via the series `'y2_axis': True` option; data tables, drop/high-low lines, and `show_blanks_as`/`show_na_as_empty_cell` policy are options rows, never a chart subclass. The chart references already-written cells, so it is emitted after the data rows it plots.
- lifecycle axis: `Workbook.close` is the single serialization trigger — it flushes pending rows, packages worksheets, chartsheets, shared strings, styles, charts, and metadata into the zip container, and writes the `.xlsx`/`.xlsm`. The workbook is invalid after `close`; serialization happens exactly once.
- integration: stream a `polars`/`pyarrow` frame through `write_row` under `constant_memory`, sharing one `add_format` number/date style across the column block; register the frame's dtype scalars (`numpy.datetime64`, `Decimal`, an `enum`/value object) once through `add_write_handler` so `write` lands typed cells with no per-row coercion. For in-memory delivery pass a `BytesIO` as `filename` so `close` writes the buffer the artifacts download owner returns, and stamp an `opentelemetry` span (`libs/python/.api/opentelemetry-api.md`) with sheet count, row/column extent, `constant_memory` flag, and output byte length. The conditional-format/data-validation options dicts and the column number-format derive from the same `msgspec`/`pydantic` column schema that drives the writes (`libs/python/.api/msgspec.md`), never hand-built twice. A printable report sets the page-setup family (`fit_to_pages(1, 0)`, `set_landscape`, `repeat_rows`, `set_header('&P')`) on the sheet before `close` so the same workbook serves screen and print; an embedded `segno` QR or `vl-convert` chart PNG anchors through `insert_image`/`embed_image`.
- evidence: each workbook captures target path/stream, sheet count, written row/column extents, `constant_memory`/`in_memory` mode, zip64 flag, chart/table count, and output byte length as a spreadsheet receipt.
- boundary: xlsxwriter owns XLSX generation, OOXML packaging, and zip container assembly with no read capability; reading or editing existing `.xlsx` routes elsewhere; the streamed file feeds the artifacts download and export owners directly; live UI rendering stays outside this package.

[RAIL_LAW]:
- Package: `xlsxwriter`
- Owns: constant-memory streaming XLSX/XLSM authoring — `Workbook` container lifecycle, `Worksheet` row-major writes plus structured features (conditional format, validation, tables, sparklines, autofilter, merge, comments, in-cell images/checkboxes, error-ignore), the `add_write_handler` type-dispatch hook, page-setup/print layout, embedded charts/images/textboxes with secondary axes, `Format` style minting, and OOXML/zip64 packaging at `close`
- Accept: write-only XLSX generation feeding the artifacts spreadsheet, download, and export owners, including large datasets under `constant_memory`, custom value-type cells via `add_write_handler`, print-ready reports via the page-setup family, charts plotting written ranges, and macro embedding via `add_vba_project`/`add_signed_vba_project`
- Reject: wrapper-renames of `Workbook`/`add_worksheet`/`write`; a hand-rolled OOXML or zip packager; an in-RAM row matrix where `constant_memory` already streams; an `isinstance` ladder or `str()` coercion at the write call site where `add_write_handler` registers the type; a parallel writer type per cell value type or per feature; `add_table` under `constant_memory`; reading or editing an existing workbook this package never opens for input
