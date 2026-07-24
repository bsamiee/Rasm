# [TS_RUNTIME_API_OPENTELEMETRY_API]

`@opentelemetry/api` is the vendor-neutral contract the whole `@opentelemetry/*` tree peers on: the trace, metric, context, propagation, and diag type shapes every SDK implements, the five global entry singletons, and the wire constants and guards. It ships no SDK — every global resolves to a no-op until a provider registers, so inside Rasm it is a type source only, with span and metric emission riding the Effect facade.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/api`
- package: `@opentelemetry/api` (Apache-2.0)
- module: ESM; the dependency-free leaf every `@opentelemetry/*` and `@effect/opentelemetry` peer ranges over
- runtime: neutral — node, bun, browser
- rail: observability/contract

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace, metric, context, and propagation contracts every SDK implements and Rasm names in type position only

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------- | :---------------- | :----------------------------------- |
|  [01]   | `SpanContext` / `Span` / `SpanOptions` / `SpanKind`      | trace contract    | span identity and creation shape     |
|  [02]   | `Tracer` / `TracerProvider` / `TraceState`               | trace contract    | tracer handle and trace-state value  |
|  [03]   | `TraceFlags` / `SpanStatusCode` / `INVALID_SPAN_CONTEXT` | trace constant    | sampled-flag and context sentinels   |
|  [04]   | `Context` / `ContextManager` / `ROOT_CONTEXT`            | context contract  | async-context propagation contract   |
|  [05]   | `TextMapPropagator` / `TextMapSetter` / `TextMapGetter`  | propagation shape | carrier-polymorphic inject/extract   |
|  [06]   | `Attributes` / `AttributeValue` / `HrTime` / `TimeInput` | value primitive   | attribute bags and signal timestamps |
|  [07]   | `Meter` / `MeterProvider`                                | metric contract   | meter handle instruments mint on     |
|  [08]   | `Counter` / `Gauge` / `Histogram` / `UpDownCounter`      | instrument types  | typed metric writes                  |
|  [09]   | `DiagLogger` / `DiagLogLevel` / `DiagConsoleLogger`      | diag contract     | SDK self-diagnostics only            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: global singletons over the guard and mint tier; Rasm admits guards, mints, and one global bracket — the browser boot's `context.setGlobalContextManager`/`context.disable()` pair inside the `Instrument` node

| [INDEX] | [SURFACE]                                                      | [SHAPE]   | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------- | :-------- | :----------------------------------- |
|  [01]   | `trace` / `metrics` / `context` / `propagation` / `diag`       | singleton | SDK-internal global entry            |
|  [02]   | `isSpanContextValid(ctx)` / `isValidTraceId` / `isValidSpanId` | guard     | validate span contexts as values     |
|  [03]   | `createTraceState(raw?)`                                       | factory   | `tracestate` value construction      |
|  [04]   | `createContextKey(desc)` / `createNoopMeter()`                 | factory   | context-key and null-object plumbing |
|  [05]   | `baggageEntryMetadataFromString(value)`                        | factory   | baggage metadata admission           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one tree copy — the API registers its globals in a versioned `globalThis` slot, so a duplicated install splits registration and `trace.getTracer` on the orphan returns a no-op; the workspace catalog pins one version and dedupe is the admission proof.
- type-position only — Rasm instrumentation is Effect-native (`Effect.withSpan`, `Metric.*`, `Effect.log*`); a `trace.getTracer` span bypasses the fiber-propagated context and is the named defect. Async context is process-global, so the facade registers no manager and the tracer and meter globals stay untouched.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): the facade implements these contracts and owns registration; `Tracer.currentOtelSpan` is the one sanctioned reach from Effect into an api `Span` where a third-party surface demands one.
- `@opentelemetry/context-zone`(`.api/opentelemetry-context-zone.md`): `ZoneContextManager` implements this package's `ContextManager` and hands it to the `web` SDK row's context wiring.
- `@opentelemetry/core`(`.api/opentelemetry-core.md`): the propagator triad implements `TextMapPropagator`; `parseTraceParent` yields this package's `SpanContext` already rejecting malformed and all-zero ids, so the guard tier never re-checks header decode.
- `otel/emit` (within-lib): the export-boundary owner consumes `SpanContext` on its continuation kernel and `TextMapPropagator` at transports in type position, the facade owning span and metric semantics through Effect so no `trace.getTracer` call crosses the seam.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits only inside `scope:runtime`; the browser instrumentation rows the app root composes reach it there in type position too.

[RAIL_LAW]:
- Package: `@opentelemetry/api`
- Owns: the vendor-neutral trace, metric, context, propagation, and diag contracts, the global registration slot, and the wire constants and guards
- Accept: type imports at SDK-bridge and continuation seams; guard and mint calls on span contexts arriving as values; the browser boot's one `context.setGlobalContextManager`/`context.disable()` bracket; one deduped tree copy
- Reject: `trace.getTracer`/`metrics.getMeter` spans or instruments in Rasm code, a second tree copy, diag as a domain log channel
