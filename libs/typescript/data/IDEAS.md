# [TS_DATA_IDEAS]

Forward pool of higher-order `data` concepts grounded in the durable-persistence domain; an idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
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

[RELATIONAL_SET_COMPLETION]-[QUEUED]: Effect-sql store family completes — pglite joins the pg lane as its in-process profile, mysql2 and mssql land as foreign-relational ingress rows.
- Capability: `@effect/sql-pglite` runs the true pg contract in-process (WASM, zero daemon) as a pg-lane profile row for browser and edge arms; `@effect/sql-mysql2` and `@effect/sql-mssql` open read-oriented interop lanes into enterprise-held MySQL/SQL-Server data — the `sql.onDialect` union already carries `mysql`/`mssql` arms no lane exploits, so the dialect algebra is pre-paid.
- Shape: One new page `libs/typescript/data/.planning/lane/interop.md` owns the foreign-relational ingress rows — guarantee pricing, capability degradation against the `Pg.Grant` vocabulary, never a record of truth — and the pglite profile row lands beside the driver mints in `libs/typescript/data/.planning/lane/postgres.md`.
- Unlocks: Browser-resident pg semantics without the sqlite degradation table, enterprise data ingress for AEC apps whose estate data lives in MSSQL/MySQL, and closure of the effect-sql client family the manifest holds partially.
- Anchors: `.api/effect-sql.md` five-way `Dialect` discriminant with `mysql`/`mssql` arm-keys; `lane/sqlite.md` `_degrades` as the degradation-pricing template; `README.md` law that a backend enters as a semantic-guarantee row on its owning lane; the read-only-interop ruling at `libs/typescript/data/RULINGS.md` `[01]-[PACKAGES]`; `@electric-sql/pglite` already vetted in the test cluster.
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

[AUDIT_JOURNAL_SATISFACTION]-[QUEUED]: Security `AuditJournal` port lands on the durable plane — append-only audit-fact store with retention rows and per-subject crypto-shred integration.
- Capability: Security-published fact rows persist through an append-only port satisfaction keyed by the standing `(app, tenant, subject)` custody spine — retention classes from the one policy table, subject-bearing fields sealed under the `SealedEnvelope`/`WrappedKey` algebra the retain page already composes, DSAR export and erasure riding the same subject spine — so breach evidence is durable receipt-truth aging under the same law as every journal fact.
- Shape: An `AuditJournal` port-satisfaction row in `libs/typescript/data/.planning/journal/append.md` beside the existing port grammar, with the audit retention-class row and crypto-shred wiring in `libs/typescript/data/.planning/journal/retain.md`.
- Unlocks: Compliance export, session forensics, and the security board pack read one durable audit plane; erasing a subject shreds audit payloads without breaking the append-only log.
- Anchors: security `access/audit.md` `AuditJournal` port (carded); `journal/retain.md` `SubjectKey` custody and `WrappedKey` erasure folds; `journal/append.md` publish transaction; `ARCHITECTURE.md` `[SHAPE]: SealedEnvelope` seam.
- Tension: Audit facts are security's mint — this plane persists and ages them, never re-derives or reinterprets a fact.
- Ripple: `security` `[SECURITY_FACT_RAIL]`.

[QUERY_PROFILE_RECEIPT_BAND]-[BLOCKED]: Admitted pg, SQLite, and DuckDB-node harvests share `Pg.Profile`; wasm and ClickHouse parity awaits exact catalog contracts.
- Capability: `_statements`/`_delta`/`_explain`, `_profiled`, and `_profile` emit one receipt shape without zero-forged evidence or interleaved session toggles.
- Shape: `_PROFILE_ENGINES` contains only landed arms; `lane/olap.md` `[RESEARCH]` owns the wasm cell-projection and ClickHouse query-id questions.
- Unlocks: pg, SQLite, and DuckDB-node harvests read one comparable profile-receipt band, query-cost evidence never a per-engine forgery or interleaved session toggle.
- Anchors: `lane/postgres.md` `[06]-[PROFILE_HARVEST]`; `lane/sqlite.md` `[05]-[PROFILE_HARVEST]`; `lane/olap.md` `[06]-[PROFILE]` and `[08]-[RESEARCH]`.
- Arms: `.api/duckdb-duckdb-wasm.md` and `.api/apache-arrow.md` declare the exact profile-cell projection and `.api/effect-sql-clickhouse.md` declares query-id scope and result contract.

[LANE_INSTRUMENT_PROJECTION]-[BLOCKED]: Pool, OLAP, and outbox projections landed; cache census projection awaits its exact substrate member.
- Capability: `_origins` projects pool leases, OLAP governor and quota brackets project wait/retry/defer, and `Journal.census` exposes outbox truth.
- Shape: `lane/cache.md` terminal `[RESEARCH]` keeps cache hit, miss, and size gauges out of settled code until their evidence read is catalog-proven.
- Unlocks: cache census joins the settled pool, OLAP, and outbox projections, lane health reading one instrument plane.
- Anchors: `lane/cache.md` `[05]-[POOLS]`; `lane/olap.md` `_waited`/`_retried`/`_deferred`; `journal/append.md` `Journal.census`.
- Arms: `libs/typescript/.api/effect.md` declares the exact `Cache.Cache` census member and return type.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[COMPLETE]: object-plane instrument rows landed — `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` off the receipt and sweep-mark folds, `object/stream.md` `_streamed` after durable re-home, reference commit, and staging retirement; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[RELAY_CLOUDEVENTS_PROJECTION]-[COMPLETE]: `journal/append.md` `[07]-[RELAY_ROWS]` `_envelope` landed as `Journal.envelope` — strict-validated `CloudEvent` with component-encoded source coordinates, `rasmtenant`, and W3C trace extensions, verified against `libs/typescript/core/.api/cloudevents.md`; `runtime/ARCHITECTURE.md` `Data e20` mirrors the shape.
[DATA_HOOK_TAP_REGISTRY]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` landed the closed four-point registry with veto/observe fan and app-scoped Layer factory; taps armed at `object/stream.md` tus create/finalize, `object/file.md` gated intake, `journal/retain.md` erase tombstone, and the `lane/olap.md` escalation composition seam.
