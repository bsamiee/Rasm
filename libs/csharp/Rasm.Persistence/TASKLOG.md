# [PERSISTENCE_TASKLOG]

Open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[MODEL_QUALIFIED_SETS]-[QUEUED]: Model-qualified element sets — re-cut the `ElementSet` preimage over `(ModelId, NodeId)` and re-freeze its parity vector in the same pass.
- Capability: set membership carries the owning model, evaluation resolves across the `ProjectGraph` roster, and the multi-graph topology view answers federation-altitude selections.
- Shape: preimage re-frame on `Query/lane#ELEMENT_SET_ALGEBRA`, the `ContentParityCorpus` `ParitySlot.ElementSet` vector re-cut beside it, and the multi-graph view case on `Query/topology#GRAPH_TOPOLOGY` over the durable `ModelLink` edges.
- Unlocks: cross-model clash sets and whole-project QTO subjects as one content-addressed currency.
- Anchors: `IDEAS.md` `[FEDERATED_ELEMENT_SET]`; `ModelLink`/`ProjectGraph` on `Element/graph`, the length-framed preimage discipline.
- Tension: the frozen parity vector binds the `NodeId`-only preimage — both cut in one pass or cross-runtime keys diverge.

[POINTCLOUD_CODEC_ADMIT_ROSTER]-[QUEUED]: Admit the surveyed reality-capture codec pair onto the central manifest and its catalogue tier.
- Capability: the ruled E57 and LAS/LAZ managed codecs become composable surface — one manifest row each, one folder registry row each, one `.api` catalogue each at member depth — so the reality-capture page composes verified members rather than a package nobody admitted.
- Shape: two `Directory.Packages.props` rows, two `libs/csharp/Rasm.Persistence/README.md` `[SCALEOUT_BACKENDS]`-peer registry rows under a reality-capture label, and two catalogues at `libs/csharp/Rasm.Persistence/.api/`.
- Unlocks: `IDEAS.md` `[POINTCLOUD_CODEC_ADMISSION]` — the codec page's `Packages:` line resolves to admitted rows.
- Anchors: the survey verdict on `[POINTCLOUD_CODEC_SURVEY]`; the catalog-alignment law binding manifest, registry, and catalogue as one touch-point set.
- Atomic: two package rows and their catalogues, no page authoring.

[SOLVER_MEMO_BAND]-[QUEUED]: Solver-memo band — content-keyed NFP pair and ICP fit memos persist beside the benchmark index and replay across runs.
- Capability: a durable memo band keyed by the Fabrication content keys — NFP pair geometry, ICP fit results — with hit accounting, so expensive solver truth computes once and replays across processes.
- Shape: one memo band on `libs/csharp/Rasm.Persistence/.planning/Query/cache.md` beside `#BENCHMARK_INDEX`, reads on the synchronous lane, publication through the standing residence law.
- Unlocks: nesting and registration solves warm-start from durable memos instead of recomputing per run.
- Anchors: the `#BENCHMARK_INDEX` content-address and recency precedent, the Fabrication memo-key origin.
- Ripple: `Rasm.Fabrication` `[SOLVER_MEMO_CACHE]`.

[ROLLING_WINDOW_PARTITIONS]-[QUEUED]: Rolling range partitions retire aged time-series rows by dropping a partition rather than sweeping them.
- Capability: a date-keyed document family declares its own retention window on its mapping, so the trailing edge retires as a partition drop the database performs in constant time and the leading edge provisions ahead of the clock — a whole retention class stops paying per-row sweep cost.
- Shape: a `ByRollingRange` declaration on the observability usage-series and evidence-bundle mappings at `Store/provisioning`, plus the maintenance verbs seated on the existing startup migration leg beside `ApplyAllDatabaseChangesOnStartup`.
- Unlocks: `Version/retention#SWEEP_AND_GC` stops enumerating aged rows for the declared-expiry classes, keeping the receipted sweep for the content-keyed classes that genuinely need reachability.
- Anchors: `.api/api-marten.md` rolling-partition rows — the declaration asserts its duplicated date key at configuration time, one shared manager rolls several tables in one pass, a `DEFAULT` overflow partition means an out-of-window row stores rather than failing, and the three `store.Advanced` verbs split roll-forward from drop-aged so the destructive half is opt-in.
- Tension: the trailing drop is a real deletion the retention receipt rail must still account for, so the conservation identity has to admit a partition-drop verdict rather than counting per-row evictions.

