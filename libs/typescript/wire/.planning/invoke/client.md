# [WIRE_CLIENT]

`invoke/client.ts` is the typed invocation client: the protocol axis — `connect` | `grpc-web`, one policy discriminant selecting one `@connectrpc/connect-web` transport factory over one Config-decoded options record — and the Effect seam every RPC crosses: unary calls lift through `Effect.tryPromise` with the fiber's interruption wired to the call's `AbortSignal`, streaming calls fold through `Stream.fromAsyncIterable`, every caught value reconstructs through `fault/detail.ts`'s `fromConnect` so the error channel is `FaultDetail` end to end, trace identity stamps outbound as W3C headers read from the current span, and retry rides schedules compiled from `kernel/fault` budget rows gated on the `Hops` retryability projection. No domain code sees a `Promise`, an `AsyncIterable`, a `ConnectError`, or a bare code.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                    |
| :-----: | :---------------- | :------------------------------------------------------------------------------ |
|   [1]   | `TRANSPORT_AXIS`  | the Config-decoded policy record, the factory table, the `Invoke` service         |
|   [2]   | `EFFECT_WRAP`     | the unary/stream lifts: interruption, deadline, trace headers, the fault fold      |
|   [3]   | `RETRY_SCHEDULES` | budget-compiled schedules gated on retryability; the layering law                 |

## [2]-[TRANSPORT_AXIS]

- Owner: `Invoke` — one `Effect.Service` whose Layer factory takes the decoded policy record and constructs exactly one transport by vocabulary lookup; the two factories are never both live for one client, and the service carries the typed-client bind plus the two lifts.
- Entry: `Invoke.Config` — the Schema the app root decodes from `host/config`: `protocol`, `baseUrl`, `useBinaryFormat`, `timeoutMs`; `Invoke.bind(service)` — `createClient` over the one transport, the descriptor-derived `Client<T>` with zero hand-written methods.
- Growth: a protocol row is the axis — a third protocol is one `_transports` row plus one config literal; per-protocol drift anywhere downstream of `createClient` cannot exist because both factories return the same `Transport`.
- Law: the options are Config-decoded values — `baseUrl` and `timeoutMs` arrive validated at construction; a hardcoded endpoint or timeout is the parameterization defect. Binary format is the default for the C#-emitted services (content-stable, compact); JSON is the debug posture the config may select.
- Law: the transport constructs once at the Layer — per-call transport construction re-mints connection state and defeats interceptor identity; the service's scoped life owns it.
- Boundary: the factories and their option records are the `@connectrpc/connect-web` surface; the emitted `DescService` arrives from `invoke/capability.ts`; a lane needing XHR upload progress or `arraybuffer` bypasses this axis for the platform XHR client, stated at the consumer.

```typescript
import { createClient, type Client, type Transport } from "@connectrpc/connect"
import { createConnectTransport, createGrpcWebTransport } from "@connectrpc/connect-web"
import type { DescService } from "@bufbuild/protobuf"
import { Effect, Schema } from "effect"

const _protocols = ["connect", "grpc-web"] as const

const _Config = Schema.Struct({
  protocol: Schema.Literal(..._protocols),
  baseUrl: Schema.NonEmptyString,
  useBinaryFormat: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  timeoutMs: Schema.Int.pipe(Schema.between(1, 120000)),
})

const _transports = {
  connect: (config: Invoke.Config): Transport =>
    createConnectTransport({ baseUrl: config.baseUrl, useBinaryFormat: config.useBinaryFormat, defaultTimeoutMs: config.timeoutMs }),
  "grpc-web": (config: Invoke.Config): Transport =>
    createGrpcWebTransport({ baseUrl: config.baseUrl, useBinaryFormat: config.useBinaryFormat, defaultTimeoutMs: config.timeoutMs }),
} as const

declare namespace Invoke {
  type Protocol = (typeof _protocols)[number]
  type Config = Schema.Schema.Type<typeof _Config>
  type _Rows<T extends Record<Protocol, (config: Config) => Transport> = typeof _transports> = T
}

class Invoke extends Effect.Service<Invoke>()("wire/Invoke", {
  effect: (config: Invoke.Config) =>
    Effect.sync(() => {
      const transport = _transports[config.protocol](config)
      return {
        bind: <T extends DescService>(service: T): Client<T> => createClient(service, transport),
        unary: _unary,
        stream: _stream,
      }
    }),
  accessors: true,
}) {
  static readonly Config: typeof _Config = _Config
}
```

## [3]-[EFFECT_WRAP]

