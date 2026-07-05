# STACK-DATA — ultra-stacking dossier for libs/typescript/data

Scope: 18 planning pages (lane×6, journal×4, object×3, read×5), 20 folder `.api` catalogs, 8 branch-substrate catalogs, prefetch-remote ruling. The corpus is already 13/10-dense on the SQL rails, the journal, the object store, the read side. The stacking headroom is concentrated in (1) the never-built remote-origin filesystem plane, (2) resilience internalization on every backend owner, (3) a handful of high-precision underutilized members with real correctness stakes. Verified spellings throughout; every member below is confirmed against the catalog that owns it.

---

## [A] UNDERUTILIZED MEMBERS — admitted, catalogued, member-exact, NOT consumed

Each row: `member` (exact spelling) — owning catalog — landing page — why.

### A1. `SqlClient.SafeIntegers` (`Context.Reference`) — `data/.api/effect-sql.md` → `journal/append.md`, `read/fold.md`, `journal/retain.md`
The journal `sequence` is the GLOBAL identity column (`BIGINT GENERATED ALWAYS AS IDENTITY`) growing unbounded across every stream. Three hot paths decode it lossily with `Number()`:
- `append.md` `_head` → `Number(cells[0]?.[0] ?? 0)` (version is per-stream, low-risk; but `_publish`/receipt carry it).
- `fold.md` `_cycle` → `globalThis.Number(row["sequence"])` paging `sequence > checkpoint` and advancing the checkpoint — **the checkpoint is a global-sequence cursor; past 2^53 it silently stalls or double-drains.**
- `retain.md` `_dsar` joins `subject_journal.sequence = journal_event.sequence` — a truncated sequence mis-joins.
The catalog names this member's consumer verbatim: "per-fiber bigint-safety toggle for large-integer columns (journal sequence numbers)". Fix: set `SqlClient.SafeIntegers` on the daemon fiber + the append/DSAR reads, decode `sequence` as `bigint` (or `Schema.BigInt`), keep per-stream `version` as `Number` only where the aggregate cardinality is provably bounded. This is a correctness defect, not a nicety.

### A2. `KeyValueStore.prefix(key)` + `KeyValueStore.SchemaStore`/`layerSchema` — `libs/typescript/.api/effect-platform.md` → `lane/cache.md` (PERSISTED), `read/batch.md` (durable band)
`cache.md` `_backing` composes `KeyValueStore.layerMemory` / `layerFileSystem(directory)` raw. `KeyValueStore.prefix(k)` scopes a store by key prefix — **the per-app/per-tenant isolation lever emphasis #3 (thousands-of-apps soundness) demands.** A multi-tenant `PersistedCache` over an unprefixed store collides keys across tenants: app A's cached capability report serves app B. Fix: `CacheLane.persisted` and `batch.md` `durable` compose `.prefix(scopeKey)` from the owning `Stores` scope so persisted bands are tenant-partitioned by construction. `layerSchema(schema)` additionally gives a typed KV so a cache key is a decoded persistable, not a hand string.

### A3. `connection.streamAndReadAll` + `connection.runAndReadUntil` — `data/.api/duckdb-node-api.md` → `lane/olap.md` (EMBEDDED, closes the RESEARCH tag)
`olap.md` `_query.window` uses only `streamAndReadUntil(sql, take)` (bounded first window) and self-declares RESEARCH on the unbounded incremental reader. The catalog surfaces `streamAndReadAll` and `runAndReadUntil` beside it. The unbounded egress rides `streamAndReadAll` with a reader-continuation loop lifted through `Stream.paginateChunkEffect` / `Stream.asyncPush` over the reader's incremental `getRows()` — this settles the deferred fence with real members, not a placeholder.

