# [TS_REGION_MAP_PAGE_REGIONS]

TS branch ledger — one ledger with four per-domain blocks plus the branch non-domain block, mirroring the C# suite ledger shape. Row shapes and protocol live in the [suite standard](../../../csharp/.planning/README.md) ledger-protocol section. Per the single-state-surface law, page-finalization state lives only in the owning charter PAGE_INDEX `[STATE]` column; this ledger carries the one-sentence concern law per page and no page-state cell, routing to the charter for state.

[@RASM/INTERCHANGE]:

- interchange/transport.md: WireTransport + WireClients over one shared grpc-web transport, TransportCapabilityWire gating the grpcWeb method set, the polymorphic correlation/trace/credential interceptor, and the buf descriptor pipeline as the build-time codegen input edge emitting src/gen/*_pb.ts.
- interchange/codec-rails.md: the single DecodeRail six-codec family read by codec key, EncodeRail as the direction-keyed write face, SchemaRefinement brand/filter decode-enforcement rows, GeometryRail embedded-geometry, ArtifactFrameRail content-addressed reassembly, and FaultDetailRail reconstructing the full ComputeFault/StoreFault/HopFault/ConfigError set as one Data.TaggedError family bound by Match.tagsExhaustive.
- interchange/gateway-and-quarantine.md: QuarantineFold tolerance terminal, the absorbed CONTRACT_INVENTORY and versioning-law fences, and CommandGateway + IntentRegistry as the outbound-verb face of WireClients co-located in the transport-owning domain by outbound-effect altitude.

[@RASM/PROJECTION]:

- projection/fold-algebra.md: the one StreamPolicy bounded-Schedule reconnect + Stream-operator vocabulary, the keyedFold combinator over a Stream/receipt sequence into a SubscriptionRef keyed map, and the five stream stores RuntimeFeed/HealthStore/SnapshotFeed/ProgressStore/ConflictPresenceStore, transport-free.
- projection/envelope-and-evidence.md: ReceiptStore/EvidenceFeed/AvailabilityStore as fold rows of the same algebra, the ReceiptEnvelopeCarrier binding every structured-text payload, and the SkewBand HLC correlation fold as a first-class confidence-interval projection output.

[@RASM/WEB]:

- web/binding.md: AtomBinding the sole React state binding, DeepLinkBinding query-string state, the url-as-state/undo-redo/offline client-state folds over the projection stores, and the development-build atom-inspector row.
- web/render-surfaces.md: the read-only observation routes EvidenceTimelineRoute (HLC-ordered with SkewBand confidence interval), BenchmarkRoute (fingerprint-gated), and CollectorPanel, plus GeoSeriesSurface over one Schema.Literal GeoSeriesLayer union owner; GlbViewport a REFINEMENT_HORIZON row.
- web/component-system.md: the one InteractionRole Schema.Literal vocabulary owner-block over the eight roles with one RoleBehavior contract per role, ThemeTokens color-space tokens, and CssVarSync runtime CSS-var sync; the react-aria per-component .tsx patterns discarded.
- web/host-runtime.md: CompositionRoot one Layer graph providing the closed five app-services, BrowserPlatform, AuthSession arctic OIDC/PKCE credential boot edge, and RuntimeConfig typed-config owner; the browser publication entry.
- web/platform-substrate.md: SelfTelemetry host OTLP-web export edge plus MetricRegistry bounded vocabulary over the WebSdk, BuildPipeline build/PWA pipeline, DecodeWorkerPool Worker.makePool transferable-buffer offload including off-main-thread ArtifactFrameRail reassembly, and LocalPersistence over the Effect KeyValueStore composing idb-keyval as backing only.

[@RASM/SERVICES]:

- services/durable-execution.md: WorkflowOwner + ActivityOwner closed durable-unit vocabulary, ClusterEngine cluster-backed WorkflowEngine wiring, the ai-activity over the sole AiProvider Schema.Literal axis, the AgentJournal durable agent ledger, and the Resilience circuit-breaker/retry/rfc6902 primitives.
- services/persistence.md: SqlBoundary the single PgClient/Migrator owner, the ~15-entity EntityRegistry bound to ONE Model.Class per entity with Model.fields/Schema.pick projections, TenantScope multi-tenant RLS, WorkQueue/JobDlq, EventJournal, Notifications matrix, the AssetTransfer export-codec axis, and FeatureFlags percentage buckets.
- services/hybrid-search.md: HybridSearch the single fused weighted-rank owner over semantic (HNSW efSearch) + lexical (tsvector) + trigram (pg_trgm) + phonetic signals with embedding-profile dimensioning and staleness tracking.
- services/internal-rpc.md: InternalRpc one RpcGroup with WorkflowProxy derived over the same wire Schema, plus RunnerBackplane 4-row protocol/message-storage/runner-storage/runner-health backplane with the snowflake id source and ScheduledWork cluster singleton + shard-pinned cron.
- services/provisioning.md: TierStack data/compute/observe ComponentResources with two-mode DeployMode dispatch (cloud K8s vs self-hosted compose+Traefik), the entry-thin/impl-dense split behind the ./provisioning subpath, StackOutputs cross-stack topology, SecretResolver Doppler boundary, the idempotent bootstrap, the self-hosted equivalence map, and ObservabilityStack Alloy->Prometheus->Grafana.

[BRANCH] (CROSS_PACKAGE_LAWS, non-domain):

- architecture-posture.md: the seven-altitude one-form grammar, the closed five-app-service budget, the platform-neutrality strata, the held Effect-v3 runtime + dual-compiler tsgo floor, the admission doctrine, the anti-spam cluster law + altitude-co-location law, and the RULE_ENFORCEMENT six-guard table.
- test-strategy.md: the algebraic PBT spine with per-file LOC and coverage floors, the browser-mode and end-to-end runner, and the mutation kill-ratio gate plus durable-cluster harness — per-domain project rows in one shared vitest.workspace.ts.
- architecture-tree.md: the flat four-domain source tree, the inter-domain Nx scope tag graph, the browser/node/neutral publication split as a tag attribute, the descriptor-pipeline placement in @rasm/interchange, and the GlbViewport refinement-horizon entry.
