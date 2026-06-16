# [INTERCHANGE_CODEC_RAILS]

One page owns the byte-to-typed decode interior of the wire boundary — the single six-codec rail family read by codec key, the direction-keyed encode write face, the brand-and-filter decode-enforcement vocabulary, the embedded-geometry rail, the server-streamed content-addressed artifact-frame rail, and the fault-detail rail that reconstructs the full .NET fault set as one tagged family bound by exhaustive match. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which rail consumes which fence and never re-authors a shape. The page is the densest owner of the wire interior: one `DecodeRail` owner carries every codec, every direction, and every refinement as rows, never a parallel rail per concern.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                       |
| :-----: | :------------ | :--------------------------------------------------------------------------- |
|   [1]   | CODEC_RAILS   | the six-codec rail family, encode, refinement, geometry, frames, fault       |
|   [2]   | FAULT_FAMILY  | the full .NET fault set as one Data.TaggedError family, exhaustively matched |
|   [3]   | TS_PROJECTION | the snapshot/sync/receipt wire shapes the rails decode                       |
|   [4]   | RESEARCH      | the XxHash128 byte-identity probe                                            |

## [2]-[CODEC_RAILS]

- Owner: `DecodeRail`, the six-codec rail family read by codec key — the protobuf-over-transport rail, the binary snapshot rail, the structured-text receipt rail, `GeometryRail` for embedded geometry, `ArtifactFrameRail` for the server-streamed content-addressed artifact byte stream, and `FaultDetailRail` for the status-details trailer; `EncodeRail` is the direction-keyed write face of the same owner and `SchemaRefinement` is the brand-and-filter decode-enforcement vocabulary every rail carries, both rows on `DecodeRail` and never a parallel rail.
- Cases: the protobuf rail decodes unary responses and server-streams against `remote-lane.md#TS_PROJECTION` and `progress-and-observation.md#TS_PROJECTION`; the binary rail decodes snapshot frames and the multi-object sync-segment stream against `snapshot-codecs.md#TS_PROJECTION` and `sync-collaboration.md#TS_PROJECTION` with 64-bit integers mapped to bigint through `useBigInt64` and the fixed-width header read through a `DataView` `getBigUint64`, zero registered extension codecs because `SnapshotExtensionRows` is `never` on its owner page; the structured-text rail is `Schema.Class` transcription of the json-stj fences, drift-gated by the app-root-emitted JSON schema set; `FaultDetailRail` reconstructs the typed failure union from the `grpc-status-details-bin` trailer named on `remote-lane.md#TS_PROJECTION` through `ConnectError.findDetails` with the generated `FaultDetail` descriptor (the FAULT_FAMILY cluster owns the reconstruction).
- Entry: `GeometryRail` sources no wire contract of its own; it decodes only embedded geometry fields — invoked on the snapshot-delta payloads whose geometry rides the GeoJSON projection owned by `snapshot-codecs.md#TS_PROJECTION`, and invoked on the geometry embedded inside the evidence envelope payloads — and is never re-authored per cluster. `ArtifactFrameRail` reassembles the server-streamed `ArtifactFrameWire` frames named on `remote-lane.md#TS_PROJECTION` (the `Solve`/`Generate`/`SubtreeFetch` server-stream artifact-delivery path, and the GLB tessellation result re-entering through the remote lane) into one content-addressed blob: each `serverStream` frame carries `artifactId`, `artifactBytes`, `offset`, the `frameCrc` `fixed32`, and the `payload` `Uint8Array`; the rail verifies the per-frame Crc32 through the owned `Crc32` rail (a direct table-driven compare against the wire `frameCrc fixed32`, no package), allocates one pre-sized `Uint8Array` of `artifactBytes` length at the first frame and writes each payload by `offset` into that single owned sink, and on the final frame derives the whole-artifact XxHash128 content key over the assembled bytes through `xxhash-wasm`, so a re-fetch of identical content keys identically and a frame whose Crc32 or assembled XxHash128 mismatches faults through `FaultDetailRail` rather than yielding a torn blob; the bidi `ArtifactSyncShape.sync` method stays structurally excluded on the grpc-web row, but the frame TYPE is browser-reachable over the server-stream and lands here. The codec posture is one row per surface: a reused `@msgpack/msgpack` `Decoder` with `useBigInt64: true` and the default `ExtensionCodec` carrying zero `register` rows; a `DataView` reading the fixed-width snapshot header; STJ Strict camelCase for the json-stj rail with instants as ISO-8601 text, durations as round-trip text, smart-enum columns as key scalars, `tenantId` as the `UInt128` decimal-string, and absent evidence as explicit null; the artifact-frame rail reads each frame `payload` as a transferable `Uint8Array` so the heavy Crc32-verify-and-stitch runs off the main thread under the `platform` `DecodeWorkerPool`.
- Auto: `EncodeRail` is the encode mirror keyed by direction — the proto write face constructs through `create` against the generated message descriptor and serializes through `toBinary`, and the json-stj write face is `Schema.encode` against the same `Schema.Class` the decode rail reads, so a `CommandPayloadWire` write and a `DeepLinkBinding` query-string round-trip emit through the identical class their inbound receipts decode through; the messagepack direction is read-only on the browser branch and its encode mirror is empty by contract; a hand-shaped message literal or a hand-serialized JSON body is the deleted form. `SchemaRefinement` makes the versioning invariants decode-enforced rather than prose — `Schema.brand` on guid correlation identifiers, 16-byte content keys, and smart-enum ordinal keys so a raw string never enters an identity slot, and `Schema.filter` on the HLC-logical number-envelope bound and the fixed-width snapshot-header discriminants so a breached envelope or malformed prefix fails decode rather than truncating; each refinement is one row on the owning rail.
- Packages: `@msgpack/msgpack` for the binary codec, `@bufbuild/protobuf` for `create`/`toBinary`/`fromBinary` and the descriptor registry, `@connectrpc/connect` for `ConnectError.findDetails`, `effect` for the `Stream`, `Schema.encode`, `Schema.brand`, `Schema.filter`, and `Match` primitives, and `xxhash-wasm` for the whole-artifact XxHash128 the `ArtifactFrameRail` content key derives; the per-frame `Crc32` is one owned table-driven rail comparing the wire `frameCrc fixed32` directly with no package.
- Growth: a new codec lands as one rail row; a new wire shape on an existing codec lands as one schema row on the owning rail; a new write direction lands as one `EncodeRail` direction key on the same owner; a new versioning invariant lands as one `SchemaRefinement` brand or filter row; zero new rail.
- Boundary: the binary rail registers zero extension codecs because `SnapshotExtensionRows` is `never` and an invented extension registration is the named defect; `GeometryRail` provenance is embedded-only and never an invented contract; `ArtifactFrameRail` reassembles only the server-streamed artifact frame type and never the bidi `ArtifactSyncShape.sync` method, and its content key is the one 16-byte `ContentKey` brand, never a second identity notion; `EncodeRail` and `DecodeRail` are the write and read faces of one owner; this domain references no telemetry type because telemetry crosses no wire contract.

