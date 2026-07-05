# [PY_DATA_API_PANDAS]

`pandas` provides labeled, axis-indexed tabular data through `DataFrame` (2-D) and `Series` (1-D) values backed by typed `Index` objects, with first-class temporal types (`Timestamp`, `Timedelta`, `Period`, `Interval`) and dtype objects (`CategoricalDtype`, `StringDtype`, `ArrowDtype`). Top-level `read_*` functions ingest CSV, Parquet, SQL, Excel, JSON, and more; reshaping and combination functions (`concat`, `merge`, `pivot_table`, `melt`, `crosstab`, `get_dummies`) operate across frames; and `groupby`/`rolling`/`resample` drive split-apply-combine aggregation. `to_*` methods and `ArrowDtype` provide Arrow and storage interop at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandas`
- package: `pandas`
- owner: `data`
- module: `pandas`
- license: BSD-3-Clause
- rail: labeled tabular

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frames, indexes, and dtypes
- rail: labeled tabular

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [ROLE]                               |
| :-----: | :----------------- | :-------------- | :----------------------------------- |
|  [01]   | `DataFrame`        | tabular frame   | labeled 2-D table over an `Index`    |
|  [02]   | `Series`           | labeled column  | labeled 1-D column                   |
|  [03]   | `Index`            | axis index      | immutable labeled axis               |
|  [04]   | `MultiIndex`       | axis index      | hierarchical multi-level index       |
|  [05]   | `DatetimeIndex`    | axis index      | datetime-typed index                 |
|  [06]   | `RangeIndex`       | axis index      | memory-efficient integer range index |
|  [07]   | `CategoricalIndex` | axis index      | categorical-typed index              |
|  [08]   | `Categorical`      | array dtype     | categorical extension array          |
|  [09]   | `CategoricalDtype` | dtype           | category set and ordering            |
|  [10]   | `StringDtype`      | dtype           | dedicated string extension dtype     |
|  [11]   | `ArrowDtype`       | dtype           | Arrow-backed column dtype            |
|  [12]   | `Grouper`          | aggregation key | groupby key specification            |

[PUBLIC_TYPE_SCOPE]: scalar and offset types
- rail: labeled tabular

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [ROLE]                             |
| :-----: | :--------------- | :------------ | :--------------------------------- |
|  [01]   | `Timestamp`      | temporal      | single point in time               |
|  [02]   | `Timedelta`      | temporal      | duration scalar                    |
|  [03]   | `Period`         | temporal      | calendar-span scalar               |
|  [04]   | `Interval`       | interval      | left/right-bounded interval scalar |
|  [05]   | `DateOffset`     | temporal      | calendar-aware offset              |
|  [06]   | `NamedAgg`       | aggregation   | named groupby aggregation spec     |
|  [07]   | `IntervalIndex`  | axis index    | interval-typed index               |
|  [08]   | `PeriodIndex`    | axis index    | period-typed index                 |
|  [09]   | `TimedeltaIndex` | axis index    | timedelta-typed index              |
|  [10]   | `ExcelWriter`    | IO writer     | multi-sheet Excel writer context   |
|  [11]   | `HDFStore`       | IO store      | HDF5 key-value table store         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and IO
- rail: labeled tabular

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `DataFrame(data, index, columns, dtype)`     | construct      | build frame from dict/array/records |
|  [02]   | `DataFrame.from_dict / from_records`         | construct      | build from dicts or row records     |
|  [03]   | `read_csv` / `read_fwf` / `read_table`       | text IO        | delimited and fixed-width readers   |
|  [04]   | `read_parquet` / `read_feather` / `read_orc` | columnar IO    | Arrow-backed columnar readers       |
|  [05]   | `read_sql / read_sql_query / read_sql_table` | SQL IO         | read from a SQL connection          |
|  [06]   | `read_excel` / `read_html` / `read_xml`      | structured IO  | spreadsheet and markup readers      |
|  [07]   | `read_json` / `json_normalize`               | JSON IO        | read and flatten JSON               |
|  [08]   | `read_iceberg` / `read_hdf` / `read_stata`   | store IO       | table-store and statistical formats |
|  [09]   | `to_parquet / to_csv / to_feather`           | columnar IO    | write frame to storage              |
|  [10]   | `to_dict / to_records / to_numpy`            | interop        | export to Python/NumPy structures   |
|  [11]   | `to_datetime / to_timedelta / to_numeric`    | coerce         | parse and coerce dtypes             |
|  [12]   | `date_range / period_range / interval_range` | range          | generate labeled index ranges       |

[ENTRYPOINT_SCOPE]: DataFrame transformation and selection
- rail: labeled tabular

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `loc` / `iloc` / `at` / `iat` / `xs`            | selection      | label and position indexing            |
|  [02]   | `query(expr)` / `filter(items, like, regex)`    | selection      | expression and label filtering         |
|  [03]   | `assign(**kwargs)` / `pipe(func)`               | transform      | add columns, chain functions           |
|  [04]   | `apply / map / agg / transform`                 | transform      | element/column-wise function dispatch  |
|  [05]   | `astype / convert_dtypes`                       | transform      | cast and infer dtypes                  |
|  [06]   | `groupby(by, level, dropna, observed)`          | aggregation    | split-apply-combine grouping           |
|  [07]   | `rolling / expanding / ewm / resample`          | aggregation    | window and time-resample reductions    |
|  [08]   | `merge / join / concat`                         | combine        | relational joins and concatenation     |
|  [09]   | `merge_asof / merge_ordered`                    | combine        | as-of and ordered merges               |
|  [10]   | `pivot / pivot_table / melt / crosstab`         | reshape        | wide/long reshape and cross-tabulation |
|  [11]   | `stack / unstack / explode / transpose`         | reshape        | level reshape, list expand, transpose  |
|  [12]   | `get_dummies / from_dummies / factorize`        | encode         | one-hot and integer encoding           |
|  [13]   | `dropna / fillna / ffill / bfill / interpolate` | missing        | handle missing values                  |
|  [14]   | `drop_duplicates / duplicated / replace`        | clean          | deduplicate and replace                |
|  [15]   | `sort_values / sort_index / nlargest / rank`    | order          | sort and rank rows                     |
|  [16]   | `set_index / reset_index / reindex / rename`    | index          | reshape labels and axes                |
|  [17]   | `describe / corr / cov / value_counts`          | stats          | summary statistics                     |
|  [18]   | `Series.str` / `Series.dt` / `Series.cat`       | accessor       | string, datetime, categorical methods  |

## [04]-[IMPLEMENTATION_LAW]

[LABELED_TOPOLOGY]:
- `DataFrame` aligns columns and rows by `Index` labels; arithmetic and `merge`/`concat` align on labels, introducing `NaN` where labels do not match
- dtypes mix NumPy-backed and extension dtypes; `ArrowDtype` and `StringDtype` give Arrow/string-native columns, `CategoricalDtype` carries category set and ordering
- temporal scalars (`Timestamp`, `Timedelta`, `Period`, `Interval`) and their index types power time-aware slicing, `resample`, and offset arithmetic with `DateOffset`
- `groupby(...).agg(...)` accepts `NamedAgg` for named outputs; `rolling`/`expanding`/`ewm`/`resample` return window objects that reduce on aggregation calls
- `loc`/`at` index by label and `iloc`/`iat` by position; `query` and `eval` evaluate string expressions over columns
- `read_*`/`to_*` cover CSV, Parquet, Feather, ORC, SQL, Excel, HTML, XML, JSON, HDF5, Iceberg, and Stata

[LOCAL_ADMISSION]:
- Prefer Arrow-backed columns (`ArrowDtype`, `read_parquet`/`to_parquet`) and `to_arrow`/`pyarrow` interop where columnar performance and zero-copy exchange matter; reach for polars on hot lazy paths. The Arrow-backed frame hands off zero-copy through `pyarrow`/`narwhals` to `polars`, and `pandera.pandas`/`pointblank` validate the same in-memory frame without a re-materialization — pandas is the boundary frame, not the compute hot path.
- Use vectorized methods, `assign`, `pipe`, and `agg`/`transform` over Python loops; pass `NamedAgg` for explicit aggregation output names.
- Index by label with `loc`/`at` and by position with `iloc`/`iat`; never chain `[]` selection for assignment.
- Reshape with `pivot_table`/`melt`/`stack`/`unstack` and combine with `merge`/`concat`/`merge_asof`; align labels deliberately to avoid silent `NaN` introduction.
- Gate a `read_*` boundary frame through a `pandera.pandas` `DataFrameSchema`/`DataFrameModel` before downstream consumption rather than asserting dtypes by hand, and treat `to_iceberg`/`read_iceberg` (experimental) and `read_orc`/`to_orc` as the lakehouse interchange edge alongside `deltalake`/`pyiceberg`.

[RAIL_LAW]:
- Package: `pandas`
- Owns: labeled in-memory tabular data, label-aligned arithmetic, split-apply-combine aggregation, and broad file/SQL IO
- Accept: dicts, NumPy arrays, records, Arrow tables, and file/SQL sources via `read_*`
- Reject: row-wise Python iteration for transforms, chained-assignment indexing, unaligned label arithmetic, reimplementing IO that `read_*` already covers
