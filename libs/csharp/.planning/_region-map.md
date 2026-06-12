# [SUITE_REGION_MAP]

Suite ownership ledger for the four app-package planning corpora. Binding precedence: locked campaign decisions, then adversarial-verifier and attack corrections, then synthesis closures, then design JSON. Authors append provisional rows before writing a page; the cold grader flips rows to FINAL on the all-PASS sweep. Authoring position: AppHost, Persistence, Compute, AppUi; charter PAGE_INDEX order within a package.

## [1]-[PAGE_REGIONS]

[APPHOST]:

[PERSISTENCE]:

[COMPUTE]:

[APPUI]:

## [2]-[SIGNATURE_REGIONS]

## [3]-[OWNER_SYMBOLS]

## [4]-[SEAM_SPLITS]

- HybridCache: mechanics at AppHost/resource-lanes#cache port, stampede, tags, entry options; consequence at Persistence/cache-indexes#L2 IDistributedCache + serializer rows.
- Outbound retry: mechanics at AppHost/outbound-resilience#one keyed Polly pipeline per hop; consequence at Compute/remote-lane#Conflict receipts, zero gRPC-native retry. Database retry is excluded from the hop law: execution-strategy rows live at Persistence/store-profiles.
- Correlation: mechanics at AppHost/diagnostics-and-telemetry#W3C TraceContext + Baggage spine; consequence on every sibling signal and across the UDS hop as gRPC metadata.
- Drain order: mechanics at AppHost/lifecycle-and-drain#frozen rank-band table (inbound-admission cessation precedes band 100; 100s AppUi, 200s Compute, 300s Persistence, telemetry last; store-dependency constraint column); consequence at each sibling's DrainParticipantPort registrations.
- DataClassification taxonomy: mechanics at AppHost/diagnostics-and-telemetry#classification axis; consequence at Persistence/redaction-retention#store-side enforcement rows.
- Lane naming: AppHost owns process-level drainable queues (DrainQueue); Compute owns solve-path bounded channels (WorkLane). The homonym is resolved by altitude.
- Profile variance: single residence at AppHost/host-profiles#HostProfile row columns and the resolved-profile record; Persistence consumes the resolved record and owns no profile-keyed table.
- Wire vocabulary: mechanics at Compute/remote-lane#proto suite (ComputeService, ArtifactSync, DocumentService, grpc.health.v1); consequence at AppHost/runtime-ports#suite wire law + TS tooling map.
- Store paths: mechanics at AppHost/host-profiles#per-user root computation; consequence at Persistence/store-profiles#resolved profile record consumption, never path derivation.
- Receipt sinks: mechanics at AppHost/runtime-ports#ReceiptSinkPort; consequence at Compute/receipts-and-benchmarks, Persistence/query-rail, AppUi/diagnostics-evidence projection rows.
- Telemetry contribution: mechanics at AppHost/runtime-ports#TelemetryContributorPort; consequence at Persistence (AddNpgsql tracer/meter rows) and Compute (ActivitySource registration rows).
- Clock seam: mechanics at AppHost/time-and-deadlines#IClock + TimeProvider injection; consequence at Persistence TTL/retention/HLC/lease stamping and Compute elapsed measurement.
