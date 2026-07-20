# [PY_BRANCH_API_CLOUDPICKLE]

`cloudpickle` extends the stdlib pickle protocol to serialize the callables and types stdlib pickle rejects — lambdas, closures over live cell state, module-local and `__main__`-defined functions, dynamic classes, enums, and by-value modules — so the runtime ships a kernel and its captured environment across a worker seam without the target process importing the defining source. `dumps`/`dump` own the extended write path; `loads`/`load` re-export stdlib `pickle`. It is the worker fabric's kernel-shipping codec: the payload the offload seam serializes and `loky` reuses across a warm pool.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudpickle`
- package: `cloudpickle`
- version: `3.1.2`
- license: BSD-3-Clause
- import: `cloudpickle`
- owner: `runtime`
- rail: kernel and closure serialization
- namespaces: `cloudpickle`, `cloudpickle.cloudpickle`
- capability: extended-pickle serialization of lambdas, closures, module-local and `__main__` functions, dynamically-defined classes and enums, and by-value modules; protocol-5 out-of-band buffer capture via `buffer_callback`; a by-value module registry lifting the by-reference default

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pickler class and protocol anchor
- rail: serialization
- `Pickler` subclasses `_pickle.Pickler` and overrides `reducer_override`, the function/class reducers, and the dispatch table to emit by-value code for the surfaces stdlib pickle rejects; `CloudPickler` is a true alias (identical class object), not a second type. A constructed `Pickler` exposes the inherited `.dump(obj)` write method and a per-instance `dispatch_table` `ChainMap`, the extension point a caller layers custom `type -> reducer` rows onto without subclassing. `DEFAULT_PROTOCOL` equals `pickle.HIGHEST_PROTOCOL` (5), so out-of-band buffers and the newest opcodes are the default write floor.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Pickler`          | pickler class | extended pickler over `_pickle.Pickler`; by-value reducers   |
|  [02]   | `CloudPickler`     | pickler class | true alias of `Pickler` (same class object)                  |
|  [03]   | `DEFAULT_PROTOCOL` | `int`         | write protocol floor, equal to `pickle.HIGHEST_PROTOCOL` (5) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and streamed serialization
- rail: serialization
- `dumps`/`dump` carry the extended write path and both accept `protocol` (default `None` resolves to `DEFAULT_PROTOCOL`) and `buffer_callback` for protocol-5 out-of-band buffers; each protocol-5 buffer surfaces once to the callback as a `pickle.PickleBuffer` and is elided from the main byte stream. `loads`/`load` re-export stdlib `pickle.loads`/`pickle.load` unchanged — the read side owns no cloudpickle logic and takes `buffers=` to reconstruct an out-of-band payload and the stdlib `fix_imports`/`encoding`/`errors` keywords.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `dumps(obj, protocol=None, buffer_callback=None)`      | encode         | serialize to `bytes` on the extended path |
|  [02]   | `dump(obj, file, protocol=None, buffer_callback=None)` | encode stream  | serialize to a writable binary file       |
|  [03]   | `loads(data, /, *, buffers=())`                        | decode         | stdlib `pickle.loads` re-export           |
|  [04]   | `load(file, *, buffers=())`                            | decode stream  | stdlib `pickle.load` re-export            |
|  [05]   | `Pickler(file, protocol=None, buffer_callback=None)`   | build          | construct an extended pickler over a file |

Full read-side signatures re-exported from stdlib `pickle`:
- [03]-[LOADS]: `loads(data, /, *, fix_imports=True, encoding='ASCII', errors='strict', buffers=())`
- [04]-[LOAD]: `load(file, *, fix_imports=True, encoding='ASCII', errors='strict', buffers=())`

[ENTRYPOINT_SCOPE]: by-value module registry
- rail: serialization
- `register_pickle_by_value(module)` takes an imported module object — a name string or any non-module raises `ValueError` — and flips it from the by-reference default to by-value, so every function and class it defines pickles with its full code and target processes deserialize it without importing the source; the module must already sit in `sys.modules` or the call raises `ValueError` a second way. `list_registry_pickle_by_value()` returns the live `set[str]` of registered module names; `unregister_pickle_by_value(module)` restores by-reference.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `register_pickle_by_value(module)`   | registry write | force a module's defs to pickle by value     |
|  [02]   | `unregister_pickle_by_value(module)` | registry write | restore a module to the by-reference default |
|  [03]   | `list_registry_pickle_by_value()`    | registry read  | live `set[str]` of by-value module names     |

## [04]-[IMPLEMENTATION_LAW]

[SERIALIZATION_TOPOLOGY]:
- `dumps`/`dump` extend stdlib pickle over one surface: a lambda, a closure capturing live cell state, a module-local or `__main__` function, a dynamically constructed class or enum, and a by-value module each round-trip where stdlib `pickle.dumps` raises `PicklingError`; an importable module attribute still pickles by reference and re-imports at load.
- by-reference is the default for attributes of an importable module — the payload stores the qualified name and re-imports at load; by-value stores the code itself, so the target process needs neither the module installed nor the same source tree.
- `register_pickle_by_value` is the seam that converts a normally-importable module to by-value for distributed development: worker nodes run the shipped code, never a re-installed package; the registry is a process-global `set[str]` keyed by module `__name__`.
- protocol 5 is the default write floor; `buffer_callback` captures each large buffer as an out-of-band `PickleBuffer` (a numpy array crosses as one `PickleBuffer`, elided from the main stream), and `loads(payload, buffers=[...])` reconstructs it — the zero-copy path for array-heavy kernels.

[DETERMINISM_LAW]:
- a cloudpickle payload couples to the interpreter version and the cloudpickle and library versions of the producing process; the consuming process runs the same versions or the load is undefined.
- cloudpickle is same-version ephemeral transport across a live worker seam, never a durable archive format and never a cross-version wire — a persisted artifact uses a versioned schema codec (`.api/msgspec.md`), never a pickle blob.
- unpickling executes arbitrary code by construction, so a payload is admitted only from a trusted in-process producer; an untrusted byte source never crosses `loads`.

[STACKING]: cloudpickle owns exactly one half of each worker-fabric seam — the by-value reduce of the request-direction callable — and each sibling owner below carries the other half; a fence-writer reads this as the member-to-member wiring, never as five isolated packages.
- `execution/workers`(`runtime/.planning/execution/workers.md`): `Kernel.of` calls `cloudpickle.dumps(fn)` to fill the `payload` bytes field on a pickle-seam kind whose callable a by-name walk loses — a `<lambda>`/`<locals>` qualname or a bound method carrying `__self__` — and the worker-floor `shipped` gate calls `cloudpickle.loads(kernel.payload)` far-side; the two direct call sites key off the `Shipping` `LIVE|REFERENCE|VALUE|GUEST` axis, where `LIVE` carries the callable whole across the seam-free loop and thread arms, `REFERENCE` re-imports by qualified name, `VALUE` ships cloudpickle bytes, and `GUEST` carries a wasm module cloudpickle never touches, so a lambda crosses the process and subinterpreter seams by value instead of degrading to a thread; the `WorkerKind.REMOTE` arm reuses the TERMINAL seal for its SSH crossing — one blob onto the far floor's stdin, one pickled verdict off its stdout — so every `Shipping` form is total across the channel.
- `tblib`(`.api/tblib.md`): two directions of one pickle seam — cloudpickle reduces the request-direction kernel by value, `tblib.pickling_support.install()` reduces the response-direction worker exception with its frames; the workers `fidelity()` one-shot latches tblib before the first pickled hop and re-arms it in every pool initializer, so a `cloudpickle.loads` kernel that raises returns a traceback-bearing fault, never a flattened subprocess marker.
- `msgspec`(`.api/msgspec.md`): `Kernel` is a `msgspec.Struct` and `payload: bytes` holds its cloudpickle content — msgspec frames the crossing envelope, cloudpickle fills the one opaque field, so the payload rides any stdlib-pickle transport as inert bytes until `shipped` re-inflates it. Same division governs durability: msgspec is the cross-version schema wire for a persisted artifact, cloudpickle the same-version ephemeral live-worker transport — never the reverse.
- `loky`(`.api/loky.md`): cloudpickle is loky's default payload codec (`loky.backend.reduction.get_loky_pickler_name()` resolves `cloudpickle`), so the `PROCESS`/`COOPERATIVE` warm reusable pool ships `shipped` submissions carrying closures by value at zero per-call opt-in; loky owns the crash-respawning pool transport, cloudpickle the serialization inside it.
- `pebble`(`.api/pebble.md`): the `PROCESS`/`TERMINAL` pool and the anyio subinterpreter arm pickle through STDLIB pickle, not cloudpickle — the TERMINAL crossing therefore seals its whole payload as one cloudpickle blob, `schedule(sealed_kernel, args=(cloudpickle.dumps((carrier, kernel, args)),))`, so a stdlib pickler carries closures inside opaque bytes and never sees one raw (pebble's `submit(function, timeout, /, *args)` binds its second positional to `timeout`, so the branch drives `schedule` alone).
- `anyio`(`.api/anyio.md`): `to_interpreter.run_sync` is the `execution/lanes` `_ISOLATION` subinterpreter arm's pickle boundary — anyio owns the crossing mechanism, cloudpickle the picklability of the payload it carries, so an ordinary closure crosses only because `Kernel.of` pre-reduced it; the process crossings ride the loky/pebble pools.
- fabric policy: a worker kernel is defined inline as a closure or local function and ships `VALUE` by default; a whole module developed against distributed workers is passed to `register_pickle_by_value` once so its defs cross by code, never by a re-install on every node.

[RAIL_LAW]:
- Package: `cloudpickle`
- Owns: extended-pickle serialization of lambdas, closures, module-local and dynamic callables and classes, by-value modules, and protocol-5 out-of-band buffer capture — the kernel-shipping codec across the worker process and subinterpreter seams
- Accept: `dumps`/`dump` for the extended write path; `loads`/`load` for the stdlib read path with `buffers=`; `register_pickle_by_value` for a module developed against distributed workers; cloudpickle as the `loky` and offload-seam payload codec
- Reject: cloudpickle for durable or cross-version persistence, a payload from an untrusted producer, a wrapper renaming `dumps`/`loads`, a hand-rolled closure serializer, and `CloudPickler` treated as a second type
</content>
