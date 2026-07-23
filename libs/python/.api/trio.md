# [PY_BRANCH_API_TRIO]

`trio` is the structured-concurrency runtime `anyio` runs on: an async kernel that scopes every task under a joined nursery and cancels through level-triggered scopes, with an `outcome`-typed thread bridge and native TCP/UNIX/SSL/DTLS transports. It is the backend the boundary tier targets through `anyio`, and the direct surface for the guest-mode hosting, custom `Clock`, `Instrument` hooks, and deterministic `trio.testing` kit `anyio` does not expose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trio`
- package: `trio` (MIT OR Apache-2.0)
- module: `trio`
- asset: runtime library
- rail: concurrency
- depends-on: `attrs`, `sortedcontainers`, `idna`, `outcome`, `sniffio`
- namespaces: `trio`, `trio.abc`, `trio.lowlevel`, `trio.socket`, `trio.to_thread`, `trio.from_thread`, `trio.testing`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: nurseries, cancellation, and run lifecycle

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :----------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Nursery`                | task scope      | structured task group; `.start_soon`/`.start`, `.cancel_scope`    |
|  [02]   | `CancelScope`            | scope class     | level-triggered cancellation boundary (`deadline=`/`shield=`)     |
|  [03]   | `TaskStatus`             | `Protocol`      | `.started(value)` readiness signal returned by `nursery.start`    |
|  [04]   | `TASK_STATUS_IGNORED`    | sentinel        | default `task_status` for non-reporting tasks                     |
|  [05]   | `Cancelled`              | `BaseException` | a `CancelScope` fired; never catch bare — re-raises at next yield |
|  [06]   | `TooSlowError`           | exception       | a `fail_after`/`fail_at` deadline elapsed                         |
|  [07]   | `RunFinishedError`       | `RuntimeError`  | `from_thread`/token call after the run finished                   |
|  [08]   | `TrioInternalError`      | exception       | trio invariant violation (bug-class, not a domain fault)          |
|  [09]   | `TrioDeprecationWarning` | `FutureWarning` | deprecated-surface warning category                               |

[PUBLIC_TYPE_SCOPE]: synchronization primitives and statistics

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Event`                     | event class   | set-once async notification (`.set`/`.is_set`/`.wait`)        |
|  [02]   | `Lock`                      | lock class    | async mutual exclusion; `async with` or `.acquire`/`.release` |
|  [03]   | `StrictFIFOLock`            | lock class    | `Lock` guaranteeing strict first-in-first-out wakeup order    |
|  [04]   | `Semaphore`                 | sema class    | async counting semaphore with optional `max_value`            |
|  [05]   | `Condition`                 | cond class    | async condition variable over a `Lock` (`.notify`/`.wait`)    |
|  [06]   | `CapacityLimiter`           | limiter class | concurrent-slot limiter; `total_tokens` resizable at runtime  |
|  [07]   | `EventStatistics`           | stats record  | event subscriber-count snapshot                               |
|  [08]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot                                   |
|  [09]   | `ConditionStatistics`       | stats record  | condition waiter snapshot                                     |
|  [10]   | `SemaphoreStatistics`       | stats record  | semaphore waiter snapshot                                     |
|  [11]   | `CapacityLimiterStatistics` | stats record  | limiter borrowers/waiters snapshot                            |

[PUBLIC_TYPE_SCOPE]: channels and channel statistics

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :------------------------ | :------------- | :----------------------------------------------------------- |
|  [01]   | `MemorySendChannel`       | object channel | clonable send half; `.send`/`.send_nowait`/`.clone`          |
|  [02]   | `MemoryReceiveChannel`    | object channel | clonable receive half; `.receive`/`.receive_nowait`/`.clone` |
|  [03]   | `MemoryChannelStatistics` | stats record   | buffered/sender/receiver snapshot                            |
|  [04]   | `EndOfChannel`            | channel signal | all senders closed (raised by `receive`; ends `async for`)   |

[PUBLIC_TYPE_SCOPE]: transports — streams, listeners, SSL/DTLS

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `SocketStream`          | byte stream   | TCP/UNIX duplex; `.send_all`/`.receive_some`/`.send_eof`    |
|  [02]   | `SocketListener`        | listener      | single-socket accept loop (`.accept`)                       |
|  [03]   | `StapledStream`         | byte stream   | staple a send + receive half into one half-closeable stream |
|  [04]   | `SSLStream`             | byte stream   | TLS over any `Stream`; `.do_handshake`/`.unwrap`            |
|  [05]   | `SSLListener`           | listener      | TLS-terminating accept loop                                 |
|  [06]   | `DTLSEndpoint`          | datagram TLS  | one UDP socket multiplexing many DTLS peers                 |
|  [07]   | `DTLSChannel`           | datagram TLS  | one DTLS association; `.send`/`.receive`/`.do_handshake`    |
|  [08]   | `DTLSChannelStatistics` | stats record  | DTLS retransmit/record snapshot                             |
|  [09]   | `NeedHandshakeError`    | TLS error     | peer metadata read before the handshake completed           |

[PUBLIC_TYPE_SCOPE]: subprocess and async filesystem

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [CAPABILITY]                                                                                         |
| :-----: | :------------ | :------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Process`     | process handle | async subprocess: `.wait`/`.kill`/`.send_signal`, `.stdin`/`.stdout`/`.stderr`, `.returncode`/`.pid` |
|  [02]   | `Path`        | async path     | `pathlib.Path` mirror; every I/O method awaitable (`.read_bytes`/`.iterdir`/`.stat`/`.open`)         |
|  [03]   | `PosixPath`   | async path     | POSIX concrete `Path` flavour                                                                        |
|  [04]   | `WindowsPath` | async path     | Windows concrete `Path` flavour                                                                      |

