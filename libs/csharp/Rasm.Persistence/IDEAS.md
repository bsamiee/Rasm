# [PERSISTENCE_IDEAS]

Forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[PERS-E2]-[QUEUED]: Project-scoped element-set currency — `ElementSet` membership widened to `(ModelId, NodeId)` so coordination selections span federated models.
- Capability: element-set algebra whose members carry their owning model — cross-model clash sets, whole-project QTO subjects, and discipline-spanning rule selections as one content-addressed currency.
- Shape: a model-qualified membership axis on `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet` (preimage re-framed over model-qualified keys), evaluation resolving across the roster `ProjectGraph` carries; `Query/topology` gains the multi-graph view the durable `ModelLink` edges anticipate.
- Unlocks: selection and topology answer at the federation altitude the `ModelLink` edge family opened — a duct-penetrates-wall selection spans models as one `SetExpr`, and a project QTO subject is one set.
- Anchors: `Element/graph` `ModelLink`/`LinkKind`/`ProjectGraph`/`ProjectRollup` (landed), the length-framed content-addressed preimage discipline, the one-stream-per-model law.
- Tension: the frozen `elementset` parity vector (`ContentParityCorpus` `ParitySlot.ElementSet`) binds the `NodeId`-only preimage — widening membership re-cuts that parity contract in the same pass.

[PERS-V4]-[QUEUED]: Inbound CDC consume leg — an ingress owner draining foreign broker topics onto the durable rail.
- Capability: the ingest half of the CDC boundary — foreign Kafka events admitted, deduped by content key, and folded onto the op-log with W3C context continued off message headers.
- Shape: one ingress owner beside `Version/egress` (the sink family stays egress-only), consuming through `InstrumentedConsumerBuilder` with `TryExtractPropagationContext` and `ConsumeAndProcessMessageAsync`, consumer registration riding the AppHost root.
- Unlocks: cross-estate ingestion — a sibling deployment's egress stream replays into this store as a first-class source.
- Anchors: the `api-otel-instrumentation-confluentkafka` consumer twins, the `Version/egress#EGRESS_SINK` dedup-honesty column, the content-key envelope `id`.

[PERS-V5]-[QUEUED]: CloudEvents protocol-binding completion — the MQTT sink case and the AMQP binding close the transport family the envelope already serves.
- Capability: every broker class the estate names carries the one CloudEvents envelope over its native binding — `EgressSink.Mqtt` publishes over MQTT v5 UserProperties with the distributed-tracing extension, and the `RabbitMq` case swaps its hand-mapped headers for the AMQP protocol binding.
- Shape: one `Mqtt` case row on `libs/csharp/Rasm.Persistence/.planning/Version/egress.md#EGRESS_SINK` composing the admitted `MQTTnet` client through `CloudNative.CloudEvents.Mqtt`, QoS-1 packet identity as the dedup column; the `RabbitMq` case's `Deliver` leg re-binds through `CloudNative.CloudEvents.Amqp` — pump, envelope, cursor, and receipt untouched per the sink growth law.
- Unlocks: edge/IoT delivery targets and AMQP-native consumers join the CDC fan without a bespoke envelope; the binding family closes as a set instead of a Kafka-weighted partial.
- Anchors: the `EgressSink` closed `[Union]` growth row, `Egress.Envelope` one-projection law, `CloudNative.CloudEvents.Kafka` binding precedent, `MQTTnet` in the central manifest.
- Tension: `CloudNative.CloudEvents.Mqtt` and `CloudNative.CloudEvents.Amqp` ride the admission lane; cards assume the admissions land.

[PERS-S1]-[QUEUED]: Persistence hook rail — typed `rasm.persistence.<domain>.<point>` hook points give the durable lifecycle veto/observe/replay taps.
- Capability: domain facts on the durable spine become subscribable points — pre-append veto, post-commit observe, egress-delivery observe, retention-sweep veto, merge-conflict observe, recovery-replay — with subscriber faults isolated onto the folder fault rail and telemetry staying a tap, never an emit call in domain code.
- Shape: one `HookPoint` registry beside `SlotRegistry` on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` under the same mount-census pattern, point rows contributed by `libs/csharp/Rasm.Persistence/.planning/Element/graph.md`, `libs/csharp/Rasm.Persistence/.planning/Version/egress.md`, `libs/csharp/Rasm.Persistence/.planning/Version/retention.md`, `libs/csharp/Rasm.Persistence/.planning/Version/merge.md`, and `libs/csharp/Rasm.Persistence/.planning/Version/recovery.md`; registries scope per composition so two apps never share a mount.
- Unlocks: policy engines, audit sidecars, and UI live-update legs subscribe to durable facts without touching owner rails; the branch hook law lands in the one C# folder still tapping only the AppHost receipt point.
- Anchors: `SlotRegistry.Mounted()` census precedent, the AppHost hook-rail receipt point the instrument projection already rides, `StatFault` rail, the per-app neutrality law.

[PERS-Q1]-[QUEUED]: Telemetry census egress — instrument roster, slot roster, and threshold hints project as one wire census the dashboard plane compiles from.
- Capability: everything Persistence emits — `InstrumentRow` names/units/descriptions, `StoreSlot` census, projection-arm keys, and alert-relevant threshold hints — folds into one typed census document, so store dashboards and burn-rate alert rules generate from declared truth instead of hand-listed metric names.
- Shape: one census projection member on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#STORE_INSTRUMENTS` folding `StoreInstruments.Rows`, `SlotRegistry.Mounted()`, and `Arms` keys into a wire JSON record the estate ships beside the package.
- Unlocks: the Foundation-SDK compile leg consumes `rasm.persistence.*` as data — a new instrument row appears on the board with zero dashboard edits.
- Anchors: `StoreInstruments.Rows` roster, `SlotRegistry` census, the estate dashboard-compile leg over the Foundation-SDK.

