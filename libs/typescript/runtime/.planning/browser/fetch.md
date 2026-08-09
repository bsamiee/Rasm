# [RUNTIME_FETCH]

Browser runtime rows, byte-flow policy, worker decoding, and generation-scoped residency compose through `Web`, `Fetch`, `Pool`, and `Depot`.

## [01]-[INDEX]

- [02]-[BINDING_ROWS]: the browser runtime rows — XHR client, socket constructor, spawner; `Web`.
- [03]-[FLOW_ROWS]: the per-class buffer, ceiling, and rate policy rows beside the link-grade budgets; `Fetch` (types).
- [04]-[DIAL_SURFACE]: the shared prelude, the credential stamp, the streaming and decoded modalities; `Fetch`, `FetchFault`.
- [05]-[WIRE_PROTOCOL]: the request family, the raster crossing, the serialized fault, the pool Tag and layer; `Pool`, `PoolFault`.
- [06]-[DEPOT_SCHEDULER]: the residency ledger, the permit governor, the haul pass, the dome lane; `Depot`.
- [07]-[RUNNER_ENTRY]: the worker-side boot module and its handler record; none.

## [02]-[BINDING_ROWS]

- Owner: `Web` publishes the browser HTTP, socket, and worker Layers.
- Law: spawn rows carry the platform's own union — a dedicated worker, a `SharedWorker` fanning one decode pool across a document set, and a bare `MessagePort` handed in by a foreign host all satisfy `BrowserWorker.layer`, so a pool outliving one document is a spawner choice, never a second binding row.
- Law: `channel` forwards the socket's own close-code policy — a live feed closing 1000 or 1006 ends a subscription rather than failing it, and `closeCodeIsError` is where that reading is declared, so a channel row and its error vocabulary stay one decision.
- Boundary: the app supplies URLs, the spawner, and root Layer composition; `route#SESSION_PLANE` owns the credential posture `[04]` stamps and the CSRF pair it echoes.

```typescript
import type { HttpClient, HttpClientError } from "@effect/platform"
import { HttpClientRequest, type HttpClientResponse, Transferable, Worker, type WorkerError } from "@effect/platform"
import { BrowserHttpClient, BrowserSocket, BrowserWorker } from "@effect/platform-browser"
import { Tracer as OtelBridge } from "@effect/opentelemetry"
import type { HrTime, Span } from "@opentelemetry/api"
import { hrTime } from "@opentelemetry/core"
import { Vital } from "../otel/vital.ts"
import { Digest, Fault, Frame, Shape, Wire } from "@rasm/ts/core"
import { Array, Chunk, Context, Data, type Duration, Effect, Either, HashMap, Layer, Option, Order, type ParseResult, Predicate, Schedule, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import { Client, type Lapse } from "../net/client.ts"
import { Boot, Connect } from "./boot.ts"
import { Kv, Opfs } from "./persist.ts"
import { Vault } from "./route.ts"

// Spawning is the platform's own union, so a shared decode pool across a document set and a foreign-host port arrive
// as spawner values instead of forking this row; `Pool.layer` takes the same alias so both ends admit one vocabulary.
type _Spawn = (id: number) => globalThis.Worker | globalThis.SharedWorker | MessagePort

const Web = {
  client: BrowserHttpClient.layerXMLHttpRequest,
  socket: BrowserSocket.layerWebSocketConstructor,
  channel: (url: string, options?: { readonly closeCodeIsError: (code: number) => boolean }) =>
    BrowserSocket.layerWebSocket(url, options),
  workers: (spawn: _Spawn) => BrowserWorker.layer(spawn),
} as const
```

## [03]-[FLOW_ROWS]

- Owner: `_flows` carries buffer, ceiling, and rate policy for each byte-feed class; `_LINKS` carries the scale and leg budget each measured link grade admits.
- Law: byte ceilings fail in-stream, and a link row scales only declared rate rows — a ceiling is a contract no link condition renegotiates.
- Law: `_LINKS` keys on `boot#SIGNAL_CELLS`'s three-grade axis whole, never on `saveData` alone — a two-case frugal switch strands `steady` and `strained` on the swift budget, so the grade selects the row and the user's own `saveData` declaration floors it to `strained`; a host withholding the surface rides `swift`, because a withheld reading is no evidence of a poor link.
- Growth: a new byte-feed class is one `_flows` row; a new grade on the signal owner breaks `_Links` until its budget lands.

