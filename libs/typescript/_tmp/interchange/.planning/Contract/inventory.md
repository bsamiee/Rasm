# [INTERCHANGE_INVENTORY]

The canonical contract inventory: every consumed C# wire cluster mapped to its codec and TS rail, the three suite anchors that bind the whole wire, and the versioning law encoded as the `Ingress/refinement.md` rows that enforce it. The `#TS_PROJECTION` fence on each owning .NET page is the authoritative shape; this page is the map a reader navigates to find which rail consumes which cluster, never a re-authored shape. The versioning obligations are decode-enforced by a `SchemaRefinement` brand or filter row, never a prose check.

## [01]-[INDEX]

- [01]-[WIRE_INVENTORY]: the cluster-to-rail map, the three suite anchors, the versioning law.
- [02]-[TS_PROJECTION]: the suite-anchor wire shapes the inventory binds cross-cluster.


## [02]-[WIRE_INVENTORY]

- Owner: the inventory of every wire contract the branch consumes, naming each owning .NET page and its codec. Three suite-level shapes anchor the whole wire: `MethodShape` keying every proto rpc (`Transport/transport.md`), `TenantContextWire` partitioning every receipt by tenancy, and the W3C TraceContext call-spine constants stamping every outbound proto call. The versioning law is the drift-tolerance obligation each enforced by its `Ingress/refinement.md` row.
- Cases: the thirteen-cluster map below carries each cluster, its codec, and its TS rail; the cross-row invariants are tenancy and the drift law. `TenantContextWire` rides the `tenant` member of every `ReceiptEnvelopeWire` with `tenantId` crossing as the `UInt128` decimal-string the .NET `TenantId` value object emits, so a TS dashboard reads the identical tenancy identity the .NET RLS predicate reads, never a re-minted client tenant key. The .NET side classifies drift as Identical, Additive (tolerated), or Breaking (typed rejection); the TS side mirrors that verdict at dial time through `Contract/descriptor.md` `DescriptorGate`, which runs the same Identical/Additive/Breaking classifier over the runtime `FileDescriptorSet` reflection so a Breaking descriptor drift faults at client construction rather than mid-decode — the one TS-side descriptor verdict, judged once against the wire descriptor. Reserved field numbers never return to use and generated parsers retain unknown fields — proto fields add numbered fields only, JSON members add additive members, kind literals add one literal folding an unknown to the `Ingress/quarantine.md` case, key scalars carry the ordinal-string brand, identity fields carry the guid and 16-byte content-key brand, breaking drift surfaces as a typed `FaultDetail` through the fault rail, and a partial-update mutation crosses as a recorded-intent `JsonPatchDocument`/`FieldMask` decoded through `Codec/patch.md`, never a state-diff re-derived at the consumer. The row [6] `crdt` column-family `OpLogEntryWire.payload` carries the `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` `[MessagePack.Union]` — its append-only union-tag sequence and per-arm `[Key(n)]` field order are the C# single mint the `Codec/codec#CRDT_OP_DECODE` row decodes verbatim, a new union tag folding additive and a `kind`-discriminated re-mint being the named cross-language drift defect.
- Entry: every json-stj receipt payload binds as the `TPayload` type parameter on the `projection` `ReceiptEnvelopeCarrier`, the envelope `kind` mirroring the payload discriminator and the envelope `tenant`/`correlation`/HLC stamp carrying the cross-process primitives; the evidence timeline carries envelopes whole. W3C TraceContext is the call-spine stamp axis: `rasm-correlation` and `traceparent` are the two ambient header rows the `Transport/transport.md` interceptor writes on every proto call, `traceparent` authored from the active span context and `tracestate` read-through on the .NET edge only.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface; `@msgpack/msgpack` for the messagepack surface; `effect` for the `Schema.Class` codec surface and the brand/filter primitives.
- Growth: a new wire contract lands as one inventory row under its owning anchor and codec; a new suite anchor lands as one cross-cluster binding row; a new versioning invariant lands as one `SchemaRefinement` row, never a prose assertion here.
- Boundary: telemetry crosses no wire contract — the AppHost OTLP exporter ships signals from the app roots to the collector and dashboards read the collector; the four host-local .NET pages reconstruct solely through `ReceiptEnvelopeWire` and author no second wire shape; the branch stamps no tenancy onto an outbound call — tenancy is a server-minted receipt dimension the TS side only reads.

