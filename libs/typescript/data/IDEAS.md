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

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows land as read-side query-store relations for transport and visualization.
- Capability: Wire-carried layer and relation keys decode into `Model.Class` relations — layer identity, layer-path nesting, membership, and per-viewport overrides as decoded rows — so the read side serves host organization to transport and visualization consumers keyed by the one `ContentKey`, with no host handle.
- Shape: `libs/typescript/data/.planning/read/query.md` gains the boundary decoder folding the detached fact rows into the read side's projection tables; `SqlSchema` typed reads and `SqlResolver` batched loaders serve layer organization over `Query.table`, the object and journal planes carry the rows across runtimes under the one `ContentKey`, the decoded relations feed the layer-visualization surface, and the projection lane binds in `libs/typescript/data/.planning/read/fold.md`.
- Unlocks: Host-organized read-side queries, cross-runtime layer transport, and a visualization-ready organizational axis every peer reads by content identity.
- Anchors: `read/query.md` `Model.Class`/`SqlSchema`/`SqlResolver`/`Query.table`; the one `ContentKey` content-identity wire; `README.md` durable-persistence plane and the bit-identical content-identity demand across wire peers.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the query-store relations carry only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[HOST_OPLOG_CRDT_CONSUMER]-[QUEUED]: Host op-log entries decode, replay, and merge against the journal plane — the TypeScript end of the shared op-log CRDT wire owner.
- Capability: `OperationId`-keyed causal entries decode at the boundary and replay through the journal's one write owner, so cross-runtime sync, collaborative merge, and checkpoint replay land as journal operations keyed by the shared causal identity, with `ContentHash` payloads resolved through the object plane.
- Shape: TypeScript contract bindings admit op-log rows; replay, merge, and checkpoints fold through the journal owners.
- Unlocks: Multi-runtime document sync into the durable plane, deterministic replay for audit, and the consumer half that arms the producer's wire.
- Anchors: `journal/append.md` `Journal.publish`/`Occ`/`StreamKey`; `journal/evolve.md` upcast road for entry payload versions; `object/store.md` `ContentKey` payload custody.
- Tension: a neutral op-log contract owns schema and identity; TypeScript owns its codec binding and merge policy.
- Ripple: `libs/.planning` `[HOST_OPLOG_CRDT_PRODUCER]`.

[FOREIGN_RELATIONAL_READS]-[QUEUED]: MySQL and MSSQL enter through the existing read owner.
- Capability: Composition-root clients expose typed enterprise relations through the provider-neutral query surface.
- Shape: `read/query.md` consumes the admitted `SqlClient`; no backend lane or interop page is added.
- Unlocks: Typed enterprise reads without another relational authority.
- Anchors: Existing SQL package catalogs and `read/query.md`.
- Tension: Foreign clients remain read ingress, preserve journal authority, and never impersonate PostgreSQL grants.

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

[OBJECT_CUSTODY_GENERATION]-[QUEUED]: Object-plane custody joins the backend generation instead of standing outside it.
- Capability: bucket topology, lifecycle class, encryption custody, and retention lock become artifact rows the branch contract carries, so a generation states the whole durable surface rather than the relational half and a hydrate phase can prove object custody the same way it proves a catalog.
- Shape: object-plane rows on `lane/capability.md` `[05]-[CONTRACT]` sources, with their observation adapters beside the existing provider rows.
- Unlocks: one admission verdict covers relational and object state together, and an object plane provisioned outside the generation stops reading as compliant.
- Anchors: `object/store.md` S3-conditional store; `journal/retain.md` retention classes and crypto-shredding; `lane/capability.md` `Backend.compose` source rows.
- Tension: object custody is provider-shaped where relational artifacts are content-shaped; a bucket has no canonical byte form, so its artifact row keys on a declared custody descriptor rather than content, and that descriptor must stay identity-bearing without turning operator settings into generation inputs.

[CHANNEL_PACK_ASSEMBLY]-[QUEUED]: Packed texture planes assemble from their component channels on the raster owner, so the frozen packing orders gain a producer.
- Capability: the object plane mints a multi-component plane by folding single-component sources into declared slots under a named packing order, each absent slot filled with its channel's own neutral rather than zero, so the packing vocabulary the container gate already reads has something on this branch that writes it.
- Shape: a channel-assembly rendition row on `libs/typescript/data/.planning/object/file.md` `[04]-[DERIVATIVE_ROWS]`, admitting sibling source keys beside the fan-out source and terminating in the same content-addressed mint.
- Unlocks: `IDEAS.md [CHANNEL_PACK_ASSEMBLY]` — a packed plane reaches the `ktx` encode rows as one input file, so the glTF read order the wire declares crosses to a viewer without a peer branch minting the bytes first.
- Anchors: `.api/sharp.md` `[CHANNEL_FOLD]` `joinChannel`/`extractChannel`/`bandbool`; `object/asset.md` `[03]` swizzle columns, which reorder ONE input's channels and cannot gather across sources; `object/file.md` `[05]` decode-once clone-N fold.
- Tension: the derivative spine opens exactly one verified source, so a multi-source rendition either fetches its siblings inside the engine's emit — the shape the `ktx` engine already proved — or the spine gains a plural-source open that every single-source engine then carries; the row belongs where it costs the spine nothing.

