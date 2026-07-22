# [RASM_APPUI_API_DYNAMICDATA]

`DynamicData` owns the live change-set rail: a keyed cache or ordered list mutates through `Edit`, one `Connect()` fans an `IChangeSet` stream, and every query, bind, and aggregate operator folds that stream into a projection a screen binds. One cache is the single source of truth, each surface a projection off it, never a parallel mutation path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DynamicData`
- package: `DynamicData` (MIT)
- assembly: `DynamicData`
- namespaces: `DynamicData`, `DynamicData.Binding`, `DynamicData.Aggregation`, `DynamicData.Diagnostics`
- depends: `System.Reactive` (the `IObservable<IChangeSet<…>>` substrate), `Splat`
- rail: live-data

## [02]-[PUBLIC_TYPES]

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

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `IChangeSet<T>`                        | interface     | list change-set                              |
|  [02]   | `IChangeSet<TObject,TKey>`             | interface     | cache change-set                             |
|  [03]   | `Change<T>`                            | struct        | one list change                              |
|  [04]   | `Change<TObject,TKey>`                 | struct        | one cache change                             |
|  [05]   | `ChangeReason`                         | enum          | cache change reason                          |
|  [06]   | `ListChangeReason`                     | enum          | list change reason                           |
|  [07]   | `ISortedChangeSet<TObject,TKey>`       | interface     | sorted change-set                            |
|  [08]   | `IGroupChangeSet<TObject,TKey,TGroup>` | interface     | grouped change-set                           |
|  [09]   | `IPagedChangeSet<TObject,TKey>`        | interface     | paged change-set, `Response` bounds          |
|  [10]   | `IVirtualChangeSet<TObject,TKey>`      | interface     | virtual-window change-set, `Response` bounds |

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

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `SortExpressionComparer<T>` | class         | multi-key comparer builder                          |
|  [02]   | `SortExpression<T>`         | class         | one sort key and direction                          |
|  [03]   | `PageRequest`               | class         | page-window request                                 |
|  [04]   | `PageContext<T>`            | class         | page-state carrier                                  |
|  [05]   | `VirtualRequest`            | class         | virtual-window request                              |
|  [06]   | `IVirtualResponse`          | interface     | virtual bounds — `StartIndex`/`Size`/`TotalSize`    |
|  [07]   | `IPageResponse`             | interface     | page bounds — `Page`/`Pages`/`PageSize`/`TotalSize` |
|  [08]   | `Node<TObject,TKey>`        | class         | tree node — `Item`/`Depth`/`Children`/`Parent`      |
|  [09]   | `IAggregateChangeSet<T>`    | interface     | aggregate change-set                                |
|  [10]   | `ChangeStatistics`          | class         | change diagnostics                                  |
|  [11]   | `ChangeSummary`             | class         | change diagnostics summary                          |

## [03]-[ENTRYPOINTS]

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

| [INDEX] | [SURFACE]                 | [SHAPE] | [CAPABILITY]                                                                          |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Filter`                  | fold    | predicate filter                                                                      |
|  [02]   | `FilterOnObservable`      | fold    | per-item `IObservable<bool>` re-filter                                                |
|  [03]   | `Sort`                    | fold    | comparer sort                                                                         |
|  [04]   | `Group`                   | fold    | key grouping into `IGroupChangeSet`                                                   |
|  [05]   | `GroupOnProperty`         | fold    | property-value regrouping, `GroupWithImmutableState` snapshots                        |
|  [06]   | `Transform`               | fold    | projection, `transformOnRefresh` re-projects on refresh                               |
|  [07]   | `TransformOnObservable`   | fold    | async projection, each item to `IObservable<TDest>`                                   |
|  [08]   | `TransformMany`           | fold    | one-to-many child expansion                                                           |
|  [09]   | `TransformToTree`         | fold    | parent-keyed `Node<T,K>` tree                                                         |
|  [10]   | `AutoRefresh`             | fold    | `INotifyPropertyChanged`/selector refresh stream                                      |
|  [11]   | `AutoRefreshOnObservable` | fold    | external-trigger re-evaluation stream                                                 |
|  [12]   | `MergeMany`               | fold    | per-item child `IObservable` merge                                                    |
|  [13]   | `MergeManyChangeSets`     | fold    | per-item child change-sets into one keyed set                                         |
|  [14]   | `ExpireAfter`             | fold    | timed expiry                                                                          |
|  [15]   | `LimitSizeTo`             | fold    | size bound                                                                            |
|  [16]   | `Page`                    | fold    | paging window                                                                         |
|  [17]   | `Virtualise`              | fold    | windowed virtualisation                                                               |
|  [18]   | `ToCollection`            | fold    | change-set to `IObservable<IReadOnlyCollection<T>>`                                   |
|  [19]   | `ToObservableChangeSet`   | fold    | `IObservable<IEnumerable<T>>` snapshot to keyed change-set (successive-snapshot diff) |
|  [20]   | `QueryWhenChanged`        | fold    | change-set to snapshot with cumulative query state                                    |

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

