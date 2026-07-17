# [PY_BRANCH_API_TBLIB]

`tblib` carries a Python traceback across a pickle seam so a worker-side exception re-raises parent-side with its frames intact. `Traceback` is the explicit carrier — it wraps a live traceback, folds to a dict or a parsed stacktrace string for wire transport, and rebuilds a native traceback for `raise exc.with_traceback(...)`. `pickling_support.install` is the monkeypatch rail — one process-global `copyreg` registration that makes every `BaseException` and `TracebackType` pickle-round-trip with its traceback attached. It is the runtime owner for cross-process fault fidelity, feeding `BoundaryFault.of` the true worker cause instead of a flattened subprocess marker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tblib`
- package: `tblib`
- version: `3.2.2`
- license: BSD-2-Clause
- import: `tblib` / `tblib.pickling_support`
- owner: `runtime`
- rail: traceback and fault fidelity
- namespaces: `tblib`, `tblib.pickling_support`
- capability: a picklable `Traceback` carrier, dict and stacktrace-string round-trips, native-traceback reconstruction for re-raise, optional per-frame locals capture, and a process-global `copyreg` install that round-trips any exception and traceback through pickle

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: carrier types
- rail: traceback

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `tblib.Traceback`           | carrier       | picklable traceback wrapping one native `tb`                 |
|  [02]   | `tblib.Frame`               | carrier       | picklable frame snapshot; `clear()` drops captured locals    |
|  [03]   | `tblib.Code`                | carrier       | picklable code snapshot; `co_code` is `None` by construction |
|  [04]   | `tblib.TracebackParseError` | fault         | `from_string` finds no frame under strict parse              |

[PUBLIC_TYPE_SCOPE]: parse and locals helpers
- rail: traceback

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                                         |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `tblib.get_all_locals` | projection    | `(frame) -> dict` snapshotting `frame.f_locals`                |
|  [02]   | `tblib.FRAME_RE`       | parser        | compiled regex matching one `File "...", line N, in name` line |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: carrier construction and round-trips
- rail: traceback
- `Traceback(tb, *, get_locals=None)` wraps a live traceback; `get_locals=get_all_locals` snapshots each frame's `f_locals`, absent it captures none. `to_dict`/`as_dict` fold to a nested `{tb_frame, tb_lineno, tb_next}` dict for wire transport; `from_dict` rebuilds the carrier and `as_traceback`/`to_traceback` reconstructs a native traceback for `raise exc.with_traceback(tb.as_traceback())`. `from_string` parses a formatted stacktrace back into frames.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Traceback(tb, *, get_locals=None)`          | build          | wrap one live traceback into the carrier                |
|  [02]   | `to_dict()` / `as_dict()`                    | encode         | fold to a nested frame dict (exact aliases)             |
|  [03]   | `Traceback.from_dict(dct)`                   | decode         | rebuild the carrier from a frame dict                   |
|  [04]   | `Traceback.from_string(string, strict=True)` | decode         | parse a formatted stacktrace into frames                |
|  [05]   | `as_traceback()` / `to_traceback()`          | reconstruct    | build a native `traceback` for re-raise (exact aliases) |
|  [06]   | `tb_next`                                    | link           | next carrier down the chain, `None` at the tail         |

Both `as_dict` and `as_traceback` are the exact same class objects as `to_dict` and `to_traceback` — one name pair, one method. `from_string` under `strict=True` skips every line until the literal `Traceback (most recent call last):` header, then reads indented `File` lines and stops at the first non-indented non-frame line; `strict=False` reads `File` lines from the first line with no header. Absence of any frame raises `TracebackParseError`.

[ENTRYPOINT_SCOPE]: pickling-support install and reducers
- rail: fault fidelity
- `install(*exc_classes_or_instances, get_locals=None)` registers the traceback and exception reducers through `copyreg.dispatch_table`. No positional argument registers the `TracebackType` reducer plus one `pickle_exception` reducer per current `BaseException` subclass, walking the live subclass tree at call time — `copyreg` dispatch is exact-type, so a subclass defined after this call stays on the default pickle path until a later `install`. A class positional registers that exact class alone, never its subclasses; an instance positional registers `type(exc)` and recurses its `__cause__`, `__context__`, and `ExceptionGroup.exceptions` members. `get_locals=get_all_locals` threads locals capture into every traceback the reducer packs.

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :---------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `install(*exc_classes_or_instances, get_locals=None)` | install        | register traceback + exception pickle reducers |

