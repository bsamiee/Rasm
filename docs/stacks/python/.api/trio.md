# [PY_BRANCH_API_TRIO]

`trio` is the structured-concurrency runtime `anyio` runs on: an opinionated async kernel built on nurseries (every spawned task is scoped and joined), level-triggered `CancelScope` cancellation with deadlines, fan-out/fan-in memory channels, an `outcome`-typed thread/loop bridge, abstract stream/channel/listener contracts, native TCP/UNIX/SSL/DTLS transports with happy-eyeballs dialing, an async `pathlib.Path` mirror, signal receivers, a pluggable `Clock` plus `Instrument` event-hook bus, guest-mode interleaving with a foreign loop, and a deterministic testing kit (`MockClock` autojump, checkpoint assertions, lockstep streams). It is the high-scrutiny backend the boundary tier targets through `anyio`, and the direct surface for guest-mode hosting, custom clocks, instrumentation, and deterministic concurrency tests that `anyio` does not expose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trio`
- package: `trio`
- module: `trio`
- version: `0.33.0` (floor `>=0.27.0`)
- license: `MIT OR Apache-2.0`
- wheel: pure-Python (`py3-none-any`), no ABI tag; resolves clean on cp315
- abi: none; `Requires-Python >=3.10`
- marker: none (admitted runtime dependency; `anyio`'s `trio` backend binds it directly)
- asset: runtime library
- rail: concurrency
- depends-on: `attrs`, `sortedcontainers`, `idna`, `outcome` (typed call results across the thread/loop bridge), `sniffio>=1.3.0` (the runtime-detection beacon `anyio`/`stamina` read to pick the active backend)
- namespaces: `trio`, `trio.abc`, `trio.lowlevel`, `trio.socket`, `trio.to_thread`, `trio.from_thread`, `trio.testing`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: nurseries, cancellation, and run lifecycle
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [RAIL]                                                              |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `Nursery`             | task scope      | structured task group; `.start_soon`/`.start`, `.cancel_scope`     |
|  [02]   | `CancelScope`         | scope class     | level-triggered cancellation boundary (`deadline=`/`shield=`)      |
|  [03]   | `TaskStatus`          | `Protocol`      | `.started(value)` readiness signal returned by `nursery.start`     |
|  [04]   | `TASK_STATUS_IGNORED` | sentinel        | default `task_status` for non-reporting tasks                      |
|  [05]   | `Cancelled`           | `BaseException` | a `CancelScope` fired; never catch bare — re-raises at next yield  |
|  [06]   | `TooSlowError`        | exception       | a `fail_after`/`fail_at` deadline elapsed                          |
|  [07]   | `RunFinishedError`    | `RuntimeError`  | `from_thread`/token call after the run finished                    |
|  [08]   | `TrioInternalError`   | exception       | trio invariant violation (bug-class, not a domain fault)           |
|  [09]   | `TrioDeprecationWarning` | `FutureWarning` | deprecated-surface warning category                            |

[PUBLIC_TYPE_SCOPE]: synchronization primitives and statistics
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Event`                     | event class   | set-once async notification (`.set`/`.is_set`/`.wait`)         |
|  [02]   | `Lock`                      | lock class    | async mutual exclusion; `async with` or `.acquire`/`.release`  |
|  [03]   | `StrictFIFOLock`            | lock class    | `Lock` guaranteeing strict first-in-first-out wakeup order     |
|  [04]   | `Semaphore`                 | sema class    | async counting semaphore with optional `max_value`            |
|  [05]   | `Condition`                 | cond class    | async condition variable over a `Lock` (`.notify`/`.wait`)     |
|  [06]   | `CapacityLimiter`           | limiter class | concurrent-slot limiter; `total_tokens` resizable at runtime   |
|  [07]   | `EventStatistics`           | stats record  | event subscriber-count snapshot                                |
|  [08]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot                                    |
|  [09]   | `ConditionStatistics`       | stats record  | condition waiter snapshot                                      |
|  [10]   | `SemaphoreStatistics`       | stats record  | semaphore waiter snapshot                                      |
|  [11]   | `CapacityLimiterStatistics` | stats record  | limiter borrowers/waiters snapshot                             |

