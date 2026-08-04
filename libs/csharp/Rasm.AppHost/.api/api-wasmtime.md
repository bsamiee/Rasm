# [RASM_APPHOST_API_WASMTIME]

`Wasmtime` binds the native WebAssembly runtime (`libwasmtime`) as the sandbox rail's plugin-isolation core: an `Engine` compiles a `Module`, a `Store` executes it under fuel, epoch, and memory limits, and a `Linker` resolves the capability-scoped import table. Guests reach host authority only through the linker's granted imports over WASI-Preview-1, and the managed surface stops at the core module — the component model is a native engine flag with no managed type behind it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wasmtime`
- package: `Wasmtime`
- assembly: `Wasmtime.Dotnet`
- namespace: `Wasmtime`
- asset: runtime library + native `libwasmtime` (`runtimes/<rid>/native/`)
- rail: sandbox

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime hierarchy

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Engine`                   | class          | root runtime engine and JIT/AOT compiler owner           |
|  [02]   | `Config`                   | class          | engine configuration builder                             |
|  [03]   | `Store`                    | class          | execution store owning state and fuel                    |
|  [04]   | `Module`                   | class          | compiled WebAssembly module                              |
|  [05]   | `Linker`                   | class          | import resolver and module instantiator                  |
|  [06]   | `Instance`                 | class          | one live module instance in a `Store`                    |
|  [07]   | `Memory`                   | class          | linear memory access over `IExternal`                    |
|  [08]   | `Global`                   | class          | WebAssembly global variable                              |
|  [09]   | `Function`                 | class          | callable WebAssembly function                            |
|  [10]   | `Table`                    | class          | function/externref element table                         |
|  [11]   | `Caller`                   | readonly ref   | host-callback context inside a call frame                |
|  [12]   | `WasiConfiguration`        | class          | WASI preview-1 environment configuration                 |
|  [13]   | `WasiDirectoryPermissions` | `[Flags]` enum | `Read=1`, `Write=2` on a preopened directory             |
|  [14]   | `WasiFilePermissions`      | `[Flags]` enum | `Read=1`, `Write=2` on files under a preopened directory |
|  [15]   | `ValueBox`                 | struct         | untyped argument/result cell on an untyped callback      |

[PUBLIC_TYPE_SCOPE]: value and type system

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :----------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `ValueKind`  | enum          | `Int32`, `Int64`, `Float32`, `Float64`, `V128`, `FuncRef`, `ExternRef`, `AnyRef` |
|  [02]   | `Mutability` | struct        | `Immutable` or `Mutable` global mutability                                       |
|  [03]   | `Import`     | class         | one module import descriptor                                                     |
|  [04]   | `Export`     | class         | one module export descriptor                                                     |
|  [05]   | `TableKind`  | enum          | `FuncRef` or `ExternRef` element kind                                            |

[PUBLIC_TYPE_SCOPE]: compiler and profiling

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------ | :------------ | :------------------------------------ |
|  [01]   | `CompilerStrategy`  | enum          | `Auto` or `Cranelift`                 |
|  [02]   | `OptimizationLevel` | enum          | `None`, `Speed`, `SpeedAndSize`       |
|  [03]   | `ProfilingStrategy` | enum          | `None`, `JitDump`, `VTune`, `PerfMap` |

[PUBLIC_TYPE_SCOPE]: error handling

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------ | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `TrapException`     | class         | WebAssembly trap, extends `WasmtimeException`; `Type` carries the code |
|  [02]   | `WasmtimeException` | class         | base exception for Wasmtime failures                                   |
|  [03]   | `TrapCode`          | enum          | trap cause taxonomy, `Undefined = -1` and dense from `StackOverflow`   |
|  [04]   | `TrapFrame`         | class         | one stack frame from a trap backtrace                                  |

