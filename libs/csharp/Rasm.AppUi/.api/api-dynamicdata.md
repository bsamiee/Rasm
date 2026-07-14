# [RASM_APPUI_API_DYNAMICDATA]

`DynamicData` supplies change-set caches, lists, live filtering, sorting, grouping, binding, aggregation, paging, virtualisation, and projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DynamicData`
- package: `DynamicData`
- assembly: `DynamicData`
- namespace: `DynamicData`
- namespace: `DynamicData.Binding`
- namespace: `DynamicData.Aggregation`
- namespace: `DynamicData.Diagnostics`
- asset: runtime library
- build-floor: ships `lib/net10.0`; the `net10.0` consumer binds it directly
- dependency: pulls `System.Reactive 6.1.0` (the `IObservable<IChangeSet<…>>` substrate) and `Splat 19.4.1`
- rail: live-data

## [02]-[PUBLIC_TYPES]

[CACHE_AND_LIST_TYPES]: mutable live data sources
- rail: live-data

| [INDEX] | [SYMBOL]                           | [RAIL]             |
| :-----: | :--------------------------------- | :----------------- |
|  [01]   | `SourceCache<TObject,TKey>`        | keyed source       |
|  [02]   | `SourceList<T>`                    | ordered source     |
|  [03]   | `ISourceCache<TObject,TKey>`       | cache contract     |
|  [04]   | `ISourceList<T>`                   | list contract      |
|  [05]   | `IObservableCache<TObject,TKey>`   | observable cache   |
|  [06]   | `IObservableList<T>`               | observable list    |
|  [07]   | `IIntermediateCache<TObject,TKey>` | intermediate cache |
|  [08]   | `ChangeAwareCache<TObject,TKey>`   | change cache       |

[CHANGE_SET_TYPES]: change records and stream contracts
- rail: live-data

| [INDEX] | [SYMBOL]                               | [RAIL]          |
| :-----: | :------------------------------------- | :-------------- |
|  [01]   | `IChangeSet<T>`                        | list changes    |
|  [02]   | `IChangeSet<TObject,TKey>`             | cache changes   |
|  [03]   | `Change<T>`                            | list change     |
|  [04]   | `Change<TObject,TKey>`                 | cache change    |
|  [05]   | `ChangeReason`                         | cache reason    |
|  [06]   | `ListChangeReason`                     | list reason     |
|  [07]   | `ISortedChangeSet<TObject,TKey>`       | sorted changes  |
|  [08]   | `IGroupChangeSet<TObject,TKey,TGroup>` | grouped changes |
|  [09]   | `IPagedChangeSet<TObject,TKey>`        | paged changes   |
|  [10]   | `IVirtualChangeSet<TObject,TKey>`      | virtual changes |

[BINDING_TYPES]: UI binding targets and adaptors
- rail: live-data

| [INDEX] | [SYMBOL]                                          | [RAIL]                     |
| :-----: | :------------------------------------------------ | :------------------------- |
|  [01]   | `ObservableCollectionExtended<T>`                 | bound collection           |
|  [02]   | `IObservableCollection<T>`                        | collection contract        |
|  [03]   | `ObservableCollectionAdaptor<T>`                  | list adaptor               |
|  [04]   | `ObservableCollectionAdaptor<TObject,TKey>`       | cache adaptor              |
|  [05]   | `SortedObservableCollectionAdaptor<TObject,TKey>` | sorted adaptor             |
|  [06]   | `BindingOptions`                                  | binding policy             |
|  [07]   | `SortAndBindOptions`                              | fused sort-and-bind policy |

[BINDING_DETAILS]: collection and policy behavior
- collection: `ObservableCollectionExtended<T>` is an Avalonia/WPF-friendly bound collection with suspendable notifications
- binding-options: `BindingOptions` is `record struct(int ResetThreshold, bool UseReplaceForUpdates=true, bool ResetOnFirstTimeLoad=true)` with `NeverFireReset()`
- sort-and-bind-options: `SortAndBindOptions` carries the reset threshold, reset behavior, and binary-search insertion policy

[QUERY_TYPES]: sort, page, virtual, aggregate, and diagnostic model
- rail: live-data

| [INDEX] | [SYMBOL]                                          | [RAIL]                                                     |
| :-----: | :------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | `SortExpressionComparer<T>`                       | sort comparer                                              |
|  [02]   | `SortExpression<T>`                               | sort expression                                            |
|  [03]   | `PageRequest`                                     | page request                                               |
|  [04]   | `PageContext<T>`                                  | page context                                               |
|  [05]   | `VirtualRequest`                                  | virtual request                                            |
|  [06]   | `VirtualResponse`                                 | virtual response — `StartIndex`/`Size`/`TotalSize`         |
|  [07]   | `PageResponse`                                    | page response — `Page`/`Pages`/`PageSize`/`TotalSize`      |
|  [08]   | `IPagedChangeSet<T,K>` / `IVirtualChangeSet<T,K>` | realized-window change-set — `Response` carries the bounds |
|  [09]   | `Node<TObject,TKey>`                              | tree node — `Item`/`Depth`/`Children`/`Parent`             |
|  [10]   | `IAggregateChangeSet<T>`                          | aggregate changes                                          |
|  [11]   | `ChangeStatistics`                                | diagnostics                                                |
|  [12]   | `ChangeSummary`                                   | diagnostics                                                |

## [03]-[ENTRYPOINTS]

[CACHE_ENTRYPOINTS]: cache mutation and connection operations
- rail: live-data

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                      | [RAIL]          |
| :-----: | :--------------------- | :---------------------------------- | :-------------- |
|  [01]   | `Connect`              | `SourceCache<TObject,TKey>`         | cache stream    |
|  [02]   | `Connect`              | `SourceList<T>`                     | list stream     |
|  [03]   | `Edit`                 | `ISourceCache<TObject,TKey>`        | cache mutation  |
|  [04]   | `Edit`                 | `ISourceList<T>`                    | list mutation   |
|  [05]   | `AddOrUpdate`          | `ISourceUpdater<TObject,TKey>`      | keyed upsert    |
|  [06]   | `RemoveKey`            | `ICacheUpdater<TObject,TKey>`       | keyed removal   |
|  [07]   | `Load`                 | `IObservableCollection<T>`          | collection load |
|  [08]   | `SuspendNotifications` | `INotifyCollectionChangedSuspender` | batch bind      |

[QUERY_ENTRYPOINTS]: change-set query operations
- rail: live-data
- surface-root: `ObservableCacheEx`

| [INDEX] | [SURFACE]                   | [RAIL]                                                                                |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Filter`                    | predicate filter                                                                      |
|  [02]   | `FilterOnObservable`        | per-item `IObservable<bool>` predicate (live re-filter on item-state stream)          |
|  [03]   | `Sort`                      | comparer sort                                                                         |
|  [04]   | `Group` / `GroupOnProperty` | key grouping — static, property-regrouped, or `GroupWithImmutableState` snapshots     |
|  [05]   | `Transform`                 | projection (with `transformOnRefresh` to re-project on refresh)                       |
|  [06]   | `TransformOnObservable`     | async/observable projection — each item maps to `IObservable<TDest>`                  |
|  [07]   | `TransformMany`             | one-to-many child expansion                                                           |
|  [08]   | `TransformToTree`           | parent-keyed `Node<T,K>` tree                                                         |
|  [09]   | `AutoRefresh`               | `INotifyPropertyChanged`/property-selector refresh stream                             |
|  [10]   | `AutoRefreshOnObservable`   | external-trigger re-evaluation stream                                                 |
|  [11]   | `MergeMany`                 | per-item child `IObservable` merge                                                    |
|  [12]   | `MergeManyChangeSets`       | per-item child change-set merge into one flattened keyed change-set                   |
|  [13]   | `ExpireAfter`               | timed expiry                                                                          |
|  [14]   | `LimitSizeTo`               | size bound                                                                            |
|  [15]   | `Page` / `Virtualise`       | paging / windowed virtualisation                                                      |
|  [16]   | `ToCollection`              | change-set to `IObservable<IReadOnlyCollection<T>>`                                   |
|  [17]   | `ToObservableChangeSet`     | `IObservable<IEnumerable<T>>` snapshot to keyed change-set (successive-snapshot diff) |
|  [18]   | `QueryWhenChanged`          | change-set to `IObservable<IReadOnlyCollection>` snapshot with cumulative query state |

