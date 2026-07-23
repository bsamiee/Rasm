# [PY_RUNTIME_API_APSCHEDULER]

`APScheduler` owns runtime's in-process job scheduling: one scheduler instance drives a bounded trigger vocabulary against a `Job` handle over pluggable persistent stores and executors, routes runs through a listener event bus, and migrates schedules by import/export. It is the sole `scheduled`-source owner on the one `AsyncIOScheduler`, each trigger type closing one row of the `execution/lanes` `Trigger` union.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `APScheduler`
- package: `APScheduler` (`MIT`)
- module: `apscheduler`
- namespaces: `apscheduler.schedulers`, `apscheduler.triggers`, `apscheduler.jobstores`, `apscheduler.executors`, `apscheduler.events`, `apscheduler.job`
- rail: scheduling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scheduler family

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------ |
|  [01]   | `schedulers.base.BaseScheduler`             | abstract base | shared scheduler contract (all entrypoints) |
|  [02]   | `schedulers.background.BackgroundScheduler` | scheduler     | daemon-thread, non-blocking                 |
|  [03]   | `schedulers.asyncio.AsyncIOScheduler`       | scheduler     | asyncio event-loop integration              |
|  [04]   | `schedulers.blocking.BlockingScheduler`     | scheduler     | blocks the calling thread in `start()`      |

[PUBLIC_TYPE_SCOPE]: trigger family

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------- |
|  [01]   | `triggers.cron.CronTrigger`                         | cron trigger      | field-based cron schedule    |
|  [02]   | `triggers.interval.IntervalTrigger`                 | interval trigger  | fixed time interval          |
|  [03]   | `triggers.date.DateTrigger`                         | date trigger      | single one-time fire         |
|  [04]   | `triggers.calendarinterval.CalendarIntervalTrigger` | calendar trigger  | calendar-unit interval       |
|  [05]   | `triggers.combining.AndTrigger`                     | combining trigger | fire when all triggers agree |
|  [06]   | `triggers.combining.OrTrigger`                      | combining trigger | fire when any trigger fires  |

[PUBLIC_TYPE_SCOPE]: store and executor family

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `jobstores.base.BaseJobStore`             | abstract base | job store contract                                |
|  [02]   | `jobstores.memory.MemoryJobStore`         | job store     | in-process volatile (default)                     |
|  [03]   | `jobstores.sqlalchemy.SQLAlchemyJobStore` | job store     | persistent SQL store, survives restart            |
|  [04]   | `jobstores.redis.RedisJobStore`           | job store     | persistent Redis store                            |
|  [05]   | `jobstores.mongodb.MongoDBJobStore`       | job store     | persistent MongoDB store                          |
|  [06]   | `executors.base.BaseExecutor`             | abstract base | executor contract                                 |
|  [07]   | `executors.pool.ThreadPoolExecutor`       | executor      | thread-pool execution (default for sync)          |
|  [08]   | `executors.pool.BasePoolExecutor`         | abstract base | pool-executor base contract                       |
|  [09]   | `executors.pool.ProcessPoolExecutor`      | executor      | process-pool execution (CPU-bound)                |
|  [10]   | `executors.asyncio.AsyncIOExecutor`       | executor      | coroutine execution on the asyncio loop           |
|  [11]   | `executors.debug.DebugExecutor`           | executor      | synchronous in-line execution (tests/diagnostics) |

[PUBLIC_TYPE_SCOPE]: job and event family

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `job.Job`                   | job object    | live handle with lifecycle ops + properties                    |
|  [02]   | `events.SchedulerEvent`     | event         | scheduler lifecycle event (`code`/`alias`)                     |
|  [03]   | `events.JobEvent`           | event         | job added/removed/modified event                               |
|  [04]   | `events.JobSubmissionEvent` | event         | job submitted to executor (carries `scheduled_run_times`)      |
|  [05]   | `events.JobExecutionEvent`  | event         | job executed/missed/errored (`retval`/`exception`/`traceback`) |