[PUBLIC_TYPE_SCOPE]: resource and stream error types

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [CAPABILITY]                                       |
| :-----: | :-------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `ClosedResourceError` | resource error | operation on a locally-closed resource             |
|  [02]   | `BrokenResourceError` | resource error | unrecoverable stream/socket break                  |
|  [03]   | `BusyResourceError`   | resource error | two tasks used one non-shareable resource          |
|  [04]   | `WouldBlock`          | sync signal    | non-blocking op cannot proceed now (`send_nowait`) |

[PUBLIC_TYPE_SCOPE]: abstract contracts (`trio.abc`)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `AsyncResource`                                 | interface     | `aclose`-capable resource base                                       |
|  [02]   | `{Send,Receive,HalfCloseable}Stream` / `Stream` | interface     | half/duplex/half-closeable byte-stream contracts                     |
|  [03]   | `{Send,Receive}Channel` / `Channel`             | interface     | typed object-channel contracts (generic over item type)              |
|  [04]   | `Listener`                                      | interface     | `accept`-loop connection listener                                    |
|  [05]   | `Clock`                                         | interface     | virtual clock: `start_clock`/`current_time`/`deadline_to_sleep_time` |
|  [06]   | `Instrument`                                    | interface     | run-loop event hook bus (task/step/io-wait callbacks)                |
|  [07]   | `HostnameResolver`                              | interface     | overridable async `getaddrinfo`/`getnameinfo` hook                   |
|  [08]   | `SocketFactory`                                 | interface     | overridable socket constructor for the network stack                 |

[PUBLIC_TYPE_SCOPE]: low-level run primitives (`trio.lowlevel`)

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------ | :------------- | :----------------------------------------------------------- |
|  [01]   | `Task`                                      | task handle    | a scheduled trio task (`.name`/`.coro`/`.context`)           |
|  [02]   | `TrioToken`                                 | loop token     | thread-safe handle to a running loop; `.run_sync_soon`       |
|  [03]   | `RunVar` / `RunVarToken`                    | run-local var  | per-`run()` `ContextVar` equivalent (`.get`/`.set`/`.reset`) |
|  [04]   | `ParkingLot`                                | wait primitive | low-level FIFO wait queue underpinning every sync primitive  |
|  [05]   | `UnboundedQueue`                            | queue          | unbounded multi-producer batch queue (`.get_batch_nowait`)   |
|  [06]   | `Abort`                                     | `Enum`         | `wait_task_rescheduled` abort outcome (`SUCCEEDED`/`FAILED`) |
|  [07]   | `FdStream`                                  | byte stream    | wrap a POSIX fd (pipe/PTY) as a trio `Stream`                |
|  [08]   | `{Run,ParkingLot,UnboundedQueue}Statistics` | stats record   | run-loop / parking-lot / queue snapshots                     |

