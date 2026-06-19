# [PY_ARTIFACTS_API_VEGAFUSION]

`vegafusion` supplies the Rust-backed server-side Vega transform engine for the artifacts charts rail: a module-level `runtime` (`VegaFusionRuntime`) whose `pre_transform_*` family executes a Vega spec's data transforms before render and a `ChartState` that maintains interactive transform state, plus an Arrow IPC transformer family that feeds inline datasets. The package owner composes `runtime.pre_transform_spec`, `pre_transform_datasets`, and `new_chart_state` into the chart `EXPORT` path; it never re-implements the Vega transform pipeline the embedded DataFusion engine already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vegafusion`
- package: `vegafusion`
- import: `vegafusion`
- owner: `artifacts`
- rail: charts
- installed: `2.0.3` reflected via `python -c "import vegafusion"` on cp315 (manifest band `python_version<'3.15'`; abi3 wheel imports on the cp315 core, gated arm runs on the subprocess lane)
- entry points: none (library only)
- capability: server-side Vega data-transform pre-evaluation, transformed-dataset extraction, interactive chart-state maintenance over inline datasets, gRPC runtime connection, Arrow IPC dataset feeding, timezone configuration, and column-usage analysis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime and chart-state roots
- rail: charts

The module `__all__` exports `runtime` (singleton `VegaFusionRuntime`), `set_local_tz`, `get_local_tz`, and `get_column_usage`; `runtime.new_chart_state` returns a `vegafusion.runtime.ChartState` carrying the interactive transform plan (reached via the runtime, not a top-level import). The `transformer` submodule converts dataframes to Arrow IPC `bytes` for the inline-dataset path.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [RAIL]                                         |
| :-----: | :------------ | :---------------- | :--------------------------------------------- |
|  [01]   | `runtime`     | runtime singleton | `VegaFusionRuntime` transform-execution owner  |
|  [02]   | `ChartState`  | interactive state | `new_chart_state` return; transform/watch plan |
|  [03]   | `transformer` | submodule         | dataframe-to-Arrow-IPC conversion family       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `runtime` transform execution
- rail: charts

The `pre_transform_*` rows share `spec`, `local_tz`, `default_input_tz`, `row_limit`, and `inline_datasets` policy; `preserve_interactivity`, `keep_signals`, and `keep_datasets` control what stays client-side.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                                                                                                                                  | [CAPABILITY]                           |
| :-----: | :------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `runtime.pre_transform_spec`     | `pre_transform_spec(spec, local_tz=None, default_input_tz=None, row_limit=None, preserve_interactivity=True, inline_datasets=None, keep_signals=None, keep_datasets=None)` -> `(spec, warnings)`                                              | pre-evaluate transforms in a Vega spec |
|  [02]   | `runtime.pre_transform_datasets` | `pre_transform_datasets(spec, datasets, local_tz=None, default_input_tz=None, row_limit=None, inline_datasets=None, trim_unused_columns=False, dataset_format='auto')` -> `(dataframes, warnings)`                                            | extract transformed datasets           |
|  [03]   | `runtime.pre_transform_extract`  | `pre_transform_extract(spec, local_tz=None, default_input_tz=None, preserve_interactivity=True, extract_threshold=20, extracted_format='arro3', inline_datasets=None, keep_signals=None, keep_datasets=None)` -> `(spec, datasets, warnings)` | split spec and extracted data tables   |
|  [04]   | `runtime.new_chart_state`        | `new_chart_state(spec, local_tz=None, default_input_tz=None, row_limit=None, inline_datasets=None)` -> `ChartState`                                                                                                                           | open an interactive chart state        |
|  [05]   | `runtime.grpc_connect`           | `grpc_connect(url)`                                                                                                                                                                                                                           | route execution to a gRPC runtime      |
|  [06]   | `runtime.clear_cache`            | `clear_cache()`                                                                                                                                                                                                                               | clear the transform cache              |
|  [07]   | `runtime.cache_capacity`         | property                                                                                                                                                                                                                                      | cache capacity policy                  |
|  [08]   | `runtime.memory_limit`           | property                                                                                                                                                                                                                                      | runtime memory cap                     |