- `TrapException.Type -> TrapCode` is the enforcement discriminant a host reads: `Interrupt` is an epoch preemption, `OutOfFuel` a fuel exhaustion, and every other row a guest-authored trap or a runtime defect.
- `TrapCode` numeric values are NOT stable across releases — the Rust core inserts rows — so a persisted or wire-carried code travels as its NAME.
- `CannotEnterComponent`, `CannotLeaveComponent`, and `NoAsyncResult` surface component-model traps from the Rust core; no managed surface can reach the code that raises them.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine and configuration construction

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `Engine()`                                        | ctor     | default engine with AOT compilation      |
|  [02]   | `Engine(Config)`                                  | ctor     | engine from explicit configuration       |
|  [03]   | `Engine.IncrementEpoch()`                         | instance | advances the epoch interruption counter  |
|  [04]   | `Engine.IsPulleyInterpreter`                      | property | true when running the Pulley interpreter |
|  [05]   | `Config()`                                        | ctor     | default configuration                    |
|  [06]   | `Config.WithDebugInfo(bool)`                      | instance | enables DWARF debug info in code         |
|  [07]   | `Config.WithEpochInterruption(bool)`              | instance | enables epoch-based interruption         |
|  [08]   | `Config.WithFuelConsumption(bool)`                | instance | enables fuel-metered execution           |
|  [09]   | `Config.WithCompilerStrategy(CompilerStrategy)`   | instance | selects the compiler strategy            |
|  [10]   | `Config.WithOptimizationLevel(OptimizationLevel)` | instance | controls Cranelift optimization level    |
|  [11]   | `Config.WithMaximumStackSize(int)`                | instance | bounds wasm stack depth                  |
|  [12]   | `Config.WithParallelCompilation(bool)`            | instance | parallelizes module compilation          |
|  [13]   | `Config.WithCraneliftNaNCanonicalization(bool)`   | instance | canonicalizes NaN payloads for replay    |
|  [14]   | `Config.WithComponentModel(bool)`                 | instance | toggles NATIVE component support only    |

- `WithComponentModel` reaches `wasmtime_config_wasm_component_model_set` in the native engine and NOTHING in managed code can then compile or instantiate a component: `Wasmtime.Dotnet` 44.0.0 (the current release) exposes zero type carrying `Component` in its name, and `Module`/`Linker`/`Instance` are core-module only. WASI Preview 2 is therefore unreachable from this binding, and a component host is a native-side embedding rather than a `Wasmtime.NET` row — enabling the flag buys nothing a managed caller can spend.
- `WithFuelConsumption` and `WithEpochInterruption` are CONSTRUCTION-time only; neither is settable after `Engine(Config)`, so a store's `Fuel` setter and `SetEpochDeadline` are inert on an engine that did not arm them.

