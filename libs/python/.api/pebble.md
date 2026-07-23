# [PY_BRANCH_API_PEBBLE]

`pebble` owns the terminal-kill worker fabric: `ProcessPool`/`ThreadPool` schedule callables onto recyclable workers, and `ProcessPool.schedule(timeout=)` enforces a wall-clock deadline by KILLING the worker mid-execution — the interruption cooperative `anyio` cancellation cannot express against a native C loop or a blocking syscall. `schedule` and `submit` are two argument spellings over that one kill mechanism; the `concurrent`/`asynchronous` decorators bridge a single call to a fresh worker. It feeds the `WorkerPool` TERMINAL-`PROCESS` arm.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pebble`
- package: `pebble` (LGPL-3.0)
- import: `pebble`
- owner: `runtime`
- rail: worker fabric
- namespaces: `pebble`, `pebble.concurrent`, `pebble.asynchronous`
- capability: process/thread worker pools with per-task wall-clock kill-on-deadline, `max_tasks` worker recycling, running-task cancellation, `OSError`-typed worker-death evidence, stdlib-`Executor`-shaped `submit` beside collection-shaped `schedule`, input-order chunked `map`, one-call process/thread decorators over both `concurrent.futures` and `asyncio` futures, and signal-handler/synchronization/wait primitives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pools, futures, faults

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                    |
| :-----: | :----------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `ProcessPool`      | pool            | process worker pool; kill-on-deadline, `max_tasks` recycling    |
|  [02]   | `ThreadPool`       | pool            | thread worker pool; cooperative only, no worker kill            |
|  [03]   | `ProcessFuture`    | future          | future whose `cancel()` terminates a RUNNING task               |
|  [04]   | `ProcessMapFuture` | future          | `ProcessPool.map` result over per-chunk process futures         |
|  [05]   | `MapFuture`        | future          | `ThreadPool.map` result over per-chunk thread futures           |
|  [06]   | `ProcessExpired`   | fault (OSError) | dead-worker evidence carrying `exitcode`/`pid`                  |
|  [07]   | `CONSTS`           | record          | `Consts(sleep_unit, term_timeout, channel_lock_timeout)` tuning |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool construction and lifecycle

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ProcessPool(max_workers=16, max_tasks=0, initializer=None, initargs=(), context=...)` | ctor     | construct a process pool     |
|  [02]   | `ThreadPool(max_workers=16, max_tasks=0, initializer=None, initargs=())`               | ctor     | construct a thread pool      |
|  [03]   | `close()`                                                                              | instance | stop intake, drain the queue |
|  [04]   | `stop()`                                                                               | instance | cancel pending, kill workers |
|  [05]   | `join(timeout=None)`                                                                   | instance | block until workers exit     |
|  [06]   | `active`                                                                               | property | `bool`, pool still running   |

[ENTRYPOINT_SCOPE]: task submission

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `ProcessPool.schedule(function, args=(), kwargs={}, timeout=None)` | instance | schedule one task with a kill deadline           |
|  [02]   | `ProcessPool.submit(function, timeout, /, *args, **kwargs)`        | instance | stdlib-shaped submit; kill deadline, flat args   |
|  [03]   | `ProcessPool.map(function, *iterables, **kwargs)`                  | instance | chunked map; `timeout=` kwarg applies per chunk  |
|  [04]   | `ThreadPool.schedule(function, args=(), kwargs={})`                | instance | schedule one task, no worker kill                |
|  [05]   | `ThreadPool.submit(function, *args, **kwargs)`                     | instance | stdlib-shaped submit, no worker kill             |
|  [06]   | `ThreadPool.map(function, *iterables, **kwargs)`                   | instance | chunked map; `timeout=` bounds the wait only     |
|  [07]   | `ProcessFuture.cancel()`                                           | instance | `bool`; terminates a running task, unlike stdlib |
|  [08]   | `ProcessExpired.exitcode` / `ProcessExpired.pid`                   | property | dead-worker exit code and process id             |

- `ProcessPool.map`/`ThreadPool.map`: `.result()` yields an input-order iterator; a per-chunk `timeout` or a task raise re-raises at that element's position and iteration continues past it.

