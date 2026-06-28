# [PY_ARTIFACTS_API_NBCLIENT]

`nbclient` supplies the Jupyter notebook execution runtime for the artifacts notebook rail: `NotebookClient` owns the full kernel lifecycle, cell-by-cell execution, timeout enforcement, output-hook capture, and cell error handling. The module-level `nbclient.execute(nb, cwd=None, km=None, **kw)` is the one-shot convenience entry that constructs and runs a client in a single call; `NotebookClient.execute` / `async_execute` are the stateful coroutine-aware entrypoints (every sync method is a `run_sync` wrapper over its `async_` mirror). The exceptions family (`CellExecutionError`, `CellTimeoutError`, `DeadKernelError`, plus the `CellControlSignal`/`CellExecutionComplete` loop signals) carries typed failure rails. Integration: nbclient executes the in-memory `nbformat.NotebookNode` in place over a `jupyter_client.KernelManager`; the executed node then feeds `nbconvert` (`ExecutePreprocessor` is nbconvert's wrapper over this same client) and `papermill` (which parameterizes the node before handing it here) — nbclient owns the execution loop those two compose, never a parallel runner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbclient`
- package: `nbclient`
- import: `nbclient`
- owner: `artifacts`
- rail: notebook
- installed: `0.11.0`
- license: BSD-3-Clause
- deps: `nbformat` (notebook model), `jupyter_client` (kernel protocol), `traitlets` (config), `jupyter_core`
- asset: runtime library; top-level surface is `NotebookClient`, `execute`, `exceptions`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and one-shot entry
- rail: notebook — `nbclient`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                          |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `NotebookClient` | execution client | `LoggingConfigurable`; kernel lifecycle, cell execution, timeout, hooks, widget state |
|  [02]   | `execute`        | module function  | `execute(nb, cwd=None, km=None, **kw) -> NotebookNode` one-shot construct-and-run |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `nbclient.exceptions`

`CellControlSignal` is the loop-control base (a normal `Exception`, not an error); `CellExecutionComplete` and `CellExecutionError` both subclass it, so the execution loop catches `CellControlSignal` to distinguish a clean cell end from a cell fault. `CellTimeoutError`/`DeadKernelError` are true faults inheriting stdlib `TimeoutError`/`RuntimeError`. `CellExecutionError.from_cell_and_msg(cell, msg)` is the classmethod the loop uses to build the error from the kernel `error` reply.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :---------------------- | :---------------- | :------------------------------------------------------------ |
|  [01]   | `CellControlSignal`     | control base      | base loop signal (`<- Exception`); caught to branch loop flow |
|  [02]   | `CellExecutionComplete` | completion signal | raised to end cell execution loop (`<- CellControlSignal`)    |
|  [03]   | `CellExecutionError`    | cell fault        | cell raised; carries `ename`/`evalue`/`traceback`; `<- CellControlSignal`; built via `from_cell_and_msg` |
|  [04]   | `CellTimeoutError`      | timeout fault     | cell execution exceeded `timeout`; `<- TimeoutError`          |
|  [05]   | `DeadKernelError`       | kernel fault      | kernel process died during execution; `<- RuntimeError`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution
- rail: notebook — `nbclient` / `nbclient.NotebookClient`

Sync rows `[03]`/`[05]`/`[09]`/`[11]`/`[13]`/`[15]` are `run_sync` wrappers auto-generated from the `async_` mirror beneath them; the async form carries the authoritative signature and is preferred in an async campaign. `[01]` is the module-level one-shot; `[02]` is the constructor.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]     | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `nbclient.execute(nb, cwd=None, km=None, **kw)`                        | one-shot function  | construct a client, run all cells, return modified `NotebookNode` |
|  [02]   | `NotebookClient(nb, km=None, **kw)`                                    | constructor        | bind a `NotebookNode` to a kernel manager (config via traits) |
|  [03]   | `execute(reset_kc=False, **kwargs)`                                    | sync execute       | run all cells; returns modified `NotebookNode`     |
|  [04]   | `async_execute(reset_kc=False, **kwargs)`                              | async execute      | coroutine variant; returns modified `NotebookNode` |
|  [05]   | `execute_cell(cell, cell_index, execution_count=None, store_history=True)` | sync cell     | execute a single cell; returns `NotebookNode`      |
|  [06]   | `async_execute_cell(cell, cell_index, execution_count=None, store_history=True)` | async cell | coroutine cell execution                           |
|  [07]   | `setup_kernel(**kwargs)`                                               | sync ctx manager   | context manager; start kernel, yield, shutdown     |
|  [08]   | `async_setup_kernel(**kwargs)`                                         | async ctx manager  | async context manager variant                      |
|  [09]   | `start_new_kernel(**kwargs)`                                           | sync kernel start  | start a fresh kernel outside context manager       |
|  [10]   | `async_start_new_kernel(**kwargs)`                                     | async kernel start | coroutine variant                                  |
|  [11]   | `start_new_kernel_client()`                                            | sync client start  | start kernel client after kernel is running        |
|  [12]   | `async_start_new_kernel_client() -> KernelClient`                      | async client start | coroutine variant                                  |
|  [13]   | `process_message(msg, cell, cell_index) -> NotebookNode \| None`       | message handler    | route a single kernel message to outputs           |
|  [14]   | `wait_for_reply(msg_id, cell=None)`                                    | sync wait          | block until kernel idle reply                      |
|  [15]   | `async_wait_for_reply(msg_id, cell=None) -> dict \| None`              | async wait         | coroutine variant                                  |
|  [16]   | `create_kernel_manager() -> KernelManager`                             | km factory         | build a `KernelManager` from `kernel_manager_class` |
|  [17]   | `reset_execution_trackers()`                                           | state reset        | reset timing and cell-index state                  |

