# [CONCURRENCY]

Covers async Python under anyio, from awaited groups and tagged steps to deadlines, shielded cleanup, worker threads and processes, and backends.

## [01]-[GROUPS]

Awaited calls running a task group raise an `ExceptionGroup`, one failed task included, and `return` inside an `except*` block is a `SyntaxError`. `gathered` binds faults in `except*`, returns success from `else`, and returns faults after the block. `Fault`, `Outcome`, `Reason`, and `paired` come from the package faults:

```python
async def gathered(paths: Sequence[anyio.Path], /) -> Outcome[tuple[bytes, ...]]:
    try:
        contents = await anyio.gather(*(path.read_bytes() for path in paths))
    except* OSError as group:
        failed = Fault.of(Reason.UNREADABLE, ", ".join(map(str, paths)), group)
    else:
        return Ok(contents)
    return Error(failed)
```

First `OSError` cancels every other read in `gathered`, and its fault keeps the `ExceptionGroup` as cause with each failed read under `exceptions`. Steps after an awaited call stay outside `try`, and a handler around them reports their defects as the call's failure.

## [02]-[TAGGED_STEPS]

Synchronous steps chain on an awaited result through `bind` and `map`, and a step that awaits matches each awaited result's `tag`. Error arms return `Error(target.error)` under their function's success type, the one rewrap an `await` forces. `Reader` is the injected read step, a coroutine function returning `Outcome[bytes]`:

```python
type Reader = Callable[[anyio.Path], Coroutine[object, object, Outcome[bytes]]]


def named(folder: anyio.Path, raw: bytes, /) -> Outcome[anyio.Path]:
    return (
        catch(bytes.decode, exception=UnicodeDecodeError)(raw)
        .map(lambda text: folder / text.strip())
        .map_error(partial(Fault.of, Reason.INVALID, repr(raw)))
    )


async def followed(read: Reader, index: anyio.Path, /) -> Outcome[bytes]:
    target = (await read(index)).bind(partial(named, index.parent))
    match target.tag:
        case "ok":
            return await read(target.ok)
        case "error":
            return Error(target.error)


async def pair(read: Reader, left: anyio.Path, right: anyio.Path, /) -> Outcome[tuple[bytes, bytes]]:
    return paired(*await anyio.gather(read(left), read(right)))
```

`followed` reads an index file, decodes a file name from it, and reads the named file. Returning `target` itself from the error arm fails both checkers, `target` holds `anyio.Path` where `followed` declares `bytes`. Independent awaits start together through `anyio.gather`, and `paired` keeps the faults of every failed read:
- `anyio.gather` takes coroutines, a step typed `Awaitable` matches none of its overloads
- `Coroutine[object, object, R]` types an injected coroutine function with no explicit `Any`
- `*Ts` runners (`anyio.run`, `start_soon`, `run_sync`) take the target and its arguments, and both checkers reject `partial(anyio.run, <target>)`

## [03]-[DEADLINES]

Deadlines are the caller's scope, and an `async def` takes no `timeout` parameter. Each item holds one limiter token for its work, and its deadline opens inside the token and excludes time spent waiting for one:

```python
async def bounded(read: Reader, limiter: anyio.CapacityLimiter, seconds: float, path: anyio.Path, /) -> Outcome[bytes]:
    async with limiter:
        try:
            with anyio.fail_after(seconds):
                return await read(path)
        except TimeoutError as error:
            return Error(Fault.of(Reason.EXPIRED, str(path), error))


async def surveyed(read: Reader, paths: Iterable[anyio.Path], limit: int, seconds: float, /) -> list[Outcome[bytes]]:
    return await anyio.amap(partial(bounded, read, anyio.CapacityLimiter(limit), seconds), paths)
```

`Reason.EXPIRED` joins the package reason enum. Limiter and deadline placement decides what each bound covers:
- Expired items return their `EXPIRED` fault, and siblings return their bytes
- `amap` starts every item's task at once, and the limiter bounds how many run past `async with limiter`
- `fail_after` inside `async with limiter` times a read alone, outside it a wait for a token counts against each deadline
- `fail_after` raises `TimeoutError` where expiry is a failure, `move_on_after` sets `cancelled_caught` where expiry is a branch
- `fail_after` with no `await` inside never expires, deadlines fire at a checkpoint, and ruff `ASYNC100` reports the block
- Limiters bind to whichever event loop first enters them, each run's async entry builds one per shared resource (a host connection, a disk)
- Fan-outs over a shared resource receive its limiter as an argument and build none of their own

## [04]-[CANCELLATION]

Cancel scopes cancel at every checkpoint inside them until they exit, and an `await` in a `finally` under a cancelled scope raises at its first checkpoint, before its cleanup runs. `claimed` removes its marker under a shielded scope:

