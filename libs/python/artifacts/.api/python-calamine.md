# [PY_ARTIFACTS_API_PYTHON_CALAMINE]

`python-calamine` binds the calamine Rust core as the read-only spreadsheet ingress for the artifacts office rail beside the odfpy ODS arm: `CalamineWorkbook` opens the OOXML, binary-Excel, and OpenDocument family (`.xlsx`/`.xlsm`/`.xlsb`/`.xls`/`.xla`/`.xlam`/`.ods`) from a path, `os.PathLike`, or seek+read buffer, introspects sheets and tables without materializing a grid, and projects each sheet to a native-scalar row matrix through `to_python`/`iter_rows` with calamine's own date-serial decode.

`python-calamine` reads the binary-Excel and SpreadsheetML formats odfpy's OASIS parser cannot and re-implements none of the unzip, BIFF reader, or shared-string decode the Rust core owns, evaluating no formula; `LensProvider.CALAMINE` folds each row matrix through its `XLSX_READ` arm into a `document/model` `TableNode` of `RunNode` cells, and authoring stays at the openpyxl/xlsxwriter/odfpy owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-calamine`
- package: `python-calamine`
- import: `python_calamine` (re-exports the native `python_calamine._python_calamine` PyO3 extension)
- owner: `artifacts`
- rail: office-ingest
- build: PyO3/Rust C-extension, version-specific ABI (not abi3)
- license: MIT
- depends: none — the calamine Rust core owns the unzip, BIFF, shared-string, and date-serial decode
- entry points: import-only; no console script

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: workbook, sheet, and table roots

`CalamineWorkbook` is the single read root — the source kind is a classmethod choice (`from_path`, `from_filelike`, `from_object`), never a parallel workbook class. `CalamineSheet` is the one grid value; `to_python` and `iter_rows` project that grid two ways, and a cell is the native scalar in the row list, so no `Cell` wrapper allocates.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                                                                               |
| :-----: | :----------------- | :--------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `CalamineWorkbook` | workbook root    | read root; props `path`/`sheet_names`/`sheets_metadata`/`table_names`; `close` ctx         |
|  [02]   | `CalamineSheet`    | sheet grid       | cell matrix; `name`, geometry, `merged_cell_ranges`; `to_python`/`iter_rows`               |
|  [03]   | `CalamineTable`    | Excel table      | Excel `Table` under `load_tables=True`; `name`/`sheet`/`columns`/geometry, `to_python()`   |
|  [04]   | `SheetMetadata`    | sheet descriptor | `sheets_metadata` row; `name`/`typ`(`SheetTypeEnum`)/`visible`(`SheetVisibleEnum`)         |
|  [05]   | `SheetTypeEnum`    | sheet-kind enum  | `WorkSheet`/`DialogSheet`/`MacroSheet`/`ChartSheet`/`Vba` (Excel only; ODS -> `WorkSheet`) |
|  [06]   | `SheetVisibleEnum` | visibility enum  | `Visible`/`Hidden`/`VeryHidden` — sheet visibility for an introspection filter             |

[PUBLIC_TYPE_SCOPE]: ingest fault family

`CalamineError` is the base every binding error derives from; the worker boundary maps it and each subclass into the `runtime` `RuntimeRail` fault.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                                     |
| :-----: | :------------------- | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `CalamineError`      | base fault      | the `Exception` base every binding error derives from; the one boundary-map root |
|  [02]   | `PasswordError`      | decrypt fault   | password-protected workbook; no decrypt, route `msoffcrypto-tool` upstream       |
|  [03]   | `WorksheetNotFound`  | lookup fault    | `get_sheet_by_name`/`get_sheet_by_index` named a sheet absent from the workbook  |
|  [04]   | `XmlError`           | container fault | corrupt SpreadsheetML XML                                                        |
|  [05]   | `ZipError`           | container fault | malformed OOXML/ODS zip container                                                |
|  [06]   | `WorkbookClosed`     | lifecycle fault | a `get_sheet_*`/`get_table_*` call after `close()` dropped the Rust handle       |
|  [07]   | `TablesNotLoaded`    | table fault     | `get_table_by_name` without `load_tables=True`                                   |
|  [08]   | `TablesNotSupported` | table fault     | `get_table_by_name` on a non-xlsx format                                         |
|  [09]   | `TableNotFound`      | table fault     | `get_table_by_name` for a missing table name                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: workbook open and introspect

Every open returns `CalamineWorkbook` and carries the `load_tables=False` knob (xlsx-only Excel `Table` parse). `from_object` discriminates a path against a `read`/`seek` buffer, so the gated arm opens a `BytesIO` payload with no temp file; `from_path`/`from_filelike` are the explicit-source forms and `load_workbook` the module-level mirror. Sheets decode lazily on `get_sheet_*`, so the introspection properties read structure without materializing a grid.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :--------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `from_object(path_or_filelike)`          | factory  | shape-discriminates a path against a `read`/`seek` buffer |
|  [02]   | `from_path(path)`                        | factory  | explicit path open                                        |
|  [03]   | `from_filelike(filelike)`                | factory  | explicit seek+read buffer open                            |
|  [04]   | `load_workbook(path_or_filelike)`        | static   | module-level mirror of `from_object`                      |
|  [05]   | `sheet_names -> list[str]`               | property | sheet names in workbook order                             |
|  [06]   | `sheets_metadata -> list[SheetMetadata]` | property | per-sheet `name`/`typ`/`visible`, no grid read            |
|  [07]   | `table_names -> list[str] \| None`       | property | `None` unless `load_tables=True`                          |
|  [08]   | `path -> str \| None`                    | property | `None` when opened from bytes                             |
|  [09]   | `close()`                                | instance | context-managed drop of the Rust handle                   |