[SEARCH_WIRE_PROJECTION]-[QUEUED]: Search wire projection — the retrieval lane exposes one typed query/result wire with corpus-coverage rows for the document-search plane.
- Capability: the landed BM25/tsquery retrieval owner projects a typed query/result wire — query union in, ranked hits with branch lineage out — and coverage rows admit the notebook-cell, issue-text, and evidence-payload corpora onto the indexed set.
- Shape: wire members on `libs/csharp/Rasm.Persistence/.planning/Query/retrieval.md` beside the fusion fold; one coverage row per corpus naming its indexed columns.
- Unlocks: the AppUi `Document/search.md` plane queries every durable text corpus through one wire.
- Anchors: the retrieval predicate family and `LexicalRank` arms, the fusion lineage receipt, the `key_field` anchor law.
- Ripple: `Rasm.AppUi` `[DOCUMENT_SEARCH]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CDC_ENVELOPE_SPELLING]-[COMPLETE]: `CdcEnvelope` returns zero hits across every design page and catalog — the `api-cloudevents.md` charter and field-map rows and the `Query/columnar.md` projection comment re-spelled to the realized `Egress.Envelope` projection (the `api-nats.md` site the card named carried no occurrence on re-probe), landed in the same pass as the AppHost `[OUTBOX_ENVELOPE_SPELLING]` mirror.
[POINTCLOUD_CODEC_SURVEY]-[COMPLETE]: the survey ran and the family EXISTS — `Aardvark.Data.E57` carries the E57 reader and `Unofficial.laszip.netstandard` the LAS/LAZ codec, both resolving on nuget.org at the same current platform version, both already transitively resident in the branch's restore graph, and neither superseded; the discarded candidates (`laszip.net`, `LASzip.Net`, `Aardvark.Data.Points.Import.Laszip`) resolve on no configured source, so the roster is the two admitted rows and the hand-rolled E57 XML-plus-binary layout the card named as the forbidden alternative is foreclosed by an existing managed reader rather than by preference.
[MQTT_SINK_MEMBERS]-[COMPLETE]: refuted on disk — `Version/egress#EGRESS_SINK` already carries the realized `Mqtt(SinkBinding, string Topic)` case, its `Binding` arm, the structured-mode `ToMqttApplicationMessage` encode, the `V500` `UserProperties` trace stamp through the `ValueBuffer` overload, and the `MqttClientPublishResult` reason-code fold, with both `libs/csharp/.api/api-mqtt.md` and `libs/csharp/.api/api-cloudevents-mqtt.md` landed; the arming condition had long since resolved.
[ARROW_PARTITIONS_RESTORE]-[COMPLETE]: `PartitionedResult.PartitionDescriptors` is `IReadOnlyList<PartitionDescriptor>` beside `Schema`/`AffectedRows`, so `Query/columnar#COLUMNAR_LANE` lands `ColumnarLane.ArrowPartitions` with the `ArrowPartitions` redemption record over `AdbcConnection.ReadPartition`; the catalog gained the descriptor rows and the trap that `ExecutePartitioned`/`ReadPartition` are `virtual` bodies throwing `AdbcException.NotImplemented`, so member presence proves no driver support.
[COLUMN_SHAPE_ALGEBRA]-[COMPLETE]: `Query/columnar#ANALYTICS_RESIDENCE` widened `ColumnType` with the unsigned and 32-bit rows and generated `List`/`Map`/`Dictionary` over it as `ColumnShape`, each residence answering four composer columns, so the OTLP wide-event column shapes provision through the one custodian and the roster stands declared branch-local.
[RESIDENCE_FAULT_NEUTRAL]-[COMPLETE]: `ResidenceFault.FleetRefused` deleted — every refusal now names its residence beside one `EngineFault` pair each row renders through its own `Diagnose` column, which also removed the unchecked `(ClickHouseServerException)` cast that threw out of the `Fin` fold on any non-server failure.
[RESIDENCE_HEALTH_FAMILY]-[COMPLETE]: policy health lifted to the family as `ResidenceRead.Health` over the resident time extent against the declared horizon, riding the four existing reach arms; the Timescale `job_stats` probe survives as `SeriesLane.Jobs`, honestly scoped rather than standing in for a family surface.
[TRACE_STAMP_PORT]-[COMPLETE]: `Version/egress.md` `EgressPorts` gained the `Stamp` arrow rendering a continued `ActivityContext` onto the W3C pair at the propagator, and `Egress.Envelope(row, redact, stamp)` sets the two attribute slots without formatting either value — the `$"00-…-01"` interpolation that froze the version and sampled bytes is gone.
[RECEIPT_PORT_KERNEL_TYPES]-[COMPLETE]: `Version/timetravel.md` re-anchors `TimeLog`'s port and its receipt emission to the kernel `ReceiptSinkPort` and names the frame's kernel causal pair (`CorrelationId`/`TenantContext`), retiring the "correlation Guid" and strata-inversion caveats; `Element/codec.md` already composed the kernel spellings.
[PARITY_SLOT_PROSE_ALIGN]-[COMPLETE]: `Version/commits.md` derives parity membership from the `ParitySlot` roster's own `MintedHere` stance at both the Cases and Entry bullets, so the count-and-enumeration sentences that `CrdtOpSet` falsified are gone and a new leg lands with zero prose debt.
[FLIGHT_SQL_SUBCLASS]-[DROPPED]: refuted on disk — `Query/federation#FLIGHT_RESULT_PLANE` rules `FlightSqlServer` the DECLINED base (27 handler-less abstracts, dispatch matching `CommandStatementSubstraitPlan` nowhere) and lands `FederationFlight : FlightServer` over the shared `ReplayKey` hold, with `.api/api-arrow-egress.md` `[FLIGHTSQLSERVER_SUBCLASS_COST]`/`[SUBSTRAIT_COMMAND_UNROUTED]` carrying the evidence.
[OBSERVABILITY_PAGE_LAND]-[COMPLETE]: `Store/observability.md` landed — slot grammar and registry, `pg_stat_statements`/`pg_stat_io` harvest, DuckDB profiling harvest, SQLite status harvest; `OpenTelemetry.Instrumentation.ConfluentKafka` admitted with csproj row, README registry row, and `.api` catalog.
[SLOT_ROSTER_SPREAD]-[COMPLETE]: every emitting page carries its `Slots` roster on its primary owner and `SlotRegistry.Mounted()` spreads the census; the topology traversal slot collapsed to `store.topology.traverse` and the vector-route fact respelled `store.vector.route` under the grammar.
[EGRESS_TRACE_ENVELOPE]-[COMPLETE]: egress context carriers landed — `Egress.Envelope` stamps the `traceparent`/`tracestate` composite on every sink and the `Nats` case row mirrors them onto `NatsHeaders` through the AppHost `TraceContext` carrier adapter.
[DATASET_SCAN_LAND]-[COMPLETE]: partitioned dataset scan landed as `FlatTableEgress.ScanDataset` (`DatasetReader` + `HivePartitioning.Factory` + `ToBatches` pushdown) with the `store.columnar.scan` slot on `Query/columnar#FLAT_TABLE_EGRESS`.
[CDC_INGRESS_OWNER]-[COMPLETE]: inbound CDC ingress owner minted at `Version/ingress.md` (`CdcIngress` — instrumented consume, envelope decode, source gate, content-key dedup, store-first offsets), registered in the README router, ARCHITECTURE codemap, `SlotRegistry.Mounted`, and the `FaultBand.Ingress` 8500 row.
[HOOK_RAIL_ROSTER]-[COMPLETE]: hook-point roster landed as `Store/observability#HOOK_RAIL` `PersistenceHooks` — six typed lifecycle points with `Guarded`/`Swept` composition adapters and per-composition mounts.
[USAGE_FOLD_LAND]-[COMPLETE]: usage fold landed as `Store/observability#USAGE_PROJECTION` `StoreUsage.Fold` over the `BLOB_GC` catalog and drain receipts, with the `rasm.persistence.usage.*` gauge rows and arm.
[PLAN_HARVEST_LAND]-[COMPLETE]: plan harvest landed as `Store/observability#PLAN_PROFILE` — pg/DuckDB/SQLite capture legs, shape-only digests, `PlanBaselineRow` identity-tier persistence, `PlanVerdict` under `store.stat.plan`.
[CIPHER_FLOOR_LAND]-[COMPLETE]: cipher floor landed on `Store/provisioning#EMBEDDED_FLOOR` — `bundle_e_sqlite3mc` provider, `raw.sqlite3_key` first-crossing key application in `Open`, `Rekey` rotation, DEK custody through the landed `Element/identity#KMS_CUSTODY` envelope algebra.
[INSTRUMENTATION_ROWS_LAND]-[COMPLETE]: instrumentation subscription rows landed — the Redis/EF/AWS settled-composition rows on the `Store/observability.md` lead and the README registry rows under their owning label groups.
[CENSUS_PROJECTION_LAND]-[COMPLETE]: census projection landed as `StoreInstruments.Census(version, registry)` folding each row's kind, declared bounds, and tag vocabulary beside the mounted slots and projected-arm keys into `StoreTelemetryCensus`.
[BENCH_FAMILY_ROWS]-[COMPLETE]: corpus family rows landed as `BenchmarkFamily` on `Query/cache#BENCHMARK_INDEX` — codec/store-append/merge/columnar/vector-route/multipart suites with subject owners and suite-owned claim keys.
[PROVISION_MANIFEST_LAND]-[COMPLETE]: manifest projection landed as `ClusterProvision.Manifest` folding roster, settings, jobs, and the embedded ritual into `ProvisionManifest` with `#STORE_AXIS_MAP` axis coordinates.
[SCHEMA_PINNED_MINT]-[COMPLETE]: schema-pinned contributor mint landed as `StoreInstruments.Telemetry(version, schemaUrl)` filling the port `SchemaUrl` slot the AppHost mint stamps as `MeterOptions.TelemetrySchemaUrl`.
[LANDING_ARM_ROWS]-[COMPLETE]: landing arm rows landed as `LandingArm` (geometry/doe/tabulate/materials) + `FlatTableEgress.Land` with the four `store.<domain>.land` slots in the mounted census.
[SHOP_STATE_SLOTS_SPAN]-[COMPLETE]: shop-state slot rows land through the `SlotRegistry.Mounted` contributed span — the `store.fabrication.<domain>.<verb>` family is call-site data under the uniqueness law; the Fabrication-side receipt pairs ride `Rasm.Fabrication` `[SHOP_STATE_SLOTS]`.
[DELTA_ENVELOPE_SUBJECT]-[COMPLETE]: delta envelope composition landed — `Egress.Envelope` stamps `Subject` with the entity identity beside `Type`/`Time`/`traceparent`, honoring the Element seam vocabulary on `GraphDelta` crossings.
[IPC_DECODE_ARM]-[COMPLETE]: compressed-carrier decode arm landed as `FlatTableEgress.ReadIpcFrames` over the one `CompressionCodecFactory` (`ArrowStreamReader(Stream, ICompressionCodecFactory)` assay-verified); identity reads decompressed canonical bytes.
[TEXTURE_ARTIFACT_ROWS]-[COMPLETE]: `Query/cache#ARTIFACT_BLOB_INDEX` carries the texture-plane families as two `ArtifactKind` rows behind ONE selector — `Texture(Option<UInt128> planKey)` answers `TextureSet` (`Cache`, rebuildable from its recorded graph/plan/seed triple) or `TextureAcquired` (`Blob`, durable because a retired model card and a drifted execution provider make the bytes unreproducible), so retention derives from provenance with no origin flag beside the value that already discriminates; the selector returns the `SourceKey` beside the row because the plan key is both projections of one value, so a rebuildable row can never carry no origin and a press family never strands under per-set content keys.
[TEXTURE_LANDING_ARM]-[COMPLETE]: `Query/columnar#FLAT_TABLE_EGRESS` carries `LandingArm.MaterialsTexture` partitioned by `channel` beside the `catalogue`-partitioned materials arm, pinning the arm as the DATASET SHAPE rather than the producing package so a per-channel cold-tail sweep prunes whole directories and the catalogue tree keeps its own prunable segment; the segment value is the producer's CANONICAL channel name, never an ingest alias splitting one channel's tree into halves no board joins.
