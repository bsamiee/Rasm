# [PY_BRANCH_API_ANYIO]

`anyio` supplies a backend-agnostic structured concurrency layer over asyncio and Trio: task groups, cancel scopes, memory object streams, networking (TCP/UNIX/UDP/TLS), async file and process I/O, thread/process/subinterpreter offload, blocking-portal bridges, synchronization primitives, signal receivers, and typed-attribute stream introspection — one stable API the boundary tier composes for every concurrent, networked, or offloaded effect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- module: `anyio`
- version-floor: `>=4.14.0`
- license: MIT
- wheel: pure-python (`py3-none-any`), no ABI tag; resolves clean on cp315
- marker: none (core dependency; already transitive via `httpx`/`grpcio` async stacks)
- asset: runtime library
- rail: concurrency
- namespaces: `anyio`, `anyio.abc`, `anyio.lowlevel`, `anyio.streams.{memory,buffered,text,tls,file,stapled}`, `anyio.to_thread`, `anyio.from_thread`, `anyio.to_process`, `anyio.to_interpreter`, `anyio.pytest_plugin`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: task, cancellation, and run lifecycle
- rail: concurrency

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :---------------- | :------------ | :------------------------------------------- |
|  [01]   | `CancelScope`     | scope class   | scoped cancellation boundary (`shield=`)     |
|  [02]   | `TaskInfo`        | info record   | running task identity (`id`/`name`/`coro`)   |
|  [03]   | `TaskHandle`      | handle class  | handle to a spawned task; `.result()`        |
|  [04]   | `TASK_STATUS_IGNORED` | sentinel  | default `task_status` for non-reporting tasks |
|  [05]   | `TaskCancelled`   | exception     | task was cancelled                           |
|  [06]   | `TaskFailed`      | exception     | task raised an exception                     |
|  [07]   | `TaskNotFinished` | exception     | result read before task done                 |
|  [08]   | `RunFinishedError`| exception     | portal/loop call after the event loop closed |

[PUBLIC_TYPE_SCOPE]: synchronization primitives
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------- | :------------ | :------------------------------ |
|  [01]   | `Event`                     | event class   | set-once async notification     |
|  [02]   | `Lock`                      | lock class    | async mutual exclusion (`fast_acquire=`) |
|  [03]   | `Semaphore`                 | sema class    | async counting semaphore        |
|  [04]   | `Condition`                 | cond class    | async condition variable        |
|  [05]   | `CapacityLimiter`           | limiter class | concurrent slot limiter (`total_tokens`) |
|  [06]   | `ResourceGuard`             | guard class   | reject concurrent use of a single-reader/writer resource |
|  [07]   | `EventStatistics`           | stats record  | event subscriber count snapshot |
|  [08]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot     |
|  [09]   | `ConditionStatistics`       | stats record  | condition waiter snapshot       |
|  [10]   | `SemaphoreStatistics`       | stats record  | semaphore slot snapshot         |
|  [11]   | `CapacityLimiterStatistics` | stats record  | limiter borrower/waiter snapshot |

[PUBLIC_TYPE_SCOPE]: networking and connectables
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------ | :------------- | :-------------------------------------------- |
|  [01]   | `abc.SocketStream`        | byte stream    | connected TCP/UNIX bidirectional stream       |
|  [02]   | `abc.SocketListener`      | listener       | single-socket accept loop                     |
|  [03]   | `abc.UNIXSocketStream`    | byte stream    | UNIX-domain stream with fd-passing support    |
|  [04]   | `abc.UDPSocket`           | datagram       | unconnected UDP socket                        |
|  [05]   | `abc.ConnectedUDPSocket`  | datagram       | UDP socket bound to one peer                  |
|  [06]   | `abc.UNIXDatagramSocket`  | datagram       | unconnected UNIX datagram socket              |
|  [07]   | `abc.ConnectedUNIXDatagramSocket` | datagram | UNIX datagram bound to one peer              |
|  [08]   | `abc.Listener`            | interface      | accept-loop connection listener               |
|  [09]   | `abc.SocketAttribute`     | typed-attr set | `local_address`/`remote_address`/`raw_socket` lookups |
|  [10]   | `TCPConnectable`          | connectable    | reusable TCP connect target (`as_connectable`) |
|  [11]   | `UNIXConnectable`         | connectable    | reusable UNIX connect target                  |
|  [12]   | `abc.ByteStreamConnectable` / `abc.ObjectStreamConnectable` | connectable | reconnectable byte/object stream factory |

