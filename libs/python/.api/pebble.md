# [PY_BRANCH_API_PEBBLE]

`pebble` owns the branch's terminal worker fabric: `ProcessPool`/`ThreadPool` schedule callables onto recyclable workers, and `schedule(timeout=)` enforces a wall-clock deadline by KILLING the worker mid-execution and reclaiming its slot — the one interruption `anyio.move_on_after` cooperative cancellation and `anyio.to_process` cannot express against a native C loop. Each process worker carries a `max_tasks` recycle bound that caps RSS growth, `ProcessFuture.cancel()` terminates a RUNNING task, and `ProcessExpired` (an `OSError` subclass) carries the dead worker's `exitcode`/`pid` as worker-death evidence. `schedule` and `submit` are two spellings over one kill mechanism — `schedule` passes `args`/`kwargs` as collections, `submit` flattens them positionally in the stdlib `Executor` shape. Decorator entrypoints in `pebble.concurrent` and `pebble.asynchronous` bridge one call to a fresh worker returning a `concurrent.futures.Future` or an `asyncio.Future`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pebble`
- package: `pebble`
- version: `5.2.0`
- license: LGPL-3.0
- import: `pebble`
- owner: `runtime`
- rail: worker fabric
- namespaces: `pebble`, `pebble.concurrent`, `pebble.asynchronous`
- capability: process/thread worker pools with per-task wall-clock kill-on-deadline, `max_tasks` worker recycling, running-task cancellation, `OSError`-typed worker-death evidence, stdlib-`Executor`-shaped `submit` beside collection-shaped `schedule`, input-order chunked `map`, one-call process/thread decorators over both `concurrent.futures` and `asyncio` futures, and signal-handler/synchronization/wait primitives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pools, futures, faults
- rail: worker fabric
- `ProcessPool` and `ThreadPool` share the `schedule`/`submit`/`map`/`close`/`stop`/`join`/`active` lifecycle, splitting on one axis: a process worker is killable, so `ProcessPool.schedule` carries `timeout=` and the pool kills-then-respawns on deadline or death; a thread worker is not killable, so `ThreadPool.schedule` carries no `timeout` and its `map(timeout=)` only bounds the wait while the thread runs on. `ProcessFuture` extends `PebbleFuture` over the stdlib future with running-task cancellation. `ProcessExpired` subclasses `OSError`; total pool loss raises the stdlib `concurrent.futures.process.BrokenProcessPool` pebble surfaces, not a pebble-owned type.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                                          |
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
- rail: worker fabric
- Construct with a worker cap and an optional `max_tasks` recycle bound that retires a worker after that many tasks and spawns a replacement, capping RSS growth; `initializer`/`initargs` run once per worker at spawn, and `ProcessPool` alone accepts a `context` selecting the multiprocessing start method (`spawn`/`fork`/`forkserver`). `close` drains intake, `stop` cancels pending and kills workers, `join` awaits shutdown, `active` reports whether the pool runs. Both pools are context managers.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `ProcessPool(max_workers=16, max_tasks=0, initializer=None, initargs=(), context=...)` | build          | construct a process pool     |
|  [02]   | `ThreadPool(max_workers=16, max_tasks=0, initializer=None, initargs=())`               | build          | construct a thread pool      |
|  [03]   | `close()`                                                                              | lifecycle      | stop intake, drain the queue |
|  [04]   | `stop()`                                                                               | lifecycle      | cancel pending, kill workers |
|  [05]   | `join(timeout=None)`                                                                   | lifecycle      | block until workers exit     |
|  [06]   | `active`                                                                               | property       | `bool`, pool still running   |

[ENTRYPOINT_SCOPE]: task submission
- rail: worker fabric
- `ProcessPool.schedule(function, args=, kwargs=, timeout=)` returns a `ProcessFuture`; a nonzero `timeout` is the wall-clock deadline the pool enforces by killing the worker and raising the builtin `TimeoutError` on `.result()`. `submit` is the stdlib-`Executor`-shaped twin — `ProcessPool.submit(function, timeout, /, *args, **kwargs)` flattens arguments positionally after a required `timeout` slot; `ThreadPool.submit(function, *args, **kwargs)` matches the stdlib signature with no kill. `map` absorbs `timeout=`/`chunksize=` as kwargs (the source signature is `map(function, *iterables, **kwargs)`), breaks each of `*iterables` into `chunksize`-sized chunks, applies `timeout` to EACH chunk, and returns a map future whose `.result()` yields an input-order ITERATOR; a per-chunk timeout or a task raise re-raises at that element's position while iteration continues past it.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `ProcessPool.schedule(function, args=(), kwargs={}, timeout=None)` | submit         | schedule one task with a kill deadline           |
|  [02]   | `ProcessPool.submit(function, timeout, /, *args, **kwargs)`        | submit         | stdlib-shaped submit; kill deadline, flat args   |
|  [03]   | `ProcessPool.map(function, *iterables, **kwargs)`                  | submit map     | chunked map; `timeout=` kwarg applies per chunk  |
|  [04]   | `ThreadPool.schedule(function, args=(), kwargs={})`                | submit         | schedule one task, no worker kill                |
|  [05]   | `ThreadPool.submit(function, *args, **kwargs)`                     | submit         | stdlib-shaped submit, no worker kill             |
|  [06]   | `ThreadPool.map(function, *iterables, **kwargs)`                   | submit map     | chunked map; `timeout=` bounds the wait only     |
|  [07]   | `ProcessFuture.cancel()`                                           | cancel         | `bool`; terminates a running task, unlike stdlib |
|  [08]   | `ProcessExpired.exitcode` / `ProcessExpired.pid`                   | evidence       | dead-worker exit code and process id             |

[ENTRYPOINT_SCOPE]: decorators and primitives
- rail: worker fabric
- `concurrent.process`/`concurrent.thread` decorate a callable so each call runs on a fresh worker returning a `concurrent.futures.Future`; the `asynchronous` twins return an `asyncio.Future` awaitable inside a running loop. Only the process decorators carry `timeout=` and `context=` — kill and start-method are process-only. A `pool=` argument reuses a standing pool instead of spawning per call, and `daemon=True` sets the underlying worker daemon flag. `sighandler` installs a function as the handler for given signals; `synchronized` serializes a function against concurrent callers; `waitforthreads`/`waitforqueues` block until any listed thread exits or queue is ready.

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------------------ | :------------- | :--------------------------- |
|  [01]   | `concurrent.process(timeout=None, name=None, daemon=True, context=None, pool=None)`   | decorator      | process worker; `Future`     |
|  [02]   | `concurrent.thread(name=None, daemon=True, pool=None)`                                | decorator      | thread worker; `Future`      |
|  [03]   | `asynchronous.process(timeout=None, name=None, daemon=True, context=None, pool=None)` | decorator      | process worker; awaitable    |
|  [04]   | `asynchronous.thread(name=None, daemon=True, pool=None)`                              | decorator      | thread worker; awaitable     |
|  [05]   | `sighandler(signals)`                                                                 | signal         | install a signal handler     |
|  [06]   | `synchronized`                                                                        | lock           | serialize concurrent callers |
|  [07]   | `waitforthreads(threads, timeout=None)`                                               | wait           | await any thread exit        |
|  [08]   | `waitforqueues(queues, timeout=None)`                                                 | wait           | await any queue ready        |

## [04]-[IMPLEMENTATION_LAW]

[PEBBLE_TOPOLOGY]:
- terminal-deadline law: `schedule(timeout=)` is a wall-clock KILL, not a cooperative signal — expiry terminates the worker process mid-execution and raises the builtin `TimeoutError` on `.result()`, so a task stuck in a native C loop, a tight compute spin, or a blocking syscall still yields the slot; live probe confirms the killed worker's pid is replaced by a fresh worker on the next schedule.
- submission-spelling law: `schedule` and `submit` drive one kill mechanism through two argument shapes — `schedule(function, args=(), kwargs={}, timeout=None)` keeps arguments as explicit collections, `submit(function, timeout, /, *args, **kwargs)` flattens them positionally for a stdlib `Executor` call site; the branch's pool driver picks `schedule` because it packs the one sealed payload as `args=(blob,)`.
- worker-recycle law: `max_tasks=0` runs a worker forever; a nonzero bound retires the worker after that many tasks and spawns a replacement, so a leaking or fragmenting native extension has its RSS reclaimed on a fixed cadence rather than growing unbounded.
- running-cancel law: `ProcessFuture.cancel()` returns `True` and terminates a RUNNING process worker — stdlib `Future.cancel()` returns `False` once a task starts; only a FINISHED future refuses cancellation.
- worker-death law: an abnormal worker exit (segfault, `os._exit`, OOM kill) surfaces as `ProcessExpired` on `.result()` carrying the worker's `exitcode` and `pid`; total worker loss surfaces as the stdlib `concurrent.futures.process.BrokenProcessPool` pebble raises, never a pebble-minted type.
- pickle-payload law: pebble serializes every payload through the `multiprocessing` ForkingPickler (STDLIB pickle), not `cloudpickle`, so the dispatched callable must resolve by qualified name and every argument must be stdlib-picklable — a live lambda handed straight to `schedule` raises `PicklingError`.
- shipped-gate law: closures never reach pebble live — the branch dispatches the module-level `shipped` gate (reference-picklable) plus a frozen `msgspec` `Kernel` Struct (stdlib-picklable) whose `payload` field carries `cloudpickle.dumps(fn)` bytes, so a lambda crosses pebble's stdlib seam as inert bytes the worker-floor `shipped` rehydrates through `cloudpickle.loads`.
- thread-asymmetry law: a thread worker is never killed — `ThreadPool.schedule` carries no `timeout`, and `ThreadPool.map(timeout=)` only bounds the caller's wait while the thread runs to completion; deadline enforcement that must reclaim a slot routes to `ProcessPool`.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): `WorkerPool` binds pebble to the one `(WorkerKind.PROCESS, Enforcement.TERMINAL)` arm key — `pebble.ProcessPool(max_workers=loky.cpu_count(only_physical_cores=True), max_tasks=64, initializer=fidelity, context=get_context("spawn"))` — and `submit` drives `self._pebble.schedule(sealed_kernel, args=(cloudpickle.dumps((carrier, kernel, args)),), timeout=kernel.deadline.to_optional())` — one sealed bytes payload, so a closure-bearing argument survives the stdlib pickler; `drain()` calls `close()`+`join()`, `retire()` calls `stop()`+`join()` and drops the arm memo. Worker death is typed at this boundary: `ProcessExpired(pid, exitcode)` or `TimeoutError` converts onto the fault rail, so pebble owns the terminal-kill arm and the pool owns the executor lifecycle.
- `execution/lanes`(`runtime/.planning/execution/lanes.md`): `LanePolicy.offload` routes `Enforcement.TERMINAL` through `WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.TERMINAL)` then `pool.submit(replace(kernel, deadline=budget), *args)`, where `budget = _tighter(self.deadline, kernel.deadline)` folds the lane budget and the per-offload deadline to whichever is sooner — the exact `float` pebble's `schedule(timeout=)` enforces. `move_on_after` owns the COOPERATIVE arms; pebble owns the TERMINAL band, the only bound a hung native kernel obeys.
- `loky`(`.api/loky.md`): the two process arms split on kill semantics — loky's `get_reusable_executor` owns `(PROCESS, COOPERATIVE)` (warm process-global reuse, crash-respawn, idle reap, no per-task terminate — teardown is whole-pool `kill_workers`), pebble owns `(PROCESS, TERMINAL)` (a per-task wall-clock KILL loky cannot express). A cooperative deadline where warm reuse pays routes to loky; a hard kill of a native-bound leg routes to pebble; both cloudpickle-ship and both fold into the `RetryClass.WORKER` respawn band.
- `cloudpickle`(`.api/cloudpickle.md`): the stdlib-pickle seam's escape hatch — `Kernel.of` fills the Struct `payload` via `cloudpickle.dumps(fn)` on a pickle-seam kind whose callable a by-name walk loses (a `<lambda>`/`<locals>` qualname, a bound method), and the worker-floor `shipped` gate calls `cloudpickle.loads(kernel.payload)` far-side; pebble transports one cloudpickle-sealed blob, so even a `LIVE`-slotted kernel a `TERMINAL` re-route sends its way crosses the ForkingPickler as inert bytes rather than degrading to a thread.
- `tblib`(`.api/tblib.md`): `pickling_support.install()` runs as pebble's `initializer=fidelity` per worker and parent-side before the first hop, so a worker that RAISES re-crosses pebble's pickle seam with its frames intact and `BoundaryFault.of` classifies the true worker cause; a worker the deadline KILLS leaves no traceback, so `ProcessExpired`'s `exitcode`/`pid` are the death evidence tblib cannot supply.
- `anyio`(`.api/anyio.md`): pebble's `schedule`/`submit` return a `concurrent.futures`-shaped `ProcessFuture` whose blocking `.result()` bridges off the loop through `anyio.to_thread.run_sync(settled, abandon_on_cancel=True, limiter=WORKER_BAND)` — pebble owns the killable worker, anyio owns the non-blocking await. loky's warm pool carries the COOPERATIVE `PROCESS` arm and cannot interrupt a native loop — the exact gap pebble fills.
- `psutil`(`.api/psutil.md`): `Supervisor._probe` reads worker RSS via `psutil.Process().oneshot()` over `children(recursive=True)` and `memory_info().rss` against the `SupervisionPolicy.rss_ceiling`, rolling the pebble arm on a `DEGRADED` breach and retiring-then-respawning on `DEAD`; `max_tasks=64` recycling and the psutil ceiling probe are the two RSS-bounding arms — pebble reclaims on a fixed task cadence, psutil on a live ceiling breach.
- `reliability/faults`(`runtime/.planning/reliability/faults.md`): `ProcessExpired` subclasses `OSError` and lands the `CLASSIFY` `resource` row, folding to the `BoundaryFault` `resource` case; pebble's `TimeoutError` lands the earlier `deadline` row (row order load-bearing — `TimeoutError` subclasses `OSError`, so `deadline` precedes `resource` or the fold coalesces a kill into `resource`), so a kill-on-deadline and a worker-death classify as distinct faults through one table.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): `ProcessExpired`/`BrokenProcessPool`/`TerminatedWorkerError` are name-matched (`_named(...)`) in the `RetryClass.WORKER` `Policy` row (`attempts=3`, `timeout=120.0`, `wait_initial=0.5`) — the pool-executor worker-death band, import-free at the BASE tier — so a died-worker leg respawns and retries inside one span; this row is distinct from `RetryClass.OCCT`, which targets the `anyio.BrokenWorkerProcess`/`BrokenWorkerInterpreter` pair.

[LOCAL_ADMISSION]:
- pebble is the branch's one worker-pool terminal-kill owner; a second pool abstraction, a hand-rolled `multiprocessing.Pool` wrapper, or a per-task `Process` spawn is the deleted form, and the warm-reuse `(PROCESS, COOPERATIVE)` half stays loky's.
- a compute leg needing a hard wall-clock deadline over a native or blocking body routes to `ProcessPool.schedule(timeout=)`; a leg needing only cooperative in-loop cancellation stays with `anyio` and never spawns a killable process.
- a standing `ProcessPool` is acquired once per arm key through `WorkerPool.acquire` and passed to the `concurrent`/`asynchronous` decorators via `pool=`; a per-call fresh spawn is reserved for one-shot legs where pool amortization does not pay.
- worker payloads reach pebble as the module-level `shipped` gate plus a stdlib-picklable `Kernel`; a closure or lambda crosses only as `cloudpickle` bytes on the `Kernel.payload`, never handed live to `schedule`/`submit`.

[RAIL_LAW]:
- Package: `pebble`
- Owns: process/thread worker pools, per-task wall-clock kill-on-deadline, `max_tasks` worker recycling, running-task cancellation, `OSError`-typed worker-death evidence, stdlib-shaped `submit` beside collection-shaped `schedule`, input-order chunked `map`, and the one-call process/thread decorators over both future kinds
- Accept: `ProcessPool.schedule(timeout=)` for a hard-deadline native leg, `submit` for a stdlib-`Executor` call site, `max_tasks` for RSS-bounded recycling, `ProcessFuture.cancel()` for running-task termination, `ProcessExpired` as worker-death evidence, and `pool=` reuse from a standing pool
- Reject: pebble for a cooperative in-loop deadline `anyio` owns, the warm-reuse process arm `loky` owns, a live closure/lambda handed to the stdlib pickle seam, a `ThreadPool` timeout expecting a worker kill, a second pool owner, and a hand-rolled `multiprocessing` wrapper
