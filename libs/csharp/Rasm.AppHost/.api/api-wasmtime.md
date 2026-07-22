# [RASM_APPHOST_API_WASMTIME]

`Wasmtime` binds the native WebAssembly runtime (`libwasmtime`) as the sandbox rail's plugin-isolation core: an `Engine` compiles a `Module`, a `Store` executes it under fuel, epoch, and memory limits, and a `Linker` resolves the capability-scoped import table. A guest reaches host authority only through the linker's granted imports over WASI-Preview-1.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wasmtime`
- package: `Wasmtime`
- assembly: `Wasmtime.Dotnet`
- namespace: `Wasmtime`
- asset: runtime library + native `libwasmtime` (`runtimes/<rid>/native/`)
- rail: sandbox

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime hierarchy

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `Engine`            | class         | root runtime engine and JIT/AOT compiler owner |
|  [02]   | `Config`            | class         | engine configuration builder                   |
|  [03]   | `Store`             | class         | execution store owning state and fuel          |
|  [04]   | `Module`            | class         | compiled WebAssembly module                    |
|  [05]   | `Linker`            | class         | import resolver and module instantiator        |
|  [06]   | `Instance`          | class         | one live module instance in a `Store`          |
|  [07]   | `Memory`            | class         | linear memory access over `IExternal`          |
|  [08]   | `Global`            | class         | WebAssembly global variable                    |
|  [09]   | `Function`          | class         | callable WebAssembly function                  |
|  [10]   | `Table`             | class         | function/externref element table               |
|  [11]   | `Caller`            | struct        | host-callback context inside a call frame      |
|  [12]   | `WasiConfiguration` | class         | WASI preview-1 environment configuration       |

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

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------ | :------------ | :-------------------------------------------- |
|  [01]   | `TrapException`     | class         | WebAssembly trap, extends `WasmtimeException` |
|  [02]   | `WasmtimeException` | class         | base exception for Wasmtime failures          |
|  [03]   | `TrapCode`          | enum          | trap cause taxonomy                           |
|  [04]   | `TrapFrame`         | class         | one stack frame from a trap backtrace         |

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

[ENTRYPOINT_SCOPE]: module compilation

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Module.FromBytes(Engine, string name, ReadOnlySpan<byte>)` | factory  | compiles from binary wasm bytes         |
|  [02]   | `Module.FromFile(Engine, string path)`                      | factory  | compiles from `.wasm` file              |
|  [03]   | `Module.FromStream(Engine, string name, Stream)`            | factory  | compiles from stream                    |
|  [04]   | `Module.FromText(Engine, string name, string text)`         | factory  | compiles from WAT text                  |
|  [05]   | `Module.FromTextFile(Engine, string path)`                  | factory  | compiles from `.wat` file               |
|  [06]   | `Module.FromTextStream(Engine, string name, Stream)`        | factory  | compiles from WAT stream                |
|  [07]   | `Module.Validate(Engine, ReadOnlySpan<byte>)`               | static   | validates bytes without compiling       |
|  [08]   | `Module.Name`                                               | property | module name used at compilation         |
|  [09]   | `Module.Imports`                                            | property | `IReadOnlyList<Import>` descriptor list |
|  [10]   | `Module.Exports`                                            | property | `IReadOnlyList<Export>` descriptor list |

[ENTRYPOINT_SCOPE]: store and instantiation

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Store(Engine)`                                     | ctor     | store with no host data               |
|  [02]   | `Store(Engine, object? data)`                       | ctor     | store with arbitrary host data        |
|  [03]   | `Store.Fuel`                                        | property | remaining fuel, set or read           |
|  [04]   | `Store.SetLimits(memorySize?, tableElements?, ...)` | instance | bounds memory, table, instance counts |
|  [05]   | `Store.SetWasiConfiguration(WasiConfiguration)`     | instance | attaches WASI environment             |
|  [06]   | `Store.SetEpochDeadline(ulong ticksBeyondCurrent)`  | instance | sets interruption deadline in epochs  |
|  [07]   | `Store.GetData()`                                   | instance | retrieves host data object            |
|  [08]   | `Store.GC()`                                        | instance | runs store garbage collection         |
|  [09]   | `Linker(Engine)`                                    | ctor     | creates linker for the given engine   |
|  [10]   | `Linker.AllowShadowing`                             | property | permits import re-definition          |
|  [11]   | `Linker.DefineWasi()`                               | instance | adds WASI preview-1 imports           |
|  [12]   | `Linker.Define(module, name, Function)`             | instance | registers a host function import      |
|  [13]   | `Linker.Define(module, name, Memory)`               | instance | registers a memory import             |
|  [14]   | `Linker.Instantiate(Store, Module)`                 | instance | links and instantiates module         |
|  [15]   | `Linker.GetFunction(Store, module, name)`           | instance | retrieves a named function export     |
|  [16]   | `Linker.GetMemory(Store, module, name)`             | instance | retrieves a named memory export       |

[ENTRYPOINT_SCOPE]: instance and typed invocation

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Instance(Store, Module, params object[] imports)` | ctor     | instantiates module with explicit imports |
|  [02]   | `Instance.GetAction(string name)`                  | instance | `Action` export with no parameters        |
|  [03]   | `Instance.GetAction<TA>(string name)`              | instance | `Action<TA>` export                       |
|  [04]   | `Instance.GetFunction<TR>(string name)`            | instance | `Func<TR?>` export                        |
|  [05]   | `Instance.GetFunction<TA, TR>(string name)`        | instance | `Func<TA, TR?>` export                    |

