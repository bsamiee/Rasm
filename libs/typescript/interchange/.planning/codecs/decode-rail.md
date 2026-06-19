# [INTERCHANGE_DECODE_RAIL]

The byte-to-typed decode interior of the wire boundary: one codec-keyed dispatch table owning every byte format and both directions as rows. `DecodeRail` is the densest owner of the wire interior — one `CODEC` vocabulary keys the proto, messagepack, json-stj, and embedded-geometry rows to their decode and encode operations, never a parallel rail per concern and never a per-instance interface re-described at each call site. The recorded-intent partial-update sibling is `codecs/patch-rail.md` `PatchRail`: `DecodeRail` admits a whole value, `PatchRail` admits a recorded mutation against an already-admitted value, the two co-located in `codecs/` as the whole-value and partial-update faces of one decode discipline. The `messagepack` row owns the Persistence sync surface in full: the snapshot frame, the sync-segment stream, and the `crdt` column-family op delta — the `CrdtOpWire` MessagePack discriminated union owned at `csharp:Rasm.Persistence/versioning/version-control#CRDT_WIRE` is decoded here as the `OpLogEntryWire.payload` carries for `columnFamily="crdt"` rows, never re-minted as a parallel envelope. The artifact-frame reassembly (`artifacts/frame-reassembly.md`), the fault reconstruction (`faults/fault-family.md`), and the brand-and-filter refinement vocabulary (`refinement/schema-refinement.md`) compose this same decode discipline from their own pages over the bytes these codec rows admit; the suite-anchor transcription law is owned once by `contracts/wire-inventory.md`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                |
| :-----: | :------------------- | :------------------------------------------------------------------- |
|   [2]   | DECODE_RAIL          | the codec dispatch vocabulary, both directions, the geometry row     |
|   [3]   | GEO_WIRE_DECODE      | the `GeoJsonWire` seven-type RFC 7946 union, the `Position` floor, the WGS84 bound, the absent-CRS guard |
|   [4]   | SEGMENT_STREAM_DECODE | the backpressured `decodeSegmentStream` async-array fold and the `SNAPSHOT_CODEC` decompression delegate-row |
|   [5]   | CRDT_OP_DECODE       | the `CrdtOpWire` ten-arm union over the crdt column-family payload    |
|   [6]   | TS_PROJECTION        | the snapshot, sync, and receipt wire shapes the rails decode         |
|   [7]   | BCF_LIVE_WIRE_DECODE | the Bim BCF topic/viewpoint and AppHost live-wire binding json-stj rows |

## [2]-[DECODE_RAIL]