[BINDING_ENTRYPOINTS]: UI binding and disposal operations
- rail: live-data

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT]            | [RAIL]                                                          |
| :-----: | :--------------------------------- | :------------------------ | :-------------------------------------------------------------- |
|  [01]   | `SortAndBind`                      | `ObservableCacheEx`       | single-pass fused sort+bind (replaces `Sort().Bind()`)          |
|  [02]   | `Bind`                             | `ObservableCacheEx`       | collection bind into `IObservableCollection` / readonly out-var |
|  [03]   | `Bind`                             | `ObservableListEx`        | list bind                                                       |
|  [04]   | `BindToObservableList`             | `ObservableCacheEx`       | bind to a target `IObservableList<T>` (no UI collection)        |
|  [05]   | `DisposeMany` / `AsyncDisposeMany` | `ObservableCacheEx`       | sync / async per-item disposal on remove/clear                  |
|  [06]   | `ToObservableChangeSet`            | `ObservableCollectionEx`  | `INotifyCollectionChanged` source to keyed/list change-set      |
|  [07]   | `ObserveCollectionChanges`         | `ObservableCollectionEx`  | collection-changed event stream                                 |
|  [08]   | `WhenValueChanged`                 | `NotifyPropertyChangedEx` | typed value stream off `INotifyPropertyChanged`                 |
|  [09]   | `WhenPropertyChanged`              | `NotifyPropertyChangedEx` | typed property stream off `INotifyPropertyChanged`              |
|  [10]   | `WhenAnyPropertyChanged`           | `NotifyPropertyChangedEx` | property-change stream off `INotifyPropertyChanged`             |

