# [INTERCHANGE_PLANNING]

`interchange` owns the entire byte-to-typed-and-back wire boundary of the TS branch — the single grpc-web transport over four browser-dialable generated services, the six-codec polymorphic decode/encode rail family, the content-addressed artifact-frame reassembly, the exhaustive .NET-fault reconstruction, the contract-drift quarantine, and the outbound command gateway. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. This is the inbound dependency root of the whole branch (descriptor set -> wire -> everything) and the SINGLE boundary at which the eleven C#->TS `#TS_PROJECTION` fences are transcribed verbatim — the domain authors no wire shape.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                    | [OWNS]                                                                                                               | [STATE]     |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------------------------- | :---------- |
|   [1]   | transport.md              | WireTransport + WireClients + TransportCapabilityWire + ArtifactFrameStreaming + CapabilitySdk + the buf codegen edge | provisional |
|   [2]   | codec-rails.md            | the six-codec DecodeRail family + EncodeRail + SchemaRefinement + GeometryRail + ArtifactFrameRail + FaultDetailRail | provisional |
|   [3]   | gateway-and-quarantine.md | QuarantineFold + CONTRACT_INVENTORY + CommandGateway + IntentRegistry                                                | provisional |

## [2]-[WIRE_PAGES]

Each page carries exactly one TS_PROJECTION cluster where it transcribes a C# `#TS_PROJECTION` fence; the domain authors no shape.

- transport.md: Compute/remote-lane#TS_PROJECTION (proto service shapes, `TransportCapabilityWire`) + the capability-descriptor codegen REFINEMENT_HORIZON gated on C# capability-registry.
- codec-rails.md: Persistence/snapshot-codecs + sync-collaboration (messagepack), Compute/remote-lane fault+artifact + receipts-and-benchmarks (json-stj), the cross-package envelope payload bindings; the full ComputeFault/StoreFault/HopFault/ConfigError fault set bound as `Match.tagsExhaustive`.
- gateway-and-quarantine.md: AppUi/commands-availability + AppHost/support-bundles + the AppUi diagnostics-evidence and AppHost runtime-ports/lifecycle/health/support envelope.

## [3]-[CATALOGUE_PENDING]

- @bufbuild/buf: catalogued with `allowBuilds: true`; the build-time descriptor codegen CLI driving `buf generate`, never a runtime import.
- xxhash-wasm: catalogued; the browser-reachable XxHash128 digest the `ArtifactFrameRail` `ContentKey` derives — landing condition is the tier-2 byte-identity harness against C# `System.IO.Hashing.XxHash128`.
- The capability-descriptor codegen row: catalogue-pending, gated on the C# `capability-registry#CAPABILITY_CATALOG` descriptor; until it lands the codegen emits no capability SDK.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                              | [CLOSED_BY (page#cluster)]                                                                        |
| :-----: | :----------------------------------------------------------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | the six-codec rail proliferating into a parallel rail per codec    | codec-rails#CODEC_RAILS (one DecodeRail, codec key)                                               |
|   [2]   | a SPA hand-rolling .NET-fault rendering per rail                   | codec-rails#CODEC_RAILS (FaultDetailRail Match.tagsExhaustive over the full fault set)            |
|   [3]   | the content-addressed cache silently diverging across C#/TS/Python | codec-rails#CODEC_RAILS (ArtifactFrameRail ContentKey byte-identity, SPIKE)                       |
|   [4]   | an unrecognized wire shape tearing the stream                      | gateway-and-quarantine#GATEWAY_AND_QUARANTINE (QuarantineFold)                                    |
|   [5]   | the dialing gateway leaking @connectrpc into the fold interior     | gateway-and-quarantine#GATEWAY_AND_QUARANTINE (gateway co-located in the transport-owning domain) |

## [5]-[DENSITY_BAR]

A new feature is a row or case, never a new surface. The owner-count budget folds every extension and mapping descriptor under its axis owner.

| [INDEX] | [AXIS/CONCERN]           | [OWNER]                   | [KIND]                   | [CASES]                                                                         | [STATE]   |
| :-----: | :----------------------- | :------------------------ | :----------------------- | :------------------------------------------------------------------------------ | :-------- |
|   [1]   | outbound transport       | `WireTransport`           | Effect.Service           | one shared transport + one polymorphic interceptor stamp                        | FINALIZED |
|   [2]   | generated clients        | `WireClients`             | derived record           | compute/document/control/health                                                 | FINALIZED |
|   [3]   | transport capability     | `TransportCapabilityWire` | two-key method-kind tuple | http2 (4 shapes) vs grpcWeb (2 shapes)                                          | FINALIZED |
|  [3a]   | chunked frame transport  | `ArtifactFrameStreaming`  | FrameDirection fold      | server-stream-down (suspend buffer) / unary-chunked-up (sequenced await)         | FINALIZED |
|  [3b]   | capability SDK codegen   | `CapabilitySdk`           | codegen leg + Service    | descriptor catalog / polymorphic invoke / MCP tool projection                    | FINALIZED |
|   [4]   | byte-to-typed codec      | `DecodeRail`/`EncodeRail` | polymorphic family       | proto/messagepack/json-stj/geometry/artifact-frame/fault-detail × decode/encode | FINALIZED |
|   [5]   | decode-enforcement       | `SchemaRefinement`        | brand/filter rows        | guid/contentKey/ordinal/hlcLogical/headerDiscriminant                           | FINALIZED |
|   [6]   | embedded geometry        | `GeometryRail`            | rail row                 | GeoJSON projection off the proto GeometryPayload oneof                          | FINALIZED |
|   [7]   | content-addressed frames | `ArtifactFrameRail`       | rail row                 | 64-KiB/Crc32/XxHash128 reassembly                                               | SPIKE     |
|   [8]   | exhaustive fault family  | `FaultDetailRail`         | Data.TaggedError + Match | ComputeFault/StoreFault/HopFault/ConfigError full set                           | FINALIZED |
|   [9]   | contract-drift tolerance | `QuarantineFold`          | fold terminal            | unknown/disconnect/additive/breaking                                            | FINALIZED |
|  [10]   | outbound verb face       | `CommandGateway`          | Effect.Service           | captureSupport/setDegradation/reloadOptions                                     | FINALIZED |
|  [11]   | deep-link intents        | `IntentRegistry`          | key vocabulary           | stable string keys -> gateway verb + payload                                    | FINALIZED |