```typescript
const _MUTATING = ["POST", "PUT", "PATCH", "DELETE"] as const
const _SETTLE = { probe: "50 millis", probes: 6 } as const // the timing-entry settle window: six spaced probes mirror the reference observer wait

// Link rows price the DECLARED rate rows and the concurrent leg budget, never a byte ceiling: `scale` shapes throttled
// classes and `legs` bounds `[06]`'s intake against the storage verdict's own degree, whichever reads tighter.
const _LINKS = {
  swift: { scale: 1, legs: 6 },
  steady: { scale: 0.5, legs: 4 },
  strained: { scale: 0.25, legs: 2 },
} as const

type _Rate = { readonly units: number; readonly per: Duration.DurationInput; readonly burst: number }

const _flows = {
  artifact: { intake: 64, posture: "suspend", cap: Option.some(268435456), rate: Option.none<_Rate>() },
  media: { intake: 32, posture: "sliding", cap: Option.none<number>(), rate: Option.some<_Rate>({ units: 1048576, per: "1 second", burst: 4194304 }) },
  live: { intake: 16, posture: "suspend", cap: Option.none<number>(), rate: Option.none<_Rate>() },
} as const

// `saveData` outranks the measured grade because a user declaration is never a measurement, and an unmeasured host
// takes the unscaled row rather than a defensive floor no reading supports
const _linked = (profile: Option.Option<Connect.Profile>): Fetch.Link =>
  Option.match(profile, {
    onNone: () => _LINKS.swift,
    onSome: (held) => (held.frugal ? _LINKS.strained : _LINKS[held.grade]),
  })

// one family seam closes the reason set: the tuple derives the type, the frozen rows derive the class projection
const _fetchFamily = Fault.Class.family(["offline", "overrun"] as const, {
  offline: { class: "unavailable" },
  overrun: { class: "invalid" },
})

declare namespace Fetch {
  type Flow = keyof typeof _flows
  type Link = (typeof _LINKS)[Connect.Grade]
  type Row = {
    readonly intake: number
    readonly posture: "suspend" | "dropping" | "sliding"
    readonly cap: Option.Option<number>
    readonly rate: Option.Option<_Rate>
  }
  type _Rows<T extends Record<Flow, Row> = typeof _flows> = T
  type _Links<T extends Record<Connect.Grade, Link> = typeof _LINKS> = T // a new grade on the signal owner breaks here, never as a stranded budget
}

declare namespace FetchFault {
  type Reason = (typeof _fetchFamily.reasons)[number]
}

class FetchFault extends Data.TaggedError("FetchFault")<{
  readonly reason: FetchFault.Reason
  readonly detail: string
}> {
  get class(): Fault.Class.Kind {
    return _fetchFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<fetch:${this.reason}> ${this.detail}`
  }
}
```

## [04]-[DIAL_SURFACE]

- Owner: `Fetch` decorates and dials browser requests; `FetchFault` carries browser-only offline and overrun reasons.
- Law: one flow row controls buffering, accumulated bytes, and rate shaping.
- Law: both modalities share one prelude — `_opened` gates on connectivity, decorates the CSRF echo, reads the link row, and opens the timeline window, so a decoded request enriches its span on the terms a streamed one does; enrichment reaching the streaming arm alone strands every request the app sends through a Schema.
- Law: credentials ride the XHR factory, never a request header — `BrowserHttpClient.layerXMLHttpRequest` constructs a bare `XMLHttpRequest` whose `withCredentials` reads false, so a cross-origin dial drops the session cookie `route#SESSION_PLANE` issued; `_stamped` supplies the platform's factory Tag under `Vault.posture` and every dial crosses it.
- Law: `withCredentials` spells `include` alone — XHR sends same-origin cookies unconditionally, so `omit` has no expression on this transport and folds onto the constructor's own same-origin default rather than minting a client row that cannot honor it.
- Boundary: `Client` owns transport retry and status admission; `Vital` owns browser timing enrichment.

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

