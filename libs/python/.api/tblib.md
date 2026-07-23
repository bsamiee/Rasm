# [PY_BRANCH_API_TBLIB]

`tblib` carries a traceback across the pickle seam so a worker-side exception re-raises parent-side with its frames intact. `Traceback` is the explicit carrier — wrapping a live traceback, folding to a dict or parsed string for wire transport, and rebuilding a native traceback for `raise exc.with_traceback(...)`; `pickling_support.install` is the process-global `copyreg` monkeypatch that round-trips every `BaseException` and `TracebackType` through pickle with tracebacks, feeding `BoundaryFault.of` the true worker cause instead of a flattened marker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tblib`
- package: `tblib` (BSD-2-Clause)
- module: `tblib`
- namespaces: `tblib`, `tblib.pickling_support`
- rail: cross-process traceback and fault fidelity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: carrier types

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `tblib.Traceback`           | carrier       | picklable traceback wrapping one native `tb`        |
|  [02]   | `tblib.Frame`               | carrier       | picklable frame snapshot; `clear()` is a no-op stub |
|  [03]   | `tblib.Code`                | carrier       | picklable code snapshot; `co_code` is `None`        |
|  [04]   | `tblib.TracebackParseError` | exception     | `from_string` found no frame under strict parse     |

[PUBLIC_TYPE_SCOPE]: parse and locals helpers

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `tblib.get_all_locals` | projection    | `(frame) -> dict` snapshotting `f_locals`                      |
|  [02]   | `tblib.FRAME_RE`       | parser        | compiled regex matching one `File "...", line N, in name` line |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: carrier construction and round-trips

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Traceback(tb, *, get_locals=None)`          | ctor     | wrap one live traceback into the carrier             |
|  [02]   | `to_dict()` / `as_dict()`                    | instance | fold to a nested frame dict (one method, two names)  |
|  [03]   | `Traceback.from_dict(dct)`                   | factory  | rebuild the carrier from a frame dict                |
|  [04]   | `Traceback.from_string(string, strict=True)` | factory  | parse a formatted stacktrace into frames             |
|  [05]   | `as_traceback()` / `to_traceback()`          | instance | build a native `traceback` for re-raise (one method) |
|  [06]   | `tb_next`                                    | property | next carrier down the chain, `None` at the tail      |

- `from_string`: `strict=True` skips every line until the `Traceback (most recent call last):` header then reads indented `File` lines to the first non-frame line; `strict=False` reads `File` lines from the first line; no frame raises `TracebackParseError`.

