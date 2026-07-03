# [BROWSER_FETCH]

`transport/fetch.ts` owns the browser byte-transport rows: how a response becomes a backpressured `Stream`, which flow-policy row governs its buffer, ceiling, and rate, and how every browser dial is decorated with the session posture before it leaves. It composes — never forks — the branch egress law: `host/net/client`'s `Client.dial` carries lane policy (status admission, transient retry, budget, trace propagation), and this page adds exactly the browser plane on top: the offline gate over `boot/connect`'s cell, the CSRF echo from `session/store` on every mutating method, and the streaming modality with its flow rows. A bare `fetch`, a second retry convention, an unbounded response buffer, or a mutation dialed without the echo is the named defect; a dial attempted offline fails fast and typed, and whether the caller parks the intent in `shell/worker`'s outbox is the caller's policy, never this page's.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                | [PUBLIC]              |
| :-----: | :------------- | :---------------------------------------------------------------------- | :-------------------- |
|  [01]   | `FLOW_ROWS`    | the per-class buffer, ceiling, and rate policy rows                     | `Fetch` (types)       |
|  [02]   | `DIAL_SURFACE` | the decorated dial, the offline gate, the streaming and decoded modalities | `Fetch`, `FetchFault` |

## [2]-[FLOW_ROWS]

