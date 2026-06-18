# [PY_BRANCH_API_ANYIO]

`anyio` supplies a backend-agnostic structured concurrency layer over asyncio and Trio: task groups, cancel scopes, memory object streams, async file and process I/O, synchronization primitives, thread interop, and networking primitives, all through a single stable API.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- module: `anyio`
- asset: runtime library
- rail: concurrency

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: task and cancellation
- rail: concurrency

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :---------------- | :------------ | :--------------------------- |
|   [1]   | `CancelScope`     | scope class   | scoped cancellation boundary |
|   [2]   | `TaskInfo`        | info record   | running task identity        |
|   [3]   | `TaskHandle`      | handle class  | handle to a spawned task     |
|   [4]   | `TaskCancelled`   | exception     | task was cancelled           |
|   [5]   | `TaskFailed`      | exception     | task raised an exception     |
|   [6]   | `TaskNotFinished` | exception     | result read before task done |

[PUBLIC_TYPE_SCOPE]: synchronization primitives
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------- | :------------ | :------------------------------ |
|   [1]   | `Event`                     | event class   | set-once async notification     |
|   [2]   | `Lock`                      | lock class    | async mutual exclusion          |
|   [3]   | `Semaphore`                 | sema class    | async counting semaphore        |
|   [4]   | `Condition`                 | cond class    | async condition variable        |
|   [5]   | `CapacityLimiter`           | limiter class | concurrent slot limiter         |
|   [6]   | `EventStatistics`           | stats record  | event subscriber count snapshot |
|   [7]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot     |
|   [8]   | `SemaphoreStatistics`       | stats record  | semaphore slot snapshot         |
|   [9]   | `CapacityLimiterStatistics` | stats record  | limiter slot snapshot           |

[PUBLIC_TYPE_SCOPE]: streams
- rail: concurrency

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [RAIL]                            |
| :-----: | :--------------------- | :-------------- | :-------------------------------- |
|   [1]   | `AsyncFile`            | async file      | async wrapper around file objects |
|   [2]   | `Path`                 | async path      | async filesystem path             |
|   [3]   | `TemporaryFile`        | context manager | async temporary file              |
|   [4]   | `NamedTemporaryFile`   | context manager | async named temporary file        |
|   [5]   | `TemporaryDirectory`   | context manager | async temporary directory         |
|   [6]   | `SpooledTemporaryFile` | context manager | async spooled temporary file      |

[PUBLIC_TYPE_SCOPE]: error types
- rail: concurrency

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                            |
| :-----: | :-------------------- | :------------- | :-------------------------------- |
|   [1]   | `EndOfStream`         | stream error   | stream exhausted                  |
|   [2]   | `ClosedResourceError` | resource error | operation on closed resource      |
|   [3]   | `BrokenResourceError` | resource error | unrecoverable stream/socket break |
|   [4]   | `BusyResourceError`   | resource error | concurrent operation in progress  |
|   [5]   | `WouldBlock`          | sync error     | operation would block in sync ctx |
|   [6]   | `DelimiterNotFound`   | read error     | delimiter absent within max bytes |
|   [7]   | `IncompleteRead`      | read error     | EOF before expected byte count    |
|   [8]   | `NoEventLoopError`    | thread error   | no running event loop for token   |
|   [9]   | `ConnectionFailed`    | net error      | connection could not be opened    |

[PUBLIC_TYPE_SCOPE]: ABC contracts
- rail: concurrency

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :---------------------- | :------------ | :------------------------------ |
|   [1]   | `abc.TaskGroup`         | interface     | structured task group contract  |
|   [2]   | `abc.AsyncResource`     | interface     | aclose-capable resource         |
|   [3]   | `abc.ByteStream`        | interface     | bidirectional byte stream       |
|   [4]   | `abc.ByteReceiveStream` | interface     | receive side of byte stream     |
|   [5]   | `abc.ByteSendStream`    | interface     | send side of byte stream        |
|   [6]   | `abc.Listener`          | interface     | accept-loop connection listener |
|   [7]   | `abc.BlockingPortal`    | interface     | thread-to-async bridge portal   |
|   [8]   | `abc.TaskStatus`        | interface     | task started notification       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event loop entry and tasks
- rail: concurrency

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                              |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------- |
|   [1]   | `run(func, *args, backend, backend_options)` | event loop entry | run async main in chosen backend    |
|   [2]   | `create_task_group()`                        | task group       | structured concurrency scope        |
|   [3]   | `get_current_task()`                         | introspection    | current TaskInfo                    |
|   [4]   | `get_running_tasks()`                        | introspection    | list of all running TaskInfo        |
|   [5]   | `wait_all_tasks_blocked()`                   | testing helper   | yield until all tasks are suspended |
|   [6]   | `get_cancelled_exc_class()`                  | backend query    | backend-specific cancellation class |

