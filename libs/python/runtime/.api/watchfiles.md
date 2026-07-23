# [PY_RUNTIME_API_WATCHFILES]

`watchfiles` binds the Rust `notify` crate for filesystem change notification: a blocking `watch` and an anyio-backed `awatch` generator over one debounce/filter/polling schema, `Change`-classified batches, and process-restart reload drivers. It is runtime's filesystem-watch owner feeding the automation lanes; no `stat`-polling loop survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `watchfiles`
- package: `watchfiles` (`MIT`)
- module: `watchfiles`
- rail: automation
- depends-on: `anyio` (async backend over asyncio/trio)
- namespaces: `watchfiles`, `watchfiles.main`, `watchfiles.filters`, `watchfiles.run`, `watchfiles._rust_notify`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: change classification (`watchfiles.main`)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Change`     | `IntEnum`     | `added`=1, `modified`=2, `deleted`=3; `.raw_str()` -> member name |
|  [02]   | `FileChange` | type alias    | `tuple[Change, str]` (kind, absolute path)                        |

[PUBLIC_TYPE_SCOPE]: filter family (`watchfiles.filters`)

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `BaseFilter`                                              | filter base   | callable `(Change, str) -> bool`; configure via class attrs |
|  [02]   | `DefaultFilter(*, …ignore_dirs/patterns/paths)`           | filter        | dotfile/VCS/build ignore defaults, per-instance overridable |
|  [03]   | `PythonFilter(*, ignore_paths=None, extra_extensions=())` | filter        | keep only Python-source changes                             |

- `BaseFilter`: subclass sets `ignore_dirs`/`ignore_entity_patterns`/`ignore_paths` (regexes compiled once in `__init__`) or passes them to `DefaultFilter`; `PythonFilter` keeps only `.py`/`.pyx`/`.pyd` and `extra_extensions`.

[PUBLIC_TYPE_SCOPE]: low-level backend (`watchfiles._rust_notify`)

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `RustNotify(watch_paths, …flags)`                                | native handle | low-level watch thread; context manager        |
|  [02]   | `RustNotify.watch(debounce_ms, step_ms, timeout_ms, stop_event)` | native op     | batch `set`, or a stop/signal/timeout sentinel |
|  [03]   | `WatchfilesRustInternalError`                                    | fault         | unknown native `notify` error (`RuntimeError`) |

- `RustNotify(...)`: spawns the watch thread on construction, raising `FileNotFoundError` for a missing path; `watch`/`awatch` own the handle.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: watch operations (`watchfiles.main`)
- watch/awatch carry: `*paths`, `watch_filter`, `debounce`, `step`, `stop_event`, `rust_timeout`, `yield_on_timeout`, `debug`, `force_polling`, `poll_delay_ms`, `recursive`, `ignore_permission_denied`; `watch` adds `raise_interrupt`. `watch_filter=None` keeps every change; `force_polling`/`debug`/`poll_delay_ms`/`ignore_permission_denied` each fall back to a `WATCHFILES_*` env var. `awatch.stop_event` is an `AnyEvent` (`anyio.Event | asyncio.Event | trio.Event`); `watch.stop_event` is any `is_set() -> bool` object.

| [INDEX] | [SURFACE]                                      | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :--------------------------------------------- | :-------- | :-------------------------------- |
|  [01]   | `watch(*paths, …schema, raise_interrupt=True)` | generator | blocking `set[FileChange]` stream |
|  [02]   | `awatch(*paths, …schema)`                      | async gen | async `set[FileChange]` stream    |

[ENTRYPOINT_SCOPE]: reload drivers (`watchfiles.run`)
- reload drivers carry: `*paths`, `target`, `args`, `kwargs`, `target_type='auto'`, `callback`, `watch_filter`, `grace_period`, `debounce`, `step`, `debug`, `recursive`, `ignore_permission_denied`; `run_process` adds `sigint_timeout=5`/`sigkill_timeout=1`. `target_type='auto'` resolves via `detect_target_type` to `'function'` (a subprocess-run callable) or `'command'` (a `.py`/`.sh`/shell command via `subprocess.Popen`); the target reads the change list from `WATCHFILES_CHANGES` (JSON `[raw_str, path]` pairs).

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `run_process(*paths, target, …schema)`  | function | run+restart a process on change; returns reload count   |
|  [02]   | `arun_process(*paths, target, …schema)` | async fn | async run+restart driver; awaits a coroutine `callback` |
|  [03]   | `run.detect_target_type(target)`        | function | classify `target` as `'function'`/`'command'`           |
|  [04]   | `run.import_string(dotted_path)`        | function | import a dotted callable for `target_type='function'`   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every filesystem watch folds through one `awatch` (or blocking `watch`) generator over the Rust `notify` backend; the consumer reads one `set[FileChange]` per debounce/step batch and matches on the `Change` enum, never a per-event callback, a `stat` loop, or an integer/string kind, and serialised forms use `Change.raw_str()`.
- `force_polling` (env `WATCHFILES_FORCE_POLLING`, auto on WSL) is the one fallback for filesystems without native events, tuned by `poll_delay_ms`.

[STACKING]:
- `anyio`(`.api/anyio.md`): `awatch` runs each Rust poll on `anyio.to_thread.run_sync` inside an `anyio.create_task_group()` it cancels per batch (`tg.cancel_scope.cancel()`); the automation lane owns the enclosing cancel scope and stops iteration through an `anyio.Event` `stop_event`.
- `apscheduler`(`.api/apscheduler.md`): the scheduler owner consumes the `set[FileChange]` stream as a trigger source over the one `AsyncIOScheduler` rather than re-polling.
- runtime automation: change batches feed the receipt surface via `Change.raw_str()` + path slots, never ad hoc logging.

[LOCAL_ADMISSION]:
- one declared `watch_filter` (a `DefaultFilter`/`PythonFilter`/`BaseFilter` subclass, a raw `Callable`, or `None` to keep all) owns ignore rules; per-event path-string filtering in the consumer is deleted.
- process-restart workflows use `arun_process` with a typed `target`/`target_type`; the SIGTERM->`KeyboardInterrupt` handler and `sigint_timeout`/`sigkill_timeout` escalation are the restart contract, so `docker compose stop` exits cleanly.
- this is a local filesystem watch, not a job framework or scheduler service.

[RAIL_LAW]:
- Package: `watchfiles`
- Owns: filesystem change watching over the Rust `notify` backend, `Change` classification, ignore filters, force-polling fallback, the `RustNotify` thread handle, and process-restart reload drivers
- Accept: `awatch` under the anyio lane with an `anyio.Event` `stop_event`, `Change`-enum matching, one declared `watch_filter` (or `None`), `debounce`/`step` batching, `arun_process` reloads with typed `target_type` and `grace_period`/`sigint_timeout`/`sigkill_timeout` control
- Reject: `stat` polling loops, integer/string-kind comparison, per-event consumer filtering, direct `RustNotify` use where `watch`/`awatch` fits, and a second process supervisor
