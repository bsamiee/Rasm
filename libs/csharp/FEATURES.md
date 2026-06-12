# [SUITE_FEATURES]

Higher-order feature atlas for the four app-grade packages. Each row is a product concept the suite owns in concert across processes; mechanics live in the named planning pages, and a concept never adds a surface — it rides axis rows. Package-local atlases (`Rasm.<Pkg>/FEATURES.md`) carry the isolation concepts; this file carries the cross-package and multi-process topologies, and every row's ANCHORS cell names the owning page clusters across the four packages.

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

| [INDEX] | [CONCEPT] | [TOPOLOGY] | [SPINE] | [ANCHORS] |
| :-----: | --------- | ---------- | ------- | --------- |
| [1] | Object-graph sync hub (speckle-class) | hub | op-log HLC changefeed + SyncTransport diff rows + ArtifactSync frames | Persistence/sync-collaboration#OPLOG_CHANGEFEED · Persistence/sync-collaboration#TRANSPORT_AXIS · Compute/remote-lane#ARTIFACT_FRAMES |
| [2] | Companion compute farm | companion | ComputeService + ProgressPhase stream + model-result cache identity + warm-affinity node column | Compute/remote-lane#PROTO_VOCABULARY · Compute/remote-lane#TRANSPORT_AXIS · Compute/progress-and-observation#OBSERVATION_SEAMS · Persistence/cache-indexes#MODEL_RESULT_INDEX |
| [3] | Sidecar ingest pipeline | sidecar | capture-direction field + memory-scratch placement + BlobRemote stream + bounded channel lanes | Persistence/sync-collaboration#TRANSPORT_AXIS · Persistence/store-profiles#PLACEMENT_MATRIX · Persistence/store-profiles#PROFILE_AXIS · Compute/scheduling-and-lanes#LANE_AXIS |
| [4] | Companion capture-event ingest | companion | CaptureEvents client-stream + capture-ingest DropOldest lane + bulk write with self-emitted changefeed | Compute/remote-lane#PROTO_VOCABULARY · Compute/scheduling-and-lanes#LANE_AXIS · Persistence/query-rail#BULK_LANE |
| [5] | Standalone ⇄ Rhino paired editing | paired | discovery manifest + DocumentService + storeEpoch lease + LocalOnly degradation fold | AppHost/outbound-resilience#DISCOVERY_ATTACH · Compute/remote-lane#PROTO_VOCABULARY · Persistence/store-profiles#CROSS_PROCESS_LAW · AppHost/health-and-degradation#DEGRADATION_RAIL |
| [6] | GH2 solve cluster feeding dashboards | companion+service | ReceiptSinkPort HLC fan-in + analytical lane + chart projections | AppHost/runtime-ports#PORT_RECORDS · Persistence/data-lanes#ANALYTICAL_LANE · AppUi/charts-dashboards#DASHBOARD_TILES |
| [7] | Multi-user collaboration | hub+service | PgLogicalReplication V4 parallel + conflict receipts + presence rows | Persistence/sync-collaboration#TRANSPORT_AXIS · Persistence/sync-collaboration#MERGE_LAW · Persistence/sync-collaboration#PRESENCE_AND_BLOB · AppUi/inspector-editing#CONFLICT_RESOLUTION |
| [8] | Telemetry lake + activity analytics | service | contributor ports + parquet export + DuckDB rollups + timeline dashboards | AppHost/runtime-ports#PORT_RECORDS · Persistence/data-lanes#ANALYTICAL_LANE · AppUi/charts-dashboards#DASHBOARD_TILES |
| [9] | Headless CI geometry validation | service | command-journal replay + virtual time + structurally absent host-document capability on headless rows | AppUi/diagnostics-evidence#HEADLESS_DERIVATION · AppUi/surface-hosts#HOST_AXIS · AppUi/commands-availability#AVAILABILITY_ALGEBRA |
| [10] | Web viewer / dashboard feed | web-fed | proto vocabulary via connect-es + SnapshotCodec wire rows + bearer credential case | AppHost/runtime-ports#WIRE_LAW · Compute/remote-lane#TRANSPORT_AXIS · Compute/remote-lane#CALL_POLICY · Persistence/snapshot-codecs#TS_PROJECTION |
| [11] | Cross-process result reuse | paired+companion | substrate-keyed result cache + shared-store writer lease + HLC tag invalidation | Persistence/cache-indexes#MODEL_RESULT_INDEX · Compute/model-lane#RESULT_CACHE · Persistence/store-profiles#CROSS_PROCESS_LAW · Persistence/sync-collaboration#OPLOG_CHANGEFEED |
| [12] | Document transaction over the wire | paired+companion | one DocumentTransaction typed receipt (Rasm.Rhino), proto-projected field-for-field | Compute/remote-lane#PROTO_VOCABULARY · AppUi/tables-hierarchy#GRID_COMMIT · AppUi/commands-availability#PALETTE_AND_REMOTE |
| [13] | Operational control plane | service+companion | ControlService verbs: capture-support, set-degradation, reload-options | Compute/remote-lane#PROTO_VOCABULARY · AppHost/support-bundles#TRIGGER_UNION · AppHost/health-and-degradation#WIRE_HEALTH · AppHost/configuration-and-options#POLICY_VALUES |
| [14] | Crash-recovery choreography | all | boot markers + host crash markers + restore choreography + dock-layout restore | AppHost/lifecycle-and-drain#FAULT_SPINE · AppHost/support-bundles#TRIGGER_UNION · Persistence/snapshot-codecs#RESTORE_AND_DIFF · AppUi/shell-navigation#DOCK_LAYOUTS |
| [15] | Correlation drill-down timeline | all | one correlation id + HLC stamps joined at the evidence-view fold | AppHost/diagnostics-and-telemetry#CORRELATION_SPINE · AppHost/runtime-ports#PORT_RECORDS · AppUi/diagnostics-evidence#CORRELATION_JOIN |
| [16] | Geospatial sync product | hub+service | PostGIS lanes + proto wire geometry + GeoJSON projection + geo chart row | Persistence/data-lanes#GEO_LANES · Compute/remote-lane#PROTO_VOCABULARY · Persistence/snapshot-codecs#CODEC_AXIS · AppUi/charts-dashboards#SERIES_TABLE |
| [17] | Live-data spine | in-process | host watch fact to projection write to tag transition to delta fetch to IChangeSet | AppUi/live-data#DATA_SOURCES · Persistence/query-rail#INTERCEPTOR_SPINE · AppHost/resource-lanes#CACHE_PORT |
| [18] | Deterministic test-host concert | in-process | test-host profile + SqliteMemory placement + InProcess transport + headless surface + fake clock pairs | AppHost/host-profiles#PROFILE_AXIS · AppHost/time-and-deadlines#CLOCK_SPLIT · Persistence/store-profiles#PLACEMENT_MATRIX · Compute/remote-lane#TRANSPORT_AXIS · AppUi/surface-hosts#HOST_AXIS |

