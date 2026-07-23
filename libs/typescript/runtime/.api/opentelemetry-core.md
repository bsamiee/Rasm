# [TS_RUNTIME_API_OPENTELEMETRY_CORE]

`@opentelemetry/core` is the dependency-free primitive layer every `@opentelemetry/sdk-*` and `@opentelemetry/exporter-*` package builds on: the concrete W3C `TextMapPropagator` triad (`W3CTraceContextPropagator`/`W3CBaggagePropagator`/`CompositePropagator`) plus the raw `traceparent`/`tracestate`/`baggage` codecs, the `ExportResult`/`ExportResultCode` rail every exporter reports through, the `Context`-key operators (`suppressTracing`, `getRPCMetadata`), the typed `OTEL_*` env readers, and the `HrTime` conversion algebra. It carries no third-party runtime dependency — only the `@opentelemetry/api` peer and the pure-data `@opentelemetry/semantic-conventions` sibling. Inside Rasm it is one row of the `[OTLP_SDK]` SDK-bridge pin block behind the `@effect/opentelemetry` facade: `otel/emit` composes the propagation codecs to turn an inbound `traceparent` into a `SpanContext` for the facade's `Tracer.makeExternalSpan`/`withSpanContext`, and the SDK-bridge lane (`NodeSdk`/`WebSdk`) rides core's `ExportResult` rail. The edge ledger fences `@opentelemetry/*` to `scope:runtime` only; this whole block is the `[OTEL_PIN_BLOCK]`-collapse target that retires once native `Otlp` parity closes (`semantic-conventions` survives, this does not).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/core`
- package: `@opentelemetry/core` (Apache-2.0)
- otel-peer: `@opentelemetry/api >=catalog <catalog` (the `Context`/`SpanContext`/`TextMapPropagator`/`HrTime`/`Attributes` type source); dep `@opentelemetry/semantic-conventions ^catalog` (pure-data, the sole runtime dep) — no third-party runtime dependency
- consumed-by: `otel/emit` (W3C extract-and-continue codecs), the SDK-bridge lane on `otel/emit` (`ExportResult` rail), and every sibling `@opentelemetry/sdk-*`/`exporter-*` peer transitively
- catalog-verdict: KEEP as SDK-bridge peer; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[OTEL_PIN_BLOCK]`-collapse member of the `[OTLP_SDK]` block (retires when native `Otlp` covers W3C context + export)
- runtime: dual — one index over a `platform/{node,browser}` split; `node` reads `process.env`, `browser` reads `globalThis`; the propagation, time, and export surfaces are runtime-neutral
- module-families: W3C propagation (`W3C*Propagator`, `CompositePropagator`, `parseTraceParent`, `TraceState`), export rail (`ExportResult*`, `internal._export`), context-key operators (`*Tracing`, `*RPCMetadata`), typed env readers (`get*FromEnv`), `HrTime` algebra (`hrTime*`), attribute/error/util primitives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: W3C propagation carriers
- rail: observability/propagation
- The propagators are ONE parameterized family, not three unrelated classes: each implements `api.TextMapPropagator` (`inject`/`extract`/`fields`), and `CompositePropagator` folds an ordered `propagators[]` into one — a new wire format is a `TextMapPropagator` row in the composite config, never a new top-level propagator the ingress must special-case. `TraceState` is the immutable `tracestate` list carrier the facade's `makeExternalSpan({ traceState })` accepts (core's `TraceState` implements `api.TraceState`).

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                       |
| :-----: | :------------------------------------ | :---------------- | :-------------------------------------------------------- |
|  [01]   | `W3CTraceContextPropagator`           | propagator        | `otel/emit` `traceparent`/`tracestate` inject+extract     |
|  [02]   | `W3CBaggagePropagator`                | propagator        | W3C `baggage` header inject+extract at the same ingress   |
|  [03]   | `CompositePropagator`                 | propagator monoid | fold trace-context + baggage into one propagator          |
|  [04]   | `CompositePropagatorConfig`           | config            | the ordered `propagators?` set injected/extracted in turn |
|  [05]   | `TraceState`                          | `tracestate` list | immutable `set`/`unset`/`get`/`serialize` list            |
|  [06]   | `TRACE_PARENT_HEADER = "traceparent"` | header const      | the `traceparent` header key `otel/emit` reads            |
|  [07]   | `TRACE_STATE_HEADER = "tracestate"`   | header const      | the `tracestate` header key `otel/emit` reads             |

