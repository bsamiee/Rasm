# [CONCURRENCY]

Covers async Python under anyio, from awaited steps and task shapes to deadlines, shielded cleanup, and worker threads, interpreters, and processes.

## [01]-[TASKS]

Synchronous steps chain on an awaited result through `bind` and `map`, and `chained` is the one bind across an `await`: it awaits its step on success and rewraps a failure under the step's success type. `Step` types every injected coroutine function, and `Fault`, `Outcome`, `Reason`, `paired`, and `accumulated` are the package faults:

```python
type Step[T, R] = Callable[[T], Coroutine[object, object, Outcome[R]]]


async def chained[T, R](step: Step[T, R], result: Outcome[T], /) -> Outcome[R]:
    match result.tag:
        case "ok":
            return await step(result.ok)
        case "error":
            return Error(result.error)


def named(folder: anyio.Path, raw: bytes, /) -> Outcome[anyio.Path]:
    return catch(bytes.decode, exception=UnicodeDecodeError)(raw).map(lambda text: folder / text.strip()).map_error(partial(Fault.of, Reason.INVALID, repr(raw)))


async def followed(read: Step[anyio.Path, bytes], index: anyio.Path, /) -> Outcome[bytes]:
    return await chained(read, (await read(index)).bind(partial(named, index.parent)))
```

`followed` reads an index file, decodes a file name from it, and reads the named file, and a failed read or decode returns its faults with no later read. Independent awaits start together, and the anyio member follows the shape of the work:
- `paired(*await anyio.gather(read(left), read(right)))` types a fixed arity as a tuple and keeps the faults of every failed read
- `accumulated(await anyio.amap(read, paths))` runs one coroutine function over a batch and keeps every fault in input order
- `anyio.as_completed(*coroutines)` yields task handles in completion order, and leaving its block cancels every unfinished task
- `await group.start(<target>)` returns once its task calls `task_status.started(<value>)`, with that value
- `create_memory_object_stream[T](<size>)` joins producer and consumer tasks with backpressure, and the receiver's `async for` ends once every send clone closes
- Runners typed over `*Ts` (`anyio.run`, `start_soon`, `run_sync`) take the target and its arguments
- `@anyio.functools.lru_cache(ttl=<seconds>)` runs one call per key while concurrent callers wait, and caches an `Error` as it caches `Ok`
- Hosts select a backend at `anyio.run(<entry>, *<arguments>, backend=<backend>)`, and code calling anyio members alone runs under asyncio and trio

## [02]-[DEADLINES]

Each item holds one limiter token for its work, and a deadline inside `async with limiter` times the work alone where one outside it counts the wait for a token. `move_on_after` ends its block at the deadline, and execution continues past it to the expiry fault:

```python
async def bounded(read: Step[anyio.Path, bytes], limiter: anyio.CapacityLimiter, limit: timedelta, path: anyio.Path, /) -> Outcome[bytes]:
    async with limiter:
        with anyio.move_on_after(limit / timedelta(seconds=1)):
            return await read(path)
    return Error(Fault.of(Reason.EXPIRED, str(path), limit))
```

`await anyio.amap(partial(bounded, read, limiter, limit), paths)` returns each expired item's fault beside its siblings' bytes, and `Reason.EXPIRED = "{subject} exceeded {cause}"` joins the package reason enum:
- `amap` starts every item's task at once, and the limiter bounds how many pass `async with limiter`
- Callers own a deadline around a call, and a callee takes a limit where its deadline sits inside a scope it opens
- Deadlines fire at a checkpoint, a block with no `await` inside runs to its end, and ruff `ASYNC100` reports the block
- Limiters bind to the first event loop entering them, and each run's entry builds one per shared resource for every batch over it
- `anyio.Path(<path>, limiter=<limiter>)` and `open_file(..., limiter=<limiter>)` run under that limiter, and paths derived through `/` keep it

## [03]-[CANCELLATION]

Cancel scopes cancel at every checkpoint inside them until they exit, and an `await` in a `finally` under a cancelled scope raises at its first checkpoint, before its cleanup runs. `claimed` removes its marker under a shielded scope its `limit` ends:

```python
@asynccontextmanager
async def claimed(limit: timedelta, marker: anyio.Path, /) -> AsyncGenerator[anyio.Path]:
    await marker.touch(exist_ok=False)
    try:
        yield marker
    finally:
        with anyio.move_on_after(limit / timedelta(seconds=1), shield=True):
            await marker.unlink(missing_ok=True)
```

Cancelling a task group with a task inside `claimed` removes its marker before `async with` on the group returns:
- Acquisition sits before `try`, and a `FileExistsError` from `touch` reaches the caller with nothing to release
- Shields block outer cancellation alone, and the shielded deadline ends a release that hangs
- Handlers acting on cancellation catch `anyio.get_cancelled_exc_class()` and re-raise it
- `group.cancel(<reason>)` cancels every task, and `group.cancel_scope.cancelled_caught` reports a group that cancellation ended
- Cancellation stays level-triggered on asyncio under anyio, every later checkpoint in a cancelled scope raises again

## [04]-[WORKERS]

Workers run synchronous code off the event loop through one call shape, `await <runner>.run_sync(<function>, *<arguments>, limiter=<limiter>)`:

| [INDEX] | [WORK]                                                    | [RUNNER]         | [CANCELLATION]                                       |
| :-----: | :-------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | Blocking call, native code releasing the GIL              | `to_thread`      | Waits, `abandon_on_cancel=True` discards the outcome |
|  [02]   | CPU work holding the GIL                                  | `to_interpreter` | Waits for the worker                                 |
|  [03]   | Extension without subinterpreter support or able to crash | `to_process`     | Waits, `cancellable=True` kills the process          |

Workers run a whole synchronous pipeline in one hop and return its `Result` to the loop as a value. Interpreter workers import the target's module, msgspec refuses to load in a subinterpreter, and a module holding an interpreter target imports the package faults `lazy`:

```python
lazy from .faults import Fault, Outcome, Reason


@catch(exception=SyntaxError)
def imported(source: str, /) -> frozenset[str]:
    return frozenset(alias.name for node in ast.walk(ast.parse(source)) if isinstance(node, ast.Import) for alias in node.names)


async def modules(name: str, source: str, /) -> Outcome[frozenset[str]]:
    return (await to_interpreter.run_sync(imported, source)).map_error(partial(Fault.of, Reason.INVALID, name))
```

`modules` parses in a subinterpreter and builds its faults in the parent:
- Interpreter and process workers pickle the callable, its arguments, and its return value, and a raised exception pickles back and raises at the `await`
- `@catch` on a module-level `def` pickles under that function's own name, with a `Result` holding a caught exception intact
- One raising call runs its `catch` inside the thread (`await to_thread.run_sync(catch(zlib.decompress, exception=zlib.error), data)`)
- Default limiters hold 40 thread tokens every `anyio.Path` and `open_file` call shares, and `os.cpu_count()` interpreter or process tokens
- Thread workers calling `from_thread.check_cancelled()` between steps raise a pending cancellation at that call
