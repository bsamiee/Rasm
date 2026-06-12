# [SUITE_REGION_MAP]

Suite ownership ledger for the four app-package planning corpora. Binding precedence: locked campaign decisions, then adversarial-verifier and attack corrections, then synthesis closures, then design JSON. Authors append provisional rows before writing a page; the cold grader flips rows to FINAL on the all-PASS sweep. Authoring position: AppHost, Persistence, Compute, AppUi; charter PAGE_INDEX order within a package.

## [1]-[PAGE_REGIONS]

[APPHOST]:

- host-profiles.md [PROVISIONAL]: one HostProfile axis resolves every modality variance as row columns; the resolved-profile record is the only profile artifact siblings consume.
- lifecycle-and-drain.md [PROVISIONAL]: closed RuntimePhase family with total transition law, drain conductor over frozen rank bands, cancellation spine, FaultSource union, crash markers.
- time-and-deadlines.md [PROVISIONAL]: IClock/TimeProvider seams, DeadlineClass taxonomy, schedule port with CronExpression record, fake pairs on the test row.
- configuration-and-options.md [PROVISIONAL]: ConfigSource axis with rank + reload-class columns, source-generated binding, validated frozen policy values, receipted reload.
- composition-and-modules.md [PROVISIONAL]: one composition root per process; ModuleContribution rows; scanning/decoration/keyed-service law.
- resource-lanes.md [PROVISIONAL]: HybridCache port (stampede, tags, entry options), ObjectPool policy rows, DrainQueue rows.
- diagnostics-and-telemetry.md [PROVISIONAL]: correlation spine, TelemetrySignal governance, DataClassification taxonomy, contributor consumption.
- health-and-degradation.md [PROVISIONAL]: HealthContributor fold, DegradationLevel rail incl. LocalOnly, grpc.health.v1 tag-predicate mapping, resource-pressure row.
- support-bundles.md [PROVISIONAL]: SupportTrigger union, window freeze, contributor fan-in, redaction, cap, manifest, receipt.
- outbound-resilience.md [PROVISIONAL]: OutboundHop axis (7 cases incl. update-check), one keyed Polly pipeline per hop, discovery manifest law, conflict evidence.
- runtime-ports.md [PROVISIONAL]: typed port records (TelemetryContributor, ReceiptSink with HLC envelope, DrainParticipant, HostAttach, UiScheduler, Support, Health), suite wire law, TS tooling map, contract-merge rule.

[PERSISTENCE]:

- store-profiles.md [PROVISIONAL]: six StoreProfile rows with the widened row record (pooling, execution strategy, concurrency, seeding, maintenance, restore, cross-process lease) and placement/provisioning law.
- data-lanes.md [PROVISIONAL]: seven DataLane rows mapped to engine capabilities and extension rows incl. geometry/geo lanes and the analytical lane.
- schema-rail.md [PROVISIONAL]: IdentityPolicy axis, migration law, generated columns, HasPostgresExtension rows, naming, concurrency tokens, Thinktecture EF converters.
- query-rail.md [PROVISIONAL]: StoreOp total dispatch, pooled context factories, bulk lane with self-emitted changefeed, MERGE/RETURNING delta projection, interceptor receipts.
- native-sqlite.md [PROVISIONAL]: verified e_sqlite3 compile-flag surface, WAL/busy law, loadable-extension route, encryption and vector gates.
- snapshot-codecs.md [PROVISIONAL]: SnapshotCodec/CompressionPolicy/HashPolicy axes, restore choreography, wire contracts (STJ + MessagePack + GeoJSON projection).
- cache-indexes.md [PROVISIONAL]: L2 IDistributedCache + serializer factory rows, model-result and artifact-blob indexes.
- sync-collaboration.md [PROVISIONAL]: SyncTransport axis with M1-M5 rows, op-log HLC changefeed, HttpDelta + JSON-Patch fallback, presence, conflict receipts, BlobRemote frame consumption.
- redaction-retention.md [PROVISIONAL]: RetentionPolicy rows, classification enforcement consuming the AppHost taxonomy, clock-seam stamping, audit binding.

[COMPUTE]:

- intent-and-selection.md [PROVISIONAL]: typed intent family, substrate-selection rail with ordered predicates and declared fallback rows, one total dispatch surface; wasm is a platform predicate.
- tensor-lane.md [PROVISIONAL]: CpuTensor substrate, verified TensorPrimitives/layout families, shape algebra, geometry-to-tensor encoding rows.
- model-lane.md [PROVISIONAL]: model identity and provenance, session capsule, EP-parameterized Onnx row (CoreML verified spellings), extension-op admission, warm-start artifact routing.
- remote-lane.md [PROVISIONAL]: the suite proto vocabulary (ComputeService, DocumentService, ControlService, ArtifactSync frame law, grpc.health.v1, FaultDetail), transport rows with streaming-capability and keepalive and node-affinity columns, CredentialPolicy axis, discovery manifest consumption, descriptor-diff contract evolution.
- staging-and-streams.md [PROVISIONAL]: AllocationClass rows with rent/return delegates, recyclable stream policy, zero-copy edges.
- scheduling-and-lanes.md [PROVISIONAL]: WorkLane bounded channels, solve-path guard, backpressure, drain participation.
- progress-and-observation.md [PROVISIONAL]: monotonic ProgressPhase rows, zero-alloc capsules, scheduler-policy-carrying subscriptions, remote projection.
- units-boundary.md [PROVISIONAL]: QuantityFamily rows, conversion-at-admission, parse/format edges, unit evidence.
- receipts-and-benchmarks.md [PROVISIONAL]: 13-case receipt union, fold projections, NodaTime-protobuf stamps, fingerprint-gated benchmark claims, artifact blob routing.

