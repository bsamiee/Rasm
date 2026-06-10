# [PYTHON_RUNTIME]

Python `>=3.15` is the active runtime surface. This page is the primitive-selection law for execution, transport, and observation: concurrency and interpreter isolation, binary payloads and numeric invariants, and diagnostics and exception flow. Each section owns one concern family: the chooser table names the form and the spelling it replaces, and the family card states the placement law and names the PEPs that canonize its rows once.

## [1]-[CONCURRENCY_AND_INTERPRETERS]

Mutation ownership and context propagation are explicit before code relies on scheduling or cache behavior.

| [INDEX] | [CONCERN]                  | [USE]                                                  | [REPLACE]                       |
| :-----: | :------------------------- | :----------------------------------------------------- | :------------------------------ |
|   [1]   | async task group           | `asyncio.TaskGroup`                                    | `gather()` task sets            |
|   [2]   | task-group stop            | `asyncio.TaskGroup.cancel()`                           | raiser-task sentinels           |
|   [3]   | async deadline             | `asyncio.timeout()` or `asyncio.timeout_at()`          | `wait_for()` wrapper ladders    |
|   [4]   | completed-task correlation | `async for` over `asyncio.as_completed()`              | task-result side maps           |
|   [5]   | event-loop lifetime        | `asyncio.Runner`                                       | manual loop lifecycle stacks    |
|   [6]   | async stream delimiter     | `asyncio.StreamReader.readuntil((...))`                | manual separator scans          |
|   [7]   | server client shutdown     | `close_clients()` and `abort_clients()`                | transport registries            |
|   [8]   | async eager execution      | `asyncio.eager_task_factory()`                         | cache-hit scheduling wrappers   |
|   [9]   | async task CLI inspection  | `python -m asyncio ps` and `pstree`                    | private task graph scraping     |
|  [10]   | async call graph           | `asyncio.capture_call_graph()` or `print_call_graph()` | private task graph scraping     |
|  [11]   | queue lifecycle            | `queue.Queue.shutdown()`                               | sentinel queue items            |
|  [12]   | async queue lifecycle      | `asyncio.Queue.shutdown()`                             | sentinel async-queue items      |
|  [13]   | context variable scope     | `ContextVar.set()` token context manager               | `reset(token)` `finally` blocks |
|  [14]   | bounded map                | `Executor.map(buffersize=...)`                         | submission throttling wrappers  |
|  [15]   | worker sizing              | `os.process_cpu_count()`                               | `os.cpu_count()` worker counts  |
|  [16]   | interpreter isolation      | `concurrent.interpreters`                              | process-only isolation wrappers |
|  [17]   | subinterpreter pool        | `concurrent.futures.InterpreterPoolExecutor`           | process-only CPU pools          |
|  [18]   | process-pool stop          | `terminate_workers()` or `kill_workers()`              | private worker traversal        |
|  [19]   | process interrupt          | `multiprocessing.Process.interrupt()`                  | cleanup-hostile `terminate()`   |
|  [20]   | iterator sharing           | `threading.serialize_iterator()` or `concurrent_tee()` | generator lock wrappers         |
|  [21]   | real timeout value         | real-number timeout arguments                          | int/float-only gates            |
|  [22]   | shared mutation            | explicit synchronization                               | implicit GIL serialization      |

[FREE_THREADING]:
- PEPs: 779, 703, 567.
- Use when: shared mutation, context propagation, or supported-target claims depend on free-threaded execution.
- Accept: free-threaded Python as a supported target, explicit synchronization for shared mutation, and `ContextVar` for async or thread context.
- Reject: experimental no-GIL caveats, implicit GIL serialization, thread-local async state, mutable ambient globals, and import-time singleton mutation as coordination.
- Law: free-threaded code makes mutation ownership and context propagation explicit before relying on scheduling or cache behavior.

```python conceptual
from collections.abc import Callable
from contextvars import ContextVar, copy_context
from functools import wraps
from threading import RLock

scope: ContextVar[str] = ContextVar("<field-a>", default="<value-a>")

def synchronized[**P, T](operation: Callable[P, T], /) -> Callable[P, T]:
    gate = RLock()
    @wraps(operation)
    def call(*args: P.args, **kwargs: P.kwargs) -> T:
        with gate: return copy_context().run(operation, *args, **kwargs)
    return call
```

[INTERPRETER_ISOLATION]:
- PEPs: 734, 684.
- Use when: interpreter isolation, independent execution, or CPU separation owns the runtime boundary.
- Accept: `concurrent.interpreters` and own-GIL interpreters.
- Reject: process-only isolation wrappers where interpreter isolation is the owner, and shared module state across interpreter boundaries.
- Law: interpreter boundaries own isolation directly; process wrappers survive only when the process boundary is the actual requirement.