- Owner: `DecodeRail`, one `CODEC` vocabulary keyed by `CodecKey` whose rows carry their own byte-format behavior — the proto row decodes unary responses and server-streams, the messagepack row decodes snapshot frames, the sync-segment stream, and the `crdt` column-family op delta through the `[5]-[CRDT_OP_DECODE]` `CrdtOpWire` union, the json-stj row is `Schema.Class` transcription of the receipt fences, and the geometry row decodes embedded GeoJSON fields. The fold reads a row and returns its `decode`/`encode` pair; a `Match` chain restating the codec→operation knowledge inline is the rejected form, the keyed domain dispatching through the vocabulary lookup the `CODEC` table already maps.
- Cases: the proto row reads `csharp:Rasm.Compute/remote/remote#TS_PROJECTION` and `csharp:Rasm.Compute/progress/progress#TS_PROJECTION` through `fromBinary` against the generated descriptor with `Stream.fromAsyncIterable` giving the server-stream pull-based backpressure the framing fold shares; the messagepack row reads `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION` and `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` through one reused `Decoder` mapping 64-bit integers to bigint and the fixed-width header through a `DataView` `getBigUint64(offset, false)` big-endian read (the C# `BinaryPrimitives.WriteUInt64BigEndian` persistence order — the `littleEndian: false` flag is load-bearing, a swap folds the schema fingerprint to a foreign value), the sync-segment `ReadableStream` through the `[4]-[SEGMENT_STREAM_DECODE]` `decodeSegmentStream` fold lifting `Decoder.decodeArrayStream` into a backpressured `Stream<OpLogEntryWire, ParseError>` over the one-frame `Stream.buffer` window the transport framing fold carries, and the `SNAPSHOT_CODEC` `none`/`lz4`/`zstd` axis through the decompression delegate-row gating the `Decoder` read so a compressed frame decompresses before decode, registering zero extension codecs because `SnapshotExtensionRows` is `never`; the json-stj row is STJ Strict camelCase with instants as ISO-8601 text, durations as round-trip text, smart-enum columns as key scalars, `tenantId` as the `UInt128` decimal-string, and absent evidence as explicit null; the geometry row sources no contract of its own, decoding the GeoJSON embedded in snapshot-delta and evidence-envelope payloads through the `[3]-[GEO_WIRE_DECODE]` `GeoJsonWire` seven-type RFC 7946 `Schema.Union` rather than a bare `JSON.parse` passthrough — the `Position` ordinate floor, the WGS84 longitude/latitude bound, and the absent-CRS guard admit XY/XYZ positions only, an M-ordinate or foreign-CRS payload faulting through the `quarantine/drift-terminal.md` fold as `Breaking` drift.
- Entry: `CODEC[key]` is the one polymorphic codec→operation entry keyed by `CodecKey` — the indexed row reads its `decode` and its `Option`-carried `encode` with no forwarding hop, so a new codec lands as one `CODEC` row and a new write direction lands as one `encode` slot, never a sibling rail and never a rename helper restating the index. The messagepack direction is read-only on the browser branch and its `encode` is `Option.none`; the proto and json-stj rows carry the encode mirror — the proto write constructs through `create` and serializes through `toBinary`, the json-stj write is `Schema.encode` against the same `Schema.Class` the row decodes, so a `CommandPayloadWire` write and a `DeepLinkBinding` query-string round-trip emit through the identical class their inbound receipts decode through.
- Packages: `@bufbuild/protobuf` for `create`/`fromBinary`/`toBinary` and the descriptor registry, `effect` for `Stream.fromAsyncIterable`, `Schema.decodeUnknown`/`Schema.encode`, the `Schema.Union`/`Schema.Tuple`/`Schema.filter` geometry surface, and the `Record` vocabulary dispatch; the messagepack codec is the workspace-cataloged binary `Decoder` mapping 64-bit integers to bigint with its `decodeArrayStream` async-generator stream decoder.
- Growth: a new codec lands as one `CODEC` row carrying its decode and encode operations; a new wire shape on an existing codec lands as one `Schema.Class` field row on the owning shape; a new GeoJSON geometry kind lands as one `GeoJsonWire` `Schema.Union` arm; a new snapshot compression codec lands as one `SNAPSHOT_CODEC` literal plus one `DECOMPRESSOR` delegate row, never a `switch` over the codec literal; a new write direction lands as one `encode` slot on the row, never a parallel `EncodeRail` owner.
- Boundary: the messagepack row registers zero extension codecs because `SnapshotExtensionRows` is `never` and an invented extension registration is the named defect; the geometry row's provenance is embedded-only — the `GeoJsonWire` union transcribes the GeoJSON the C# `GeoJsonConverterFactory` emits under the wire profile and never an invented standalone contract; the decompression delegate-row decodes the C# producer bytes only and re-mints no compression notion; decode and encode are the two slots of one row, never two owners; this domain references no telemetry type because telemetry crosses no wire contract.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type CodecKey = "proto" | "messagepack" | "json-stj" | "geometry";

interface CodecRow<Wire, Domain> {
  readonly decode: (bytes: Uint8Array) => Effect.Effect<Domain, ParseResult.ParseError>;
  readonly encode: Option.Option<(value: Domain) => Effect.Effect<Wire, ParseResult.ParseError>>;
}

// --- [OPERATIONS] --------------------------------------------------------------------
const snapshotDecoder = new Decoder({ useBigInt64: true });

const CODEC = {
  proto: {
    decode: (bytes) => Schema.decodeUnknown(ProtoMessageWire)(fromBinary(ProtoMessageSchema, bytes)),
    encode: Option.some((value) => Schema.encode(ProtoMessageWire)(value).pipe(Effect.map((init) => toBinary(ProtoMessageSchema, create(ProtoMessageSchema, init))))),
  },
  messagepack: {
    decode: (bytes) => Schema.decodeUnknown(SnapshotHeaderWire)(snapshotDecoder.decode(bytes)),
    encode: Option.none(),
  },
  "json-stj": {
    decode: (bytes) => Schema.decodeUnknown(ComputeReceiptWire)(JSON.parse(new TextDecoder().decode(bytes))),
    encode: Option.some((value) => Schema.encode(ComputeReceiptWire)(value).pipe(Effect.map((j) => new TextEncoder().encode(JSON.stringify(j))))),
  },
  geometry: { decode: (bytes) => Schema.decodeUnknown(GeoJsonWire)(JSON.parse(new TextDecoder().decode(bytes))), encode: Option.none() },
} as const satisfies Record<CodecKey, CodecRow<unknown, unknown>>;

const decodeSnapshotHeader = (bytes: Uint8Array): Effect.Effect<SnapshotHeaderRead, ParseResult.ParseError> =>
  Effect.gen(function* () {
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    const schemaFingerprint = view.getBigUint64(0, false);
    const codec = SNAPSHOT_CODEC[view.getUint8(8)];
    const classification = SNAPSHOT_CLASS[view.getUint8(9)];
    return yield* Schema.decodeUnknown(SnapshotHeaderWire)({ schemaFingerprint, codec, classification });
  });
```

## [3]-[GEO_WIRE_DECODE]

- Owner: `GeoJsonWire`, the `Schema.Union` over the seven RFC 7946 geometry types (`Point | LineString | Polygon | MultiPoint | MultiLineString | MultiPolygon | GeometryCollection`) plus the `Feature`/`FeatureCollection` wrappers, the `geometry` `CODEC` row decodes in place of the prior bare `GeometryWire` `JSON.parse` stub. The union is keyed by the GeoJSON `type` literal so a malformed `type` faults at the `Schema.Union` boundary rather than rendering corrupt, and the `Position` ordinate floor, the WGS84 bound, and the absent-CRS guard are the admission filters the C# `data-interchange#GEO_INTERCHANGE` NTS-interior CRS-fixed-by-format law projects as decode filters. The row sources no contract of its own — it transcribes the GeoJSON the C# `GeoJsonConverterFactory` emits under the wire profile, never re-authored.
- Cases: `Position` is the `Schema.Tuple` of two-or-three `number` ordinates (`[longitude, latitude]` or `[longitude, latitude, altitude]`) bounded so `longitude ∈ [-180, 180]` and `latitude ∈ [-90, 90]` — a projected (non-WGS84) coordinate faults the bound rather than rendering corrupt, and a four-ordinate XYM/XYZM position faults the tuple arity because the GeoJSON text wire structurally cannot carry the M ordinate (the C# blob projection routes measure data, never the text wire). `Point` carries one `Position`, `LineString` an array of `Position`, `Polygon` an array of linear rings (each an array of `Position`), the three `Multi*` types one nesting level deeper, and `GeometryCollection` an array of the six non-collection geometry arms (the recursion is one level — a `GeometryCollection` nested in a `GeometryCollection` is the rejected form RFC 7946 discourages). `Feature` wraps a nullable `geometry` plus a `properties` object and an optional `id`; `FeatureCollection` wraps an array of `Feature`. The absent-CRS guard rejects any decoded value carrying a `crs` member — RFC 7946 removed the CRS member (WGS84 is fixed by the format), so a present `crs` member is a foreign pre-2016 GeoJSON dialect that faults as `Breaking` drift.
- Entry: the `geometry` `CODEC` row decodes through `Schema.decodeUnknown(GeoJsonWire)` over the `JSON.parse`d embedded payload — the seven-type discrimination, the `Position` filter, the WGS84 bound, and the absent-CRS guard all admit at the one decode boundary, so a downstream fold reads a typed `GeoJsonWire` rather than an opaque parsed object. A malformed `type` literal, an out-of-range coordinate, an M-ordinate position, or a foreign `crs` member each faults through the `quarantine/drift-terminal.md` fold carrying the exact failing field path, never a silent passthrough.
- Packages: `effect` `Schema.Union`/`Schema.Tuple`/`Schema.Literal`/`Schema.filter`/`Schema.Array`/`Schema.NullOr`/`Schema.suspend` for the recursive geometry-type discrimination and the coordinate-shape floor; no new package.
- Growth: a new GeoJSON geometry kind lands as one `Schema.Union` arm keyed by its `type` literal and one `GeometryKind` literal; a tightened coordinate bound lands as one `Schema.filter` on `Position` (a data migration, never a compile signal, because an already-decoded value is not re-checked); zero second geometry owner.
- Boundary: the row sources no contract of its own — the GeoJSON `type` vocabulary, the `Position` shape, and the WGS84 fixed-CRS law are the C# producer's, transcribed not re-authored; a branch-side geometry shape, a measure-bearing ordinate, or a re-admitted `crs` member is the named drift defect; the geometry provenance stays embedded-only, never an invented standalone contract; the bound is the C# CRS-fixed-by-format law decoded as a filter, never a re-projection.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type GeometryKind =
  | "Point" | "LineString" | "Polygon"
  | "MultiPoint" | "MultiLineString" | "MultiPolygon"
  | "GeometryCollection";

// --- [MODELS] ------------------------------------------------------------------------
const Position = Schema.Tuple(Schema.Number, Schema.Number).pipe(
  Schema.filter((p): p is readonly [number, number] => p[0] >= -180 && p[0] <= 180 && p[1] >= -90 && p[1] <= 90, {
    message: () => "GeoJSON Position out of WGS84 bound: longitude ∈ [-180,180], latitude ∈ [-90,90]",
  }),
);

const PositionZ = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number).pipe(
  Schema.filter((p): p is readonly [number, number, number] => p[0] >= -180 && p[0] <= 180 && p[1] >= -90 && p[1] <= 90, {
    message: () => "GeoJSON Position out of WGS84 bound: longitude ∈ [-180,180], latitude ∈ [-90,90]",
  }),
);

const GeoPosition = Schema.Union(Position, PositionZ);
const LinearRing = Schema.Array(GeoPosition);

const PointWire = Schema.Struct({ type: Schema.Literal("Point"), coordinates: GeoPosition });
const LineStringWire = Schema.Struct({ type: Schema.Literal("LineString"), coordinates: Schema.Array(GeoPosition) });
const PolygonWire = Schema.Struct({ type: Schema.Literal("Polygon"), coordinates: Schema.Array(LinearRing) });
const MultiPointWire = Schema.Struct({ type: Schema.Literal("MultiPoint"), coordinates: Schema.Array(GeoPosition) });
const MultiLineStringWire = Schema.Struct({ type: Schema.Literal("MultiLineString"), coordinates: Schema.Array(Schema.Array(GeoPosition)) });
const MultiPolygonWire = Schema.Struct({ type: Schema.Literal("MultiPolygon"), coordinates: Schema.Array(Schema.Array(LinearRing)) });

const SimpleGeometryWire = Schema.Union(
  PointWire, LineStringWire, PolygonWire,
  MultiPointWire, MultiLineStringWire, MultiPolygonWire,
);

const GeometryCollectionWire = Schema.Struct({
  type: Schema.Literal("GeometryCollection"),
  geometries: Schema.Array(SimpleGeometryWire),
});

const GeometryWire = Schema.Union(SimpleGeometryWire, GeometryCollectionWire);

const FeatureWire = Schema.Struct({
  type: Schema.Literal("Feature"),
  geometry: Schema.NullOr(GeometryWire),
  properties: Schema.NullOr(Schema.Record({ key: Schema.String, value: Schema.Unknown })),
  id: Schema.optional(Schema.Union(Schema.String, Schema.Number)),
});

const FeatureCollectionWire = Schema.Struct({
  type: Schema.Literal("FeatureCollection"),
  features: Schema.Array(FeatureWire),
});

const GeoJsonWire = Schema.Union(GeometryWire, FeatureWire, FeatureCollectionWire).pipe(
  Schema.filter((v) => !(typeof v === "object" && v !== null && "crs" in v), {
    message: () => "foreign crs member: RFC 7946 fixes WGS84 by format and removed the crs member",
  }),
);
type GeoJsonWire = Schema.Schema.Type<typeof GeoJsonWire>;
```

## [4]-[SEGMENT_STREAM_DECODE]

- Owner: `decodeSegmentStream`, the one async-array fold lifting the C# sync-segment `ReadableStream` into a backpressured `Stream<OpLogEntryWire, ParseError>` through `@msgpack/msgpack` `Decoder.decodeArrayStream`, plus the `DECOMPRESSOR` delegate-row extending the `SNAPSHOT_CODEC` frozen table so each compression codec carries its decompressor. One entrypoint keyed by input shape (a `ReadableStreamLike` of an MessagePack array) — never a name-suffixed sibling of the sync `decode`, and the decompression is a config-as-value delegate row, never a `switch` over the codec literal.
- Cases: `decodeSegmentStream` reads the `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` sync-segment stream — the `OpLogEntryWire` array the C# `data-interchange#CHUNK_ALGEBRA` symmetric-chunk law crosses — through the reused `snapshotDecoder` (`Decoder({ useBigInt64: true })`) `decodeArrayStream` `AsyncGenerator`, lifted to a `Stream` through `Stream.fromAsyncIterable` and bounded by the same `Stream.buffer({ capacity: 1, strategy: "suspend" })` one-frame window the transport framing fold carries (peak memory one chunk wide, backpressure pull-shaped) and the `refinement/schema-refinement.md` `boundedFrames` budget so an unbounded segment stream faults before exhaustion. Each yielded array element decodes through `Schema.decodeUnknown(OpLogEntryWire)`. The `DECOMPRESSOR` row gates the `Decoder` read: a `none` frame reads its bytes directly, an `lz4`/`zstd` frame runs its decompressor delegate before the `Decoder` reads — a compressed frame parsed as raw MessagePack decodes to garbage, so decompression precedes decode.
- Entry: `decodeSegmentStream(source)` reads the `ReadableStream` against the reused `snapshotDecoder`, never a fresh `Decoder` per segment; the `decompressFrame(codec, bytes)` delegate reads the `SNAPSHOT_CODEC`-indexed `DECOMPRESSOR` row and applies it before the array decode. The `none` row is the identity delegate decoding today; the `lz4`/`zstd` rows are gated behind the RESEARCH spike (a verified wasm decoder round-tripping the C# `K4os.Compression.LZ4`/`ZstdSharp` producer bytes) and carry the failing delegate until the provider is admitted — until then the compressed-frame leg faults through `FaultDetail.HopFault` rather than silently corrupting.
- Packages: `@msgpack/msgpack` `Decoder.decodeArrayStream` (the `AsyncGenerator` over a `ReadableStreamLike` that handles chunking and buffering internally — `useBigInt64: true` mapping the 64-bit integers to bigint); `effect` `Stream.fromAsyncIterable`/`Stream.buffer`/`Stream.mapEffect` for the backpressured lift and the per-element decode.
- Growth: a new snapshot compression codec lands as one `SNAPSHOT_CODEC` literal plus one `DECOMPRESSOR` delegate row carrying its decompressor (config-as-value), never a branch arm; a new segment shape lands as one `Schema.Struct` field on `OpLogEntryWire`; zero second stream decoder.
- Boundary: the segment stream shares the transport one-frame `Stream.buffer` window and never a parallel buffering scheme; the `DECOMPRESSOR` row decompresses the C# producer bytes only and re-mints no compression notion — the `lz4`/`zstd` delegate stays the failing row until the spike admits a wasm provider that round-trips the producer bytes, the recommended cross-folder fallback being a `csharp:Rasm.Persistence/snapshots/codecs` `MessagePackCompression.None` companion lane so the browser never needs the wasm decoder; the segment fold reads the producer-owned `OpLogEntryWire`, never a re-minted segment envelope.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type SnapshotCodecKey = (typeof SNAPSHOT_CODEC)[number];
type Decompressor = (bytes: Uint8Array) => Effect.Effect<Uint8Array, FaultDetail>;

// --- [OPERATIONS] --------------------------------------------------------------------
const gatedDecompressor =
  (codec: SnapshotCodecKey): Decompressor =>
  () =>
    Effect.fail(FaultDetail.HopFault({ reason: "command-disabled", evidence: { codec, spike: "wasm-lz4-zstd-provider" } }));

const DECOMPRESSOR = {
  none: (bytes) => Effect.succeed(bytes),
  lz4: gatedDecompressor("lz4"),
  zstd: gatedDecompressor("zstd"),
} as const satisfies Record<SnapshotCodecKey, Decompressor>;

const decompressFrame = (codec: SnapshotCodecKey, bytes: Uint8Array): Effect.Effect<Uint8Array, FaultDetail> =>
  DECOMPRESSOR[codec](bytes);

const decodeSegmentStream = (
  source: ReadableStream<Uint8Array>,
  budget: DecodeBudget,
): Stream.Stream<OpLogEntryWire, ParseResult.ParseError | FaultDetail> =>
  Stream.fromAsyncIterable(snapshotDecoder.decodeArrayStream(source), (cause) => faultDetailRail.fromConnect(cause)).pipe(
    Stream.buffer({ capacity: 1, strategy: "suspend" }),
    Stream.take(budget.maxFrames),
    Stream.mapEffect((element) => Schema.decodeUnknown(OpLogEntryWire)(element)),
  );
```

The `decodeSegmentStream` fold reuses the SAME `snapshotDecoder` the whole-frame `messagepack` `CODEC` row reads with — the stream entry and the sync entry are the two shapes of one decoder, never a second `Decoder` per direction — and the `DECOMPRESSOR` delegate-row gates the read so the compressed-frame leg is one frozen-table row, not a branch over the `SNAPSHOT_CODEC` literal.

## [5]-[CRDT_OP_DECODE]

- Owner: `CrdtOpWire`, the ten-arm discriminated union the `messagepack` row decodes out of an `OpLogEntryWire.payload` whose `columnFamily` is `"crdt"`, sourced verbatim from `csharp:Rasm.Persistence/versioning/version-control#CRDT_WIRE`. The C# producer is the single mint — a `[MessagePack.Union]` over the union tags `Set=0 | Write=1 | Add=2 | Remove=3 | Increment=4 | InsertAfter=5 | Delete=6 | Maintain=7 | Beat=8 | Leave=9`, each arm a `[MessagePackObject]` whose `[Key(n)]` positions are the dense array schema. This row decodes that union and never re-declares a parallel `kind`-discriminated CRDT envelope — a second mint is the named cross-language drift defect the `contracts/wire-inventory#WIRE_LAW` fixes.
- Cases: the MessagePack union encodes as the two-element array `[tag, [field0, field1, …]]` — the union tag selects the arm and the inner array carries the arm's `[Key]`-ordered fields. The HLC cell crosses as the two flat halves `physicalTicks` (`long`, .NET unix ticks via `Hlc.Physical.ToUnixTimeTicks()`) and `logical` (`ulong`) — NOT an ISO-8601 instant, because the wire arm flattens the `Hlc` to `(PhysicalTicks, Logical)` rather than carrying the `OpLogEntryWire.physical` string; both halves decode as bigint under `useBigInt64`. The OR-set tags cross as the flat `(tagOrigin: Guid, tagLogical: ulong)` pair (`Add`) and the `observedTags` array of `(origin, logical)` pairs (`Remove`); the RGA element ids cross as flat `(predOrigin, predLogical)`/`(idOrigin, idLogical)` Guid+ulong pairs; the version-vector `context`/`quiescent` crosses as the `(origin: Guid, seq: long)[]` slot array; `presence`-family `beat`/`leave` carry the `EphemeralMap` delta so the live-multiplayer state decodes on the one wire vocabulary.
- Entry: `decodeCrdtOp(bytes)` reads the union through the reused `snapshotDecoder` (`Decoder({ useBigInt64: true })`) then folds the `[tag, fields]` pair through `Schema.decodeUnknown(CrdtOpWire)`, the `Schema.Union` keyed by the literal `op` tag the decode normalizes from the numeric union index; the `crdt` payload slot on `OpLogEntryWire` decodes through this entry whenever `columnFamily === "crdt"`, so the sync-segment fold reads a typed `CrdtOpWire` rather than an opaque `Uint8Array`. The numeric union tag normalizes to the literal `op` discriminant through `CRDT_OP_TAG` so the decoded union is the same `op`-keyed shape the Python leg and the C# `CrdtOpWire` carry, never the raw numeric index leaking past the boundary.
- Packages: `@msgpack/msgpack` `Decoder({ useBigInt64: true })` for the MessagePack array decode with the 64-bit-to-bigint mapping (the `Guid` 16-byte fields decode as their MessagePack binary then normalize to the canonical string through the `Guid` brand); `effect` `Schema.decodeUnknown`/`Schema.Union`/`Schema.Literal` for the ten-arm union surface and the `op`-tag normalization.
- Growth: a new C# `CrdtOpWire` arm lands as one added union tag in `CRDT_OP_TAG`, one `Schema.Struct` arm on `CrdtOpWire`, and one normalize row — an unknown union tag folds to the `quarantine/drift-terminal.md` `Additive` case rather than a silent miss, never a branch-side invented arm; a retired C# union tag is never reassigned because the producer's append-only `[MessagePack.Union]` law forbids reuse; zero second CRDT envelope.
- Boundary: this row decodes the producer-owned union and re-mints NO CRDT shape — the `op` literal vocabulary, the `[Key]` field order, and the union-tag sequence are the C# producer's, transcribed not re-authored, and a `kind`-discriminated re-mint on this page is the named drift defect; the HLC cell crosses as the two flat `physicalTicks`/`logical` bigint halves the wire arm carries, never the `OpLogEntryWire.physical` ISO-8601 string (a different HLC projection on a different surface), and the two-half order is the load-bearing one the `parity/content-key-parity#HLC_TWO_HALF_PARITY` round-trip asserts; the `Guid` fields decode through the `refinement/schema-refinement.md` `Guid` brand so a raw string never enters an origin slot; the `crdt` payload is the only column family routed through this union — an `upsert`/`delete`/`presence` payload on a non-`crdt` column family stays the opaque `Uint8Array` the `OpLogEntryWire.payload` slot carries, never folded through the CRDT union.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type CrdtOpTag =
  | "set" | "write" | "add" | "remove" | "increment"
  | "insertAfter" | "delete" | "maintain" | "beat" | "leave";

// --- [CONSTANTS] ---------------------------------------------------------------------
const CRDT_OP_TAG = [
  "set", "write", "add", "remove", "increment",
  "insertAfter", "delete", "maintain", "beat", "leave",
] as const satisfies ReadonlyArray<CrdtOpTag>;

// --- [MODELS] ------------------------------------------------------------------------
const VectorSlot = Schema.Tuple(Guid, Schema.BigIntFromSelf);
const ElementTag = Schema.Tuple(Guid, Schema.BigIntFromSelf);

const CrdtOpWire = Schema.Union(
  Schema.Struct({ op: Schema.Literal("set"), field: Schema.String, value: Schema.Uint8ArrayFromSelf, physicalTicks: Schema.BigIntFromSelf, logical: Schema.BigIntFromSelf, origin: Guid }),
  Schema.Struct({ op: Schema.Literal("write"), field: Schema.String, value: Schema.Uint8ArrayFromSelf, context: Schema.Array(VectorSlot), physicalTicks: Schema.BigIntFromSelf, logical: Schema.BigIntFromSelf, origin: Guid }),
  Schema.Struct({ op: Schema.Literal("add"), field: Schema.String, element: ContentKey, tagOrigin: Guid, tagLogical: Schema.BigIntFromSelf }),
  Schema.Struct({ op: Schema.Literal("remove"), field: Schema.String, element: ContentKey, observedTags: Schema.Array(ElementTag) }),
  Schema.Struct({ op: Schema.Literal("increment"), field: Schema.String, origin: Guid, delta: Schema.BigIntFromSelf }),
  Schema.Struct({ op: Schema.Literal("insertAfter"), field: Schema.String, predOrigin: Guid, predLogical: Schema.BigIntFromSelf, idOrigin: Guid, idLogical: Schema.BigIntFromSelf, value: Schema.Uint8ArrayFromSelf }),
  Schema.Struct({ op: Schema.Literal("delete"), field: Schema.String, idOrigin: Guid, idLogical: Schema.BigIntFromSelf }),
  Schema.Struct({ op: Schema.Literal("maintain"), field: Schema.String, quiescent: Schema.Array(VectorSlot) }),
  Schema.Struct({ op: Schema.Literal("beat"), field: Schema.String, origin: Guid, state: Schema.Uint8ArrayFromSelf, physicalTicks: Schema.BigIntFromSelf, logical: Schema.BigIntFromSelf }),
  Schema.Struct({ op: Schema.Literal("leave"), field: Schema.String, origin: Guid, physicalTicks: Schema.BigIntFromSelf, logical: Schema.BigIntFromSelf }),
);
type CrdtOpWire = Schema.Schema.Type<typeof CrdtOpWire>;

// --- [OPERATIONS] --------------------------------------------------------------------
const crdtFieldsByTag: Record<CrdtOpTag, ReadonlyArray<string>> = {
  set: ["field", "value", "physicalTicks", "logical", "origin"],
  write: ["field", "value", "context", "physicalTicks", "logical", "origin"],
  add: ["field", "element", "tagOrigin", "tagLogical"],
  remove: ["field", "element", "observedTags"],
  increment: ["field", "origin", "delta"],
  insertAfter: ["field", "predOrigin", "predLogical", "idOrigin", "idLogical", "value"],
  delete: ["field", "idOrigin", "idLogical"],
  maintain: ["field", "quiescent"],
  beat: ["field", "origin", "state", "physicalTicks", "logical"],
  leave: ["field", "origin", "physicalTicks", "logical"],
};

const decodeCrdtOp = (bytes: Uint8Array): Effect.Effect<CrdtOpWire, ParseResult.ParseError> =>
  Effect.gen(function* () {
    const [tag, fields] = snapshotDecoder.decode(bytes) as readonly [number, ReadonlyArray<unknown>];
    const op = CRDT_OP_TAG[tag];
    const shape = crdtFieldsByTag[op].reduce<Record<string, unknown>>(
      (acc, key, i) => ({ ...acc, [key]: fields[i] }),
      { op },
    );
    return yield* Schema.decodeUnknown(CrdtOpWire)(shape);
  });
```

The `messagepack` `CODEC` row decodes the snapshot/segment frame; the `crdt` op delta is the SAME row's payload projection — `decodeCrdtOp` reads the `OpLogEntryWire.payload` bytes only when `columnFamily === "crdt"`, so the sync-segment fold lifts each entry's payload into the typed `CrdtOpWire` union rather than the opaque `Uint8Array`, and a non-`crdt` payload stays the raw byte slot.

```ts contract
const decodeOpPayload = (entry: OpLogEntryWire): Effect.Effect<CrdtOpWire | Uint8Array, ParseResult.ParseError> =>
  entry.columnFamily === "crdt" ? decodeCrdtOp(entry.payload) : Effect.succeed(entry.payload);
```

## [6]-[TS_PROJECTION]

- Owner: the snapshot, sync, and receipt wire shapes the messagepack and json-stj rows decode, sourced from `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION`, `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION`, `csharp:Rasm.Persistence/versioning/version-control#TS_PROJECTION`, and `csharp:Rasm.Compute/receipts/receipts#TS_PROJECTION`; the artifact-frame and fault-detail shapes ride their own pages. `OpLogEntryWire` transcribes the producer op-log row whole — every field the C# `OpLogEntry` emits — so the `projection` causal, cursor, and convergence folds read `entityKind`/`entityKey`/`columnFamily`/`origin`/`sequence` from a decoded source rather than an absent slot; `CommitNodeWire`/`VersionVectorWire`/`HlcWire` transcribe the `version-control#TS_PROJECTION` commit, vector, and clock shapes the version-vector concurrency read model reconstructs. The `crdt` column-family `OpLogEntryWire.payload` decodes through the `[5]-[CRDT_OP_DECODE]` `CrdtOpWire` union owned at `csharp:Rasm.Persistence/versioning/version-control#CRDT_WIRE`, consumed here and never re-declared.
- Entry: `SnapshotHeaderWire.schemaFingerprint` decodes as bigint via `DataView` `getBigUint64`; the messagepack `Decoder` carries the bigint mapping; `OpLogEntryWire` is the full producer op-log row — `sequence`/`entityKind`/`entityKey`/`columnFamily`/`kind`/`codec`/`payload`/`image`/`contentKey`/`closure`/`actor`/`origin`/`physical`/`logical` field-for-field against `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` — so `entityKind`/`entityKey` key the `projection` causal fold, `origin`/`sequence` reconstruct the per-origin cursor the version vector projects, `columnFamily` gates the `crdt` op-delta decode, and `image`/`closure` carry the before-image and the content-key transfer manifest; content keys cross as 16-byte binary and the scalar HLC stamp crosses as the `physical` ISO-8601 instant plus the `logical` bigint half the `projection` event-time fold and convergence order read; `CommitNodeWire.vector` and the standalone `VersionVectorWire` cross as the `Record<string, bigint>` per-origin slot map the `causality-graph/version-vector` fold reads as the producer-stamped vector, and `HlcWire` carries the `{ physical, logical }` causal cell the commit and blame rows stamp; absent evidence on `ComputeReceiptWire` crosses as explicit null; `SnapshotExtensionRows` is `never` and no extension codec is registered.
- Packages: the messagepack `Decoder` and `effect` `Schema` for the codec surface.

```ts contract
const SNAPSHOT_CODEC = ["none", "lz4", "zstd"] as const;
const SNAPSHOT_CLASS = ["public", "internal", "restricted"] as const;

const SnapshotHeaderWire = Schema.Struct({
  schemaFingerprint: Schema.BigIntFromSelf,
  codec: Schema.Literal(...SNAPSHOT_CODEC),
  classification: Schema.Literal(...SNAPSHOT_CLASS),
});

const HlcWire = Schema.Struct({ physical: Schema.String, logical: Schema.BigIntFromSelf });

const VersionVectorWire = Schema.Struct({ slots: Schema.Record({ key: Schema.String, value: Schema.BigIntFromSelf }) });

const OpLogEntryWire = Schema.Struct({
  sequence: Schema.BigIntFromSelf,
  entityKind: Schema.String,
  entityKey: Schema.String,
  columnFamily: Schema.String,
  kind: Schema.Literal("upsert", "delete", "presence"),
  codec: Schema.String,
  payload: Schema.Uint8ArrayFromSelf,
  image: Schema.Uint8ArrayFromSelf,
  contentKey: ContentKey,
  closure: Schema.Array(ContentKey),
  actor: Schema.String,
  origin: Schema.String,
  physical: Schema.String,
  logical: Schema.BigIntFromSelf,
});

const CommitNodeWire = Schema.Struct({
  contentKey: ContentKey,
  parents: Schema.Array(ContentKey),
  opKeys: Schema.Array(ContentKey),
  branch: Schema.String,
  vector: VersionVectorWire,
  actor: Schema.String,
  cell: HlcWire,
});
```

## [7]-[BCF_LIVE_WIRE_DECODE]

- Owner: the `json-stj` `Schema.Struct` decode rows for the two host-local C# coordination/binding clusters the eleven-anchor map gained — `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` sourced verbatim from `csharp:Rasm.Bim/coordination/issue-exchange#TS_PROJECTION`, and `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire`/`WriteBackWire` sourced verbatim from `csharp:Rasm.AppHost/live-wire/live-wire#TS_PROJECTION`. The `ui/bcf-anchor` and `ui/live-binding-dashboard` leaves decode these rows by reference through the `json-stj` `CODEC` row; nothing here re-mints a C#-owned shape — the BCF topic vocabulary is the `Rasm.Bim` single mint and the binding/write-receipt vocabulary is the `Rasm.AppHost/live-wire` single mint, transcribed not re-authored.
- Cases: the BCF rows cross under `BimWireOptions.Json` camelCase — `BcfStatus` as the `[JsonStringEnumMemberName]` lower-kebab string literal, the kernel `Vector3` camera triplet as the camelCase `{ x, y, z }` object, the IFC-GUID `selectedGlobalIds`/`visibleGlobalIds` as string arrays, the `Instant` creation/comment dates as ISO-8601 extended text, and the `Option<string> viewpointGuid` as a present-or-absent nullable; the live-wire rows cross under `LiveWireOptions.Json` camelCase — `ExternalTransport`/`BindingState` as their `[SmartEnum<string>]` key literals, the projected `BindingDirectionKey` as the single lowered token (never the `[Flags]` bitmask or the STJ `"Inbound, Outbound"` join), the `Option<Instant> lastGoodAt` as `string | null`, the `Instant` source/ack timestamps as ISO-8601 text and the `Duration elapsed` as round-trip text, and the `WriteBackWire` disposition as the `kind`-discriminated union the C# `LiveWireProjection.Lower` projects from the `WriteBack` `[Union]`.
- Entry: each row decodes through `Schema.decodeUnknown` off the `json-stj` `CODEC` row's `JSON.parse`d body — `decodeBcfTopics` admits the `BcfWire.Topics` payload as a `Schema.Array(BcfTopicWire)`, the `ui/bcf-anchor` `anchorViewpoint` reading `selectedGlobalIds` against the model GlobalIds; `decodeBindingStatus`/`decodeWriteReceipt`/`decodeCoercedValue` admit each live-wire row, the write receipt riding the existing `projection` `ReceiptEnvelopeWire<WriteReceiptWire>` so the studio evidence timeline reads one envelope vocabulary, and the `dispositionOf` switch reads the `WriteBackWire` `kind` literal verbatim.
- Packages: `effect` `Schema.Struct`/`Schema.Array`/`Schema.Literal`/`Schema.Union`/`Schema.NullOr`/`Schema.optionalWith`/`Schema.decodeUnknown` for the json-stj surface; no codec beyond the settled `json-stj` `CODEC` row.
- Growth: a new BCF entity is one added `Schema.Struct` field on the owning wire row mirroring its `[JsonSerializable]` producer record; a new binding field is one `BindingStatusWire` field; a new write disposition is one added `Schema.Struct` arm on the `WriteBackWire` union mirroring its C# `WriteBack` `[Union]` case; a new lifecycle state or transport is one added `Schema.Literal` member; never a second decode beside the `json-stj` row.
- Boundary: these rows decode the producer-owned shapes and re-mint NOTHING — the `BcfStatus` lower-kebab vocabulary, the `BindingDirectionKey` single token, the `WriteBackWire` `kind` discriminant, and the field order are the C# producers', transcribed; a branch-side BCF topic shape, a branch-side binding-status enum, or a second `WriteBackWire` re-mint is the named cross-language drift defect; the `Vector3` camera crosses as the `{ x, y, z }` camelCase object the kernel produces under `BimWireOptions.Json`, never a `[x,y,z]` tuple; the IFC GlobalIds cross as plain strings (the 22-char IFC base64 GUID), never the 16-byte `ContentKey` brand nor the UUID `Guid` brand; the `[Flags] BindingDirection` never crosses as a raw bitmask — the producer `LiveWireProjection.DirectionKey` lowers it to the one token the `BindingDirectionKey` literal decodes, a flags integer or the comma-joined STJ string being the named seam violation.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type BcfStatusKey = "open" | "in-progress" | "resolved" | "closed" | "reopened";
type ExternalTransportKey =
  | "opc-ua" | "modbus" | "mqtt" | "serial" | "rest" | "graphql" | "spreadsheet" | "erp-plm";
type BindingStateKey = "connecting" | "subscribed" | "polling" | "stale" | "faulted";
type BindingDirectionKey = "inbound" | "outbound" | "bidirectional";

// --- [MODELS] ------------------------------------------------------------------------
const Vector3Wire = Schema.Struct({ x: Schema.Number, y: Schema.Number, z: Schema.Number });

const BcfViewpointWire = Schema.Struct({
  guid: Schema.String,
  cameraPosition: Vector3Wire,
  cameraDirection: Vector3Wire,
  cameraUpVector: Vector3Wire,
  fieldOfView: Schema.Number,
  selectedGlobalIds: Schema.Array(Schema.String),
  visibleGlobalIds: Schema.Array(Schema.String),
});

const BcfCommentWire = Schema.Struct({
  guid: Schema.String,
  author: Schema.String,
  text: Schema.String,
  viewpointGuid: Schema.optionalWith(Schema.String, { nullable: true, as: "Option" }),
  date: Schema.String,
});

const BcfTopicWire = Schema.Struct({
  guid: Schema.String,
  title: Schema.String,
  status: Schema.Literal("open", "in-progress", "resolved", "closed", "reopened"),
  topicType: Schema.String,
  priority: Schema.String,
  author: Schema.String,
  creationDate: Schema.String,
  comments: Schema.Array(BcfCommentWire),
  viewpoints: Schema.Array(BcfViewpointWire),
});

const CoercedValueWire = Schema.Struct({
  canonical: Schema.Number,
  canonicalUnit: Schema.String,
  sourceUnit: Schema.String,
  sourceAt: Schema.String,
});

const BindingStatusWire = Schema.Struct({
  bindingId: Schema.String,
  transport: Schema.Literal("opc-ua", "modbus", "mqtt", "serial", "rest", "graphql", "spreadsheet", "erp-plm"),
  state: Schema.Literal("connecting", "subscribed", "polling", "stale", "faulted"),
  direction: Schema.Literal("inbound", "outbound", "bidirectional"),
  lastGoodAt: Schema.NullOr(Schema.String),
});

const WriteBackWire = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("acknowledged"), sourceAck: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("rejected"), fault: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("rolled-back"), priorValue: Schema.Number }),
  Schema.Struct({ kind: Schema.Literal("coalesced"), foldedInto: Schema.Number }),
);

