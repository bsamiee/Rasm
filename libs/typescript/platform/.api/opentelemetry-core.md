# [API_CATALOGUE] @opentelemetry/core

`@opentelemetry/core` supplies the OpenTelemetry SDK utility layer: the W3C Trace Context and Baggage `TextMapPropagator` implementations (`W3CTraceContextPropagator`, `W3CBaggagePropagator`), the `CompositePropagator` that runs an ordered list of propagators as one, the `traceparent` parse helper (`parseTraceParent`), and the `TRACE_PARENT_HEADER`/`TRACE_STATE_HEADER` header-name constants. The propagators implement the `@opentelemetry/api` `TextMapPropagator` contract (`inject`/`extract`/`fields`). This package supplies the propagator CLASSES only; REGISTRATION is the consumer's seam choice (the `@effect/opentelemetry` `WebSdk.Configuration` has NO `propagator` field), realized either through a self-constructed `@opentelemetry/sdk-trace-web` `WebTracerProvider.register({ propagator })` or through the `@opentelemetry/api` `propagation.setGlobalPropagator` global registration — never a non-existent WebSDK `DefaultTextMapPropagator` field.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/core`
- package: `@opentelemetry/core`
- module: `@opentelemetry/core` (barrel re-export from `build/src/index.d.ts`)
- asset: runtime library
- rail: observability

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: propagator types and config
- rail: observability

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------ |
|   [1]   | `W3CTraceContextPropagator` | class         | `TextMapPropagator` for `traceparent`/`tracestate`      |
|   [2]   | `W3CBaggagePropagator`      | class         | `TextMapPropagator` for the W3C `baggage` header        |
|   [3]   | `CompositePropagator`       | class         | ordered fan-out of a `TextMapPropagator[]` as one       |
|   [4]   | `CompositePropagatorConfig` | interface     | `{ propagators?: TextMapPropagator[] }` constructor arg |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: W3C trace-context propagator
- rail: observability

```ts
// build/src/trace/W3CTraceContextPropagator.d.ts
export declare const TRACE_PARENT_HEADER = "traceparent";
export declare const TRACE_STATE_HEADER = "tracestate";

export declare function parseTraceParent(traceParent: string): SpanContext | null;

export declare class W3CTraceContextPropagator implements TextMapPropagator {
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}
```

[ENTRYPOINT_SCOPE]: W3C baggage propagator
- rail: observability

```ts
// build/src/baggage/propagation/W3CBaggagePropagator.d.ts
export declare class W3CBaggagePropagator implements TextMapPropagator {
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}
```

[ENTRYPOINT_SCOPE]: composite propagator
- rail: observability

```ts
// build/src/propagation/composite.d.ts
export interface CompositePropagatorConfig {
  propagators?: TextMapPropagator[];
}

export declare class CompositePropagator implements TextMapPropagator {
  constructor(config?: CompositePropagatorConfig);
  inject(context: Context, carrier: unknown, setter: TextMapSetter): void;
  extract(context: Context, carrier: unknown, getter: TextMapGetter): Context;
  fields(): string[];
}
```

## [4]-[IMPLEMENTATION_LAW]

[PROPAGATION_TOPOLOGY]:
- a propagator implements the `@opentelemetry/api` `TextMapPropagator` triple — `inject` writes the active `Context` onto an outbound carrier, `extract` reads an inbound carrier into a `Context`, `fields` lists the header names it owns
- `CompositePropagator` runs its `propagators` list in order; a later propagator writing the same context/carrier key wins, so a `[W3CTraceContextPropagator, W3CBaggagePropagator]` composite injects both `traceparent`/`tracestate` and `baggage` headers from one propagator handle
- `parseTraceParent` decodes a `traceparent` header string to a `SpanContext` or `null` on a malformed value; it never throws

[LOCAL_ADMISSION]:
- the composite is registered through the consumer's chosen seam — a self-constructed `@opentelemetry/sdk-trace-web` `WebTracerProvider.register({ propagator })` (in the current package set) or the `@opentelemetry/api` `propagation.setGlobalPropagator` global registration (an added `@opentelemetry/api` admission) — so every inbound carrier seeds the span from the extracted parent rather than a fresh root. The `@effect/opentelemetry` `WebSdk.layer` builds its `WebTracerProvider` internally and exposes no `.register` handle, so registration on the WebSdk-built provider is unreachable; the registration is a standalone-provider or global call composed alongside the WebSdk export layer.
- `TRACE_PARENT_HEADER`/`TRACE_STATE_HEADER` are the canonical header names a transport interceptor injects under, never a hand-written `"traceparent"` literal
- the propagator is paired with a `ParentBasedSampler` so the extracted parent's sampling decision is honored and a root span samples at the configured ratio
- `MetricRegistry.tracePropagation` (`observability/metric-registry#TRACE_PROPAGATION`) registers extract-and-continue ONLY; the outbound inject leg is settled at `interchange/transport/transport#WIRE_TRANSPORT`

[RAIL_LAW]:
- Package: `@opentelemetry/core`
- Owns: W3C trace-context and baggage propagation, the composite propagator fan-out, and the `traceparent` parse/header constants
- Accept: `W3CTraceContextPropagator`/`W3CBaggagePropagator` composed in a `CompositePropagator`, registered through a self-constructed `WebTracerProvider.register({ propagator })` or `propagation.setGlobalPropagator`
- Reject: a hand-rolled `traceparent` header build/parse, a per-request fresh root span where an inbound parent context exists, a `"traceparent"` string literal in place of `TRACE_PARENT_HEADER`
