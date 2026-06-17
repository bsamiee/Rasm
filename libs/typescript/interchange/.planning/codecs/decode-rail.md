# [INTERCHANGE_DECODE_RAIL]

The byte-to-typed decode interior of the wire boundary: the single codec rail family read by codec key, the direction-keyed encode write face, and the embedded-geometry rail. `DecodeRail` is the densest owner of the wire interior — one owner carries every codec and every direction as rows, never a parallel rail per concern. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which rail consumes which fence and never re-authors a shape. The brand-and-filter decode-enforcement vocabulary lives at `refinement/schema-refinement.md`, the content-addressed frame rail at `artifacts/frame-reassembly.md`, and the fault rail at `faults/fault-family.md`; each is a row on this owner read from its own page.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                       |
| :-----: | :------------ | :---------------------------------------------------------- |
|   [1]   | DECODE_RAIL   | the codec rail family, the encode mirror, the geometry rail |
|   [2]   | TS_PROJECTION | the snapshot, sync, and receipt wire shapes the rails decode |

## [2]-[DECODE_RAIL]

- Owner: `DecodeRail`, the codec rail family read by codec key — the protobuf-over-transport rail, the binary snapshot rail, the structured-text receipt rail, and `GeometryRail` for embedded geometry. `EncodeRail` is the direction-keyed write face of the same owner. The artifact-frame rail (`artifacts/frame-reassembly.md`), the fault-detail rail (`faults/fault-family.md`), and the refinement vocabulary (`refinement/schema-refinement.md`) are rows on this owner, never a parallel rail.
- Cases: the protobuf rail decodes unary responses and server-streams against `csharp:Rasm.Compute/remote/remote#TS_PROJECTION` and `csharp:Rasm.Compute/progress/progress#TS_PROJECTION`; the binary rail decodes snapshot frames and the multi-object sync-segment stream against `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION` and `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` with 64-bit integers mapped to bigint through `useBigInt64` and the fixed-width header read through a `DataView` `getBigUint64`, zero registered extension codecs because `SnapshotExtensionRows` is `never` on its owner page; the structured-text rail is `Schema.Class` transcription of the json-stj fences, drift-gated by the app-root-emitted JSON schema set.
- Entry: `GeometryRail` sources no wire contract of its own; it decodes only embedded geometry fields — invoked on the snapshot-delta payloads whose geometry rides the GeoJSON projection owned by `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION`, and invoked on the geometry embedded inside the evidence envelope payloads — and is never re-authored per cluster. The codec posture is one row per surface: a reused `@msgpack/msgpack` `Decoder` with `useBigInt64: true` and the default `ExtensionCodec` carrying zero `register` rows; a `DataView` reading the fixed-width snapshot header; STJ Strict camelCase for the json-stj rail with instants as ISO-8601 text, durations as round-trip text, smart-enum columns as key scalars, `tenantId` as the `UInt128` decimal-string, and absent evidence as explicit null. `Stream.fromAsyncIterable` gives the proto server-stream pull-based backpressure shared with the framing fold.
- Encode: `EncodeRail` is the encode mirror keyed by direction — the proto write face constructs through `create` against the generated message descriptor and serializes through `toBinary`, and the json-stj write face is `Schema.encode` against the same `Schema.Class` the decode rail reads, so a `CommandPayloadWire` write and a `DeepLinkBinding` query-string round-trip emit through the identical class their inbound receipts decode through; the messagepack direction is read-only on the browser branch and its encode mirror is empty by contract; a hand-shaped message literal or a hand-serialized JSON body is the deleted form.
- Packages: `@msgpack/msgpack` for the binary codec, `@bufbuild/protobuf` for `create`/`toBinary`/`fromBinary` and the descriptor registry, `@connectrpc/connect` for `ConnectError.findDetails`, and `effect` for the `Stream`, `Schema.encode`, and `Match` primitives.
- Growth: a new codec lands as one rail row; a new wire shape on an existing codec lands as one schema row on the owning rail; a new write direction lands as one `EncodeRail` direction key on the same owner; zero new rail.
- Boundary: the binary rail registers zero extension codecs because `SnapshotExtensionRows` is `never` and an invented extension registration is the named defect; `GeometryRail` provenance is embedded-only and never an invented contract; `EncodeRail` and `DecodeRail` are the write and read faces of one owner; this domain references no telemetry type because telemetry crosses no wire contract.

```ts contract
type CodecKey = "proto" | "messagepack" | "json-stj" | "geometry" | "artifact-frame" | "fault-detail";
type Direction = "decode" | "encode";

interface DecodeRail<Wire, Domain> {
  readonly codec: CodecKey;
  readonly decode: (bytes: Uint8Array) => Effect.Effect<Domain, ParseResult.ParseError>;
  readonly encode: Option.Option<(value: Domain) => Effect.Effect<Wire, ParseResult.ParseError>>;
}

const snapshotDecoder = new Decoder({ useBigInt64: true });

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

- Owner: the snapshot, sync, and receipt wire shapes the binary and json-stj rails decode — transcribed verbatim from `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION`, `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION`, and `csharp:Rasm.Compute/receipts/receipts#TS_PROJECTION`; the artifact-frame and fault-detail shapes ride their own pages.
- Entry: `SnapshotHeaderWire.schemaFingerprint` decodes as bigint via `DataView` `getBigUint64`; `SnapshotDecodeOptions` carries `useBigInt64: true`; `OpLogEntryWire` content keys cross as 16-byte binary; absent evidence on `ComputeReceiptWire` crosses as explicit null.
- Packages: `@msgpack/msgpack` and `effect` `Schema` for the codec surface.
- Growth: a new snapshot/sync/receipt member lands as one `Schema.Class` field row; the branch authors no shape absent from the C# fence.
- Boundary: every shape transcribes a C# `#TS_PROJECTION` fence; `SnapshotExtensionRows` is `never` and no extension codec is registered.

```ts contract
const SnapshotHeaderWire = Schema.Struct({
  schemaFingerprint: Schema.BigIntFromSelf,
  codec: Schema.Literal("none", "lz4", "zstd"),
  classification: Schema.Literal("public", "internal", "restricted"),
});

const OpLogEntryWire = Schema.Struct({
  kind: Schema.Literal("upsert", "delete", "presence"),
  payload: Schema.Uint8ArrayFromSelf,
  contentKey: ContentKey,
  sequence: Schema.BigIntFromSelf,
  logical: Schema.BigIntFromSelf,
});
```
