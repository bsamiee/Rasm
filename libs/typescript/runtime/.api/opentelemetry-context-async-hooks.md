# [TS_RUNTIME_API_OPENTELEMETRY_CONTEXT_ASYNC_HOOKS]

`@opentelemetry/context-async-hooks` owns node async-context continuity: two `ContextManager` implementations carry a span's context across promises, timers, and callbacks, so a library reading `context.active()` inside an async hop sees the live span instead of ROOT. `AsyncLocalStorageContextManager` rides `node:async_hooks`' `AsyncLocalStorage` and is the standing row; `AsyncHooksContextManager` is the raw-hook implementation kept for runtimes without `AsyncLocalStorage`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/context-async-hooks`
- package: `@opentelemetry/context-async-hooks` (Apache-2.0)
- module: dual CJS + ESM flat barrel, no subpath exports; `@opentelemetry/api` `>=1.0.0 <1.10.0` is the one peer
- runtime: node and bun only — `node:async_hooks` is the substrate; the browser condition binds `@opentelemetry/context-zone` instead
- rail: observability/context

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two manager implementations of the api `ContextManager` contract

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `AsyncLocalStorageContextManager` | class         | `ContextManager` over `AsyncLocalStorage`           |
|  [02]   | `AsyncHooksContextManager`        | class         | `ContextManager` over raw `async_hooks` bookkeeping |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the context lifecycle — the shape both classes share

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new AsyncLocalStorageContextManager()`   | ctor     | one instance at the composition root          |
|  [02]   | `.enable()` / `.disable()`                | instance | bracket the active window; both return `this` |
|  [03]   | `.active()` / `.with(context, fn, …args)` | instance | read and run within a context                 |
|  [04]   | `.bind(context, target)`                  | instance | bind a context to a function or emitter       |

- `.enable()` returns the manager, so `context.setGlobalContextManager(new AsyncLocalStorageContextManager().enable())` is the one-expression install.
- `.with(context, fn, thisArg?, …args)` runs `fn` inside the storage frame; every async continuation created within it inherits the frame.
- `.disable()` clears the storage and the tracked bindings, so scope teardown releases the process-global hook.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — the manager installs process-globally through `context.setGlobalContextManager`, so exactly one module binds it and a library install fights the host for one global slot.
- `AsyncLocalStorageContextManager` is the standing row; `AsyncHooksContextManager` enters only where `AsyncLocalStorage` is unavailable, and both cannot be installed at once.

[STACKING]:
- `opentelemetry-api.md` `context`: the manager satisfies `context.setGlobalContextManager(ContextManager)` and `context.disable()` releases it, so `context.active()` resolves the live span for every foreign reader.
- `opentelemetry-instrumentation.md` `registerInstrumentations`: node instrumentation rows read `context.active()` to parent their spans, so the manager is what stops each library hop from rooting a fresh trace.
- `effect-opentelemetry.md` `Tracer`: Effect's own spans ride the fiber-backed tracer and need no ambient manager; the install exists for foreign libraries alone, so it adds context and takes nothing from the Effect path.
- `otel/server`: composes the one construction inside the server-condition registration bracket and releases it with the instrumentation unload thunk.

[LOCAL_ADMISSION]:
- `scope:runtime`, server condition only — the server registration node is the sole importer, and the condition split keeps a browser bundle from resolving `node:async_hooks`.

[RAIL_LAW]:
- Package: `@opentelemetry/context-async-hooks`
- Owns: node async-context continuity for foreign instrumentation rows
- Accept: one `AsyncLocalStorageContextManager` construction at the server registration node, enabled into the api global and disabled on scope close
- Reject: library-altitude install, both managers at once, browser-condition resolution, reading the global back inside branch code
