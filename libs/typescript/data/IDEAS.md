# [TS_DATA_IDEAS]

Forward pool of higher-order `data` concepts grounded in the durable-persistence domain; an idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[GENERATION_RECOVERY_CONTRACT]-[COMPLETE]: landed at `lane/capability#CONTRACT` as `Backend.Objective`/`Window`/`Reading`/`Observation`, `_window` deriving off the observation's own stamps, `_exceeding` carrying the opposite absence polarity per half, and the `recovery` `_Check` seated after the realization rows. Wire shapes untouched, so no peer decode ripples.
[HOST_OPLOG_CRDT_CONSUMER]-[COMPLETE]: decode lands on `journal/append#ATOMIC_PUBLISH`, lifting through the same `spec.plan.decode` the windowed read runs, so a producer-side schema move rides the one upcast chain. `state/causal` already separated the dot from the content digest and needed only its encode sorted by replica.
[LAYER_TOPOLOGY_GRAPH_FACTS]-[COMPLETE]: landed as `read/query#ORGANIZATION_ROWS` — the `Organization.Entity` relation with its `organization_member`/`organization_view` edge relations and four grouped resolvers — beside the `read/fold#LANE_SPEC` `Lane.Organization` fold. Rows decode from the core `Wire.Organization` landing and this plane mints nothing.
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[FOREIGN_RELATIONAL_READS]-[COMPLETE]: landed as the discriminated `Query.table` spec at `read/query#TABLE_BINDING` — the `Ingress` case carries the explicit foreign client Tag and yields reads/resolvers only (no repository, no loaders, no `ensure`), so read-only ingress is structural per the folder ruling; mysql2/mssql/effect-sql catalogs repaired against the installed tree.
[OBJECT_CUSTODY_GENERATION]-[COMPLETE]: `object/store#CUSTODY_CONTRACT` carries the operator-free custody descriptor (conditional, versioning, lifecycle, reap, SSE mode) with `_observed` adapters, and `Backend.Reading` gained the canonical `granted` slot unioned at `Backend.observe`, so one `admit` verdict covers relational and object custody; retention lock refused by the Object Lock ruling.
[LEGAL_HOLD_SUSPENSION]-[COMPLETE]: the `legal_hold` ledger, `_holding` predicate pair, both groom renderings, and the erase refusal stand; the third-ender demand REFUTED — a hold suspends an ender and is not one; held objects freeze under the `held` tag posture, and preservation is collection at declaration through the `Preserve` port, so partition drops stay hold-blind by law.
[LANE_SOURCE_AS_RELATION]-[COMPLETE]: landed as `_SOURCES` route rows at `lane/olap#EMBEDDED` — `scan` rows (flight, objects) as handle-registered pre-pumped table functions under mandatory projection pushdown and exact cardinality, `sql` rows (journal, lake) riding the strictly stronger ATTACH/`read_parquet` routes; the re-register-per-lease premise refuted on the live rail (instance-scoped, silent no-op).
[CHANNEL_PACK_ASSEMBLY]-[COMPLETE]: packed planes assemble positionally on `object/file.md`'s raster owner — `Derive.Assembly` where position IS the slot, so the data plane spells no role and reads no foreign neutral column.
[ASSET_UNWRAP_ROW]-[COMPLETE]: `unwrap` is a `_TRANSFORMS` row taking `Omit<UnwrapOptions, "watlas">` with the initialized module injected whole, `watlas.Initialize()` proven on its own construction leg (the module publishes no `supported` flag), and the watlas/meshopt BYTES-vs-FLOAT-ELEMENTS stride divergence recorded at both catalogs.
[OBJECT_ARCHIVE_TIER]-[COMPLETE]: the cold-tier axis landed whole — retention owns the ordered depth vocabulary, the engine owner keeps the vendor storage-class spelling, restore is a typed deferral with a real poll coordinate, and archive capability narrows by conformance row; the carded `SelectObjectContentCommand` projection refuted by its own consumer slot.
[LIVE_SSE_CHANNEL]-[COMPLETE]: realized by [SSE_MODALITY_ROW] — the live bound gains the emission-identity `coordinate` projection and the serving plane's one `Realtime.sse` fold frames it; the branch keeps a single `Sse` encode seam, so browser live views over plain HTTP arrive with resumable dedupe and zero second codec site.
[QUERY_PROFILE_RECEIPT_BAND]-[COMPLETE]: the browser arm landed and its blocker proved false — `conn.query` answers `Promise<arrow.Table>`, `Table.getChild(name)` selects the `explain_value` column by NAME with no positional column assumption, and `Vector.get(index)` reads a row off that selected column; `lane/olap.md` `_ROWED` normalizes both bounded grains and `lane/postgres.md` `_PROFILE_ENGINES` admits `duckdbWasm`.
[AUDIT_JOURNAL_SATISFACTION]-[COMPLETE]: `journal/fact.md` `[05]-[RAIL]` landed `Fact.audits` satisfying the security `AuditJournal` port on the one fact rail, with the `subject`/`sealed` erasure pair and the `retain.md` DSAR fact leg, so export and erasure answer one custody coordinate across all three planes.
[LANE_INSTRUMENT_PROJECTION]-[COMPLETE]: superseded by `[CACHE_CENSUS_SAMPLING]` — `lane/cache.md` `[05]-[POOLS]` carries `CacheLane.census(name, cache)` off the substrate's own `cacheStats` snapshot, so pool, OLAP, outbox, and cache projections all read one instrument plane and the arming catalog row landed at `libs/typescript/.api/effect.md`.
[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[COMPLETE]: object-plane instrument rows landed — `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` off the receipt and sweep-mark folds, `object/stream.md` `_streamed` after durable re-home, reference commit, and staging retirement; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[RELAY_CLOUDEVENTS_PROJECTION]-[COMPLETE]: `Journal.Deliverable.envelope` uses `Carrier.promote` to emit a complete `CloudEventV1`; tenancy travels only in W3C baggage.
[DATA_HOOK_TAP_REGISTRY]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` landed the closed four-point registry with veto/observe fan and app-scoped Layer factory; taps armed at `object/stream.md` tus create/finalize, `object/file.md` gated intake, `journal/retain.md` erase tombstone, and the `lane/olap.md` escalation composition seam.
