# [TYPESCRIPT_WIRE_CONSUMPTION]

One page inventories every wire contract the TS campaign consumes and fixes the codegen, codec, and tolerance laws over them. The eleven TS_PROJECTION clusters across the four .NET planning corpora are the complete contract surface; TS transcribes their fences verbatim and declares no wire type beside them.

## [1]-[CONTRACT_INVENTORY]

Each entry names the owning .NET page and its codec; the TS_PROJECTION fence on that page is the authoritative shape.

[APPHOST]:
- lifecycle-and-drain (json-stj): `RuntimePhaseKey`, `DrainOutcomeKey`, `PhaseReceiptWire`, `BootMarkerWire`, `FaultRecordWire`, `DrainStepWire`, `DrainReceiptWire`
- health-and-degradation (json-stj): `HealthStatusWire`, `CapabilityKey`, `DegradationLevelKey`, `HealthEntryWire`, `HealthSnapshotWire`, `DegradationWire`
- support-bundles (json-stj): `SupportTriggerKind`, `SupportManifestEntry`, `SupportManifest`, `SupportReceipt`
- runtime-ports (json-stj): `RasmPackage`, `HlcStampWire`, `ReceiptEnvelopeWire`

[PERSISTENCE]:
- snapshot-codecs (messagepack): `SnapshotCodecKey`, `SnapshotCompressionKey`, `DataClassificationKey`, `SnapshotHeaderWire`, `SnapshotCatalogRowWire`, `SnapshotDeltaWire`, `RestoreReceiptWire`, `SnapshotDecodeOptions`, `SnapshotExtensionRows`
- sync-collaboration (messagepack): `SyncCursorWire`, `OpLogEntryWire`, `SyncSegmentWire`, `SyncRejectionWire`, `ConflictOutcomeKind`, `ConflictReceiptWire`, `PresenceRowWire`

[COMPUTE]:
- remote-lane (proto): `StreamKind`, `MethodShape`, `TransportCapabilityWire`, `ComputeServiceShape`, `DocumentServiceShape`, `ControlServiceShape`, `ArtifactSyncShape`, `HealthShape`, `FaultDetailWire`, `ArtifactFrameWire`
- progress-and-observation (proto): `ProgressPhaseKey`, `ProgressMarkWire`
- receipts-and-benchmarks (json-stj): `ComputeReceiptKind`, `ComputeReceiptSpineWire`, the thirteen case interfaces, `ComputeReceiptWire`, `ComputeReceiptEnvelopeWire`, `HostFingerprintWire`, `BenchmarkClaimWire`

[APPUI]:
- commands-availability (json-stj): `CommandPayloadWire`, `CommandOutcomeWire`, `CommandIntentWire`, `CommandAvailabilityWire`, `CommandInvocationWire`, `CommandReceiptWire`
- diagnostics-evidence (json-stj): `EvidenceReceiptWire`, `SurfaceReceiptWire`, `NativeAssetFactWire`, `SkewBandWire`, `EvidenceRowWire`, `EvidenceTimelineWire`

Cross-cluster binding: every json-stj receipt payload binds as the `TPayload` type parameter on `ReceiptEnvelopeWire`, with the envelope `kind` mirroring the payload discriminator; the evidence timeline carries envelopes whole, so each payload decodes against its owning package contract. Telemetry signals cross no wire contract: the AppHost OTLP exporter ships them from the app roots to the collector, and dashboards read the collector, never a bespoke wire.

## [2]-[CODEGEN_TOOLING]

The codegen line is one descriptor pipeline with four ordered stages: the app-root-emitted descriptor set is the input, the buf generation pass runs the single message-and-service plugin over it, the generated descriptor module carries one service descriptor per wire service, and the connect runtime derives one client per browser-dialable service from those descriptors over one shared transport. The single-plugin line generates messages and service descriptors together; `@connectrpc/protoc-gen-connect-es` is the rejected separate-plugin form. Peer pins are exact across the connect line, so the four wire packages move together in one resolve.

```yaml
version: v2
inputs:
  - directory: gen/descriptors
plugins:
  - local: protoc-gen-es
    out: src/gen
    include_imports: true
    opt: target=ts
```

