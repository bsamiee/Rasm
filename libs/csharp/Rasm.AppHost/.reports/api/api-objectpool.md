# [RASM_APPHOST_API_OBJECTPOOL]

`Microsoft.Extensions.ObjectPool` supplies pooled runtime resources for bounded allocation lanes and bootstrap services.

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

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]  | [CAPABILITY]               |
| :-----: | :-------------------------------- | :-------------- | :------------------------- |
|   [1]   | `ObjectPool<T>`                   | pool handle     | anchors pooling contract   |
|   [2]   | `ObjectPoolProvider`              | pool factory    | anchors pooling contract   |
|   [3]   | `DefaultObjectPoolProvider`       | default factory | anchors pooling contract   |
|   [4]   | `PooledObjectPolicy<T>`           | pool policy     | anchors pooling contract   |
|   [5]   | `DefaultPooledObjectPolicy<T>`    | default policy  | anchors pooling contract   |
|   [6]   | `StringBuilderPooledObjectPolicy` | builder surface | constructs configured root |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool operations
- rail: pooling

| [INDEX] | [SURFACE]              | [CALL_SHAPE]       | [CAPABILITY]              |
| :-----: | :--------------------- | :----------------- | :------------------------ |
|   [1]   | `Get`                  | lookup call        | resolves typed value      |
|   [2]   | `Return`               | pool return method | returns pooled object     |
|   [3]   | `Create`               | factory call       | creates configured handle |
|   [4]   | `CreatePolicy`         | factory call       | creates configured handle |
|   [5]   | `MaximumRetained`      | property surface   | binds surface state       |
|   [6]   | `IResettable.TryReset` | reset predicate    | cleans pooled object      |

## [4]-[IMPLEMENTATION_LAW]

[POOL_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.ObjectPool`
- pool contracts: `ObjectPool<T>`, `ObjectPoolProvider`, `IPooledObjectPolicy<T>`
- policy contracts: create object, decide return, reset object
- retained count: provider policy controls retained instances, not total allocations
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