[ENTRYPOINT_SCOPE]: decorators and primitives

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :--------------------------- |
|  [01]   | `concurrent.process(timeout=None, name=None, daemon=True, context=None, pool=None)`   | factory | process worker; `Future`     |
|  [02]   | `concurrent.thread(name=None, daemon=True, pool=None)`                                | factory | thread worker; `Future`      |
|  [03]   | `asynchronous.process(timeout=None, name=None, daemon=True, context=None, pool=None)` | factory | process worker; awaitable    |
|  [04]   | `asynchronous.thread(name=None, daemon=True, pool=None)`                              | factory | thread worker; awaitable     |
|  [05]   | `sighandler(signals)`                                                                 | factory | install a signal handler     |
|  [06]   | `synchronized`                                                                        | factory | serialize concurrent callers |
|  [07]   | `waitforthreads(threads, timeout=None)`                                               | static  | await any thread exit        |
|  [08]   | `waitforqueues(queues, timeout=None)`                                                 | static  | await any queue ready        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- terminal-deadline: `schedule(timeout=)` is a wall-clock KILL, not a cooperative signal — expiry terminates the worker process mid-execution and raises the builtin `TimeoutError` on `.result()`, so a task stuck in a native C loop, a compute spin, or a blocking syscall still yields its slot and the pool respawns a fresh worker.
- submission-spelling: `schedule` and `submit` drive one kill mechanism through two argument shapes — `schedule(function, args=(), kwargs={}, timeout=None)` keeps arguments as explicit collections, `submit(function, timeout, /, *args, **kwargs)` flattens them positionally for a stdlib `Executor` call site.
- worker-recycle: `max_tasks=0` runs a worker forever; a nonzero bound retires the worker after that many tasks and spawns a replacement, reclaiming a leaking or fragmenting native extension's RSS on a fixed cadence.
- running-cancel: `ProcessFuture.cancel()` returns `True` and terminates a RUNNING process worker, where stdlib `Future.cancel()` returns `False` once a task starts; only a FINISHED future refuses cancellation.
- worker-death: an abnormal worker exit (segfault, `os._exit`, OOM kill) surfaces as `ProcessExpired` on `.result()` carrying the worker's `exitcode`/`pid`; total worker loss surfaces as the stdlib `concurrent.futures.process.BrokenProcessPool`, never a pebble-minted type.
- pickle-payload: pebble serializes every payload through the `multiprocessing` ForkingPickler (stdlib pickle), not `cloudpickle`, so the dispatched callable resolves by qualified name and every argument is stdlib-picklable — a live lambda handed to `schedule` raises `PicklingError`.
- thread-asymmetry: a thread worker is never killed — `ThreadPool.schedule` carries no `timeout` and `ThreadPool.map(timeout=)` only bounds the caller's wait while the thread runs to completion; deadline enforcement that must reclaim a slot routes to `ProcessPool`.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): the PRIMARY consumer. `WorkerPool` binds pebble to the `(WorkerKind.PROCESS | WorkerKind.GPU, Enforcement.TERMINAL)` arm as `pebble.ProcessPool(max_workers=loky.cpu_count(only_physical_cores=True), max_tasks=64, initializer=fidelity, context=get_context("spawn"))` — the GPU arm swaps `initializer=partial(_device_latch, device)` — and `submit` mints the future through `self._pebble.schedule(sealed_kernel, args=(cloudpickle.dumps((carrier, kernel, args)),), timeout=kernel.deadline.to_optional())`, one sealed bytes payload so a closure-bearing argument survives the stdlib pickler; a cooperative cancel escalates through `ProcessFuture.cancel()` to reclaim the killable slot, `drain()` calls `close()`+`join()`, `retire()` calls `stop()`+`join()`. `ProcessExpired(pid, exitcode)`/`TimeoutError` convert onto the fault rail here — pebble owns the terminal-kill arm, the pool owns the executor lifecycle.
- `execution/lanes`(`runtime/.planning/execution/lanes.md`): `LanePolicy.offload` routes `Enforcement.TERMINAL` through `WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.TERMINAL)` then `pool.submit(replace(kernel, deadline=budget), *args)`, where `budget = _tighter(self.deadline, kernel.deadline)` folds lane budget and per-offload deadline to whichever is sooner — the exact `float` `schedule(timeout=)` enforces. `move_on_after` owns the COOPERATIVE arms; pebble owns the TERMINAL band, the only bound a hung native kernel obeys.
- `loky`(`.api/loky.md`): the two process arms split on kill semantics — loky owns `(PROCESS, COOPERATIVE)` (warm reuse, crash-respawn, idle reap, whole-pool `kill_workers` teardown), pebble owns `(PROCESS, TERMINAL)` (a per-task wall-clock KILL loky cannot express). A cooperative deadline where warm reuse pays routes to loky, a hard kill of a native leg to pebble; both cloudpickle-ship and both fold into the `RetryClass.WORKER` respawn band.
- `cloudpickle`(`.api/cloudpickle.md`): the stdlib-pickle seam's escape hatch — `Kernel.of` fills the Struct `payload` via `cloudpickle.dumps(fn)` when a by-name walk loses the callable (a `<lambda>`/`<locals>` qualname, a bound method), and the worker-floor `shipped` gate calls `cloudpickle.loads(kernel.payload)` far-side; pebble transports one cloudpickle-sealed blob, so even a `LIVE`-slotted kernel a `TERMINAL` re-route sends its way crosses the ForkingPickler as inert bytes rather than degrading to a thread.
- `tblib`(`.api/tblib.md`): `pickling_support.install()` runs as pebble's `initializer=fidelity` per worker and parent-side before the first hop, so a worker that RAISES re-crosses the pickle seam with frames intact and `BoundaryFault.of` classifies the true cause; a worker the deadline KILLS leaves no traceback, so `ProcessExpired`'s `exitcode`/`pid` are the death evidence tblib cannot supply.
- `anyio`(`.api/anyio.md`): pebble's `schedule`/`submit` return a `concurrent.futures`-shaped `ProcessFuture` whose blocking `.result()` bridges off the loop through `anyio.to_thread.run_sync(settled, abandon_on_cancel=True, limiter=WORKER_BAND)` — pebble owns the killable worker, anyio owns the non-blocking await against the native loop loky's warm pool cannot interrupt.
- `psutil`(`.api/psutil.md`): `Supervisor._probe` reads worker RSS via `psutil.Process().oneshot()` over `children(recursive=True)` and `memory_info().rss` against `SupervisionPolicy.rss_ceiling`, rolling the pebble arm on a `DEGRADED` breach and retiring-then-respawning on `DEAD`; `max_tasks=64` recycling and the psutil ceiling probe are the two RSS-bounding arms — pebble reclaims on a fixed task cadence, psutil on a live ceiling breach.
- `reliability/faults`(`runtime/.planning/reliability/faults.md`): `ProcessExpired` subclasses `OSError` and lands the `CLASSIFY` `resource` row folding to the `BoundaryFault` `resource` case; pebble's `TimeoutError` lands the earlier `deadline` row (order load-bearing — `TimeoutError` also subclasses `OSError`, so `deadline` precedes `resource` or the fold coalesces a kill into `resource`), so a kill-on-deadline and a worker-death classify as distinct faults through one table.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): `ProcessExpired`/`BrokenProcessPool`/`TerminatedWorkerError` match by module-qualified spelling (`_transient("pebble.common.types.ProcessExpired", ...)`) in the `RetryClass.WORKER` `Policy` row (`attempts=3`, `timeout=120.0`, `wait_initial=0.5`), import-free at the BASE tier, so a died-worker leg respawns and retries inside one span; distinct from `RetryClass.OCCT`, which targets the `anyio.BrokenWorkerProcess`/`BrokenWorkerInterpreter` pair.

