# [PY_ARTIFACTS_API_XLSXWRITER]

`xlsxwriter` supplies the constant-memory streaming XLSX writer for the artifacts spreadsheet rail: a single `Workbook` root that opens a file or stream, mints `Worksheet` and `Format` objects, and serializes Office Open XML rows directly to a zip container at `close`. The package owner composes `Workbook(constant_memory=True)`, `Worksheet.write_row`, and `Workbook.close` into the `XLSX_STREAMING_ROW` path; it removes any in-RAM row matrix because `constant_memory` flushes each completed row to a temp file and holds only the active row, and it never re-implements the OOXML/zip64 packaging xlsxwriter already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xlsxwriter`
- package: `xlsxwriter` (PyPI distribution `XlsxWriter`)
- import: `xlsxwriter`
- owner: `artifacts`
- rail: spreadsheet
- installed: `3.2.9` resolved in `uv.lock` (`py3-none-any` pure-Python wheel; no native ABI, interpreter-agnostic)
- license: `BSD-2-Clause`
- entry points: console script `vba_extract.py` (extracts a `vbaProject.bin` for `Workbook.add_vba_project`); library use is import-only via the single top-level `Workbook` class
- capability: constant-memory streaming XLSX authoring — `Workbook` opens a path or file-like stream, `add_worksheet`/`add_format`/`add_chart`/`add_chartsheet` mint sheets, styles, and charts, `Worksheet.write*` plus the `add_write_handler` type-extension hook and the conditional-format/data-validation/autofilter/table/sparkline/image/in-cell-image/checkbox/page-setup family emit content row-major, and `close` packages worksheets, shared strings, styles, charts, and metadata into a zip64-capable `.xlsx` with O(1) row memory under `constant_memory`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook root and minted objects
- rail: spreadsheet

`Workbook` is the sole top-level export and the only object constructed directly; `Worksheet`, `Chartsheet`, `Format`, and `Chart` are minted by `add_worksheet`/`add_chartsheet`/`add_format`/`add_chart` and never instantiated by the consumer. The `exceptions` module roots every input/file failure under `XlsxWriterException`, branching into `XlsxInputError` (data/name/range) and `XlsxFileError` (zip/image/theme).

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :------------------------------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `xlsxwriter.Workbook`                       | writer root   | open/own the XLSX container and mint sheets, formats, charts |
|  [02]   | `xlsxwriter.worksheet.Worksheet`            | sheet writer  | row-major cell emission plus tables/validation/charts        |
|  [03]   | `xlsxwriter.chartsheet.Chartsheet`          | chart sheet   | a sheet holding a single full-page chart                     |
|  [04]   | `xlsxwriter.format.Format`                  | cell style    | reusable number/font/fill/border/align style object          |
|  [05]   | `xlsxwriter.chart.Chart`                    | chart builder | `add_chart`-minted chart; `add_series`/`set_*_axis`/`combine` |
|  [06]   | `xlsxwriter.exceptions.XlsxWriterException` | error root    | base of every xlsxwriter failure                             |
|  [07]   | `xlsxwriter.exceptions.XlsxInputError`      | error branch  | `DuplicateWorksheetName`/`InvalidWorksheetName`/`DuplicateTableName`/`OverlappingRange`/`EmptyChartSeries` |
|  [08]   | `xlsxwriter.exceptions.XlsxFileError`       | error branch  | `FileCreateError`/`FileSizeError`/`UndefinedImageSize`/`UnsupportedImageFormat`/`ThemeFileError` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbook` lifecycle and minting
- rail: spreadsheet

