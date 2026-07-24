# [TS_RUNTIME_API_OPENTELEMETRY_CONTEXT_ZONE]

`@opentelemetry/context-zone` owns browser async-context continuity: its one export `ZoneContextManager` implements the api `ContextManager` over `zone.js`, so a span's context survives the timers, promises, and event listeners the stack context manager drops. Importing patches the global `Zone`, so one browser composition-root module binds it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/context-zone`
- package: `@opentelemetry/context-zone` (Apache-2.0)
- module: dual CJS + ESM flat barrel, no subpath exports, `sideEffects: true`; bundles `zone.js` by side-effect import and re-exports `@opentelemetry/context-zone-peer-dep`, so one import wires the manager with no peer to add.
- runtime: browser only — importing patches the global `Zone`.
- rail: observability/context

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the zone context manager and its bind-target shapes

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------- | :------------ | :------------------------------------- |
|  [01]   | `ZoneContextManager` | class         | api `ContextManager` over `zone.js`    |
|  [02]   | `TargetWithEvents`   | interface     | `addEventListener` bind target         |
|  [03]   | `Func`               | type          | bound-function shape the manager wraps |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the context lifecycle

| [INDEX] | [SURFACE]                      | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------- | :------- | :----------------------------------- |
|  [01]   | `new ZoneContextManager()`     | ctor     | one instance at the composition root |
|  [02]   | `.enable()` / `.disable()`     | instance | bracket the active window            |
|  [03]   | `.active()` / `.with(ctx, fn)` | instance | read and run within a context        |
|  [04]   | `.bind(context, target)`       | instance | bind a context to a target           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — importing patches `Zone` globally, so exactly one module (the browser boot graph) imports it; a library import double-patches the host.

[STACKING]:
- `opentelemetry-sdk-trace-web.md` `WebTracerProvider.register`: `ZoneContextManager` satisfies `SDKRegistrationConfig.contextManager`, replacing the sync-only `StackContextManager` so document-load, fetch, and interaction spans parent across async hops.
- `opentelemetry-instrumentation-user-interaction.md` `UserInteractionInstrumentation`: detects the patched `Zone` and parents the triggered fetch under the interaction span; absent the manager it degrades to `addEventListener` patching.
- `browser/boot`: composes the one construction at startup and hands it to the web SDK context wiring.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane only — the boot graph is the sole importer, and the edge ledger keeps a server bundle from resolving it.

[RAIL_LAW]:
- Package: `@opentelemetry/context-zone`
- Owns: browser async-context continuity for the web SDK row
- Accept: one construction at the browser composition root, handed to the SDK context wiring
- Reject: library-altitude import, a second context manager beside it, server-lane resolution
