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

[FOREIGN_RELATIONAL_READS]-[QUEUED]: MySQL and MSSQL enter through the existing read owner.
- Capability: Composition-root clients expose typed enterprise relations through the provider-neutral query surface.
- Shape: `read/query.md` consumes the admitted `SqlClient`; no backend lane or interop page is added.
- Unlocks: Typed enterprise reads without another relational authority.
- Anchors: Existing SQL package catalogs and `read/query.md`.
- Tension: Foreign clients remain read ingress, preserve journal authority, and never impersonate PostgreSQL grants.

[OBJECT_CUSTODY_GENERATION]-[QUEUED]: Object-plane custody joins the backend generation instead of standing outside it.
- Capability: bucket topology, lifecycle class, encryption custody, and retention lock become artifact rows the branch contract carries, so a generation states the whole durable surface rather than the relational half and a hydrate phase can prove object custody the same way it proves a catalog.
- Shape: object-plane rows on `lane/capability.md` `[05]-[CONTRACT]` sources, with their observation adapters beside the existing provider rows.
- Unlocks: one admission verdict covers relational and object state together, and an object plane provisioned outside the generation stops reading as compliant.
- Anchors: `object/store.md` S3-conditional store; `journal/retain.md` retention classes and crypto-shredding; `lane/capability.md` `Backend.compose` source rows.
- Tension: object custody is provider-shaped where relational artifacts are content-shaped; a bucket has no canonical byte form, so its artifact row keys on a declared custody descriptor rather than content, and that descriptor must stay identity-bearing without turning operator settings into generation inputs.

