# [PY_BRANCH_API_ANYIO]

`anyio` mints one backend-agnostic structured-concurrency layer over asyncio and Trio; every concurrent, networked, or offloaded effect the boundary tier runs folds through its task groups, cancel scopes, and streams instead of a raw event loop, so domain code never imports a backend.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- module: `anyio`
- license: MIT
- asset: runtime library
- rail: concurrency
- namespaces: `anyio`, `anyio.abc`, `anyio.lowlevel`, `anyio.streams.{memory,buffered,text,tls,file,stapled}`, `anyio.to_thread`, `anyio.from_thread`, `anyio.to_process`, `anyio.to_interpreter`, `anyio.pytest_plugin`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: task, cancellation, and run lifecycle
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                                                       |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `CancelScope`         | scope class   | scoped cancellation boundary (`shield=`)                                                     |
|  [02]   | `TaskInfo`            | info record   | running task identity (`id`/`name`/`coro`)                                                   |
|  [03]   | `TaskHandle`          | handle class  | spawned-task handle; `.return_value`/`.start_value`/`.status`/`.wait`/`.cancel`/`.exception` |
|  [04]   | `TASK_STATUS_IGNORED` | sentinel      | default `task_status` for non-reporting tasks                                                |
|  [05]   | `TaskCancelled`       | exception     | task was cancelled                                                                           |
|  [06]   | `TaskFailed`          | exception     | task raised an exception                                                                     |
|  [07]   | `TaskNotFinished`     | exception     | result read before task done                                                                 |
|  [08]   | `RunFinishedError`    | exception     | portal/loop call after the event loop closed                                                 |