```python conceptual
from collections.abc import Callable
from concurrent import interpreters
from functools import wraps
from typing import Literal

type InterpreterGil = Literal["own"]

def isolated[**P, T](entrypoint: str, /, *, gil: InterpreterGil = "own") -> Callable[[Callable[P, T]], Callable[P, T]]:
    def bind(operation: Callable[P, T], /) -> Callable[P, T]:
        route = operation.__module__, operation.__qualname__
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> T:
            with interpreters.create(gil=gil) as runtime: return runtime.call(entrypoint, route, args, kwargs)
        return call
    return bind
```

## [2]-[BINARY_AND_NUMERIC]

Binary boundaries carry buffer, compression, and serialization semantics directly; numeric invariants run on the numeric owner.

| [INDEX] | [CONCERN]             | [USE]                                          | [REPLACE]                           |
| :-----: | :-------------------- | :--------------------------------------------- | :---------------------------------- |
|   [1]   | buffer protocol       | `collections.abc.Buffer`                       | `ByteString` or bytes prose         |
|   [2]   | buffer flags          | `inspect.BufferFlags`                          | magic integer buffer flags          |
|   [3]   | binary numeric views  | `array` and `memoryview` complex codes         | struct-packed numeric buffers       |
|   [4]   | fd buffer read        | `os.readinto()`                                | `os.read()` copy slices             |
|   [5]   | byte buffer drain     | `bytearray.take_bytes()`                       | `bytes(buffer)` plus `clear()`      |
|   [6]   | FFI memory view       | `ctypes.memoryview_at()`                       | `string_at()` copy scaffolds        |
|   [7]   | pickle buffers        | protocol 5 out-of-band buffers                 | copy-heavy pickle blobs             |
|   [8]   | Zstandard payload     | `compression.zstd`                             | subprocess or bespoke zstd adapters |
|   [9]   | Z85 payload           | `base64.z85encode()` and `z85decode()`         | local Z85 codecs                    |
|  [10]   | base-N canonical      | `canonical=True` decoders                      | padding-bit postchecks              |
|  [11]   | base-N format control | `padded=`, `wrapcol=`, `ignorechars=`          | pre/post encode formatting          |
|  [12]   | JSON arrays           | `array_hook=` decoders                         | post-decode list walks              |
|  [13]   | file digest           | `hashlib.file_digest()`                        | manual chunked hash loops           |
|  [14]   | checksum combine      | `zlib.adler32_combine()` and `crc32_combine()` | recompress-to-checksum loops        |
|  [15]   | ordered identifiers   | `uuid.uuid7()`                                 | timestamp-prefixed UUID wrappers    |
|  [16]   | UUID boundaries       | `uuid.NIL` and `uuid.MAX`                      | magic UUID boundary literals        |
|  [17]   | integer math          | `math.integer`                                 | float math for integers             |
|  [18]   | fused multiply-add    | `math.fma()`                                   | rounded multiply-add                |
|  [19]   | float extrema         | `math.fmax()` and `math.fmin()`                | NaN-aware min/max wrappers          |
|  [20]   | float classification  | `isnormal()`, `issubnormal()`, `signbit()`     | bit-level float probes              |
|  [21]   | fraction conversion   | `Fraction.from_number()`                       | constructor branch ladders          |
|  [22]   | statistical density   | `statistics.kde()`                             | local kernel-density estimators     |

[BINARY_TRANSPORT]:
- PEPs: 688, 574, 784.
- Use when: binary payloads, buffers, compressed data, or serialized streams cross boundaries.
- Accept: `Buffer`, `__buffer__`, protocol 5 out-of-band buffers, and `compression.zstd`.
- Reject: `bytes` or `ByteString` buffer aliases, copy-heavy pickle blobs, bespoke zstd adapters, subprocess compression shells, and pre-decoded byte piles that discard buffer ownership.

[NUMERIC_INVARIANTS]:
- PEPs: 791.
- Use when: integer, float, fraction, or density invariants must run on the owning numeric primitive.
- Accept: `math.integer` on integer algorithms, IEEE-aware extrema and classification, `Fraction.from_number()`, and `statistics.kde()`.
- Reject: float-path integer helpers, NaN-aware wrappers, bit-level float probes, and local density estimators.