`ArtifactFrameRail` is SPIKE pending the tier-2 XxHash128 byte-identity harness; it is fully shaped now, not a deferred surface.

## [6]-[BUILD_ORDER]

The descriptor codegen edge precedes every runtime module; transport precedes the rails that read the descriptor registry; the gateway precedes nothing it does not read.

| [INDEX] | [FILE]                        | [TRANSCRIBES]                                      | [GATE]                     |
| :-----: | :---------------------------- | :------------------------------------------------- | :------------------------- |
|   [1]   | buf.gen.yaml + gen/           | transport#CODEGEN_TOOLING                          | buf generate emits src/gen |
|   [2]   | src/transport.ts              | transport#TRANSPORT_AND_CLIENTS + #CODEGEN_TOOLING | tsgo --noEmit clean        |
|   [3]   | src/codec-rails.ts            | codec-rails#CODEC_RAILS                            | tsgo + XxHash128 harness   |
|   [4]   | src/gateway-and-quarantine.ts | gateway-and-quarantine (all three clusters)        | tsgo + unit-pbt specs      |
|   [5]   | src/index.ts                  | the single neutral "." export                      | exports resolve            |

## [7]-[PROOF_GATES]

| [GATE]            | [COMMAND]                                      | [EVIDENCE]                                      |
| :---------------- | :--------------------------------------------- | :---------------------------------------------- |
| catalog resolve   | `pnpm install`                                 | catalogMode strict resolves @rasm/ts            |
| codegen           | `pnpm --filter @rasm/ts buf:generate`          | interchange/gen/*_pb.ts present                 |
| typecheck         | tsgo `--noEmit` over the domain                | zero diagnostics                                |
| unit-pbt          | vitest project `interchange`                   | codec-fold and frame-stitch algebraic laws pass |
| content-key spike | tier-2 XxHash128 harness                       | byte-identity vs C# XxHash128 seed=0            |

## [8]-[PROHIBITIONS]

No new public surface beside the eleven budgeted owners; no wrapper or rename adapter over connect-es/protobuf/msgpack; no second binary codec beside @msgpack/msgpack; no second content-address notion beside the 16-byte `ContentKey`; no hand-shaped message literal or hand-serialized JSON body where `create`/`toBinary`/`Schema.encode` exist; no generic `IReceipt`-style fault abstraction replacing the typed `FaultDetailRail` family; no wire shape authored branch-side; no `@connectrpc/*` import re-exported into the fold interior; no comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]               | [PAGE]                    | [CATALOGUE]        | [STATUS] |
| :-----: | :---------------------- | :------------------------ | :----------------- | :------- |
|   [1]   | @connectrpc/connect     | transport.md              | api-transport-wire | admitted |
|   [2]   | @connectrpc/connect-web | transport.md, gateway     | api-transport-wire | admitted |
|   [3]   | @bufbuild/protobuf      | transport.md, codec-rails | api-transport-wire | admitted |
|   [4]   | @bufbuild/protoc-gen-es | transport.md              | api-transport-wire | admitted |
|   [5]   | @bufbuild/buf           | transport.md              | api-transport-wire | admitted |
|   [6]   | @msgpack/msgpack        | codec-rails.md            | api-transport-wire | admitted |
|   [7]   | xxhash-wasm             | codec-rails.md            | api-transport-wire | admitted |
|   [8]   | rfc6902                 | codec-rails.md            | api-transport-wire | admitted |
|   [9]   | effect                  | all pages                 | api-effect         | admitted |

## [10]-[REFINEMENT_HORIZON]

The chunked frame streaming row and the capability-descriptor codegen row are now LANDED on `transport.md`: the `ArtifactFrameStreaming` `FrameDirection` fold owns the transport-level chunk boundary and backpressure (server-stream-down `Stream.buffer` suspend, unary-chunked-up sequenced await) feeding the `codec-rails#ArtifactFrameRail` reassembly, and the `CapabilitySdk` codegen leg consumes the C# `capability-registry#SDK_CODEGEN` `DiscoveryResultWire[]` descriptor as a SECOND plugin on the same `buf.gen.yaml` emitting a typed effect-classed capability command surface plus the MCP tool projection — both DERIVED from the C# descriptor, never hand-authored. The remaining horizon is deeper standing-query windowing over the assembled frame stream. Closed by the bar: every codec, direction, refinement, frame, and fault case is one row on its owner, and the domain admits no parallel rail.
