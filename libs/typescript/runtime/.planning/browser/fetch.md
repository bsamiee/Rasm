# [RUNTIME_FETCH]

Browser byte transport and the folder's kernel-delegating mint site: the browser runtime binding rows (the XHR `HttpClient` with forced-arraybuffer responses, the `WebSocket` constructor row, and the worker spawner), the per-class flow-policy rows governing how a response becomes a backpressured `Stream`, the decorated dial every browser egress rides, and the decode-worker pool behind one closed `Schema.TaggedRequest` protocol with every band crossing zero-copy as a declared `Transferable`. That dial composes — never forks — the branch egress law: `net/client`'s `Client.dial` carries lane policy (status admission, transient retry, budget, trace propagation), and this page adds exactly the browser plane on top: the offline gate over `boot#SIGNAL_CELLS`'s cell, the CSRF echo from `route#SESSION_PLANE` on every mutating method, and the streaming modality with its flow rows. Its decode worker composes the settled interchange surfaces — `ArtifactFrame.reassembled` for the ordered fold-and-verify, `Residency.envelope` for the plan decode, the codec registry's `Wire.decode` for the set document — and delegates its cache-reverify mint to the one core `Digest` content row; a second content-address notion, a main-thread re-decode re-paying the offloaded cost, an untyped `postMessage` arm, or a bare `fetch` is the named defect. `Depot` is the window-side residency scheduler and dome joiner the app composes into the ui wave's declared viewport port at the root, because the browser and ui waves never import each other. Its modules are `runtime/src/browser/fetch.ts` and the terminal worker entry `runtime/src/browser/fetch.worker.ts`.

## [01]-[INDEX]

- [02]-[BINDING_ROWS]: the browser runtime rows — XHR client, socket constructor, spawner; `Web`.
- [03]-[FLOW_ROWS]: the per-class buffer, ceiling, and rate policy rows; `Fetch` (types).
- [04]-[DIAL_SURFACE]: the decorated dial, the offline gate, the streaming and decoded modalities; `Fetch`, `FetchFault`.
- [05]-[WIRE_PROTOCOL]: the request family, the serialized fault, the pool Tag and layer; `Pool`, `PoolFault`.
- [06]-[DEPOT_SCHEDULER]: the residency ledger, the permit governor, the haul pass, the dome lane; `Depot`.
- [07]-[RUNNER_ENTRY]: the worker-side boot module and its handler record; none.

## [02]-[BINDING_ROWS]

[BINDING_ROWS]:
- Owner: `Web`, the browser counterpart of `proc/exec#RUNTIME_ROWS` — one `as const` roster of the Layer rows the browser root merges: `client` (`BrowserHttpClient.layerXMLHttpRequest`, the `HttpClient` every `Client.dial` reaches — XHR selected deliberately for the shipped arraybuffer-response control), `socket` (`BrowserSocket.layerWebSocketConstructor`, the `Socket.WebSocketConstructor` row `net/channel`'s framed transport and `persist#OVERLAY_AND_LANE`'s sync row construct against), `channel(url)` (`BrowserSocket.layerWebSocket`, the ready socket for a fixed peer), and `workers(spawn)` (`BrowserWorker.layer` over the app-supplied spawn factory — the worker script URL is app data, never a lib literal).
- Law: the rows compose once at `boot#SINGLE_BOOT`'s root — a lane never names a binding fact, domain modules import zero rows, and the per-runtime subpath gate keeps node and bun bindings physically unresolvable from this lane; the OTLP exporter and the config chain dial through the same XHR client, so telemetry egress inherits the browser posture like every other call.
- Law: the credentials posture is root material — the cookie-carrying dial rides `route#SESSION_PLANE`'s `posture` row configured at the client binding, so no per-call credentials knob exists.
- Growth: a new browser binding (a shared-worker spawner row, a WebTransport row) is one roster entry consumed at the same root seam.
- Boundary: the abstract Tags are `@effect/platform`'s; the node and bun rows are `proc/exec#RUNTIME_ROWS`'s; which rows an app merges is root selection.
- Packages: `@effect/platform-browser` (`BrowserHttpClient`, `BrowserSocket`, `BrowserWorker`).

