# [PY_RUNTIME_API_ANYIO]

`anyio` supplies the structured-concurrency core for the runtime branch: task groups, cancel scopes, capacity limiters, deadlines, memory object streams, thread/process/subprocess offload, an async filesystem path, and blocking-portal bridges. It is the single concurrency owner the lane policy and drain surfaces compose.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- import: `anyio`
- version: `4.13.0`
- owner: `runtime`
- rail: concurrency
- namespaces: `anyio`, `anyio.abc`, `anyio.lowlevel`, `anyio.to_thread`, `anyio.from_thread`, `anyio.to_process`, `anyio.streams.memory`, `anyio.streams.buffered`, `anyio.streams.text`, `anyio.streams.tls`, `anyio.streams.stapled`, `anyio.streams.file`
- capability: structured concurrency, cancellation, capacity limiting, deadlines, memory/byte streams, thread/process offload, blocking-portal bridges, async filesystem path

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: task and cancellation family
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [RAIL]                        |
| :-----: | :-------------------------- | :----------------- | :---------------------------- |
|   [1]   | `CancelScope`               | scope              | bounded cancellation region   |
|   [2]   | `abc.TaskGroup`             | task group         | structured child-task nursery |
|   [3]   | `abc.TaskStatus`            | task status        | started-signal handshake      |
|   [4]   | `TASK_STATUS_IGNORED`       | sentinel           | no-op task-status target      |
|   [5]   | `TaskInfo`                  | task introspection | running-task descriptor       |
|   [6]   | `CapacityLimiter`           | limiter            | bounded concurrent-token gate |
|   [7]   | `CapacityLimiterStatistics` | stats              | limiter observation           |
|   [8]   | `Semaphore`                 | primitive          | counting concurrency gate     |
|   [9]   | `Lock`                      | primitive          | mutual exclusion              |
|  [10]   | `Condition`                 | primitive          | wait/notify coordination      |
|  [11]   | `Event`                     | primitive          | one-shot signal               |
|  [12]   | `ResourceGuard`             | guard              | single-borrow resource guard  |

[PUBLIC_TYPE_SCOPE]: stream and resource family
- rail: concurrency

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------------- | :------------ | :------------------------- |
|   [1]   | `abc.ObjectSendStream`                       | stream        | typed object send half     |
|   [2]   | `abc.ObjectReceiveStream`                    | stream        | typed object receive half  |
|   [3]   | `abc.ByteStream`                             | stream        | bidirectional byte stream  |
|   [4]   | `streams.memory.MemoryObjectSendStream`      | stream        | in-process send channel    |
|   [5]   | `streams.memory.MemoryObjectReceiveStream`   | stream        | in-process receive channel |
|   [6]   | `streams.buffered.BufferedByteReceiveStream` | stream        | buffered byte reader       |
|   [7]   | `streams.text.TextReceiveStream`             | stream        | decoded text reader        |
|   [8]   | `abc.AsyncResource`                          | resource      | async-closeable contract   |
|   [9]   | `abc.Listener`                               | listener      | accept-loop server socket  |
|  [10]   | `abc.SocketStream`                           | socket        | connected stream socket    |
|  [11]   | `Path`                                       | filesystem    | async pathlib-shaped path  |
|  [12]   | `AsyncFile`                                  | filesystem    | async file handle          |
|  [13]   | `TemporaryDirectory`                         | filesystem    | async temp directory       |
|  [14]   | `NamedTemporaryFile`                         | filesystem    | async named temp file      |

