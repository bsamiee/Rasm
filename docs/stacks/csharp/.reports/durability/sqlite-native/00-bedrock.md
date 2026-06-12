# [BEDROCK] sqlite-native

## pragma rows

- `journal_mode=WAL` is file-persistent: provisioned once, inherited by every later open from any process; re-issuing is a cheap idempotent read.
- The pragma table carries a residency column — file-persistent (`journal_mode`, `page_size`, `auto_vacuum`, `user_version`, `application_id`) versus per-connection (everything else) — and the open ritual folds only the per-connection rows; file rows belong to provisioning.
- The bundled native build defaults `synchronous` to FULL even under WAL; the WAL throughput row is an explicit `PRAGMA synchronous=NORMAL` per connection.
- NORMAL's loss boundary: a power cut can drop the last commits but never corrupts; FULL stays the row for stores whose single commit is the artifact of record — the synchronous row is per-store-class policy, not a global constant.
- The bundled build compiles foreign-key enforcement ON by default — the inverse of stock SQLite. Provisioning still pins it (the `Foreign Keys` connection-string row emits the pragma at open) because the file may later open under a differently-compiled host.
- `recursive_triggers` is likewise default-on in this build — trigger logic written assuming the stock off-default recurses here.
- `busy_timeout` stays 0. The provider owns busy retry (law below); a nonzero `busy_timeout` stacks a native sleep beneath the managed loop and the two budgets multiply — the double-retry-owner form is rejected.
- `wal_autocheckpoint` defaults to 1000 pages; `journal_size_limit` bounds the WAL file's disk footprint after RESTART/TRUNCATE checkpoints — without it a once-bloated WAL never shrinks on PASSIVE checkpoints.
- `cache_size` takes the negative-KiB spelling so the budget is size-stable across page sizes; `temp_store=MEMORY` overrides the build's file default for sort and index spill.
- `page_size` binds only before the first write or via VACUUM (build default 4096, ceiling 65536) — a page-size decision deferred past first write costs a full rewrite.
- `mmap_size` is 0 by default with a ~2 GiB build ceiling; mmap converts read syscalls to page faults, but media I/O errors then surface as process-fatal faults outside any typed rail — a measured opt-in for read-heavy stores on trusted local media, never a default row.
- `user_version` is a 32-bit app-owned register readable without schema — the migration/epoch stamp; `application_id` is the file-identity register distinguishing this store class from generic SQLite files. Identity check precedes epoch check precedes any query.
- `PRAGMA data_version` moves when ANOTHER connection (including other processes) commits and does not move for own-connection writes — the polling-free cross-process change-detection primitive: an unchanged register proves cache validity without touching tables.
- The bundled build sets `DQS=0`: double-quoted string literals are syntax errors in both DDL and DML — identifiers quote with `"`, strings with `'` only; SQL written for lenient builds breaks here at prepare, which is the desired failure point.
- `auto_vacuum=INCREMENTAL` binds only before the first table (or via VACUUM); `incremental_vacuum(N)` then reclaims free pages in bounded steps — steady-state full VACUUM is the rejected reclamation verb.
- Rejected rows: `locking_mode=EXCLUSIVE` (forecloses cross-process WAL), shared-cache and its `read_uncommitted` (single-process aliasing model superseded by WAL), network filesystems as store hosts (the WAL shm index requires coherent same-host shared memory).
- Platform barrier rows: `fullfsync` / `checkpoint_fullfsync` upgrade flush strength on hosts where plain fsync does not reach stable media.
- Build ceilings that shape statement design: 32766 bound parameters per statement (the bulk-write batch ceiling), 10 ATTACH slots, no `dbstat` virtual table, stat1-only ANALYZE (no stat4 sketches).
- ATTACH atomicity trap: with the main database in WAL mode, a transaction spanning attached database files is atomic per file but NOT atomic across files on a crash mid-commit — multi-file stores cannot lean on cross-ATTACH transactions for invariants; one store file per invariant boundary.

