# [PY_ARTIFACTS_API_NBCLIENT]

`nbclient` supplies the Jupyter notebook execution runtime for the artifacts notebook rail: `NotebookClient` owns the full kernel lifecycle, cell-by-cell execution, timeout enforcement, and cell error handling; `execute` / `async_execute` are the primary coroutine-aware entrypoints; the exceptions family (`CellExecutionError`, `CellTimeoutError`, `DeadKernelError`) carries typed failure rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbclient`
- package: `nbclient`
- import: `nbclient`
- owner: `artifacts`
- rail: notebook
- asset: runtime library

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and kernel family
- rail: notebook — `nbclient`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                   |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------- |
|   [1]   | `NotebookClient` | execution client | kernel lifecycle, cell execution, timeout, hooks, widget state |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `nbclient.exceptions`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :---------------------- | :---------------- | :------------------------------------------------------------ |
|   [1]   | `CellControlSignal`     | control base      | base for cell-level execution signals                         |
|   [2]   | `CellExecutionComplete` | completion signal | raised to end cell execution loop                             |
|   [3]   | `CellExecutionError`    | cell fault        | cell raised an exception; carries traceback and output        |
|   [4]   | `CellTimeoutError`      | timeout fault     | cell execution exceeded `timeout`; inherits `TimeoutError`    |
|   [5]   | `DeadKernelError`       | kernel fault      | kernel process died during execution; inherits `RuntimeError` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution
- rail: notebook — `nbclient.NotebookClient`

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]     | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------- | :----------------- | :------------------------------------------------- |
|   [1]   | `NotebookClient(nb, km=None, **kw)`                                    | constructor        | bind a `NotebookNode` to a kernel manager          |
|   [2]   | `execute(reset_kc=False, **kwargs)`                                    | sync execute       | run all cells; returns modified `NotebookNode`     |
|   [3]   | `async_execute(reset_kc=False, **kwargs)`                              | async execute      | coroutine variant; returns modified `NotebookNode` |
|   [4]   | `execute_cell(cell, cell_index, execution_count, store_history)`       | sync cell          | execute a single cell; returns `NotebookNode`      |
|   [5]   | `async_execute_cell(cell, cell_index, execution_count, store_history)` | async cell         | coroutine cell execution                           |
|   [6]   | `setup_kernel(**kwargs)`                                               | sync ctx manager   | context manager; start kernel, yield, shutdown     |
|   [7]   | `async_setup_kernel(**kwargs)`                                         | async ctx manager  | async context manager variant                      |
|   [8]   | `start_new_kernel(**kwargs)`                                           | sync kernel start  | start a fresh kernel outside context manager       |
|   [9]   | `async_start_new_kernel(**kwargs)`                                     | async kernel start | coroutine variant                                  |
|  [10]   | `start_new_kernel_client()`                                            | sync client start  | start kernel client after kernel is running        |
|  [11]   | `async_start_new_kernel_client()`                                      | async client start | coroutine variant                                  |
|  [12]   | `process_message(msg, cell, cell_index)`                               | message handler    | route a single kernel message to outputs           |
|  [13]   | `wait_for_reply(msg_id, cell)`                                         | sync wait          | block until kernel idle reply                      |
|  [14]   | `async_wait_for_reply(msg_id, cell)`                                   | async wait         | coroutine variant                                  |
|  [15]   | `create_kernel_manager()`                                              | km factory         | build a `KernelManager` from client config         |
|  [16]   | `reset_execution_trackers()`                                           | state reset        | reset timing and cell-index state                  |

[ENTRYPOINT_SCOPE]: key configuration traits
- rail: notebook — `nbclient.NotebookClient`

| [INDEX] | [SURFACE]              | [TYPE]     | [CAPABILITY]                                        |
| :-----: | :--------------------- | :--------- | :-------------------------------------------------- |
|   [1]   | `timeout`              | `Int`      | per-cell execution timeout in seconds               |
|   [2]   | `startup_timeout`      | `Int`      | kernel start wait in seconds                        |
|   [3]   | `kernel_name`          | `Unicode`  | kernel spec name override                           |
|   [4]   | `allow_errors`         | `Bool`     | continue execution past cell errors                 |
|   [5]   | `force_raise_errors`   | `Bool`     | raise `CellExecutionError` even with `allow_errors` |
|   [6]   | `skip_cells_with_tag`  | `Unicode`  | skip cells bearing this metadata tag                |
|   [7]   | `record_timing`        | `Bool`     | record per-cell timing in cell metadata             |
|   [8]   | `shutdown_kernel`      | `Enum`     | kernel shutdown policy after execution              |
|   [9]   | `on_cell_execute`      | `Callable` | hook called before each cell executes               |
|  [10]   | `on_cell_executed`     | `Callable` | hook called after each cell completes               |
|  [11]   | `on_notebook_start`    | `Callable` | hook called before notebook execution begins        |
|  [12]   | `on_notebook_complete` | `Callable` | hook called when notebook execution finishes        |
|  [13]   | `on_notebook_error`    | `Callable` | hook called when notebook execution raises          |
|  [14]   | `coalesce_streams`     | `Bool`     | merge consecutive stream outputs                    |
|  [15]   | `interrupt_on_timeout` | `Bool`     | send kernel interrupt on cell timeout               |

## [4]-[IMPLEMENTATION_LAW]

[NOTEBOOK_TOPOLOGY]:
- primary axis: `NotebookClient(nb)` -> `execute()` returns the mutated `NotebookNode` in-place
- context manager axis: `with NotebookClient(nb).setup_kernel(): client.execute()` for explicit kernel lifecycle
- async axis: `async_execute` / `async_execute_cell` / `async_setup_kernel` are the preferred forms in async execution contexts
- output capture: `allow_errors=True` collects cell errors as output without raising; `force_raise_errors=True` overrides per-execution
- timeout: `timeout` applies per-cell; `startup_timeout` applies to the kernel start handshake; `error_on_timeout` is per-cell-index
- hooks: `on_cell_execute`, `on_cell_executed`, `on_notebook_start`, `on_notebook_complete`, `on_notebook_error` provide lifecycle callbacks without subclassing
- widget state: `store_widget_state=True` captures Jupyter widget state in notebook metadata for UI replay

[LOCAL_ADMISSION]:
- The `NotebookNode` object is the `nbformat` notebook in-memory form; the owner mutates it in place during execution.
- `km` constructor parameter accepts an externally managed `KernelManager`; when absent, the client creates and destroys one.
- `CellExecutionError` carries `ename`, `evalue`, and `traceback` from the kernel; the error cell output is already embedded in the notebook.
- `DeadKernelError` requires kernel restart; it is not recoverable by the same `NotebookClient` instance.

[RAIL_LAW]:
- Package: `nbclient`
- Owns: Jupyter notebook kernel lifecycle, cell-by-cell execution, output capture, timeout enforcement, and hook dispatch
- Accept: `NotebookNode` from `nbformat`; `KernelManager` from `jupyter_client`
- Reject: hand-rolled kernel protocol; parallel notebook runners that duplicate the execution loop the client already owns