[ASSET_UNWRAP_ROW]-[BLOCKED]: UV-atlas generation joins the container transform vocabulary as one row.
- Capability: served containers gain a generated UV atlas — texture-space bake targets and lightmap-ready second channels — through the same one-fold optimization vocabulary, the atlas codec injected like every other engine instance.
- Shape: one `_TRANSFORMS` row on `libs/typescript/data/.planning/object/asset.md` `[03]-[TRANSFORM_ROWS]` minting `unwrap` with its injected `watlas` instance.
- Unlocks: mesh-space texture products for the C# chart-atlas bake counterpart and lightmap channels with zero new surface — the roster growth law is the acceptance contract.
- Anchors: `.api/gltf-transform-functions.md` `unwrap`/`UnwrapOptions` injected-codec row; `object/asset.md` `[03]` roster growth law.
- Arms: `watlas` admitted in `pnpm-workspace.yaml` with its `.api` catalogue row landed.

[GENERATION_RECOVERY_CONTRACT]-[QUEUED]: Restored lanes admit on evidence — the contract owner grades recovery instead of trusting the store it opened.
- Capability: the contract admission verdict widens to a restored store — recovered generation identity, the frontier instant the restore reached, and the objective the composition root declares grade together, so a promoted replica, a point-in-time restore, and a rebuilt embedded lane resolve on one verdict; the branch mints its recovery evidence from its own lanes and reads no peer's runbook.
- Shape: recovery observation and verdict rows on `libs/typescript/data/.planning/lane/capability.md` `[05]-[CONTRACT]`, sourced from the lane owners that already carry restore mechanics — `lane/sqlite.md` embedded rebuild and `journal/evolve.md` generation succession.
- Unlocks: a TypeScript-only application restores and admits with no peer present, and the merged-generation restore in a polyglot root reads one verdict shape at every branch.
- Anchors: `lane/capability.md` `Backend.observe`/`Backend.admit` join and its `_Check` invariant rows; `journal/retain.md` retention classes bounding the reachable window; `tests/contracts/MANIFEST.md` `BACKEND_CONTRACT`.
- Tension: contract identity is content-shaped where recovery evidence is time-shaped — a restore lands a valid generation whose data frontier trails it, so the verdict carries both facts without minting a second generation notion.
- Ripple: mirrors `libs` `[GENERATION_RECOVERY_CONTRACT]`, the cross-libs origin carding the corpus-schema rows; peer counterpart `python:runtime` `[GENERATION_RECOVERY_CONTRACT]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[QUERY_PROFILE_RECEIPT_BAND]-[COMPLETE]: the browser arm landed and its blocker proved false — `conn.query` answers `Promise<arrow.Table>`, `Table.getChild(name)` selects the `explain_value` column by NAME with no positional column assumption, and `Vector.get(index)` reads a row off that selected column; `lane/olap.md` `_ROWED` normalizes both bounded grains and `lane/postgres.md` `_PROFILE_ENGINES` admits `duckdbWasm`.
[AUDIT_JOURNAL_SATISFACTION]-[COMPLETE]: `journal/fact.md` `[05]-[RAIL]` landed `Fact.audits` — the security `AuditJournal` port satisfied by projecting each `AuditRecord` through the `_AUDITED` row table onto one `Fact.AuditDraft`, `action` derived from the registry point and `retention` from the point's lane class, subject-bearing fields sealed under `Retain.seal` before the draft leaves and `AuditFact` widened with the `subject`/`sealed` erasure pair; `fact_journal` gained its partial subject index and `retain.md` `[04]-[DSAR_EXPORT]` gained the fact leg, so export and erasure answer one custody coordinate across all three planes.
[LANE_INSTRUMENT_PROJECTION]-[COMPLETE]: superseded by `[CACHE_CENSUS_SAMPLING]` — `lane/cache.md` `[05]-[POOLS]` carries `CacheLane.census(name, cache)` off the substrate's own `cacheStats` snapshot, so pool, OLAP, outbox, and cache projections all read one instrument plane and the arming catalog row landed at `libs/typescript/.api/effect.md`.
[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[COMPLETE]: object-plane instrument rows landed — `object/store.md` `[05]-[INSTRUMENT_ROWS]` `_measured`/`_reclaimed` off the receipt and sweep-mark folds, `object/stream.md` `_streamed` after durable re-home, reference commit, and staging retirement; core `convention.md` `[03]-[RASM_ROWS]` owns the exact vocabulary.
[RELAY_CLOUDEVENTS_PROJECTION]-[COMPLETE]: `journal/append.md` `[07]-[RELAY_ROWS]` `_envelope` landed as `Journal.envelope` — strict-validated `CloudEvent` with component-encoded source coordinates, `rasmtenant`, and W3C trace extensions, verified against `libs/typescript/core/.api/cloudevents.md`; `runtime/ARCHITECTURE.md` `Data e20` mirrors the shape.
[DATA_HOOK_TAP_REGISTRY]-[COMPLETE]: `journal/append.md` `[08]-[HOOK_POINTS]` landed the closed four-point registry with veto/observe fan and app-scoped Layer factory; taps armed at `object/stream.md` tus create/finalize, `object/file.md` gated intake, `journal/retain.md` erase tombstone, and the `lane/olap.md` escalation composition seam.
