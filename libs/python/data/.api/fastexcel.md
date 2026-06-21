# [PY_DATA_API_FASTEXCEL]

`fastexcel` binds the Rust `calamine` Excel reader to an Arrow boundary for the data ingestion rail: a `read_excel` factory opens a workbook into an `ExcelReader`, and a single polymorphic `load_sheet`/`load_table` surface materializes any sheet or named table to a lazy Arrow PyCapsule (`ExcelSheet`/`ExcelTable`) or an eager `pyarrow.RecordBatch`. The package owner composes `read_excel`, `ExcelReader.load_sheet`, and the PyCapsule export into the FASTEXCEL_DATASET path; it carries `header_row`, `skip_rows`, `use_columns`, `dtypes`, and `dtype_coercion` as call rows, exports zero-copy through `__arrow_c_array__` without a mandatory `pyarrow` dependency, and never re-implements the calamine xlsx/xls/ods/xlsb decode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fastexcel`
- package: `fastexcel`
- import: `fastexcel`
- owner: `data`
- rail: ingestion
- installed: `0.20.2` reflected via `forge-scientific-env python -m tools.assay api query --key fastexcel --symbol fastexcel` on cp315
- entry points: no console script; library use is import-only via `read_excel`
- capability: calamine-backed xlsx/xlsm/xlsb/xls/ods reading into Arrow; lazy PyCapsule or eager `pyarrow.RecordBatch` materialization; per-column dtype control and coercion modes; header/skip/row-count windowing; Excel-letter and callable column selection; named-table and defined-name access; per-cell parse-error capture; zero-copy `to_polars`/`to_pandas` bridges

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader, materialized blocks, metadata, and failures
- rail: ingestion

`read_excel` returns an `ExcelReader`; `load_sheet`/`load_table` return an `ExcelSheet`/`ExcelTable` (lazy) or a `pyarrow.RecordBatch` (eager). `ColumnInfo`/`ColumnInfoNoDtype` describe resolved columns, `DefinedName` describes workbook named ranges, and `CellError`/`CellErrors` carry per-cell parse failures. Every exception derives from `FastExcelError`.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `ExcelReader`                           | reader        | open workbook exposing sheet/table load                  |
|  [02]   | `ExcelSheet`                            | data block    | one sheet as an Arrow PyCapsule with metadata            |
|  [03]   | `ExcelTable`                            | data block    | one named table as an Arrow PyCapsule with metadata      |
|  [04]   | `ColumnInfo`                            | metadata      | resolved column name, index, dtype, and provenance       |
|  [05]   | `ColumnInfoNoDtype`                     | metadata      | pre-dtype column descriptor for `use_columns` predicates |
|  [06]   | `DefinedName`                           | metadata      | workbook named range (`name`, `formula`)                 |
|  [07]   | `CellError`                             | metadata      | one cell parse failure with position and detail          |
|  [08]   | `CellErrors`                            | metadata      | collection of `CellError` from a fallible conversion     |
|  [09]   | `DType`                                 | type alias    | column dtype literal vocabulary                          |
|  [10]   | `DTypeMap`                              | type alias    | `dict[str \| int, DType]` per-column dtype override      |
|  [11]   | `ColumnNameFrom`                        | type alias    | column-name provenance literal                           |
|  [12]   | `DTypeFrom`                             | type alias    | dtype provenance literal                                 |
|  [13]   | `FastExcelError`                        | error         | base failure for every `fastexcel` exception             |
|  [14]   | `CalamineError`                         | error         | calamine engine read failure                             |
|  [15]   | `CalamineCellError`                     | error         | calamine cell-level read failure                         |
|  [16]   | `CannotRetrieveCellDataError`           | error         | cell-data retrieval failure                              |
|  [17]   | `SheetNotFoundError`                    | error         | requested sheet index/name absent                        |
|  [18]   | `ColumnNotFoundError`                   | error         | requested column absent                                  |
|  [19]   | `ArrowError`                            | error         | Arrow conversion/export failure                          |
|  [20]   | `InvalidParametersError`                | error         | inconsistent load parameters                             |
|  [21]   | `UnsupportedColumnTypeCombinationError` | error         | column mixes types coercion cannot reconcile             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open factory
- rail: ingestion

`read_excel` is the single open surface; `source` discriminates between a filesystem path (`str`/`Path`, `~` expanded) and in-memory `bytes`.

| [INDEX] | [SURFACE]    | [CALL_SHAPE]                                              | [CAPABILITY]                                |
| :-----: | :----------- | :-------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `read_excel` | `read_excel(source: Path \| str \| bytes) -> ExcelReader` | open a workbook from a path or bytes buffer |

[ENTRYPOINT_SCOPE]: `ExcelReader` load and inspect
- rail: ingestion

`load_sheet` is the polymorphic materialization surface: `idx_or_name` discriminates sheet-by-index versus sheet-by-name, and `eager` selects a lazy `ExcelSheet` (`False`, PyCapsule) versus an eager `pyarrow.RecordBatch` (`True`). `load_sheet_by_name`/`load_sheet_by_idx`/`load_sheet_eager` are fixed-arity rows over the same kernel. `load_table` materializes a named table; `table_names` and `defined_names` enumerate workbook structure.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                                                                                                                                                                                    | [CAPABILITY]                                  |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `ExcelReader.load_sheet`         | `load_sheet(idx_or_name: int \| str, *, header_row=0, column_names=None, skip_rows=None, n_rows=None, schema_sample_rows=1000, dtype_coercion="coerce", use_columns=None, dtypes=None, eager=False, skip_whitespace_tail_rows=False, whitespace_as_null=False) -> ExcelSheet \| pa.RecordBatch` | materialize a sheet (lazy or eager)           |
|  [02]   | `ExcelReader.load_sheet_eager`   | `load_sheet_eager(idx_or_name: int \| str, *, header_row=0, column_names=None, skip_rows=None, n_rows=None, schema_sample_rows=1000, dtype_coercion="coerce", use_columns=None, dtypes=None) -> pa.RecordBatch`                                                                                 | eager sheet load via borrowed `pyarrow` types |
|  [03]   | `ExcelReader.load_sheet_by_name` | `load_sheet_by_name(name: str, *, header_row=0, column_names=None, skip_rows=None, n_rows=None, schema_sample_rows=1000, dtype_coercion="coerce", use_columns=None, dtypes=None) -> ExcelSheet`                                                                                                 | lazy sheet load fixed to name                 |
|  [04]   | `ExcelReader.load_sheet_by_idx`  | `load_sheet_by_idx(idx: int, *, header_row=0, column_names=None, skip_rows=None, n_rows=None, schema_sample_rows=1000, dtype_coercion="coerce", use_columns=None, dtypes=None) -> ExcelSheet`                                                                                                   | lazy sheet load fixed to index                |
|  [05]   | `ExcelReader.load_table`         | `load_table(name: str, *, header_row=None, column_names=None, skip_rows=None, n_rows=None, schema_sample_rows=1000, dtype_coercion="coerce", use_columns=None, dtypes=None, eager=False, skip_whitespace_tail_rows=False, whitespace_as_null=False) -> ExcelTable \| pa.RecordBatch`            | materialize a named table (lazy or eager)     |
|  [06]   | `ExcelReader.table_names`        | `table_names(sheet_name: str \| None = None) -> list[str]`                                                                                                                                                                                                                                      | list table names (optionally one sheet)       |
|  [07]   | `ExcelReader.defined_names`      | `defined_names() -> list[DefinedName]`                                                                                                                                                                                                                                                          | list workbook defined names                   |
|  [08]   | `ExcelReader.sheet_names`        | property -> `list[str]`                                                                                                                                                                                                                                                                         | the list of sheet names                       |

[ENTRYPOINT_SCOPE]: `ExcelSheet` materialize and inspect
- rail: ingestion

A lazy `ExcelSheet` exports through the Arrow PyCapsule interface (`__arrow_c_schema__`/`__arrow_c_array__`) so `to_arrow`/`to_polars`/`to_pandas` and any Arrow consumer read it zero-copy; `to_arrow_with_errors` returns the batch plus a `CellErrors` (or `None`). Read properties carry the dimensions and column metadata the receipt records. `ExcelTable` exposes the identical materialization surface plus `sheet_name` and `offset`.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                          | [CAPABILITY]                                           |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `ExcelSheet.to_arrow`             | `to_arrow() -> pa.RecordBatch`                                        | materialize to a `pyarrow.RecordBatch`                 |
|  [02]   | `ExcelSheet.to_arrow_with_errors` | `to_arrow_with_errors() -> tuple[pa.RecordBatch, CellErrors \| None]` | materialize plus captured per-cell parse errors        |
|  [03]   | `ExcelSheet.to_polars`            | `to_polars() -> pl.DataFrame`                                         | zero-copy Polars frame via PyCapsule                   |
|  [04]   | `ExcelSheet.to_pandas`            | `to_pandas() -> pd.DataFrame`                                         | Pandas frame via Arrow conversion                      |
|  [05]   | `ExcelSheet.available_columns`    | `available_columns() -> list[ColumnInfo]`                             | columns available before `use_columns` selection       |
|  [06]   | `ExcelSheet.__arrow_c_schema__`   | `__arrow_c_schema__() -> object`                                      | export the `ArrowSchema` PyCapsule                     |
|  [07]   | `ExcelSheet.__arrow_c_array__`    | `__arrow_c_array__(requested_schema=None) -> tuple[object, object]`   | export the `ArrowSchema`/`ArrowArray` PyCapsules       |
|  [08]   | `ExcelSheet.name`                 | property -> `str`                                                     | the sheet name                                         |
|  [09]   | `ExcelSheet.width`                | property -> `int`                                                     | the sheet width                                        |
|  [10]   | `ExcelSheet.height`               | property -> `int`                                                     | the height with `skip_rows`/`n_rows` applied           |
|  [11]   | `ExcelSheet.total_height`         | property -> `int`                                                     | the sheet total height                                 |
|  [12]   | `ExcelSheet.selected_columns`     | property -> `list[ColumnInfo]`                                        | the columns retained after selection                   |
|  [13]   | `ExcelSheet.specified_dtypes`     | property -> `DTypeMap \| None`                                        | the dtypes specified for the sheet                     |
|  [14]   | `ExcelSheet.visible`              | property -> `SheetVisible`                                            | the sheet visibility (`visible`/`hidden`/`veryhidden`) |
|  [15]   | `ExcelTable.sheet_name`           | property -> `str`                                                     | the sheet a table belongs to                           |
|  [16]   | `ExcelTable.offset`               | property -> `int`                                                     | the table offset before data starts                    |

## [04]-[IMPLEMENTATION_LAW]

[INGESTION_EXCEL]:
- import: `import fastexcel` at boundary scope only; module-level import is banned by the manifest import policy.
- open axis: one `read_excel` owns workbook opening; `source` discriminates path versus `bytes` in the request value, never a per-source factory type — `~` expansion happens once at the boundary for `str`/`Path`.
- load axis: one `load_sheet` owns sheet materialization; `idx_or_name` discriminates index versus name and `eager` discriminates lazy `ExcelSheet` versus eager `pyarrow.RecordBatch` as call rows; `load_sheet_by_name`/`load_sheet_by_idx`/`load_sheet_eager` are fixed-arity convenience rows over the same kernel, never parallel readers; `load_table` is the named-table row sharing the identical policy axis.
- window axis: `header_row`, `column_names`, `skip_rows`, `n_rows`, `skip_whitespace_tail_rows`, and `whitespace_as_null` are windowing call rows; row windowing is a parameter, never a pre-filtered copy of the sheet.
- dtype axis: `dtypes` (a `DType` for all columns or a `DTypeMap` keyed by index/name) overrides per-column inference; `schema_sample_rows` bounds inference; `dtype_coercion` selects `coerce` versus `strict` failure on mixed columns — typing is a call row, never a downstream cast pass.
- selection axis: `use_columns` selects by name/index list, Excel-letter range string (`"A:E"`, `":C,E:"`), or `Callable[[ColumnInfoNoDtype], bool]` predicate; column pruning happens at decode, never as a post-load projection.
- export axis: lazy `ExcelSheet`/`ExcelTable` export through `__arrow_c_schema__`/`__arrow_c_array__`; `to_polars` and any PyCapsule consumer read zero-copy with no mandatory `pyarrow`; `to_arrow`/`to_pandas` and `eager=True` require the `pyarrow` extra; `to_arrow_with_errors` pairs the batch with a `CellErrors` receipt.
- error axis: every failure derives from `FastExcelError`; `CalamineError`/`CalamineCellError`/`CannotRetrieveCellDataError`/`ArrowError`/`SheetNotFoundError`/`ColumnNotFoundError`/`InvalidParametersError`/`UnsupportedColumnTypeCombinationError` are typed rows mapped to the data error rail; per-cell parse failures surface as `CellError` (position, offset, detail) inside `CellErrors`, never as silent nulls.
- evidence: each load captures sheet/table name, width, height, total height, selected `ColumnInfo` (name, index, dtype, name/dtype provenance), specified dtypes, visibility, and any `CellErrors` as an ingestion receipt.
- boundary: fastexcel owns calamine xlsx/xlsm/xlsb/xls/ods decode and the Arrow export edge; the Arrow batch feeds the dataset and frame owners directly; spreadsheet authoring, formula evaluation, and styling stay outside this package.

[RAIL_LAW]:
- Package: `fastexcel`
- Owns: calamine-backed Excel reading into Arrow, per-column dtype control and coercion, header/skip/row windowing, Excel-letter and callable column selection, named-table and defined-name access, per-cell parse-error capture, and zero-copy PyCapsule export to Polars/pandas/pyarrow
- Accept: Excel workbook ingestion into Arrow record batches feeding the dataset, frame, and persistence owners
- Reject: wrapper-renames of `read_excel`/`load_sheet`; a hand-rolled xlsx/ods decoder; a parallel reader type per source kind or per eager/lazy mode; a per-sheet column-name family such as `load_by_a`/`load_by_b`; a forced `pyarrow` dependency where the PyCapsule export needs none; silent dropping of unparseable cells the `CellErrors` receipt must record
