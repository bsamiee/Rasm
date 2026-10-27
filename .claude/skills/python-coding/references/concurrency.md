# [CONCURRENCY]

Covers async Python under anyio, from one awaited call returning its result to bounded fan-out, deadlines, shielded cleanup, and worker threads and processes.

## [01]-[AWAITED_CALLS]

`try` holds one awaited raising call, its handler returns an `Error`, and success returns after the block:

```python
async def loaded(path: anyio.Path, /) -> Result[bytes, Faults]:
    try:
        content = await path.read_bytes()
    except OSError as error:
        return Error(Fault.caught(Reason.UNREADABLE, path, error))
    return Ok(content)
```

Steps after an awaited call stay outside `try`, a handler around them reports their defects as the call's failure. Awaited calls running a task group raise an `ExceptionGroup` even for one failed task, and `return` inside an `except*` block is a `SyntaxError`. Handlers over a group bind faults in `except*`, `else` returns success, and functions return faults after the block:

```python
async def gathered(paths: Sequence[anyio.Path], /) -> Result[tuple[bytes, ...], Faults]:
    try:
        contents = await anyio.gather(*(path.read_bytes() for path in paths))
    except* OSError as group:
        failed = Fault.caught(Reason.UNREADABLE, paths, group)
    else:
        return Ok(contents)
    return Error(failed)
```

First `OSError` cancels every other read in `gathered`, and `Fault.caught` turns each group leaf into one fault with the leaf as cause. Reads that fail independently run as separate tasks returning their own results (`amap` over `loaded`).

## [02]-[TAGGED_STEPS]

Synchronous steps chain on an awaited result through `bind` and `map`, and a step that awaits matches each awaited result's `tag`. Error arms return `Error(target.error)` under their function's success type, the one rewrap an `await` forces:

```python
def named(folder: anyio.Path, raw: bytes, /) -> Result[anyio.Path, Faults]:
    return (
        catch(bytes.decode, exception=UnicodeDecodeError)(raw)
        .map(lambda text: folder / text.strip())
        .map_error(partial(Fault.caught, Reason.INVALID, raw))
    )


async def followed(index: anyio.Path, /) -> Result[bytes, Faults]:
    target = (await loaded(index)).bind(partial(named, index.parent))
    match target.tag:
        case "ok":
            return await loaded(target.ok)
        case "error":
            return Error(target.error)
```

`followed` reads an index file, decodes a file name from it, and reads the named file. Returning `target` itself from the error arm fails both checkers, `target` holds `anyio.Path` where `followed` declares `bytes`. Independent awaits start together through `anyio.gather`, and a match over both tags keeps the faults of every failed read:

```python
async def paired(left: anyio.Path, right: anyio.Path, /) -> Result[tuple[bytes, bytes], Faults]:
    first, second = await anyio.gather(loaded(left), loaded(right))
    match first.tag, second.tag:
        case "ok", "ok":
            return Ok((first.ok, second.ok))
        case _:
            return Error((*first.swap().default_value(()), *second.swap().default_value(())))
```

## [03]-[BOUNDED_FAN_OUT]

Fan-outs bind their policy into a per-item step with `partial` and pass `anyio.amap` the step and its items. Each item holds one limiter token for its work, and its deadline opens inside the token and excludes time spent waiting for one:

```python
type Reader = Callable[[anyio.Path], Awaitable[Result[bytes, Faults]]]


async def bounded(read: Reader, limiter: anyio.CapacityLimiter, seconds: float, path: anyio.Path, /) -> Result[bytes, Faults]:
    async with limiter:
        try:
            with anyio.fail_after(seconds):
                return await read(path)
        except TimeoutError as error:
            return Error(Fault.caught(Reason.EXPIRED, path, error))


async def surveyed(read: Reader, paths: Iterable[anyio.Path], limit: int, seconds: float, /) -> list[Result[bytes, Faults]]:
    return await anyio.amap(partial(bounded, read, anyio.CapacityLimiter(limit), seconds), paths)
```

`Reason.EXPIRED` joins the package reason enum, and `Reader` is an injected read step (`loaded` or a stub). Limiter and deadline placement decides what each bound covers:
- Expired items return their `EXPIRED` fault, and siblings return their bytes
- `amap` starts every item's task at once, and the limiter bounds how many run past `async with limiter`
- `fail_after` inside `async with limiter` times a read alone, outside it a wait for a token counts against each deadline
- `fail_after` with no `await` inside never expires, deadlines fire at a checkpoint, and ruff `ASYNC100` reports the block
- Limiters bind to whichever event loop first enters them, each run's async entry builds one per shared resource (a host connection, a disk)
- Fan-outs over a shared resource receive its limiter as an argument and build none of their own

