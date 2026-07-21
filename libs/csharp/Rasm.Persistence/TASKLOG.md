# [PERSISTENCE_TASKLOG]

Open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[MODEL_QUALIFIED_SETS]-[QUEUED]: Model-qualified element sets — re-cut the `ElementSet` preimage over `(ModelId, NodeId)` and re-freeze its parity vector in the same pass.
- Capability: set membership carries the owning model, evaluation resolves across the `ProjectGraph` roster, and the multi-graph topology view answers federation-altitude selections.
- Shape: preimage re-frame on `libs/csharp/Rasm.Persistence/.planning/Query/lane.md#ELEMENT_SET_ALGEBRA`, the `ContentParityCorpus` `ParitySlot.ElementSet` vector re-cut beside it, and the multi-graph view case on `libs/csharp/Rasm.Persistence/.planning/Query/topology.md#GRAPH_TOPOLOGY` over the durable `ModelLink` edges.
- Unlocks: cross-model clash sets and whole-project QTO subjects as one content-addressed currency.
- Anchors: `IDEAS.md` `[PERS_E2]`; `ModelLink`/`ProjectGraph` on `Element/graph`, the length-framed preimage discipline.
- Tension: the frozen parity vector binds the `NodeId`-only preimage — both cut in one pass or cross-runtime keys diverge.

[POINTCLOUD_CODEC_SURVEY]-[BLOCKED]: E57/LAS/LAZ codec admission survey — resolve the one question blocking the reality-capture page.
- Capability: a ruled managed codec admission (or a ruled non-existence verdict) arms `[PERS_I3]` and pins the `libs/csharp/Rasm.Persistence/.planning/Ingest/pointcloud.md` package roster.
- Shape: nuget MCP survey over the managed E57/LAS/LAZ candidate family scoring license, maintenance signal, and net10 asset; verdict lands as the `[PERS_I3]` arming edit and its packageNeeds row.
- Unlocks: the blocked reality-capture codec becomes buildable.
- Anchors: `IDEAS.md` `[PERS_I3]`; the admission-gate law (supersession-only rejection).
- Arms: the nuget survey verdict — a ruled managed E57/LAS/LAZ codec admission or a ruled non-existence verdict.

[SOLVER_MEMO_BAND]-[QUEUED]: Solver-memo band — content-keyed NFP pair and ICP fit memos persist beside the benchmark index and replay across runs.
- Capability: a durable memo band keyed by the Fabrication content keys — NFP pair geometry, ICP fit results — with hit accounting, so expensive solver truth computes once and replays across processes.
- Shape: one memo band on `libs/csharp/Rasm.Persistence/.planning/Query/cache.md` beside `#BENCHMARK_INDEX`, reads on the synchronous lane, publication through the standing residence law.
- Unlocks: nesting and registration solves warm-start from durable memos instead of recomputing per run.
- Anchors: the `#BENCHMARK_INDEX` content-address and recency precedent, the Fabrication memo-key origin.
- Ripple: `Rasm.Fabrication` `[SOLVER_MEMO_CACHE]`.

[SEARCH_WIRE_PROJECTION]-[QUEUED]: Search wire projection — the retrieval lane exposes one typed query/result wire with corpus-coverage rows for the document-search plane.
- Capability: the landed BM25/tsquery retrieval owner projects a typed query/result wire — query union in, ranked hits with branch lineage out — and coverage rows admit the notebook-cell, issue-text, and evidence-payload corpora onto the indexed set.
- Shape: wire members on `libs/csharp/Rasm.Persistence/.planning/Query/retrieval.md` beside the fusion fold; one coverage row per corpus naming its indexed columns.
- Unlocks: the AppUi `Document/search.md` plane queries every durable text corpus through one wire.
- Anchors: the retrieval predicate family and `LexicalRank` arms, the fusion lineage receipt, the `key_field` anchor law.
- Ripple: `Rasm.AppUi` `[DOCUMENT_SEARCH]`.

[TRACE_STAMP_PORT]-[QUEUED]: Egress trace stamping becomes port-injected — the envelope arrives propagator-stamped and no transport leg formats wire headers.
- Capability: trace context reaches the broker egress through a composition-injected stamp, so spec revisions and tracestate flow through with zero Persistence edits and the ledger's never-re-mint law holds by construction.
- Shape: `libs/csharp/Rasm.Persistence/.planning/Version/egress.md` — `EgressPorts` gains the trace-stamp delegate pair and `Egress.Envelope` deletes its hand-formatted traceparent interpolation.
- Unlocks: the one live violation of the propagator custody law dies at its root.
- Anchors: `Version/ledger.md` propagator custody clause; `libs/csharp/.planning/RULINGS.md` causal-frame row.
- Ripple: follows `Rasm.AppHost` `[EGRESS_CARRIER_SETTERS]`.
- Atomic: one port pair, one literal deletion.