## [3]-[CROSS_PACKAGE_LAWS]

- One wire vocabulary: Compute owns the proto suite (ComputeService, DocumentService, ControlService, ArtifactSync, grpc.health.v1); every other package projects into it.
- One causal primitive: the ReceiptSinkPort HLC stamp orders cross-process evidence; receipts, bundles, and degradation stay process-local.
- One frame law: 64 KiB frames, Crc32 per frame, XxHash128 whole-artifact — declared at ArtifactSync, consumed by BlobRemote.
- One canonical wire geometry: the proto geometry family; GeoJSON, NTS, and RhinoCommon shapes are boundary projections.
- One codec table: seam-keyed (config, snapshot, delta, gRPC, dashboard, deep-link) — STJ source-gen, MessagePack, protobuf each own named seams; no codec crosses into another's seam.
- One retry owner per hop, one cache owner, one clock seam, one drain conductor — the AppHost ports.

## [4]-[ROUTING]

- Concept mechanics: `Rasm.<Pkg>/.planning/<page>.md` at the cluster anchor named in each row's ANCHORS cell.
- Implementation charters: `Rasm.<Pkg>/.planning/README.md`; implementation-start gates and research probes: `Rasm.<Pkg>/ROADMAP.md`.
- Server topologies (service, hub, web-fed): operator provisioning rows at `Rasm.Persistence/.planning/store-profiles.md#PROVISIONING_ROWS`; physical deploy assets (postgresql.conf and pg_hba fragments, role grants) gate at the first headless/web app root per the ROADMAP tables.
- TS web layer: `libs/typescript/.planning/` (`architecture-posture.md`, `wire-consumption.md`) consumes the TS_PROJECTION clusters listed in each package charter WIRE_PAGES.
- Suite authoring law: `libs/csharp/.planning/README.md`; ownership ledger: `libs/csharp/.planning/_region-map.md`.