[AGGREGATE_ENTRYPOINTS]: `DynamicData.Aggregation` operators across `CountEx`/`SumEx`/`AvgEx`/`MaxEx`/`StdDevEx`/`AggregationEx`, each taking a `Func<T,TValue>` selector and emitting `IObservable<TValue>`

| [INDEX] | [SURFACE]              | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :--------------------- | :------ | :--------------------------------------------- |
|  [01]   | `Count`                | fold    | live count `IObservable<int>`                  |
|  [02]   | `IsEmpty`              | fold    | live emptiness `IObservable<bool>`             |
|  [03]   | `IsNotEmpty`           | fold    | live non-emptiness `IObservable<bool>`         |
|  [04]   | `Sum`                  | fold    | int/long/double/decimal/float sum              |
|  [05]   | `Avg`                  | fold    | double/decimal/float running average           |
|  [06]   | `Maximum`              | fold    | running maximum over a comparable selector     |
|  [07]   | `Minimum`              | fold    | running minimum over a comparable selector     |
|  [08]   | `StdDev`               | fold    | double/decimal standard deviation              |
|  [09]   | `ToAggregateChangeSet` | fold    | raw `IAggregateChangeSet<T>` for a custom fold |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every state update flows through an `IChangeSet` before a screen sees it; a view never mutates a bound collection directly.
- One `Connect()` is the single source of truth, and every query, bind, and aggregate operator is a projection off it, never a parallel mutation path.
- `TransformToTree` with `Node` recursion is the one hierarchical projection: a flat parent-keyed cache folds to `IObservable<IChangeSet<Node<TObject,TKey>,TKey>>`, each `Node` carrying `Item`/`Depth`/`Parent`/a nested `Children` cache and emitting roots only under the default `IsRoot` while the consumer walks `Children` — never a per-surface tree control or a hand-sliced descendant collection.
- `SortAndBind` is the canonical bound-and-sorted projection: one pass computes the insert position and writes the `ReadOnlyObservableCollection`, so the `Sort().Bind()` two-operator chain is the form it forecloses.

[STACKING]:
- `api-reactive.md` (`System.Reactive`): every operator emits `IObservable<T>`, and `WhenAnyPropertyChanged`/`AutoRefresh(x => x.Prop)` lift an `INotifyPropertyChanged` edit back into the change-set so an in-place mutation re-flows the sort/filter/aggregate pipeline with no manual `Refresh`.
- `api-kiwi.md` (`Kiwi`): `Transform` projects layout-edit deltas into `(Variable, double)` pairs, and the subscription calls `Solver.SuggestValue` per item then `Solve()` once per frame, so one observable drives a `DynamicData` edit and a `Kiwi` re-solve.
- `api-livecharts.md` (`LiveChartsCore`): `ToCollection()` snapshots the live projection into an `ISeries.Values` binding, so a chart redraws off the same cache a `DataGrid` binds.
- within-lib: one `SourceCache.Connect()` fans to `SortAndBind` (the `DataGrid`/`ItemsControl` source), `ToCollection()` (chart series), and the `Aggregation` operators (`Count`/`Sum`/`Maximum`) feeding dashboard tiles.
- within-lib: tree expansion holds one `TransformToTree` subscription; `CombineLatest(tree.ToCollection(), expansion)` re-walks roots against the live expansion set and `ToObservableChangeSet(keySelector)` diffs successive flat snapshots into the minimal keyed change-set, re-realizing only changed indent rows — `expansion.Select(rebuild).Switch()` re-subscribes `TransformToTree` per toggle at O(n) and is the rejected form.
- `TransformOnObservable`/`FilterOnObservable` stack an async or `Rasm.Compute` receipt stream per item — each row maps to an `IObservable<TDest>` — so the bound collection updates as each async result arrives without leaving the change-set rail.

[LOCAL_ADMISSION]:
- A live collection in the AppUi shell is admitted only as a projection off a `SourceCache`/`SourceList`; a screen binding raw mutable state instead of a change-set stream is rejected.

[RAIL_LAW]:
- Package: `DynamicData`
- Owns: the keyed-cache, ordered-list, binding, query, aggregate, page, virtual, and diagnostic change-set rails.
- Accept: state reaches every host panel, companion window, sidecar, diagnostic, and shell as a projection off one shared change-set stream, sorted binding through the fused `SortAndBind`.
- Reject: manual `ObservableCollection` mutation, a per-view-modality mutation path, and the `Sort().Bind()` chain `SortAndBind` collapses.