[CDC_ENVELOPE_SPELLING]-[QUEUED]: Egress envelope vocabulary aligns to the one realized owner spelling.
- Capability: every catalog and comment names the realized CloudEvents projection owner, so the seam vocabulary carries one spelling and a phantom type never anchors a consumer.
- Shape: `libs/csharp/Rasm.Persistence/.api/api-cloudevents.md` (`CdcEnvelope` charter and boundary rows, `CdcEnvelopeWire` at the three-consumer row), the `libs/csharp/Rasm.Persistence/.api/api-nats.md` snapshot-codec row, and the `libs/csharp/Rasm.Persistence/.planning/Query/columnar.md` projection comment — each re-spells to the `Version/egress.md` `Egress.Envelope` projection.
- Unlocks: the three-consumer drain law reads against a spelling the owning page carries.
- Anchors: `Version/egress.md` `Egress.Envelope` fence; the decoded-never-re-minted boundary law both ends carry.
- Ripple: mirrors `Rasm.AppHost` `[OUTBOX_ENVELOPE_SPELLING]`.
- Atomic: spelling alignment, no shape change.

[RECEIPT_PORT_KERNEL_TYPES]-[QUEUED]: Receipt-seam spellings re-anchor to the kernel causal-frame owners.
- Capability: the receipt seam names kernel-owned identity, tenancy, and envelope types; a receipt consumer reads one type family with no strata caveat.
- Shape: `libs/csharp/Rasm.Persistence/.planning/Element/codec.md` and `libs/csharp/Rasm.Persistence/.planning/Version/timetravel.md` — `ReceiptEnvelope`/`CorrelationId`/`TenantContext` mentions re-anchor to the kernel capsule spellings.
- Unlocks: the strata-inversion caveats those pages carry dissolve.
- Anchors: `libs/csharp/.planning/RULINGS.md` causal-frame row; kernel `Domain/telemetry.md`.
- Ripple: follows `Rasm` `[CAPSULE_EXTENSION_MINTS]`.
- Atomic: spelling re-anchors, no shape change.

[PARITY_SLOT_PROSE_ALIGN]-[QUEUED]: Parity-corpus prose derives slot membership from the fence instead of legislating a count.
- Capability: the corpus prose states the contribute rule; the fence roster owns membership, so a new slot lands without falsifying a sentence.
- Shape: `libs/csharp/Rasm.Persistence/.planning/Version/commits.md` — the "four parity slots" sentence restates over the fence-owned `ParitySlot` roster covering `CrdtOpSet`.
- Unlocks: slot growth with zero prose debt.
- Anchors: the `ParitySlot` fence roster; `RULINGS.md` parity re-freeze row.
- Atomic: one sentence.

[FLIGHT_SQL_SUBCLASS]-[BLOCKED]: Flight SQL subclass completes over the shared `ReplayKey` hold.
- Capability: a transcription-complete `FlightSqlServer` subclass joins the landed plain-Flight serving, every handler typed over the shared hold.
- Shape: a restored `libs/csharp/Rasm.Persistence/.planning/Query/federation.md` `#RESEARCH` fence.
- Unlocks: `[PERS_L1]` — Flight SQL serving settles over the columnar plane.
- Anchors: `.api/api-arrow.md` `FlightSqlServer` roster; `Query/federation.md` `ReplayKey` hold.
- Arms: `.api/api-arrow.md` gains every exact protected abstract `FlightSqlServer` handler row.

[MQTT_SINK_MEMBERS]-[BLOCKED]: MQTT sink members compose into the egress family.
- Capability: verified message, publish, header, and QoS members compose into `EgressSink.Mqtt` beside the landed AMQP sink.
- Shape: a restored `libs/csharp/Rasm.Persistence/.planning/Version/egress.md` `#RESEARCH` fence.
- Unlocks: `[PERS_V5]` — MQTT egress settles on the one sink rail.
- Anchors: `MQTTnet` and `CloudNative.CloudEvents.Mqtt` packages; `Version/egress.md` sink family.
- Arms: both packages gain exact folder-tier `.api` catalogs.

