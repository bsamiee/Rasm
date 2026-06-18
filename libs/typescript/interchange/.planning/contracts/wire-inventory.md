# [INTERCHANGE_WIRE_INVENTORY]

The canonical contract inventory: every consumed C# wire cluster mapped to its codec and TS rail, the three suite anchors that bind the whole wire, and the versioning law encoded as the `refinement/schema-refinement.md` rows that enforce it. The `#TS_PROJECTION` fence on each owning .NET page is the authoritative shape; this page is the map a reader navigates to find which rail consumes which cluster, never a re-authored shape. The versioning obligations are decode-enforced by a `SchemaRefinement` brand or filter row, never a prose check.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                          |
| :-----: | :------------- | :------------------------------------------------------------- |
|   [1]   | WIRE_INVENTORY | the cluster-to-rail map, the three suite anchors, the versioning law |
|   [2]   | TS_PROJECTION  | the suite-anchor wire shapes the inventory binds cross-cluster   |

## [2]-[WIRE_INVENTORY]

- Owner: the inventory of every wire contract the branch consumes, naming each owning .NET page and its codec. Three suite-level shapes anchor the whole wire: `MethodShape` keying every proto rpc (`transport/transport.md`), `TenantContextWire` partitioning every receipt by tenancy, and the W3C TraceContext call-spine constants stamping every outbound proto call. The versioning law is the drift-tolerance obligation each enforced by its `refinement/schema-refinement.md` row.
- Cases: the eleven-cluster map below carries each cluster, its codec, and its TS rail; the cross-row invariants are tenancy and the drift law. `TenantContextWire` rides the `tenant` member of every `ReceiptEnvelopeWire` with `tenantId` crossing as the `UInt128` decimal-string the .NET `TenantId` value object emits, so a TS dashboard reads the identical tenancy identity the .NET RLS predicate reads, never a re-minted client tenant key. The .NET side classifies drift as Identical, Additive (tolerated), or Breaking (typed rejection); reserved field numbers never return to use and generated parsers retain unknown fields — proto fields add numbered fields only, JSON members add additive members, kind literals add one literal folding an unknown to the `quarantine/drift-terminal.md` case, key scalars carry the ordinal-string brand, identity fields carry the guid and 16-byte content-key brand, and breaking drift surfaces as a typed `FaultDetail` through the fault rail.
- Entry: every json-stj receipt payload binds as the `TPayload` type parameter on the `projection` `ReceiptEnvelopeCarrier`, the envelope `kind` mirroring the payload discriminator and the envelope `tenant`/`correlation`/HLC stamp carrying the cross-process primitives; the evidence timeline carries envelopes whole. W3C TraceContext is the call-spine stamp axis: `rasm-correlation` and `traceparent` are the two ambient header rows the `transport/transport.md` interceptor writes on every proto call, `traceparent` authored from the active span context and `tracestate` read-through on the .NET edge only.
- Packages: `@bufbuild/protobuf` and `@connectrpc/connect` for the descriptor and method-shape surface; `@msgpack/msgpack` for the messagepack surface; `effect` for the `Schema.Class` codec surface and the brand/filter primitives.
- Growth: a new wire contract lands as one inventory row under its owning anchor and codec; a new suite anchor lands as one cross-cluster binding row; a new versioning invariant lands as one `SchemaRefinement` row, never a prose assertion here.
- Boundary: telemetry crosses no wire contract — the AppHost OTLP exporter ships signals from the app roots to the collector and dashboards read the collector; the four host-local .NET pages reconstruct solely through `ReceiptEnvelopeWire` and author no second wire shape; the branch stamps no tenancy onto an outbound call — tenancy is a server-minted receipt dimension the TS side only reads.

| [INDEX] | [C# WIRE ANCHOR]                                          | [CODEC]     | [TS RAIL/FOLD]                                    |
| :-----: | :------------------------------------------------------- | :---------- | :------------------------------------------------ |
|   [1]   | `csharp:Rasm.AppHost/hosting/lifecycle-and-drain`        | json-stj    | RuntimeFeed                                       |
|   [2]   | `csharp:Rasm.AppHost/observability/health-and-degradation` | json-stj  | HealthStore                                       |
|   [3]   | `csharp:Rasm.AppHost/observability/support-bundles`      | json-stj    | CommandGateway capture-support                    |
|   [4]   | `csharp:Rasm.AppHost/ports/runtime-ports`                | json-stj    | ReceiptEnvelopeCarrier                            |
|   [5]   | `csharp:Rasm.Persistence/snapshots/codecs`               | messagepack | DecodeRail geometry row + SnapshotFeed            |
|   [6]   | `csharp:Rasm.Persistence/sync/collaboration`             | messagepack | DecodeRail + ConflictPresenceStore                |
|   [7]   | `csharp:Rasm.Compute/remote/remote`                      | proto       | WireClients + ArtifactFrameRail + FaultDetailRail |
|   [8]   | `csharp:Rasm.Compute/progress/progress`                  | proto       | ProgressStore                                     |
|   [9]   | `csharp:Rasm.Compute/receipts/receipts`                  | json-stj    | ReceiptStore + BenchmarkRoute                     |
|  [10]   | `csharp:Rasm.AppUi/commands/commands-availability`       | json-stj    | CommandGateway + AvailabilityStore                |
|  [11]   | `csharp:Rasm.AppUi/evidence/diagnostics-evidence`        | json-stj    | EvidenceFeed + EvidenceTimelineRoute              |

Each anchor's `#TS_PROJECTION` cluster is the authoritative wire shape; the codec column fixes the byte format and the rail column the consuming TS owner. The four host-local .NET pages — `csharp:Rasm.AppHost/resources/resource-lanes`, `csharp:Rasm.AppHost/observability/diagnostics-and-telemetry`, `csharp:Rasm.Persistence/modalities/data-lanes`, `csharp:Rasm.Persistence/stores-remote/object-store` — carry no `#TS_PROJECTION` cluster and never enter the inventory.

## [3]-[TS_PROJECTION]

- Owner: the three suite-anchor shapes the inventory binds cross-cluster — the HLC stamp, the tenant context, and the package literal, sourced from `csharp:Rasm.AppHost/ports/runtime-ports#TS_PROJECTION`. This page owns the verbatim-transcription law for the whole branch; every other page's projection cluster carries only its own shape delta.
- Entry: `HlcStampWire` carries the physical/logical/skew triple every envelope stamps; `TenantContextWire.tenantId` is the `UInt128` decimal-string; `RasmPackage` is the four-package literal gating the fault projection.
- Packages: `effect` `Schema` for the codec surface.
- Growth: a new suite anchor lands as one shape row.

```ts contract
const HlcStampWire = Schema.Struct({ physical: Schema.String, logical: Schema.Number, skewBound: Schema.String });
const TenantContextWire = Schema.Struct({ tenantId: Schema.String, slug: Schema.String });
const RasmPackage = Schema.Literal("Rasm.AppHost", "Rasm.Persistence", "Rasm.Compute", "Rasm.AppUi");
```