[ENTRYPOINT_SCOPE]: sheet resolve and row-matrix projection

`get_sheet_by_name`/`get_sheet_by_index` resolve one `CalamineSheet`; `to_python` is the eager matrix and `iter_rows` the constant-memory stream over the same grid, `skip_empty_area=True` trimming leading empty rows/cols and `nrows` capping the read. Each row is a `list` of `CellValue = int | float | str | bool | datetime.{time,date,datetime,timedelta}` native scalars, calamine decoding date serials and durations to the `datetime` types directly.

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `get_sheet_by_name(name) -> CalamineSheet`    | instance | resolve by name; raises `WorksheetNotFound`   |
|  [02]   | `get_sheet_by_index(index) -> CalamineSheet`  | instance | resolve by position                           |
|  [03]   | `to_python(skip_empty_area=True, nrows=None)` | instance | eager `list[list[CellValue]]` matrix          |
|  [04]   | `iter_rows(skip_empty_area=True, nrows=None)` | instance | constant-memory row stream over the same grid |
|  [05]   | `merged_cell_ranges -> list[tuple] \| None`   | property | `(start_rc, end_rc)` quads; `None` for ods    |
|  [06]   | `get_table_by_name(name) -> CalamineTable`    | instance | one Excel `Table`; needs `load_tables=True`   |

- [GEOMETRY]: `CalamineSheet` props `name` `height` `width` `total_height` `total_width` `start` `end` (`start`/`end` -> `tuple[int,int] | None`)

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `from_object` is the one shape-discriminating ingress and `load_tables=True` (xlsx only) the single knob, never a per-format open; introspection reads workbook structure before any grid, filtering a non-`WorkSheet` `SheetTypeEnum` or `Hidden`/`VeryHidden` `SheetVisibleEnum` at the boundary.
- A cell is a native `CellValue` scalar in the row list — no `Cell` wrapper and no formula evaluation, calamine reading the stored value and decoding a date serial to `datetime` directly.
- A module-scope `lazy import python_calamine` defers the PyO3 binding, and `_gated_recover` reifies it inside a worker process, so the native extension is paid only on an `XLSX_READ` op and only in the subprocess, trapping a Rust panic at the process seam.

[STACKING]:
- `folder:document/lens#LENS` (READ): the `LensProvider.CALAMINE` `XLSX_READ` recover arm — `from_object(BytesIO)` → `get_sheet_*` → `to_python` folds each row matrix into a `document/model#NODE` `TableNode` of `RunNode` cells, `merged_cell_ranges` into `TableNode.spans` and `start`/`end`/`height`/`width` into its bounds; `_gated_recover` re-resolves the `_ROUTES[XLSX_READ]` row and maps the `CalamineError` family to the read rail beside `pymupdf`/`pdfplumber`.
- `folder:core/receipt#RECEIPT`: each ingest contributes the `ArtifactReceipt.Introspection(key, nodes, text_len, images, hits)` case through the `ReceiptContributor` port, keyed by the `document/model` `node_digest` `xxhash` merkle through `ContentIdentity.of`.
- `msoffcrypto-tool`: a `PasswordError` routes the workbook through its decrypt, and the decrypted `BytesIO` re-enters `from_object` — calamine never decrypts.
- `fastexcel`: disjoint on the shared Rust core — `fastexcel` owns the calamine-to-Arrow PyCapsule/`pyarrow.RecordBatch` columnar path for `libs/python/data`, this owner the `to_python` document-tree path for artifacts.
- universal tier (`libs/python/.api`): `expression` `Result[Self, LensFault]`/`RuntimeRail` owns the admission fold and fault discriminant; the `runtime` `LanePolicy` bounds the `anyio` process band each worker decode runs in.

[LOCAL_ADMISSION]:
- Spreadsheet and binary-Excel ingest feeding the `document/lens#LENS` `XLSX_READ` arm, including `msoffcrypto-tool`-decrypted workbooks and the in-memory `from_object(BytesIO(payload))` open; `CALAMINE` rides `_GATED_PROVIDERS` for worker isolation of the Rust core, distinct from the manifest environment marker scoping the arm's availability.

[RAIL_LAW]:
- Package: `python-calamine`
- Owns: read-only ingest of the Excel/OpenDocument family via `from_object`/`from_path`/`from_filelike`/`load_workbook`, workbook and table introspection, sheet resolution, the `to_python`/`iter_rows` native-scalar projection with date-serial decode, sheet geometry and `merged_cell_ranges`, and the `CalamineError` fault family.
- Accept: the `XLSX_READ` document-tree path — a row matrix folded into a `TableNode` of `RunNode` cells with `merged_cell_ranges` into `TableNode.spans`, sealed by content key into `ArtifactReceipt.Introspection`.
- Reject: any authoring path (openpyxl/xlsxwriter own `.xlsx`, odfpy owns `.ods`); the Arrow columnar frame (`fastexcel`); per-cell formula evaluation; workbook decryption (route a `PasswordError` through `msoffcrypto-tool`).