`AsyncDisposeMany(source, Action<IObservable<Unit>> disposalsCompletedAccessor)` disposes `IAsyncDisposable` items itself; the accessor hands out the one disposals-completed stream a deactivation scope awaits before teardown.

[AGGREGATE_ENTRYPOINTS]: computed stream summaries
- rail: live-data
- surface-root: `DynamicData.Aggregation` — operators are extension methods on `IObservable<IChangeSet<…>>` spread across `CountEx` / `SumEx` / `AvgEx` / `MaxEx` / `StdDevEx`; each consumes a `Func<T,TValue>` value selector and emits `IObservable<TValue>`

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]  | [RAIL]                                         |
| :-----: | :----------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `Count`                  | `CountEx`       | live count (`IObservable<int>`)                |
|  [02]   | `IsEmpty` / `IsNotEmpty` | `CountEx`       | live emptiness predicate (`IObservable<bool>`) |
|  [03]   | `Sum`                    | `SumEx`         | int/long/double/decimal/float numeric sum      |
|  [04]   | `Avg`                    | `AvgEx`         | double/decimal/float running average           |
|  [05]   | `Maximum`                | `MaxEx`         | running maximum over a comparable selector     |
|  [06]   | `Minimum`                | `MaxEx`         | running minimum over a comparable selector     |
|  [07]   | `StdDev`                 | `StdDevEx`      | double/decimal standard deviation              |
|  [08]   | `ToAggregateChangeSet`   | `AggregationEx` | raw `IAggregateChangeSet<T>` for a custom fold |

## [04]-[IMPLEMENTATION_LAW]

[LIVE_DATA_LAW]:
- Package: `DynamicData`
- Owns: keyed cache, ordered list, binding, query, aggregate, page, virtual, and diagnostic change-set rails
- Accept: state updates flow through change sets before they reach screens
- Reject: manual observable collection mutation

