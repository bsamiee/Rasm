# [INTERCHANGE_DECODE_RAIL]

The byte-to-typed decode interior of the wire boundary: one codec-keyed dispatch table owning every byte format and both directions as rows. `DecodeRail` is the densest owner of the wire interior — one `CODEC` vocabulary keys the proto, messagepack, json-stj, and embedded-geometry rows to their decode and encode operations, never a parallel rail per concern and never a per-instance interface re-described at each call site. The artifact-frame reassembly (`artifacts/frame-reassembly.md`), the fault reconstruction (`faults/fault-family.md`), and the brand-and-filter refinement vocabulary (`refinement/schema-refinement.md`) compose this same decode discipline from their own pages over the bytes these codec rows admit; the suite-anchor transcription law is owned once by `contracts/wire-inventory.md`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                            |
| :-----: | :------------ | :--------------------------------------------------------------- |
|   [1]   | DECODE_RAIL   | the codec dispatch vocabulary, both directions, the geometry row |
|   [2]   | TS_PROJECTION | the snapshot, sync, and receipt wire shapes the rails decode     |

## [2]-[DECODE_RAIL]

- Owner: `DecodeRail`, one `CODEC` vocabulary keyed by `CodecKey` whose rows carry their own byte-format behavior — the proto row decodes unary responses and server-streams, the messagepack row decodes snapshot frames and the sync-segment stream, the json-stj row is `Schema.Class` transcription of the receipt fences, and the geometry row decodes embedded GeoJSON fields. The fold reads a row and returns its `decode`/`encode` pair; a `Match` chain restating the codec→operation knowledge inline is the rejected form, the keyed domain dispatching through the vocabulary lookup the `CODEC` table already maps.
- Cases: the proto row reads `csharp:Rasm.Compute/remote/remote#TS_PROJECTION` and `csharp:Rasm.Compute/progress/progress#TS_PROJECTION` through `fromBinary` against the generated descriptor with `Stream.fromAsyncIterable` giving the server-stream pull-based backpressure the framing fold shares; the messagepack row reads `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION` and `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` through one reused `Decoder` mapping 64-bit integers to bigint and the fixed-width header through a `DataView` `getBigUint64`, registering zero extension codecs because `SnapshotExtensionRows` is `never`; the json-stj row is STJ Strict camelCase with instants as ISO-8601 text, durations as round-trip text, smart-enum columns as key scalars, `tenantId` as the `UInt128` decimal-string, and absent evidence as explicit null; the geometry row sources no contract of its own, decoding only the GeoJSON embedded in snapshot-delta and evidence-envelope payloads.
- Entry: `CODEC[key]` is the one polymorphic codec→operation entry keyed by `CodecKey` — the indexed row reads its `decode` and its `Option`-carried `encode` with no forwarding hop, so a new codec lands as one `CODEC` row and a new write direction lands as one `encode` slot, never a sibling rail and never a rename helper restating the index. The messagepack direction is read-only on the browser branch and its `encode` is `Option.none`; the proto and json-stj rows carry the encode mirror — the proto write constructs through `create` and serializes through `toBinary`, the json-stj write is `Schema.encode` against the same `Schema.Class` the row decodes, so a `CommandPayloadWire` write and a `DeepLinkBinding` query-string round-trip emit through the identical class their inbound receipts decode through.
- Packages: `@bufbuild/protobuf` for `create`/`fromBinary`/`toBinary` and the descriptor registry, `effect` for `Stream.fromAsyncIterable`, `Schema.decodeUnknown`/`Schema.encode`, and the `Record` vocabulary dispatch; the messagepack codec is the workspace-cataloged binary `Decoder` mapping 64-bit integers to bigint.
- Growth: a new codec lands as one `CODEC` row carrying its decode and encode operations; a new wire shape on an existing codec lands as one `Schema.Class` field row on the owning shape; a new write direction lands as one `encode` slot on the row, never a parallel `EncodeRail` owner.
- Boundary: the messagepack row registers zero extension codecs because `SnapshotExtensionRows` is `never` and an invented extension registration is the named defect; the geometry row's provenance is embedded-only and never an invented contract; decode and encode are the two slots of one row, never two owners; this domain references no telemetry type because telemetry crosses no wire contract.

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
  geometry: { decode: (bytes) => Schema.decodeUnknown(GeometryWire)(JSON.parse(new TextDecoder().decode(bytes))), encode: Option.none() },
} as const satisfies Record<CodecKey, CodecRow<unknown, unknown>>;

const decodeSnapshotHeader = (bytes: Uint8Array): Effect.Effect<SnapshotHeaderRead, ParseResult.ParseError> =>
  Effect.gen(function* () {
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    const schemaFingerprint = view.getBigUint64(0, true);
    const codec = SNAPSHOT_CODEC[view.getUint8(8)];
    const classification = SNAPSHOT_CLASS[view.getUint8(9)];
    return yield* Schema.decodeUnknown(SnapshotHeaderWire)({ schemaFingerprint, codec, classification });
  });
```

## [3]-[TS_PROJECTION]

- Owner: the snapshot, sync, and receipt wire shapes the messagepack and json-stj rows decode, sourced from `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION`, `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION`, and `csharp:Rasm.Compute/receipts/receipts#TS_PROJECTION`; the artifact-frame and fault-detail shapes ride their own pages.
- Entry: `SnapshotHeaderWire.schemaFingerprint` decodes as bigint via `DataView` `getBigUint64`; the messagepack `Decoder` carries the bigint mapping; `OpLogEntryWire` content keys cross as 16-byte binary and the HLC stamp crosses as the `physical` ISO-8601 instant plus the `logical` bigint half the `projection` event-time fold and convergence order read; absent evidence on `ComputeReceiptWire` crosses as explicit null; `SnapshotExtensionRows` is `never` and no extension codec is registered.
- Packages: the messagepack `Decoder` and `effect` `Schema` for the codec surface.

```ts contract
const SNAPSHOT_CODEC = ["none", "lz4", "zstd"] as const;
const SNAPSHOT_CLASS = ["public", "internal", "restricted"] as const;

const SnapshotHeaderWire = Schema.Struct({
  schemaFingerprint: Schema.BigIntFromSelf,
  codec: Schema.Literal(...SNAPSHOT_CODEC),
  classification: Schema.Literal(...SNAPSHOT_CLASS),
});

const OpLogEntryWire = Schema.Struct({
  kind: Schema.Literal("upsert", "delete", "presence"),
  payload: Schema.Uint8ArrayFromSelf,
  contentKey: ContentKey,
  sequence: Schema.BigIntFromSelf,
  physical: Schema.String,
  logical: Schema.BigIntFromSelf,
});
```