## schema-level admission rows

- `STRICT` tables enforce declared column types at insert — type-mismatched writes are statement errors, not silent affinity coercions; the schema becomes a typed admission gate, and STRICT is the default row for new durable tables.
- The `ANY` column type inside a STRICT table is the declared escape slot for genuinely polymorphic cells — opting out per column, never per table.
- `WITHOUT ROWID` tables cluster storage on the primary key — the row for wide natural keys and content-key tables where the rowid indirection costs a second b-tree probe per lookup.
- `WITHOUT ROWID` forecloses incremental blob I/O: blob stream handles open by rowid only — tables carrying streamable blobs stay rowid-keyed, and the two storage forms are chosen per table by access pattern, not by convention.
- `RETURNING` on INSERT/UPDATE/DELETE supersedes the write-then-read-last-rowid round trip — one statement yields the written keys and computed columns; separate identity queries after writes are the rejected form.

## provider connection law

- The connection-string rows are exactly the `SqliteConnectionStringBuilder` properties: `DataSource`, `Mode` (`ReadWriteCreate`|`ReadWrite`|`ReadOnly`|`Memory`), `Cache` (`Default`|`Private`|`Shared`), `ForeignKeys` (tri-state), `RecursiveTriggers`, `DefaultTimeout`, `Pooling`, `Vfs`, `Password`.
- `Password` is a rejected row — the admitted native bundle excludes the cipher provider; supplying it fails at open, not silently.
- `Mode=Memory` plus `Cache=Shared` is the shared in-memory store form (multiple connections, one transient database); private in-memory databases vanish per connection — the test-double and scratch lane, never a durability lane.
- `Mode=ReadOnly` cannot serve a WAL store on read-only media: readers must materialize the shm index. The read-only-media row is an immutable file-URI data source, which also disables all locking — admissible only for genuinely frozen artifacts.
- Pooling defaults on and is keyed by exact connection string. Pooled inner handles survive `Close`, so per-connection pragmas and registered functions persist on reused handles — the open ritual stays idempotent so a primed handle and a cold handle converge.
- Fencing law: file replacement requires `SqliteConnection.ClearPool(connection)` (or `ClearAllPools()`) before the swap — an open pooled handle pins the deleted inode and its readers silently continue against the dead store.
- `BeginTransaction(deferred:)` with non-deferred Serializable issues `BEGIN IMMEDIATE` — write intent declared at begin; the deferred overloads exist for read-only transactions that must never take the write lock.
- Savepoints are first-class (`Save`/`Rollback(name)`/`Release`), giving nested partial rollback inside one IMMEDIATE envelope — batch apply loops roll back one unit without surrendering the write lock.
- `DefaultTimeout` (seconds, default 30) is the busy budget; per-command override via `CommandTimeout`; 0 means retry forever.
- One command text may carry multiple statements; the provider prepares and steps them in sequence inside the command — migration scripts run as one command, but per-statement receipts require per-statement commands.
- Parameter binding accepts `$`, `@`, and `:` prefixes; `SqliteType` classifies binding when type affinity inference is wrong (notably BLOB versus TEXT for byte payloads).
- `ServerVersion` reads the running native library's version string — a boot receipt fact for the support bundle, never a branch condition in code.
- `Vfs` selects a registered virtual filesystem by name — the niche row for platform-specific I/O layers; absent means the platform default, and naming an unregistered VFS fails at open.

## busy-retry law

