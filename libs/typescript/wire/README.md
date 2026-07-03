# [WIRE]

`wire` is the one boundary rail to the C# wire: ALL C#-minted `*Wire` shapes decode here and nowhere else. Decode happens exactly once, INTO the owned vocabulary of the domain folder where one exists — `Hlc`/`TenantContext` into `kernel`, receipt/availability/progress evidence into `state` — and into a `wire`-owned decoded shape otherwise. Consumers reach decoded values through the `#vocab` exports subpath or ports declared at the shared vocabulary owner; the codec machinery interior is physically unresolvable, so a bundle resolves decoded values without ever resolving the transport. C# owns every wire shape and `wire` authors none — a TS re-mint of any `*Wire`, a second content-address notion, or a non-zero hash seed is the named cross-language drift defect. The fault altitude here is wire-only: `fault/detail` reconstructs the C#-minted `FaultDetail` over a closed `HopReason` vocabulary and a `fromConnect` total fold; a node rail importing `FaultDetail` for a local failure is the named defect. `frame` delegates the one `XxHash128` seed-zero mint to `kernel/identity`, never re-mints. The folder references `kernel`, `state`, and `host` only. The domain map and seam record live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

The decode surface sequences by sub-domain — codec → frame → gateway → invoke → contract → fault. Each page under `.planning/<sub-domain>/<page>.md` becomes one eventual source file.

[CODEC] — one C#-minted `*Wire` family per page, decoding INTO owned vocabulary or a `wire`-owned shape:
- [ENVELOPE](.planning/codec/envelope.md): `ReceiptEnvelope`/`HlcStamp`/`TenantContext`/`RenderReceipt` decode — the typed receipt family, never erased.
- [PROTO](.planning/codec/proto.md): protobuf suite decode + the `FaultDetail` vocabulary hook + `QuantityFamily` SI-scalar decode into the kernel `Quantity`.
- [GRAPH](.planning/codec/graph.md): `ElementGraph`/`Node`/`Relationship` content-keyed decode under the descriptor drift gate.
- [OPLOG](.planning/codec/oplog.md): `OpLog` CRDT wire decode feeding `store/journal`.
- [SNAPSHOT](.planning/codec/snapshot.md): `SnapshotHeader` canonical-CBOR decode feeding `store` snapshots.
- [CRDT](.planning/codec/crdt.md): `CrdtOpWire` MessagePack union + the 16-byte `Hlc` cell feeding `state/crdt`.
- [VERSION](.planning/codec/version.md): commit/branch/version-vector/Merkle decode feeding `state/causal`.
- [PATCH](.planning/codec/patch.md): RFC 6902 `EntityEdit` egress codec.
- [PROGRESS](.planning/codec/progress.md): `ProgressStore` stream proto + progress-mark decode feeding `state/evidence`.
- [CREDENTIAL](.planning/codec/credential.md): `CredentialPemWire` redacted-carrier decode terminating in `security/secret`.
- [CLAIM](.planning/codec/claim.md): `BenchmarkClaimWire`/`HostFingerprintWire` identity-gate decode.
- [LIVEWIRE](.planning/codec/livewire.md): `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` decode.
- [FLAG](.planning/codec/flag.md): `FlagVerdictWire` decode feeding `host/flag`.
- [CONTROL](.planning/codec/control.md): `ControlIntentWire` kind-discriminated decode.
- [LAYOUT](.planning/codec/layout.md): `LayoutConstraintWire` ordered Cassowary-program decode.
- [BCF](.planning/codec/bcf.md): `BcfTopicWire`/`BcfViewpointWire` decode.
- [GEO](.planning/codec/geo.md): `GeoFeature` WKB decode; turf owns planar ops only, in the viewer.
- [BIM](.planning/codec/bim.md): `BimWire`/`DiffWire`/`IdsAudit` golden-byte decode.
- [APPEARANCE](.planning/codec/appearance.md): `MaterialWire`/`OpenPbrGroupsWire`/`AppearanceSummary` field-for-field decode.

[FRAME] — multi-part payload reassembly with content-key verification:
- [ARTIFACT](.planning/frame/artifact.md): `ArtifactFrameWire` reassembly + content-key verify — the kernel-delegating mint site.
- [GEOMETRY](.planning/frame/geometry.md): `GeometryPayload`/`MeshTensor` frames — the GLB rail.
- [RESIDENCY](.planning/frame/residency.md): `GeometryResidencyWire` residency protocol.

[GATEWAY] — verb dispatch over decoded command payloads:
- [COMMAND](.planning/gateway/command.md): `CommandPayloadWire` dispatch + the availability gate port typed against `state/evidence/availability`.
- [SUPPORT](.planning/gateway/support.md): support-capture verb feeding `telemetry` crash.

[INVOKE] — the typed invocation client and capability SDK:
- [CAPABILITY](.planning/invoke/capability.md): `CapabilityDescriptor` decode + the capability SDK the C# `SdkTarget.TypeScript` emits.
- [CLIENT](.planning/invoke/client.md): the typed invocation client — retry/backoff budgets from `kernel/fault`; the protocol axis (connect | grpc-web) + retryable-wire schedules.

[CONTRACT] — schema-drift adjudication over C#-owned descriptors:
- [DESCRIPTOR](.planning/contract/descriptor.md): the `FileDescriptorSet` drift gate.
- [DRIFT](.planning/contract/drift.md): the drift verdict vocabulary + wire inventory.

[FAULT] — the wire-only fault altitude:
- [DETAIL](.planning/fault/detail.md): `FaultDetail` reconstruction + the closed `HopReason` hop vocabulary + the `fromConnect` total fold.
- [QUARANTINE](.planning/fault/quarantine.md): poison-frame quarantine + replay.

## [2]-[DOMAIN_PACKAGES]

Every wire-domain library the folder decodes with, planned or implemented. Versions are centralized in the one `pnpm-workspace.yaml` catalog and never pinned here; API evidence lives in the adjacent `.api/` folder.

[PROTO_TRANSPORT]:
- `@bufbuild/protobuf` — the protobuf runtime and `FileDescriptorSet` reflection backing `codec/proto`, `codec/graph`, and the `contract/descriptor` drift gate.
- `@connectrpc/connect` — the Connect protocol invocation client the `invoke/client` connect arm composes.
- `@connectrpc/connect-web` — the connect-web browser transport the `invoke/client` grpc-web arm composes.

[BINARY_CODEC]:
- `@msgpack/msgpack` — the `CrdtOpWire` MessagePack union decode on `codec/crdt`.
- `cbor-x` — the `SnapshotHeader` canonical-CBOR decode on `codec/snapshot`.

[PATCH]:
- `rfc6902` — the RFC 6902 JSON Patch `EntityEdit` egress on `codec/patch`.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate `wire` composes; the full registry and substrate contracts live in `libs/typescript/.planning/README.md` with shared API evidence in `libs/typescript/.api/`.

- `effect` — rails, `Schema`-first decode-once boundaries, `Layer`, `Match`, `Stream`, and the vocabulary substrate every codec folds through.
- `@effect/platform` — the platform service contracts the transport and channel decode rows compose against.