```ts contract
type CodecKey = "proto" | "messagepack" | "json-stj" | "geometry" | "artifact-frame" | "fault-detail";
type Direction = "decode" | "encode";

interface DecodeRail<Wire, Domain> {
  readonly codec: CodecKey;
  readonly decode: (bytes: Uint8Array) => Effect.Effect<Domain, ParseResult.ParseError>;
  readonly encode: Option.Option<(value: Domain) => Effect.Effect<Wire, ParseResult.ParseError>>;
}

const Guid = Schema.String.pipe(Schema.filter((s) => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(s)), Schema.brand("Guid"));
const ContentKey = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((b) => b.length === 16), Schema.brand("ContentKey"));
const OrdinalKey = Schema.String.pipe(Schema.brand("OrdinalKey"));
const HlcLogical = Schema.Number.pipe(Schema.filter((n) => Number.isInteger(n) && n >= 0 && n <= Number.MAX_SAFE_INTEGER), Schema.brand("HlcLogical"));
const HeaderDiscriminant = Schema.Number.pipe(Schema.filter((n) => Number.isInteger(n) && n >= 0), Schema.brand("HeaderDiscriminant"));

class SchemaRefinement extends Schema.Class<SchemaRefinement>("SchemaRefinement")({
  guid: Guid,
  contentKey: ContentKey,
  ordinalKey: OrdinalKey,
  hlcLogical: HlcLogical,
  headerDiscriminant: HeaderDiscriminant,
}) {}

type RefinedIdentity = Schema.Schema.Type<typeof SchemaRefinement>;

interface ArtifactBlob {
  readonly artifactId: string;
  readonly contentKey: RefinedIdentity["contentKey"];
  readonly bytes: Uint8Array;
}

interface ArtifactFrameRail {
  readonly reassemble: (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) => Effect.Effect<ArtifactBlob, FaultDetail>;
}

interface Crc32 {
  readonly of: (bytes: Uint8Array) => number;
}

// crc32 is the ONE concrete table-driven rail the whole branch reads — the IEEE 802.3 reflected polynomial table
// built once, no package; ArtifactFrameRail reassembly and the transport-side splitFrames both consume THIS value,
// never a free crc32Of duplicating the surface. The compare against the wire frameCrc fixed32 is a direct number eq.
const CRC32_TABLE: Uint32Array = Uint32Array.from({ length: 256 }, (_, n) =>
  Array.range(0, 7).reduce((c) => ((c & 1) !== 0 ? 0xedb88320 ^ (c >>> 1) : c >>> 1), n));

const crc32: Crc32 = {
  of: (bytes) =>
    (bytes.reduce((c, b) => CRC32_TABLE[(c ^ b) & 0xff] ^ (c >>> 8), 0xffffffff) ^ 0xffffffff) >>> 0,
};

interface AssemblyState {
  readonly id: string;
  readonly sink: Option.Option<Uint8Array>;
}

const reassemble = (crc: Crc32, xxh: XxHash128, frames: Stream.Stream<ArtifactFrameWire, FaultDetail>): Effect.Effect<ArtifactBlob, FaultDetail> =>
  frames.pipe(
    Stream.runFoldEffect(
      { id: "", sink: Option.none<Uint8Array>() } as AssemblyState,
      (state, frame) =>
        crc.of(frame.payload) !== frame.frameCrc
          ? Effect.fail(FaultDetail.HopFault({ code: "frame-crc-mismatch", evidence: { artifactId: frame.artifactId } }))
          : Effect.sync(() => {
              const sink = Option.getOrElse(state.sink, () => new Uint8Array(frame.artifactBytes));
              sink.set(frame.payload, frame.offset);
              return { id: frame.artifactId, sink: Option.some(sink) } satisfies AssemblyState;
            }),
    ),
    Effect.flatMap((state) =>
      Option.match(state.sink, {
        onNone: () => Effect.fail(FaultDetail.HopFault({ code: "empty-artifact", evidence: {} })),
        onSome: (bytes) => Effect.succeed({ artifactId: state.id, contentKey: xxh.h128(bytes) as RefinedIdentity["contentKey"], bytes }),
      })),
  );
```