- The provider retries both prepare and step while the result code is BUSY (5), LOCKED (6), or shared-cache LOCKED (262), sleeping 150 ms per probe until `CommandTimeout` seconds elapse.
- Budgets are therefore quantized at 150 ms and a sub-150 ms budget degrades to a single attempt — fine-grained busy budgets do not exist on this surface; the schedule layer owns finer policy by wrapping the call, not by tuning the loop.
- Extended result codes are off by default, so BUSY_SNAPSHOT surfaces as plain BUSY and the loop burns its whole budget on a retry that cannot succeed: a deferred read transaction attempting its first write after another writer committed holds a stale snapshot — only ROLLBACK and restart helps.
- Law derived from the above: any transaction that may write begins IMMEDIATE; deferred-then-write is the rejected form, rejected precisely because retry semantics differ by failure class and the failure class is invisible at default code granularity.
- `raw.sqlite3_extended_result_codes(db, 1)` upgrades the running taxonomy when receipts must discriminate BUSY_SNAPSHOT from BUSY_RECOVERY; `SqliteException` carries both `SqliteErrorCode` and `SqliteExtendedErrorCode` on throw regardless.
- Retry-eligibility partition: BUSY (cross-connection contention) is retry-correct; LOCKED (same-process conflict) waits on nothing external; BUSY_SNAPSHOT requires transaction restart; CORRUPT/NOTADB are terminal admission failures routed to restore, never retried.

## WAL cross-process law

- WAL coordination rides the memory-mapped `-shm` index: same-host processes only. The `-wal`/`-shm` sidecars are part of store identity — the sidecar set, never the bare file, is the unit of copy, replace, and deletion.
- Readers never block the writer and the writer never blocks readers; exactly one writer commits at a time — multi-process write topology is contention-managed serialization, not parallelism, and write-burst processes must treat BUSY as a steady-state signal, not an anomaly.
- First-opener-migrates: every process runs one idempotent open ritual — open, `BEGIN IMMEDIATE`, read `user_version`; if behind the compiled expectation, apply migration steps and bump the register inside the same transaction.
- Losers blocked on the IMMEDIATE lock observe the bumped version on acquisition and no-op; correctness needs no leader election because the version check and the DDL share one transaction.
- A `user_version` ahead of the compiled expectation is a typed rejection at open, never a best-effort proceed.
- Epoch fencing: restore or rebuild bumps the epoch register and clears the connection pool before the file swap; the fence order is pool-clear → sidecar-set replace → epoch bump → reopen.
- Sibling processes detect the new epoch through `data_version` movement plus the epoch read in their next open ritual — no notification channel is required for correctness, only for latency.
- Checkpoint starvation: continuously overlapping readers prevent PASSIVE checkpoints from completing and the WAL grows unbounded; the countermeasure is a scheduled TRUNCATE checkpoint row, and reader code keeps read transactions short by construction.
- `sqlite3_wal_checkpoint_v2` exposes the four modes (PASSIVE 0, FULL 1, RESTART 2, TRUNCATE 3) and two out-parameters (`logSize`, `framesCheckPointed`) — the typed checkpoint receipt; RESTART/TRUNCATE return BUSY while a reader pins the tail, and the receipt records the refusal for the schedule to retry rather than escalate.
- Crash recovery is automatic and first-connection-borne: the first open after an unclean exit replays the WAL and rebuilds the shm index; siblings arriving during the replay window see the recovery-class busy code — recovery needs no operator verb, only the patience the busy budget already provides.
- A `-wal` file paired with a database it does not belong to (a main file copied without its sidecars, or sidecars surviving a payload swap) is silent corruption at the page level — the sidecar-set law exists because recovery cannot detect the mismatch.

## raw escape hatch

- `SqliteConnection.Handle` exposes the native db handle to the bound raw surface — the one sanctioned crossing, used for capabilities the provider does not wrap.
- Hardening rows via `raw.sqlite3_db_config`: DEFENSIVE (1010) blocks corrupting writes through SQL, RESET_DATABASE (1009) powers destructive re-initialization as a two-step config-then-VACUUM, ENABLE_FKEY (1002), DQS_DML/DQS_DDL (1013/1014), TRUSTED_SCHEMA (1017).
- Three `sqlite3_db_config` call shapes exist (string, int-with-out-result, pointer-plus-two-ints); the int form's out-parameter echoes the applied state — a configuration receipt, read it.
- `raw.sqlite3_limit` lowers per-connection runtime ceilings beneath compile caps — the defense row for surfaces accepting foreign SQL fragments; `sqlite3_db_readonly` and `sqlite3_db_filename` introspect live handles; `sqlite3_wal_autocheckpoint` re-tunes the page threshold at runtime.
- The native build enables the session/preupdate machinery, but the managed binding exposes neither — changeset-based replication is unreachable from the admitted surface: a reject row, not an option.
- Custom R*Tree geometry callbacks are likewise unbound — spatial MATCH against app-defined geometry is foreclosed; the rtree-filter-then-recheck pattern is the admitted spelling.

