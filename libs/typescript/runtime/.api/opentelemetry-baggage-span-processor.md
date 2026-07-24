# [TS_RUNTIME_API_OPENTELEMETRY_BAGGAGE_SPAN_PROCESSOR]

`@opentelemetry/baggage-span-processor` contributes one `SpanProcessor` promoting baggage onto spans: `BaggageSpanProcessor.onStart` reads the parent context's `Baggage` and stamps each predicate-admitted key-value pair as a span attribute, so a `rasm.tenant` identity rides every child span. Its mandatory `BaggageKeyPredicate` refuses foreign promotion by construction, never by after-the-fact scrub.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/baggage-span-processor`
- package: `@opentelemetry/baggage-span-processor` (Apache-2.0)
- module: the `BaggageSpanProcessor` class, `BaggageKeyPredicate` type, and `ALLOW_ALL_BAGGAGE_KEYS` default — no bundled peer, no global patch
- depends: `@opentelemetry/sdk-trace-base` for `SpanProcessor`; peer `@opentelemetry/api` for `Baggage`/`Context`
- runtime: lane-neutral — node and browser tracer providers both accept the processor row
- rail: observability/trace

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the baggage promotion filter

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `BaggageKeyPredicate`    | type alias    | `(baggageKey: string) => boolean` gate; `rasm.*` pass, rest fail |
|  [02]   | `ALLOW_ALL_BAGGAGE_KEYS` | const         | permissive default, unwired; the closed `rasm.*` predicate wins  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the span processor — Rasm constructs it once; the SDK drives every lifecycle member

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `new BaggageSpanProcessor(BaggageKeyPredicate)` | ctor     | contributed once to the tracer provider; predicate keys `rasm.*` |
|  [02]   | `.onStart(Span, Context)`                       | instance | reads parent `Baggage`, stamps admitted pairs as span attributes |
|  [03]   | `.onEnd(ReadableSpan)`                          | instance | SDK-driven no-op; promotion is a start-time act                  |
|  [04]   | `.forceFlush()` / `.shutdown()`                 | instance | `Promise<void>`; SDK owns drain and teardown                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `onStart` promotes at span start alone — it walks the parent `Baggage`, tests each key against the predicate, writes admitted pairs as attributes; `onEnd` is inert, so the read-only ended span is never touched.
- Constructor demands a predicate, so every promotion set is explicit: the `rasm.*` allow-list is the value and `ALLOW_ALL_BAGGAGE_KEYS` stays unwired against the cardinality governor.

[STACKING]:
- `@opentelemetry/sdk-trace-base`(`.api/opentelemetry-sdk-trace-base.md`): `BaggageSpanProcessor implements SpanProcessor`, contributed into `TracerConfig.spanProcessors: SpanProcessor[]`; the SDK drives its `onStart`/`onEnd`/`forceFlush`/`shutdown`.
- `@opentelemetry/api`(`.api/opentelemetry-api.md`): `onStart` reads the parent `Context`'s `Baggage`, the peer contract it promotes from into span attributes.
- `otel/emit.md` `Hooks.contribute`: one `SpanProcessor` row folded behind the policy's own; identity scopes the tracer provider, so per-app promotion never tangles across a multi-app host.
- `otel/emit.md` `Propagation.ingress`: ingress decodes the `baggage` header into the context `Baggage` this reads, closing the loop propagation opens — an upstream tenant set rides span attributes with zero emit-site change.
- `otel/emit.md` `Redaction`: promotion writes attributes before the span freezes, so the export scrub still governs — a promoted key matching a deny rule seals at `onEnding`.
- `otel/meter.md` `Pulse.tenants`: the promoted `rasm.tenant` attribute is the walk's first hop — baggage to span to metric view under the shared cardinality ceiling.

[LOCAL_ADMISSION]:
- `scope:runtime`, lane-neutral; the app composition root builds the predicate from the promotion key set and contributes the processor once — a library-altitude construction double-stamps the span.

[RAIL_LAW]:
- Package: `@opentelemetry/baggage-span-processor`
- Owns: parent-baggage to span-attribute promotion under an explicit key predicate
- Accept: one construction keyed to the `rasm.*` promotion set, contributed as a `Hooks` `SpanProcessor` row
- Reject: `ALLOW_ALL_BAGGAGE_KEYS` promotion, a hand-rolled `onStart` bridge, library-altitude construction, or sensitive keys promoted into outbound baggage headers
