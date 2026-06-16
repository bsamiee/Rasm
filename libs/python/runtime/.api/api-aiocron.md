# [PY_RUNTIME_API_AIOCRON]

`aiocron` supplies asyncio cron scheduling: a `crontab` factory and `Cron` object that fire a coroutine on a cron expression, with timezone resolution and start/stop control. It is the runtime owner for cron-style local schedule hooks feeding the automation lanes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `aiocron`
- package: `aiocron`
- import: `aiocron`
- version: `2.1`
- owner: `runtime`
- rail: automation
- namespaces: `aiocron`
- capability: asyncio cron scheduling, cron-expression simulation, timezone-aware firing, start/stop control

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schedule family
- rail: automation

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Cron` | schedule | cron-driven coroutine runner |
| [2] | `CronSim` | simulator | next-fire-time iterator |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schedule operations
- rail: automation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `crontab` | build | construct a `Cron` from an expression |
| [2] | `Cron.start` | control | begin firing on schedule |
| [3] | `Cron.stop` | control | stop the schedule |
| [4] | `Cron.next` | query | await the next fire |
| [5] | `CronSim` | iterate | enumerate upcoming fire times |

## [4]-[IMPLEMENTATION_LAW]

[AUTOMATION_TOPOLOGY]:
- schedule law: cron-driven work is one `crontab(expr, func=..., tz=...)` whose `Cron` is started under the anyio lane and stopped in the lane drain; no manual sleep-until-next loop.
- timezone law: the schedule timezone is resolved explicitly from the settings model; the runtime never assumes a process-default timezone.
- coroutine law: the scheduled callable is a coroutine returning `Result`; a fire failure is recorded on the receipt surface, never an unhandled task exception.
- lifecycle law: `start`/`stop` are bound to the lane lifecycle; the schedule never outlives its lane.

[LOCAL_ADMISSION]:
- The automation lane composes `aiocron` for cron hooks alongside the `watchfiles` change stream; the lane policy owns concurrency, aiocron owns the schedule.
- This is a local schedule hook, not a distributed scheduler or job-queue service.

[RAIL_LAW]:
- Package: `aiocron`
- Owns: cron-style local schedule hooks with timezone-aware firing and start/stop control
- Accept: `crontab` schedules under the lane, explicit timezone, `Result`-returning coroutines, lane-bound lifecycle
- Reject: manual sleep-until loops, process-default timezone assumptions, unhandled fire exceptions, a distributed scheduler