## custom functions

- `CreateFunction` rows: typed arities to 16, an `object?[]` var-args catch-all, and state-threading overloads `CreateFunction<TState, …>(name, state, fn)` that close over a capability without per-call allocation.
- `isDeterministic: true` admits the function into expression indexes, generated columns, and partial-index predicates — the flag is a capability grant, not an optimization hint.
- Schema-residency trap: any index, view, trigger, or generated column referencing an app-defined function makes the file unreadable to connections that have not registered that function before first schema use — registration is connection-instance-scoped and never persisted.
- Law: the open ritual registers every schema-resident capability before the first statement, and the capability list is one declared table, not scattered calls.
- `CreateAggregate` rows: seedless variants thread a nullable accumulator (null marks the first step); seeded variants take `(name, seed, stepFn[, resultSelector])` — seed plus step plus projection is a complete fold specification, so domain folds run inside the store instead of materializing rows out.
- Window functions are not exposed by the provider — aggregate rows only; window-shaped needs route through SQL window functions over plain columns.
- `CreateCollation` (with optional state) follows the same registration-before-use law; collations named in persisted DDL are schema-resident.

## blob lane

- `SqliteBlob` is a `Stream` over one blob cell — `(connection, [databaseName,] tableName, columnName, rowid, readOnly)` — seekable, span-read/write capable; large payloads stream without materializing row-sized arrays.
- The write protocol is `zeroblob(N)` preallocation in an INSERT/UPDATE, then `last_insert_rowid()`, then streaming through the blob handle — blob cells are fixed-size once written; appends are a new row, not a grow.
- A blob handle aborts when its row is mutated by any writer — blob streaming windows are short-lived and re-openable by design; long-held handles against hot rows are the rejected form.
- `SqliteDataReader.GetStream` projects a read-side stream over a blob column — the symmetric read lane.

## FTS5

- External-content form `fts5(cols…, content='base', content_rowid='id')` keeps text out of the index file; synchronization rides three triggers where delete is spelled `INSERT INTO t(t, rowid, cols…) VALUES('delete', old.id, old.cols…)` — deletes must replay the OLD column values.
- Drift between base table and index corrupts match results silently, so `INSERT INTO t(t) VALUES('integrity-check')` is a scheduled verification row, not an optional one.
- Contentless form (`content=''` with `contentless_delete=1`) admits delete and update without stored text — the minimal-footprint row when original text lives elsewhere.
- Tokenizer rows: `unicode61` with `remove_diacritics 2`, `porter` stemming wrapper, `trigram` for substring-class matching (which also accelerates LIKE/GLOB over the index).
- `detail=none` halves index size but forecloses phrase and NEAR queries; `detail=column` is the middle row — capability traded for bytes as an explicit declaration.
- `prefix='2 3'` materializes prefix indexes for typeahead; `UNINDEXED` columns carry payload through the virtual table without index cost.
- `bm25(t, w1, w2, …)` ranks with per-column weights; `ORDER BY rank` consumes the configured rank function (settable as a row via the `'rank'` config verb); `highlight()`/`snippet()` project match evidence.
- Index maintenance is the special-INSERT verb family: `'optimize'` (full b-tree merge), `'merge'` with a page quantum (incremental), `'automerge'`/`'usermerge'` (merge policy values), `'rebuild'`, `'delete-all'`, `'pgsz'`, `'secure-delete'` (purge deleted-row residue from the index file).
- The `fts5vocab` virtual table projects term statistics — the vocabulary audit and stopword-discovery surface, queryable like any table.
- MATCH grammar carries column filters (`col: term`), NEAR with distance, boolean operators, caret-anchored initial tokens, and quoted phrases — query shaping belongs in the MATCH expression, not in post-filtering.

