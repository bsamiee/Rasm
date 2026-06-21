# [PY_ARTIFACTS_API_VEGAFUSION]

`vegafusion` supplies the Rust-backed (DataFusion-embedded) server-side Vega transform engine for the artifacts charts rail: a module-level `runtime` (`VegaFusionRuntime`) whose `pre_transform_*` family executes a Vega spec's data transforms before render, a `ChartState` that maintains the interactive server/client transform split and applies signal/dataset updates, and a `transformer` submodule that converts any narwhals-compatible dataframe to Arrow IPC `bytes` for the inline-dataset path. The package owner composes `runtime.pre_transform_spec`, `pre_transform_datasets`/`pre_transform_extract`, and `new_chart_state` into the chart `EXPORT` path; it never re-implements the Vega transform pipeline the embedded DataFusion engine already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vegafusion`
- package: `vegafusion`
- import: `vegafusion`
- owner: `artifacts`
- rail: charts
- license: BSD-3-Clause (PyO3 + embedded Apache DataFusion / Arrow query engine)
- asset: runtime library (Rust extension); depends on `arro3-core` (Arrow), `narwhals>=1.42` (dataframe interchange), `packaging`. Default extracted-table format is `arro3`, not `pyarrow`
- installed: `2.0.3` reflected via `import vegafusion` on python 3.13 (manifest band `python_version<'3.15'`; no cp315 wheel — the chart owner runs the transform arm on the gated subprocess lane and imports `vegafusion` at module scope only inside that worker)
- entry points: none (library only)
- capability: server-side Vega data-transform pre-evaluation, transformed-dataset extraction, spec/data splitting, interactive chart-state maintenance over inline datasets, gRPC runtime connection, multi-threaded worker pool with cache/memory limits, Arrow IPC dataset feeding via narwhals interchange, timezone configuration, and column-usage analysis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime and chart-state roots
- rail: charts

The module `__all__` exports exactly `runtime` (singleton `VegaFusionRuntime`), `set_local_tz`, `get_local_tz`, and `get_column_usage`. The only importable public class is `vegafusion.runtime.ChartState` (the native core exports `PyChartState`/`PyVegaFusionRuntime`); `runtime.new_chart_state` returns it, reached via the runtime, never a top-level import. `CommPlan`, `VariableUpdate`, and `PreTransformWarning` exist only as forward-ref string annotations on the method signatures — they are return/element *shapes*, not importable type objects, so the owner types against the structural shape (a warning is a `(message, ...)`-bearing record), not an imported class. The `transformer` submodule converts dataframes to Arrow IPC `bytes`/`pa.Table` for the inline-dataset path.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [RAIL]                                                                 |
| :-----: | :------------ | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `runtime`     | runtime singleton | `VegaFusionRuntime` transform-execution owner (worker pool)           |
|  [02]   | `ChartState`  | interactive state | importable `vegafusion.runtime.ChartState`; `new_chart_state` return  |
|  [03]   | `CommPlan` (annotation) | watch plan | `get_comm_plan`/`get_watch_plan` return shape; signal/dataset deps     |
|  [04]   | `VariableUpdate` (annotation) | update record | `ChartState.update` input/output element shape               |
|  [05]   | `PreTransformWarning` (annotation) | warning | element of every `pre_transform_*` warnings list                 |
|  [06]   | `transformer` | submodule         | dataframe-to-Arrow-IPC conversion family                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `runtime` transform execution
- rail: charts

