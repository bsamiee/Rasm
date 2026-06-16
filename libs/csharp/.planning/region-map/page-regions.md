# [REGION_MAP_PAGE_REGIONS]

Suite region map — row shapes and protocol live in the [suite standard](../README.md) ledger-protocol section.

[APPHOST]:

- host-profiles.md [FINAL]: one HostProfile axis resolves every modality variance as row columns; the resolved-profile record is the only profile artifact siblings consume.
- lifecycle-and-drain.md [FINAL]: closed RuntimePhase family with total transition law, drain conductor over frozen rank bands, cancellation spine, FaultSource union, crash markers.
- time-and-deadlines.md [FINAL]: IClock/TimeProvider seams, DeadlineClass taxonomy, schedule port with CronExpression record, fake pairs on the test row.
- configuration-and-options.md [FINAL]: ConfigSource axis with rank + reload-class columns, source-generated binding, validated frozen policy values, receipted reload.
- composition-and-modules.md [FINAL]: one composition root per process; ModuleContribution rows; scanning/decoration/keyed-service law.
- resource-lanes.md [FINAL]: HybridCache port (stampede, tags, entry options), ObjectPool policy rows, DrainQueue rows.
- diagnostics-and-telemetry.md [FINAL]: correlation spine, TelemetrySignal governance, DataClassification taxonomy, contributor consumption.
- health-and-degradation.md [FINAL]: HealthContributor fold, DegradationLevel rail incl. LocalOnly, grpc.health.v1 tag-predicate mapping, resource-pressure row.
- support-bundles.md [FINAL]: SupportTrigger union, window freeze, contributor fan-in, redaction, cap, manifest, receipt.
- outbound-resilience.md [FINAL]: OutboundHop axis (7 cases incl. update-check), one keyed Polly pipeline per hop, single retry-owner conflict evidence and receipts; the DISCOVERY_ATTACH cluster owns DiscoveryManifest attach law, atomic publish, staleness probe, checksum gate, UDS connect, single-shot companion spawn, peer-credential probe.
- runtime-ports.md [FINAL]: typed port records (TelemetryContributor, ReceiptSink with HLC envelope, DrainParticipant, HostAttach, UiScheduler, Support, Health), suite wire law, TS tooling map, contract-merge rule.
- provisioning-and-update.md [FINAL]: post-fetch UpdateManager state machine (download/stage/rollover/rollback) with per-phase UpdateReceipt and source-gen instruments, three-row UpdateChannel feed axis with downgrade policy, drain-before-swap rollover folding DrainConductor ahead of ApplyUpdatesAndRestart.
- companion-sidecar.md [FINAL]: ProcessModality axis (companion/sidecar/paired-peer spawn-attach-discovery-degradation), ControlInbound folding three ControlService verbs onto degradation/options/support owners, ServiceHost gRPC server over UDS-only ControlTransport, parent-to-child DegradationCascade write, accept-side PeerAdmission credential read.
- capability-registry.md [FINAL]: one self-describing CapabilityDescriptor catalog over an effect/idempotency/cost/permission axis, a shape-discriminated DiscoveryQuery fold, a commit-or-rollback CommandAlgebra over the Compute dispatch rail, a scoped GrantBroker with consent/dry-run cost metering, and polyglot SdkCodegen; mints no eighth port.
- determinism-and-replay.md [FINAL]: a DeterminismKernel pinning RNG/float-mode/env fingerprint, a hash-chained content-addressed EventLog, a ReplayVerify per-step content-hash proof rail, a MacroEngine record/replay unit, and a content-address RecomputeGraph partial-recompute walk over the op-log.
- live-wire.md [FINAL]: an eight-row ExternalTransport industrial-binding axis under one read/write adapter contract, a BindingSpec with edge unit coercion through the Compute unit algebra, a WriteBack acknowledgement transaction, and a BindingHealth lifecycle projecting onto the health fold.
- mcp-projection.md [FINAL]: an McpMethod axis projecting the capability registry onto an MCP tool catalog, a dry-run CostPreview brokered McpDispatch, and a StreamProgress server-stream fan with resumable handles over the cancel spine and keyed backpressure.
- sandbox-host.md [FINAL]: a two-row SandboxIsolation axis (WASM-component/out-of-process) under no-ambient-authority load, a brokered GrantHandle, a QuotaControl kill/quarantine cell, and a SupplyChainGate signature/SLSA/semver admission.
- solver-plugin.md [FINAL]: a seven-row SolverKind extension-category axis, a SolverPluginContract representation/op/descriptor validation, and a SolverHosting sandboxed load projecting declared ops into the capability registry with representation negotiation.