[APPUI]:

- surface-hosts.md [PROVISIONAL]: SurfaceHost axis with marshal rows, mount transaction, embedding gates; web-browser designed-only.
- shell-navigation.md [PROVISIONAL]: shell composition, routing, dock layouts with checkpoint and crash-restore columns.
- screens-activation.md [PROVISIONAL]: screen family, activation scopes, lifecycle binding.
- commands-availability.md [PROVISIONAL]: one CommandIntent table; derived menus/toolbars/palette/deep-links/remote; fuzzy-search index; hotkeys.
- live-data.md [PROVISIONAL]: DataSource axis, change-set engine depth, the live-data spine composite, IdentityPolicy-keyed caches.
- tables-hierarchy.md [PROVISIONAL]: TableProjection rows, virtualization, tree-flatten fold, grid-edit commit path.
- inspector-editing.md [PROVISIONAL]: EditorFactory rows, validation-to-save gate, options inspector, value-object and quantity editors, conflict-resolution row.
- charts-dashboards.md [PROVISIONAL]: ChartSeriesSpec rows incl. geo/map, dashboard composition, benchmark and timeline dashboards.
- visuals-offscreen.md [PROVISIONAL]: offscreen rendering, render-hash proof, DocumentExport cluster.
- theme-tokens.md [PROVISIONAL]: orthogonal ThemeVariant and Density families, token resolution, host-matched probing, switch atomicity.
- typography-shaping.md [PROVISIONAL]: TypographyRole rows, shaping pipeline, MarkdownProjection fold.
- icons-assets.md [PROVISIONAL]: IconSource rows with fallback ranks, asset loading, geo asset routing.
- dialogs-notifications.md [PROVISIONAL]: DialogIntent rows, session algebra, notification policy incl. drain suppression.
- input-interaction.md [PROVISIONAL]: pointer/keyboard/gesture rows, pan-zoom canvases.
- motion-tokens.md [PROVISIONAL]: MotionToken rows with reduced pairs, phase mapping, perceptual color interpolation.
- accessibility.md [PROVISIONAL]: automation peers, contrast governance, reduced-motion law.
- localization-culture.md [PROVISIONAL]: LocaleRow on inbox resx/ICU/NodaTime, culture composition.
- diagnostics-evidence.md [PROVISIONAL]: EvidenceReceipt union, correlation join fold, headless proof derivation, provenance projection.

## [2]-[SIGNATURE_REGIONS]

- AH-04a SOURCE_AXIS: ConfigSource [SmartEnum<string>], ConfigSourceKeyPolicy, ReloadClass [SmartEnum<string>], ConfigLayer — spotlight: the rank+reload-class source axis with per-source factory delegate and precedence fold.
- AH-04b TYPED_BINDING: PolicyBinding, ConfigError — spotlight: source-generated AOT-safe bind into immutable policy records with fail-closed unknown-key admission folding into Validation<ConfigError,T>.
- AH-04c POLICY_VALUES: OptionsAdmission, ReloadReceipt, ReloadOutcome [Union] — spotlight: validate-once frozen policy publish, reload-class-gated OptionsMonitor transition, ControlService reload-options consequence.
- AH-04d KILL_SWITCH: KillSwitchConfig, OperatorOverride [Union] — spotlight: operator-forced degradation as a reload-class:transition config row feeding the health-and-degradation fold.

## [3]-[OWNER_SYMBOLS]

- ConfigSource — AppHost/configuration-and-options#SOURCE_AXIS [SmartEnum<string>]
- ConfigSourceKeyPolicy — AppHost/configuration-and-options#SOURCE_AXIS [comparer accessor]
- ReloadClass — AppHost/configuration-and-options#SOURCE_AXIS [SmartEnum<string>]
- ConfigLayer — AppHost/configuration-and-options#SOURCE_AXIS [record]
- PolicyBinding — AppHost/configuration-and-options#TYPED_BINDING [static surface]
- ConfigError — AppHost/configuration-and-options#TYPED_BINDING [Union fault]
- OptionsAdmission — AppHost/configuration-and-options#POLICY_VALUES [static surface]
- ReloadReceipt — AppHost/configuration-and-options#POLICY_VALUES [record]
- ReloadOutcome — AppHost/configuration-and-options#POLICY_VALUES [Union]
- KillSwitchConfig — AppHost/configuration-and-options#KILL_SWITCH [record]
- OperatorOverride — AppHost/configuration-and-options#KILL_SWITCH [Union]

## [4]-[SEAM_SPLITS]

- Config reload propagation: mechanics at AppHost/configuration-and-options#POLICY_VALUES (reload-class transition + ReloadReceipt); consequence at Persistence user-settings write and the op-log HLC tag-invalidation cursor (cross-process peer observation); frozen reload-class requires process restart.
- Operator kill-switch: mechanics at AppHost/configuration-and-options#KILL_SWITCH (KillSwitchConfig row, OperatorOverride union); consequence at AppHost/health-and-degradation (forced DegradationLevel input to the fold) and the ControlService set-degradation/reload-options inbound verbs for service modalities.

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