[PUBLIC_TYPE_SCOPE]: channels and channel statistics
- rail: concurrency

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `MemorySendChannel`      | object channel | clonable send half; `.send`/`.send_nowait`/`.clone`        |
|  [02]   | `MemoryReceiveChannel`   | object channel | clonable receive half; `.receive`/`.receive_nowait`/`.clone` |
|  [03]   | `MemoryChannelStatistics` | stats record  | buffered/sender/receiver snapshot                           |
|  [04]   | `EndOfChannel`           | channel signal | all senders closed (raised by `receive`; ends `async for`)  |

[PUBLIC_TYPE_SCOPE]: transports — streams, listeners, SSL/DTLS
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :------------------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `SocketStream`            | byte stream    | TCP/UNIX duplex; `.send_all`/`.receive_some`/`.send_eof`    |
|  [02]   | `SocketListener`          | listener       | single-socket accept loop (`.accept`)                       |
|  [03]   | `StapledStream`           | byte stream    | staple a send + receive half into one half-closeable stream |
|  [04]   | `SSLStream`               | byte stream    | TLS over any `Stream`; `.do_handshake`/`.unwrap`            |
|  [05]   | `SSLListener`             | listener       | TLS-terminating accept loop                                 |
|  [06]   | `DTLSEndpoint`            | datagram TLS   | one UDP socket multiplexing many DTLS peers                 |
|  [07]   | `DTLSChannel`             | datagram TLS   | one DTLS association; `.send`/`.receive`/`.do_handshake`    |
|  [08]   | `DTLSChannelStatistics`   | stats record   | DTLS retransmit/record snapshot                             |
|  [09]   | `NeedHandshakeError`      | TLS error      | peer metadata read before the handshake completed           |

