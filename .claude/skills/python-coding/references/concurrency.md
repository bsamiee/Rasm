# [CONCURRENCY]

Covers async Python under anyio.

## [01]-[TASKS]

Synchronous steps chain on an awaited result through `bind` and `map`, and `chained` is the one bind across an `await`: it awaits its step on success and rewraps a failure under the step's success type:

```python
async def chained[T, R](step: Callable[[T], Awaitable[Outcome[R]]], result: Outcome[T], /) -> Outcome[R]:
    match result:
        case Result(tag="ok", ok=value):
            return await step(value)
        case Result(error=faults):
            return Error(faults)


def named(folder: anyio.Path, raw: bytes, /) -> Outcome[anyio.Path]:
    return catch(bytes.decode, exception=UnicodeDecodeError)(raw).map(lambda text: folder / text.strip()).map_error(partial(Fault.of, Reason.INVALID, repr(raw)))


async def followed(index: anyio.Path, /) -> Outcome[bytes]:
    return await chained(loaded, (await loaded(index)).bind(partial(named, index.parent)))
```

`followed` reads an index file, decodes a file name from it, and reads the named file, and a failed read or decode returns its faults with no later read. `await anyio.functools.reduce(lambda held, item: chained(partial(<step>, item), held), items, initial=Result[<state>, Faults].Ok(<seed>))` folds state through dependent awaited steps and awaits no step after the first fault. Independent awaits start together, and the anyio member follows the shape of the work:
- `anyio.gather` returns a typed tuple, and `paired(*await anyio.gather(loaded(left), loaded(right)))` keeps the faults of every failed read
- `accumulated(await anyio.amap(loaded, paths))` combines a batch into one outcome holding every fault in input order
- `async with anyio.as_completed(*coroutines) as done` yields task handles in completion order, `await <handle>` reads each value, and leaving the block cancels every unfinished task
- `create_memory_object_stream[T](<size>)` joins producer and consumer tasks with backpressure, the receiver's `async for` ending once every send clone closes
- `anyio.itertools` members (`batched(<receiver>, <n>)`, `pairwise`, `groupby`) and `anyio.functools.reduce` read a receiver as their iterable
- `@anyio.functools.lru_cache(ttl=<seconds>)` runs one call per key while concurrent callers wait, and caches an `Error` as it caches `Ok`

## [02]-[DEADLINES]

Each item holds one limiter token for its work, and a deadline inside `async with limiter` times the work alone where one outside it counts the wait for a token. `move_on_after` ends its block at the deadline, and execution continues past it to the expiry fault:

```python
async def bounded[S, T](step: Callable[[S], Awaitable[Outcome[T]]], limiter: anyio.CapacityLimiter, limit: timedelta, subject: S, /) -> Outcome[T]:
    async with limiter:
        with anyio.move_on_after(limit / timedelta(seconds=1)):
            return await step(subject)
    return Error(Fault.of(Reason.EXPIRED, str(subject), limit))
```

`await anyio.amap(partial(bounded, loaded, limiter, limit), paths)` returns each expired item's fault beside its siblings' bytes:
- `amap` starts every item's task at once, and the limiter bounds how many pass `async with limiter`
- Callers own a deadline around a call, and a callee takes a limit where its deadline sits inside a scope it opens
- Limiters bind to the first event loop entering them, and each run's entry builds one per shared resource for every batch over it
- `anyio.Path(<path>, limiter=<limiter>)` and `open_file(..., limiter=<limiter>)` run under that limiter, and paths derived through `/` keep it

## [03]-[CANCELLATION]

Cancel scopes cancel at every checkpoint inside them until they exit, and an `await` in a `finally` under a cancelled scope raises at its first checkpoint, before its cleanup runs. `staged` runs a step that writes a scratch path and removes the path on every exit under a shielded scope its `limit` ends:

```python
async def staged[T](step: Callable[[anyio.Path], Awaitable[Outcome[T]]], limit: timedelta, scratch: anyio.Path, /) -> Outcome[T]:
    try:
        return await step(scratch)
    finally:
        with anyio.move_on_after(limit / timedelta(seconds=1), shield=True):
            await scratch.unlink(missing_ok=True)
```

Cancelling a task group around `staged` removes the scratch path before `async with` on the group returns, and a step's fault returns after the same removal:
- Releases free only what their scope acquired, and `missing_ok=True` covers a step cancelled before it wrote
- Shields block outer cancellation alone, and the shielded deadline ends a release that hangs
- `group.cancel(<reason>)` cancels every task, and `group.cancel_scope.cancelled_caught` reports a group that cancellation ended

## [04]-[WORKERS]

Workers run synchronous code off the event loop through one call shape, `await <runner>.run_sync(<function>, *<arguments>, limiter=<limiter>)`:

| [INDEX] | [WORK]                                                    | [RUNNER]         | [CANCELLATION]                                       |
| :-----: | :-------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | Blocking call, native code releasing the GIL              | `to_thread`      | Waits, `abandon_on_cancel=True` discards the outcome |
|  [02]   | CPU work holding the GIL                                  | `to_interpreter` | Waits for the worker                                 |
|  [03]   | Extension without subinterpreter support or able to crash | `to_process`     | Waits, `cancellable=True` kills the process          |

Workers run a whole synchronous pipeline in one call and return its `Result` to the loop as a value. Interpreter workers import the target's module, msgspec refuses to load in a subinterpreter, and a module holding an interpreter target imports the package faults `lazy`:

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
