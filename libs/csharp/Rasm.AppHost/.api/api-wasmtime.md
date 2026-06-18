# [RASM_APPHOST_API_WASMTIME]

`Wasmtime` supplies a P/Invoke binding over the Wasmtime native runtime (`libwasmtime`), exposing engine configuration, module compilation, store lifecycle, instance execution, WASI configuration, linear memory access, function and global definitions, and a linker that resolves imports across instances.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wasmtime`
- package: `Wasmtime`
- assembly: `Wasmtime.Dotnet`
- namespace: `Wasmtime`
- asset: runtime library + native `libwasmtime` (`runtimes/<rid>/native/`)
- rail: sandbox

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime hierarchy — `Wasmtime`
- rail: sandbox

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- |
|   [1]   | `Engine`            | disposable class | root runtime engine; owns the JIT/AOT compiler     |
|   [2]   | `Config`            | disposable class | engine configuration; fluent builder pattern       |
|   [3]   | `Store`             | disposable class | execution store; owns state and fuel               |
|   [4]   | `Module`            | disposable class | compiled WebAssembly module                        |
|   [5]   | `Linker`            | disposable class | resolves imports, instantiates modules             |
|   [6]   | `Instance`          | class            | one live module instance inside a `Store`          |
|   [7]   | `Memory`            | class            | linear memory access, implements `IExternal`       |
|   [8]   | `Global`            | class            | WebAssembly global variable                        |
|   [9]   | `Function`          | class            | callable WebAssembly function                      |
|  [10]   | `Table`             | class            | WebAssembly function/externref table               |
|  [11]   | `Caller`            | class            | host-function callback context inside a call frame |
|  [12]   | `WasiConfiguration` | class            | WASI preview-1 environment configuration           |

[PUBLIC_TYPE_SCOPE]: value and type system — `Wasmtime`
- rail: sandbox

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :----------- | :------------ | :------------------------------------------------------------------------------- |
|   [1]   | `ValueKind`  | enum (byte)   | `Int32`, `Int64`, `Float32`, `Float64`, `V128`, `FuncRef`, `ExternRef`, `AnyRef` |
|   [2]   | `Mutability` | enum          | `Const` or `Mutable` global mutability                                           |
|   [3]   | `Import`     | class         | one module import descriptor                                                     |
|   [4]   | `Export`     | class         | one module export descriptor                                                     |
|   [5]   | `TableKind`  | enum          | `FuncRef` or `ExternRef` element kind                                            |

[PUBLIC_TYPE_SCOPE]: compiler and profiling — `Wasmtime`
- rail: sandbox

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------ | :------------ | :------------------------------------- |
|   [1]   | `CompilerStrategy`  | enum          | `Auto`, `Cranelift`, `Winch`, `Pulley` |
|   [2]   | `OptimizationLevel` | enum          | `None`, `Speed`, `SpeedAndSize`        |
|   [3]   | `ProfilingStrategy` | enum          | `None`, `JitDump`, `VTune`, `PerfMap`  |

[PUBLIC_TYPE_SCOPE]: error handling — `Wasmtime`
- rail: sandbox

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------ | :------------ | :-------------------------------------------- |
|   [1]   | `TrapException`     | class         | WebAssembly trap, extends `WasmtimeException` |
|   [2]   | `WasmtimeException` | class         | base exception for all Wasmtime failures      |
|   [3]   | `TrapCode`          | enum          | trap cause taxonomy                           |
|   [4]   | `TrapFrame`         | class         | one stack frame from a trap backtrace         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine and configuration construction
- rail: sandbox

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------- |
|   [1]   | `Engine()`                                        | constructor    | default engine with AOT compilation        |
|   [2]   | `Engine(Config)`                                  | constructor    | engine from explicit configuration         |
|   [3]   | `Engine.IncrementEpoch()`                         | lifecycle call | advances epoch counter for interruption    |
|   [4]   | `Engine.IsPulleyInterpreter`                      | bool property  | true when using Pulley interpreter         |
|   [5]   | `Config()`                                        | constructor    | default configuration                      |
|   [6]   | `Config.WithDebugInfo(bool)`                      | fluent builder | enables DWARF debug info in code           |
|   [7]   | `Config.WithEpochInterruption(bool)`              | fluent builder | enables epoch-based interruption           |
|   [8]   | `Config.WithFuelConsumption(bool)`                | fluent builder | enables fuel-metered execution             |
|   [9]   | `Config.WithCompilerStrategy(CompilerStrategy)`   | fluent builder | selects compiler (Cranelift, Pulley, etc.) |
|  [10]   | `Config.WithOptimizationLevel(OptimizationLevel)` | fluent builder | controls Cranelift optimization level      |
|  [11]   | `Config.WithMaximumStackSize(int)`                | fluent builder | bounds wasm stack depth                    |
|  [12]   | `Config.WithParallelCompilation(bool)`            | fluent builder | parallelizes module compilation            |

[ENTRYPOINT_SCOPE]: module compilation
- rail: sandbox

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------- | :-------------- | :-------------------------------------- |
|   [1]   | `Module.FromBytes(Engine, string name, ReadOnlySpan<byte>)` | static factory  | compiles from binary wasm bytes         |
|   [2]   | `Module.FromFile(Engine, string path)`                      | static factory  | compiles from `.wasm` file              |
|   [3]   | `Module.FromStream(Engine, string name, Stream)`            | static factory  | compiles from stream                    |
|   [4]   | `Module.FromText(Engine, string name, string text)`         | static factory  | compiles from WAT text                  |
|   [5]   | `Module.FromTextFile(Engine, string path)`                  | static factory  | compiles from `.wat` file               |
|   [6]   | `Module.FromTextStream(Engine, string name, Stream)`        | static factory  | compiles from WAT stream                |
|   [7]   | `Module.Validate(Engine, ReadOnlySpan<byte>)`               | static call     | validates bytes without compiling       |
|   [8]   | `Module.Name`                                               | string property | module name used at compilation         |
|   [9]   | `Module.Imports`                                            | list property   | `IReadOnlyList<Import>` descriptor list |
|  [10]   | `Module.Exports`                                            | list property   | `IReadOnlyList<Export>` descriptor list |

[ENTRYPOINT_SCOPE]: store and instantiation
- rail: sandbox

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `Store(Engine)`                                     | constructor    | store with no host data               |
|   [2]   | `Store(Engine, object? data)`                       | constructor    | store with arbitrary host data        |
|   [3]   | `Store.Fuel`                                        | ulong property | remaining fuel (set or read)          |
|   [4]   | `Store.SetLimits(memorySize?, tableElements?, ...)` | call           | bounds memory, table, instance counts |
|   [5]   | `Store.SetWasiConfiguration(WasiConfiguration)`     | call           | attaches WASI environment             |
|   [6]   | `Store.SetEpochDeadline(ulong ticksBeyondCurrent)`  | call           | sets interruption deadline in epochs  |
|   [7]   | `Store.GetData()`                                   | call           | retrieves host data object            |
|   [8]   | `Store.GC()`                                        | call           | runs store garbage collection         |
|   [9]   | `Linker(Engine)`                                    | constructor    | creates linker for the given engine   |
|  [10]   | `Linker.AllowShadowing`                             | bool property  | permits import re-definition          |
|  [11]   | `Linker.DefineWasi()`                               | call           | adds WASI preview-1 imports           |
|  [12]   | `Linker.Define(module, name, Function)`             | call           | registers a host function import      |
|  [13]   | `Linker.Define(module, name, Memory)`               | call           | registers a memory import             |
|  [14]   | `Linker.Instantiate(Store, Module)`                 | call           | links and instantiates module         |
|  [15]   | `Linker.GetFunction(Store, module, name)`           | call           | retrieves a named function export     |
|  [16]   | `Linker.GetMemory(Store, module, name)`             | call           | retrieves a named memory export       |

[ENTRYPOINT_SCOPE]: instance and typed invocation
- rail: sandbox

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `Instance(Store, Module, params object[] imports)` | constructor    | instantiates module with explicit imports |
|   [2]   | `Instance.GetAction(string name)`                  | typed call     | `Action` export with no parameters        |
|   [3]   | `Instance.GetAction<TA>(string name)`              | typed call     | `Action<TA>` export                       |
|   [4]   | `Instance.GetFunction<TR>(string name)`            | typed call     | `Func<TR?>` export                        |
|   [5]   | `Instance.GetFunction<TA, TR>(string name)`        | typed call     | `Func<TA, TR?>` export                    |

[ENTRYPOINT_SCOPE]: WASI configuration
- rail: sandbox

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------ | :------------- | :---------------------------------- |
|   [1]   | `WasiConfiguration.WithArg(string)`                     | fluent builder | appends one command-line argument   |
|   [2]   | `WasiConfiguration.WithArgs(IEnumerable<string>)`       | fluent builder | appends multiple arguments          |
|   [3]   | `WasiConfiguration.WithInheritedArgs()`                 | fluent builder | inherits host process arguments     |
|   [4]   | `WasiConfiguration.WithEnvironmentVariable(k, v)`       | fluent builder | sets one environment variable       |
|   [5]   | `WasiConfiguration.WithInheritedEnvironment()`          | fluent builder | inherits host environment           |
|   [6]   | `WasiConfiguration.WithPreopenedDirectory(host, guest)` | fluent builder | mounts host path at guest WASI path |
|   [7]   | `WasiConfiguration.WithStandardInput(path)`             | fluent builder | redirects stdin from file           |
|   [8]   | `WasiConfiguration.WithStandardOutput(path)`            | fluent builder | redirects stdout to file            |
|   [9]   | `WasiConfiguration.WithInheritedStandardInput()`        | fluent builder | passes host stdin                   |
|  [10]   | `WasiConfiguration.WithInheritedStandardOutput()`       | fluent builder | passes host stdout                  |

## [4]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:
- namespace: `Wasmtime` only; 158 types
- native library: `libwasmtime` loaded from `runtimes/<rid>/native/`; all public APIs are thin safe-handle wrappers over P/Invoke
- execution hierarchy: `Engine` → `Module` (compiled) + `Store` (state) → `Linker` → `Instance`
- lifetime discipline: `Engine`, `Config`, `Store`, `Module`, `Linker` all implement `IDisposable`; dispose order must follow the hierarchy — store before engine
- typed exports: `Instance.GetAction<...>` and `Instance.GetFunction<...>` return null when the export is absent; callers must check null before invoking

[FUEL_AND_EPOCH]:
- fuel interruption: enabled via `Config.WithFuelConsumption(true)`; `Store.Fuel` setter adds fuel; exhaustion raises `TrapException` with `TrapCode.OutOfFuel`
- epoch interruption: enabled via `Config.WithEpochInterruption(true)`; `Engine.IncrementEpoch()` advances the counter; `Store.SetEpochDeadline(ticks)` sets the cutoff

[WASI_ADMISSION]:
- WASI entry: `Linker.DefineWasi()` registers WASI preview-1 host functions; must be called before instantiation
- WASI environment: `WasiConfiguration` built fluently, then passed to `Store.SetWasiConfiguration` before instantiation
- directory access: `WithPreopenedDirectory(hostPath, guestPath)` scopes filesystem access to specific prefixes

[LOCAL_ADMISSION]:
- Sandbox modules enter through `Module.FromBytes` or `Module.FromFile`; WAT text forms (`FromText`) are development/test paths only
- Host function callbacks access the call store through `Caller` — do not capture `Store` reference in closures
- `Linker.DefineFunction(module, name, callback, paramKinds, resultKinds)` accepts untyped delegates when type cardinality is dynamic

[RAIL_LAW]:
- Package: `Wasmtime`
- Owns: WebAssembly compilation, store-scoped execution, WASI environment, linear memory access, host function binding
- Accept: compiled modules, WASI-configured stores, typed export invocation
- Reject: unsafe P/Invoke against `libwasmtime` directly, captured-store closures in host functions, `Instance` construction without `Linker` for import-bearing modules