[LOCAL_ADMISSION]:
- pebble is the branch's sole terminal-kill worker-pool owner; the warm-reuse `(PROCESS, COOPERATIVE)` half is loky's.
- a compute leg needing a hard wall-clock deadline over a native or blocking body routes to `ProcessPool.schedule(timeout=)`; a leg needing only cooperative in-loop cancellation stays with `anyio`.
- a standing `ProcessPool` is acquired once per arm key through `WorkerPool.acquire` and passed to the `concurrent`/`asynchronous` decorators via `pool=`; a per-call fresh spawn is reserved for a one-shot leg where pool amortization does not pay.
- worker payloads reach pebble as the module-level `shipped` gate carrying a stdlib-picklable `Kernel`; a closure crosses only as `cloudpickle` bytes on `Kernel.payload`, never live to `schedule`/`submit`.

[RAIL_LAW]:
- Package: `pebble`
- Owns: process/thread worker pools, per-task wall-clock kill-on-deadline, `max_tasks` recycling, running-task cancellation, `OSError`-typed worker-death evidence, stdlib-shaped `submit` beside collection-shaped `schedule`, input-order chunked `map`, and the one-call process/thread decorators over both future kinds
- Accept: `ProcessPool.schedule(timeout=)` for a hard-deadline native leg, `submit` for a stdlib-`Executor` call site, `max_tasks` for RSS-bounded recycling, `ProcessFuture.cancel()` for running-task termination, `ProcessExpired` as worker-death evidence, `pool=` reuse from a standing pool
- Reject: pebble for a cooperative in-loop deadline `anyio` owns, the warm-reuse process arm `loky` owns, a live closure handed to the stdlib pickle seam, a `ThreadPool` timeout expecting a worker kill, a second pool owner, and a hand-rolled `multiprocessing` wrapper