Reducers name three pickle-side reconstructors the caller never invokes directly: `unpickle_traceback(tb_frame, tb_lineno, tb_next)` rebuilds a traceback, `unpickle_exception(func, args, cause, tb, context=None, suppress_context=False, notes=None)` rebuilds an exception, and `unpickle_exception_with_attrs(func, attrs, cause, tb, context, suppress_context, notes, args=())` additionally restores a subclass instance `__dict__`.

## [04]-[IMPLEMENTATION_LAW]

[TBLIB_TOPOLOGY]:
- two rails own one concern: `Traceback` is the explicit carrier a producer wraps, transports, and re-raises by hand; `pickling_support.install` is the monkeypatch that makes ordinary `pickle` carry the traceback with no carrier in the caller's code.
- `Traceback` reconstructs a native traceback pointing at synthetic per-frame code, so `as_traceback` recovers frame `co_name` and `tb_lineno`; the frame's `f_locals` is empty unless the carrier was built with `get_locals=get_all_locals`, and source text is never carried — the interpreter re-reads it from the file at display time.
- `Code.co_code` is `None` and `f_globals` is a two-key stub (`__file__`, `__name__`) — the carrier reconstructs a displayable and re-raisable traceback, not an executable frame; a consumer treating a reconstructed frame as live is a defect.
- dict transport and string transport are the two wire forms: `to_dict`/`from_dict` is the lossless structured round-trip for `msgspec`-encoded payloads, `from_string` is the lossy recovery of frames from an already-formatted stacktrace when only text survived.

[PICKLING_SUPPORT]:
- global install is idempotent: `copyreg.dispatch_table` keys on exact type, so re-running `install()` overwrites each type's reducer in place and never stacks a hook — `pickle_exception` re-binds by shared function identity while the `TracebackType` reducer re-binds as a fresh behaviorally-identical `partial`, and a repeat call nets a no-op latch.
- per-class install targets one exact class: `install(MyErr)` registers `MyErr` alone and leaves every subclass and sibling on the default pickle path; the instance form `install(exc)` registers `type(exc)` and recurses `__cause__`, `__context__`, and `ExceptionGroup.exceptions` members, so a live fault graph installs whole from its root instance.
- decorator form is the single class positional applied at definition: `@install` above a `class MyErr(Exception)` registers that class and returns it unchanged; a multi-positional `install(A, B)` returns `None`.
- a round-tripped exception carries its `__traceback__`, `__cause__`, `__context__`, `suppress_context`, and `__notes__`; each chained cause and context round-trips with its own traceback, and a worker-fan `ExceptionGroup` round-trips every member exception with frames intact, so a `raise ... from ...` chain and a multi-fault group both rebuild parent-side whole.
- locals capture is a crossing hazard, not a size knob: `install(get_locals=get_all_locals)` packs each frame's `f_locals` into the pickled traceback, and one unpicklable local — a module, a socket, an open handle — raises `TypeError` mid-pickle and destroys the whole crossing, so default-off locals capture is a correctness floor.