[PUBLIC_TYPE_SCOPE]: export rail + diagnostic carriers
- rail: observability/export
- Every exporter (`OTLPTraceExporter`/`OTLPMetricExporter` and every SDK processor) reports terminal disposition through `ExportResult`/`ExportResultCode` — the single result rail the SDK-bridge lane surfaces. `internal._export` adapts a callback-style `Exporter<T>` into a `Promise<ExportResult>` at the SDK seam.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                           |
| :-----: | :------------------------------------------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `ExportResult { code: ExportResultCode; error?: Error }` | result carrier    | terminal exporter disposition                 |
|  [02]   | `ExportResultCode { SUCCESS = 0, FAILED = 1 }`           | result enum       | the two-state export outcome discriminant     |
|  [03]   | `internal.Exporter<T> { export(arg, cb): void }`         | exporter shape    | callback exporter the `_export` adapter wraps |
|  [04]   | `RPCMetadata` (`= HTTPMetadata`) / `RPCType { HTTP }`    | context-key value | active HTTP-route metadata for span naming    |
|  [05]   | `InstrumentationScope { name; version?; schemaUrl? }`    | scope record      | emitting-scope identity on spans/metrics/logs |
|  [06]   | `ErrorHandler = (ex: Exception) => void`                 | callback          | the global error-sink type                    |
|  [07]   | `Clock { now(): number }`                                | clock             | monotonic clock contract for `AnchoredClock`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: W3C extract-and-continue codecs
- rail: observability/propagation
- `otel/emit` decodes an inbound header with `parseTraceParent` (never a hand-rolled regex — root policy bans stringy `traceparent` parsing), lifts the `tracestate` through `new TraceState(raw)`, and hands the resulting `SpanContext` to the facade's `Tracer.makeExternalSpan`/`withSpanContext`. `parseKeyPairsIntoRecord` is the symmetric `baggage` decoder.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                                  |
| :-----: | :----------------------------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `parseTraceParent(traceParent: string)`    | codec           | inbound `traceparent` → `SpanContext \| null` for `makeExternalSpan` |
|  [02]   | `parseKeyPairsIntoRecord(value?: string)`  | codec           | inbound W3C `baggage` → `Record<string, string>`                     |
|  [03]   | `new TraceState(rawTraceState?: string)`   | carrier ctor    | `tracestate` list round-trip; `serialize()` for egress               |
|  [04]   | `new CompositePropagator({ propagators })` | propagator ctor | one propagator over the trace-context + baggage row set              |

[ENTRYPOINT_SCOPE]: `Context`-key operators
- rail: observability/context
- Two immutable `Context`-key families, both `(Context) -> Context` writers with a matching reader: `suppressTracing` fences internal exporter HTTP calls out of their own trace (the SDK-bridge exporter calls this so OTLP egress is not self-traced), and `setRPCMetadata` carries active HTTP-route data for span naming.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `suppressTracing` / `unsuppressTracing` / `isTracingSuppressed` | context key    | fence exporter self-spans out of trace     |
|  [02]   | `setRPCMetadata` / `deleteRPCMetadata` / `getRPCMetadata`       | context key    | active HTTP-route metadata for span naming |

[ENTRYPOINT_SCOPE]: typed `OTEL_*` env readers
- rail: observability/config
- ONE typed-coercion reader family keyed by the target type, not four ad-hoc parsers: each reads an `OTEL_*` key from `process.env` (node) or `globalThis` (browser) and coerces, so the exporter/resource config never touches `process.env` directly. `diagLogLevelFromString` maps `OTEL_LOG_LEVEL` to the api `DiagLogLevel`.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `getStringFromEnv(key)`                      | env reader     | `OTEL_EXPORTER_OTLP_ENDPOINT` + string config    |
|  [02]   | `getNumberFromEnv(key)`                      | env reader     | timeout/batch-size numeric config                |
|  [03]   | `getBooleanFromEnv(key)`                     | env reader     | feature-flag booleans (default `false`)          |
|  [04]   | `getStringListFromEnv(key)`                  | env reader     | comma-list (`OTEL_RESOURCE_ATTRIBUTES`, headers) |
|  [05]   | `diagLogLevelFromString(value)`              | env reader     | `OTEL_LOG_LEVEL` → api `DiagLogLevel`            |
|  [06]   | `SDK_INFO` / `_globalThis` / `otperformance` | platform const | SDK name/version seed; cross-runtime global/perf |