[PERS-S2]-[QUEUED]: Storage cost attribution — per-tenant, per-retention-class, per-tier durable-usage accounting turns storage truth into chargeback evidence.
- Capability: identity-tier tenancy rows, blob object sizes, retention classes, and egress delivery counts fold into usage receipts — bytes, objects, deliveries keyed `(tenant, class, tier)` — projected as `rasm.persistence.usage.*` instruments; journal rows stay billing truth, instruments stay the lossy dashboard channel.
- Shape: one usage fold joining the reachability walk on `libs/csharp/Rasm.Persistence/.planning/Store/blobstore.md#BLOB_GC` with the class rows on `libs/csharp/Rasm.Persistence/.planning/Version/retention.md#RETENTION_CLASSES`, receipts under a `store.cost.usage` slot and instrument rows on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md#STORE_INSTRUMENTS`.
- Unlocks: metered multi-tenant storage products and per-project spend boards; the estate cost-attribution move gains its durable-bytes half.
- Anchors: tenancy on the identity tier, `BLOB_GC` full-history reachability walk, `RetentionClass` storage-lane binding, the `rasm.tenant` baggage law.
- Tension: tenant cardinality caps bound the per-tenant series grain; above the cap, attribution rides receipts and exemplar-sampled traces, never unbounded tag values.
- Ripple: `libs` `[COST_ATTRIBUTION_BAGGAGE]`.

[PERS-S3]-[QUEUED]: Plan-profile rail — three-engine query-plan harvest with digest baselines makes plan regression a typed verdict.
- Capability: suspect statements bracket under plan capture — PostgreSQL `EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)`, DuckDB `EXPLAIN ANALYZE` JSON through the standing profile parse fold, SQLite `EXPLAIN QUERY PLAN` — each folding to a plan receipt whose digest compares against a stored baseline, so a flipped join order or lost index surfaces as a regression verdict, not a latency mystery.
- Shape: one `PlanProfile` section beside the engine harvests on `libs/csharp/Rasm.Persistence/.planning/Store/observability.md`, receipts under a `store.stat.plan` slot, baseline digests persisted as relational rows keyed by statement identity.
- Unlocks: plan-regression gates on the hot read lanes; the `queryid` join from `pg_stat_statements` gains its explaining half.
- Anchors: `DuckProfileHarvest` parse fold and `PlanDigest` precedent, `pg_stat_statements` queryid identity, the SQLite raw-bridge handle path.
- Tension: SQLite scan-status depth gates on bundled-provider compile options — same `PRAGMA compile_options` verification route as `[DBSTAT_ENABLEMENT]`.

