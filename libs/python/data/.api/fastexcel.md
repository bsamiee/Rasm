# [PY_DATA_API_FASTEXCEL]

`fastexcel` binds the Rust `calamine` Excel reader to an Arrow boundary for the data ingestion rail: a `read_excel` factory opens a workbook into an `ExcelReader`, and a single polymorphic `load_sheet`/`load_table` surface materializes any sheet or named table to a lazy Arrow PyCapsule (`ExcelSheet`/`ExcelTable`) or an eager `pyarrow.RecordBatch`. The package owner composes `read_excel`, `ExcelReader.load_sheet`, and the PyCapsule export into the FASTEXCEL_DATASET path; it carries `header_row`, `skip_rows`, `use_columns`, `dtypes`, and `dtype_coercion` as call rows, exports zero-copy through `__arrow_c_array__` without a mandatory `pyarrow` dependency, and never re-implements the calamine xlsx/xls/ods/xlsb decode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fastexcel`
- package: `fastexcel`
- import: `fastexcel`
- owner: `data`
- rail: ingestion
- installed: `0.20.2` reflected from the bundled stubs (`_fastexcel.pyi`, `__init__.py`) and module introspection; MIT; abi3 wheel (`cp310-abi3`, stable ABI — one wheel covers cp310 through cp315+); locked unconditionally in `uv.lock` (no marker)
- license/floor: MIT; `requires-python >=3.10`; the calamine engine is bundled (no separate dep); `polars` extra enables `to_polars` (zero-copy via PyCapsule, no pyarrow needed), and the `pyarrow`/`pandas` extras (both pulling `pyarrow>=8`) enable `to_arrow`/`to_arrow_with_errors`/`to_pandas` and `eager=True`
- entry points: no console script; library use is import-only via `read_excel`
- capability: calamine-backed xlsx/xlsm/xlsb/xls/ods reading into Arrow; lazy PyCapsule or eager `pyarrow.RecordBatch` materialization; per-column dtype control and coercion modes; header/skip/row-count windowing with int/list/callable row-skip predicates; Excel-letter, list, and callable column selection; named-table and defined-name access; per-cell parse-error capture; whitespace-tail trimming and whitespace-as-null; zero-copy `to_polars`/`to_pandas` bridges

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader, materialized blocks, metadata, and failures
- rail: ingestion

`read_excel` returns an `ExcelReader`; `load_sheet`/`load_table` return an `ExcelSheet`/`ExcelTable` (lazy) or a `pyarrow.RecordBatch` (eager). `ColumnInfo`/`ColumnInfoNoDtype` describe resolved columns, `DefinedName` describes workbook named ranges, and `CellError`/`CellErrors` carry per-cell parse failures. Every exception derives from `FastExcelError`.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `ExcelReader`                           | reader        | open workbook exposing sheet/table load                  |
|  [02]   | `ExcelSheet`                            | data block    | one sheet as an Arrow PyCapsule with metadata            |
|  [03]   | `ExcelTable`                            | data block    | one named table as an Arrow PyCapsule with metadata      |
|  [04]   | `ColumnInfo`                            | metadata      | resolved column: `name`, `index`, `absolute_index`, `dtype`, `column_name_from`, `dtype_from` |
|  [05]   | `ColumnInfoNoDtype`                     | metadata      | pre-dtype column descriptor (`name`, `index`, `absolute_index`, `column_name_from`) for `use_columns` predicates |
|  [06]   | `DefinedName`                           | metadata      | workbook named range (`name`, `formula`)                 |
|  [07]   | `CellError`                             | metadata      | one cell parse failure (`position`, `offset_position`, `row_offset`, `detail`) |
|  [08]   | `CellErrors`                            | metadata      | collection of `CellError` (`.errors`) from a fallible conversion |
|  [09]   | `DType`                                 | type alias    | `Literal["null","int","float","string","boolean","datetime","date","duration"]` |
|  [10]   | `DTypeMap`                              | type alias    | `dict[str \| int, DType]` per-column dtype override      |
|  [11]   | `ColumnNameFrom`                        | type alias    | `Literal["provided","looked_up","generated"]` name provenance |
|  [12]   | `DTypeFrom`                             | type alias    | `Literal["provided_for_all","provided_by_index","provided_by_name","guessed"]` dtype provenance |
|  [13]   | `SheetVisible`                          | type alias    | `Literal["visible","hidden","veryhidden"]` sheet visibility (public alias, not in `__all__`) |
|  [14]   | `FastExcelError`                        | error         | base failure for every `fastexcel` exception             |
|  [15]   | `CalamineError`                         | error         | calamine engine read failure                             |
|  [16]   | `CalamineCellError`                     | error         | calamine cell-level read failure                         |
|  [17]   | `CannotRetrieveCellDataError`           | error         | cell-data retrieval failure                              |
|  [18]   | `SheetNotFoundError`                    | error         | requested sheet index/name absent                        |
|  [19]   | `ColumnNotFoundError`                   | error         | requested column absent                                  |
|  [20]   | `ArrowError`                            | error         | Arrow conversion/export failure                          |
|  [21]   | `InvalidParametersError`                | error         | inconsistent load parameters                             |
|  [22]   | `UnsupportedColumnTypeCombinationError` | error         | column mixes types coercion cannot reconcile             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open factory
- rail: ingestion

`read_excel` is the single open surface; `source` discriminates between a filesystem path (`str`/`Path`, `~` expanded) and in-memory `bytes`.

| [INDEX] | [SURFACE]    | [CALL_SHAPE]                                              | [CAPABILITY]                                |
| :-----: | :----------- | :-------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `read_excel` | `read_excel(source: Path \| str \| bytes) -> ExcelReader` | open a workbook from a path or bytes buffer |

[ENTRYPOINT_SCOPE]: `ExcelReader` load and inspect
- rail: ingestion

`load_sheet` is the polymorphic materialization surface: `idx_or_name` discriminates sheet-by-index versus sheet-by-name, and `eager` selects a lazy `ExcelSheet` (`False`, PyCapsule) versus an eager `pyarrow.RecordBatch` (`True`, `@overload`-typed on the `eager` literal). `load_sheet_by_name`/`load_sheet_by_idx`/`load_sheet_eager` are fixed-arity Python wrappers over the same kernel. `load_table` materializes a named table; `table_names` and `defined_names` enumerate workbook structure. `header_row=None` means no header row; `schema_sample_rows=None` scans every row for inference; `load_sheet`/`load_sheet_eager`'s `skip_rows` accepts an `int`, an explicit `list[int]`, or a `Callable[[int], bool]` predicate over the data-relative row index, while the `by_name`/`by_idx`/`load_table` rows take only `int`.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                                                                                                                                                                                    | [CAPABILITY]                                  |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `ExcelReader.load_sheet`         | `load_sheet(idx_or_name: int \| str, *, header_row: int \| None=0, column_names: list[str] \| None=None, skip_rows: int \| list[int] \| Callable[[int], bool] \| None=None, n_rows: int \| None=None, schema_sample_rows: int \| None=1000, dtype_coercion="coerce", use_columns=None, dtypes: DType \| DTypeMap \| None=None, eager=False, skip_whitespace_tail_rows=False, whitespace_as_null=False) -> ExcelSheet \| pa.RecordBatch` | materialize a sheet (lazy or eager)           |
|  [02]   | `ExcelReader.load_sheet_eager`   | `load_sheet_eager(idx_or_name: int \| str, *, header_row: int \| None=0, column_names=None, skip_rows: int \| list[int] \| Callable[[int], bool] \| None=None, n_rows=None, schema_sample_rows: int \| None=1000, dtype_coercion="coerce", use_columns: list[str] \| list[int] \| str \| None=None, dtypes=None) -> pa.RecordBatch` | eager sheet load via borrowed `pyarrow` types (no callable `use_columns`) |
|  [03]   | `ExcelReader.load_sheet_by_name` | `load_sheet_by_name(name: str, *, header_row: int \| None=0, column_names=None, skip_rows: int \| None=None, n_rows=None, schema_sample_rows: int \| None=1000, dtype_coercion="coerce", use_columns=None, dtypes=None) -> ExcelSheet`                                                           | lazy sheet load fixed to name                 |
|  [04]   | `ExcelReader.load_sheet_by_idx`  | `load_sheet_by_idx(idx: int, *, header_row: int \| None=0, column_names=None, skip_rows: int \| None=None, n_rows=None, schema_sample_rows: int \| None=1000, dtype_coercion="coerce", use_columns=None, dtypes=None) -> ExcelSheet`                                                             | lazy sheet load fixed to index                |
|  [05]   | `ExcelReader.load_table`         | `load_table(name: str, *, header_row: int \| None=None, column_names=None, skip_rows: int \| None=None, n_rows=None, schema_sample_rows: int \| None=1000, dtype_coercion="coerce", use_columns=None, dtypes: DType \| DTypeMap \| None=None, eager=False, skip_whitespace_tail_rows=False, whitespace_as_null=False) -> ExcelTable \| pa.RecordBatch` | materialize a named table (lazy or eager)     |
|  [06]   | `ExcelReader.table_names`        | `table_names(sheet_name: str \| None = None) -> list[str]`                                                                                                                                                                                                                                      | list table names (optionally one sheet)       |
|  [07]   | `ExcelReader.defined_names`      | `defined_names() -> list[DefinedName]`                                                                                                                                                                                                                                                          | list workbook defined names                   |
|  [08]   | `ExcelReader.sheet_names`        | property -> `list[str]`                                                                                                                                                                                                                                                                         | the list of sheet names                       |

[ENTRYPOINT_SCOPE]: `ExcelSheet` materialize and inspect
- rail: ingestion

A lazy `ExcelSheet` exports through the Arrow PyCapsule interface (`__arrow_c_schema__`/`__arrow_c_array__`) so `to_arrow`/`to_polars`/`to_pandas` and any Arrow consumer read it zero-copy; `to_arrow_with_errors` returns the batch plus a `CellErrors` (or `None` when no cell failed). Read properties carry the dimensions and column metadata the receipt records. `ExcelTable` shares the materialization surface (`to_arrow`/`to_polars`/`to_pandas`, the PyCapsule dunders, `name`/`width`/`height`/`total_height`/`selected_columns`/`available_columns`/`specified_dtypes`) and adds `sheet_name` and `offset`, but does NOT expose `to_arrow_with_errors` (no per-cell error capture) or `visible` (table visibility is undefined). `to_polars` requires only the `polars` extra; `to_arrow`/`to_arrow_with_errors`/`to_pandas` require the `pyarrow` extra (`to_pandas` routes through `to_arrow().to_pandas()`).

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
- window axis: `header_row` (`int` index, or `None` for no header), `column_names`, `skip_rows` (an `int`, an explicit `list[int]` of data-relative indices, or a `Callable[[int], bool]` predicate on `load_sheet`/`load_sheet_eager`), `n_rows`, `skip_whitespace_tail_rows`, and `whitespace_as_null` are windowing call rows; row windowing is a parameter, never a pre-filtered copy of the sheet.
- dtype axis: `dtypes` (a `DType` for all columns or a `DTypeMap` keyed by index/name) overrides per-column inference; `schema_sample_rows` bounds inference (`None` scans every row); `dtype_coercion` selects `coerce` versus `strict` failure on mixed columns — typing is a call row, never a downstream cast pass.
- selection axis: `use_columns` selects by name/index list, Excel-letter range string (`"A:E"`, `"A,C,E:F"`, open-ended `"B:"`, from-beginning `":C"`, and `":C,E:"` except-patterns), or `Callable[[ColumnInfoNoDtype], bool]` predicate; column pruning happens at decode, never as a post-load projection (the eager `load_sheet_eager` row narrows `use_columns` to list/string only, no callable).
- export axis: lazy `ExcelSheet`/`ExcelTable` export through `__arrow_c_schema__`/`__arrow_c_array__`; `to_polars` constructs `pl.DataFrame(self)` zero-copy from the PyCapsule and needs only the `polars` extra (no pyarrow); `to_arrow`/`to_pandas` and `eager=True` require the `pyarrow` extra (`to_pandas` is `to_arrow().to_pandas()`); `to_arrow_with_errors` pairs the batch with a `CellErrors` receipt and is `ExcelSheet`-only — `ExcelTable` has no error-capture export.
- error axis: every failure derives from `FastExcelError`; `CalamineError`/`CalamineCellError`/`CannotRetrieveCellDataError`/`ArrowError`/`SheetNotFoundError`/`ColumnNotFoundError`/`InvalidParametersError`/`UnsupportedColumnTypeCombinationError` are typed rows mapped to the data error rail; per-cell parse failures surface as `CellError` (`position`, `offset_position`, `row_offset`, `detail`) inside `CellErrors.errors`, never as silent nulls.
- evidence: each load captures sheet/table name, width, height, total height, selected `ColumnInfo` (`name`, `index`, `absolute_index`, `dtype`, `column_name_from`, `dtype_from`), specified dtypes, sheet visibility (`ExcelSheet` only), and any `CellErrors` as an ingestion receipt.
- stacking: the lazy `ExcelSheet`/`ExcelTable` is a pure PyCapsule producer, so it flows zero-copy into the rest of the data rail without a forced Arrow dependency — `pl.DataFrame(sheet)`, `pa.record_batch(sheet)`, a `duckdb` cursor's Arrow ingestion, or a `ducklake` `INSERT` all consume the same capsule; the calamine decode is the single ingress edge and the downstream frame/dataset/lakehouse owner picks the materialization.
- boundary: fastexcel owns calamine xlsx/xlsm/xlsb/xls/ods decode and the Arrow export edge; the Arrow batch feeds the dataset and frame owners directly; spreadsheet authoring, formula evaluation, and styling stay outside this package.

[RAIL_LAW]:
- Package: `fastexcel`
- Owns: calamine-backed Excel reading into Arrow, per-column dtype control and coercion, header/skip/row windowing (with int/list/callable row-skip predicates), Excel-letter/list/callable column selection, whitespace-tail trimming and whitespace-as-null, named-table and defined-name access, `ExcelSheet` per-cell parse-error capture, and zero-copy PyCapsule export (polars without pyarrow; pyarrow/pandas via the extra)
- Accept: Excel workbook ingestion into Arrow record batches feeding the dataset, frame, and persistence owners
- Reject: wrapper-renames of `read_excel`/`load_sheet`; a hand-rolled xlsx/ods decoder; a parallel reader type per source kind or per eager/lazy mode; a per-sheet column-name family such as `load_by_a`/`load_by_b`; a forced `pyarrow` dependency where the polars PyCapsule export needs only the `polars` extra; a post-load row filter where the `skip_rows` callable predicate prunes at decode; silent dropping of unparseable cells the `ExcelSheet.to_arrow_with_errors` `CellErrors` receipt must record
