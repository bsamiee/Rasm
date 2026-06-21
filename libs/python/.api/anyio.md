# [PY_BRANCH_API_ANYIO]

`anyio` supplies a backend-agnostic structured concurrency layer over asyncio and Trio: task groups, cancel scopes, memory object streams, async file and process I/O, synchronization primitives, thread interop, and networking primitives, all through a single stable API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- module: `anyio`
- asset: runtime library
- rail: concurrency

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: task and cancellation
- rail: concurrency

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :---------------- | :------------ | :--------------------------- |
|  [01]   | `CancelScope`     | scope class   | scoped cancellation boundary |
|  [02]   | `TaskInfo`        | info record   | running task identity        |
|  [03]   | `TaskHandle`      | handle class  | handle to a spawned task     |
|  [04]   | `TaskCancelled`   | exception     | task was cancelled           |
|  [05]   | `TaskFailed`      | exception     | task raised an exception     |
|  [06]   | `TaskNotFinished` | exception     | result read before task done |

[PUBLIC_TYPE_SCOPE]: synchronization primitives
- rail: concurrency

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------- | :------------ | :------------------------------ |
|  [01]   | `Event`                     | event class   | set-once async notification     |
|  [02]   | `Lock`                      | lock class    | async mutual exclusion          |
|  [03]   | `Semaphore`                 | sema class    | async counting semaphore        |
|  [04]   | `Condition`                 | cond class    | async condition variable        |
|  [05]   | `CapacityLimiter`           | limiter class | concurrent slot limiter         |
|  [06]   | `EventStatistics`           | stats record  | event subscriber count snapshot |
|  [07]   | `LockStatistics`            | stats record  | lock holder/waiter snapshot     |
|  [08]   | `SemaphoreStatistics`       | stats record  | semaphore slot snapshot         |
|  [09]   | `CapacityLimiterStatistics` | stats record  | limiter slot snapshot           |

[PUBLIC_TYPE_SCOPE]: streams
- rail: concurrency

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [RAIL]                            |
| :-----: | :--------------------- | :-------------- | :-------------------------------- |
|  [01]   | `AsyncFile`            | async file      | async wrapper around file objects |
|  [02]   | `Path`                 | async path      | async filesystem path             |
|  [03]   | `TemporaryFile`        | context manager | async temporary file              |
|  [04]   | `NamedTemporaryFile`   | context manager | async named temporary file        |
|  [05]   | `TemporaryDirectory`   | context manager | async temporary directory         |
|  [06]   | `SpooledTemporaryFile` | context manager | async spooled temporary file      |

[PUBLIC_TYPE_SCOPE]: error types
- rail: concurrency

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                         |
| :-----: | :------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `EndOfStream`             | stream error   | stream exhausted                               |
|  [02]   | `ClosedResourceError`     | resource error | operation on closed resource                   |
|  [03]   | `BrokenResourceError`     | resource error | unrecoverable stream/socket break              |
|  [04]   | `BusyResourceError`       | resource error | concurrent operation in progress               |
|  [05]   | `WouldBlock`              | sync error     | operation would block in sync ctx              |
|  [06]   | `DelimiterNotFound`       | read error     | delimiter absent within max bytes              |
|  [07]   | `IncompleteRead`          | read error     | EOF before expected byte count                 |
|  [08]   | `NoEventLoopError`        | thread error   | no running event loop for token                |
|  [09]   | `ConnectionFailed`        | net error      | connection could not be opened                 |
|  [10]   | `BrokenWorkerInterpreter` | worker error   | subinterpreter worker raised or died (PEP 734) |

[PUBLIC_TYPE_SCOPE]: ABC contracts
- rail: concurrency

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :---------------------- | :------------ | :------------------------------ |
|  [01]   | `abc.TaskGroup`         | interface     | structured task group contract  |
|  [02]   | `abc.AsyncResource`     | interface     | aclose-capable resource         |
|  [03]   | `abc.ByteStream`        | interface     | bidirectional byte stream       |
|  [04]   | `abc.ByteReceiveStream` | interface     | receive side of byte stream     |
|  [05]   | `abc.ByteSendStream`    | interface     | send side of byte stream        |
|  [06]   | `abc.Listener`          | interface     | accept-loop connection listener |
|  [07]   | `abc.BlockingPortal`    | interface     | thread-to-async bridge portal   |
|  [08]   | `abc.TaskStatus`        | interface     | task started notification       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event loop entry and tasks
- rail: concurrency

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                              |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `run(func, *args, backend, backend_options)` | event loop entry | run async main in chosen backend    |
|  [02]   | `create_task_group()`                        | task group       | structured concurrency scope        |
|  [03]   | `get_current_task()`                         | introspection    | current TaskInfo                    |
|  [04]   | `get_running_tasks()`                        | introspection    | list of all running TaskInfo        |
|  [05]   | `wait_all_tasks_blocked()`                   | testing helper   | yield until all tasks are suspended |
|  [06]   | `get_cancelled_exc_class()`                  | backend query    | backend-specific cancellation class |