| [INDEX] | [C# WIRE ANCHOR]                             | [CODEC]     | [TS RAIL/FOLD]                                        |
| :-----: | :------------------------------------------- | :---------- | :---------------------------------------------------- |
|  [01]   | `csharp:Rasm.AppHost/Runtime/lifecycle`      | json-stj    | RuntimeFeed                                           |
|  [02]   | `csharp:Rasm.AppHost/Observability/health`   | json-stj    | HealthStore                                           |
|  [03]   | `csharp:Rasm.AppHost/Observability/bundles`  | json-stj    | CommandGateway capture-support                        |
|  [04]   | `csharp:Rasm.AppHost/Runtime/ports`          | json-stj    | ReceiptEnvelopeCarrier                                |
|  [05]   | `csharp:Rasm.Persistence/Version/snapshots`  | messagepack | DecodeRail geometry row + SnapshotFeed                |
|  [06]   | `csharp:Rasm.Persistence/Sync/collaboration` | messagepack | DecodeRail (crdt-op union) + ConflictPresenceStore    |
|  [07]   | `csharp:Rasm.Compute/Runtime/channels`       | proto       | WireClients + ArtifactFrameRail + FaultDetailRail     |
|  [08]   | `csharp:Rasm.Compute/Runtime/progress`       | proto       | ProgressStore                                         |
|  [09]   | `csharp:Rasm.Compute/Runtime/receipts`       | json-stj    | ReceiptStore + BenchmarkRoute                         |
|  [10]   | `csharp:Rasm.AppUi/Shell/commands`           | json-stj    | CommandGateway + AvailabilityStore                    |
|  [11]   | `csharp:Rasm.AppUi/Render/evidence`          | json-stj    | EvidenceFeed + EvidenceTimelineRoute                  |
|  [12]   | `csharp:Rasm.Bim/Review/issues`              | json-stj    | DecodeRail BCF rows + ui/bcf-anchor                   |
|  [13]   | `csharp:Rasm.AppHost/Wire/livewire`          | json-stj    | DecodeRail live-wire rows + ui/live-binding-dashboard |

Each anchor's `#TS_PROJECTION` cluster is the authoritative wire shape; the codec column fixes the byte format and the rail column the consuming TS owner. Rows [12]-[13] are the BCF coordination and live-wire binding clusters the UI bcf-anchor and live-binding-dashboard leaves decode: `Rasm.Bim/Review/issues#TS_PROJECTION` mints `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` over `BimWireOptions.Json`, and `Rasm.AppHost/Wire/livewire#TS_PROJECTION` mints `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire`/`WriteBackWire` over `LiveWireOptions.Json` (the write receipt also riding the existing `ReceiptEnvelopeWire`), both decoded at `Codec/codec#BCF_LIVE_WIRE_DECODE`. The four host-local .NET pages — `csharp:Rasm.AppHost/Runtime/resources`, `csharp:Rasm.AppHost/Observability/telemetry`, `csharp:Rasm.Persistence/Query/lanes`, `csharp:Rasm.Persistence/Store/remote` — carry no `#TS_PROJECTION` cluster and never enter the inventory.

## [03]-[TS_PROJECTION]

- Owner: the three suite-anchor shapes the inventory binds cross-cluster — the HLC stamp, the tenant context, and the package literal, sourced from `csharp:Rasm.AppHost/Runtime/ports#TS_PROJECTION`. This page owns the verbatim-transcription law for the whole branch; every other page's projection cluster carries only its own shape delta.
- Entry: `HlcStampWire` carries the physical/logical/skew triple every envelope stamps; `TenantContextWire.tenantId` is the `UInt128` decimal-string; `RasmPackage` is the four-package literal gating the fault projection.
- Packages: `effect` `Schema` for the codec surface.
- Growth: a new suite anchor lands as one shape row.

```ts contract
const HlcStampWire = Schema.Struct({ physical: Schema.String, logical: Schema.Number, skewBound: Schema.String });
const TenantContextWire = Schema.Struct({ tenantId: Schema.String, slug: Schema.String });
const RasmPackage = Schema.Literal("Rasm.AppHost", "Rasm.Persistence", "Rasm.Compute", "Rasm.AppUi");
```
