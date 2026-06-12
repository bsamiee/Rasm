# dynamicdata-changesets — bedrock

## source mechanics

- Two sources own all live collection state: `SourceCache<TObject, TKey>(keySelector)` for keyed sets, `SourceList<T>` for ordered sequences.
- Everything downstream of a source is a change-set stream (`IObservable<IChangeSet<...>>`) — the change-set is the only state transport; values never travel beside it.
- Reason vocabularies differ by shape: cache reasons are { `Add`, `Update`, `Remove`, `Refresh`, `Moved` }; list reasons add range forms { `AddRange`, `RemoveRange`, `Clear`, `Replace` }.
- Operators and adaptors dispatch on reason — reason fidelity is the contract; an operator that flattens ranges to singles multiplies downstream work by the range size.
- `Edit(updater)` batches every mutation inside the updater into one change-set emission under the source's internal lock.
- N separate `AddOrUpdate` calls emit N change-sets and N downstream recomputations — the rejected form.
- All multi-item mutation rides one `Edit`; the batch boundary is the emission boundary.
- The updater surface (`AddOrUpdate`, `RemoveKey`, `Clear`, `Refresh`, lookups) is the only mutation vocabulary; mutating items outside `Edit` and hoping consumers notice is the deleted pattern `AutoRefresh` exists to replace.
- Writes serialize under the source's lock — concurrent `Edit` calls from multiple writers are safe at the source.
- The consequence most miss: change-set emission rides the editing thread, still inside that serialization scope, so a heavy operator chain attached to `Connect` bills its full cost to every writer.
- Cost defers off the edit path through the batching windows below; thread marshaling itself is settled law elsewhere — downstream operators assume serialized change-set delivery and add no synchronization of their own.
- `Connect(predicate?, suppressEmptyChangeSets = true)` emits current state as the first change-set, then deltas.
- The connect predicate scopes the connection at the source — consumers receive only their slice, with no downstream filter state.
- `suppressEmptyChangeSets: false` is required only when a consumer keys on heartbeat-empty emissions; the default deletes empty-set noise.
- `Preview(predicate?)` observes change-sets BEFORE the cache state mutates — the veto and validation window companion to `Connect`'s after-image.
- Pairing `Preview` and `Connect` around one source yields before/after evidence per mutation with zero copying.
- `Watch(key)` is the per-key change stream — one key's lifecycle without filtering the whole set.
- `CountChanged`, `Items`, `Keys`, `KeyValues` are the snapshot reads.
- `Lookup(key)` returns an optional, never a sentinel — absence is a value at this surface.
- Under suspended notifications (`SuspendNotifications`, with `SuspendCount` as the count-only variant), mutations buffer and replay as one consolidated edge on resume; `SuspendCount` defers only the count stream, leaving change-sets flowing — the cheap variant when only count consumers are noisy.
- `Connect`'s predicate is fixed at connection time: dynamic membership criteria belong to the `Filter` rows downstream — re-`Connect`-ing to change criteria is the deleted form because it discards every downstream operator's accumulated state.
- `IIntermediateCache` is the keyed cache without a key selector — keys supplied per operation.
- `ChangeAwareCache` is the low-level mutation buffer that records reasons as it mutates and yields the captured change-set on demand — the authoring primitive for custom operators, never an application surface.
- A `Connect` made during suspension defers its initial emission until resume — bulk loads are one change-set by construction.
- `AsObservableCache(applyLocking = true)` on a change-set stream materializes a shared derived cache; the locking opt-out exists for chains proven single-threaded — connect-many, compute-once is the sharing law.
- Key stability is a contract: a key must be immutable for the object's cache lifetime — identity change is spelled remove-then-add inside one `Edit`, never an `Update` under a new key, because every downstream per-key index trusts the key.
- The key selector runs at edit time on every mutation: it is pure and cheap by contract, and an expensive derivation memoizes inside the object rather than recomputing per edit.
- Significance gating has three altitudes, and the law is to decide at the earliest one that can.
- Ingress altitude: the `EditDiff` comparer suppresses no-op updates before they exist.
- Propagation altitude: `IgnoreUpdateWhen` stops insignificant updates mid-chain.
- Egress altitude: value-level distinct gates at the delivery edge, owned by the streams lane.
- Gating late what could be gated early bills every intermediate operator for noise — the altitude choice is a cost decision, not a style one.

