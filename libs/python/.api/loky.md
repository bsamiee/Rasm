# [PY_BRANCH_API_LOKY]

`loky` mints the process-global reusable pool: one warm fabric resized on re-acquisition, respawned whole on worker death, reached through `get_reusable_executor`. Worker death surfaces its pending future as `TerminatedWorkerError` and swaps the broken pool for a fresh instance; cloudpickle payloads carry closures across the boundary stdlib `ProcessPoolExecutor` rejects. It feeds the `WorkerPool` COOPERATIVE-`PROCESS` arm.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `loky`
- package: `loky` (BSD-3-Clause)
- import: `loky`
- owner: `runtime`
- rail: worker fabric
- namespaces: `loky`, `loky.process_executor`, `loky.reusable_executor`, `loky.backend`, `loky.cloudpickle_wrapper`, `loky.initializers`
- capability: process-global crash-respawning reusable process pool with cloudpickle payloads, dynamic resize, idle-worker reaping, per-worker `initializer`/`env` warm state, and host-aware CPU sizing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: executor classes

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                                         |
| :-----: | :-------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `ProcessPoolExecutor` | executor class | crash-resilient process pool over cloudpickle payloads         |
|  [02]   | `Executor`            | abstract base  | stdlib `concurrent.futures.Executor`, re-exported unchanged    |
|  [03]   | `Future`              | future class   | loky-owned `Future`, stdlib-compatible result/exception handle |

[PUBLIC_TYPE_SCOPE]: fault types

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                                            |
| :-----: | :---------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `BrokenProcessPool`     | pool fault     | top-level; pool unusable after a worker death, subclasses stdlib  |
|  [02]   | `TerminatedWorkerError` | worker fault   | `process_executor`; a worker exited abruptly (crash/SIGKILL/OOM)  |
|  [03]   | `ShutdownExecutorError` | lifecycle rail | `process_executor`; submit after shutdown was requested           |
|  [04]   | `LokyRecursionError`    | nesting fault  | `process_executor`; worker spawns a pool past `LOKY_MAX_DEPTH`=10 |
|  [05]   | `TimeoutError`          | timeout rail   | builtin re-export; `result`/`wait` deadline hit                   |
|  [06]   | `CancelledError`        | cancel rail    | `concurrent.futures` re-export; future cancelled before run       |

