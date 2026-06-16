# [TS_FEATURES]

The branch capability atlas — a dense, ambitious universal-library feature reservoir held lean (feature + owner + page#cluster, no state, no case counts). Owner state lives only on each charter DENSITY_BAR; this atlas routes to the owner for state. Features span the five flat domains and the cross-domain concert; the branch serves every C# modality through the wire contracts and the infrastructure boundary while standing alone as an adoptable Effect library.

## [1]-[INTERCHANGE] — the byte-to-typed-and-back boundary

- One shared grpc-web transport over four browser-dialable generated services, one polymorphic correlation/trace/credential interceptor stamp — `WireTransport`/`WireClients` (interchange/transport#TRANSPORT_AND_CLIENTS).
- The transport-capability shape gating http2 (4 method shapes) vs grpcWeb (unary + server-stream only) — `TransportCapabilityWire` (interchange/transport#TRANSPORT_AND_CLIENTS).
- The committed-descriptor buf pipeline as a build-time codegen edge emitting `interchange/gen/*_pb.ts` — (interchange/transport#CODEGEN_TOOLING).
- One `DecodeRail` six-codec family read by codec key with `EncodeRail` as a direction key on the same owner — (interchange/codec-rails#CODEC_RAILS).
- `SchemaRefinement` brand/filter decode-enforcement rows making every versioning invariant decode-enforced — (interchange/codec-rails#CODEC_RAILS).
- `GeometryRail` GeoJSON projection off the proto `GeometryPayload` oneof — (interchange/codec-rails#CODEC_RAILS).
- `ArtifactFrameRail` 64-KiB/Crc32/XxHash128 content-addressed reassembly with `ContentKey` byte-identity to C# — (interchange/codec-rails#CODEC_RAILS).
- `FaultDetailRail` reconstructing the full .NET fault set as one `Data.TaggedError` family bound by `Match.tagsExhaustive` (ComputeFault 13, StoreFault, HopFault 7, ConfigError 7) — (interchange/codec-rails#CODEC_RAILS).
- The Persistence snapshot/sync `#TS_PROJECTION` bigint decode via `DataView`/`useBigInt64` — (interchange/codec-rails#CODEC_RAILS).
- `QuarantineFold` additive-only contract-drift defense + `CONTRACT_INVENTORY` map — (interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE).
- `CommandGateway` outbound verb face gated by availability + `ControlService` verbs, with `IntentRegistry` deep-link key vocabulary — (interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE).
- The capability-descriptor codegen REFINEMENT_HORIZON row — (interchange/transport#CODEGEN_TOOLING).

## [2]-[PROJECTION] — the unified fold algebra

- One `StreamPolicy` (single `Schedule` reconnect + Stream-operator vocabulary) every fold composes — (projection/fold-algebra#FOLD_ALGEBRA).
- The key-discriminated fold combinator over a Stream/receipt sequence into an immutable `SubscriptionRef`-backed keyed map — (projection/fold-algebra#FOLD_ALGEBRA).
- The five stream stores `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore` — (projection/fold-algebra#FOLD_ALGEBRA).
- `ReceiptStore`/`EvidenceFeed`/`AvailabilityStore` as fold rows of the same algebra — (projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE).
- The `ReceiptEnvelopeCarrier<TPayload>` binding for every structured-text payload — (projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE).
- The `SkewBandWire` HLC skew-band correlation fold AS a first-class confidence-interval projection output — the one place the `ui` domain surfaces distributed-systems clock-uncertainty as product UI — (projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE).
- The standing-query window vocabulary (tumbling/sliding/session + watermarks) REFINEMENT_HORIZON — (projection/fold-algebra#FOLD_ALGEBRA).

## [3]-[UI] — the browser UI/UX/components library (AppUi-analog)

- `AtomBinding` the one `@effect-atom` spine + `DeepLinkBinding` + dev atom inspector — (ui/binding#BINDING).
- url-as-state, offline IndexedDB persistence via Effect `KeyValueStore` over idb-keyval, undo/redo as Effect-native client-state folds — (ui/binding#BINDING).
- Leaf observation routes `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel` as fold subscribers — (ui/render-surfaces#RENDER_SURFACES).
- `GeoSeriesSurface` with one `GeoSeriesLayer` union owner (maplibre + deck.gl `GeoJsonLayer` + `MapboxOverlay` interleave) — (ui/render-surfaces#RENDER_SURFACES).
- The role-based headless interaction-role vocabulary owner-block (actions/collections/inputs/overlays/navigation/feedback/pickers/core) — (ui/component-system#COMPONENT_SYSTEM).
- Theme tokens + runtime CSS-var sync — (ui/component-system#COMPONENT_SYSTEM).
- The `GlbViewport` WebGL render REFINEMENT_HORIZON (renderer-backend `Schema.Literal` axis incl. webgpu) — (ui/render-surfaces#RENDER_SURFACES).

## [4]-[PLATFORM] — the browser SPA infrastructure + entry (AppHost-analog)

- `CompositionRoot`/`BrowserPlatform`/`AuthSession` (arctic OIDC/PKCE)/`RuntimeConfig` SPA browser entry Layer graph composing ui+interchange+projection into one runtime — (platform/host-runtime#HOST_RUNTIME).
- `SelfTelemetry`/`MetricRegistry` over the browser OTel `WebSdk` exporter — (platform/platform-substrate#PLATFORM_SUBSTRATE).
- `BuildPipeline` + `DecodeWorkerPool` (`@effect/platform` `Worker.makePool`) + `LocalPersistence` — (platform/platform-substrate#PLATFORM_SUBSTRATE).
- `AppRouter` client routing over a `Schema.Literal` route-key axis + `NavigationGuard` + `RouteParamCodec` (nuqs query-state, ZERO router package) — (platform/routing-navigation#ROUTING_NAVIGATION).
- `ServiceWorkerHost` PWA/offline-first cache lifecycle over a `CacheStrategy` axis + `BackgroundSyncReplay` draining `LocalPersistence` into `CommandGateway` on redial — (platform/service-worker#SERVICE_WORKER).
- `CrashTelemetry` global error capture reconstructed into the interchange `FaultDetail` family + `ErrorBoundaryBinding` (react-error-boundary) shipping `CrashReport` through `SelfTelemetry` — (platform/error-boundary#ERROR_BOUNDARY).
- `RemoteConfig` the browser read-side over the services `FeatureFlags` vocabulary + `FlagKey` axis + `FlagEvaluation` Match dispatch — (platform/feature-flags-config#FEATURE_FLAGS_CONFIG).
- `PerformanceBudget` Core-Web-Vitals capture (native `PerformanceObserver`) over a `VitalMetric` axis feeding the `MetricRegistry` instrument rows + `BudgetThreshold` gating Record — (platform/web-vitals#WEB_VITALS).

## [5]-[SERVICES] — the node services tier + its infrastructure

- `WorkflowOwner`/`ActivityOwner`/`ClusterEngine` durable units + cluster-backed `WorkflowEngine` — (services/durable-execution#DURABLE_EXECUTION).
- The `AiProvider` `Schema.Literal` provider axis (anthropic/openai/google/bedrock/openrouter), the sole declaration site — (services/durable-execution#DURABLE_EXECUTION).
- The agent-execution journal (session_start/tool_call/checkpoint/session_complete) + resilience primitives (circuit-breaker/retry/rfc6902 JSON-patch) — (services/durable-execution#DURABLE_EXECUTION).
- `SqlBoundary` single `PgClient`/`Model.Class`/`Migrator` owner, one `Model.Class` per entity, projections via `Model.fields`/`Schema.pick` — (services/persistence#STORE_BOUNDARY).
- Multi-tenancy RLS scoping axis (`app.current_tenant` GUC + tenant lifecycle + purge family) — (services/persistence#TENANCY).
- jobs/DLQ + events/journal + notifications multi-channel preference matrix + assets/transfer export-codec (exceljs/papaparse/jspdf/jszip) + feature-flag percentage-rollout buckets — (services/persistence#WORK_AND_SIGNALS).
- The fused semantic+lexical+trigram+phonetic weighted-rank search owner with HNSW efSearch tuning and staleness tracking — (services/hybrid-search#HYBRID_SEARCH).
- `InternalRpc` one `RpcGroup` + `WorkflowProxy` derived over the same wire Schema + `RunnerBackplane` + `ScheduledWork` (Redis backplane) — (services/internal-rpc#INTERNAL_RPC).
- The data/compute/observe tier `ComponentResource` model + deploy-mode dispatch (cloud K8s vs self-hosted compose+Traefik) — (services/provisioning#PROVISIONING).
- entry-thin/impl-dense split + the `./provisioning` exports subpath + `StackOutputs`/`StackReference` + Doppler secret boundary + pluggable state backend + idempotent bootstrap + the self-hosted service-equivalence map + `ObservabilityStack` (Grafana Alloy OTLP -> Prometheus -> Grafana) — (services/provisioning#PROVISIONING).

## [6]-[CONCERT] — cross-domain capability the suite powers

- Web-fed SPA co-hosted same-origin by the C# web-service root, consuming grpc-web unary + server-stream only, partitioning all evidence by `TenantContextWire` — the web face of the C# AppUi product UI and the gh2/Rhino solve-cluster evidence.
- One boot-minted correlation id + HLC `SkewBand` as the single ordering primitive, rendering a "within +/-N ms confidence" dashboard a capability the C# AppUi structurally cannot own.
- The node tier as one peer of the remote/companion/sidecar/hub/service topology over the same proto vocabulary — dialing `DocumentService`, ingesting capture-event client-streams (dialed on node, structurally non-dialable from the browser), participating as a hub peer over the op-log HLC changefeed.
- Content-addressed `XxHash128` cache identity shared across C#, the TS browser worker pool, and the future Python IFC->GLB companion — the content key is byte-identical or it is a silent cross-runtime cache miss.
- The two-mode IaC tier provisioning the observability stack the C# service tier and the TS SPA both export to and read from.
- Four cross-branch precondition-DAG concert frontiers the branch observes but cannot pre-author: GraphFork CRDT, the capability-descriptor SDK + MCP client, the BCF anchor algebra, and the GlbViewport mesh render.
