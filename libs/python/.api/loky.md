# [PY_BRANCH_API_LOKY]

`loky` mints the process-global reusable process pool: one warm worker fabric shared across every call site, dynamically resized on re-acquisition and respawned whole when a worker dies, reached through the `get_reusable_executor` singleton factory over the `ProcessPoolExecutor` class it wraps. Crash resilience is the defining trait — a SIGKILLed worker surfaces its pending future as `TerminatedWorkerError` and the broken pool is discarded for a fresh instance on the next acquisition — and cloudpickle is the default payload codec, so lambdas, closures, and locally-defined kernels cross the process boundary that stdlib `ProcessPoolExecutor` rejects. `execution/workers` binds it as the `WorkerPool` COOPERATIVE-`PROCESS` arm: `get_reusable_executor` warms one pool the `Supervisor` probes and the `reliability/resilience` `WORKER` retry row respawns, sized by `cpu_count(only_physical_cores=True)`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `loky`
- package: `loky`
- version: `3.5.6`
- license: BSD-3-Clause
- import: `loky`
- owner: `runtime`
- rail: worker fabric
- namespaces: `loky`, `loky.process_executor`, `loky.reusable_executor`, `loky.backend`, `loky.cloudpickle_wrapper`, `loky.initializers`
- capability: process-global crash-respawning reusable process pool with cloudpickle payloads, dynamic resize, idle-worker reaping, per-worker `initializer`/`env` warm state, and host-aware CPU sizing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: executor classes
- rail: worker fabric
- `ProcessPoolExecutor` extends the stdlib `concurrent.futures.ProcessPoolExecutor` contract with `job_reducers`/`result_reducers` pickling hooks, an `env` per-worker override, the `loky` spawn context, cloudpickle payloads, and a crash-detecting worker-management thread. `Executor` re-exports the stdlib abstract base unchanged; `Future` is loky's own subclass carrying the same public API.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                                         |
| :-----: | :-------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `ProcessPoolExecutor` | executor class | crash-resilient process pool over cloudpickle payloads         |
|  [02]   | `Executor`            | abstract base  | stdlib `concurrent.futures.Executor`, re-exported unchanged    |
|  [03]   | `Future`              | future class   | loky-owned `Future`, stdlib-compatible result/exception handle |

[PUBLIC_TYPE_SCOPE]: fault types
- rail: worker fabric
- `BrokenProcessPool` is the sole top-level re-export; `TerminatedWorkerError`, `ShutdownExecutorError`, and `LokyRecursionError` import from `loky.process_executor`. `TerminatedWorkerError` subclasses `BrokenProcessPool`, which subclasses the stdlib `concurrent.futures.process.BrokenProcessPool`, so a stdlib-shaped `except BrokenProcessPool` catches every worker-death raise. `TimeoutError` re-exports the builtin; `CancelledError` re-exports `concurrent.futures.CancelledError`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                                            |
| :-----: | :---------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `BrokenProcessPool`     | pool fault     | top-level; pool unusable after a worker death, subclasses stdlib  |
|  [02]   | `TerminatedWorkerError` | worker fault   | `process_executor`; a worker exited abruptly (crash/SIGKILL/OOM)  |
|  [03]   | `ShutdownExecutorError` | lifecycle rail | `process_executor`; submit after shutdown was requested           |
|  [04]   | `LokyRecursionError`    | nesting fault  | `process_executor`; worker spawns a pool past `LOKY_MAX_DEPTH`=10 |
|  [05]   | `TimeoutError`          | timeout rail   | builtin re-export; `result`/`wait` deadline hit                   |
|  [06]   | `CancelledError`        | cancel rail    | `concurrent.futures` re-export; future cancelled before run       |