[PROJECTION_LAW]:
- Package: `DynamicData`
- Owns: filter, sort, group, transform, bind, dispose, aggregate, page, and virtualise operations
- Accept: host panels, companion windows, sidecars, diagnostics, and downstream shells share one live projection rail; sorted binding uses the fused `SortAndBind`
- Reject: separate collection mutation paths per view modality; the legacy `Sort().Bind()` two-operator chain where `SortAndBind` collapses it

[HIERARCHY_LAW]:
- `TransformToTree(parentKeySelector, predicateChanged?)` folds a flat parent-keyed cache into an `IObservable<IChangeSet<Node<TObject,TKey>,TKey>>` whose `Node<TObject,TKey>` carries `Item`, `Depth`, `Parent`, and a nested `Children` observable cache; its default root predicate is `IsRoot`, so it emits root nodes only and the consumer walks `Children` for descendants.
- `TransformMany(manySelector, keySelector)` expands each source item into a child change-set, the operator a flatten composes onto to project a `Node` tree into flat indent rows.
- The tree-flatten fold is one owner: `TransformToTree` plus the `Node` recursion is the hierarchical projection, never a per-surface tree control or a hand-sliced descendant collection.
- Expansion toggles retain one `TransformToTree` subscription: `CombineLatest(tree.ToCollection(), expansion)` re-walks the root collection against the live expansion set, and `ToObservableChangeSet(keySelector)` diffs successive flat snapshots into the minimal keyed change-set.
- The diff re-realizes only the changed indent rows without re-subscribing the tree transform; `expansion.Select(rebuild).Switch()` re-runs `TransformToTree` per toggle and incurs O(n) work.

[SORTED_BIND_LAW]:
- `SortAndBind(out var collection, comparer, SortAndBindOptions)` is the canonical bound-and-sorted projection — one operator computes the sorted insert position and writes it into the `ReadOnlyObservableCollection` in a single pass, replacing the legacy `Sort(comparer).Bind(out collection)` two-operator chain. A `SortExpressionComparer<T>.Ascending(x => x.A).ThenByDescending(x => x.B)` builds the multi-key comparer the operator consumes.
- `BindingOptions`/`SortAndBindOptions` set `ResetThreshold` (a batch larger than the threshold fires one collection reset instead of N adds — the Avalonia/WPF virtualization-friendly path) and `UseReplaceForUpdates`; `BindingOptions.NeverFireReset()` forces incremental notifications when a downstream control mis-handles `Reset`.

[STACKING]:
- One change-set fans to every downstream rail: a single `SourceCache.Connect()` feeds `SortAndBind` (the `Avalonia.Controls.DataGrid`/`ItemsControl` source), `ToCollection()` (a snapshot for a `LiveChartsCore` `ISeries.Values` binding so a chart redraws off the same live projection), and the `DynamicData.Aggregation` operators (`Count`/`Sum`/`Maximum`) feeding dashboard tiles — the cache is the one source of truth and each surface is a projection, never a parallel mutation path.
- Drive the constraint solver from the data rail: `Transform` projects layout-edit deltas into `(Variable, double)` pairs and the subscription calls `Kiwi.Solver.TrySuggestValue` per item then `Solve()` once per frame, so a `DynamicData` edit and a `Kiwi` re-solve share one observable.
- `ReactiveUI`/`System.Reactive` interop: `WhenAnyPropertyChanged` and `AutoRefresh(x => x.Prop)` lift `INotifyPropertyChanged` view-model mutations into the change-set so an in-place edit re-flows the sort/filter/aggregate pipeline without a manual `Refresh` call; `BindToObservableList` targets a non-UI `IObservableList<T>` when the consumer is another rail rather than a control.
- `TransformOnObservable`/`FilterOnObservable` stack an async or Compute-receipt stream per item (each row maps to an `IObservable<TDest>` from a `Rasm.Compute` query or a live validation), so the bound collection updates as each item's async result arrives without leaving the change-set rail.