[ENTRYPOINT_SCOPE]: output-hook and widget capture
- rail: notebook — `nbclient.NotebookClient`

The output-hook surface intercepts a running cell's display output by `msg_id`, enabling live widget-state capture without subclassing. `register_output_hook` pushes an `OutputWidget` (`nbclient.output_widget.OutputWidget`) onto the stack for one `msg_id`; the client routes `display_data`/`update_display_data`/comm messages to it. This is how `store_widget_state=True` populates the notebook's `widgets` metadata.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `register_output_hook(msg_id, hook)`               | hook push        | route a `msg_id`'s outputs to an `OutputWidget`      |
|  [02]   | `remove_output_hook(msg_id, hook)`                 | hook pop         | detach a previously registered output hook           |
|  [03]   | `output(outs, msg, display_id, cell_index)`        | output sink      | append a kernel output message to the cell's `outputs` |
|  [04]   | `clear_output(outs, msg, cell_index)`              | output clear     | apply a `clear_output` message to the cell           |
|  [05]   | `handle_comm_msg(outs, msg, cell_index)`           | comm router      | dispatch a Jupyter comm (widget) message             |
|  [06]   | `set_widgets_metadata()`                           | widget snapshot  | write captured widget state to notebook `metadata`   |

[ENTRYPOINT_SCOPE]: key configuration traits
- rail: notebook — `nbclient.NotebookClient`