[PERSISTENCE]:

- store-profiles.md [FINAL]: six StoreProfile rows with the widened row record (pooling, execution strategy, concurrency, seeding, maintenance, restore, cross-process lease) and placement/provisioning law.
- data-lanes.md [FINAL]: seven DataLane rows mapped to engine capabilities and extension rows incl. geometry/geo lanes and the analytical lane.
- schema-rail.md [FINAL]: IdentityPolicy axis, migration law, generated columns, HasPostgresExtension rows, naming, concurrency tokens, Thinktecture EF converters.
- query-rail.md [FINAL]: StoreOp total dispatch, pooled context factories, bulk lane with self-emitted changefeed, MERGE/RETURNING delta projection, interceptor receipts.
- native-sqlite.md [FINAL]: e_sqlite3 compile-flag surface, WAL/busy law, loadable-extension route, encryption and vector gates.
- snapshot-codecs.md [FINAL]: SnapshotCodec/CompressionPolicy/HashPolicy axes, restore choreography, wire contracts (STJ + MessagePack + GeoJSON projection).
- cache-indexes.md [FINAL]: L2 IDistributedCache + serializer factory rows, model-result and artifact-blob indexes.
- sync-collaboration.md [FINAL]: SyncTransport axis with M1-M5 rows, op-log HLC changefeed, HttpDelta + JSON-Patch fallback, presence, conflict receipts, BlobRemote frame consumption.
- redaction-retention.md [FINAL]: RetentionPolicy rows, classification enforcement consuming the AppHost taxonomy, clock-seam stamping, audit binding.
- remote-stores.md [FINAL]: cloud object-store residence on one ObjectStore provider axis behind the BlobRemote contract — S3/Azure/GCS rows, chunked resumable multipart at the ArtifactSync frame width, content-key residence with conditional-write concurrency, content-address cloud-sync-hub seam over the op-log; tier-2 battery proved the S3/Azure/GCS multipart roundtrip GREEN against MinIO/Azurite/fake-gcs ([OBJECT_ROUNDTRIP]/[OBJECT_MEMBER_SPELLINGS]/[REMOTE_FAULT_LIFT] closed), host-local wire posture (no TS_PROJECTION).
- server-tier.md [FINAL]: the self-provisioned PostgreSQL 18.4 server tier — TimescaleDB time-series DDL, pgvectorscale diskann + pg_search BM25 index DDL, cluster GUC deploy fragments, the TenancyModel multi-tenancy/RLS axis with TenantProvision lifecycle + TenantQuota bound, and the migration-bundle deploy gate; tier-2 battery proved the provisioning/RLS/index/bundle DDL GREEN against PG18.4 (timescaledb-ha + paradedb), host-local wire posture (no TS_PROJECTION). The live-PG runtime probes (pgoutput decode-fold, pgaudit session-category emission, two-process WAL first-open race) ride the EXISTING DENSITY_BAR SPIKE rows [4]/[19]/[20] on store-profiles/sync-collaboration/redaction-retention, never on this provisioning page.
- annotation.md [FINAL]: one Anchor union binding annotations to node/sub-entity/parameter/world-point targets with re-anchoring, a Thread comment/mention/status fold over the op-log, and the Bcf 2.1/3.0 + BCF-API CdeSync OAuth2 backbone over the AppHost outbound hop.
- catalog-cost.md [FINAL]: a multi-standard ClassificationStandard axis with ltree-pathed hierarchical codes, a federated ClassificationMap, and a CostRollup formula-evaluated quantity-takeoff hierarchical DuckDB rollup over the element set.
- federation.md [FINAL]: a source-agnostic FederatedEntity graph keyed by stable composite identity, the polymorphic ElementSet selection currency, typed CrossDocLink pin/float impact, a RulePlan declarative-DSL query lowering, a FusionRank HNSW+GiST+FTS fusion, and a cost-based FederatedPlan cross-store pushdown.
- provenance.md [FINAL]: a W3C-PROV ProvEdge causal DAG over the op-log with backward/forward LineageSlice fold, a hash-chained AttestedLedger with head proof, and a redaction-preserving lineage-scoped LineageCdc feed.
- schedule-interchange.md [FINAL]: a ScheduleImport P6-XER/MS-Project-XML activity-network reader over the Sep tabular lane, and a TaskElementLink/FourDState as-of 4D construction-state fold binding schedule activities to the federated element set.
- version-control.md [FINAL]: a content-addressed CommitGraph DAG with branches/merge-base/version-vectors/Merkle-range, a CrdtField RGA/OR-set/LWW-register convergent CRDT superseding the LWW Adjudicate, a TimeTravel AS-OF reconstruction/diff/blame/scrub engine, and a StructuralMerge tree-edit-distance three-way geometry merge.