[ARROW_PARTITIONS_RESTORE]-[BLOCKED]: `ArrowPartitions` restores over the descriptor-enumeration spelling.
- Capability: partitioned ADBC execution and redemption serve the columnar plane typed, `ArrowPartitions` composed over the exact `PartitionedResult` descriptor enumeration.
- Shape: a restored `libs/csharp/Rasm.Persistence/.planning/Query/columnar.md` `#RESEARCH` fence.
- Unlocks: partition-parallel consumers redeem descriptors against the durable plane with no raw ADBC surface.
- Anchors: `.api/api-arrow.md` ADBC rows; `Query/columnar.md` columnar owner.
- Arms: `.api/api-arrow.md` gains the exact `PartitionedResult` descriptor-enumeration row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[OBSERVABILITY_PAGE_LAND]-[COMPLETE]: `Store/observability.md` landed — slot grammar and registry, `pg_stat_statements`/`pg_stat_io` harvest, DuckDB profiling harvest, SQLite status harvest; `OpenTelemetry.Instrumentation.ConfluentKafka` admitted with csproj row, README registry row, and `.api` catalog.
[SLOT_ROSTER_SPREAD]-[COMPLETE]: every emitting page carries its `Slots` roster on its primary owner and `SlotRegistry.Mounted()` spreads the census; the topology traversal slot collapsed to `store.topology.traverse` and the vector-route fact respelled `store.vector.route` under the grammar.
[EGRESS_TRACE_ENVELOPE]-[COMPLETE]: egress context carriers landed — `Egress.Envelope` stamps the `traceparent`/`tracestate` composite on every sink and the `Nats` case row mirrors them onto `NatsHeaders` through the AppHost `TraceContext` carrier adapter.
[DATASET_SCAN_LAND]-[COMPLETE]: partitioned dataset scan landed as `FlatTableEgress.ScanDataset` (`DatasetReader` + `HivePartitioning.Factory` + `ToBatches` pushdown) with the `store.columnar.scan` slot on `Query/columnar.md#FLAT_TABLE_EGRESS`.
[CDC_INGRESS_OWNER]-[COMPLETE]: inbound CDC ingress owner minted at `Version/ingress.md` (`CdcIngress` — instrumented consume, envelope decode, source gate, content-key dedup, store-first offsets), registered in the README router, ARCHITECTURE codemap, `SlotRegistry.Mounted`, and the `FaultBand.Ingress` 8500 row.
[HOOK_RAIL_ROSTER]-[COMPLETE]: hook-point roster landed as `Store/observability.md#HOOK_RAIL` `PersistenceHooks` — six typed lifecycle points with `Guarded`/`Swept` composition adapters and per-composition mounts.
[USAGE_FOLD_LAND]-[COMPLETE]: usage fold landed as `Store/observability.md#USAGE_PROJECTION` `StoreUsage.Fold` over the `BLOB_GC` catalog and drain receipts, with the `rasm.persistence.usage.*` gauge rows and arm.
[PLAN_HARVEST_LAND]-[COMPLETE]: plan harvest landed as `Store/observability.md#PLAN_PROFILE` — pg/DuckDB/SQLite capture legs, shape-only digests, `PlanBaselineRow` identity-tier persistence, `PlanVerdict` under `store.stat.plan`.
[CIPHER_FLOOR_LAND]-[COMPLETE]: cipher floor landed on `Store/provisioning.md#EMBEDDED_FLOOR` — `bundle_e_sqlite3mc` provider, `raw.sqlite3_key` first-crossing key application in `Open`, `Rekey` rotation, DEK custody through the landed `Element/identity#KMS_CUSTODY` envelope algebra.
[INSTRUMENTATION_ROWS_LAND]-[COMPLETE]: instrumentation subscription rows landed — the Redis/EF/AWS settled-composition rows on the `Store/observability.md` lead and the README registry rows under their owning label groups.
[CENSUS_PROJECTION_LAND]-[COMPLETE]: census projection landed as `StoreInstruments.Census(version, registry)` folding rows, bucket thresholds, mounted slots, and projected-arm keys into `StoreTelemetryCensus`.
[BENCH_FAMILY_ROWS]-[COMPLETE]: corpus family rows landed as `BenchmarkFamily` on `Query/cache.md#BENCHMARK_INDEX` — codec/store-append/merge/columnar/vector-route/multipart suites with subject owners and suite-owned claim keys.
[PROVISION_MANIFEST_LAND]-[COMPLETE]: manifest projection landed as `ClusterProvision.Manifest` folding roster, settings, jobs, and the embedded ritual into `ProvisionManifest` with `#STORE_AXIS_MAP` axis coordinates.
[SCHEMA_PINNED_MINT]-[COMPLETE]: schema-pinned contributor mint landed as `StoreInstruments.Telemetry(version, schemaUrl)` filling the port `SchemaUrl` slot the AppHost mint stamps as `MeterOptions.TelemetrySchemaUrl`.
[LANDING_ARM_ROWS]-[COMPLETE]: landing arm rows landed as `LandingArm` (geometry/doe/tabulate/materials) + `FlatTableEgress.Land` with the four `store.<domain>.land` slots in the mounted census.
[SHOP_STATE_SLOTS_SPAN]-[COMPLETE]: shop-state slot rows land through the `SlotRegistry.Mounted` contributed span — the `store.fabrication.<domain>.<verb>` family is call-site data under the uniqueness law; the Fabrication-side receipt pairs ride `Rasm.Fabrication` `[SHOP_STATE_SLOTS]`.
[DELTA_ENVELOPE_SUBJECT]-[COMPLETE]: delta envelope composition landed — `Egress.Envelope` stamps `Subject` with the entity identity beside `Type`/`Time`/`traceparent`, honoring the Element seam vocabulary on `GraphDelta` crossings.
[IPC_DECODE_ARM]-[COMPLETE]: compressed-carrier decode arm landed as `FlatTableEgress.ReadIpcFrames` over the one `CompressionCodecFactory` (`ArrowStreamReader(Stream, ICompressionCodecFactory)` assay-verified); identity reads decompressed canonical bytes.