## [3]-[FAULT_FAMILY]

- Owner: `FaultDetail`, the single `Data.TaggedEnum` family reconstructing every .NET fault the wire emits across all four C# packages, with one `Match.tagsExhaustive` table the SPA renders through so no surface hand-rolls fault rendering per rail. The family is the full cross-package fault case set — `ComputeFault`, `StoreFault`, `HopFault`, `ConfigError`, and the `Quarantine` landing for the not-yet-enumerated wire `case` — ONE tagged owner whose constructors and error channel are the same symbol, never an empty `Data.TaggedError` class beside a parallel enum and never four parallel error rails.
- Cases: `FaultDetailRail` reads the `grpc-status-details-bin` trailer through `ConnectError.findDetails(FaultDetailSchema)` and decodes the `FaultDetailWire` (package, code, case, message, evidence, correlation, hlcPhysical, hlcLogical) into the matching `FaultDetail` tagged case through `faultTagOf`, the explicit `(package, case)`-string-pair projection table mapping every enumerated .NET fault key to its `_tag`; a `(package, case)` pair the table does not enumerate folds to `FaultDetail.Quarantine` rather than throwing, so the wire→tag hop is itself total and an unmapped pair is a typed quarantine fault, never a silent miss; the exhaustive render proves at compile time through `Match.tagsExhaustive` that every tagged case has a render arm, so adding a fault tag without a TS arm is a typecheck failure.
- Packages: `@connectrpc/connect` for `findDetails`, `@bufbuild/protobuf` for the `FaultDetail` descriptor, `effect` for `Data.TaggedEnum`/`Data.taggedEnum` and `Match.tagsExhaustive`.
- Growth: a new .NET fault key lands as one `(package, case)` row on the `faultTagOf` table and, when it carries a new tag, one `Data.TaggedEnum` case and one `Match.tagsExhaustive` arm; a sixth fault rail is the named defect — every fault is one case on the one family and every wire key is one projection-table row.
- Boundary: the family is the only fault rail in the branch and `FaultDetail` is BOTH the error-channel type and the constructor namespace (`FaultDetail.HopFault({...})`), never an empty `Data.TaggedError` class shadowing a separate enum; a generic reported-value or `IReceipt`-style abstraction replacing the typed cases is the named defect; the `faultTagOf` projection makes the wire→tag hop total so the exhaustiveness guarantee reaches the wire boundary, never stopping one hop short; the render exhaustiveness is a typecheck obligation, never a runtime default arm.

