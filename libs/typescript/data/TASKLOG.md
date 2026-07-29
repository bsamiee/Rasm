# [TS_DATA_TASKLOG]

Open and closed `data` work distilled from `IDEAS.md`; each task names the exact sub-domain or file it lands in.

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

[CHANNEL_PACK_ROW]-[QUEUED]: Raster owners gain the channel-assembly rendition that gives the frozen packing orders a producer on this branch.
- Capability: a rendition row gathering component sources into declared slots under a named packing order, filling an absent slot with its channel's neutral and terminating in the same content-addressed mint every other rendition takes.
- Shape: the assembly row and its slot vocabulary on `libs/typescript/data/.planning/object/file.md` `[04]-[DERIVATIVE_ROWS]`, with the sibling fetch riding the engine's own emit exactly as the `ktx` engine's extra inputs do.
- Unlocks: IDEAS.md [CHANNEL_PACK_ASSEMBLY] — a packed plane reaches the `ktx` encode rows as one staged input, so the glTF occlusion-metallic-roughness read order crosses to a viewer without a peer branch minting the bytes.
- Anchors: `.api/sharp.md` `[CHANNEL_FOLD]` `joinChannel`/`extractChannel`; `object/asset.md` `[04]` sibling-source fetch inside emit; `object/file.md` `[05]` decode-once clone-N fold.
- Ripple: precedes `object/asset.md` `[03]` swizzle columns, which reorder one input and never gather across sources.

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
- Tension: the neutral op-log contract owns identity; TypeScript owns local encoding, decoding, and merge.

[FOREIGN_RELATIONAL_READS]-[QUEUED]: Existing query ownership admits MySQL and MSSQL clients.
- Capability: Composition-root clients feed provider-neutral typed reads.
- Shape: `read/query.md` gains client admission and query rows; no new page or lane owner.
- Unlocks: IDEAS.md [FOREIGN_RELATIONAL_READS].
- Anchors: Existing SQL package catalogs and `read/query.md`.
- Tension: Foreign clients remain read ingress, preserve journal authority, and never impersonate PostgreSQL grants.

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
- Anchors: `libs/typescript/.planning/RULINGS.md` `[02]-[SHAPE]` fault row; `object/asset.md` `AssetFault` — the in-folder `FaultClass.family` instance the siblings converge on; `append.md` `JournalFault` (reason/stream/detail, no class field).

