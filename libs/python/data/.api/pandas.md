# [PY_DATA_API_PANDAS]

`pandas` owns the boundary frame: the labeled, axis-indexed `DataFrame` and `Series` value, backed by typed `Index` and dtype objects, that every non-native source lowers into and every external consumer receives. Its `read_*` ingest and `to_*` egress span text, SQL, columnar, and lakehouse formats, and `groupby`/`rolling`/`resample` drive split-apply-combine reduction. Label alignment is the topology every operation folds through; the frame is the interchange edge, never the compute hot path, and Arrow-backed columns hand off zero-copy to the columnar rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandas`
- package: `pandas` (BSD-3-Clause)
- owner: `data`
- module: `pandas`
- rail: labeled tabular

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frames, indexes, and dtypes

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                         |
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

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                       |
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

| [INDEX] | [SURFACE]                                    | [SHAPE]       | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `DataFrame(data, index, columns, dtype)`     | construct     | build frame from dict/array/records |
|  [02]   | `DataFrame.from_dict / from_records`         | construct     | build from dicts or row records     |
|  [03]   | `read_csv` / `read_fwf` / `read_table`       | text IO       | delimited and fixed-width readers   |
|  [04]   | `read_parquet` / `read_feather` / `read_orc` | columnar IO   | Arrow-backed columnar readers       |
|  [05]   | `read_sql / read_sql_query / read_sql_table` | SQL IO        | read from a SQL connection          |
|  [06]   | `read_excel` / `read_html` / `read_xml`      | structured IO | spreadsheet and markup readers      |
|  [07]   | `read_json` / `json_normalize`               | JSON IO       | read and flatten JSON               |
|  [08]   | `read_iceberg` / `read_hdf` / `read_stata`   | store IO      | table-store and statistical formats |
|  [09]   | `to_parquet / to_csv / to_feather`           | columnar IO   | write frame to storage              |
|  [10]   | `to_dict / to_records / to_numpy`            | interop       | export to Python/NumPy structures   |
|  [11]   | `to_datetime / to_timedelta / to_numeric`    | coerce        | parse and coerce dtypes             |
|  [12]   | `date_range / period_range / interval_range` | range         | generate labeled index ranges       |

[ENTRYPOINT_SCOPE]: DataFrame transformation and selection

| [INDEX] | [SURFACE]                                       | [SHAPE]     | [CAPABILITY]                           |
| :-----: | :---------------------------------------------- | :---------- | :------------------------------------- |
|  [01]   | `loc` / `iloc` / `at` / `iat` / `xs`            | selection   | label and position indexing            |
|  [02]   | `query(expr)` / `filter(items, like, regex)`    | selection   | expression and label filtering         |
|  [03]   | `assign(**kwargs)` / `pipe(func)`               | transform   | add columns, chain functions           |
|  [04]   | `apply / map / agg / transform`                 | transform   | element/column-wise function dispatch  |
|  [05]   | `astype / convert_dtypes`                       | transform   | cast and infer dtypes                  |
|  [06]   | `groupby(by, level, dropna, observed)`          | aggregation | split-apply-combine grouping           |
|  [07]   | `rolling / expanding / ewm / resample`          | aggregation | window and time-resample reductions    |
|  [08]   | `merge / join / concat`                         | combine     | relational joins and concatenation     |
|  [09]   | `merge_asof / merge_ordered`                    | combine     | as-of and ordered merges               |
|  [10]   | `pivot / pivot_table / melt / crosstab`         | reshape     | wide/long reshape and cross-tabulation |
|  [11]   | `stack / unstack / explode / transpose`         | reshape     | level reshape, list expand, transpose  |
|  [12]   | `get_dummies / from_dummies / factorize`        | encode      | one-hot and integer encoding           |
|  [13]   | `dropna / fillna / ffill / bfill / interpolate` | missing     | handle missing values                  |
|  [14]   | `drop_duplicates / duplicated / replace`        | clean       | deduplicate and replace                |
|  [15]   | `sort_values / sort_index / nlargest / rank`    | order       | sort and rank rows                     |
|  [16]   | `set_index / reset_index / reindex / rename`    | index       | reshape labels and axes                |
|  [17]   | `describe / corr / cov / value_counts`          | stats       | summary statistics                     |
|  [18]   | `Series.str` / `Series.dt` / `Series.cat`       | accessor    | string, datetime, categorical methods  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DataFrame` aligns rows and columns by `Index` label; arithmetic, `merge`, and `concat` align on labels, minting `NaN` where labels do not match.
- dtypes mix NumPy-backed and extension families; `ArrowDtype` and `StringDtype` carry Arrow- and string-native columns, `CategoricalDtype` carries category set and ordering.
- Temporal scalars and their index types power time-aware slicing, `resample`, and `DateOffset` arithmetic.
- `groupby(...).agg(...)` takes `NamedAgg` for named outputs; `rolling`/`expanding`/`ewm`/`resample` return window objects that reduce only on the aggregation call.
- `loc`/`at` index by label and `iloc`/`iat` by position; `query`/`eval` evaluate string expressions over columns.

[STACKING]:
- `narwhals`(`.api/narwhals.md`): `narwhals.from_native(df)` wraps the frame as a backend-agnostic frame, the translation seam behind interop egress.
- `polars`(`.api/polars.md`): `polars.from_pandas(df)` lifts the frame into the columnar engine, an `ArrowDtype`-backed frame crossing zero-copy through Arrow.
- `pyarrow`(`.api/pyarrow.md`): `pyarrow.Table.from_pandas(df)`, the `DataFrame.__dataframe__` interchange protocol, and `DataFrame.__arrow_c_stream__` bridge to Arrow; `ArrowDtype` columns are already Arrow-native.
- `pandera`(`.api/pandera.md`): `pandera.pandas.DataFrameSchema`/`DataFrameModel` validate the in-memory frame with no re-materialization; `pointblank`(`.api/pointblank.md`) `Validate` grades the same frame against quality thresholds.
- lakehouse edge: `read_iceberg`/`DataFrame.to_iceberg` and `read_orc`/`DataFrame.to_orc` cross the frame to the `deltalake`/`pyiceberg` interchange.

[LOCAL_ADMISSION]:
- `ArrowDtype` columns with `read_parquet`/`to_parquet` win the columnar and zero-copy-exchange path; hot lazy compute belongs to polars.
- Express transforms as vectorized methods, `assign`, `pipe`, and `agg`/`transform` over Python row loops; pass `NamedAgg` for explicit aggregation-output names.
- Index by label with `loc`/`at` and by position with `iloc`/`iat`; a `read_*` boundary frame gates through a `pandera.pandas` schema before downstream consumption rather than hand-asserted dtypes.
- Reshape with `pivot_table`/`melt`/`stack`/`unstack` and combine with `merge`/`concat`/`merge_asof`, aligning labels deliberately so no silent `NaN` enters.

[RAIL_LAW]:
- Package: `pandas`
- Owns: labeled in-memory tabular data, label-aligned arithmetic, split-apply-combine aggregation, and broad file/SQL/lakehouse IO
- Accept: dicts, NumPy arrays, records, Arrow tables, and file/SQL sources via `read_*`
- Reject: row-wise Python iteration for transforms, chained-`[]` assignment, unaligned label arithmetic, and reimplementing IO `read_*` already covers