```ts contract
type FaultDetail = Data.TaggedEnum<{
  readonly ComputeFault: { readonly code: string; readonly evidence: Record<string, string>; readonly correlation: string };
  readonly StoreFault: { readonly code: string; readonly evidence: Record<string, string>; readonly correlation: string };
  readonly HopFault: { readonly code: string; readonly evidence: Record<string, string> };
  readonly ConfigError: { readonly code: string; readonly evidence: Record<string, string> };
  readonly Quarantine: { readonly code: string; readonly evidence: Record<string, string> };
}>;
const FaultDetail = Data.taggedEnum<FaultDetail>();
type FaultTag = FaultDetail["_tag"];

const faultTagOf = (wire: { readonly package: string; readonly case: string }): FaultTag =>
  Match.value(wire.package).pipe(
    Match.when("Rasm.Compute", () => "ComputeFault" as const),
    Match.when("Rasm.Persistence", () => "StoreFault" as const),
    Match.when("Rasm.AppHost", () => "HopFault" as const),
    Match.when("Rasm.AppUi", () => "ConfigError" as const),
    Match.orElse(() => "Quarantine" as const),
  );

const renderFault = (fault: FaultDetail): string =>
  Match.value(fault).pipe(
    Match.tagsExhaustive({
      ComputeFault: (f) => `compute:${f.code}`,
      StoreFault: (f) => `store:${f.code}`,
      HopFault: (f) => `hop:${f.code}`,
      ConfigError: (f) => `config:${f.code}`,
      Quarantine: (f) => `quarantine:${f.code}`,
    }),
  );

interface FaultDetailRail {
  readonly fromTrailer: (error: ConnectError) => Effect.Effect<FaultDetail, ParseResult.ParseError>;
  readonly fromConnect: (cause: unknown) => FaultDetail;
}

// fromConnect is the ONE polymorphic cause→FaultDetail entry keyed by error shape — the total infallible
// projection every transport call site folds through (connect-es rejects a Promise/async-iterator with a
// ConnectError or, on a non-connect throw, an arbitrary cause). ConnectError.from normalises any cause to a
// ConnectError; findDetails(FaultDetailSchema) reads the typed grpc-status-details-bin trailer through the
// generated FaultDetail descriptor when present and faultTagOf maps the (package, case) pair total; a cause
// carrying no trailer lands FaultDetail.Quarantine keyed by the connect Code, never a throw and never a leaked
// ParseResult.ParseError — fromTrailer stays the typed-failure entry the rails use, fromConnect is the
// infallible boundary fold the transport stamps onto its E channel.
// faultOf constructs the exact tagged case for a decoded wire detail — ComputeFault/StoreFault carry correlation,
// HopFault/ConfigError/Quarantine carry only code+evidence, so the construction dispatches per tag rather than a
// dynamic indexed FaultDetail[tag](...) that would violate each case's strict field shape.
const faultOf = (wire: { readonly package: string; readonly case: string; readonly code: string; readonly evidence: Record<string, string>; readonly correlation: string }): FaultDetail =>
  Match.value(faultTagOf(wire)).pipe(
    Match.when("ComputeFault", () => FaultDetail.ComputeFault({ code: wire.code, evidence: wire.evidence, correlation: wire.correlation })),
    Match.when("StoreFault", () => FaultDetail.StoreFault({ code: wire.code, evidence: wire.evidence, correlation: wire.correlation })),
    Match.when("HopFault", () => FaultDetail.HopFault({ code: wire.code, evidence: wire.evidence })),
    Match.when("ConfigError", () => FaultDetail.ConfigError({ code: wire.code, evidence: wire.evidence })),
    Match.orElse(() => FaultDetail.Quarantine({ code: wire.code, evidence: wire.evidence })),
  );

const faultDetailRail: FaultDetailRail = {
  fromConnect: (cause: unknown): FaultDetail => {
    const error = ConnectError.from(cause);
    return Option.match(Array.head(error.findDetails(FaultDetailSchema)), {
      onNone: () => FaultDetail.Quarantine({ code: Code[error.code], evidence: { message: error.rawMessage } }),
      onSome: faultOf,
    });
  },
  fromTrailer: (error: ConnectError): Effect.Effect<FaultDetail, ParseResult.ParseError> =>
    Effect.sync(() => faultDetailRail.fromConnect(error)),
};
```