[PUBLIC_TYPE_SCOPE]: streams and stream wrappers
- rail: concurrency

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `streams.memory.MemoryObjectSendStream` / `MemoryObjectReceiveStream` | object stream | in-process fan-out/fan-in channel pair |
|  [02]   | `streams.memory.MemoryObjectStreamStatistics` | stats record | buffered/waiting item snapshot         |
|  [03]   | `streams.tls.TLSStream`               | byte stream     | TLS wrapper over any `ByteStream`            |
|  [04]   | `streams.tls.TLSListener`             | listener        | TLS-terminating accept loop                  |
|  [05]   | `streams.tls.TLSAttribute`            | typed-attr set  | peer cert / cipher / ALPN protocol lookups   |
|  [06]   | `streams.buffered.BufferedByteReceiveStream` | byte stream | `receive_exactly`/`receive_until` framing    |
|  [07]   | `streams.text.TextReceiveStream` / `TextSendStream` / `TextStream` | object stream | codec-decoding stream wrapper      |
|  [08]   | `streams.stapled.StapledByteStream` / `StapledObjectStream` | byte/object stream | staple a send + receive half into one duplex |
|  [09]   | `streams.stapled.MultiListener`       | listener        | fan multiple listeners into one accept loop  |
|  [10]   | `streams.file.FileReadStream` / `FileWriteStream` | byte stream | stream a file as a `ByteStream`           |
|  [11]   | `abc.ObjectStream` / `ObjectReceiveStream` / `ObjectSendStream` | interface | typed object channel contracts        |
|  [12]   | `abc.ByteStream` / `ByteReceiveStream` / `ByteSendStream` | interface | bidirectional/half byte stream contracts |

[PUBLIC_TYPE_SCOPE]: async filesystem and tempfiles
- rail: concurrency

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [RAIL]                            |
| :-----: | :--------------------- | :-------------- | :-------------------------------- |
|  [01]   | `AsyncFile`            | async file      | thread-offloaded file object      |
|  [02]   | `Path`                 | async path      | `pathlib.Path` mirror, all I/O offloaded to threads |
|  [03]   | `TemporaryFile`        | context manager | async temporary file              |
|  [04]   | `NamedTemporaryFile`   | context manager | async named temporary file        |
|  [05]   | `TemporaryDirectory`   | context manager | async temporary directory         |
|  [06]   | `SpooledTemporaryFile` | context manager | async in-memory-then-spill temp file |

[PUBLIC_TYPE_SCOPE]: thread / process / interpreter bridges
- rail: concurrency

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [RAIL]                                         |
| :-----: | :---------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `abc.BlockingPortal`                | interface       | call async code from a worker thread           |
|  [02]   | `from_thread.BlockingPortalProvider`| portal factory  | one event loop shared by many threads          |
|  [03]   | `abc.Process`                       | process handle  | async subprocess (`stdin`/`stdout`/`stderr` streams) |
|  [04]   | `BrokenWorkerProcess`               | worker error    | `to_process` worker raised or died             |
|  [05]   | `BrokenWorkerInterpreter`           | worker error    | `to_interpreter` PEP-734 subinterpreter raised or died |

[PUBLIC_TYPE_SCOPE]: typed-attribute introspection and context mixins
- rail: concurrency

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `TypedAttributeProvider`       | mixin         | base for streams exposing `extra(attr)`      |
|  [02]   | `TypedAttributeSet`            | attr group    | declarative typed-attribute namespace        |
|  [03]   | `typed_attribute()`            | descriptor    | declare one typed attribute on a set         |
|  [04]   | `TypedAttributeLookupError`    | lookup error  | attribute not provided by the stream         |
|  [05]   | `ContextManagerMixin` / `AsyncContextManagerMixin` | mixin | author `__enter__`/`__aenter__` as a single generator method |

[PUBLIC_TYPE_SCOPE]: error and stream-signal types
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                         |
| :-----: | :------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `EndOfStream`             | stream signal  | stream exhausted (raised by `receive`)         |
|  [02]   | `ClosedResourceError`     | resource error | operation on a closed resource                 |
|  [03]   | `BrokenResourceError`     | resource error | unrecoverable stream/socket break              |
|  [04]   | `BusyResourceError`       | resource error | concurrent operation in progress               |
|  [05]   | `WouldBlock`              | sync signal    | non-blocking op would block (`send_nowait`)    |
|  [06]   | `DelimiterNotFound`       | read error     | delimiter absent within max bytes              |
|  [07]   | `IncompleteRead`          | read error     | EOF before expected byte count                 |
|  [08]   | `ConnectionFailed`        | net error      | connection could not be opened                 |
|  [09]   | `NoEventLoopError`        | thread error   | no running event loop for the captured token   |

