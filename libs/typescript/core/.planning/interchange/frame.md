# [CORE_FRAME]

`Frame` owns bounded artifact reassembly, verified geometry rendezvous, schema-pinned residency admission, and IFC container admission. Interleaved bands fold by artifact and generation under one ingress budget, verification gates the joined allocation, tensor views prove span, stride, and alignment, and each residency manifest replaces whole against the producer's pinned cluster roster and the budget it declares. Module `core/src/interchange/frame.ts` admits an arrival class as one refusal row, a tensor element type as one view row, a residency payload as one kind row, and an IFC serialization as one admission row.

`Frame` composes the `value` floor's `Digest` identity and `Shape.Ingress` ceilings, the `codec` owner's fault, gap, parity, and quarantine rails, and the `format` owner's proto suite and JSON schema mints. Producers own every payload axis crossing this plane, so `Frame` folds arrivals into receipts, views, ledgers, and admissions and mints no payload axis of its own.

## [01]-[INDEX]

- [02]-[FRAME_PROTOCOL]: bounded keyed frame assembly and sequence evidence; `Frame.Artifact`.
- [03]-[KEY_VERIFY]: delegated verification and single-allocation joins; `Frame.Artifact`.
- [04]-[GEOMETRY_PLANE]: geometry envelopes, tensor views, and rendezvous; `Frame.Geometry`.
- [05]-[RESIDENCY_MANIFEST]: schema-pinned viewport manifest admission, per-kind census, and budget grading; `Frame.Residency`.
- [06]-[IFC_ADMISSION]: serialization rows and their two direction verdicts, the sparse container and release crossings, the release-header read; `Frame.Ifc`.

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

