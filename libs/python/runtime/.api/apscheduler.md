# [PY_RUNTIME_API_APSCHEDULER]

`APScheduler` supplies in-process job scheduling via three scheduler classes (`BackgroundScheduler`, `AsyncIOScheduler`, `BlockingScheduler`), six trigger types (cron, interval, date, calendar-interval, and combining triggers), a `Job` object with per-job lifecycle controls, pluggable job stores and executors, and a listener event bus for scheduler and job lifecycle events.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `APScheduler`
- package: `APScheduler`
- module: `apscheduler`
- asset: runtime library
- rail: scheduling
- namespaces: `apscheduler.schedulers`, `apscheduler.triggers`, `apscheduler.jobstores`, `apscheduler.executors`, `apscheduler.events`, `apscheduler.job`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scheduler family
- rail: scheduling

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------------ | :------------ | :----------------------------- |
|  [01]   | `schedulers.background.BackgroundScheduler` | scheduler     | daemon thread, non-blocking    |
|  [02]   | `schedulers.asyncio.AsyncIOScheduler`       | scheduler     | asyncio event loop integration |
|  [03]   | `schedulers.blocking.BlockingScheduler`     | scheduler     | blocks calling thread          |

[PUBLIC_TYPE_SCOPE]: trigger family
- rail: scheduling

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [RAIL]                       |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------- |
|  [01]   | `triggers.cron.CronTrigger`                         | cron trigger      | field-based cron schedule    |
|  [02]   | `triggers.interval.IntervalTrigger`                 | interval trigger  | fixed time interval          |
|  [03]   | `triggers.date.DateTrigger`                         | date trigger      | single one-time fire         |
|  [04]   | `triggers.calendarinterval.CalendarIntervalTrigger` | calendar trigger  | calendar-unit interval       |
|  [05]   | `triggers.combining.AndTrigger`                     | combining trigger | fire when all triggers agree |
|  [06]   | `triggers.combining.OrTrigger`                      | combining trigger | fire when any trigger fires  |

[PUBLIC_TYPE_SCOPE]: store and executor family
- rail: scheduling

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------------- | :------------ | :---------------------------- |
|  [01]   | `jobstores.memory.MemoryJobStore`    | job store     | in-process volatile job store |
|  [02]   | `jobstores.base.BaseJobStore`        | abstract base | job store contract            |
|  [03]   | `executors.pool.ThreadPoolExecutor`  | executor      | thread pool job execution     |
|  [04]   | `executors.pool.ProcessPoolExecutor` | executor      | process pool job execution    |
|  [05]   | `executors.asyncio.AsyncIOExecutor`  | executor      | asyncio coroutine execution   |

[PUBLIC_TYPE_SCOPE]: job and event family
- rail: scheduling

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------- | :------------ | :--------------------------------- |
|  [01]   | `job.Job`                   | job object    | live job handle with lifecycle ops |
|  [02]   | `events.SchedulerEvent`     | event         | scheduler lifecycle event          |
|  [03]   | `events.JobEvent`           | event         | job added/removed/modified event   |
|  [04]   | `events.JobExecutionEvent`  | event         | job executed/errored event         |
|  [05]   | `events.JobSubmissionEvent` | event         | job submitted event                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: job management
- rail: scheduling

| [INDEX] | [SURFACE]                                                                                                                                                          | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `add_job(func, trigger, args, kwargs, id, name, misfire_grace_time, coalesce, max_instances, next_run_time, jobstore, executor, replace_existing, **trigger_args)` | add job        | register callable as a job         |
|  [02]   | `scheduled_job(trigger, args, kwargs, id, name, ...)`                                                                                                              | decorator      | decorate function as scheduled job |
|  [03]   | `modify_job(job_id, jobstore, **changes)`                                                                                                                          | job modify     | change job attributes in place     |
|  [04]   | `remove_job(job_id, jobstore)`                                                                                                                                     | job remove     | remove one job                     |
|  [05]   | `remove_all_jobs(jobstore)`                                                                                                                                        | job remove     | remove all jobs in store           |
|  [06]   | `get_job(job_id, jobstore) -> Job\|None`                                                                                                                           | job query      | retrieve one Job by id             |
|  [07]   | `get_jobs(jobstore, pending) -> list[Job]`                                                                                                                         | job query      | list jobs in store                 |

[ENTRYPOINT_SCOPE]: scheduler lifecycle
- rail: scheduling

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :----------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `start(paused=False)`          | lifecycle      | start scheduler                                                       |
|  [02]   | `shutdown(wait=True)`          | lifecycle      | stop scheduler, optionally drain                                      |
|  [03]   | `pause()`                      | lifecycle      | pause all job execution                                               |
|  [04]   | `resume()`                     | lifecycle      | resume from paused state                                              |
|  [05]   | `pause_job(job_id, jobstore)`  | lifecycle      | pause one job                                                         |
|  [06]   | `resume_job(job_id, jobstore)` | lifecycle      | resume one job                                                        |
|  [07]   | `state`                        | property       | current state: `STATE_STOPPED`=0, `STATE_RUNNING`=1, `STATE_PAUSED`=2 |

