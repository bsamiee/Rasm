# [PY_RUNTIME_API_WASMTIME]

`wasmtime` embeds the Wasmtime runtime behind a ctypes-loaded shared library; runtime's `WorkerKind.WASM` guest sandbox consumes one slice (`workers.md`). One epoch-armed `Engine` per interpreter compiles each module once; every call instantiates fresh against a limit-bounded `Store`, exchanging bytes over the `GUEST_ABI` exports with zero imports. Epoch deadline is the kill mechanism: a daemon pacer increments the engine-global epoch against each store's relative tick budget, so a guest dies mid-kernel at wall clock. Fuel metering, WASI, and the component model sit outside runtime scope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `wasmtime`
- package: `wasmtime` (`Apache-2.0 WITH LLVM-exception`)
- module: `wasmtime`
- rail: worker fabric
- namespaces: `wasmtime`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine, store, module, instance
- every store-scoped read or call takes the `Store` as its first argument.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :--------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Config`   | build config  | engine construction policy; `epoch_interruption` armed here  |
|  [02]   | `Engine`   | runtime       | compilation + epoch owner, one per interpreter               |
|  [03]   | `Store`    | call scope    | per-call isolation unit carrying limits and the epoch budget |
|  [04]   | `Module`   | compiled unit | compiled wasm, cached per module bytes                       |
|  [05]   | `Instance` | guest         | zero-import instantiation; `exports(store)` string-indexes   |
|  [06]   | `Func`     | export        | direct call `func(store, *params)`, packed `i64` reply       |
|  [07]   | `Memory`   | export        | linear memory; `read`/`write` byte exchange with the guest   |

[PUBLIC_TYPE_SCOPE]: fault family
- `WasmtimeError` is the one raise across the call and instantiate paths — a guest trap, an epoch-deadline trip, and an instantiation failure all cross as it, carrying no addressable trap code; the separate `Trap.trap_code` never propagates from a call.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :-------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `WasmtimeError` | fault base    | guest trap, epoch trip, and instantiation failure carrier |
|  [02]   | `ExitTrap`      | fault         | WASI `proc_exit` subclass carrying `.code`; unconsumed    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine, compilation, and per-call isolation
- `Config` setters are write-only; `set_epoch_deadline` takes ticks relative to the engine's current epoch, so concurrent stores under one heartbeat stay isolated, and `set_limits` negative arguments leave a bound unset.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Config()` + `config.epoch_interruption = True`                         | ctor     | arm the epoch kill at engine construction     |
|  [02]   | `Engine(config)` / `engine.increment_epoch()`                           | ctor     | one engine per interpreter; pacer heartbeat   |
|  [03]   | `Module(engine, wasm)` / `Module.validate(engine, wasm)`                | ctor     | compile once per module bytes                 |
|  [04]   | `Store(engine)`                                                         | ctor     | fresh isolation unit per call                 |
|  [05]   | `store.set_limits(memory_size=, table_elements=)`                       | instance | linear-memory ceiling in bytes                |
|  [06]   | `store.set_epoch_deadline(ticks_after_current)`                         | instance | relative tick budget under the shared pacer   |
|  [07]   | `Instance(store, module, [])` / `instance.exports(store)[name]`         | ctor     | zero-import guest; string-indexed exports     |
|  [08]   | `func(store, *params)`                                                  | instance | direct export call; single value or list back |
|  [09]   | `memory.write(store, value, start)` / `memory.read(store, start, stop)` | instance | request in, reply out as bytes                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- engine law: one epoch-armed `Engine` per interpreter; the daemon pacer increments the engine-global epoch every `EPOCH_TICK` while per-store deadlines stay relative ticks, so one heartbeat serves every concurrent guest and none kills another.
- compile law: `Module(engine, payload)` compiles once per module bytes and instantiation stays per call, so the per-call cost is instantiation alone and guest state never leaks across kernels.
- isolation law: `Instance(store, module, [])` constructs with zero imports, so a guest owns no ambient capability by construction; `store.set_limits(memory_size=GUEST_MEMORY)` bounds linear memory and refuses growth past the ceiling.
- abi law: request and reply cross as bytes over the `GUEST_ABI` exports — `alloc(store, len)` reserves guest memory, `memory.write` lands the request, `run(store, ptr, len)` returns one packed `i64` (`(ptr << 32) | len`), and `memory.read` copies the reply out before the store drops.
- scope law: fuel metering (`consume_fuel`/`set_fuel`), WASI (`WasiConfig`/`Linker.define_wasi`), the component model, `SharedMemory`, serialization (`Module.serialize`/`deserialize`), and the cranelift tuning surface stay unconsumed.

[STACKING]:
- `execution/workers`(`runtime/.planning/execution/workers.md`): the primary owner — `Shipping.GUEST` carries the wasm module as `Kernel.payload` under a digest-label `name`, `_guest` resolves it, and the `KernelTrait.SANDBOXED` row maps onto `WorkerKind.WASM` with `retry=Nothing` (a trap is deterministic, never retried); `KIND_POLICY[WASM]` reads `fidelity=False`, so the guest crosses no pickle seam.
- `execution/lanes`(`runtime/.planning/execution/lanes.md`): the `_ISOLATION` WASM row rides `anyio.to_thread.run_sync` under `THREAD_BAND`, and the guest's epoch deadline is its own in-process kill, so a `SANDBOXED` kernel rides the thread arm and never re-routes to the pebble pool for wall-clock enforcement.
- `reliability/faults`(`runtime/.planning/reliability/faults.md`): the epoch kill's `TimeoutError` lands the `deadline` `CLASSIFY` row and a guest trap's `WasmtimeError` falls to the catch-all `boundary` case keeping the trap message, so no wasmtime-specific row is owed.

[LOCAL_ADMISSION]:
- wasmtime is the branch's sole guest-sandbox owner; a second WASM runtime, a subprocess-per-guest scheme, or an `exec`-based plugin sandbox is the deleted form.
- guest modules reach the fabric only as `Kernel.of(wasm_bytes)`; no page constructs an `Engine`, `Store`, or `Instance` beside the workers arm.

[RAIL_LAW]:
- Package: `wasmtime`
- Owns: in-process WASM guest execution — the epoch-armed engine, per-call store isolation with memory limits, once-per-bytes compilation, zero-import instantiation, and the byte-exchange ABI over exports
- Accept: `Kernel.of(wasm_bytes)` as the sole ingress, the `GUEST_ABI` export triple, `set_epoch_deadline` relative ticks under the one pacer, `set_limits(memory_size=)` ceilings, `except WasmtimeError` with elapsed-budget discrimination
- Reject: WASI or host imports on a guest, fuel metering beside the epoch kill, a per-call `Engine` or `Module` compile, `Trap` caught where `WasmtimeError` is the raise, a second sandbox runtime, and guest state carried across calls through a reused `Store`