`spec` accepts a `dict` or JSON `str`. The transform rows share `local_tz`, `default_input_tz`, and `inline_datasets` (a `dict[str, DataFrameLike]` of named tables resolved against `data.name`/`data.url` references). `pre_transform_spec`/`pre_transform_extract` carry `preserve_interactivity` + `keep_signals`/`keep_datasets` (what stays client-side, addressed as `name` or `(name, scope)`); `pre_transform_datasets` instead takes an explicit `datasets` selector list and has no interactivity flags. The runtime is a multi-threaded worker pool; cache/memory/thread state is read through properties.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                                                                                                                                                              | [CAPABILITY]                              |
| :-----: | :------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `runtime.pre_transform_spec`     | `pre_transform_spec(spec, local_tz=None, default_input_tz=None, row_limit=None, preserve_interactivity=True, inline_datasets=None, keep_signals=None, keep_datasets=None)` -> `(dict, list[PreTransformWarning])`                                                          | pre-evaluate transforms in a Vega spec    |
|  [02]   | `runtime.pre_transform_datasets` | `pre_transform_datasets(spec, datasets: list[str \| tuple[str, list[int]]], local_tz=None, default_input_tz=None, row_limit=None, inline_datasets=None, trim_unused_columns=False, dataset_format='auto')` -> `(list[DataFrameLike], list[PreTransformWarning])`           | extract named transformed datasets        |
|  [03]   | `runtime.pre_transform_extract`  | `pre_transform_extract(spec, local_tz=None, default_input_tz=None, preserve_interactivity=True, extract_threshold=20, extracted_format='arro3', inline_datasets=None, keep_signals=None, keep_datasets=None)` -> `(dict, list[tuple[str, list[int], pa.Table]], list[PreTransformWarning])` | split spec and inline data tables |
|  [04]   | `runtime.new_chart_state`        | `new_chart_state(spec, local_tz=None, default_input_tz=None, row_limit=None, inline_datasets=None)` -> `ChartState`                                                                                                                                                       | open an interactive chart state           |
|  [05]   | `runtime.grpc_connect`           | `grpc_connect(url: str)` -> `None`                                                                                                                                                                                                                                       | route execution to a gRPC runtime         |
|  [06]   | `runtime.clear_cache`            | `clear_cache()` -> `None`                                                                                                                                                                                                                                                | clear the transform cache                 |
|  [07]   | `runtime.reset`                  | `reset()` -> `None`                                                                                                                                                                                                                                                      | reset the worker pool and connection state |
|  [08]   | `runtime.cache_capacity` / `memory_limit` / `worker_threads` | properties (read/configure capacity, byte cap, pool size)                                                                                                                                                                                   | runtime resource policy                   |
|  [09]   | `runtime.size` / `total_memory` / `using_grpc`               | properties (current cache size, resident bytes, gRPC-routed flag)                                                                                                                                                                            | runtime introspection                     |

[ENTRYPOINT_SCOPE]: `ChartState` and transformer helpers
- rail: charts

`ChartState` maintains the server/client spec split and applies interactive updates; the `transformer` family feeds inline datasets as Arrow IPC (the wire `inline_datasets` rides). `get_comm_plan` and `get_watch_plan` are the two names for the signal/dataset dependency graph that drives incremental `update`.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                                              | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `ChartState.get_transformed_spec`    | `get_transformed_spec()` -> `dict`                                       | the fully pre-transformed spec            |
|  [02]   | `ChartState.get_client_spec`         | `get_client_spec()` -> `dict`                                            | the client-side render spec               |
|  [03]   | `ChartState.get_server_spec`         | `get_server_spec()` -> `dict`                                            | the server-side transform spec            |
|  [04]   | `ChartState.update`                  | `update(client_updates: list[VariableUpdate])` -> `list[VariableUpdate]` | apply interactive variable updates        |
|  [05]   | `ChartState.get_comm_plan`           | `get_comm_plan()` -> `CommPlan`                                          | signal/dataset comm dependency plan       |
|  [06]   | `ChartState.get_watch_plan`          | `get_watch_plan()` -> `CommPlan`                                         | the watch plan (alias of comm plan)       |
|  [07]   | `ChartState.get_warnings`            | `get_warnings()` -> `list[PreTransformWarning]`                          | warnings collected opening the state      |
|  [08]   | `transformer.to_arrow_ipc_bytes`     | `to_arrow_ipc_bytes(data: DataFrameLike, stream=False)` -> `bytes`       | narwhals dataframe to Arrow IPC bytes     |
|  [09]   | `transformer.arrow_table_to_ipc_bytes` | `arrow_table_to_ipc_bytes(table: pa.Table, stream=False)` -> `bytes`   | Arrow table to IPC bytes (file/stream)    |
|  [10]   | `transformer.to_arrow_table`         | `to_arrow_table(data: DataFrameLike)` -> `pa.Table`                      | narwhals dataframe to a pyarrow table     |
|  [11]   | `transformer.to_feather`             | `to_feather(data: DataFrameLike, file)` -> `None`                        | write dataframe as a Feather/Arrow file   |
|  [12]   | `get_column_usage`                   | `get_column_usage(spec: dict)` -> `dict[str, list[str] \| None]`         | per-dataset referenced-column analysis    |
|  [13]   | `set_local_tz` / `get_local_tz`      | `set_local_tz(local_tz: str)` -> `None` / `get_local_tz()` -> `str`      | configure/read the runtime default tz     |

