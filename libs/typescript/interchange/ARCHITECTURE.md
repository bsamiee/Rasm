# [INTERCHANGE_ARCHITECTURE]

`interchange` is the wire boundary as one folder: every codec is a row on one polymorphic rail family, every generated client is a row on one shared transport, and every cross-folder fact crosses through a decoded `Schema` shape or the outbound gateway. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the source tree and build order, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, boundaries, and prohibitions.

## [1]-[SOURCE_TREE]

The flat module layout IS the build order: the descriptor codegen edge precedes every runtime module, transport precedes the rails that read the descriptor registry, and the gateway precedes nothing it does not read. Each leaf is one transcription unit annotated with the owners it transcribes and the owning page#cluster.

```text codemap
interchange/
├── buf.gen.yaml + gen/             # generated descriptor set — transport#CODEGEN_TOOLING
├── transport.ts                    # WireTransport, WireClients, TransportCapabilityWire, ArtifactFrameStreaming, CapabilitySdk — transport#TRANSPORT_AND_CLIENTS, #CODEGEN_TOOLING
├── codec-rails.ts                  # DecodeRail/EncodeRail, SchemaRefinement, GeometryRail, ArtifactFrameRail, FaultDetailRail — codec-rails#CODEC_RAILS, #FAULT_FAMILY
├── gateway-and-quarantine.ts       # QuarantineFold, CONTRACT_INVENTORY, CommandGateway, IntentRegistry — gateway-and-quarantine#CONTRACT_INVENTORY, #GATEWAY_AND_QUARANTINE
└── index.ts                        # the single neutral "." export
```

