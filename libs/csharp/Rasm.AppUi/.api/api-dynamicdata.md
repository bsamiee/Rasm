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

| [INDEX] | [SYMBOL]                                          | [RAIL]              |
| :-----: | :------------------------------------------------ | :------------------ |
|  [01]   | `ObservableCollectionExtended<T>`                 | bound collection    |
|  [02]   | `IObservableCollection<T>`                        | collection contract |
|  [03]   | `ObservableCollectionAdaptor<T>`                  | list adaptor        |
|  [04]   | `ObservableCollectionAdaptor<TObject,TKey>`       | cache adaptor       |
|  [05]   | `SortedObservableCollectionAdaptor<TObject,TKey>` | sorted adaptor      |
|  [06]   | `BindingOptions`                                  | binding options     |
|  [07]   | `SortAndBindOptions`                              | sorted binding      |

[QUERY_TYPES]: sort, page, virtual, aggregate, and diagnostic model
- rail: live-data

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `SortExpressionComparer<T>` | sort comparer     |
|  [02]   | `SortExpression<T>`         | sort expression   |
|  [03]   | `PageRequest`               | page request      |
|  [04]   | `PageContext<T>`            | page context      |
|  [05]   | `VirtualRequest`            | virtual request   |
|  [06]   | `VirtualResponse`           | virtual response  |
|  [07]   | `IAggregateChangeSet<T>`    | aggregate changes |
|  [08]   | `ChangeStatistics`          | diagnostics       |
|  [09]   | `ChangeSummary`             | diagnostics       |

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

| [INDEX] | [SURFACE]     | [RAIL]             |
| :-----: | :------------ | :----------------- |
|  [01]   | `Filter`      | predicate filter   |
|  [02]   | `Sort`        | comparer sort      |
|  [03]   | `Group`       | key grouping       |
|  [04]   | `Transform`   | projection         |
|  [05]   | `AutoRefresh` | refresh stream     |
|  [06]   | `MergeMany`   | child stream merge |
|  [07]   | `ExpireAfter` | timed expiry       |
|  [08]   | `LimitSizeTo` | size bound         |
|  [09]   | `Page`        | paging             |
|  [10]   | `Virtualise`  | virtualisation     |

[BINDING_ENTRYPOINTS]: UI binding and disposal operations
- rail: live-data

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT]            | [RAIL]            |
| :-----: | :------------------------- | :------------------------ | :---------------- |
|  [01]   | `Bind`                     | `ObservableCacheEx`       | collection bind   |
|  [02]   | `Bind`                     | `ObservableListEx`        | list bind         |
|  [03]   | `ToObservableChangeSet`    | `ObservableCollectionEx`  | change conversion |
|  [04]   | `BindToObservableList`     | `IObservableListEx`       | list target       |
|  [05]   | `DisposeMany`              | `ObservableCacheEx`       | disposal          |
|  [06]   | `AsyncDisposeMany`         | `ObservableCacheEx`       | async disposal    |
|  [07]   | `ObserveCollectionChanges` | `ObservableCollectionEx`  | collection events |
|  [08]   | `WhenValueChanged`         | `NotifyPropertyChangedEx` | property stream   |

[AGGREGATE_ENTRYPOINTS]: computed stream summaries
- rail: live-data
- surface-root: `DynamicData.Aggregation`

| [INDEX] | [SURFACE] | [RAIL]    |
| :-----: | :-------- | :-------- |
|  [01]   | `Count`   | count     |
|  [02]   | `Sum`     | sum       |
|  [03]   | `Avg`     | average   |
|  [04]   | `Min`     | minimum   |
|  [05]   | `Max`     | maximum   |
|  [06]   | `StdDev`  | deviation |

## [04]-[IMPLEMENTATION_LAW]

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