[PUBLIC_TYPE_SCOPE]: subprocess and async filesystem
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :-------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Process`             | process handle | async subprocess; `.wait`/`.kill`/`.send_signal`, `.stdin`/`.stdout`/`.stderr` stream handles, `.returncode`/`.pid` |
|  [02]   | `Path`                | async path     | `pathlib.Path` mirror; every I/O method awaitable (`.read_bytes`/`.iterdir`/`.stat`/`.open`) |
|  [03]   | `PosixPath`           | async path     | POSIX concrete `Path` flavour                               |
|  [04]   | `WindowsPath`         | async path     | Windows concrete `Path` flavour                             |

[PUBLIC_TYPE_SCOPE]: resource and stream error types
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :-------------------- | :------------- | :------------------------------------------- |
|  [01]   | `ClosedResourceError` | resource error | operation on a locally-closed resource       |
|  [02]   | `BrokenResourceError` | resource error | unrecoverable stream/socket break            |
|  [03]   | `BusyResourceError`   | resource error | two tasks used one non-shareable resource     |
|  [04]   | `WouldBlock`          | sync signal    | non-blocking op would block (`send_nowait`)  |

[PUBLIC_TYPE_SCOPE]: abstract contracts (`trio.abc`)
- rail: concurrency

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `AsyncResource`          | interface     | `aclose`-capable resource base                             |
|  [02]   | `SendStream` / `ReceiveStream` / `Stream` / `HalfCloseableStream` | interface | half/duplex/half-closeable byte-stream contracts |
|  [03]   | `SendChannel` / `ReceiveChannel` / `Channel` | interface | typed object-channel contracts (generic over item type)    |
|  [04]   | `Listener`               | interface     | `accept`-loop connection listener                          |
|  [05]   | `Clock`                  | interface     | pluggable virtual-time clock (`start_clock`/`current_time`/`deadline_to_sleep_time`) |
|  [06]   | `Instrument`             | interface     | run-loop event hook bus (task/step/io-wait callbacks)      |
|  [07]   | `HostnameResolver`       | interface     | overridable async `getaddrinfo`/`getnameinfo` hook         |
|  [08]   | `SocketFactory`          | interface     | overridable socket constructor for the network stack       |

[PUBLIC_TYPE_SCOPE]: low-level run primitives (`trio.lowlevel`)
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `Task`                    | task handle   | a scheduled trio task (`.name`/`.coro`/`.context`)         |
|  [02]   | `TrioToken`               | loop token    | thread-safe handle to a running loop; `.run_sync_soon`     |
|  [03]   | `RunVar` / `RunVarToken`  | run-local var | per-`run()` `ContextVar` equivalent (`.get`/`.set`/`.reset`) |
|  [04]   | `ParkingLot`              | wait primitive | low-level FIFO wait queue underpinning every sync primitive |
|  [05]   | `UnboundedQueue`          | queue         | unbounded multi-producer batch queue (`.get_batch_nowait`) |
|  [06]   | `Abort`                   | `Enum`        | `wait_task_rescheduled` abort outcome (`SUCCEEDED`/`FAILED`) |
|  [07]   | `FdStream`                | byte stream   | wrap a POSIX fd (pipe/PTY) as a trio `Stream`              |
|  [08]   | `RunStatistics` / `ParkingLotStatistics` / `UnboundedQueueStatistics` | stats record | run-loop / parking-lot / queue snapshots |

[PUBLIC_TYPE_SCOPE]: testing kit (`trio.testing`)
- rail: concurrency

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `MockClock`                               | clock         | virtual clock; `autojump_threshold=0` collapses idle time, `rate` scales speed, `.jump(s)` steps forward |
|  [02]   | `Sequencer`                               | ordering tool | force a deterministic interleaving across tasks (`async with seq(n)`) |
|  [03]   | `MemorySendStream` / `MemoryReceiveStream` | test stream   | in-memory byte stream with injectable `send_all_hook`/`receive_some_hook` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: run entry, nurseries, and clock
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                                                    | [ENTRY_FAMILY]   | [RAIL]                                                       |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `run(async_fn, *args, clock=None, instruments=(), restrict_keyboard_interrupt_to_checkpoints=False, strict_exception_groups=True)` | event loop entry | run an async main; inject a `Clock` and `Instrument` list   |
|  [02]   | `open_nursery(strict_exception_groups=None)`                                                                                 | task group       | `async with` structured nursery (`start_soon`/`start`)      |
|  [03]   | `current_time()`                                                                                                            | clock            | the active `Clock`'s monotonic time                         |
|  [04]   | `current_effective_deadline()`                                                                                             | clock            | nearest active cancel deadline (`-inf` if already cancelled) |

[ENTRYPOINT_SCOPE]: cancel scopes, timeouts, and sleeping
- rail: concurrency

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :--------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `fail_after(seconds, *, shield=False)`   | cancel scope   | raise `TooSlowError` on a relative deadline                 |
|  [02]   | `fail_at(deadline, *, shield=False)`     | cancel scope   | raise `TooSlowError` at an absolute `current_time` deadline |
|  [03]   | `move_on_after(seconds, *, shield=False)` | cancel scope  | silently cancel on a relative deadline (`scope.cancelled_caught`) |
|  [04]   | `move_on_at(deadline, *, shield=False)`  | cancel scope   | silently cancel at an absolute deadline                     |
|  [05]   | `sleep(seconds)`                         | sleep          | sleep for seconds (cancellation checkpoint)                 |
|  [06]   | `sleep_until(deadline)`                  | sleep          | sleep until an absolute `current_time` deadline             |
|  [07]   | `sleep_forever()`                        | sleep          | sleep until cancelled                                       |

[ENTRYPOINT_SCOPE]: channels
- rail: concurrency

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :--------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `open_memory_channel[T](max_buffer_size)`                  | factory        | paired `(MemorySendChannel[T], MemoryReceiveChannel[T])`; subscript for the item type, `math.inf` for an unbounded buffer |
|  [02]   | `as_safe_channel(fn)`                                      | adapter        | turn an `AsyncGenerator` into a background-pumped `ReceiveChannel` context manager (decouples producer cancellation from the consumer) |

[ENTRYPOINT_SCOPE]: networking, TLS, and signals
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `open_tcp_stream(host, port, *, happy_eyeballs_delay=0.25, local_address=None)`                                        | connect        | TCP `SocketStream`, RFC 6555 dual-stack dialing            |
|  [02]   | `open_unix_socket(filename)`                                                                                           | connect        | UNIX-domain `SocketStream`                                 |
|  [03]   | `open_tcp_listeners(port, *, host=None, backlog=None)` / `serve_tcp(handler, port, *, host=None, handler_nursery=None)` | listen / serve | build TCP listeners, or accept-loop each connection into `handler` under a nursery |
|  [04]   | `open_ssl_over_tcp_stream(host, port, *, ssl_context=None, https_compatible=False, happy_eyeballs_delay=0.25)`         | connect        | TLS `SSLStream` over a fresh TCP dial                      |
|  [05]   | `open_ssl_over_tcp_listeners(port, ssl_context, *, host=None, https_compatible=False)` / `serve_ssl_over_tcp(handler, port, ssl_context, ...)` | listen / serve | TLS-terminating listeners, or serve each TLS stream into `handler` |
|  [06]   | `serve_listeners(handler, listeners, *, handler_nursery=None)`                                                         | serve          | accept-loop any `Listener` list, one child task per connection |
|  [07]   | `open_signal_receiver(*signals)`                                                                                       | signals        | `with` → async iterator of received signal numbers          |
|  [08]   | `aclose_forcefully(resource)`                                                                                          | teardown       | best-effort `aclose` that ignores errors and never blocks   |

[ENTRYPOINT_SCOPE]: file and process I/O
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `open_file(file, mode='r', ...)`                                                                                     | file open      | async file handle (thread-offloaded `AsyncIOWrapper`)      |
|  [02]   | `wrap_file(file)`                                                                                                    | file wrap      | wrap an already-open sync file object as async             |
|  [03]   | `run_process(command, *, stdin=b'', capture_stdout=False, capture_stderr=False, check=True, deliver_cancel=None, task_status=...)` | process run    | run a subprocess to completion → `subprocess.CompletedProcess`; `task_status.started` yields the live `Process` for mid-run signalling |

[ENTRYPOINT_SCOPE]: thread / loop bridge (`trio.to_thread`, `trio.from_thread`)
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]   | [RAIL]                                                      |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------- | :--------------------------------------------------------- |
|  [01]   | `to_thread.run_sync(sync_fn, *args, thread_name=None, abandon_on_cancel=False, limiter=None)` | thread dispatch  | run a sync callable in a worker thread, `CapacityLimiter`-bounded |
|  [02]   | `to_thread.current_default_thread_limiter()`                                                | limiter query    | per-`run` default thread limiter (40 tokens)               |
|  [03]   | `from_thread.run(afn, *args, trio_token=None)` / `from_thread.run_sync(fn, *args, trio_token=None)` | bridge call | call async/sync trio code from a worker thread back into the loop |
|  [04]   | `from_thread.check_cancelled()`                                                             | bridge query     | raise `Cancelled` in the worker thread if the host scope was cancelled |

[ENTRYPOINT_SCOPE]: low-level checkpoints, scheduling, and guest mode (`trio.lowlevel`)
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `lowlevel.checkpoint()`                                                                                    | checkpoint     | unconditional yield + cancellation check                   |
|  [02]   | `lowlevel.checkpoint_if_cancelled()` / `lowlevel.cancel_shielded_checkpoint()`                             | checkpoint     | cancel-only check / yield shielded from cancellation       |
|  [03]   | `lowlevel.current_task()` / `lowlevel.current_root_task()` / `lowlevel.current_trio_token()` / `lowlevel.current_statistics()` | introspection  | running task / root task / loop token / `RunStatistics`    |
|  [04]   | `lowlevel.spawn_system_task(async_fn, *args, name=None, context=None)`                                     | scheduling     | spawn a nursery-free, shielded-from-`KeyboardInterrupt` system task |
|  [05]   | `lowlevel.wait_task_rescheduled(abort_func)` / `lowlevel.reschedule(task, next_send)`                      | scheduling     | the park/unpark pair every custom async primitive is built on |
|  [06]   | `lowlevel.wait_readable(fd)` / `lowlevel.wait_writable(fd)` / `lowlevel.notify_closing(fd)`                | readiness      | await raw fd/socket readiness; wake waiters before closing |
|  [07]   | `lowlevel.start_guest_run(async_fn, *args, run_sync_soon_threadsafe=..., done_callback=...)`               | guest mode     | interleave a trio run inside a foreign event loop (Qt/asyncio/GUI host) |
|  [08]   | `lowlevel.add_instrument(instrument)` / `lowlevel.remove_instrument(instrument)`                           | instrumentation | attach/detach an `Instrument` on the live run              |
|  [09]   | `lowlevel.enable_ki_protection(f)` / `lowlevel.disable_ki_protection(f)`                                   | KI control     | mark a region (un)protected from `KeyboardInterrupt`       |

[ENTRYPOINT_SCOPE]: deterministic testing (`trio.testing`)
- rail: concurrency

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `testing.trio_test(fn)`                                                                     | test decorator | run an `async def` test under `trio.run` (pytest-agnostic) |
|  [02]   | `testing.wait_all_tasks_blocked(cushion=0.0)`                                               | testing helper | yield until every other task is blocked (deterministic settle) |
|  [03]   | `testing.assert_checkpoints()` / `testing.assert_no_checkpoints()`                          | assertion CM   | assert a block does / does not execute a cancellation checkpoint |
|  [04]   | `testing.memory_stream_pair()` / `testing.lockstep_stream_pair()`                           | stream factory  | bidirectional in-memory / lockstep-synchronised test stream pair |
|  [05]   | `testing.memory_stream_pump(send, receive, *, max_bytes=None)`                              | stream pump    | manually shuttle bytes between a `MemorySendStream`/`MemoryReceiveStream` |
|  [06]   | `testing.check_two_way_stream(...)` / `check_one_way_stream(...)` / `check_half_closeable_stream(...)` | conformance | exhaustive contract test for a custom `Stream` implementation |
|  [07]   | `testing.open_stream_to_socket_listener(listener)`                                          | test connect   | dial a `SocketListener` directly without a real network round-trip |

## [04]-[IMPLEMENTATION_LAW]

[TRIO_TOPOLOGY]:
- nurseries enforce structured concurrency: `open_nursery()` is `async with`, every child started via `nursery.start_soon`/`nursery.start` completes before the block exits, and child failures aggregate into a `BaseExceptionGroup` — `run(..., strict_exception_groups=True)` is the trio default (unlike `anyio`, which historically wrapped single failures bare), so a child raise surfaces as a one-element group caught with `except*`.
- `nursery.start(async_fn, ...)` waits for the child to call `task_status.started(value)` and returns that value (the listener-ready / port-bound handshake); `nursery.start_soon(async_fn, ...)` fires-and-tracks with no readiness signal. `nursery.cancel_scope.cancel()` cancels the whole subtree.
- cancellation is level-triggered through `CancelScope(deadline=, shield=)`: a scope stays cancelled, every checkpoint inside a cancelled non-shielded scope re-raises `Cancelled`, and swallowing `Cancelled` without exiting the scope just re-raises at the next checkpoint. `fail_after`/`move_on_after` (relative) and `fail_at`/`move_on_at` (absolute) return scopes; `fail_*` raises `TooSlowError`, `move_on_*` sets `scope.cancelled_caught`. `CancelScope.cancel(reason=...)` records a human reason on the cancellation.
- `Cancelled` subclasses `BaseException` (not `Exception`) precisely so a broad `except Exception` cannot eat it; never catch `Cancelled` directly — let the scope own it.
- `open_memory_channel[T](max_buffer_size)` returns a `(MemorySendChannel[T], MemoryReceiveChannel[T])` pair; subscript the call for the item type, use `math.inf` for an unbounded buffer, `send_nowait`/`receive_nowait` raise `WouldBlock`, a closed receive end raises `BrokenResourceError` on send, and an exhausted-but-open channel raises `EndOfChannel` from `receive` (which cleanly ends an `async for`). `.clone()` mints additional send/receive ends for fan-in/fan-out; the channel closes when the last end of a side closes.
- transports speak one byte-stream verb set — `send_all` / `receive_some(max_bytes)` / `wait_send_all_might_not_block` / `send_eof` (half-close) / `aclose`. `open_ssl_over_tcp_stream` performs the TLS handshake lazily on first I/O (force it eagerly with `SSLStream.do_handshake()`); `SSLStream.unwrap()` recovers the raw `Stream` plus any trailing bytes. `DTLSEndpoint` multiplexes many `DTLSChannel` peers over one UDP socket.
- the `outcome` dependency types the thread/loop bridge: a worker result crosses back as an `outcome.Value`/`outcome.Error` that re-raises in the awaiting task, so `to_thread.run_sync`/`from_thread.run` preserve the original exception (and `Cancelled`) across the thread boundary instead of flattening it.
- `to_thread.run_sync` dispatches to a `CapacityLimiter`-bounded thread pool (default 40 tokens); `abandon_on_cancel=True` lets a cancelled call leave the thread running (use only for truly side-effect-free work). `from_thread.run`/`run_sync` (or a captured `TrioToken.run_sync_soon`) re-enter the loop from a worker thread; calls after the run ends raise `RunFinishedError`.
- the `Clock`/`Instrument` seams are trio-exclusive (no `anyio` equivalent): inject a `trio.testing.MockClock(autojump_threshold=0)` to collapse all idle waiting in a test, and attach an `Instrument` (or `lowlevel.add_instrument`) to observe `task_spawned`/`before_task_step`/`before_io_wait` for tracing/metrics without touching task code.
- `lowlevel.start_guest_run` interleaves a trio run on top of a foreign loop (asyncio, Qt, a GUI toolkit) by handing trio a `run_sync_soon_threadsafe` callback — the integration point when trio must coexist with a host loop it does not own.

[STACKS_WITH]:
- `anyio` (`.api/anyio.md`): trio is the high-scrutiny backend `anyio` runs on (`anyio.run(main, backend='trio')` / the `anyio_backend='trio'` pytest fixture). Domain and boundary code targets the `anyio` surface; reach for the bare `trio` surface only for what `anyio` does not expose — guest mode (`lowlevel.start_guest_run`), a custom `Clock`, `Instrument` hooks, `DTLS`, and the `trio.testing` autojump/checkpoint kit. The vocabularies are deliberately parallel: an `anyio` `CancelScope`/`create_task_group`/`create_memory_object_stream` maps one-to-one onto trio's `CancelScope`/`open_nursery`/`open_memory_channel`, so a backend swap is a `run` argument, never a rewrite.
- `stamina` (`.api/stamina.md`): wrap an `open_tcp_stream`/`run_process`/`to_thread.run_sync` effect in `retry_context(...)`; stamina detects the trio loop via `sniffio` and its retry sleep IS a `trio.sleep` checkpoint, so an enclosing `move_on_after`/`fail_after` deadline preempts a retry storm and a `Cancelled` cuts the wait. `RetryDetails` flows into the receipt stream unchanged.
- `msgspec`/`pydantic` (`.api/msgspec.md`, `.api/pydantic.md`): frame a `SocketStream` by looping `receive_some(max_bytes)` until a delimiter, then feed the frame to `msgspec.json.Decoder(type=T).decode(...)` for a zero-copy validated wire decode; a `MemoryReceiveChannel[T]` typed on a `msgspec.Struct` carries already-validated objects between tasks without re-serialization, and a tagged `Struct` union is the on-wire discriminant on both ends of the channel.
- `structlog`/`opentelemetry` (`.api/structlog.md`, `.api/opentelemetry-*.md`): bind the trace/log context in a `lowlevel.RunVar` (or a `contextvars.ContextVar`, which trio copies per spawned task) so it survives nursery hops and `from_thread` bridges; open one OTel span per task inside the child coroutine. An `Instrument` subclass is the trio-native sink for emitting a span/metric on `task_spawned`/`task_exited` across the whole run without instrumenting each task by hand.
- `expression` (`.api/expression.md`): a `to_thread.run_sync` / `run_process` boundary returns a value the caller lifts into `Result` via `result.of_option` or a try-builder; never let a `TooSlowError`/`BrokenResourceError`/`subprocess.CalledProcessError` escape the boundary adapter as a raw exception. `Cancelled` is the one exception that is re-raised untouched — it belongs to the owning `CancelScope`, not the `Result` rail.
- `cyclopts`/`grpcio` (`.api/cyclopts.md`, `.api/grpcio.md`): a sync CLI or servicer entrypoint hosts the async core with `trio.run(main)`; bridge a sync callback that must re-enter the loop through a captured `TrioToken.run_sync_soon`, and bound every blocking servicer body through `to_thread.run_sync(..., limiter=...)`.

[LOCAL_ADMISSION]:
- Spawn concurrent work only inside `open_nursery()`; never bare-`await` a fire-and-forget coroutine or hand-roll a task list — an unscoped task escapes cancellation and failure aggregation.
- Use `fail_after`/`move_on_after` (or the `_at` absolute forms) for every deadline-bounded I/O; the deadline rides the enclosing `CancelScope`, so it composes through nested calls without a per-call timeout argument.
- Use `open_tcp_stream`/`open_tcp_listeners`/`serve_tcp` and `open_ssl_over_tcp_stream`/`serve_ssl_over_tcp` and `open_signal_receiver` instead of raw `socket`/`ssl`/`signal` — those bypass cancellation, deadlines, and happy-eyeballs dialing.
- Use `to_thread.run_sync(..., limiter=...)` for blocking native/sync calls and `from_thread.run` to re-enter the loop from a thread; never `concurrent.futures`/`threading` directly, and pass an explicit `CapacityLimiter` to bound a subsystem.
- Use `trio.Path` for async filesystem work and `run_process` for subprocesses; never block the loop with sync `open`/`pathlib`/`subprocess`.
- Use `lowlevel.checkpoint()` in tight compute or polling loops so cancellation and fairness still apply; in tests, gate timing assumptions on `testing.wait_all_tasks_blocked()` and `testing.MockClock`, never wall-clock `sleep`.
- Never catch `Cancelled` or `BaseException` broadly; let the owning `CancelScope` re-raise it, and split real failures out of the `BaseExceptionGroup` with `except*`.

[RAIL_LAW]:
- Package: `trio`
- Owns: the structured-concurrency runtime — nurseries, level-triggered cancel scopes + deadlines, memory channels, `outcome`-typed thread/loop bridge, abstract stream/channel/listener/clock/instrument contracts, native TCP/UNIX/SSL/DTLS transports, async `Path`, subprocess, signal receivers, guest-mode interleaving, low-level checkpoints/parking, and the deterministic `trio.testing` kit
- Accept: `open_nursery` + `nursery.start`/`start_soon`, `fail_after`/`move_on_after`/`fail_at`/`move_on_at`, `open_memory_channel`, `open_tcp_stream`/`serve_tcp`/SSL+DTLS transports, `to_thread`/`from_thread` with explicit `CapacityLimiter`, `trio.Path`/`run_process`, `open_signal_receiver`, `lowlevel.checkpoint`/`start_guest_run`/`RunVar`/`Instrument`, `trio.testing` autojump + checkpoint assertions
- Reject: bare un-nurseried task spawns, raw `socket`/`ssl`/`signal`/`subprocess`/`threading`/`concurrent.futures`, blocking sync `open`/`pathlib`/`time.sleep` on the loop, catching `Cancelled`/`BaseException` broadly, `asyncio` primitives mixed into a trio run, and wall-clock timing in tests
