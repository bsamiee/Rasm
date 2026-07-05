# [CORE_FRAME]

The reassembly rail of the interchange plane: multi-part payloads from the compute runtime arrive as content-keyed, ordinal-positioned frames; reassembly is one keyed Mealy fold over the frame stream under the `value` ingress budget — frame count by `Stream.take`, assembled bytes by a running per-artifact ceiling — the held bands stream-verify through the one `Parity` combinator before the join allocates, the proven octets join in a single allocation, and the verified receipt travels beside its bytes. Three planes ride the rail: `ArtifactFrame`, the format-agnostic reassembly-and-verify fold; `GeometryFrame`, the GLB control plane — payload envelope, encoding vocabulary, tensor rows, the zero-copy typed-array windows over verified octets, and the `joined` content-key rendezvous marrying envelopes to their verified artifacts; `Residency`, the manifest-replace/delta-evolve protocol whose polymorphic ledger fold IS the fetch plan the runtime transport schedules against. Every coordinate is a verbatim `ContentKey`, so the ledger, the artifact receipts, and the viewer scene keys speak one identity. The module is `core/src/interchange/frame.ts`; a frame envelope axis is one field mirroring the C# emit, a new encoding or residency state is one literal row, and a new tensor dtype is one vocabulary row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     | [PUBLIC]        |
| :-----: | :---------------- | :------------------------------------------------------------------------- | :-------------- |
|  [01]   | `FRAME_PROTOCOL`  | the frame shape, the budgeted keyed reassembly fold, sequence evidence      | `ArtifactFrame` |
|  [02]   | `KEY_VERIFY`      | the single-allocation join, the delegated verify, the artifact receipt     | `ArtifactFrame` |
|  [03]   | `GEOMETRY_PLANE`  | the GLB envelope, encoding and tensor vocabularies, zero-copy views        | `GeometryFrame` |
|  [04]   | `RESIDENCY_LEDGER`| the state vocabulary, manifest/delta envelope, the one ledger fold         | `Residency`     |

## [2]-[FRAME_PROTOCOL]

