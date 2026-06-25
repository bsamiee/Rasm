# [PY_ARTIFACTS_API_PYTHON_CALAMINE]

`python-calamine` is the lazy cell-grid spreadsheet reader for the artifacts ingestion rail: a `load_workbook` factory (and the `CalamineWorkbook.from_path`/`from_filelike`/`from_object` classmethods) opens an xlsx/xlsm/xlsb/xls/ods/csv container through the Rust `calamine` engine into a `CalamineWorkbook`, whose `get_sheet_by_name`/`get_sheet_by_index` yield a `CalamineSheet` that materializes either an eager nested `to_python` cell grid or a streaming `iter_rows` row iterator of the typed scalar union `int | float | str | bool | time | date | datetime | timedelta`. The package owner composes `load_workbook`, the `CalamineSheet` row stream, and `sheets_metadata` into the `SPREADSHEET_CELLGRID` path; it carries `skip_empty_area`/`nrows` as call rows, exposes merged-range and used-range geometry, optional Excel `CalamineTable` access (`load_tables=True`), and never re-implements the calamine OOXML/BIFF/ODF decode. It is the cell-level inverse of `fastexcel`: `fastexcel` (same calamine engine) owns the eager zero-copy Arrow path for tabular ingestion, while `python-calamine` owns the cell-exact Python-object grid where per-cell typing, merged ranges, sheet kind, and ODF/legacy-`xls` reach matter and a frame is not the target — both are the calamine ingress edge, never a hand-rolled spreadsheet parser.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-calamine`
- package: `python-calamine`
- import: `python_calamine`
- owner: `artifacts`
- rail: ingestion
- marker: `python_version<'3.15'` (companion-gated — pinned `python-calamine; python_version<'3.15'`; no cp315 wheel, so it does not install on the active 3.15 interpreter and `assay api resolve` reflection is blocked there; surface reflected from an isolated cp313 install + the bundled `_python_calamine.pyi` source stub, which is the authoritative public contract)
- installed: `0.7.0` (isolated cp313 install); license `MIT` (`License-Expression: MIT`)
- abi: native maturin/pyo3 extension (`_python_calamine.cpython-3xx-<plat>.so`, `Root-Is-Purelib: false`); per-interpreter version-specific cp tag (NOT abi3) — this is the cp315-wheel gap that gates the marker; `Requires-Python >=3.10`; ships `py.typed` + `_python_calamine.pyi`; zero runtime `Requires-Dist` (the calamine Rust engine is statically linked)
- entry points: no console script and no pandas-engine entry point in this package; library use is import-only via `load_workbook` / `CalamineWorkbook`. `pandas>=2.2` carries its own built-in `engine="calamine"` reader that calls `python_calamine.load_workbook` internally — the consumer side of the integration lives in pandas, not here
- capability: calamine-backed xlsx/xlsm/xlsb/xls/ods/csv reading into native Python objects; eager nested-list `to_python` grid OR streaming `iter_rows` row iterator; per-cell value typing into the `int|float|str|bool|time|date|datetime|timedelta` union (dates/times/durations decoded to `datetime` objects, not serials); `skip_empty_area` used-region trimming and `nrows` row capping; used-range geometry (`start`/`end`/`width`/`height`/`total_width`/`total_height`); merged-cell ranges; sheet-kind and visibility metadata (`SheetMetadata`/`SheetTypeEnum`/`SheetVisibleEnum`); optional Excel structured-table access (`load_tables=True` -> `table_names`/`get_table_by_name`/`CalamineTable`); path, file-like (`seek`/`read` `ReadBuffer`), and in-memory-bytes sources; context-manager workbook lifecycle with explicit `close`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook, sheet, table, and metadata
- rail: ingestion

`load_workbook` (and the three `CalamineWorkbook.from_*` classmethods) returns a `CalamineWorkbook`; `get_sheet_by_name`/`get_sheet_by_index` return a `CalamineSheet`; `get_table_by_name` returns a `CalamineTable` (only after `load_tables=True`). `sheets_metadata` is a `list[SheetMetadata]` carrying `name`/`typ`/`visible`, where `typ` is a `SheetTypeEnum` and `visible` is a `SheetVisibleEnum`. The full `__all__` is 16 symbols: the four types above plus `SheetMetadata`, the two enums, `load_workbook`, and the nine-member exception family.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                                                          |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `CalamineWorkbook`        | reader        | open workbook exposing sheet/table load, metadata, and context-manager lifecycle               |
|  [02]   | `CalamineSheet`           | data block    | one sheet as an eager nested grid (`to_python`) or streaming row iterator (`iter_rows`)         |
|  [03]   | `CalamineTable`           | data block    | one Excel structured table (`load_tables=True`) with `columns`/`sheet` and an eager grid       |
|  [04]   | `SheetMetadata`           | metadata      | per-sheet record: `name`, `typ: SheetTypeEnum`, `visible: SheetVisibleEnum`                     |
|  [05]   | `SheetTypeEnum`           | enum          | sheet kind — `WorkSheet`/`DialogSheet`/`MacroSheet`/`ChartSheet`/`Vba` (pyo3-native, see below) |
|  [06]   | `SheetVisibleEnum`        | enum          | sheet visibility — `Visible`/`Hidden`/`VeryHidden` (pyo3-native, see below)                     |
|  [07]   | `ReadBuffer`              | type alias    | `type_check_only` Protocol: a file-like with `seek(offset, whence=...)`/`read(size=...)->bytes` |

[PUBLIC_TYPE_SCOPE]: cell scalar union
- rail: ingestion
- The per-cell decoded type, shared by `CalamineSheet.to_python`/`iter_rows` and `CalamineTable.to_python`. calamine resolves each cell to one of these eight native types — dates/times/durations are real `datetime` objects (not Excel serials), booleans are `bool`, numbers split `int`/`float`, and text is `str`. This is the discriminant the substrate row-shaper folds over; there is no string-of-everything fallback.

| [INDEX] | [PYTHON_TYPE]        | [CELL_MEANING]                                          |
| :-----: | :------------------- | :----------------------------------------------------- |
|  [01]   | `int`                | integer-valued numeric cell                            |
|  [02]   | `float`              | non-integer numeric cell                               |
|  [03]   | `str`                | text / inline-string / shared-string cell              |
|  [04]   | `bool`               | boolean cell                                           |
|  [05]   | `datetime.time`      | time-of-day cell                                       |
|  [06]   | `datetime.date`      | date cell                                              |
|  [07]   | `datetime.datetime`  | date-time cell                                         |
|  [08]   | `datetime.timedelta` | duration cell (`[hh]:mm:ss`-style elapsed time)        |

[PUBLIC_TYPE_SCOPE]: failure family
- rail: ingestion
- Every failure derives from `CalamineError(Exception)`; the eight leaves are all DIRECT subclasses (flat hierarchy, no intermediate bases), so one `except CalamineError` traps the engine and the discriminated leaves map onto distinct error-rail rows. `WorkbookClosed` enforces the context-manager lifecycle; the three `Tables*` leaves gate the optional structured-table surface.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                                        |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `CalamineError`       | error (base)  | root of every calamine failure (`Exception` subclass)                        |
|  [02]   | `PasswordError`       | error         | workbook is encrypted / password-protected (decrypt upstream first)          |
|  [03]   | `WorksheetNotFound`   | error         | requested sheet name/index absent                                            |
|  [04]   | `XmlError`            | error         | malformed OOXML/ODF XML in the container                                     |
|  [05]   | `ZipError`            | error         | corrupt or unreadable ZIP container (xlsx/xlsm/xlsb/ods are zip archives)     |
|  [06]   | `WorkbookClosed`      | error         | operation attempted after `close()` / context exit                           |
|  [07]   | `TablesNotLoaded`     | error         | table access without `load_tables=True` at open time                         |
|  [08]   | `TablesNotSupported`  | error         | container format has no structured-table concept (e.g. csv/ods)              |
|  [09]   | `TableNotFound`       | error         | requested table name absent                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open factory
- rail: ingestion

`load_workbook` is the single canonical open surface; `path_or_filelike` discriminates a filesystem path (`str`/`os.PathLike`) from a file-like `ReadBuffer` (or in-memory `bytes`) in the request value, never a per-source factory. `CalamineWorkbook.from_object` is the same polymorphic kernel `load_workbook` delegates to; `from_path`/`from_filelike` are the fixed-arity source-pinned classmethods over that kernel. `load_tables=False` defers structured-table parsing (cheaper open); `load_tables=True` is required before any `table_names`/`get_table_by_name` access.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                | [CAPABILITY]                                          |
| :-----: | :------------------------------- | :--------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `load_workbook`                  | `load_workbook(path_or_filelike: str \| os.PathLike \| ReadBuffer, load_tables: bool = False) -> CalamineWorkbook` | open a workbook from a path, file-like, or bytes      |
|  [02]   | `CalamineWorkbook.from_object`   | `from_object(path_or_filelike: str \| os.PathLike \| ReadBuffer, load_tables: bool = False) -> CalamineWorkbook` | polymorphic open kernel (path/file-like/bytes)        |
|  [03]   | `CalamineWorkbook.from_path`     | `from_path(path: str \| os.PathLike, load_tables: bool = False) -> CalamineWorkbook`                        | open fixed to a filesystem path                       |
|  [04]   | `CalamineWorkbook.from_filelike` | `from_filelike(filelike: ReadBuffer, load_tables: bool = False) -> CalamineWorkbook`                        | open fixed to a `seek`/`read` file-like buffer        |

[ENTRYPOINT_SCOPE]: `CalamineWorkbook` inspect, load, and lifecycle
- rail: ingestion

`get_sheet_by_name`/`get_sheet_by_index` are the two sheet-resolution rows (name versus ordinal); `sheet_names`/`sheets_metadata` enumerate workbook structure (the metadata carries kind and visibility per sheet without materializing any cell). `table_names`/`get_table_by_name` are the optional Excel structured-table rows gated on `load_tables=True`. The workbook is a context manager (`with load_workbook(...) as wb:`) and `close()` releases the native handle; any access after close raises `WorkbookClosed`.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                                | [CAPABILITY]                                              |
| :-----: | :----------------------------------- | :--------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `CalamineWorkbook.get_sheet_by_name` | `get_sheet_by_name(name: str) -> CalamineSheet`            | resolve a sheet by name                                  |
|  [02]   | `CalamineWorkbook.get_sheet_by_index`| `get_sheet_by_index(index: int) -> CalamineSheet`          | resolve a sheet by ordinal                               |
|  [03]   | `CalamineWorkbook.get_table_by_name` | `get_table_by_name(name: str) -> CalamineTable`            | resolve an Excel structured table (needs `load_tables`)  |
|  [04]   | `CalamineWorkbook.sheet_names`       | property -> `list[str]`                                     | ordered sheet names                                      |
|  [05]   | `CalamineWorkbook.sheets_metadata`   | property -> `list[SheetMetadata]`                          | per-sheet `name`/`typ`/`visible` records                 |
|  [06]   | `CalamineWorkbook.table_names`       | property -> `list[str] \| None`                            | structured-table names (`None` until `load_tables=True`) |
|  [07]   | `CalamineWorkbook.path`              | property -> `str \| None`                                  | source path (`None` for file-like/bytes sources)         |
|  [08]   | `CalamineWorkbook.close`             | `close() -> None`                                          | release the native workbook handle                       |
|  [09]   | `CalamineWorkbook.__enter__`         | `__enter__() -> CalamineWorkbook`                          | context-manager entry                                    |
|  [10]   | `CalamineWorkbook.__exit__`          | `__exit__(exc_type, exc_val, exc_tb) -> None`              | context-manager exit (calls `close`)                     |

[ENTRYPOINT_SCOPE]: `CalamineSheet` materialize and inspect
- rail: ingestion

`to_python` is the eager nested-list materialization (`list[list[ScalarUnion]]`); `iter_rows` is the streaming counterpart yielding one `list[ScalarUnion]` per row without building the whole grid (bounded-memory ingestion of large sheets). `skip_empty_area=True` (the default) trims the leading empty rows/cols so the grid starts at the used region; `skip_empty_area=False` includes leading blanks; `nrows` caps the eager grid to the first N data rows. `start`/`end` give the used-range corner coordinates `(row, col)` (`None` for an empty sheet); `width`/`height` are the used-region dimensions and `total_width`/`total_height` the full extents; `merged_cell_ranges` lists `((r0,c0),(r1,c1))` merge spans (`None` when unsupported by the format).

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                     | [CAPABILITY]                                            |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `CalamineSheet.to_python`          | `to_python(skip_empty_area: bool = True, nrows: int \| None = None) -> list[list[ScalarUnion]]` | eager nested-list cell grid                            |
|  [02]   | `CalamineSheet.iter_rows`          | `iter_rows() -> Iterator[list[ScalarUnion]]`                                     | streaming per-row iterator (bounded memory)            |
|  [03]   | `CalamineSheet.name`               | property -> `str`                                                                | sheet name                                             |
|  [04]   | `CalamineSheet.start`              | property -> `tuple[int, int] \| None`                                            | used-range start `(row, col)` (`None` if empty)        |
|  [05]   | `CalamineSheet.end`                | property -> `tuple[int, int] \| None`                                            | used-range end `(row, col)` (`None` if empty)          |
|  [06]   | `CalamineSheet.width`              | property -> `int`                                                                | used-region column count                               |
|  [07]   | `CalamineSheet.height`             | property -> `int`                                                                | used-region row count                                  |
|  [08]   | `CalamineSheet.total_width`        | property -> `int`                                                                | full column extent                                     |
|  [09]   | `CalamineSheet.total_height`       | property -> `int`                                                                | full row extent                                        |
|  [10]   | `CalamineSheet.merged_cell_ranges` | property -> `list[tuple[tuple[int,int], tuple[int,int]]] \| None`                | merged-cell spans (`None` if unsupported)              |

[ENTRYPOINT_SCOPE]: `CalamineTable` materialize and inspect
- rail: ingestion

`CalamineTable` is the Excel structured-table block (gated on `load_tables=True`); it shares the `to_python` eager grid and the `start`/`end`/`width`/`height` used-range geometry, and adds `name`/`sheet`/`columns` (the table's resolved column-name list). It has no `iter_rows` and no merged-range surface — table bodies are bounded, so the eager grid is the only materialization.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                              | [CAPABILITY]                                  |
| :-----: | :------------------------ | :---------------------------------------- | :-------------------------------------------- |
|  [01]   | `CalamineTable.to_python` | `to_python() -> list[list[ScalarUnion]]`  | eager nested-list table-body grid             |
|  [02]   | `CalamineTable.name`      | property -> `str`                         | table name                                    |
|  [03]   | `CalamineTable.sheet`     | property -> `str`                         | owning sheet name                             |
|  [04]   | `CalamineTable.columns`   | property -> `list[str]`                   | resolved column header names                  |
|  [05]   | `CalamineTable.start`     | property -> `tuple[int, int] \| None`     | table start `(row, col)`                       |
|  [06]   | `CalamineTable.end`       | property -> `tuple[int, int] \| None`     | table end `(row, col)`                         |
|  [07]   | `CalamineTable.width`     | property -> `int`                         | table column count                            |
|  [08]   | `CalamineTable.height`    | property -> `int`                         | table row count                               |

## [04]-[IMPLEMENTATION_LAW]

[INGESTION_SPREADSHEET]:
- import: `import python_calamine` at boundary scope only; module-level import is banned by the manifest import policy. The package is companion-gated (`python_version<'3.15'`), so any module owning it is reached on the cp313 companion interpreter; on the active 3.15 interpreter the import is absent and the rail degrades to `fastexcel` (cp315-clean abi3) for tabular workbooks where the Arrow path suffices.
- open axis: one `load_workbook` owns workbook opening; `path_or_filelike` discriminates path versus file-like/bytes in the request value, never a per-source factory — `CalamineWorkbook.from_object` is the same kernel and `from_path`/`from_filelike` are the source-pinned rows over it. `load_tables` is an open-time policy flag, not a second open surface; defer it (`False`) unless structured-table access is required.
- lifecycle axis: the workbook is a native-handle resource; open it under `with load_workbook(...) as wb:` or call `close()` explicitly. Post-close access is `WorkbookClosed`, never undefined behavior — the context manager is the canonical lifecycle, not a manual `try/finally`.
- materialize axis: one sheet exposes two materialization rows discriminated by memory posture — `to_python` (eager `list[list[ScalarUnion]]`) for bounded sheets and `iter_rows` (streaming `Iterator[list[ScalarUnion]]`) for large sheets; choose by row count, never build the full grid only to iterate it. `CalamineTable.to_python` is the bounded-table row of the same kernel.
- window axis: `skip_empty_area` (used-region trim, default `True`) and `nrows` (eager row cap) are windowing call rows on `to_python`; row windowing is a parameter, never a post-materialization slice of the grid. The used-range geometry (`start`/`end`/`width`/`height`/`total_*`) is read off the sheet, not recomputed by scanning cells.
- typing axis: each cell is decoded by the engine into the eight-member scalar union (`int|float|str|bool|time|date|datetime|timedelta`); dates/times/durations arrive as real `datetime` objects, so no Excel-serial-to-date conversion is hand-rolled downstream. The row-shaper folds over the union discriminant to project each row onto its typed target; mixed-type columns are a domain decision at the shaper, not a calamine concern.
- table axis: structured Excel tables require `load_tables=True` at open; `table_names`/`get_table_by_name`/`CalamineTable` then expose `columns`/`sheet` plus the eager body grid. Access without the flag is `TablesNotLoaded`; a format with no table concept (csv/ods) is `TablesNotSupported`; an absent name is `TableNotFound` — the three are distinct rail rows, not one generic miss.
- enum axis: `SheetTypeEnum` and `SheetVisibleEnum` are pyo3-NATIVE enum types, NOT `enum.Enum` instances at runtime — they are NOT iterable (`list(SheetTypeEnum)` raises `TypeError`) and expose members only as class attributes; the `.pyi` annotates them as `enum.Enum` for type-checkers only. Discriminate by identity (`meta.typ is SheetTypeEnum.WorkSheet`, `meta.visible is SheetVisibleEnum.Hidden`), never by iteration or value comparison; a sheet filter is an identity-match fold over `sheets_metadata`, never `for member in Enum`.
- error axis: every failure derives from `CalamineError`; `PasswordError`/`WorksheetNotFound`/`XmlError`/`ZipError`/`WorkbookClosed`/`TablesNotLoaded`/`TablesNotSupported`/`TableNotFound` are flat typed leaves mapped to the artifacts error rail. `PasswordError` is the boundary to the confidentiality owner — an encrypted workbook is decrypted upstream (`.api/msoffcrypto-tool.md`) and the plaintext bytes are re-opened via `load_workbook(<bytes>)`, never decrypted inside this rail.
- stacking: the `iter_rows` stream and `to_python` grid are pure native-Python producers, so they flow into the substrate without an Arrow dependency. The canonical dense rail is: a stamina `@retry`/`retry_context` (`.api/stamina.md`) over the file-like/transient open guards the native read; an `anyio.to_thread.run_sync` (`.api/anyio.md`) offloads the blocking calamine decode off the event loop so `iter_rows` does not stall the loop; each yielded row is a `msgspec.convert(row, type=RowStruct, ...)` (`.api/msgspec.md`) shaping the typed scalar union into a discriminated `Struct` — the eight-member cell union maps field-for-field onto the struct's annotated fields with no manual coercion; a `beartype` row contract (`.api/beartype.md`) on the shaper guards the column schema; the whole load is wrapped in a structlog/OpenTelemetry span (`.api/structlog.md`, `.api/opentelemetry-api.md`) carrying `sheet name`, `width`/`height`, used-range corners, and merged-range count as receipt facts; and the engine's typed exceptions are caught at the boundary and lifted into an `expression` `Result`/`Error(...)` rail (`.api/expression.md`) — `PasswordError` -> confidentiality boundary, `WorksheetNotFound`/`TableNotFound` -> not-found row, `XmlError`/`ZipError` -> corrupt-container row — never raised through domain logic.
- boundary: `python-calamine` owns calamine xlsx/xlsm/xlsb/xls/ods/csv decode into native Python cell objects and the cell-grid/row-stream egress edge for the artifacts ingestion rail; `fastexcel` (`.api/fastexcel.md`) owns the same engine's eager zero-copy Arrow path for the data rail (pick calamine for cell-exact typing, merged ranges, sheet kind, and ODF/legacy-`xls` reach; pick fastexcel when an Arrow `RecordBatch`/Polars frame is the target). `openpyxl` (`.api/openpyxl.md`) remains the xlsx WRITE/styling/formula owner — calamine is read-only and does not author, style, or evaluate formulas. Encryption/decryption is `msoffcrypto-tool`'s; pandas' `engine="calamine"` reader is the consumer-side integration owned by pandas, not re-exposed here.

[RAIL_LAW]:
- Package: `python-calamine`
- Owns: calamine-backed read of xlsx/xlsm/xlsb/xls/ods/csv into native Python objects, eager nested-list (`to_python`) and streaming (`iter_rows`) sheet materialization, per-cell typing into the `int|float|str|bool|time|date|datetime|timedelta` union, used-range geometry, merged-cell ranges, sheet-kind/visibility metadata, optional Excel structured-table access (`load_tables=True`), and path/file-like/bytes sources with context-manager lifecycle
- Accept: spreadsheet/ODF cell-grid ingestion feeding the row-shaper, msgspec struct decode, and artifacts receipt owners; the companion-band cell-exact reader where `fastexcel`'s Arrow path is not the target
- Reject: a hand-rolled OOXML/BIFF/ODF decoder; wrapper-renames of `load_workbook`/`get_sheet_by_*`; a parallel reader type per source kind or per eager/streaming mode where `path_or_filelike` and the `to_python`/`iter_rows` rows discriminate; `list(SheetTypeEnum)`/value-comparison enum handling where the pyo3 enums demand identity matching and are not iterable; Excel-serial-to-date conversion where the engine already returns `datetime` objects; table access without `load_tables=True` swallowing the typed `Tables*` leaves; decrypting an encrypted workbook inside this rail where `PasswordError` is the boundary to the confidentiality owner; an Arrow/Polars materialization where that is `fastexcel`'s rail; any write/style/formula path where that is `openpyxl`'s
