# [TS_REGION_MAP_OWNER_SYMBOLS]

TS branch ledger — row shapes and protocol live in the [suite standard](../../../csharp/.planning/README.md) ledger-protocol section. The five app-services head the registry as the closed STATE/GATEWAY budget; every name is unique within the TS branch and grepped clean against the suite owner-symbol registry. Anchors are domain-qualified (`@rasm/<domain>/page#CLUSTER`). The AiProvider literal is folded under ActivityOwner as its provider axis (sole declaration site), the four prior geo aliases are folded under one GeoSeriesLayer union owner, the AssetTransfer format axis folds the four export codecs, and CommandGateway + IntentRegistry are homed to @rasm/interchange as the outbound face of WireClients by outbound-effect altitude.

[APP_SERVICE_BUDGET] (closed at five):

- WireClients — @rasm/interchange/transport#TRANSPORT_AND_CLIENTS [Effect.Service]
- CommandGateway — @rasm/interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [Effect.Service]
- SnapshotFeed — @rasm/projection/fold-algebra#FOLD_ALGEBRA [Effect.Service]
- RuntimeFeed — @rasm/projection/fold-algebra#FOLD_ALGEBRA [Effect.Service]
- EvidenceFeed — @rasm/projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [Effect.Service]

[@RASM/INTERCHANGE]:

- WireTransport — @rasm/interchange/transport#TRANSPORT_AND_CLIENTS [transport owner]
- TransportCapabilityWire — @rasm/interchange/transport#TRANSPORT_AND_CLIENTS [Schema.Literal capability axis]
- MethodShape — @rasm/interchange/transport#TS_PROJECTION [proto rpc key]
- DecodeRail — @rasm/interchange/codec-rails#CODEC_RAILS [six-codec rail family]
- EncodeRail — @rasm/interchange/codec-rails#CODEC_RAILS [encode write-face direction row on DecodeRail]
- SchemaRefinement — @rasm/interchange/codec-rails#CODEC_RAILS [brand/filter decode-enforcement rows: Guid/ContentKey/OrdinalKey/HlcLogical/HeaderDiscriminant]
- GeometryRail — @rasm/interchange/codec-rails#CODEC_RAILS [embedded-geometry rail]
- ArtifactFrameRail — @rasm/interchange/codec-rails#CODEC_RAILS [server-stream content-addressed artifact rail; SPIKE on the XxHash128 byte-identity harness]
- FaultDetail — @rasm/interchange/codec-rails#FAULT_FAMILY [Data.TaggedError family over the full ComputeFault/StoreFault/HopFault/ConfigError set, Match.tagsExhaustive]
- FaultDetailRail — @rasm/interchange/codec-rails#FAULT_FAMILY [trailer-to-FaultDetail reconstruction]
- QuarantineFold — @rasm/interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [tolerance terminal]
- IntentRegistry — @rasm/interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [deep-link key vocabulary]

[@RASM/PROJECTION]:

- StreamPolicy — @rasm/projection/fold-algebra#FOLD_ALGEBRA [one Schedule reconnect + Stream-operator vocabulary]
- keyedFold — @rasm/projection/fold-algebra#FOLD_ALGEBRA [the key-discriminated fold combinator]
- HealthStore — @rasm/projection/fold-algebra#FOLD_ALGEBRA [fold row]
- ProgressStore — @rasm/projection/fold-algebra#FOLD_ALGEBRA [monotonic fold row]
- ConflictPresenceStore — @rasm/projection/fold-algebra#FOLD_ALGEBRA [fold row]
- ReceiptStore — @rasm/projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [compute-receipt fold row]
- AvailabilityStore — @rasm/projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [read-side fold the gateway reads across the boundary]
- ReceiptEnvelopeCarrier — @rasm/projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [Schema factory binding every structured-text payload]
- SkewBand — @rasm/projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [HLC skew-band confidence-interval projection output]

[@RASM/WEB]:

