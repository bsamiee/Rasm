# [RUNTIME_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress — AI provider calls, runner discovery, OTLP export, the config chain's remote stage — dials through one entry that applies its lane's status admission, transient retry, redirect ceiling, total budget, circuit admission, and W3C trace propagation as composed transformers over the shared `HttpClient` the runtime row provided. A lane is a policy row whose durations are the core budget ledger's — each row names its `Budget` kind, its retry pulse compiles from that row's axes, and its total budget is that row's `total` — so retry posture is one cross-language ledger and no per-lane duration literal exists. The circuit ledger is the branch's one breaker owner: a keyed closed→open→half-open cell folded purely and applied as a guard transformer, riding every dial by row and exported so the fanout publish and the delivery transmit inherit the identical admission law. Transport residency tunes beneath the table: the undici dispatcher rows — connection ceilings, pipelining, keep-alive — pin under the node row's client at the root, so pooling is dispatcher configuration and policy is composed transformers, one client, both concerns. A per-folder client, a bare `fetch`, a call-site retry loop, a hand breaker beside the ledger, and a second timeout convention are the named defects; framed and server-sent transport is `channel`'s. The module is `runtime/src/net/client.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                     | [PUBLIC]          |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :---------------- |
|  [01]   | `LANE_ROWS`      | the closed policy table — ledger binding, pulse compile, hops, circuit row   | `Client`          |
|  [02]   | `BREAK_STATE`    | the keyed circuit ledger — pure admission/settle folds, the guard transformer | `Breaker`, `Lapse` |
|  [03]   | `DIAL_SEAM`      | the one entry, the budget geometry, the consumer law                         | `Client`          |
|  [04]   | `DISPATCH_ROWS`  | undici dispatcher tuning beneath the node row's client                       | root data         |

## [2]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the interior `_lanes` anchor — `live` (interactive calls), `batch` (bulk and export egress), `feed` (long-lived streaming responses) — each row carrying `kind` (the `core/value/fault#RETRY_BUDGET` ledger row the lane's durations read), `budget` (`Option<Duration>` — the ledger row's `total` on the settled lanes, stated absence on `feed` because the connection outlives any deadline), `hops` (the redirect ceiling, zero on `feed`), and `break` (`Option<Breaker.Policy>` — the circuit row the guard reads; stated absence on `feed` because the reconnect pulse already paces re-dials).
- Law: the pulse compiles from the ledger row's axes — `exponential(base, factor)` → `jittered` → `resetAfter(reset)` → `intersect(recurs(attempts))` → `upTo(window)` — without the ledger's class gate, because `HttpClient.retryTransient` already gates transience at the transport altitude; the compile is one interior function over `Budget[kind]`, so tuning a lane is editing the ledger row and every consumer of that lane inherits the edit at once.
- Law: the row guard closes the member set and the table grows by evidence — `_Rows` proves every lane carries the full policy complement, the anchor itself is the lane set, and a genuinely new egress contract (a webhook lane, a hedged lane) is one row plus zero new surface.
- Boundary: proxy is transport residency, not per-call policy — the lane table carries no proxy knob, the browser lane has none by construction, and the dispatcher rows in `[5]` own residency.
- Packages: `effect` (`Schedule`, `Duration`, `Option`), `@rasm/ts/core` (`Budget`).

## [3]-[BREAK_STATE]

[BREAK_STATE]:
- Owner: `Breaker` — the one circuit owner of the branch. A circuit is a keyed cell (`closed` counting faults, `open` holding its reopen instant, `half` rationing probes) whose transitions are two pure folds: `_admitted` decides entry and advances `open→half` when the cool window lapses, `_settled` folds the outcome — success rests the cell, a half-open failure or a trip-count breach opens it for the policy's `cool` span. `Breaker.guard(key, policy)` is the transformer any egress effect composes; the dial applies it by lane row, and the fanout publish and delivery transmit compose the same guard under their own keys.
- Law: state rides `Ref.modify` — admission and settlement are atomic pure folds over the cell, so concurrent dials race on the ledger, never on a lock, and the machine is replayable from its fold functions alone.
- Law: rejection is `Lapse` evidence — `reason: "break"`, class `unavailable`, the policy's `cool` as the spent span — so an open circuit routes through the same budget gate as every transient and no second shed fault exists.
- Law: the registry is a `Context.Reference` — cells key by guard key in one `MutableHashMap` default, the requirement channel stays clean (`R` never grows), and a root or proof overrides the whole ledger by providing the Reference. Exemption: `_cell` is the one statement kernel — the synchronous mint-or-get against the registry map.
- Growth: hedging and load-shed stay owned elsewhere (`Effect.raceAll` at the caller, `Gate.shed` at the serving edge); a per-tenant circuit is a key suffix, zero new surface.
- Packages: `effect` (`Clock`, `Context`, `Data`, `Duration`, `MutableHashMap`, `Option`, `Ref`), `@rasm/ts/core` (`FaultClass`).

