# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_UNDICI]

`@opentelemetry/instrumentation-undici` traces the undici client and the global `fetch` node builds on it, a transport `node:http` patching never sees. It subscribes to undici's diagnostics channel rather than patching a module, so registration order is free, and its parent-presence gate is what keeps an orphan client call from rooting a rival trace.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the instrumentation row, its config, and the request and response shapes its hooks receive

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `UndiciInstrumentation`                                 | class         | the `Instrumentation` row                                |
|  [02]   | `UndiciInstrumentationConfig<Q, S>`                     | interface     | the whole knob surface, generic over request/response    |
|  [03]   | `UndiciRequest`                                         | interface     | origin, method, path, headers, body, and lifecycle flags |
|  [04]   | `UndiciResponse`                                        | interface     | raw header buffers, status code, status text             |
|  [05]   | `IgnoreRequestFunction<Q>`                              | interface     | `(request) => boolean`                                   |
|  [06]   | `RequestHookFunction<Q>` / `ResponseHookFunction<Q, S>` | interface     | span enrichment at request and response                  |
|  [07]   | `StartSpanHookFunction<Q>`                              | interface     | attributes computed at span start                        |

- `UndiciInstrumentationConfig` extends `InstrumentationConfig` (`enabled?`) with `ignoreRequestHook?`, `requestHook?`, `responseHook?`, `startSpanHook?`, `requireParentforSpans?`, and `headersToSpanAttributes?: { requestHeaders?: string[]; responseHeaders?: string[] }`.
- `UndiciRequest.origin` carries scheme and authority, so an ignore hook matches an endpoint by origin without reassembling a URL.
- `UndiciRequest.addHeader(name, value)` opens the injection seam trace-context propagation uses, so a hook adds a header without touching the raw array form.
- `requireParentforSpans` gates on a live parent rather than suppressing a downstream layer; two different mechanisms, and this package ships the gate.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction — the row is data, activation belongs to `registerInstrumentations`

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `new UndiciInstrumentation(config?)`  | ctor     | one row in the registered array             |
|  [02]   | `.setConfig(config)` / `.getConfig()` | instance | replace or read the row's config live       |
|  [03]   | `.enable()` / `.disable()`            | instance | the `Instrumentation` contract's own toggle |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- diagnostics-channel subscription, not module patching — registration order carries no constraint, unlike the `http` row.
- one row covers `fetch` and every undici dispatcher, so a composition tracing outbound calls wires no per-client hook.

[STACKING]:
- `opentelemetry-instrumentation.md` `registerInstrumentations`: the row joins one array with an explicit `tracerProvider`; construction policy lives here, activation there.
- `opentelemetry-instrumentation-http.md`: two rows split the client surface — `node:http` traffic on that row, undici and `fetch` on this one — so a single outbound hop is never traced twice.
- `opentelemetry-context-async-hooks.md`: `requireParentforSpans` reads `context.active()`, so without an installed manager every client call reads orphan and the gate drops every span.
- `otel/server` `[02]-[REGISTRATION]`: the server registration node constructs the row from the export policy — origin self-exclusion against the collector and the parent gate off `policy.server.orphan`.

[LOCAL_ADMISSION]:
- `scope:runtime`, server condition only — the server registration node is the sole importer.
- branch-owned outbound HTTP rides `net/client`'s policy `HttpClient` under `Effect.withSpan`, so this row covers foreign libraries dialing out, never the branch's own lanes.
