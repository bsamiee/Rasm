# [RUNTIME_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress — AI provider calls, runner discovery, OTLP export, the config chain's remote stage — dials through one entry that applies its lane's status admission, transient retry, redirect ceiling, total budget, and W3C trace propagation as composed transformers over the shared `HttpClient` the runtime row provided. A lane is a policy row whose durations are the core budget ledger's — each row names its `Budget` kind, its retry pulse compiles from that row's axes, and its total budget is that row's `total` — so retry posture is one cross-language ledger and no per-lane duration literal exists. Transport residency tunes beneath the table: the undici dispatcher rows — connection ceilings, pipelining, keep-alive — pin under the node row's client at the root, so pooling is dispatcher configuration and policy is composed transformers, one client, both concerns. A per-folder client, a bare `fetch`, a call-site retry loop, and a second timeout convention are the named defects; framed and server-sent transport is `channel`'s. The module is `runtime/src/net/client.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                     | [PUBLIC]          |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :---------------- |
|  [01]   | `LANE_ROWS`      | the closed policy table — ledger binding, pulse compile, hops                | `Client`          |
|  [02]   | `DIAL_SEAM`      | the one entry, the budget geometry, the consumer law                         | `Client`, `Lapse` |
|  [03]   | `DISPATCH_ROWS`  | undici dispatcher tuning beneath the node row's client                       | root data         |

## [2]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the interior `_lanes` anchor — `live` (interactive calls), `batch` (bulk and export egress), `feed` (long-lived streaming responses) — each row carrying `kind` (the `core/value/fault#RETRY_BUDGET` ledger row the lane's durations read), `budget` (`Option<Duration>` — the ledger row's `total` on the settled lanes, stated absence on `feed` because the connection outlives any deadline), and `hops` (the redirect ceiling, zero on `feed`).
- Law: the pulse compiles from the ledger row's axes — `exponential(base, factor)` → `jittered` → `resetAfter(reset)` → `intersect(recurs(attempts))` → `upTo(window)` — without the ledger's class gate, because `HttpClient.retryTransient` already gates transience at the transport altitude; the compile is one interior function over `Budget[kind]`, so tuning a lane is editing the ledger row and every consumer of that lane inherits the edit at once.
- Law: the row guard closes the member set and the table grows by evidence — `_Rows` proves every lane carries the full policy complement, the anchor itself is the lane set, and a genuinely new egress contract (a webhook lane, a hedged lane) is one row plus zero new surface.
- Boundary: proxy is transport residency, not per-call policy — the lane table carries no proxy knob, the browser lane has none by construction, and the dispatcher rows in `[4]` own residency.
- Packages: `effect` (`Schedule`, `Duration`, `Option`), `@rasm/ts/core` (`Budget`).

## [3]-[DIAL_SEAM]

[DIAL_SEAM]:
- Owner: `Client.dial` — the one entry. Modality follows the call shape: `dial(lane, request)` yields the scoped `HttpClientResponse` (the caller owns the body's lifetime — the `feed` posture); `dial(lane, request, shape)` fuses execution, status admission, JSON decode through the owning Schema, and scope closure into one self-contained step; both apply the lane's transformers — `HttpClient.filterStatusOk`, `HttpClient.followRedirects`, `HttpClient.retryTransient({ schedule })`, `HttpClient.withTracerPropagation(true)` — over the client yielded from the requirement channel.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above the client's transient retry, so retries spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one composes the ledger row's `attempt` duration as its own `Effect.fn` pipeline step under the rails layering law.
- Law: expiry is typed — `Lapse` carries the lane and the spent budget as evidence and its `class` is `expired`, so the core budget gate re-drives it where a consumer composes a ledger schedule; transport and status faults ride the platform's own `HttpClientError` family untouched, and decode skew rides `ParseError` — three families, each already routable, none re-wrapped.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `bearerToken`, `setHeader`, `setUrlParams` compose at the consumer's seam; the dial owns policy, never request vocabulary.
- Boundary: the client binding is the runtime row's (`exec#RUNTIME_ROWS`); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork.
- Entry: `Client.dial(lane, request[, shape])`; `R` carries `HttpClient` (plus `Scope` on the response modality) to the root.
- Receipt: the overload annotations are the whole seam contract — fault union and requirement set readable without opening the body.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`), `effect` (`Data`, `Effect`, `Option`, `Schedule`).

```typescript
import { HttpClient, type HttpClientError, type HttpClientRequest, HttpClientResponse } from "@effect/platform"
import { Data, type Duration, Effect, Option, type ParseResult, Schedule, type Schema, type Scope, pipe } from "effect"
import { Budget, type FaultClass } from "@rasm/ts/core"

const _pulse = (kind: Budget.Kind): Schedule.Schedule<unknown> =>
  Schedule.exponential(Budget[kind].base, Budget[kind].factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(Budget[kind].reset),
    Schedule.intersect(Schedule.recurs(Budget[kind].attempts)),
    Schedule.upTo(Budget[kind].window),
  )

const _lanes = {
  live: { kind: "pulse", budget: Option.some(Budget.pulse.total), hops: 2 },
  batch: { kind: "bulk", budget: Option.some(Budget.bulk.total), hops: 2 },
  feed: { kind: "feed", budget: Option.none<Duration.Duration>(), hops: 0 },
} as const

class Lapse extends Data.TaggedError("Lapse")<{
  readonly lane: Client.Lane
  readonly budget: Duration.Duration
}> {
  get class(): FaultClass.Kind {
    return "expired"
  }
}

declare namespace Client {
  type Lane = keyof typeof _lanes
  type Row = {
    readonly kind: Budget.Kind
    readonly budget: Option.Option<Duration.Duration>
    readonly hops: number
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
        onSome: (budget) => Effect.timeoutFail(sent, { duration: budget, onTimeout: () => new Lapse({ lane, budget }) }),
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

## [4]-[DISPATCH_ROWS]

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

export { Client, Lapse }
```