## edge-triggered whole-set re-resolution

- `EditDiff(items, equalityComparer | areItemsEqual)` computes the minimal add/update/remove delta against a full replacement set inside one `Edit` — the one-call whole-set swap.
- One root observation rides into one `EditDiff` and exits as one change-set.
- Per-control handlers reacting to the root and per-frame re-checks are the deleted forms — the swap consolidates what they fragment.
- The equality argument decides re-emission: items judged equal produce no change, so the comparer is the update-noise gate of the entire swap.
- The stream forms close the family: `IObservable<IEnumerable<T>>.EditDiff(keySelector, equalityComparer?)` turns a re-resolved-snapshot stream directly into a change-set stream with no intermediate source.
- The `IObservable<Optional<T>>.EditDiff` overload covers zero-or-one ownership — a presence stream becomes a one-key live set.
- A poller, a query refresh, or a configuration reload becomes a live set by appending one operator — re-resolution is a stream shape, not an imperative routine.
- The edge-trigger taxonomy is five rows; every global invalidation enters as exactly one:
  - whole-set: `EditDiff` — the replacement set is recomputed.
  - predicate-stream: `Filter(IObservable<Func<T, bool>>)`, or state-shaped `Filter(predicateState, (state, item) => bool)` — membership re-evaluates per emission; `reapplyFilter: IObservable<Unit>` forces re-application of an unchanged predicate.
  - regrouped: `Group(selector, regrouper)` and the observable-group-selector overloads — keying re-evaluates on the edge.
  - re-projected: `Transform(factory, forceTransform)` — projection re-runs on the edge.
  - per-item: `FilterOnObservable(item => IObservable<bool>, buffer?)` — each item carries its own membership stream, folded into set membership.
- The taxonomy is a partition, not a menu: choosing two rows for one invalidation double-fires the set.
- Above the five rows sits the source-swap row: `Switch` over `IObservable<IObservableCache<TObject, TKey>>` or `IObservable<IObservable<IChangeSet<TObject, TKey>>>` replaces the ENTIRE source while preserving the downstream operator chain — re-resolution of which set, where the five rows re-resolve which members; the previous source's contribution retracts as removals, the new source's state arrives as adds.
- Source-swap versus whole-set swap chooser: when the new state is a different store, query, or scope, `Switch`; when it is a recomputation of the same logical set, `EditDiff` — swapping sources for recomputed membership discards operator state for no reason, and diffing across genuinely different sources hides the provenance change.

## operator algebra

