# [TS_DATA_IDEAS]

Forward pool of higher-order `data` concepts grounded in the durable-persistence domain; an idea drives one or more `TASKLOG.md` tasks.

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

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows land as read-side query-store relations for transport and visualization.
- Capability: Wire-carried layer and relation keys decode into `Model.Class` relations — layer identity, layer-path nesting, membership, and per-viewport overrides as decoded rows — so the read side serves host organization to transport and visualization consumers keyed by the one `ContentKey`, with no host handle.
- Shape: A boundary decoder folds the detached fact rows into the read side's projection tables; `SqlSchema` typed reads and `SqlResolver` batched loaders serve layer organization over `Query.table`, the object and journal planes carry the rows across runtimes under the one `ContentKey`, and the decoded relations feed the layer-visualization surface; lands in `libs/typescript/data/.planning/read/query.md` with the projection lane binding in `libs/typescript/data/.planning/read/fold.md`.
- Unlocks: Host-organized read-side queries, cross-runtime layer transport, and a visualization-ready organizational axis every peer reads by content identity.
- Anchors: `read/query.md` `Model.Class`/`SqlSchema`/`SqlResolver`/`Query.table`; the one `ContentKey` content-identity wire; `README.md` durable-persistence plane and the bit-identical content-identity demand across wire peers.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the query-store relations carry only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[HOST_OPLOG_CRDT_CONSUMER]-[QUEUED]: Host op-log entries decode, replay, and merge against the journal plane — the TypeScript end of the shared op-log CRDT wire owner.
- Capability: `OperationId`-keyed causal entries decode at the boundary and replay through the journal's one write owner, so cross-runtime sync, collaborative merge, and checkpoint replay land as journal operations keyed by the shared causal identity, with `ContentHash` payloads resolved through the object plane.
- Shape: A boundary decoder admits the C#-minted op-log wire rows; replay folds entries into `Journal.publish` intents under `Occ` arbitration, merge applies the CRDT commutation policy per mutation kind before append, and a checkpoint snapshot bounds replay windows through the journal's windowed `read`; lands in `libs/typescript/data/.planning/journal/append.md` with the payload-version road in `libs/typescript/data/.planning/journal/evolve.md`.
- Unlocks: Multi-runtime document sync into the durable plane, deterministic replay for audit, and the consumer half that arms the producer's wire.
- Anchors: `journal/append.md` `Journal.publish`/`Occ`/`StreamKey`; `journal/evolve.md` upcast road for entry payload versions; `object/store.md` `ContentKey` payload custody.
- Tension: C# mints the wire schema and codec — this plane decodes and never re-mints; merge policy settles commutation per mutation kind without conflating operation identity with payload identity.
- Ripple: `libs/.planning` `[HOST_OPLOG_CRDT_PRODUCER]`.