[ENTRYPOINT_SCOPE]: `ChartState` and transformer helpers
- rail: charts

`ChartState` maintains the server/client spec split and applies interactive updates; the `transformer` family feeds inline datasets as Arrow IPC.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                         | [CAPABILITY]                       |
| :-----: | :-------------------------------- | :--------------------------------------------------- | :--------------------------------- |
|  [01]   | `ChartState.get_transformed_spec` | `get_transformed_spec()` -> `dict`                   | the fully pre-transformed spec     |
|  [02]   | `ChartState.get_client_spec`      | `get_client_spec()` -> `dict`                        | the client-side render spec        |
|  [03]   | `ChartState.get_server_spec`      | `get_server_spec()` -> `dict`                        | the server-side transform spec     |
|  [04]   | `ChartState.update`               | `update(client_updates)` -> `list[VariableUpdate]`   | apply interactive variable updates |
|  [05]   | `ChartState.get_watch_plan`       | `get_watch_plan()` -> `CommPlan`                     | the signal/dataset watch plan      |
|  [06]   | `transformer.to_arrow_ipc_bytes`  | `to_arrow_ipc_bytes(data, stream=False)` -> `bytes`  | dataframe to Arrow IPC bytes       |
|  [07]   | `transformer.to_arrow_table`      | `to_arrow_table(data)` -> `pa.Table`                 | dataframe to a pyarrow table       |
|  [08]   | `get_column_usage`                | `get_column_usage(spec)` -> `dict[str, list[str]     | None]`                             | per-dataset referenced-column analysis |
|  [09]   | `set_local_tz` / `get_local_tz`   | `set_local_tz(local_tz)` / `get_local_tz()` -> `str` | configure the runtime timezone     |

## [04]-[IMPLEMENTATION_LAW]

[CHARTS_EXPORT]:
- import: `import vegafusion` at boundary scope only; module-level import is banned by the manifest import policy. The manifest gates the row `python_version<'3.15'`; the chart owner dispatches the transform arm onto the runtime subprocess lane, and the gated-band worker imports `vegafusion` at module scope.
- transform axis: `runtime.pre_transform_spec` is the single pre-evaluation surface keyed by spec; `preserve_interactivity`, `keep_signals`, and `keep_datasets` are rows on that surface, never a per-mode transform function.
- extraction axis: `pre_transform_datasets`/`pre_transform_extract` return server-computed tables for the static `EXPORT` path; extraction is a row of the transform surface, not a re-minted query engine.
- state axis: `new_chart_state` opens a `ChartState` for interactive maintenance; the server/client spec split and `update` are the interactive rows, never a duplicated spec per frame.
- dataset axis: `transformer.to_arrow_ipc_bytes`/`to_arrow_table` feed `inline_datasets`; Arrow IPC is the single dataset wire, never a per-format serializer.
- evidence: each transform captures spec identity, row limit, transformed dataset count/byte size, timezone, and collected warnings as a charts receipt.
- boundary: vegafusion owns server-side Vega transform execution; `altair` produces the input spec; static rasterization routes to `vl-convert-python`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `vegafusion`
- Owns: server-side Vega data-transform pre-evaluation, transformed-dataset extraction, interactive chart-state maintenance, gRPC runtime connection, and Arrow IPC dataset feeding
- Accept: pre-transform and dataset extraction for `altair`/Vega specs feeding the chart `EXPORT` and `vl-convert` render paths
- Reject: wrapper-renames of `pre_transform_*`; a hand-rolled transform engine where the embedded DataFusion runtime executes; a per-format dataset serializer where Arrow IPC is the wire; a re-minted rasterizer where `vl-convert` renders; identity minting the runtime owns
