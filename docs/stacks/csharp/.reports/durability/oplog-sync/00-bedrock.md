# [BEDROCK] oplog-sync

## the one op-log shape

- One table serves every entity family: local sequence as the integer primary key (rowid alias — appends land at the b-tree right edge), origin (replica identity), entity-kind (a closed vocabulary column), entity id, verb (closed: upsert | tombstone), stamp, payload (sealed-codec row image), content key (codec-owned identity of the canonical payload bytes).
- The sequence column doubles as physical clustering: replay, cursor streams, and segment cuts are all right-edge or range operations — the shape is append-optimal by construction, with no covering-index tax on the hot path.
- The stamp is the already-stamped causal version packed into one 64-bit integer column so ordering is integer comparison; stamping mechanics arrive settled from the signal layer — this lane only stores and compares stamps, and a lane-local stamp generator is the rejected form.
- Storage packing law: the packed column must order identically to the stamp's own (physical, logical) order — fixed bit allocation, high bits physical — so the store's integer comparison and the stamp algebra can never disagree; a packing that breaks order equivalence is a silent adjudication corruption.
- Entity ids are globally unique across origins (content-derived or random), never per-origin sequential — LWW registers key on entity id alone, and two origins minting the same id silently merge two entities into one register.
- Per-entity-kind log tables are the rejected form: they foreclose single-transaction capture of multi-entity mutations, destroy the total local replay order, and multiply every cursor, manifest, and sweep by the table count.
- A new entity family is one vocabulary row plus one formatter case — zero schema change; the entity-kind vocabulary is the growth axis of the entire sync system.
- Index plan is fixed by the three access patterns: UNIQUE(origin, seq) is the replay-dedup key, (entity-kind, entity-id) serves materialization, the primary key itself serves cursor range streams.
- Three indexes total is a law, not a starting point — any further index pays append cost on every durable mutation in the system for a read the shape already serves; read patterns that want a fourth index want a projection, and projections are watermark consumers, not log indexes.
- Local seq is minted by the log's own append, never client-side: the (origin, seq) identity of an op exists exactly once, at the insert that also commits the state change.
- Payload law: the payload is the full post-write row image (state-based), not the operation arguments (op-based).
- The state-based requirement is load-bearing: LWW materialization must be reconstructible from any single op without replay context; op-based payloads require ordered full replay and are rejected under LWW.
- The content key is computed over the canonical payload bytes the sealed codec emits — payload identity and payload encoding share one truth, so two replicas encoding the same write produce the same key.
- The materialized table carries (stamp, origin) register columns per row, so adjudication never consults the log: the log is the history and transfer structure; the materialized table is the set of last-writer-wins registers.
- Reads of current state hit the materialized table only — deriving current state by folding the log at read time is the rejected form; the log is folded exactly once per op, at apply.
- The log is append-only by law: the only row-removing verb is fence-gated segment truncation — UPDATE or DELETE against log rows anywhere else is history falsification, and the audit capability stands on this single prohibition.
- Tombstone rows carry an empty payload — the verb is the semantics; a tombstone with a payload is two contradictory statements in one row.
- The verb vocabulary stays closed at two. Field-level patch verbs are admitted only as a declared kind row when concurrent-edit granularity is a product requirement, with the cost stated under adjudication — verb growth is a vocabulary decision, never an ad-hoc enum append.

## single-transaction law