[FRAME_PROTOCOL]:
- Owner: `Frame`, the decoded frame class — declared artifact key, dense ordinal, total count, held band — and `_gathered`, the keyed Mealy fold threading per-artifact assembly state through the stream: completion is `ordinal + 1 === total`, interleaving across artifacts is legal because state keys by artifact, and every abnormality emits typed evidence through the plane's shared vocabulary.
- Law: ordinals are dense per artifact — a non-consecutive ordinal aborts that artifact with the codec `Gap` sequence evidence carrying both coordinates, later frames of the aborted artifact fall through as fresh evidence rather than resurrecting state, and a headless arrival (first frame with a nonzero ordinal) is the same evidence at expected zero; a mid-chain `total` flip aborts the same way as `<total-drift>` sequence evidence, because the declared length is part of the chain contract and a silently shortened emission would otherwise strand held state without a verdict.
- Law: the budget is the `value` `Ingress` row consumed as values — `Stream.take(budget.maxFrames)` bounds the whole feed, and the running per-artifact byte ceiling refuses a band that lifts the accumulated extent past `budget.maxAssembledBytes` as `overrun` evidence before any state grows for it; the ceilings have one declaration and this rail enforces the stream half.
- Law: bands are held verbatim — the fold accumulates received `Uint8Array` views untouched; the join is the only allocation and nothing recodes a band.
- Growth: a frame envelope axis (a compression flag, a priority lane) is one field mirroring the C# emit; the fold's shape never changes.
- Boundary: the frame decode rides the codec registry's `ArtifactFrameWire` schema composition; the quarantine divert composes on the consuming stream; fetch scheduling against the ledger is `[04]`'s consumer.
- Packages: `effect` (`Schema`, `Stream`, `HashMap`, `Chunk`, `Array`, `Effect`, `Either`, `Option`); `./codec.ts` (`Gap`, `Parity`, `Quarantine`, `Wire`, `WireFault`); `./format.ts` (`Proto`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/schema.ts` (`Ingress`).

```typescript
import { Array, Chunk, Effect, Either, HashMap, Option, Schema, Stream } from "effect"
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
type _State = HashMap.HashMap<ContentKey, _Held>
type _Emit = Either.Either<Option.Option<{ readonly key: ContentKey; readonly bands: Chunk.Chunk<Uint8Array> }>, WireFault>

const _overrun = (frame: Frame, extent: number, ceiling: number): WireFault =>
  new WireFault({
    family: "ArtifactFrameWire",
    reason: "overrun",
    detail: "<assembled-over-budget>",
    evidence: Option.some({ actual: extent + frame.band.length, expected: ceiling }),
  })

const _gathered = (budget: Ingress.Shape) => (state: _State, frame: Frame): readonly [_State, _Emit] =>
  Option.match(HashMap.get(state, frame.artifact), {
    onNone: () =>
      frame.ordinal !== 0
        ? ([state, Either.left(Gap.evidence("ArtifactFrameWire", 0n, BigInt(frame.ordinal)))] as const)
        : frame.band.length > budget.maxAssembledBytes
          ? ([state, Either.left(_overrun(frame, 0, budget.maxAssembledBytes))] as const)
          : frame.total === 1
            ? ([state, Either.right(Option.some({ key: frame.artifact, bands: Chunk.of(frame.band) }))] as const)
            : ([
                HashMap.set(state, frame.artifact, { expect: 1, extent: frame.band.length, total: frame.total, bands: Chunk.of(frame.band) }),
                Either.right(Option.none()),
              ] as const),
    onSome: (held) =>
      frame.ordinal !== held.expect
        ? ([
            HashMap.remove(state, frame.artifact),
            Either.left(Gap.evidence("ArtifactFrameWire", BigInt(held.expect), BigInt(frame.ordinal))),
          ] as const)
        : frame.total !== held.total
          ? ([
              HashMap.remove(state, frame.artifact),
              Either.left(Gap.evidence("ArtifactFrameWire", BigInt(held.total), BigInt(frame.total), "<total-drift>")),
            ] as const)
          : held.extent + frame.band.length > budget.maxAssembledBytes
            ? ([HashMap.remove(state, frame.artifact), Either.left(_overrun(frame, held.extent, budget.maxAssembledBytes))] as const)
            : frame.ordinal + 1 === held.total
              ? ([
                  HashMap.remove(state, frame.artifact),
                  Either.right(Option.some({ key: frame.artifact, bands: Chunk.append(held.bands, frame.band) })),
                ] as const)
              : ([
                  HashMap.set(state, frame.artifact, {
                    ...held,
                    expect: held.expect + 1,
                    extent: held.extent + frame.band.length,
                    bands: Chunk.append(held.bands, frame.band),
                  }),
                  Either.right(Option.none()),
                ] as const),
  })
```

## [3]-[KEY_VERIFY]

[KEY_VERIFY]:
- Owner: `ArtifactFrame`, the assembled owner — the marked-kernel byte join, the delegated verify, and `reassembled`, the one stream surface turning a budgeted frame feed into verified artifacts; `Artifact` is the receipt class carrying key, extent, and frame count beside the assembled octets.
- Law: the verify is the plane's shared combinator — the held bands prove against the declared key through `Parity.verified` on its band-iterable payload modality, so this rail is the interchange's one content-mint delegation site and a per-consumer re-verify is the second-mint defect; a parity miss carries both keys as evidence and refuses BEFORE the summed buffer allocates, so a corrupted multi-frame artifact never costs its own join.
- Law: verify is per-artifact, not per-frame — frames carry no per-frame hash; the artifact's declared key proves the whole over one streaming digest walk, so a corrupted band surfaces exactly once and the joined buffer is never re-hashed.
- Exemption: `_joined` is a marked kernel — one allocation at the summed extent, bands copied in ordinal order, the draft detaching immutably at the return; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a second verified consumer composes `reassembled` — fold, join, and verify are one spelling.
- Boundary: the receipt and octets travel to the runtime wave's fetch worker and the data wave's object store, both of which compare keys and never re-mint; parity-refused artifacts quarantine at the consumer holding the octets.

```typescript
class Artifact extends Schema.Class<Artifact>("Artifact")({
  key: Digest.FromBytes,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  frames: Schema.Int.pipe(Schema.positive()),
}) {}

const _joined = (bands: Chunk.Chunk<Uint8Array>): Uint8Array => {
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
    frames.pipe(
      Stream.take(budget.maxFrames),
      Stream.mapAccum(HashMap.empty<ContentKey, _Held>(), _gathered(budget)),
      Stream.filterMap((emit) =>
        Either.match(emit, {
          onLeft: (fault) => Option.some(Effect.succeed(Either.left(fault))),
          onRight: (held) => Option.map(held, (ready) => Effect.either(_verifiedArtifact(ready.key, ready.bands))),
        })),
      Stream.mapEffect((settle) => settle, { concurrency: 1 }),
    ),
}
```

## [4]-[GEOMETRY_PLANE]

[GEOMETRY_PLANE]:
- Owner: `GeometryFrame`, the GLB control plane — mesh content key, LOD ordinal, the closed `encoding` vocabulary (`glb`, `draco`, `meshopt`), the tensor row set (semantic, dtype, shape, offset, stride), the artifact coordinate binding the envelope to its verified octets, and `joined`, the content-key rendezvous fold; `_views` is the dtype-to-constructor vocabulary the `view` and `extent` statics read.
- Law: the GLB band is consume-only carriage — the interchange opens no glTF parser; the band travels verbatim from artifact verify to the viewer's loader, and this plane's knowledge ends at the encoding row.
- Law: envelopes decode independently of octets — envelopes are small control-plane frames, octets are bulk artifact frames; the two planes join by content key, so envelope loss never strands verified bytes and byte loss never blocks planning.
- Law: the join is this rail's owned geometry — `joined` merges the settled envelope stream with the verified artifact stream and holds whichever side arrives first in a key-addressed rendezvous, emitting the `[envelope, receipt, octets]` triple on the match and replacing a stale hold on re-arrival; held state is bounded by the in-flight artifact set the `Ingress` budget already ceilings upstream, and what the triple feeds — decoder admission, GPU upload, scheduling — stays the runtime and ui waves' policy over these values.
- Law: the envelope streams instantiate the codec's `Wire.diverted` framed-divert combinator over their own suite rows — the frame page spells no pipeline of its own, and the `_framed` partial application is the only local surface; its quarantine thunks hold whole-message octets, the registry's replay coordinate, and `Proto.delimit` re-frames only where an egress joins a size-delimited transport.
- Law: views alias, transfers detach — `view` is a window over the verified buffer positioned by offset and sized by the shape product; the moment the buffer transfers to a decode worker the view is dead on this side, so views construct where they are consumed and never store in cells.
- Law: bounds prove before construction — `extent` prices a tensor's byte span off its own dtype row (shape product times element width), so the holder proves `byteOffset + extent(tensor)` against the verified buffer; an envelope overrunning its octets is `parity`-grade evidence minted by the holder of both extents, never a repair.
- Exemption: the typed-array window construction is the platform-forced seam; only the aliasing view leaves and the transfer list is the consumer's marshal declaration.
- Growth: a new encoding or tensor semantic is one literal row; a new dtype is one `_views` row — constructor and width land as data, and consumers never switch on dtype.
- Boundary: draco/meshopt decoder admission, GPU upload, and worker marshal are ui- and runtime-wave concerns over these values; the zero-copy worker crossing is the consumer's `Transferable.schema` declaration at its own marshal boundary, deliberately not a core surface.

```typescript
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
  static view(octets: Uint8Array, tensor: GeometryFrame.Tensor): GeometryFrame.View {
    return new _views[tensor.dtype].of(octets.buffer, octets.byteOffset + tensor.byteOffset, _count(tensor.shape))
  }
  static readonly extent = (tensor: GeometryFrame.Tensor): number => _count(tensor.shape) * _views[tensor.dtype].width
  static readonly joined = <E1, R1, E2, R2>(
    envelopes: Stream.Stream<GeometryFrame, E1, R1>,
    artifacts: Stream.Stream<readonly [Artifact, Uint8Array], E2, R2>,
  ): Stream.Stream<readonly [GeometryFrame, Artifact, Uint8Array], E1 | E2, R1 | R2> =>
    Stream.merge(
      Stream.map(envelopes, Either.left),
      Stream.map(artifacts, Either.right),
      { haltStrategy: "both" },
    ).pipe(
      Stream.mapAccum(
        HashMap.empty<ContentKey, Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>>(),
        (held, arrival): readonly [
          HashMap.HashMap<ContentKey, Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>>,
          Option.Option<readonly [GeometryFrame, Artifact, Uint8Array]>,
        ] =>
          Either.match(arrival, {
            onLeft: (envelope) =>
              Option.match(Option.flatMap(HashMap.get(held, envelope.artifact), Either.getRight), {
                onNone: () => [HashMap.set(held, envelope.artifact, Either.left(envelope)), Option.none()] as const,
                onSome: ([receipt, octets]) =>
                  [HashMap.remove(held, envelope.artifact), Option.some([envelope, receipt, octets] as const)] as const,
              }),
            onRight: ([receipt, octets]) =>
              Option.match(Option.flatMap(HashMap.get(held, receipt.key), Either.getLeft), {
                onNone: () => [HashMap.set(held, receipt.key, Either.right([receipt, octets] as const)), Option.none()] as const,
                onSome: (envelope) =>
                  [HashMap.remove(held, receipt.key), Option.some([envelope, receipt, octets] as const)] as const,
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

## [5]-[RESIDENCY_LEDGER]

[RESIDENCY_LEDGER]:
- Owner: `Residency`, the residency protocol — the closed `state` vocabulary (`resident`, `pending`, `evicted`), the `Manifest` class (mesh rows keyed by content key with LOD and extent), the `Delta` class (one row's transition, declared over `_Row.fields` so transition and ledger row are one field record), the shape-discriminated envelope union both arrive on — the two arms share no field record, so the union decode selects structurally with no `kind` column — and `folded`, the one polymorphic ledger fold discriminating on the arrival value: a `Manifest` REPLACES the ledger whole, a `Delta` evolves it.
- Law: the ledger IS the fetch plan — `pending` rows are the fetch queue, `resident` rows are addressable, `evicted` rows are reclaimable; the runtime transport schedules against it and the artifact receipts confirm arrivals by the same key, so residency questions are `HashMap` lookups, never scans.
- Law: deltas are idempotent transitions — a delta matching the ledger's current state is a no-op, so the wire replays deltas across reconnects and the fold absorbs them; a delta referencing an unknown key folds to an insert because the manifest introducing it may still be in flight.
- Law: the manifest is authoritative at arrival — the C# render side owns truth, so the manifest arm discards the prior ledger whole and deltas only evolve the last manifest.
- Law: one wire family, one envelope — manifest and delta are the two arms of one message; a second decode surface per arm forks the family the census fences.
- Growth: a new residency state is one literal row every exhaustive consumer breaks on until handled; a new row axis (priority score, byte budget) is one field on the row struct.
- Boundary: fetch scheduling, worker transfer, and byte budgets are the runtime wave's policy; the artifact verify that flips `pending` to `resident` is `[03]`'s receipt joined at the consumer.

```typescript
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
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly Delta: typeof Delta
  readonly envelope: Schema.Schema<Residency.Arrival, Uint8Array>
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<Either.Either<Residency.Arrival, WireFault>, WireFault, Quarantine>
  readonly folded: (ledger: Residency.Ledger, arrival: Residency.Arrival) => Residency.Ledger
} = {
  Manifest,
  Delta,
  envelope: Proto.family(Proto.suite.GeometryResidencyWire, _envelope),
  stream: _framed(Proto.suite.GeometryResidencyWire, "GeometryResidencyWire", _envelope),
  folded: (ledger, arrival) =>
    arrival instanceof Manifest
      ? Array.reduce(arrival.rows, HashMap.empty<ContentKey, Residency.Row>(), (acc, row) => HashMap.set(acc, row.mesh, row))
      : HashMap.set(ledger, arrival.mesh, arrival),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ArtifactFrame, GeometryFrame, Residency }
```