[PUBLIC_TYPE_SCOPE]: wait discriminants

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------- | :------------- | :------------------------------------------- |
|  [01]   | `FIRST_COMPLETED` | `str` sentinel | `wait` returns on the first future to finish |
|  [02]   | `FIRST_EXCEPTION` | `str` sentinel | `wait` returns on the first future to raise  |
|  [03]   | `ALL_COMPLETED`   | `str` sentinel | `wait` returns once every future is done     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: executor acquisition and lifecycle

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get_reusable_executor(max_workers, ...)` | build          | acquire/resize/respawn the singleton pool |
|  [02]   | `ProcessPoolExecutor(max_workers, ...)`   | build          | construct a standalone pool               |
|  [03]   | `submit(fn, *args, **kwargs) -> Future`   | dispatch       | queue one call, return its future         |
|  [04]   | `map(fn, *iterables, timeout, chunksize)` | dispatch       | lazy parallel map over the pool           |
|  [05]   | `shutdown(wait=True, kill_workers=False)` | teardown       | drain or forcibly kill workers            |

[ENTRYPOINT_SCOPE]: payload codec and pickler

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `wrap_non_picklable_objects(obj, keep_wrapper=True)` | payload wrap   | cloudpickle-wrap one object for the pool               |
|  [02]   | `set_loky_pickler(loky_pickler=None)`                | pickler swap   | set the process-wide serializer (`None` = cloudpickle) |
|  [03]   | `cloudpickle_wrapper.dumps` / `loads`                | codec          | direct cloudpickle round-trip helpers                  |

[ENTRYPOINT_SCOPE]: host sizing and worker introspection

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `cpu_count(only_physical_cores=False)`                 | host sizing    | schedulable CPU count under affinity/cgroup/env caps |
|  [02]   | `process_executor.get_exitcodes_terminated_worker(p)`  | forensics      | exit codes of a terminated pool's workers            |
|  [03]   | `ProcessPoolExecutor._processes -> dict[int, Process]` | introspection  | live `{pid: worker}` map for external introspection  |

[ENTRYPOINT_SCOPE]: futures utilities

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `as_completed(fs, timeout=None)`                      | future join    | yield futures as each finishes               |
|  [02]   | `wait(fs, timeout=None, return_when='ALL_COMPLETED')` | future join    | block on a set to a return-when discriminant |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- singleton-reuse: `get_reusable_executor` owns one process-global warm pool; every acquisition returns the same instance, sparing the spawn and import cost each `ProcessPoolExecutor()` pays. A differing `max_workers` resizes in place and returns the same object; a healthy pool is never torn down to change width.
- reuse-policy: `reuse='auto'` reuses a healthy singleton and forces a fresh one only when the prior broke or its constructor arguments changed; `reuse=True` reuses unconditionally, `reuse=False` always replaces.
- idle-reap: `timeout` (default 10s) reaps idle workers to release memory and file descriptors; a later `submit` respawns cold workers to `max_workers`.
- teardown: `kill_workers=True` on `get_reusable_executor` or `shutdown` is the sole forcible teardown, interrupting in-flight jobs to swap the pool under new constructor arguments; loky carries no per-worker terminate method.
- start-method: `context` fixes the multiprocessing start method for the pool's lifetime and accepts a `START_METHODS` name (`loky`, `loky_init_main`, `spawn`, `fork`, `forkserver`) or a context object; the `env` per-worker override applies before any module loads and binds only under the `loky` context.
- crash-respawn: a worker dying mid-task (segfault, SIGKILL, OOM, `os._exit`) surfaces its pending future as `TerminatedWorkerError` and raises `BrokenProcessPool` on every other pending future; the pool is marked broken and accepts no further work. Re-acquisition returns a fresh respawned pool, so the caller recovers by re-acquiring, never by resetting the dead instance.
- fault-catch: `TerminatedWorkerError` subclasses `BrokenProcessPool` (in turn the stdlib `concurrent.futures.process.BrokenProcessPool`), so one `except BrokenProcessPool` covers both the abrupt-exit and poisoned-pool raise; `get_exitcodes_terminated_worker` reads the worker exit codes for the crash cause.
- payload: cloudpickle is the default serializer, so a closure, lambda, or nested function crosses the boundary stdlib `pickle` rejects with a lookup error; `wrap_non_picklable_objects` escapes one argument that resists even cloudpickle.
- reducers: `job_reducers`/`result_reducers` register per-type `copyreg`-style reducers customizing task and result pickling — a large read-only argument reduces to a shared-memory handle instead of a per-worker copy.
- warm-state: `initializer`/`initargs` run once in each freshly spawned worker before its first task and re-fire on every respawn — the per-worker import, native handle, or cache hook.
- host-sizing: `cpu_count` folds `os.cpu_count`, process affinity (`sched_getaffinity`), the Linux cgroup CPU-bandwidth limit (`cpu.max`/`cfs_quota`), and `LOKY_MAX_CPU_COUNT` as `max(min(caps), 1)`; `only_physical_cores=True` drops SMT siblings and needs `psutil`.
- env-knobs: `LOKY_PICKLER` name-selects the process-wide serializer (default `cloudpickle`, the env twin of `set_loky_pickler`; `loky.backend.reduction.get_loky_pickler_name()` reads the live choice), and a name-only stdlib `pickle` drops closure payloads. `LOKY_MAX_CPU_COUNT` caps `cpu_count` below the host count. `LOKY_MAX_DEPTH` (default 10) bounds nested-pool spawning, a worker past it raising `LokyRecursionError`; the `fork` start method forbids nesting past depth 0.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): the PRIMARY owner. `WorkerPool` holds acquisition arguments and re-resolves `get_reusable_executor(max_workers=cpu_count(only_physical_cores=True), context="loky", timeout=120, reuse="auto", initializer=fidelity)` per `submit` on the settle thread, so a broken singleton is replaced under the in-band `WORKER` retry instead of a dead instance re-raising; `drain` resolves-then-`shutdown(wait=True)`, `retire` resolves-then-`shutdown(wait=False, kill_workers=True)`, `roll` is one `kill_workers=True` re-acquisition swapping the singleton in place. Its `KIND_POLICY` `PROCESS` row binds `restart=RetryClass.WORKER`; loky reaches `execution/lanes.offload` only through `WorkerPool`, while the lanes TERMINAL arm routes to the pebble pool and both arms share the schedulable-CPU sizing discipline.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): `RetryClass.WORKER` = `Policy(attempts=3, timeout=120.0, target=_transient("loky.process_executor.TerminatedWorkerError", "concurrent.futures.process.BrokenProcessPool", "pebble.common.types.ProcessExpired"), wait_initial=0.5)` is the pool worker-death band, matched by module-qualified spelling at the BASE tier with no loky import; `guard(cls)` owns the one span, and narrowing the target below the `TerminatedWorkerError`/`BrokenProcessPool` pair is a cross-folder break.
- `cloudpickle`(`.api/cloudpickle.md`): cloudpickle owns the reduce path, loky owns pool reuse. cloudpickle vendors inside `loky.backend.reduction` as the default `loky_pickler`, but the estate consumes the first-class `cloudpickle` package directly — `Kernel.of` calls `cloudpickle.dumps(fn)` for the `VALUE` payload and the worker-floor `shipped` gate calls `cloudpickle.loads`, so a closure crosses loky's warm pool with no per-call opt-in.
- `tblib`(`.api/tblib.md`): tblib owns fault fidelity, loky owns the crossing. `WorkerPool.warm` folds the `fidelity` kernel first, running `pickling_support.install()` as loky's `initializer`, so a `TerminatedWorkerError` or ordinary worker raise re-crosses the pickle seam with its worker frames intact for `BoundaryFault.of` to classify the true cause.
- `pebble`(`.api/pebble.md`): one `WorkerKind.PROCESS` split by `Enforcement`. loky owns the COOPERATIVE warm-reuse arm — idle reap on `timeout`, crash respawn, cloudpickle payloads; pebble owns the TERMINAL arm — `ProcessPool.schedule(timeout=)` wall-clock kill and `max_tasks` recycling. loky's `TerminatedWorkerError`/`BrokenProcessPool` and pebble's `ProcessExpired` co-target the one `RetryClass.WORKER` band.
- `anyio`(`.api/anyio.md`): `anyio.to_thread.run_sync(settled, abandon_on_cancel=True, limiter=WORKER_BAND)` inside `async_boundary` bridges loky's blocking `concurrent.futures.Future` to the event loop — the band token bounds submission and settle in one acquisition, and a cooperative cancel abandons the settle to the pool's reaper. This is the thread bridge, not `to_process`; loky's own IPC hop is internal to the pool.
- `psutil`(`.api/psutil.md`): `cpu_count(only_physical_cores=True)` reads the physical-core count through psutil to size the pool. `Supervisor` reads pool worker RSS via `psutil.Process().children(recursive=True)` under one `oneshot()` against the `SupervisionPolicy` ceiling, scoped through `WorkerPool.pids()` — the cooperative arm's `_processes` `{pid: worker}` map — while liveness reads `WorkerPool._live` and `alive()`.

[LOCAL_ADMISSION]:
- `WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.COOPERATIVE)` is the acquisition path, wrapping `get_reusable_executor` behind the memoized arm; a standalone `ProcessPoolExecutor` is admitted for the `WorkerKind.GPU` device arm, whose per-device `env=` the process-global singleton cannot hold, and otherwise only for a one-shot pool whose warm reuse carries no value.
- `max_workers` pins to `cpu_count(only_physical_cores=True)` once so concurrent process crossings never oversubscribe the host against each package's internal thread pool.
- `submit`/`result` ride the resilience retry so a `TerminatedWorkerError` respawns a fresh pool rather than propagating a worker crash; a pure transform never rides the pool.
- `context` and `initializer`/`env` fix per owner identity so worker warm state is reproducible; mixing start methods or env overrides across acquisitions of the same singleton is rejected.

[RAIL_LAW]:
- Package: `loky`
- Owns: the process-global reusable crash-respawning process pool, cloudpickle task payloads, dynamic pool resize, idle-worker reaping, per-worker `initializer`/`env` warm state, and host-aware CPU sizing
- Accept: `get_reusable_executor` for the singleton fabric, `submit`/`map` over cloudpickle payloads, `shutdown(kill_workers=)` teardown, `cpu_count` for host sizing, `_processes` for per-pid introspection, `wrap_non_picklable_objects` for a resistant argument
- Reject: a per-task `ProcessPoolExecutor` where the warm singleton serves, a local catch that swallows `TerminatedWorkerError`/`BrokenProcessPool` instead of the resilience retry, a hand-rolled multiprocessing pool where loky owns crash respawn, oversubscribing the host past the worker band, pinning a non-cloudpickle `loky_pickler` that drops closure payloads