- Local write path: one IMMEDIATE transaction holds { materialized-table mutation; op-row insert } — the log row and the state row commit or vanish together.
- The log IS the outbox: no separate outbox table, no dual-write skew, no relay process between state and history — outbox-pattern machinery is the rejected form because the store's own transaction already provides the guarantee the pattern exists to approximate.
- Remote apply path: one IMMEDIATE transaction per batch holds { dedup-guarded op inserts; set-statement adjudication; peer-cursor advance }.
- A crash replays the whole batch idempotently because all three effects share the commit point — apply-side crash recovery is re-delivery, nothing more.
- Within a batch, ops apply per-origin in seq order — intra-batch reordering is free to the lattice but receipt streams stay deterministic when order is fixed.
- Batch size is a policy row; the batch-shredding statement form below removes the host parameter ceiling from the sizing decision, leaving only latency and lock-hold budgets.
- The lock-hold budget is the binding constraint in a multi-process store: a large apply batch holds the single write window against every sibling writer — batch sizing is a contention decision, and the busy receipts of sibling processes are its feedback signal.
- The local and remote paths are one rail: a local mutation is an op with a fresh stamp applied through the same adjudication statement remote ops use. A local fast path bypassing adjudication is the rejected form — it forks the write rail and lets a stale local write overwrite newer synced state.
- Savepoint-per-unit inside the batch transaction gives unit-level rollback without surrendering the write window — one poisoned op rolls back alone and emits its rejection receipt while the batch proceeds.
- Poison ops — payload decode failures under the sealed-codec rejection ladder — land in a receipted quarantine state rather than blocking the batch or vanishing; quarantine is re-examined on capability upgrades because tier-six rejections are deployment gaps, not data faults.
- Read-your-writes holds by construction: the materialized table mutates in the same transaction as the log append, so a session's own writes are visible at commit with no session-guarantee machinery.

## LWW adjudication

- The whole adjudication is one guarded set statement: upsert into the materialized table with the update arm guarded by `WHERE (excluded.stamp, excluded.origin) > (stamp, origin)`.
- The row-value comparison expresses the total order with origin tiebreak in a single predicate — adjudication logic lives in one SQL expression, so there is no application-side comparison to drift from the stored one.
- The comparison executes inside the store's write lock, so adjudication is race-free without any read-modify-write round trip — there is no window between compare and write.
- `RETURNING` projects exactly the rows the statement inserted or updated; rows the guard filtered return nothing.
- The applied/superseded partition derives from the statement result itself — outcome accounting costs zero extra queries, and there is no second read whose answer could diverge from what the write did.
- Ties are impossible by construction: the stamp orders causally, equal stamps order by origin, and equal (stamp, origin) pairs are the same op — caught by the dedup key, never adjudicated.
- Whole-row LWW is the default row: it preserves writer-intent atomicity — a materialized row state is always some writer's complete write.
- The precise correctness claim: LWW over causal stamps never loses a causally-later write to a causally-earlier one; only CONCURRENT writes resolve arbitrarily-but-deterministically by the origin tiebreak — the loss class is concurrency, never causality, and stating it this way is what makes the receipt stream interpretable.
- Per-field LWW is not a merge upgrade: interleaving field sets from two origins can manufacture a row state no writer ever wrote, violating cross-field invariants.
- Per-field granularity is admitted per entity-kind only when field-level concurrency is the product requirement, and it costs per-field stamps in the log key plus per-field adjudication statements — the granularity decision is a class declaration with a stated price, never a default.
- Batch adjudication shreds the batch through a table-valued JSON expansion into one upsert statement regardless of row count — one statement, one `RETURNING` set, no per-op statement loop.
- The batch travels as one bound document argument, so statement shape is constant across batch sizes — plan caching holds and the prepare cost amortizes to zero.
- The stamp guard doubles as optimistic concurrency for local writes: a stale local edit computed against an old read loses the same comparison a stale remote op loses — offline-edit reconciliation and live conflict control are one mechanism, not two.

## typed conflict outcomes

