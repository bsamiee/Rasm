# [PERSISTENCE_TASKLOG]

Open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0003]-[QUEUED]: Egress context carriers — NATS and CloudEvents legs continue the envelope trace the Kafka leg already carries.
- Capability: the `Nats` sink injects `traceparent` and baggage onto `NatsHeaders` beside `Nats-Msg-Id`; the webhook, Pulsar, and wire-native legs stamp the CloudEvents `traceparent`/`tracestate` extension attributes — every delivery joins the drain trace, never only the Kafka leg.
- Shape: carrier rows on `Version/egress.md#EGRESS_SINK` delegating to the AppHost `TraceContext` adapter family; the dedup-honesty column stays untouched.
- Unlocks: broker-hop trace continuity for the spine sink and CDC lanes; delivery evidence joins outbox drain spans across every sink case.
- Anchors: the `Version/egress.md` sink table, AppHost `telemetry.md#CORRELATION_SPINE` carrier growth row, `CloudNative.CloudEvents` extension attributes.
- Ripple: `Rasm.AppHost` `[WIRE_CARRIER_ADAPTERS]`.

[0004]-[QUEUED]: Flight SQL result plane — the federation producer speaks Flight SQL so ADBC-native consumers redeem without a bespoke descriptor contract.
- Capability: `FederationFlight` gains the `FlightSqlServer` arm — statement execution, prepared statements, and the catalog/schema metadata verbs over the same listener; the plain-Flight `GetFlightInfo`/`DoGet` ticket lane stands unchanged.
- Shape: one server arm on `Query/federation.md#FLIGHT_RESULT_PLANE` — `FlightSqlServer` beside the existing `FlightServer` subclass, Substrait plan bytes as the statement payload, results streaming through the held `ReplayKey` ticket registry; slots extend `store.federation.flight.*`.
- Unlocks: `python:data` `RemoteDriver.FLIGHTSQL` consumes the C# result plane directly through `adbc-driver-flightsql` — one cross-runtime analytics wire, no custom client.
- Anchors: the `api-arrow.md` Flight SQL surface (`FlightSqlClient` `ExecuteAsync`/`PrepareAsync` and the metadata verb family), the landed `FederationFlight` ticket registry, `FlowtideDotNet.Substrait` plan ingress.

[0005]-[QUEUED]: Partitioned dataset scan — `ParquetSharp.Dataset` reads multi-file hive-partitioned lake layouts as one Arrow stream.
- Capability: dataset scan with partition pruning and schema unification over directory datasets — the lake-scan counterpart to the single-file `ParquetSharp.Arrow` codec lane, batches feeding the same egress and query lanes.
- Shape: one scan row beside `Query/columnar.md#FLAT_TABLE_EGRESS` — `DatasetReader` over `HivePartitioning`/`IPartitioningFactory`, filter pushdown through `Col` and `FilterExtensions`, egress through `ToBatches()` yielding `IArrowArrayStream`; slot `store.columnar.scan` under the grammar.
- Unlocks: lake-resident history — Parquet daemon materializations and Delta-log generations queryable without a DuckDB mount in the loop.
- Anchors: the `api-parquetsharp.md` dataset surface, the async Parquet daemon materialization, the `store.columnar.*` slot roster.

[0006]-[QUEUED]: Model-qualified element sets — re-cut the `ElementSet` preimage over `(ModelId, NodeId)` and re-freeze its parity vector in the same pass.
- Capability: set membership carries the owning model, evaluation resolves across the `ProjectGraph` roster, and the multi-graph topology view answers federation-altitude selections.
- Shape: preimage re-frame on `libs/csharp/Rasm.Persistence/.planning/Query/lane.md#ELEMENT_SET_ALGEBRA`, the `ContentParityCorpus` `ParitySlot.ElementSet` vector re-cut beside it, and the multi-graph view case on `libs/csharp/Rasm.Persistence/.planning/Query/topology.md#GRAPH_TOPOLOGY` over the durable `ModelLink` edges.
- Unlocks: cross-model clash sets and whole-project QTO subjects as one content-addressed currency.
- Anchors: `IDEAS.md` `[PERS-E2]`; `ModelLink`/`ProjectGraph` on `Element/graph`, the length-framed preimage discipline.
- Tension: the frozen parity vector binds the `NodeId`-only preimage — both cut in one pass or cross-runtime keys diverge.

[0007]-[QUEUED]: Inbound CDC ingress owner — one new page drains foreign broker topics onto the durable rail.
- Capability: foreign Kafka events admit through the instrumented consumer, dedup by content key, continue W3C context off message headers, and fold onto the op-log as first-class ops.
- Shape: one new page at `libs/csharp/Rasm.Persistence/.planning/Version/ingress.md` — `InstrumentedConsumerBuilder` consume leg with `TryExtractPropagationContext` and `ConsumeAndProcessMessageAsync`, content-key dedup against the envelope `id`, consumer registration riding the AppHost root; the `EGRESS_SINK` family stays egress-only.
- Unlocks: a sibling deployment's egress stream replays into this store as a source.
- Anchors: `IDEAS.md` `[PERS-V4]`; the `api-otel-instrumentation-confluentkafka` consumer twins, `Version/egress#EGRESS_SINK` dedup-honesty column.

