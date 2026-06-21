# [PY_ARTIFACTS_API_XLSXWRITER]

`xlsxwriter` supplies the constant-memory streaming XLSX writer for the artifacts spreadsheet rail: a single `Workbook` root that opens a file or stream, mints `Worksheet` and `Format` objects, and serializes Office Open XML rows directly to a zip container at `close`. The package owner composes `Workbook(constant_memory=True)`, `Worksheet.write_row`, and `Workbook.close` into the `XLSX_STREAMING_ROW` path; it removes any in-RAM row matrix because `constant_memory` flushes each completed row to a temp file and holds only the active row, and it never re-implements the OOXML/zip64 packaging xlsxwriter already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xlsxwriter`
- package: `xlsxwriter`
- import: `xlsxwriter`
- owner: `artifacts`
- rail: spreadsheet
- installed: `3.2.9` reflected via assay api on cp315
- entry points: console script `vba_extract.py` (VBA tooling); library use is import-only via the single top-level `Workbook` class
- capability: constant-memory streaming XLSX authoring — `Workbook` opens a path or file-like stream, `add_worksheet`/`add_format` mint sheets and styles, `Worksheet.write*` emits cells row-major, and `close` packages worksheets, shared strings, styles, and metadata into a zip64-capable `.xlsx` with O(1) row memory under `constant_memory`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook root and minted objects
- rail: spreadsheet

`Workbook` is the sole top-level export and the only object constructed directly; `Worksheet` and `Format` are minted by `add_worksheet` and `add_format` and never instantiated by the consumer. The `exceptions` module roots every input/file failure under `XlsxWriterException`, with `DuplicateWorksheetName`/`InvalidWorksheetName` on the input rail and `FileCreateError`/`FileSizeError` on the file rail.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `xlsxwriter.Workbook`                       | writer root   | open/own the XLSX container and mint sheets and formats |
|  [02]   | `xlsxwriter.worksheet.Worksheet`            | sheet writer  | row-major cell emission for one sheet                   |
|  [03]   | `xlsxwriter.format.Format`                  | cell style    | reusable number/font/fill/border style object           |
|  [04]   | `xlsxwriter.exceptions.XlsxWriterException` | error root    | base of every xlsxwriter failure                        |
|  [05]   | `xlsxwriter.exceptions.XlsxInputError`      | error         | invalid-input branch (name/series/range)                |
|  [06]   | `xlsxwriter.exceptions.XlsxFileError`       | error         | file/zip branch (create/size/image)                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbook` lifecycle and minting
- rail: spreadsheet

The constructor `options` dict carries the streaming policy; `constant_memory=True` flushes each completed row to `tmpdir` and holds only the active row, and `in_memory=True` keeps the temp file in RAM when a disk path is unavailable. `add_worksheet`/`add_format` mint the objects consumed by the write path, and `close` is the single serialization trigger.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                                                          | [CAPABILITY]                                                                                                 |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `Workbook.__init__`       | `Workbook(filename: str \| IO[AnyStr] \| os.PathLike \| None = None, options: Dict[str, Any] \| None = None) -> None` | open a workbook on a path or stream; `options` carries `constant_memory`, `in_memory`, `tmpdir`, `use_zip64` |
|  [02]   | `Workbook.add_worksheet`  | `add_worksheet(name: str \| None = None, worksheet_class=None) -> Worksheet`                                          | mint a sheet (auto-named when `name` omitted)                                                                |
|  [03]   | `Workbook.add_format`     | `add_format(properties=None) -> Format`                                                                               | mint a reusable cell-style object                                                                            |
|  [04]   | `Workbook.define_name`    | `define_name(name: str, formula: str) -> Literal[0, -1]`                                                              | register a workbook-scoped defined name                                                                      |
|  [05]   | `Workbook.set_properties` | `set_properties(properties) -> None`                                                                                  | set document core properties (title/author/etc.)                                                             |
|  [06]   | `Workbook.worksheets`     | `worksheets() -> List[Worksheet]`                                                                                     | enumerate minted sheets                                                                                      |
|  [07]   | `Workbook.close`          | `close() -> None`                                                                                                     | flush, package, and write the `.xlsx` container                                                              |

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
|  [10]   | `Worksheet.set_column`     | `set_column(first_col: int, last_col: int, width: float \| None = None, cell_format: Format \| None = None, options: Dict[str, Any] \| None = None) -> Literal[0, -1]` | set column width/default format/visibility                                          |
|  [11]   | `Worksheet.set_row`        | `set_row(row: int, height: float \| None = None, cell_format: Format \| None = None, options: Dict[str, Any] \| None = None) -> Literal[0, -1]`                        | set row height/default format (call before writing the row under `constant_memory`) |
|  [12]   | `Worksheet.freeze_panes`   | `freeze_panes(row: int, col: int, top_row: int \| None = None, left_col: int \| None = None, pane_type: int = 0) -> None`                                              | freeze header rows/columns                                                          |

[ENTRYPOINT_SCOPE]: `Format` style minting
- rail: spreadsheet

`Format` is constructed by `add_format`; its `set_*` family (60 setters) configures number format, font, alignment, fill, and border. The style object is shared by reference across cells and sheets to keep the format table small.

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
- write axis: `Worksheet.write` is the single type-dispatching entry; it discriminates str/number/bool/datetime/url and routes to the typed writers. `write_row`/`write_column` are the bulk-sequence rows with one shared `cell_format`; typed `write_string`/`write_number`/`write_datetime` are forced-type rows, never a per-type sheet object.
- style axis: `Format` is minted once by `add_format` and shared by reference across cells and sheets; its `set_*` setters are the style rows. The format object is a reused style table entry, never a per-cell style allocation.
- lifecycle axis: `Workbook.close` is the single serialization trigger — it flushes pending rows, packages worksheets, shared strings, styles, and metadata into the zip container, and writes the `.xlsx`. The workbook is invalid after `close`; serialization happens exactly once.
- evidence: each workbook captures target path/stream, sheet count, written row/column extents, `constant_memory`/`in_memory` mode, zip64 flag, and output byte length as a spreadsheet receipt.
- boundary: xlsxwriter owns XLSX generation, OOXML packaging, and zip container assembly with no read capability; reading or editing existing `.xlsx` routes elsewhere; the streamed file feeds the artifacts download and export owners directly; live UI rendering stays outside this package.

[RAIL_LAW]:
- Package: `xlsxwriter`
- Owns: constant-memory streaming XLSX authoring — `Workbook` container lifecycle, `Worksheet` row-major writes, `Format` style minting, and OOXML/zip64 packaging at `close`
- Accept: write-only XLSX generation feeding the artifacts spreadsheet, download, and export owners, including large datasets under `constant_memory`
- Reject: wrapper-renames of `Workbook`/`add_worksheet`/`write`; a hand-rolled OOXML or zip packager; an in-RAM row matrix where `constant_memory` already streams; a parallel writer type per cell value type; reading or editing an existing workbook this package never opens for input
