# [CORE_FRAME]

The reassembly rail of the interchange plane: multi-part payloads from the compute runtime arrive as content-keyed, ordinal-positioned frames; reassembly is one keyed Mealy fold over the frame stream under the `value` ingress budget — the frame-count ceiling and the per-artifact byte ceiling are both arms of the fold, so every frame past either bound is a typed `overrun` refusal and no arrival leaves the rail without a receipt or a verdict — the held bands stream-verify through the one `Parity` combinator before the join allocates, the proven octets join in a single allocation, and the verified receipt travels beside its bytes. Three planes ride the rail: `ArtifactFrame`, the format-agnostic reassembly-and-verify fold; `GeometryFrame`, the GLB control plane — payload envelope, encoding vocabulary, tensor rows, the span-proven typed-array windows over verified octets, and the `joined` content-key rendezvous marrying envelopes to their verified artifacts; `Residency`, the manifest-replace/delta-evolve protocol whose polymorphic ledger fold IS the fetch plan the runtime transport schedules against, with the ledger's pending and census reads riding the same owner. Every coordinate is a verbatim `ContentKey`, so the ledger, the artifact receipts, and the viewer scene keys speak one identity. The module is `core/src/interchange/frame.ts`; a frame envelope axis is one field mirroring the C# emit, a new encoding or residency state is one literal row, and a new tensor dtype is one vocabulary row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                 | [PUBLIC]        |
| :-----: | :----------------- | :--------------------------------------------------------------------- | :-------------- |
|  [01]   | `FRAME_PROTOCOL`   | the frame shape, the budgeted keyed reassembly fold, sequence evidence | `ArtifactFrame` |
|  [02]   | `KEY_VERIFY`       | the single-allocation join, the delegated verify, the artifact receipt | `ArtifactFrame` |
|  [03]   | `GEOMETRY_PLANE`   | the GLB envelope, encoding and tensor vocabularies, span-proven views  | `GeometryFrame` |
|  [04]   | `RESIDENCY_LEDGER` | the state vocabulary, manifest/delta envelope, the one ledger fold     | `Residency`     |

## [02]-[FRAME_PROTOCOL]