[0008]-[BLOCKED]: E57/LAS/LAZ codec admission survey — resolve the one question blocking the reality-capture page.
- Capability: a ruled managed codec admission (or a ruled non-existence verdict) arms `[PERS-I3]` and pins the `libs/csharp/Rasm.Persistence/.planning/Ingest/pointcloud.md` package roster.
- Shape: nuget MCP survey over the managed E57/LAS/LAZ candidate family scoring license, maintenance signal, and net10 asset; verdict lands as the `[PERS-I3]` arming edit plus its packageNeeds row.
- Unlocks: the blocked reality-capture codec becomes buildable.
- Anchors: `IDEAS.md` `[PERS-I3]`; the admission-gate law (supersession-only rejection).
- Tension: BLOCKED as the survey itself — this task IS the resolution route.

[0009]-[QUEUED]: Hook-point roster — mint the `HookPoint` registry and the six lifecycle point rows.
- Capability: `rasm.persistence.<domain>.<point>` names with veto/observe/replay modality columns, mount-census uniqueness, subscriber-fault isolation onto `StatFault`.
- Shape: registry owner beside `SlotRegistry` on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#SLOT_REGISTRY`, contributed point rows on `libs/csharp/Rasm.Persistence/.planning/Element/graph.md`, `libs/csharp/Rasm.Persistence/.planning/Version/egress.md`, `libs/csharp/Rasm.Persistence/.planning/Version/retention.md`, `libs/csharp/Rasm.Persistence/.planning/Version/merge.md`, and `libs/csharp/Rasm.Persistence/.planning/Version/recovery.md`.
- Unlocks: `[PERS-S1]` realization surface pinned page by page.
- Anchors: `IDEAS.md` `[PERS-S1]`; `SlotRegistry.Mount` collision law, per-composition scoping.

[0010]-[QUEUED]: Usage fold — join the reachability walk, class rows, and tenancy into `(tenant, class, tier)` usage receipts.
- Capability: bytes, object counts, and delivery counts per tenant/class/tier as one receipt family under a `store.cost.usage` slot, with `rasm.persistence.usage.*` instrument rows and arms.
- Shape: usage census joining `libs/csharp/Rasm.Persistence/.planning/Store/blobstore.md#BLOB_GC` walk output with `libs/csharp/Rasm.Persistence/.planning/Version/retention.md#RETENTION_CLASSES` bindings; instrument and arm rows on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#STORE_INSTRUMENTS`.
- Unlocks: `[PERS-S2]` chargeback evidence lands on the standing projection rail.
- Anchors: `IDEAS.md` `[PERS-S2]`; identity-tier tenancy rows, the arms one-place law.

[0011]-[QUEUED]: Plan harvest members — three engine capture legs, one digest baseline, one regression verdict.
- Capability: `EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)` over the pooled data source, DuckDB `EXPLAIN ANALYZE` JSON through the standing parse fold, SQLite `EXPLAIN QUERY PLAN` off the raw bridge; digests baselined by statement identity, compare folding to a typed verdict.
- Shape: one `PLAN_PROFILE` section on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` beside the engine harvests, slot `store.stat.plan`, baseline rows in the relational identity tier.
- Unlocks: `[PERS-S3]` regression gates on hot lanes.
- Anchors: `IDEAS.md` `[PERS-S3]`; `DuckProfileHarvest.Decode` fold, `PgStatHarvest` pooled-source pattern.

[0012]-[QUEUED]: Cipher floor row — swap the encrypted bundle in and thread the KMS data key through the open ritual.
- Capability: `PRAGMA key` application inside the idempotent open, data-key mint/unwrap through the custody tier, classification ceilings extended to the offline lane.
- Shape: cipher row on `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#EMBEDDED_FLOOR` composing `SQLitePCLRaw.bundle_e_sqlite3mc`; key custody members on `libs/csharp/Rasm.Persistence/.planning/Element/identity.md#KMS_CUSTODY`.
- Unlocks: `[PERS-S4]` at-rest guarantee on every embedded store.
- Anchors: `IDEAS.md` `[PERS-S4]`; the open-ritual idempotence law, envelope-key algebra.

[0013]-[QUEUED]: Instrumentation subscription rows — Redis, EF, and AWS span trains named with their root posture.
- Capability: each package's AppHost-root subscription posture stated beside the settled Npgsql row, so composition is a copyable row, not tribal knowledge.
- Shape: settled-composition rows on the `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` lead; registry rows on `libs/csharp/Rasm.Persistence/README.md` under the owning label groups.
- Unlocks: `[PERS-S5]` closes the store span train as a set.
- Anchors: `IDEAS.md` `[PERS-S5]`; `Npgsql.OpenTelemetry` row precedent.
- Atomic: composition-lead and README rows only.

