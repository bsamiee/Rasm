# [TS_REGION_MAP_OWNER_SYMBOLS]

TS branch ledger — row shapes and protocol live in the [suite standard](../../../csharp/.planning/README.md) ledger-protocol section. The five app-services head the registry as the closed STATE/GATEWAY budget; every name is unique within the TS branch and grepped clean against the suite owner-symbol registry. Anchors are bare-domain-qualified (`<domain>/page#CLUSTER`). The AiProvider literal is folded under ActivityOwner as its provider axis (sole declaration site), the four prior geo aliases are folded under one GeoSeriesLayer union owner, the AssetTransfer format axis folds the four export codecs, and CommandGateway + IntentRegistry are homed to interchange as the outbound face of WireClients by outbound-effect altitude. The browser stratum is two folders: `ui` (UI/UX/components, the AppUi-analog library) and `platform` (the AppHost-analog SPA infrastructure entry); the five platform infrastructure owners (AppRouter/ServiceWorkerHost/CrashTelemetry/RemoteConfig/PerformanceBudget) are platform-bound HOST owners, NOT app-services — the closed five-app-service budget is unchanged.

[APP_SERVICE_BUDGET] (closed at five):

- WireClients — interchange/transport#TRANSPORT_AND_CLIENTS [Effect.Service]
- CommandGateway — interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [Effect.Service]
- SnapshotFeed — projection/fold-algebra#FOLD_ALGEBRA [Effect.Service]
- RuntimeFeed — projection/fold-algebra#FOLD_ALGEBRA [Effect.Service]
- EvidenceFeed — projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [Effect.Service]

[INTERCHANGE]:

- WireTransport — interchange/transport#TRANSPORT_AND_CLIENTS [transport owner]
- TransportCapabilityWire — interchange/transport#TRANSPORT_AND_CLIENTS [Schema.Literal capability axis]
- MethodShape — interchange/transport#TS_PROJECTION [proto rpc key]
- DecodeRail — interchange/codec-rails#CODEC_RAILS [six-codec rail family]
- EncodeRail — interchange/codec-rails#CODEC_RAILS [encode write-face direction row on DecodeRail]
- SchemaRefinement — interchange/codec-rails#CODEC_RAILS [brand/filter decode-enforcement rows: Guid/ContentKey/OrdinalKey/HlcLogical/HeaderDiscriminant]
- GeometryRail — interchange/codec-rails#CODEC_RAILS [embedded-geometry rail]
- ArtifactFrameRail — interchange/codec-rails#CODEC_RAILS [server-stream content-addressed artifact rail; SPIKE on the XxHash128 byte-identity harness]
- FaultDetail — interchange/codec-rails#FAULT_FAMILY [Data.TaggedError family over the full ComputeFault/StoreFault/HopFault/ConfigError set, Match.tagsExhaustive]
- FaultDetailRail — interchange/codec-rails#FAULT_FAMILY [trailer-to-FaultDetail reconstruction]
- QuarantineFold — interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [tolerance terminal]
- IntentRegistry — interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE [deep-link key vocabulary]

[PROJECTION]:

- StreamPolicy — projection/fold-algebra#FOLD_ALGEBRA [one Schedule reconnect + Stream-operator vocabulary]
- keyedFold — projection/fold-algebra#FOLD_ALGEBRA [the key-discriminated fold combinator]
- HealthStore — projection/fold-algebra#FOLD_ALGEBRA [fold row]
- ProgressStore — projection/fold-algebra#FOLD_ALGEBRA [monotonic fold row]
- ConflictPresenceStore — projection/fold-algebra#FOLD_ALGEBRA [fold row]
- ReceiptStore — projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [compute-receipt fold row]
- AvailabilityStore — projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [read-side fold the gateway reads across the boundary]
- ReceiptEnvelopeCarrier — projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [Schema factory binding every structured-text payload]
- SkewBand — projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE [HLC skew-band confidence-interval projection output]

[UI]:

- AtomBinding — ui/binding#BINDING [Effect.Service; the sole React state binding + dev inspector row]
- DeepLinkBinding — ui/binding#BINDING [query-string state]
- UndoStack — ui/binding#BINDING [Effect-native undo/redo fold]
- OfflineState — ui/binding#BINDING [offline-state client fold reading the platform LocalPersistence last-good cell]
- EvidenceTimelineRoute — ui/render-surfaces#RENDER_SURFACES [HLC-ordered route subscriber]
- BenchmarkRoute — ui/render-surfaces#RENDER_SURFACES [fingerprint-gated route subscriber]
- CollectorPanel — ui/render-surfaces#RENDER_SURFACES [telemetry collector reader]
- GeoSeriesSurface — ui/render-surfaces#RENDER_SURFACES [2D geo surface]
- GeoSeriesLayer — ui/render-surfaces#RENDER_SURFACES [one Schema.Literal union folding the four prior geo aliases]
- InteractionRole — ui/component-system#COMPONENT_SYSTEM [one Schema.Literal interaction-role vocabulary owner-block over the eight roles]
- RoleBehavior — ui/component-system#COMPONENT_SYSTEM [one behavior contract per interaction role]
- ThemeTokens — ui/component-system#COMPONENT_SYSTEM [color-space token owner]
- CssVarSync — ui/component-system#COMPONENT_SYSTEM [runtime CSS-var sync]
- GlbViewport — ui/render-surfaces#RESEARCH [REFINEMENT_HORIZON; WebGL mesh render, renderer-backend Schema.Literal axis incl. webgpu; no leaf this turn]

[PLATFORM]:

- CompositionRoot — platform/host-runtime#HOST_RUNTIME [Effect.Service; one Layer graph composing ui+interchange+projection; platform-bound host owner, not an app-service]
- BrowserPlatform — platform/host-runtime#HOST_RUNTIME [browser platform layer; platform-bound host owner]
- AuthSession — platform/host-runtime#HOST_RUNTIME [Effect.Service; arctic OIDC/PKCE credential owner; platform-bound host owner, not an app-service]
- RuntimeConfig — platform/host-runtime#HOST_RUNTIME [one Config schema + provider layer]
- SelfTelemetry — platform/platform-substrate#PLATFORM_SUBSTRATE [host OTLP-web export edge]
- MetricRegistry — platform/platform-substrate#PLATFORM_SUBSTRATE [bounded instrument + span vocabulary over the WebSdk incl. the Core Web Vitals instrument family]
- BuildPipeline — platform/platform-substrate#PLATFORM_SUBSTRATE [build pipeline; emits the PWA worker asset, lifecycle owned by ServiceWorkerHost]
- DecodeWorkerPool — platform/platform-substrate#PLATFORM_SUBSTRATE [Worker.makePool transferable-buffer offload]
- LocalPersistence — platform/platform-substrate#PLATFORM_SUBSTRATE [KeyValueStore over idb-keyval as backing only; holds the offline command queue]
- AppRouter — platform/routing-navigation#ROUTING_NAVIGATION [Effect.Service over a Schema.Literal route-key axis + history SubscriptionRef; platform-bound host owner, not an app-service; ZERO routing package admitted]
- NavigationGuard — platform/routing-navigation#ROUTING_NAVIGATION [route guard fold over AuthSession.status + projection AvailabilityStore]
- RouteParamCodec — platform/routing-navigation#ROUTING_NAVIGATION [Schema route-param round-trip composing nuqs]
- ServiceWorkerHost — platform/service-worker#SERVICE_WORKER [Effect.Service over the registration/update lifecycle; platform-bound host owner, not an app-service]
- CacheStrategy — platform/service-worker#SERVICE_WORKER [Schema.Literal route-strategy axis: cache-first/network-first/stale-while-revalidate]
- BackgroundSyncReplay — platform/service-worker#SERVICE_WORKER [offline-queue redial drain into interchange CommandGateway, reading LocalPersistence offline queue]
- CrashTelemetry — platform/error-boundary#ERROR_BOUNDARY [Effect.Service over global error capture + crash fold; platform-bound host owner, not an app-service]
- ErrorBoundaryBinding — platform/error-boundary#ERROR_BOUNDARY [react-error-boundary integration emitting to CrashTelemetry]
- CrashReport — platform/error-boundary#ERROR_BOUNDARY [sanitized exception envelope shipped via SelfTelemetry, reconstructing uncaught faults as the interchange FaultDetail family]
- RemoteConfig — platform/feature-flags-config#FEATURE_FLAGS_CONFIG [Effect.Service over the FlagSet fold + refresh Schedule; platform-bound host owner, not an app-service]
- FlagKey — platform/feature-flags-config#FEATURE_FLAGS_CONFIG [Schema.Literal flag axis referencing the services FeatureFlags vocabulary]
- FlagEvaluation — platform/feature-flags-config#FEATURE_FLAGS_CONFIG [Match total dispatch over the services bucket/variant resolution]
- PerformanceBudget — platform/web-vitals#WEB_VITALS [Effect.Service over the web-vitals capture + budget fold; platform-bound host owner, not an app-service]
- VitalMetric — platform/web-vitals#WEB_VITALS [Schema.Literal vital axis feeding the existing MetricRegistry Core-Web-Vitals instrument rows]
- BudgetThreshold — platform/web-vitals#WEB_VITALS [data-driven Record gating the budget-exceeded fold]

