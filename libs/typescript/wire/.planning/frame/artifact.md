# [WIRE_ARTIFACT]

`frame/artifact.ts` reassembles `ArtifactFrameWire` multi-part payloads from `Rasm.Compute/Runtime` and is the folder's kernel-delegating mint site (invariant 2): frames carry the artifact's declared content key, ordinal position, and byte band; reassembly is an ordered keyed fold over the frame stream; and the assembled octets re-verify through the ONE `kernel/identity` mint — `contentKey`, LE→BE normalization inside the delegate — before any consumer sees them. A key mismatch is `parity` evidence with both keys held; a broken ordinal chain is `sequence` evidence; a verified artifact is a receipt plus its octets, delivered toward `browser/transport/pool` through `#vocab`. `[R2]` gates the mint going load-bearing: bit-parity against `CANONICAL_BYTE_IDENTITY` and `MATERIAL_LAYER_GOLDEN` proves the two-half fold before this rail carries production traffic.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                  |
| :-----: | :--------------- | :----------------------------------------------------------------------------- |
|   [1]   | `FRAME_PROTOCOL` | the frame shape, the ordered keyed reassembly fold, sequence evidence           |
|   [2]   | `KEY_VERIFY`     | the kernel-delegated verify, the artifact receipt, the assembled stream surface  |

## [2]-[FRAME_PROTOCOL]

- Owner: `Frame` — the decoded frame class: declared artifact key, dense ordinal, total count, held band; and `_gathered`, the keyed Mealy fold that threads per-artifact assembly state through the stream and emits a completed chunk set or typed sequence evidence.
- Entry: interior — the assembled `ArtifactFrame` owner in `[3]` is the module surface; `Frame` rides it as a static.
- Growth: a frame envelope axis (a compression flag, a priority lane) is one field mirroring the C# emit; the fold's shape never changes because completion is `ordinal + 1 === total` regardless of payload.
- Law: ordinals are dense per artifact and interleaving is legal across artifacts — the fold keys state by artifact key, so concurrent artifact streams reassemble independently; within one artifact a non-consecutive ordinal aborts that artifact with `sequence` evidence carrying both coordinates, and later frames of the aborted artifact fall through as fresh sequence evidence rather than resurrecting state.
- Law: bands are held verbatim — the fold accumulates the received `Uint8Array` views untouched; the join in `[3]` is the only allocation, and nothing recodes a band.
- Law: the lane cap precedes accumulation — a band larger than `_CAP.band` refuses as `overrun` before any state exists for it; the ceiling is a policy row, and the engine's own recursion bound covers the decode plane separately.
- Boundary: the frame decode rides `codec/proto.ts`; the quarantine divert composes on the consuming stream; residency-driven fetch decisions are `frame/residency.ts`'s protocol.

```typescript
import { ContentKey, contentKey } from "@rasm/ts/kernel"
import { Array, Chunk, Either, HashMap, Option, Schema } from "effect"
import { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

class Frame extends Schema.Class<Frame>("Frame")({
  artifact: ContentKey.FromCell,
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  total: Schema.Int.pipe(Schema.positive()),
  band: Schema.Uint8ArrayFromSelf,
}) {}

const _CAP = { band: 16777216 } as const

type _Held = { readonly expect: number; readonly total: number; readonly bands: Chunk.Chunk<Uint8Array> }
type _State = HashMap.HashMap<ContentKey, _Held>
type _Emit = Either.Either<Option.Option<{ readonly key: ContentKey; readonly bands: Chunk.Chunk<Uint8Array> }>, WireFault>

const _gathered = (state: _State, frame: Frame): readonly [_State, _Emit] =>
  frame.band.length > _CAP.band
    ? ([
        HashMap.remove(state, frame.artifact),
        Either.left(new WireFault({ family: "ArtifactFrameWire", reason: "overrun", detail: "<band-over-cap>", evidence: Option.some({ actual: frame.band.length, expected: _CAP.band }) })),
      ] as const)
    : Option.match(HashMap.get(state, frame.artifact), {
    onNone: () =>
      frame.ordinal === 0
        ? frame.total === 1
          ? ([state, Either.right(Option.some({ key: frame.artifact, bands: Chunk.of(frame.band) }))] as const)
          : ([HashMap.set(state, frame.artifact, { expect: 1, total: frame.total, bands: Chunk.of(frame.band) }), Either.right(Option.none())] as const)
        : ([
            state,
            Either.left(new WireFault({ family: "ArtifactFrameWire", reason: "sequence", detail: "<headless>", evidence: Option.some({ actual: frame.ordinal, expected: 0 }) })),
          ] as const),
    onSome: (held) =>
      frame.ordinal !== held.expect
        ? ([
            HashMap.remove(state, frame.artifact),
            Either.left(new WireFault({ family: "ArtifactFrameWire", reason: "sequence", detail: "<gap>", evidence: Option.some({ actual: frame.ordinal, expected: held.expect }) })),
          ] as const)
        : frame.ordinal + 1 === held.total
          ? ([HashMap.remove(state, frame.artifact), Either.right(Option.some({ key: frame.artifact, bands: Chunk.append(held.bands, frame.band) }))] as const)
          : ([HashMap.set(state, frame.artifact, { ...held, expect: held.expect + 1, bands: Chunk.append(held.bands, frame.band) }), Either.right(Option.none())] as const),
  })
```