- Transform rows carry their refresh and re-projection policy in the signature: `Transform(factory, transformOnRefresh: bool)` declares whether a Refresh re-runs the projection.
- By default a Refresh does NOT re-run the projection — stale projections under item mutation are the silent default the flag exists to fix.
- Factory arities reach `(current, Optional<previous>, key)` — update-aware projection is declarative, not a cache of previous values beside the chain.
- `TransformAsync` (with `TransformAsyncOptions`) owns async projection inside the chain; `FilterImmutable` is the cheaper filter for items whose predicate inputs never mutate.
- `AutoRefresh(propertySelector?, changeSetBuffer?, propertyChangeThrottle?, scheduler?)` lifts item-level property notification into Refresh reasons.
- The two `AutoRefresh` knobs price two different noises: `propertyChangeThrottle` coalesces one item's burst; `changeSetBuffer` coalesces many items' refreshes into one change-set.
- `AutoRefreshOnObservable(item => IObservable<TAny>, changeSetBuffer?)` generalizes the refresh trigger to any per-item signal — the property-notification path is just its specialization.
- The refresh response matrix is the downstream law: `Filter` re-tests membership on Refresh; sorted consumers re-position; `GroupOnProperty`/`GroupOnPropertyWithImmutableState` re-key; `Transform` ignores Refresh unless `transformOnRefresh`.
- Reading a chain means reading its refresh column — the matrix decides which operators react to item mutation at all.
- `SuppressRefresh()` (= `WhereReasonsAreNot(Refresh)`) is the declared opt-out for refresh-noisy upstreams.
- Update-significance gates: `IgnoreUpdateWhen(predicate)` and `IncludeUpdateWhen(predicate)` suppress or admit Update propagation by comparing current and previous; the reference-equality overload dedups re-publication of identical instances — significance is declared once at the noise source, never re-checked per consumer.
- Join rows: `InnerJoin` keys its output on the composite `(leftKey, rightKey)` tuple — output identity multiplies, by design.
- `LeftJoin` and `FullJoin` key on the left key with `Optional` sides — the left-identity-preserving rows.
- `RightJoin` keys on the right key with an optional left — the mirror row, not a left-keyed variant.
- The `*JoinMany` family aggregates the right side into a grouping per left key — one-to-many composition without a nested cache.
- Join chooser: preserve an identity → the side whose key survives; relation-as-entity → `InnerJoin` with the tuple key owned downstream.
- `MergeMany(item => IObservable<TDestination>)` flattens per-item value streams into one stream.
- Membership controls subscription in the merge family: removal disposes the child subscription automatically — no leak window between member exit and unsubscribe.
- `MergeManyChangeSets(item => IObservable<IChangeSet<child>>, equalityComparer?, comparer?)` flattens per-item CHANGE-SET streams into one child set.
- Comparer-based conflict resolution adjudicates when multiple parents emit the same child key — the duplicate-key policy is declared, never first-writer-wins by accident.
- The `sourceComparer` + `resortOnSourceRefresh` overloads decide which parent wins and whether a parent Refresh re-adjudicates — a two-level cache hierarchy as one operator.
- Receipted bounding rows: `ExpireAfter(timeSelector, pollingInterval?, scheduler?)` on a source returns the stream of removed key-value pairs — expiry receipts, not just removals.
- A null per-item expiry time means never-expires — retention is per-item data, not a cache-wide constant.
- The polling-interval overloads trade timer count for expiry precision: one poll timer amortizes N item expirations at the cost of expiry granularity.
- `LimitSizeTo(sizeLimit, scheduler?)` on a source likewise returns the evicted pairs.
- The in-chain stream forms of both remove without that receipt stream — chooser: source forms where loss must be receipted; chain forms where expiry is pure projection policy.
- Membership-bound resources: `SubscribeMany(item => IDisposable)` ties a subscription to membership; `DisposeMany()` disposes removed items; `AsyncDisposeMany(disposalsCompletedAccessor)` exposes the async-disposal completion stream through the accessor.
- The accessor is the drain hook: a cache-owned resource set participates in drain as (stop edits, flush batching windows, await the disposals-completed stream).
- `OnItemAdded`, `OnItemUpdated`, `OnItemRemoved(action, invokeOnUnsubscribe = true)`, `OnItemRefreshed` are the membership side-effect hooks, with teardown symmetry as a declared flag.
- `Cast(converter)` re-types a cache without re-keying; `WhenValueChanged`/`WhenAnyPropertyChanged` lift item property streams out of a change-set stream for value-level composition.
- `DistinctValues(valueSelector)` projects a cache into a live distinct value set (`IDistinctChangeSet<TValue>`) — the vocabulary-extraction row.
- The distinct value set is the input the distinct-group `Group(selector, resultGroupSource)` overload consumes to pre-declare the group universe.
- `PopulateInto(destination)` pumps a change-set stream into another source or intermediate cache, returning the pump as a disposable.
- The pump serves staged pipelines across ownership boundaries where the destination must remain independently editable; the rejected alternative is subscribing and hand-copying edits.
- `SourceList` convenience mutations (`Add`, `AddRange`, `Insert`, `InsertRange`, `Move`, `RemoveAt`, `RemoveMany`, `RemoveRange`, `Clear`, and the list-form `EditDiff(allItems, comparer?)`) each wrap exactly one `Edit`.
- A loop of conveniences is N change-sets — the one-`Edit` law governs lists identically: batch loops inside one `Edit`.
- `RemoveMany` exists because N single removals inside one `Edit` still beat N `Remove` conveniences outside it — the convenience surface is for single mutations, the updater surface for plural ones.
- `GroupWithImmutableState`/`GroupOnPropertyWithImmutableState` emit immutable group snapshots where the plain forms expose live inner caches — chooser: immutable snapshots for cheap equality-based re-publication and value semantics; live groups for nested operator chains per group.