- The outcome union is closed: Applied | Superseded | Duplicate | TombstoneSuppressed.
- Duplicate falls out of the dedup-guarded insert (an (origin, seq) already present); Superseded falls out of the adjudication guard; TombstoneSuppressed is an upsert losing to a newer tombstone register; Applied is the `RETURNING` set — every outcome is derived from statement results, none from separate reads.
- Every non-applied op emits a receipt — merge loss is always accounted; a silent partition is the rail rejection.
- The conservation invariant is the merge audit: batch size = applied + superseded + duplicates + suppressed, emitted as one merge receipt per batch; a batch whose partition does not sum is itself a typed merge fault — bookkeeping bugs become detected facts.
- Outcome receipts carry (origin, seq, entity ref, incumbent stamp, incoming stamp) — enough to reconstruct the adjudication decision without the log, making conflict review a fold over receipts rather than a forensic join.
- Receipt cadence is two-tier: per-batch merge receipts for accounting, per-session rollups for trend — the rollup is a fold over batch receipts, never a second counting path.
- Future-looking stamps adjudicate normally: a stamp ahead of local time is skew evidence for the signal layer, never an admission rejection here — refusing "future" ops would make merge availability hostage to clock quality, and the lattice is total over all stamp values by construction.
- A detected fork — same (origin, seq) with different content keys — is an epoch-class event emitting a typed fork receipt and halting merge with that peer for operator decision; auto-merging forked histories is the rejected form because a fork means positional identity itself has been violated.

## idempotent replay and cursors

