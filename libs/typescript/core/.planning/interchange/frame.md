# [CORE_FRAME]

`Frame` owns bounded artifact reassembly, verified geometry rendezvous, generation-aware residency, and IFC container admission. Interleaved bands fold by artifact and generation under one ingress budget, verification gates the joined allocation, tensor views prove span, stride, and alignment, and manifests replace ledger state while generation-matched deltas patch it. Module `core/src/interchange/frame.ts` admits an arrival class as one refusal row, a tensor element type as one view row, a residency payload as one kind row, and an IFC serialization as one admission row.

`Frame` composes the `value` floor's `Digest` identity and `Shape.Ingress` ceilings, the `codec` owner's fault, gap, parity, and quarantine rails, and the `format` owner's proto suite and JSON schema mints. Producers own every payload axis crossing this plane, so `Frame` folds arrivals into receipts, views, ledgers, and admissions and mints no payload axis of its own.

## [01]-[INDEX]

- [02]-[FRAME_PROTOCOL]: bounded keyed frame assembly and sequence evidence; `Frame.Artifact`.
- [03]-[KEY_VERIFY]: delegated verification and single-allocation joins; `Frame.Artifact`.
- [04]-[GEOMETRY_PLANE]: geometry envelopes, tensor views, and rendezvous; `Frame.Geometry`.
- [05]-[RESIDENCY_LEDGER]: generation-aware manifest replacement and delta folding; `Frame.Residency`.
- [06]-[IFC_ADMISSION]: serialization rows, the sparse container and release crossings, the release-header read; `Frame.Ifc`.

## [02]-[FRAME_PROTOCOL]

- Owner: `Frame.Artifact` folds interleaved dense frame bands by artifact and generation.
- Law: ordinal gaps, total drift, budget overruns, and unfinished tails emit `Wire.Fault` evidence.
- Law: raw bands remain unchanged until the verified single-allocation join.
- Law: `Shape.Ingress` owns frame and byte ceilings.
- Packages: `effect`; `./codec.ts` (`Wire`); `./format.ts` (`Format`); value `Digest` and `Shape`.

