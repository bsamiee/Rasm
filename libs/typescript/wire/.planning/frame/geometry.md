# [WIRE_GEOMETRY]

`frame/geometry.ts` is the GLB rail: `GeometryPayloadWire` frames from `Rasm.Compute/Runtime` carry tessellated geometry — the GLB band TS consumes and never authors (invariant 7) — plus `MeshTensor` views, the typed-array windows over the payload bytes a viewer uploads zero-copy. The payload rides the artifact reassembly-and-verify fold whole; this module adds the geometry-plane semantics: the payload envelope (which mesh, which LOD, which encoding), the tensor view vocabulary (dtype, shape, strides, offset), and the transferable handoff toward `ui/viewer` `scene/glb` and the `browser/transport` decode worker.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                     |
| :-----: | :------------ | :-------------------------------------------------------------------------------- |
|   [1]   | `GLB_PAYLOAD` | the payload envelope over the artifact rail; encoding vocabulary; the framed feed   |
|   [2]   | `MESH_TENSOR` | the tensor view rows and the zero-copy view construction over verified octets       |

## [2]-[GLB_PAYLOAD]

- Owner: `GeometryFrame` — the payload envelope class: mesh content key, LOD ordinal, the closed `encoding` vocabulary row (`glb`, `draco`, `meshopt`), the tensor row set, and the artifact coordinate binding the envelope to its verified octets.
- Entry: `GeometryFrame.payload` the envelope byte schema; `GeometryFrame.stream(frames)` — the framed feed: `ProtoCodec.stream` walks the length-prefixed envelopes and each admits through the envelope schema; the OCTETS travel the artifact rail separately, joined to the envelope by artifact key at the consumer.
- Receipt: the envelope names what the verified octets ARE — mesh key, LOD, encoding, tensor windows; the viewer's residency ledger keys on `mesh` and the scene binds buffers through the tensor rows.
- Growth: a new encoding is one literal row — `meshopt` is present because the C# IfcConvert artifact MAY emit `EXT_meshopt_compression` (`[R23]`); the decoder admission that would follow is viewer-local and never lands here. A new tensor semantic is one `_semantics` row.
- Law: the GLB band is consume-only carriage — TS opens no glTF parser in `wire`; the band travels verbatim from artifact verify to the viewer's loader, and this module's knowledge of it ends at the encoding row.
- Law: the envelope decodes independently of the octets — envelopes are small control-plane frames, octets are bulk artifact frames; the two planes join by content key, so envelope loss never strands verified bytes and byte loss never blocks envelope planning.
- Boundary: reassembly and key verify are `frame/artifact.ts`'s fold; residency asks/acks are `frame/residency.ts`; GLB parsing, draco/meshopt decode, and GPU upload are `ui/viewer` `scene/glb` and the `browser/transport` worker.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { type ParseResult, Schema, Stream } from "effect"
import type { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

const _encodings = ["glb", "draco", "meshopt"] as const
const _dtypes = ["f32", "u32", "u16", "u8"] as const
const _semantics = ["position", "normal", "uv", "index", "color"] as const

const _Tensor = Schema.Struct({
  semantic: Schema.Literal(..._semantics),
  dtype: Schema.Literal(..._dtypes),
  shape: Schema.NonEmptyArray(Schema.Int.pipe(Schema.positive())),
  byteOffset: Schema.Int.pipe(Schema.nonNegative()),
  byteStride: Schema.Int.pipe(Schema.nonNegative()),
})

class GeometryFrame extends Schema.Class<GeometryFrame>("GeometryFrame")({
  mesh: ContentKey.FromCell,
  artifact: ContentKey.FromCell,
  lod: Schema.Int.pipe(Schema.nonNegative()),
  encoding: Schema.Literal(..._encodings),
  tensors: Schema.Array(_Tensor),
}) {
  static readonly payload: Schema.Schema<GeometryFrame, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.GeometryPayloadWire, GeometryFrame)
  static readonly stream = (frames: AsyncIterable<Uint8Array>): Stream.Stream<GeometryFrame, WireFault | ParseResult.ParseError> =>
    ProtoCodec.stream(ProtoCodec.suite.GeometryPayloadWire, "GeometryPayloadWire")(frames).pipe(
      Stream.mapEffect(Schema.decodeUnknown(GeometryFrame), { concurrency: 1 }),
    )
}

declare namespace GeometryFrame {
  type Encoding = (typeof _encodings)[number]
  type Dtype = (typeof _dtypes)[number]
  type Tensor = Schema.Schema.Type<typeof _Tensor>
}
```

## [3]-[MESH_TENSOR]

- Owner: the tensor view construction — the dtype-to-constructor vocabulary row and `view`, the zero-copy window over verified artifact octets a tensor row addresses.
- Entry: `GeometryFrame.view(octets, tensor)` — one typed-array window per tensor row: constructor selected by the dtype vocabulary, positioned by `byteOffset`, sized by the shape product; the returned view aliases the verified buffer deliberately, and the caller transfers the underlying buffer to the worker in the same expression chain.
- Growth: a new dtype is one `_views` vocabulary row — constructor and byte width land as data; consumers never switch on dtype.
- Law: views alias, transfers detach — the view is a window, not a copy; the moment the buffer transfers to the decode worker the view is dead on this side, so views are constructed where they are consumed, never stored in cells; the aliasing discipline is the card's law and the transfer list is the marshal declaration's (`browser`'s worker protocol).
- Law: bounds are proven before construction — offset plus extent must fit the buffer; an overrun is a `parity`-grade evidence failure of the envelope against its octets, minted by the caller holding both extents, never an `!` repair.
- Boundary: worker marshal and `Transferable` collection are `browser/transport`'s protocol declarations; GPU buffer upload is the viewer's.

```typescript
import { Array } from "effect"

const _views = {
  f32: { of: Float32Array, width: 4 },
  u32: { of: Uint32Array, width: 4 },
  u16: { of: Uint16Array, width: 2 },
  u8: { of: Uint8Array, width: 1 },
} as const

const _extent = (tensor: GeometryFrame.Tensor): number =>
  Array.reduce(tensor.shape, 1, (total, dim) => total * dim)

const _view = (octets: Uint8Array, tensor: GeometryFrame.Tensor): Float32Array | Uint32Array | Uint16Array | Uint8Array =>
  new _views[tensor.dtype].of(octets.buffer, octets.byteOffset + tensor.byteOffset, _extent(tensor))

// --- [EXPORTS] --------------------------------------------------------------------------

export { GeometryFrame }
```

`view` and `extent` ride the class as `static readonly view = _view` and `static readonly extent = _extent` in the single `GeometryFrame` declaration; the module's one export carries envelope, feed, and window construction together.