```typescript
class Lapse extends Data.TaggedError("Lapse")<{
  readonly lane: string
  readonly budget: Duration.Duration
  readonly reason: "budget" | "break"
}> {
  get class(): FaultClass.Kind {
    return this.reason === "budget" ? "expired" : "unavailable"
  }
}

declare namespace Breaker {
  type Cell = { readonly state: "closed" | "open" | "half"; readonly faults: number; readonly until: number; readonly probes: number }
  type Policy = { readonly trip: number; readonly cool: Duration.Duration; readonly probes: number }
}

const _REST: Breaker.Cell = { state: "closed", faults: 0, until: 0, probes: 0 }

class Breakers extends Context.Reference<Breakers>()("runtime/Breakers", {
  defaultValue: () => MutableHashMap.empty<string, Ref.Ref<Breaker.Cell>>(),
}) {}

const _admitted = (held: Breaker.Cell, now: number, policy: Breaker.Policy): readonly [boolean, Breaker.Cell] =>
  held.state === "open"
    ? now >= held.until
      ? [true, { state: "half", faults: 0, until: 0, probes: policy.probes - 1 }]
      : [false, held]
    : held.state === "half"
      ? held.probes > 0
        ? [true, { ...held, probes: held.probes - 1 }]
        : [false, held]
      : [true, held]

const _settled = (held: Breaker.Cell, now: number, policy: Breaker.Policy, passed: boolean): Breaker.Cell =>
  passed
    ? _REST
    : held.state === "half" || held.faults + 1 >= policy.trip
      ? { state: "open", faults: 0, until: now + Duration.toMillis(policy.cool), probes: 0 }
      : { ...held, faults: held.faults + 1 }

const _cell = (
  cells: MutableHashMap.MutableHashMap<string, Ref.Ref<Breaker.Cell>>,
  key: string,
): Ref.Ref<Breaker.Cell> =>
  Option.getOrElse(MutableHashMap.get(cells, key), () => {
    const minted = Ref.unsafeMake(_REST)
    MutableHashMap.set(cells, key, minted)
    return minted
  })

const _guard = (key: string, policy: Breaker.Policy) =>
  <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | Lapse, R> =>
    Effect.gen(function* () {
      const cell = _cell(yield* Breakers, key)
      const now = yield* Clock.currentTimeMillis
      const entered = yield* Ref.modify(cell, (held) => _admitted(held, now, policy))
      return entered
        ? yield* Effect.tapBoth(self, {
            onFailure: () =>
              Effect.flatMap(Clock.currentTimeMillis, (at) => Ref.update(cell, (held) => _settled(held, at, policy, false))),
            onSuccess: () => Ref.update(cell, (held) => _settled(held, now, policy, true)),
          })
        : yield* new Lapse({ lane: key, budget: policy.cool, reason: "break" })
    })

const Breaker = { guard: _guard } as const
```

## [4]-[DIAL_SEAM]

