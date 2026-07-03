# [HOST_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress — `ai` provider calls, `work` runner discovery, `telemetry` OTLP export, the config chain's remote stage — dials through one entry that applies its lane's status admission, transient retry, redirect ceiling, total budget, and W3C trace propagation as composed transformers over the shared `HttpClient` the runtime row provided. A lane is a policy row, so a new egress contract is one row — never a per-folder client, a bare `fetch`, a call-site retry loop, or a second timeout convention. The channel siblings for framed and server-sent transport are `channel.md`'s; this page owns request/response egress.

## [1]-[INDEX]

- [01]-[LANE_ROWS]: the closed policy table — budget, pulse, hops, one row per egress contract.
- [02]-[DIAL_SEAM]: the one entry, the budget geometry, and the consumer law.

## [2]-[LANE_ROWS]

- Owner: the interior `_lanes` anchor — `live` (interactive calls: tight total budget, brisk jittered retry), `batch` (bulk and export egress: patient budget, bounded elapsed retry), `feed` (long-lived streaming responses: no total budget — the connection outlives any deadline — minimal pre-stream retry, zero redirects). Each row carries `budget` (`Option<Duration>` — absence is a stated decision, not an omission), `pulse` (one composed `Schedule`: exponential base, `jittered` to decorrelate the fleet, `intersect(recurs)` and `upTo` as the attempt and elapsed bounds), and `hops` (the redirect ceiling).
- Law: the row set is closed by the guard pair and grows by evidence — a genuinely new egress contract (a webhook lane, a hedged lane) is one row plus zero new surface; tuning an existing consumer is editing its row, and every consumer of that lane inherits the edit at once.
- Law: budgets are the branch instantiation of the kernel degradation-budget vocabulary — the row values type against `kernel/fault` budget rows once that page realizes, so retry posture stays one cross-language ledger.
- Boundary: proxy is transport residency, not per-call policy — the proxy posture pins beneath the node row's client through `NodeHttpClient.dispatcherLayer`/`makeDispatcher` at the root; the lane table carries no proxy knob, and the browser lane has none by construction.
- Packages: `effect` (`Schedule`, `Duration`, `Option`).

## [3]-[DIAL_SEAM]

- Owner: `Client.dial` — the one entry. Modality follows the call shape: `dial(lane, request)` yields the scoped `HttpClientResponse` (the caller owns the body's lifetime — the `feed` posture); `dial(lane, request, shape)` fuses execution, status admission, JSON decode through the owning Schema, and scope closure into one self-contained step. Both apply the lane's transformers — `HttpClient.filterStatusOk`, `HttpClient.followRedirects`, `HttpClient.retryTransient({ schedule })`, `HttpClient.withTracerPropagation(true)` — over the client yielded from the requirement channel.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above the client's transient retry, so retries spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one names it as its own `Effect.fn` pipeline step under the rails layering law.
- Law: expiry is typed — `Lapse` carries the lane and the spent budget as evidence; transport and status faults ride the platform's own `HttpClientError` family untouched, and decode skew rides `ParseError` — three families, each already routable, none re-wrapped.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `bearerToken`, `setHeader`, `setUrlParams` compose at the consumer's seam; the dial owns policy, never request vocabulary.
- Boundary: the client binding is the runtime row's (`layerUndici` on node, `FetchHttpClient` on bun, the XHR client in the browser lane); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork.
- Entry: `Client.dial(lane, request[, shape])`; `R` carries `HttpClient` (plus `Scope` on the response modality) to the root.
- Receipt: the overload annotations are the whole seam contract — fault union and requirement set readable without opening the body.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`), `effect` (`Data`, `Effect`, `Option`, `Schedule`).

```typescript
import { HttpClient, type HttpClientError, type HttpClientRequest, HttpClientResponse } from "@effect/platform"
import { Data, Duration, Effect, Option, type ParseResult, Schedule, type Schema, type Scope } from "effect"

const _lanes = {
  live: {
    budget: Option.some(Duration.seconds(2)),
    pulse: Schedule.exponential("80 millis").pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(3))),
    hops: 2,
  },
  batch: {
    budget: Option.some(Duration.seconds(30)),
    pulse: Schedule.exponential("250 millis").pipe(
      Schedule.jittered,
      Schedule.intersect(Schedule.recurs(5)),
      Schedule.upTo("20 seconds"),
    ),
    hops: 2,
  },
  feed: {
    budget: Option.none<Duration.Duration>(),
    pulse: Schedule.exponential("500 millis").pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(2))),
    hops: 0,
  },
} as const

class Lapse extends Data.TaggedError("Lapse")<{
  readonly lane: Client.Lane
  readonly budget: Duration.Duration
}> {}

declare namespace Client {
  type Lane = keyof typeof _lanes
  type Row = {
    readonly budget: Option.Option<Duration.Duration>
    readonly pulse: Schedule.Schedule<unknown, never>
    readonly hops: number
  }
  type _Rows<T extends Record<Lane, Row> = typeof _lanes> = T
  type _Keys<K extends Lane = keyof typeof _lanes> = K
}

const _tempered = (lane: Client.Lane) => (client: HttpClient.HttpClient): HttpClient.HttpClient =>
  client.pipe(
    HttpClient.filterStatusOk,
    HttpClient.followRedirects(_lanes[lane].hops),
    HttpClient.retryTransient({ schedule: _lanes[lane].pulse }),
    HttpClient.withTracerPropagation(true),
  )

const _gated = (
  lane: Client.Lane,
  request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
  Option.match(_lanes[lane].budget, {
    onNone: () => Effect.flatMap(HttpClient.HttpClient, (client) => _tempered(lane)(client).execute(request)),
    onSome: (budget) =>
      Effect.timeoutFail(
        Effect.flatMap(HttpClient.HttpClient, (client) => _tempered(lane)(client).execute(request)),
        { duration: budget, onTimeout: () => new Lapse({ lane, budget }) },
      ),
  })

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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Client, Lapse }
```