[ENTRYPOINT_SCOPE]: pickling-support install

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `install(*exc_classes_or_instances, get_locals=None)` | static  | register traceback + exception pickle reducers |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- two rails own one concern: `Traceback` is the explicit carrier a producer wraps, transports, and re-raises by hand; `pickling_support.install` monkeypatches ordinary `pickle` to carry the traceback with no carrier code at the call site.
- `Traceback` reconstructs a native traceback over synthetic per-frame `Code`: `as_traceback` recovers `co_name` and `tb_lineno` while `co_code` is `None`, `f_globals` a two-key stub (`__file__`, `__name__`), and `f_locals` empty unless the carrier was built with `get_locals=get_all_locals`; the result is displayable and re-raisable, never executable, and treating a reconstructed frame as live is a defect. Source text re-reads from the file at display time, never carried.
- `to_dict`/`from_dict` is the lossless structured round-trip for encoded payloads; `from_string` is the lossy recovery of frames from an already-formatted stacktrace when only text survived.
- `copyreg.dispatch_table` keys on exact type, so re-running `install()` overwrites each reducer in place and never stacks a hook — a repeat call nets a no-op latch. No positional argument registers the `TracebackType` reducer and one reducer per current `BaseException` subclass walked at call time, so a subclass defined afterward stays on the default pickle path until a later `install`.
- `install(cls)` registers that exact class alone; `install(exc)` registers `type(exc)` and recurses `__cause__`, `__context__`, and `ExceptionGroup.exceptions`, so a live fault graph installs whole from its root; `@install` above a class registers and returns it, while a multi-positional `install(A, B)` returns `None`.
- a round-tripped exception carries `__traceback__`, `__cause__`, `__context__`, `suppress_context`, and `__notes__`; each chained cause and context round-trips with its own traceback, and a worker-fan `ExceptionGroup` rebuilds every member with frames intact.
- `get_locals=get_all_locals` packs each frame's `f_locals` into the pickled traceback; one unpicklable local — a module, socket, open handle — raises `TypeError` mid-pickle and destroys the whole crossing, so default-off locals capture is a correctness floor.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): `fidelity()` wraps `pickling_support.install()` under a per-interpreter `@cache` one-shot, and each `KIND_POLICY` row carries the `fidelity` obligation — `THREAD`/`DAEMON` share the interpreter and `WASM` crosses no pickle seam (`fidelity=False`), while `INTERPRETER`, `PROCESS`, `GPU`, and `REMOTE` set `fidelity=True`. Every `WorkerPool` executor runs the latch through `initializer=fidelity` and re-arms it on the worker floor, since a spawned interpreter starts cold and the `copyreg` table never crosses the seam with it.
- `reliability/faults`(`runtime/.planning/reliability/faults.md`): a pickled worker raise re-crosses with frames intact, so `BoundaryFault.of(subject, cause)` runs the `CLASSIFY` `isinstance` rows against the true worker exception rather than a flattened subprocess marker; a worker-fan `ExceptionGroup` lands the `BoundaryFault.aggregate` case through `combine`, each member fault keeping its own reconstructed traceback.
- `cloudpickle`(`.api/cloudpickle.md`): the two codecs split one PROCESS hop — `cloudpickle.dumps(fn)` ships the kernel in by value and tblib ships the fault out; absent the fidelity latch the shipped kernel's raise degrades to a traceback-less marker.
- `loky`(`.api/loky.md`) and `pebble`(`.api/pebble.md`): a worker that RAISES pickles its exception back through tblib with frames intact (→ `BoundaryFault.of` on the true type), while a worker that DIES — `TerminatedWorkerError`/`BrokenProcessPool`, `ProcessExpired`/`TimeoutError` — is pool-synthesized on the pending future with no worker traceback, landing the resilience `WORKER` retry band or the faults `resource`/`deadline` rows. Both pools arm the latch through `initializer=fidelity`.
- `msgspec`(`.api/msgspec.md`): the DATA wire form disjoint from the pickle-exception form — `Traceback.to_dict` yields a plain nested `{tb_frame, tb_lineno, tb_next}` dict `msgspec.json.encode` serializes, and `decode` -> `Traceback.from_dict` -> `as_traceback` rebuilds the native traceback when it rides as structured payload outside the exception-object path.
- `structlog`(`.api/structlog.md`): renders what tblib reconstructs — `processors.format_exc_info` folds the rebuilt `__traceback__` to a string and `processors.dict_tracebacks` to a JSON-safe frame list, a display projection distinct from `Traceback.to_dict`, which alone rebuilds a re-raisable native traceback.

[LOCAL_ADMISSION]:
- `install()` at process init is the worker-fabric default, making every crossing exception faithful with zero carrier code at the call site; the explicit `Traceback` carrier serves a consumer transporting a traceback as data over `to_dict`/`from_dict` outside the pickle path.
- locals capture stays off unless a fault's frame locals are the diagnostic payload, because one unpicklable local breaks the whole crossing before it inflates any wire form.
- `from_string` is the last-resort recovery lane when only formatted text survived a boundary; a producer owning the live exception uses the pickle rail or `to_dict`.
- re-raise is `raise exc.with_traceback(tb.as_traceback())` at the owning boundary, and interior code receives the `Result`/`Option` rail.

[RAIL_LAW]:
- Package: `tblib`
- Owns: cross-process traceback fidelity — the picklable `Traceback` carrier, dict and stacktrace-string round-trips, native-traceback reconstruction for re-raise, and the process-global `pickling_support.install` latch round-tripping any exception and traceback through pickle
- Accept: `install()` at worker-fabric init; per-class `install(cls)` or per-instance `install(exc)` over the instance's cause/context/group graph; the explicit `Traceback` carrier for dict/string transport; `raise exc.with_traceback(tb.as_traceback())` at the owning boundary
- Reject: a hand-rolled traceback serializer; a reconstructed frame treated as live or executable; default-on locals capture across an untrusted crossing; a per-submission install where the fabric-init latch already holds; `from_string` re-parsing over a live exception the pickle rail owns
