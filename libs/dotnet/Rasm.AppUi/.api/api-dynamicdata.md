# [RASM_APPUI_API_DYNAMICDATA]

`DynamicData` owns the live change-set pipeline: a keyed cache or ordered list mutates through `Edit`, one `Connect()` fans an `IChangeSet` stream, and every query, bind, and aggregate operator folds that stream into a projection a screen binds. One cache is the single source of truth, each surface a projection off it, never a parallel mutation path.

## [01]-[PUBLIC_TYPES]

[CACHE_AND_LIST_TYPES]: mutable and observable live-data sources

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :--------------------------------- | :------------ | :------------------------------ |
|  [01]   | `SourceCache<TObject,TKey>`        | class         | keyed mutable source            |
|  [02]   | `SourceList<T>`                    | class         | ordered mutable source          |
|  [03]   | `ISourceCache<TObject,TKey>`       | interface     | keyed source contract           |
|  [04]   | `ISourceList<T>`                   | interface     | ordered source contract         |
|  [05]   | `IObservableCache<TObject,TKey>`   | interface     | read-only observable cache      |
|  [06]   | `IObservableList<T>`               | interface     | read-only observable list       |
|  [07]   | `IIntermediateCache<TObject,TKey>` | interface     | detached intermediate cache     |
|  [08]   | `ChangeAwareCache<TObject,TKey>`   | class         | change-tracking cache primitive |

[CHANGE_SET_TYPES]: change records and stream contracts

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `IChangeSet<T>`                                 | interface     | list change-set                                     |
|  [02]   | `IChangeSet<TObject,TKey>`                      | interface     | cache change-set                                    |
|  [03]   | `Change<T>`                                     | struct        | one list change                                     |
|  [04]   | `Change<TObject,TKey>`                          | struct        | one cache change                                    |
|  [05]   | `ChangeReason`                                  | enum          | cache change reason                                 |
|  [06]   | `ListChangeReason`                              | enum          | list change reason                                  |
|  [07]   | `ISortedChangeSet<TObject,TKey>`                | interface     | sorted change-set, `SortedItems` order snapshot     |
|  [08]   | `IGroupChangeSet<TObject,TKey,TGroup>`          | interface     | `IChangeSet<IGroup<TObject,TKey,TGroup>,TGroup>`    |
|  [09]   | `IImmutableGroupChangeSet<TObject,TKey,TGroup>` | interface     | `IChangeSet<IGrouping<TObject,TKey,TGroup>,TGroup>` |
|  [10]   | `IPagedChangeSet<TObject,TKey>`                 | interface     | paged change-set, `Response` bounds                 |
|  [11]   | `IVirtualChangeSet<TObject,TKey>`               | interface     | `ISortedChangeSet` plus `Response` bounds           |

