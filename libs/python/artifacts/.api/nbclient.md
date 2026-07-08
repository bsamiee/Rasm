# [PY_ARTIFACTS_API_NBCLIENT]

`nbclient` supplies the Jupyter notebook execution runtime for the artifacts notebook rail: `NotebookClient` owns the full kernel lifecycle, cell-by-cell execution, timeout enforcement, output-hook capture, and cell error handling. The module-level `nbclient.execute(nb, cwd=None, km=None, **kw)` is the one-shot convenience entry that constructs and runs a client in a single call (when `cwd` is set it injects `resources["metadata"]["path"]=cwd` and forwards every other `**kw` as a `NotebookClient` trait); `NotebookClient.async_execute` is the authoritative coroutine entrypoint and `execute` its `run_sync` wrapper (every sync method on the client is a `run_sync(async_…)` trampoline, so the `async_` mirror always carries the real signature). The exceptions family (`CellExecutionError`, `CellTimeoutError`, `DeadKernelError`, plus the `CellControlSignal`/`CellExecutionComplete` loop signals) carries typed failure rails.

Integration into the `document/report#NBCLIENT_ENGINE` owner is the binding contract: `_notebook_arm` constructs `NotebookClient(parameterized, **NotebookEngine.client_kwargs()).async_execute()` and `await`s it directly on the structured-concurrency loop — host-free, with NO `anyio.to_thread` offload, because the kernel handshake is already non-blocking; only the downstream blocking `nbconvert` render crosses `anyio.to_thread.run_sync(limiter=_OFFLOAD)`, so nbclient is the one rail member that composes the `anyio` event loop natively rather than through a worker thread. The `parameterized` node arrives from `papermill.parameterize.parameterize_notebook` (upstream parameter injection) over a `jupytext.reads` source; the executed `nbformat.NotebookNode` is mutated in place, archived via `jupytext.writes(…, "ipynb")`, then handed to `nbconvert.get_exporter(…).from_notebook_node` (downstream lowering) — nbclient owns the execution loop those two compose, never a parallel runner (`nbconvert.ExecutePreprocessor` is itself only a thin wrapper over this same client and is left `enabled=False` to avoid double execution). Every cell/timeout/kernel fault raised here funnels through the `runtime/reliability/faults#FAULT` `async_boundary` into a `BoundaryFault` on the one `RuntimeRail = Result[T, BoundaryFault]` (`expression`), so the consumer reads a typed rail value, never an escaped `try`/`except` in domain flow.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbclient`
- package: `nbclient`
- import: `nbclient`
- owner: `artifacts`
- rail: notebook
- installed: `0.11.0` (verified, pure-py wheel, runs on the cp315 interpreter)
- license: BSD-3-Clause
- deps: `nbformat` (notebook model — supplies the `NotebookNode` mutated in place), `jupyter_client` (kernel protocol — `KernelManager`/`KernelClient`), `traitlets` (`LoggingConfigurable` base + every config trait), `jupyter_core`
- exports: `__all__ = ["__version__", "version_info", "NotebookClient", "execute"]` — ONLY `NotebookClient` and `execute` are re-exported at the package root; the exception family imports from `nbclient.exceptions` and `OutputWidget` from `nbclient.output_widget`
- asset: pure-Python runtime library; the executing surface is `NotebookClient`, the `execute` one-shot, and the `nbclient.exceptions` fault family

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and one-shot entry
- rail: notebook — `nbclient`

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `NotebookClient` | execution client | `LoggingConfigurable`; kernel lifecycle, cell execution, timeout, hooks, widget state |
| [02] | `execute` | module function | `execute(nb, cwd=None, km=None, **kw) -> NotebookNode` one-shot construct-and-run |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `nbclient.exceptions`

`CellControlSignal(Exception)` is the loop-control base (a normal `Exception`, not an error); `CellExecutionComplete` and `CellExecutionError` both subclass it, and `CellTimeoutError(TimeoutError, CellControlSignal)` subclasses it AS WELL AS stdlib `TimeoutError` — the execution loop catches `CellControlSignal` to distinguish a clean cell end (`CellExecutionComplete`) from a fault, while the `TimeoutError` base makes a timeout also catchable as a stdlib timeout. `DeadKernelError(RuntimeError)` is the one fault OUTSIDE the `CellControlSignal` hierarchy. `CellExecutionError` is a structured, picklable fault: `__init__(traceback, ename, evalue)` (note the `traceback`-first positional order) sets the three attributes, `__reduce__` makes it cross-process safe, and `__str__` returns the traceback (or `"{ename}: {evalue}"`); the loop builds it from the kernel `error` reply via `from_cell_and_msg(cell, msg)`. `CellTimeoutError.error_from_timeout_and_cell(msg, timeout, cell)` is the parallel classmethod that builds a timeout fault carrying a previewed cell-source snippet.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `CellControlSignal` | control base | base loop signal (`<- Exception`); caught to branch loop flow |
| [02] | `CellExecutionComplete` | completion signal | raised to end cell execution loop (`<- CellControlSignal`) |
| [03] | `CellExecutionError` | cell fault | cell raised; attrs `traceback`/`ename`/`evalue`; `<- CellControlSignal`; built via `from_cell_and_msg`; picklable via `reduce` |
| [04] | `CellTimeoutError` | timeout fault | cell exceeded `timeout`; `<- (TimeoutError, CellControlSignal)`; built via `error_from_timeout_and_cell` |
| [05] | `DeadKernelError` | kernel fault | kernel process died during execution; `<- RuntimeError` (sole non-`CellControlSignal` fault) |

[ENTRYPOINT_SCOPE]: fault constructors
- rail: notebook — `nbclient.exceptions`

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `CellExecutionError(traceback, ename, evalue)` | constructor | direct construct (`traceback`-first positional order) |
| [02] | `CellExecutionError.from_cell_and_msg(cell, msg: dict)` | reply builder | build from a cell + kernel `execute_reply`/`error` reply dict; folds stream outputs into the message |
| [03] | `CellTimeoutError.error_from_timeout_and_cell(msg, timeout: int, cell)` | timeout builder | build a timeout fault with a previewed cell-source snippet |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution
- rail: notebook — `nbclient` / `nbclient.NotebookClient`

Sync rows `[03]`/`[05]`/`[09]`/`[11]`/`[14]` are `run_sync` wrappers auto-generated from the `async_` mirror beneath them; the async form carries the authoritative signature and is the form the report rail composes (`async_execute`). `[01]` is the module-level one-shot; `[02]` is the constructor. The constructor accepts `resources` (the dict the one-shot seeds with `metadata.path` from `cwd`).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `nbclient.execute(nb, cwd=None, km=None, **kwargs) -> NotebookNode` | one-shot function | construct a client, run all cells, return modified `NotebookNode`; `cwd` seeds `resources["metadata"]["path"]` |
| [02] | `NotebookClient(nb: NotebookNode, km: KernelManager \| None = None, **kw)` | constructor | bind a `NotebookNode` to a kernel manager; config via traits and a `resources=` dict |
| [03] | `execute(reset_kc=False, **kwargs) -> NotebookNode` | sync execute | run all cells; returns modified `NotebookNode` |
| [04] | `async_execute(reset_kc=False, **kwargs) -> NotebookNode` | async execute | the rail's entrypoint; runs all cells on the loop; returns modified `NotebookNode` |
| [05] | `execute_cell(cell, cell_index, execution_count=None, store_history=True) -> NotebookNode` | sync cell | execute a single cell; mutates `cell` in place |
| [06] | `async_execute_cell(cell: NotebookNode, cell_index: int, execution_count=None, store_history=True) -> NotebookNode` | async cell | coroutine single-cell execution; mutates `cell` in place |
| [07] | `setup_kernel(**kwargs)` | sync ctx manager | `@contextmanager`; start kernel, yield, shutdown |
| [08] | `async_setup_kernel(**kwargs)` | async ctx manager | `@asynccontextmanager`; registers SIGINT/SIGTERM + `atexit` cleanup and fires `on_notebook_complete`/`on_notebook_error` around the yield |
| [09] | `start_new_kernel(**kwargs)` | sync kernel start | start a fresh kernel outside the context manager |
| [10] | `async_start_new_kernel(**kwargs) -> None` | async kernel start | coroutine variant; populates `self.km` |
| [11] | `start_new_kernel_client() -> KernelClient` | sync client start | start + connect the kernel client after the kernel runs |
| [12] | `async_start_new_kernel_client() -> KernelClient` | async client start | coroutine variant; sets `self.kc` |
| [13] | `process_message(msg: dict, cell, cell_index) -> NotebookNode \| None` | message handler | route a single kernel message to outputs; mutates `cell.outputs` in place |
| [14] | `wait_for_reply(msg_id, cell=None)` / alias `_wait_for_reply` | sync wait | block until the kernel idle reply; `_wait_for_reply` is the back-compat name papermill calls |
| [15] | `async_wait_for_reply(msg_id: str, cell=None) -> dict[str, Any] \| None` | async wait | coroutine variant; returns the matching shell reply or `None` on timeout |
| [16] | `create_kernel_manager() -> KernelManager` | km factory | build a `KernelManager` from `kernel_manager_class` |
| [17] | `reset_execution_trackers() -> None` | state reset | reset timing and cell-index state |

[ENTRYPOINT_SCOPE]: output-hook and widget capture
- rail: notebook — `nbclient.NotebookClient`

The output-hook surface intercepts a running cell's display output by `msg_id`, enabling live widget-state capture without subclassing. `register_output_hook` pushes an `OutputWidget` (`nbclient.output_widget.OutputWidget`) onto a per-`msg_id` stack (last registered wins, mirroring JupyterLab's `registerMessageHook`); the client routes `display_data`/`update_display_data`/comm messages to it. `on_comm_open_jupyter_widget` is the comm-open handler that instantiates an `OutputWidget` from the widget registry on a `comm_open` whose target is a Jupyter widget. This is the path `store_widget_state=True` drives to populate `metadata.widgets`. `OutputWidget` itself exposes `output`/`clear_output`/`set_state`/`sync_state`/`handle_msg`/`send` — the rail never subclasses it; it is the runtime sink the client manages.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `register_output_hook(msg_id: str, hook: OutputWidget)` | hook push | push an `OutputWidget` onto the `msg_id` stack |
| [02] | `remove_output_hook(msg_id: str, hook: OutputWidget)` | hook pop | pop a previously registered output hook (asserts identity) |
| [03] | `output(outs, msg, display_id, cell_index) -> NotebookNode \| None` | output sink | append a kernel output message to the cell's `outputs` |
| [04] | `clear_output(outs, msg, cell_index)` | output clear | apply a `clear_output` message to the cell |
| [05] | `clear_display_id_mapping(cell_index: int)` | display reset | clear the cell's `display_id -> output` index |
| [06] | `handle_comm_msg(outs, msg, cell_index)` | comm router | dispatch a Jupyter comm (widget) message |
| [07] | `on_comm_open_jupyter_widget(msg) -> Any \| None` | comm-open hook | build an `OutputWidget` from the widget registry on widget `comm_open` |
| [08] | `set_widgets_metadata()` | widget snapshot | write captured widget state to `metadata.widgets` |

[ENTRYPOINT_SCOPE]: key configuration traits
- rail: notebook — `nbclient.NotebookClient`

| [INDEX] | [SURFACE] | [TRAIT_TYPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `timeout` | `Integer=None` | per-cell execution timeout in seconds (`None` = no limit) |
| [02] | `startup_timeout` | `Integer=60` | kernel start wait in seconds |
| [03] | `kernel_name` | `Unicode=''` | kernel spec name override (`''` = notebook's own kernelspec) |
| [04] | `allow_errors` | `Bool=False` | continue execution past cell errors |
| [05] | `allow_error_names` | `List` | error names tolerated even when `allow_errors=False` |
| [06] | `force_raise_errors` | `Bool=False` | raise `CellExecutionError` even with `allow_errors`/`raises-exception` tag |
| [07] | `skip_cells_with_tag` | `Unicode='skip-execution'` | skip cells bearing this metadata tag |
| [08] | `record_timing` | `Bool=True` | record per-cell timing in cell metadata |
| [09] | `store_widget_state` | `Bool=True` | capture Jupyter widget state into notebook `metadata.widgets` |
| [10] | `shutdown_kernel` | `Enum('graceful'\|'immediate')='graceful'` | kernel shutdown policy |
| [11] | `interrupt_on_timeout` | `Bool=False` | send kernel interrupt on cell timeout instead of failing |
| [12] | `error_on_timeout` | `Dict=None` | fake `error` reply dict injected on timeout (`ename`/`evalue`/`traceback`) |
| [13] | `iopub_timeout` | `Integer=4` | seconds to wait for iopub output after an idle reply |
| [14] | `raise_on_iopub_timeout` | `Bool=False` | raise instead of warn when iopub output is late |
| [15] | `coalesce_streams` | `Bool=False` | merge consecutive stream outputs into one |
| [16] | `shell_timeout_interval` | `Integer=5` | shell-channel poll granularity (seconds) the reply loop waits per `get_msg` |
| [17] | `kernel_manager_class` | `Type(klass=KernelManager)` | `KernelManager` subclass `create_kernel_manager` instantiates |
| [18] | `extra_arguments` | `List(Unicode)` | extra CLI args appended to the kernel launch |
| [19] | `ipython_hist_file` | `Unicode=':memory:'` | IPython history-file path appended as a launch arg (`:memory:` = ephemeral) |
| [20] | `display_data_priority` | `List` | MIME-type preference order for display output selection |

[ENTRYPOINT_SCOPE]: lifecycle-hook traits (kept OFF the reproducible plan)
- rail: notebook — `nbclient.NotebookClient`

These eight `Callable` traits are real per-run observation channels nbclient invokes around the notebook and per-cell lifecycle (the loop fires them through `run_hook`/`run_sync(run_hook)`, so a hook may be sync or async). The `document/report#NBCLIENT_ENGINE` owner deliberately keeps EVERY one OFF the frozen `NotebookEngine`: a `Callable` is not a serializable reproducibility fact the content key can key, so admitting a hook does smuggle a non-serializable runtime capability onto the keyed plan. Progress/receipt capture is harvested off the stepped owner's `self.fact` via the `@receipted` weave and the `runtime` observability owner's spans, NEVER threaded through these hooks. They are documented as the surface a non-reproducible interactive consumer (not this rail) does use.