[PUBLIC_TYPE_SCOPE]: backend and ABC contracts
- rail: concurrency

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :---------------------- | :------------ | :------------------------------ |
|  [01]   | `abc.TaskGroup`         | interface     | structured task group contract  |
|  [02]   | `abc.TaskStatus`        | interface     | `started(value)` readiness signal for `task_status` |
|  [03]   | `abc.AsyncResource`     | interface     | `aclose`-capable resource        |
|  [04]   | `abc.AsyncBackend`      | interface     | pluggable backend dispatch contract |
|  [05]   | `abc.TestRunner`        | interface     | test-fixture event-loop runner   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event loop entry, tasks, and backend query
- rail: concurrency

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `run(func, *args, backend='asyncio', backend_options=None)` | event loop entry | run async main in chosen backend |
|  [02]   | `create_task_group()`                        | task group       | structured concurrency scope (`async with`) |
|  [03]   | `get_current_task()`                         | introspection    | current `TaskInfo`                      |
|  [04]   | `get_running_tasks()`                        | introspection    | list of all running `TaskInfo`          |
|  [05]   | `get_cancelled_exc_class()`                  | backend query    | backend-specific cancellation class     |
|  [06]   | `get_all_backends()` / `get_available_backends()` | backend query | declared vs importable backend names |
|  [07]   | `wait_all_tasks_blocked()`                   | testing helper   | yield until all tasks are suspended     |

[ENTRYPOINT_SCOPE]: cancel scopes and timing
- rail: concurrency

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :--------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `fail_after(delay, shield=False)`  | cancel scope   | raise `TimeoutError` on deadline      |
|  [02]   | `move_on_after(delay, shield=False)` | cancel scope | silently cancel on deadline (`scope.cancelled_caught`) |
|  [03]   | `sleep(delay)`                     | sleep          | sleep for seconds (cancellation point) |
|  [04]   | `sleep_forever()`                  | sleep          | sleep until cancelled                  |
|  [05]   | `sleep_until(deadline)`            | sleep          | sleep until absolute monotonic time    |
|  [06]   | `current_time()`                   | clock          | backend monotonic time                 |
|  [07]   | `current_effective_deadline()`     | clock          | nearest active cancel deadline         |

[ENTRYPOINT_SCOPE]: memory streams
- rail: concurrency

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `create_memory_object_stream[T](max_buffer_size=0, item_type=None)` | factory | paired `(send, receive)` stream; subscript for item type |

[ENTRYPOINT_SCOPE]: networking, TLS, and signals
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `connect_tcp(host, port, *, tls=False, ssl_context=None, tls_hostname=None, happy_eyeballs_delay=0.25, local_host=None)` | connect | TCP `SocketStream`/`TLSStream`, RFC 6555 dual-stack |
|  [02]   | `connect_unix(path)`                                                                       | connect        | UNIX-domain `UNIXSocketStream`                    |
|  [03]   | `create_tcp_listener(*, local_host=None, local_port=0, family=AF_UNSPEC, backlog=65536, reuse_port=False)` | listen | `MultiListener[SocketStream]`             |
|  [04]   | `create_unix_listener(path, *, mode=None, backlog=65536)`                                  | listen         | UNIX `SocketListener`                             |
|  [05]   | `create_udp_socket(family=AF_UNSPEC, *, local_host=None, local_port=0, reuse_port=False)`  | datagram       | unconnected `UDPSocket`                           |
|  [06]   | `create_connected_udp_socket(remote_host, remote_port, *, family=AF_UNSPEC, ...)`          | datagram       | peer-bound `ConnectedUDPSocket`                   |
|  [07]   | `create_unix_datagram_socket(*, local_path=None, local_mode=None)` / `create_connected_unix_datagram_socket(remote_path, ...)` | datagram | UNIX datagram socket variants    |
|  [08]   | `getaddrinfo(host, port, *, family=0, type=0, proto=0, flags=0)` / `getnameinfo(sockaddr, flags=0)` | resolve | async DNS resolution                       |
|  [09]   | `as_connectable(obj)`                                                                      | connectable    | normalize host/path/connectable into a reusable connect target |
|  [10]   | `open_signal_receiver(*signals)`                                                           | signals        | `async with` → async iterator of received signals |
|  [11]   | `wait_readable(obj)` / `wait_writable(obj)` / `wait_socket_readable(sock)` / `wait_socket_writable(sock)` | readiness | await fd/socket readiness without owning the loop |
|  [12]   | `notify_closing(obj)`                                                                      | readiness      | wake pending readiness waiters before closing an fd |
|  [13]   | `aclose_forcefully(resource)`                                                              | teardown       | best-effort `aclose` inside a shielded scope      |