[ENTRYPOINT_SCOPE]: module compilation

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Module.FromBytes(Engine, string name, ReadOnlySpan<byte>)`   | factory  | compiles from binary wasm bytes         |
|  [02]   | `Module.FromFile(Engine, string path)`                        | factory  | compiles from `.wasm` file              |
|  [03]   | `Module.FromStream(Engine, string name, Stream)`              | factory  | compiles from stream                    |
|  [04]   | `Module.FromText(Engine, string name, string text)`           | factory  | compiles from WAT text                  |
|  [05]   | `Module.FromTextFile(Engine, string path)`                    | factory  | compiles from `.wat` file               |
|  [06]   | `Module.FromTextStream(Engine, string name, Stream)`          | factory  | compiles from WAT stream                |
|  [07]   | `Module.Validate(Engine, ReadOnlySpan<byte>)`                 | static   | validates bytes without compiling       |
|  [08]   | `Module.ConvertText(string wat) -> byte[]`                    | static   | WAT to binary wasm, no compile          |
|  [09]   | `Module.Serialize() -> byte[]`                                | instance | the compiled artifact as bytes          |
|  [10]   | `Module.Deserialize(Engine, string name, ReadOnlySpan<byte>)` | factory  | loads a serialized artifact             |
|  [11]   | `Module.DeserializeFile(Engine, string name, string path)`    | factory  | loads a serialized artifact file        |
|  [12]   | `Module.Name`                                                 | property | module name used at compilation         |
|  [13]   | `Module.Imports`                                              | property | `IReadOnlyList<Import>` descriptor list |
|  [14]   | `Module.Exports`                                              | property | `IReadOnlyList<Export>` descriptor list |

- `Wasmtime.Module` collides with `System.Reflection.Module`, which `ImplicitUsings` pulls in through `using System.Reflection` — any fence naming both needs `using WasmModule = Wasmtime.Module;` or a fully-qualified spelling, and an unqualified `Module.FromText(...)` in such a file does not compile.
- `Serialize`/`Deserialize` skip compilation on a warm start; the artifact is engine-configuration- and version-bound, so a cached blob re-verifies against the current `Engine` and a mismatch faults rather than loading.

[ENTRYPOINT_SCOPE]: store and instantiation

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Store(Engine)`                                     | ctor     | store with no host data               |
|  [02]   | `Store(Engine, object? data)`                       | ctor     | store with arbitrary host data        |
|  [03]   | `Store.Fuel`                                        | property | remaining fuel, set or read           |
|  [04]   | `Store.SetLimits(memorySize?, tableElements?, ...)` | instance | bounds memory, table, instance counts |
|  [05]   | `Store.SetWasiConfiguration(WasiConfiguration)`     | instance | attaches WASI environment             |
|  [06]   | `Store.SetEpochDeadline(ulong ticksBeyondCurrent)`  | instance | sets interruption deadline in epochs  |
|  [07]   | `Store.GetData()` / `SetData(object?)`              | instance | host data object on the store         |
|  [08]   | `Store.GC()`                                        | instance | runs store garbage collection         |
|  [09]   | `Linker(Engine)`                                    | ctor     | creates linker for the given engine   |
|  [10]   | `Linker.AllowShadowing`                             | property | permits import re-definition          |
|  [11]   | `Linker.DefineWasi()`                               | instance | adds WASI preview-1 imports           |
|  [12]   | `Linker.Define(module, name, Function)`             | instance | registers a host function import      |
|  [13]   | `Linker.Define(module, name, Memory)`               | instance | registers a memory import             |
|  [14]   | `Linker.Instantiate(Store, Module)`                 | instance | links and instantiates module         |
|  [15]   | `Linker.GetFunction(Store, module, name)`           | instance | retrieves a named function export     |
|  [16]   | `Linker.GetMemory(Store, module, name)`             | instance | retrieves a named memory export       |
|  [17]   | `Linker.DefineInstance(Store, name, Instance)`      | instance | registers a whole instance's exports  |
|  [18]   | `Linker.DefineModule(Store, Module)`                | instance | registers a module's exports by name  |

[ENTRYPOINT_SCOPE]: host-function binding — the one import-table seat