The descriptor codegen edge emits `gen/` before any module compiles. `transport.ts` lands before the rails because `DecodeRail`/`ArtifactFrameRail` read the generated descriptor registry and the `ArtifactFrameStreaming` `FrameDirection` fold feeds the `ArtifactFrameRail` reassembly. `gateway-and-quarantine.ts` lands last: `QuarantineFold` terminates the decode stream and `CommandGateway` dials only the outbound verbs already declared on the transport. The TS_PROJECTION clusters carry no extra build row; they transcribe the wire-projection fences verbatim into the rails and gateway.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the folder. Implementation collapses to one owner per axis and one entrypoint family per rail; a new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]              | [OWNER]                      | [KIND]                       | [CASES]                                                                          | [PAGE#CLUSTER]                          |  [STATE]  |
| :-----: | :---------------------- | :--------------------------- | :--------------------------- | :------------------------------------------------------------------------------ | :-------------------------------------- | :-------: |
|   [1]   | outbound transport      | `WireTransport`              | Effect.Service               | one shared transport + one polymorphic interceptor stamp                          | transport#TRANSPORT_AND_CLIENTS         | FINALIZED |
|   [2]   | generated clients       | `WireClients`                | derived record               | compute/document/control/health                                                  | transport#TRANSPORT_AND_CLIENTS         | FINALIZED |
|   [3]   | transport capability    | `TransportCapabilityWire`    | two-key method-kind tuple    | http2 (4 shapes) vs grpcWeb (2 shapes)                                            | transport#TRANSPORT_AND_CLIENTS         | FINALIZED |
|   [4]   | chunked frame transport | `ArtifactFrameStreaming`     | FrameDirection fold          | server-stream-down (suspend buffer) / unary-chunked-up (sequenced await)          | transport#TRANSPORT_AND_CLIENTS         | FINALIZED |
|   [5]   | capability SDK codegen  | `CapabilitySdk`              | codegen leg + Service        | descriptor catalog / polymorphic invoke / MCP tool projection                     | transport#CODEGEN_TOOLING               | FINALIZED |
|   [6]   | byte-to-typed codec     | `DecodeRail`/`EncodeRail`    | polymorphic family           | proto/messagepack/json-stj/geometry/artifact-frame/fault-detail × decode/encode   | codec-rails#CODEC_RAILS                 | FINALIZED |
|   [7]   | decode-enforcement      | `SchemaRefinement`           | brand/filter rows            | guid/contentKey/ordinal/hlcLogical/headerDiscriminant                             | codec-rails#CODEC_RAILS                 | FINALIZED |
|   [8]   | embedded geometry       | `GeometryRail`               | rail row                     | GeoJSON projection off the proto GeometryPayload oneof                            | codec-rails#CODEC_RAILS                 | FINALIZED |
|   [9]   | content-addressed frames | `ArtifactFrameRail`         | rail row                     | 64-KiB/Crc32/XxHash128 reassembly                                                 | codec-rails#CODEC_RAILS                 | SPIKE     |
|  [10]   | exhaustive fault family | `FaultDetailRail`            | Data.TaggedError + Match     | the full fault set bound as `Match.tagsExhaustive`                                 | codec-rails#FAULT_FAMILY                | FINALIZED |
|  [11]   | contract-drift tolerance | `QuarantineFold`            | fold terminal                | unknown/disconnect/additive/breaking                                              | gateway-and-quarantine#GATEWAY_AND_QUARANTINE | FINALIZED |
|  [12]   | outbound verb face      | `CommandGateway`             | Effect.Service               | captureSupport/setDegradation/reloadOptions                                        | gateway-and-quarantine#GATEWAY_AND_QUARANTINE | FINALIZED |
|  [13]   | deep-link intents       | `IntentRegistry`             | key vocabulary               | stable string keys to gateway verb + payload                                       | gateway-and-quarantine#GATEWAY_AND_QUARANTINE | FINALIZED |

`ArtifactFrameRail` is SPIKE pending the tier-2 XxHash128 byte-identity harness; it is fully shaped now, not a deferred surface.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [FOLDER]      | [MAY_REFERENCE_INTERCHANGE] | [INTERCHANGE_MAY_REFERENCE] | [BOUNDARY]                                  |
| :-----: | :------------ | :-------------------------: | :-------------------------: | :------------------------------------------ |
|   [1]   | `projection`  |             yes             |             no              | folds the decoded `Schema` shapes verbatim   |
|   [2]   | `ui`          |             yes             |             no              | geometry through `GeometryRail`, mutation through `CommandGateway` |
|   [3]   | `platform`    |             yes             |             no              | composes the transport + gateway at the entry |
|   [4]   | `services`    |             yes             |             no              | participates over the same wire vocabulary    |

`interchange` is the inbound dependency root: it dials the wire and imports no sibling folder. Siblings consume the decoded shapes, `GeometryRail`, and `CommandGateway`; the `@connectrpc/*` import never escapes this folder.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named `interchange` cluster, consequences land at the consumer. Intra-TypeScript seams ride `pkg/page#CLUSTER`; the wire contracts the rails transcribe route through the Tier-0 seam ledger.

| [INDEX] | [SEAM]                | [MECHANICS_AT]                                | [CONSEQUENCE_AT]                                                       |
| :-----: | :-------------------- | :-------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | decoded vocabulary    | codec-rails#CODEC_RAILS                        | projection/fold-algebra#FOLD_ALGEBRA folds the decoded shapes          |
|   [2]   | geometry projection   | codec-rails#CODEC_RAILS                        | ui/render-surfaces#RENDER_SURFACES reads the `GeometryRail` GeoJSON     |
|   [3]   | artifact blob         | codec-rails#CODEC_RAILS                        | ui/render-surfaces#GLB_VIEWPORT reads the `ArtifactFrameRail` blob       |
|   [4]   | envelope payload      | gateway-and-quarantine#CONTRACT_INVENTORY      | projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE binds the carrier |
|   [5]   | outbound gateway      | gateway-and-quarantine#GATEWAY_AND_QUARANTINE  | platform/service-worker#SERVICE_WORKER redial drain, ui mutation egress  |
|   [6]   | wire contract source  | the Tier-0 seam ledger                         | every TS_PROJECTION cluster transcribes the upstream fence verbatim      |

## [5]-[BOUNDARIES]

- `interchange` is not a domain-state package, a view package, or a host package; it owns the wire boundary and nothing above it.
- The wire shape is never authored branch-side: each TS_PROJECTION cluster transcribes the upstream fence as settled vocabulary through the Tier-0 seam.
- `@connectrpc/*` stops at this folder: the dialing gateway is co-located in the transport-owning domain and never re-exported into the fold interior.
- One content-address notion only: the 16-byte `ContentKey` is the single digest; the `ArtifactFrameRail` byte-identity proof is the SPIKE residual.
- Every codec, direction, refinement, frame, and fault case is one row on its owner; the domain admits no parallel rail.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a public surface beside the budgeted owners; a parallel rail per codec is the named defect.
- NEVER a wrapper or rename adapter over the connect, protobuf, or msgpack surfaces.
- NEVER a second binary codec beside the admitted messagepack owner, or a second content-address notion beside the 16-byte `ContentKey`.
- NEVER a hand-shaped message literal or hand-serialized body where the generated `create`/`toBinary`/`Schema.encode` paths exist.
- NEVER a generic `IReceipt`-style fault abstraction replacing the typed `FaultDetailRail` family.
- NEVER a wire shape authored branch-side; the projection fences are transcribed, never re-designed.
- NEVER a `@connectrpc/*` import re-exported into the fold interior.
- NEVER a comment carrying task or process narration.
