# [TS_DATA_TASKLOG]

Open and closed `data` work distilled from `IDEAS.md`; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
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

[FLIGHT_SQL_INGRESS_ROW]-[QUEUED]: Flight SQL lands as an analytical ingress row.
- Capability: `FlightSqlClient` queries remote columnar engines and decodes straight to Arrow tables, joining the analytical matrix as an engine-blind wire row beside the ClickHouse and DuckDB rows.
- Shape: a `lane/olap.md` row — `createFlightSqlClient` construction off lane config, `decodeFlightDataToTable` landing on the same Arrow plane `Olap.ingest` rides.
- Unlocks: any Flight SQL server joins the analytical matrix as an engine-blind ingress row, decoding straight to Arrow tables beside the ClickHouse and DuckDB rows with no per-engine driver.
- Anchors: `.api/qualithm-arrow-flight-client.md`; `lane/olap.md` `_engines` and `Olap.ingest`; the `@connectrpc/connect` transport substrate.
- Tension: the flight row is a read and ingest wire, never a record of truth — journal law holds.

[LAYER_TOPOLOGY_READ_ROWS]-[QUEUED]: Layer-topology relations land on the read side — the decoded query-store half of `[LAYER_TOPOLOGY_GRAPH_FACTS]`.
- Capability: `Model.Class` relations for layer identity, layer-path nesting, membership, and per-viewport overrides; `SqlSchema` typed reads and `SqlResolver` batched loaders bound through `Query.table`; a `Lane.Spec` projection binding keeps the relations fold-maintained from journal facts.
- Shape: relation models and resolver rows on `libs/typescript/data/.planning/read/query.md`; the projection lane binding on `libs/typescript/data/.planning/read/fold.md`.
- Unlocks: IDEAS.md [LAYER_TOPOLOGY_GRAPH_FACTS] — host-organized read-side queries and cross-runtime layer transport, the decoded relations keyed by content identity feeding visualization.
- Anchors: `read/query.md` `MODEL_FAMILY`/`RESOLVER_ROWS`/`TABLE_BINDING`; `read/fold.md` `Lane.Spec` and `Lane.ddl`.
- Tension: rows are detached facts keyed by `ContentKey` — no host layer handle enters any relation.

[OPLOG_REPLAY_ROWS]-[QUEUED]: Op-log decode and replay rows land on the journal — the consumer half of `[HOST_OPLOG_CRDT_CONSUMER]`.
- Capability: a boundary decoder admits `OperationId`-keyed causal entries; replay folds entries into `Journal.publish` intents under `Occ` arbitration with the commutation policy applied per mutation kind before append; checkpoint snapshots bound replay windows through the windowed `read`.
- Shape: decoder and replay-fold rows on `libs/typescript/data/.planning/journal/append.md`; the entry-payload upcast road on `libs/typescript/data/.planning/journal/evolve.md`.
- Unlocks: IDEAS.md [HOST_OPLOG_CRDT_CONSUMER] — multi-runtime document sync into the durable plane and deterministic replay for audit, the consumer half arming the producer's wire.
- Anchors: `journal/append.md` `Journal.publish`/`Occ`/`StreamKey` and the windowed `READ_SURFACE`; `journal/evolve.md` `Upcast.plan`; `object/store.md` `ContentKey` payload custody.
- Tension: C# mints the wire schema — this side decodes, never re-mints; operation identity never conflates with payload identity.

[INTEROP_LANE_PAGE]-[QUEUED]: Interop lane page mints and pglite joins the pg profile rows — realizes `[RELATIONAL_SET_COMPLETION]`.
- Capability: `libs/typescript/data/.planning/lane/interop.md` mints with the foreign-relational ingress rows — mysql2 and mssql guarantee pricing, capability degradation against `Pg.Grant`, read-oriented ingress law; `@effect/sql-pglite` lands as the in-process pg profile row beside the driver mints.
- Shape: one new page `libs/typescript/data/.planning/lane/interop.md`; the pglite profile row on `libs/typescript/data/.planning/lane/postgres.md` `DRIVER_ROWS`.
- Unlocks: IDEAS.md [RELATIONAL_SET_COMPLETION] — browser-resident pg semantics without the sqlite degradation table and enterprise MSSQL/MySQL ingress, closing the effect-sql client family.
- Anchors: `.api/effect-sql.md` `mysql`/`mssql` dialect arms; `lane/sqlite.md` `_degrades` as the pricing template; `lane/postgres.md` `_client`/`_fromPool` mint pattern; the read-only-interop ruling at `libs/typescript/data/RULINGS.md` `[01]-[PACKAGES]`.
- Tension: ingress lanes never become records of truth and never reach the tenant write path — the `README.md` interop rows align to that posture (the "lighting the idle arms" phrasing dies with this landing); the three admissions ride the serialized admission lane.

