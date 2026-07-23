# [PY_ARTIFACTS_API_VEGAFUSION]

`vegafusion` owns the Rust-backed server-side Vega transform engine (embedded Apache DataFusion) for the charts rail: a module-level `runtime` singleton whose `pre_transform_*` family server-evaluates a spec's transforms — aggregation, join, binning, filtering — before render, a `ChartState` maintaining the interactive server/client split, the `vegafusion.utils` planner diagnostic and referenced-column analyzer, and a `transformer` submodule serializing frames to Arrow IPC. Two disjoint consumer planes divide it: the chart-render pre-pass (`visualization/chart/export#PREPASS` folds `runtime.pre_transform_spec` for the static spec, `runtime.new_chart_state(...).get_transformed_spec()` for the host-free interactive HTML row, the reduction inlining INSIDE one self-contained spec) and the columnar egress plane (`pre_transform_extract` `(name, scope, table)` tuples, `transformer` serializers, `get_column_usage`) owned by `data/tabular/columnar#COLUMNAR`. Each pre-pass folds onto the universal rails: one `msgspec` charts receipt under one `structlog` event inside an `opentelemetry` span, a malformed spec or `PreTransformWarning` onto the `expression.Result` rail, the native pre-pass crossing the subprocess seam via `anyio` `to_process`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vegafusion`
- package: `vegafusion` (BSD-3-Clause)
- module: `vegafusion` (canonical alias `import vegafusion as vf`)
- owner: `artifacts`
- rail: charts
- asset: Rust extension wheel (`_vegafusion.abi3.so`), abi3 forward-compatible, no cp-gate; the native core returns an `arro3-core` Arrow `Table` (default extracted format `arro3`, not `pyarrow`); hard deps `narwhals` (dataframe interchange — polars/pandas/pyarrow ingest), `packaging`; `vl-convert-python` is a soft dep reached lazily by `get_local_tz()` only (tz fallback); `pandas`/`polars`/`pyarrow` resolve through `sys.modules` at call time, never eager
- capability: server-side Vega data-transform pre-evaluation, named/threshold dataset extraction, spec/data splitting, interactive chart-state maintenance, planner diagnostics, per-dataset referenced-column analysis, gRPC runtime connection, reset-on-set worker-pool resource policy, and Arrow IPC dataset feeding via the narwhals interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime and chart-state roots

Module `__all__` exports exactly `runtime` (the singleton `VegaFusionRuntime` *instance*), `set_local_tz`, `get_local_tz`, `get_column_usage`; `ChartState`, `VariableUpdate`, `Variable`, `CommPlan`, `PreTransformWarning` are `class`/`TypedDict` defs in `vegafusion.runtime`, `PreTransformSpecPlan` a `TypedDict` in `vegafusion.utils`. Native core `vegafusion._vegafusion` binds `PyChartState`, `PyVegaFusionRuntime`, `build_pre_transform_spec_plan`, `get_column_usage`, `get_cpu_count`, `get_virtual_memory`; `PyChartStateGrpc` is `TYPE_CHECKING`-only, not importable at runtime (gRPC routes `runtime.grpc_connect` -> `new_grpc(url)`). Shadowing trap: `__init__.py` rebinds attribute `runtime` to the singleton instance, so `vegafusion.runtime.ChartState` raises `AttributeError` while `runtime.pre_transform_spec` resolves on the instance; reach `ChartState` via `sys.modules["vegafusion.runtime"]`/`importlib` or the `runtime.new_chart_state(...)` return.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]     | [CAPABILITY]                                                                 |
| :-----: | :---------------------------- | :---------------- | :--------------------------------------------------------------------------- |
|  [01]   | `runtime`                     | runtime singleton | `VegaFusionRuntime` worker-pool transform owner; sole `__all__` export       |
|  [02]   | `ChartState`                  | interactive state | the `new_chart_state` return; the shadowed interactive-state class           |
|  [03]   | `CommPlan`                    | watch plan        | `get_comm_plan`/`get_watch_plan` return; the signal/dataset comm plan        |
|  [04]   | `VariableUpdate` / `Variable` | update record     | `ChartState.update` element; `{name, namespace 'data'/'signal', scope}`      |
|  [05]   | `PreTransformWarning`         | warning           | the `pre_transform_*`/`ChartState` warnings element (values at [05])         |
|  [06]   | `PreTransformSpecPlan`        | planner plan      | `build_pre_transform_spec_plan` return; the pre-exec planned split           |
|  [07]   | `DatasetFormat` (`Literal`)   | format axis       | `auto`/`polars`/`pandas`/`pyarrow`/`arro3`; the `dataset_format=` selector   |
|  [08]   | `DataFrameLike` (`= Any`)     | interchange alias | any pandas/polars/pyarrow/`dataframe` frame the narwhals path accepts        |
|  [09]   | `transformer`                 | submodule         | dataframe-to-Arrow-IPC conversion family (columnar egress, not chart render) |

