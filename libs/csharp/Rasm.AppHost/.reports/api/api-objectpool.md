# [RASM_APPHOST_API_OBJECTPOOL]

`Microsoft.Extensions.ObjectPool` supplies pooled runtime resources, object policies,
reset contracts, leak-tracking diagnostics, and StringBuilder pools for bounded
allocation lanes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.ObjectPool`
- package: `Microsoft.Extensions.ObjectPool`
- assembly: `Microsoft.Extensions.ObjectPool`
- namespace: `Microsoft.Extensions.ObjectPool`
- asset: runtime library
- rail: pooling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pool family
- rail: pooling

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [RAIL]                 |
| :-----: | :-------------------------------- | :------------------ | :--------------------- |
|   [1]   | `ObjectPool<T>`                   | pool contract       | object checkout        |
|   [2]   | `DefaultObjectPool<T>`            | default pool        | retained object store  |
|   [3]   | `ObjectPool`                      | static factory      | direct pool creation   |
|   [4]   | `ObjectPoolProvider`              | provider contract   | pool factory           |
|   [5]   | `DefaultObjectPoolProvider`       | default provider    | retained-count policy  |
|   [6]   | `IPooledObjectPolicy<T>`          | policy contract     | create/return decision |
|   [7]   | `PooledObjectPolicy<T>`           | policy base         | custom pool policy     |
|   [8]   | `DefaultPooledObjectPolicy<T>`    | default policy      | default construction   |
|   [9]   | `IResettable`                     | reset contract      | return-time cleanup    |
|  [10]   | `StringBuilderPooledObjectPolicy` | text buffer policy  | StringBuilder reuse    |
|  [11]   | `LeakTrackingObjectPool<T>`       | diagnostic pool     | lease leak detection   |
|  [12]   | `LeakTrackingObjectPoolProvider`  | diagnostic provider | leak-tracking factory  |
|  [13]   | `ObjectPoolProviderExtensions`    | extension family    | StringBuilder pool     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool operations
- rail: pooling

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]     | [RAIL]                    |
| :-----: | :----------------------------- | :----------------- | :------------------------ |
|   [1]   | `ObjectPool<T>.Get`            | checkout           | leases pooled object      |
|   [2]   | `ObjectPool<T>.Return`         | return             | returns pooled object     |
|   [3]   | `ObjectPool.Create<T>`         | static factory     | creates direct pool       |
|   [4]   | `ObjectPoolProvider.Create<T>` | provider factory   | creates provider pool     |
|   [5]   | `IPooledObjectPolicy.Create`   | policy factory     | constructs pooled object  |
|   [6]   | `IPooledObjectPolicy.Return`   | policy decision    | accepts or rejects return |
|   [7]   | `IResettable.TryReset`         | reset predicate    | cleans returned object    |
|   [8]   | `MaximumRetained`              | provider setting   | caps retained instances   |
|   [9]   | `InitialCapacity`              | buffer setting     | StringBuilder capacity    |
|  [10]   | `MaximumRetainedCapacity`      | buffer setting     | StringBuilder retention   |
|  [11]   | `CreateStringBuilderPool`      | provider extension | creates text buffer pool  |

## [4]-[IMPLEMENTATION_LAW]

[POOL_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.ObjectPool`
- pool contracts: `ObjectPool<T>`, `ObjectPoolProvider`, `IPooledObjectPolicy<T>`
- policy contracts: create object, decide return, reset object
- retained count: provider policy controls retained instances, not total allocations
- string builder policy: initial capacity and maximum retained capacity
- diagnostics: leak-tracking pool provider is diagnostic-only material

[LOCAL_ADMISSION]:
- Pools are injected policy values for bounded allocation lanes.
- Returned objects reset through `IResettable` or policy-owned cleanup.
- Pooled instances never carry request, document, host, or user state across returns.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.ObjectPool`
- Owns: bounded resource reuse
- Accept: pools are runtime policy inputs
- Reject: ad hoc static pools
