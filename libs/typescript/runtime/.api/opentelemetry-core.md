# [TS_RUNTIME_API_OPENTELEMETRY_CORE]

`@opentelemetry/core` is the dependency-free primitive layer beneath every `@opentelemetry/sdk-*` and `exporter-*` peer — the W3C propagation codecs, the `ExportResult` export rail, and the `HrTime` timestamp algebra those peers build on.

`otel/emit` composes its codecs to continue an inbound `traceparent` into the `@effect/opentelemetry` facade, and the SDK-bridge lane reports through its `ExportResult` rail; the native `Otlp` lane owns its own context bridge, so core is never reached there.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/core`
- package: `@opentelemetry/core` (Apache-2.0)
- module: ESM + CJS, single index; the `internal` namespace export carries the SDK-seam adapters
- runtime: isomorphic — `node` reads `process.env`, `browser` reads `globalThis`; propagation, export, and time surfaces are runtime-neutral
- depends: `@opentelemetry/api` (peer, the `Context`/`SpanContext`/`TextMapPropagator`/`HrTime`/`Attributes` type source); `@opentelemetry/semantic-conventions` (pure-data, sole runtime dep)
- rail: observability primitive substrate every `@opentelemetry/sdk-*`/`exporter-*` peer builds on

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: W3C propagation carriers

Propagators are one parameterized `api.TextMapPropagator` family (`inject`/`extract`/`fields`); `CompositePropagator` folds an ordered `propagators[]` into one, so a new wire format is a `TextMapPropagator` row in the composite config. `TraceState` is the immutable `tracestate` carrier the facade's `makeExternalSpan({ traceState })` accepts.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]     | [CAPABILITY]                              |
| :-----: | :------------------------------------ | :---------------- | :---------------------------------------- |
|  [01]   | `W3CTraceContextPropagator`           | propagator        | W3C trace-context inject+extract          |
|  [02]   | `W3CBaggagePropagator`                | propagator        | W3C `baggage` inject+extract              |
|  [03]   | `CompositePropagator`                 | propagator monoid | fold ordered propagators into one         |
|  [04]   | `CompositePropagatorConfig`           | config            | the ordered `propagators?` set            |
|  [05]   | `TraceState`                          | `tracestate` list | immutable `set`/`unset`/`get`/`serialize` |
|  [06]   | `TRACE_PARENT_HEADER = "traceparent"` | header const      | the `traceparent` header key              |
|  [07]   | `TRACE_STATE_HEADER = "tracestate"`   | header const      | the `tracestate` header key               |

[PUBLIC_TYPE_SCOPE]: export rail + diagnostic carriers

Every exporter and SDK processor reports terminal disposition through `ExportResult`/`ExportResultCode`, the single result rail the SDK-bridge lane surfaces; `internal._export` adapts a callback `Exporter<T>` into a `Promise<ExportResult>` at the SDK seam.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]      | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `ExportResult { code: ExportResultCode; error?: Error }` | result carrier     | terminal exporter disposition       |
|  [02]   | `ExportResultCode { SUCCESS = 0, FAILED = 1 }`           | result enum        | two-state export outcome            |
|  [03]   | `internal.Exporter<T>`                                   | exporter interface | callback exporter the adapter wraps |
|  [04]   | `RPCMetadata` (alias `HTTPMetadata`)                     | context-key value  | active HTTP-route metadata          |
|  [05]   | `RPCType { HTTP }`                                       | enum               | the route-type discriminant         |
|  [06]   | `InstrumentationScope { name; version?; schemaUrl? }`    | scope record       | emitting-scope identity             |
|  [07]   | `ErrorHandler = (ex: Exception) => void`                 | callback type      | the global error-sink type          |
|  [08]   | `Clock { now(): number }`                                | clock interface    | monotonic clock for `AnchoredClock` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: W3C extract-and-continue codecs

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `parseTraceParent(traceParent: string) -> SpanContext \| null` | codec   | inbound `traceparent` for `makeExternalSpan`  |
|  [02]   | `parseKeyPairsIntoRecord(value?: string)`                      | codec   | inbound `baggage` to a record                 |
|  [03]   | `new TraceState(rawTraceState?: string)`                       | ctor    | `tracestate` round-trip; `serialize()` egress |
|  [04]   | `new CompositePropagator({ propagators })`                     | ctor    | one propagator over the row set               |

[ENTRYPOINT_SCOPE]: `Context`-key operators

Two immutable `Context`-key families, each a `(Context) -> Context` writer with a matching reader: `suppressTracing` fences an exporter's own HTTP calls out of trace, `setRPCMetadata` carries active HTTP-route data for span naming.

| [INDEX] | [SURFACE]                                                       | [SHAPE]     | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :---------- | :----------------------------------------- |
|  [01]   | `suppressTracing` / `unsuppressTracing` / `isTracingSuppressed` | context key | fence exporter self-spans out of trace     |
|  [02]   | `setRPCMetadata` / `deleteRPCMetadata` / `getRPCMetadata`       | context key | active HTTP-route metadata for span naming |

[ENTRYPOINT_SCOPE]: typed `OTEL_*` env readers

One typed-coercion reader family keyed by the target type: each reads an `OTEL_*` key from `process.env` (node) or `globalThis` (browser) and coerces, so exporter and resource config never touch `process.env` directly.

| [INDEX] | [SURFACE]                                    | [SHAPE]        | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `getStringFromEnv(key)`                      | env reader     | `OTEL_EXPORTER_OTLP_ENDPOINT` + string cfg |
|  [02]   | `getNumberFromEnv(key)`                      | env reader     | timeout/batch-size numeric config          |
|  [03]   | `getBooleanFromEnv(key)`                     | env reader     | feature-flag booleans (default `false`)    |
|  [04]   | `getStringListFromEnv(key)`                  | env reader     | comma-list (`OTEL_RESOURCE_ATTRIBUTES`)    |
|  [05]   | `diagLogLevelFromString(value)`              | env reader     | `OTEL_LOG_LEVEL` to api `DiagLogLevel`     |
|  [06]   | `SDK_INFO` / `_globalThis` / `otperformance` | platform const | SDK name/version seed; runtime global/perf |

[ENTRYPOINT_SCOPE]: `HrTime` conversion algebra + primitives

Timestamp conversion folds one `api.HrTime` `[seconds, nanos]` tuple: a new time representation is another conversion arm on it. `AnchoredClock` pins a monotonic clock to a wall-clock origin so long-lived spans keep drift-free durations.

| [INDEX] | [SURFACE]                                                       | [SHAPE]         | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `hrTime` / `millisToHrTime` / `timeInputToHrTime`               | constructor     | `HrTime` from perf-now, millis, or `TimeInput`    |
|  [02]   | `hrTimeToNanoseconds` / `-Microseconds` / `-Milliseconds`       | conversion      | `HrTime` to ns/µs/ms number                       |
|  [03]   | `hrTimeToSeconds` / `hrTimeToTimeStamp`                         | conversion      | `HrTime` to seconds or ISO string                 |
|  [04]   | `hrTimeDuration` / `addHrTimes` / `getTimeOrigin`               | algebra         | span duration + offset arithmetic                 |
|  [05]   | `isTimeInput` / `isTimeInputHrTime`                             | guard           | narrow `TimeInput` at span start                  |
|  [06]   | `new AnchoredClock(systemClock, monotonicClock)`                | clock ctor      | drift-free `now()` for long-lived spans           |
|  [07]   | `sanitizeAttributes` / `isAttributeValue`                       | validator       | scrub/guard `Attributes` before a signal          |
|  [08]   | `globalErrorHandler` / `setGlobalErrorHandler`                  | error sink      | process-wide exporter error handler + setter      |
|  [09]   | `loggingErrorHandler`                                           | error sink      | the default logging `ErrorHandler`                |
|  [10]   | `internal._export(exporter, arg) -> Promise<ExportResult>`      | promise adapter | callback `Exporter<T>` to the `ExportResult` rail |
|  [11]   | `merge` / `BindOnceFuture` / `callWithTimeout` + `TimeoutError` | util            | config-merge, once-shutdown future, deadline wrap |
|  [12]   | `isUrlIgnored` / `urlMatches` / `unrefTimer`                    | util            | URL-filter predicates, timer unref                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Core composes only on the SDK-bridge path (`NodeSdk`/`WebSdk`) and on `otel/emit`'s raw `traceparent` parse; the native `Otlp` lane owns its own W3C context bridge and serialization, so core's codecs are reached only where the facade does not already own the extract-and-continue.
- One build-time bundler condition resolves the `platform/{node,browser}` split under a single index (`process.env` vs `globalThis`); design code imports one surface regardless of runtime.
- `[OTEL_PIN_BLOCK]`: core is the collapse member of the `[OTLP_SDK]` pin block native `Otlp` parity retires; `@opentelemetry/semantic-conventions` persists as substrate, this does not.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `otel/emit` calls `parseTraceParent(header) -> SpanContext`, lifts `tracestate` via `new TraceState(raw)`, and hands both to `Tracer.makeExternalSpan({ traceId, spanId, traceFlags, traceState })` / `Tracer.withSpanContext` — core supplies the codec, the facade owns the Effect parent-span continuation, and `parseTraceParent` is the one admitted `traceparent` decoder.
- `opentelemetry-exporter-trace-otlp-http`(`.api/opentelemetry-exporter-trace-otlp-http.md`): the OTLP exporters and every SDK processor report through core's `ExportResult`/`ExportResultCode`, call `suppressTracing` on their outbound HTTP `Context` so egress is never self-traced, and adapt callback exporters through `internal._export`.
- `@effect/platform`(`.api/effect-platform.md`): core carries no HTTP client, so the SDK exporters bring their own `http`/`XMLHttpRequest` transport and the SDK-bridge lane does not inherit the `net/client` retry/proxy policy the native `Otlp` lane gets.
- `opentelemetry-resources`(`.api/opentelemetry-resources.md`): `SDK_INFO` seeds the `telemetry.sdk.*` attributes `defaultResource()` merges onto the `AppIdentity`-derived base, and the typed env readers back `OTEL_RESOURCE_ATTRIBUTES` ingestion on the same resource.
- `otel/emit` (within-lib): the export-boundary owner composes core's codecs at every ingress for extract-and-continue and surfaces core's `ExportResult` rail on the SDK-bridge lane.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger); every other folder emits through Effect's native `Effect.withSpan`/`Metric`/`Effect.log` and never imports core.
- core's `semconv` internal module stays off the package index — `@opentelemetry/semantic-conventions` (`telemetry/core/observe/convention`) owns the semantic-convention vocabulary, so core's internal `ATTR_*` constants are never transcribed.
- `_export`/`Exporter` sit under the `internal` namespace export, used only where the SDK-bridge lane adapts a callback exporter, never as a first-class instrumentation surface.

[RAIL_LAW]:
- Package: `@opentelemetry/core`
- Owns: the W3C `TextMapPropagator` codecs, the `ExportResult`/`ExportResultCode` export rail, the `suppressTracing`/`RPCMetadata` context-key operators, the typed `OTEL_*` env readers, and the `HrTime` conversion algebra with the shared attribute/error/util primitives every SDK peer reuses
- Accept: `parseTraceParent` + `TraceState` feeding `makeExternalSpan`/`withSpanContext` on `otel/emit`; `CompositePropagator` as the one folded propagator; `ExportResult` as the terminal export disposition; typed env readers over raw `process.env`; core composed only on the SDK-bridge/context path
- Reject: `@opentelemetry/*` outside `scope:runtime`, hand-rolled `traceparent`/`tracestate` parsing, core codecs where the native `Otlp` lane already owns the seam, transcribing core's internal `semconv` constants, treating the `platform/*` split as a fork
