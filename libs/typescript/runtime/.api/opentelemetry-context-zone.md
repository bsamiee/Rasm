# [TS_RUNTIME_API_OPENTELEMETRY_CONTEXT_ZONE]

`@opentelemetry/context-zone` is the browser async-context manager: one export, `ZoneContextManager`, implementing the api `ContextManager` over `zone.js` so a span's context survives browser async hops — timers, promises, and event listeners — that the default stack context manager loses. Its bundled form carries `zone.js` by side-effect import, and it exists exactly for the `web` SDK row, where user-interaction and fetch spans must parent correctly across the event loop. `plane:browser` by nature: importing it patches the global `Zone`, so it is app-composition-root material under the same law as the instrumentation rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/context-zone`
- package: `@opentelemetry/context-zone`
- license: `Apache-2.0`
- composition: re-exports `@opentelemetry/context-zone-peer-dep` with `zone.js` bundled — one import, no peer to wire
- consumed-by: the `web` SDK row's context wiring at the browser composition root; nothing else
- runtime: browser only — `Zone` patching is a global side effect

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the zone context manager
- rail: observability/context

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `ZoneContextManager` | `ContextManager`  | the `web` row's SDK context manager                      |
|  [02]   | `TargetWithEvents`   | bind target shape | `addEventListener`/`removeEventListener` bearing targets |
|  [03]   | `Func`               | callback shape    | the bound-function type the manager wraps                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle
- rail: observability/context
- One construction feeds the web SDK configuration; `enable()`/`disable()` bracket its active window, and `bind(context, target)` attaches a context to a function or event-bearing target — the mechanism the user-interaction row rides.

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `new ZoneContextManager()`     | ctor           | one instance at the browser composition root     |
|  [02]   | `.enable()` / `.disable()`     | lifecycle      | SDK registration owns the bracket                |
|  [03]   | `.active()` / `.with(ctx, fn)` | context read   | SDK-internal; Rasm code reads context via Effect |
|  [04]   | `.bind(context, target)`       | bind           | span-to-listener binding under interaction spans |

## [04]-[IMPLEMENTATION_LAW]

[CONTEXT_TOPOLOGY]:
- composition-root only — the import patches `Zone` globally, so exactly one module (the browser boot graph) may import it; a library import double-patches the host.

[INTEGRATION_LAW]:
- Stack with `otel/emit`'s `web` row: the manager rides the `WebSdk` configuration so fetch, document-load, and interaction spans parent across async hops; without it the stack context manager drops the parent at the first timer.
- Stack with `opentelemetry-instrumentation-user-interaction.md`: the interaction row detects the patched `Zone` and rides it; absent the manager it degrades to `addEventListener` patching.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane only; the exports-map browser condition is what keeps a server bundle from resolving it.

[RAIL_LAW]:
- Package: `@opentelemetry/context-zone`
- Owns: browser async context continuity for the web SDK row
- Accept: one construction at the browser composition root, handed to the SDK configuration
- Reject: library-altitude import, a second context manager beside it, server-lane resolution