- `Change<TObject,TKey>` carries `Reason : ChangeReason`, `Key : TKey`, and `Current : TObject` populated on EVERY reason — the removed object on `Remove` — beside `Previous : Optional<TObject>` (DynamicData's own `Optional`, populated on `Update` alone) and the `CurrentIndex`/`PreviousIndex` pair.

[BINDING_TYPES]: UI binding targets and adaptors

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------ | :------------ | :------------------------------------------ |
|  [01]   | `ObservableCollectionExtended<T>`                 | class         | suspendable bound collection (Avalonia/WPF) |
|  [02]   | `IObservableCollection<T>`                        | interface     | bound-collection contract                   |
|  [03]   | `ObservableCollectionAdaptor<T>`                  | class         | list bind adaptor                           |
|  [04]   | `ObservableCollectionAdaptor<TObject,TKey>`       | class         | cache bind adaptor                          |
|  [05]   | `SortedObservableCollectionAdaptor<TObject,TKey>` | class         | sorted bind adaptor                         |
|  [06]   | `BindingOptions`                                  | record struct | bind reset/replace policy                   |
|  [07]   | `SortAndBindOptions`                              | record struct | fused sort+bind policy                      |

- `BindingOptions`/`SortAndBindOptions`: a batch past `ResetThreshold` fires one collection reset (the virtualization-friendly path); `BindingOptions.NeverFireReset()` forces incremental notifications for a control that mishandles `Reset`, and `SortAndBindOptions.UseBinarySearch` inserts by binary search when the comparer is pure.

[QUERY_TYPES]: sort, page, virtual, aggregate, and diagnostic models

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                                       |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `SortExpressionComparer<T>`         | class         | multi-key comparer builder                                                         |
|  [02]   | `SortExpression<T>`                 | class         | one sort key and direction                                                         |
|  [03]   | `PageRequest`                       | class         | page-window request                                                                |
|  [04]   | `PageContext<T>`                    | class         | page-state carrier                                                                 |
|  [05]   | `IVirtualRequest`                   | interface     | virtual-window request — `StartIndex`/`Size`                                       |
|  [06]   | `VirtualRequest`                    | class         | `IVirtualRequest` impl, `StartIndexSizeComparer`                                   |
|  [07]   | `IVirtualResponse`                  | interface     | virtual bounds — `StartIndex`/`Size`/`TotalSize`                                   |
|  [08]   | `IPageResponse`                     | interface     | page bounds — `Page`/`Pages`/`PageSize`/`TotalSize`                                |
|  [09]   | `IKeyValueCollection<TObject,TKey>` | interface     | sorted key-value snapshot with its comparer                                        |
|  [10]   | `IGroup<TObject,TKey,TGroup>`       | interface     | live group — `Key`, `Cache : IObservableCache<TObject,TKey>`                       |
|  [11]   | `IGrouping<TObject,TKey,TGroup>`    | interface     | immutable group snapshot — `Key`/`Count`/`Items`/`Keys`/`KeyValues`/`Lookup(TKey)` |
|  [12]   | `Node<TObject,TKey>`                | class         | tree node — `Item`/`Depth`/`Children`/`Parent`                                     |
|  [13]   | `IAggregateChangeSet<T>`            | interface     | `IEnumerable<AggregateItem<T>>` over one change-set                                |
|  [14]   | `AggregateItem<T>`                  | struct        | one aggregate delta — `Type`/`Item`                                                |
|  [15]   | `AggregateType`                     | enum          | delta direction — `Add`/`Remove`                                                   |
|  [16]   | `ChangeStatistics`                  | class         | one change-set tally over six axes plus its index                                  |
|  [17]   | `ChangeSummary`                     | class         | latest-versus-overall tally pair                                                   |

- `IKeyValueCollection<TObject,TKey>` IS an `IReadOnlyList<KeyValuePair<TKey,TObject>>` in sort order, carrying `Comparer`, `SortReason`, and `Optimisations` beside it.

## [02]-[ENTRYPOINTS]

[CACHE_ENTRYPOINTS]: instance mutation and connection on the source and updater types

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `SourceCache.Connect`                                    | instance | keyed change-set stream                  |
|  [02]   | `SourceList.Connect`                                     | instance | list change-set stream                   |
|  [03]   | `ISourceCache.Edit`                                      | instance | atomic cache mutation batch              |
|  [04]   | `ISourceList.Edit`                                       | instance | atomic list mutation batch               |
|  [05]   | `ISourceUpdater.AddOrUpdate`                             | instance | keyed upsert                             |
|  [06]   | `ICacheUpdater.RemoveKey`                                | instance | keyed removal                            |
|  [07]   | `IObservableCollection.Load`                             | instance | replace collection contents              |
|  [08]   | `INotifyCollectionChangedSuspender.SuspendNotifications` | instance | batch bind under suspended notifications |

[QUERY_ENTRYPOINTS]: operators on `IObservable<IChangeSet<…>>` from `ObservableCacheEx`, each folding the stream into a new change-set

| [INDEX] | [SURFACE]                 | [SHAPE] | [CAPABILITY]                                                                         |
| :-----: | :------------------------ | :------ | :----------------------------------------------------------------------------------- |
|  [01]   | `Filter`                  | fold    | predicate filter                                                                     |
|  [02]   | `FilterOnObservable`      | fold    | per-item `IObservable<bool>` re-filter                                               |
|  [03]   | `Sort`                    | fold    | comparer sort                                                                        |
|  [04]   | `Group`                   | fold    | key grouping into `IGroupChangeSet`, live per-group `Cache`                          |
|  [05]   | `GroupOnProperty`         | fold    | property-value regrouping; `GroupOnObservable` per-item group streams                |
|  [06]   | `GroupWithImmutableState` | fold    | grouping into `IImmutableGroupChangeSet` snapshots                                   |
|  [07]   | `Transform`               | fold    | projection, `transformOnRefresh` re-projects on refresh                              |
|  [08]   | `TransformOnObservable`   | fold    | async projection, each item to `IObservable<TDest>`                                  |
|  [09]   | `TransformMany`           | fold    | one-to-many child expansion                                                          |
|  [10]   | `TransformToTree`         | fold    | parent-keyed `Node<T,K>` tree                                                        |
|  [11]   | `AutoRefresh`             | fold    | `INotifyPropertyChanged`/selector refresh stream                                     |
|  [12]   | `AutoRefreshOnObservable` | fold    | external-trigger re-evaluation stream                                                |
|  [13]   | `MergeMany`               | fold    | per-item child `IObservable` merge                                                   |
|  [14]   | `MergeManyChangeSets`     | fold    | per-item child change-sets into one keyed set                                        |
|  [15]   | `MergeChangeSets`         | fold    | N sibling change-set streams into one keyed set                                      |
|  [16]   | `ExpireAfter`             | fold    | timed expiry                                                                         |
|  [17]   | `LimitSizeTo`             | fold    | size bound                                                                           |
|  [18]   | `Page`                    | fold    | paging window                                                                        |
|  [19]   | `Virtualise`              | fold    | windowed virtualisation                                                              |
|  [20]   | `ToCollection`            | fold    | change-set to `IObservable<IReadOnlyCollection<T>>`                                  |
|  [21]   | `EditDiff`                | fold    | `IObservable<IEnumerable<T>>` snapshot to keyed change-set, removals included        |
|  [22]   | `ToObservableChangeSet`   | fold    | `IObservable<IEnumerable<T>>` snapshot upserted into a keyed change-set, no removals |
|  [23]   | `AsObservableCache`       | fold    | change-set stream materialized as a shareable read-only cache                        |
|  [24]   | `QueryWhenChanged`        | fold    | change-set to snapshot with cumulative query state                                   |
|  [25]   | `ChangeKey`               | fold    | re-key a change-set; `Func<TObject,TKey2>` or `Func<TKey1,TObject,TKey2>` selector   |
|  [26]   | `Batch`                   | fold    | window's change-sets coalesced into one; buffers, never drops                        |
|  [27]   | `BatchIf`                 | fold    | that same coalescing behind an `IObservable<bool>` gate, with a timeout release      |

- `Batch(source, timeSpan, scheduler?)` decompiles to `Observable.Buffer(source, timeSpan, scheduler).FlattenBufferResult()`: the window's change-sets MERGE into one and nothing is discarded, which is why it — never `Throttle` or `Sample` — is the backpressure operator for a delta stream, since a rate limiter over `IObservable<IChangeSet<…>>` drops whole deltas and leaves the bound collection describing a state no producer published. `BatchIf(source, pauseIfTrueSelector, initialPauseState = false, timeOut = null, scheduler = null)` holds the same coalescing behind an external gate and releases on the timeout even while the gate stays asserted, so a stuck hold cannot starve the stream; its five overloads vary only in which of `initialPauseState`, `TimeSpan? timeOut`, `IObservable<Unit>? timer`, and `IScheduler?` they carry.
- `EditDiff` against `ToObservableChangeSet` is the SNAPSHOT-semantics fork and the two are not interchangeable: `EditDiff(source, keySelector, equalityComparer?)` diffs each emission against the held set and emits the adds, updates, AND removes that reconcile them, while `ToObservableChangeSet(source, keySelector, expireAfter?, limitSizeTo?, scheduler?)` upserts every emitted item and removes NONE — its only removal paths are the expiry queue and the size-limit eviction queue, so a query-superseding source lowered through it keeps every row of every earlier answer.
- `MergeChangeSets` carries 16 overloads over three axes — the source shape (`IObservable<IObservable<IChangeSet<T,K>>>`, a two-stream pair, a stream-plus-`IEnumerable`, or a bare `IEnumerable<IObservable<IChangeSet<T,K>>>`), an optional `IComparer<T>`, and an optional `IEqualityComparer<T>`; the `IEnumerable`-rooted shapes also take `IScheduler?` and `bool completable = true`, and `completable: false` keeps the merged stream alive after every leg completes. Key COLLISION across legs resolves on the comparer: the tracker replaces the incumbent only when `comparer.Compare(candidate, current) < 0`, so the value sorting FIRST wins, and with no comparer supplied a second leg's add for a held key is IGNORED and the first-seen value stands.
- `MergeChangeSets` REMOVAL is a re-rank, not a delete: the cache merge tracker's `OnItemRemoved` fires `UpdateToBestValue`, which re-looks-up the key across every remaining source cache, republishes the best surviving value under the comparer, and removes the key only when NO source still holds it — so a two-leg merge where one leg is an overriding overlay reconciles by dropping the overlay row and needs no restore path, because the underlying leg's value republishes itself. `OnItemUpdated` under a comparer re-ranks the same way whenever the updated value was the published one, so an update on either leg cannot strand a stale winner.
- `AsObservableCache(source, applyLocking = true)` is how one change-set stream serves several readers: a change-set cannot be replayed to a late subscriber because a replayed delta is not a state, so the shared materialization is a cache whose `Connect()`, `CountChanged`, `Items`, `Keys`, `KeyValues`, and `Lookup` each read the one held set; the cache is `IDisposable` and its owner disposes it.

[BINDING_ENTRYPOINTS]: bind, disposal, and `INotifyPropertyChanged` bridge operators

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------ | :------ | :--------------------------------------------------------- |
|  [01]   | `ObservableCacheEx.SortAndBind`                   | fold    | single-pass fused sort+bind                                |
|  [02]   | `ObservableCacheEx.Bind`                          | fold    | cache bind into `IObservableCollection`/readonly out-var   |
|  [03]   | `ObservableListEx.Bind`                           | fold    | list bind                                                  |
|  [04]   | `ObservableCacheEx.BindToObservableList`          | fold    | bind to `IObservableList<T>`, no UI collection             |
|  [05]   | `ObservableCacheEx.DisposeMany`                   | fold    | per-item disposal on remove/clear                          |
|  [06]   | `ObservableCacheEx.AsyncDisposeMany`              | fold    | async per-item `IAsyncDisposable` disposal                 |
|  [07]   | `ObservableCollectionEx.ToObservableChangeSet`    | fold    | `INotifyCollectionChanged` source to keyed/list change-set |
|  [08]   | `ObservableCollectionEx.ObserveCollectionChanges` | fold    | collection-changed event stream                            |
|  [09]   | `NotifyPropertyChangedEx.WhenValueChanged`        | fold    | typed value stream off `INotifyPropertyChanged`            |
|  [10]   | `NotifyPropertyChangedEx.WhenPropertyChanged`     | fold    | typed property stream                                      |
|  [11]   | `NotifyPropertyChangedEx.WhenAnyPropertyChanged`  | fold    | any-property-change stream                                 |

- `ObservableCacheEx.AsyncDisposeMany(source, accessor)`: disposes `IAsyncDisposable` items itself; the accessor hands the one disposals-completed stream a deactivation scope awaits before teardown.

[AGGREGATE_ENTRYPOINTS]: `DynamicData.Aggregation` operators across `CountEx`/`SumEx`/`AvgEx`/`MaxEx`/`StdDevEx`/`AggregationEx` — rows [04]-[08] take a `Func<T,TValue>` selector and emit `IObservable<TValue>`, rows [01]-[03] are selector-free set-level folds, and `AggregationEx` carries the selector-free custom-fold pair

| [INDEX] | [SURFACE]                      | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `Count`                        | fold    | live count `IObservable<int>`                       |
|  [02]   | `IsEmpty`                      | fold    | live emptiness `IObservable<bool>`                  |
|  [03]   | `IsNotEmpty`                   | fold    | live non-emptiness `IObservable<bool>`              |
|  [04]   | `Sum`                          | fold    | int/long/double/decimal/float sum                   |
|  [05]   | `Avg`                          | fold    | double/decimal/float running average                |
|  [06]   | `Maximum`                      | fold    | running maximum over a comparable selector          |
|  [07]   | `Minimum`                      | fold    | running minimum over a comparable selector          |
|  [08]   | `StdDev`                       | fold    | double/decimal standard deviation                   |
|  [09]   | `AggregationEx.ForAggregation` | fold    | change-set to `IObservable<IAggregateChangeSet<T>>` |
|  [10]   | `AggregationEx.InvalidateWhen` | fold    | re-run the aggregation on an external trigger       |

- `AggregationEx.ForAggregation<TObject,TKey>`/`ForAggregation<TObject>`: the keyed and list overloads both project onto `IObservable<IAggregateChangeSet<TObject>>`, whose enumeration yields `AggregateItem<TObject>` deltas — the custom fold discriminates on `AggregateType.Add`/`Remove` and accumulates each `Item` itself, so one scan reduces any number of accumulators.
- `Accumulate` is `internal` at the admitted pin: a custom fold composes `ForAggregation` with a `Scan`, never that member.

[DIAGNOSTIC_ENTRYPOINTS]: `DynamicData.Diagnostics.DiagnosticOperators` — change-volume accounting off any change-set

| [INDEX] | [SURFACE]            | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------- | :------ | :---------------------------------------------- |
|  [01]   | `CollectUpdateStats` | fold    | change-set to `IObservable<ChangeSummary>` scan |

- `CollectUpdateStats<TObject,TKey>()`/`CollectUpdateStats<TObject>()` scan from `ChangeSummary.Empty`, emitting one summary per change-set: `Latest` is THAT change-set's tally and `Overall` the cumulative run, so a per-delta consumer reads `Latest` and re-publishing `Overall` re-counts every earlier delta. The keyed overload maps `Refreshes` from the change-set's own refresh count; the list overload has no refresh reason and pins that axis to `0` while reading `Replaced` as `Updates`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every state update flows through an `IChangeSet` before a screen sees it; a view never mutates a bound collection directly.
- One `Connect()` is the single source of truth, and every query, bind, and aggregate operator is a projection off it, never a parallel mutation path.
- `TransformToTree` with `Node` recursion is the one hierarchical projection: a flat parent-keyed cache folds to `IObservable<IChangeSet<Node<TObject,TKey>,TKey>>`, each `Node` carrying `Item`/`Depth`/`Parent`/a nested `Children` cache and emitting roots only under the default `IsRoot` while the consumer walks `Children` — never a per-surface tree control or a hand-sliced descendant collection.
- `SortAndBind` is the canonical bound-and-sorted projection: one pass computes the insert position and writes the `ReadOnlyObservableCollection`, so the `Sort().Bind()` two-operator chain is the form it forecloses.
- `Virtualise` takes an `IObservable<ISortedChangeSet<TObject,TKey>>` and an `IObservable<IVirtualRequest>`: a plain keyed change-set does not typecheck, so a windowing pipeline is `Sort(comparer)` then `Virtualise(requests)` and the ordering authority is that comparer alone. The sorted value carries `SortedItems`, an `IKeyValueCollection` reading as `IReadOnlyList<KeyValuePair<TKey,TObject>>` in sort order — so a consumer needing the ordinal sequence reads it off the same change-set it windows rather than a snapshot delegate supplied beside it, which is the form that lets two orders disagree. `VirtualRequest.StartIndexSizeComparer` is the package's own request-identity comparer, so request de-duplication composes it instead of a local equality shim; `IVirtualChangeSet` extends `ISortedChangeSet` and adds `Response`.
- `Group` yields `IGroupChangeSet` whose element is a LIVE `IGroup` carrying its own `Cache` — so per-group aggregates and per-group item collections are projections off that cache and re-emit for the edited group alone, while `GroupWithImmutableState` yields `IGrouping` SNAPSHOTS carrying `Count`/`Items`/`Lookup` with no live connection. A grouping surface that must collapse, count, or window its headers takes the live form and materializes a real group node; a delegate over already-realized rows is the rejected form, because it emits no node to key, collapse, or measure.

[STACKING]:
- `api-reactive.md` (`System.Reactive`): every operator emits `IObservable<T>`, and `WhenAnyPropertyChanged`/`AutoRefresh(x => x.Prop)` lift an `INotifyPropertyChanged` edit back into the change-set so an in-place mutation re-flows the sort/filter/aggregate pipeline with no manual `Refresh`.
- `api-kiwi.md` (`Kiwi`): `Transform` projects layout-edit deltas into `(Variable, double)` pairs, and the subscription calls `Solver.SuggestValue` per item then `Solve()` once per frame, so one observable drives a `DynamicData` edit and a `Kiwi` re-solve.
- `api-livecharts.md` (`LiveChartsCore`): `ToCollection()` snapshots the live projection into an `ISeries.Values` binding, so a chart redraws off the same cache a `DataGrid` binds.
- within-lib: one `SourceCache.Connect()` fans to `SortAndBind` (the `DataGrid`/`ItemsControl` source), `ToCollection()` (chart series), and the `Aggregation` operators (`Count`/`Sum`/`Maximum`) feeding dashboard tiles.
- within-lib: a grouped window holds one `Group` subscription; `TransformOnObservable` maps each `IGroup` to a slice combining its `Cache.Connect()` collection with its `Aggregation` folds over that same published connection, and `CombineLatest(…, expansion).EditDiff(keySelector)` flattens band-then-members into the one keyed node stream `Virtualise` windows — so a member edit re-emits its own group alone and a collapsed group emits one node whose dropped members are REMOVED, which `ToObservableChangeSet` cannot express because it never removes.
- within-lib: tree expansion holds one `TransformToTree` subscription; `CombineLatest(tree.ToCollection(), expansion)` re-walks roots against the live expansion set and `EditDiff(keySelector)` diffs successive flat snapshots into the minimal keyed change-set, re-realizing only changed indent rows and REMOVING the rows a collapse dropped — `ToObservableChangeSet` upserts without removing, so a collapse would leave every expanded descendant standing — `expansion.Select(rebuild).Switch()` re-subscribes `TransformToTree` per toggle at O(n) and is the rejected form.
- `TransformOnObservable`/`FilterOnObservable` stack an async or `Rasm.Compute` result stream per item — each row maps to an `IObservable<TDest>` — so the bound collection updates as each result arrives without leaving the change-set pipeline.

[LOCAL_ADMISSION]:
- A live collection in the AppUi shell is admitted only as a projection off a `SourceCache`/`SourceList`; a screen binding raw mutable state instead of a change-set stream is rejected.
