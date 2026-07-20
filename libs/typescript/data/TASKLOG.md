# [TS_DATA_TASKLOG]

Open and closed `data` work distilled from `IDEAS.md`; each task names the exact sub-domain or file it lands in.

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

[0002]-[QUEUED]: Flight SQL lands as an analytical ingress row.
- Capability: `FlightSqlClient` queries remote columnar engines and decodes straight to Arrow tables, joining the analytical matrix as an engine-blind wire row beside the ClickHouse and DuckDB rows.
- Shape: a `lane/olap.md` row — `createFlightSqlClient` construction off lane config, `decodeFlightDataToTable` landing on the same Arrow plane `Olap.ingest` rides.
- Anchors: `.api/qualithm-arrow-flight-client.md`; `lane/olap.md` `_engines` and `Olap.ingest`; the `@connectrpc/connect` transport substrate.
- Tension: the flight row is a read and ingest wire, never a record of truth — journal law holds.

[0003]-[QUEUED]: DuckDB query-profile harvest as a receipt projection.
- Capability: per-query profile receipts harvested through `PRAGMA enable_profiling` json output or `EXPLAIN ANALYZE` — latency, cpu time, rows, operator timings — projected onto Convention duration instruments; receipts stay the truth, instruments the lossy projection.
- Shape: a profile-harvest row on `lane/olap.md` beside `_engines` — profiling keys as row data, one receipt shape spanning the node and wasm arms.
- Anchors: `lane/olap.md` `_engines`; the `journal/fact.md` and `read/batch.md` instrument-row idiom; embedded engines expose no scrape surface, so the harvest is their whole observability.
- Tension: profiling toggles are per-connection state — the row scopes them to the profiled query, never lane-global.

[0004]-[QUEUED]: Engine escalation triggers consume measured probe evidence.
- Capability: lane engine probes — bounded, repeatable measurement runs per `_engines` row — yield claim-shaped receipts whose deltas arm the row's escalation trigger, so an engine escalation is evidence-driven row data.
- Shape: a probe-evidence row beside `_engines` on `lane/olap.md`, receipts in the core claim metric-row shape; the DuckDB profile-harvest receipts supply the measured fields where the engine exposes them.
- Anchors: `lane/olap.md` `_engines` trigger column; core `interchange/codec` claim metric rows; ui `viewer/probe` board join for operator review.
- Tension: probes run beside production lanes — budget and isolation policy bound them to idle windows.

[0005]-[QUEUED]: Layer-topology relations land on the read side — the decoded query-store half of `[LAYER_TOPOLOGY_GRAPH_FACTS]`.
- Capability: `Model.Class` relations for layer identity, layer-path nesting, membership, and per-viewport overrides; `SqlSchema` typed reads and `SqlResolver` batched loaders bound through `Query.table`; a `Lane.Spec` projection binding keeps the relations fold-maintained from journal facts.
- Shape: relation models and resolver rows on `libs/typescript/data/.planning/read/query.md`; the projection lane binding on `libs/typescript/data/.planning/read/fold.md`.
- Anchors: `read/query.md` `MODEL_FAMILY`/`RESOLVER_ROWS`/`TABLE_BINDING`; `read/fold.md` `Lane.Spec` and `Lane.ddl`.
- Tension: rows are detached facts keyed by `ContentKey` — no host layer handle enters any relation.

[0006]-[QUEUED]: Op-log decode and replay rows land on the journal — the consumer half of `[HOST_OPLOG_CRDT_CONSUMER]`.
- Capability: a boundary decoder admits `OperationId`-keyed causal entries; replay folds entries into `Journal.publish` intents under `Occ` arbitration with the commutation policy applied per mutation kind before append; checkpoint snapshots bound replay windows through the windowed `read`.
- Shape: decoder and replay-fold rows on `libs/typescript/data/.planning/journal/append.md`; the entry-payload upcast road on `libs/typescript/data/.planning/journal/evolve.md`.
- Anchors: `journal/append.md` `Journal.publish`/`Occ`/`StreamKey` and the windowed `READ_SURFACE`; `journal/evolve.md` `Upcast.plan`; `object/store.md` `ContentKey` payload custody.
- Tension: C# mints the wire schema — this side decodes, never re-mints; operation identity never conflates with payload identity.

[0007]-[QUEUED]: Object-plane instrument rows land once `Convention` mints `rasm.object.*` — realizes `[OBJECT_PLANE_INSTRUMENT_PROJECTION]`.
- Capability: dedup rate, bytes written, GC reclaim, and resumable-upload throughput project from the receipts the store and stream pages already mint, through the settled instrument-row idiom.
- Shape: instrument rows on `libs/typescript/data/.planning/object/store.md` and `libs/typescript/data/.planning/object/stream.md`, each reading name/description/tags off its `Convention.instrument` entry.
- Anchors: `object/store.md` receipt family; `object/stream.md` `ChunkMark` receipts; `journal/fact.md` `_drained`/`_usage` as the idiom.
- Tension: emission waits on core's `Convention` mint — never a free-string metric name.
- Atomic: instrument rows on two pages over existing receipts.