const WriteReceiptWire = Schema.Struct({
  bindingId: Schema.String,
  canonical: Schema.Number,
  rendered: Schema.Number,
  renderedUnit: Schema.String,
  disposition: WriteBackWire,
  elapsed: Schema.String,
  correlation: Schema.String,
});

// --- [OPERATIONS] --------------------------------------------------------------------
const decodeBcfTopics = (bytes: Uint8Array): Effect.Effect<ReadonlyArray<typeof BcfTopicWire.Type>, ParseResult.ParseError> =>
  Schema.decodeUnknown(Schema.Struct({ topics: Schema.Array(BcfTopicWire) }))(JSON.parse(new TextDecoder().decode(bytes))).pipe(Effect.map((w) => w.topics));

const decodeBindingStatus = (bytes: Uint8Array): Effect.Effect<typeof BindingStatusWire.Type, ParseResult.ParseError> =>
  Schema.decodeUnknown(BindingStatusWire)(JSON.parse(new TextDecoder().decode(bytes)));

const decodeWriteReceipt = (bytes: Uint8Array): Effect.Effect<typeof WriteReceiptWire.Type, ParseResult.ParseError> =>
  Schema.decodeUnknown(WriteReceiptWire)(JSON.parse(new TextDecoder().decode(bytes)));

const decodeCoercedValue = (bytes: Uint8Array): Effect.Effect<typeof CoercedValueWire.Type, ParseResult.ParseError> =>
  Schema.decodeUnknown(CoercedValueWire)(JSON.parse(new TextDecoder().decode(bytes)));
```