```python
@asynccontextmanager
async def claimed(marker: anyio.Path, /) -> AsyncGenerator[anyio.Path]:
    await marker.touch(exist_ok=False)
    try:
        yield marker
    finally:
        with anyio.CancelScope(shield=True):
            await marker.unlink(missing_ok=True)
```

Cancelling a task group with a task inside `claimed` removes its marker before `async with` on the group returns. Without a shield, `unlink` raises a pending cancellation and leaves its marker on disk. Cancellation acts on acquisition, shields, and scope state:
- Acquisition sits before `try`, a `FileExistsError` from `touch` leaves nothing to release and reaches the caller
- Cancellation is caught as `anyio.get_cancelled_exc_class()` alone and re-raised, every other handler lets it pass
- Shields block outer cancellation alone, cleanup that can hang runs under `move_on_after(<seconds>, shield=True)`
- `group.cancel()` takes a reason string, and `group.cancel_scope.cancelled_caught` reports a group that cancellation ended
- Cancellation stays level-triggered on asyncio under anyio, every later checkpoint in a cancelled scope raises again

## [05]-[WORKERS]

`digested` reads and hashes in a worker thread, and its `Result` returns to the loop as a value. Fan-outs over threads pass their own limiter through `limiter=`, and `content` is the package byte reader:

```python
def digested(path: Path, /) -> Outcome[str]:
    return content(path).map(lambda data: hashlib.sha256(data).hexdigest())


async def digest(limiter: anyio.CapacityLimiter, path: Path, /) -> Outcome[str]:
    return await to_thread.run_sync(digested, path, limiter=limiter)


async def digests(paths: Iterable[Path], limit: int, /) -> list[Outcome[str]]:
    return await anyio.amap(partial(digest, anyio.CapacityLimiter(limit)), paths)
```

`digest` gives `amap` a one-argument coroutine function. Thread calls follow `to_thread.run_sync`:
- Blocking raising calls run their `catch` inside the worker (`await to_thread.run_sync(catch(path.read_bytes, exception=OSError))`)
- Calls without `limiter=` draw from a default limiter of 40 tokens that every `anyio.Path` and `open_file` call shares
- `anyio.Path(<path>, limiter=<limiter>)` and `open_file(..., limiter=<limiter>)` run their thread calls under a given limiter
- Cancellation waits for a running worker by default, `run_sync` returns its value, and the next checkpoint raises
- `abandon_on_cancel=True` returns at cancellation and discards a worker's outcome while its thread runs to completion
- Workers calling `from_thread.check_cancelled()` between steps raise the cancellation at their next call

Process workers pickle a callable, its arguments, and its return value, and `@catch` on a module-level `def` keeps a function picklable under its own name. `catch(tomllib.loads, exception=...)` builds a wrapper `pickle` resolves to a different object, and `run_sync` raises `PicklingError`:

```python
@catch(exception=tomllib.TOMLDecodeError)
def parsed(text: str, /) -> dict[str, object]:
    return tomllib.loads(text)


async def table(name: str, text: str, /) -> Outcome[dict[str, object]]:
    outcome = await to_process.run_sync(parsed, text)
    return outcome.map_error(partial(Fault.of, Reason.INVALID, name))
```

`Result` cases and a caught `TOMLDecodeError` pickle back intact, and `map_error` builds faults in the parent. Process calls follow `to_process.run_sync`:
- Exceptions a worker raises pickle back and raise at the `await`
- Cancellation waits for a worker by default, `cancellable=True` kills its process
- Default process limiter holds `os.cpu_count()` tokens, and idle workers serve later calls

## [06]-[BACKENDS]

Code calling anyio members alone runs unchanged under asyncio and trio, and hosts select a backend at `anyio.run`:

```python
type Backend = Literal["asyncio", "trio"]


def run(paths: Sequence[anyio.Path], backend: Backend, /) -> list[Outcome[bytes]]:
    return anyio.run(surveyed, loaded, paths, 4, 1.0, backend=backend)
```

`loaded` is a `Reader` returning each read's outcome, and `run(paths, "asyncio")` and `run(paths, "trio")` return equal bytes and equal fault messages, a caught exception comparing by identity. Backends differ at their own calls and classes:
- Direct `asyncio` or `trio` calls bind code to one backend, `asyncio.sleep` under trio raises `RuntimeError` (no running event loop)
- `anyio.get_cancelled_exc_class()` returns `asyncio.CancelledError` or `trio.Cancelled` for the running backend
- `backend_options` passes loop settings to the backend (`{"use_uvloop": True}` on asyncio)