[0014]-[QUEUED]: Binding sink rows — the `Mqtt` case and the AMQP re-bind land on the sink table.
- Capability: `EgressSink.Mqtt` with MQTT v5 UserProperties carrying the tracing extension and QoS-1 packet identity as its dedup column; `RabbitMq` `Deliver` leg re-bound through the AMQP protocol binding.
- Shape: case row plus dedup-honesty column on `libs/csharp/Rasm.Persistence/.planning/Version/egress.md#EGRESS_SINK`; pump, envelope, and cursor untouched.
- Unlocks: `[PERS-V5]` transport-family closure.
- Anchors: `IDEAS.md` `[PERS-V5]`; the sink growth law, `MQTTnet` manifest row.

[0015]-[QUEUED]: Census projection member — fold rows, slots, arms, and thresholds into the wire census record.
- Capability: one projection folding `StoreInstruments.Rows`, `SlotRegistry.Mounted()`, `Arms` keys, and threshold hints into a typed wire document.
- Shape: one member on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#STORE_INSTRUMENTS` beside `Telemetry(version)`.
- Unlocks: `[PERS-Q1]` dashboard compilation from declared truth.
- Anchors: `IDEAS.md` `[PERS-Q1]`; the roster and census owners already on the page.
- Atomic: one projection member and its record.

[0016]-[QUEUED]: Corpus family rows — name each benchmark family, its subject owner, and its claim slot on the index.
- Capability: codec chunk/compress/hash, store append and AS-OF fold, structural merge, columnar aggregate, ANN route, and blob multipart as standing families minting fingerprint-gated claims.
- Shape: family rows on `libs/csharp/Rasm.Persistence/.planning/Query/cache.md#BENCHMARK_INDEX` naming `SnapshotCodec`, `GraphStoreOp`, `StructuralMerge`, the columnar lane, `VectorCodebook` routing, and multipart transfer.
- Unlocks: `[PERS-Q2]` regression gates over the durable spine.
- Anchors: `IDEAS.md` `[PERS-Q2]`; the claim-admission and recency owners on the page.

[0017]-[QUEUED]: Manifest projection member — fold the expectation set into the desired-state wire record.
- Capability: extension roster, server postures, cron and partition job rosters, and embedded pragma set as one typed manifest the deploy plane converges on.
- Shape: one projection member on `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#SERVER_EXTENSIONS` folding the `ServerExtension` roster; store-axis coordinates from `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#STORE_AXIS_MAP` ride along.
- Unlocks: `[PERS-S6]` drift-as-diff and derived fleet provisioning.
- Anchors: `IDEAS.md` `[PERS-S6]`; the verification-first fold this projection reuses.
- Atomic: one projection member and its record.

[0018]-[QUEUED]: Partitioned ADBC redemption — `ExecutePartitioned()` fans a federation result across parallel partition readers.
- Capability: the ADBC tabular arm executes partitioned, each `PartitionDescriptor` redeems through `ReadPartition` on its own reader, and the lowered plan streams as parallel Arrow partitions instead of one cursor.
- Shape: one partitioned-execution row on `libs/csharp/Rasm.Persistence/.planning/Query/federation.md#PLAN_LOWERING` over the `AdbcStatement` partition surface the folder catalog carries.
- Unlocks: distributed warehouse reads saturate instead of serializing; the cataloged member stops being shelf inventory.
- Anchors: `api-arrow.md` `ExecutePartitioned()` and `ReadPartition(PartitionDescriptor)` rows, the ADBC arm on the lowering table.
- Atomic: one lowering-table row.

[0019]-[QUEUED]: Schema-pinned contributor mint — the semconv schema pin rides the contributor port so the meter mint stamps it.
- Capability: `StoreInstruments.Telemetry` carries the schema coordinate and the composition-root mint applies `MeterOptions.TelemetrySchemaUrl`, so scope identity satisfies the wire law's schema pin without a folder OTel reference.
- Shape: schema coordinate on the contributor mint at `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#STORE_INSTRUMENTS`; the port-side slot and mint stamp land on the AppHost counterpart.
- Unlocks: schema-aware backends read `rasm.persistence.*` scopes with pinned semantics.
- Anchors: branch `api-diagnostics-metrics.md` `MeterOptions.TelemetrySchemaUrl` law row, `TelemetryContributorPort` on the AppHost port vocabulary.
- Atomic: one mint-signature coordinate.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `Store/observability.md` landed — slot grammar and registry, `pg_stat_statements`/`pg_stat_io` harvest, DuckDB profiling harvest, SQLite status harvest; `OpenTelemetry.Instrumentation.ConfluentKafka` admitted with csproj row, README registry row, and `.api` catalog.
- [0002]-[COMPLETE]: every emitting page carries its `Slots` roster on its primary owner and `SlotRegistry.Mounted()` spreads the census; the topology traversal slot collapsed to `store.topology.traverse` and the vector-route fact respelled `store.vector.route` under the grammar.
