# [TYPESCRIPT_WIRE_CONSUMPTION]

One page inventories every wire contract the TS campaign consumes and fixes the codegen, codec, and tolerance laws over them. The eleven TS_PROJECTION clusters across the four .NET planning corpora are the complete contract surface; TS transcribes their fences verbatim and declares no wire type beside them.

## [1]-[CONTRACT_INVENTORY]

Every cluster below is a TS_PROJECTION section on the named .NET page; the fence in that section is the authoritative shape.

| [INDEX] | [PACKAGE]    | [PAGE]                     | [CODEC]     | [WIRE_TYPES]                                                                                                                                              |
| :-----: | :----------- | :------------------------- | :---------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | AppHost      | lifecycle-and-drain        | json-stj    | RuntimePhaseKey, DrainOutcomeKey, PhaseReceiptWire, BootMarkerWire, FaultRecordWire, DrainStepWire, DrainReceiptWire                                          |
|   [2]   | AppHost      | health-and-degradation     | json-stj    | HealthStatusWire, CapabilityKey, DegradationLevelKey, HealthEntryWire, HealthSnapshotWire, DegradationWire                                                    |
|   [3]   | AppHost      | support-bundles            | json-stj    | SupportTriggerKind, SupportManifestEntry, SupportManifest, SupportReceipt                                                                                     |
|   [4]   | AppHost      | runtime-ports              | json-stj    | RasmPackage, HlcStampWire, ReceiptEnvelopeWire                                                                                                                |
|   [5]   | Persistence  | snapshot-codecs            | messagepack | SnapshotCodecKey, SnapshotCompressionKey, DataClassificationKey, SnapshotHeaderWire, SnapshotCatalogRowWire, SnapshotDeltaWire, RestoreReceiptWire, SnapshotDecodeOptions, SnapshotExtensionRows |
|   [6]   | Persistence  | sync-collaboration         | messagepack | SyncCursorWire, OpLogEntryWire, SyncSegmentWire, SyncRejectionWire, ConflictOutcomeKind, ConflictReceiptWire, PresenceRowWire                                 |
|   [7]   | Compute      | remote-lane                | proto       | StreamKind, MethodShape, TransportCapabilityWire, ComputeServiceShape, DocumentServiceShape, ControlServiceShape, ArtifactSyncShape, HealthShape, FaultDetailWire, ArtifactFrameWire |
|   [8]   | Compute      | progress-and-observation   | proto       | ProgressPhaseKey, ProgressMarkWire                                                                                                                            |
|   [9]   | Compute      | receipts-and-benchmarks    | json-stj    | ComputeReceiptKind, ComputeReceiptSpineWire, the thirteen case interfaces, ComputeReceiptWire, ComputeReceiptEnvelopeWire, HostFingerprintWire, BenchmarkClaimWire |
|  [10]   | AppUi        | commands-availability      | json-stj    | CommandPayloadWire, CommandOutcomeWire, CommandIntentWire, CommandAvailabilityWire, CommandInvocationWire, CommandReceiptWire                                 |
|  [11]   | AppUi        | diagnostics-evidence       | json-stj    | EvidenceReceiptWire, SurfaceReceiptWire, NativeAssetFactWire, SkewBandWire, EvidenceRowWire, EvidenceTimelineWire                                             |

Cross-cluster binding: every json-stj receipt payload binds as the `TPayload` type parameter on `ReceiptEnvelopeWire`, with the envelope `kind` mirroring the payload discriminator; the evidence timeline carries envelopes whole, so each payload decodes against its owning package contract.

## [2]-[CODEGEN_TOOLING]

The single-plugin codegen line generates messages and service descriptors together; `@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form. Peer pins are exact across the connect line, so the four wire packages move together in one resolve.

```yaml
version: v2
plugins:
  - local: protoc-gen-es
    out: src/gen
    include_imports: true
    opt: target=ts