[PUBLIC_TYPE_SCOPE]: testing kit (`trio.testing`)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `MockClock`                  | clock         | virtual clock: `autojump_threshold` collapses idle, `rate` scales, `.jump(s)` steps |
|  [02]   | `Sequencer`                  | ordering tool | force a deterministic interleaving across tasks (`async with seq(n)`)               |
|  [03]   | `Memory{Send,Receive}Stream` | test stream   | in-memory byte stream with injectable `send_all_hook`/`receive_some_hook`           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: run entry, nurseries, and clock
- `run` carries `clock=None, instruments=(), restrict_keyboard_interrupt_to_checkpoints=False, strict_exception_groups=True`

| [INDEX] | [SURFACE]                                    | [SHAPE]          | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `run(async_fn, *args, ...)`                  | event loop entry | run an async main; inject a `Clock` and `Instrument` list    |
|  [02]   | `open_nursery(strict_exception_groups=None)` | task group       | `async with` structured nursery (`start_soon`/`start`)       |
|  [03]   | `current_time()`                             | clock            | the active `Clock`'s monotonic time                          |
|  [04]   | `current_effective_deadline()`               | clock            | nearest active cancel deadline (`-inf` if already cancelled) |

[ENTRYPOINT_SCOPE]: cancel scopes, timeouts, and sleeping

| [INDEX] | [SURFACE]                                 | [SHAPE]      | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------- | :----------- | :---------------------------------------------------------------- |
|  [01]   | `fail_after(seconds, *, shield=False)`    | cancel scope | raise `TooSlowError` on a relative deadline                       |
|  [02]   | `fail_at(deadline, *, shield=False)`      | cancel scope | raise `TooSlowError` at an absolute `current_time` deadline       |
|  [03]   | `move_on_after(seconds, *, shield=False)` | cancel scope | silently cancel on a relative deadline (`scope.cancelled_caught`) |
|  [04]   | `move_on_at(deadline, *, shield=False)`   | cancel scope | silently cancel at an absolute deadline                           |
|  [05]   | `sleep(seconds)`                          | sleep        | sleep for seconds (cancellation checkpoint)                       |
|  [06]   | `sleep_until(deadline)`                   | sleep        | sleep until an absolute `current_time` deadline                   |
|  [07]   | `sleep_forever()`                         | sleep        | sleep until cancelled                                             |

[ENTRYPOINT_SCOPE]: channels

| [INDEX] | [SURFACE]                                 | [SHAPE] | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :------ | :------------------------------------------------------------ |
|  [01]   | `open_memory_channel[T](max_buffer_size)` | factory | `(send, receive)` channel pair; `math.inf` unbounded          |
|  [02]   | `as_safe_channel(fn)`                     | adapter | pump `AsyncGenerator` → `ReceiveChannel` CM; cancel-decoupled |

[ENTRYPOINT_SCOPE]: networking, TLS, and signals
- connect/listen surfaces carry `happy_eyeballs_delay=0.25, local_address, host, backlog, ssl_context, https_compatible, handler_nursery` as applicable

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `open_tcp_stream(host, port, *, ...)`                    | connect  | TCP `SocketStream`, RFC 6555 dual-stack dialing            |
|  [02]   | `open_unix_socket(filename)`                             | connect  | UNIX-domain `SocketStream`                                 |
|  [03]   | `open_tcp_listeners(port, *, ...)`                       | listen   | build TCP listeners                                        |
|  [04]   | `serve_tcp(handler, port, *, ...)`                       | serve    | accept-loop each connection into `handler` under a nursery |
|  [05]   | `open_ssl_over_tcp_stream(host, port, *, ...)`           | connect  | TLS `SSLStream` over a fresh TCP dial                      |
|  [06]   | `open_ssl_over_tcp_listeners(port, ssl_context, *, ...)` | listen   | TLS-terminating listeners                                  |
|  [07]   | `serve_ssl_over_tcp(handler, port, ssl_context, ...)`    | serve    | serve each TLS stream into `handler`                       |
|  [08]   | `serve_listeners(handler, listeners, *, ...)`            | serve    | accept-loop any `Listener` list, one task per connection   |
|  [09]   | `open_signal_receiver(*signals)`                         | signals  | `with` → async iterator of received signal numbers         |
|  [10]   | `aclose_forcefully(resource)`                            | teardown | best-effort `aclose` that ignores errors and never blocks  |