Untyped surfaces trail `(… , IReadOnlyList<ValueKind> parameterKinds, IReadOnlyList<ValueKind> resultKinds)`, and
`UntypedCallbackDelegate` is `void (Caller, ReadOnlySpan<ValueBox> arguments, Span<ValueBox> results)`.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Function.UntypedCallbackDelegate`                                   | delegate | the untyped host-callback shape    |
|  [02]   | `Linker.DefineFunction(module, name, UntypedCallbackDelegate, …)`    | instance | one untyped import, dynamic arity  |
|  [03]   | `Linker.DefineFunction<…>(module, name, Action/Func/CallerFunc<…>)`  | instance | typed family, up to 12 parameters  |
|  [04]   | `Function.FromCallback(Store, UntypedCallbackDelegate, …)`           | factory  | mints a `Function` for `Define`    |
|  [05]   | `Function.Null`                                                      | property | the null function reference        |

- `DefineFunction` is the seat a capability-scoped import table folds onto: one call per granted descriptor, so the linkage IS the grant set and an ungranted capability is structurally absent rather than refused at runtime.
- The untyped overload is the row a dynamic grant set needs — parameter and result kinds arrive as data, so a new granted capability adds a row rather than a generic arity.
- `CallerAction<…>`/`CallerFunc<…>` are the frame-aware delegate families (`void CallerAction(Caller)`, `TResult CallerFunc<out TResult>(Caller)`), each generic over up to 12 parameters, and the `Func` arms carry up to four results as a `ValueTuple` — a host function reading guest memory takes a `CallerFunc` arm rather than closing over the `Store`.

[ENTRYPOINT_SCOPE]: call-frame reads — host callbacks reach guest state through `Caller` only

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Caller.Store`                                                     | property | the store owning this frame                 |
|  [02]   | `Caller.Fuel`                                                      | property | remaining fuel, readable and settable       |
|  [03]   | `Caller.GetMemory(string name)`                                    | instance | named memory export (`Memory?`)             |
|  [04]   | `Caller.GetFunction(string name)`                                  | instance | named function export (`Function?`)         |
|  [05]   | `Caller.TryGetMemorySpan<T>(name, address, length, out Span<T>)`   | instance | one-call bounded window, `T : unmanaged`    |
|  [06]   | `Caller.GetData()` / `SetData(object?)`                            | instance | store host-data access from the frame       |
|  [07]   | `Memory.GetSpan(long address, int length)`                         | instance | bounded `Span<byte>` window                 |
|  [08]   | `Memory.GetSpan<T>(long address, int length)`                      | instance | bounded typed window, `T : unmanaged`       |
|  [09]   | `Memory.Read<T>(long)` / `Write<T>(long, T)`                       | instance | single unmanaged value at an address        |
|  [10]   | `Memory.ReadString(address, length, Encoding?)`                    | instance | decoded string over a bounded window        |
|  [11]   | `Memory.ReadNullTerminatedString(long address)`                    | instance | decoded NUL-terminated guest string         |
|  [12]   | `Memory.WriteString(address, string, Encoding?)`                   | instance | encodes into guest memory, returns bytes    |
|  [13]   | `Memory.GetLength()` / `GetSize()` / `PageSize`                    | instance | byte length, page count, `65536` page size  |

- `Caller` is a `readonly ref struct`: it cannot outlive the frame, which is exactly why a `Store` captured in a callback closure is the deleted form — the closure survives the call and the store's context does not.
- `TryGetMemorySpan<T>` collapses the `GetMemory` + `GetSpan` pair into one bounded, null-safe read and is the preferred form when the export name is known.

