# [RUNTIME_FETCH]

The browser byte-transport plane and the folder's kernel-delegating mint site: the browser runtime binding rows (the XHR `HttpClient` — the one transport carrying upload/download progress and forced-arraybuffer responses — the `WebSocket` constructor row, and the worker spawner), the per-class flow-policy rows governing how a response becomes a backpressured `Stream`, the decorated dial every browser egress rides, and the decode-worker pool behind one closed `Schema.TaggedRequest` protocol with every band crossing zero-copy as a declared `Transferable`. The dial composes — never forks — the branch egress law: `net/client`'s `Client.dial` carries lane policy (status admission, transient retry, budget, trace propagation), and this page adds exactly the browser plane on top: the offline gate over `boot#SIGNAL_CELLS`'s cell, the CSRF echo from `route#SESSION_PLANE` on every mutating method, and the streaming modality with its flow rows. The worker composes `core/interchange/frame`'s settled surfaces — `ArtifactFrame.reassembled` for the ordered fold-and-verify, `Residency.envelope` for the plan decode — and delegates its cache-reverify mint to the one core `Digest` content row; a second content-address notion, a main-thread re-decode re-paying the offloaded cost, an untyped `postMessage` arm, or a bare `fetch` is the named defect. `Depot` is the window-side residency scheduler the app composes into the ui wave's declared viewport port at the root, because the browser and ui waves never import each other. The module is `runtime/src/browser/fetch.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              | [PUBLIC]            |
| :-----: | :---------------- | :--------------------------------------------------------------------- | :------------------ |
|  [01]   | `BINDING_ROWS`    | the browser runtime rows — XHR client, socket constructor, spawner     | `Web`               |
|  [02]   | `FLOW_ROWS`       | the per-class buffer, ceiling, and rate policy rows                    | `Fetch` (types)     |
|  [03]   | `DIAL_SURFACE`    | the decorated dial, the offline gate, the streaming and decoded modalities | `Fetch`, `FetchFault` |
|  [04]   | `WIRE_PROTOCOL`   | the request family, the serialized fault, the pool Tag and layer       | `Pool`, `PoolFault` |
|  [05]   | `DEPOT_SCHEDULER` | the residency ledger, the degree rows, the budget-bounded haul         | `Depot`             |
|  [06]   | `RUNNER_ENTRY`    | the worker-side boot module and its handler record                     | none                |

## [2]-[BINDING_ROWS]

[BINDING_ROWS]:
- Owner: `Web`, the browser counterpart of `exec#RUNTIME_ROWS` — one `as const` roster of the Layer rows the browser root merges: `client` (`BrowserHttpClient.layerXMLHttpRequest`, the `HttpClient` every `Client.dial` reaches — XHR selected deliberately because it is the one browser transport exposing upload/download progress and arraybuffer responses), `socket` (`BrowserSocket.layerWebSocketConstructor`, the `Socket.WebSocketConstructor` row `net/channel`'s framed transport and `persist#OVERLAY_AND_LANE`'s sync row construct against), `channel(url)` (`BrowserSocket.layerWebSocket`, the ready socket for a fixed peer), and `workers(spawn)` (`BrowserWorker.layer` over the app-supplied spawn factory — the worker script URL is app data, never a lib literal).
- Law: the rows compose once at `boot#SINGLE_BOOT`'s root — a lane never names a binding fact, domain modules import zero rows, and the per-runtime subpath gate keeps node and bun bindings physically unresolvable from this lane; the OTLP exporter and the config chain dial through the same XHR client, so telemetry egress inherits the browser posture like every other call.
- Law: the credentials posture is root material — the cookie-carrying dial rides `route#SESSION_PLANE`'s `posture` row configured at the client binding, so no per-call credentials knob exists.
- Growth: a new browser binding (a shared-worker spawner row, a WebTransport row) is one roster entry consumed at the same root seam.
- Boundary: the abstract Tags are `@effect/platform`'s; the node and bun rows are `exec#RUNTIME_ROWS`'s; which rows an app merges is root selection.
- Packages: `@effect/platform-browser` (`BrowserHttpClient`, `BrowserSocket`, `BrowserWorker`).