- [05]-[PRETRANSFORMWARNING]: `type` ∈ `RowLimitExceeded`/`BrokenInteractivity`/`Unsupported`; `message: str`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `runtime` transform execution
- shared carry: `spec` (`dict` or JSON `str`), `local_tz` (defaults `get_local_tz()`), `default_input_tz`, `inline_datasets`

`inline_datasets` is a `dict[str, DataFrameLike]` resolved against a `data.url` of `vegafusion+dataset://{name}` or `table://{name}`; inline-frame columns auto-prune to those the spec references (`get_inline_column_usage` runs internally on every call). `pre_transform_spec`/`pre_transform_extract` carry `preserve_interactivity` + `keep_signals`/`keep_datasets` (retained client-side, each a `name` or `(name, scope)`); `pre_transform_datasets` takes an explicit `datasets` list with no interactivity flags. Runtime is a lazily-initialized worker pool (default `cache_capacity=64`, `memory_limit=virtual_memory//2`, `worker_threads=cpu_count`) whose cache/memory/thread caps are read/write properties whose setters `reset()` the pool. Surfaces below are on the `runtime` singleton; `...` abbreviates the shared carry.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                                                  |
| :-----: | :------------------------------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `.pre_transform_spec`                                   | `pre_transform_spec(spec, ...)` -> `(dict, warnings)`; reduce-and-inline      |
|  [02]   | `.pre_transform_datasets`                               | `pre_transform_datasets(spec, datasets, dataset_format)` -> native frames     |
|  [03]   | `.pre_transform_extract`                                | `pre_transform_extract(spec, ...)` -> `(dict, [(name,scope,DATA)], warnings)` |
|  [04]   | `.new_chart_state`                                      | `new_chart_state(spec, ...)` -> `ChartState`; interactive chart state         |
|  [05]   | `.grpc_connect` / `.using_grpc`                         | `grpc_connect(url)` binds `new_grpc`; `using_grpc` -> `bool`                  |
|  [06]   | `.clear_cache` / `.reset`                               | `clear_cache()` keeps the pool; `reset()` drops it for re-init                |
|  [07]   | `.cache_capacity` / `.memory_limit` / `.worker_threads` | read/write props; the setter `reset()`s the pool only on a changed value      |
|  [08]   | `.size` / `.total_memory`                               | read-only props (`None` before init); cache count, resident bytes             |

[ENTRYPOINT_SCOPE]: `ChartState`, planner, and transformer helpers

`ChartState` maintains the server/client spec split and applies interactive `update`; `get_comm_plan`/`get_watch_plan` are two names for the one `CommPlan` dependency graph. `build_pre_transform_spec_plan` (in `vegafusion.utils`) returns the SAME `client_spec`/`server_spec`/`comm_plan` split `pre_transform_spec` uses without executing — a diagnostic reading the partition before paying for execution. `transformer` + `get_column_usage` belong to the columnar egress plane: `to_arrow_table` is pandas-specific (inspects `data.dtypes`, expands `pd.CategoricalDtype`, batches at `BATCH_SIZE=8096`), `to_arrow_ipc_bytes` accepts pandas / pyarrow / any `__dataframe__`-interchange object. `ChartState.` prefix dropped below.