```typescript signature
import { Array, Chunk, Effect, Either, HashMap, Option, Ref, Schema, Stream } from "effect"
import { Digest } from "../value/contentKey.ts"
import { Shape } from "../value/schema.ts"
import { Wire } from "./codec.ts"
import { Format } from "./format.ts"

class ArtifactBand extends Schema.Class<ArtifactBand>("ArtifactBand")({
  artifact: Digest.codecs.content.bytes,
  generation: Shape.Refined.OrdinalKey,
  ordinal: Shape.Refined.OrdinalKey,
  total: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  band: Schema.Uint8ArrayFromSelf,
}) {}

type _Held = { readonly expect: number; readonly extent: number; readonly total: number; readonly bands: Chunk.Chunk<Uint8Array> }
type _Coordinate = `${Digest.Key<"content">}:${number}`
type _State = { readonly seen: number; readonly held: HashMap.HashMap<_Coordinate, _Held> }
type _Emit = Either.Either<
  Option.Option<{ readonly key: Digest.Key<"content">; readonly generation: number; readonly bands: Chunk.Chunk<Uint8Array> }>,
  Wire.Fault
>

const _SEED: _State = { seen: 0, held: HashMap.empty() }

const _overrun = (
  family: Wire.FaultFamily,
  detail: string,
  actual: number,
  expected: number,
  coordinate?: { readonly key: Digest.Key<"content">; readonly generation: number },
): Wire.Fault =>
  new Wire.Fault({
    family,
    reason: "overrun",
    detail,
    evidence: Option.some(coordinate === undefined
      ? { actual, expected }
      : { artifact: coordinate.key, generation: coordinate.generation, actual, expected }),
  })

const _coordinateOf = (key: Digest.Key<"content">, generation: number): _Coordinate => `${key}:${generation}`
const _coordinate = (frame: ArtifactBand): _Coordinate => _coordinateOf(frame.artifact, frame.generation)
const _next = (state: _State, held: HashMap.HashMap<_Coordinate, _Held>): _State => ({ seen: state.seen + 1, held })

// Refusal rows, one roster per arrival class: growth is a row rather than another ternary rung, and the
// refusal-to-state correspondence is structural — an OPEN arrival holds nothing to drop whichever row fires, a HELD
// one drops its artifact whichever row fires — so the fold body carries exactly two decisions past the roster read.
type _OpenRefusal = { readonly when: (frame: ArtifactBand) => boolean; readonly fault: (frame: ArtifactBand) => Wire.Fault }
type _HeldRefusal = {
  readonly when: (frame: ArtifactBand, held: _Held) => boolean
  readonly fault: (frame: ArtifactBand, held: _Held) => Wire.Fault
}

const _gathered = (budget: Shape.Ingress) => {
  const opening: ReadonlyArray<_OpenRefusal> = [
    { // a first frame at a nonzero ordinal is a headless arrival: the same gap evidence at expected zero
      when: (frame) => frame.ordinal !== 0,
      fault: (frame) => Wire.Gap.evidence("ArtifactFrame", 0n, BigInt(frame.ordinal)),
    },
    {
      when: (frame) => frame.band.length > budget.bytes,
      fault: (frame) => _overrun("ArtifactFrame", "<assembled-over-budget>", frame.band.length, budget.bytes,
        { key: frame.artifact, generation: frame.generation }),
    },
  ]
  const holding: ReadonlyArray<_HeldRefusal> = [
    {
      when: (frame, held) => frame.ordinal !== held.expect,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactFrame", BigInt(held.expect), BigInt(frame.ordinal)),
    },
    {
      when: (frame, held) => frame.total !== held.total,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactFrame", BigInt(held.total), BigInt(frame.total), "<total-drift>"),
    },
    {
      when: (frame, held) => held.extent + frame.band.length > budget.bytes,
      fault: (frame, held) =>
        _overrun("ArtifactFrame", "<assembled-over-budget>", held.extent + frame.band.length, budget.bytes,
          { key: frame.artifact, generation: frame.generation }),
    },
  ]
  return (state: _State, frame: ArtifactBand): readonly [_State, _Emit] =>
    state.seen >= budget.frames
      ? ([_next(state, state.held), Either.left(_overrun("ArtifactFrame", "<frame-budget>", state.seen + 1, budget.frames,
          { key: frame.artifact, generation: frame.generation }))] as const)
      : Option.match(HashMap.get(state.held, _coordinate(frame)), {
          onNone: () =>
            Option.match(Option.map(Array.findFirst(opening, (row) => row.when(frame)), (row) => row.fault(frame)), {
              onSome: (fault) => [_next(state, state.held), Either.left(fault)] as const,
              onNone: () =>
                frame.total === 1
                  ? ([_next(state, state.held), Either.right(Option.some({ key: frame.artifact, generation: frame.generation, bands: Chunk.of(frame.band) }))] as const)
                  : ([
                      _next(state, HashMap.set(state.held, _coordinate(frame), {
                        expect: 1,
                        extent: frame.band.length,
                        total: frame.total,
                        bands: Chunk.of(frame.band),
                      })),
                      Either.right(Option.none()),
                    ] as const),
            }),
          onSome: (held) =>
            Option.match(
              Option.map(Array.findFirst(holding, (row) => row.when(frame, held)), (row) => row.fault(frame, held)),
              {
                onSome: (fault) => [_next(state, HashMap.remove(state.held, _coordinate(frame))), Either.left(fault)] as const,
                onNone: () =>
                  frame.ordinal + 1 === held.total
                    ? ([
                        _next(state, HashMap.remove(state.held, _coordinate(frame))),
                        Either.right(Option.some({ key: frame.artifact, generation: frame.generation, bands: Chunk.append(held.bands, frame.band) })),
                      ] as const)
                    : ([
                        _next(state, HashMap.set(state.held, _coordinate(frame), {
                          ...held,
                          expect: held.expect + 1,
                          extent: held.extent + frame.band.length,
                          bands: Chunk.append(held.bands, frame.band),
                        })),
                        Either.right(Option.none()),
                      ] as const),
              },
            ),
        })
}
```

## [03]-[KEY_VERIFY]

- Owner: `Frame.Artifact` verifies held bands before joining and emits the artifact receipt with its octets.
- Law: verification covers the complete artifact, and parity failure prevents the joined allocation.
- Exemption: `_joined` performs one bounded allocation and ordinal copy.