## [04]-[IMPLEMENTATION_LAW]

[CHARTS_EXPORT]:
- import: `import vegafusion` at boundary scope only; module-level import is banned by the manifest import policy. The manifest gates the row `python_version<'3.15'` (no cp315 wheel); the chart owner dispatches the transform arm onto the runtime subprocess lane, and the gated-band worker imports `vegafusion` at module scope inside that lane.
- transform axis: `runtime.pre_transform_spec` is the single pre-evaluation surface keyed by spec; `preserve_interactivity`, `keep_signals`, and `keep_datasets` are rows on that surface, never a per-mode transform function. `keep_signals`/`keep_datasets` accept `name` or `(name, scope)` to retain selected interactive variables client-side.
- extraction axis: `pre_transform_datasets` returns explicitly named server-computed tables (the `datasets` selector list, each `name` or `(name, scope)`); `pre_transform_extract` splits the spec from inline tables larger than `extract_threshold` and returns them as `(name, scope, table)` in `extracted_format` (default `'arro3'`). Extraction is two rows of the transform surface, not a re-minted query engine.
- state axis: `new_chart_state` opens a `ChartState` for interactive maintenance; the server/client spec split, `update`, and the `get_comm_plan`/`get_watch_plan` dependency graph are the interactive rows, never a duplicated spec per frame.
- dataset axis: `transformer.to_arrow_ipc_bytes`/`arrow_table_to_ipc_bytes`/`to_arrow_table`/`to_feather` feed `inline_datasets`; Arrow IPC over the narwhals interchange is the single dataset wire, never a per-format serializer. `inline_datasets` maps a spec `data.name`/`data.url` to a `DataFrameLike`, so polars/pandas/pyarrow frames all enter through the one narwhals path.
- resource axis: the runtime is a worker pool; `worker_threads`/`cache_capacity`/`memory_limit` configure it once, `clear_cache`/`reset` reclaim, and `size`/`total_memory`/`using_grpc` introspect — caps are properties on the singleton, never a fresh runtime per transform.
- evidence: each transform captures spec identity, row limit, transformed dataset count/byte size, timezone, worker/cache state, and collected `PreTransformWarning` list as a charts receipt.
- boundary: vegafusion owns server-side Vega transform execution; `altair` produces the input spec; static rasterization routes to `vl-convert-python`; live UI stays outside this package.

[STACK_INTEGRATION]:
- `altair` -> `vegafusion` -> `vl_convert`: `altair.Chart.to_dict()` yields the Vega-Lite spec; the chart owner compiles it to Vega via `vl_convert.vegalite_to_vega`, then `runtime.pre_transform_spec` server-evaluates the transforms (aggregations, joins, binning) so the emitted spec carries pre-computed tables; `vl_convert.vega_to_svg`/`vega_to_png` renders that spec with no client-side transform work and no oversized embedded data.
- narwhals dataset rail: a polars/pandas frame the data tier produces is handed to `transformer.to_arrow_ipc_bytes` (or passed directly as a `DataFrameLike` in `inline_datasets`) — narwhals resolves the interchange, so the chart owner never branches on the source dataframe library.
- pre-flight column pruning: `get_column_usage(spec)` reports the columns each dataset actually references; the owner trims wide inline frames before `pre_transform_datasets(trim_unused_columns=True)` so only used columns cross the Arrow wire.

[RAIL_LAW]:
- Package: `vegafusion`
- Owns: server-side Vega data-transform pre-evaluation, named/threshold dataset extraction, spec/data splitting, interactive chart-state maintenance, gRPC runtime connection, worker-pool resource policy, and Arrow IPC dataset feeding via narwhals
- Accept: pre-transform, dataset extraction, and column-usage analysis for `altair`/Vega specs feeding the chart `EXPORT` and `vl-convert` render paths
- Reject: wrapper-renames of `pre_transform_*`; a hand-rolled transform engine where the embedded DataFusion runtime executes; a per-format dataset serializer where Arrow IPC is the wire; a fresh runtime per transform where the singleton's properties tune the pool; a re-minted rasterizer where `vl-convert` renders; identity minting the runtime owns