[PUBLIC_TYPE_SCOPE]: synchronization primitives
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Event`                     | event class   | set-once async notification                              |
|  [02]   | `Lock`                      | lock class    | async mutual exclusion (`fast_acquire=`)                 |
|  [03]   | `Semaphore`                 | sema class    | async counting semaphore                                 |
|  [04]   | `Condition`                 | cond class    | async condition variable                                 |
|  [05]   | `CapacityLimiter`           | limiter class | concurrent slot limiter (`total_tokens`)                 |
|  [06]   | `ResourceGuard`             | guard class   | reject concurrent use of a single-reader/writer resource |
|  [07]   | `EventStatistics`           | stats record  | event subscriber count snapshot                          |
|  [08]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot                              |
|  [09]   | `ConditionStatistics`       | stats record  | condition waiter snapshot                                |
|  [10]   | `SemaphoreStatistics`       | stats record  | semaphore slot snapshot                                  |
|  [11]   | `CapacityLimiterStatistics` | stats record  | limiter borrower/waiter snapshot                         |

[PUBLIC_TYPE_SCOPE]: networking and connectables
- rail: concurrency

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]  | [RAIL]                                                |
| :-----: | :---------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `abc.SocketStream`                                          | byte stream    | connected TCP/UNIX bidirectional stream               |
|  [02]   | `abc.SocketListener`                                        | listener       | single-socket accept loop                             |
|  [03]   | `abc.UNIXSocketStream`                                      | byte stream    | UNIX-domain stream with fd-passing support            |
|  [04]   | `abc.UDPSocket`                                             | datagram       | unconnected UDP socket                                |
|  [05]   | `abc.ConnectedUDPSocket`                                    | datagram       | UDP socket bound to one peer                          |
|  [06]   | `abc.UNIXDatagramSocket`                                    | datagram       | unconnected UNIX datagram socket                      |
|  [07]   | `abc.ConnectedUNIXDatagramSocket`                           | datagram       | UNIX datagram bound to one peer                       |
|  [08]   | `abc.Listener`                                              | interface      | accept-loop connection listener                       |
|  [09]   | `abc.SocketAttribute`                                       | typed-attr set | `local_address`/`remote_address`/`raw_socket` lookups |
|  [10]   | `TCPConnectable`                                            | connectable    | reusable TCP connect target (`as_connectable`)        |
|  [11]   | `UNIXConnectable`                                           | connectable    | reusable UNIX connect target                          |
|  [12]   | `abc.ByteStreamConnectable` / `abc.ObjectStreamConnectable` | connectable    | reconnectable byte/object stream factory              |

[PUBLIC_TYPE_SCOPE]: streams and stream wrappers
- rail: concurrency

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]      | [RAIL]                                      |
| :-----: | :-------------------------------------------------------------------- | :----------------- | :------------------------------------------ |
|  [01]   | `streams.memory.MemoryObjectSendStream` / `MemoryObjectReceiveStream` | object stream      | in-process fan-out/fan-in channel pair      |
|  [02]   | `streams.memory.MemoryObjectStreamStatistics`                         | stats record       | buffered/waiting item snapshot              |
|  [03]   | `streams.tls.TLSStream`                                               | byte stream        | TLS wrapper over any `ByteStream`           |
|  [04]   | `streams.tls.TLSListener`                                             | listener           | TLS-terminating accept loop                 |
|  [05]   | `streams.tls.TLSAttribute`                                            | typed-attr set     | peer cert / cipher / ALPN protocol lookups  |
|  [06]   | `streams.buffered.BufferedByteReceiveStream`                          | byte stream        | `receive_exactly`/`receive_until` framing   |
|  [07]   | `streams.text.TextReceiveStream` / `TextSendStream` / `TextStream`    | object stream      | codec-decoding stream wrapper               |
|  [08]   | `streams.stapled.StapledByteStream` / `StapledObjectStream`           | byte/object stream | staple send + receive into one duplex       |
|  [09]   | `streams.stapled.MultiListener`                                       | listener           | fan multiple listeners into one accept loop |
|  [10]   | `streams.file.FileReadStream` / `FileWriteStream`                     | byte stream        | stream a file as a `ByteStream`             |
|  [11]   | `abc.ObjectStream` / `ObjectReceiveStream` / `ObjectSendStream`       | interface          | typed object channel contracts              |
|  [12]   | `abc.ByteStream` / `ByteReceiveStream` / `ByteSendStream`             | interface          | bidirectional/half byte stream contracts    |

[PUBLIC_TYPE_SCOPE]: async filesystem and tempfiles
- rail: concurrency

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [RAIL]                                              |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `AsyncFile`            | async file      | thread-offloaded file object                        |
|  [02]   | `Path`                 | async path      | `pathlib.Path` mirror, all I/O offloaded to threads |
|  [03]   | `TemporaryFile`        | context manager | async temporary file                                |
|  [04]   | `NamedTemporaryFile`   | context manager | async named temporary file                          |
|  [05]   | `TemporaryDirectory`   | context manager | async temporary directory                           |
|  [06]   | `SpooledTemporaryFile` | context manager | async in-memory-then-spill temp file                |

[PUBLIC_TYPE_SCOPE]: thread / process / interpreter bridges
- rail: concurrency

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]  | [RAIL]                                                 |
| :-----: | :----------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `abc.BlockingPortal`                 | interface      | call async code from a worker thread                   |
|  [02]   | `from_thread.BlockingPortalProvider` | portal factory | one event loop shared by many threads                  |
|  [03]   | `abc.Process`                        | process handle | async subprocess (`stdin`/`stdout`/`stderr` streams)   |
|  [04]   | `BrokenWorkerProcess`                | worker error   | `to_process` worker raised or died                     |
|  [05]   | `BrokenWorkerInterpreter`            | worker error   | `to_interpreter` PEP-734 subinterpreter raised or died |

[PUBLIC_TYPE_SCOPE]: typed-attribute introspection and context mixins
- rail: concurrency

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `TypedAttributeProvider`                           | mixin         | base for streams exposing `extra(attr)`                      |
|  [02]   | `TypedAttributeSet`                                | attr group    | declarative typed-attribute namespace                        |
|  [03]   | `typed_attribute()`                                | descriptor    | declare one typed attribute on a set                         |
|  [04]   | `TypedAttributeLookupError`                        | lookup error  | attribute not provided by the stream                         |
|  [05]   | `ContextManagerMixin` / `AsyncContextManagerMixin` | mixin         | author `__enter__`/`__aenter__` as a single generator method |

[PUBLIC_TYPE_SCOPE]: error and stream-signal types
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                         |
| :-----: | :-------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `EndOfStream`         | stream signal  | stream exhausted (raised by `receive`)         |
|  [02]   | `ClosedResourceError` | resource error | operation on a closed resource                 |
|  [03]   | `BrokenResourceError` | resource error | unrecoverable stream/socket break              |
|  [04]   | `BusyResourceError`   | resource error | concurrent operation in progress               |
|  [05]   | `WouldBlock`          | sync signal    | non-blocking op cannot proceed (`send_nowait`) |
|  [06]   | `DelimiterNotFound`   | read error     | delimiter absent within max bytes              |
|  [07]   | `IncompleteRead`      | read error     | EOF before expected byte count                 |
|  [08]   | `ConnectionFailed`    | net error      | connection open failed                         |
|  [09]   | `NoEventLoopError`    | thread error   | no running event loop for the captured token   |

[PUBLIC_TYPE_SCOPE]: backend and ABC contracts
- rail: concurrency

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :------------------ | :------------ | :-------------------------------------------------- |
|  [01]   | `abc.TaskGroup`     | interface     | structured task group contract                      |
|  [02]   | `abc.TaskStatus`    | interface     | `started(value)` readiness signal for `task_status` |
|  [03]   | `abc.AsyncResource` | interface     | `aclose`-capable resource                           |
|  [04]   | `abc.AsyncBackend`  | interface     | pluggable backend dispatch contract                 |
|  [05]   | `abc.TestRunner`    | interface     | test-fixture event-loop runner                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event loop entry, tasks, and backend query
- rail: concurrency

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :---------------------------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `run(func, *args, backend='asyncio', backend_options=None)` | event loop entry | run async main in chosen backend            |
|  [02]   | `create_task_group()`                                       | task group       | structured concurrency scope (`async with`) |
|  [03]   | `get_current_task()`                                        | introspection    | current `TaskInfo`                          |
|  [04]   | `get_running_tasks()`                                       | introspection    | list of all running `TaskInfo`              |
|  [05]   | `get_cancelled_exc_class()`                                 | backend query    | backend-specific cancellation class         |
|  [06]   | `get_all_backends()` / `get_available_backends()`           | backend query    | declared vs importable backend names        |
|  [07]   | `wait_all_tasks_blocked()`                                  | testing helper   | yield until all tasks are suspended         |

[ENTRYPOINT_SCOPE]: cancel scopes and timing
- rail: concurrency

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :----------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `fail_after(delay, shield=False)`    | cancel scope   | raise `TimeoutError` on deadline                       |
|  [02]   | `move_on_after(delay, shield=False)` | cancel scope   | silently cancel on deadline (`scope.cancelled_caught`) |
|  [03]   | `sleep(delay)`                       | sleep          | sleep for seconds (cancellation point)                 |
|  [04]   | `sleep_forever()`                    | sleep          | sleep until cancelled                                  |
|  [05]   | `sleep_until(deadline)`              | sleep          | sleep until absolute monotonic time                    |
|  [06]   | `current_time()`                     | clock          | backend monotonic time                                 |
|  [07]   | `current_effective_deadline()`       | clock          | nearest active cancel deadline                         |

[ENTRYPOINT_SCOPE]: memory streams
- rail: concurrency

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------ |
|  [01]   | `create_memory_object_stream[T](max_buffer_size=0, item_type=None)` | factory        | paired `(send, receive)` channel pair |

[ENTRYPOINT_SCOPE]: networking, TLS, and signals
- rail: concurrency
- Connect/listen/datagram factories carry a keyword-only tuning tail past `*`; the row shows the distinguishing head.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `connect_tcp(host, port, *, tls=False, ...)`                 | connect        | TCP `SocketStream`/`TLSStream`, RFC 6555 dual-stack |
|  [02]   | `connect_unix(path)`                                         | connect        | UNIX-domain `UNIXSocketStream`                      |
|  [03]   | `create_tcp_listener(*, local_port=0, backlog=65536, ...)`   | listen         | `MultiListener[SocketStream]`                       |
|  [04]   | `create_unix_listener(path, *, mode=None, backlog=65536)`    | listen         | UNIX `SocketListener`                               |
|  [05]   | `create_udp_socket(family=AF_UNSPEC, *, local_port=0, ...)`  | datagram       | unconnected `UDPSocket`                             |
|  [06]   | `create_connected_udp_socket(remote_host, remote_port, ...)` | datagram       | peer-bound `ConnectedUDPSocket`                     |
|  [07]   | `create_unix_datagram_socket(*, local_path=None, ...)`       | datagram       | unconnected UNIX datagram socket                    |
|  [08]   | `create_connected_unix_datagram_socket(remote_path, ...)`    | datagram       | peer-bound UNIX datagram socket                     |
|  [09]   | `getaddrinfo(host, port, *, family=0, type=0, ...)`          | resolve        | async DNS resolution                                |
|  [10]   | `getnameinfo(sockaddr, flags=0)`                             | resolve        | async reverse DNS resolution                        |
|  [11]   | `as_connectable(obj)`                                        | connectable    | normalize into a reusable connect target            |
|  [12]   | `open_signal_receiver(*signals)`                             | signals        | sync `with` → async iterator of received signals    |
|  [13]   | `wait_readable(obj)` / `wait_writable(obj)`                  | readiness      | await fd readiness without owning the loop          |
|  [14]   | `wait_socket_readable(sock)` / `wait_socket_writable(sock)`  | readiness      | await socket readiness without owning the loop      |
|  [15]   | `notify_closing(obj)`                                        | readiness      | wake readiness waiters before closing an fd         |
|  [16]   | `aclose_forcefully(resource)`                                | teardown       | best-effort `aclose` inside a shielded scope        |

[ENTRYPOINT_SCOPE]: file and process I/O
- rail: concurrency

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `open_file(file, mode='r', ...)`                       | file open      | async file handle (`AsyncFile`)           |
|  [02]   | `wrap_file(file)`                                      | file wrap      | wrap an open sync file object as async    |
|  [03]   | `open_process(command, *, stdin, stdout, stderr, ...)` | process open   | async `Process` with stream handles       |
|  [04]   | `run_process(command, *, input=None, check=True, ...)` | process run    | run subprocess, return `CompletedProcess` |
|  [05]   | `mkstemp(...)` / `mkdtemp(...)`                        | tempfile       | async temp file/dir path mirrors          |
|  [06]   | `gettempdir()` / `gettempdirb()`                       | tempfile       | async temp-dir path mirrors               |

[ENTRYPOINT_SCOPE]: thread / process / interpreter offload
- rail: concurrency

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `to_thread.run_sync(func, *args, abandon_on_cancel=False, ...)`      | thread         | run sync callable in a worker thread         |
|  [02]   | `to_thread.current_default_thread_limiter()`                         | limiter        | per-loop default thread limiter (40 tokens)  |
|  [03]   | `from_thread.run(func, *args)` / `from_thread.run_sync(func, *args)` | bridge         | call async/sync code from a worker thread    |
|  [04]   | `from_thread.start_blocking_portal(backend='asyncio', ...)`          | portal         | spin up a loop thread + `BlockingPortal` CM  |
|  [05]   | `from_thread.check_cancelled()`                                      | bridge         | raise if the portal task was cancelled       |
|  [06]   | `to_process.run_sync(func, *args, cancellable=False, ...)`           | process        | run sync in a worker subprocess (pickle hop) |
|  [07]   | `to_process.current_default_process_limiter()`                       | limiter        | per-loop default process limiter (CPU count) |
|  [08]   | `to_interpreter.run_sync(func, *args, limiter=None)`                 | interp         | run sync in a subinterpreter (in-process)    |
|  [09]   | `to_interpreter.current_default_interpreter_limiter()`               | limiter        | per-loop default subinterpreter limiter      |

[ENTRYPOINT_SCOPE]: low-level checkpoints and run-locals
- rail: concurrency

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :-------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `lowlevel.checkpoint()`                 | checkpoint     | unconditional yield + cancellation check             |
|  [02]   | `lowlevel.checkpoint_if_cancelled()`    | checkpoint     | raise only if cancel scope tripped                   |
|  [03]   | `lowlevel.cancel_shielded_checkpoint()` | checkpoint     | yield shielded from cancellation                     |
|  [04]   | `lowlevel.RunVar(name, default)`        | run-local var  | per-event-loop `ContextVar` equivalent (`get`/`set`) |
|  [05]   | `lowlevel.current_token()`              | backend query  | opaque token identifying the running loop            |
|  [06]   | `lowlevel.get_async_backend()`          | backend query  | current running backend instance                     |

## [04]-[IMPLEMENTATION_LAW]

[ANYIO_TOPOLOGY]:
- `run(backend=...)` or the `anyio_backend` pytest fixture selects `asyncio` (default) or `trio`; domain code imports neither backend module.
- Task groups enforce structured concurrency: every `tg.start_soon`/`tg.start` child finishes before the `async with` exits, and child failures aggregate into a `BaseExceptionGroup` caught with `except*`.
- `tg.start(func, ...)` blocks until the child calls `task_status.started(value)` and returns that value; `return_handle=True` yields the `TaskHandle` whose `.start_value` carries the handshake. `tg.start_soon` runs no handshake and returns `TaskHandle[T]` — `.return_value` reads the terminal after the block closes, `.status` its closed `TaskHandle.Status`.
- `CancelScope(deadline=, shield=)` is the cancellation primitive `fail_after`/`move_on_after` wrap; cancellation is level-triggered, so a scope stays cancelled and a swallowed cancellation re-raises at the next checkpoint unless shielded.
- `create_memory_object_stream[T](...)` is subscripted for the item type; `send_nowait`/`receive_nowait` raise `WouldBlock`, and a send onto a closed receive end raises `BrokenResourceError`.
- TLS rides `connect_tcp(..., tls=True, tls_hostname=...)` inline or `streams.tls.TLSStream.wrap(...)` over any `ByteStream`; peer cert and ALPN read through `stream.extra(TLSAttribute.*)`.
- Offload splits by cost: `to_thread.run_sync` bounds a `CapacityLimiter` thread pool (40 default), `to_process.run_sync` crosses a persistent subprocess by pickle, `to_interpreter.run_sync` dispatches a PEP-734 subinterpreter (own GIL, in-process) where only PEP-734-shareable values cross copy-free — a non-shareable payload still pickles on the hop, so serialization cost is a lane-selection input. A worker raise or death surfaces as `BrokenWorkerProcess`/`BrokenWorkerInterpreter`.
- `from_thread.BlockingPortalProvider(backend, backend_options)` shares one loop across many threads; `portal.call`/`portal.start_task_soon`/`portal.wrap_async_context_manager` bridge sync callers in, raising `RunFinishedError` after the loop closes.
- `stream.extra(attr)` is the one polymorphic surface for socket, TLS, and file metadata; an owner-local wrapper declares a `TypedAttributeSet`.

[STACKING]:
- `grpcio`(`.api/grpcio.md`): `grpc.aio` servers and channels enter via `anyio.run(serve, backend='asyncio')`, every blocking servicer body bounded through `to_thread.run_sync(..., limiter=...)`; per-call deadlines map onto `fail_after`/`move_on_after`, sync entrypoints bridge through a `BlockingPortalProvider`.
- `httpx`: an `httpx.AsyncClient` call takes an `anyio` deadline via `fail_after`, and one `create_task_group` fans concurrent requests so a single failure cancels siblings and aggregates as an `ExceptionGroup`.
- `msgspec`/`pydantic`(`.api/msgspec.md`, `.api/pydantic.md`): `BufferedByteReceiveStream.receive_until(b'\n', max_bytes)` or `receive_exactly(n)` frames a raw `ByteStream`, the frame feeding `msgspec.json.Decoder(type=T).decode(...)` for a zero-copy validated wire decode; a `MemoryObjectReceiveStream[T]` typed on a `msgspec.Struct` moves validated objects between tasks with no re-serialization.
- `stamina`: a `connect_tcp`/`run_process`/`to_thread.run_sync` effect wraps in `stamina.retry_context(...)` whose retry sleep IS an `anyio.sleep` checkpoint, so an enclosing `move_on_after` still preempts a retry storm.
- `structlog`/`opentelemetry`(`.api/structlog.md`, `.api/opentelemetry-sdk.md`): a `RunVar` (or a per-task-propagated `contextvars.ContextVar`) carries the trace context across task-group hops and `from_thread` bridges; one OTel span per task, opened inside the child coroutine.
- `expression`(`.api/expression.md`): an offloaded `to_thread.run_sync` return lifts into `Result` via `result.of_option`/a try-builder at the boundary adapter, so a `BrokenWorkerProcess`/`TimeoutError` never escapes raw.
- `loky`(`.api/loky.md`): `get_reusable_executor().submit(...)` returns a blocking `concurrent.futures` future; `to_thread.run_sync(future.result, abandon_on_cancel=True, limiter=...)` bridges its wait off the loop, so the `WorkerPool` COOPERATIVE-`PROCESS` arm never stalls the event loop.
- `pebble`(`.api/pebble.md`): `ProcessPool.schedule(fn, timeout=)` owns the wall-clock kill anyio cancellation cannot express against a native C loop; its `ProcessFuture.result` bridges through `to_thread.run_sync(..., abandon_on_cancel=True, limiter=...)`.
- `cloudpickle`(`.api/cloudpickle.md`): `to_interpreter.run_sync` is the PEP-734 subinterpreter pickle boundary — an ordinary closure crosses only because `Kernel.of` pre-reduced it with `cloudpickle.dumps`; the process crossings ride the `loky`/`pebble` pools.

[LOCAL_ADMISSION]:
- Offload by workload: `to_thread.run_sync` for CPU-light blocking I/O, `to_interpreter.run_sync` for CPU-bound shareable-payload work, `to_process.run_sync` for process isolation or GIL-hostile native calls — each with an explicit `CapacityLimiter` per bounded subsystem.
- `lowlevel.checkpoint` in a tight loop or polling body keeps cancellation and fairness live.

[RAIL_LAW]:
- Package: `anyio`
- Owns: structured concurrency, cancel scopes, memory streams, networking + TLS, file/process async I/O, thread/process/subinterpreter offload, blocking-portal bridges, sync primitives, signal receivers, typed-attribute introspection
- Accept: `create_task_group`, `fail_after`/`move_on_after`, `connect_tcp`/listeners, `to_thread`/`to_process`/`to_interpreter.run_sync` with explicit `CapacityLimiter`, `create_memory_object_stream`, `BlockingPortalProvider`, `open_signal_receiver`
- Reject: direct `asyncio.gather`/`create_task`/`wait_for`, raw `socket`/`signal`/`subprocess`/`time.sleep`/`ThreadPoolExecutor`/`ProcessPoolExecutor`, swallowing cancellation without a shielded scope