```typescript signature
class Artifact extends Schema.Class<Artifact>("Artifact")({
  key: Digest.codecs.content.bytes,
  generation: Shape.Refined.OrdinalKey,
  extent: Shape.Refined.OrdinalKey,
  frames: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
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
  key: Digest.Key<"content">,
  generation: number,
  bands: Chunk.Chunk<Uint8Array>,
): Effect.Effect<readonly [Artifact, Uint8Array], Wire.Fault> =>
  Wire.Parity.verified("ArtifactFrame", key, bands).pipe(
    Effect.map(() => {
      const octets = _joined(bands)
      return [new Artifact({ key, generation, extent: octets.length, frames: Chunk.size(bands) }), octets] as const
    }),
  )

const _unfinished = (state: _State): ReadonlyArray<_Emit> =>
  Array.map(Array.fromIterable(HashMap.values(state.held)), (held) =>
    Either.left(Wire.Gap.evidence("ArtifactFrame", BigInt(held.total), BigInt(held.expect), "<truncated>")))

const _artifactWire = <F, FI, S, SI>(
  format: Schema.Schema<F, FI>,
  revision: Schema.Schema<S, SI>,
) => Schema.Struct({
  format,
  bytes: Schema.Uint8ArrayFromBase64,
  schema: revision,
  content: Digest.codecs.content.wire,
  at: Schema.DateTimeUtc,
})

const ArtifactFrame: {
  readonly Frame: typeof ArtifactBand
  readonly Artifact: typeof Artifact
  readonly wire: typeof _artifactWire
  readonly frame: Schema.Schema<Frame, Uint8Array>
  readonly reassembled: <E, R>(
    frames: Stream.Stream<ArtifactBand, E, R>,
    budget?: Shape.Ingress,
  ) => Stream.Stream<Either.Either<readonly [Artifact, Uint8Array], Wire.Fault>, E, R>
} = {
  Frame: ArtifactBand,
  Artifact,
  wire: _artifactWire,
  frame: Format.proto.family(Format.proto.suite.ArtifactFrame, ArtifactBand),
  reassembled: (frames, budget = Shape.Ingress.floor) =>
    Stream.unwrap(
      Ref.make(_SEED).pipe(
        Effect.map((state) => {
          const step = _gathered(budget) // one partial application per feed: the refusal rosters and the closure build once, never per frame
          return frames.pipe(
            Stream.mapEffect((frame) =>
              Ref.modify(state, (current) => {
                const [next, emit] = step(current, frame)
                return [emit, next] as const
              })),
            Stream.concat(Stream.fromEffect(Ref.get(state)).pipe(Stream.flatMap((settled) => Stream.fromIterable(_unfinished(settled))))),
            Stream.filterMap((emit) =>
              Either.match(emit, {
                onLeft: (fault) => Option.some(Effect.succeed(Either.left(fault))),
                onRight: (held) => Option.map(held, (ready) =>
                  Effect.either(_verifiedArtifact(ready.key, ready.generation, ready.bands))),
              })),
            Stream.mapEffect((settle) => settle, { concurrency: 1 }),
          )
        }),
      ),
    ),
}
```

## [04]-[GEOMETRY_PLANE]

- Owner: `Frame.Geometry` binds geometry envelopes to verified artifacts and proves typed-array views.
- Law: rendezvous keys include artifact and generation, and both unmatched tails emit typed evidence.
- Law: checked multiplication and addition prove tensor span, stride, alignment, and ingress budget before construction.
- Exemption: `_packed` owns typed-array window construction and strided gathering.

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

const _product = (values: ReadonlyArray<number>): Option.Option<number> =>
  Array.reduce(values, Option.some(1), (held, value) =>
    Option.flatMap(held, (total) => value <= Number.MAX_SAFE_INTEGER / total ? Option.some(total * value) : Option.none()))

const _sum = (left: number, right: number): Option.Option<number> =>
  right <= Number.MAX_SAFE_INTEGER - left ? Option.some(left + right) : Option.none()

const _count = (shape: ReadonlyArray<number>): number => Option.getOrThrow(_product(shape))

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

const _payloadStream = <A>(
  family: Wire.FaultFamily,
  schema: Schema.Schema<A, Uint8Array>,
  frames: AsyncIterable<Uint8Array>,
): Stream.Stream<Either.Either<A, Wire.Fault>, Wire.Fault, Wire.Quarantine> =>
  Stream.fromAsyncIterable(frames, (defect) =>
    new Wire.Fault({ family, reason: "malformed", detail: String(defect), evidence: Option.none() })).pipe(
    Stream.mapEffect((octets) =>
      Schema.decodeUnknown(schema)(octets).pipe(
        Effect.mapError((issue) =>
          new Wire.Fault({ family, reason: "malformed", detail: issue.message, evidence: Option.none() })),
        Wire.Quarantine.divert({ family, octets: () => octets }),
      ), { concurrency: 1 }),
  )

const _Tensor = Schema.Struct({
  semantic: Schema.Literal(..._semantics),
  dtype: Schema.Literal(..._dtypes),
  shape: Schema.NonEmptyArray(Shape.Refined.OrdinalKey.pipe(Schema.positive())).pipe(
    Schema.filter((shape) => Option.isSome(_product(shape)) || "<tensor-shape-overflow>"),
  ),
  byteOffset: Shape.Refined.OrdinalKey,
  byteStride: Shape.Refined.OrdinalKey,
}).pipe(
  Schema.filter((tensor) => {
    const count = _product(tensor.shape)
    if (Option.isNone(count)) return "<tensor-shape-overflow>"
    const width = _views[tensor.dtype].width
    if (count.value > Number.MAX_SAFE_INTEGER / width) return "<tensor-extent-overflow>"
    const columns = Array.lastNonEmpty(tensor.shape)
    const rows = count.value / columns
    const rowBytes = columns * width
    const span = tensor.byteStride === 0
      ? count.value * width
      : tensor.byteStride > Number.MAX_SAFE_INTEGER / Math.max(0, rows - 1)
        ? Number.POSITIVE_INFINITY
        : tensor.byteStride * (rows - 1) + rowBytes
    return Number.isSafeInteger(span) && Option.isSome(_sum(tensor.byteOffset, span)) || "<tensor-span-overflow>"
  }),
)