[STACKING]:
- `execution/workers`(`.planning/execution/workers.md`) owns the tblib latch: `fidelity()` wraps `pickling_support.install()` under a per-interpreter `@cache` one-shot, and each `KIND_POLICY` row carries the `fidelity` obligation — `THREAD` and `DAEMON` share the interpreter and `WASM` crosses no pickle seam, so each sets `fidelity=False`; `INTERPRETER`, `PROCESS`, `GPU`, and `REMOTE` set `fidelity=True`. Every `WorkerPool` executor runs the latch through `initializer=fidelity` and re-arms it on the worker floor through pool priming, since a spawned interpreter starts cold and the `copyreg` table never crosses the seam with it.
- `reliability/faults`(`.planning/reliability/faults.md`): a pickled worker raise re-crosses with frames intact, so `BoundaryFault.of(subject, cause)` runs the `CLASSIFY` `isinstance` rows against the true worker exception type and message rather than a flattened subprocess marker; a worker-fan `ExceptionGroup` round-trips every member and lands the `BoundaryFault.aggregate` case through `combine`, each member fault keeping its own reconstructed traceback as evidence.
- `cloudpickle`(`.api/cloudpickle.md`) owns the reverse direction of the same PROCESS hop: `cloudpickle.dumps(fn)` ships the kernel — lambda, closure, `<locals>` callable — in by value, and tblib ships the fault out, so the two codecs split call-ingress from fault-egress across one crossing; absent the fidelity latch the shipped kernel's raise degrades to a traceback-less marker.
- `loky`(`.api/loky.md`) and `pebble`(`.api/pebble.md`) split into two disjoint fault routes each: a worker that RAISES and returns pickles its exception back through tblib with frames intact (→ `BoundaryFault.of` on the true type), while a worker that DIES — `TerminatedWorkerError`/`BrokenProcessPool` under loky, `ProcessExpired`/`TimeoutError` under pebble's `schedule(timeout=)` kill — is pool-synthesized on the pending future with no worker traceback for tblib to carry, landing the resilience `WORKER` retry band or the faults `resource`/`deadline` rows. A death-fault never routes through tblib: the TERMINAL kill severs the worker before it can pickle. Both pools arm the latch through `initializer=fidelity`.
- `msgspec`(`.api/msgspec.md`) owns the DATA wire form disjoint from the pickle-exception form: `Traceback.to_dict` yields a plain nested `{tb_frame, tb_lineno, tb_next}` dict `msgspec.json.encode` serializes, and `msgspec.json.decode` -> `Traceback.from_dict` -> `as_traceback` rebuilds the native traceback when a traceback rides as structured payload outside the exception-object path.
- `structlog`(`.api/structlog.md`) renders what tblib reconstructs, never the reverse: `processors.format_exc_info` folds the tblib-rebuilt `__traceback__` to a string and `processors.dict_tracebacks` folds it to a JSON-safe frame list — a display projection distinct from `Traceback.to_dict`, which alone rebuilds a re-raisable native traceback.
- worker fabric: the fidelity latch is a fabric-level fact run once at parent init and once in each worker initializer, never a per-submission or per-lane call, and re-running it anywhere is the idempotent no-op.

[LOCAL_ADMISSION]:
- monkeypatch rail is the default for the worker fabric: `install()` at process init makes every crossing exception faithful with zero carrier code at the call site; the explicit `Traceback` carrier is reserved for a consumer that transports a traceback as data over `to_dict`/`from_dict` outside the pickle path.
- locals capture stays off by default: `get_locals=get_all_locals` is admitted only where a fault's frame locals are the diagnostic payload, because one unpicklable local raises `TypeError` and breaks the whole crossing before it ever inflates a wire form; a locals-bearing carrier is scrubbed with `Frame.clear` before an untrusted crossing.
- `from_string` is the last-resort recovery lane when only formatted text survived a boundary; a producer that owns the live exception uses the pickle rail or `to_dict`, never re-parses its own formatted output.
- re-raise is `raise exc.with_traceback(tb.as_traceback())`: the reconstructed native traceback attaches to the rehydrated exception at the boundary, and interior code receives the `Result`/`Option` rail, never the raise.

[RAIL_LAW]:
- Package: `tblib`
- Owns: cross-process traceback fidelity — the picklable `Traceback` carrier, dict and stacktrace-string round-trips, native-traceback reconstruction for re-raise, and the process-global `pickling_support.install` latch that round-trips any exception and traceback through pickle
- Accept: `pickling_support.install()` at worker-fabric init; per-exact-class `install(cls)` or per-instance `install(exc)` over the instance's cause/context/group graph; the explicit `Traceback` carrier for dict/string wire transport; `raise exc.with_traceback(tb.as_traceback())` at the owning boundary
- Reject: a hand-rolled traceback serializer; treating a reconstructed frame as live or executable; default-on locals capture across an untrusted crossing; a per-submission or per-lane install where the fabric-init latch already holds; `from_string` re-parsing over a live exception the pickle rail owns