[ENTRYPOINT_SCOPE]: `HrTime` conversion algebra + primitives
- rail: observability/time
- The span/metric timestamp algebra: one `api.HrTime` `[seconds, nanos]` tuple, N total conversions and constructors — a new time representation is another conversion arm, never a new clock type. `AnchoredClock` pins a monotonic clock to a wall-clock origin so long-lived spans keep drift-free durations. The remaining rows are the attribute-sanitizer, global-error-handler, and small-util primitives every SDK peer shares.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                  |
| :-----: | :-------------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `hrTime` / `millisToHrTime` / `timeInputToHrTime`               | constructor     | `HrTime` from perf-now, millis, or `TimeInput`       |
|  [02]   | `hrTimeToNanoseconds` / `-Microseconds` / `-Milliseconds`       | conversion      | `HrTime` → ns/µs/ms number                           |
|  [03]   | `hrTimeToSeconds` / `hrTimeToTimeStamp`                         | conversion      | `HrTime` → seconds or ISO string for OTLP            |
|  [04]   | `hrTimeDuration` / `addHrTimes` / `getTimeOrigin`               | algebra         | span duration + offset arithmetic on the tuple       |
|  [05]   | `isTimeInput` / `isTimeInputHrTime`                             | guard           | narrow `TimeInput` at the span-start boundary        |
|  [06]   | `new AnchoredClock(systemClock, monotonicClock)`                | clock           | drift-free `now()` timestamps for long-lived spans   |
|  [07]   | `sanitizeAttributes` / `isAttributeValue`                       | validator       | scrub/guard `Attributes` maps before a signal        |
|  [08]   | `globalErrorHandler` / `setGlobalErrorHandler`                  | error sink      | the process-wide exporter error handler + its setter |
|  [09]   | `loggingErrorHandler`                                           | error sink      | the default logging `ErrorHandler`                   |
|  [10]   | `internal._export(exporter, arg): Promise<ExportResult>`        | promise adapter | callback `Exporter<T>` → the `ExportResult` rail     |
|  [11]   | `merge` / `BindOnceFuture` / `callWithTimeout` + `TimeoutError` | util            | config-merge, once-shutdown future, deadline wrap    |
|  [12]   | `isUrlIgnored` / `urlMatches` / `unrefTimer`                    | util            | URL-filter predicates, timer unref                   |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge only, never the native lane: the facade's native `Otlp` lane owns its own W3C context bridge and serialization; core is composed only on the SDK-bridge path (`NodeSdk`/`WebSdk` `[OTEL_PIN_BLOCK]`) and on `otel/emit` for raw `traceparent` parsing. Prefer the native `Otlp` lane per the facade's `[OTEL_PIN_BLOCK]` law; reach for core's codecs only where the facade does not already own the extract-and-continue.
- one index, two platforms, never a fork: the `platform/{node,browser}` split under one index is a build-time selection (`process.env` vs `globalThis`) — a runtime change is a bundler condition, never a second import in design code.

[INTEGRATION_LAW]:
- Stack with `.api/effect-opentelemetry.md` `Tracer` (the primary seam): `otel/emit` reads the carrier header, calls `parseTraceParent(header)` → `SpanContext`, lifts `tracestate` via `new TraceState(raw)`, and hands both to `Tracer.makeExternalSpan({ traceId, spanId, traceFlags, traceState })` / `Tracer.withSpanContext` — core supplies the codec, the facade owns the Effect parent-span continuation. Root policy bans hand-rolled `traceparent` parsing, so `parseTraceParent` is the one admitted decoder.
- Stack with the sibling `[OTLP_SDK]` exporters + processors: `opentelemetry-exporter-trace-otlp-http`/`-metrics-otlp-http` and every SDK processor report through core's `ExportResult`/`ExportResultCode`; the exporters call `suppressTracing` on their own outbound HTTP `Context` so OTLP egress is never self-traced. `internal._export` is the callback→promise adapter at that seam.
- Stack with `.api/effect-platform.md` `HttpClient` posture: core is transport-agnostic (it carries no HTTP client); the SDK exporters that consume core bring their own `http`/`XMLHttpRequest` transport, so the SDK-bridge lane does NOT inherit the `net/client` retry/proxy policy the native `Otlp` lane gets — a real reason the native lane is `[OTEL_PIN_BLOCK]`-preferred.
- Stack with `opentelemetry-resources` + the facade `Resource`: `SDK_INFO` seeds the `telemetry.sdk.*` resource attributes that `defaultResource()` merges onto the `AppIdentity`-derived base; the typed env readers back `OTEL_RESOURCE_ATTRIBUTES` ingestion on the same resource.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); no other folder imports core. Instrumentation code uses Effect's native `Effect.withSpan`/`Metric`/`Effect.log` and never touches these primitives.
- core's `semconv` internal module is NOT on the package index — the semantic-convention vocabulary is owned by `@opentelemetry/semantic-conventions` (`telemetry/core/observe/convention`); do not transcribe core's internal `ATTR_*` constants.
- `_export`/`Exporter` sit under the `internal` namespace export — used only where the SDK-bridge lane adapts a callback exporter, never as a first-class instrumentation surface.

[RAIL_LAW]:
- Package: `@opentelemetry/core`
- Owns: the concrete W3C `TextMapPropagator` triad + raw `traceparent`/`tracestate`/`baggage` codecs, the `ExportResult`/`ExportResultCode` export rail, the `suppressTracing`/`RPCMetadata` context-key operators, the typed `OTEL_*` env readers, the `HrTime` conversion algebra, and shared attribute/error/util primitives
- Accept: `parseTraceParent` + `TraceState` feeding the facade's `makeExternalSpan`/`withSpanContext` on `otel/emit`; `CompositePropagator` as the one folded propagator; `ExportResult` as the terminal export disposition; typed env readers over raw `process.env`; core composed only on the SDK-bridge/context path
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, hand-rolled `traceparent`/`tracestate` parsing, core codecs where the native `Otlp` lane already owns the seam, transcribing core's internal `semconv` constants (that is `semantic-conventions`), treating the runtime `platform/*` split as a fork