[FACT_JOURNAL_RLS_ROW]-[QUEUED]: `fact_journal` registers under the tenancy law like every journal relation.
- Capability: the fact table's DDL carries the RLS registration its own law line asserts, so the tenant write path's every-relation-registers demand is structural, never prose.
- Shape: one `Tenancy.rls("fact_journal")` interpolation in `_factDdl.pg` in `libs/typescript/data/.planning/journal/fact.md`, matching the three journal-table DDLs.
- Unlocks: every tenant-carrying relation provably registers — the tenancy registration law holds with zero exceptions.
- Anchors: `fact.md` law line asserting `Tenancy.rls("fact_journal")`; `journal/append.md` `${Tenancy.rls("journal_event")}`/`idempotency_ledger`/`outbox` DDL interpolations; `lane/tenant.md` `Tenancy.rls(relation)` ensure.
- Atomic: one DDL interpolation.

[OBJECT_REF_READ_CONTRACT]-[QUEUED]: DSAR export composes a published reference-read contract, never raw cross-strata SQL.
- Capability: the object store publishes its owner-keyed reference read as a seam contract and the retention plane composes it, so the strata direction holds and the reference relation has one reader surface.
- Shape: one published read contract on `libs/typescript/data/.planning/object/store.md` beside the reference verbs; the `_dsar.objects` raw `SELECT` in `libs/typescript/data/.planning/journal/retain.md` swaps to the composition.
- Unlocks: an `object_ref` schema change ripples through one contract; the retention plane carries none of the store's SQL.
- Anchors: `retain.md` `_dsar` raw `object_ref` query; `store.md` `object_ref` ensure and reference verbs; `journal/append.md` `Journal.claimBatch` as the seam-publication precedent.

[OWNER_NAMESPACE_CONTRACT]-[QUEUED]: `object_ref.owner` carries a stated `<producer>:<coordinate>` namespace contract at its owner.
- Capability: the store states the full owner-prefix namespace — which prefixes exist, which drive the GC cascade, which drive the DSAR scan — so a fresh producer prefix without its cascade and erasure forms is a stated defect, never a silent sweep hole.
- Shape: one namespace-contract law block on `libs/typescript/data/.planning/object/store.md` `[04]` beside the `derivative:<sourceKey>` cascade law.
- Unlocks: sweep and erasure stay total as byte planes grow; a new producer's prefix is one contract row.
- Anchors: coining sites — `object/remote.md` `remote:`, `object/stream.md` `tus:`, `object/file.md` `disk:`/`derivative:`; `retain.md`'s subject-keyed DSAR scan; `store.md` `[04]` cascade law.
- Atomic: one law block at the owner.

[ARCHIVE_TIER_ROWS]-[QUEUED]: Archive-tier rows land on the object plane — realizes `[OBJECT_ARCHIVE_TIER]`.
- Capability: `StorageClass` on the conditional put, `Retain.Class`-to-storage-class mapping driving `_lifecycle` transition rules, `RestoreObjectCommand` as a typed restore verb with `InvalidObjectState` folded to an archive-state fault, `StorageClass` evidence on `ObjectStore.Stat`, and `SelectObjectContentCommand` as the server-side projection read.
- Shape: archive rows on `libs/typescript/data/.planning/object/store.md`; the class-mapping row on `libs/typescript/data/.planning/journal/retain.md`.
- Unlocks: IDEAS.md [OBJECT_ARCHIVE_TIER] — regulatory-class objects age to Glacier-tier pricing automatically, DSAR export over archived subjects restoring on demand.
- Anchors: `.api/aws-sdk-client-s3.md` archive/query command row, `StorageClass` enum, `InvalidObjectState` fault; `object/store.md` `_lifecycle` generator and conformance table.
- Tension: restore is asynchronous — a typed deferral with a poll coordinate, never a blocking read; engines refusing archive classes narrow by conformance row.