[FLOW_ROWS]:
- Owner: the interior `_flows` anchor — one row per byte-feed class: `artifact` (frame bands for the decode pool: suspend-posture buffer, a hard byte ceiling, no rate shaping — the pool's scheduler is the governor), `media` (progressive media: sliding buffer sheds the oldest, rate-shaped to steady state), `live` (long-lived event bytes: small suspend buffer, no ceiling — the connection outlives any cap). Each row carries `intake` (buffer capacity), `posture` (the `"suspend" | "dropping" | "sliding"` decision), `cap` (`Option` — absence is a stated decision), and `rate` (`Option` of the token-bucket row `Stream.throttle` consumes).
- Law: the frugal downshift is one multiplier, not a second table — when `boot/connect`'s profile carries `frugal`, the rate row's units scale by `_FRUGAL` at dial time, so data-saver posture is honored across every class with zero per-call knobs.
- Law: the ceiling is enforced in-stream — the running byte count folds through the pipeline and the first band crossing `cap` fails the stream with `overrun` evidence carrying both counts; a ceiling checked after materialization is the rejected order.
- Growth: a new feed class is one row; a new axis (a burst window, a chunk floor) is one field every row states.
- Boundary: lane policy — retry, budget, status — is `host/net/client`'s row table; a flow row never restates it.
- Packages: `effect` (`Option`, `Duration`).

## [3]-[DIAL_SURFACE]

[DIAL_SURFACE]:
- Owner: `Fetch`, one `Effect.Service` over `Client`, `Vault`, and `Connect` — `pull(lane, request, flow)` is the streaming modality: the offline gate, the decoration, the host dial, and the response's byte stream under the flow row's buffer, ceiling, and shaped rate, returned as one `Stream<Uint8Array>` whose scope rides the stream's own lifetime; `send(lane, request, shape)` is the decoded modality: the same gate and decoration delegated to the host dial's fused decode, one self-contained step.
- Law: decoration is recoverable from this declaration — a mutating method (`POST`/`PUT`/`PATCH`/`DELETE`) stamps the CSRF echo pair read from `Vault.csrf`, absence stamping nothing (the server refuses, the browser never guesses); the cookie credentials posture is root material — `Vault.posture` configures the client binding at composition, so no per-call credentials knob exists.
- Law: the offline gate reads the one cell — `Connect.online` false short-circuits to `FetchFault.offline` before any byte moves; the gate is a fast-fail courtesy, not truth (a race with the cell is settled by the transport fault the host rail already types).
- Law: three fault families, none re-wrapped — this page's `FetchFault` carries only the browser-plane reasons (`offline`, `overrun`); transport and status faults ride the platform's `HttpClientError` untouched, budget expiry rides host's `Lapse`, and decode skew rides `ParseError` — every family already routable.
- Entry: `Fetch.pull` / `Fetch.send`; `R` carries `HttpClient` outward to the root through the host dial.
- Receipt: the stated annotations are the seam contract — the streaming modality's error union names every family a consumer can meet, readable without the body.
- Boundary: which requests exist is the consumer's vocabulary over `HttpClientRequest` at full depth; scheduling of artifact pulls is `transport/pool`'s; parked offline intents are `shell/worker`'s outbox.
- Packages: `@rasm/ts/host` (`Client`, `Lapse`); `@effect/platform` (`HttpClientRequest`, `HttpClientResponse`, `HttpClientError`); `effect` (`Effect`, `Stream`, `Option`, `Duration`, `Data`, `Chunk`, `SubscriptionRef`); `../boot/connect.ts` (`Connect`); `../session/store.ts` (`Vault`).

```typescript
import type { HttpClient, HttpClientError } from "@effect/platform"
import { HttpClientRequest, type HttpClientResponse } from "@effect/platform"
import { Chunk, Data, type Duration, Effect, Option, type ParseResult, type Schema, Stream, SubscriptionRef } from "effect"
import { Client, type Lapse } from "@rasm/ts/host"
import { Connect } from "../boot/connect.ts"
import { Vault } from "../session/store.ts"

const _FRUGAL = 0.5
const _MUTATING = ["POST", "PUT", "PATCH", "DELETE"] as const

type _Rate = { readonly units: number; readonly per: Duration.DurationInput; readonly burst: number }

const _flows = {
  artifact: { intake: 64, posture: "suspend", cap: Option.some(268435456), rate: Option.none<_Rate>() },
  media: { intake: 32, posture: "sliding", cap: Option.none<number>(), rate: Option.some<_Rate>({ units: 1048576, per: "1 second", burst: 4194304 }) },
  live: { intake: 16, posture: "suspend", cap: Option.none<number>(), rate: Option.none<_Rate>() },
} as const

const FetchFaultPolicy = {
  offline: { rank: 2, retry: true },
  overrun: { rank: 4, retry: false },
} as const

declare namespace Fetch {
  type Flow = keyof typeof _flows
  type Row = {
    readonly intake: number
    readonly posture: "suspend" | "dropping" | "sliding"
    readonly cap: Option.Option<number>
    readonly rate: Option.Option<_Rate>
  }
  type _Rows<T extends Record<Flow, Row> = typeof _flows> = T
}

declare namespace FetchFault {
  type Reason = keyof typeof FetchFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof FetchFaultPolicy> = T
}

class FetchFault extends Data.TaggedError("FetchFault")<{
  readonly reason: FetchFault.Reason
  readonly detail: string
}> {
  get policy(): FetchFault.Row {
    return FetchFaultPolicy[this.reason]
  }
}

const _capped = (cap: Option.Option<number>) => <E, R>(bands: Stream.Stream<Uint8Array, E, R>): Stream.Stream<Uint8Array, E | FetchFault, R> =>
  Option.match(cap, {
    onNone: () => bands,
    onSome: (ceiling) =>
      Stream.mapAccumEffect(bands, 0, (total, band) =>
        total + band.length > ceiling
          ? Effect.fail(new FetchFault({ reason: "overrun", detail: `${total + band.length}>${ceiling}` }))
          : Effect.succeed([total + band.length, band] as const),
      ),
  })

class Fetch extends Effect.Service<Fetch>()("browser/transport/Fetch", {
  effect: Effect.gen(function* () {
    const connect = yield* Connect
    const vault = yield* Vault
    const _gated: Effect.Effect<void, FetchFault> = Effect.flatMap(SubscriptionRef.get(connect.online), (up) =>
      up ? Effect.void : Effect.fail(new FetchFault({ reason: "offline", detail: "<offline>" })),
    )
    const _decorated = (request: HttpClientRequest.HttpClientRequest): Effect.Effect<HttpClientRequest.HttpClientRequest> =>
      _MUTATING.some((method) => method === request.method)
        ? Effect.map(vault.csrf, (echo) =>
            Option.match(echo, {
              onNone: () => request,
              onSome: ([name, value]) => HttpClientRequest.setHeader(request, name, value),
            }),
          )
        : Effect.succeed(request)
    const _shaped = (row: Fetch.Row, frugal: boolean) => <E, R>(bands: Stream.Stream<Uint8Array, E, R>): Stream.Stream<Uint8Array, E, R> =>
      Option.match(row.rate, {
        onNone: () => bands,
        onSome: (rate) =>
          Stream.throttle(bands, {
            cost: (chunk) => Chunk.reduce(chunk, 0, (total, band) => total + band.length),
            units: frugal ? Math.ceil(rate.units * _FRUGAL) : rate.units,
            duration: rate.per,
            burst: rate.burst,
            strategy: "shape",
          }),
      })
    const pull = (
      lane: Client.Lane,
      request: HttpClientRequest.HttpClientRequest,
      flow: Fetch.Flow,
    ): Stream.Stream<Uint8Array, FetchFault | HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient> =>
      Stream.unwrapScoped(
        Effect.gen(function* () {
          yield* _gated
          const profile = yield* SubscriptionRef.get(connect.profile)
          const frugal = Option.match(profile, { onNone: () => false, onSome: (held) => held.frugal })
          const decorated = yield* _decorated(request)
          const response: HttpClientResponse.HttpClientResponse = yield* Client.dial(lane, decorated)
          const row = _flows[flow]
          return response.stream.pipe(
            Stream.buffer({ capacity: row.intake, strategy: row.posture }),
            _capped(row.cap),
            _shaped(row, frugal),
          )
        }),
      )
    const send = <A, I, R>(
      lane: Client.Lane,
      request: HttpClientRequest.HttpClientRequest,
      shape: Schema.Schema<A, I, R>,
    ): Effect.Effect<
      A,
      FetchFault | HttpClientError.HttpClientError | Lapse | ParseResult.ParseError,
      HttpClient.HttpClient | R
    > => _gated.pipe(Effect.zipRight(_decorated(request)), Effect.flatMap((decorated) => Client.dial(lane, decorated, shape)))
    return { pull, send }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Fetch, FetchFault }
```
