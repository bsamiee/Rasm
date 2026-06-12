# query-bulk-rail — bedrock

## store-op rail

- One op algebra, no repositories: store operations form one closed request family dispatched by one rail; a repository-per-aggregate layer is the rejected form — it multiplies surfaces while every method body repeats the same bracket.
- Arity discriminates on the input value, never on name suffixes: a single key resolves to an optional value, a key set to a batch read, a predicate plus cursor to a page, a predicate alone to a stream.
- Every op runs inside one pooled-context bracket: acquire from the context factory, execute, return; the bracket owns disposal, save, and fault conversion to the typed rail at its boundary; no context instance escapes it.
- The bracket is where store exceptions become typed rejections; interior op bodies never see provider exceptions.
- The execution strategy composes the bracket, not the op: `IExecutionStrategy.Execute<TState, TResult>(state, operation, verifySucceeded)` and its `ExecuteAsync` twin are state-threaded — pass the op's inputs as `TState` with static lambdas so retries re-run a closed value, not a captured closure.
- The strategy instance arrives from the profile row; the rail consumes it, never constructs it.
- Strategy-transaction trap with its boundary: under a retrying strategy, a transaction opened outside `Execute` poisons every retry — the whole bracket (begin, ops, commit) lives inside the strategy callback, with `verifySucceeded` supplied for non-idempotent tails.
- Retry-ambiguity law for receipts: an ambiguous commit can re-execute the op, so delta-shaped work (increment-style setters) double-applies under retry — write state-shaped setters where possible, and where not, the `verifySucceeded` probe is mandatory, not optional.
- Tracking is an op-class decision, not a call-site decision: read ops default to no-tracking; `AsNoTrackingWithIdentityResolution` is the row for projections that must alias repeated entities; the tracked path exists only inside unit-of-work ops that end in a save.
- A read op returning tracked entities is a leak of the bracket's interior — the tracker is bracket state, not a return channel.
- Projection law: ops return value projections (`Select` into typed shapes), never entities across the rail boundary; entity egress couples every consumer to the model and drags the tracker across the seam.
- The stream arity materializes as `AsAsyncEnumerable` consumed inside the bracket: the bracket must span the enumeration, because a live enumerable returned after the context goes back to the pool enumerates over reclaimed state.
- A stream op therefore either folds to its result inside the bracket or hands rows off through a channel under a declared policy — returning the raw enumerable is the lifetime bug.
- Every rail op stamps provenance: `TagWith(opName)` from the op's own symbol, `TagWithCallSite` where file/line provenance pays — store-side plan inspection then maps one-to-one to rail ops.
- Query shape stability is a rail invariant: op templates parameterize all values — one cached plan per op; dynamically composed predicates must produce parameter nodes, not constant nodes, or every execution recompiles and pollutes both caches.
- `EF.Constant` is admitted only as a declared policy row for provably low-cardinality hot filters — a per-op decision recorded with the op, never an inline habit.
- Proven hot ops graduate to compiled delegates (`EF.CompileAsyncQuery`) as rows in an op table — the delegate bypasses cache lookup entirely; the graduation is measured, not authored.
- First-class outer joins delete the old idiom: `Queryable.LeftJoin` / `RightJoin` (outer, inner, outer-key selector, inner-key selector, result selector) translate directly; the `GroupJoin`/`SelectMany`/`DefaultIfEmpty` construction is the rejected spelling.
- The raw seam is parameterized by type: `FromSql(FormattableString)` turns every interpolation hole into a parameter; `FromSqlRaw` admits only sanitized fragments, and string concatenation inside a raw call is an analyzer-flagged defect.
- Non-entity statements ride the facade family — `ExecuteSql(FormattableString)` / `ExecuteSqlAsync` for parameterized maintenance statements — inside the same bracket, never on a side connection.

## keyset pagination