[ENTRYPOINT_SCOPE]: file and process I/O
- `run_process` carries `stdin=b'', capture_stdout=False, capture_stderr=False, check=True, deliver_cancel=None, task_status`

| [INDEX] | [SURFACE]                        | [SHAPE]     | [CAPABILITY]                                                              |
| :-----: | :------------------------------- | :---------- | :------------------------------------------------------------------------ |
|  [01]   | `open_file(file, mode='r', ...)` | file open   | async file handle (thread-offloaded `AsyncIOWrapper`)                     |
|  [02]   | `wrap_file(file)`                | file wrap   | wrap an already-open sync file object as async                            |
|  [03]   | `run_process(command, *, ...)`   | process run | run → `CompletedProcess`; `task_status.started` yields the live `Process` |

[ENTRYPOINT_SCOPE]: thread / loop bridge (`trio.to_thread`, `trio.from_thread`)
- `to_thread.run_sync` carries `thread_name, abandon_on_cancel=False, limiter`; `from_thread.run`/`run_sync` carry `trio_token`

| [INDEX] | [SURFACE]                                    | [SHAPE]         | [CAPABILITY]                                                           |
| :-----: | :------------------------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `to_thread.run_sync(sync_fn, *args, ...)`    | thread dispatch | run a sync callable in a worker thread, `CapacityLimiter`-bounded      |
|  [02]   | `to_thread.current_default_thread_limiter()` | limiter query   | per-`run` default thread limiter (40 tokens)                           |
|  [03]   | `from_thread.run(afn, *args, ...)`           | bridge call     | call async trio code from a worker thread back into the loop           |
|  [04]   | `from_thread.run_sync(fn, *args, ...)`       | bridge call     | call sync trio code from a worker thread back into the loop            |
|  [05]   | `from_thread.check_cancelled()`              | bridge query    | raise `Cancelled` in the worker thread if the host scope was cancelled |

[ENTRYPOINT_SCOPE]: low-level checkpoints, scheduling, and guest mode (`trio.lowlevel`)
- `start_guest_run` carries `run_sync_soon_threadsafe, done_callback`; `spawn_system_task` carries `name=None, context=None`; `wait_task_rescheduled(abort_func)` and `reschedule(task, next_send)` name their sole args

| [INDEX] | [SURFACE]                                                | [SHAPE]         | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `checkpoint()`                                           | checkpoint      | unconditional yield + cancellation check             |
|  [02]   | `checkpoint_if_cancelled` / `cancel_shielded_checkpoint` | checkpoint      | cancel-only check / yield shielded from cancellation |
|  [03]   | `current_{task,root_task,trio_token,statistics}()`       | introspection   | running task / root / loop token / `RunStatistics`   |
|  [04]   | `spawn_system_task(async_fn, *args, ...)`                | scheduling      | spawn a nursery-free, KI-shielded system task        |
|  [05]   | `wait_task_rescheduled` / `reschedule`                   | scheduling      | the park/unpark pair under every async primitive     |
|  [06]   | `wait_{readable,writable}(fd)` / `notify_closing(fd)`    | readiness       | await raw fd readiness; wake waiters before closing  |
|  [07]   | `start_guest_run(async_fn, *args, ...)`                  | guest mode      | interleave a trio run in a foreign loop (Qt/asyncio) |
|  [08]   | `add_instrument` / `remove_instrument`                   | instrumentation | attach/detach an `Instrument` on the live run        |
|  [09]   | `enable_ki_protection(f)` / `disable_ki_protection(f)`   | KI control      | mark a region (un)protected from `KeyboardInterrupt` |

[ENTRYPOINT_SCOPE]: deterministic testing (`trio.testing`)