[ENTRYPOINT_SCOPE]: cancel scopes and timing
- rail: concurrency

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------- | :------------- | :----------------------------- |
|   [1]   | `fail_after(delay, shield)`    | cancel scope   | raise on deadline              |
|   [2]   | `move_on_after(delay, shield)` | cancel scope   | silently cancel on deadline    |
|   [3]   | `sleep(delay)`                 | sleep          | sleep for seconds              |
|   [4]   | `sleep_forever()`              | sleep          | sleep until cancelled          |
|   [5]   | `sleep_until(deadline)`        | sleep          | sleep until absolute time      |
|   [6]   | `current_time()`               | clock          | backend monotonic time         |
|   [7]   | `current_effective_deadline()` | clock          | nearest active cancel deadline |

[ENTRYPOINT_SCOPE]: memory streams
- rail: concurrency

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------- |
|   [1]   | `create_memory_object_stream(max_buffer_size, item_type)` | factory        | paired send/receive stream |

[ENTRYPOINT_SCOPE]: file and process I/O
- rail: concurrency

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------ | :------------- | :--------------------------- |
|   [1]   | `open_file(file, mode, ...)`    | file open      | async file handle            |
|   [2]   | `wrap_file(file)`               | file wrap      | wrap sync file as async      |
|   [3]   | `open_process(command, **opts)` | process open   | async subprocess             |
|   [4]   | `run_process(command, **opts)`  | process run    | run subprocess to completion |

[ENTRYPOINT_SCOPE]: thread interop
- rail: concurrency

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :------------------------------------------------------ | :-------------- | :---------------------------------- |
|   [1]   | `to_thread.run_sync(func, *args, limiter, cancellable)` | thread dispatch | run sync callable in worker thread  |
|   [2]   | `to_thread.current_default_thread_limiter()`            | limiter query   | global default thread limiter       |
|   [3]   | `from_thread.run(func, *args, ...)`                     | bridge call     | call async fn from worker thread    |
|   [4]   | `from_thread.run_sync(func, *args, ...)`                | bridge call     | call sync fn from event loop thread |
|   [5]   | `from_thread.start_blocking_portal(backend, ...)`       | portal factory  | blocking portal context manager     |

[ENTRYPOINT_SCOPE]: low-level checkpoints
- rail: concurrency

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `lowlevel.checkpoint()`                 | checkpoint     | unconditional yield to scheduler     |
|   [2]   | `lowlevel.checkpoint_if_cancelled()`    | checkpoint     | yield only if cancel scope tripped   |
|   [3]   | `lowlevel.cancel_shielded_checkpoint()` | checkpoint     | yield shielded from cancellation     |
|   [4]   | `lowlevel.RunVar`                       | run-local var  | per-event-loop contextvar equivalent |
|   [5]   | `lowlevel.get_async_backend()`          | backend query  | current running backend instance     |

## [4]-[IMPLEMENTATION_LAW]

[ANYIO_TOPOLOGY]:
- backends: `asyncio` (default) and `trio`; selected at `run(backend=...)` or test fixture level
- task groups use structured concurrency: all child tasks must complete before the group exits; exceptions propagate via `ExceptionGroup`
- `CancelScope` is the cancellation primitive; `fail_after`/`move_on_after` return one as a context manager
- `create_memory_object_stream` returns `(MemoryObjectSendStream, MemoryObjectReceiveStream)` as a paired tuple
- `to_thread.run_sync` dispatches to a `CapacityLimiter`-bounded thread pool; default limiter has 40 slots

[LOCAL_ADMISSION]:
- Use `create_task_group` for concurrent task launch; never use `asyncio.gather` or `asyncio.create_task` directly.
- Use `fail_after`/`move_on_after` for deadline-bounded I/O; never use `asyncio.wait_for` directly.
- Use `to_thread.run_sync` for blocking I/O in async context; pass an explicit `CapacityLimiter` for bounded pools.
- Use `lowlevel.checkpoint` in tight loops or polling code to yield control to the scheduler.

[RAIL_LAW]:
- Package: `anyio`
- Owns: structured concurrency, cancel scopes, memory streams, file/process async I/O, thread interop, sync primitives
- Accept: `create_task_group`, `fail_after`, `move_on_after`, `to_thread.run_sync`, `create_memory_object_stream`
- Reject: direct `asyncio.gather`, `asyncio.create_task`, `asyncio.wait_for` in code targeting anyio's abstraction layer
