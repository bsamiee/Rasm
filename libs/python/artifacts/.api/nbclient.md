# [PY_ARTIFACTS_API_NBCLIENT]

`nbclient` owns the Jupyter notebook execution runtime for the `artifacts` notebook rail: `NotebookClient` drives the full kernel lifecycle — cell-by-cell execution, per-cell timeout, output-hook and widget-state capture — and folds every cell, timeout, and kernel fault onto a typed exception family. `async_execute` is the authoritative coroutine entrypoint; every sync method trampolines it through `run_sync`, so the async mirror carries the real signature. Rail composition rides the `anyio` loop natively, never a worker thread.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbclient`
- package: `nbclient` (BSD-3-Clause)
- module: `nbclient`
- owner: `artifacts`
- rail: notebook
- asset: pure-Python runtime, no native ABI
- depends: `nbformat` (the `NotebookNode` model), `jupyter_client` (`KernelManager`/`KernelClient` protocol), `traitlets` (config-trait base)
- exports: `NotebookClient` and `execute` at the package root; the fault family from `nbclient.exceptions`, `OutputWidget` from `nbclient.output_widget`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and one-shot entry

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                                          |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `NotebookClient` | execution client | `LoggingConfigurable`; kernel lifecycle, cell execution, timeout, hooks, widget state |
|  [02]   | `execute`        | module function  | `execute(nb, cwd=None, km=None, **kw) -> NotebookNode` one-shot construct-and-run     |

[PUBLIC_TYPE_SCOPE]: exception family — `nbclient.exceptions`

`CellControlSignal` is a plain `Exception`, not a fault; the loop catches it to split a clean cell end (`CellExecutionComplete`) from an error. `CellTimeoutError` also subclasses stdlib `TimeoutError`, so a timeout is catchable as a stdlib timeout — the seam the `[04]` `deadline` route keys on.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                                                        |
| :-----: | :---------------------- | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `CellControlSignal`     | control base      | base loop signal (`<- Exception`); caught to branch loop flow                       |
|  [02]   | `CellExecutionComplete` | completion signal | raised to end cell execution loop (`<- CellControlSignal`)                          |
|  [03]   | `CellExecutionError`    | cell fault        | cell raised; `traceback`/`ename`/`evalue`; built via `from_cell_and_msg`; picklable |
|  [04]   | `CellTimeoutError`      | timeout fault     | cell exceeded `timeout`; built via `error_from_timeout_and_cell`                    |
|  [05]   | `DeadKernelError`       | kernel fault      | kernel process died mid-run; the sole non-`CellControlSignal` fault                 |

[ENTRYPOINT_SCOPE]: fault constructors — `nbclient.exceptions`

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `CellExecutionError(traceback, ename, evalue)`                     | direct construct (`traceback`-first positional order)              |
|  [02]   | `CellExecutionError.from_cell_and_msg(cell, msg)`                  | build from a cell + kernel `error` reply dict; folds stream output |
|  [03]   | `CellTimeoutError.error_from_timeout_and_cell(msg, timeout, cell)` | build a timeout fault with a previewed cell-source snippet         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution — `nbclient.NotebookClient`

Sync rows `[03]`/`[05]`/`[09]`/`[11]`/`[14]` are `run_sync` wrappers of the `async_` mirror beneath them; the async form carries the authoritative signature and is the form the rail composes. Constructor `resources` seeds `metadata.path` from `cwd`; every `execute`/cell method returns the mutated `NotebookNode`.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `nbclient.execute(nb, cwd, km, **kwargs)`                              | one-shot construct + run; `cwd` seeds `metadata.path`       |
|  [02]   | `NotebookClient(nb, km, **kw)`                                         | bind a node to a kernel manager; traits + `resources=` dict |
|  [03]   | `execute(reset_kc, **kwargs)`                                          | sync `run_sync` wrapper of `async_execute`                  |
|  [04]   | `async_execute(reset_kc, **kwargs)`                                    | the rail entrypoint; runs all cells on the loop             |
|  [05]   | `execute_cell(cell, cell_index, execution_count, store_history)`       | sync wrapper; mutates `cell` in place                       |
|  [06]   | `async_execute_cell(cell, cell_index, execution_count, store_history)` | coroutine single-cell exec; mutates `cell` in place         |
|  [07]   | `setup_kernel(**kwargs)`                                               | sync `@contextmanager`: start, yield, shutdown              |
|  [08]   | `async_setup_kernel(**kwargs)`                                         | `@asynccontextmanager`; SIGINT/SIGTERM + `atexit` cleanup   |
|  [09]   | `start_new_kernel(**kwargs)`                                           | sync fresh-kernel start outside the ctx manager             |
|  [10]   | `async_start_new_kernel(**kwargs)`                                     | coroutine; populates `self.km`                              |
|  [11]   | `start_new_kernel_client()`                                            | sync start + connect the kernel client                      |
|  [12]   | `async_start_new_kernel_client()`                                      | coroutine; sets `self.kc`                                   |
|  [13]   | `process_message(msg, cell, cell_index)`                               | route one kernel msg to outputs; mutates `cell.outputs`     |
|  [14]   | `wait_for_reply(msg_id, cell)` / `_wait_for_reply`                     | sync block until idle reply (`_wait_for_reply` alias)       |
|  [15]   | `async_wait_for_reply(msg_id, cell)`                                   | coroutine; returns the shell reply or `None` on timeout     |
|  [16]   | `create_kernel_manager()`                                              | build a `KernelManager` from `kernel_manager_class`         |
|  [17]   | `reset_execution_trackers()`                                           | reset timing + cell-index state                             |

[ENTRYPOINT_SCOPE]: output-hook and widget capture — `nbclient.NotebookClient`

`register_output_hook` pushes an `OutputWidget` onto a per-`msg_id` stack, the last registered winning; `store_widget_state=True` drives this path to populate `metadata.widgets` without subclassing.

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                                           |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | `register_output_hook(msg_id, hook)`        | push an `OutputWidget` onto the `msg_id` stack                         |
|  [02]   | `remove_output_hook(msg_id, hook)`          | pop a previously registered output hook (asserts identity)             |
|  [03]   | `output(outs, msg, display_id, cell_index)` | append a kernel output message to the cell's `outputs`                 |
|  [04]   | `clear_output(outs, msg, cell_index)`       | apply a `clear_output` message to the cell                             |
|  [05]   | `clear_display_id_mapping(cell_index)`      | clear the cell's `display_id -> output` index                          |
|  [06]   | `handle_comm_msg(outs, msg, cell_index)`    | dispatch a Jupyter comm (widget) message                               |
|  [07]   | `on_comm_open_jupyter_widget(msg)`          | build an `OutputWidget` from the widget registry on widget `comm_open` |
|  [08]   | `set_widgets_metadata()`                    | write captured widget state to `metadata.widgets`                      |

[ENTRYPOINT_SCOPE]: key configuration traits — `nbclient.NotebookClient`

| [INDEX] | [SURFACE]                | [TRAIT_TYPE]                               | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :----------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `timeout`                | `Integer=None`                             | per-cell execution timeout in seconds (`None` = no limit)      |
|  [02]   | `startup_timeout`        | `Integer=60`                               | kernel start wait in seconds                                   |
|  [03]   | `kernel_name`            | `Unicode=''`                               | kernel spec name override (`''` = notebook's own kernelspec)   |
|  [04]   | `allow_errors`           | `Bool=False`                               | continue execution past cell errors                            |
|  [05]   | `allow_error_names`      | `List`                                     | error names tolerated even when `allow_errors=False`           |
|  [06]   | `force_raise_errors`     | `Bool=False`                               | `CellExecutionError` past `allow_errors`/`raises-exception`    |
|  [07]   | `skip_cells_with_tag`    | `Unicode='skip-execution'`                 | skip cells bearing this metadata tag                           |
|  [08]   | `record_timing`          | `Bool=True`                                | record per-cell timing in cell metadata                        |
|  [09]   | `store_widget_state`     | `Bool=True`                                | capture Jupyter widget state into notebook `metadata.widgets`  |
|  [10]   | `shutdown_kernel`        | `Enum('graceful'\|'immediate')='graceful'` | kernel shutdown policy                                         |
|  [11]   | `interrupt_on_timeout`   | `Bool=False`                               | send kernel interrupt on cell timeout instead of failing       |
|  [12]   | `error_on_timeout`       | `Dict=None`                                | fake `error` reply on timeout (`ename`/`evalue`/`traceback`)   |
|  [13]   | `iopub_timeout`          | `Integer=4`                                | seconds to wait for iopub output after an idle reply           |
|  [14]   | `raise_on_iopub_timeout` | `Bool=False`                               | raise instead of warn when iopub output is late                |
|  [15]   | `coalesce_streams`       | `Bool=False`                               | merge consecutive stream outputs into one                      |
|  [16]   | `shell_timeout_interval` | `Integer=5`                                | shell-channel poll granularity, reply loop ticks per `get_msg` |
|  [17]   | `kernel_manager_class`   | `Type(klass=KernelManager)`                | `KernelManager` subclass `create_kernel_manager` builds        |
|  [18]   | `extra_arguments`        | `List(Unicode)`                            | extra CLI args appended to the kernel launch                   |
|  [19]   | `ipython_hist_file`      | `Unicode=':memory:'`                       | IPython history-file launch arg (`:memory:` = ephemeral)       |
|  [20]   | `display_data_priority`  | `List`                                     | MIME-type preference order for display output selection        |

[ENTRYPOINT_SCOPE]: lifecycle-hook traits — `nbclient.NotebookClient`

nbclient fires these eight `Callable` traits around the notebook and per-cell lifecycle through `run_hook`/`run_sync(run_hook)`, so a hook may be sync or async.

| [INDEX] | [SURFACE]              | [TRAIT_TYPE] | [CAPABILITY]                                                                 |
| :-----: | :--------------------- | :----------- | :--------------------------------------------------------------------------- |
|  [01]   | `on_notebook_start`    | `Callable`   | hook before notebook execution begins (`fn(notebook=)`)                      |
|  [02]   | `on_notebook_complete` | `Callable`   | hook when notebook execution finishes (`fn(notebook=)`)                      |
|  [03]   | `on_notebook_error`    | `Callable`   | hook when notebook execution raises (`fn(notebook=)`)                        |
|  [04]   | `on_cell_start`        | `Callable`   | hook before a cell is sent to the kernel (`fn(cell=, cell_index=)`)          |
|  [05]   | `on_cell_execute`      | `Callable`   | hook as a cell begins executing (`fn(cell=, cell_index=)`)                   |
|  [06]   | `on_cell_executed`     | `Callable`   | hook after a cell's execute reply (`fn(cell=, cell_index=, execute_reply=)`) |
|  [07]   | `on_cell_complete`     | `Callable`   | hook after a cell fully completes (`fn(cell=, cell_index=)`)                 |
|  [08]   | `on_cell_error`        | `Callable`   | hook when a cell raises (`fn(cell=, cell_index=, execute_reply=)`)           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `async_execute` awaited directly on the `anyio` loop is the one rail form; it returns the mutated in-memory `NotebookNode`. Every sync method is a `run_sync(async_…)` trampoline, so a sync `execute` inside a running event loop is the nested-loop hazard the `async_` mirror avoids; `setup_kernel`/`async_setup_kernel` own explicit kernel lifecycle when the client outlives one `execute`.
- `NotebookEngine.client_kwargs()` is the one boundary view: it `asdict`s the frozen `msgspec.Struct` and re-projects `error_on_timeout` from its `frozendict` to a real `dict` (a `traitlets.Dict` rejects a `frozendict`), while tuple fields (`allow_error_names`/`display_data_priority`/`extra_arguments`) ride as-is since `traitlets.List` coerces a tuple. A new bounded-safety trait is one struct field, zero call-site edit.
- Bounded-safety defaults: `force_raise_errors=True` + `interrupt_on_timeout=True` + `raise_on_iopub_timeout=True` make a runaway cell raise rather than hang and a late iopub raise rather than warn; `coalesce_streams=True` merges adjacent stream output; `record_timing=True` stamps per-cell timing; `allow_errors=False` with `allow_error_names` as the narrow whitelist keeps a failing cell a hard fault.
- Timeout: `timeout` applies per-cell (`None` = unbounded, the rail pins 600); `startup_timeout` bounds the kernel-start handshake; `error_on_timeout` injects a fake `error` reply as the cell result; `iopub_timeout`/`raise_on_iopub_timeout` bound the trailing output wait; `shell_timeout_interval` is the shell-poll granularity the reply loop ticks on.
- Widget state: `store_widget_state=True` captures Jupyter widget state into `metadata.widgets` via `on_comm_open_jupyter_widget` -> `register_output_hook` -> `OutputWidget` -> `set_widgets_metadata`.
- Eight `on_*` `Callable` hooks stay off the keyed plan: a `Callable` is not a content-keyable reproducibility fact, so receipt and progress capture is the `@receipted` harvest off `self.fact` and the `runtime` observability span.

[STACKING]:
- `papermill`(`.api/papermill.md`): `parameterize_notebook` injects the parameter cell upstream and its default `NBClientEngine` delegates the kernel lifecycle to this client — nbclient owns the execution loop papermill's engine wraps.
- `jupytext`(`.api/jupytext.md`): `jupytext.reads` sources the node; `jupytext.writes(executed, "ipynb")` archives the mutated node after execution.
- `nbconvert`(`.api/nbconvert.md`): the executed `NotebookNode` feeds `nbconvert.get_exporter(...).from_notebook_node` downstream; `nbconvert.ExecutePreprocessor` is a thin wrapper over this same client, left `enabled=False` to avoid double execution.
- `document/report#NBCLIENT_ENGINE` constructs `NotebookClient(parameterized, **NotebookEngine.client_kwargs()).async_execute()` awaited natively on the `anyio` loop with no `to_thread` offload; only the downstream blocking `nbconvert` render crosses `anyio.to_thread.run_sync(limiter=_OFFLOAD)`. Faults funnel through `runtime/reliability/faults#FAULT` `async_boundary` onto `RuntimeRail = Result[T, BoundaryFault]` (`expression`).