```typescript
import type { HttpClient, HttpClientError } from "@effect/platform"
import { HttpClientRequest, type HttpClientResponse, Transferable, Worker, type WorkerError } from "@effect/platform"
import { BrowserHttpClient, BrowserSocket, BrowserWorker } from "@effect/platform-browser"
import { Tracer as OtelBridge } from "@effect/opentelemetry"
import type { HrTime, Span } from "@opentelemetry/api"
import { hrTime } from "@opentelemetry/core"
import { Vital } from "../otel/vital.ts"
import { AssetSetManifest, ContentKey, FaultClass, Residency } from "@rasm/ts/core"
import { Array, Chunk, Context, Data, type Duration, Effect, HashMap, Layer, Option, Order, type ParseResult, Predicate, Schedule, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import { Client, type Lapse } from "../net/client.ts"
import { Boot, Connect } from "./boot.ts"
import { Kv, Opfs } from "./persist.ts"
import { Vault } from "./route.ts"

const Web = {
  client: BrowserHttpClient.layerXMLHttpRequest,
  socket: BrowserSocket.layerWebSocketConstructor,
  channel: (url: string) => BrowserSocket.layerWebSocket(url),
  workers: (spawn: (id: number) => globalThis.Worker) => BrowserWorker.layer(spawn),
} as const
```

## [03]-[FLOW_ROWS]

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
const _SETTLE = { probe: "50 millis", probes: 6 } as const // the timing-entry settle window: six spaced probes mirror the reference observer wait

type _Rate = { readonly units: number; readonly per: Duration.DurationInput; readonly burst: number }

const _flows = {
  artifact: { intake: 64, posture: "suspend", cap: Option.some(268435456), rate: Option.none<_Rate>() },
  media: { intake: 32, posture: "sliding", cap: Option.none<number>(), rate: Option.some<_Rate>({ units: 1048576, per: "1 second", burst: 4194304 }) },
  live: { intake: 16, posture: "suspend", cap: Option.none<number>(), rate: Option.none<_Rate>() },
} as const

// the family seam closes the reason set once: the tuple derives the type, the frozen rows derive the class projection
const _fetchFamily = FaultClass.family(["offline", "overrun"] as const, {
  offline: { class: "unavailable" },
  overrun: { class: "invalid" },
})

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
  type Reason = (typeof _fetchFamily.reasons)[number]
}

