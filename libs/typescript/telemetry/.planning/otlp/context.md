# [TELEMETRY_CONTEXT]

Causal identity crosses every ingress through one `Propagation` owner: the W3C `traceparent`/`tracestate`/`baggage` triple decodes from any string-keyed carrier — edge middleware headers, `work` entity envelopes, `browser` boot state, worker marshal frames — into an `Option`-carried parent, and the same owner stamps the outbound triple on non-HTTP carriers so a trace survives every hop the platform client does not already cover. Absence is normal, never a fault: an ingress without a `traceparent` starts a fresh root, so extraction returns `Option` and the continuation transformer is total. Decoding rides the admitted codecs only — `parseTraceParent` and `TraceState` from `@opentelemetry/core`, `Tracer.makeExternalSpan`/`Tracer.withSpanContext` from the `@effect/opentelemetry` facade — and a hand-rolled `traceparent` regex is the named defect.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                            |
| :-----: | :------------- | :-------------------------------------------------------------------------------- |
|  [01]   | [EXTRACTION]   | carrier decode: `traceparent` + `tracestate` + `baggage` into typed values         |
|  [02]   | [CONTINUATION] | the ingress rail transformer, the egress stamp, and the per-transport seam routing |

## [2]-[EXTRACTION]

[EXTRACTION]:
- Owner: the interior codec kernel — `parseTraceParent` decodes the header into an OTel `SpanContext`, `new TraceState(raw)` lifts the `tracestate` list, `parseKeyPairsIntoRecord` decodes `baggage` into a flat record, and the header names are the core constants `TRACE_PARENT_HEADER`/`TRACE_STATE_HEADER`, never literals.
- Packages: `@opentelemetry/core` (the propagation codecs — an `[R3]`-collapse member; when native `Otlp` parity closes, the codec import collapses onto the facade's own context bridge and this owner's surface is unchanged), `@effect/opentelemetry` (`Tracer.makeExternalSpan` minting the Effect-native parent value).
- Law: the carrier is one shape — `Readonly<Record<string, string | undefined>>` — so platform `Headers`, worker message metadata, queue envelope maps, and plain records all admit through one signature; carrier keys read case-normalized at the seam because HTTP header casing is transport accident, never data.
- Law: `parseTraceParent` returning `null` and a malformed `tracestate` both fold to `Option.none` — a damaged inbound context is indistinguishable from an absent one by design, because continuing a corrupt trace forges causality where starting a root records the truth.
- Law: `baggage` is annotation material, not span identity — the decoded record stamps `Effect.annotateLogs`/`Effect.annotateSpans` regions and never widens the parent span; a baggage value never becomes a metric tag (unbounded cardinality).
- Receipt: `Option<Tracer.ExternalSpan>` — the doctrine interior form for inbound trace identity; consumers needing the raw OTel `SpanContext` read the interior lane through `Propagation.ingress` instead of re-parsing.
- Growth: a new wire dialect (`b3`, `xb3`) is one decode arm inside `_context` selecting on the carrier's present keys — never a second extraction owner.

```typescript
import { Option, type Tracer } from "effect"
import { Tracer as OtelBridge } from "@effect/opentelemetry"
import {
  TRACE_PARENT_HEADER,
  TRACE_STATE_HEADER,
  TraceState,
  parseKeyPairsIntoRecord,
  parseTraceParent,
} from "@opentelemetry/core"
import type { SpanContext } from "@opentelemetry/api"

const _BAGGAGE_HEADER = "baggage"

declare namespace Propagation {
  type Carrier = Readonly<Record<string, string | undefined>>
}

const _read = (carrier: Propagation.Carrier, key: string): Option.Option<string> =>
  Option.orElse(Option.fromNullable(carrier[key]), () => Option.fromNullable(carrier[key.toLowerCase()]))

const _context = (carrier: Propagation.Carrier): Option.Option<SpanContext> =>
  Option.map(
    Option.flatMap(_read(carrier, TRACE_PARENT_HEADER), (header) => Option.fromNullable(parseTraceParent(header))),
    (context) =>
      Option.match(_read(carrier, TRACE_STATE_HEADER), {
        onNone: () => context,
        onSome: (raw) => ({ ...context, traceState: new TraceState(raw) }),
      }),
  )

const _extract = (carrier: Propagation.Carrier): Option.Option<Tracer.ExternalSpan> =>
  Option.map(_context(carrier), (context) =>
    OtelBridge.makeExternalSpan({
      spanId: context.spanId,
      traceFlags: context.traceFlags,
      traceId: context.traceId,
      ...(context.traceState !== undefined && { traceState: context.traceState.serialize() }),
    }))

const _baggage = (carrier: Propagation.Carrier): Readonly<Record<string, string>> =>
  parseKeyPairsIntoRecord(Option.getOrUndefined(_read(carrier, _BAGGAGE_HEADER)))
```

## [3]-[CONTINUATION]

[CONTINUATION]:
- Owner: the assembled `Propagation` export — extraction members plus the one ingress transformer and the one egress stamp, `Function.dual` so the transformer follows a live pipe subject at every entry seam.
- Law: `Propagation.ingress` is the entry-seam law — one transformer that continues the inbound parent through the facade's `Tracer.withSpanContext` when present, runs unchanged when absent, and stamps the decoded baggage as log annotations in the same declaration; every ingress (edge middleware, `work` entity handler, `browser` boot, worker runner) composes this one member, so extract-and-continue can never be half-applied.
- Law: transport seams split by owner — the shared HTTP client egress rides `HttpClient.withTracerPropagation` composed on `host/net`'s client, outbound stamping onto any record-shaped carrier rides the platform `HttpTraceContext.toHeaders` directly (queue envelopes and worker frames are string records, so the platform codec already serves them and a local forwarding member would be a rename wrapper), HTTP-header ingress at `edge` may equivalently ride `HttpTraceContext.fromHeaders` with its `w3c`/`b3`/`xb3` dialect rows since both produce the same `Option<Tracer.ExternalSpan>` interior form, and `Propagation` owns the general inbound carrier those families do not reach.
- Boundary: span creation, span naming, and the `Effect.fn` seam are `surfaces-and-dispatch` law composed by callers; this owner never opens a span — it only fixes the parent of whatever span the caller opens next.
- Entry: `Propagation.ingress(effect, carrier)` or `effect.pipe(Propagation.ingress(carrier))`; `Propagation.extract(carrier)` for the parent value; `Propagation.baggage(carrier)` for the annotation record.
- Growth: a new inbound transport is one call site composing `ingress` — the owner is closed.

```typescript
import { Effect, Function, Option, type Tracer } from "effect"
import { Tracer as OtelBridge } from "@effect/opentelemetry"

const _ingress: {
  (carrier: Propagation.Carrier): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, carrier: Propagation.Carrier): Effect.Effect<A, E, R>
} = Function.dual(
  2,
  <A, E, R>(self: Effect.Effect<A, E, R>, carrier: Propagation.Carrier): Effect.Effect<A, E, R> =>
    Option.match(_context(carrier), {
      onNone: () => Effect.annotateLogs(self, _baggage(carrier)),
      onSome: (context) => OtelBridge.withSpanContext(Effect.annotateLogs(self, _baggage(carrier)), context),
    }),
)

const Propagation: {
  readonly baggage: (carrier: Propagation.Carrier) => Readonly<Record<string, string>>
  readonly extract: (carrier: Propagation.Carrier) => Option.Option<Tracer.ExternalSpan>
  readonly ingress: typeof _ingress
} = {
  baggage: _baggage,
  extract: _extract,
  ingress: _ingress,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Propagation }
```