class GeometryFrame extends Schema.Class<GeometryFrame>("GeometryFrame")({
  mesh: Digest.codecs.content.bytes,
  artifact: Digest.codecs.content.bytes,
  generation: Shape.Refined.OrdinalKey,
  lod: Shape.Refined.OrdinalKey,
  encoding: Schema.Literal(..._encodings),
  tensors: Schema.Array(_Tensor),
}) {
  static readonly payload: Schema.Schema<GeometryFrame, Uint8Array> =
    Format.proto.family(Format.proto.suite.GeometryPayload, GeometryFrame)
  static readonly stream = (
    frames: AsyncIterable<Uint8Array>,
  ): Stream.Stream<Either.Either<GeometryFrame, Wire.Fault>, Wire.Fault, Wire.Quarantine> =>
    _payloadStream("GeometryPayload", GeometryFrame.payload, frames)
  static readonly extent = (tensor: GeometryFrame.Tensor): number => _span(tensor)
  static readonly view = (octets: Uint8Array, tensor: GeometryFrame.Tensor): Either.Either<GeometryFrame.View, Wire.Fault> =>
    octets.byteLength > Shape.Ingress.floor.bytes || _span(tensor) > Shape.Ingress.floor.bytes
      ? Either.left(
          new Wire.Fault({
            family: "GeometryPayload",
            reason: "overrun",
            detail: "<tensor-budget>",
            evidence: Option.some({ actual: Math.max(octets.byteLength, _span(tensor)), expected: Shape.Ingress.floor.bytes }),
          }),
        )
      : tensor.byteStride !== 0 && tensor.byteStride < _rowBytes(tensor)
      ? Either.left(
          new Wire.Fault({
            family: "GeometryPayload",
            reason: "overrun",
            detail: "<tensor-stride>",
            evidence: Option.some({ actual: tensor.byteStride, expected: _rowBytes(tensor) }),
          }),
        )
      : tensor.byteOffset + _span(tensor) > octets.byteLength
      ? Either.left(
          new Wire.Fault({
            family: "GeometryPayload",
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
    budget: Shape.Ingress = Shape.Ingress.floor,
  ): Stream.Stream<Either.Either<readonly [GeometryFrame, Artifact, Uint8Array], Wire.Fault>, E1 | E2, R1 | R2> =>
    _joinedGeometry(envelopes, artifacts, budget)
}

declare namespace GeometryFrame {
  type Encoding = (typeof _encodings)[number]
  type Dtype = (typeof _dtypes)[number]
  type Tensor = Schema.Schema.Type<typeof _Tensor>
  type View = InstanceType<(typeof _views)[Dtype]["of"]>
  type _Views<T extends Record<Dtype, { readonly of: unknown; readonly width: number }> = typeof _views> = T
}

type _Rendezvous = HashMap.HashMap<
  _Coordinate,
  Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>
>
type _Join = Either.Either<readonly [GeometryFrame, Artifact, Uint8Array], Wire.Fault>

const _rendezvous = (
  held: _Rendezvous,
  arrival: Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>,
  budget: Shape.Ingress,
): readonly [_Rendezvous, Option.Option<_Join>] =>
  Either.match(arrival, {
    onLeft: (envelope) => {
      const coordinate = _coordinateOf(envelope.artifact, envelope.generation)
      return Option.match(Option.flatMap(HashMap.get(held, coordinate), Either.getRight), {
        onNone: () => HashMap.size(held) >= budget.collection
          ? [held, Option.some(Either.left(_overrun("GeometryPayload", "<geometry-rendezvous>",
              HashMap.size(held) + 1, budget.collection, { key: envelope.artifact, generation: envelope.generation })))] as const
          : [HashMap.set(held, coordinate, Either.left(envelope)), Option.none()] as const,
        onSome: ([receipt, octets]) =>
          [HashMap.remove(held, coordinate), Option.some(Either.right([envelope, receipt, octets] as const))] as const,
      })
    },
    onRight: ([receipt, octets]) => {
      const coordinate = _coordinateOf(receipt.key, receipt.generation)
      return Option.match(Option.flatMap(HashMap.get(held, coordinate), Either.getLeft), {
        onNone: () => HashMap.size(held) >= budget.collection
          ? [held, Option.some(Either.left(_overrun("GeometryPayload", "<geometry-rendezvous>",
              HashMap.size(held) + 1, budget.collection, { key: receipt.key, generation: receipt.generation })))] as const
          : [HashMap.set(held, coordinate, Either.right([receipt, octets] as const)), Option.none()] as const,
        onSome: (envelope) =>
          [HashMap.remove(held, coordinate), Option.some(Either.right([envelope, receipt, octets] as const))] as const,
      })
    },
  })

const _unmatched = (held: _Rendezvous): ReadonlyArray<Option.Option<_Join>> =>
  Array.map(Array.fromIterable(HashMap.entries(held)), ([coordinate, lane]) =>
    Option.some(Either.left(new Wire.Fault({
      family: "GeometryPayload",
      reason: "truncated",
      detail: Either.isLeft(lane) ? `<unmatched-envelope:${coordinate}>` : `<unmatched-artifact:${coordinate}>`,
      evidence: Option.none(),
    }))))

const _joinedGeometry = <E1, R1, E2, R2>(
  envelopes: Stream.Stream<GeometryFrame, E1, R1>,
  artifacts: Stream.Stream<readonly [Artifact, Uint8Array], E2, R2>,
  budget: Shape.Ingress,
): Stream.Stream<_Join, E1 | E2, R1 | R2> =>
  Stream.unwrap(
    Ref.make(HashMap.empty<_Coordinate, Either.Either<readonly [Artifact, Uint8Array], GeometryFrame>>()).pipe(
      Effect.map((cell) =>
        Stream.merge(Stream.map(envelopes, Either.left), Stream.map(artifacts, Either.right), { haltStrategy: "both" }).pipe(
          Stream.mapEffect((arrival) => Ref.modify(cell, (held) => {
            const [next, emit] = _rendezvous(held, arrival, budget)
            return [emit, next] as const
          })),
          Stream.concat(Stream.fromEffect(Ref.get(cell)).pipe(Stream.flatMap((held) => Stream.fromIterable(_unmatched(held))))),
          Stream.filterMap((emit) => emit),
        )),
    ),
  )
```

## [05]-[RESIDENCY_LEDGER]

- Owner: `Frame.Residency` folds authoritative manifests and generation-matched deltas into a keyed ledger.
- Law: manifests replace state, duplicate keys refuse, and a delta applies at the held generation and scene alone.
- Law: refusal direction grades the arrival — a superseded one reads `stale`, an unreached one `conflict`, both carrying the generation pair.
- Law: `pending`, `census`, and `kind` projections derive scheduling and render policy from the same rows.
- Law: Generation supersession ends a row's lifetime, and the arriving manifest is the sole authority ending it.

```typescript signature
const _states = ["resident", "pending", "evicted"] as const

// Producers cross their payload axis verbatim, and its two consumer columns cross with it: `coneCullable` names the
// rows a cluster cull may reject before upload, `splatBorne` the rows a raster shader cannot draw. A consumer told
// only a mesh key and an extent has to infer both from the bytes it has not fetched yet.
const _kinds = ["meshlet-cluster", "quantized-vertex", "point-splat", "gaussian-splat"] as const
// `as const satisfies`: a mapped ANNOTATION widens both columns to `boolean` and erases the row literals, so a
// consumer told to raise over a column reads `boolean` and every `extends true` derivation beside it resolves `never`
// while it reads correct; the guard pair below closes the table against the tuple in both directions.
const _kindRows = {
  "meshlet-cluster": { coneCullable: true, splatBorne: false },
  "quantized-vertex": { coneCullable: false, splatBorne: false },
  "point-splat": { coneCullable: false, splatBorne: false },
  "gaussian-splat": { coneCullable: false, splatBorne: true },
} as const satisfies { readonly [K in Residency.Kind]: { readonly coneCullable: boolean; readonly splatBorne: boolean } }

const _Row = Schema.Struct({
  mesh: Digest.codecs.content.wire,
  lod: Shape.Refined.OrdinalKey,
  extent: Shape.Refined.OrdinalKey,
  kind: Schema.Literal(..._kinds),
  state: Schema.Literal(..._states),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  scene: Digest.codecs.content.wire,
  generation: Shape.Refined.OrdinalKey,
  rows: Schema.Array(_Row).pipe(
    Schema.filter((rows) => rows.length <= Shape.Ingress.floor.collection || "<residency-collection>"),
    Schema.filter((rows) => Array.dedupe(Array.map(rows, (row) => row.mesh)).length === rows.length || "<duplicate-residency-key>"),
  ),
  minted: Schema.DateTimeUtc,
}) {}

class Delta extends Schema.Class<Delta>("Delta")({
  scene: Digest.codecs.content.wire,
  generation: Shape.Refined.OrdinalKey,
  ..._Row.fields,
}) {}

const _envelope: Schema.Union<[typeof Manifest, typeof Delta]> = Schema.Union(Manifest, Delta)

declare namespace Residency {
  type Kind = (typeof _kinds)[number]
  type State = (typeof _states)[number]
  type Row = Schema.Schema.Type<typeof _Row>
  type Arrival = Manifest | Delta
  type Ledger = {
    readonly scene: Option.Option<Digest.Key<"content">>
    readonly generation: number
    readonly rows: HashMap.HashMap<Digest.Key<"content">, Row>
  }
  type Tally = { readonly count: number; readonly extent: number }
  type Census = { readonly [S in State]: Tally }
  type _Kinds<T extends Record<Kind, { readonly coneCullable: boolean; readonly splatBorne: boolean }> = typeof _kindRows> = T
}

const _EMPTY_LEDGER: Residency.Ledger = { scene: Option.none(), generation: 0, rows: HashMap.empty() }

// Direction names the refusal and the poison census grades on it: a ledger PAST the arrival supersedes it, while an
// arrival at or beyond the held generation contradicts a coordinate this fold never reached and applies whole once
// its manifest lands. Evidence carries the pair a board drills on, so this refusal reads like every other on the page.
const _residencyFault = (ledger: Residency.Ledger, arrival: Residency.Arrival): Wire.Fault =>
  new Wire.Fault({
    family: "GeometryResidencyWire",
    reason: arrival.generation < ledger.generation ? "stale" : "conflict",
    detail: arrival.generation < ledger.generation
      ? "<residency-superseded>"
      : arrival.generation === ledger.generation
      ? "<residency-scene>"
      : "<residency-unreached>",
    evidence: Option.some({
      artifact: arrival.scene,
      generation: arrival.generation,
      actual: arrival.generation,
      expected: ledger.generation,
    }),
  })

const _landed = (ledger: Residency.Ledger, arrival: Residency.Arrival): Either.Either<Residency.Ledger, Wire.Fault> =>
  arrival instanceof Manifest
    ? arrival.generation < ledger.generation
      ? Either.left(_residencyFault(ledger, arrival))
      : Either.right({
          scene: Option.some(arrival.scene),
          generation: arrival.generation,
          rows: Array.reduce(arrival.rows, HashMap.empty<Digest.Key<"content">, Residency.Row>(),
            (rows, row) => HashMap.set(rows, row.mesh, row)),
        })
    : ledger.generation !== arrival.generation || !Option.exists(ledger.scene, (scene) => scene === arrival.scene)
      ? Either.left(_residencyFault(ledger, arrival))
      : !HashMap.has(ledger.rows, arrival.mesh) && HashMap.size(ledger.rows) >= Shape.Ingress.floor.collection
        ? Either.left(_overrun(
            "GeometryResidencyWire",
            "<residency-collection>",
            HashMap.size(ledger.rows) + 1,
            Shape.Ingress.floor.collection,
            { key: arrival.mesh, generation: arrival.generation },
          ))
        : Either.right({ ...ledger, rows: HashMap.set(ledger.rows, arrival.mesh, arrival) })

function _folded(ledger: Residency.Ledger, arrival: Residency.Arrival): Either.Either<Residency.Ledger, Wire.Fault>
function _folded<E, R>(arrivals: Stream.Stream<Residency.Arrival, E, R>): Stream.Stream<Either.Either<Residency.Ledger, Wire.Fault>, E, R>
function _folded<E, R>(
  ...input: readonly [Residency.Ledger, Residency.Arrival] | readonly [Stream.Stream<Residency.Arrival, E, R>]
): Either.Either<Residency.Ledger, Wire.Fault> | Stream.Stream<Either.Either<Residency.Ledger, Wire.Fault>, E, R> {
  return input.length === 2
    ? _landed(input[0], input[1])
    : Stream.mapAccum(input[0], _EMPTY_LEDGER, (ledger, arrival) =>
        Either.match(_landed(ledger, arrival), {
          onLeft: (fault) => [ledger, Either.left(fault)] as const,
          onRight: (next) => [next, Either.right(next)] as const,
        }))
}

const _EMPTY_CENSUS: Residency.Census = {
  resident: { count: 0, extent: 0 },
  pending: { count: 0, extent: 0 },
  evicted: { count: 0, extent: 0 },
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly Delta: typeof Delta
  readonly empty: Residency.Ledger
  readonly kinds: typeof _kinds
  readonly kind: typeof _kindRows
  readonly envelope: Schema.Schema<Residency.Arrival, Uint8Array>
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<Either.Either<Residency.Arrival, Wire.Fault>, Wire.Fault, Wire.Quarantine>
  readonly folded: typeof _folded
  readonly pending: (ledger: Residency.Ledger) => ReadonlyArray<Residency.Row>
  readonly census: (ledger: Residency.Ledger) => Either.Either<Residency.Census, Wire.Fault>
} = {
  Manifest,
  Delta,
  empty: _EMPTY_LEDGER,
  kinds: _kinds,
  kind: _kindRows,
  // Residency envelopes arrive as the AppUi shell's source-generated JSON mint, so both rows ride the json arm while
  // both Compute-minted frame families above keep the proto walk their own descriptor source declares
  envelope: Format.json.schema(_envelope),
  stream: (frames) => _payloadStream("GeometryResidencyWire", Format.json.schema(_envelope), frames),
  folded: _folded,
  pending: (ledger) => Array.filter(Array.fromIterable(HashMap.values(ledger.rows)), (row) => row.state === "pending"),
  census: (ledger) =>
    HashMap.reduce(ledger.rows, Either.right(_EMPTY_CENSUS), (held, row) =>
      Either.flatMap(held, (acc) => {
        const tally = acc[row.state]
        return Option.match(Option.all([_sum(tally.count, 1), _sum(tally.extent, row.extent)]), {
          onNone: () => Either.left(_overrun(
            "GeometryResidencyWire",
            "<residency-census>",
            tally.extent,
            Number.MAX_SAFE_INTEGER,
            { key: row.mesh, generation: ledger.generation },
          )),
          onSome: ([count, extent]) => Either.right({ ...acc, [row.state]: { count, extent } }),
        })
      })),
}

```

## [06]-[IFC_ADMISSION]

- Owner: `Frame.Ifc` owns the IFC wire form — serialization rows, the container column, and the release roster both cross sparsely.
- Law: Serialization and container stay separate axes whose product is SPARSE on both crossings — a wrapper names the serializations it carries and never the text inside them, so `zip` admits STEP and XML alone, and each serialization names the releases it publishes for, so `Ifc.published` refuses an unpublished pair by row where a cross product admits a document no schema validates.
- Law: `IFC4X3` publishes under no serialization — the ISO-approved 4.3 line carries the `IFC4X3_ADD2` identifier and every published 4.3 artifact spells it, so the roster keeps the token to NAME that refusal rather than failing an unknown literal at decode.
- Law: `ifcx` is IFC5's own encoding rather than a `json` release — a document there carries its release at `header.ifcxVersion` where ifcJSON reads `schemaIdentifier`, so folding IFC5 onto the JSON row forks two vocabularies into one member read; that `json` row itself publishes against a community-maintained schema where `step` and `xml` publish against the ISO editions, a weaker claim its consumers price at the seam.
- Law: Each serialization declares the header member carrying its release, so admission reads a named member rather than guessing.
- Law: `sniff` prices the release read, and an inflating container raises its serialization's extent to the whole document.
- Law: Rows decide selection, admission, and `degrade` alone — a wire form realizes no tenancy and ends no lifetime.
- Growth: a serialization is one `_ifcRows` row, a container one `_ifcContainerRows` row, a release one roster entry beside the row cells that publish it.
- Boundary: `Frame` carries the wire form and its descriptor; entity-graph authoring and release raising stay with the producing runtime.
- Packages: `effect` (`Array`, `Option`, `Schema`); `./format.ts` (`Format`).

```typescript signature
// Containers wrap whatever text a serialization wrote, so the wrapper rides its OWN column: a zip seated beside the
// serializations names no text at all, leaves a zipped ifcXML with no seat, and hands every reader a format token it
// must re-inspect the bytes to interpret. Two axes cross instead, and the product is generated rather than enumerated.
const _ifcSerializations = ["step", "xml", "json", "ifcx"] as const
const _ifcContainers = ["plain", "zip"] as const
const _ifcReleases = ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3", "IFC4X3_ADD2", "IFC5"] as const

// `sniff` prices the release read in the ONE unit an ingress budget spends — how much payload must arrive before the
// release is known. Serializations differ genuinely here: a STEP header is a line, an XML root is one element, and a
// JSON member carries no ordering guarantee at all, so `degrade` states each forfeit rather than implying parity.
const _ifcSniffs = ["line", "element", "document"] as const

// `releases` is the SPAN a serialization publishes across, and it is sparse rather than total: STEP and XML carry the
// four editions that shipped an EXPRESS schema and an XSD together, ifcJSON carries the one release its schema was
// authored for, and IFCX carries IFC5 alone. `IFC4X3` seats in the roster with no row naming it, so a document
// spelling that identifier refuses by name — the ISO edition publishes as `IFC4X3_ADD2` and every artifact says so.
const _ifcRows = {
  step: {
    extension: ".ifc",
    header: "FILE_SCHEMA",
    sniff: "line",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    degrade: "<release-unknown-before-header-line>",
  },
  xml: {
    extension: ".ifcxml",
    header: "xmlns",
    sniff: "element",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    degrade: "<release-unknown-before-root-element>",
  },
  json: {
    extension: ".ifcjson",
    header: "schemaIdentifier",
    sniff: "document",
    releases: ["IFC4"],
    degrade: "<release-unknown-before-whole-document>",
  },
  ifcx: {
    extension: ".ifcx",
    header: "header.ifcxVersion",
    sniff: "document",
    releases: ["IFC5"],
    degrade: "<release-carried-as-ifcx-version>",
  },
} as const satisfies {
  readonly [S in Ifc.Serialization]: {
    readonly extension: string
    readonly header: string
    readonly sniff: Ifc.Sniff
    readonly releases: Array.NonEmptyReadonlyArray<Ifc.Release>
    readonly degrade: string
  }
}

// Inflation is the container's whole consequence, so the column states it and the descriptor DERIVES the raised extent
// rather than a second row restating every serialization under a wrapper. `wraps` is the other sparse crossing: the
// zip container admits STEP and XML text alone, so a zipped ifcJSON or ifcx names a wrapper nothing defines.
const _ifcContainerRows = {
  plain: {
    extension: Option.none(),
    inflates: false,
    wraps: _ifcSerializations,
    degrade: "<none>",
  },
  zip: {
    extension: Option.some(".ifczip"),
    inflates: true,
    wraps: ["step", "xml"],
    degrade: "<sniff-extent-raised-to-document>",
  },
} as const satisfies {
  readonly [C in Ifc.Container]: {
    readonly extension: Option.Option<string>
    readonly inflates: boolean
    readonly wraps: Array.NonEmptyReadonlyArray<Ifc.Serialization>
    readonly degrade: string
  }
}

const _IfcForm = Schema.Struct({
  serialization: Schema.Literal(..._ifcSerializations),
  container: Schema.Literal(..._ifcContainers),
})

// Two row reads answer the whole descriptor: a wrapper's extension WINS where it names one, and inflation raises
// sniff to the whole document. Matching on that extension instead splits one derivation into two arms whose
// header column is identical and whose sniff column differs only by the `inflates` cell the row already carries.
const _ifcDescriptor = (form: Ifc.Form): Ifc.Descriptor => {
  const row = _ifcRows[form.serialization]
  const wrapper = _ifcContainerRows[form.container]
  return {
    extension: Option.getOrElse(wrapper.extension, () => row.extension),
    header: row.header,
    sniff: wrapper.inflates ? "document" : row.sniff,
  }
}

// Admission reads BOTH sparse crossings off the rows a caller can also read, so a producer selecting a form asks the
// same question the decoder answers and no consumer re-derives the matrix from an extension.
const _ifcPublished = (form: Ifc.Form, release: Ifc.Release): boolean =>
  Array.contains(_ifcRows[form.serialization].releases, release) &&
  Array.contains(_ifcContainerRows[form.container].wraps, form.serialization)

const _IfcWire = ArtifactFrame.wire(_IfcForm, Schema.Literal(..._ifcReleases)).pipe(
  Schema.filter((payload) =>
    _ifcPublished(payload.format, payload.schema) ||
    `<ifc-unpublished:${payload.format.container}:${payload.format.serialization}:${payload.schema}>`),
)

const Ifc: {
  readonly Wire: typeof _IfcWire
  readonly Form: typeof _IfcForm
  readonly serializations: typeof _ifcSerializations
  readonly containers: typeof _ifcContainers
  readonly releases: typeof _ifcReleases
  readonly row: typeof _ifcRows
  readonly container: typeof _ifcContainerRows
  readonly descriptor: typeof _ifcDescriptor
  readonly published: typeof _ifcPublished
  readonly schema: Schema.Schema<Ifc.Payload, Uint8Array>
} = {
  Wire: _IfcWire,
  Form: _IfcForm,
  serializations: _ifcSerializations,
  containers: _ifcContainers,
  releases: _ifcReleases,
  row: _ifcRows,
  container: _ifcContainerRows,
  descriptor: _ifcDescriptor,
  published: _ifcPublished,
  schema: Format.json.schema(_IfcWire),
}

declare namespace Ifc {
  type Serialization = (typeof _ifcSerializations)[number]
  type Container = (typeof _ifcContainers)[number]
  type Release = (typeof _ifcReleases)[number]
  type Sniff = (typeof _ifcSniffs)[number]
  type Form = Schema.Schema.Type<typeof _IfcForm>
  type Payload = Schema.Schema.Type<typeof _IfcWire>
  type Descriptor = { readonly extension: string; readonly header: string; readonly sniff: Sniff }
  type _Rows<
    T extends Record<Serialization, { readonly sniff: Sniff; readonly releases: Array.NonEmptyReadonlyArray<Release> }> = typeof _ifcRows,
  > = T
  type _Containers<
    T extends Record<Container, { readonly inflates: boolean; readonly wraps: Array.NonEmptyReadonlyArray<Serialization> }> = typeof _ifcContainerRows,
  > = T
}

const Frame = {
  Artifact: ArtifactFrame,
  Geometry: GeometryFrame,
  Residency,
  Ifc,
} as const

declare namespace Frame {
  type Band = ArtifactBand
  type Receipt = Artifact
  type Geometry = GeometryFrame
  type IfcWire = Ifc.Payload
  type IfcForm = Ifc.Form
  type IfcDescriptor = Ifc.Descriptor
  type ResidencyLedger = Residency.Ledger
  type ResidencyArrival = Residency.Arrival
  type ResidencyRow = Residency.Row
  type ResidencyKind = Residency.Kind
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Frame }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