[PIPELINE_STAGES]:
- Descriptor ingestion: the codegen input is the app-root-emitted `FileDescriptorSet`, published beside the discovery manifest by the C# `ContractGuard`; the buf input row points at the committed descriptor directory rather than a `.proto` source tree, so the TS branch consumes the same descriptor set the .NET side emits and never re-authors or hand-copies a `.proto` file. The `include_imports` row embeds every transitively imported descriptor in the generated output so the registry resolves the full type graph from one module.
- Generation pass: `buf generate` is the build-time driver — a tool surface, never a runtime import — invoking the single `protoc-gen-es` plugin with the `target=ts` option so the output is `.ts`, not the `.js`/`.d.ts` split. The generated surface is `*_pb.ts` modules, one `GenService` descriptor per service and one message descriptor per message, each rpc carrying its `methodKind`, `input`, and `output`; the five method-shape aliases in the inventory mirror the descriptor `methodKind` exactly.
- Descriptor runtime: the generated descriptors are the protobuf-runtime values the consumer composes — message construction goes through `create` from the protobuf runtime against the message descriptor, never an object literal cast to a message type; binary read and write go through the schema-anchored `fromBinary`/`toBinary` against the same descriptor; and the file-aware registry constructed from the emitted `FileDescriptorSet` is the lookup the fault rail passes to `findDetails` to resolve `google.rpc.Status` detail messages by type name. Unknown fields are retained by the descriptor runtime by default, which is the additive-drift tolerance the versioning law relies on.
- Client derivation: `createClient(service, transport)` derives one typed client per browser-dialable service descriptor over one shared transport; the return maps each unary rpc to a `Promise` and each server-streaming rpc to an `AsyncIterable`. Hand-written wire clients are the deleted form, and the descriptor is the single source the client shape derives from.

[TRANSPORT_LAWS]:
- Browser transport: `createGrpcWebTransport({ baseUrl })` with the binary format default and same-origin `baseUrl` under the co-hosted topology; `GrpcWebTransportOptions` carries `interceptors`, `fetch`, and `defaultTimeoutMs`.
- Call shapes: unary as `await`, server-stream as `for await` — genuine binary server-streaming over Fetch, and the text mode never enters; client-stream and bidi are structurally absent in the browser; the per-call `signal` option carries Effect interruption into transport cancellation.
- Call stamping: a transport interceptor stamps `rasm-correlation` and `traceparent` metadata, mirroring the .NET `CallSpine` constants.

## [3]-[CODEC_ROWS]

One codec posture per wire surface:

- Snapshot blobs: one reused `@msgpack/msgpack` `Decoder` instance; `useBigInt64: true` aligns 64-bit integers with .NET; zero `ExtensionCodec` registrations because `SnapshotExtensionRows` is `never` — every shape crosses primitive-mapped.
- Snapshot header: a `DataView` over the prefix; `schemaFingerprint` reads through `getBigUint64`, and magic, codec, and compression ids read as fixed-width fields.
- Sync segments: `@msgpack/msgpack` decode; 64-bit sequence and logical fields decode as bigint, content keys cross as 16-byte binary, and outcome discriminators reconstruct as literal unions.
- JSON runtime records: STJ Strict camelCase emission — kind-literal discriminated unions, instants as ISO-8601 text, durations as round-trip text, smart-enum columns as key scalars, correlation as guid strings, absent evidence as explicit null and never an omitted member. The decode rail is `Schema.Class` transcription of the json-stj TS_PROJECTION fences, drift-gated by the app-root-emitted JSON schema set.
- Geometry JSON: the GeoJSON projection of the proto geometry family, emitted through the Persistence `GeoJsonConverterFactory` STJ rail; feeds the map and geo dashboard series directly.
- Fault details: `ConnectError.findDetails` with the generated `FaultDetail` descriptor extracts the `google.rpc.Status` details riding the `grpc-status-details-bin` trailer and reconstructs `FaultDetailWire` as the literal-discriminated typed-failure union keyed by `case`; hand-parsed trailers are the deleted form.
- Temporal proto fields: well-known Timestamp and Duration read as emitted; the .NET edge owns the NodaTime conversion.

A custom .NET MessagePack extension byte pairs one-for-one with one `ExtensionCodec.register({ type, encode, decode })` row; the set is empty by contract.

## [4]-[VERSIONING_LAW]

The .NET side classifies descriptor drift as Identical, Additive (tolerated), or Breaking (typed rejection); reserved field numbers never return to use, and generated parsers retain unknown fields. The TS client's tolerance obligations derive from that law:

- Proto fields: the producer adds numbered fields only, and removals become reserved rows; unknown fields skip-decode safely, and the client regenerates from the published descriptor set to surface them.
- Proto rpcs: one rpc row per new verb; absent methods are never dialed, and the Capabilities verbs gate feature exposure at runtime.
- JSON members: one additive member row per contract extension; schema decode ignores excess members, and an additive member lands as one field row on the owning `Schema.Class`, surfaced by the drift gate.
- Kind literals: one literal per new union case; an unknown literal folds to a quarantine case on the decode rail, and the stream survives.
- Key scalars: smart-enum string keys are stable identifiers; comparison is ordinal string only, and display names are never re-derived from keys.
- 64-bit envelopes: `hlcLogical` resets on every physical advance; JSON logical counters stay inside the number envelope, and msgpack 64-bit fields are bigint.
- Breaking drift: typed rejection at the .NET attach edge; the browser observes it as a `FaultDetailWire` failure, never a silent decode error.