### A4. S3 command rows imported/named but not wired — `data/.api/aws-sdk-client-s3.md` → `object/store.md`
- `PutBucketLifecycleConfigurationCommand` — `store.md` imports it in REFERENCE_GC and the prose says "bucket lifecycle rules land as data", but the code fence never composes it. Retention-class windowed expiry should push a native S3 lifecycle rule set (belt-and-suspenders under the CAS sweep, and the only GC that survives SQL-ledger loss). Wire it in `store.md` as a data row over `Retain.Policy` windows.
- `PutObjectTaggingCommand` / `GetObjectTaggingCommand` — retention-class + reference-count tags ON the object. `store.md` keeps retention only in the `object_ref` SQL ledger; object tags let the native lifecycle rules act and reconcile GC against S3-side truth. Underutilized.
- `waitUntilObjectNotExists` — `store.md` `_settled` uses `waitUntilObjectExists` for the write-then-serve window, but the sweep/delete path (`_sweepDelete`) has NO post-delete consistency waiter. `waitUntilObjectNotExists({ client, maxWaitTime })` closes the delete-then-relist race on eventually-consistent engines.
- `GetObjectAttributesCommand` — `store.md` `_headed` uses `HeadObjectCommand`; `GetObjectAttributes` yields `ObjectParts` + `Checksum` evidence for multipart integrity verification the plain HEAD cannot carry.
- `CopyObjectCommand` (`CopySourceIfMatch`) — server-side content-key copy/rekey (zero-byte-transfer), never used; the derivative/promotion path re-uploads bytes it could server-copy.

### A5. sharp members claimed-in-prose, not-in-fence — `data/.api/sharp.md` → `object/file.md` (DERIVATIVE_ROWS / FANOUT / CODEC_GATE)
- `.metadata()` — `file.md` DERIVATIVE_ROWS law says "`metadata()` and `stats()` are the decision reads — pre-decode format/geometry select or veto rows", but the `_fanout` fence reads only `decoded.clone().stats()`. The `metadata()` pre-decode veto (SVG source never reaches a raster row) is unimplemented; the `Derive.Spec` roster has no metadata predicate. Wire a `spec.admit?: (m: Metadata) => boolean` row predicate over one `metadata()` lift.
- `sharp.format` / `sharp.versions` — `file.md` CODEC_GATE prose says "format capability gates through `sharp.format` at construction", but `_governed` composes only `block`/`cache`/`concurrency`. Add the `sharp.format` capability read so an unbuildable rendition row refuses at boot, and `sharp.simd(enable?)` as a governance lever beside `cache`/`concurrency`.
- `.tile(TileOptions)` — `file.md` growth clause names "a tile-pyramid rendition is a row whose terminal is `tile`", but `_fanout`'s terminal is hardcoded `toFormat(...).toBuffer(...)`. `tile()` writes a multi-file pyramid (`layout: dz|iiif|iiif3|zoomify|google`) — it needs a distinct terminal arm the fanout dispatches on `spec.terminal`. Genuine deep-zoom gap for the AEC large-image viewer (emphasis #9).
- `.composite(overlays: OverlayOptions[])` — growth names watermarking as a `composite` step; not wired. Row-driven `spec.composite?` step in the clone chain.

### A6. `Effect.cachedFunction` / `Effect.cachedWithTTL` — `libs/typescript/.api/effect.md` → `lane/cache.md`
`cache.md` `_tiers.memo` names the row "Effect.cachedFunction / cachedWithTTL" as the Tier-0 pure-recompute-avoidance posture, but `CacheLane` exports only `keyed`/`dedup`/`persisted`/`handle`/`map` — the memo tier is described, never surfaced. Add `CacheLane.memo = Effect.cachedFunction` / the TTL twin so the row is a real member, not prose.