[SSE_MODALITY_ROW]-[QUEUED]: SSE egress modality lands on the live bound — realizes `[LIVE_SSE_CHANNEL]`.
- Capability: `Live.Bound` grows an `sse` projection encoding `changes` emissions into `Sse.Event` frames through `Sse.makeChannel`/`Sse.encoder`, event `id` carrying the emission coordinate for `Last-Event-ID` resume off the mailbox twin.
- Shape: one modality row on `libs/typescript/data/.planning/read/live.md` `LIVE_READS`.
- Unlocks: IDEAS.md [LIVE_SSE_CHANNEL] — browser live views over plain HTTP with zero socket infrastructure and resumable change feeds.
- Anchors: `libs/typescript/.api/effect-experimental.md` `Sse` codec rows; `read/live.md` `Live.of` bound surface.
- Tension: encode here, route and connection lifecycle in runtime.
- Atomic: one modality row over the existing bound.

[PARQUET_CODEC_ROW]-[QUEUED]: Parquet codec row joins the Arrow wire — Table-to-Parquet both directions without an engine.
- Capability: `parquet-wasm` round-trips `apache-arrow` Tables to Parquet bytes in node and browser, so the wasm arm writes lake-format objects straight to the object plane and a Parquet object decodes to a Table without instantiating DuckDB.
- Shape: a codec row on `libs/typescript/data/.planning/lane/olap.md` `ARROW_WIRE` beside `_wire.decode`/`_wire.encode`, egress landing through the object plane's conditional put.
- Unlocks: Tables round-trip to lake-format Parquet objects in node and browser with no engine, so the object plane writes and reads Parquet without instantiating DuckDB.
- Anchors: `lane/olap.md` one-wire law and `_wire` family; `object/store.md` content-addressed put; `.api/apache-arrow.md` Table interchange.
- Tension: the codec is interchange, never a query engine — querying stays with the engine rows; `parquet-wasm` admission rides the serialized admission lane.

[AUDIT_SATISFACTION_ROWS]-[QUEUED]: AuditJournal satisfaction rows land on the journal and retain pages — realizes `[AUDIT_JOURNAL_SATISFACTION]`.
- Capability: a port-satisfaction row mapping `AuditJournal.append` onto the one atomic write, the audit retention class joining the policy table, sealed subject-field wiring through the `Shredder` algebra, and the DSAR fold widened over audit facts.
- Shape: rows on `libs/typescript/data/.planning/journal/append.md` and `libs/typescript/data/.planning/journal/retain.md`.
- Unlocks: IDEAS.md [AUDIT_JOURNAL_SATISFACTION] — compliance export, session forensics, and the security board pack read one durable audit plane, subject erasure shredding audit payloads without breaking the append-only log.
- Anchors: security `access/audit.md` port shape (carded); `retain.md` `SubjectKey`/`WrappedKey` folds; `append.md` publish transaction.
- Tension: append-only law holds — no update or delete verb enters the satisfaction; erasure is key destruction, never row mutation.

[BUDGET_SCHEDULE_COMPOSE]-[QUEUED]: Lane retries compose the core budget owner — four hand-spelled schedule chains collapse.
- Capability: retry cadence is a core-compiled budget schedule at every data site, so transient-fault policy is one vocabulary and a cadence change is a row edit, never four page edits.
- Shape: the `_RETRY` chains in `libs/typescript/data/.planning/journal/fact.md`, `read/fold.md`, and `object/store.md` and the `_GOVERNOR.retry` row in `lane/olap.md` swap to composed `Budget` schedules.
- Unlocks: one retry vocabulary branch-wide — the runtime pages already hold this bar with `Budget.schedule` as the single spelling.
- Anchors: core `value/fault.md` `Budget` schedule compiler; the four cited retry fences; runtime's composed sites as precedent.
- Atomic: four schedule swaps.

[STREAM_IDENTITY_FRAGMENT]-[QUEUED]: Stream identity gets one owned SQL fragment — the composed `app:tenant:aggregate` spelling stops living twice.
- Capability: the stream-identity composition is one owned fragment the advisory-lock hash and the head resolver both read, so a separator or column change cannot desync the lock key from the resolver key.
- Shape: an owned fragment beside `StreamKey` in `libs/typescript/data/.planning/journal/append.md`; `read/query.md`'s head resolver composes it.
- Unlocks: lock hash and resolver key provably agree; a third consumer composes, never re-spells.
- Anchors: `append.md` advisory-lock `hashtextextended` composition; `query.md` identical raw spelling; the `StreamKey` owner.
- Atomic: one fragment mint and two composition swaps.