[COMPUTE]:

- intent-and-selection.md [FINAL]: typed intent family, substrate-selection rail with ordered predicates and declared fallback rows, one total dispatch surface; wasm is a platform predicate.
- tensor-lane.md [FINAL]: CpuTensor substrate, TensorPrimitives/layout families, shape algebra, geometry-to-tensor encoding rows.
- model-lane.md [FINAL]: model identity and provenance, session capsule, EP-parameterized Onnx row, extension-op admission, warm-start artifact routing.
- remote-lane.md [FINAL]: the suite proto vocabulary (ComputeService, DocumentService, ControlService, ArtifactSync frame law, grpc.health.v1, FaultDetail), transport rows with streaming-capability and keepalive and node-affinity columns, CredentialPolicy axis, discovery manifest consumption, descriptor-diff contract evolution.
- staging-and-streams.md [FINAL]: AllocationClass rows with rent/return delegates, recyclable stream policy, zero-copy edges.
- scheduling-and-lanes.md [FINAL]: WorkLane bounded channels, solve-path guard, backpressure, drain participation.
- progress-and-observation.md [FINAL]: monotonic ProgressPhase rows, zero-alloc capsules, scheduler-policy-carrying subscriptions, remote projection.
- units-boundary.md [FINAL]: QuantityFamily rows, conversion-at-admission, parse/format edges, unit evidence.
- receipts-and-benchmarks.md [FINAL]: 13-case receipt union, fold projections, NodaTime-protobuf stamps, fingerprint-gated benchmark claims, artifact blob routing.
- numeric-lane.md [FINAL]: RID-keyed LinearProvider BLAS availability table (managed terminal / native-mkl x64-only / native-openblas) over MathNet Control.TryUse* probes, five-case Factorization union (LU/QR/Cholesky/SVD/EVD), SparseFormat csr/csc/coo/dok ingestion over CSR storage + CSparse direct solve, KernelLowering binding lowering tensor matrix/structural rows onto GEMM, ShardPlan row-block fan-out; managed terminal proven on osx-arm64, native-BLAS execution is a per-RID deploy-asset gate (Compute TASKLOG residual), never a page RESEARCH probe.
- interchange.md [FINAL]: one InterchangeFormat/InterchangeCodec axis discriminating import/export across managed glTF/GLB (SharpGLTF + Draco/Meshopt + 3D-Tiles), STL/3MF/OBJ/PLY, E57/LAS/LAZ/PTS scans, CGNS/EnSight/VTK/Zarr fields, in-proc IFC/IFC5 (GeometryGym), the AP242/native companion two-hop TessellationRequest, FrameNormalization onto the DDG frame, the FieldCodec/DeltaCodec codecs, and the XxHash128 InterchangeIdentity; host-local, no TS_PROJECTION.
- solver-and-optimization.md [FINAL]: a PhysicsKind×BoundaryCondition×ElementClass SolveProblem axis over a volumetric MeshKernel, a numeric-lane-backed SolveLane assemble-solve fold, an Optimizer design-space search with Surrogate ROM duality and ParetoFront, a SweepGrid/FrameBudget DOE governor, and a ClashScale/DigitalTwin acceleration-structure twin loop; host-local behind the existing Solve rpc, no TS_PROJECTION.