[LOCAL_ADMISSION]:
- `NotebookNode` is `nbformat`'s in-memory form, mutated in place during execution; the owner archives it via `jupytext.writes` and lowers it via `nbconvert.get_exporter(...).from_notebook_node`, never re-reading from disk.
- `km` accepts an externally managed `KernelManager`; when absent the client builds one from `kernel_manager_class` and destroys it. `kernel_name=''` defers to the notebook's kernelspec; the rail pins `kernel_name="python3"` and threads the same value into `parameterize_notebook` so a non-Python kernel routes its own `papermill` translator.
- `document/report`'s `async_boundary` catches every fault; the `CLASSIFY` table routes by family: `CellTimeoutError` lands on the `deadline` row through its stdlib `TimeoutError` base, while `CellExecutionError` (`<- Exception`) and `DeadKernelError` (`<- RuntimeError`) land on the `boundary` catch-all — the rail never catches an nbclient fault itself.
- `CellExecutionError` carries `traceback`/`ename`/`evalue` from the kernel, embeds the error output in the node, and is picklable (`__reduce__`) so it survives a process boundary; `DeadKernelError` requires a kernel restart and is not recoverable by the same `NotebookClient` instance.

[RAIL_LAW]:
- Package: `nbclient`
- Owns: Jupyter notebook kernel lifecycle, cell-by-cell execution, output/widget capture, timeout enforcement, and the typed-fault family (`CellExecutionError`/`CellTimeoutError`/`DeadKernelError`).
- Accept: `NotebookNode` from `nbformat` mutated in place; `KernelManager`/`KernelClient` from `jupyter_client`; a `papermill.parameterize_notebook`-injected node; bounded-safety traits projected through `NotebookEngine.client_kwargs()`.
- Reject: hand-rolled kernel protocol; a parallel notebook runner duplicating the execution loop; a `NotebookClient` subclass where a trait or `client_kwargs` projection carries the config; an `on_*` hook on the keyed plan where a `Callable` is not a serializable reproducibility fact; a sync `execute` inside the running event loop where `async_execute` is correct; a local `try`/`except` around `async_execute` where the `async_boundary` fold owns the conversion onto the rail.