## [04]-[CANCELLATION]

Cancel scopes cancel at every checkpoint inside them until they exit, and an `await` in a `finally` under a cancelled scope raises at its first checkpoint, before its cleanup runs. Cleanup that awaits runs under a shielded scope:

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
- Shields block outer cancellation alone, cleanup that can hang runs under `move_on_after(<seconds>, shield=True)`
- `group.cancel()` takes a reason string, and `group.cancel_scope.cancelled_caught` reports a group that cancellation ended
- Cancellation stays level-triggered on asyncio under anyio, every later checkpoint in a cancelled scope raises again

## [05]-[WORKERS]

Blocking work runs as one synchronous function in a worker thread with its `catch` inside, and its `Result` returns to the loop as a value. Fan-outs over threads pass their own limiter through `limiter=`:

```python
def digested(path: Path, /) -> Result[str, Faults]:
    return (
        catch(path.read_bytes, exception=OSError)()
        .map(lambda content: hashlib.sha256(content).hexdigest())
        .map_error(partial(Fault.caught, Reason.UNREADABLE, path))
    )


async def digest(limiter: anyio.CapacityLimiter, path: Path, /) -> Result[str, Faults]:
    return await to_thread.run_sync(digested, path, limiter=limiter)


async def digests(paths: Iterable[Path], limit: int, /) -> list[Result[str, Faults]]:
    return await anyio.amap(partial(digest, anyio.CapacityLimiter(limit)), paths)
```

`digest` gives `amap` a one-argument coroutine function, and hashing runs beside the read inside a worker. Thread calls follow `to_thread.run_sync`:
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


async def table(name: str, text: str, /) -> Result[dict[str, object], Faults]:
    outcome = await to_process.run_sync(parsed, text)
    return outcome.map_error(partial(Fault.caught, Reason.INVALID, name))
```

`Result` cases and a caught `TOMLDecodeError` pickle back intact, and `map_error` builds faults in the parent. Process calls follow `to_process.run_sync`:
- Exceptions a worker raises pickle back and raise at the `await`
- Cancellation waits for a worker by default, `cancellable=True` kills its process
- Default process limiter holds `os.cpu_count()` tokens, and idle workers serve later calls
- Workers re-run the parent's main module as `__mp_main__` before their first call

## [06]-[BLOCKING_CALLS]

Blocking calls inside an `async def` stall every task on the loop, and ruff reports each one with its async member:

| [INDEX] | [BLOCKING]                           | [ASYNC_MEMBER]                      | [RULE]                 |
| :-----: | :----------------------------------- | :---------------------------------- | :--------------------- |
|  [01]   | `open`, `pathlib.Path` I/O           | `anyio.open_file`, `anyio.Path`     | `ASYNC230`, `ASYNC240` |
|  [02]   | `time.sleep`                         | `anyio.sleep`                       | `ASYNC251`             |
|  [03]   | `subprocess.run`, `Popen`, `os.wait` | `anyio.run_process`, `open_process` | `ASYNC220`-`ASYNC222`  |
|  [04]   | Sleep loop polling a flag            | `anyio.Event`                       | `ASYNC110`             |
|  [05]   | `anyio.sleep(0)`                     | `anyio.lowlevel.checkpoint()`       | `ASYNC115`             |
|  [06]   | Sleep longer than a day              | `anyio.sleep_forever()`             | `ASYNC116`             |
|  [07]   | Synchronous HTTP client call         | `httpx.AsyncClient`                 | `ASYNC210`, `ASYNC212` |
|  [08]   | `input()`                            | `to_thread.run_sync(input)`         | `ASYNC250`             |

## [07]-[BACKENDS]

Code calling anyio members alone runs unchanged under asyncio and trio, and hosts select a backend at `anyio.run`:

```python
type Backend = Literal["asyncio", "trio"]


def run(paths: Sequence[anyio.Path], backend: Backend, /) -> list[Result[bytes, Faults]]:
    return anyio.run(surveyed, loaded, paths, 4, 1.0, backend=backend)
```

`run(paths, "asyncio")` and `run(paths, "trio")` return equal lists. Backends differ at their own calls and classes:
- Direct `asyncio` or `trio` calls bind code to one backend, `asyncio.sleep` under trio raises `RuntimeError` (no running event loop)
- `anyio.get_cancelled_exc_class()` returns `asyncio.CancelledError` or `trio.Cancelled` for the running backend
- `backend_options` passes loop settings to the backend (`{"use_uvloop": True}` on asyncio)
