# [TS_RUNTIME_API_OPENTELEMETRY_BAGGAGE_SPAN_PROCESSOR]

`@opentelemetry/baggage-span-processor` is the contributed `SpanProcessor` promoting baggage onto spans: `BaggageSpanProcessor` reads the parent context's `Baggage` at span start and stamps each admitted key-value pair as a span attribute, so a `rasm.tenant` identity rides every child span without a hand-rolled `onStart` fold. Its `BaggageKeyPredicate` — a `(key) => boolean` the constructor demands — refuses foreign promotion by construction, never by after-the-fact scrub. Side-effect-free, it depends only on `sdk-trace-base` for the processor contract and the api peer for `Baggage`/`Context`, composing as one `Hooks.contribute` row on either lane's tracer provider.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/baggage-span-processor`
- package: `@opentelemetry/baggage-span-processor` (Apache-2.0)
- composition: one processor class, a predicate type, and its permissive default; no bundled peer, no global patch
- depends: `@opentelemetry/sdk-trace-base` for the `SpanProcessor` contract; peer `@opentelemetry/api` for `Baggage`/`Context`
- consumed-by: `otel/emit.md` `[04]-[HOOKS]` as one contributed `SpanProcessor` tap, replacing the described-but-unshipped tenant `onStart` bridge
- runtime: lane-neutral — node and browser tracer providers both accept the processor row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the baggage promotion filter
- rail: observability/trace

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]                     | [CONSUMER_BOUNDARY]                                            |
| :-----: | :----------------------- | :-------------------------------- | :------------------------------------------------------------- |
|  [01]   | `BaggageKeyPredicate`    | `(baggageKey: string) => boolean` | constructor-demanded gate; `rasm.*` admit, foreign refuse      |
|  [02]   | `ALLOW_ALL_BAGGAGE_KEYS` | `BaggageKeyPredicate` const       | permissive default; Rasm never wires it (closed key predicate) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the span processor
- rail: observability/trace
- One construction feeds `Hooks.contribute`; the constructor's predicate is the entire policy surface — the SDK drives the four lifecycle members, and Rasm code touches only the constructor.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                           |
| :-----: | :------------------------------------ | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `new BaggageSpanProcessor(predicate)` | ctor           | one row contributed to the tracer provider; predicate keys the `rasm.*` set   |
|  [02]   | `.onStart(span, parentContext)`       | lifecycle      | SDK-driven — reads parent `Baggage`, stamps admitted pairs as span attributes |
|  [03]   | `.onEnd(span)`                        | lifecycle      | SDK-driven no-op; promotion is a start-time act                               |
|  [04]   | `.forceFlush()` / `.shutdown()`       | lifecycle      | `Promise<void>`; SDK drain and teardown own the bracket                       |

## [04]-[IMPLEMENTATION_LAW]

[PROMOTION_TOPOLOGY]:
- Start-time only: `onStart` walks the parent context's `Baggage`, tests each key against the predicate, and writes the admitted pairs as attributes on the starting span; `onEnd` is inert, so promotion never touches the read-only ended span.
- Predicate is the closed gate: the constructor accepts no processor without one, so a promotion set is always explicit — the `rasm.*` allow-list is the value, and `ALLOW_ALL_BAGGAGE_KEYS` stays unused because unbounded promotion breaches the cardinality governor.

[INTEGRATION_LAW]:
- Stack with `otel/emit.md` `Propagation.ingress`: ingress decodes the `baggage` header into the context Baggage this processor reads, closing the loop propagation opens — a tenant set upstream rides span attributes downstream with zero emit-site changes.
- Stack with `otel/emit.md` `Hooks`: the processor is one `Hooks.contribute` `SpanProcessor` row folded behind the policy's own; identity scopes the tracer provider, so per-app baggage promotion never tangles across a multi-app host.
- Stack with `otel/meter.md` `Pulse.tenants`: the promoted `rasm.tenant` span attribute is the walk's first hop — baggage to span to metric view — under the three-tier cardinality ceiling every reader inherits, so tenant cost slices exist as governed streams.
- Stack with `otel/emit.md` `Redaction`: promotion writes attributes before the span freezes, so the export-boundary scrub still governs — a promoted key matching a deny rule is sealed at `onEnding`, keeping baggage promotion inside the one scrub law.

[LOCAL_ADMISSION]:
- `scope:runtime`, lane-neutral; the app composition root builds the predicate from the promotion key set and contributes the processor once — a library-altitude construction double-stamps the span.

[RAIL_LAW]:
- Package: `@opentelemetry/baggage-span-processor`
- Owns: parent-baggage to span-attribute promotion under an explicit key predicate
- Accept: one construction keyed to the `rasm.*` promotion set, contributed as a `Hooks` `SpanProcessor` row
- Reject: `ALLOW_ALL_BAGGAGE_KEYS` promotion, a hand-rolled `onStart` bridge beside it, library-altitude construction, promotion of sensitive keys into outbound baggage headers