[RESIDENCE_DISTRIBUTION_FOLD]-[QUEUED]: Bucket relations answer the quantile and fraction cases from their own bucket columns.
- Capability: a metric residence relating histograms as bucket columns renders the distribution cases off `ExplicitBounds` and `BucketCounts`, so a latency objective reads the same value on the residence plane it reads on the metrics store rather than degrading to a scalar the relation only approximates.
- Shape: two row functions on the metric record in `libs/typescript/data/.planning/lane/olap.md` `[04]-[RESIDENCE_ROWS]`, projected onto `Query.Residence` and read by the `Quantile` and `Fraction` arms of `libs/typescript/core/.planning/observe/board.md` `_leaf`.
- Unlocks: the burn panel and the objective query render identically against a store and a residence, closing the last case where a target swap changes the number an operator reads.
- Anchors: `olap.md` `_POINTS.histogram` columns and the `series`/`access` row-function precedent; `board.md` `_leaf` `Quantile`/`Fraction` arms and the `_ENGINES.quantile`/`.share` scalar folds.
- Tension: bucket interpolation is the residence engine's dialect where the scalar fold is the engine roster's, so the interpolation rides the residence row and the arm selects between them on the metric kind alone.
- Ripple: `core` `[RESIDENCE_KIND_SCALAR]` landed the per-kind scalar this case narrows.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ASSET_ENGINE_PLANES]-[COMPLETE]: `object/asset.md` landed the category-general asset plane — categories as rows, the GPU container/ktx family the first row family, `Asset.gate` dispatching on the declaration's own `category` tag through the `_GATES` record — over six frozen tables — `_PAYLOADS` (the `rawBcn` residue row absorbing every unrostered `colorModel`, the `wire` column deriving the declarable subset), `_TRANSFERS` and `_PRIMARIES` (each transfer tag naming the working space its DFD must carry, so `linear` is AP1 and a `raw` parameter plane declares no chromaticity), `_STORES` (the `block` column making the measured 8-bit encode bound a TYPE and the deep rows carrying no `srgb` target), `_LAYERS` (`faceCount`/`layerCount`/`pixelDepth` proven against the declared law, and the texcoord origin `--cubemap` pins riding the row), `_MIPS` (the resampling kernel each policy spells, and the `folds` column refusing the two policies the tool cannot express) — with `Asset.gate` proving payload, `vkFormat`-against-store, transfer, primaries, alpha, layer shape, RAW `levelCount` (so the loader-generated pyramid and the block-payload prohibition on it both read), and extent. `_TRANSFORMS` rows carry each package option record, so `prune`/`dedup`/`quantize`/`weld`/`join`/`flatten`/`palette`/`reorder`/`meshopt`/`simplify` state policy per step; `_KTX` is the peer table over the tool's whole subcommand family — `create` (mipmap posture, swizzle, assign/convert color and origin under `--fail-on-color-conversions`), `encode`, `transcode` (the only `rawBcn` producer), `deflate` (the only sanctioned repack), `extract`, and `validate` — each row taking its own option record and answering the `Asset.Spawn` value whose flag fold defers to the staged input class. Every product proves itself before the store sees a byte: a container re-proves its extension vocabulary after the fold, a KTX2 runs the Khronos validator then the declaration gate. `_ENGINES` is the governed engine table with its namespace guard pair, so `Asset.pipe` is kind-blind and a third engine adds no branch; `assetTransformed` partitions on engine AND outcome with a boot refusal routed by its own fault class. `object/file.md` `[05]` gained `Derive.Plane` with plane-first `Derive.fanout`, so raster, container, and `ktx` engines share one spine, receipts, cascade, and grants; `AssetFault` closes through `FaultClass.family`.
[LAKE_RELATION_DDL]-[COMPLETE]: `lane/olap.md` `[04]` landed the lake DDL — `_D`/`_METRIC_HEAD`/`_POINTS`/`_WIDE` carry every column, `Olap.mount` attaches and creates the two wide-event and five metric relations in one idempotent statement, `plant` sits beside `absorb`, and `Olap.recorded` folds `DashboardModel.snapshot` into the point relations. All three arms proved false: `Olap.lake.sink` already mints the Parquet, the metric roster is the OTLP point model this page owns, and the demanded type map translates ClickHouse INTO DuckDB.
[FACT_JOURNAL_RLS_ROW]-[COMPLETE]: `journal/fact.md` `_factDdl.pg` interpolates `${Tenancy.rls("fact_journal")}` beside its indices, so every tenant-carrying relation registers structurally and the law line states the registration rather than gating it on scope.
[AUDIT_SATISFACTION_ROWS]-[COMPLETE]: satisfaction landed at `journal/fact.md`, not `append.md` — the fact plane already owns the append-only `AuditFact` stream with its retention class, so `Fact.audits` maps `AuditJournal.append` onto the one rail, `AuditFact` gained the `subject`/`sealed` erasure pair with its partial index, and `retain.md` `_dsar` gained the fact leg.
[CACHE_CENSUS_SAMPLING]-[COMPLETE]: `Cache.ConsumerCache.cacheStats` proved on the rail as the substrate's own cumulative `{ hits, misses, size }` snapshot, so `lane/cache.md` landed `CacheLane.census(name, cache)` — one scoped repeating probe SETTING the `cacheHits`/`cacheMisses` levels tagged by the caller's cache name, instrumenting no lookup and keeping no tally beside the cache; the `board#PACKS` `lake` hit-share tile now reads a series a producer genuinely mints, and the lane instrument plane closes.
[SIGNAL_SITE_CONFORMANCE]-[COMPLETE]: signal-site conformance — `read/fold.md` checkpoint gauge re-keyed to `Convention.metric.laneCheckpoint` tagged `rasm.lane.name`; `read/batch.md` gained the `rasm.batch.duration` histogram on the timing bracket; `journal/append.md` gained `Journal.census`, the outbox probe the runtime meter bridge samples.
[PARQUET_CODEC_ROW]-[COMPLETE]: `lane/olap.md` `[08]-[ARROW_WIRE]` landed `Olap.lake` — `read`/`schema` decode through `intoIPCStream`, `batches` streams `ParquetFile` range reads, `write` folds one `_PARQUET` policy row through `writeParquet`, and `sink` weights an Arrow batch feed by rows into one object per row group for `object/store.md`'s conditional put; every composed member consumes its own handle, so `ParquetFile` alone brackets.
[EMBEDDED_SESSION_COLLAPSE]-[COMPLETE]: `lane/olap.md` `[03]-[EMBEDDED]` collapsed both DuckDB drivers onto one `_DRIVERS` row family — `Olap.wasm` mints the same `Olap.Handle` `Olap.node` does, so the browser lane leases, gates, budgets, replays, and meters exactly like the node lane, binds through `prepare` instead of splicing, and `_wire.pull` retired into the `Drain` case.
[FLIGHT_SQL_INGRESS_ROW]-[COMPLETE]: `lane/olap.md` `[06]-[FLIGHT]` landed the engine-blind wire — scoped `createFlightSqlClient` off `Olap.Flight` with `Redacted` auth custody, `Olap.flown` dispatching one closed intent family across two mapped halves, the endpoint fan `FlightInfo.ordered` widths, `doPut` over the server-echoed descriptor, and the `FlightError` family folded onto the existing `OlapFault` reasons; every codec sits at `Olap.wire.flight`.
[OLAP_PROFILE_PERMIT]-[COMPLETE]: `lane/olap.md` `[07]-[PROFILE]` `_profile` holds one session permit across enable, `EXPLAIN ANALYZE`, and teardown, requires root latency and rows, projects `Pg.Profile`, and taps `profileDuration`.
[OLAP_ESCALATION_PROBE]-[COMPLETE]: `lane/olap.md` `_probe`/`_armed` — bounded serial runs fold into `Olap.Evidence` off the handle's own engine; the p50 ratio arms `Olap.Escalation` against the CANDIDATE row's trigger, and the verdict fans `laneEscalate` at the maintenance seam.
[OBJECT_INSTRUMENT_ROWS]-[COMPLETE]: `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` and `object/stream.md` `_streamed` landed over receipt owners; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[PG_PROFILE_HARVEST]-[COMPLETE]: `lane/postgres.md` `[06]-[PROFILE_HARVEST]` — the `pg_stat_statements` core row in `_rows`, `_statements`/`_delta` window-delta receipts keyed by `queryid`, and the `_explain` json harvest over a spliced `Fragment`.
[SQLITE_PROFILE_HARVEST]-[COMPLETE]: `lane/sqlite.md` `[05]-[PROFILE_HARVEST]` — `_harvest` availability rows, timed `_profiled` with plan, page, and probed `dbstat` counters; `stmtStatus` recorded `none` on every profile (no admitted driver reaches the `sqlite3_stmt_status` C counters), superseding the card's counter claim.
[JOURNAL_RELAY_ENVELOPE]-[COMPLETE]: `journal/append.md` `[07]-[RELAY_ROWS]` `_envelope`/`Journal.envelope` — strict-validated `CloudEvent` with encoded source components, `rasmtenant`, and W3C trace extensions, verified against `libs/typescript/core/.api/cloudevents.md`; `runtime/ARCHITECTURE.md` `Data e20` mirrors the shape.
[JOURNAL_HOOK_POINTS]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` — the closed four-point vocabulary with veto-legality derivation and app-scoped registry; `Hook.gated`/`tapped` seams landed across append publish, `object/stream.md` tus create/finalize, `object/file.md` gated intake, and `journal/retain.md` erase tombstone.