[PUBLIC_TYPE_SCOPE]: wait discriminants
- rail: worker fabric

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------- | :------------- | :------------------------------------------- |
|  [01]   | `FIRST_COMPLETED` | `str` sentinel | `wait` returns on the first future to finish |
|  [02]   | `FIRST_EXCEPTION` | `str` sentinel | `wait` returns on the first future to raise  |
|  [03]   | `ALL_COMPLETED`   | `str` sentinel | `wait` returns once every future is done     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: executor acquisition and lifecycle
- rail: worker fabric
- `get_reusable_executor(max_workers=None, context=None, timeout=10, kill_workers=False, reuse='auto', job_reducers=None, result_reducers=None, initializer=None, initargs=(), env=None)` returns the process-global singleton, starting one when absent or when the prior instance broke, and resizing in place when `max_workers` differs. `ProcessPoolExecutor(max_workers=None, job_reducers=None, result_reducers=None, timeout=None, context=None, initializer=None, initargs=(), env=None)` is the wrapped standalone class. `submit` and `map` accept cloudpickle-serializable callables and arguments; `map` forwards `timeout`/`chunksize` as keywords. `shutdown(kill_workers=True)` and `get_reusable_executor(kill_workers=True)` are the forcible-teardown path — no `terminate_workers` method exists.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get_reusable_executor(max_workers, ...)` | build          | acquire/resize/respawn the singleton pool |
|  [02]   | `ProcessPoolExecutor(max_workers, ...)`   | build          | construct a standalone pool               |
|  [03]   | `submit(fn, *args, **kwargs) -> Future`   | dispatch       | queue one call, return its future         |
|  [04]   | `map(fn, *iterables, timeout, chunksize)` | dispatch       | lazy parallel map over the pool           |
|  [05]   | `shutdown(wait=True, kill_workers=False)` | teardown       | drain or forcibly kill workers            |

[ENTRYPOINT_SCOPE]: payload codec and pickler
- rail: worker fabric
- cloudpickle is the default `loky_pickler`, so closures, lambdas, and locally-defined functions ship without a module-level definition. `wrap_non_picklable_objects` cloudpickle-wraps one object crossing an otherwise-pickle-hostile boundary; `set_loky_pickler` swaps the process-wide serializer (`None` restores cloudpickle).

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `wrap_non_picklable_objects(obj, keep_wrapper=True)` | payload wrap   | cloudpickle-wrap one object for the pool               |
|  [02]   | `set_loky_pickler(loky_pickler=None)`                | pickler swap   | set the process-wide serializer (`None` = cloudpickle) |
|  [03]   | `cloudpickle_wrapper.dumps` / `loads`                | codec          | direct cloudpickle round-trip helpers                  |

[ENTRYPOINT_SCOPE]: host sizing and worker introspection
- rail: worker fabric
- `cpu_count` is the schedulable-core source: it folds `os.cpu_count`, process affinity (`sched_getaffinity`), the Linux cgroup CPU-bandwidth limit (`cpu.max`/`cfs_quota`), and `LOKY_MAX_CPU_COUNT` as `max(min(constraints), 1)`; `only_physical_cores=True` drops SMT siblings and needs `psutil` to read the physical count. `get_exitcodes_terminated_worker(processes)` imports from `loky.process_executor` and reads the exit codes off a terminated pool's processes for crash forensics; `ProcessPoolExecutor._processes` is the `{pid: BaseProcess}` introspection map — the abstract `Executor` base carries no such attribute.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `cpu_count(only_physical_cores=False)`                 | host sizing    | schedulable CPU count under affinity/cgroup/env caps |
|  [02]   | `process_executor.get_exitcodes_terminated_worker(p)`  | forensics      | exit codes of a terminated pool's workers            |
|  [03]   | `ProcessPoolExecutor._processes -> dict[int, Process]` | introspection  | live `{pid: worker}` map for external introspection  |

[ENTRYPOINT_SCOPE]: futures utilities
- rail: worker fabric
- `as_completed` and `wait` re-export the stdlib helpers unchanged, so a loky `Future` and a stdlib future compose in one wait set.

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `as_completed(fs, timeout=None)`                      | future join    | yield futures as each finishes               |
|  [02]   | `wait(fs, timeout=None, return_when='ALL_COMPLETED')` | future join    | block on a set to a return-when discriminant |

## [04]-[IMPLEMENTATION_LAW]

[POOL_TOPOLOGY]:
- `get_reusable_executor` owns one process-global singleton: repeated acquisition from any call site returns the same warm pool, sparing the worker-spawn and import cost each stdlib `ProcessPoolExecutor()` pays. Re-acquisition with a different `max_workers` resizes the running pool in place and returns the same executor object; a healthy pool is never torn down to change its width.
- `reuse='auto'` reuses a healthy singleton and forces a fresh one only when the prior instance broke or its constructor arguments changed; `reuse=True` reuses unconditionally, `reuse=False` always replaces.
- `timeout` (default 10s) reaps idle workers to release memory and file descriptors; a later `submit` respawns cold workers back to `max_workers`. Sizing `timeout` near 100 times the worker warm-up cost keeps the respawn overhead negligible.
- `kill_workers=True` on `get_reusable_executor` or `shutdown` forcibly interrupts in-flight jobs to swap the pool under new constructor arguments; teardown is this flag, never a per-worker terminate method.
- `context` fixes the multiprocessing start method for the pool's whole lifetime; the `env` per-worker override applies before any module loads and binds only under the `loky` context.

[CRASH_RESILIENCE]:
- A worker that dies mid-task — segfault, SIGKILL, OOM kill, `os._exit` — surfaces its pending future as `TerminatedWorkerError`, and every other pending future on that pool raises `BrokenProcessPool`; the pool is marked broken and accepts no further work.
- `get_reusable_executor` after a break returns a new executor object with fully respawned worker pids, so the caller recovers by re-acquiring rather than by resetting the dead instance; the broken pool is discarded, never repaired.
- `TerminatedWorkerError` subclasses `BrokenProcessPool`, so a single `except BrokenProcessPool` covers both the abrupt-exit and the poisoned-pool raise; `get_exitcodes_terminated_worker` reads the worker exit codes for the crash cause.

[PAYLOAD_LAW]:
- cloudpickle is the default serializer, so a task closing over local state, a lambda, or a function defined inside another function crosses the process boundary that stdlib `pickle` rejects with a lookup error. `wrap_non_picklable_objects` is the escape for one object that resists even cloudpickle at the argument boundary.
- `job_reducers`/`result_reducers` register per-type `copyreg`-style reducers to customize task and result pickling — a large read-only argument reduces to a shared-memory handle instead of a full copy per worker.
- `initializer`/`initargs` run once in each freshly spawned worker before its first task, the warm-state hook for a per-worker import, native handle, or cache; the same run re-fires whenever a reaped worker respawns.

[ENV_KNOBS]:
- `LOKY_PICKLER` selects the process-wide serializer by name (default `cloudpickle`), the env twin of `set_loky_pickler`; `get_loky_pickler_name()` reads the live choice. Pinning a name-only stdlib `pickle` drops the closure payloads.
- `LOKY_MAX_CPU_COUNT` caps `cpu_count` below the host's schedulable count, the deploy-time override the container orchestrator sets.
- `LOKY_MAX_DEPTH` (default 10) bounds nested-pool spawning — a worker at that depth acquiring a further pool raises `LokyRecursionError`; the `fork` start method forbids any nesting past depth 0.
- `context` accepts a `START_METHODS` name (`loky`, `loky_init_main`, `spawn`, `fork`, `forkserver`) or a context object and fixes the start method for the pool's whole lifetime.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): the PRIMARY owner. `WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.COOPERATIVE)` memoizes one `get_reusable_executor(max_workers=cpu_count(only_physical_cores=True), timeout=120, reuse="auto", initializer=fidelity)` per arm key in `WorkerPool._live`; `submit` drives `self._loky.submit(shipped, kernel, *args)`; `drain` calls `shutdown(wait=True)`, `retire` calls `shutdown(wait=False, kill_workers=True)`. Its `KIND_POLICY` `PROCESS` row binds `restart=RetryClass.WORKER`, and a break respawns on the next `acquire`. loky never enters through `execution/lanes.offload` — that hop's COOPERATIVE-`PROCESS` arm is `anyio.to_process.run_sync`, and the lanes TERMINAL arm routes to the pebble pool; both fabrics share only the schedulable-CPU sizing discipline.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): `RetryClass.WORKER` = `Policy(attempts=3, timeout=120.0, target=_named("TerminatedWorkerError", "BrokenProcessPool", "ProcessExpired"), wait_initial=0.5)` is the pool worker-death band — name-matched at the BASE tier with no loky import, so the whole-pool respawn `TerminatedWorkerError` implies rides a wide `wait_initial`. `guard(cls)` owns the one span; narrowing the target below the `TerminatedWorkerError`/`BrokenProcessPool` pair is a cross-folder break.
- `cloudpickle`(`.api/cloudpickle.md`): cloudpickle owns the reduce path, loky owns pool reuse. cloudpickle vendors inside `loky.backend.reduction` as the default `loky_pickler` (`get_loky_pickler_name()` resolves `cloudpickle`), but the estate consumes the FIRST-CLASS `cloudpickle` package directly — `Kernel.of` calls `cloudpickle.dumps(fn)` for the `VALUE` payload and the worker-floor `shipped` gate calls `cloudpickle.loads`, so a closure crosses loky's warm pool with no per-call opt-in.
- `tblib`(`.api/tblib.md`): tblib owns fault fidelity, loky owns the crossing. `WorkerPool.warm` primes each worker by folding the `fidelity` kernel first, which runs `pickling_support.install()` as loky's `initializer`, so a `TerminatedWorkerError` or an ordinary worker raise re-crosses the pickle seam with its worker frames intact for `BoundaryFault.of` to classify the true cause.
- `pebble`(`.api/pebble.md`): division of labor across one `WorkerKind.PROCESS`, discriminated by `Enforcement`. loky owns the COOPERATIVE warm-reuse arm — idle reap on `timeout`, crash respawn, cloudpickle payloads; pebble owns the TERMINAL arm — `ProcessPool.schedule(timeout=)` wall-clock kill and `max_tasks` recycling. loky's `TerminatedWorkerError`/`BrokenProcessPool` and pebble's `ProcessExpired` co-target the one `RetryClass.WORKER` band.
- `anyio`(`.api/anyio.md`): `anyio.to_thread.run_sync(future.result)` inside `async_boundary` bridges loky's blocking `concurrent.futures.Future` to the event loop, so the pool future settles on a band thread and never blocks the loop. This is the thread bridge, not `to_process` — loky's own IPC hop is internal to the pool.
- `psutil`(`.api/psutil.md`): `cpu_count(only_physical_cores=True)` reads the physical-core count through psutil to size the loky pool. `Supervisor` reads pool worker RSS via `psutil.Process().children(recursive=True)` under one `oneshot()` against the `SupervisionPolicy` ceiling and takes pool liveness from `WorkerPool._live` presence — not a `_processes` scan; `_processes` stays the direct per-pid introspection handle for a bespoke read.

[LOCAL_ADMISSION]:
- `WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.COOPERATIVE)` is the acquisition path — it wraps `get_reusable_executor` behind the memoized arm; a bare `ProcessPoolExecutor()` or a raw `get_reusable_executor` per task is admitted only for a one-shot pool whose warm reuse carries no value.
- `max_workers` defaults to `cpu_count()`; the pool pins it to `cpu_count(only_physical_cores=True)` once so concurrent process crossings never oversubscribe the host against each package's internal thread pool.
- Wrap `submit`/`result` in the resilience retry so a `TerminatedWorkerError` respawns a fresh pool rather than propagating a worker crash as a hard failure; a pure transform never rides the pool.
- Fix `context` and `initializer`/`env` per owner identity so worker warm state is reproducible; mixing start methods or env overrides across acquisitions of the same singleton is rejected.

[RAIL_LAW]:
- Package: `loky`
- Owns: the process-global reusable crash-respawning process pool, cloudpickle task payloads, dynamic pool resize, idle-worker reaping, per-worker `initializer`/`env` warm state, and host-aware CPU sizing
- Accept: `get_reusable_executor` for the singleton fabric, `submit`/`map` over cloudpickle payloads, `shutdown(kill_workers=)` teardown, `cpu_count` for host sizing, `_processes` for per-pid introspection, `wrap_non_picklable_objects` for a resistant argument
- Reject: a per-task `ProcessPoolExecutor` where the warm singleton serves, a local catch that swallows `TerminatedWorkerError`/`BrokenProcessPool` instead of the resilience retry, a hand-rolled multiprocessing pool where loky owns crash respawn, oversubscribing the host past the worker band, pinning a non-cloudpickle `loky_pickler` that drops closure payloads
