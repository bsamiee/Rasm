# [SUITE_FEATURES]

Higher-order feature atlas for the four app-grade packages. Each row is a product concept the suite owns in concert across processes; mechanics live in the named planning pages, and a concept never adds a surface — it rides axis rows. Package-local atlases (`Rasm.<Pkg>/FEATURES.md`) carry the isolation concepts; this file carries the cross-package and multi-process topologies.

## [1]-[TOPOLOGY_VOCABULARY]

| [INDEX] | [TOPOLOGY] | [SHAPE] |
| :-----: | ---------- | ------- |
| [1] | in-process | one host, all four packages composed in one ALC |
| [2] | paired | standalone discovers a running Rhino host over UDS; single-writer store lease |
| [3] | companion | host dials out to N spawned/attached service processes over UDS/HTTP2 |
| [4] | sidecar | memory-scratch process; durable access remote-only through its owner |
| [5] | hub | N peers sync through one durable hub (op-log changefeed + delta transports) |
| [6] | service | headless/web roots on PostgresServer with migration bundles at deploy |
| [7] | web-fed | web-service co-hosts the TS SPA; browser consumes grpc-web unary + server-stream |

## [2]-[CONCERT_CONCEPTS]

| [INDEX] | [CONCEPT] | [TOPOLOGY] | [SPINE] |
| :-----: | --------- | ---------- | ------- |
| [1] | Object-graph sync hub (speckle-class) | hub | op-log HLC changefeed + SyncTransport diff rows + ArtifactSync frames |
| [2] | Companion compute farm | companion | ComputeService + ProgressPhase stream + model-result cache identity + warm-affinity node column |
| [3] | Sidecar ingest pipeline | sidecar | capture-direction rows + BlobRemote stream + bounded channel lanes |
| [4] | Standalone ⇄ Rhino paired editing | paired | discovery manifest + DocumentService + storeEpoch lease + LocalOnly degradation fold |
| [5] | GH2 solve cluster feeding dashboards | companion+service | ReceiptSinkPort HLC fan-in + analytical lane + chart projections |
| [6] | Multi-user collaboration | hub+service | PgLogicalReplication V4 parallel + conflict receipts + presence rows |
| [7] | Telemetry lake + activity analytics | service | contributor ports + parquet export + DuckDB rollups + timeline dashboards |
| [8] | Headless CI geometry validation | service | command-journal replay + virtual time + kernel-only validation column |
| [9] | Web viewer / dashboard feed | web-fed | proto vocabulary via connect-es + SnapshotCodec wire rows + bearer credential case |
| [10] | Cross-process result reuse | paired+companion | substrate-keyed result cache + shared-store writer lease + HLC tag invalidation |
| [11] | Document transaction over the wire | paired+companion | one 14-slot transaction receipt, proto-projected field-for-field |
| [12] | Operational control plane | service+companion | ControlService verbs: capture-support, set-degradation, reload-options |
| [13] | Crash-recovery choreography | all | boot markers + host crash markers + restore choreography + dock-layout restore |
| [14] | Correlation drill-down timeline | all | one correlation id + HLC stamps joined at the evidence-view fold |
| [15] | Geospatial sync product | hub+service | PostGIS lanes + proto wire geometry + GeoJSON projection + geo chart row |

## [3]-[CROSS_PACKAGE_LAWS]

- One wire vocabulary: Compute owns the proto suite (ComputeService, DocumentService, ControlService, ArtifactSync, grpc.health.v1); every other package projects into it.
- One causal primitive: the ReceiptSinkPort HLC stamp orders cross-process evidence; receipts, bundles, and degradation stay process-local.
- One frame law: 64 KiB frames, Crc32 per frame, XxHash128 whole-artifact — declared at ArtifactSync, consumed by BlobRemote.
- One canonical wire geometry: the proto geometry family; GeoJSON, NTS, and RhinoCommon shapes are boundary projections.
- One codec table: seam-keyed (config, snapshot, delta, gRPC, dashboard, deep-link) — STJ source-gen, MessagePack, protobuf each own named seams; no codec crosses into another's seam.
- One retry owner per hop, one cache owner, one clock seam, one drain conductor — the AppHost ports.

## [4]-[ROUTING]

- Concept mechanics: `Rasm.<Pkg>/.planning/<page>.md` per the page named in each package atlas.
- Implementation charters: `Rasm.<Pkg>/.planning/README.md`.
- Suite authoring law: `libs/csharp/.planning/README.md`; ownership ledger: `libs/csharp/.planning/_region-map.md`.