The constructor `options` dict carries the streaming and coercion policy: `constant_memory=True` flushes each completed row to `tmpdir` and holds only the active row; `in_memory=True` keeps the temp file in RAM when a disk path is unavailable; `tmpdir` redirects the spill directory; `use_zip64=True` lifts the 4 GiB archive ceiling; `strings_to_numbers`/`strings_to_formulas`/`strings_to_urls` govern `write` auto-coercion; `default_date_format` sets the implicit datetime format; `remove_timezone` strips tz from datetimes; `nan_inf_to_errors` maps NaN/Inf to Excel error codes; `max_url_length` bounds hyperlink length; `date_1904` selects the Mac epoch. `add_worksheet`/`add_chartsheet`/`add_format`/`add_chart` mint the objects consumed by the write path, and `close` is the single serialization trigger.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                                                          | [CAPABILITY]                                                                                                 |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `Workbook.__init__`       | `Workbook(filename: str \| IO[AnyStr] \| os.PathLike \| None = None, options: Dict[str, Any] \| None = None) -> None` | open a workbook on a path or stream; `options` carries `constant_memory`, `in_memory`, `tmpdir`, `use_zip64` |
|  [02]   | `Workbook.add_worksheet`  | `add_worksheet(name: str \| None = None, worksheet_class=None) -> Worksheet`                                          | mint a data sheet (auto-named when `name` omitted)                                                           |
|  [03]   | `Workbook.add_chartsheet` | `add_chartsheet(name: str \| None = None, chartsheet_class=None) -> Chartsheet`                                       | mint a single-chart full-page sheet                                                                          |
|  [04]   | `Workbook.add_format`     | `add_format(properties: Dict[str, Any] \| None = None) -> Format`                                                    | mint a reusable cell-style object                                                                            |
|  [05]   | `Workbook.add_chart`      | `add_chart(options: Dict[str, Any]) -> Chart \| None`                                                                | mint a chart by `{'type','subtype'}`; insert via `Worksheet.insert_chart`                                    |
|  [06]   | `Workbook.define_name`    | `define_name(name: str, formula: str) -> Literal[0, -1]`                                                             | register a workbook- or sheet-scoped defined name                                                            |
|  [07]   | `Workbook.add_vba_project`| `add_vba_project(vba_project: str \| BinaryIO, is_stream: bool = False) -> Literal[0, -1]`                           | embed an extracted `vbaProject.bin` for a macro-enabled `.xlsm`                                              |
|  [08]   | `Workbook.set_properties` | `set_properties(properties: Dict[str, Any]) -> None`                                                                 | set document core properties (title/author/subject/keywords/created)                                         |
|  [09]   | `Workbook.set_custom_property` | `set_custom_property(name: str, value: bool \| datetime \| int \| float \| Decimal \| Fraction \| Any, property_type: Literal['bool','date','number','number_int','text'] \| None = None) -> Literal[0, -1]` | add one typed custom document property                                                |
|  [10]   | `Workbook.get_worksheet_by_name` | `get_worksheet_by_name(name: str) -> Worksheet \| None`                                                        | polymorphic lookup of a minted sheet by name                                                                 |
|  [11]   | `Workbook.worksheets`     | `worksheets() -> List[Worksheet]`                                                                                    | enumerate minted sheets in tab order                                                                         |
|  [12]   | `Workbook.set_calc_mode`  | `set_calc_mode(mode: Literal['auto','manual','auto_except_tables'], calc_id=None) -> None`                           | recalc policy                                                                                                |
|  [13]   | `Workbook.read_only_recommended` | `read_only_recommended() -> None`                                                                              | flag the file read-only-recommended on open                                                                  |
|  [14]   | `Workbook.add_signed_vba_project` | `add_signed_vba_project(vba_project, signature, project_is_stream=False, signature_is_stream=False) -> Literal[0, -1]` | embed a digitally-signed `vbaProject.bin` + signature for a trusted macro `.xlsm`                  |
|  [15]   | `Workbook.set_size` / `set_tab_ratio` | `set_size(width, height) -> None` / `set_tab_ratio(tab_ratio: float) -> None`                              | workbook window pixel size / sheet-tab-to-scrollbar ratio                                                     |
|  [16]   | `Workbook.use_zip64`      | `use_zip64() -> None`                                                                                                | enable zip64 at runtime (the method form of the `use_zip64` option, lifting the 4 GiB ceiling)               |
|  [17]   | `Workbook.get_default_url_format` | `get_default_url_format() -> Format`                                                                          | the implicit hyperlink `Format` `write_url` applies — reuse it to extend link styling                        |
|  [18]   | `Workbook.close`          | `close() -> None`                                                                                                    | flush, package, and write the `.xlsx`/`.xlsm` container                                                      |

[ENTRYPOINT_SCOPE]: `Worksheet` row-major writes
- rail: spreadsheet

