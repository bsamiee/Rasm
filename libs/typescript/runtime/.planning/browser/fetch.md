# [RUNTIME_FETCH]

Browser runtime rows, byte-flow policy, worker decoding, and viewpoint-replaced residency compose through `Web`, `Fetch`, `Pool`, and `Depot`.

## [01]-[INDEX]

- [02]-[BINDING_ROWS]: the browser runtime rows — XHR client, socket constructor, spawner; `Web`.
- [03]-[FLOW_ROWS]: the per-class buffer, ceiling, and rate policy rows beside the link-grade budgets; `Fetch` (types).
- [04]-[DIAL_SURFACE]: the shared prelude, the credential stamp, the streaming and decoded modalities; `Fetch`, `FetchFault`.
- [05]-[WIRE_PROTOCOL]: the request family, the raster crossing, the serialized fault, the pool Tag and layer; `Pool`, `PoolFault`.
- [06]-[DEPOT_SCHEDULER]: the residency view cell, the permit governor, the haul pass, the dome lane; `Depot`.
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
import { Digest, Fault, Frame, Shape, Wire } from "@rasm/core"
import { Array, Chunk, Context, type Duration, Effect, Either, HashMap, HashSet, Layer, Match, Option, Order, type ParseResult, Schedule, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import { Client, type Lapse } from "../net/client.ts"
import { Boot, Connect } from "./boot.ts"
import { Kv, Opfs } from "./persist.ts"
import { Vault } from "./route.ts"

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
const _SETTLE = { probe: "50 millis", probes: 6 } as const

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

const _linked = (profile: Option.Option<Connect.Profile>): Fetch.Link =>
  Option.match(profile, {
    onNone: () => _LINKS.swift,
    onSome: (held) => (held.frugal ? _LINKS.strained : _LINKS[held.grade]),
  })

const _fetchFamily = Fault.Class.family(["offline", "overrun"] as const, {
  offline: Fault.Class.row({
    class: "unavailable",
    leg: "dial",
    detail: Schema.Struct({}),
    render: () => "the host reports no network connection",
  }),
  overrun: Fault.Class.row({
    class: "invalid",
    leg: "flow",
    detail: Schema.Struct({ actual: Schema.Number, ceiling: Schema.Number }),
    render: ({ actual, ceiling }) => `feed spent ${actual} bytes against a ceiling of ${ceiling}`,
  }),
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
  type _Links<T extends Record<Connect.Grade, Link> = typeof _LINKS> = T
}

class FetchFault extends Schema.TaggedError<FetchFault>()("FetchFault", {
  case: _fetchFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _fetchFamily.classOf(this.case.reason)
  }
  override get message(): string {
    return _fetchFamily.render(this.case)
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
          ? Effect.fail(new FetchFault({ case: { reason: "overrun", actual: total + band.length, ceiling } }))
          : Effect.succeed([total + band.length, band] as const),
      ),
  })

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
      up ? Effect.void : Effect.fail(new FetchFault({ case: { reason: "offline" } })),
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
    const used = new WeakSet<PerformanceResourceTiming>()
    const _enriched = (span: Option.Option<Span>, url: string, opened: HrTime) =>
      Option.match(span, {
        onNone: () => Effect.void,
        onSome: (live) =>
          Effect.flatMap(Effect.sync(() => hrTime()), (closed) =>
            Effect.repeat(
              Effect.sync(() =>
                Vital.enrich(live, { url, start: opened, end: closed, initiator: Option.some("xmlhttprequest"), used }),
              ),
              { until: Option.isSome, schedule: Schedule.spaced(_SETTLE.probe), times: _SETTLE.probes },
            ),
          ).pipe(Effect.asVoid),
      })
    const _stamped = <A, E, R>(dial: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      Effect.provideService(dial, BrowserHttpClient.XMLHttpRequest, _wire(vault.posture))
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
        const opened = hrTime()
        const span = yield* Effect.optionFromOptional(OtelBridge.currentOtelSpan)
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
            Stream.ensuring(settled),
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
- Law: transferable schemas carry every byte buffer, and the one serialized `PoolFault` family carries its whole refusal as `case` — the reason and exactly that reason's own columns — so a comparing cause cannot cross the port without the pair that decided it.
- Law: pixel crossings are seated HERE and only here — `proc/worker` refuses the row because its node and bun members never construct the DOM type, so this browser-only protocol owns `Transferable.ImageData`, whose transfer list projects `data.buffer` and moves the plane rather than copying it.
- Law: raster encode belongs to the pool, never the document — a readback band's PNG encode blocks the frame that produced it, so `Imprint` takes the plane and answers octets, and `OffscreenCanvas` is the encoder because a worker holds no DOM canvas.
- Boundary: `Frame` owns artifact and residency decoding, `Wire` owns asset-manifest decoding, `Digest` owns minting, and `ui/view/export#SERIALIZER_MATRIX`'s readback arm is the raster consumer that hands `Imprint` its plane.

```typescript
const _Mismatch = Schema.Struct({ detail: Schema.String, actual: Schema.String, expected: Schema.String })

const _poolFamily = Fault.Class.family(["parity", "sequence", "overrun", "codec"] as const, {
  parity: Fault.Class.row({
    class: "breached",
    leg: "identity",
    detail: _Mismatch,
    render: ({ actual, detail, expected }) => `<${detail}> re-derived ${actual} where ${expected} was declared`,
  }),
  sequence: Fault.Class.row({
    class: "conflicted",
    leg: "residency",
    detail: _Mismatch,
    render: ({ actual, detail, expected }) => `<${detail}> read ${actual} where ${expected} was owed`,
  }),
  overrun: Fault.Class.row({
    class: "invalid",
    leg: "budget",
    detail: _Mismatch,
    render: ({ actual, detail, expected }) => `<${detail}> spent ${actual} against a ceiling of ${expected}`,
  }),
  codec: Fault.Class.row({
    class: "malformed",
    leg: "pool",
    detail: Schema.Struct({ detail: Schema.String }),
    render: ({ detail }) => `the port refused the frame — ${detail}`,
  }),
})

declare namespace PoolFault {
  type Case = typeof _poolFamily.payload.Type
  type Reason = (typeof _poolFamily.kinds)[number]
}

class PoolFault extends Schema.TaggedError<PoolFault>()("PoolFault", {
  case: _poolFamily.payload,
}) {
  static readonly of = (fault: Wire.Fault): PoolFault =>
    new PoolFault({
      case: fault.case.reason === "parity" || fault.case.reason === "sequence" || fault.case.reason === "overrun"
        ? {
          reason: fault.case.reason,
          detail: fault.message,
          actual: String(fault.case.actual),
          expected: String(fault.case.expected),
        }
        : { reason: "codec", detail: fault.message },
    })
  get reason(): PoolFault.Reason {
    return this.case.reason
  }
  get class(): Fault.Class.Kind {
    return _poolFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _poolFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _poolFamily.render(this.case)
  }
}

const _Verified = Schema.Struct({
  key: Wire.Artifact.identity,
  extent: Shape.Refined.OrdinalKey,
})

class Redeem extends Schema.TaggedRequest<Redeem>()("Redeem", {
  payload: { key: Wire.Artifact.identity, extent: Shape.Refined.OrdinalKey, payloads: Schema.Array(Transferable.Uint8Array) },
  success: Schema.Struct({ ..._Verified.fields, octets: Transferable.Uint8Array }),
  failure: PoolFault,
}) {}

class VerifyArtifact extends Schema.TaggedRequest<VerifyArtifact>()("VerifyArtifact", {
  payload: { key: Wire.Artifact.identity, extent: Shape.Refined.OrdinalKey, octets: Transferable.Uint8Array },
  success: _Verified,
  failure: PoolFault,
}) {}

class Chart extends Schema.TaggedRequest<Chart>()("Chart", {
  payload: { bytes: Transferable.Uint8Array },
  success: Frame.Residency.Manifest,
  failure: PoolFault,
}) {}

class Survey extends Schema.TaggedRequest<Survey>()("Survey", {
  payload: { bytes: Transferable.Uint8Array },
  success: Schema.typeSchema(Wire.schema("Set")),
  failure: PoolFault,
}) {}

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

const _protocol = Schema.Union(Redeem, VerifyArtifact, Chart, Survey, Imprint)

class Pool extends Context.Tag("runtime/browser/Pool")<
  Pool,
  Worker.SerializedWorkerPool<Redeem | VerifyArtifact | Chart | Survey | Imprint>
>() {
  static readonly Redeem = Redeem
  static readonly VerifyArtifact = VerifyArtifact
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

- Owner: `Depot` holds one admitted residency view, schedules the tiles it has not hauled, and shares one governed byte-haul leg.
- Entry: `Depot.dome` admits environment-set bytes before resolving and hauling the generated product leaves.
- Law: an admitted manifest REPLACES the held view whole and advances the epoch; a viewpoint that does not advance refuses and reaches neither the cell nor the scene port.
- Law: pending-ness is derived here and never decoded — the manifest names the whole resident set for one viewpoint and carries no row state, so what remains to haul is the tile set this epoch has not landed.
- Law: geometry and appearance bytes share one generated `ArtifactFrame` redemption leg: every frame repeats the requested reference, accumulated payload equals `artifactBytes`, and ordered raw payload hashes to its 32-byte SHA-256 identity before one whole buffer lands.
- Law: artifact SHA-256 keys raw byte custody, while `Digest.Key<"content">` remains the separate semantic/cache vocabulary; the depot converts neither into the other.
- Law: one `_admitted` read per pass answers both governors and carries the band's own admission row into every leg — storage pressure prices how fast this origin may fill, link grade prices how many legs are worth opening, and the tighter degree resizes the one gate; a second read inside a leg prices a pass already in flight.
- Law: `warmDepot` refuses ADDING residency, never a pull — a near-full origin still serves the scene and an already-warm band still reads, so the admission gates the cache write alone and eviction pressure prices new rows.
- Boundary: callers resolve each portable `ArtifactRef` through their application asset rail and forward the residency view, geometry arrivals, and dome arrivals to the UI port; no storage key crosses the semantic contract.

```typescript
const _DEGREES = { ample: 6, tight: 2, critical: 1, opaque: 2 } as const

const _byOrder: Order.Order<Frame.ResidencyTile> = Order.combine(
  Order.mapInput(Order.bigint, (tile: Frame.ResidencyTile) => tile.artifact.artifactBytes),
  Order.mapInput(Order.number, (tile: Frame.ResidencyTile) => tile.residentCount),
)

declare namespace Depot {
  type Pull<E, R> = (reference: Wire.Artifact.Reference) => Stream.Stream<Wire.Artifact.Frame, E, R>
  type Fault<E> = E | PoolFault | ParseResult.ParseError | WorkerError.WorkerError
  type Residency = {
    readonly view: Frame.ResidencyView
    readonly keyed: ReadonlyArray<readonly [Frame.ResidencyTile, Wire.Artifact.Reference]>
    readonly hauled: HashSet.HashSet<Wire.Artifact.Identity>
    readonly epoch: number
  }
  type Order = {
    readonly epoch: number
    readonly tile: Frame.ResidencyTile
    readonly artifact: Wire.Artifact.Reference
    readonly key: Wire.Artifact.Identity
  }
  type Settled = {
    readonly key: Wire.Artifact.Identity
    readonly extent: number
  }
  type Landed = readonly [Settled, Uint8Array<ArrayBuffer>]
  type Dome = {
    readonly set: Wire.Decoded<"Set">
    readonly planes: HashMap.HashMap<Wire.Artifact.Identity, Uint8Array<ArrayBuffer>>
  }
  type _Degrees<T extends Record<Opfs.Verdict, number> = typeof _DEGREES> = T
}

const _skewed = (held: Option.Option<Depot.Residency>, manifest: Frame.ResidencyManifest): Option.Option<PoolFault> =>
  Option.flatMap(held, (live) =>
    Option.flatMap(Option.all([Option.fromNullable(live.view.manifest.viewpoint), Option.fromNullable(manifest.viewpoint)]), ([prior, next]) =>
      prior.key === next.key && next.version <= prior.version
        ? Option.some(
            new PoolFault({
              case: {
                reason: "sequence",
                detail: "residency-viewpoint-superseded",
                actual: String(next.version),
                expected: String(prior.version + 1),
              },
            }),
          )
        : Option.none()))

type _EnvironmentSet = Extract<Wire.Decoded<"Set">["product"], { readonly case: "environment" }>["value"]
type _EnvironmentIbl = Extract<_EnvironmentSet["product"], { readonly case: "ibl" }>["value"]
type _EnvironmentPlane = _EnvironmentIbl["source"]["equirect"]

const _environmentPlanes = (set: Wire.Decoded<"Set">): Option.Option<ReadonlyArray<_EnvironmentPlane>> =>
  Match.value(set.product).pipe(Match.discriminatorsExhaustive("case")({
    pbr: () => Option.none(),
    baked: () => Option.none(),
    environment: ({ value }) => Option.some(Match.value(value.product).pipe(Match.discriminatorsExhaustive("case")({
      hdri: ({ value: hdri }) => [
        hdri.source.equirect,
        ...(hdri.source.cubemap === undefined ? [] : [hdri.source.cubemap]),
        ...(hdri.source.preview === undefined ? [] : [hdri.source.preview]),
      ],
      ibl: ({ value: ibl }) => [
        ibl.source.equirect,
        ...(ibl.source.cubemap === undefined ? [] : [ibl.source.cubemap]),
        ...(ibl.source.preview === undefined ? [] : [ibl.source.preview]),
        ...ibl.specular,
        ibl.brdfLut,
        ...(ibl.luminanceCdf === undefined ? [] : [ibl.luminanceCdf]),
      ],
    }))),
  }))

class Depot extends Effect.Service<Depot>()("runtime/browser/Depot", {
  scoped: Effect.gen(function* () {
    const pool = yield* Pool
    const opfs = yield* Opfs
    const kv = yield* Kv
    const connect = yield* Connect
    const _residency = yield* SubscriptionRef.make(Option.none<Depot.Residency>())
    const _gate = yield* Effect.makeSemaphore(_DEGREES.opaque)
    const _admitted: Effect.Effect<Opfs.Admission> = Effect.flatMap(opfs.budget, (budget) =>
      Effect.flatMap(connect.profile.get, (profile) =>
        Effect.as(_gate.resize(Math.min(_DEGREES[budget.verdict], _linked(profile).legs)), opfs.band[budget.verdict])))
    const plan: Effect.Effect<ReadonlyArray<Depot.Order>> = Effect.map(SubscriptionRef.get(_residency), (held) =>
      Option.match(held, {
        onNone: () => Array.empty<Depot.Order>(),
        onSome: (live) =>
          Array.map(
            Array.sort(
              Array.filter(live.keyed, ([, artifact]) => !HashSet.has(live.hauled, artifact.sha256)),
              Order.mapInput(_byOrder, ([tile]: readonly [Frame.ResidencyTile, Wire.Artifact.Reference]) => tile),
            ),
            ([tile, artifact]) => ({ epoch: live.epoch, tile, artifact, key: artifact.sha256 }),
          ),
      }))
    const landed = (order: Depot.Order): Effect.Effect<void> =>
      SubscriptionRef.update(_residency, Option.map((live) =>
        order.epoch !== live.epoch ? live : { ...live, hauled: HashSet.add(live.hauled, order.key) }))
    const _artifactCache = (identity: Wire.Artifact.Identity): string => `artifact/${identity}`
    const _warmed = (reference: Wire.Artifact.Reference) =>
      kv.read("cache", _artifactCache(reference.sha256)).pipe(
        Effect.catchTag("KvFault", () =>
          Effect.as(Effect.logWarning("browser cache read unavailable"), Option.none<Uint8Array>())),
        Effect.flatMap(
          Option.match({
            onNone: () => Effect.succeedNone,
            onSome: (band) =>
              pool.executeEffect(new VerifyArtifact({
                key: reference.sha256,
                extent: Number(reference.artifactBytes),
                octets: band,
              })).pipe(
                Effect.map((verified) => Option.some([verified, band] as const)),
                Effect.catchTags({
                  PoolFault: (fault) =>
                    Effect.as(
                      fault.reason === "parity"
                        ? Effect.ignoreLogged(kv.drop("cache", _artifactCache(reference.sha256)))
                        : Effect.void,
                      Option.none(),
                    ),
                  ParseError: () => Effect.as(Effect.logWarning("browser cache entry undecodable"), Option.none()),
                  WorkerError: () => Effect.as(Effect.logWarning("browser cache verification unavailable"), Option.none()),
                }),
              ),
          }),
        ),
      )
    const _payloads = <E, R>(
      reference: Wire.Artifact.Reference,
      frames: Stream.Stream<Wire.Artifact.Frame, E, R>,
    ): Stream.Stream<Uint8Array, E | PoolFault, R> =>
      Stream.mapAccumEffect(frames, 0, (spent, frame) => {
        const extent = Number(reference.artifactBytes)
        const next = spent + frame.payload.byteLength
        return frame.artifact.sha256 !== reference.sha256 || frame.artifact.artifactBytes !== reference.artifactBytes
          ? Effect.fail(new PoolFault({ case: {
              reason: "parity",
              detail: "artifact-frame-reference-drift",
              actual: `${frame.artifact.sha256}:${frame.artifact.artifactBytes}`,
              expected: `${reference.sha256}:${reference.artifactBytes}`,
            } }))
          : next > extent
            ? Effect.fail(new PoolFault({ case: {
                reason: "overrun", detail: "artifact-payload-extent", actual: String(next), expected: String(extent),
              } }))
            : Effect.succeed([next, frame.payload] as const)
      })
    const _hauledOne = <E, R>(
      reference: Wire.Artifact.Reference,
      admits: Opfs.Admission,
      frames: Stream.Stream<Wire.Artifact.Frame, E, R>,
    ): Effect.Effect<Depot.Landed, Depot.Fault<E>, R> =>
      _gate.withPermits(1)(
        Effect.flatMap(_warmed(reference), (warm) =>
          Option.match(warm, {
            onSome: ([verified, octets]) => Effect.succeed([verified, octets] as const),
            onNone: () =>
              Stream.runCollect(_payloads(reference, frames)).pipe(
                Effect.flatMap((held) => pool.executeEffect(new Redeem({
                  key: reference.sha256,
                  extent: Number(reference.artifactBytes),
                  payloads: Chunk.toReadonlyArray(held),
                }))),
                Effect.tap(({ key: minted, octets }) =>
                  admits.warmDepot
                    ? Effect.ignoreLogged(kv.write("cache", _artifactCache(minted), octets))
                    : Effect.void),
                Effect.map(({ extent, key, octets }) => [{ key, extent }, octets] as const),
              ),
          }),
        ),
      )
    const _surveyed = (
      source: Uint8Array,
    ): Effect.Effect<Wire.Decoded<"Set">, PoolFault | ParseResult.ParseError | WorkerError.WorkerError> =>
      pool.executeEffect(new Survey({ bytes: source }))
    const haul = <E, R>(
      pull: Depot.Pull<Wire.Artifact.Reference, E, R>,
    ): Effect.Effect<readonly [ReadonlyArray<Depot.Fault<E>>, ReadonlyArray<Depot.Landed>], never, R> =>
      Effect.gen(function* () {
        const orders = yield* plan
        const admits = yield* _admitted
        return yield* Effect.partition(orders, (order) =>
          Effect.tap(
            _hauledOne(order.key, admits, pull(order.artifact)),
            () => landed(order),
          ), {
          concurrency: "unbounded",
        })
      })
    const dome = <E, R>(
      source: Uint8Array,
      pull: Depot.Pull<Wire.Artifact.Reference, E, R>,
    ): Effect.Effect<Option.Option<Depot.Dome>, Depot.Fault<E>, R> =>
      Effect.flatMap(_admitted, (admits) =>
        Effect.flatMap(_surveyed(source), (manifest) =>
          Option.match(_environmentPlanes(manifest), {
            onNone: () => Effect.succeedNone,
            onSome: (products) =>
              Effect.gen(function* () {
                const leaves = yield* Effect.forEach(products, (product) =>
                  Effect.map(Schema.decode(Wire.Texture.reference)(product.plane), (reference) => [
                    reference.artifact.artifactId,
                    reference,
                  ] as const))
                const distinct = yield* Effect.reduce(
                  leaves,
                  HashMap.empty<Digest.Key<"content">, Wire.Texture.Reference>(),
                  (held, [key, reference]) => Option.match(HashMap.get(held, key), {
                    onNone: () => Effect.succeed(HashMap.set(held, key, reference)),
                    onSome: (prior) => prior.artifact.artifactBytes === reference.artifact.artifactBytes
                      ? Effect.succeed(held)
                      : Effect.fail(new PoolFault({ case: {
                          reason: "parity", detail: `environment-plane-declared-extent:${key}`,
                          actual: String(reference.artifact.artifactBytes),
                          expected: String(prior.artifact.artifactBytes),
                        } })),
                  }),
                )
                const planes = yield* Effect.forEach(HashMap.toEntries(distinct), ([key, reference]) =>
                  Effect.flatMap(_hauledOne(key, admits, pull(reference.artifact)), ([verified, octets]) =>
                    BigInt(verified.extent) === reference.artifact.artifactBytes
                      ? Effect.succeed([key, octets] as const)
                      : Effect.fail(new PoolFault({ case: {
                          reason: "parity", detail: `environment-plane-landed-extent:${key}`,
                          actual: String(verified.extent), expected: String(reference.artifact.artifactBytes),
                        } }))), {
                    concurrency: "unbounded",
                  })
                return Option.some({ set: manifest, planes: HashMap.fromIterable(planes) })
              }),
          }),
        ),
      )
    const residency: Subscribable.Subscribable<Option.Option<Depot.Residency>> = _residency
    return {
      residency,
      plan,
      landed,
      haul,
      dome,
      charted: (manifest: Frame.ResidencyManifest): Effect.Effect<Either.Either<Frame.ResidencyView, PoolFault>> =>
        SubscriptionRef.modify(_residency, (held) =>
          Option.match(_skewed(held, manifest), {
            onSome: (refusal) => [Either.left(refusal), held] as const,
            onNone: () =>
              Either.match(
                Either.flatMap(Frame.Residency.admit(manifest), (view) =>
                  Either.map(
                    Either.mapLeft(
                      Either.all(Array.map(view.manifest.tiles, (tile) =>
                        Either.map(Schema.decodeEither(Wire.Artifact.reference)(tile.artifact), (artifact) => [tile, artifact] as const))),
                      (error) => _codec(error.message),
                    ),
                    (keyed) => [view, keyed] as const,
                  )),
                {
                  onLeft: (fault) => [Either.left(fault instanceof PoolFault ? fault : PoolFault.of(fault)), held] as const,
                  onRight: ([view, keyed]) => [
                    Either.right(view),
                    Option.some<Depot.Residency>({
                      view,
                      keyed,
                      hauled: HashSet.empty<Digest.Key<"content">>(),
                      epoch: Option.match(held, { onNone: () => 1, onSome: (live) => live.epoch + 1 }),
                    }),
                  ] as const,
                },
              ),
          })),
    }
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Depot, Fetch, FetchFault, Pool, PoolFault, Web }
```

## [07]-[RUNNER_ENTRY]

- Owner: the terminal worker maps every request class to the canonical `Frame`, `Wire`, and `Digest` owners.
- Law: artifact assembly preserves the band generation, and every boundary failure crosses through `PoolFault.of` — the one wire-to-protocol fold both ends of the port share.
- Law: raster encode runs on `OffscreenCanvas` — a worker thread reaches no DOM canvas, `convertToBlob` is its one encoder, and the codec row supplies the mime while quality reaches the lossy rows alone; a refused context or encoder folds to `codec` like every other boundary failure.
- Boundary: the app owns worker script wiring and process boot.

```typescript
import { WorkerRunner } from "@effect/platform"
import { BrowserRuntime, BrowserWorkerRunner } from "@effect/platform-browser"
import { Digest, Frame, Wire } from "@rasm/core"
import { Chunk, Effect, Either, Layer, Option, Schema, Stream } from "effect"
import { Pool, PoolFault } from "./fetch.ts"

const _codec = (detail: string): PoolFault => new PoolFault({ case: { reason: "codec", detail } })

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
          onLeft: (fault) => Effect.fail(PoolFault.of(fault)),
          onRight: ([artifact, octets]) =>
            artifact.key === key
              ? Effect.succeed({ key, generation: artifact.generation, extent: artifact.extent, octets })
              : Effect.fail(
                new PoolFault({
                  case: { reason: "parity", detail: "declared-key-mismatch", actual: artifact.key, expected: key },
                }),
              ),
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
        : Effect.fail(
          new PoolFault({ case: { reason: "parity", detail: "reverify-mismatch", actual: minted, expected: key } }),
        ),
    ),
  Chart: ({ bytes }) => Effect.mapError(Schema.decode(Frame.Residency.envelope)(bytes), (fault) => _codec(String(fault))),
  Survey: ({ bytes }) => Effect.mapError(Wire.decode("Set", bytes), (fault) => _codec(String(fault))),
  Imprint: ({ codec, pixels, quality }) =>
    Effect.tryPromise({
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

// --- [EXPORTS] -------------------------------------------------------------------------

export {}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
