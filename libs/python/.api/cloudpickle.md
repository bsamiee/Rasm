# [PY_BRANCH_API_CLOUDPICKLE]

`cloudpickle` extends stdlib pickle to serialize the callables and types stdlib pickle rejects — lambdas, closures over live cell state, module-local and `__main__` functions, dynamic classes and enums, and by-value modules — shipping a kernel and its captured environment across a worker boundary without the target process importing the defining source. `dumps`/`dump` own the extended write path; `loads`/`load` re-export stdlib `pickle`. It is the worker crossing's kernel-shipping codec: the payload the offload boundary serializes and `loky` reuses across a warm pool.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pickler class and protocol anchor
- concern: serialization
- `Pickler` overrides `reducer_override`, the function/class reducers, and the per-instance `dispatch_table` `ChainMap` — the extension point a caller layers custom `type -> reducer` rows onto without subclassing; `.dump(obj)` inherits from `_pickle.Pickler`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RESULT]                                                     |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Pickler`          | pickler class | extended pickler over `_pickle.Pickler`; by-value reducers   |
|  [02]   | `CloudPickler`     | pickler class | true alias of `Pickler` (same class object)                  |
|  [03]   | `DEFAULT_PROTOCOL` | `int`         | write protocol floor, equal to `pickle.HIGHEST_PROTOCOL` (5) |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and streamed serialization
- concern: serialization
- `dumps`/`dump` carry the extended write path; `protocol=None` resolves to `DEFAULT_PROTOCOL`, and `buffer_callback` surfaces each protocol-5 buffer once as a `pickle.PickleBuffer` elided from the main stream. `loads`/`load` re-export stdlib `pickle` unchanged — the read side owns no cloudpickle logic and takes `buffers=` to reconstruct an out-of-band payload.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RESULT]                                  |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `dumps(obj, protocol=None, buffer_callback=None)`      | encode         | serialize to `bytes` on the extended path |
|  [02]   | `dump(obj, file, protocol=None, buffer_callback=None)` | encode stream  | serialize to a writable binary file       |
|  [03]   | `loads(data, /, *, buffers=())`                        | decode         | stdlib `pickle.loads` re-export           |
|  [04]   | `load(file, *, buffers=())`                            | decode stream  | stdlib `pickle.load` re-export            |
|  [05]   | `Pickler(file, protocol=None, buffer_callback=None)`   | build          | construct an extended pickler over a file |

[ENTRYPOINT_SCOPE]: by-value module registry
- concern: serialization
- `register_pickle_by_value(module)` flips an imported module from the by-reference default to by-value, so its functions and classes pickle with full code and target processes deserialize without importing the source; `unregister_pickle_by_value` restores by-reference and `list_registry_pickle_by_value` reads the live `set[str]` of registered names.
- `register_pickle_by_value`: a name string, a non-module, or a module absent from `sys.modules` raises `ValueError`.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RESULT]                                     |
| :-----: | :----------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `register_pickle_by_value(module)`   | registry write | force a module's defs to pickle by value     |
|  [02]   | `unregister_pickle_by_value(module)` | registry write | restore a module to the by-reference default |
|  [03]   | `list_registry_pickle_by_value()`    | registry read  | live `set[str]` of by-value module names     |

## [03]-[IMPLEMENTATION_LAW]

[SERIALIZATION_TOPOLOGY]:
- `dumps`/`dump` serialize by value what stdlib `pickle.dumps` raises `PicklingError` on — a lambda, a closure over live cell state, a module-local or `__main__` callable, a dynamic class or enum; an importable module attribute still pickles by reference and re-imports at load.
- by-reference stores the qualified name and re-imports at load; by-value stores the code itself, so the target process needs neither the module installed nor the same source tree. `register_pickle_by_value` converts a normally-importable module to by-value for distributed development — worker nodes run the shipped code, never a re-installed package.
- protocol 5 is the default write floor; `buffer_callback` captures each large buffer out-of-band as a `PickleBuffer` (a numpy array crosses as one buffer, elided from the main stream), and `loads(payload, buffers=[...])` reconstructs it — the zero-copy path for array-heavy kernels.

[DETERMINISM_LAW]:
- a payload couples to the interpreter version and the cloudpickle and library versions of the producing process; the consuming process runs the same versions or the load is undefined.
- cloudpickle is same-version ephemeral transport across a live worker boundary, never a durable archive or cross-version wire — a persisted artifact rides a versioned schema codec, cloudpickle the live-worker hop.
- unpickling executes arbitrary code by construction, so a payload crosses `loads` only from a trusted in-process producer.

[STACKING]: cloudpickle owns one half of each worker-crossing boundary — the by-value reduce of the request-direction callable — and each sibling below carries the other half, read as member-to-member wiring.
- `execution/workers`(`runtime/.planning/execution/workers.md`): `Kernel.of` calls `cloudpickle.dumps(fn)` to fill the `payload` bytes on a pickle-boundary kind whose callable a by-name walk loses — a `<lambda>`/`<locals>` qualname or a bound method carrying `__self__` — and the worker-floor `shipped` gate calls `cloudpickle.loads(kernel.payload)` far-side. Both call sites key off the `Shipping` `LIVE|REFERENCE|VALUE|GUEST` axis: `VALUE` ships cloudpickle bytes across the process and subinterpreter boundaries, `LIVE` carries the callable whole on the boundary-free loop/thread arms, `REFERENCE` re-imports by qualified name, `GUEST` carries a wasm module cloudpickle never touches; the `WorkerKind.REMOTE` arm seals one blob onto the far floor's stdin.
- `tblib`(`.api/tblib.md`): two directions of one pickle boundary — cloudpickle reduces the request-direction kernel by value, `tblib.pickling_support.install()` reduces the response-direction worker exception with its frames; the workers `fidelity()` latch arms tblib before the first pickled hop, so a `cloudpickle.loads` kernel that raises returns a traceback-bearing fault.
- `msgspec`(`.api/msgspec.md`): `Kernel` is a `msgspec.Struct` and `payload: bytes` holds its cloudpickle content — msgspec frames the crossing envelope, cloudpickle fills the one opaque field. Same division governs durability: msgspec is the cross-version schema wire for a persisted artifact, cloudpickle the same-version live-worker transport.
- `loky`(`.api/loky.md`): cloudpickle is loky's default payload codec (`loky.backend.reduction.get_loky_pickler_name()` resolves `cloudpickle`), so the warm reusable pool ships `shipped` submissions carrying closures by value at zero per-call opt-in; loky owns the crash-respawning pool transport, cloudpickle the serialization inside it.
- `pebble`(`.api/pebble.md`): the `PROCESS`/`TERMINAL` pool and the anyio subinterpreter arm pickle through STDLIB pickle, so the TERMINAL crossing seals its whole payload as one cloudpickle blob — `schedule(sealed_kernel, args=(cloudpickle.dumps((carrier, kernel, args)),))` — and a stdlib pickler carries closures inside opaque bytes, never one raw.
- `anyio`(`.api/anyio.md`): `to_interpreter.run_sync` is the `execution/lanes` subinterpreter pickle boundary — anyio owns the crossing, cloudpickle the picklability of the payload, so an ordinary closure crosses only because `Kernel.of` pre-reduced it.

[LOCAL_ADMISSION]:
- a worker kernel defined inline as a closure or local function ships `VALUE` by default; a whole module developed against distributed workers registers once through `register_pickle_by_value` so its defs cross by code, never a re-install per node.
- direct `cloudpickle.dumps`/`loads` at the `Kernel.of`/`shipped` boundary owns the reduce; the repo consumes the first-class package, never loky's vendored copy or a renaming wrapper.