[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[QUEUED]: Object-plane receipts gain their lossy instrument projections once `Convention` mints the `rasm.object.*` rows.
- Capability: Dedup rate (`ObjectStore.Receipt.written`), bytes written, GC reclaim, and resumable-upload throughput project from receipts the store and stream pages already mint — receipts stay the truth, instruments the dashboard projection.
- Shape: `Convention` rows land first under its growth law (metric name with instrument metadata), then the object owners emit through the same instrument-row idiom the journal and read pages carry; lands in `libs/typescript/data/.planning/object/store.md` and `libs/typescript/data/.planning/object/stream.md`.
- Unlocks: Object-plane health on the estate dashboards with zero new evidence surfaces.
- Anchors: `object/store.md` receipt family; `object/stream.md` `ChunkMark` and Merkle proof receipts; the `journal/fact.md`/`read/fold.md` instrument-row idiom.
- Tension: `Convention` rows are core's mint — this folder emits only after the vocabulary exists, never through a free-string metric name.
- Ripple: `libs/.planning` `[UNIFIED_SIGNAL_FABRIC]`.

[LANE_INSTRUMENT_PROJECTION]-[QUEUED]: Every lane receipt gains its lossy Convention instrument projection — the folder's whole signal surface, not just the object plane.
- Capability: Cache `cacheStats` hits/misses/size, origin-pool occupancy, the OLAP governor's session-gate wait and retry counts, ClickHouse ingest deferral under the token-bucket quota, and relay/outbox depth all project onto `Convention` instrument rows the runtime meter bridge samples — receipts stay the truth, instruments the dashboard channel, matching the batch histogram and lane-checkpoint gauge already landed.
- Shape: Instrument rows ride each owner's existing drain or bracket — never per-effect decorators — landing in `libs/typescript/data/.planning/lane/cache.md`, `libs/typescript/data/.planning/lane/olap.md`, and `libs/typescript/data/.planning/journal/append.md`; each row reads name, description, and tag keys off its `Convention.instrument` entry exactly as `read/fold.md` `laneCheckpoint` and `journal/fact.md` `factDrained`/`meterUsage` do.
- Unlocks: Lane health, saturation, and backpressure visible on the estate boards; escalation-trigger reviews argued from measured gate pressure instead of anecdote.
- Anchors: `lane/cache.md` `cacheStats` receipt law; `lane/olap.md` `_GOVERNOR` and `_INGEST_QUOTA` policy rows; `journal/append.md` `Journal.census` outbox probe; `read/batch.md` `batchDuration` histogram as the settled idiom.
- Tension: `Convention` rows are core's mint — each instrument lands only after its vocabulary row exists, never through a free-string name.

[QUERY_PROFILE_RECEIPT_BAND]-[QUEUED]: One engine-profile receipt band spans every lane — pg, sqlite, and DuckDB query evidence in one schema-owned shape.
- Capability: Per-query profile receipts — latency, rows, operator timings, buffer/IO counters — harvest from each engine's native surface (pg `EXPLAIN (ANALYZE, FORMAT JSON)` and `pg_stat_statements`, sqlite `sqlite3_stmt_status`-class counters and `DBSTAT` aggregates, DuckDB `PRAGMA enable_profiling` json) into ONE receipt family, so engine-profile parity is structural and a slow query is diagnosable identically on every lane.
- Shape: One profile receipt schema with per-engine harvest arms — a `pg_stat_statements` extension row joins the `lane/postgres.md` matrix (core layer, granting `statements`), harvest rows land in `libs/typescript/data/.planning/lane/postgres.md`, `libs/typescript/data/.planning/lane/sqlite.md`, and `libs/typescript/data/.planning/lane/olap.md`; receipts feed the probe-evidence escalation triggers and project onto Convention duration instruments.
- Unlocks: Evidence-driven engine escalation, slow-query triage across profiles, and the measured fields the benchmark probe rows consume.
- Anchors: `lane/postgres.md` `_rows` extension matrix and its zero-consumer-edit growth law; `lane/sqlite.md` `_degrades` total-vocabulary guard; `lane/olap.md` `_engines`; embedded engines expose no scrape surface, so harvest is their whole observability.
- Tension: Profiling toggles are per-connection or per-statement state — every arm scopes them to the profiled query, never lane-global; `pg_stat_statements` is cumulative shared state, so its receipts are window deltas, never raw counters.

[RELAY_CLOUDEVENTS_PROJECTION]-[QUEUED]: Outbox deliverables project as CloudEvents envelopes — the journal's wire-neutral egress every runtime transport carries unchanged.
- Capability: Each relay deliverable projects into a CloudEvents envelope — `type` from the event tag, `source` from `StreamKey`, `id` from the landed sequence, W3C `traceparent`/`tracestate` via the distributed-tracing extension, tenant as a `rasm.tenant` extension attribute — so NATS, MQTT, Connect, and Kafka carriers deliver journal facts as standard events with zero per-transport envelope forks.
- Shape: A schema-owned envelope codec beside the relay rows in `libs/typescript/data/.planning/journal/append.md` — projection is a fold over the outbox row and its receipt, binding-mode selection (structured vs binary) stays a carrier fact across the runtime seam; composes the `cloudevents` SDK for the envelope and extension vocabulary.
- Unlocks: Cross-runtime event delivery on any transport, trace-correlated consumption, and the C#/python peers decoding one envelope shape.
- Anchors: `journal/append.md` `RELAY_ROWS` deliverable model and `Journal.claimBatch` runtime seam; `journal/fact.md` `trace` field; `ARCHITECTURE.md` `[BOUNDARY]: Journal.claimBatch` bidirectional edge.
- Tension: Data owns the envelope projection, runtime owns carriage and binding mode — the pair splits exactly at the claim seam, and the envelope never becomes a second record of truth.

[RELATIONAL_SET_COMPLETION]-[QUEUED]: Effect-sql store family completes — pglite joins the pg lane as its in-process profile, mysql2 and mssql land as foreign-relational ingress rows.
- Capability: `@effect/sql-pglite` runs the true pg contract in-process (WASM, zero daemon) as a pg-lane profile row for browser and edge arms; `@effect/sql-mysql2` and `@effect/sql-mssql` open read-oriented interop lanes into enterprise-held MySQL/SQL-Server data — the `sql.onDialect` union already carries `mysql`/`mssql` arms no lane exploits, so the dialect algebra is pre-paid.
- Shape: One new page `libs/typescript/data/.planning/lane/interop.md` owns the foreign-relational ingress rows — guarantee pricing, capability degradation against the `Pg.Grant` vocabulary, never a record of truth — and the pglite profile row lands beside the driver mints in `libs/typescript/data/.planning/lane/postgres.md`.
- Unlocks: Browser-resident pg semantics without the sqlite degradation table, enterprise data ingress for AEC apps whose estate data lives in MSSQL/MySQL, and closure of the effect-sql client family the manifest holds partially.
- Anchors: `.api/effect-sql.md` five-way `Dialect` discriminant with `mysql`/`mssql` arm-keys; `lane/sqlite.md` `_degrades` as the degradation-pricing template; `README.md` law that a backend enters as a semantic-guarantee row on its owning lane; `@electric-sql/pglite` already vetted in the test cluster.
- Tension: Interop lanes are ingress, never authority — journal law holds; admission of `@effect/sql-pglite`, `@effect/sql-mysql2`, `@effect/sql-mssql` rides the serialized admission lane.

[OBJECT_ARCHIVE_TIER]-[QUEUED]: Object plane gains the cold-tier archival axis — storage-class transitions keyed by retention class, restore as a typed verb.
- Capability: `StorageClass` on the conditional put, lifecycle transition rules generated from `Retain.Policy` beside the existing expiry rules, `RestoreObjectCommand` as the restore verb with `InvalidObjectState` folded to a typed archive-state fault, restore-progress evidence on `ObjectStore.Stat` via `GetObjectAttributesCommand`'s `StorageClass` member, and `SelectObjectContentCommand` as the server-side projection read over archived structured objects — cold data prices storage honestly without leaving the content-addressed plane.
- Shape: Archive rows land in `libs/typescript/data/.planning/object/store.md` — a class-to-storage-class mapping row derives from the one retention vocabulary in `libs/typescript/data/.planning/journal/retain.md`, transitions ride the existing `_lifecycle` generator, and the restore verb joins the command-value family under the one abort-bridged `send`.
- Unlocks: Regulatory-class objects age to Glacier-tier pricing automatically, DSAR export over archived subjects restores on demand, and the GC sweep prices restore latency instead of treating every object as hot.
- Anchors: `.api/aws-sdk-client-s3.md` `RestoreObjectCommand`/`SelectObjectContentCommand` archive/query row, `StorageClass` enum vocabulary, `InvalidObjectState` tagged fault, `PutBucketLifecycleConfigurationCommand`; `object/store.md` `_lifecycle` generation from `Retain.Policy` and the two-layer GC law.
- Tension: Restore is asynchronous — a read against an archived key is a typed deferral with a poll coordinate, never a blocking wait; S3-compatible engines refusing archive classes narrow by the conformance-table row, never by fork.

[LIVE_SSE_CHANNEL]-[QUEUED]: Live bindings gain the SSE egress modality — change emissions encode through the one branch SSE codec at the data seam.
- Capability: `Live.Bound` grows an `sse` projection encoding each `changes` emission into `Sse.Event` frames through `Sse.makeChannel`/`Sse.encoder`, so the runtime HTTP seam serves a live view as a standards-shaped event stream with reconnection `retry:` directives, and every SSE surface in the branch shares one codec.
- Shape: One modality row on the bound surface in `libs/typescript/data/.planning/read/live.md` — the channel wraps the existing decoded `changes` stream, event `id` carries the emission coordinate so `Last-Event-ID` resume replays from the mailbox twin, and the route itself stays runtime's.
- Unlocks: Browser live views over plain HTTP with zero socket infrastructure, resumable change feeds, and the `Sse` substrate member the branch admits but no page exploits.
- Anchors: `libs/typescript/.api/effect-experimental.md` `Sse.makeChannel`/`makeParser`/`encoder` codec rows and `Sse.Retry`; `read/live.md` `Live.of` three-modality bound; `ARCHITECTURE.md` `[SHAPE]: Live.changes` runtime seam.
- Tension: Data owns the encode, runtime owns the route and connection lifecycle — the codec value crosses the seam, the HTTP server never enters this folder.

[DATA_HOOK_TAP_REGISTRY]-[QUEUED]: Scattered gate and tap seams unify into one typed data hook-point vocabulary — telemetry and policy subscribe to domain facts, never instrument domain code.
- Capability: Hook points `rasm.data.journal.publish`, `rasm.data.object.admit`, `rasm.data.retain.erase`, and `rasm.data.lane.escalate` land as a closed, typed point vocabulary with veto/observe modalities — the publish transaction's slots, the tus admission hooks, the file-plane codec gate, and the erase tombstone already ARE these seams page-locally; the registry names them once so observability, audit, and app policy subscribe by point key with subscriber faults isolated onto the fault rail.
- Shape: One point-vocabulary owner with per-page tap rows — landing across `libs/typescript/data/.planning/journal/append.md`, `libs/typescript/data/.planning/object/stream.md`, and `libs/typescript/data/.planning/object/file.md` — veto points run pre-commit inside the owning transaction, observe points fan out post-durable-completion beside the `Live` stamp, and two apps composing the same libraries never collide because points key under app identity.
- Unlocks: App-composable data-plane policy (admission veto, compliance observers) without forking owner pages; the branch hook-rail doctrine realized at the data altitude.
- Anchors: `journal/append.md` publish-transaction slots and post-commit registrar; `object/stream.md` `onUploadCreate`/`onIncomingRequest`/`onUploadFinish` armed seams as `Rail.Spec` values; `object/file.md` codec-gated intake; `lane/tenant.md` `Tenant.afterCommit` roster as the post-commit fan template.
- Tension: Observe taps never join the commit — a slow subscriber costs fan-out latency, never write availability; veto points are bounded to admission seams so the journal's atomicity is untouched.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
