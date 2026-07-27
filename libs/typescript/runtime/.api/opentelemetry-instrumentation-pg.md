# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_PG]

`@opentelemetry/instrumentation-pg` traces the `pg` client: one span per query and, unless suppressed, per connection acquisition, under stable database semconv. Three capabilities beyond span emission earn the row — `enhancedDatabaseReporting` attaches statement parameters, `addSqlCommenterCommentToQueries` writes a SQLCommenter comment into the statement text `pg_stat_statements` normalizes, and `enableTraceContextPropagation` stamps the live `traceparent` onto the session so `pg_stat_activity` reads it — each carrying a different cost and each priced on its own row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-pg`
- package: `@opentelemetry/instrumentation-pg` (Apache-2.0)
- module: dual CJS + ESM flat barrel; `@opentelemetry/api` `^1.3.0` is the one peer, `@opentelemetry/instrumentation` the base, `pg` the patched module
- runtime: node and bun only — the row patches the `pg` client and pool at require time
- rail: observability/tracing — the PostgreSQL client span source

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the instrumentation row, its config, and the hook payload shapes

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `PgInstrumentation`                         | class         | the `Instrumentation` row                                  |
|  [02]   | `PgInstrumentationConfig`                   | interface     | the whole knob surface                                     |
|  [03]   | `AttributeNames`                            | enum          | the package's own attribute-key roster                     |
|  [04]   | `PgRequestHookInformation`                  | interface     | query text and name beside the connection coordinates      |
|  [05]   | `PgResponseHookInformation`                 | interface     | the settled `QueryResult` or `QueryArrayResult`            |
|  [06]   | `PgInstrumentationExecutionRequestHook` / `…ResponseHook` | interface | span enrichment at request and response    |

- `PgInstrumentationConfig` extends `InstrumentationConfig` (`enabled?`) with `enhancedDatabaseReporting?`, `requestHook?`, `responseHook?`, `requireParentSpan?`, `addSqlCommenterCommentToQueries?`, `ignoreConnectSpans?`, `enableTraceContextPropagation?`.
- `enhancedDatabaseReporting` attaches the statement's bound parameter values to the span; parameters are identifier-grade material, so the field is a compliance decision rather than a verbosity one.
- `addSqlCommenterCommentToQueries` appends a SQLCommenter comment to the statement text, so `pg_stat_statements` carries the trace coordinate on the normalized query; the cost is comment bytes on every statement.
- `enableTraceContextPropagation` is an INDEPENDENT mechanism, not that comment's other half: it issues a `SET application_name` carrying the W3C `traceparent` before each user query, and the SET must settle before pg's queue dispatches the query, so arming it roughly DOUBLES the connection's network round-trips. It buys `pg_stat_activity.application_name` carrying the live trace, and binding it to the comment field spends that latency on every deployment that wanted comments alone.
- `ignoreConnectSpans` drops the acquisition span while keeping query spans and pool metrics, so a pooled workload takes that row rather than minting a checkout span per query.
- `requireParentSpan` gates on a live parent, so a background pool health query never roots a trace.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction — the row is data, activation belongs to `registerInstrumentations`

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new PgInstrumentation(config?)`     | ctor     | one row in the registered array             |
|  [02]   | `.setConfig(config)` / `.getConfig()` | instance | replace or read the row's config live       |
|  [03]   | `.enable()` / `.disable()`           | instance | the `Instrumentation` contract's own toggle |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registration precedes module load — the row patches `pg` on require, so the registration node must run before any module importing the client.
- this row traces the client, never the server; database-side timing arrives through the store's own telemetry and joins on the propagated trace id.

[STACKING]:
- `opentelemetry-instrumentation.md` `registerInstrumentations`: the row joins one array with an explicit `tracerProvider`; construction policy lives here, activation there.
- `opentelemetry-context-async-hooks.md`: `requireParentSpan` reads `context.active()`, so without an installed manager every query reads orphan and the gate drops every span.
- `effect-sql-pg` (data lane): the branch's own statements ride the SQL client under `Effect.withSpan`, so this row exists for foreign libraries reaching `pg` directly and its parent gate is what keeps the two from double-spanning one statement.
- `otel/emit` `[07]-[INSTRUMENT]`: the server registration node constructs the row from the export policy — statement capture off `policy.server.statement`, the SQLCommenter comment off `policy.server.comment`, the session stamp off its own `policy.server.session` so its round-trip cost is priced apart, the acquisition span off `policy.server.connect`, and the parent gate off `policy.server.orphan`.

[LOCAL_ADMISSION]:
- `scope:runtime`, server condition only — the server registration node is the sole importer.
- statement capture defaults off; a deployment enabling it inherits the export-boundary redaction scrub as its only guard.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-pg`
- Owns: PostgreSQL client spans under stable database semconv, statement-parameter capture, the SQLCommenter statement comment, the `application_name` session stamp, and the connection-span and parent gates
- Accept: one construction inside the server registration node with the parent gate armed, statement capture decided by compliance posture, and the comment and session rows decided independently
- Reject: statement capture as a debug default, the session stamp bound to the comment field so its round-trip cost rides an unrelated decision, a second `pg` instrumentation row, use as a substitute for an owning seam's own span