[EVENT_CONSTRUCTORS]:
- [02]-[SCHEDULER_EVENT]: `SchedulerEvent(code, alias=None)`.
- [03]-[JOB_EVENT]: `JobEvent(code, job_id, jobstore)`.
- [04]-[SUBMISSION_EVENT]: `JobSubmissionEvent(code, job_id, jobstore, scheduled_run_times)`.
- [05]-[EXECUTION_EVENT]: `JobExecutionEvent(code, job_id, jobstore, scheduled_run_time, retval=None, exception=None, traceback=None)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: job management
- defined on `BaseScheduler` (PUBLIC_TYPES [01]); all three concrete schedulers inherit these.

`add_job` returns a `Job`; `scheduled_job` is its decorator twin, dropping `func`/`replace_existing`. Both share the tail `id=None, name=None, misfire_grace_time=<undefined>, coalesce=<undefined>, max_instances=<undefined>, next_run_time=<undefined>, jobstore='default', executor='default', **trigger_args`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]        | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `add_job(func, trigger=None, args=None, kwargs=None, ...)`            | add job        | register a callable as a job (returns `Job`) |
|  [02]   | `scheduled_job(trigger, args=None, kwargs=None, ...)`                 | decorator      | decorate a function as a scheduled job       |
|  [03]   | `modify_job(job_id, jobstore=None, **changes)`                        | job modify     | change job attributes in place               |
|  [04]   | `reschedule_job(job_id, jobstore=None, trigger=None, **trigger_args)` | job reschedule | swap a job's trigger                         |
|  [05]   | `remove_job(job_id, jobstore=None)`                                   | job remove     | remove one job                               |
|  [06]   | `remove_all_jobs(jobstore=None)`                                      | job remove     | remove every job                             |
|  [07]   | `get_job(job_id, jobstore=None) -> Job\|None`                         | job query      | retrieve one job                             |
|  [08]   | `get_jobs(jobstore=None, pending=None) -> list[Job]`                  | job query      | list jobs                                    |
|  [09]   | `pause_job(job_id, jobstore=None)`                                    | job lifecycle  | pause one job                                |
|  [10]   | `resume_job(job_id, jobstore=None)`                                   | job lifecycle  | resume one job                               |
|  [11]   | `print_jobs(jobstore=None, out=None)`                                 | job io         | render jobs (`out=None` -> `sys.stdout`)     |
|  [12]   | `export_jobs(outfile, jobstore=None)`                                 | job io         | serialize jobs (stream first)                |
|  [13]   | `import_jobs(infile, jobstore='default')`                             | job io         | restore jobs (stream first)                  |

[ENTRYPOINT_SCOPE]: scheduler lifecycle

| [INDEX] | [SURFACE]                                                 | [SHAPE]   | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------- | :-------- | :-------------------------------------------------- |
|  [01]   | `configure(gconfig={}, prefix='apscheduler.', **options)` | configure | apply jobstore/executor/job-defaults before `start` |
|  [02]   | `start(paused=False)`                                     | lifecycle | start scheduler (optionally paused)                 |
|  [03]   | `shutdown(wait=True)`                                     | lifecycle | stop scheduler, optionally drain running jobs       |
|  [04]   | `pause()` / `resume()`                                    | lifecycle | pause / resume all job processing                   |
|  [05]   | `wakeup()`                                                | lifecycle | force a wakeup to re-evaluate next run times        |
|  [06]   | `running` (property)                                      | property  | `state != STATE_STOPPED`, True even while paused    |
|  [07]   | `state` (int attr)                                        | property  | public integer state; values in [07]                |

- [07]-[STATE]: `STATE_STOPPED=0`/`STATE_RUNNING=1`/`STATE_PAUSED=2`, exported from `apscheduler.schedulers.base`.

[ENTRYPOINT_SCOPE]: infrastructure and listeners

| [INDEX] | [SURFACE]                                                        | [SHAPE]         | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `add_jobstore(jobstore, alias='default', **opts)`                | store add       | register a job store                         |
|  [02]   | `remove_jobstore(alias, shutdown=True)`                          | store remove    | deregister a job store                       |
|  [03]   | `add_executor(executor, alias='default', **opts)`                | executor add    | register an executor                         |
|  [04]   | `remove_executor(alias, shutdown=True)`                          | executor remove | deregister an executor                       |
|  [05]   | `add_listener(callback, mask=EVENT_ALL)` (`EVENT_ALL == 131071`) | listener        | subscribe to scheduler/job events by bitmask |
|  [06]   | `remove_listener(callback)`                                      | listener        | unsubscribe a listener                       |

[ENTRYPOINT_SCOPE]: trigger constructors
- every field-based trigger constructor ends with `start_date, end_date, timezone, jitter`, elided as `...` below.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]      | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------- | :----------- | :------------------------------ |
|  [01]   | `CronTrigger(year, month, day, week, day_of_week, hour, minute, second, ...)`    | cron         | field-based cron schedule       |
|  [02]   | `CronTrigger.from_crontab(expr, timezone=None)`                                  | factory      | construct from standard crontab |
|  [03]   | `IntervalTrigger(weeks, days, hours, minutes, seconds, ...)`                     | interval     | fixed interval trigger          |
|  [04]   | `DateTrigger(run_date, timezone)`                                                | date trigger | single one-time fire            |
|  [05]   | `CalendarIntervalTrigger(years, months, weeks, days, hour, minute, second, ...)` | calendar     | calendar-unit trigger           |
|  [06]   | `AndTrigger(triggers, jitter)` / `OrTrigger(triggers, jitter)`                   | combining    | all-agree / first-fires gate    |

[ENTRYPOINT_SCOPE]: Job object operations
- defined on `job.Job` (PUBLIC_TYPES [01]).

| [INDEX] | [SURFACE]                                                     | [SHAPE]        | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `job.modify(**changes)`                                       | job modify     | change attributes of a live job              |
|  [02]   | `job.reschedule(trigger, **trigger_args)`                     | job reschedule | swap the trigger                             |
|  [03]   | `job.pause()` / `job.resume()` / `job.remove()`               | lifecycle      | pause / resume / remove                      |
|  [04]   | `job.id`, `job.name`, `job.func`                              | property       | identity fields                              |
|  [05]   | `job.func_ref`, `job.trigger`, `job.executor`                 | property       | binding fields                               |
|  [06]   | `job.args`, `job.kwargs`, `job.next_run_time`, `job.coalesce` | property       | call args, next fire, coalesce               |
|  [07]   | `job.max_instances`, `job.misfire_grace_time`, `job.pending`  | property       | concurrency cap, misfire grace, pending flag |

[ENTRYPOINT_SCOPE]: event codes (`apscheduler.events`)

| [INDEX] | [SURFACE]                                           | [SHAPE]    | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------------------ |
|  [01]   | `EVENT_SCHEDULER_START` / `EVENT_SCHEDULER_STARTED` | event code | scheduler started (both names exported)           |
|  [02]   | `EVENT_SCHEDULER_SHUTDOWN` / `_PAUSED` / `_RESUMED` | event code | scheduler lifecycle transitions                   |
|  [03]   | `EVENT_JOBSTORE_ADDED` / `EVENT_JOBSTORE_REMOVED`   | event code | job store registration                            |
|  [04]   | `EVENT_EXECUTOR_ADDED` / `EVENT_EXECUTOR_REMOVED`   | event code | executor registration                             |
|  [05]   | `EVENT_JOB_ADDED` / `_REMOVED` / `_MODIFIED`        | event code | job registry changes                              |
|  [06]   | `EVENT_ALL_JOBS_REMOVED`                            | event code | every job cleared                                 |
|  [07]   | `EVENT_JOB_SUBMITTED` / `_MAX_INSTANCES`            | event code | submitted to executor / concurrency cap hit       |
|  [08]   | `EVENT_JOB_EXECUTED` / `_ERROR` / `_MISSED`         | event code | ran / raised / missed its fire time               |
|  [09]   | `EVENT_ALL`                                         | event code | bitmask OR of every event (default listener mask) |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One scheduler instance owns every job. Responsive service code binds `AsyncIOScheduler` on the anyio lane, never `BlockingScheduler`. `running` reports `state != STATE_STOPPED`, true even while paused; a paused-vs-active test reads `state` against `STATE_PAUSED`.
- `misfire_grace_time`/`coalesce`/`max_instances`/`next_run_time` default to the `<undefined>` sentinel that falls back to scheduler `job_defaults`, distinct from an explicit `None`; `jobstore`/`executor` are explicit keyword args outside `**trigger_args`.
- `MemoryJobStore` (alias `'default'`) is volatile; a durable schedule registers a persistent store before `start()`, whose backend package must be present and whose job functions are importable module-level callables the store can serialize.
- `CronTrigger.from_crontab` intakes external crontab strings, field construction drives programmatic schedules, `jitter` caps fire-time randomization against thundering herds, and `AndTrigger`/`OrTrigger` compose existing triggers rather than minting a type.
- Jobstores, executors, and `job_defaults` apply through `configure(...)` or the constructor before `start()`; post-start infrastructure mutates through `add_*`/`remove_*`, never attribute assignment.
- Failures surface through the event bus: `EVENT_JOB_ERROR` reads `JobExecutionEvent.exception`/`.traceback` and `EVENT_JOB_EXECUTED` reads `.retval`, never inline return inspection.

[STACKING]:
- `stamina`(`.api/stamina.md`): a coroutine job body opens `stamina.retry_context` for its own transient retries while the scheduler's `misfire_grace_time`/`coalesce` own missed-fire policy — orthogonal rails.
- `AsyncIOScheduler` + `AsyncIOExecutor` run coroutine jobs on the anyio loop; the single `add_listener(..., EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED)` seam emits the run's OTel span and receipt fact from `JobExecutionEvent` fields, never a wrapper around the job function.

[LOCAL_ADMISSION]:
- One scheduler constructs once, registers infrastructure, then `start()`s inside the host lifecycle, and `shutdown(wait=True)` joins the host drain.
- `export_jobs`/`import_jobs` is the store-to-store migration seam; a populated persistent store is never re-seeded by re-running registration.
- A job body returns a `Result`; the listener maps `EVENT_JOB_ERROR` to a receipt fault, capturing the executor's exception.

[RAIL_LAW]:
- Package: `APScheduler`
- Owns: in-process scheduled execution across the trigger vocabulary, the `Job` lifecycle, pluggable persistent stores and executors, the listener event bus, and job import/export
- Accept: `AsyncIOScheduler`/`BackgroundScheduler` non-blocking use, `add_job`/`scheduled_job` registration, `CronTrigger.from_crontab` for external strings, `configure` before `start`, persistent jobstores for durable schedules, `add_listener` bitmask observability, `export_jobs`/`import_jobs` migration
- Reject: hand-rolled `threading.Timer`/`asyncio.call_later` loops, `aiocron`, `BlockingScheduler` in responsive code, re-registration against a populated store, `<undefined>` treated as `None`, inline return inspection over the event bus
