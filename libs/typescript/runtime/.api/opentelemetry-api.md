# [TS_RUNTIME_API_OPENTELEMETRY_API]

`@opentelemetry/api` is the vendor-neutral contract package the whole `@opentelemetry/*` tree peers on: the five global entry singletons (`trace`, `metrics`, `context`, `propagation`, `diag`), the type surfaces every SDK implements (`Tracer`/`Span`/`SpanContext`, `Meter`/`Counter`/`Histogram`, `Context`/`ContextManager`, `TextMapPropagator`), and the wire-level constants (`TraceFlags`, `INVALID_SPAN_CONTEXT`). It ships no SDK: every global resolves to a no-op until a provider registers. Inside Rasm it is a TYPE source and dedupe anchor — the facade owns span/metric semantics through Effect, `otel/emit` consumes `SpanContext` and `TextMapPropagator`-adjacent types from here, and exactly one copy may exist in the tree because a second copy's `trace.getTracer` answers no-ops.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/api`
- package: `@opentelemetry/api`
- license: `Apache-2.0`
- dependencies: none — the leaf every `@opentelemetry/*` and `@effect/opentelemetry` peer ranges over
- consumed-by: `otel/emit` (`SpanContext` type on the continuation kernel), every SDK-bridge row transitively; the dedupe law binds the whole tree
- runtime: neutral — globals register through a versioned `globalThis` slot, so API and SDK copies must resolve to one instance
- module-families: global entry singletons (`trace`/`metrics`/`context`/`propagation`/`diag`), trace types + constants, metric instrument types, context/propagation contracts, diag logging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace + context + propagation contracts
- rail: observability/contract
- Every SDK implements and every instrumentation consumes these shapes; Rasm code names them in type position only — span creation rides `Effect.withSpan`, never `trace.getTracer`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `SpanContext` / `Span` / `SpanOptions` / `SpanKind`      | trace contract    | `otel/emit` continuation kernel; facade span bridging  |
|  [02]   | `Tracer` / `TracerProvider` / `TraceState`               | trace contract    | SDK rows implement; `createTraceState` mints the value |
|  [03]   | `TraceFlags` / `SpanStatusCode` / `INVALID_SPAN_CONTEXT` | trace constant    | sampled-flag and invalid-context checks                |
|  [04]   | `Context` / `ContextManager` / `ROOT_CONTEXT`            | context contract  | `ZoneContextManager` implements; SDK context plumbing  |
|  [05]   | `TextMapPropagator` / `TextMapSetter` / `TextMapGetter`  | propagation shape | carrier-polymorphic inject/extract at transports       |
|  [06]   | `Attributes` / `AttributeValue` / `HrTime` / `TimeInput` | value primitive   | attribute bags and timestamps across every signal      |
|  [07]   | `Meter` / `MeterProvider`                                | metric contract   | third-party instruments mint on `Hooks.Meter`          |
|  [08]   | `Counter` / `Gauge` / `Histogram` / `UpDownCounter`      | instrument types  | typed writes on the minted meter                       |
|  [09]   | `DiagLogger` / `DiagLogLevel` / `DiagConsoleLogger`      | diag contract     | SDK self-diagnostics; never a domain log channel       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: global singletons + guards
- rail: observability/contract
- Rasm composition admits the guard/mint tier and exactly one global bracket: the browser boot's `context.setGlobalContextManager`/`context.disable()` pair inside the `Instrument` node — async context is process-global and the facade registers no manager; tracer and meter globals stay untouched.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                  |
| :-----: | :------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `trace` / `metrics` / `context` / `propagation` / `diag`       | global entry   | SDK-internal; `context` admits the zone bracket      |
|  [02]   | `isSpanContextValid(ctx)` / `isValidTraceId` / `isValidSpanId` | guard          | guard span contexts arriving as values, never decode |
|  [03]   | `createTraceState(raw?)`                                       | mint           | `tracestate` value construction                      |
|  [04]   | `createContextKey(desc)` / `createNoopMeter()`                 | mint           | context-key and null-object plumbing at SDK seams    |
|  [05]   | `baggageEntryMetadataFromString(value)`                        | mint           | baggage metadata admission                           |

## [04]-[IMPLEMENTATION_LAW]

[CONTRACT_TOPOLOGY]:
- one tree copy — the API registers its globals in a versioned `globalThis` slot, so a duplicated install splits registration and `trace.getTracer` on the orphan copy returns a no-op; the workspace catalog pins one version and dedupe is the admission proof.
- type-position only — Rasm instrumentation is Effect-native (`Effect.withSpan`, `Metric.*`, `Effect.log*`); a `trace.getTracer` span bypasses the fiber-propagated span context and is the named defect.

[INTEGRATION_LAW]:
- Stack with `.api/effect-opentelemetry.md`: the facade implements these contracts and owns registration; `Tracer.currentOtelSpan` is the one sanctioned reach from Effect into an api `Span` where a third-party surface demands one.
- Stack with `opentelemetry-context-zone.md`: `ZoneContextManager` implements this package's `ContextManager` and hands it to the `web` SDK row's context wiring.
- Stack with `opentelemetry-core.md`: the propagator triad implements `TextMapPropagator`; `parseTraceParent` yields this package's `SpanContext` and already rejects malformed and all-zero ids, so the guard tier never re-checks header decode.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted only inside `scope:runtime`; type positions in the browser instrumentation rows the app root composes reach it there too.

[RAIL_LAW]:
- Package: `@opentelemetry/api`
- Owns: the vendor-neutral trace/metric/context/propagation/diag contracts, the global registration slot, the wire constants and guards
- Accept: type imports at the SDK-bridge and continuation seams; guard/mint calls (`isSpanContextValid`, `createTraceState`) on span contexts arriving as values; the browser boot's one `context.setGlobalContextManager`/`context.disable()` bracket; one deduped tree copy
- Reject: `trace.getTracer`/`metrics.getMeter` spans or instruments in Rasm code (Effect owns the fiber context), a second tree copy, diag as a domain log channel