[ENTRYPOINT_SCOPE]: file and process I/O
- rail: concurrency

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `open_file(file, mode='r', ...)`                           | file open      | async file handle (`AsyncFile`)         |
|  [02]   | `wrap_file(file)`                                          | file wrap      | wrap an open sync file object as async  |
|  [03]   | `open_process(command, *, stdin, stdout, stderr, cwd, env, ...)` | process open | async `Process` with stream handles |
|  [04]   | `run_process(command, *, input=None, check=True, ...)`     | process run    | run subprocess to completion, return `CompletedProcess` |
|  [05]   | `mkstemp(...)` / `mkdtemp(...)` / `gettempdir()` / `gettempdirb()` | tempfile | async `tempfile`-module mirrors |

[ENTRYPOINT_SCOPE]: thread / process / interpreter offload
- rail: concurrency

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `to_thread.run_sync(func, *args, abandon_on_cancel=False, limiter=None)` | thread dispatch | run sync callable in a worker thread     |
|  [02]   | `to_thread.current_default_thread_limiter()`                    | limiter query    | per-loop default thread limiter (40 tokens)     |
|  [03]   | `from_thread.run(func, *args)` / `from_thread.run_sync(func, *args)` | bridge call  | call async/sync code from a worker thread       |
|  [04]   | `from_thread.start_blocking_portal(backend='asyncio', backend_options=None)` | portal factory | spin up a loop thread + `BlockingPortal` CM |
|  [05]   | `from_thread.check_cancelled()`                                 | bridge query     | raise if the portal task was cancelled          |
|  [06]   | `to_process.run_sync(func, *args, cancellable=False, limiter=None)` | process dispatch | run sync callable in a worker subprocess (pickle hop) |
|  [07]   | `to_process.current_default_process_limiter()`                  | limiter query    | per-loop default process limiter (CPU count)    |
|  [08]   | `to_interpreter.run_sync(func, *args, limiter=None)`            | interp dispatch  | run sync callable in a PEP-734 subinterpreter (no pickle hop) |
|  [09]   | `to_interpreter.current_default_interpreter_limiter()`          | limiter query    | per-loop default subinterpreter limiter         |

[ENTRYPOINT_SCOPE]: low-level checkpoints and run-locals
- rail: concurrency

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `lowlevel.checkpoint()`                 | checkpoint     | unconditional yield + cancellation check |
|  [02]   | `lowlevel.checkpoint_if_cancelled()`    | checkpoint     | raise only if cancel scope tripped       |
|  [03]   | `lowlevel.cancel_shielded_checkpoint()` | checkpoint     | yield shielded from cancellation         |
|  [04]   | `lowlevel.RunVar(name, default)`        | run-local var  | per-event-loop `ContextVar` equivalent (`get`/`set`) |
|  [05]   | `lowlevel.current_token()`              | backend query  | opaque token identifying the running loop |
|  [06]   | `lowlevel.get_async_backend()`          | backend query  | current running backend instance         |

## [04]-[IMPLEMENTATION_LAW]

[ANYIO_TOPOLOGY]:
- backends: `asyncio` (default) and `trio`, selected at `run(backend=...)` or the `anyio_backend` pytest fixture; domain code never imports backend modules.
- task groups enforce structured concurrency: every child started via `tg.start_soon`/`tg.start` completes before the `async with` block exits; failures aggregate into a `BaseExceptionGroup` (use `except*`).
- `tg.start(func, ...)` waits for the child to call `task_status.started(value)` and returns that value; `tg.start_soon(func, ...)` fires-and-tracks without a readiness handshake.
- `CancelScope(deadline=, shield=)` is the cancellation primitive; `fail_after`/`move_on_after` return one as a context manager. Cancellation is level-triggered: a scope stays cancelled, and a swallowed cancellation re-raises at the next checkpoint unless shielded.
- `create_memory_object_stream[T](max_buffer_size, item_type)` returns a `(MemoryObjectSendStream[T], MemoryObjectReceiveStream[T])` pair; subscript the call for the item type, `send_nowait`/`receive_nowait` raise `WouldBlock`, and a closed receive end raises `BrokenResourceError` on send.
- `connect_tcp(..., tls=True, tls_hostname=...)` performs the TLS handshake inline and returns a `TLSStream`; otherwise wrap any `ByteStream` with `streams.tls.TLSStream.wrap(...)` and read peer-cert/ALPN via `stream.extra(TLSAttribute.peer_certificate)` etc.
- `to_thread.run_sync` dispatches to a `CapacityLimiter`-bounded thread pool (default 40 tokens); pass an explicit limiter to bound a subsystem. `to_process.run_sync` forks a persistent worker subprocess and crosses arguments/results by pickle. `to_interpreter.run_sync` dispatches into a PEP-734 subinterpreter (each carrying its own GIL on a runnable `concurrent.interpreters`) with no pickle round-trip on the worker hop; a worker raise/death surfaces as `BrokenWorkerProcess`/`BrokenWorkerInterpreter` respectively.
- `from_thread.BlockingPortalProvider(backend, backend_options)` lets many threads share one event loop; `portal.call`/`portal.start_task_soon`/`portal.wrap_async_context_manager` bridge sync callers into async code, raising `RunFinishedError` after the loop closes.
- typed-attribute introspection (`stream.extra(attr)`) is the single polymorphic surface for socket/TLS/file metadata — never reach for `raw_socket` properties directly; declare a `TypedAttributeSet` for owner-local stream wrappers.