- Owner: the two lifts — `_unary`, the promise-call seam, and `_stream`, the server-streaming seam; both total over their foreign failure modes through the `fromConnect` reconstruction.
- Entry: `Invoke.unary((signal, headers) => client.method(input, { signal, headers }))` — the caller closes over its typed client method; the lift owns interruption, trace stamping, and the fault fold. `Invoke.stream((headers) => client.method(input, { headers }))` for the streaming shape.
- Receipt: the error channel is `FaultDetail` — reason, surface, hop chain — so a recovery arm upstream dispatches on the closed `Hops` vocabulary and never inspects transport internals.
- Growth: a per-call context axis (tenant header, HLC stamp) is one line in `_stamped` — every call inherits it; a per-call override rides the caller's own closure, never a knob here.
- Law: interruption crosses with the call — `Effect.tryPromise`'s `try` receives the fiber-wired `AbortSignal` and hands it to `CallOptions.signal`, so a scope close or race loss aborts the in-flight RPC as `Code.Canceled`; no controller is managed by hand.
- Law: trace identity is per-call headers, not a fetch rewrite — the current span reads off the fiber (`Effect.currentSpan`), `HttpTraceContext.toHeaders` spells the W3C pair, and absence of a span sends no header; propagation is uniform across both protocol arms because it rides `CallOptions`.
- Law: the fold is at the seam — every rejection passes `FaultDetail.fromConnect` inside the lift; a `ConnectError` escaping this module, or a raw `try`/`catch` around a client call in domain code, is the leak defect.
- Boundary: `FaultDetail` and the `Hops` vocabulary are `fault/detail.ts`'s; `HttpTraceContext` is the `@effect/platform` codec family; the capability SDK composes these lifts per method at `invoke/capability.ts`.

```typescript
import { Headers, HttpTraceContext } from "@effect/platform"
import { Effect, Option, Stream } from "effect"
import { FaultDetail } from "../fault/detail.ts"

const _stamped: Effect.Effect<Headers.Headers> = Effect.map(
  Effect.option(Effect.currentSpan),
  Option.match({
    onNone: () => Headers.empty,
    onSome: (span) => HttpTraceContext.toHeaders(span),
  }),
)

const _unary = <O>(call: (signal: AbortSignal, headers: Headers.Headers) => Promise<O>): Effect.Effect<O, FaultDetail> =>
  Effect.flatMap(_stamped, (headers) =>
    Effect.tryPromise({
      try: (signal) => call(signal, headers),
      catch: (caught) => FaultDetail.fromConnect(caught),
    }),
  )

const _stream = <O>(open: (headers: Headers.Headers) => AsyncIterable<O>): Stream.Stream<O, FaultDetail> =>
  Stream.unwrap(
    Effect.map(_stamped, (headers) =>
      Stream.fromAsyncIterable(open(headers), (caught) => FaultDetail.fromConnect(caught)),
    ),
  )
```

## [4]-[RETRY_SCHEDULES]

- Owner: the retryable-wire schedule compiler — one fold from a `kernel/fault` budget row to a composed `Schedule`, gated on the `FaultDetail.retryable` projection so the policy cannot re-drive a terminal reason; and `retrying`, the dual transformer exported beside the service as the module's one operation family.
- Entry: `retrying(effect, budget)` or the pipe posture `effect.pipe(retrying(budget))` — the budget row arrives from the caller's own policy (the capability page names per-method budgets); the schedule compiles per composition from the row, a cheap value the transformer builds where it attaches.
- Law: the gate rides the policy value — `Schedule.whileInput` over `FaultDetail.retryable` travels WITH the schedule, so a misapplied policy cannot re-drive `denied` or `dataloss`; a call-site predicate re-deriving retryability is policy leakage.
- Law: the budget names both bounds — attempts intersect elapsed (`Schedule.intersect(Schedule.recurs(n))` under `Schedule.upTo(window)`), jitter decorrelates the fleet, and the per-attempt deadline is the transport's `timeoutMs` BELOW the retry while the budget's window bounds the whole call ABOVE it — the two-budget layering stated once here and inherited by every capability method.
- Boundary: the `Budget` row (`base`, `growth`, `attempts`, `window`) is `kernel/fault` vocabulary this module consumes as a value; which methods retry (idempotency knowledge) is `invoke/capability.ts`'s per-method declaration.

```typescript
import { Budget } from "@rasm/ts/kernel"
import { Function, Schedule } from "effect"

const _schedule = (budget: Budget): Schedule.Schedule<unknown, FaultDetail> =>
  Schedule.exponential(budget.base, budget.growth).pipe(
    Schedule.jittered,
    Schedule.intersect(Schedule.recurs(budget.attempts)),
    Schedule.upTo(budget.window),
    Schedule.whileInput((fault: FaultDetail) => fault.retryable),
  )

const retrying: {
  (budget: Budget): <A, R>(self: Effect.Effect<A, FaultDetail, R>) => Effect.Effect<A, FaultDetail, R>
  <A, R>(self: Effect.Effect<A, FaultDetail, R>, budget: Budget): Effect.Effect<A, FaultDetail, R>
} = Function.dual(
  2,
  <A, R>(self: Effect.Effect<A, FaultDetail, R>, budget: Budget): Effect.Effect<A, FaultDetail, R> =>
    Effect.retry(self, _schedule(budget)),
)

// --- [EXPORTS] --------------------------------------------------------------------------

export { Invoke, retrying }
```