[SERVICES]:

- WorkflowOwner — services/durable-execution#DURABLE_EXECUTION [durable-unit owner]
- ActivityOwner — services/durable-execution#DURABLE_EXECUTION [durable-unit owner; aiActivity holds the AiProvider axis]
- ClusterEngine — services/durable-execution#DURABLE_EXECUTION [WorkflowEngine wiring]
- AiProvider — services/durable-execution#DURABLE_EXECUTION [Schema.Literal provider axis; sole declaration site]
- AgentJournal — services/durable-execution#DURABLE_EXECUTION [Model.Class durable agent ledger]
- Resilience — services/durable-execution#DURABLE_EXECUTION [circuit-breaker/retry/rfc6902 primitives]
- SqlBoundary — services/persistence#STORE_BOUNDARY [single PgClient/Migrator owner]
- EntityRegistry — services/persistence#STORE_BOUNDARY [~15 Model.Class rows, projections via Model.fields/Schema.pick]
- TenantScope — services/persistence#TENANCY [multi-tenant RLS axis: app.current_tenant GUC + lifecycle + purge family]
- WorkQueue — services/persistence#WORK_AND_SIGNALS [Job priority/status + JobDlq dead-letter]
- EventJournal — services/persistence#WORK_AND_SIGNALS [retainable tenant-scoped event ledger]
- Notifications — services/persistence#WORK_AND_SIGNALS [multi-channel preference matrix + delivery state machine]
- AssetTransfer — services/persistence#WORK_AND_SIGNALS [Schema.Literal export-codec axis: csv/xlsx/pdf/archive]
- FeatureFlags — services/persistence#WORK_AND_SIGNALS [percentage-rollout buckets; sole declaration site of the flag bucket/variant vocabulary the platform RemoteConfig consumes]
- HybridSearch — services/hybrid-search#HYBRID_SEARCH [fused weighted-rank owner over four signals]
- InternalRpc — services/internal-rpc#INTERNAL_RPC [one RpcGroup; WorkflowProxy derived over the same wire Schema]
- RunnerBackplane — services/internal-rpc#RUNNER_AND_SCHEDULING [4-row protocol/message-storage/runner-storage/runner-health backplane + snowflake]
- ScheduledWork — services/internal-rpc#RUNNER_AND_SCHEDULING [cluster singleton + shard-pinned cron]
- TierStack — services/provisioning#PROVISIONING [data/compute/observe ComponentResources, two-mode DeployMode dispatch]
- AutomationDriver — services/provisioning#PROVISIONING [@effect/cli up/preview/refresh/destroy verb tree over the Automation API]
- SecretResolver — services/provisioning#PROVISIONING [Doppler/ESC/config secret boundary]
- PolicyGuard — services/provisioning#PROVISIONING [engine/program policy guardrails]
- StackOutputs — services/provisioning#PROVISIONING [typed cross-stack StackReference topology contract]
- ObservabilityStack — services/provisioning#PROVISIONING [collector tier: Alloy OTLP -> Prometheus -> Grafana]

[BRANCH] (CROSS_PACKAGE_LAWS, non-domain):

- UnitProperty — branch/test-strategy#UNIT_AND_PROPERTY [PBT spec spine]
- BrowserE2E — branch/test-strategy#BROWSER_AND_E2E [DOM-mode + navigation runner; one browser-mode project covers both ui and platform specs]
- MutationHarness — branch/test-strategy#MUTATION_AND_HARNESS [stryker kill-ratio gate + DurableHarness container harness]
