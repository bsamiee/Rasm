# [PY_DATA_API_FASTEXCEL]

`fastexcel` binds the Rust `calamine` decoder to an Arrow boundary for the data ingestion rail. One `read_excel` factory opens a workbook into an `ExcelReader`; one polymorphic `load_sheet`/`load_table` surface materializes any sheet or named table to a lazy Arrow PyCapsule (`ExcelSheet`/`ExcelTable`) or an eager `pyarrow.RecordBatch`. Windowing, dtype, and column-selection ride as call rows, and the lazy block exports zero-copy through `__arrow_c_array__` with no mandatory `pyarrow` dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fastexcel`
- package: `fastexcel` (MIT, ToucanToco)
- module: `fastexcel`
- owner: `data`
- rail: ingestion

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader, materialized blocks, and metadata

`read_excel` returns an `ExcelReader`; `load_sheet`/`load_table` return a lazy `ExcelSheet`/`ExcelTable` or an eager `pyarrow.RecordBatch`. `ColumnInfo` carries `name`, `index`, `absolute_index`, `dtype`, `column_name_from`, `dtype_from` as the ingestion receipt; `ColumnInfoNoDtype` drops `dtype`/`dtype_from` for pre-dtype predicates. `DefinedName` carries `name`/`formula`; `CellError` carries `position`, `offset_position`, `row_offset`, `detail`, collected in `CellErrors.errors`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------ | :------------ | :--------------------------------------- |
|  [01]   | `ExcelReader`       | reader        | open workbook exposing sheet/table load  |
|  [02]   | `ExcelSheet`        | data block    | one sheet as an Arrow PyCapsule          |
|  [03]   | `ExcelTable`        | data block    | one named table as an Arrow PyCapsule    |
|  [04]   | `ColumnInfo`        | metadata      | resolved-column receipt descriptor       |
|  [05]   | `ColumnInfoNoDtype` | metadata      | pre-dtype descriptor for `use_columns`   |
|  [06]   | `DefinedName`       | metadata      | workbook named range                     |
|  [07]   | `CellError`         | metadata      | one cell parse failure                   |
|  [08]   | `CellErrors`        | metadata      | `CellError` collection from a conversion |

[PUBLIC_TYPE_SCOPE]: dtype, name-provenance, and visibility aliases

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                                    |
| :-----: | :--------------- | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | `DType`          | type alias    | `Literal["null","int","float","string","boolean","datetime","date","duration"]`                 |
|  [02]   | `DTypeMap`       | type alias    | `dict[str \| int, DType]` per-column override                                                   |
|  [03]   | `ColumnNameFrom` | type alias    | `Literal["provided","looked_up","generated"]` name provenance                                   |
|  [04]   | `DTypeFrom`      | type alias    | `Literal["provided_for_all","provided_by_index","provided_by_name","guessed"]` dtype provenance |
|  [05]   | `SheetVisible`   | type alias    | `Literal["visible","hidden","veryhidden"]` visibility, not in `__all__`                         |

[PUBLIC_TYPE_SCOPE]: failure rail — every row derives from `FastExcelError` and maps to the data error rail

| [INDEX] | [SYMBOL]                                | [CAPABILITY]                                 |
| :-----: | :-------------------------------------- | :------------------------------------------- |
|  [01]   | `FastExcelError`                        | base failure for every `fastexcel` exception |
|  [02]   | `CalamineError`                         | calamine engine read failure                 |
|  [03]   | `CalamineCellError`                     | calamine cell-level read failure             |
|  [04]   | `CannotRetrieveCellDataError`           | cell-data retrieval failure                  |
|  [05]   | `SheetNotFoundError`                    | requested sheet index/name absent            |
|  [06]   | `ColumnNotFoundError`                   | requested column absent                      |
|  [07]   | `ArrowError`                            | Arrow conversion/export failure              |
|  [08]   | `InvalidParametersError`                | inconsistent load parameters                 |
|  [09]   | `UnsupportedColumnTypeCombinationError` | column mixes types coercion cannot reconcile |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open factory

`read_excel` is the sole open surface; `source` discriminates a filesystem path (`str`/`Path`, `~`-expanded) from in-memory `bytes`.

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                         |
| :-----: | :---------------------------------- | :------ | :----------------------------------- |
|  [01]   | `read_excel(source) -> ExcelReader` | factory | open a workbook from a path or bytes |

[ENTRYPOINT_SCOPE]: `ExcelReader` load and inspect

`load_sheet` folds sheet materialization: `idx_or_name` discriminates index from name, `eager` selects a lazy `ExcelSheet` from an eager `pyarrow.RecordBatch` (`@overload`-typed on the `eager` literal). `load_sheet_by_name`, `load_sheet_by_idx`, and `load_sheet_eager` are fixed-arity wrappers over the one kernel; `load_table` shares the policy axis on named tables.

- All load rows carry: `header_row`, `column_names`, `skip_rows`, `n_rows`, `schema_sample_rows`, `dtype_coercion`, `use_columns`, `dtypes`; `load_sheet`/`load_table` add `eager`, `skip_whitespace_tail_rows`, `whitespace_as_null`.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `load_sheet(idx_or_name, *, ...) -> ExcelSheet \| RecordBatch` | instance | materialize a sheet, lazy or eager    |
|  [02]   | `load_sheet_eager(idx_or_name, *, ...) -> RecordBatch`         | instance | eager load; no callable `use_columns` |
|  [03]   | `load_sheet_by_name(name, *, ...) -> ExcelSheet`               | instance | lazy load fixed to name               |
|  [04]   | `load_sheet_by_idx(idx, *, ...) -> ExcelSheet`                 | instance | lazy load fixed to index              |
|  [05]   | `load_table(name, *, ...) -> ExcelTable \| RecordBatch`        | instance | materialize a named table             |
|  [06]   | `table_names(sheet_name=None) -> list[str]`                    | instance | table names, optionally one sheet     |
|  [07]   | `defined_names() -> list[DefinedName]`                         | instance | workbook defined names                |
|  [08]   | `sheet_names -> list[str]`                                     | property | the sheet-name list                   |

[ENTRYPOINT_SCOPE]: `ExcelSheet` and `ExcelTable` materialize and inspect

A lazy `ExcelSheet`/`ExcelTable` exports through the Arrow PyCapsule dunders, so `to_arrow`/`to_polars`/`to_pandas` and any Arrow consumer read it zero-copy. `ExcelTable` shares the surface with `sheet_name`/`offset` and exposes no `to_arrow_with_errors` and no `visible`. Every row is an `ExcelSheet` member unless [CAPABILITY] names `ExcelTable`.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `to_arrow() -> RecordBatch`                                    | instance | materialize to a `pyarrow.RecordBatch`   |
|  [02]   | `to_arrow_with_errors() -> (RecordBatch, CellErrors \| None)`  | instance | materialize plus per-cell error capture  |
|  [03]   | `to_polars() -> pl.DataFrame`                                  | instance | zero-copy Polars frame via PyCapsule     |
|  [04]   | `to_pandas() -> pd.DataFrame`                                  | instance | Pandas frame via Arrow conversion        |
|  [05]   | `available_columns() -> list[ColumnInfo]`                      | instance | columns available before `use_columns`   |
|  [06]   | `__arrow_c_schema__() -> object`                               | instance | export the `ArrowSchema` PyCapsule       |
|  [07]   | `__arrow_c_array__(requested_schema=None) -> (object, object)` | instance | export the schema/array PyCapsule pair   |
|  [08]   | `name -> str`                                                  | property | the sheet name                           |
|  [09]   | `width -> int`                                                 | property | the sheet width                          |
|  [10]   | `height -> int`                                                | property | height with `skip_rows`/`n_rows` applied |
|  [11]   | `total_height -> int`                                          | property | the sheet total height                   |
|  [12]   | `selected_columns -> list[ColumnInfo]`                         | property | columns retained after selection         |
|  [13]   | `specified_dtypes -> DTypeMap \| None`                         | property | the dtypes specified for the sheet       |
|  [14]   | `visible -> SheetVisible`                                      | property | the sheet visibility                     |
|  [15]   | `sheet_name -> str`                                            | property | `ExcelTable` owning-sheet name           |
|  [16]   | `offset -> int`                                                | property | `ExcelTable` data-start offset           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `read_excel` owns workbook opening; `source` discriminates path from `bytes` in the request value, never a per-source factory type, and `~` expands once at the boundary for `str`/`Path`.
- `load_sheet` owns sheet materialization; `idx_or_name` discriminates index from name and `eager` discriminates lazy `ExcelSheet` from eager `pyarrow.RecordBatch`; the `by_name`/`by_idx`/`eager` rows are fixed-arity convenience over the one kernel, never parallel readers, and `load_table` is the named-table row on the identical policy axis.
- window: `header_row` (`int`, or `None` for no header), `column_names`, `skip_rows`, `n_rows`, `skip_whitespace_tail_rows`, and `whitespace_as_null` are call rows; `skip_rows` widens to `int | list[int] | Callable[[int], bool]` on `load_sheet`/`load_sheet_eager` (data-relative index) and stays `int` on the `by_name`/`by_idx`/`load_table` rows; windowing is a parameter, never a pre-filtered sheet copy.
- dtype: `dtypes` (one `DType` for all columns or a `DTypeMap` keyed by index/name) overrides inference; `schema_sample_rows` bounds inference (`None` scans every row); `dtype_coercion` selects `coerce` from `strict` on mixed columns, and typing is a call row, never a downstream cast.
- selection: `use_columns` takes a name/index list, an Excel-letter range string (`"A:E"`, `"A,C,E:F"`, open `"B:"`, from-start `":C"`, except `":C,E:"`), or a `Callable[[ColumnInfoNoDtype], bool]`; pruning happens at decode, never as a post-load projection, and the eager `load_sheet_eager` row drops the callable.
- export: lazy `ExcelSheet`/`ExcelTable` export through `__arrow_c_schema__`/`__arrow_c_array__`; `to_polars` builds `pl.DataFrame(self)` zero-copy on the `polars` extra alone; `to_arrow`/`to_pandas` and `eager=True` require the `pyarrow` extra (`to_pandas` is `to_arrow().to_pandas()`); `to_arrow_with_errors` pairs the batch with a `CellErrors` receipt and is `ExcelSheet`-only.
- error: every failure derives from `FastExcelError`, and per-cell parse failures surface as `CellError` inside `CellErrors.errors`, never as silent nulls.
- evidence: each load records sheet/table name, width, height, total height, selected `ColumnInfo`, specified dtypes, sheet visibility (`ExcelSheet` only), and any `CellErrors` as the ingestion receipt.

[STACKING]:
- `polars`(`polars.md`): the lazy sheet's PyCapsule lands in the eager columnar owner through `pl.DataFrame(sheet)` zero-copy on the `polars` extra alone.
- `pyarrow`(`pyarrow.md`): `pa.record_batch(sheet)` or `eager=True` materializes the same capsule into a `RecordBatch` for the Arrow-native rail.
- `narwhals`(`narwhals.md`): the PyCapsule sheet is a native producer the backend-agnostic interop carrier wraps with no pyarrow floor.
- `duckdb`(`duckdb.md`): the request-scoped `DuckDbSession` scan ingests the Arrow capsule directly for relational egress.
- within-lib: calamine decode is the single ingress edge, and the `data/tabular` interop and columnar owners pick the downstream frame/dataset materialization off the one capsule, never re-decoding.

[LOCAL_ADMISSION]:
- import `fastexcel` at boundary scope only; module-level import is banned by the manifest import policy.
- open through `read_excel(source)` and materialize through `load_sheet`/`load_table` with windowing, dtype, and selection as call args, never a per-shape wrapper.
- calamine owns xlsx/xlsm/xlsb/xls/ods decode and the Arrow export edge; authoring, formula evaluation, and styling stay outside the package.

[RAIL_LAW]:
- Package: `fastexcel`
- Owns: calamine-backed Excel decode into Arrow, per-column dtype control and coercion, header/skip/row windowing with int/list/callable skip predicates, Excel-letter/list/callable column selection, whitespace-tail trimming and whitespace-as-null, named-table and defined-name access, `ExcelSheet` per-cell parse-error capture, and zero-copy PyCapsule export
- Accept: Excel workbook ingestion into Arrow record batches feeding the dataset, frame, and persistence owners
- Reject: a wrapper-rename of `read_excel`/`load_sheet`; a hand-rolled xlsx/ods decoder; a parallel reader type per source kind or eager/lazy mode; a per-sheet column-name family; a forced `pyarrow` dependency where the polars PyCapsule export needs only the `polars` extra; a post-load row filter where the `skip_rows` predicate prunes at decode; silent dropping of unparseable cells the `to_arrow_with_errors` `CellErrors` receipt records