[ENTRYPOINT_SCOPE]: infrastructure management
- rail: scheduling

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RAIL]                            |
| :-----: | :--------------------------------------- | :-------------- | :-------------------------------- |
|  [01]   | `add_jobstore(jobstore, alias, **opts)`  | store add       | register a job store              |
|  [02]   | `remove_jobstore(alias, shutdown=True)`  | store remove    | deregister a job store            |
|  [03]   | `add_executor(executor, alias, **opts)`  | executor add    | register an executor              |
|  [04]   | `remove_executor(alias, shutdown=True)`  | executor remove | deregister an executor            |
|  [05]   | `add_listener(callback, mask=EVENT_ALL)` | listener        | subscribe to scheduler/job events |
|  [06]   | `remove_listener(callback)`              | listener        | unsubscribe a listener            |

[ENTRYPOINT_SCOPE]: trigger constructors
- rail: scheduling

| [INDEX] | [SURFACE]                                                                                                           | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------ |
|  [01]   | `CronTrigger(year, month, day, week, day_of_week, hour, minute, second, start_date, end_date, timezone, jitter)`    | cron           | field-based cron schedule       |
|  [02]   | `CronTrigger.from_crontab(expr, timezone=None)`                                                                     | class method   | construct from standard crontab |
|  [03]   | `IntervalTrigger(weeks, days, hours, minutes, seconds, start_date, end_date, timezone, jitter)`                     | interval       | fixed interval trigger          |
|  [04]   | `DateTrigger(run_date, timezone)`                                                                                   | date trigger   | single one-time fire            |
|  [05]   | `CalendarIntervalTrigger(years, months, weeks, days, hour, minute, second, start_date, end_date, timezone, jitter)` | calendar       | calendar-unit trigger           |
|  [06]   | `AndTrigger(triggers, jitter)`                                                                                      | combining      | all-triggers-must-agree gate    |
|  [07]   | `OrTrigger(triggers, jitter)`                                                                                       | combining      | first-trigger-fires gate        |

[ENTRYPOINT_SCOPE]: Job object operations
- rail: scheduling

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `job.modify(**changes)`                                  | job modify     | change attributes of running job |
|  [02]   | `job.reschedule(trigger)`                                | job reschedule | swap trigger on running job      |
|  [03]   | `job.pause()`                                            | lifecycle      | pause one job                    |
|  [04]   | `job.resume()`                                           | lifecycle      | resume one paused job            |
|  [05]   | `job.remove()`                                           | lifecycle      | remove job from scheduler        |
|  [06]   | `job.id`, `job.name`, `job.func`, `job.trigger`          | properties     | job identity fields              |
|  [07]   | `job.next_run_time`, `job.coalesce`, `job.max_instances` | properties     | scheduling parameters            |

[ENTRYPOINT_SCOPE]: event codes
- rail: scheduling

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------- | :------------- | :------------------------------- |
|  [01]   | `EVENT_SCHEDULER_STARTED`  | event code     | scheduler started                |
|  [02]   | `EVENT_SCHEDULER_SHUTDOWN` | event code     | scheduler stopped                |
|  [03]   | `EVENT_SCHEDULER_PAUSED`   | event code     | scheduler paused                 |
|  [04]   | `EVENT_SCHEDULER_RESUMED`  | event code     | scheduler resumed                |
|  [05]   | `EVENT_JOB_ADDED`          | event code     | job registered                   |
|  [06]   | `EVENT_JOB_REMOVED`        | event code     | job removed                      |
|  [07]   | `EVENT_JOB_EXECUTED`       | event code     | job ran successfully             |
|  [08]   | `EVENT_JOB_ERROR`          | event code     | job raised exception             |
|  [09]   | `EVENT_JOB_MISSED`         | event code     | job missed its fire time         |
|  [10]   | `EVENT_JOB_SUBMITTED`      | event code     | job submitted to executor        |
|  [11]   | `EVENT_JOB_MAX_INSTANCES`  | event code     | max concurrent instances reached |
|  [12]   | `EVENT_ALL`                | event code     | bitmask for all events           |

## [04]-[IMPLEMENTATION_LAW]

[SCHEDULER_TOPOLOGY]:
- `BackgroundScheduler` runs in a daemon thread; `AsyncIOScheduler` integrates with an existing event loop; `BlockingScheduler` blocks `start()`
- `add_job` returns a `Job` object; `scheduled_job` is the decorator alias with identical parameters
- job store `'default'` and executor `'default'` are used when no alias is given
- `MemoryJobStore` is the default; volatile across restarts; persistent stores require extra packages
- `STATE_STOPPED=0`, `STATE_RUNNING=1`, `STATE_PAUSED=2` are integer constants on `apscheduler.schedulers.base`
- `JobExecutionEvent.retval` carries the return value on success; `.exception` and `.traceback` carry failure detail
- `misfire_grace_time` is `None` by default, meaning missed firings run immediately; set to seconds integer to gate

[LOCAL_ADMISSION]:
- one scheduler instance owns all jobs; job stores and executors are added before `start()`
- `add_listener` with a specific event bitmask routes events without polling; `EVENT_ALL` is the default mask
- `CronTrigger.from_crontab` is the intake path for external cron strings; field-by-field construction is for programmatic schedules
- `jitter` on triggers adds randomized seconds delay to prevent thundering herd; it is an integer second cap

[RAIL_LAW]:
- Package: `APScheduler`
- Owns: in-process scheduled job execution with cron, interval, and date-based triggers
- Accept: `BackgroundScheduler` or `AsyncIOScheduler` for non-blocking use; `add_job`/`scheduled_job` for job registration; `add_listener` for event observability
- Reject: hand-rolled `threading.Timer` or `asyncio.call_later` loops for recurring schedules; `BlockingScheduler` in service code that must stay responsive