[0008]-[QUEUED]: Lane instrument rows land across cache, olap, and relay — realizes `[LANE_INSTRUMENT_PROJECTION]`.
- Capability: `cacheStats` hits/misses/size and origin-pool occupancy, OLAP session-gate wait and governed-retry counts, ClickHouse ingest deferrals, and relay/outbox depth project onto Convention instruments sampled by the runtime meter bridge.
- Shape: instrument rows on `libs/typescript/data/.planning/lane/cache.md`, `libs/typescript/data/.planning/lane/olap.md`, and `libs/typescript/data/.planning/journal/append.md`, riding each owner's existing drain or bracket.
- Anchors: `lane/cache.md` `cacheStats` receipt; `lane/olap.md` `_GOVERNOR`/`_INGEST_QUOTA`; `journal/append.md` `Journal.census`; `read/batch.md` timing bracket as the pattern.
- Tension: counters ride owner folds, never per-effect decorators; each row waits on its `Convention` vocabulary entry.

[0009]-[QUEUED]: Pg profile harvest lands — `EXPLAIN (ANALYZE, FORMAT JSON)` and `pg_stat_statements` receipts on the spine, first arm of `[QUERY_PROFILE_RECEIPT_BAND]`.
- Capability: a `pg_stat_statements` extension row joins the matrix (core layer, granting `statements`); windowed delta receipts over its cumulative counters and per-query `EXPLAIN` json harvest land in the one profile receipt shape.
- Shape: the extension row and harvest rows on `libs/typescript/data/.planning/lane/postgres.md`; the receipt schema minted once for all engine arms.
- Anchors: `lane/postgres.md` `_rows` matrix and zero-consumer-edit growth law; `lane/capability.md` batched probe inheriting the new row.
- Tension: `pg_stat_statements` is cumulative shared state — receipts are window deltas keyed by `queryid`, never raw counters; `EXPLAIN ANALYZE` executes the statement, so it scopes to explicit diagnosis, never ambient reads.

[0010]-[QUEUED]: Sqlite statement-status harvest lands — counters and `DBSTAT` aggregates per profile row, second arm of `[QUERY_PROFILE_RECEIPT_BAND]`.
- Capability: `sqlite3_stmt_status`-class counters (fullscan steps, sort operations, autoindex, VM steps) and `DBSTAT` page/space aggregates harvest into the same profile receipt shape, with per-profile availability priced as row data.
- Shape: a harvest row on `libs/typescript/data/.planning/lane/sqlite.md` beside `_degrades`, availability columns per profile.
- Anchors: `lane/sqlite.md` `_degrades` total-vocabulary guard and profile rows; `lane/olap.md` profile-harvest receipt shape shared across arms.
- Tension: wasm-OPFS and D1 expose narrowed introspection — absent counters record as refusal verdicts, never zeros.

[0011]-[QUEUED]: CloudEvents envelope codec lands beside the relay rows — realizes `[RELAY_CLOUDEVENTS_PROJECTION]`.
- Capability: a schema-owned envelope projection over the outbox deliverable — `type`/`source`/`id` from tag, `StreamKey`, and landed sequence; `traceparent`/`tracestate` via the distributed-tracing extension; `rasm.tenant` extension attribute — composing the `cloudevents` SDK vocabulary.
- Shape: envelope codec and projection-fold rows on `libs/typescript/data/.planning/journal/append.md` `RELAY_ROWS`.
- Anchors: `journal/append.md` deliverable model and `Journal.claimBatch` seam; `journal/fact.md` `trace` field.
- Tension: binding mode (structured vs binary) is the carrier's fact across the runtime seam; admission of `cloudevents` rides the serialized admission lane.

[0012]-[QUEUED]: Interop lane page mints and pglite joins the pg profile rows — realizes `[RELATIONAL_SET_COMPLETION]`.
- Capability: `libs/typescript/data/.planning/lane/interop.md` mints with the foreign-relational ingress rows — mysql2 and mssql guarantee pricing, capability degradation against `Pg.Grant`, read-oriented ingress law; `@effect/sql-pglite` lands as the in-process pg profile row beside the driver mints.
- Shape: one new page `libs/typescript/data/.planning/lane/interop.md`; the pglite profile row on `libs/typescript/data/.planning/lane/postgres.md` `DRIVER_ROWS`.
- Anchors: `.api/effect-sql.md` `mysql`/`mssql` dialect arms; `lane/sqlite.md` `_degrades` as the pricing template; `lane/postgres.md` `_client`/`_fromPool` mint pattern.
- Tension: ingress lanes never become records of truth; the three admissions ride the serialized admission lane.