[APPUI]:

- surface-hosts.md [FINAL]: SurfaceHost axis with marshal rows, mount transaction, embedding gates; web-browser designed-only.
- shell-navigation.md [FINAL]: shell composition, routing, dock layouts with checkpoint and crash-restore columns.
- screens-activation.md [FINAL]: screen family, activation scopes, lifecycle binding.
- commands-availability.md [FINAL]: one CommandIntent table; derived menus/toolbars/palette/deep-links/remote; fuzzy-search index; hotkeys.
- live-data.md [FINAL]: DataSource axis, change-set engine depth, the live-data spine composite, IdentityPolicy-keyed caches.
- tables-hierarchy.md [FINAL]: TableProjection rows, virtualization, tree-flatten fold, grid-edit commit path.
- inspector-editing.md [FINAL]: EditorFactory rows, validation-to-save gate, options inspector, value-object and quantity editors, conflict-resolution row.
- charts-dashboards.md [FINAL]: ChartSeriesSpec rows incl. geo/map, dashboard composition, benchmark and timeline dashboards.
- visuals-offscreen.md [FINAL]: offscreen rendering, render-hash proof, DocumentExport cluster.
- theme-tokens.md [FINAL]: orthogonal ThemeVariant and Density families, token resolution, host-matched probing, switch atomicity.
- typography-shaping.md [FINAL]: TypographyRole rows, shaping pipeline, MarkdownProjection fold.
- icons-assets.md [FINAL]: IconSource rows with fallback ranks, asset loading, geo asset routing.
- dialogs-notifications.md [FINAL]: DialogIntent rows, session algebra, notification policy incl. drain suppression.
- input-interaction.md [FINAL]: pointer/keyboard/gesture rows, pan-zoom canvases.
- motion-tokens.md [FINAL]: MotionToken rows with reduced pairs, phase mapping, perceptual color interpolation.
- accessibility.md [FINAL]: automation peers, contrast governance, reduced-motion law.
- localization-culture.md [FINAL]: LocaleRow on inbox resx/ICU/NodaTime, culture composition.
- diagnostics-evidence.md [FINAL]: EvidenceReceipt union, correlation join fold, headless proof derivation, provenance projection.
- custom-visuals.md [FINAL]: four Skia-drawn CustomVisual kinds LiveCharts cannot supply (sankey/treemap/waterfall/funnel) as pure layout folds with per-cell render-hash twins, plus the single suite-wide ColorSpaceAxis wide-gamut vocabulary the codec working-space consumes.
- animation-timeline.md [FINAL]: a closed Track keyframe-track union with motion-token Easing, a deterministic-playhead Timeline sampler, a Scrub kinematic/transient-field fold over the Compute SimField frame index, and an offline Walkthrough render to the encode rail.
- drafting-sheets.md [FINAL]: a locale-aware SheetSet with ISO/ANSI/JIS title blocks, a hidden-line Viewport2D projection kernel over the Compute geometry payload, the Dimension/Annotation GD&T families, and a DraftEmit DWG/DXF/PDF/SVG dispatch over the SKDocument rail.
- notebook-document.md [FINAL]: a closed NotebookCell union with a pinned CapabilityPin fingerprint, a DependencyGraph dirty-recompute fold over the affected closure, a NotebookCrdt op-log co-edit merge, and a ReplayBundle bit-identical export.
- viewport-pipeline.md [FINAL]: a RenderGraph pass-DAG over a host-shared GRContext lease, MeshletCluster geometry virtualization with ResidencyBudget out-of-core streaming, a PathTracePass ReSTIR/denoise GI pass, SimVisual field renders off the Compute receipts, and a BCF-compatible Viewpoint codec; GPU passes SPIKE-gated on the live shared context, 2D-Skia fallback ships today.