## R*Tree and spatial

- `rtree(id, minX, maxX, …)` indexes 1–5 dimensions; coordinates store as 32-bit floats rounded outward: every stored box contains its true box, so overlap and containment probes are complete but inexact.
- The exactness law: rtree filters candidates, the row's true geometry re-checks — skipping the recheck ships float-rounding false positives; `rtree_i32` stores exact 32-bit integers when coordinates are integral.
- Auxiliary columns (`+name`) carry non-indexed payload inside the rtree row, making the join back to a content table optional for payload-only reads.
- The build includes the polygon layer (`geopoly`) with containment and overlap functions over JSON-array polygons — the 2D irregular-shape row above plain boxes.
- Rtree, FTS5, and ordinary tables all key on rowid — hybrid predicates (text ∩ spatial ∩ relational ∩ document) compose as rowid joins where each index prunes its own dimension inside one SELECT.

## JSON

- The binary JSON form is the storage row: `jsonb(x)` re-encodes once at write, `jsonb_extract`/`jsonb_set` and family operate without re-parsing; project to text with `json(x)` only at egress. Text-JSON storage pays a full parse per touch — the rejected default.
- `->` returns JSON, `->>` returns an SQL value; expression-index matching is textual, so the indexed expression and the query expression must use the identical operator and arguments.
- `json_each`/`json_tree` shred documents as table-valued functions (lateral joins over arrays) — also the batch-argument shredder that turns one bound document into a relational rowset, sidestepping the parameter ceiling for bulk statements.
- `json_group_array`/`json_group_object` aggregate rows back to documents; `json_patch` applies merge-patch semantics; relaxed JSON5 input is accepted at parse boundaries.
- Indexing law: a VIRTUAL generated column over `jsonb_extract` plus an ordinary index, or a direct deterministic expression index — both make document fields planner-visible; un-indexed `->>` predicates scan.

## maintenance verbs

- Integrity tiers: `quick_check` (skips index-versus-content cross-validation; the boot tier) and `integrity_check(N)` (full tier, error-row cap N) — the returned error rows are the receipt, empty set is the proof.
- `foreign_key_check` is a separate verb — FK violations never surface from integrity checks; the deep cycle runs both families plus the FTS verification verb.
- `PRAGMA optimize` is the close-of-connection row and the long-session cadence row; it re-analyzes only tables whose shape drifted, so its steady-state cost is near zero.
- `VACUUM INTO 'path'` produces an online, compacted, point-in-time copy through one read transaction — the cold-backup and compaction-proof verb that never blocks writers; it fails fast if the target exists.
- In-place VACUUM requires no open transaction, rewrites the whole file, and is the only point `page_size`/`auto_vacuum` changes take effect — a provisioning verb, not a maintenance verb.
- Size receipts ride `page_count` and `freelist_count`; with no `dbstat` table in the build, per-object size breakdown is foreclosed — size accounting stays file-grain and the receipts say so.

## backup and snapshot

- `BackupDatabase(destination[, destinationName, sourceName])` runs the online backup: writes by OTHER connections during the copy restart it (livelock risk under hot write load), while writes through the SAME source connection fold into the copy without restart — on a hot store, back up on the writing connection or use `VACUUM INTO`.
- The paced raw form — `sqlite3_backup_init/step(nPage)/remaining/pagecount/finish` — yields bounded latency injection and progress receipts (`remaining`/`pagecount` per step); BUSY or LOCKED from `step` is retryable without abandoning the backup handle.
- Route chooser as policy values on one verb: whole-file managed (cold or idle stores), paced raw (hot stores needing progress receipts), `VACUUM INTO` (compacted artifact, fails-fast, single read transaction, no restart semantics).
- The WAL snapshot family (`sqlite3_snapshot_get/open/cmp/recover/free`) pins a read position: `get` requires an open read transaction, `open` rewinds a fresh read transaction to the pin, `cmp` orders two pins from one connection era, `recover` restores pin availability after reopen.
- A pin survives only while checkpoints have not overwritten its frames — the pinning strategy is hold-a-read-transaction or suppress checkpointing for the window, and a lost pin is a receipted failure, never a silent rewind to a different position.
- Snapshot pins and checkpoint maintenance are adversaries by construction: a pinned window blocks TRUNCATE progress, and TRUNCATE progress kills pins — the schedule that owns both rows must interleave them explicitly rather than discovering the conflict as BUSY noise.