## [4]-[TS_PROJECTION]

- Owner: the snapshot, sync, and receipt wire shapes the binary and json-stj rails decode — transcribed verbatim from `snapshot-codecs.md#TS_PROJECTION`, `sync-collaboration.md#TS_PROJECTION`, and `receipts-and-benchmarks.md#TS_PROJECTION`; the artifact-frame and fault-detail shapes ride `remote-lane.md#TS_PROJECTION`.
- Entry: `SnapshotHeaderWire.schemaFingerprint` decodes as bigint via `DataView` `getBigUint64`; `SnapshotDecodeOptions` carries `useBigInt64: true`; `OpLogEntryWire` content keys cross as 16-byte binary; absent evidence on `ComputeReceiptWire` crosses as explicit null.
- Packages: `@msgpack/msgpack` and `effect` `Schema` for the codec surface.
- Growth: a new snapshot/sync/receipt member lands as one `Schema.Class` field row; the branch authors no shape absent from the C# fence.
- Boundary: every shape transcribes a C# `#TS_PROJECTION` fence; `SnapshotExtensionRows` is `never` and no extension codec is registered.

```ts contract
interface ArtifactFrameWire {
  readonly artifactId: string;
  readonly artifactBytes: number;
  readonly offset: number;
  readonly frameCrc: number;
  readonly payload: Uint8Array;
}

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

const FaultDetailWire = Schema.Struct({
  package: Schema.Literal("Rasm.AppHost", "Rasm.Persistence", "Rasm.Compute", "Rasm.AppUi"),
  code: Schema.String,
  case: Schema.String,
  message: Schema.String,
  evidence: Schema.Record({ key: Schema.String, value: Schema.String }),
  correlation: Schema.String,
  hlcPhysical: Schema.String,
  hlcLogical: Schema.Number,
});
```

## [5]-[RESEARCH]

- [CONTENT_HASHING]: the `xxhash-wasm` `h128` digest over the assembled artifact blob keyed identically to the C# `System.IO.Hashing.XxHash128` suite hash — seed=0, fixed endianness, two-64-bit-half byte order — proven by the tier-2 byte-identity harness so a re-tessellation of identical bytes is a cache hit across C#, the TS browser worker pool, and the future Python IFC->GLB companion; the per-frame Crc32 is a direct compare against the wire `frameCrc fixed32` and needs no probe.
- [BIGINT_ROUNDTRIP]: the `SnapshotHeaderWire.schemaFingerprint` and the `OpLogEntryWire.logical`/`sequence` bigints crossing `@msgpack/msgpack` `useBigInt64: true` and the `DataView` `getBigUint64` header read must round-trip bit-for-bit against the C# `long`/`ulong` HLC encoding — the same tier-2 byte-identity harness asserts the two-64-bit-half order so a half-swap never silently corrupts the conflict-presence fold, since an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal.