// The axis is a roster member of the codec's own overrun vocabulary, so every ceiling this page refuses under names
// itself in a column rather than in a token, and the artifact coordinate rides the branch's absence carrier — the
// tensor legs genuinely hold none, where every assembly leg does.
const _overrun = (
  family: Wire.FaultFamily,
  axis: Wire.OverrunAxis,
  actual: number,
  expected: number,
  at: Option.Option<{ readonly artifact: Digest.Key<"content">; readonly generation: number }>,
): Wire.Fault => new Wire.Fault({ family, case: { reason: "overrun", axis, actual, expected, at } })

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
      fault: (frame) => Wire.Gap.evidence("ArtifactFrame", "ordinal", 0n, BigInt(frame.ordinal)),
    },
    {
      when: (frame) => frame.band.length > budget.bytes,
      fault: (frame) => _overrun("ArtifactFrame", "assembly", frame.band.length, budget.bytes, Option.some(frame)),
    },
  ]
  const holding: ReadonlyArray<_HeldRefusal> = [
    {
      when: (frame, held) => frame.ordinal !== held.expect,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactFrame", "ordinal", BigInt(held.expect), BigInt(frame.ordinal)),
    },
    {
      when: (frame, held) => frame.total !== held.total,
      fault: (frame, held) => Wire.Gap.evidence("ArtifactFrame", "total", BigInt(held.total), BigInt(frame.total)),
    },
    {
      when: (frame, held) => held.extent + frame.band.length > budget.bytes,
      fault: (frame, held) =>
        _overrun("ArtifactFrame", "assembly", held.extent + frame.band.length, budget.bytes, Option.some(frame)),
    },
  ]
  return (state: _State, frame: ArtifactBand): readonly [_State, _Emit] =>
    state.seen >= budget.frames
      ? ([_next(state, state.held), Either.left(_overrun("ArtifactFrame", "frames", state.seen + 1, budget.frames,
          Option.some(frame)))] as const)
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
    Either.left(Wire.Gap.evidence("ArtifactFrame", "tail", BigInt(held.total), BigInt(held.expect))))

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
    new Wire.Fault({ family, case: { reason: "malformed", at: "source", issue: String(defect) } })).pipe(
    Stream.mapEffect((octets) =>
      Schema.decodeUnknown(schema)(octets).pipe(
        Effect.mapError((issue) =>
          new Wire.Fault({ family, case: { reason: "malformed", at: "decode", issue: issue.message } })),
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
          _overrun("GeometryPayload", "tensor-extent",
            Math.max(octets.byteLength, _span(tensor)), Shape.Ingress.floor.bytes, Option.none()),
        )
      : tensor.byteStride !== 0 && tensor.byteStride < _rowBytes(tensor)
      ? Either.left(
          _overrun("GeometryPayload", "tensor-stride", tensor.byteStride, _rowBytes(tensor), Option.none()),
        )
      : tensor.byteOffset + _span(tensor) > octets.byteLength
      ? Either.left(
          _overrun("GeometryPayload", "tensor-span", tensor.byteOffset + _span(tensor), octets.byteLength, Option.none()),
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
          ? [held, Option.some(Either.left(_overrun("GeometryPayload", "rendezvous", HashMap.size(held) + 1,
              budget.collection, Option.some({ artifact: receipt.key, generation: receipt.generation }))))] as const
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
      // the side and the coordinate were fused into one token, so the fact a reader wants first — WHICH half never
      // arrived — could only be recovered by matching a prefix on prose
      case: { reason: "truncated", side: Either.isLeft(lane) ? "envelope" : "artifact", coordinate },
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

## [05]-[RESIDENCY_MANIFEST]

- Owner: `Frame.Residency` admits the producer's viewport residency manifest and grades it against the pinned schema and the budget it declares.
- Law: the manifest REPLACES — the producer mints the whole resident tile set for one viewpoint on every emission, so no held state exists for an arrival to patch and this crossing carries no delta arm.
- Law: the schema pin grades the CLUSTER ROSTER as much as the envelope, so a version off the pin refuses before any tile column is read.
- Law: refusal direction grades the arrival — a version below the pin reads `stale`, one above reads `conflict`, both carrying the version pair.
- Law: duplicate content keys refuse, and the collection ceiling is `Shape.Ingress`.
- Law: `kind` decides cull and draw posture, and the same rows carry the per-kind census the declared VRAM budget is judged against.

```typescript signature
// The producer's schema pin, carried as a VALUE because it grades every arrival: the pin counts the cluster roster
// as much as the envelope, so a decoder reading a column set one row short of the producer's stops at the wrong
// offset on every cluster past the first and every figure derived below it is fiction. Four is the roster carrying
// the producer's `parent`, `parentError`, and `cut` columns.
const _RESIDENCY_SCHEMA = 4

// Producers cross their payload axis verbatim, and its two consumer columns cross with it: `coneCullable` names the
// tiles a cluster cull may reject before upload, `splatBorne` the tiles a raster shader cannot draw. A consumer told
// only a content key and a bounding sphere has to infer both from the bytes it has not fetched yet.
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

// Both rosters cross as the producer's own uppercase tokens, because they name the meshopt codec's encoding and
// filter modes rather than a vocabulary either end mints — a lowercased mirror here would be a second spelling of
// one decoder's contract.
const _modes = ["ATTRIBUTES", "TRIANGLES", "INDICES", "RAW"] as const
const _filters = ["NONE", "OCTAHEDRAL", "QUATERNION", "EXPONENTIAL"] as const

const _Triple = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

// Absence OMITS on this crossing and never crosses null: the producing shell serializes under one merged options
// identity whose carrier modifier DROPS the member outright, so `optional` is the only spelling that decodes what
// the emission writes and a `NullOr` here would declare a token that emission cannot produce.
const _Camera = Schema.Struct({
  projection: Schema.Literal("perspective", "orthographic"),
  eye: _Triple,
  target: _Triple,
  up: _Triple,
  scale: Schema.Number,
})

const _Measurement = Schema.Struct({
  key: Schema.NonEmptyString,
  vertices: Schema.Array(Schema.Struct({
    sourceKey: Schema.NonEmptyString,
    sampleIndex: Shape.Refined.OrdinalKey,
    position: _Triple,
  })),
  totalMeters: Schema.Number,
  anglesDegrees: Schema.Array(Schema.Number),
})

const _Viewpoint = Schema.Struct({
  key: Schema.NonEmptyString,
  version: Shape.Refined.OrdinalKey,
  camera: _Camera,
  section: Schema.optional(Schema.Struct({ min: _Triple, max: _Triple })),
  overrides: Schema.Array(Schema.Struct({
    elementId: Schema.NonEmptyString,
    visible: Schema.Boolean,
    colorArgb: Schema.optional(Shape.Refined.OrdinalKey),
    transparency: Schema.Number,
  })),
  selection: Schema.Array(Schema.NonEmptyString),
  measurements: Schema.Array(_Measurement),
  at: Schema.DateTimeUtc,
})

const _Stream = Schema.Struct({
  stream: Schema.NonEmptyString,
  mode: Schema.Literal(..._modes),
  filter: Schema.Literal(..._filters),
  byteOffset: Shape.Refined.OrdinalKey,
  byteLength: Shape.Refined.OrdinalKey,
  count: Shape.Refined.OrdinalKey,
  byteStride: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  codecVersion: Shape.Refined.OrdinalKey,
})

// `parent` and `parentError` are ABSENT at the LOD subtree root and at its terminus rather than sentinel-valued, so
// a cut walk reads absence as the boundary the producer meant instead of chasing a fabricated index; `cut` and
// `curvature` are the producer's own realized figures, carried so this head's footprint derivation reads the same
// measured bound the producing integrator's ray cone did and the two runtimes cannot disagree on a texture level.
const _Meshlet = Schema.Struct({
  vertexOffset: Shape.Refined.OrdinalKey,
  triangleOffset: Shape.Refined.OrdinalKey,
  vertexCount: Shape.Refined.OrdinalKey,
  triangleCount: Shape.Refined.OrdinalKey,
  center: _Triple,
  radius: Schema.Number,
  coneApex: _Triple,
  coneAxis: _Triple,
  coneCutoff: Schema.Number,
  level: Shape.Refined.OrdinalKey,
  parent: Schema.optional(Shape.Refined.OrdinalKey),
  shell: Shape.Refined.OrdinalKey,
  error: Schema.Number,
  parentError: Schema.optional(Schema.Number),
  curvature: Schema.Number,
  cut: Shape.Refined.OrdinalKey,
})

// Bounds are the producer's packed `[x, y, z, radius]` sphere, so the TUPLE states the arity a bare array spelling
// leaves open — a three-column read drops the radius silently and culls against a point.
const _Tile = Schema.Struct({
  kind: Schema.Literal(..._kinds),
  contentKey: Digest.codecs.content.wire,
  blobKey: Schema.NonEmptyString,
  bytes: Shape.Refined.OrdinalKey,
  residentCount: Shape.Refined.OrdinalKey,
  harmonicDegree: Shape.Refined.OrdinalKey,
  bounds: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number, Schema.Number),
  streams: Schema.Array(_Stream),
  meshlets: Schema.Array(_Meshlet),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  version: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  viewpoint: _Viewpoint,
  tiles: Schema.Array(_Tile).pipe(
    Schema.filter((tiles) => tiles.length <= Shape.Ingress.floor.collection || "<residency-collection>"),
    Schema.filter((tiles) =>
      Array.dedupe(Array.map(tiles, (tile) => tile.contentKey)).length === tiles.length || "<duplicate-residency-key>"),
  ),
  vramBudget: Shape.Refined.OrdinalKey,
}) {}

declare namespace Residency {
  type Kind = (typeof _kinds)[number]
  type Mode = (typeof _modes)[number]
  type Filter = (typeof _filters)[number]
  type Viewpoint = Schema.Schema.Type<typeof _Viewpoint>
  type Tile = Schema.Schema.Type<typeof _Tile>
  type Meshlet = Schema.Schema.Type<typeof _Meshlet>
  type StreamRow = Schema.Schema.Type<typeof _Stream>
  type Tally = { readonly count: number; readonly bytes: number; readonly meshlets: number }
  type Census = { readonly [K in Kind]: Tally }
  type View = { readonly manifest: Manifest; readonly resident: number; readonly census: Census }
  type _Kinds<T extends Record<Kind, { readonly coneCullable: boolean; readonly splatBorne: boolean }> = typeof _kindRows> = T
}

const _EMPTY_TALLY: Residency.Tally = { count: 0, bytes: 0, meshlets: 0 }

// Spelled per kind rather than mapped off the roster because the CENSUS TYPE closes the table against `_kinds` in
// both directions: a fifth payload axis fails at this declaration, where a generic fold would silently seed a census
// row nothing counts.
const _EMPTY_CENSUS: Residency.Census = {
  "meshlet-cluster": _EMPTY_TALLY,
  "quantized-vertex": _EMPTY_TALLY,
  "point-splat": _EMPTY_TALLY,
  "gaussian-splat": _EMPTY_TALLY,
}

// Direction names the refusal exactly as every other arrival on this page grades one: a manifest BELOW the pin is
// superseded and reads `stale`, one ABOVE carries columns this decoder has never seen and reads `conflict`. Both are
// one defect — a cluster roster read at the wrong offset — told apart by which end moved, and the version pair rides
// the evidence a board drills on.
const _schemaFault = (manifest: Manifest): Wire.Fault =>
  new Wire.Fault({
    family: "GeometryResidencyWire",
    // one comparison elects the reason and the SAME pair rides both arms, so the two verdicts cannot disagree about
    // which end moved — the retired form re-spelled that comparison a second time to pick a prose token
    case: manifest.version < _RESIDENCY_SCHEMA
      ? { reason: "stale", pinned: _RESIDENCY_SCHEMA, arrived: manifest.version }
      : { reason: "conflict", pinned: _RESIDENCY_SCHEMA, arrived: manifest.version },
  })

// The census and the resident total are ONE fold: a second pass to sum what the per-kind tally already accumulated is
// the parallel model this owner forecloses, and the budget guard needs both figures at the same moment anyway. Each
// accumulation crosses the safe-integer guard because tile extents are producer byte totals with no declared ceiling.
const _tallied = (manifest: Manifest): Either.Either<Residency.View, Wire.Fault> =>
  Array.reduce(
    manifest.tiles,
    Either.right<Residency.View, Wire.Fault>({ manifest, resident: 0, census: _EMPTY_CENSUS }),
    (held, tile) =>
      Either.flatMap(held, (view) => {
        const tally = view.census[tile.kind]
        return Option.match(
          Option.all([
            _sum(view.resident, tile.bytes),
            _sum(tally.bytes, tile.bytes),
            _sum(tally.meshlets, tile.meshlets.length),
          ]),
          {
            onNone: () =>
              Either.left(_overrun("GeometryResidencyWire", "<residency-census>", tally.bytes, Number.MAX_SAFE_INTEGER, {
                key: tile.contentKey,
                generation: manifest.version,
              })),
            onSome: ([resident, bytes, meshlets]) =>
              Either.right({
                manifest,
                resident,
                census: { ...view.census, [tile.kind]: { count: tally.count + 1, bytes, meshlets } },
              }),
          },
        )
      }),
  )

// Schema first, then the claim: a roster read at the wrong offset makes every byte figure below it fiction, and a
// manifest whose resident set overruns the budget it DECLARES evicted nothing — the producer refuses that plan as a
// budget fault before it mints, so an arrival carrying one crossed a producer that did not run its own gate.
const _admitted = (manifest: Manifest): Either.Either<Residency.View, Wire.Fault> =>
  manifest.version !== _RESIDENCY_SCHEMA
    ? Either.left(_schemaFault(manifest))
    : Either.flatMap(_tallied(manifest), (view) =>
        view.resident > manifest.vramBudget
          ? Either.left(_overrun("GeometryResidencyWire", "<residency-vram-budget>", view.resident, manifest.vramBudget))
          : Either.right(view))

// ONE entry over both arities, discriminating on the value's own shape: a manifest replaces whole, so the stream arm
// is a MAP where the prior ledger needed an accumulator, and a caller holding a single decoded arrival reaches the
// same grading without assembling a one-element stream.
function _admit(manifest: Manifest): Either.Either<Residency.View, Wire.Fault>
function _admit<E, R>(arrivals: Stream.Stream<Manifest, E, R>): Stream.Stream<Either.Either<Residency.View, Wire.Fault>, E, R>
function _admit<E, R>(
  input: Manifest | Stream.Stream<Manifest, E, R>,
): Either.Either<Residency.View, Wire.Fault> | Stream.Stream<Either.Either<Residency.View, Wire.Fault>, E, R> {
  return input instanceof Manifest ? _admitted(input) : Stream.map(input, _admitted)
}

const Residency: {
  readonly Manifest: typeof Manifest
  readonly schema: typeof _RESIDENCY_SCHEMA
  readonly kinds: typeof _kinds
  readonly kind: typeof _kindRows
  readonly envelope: Schema.Schema<Manifest, Uint8Array>
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<Either.Either<Manifest, Wire.Fault>, Wire.Fault, Wire.Quarantine>
  readonly admit: typeof _admit
  readonly cullable: (view: Residency.View) => ReadonlyArray<Residency.Tile>
  readonly splatBorne: (view: Residency.View) => ReadonlyArray<Residency.Tile>
} = {
  Manifest,
  schema: _RESIDENCY_SCHEMA,
  kinds: _kinds,
  kind: _kindRows,
  // The residency manifest arrives as the producing shell's source-generated JSON mint, so it rides the json arm
  // while both Compute-minted frame families above keep the proto walk their own descriptor source declares
  envelope: Format.json.schema(Manifest),
  stream: (frames) => _payloadStream("GeometryResidencyWire", Format.json.schema(Manifest), frames),
  admit: _admit,
  // The two kind columns get their readers HERE rather than at each consumer: a scheduler asking which tiles a
  // cluster cull may reject and a renderer asking which the raster path cannot draw are two reads of one table, and
  // a consumer re-deriving either from the kind literal has forked the roster.
  cullable: (view) => Array.filter(view.manifest.tiles, (tile) => _kindRows[tile.kind].coneCullable),
  splatBorne: (view) => Array.filter(view.manifest.tiles, (tile) => _kindRows[tile.kind].splatBorne),
}

```

## [06]-[IFC_ADMISSION]

- Owner: `Frame.Ifc` owns the IFC wire form — serialization rows, the container column, and the release roster both cross sparsely.
- Law: Serialization and container stay separate axes whose product is SPARSE on both crossings — a wrapper names the serializations it carries and never the text inside them, so `zip` admits STEP and XML alone, and each serialization names the releases it publishes for, so `Ifc.admits` refuses an unpublished pair by row where a cross product admits a document no schema validates.
- Law: ADMISSION and PUBLICATION are two verdicts over one row set — `Ifc.admits` grades the bytes this decoder reads, `Ifc.seals` grades what a producer may write, and one fused predicate either forfeits a lawful read or authorizes a form no producer of this estate mints.
- Law: Direction rides the `refusal` cell, and the cell QUOTES its producer rather than re-deriving it — `libs/csharp/Rasm.Bim/.planning/Projection/wireform.md` `IfcSerialization.Refusal` names `ifcx` unproduced, its `Published` reads that cell first and fails whatever release follows, and its `Sniff` still admits the span.
- Law: `IFC4X3` publishes under no serialization — the ISO-approved 4.3 line carries the `IFC4X3_ADD2` identifier and every published 4.3 artifact spells it, so the roster keeps the token to NAME that refusal rather than failing an unknown literal at decode.
- Law: `ifcx` is IFC5's own encoding rather than a `json` release — a document there carries its release at `header.ifcxVersion` where ifcJSON reads `schemaIdentifier`, so folding IFC5 onto the JSON row forks two vocabularies into one member read; that `json` row itself publishes against a community-maintained schema where `step` and `xml` publish against the ISO editions, a weaker claim its consumers price at the seam.
- Law: `ifcx` reaches this branch from a foreign producer alone — the estate's own producer maps IFC5 to no writer, so the row admits on the read side and refuses on the seal side, stated as a cell on both ends rather than a shape one end silently lacks.
- Law: Each serialization declares the header member carrying its release, so admission reads a named member rather than guessing.
- Law: `sniff` prices the release read, and an inflating container raises its serialization's extent to the whole document.
- Law: Rows decide selection, admission, publication, and `degrade` alone — a wire form realizes no tenancy and ends no lifetime.
- Growth: a serialization is one `_ifcRows` row, a container one `_ifcContainerRows` row, a release one roster entry beside the row cells that publish it; a producer refusal is one `refusal` cell, never an absent row.
- Boundary: `Frame` carries the wire form and its descriptor; entity-graph authoring and release raising stay with the producing runtime.
- Packages: `effect` (`Array`, `Either`, `Option`, `Schema`); `./format.ts` (`Format`).

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
//
// `refusal` is the DIRECTION cell the span cannot carry: absence means the estate's producer of record authors the
// row, and a present detail is that producer's own token quoted verbatim. The two gates below diverge on this single
// column rather than on a second table, so a serialization cannot land a span without stating which way it crosses.
const _ifcRows = {
  step: {
    extension: ".ifc",
    header: "FILE_SCHEMA",
    sniff: "line",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-header-line>",
  },
  xml: {
    extension: ".ifcxml",
    header: "xmlns",
    sniff: "element",
    releases: ["IFC2X3", "IFC4", "IFC4X1", "IFC4X3_ADD2"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-root-element>",
  },
  json: {
    extension: ".ifcjson",
    header: "schemaIdentifier",
    sniff: "document",
    releases: ["IFC4"],
    refusal: Option.none<string>(),
    degrade: "<release-unknown-before-whole-document>",
  },
  // Read-only against this estate, and the cell SAYS so: `ifc-form-unproduced` is the producer's own word, so a
  // caller asking to seal here reads the token that producer's gate raises instead of inferring a no from a row
  // this end never authored. The span stays IFC5 because the decode side is real — the direction is what differs.
  ifcx: {
    extension: ".ifcx",
    header: "header.ifcxVersion",
    sniff: "document",
    releases: ["IFC5"],
    refusal: Option.some("ifc-form-unproduced"),
    degrade: "<release-carried-as-ifcx-version>",
  },
} as const satisfies {
  readonly [S in Ifc.Serialization]: {
    readonly extension: string
    readonly header: string
    readonly sniff: Ifc.Sniff
    readonly releases: Array.NonEmptyReadonlyArray<Ifc.Release>
    readonly refusal: Option.Option<string>
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
// same question the decoder answers and no consumer re-derives the matrix from an extension. Each crossing refuses
// under its OWN token — a fused message left a caller unable to tell a wrapper nothing defines from a release no
// schema validates — and all three tokens are the producer's own words, so one diagnostic vocabulary spans the seam.
const _ifcAdmits = (form: Ifc.Form, release: Ifc.Release): Either.Either<Ifc.Form, string> =>
  !Array.contains(_ifcContainerRows[form.container].wraps, form.serialization)
    ? Either.left(`<ifc-form-uncontained:${form.container}:${form.serialization}>`)
    : Array.contains(_ifcRows[form.serialization].releases, release)
    ? Either.right(form)
    : Either.left(`<ifc-form-unpublished:${form.container}:${form.serialization}:${release}>`)

// Publication reads the `refusal` cell FIRST and fails whatever release follows, exactly as the producer's own
// `Published` does; the span check behind it IS the admit gate, so publication costs one column over admission
// rather than a second matrix walk. Spending the decode gate here would authorize the one pair this branch's own
// peer names unproduced, and that divergence would land at the peer's seam instead of at this selection.
const _ifcSeals = (form: Ifc.Form, release: Ifc.Release): Either.Either<Ifc.Form, string> =>
  Option.match(_ifcRows[form.serialization].refusal, {
    onSome: (detail) => Either.left(`<${detail}:${form.container}:${form.serialization}:${release}>`),
    onNone: () => _ifcAdmits(form, release),
  })

const _IfcWire = ArtifactFrame.wire(_IfcForm, Schema.Literal(..._ifcReleases)).pipe(
  // Decode spends the ADMIT gate alone: a payload reaching this schema was already written by whoever wrote it, and
  // refusing an `ifcx` document here would delete the read capability this row exists to carry. A producer spends
  // `Ifc.seals` before it mints, so the seal verdict never rides an arriving artifact's filter.
  Schema.filter((payload) =>
    Either.match(_ifcAdmits(payload.format, payload.schema), { onLeft: (detail) => detail, onRight: () => true })),
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
  readonly admits: typeof _ifcAdmits
  readonly seals: typeof _ifcSeals
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
  // Both directions publish as members of ONE surface: a `direction` argument would be the mode knob, and a caller
  // holding only the decode verdict is exactly the producer that seals what its peer refuses.
  admits: _ifcAdmits,
  seals: _ifcSeals,
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
    T extends Record<
      Serialization,
      { readonly sniff: Sniff; readonly releases: Array.NonEmptyReadonlyArray<Release>; readonly refusal: Option.Option<string> }
    > = typeof _ifcRows,
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
  type ResidencyManifest = Manifest
  type ResidencyView = Residency.View
  type ResidencyTile = Residency.Tile
  type ResidencyViewpoint = Residency.Viewpoint
  type ResidencyMeshlet = Residency.Meshlet
  type ResidencyCensus = Residency.Census
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