[ENTRYPOINT_SCOPE]: WASI configuration

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `WasiConfiguration.WithArg(string)`                     | instance | appends one command-line argument   |
|  [02]   | `WasiConfiguration.WithArgs(IEnumerable<string>)`       | instance | appends multiple arguments          |
|  [03]   | `WasiConfiguration.WithInheritedArgs()`                 | instance | inherits host process arguments     |
|  [04]   | `WasiConfiguration.WithEnvironmentVariable(k, v)`       | instance | sets one environment variable       |
|  [05]   | `WasiConfiguration.WithInheritedEnvironment()`          | instance | inherits host environment           |
|  [06]   | `WasiConfiguration.WithPreopenedDirectory(host, guest)` | instance | mounts host path at guest WASI path |
|  [07]   | `WasiConfiguration.WithStandardInput(path)`             | instance | redirects stdin from file           |
|  [08]   | `WasiConfiguration.WithStandardOutput(path)`            | instance | redirects stdout to file            |
|  [09]   | `WasiConfiguration.WithInheritedStandardInput()`        | instance | passes host stdin                   |
|  [10]   | `WasiConfiguration.WithInheritedStandardOutput()`       | instance | passes host stdout                  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- execution hierarchy: `Engine` compiles a `Module` and roots a `Store`, a `Linker` resolves imports and `Instantiate` yields an `Instance`.
- native binding: every public type is a safe-handle wrapper over P/Invoke into `libwasmtime` (`runtimes/<rid>/native/`).
- dispose discipline: `Engine`, `Config`, `Store`, `Module`, `Linker` implement `IDisposable`, and dispose follows the hierarchy — `Store` before `Engine`.
- fluent config: every `Config.With*` and `WasiConfiguration.With*` returns the receiver, so configuration chains.
- typed exports: `Instance.GetAction<...>` and `Instance.GetFunction<...>` return null for an absent export, so a caller checks null before invoking.
- fuel metering: `Config.WithFuelConsumption(true)` arms it, the `Store.Fuel` setter adds fuel, and exhaustion raises `TrapException` with `TrapCode.OutOfFuel`.
- epoch interruption: `Config.WithEpochInterruption(true)` arms it, `Engine.IncrementEpoch()` advances the counter, and `Store.SetEpochDeadline(ticks)` sets the cutoff.
- WASI admission: `Linker.DefineWasi()` and `Store.SetWasiConfiguration(WasiConfiguration)` both precede instantiation, and `WithPreopenedDirectory(host, guest)` scopes filesystem access to named prefixes.

[STACKING]:
- within-lib fold: `Engine` → `Module` + `Store` → `Linker.Instantiate` → `Instance`; typed exports invoke through `Instance.GetFunction<...>`/`GetAction<...>`, and `Config` → `Store` threads fuel and epoch state.
- `SandboxIsolation.WasmModule` (AppHost sandbox owner): the `Linker` import table carries only the grant-scoped host functions, `Linker.DefineWasi()` mounts the WASI-Preview-1 descriptors a `WasiConfiguration` pre-open set scopes, `Config.WithFuelConsumption`/`Store.Fuel` meters CPU, and `Store.SetLimits` caps linear memory.

[LOCAL_ADMISSION]:
- Sandbox modules enter through `Module.FromBytes` or `Module.FromFile`; the `FromText` WAT forms are development paths.
- Host callbacks reach the call store through `Caller`, never a captured `Store` in a closure.
- `Linker.DefineFunction(module, name, callback, paramKinds, resultKinds)` binds untyped delegates when type cardinality is dynamic.

[RAIL_LAW]:
- Package: `Wasmtime`
- Owns: WebAssembly compilation, store-scoped execution, WASI environment, linear memory access, host function binding.
- Accept: compiled modules, WASI-configured stores, typed export invocation.
- Reject: direct P/Invoke against `libwasmtime`, captured-store closures in host callbacks, `Instance` construction without a `Linker` for import-bearing modules.