// XHR carries same-origin cookies unconditionally and cross-origin ones only under `withCredentials`, so the posture
// resolves to one boolean at the factory the platform reads out of the calling fiber's context
const _wire = (posture: RequestCredentials) => (): globalThis.XMLHttpRequest => {
  const held = new globalThis.XMLHttpRequest()
  held.withCredentials = posture === "include"
  return held
}

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
    const _shaped = (row: Fetch.Row, scale: number) => <E, R>(bands: Stream.Stream<Uint8Array, E, R>): Stream.Stream<Uint8Array, E, R> =>
      Option.match(row.rate, {
        onNone: () => bands,
        onSome: (rate) =>
          Stream.throttle(bands, {
            cost: (chunk) => Chunk.reduce(chunk, 0, (total, band) => total + band.length),
            units: Math.ceil(rate.units * scale),
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
    const _stamped = <A, E, R>(dial: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      Effect.provideService(dial, BrowserHttpClient.XMLHttpRequest, _wire(vault.posture))
    // one prelude for both modalities: connectivity gate, CSRF decoration, link row, and the timeline window whose
    // finalizer each arm hands its own terminator, so a decoded request is never the unenriched one
    const _opened = (
      request: HttpClientRequest.HttpClientRequest,
    ): Effect.Effect<
      {
        readonly decorated: HttpClientRequest.HttpClientRequest
        readonly link: Fetch.Link
        readonly settled: Effect.Effect<void>
      },
      FetchFault
    > =>
      Effect.gen(function* () {
        yield* _gated
        const decorated = yield* _decorated(request)
        const link = _linked(yield* connect.profile.get)
        const opened = hrTime() // timeline coordinate: getResource matches entries inside this window
        const span = yield* Effect.optionFromOptional(OtelBridge.currentOtelSpan) // caller-held live span, the enrich target outliving the drain
        return { decorated, link, settled: _enriched(span, decorated.url, opened) }
      })
    const pull = (
      lane: Client.Lane,
      request: HttpClientRequest.HttpClientRequest,
      flow: Fetch.Flow,
    ): Stream.Stream<Uint8Array, FetchFault | HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient> =>
      Stream.unwrapScoped(
        Effect.gen(function* () {
          const { decorated, link, settled } = yield* _opened(request)
          const response: HttpClientResponse.HttpClientResponse = yield* BrowserHttpClient.withXHRArrayBuffer(
            _stamped(Client.dial(lane, decorated)),
          )
          const row = _flows[flow]
          return response.stream.pipe(
            Stream.buffer({ capacity: row.intake, strategy: row.posture }),
            _capped(row.cap),
            _shaped(row, link.scale),
            Stream.ensuring(settled), // the projection runs at stream end, inside the caller's still-open window
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
    > =>
      Effect.flatMap(_opened(request), ({ decorated, settled }) =>
        Effect.ensuring(_stamped(Client.dial(lane, decorated, shape)), settled))
    return { pull, send }
  }),
}) {}
```

## [05]-[WIRE_PROTOCOL]

- Owner: `Pool` closes the worker protocol over assemble, verify, residency, asset-manifest, and raster-encode requests.
- Law: transferable schemas carry every byte buffer, and `Fault.Class` classifies the one serialized `PoolFault` family.
- Law: pixel crossings are seated HERE and only here — `proc/worker` refuses the row because its node and bun members never construct the DOM type, so this browser-only protocol owns `Transferable.ImageData`, whose transfer list projects `data.buffer` and moves the plane rather than copying it.
- Law: raster encode belongs to the pool, never the document — a readback band's PNG encode blocks the frame that produced it, so `Imprint` takes the plane and answers octets, and `OffscreenCanvas` is the encoder because a worker holds no DOM canvas.
- Boundary: `Frame` owns artifact and residency decoding, `Wire` owns asset-manifest decoding, `Digest` owns minting, and `ui/view/export#SERIALIZER_MATRIX`'s readback arm is the raster consumer that hands `Imprint` its plane.

```typescript
const _poolFamily = Fault.Class.family(["parity", "sequence", "overrun", "codec"] as const, {
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
  get class(): Fault.Class.Kind {
    return _poolFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<pool:${this.reason}> ${this.detail}`
  }
}

const _Receipt = Schema.Struct({
  key: Digest.Key.content,
  generation: Shape.Refined.OrdinalKey,
  extent: Shape.Refined.OrdinalKey,
})

class Assemble extends Schema.TaggedRequest<Assemble>()("Assemble", {
  payload: { key: Digest.Key.content, frames: Schema.Array(Transferable.Uint8Array) },
  success: Schema.Struct({ ..._Receipt.fields, octets: Transferable.Uint8Array }),
  failure: PoolFault,
}) {}

class Verify extends Schema.TaggedRequest<Verify>()("Verify", {
  payload: { key: Digest.Key.content, octets: Transferable.Uint8Array },
  success: Schema.Struct({ key: Digest.Key.content, extent: Shape.Refined.OrdinalKey }),
  failure: PoolFault,
}) {}

class Chart extends Schema.TaggedRequest<Chart>()("Chart", {
  payload: { bytes: Transferable.Uint8Array },
  success: Schema.Union(Frame.Residency.Manifest, Frame.Residency.Delta),
  failure: PoolFault,
}) {}

class Survey extends Schema.TaggedRequest<Survey>()("Survey", {
  payload: { bytes: Transferable.Uint8Array },
  success: Wire.AssetSetManifest,
  failure: PoolFault,
}) {}

// `convertToBlob` admits these three raster codecs, and the mime derives from the row rather than a literal, so a new
// admitted codec is one tuple member and no call site carries an encoder name; quality reaches the lossy rows alone
const _IMPRINTS = ["png", "jpeg", "webp"] as const

class Imprint extends Schema.TaggedRequest<Imprint>()("Imprint", {
  payload: {
    pixels: Transferable.ImageData,
    codec: Schema.Literal(..._IMPRINTS),
    quality: Schema.optionalWith(Schema.Number.pipe(Schema.between(0, 1)), { as: "Option" }),
  },
  success: Transferable.Uint8Array,
  failure: PoolFault,
}) {}

const _protocol = Schema.Union(Assemble, Verify, Chart, Survey, Imprint)

class Pool extends Context.Tag("runtime/browser/Pool")<
  Pool,
  Worker.SerializedWorkerPool<Assemble | Verify | Chart | Survey | Imprint>
>() {
  static readonly Assemble = Assemble
  static readonly Verify = Verify
  static readonly Chart = Chart
  static readonly Survey = Survey
  static readonly Imprint = Imprint
  static readonly protocol = _protocol
  static readonly layer = (
    spawn: _Spawn,
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

- Owner: `Depot` folds generation-scoped residency, schedules pending rows, and shares one governed byte-haul leg.
- Law: manifest replacement advances generation; stale arrivals cannot mutate the ledger or enter the scene port.
- Law: geometry arrivals carry `{ key, generation, extent }`; dome artifacts reuse verification without entering residency.
- Law: one `_admitted` read per pass answers both governors and carries the band's own admission row into every leg — storage pressure prices how fast this origin may fill, link grade prices how many legs are worth opening, and the tighter degree resizes the one gate; a second read inside a leg prices a pass already in flight.
- Law: `warmDepot` refuses ADDING residency, never a pull — a near-full origin still serves the scene and an already-warm band still reads, so the admission gates the cache write alone and eviction pressure prices new rows.
- Boundary: callers resolve served addresses and forward ledger, geometry arrivals, and dome arrivals to the UI port.

```typescript
const _DEGREES = { ample: 6, tight: 2, critical: 1, opaque: 2 } as const

const _byOrder: Order.Order<Frame.ResidencyRow> = Order.combine(
  Order.mapInput(Order.number, (row: Frame.ResidencyRow) => row.lod),
  Order.mapInput(Order.number, (row: Frame.ResidencyRow) => row.extent),
)

declare namespace Depot {
  type Leaf = { readonly set: Digest.Key<"content">; readonly file: string }
  type Pull<Address, E, R> = (address: Address) => Stream.Stream<Uint8Array, E, R>
  type Fault<E> = E | PoolFault | ParseResult.ParseError | WorkerError.WorkerError
  type Order = { readonly generation: number; readonly row: Frame.ResidencyRow }
  type Landed = readonly [
    { readonly key: Digest.Key<"content">; readonly generation: number; readonly extent: number },
    Uint8Array<ArrayBuffer>,
  ]
  type Dome = {
    readonly key: Digest.Key<"content">
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
    const connect = yield* Connect
    const _ledger = yield* SubscriptionRef.make(Frame.Residency.empty)
    const _gate = yield* Effect.makeSemaphore(_DEGREES.opaque) // one intake budget spanning every leg; the two governors retune it live
    // one read per pass, two governors, one gate: storage pressure prices the fill rate and link grade prices the leg
    // count, so the tighter degree wins and the band's admission row rides into the legs the pass opens
    const _admitted: Effect.Effect<Opfs.Admission> = Effect.flatMap(opfs.budget, (budget) =>
      Effect.flatMap(connect.profile.get, (profile) =>
        Effect.as(_gate.resize(Math.min(_DEGREES[budget.verdict], _linked(profile).legs)), opfs.band[budget.verdict])))
    const plan: Effect.Effect<ReadonlyArray<Depot.Order>> = Effect.map(SubscriptionRef.get(_ledger), (ledger) =>
      Array.map(Array.sort(Frame.Residency.pending(ledger), _byOrder), (row) => ({ generation: ledger.generation, row })))
    const landed = (receipt: Depot.Landed[0]): Effect.Effect<void> =>
      SubscriptionRef.update(_ledger, (ledger) => receipt.generation !== ledger.generation
        ? ledger
        : { ...ledger, rows: HashMap.modifyAt(ledger.rows, receipt.key, (slot) =>
            Option.map(slot, (row) => ({ ...row, extent: receipt.extent, state: "resident" as const }))) })
    const _warmed = (key: Digest.Key<"content">) =>
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
                  ParseError: () => Effect.as(Effect.logWarning("browser cache receipt undecodable"), Option.none()),
                  WorkerError: () => Effect.as(Effect.logWarning("browser cache verification unavailable"), Option.none()),
                }),
              ),
          }),
        ),
      )
    // one governed leg, address-blind and permit-bracketed, warm-then-assemble: both entrypoints resolve their own address and hand it bands
    const _hauledOne = <E, R>(
      key: Digest.Key<"content">,
      generation: Option.Option<number>,
      admits: Opfs.Admission,
      bands: Stream.Stream<Uint8Array, E, R>,
    ): Effect.Effect<readonly [typeof _Receipt.Type, Uint8Array<ArrayBuffer>], Depot.Fault<E>, R> =>
      _gate.withPermits(1)(
        Effect.flatMap(Option.match(generation, { onNone: Effect.succeedNone, onSome: () => _warmed(key) }), (warm) =>
          Option.match(warm, {
            onSome: ([receipt, octets]) => Option.match(generation, {
              onNone: () => Effect.fail(new PoolFault({ reason: "sequence", detail: "<missing-generation>", evidence: Option.none() })),
              onSome: (actual) => Effect.succeed([{ ...receipt, generation: actual }, octets] as const),
            }),
            onNone: () =>
              Stream.runCollect(bands).pipe(
                Effect.flatMap((held) => pool.executeEffect(new Assemble({ key, frames: Chunk.toReadonlyArray(held) }))),
                // `warmDepot` prices ADDING residency alone, so a refusing band still serves the scene and still reads what it already holds
                Effect.tap(({ key: minted, octets }) =>
                  admits.warmDepot ? Effect.ignoreLogged(kv.write("cache", minted, octets)) : Effect.void),
                Effect.filterOrFail(
                  (receipt) => Option.forall(generation, (expected) => expected === receipt.generation),
                  () => new PoolFault({ reason: "sequence", detail: "<artifact-generation>", evidence: Option.none() }),
                ),
                Effect.map(({ extent, generation: actual, key: minted, octets }) => [
                  { key: minted, generation: Option.getOrElse(generation, () => actual), extent },
                  octets,
                ] as const),
              ),
          }),
        ),
      )
    const _surveyed = (
      source: Uint8Array | Wire.Decoded<"AssetSetManifest">,
    ): Effect.Effect<Wire.Decoded<"AssetSetManifest">, PoolFault | ParseResult.ParseError | WorkerError.WorkerError> =>
      Predicate.isUint8Array(source) ? pool.executeEffect(new Survey({ bytes: source })) : Effect.succeed(source) // one entrypoint, two set-document modalities, discriminated on the value
    const haul = <E, R>(
      pull: Depot.Pull<Frame.ResidencyRow, E, R>,
    ): Effect.Effect<readonly [ReadonlyArray<Depot.Fault<E>>, ReadonlyArray<Depot.Landed>], never, R> =>
      Effect.gen(function* () {
        const orders = yield* plan
        const admits = yield* _admitted
        return yield* Effect.partition(orders, (order) =>
          Effect.tap(
            _hauledOne(order.row.mesh, Option.some(order.generation), admits, pull(order.row)),
            ([receipt]) => landed(receipt),
          ), {
          concurrency: "unbounded", // the permit budget carries the real bound; the partition owns quarantine alone
        })
      })
    const dome = <E, R>(
      source: Uint8Array | Wire.Decoded<"AssetSetManifest">,
      pull: Depot.Pull<Depot.Leaf, E, R>,
    ): Effect.Effect<Option.Option<Depot.Dome>, Depot.Fault<E>, R> =>
      Effect.flatMap(_admitted, (admits) =>
        Effect.flatMap(_surveyed(source), (manifest) =>
          Option.match(manifest.ibl, {
            onNone: () => Effect.succeedNone, // a set with no dome is an answer, never a fault
            onSome: (ibl) =>
              Effect.map(
                _hauledOne(ibl.equirect.address, Option.none(), admits, pull({ set: manifest.manifestKey, file: ibl.equirect.file })),
                ([, octets]): Option.Option<Depot.Dome> =>
                  Option.some({ key: ibl.equirect.address, octets, intensity: ibl.intensity, rotation: ibl.rotation, sh9: ibl.sh9 }),
              ),
          }),
        ),
      )
    const ledger: Subscribable.Subscribable<Frame.ResidencyLedger> = _ledger
    return {
      ledger,
      plan,
      landed,
      haul,
      dome,
      folded: (arrival: Frame.ResidencyArrival) => SubscriptionRef.modify(_ledger, (held) =>
        Either.match(Frame.Residency.folded(held, arrival), {
          onLeft: (fault) => [Either.left(fault), held] as const,
          onRight: (next) => [Either.right(next), next] as const,
        })),
    }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Depot, Fetch, FetchFault, Pool, PoolFault, Web }
```

## [07]-[RUNNER_ENTRY]

- Owner: the terminal worker maps every request class to the canonical `Frame`, `Wire`, and `Digest` owners.
- Law: artifact assembly preserves generation and maps all boundary failures into `PoolFault`.
- Law: raster encode runs on `OffscreenCanvas` — a worker thread reaches no DOM canvas, `convertToBlob` is its one encoder, and the codec row supplies the mime while quality reaches the lossy rows alone; a refused context or encoder folds to `codec` like every other boundary failure.
- Boundary: the app owns worker script wiring and process boot.

```typescript
// runtime/src/browser/fetch.worker.ts — the decode-worker terminal entry; its own thread's boot, never part of the browser module
import { WorkerRunner } from "@effect/platform"
import { BrowserRuntime, BrowserWorkerRunner } from "@effect/platform-browser"
import { Digest, Frame, Wire } from "@rasm/ts/core"
import { Chunk, Effect, Either, Layer, Option, Schema, Stream } from "effect"
import { Pool, PoolFault } from "./fetch.ts"

const _folded = (fault: Wire.Fault): PoolFault =>
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
  key: Digest.Key<"content">,
  frames: ReadonlyArray<Uint8Array>,
): Effect.Effect<{
  readonly key: Digest.Key<"content">
  readonly generation: number
  readonly extent: number
  readonly octets: Uint8Array
}, PoolFault> =>
  Stream.fromIterable(frames).pipe(
    Stream.mapEffect((band) => Effect.mapError(Schema.decode(Frame.Artifact.frame)(band), (fault) => _codec(String(fault))), { concurrency: 1 }),
    (decoded) => Frame.Artifact.reassembled(decoded),
    Stream.runCollect,
    Effect.filterOrFail((emitted) => Chunk.size(emitted) <= 1, () => _codec("<split-artifact>")),
    Effect.flatMap((emitted) =>
      Option.match(Chunk.head(emitted), {
        onNone: () => Effect.fail(_codec("<incomplete-artifact>")),
        onSome: Either.match({
          onLeft: (fault) => Effect.fail(_folded(fault)),
          onRight: ([artifact, octets]) =>
            artifact.key === key
              ? Effect.succeed({ key, generation: artifact.generation, extent: artifact.extent, octets })
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
  Chart: ({ bytes }) => Effect.mapError(Schema.decode(Frame.Residency.envelope)(bytes), (fault) => _codec(String(fault))),
  Survey: ({ bytes }) => Effect.mapError(Wire.decode("AssetSetManifest", bytes), (fault) => _codec(String(fault))),
  Imprint: ({ codec, pixels, quality }) =>
    Effect.tryPromise({
      // BOUNDARY ADAPTER: a 2d context is nullable by contract, and the encoder is promise-shaped on both arms
      try: async () => {
        const surface = new OffscreenCanvas(pixels.width, pixels.height)
        const ink = surface.getContext("2d")
        if (ink === null) throw new Error("<no-2d-context>")
        ink.putImageData(pixels, 0, 0)
        const held = await surface.convertToBlob({ type: `image/${codec}`, quality: Option.getOrUndefined(quality) })
        return new Uint8Array(await held.arrayBuffer())
      },
      catch: (defect) => _codec(String(defect)),
    }),
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