[ENTRYPOINT_SCOPE]: instance and typed invocation

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Instance(Store, Module, params object[] imports)` | ctor     | instantiates module with explicit imports |
|  [02]   | `Instance.GetAction(string name)`                  | instance | `Action` export with no parameters        |
|  [03]   | `Instance.GetAction<TA>(string name)`              | instance | `Action<TA>` export                       |
|  [04]   | `Instance.GetFunction<TR>(string name)`            | instance | `Func<TR?>` export                        |
|  [05]   | `Instance.GetFunction<TA, TR>(string name)`        | instance | `Func<TA, TR?>` export                    |

[ENTRYPOINT_SCOPE]: WASI configuration

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `WithArg(string)`                                                                  | instance | appends one command-line argument    |
|  [02]   | `WithArgs(IEnumerable<string>)` / `(params string[])` / `(ReadOnlySpan<string>)`   | instance | appends multiple arguments           |
|  [03]   | `WithInheritedArgs()`                                                              | instance | inherits host process arguments      |
|  [04]   | `WithEnvironmentVariable(name, value)`                                             | instance | sets one environment variable        |
|  [05]   | `WithEnvironmentVariables(IEnumerable<(string, string)>)`                          | instance | sets a whole environment set         |
|  [06]   | `WithInheritedEnvironment()`                                                       | instance | inherits host environment            |
|  [07]   | `WithPreopenedDirectory(path, guestPath, dirPerms, filePerms)`                     | instance | mounts a host path at a guest path   |
|  [08]   | `WithStandardInput(path)` / `WithStandardOutput(path)` / `WithStandardError(path)` | instance | redirects one stdio stream to a file |
|  [09]   | `WithInheritedStandardInput()`                                                     | instance | passes host stdin through            |
|  [10]   | `WithInheritedStandardOutput()`                                                    | instance | passes host stdout through           |
|  [11]   | `WithInheritedStandardError()`                                                     | instance | passes host stderr through           |

- `WithPreopenedDirectory` has ONE arity — `dirPerms` is `WasiDirectoryPermissions` and `filePerms` is `WasiFilePermissions`, both required — so a mount always states its read/write posture and no overload silently grants more than a caller asked for.
- Every `With*` returns the receiver, so a `GrantScope` folds onto one configuration without an intermediate builder.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- execution hierarchy: `Engine` compiles a `Module` and roots a `Store`, a `Linker` resolves imports and `Instantiate` yields an `Instance`.
- native binding: every public type is a safe-handle wrapper over P/Invoke into `libwasmtime` (`runtimes/<rid>/native/`).
- dispose discipline: `Engine`, `Config`, `Store`, `Module`, `Linker` implement `IDisposable`, and dispose follows the hierarchy — `Store` before `Engine`.
- fluent config: every `Config.With*` and `WasiConfiguration.With*` returns the receiver, so configuration chains.
- typed exports: `Instance.GetAction<...>` and `Instance.GetFunction<...>` return null for an absent export, so a caller checks null before invoking.
- fuel metering: `Config.WithFuelConsumption(true)` arms it, the `Store.Fuel` setter adds fuel, and exhaustion raises `TrapException` with `TrapCode.OutOfFuel`.
- epoch interruption: `Config.WithEpochInterruption(true)` arms it, `Engine.IncrementEpoch()` advances the counter, and `Store.SetEpochDeadline(ticks)` sets the cutoff — the deadline counts ENGINE EPOCHS, never wall time, so a host that never increments arms a deadline that never arrives.
- preemption reach: epoch interruption is the ONLY mechanism that reaches a guest inside a host-free loop. `Store.Dispose()` releases a `SafeHandle` and cannot release while a native call is in flight on that store, and `Store.SetLimits` caps what a store may ACQUIRE — neither converges a spinning guest, so neither substitutes for an epoch increment.
- component model: `Config.WithComponentModel(bool)` toggles the native engine only; the managed surface has no component type, so WASI Preview 2 is out of reach at 44.0.0.
- WASI admission: `Linker.DefineWasi()` and `Store.SetWasiConfiguration(WasiConfiguration)` both precede instantiation, and `WithPreopenedDirectory` scopes filesystem access to named prefixes under explicit directory and file permission flags.

[STACKING]:
- within-lib fold: `Engine` → `Module` + `Store` → `Linker.Instantiate` → `Instance`; typed exports invoke through `Instance.GetFunction<...>`/`GetAction<...>`, and `Config` → `Store` threads fuel and epoch state.
- `SandboxIsolation.WasmModule` (AppHost sandbox owner): one `Linker.DefineFunction` row lands per granted `CapabilityDescriptor` so the import table IS the grant scope, `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors a `WasiConfiguration` pre-open set scopes, `Config.WithFuelConsumption`/`Store.Fuel` meters CPU, `Store.SetLimits` caps linear memory, and `Config.WithEpochInterruption` + `Store.SetEpochDeadline` + `Engine.IncrementEpoch` carry the wall budget and the kill rail.
- `TrapException.Type` is that owner's kill witness: `Interrupt` proves an epoch kill converged and `OutOfFuel` a CPU breach, both projecting onto one quota fault while the raw code rides the eviction receipt.

[LOCAL_ADMISSION]:
- Sandbox modules enter through `Module.FromBytes` or `Module.FromFile`; the `FromText` WAT forms are development paths.
- Host callbacks reach the call store through `Caller`, never a captured `Store` in a closure.
- A dynamic grant set binds through the untyped `DefineFunction` row; the typed overload family serves fixed-arity host functions only.

[RAIL_LAW]:
- Package: `Wasmtime`
- Owns: WebAssembly compilation, store-scoped execution, WASI-Preview-1 environment, linear memory access, host function binding, fuel and epoch preemption.
- Accept: compiled core modules, WASI-configured stores, typed export invocation, untyped grant-scoped imports, epoch-converged eviction.
- Reject: direct P/Invoke against `libwasmtime`, captured-store closures in host callbacks, `Instance` construction without a `Linker` for import-bearing modules, a WASI-Preview-2 or component-model claim on the managed surface, and a wall guarantee resting on disposal or `SetLimits`.