[ENTRYPOINT_SCOPE]: cancel scopes and timing
- rail: concurrency

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------- | :------------- | :----------------------------- |
|  [01]   | `fail_after(delay, shield)`    | cancel scope   | raise on deadline              |
|  [02]   | `move_on_after(delay, shield)` | cancel scope   | silently cancel on deadline    |
|  [03]   | `sleep(delay)`                 | sleep          | sleep for seconds              |
|  [04]   | `sleep_forever()`              | sleep          | sleep until cancelled          |
|  [05]   | `sleep_until(deadline)`        | sleep          | sleep until absolute time      |
|  [06]   | `current_time()`               | clock          | backend monotonic time         |
|  [07]   | `current_effective_deadline()` | clock          | nearest active cancel deadline |

[ENTRYPOINT_SCOPE]: memory streams
- rail: concurrency

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `create_memory_object_stream(max_buffer_size, item_type)` | factory        | paired send/receive stream |

[ENTRYPOINT_SCOPE]: file and process I/O
- rail: concurrency

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------ | :------------- | :--------------------------- |
|  [01]   | `open_file(file, mode, ...)`    | file open      | async file handle            |
|  [02]   | `wrap_file(file)`               | file wrap      | wrap sync file as async      |
|  [03]   | `open_process(command, **opts)` | process open   | async subprocess             |
|  [04]   | `run_process(command, **opts)`  | process run    | run subprocess to completion |

[ENTRYPOINT_SCOPE]: thread interop
- rail: concurrency

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]   | [RAIL]                                        |
| :-----: | :------------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `to_thread.run_sync(func, *args, limiter, cancellable)`  | thread dispatch  | run sync callable in worker thread            |
|  [02]   | `to_thread.current_default_thread_limiter()`             | limiter query    | global default thread limiter                 |
|  [03]   | `from_thread.run(func, *args, ...)`                      | bridge call      | call async fn from worker thread              |
|  [04]   | `from_thread.run_sync(func, *args, ...)`                 | bridge call      | call sync fn from event loop thread           |
|  [05]   | `from_thread.start_blocking_portal(backend, ...)`        | portal factory   | blocking portal context manager               |
|  [06]   | `to_interpreter.run_sync(func, *args, limiter=None)`     | interp dispatch  | run sync callable in a PEP-734 subinterpreter |
|  [07]   | `to_process.run_sync(func, *args, cancellable, limiter)` | process dispatch | run sync callable in a worker subprocess      |

[ENTRYPOINT_SCOPE]: low-level checkpoints
- rail: concurrency

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `lowlevel.checkpoint()`                 | checkpoint     | unconditional yield to scheduler     |
|  [02]   | `lowlevel.checkpoint_if_cancelled()`    | checkpoint     | yield only if cancel scope tripped   |
|  [03]   | `lowlevel.cancel_shielded_checkpoint()` | checkpoint     | yield shielded from cancellation     |
|  [04]   | `lowlevel.RunVar`                       | run-local var  | per-event-loop contextvar equivalent |
|  [05]   | `lowlevel.get_async_backend()`          | backend query  | current running backend instance     |

## [04]-[IMPLEMENTATION_LAW]

[ANYIO_TOPOLOGY]:
- backends: `asyncio` (default) and `trio`; selected at `run(backend=...)` or test fixture level
- task groups use structured concurrency: all child tasks must complete before the group exits; exceptions propagate via `ExceptionGroup`
- `CancelScope` is the cancellation primitive; `fail_after`/`move_on_after` return one as a context manager
- `create_memory_object_stream` returns `(MemoryObjectSendStream, MemoryObjectReceiveStream)` as a paired tuple
- `to_thread.run_sync` dispatches to a `CapacityLimiter`-bounded thread pool; default limiter has 40 slots
- `to_interpreter.run_sync(func, *args, limiter=None)` dispatches a sync callable into a PEP-734 subinterpreter (each carrying its own GIL on a runnable `concurrent.interpreters`), bounded by an optional `CapacityLimiter`, with no pickle round-trip on the worker hop; a worker raise or death surfaces as `BrokenWorkerInterpreter`
- `to_process.run_sync` dispatches into a worker subprocess for true process isolation; arguments and results cross by pickle

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