| [INDEX] | [SURFACE]                  | [TYPE]     | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :--------- | :----------------------------------------------------------------- |
|  [01]   | `timeout`                  | `Int=None` | per-cell execution timeout in seconds (`None` = no limit)          |
|  [02]   | `startup_timeout`          | `Int=60`   | kernel start wait in seconds                                       |
|  [03]   | `kernel_name`              | `Unicode`  | kernel spec name override (`''` = notebook's own kernelspec)       |
|  [04]   | `allow_errors`             | `Bool=False` | continue execution past cell errors                              |
|  [05]   | `allow_error_names`        | `List`     | error names tolerated even when `allow_errors=False`               |
|  [06]   | `force_raise_errors`       | `Bool=False` | raise `CellExecutionError` even with `allow_errors`/`raises-exception` tag |
|  [07]   | `skip_cells_with_tag`      | `Unicode='skip-execution'` | skip cells bearing this metadata tag                |
|  [08]   | `record_timing`            | `Bool=True` | record per-cell timing in cell metadata                           |
|  [09]   | `store_widget_state`       | `Bool=True` | capture Jupyter widget state into notebook `metadata.widgets`     |
|  [10]   | `shutdown_kernel`          | `Enum='graceful'` | kernel shutdown policy (`graceful`/`immediate`)            |
|  [11]   | `interrupt_on_timeout`     | `Bool=False` | send kernel interrupt on cell timeout instead of failing         |
|  [12]   | `error_on_timeout`         | `Dict`     | fake `error` reply dict injected on timeout (`ename`/`evalue`/`traceback`) |
|  [13]   | `iopub_timeout`            | `Int=4`    | seconds to wait for iopub output after an idle reply               |
|  [14]   | `raise_on_iopub_timeout`   | `Bool=False` | raise instead of warn when iopub output is late                  |
|  [15]   | `coalesce_streams`         | `Bool=False` | merge consecutive stream outputs into one                        |
|  [16]   | `kernel_manager_class`     | `Type`     | `KernelManager` subclass `create_kernel_manager` instantiates      |
|  [17]   | `extra_arguments`          | `List`     | extra CLI args appended to the kernel launch                       |
|  [18]   | `display_data_priority`    | `List`     | MIME-type preference order for display output selection            |
|  [19]   | `on_notebook_start`        | `Callable` | hook before notebook execution begins (`fn(notebook=)`)            |
|  [20]   | `on_notebook_complete`     | `Callable` | hook when notebook execution finishes (`fn(notebook=)`)            |
|  [21]   | `on_notebook_error`        | `Callable` | hook when notebook execution raises (`fn(notebook=)`)              |
|  [22]   | `on_cell_start`            | `Callable` | hook before a cell is sent to the kernel (`fn(cell=, cell_index=)`)|
|  [23]   | `on_cell_execute`          | `Callable` | hook as a cell begins executing (`fn(cell=, cell_index=)`)         |
|  [24]   | `on_cell_executed`         | `Callable` | hook after a cell's execute reply (`fn(cell=, cell_index=, execute_reply=)`) |
|  [25]   | `on_cell_complete`         | `Callable` | hook after a cell fully completes (`fn(cell=, cell_index=)`)       |
|  [26]   | `on_cell_error`            | `Callable` | hook when a cell raises (`fn(cell=, cell_index=, execute_reply=)`) |

## [04]-[IMPLEMENTATION_LAW]

[NOTEBOOK_TOPOLOGY]:
- one-shot axis: `nbclient.execute(nb, cwd=, km=)` constructs a client, runs, and returns the mutated `NotebookNode` in one call — the campaign's default entry when no per-run trait control is needed.
- stateful axis: `NotebookClient(nb, km=, **traits)` -> `execute()` returns the mutated `NotebookNode` in-place; trait config (`timeout`, `allow_errors`, hooks, `store_widget_state`) is passed at construction, never a parallel runner subclass.
- context-manager axis: `with NotebookClient(nb).setup_kernel(): client.execute()` for explicit kernel lifecycle; `start_new_kernel`/`start_new_kernel_client` split the handshake when the client must outlive a single `execute`.
- async axis: every sync method is a `run_sync` wrapper; `async_execute`/`async_execute_cell`/`async_setup_kernel`/`async_wait_for_reply` are the authoritative forms in an async campaign and avoid the nested-event-loop hazard of the sync wrappers.
- output capture: `allow_errors=True` collects cell errors as output without raising; `allow_error_names` whitelists specific error names; `force_raise_errors=True` overrides per-execution; `coalesce_streams=True` merges adjacent stream output.
- timeout: `timeout` applies per-cell (`None` = unbounded); `startup_timeout` applies to the kernel start handshake; `interrupt_on_timeout=True` sends a kernel interrupt instead of failing; `error_on_timeout` is the fake-error reply dict (`ename`/`evalue`/`traceback`) injected as the cell result on timeout; `iopub_timeout`/`raise_on_iopub_timeout` bound the trailing output wait.
- hooks: `on_notebook_start`/`on_notebook_complete`/`on_notebook_error` and the per-cell `on_cell_start`/`on_cell_execute`/`on_cell_executed`/`on_cell_complete`/`on_cell_error` provide lifecycle callbacks without subclassing; the campaign threads progress/receipt capture through these, not a subclass override.
- widget state: `store_widget_state=True` (default) captures Jupyter widget state into `metadata.widgets` via the output-hook path (`register_output_hook` -> `OutputWidget` -> `set_widgets_metadata`) for UI replay.

[LOCAL_ADMISSION]:
- The `NotebookNode` object is the `nbformat` notebook in-memory form; the owner mutates it in place during execution.
- `km` constructor parameter accepts an externally managed `KernelManager`; when absent, the client builds one from `kernel_manager_class` and destroys it. `kernel_name=''` defers to the notebook's own kernelspec.
- `CellExecutionError` carries `ename`, `evalue`, and `traceback` from the kernel and is built via `from_cell_and_msg`; the error cell output is already embedded in the notebook. Because it subclasses `CellControlSignal`, the loop distinguishes it from the clean-end `CellExecutionComplete` signal.
- `DeadKernelError` requires kernel restart; it is not recoverable by the same `NotebookClient` instance.

[RAIL_LAW]:
- Package: `nbclient`
- Owns: Jupyter notebook kernel lifecycle, cell-by-cell execution, output/widget capture, timeout enforcement, and the lifecycle-hook dispatch that `nbconvert.ExecutePreprocessor` and `papermill` both compose
- Accept: `NotebookNode` from `nbformat`; `KernelManager`/`KernelClient` from `jupyter_client`; `papermill`-parameterized nodes for execution
- Reject: hand-rolled kernel protocol; a parallel notebook runner duplicating the execution loop the client already owns; a `NotebookClient` subclass where a lifecycle hook trait already carries the callback; a sync `execute` call inside a running event loop where `async_execute` is the correct form