- Offset pagination is the rejected form twice over: cost grows linearly with page depth (the store scans and discards the skip), and concurrent writes shift page boundaries so consumers see duplicates and gaps. The rail's page op is keyset-only.
- Keyset law: order by a stable column tuple whose suffix is a unique tiebreaker (the aggregate's key selector — derivation arrives settled), and predicate from the last-seen tuple.
- The predicate spelling is the lexicographic expansion — `(k₁ > a) || (k₁ == a && k₂ > b)` — because tuple row-value comparison does not translate from the operator surface.
- Cursor values bind as parameters, so page depth never changes the SQL shape — keyset paging is plan-stable by construction, completing the rail's shape-stability invariant.
- Descending lanes flip the ordering and every comparison together — a half-flipped lane silently returns the first page forever.
- The cursor is the projected ordering tuple of the last row, opaque to callers: callers hold a token, never column values.
- The page op's input arity is `Option<cursor>` — absent means first page — so pagination adds zero extra entrypoints to the rail.
- Cursor validity is bounded by the ordering tuple's schema lifetime: a cursor minted before an identity-axis flip expires with the dual-key window; the rail surfaces this as a typed stale-cursor rejection, never an empty page.
- Index law: the ordering tuple must be a contiguous index prefix or the keyset predicate degenerates to scan-plus-sort — the page op and its covering index are one declaration reviewed together; a page op without its index is half a feature.

## set-based update and delete

- `ExecuteUpdate(Action<UpdateSettersBuilder<TSource>>)` / `ExecuteUpdateAsync(...)` and `ExecuteDelete()` / `ExecuteDeleteAsync()` are the set-based lanes on any filtered queryable: one statement, no materialization, affected-count receipt.
- The setter builder is statement-bodied: `SetProperty(selector, value)` and `SetProperty(selector, valueExpression)` calls compose conditionally inside the lambda — a plain `if` adds a setter — deleting the expression-tree surgery the expression-typed shape forced.
- Setters reach inside document columns: properties of complex-type JSON mappings are legal `SetProperty` targets, translated to in-place document modification.
- Owned-entity JSON mappings are foreclosed from the set-based lane — the model-shape decision arrives settled; this lane only inherits its consequence.
- Two-rail consistency law: set-based ops bypass the change tracker and the unit-of-work interceptor altitude entirely — tracked instances in any live bracket go silently stale, and no save-spine fact is emitted.
- The rail therefore confines set-based ops to brackets that own no tracked graph; expecting the save spine to observe them is the canonical integration bug.
- Every set-based op self-emits its facts and its invalidation (the tag-cut) as part of the same op — self-emission is the structural consequence of spine blindness, not a style preference.
- The affected count is a first-class receipt, not a debug value: zero-affected where the predicate proved one-or-more is a typed concurrency signal — the row moved between read and update; rails fold it, never discard it.

## bulk and merge

- Bridge composition: `Initialize()` once at composition activates the bridge; `ToLinqToDB()` hands any rail queryable to the deeper translator inside the same model and connection; `GetTable<T>()` opens the bulk-side table surface.
- `EnableChangeTracker` gates the bridge's tracker integration off for read lanes — the bridge defaults to honoring tracking; the rail's no-tracking read posture turns it off explicitly.
- Bridge options are explicit profile declarations: `AddMappingSchema`, `AddInterceptor`, `AddCustomOptions` on the bridge options builder, read back via `GetLinqToDBOptions` — silent schema or interceptor injection outside the profile row is the rejected form.
- The bridge is a lane of the one rail — a second public query surface built on it is the rejected form.
- The bridge and rail operator sets collide on materialization verbs; the suffixed pairs (`ToListAsyncLinqToDB` / `ToListAsyncEF` and siblings) exist to break the ambiguity — a file importing both surfaces names its lane explicitly instead of relying on resolution order.
- Transaction law — the enlistment fact: the bridge connection enlists in the rail's current transaction by default (`CreateLinqToDBConnection(context, transaction = null)` resolves `Database.CurrentTransaction` and rides it).
- `CreateLinqToDBConnectionDetached` is the explicit opt-out row — a bulk lane that must not share the ambient transaction says so in its signature, never by accident.
- The bulk path's atomic unit follows from enlistment: begin on the facade → bulk op → changefeed rows → tag-cut rows → commit; changefeed and invalidation are in-transaction by construction, so a crash leaves everything or nothing — no fact-without-row or row-without-fact state exists.
- `BulkCopy`/`BulkCopyAsync` take an entity collection or async stream and return a `BulkCopyRowsCopied` receipt — `RowsCopied`, `StartTime`, and a writable `Abort`.
- One options record governs the lane: `BulkCopyType` (`Default`/`RowByRow`/`MultipleRows`/`ProviderSpecific` — provider-native ingestion where the engine has one, multi-row values otherwise), `MaxBatchSize`, `BulkCopyTimeout`, `KeepIdentity`, `UseParameters`/`MaxParametersForBatch`, `TableOptions`, `ConflictAction`.
- `NotifyAfter` + `RowsCopiedCallback` is the progress channel; setting `Abort` inside the callback is the cancellation lever mid-copy.
- `UseInternalTransaction` is the rejected knob on enlisted lanes: it opens the copy's own transaction and conflicts with the ambient one the rail provides — legal only on detached-connection lanes.
- The name overrides (`TableName`, `SchemaName`, `ServerName`, `DatabaseName`) redirect ingestion to a staging table without remapping the entity — stage-then-merge is the two-step reconciliation for engines whose conflict vocabulary is too narrow for direct ingestion.
- `MaxDegreeOfParallelism` parallelizes the copy itself — a per-op budget consumed from the suite's parallelism budget, never an independent pool.
- `KeepIdentity` is the client-minted-key row: bulk ingestion under the time-ordered-surrogate identity row requires it, or the store re-mints and the admission-time identity is lost.
- `ConflictAction.Ignore` is the bulk-upsert-ignore row: ingestion rewrites as conflict-tolerant insert (`ON CONFLICT DO NOTHING` / `INSERT OR IGNORE` class).
- The ignore row is doubly gated: valid only under `MultipleRows` and only on engines that spell it — a capability row resolved at composition, natural partner of the content-hash identity row (idempotent re-ingestion).
- The merge algebra is one fluent total order: source binding (`Merge(target)` + `Using(queryable | enumerable)` / `UsingTarget()`, or `MergeInto(source, target)`) → match condition (`On(targetKey, sourceKey)` | `On(predicate)` | `OnTargetKey()`) → when-clause rows → terminal `Merge()`/`MergeAsync()` returning the affected count.
- The when-clause vocabulary is closed and two-sided: `InsertWhenNotMatched[And]`, `UpdateWhenMatched[And]`, `UpdateWhenMatchedThenDelete` / `UpdateWhenMatchedAndThenDelete`, `DeleteWhenMatched[And]`, `UpdateWhenNotMatchedBySource[And]`, `DeleteWhenNotMatchedBySource[And]` — the by-source rows make full two-sided reconciliation one statement.
- `Using(IEnumerable)` admits client-side collections as merge sources — the reconcile-a-batch op needs no staging table.
- Merge-with-output is the changefeed-in-same-statement form: `MergeWithOutput(Expression<Func<string, TTarget, TTarget, TOutput>>)` projects `(action, deleted, inserted)` — the four-parameter overload adds the source row — streaming as `IEnumerable` / `IAsyncEnumerable`.
- `MergeWithOutputInto(outputTable, ...)` lands the projection in a table inside the same statement — the zero-roundtrip spelling of the in-transaction changefeed: action discriminant plus before/after images, produced by the statement that caused them; no second query, no log mining, no trigger.
- The non-merge output family completes the lane: `InsertWithOutput` (the inserted row or a projection — round-trip-free observation of store-computed columns), `UpdateWithOutput` (old/new pairs), `DeleteWithOutput()` (the deleted rows — tombstone capture inside the delete), each with `...Into` table-landing variants.
- Output return shape is arity-discriminated like the rail itself: single-row forms return the value directly, set-shaped forms stream — the same input-shape law, surfacing in the return type.
- `InsertOrUpdate(insertSetter, onDuplicateKeyUpdateSetter, keySelector?)` is the two-lambda upsert row where the merge lane is structurally absent; a null update setter spells insert-or-ignore.
- Lane gating arrives settled from the profile axis: merge and output lanes compose only where the profile row carries them; op code never probes for support.
- Every bulk-side composition renders to SQL without executing: `ToSqlQuery(options?)` on queryables, updatables, insertables, and mergeables returns the statement as a value — the audit receipt for CI plan review and the dry-run spelling for gated destructive lanes.
- Set-op composition rows: `AsCte(name?)` names a queryable as a common table expression for multi-reference set ops; `QueryName` stamps the bulk-side query for provenance symmetry with the rail's tagging.

## cache port and store-state folds

- The rail's read ops accept one read-through cache port as a decoration row — the backing enters via the storage-contribution seam; the rail owns only the port's law, never the cache mechanics.
- Tag grammar is closed: tags derive mechanically from lane keys plus admitted owner keys (the key-selector derivation arrives settled).
- Free-string tags are rejected at admission — an underivable tag is uninvalidatable by construction, so the grammar rejection is the invalidation guarantee.
- Logical tag-cut is distinct from physical delete: a tag-cut expires cached projections for a lane; a delete is a store op that, like every write, emits its own tag-cut.
- Conflating the two — invalidating by deleting, or deleting by invalidating — collapses two lifetimes into one and is the port's primary rejected form.
- Cross-process first-level convergence is TTL-bounded with no backplane: each process's near cache converges by natural expiry or by the next tag-cut it observes.
- The TTL is the declared staleness ceiling for unsynchronized readers — a budget chosen per lane; lanes that cannot tolerate the ceiling bypass the near tier rather than shrinking the suite-wide TTL.
- Snapshot/restore-on-activate merge law: on activation, live store rows are authoritative for existence — a persisted view-state entry whose key has no live row is pruned; persisted view state for keys surviving the alive-prune merges onto the live rows.
- Existence flows one way (store → view), view state flows the other (snapshot → surviving keys); inverting either direction resurrects deleted rows or discards user state.
- Checkpoint cadence is one flush fold registered three ways — periodic schedule entry, on-drain participant, on-capture contributor — three registrations, one fold, zero per-trigger flush variants.
- The fold computes a content hash of the flush payload and skips when unchanged — idempotent flush as a hash check, making three-way registration safe to over-trigger.
- The hash, not a dirty flag, is the skip authority: flags drift across concurrent writers; hashes cannot.

## divergent

- store-op-rail — the one op law: every store interaction is a value in one closed request family, executed by one bracket that composes pool, strategy, transaction, tracking posture, provenance stamp, and fault conversion as policy rows — a new operation is a request case plus a dispatch arm; a new cross-cutting concern (slow-op threshold, read-replica route) is one bracket row touching zero ops. Foreclosed forms: repository interfaces (surface multiplication), per-op context ceremony (the bracket owns acquisition), service-layer try/catch translation (the boundary owns conversion), entity-returning queries, raw-enumerable stream egress, offset paging, `GroupJoin` outer-join scaffolding, raw-SQL concatenation. The deep interaction: arity discrimination and keyset pagination are one design — the page op's `Option<cursor>` input is the same shape-discrimination law that separates single from batch reads, so point reads, batches, pages, and streams are arities of one entrypoint, not four ops. Quantitative posture: compiled-delegate graduation pays in proportion to expression-tree depth and is worthless below it; the rail makes graduation a per-op-table row so measurement decides, not authorship.
- bulk-changefeed — the one write-mass law: high-volume mutation is one lane with three intensities — set-based statement (predicate-shaped work), bulk copy (collection-shaped ingestion), merge (source-against-target reconciliation) — all enlisted in the rail's ambient transaction, all receipted (affected count, rows-copied, output stream), and all self-emitting: the statement that mutates also produces the facts (output projections) and the invalidation (tag-cut rows) before commit. Maximal collapse: the changefeed is not a system beside the store — it is the output clause of the write that caused it, landed in-transaction; this deletes change-data-capture infrastructure, the dual-write risk of polled outboxes, and trigger maintenance in one move, and the `...Into` variants make it zero-roundtrip. Failure taxonomy with boundaries: tracker staleness (set-based ops bypass tracked graphs — confine or invalidate); spine blindness (no save-spine facts for bulk lanes — self-emission is mandatory, not optional); conflict silence (`ConflictAction.Ignore` drops conflicting rows without error — pair it with a rows-copied-versus-source-count reconciliation receipt or losses are invisible); abort semantics (`Abort` inside the progress callback stops the copy mid-stream inside the transaction — the bracket treats an aborted copy as a rollback signal, never commits a prefix); clause-order semantics (merge when-clause rows evaluate in declaration order — a `DeleteWhenMatched` declared before `UpdateWhenMatched` deletes what the update would have claimed; clause order is semantics, not style).
