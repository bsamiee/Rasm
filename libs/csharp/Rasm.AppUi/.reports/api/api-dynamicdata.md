# [RASM_APPUI_API_DYNAMICDATA]

`DynamicData` supplies change-set collections, live filtering, sorting, grouping, binding, and cache projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DynamicData`
- package: `DynamicData`
- assembly: `DynamicData`
- namespace: `DynamicData`
- asset: runtime library
- rail: live-data

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: live collection family
- rail: live-data

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]       | [CAPABILITY]              |
| :-----: | :-------------------------------- | :------------------- | :------------------------ |
|   [1]   | `SourceCache<TObject,TKey>`       | keyed change store   | projects live state       |
|   [2]   | `SourceList<T>`                   | ordered change store | projects live state       |
|   [3]   | `IChangeSet<T>`                   | contract surface     | defines boundary contract |
|   [4]   | `IChangeSet<TObject,TKey>`        | contract surface     | defines boundary contract |
|   [5]   | `Change<T>`                       | change record        | projects live state       |
|   [6]   | `ChangeReason`                    | change reason        | projects live state       |
|   [7]   | `SortExpressionComparer<T>`       | sort expression      | projects live state       |
|   [8]   | `ReadOnlyObservableCollection<T>` | bound collection     | projects live state       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: change-set operations
- rail: live-data

| [INDEX] | [SURFACE]     | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :------------ | :---------------- | :------------------------ |
|   [1]   | `Connect`     | change-set source | opens change stream       |
|   [2]   | `Edit`        | mutation scope    | batches mutations         |
|   [3]   | `AddOrUpdate` | cache mutation    | upserts keyed item        |
|   [4]   | `RemoveKey`   | key removal       | removes keyed item        |
|   [5]   | `Filter`      | query operator    | projects stream state     |
|   [6]   | `Sort`        | query operator    | projects stream state     |
|   [7]   | `Group`       | query operator    | projects stream state     |
|   [8]   | `Transform`   | query operator    | projects stream state     |
|   [9]   | `Bind`        | mutation call     | admits configured surface |
|  [10]   | `DisposeMany` | disposal operator | disposes removed items    |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `DynamicData`
- Owns: live projection collections
- Accept: state updates flow through change sets
- Reject: manual observable collection mutation