- AtomBinding — @rasm/web/binding#BINDING [Effect.Service; the sole React state binding + dev inspector row]
- DeepLinkBinding — @rasm/web/binding#BINDING [query-string state]
- UndoStack — @rasm/web/binding#BINDING [Effect-native undo/redo fold]
- EvidenceTimelineRoute — @rasm/web/render-surfaces#RENDER_SURFACES [HLC-ordered route subscriber]
- BenchmarkRoute — @rasm/web/render-surfaces#RENDER_SURFACES [fingerprint-gated route subscriber]
- CollectorPanel — @rasm/web/render-surfaces#RENDER_SURFACES [telemetry collector reader]
- GeoSeriesSurface — @rasm/web/render-surfaces#RENDER_SURFACES [2D geo surface]
- GeoSeriesLayer — @rasm/web/render-surfaces#RENDER_SURFACES [one Schema.Literal union folding the four prior geo aliases]
- InteractionRole — @rasm/web/component-system#COMPONENT_SYSTEM [one Schema.Literal interaction-role vocabulary owner-block over the eight roles]
- ThemeTokens — @rasm/web/component-system#COMPONENT_SYSTEM [color-space token owner]
- CssVarSync — @rasm/web/component-system#COMPONENT_SYSTEM [runtime CSS-var sync]
- CompositionRoot — @rasm/web/host-runtime#HOST_RUNTIME [Effect.Service; one Layer graph]
- BrowserPlatform — @rasm/web/host-runtime#HOST_RUNTIME [browser platform layer]
- AuthSession — @rasm/web/host-runtime#HOST_RUNTIME [Effect.Service; arctic OIDC/PKCE credential owner]
- RuntimeConfig — @rasm/web/host-runtime#HOST_RUNTIME [one Config schema + provider layer]
- SelfTelemetry — @rasm/web/platform-substrate#PLATFORM_SUBSTRATE [host OTLP-web export edge]
- MetricRegistry — @rasm/web/platform-substrate#PLATFORM_SUBSTRATE [bounded instrument + span vocabulary over the WebSdk]
- BuildPipeline — @rasm/web/platform-substrate#PLATFORM_SUBSTRATE [build/PWA/styling pipeline]
- DecodeWorkerPool — @rasm/web/platform-substrate#PLATFORM_SUBSTRATE [Worker.makePool transferable-buffer offload]
- LocalPersistence — @rasm/web/platform-substrate#PLATFORM_SUBSTRATE [KeyValueStore over idb-keyval as backing only]
- GlbViewport — @rasm/web/render-surfaces#RESEARCH [REFINEMENT_HORIZON; WebGL mesh render, renderer-backend Schema.Literal axis incl. webgpu; no leaf this turn]

[@RASM/SERVICES]:

- WorkflowOwner — @rasm/services/durable-execution#DURABLE_EXECUTION [durable-unit owner]
- ActivityOwner — @rasm/services/durable-execution#DURABLE_EXECUTION [durable-unit owner; aiActivity holds the AiProvider axis]
- ClusterEngine — @rasm/services/durable-execution#DURABLE_EXECUTION [WorkflowEngine wiring]
- AiProvider — @rasm/services/durable-execution#DURABLE_EXECUTION [Schema.Literal provider axis; sole declaration site]
- AgentJournal — @rasm/services/durable-execution#DURABLE_EXECUTION [Model.Class durable agent ledger]
- Resilience — @rasm/services/durable-execution#DURABLE_EXECUTION [circuit-breaker/retry/rfc6902 primitives]
- SqlBoundary — @rasm/services/persistence#STORE_BOUNDARY [single PgClient/Migrator owner]
- EntityRegistry — @rasm/services/persistence#STORE_BOUNDARY [~15 Model.Class rows, projections via Model.fields/Schema.pick]
- TenantScope — @rasm/services/persistence#TENANCY [multi-tenant RLS axis: app.current_tenant GUC + lifecycle + purge family]
- WorkQueue — @rasm/services/persistence#WORK_AND_SIGNALS [Job priority/status + JobDlq dead-letter]
- EventJournal — @rasm/services/persistence#WORK_AND_SIGNALS [retainable tenant-scoped event ledger]
- Notifications — @rasm/services/persistence#WORK_AND_SIGNALS [multi-channel preference matrix + delivery state machine]
- AssetTransfer — @rasm/services/persistence#WORK_AND_SIGNALS [Schema.Literal export-codec axis: csv/xlsx/pdf/archive]
- FeatureFlags — @rasm/services/persistence#WORK_AND_SIGNALS [percentage-rollout buckets]
- HybridSearch — @rasm/services/hybrid-search#HYBRID_SEARCH [fused weighted-rank owner over four signals]
- InternalRpc — @rasm/services/internal-rpc#INTERNAL_RPC [one RpcGroup; WorkflowProxy derived over the same wire Schema]
- RunnerBackplane — @rasm/services/internal-rpc#RUNNER_AND_SCHEDULING [4-row protocol/message-storage/runner-storage/runner-health backplane + snowflake]
- ScheduledWork — @rasm/services/internal-rpc#RUNNER_AND_SCHEDULING [cluster singleton + shard-pinned cron]
- TierStack — @rasm/services/provisioning#PROVISIONING [data/compute/observe ComponentResources, two-mode DeployMode dispatch]
- AutomationDriver — @rasm/services/provisioning#PROVISIONING [@effect/cli up/preview/refresh/destroy verb tree over the Automation API]
- SecretResolver — @rasm/services/provisioning#PROVISIONING [Doppler/ESC/config secret boundary]
- PolicyGuard — @rasm/services/provisioning#PROVISIONING [engine/program policy guardrails]
- StackOutputs — @rasm/services/provisioning#PROVISIONING [typed cross-stack StackReference topology contract]
- ObservabilityStack — @rasm/services/provisioning#PROVISIONING [collector tier: Alloy OTLP -> Prometheus -> Grafana]

[BRANCH] (CROSS_PACKAGE_LAWS, non-domain):

- UnitProperty — branch/test-strategy#UNIT_AND_PROPERTY [PBT spec spine]
- BrowserE2E — branch/test-strategy#BROWSER_AND_E2E [DOM-mode + navigation runner]
- MutationHarness — branch/test-strategy#MUTATION_AND_HARNESS [stryker kill-ratio gate + DurableHarness container harness]