### A7. `Pool.make` / `KeyedPool.make` — `libs/typescript/.api/effect.md` → `lane/cache.md` (consumed by the remote-origin owner, [B1])
`cache.md` POOLS surfaces `RcRef.make` (single ref-counted) + `RcMap.make` (keyed ref-counted). Neither is a bounded multi-instance pool. The remote-origin plane ([B1]) needs "bounded connection reuse via an Effect pool keyed by `{host,port,user}`" (prefetch-remote §a) — that is `KeyedPool.make` (min/max sizing + TTL over a scoped acquire), distinct from `RcMap`'s single-instance-per-key semantics. Surface `CacheLane.pool = Pool.make` / `CacheLane.keyedPool = KeyedPool.make` for the ssh2/ftp/dav connection lane.

### A8. `Multipart.schemaPersisted(schema)` / `Multipart.withLimits(opts)` — `libs/typescript/.api/effect-platform.md` → `object/stream.md` (BYTE_INGRESS)
`stream.md` cites `Multipart.toPersisted` + the `MaxFileSize`/`MaxParts` fiber-refs. `Multipart.schemaPersisted(schema)` gives a typed decode of the whole form (parts as a `Schema`-proven struct), and `Multipart.withLimits(opts)` is the composable bound vs. ambient fiber-ref mutation. Upgrade the form seam to the typed+bounded pair.

---

## [B] NEVER-USED ADMITTED CAPABILITY THE FOLDER CONCEPT DEMANDS