class FetchFault extends Data.TaggedError("FetchFault")<{
  readonly reason: FetchFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _fetchFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<fetch:${this.reason}> ${this.detail}`
  }
}
```

## [04]-[DIAL_SURFACE]

[DIAL_SURFACE]:
- Owner: `Fetch`, one `Effect.Service` over `Connect` and `Vault` — `pull(lane, request, flow)` is the streaming modality: the offline gate, the decoration, the host dial, and the response's byte stream under the flow row's buffer, ceiling, and shaped rate, returned as one `Stream<Uint8Array>` whose scope rides the stream's own lifetime and whose binary read runs under `BrowserHttpClient.withXHRArrayBuffer` so bands arrive as octets, never re-decoded text; `send(lane, request, shape)` is the decoded modality: the same gate and decoration delegated to the host dial's fused decode, one self-contained step.
- Law: decoration is recoverable from this declaration — a mutating method (`POST`/`PUT`/`PATCH`/`DELETE`) stamps the CSRF echo pair read from `Vault.csrf`, absence stamping nothing (the server refuses, the browser never guesses); the cookie credentials posture rides `[2]`'s root row, so no per-call credentials knob exists.
- Law: the offline gate reads the one cell — `Connect.online` false short-circuits to the class-carried `offline` fault before any byte moves; the gate is a fast-fail courtesy, not truth (a race with the cell is settled by the transport fault the host rail already types).
- Law: three fault families, none re-wrapped — this page's `FetchFault` carries only the browser-plane reasons (`offline`, `overrun`) over one core `FaultClass.family` mint, so the reason type, the reason schema, and the class projection derive from one frozen row table and no local guard pair or rank column exists; transport and status faults ride the platform's `HttpClientError` untouched, budget expiry rides `net/client`'s `Lapse`, and decode skew rides `ParseError` — every family already routable, each carrying its `FaultClass` projection.
- Law: no progress modality exists on this surface — the shipped `BrowserHttpClient` declarations expose `layerXMLHttpRequest`, `currentXHRResponseType`, and `withXHRArrayBuffer` and NO upload/download progress observation member, a verified absence; a per-transfer progress feed therefore lands only when the platform binding ships the member, and a hand-attached `XMLHttpRequest.upload` listener beside the owned client is the second-transport defect, never a stopgap.
- Law: the timing projection is `otel/vital`'s `Vital.enrich` folded at the dial's stream end — the enrich target is the CALLER-held live span read through `Tracer.currentOtelSpan` at dial time, still open when the byte stream drains because the stream is consumed inside the caller's own span window; the platform client's interior request span is refuted as the target — its `Effect.useSpan` body settles at response arrival, before any body byte and before the matching `PerformanceResourceTiming` entry lands, and no public surface reaches its handle. The dial owns the request coordinates (url, the `hrTime()` exchange window, one document-scoped `used` ledger), the vital page owns the Performance-Timeline read, and the entry race resolves by a bounded settle poll — the timing buffer is probed on a spaced schedule up to the reference implementation's observer wait, a miss forfeiting enrichment as a telemetry gap, never a fault — so XHR spans gain the network timing the browser's fetch instrumentation never sees.
- Entry: `Fetch.pull` / `Fetch.send`; `R` carries `HttpClient` outward to the root through the host dial.
- Receipt: the stated annotations are the seam contract — the streaming modality's error union names every family a consumer meets, readable without the body.
- Boundary: which requests exist is the consumer's vocabulary over `HttpClientRequest` at full depth; scheduling of artifact pulls is `[6]`'s; parked offline intents are `shell#REPLAY_DRAIN`'s outbox.
- Packages: `../net/client.ts` (`Client`, `Lapse`); `../otel/vital.ts` (`Vital`); `@effect/platform` (`HttpClientRequest`, `HttpClientResponse`, `HttpClientError`); `@effect/platform-browser` (`BrowserHttpClient`); `effect` (`Array`, `Chunk`, `Data`, `Effect`, `Option`, `Stream`); `./boot.ts` (`Connect`); `./route.ts` (`Vault`).

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
    const used = new WeakSet<PerformanceResourceTiming>() // one document-scoped ledger: an entry enriches exactly one span, never a neighbor's
    const _enriched = (span: Option.Option<Span>, url: string, opened: HrTime) =>
      Option.match(span, {
        onNone: () => Effect.void,
        onSome: (live) =>
          Effect.flatMap(Effect.sync(() => hrTime()), (closed) =>
            Effect.repeat(
              Effect.sync(() =>
                Vital.enrich(live, { url, start: opened, end: closed, initiator: Option.some("xmlhttprequest"), used }),
              ),
              { until: Option.isSome, schedule: Schedule.spaced(_SETTLE.probe), times: _SETTLE.probes }, // the bounded settle poll: the reference observer wait as spaced probes, a miss forfeits enrichment
            ),
          ).pipe(Effect.asVoid),
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
          const opened = hrTime() // timeline coordinate: getResource matches entries inside this window
          const span = yield* Effect.optionFromOptional(OtelBridge.currentOtelSpan) // the caller-held live span — the enrich target that outlives the drain
          const response: HttpClientResponse.HttpClientResponse = yield* BrowserHttpClient.withXHRArrayBuffer(
            Client.dial(lane, decorated),
          )
          const row = _flows[flow]
          return response.stream.pipe(
            Stream.buffer({ capacity: row.intake, strategy: row.posture }),
            _capped(row.cap),
            _shaped(row, frugal),
            Stream.ensuring(_enriched(span, decorated.url, opened)), // the projection runs at stream end, inside the caller's still-open window
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

## [05]-[WIRE_PROTOCOL]

[WIRE_PROTOCOL]:
- Owner: `Pool` — the `Context.Tag` holding the `Worker.SerializedWorkerPool` over the closed request union, the request classes riding it as statics (`Pool.Assemble`: an artifact's frame bands in, the verified receipt with its octets out; `Pool.Verify`: cache-warmed octets re-proven against their declared key — the browser's own delegated mint site; `Pool.Chart`: residency envelope bytes — manifest or delta — decoded off-thread; `Pool.Survey`: asset-set manifest bytes decoded off-thread into the `AssetSetManifest` landing `[6]`'s dome lane reads its IBL policy from), `Pool.protocol` (the union the runner checks its handlers against), and `Pool.layer(spawn)` (the pool layer over `Web.workers`, sized from `Boot.ceilings.workers`; the worker script URL is app data, never a lib literal).
- Law: every band is a declared transfer — `Transferable.Uint8Array` on the `Assemble`/`Verify`/`Chart`/`Survey` payloads and on the `Assemble` success, so frames and envelope bytes move to the worker and assembled octets move back with zero copies, and the marshal plan is recoverable from the request declarations alone.
- Law: the manifest planes decode off-thread on the same rail as the geometry planes — `Chart` lands the residency envelope and `Survey` lands the set document, so the window thread never pays a proto decode and the branch's one `Wire.decode("AssetSetManifest", octets)` call site lives in `[7]`'s handler record rather than nowhere.
- Law: `PoolFault` is `Schema.TaggedError` because it crosses the thread wire, minted over one core `FaultClass.family` seam so the reason schema on the request declarations and the class projection derive from one frozen row table — reason rows `parity` (mint mismatch, both keys held as evidence), `sequence` (broken ordinal chain), `overrun` (band over the lane cap), `codec` (frame, residency, or set-manifest decode refusal) — reconstructing as the same tagged class on the window side with its `FaultClass` projection intact, so recovery dispatches one family; the worker folds `core/interchange/codec`'s `WireFault` evidence into these rows at the handler seam, never re-throwing it.
- Law: request identity is dedup identity — structural `Equal` over the payload fields is what collapses two identical `Verify` calls in one window; the fields carry exactly the coordinate, nothing incidental.
- Growth: a new offload concern is one request class and one handler row — the union and the runner's compile-checked record break every site until both exist; a second pool per load profile restates what `Pool.layer`'s size row already carries.
- Boundary: the frame and residency byte shapes are `core/interchange/frame`'s and the set-document shape `core/interchange/codec`'s; the mint is `core/value/contentKey`'s; the pool executes, never re-models; the protocol law and sizing vocabulary are `proc/worker`'s, instantiated here on the browser rows.
- Packages: `@effect/platform` (`Transferable`, `Worker`, `WorkerError`); `@rasm/ts/core` (`AssetSetManifest`, `ContentKey`, `Residency`, `FaultClass`); `effect` (`Context`, `Layer`, `Option`, `Schema`).

```typescript
const _poolFamily = FaultClass.family(["parity", "sequence", "overrun", "codec"] as const, {
  parity: { class: "breached" },
  sequence: { class: "conflicted" },
  overrun: { class: "invalid" },
  codec: { class: "malformed" },
})

declare namespace PoolFault {
  type Reason = (typeof _poolFamily.reasons)[number]
}

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  reason: _poolFamily.schema, // the family's own literal schema: the wire reason set and the type derive from one tuple
  detail: Schema.String,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.String, expected: Schema.String }), { as: "Option" }),
}) {
  get class(): FaultClass.Kind {
    return _poolFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<pool:${this.reason}> ${this.detail}`
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

class Survey extends Schema.TaggedRequest<Survey>()("Survey", {
  payload: { bytes: Transferable.Uint8Array },
  success: AssetSetManifest, // the decoded set document travels back whole; `[6]`'s dome reads its `ibl` policy off the instance
  failure: PoolFault,
}) {}

const _protocol = Schema.Union(Assemble, Verify, Chart, Survey)

class Pool extends Context.Tag("runtime/browser/Pool")<
  Pool,
  Worker.SerializedWorkerPool<Assemble | Verify | Chart | Survey>
>() {
  static readonly Assemble = Assemble
  static readonly Verify = Verify
  static readonly Chart = Chart
  static readonly Survey = Survey
  static readonly protocol = _protocol
  static readonly layer = (
    spawn: (id: number) => globalThis.Worker,
  ): Layer.Layer<Pool, WorkerError.WorkerError, Boot> =>
    Layer.unwrapEffect(
      Effect.map(Boot, (spec) =>
        Worker.makePoolSerializedLayer(Pool, { size: spec.ceilings.workers, concurrency: 1 }).pipe(
          Layer.provide(Web.workers(spawn)),
        )),
    )
}
```

## [06]-[DEPOT_SCHEDULER]

[DEPOT_SCHEDULER]:
- Owner: `Depot`, one scoped `Effect.Service` over `Pool`, `Opfs`, and `Kv` — `ledger`, the residency cell (`core/interchange/frame`'s `Residency.Ledger`) published `Subscribable` so the port read cannot write it; `folded(arrival)` mirroring the frame page's one polymorphic fold — a `Manifest` arrival replaces the ledger (the C# render side owns truth), a `Delta` evolves it, discriminated on the value, never two entrypoints; `landed(receipt)` flipping the receipt's row to `resident`; `plan`, the pending rows as fetch orders — worst-first by LOD then smallest-first by extent so coarse geometry lands early; `haul(pull)`, one pressure-governed pass turning the current plan into verified arrivals; `dome(source, pull)`, the environment lane joining a set manifest's IBL address to the same verified-fetch leg.
- Law: the intake bound is one permit budget, never a per-pass degree — `_gate` is the service's own `Effect.makeSemaphore` and every leg takes one permit, so `haul`'s fan and a concurrent `dome` share one governor instead of each carrying an unrelated local ceiling; `_pressured` folds `Opfs`'s verdict through `_DEGREES` and `resize`s that budget at each entrypoint, so storage pressure throttles intake live with zero per-call knobs, the `Effect.partition` degree states `"unbounded"` as the written claim that the permit owner carries the real bound, and the byte-feed policy stays `[3]`'s flow row.
- Law: the shared leg is address-blind — `_hauledOne(key, bands)` takes the content key it warms, verifies, and assembles under, plus an already-resolved band stream, so each entrypoint owns its own address vocabulary (`haul` a `Residency.Row`, `dome` a set-plus-file leaf) and neither forges a coordinate to satisfy the other; a synthesized residency row standing in for a dome plane would publish an LOD and a state nothing measured.
- Law: warm before fetch — each leg first probes `persist#DOMAIN_ROWS`'s `cache` domain under its content key; a hit is admitted material and re-proves off-thread through `Verify` (the browser's delegated mint) before it lands, a `parity` refusal alone evicts the poisoned band (a transient worker or codec fault forfeits warmth without evicting) while every refused warm falls through to the fetch path, and a fresh assemble writes the cache best-effort — cache faults never fail an arrival, they only forfeit warmth.
- Law: the haul quarantines per artifact — `Effect.partition` keeps every fault beside every arrival, a `parity` refusal never blocks its siblings, and a landed receipt folds into the ledger inside the same pass so a repeated haul cannot re-order what already arrived; deltas replayed across reconnects are idempotent by the frame page's own fold law.
- Law: one pass per call — `haul` drives the plan as it stands; the continuous loop is app composition (`ledger.changes` debounced into repeated hauls), so the lib owns the pass and the app owns the cadence.
- Law: the dome lane admits both set-document modalities on one entrypoint — raw manifest octets decode off-thread through `Survey` and a landing already decoded at the interchange registry passes straight through, discriminated on the value by `Predicate.isUint8Array` rather than a modality parameter — then answers `Option`: a manifest whose `ibl` is absent is a set with no dome, not a fault, so a `pbr_set` and an `hdri` ride one call shape.
- Law: the dome hauls the equirect alone — the prefilter pyramid, the BRDF LUT, and the luminance CDF have no browser consumer because the viewer's own prefilter owns the roughness-to-mip relation, so a second haul would land bytes nothing reads; the answer carries the read policy the wire declares (`intensity`, `rotation`, `sh9`) beside the verified octets and nothing else.
- Law: the dome row never enters the ledger — residency is the geometry plane's coordinate and an environment plane has no LOD, no eviction lane, and no census tally, so `landed` stays `haul`'s alone and a dome haul leaves the census truthful.
- Receipt: `haul` yields `[faults, arrivals]` and `dome` yields `Option` of one arrival widened by its read policy — both are receipt-plus-octets shapes the app forwards into the ui wave's declared viewport port at the root, arrivals into `arrivals` and domes into `environments`; the ledger cell is the port's residency read.
- Boundary: manifest and delta ingress arrive decoded from `Residency.stream` over `[4]`'s feeds; the served-asset address both pullers resolve is the iac publish and ui read spelling, never re-derived here; the GLB scene semantics and the dome decode are the ui wave's viewer; this owner schedules and joins.
- Packages: `effect` (`Array`, `Chunk`, `Effect`, `HashMap`, `Option`, `Order`, `Predicate`, `Stream`, `Subscribable`, `SubscriptionRef`); `@effect/platform` (`WorkerError`); `@rasm/ts/core` (`AssetSetManifest`, `ContentKey`, `Residency`); `./persist.ts` (`Kv`, `Opfs`).

```typescript
const _DEGREES = { ample: 6, tight: 2, critical: 1, opaque: 2 } as const

const _byOrder: Order.Order<Residency.Row> = Order.combine(
  Order.mapInput(Order.number, (row: Residency.Row) => row.lod),
  Order.mapInput(Order.number, (row: Residency.Row) => row.extent),
)

declare namespace Depot {
  type Leaf = { readonly set: ContentKey; readonly file: string } // the served-asset coordinate; the address spelling stays iac's publish and ui's read
  type Pull<Address, E, R> = (address: Address) => Stream.Stream<Uint8Array, E, R>
  type Fault<E> = E | PoolFault | ParseResult.ParseError | WorkerError.WorkerError
  // Assembled payloads declare the non-shared generic: the ui port hands `.buffer` to platform decoders, so the
  // whole-buffer contract is structural at BOTH seam ends — the bare spelling widens to ArrayBufferLike and refuses there.
  type Landed = readonly [typeof _Receipt.Type, Uint8Array<ArrayBuffer>]
  type Dome = {
    readonly key: ContentKey
    readonly octets: Uint8Array<ArrayBuffer>
    readonly intensity: number
    readonly rotation: number
    readonly sh9: ReadonlyArray<number>
  }
  type _Degrees<T extends Record<Opfs.Verdict, number> = typeof _DEGREES> = T // a new pressure band on the residency owner breaks here, never as a stranded degree
}

class Depot extends Effect.Service<Depot>()("runtime/browser/Depot", {
  scoped: Effect.gen(function* () {
    const pool = yield* Pool
    const opfs = yield* Opfs
    const kv = yield* Kv
    const _ledger = yield* SubscriptionRef.make(HashMap.empty<ContentKey, Residency.Row>())
    const _gate = yield* Effect.makeSemaphore(_DEGREES.opaque) // one intake budget spanning every leg; the verdict retunes it live
    const _pressured: Effect.Effect<void> = Effect.flatMap(opfs.budget, (budget) => _gate.resize(_DEGREES[budget.verdict]))
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
    const _warmed = (key: ContentKey) =>
      kv.read("cache", key).pipe(
        Effect.catchTag("KvFault", () =>
          Effect.as(Effect.logWarning("browser cache read unavailable"), Option.none<Uint8Array>())),
        Effect.flatMap(
          Option.match({
            onNone: () => Effect.succeedNone,
            onSome: (band) =>
              pool.executeEffect(new Verify({ key, octets: band })).pipe(
                Effect.map((receipt) => Option.some([receipt, band] as const)),
                Effect.catchTags({
                  // a parity refusal alone evicts the poisoned band; transient worker or wire faults forfeit warmth without evicting
                  PoolFault: (fault) =>
                    Effect.as(fault.reason === "parity" ? Effect.ignoreLogged(kv.drop("cache", key)) : Effect.void, Option.none()),
                  WorkerError: () => Effect.as(Effect.logWarning("browser cache verification unavailable"), Option.none()),
                }),
              ),
          }),
        ),
      )
    // the one governed leg: address-blind, permit-bracketed, warm-then-assemble — both entrypoints resolve their own address and hand it bands
    const _hauledOne = <E, R>(key: ContentKey, bands: Stream.Stream<Uint8Array, E, R>): Effect.Effect<Depot.Landed, Depot.Fault<E>, R> =>
      _gate.withPermits(1)(
        Effect.flatMap(_warmed(key), (warm) =>
          Option.match(warm, {
            onSome: Effect.succeed,
            onNone: () =>
              Stream.runCollect(bands).pipe(
                Effect.flatMap((held) => pool.executeEffect(new Assemble({ key, frames: Chunk.toReadonlyArray(held) }))),
                Effect.tap(({ key: minted, octets }) => Effect.ignoreLogged(kv.write("cache", minted, octets))),
                Effect.map(({ extent, key: minted, octets }) => [{ key: minted, extent }, octets] as const),
              ),
          }),
        ),
      )
    const _surveyed = (
      source: Uint8Array | AssetSetManifest,
    ): Effect.Effect<AssetSetManifest, PoolFault | ParseResult.ParseError | WorkerError.WorkerError> =>
      Predicate.isUint8Array(source) ? pool.executeEffect(new Survey({ bytes: source })) : Effect.succeed(source) // one entrypoint, two set-document modalities, discriminated on the value
    const haul = <E, R>(
      pull: Depot.Pull<Residency.Row, E, R>,
    ): Effect.Effect<readonly [ReadonlyArray<Depot.Fault<E>>, ReadonlyArray<Depot.Landed>], never, R> =>
      Effect.gen(function* () {
        const orders = yield* plan
        yield* _pressured
        return yield* Effect.partition(orders, (order) => Effect.tap(_hauledOne(order.mesh, pull(order)), ([receipt]) => landed(receipt)), {
          concurrency: "unbounded", // the permit budget carries the real bound; the partition owns quarantine alone
        })
      })
    const dome = <E, R>(
      source: Uint8Array | AssetSetManifest,
      pull: Depot.Pull<Depot.Leaf, E, R>,
    ): Effect.Effect<Option.Option<Depot.Dome>, Depot.Fault<E>, R> =>
      _pressured.pipe(
        Effect.zipRight(_surveyed(source)),
        Effect.flatMap((manifest) =>
          Option.match(manifest.ibl, {
            onNone: () => Effect.succeedNone, // a set with no dome is an answer, never a fault
            onSome: (ibl) =>
              Effect.map(
                _hauledOne(ibl.equirect.address, pull({ set: manifest.manifestKey, file: ibl.equirect.file })),
                ([, octets]): Option.Option<Depot.Dome> =>
                  Option.some({ key: ibl.equirect.address, octets, intensity: ibl.intensity, rotation: ibl.rotation, sh9: ibl.sh9 }),
              ),
          }),
        ),
      )
    const ledger: Subscribable.Subscribable<HashMap.HashMap<ContentKey, Residency.Row>> = _ledger
    return {
      ledger,
      plan,
      landed,
      haul,
      dome,
      folded: (arrival: Residency.Arrival) => SubscriptionRef.update(_ledger, (held) => Residency.folded(held, arrival)),
    }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Depot, Fetch, FetchFault, Pool, PoolFault, Web }
```

## [07]-[RUNNER_ENTRY]

[RUNNER_ENTRY]:
- Owner: the worker-side boot module `runtime/src/browser/fetch.worker.ts` — its OWN terminal entry, physically separate from the browser module: the app's spawn factory names this script, it composes `WorkerRunner.layerSerialized(Pool.protocol, handlers)` under `BrowserWorkerRunner.layer`, launches that as its whole life, and exports nothing — the empty exports block is the structural proof it is terminal. A worker thread is its own process under the boot-edge law, so its `runMain` is the thread's one boot and never a second document boot; the entry imports only declaration-space owners (`Pool`/`PoolFault` from `./fetch.ts` — the module law keeps every top level inert, so the window graph's services never instantiate in the worker), and a `runMain` or an export block inside `fetch.ts` itself is the mixed-module defect this split forecloses.
- Law: the handler record is compile-checked against the union — `Assemble` decodes each band through `ArtifactFrame.frame`, drives the settled `ArtifactFrame.reassembled` fold (ordering, join, and the frame-side `Parity` verify all inside the frame page's law), demands exactly one completed artifact (an empty or split reassembly refuses typed), and refuses a key mismatch with the declared key as evidence; `Verify` is the browser's delegated mint — `Digest.mint("content", octets)` over the presented octets compared to the declared key, `parity` with both keys on refusal — one of the branch's three sanctioned content-mint delegation sites; `Chart` decodes the envelope bytes through `Residency.envelope`, yielding the manifest-or-delta arrival; `Survey` decodes the set document through the codec registry's own one-hop entry, `Wire.decode("AssetSetManifest", bytes)`, so the browser reads the python-assembled ingest/IBL manifest through the registry that owns the family rather than a second landing.
- Law: fault folding is total at this seam — a `WireFault` from the frame fold maps `parity`/`sequence`/`overrun` reason-to-reason into `PoolFault` with its evidence stringified and every other codec reason folds to `codec`; the worker never throws across the boundary because the request's failure schema is the only exit.
- Law: the handlers run one request at a time per worker (`concurrency: 1` at the pool) because reassembly is memory-bound, and the pool's `size` row is the parallelism — worker count, not handler interleaving, scales throughput; the size row arrives from `boot#BUDGET_VALUE`'s `workers` ceiling at composition.
- Boundary: the spawn factory, the script URL, and the bundler wiring are app build material; this module is the script's content law.
- Packages: `@effect/platform` (`WorkerRunner`); `@effect/platform-browser` (`BrowserRuntime`, `BrowserWorkerRunner`); `@rasm/ts/core` (`ArtifactFrame`, `Digest`, `Residency`, `Wire`, type `WireFault`); `effect` (`Chunk`, `Effect`, `Either`, `Layer`, `Option`, `Schema`, `Stream`).

```typescript
// runtime/src/browser/fetch.worker.ts — the decode-worker terminal entry; its own thread's boot, never part of the browser module
import { WorkerRunner } from "@effect/platform"
import { BrowserRuntime, BrowserWorkerRunner } from "@effect/platform-browser"
import { ArtifactFrame, type ContentKey, Digest, Residency, Wire, type WireFault } from "@rasm/ts/core"
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
  Survey: ({ bytes }) => Effect.mapError(Wire.decode("AssetSetManifest", bytes), (fault) => _codec(String(fault))),
})

BrowserRuntime.runMain(WorkerRunner.launch(Layer.provide(_handlers, BrowserWorkerRunner.layer)))

// --- [EXPORTS] --------------------------------------------------------------------------

export {} // terminal entry: the empty surface is the structural proof
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