## derived projections and reduction

- The sorted-binding law is `SortAndBind`: the legacy `Sort`-then-`Bind` pair is marked superseded in favor of it.
- `SortAndBind(target, comparer | SortAndBindOptions)` owns comparison, binding, and reset thresholds in one declaration; the option record's field semantics belong to the binding consumer's layer.
- Sorted change-sets (`ISortedChangeSet`) remain the required input shape for windowed projection — sorting is a prerequisite row, not an optimization.
- Windowed projection is request-driven: `Page(IObservable<IPageRequest>)` and `Virtualise(IObservable<IVirtualRequest>)` consume a sorted stream plus a request stream.
- The scroll position or pager is itself an observable: the window re-projects per request, and the response context describes the effective window granted.
- The UI binding of windows is owned elsewhere; the mechanics here are sorted input, request stream in, paged or virtual change-set out — a moving window over a large live set with no per-frame re-query.
- Reduction rows collapse set state into scalar streams: `QueryWhenChanged(query => result)` projects against the full internal state per change-set — the escape hatch into arbitrary set-level computation.
- `ToCollection()` materializes a read-only snapshot per change; its dual `EditDiff` re-enters change-set space from snapshot space.
- Aggregation (`Count`, `Sum`, `Avg`, `Max`, `Min`, `StdDev`) folds incrementally over aggregate change-sets — live scalars without re-enumeration: add and remove deltas adjust the scalar, and the moment-carrying rows (`Avg`, `StdDev`) maintain running state rather than recomputing.
- `QueryWhenChanged`'s query argument exposes the full keyed state (count, items, key lookups) without copying it — set-level computations read through the query and emit only their result, which is what keeps arbitrary set logic from forcing `ToCollection` materialization.
- `TrueForAll`/`TrueForAny(item => IObservable<TValue>, condition)` fold per-item observable values into one boolean stream — live invariants over the set without re-querying it.
- `BindToObservableList` projects a change-set stream into an `IObservableList<T>` — a data-side ordered projection target for non-binding consumers; the UI-collection adaptors beside it are owned elsewhere.
- `DeferUntilLoaded` delays emission until real state exists; `StartWithEmpty` (typed overloads across plain, sorted, virtual, paged, and grouped shapes) seeds combinators that require an initial emission — the two startup rows that prevent both premature-empty rendering and combinator deadlock.
- Change-set-level batching windows: `Batch(timeSpan, scheduler?)` time-slices emissions into consolidated change-sets.
- `BatchIf(pauseIndicator, initialPauseState?, timeOut?, timer?, scheduler?)` gates emission on a boolean stream with an optional flush timeout and an external flush timer — suspend-while-busy as a declared stream, with the timeout bounding worst-case staleness.
- `BufferInitial(initialBuffer, scheduler?)` folds the startup burst into one opening change-set, then goes live.
- These windows coalesce change-sets — consolidating deltas; value-level cadence rows belong to the streams lane, and the two coalescing altitudes never substitute for each other.
- Every time-taking operator in the family (`ExpireAfter`, `LimitSizeTo`, `Batch`, `BatchIf`, `AutoRefresh`, `BufferInitial`) accepts a scheduler.
- An omitted scheduler resolves to a library-global default — time logic silently escapes virtualization through that global unless every scheduler is pinned explicitly; the clock plane law holds here exactly as in the streams lane.
- `MergeManyChangeSets` also carries list-shaped destination overloads (child sets without keys) — the parent-child flattening law covers both collection shapes through one operator family.
- Reason filtering is a general row, not only the refresh opt-out: `WhereReasonsAreNot` (and its inclusion dual) scopes a consumer to the change classes it can act on — an adds-only indexer or a removals-only janitor declares its reason set instead of switching on reasons per change.
- The library's optional carrier surfaces at `Lookup`, join sides, and the optional `EditDiff` ingress; it projects to the canonical absence carrier at the boundary seam — interior code consumes it where the operator hands it over and never converts mid-chain.
- List-only set algebra: `Or`, `And`, `Xor`, `Except` compose change-set streams into derived membership.
- The operand collections themselves can be live (`IObservableList<IObservableList<T>>`, `IObservableList<ISourceList<T>>`) — adding a source to the operand list extends the union with zero new wiring.
- Derived membership from N sources is one declared expression; hand-merging source lists into a target re-implements the algebra imperatively and loses the dynamic-operand row.
- `BufferIf` is the list-side emission gate, mirroring the cache-side `BatchIf`.
- Admission edges into change-set space: `ToObservableChangeSet` spans observable collections and read-only observable collections (plain and keyed) and plain value streams (`IObservable<T>` with expire and size-limit knobs).
- The admission edges mean existing collection-shaped and stream-shaped state enters the algebra without rewriting its producer.
- `ObserveCollectionChanges` is the lower edge beneath them — raw collection-change events as a stream, for the rare consumer that needs events rather than change-sets.
- Diagnostics carriers (`ChangeStatistics`, `ChangeSummary`) make chain behavior itself observable — change-rate evidence rides values, not logging side channels.
- The diagnostic stream is itself a change-set derivative: chain health folds from the same transport as chain data, so observing a chain never requires instrumenting it.

