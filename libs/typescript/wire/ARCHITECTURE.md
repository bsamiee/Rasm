# [WIRE_ARCHITECTURE]

The domain map of `wire` — the W2 boundary rail that decodes every C#-minted `*Wire` shape and authors none. The `codec`, `frame`, `gateway`, `invoke`, `contract`, and `fault` sub-domains each land at a narrow surface: `codec` holds one row per wire family decoding INTO owned vocabulary or a `wire`-owned shape, `frame` reassembles content-keyed payloads, `gateway` dispatches decoded commands, `invoke` carries the typed client and capability SDK, `contract` adjudicates schema drift over C#-owned descriptors, and `fault` reconstructs the wire-only `FaultDetail`. The folder references `kernel`, `state`, and `host` only; the permitted-edge ledger, tag law, and port registry are the branch `composition-system.md`.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
wire/ # W2 runtime; refs kernel, state, host ONLY; ALL C#-minted *Wire decode, authors none; #vocab publishes decoded values, the codec interior is unexported
├── codec/ # One C#-minted *Wire family per page — decode INTO owned vocabulary where an owner exists, own the decoded shape otherwise
│ ├── envelope.ts # ReceiptEnvelope/HlcStamp/TenantContext/RenderReceipt decode — the typed receipt family, never erased into one shape
│ ├── proto.ts # protobuf suite decode + the FaultDetail vocabulary hook + QuantityFamily SI-scalar decode into the kernel Quantity
│ ├── graph.ts # ElementGraph/Node/Relationship content-keyed decode under the contract/descriptor drift gate
│ ├── oplog.ts # OpLog CRDT wire decode → store/journal
│ ├── snapshot.ts # SnapshotHeader canonical-CBOR decode → store snapshots
│ ├── crdt.ts # CrdtOpWire MessagePack union + the 16-byte Hlc cell → state/crdt
│ ├── version.ts # commit/branch/version-vector/Merkle decode → state/causal
│ ├── patch.ts # RFC 6902 EntityEdit egress codec
│ ├── progress.ts # ProgressStore stream proto + progress-mark decode → state/evidence
│ ├── credential.ts # CredentialPemWire redacted-carrier decode → security/secret
│ ├── claim.ts # BenchmarkClaimWire/HostFingerprintWire identity-gate decode
│ ├── livewire.ts # BindingStatusWire/CoercedValueWire/WriteReceiptWire decode
│ ├── flag.ts # FlagVerdictWire decode → host/flag
│ ├── control.ts # ControlIntentWire kind-discriminated decode
│ ├── layout.ts # LayoutConstraintWire ordered Cassowary-program decode
│ ├── bcf.ts # BcfTopicWire/BcfViewpointWire decode
│ ├── geo.ts # GeoFeature WKB decode + geometry/extent/CRS/tile vocabulary — turf owns planar ops only, in ui/viewer
│ ├── bim.ts # BimWire/DiffWire/IdsAudit golden-byte decode
│ └── appearance.ts # MaterialWire/OpenPbrGroupsWire/AppearanceSummary field-for-field decode
├── frame/ # Multi-part payload reassembly with content-key verification (the kernel-delegating mint sites)
│ ├── artifact.ts # ArtifactFrameWire reassembly + content-key verify
│ ├── geometry.ts # GeometryPayload/MeshTensor frames — the GLB rail
│ └── residency.ts # GeometryResidencyWire residency protocol
├── gateway/ # Verb dispatch over decoded command payloads
│ ├── command.ts # CommandPayloadWire dispatch + the availability gate port typed against state/evidence/availability
│ └── support.ts # support-capture verb → telemetry crash
├── invoke/ # The typed invocation client and capability SDK
│ ├── capability.ts # CapabilityDescriptor decode + the capability SDK (C# SdkTarget.TypeScript generated emit)
│ └── client.ts # typed invocation client: retry/backoff budgets from kernel/fault; protocol axis (connect | grpc-web) + retryable-wire schedules
├── contract/ # Schema-drift adjudication over C#-owned descriptors
│ ├── descriptor.ts # FileDescriptorSet drift gate
│ └── drift.ts # drift verdict vocabulary + wire inventory
└── fault/ # The wire-only fault altitude
  ├── detail.ts # FaultDetail reconstruction + the closed HopReason hop vocabulary + the fromConnect total fold
  └── quarantine.ts # poison-frame quarantine + replay