Under `constant_memory` cells are written in strict row-major order; a row is sealed when the next-higher row is touched. `write` discriminates the value type and dispatches to the typed writers; `write_row`/`write_column` accept a sequence and an optional shared `cell_format`.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                                                           | [CAPABILITY]                                                                        |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Worksheet.write`          | `write(row: int, col: int, *args) -> Literal[0, -1] \| Any`                                                                                                            | type-dispatching cell write (str/number/bool/datetime/url)                          |
|  [02]   | `Worksheet.write_row`      | `write_row(row: int, col: int, data, cell_format: Format \| None = None) -> Literal[0] \| Any`                                                                         | write a sequence across one row                                                     |
|  [03]   | `Worksheet.write_column`   | `write_column(row: int, col: int, data, cell_format: Format \| None = None) -> Literal[0] \| Any`                                                                      | write a sequence down one column                                                    |
|  [04]   | `Worksheet.write_string`   | `write_string(row: int, col: int, string: str, cell_format: Format \| None = None) -> Literal[0, -1, -2]`                                                              | write a string cell                                                                 |
|  [05]   | `Worksheet.write_number`   | `write_number(row: int, col: int, number: int \| float \| Fraction, cell_format: Format \| None = None) -> Literal[0, -1]`                                             | write a numeric cell                                                                |
|  [06]   | `Worksheet.write_boolean`  | `write_boolean(row: int, col: int, boolean: bool, cell_format: Format \| None = None)`                                                                                 | write a boolean cell                                                                |
|  [07]   | `Worksheet.write_datetime` | `write_datetime(row: int, col: int, date: datetime, cell_format: Format \| None = None) -> Literal[0, -1]`                                                             | write a date/time cell (number-formatted)                                           |
|  [08]   | `Worksheet.write_formula`  | `write_formula(row: int, col: int, formula: str, cell_format: Format \| None = None, value=0) -> Literal[0, -1, -2]`                                                   | write a formula with cached value                                                   |
|  [09]   | `Worksheet.write_blank`    | `write_blank(row: int, col: int, blank: Any, cell_format: Format \| None = None)`                                                                                      | write a formatted empty cell                                                        |
|  [10]   | `Worksheet.write_url`      | `write_url(row: int, col: int, url: str, cell_format: Format \| None = None, string: str \| None = None, tip: str \| None = None) -> Literal[0, -1, -2, -3]`            | write a hyperlink (external/internal/mailto)                                        |
|  [11]   | `Worksheet.write_rich_string` | `write_rich_string(row: int, col: int, *args) -> Literal[0, -1, -2, -3]`                                                                                            | write a multi-format string (alternating `Format`/text segments)                   |
|  [12]   | `Worksheet.write_array_formula` | `write_array_formula(first_row, first_col, last_row, last_col, formula: str, cell_format=None, value=0) -> Literal[0, -1]`                                          | write a CSE array formula over a range                                             |
|  [13]   | `Worksheet.write_dynamic_array_formula` | `write_dynamic_array_formula(first_row, first_col, last_row, last_col, formula: str, cell_format=None, value=0) -> Literal[0, -1]`                          | write an Excel 365 spilling dynamic-array formula                                  |
|  [14]   | `Worksheet.set_column`     | `set_column(first_col: int, last_col: int, width: float \| None = None, cell_format: Format \| None = None, options: Dict[str, Any] \| None = None) -> Literal[0, -1]` | set column width/default format/visibility                                          |
|  [15]   | `Worksheet.set_row`        | `set_row(row: int, height: float \| None = None, cell_format: Format \| None = None, options: Dict[str, Any] \| None = None) -> Literal[0, -1]`                        | set row height/default format (call before writing the row under `constant_memory`) |
|  [16]   | `Worksheet.freeze_panes`   | `freeze_panes(row: int, col: int, top_row: int \| None = None, left_col: int \| None = None, pane_type: int = 0) -> None`                                              | freeze header rows/columns                                                          |

[ENTRYPOINT_SCOPE]: `Worksheet` structured-feature family
- rail: spreadsheet

These are the higher-level Excel features layered over cells. `add_table`, `add_sparkline`, `data_validation`, and `conditional_format` accept an `options` dict carrying the full feature parameters. Under `constant_memory` these remain available except `add_table`, which the streaming flush cannot back-patch.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Worksheet.conditional_format` | `conditional_format(first_row, first_col, last_row, last_col, options: Dict[str, Any]) -> Literal[0, -1, -2]` | cell/3-color-scale/data-bar/icon-set/formula conditional styling |
|  [02]   | `Worksheet.data_validation`    | `data_validation(first_row, first_col, last_row, last_col, options: Dict[str, Any]) -> Literal[0, -1, -2]`    | dropdown lists, ranges, input/error messages                     |
|  [03]   | `Worksheet.add_table`          | `add_table(first_row, first_col, last_row, last_col, options: Dict[str, Any] \| None = None) -> Literal[0, -1, -2, -3]` | banded structured table with header/totals (no `constant_memory`) |
|  [04]   | `Worksheet.add_sparkline`      | `add_sparkline(row, col, options: Dict[str, Any]) -> Literal[0, -1, -2]`                                  | inline line/column/win-loss sparkline                                |
|  [05]   | `Worksheet.autofilter`         | `autofilter(first_row, first_col, last_row, last_col) -> None`                                           | declare a filterable header range                                    |
|  [06]   | `Worksheet.filter_column`      | `filter_column(col, criteria: str) -> None`                                                              | apply a filter expression to one autofilter column                   |
|  [07]   | `Worksheet.merge_range`        | `merge_range(first_row, first_col, last_row, last_col, data: Any, cell_format: Format \| None = None) -> int` | merge a cell range with one formatted value (delegates to the typed `write`, so it returns that writer's code) |
|  [08]   | `Worksheet.insert_chart`       | `insert_chart(row, col, chart: Chart, options: Dict[str, Any] \| None = None) -> Literal[0, -1]`         | embed an `add_chart` object at an anchor                             |
|  [09]   | `Worksheet.insert_image`       | `insert_image(row, col, source: str \| BytesIO, options: Dict[str, Any] \| None = None) -> Literal[0, -1, -2]` | embed a PNG/JPEG/GIF/BMP from path or in-memory stream         |
|  [10]   | `Worksheet.insert_textbox`     | `insert_textbox(row, col, text: str, options: Dict[str, Any] \| None = None) -> Literal[0, -1]`          | place a floating text box                                            |
|  [11]   | `Worksheet.write_comment`      | `write_comment(row, col, comment: str, options: Dict[str, Any] \| None = None) -> Literal[0, -1, -2]`    | attach a cell comment/note                                           |
|  [12]   | `Worksheet.protect`            | `protect(password: str = "", options: Dict[str, Any] \| None = None) -> None`                            | sheet protection with feature allowances                             |
|  [13]   | `Worksheet.add_write_handler`  | `add_write_handler(user_type: type, user_function: Callable[[Worksheet, int, int, Any, Format \| None], Any]) -> None` | register a type->writer so `write` natively dispatches a custom value type (e.g. a `numpy.datetime64`, `polars`/`pandas` scalar, `decimal.Decimal`); THE polymorphic write-extension hook — no per-type call site |
|  [14]   | `Worksheet.embed_image`        | `embed_image(row, col, source: str \| BytesIO \| Image, options: Dict[str, Any] \| None = None) -> Literal[0, -1]` | embed an image IN a cell (Excel-365 `IMAGE()` cell, scales with the cell) vs floating `insert_image` |
|  [15]   | `Worksheet.insert_checkbox`    | `insert_checkbox(row, col, boolean: bool, cell_format: Format \| None = None)`                           | a boolean checkbox cell (Excel-365)                                  |
|  [16]   | `Worksheet.filter_column_list` | `filter_column_list(col, filters: List[str]) -> None`                                                    | multi-value ("in" set) autofilter, the list mirror of `filter_column` |
|  [17]   | `Worksheet.ignore_errors`      | `ignore_errors(options: Dict[str, Any] \| None = None) -> Literal[0, -1]`                                | suppress the green-triangle error markers over a range (e.g. number-as-text) |
|  [18]   | `Worksheet.autofit`            | `autofit(max_width: int = 1790) -> None`                                                                 | size every column to its widest written cell at `close` (post-write only) |

[ENTRYPOINT_SCOPE]: `Worksheet` page setup and print layout
- rail: spreadsheet

A printable report artifact (the export-bundle target) sets orientation, paper, fit-to-pages, margins, header/footer, print area, and repeated title rows on the sheet. These are sheet-state setters, available under `constant_memory` (they touch sheet metadata, not back-patched cell XML), never a parallel "page" object.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Worksheet.set_landscape` / `set_portrait` | `set_landscape() -> None` / `set_portrait() -> None`                      | page orientation                                                     |
|  [02]   | `Worksheet.set_paper`      | `set_paper(paper_size: int) -> None`                                                     | paper size code (`9`=A4, `1`=Letter, ...)                            |
|  [03]   | `Worksheet.fit_to_pages`   | `fit_to_pages(width: int, height: int) -> None`                                          | scale the print to W×H pages (`fit_to_pages(1, 0)` = one page wide)  |
|  [04]   | `Worksheet.set_margins`    | `set_margins(left=0.7, right=0.7, top=0.75, bottom=0.75) -> None`                         | print margins in inches                                              |
|  [05]   | `Worksheet.set_header` / `set_footer` | `set_header(header='', options=None, margin=None) -> None`                     | header/footer with `&P`/`&N`/`&D` field codes                       |
|  [06]   | `Worksheet.print_area`     | `print_area(first_row, first_col, last_row, last_col) -> Literal[0, -1]`                 | bound the printable range                                            |
|  [07]   | `Worksheet.repeat_rows` / `repeat_columns` | `repeat_rows(first_row, last_row=None) -> None`                          | repeat title rows/cols on every printed page                        |
|  [08]   | `Worksheet.set_zoom` / `set_tab_color` | `set_zoom(zoom=100) -> None` / `set_tab_color(color) -> None`                | screen zoom / sheet-tab color                                       |
|  [09]   | `Worksheet.set_default_row` | `set_default_row(height=None, hide_unused_rows=False) -> None`                           | default row height; hide trailing empty rows                        |

[ENTRYPOINT_SCOPE]: `Chart` builder
- rail: spreadsheet — `add_chart({'type': ..., 'subtype': ...})` mints the object; `insert_chart` anchors it

`Chart` is minted by `Workbook.add_chart` with `type` in `column`/`bar`/`line`/`area`/`pie`/`doughnut`/`scatter`/`stock`/`radar`. Series reference worksheet ranges by `=Sheet1!$A$1:$A$10` strings; `combine` overlays a second chart type (column+line Pareto). A `Chartsheet` from `add_chartsheet` hosts a single full-page chart via its own `set_chart`.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                | [CAPABILITY]                                              |
| :-----: | :--------------------- | :-------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Chart.add_series`     | `add_series(options: Dict[str, Any] \| None = None) -> None`                | one data series (`values`/`categories`/`name`/`line`/`fill`/`trendline`/`data_labels`/`y2_axis`) |
|  [02]   | `Chart.set_x_axis` / `set_y_axis` | `set_x_axis(options: Dict[str, Any]) -> None`                    | primary axis name/range/number format/gridlines/log      |
|  [03]   | `Chart.set_x2_axis` / `set_y2_axis` | `set_y2_axis(options: Dict[str, Any]) -> None`                  | the secondary axes a `combine`d series (`'y2_axis': True`) binds to (Pareto) |
|  [04]   | `Chart.set_title`      | `set_title(options: Dict[str, Any] \| None = None) -> None`                 | chart title and font                                     |
|  [05]   | `Chart.set_legend`     | `set_legend(options: Dict[str, Any]) -> None`                               | legend position/visibility/font                          |
|  [06]   | `Chart.set_style`      | `set_style(style_id: int = 2) -> None`                                      | apply one of the 48 built-in Excel chart styles          |
|  [07]   | `Chart.set_size`       | `set_size(options: Dict[str, Any] \| None = None) -> None`                  | chart pixel dimensions and scaling                       |
|  [08]   | `Chart.set_plotarea` / `set_chartarea` | `set_plotarea(options: Dict[str, Any]) -> None`             | plot-area / chart-area fill, border, gradient            |
|  [09]   | `Chart.set_table`      | `set_table(options: Dict[str, Any] \| None = None) -> None`                 | render a data table beneath the plot                     |
|  [10]   | `Chart.combine`        | `combine(chart: Chart \| None = None) -> None`                              | overlay a second chart type on a shared (or `y2`) axis    |
|  [11]   | `Chart.set_high_low_lines` / `set_drop_lines` / `set_up_down_bars` | `set_high_low_lines(options=None) -> None` | stock/line connectors and up-down bars                    |
|  [12]   | `Chart.show_blanks_as` / `show_na_as_empty_cell` / `show_hidden_data` | `show_blanks_as(option: str) -> None` | gap/zero/connect policy for blank & `#N/A` cells          |

[ENTRYPOINT_SCOPE]: `Format` style minting
- rail: spreadsheet

`Format` is constructed by `add_format`; its `set_*` family configures number format, font, alignment, fill, border, rotation, indent, and protection. Equivalent properties pass as the `add_format({...})` dict in one call. The style object is shared by reference across cells and sheets to keep the format table small.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                         | [CAPABILITY]                      |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `Format.set_num_format` | `set_num_format(num_format: str) -> None`                                            | apply an Excel number-format code |
|  [02]   | `Format.set_bold`       | `set_bold(bold: bool = True) -> None`                                                | toggle bold font                  |
|  [03]   | `Format.set_align`      | `set_align(alignment: Literal['left','center','right','top','vcenter',...]) -> None` | horizontal/vertical alignment     |
|  [04]   | `Format.set_bg_color`   | `set_bg_color(bg_color: str \| Color) -> None`                                       | set cell fill color               |
|  [05]   | `Format.set_border`     | `set_border(style: int = 1) -> None`                                                 | set the cell border style         |

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
