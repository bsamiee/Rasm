# [PY_RUNTIME_API_AIOCRON]

`aiocron` supplies asyncio-native cron scheduling: the `Cron` class drives repeating or one-shot coroutine invocations against a `cronsim`-parsed cron expression, and `crontab` is the module-level factory shorthand that starts the schedule immediately by default.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `aiocron`
- package: `aiocron`
- module: `aiocron`
- asset: runtime library
- rail: schedule

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scheduling
- rail: schedule

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [RAIL]                              |
| :-----: | :-------------- | :-------------- | :---------------------------------- |
|   [1]   | `Cron`          | scheduler class | repeating or one-shot async job     |
|   [2]   | `null_callback` | async function  | no-op coroutine default placeholder |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and decoration
- rail: schedule

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [RAIL]                               |
| :-----: | :------------------------------------------------------ | :---------------- | :----------------------------------- |
|   [1]   | `Cron(spec, func, args, kwargs, start, uuid, loop, tz)` | constructor       | create job, optionally auto-start    |
|   [2]   | `crontab(spec, func, args, kwargs, start, loop, tz)`    | factory function  | `Cron` factory, `start=True` default |
|   [3]   | `wrap_func(func)`                                       | coroutine wrapper | coerce sync or async callable        |
|   [4]   | `Cron.__call__(func)`                                   | decorator         | bind callable after construction     |

[ENTRYPOINT_SCOPE]: lifecycle and execution
- rail: schedule

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------ | :------------- | :--------------------------------------- |
|   [1]   | `Cron.start()`            | lifecycle      | schedule first hop, cancels prior handle |
|   [2]   | `Cron.stop()`             | lifecycle      | cancel pending handle, reset state       |
|   [3]   | `Cron.next(*args)`        | one-shot await | `await` returns next execution result    |
|   [4]   | `Cron.initialize()`       | internal setup | lazy `CronSim` + loop-time anchor init   |
|   [5]   | `Cron.get_next()`         | timing         | loop-relative timestamp of next tick     |
|   [6]   | `Cron.call_next()`        | dispatch       | reschedule and fire in repeating mode    |
|   [7]   | `Cron.call_func(*args)`   | dispatch       | dispatch via `asyncio.gather`            |
|   [8]   | `Cron.set_result(result)` | result sink    | resolve or raise on pending future       |

## [4]-[IMPLEMENTATION_LAW]

[SCHEDULE_TOPOLOGY]:
- spec format: any cron expression accepted by `cronsim.CronSim` (five-field standard)
- timezone: defaults to `tzlocal.get_localzone()`; supply any `datetime.tzinfo`-compatible zone via `tz`
- loop: defaults to `asyncio.get_event_loop()` at construction time; explicit `loop` injection available
- function wrapping: `wrap_func` normalises sync callables to coroutines; `partial` binds `args`/`kwargs` at construction
- `start=True` at construction calls `loop.call_soon_threadsafe(self.start)` immediately if `func` is not `null_callback`

[EXECUTION_MODEL]:
- repeating mode: `start()` schedules `call_next()` which reschedules itself each tick; `stop()` cancels the handle
- one-shot mode: `await cron.next()` schedules a single execution and returns the result; concurrent calls each produce an independent future
- exception handling: `asyncio.gather(return_exceptions=True)` collects results; exceptions propagate via `future.set_exception` or direct raise when no future is pending
- `uuid` identifies the job and defaults to `uuid4()` when omitted

[RAIL_LAW]:
- package: `aiocron`
- owns: asyncio cron scheduling backed by `cronsim` expression parsing
- accept: coroutine functions, sync callables wrapped by `wrap_func`, `crontab` factory for fire-and-forget schedules
- reject: hand-rolled asyncio scheduling loops, direct `cronsim.CronSim` management outside this class