[PUBLIC_TYPE_SCOPE]: portal and backend family
- rail: concurrency

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]  | [RAIL]                       |
| :-----: | :----------------------------------- | :------------- | :--------------------------- |
|   [1]   | `from_thread.BlockingPortal`         | portal         | sync→async call bridge       |
|   [2]   | `from_thread.BlockingPortalProvider` | portal factory | shared-loop portal source    |
|   [3]   | `abc.AsyncBackend`                   | backend        | pluggable event-loop backend |
|   [4]   | `TypedAttributeProvider`             | attribute      | typed-attribute lookup base  |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :------------------------ | :------------ | :-------------------------- |
|   [1]   | `BrokenResourceError`     | fault         | resource broken mid-use     |
|   [2]   | `ClosedResourceError`     | fault         | use after close             |
|   [3]   | `BusyResourceError`       | fault         | concurrent-borrow violation |
|   [4]   | `EndOfStream`             | fault         | stream exhausted            |
|   [5]   | `IncompleteRead`          | fault         | short read                  |
|   [6]   | `DelimiterNotFound`       | fault         | delimiter scan overrun      |
|   [7]   | `WouldBlock`              | fault         | non-blocking statement fail |
|   [8]   | `BrokenWorkerProcess`     | fault         | process-worker crash        |
|   [9]   | `BrokenWorkerInterpreter` | fault         | subinterpreter-worker crash |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle and cancellation
- rail: concurrency

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]   | [RAIL]                              |
| :-----: | :--------------------------- | :--------------- | :---------------------------------- |
|   [1]   | `run`                        | event-loop entry | run async function on a backend     |
|   [2]   | `create_task_group`          | nursery          | structured child-task group         |
|   [3]   | `CancelScope`                | scope            | deadline/shield cancellation region |
|   [4]   | `fail_after`                 | deadline         | timeout-or-raise context            |
|   [5]   | `move_on_after`              | deadline         | timeout-or-continue context         |
|   [6]   | `current_effective_deadline` | deadline read    | active scope deadline               |
|   [7]   | `current_time`               | clock            | backend monotonic clock             |
|   [8]   | `sleep`                      | yield            | cancellable sleep                   |
|   [9]   | `sleep_forever`              | yield            | cancellable park                    |
|  [10]   | `get_cancelled_exc_class`    | fault class      | backend cancellation type           |
|  [11]   | `aclose_forcefully`          | teardown         | best-effort async close             |

[ENTRYPOINT_SCOPE]: streams and offload
- rail: concurrency

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `create_memory_object_stream`                | channel        | typed bounded in-process channel    |
|   [2]   | `to_thread.run_sync`                         | offload        | run blocking call on worker thread  |
|   [3]   | `to_thread.current_default_thread_limiter`   | offload        | shared thread-pool limiter          |
|   [4]   | `to_process.run_sync`                        | offload        | run callable in worker process      |
|   [5]   | `to_process.current_default_process_limiter` | offload        | shared process-pool limiter         |
|   [6]   | `to_interpreter.run_sync`                    | offload        | run callable in subinterpreter      |
|   [7]   | `from_thread.run`                            | bridge         | call coroutine from worker thread   |
|   [8]   | `from_thread.run_sync`                       | bridge         | call sync portal target from thread |
|   [9]   | `from_thread.start_blocking_portal`          | bridge         | spin loop-backed portal             |
|  [10]   | `run_process`                                | subprocess     | run+await external process          |
|  [11]   | `open_process`                               | subprocess     | spawn streamed subprocess           |
|  [12]   | `connect_tcp`                                | socket         | client TCP stream                   |
|  [13]   | `connect_unix`                               | socket         | client UNIX stream                  |
|  [14]   | `create_tcp_listener`                        | socket         | server TCP listener                 |

## [4]-[IMPLEMENTATION_LAW]

[CONCURRENCY_TOPOLOGY]:
- backend: asyncio is the admitted backend; trio backend present but the runtime composes asyncio only.
- nursery law: every concurrent fan-out is a `create_task_group` block; child cancellation is structured, never an orphan `asyncio.create_task`.
- deadline law: timeouts are `fail_after`/`move_on_after` scopes carrying the `Deadline` value the context owner threads; no `asyncio.wait_for` and no raw `asyncio.timeout`.
- capacity law: bounded concurrency is one `CapacityLimiter` per lane; `LanePolicy` carries the token count, never an ad hoc semaphore loop.
- channel law: producer/consumer fan-out uses `create_memory_object_stream` typed send/receive halves with explicit buffer size; no `asyncio.Queue`.
- offload law: blocking work is `to_thread.run_sync` under the shared limiter; CPU-bound isolation is `to_process.run_sync`; the sync→async ingress is one `BlockingPortalProvider`.

[LOCAL_ADMISSION]:
- The lane surface composes task groups, cancel scopes, and capacity limiters; it never re-implements them.
- Deadline values flow from the context owner into `fail_after`; the lane never reads a wall clock.
- Drain receipts capture cancelled/limited statistics from `CapacityLimiterStatistics`, never a hand-rolled counter.

[RAIL_LAW]:
- Package: `anyio`
- Owns: structured concurrency, cancellation, capacity limiting, deadlines, in-process channels, and thread/process/subprocess offload
- Accept: task-group fan-out, scoped deadlines, limiter-gated lanes, portal bridges
- Reject: bare `asyncio` task spawning, `asyncio.Queue`, `wait_for` timeouts, and hand-rolled semaphore loops