```

| [INDEX] | [CONCERN]          | [SYMBOL]                                  | [LAW]                                                                                          |
| :-----: | :----------------- | :----------------------------------------- | :----------------------------------------------------------------------------------------------- |
|   [1]   | codegen input      | app-root-emitted descriptor set            | published beside the discovery manifest by `ContractGuard`; the pnpm build consumes it, never hand-copied `.proto` files |
|   [2]   | generated surface  | `*_pb.ts` with one `GenService` per service | each rpc carries `methodKind`, `input`, `output`; the five method-shape aliases in the inventory mirror it |
|   [3]   | message construction | `create` from `@bufbuild/protobuf`        | generated messages construct through `create`, never object literals cast to message types        |
|   [4]   | client factory     | `createClient(service: DescService, transport)` | one client per browser-dialable service over one shared transport; hand-written wire clients are the deleted form |
|   [5]   | browser transport  | `createGrpcWebTransport({ baseUrl })`      | binary format default; same-origin `baseUrl` under the co-hosted topology; `GrpcWebTransportOptions` carries `interceptors`, `fetch`, `defaultTimeoutMs` |
|   [6]   | call shapes        | unary as `await`, server-stream as `for await` | genuine binary server-streaming over Fetch; the text mode never enters; client-stream and bidi are structurally absent in the browser; the per-call `signal` option carries Effect interruption into transport cancellation |
|   [7]   | call stamping      | transport interceptor                      | stamps `rasm-correlation` and `traceparent` metadata, mirroring the .NET `CallSpine` constants     |

## [3]-[CODEC_ROWS]

| [INDEX] | [SURFACE]            | [CODEC]                     | [POSTURE]                                                                                       |
| :-----: | :------------------- | :--------------------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | snapshot blobs       | @msgpack/msgpack             | reused `Decoder` instance; `useBigInt64: true` aligns 64-bit integers with .NET; zero `ExtensionCodec` registrations because `SnapshotExtensionRows` is `never` — every shape crosses primitive-mapped |
|   [2]   | snapshot header      | DataView over the prefix     | `schemaFingerprint` reads through `getBigUint64`; magic, codec, and compression ids read as fixed-width fields |
|   [3]   | sync segments        | @msgpack/msgpack             | 64-bit sequence and logical fields decode as bigint; content keys cross as 16-byte binary; outcome discriminators reconstruct as literal unions |
|   [4]   | JSON runtime records | STJ Strict camelCase emission | kind-literal discriminated unions; instants as ISO-8601 text; durations as round-trip text; smart-enum columns as key scalars; correlation as guid strings; absent evidence as explicit null, never an omitted member; the decode rail is `Schema.Class` transcription of the json-stj TS_PROJECTION fences, drift-gated by the app-root-emitted JSON schema set |
|   [5]   | geometry JSON        | GeoJSON                      | the JSON projection of the proto geometry family, emitted through the Persistence `GeoJsonConverterFactory` STJ rail; feeds map and geo dashboard series directly |
|   [6]   | fault details        | generated `FaultDetail` message | extracted via `ConnectError.findDetails` with the generated `FaultDetail` descriptor — `google.rpc.Status` details ride the `grpc-status-details-bin` trailer; reconstructs `FaultDetailWire` as the literal-discriminated typed-failure union keyed by `case`; hand-parsed trailers are the deleted form |
|   [7]   | temporal proto fields | well-known Timestamp and Duration | the .NET edge owns the NodaTime conversion; TS reads the generated message fields as emitted    |

A custom .NET MessagePack extension byte pairs one-for-one with one `ExtensionCodec.register({ type, encode, decode })` row; today that set is empty by contract.

## [4]-[VERSIONING_LAW]

The .NET side classifies descriptor drift as Identical, Additive (tolerated), or Breaking (typed rejection); reserved field numbers never return to use, and generated parsers retain unknown fields. The TS client's tolerance obligations derive from that law.

| [INDEX] | [AXIS]            | [PRODUCER_LAW]                                        | [TS_TOLERANCE]                                                                       |
| :-----: | :---------------- | :----------------------------------------------------- | :------------------------------------------------------------------------------------- |
|   [1]   | proto fields      | additive numbered fields only; removals become reserved rows | unknown fields skip-decode safely; the client regenerates from the published descriptor set to surface them |
|   [2]   | proto rpcs        | one rpc row per new verb                                | absent methods are never dialed; the Capabilities verbs gate feature exposure at runtime |
|   [3]   | JSON members      | one additive member row per contract extension          | schema decode ignores excess members; an additive member lands as one field row on the owning `Schema.Class`, surfaced by the drift gate |
|   [4]   | kind literals     | one literal per new union case                          | an unknown literal folds to a quarantine case on the decode rail; the stream survives   |
|   [5]   | key scalars       | smart-enum string keys are stable identifiers           | ordinal string comparison only; display names are never re-derived from keys            |
|   [6]   | 64-bit envelopes  | `hlcLogical` resets on every physical advance           | JSON logical counters stay inside the number envelope; msgpack 64-bit fields are bigint  |
|   [7]   | breaking drift    | typed rejection at the .NET attach edge                 | the browser observes breaking drift as a `FaultDetailWire` failure, never a silent decode error |