| [INDEX] | [SURFACE]                                              | [SHAPE]        | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `trio_test(fn)`                                        | test decorator | run an `async def` test under `trio.run` (pytest-agnostic) |
|  [02]   | `wait_all_tasks_blocked(cushion=0.0)`                  | testing helper | yield until every other task is blocked                    |
|  [03]   | `assert_checkpoints()` / `assert_no_checkpoints()`     | assertion CM   | assert a block does/doesn't hit a cancellation checkpoint  |
|  [04]   | `{memory,lockstep}_stream_pair()`                      | stream factory | in-memory / lockstep-synced test stream pair               |
|  [05]   | `memory_stream_pump(send, receive, *, max_bytes=None)` | stream pump    | shuttle bytes between the test stream halves               |
|  [06]   | `check_{two_way,one_way,half_closeable}_stream(...)`   | conformance    | exhaustive contract test for a custom `Stream`             |
|  [07]   | `open_stream_to_socket_listener(listener)`             | test connect   | dial a `SocketListener` with no real network round-trip    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `open_nursery()` is `async with`; every `nursery.start_soon`/`nursery.start` child completes before the block exits, and child failures aggregate into a `BaseExceptionGroup` split with `except*` — `strict_exception_groups=True` is the trio default, so a lone child raise still surfaces as a one-element group.
- `nursery.start(async_fn, ...)` blocks until the child calls `task_status.started(value)` and returns that value (the listener-ready / port-bound handshake); `nursery.start_soon` fires-and-tracks with no readiness signal, and `nursery.cancel_scope.cancel()` cancels the whole subtree.
- Cancellation is level-triggered through `CancelScope(deadline=, shield=)`: a cancelled scope re-raises `Cancelled` at every checkpoint until exit. `fail_after`/`move_on_after` (relative) and `fail_at`/`move_on_at` (absolute) return scopes — `fail_*` raises `TooSlowError`, `move_on_*` sets `scope.cancelled_caught`, and `CancelScope.cancel(reason=...)` records a human reason.
- `Cancelled` subclasses `BaseException`, not `Exception`, so a broad `except Exception` cannot eat it; let the owning scope re-raise it.
- `open_memory_channel[T](max_buffer_size)` returns a `(MemorySendChannel[T], MemoryReceiveChannel[T])` pair — subscript the call for the item type, `math.inf` for unbounded. `send_nowait`/`receive_nowait` raise `WouldBlock`, a send onto a closed receive end raises `BrokenResourceError`, and an exhausted-but-open channel raises `EndOfChannel` from `receive` (which ends an `async for`). `.clone()` mints extra ends for fan-in/fan-out, and a side closes when its last end closes.
- `open_ssl_over_tcp_stream` runs the TLS handshake lazily on first I/O (force it with `SSLStream.do_handshake()`); `SSLStream.unwrap()` recovers the raw `Stream` with any trailing bytes, and `DTLSEndpoint` multiplexes many `DTLSChannel` peers over one UDP socket.
- `outcome` types the thread/loop bridge: a worker result crosses back as an `outcome.Value`/`outcome.Error` that re-raises in the awaiting task, so `to_thread.run_sync`/`from_thread.run` preserve the original exception and `Cancelled` across the thread boundary.
- `to_thread.run_sync` dispatches to a `CapacityLimiter`-bounded pool (default 40 tokens); `abandon_on_cancel=True` leaves a cancelled call's thread running, for side-effect-free work only. `from_thread.run`/`run_sync` (or a captured `TrioToken.run_sync_soon`) re-enter the loop from a worker thread, raising `RunFinishedError` after the run ends.
- `Clock` and `Instrument` are the trio-exclusive seams `anyio` does not expose: `trio.testing.MockClock(autojump_threshold=0)` collapses all idle waiting in a test, and an `Instrument` (or `lowlevel.add_instrument`) observes `task_spawned`/`before_task_step`/`before_io_wait` for tracing without touching task code.
- `lowlevel.start_guest_run` interleaves a trio run atop a foreign loop (asyncio, Qt, a GUI toolkit) through a `run_sync_soon_threadsafe` callback — the integration point when trio coexists with a host loop it does not own.