[STACKS_WITH]:
- `grpcio` (`.api/grpcio.md`): `grpc.aio` servers and channels require a running loop; enter via `anyio.run(serve, backend='asyncio')` and bound every blocking servicer body through `to_thread.run_sync(..., limiter=...)`. Map per-call deadlines onto `fail_after`/`move_on_after` rather than channel options, and bridge sync entrypoints with a `BlockingPortalProvider`.
- `httpx`: pass `anyio` deadlines around `httpx.AsyncClient` calls via `fail_after`; never `asyncio.wait_for`. Fan concurrent requests through one `create_task_group` so a single failure cancels siblings and aggregates as an `ExceptionGroup`.
- `msgspec`/`pydantic` (`.api/msgspec.md`, `.api/pydantic.md`): frame a raw `ByteStream` with `BufferedByteReceiveStream.receive_until(b'\n', max_bytes)` (or `receive_exactly(n)`), then feed the frame to `msgspec.json.Decoder(type=T).decode(...)` for a zero-copy validated wire decode; a `MemoryObjectReceiveStream[T]` typed on a `msgspec.Struct` carries validated objects between tasks without re-serialization.
- `stamina`: wrap a `connect_tcp`/`run_process`/`to_thread.run_sync` effect in `stamina.retry_context(...)`; the retry sleep IS an `anyio.sleep` checkpoint, so an enclosing `move_on_after` deadline still preempts a retry storm.
- `structlog`/`opentelemetry` (`.api/structlog.md`, `.api/opentelemetry-*.md`): bind a `RunVar` (or a `contextvars.ContextVar`, which anyio backends propagate per-task) carrying the trace context so structlog and OTel spans survive task-group hops and `from_thread` bridges; one OTel span per task, opened inside the child coroutine.
- `expression` (`.api/expression.md`): an offloaded `to_thread.run_sync` boundary returns a value the caller lifts into `Result` via `result.of_option`/a try-builder; never let a `BrokenWorkerProcess`/`TimeoutError` escape the boundary adapter as a raw exception.

[LOCAL_ADMISSION]:
- Use `create_task_group` for concurrent task launch; never `asyncio.gather`, `asyncio.create_task`, or `asyncio.TaskGroup` directly.
- Use `fail_after`/`move_on_after` for every deadline-bounded I/O; never `asyncio.wait_for`.
- Use `connect_tcp`/`create_tcp_listener`/`create_udp_socket` and `open_signal_receiver` instead of raw `socket`/`signal` — those bypass deadline, cancellation, and observability policy.
- Use `to_thread.run_sync` (CPU-light blocking I/O), `to_interpreter.run_sync` (CPU-bound, picklable-free), and `to_process.run_sync` (process isolation / GIL-hostile native calls); always pass an explicit `CapacityLimiter` for bounded subsystems.
- Use `lowlevel.checkpoint` in tight loops or polling code so cancellation and fairness still apply.

[RAIL_LAW]:
- Package: `anyio`
- Owns: structured concurrency, cancel scopes, memory streams, networking + TLS, file/process async I/O, thread/process/subinterpreter offload, blocking-portal bridges, sync primitives, signal receivers, typed-attribute introspection
- Accept: `create_task_group`, `fail_after`/`move_on_after`, `connect_tcp`/listeners, `to_thread`/`to_process`/`to_interpreter.run_sync` with explicit `CapacityLimiter`, `create_memory_object_stream`, `BlockingPortalProvider`, `open_signal_receiver`
- Reject: direct `asyncio.gather`/`create_task`/`wait_for`, raw `socket`/`signal`/`subprocess`/`time.sleep`/`ThreadPoolExecutor`/`ProcessPoolExecutor`, swallowing cancellation without a shielded scope