## [3]-[KEY_VERIFY]

- Owner: `ArtifactFrame` — the assembled owner: the byte join, the kernel-delegated verify, and the one stream surface that turns a frame feed into verified artifacts; `Artifact` is the receipt class carrying key, extent, and frame count beside the assembled octets.
- Entry: `ArtifactFrame.reassembled(frames)` — `Stream<Frame> -> Stream<Either<[Artifact, octets], WireFault>>`: the fold gathers, completed chunk sets join once, the joined octets mint through the kernel `contentKey`, and the declared key either matches (a right `Artifact` receipt beside its verified octets) or refuses (`parity` left with both keys as evidence); `ArtifactFrame.frame` is the single-frame byte schema for point decode.
- Receipt: `Artifact` carries the verified coordinate — key, extent, frames — and the octets travel beside it; `browser/transport/pool` transfers the octets to its decode worker and the receipt to its residency ledger.
- Growth: a second verified consumer composes `reassembled` — the fold, join, and verify are one spelling; a per-consumer re-verify re-derives the mint and is the invariant-2 defect.
- Law: the mint is delegated, never local — `contentKey` is the kernel's one `XxHash128` seed-zero fold with the `:x32` spelling and LE→BE normalization inside, and branded keys compare by bare `===`; exactly three delegating sites exist branch-wide (here, the `browser/transport` worker, `store/object`), and this module is the wire's one.
- Law: the join is the page's marked kernel — one allocation at the summed extent, bands copied in ordinal order, the draft detaching immutably at the return; the implementer carries the `// BOUNDARY ADAPTER` mark on `_joined`'s first line per the statement-seam law.
- Law: verify is per-artifact, not per-frame — frames carry no per-frame hash; the artifact's declared key proves the whole, so a corrupted band surfaces exactly once at the join.
- Boundary: `[R2]` gates load-bearing use; the GLB-specific frame semantics ride `frame/geometry.ts` over this same fold; quarantine of parity-refused artifacts composes at the consumer with the octets this module already holds.

```typescript
import { Effect, Stream } from "effect"

class Artifact extends Schema.Class<Artifact>("Artifact")({
  key: ContentKey.FromCell,
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

const _verified = (key: ContentKey, bands: Chunk.Chunk<Uint8Array>): Effect.Effect<readonly [Artifact, Uint8Array], WireFault> =>
  Effect.gen(function* () {
    const octets = _joined(bands)
    const minted = yield* contentKey(octets)
    return minted === key
      ? ([new Artifact({ key, extent: octets.length, frames: Chunk.size(bands) }), octets] as const)
      : yield* new WireFault({
          family: "ArtifactFrameWire",
          reason: "parity",
          detail: "<artifact-key-mismatch>",
          evidence: Option.some({ actual: minted, expected: key }),
        })
  })

const ArtifactFrame: {
  readonly Frame: typeof Frame
  readonly Artifact: typeof Artifact
  readonly frame: Schema.Schema<Frame, Uint8Array>
  readonly reassembled: <E, R>(
    frames: Stream.Stream<Frame, E, R>,
  ) => Stream.Stream<Either.Either<readonly [Artifact, Uint8Array], WireFault>, E, R>
} = {
  Frame,
  Artifact,
  frame: ProtoCodec.family(ProtoCodec.suite.ArtifactFrameWire, Frame),
  reassembled: (frames) =>
    frames.pipe(
      Stream.mapAccum(HashMap.empty<ContentKey, _Held>(), _gathered),
      Stream.filterMap((emit) =>
        Either.match(emit, {
          onLeft: (fault) => Option.some(Effect.succeed(Either.left(fault))),
          onRight: (held) => Option.map(held, (ready) => Effect.either(_verified(ready.key, ready.bands))),
        }),
      ),
      Stream.mapEffect((settle) => settle, { concurrency: 1 }),
    ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ArtifactFrame }
```