- Replay is convergent by algebra, not by discipline: the dedup key absorbs duplicates, the stamp guard absorbs reordering, tombstone registers absorb resurrection.
- The adjudication relation is a join-semilattice over (stamp, origin) — lattice max is associative, commutative, idempotent, and those three properties are the entire correctness argument.
- Applying any partition of any permutation of the op multiset any number of times yields the same materialized state — this single statement subsumes idempotency, commutativity, reorder tolerance, and crash-replay correctness, and it is the property-test specification verbatim.
- Per-peer cursors are version vectors: peer → (origin → highest contiguous seq applied), persisted as rows keyed (peer, origin).
- When transport delivers per-origin in order (the wire layer's settled guarantee), each origin's cursor entry is a scalar watermark.
- If a route can reorder batches, the cursor advances only over contiguous prefixes and gapped ops wait in receipted hold-back state — silent gap-skipping is cursor corruption, because a skipped seq is an op the cursor claims to have applied.
- The cursor advances inside the apply transaction (single-transaction law), so cursor state can never run ahead of or behind applied state.
- No reconciliation job exists because there is nothing to reconcile — every "cursor repair" mechanism in a sync design is evidence the cursor left the transaction it belonged in.
- The same watermark structure serves in-process invalidation: projections and caches track a log seq watermark, and "what changed since" is a primary-key range scan — the change feed and the sync feed are one structure with different consumers.
- Projection watermarks are registered consumers, not ad-hoc readers: the set of live watermarks participates in the log truncation floor, so a projection can never be invalidated by log compaction it did not consent to.
- Cursor rows carry the last-applied stamp per origin beside the seq watermark — a cheap divergence probe: two replicas comparing (seq, stamp) pairs detect a forked history without exchanging ops.

## transport-invariant merge

- The merge fold takes a sequence of ops and adjudicates — the same fold for incremental batches, full-snapshot import, and file-drop import, because convergence comes from the lattice, not from arrival discipline.
- Merge(snapshot) ≡ merge(ops-of-snapshot): a state snapshot is the compressed prefix of the log.
- A restored materialized table already carries its (stamp, origin) registers — post-restore ops adjudicate correctly without the log prefix being present, which is what licenses log truncation behind snapshots.
- Snapshot emission is one read transaction over the materialized state plus the watermark vector plus the epoch token — the snapshot is self-positioning: any peer can resume incremental sync from the embedded watermark without negotiation.
- Snapshot cadence is a ratio policy row — emit when log-growth-since-last-snapshot crosses a declared multiple of snapshot size — so bootstrap cost and log-truncation eligibility stay bounded together by one number.
- Set-difference transfer has two manifest forms chosen by identity space.
- The seq-vector manifest (origin → max seq; O(origins) bytes) covers log-shaped data where identity is ordered — difference is computable from the vector alone, with zero per-row enumeration.
- The content-key manifest (exchanged key sets over codec-owned content identity) covers content-addressed blob lanes where identity is unordered — difference is set subtraction over keys.
- Cross-use is rejected in both directions: content manifests for log rows re-derive what seq already proves; seq vectors for blobs couple blob identity to log order.
- Both manifests are commutative-monoid summaries: manifest(a ∪ b) = manifest(a) ⊔ manifest(b).
- Peers aggregate manifests from partial views without coordination — which is what makes mesh and relay topologies free at this layer; no topology row exists in durability because none is needed.
- Responding to a manifest is a range query per origin (`seq > cursor`) streaming in seq order — the responder needs no per-peer state beyond the received manifest itself.
- Acknowledgment IS the cursor echo: a receiver acknowledging a batch reports its new cursor value, and the sender's per-peer cursor adopts it — no separate ack protocol, no sequence-number bookkeeping beside the one the log already owns.
- Because the responder streams in seq order, receiver cursor advance is monotone within a session — out-of-order arrival is a route property, never a responder property, which is what localizes the hold-back requirement to reordering transports only.

## presence as ephemeral rows

- Presence and awareness ride the same table shape under an ephemeral entity-kind class: stamped with a TTL expiry, never tombstoned (absence after expiry IS the delete), merged as max-stamp per (origin, presence key).
- Ephemeral rows are excluded from durable cursors and manifests — a peer's sync position is never a function of liveness chatter, so presence churn cannot inflate sync state.
- The ephemeral class declares its loss policy up front: expiry is receipt-in-advance, so an expired presence row is droppable under the class's declared policy and the sweep emits one count receipt rather than per-row receipts — declared-expiry loss and receipted-eviction loss are distinct accountings, never an implicit downgrade.
- Presence needs no tombstones because its key space is bounded — one register per (origin, presence key) — and absence is expiry; the unbounded-key deletion problem that forces tombstones on durable kinds structurally cannot arise.
- Presence rows are excluded from snapshot emission — a snapshot is durable state by definition, and a presence row in a snapshot would resurrect a stale liveness claim at every restore.
- Presence proves the class axis: durable and ephemeral are rows of one table's class column, not two tables — high-frequency zero-durability traffic and low-frequency durable mutation share the shape, the adjudication statement, and the merge fold, diverging only in cursor participation and sweep policy.

## tombstones

- A delete is a tombstone row adjudicated exactly like a write: a stale upsert arriving after a newer tombstone loses the stamp comparison and emits TombstoneSuppressed.
- Deletion is a register state, not an absence — a register that vanished instead of tombstoning cannot defend itself against late writes, which is the entire reason tombstones exist.
- Tombstone collection is cursor-fenced, never time-fenced: a tombstone is collectible only when every known peer's cursor has passed its seq. A time window reintroduces resurrection through any peer offline longer than the window — the classic anomaly, foreclosed structurally by the fence predicate.
- The fence predicate degrades safely under peer churn: a permanently departed peer must be explicitly retired from the known-peer set (a receipted administrative op), otherwise its frozen cursor pins tombstones forever.
- Pinning is the deliberate failure direction: unbounded tombstone growth is observable and repairable, while early collection is silent resurrection — the asymmetry decides every marginal case toward pinning.
- Boundary: this lane owns the fence predicate (when a tombstone is safe to drop); the retention layer owns sweep execution and its receipts — the fence is supplied to the sweep as the eligibility function for log-segment and tombstone-bearing classes.

## epoch and cursor fencing

- Every cursor is (store epoch, version vector). Restore, rebuild, or destructive re-initialization bumps the store's epoch token.
- A peer presenting a stale-epoch cursor receives a typed epoch-mismatch outcome that routes to full resync — never a silent serve from rewound sequence numbers.
- Seq aliasing after restore — new ops reusing burned seq values against old cursors — is the corruption class the epoch forecloses; it cannot be detected from seq values alone, which is why the epoch is a separate token and not a seq convention.
- The epoch token travels on the discovery manifest the wire layer publishes; durability owns the token's semantics, comparison, and bump points — the wire layer owns only carriage.
- The token is the same store-epoch the sealed-artifact layer stamps into headers, so a restored snapshot and the cursors it invalidates reference one epoch vocabulary.
- One token, three consumers — open ritual, sync cursors, artifact headers — and a second epoch-like counter anywhere is the rejected form that re-splits the fence.
- Epoch comparison is equality-only, never ordering: any mismatch routes to resync regardless of which side looks newer — reasoning about epoch recency re-introduces exactly the trust in rewound counters the token exists to remove.
- Epoch bump points are exactly the restore choreography's commit point — there is no separate epoch management; the fence is a consequence of restore law, observed by sync.
- Cursor rows persist in the same store as the log and the materialized state, inside the same transactions — externalizing cursors to a side file re-opens the dual-write skew the single-transaction law closed.

## divergent

### oplog-shape-transactions — the log as the universal write-ahead domain structure

- Four capabilities fall out of one table with zero additional machinery: undo/redo (walk own-origin seq descending, inverting payloads), audit (the log IS the audit artifact class; no parallel audit table exists to drift), change-data-capture (projection consumers fold seq ranges through the watermark structure), and sync (peers fold origin ranges through cursors).
- Each capability is a different fold over the same rows — the deep reason the one-table law holds: every second table proposed for these jobs is a materialized fold of this one, and materializing it by hand re-derives consistency the transaction law already grants.
- Segmentation by seq band turns cold log prefixes into immutable sealed artifacts (handed to the artifact lifecycle as a class) while the active band stays relational for adjudication — the log is relational at the write edge and content-addressed at the cold edge, one structure with a temperature gradient.
- The segment boundary is itself cursor-fenced: the truncation floor is min(every peer cursor, every local projection watermark), so segmentation can never strand an op a live consumer still needs in relational form.
- One log per store file; cross-store sync is N independent lanes with no global order — a requirement for cross-store ordering is a store-boundary error, not a log feature request.

### lww-adjudication — set algebra with a conservation proof

- The batch-shredded single-statement form makes merge cost O(1) statements per batch and the `RETURNING` set the single source of outcome truth.
- The conservation invariant upgrades merge from "applied without error" to "fully accounted" — a conservation breach is a detectable implementation fault, never silent loss.
- The property-test specification falls directly out of the lattice: for any op multiset, any partition, any permutation, any repetition — identical materialized state and identical receipt multiset modulo order; one law subsuming idempotency, commutativity, reorder tolerance, and crash-replay correctness, executable as a generator-driven test without mocks.
- The cross-feature collapse: because the stamp guard is also the optimistic-concurrency check, a UI undo, an offline edit replay, a remote batch, and a restore-then-catch-up are all the same statement against the same registers — the system has exactly one way to change durable state, and every consistency argument reduces to the lattice plus the transaction law.
- Outcome receipts ride the operational fact stream, never the log itself — receipts about merging are not ops, and logging them as ops would make every merge self-amplifying.

### merge-presence — arrival-route invariance as the design generator

- Because merge(snapshot) ≡ merge(ops), restore-then-sync needs no special mode: bootstrap of a new replica is "restore latest sealed snapshot, then range-pull from the snapshot's embedded watermark", and catastrophic cursor loss degrades to full resync rather than manual repair — every recovery path is the normal path with a wider range.
- The manifest duality is a chooser law: ordered identity space → seq vector, unordered identity space → content-key set; choosing the manifest IS choosing the identity space.
- An artifact wanting both (a sealed log segment: ordered interior, content-addressed exterior) carries both — the seq range in its manifest entry and the content key as its identity — letting a peer decide "skip, have it" (content key) versus "need this seq range" (vector) from one manifest line.
- The hold-back buffer for gapped ops is budget-bounded with receipts under the artifact budget law — an unbounded reorder buffer is the rejected form, and a budget overflow there is a typed backpressure fact routed to the transport seam.
- Presence economics close the loop: the highest-frequency traffic rides the cheapest path — no cursor writes, no manifest entries, count-receipt sweeps — proving the class column, not a second system, is the cost dial; a presence-shaped requirement reaching for a side channel has misread the class axis.