[LEGAL_HOLD_SUSPENSION]-[QUEUED]: Declared holds outrank the retention window — evidence under litigation outlives the class that closes it.
- Capability: retention gains a suspension authority above the class window, so a preservation obligation is a custody fact the sweep reads rather than an operator's memory, and the ender vocabulary carries a third closer beside the wall-clock groom and the shredder.
- Shape: `libs/typescript/data/.planning/journal/retain.md` `[02]-[RETENTION_ROWS]` — the hold declaration beside `_Policy`, the `_GROOMS` predicate reading it, and both renderings carrying it so a scheduled maintenance statement honours the suspension the in-process sweep honours.
- Unlocks: litigation-safe operation of the durable plane — a preservation order is answered per matter instead of by pausing every sweep estate-wide, and the erasure leg states which subjects a live hold forecloses.
- Anchors: `journal/retain.md` `_Policy` rows with their `lifetime.owner` ender pair, `_GROOMS` with its `live` eligibility gate and `scope` column, `Retain.groomText` rendering the statements `read/fold#MAINTENANCE` schedules; `journal/fact.md` `Fact.audits` for the declaration trail; `journal/retain.md` `[04]-[DSAR_EXPORT]` erasure.
- Tension: two questions shape the surface and neither is settled — WHO declares a hold, since this plane admits no operator identity and a hold any writer stamps is no hold; and whether a hold SUSPENDS the sweep, leaving the class intact so lifting it ages the row out at once, or RECLASSIFIES the row, which demands the ledger carry the pre-hold class or the restore forges one.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[GENERATION_RECOVERY_CONTRACT]-[COMPLETE]: landed at `lane/capability#CONTRACT` as `Backend.Objective`/`Window`/`Reading`/`Observation`, `_window` deriving off the observation's own stamps, `_exceeding` carrying the opposite absence polarity per half, and the `recovery` `_Check` seated after the realization rows. Wire shapes untouched, so no peer decode ripples.
[HOST_OPLOG_CRDT_CONSUMER]-[COMPLETE]: decode lands on `journal/append#ATOMIC_PUBLISH`, lifting through the same `spec.plan.decode` the windowed read runs, so a producer-side schema move rides the one upcast chain. `state/causal` already separated the dot from the content digest and needed only its encode sorted by replica.
[LAYER_TOPOLOGY_GRAPH_FACTS]-[COMPLETE]: landed as `read/query#ORGANIZATION_ROWS` — the `Organization.Entity` relation with its `organization_member`/`organization_view` edge relations and four grouped resolvers — beside the `read/fold#LANE_SPEC` `Lane.Organization` fold. Rows decode from the core `Wire.Organization` landing and this plane mints nothing.
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CHANNEL_PACK_ASSEMBLY]-[COMPLETE]: packed planes assemble positionally — `Derive.Assembly = { pack, bands: [Band, Band, Band] }` on the raster owner, position IS the slot so the data plane spells no role and reads no foreign neutral column; `_banded`/`_assembled` fold source/plane/level bands through one lossless single-channel leg and the chain head wires into `_RASTER.emit`.
[ASSET_UNWRAP_ROW]-[COMPLETE]: `unwrap` is a `_TRANSFORMS` row taking `Omit<UnwrapOptions, "watlas">` with the initialized module injected whole, `watlas.Initialize()` proven on its own construction leg (the module publishes no `supported` flag), and the watlas/meshopt BYTES-vs-FLOAT-ELEMENTS stride divergence recorded at both catalogs.
[OBJECT_ARCHIVE_TIER]-[COMPLETE]: the cold-tier axis lands whole — retention owns the ordered depth vocabulary (`cool`/`cold`/`frozen`) while the engine owner keeps the vendor storage-class spelling, transitions and expiry ride one `_lifecycle` rule per class, restore is a typed deferral with a real poll coordinate, and archive capability narrows by conformance row (`tigris archive: "none"`); the carded `SelectObjectContentCommand` projection was refuted by its own consumer slot.
[LIVE_SSE_CHANNEL]-[COMPLETE]: realized by [SSE_MODALITY_ROW] — the live bound gains the emission-identity `coordinate` projection and the serving plane's one `Realtime.sse` fold frames it; the branch keeps a single `Sse` encode seam, so browser live views over plain HTTP arrive with resumable dedupe and zero second codec site.
[QUERY_PROFILE_RECEIPT_BAND]-[COMPLETE]: the browser arm landed and its blocker proved false — `conn.query` answers `Promise<arrow.Table>`, `Table.getChild(name)` selects the `explain_value` column by NAME with no positional column assumption, and `Vector.get(index)` reads a row off that selected column; `lane/olap.md` `_ROWED` normalizes both bounded grains and `lane/postgres.md` `_PROFILE_ENGINES` admits `duckdbWasm`.
[AUDIT_JOURNAL_SATISFACTION]-[COMPLETE]: `journal/fact.md` `[05]-[RAIL]` landed `Fact.audits` — the security `AuditJournal` port satisfied by projecting each `AuditRecord` through the `_AUDITED` row table onto one `Fact.AuditDraft`, `action` derived from the registry point and `retention` from the point's lane class, subject-bearing fields sealed under `Retain.seal` before the draft leaves and `AuditFact` widened with the `subject`/`sealed` erasure pair; `fact_journal` gained its partial subject index and `retain.md` `[04]-[DSAR_EXPORT]` gained the fact leg, so export and erasure answer one custody coordinate across all three planes.
[LANE_INSTRUMENT_PROJECTION]-[COMPLETE]: superseded by `[CACHE_CENSUS_SAMPLING]` — `lane/cache.md` `[05]-[POOLS]` carries `CacheLane.census(name, cache)` off the substrate's own `cacheStats` snapshot, so pool, OLAP, outbox, and cache projections all read one instrument plane and the arming catalog row landed at `libs/typescript/.api/effect.md`.
[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[COMPLETE]: object-plane instrument rows landed — `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` off the receipt and sweep-mark folds, `object/stream.md` `_streamed` after durable re-home, reference commit, and staging retirement; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[RELAY_CLOUDEVENTS_PROJECTION]-[COMPLETE]: `Journal.Deliverable.envelope` uses `Carrier.promote` to emit a complete `CloudEventV1`; tenancy travels only in W3C baggage.
[DATA_HOOK_TAP_REGISTRY]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` landed the closed four-point registry with veto/observe fan and app-scoped Layer factory; taps armed at `object/stream.md` tus create/finalize, `object/file.md` gated intake, `journal/retain.md` erase tombstone, and the `lane/olap.md` escalation composition seam.
