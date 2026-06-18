# [RASM_APPUI_API_DYNAMICDATA]

`DynamicData` supplies change-set caches, lists, live filtering, sorting, grouping, binding, aggregation, paging, virtualisation, and projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DynamicData`
- package: `DynamicData`
- assembly: `DynamicData`
- namespace: `DynamicData`
- namespace: `DynamicData.Binding`
- namespace: `DynamicData.Aggregation`
- namespace: `DynamicData.Diagnostics`
- asset: runtime library
- rail: live-data

## [2]-[PUBLIC_TYPES]

[CACHE_AND_LIST_TYPES]: mutable live data sources
- rail: live-data

| [INDEX] | [SYMBOL]                           | [RAIL]             |
| :-----: | :--------------------------------- | :----------------- |
|   [1]   | `SourceCache<TObject,TKey>`        | keyed source       |
|   [2]   | `SourceList<T>`                    | ordered source     |
|   [3]   | `ISourceCache<TObject,TKey>`       | cache contract     |
|   [4]   | `ISourceList<T>`                   | list contract      |
|   [5]   | `IObservableCache<TObject,TKey>`   | observable cache   |
|   [6]   | `IObservableList<T>`               | observable list    |
|   [7]   | `IIntermediateCache<TObject,TKey>` | intermediate cache |
|   [8]   | `ChangeAwareCache<TObject,TKey>`   | change cache       |

[CHANGE_SET_TYPES]: change records and stream contracts
- rail: live-data

| [INDEX] | [SYMBOL]                               | [RAIL]          |
| :-----: | :------------------------------------- | :-------------- |
|   [1]   | `IChangeSet<T>`                        | list changes    |
|   [2]   | `IChangeSet<TObject,TKey>`             | cache changes   |
|   [3]   | `Change<T>`                            | list change     |
|   [4]   | `Change<TObject,TKey>`                 | cache change    |
|   [5]   | `ChangeReason`                         | cache reason    |
|   [6]   | `ListChangeReason`                     | list reason     |
|   [7]   | `ISortedChangeSet<TObject,TKey>`       | sorted changes  |
|   [8]   | `IGroupChangeSet<TObject,TKey,TGroup>` | grouped changes |
|   [9]   | `IPagedChangeSet<TObject,TKey>`        | paged changes   |
|  [10]   | `IVirtualChangeSet<TObject,TKey>`      | virtual changes |

[BINDING_TYPES]: UI binding targets and adaptors
- rail: live-data

| [INDEX] | [SYMBOL]                                          | [RAIL]              |
| :-----: | :------------------------------------------------ | :------------------ |
|   [1]   | `ObservableCollectionExtended<T>`                 | bound collection    |
|   [2]   | `IObservableCollection<T>`                        | collection contract |
|   [3]   | `ObservableCollectionAdaptor<T>`                  | list adaptor        |
|   [4]   | `ObservableCollectionAdaptor<TObject,TKey>`       | cache adaptor       |
|   [5]   | `SortedObservableCollectionAdaptor<TObject,TKey>` | sorted adaptor      |
|   [6]   | `BindingOptions`                                  | binding options     |
|   [7]   | `SortAndBindOptions`                              | sorted binding      |

[QUERY_TYPES]: sort, page, virtual, aggregate, and diagnostic model
- rail: live-data

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|   [1]   | `SortExpressionComparer<T>` | sort comparer     |
|   [2]   | `SortExpression<T>`         | sort expression   |
|   [3]   | `PageRequest`               | page request      |
|   [4]   | `PageContext<T>`            | page context      |
|   [5]   | `VirtualRequest`            | virtual request   |
|   [6]   | `VirtualResponse`           | virtual response  |
|   [7]   | `IAggregateChangeSet<T>`    | aggregate changes |
|   [8]   | `ChangeStatistics`          | diagnostics       |
|   [9]   | `ChangeSummary`             | diagnostics       |

## [3]-[ENTRYPOINTS]

[CACHE_ENTRYPOINTS]: cache mutation and connection operations
- rail: live-data

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                      | [RAIL]          |
| :-----: | :--------------------- | :---------------------------------- | :-------------- |
|   [1]   | `Connect`              | `SourceCache<TObject,TKey>`         | cache stream    |
|   [2]   | `Connect`              | `SourceList<T>`                     | list stream     |
|   [3]   | `Edit`                 | `ISourceCache<TObject,TKey>`        | cache mutation  |
|   [4]   | `Edit`                 | `ISourceList<T>`                    | list mutation   |
|   [5]   | `AddOrUpdate`          | `ISourceUpdater<TObject,TKey>`      | keyed upsert    |
|   [6]   | `RemoveKey`            | `ICacheUpdater<TObject,TKey>`       | keyed removal   |
|   [7]   | `Load`                 | `IObservableCollection<T>`          | collection load |
|   [8]   | `SuspendNotifications` | `INotifyCollectionChangedSuspender` | batch bind      |

[QUERY_ENTRYPOINTS]: change-set query operations
- rail: live-data
- surface-root: `ObservableCacheEx`

| [INDEX] | [SURFACE]     | [RAIL]             |
| :-----: | :------------ | :----------------- |
|   [1]   | `Filter`      | predicate filter   |
|   [2]   | `Sort`        | comparer sort      |
|   [3]   | `Group`       | key grouping       |
|   [4]   | `Transform`   | projection         |
|   [5]   | `AutoRefresh` | refresh stream     |
|   [6]   | `MergeMany`   | child stream merge |
|   [7]   | `ExpireAfter` | timed expiry       |
|   [8]   | `LimitSizeTo` | size bound         |
|   [9]   | `Page`        | paging             |
|  [10]   | `Virtualise`  | virtualisation     |

[BINDING_ENTRYPOINTS]: UI binding and disposal operations
- rail: live-data

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT]            | [RAIL]            |
| :-----: | :------------------------- | :------------------------ | :---------------- |
|   [1]   | `Bind`                     | `ObservableCacheEx`       | collection bind   |
|   [2]   | `Bind`                     | `ObservableListEx`        | list bind         |
|   [3]   | `ToObservableChangeSet`    | `ObservableCollectionEx`  | change conversion |
|   [4]   | `BindToObservableList`     | `IObservableListEx`       | list target       |
|   [5]   | `DisposeMany`              | `ObservableCacheEx`       | disposal          |
|   [6]   | `AsyncDisposeMany`         | `ObservableCacheEx`       | async disposal    |
|   [7]   | `ObserveCollectionChanges` | `ObservableCollectionEx`  | collection events |
|   [8]   | `WhenValueChanged`         | `NotifyPropertyChangedEx` | property stream   |

[AGGREGATE_ENTRYPOINTS]: computed stream summaries
- rail: live-data
- surface-root: `DynamicData.Aggregation`

| [INDEX] | [SURFACE] | [RAIL]    |
| :-----: | :-------- | :-------- |
|   [1]   | `Count`   | count     |
|   [2]   | `Sum`     | sum       |
|   [3]   | `Avg`     | average   |
|   [4]   | `Min`     | minimum   |
|   [5]   | `Max`     | maximum   |
|   [6]   | `StdDev`  | deviation |

## [4]-[IMPLEMENTATION_LAW]

[LIVE_DATA_LAW]:
- Package: `DynamicData`
- Owns: keyed cache, ordered list, binding, query, aggregate, page, virtual, and diagnostic change-set rails
- Accept: state updates flow through change sets before they reach screens
- Reject: manual observable collection mutation

[PROJECTION_LAW]:
- Package: `DynamicData`
- Owns: filter, sort, group, transform, bind, dispose, aggregate, page, and virtualise operations
- Accept: host panels, companion windows, sidecars, diagnostics, and downstream shells share one live projection rail
- Reject: separate collection mutation paths per view modality
