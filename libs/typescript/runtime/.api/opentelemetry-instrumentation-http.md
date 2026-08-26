# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_HTTP]

`@opentelemetry/instrumentation-http` patches node's `http` and `https` modules so every inbound request and outbound client call opens a span carrying stable HTTP semconv. One row covers both directions, each independently disableable, each with its own ignore hook, parent-presence gate, and header allow-list; query-parameter redaction runs inside the span builder, before `url.full` reaches an attribute.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the instrumentation row and its config hook family

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `HttpInstrumentation`                                             | class         | the `Instrumentation` row            |
|  [02]   | `HttpInstrumentationConfig`                                       | interface     | the whole knob surface               |
|  [03]   | `IgnoreIncomingRequestFunction` / `IgnoreOutgoingRequestFunction` | interface     | inbound and outbound skip predicates |
|  [04]   | `HttpRequestCustomAttributeFunction` / `…Response…`               | interface     | per-direction span enrichment hooks  |
|  [05]   | `StartIncomingSpanCustomAttributeFunction` / `…Outgoing…`         | interface     | attributes computed at span start    |
|  [06]   | `HttpCustomAttributeFunction`                                     | interface     | request-and-response pair hook       |

- `HttpInstrumentationConfig` extends `InstrumentationConfig` (`enabled?`) with `ignoreIncomingRequestHook?`, `ignoreOutgoingRequestHook?`, `disableIncomingRequestInstrumentation?`, `disableOutgoingRequestInstrumentation?`, `applyCustomAttributesOnSpan?`, `requestHook?`, `responseHook?`, `startIncomingSpanHook?`, `startOutgoingSpanHook?`, `serverName?`, `requireParentforIncomingSpans?`, `requireParentforOutgoingSpans?`, `headersToSpanAttributes?`, `enableSyntheticSourceDetection?`, `redactedQueryParams?`.
- `headersToSpanAttributes` nests by direction: `{ client?: { requestHeaders?, responseHeaders? }, server?: { requestHeaders?, responseHeaders? } }`, each a header-name roster — never a blanket capture.
- `redactedQueryParams` names query-string parameters masked inside the recorded URL, so a deployment keeping `url.full` still drops named secrets at the span source; the roster REPLACES the package's own default set (`sig`, `Signature`, `AWSAccessKeyId`, `X-Goog-Signature`) rather than extending it, so a policy row that names one parameter unmasks the other four.
- `requireParentforIncomingSpans` and `requireParentforOutgoingSpans` stay separate: an inbound request is legitimately a trace root, while an orphan outbound span signals a hop the owning code never opened.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction — the row is data, activation belongs to `registerInstrumentations`

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `new HttpInstrumentation(config?)`    | ctor     | one row in the registered array             |
|  [02]   | `.setConfig(config)` / `.getConfig()` | instance | replace or read the row's config live       |
|  [03]   | `.enable()` / `.disable()`            | instance | the `Instrumentation` contract's own toggle |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registration precedes module load — the row patches `http`/`https` on require, so the registration node must run before any module that imports them; a lazily-imported agent factory exists precisely to avoid loading `http` too early.
- both directions ride one row — disabling a direction is a config field, never a second instrumentation.
- ignore hooks receive different shapes and therefore different coordinates: `ignoreIncomingRequestHook` takes an `IncomingMessage` whose `url` is a request PATH, `ignoreOutgoingRequestHook` takes the PARSED `RequestOptions`, and neither ever sees a scheme-qualified URL.
- parsed outgoing options split the authority across three optional fields, filled only where the call site supplied them: a `URL` argument — what the OTLP node transport passes — yields `{ protocol, hostname, path }` with `port` set only when non-default and `host` ABSENT, while an options object passes its own `host` through, which may carry `:port`. Reading `host` alone therefore misses the exporter's own traffic and the trace feed traces itself.
- normalization runs internally as `hostname || host.match(/^([^:/ ]+)(:\d{1,5})?/)[1] || 'localhost'` beside `port || the matched group || protocol-default`, and no export exposes it, so a self-exclusion predicate spells that fold itself.

[STACKING]:
- `opentelemetry-instrumentation.md` `registerInstrumentations`: the row joins one array with an explicit `tracerProvider`; construction policy lives here, activation there.
- `opentelemetry-context-async-hooks.md`: without an installed context manager every patched callback reads ROOT, so an inbound span never parents the outbound call it triggers.
- `opentelemetry-instrumentation-undici.md`: undici and `fetch` bypass `node:http` entirely, so the two rows split the client surface with no overlap and neither double-traces the other's transport.
- `otel/server` `[02]-[REGISTRATION]`: the server registration node constructs the row from the export policy — the outbound hook matched against the collector host and port its `_authority` projection normalizes out of the parsed options, the inbound hook against the policy's own path roster, the parent-presence gate off `policy.server.orphan`, and the redaction roster off `policy.server.redact`.

[LOCAL_ADMISSION]:
- `scope:runtime`, server condition only — the server registration node is the sole importer, and Effect's own `Effect.withSpan` call sites remain the primary span source.
- this row covers foreign libraries; a branch boundary reaching for it instead of opening its own span inverts the precedence law.