```

The `codec` sub-domain is the surface: each page decodes one C#-minted wire family, keys verbatim, and lands owned vocabulary — `Hlc`/`TenantContext`/`Quantity` into `kernel`, receipt/availability/progress/version evidence into `state`, and a `wire`-owned decoded shape where no domain owner exists. `frame` reassembles the content-keyed payloads and verifies through the one `kernel/identity` `XxHash128` seed-zero mint (never a second mint). `gateway` dispatches decoded command payloads, gating on a `state/evidence/availability` port. `invoke` carries the typed client protocol axis and the capability SDK the C# emitter generates. `contract` diffs the C#-owned `FileDescriptorSet` into a typed drift verdict, so schema drift surfaces as a value, never a runtime decode failure. `fault` reconstructs the C#-minted `FaultDetail` alone — the wire-only altitude, distinct from every folder's local `Data.TaggedError` rail and from `edge/problem`.

## [02]-[SEAMS]

```text seams
codec/envelope        ←  csharp:Rasm.AppHost                # [WIRE]: ReceiptEnvelope/HlcStamp/TenantContext typed receipt decode + the capability SDK on invoke/capability
codec/envelope        ←  csharp:Rasm.AppUi/Render           # [PROJECTION]: RenderReceiptWire frame-hash proof → ui/viewer probe/receipt
codec/proto           ←  csharp:Rasm.Compute                # [WIRE]: proto suite wire + the FaultDetail vocabulary hook
codec/graph           ⇄  csharp:Rasm.Element/Graph          # [WIRE]: ElementGraph/Node/Relationship content-keyed wire the TypeScript peer decodes verbatim
codec/graph           ⇄  csharp:Rasm.Element/Graph          # [WIRE]: rasm.element.v1 ElementGraphWire/NodeWire/RelationshipWire proto under the contract/descriptor drift gate
codec/oplog           ←  csharp:Rasm.Persistence            # [WIRE]: OpLog CRDT wire → store/journal
codec/snapshot        ←  csharp:Rasm.Persistence/Element    # [WIRE]: SnapshotHeader + canonical-CBOR content-stable bytes → store/journal/snapshot
codec/crdt            ←  csharp:Rasm.Persistence/Version    # [WIRE]: CrdtOpWire MessagePack union + the Hlc 16-byte cell → state/crdt/merge
codec/patch           ←  csharp:Rasm.Persistence/Version    # [SHAPE]: JsonPatchDocument RFC 6902 EntityEdit egress
codec/progress        ←  csharp:Rasm.Compute/Runtime        # [PROJECTION]: ProgressStore stream proto → state/evidence/progress
codec/credential      ←  csharp:Rasm.AppHost                # [WIRE]: CredentialPemWire redacted carrier → security/secret/material
codec/claim           ←  csharp:Rasm.AppHost/Observability  # [WIRE]: BenchmarkClaimWire/HostFingerprintWire identity gate → ui/viewer probe/benchmark
codec/livewire        ←  csharp:Rasm.AppHost/Wire           # [WIRE]: BindingStatusWire/CoercedValueWire/WriteReceiptWire → ui/viewer panel/binding
codec/control         ←  csharp:Rasm.AppUi/Shell            # [WIRE]: ControlIntentWire kind-discriminated control vocabulary → ui/viewer panel/control
codec/layout          ←  csharp:Rasm.AppUi/Shell            # [WIRE]: LayoutConstraintWire ordered Kiwi constraint program → ui/viewer panel/layout
codec/bcf             ←  csharp:Rasm.Bim                     # [WIRE]: BcfTopicWire/BcfViewpointWire
codec/geo             ←  csharp:Rasm.Bim/Semantics          # [WIRE]: GeoFeature WKB decode (turf NTS-equivalent planar peer in ui/viewer)
codec/bim             ←  csharp:Rasm.Bim/Exchange           # [WIRE]: BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity
codec/appearance      ←  csharp:Rasm.Materials/Appearance   # [WIRE]: decode-only MaterialWire/OpenPbrGroupsWire/AppearanceSummary field-for-field → ui/viewer scene/appearance

frame/artifact        ←  csharp:Rasm.Compute/Runtime        # [WIRE]: ArtifactFrameWire reassembly → browser/transport/pool
frame/geometry        ←  csharp:Rasm.Compute/Runtime        # [WIRE]: GeometryPayload proto descriptor / MeshTensor view → ui/viewer scene/glb
frame/residency       ←  csharp:Rasm.AppUi/Render           # [PROJECTION]: GeometryResidencyWire ResidencyManifest content-key → browser/transport/pool

gateway/command       ←  csharp:Rasm.AppUi/Shell            # [WIRE]: CommandPayloadWire dispatch + the AvailabilityStore gate port
gateway/support       ←  csharp:Rasm.AppHost                # [WIRE]: support-capture verb → telemetry/signal/crash

invoke/capability     ←  csharp:Rasm.AppHost/Agent          # [CONTENT_KEY]: CapabilityDescriptor command-shape + the SdkTarget.TypeScript generated emit

contract/descriptor   ←  csharp:Rasm.Compute/Runtime        # [WIRE]: FileDescriptorSet ContractDrift verdict (the element.proto descriptor source the gate diffs)

fault/detail          ←  csharp:Rasm.Compute/Runtime        # [WIRE]: ReceiptEnvelopeWire/FaultDetailWire/proto vocabulary reconstruction — the wire-only fault altitude
```

Every seam consumes a C#-minted wire shape; `wire` re-mints none. The `⇄` rows on `codec/graph` are the shared `rasm.element.v1` contract the C# `Rasm.Element` owns and the TypeScript peer decodes by content key — keys crossing verbatim, the descriptor source the drift gate diffs. Decoded values reach their downstream folder through the `#vocab` subpath or a port at the shared vocabulary owner; the folder listed after `→` in a shape is the consuming surface, never a `wire` import of it.