[0013]-[QUEUED]: Archive-tier rows land on the object plane — realizes `[OBJECT_ARCHIVE_TIER]`.
- Capability: `StorageClass` on the conditional put, `Retain.Class`-to-storage-class mapping driving `_lifecycle` transition rules, `RestoreObjectCommand` as a typed restore verb with `InvalidObjectState` folded to an archive-state fault, `StorageClass` evidence on `ObjectStore.Stat`, and `SelectObjectContentCommand` as the server-side projection read.
- Shape: archive rows on `libs/typescript/data/.planning/object/store.md`; the class-mapping row on `libs/typescript/data/.planning/journal/retain.md`.
- Anchors: `.api/aws-sdk-client-s3.md` archive/query command row, `StorageClass` enum, `InvalidObjectState` fault; `object/store.md` `_lifecycle` generator and conformance table.
- Tension: restore is asynchronous — a typed deferral with a poll coordinate, never a blocking read; engines refusing archive classes narrow by conformance row.

[0014]-[QUEUED]: SSE egress modality lands on the live bound — realizes `[LIVE_SSE_CHANNEL]`.
- Capability: `Live.Bound` grows an `sse` projection encoding `changes` emissions into `Sse.Event` frames through `Sse.makeChannel`/`Sse.encoder`, event `id` carrying the emission coordinate for `Last-Event-ID` resume off the mailbox twin.
- Shape: one modality row on `libs/typescript/data/.planning/read/live.md` `LIVE_READS`.
- Anchors: `libs/typescript/.api/effect-experimental.md` `Sse` codec rows; `read/live.md` `Live.of` bound surface.
- Tension: encode here, route and connection lifecycle in runtime.
- Atomic: one modality row over the existing bound.

[0015]-[QUEUED]: Hook-point vocabulary and tap rows land across the domain seams — realizes `[DATA_HOOK_TAP_REGISTRY]`.
- Capability: closed point vocabulary `rasm.data.journal.publish`/`rasm.data.object.admit`/`rasm.data.retain.erase`/`rasm.data.lane.escalate` with veto/observe modalities; veto runs pre-commit at admission seams, observe fans out post-durable-completion with subscriber faults isolated.
- Shape: the vocabulary owner and its tap rows on `libs/typescript/data/.planning/journal/append.md`, `libs/typescript/data/.planning/object/stream.md`, and `libs/typescript/data/.planning/object/file.md`.
- Anchors: `journal/append.md` publish slots and post-commit registrar; `object/stream.md` tus hook seams as `Rail.Spec` values; `object/file.md` codec gate; `lane/tenant.md` `Tenant.afterCommit` fan template.
- Tension: observe taps never join the commit; points key under app identity so co-resident apps never collide.

[0016]-[QUEUED]: Parquet codec row joins the Arrow wire — Table-to-Parquet both directions without an engine.
- Capability: `parquet-wasm` round-trips `apache-arrow` Tables to Parquet bytes in node and browser, so the wasm arm writes lake-format objects straight to the object plane and a Parquet object decodes to a Table without instantiating DuckDB.
- Shape: a codec row on `libs/typescript/data/.planning/lane/olap.md` `ARROW_WIRE` beside `_wire.decode`/`_wire.encode`, egress landing through the object plane's conditional put.
- Anchors: `lane/olap.md` one-wire law and `_wire` family; `object/store.md` content-addressed put; `.api/apache-arrow.md` Table interchange.
- Tension: the codec is interchange, never a query engine — querying stays with the engine rows; `parquet-wasm` admission rides the serialized admission lane.

[0017]-[QUEUED]: AuditJournal satisfaction rows land on the journal and retain pages — realizes `[AUDIT_JOURNAL_SATISFACTION]`.
- Capability: a port-satisfaction row mapping `AuditJournal.append` onto the one atomic write, the audit retention class joining the policy table, sealed subject-field wiring through the `Shredder` algebra, and the DSAR fold widened over audit facts.
- Shape: rows on `libs/typescript/data/.planning/journal/append.md` and `libs/typescript/data/.planning/journal/retain.md`.
- Anchors: security `access/audit.md` port shape (carded); `retain.md` `SubjectKey`/`WrappedKey` folds; `append.md` publish transaction.
- Tension: append-only law holds — no update or delete verb enters the satisfaction; erasure is key destruction, never row mutation.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: signal-site conformance — `read/fold.md` checkpoint gauge re-keyed to `Convention.metric.laneCheckpoint` tagged `rasm.lane.name`; `read/batch.md` gained the `rasm.batch.duration` histogram on the timing bracket; `journal/append.md` gained `Journal.census`, the outbox probe the runtime meter bridge samples.