## divergent

- Relational-algebra closure: the operator set is a complete relational algebra over live sets — select (`Filter`), project (`Transform`), join (the join family), group (`Group*`), window (`Page`/`Virtualise`), aggregate (the reduction rows).
- The design rule the closure licenses is total: any derived view is ONE declared chain off ONE `Connect`.
- Materializing an intermediate (snapshot, re-source, re-Connect) to simplify a chain is the deleted form: it severs the incremental delta path and reintroduces whole-set recomputation.
- Chain cost is per-operator keyed state — each stateful operator holds a per-item index — so memory is operator-count × set-size, sharing via `AsObservableCache` at the widest fan-out point is the only lever, and chain length is a budgeted quantity, not free composition.
- The cost model refines by operator class: grouping adds per-group inner state, joins hold indexes on both sides, and the merge-many family holds one subscription per parent item — a chain audit prices each stateful operator against set cardinality before it ships.
- Cache-owned drain is an ordered four-step fold: stop edits at the source; release batching gates (`BatchIf` resume or its flush timer) so buffered change-sets emit; let the receipted bounding rows close their receipt streams; await the `AsyncDisposeMany` completion stream — the band's contribution to process drain, with every step's evidence already a declared stream.
- Refresh-semantics matrix as the page's decision table: operator × Refresh response (re-test | re-position | re-key | ignore-unless-declared | suppress) is closed and fits one table.
- A mutation-rich domain reads its chains against the matrix once at design time instead of discovering stale projections at runtime.
- The matrix also bounds `AutoRefresh` blast radius: property noise × matrix row = the recomputation bill, which `propertyChangeThrottle` and `changeSetBuffer` price explicitly — refresh cost is declared, not discovered.
- Receipted-loss participation: expiry receipts and size-eviction receipts from the source-form operators are the cache's contribution to the process-wide loss identity — set ingress = membership + receipted expiry + receipted eviction.
- The identity makes retention policy auditable from declarations exactly as lane loss is; the chain forms are the rejected spelling wherever the identity must close.
- Whole-set swap as the universal invalidation spelling: the five edge-trigger rows compose with one root observation upstream — one configuration stream, one capability stream.
- Global re-resolution is therefore always (root stream) → (one of five rows) → (one change-set), and the audit question "what re-fires when X changes" has a one-line answer per X.
- Foreclosed invalidation form: per-consumer root subscriptions — N re-fires, no consolidation.
- Foreclosed invalidation form: re-`Connect` on invalidation — loses all operator state.
- Foreclosed invalidation form: hand-written snapshot diffing — re-implements `EditDiff` without its minimality guarantee.
- Foreclosed invalidation form: flag-and-poll re-checks — the form the request-driven window rows delete.