```python conceptual
from collections.abc import Buffer, Callable
import compression.zstd as zstd
import pickle

type BinaryFrame = tuple[bytes, tuple[pickle.PickleBuffer, ...]]

class BinaryPacket[T: Buffer]:
    def __init__(self, payload: T, /): self._view = memoryview(payload)
    def __buffer__(self, flags: int, /) -> memoryview: return self._view

def encode[T: Buffer](payload: T, /, shape: Callable[[pickle.PickleBuffer], object]) -> BinaryFrame:
    buffers: list[pickle.PickleBuffer] = []
    stream = pickle.dumps(shape(pickle.PickleBuffer(BinaryPacket(payload))), protocol=5, buffer_callback=buffers.append)
    return zstd.compress(stream), tuple(buffers)
```

## [3]-[DIAGNOSTICS_AND_EXCEPTIONS]

Diagnostics use runtime-owned observation surfaces; exception flow preserves the failure set and causal context.

| [INDEX] | [CONCERN]            | [USE]                                 | [REPLACE]                          |
| :-----: | :------------------- | :------------------------------------ | :--------------------------------- |
|   [1]   | execution monitoring | `sys.monitoring`                      | `settrace()` event scrapers        |
|   [2]   | sampling profiler    | `profiling.sampling`                  | handwritten timers or `profile`    |
|   [3]   | native profiling     | default frame pointers                | opaque native-extension stacks     |
|   [4]   | C-stack dump         | `faulthandler.dump_c_stack()`         | external native stack probes       |
|   [5]   | live debug attach    | `sys.remote_exec()`                   | debugger injection hooks           |
|   [6]   | runtime auditing     | audit hooks                           | monkeypatch security probes        |
|   [7]   | ABI reflection       | `sys.abi_info`                        | parsed SOABI strings               |
|   [8]   | debug line tables    | `co_lines()`                          | `co_lnotab` decoding               |
|   [9]   | traceback locations  | fine-grained code positions           | line-only diagnostics              |
|  [10]   | grouped exceptions   | `except*` handlers                    | single-error collapse              |
|  [11]   | exception context    | `BaseException.add_note()`            | message concatenation              |
|  [12]   | active exception     | `sys.exception()`                     | `sys.exc_info()[1]`                |
|  [13]   | exception syntax     | unparenthesized `except` without `as` | tuple wrapper noise                |
|  [14]   | finally control flow | exits kept out of `finally`           | `finally` control-flow exits       |
|  [15]   | deprecation marker   | `@warnings.deprecated()`              | docstring-only deprecation notices |
|  [16]   | warnings filter      | `/regex/` warning filters             | literal-only warning fields        |

[RUNTIME_EVIDENCE]:
- PEPs: 831, 799, 768, 669, 578, 626, 657.
- Use when: runtime-owned evidence should explain execution behavior, performance, security observation, or failure location.
- Accept: frame-pointer-preserving builds, profiling namespaces, safe debug attach points, `sys.monitoring`, audit hooks, `co_lines()`, and fine-grained traceback locations.
- Reject: frame-pointer-stripped native builds, legacy `profile`, debugger injection hooks, `settrace()` event scrapers, monkeypatch security probes, `co_lnotab` decoding, and line-only diagnostics.

[EXCEPTION_FLOW]:
- PEPs: 765, 654, 678, 758.
- Use when: exception structure, grouped failure transport, or handler syntax changes control-flow semantics.
- Accept: `except*`, `BaseException.add_note()`, exits kept out of `finally`, and unparenthesized exception handlers without `as`.
- Reject: single-error collapse, message concatenation, `return`, `break`, or `continue` that exits `finally`, tuple-wrapper noise, and handler branches that erase grouped-failure identity.
- Law: exception flow preserves the failure set and causal context; syntax cleanup is allowed only when it keeps the handled shape visible.

```python conceptual
from collections.abc import Callable
import sys

type DiagnosticSink = Callable[[str, dict[str, object]], None]

def observed(surface: str, sink: DiagnosticSink, /, *, tool: int = sys.monitoring.PROFILER_ID) -> int:
    monitoring = sys.monitoring
    monitoring.use_tool_id(tool, surface)
    event = monitoring.events.RAISE
    def raised(frame, offset: int, error: BaseException, /) -> None:
        code = frame.f_code
        sink("<raise>", {"offset": offset, "lines": tuple(code.co_lines()), "positions": tuple(code.co_positions()), "error": error})
    sys.addaudithook(lambda name, args: sink("<audit>", {"event": name, "args": args}))
    monitoring.register_callback(tool, event, raised); monitoring.set_events(tool, event)
    return tool
```