| [INDEX] | [SURFACE]                              | [CAPABILITY]                                                                                  |
| :-----: | :------------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `get_transformed_spec`                 | `get_transformed_spec()` -> `dict`; the pre-transformed self-contained spec                   |
|  [02]   | `get_client_spec` / `get_server_spec`  | `-> dict` each; client render spec / server transform spec                                    |
|  [03]   | `update`                               | `update(list[VariableUpdate])` -> `list[VariableUpdate]`; apply, return client push-back      |
|  [04]   | `get_comm_plan` / `get_watch_plan`     | `-> CommPlan` (alias pair); the signal/dataset comm plan                                      |
|  [05]   | `get_warnings`                         | `get_warnings()` -> `list[PreTransformWarning]`; warnings from opening the state              |
|  [06]   | `build_pre_transform_spec_plan`        | `build_pre_transform_spec_plan(spec, ...)` -> `PreTransformSpecPlan`; the split, no execution |
|  [07]   | `get_column_usage`                     | `get_column_usage(spec)` -> `dict`; per-dataset referenced columns (`None`=not static)        |
|  [08]   | `transformer.to_arrow_ipc_bytes`       | `to_arrow_ipc_bytes(data, stream=False)` -> `bytes`; frame to Arrow IPC                       |
|  [09]   | `transformer.arrow_table_to_ipc_bytes` | `arrow_table_to_ipc_bytes(table, stream=False)` -> `bytes`; batched `BATCH_SIZE=8096`         |
|  [10]   | `transformer.to_arrow_table`           | `to_arrow_table(data)` -> `pa.Table`; pandas -> pyarrow (Decimal->float, categorical-expand)  |
|  [11]   | `transformer.to_feather`               | `to_feather(data, file)` -> `None`; Feather/Arrow IPC file/stream sink                        |
|  [12]   | `set_local_tz` / `get_local_tz`        | configure/read runtime default tz (falls back to `vl_convert` then `'UTC'`)                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `runtime.pre_transform_spec` is the single spec-keyed pre-evaluation surface; `preserve_interactivity`/`keep_signals`/`keep_datasets` are rows on it, never per-mode transform functions. DataFusion transforms execute server-side and the reduced result inlines INTO the returned spec — the aggregation IS the size reduction, so one self-contained spec renders with no client-side transform work and no oversized embedded data.
- `new_chart_state` -> `ChartState` opens the host-free interactive HTML row; `get_transformed_spec` yields the fully-transformed self-contained spec with no live server; the server/client split, `update` round-trip, and one `CommPlan` dependency graph are the interactive rows, never a duplicated spec per frame.
- `build_pre_transform_spec_plan` is the diagnostic projection of that same plan — one row, never a parallel planning engine.
- resource: `worker_threads`/`cache_capacity`/`memory_limit` setters `reset()` the pool only on a changed value, `clear_cache` reclaims without dropping, `size`/`total_memory` introspect — caps tune the singleton, never a fresh runtime per transform.
- columnar egress: `pre_transform_datasets` returns explicitly named server-computed tables as native frames in `dataset_format`, tz-normalized to `local_tz`; `pre_transform_extract` splits inline tables `>= extract_threshold` rows as `(name, scope, DATA)` following `extracted_format` (`arro3`/`pyarrow` table, `arrow-ipc` bytes, `arrow-ipc-base64` str); these cross to the `data/tabular/columnar#COLUMNAR` owner, tz-normalized, never the renderer.
- evidence: each pre-pass captures spec identity, row limit, transformed/inlined dataset count, timezone, worker/cache state, and the `PreTransformWarning` list as one charts receipt.

[STACKING]:
- `altair`(`altair.md`): `altair.Chart.to_dict()` yields the Vega-Lite spec the pre-pass consumes; `VegaTransform.apply` (`visualization/chart/export#PREPASS`) folds `runtime.pre_transform_spec(spec)` (the `Inline` arm) or `runtime.new_chart_state(spec).get_transformed_spec()` (the `State` arm).
- `vl-convert-python`(`vl-convert-python.md`): renders the ONE reduced self-contained spec to static bytes; the no-external-feed constraint that forces the in-spec reduction is owned there.
- `anyio`(`../../.api/anyio.md`): the gated native pre-pass crosses the subprocess seam via `to_process.run_sync` under one `CapacityLimiter` (GIL-releasing DataFusion core).
- universal rails: one `msgspec`(`../../.api/msgspec.md`) charts receipt under one `structlog`(`../../.api/structlog.md`) event inside an `opentelemetry`(`../../.api/opentelemetry-api.md`) span; a malformed spec or a `ValueError` from `_import_inline_datasets` folds onto the `expression.Result`(`../../.api/expression.md`) rail.
- within-lib: a polars/pandas frame the `data` tier produces enters `inline_datasets` directly as a `DataFrameLike` (narwhals resolves interchange, the owner never branches on source library); `get_column_usage(spec)` drives pre-flight pruning before `pre_transform_extract(extracted_format='arrow-ipc')` hands `bytes` to the columnar owner.

[LOCAL_ADMISSION]:
- Admitted as the charts pre-transform engine; composed only through the `runtime` singleton and the `ChartState` return, over the embedded DataFusion core.

[RAIL_LAW]:
- Package: `vegafusion`
- Owns: server-side Vega data-transform pre-evaluation, named/threshold dataset extraction, spec/data splitting, interactive chart-state maintenance, planner diagnostics, referenced-column analysis, gRPC runtime connection, reset-on-set worker-pool resource policy, and Arrow IPC dataset feeding via the internal narwhals interchange
- Accept: the chart-render pre-pass (`pre_transform_spec` reduce-and-inline, `new_chart_state`/`get_transformed_spec` for the interactive row) for `altair`/Vega specs feeding the `vl-convert` render path; the columnar egress surface (`pre_transform_extract`/`pre_transform_datasets`/`transformer`/`get_column_usage`) for the `data/tabular/columnar` owner
- Reject: a hand-rolled transform engine where the embedded DataFusion runtime executes; an Arrow-IPC side channel into the renderer where the reduction crosses inside the spec; a per-format dataset serializer where the narwhals interchange is the wire; a fresh runtime per transform where the singleton's reset-on-set properties tune the pool; `vegafusion.runtime.ChartState` where the singleton shadows the submodule; a `PyChartStateGrpc` runtime import absent from the wheel; the `VegaFusionWidget` as a charts surface where live UI is out of rail
