# [PY_RUNTIME_API_WASMTIME]

`wasmtime` embeds the Wasmtime WebAssembly runtime behind a ctypes-loaded shared library; runtime consumes one slice of it as the `WorkerKind.WASM` guest sandbox (`workers.md` `_guest`/`_guest_engine`/`_guest_module`). One epoch-armed `Engine` per interpreter compiles each module once, every call instantiates fresh against a limit-bounded `Store`, and the guest exchanges bytes over the `GUEST_ABI` exports with zero imports — no WASI, no ambient capability. The epoch deadline is the consumed kill mechanism: a daemon pacer increments the engine-global epoch while each store carries a relative tick budget, so a guest dies mid-kernel in-process at wall clock. Fuel metering, WASI, the component model, `SharedMemory`, and the compilation-tuning surface are unconsumed and out of runtime scope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `wasmtime`
- package: `wasmtime`
- import: `wasmtime`
- owner: `runtime`
- rail: worker fabric
- version: `46.0.1`
- license: Apache-2.0 WITH LLVM-exception
- namespaces: `wasmtime`
- capability: in-process WASM guest execution — epoch-interruptible `Engine`/`Store`, per-store linear-memory and instance limits, once-per-bytes `Module` compilation, zero-import `Instance` construction, string-indexed exports, direct `Func` calls, and `Memory.read`/`write` byte exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine, store, module, instance
- rail: worker fabric
- every store-scoped read or call takes the `Store` as its first argument; `Config` setters are write-only properties.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :--------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Config`   | build config  | engine construction policy; `epoch_interruption` armed here  |
|  [02]   | `Engine`   | runtime       | compilation + epoch owner, one per interpreter               |
|  [03]   | `Store`    | call scope    | per-call isolation unit carrying limits and the epoch budget |
|  [04]   | `Module`   | compiled unit | compiled wasm, cached per module bytes                       |
|  [05]   | `Instance` | guest         | zero-import instantiation; `exports(store)` string-indexes   |
|  [06]   | `Func`     | export        | direct call `func(store, *params)`, packed `i64` reply       |
|  [07]   | `Memory`   | export        | linear memory; `read`/`write` byte exchange with the guest   |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: worker fabric
- `WasmtimeError` is the ONE raise the call and instantiate paths surface — a guest trap, an epoch-deadline trip, and an instantiation failure all cross as it, and no addressable trap code rides it (the separate `Trap` class with `trap_code` never propagates from a call), so the workers arm discriminates the epoch kill by elapsed budget, never by code.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :-------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `WasmtimeError` | fault base    | guest trap, epoch trip, and instantiation failure carrier |
|  [02]   | `ExitTrap`      | fault         | WASI `proc_exit` subclass carrying `.code`; unconsumed    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine, compilation, and per-call isolation
- rail: worker fabric
- `Config` setters are write-only; `set_epoch_deadline` takes ticks RELATIVE to the engine's current epoch, so concurrent stores under one heartbeat stay isolated; `set_limits` negative arguments leave a bound unset.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :--------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `Config()` + `config.epoch_interruption = True`                        | build          | arm the epoch kill at engine construction     |
|  [02]   | `Engine(config)` / `engine.increment_epoch()`                          | build / pace   | one engine per interpreter; pacer heartbeat   |
|  [03]   | `Module(engine, wasm_bytes)` / `Module.validate(engine, wasm_bytes)`   | compile        | compile once per module bytes                 |
|  [04]   | `Store(engine)`                                                        | call scope     | fresh isolation unit per call                 |
|  [05]   | `store.set_limits(memory_size=, table_elements=)`                      | bound          | linear-memory ceiling in bytes                |
|  [06]   | `store.set_epoch_deadline(ticks_after_current)`                        | deadline       | relative tick budget under the shared pacer   |
|  [07]   | `Instance(store, module, [])` / `instance.exports(store)[name]`        | instantiate    | zero-import guest; string-indexed exports     |
|  [08]   | `func(store, *params)`                                                 | exec           | direct export call; single value or list back |
|  [09]   | `memory.write(store, data, start)` / `memory.read(store, start, stop)` | exec io        | request in, reply out as bytes                |

## [04]-[IMPLEMENTATION_LAW]

[SANDBOX_TOPOLOGY]:
- engine law: one epoch-armed `Engine` per interpreter (`workers.md` `_guest_engine`, `@cache`); the daemon pacer increments the engine-global epoch every `EPOCH_TICK`, and per-store deadlines are relative ticks, so one heartbeat serves every concurrent guest and none kills another.
- compile law: `Module(engine, payload)` compiles once per module bytes (`_guest_module`, `@cache`); instantiation stays per call, so the per-call cost is instantiation alone and guest state never leaks across kernels.
- isolation law: `Instance(store, module, [])` constructs with ZERO imports — no WASI, no host functions — so a guest owns no ambient capability by construction; `store.set_limits(memory_size=GUEST_MEMORY)` bounds linear memory and refuses growth past the ceiling.
- abi law: request and reply cross as bytes over the `GUEST_ABI` exports — `alloc(store, len)` reserves guest memory, `memory.write` lands the request, `run(store, ptr, len)` returns one packed `i64` (`(ptr << 32) | len`), and `memory.read` copies the reply out before the store drops.
- fault law: `WasmtimeError` is the one raise and carries no addressable trap code, so the workers arm discriminates by elapsed budget — an epoch kill re-raises `TimeoutError` onto the faults `deadline` row, a genuine trap crosses whole into the catch-all `boundary` case with its trap message.
- scope law: fuel metering (`consume_fuel`/`set_fuel`), WASI (`WasiConfig`/`Linker.define_wasi`), the component model, `SharedMemory`, serialization (`Module.serialize`/`deserialize`), and the cranelift tuning surface are UNCONSUMED in runtime; any future need is a live fence, not a speculative re-catalog.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): the PRIMARY owner. `Shipping.GUEST` carries the wasm module as `Kernel.payload` with a digest-label `name`; `shipped` resolves it through `_guest`, and `KernelTrait.SANDBOXED` maps onto `WorkerKind.WASM` with `retry=Nothing` — a trap is deterministic and never retried. `KIND_POLICY[WASM]` reads `fidelity=False`: the guest crosses no pickle seam.
- `execution/lanes`(`runtime/.planning/execution/lanes.md`): the `_ISOLATION` WASM row rides `anyio.to_thread.run_sync` under `THREAD_BAND` — the guest arm's epoch deadline is its own in-process kill, so a `SANDBOXED` kernel never re-routes to the pebble arm for wall-clock enforcement.
- `reliability/faults`(`runtime/.planning/reliability/faults.md`): the epoch kill's `TimeoutError` lands the `deadline` `CLASSIFY` row; a guest trap's `WasmtimeError` falls to the catch-all `boundary` case keeping the trap message — no wasmtime row exists because the default classification is the honest landing.
- `pebble`(`.api/pebble.md`): a `TERMINAL` `SANDBOXED` kernel still routes to the pebble pool like every kernel — the sealed blob carries the guest bytes, `_guest` runs on the far worker, and the process kill backs the epoch kill; the two enforcement substrates compose rather than compete.

[LOCAL_ADMISSION]:
- wasmtime is the branch's one guest-sandbox owner; a second WASM runtime, a subprocess-per-guest scheme, or an `exec`-based plugin sandbox is the deleted form.
- guest modules reach the fabric only as `Kernel.of(wasm_bytes)` — no page constructs an `Engine`, `Store`, or `Instance` beside the workers arm.

[RAIL_LAW]:
- Package: `wasmtime`
- Owns: in-process WASM guest execution — the epoch-armed engine, per-call store isolation with memory limits, once-per-bytes compilation, zero-import instantiation, and the byte-exchange ABI over exports
- Accept: `Kernel.of(wasm_bytes)` as the sole ingress, the `GUEST_ABI` export triple, `set_epoch_deadline` relative ticks under the one pacer, `set_limits(memory_size=)` ceilings, `except WasmtimeError` with elapsed-budget discrimination
- Reject: WASI or host imports on a guest, fuel metering beside the epoch kill, a per-call `Engine` or per-call `Module` compile, `Trap` caught where `WasmtimeError` is the raise, a second sandbox runtime, and guest state carried across calls through a reused `Store`
