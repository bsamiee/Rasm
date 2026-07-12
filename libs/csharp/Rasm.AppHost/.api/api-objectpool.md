# [RASM_APPHOST_API_OBJECTPOOL]

`Microsoft.Extensions.ObjectPool` supplies pooled runtime resources, object policies, reset contracts, leak-tracking diagnostics, and StringBuilder pools for bounded allocation lanes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.ObjectPool`

- package: `Microsoft.Extensions.ObjectPool`
- assembly: `Microsoft.Extensions.ObjectPool`
- namespace: `Microsoft.Extensions.ObjectPool`
- asset: runtime library
- rail: pooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pool family

- rail: pooling

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [RAIL]                 |
| :-----: | :-------------------------------- | :------------------ | :--------------------- |
|  [01]   | `ObjectPool<T>`                   | pool contract       | object checkout        |
|  [02]   | `DefaultObjectPool<T>`            | default pool        | retained object store  |
|  [03]   | `ObjectPool`                      | static factory      | direct pool creation   |
|  [04]   | `ObjectPoolProvider`              | provider contract   | pool factory           |
|  [05]   | `DefaultObjectPoolProvider`       | default provider    | retained-count policy  |
|  [06]   | `IPooledObjectPolicy<T>`          | policy contract     | create/return decision |
|  [07]   | `PooledObjectPolicy<T>`           | policy base         | custom pool policy     |
|  [08]   | `DefaultPooledObjectPolicy<T>`    | default policy      | default construction   |
|  [09]   | `IResettable`                     | reset contract      | return-time cleanup    |
|  [10]   | `StringBuilderPooledObjectPolicy` | text buffer policy  | StringBuilder reuse    |
|  [11]   | `LeakTrackingObjectPool<T>`       | diagnostic pool     | lease leak detection   |
|  [12]   | `LeakTrackingObjectPoolProvider`  | diagnostic provider | leak-tracking factory  |
|  [13]   | `ObjectPoolProviderExtensions`    | extension family    | StringBuilder pool     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool operations

- rail: pooling

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]     | [RAIL]                    |
| :-----: | :----------------------------- | :----------------- | :------------------------ |
|  [01]   | `ObjectPool<T>.Get`            | checkout           | leases pooled object      |
|  [02]   | `ObjectPool<T>.Return`         | return             | returns pooled object     |
|  [03]   | `ObjectPool.Create<T>`         | static factory     | creates direct pool       |
|  [04]   | `ObjectPoolProvider.Create<T>` | provider factory   | creates provider pool     |
|  [05]   | `IPooledObjectPolicy.Create`   | policy factory     | constructs pooled object  |
|  [06]   | `IPooledObjectPolicy.Return`   | policy decision    | accepts or rejects return |
|  [07]   | `IResettable.TryReset`         | reset predicate    | cleans returned object    |
|  [08]   | `MaximumRetained`              | provider setting   | caps retained instances   |
|  [09]   | `InitialCapacity`              | buffer setting     | StringBuilder capacity    |
|  [10]   | `MaximumRetainedCapacity`      | buffer setting     | StringBuilder retention   |
|  [11]   | `CreateStringBuilderPool`      | provider extension | creates text buffer pool  |

## [04]-[IMPLEMENTATION_LAW]

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