| [INDEX] | [SURFACE] | [TRAIT_TYPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `on_notebook_start` | `Callable` | hook before notebook execution begins (`fn(notebook=)`) |
| [02] | `on_notebook_complete` | `Callable` | hook when notebook execution finishes (`fn(notebook=)`) |
| [03] | `on_notebook_error` | `Callable` | hook when notebook execution raises (`fn(notebook=)`) |
| [04] | `on_cell_start` | `Callable` | hook before a cell is sent to the kernel (`fn(cell=, cell_index=)`) |
| [05] | `on_cell_execute` | `Callable` | hook as a cell begins executing (`fn(cell=, cell_index=)`) |
| [06] | `on_cell_executed` | `Callable` | hook after a cell's execute reply (`fn(cell=, cell_index=, execute_reply=)`) |
| [07] | `on_cell_complete` | `Callable` | hook after a cell fully completes (`fn(cell=, cell_index=)`) |
| [08] | `on_cell_error` | `Callable` | hook when a cell raises (`fn(cell=, cell_index=, execute_reply=)`) |

## [04]-[IMPLEMENTATION_LAW]

[NOTEBOOK_TOPOLOGY]:
- async axis (the rail form): `NotebookClient(parameterized, **NotebookEngine.client_kwargs()).async_execute()` awaited directly on the `anyio` loop is the one form `document/report#NBCLIENT_ENGINE` composes; it returns the mutated in-memory `NotebookNode`. Every sync method is a `run_sync(async_…)` trampoline, so a sync `execute` inside a running event loop is the nested-loop hazard `async_execute`/`async_execute_cell`/`async_setup_kernel`/`async_wait_for_reply` exist to avoid.
- boundary projection: the frozen `NotebookEngine` (a `msgspec.Struct`) is NOT passed as traits piecemeal — `client_kwargs()` is the one boundary view that `asdict`s the struct and re-projects the single map field `error_on_timeout` from its immutable `frozendict` to a real `dict` (or the client's `None` default), because a `traitlets.Dict` rejects a `frozendict` (no `dict` subclass); the tuple-typed fields (`allow_error_names`/`display_data_priority`/`extra_arguments`) ride as-is because `traitlets.List` coerces a tuple. A new bounded-safety trait is one struct field carried by `client_kwargs` with zero call-site edit.
- one-shot axis: `nbclient.execute(nb, cwd=, km=)` constructs a client, runs, returns the mutated `NotebookNode` in one call (seeding `resources["metadata"]["path"]` from `cwd`) — the convenience entry the rail does NOT use, because the async boundary needs `async_execute` and the keyed plan owns its own traits.
- context-manager axis: `with NotebookClient(nb).setup_kernel(): client.execute()` (or `async_setup_kernel`) for explicit kernel lifecycle when the client must outlive one `execute`; `start_new_kernel`/`start_new_kernel_client` split the handshake. `async_setup_kernel` additionally installs SIGINT/SIGTERM + `atexit` cleanup so an interrupted long run shuts the kernel down.
- bounded-safety posture (the rail's chosen defaults): `force_raise_errors=True` + `interrupt_on_timeout=True` + `raise_on_iopub_timeout=True` make a runaway cell raise rather than hang and a late iopub raise rather than silently warn; `coalesce_streams=True` merges adjacent stream output; `record_timing=True` stamps per-cell timing into the node metadata; `allow_errors=False` (with `allow_error_names` as the narrow whitelist) keeps a failing cell a hard fault.
- timeout: `timeout` applies per-cell (`None` = unbounded; the rail pins 600); `startup_timeout` bounds the kernel start handshake; `error_on_timeout` is the fake-error reply dict (`ename`/`evalue`/`traceback`) injected as the cell result on timeout; `iopub_timeout`/`raise_on_iopub_timeout` bound the trailing output wait; `shell_timeout_interval` is the shell-poll granularity the reply loop ticks on.
- lifecycle hooks stay OFF: the eight `on_*` `Callable` traits are real but the keyed plan admits none — a `Callable` is not a content-keyable reproducibility fact, so receipt/progress capture is the `@receipted` harvest off `self.fact` plus the `runtime` observability span, never a hook. Documented for completeness, not composed by this rail.
- widget state: `store_widget_state=True` (default) captures Jupyter widget state into `metadata.widgets` via the output-hook path (`on_comm_open_jupyter_widget` -> `register_output_hook` -> `OutputWidget` -> `set_widgets_metadata`) for UI replay.

[LOCAL_ADMISSION]:
- The `NotebookNode` is the `nbformat` in-memory form; the owner mutates it in place during execution — the rail archives the executed node via `jupytext.writes(executed, "ipynb")` and lowers it via `nbconvert.get_exporter(...).from_notebook_node(executed)`, never re-reading from disk.
- `km` constructor parameter accepts an externally managed `KernelManager`; when absent, the client builds one from `kernel_manager_class` and destroys it. `kernel_name=''` defers to the notebook's own kernelspec; the rail pins `kernel_name="python3"` and threads the same value into `parameterize_notebook` so a non-Python kernel routes its own `papermill` translator.
- exception-rail stacking (the load-bearing seam onto the universal `expression` rail): every fault raised here is caught by the `document/report` owner's `async_boundary("report.compose", ...)` and folded through `runtime/reliability/faults#FAULT` `_convert` into a `BoundaryFault` on `RuntimeRail = Result[T, BoundaryFault]`. The `CLASSIFY` table routes by exception family, and because `CellTimeoutError` subclasses stdlib `TimeoutError` it lands on the `deadline` row (carrying the unknown-budget floor), while `CellExecutionError` (`<- Exception`) and `DeadKernelError` (`<- RuntimeError`) land on the `boundary` catch-all — so the rail NEVER catches nbclient's faults itself, it lets the one boundary fold own the conversion and the OTel span annotation.
- `CellExecutionError` carries `traceback`/`ename`/`evalue` from the kernel and is built via `from_cell_and_msg(cell, msg)`; the error cell output is already embedded in the node, and the fault is picklable (`__reduce__`) so it survives a process boundary. `DeadKernelError` requires a kernel restart; it is not recoverable by the same `NotebookClient` instance.

[RAIL_LAW]:
- Package: `nbclient`
- Owns: Jupyter notebook kernel lifecycle, cell-by-cell execution, output/widget capture, timeout enforcement, and the typed-fault family (`CellExecutionError`/`CellTimeoutError`/`DeadKernelError`) that `nbconvert.ExecutePreprocessor` and `papermill` both compose over — nbclient is the execution loop, never a parallel runner.
- Accept: `NotebookNode` from `nbformat` (mutated in place); `KernelManager`/`KernelClient` from `jupyter_client`; a `papermill.parameterize_notebook`-injected node for execution; bounded-safety traits projected through `NotebookEngine.client_kwargs()`.
- Stack: the executed node feeds `nbconvert` downstream and arrives from `papermill`/`jupytext` upstream; faults funnel through `runtime/reliability/faults#FAULT` `async_boundary` onto the `expression` `Result` rail; the in-memory `async_execute` composes the `anyio` loop natively (no `to_thread`) while only the downstream blocking `nbconvert` render crosses `anyio.to_thread.run_sync(limiter=_OFFLOAD)`.
- Reject: hand-rolled kernel protocol; a parallel notebook runner duplicating the execution loop; a `NotebookClient` subclass where a trait or `client_kwargs` projection already carries the config; admitting an `on_*` lifecycle hook onto the keyed plan where a `Callable` is not a serializable reproducibility fact; a sync `execute` call inside the running event loop where `async_execute` is the correct form; a local `try`/`except` around `async_execute` where the `async_boundary` fold owns the conversion onto the rail.