[DIAL_SEAM]:
- Owner: `Client.dial` — the one entry. Modality follows the call shape: `dial(lane, request)` yields the scoped `HttpClientResponse` (the caller owns the body's lifetime — the `feed` posture); `dial(lane, request, shape)` fuses execution, status admission, JSON decode through the owning Schema, and scope closure into one self-contained step; both apply the lane's transformers — `HttpClient.filterStatusOk`, `HttpClient.followRedirects`, `HttpClient.retryTransient({ schedule })`, `HttpClient.withTracerPropagation(true)` — over the client yielded from the requirement channel.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above the client's transient retry, so retries spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one composes the ledger row's `attempt` duration as its own `Effect.fn` pipeline step under the rails layering law.
- Law: expiry and shed are one typed family — `Lapse` carries the lane and the spent span as evidence, its `reason` splitting `budget` (class `expired`) from `break` (class `unavailable`), so the core budget gate re-drives both where a consumer composes a ledger schedule; transport and status faults ride the platform's own `HttpClientError` family untouched, and decode skew rides `ParseError` — three families, each already routable, none re-wrapped.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `bearerToken`, `setHeader`, `setUrlParams` compose at the consumer's seam; the dial owns policy, never request vocabulary.
- Boundary: the client binding is the runtime row's (`exec#RUNTIME_ROWS`); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork.
- Entry: `Client.dial(lane, request[, shape])`; `R` carries `HttpClient` (plus `Scope` on the response modality) to the root.
- Receipt: the overload annotations are the whole seam contract — fault union and requirement set readable without opening the body.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`), `effect` (`Data`, `Effect`, `Option`, `Schedule`).

```typescript
import { HttpClient, type HttpClientError, type HttpClientRequest, HttpClientResponse } from "@effect/platform"
import { Clock, Context, Data, Duration, Effect, MutableHashMap, Option, type ParseResult, Ref, Schedule, type Schema, type Scope, pipe } from "effect"
import { Budget, type FaultClass } from "@rasm/ts/core"

const _pulse = (kind: Budget.Kind): Schedule.Schedule<unknown> =>
  Schedule.exponential(Budget[kind].base, Budget[kind].factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(Budget[kind].reset),
    Schedule.intersect(Schedule.recurs(Budget[kind].attempts)),
    Schedule.upTo(Budget[kind].window),
  )

const _lanes = {
  live: { kind: "pulse", budget: Option.some(Budget.pulse.total), hops: 2, break: Option.some({ trip: 8, cool: Duration.seconds(30), probes: 1 }) },
  batch: { kind: "bulk", budget: Option.some(Budget.bulk.total), hops: 2, break: Option.some({ trip: 16, cool: Duration.seconds(45), probes: 2 }) },
  feed: { kind: "feed", budget: Option.none<Duration.Duration>(), hops: 0, break: Option.none<Breaker.Policy>() },
} as const

declare namespace Client {
  type Lane = keyof typeof _lanes
  type Row = {
    readonly kind: Budget.Kind
    readonly budget: Option.Option<Duration.Duration>
    readonly hops: number
    readonly break: Option.Option<Breaker.Policy>
  }
  type _Rows<T extends Record<Lane, Row> = typeof _lanes> = T
}

const _tempered = (lane: Client.Lane) => (client: HttpClient.HttpClient): HttpClient.HttpClient =>
  client.pipe(
    HttpClient.filterStatusOk,
    HttpClient.followRedirects(_lanes[lane].hops),
    HttpClient.retryTransient({ schedule: _pulse(_lanes[lane].kind) }),
    HttpClient.withTracerPropagation(true),
  )

const _gated = (
  lane: Client.Lane,
  request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
  pipe(
    Effect.flatMap(HttpClient.HttpClient, (client) => _tempered(lane)(client).execute(request)),
    (sent) =>
      Option.match(_lanes[lane].budget, {
        onNone: () => sent,
        onSome: (budget) =>
          Effect.timeoutFail(sent, { duration: budget, onTimeout: () => new Lapse({ lane, budget, reason: "budget" }) }),
      }),
    (bounded) =>
      Option.match(_lanes[lane].break, {
        onNone: () => bounded,
        onSome: (policy) => Breaker.guard(lane, policy)(bounded),
      }),
  )

function dial(
  lane: Client.Lane,
  request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope>
function dial<A, I, R>(
  lane: Client.Lane,
  request: HttpClientRequest.HttpClientRequest,
  shape: Schema.Schema<A, I, R>,
): Effect.Effect<A, HttpClientError.HttpClientError | Lapse | ParseResult.ParseError, HttpClient.HttpClient | R>
function dial<A, I, R>(lane: Client.Lane, request: HttpClientRequest.HttpClientRequest, shape?: Schema.Schema<A, I, R>) {
  return shape === undefined
    ? _gated(lane, request)
    : Effect.scoped(Effect.flatMap(_gated(lane, request), HttpClientResponse.schemaJson(shape)))
}

const Client = { dial } as const
```

## [5]-[DISPATCH_ROWS]

[DISPATCH_ROWS]:
- Owner: the dispatcher tuning rows the root composes beneath the node row's client — one `as const` policy table (connection ceiling per origin, pipelining depth, keep-alive posture, header ceiling) consumed by the undici dispatcher construction the binding package owns: `NodeHttpClient.layerUndici` rides the default dispatcher, `NodeHttpClient.dispatcherLayer`/`dispatcherLayerGlobal` install a tuned one, and `NodeHttpClient.makeDispatcher` is the scoped construction the layers wrap.
- Law: residency is root data — the dispatcher row composes once at the boot edge under `exec#RUNTIME_ROWS`'s node row; a lane never names a dispatcher fact, the bun row has no dispatcher by construction (native fetch), and the browser lane's transport is `browser/fetch#BINDING_ROWS`'s XHR client row.
- Law: the raw `undici` surface is reached only through the binding's `Undici` re-export at this one seam — a direct `undici` import anywhere else bypasses tracing, the typed error rail, and pooling policy in one stroke.
- RESEARCH: the exact undici `Agent` option member spellings the tuning rows feed (`connections`, `pipelining`, `keepAliveTimeout`, proxy and TLS-pin construction) are unverified until a folder catalogue documents the `Undici` re-export surface; the rows below are settled policy data, their application member is the research item.
- Growth: a new residency fact (an egress proxy, a TLS pin, HTTP/2 session tuning) is one row consumed at the same root seam.
- Packages: `@effect/platform-node` (`NodeHttpClient`).

```typescript
const _dispatch = {
  connections: 128,
  pipelining: 1,
  keepAliveMillis: 30_000,
  headersCeiling: 32_768,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Breaker, Client, Lapse }
```