```typescript
import type { HttpClient, HttpClientError } from "@effect/platform"
import { HttpClientRequest, type HttpClientResponse, Transferable, Worker, type WorkerError } from "@effect/platform"
import { BrowserHttpClient, BrowserSocket, BrowserWorker } from "@effect/platform-browser"
import { ContentKey, FaultClass, Residency } from "@rasm/ts/core"
import { Array, Chunk, Context, Data, type Duration, Effect, HashMap, Layer, Option, Order, type ParseResult, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import { Client, type Lapse } from "../net/client.ts"
import { Connect } from "./boot.ts"
import { Kv, Opfs } from "./persist.ts"
import { Vault } from "./route.ts"

const Web = {
  client: BrowserHttpClient.layerXMLHttpRequest,
  socket: BrowserSocket.layerWebSocketConstructor,
  channel: (url: string) => BrowserSocket.layerWebSocket(url),
  workers: (spawn: (id: number) => globalThis.Worker) => BrowserWorker.layer(spawn),
} as const
```

## [3]-[FLOW_ROWS]

[FLOW_ROWS]:
- Owner: the interior `_flows` anchor — one row per byte-feed class: `artifact` (frame bands for the decode pool: suspend-posture buffer, a hard byte ceiling, no rate shaping — the pool's scheduler is the governor), `media` (progressive media: sliding buffer sheds the oldest, rate-shaped to steady state), `live` (long-lived event bytes: small suspend buffer, no ceiling — the connection outlives any cap). Each row carries `intake` (buffer capacity), `posture` (the `"suspend" | "dropping" | "sliding"` decision), `cap` (`Option` — absence is a stated decision), and `rate` (`Option` of the token-bucket row `Stream.throttle` consumes).
- Law: the frugal downshift is one multiplier, not a second table — when `boot#SIGNAL_CELLS`'s profile carries `frugal`, the rate row's units scale by `_FRUGAL` at dial time, so data-saver posture is honored across every class with zero per-call knobs.
- Law: the ceiling is enforced in-stream — the running byte count folds through the pipeline and the first band crossing `cap` fails the stream with `overrun` evidence carrying both counts; a ceiling checked after materialization is the rejected order.
- Growth: a new feed class is one row; a new axis (a burst window, a chunk floor) is one field every row states.
- Boundary: lane policy — retry, budget, status — is `net/client`'s row table; a flow row never restates it.
- Packages: `effect` (`Duration`, `Option`).

```typescript
const _FRUGAL = 0.5
const _MUTATING = ["POST", "PUT", "PATCH", "DELETE"] as const

type _Rate = { readonly units: number; readonly per: Duration.DurationInput; readonly burst: number }

const _flows = {
  artifact: { intake: 64, posture: "suspend", cap: Option.some(268435456), rate: Option.none<_Rate>() },
  media: { intake: 32, posture: "sliding", cap: Option.none<number>(), rate: Option.some<_Rate>({ units: 1048576, per: "1 second", burst: 4194304 }) },
  live: { intake: 16, posture: "suspend", cap: Option.none<number>(), rate: Option.none<_Rate>() },
} as const

const _fetchFaults = {
  offline: { class: "unavailable" },
  overrun: { class: "invalid" },
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
  type Reason = keyof typeof _fetchFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _fetchFaults> = T
}

class FetchFault extends Data.TaggedError("FetchFault")<{
  readonly reason: FetchFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _fetchFaults[this.reason].class
  }
}
```

## [4]-[DIAL_SURFACE]

[DIAL_SURFACE]:
- Owner: `Fetch`, one `Effect.Service` over `Connect` and `Vault` — `pull(lane, request, flow)` is the streaming modality: the offline gate, the decoration, the host dial, and the response's byte stream under the flow row's buffer, ceiling, and shaped rate, returned as one `Stream<Uint8Array>` whose scope rides the stream's own lifetime and whose binary read runs under `BrowserHttpClient.withXHRArrayBuffer` so bands arrive as octets, never re-decoded text; `send(lane, request, shape)` is the decoded modality: the same gate and decoration delegated to the host dial's fused decode, one self-contained step.
- Law: decoration is recoverable from this declaration — a mutating method (`POST`/`PUT`/`PATCH`/`DELETE`) stamps the CSRF echo pair read from `Vault.csrf`, absence stamping nothing (the server refuses, the browser never guesses); the cookie credentials posture rides `[2]`'s root row, so no per-call credentials knob exists.
- Law: the offline gate reads the one cell — `Connect.online` false short-circuits to the class-carried `offline` fault before any byte moves; the gate is a fast-fail courtesy, not truth (a race with the cell is settled by the transport fault the host rail already types).
- Law: three fault families, none re-wrapped — this page's `FetchFault` carries only the browser-plane reasons (`offline`, `overrun`); transport and status faults ride the platform's `HttpClientError` untouched, budget expiry rides `net/client`'s `Lapse`, and decode skew rides `ParseError` — every family already routable, each carrying its `FaultClass` projection.
- RESEARCH: the XHR upload/download progress-event observation members are unverified — the folder catalogue documents `currentXHRResponseType` and `withXHRArrayBuffer` only; the progress modality (a per-transfer progress feed beside `pull`) lands the day the catalogue verifies the member spellings, and until then progress is a stated capability of the `[2]` client row, never settled fence code.
- Entry: `Fetch.pull` / `Fetch.send`; `R` carries `HttpClient` outward to the root through the host dial.
- Receipt: the stated annotations are the seam contract — the streaming modality's error union names every family a consumer meets, readable without the body.
- Boundary: which requests exist is the consumer's vocabulary over `HttpClientRequest` at full depth; scheduling of artifact pulls is `[6]`'s; parked offline intents are `shell#REPLAY_DRAIN`'s outbox.
- Packages: `../net/client.ts` (`Client`, `Lapse`); `@effect/platform` (`HttpClientRequest`, `HttpClientResponse`, `HttpClientError`); `@effect/platform-browser` (`BrowserHttpClient`); `effect` (`Array`, `Chunk`, `Data`, `Effect`, `Option`, `Stream`); `./boot.ts` (`Connect`); `./route.ts` (`Vault`).

```typescript
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

class Fetch extends Effect.Service<Fetch>()("runtime/browser/Fetch", {
  effect: Effect.gen(function* () {
    const connect = yield* Connect
    const vault = yield* Vault
    const _gated: Effect.Effect<void, FetchFault> = Effect.flatMap(connect.online.get, (up) =>
      up ? Effect.void : Effect.fail(new FetchFault({ reason: "offline", detail: "<offline>" })),
    )
    const _decorated = (request: HttpClientRequest.HttpClientRequest): Effect.Effect<HttpClientRequest.HttpClientRequest> =>
      Array.some(_MUTATING, (method) => method === request.method)
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
          const profile = yield* connect.profile.get
          const frugal = Option.match(profile, { onNone: () => false, onSome: (held) => held.frugal })
          const decorated = yield* _decorated(request)
          const response: HttpClientResponse.HttpClientResponse = yield* BrowserHttpClient.withXHRArrayBuffer(
            Client.dial(lane, decorated),
          )
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
```

## [5]-[WIRE_PROTOCOL]

[WIRE_PROTOCOL]:
- Owner: `Pool` — the `Context.Tag` holding the `Worker.SerializedWorkerPool` over the closed request union, the request classes riding it as statics (`Pool.Assemble`: an artifact's frame bands in, the verified receipt plus octets out; `Pool.Verify`: cache-warmed octets re-proven against their declared key — the browser's own delegated mint site; `Pool.Chart`: residency envelope bytes — manifest or delta — decoded off-thread), `Pool.protocol` (the union the runner checks its handlers against), and `Pool.layer(spawn, size)` (the pool layer over `Web.workers` — the worker script URL is app data, never a lib literal).
- Law: every band is a declared transfer — `Transferable.Uint8Array` on the `Assemble`/`Verify` payloads and on the `Assemble` success, so frames move to the worker and assembled octets move back with zero copies, and the marshal plan is recoverable from the request declarations alone.
- Law: `PoolFault` is `Schema.TaggedError` because it crosses the thread wire — reason rows `parity` (mint mismatch, both keys held as evidence), `sequence` (broken ordinal chain), `overrun` (band over the lane cap), `codec` (frame or manifest decode refusal) — reconstructing as the same tagged class on the window side with its `FaultClass` projection intact, so recovery dispatches one family; the worker folds `core/interchange/codec`'s `WireFault` evidence into these rows at the handler seam, never re-throwing it.
- Law: request identity is dedup identity — structural `Equal` over the payload fields is what collapses two identical `Verify` calls in one window; the fields carry exactly the coordinate, nothing incidental.
- Growth: a new offload concern is one request class plus one handler row — the union and the runner's compile-checked record break every site until both exist; a second pool per load profile restates what `Pool.layer`'s size row already carries.
- Boundary: the frame and manifest byte shapes are `core/interchange/frame`'s; the mint is `core/value/contentKey`'s; the pool executes, never re-models; the protocol law and sizing vocabulary are `proc/worker`'s, instantiated here on the browser rows.
- Packages: `@effect/platform` (`Worker`, `Transferable`); `@rasm/ts/core` (`ContentKey`, `Residency`, `FaultClass`); `effect` (`Context`, `Layer`, `Option`, `Schema`).

```typescript
const _REASONS = ["parity", "sequence", "overrun", "codec"] as const

const _poolFaults = {
  parity: { class: "breached" },
  sequence: { class: "conflicted" },
  overrun: { class: "invalid" },
  codec: { class: "malformed" },
} as const

declare namespace PoolFault {
  type Reason = (typeof _REASONS)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _poolFaults> = T
}

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  reason: Schema.Literal(..._REASONS),
  detail: Schema.String,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.String, expected: Schema.String }), { as: "Option" }),
}) {
  get class(): FaultClass.Kind {
    return _poolFaults[this.reason].class
  }
}

const _Receipt = Schema.Struct({
  key: ContentKey,
  extent: Schema.Int.pipe(Schema.nonNegative()),
})

class Assemble extends Schema.TaggedRequest<Assemble>()("Assemble", {
  payload: { key: ContentKey, frames: Schema.Array(Transferable.Uint8Array) },
  success: Schema.Struct({ ..._Receipt.fields, octets: Transferable.Uint8Array }),
  failure: PoolFault,
}) {}

class Verify extends Schema.TaggedRequest<Verify>()("Verify", {
  payload: { key: ContentKey, octets: Transferable.Uint8Array },
  success: _Receipt,
  failure: PoolFault,
}) {}

class Chart extends Schema.TaggedRequest<Chart>()("Chart", {
  payload: { bytes: Transferable.Uint8Array },
  success: Schema.Union(Residency.Manifest, Residency.Delta),
  failure: PoolFault,
}) {}

const _protocol = Schema.Union(Assemble, Verify, Chart)

class Pool extends Context.Tag("runtime/browser/Pool")<Pool, Worker.SerializedWorkerPool<Assemble | Verify | Chart>>() {
  static readonly Assemble = Assemble
  static readonly Verify = Verify
  static readonly Chart = Chart
  static readonly protocol = _protocol
  static readonly layer = (
    spawn: (id: number) => globalThis.Worker,
    size: number,
  ): Layer.Layer<Pool, WorkerError.WorkerError> =>
    Worker.makePoolSerializedLayer(Pool, { size, concurrency: 1 }).pipe(Layer.provide(Web.workers(spawn)))
}
```

## [6]-[DEPOT_SCHEDULER]

[DEPOT_SCHEDULER]:
- Owner: `Depot`, one scoped `Effect.Service` over `Pool`, `Opfs`, and `Kv` — `ledger`, the residency cell (`core/interchange/frame`'s `Residency.Ledger`) published `Subscribable` so the port read cannot write it; `folded(arrival)` mirroring the frame page's one polymorphic fold — a `Manifest` arrival replaces the ledger (the C# render side owns truth), a `Delta` evolves it, discriminated on the value, never two entrypoints; `landed(receipt)` flipping the receipt's row to `resident`; `plan`, the pending rows as fetch orders — worst-first by LOD then smallest-first by extent so coarse geometry lands early; `haul(pull)`, one budget-bounded pass turning the current plan into verified arrivals.
- Law: the degree is a verdict row — `_DEGREES` maps `Opfs`'s pressure verdict to the haul's concurrency, so storage pressure throttles intake with zero per-call knobs and the byte-feed policy stays `[3]`'s flow row.
- Law: warm before fetch — each order first probes `persist#DOMAIN_ROWS`'s `cache` domain under its content key; a hit is admitted material and re-proves off-thread through `Verify` (the browser's delegated mint) before it lands, a `parity` refusal alone evicts the poisoned band (a transient worker or codec fault forfeits warmth without evicting) while every refused warm falls through to the fetch path, and a fresh assemble writes the cache best-effort — cache faults never fail an arrival, they only forfeit warmth.
- Law: the haul quarantines per artifact — `Effect.partition` keeps every fault beside every arrival, a `parity` refusal never blocks its siblings, and a landed receipt folds into the ledger inside the same pass so a repeated haul cannot re-order what already arrived; deltas replayed across reconnects are idempotent by the frame page's own fold law.
- Law: one pass per call — `haul` drives the plan as it stands; the continuous loop is app composition (`ledger.changes` debounced into repeated hauls), so the lib owns the pass and the app owns the cadence.
- Receipt: `haul` yields `[faults, arrivals]` — arrivals as receipt-plus-octets pairs the app forwards into the ui wave's declared viewport port at the root; the ledger cell is the port's residency read.
- Boundary: manifest and delta ingress arrive decoded from `Residency.stream` over `[4]`'s feeds; the GLB scene semantics are the ui wave's viewer; this owner schedules and joins.
- Packages: `effect` (`Array`, `Chunk`, `Effect`, `HashMap`, `Option`, `Order`, `Stream`, `Subscribable`, `SubscriptionRef`); `@rasm/ts/core` (`Residency`, `ContentKey`); `./persist.ts` (`Kv`, `Opfs`).

```typescript
const _DEGREES = { ample: 6, tight: 2, critical: 1, opaque: 2 } as const

const _byOrder: Order.Order<Residency.Row> = Order.combine(
  Order.mapInput(Order.number, (row: Residency.Row) => row.lod),
  Order.mapInput(Order.number, (row: Residency.Row) => row.extent),
)

class Depot extends Effect.Service<Depot>()("runtime/browser/Depot", {
  scoped: Effect.gen(function* () {
    const pool = yield* Pool
    const opfs = yield* Opfs
    const kv = yield* Kv
    const _ledger = yield* SubscriptionRef.make(HashMap.empty<ContentKey, Residency.Row>())
    const plan: Effect.Effect<ReadonlyArray<Residency.Row>> = Effect.map(SubscriptionRef.get(_ledger), (held) =>
      Array.sort(
        Array.filter(Array.fromIterable(HashMap.values(held)), (row) => row.state === "pending"),
        _byOrder,
      ),
    )
    const landed = (receipt: { readonly key: ContentKey; readonly extent: number }): Effect.Effect<void> =>
      SubscriptionRef.update(_ledger, (held) =>
        HashMap.modifyAt(held, receipt.key, (slot) =>
          Option.map(slot, (row) => ({ ...row, extent: receipt.extent, state: "resident" as const })),
        ),
      )
    const haul = <E, R>(
      pull: (row: Residency.Row) => Stream.Stream<Uint8Array, E, R>,
    ): Effect.Effect<
      readonly [
        ReadonlyArray<E | PoolFault | WorkerError.WorkerError | ParseResult.ParseError>,
        ReadonlyArray<readonly [typeof _Receipt.Type, Uint8Array]>,
      ],
      never,
      R
    > =>
      Effect.gen(function* () {
        const orders = yield* plan
        const budget = yield* opfs.budget
        const _warmed = (order: Residency.Row) =>
          kv.read("cache", order.mesh).pipe(
            Effect.orElseSucceed(Option.none<Uint8Array>),
            Effect.flatMap(
              Option.match({
                onNone: () => Effect.succeedNone,
                onSome: (band) =>
                  pool.executeEffect(new Verify({ key: order.mesh, octets: band })).pipe(
                    Effect.map((receipt) => Option.some([receipt, band] as const)),
                    Effect.catchAll((fault) =>
                      Effect.as(
                        fault._tag === "PoolFault" && fault.reason === "parity"
                          ? Effect.ignore(kv.drop("cache", order.mesh))
                          : Effect.void,
                        Option.none(),
                      ),
                    ),
                  ),
              }),
            ),
          )
        const _hauledOne = (order: Residency.Row) =>
          Effect.flatMap(_warmed(order), (warm) =>
            Option.match(warm, {
              onSome: Effect.succeed,
              onNone: () =>
                Stream.runCollect(pull(order)).pipe(
                  Effect.flatMap((bands) =>
                    pool.executeEffect(new Assemble({ key: order.mesh, frames: Chunk.toReadonlyArray(bands) })),
                  ),
                  Effect.tap(({ key, octets }) => Effect.ignore(kv.write("cache", key, octets))),
                  Effect.map(({ extent, key, octets }) => [{ key, extent }, octets] as const),
                ),
            }),
          )
        return yield* Effect.partition(orders, (order) => Effect.tap(_hauledOne(order), ([receipt]) => landed(receipt)), {
          concurrency: _DEGREES[budget.verdict],
        })
      })
    const ledger: Subscribable.Subscribable<HashMap.HashMap<ContentKey, Residency.Row>> = _ledger
    return {
      ledger,
      plan,
      landed,
      haul,
      folded: (arrival: Residency.Arrival) => SubscriptionRef.update(_ledger, (held) => Residency.folded(held, arrival)),
    }
  }),
}) {}
```

## [7]-[RUNNER_ENTRY]

[RUNNER_ENTRY]:
- Owner: the worker-side boot module — a separate entry the app's spawn factory names, composing `WorkerRunner.layerSerialized(Pool.protocol, handlers)` under `BrowserWorkerRunner.layer` and launching it as its whole life; the module exports nothing, the structural proof it is terminal. A worker thread is its own process under the boot-edge law, so its `runMain` is the thread's one boot and never a second document boot.
- Law: the handler record is compile-checked against the union — `Assemble` decodes each band through `ArtifactFrame.frame`, drives the settled `ArtifactFrame.reassembled` fold (ordering, join, and the frame-side `Parity` verify all inside the frame page's law), demands exactly one completed artifact (an empty or split reassembly refuses typed), and refuses a key mismatch with the declared key as evidence; `Verify` is the browser's delegated mint — `Digest.mint("content", octets)` over the presented octets compared to the declared key, `parity` with both keys on refusal — one of the branch's three sanctioned content-mint delegation sites; `Chart` decodes the envelope bytes through `Residency.envelope`, yielding the manifest-or-delta arrival.
- Law: fault folding is total at this seam — a `WireFault` from the frame fold maps `parity`/`sequence`/`overrun` reason-to-reason into `PoolFault` with its evidence stringified and every other codec reason folds to `codec`; the worker never throws across the boundary because the request's failure schema is the only exit.
- Law: the handlers run one request at a time per worker (`concurrency: 1` at the pool) because reassembly is memory-bound, and the pool's `size` row is the parallelism — worker count, not handler interleaving, scales throughput; the size row arrives from `boot#BUDGET_VALUE`'s `workers` ceiling at composition.
- Boundary: the spawn factory, the script URL, and the bundler wiring are app build material; this module is the script's content law.
- Packages: `@effect/platform` (`WorkerRunner`); `@effect/platform-browser` (`BrowserRuntime`, `BrowserWorkerRunner`); `@rasm/ts/core` (`ArtifactFrame`, `Digest`, `Residency`, type `WireFault`); `effect` (`Chunk`, `Effect`, `Either`, `Layer`, `Option`, `Schema`, `Stream`).

```typescript
import { WorkerRunner } from "@effect/platform"
import { BrowserRuntime, BrowserWorkerRunner } from "@effect/platform-browser"
import { ArtifactFrame, type ContentKey, Digest, Residency, type WireFault } from "@rasm/ts/core"
import { Chunk, Effect, Either, Layer, Option, Schema, Stream } from "effect"
import { Pool, PoolFault } from "./fetch.ts"

const _folded = (fault: WireFault): PoolFault =>
  new PoolFault({
    reason: fault.reason === "parity" || fault.reason === "sequence" || fault.reason === "overrun"
      ? fault.reason
      : "codec",
    detail: fault.detail,
    evidence: Option.map(fault.evidence, ({ actual, expected }) => ({ actual: String(actual), expected: String(expected) })),
  })

const _codec = (detail: string): PoolFault =>
  new PoolFault({ reason: "codec", detail, evidence: Option.none() })

const _assembled = (
  key: ContentKey,
  frames: ReadonlyArray<Uint8Array>,
): Effect.Effect<{ readonly key: ContentKey; readonly extent: number; readonly octets: Uint8Array }, PoolFault> =>
  Stream.fromIterable(frames).pipe(
    Stream.mapEffect((band) => Effect.mapError(Schema.decode(ArtifactFrame.frame)(band), (fault) => _codec(String(fault))), { concurrency: 1 }),
    (decoded) => ArtifactFrame.reassembled(decoded),
    Stream.runCollect,
    Effect.filterOrFail((emitted) => Chunk.size(emitted) <= 1, () => _codec("<split-artifact>")),
    Effect.flatMap((emitted) =>
      Option.match(Chunk.head(emitted), {
        onNone: () => Effect.fail(_codec("<incomplete-artifact>")),
        onSome: Either.match({
          onLeft: (fault) => Effect.fail(_folded(fault)),
          onRight: ([artifact, octets]) =>
            artifact.key === key
              ? Effect.succeed({ key, extent: artifact.extent, octets })
              : Effect.fail(new PoolFault({ reason: "parity", detail: "<declared-key-mismatch>", evidence: Option.some({ actual: artifact.key, expected: key }) })),
        }),
      }),
    ),
  )

const _handlers = WorkerRunner.layerSerialized(Pool.protocol, {
  Assemble: ({ frames, key }) => _assembled(key, frames),
  Verify: ({ key, octets }) =>
    Effect.flatMap(Digest.mint("content", octets), (minted) =>
      minted === key
        ? Effect.succeed({ key, extent: octets.length })
        : Effect.fail(new PoolFault({ reason: "parity", detail: "<reverify-mismatch>", evidence: Option.some({ actual: minted, expected: key }) })),
    ),
  Chart: ({ bytes }) => Effect.mapError(Schema.decode(Residency.envelope)(bytes), (fault) => _codec(String(fault))),
})

BrowserRuntime.runMain(WorkerRunner.launch(Layer.provide(_handlers, BrowserWorkerRunner.layer)))

// --- [EXPORTS] --------------------------------------------------------------------------

export { Depot, Fetch, FetchFault, Pool, PoolFault, Web }
```