### B1. THE REMOTE-ORIGIN FILESYSTEM PLANE — `ssh2`, `webdav`, `basic-ftp`, `chokidar` are ALL admitted, ALL catalogued as `object/file` rows, NONE wired. (emphasis #8, the single largest gap)
Confirmed by the census (mismatch #1) AND by all four catalogs self-describing: ssh2.md "sibling remote-origin rows on the one origin-addressed `object/file` surface — capability flags dispatch"; webdav.md "a remote-origin backend row of the `object/file` plane beside the SFTP and FTP rows"; basic-ftp.md identical; chokidar.md "the local half of the watch strategy row; the remote halves live on the remote-origin rows". `object/file.md` implements ONLY local `Disk.intake/watch/stage/egress` over platform `FileSystem` — zero SFTP, FTP, WebDAV, SSH-exec, sync, or remote-watch. `prefetch-remote.md` is the CLOSED design (do not re-research): the rclone `fs.Fs` capability-flag model, ssh2 boundary-adapt shape, the two-transfer-engine policy (in-process ssh2 + external `Command` rsync/scp), the sync comparator rows, the remote-watch strategy rows. This is a genuine NEW OWNER with no home → **new page `object/remote.md`** (the campaign admits new files for earned concerns):
- Origin-addressed URI-scheme dispatch: `file:` / `sftp:` / `ssh:` / `ftp:` / `ftps:` / `webdav:` / `s3:` selects the backend row.
- Capability-flag rows per backend (rclone model): server-side `Copy`/`Move`, `PutStream`, `ChangeNotify`, hash types — polymorphic ops degrade on the flags (no server-side `Move` ⇒ `Copy`+`delete`).
- Polymorphic op surface (single entry, arity in input): `stat`, `list`, `read`(→Stream), `write`(←Sink), `copy`, `move`, `remove`, `mkdir`, `transfer`(origin→origin), `sync`(comparator + resume policy), `watch`(strategy row), `exec`(ssh/local only).
- Two transfer engines behind one policy surface: `ssh2` in-process (boundary-adapted — `Effect.acquireRelease` on `ready`/`end()`, `NodeStream.fromDuplex`/`fromReadable` + `NodeSink.fromWritable` lift channels/SFTP streams, `Effect.async`/`Stream.asyncPush` for discrete events) and external `rsync`/`scp`/`ssh` via `@effect/platform` `Command` (already-native `stdin: Sink`/`stdout: Stream`/`exitCode: Effect`).
- Resume policy rows: rsync `--partial --append-verify --inplace --checksum` (preferred, delta+resume+integrity in one), SFTP offset-resume (`stat` size → `open(path,'a'|'r+')` → positioned `read`/`write`), chunked-parallel `fastGet`/`fastPut` (`concurrency` 64, `chunkSize` 32768 — mined defaults, not wrapper imports).
- Sync engine (rclone bisync model): persist Path1/Path2 listings, diff on `size`+`modtime` (`--checksum`/`--size-only` as comparator policy rows), reconcile deltas with resync/recover on interrupt.
- Remote-watch strategy rows: ssh-exec push (`inotifywait -m -r` / `fswatch` over an `ssh2.exec` channel → `NodeStream.fromDuplex` → change `Stream`), SFTP poll (`Schedule`-driven `readdir`+`stat` mtime/size diff — universal default).
- Connection reuse via `KeyedPool.make` keyed `{host,port,username}` ([A7]).
- Every remote read feeds the SAME content-addressed intake fold (`Rail.identity` over `Rail.chunked`) — the origin row grows no addressing vocabulary. `webdav`/`basic-ftp`/`ssh2-sftp-client`/`node-ssh` Promise members are design-mined, adapted at the seam, never leaked (per admissions.md rejects).

### B2. `chokidar` `awaitWriteFinish` — CORRECTNESS DEFECT in current `Disk.watch`. (emphasis #8, latent bug)
`file.md` `_watch` rides `FileSystem.watch(dir)` filtering `_tag === "Create"` and pipes straight into `_intake` at `concurrency: 2`. Platform `FileSystem.watch` has NO write-settle guard, so **a file still being written emits `Create` and gets digested mid-write → the content key is minted over a truncated body → a wrong, non-idempotent key.** chokidar.md names this exactly: "digesting an unsettled file is the named defect." chokidar's `awaitWriteFinish: { stabilityThreshold, pollInterval }` holds `add`/`change` until size stabilizes, and `atomic` absorbs editor rename-swap artifacts. The intake-critical drop-directory watch MUST ride chokidar (admitted, zero current consumers): `watch(dir, { awaitWriteFinish, atomic, ignored: <predicate rows> })` → `Stream.asyncPush` over the `all` listener → the gated intake, released by awaited `close()`. Keep platform `FileSystem.watch` only for non-intake observation. This is both a never-used-package finding and a correctness fix.

### B3. `@effect/experimental` `RateLimiter.makeWithRateLimiter` / `layerStoreMemory` — never used in data. (emphasis #6, egress load-shed)
`effect-experimental.md` surfaces the store-backed distributed limiter (`algorithm: fixed-window | token-bucket`, `onExceeded: delay | fail`, `RateLimitExceeded` fault). The ObjectStore presign-grant mint, the browser-direct upload lane, the ClickHouse ingestion, and the OLAP `httpfs` range-reads have NO egress rate control. A per-app/per-tenant token-bucket over these egresses is exactly the "load-shed rides every egress owner" of emphasis #6, and the store-backed variant survives multi-replica (the distributed limiter the naive in-process one cannot be). Candidate for the ObjectStore `grant`/`putKeyed` and ClickHouse `insertQuery` seams. (Weigh against: rate-limiting is arguably an edge concern — but egress QUOTA on a shared object plane is a data-owned resilience property.)

### B4. `@effect/experimental` `Machine.makeSerializable` / `boot` / `snapshot` / `restore` — never used in data. (emphasis #4, crash-durable resume)
`stream.md` IDENTITY_FOLD's resume checkpoint is `{ offset, session }` — the tus offset PLUS the digest-session snapshot — and the page states "a resumed upload continues its identity fold from the verified byte." Today the digest-session checkpoint has no durable home (the page defers it: "persists only in the staging band's metadata"). A serializable durable `Machine` actor over the tus upload lifecycle (create→patch→resume→finalize) with `snapshot`/`restore` gives crash-durable resume of the identity fold across process restarts, and `Machine.Actor` is a `Subscribable` of upload state → wire to a UI progress feed (emphasis #4 + #9). Candidate for `object/stream.md` RESUME_RAIL. (Weigh: tus S3Store already persists byte offset; the Machine adds only the digest-session durability the page currently defers — real but bounded value.)

---

## [C] CROSS-STACKING PLAYS — package × package the corpus never attempts

### C1. `Effect` resilience algebra × EVERY backend owner (emphasis #6 — resilience as a concept, internalized)
The corpus applies `Effect.retry(_RETRY)` on exactly ONE seam: `fact.md`'s drain (`Schedule.exponential.jittered ∪ Schedule.spaced`). NOWHERE else. The ObjectStore (`put`/`get`/`sweep`/`grant`), the OLAP engine (`_query`), the ClickHouse `insertQuery`, and the sqlite snapshot IO carry NO retry, NO bulkhead, NO timeout, NO hedging — only the S3 client's opaque `maxAttempts`. Emphasis #6 is explicit: circuit breakers, bulkheads, hedging, load-shed, adaptive retry ride EVERY cache/store/backend/egress owner INTERNALLY so a consumer composes capability, never plumbing. The play is one owner-internal `_resilient` bracket per backend, composed from admitted primitives (no new package):
- adaptive retry: `Effect.retry(Schedule.exponential(base).pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(n))))` — already the `fold.md` `_RETRY` shape, generalized.
- bulkhead: `Effect.makeSemaphore(n)` gating the backend's concurrent ops (the ObjectStore multipart `concurrency: 4` and derivative `concurrency: 4` are ad-hoc knobs; a bulkhead makes it an owner property).
- timeout: `Effect.timeout(budget)` per op.
- hedging: `Effect.race` of a primary + a delayed replica read for latency-critical GETs.
- circuit breaker: effect has no core breaker primitive — the honest closure is a `Ref<{open,failures,openedAt}>`-backed gate the `_resilient` bracket consults, or a `Schedule` that trips open; note as the one primitive the branch must own locally (a `kernel/fault` degradation-budget concern the seams cite). This is the largest cross-cutting stacking opportunity after [B1].

### C2. `apache-arrow` × `duckdb-wasm` × `@aws-sdk/s3-request-presigner` — the zero-copy browser analytics loop (already partially present, completeable)
`olap.md` ARROW_WIRE + LAKE_ROWS wire `registerFileURL(name, url, DuckDBDataProtocol.HTTP, false)` over presigned grants and `conn.insertArrowTable`/`insertArrowFromIPCStream`. This is the right shape. The uncomposed edge: the presigned URL for the wasm range-read comes from `ObjectStore.grant(key, new GetObjectCommand(...))` — but `olap.md` never imports `ObjectStore`, so the browser-analytics-over-object-plane loop is described but not seamed. The play: `olap.md` LAKE_ROWS' browser row consumes `ObjectStore.grant` (a `data → data` internal compose) so "range-read Parquet is the browser's only remote source" is a wired path, not a prose promise. (The Arrow catalog defers full surface to `ui/.api` — the interchange seam is correctly minimal here.)

### C3. `PgClient.listen`/`notify` × `Reactivity` × `read/fold` daemon — the wake bus is single-composed, but the fan is not
`fold.md` `_wake` composes `PgClient.listen(Journal.channel(app))` ∪ spaced-poll under `haltStrategy: "both"`, and `live.md` FOREIGN_EDGE stamps `Reactivity.invalidate`. Correct. The uncomposed play: `append.md` `_publish` fires `pg.notify(_channel(...), String(journal.version))` carrying the VERSION as payload — but `fold.md`'s daemon ignores the payload and always re-pages from `checkpoint`. The notify payload (the new head version) could short-circuit the "is there work" probe (skip the claim transaction when `payload_version <= checkpoint`), collapsing empty wake cycles. Minor throughput play, real under high-fanout multi-lane deployments.

### C4. `SqlEventJournal` / `SqlEventLogServer` overlay × the browser local-first lane — bound but not exercised
`append.md` `_overlay` binds `SqlEventJournal.layer`, `SqlEventLogServer.layerStorage`/`layerStorageSubtle` — the SQL backing under the `@effect/experimental` EventLog overlay. This is the server half. The client half (`EventLog.schema`/`makeClient`, `EventLogRemote.layerWebSocketBrowser`, `EventLogEncryption.layerSubtle`) is browser-local-first and correctly belongs to `runtime/browser`. The seam to assert in `append.md`: the overlay bindings are the durable authority beneath the local-first client, never a second record of truth (`[R19]`) — the page's RELAY_ROWS law states this; confirm the improver keeps `layerStorageSubtle` (zero-knowledge, no `EventLogEncryption` dep) as the default for the untrusted-multi-tenant posture.

---

## [D] GAP CAPABILITIES — package + integration shape (improver weighs)

### D1. Object-side native retention GC via S3 lifecycle + tagging (closes A4, complements store.md CAS sweep)
Package: `@aws-sdk/client-s3` (already admitted). Shape: `store.md` REFERENCE_GC composes `PutBucketLifecycleConfigurationCommand` rules generated from `Retain.Policy` windows + `PutObjectTaggingCommand` writing the retention-class tag at put time, so windowed expiry runs S3-native (survives SQL-ledger loss) UNDER the CAS reference sweep (which owns the referential correctness the lifecycle rules cannot). Two-layer GC: native lifecycle for time-windowed classes, CAS sweep for reference-orphan reclamation.

### D2. Circuit-breaker owner (the one resilience primitive effect core lacks) — closes C1
Package: none (local kernel). Shape: a `Ref<BreakerState>`-backed gate (`closed | open{openedAt} | halfOpen`) with `Schedule`-driven half-open probes, composed into the `_resilient` bracket every backend owner rides. This is a `kernel/fault` concern the data seams cite ("ride `kernel/fault` degradation budgets") — the data folder consumes it, does not own it. Flag as an upstream dependency the RemoteFs + ObjectStore + Olap + ClickHouse owners all need.

### D3. Durable digest-session checkpoint for tus resume — closes B4
Package: `@effect/experimental` `Machine` (admitted). Shape: the tus upload as `Machine.makeSerializable` with state `{ offset, session: DigestSnapshot }`, `snapshot`/`restore` onto the staging band metadata via `Persistence`; the `Machine.Actor` `Subscribable` feeds a UI progress atom. Weigh: bounded value over the existing S3Store offset persistence — the delta is digest-session durability the page currently defers.

### D4. FastCDC chunk-level Merkle proof tree — self-declared RESEARCH in stream.md CHUNK_STAGE
Package: core `Digest` (`createBLAKE3` keyed-tree row — a `kernel` digest-table growth, not a data package). Shape: the `ChunkMark` receipts already carry per-chunk sub-keys; the proof tree folds them into an O(log n) verified-streaming read (`range.md` growth clause). This is a `kernel/digest` capability the stream rail consumes — flag upstream, not a data-owned build.

---

## [E] PER-PAGE INTEGRATION MAP — what the improver executes

| Page | Actions (ranked) |
| :--- | :--- |
| **object/file.md** | (1) `Disk.watch` → ride `chokidar` with `awaitWriteFinish`+`atomic`+`ignored` predicate rows via `Stream.asyncPush` over the `all` listener, awaited `close()` release — **fixes the mid-write-digest defect** [B2]. (2) Wire `metadata()` pre-decode veto as a `Derive.Spec.admit?` predicate; add `sharp.format`/`sharp.versions` capability gate + `sharp.simd` to `_governed` [A5]. (3) Add `spec.terminal` dispatch for `.tile(TileOptions)` deep-zoom + `spec.composite?` watermark step [A5]. |
| **object/remote.md** (NEW) | Author the full RemoteFs plane per prefetch-remote: URI-scheme backend dispatch, rclone capability-flag rows, polymorphic op surface, two transfer engines (ssh2 boundary-adapt + `Command` rsync), resume/sync/watch policy rows, `KeyedPool` connection reuse, content-addressed intake reuse [B1]. Consumes `ssh2`/`webdav`/`basic-ftp` + `@effect/platform` `Command`/`NodeStream`/`NodeSink`. |
| **object/store.md** | (1) Compose `PutBucketLifecycleConfigurationCommand` + `PutObjectTagging` native GC over `Retain.Policy` [A4/D1]. (2) Add `waitUntilObjectNotExists` post-delete waiter to `_sweepDelete` [A4]. (3) Internalize `_resilient` bracket (retry+semaphore+timeout+breaker) on `put`/`get`/`sweep`/`grant` [C1]. (4) `GetObjectAttributesCommand` for multipart checksum evidence; `CopyObjectCommand` for server-side rekey [A4]. |
| **object/stream.md** | (1) Typed+bounded form seam via `Multipart.schemaPersisted`/`withLimits` [A8]. (2) Weigh serializable `Machine` for crash-durable digest-session resume [B4/D3]. |
| **journal/append.md** | (1) `SqlClient.SafeIntegers` + bigint decode on `sequence`/receipt reads [A1]. (2) Confirm `layerStorageSubtle` overlay default for multi-tenant zero-knowledge [C4]. (3) notify payload carries head version for daemon short-circuit [C3]. |
| **read/fold.md** | (1) `SqlClient.SafeIntegers` on the daemon fiber — **the checkpoint is a global-sequence cursor, bigint-unsafe today** [A1]. (2) notify-payload short-circuit of empty wake cycles [C3]. |
| **journal/retain.md** | `SafeIntegers` on the `subject_journal.sequence` join [A1]. |
| **lane/olap.md** | (1) `streamAndReadAll`/`runAndReadUntil` reader-continuation → `Stream` — closes the RESEARCH tag [A3]. (2) LAKE_ROWS browser row consumes `ObjectStore.grant` [C2]. (3) `_resilient` on `_query`/ClickHouse `insertQuery` [C1]. |
| **lane/cache.md** | (1) `KeyValueStore.prefix(scopeKey)` on `persisted`/`backing` — tenant partition [A2]. (2) Surface `CacheLane.memo` (`Effect.cachedFunction`/`cachedWithTTL`) [A6]. (3) Surface `CacheLane.pool`/`keyedPool` (`Pool.make`/`KeyedPool.make`) for the RemoteFs connection lane [A7]. |
| **read/batch.md** | `KeyValueStore.prefix` on the `durable` persisted band [A2]. |
| **lane/clickhouse (olap)** + **fact.md** | `RateLimiter.makeWithRateLimiter` egress quota on ingestion/grant seams [B3]. |

Deferred/upstream (NOT data-owned): circuit-breaker primitive [D2] and BLAKE3 keyed-tree [D4] are `kernel/fault`+`kernel/digest` concerns the data seams consume; the census's 4 self-declared RESEARCH tags for `pg_incremental` create-pipeline spelling and `vchord_bm25` DDL are extension-reference (coding-pg / extension-source) closures, not stacking. project.json `plane:runtime` tag mismatch and empty IDEAS/TASKLOG are governance notes, not stacking targets.

Non-findings (verified already-correct, do not "fix"): the `onDialectOrElse{orElse,pg}` pattern is the correct 2-dialect split (five-arm `onDialect` is not warranted); the hand-rolled outbox `claimBatch`/`complete` is correct (must be transactional with the journal append — `SqlPersistedQueue` is a runtime/work overlay, not a substitute); the `sql.reserve`+`executeUnprepared` advisory-lock idiom in `fold.md` rebuild is correct (session-pinned); the Arrow catalog's deliberate minimal surface (defers to `ui/.api`) is correct.
