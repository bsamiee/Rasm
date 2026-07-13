# [PY_RUNTIME_API_WATCHFILES]

`watchfiles` (release, MIT) binds the Rust `notify` crate (native `_rust_notify` extension; vendored CycloneDX SBOM) for filesystem change notification: a blocking `watch` generator and an anyio-backed `awatch` async generator sharing one debounce/step/timeout/filter/polling keyword schema, a `Change` IntEnum classifying each event, configurable `BaseFilter`/`DefaultFilter`/`PythonFilter` callables, the low-level `RustNotify` context-manager backend, and `run_process`/`arun_process` reload drivers that restart a function or command on change. It is the runtime owner for filesystem watches feeding the automation lanes; no `stat`-polling loop survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `watchfiles`
- package: `watchfiles`
- version: `1.2.0`
- license: MIT
- import: `watchfiles`
- owner: `runtime`
- rail: automation
- depends-on: `anyio` (async backend; supports asyncio and trio event loops)
- namespaces: `watchfiles`, `watchfiles.main`, `watchfiles.filters`, `watchfiles.run`, `watchfiles._rust_notify`
- capability: sync/async filesystem watches over the Rust `notify` backend, debounce/step batching, `Change` classification, ignore filters, force-polling fallback, the low-level `RustNotify` thread handle, and process-restart reload drivers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: change classification (`watchfiles.main`)
- rail: automation
- `Change` is an `IntEnum`; each yielded batch is a `set[FileChange]` where `FileChange = tuple[Change, str]` (the change kind and the absolute path string). Dispatch on the enum member, never the integer value or a string kind. `Change.raw_str()` returns the lowercase member name (`'added'`/`'modified'`/`'deleted'`) — the form the reload drivers serialise into `WATCHFILES_CHANGES`.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                                                            |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Change`     | `IntEnum`     | `added`=1, `modified`=2, `deleted`=3; `.raw_str()` -> member name |
|  [02]   | `FileChange` | type alias    | `tuple[Change, str]` (kind, absolute path)                        |

[PUBLIC_TYPE_SCOPE]: filter family (`watchfiles.filters`)
- rail: automation
- `BaseFilter` is a configurable callable `(change: Change, path: str) -> bool` returning whether to keep the change. Subclass and set the class attributes `ignore_dirs`, `ignore_entity_patterns` (regex strings compiled once in `__init__`), `ignore_paths`, or pass them to the `DefaultFilter` constructor. `DefaultFilter` ships dotfile/VCS/build defaults (`__pycache__`, `.git`, `.venv`, `node_modules`, `.mypy_cache`, ... and `\.py[cod]$`, `~$`, `^\.DS_Store$`, ...). `PythonFilter` extends `DefaultFilter` to keep only `.py`/`.pyx`/`.pyd` changes plus `extra_extensions`. A raw `Callable[[Change, str], bool]` is also accepted in `watch_filter=`; `watch_filter=None` keeps every change.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :-------------------------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `BaseFilter`                                              | filter base   | callable `(Change, str) -> bool`; configure via class attrs |
|  [02]   | `DefaultFilter(*, …ignore_dirs/patterns/paths)`           | filter        | dotfile/VCS/build ignore defaults, per-instance overridable |
|  [03]   | `PythonFilter(*, ignore_paths=None, extra_extensions=())` | filter        | keep only Python-source changes                             |

[PUBLIC_TYPE_SCOPE]: low-level backend (`watchfiles._rust_notify`)
- rail: automation
- `RustNotify` is the Rust thread handle wrapped by `watch`/`awatch`; its constructor takes `watch_paths` plus the `flags` `debug`/`force_polling`/`poll_delay_ms`/`recursive`/`ignore_permission_denied`, spawns the watch thread immediately (raises `FileNotFoundError` for a missing path), and `RustNotify.watch(debounce_ms, step_ms, timeout_ms, stop_event)` returns either a `set[(int, str)]` raw-change set or one of the sentinel strings `'signal'`/`'stop'`/`'timeout'`. It is a context manager (`__enter__`/`__exit__` -> `close`). `WatchfilesRustInternalError` (a `RuntimeError`) surfaces an unknown native error. Consumers use `watch`/`awatch`; `RustNotify` is the seam for a bespoke driver only.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :--------------------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `RustNotify(watch_paths, …flags)`                                | native handle | low-level watch thread; context manager        |
|  [02]   | `RustNotify.watch(debounce_ms, step_ms, timeout_ms, stop_event)` | native op     | batch `set`, or a stop/signal/timeout sentinel |
|  [03]   | `WatchfilesRustInternalError`                                    | fault         | unknown native `notify` error (`RuntimeError`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: watch operations (`watchfiles.main`)
- rail: automation
- `watch` (blocking generator) and `awatch` (async generator) share the schema: `*paths`, `watch_filter=DefaultFilter()` (or `None` to keep all), `debounce=1600` (max ms to group a batch), `step=50` (ms to wait for more changes), `stop_event=None`, `rust_timeout` (ms in Rust per poll; `watch` default `5000`, `awatch` default `None` -> `1000` on win32 / `5000` elsewhere), `yield_on_timeout=False`, `debug=None` (or `WATCHFILES_DEBUG`), `force_polling=None` (auto, or `WATCHFILES_FORCE_POLLING`; auto-enabled on WSL), `poll_delay_ms=300` (or `WATCHFILES_POLL_DELAY_MS`), `recursive=True`, `ignore_permission_denied=None` (or `WATCHFILES_IGNORE_PERMISSION_DENIED`). `watch` additionally takes `raise_interrupt=True`; `awatch`'s `raise_interrupt` is deprecated (a `KeyboardInterrupt` cancels the coroutine and re-raises at `anyio.run`/`asyncio.run`). `awatch.stop_event` is an `AnyEvent = anyio.Event | asyncio.Event | trio.Event`; `watch.stop_event` is any object with `is_set() -> bool`. Each iteration yields `set[FileChange]`.

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `watch(*paths, …schema, raise_interrupt=True)` | watch          | blocking `set[FileChange]` generator                            |
|  [02]   | `awatch(*paths, …schema)`                      | watch          | async `set[FileChange]` generator; `raise_interrupt` deprecated |

[ENTRYPOINT_SCOPE]: reload drivers (`watchfiles.run`)
- rail: automation
- `run_process`/`arun_process` watch `*paths` and restart `target` on each debounced batch. `target_type='auto'` resolves via `detect_target_type` to `'function'` (an importable dotted callable, or a non-string callable, run in a `spawn`-context subprocess) or `'command'` (a `.py`/`.sh`/shell command run via `subprocess.Popen`). `callback` receives the change set on each reload (`arun_process` awaits a coroutine callback); the target reads the change list from the `WATCHFILES_CHANGES` env var (JSON of `[raw_str, path]` pairs). `grace_period` seconds delay watching after first start. `run_process` restart signalling is `sigint_timeout`/`sigkill_timeout` seconds (SIGINT then SIGKILL); both drivers register a SIGTERM->KeyboardInterrupt handler so `docker compose stop` exits cleanly. Both return the final reload count; `arun_process` is the async mirror (no `sigint_timeout`/`sigkill_timeout`/`raise_interrupt`). Both share the `schema` `args=()`/`kwargs=None`/`target_type='auto'`/`callback=None`/`watch_filter=DefaultFilter()`/`grace_period=0`/`debounce=1600`/`step=50`/`debug=None`/`recursive=True`/`ignore_permission_denied=False`; `run_process` adds `sigint_timeout=5`/`sigkill_timeout=1`.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :-------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `run_process(*paths, target, …schema)`  | reload         | run+restart a process on change; returns reload count |
|  [02]   | `arun_process(*paths, target, …schema)` | reload         | async run+restart driver; awaits coroutine `callback` |
|  [03]   | `run.detect_target_type(target)`        | reload         | classify `target` as `'function'`/`'command'`         |
|  [04]   | `run.import_string(dotted_path)`        | reload         | import a dotted callable for `target_type='function'` |

## [04]-[IMPLEMENTATION_LAW]

[AUTOMATION_TOPOLOGY]:
- watch law: filesystem watching is one `awatch` async generator under the anyio lane, stopped through an `AnyEvent` `stop_event` (or the lane's cancel scope); no polling loop with `stat` comparison.
- backend law: change detection is the Rust `notify` backend; `force_polling=True` (or `WATCHFILES_FORCE_POLLING`, auto on WSL) is the deliberate fallback for filesystems without native events, tuned by `poll_delay_ms`, never a hand-rolled poll. The `RustNotify` handle is owned by `watch`/`awatch`; a consumer never spawns it directly except to build a bespoke driver.
- classification law: change handling matches on the `Change` enum (`added`/`modified`/`deleted`), never the integer value or a string kind; serialised forms use `Change.raw_str()`.
- batching law: `debounce`/`step` set the batch window; the consumer reads one `set[FileChange]` per iteration rather than per-event callbacks. `rust_timeout`/`yield_on_timeout` govern idle wakeups.
- filter law: ignore rules are a `DefaultFilter`/`PythonFilter` or a `BaseFilter` subclass passed once via `watch_filter` (or a raw `Callable`, or `None` to keep all); per-event path-string filtering in the consumer is deleted.
- reload law: process-restart workflows use `arun_process` with a typed `target`/`target_type`; the SIGTERM->KeyboardInterrupt handler and `sigint_timeout`/`sigkill_timeout` escalation are the restart contract, and the runtime owns no second process supervisor.

[LOCAL_ADMISSION]:
- the automation lane composes `awatch` over an `anyio` cancel scope (`.api/anyio.md`): the lane owns the cancel scope, capacity, and `stop_event` (`anyio.Event`); watchfiles owns the change stream. The async generator is iterated under the same structured-concurrency lane as the rest of the runtime's async work, and `awatch` already runs each Rust poll on `anyio.to_thread.run_sync` inside a task group it cancels per batch.
- change batches feed the receipt surface as a watch signal (`set[FileChange]` -> receipt fact via `Change.raw_str()` + path slots), never ad hoc logging; the scheduler/automation owner (`.api/apscheduler.md`) consumes the watch stream as a trigger rather than re-polling.
- this is a local filesystem watch, not a job framework or scheduler service.

[RAIL_LAW]:
- Package: `watchfiles`
- Owns: filesystem change watching over the Rust `notify` backend, `Change` classification, ignore filters, force-polling fallback, the `RustNotify` thread handle, and process-restart reload drivers
- Accept: `awatch` under the anyio lane with an `anyio.Event` `stop_event`, `Change`-enum matching, a single declared `watch_filter` (or `None`), `debounce`/`step` batching, `arun_process` reloads with typed `target_type`, `grace_period`/`sigint_timeout`/`sigkill_timeout` restart control
- Reject: `stat` polling loops, integer/string-kind comparison, per-event consumer filtering, a hand-rolled poll where `force_polling` fits, direct `RustNotify` use where `watch`/`awatch` fits, a second process supervisor, the deprecated `awatch(raise_interrupt=...)`