- Owner: `Frame`, the decoded frame class — declared artifact key, dense ordinal, total count, held band — and `_gathered`, the keyed Mealy fold threading the feed-level frame count and per-artifact assembly state through the stream: completion is `ordinal + 1 === total`, interleaving across artifacts is legal because state keys by artifact, and every abnormality emits typed evidence through the plane's shared vocabulary.
- Law: ordinals are dense per artifact — a non-consecutive ordinal aborts that artifact with the codec `Gap` sequence evidence carrying both coordinates, later frames of the aborted artifact fall through as fresh evidence rather than resurrecting state, and a headless arrival (first frame with a nonzero ordinal) is the same evidence at expected zero; a mid-chain `total` flip aborts as `<total-drift>`, and source exhaustion emits `<truncated>` evidence for every held artifact, so an incomplete tail never disappears with the fold state.
- Law: the budget is the `value` `Ingress` row consumed as values, and both ceilings are fold arms — the running feed-level frame count refuses every frame past `budget.maxFrames` as `overrun` evidence carrying both coordinates while holding zero state for it, and the running per-artifact byte ceiling refuses a band that lifts the accumulated extent past `budget.maxAssembledBytes` before any state grows for it — so the ceilings have one declaration, the surplus arm costs evidence only, and a tail-discarding `Stream.take` past the ceiling is the deleted spelling: no decoded frame is ever dropped without either a receipt or a typed refusal, and whether a consumer halts on the first budget refusal or drains the evidence is its divert policy, never this fold's.
- Law: bands are held verbatim — the fold accumulates received `Uint8Array` views untouched; the join is the only allocation and nothing recodes a band.
- Growth: a frame envelope axis (a compression flag, a priority lane) is one field mirroring the C# emit; the fold's shape never changes.
- Boundary: the frame decode rides the codec registry's `ArtifactFrameWire` schema composition; the quarantine divert composes on the consuming stream; fetch scheduling against the ledger is `[04]`'s consumer.
- Packages: `effect` (`Schema`, `Stream`, `HashMap`, `Chunk`, `Array`, `Effect`, `Either`, `Option`, `Record`); `./codec.ts` (`Gap`, `Parity`, `Quarantine`, `Wire`, `WireFault`); `./format.ts` (`Proto`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/schema.ts` (`Ingress`).

```typescript signature
import { Array, Chunk, Effect, Either, HashMap, Option, Record, Ref, Schema, Stream } from "effect"
import { ContentKey, Digest } from "../value/contentKey.ts"
import { Ingress } from "../value/schema.ts"
import { Gap, Parity, type Quarantine, Wire, WireFault } from "./codec.ts"
import { Proto } from "./format.ts"

class Frame extends Schema.Class<Frame>("Frame")({
  artifact: Digest.FromBytes,
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  total: Schema.Int.pipe(Schema.positive()),
  band: Schema.Uint8ArrayFromSelf,
}) {}

type _Held = { readonly expect: number; readonly extent: number; readonly total: number; readonly bands: Chunk.Chunk<Uint8Array> }
type _State = { readonly seen: number; readonly held: HashMap.HashMap<ContentKey, _Held> }
type _Emit = Either.Either<Option.Option<{ readonly key: ContentKey; readonly bands: Chunk.Chunk<Uint8Array> }>, WireFault>

const _SEED: _State = { seen: 0, held: HashMap.empty() }

const _overrun = (detail: string, actual: number, expected: number): WireFault =>
  new WireFault({ family: "ArtifactFrameWire", reason: "overrun", detail, evidence: Option.some({ actual, expected }) })

const _next = (state: _State, held: HashMap.HashMap<ContentKey, _Held>): _State => ({ seen: state.seen + 1, held })

const _gathered = (budget: Ingress.Shape) => (state: _State, frame: Frame): readonly [_State, _Emit] =>
  state.seen >= budget.maxFrames
    ? ([_next(state, state.held), Either.left(_overrun("<frame-budget>", state.seen + 1, budget.maxFrames))] as const) // the surplus arm: typed refusal per frame past the ceiling, zero state held for it
    : Option.match(HashMap.get(state.held, frame.artifact), {
        onNone: () =>
          frame.ordinal !== 0
            ? ([_next(state, state.held), Either.left(Gap.evidence("ArtifactFrameWire", 0n, BigInt(frame.ordinal)))] as const)
            : frame.band.length > budget.maxAssembledBytes
              ? ([_next(state, state.held), Either.left(_overrun("<assembled-over-budget>", frame.band.length, budget.maxAssembledBytes))] as const)
              : frame.total === 1
                ? ([_next(state, state.held), Either.right(Option.some({ key: frame.artifact, bands: Chunk.of(frame.band) }))] as const)
                : ([
                    _next(state, HashMap.set(state.held, frame.artifact, {
                      expect: 1,
                      extent: frame.band.length,
                      total: frame.total,
                      bands: Chunk.of(frame.band),
                    })),
                    Either.right(Option.none()),
                  ] as const),
        onSome: (held) =>
          frame.ordinal !== held.expect
            ? ([
                _next(state, HashMap.remove(state.held, frame.artifact)),
                Either.left(Gap.evidence("ArtifactFrameWire", BigInt(held.expect), BigInt(frame.ordinal))),
              ] as const)
            : frame.total !== held.total
              ? ([
                  _next(state, HashMap.remove(state.held, frame.artifact)),
                  Either.left(Gap.evidence("ArtifactFrameWire", BigInt(held.total), BigInt(frame.total), "<total-drift>")),
                ] as const)
              : held.extent + frame.band.length > budget.maxAssembledBytes
                ? ([
                    _next(state, HashMap.remove(state.held, frame.artifact)),
                    Either.left(_overrun("<assembled-over-budget>", held.extent + frame.band.length, budget.maxAssembledBytes)),
                  ] as const)
                : frame.ordinal + 1 === held.total
                  ? ([
                      _next(state, HashMap.remove(state.held, frame.artifact)),
                      Either.right(Option.some({ key: frame.artifact, bands: Chunk.append(held.bands, frame.band) })),
                    ] as const)
                  : ([
                      _next(state, HashMap.set(state.held, frame.artifact, {
                        ...held,
                        expect: held.expect + 1,
                        extent: held.extent + frame.band.length,
                        bands: Chunk.append(held.bands, frame.band),
                      })),
                      Either.right(Option.none()),
                    ] as const),
      })
```

## [03]-[KEY_VERIFY]

- Owner: `ArtifactFrame`, the assembled owner — the marked-kernel byte join, the delegated verify, and `reassembled`, the one stream surface turning a budgeted frame feed into verified artifacts; `Artifact` is the receipt class carrying key, extent, and frame count beside the assembled octets.
- Law: the verify is the plane's shared combinator — the held bands prove against the declared key through `Parity.verified` on its band-iterable payload modality, so this rail is the interchange's one content-mint delegation site and a per-consumer re-verify is the second-mint defect; a parity miss carries both keys as evidence and refuses BEFORE the summed buffer allocates, so a corrupted multi-frame artifact never costs its own join.
- Law: verify is per-artifact, not per-frame — frames carry no per-frame hash; the artifact's declared key proves the whole over one streaming digest walk, so a corrupted band surfaces exactly once and the joined buffer is never re-hashed.
- Exemption: `_joined` is a marked kernel — one allocation at the summed extent, bands copied in ordinal order, the draft detaching immutably at the return; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a second verified consumer composes `reassembled` — fold, join, and verify are one spelling.
- Boundary: the receipt and octets travel to the runtime wave's fetch worker and the data wave's object store, both of which compare keys and never re-mint; parity-refused artifacts quarantine at the consumer holding the octets.

```typescript signature
class Artifact extends Schema.Class<Artifact>("Artifact")({
  key: Digest.FromBytes,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  frames: Schema.Int.pipe(Schema.positive()),
}) {}

const _joined = (bands: Chunk.Chunk<Uint8Array>): Uint8Array => {
  // BOUNDARY ADAPTER: single-allocation byte join — the draft detaches immutably at the return
  const extent = Chunk.reduce(bands, 0, (total, band) => total + band.length)
  const out = new Uint8Array(extent)
  let at = 0
  for (const band of bands) {
    out.set(band, at)
    at += band.length
  }
  return out
}

const _verifiedArtifact = (
  key: ContentKey,
  bands: Chunk.Chunk<Uint8Array>,
): Effect.Effect<readonly [Artifact, Uint8Array], WireFault> =>
  Parity.verified("ArtifactFrameWire", key, bands).pipe(
    Effect.map(() => {
      const octets = _joined(bands)
      return [new Artifact({ key, extent: octets.length, frames: Chunk.size(bands) }), octets] as const
    }),
  )

const _unfinished = (state: _State): ReadonlyArray<_Emit> =>
  Array.map(Array.fromIterable(HashMap.values(state.held)), (held) =>
    Either.left(Gap.evidence("ArtifactFrameWire", BigInt(held.total), BigInt(held.expect), "<truncated>")))

const ArtifactFrame: {
  readonly Frame: typeof Frame
  readonly Artifact: typeof Artifact
  readonly frame: Schema.Schema<Frame, Uint8Array>
  readonly reassembled: <E, R>(
    frames: Stream.Stream<Frame, E, R>,
    budget?: Ingress.Shape,
  ) => Stream.Stream<Either.Either<readonly [Artifact, Uint8Array], WireFault>, E, R>
} = {
  Frame,
  Artifact,
  frame: Proto.family(Proto.suite.ArtifactFrameWire, Frame),
  reassembled: (frames, budget = Ingress.floor) =>
    Stream.unwrap(
      Ref.make(_SEED).pipe(
        Effect.map((state) =>
          frames.pipe(
            Stream.mapEffect((frame) =>
              Ref.modify(state, (current) => {
                const [next, emit] = _gathered(budget)(current, frame)
                return [emit, next] as const
              })),
            Stream.concat(Stream.fromEffect(Ref.get(state)).pipe(Stream.flatMap((settled) => Stream.fromIterable(_unfinished(settled))))),
            Stream.filterMap((emit) =>
              Either.match(emit, {
                onLeft: (fault) => Option.some(Effect.succeed(Either.left(fault))),
                onRight: (held) => Option.map(held, (ready) => Effect.either(_verifiedArtifact(ready.key, ready.bands))),
              })),
            Stream.mapEffect((settle) => settle, { concurrency: 1 }),
          )),
      ),
    ),
}
```

## [04]-[GEOMETRY_PLANE]

- Owner: `GeometryFrame`, the GLB control plane — mesh content key, LOD ordinal, the closed `encoding` vocabulary (`glb`, `draco`, `meshopt`), the tensor row set (semantic, dtype, shape, offset, stride), the artifact coordinate binding the envelope to its verified octets, and `joined`, the content-key rendezvous fold; `_views` is the dtype-to-constructor vocabulary the `view` and `extent` statics read.
- Law: the GLB band is consume-only carriage — the interchange opens no glTF parser; the band travels verbatim from artifact verify to the viewer's loader, and this plane's knowledge ends at the encoding row.
- Law: envelopes decode independently of octets — envelopes are small control-plane frames, octets are bulk artifact frames; the two planes join by content key, so envelope loss never strands verified bytes and byte loss never blocks planning.
- Law: the join is this rail's owned geometry — `joined` merges the settled envelope stream with the verified artifact stream and holds whichever side arrives first in a key-addressed rendezvous, emitting the `[envelope, receipt, octets]` triple on the match and replacing a stale hold on re-arrival; the same `Ingress` carrier explicitly ceilings unmatched keys and emits an `overrun` left value instead of growing the rendezvous, while decoder admission, GPU upload, and scheduling remain runtime and ui policy over the right value.
- Law: the envelope streams instantiate the codec's `Wire.diverted` framed-divert combinator over their own suite rows — the frame page spells no pipeline of its own, and the `_framed` partial application is the only local surface; its quarantine thunks hold whole-message octets, the registry's replay coordinate, and `Proto.delimit` re-frames only where an egress joins a size-delimited transport.
- Law: the view proves before it constructs — `extent` prices a tensor's byte span off its own dtype row and declared stride (`byteStride` zero is packed; a positive stride prices `stride * (rows - 1)` plus one packed row), `view` rejects a positive stride smaller than the packed row and any span past the verified buffer as typed `overrun` evidence, a contiguous aligned tensor aliases zero-copy, and a strided or misaligned tensor projects through the gather kernel into a fresh contiguous view.
- Law: views alias, transfers detach — the zero-copy window dies on this side the moment the buffer transfers to a decode worker, so views construct where they are consumed and never store in cells; the gathered strided view owns its fresh buffer and carries no alias.
- Exemption: the typed-array window construction and the strided gather copy are the platform-forced seam — `_packed` carries the `// BOUNDARY ADAPTER` mark, only the detached view leaves, and the transfer list is the consumer's marshal declaration.
- Growth: a new encoding or tensor semantic is one literal row; a new dtype is one `_views` row — constructor and width land as data, and consumers never switch on dtype.
- Boundary: draco/meshopt decoder admission, GPU upload, and worker marshal are ui- and runtime-wave concerns over these values; the zero-copy worker crossing is the consumer's `Transferable.schema` declaration at its own marshal boundary, deliberately not a core surface.

```typescript signature
const _encodings = ["glb", "draco", "meshopt"] as const
const _dtypes = ["f32", "u32", "u16", "u8"] as const
const _semantics = ["position", "normal", "uv", "index", "color"] as const

const _views = {
  f32: { of: Float32Array, width: 4 },
  u32: { of: Uint32Array, width: 4 },
  u16: { of: Uint16Array, width: 2 },
  u8: { of: Uint8Array, width: 1 },
} as const

const _count = (shape: ReadonlyArray<number>): number => Array.reduce(shape, 1, (total, dim) => total * dim)

const _rowBytes = (tensor: GeometryFrame.Tensor): number => _views[tensor.dtype].width * Array.lastNonEmpty(tensor.shape)

const _rows = (tensor: GeometryFrame.Tensor): number => _count(tensor.shape) / Array.lastNonEmpty(tensor.shape)

const _span = (tensor: GeometryFrame.Tensor): number =>
  tensor.byteStride === 0
    ? _count(tensor.shape) * _views[tensor.dtype].width
    : tensor.byteStride * (_rows(tensor) - 1) + _rowBytes(tensor)

const _packed = (octets: Uint8Array, tensor: GeometryFrame.Tensor): GeometryFrame.View => {
  // BOUNDARY ADAPTER: strided gather kernel — row bytes copy into a fresh contiguous buffer; only the detached view leaves
  const rowBytes = _rowBytes(tensor)
  const stride = tensor.byteStride === 0 ? rowBytes : tensor.byteStride
  const rows = _rows(tensor)
  const gathered = new Uint8Array(rows * rowBytes)
  for (let row = 0; row < rows; row += 1) {
    gathered.set(octets.subarray(tensor.byteOffset + row * stride, tensor.byteOffset + row * stride + rowBytes), row * rowBytes)
  }
  return new _views[tensor.dtype].of(gathered.buffer, 0, _count(tensor.shape))
}

const _framed = <A, I>(gen: (typeof Proto.suite)[keyof typeof Proto.suite], family: Wire.Family, owned: Schema.Schema<A, I>) =>
(frames: AsyncIterable<Uint8Array>): Stream.Stream<Either.Either<A, WireFault>, WireFault, Quarantine> =>
  Wire.diverted(family, Proto.stream(gen)(frames), Schema.decodeUnknown(owned), Schema.encodeSync(Proto.frame(gen)))

const _Tensor = Schema.Struct({
  semantic: Schema.Literal(..._semantics),
  dtype: Schema.Literal(..._dtypes),
  shape: Schema.NonEmptyArray(Schema.Int.pipe(Schema.positive())),
  byteOffset: Schema.Int.pipe(Schema.nonNegative()),
  byteStride: Schema.Int.pipe(Schema.nonNegative()),
})

class GeometryFrame extends Schema.Class<GeometryFrame>("GeometryFrame")({
  mesh: Digest.FromBytes,
  artifact: Digest.FromBytes,
  lod: Schema.Int.pipe(Schema.nonNegative()),
  encoding: Schema.Literal(..._encodings),
  tensors: Schema.Array(_Tensor),
}) {
  static readonly payload: Schema.Schema<GeometryFrame, Uint8Array> = Proto.family(Proto.suite.GeometryPayloadWire, GeometryFrame)
  static readonly stream = (
    frames: AsyncIterable<Uint8Array>,
  ): Stream.Stream<Either.Either<GeometryFrame, WireFault>, WireFault, Quarantine> =>
    _framed(Proto.suite.GeometryPayloadWire, "GeometryPayloadWire", GeometryFrame)(frames)
  static readonly extent = (tensor: GeometryFrame.Tensor): number => _span(tensor)
  static readonly view = (octets: Uint8Array, tensor: GeometryFrame.Tensor): Either.Either<GeometryFrame.View, WireFault> =>
    tensor.byteStride !== 0 && tensor.byteStride < _rowBytes(tensor)
      ? Either.left(
          new WireFault({
            family: "GeometryPayloadWire",
            reason: "overrun",
            detail: "<tensor-stride>",
            evidence: Option.some({ actual: tensor.byteStride, expected: _rowBytes(tensor) }),
          }),
        )
      : tensor.byteOffset + _span(tensor) > octets.byteLength
      ? Either.left(
          new WireFault({
            family: "GeometryPayloadWire",
            reason: "overrun",
            detail: "<tensor-span>",
            evidence: Option.some({ actual: tensor.byteOffset + _span(tensor), expected: octets.byteLength }),
          }),
        )
      : Either.right(
          tensor.byteStride === 0 && (octets.byteOffset + tensor.byteOffset) % _views[tensor.dtype].width === 0
            ? new _views[tensor.dtype].of(octets.buffer, octets.byteOffset + tensor.byteOffset, _count(tensor.shape)) // packed and aligned: the zero-copy alias
            : _packed(octets, tensor), // strided or misaligned: the gather kernel detaches a fresh contiguous view
        )
  static readonly joined = <E1, R1, E2, R2>(
    envelopes: Stream.Stream<GeometryFrame, E1, R1>,
    artifacts: Stream.Stream<readonly [Artifact, Uint8Array], E2, R2>,
    budget: Ingress.Shape = Ingress.floor,
  ): Stream.Stream<Either.Either<readonly [GeometryFrame, Artifact, Uint8Array], WireFault>, E1 | E2, R1 | R2> =>
    Stream.merge(
      Stream.map(envelopes, Either.left),
      Stream.map(artifacts, Either.right),
      { haltStrategy: "both" },
    ).pipe(
      Stream.mapAccum(
        HashMap.empty<ContentKey, Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>>(),
        (held, arrival): readonly [
          HashMap.HashMap<ContentKey, Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>>,
          Option.Option<Either.Either<readonly [GeometryFrame, Artifact, Uint8Array], WireFault>>,
        ] =>
          Either.match(arrival, {
            onLeft: (envelope) =>
              Option.match(Option.flatMap(HashMap.get(held, envelope.artifact), Either.getRight), {
                onNone: () =>
                  HashMap.size(held) >= budget.maxFrames
                    ? [held, Option.some(Either.left(_overrun("<geometry-rendezvous>", HashMap.size(held) + 1, budget.maxFrames)))] as const
                    : [HashMap.set(held, envelope.artifact, Either.left(envelope)), Option.none()] as const,
                onSome: ([receipt, octets]) =>
                  [HashMap.remove(held, envelope.artifact), Option.some(Either.right([envelope, receipt, octets] as const))] as const,
              }),
            onRight: ([receipt, octets]) =>
              Option.match(Option.flatMap(HashMap.get(held, receipt.key), Either.getLeft), {
                onNone: () =>
                  HashMap.size(held) >= budget.maxFrames
                    ? [held, Option.some(Either.left(_overrun("<geometry-rendezvous>", HashMap.size(held) + 1, budget.maxFrames)))] as const
                    : [HashMap.set(held, receipt.key, Either.right([receipt, octets] as const)), Option.none()] as const,
                onSome: (envelope) =>
                  [HashMap.remove(held, receipt.key), Option.some(Either.right([envelope, receipt, octets] as const))] as const,
              }),
          }),
      ),
      Stream.filterMap((emit) => emit),
    )
}

declare namespace GeometryFrame {
  type Encoding = (typeof _encodings)[number]
  type Dtype = (typeof _dtypes)[number]
  type Tensor = Schema.Schema.Type<typeof _Tensor>
  type View = InstanceType<(typeof _views)[Dtype]["of"]>
  type _Views<T extends Record<Dtype, { readonly of: unknown; readonly width: number }> = typeof _views> = T
}
```

## [05]-[RESIDENCY_LEDGER]

- Owner: `Residency`, the residency protocol — the closed `state` vocabulary (`resident`, `pending`, `evicted`), the `Manifest` class, the `_Row.fields`-derived `Delta`, their structurally discriminated envelope union, and `folded`, whose tuple-shaped overload input distinguishes the `(ledger, arrival)` value modality from the one-element stream modality without an optional-argument ghost; both modalities execute `_landed`.
- Law: the ledger IS the fetch plan — `pending` rows are the fetch queue, `resident` rows are addressable, `evicted` rows are reclaimable; the runtime transport schedules against it and the artifact receipts confirm arrivals by the same key, so residency questions are `HashMap` lookups, never scans, and the two standing reads ride the owner: `pending(ledger)` is the unordered fetch queue a scheduler orders by its own policy, `census(ledger)` the per-state count and byte tallies a board or budget read consumes.
- Law: deltas are idempotent transitions — a delta matching the ledger's current state is a no-op, so the wire replays deltas across reconnects and the fold absorbs them; a delta referencing an unknown key folds to an insert because the manifest introducing it may still be in flight.
- Law: the manifest is authoritative at arrival — the C# render side owns truth, so the manifest arm discards the prior ledger whole and deltas only evolve the last manifest.
- Law: one wire family, one envelope — manifest and delta are the two arms of one message; a second decode surface per arm forks the family the census fences.
- Growth: a new residency state is one literal row every exhaustive consumer breaks on until handled; a new row axis (priority score, byte budget) is one field on the row struct.
- Boundary: fetch scheduling, worker transfer, and byte budgets are the runtime wave's policy — the runtime `Depot` composes `folded` and orders `pending` by its own worst-first policy; the artifact verify that flips `pending` to `resident` is `[03]`'s receipt joined at the consumer.

```typescript signature
const _states = ["resident", "pending", "evicted"] as const

const _Row = Schema.Struct({
  mesh: Digest.FromBytes,
  lod: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.nonNegative()),
  state: Schema.Literal(..._states),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  scene: Digest.FromBytes,
  rows: Schema.Array(_Row),
  minted: Schema.DateTimeUtc,
}) {}

class Delta extends Schema.Class<Delta>("Delta")(_Row.fields) {}

const _envelope: Schema.Union<[typeof Manifest, typeof Delta]> = Schema.Union(Manifest, Delta)

declare namespace Residency {
  type State = (typeof _states)[number]
  type Row = Schema.Schema.Type<typeof _Row>
  type Arrival = Manifest | Delta
  type Ledger = HashMap.HashMap<ContentKey, Row>
  type Tally = { readonly count: number; readonly extent: number }
  type Census = { readonly [S in State]: Tally }
}

const _landed = (ledger: Residency.Ledger, arrival: Residency.Arrival): Residency.Ledger =>
  arrival instanceof Manifest
    ? Array.reduce(arrival.rows, HashMap.empty<ContentKey, Residency.Row>(), (acc, row) => HashMap.set(acc, row.mesh, row))
    : HashMap.set(ledger, arrival.mesh, arrival)

function _folded(ledger: Residency.Ledger, arrival: Residency.Arrival): Residency.Ledger
function _folded<E, R>(arrivals: Stream.Stream<Residency.Arrival, E, R>): Stream.Stream<Residency.Ledger, E, R>
function _folded<E, R>(
  ...input: readonly [Residency.Ledger, Residency.Arrival] | readonly [Stream.Stream<Residency.Arrival, E, R>]
): Residency.Ledger | Stream.Stream<Residency.Ledger, E, R> {
  return input.length === 2
    ? _landed(input[0], input[1])
    : Stream.scan(input[0], HashMap.empty<ContentKey, Residency.Row>(), _landed)
}

const _EMPTY_CENSUS: Residency.Census = {
  resident: { count: 0, extent: 0 },
  pending: { count: 0, extent: 0 },
  evicted: { count: 0, extent: 0 },
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly Delta: typeof Delta
  readonly envelope: Schema.Schema<Residency.Arrival, Uint8Array>
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<Either.Either<Residency.Arrival, WireFault>, WireFault, Quarantine>
  readonly folded: typeof _folded
  readonly pending: (ledger: Residency.Ledger) => ReadonlyArray<Residency.Row>
  readonly census: (ledger: Residency.Ledger) => Residency.Census
} = {
  Manifest,
  Delta,
  envelope: Proto.family(Proto.suite.GeometryResidencyWire, _envelope),
  stream: _framed(Proto.suite.GeometryResidencyWire, "GeometryResidencyWire", _envelope),
  folded: _folded,
  pending: (ledger) => Array.filter(Array.fromIterable(HashMap.values(ledger)), (row) => row.state === "pending"),
  census: (ledger) =>
    HashMap.reduce(ledger, _EMPTY_CENSUS, (acc, row) =>
      Record.modify(acc, row.state, (tally) => ({ count: tally.count + 1, extent: tally.extent + row.extent }))),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ArtifactFrame, GeometryFrame, Residency }
```