[FAULT_CLASS_CONFORMANCE]-[QUEUED]: Data fault families carry the core class field the branch fault ruling demands.
- Capability: every data folder fault family derives the core `FaultClass` kind from its reason vocabulary, so the serving edge's governed fold prices data faults structurally like every sibling folder's.
- Shape: `class` derivation on the folder fault families — `journal/append.md` `JournalFault`, `object/store.md` `ObjectFault`, and their lane/read siblings — matching the runtime reason-to-class pattern.
- Unlocks: the branch fault ruling holds with zero exceptions; the serve `Problem` ladder reads data faults off the structural field.
- Anchors: `libs/typescript/.planning/RULINGS.md` `[01]-[SHAPE]` fault row; the runtime folder families' class-derivation pattern; `append.md` `JournalFault` (reason/stream/detail, no class field).

[CACHE_CENSUS_SAMPLING]-[BLOCKED]: Pool, OLAP, and relay instruments landed; cache sampling awaits a catalog-proven census member.
- Capability: `poolHeld`, `olapWait`, `olapRetried`, `olapDeferred`, and `Journal.census` remain settled on their owner brackets.
- Shape: `lane/cache.md` terminal `[RESEARCH]` excludes the former `_observed` fence until hit, miss, and size evidence has an exact substrate declaration.
- Unlocks: `IDEAS.md` `[LANE_INSTRUMENT_PROJECTION]` — cache sampling completes the lane instrument plane.
- Anchors: `lane/cache.md` `[05]-[POOLS]`; `lane/olap.md` governor and quota brackets; `journal/append.md` `Journal.census`.
- Arms: `libs/typescript/.api/effect.md` declares the exact `Cache.Cache` census member and return type.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SIGNAL_SITE_CONFORMANCE]-[COMPLETE]: signal-site conformance — `read/fold.md` checkpoint gauge re-keyed to `Convention.metric.laneCheckpoint` tagged `rasm.lane.name`; `read/batch.md` gained the `rasm.batch.duration` histogram on the timing bracket; `journal/append.md` gained `Journal.census`, the outbox probe the runtime meter bridge samples.
[OLAP_PROFILE_PERMIT]-[COMPLETE]: `lane/olap.md` `[06]-[PROFILE]` `_profile` holds one session permit across enable, `EXPLAIN ANALYZE`, and teardown, requires root latency and rows, projects `Pg.Profile`, and taps `profileDuration`.
[OLAP_ESCALATION_PROBE]-[COMPLETE]: `lane/olap.md` `_probe`/`_armed` — bounded serial runs fold into `Olap.Evidence`; the p50 ratio arms `Olap.Escalation` against `_engines[engine].trigger`, and the verdict fans `laneEscalate` at the maintenance seam.
[OBJECT_INSTRUMENT_ROWS]-[COMPLETE]: `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` and `object/stream.md` `_streamed` landed over receipt owners; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[PG_PROFILE_HARVEST]-[COMPLETE]: `lane/postgres.md` `[06]-[PROFILE_HARVEST]` — the `pg_stat_statements` core row in `_rows`, `_statements`/`_delta` window-delta receipts keyed by `queryid`, and the `_explain` json harvest over a spliced `Fragment`.
[SQLITE_PROFILE_HARVEST]-[COMPLETE]: `lane/sqlite.md` `[05]-[PROFILE_HARVEST]` — `_harvest` availability rows, timed `_profiled` with plan, page, and probed `dbstat` counters; `stmtStatus` recorded `none` on every profile (no admitted driver reaches the `sqlite3_stmt_status` C counters), superseding the card's counter claim.
[JOURNAL_RELAY_ENVELOPE]-[COMPLETE]: `journal/append.md` `[07]-[RELAY_ROWS]` `_envelope`/`Journal.envelope` — strict-validated `CloudEvent` with encoded source components, `rasmtenant`, and W3C trace extensions, verified against `libs/typescript/core/.api/cloudevents.md`; `runtime/ARCHITECTURE.md` `Data e20` mirrors the shape.
[JOURNAL_HOOK_POINTS]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` — the closed four-point vocabulary with veto-legality derivation and app-scoped registry; `Hook.gated`/`tapped` seams landed across append publish, `object/stream.md` tus create/finalize, `object/file.md` gated intake, and `journal/retain.md` erase tombstone.