[PERS-S5]-[QUEUED]: Store client instrumentation set completion — Redis, EF, and cloud-SDK span trains join the settled Npgsql pattern.
- Capability: every held store client emits provider spans under the AppHost root — Redis command spans off the cache backplane, EF command spans over the relational tier, S3/KMS client-call spans over the object-store and custody paths — closing the train the Kafka and Npgsql legs opened.
- Shape: settled-composition rows on the `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` lead naming `OpenTelemetry.Instrumentation.StackExchangeRedis`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`, and `OpenTelemetry.Instrumentation.AWS` with their AppHost-root subscription posture, plus registry rows on `libs/csharp/Rasm.Persistence/README.md`.
- Unlocks: cache, ORM, and object-store hops join the one trace the driver spans already carry; the instrumentation family finishes as a set.
- Anchors: `Npgsql.OpenTelemetry` AppHost-root precedent, `OpenTelemetry.Instrumentation.ConfluentKafka` admission, held `StackExchange.Redis`, EF providers, `AWSSDK.S3`, `AWSSDK.KeyManagementService`.
- Tension: all three packages ride the admission lane; cards assume the admissions land.

[PERS-S6]-[QUEUED]: Provisioning desired-state manifest — the verification fold's expectation set egresses as the converge target the deploy plane consumes.
- Capability: everything verification already asserts — extension roster, server postures (`compute_query_id`, `track_io_timing`, preload rows), cron and partition-maintenance job rosters, embedded-floor pragma set — projects as one typed manifest, so the deploy plane converges toward declared truth while in-process provisioning stays verification-only.
- Shape: one manifest projection member on `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#SERVER_EXTENSIONS` folding the `ServerExtension` roster and posture rows into a wire record, the store-axis coordinates on `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#STORE_AXIS_MAP` riding along.
- Unlocks: server drift becomes a diff between two typed documents; fleet provisioning scripts derive from the manifest instead of restating the roster by hand.
- Anchors: verification-first `ClusterProvision` fold, the `ServerExtension` authoritative roster, the identity tier's idempotent deploy-SQL lanes.

[PERS-S4]-[QUEUED]: Encrypted embedded floor — the multi-cipher SQLite bundle puts the offline store under KMS-custodied keys.
- Capability: embedded-floor data at rest encrypts under a per-store data key — cipher bundle swapped in, key custody riding the standing KMS tier, open ritual extended with key application — so a stolen laptop or synced file leaks nothing.
- Shape: one cipher row on `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md#EMBEDDED_FLOOR` composing `SQLitePCLRaw.bundle_e_sqlite3mc` with key application in the idempotent open ritual, data-key mint and unwrap through `libs/csharp/Rasm.Persistence/.planning/Element/identity.md#KMS_CUSTODY`.
- Unlocks: classification ceilings extend to the offline lane; field laptops carry regulated models without a compliance carve-out.
- Anchors: `EMBEDDED_FLOOR` open ritual, `KMS_CUSTODY` envelope-key algebra, held cloud KMS clients.
- Tension: `SQLitePCLRaw.bundle_e_sqlite3mc` rides the admission lane and supersedes the plain bundle row where the encrypted floor mounts; bundle coexistence is a provisioning row decision, not dual providers on one connection.

[PERS-Q2]-[QUEUED]: Persistence benchmark corpus — named BenchmarkDotNet families mint the claims the benchmark index was built to admit.
- Capability: the folder's hot paths carry standing benchmark families — codec chunk/compress/hash, store append and AS-OF fold, structural merge, columnar aggregate, ANN route, blob multipart — each minting fingerprint-gated `BenchmarkRow` claims, so regressions gate on measured deltas instead of review intuition.
- Shape: corpus family rows on `libs/csharp/Rasm.Persistence/.planning/Query/cache.md#BENCHMARK_INDEX` naming each family's subject owner — `SnapshotCodec`, `GraphStoreOp`, `StructuralMerge`, the columnar lane, `VectorCodebook` routing, multipart transfer — with claims admitted through the standing host-fingerprint gate.
- Unlocks: the corpus-gate law lands on the durable spine; a slower codec or merge fold fails a claim, not a vibe check.
- Anchors: `BENCHMARK_INDEX` claim admission and recency owner, `BenchmarkDotNet` in the test substrate, branch corpus-gate law.

[PERS-I3]-[BLOCKED]: Reality-capture codec — E57/LAS/LAZ point-cloud ingest into chunked residence with H3 spatial bucketing.
- Capability: the as-built half of the model lifecycle — scan header/metadata rows, registration transform, chunked blob residence, per-region cells — feeding compare-to-design compute without owning scan semantics.
- Shape: one new Ingest codec page at `libs/csharp/Rasm.Persistence/.planning/Ingest/pointcloud.md` under the [A.4] growth row, bytes through Store/blobstore#MULTIPART_TRANSFER + Element/codec#CONTENT_CHUNKING, region cells through Element/identity H3Cell.
- Unlocks: scan-to-BIM verification; the heaviest residence-demanding payload class gains an entry point.
- Anchors: Store/blobstore#CONTENT_CHUNKING (FastCDC), Element/identity H3Cell, Ingest/geospatial and Ingest/issue (the [A.4] codec-page pattern).
- Tension: BLOCKED on one answerable question — which managed E57/LAS/LAZ codec package admits under the gate (license, maintenance signal, net10 asset)? Resolution route: nuget MCP survey over the E57/LAS candidate family; hand-rolling the E57 XML+binary layout without that ruling is the forbidden alternative.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: Engine-stat observability and the receipt-slot registry — landed as `.planning/Store/observability.md` with the `store.<domain>.<verb>` slot grammar, the composition-time registry, and the pg/DuckDB/SQLite harvest receipts.