## divergent

### pragma-wal-crossprocess — the one open ritual

- Every connection in every process folds the same declared sequence: identity check (`application_id`) → per-connection pragma rows → `db_config` hardening rows → capability registration rows → IMMEDIATE migration gate (`user_version`) → epoch read.
- Each ritual row carries residency, value, and verification read; the ritual is idempotent end-to-end, so bootstrap, crash-recovery reopen, and steady-state open are one fold and no first-process special case exists anywhere.
- Cross-process cache invalidation needs no message bus: a schedule row polls `data_version`, and an unchanged register short-circuits all downstream invalidation work.
- The ritual table is also the audit surface — diffing two processes' rituals is diffing two declarations, not two codebases; a process that cannot state its ritual as a table is the defect.
- Rejected forms this design forecloses: out-of-band migration runners (lock not held during DDL), busy_timeout-owned retry (double budget), deferred-write transactions (unretryable stale-snapshot class), EXCLUSIVE and shared-cache modes, network-filesystem hosting, and cross-ATTACH invariants under WAL.

### fts-rtree-functions — capability as one registration table

- Text indexes, spatial indexes, scalar functions, aggregates, and collations are rows of one declared capability table (name, kind, registration thunk or DDL, deterministic flag, schema-resident flag) folded during the open ritual.
- The schema-resident flag derives the ordering law mechanically: resident rows must register before the first statement or the file is unreadable — the trap becomes a sortable column instead of tribal knowledge.
- Growth: a new search dimension is one row plus one rowid join in the hybrid SELECT; each virtual-table index prunes its own dimension, and post-filter predicates in application code are the rejected form because they hide selectivity from the planner.
- JSON bridges in through generated columns: one VIRTUAL projection feeds btree, FTS, and rtree dimensions from a single stored document — document storage and multi-modal indexing are orthogonal axes, not a format decision.
- The same table absorbs in-store domain folds: a seeded aggregate row is a fold specification executed at the data; the chooser between in-store aggregate and out-of-store fold is row cardinality at the seam — ship the fold to the rows when the rowset is large, ship the rows out when the fold needs domain context the store lacks.

### maintenance-backup — verbs as one receipted schedule table

- Every maintenance verb (checkpoint TRUNCATE, optimize, incremental_vacuum quantum, integrity tier, FTS merge/optimize, backup route, snapshot pin window) is a row carrying cadence, budget, and receipt shape.
- Receipts are the verbs' native out-channels (`logSize`/`framesCheckPointed`, error-row sets, `remaining`/`pagecount`) lifted onto the fact stream — no verb invents a receipt format.
- Admission law for backup artifacts: a copy is admitted only after a post-copy `quick_check` on the copy itself plus content identity — the verb succeeding is never the proof; verification of the artifact is.
- The integrity ladder orders cheap-to-deep — boot quick_check → cycle integrity_check + foreign_key_check + FTS integrity-check → pre-backup verification — and a deeper tier failing routes to restore choreography rather than retry; maintenance, verification, and restore form one decision lattice with receipts as its edges.
- Verb adjacency is schedule data: TRUNCATE checkpoints precede backup rows (smaller copy), snapshot pin windows exclude checkpoint rows (adversaries), optimize follows bulk-write bands — adjacency encoded as ordering rows is what keeps the interactions out of prose.