[STACKING]:
- `anyio`(`.api/anyio.md`): trio is the backend `anyio` runs on (`anyio.run(main, backend='trio')` / the `anyio_backend='trio'` fixture); an `anyio` `CancelScope`/`create_task_group`/`create_memory_object_stream` maps one-to-one onto trio's `CancelScope`/`open_nursery`/`open_memory_channel`, so a backend swap is a `run` argument. Reach for the bare trio surface only for guest mode, a custom `Clock`, `Instrument` hooks, `DTLS`, and the `trio.testing` autojump/checkpoint kit.
- `stamina`(`libs/python/runtime/.api/stamina.md`): wrap an `open_tcp_stream`/`run_process`/`to_thread.run_sync` effect in `retry_context(...)`; stamina detects the trio loop through `sniffio` and its retry sleep is a `trio.sleep` checkpoint, so an enclosing `move_on_after`/`fail_after` preempts a retry storm and a `Cancelled` cuts the wait. `RetryDetails` flows into the receipt stream unchanged.
- `msgspec`/`pydantic`(`.api/msgspec.md`, `.api/pydantic.md`): frame a `SocketStream` by looping `receive_some(max_bytes)` to a delimiter, then feed the frame to `msgspec.json.Decoder(type=T).decode(...)` for a zero-copy validated decode; a `MemoryReceiveChannel[T]` typed on a `msgspec.Struct` carries already-validated objects between tasks, and a tagged `Struct` union is the on-wire discriminant on both channel ends.
- `structlog`/`opentelemetry`(`.api/structlog.md`, `.api/opentelemetry-*.md`): bind the trace/log context in a `lowlevel.RunVar` (or a `contextvars.ContextVar`, copied per spawned task) so it survives nursery hops and `from_thread` bridges, one OTel span per task opened inside the child coroutine; an `Instrument` subclass is the trio-native sink for a span/metric on `task_spawned`/`task_exited` across the whole run.
- `expression`(`.api/expression.md`): a `to_thread.run_sync`/`run_process` boundary lifts its return into `Result` via `result.of_option` or a try-builder, so a `TooSlowError`/`BrokenResourceError`/`subprocess.CalledProcessError` never escapes raw; `Cancelled` alone re-raises untouched — it belongs to the owning `CancelScope`, not the `Result` rail.
- `cyclopts`/`grpcio`(`libs/python/runtime/.api/cyclopts.md`, `.api/grpcio.md`): a sync CLI or servicer entrypoint hosts the async core with `trio.run(main)`, bridges a re-entrant sync callback through a captured `TrioToken.run_sync_soon`, and bounds every blocking servicer body through `to_thread.run_sync(..., limiter=...)`.

[LOCAL_ADMISSION]:
- Spawn concurrent work only inside `open_nursery()`; an unscoped task escapes cancellation and failure aggregation.
- Bound every deadline-limited I/O with `fail_after`/`move_on_after` (or the `_at` absolute forms) — the deadline rides the enclosing `CancelScope` and composes through nested calls without a per-call timeout argument.
- Dispatch blocking native/sync calls through `to_thread.run_sync(..., limiter=...)` with an explicit `CapacityLimiter`, and re-enter the loop from a thread through `from_thread.run`.
- Checkpoint tight compute or polling loops with `lowlevel.checkpoint()` so cancellation and fairness stay live; gate test timing on `testing.wait_all_tasks_blocked()` and `testing.MockClock`, never a wall-clock sleep.

[RAIL_LAW]:
- Package: `trio`
- Owns: the structured-concurrency runtime — nurseries, level-triggered cancel scopes + deadlines, memory channels, `outcome`-typed thread/loop bridge, abstract stream/channel/listener/clock/instrument contracts, native TCP/UNIX/SSL/DTLS transports, async `Path`, subprocess, signal receivers, guest-mode interleaving, low-level checkpoints/parking, and the deterministic `trio.testing` kit
- Accept: `open_nursery` + `nursery.start`/`start_soon`, `fail_after`/`move_on_after`/`fail_at`/`move_on_at`, `open_memory_channel`, `open_tcp_stream`/`serve_tcp`/SSL+DTLS transports, `to_thread`/`from_thread` with explicit `CapacityLimiter`, `trio.Path`/`run_process`, `open_signal_receiver`, `lowlevel.checkpoint`/`start_guest_run`/`RunVar`/`Instrument`, `trio.testing` autojump + checkpoint assertions
- Reject: bare un-nurseried task spawns, raw `socket`/`ssl`/`signal`/`subprocess`/`threading`/`concurrent.futures`, blocking sync `open`/`pathlib`/`time.sleep` on the loop, catching `Cancelled`/`BaseException` broadly, `asyncio` primitives mixed into a trio run, and wall-clock timing in tests
