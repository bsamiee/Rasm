# [RASM_APPHOST_API_OBJECTPOOL]

`Microsoft.Extensions.ObjectPool` owns bounded instance reuse for allocation-hot AppHost lanes: a policy mints and resets pooled objects, a provider caps how many survive a return, and a StringBuilder policy pools text buffers. Retention bounds live retained instances, never total allocation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.ObjectPool`
- package: `Microsoft.Extensions.ObjectPool`
- assembly: `Microsoft.Extensions.ObjectPool`
- namespace: `Microsoft.Extensions.ObjectPool`
- asset: runtime library
- rail: pooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pool family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CAPABILITY]                   |
| :-----: | :-------------------------------- | :------------- | :----------------------------- |
|  [01]   | `ObjectPool<T>`                   | abstract class | pooled-instance lease contract |
|  [02]   | `DefaultObjectPool<T>`            | class          | retained-instance store        |
|  [03]   | `ObjectPool`                      | static class   | policy-driven pool creation    |
|  [04]   | `ObjectPoolProvider`              | abstract class | pool factory contract          |
|  [05]   | `DefaultObjectPoolProvider`       | class          | retained-count pool factory    |
|  [06]   | `IPooledObjectPolicy<T>`          | interface      | pooled-object lifecycle policy |
|  [07]   | `PooledObjectPolicy<T>`           | abstract class | base for a custom pool policy  |
|  [08]   | `DefaultPooledObjectPolicy<T>`    | class          | default-construction policy    |
|  [09]   | `IResettable`                     | interface      | return-time reset contract     |
|  [10]   | `StringBuilderPooledObjectPolicy` | class          | StringBuilder reuse policy     |
|  [11]   | `ObjectPoolProviderExtensions`    | static class   | StringBuilder pool extension   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pool operations

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `ObjectPool<T>.Get()`                                     | instance | leases a pooled instance        |
|  [02]   | `ObjectPool<T>.Return(T)`                                 | instance | returns an instance for reuse   |
|  [03]   | `ObjectPool.Create<T>(IPooledObjectPolicy<T>)`            | static   | mints a default-bounded pool    |
|  [04]   | `ObjectPoolProvider.Create<T>(IPooledObjectPolicy<T>)`    | factory  | mints a provider-bounded pool   |
|  [05]   | `IPooledObjectPolicy<T>.Create()`                         | instance | constructs a pooled instance    |
|  [06]   | `IPooledObjectPolicy<T>.Return(T)`                        | instance | return-eligibility decision     |
|  [07]   | `IResettable.TryReset()`                                  | instance | resets a returned instance      |
|  [08]   | `DefaultObjectPoolProvider.MaximumRetained`               | property | caps retained instances         |
|  [09]   | `StringBuilderPooledObjectPolicy.InitialCapacity`         | property | StringBuilder seed capacity     |
|  [10]   | `StringBuilderPooledObjectPolicy.MaximumRetainedCapacity` | property | StringBuilder retention ceiling |
|  [11]   | `ObjectPoolProvider.CreateStringBuilderPool(int, int)`    | static   | mints a StringBuilder pool      |

- `ObjectPool.Create<T>` and `ObjectPoolProvider.Create<T>` each carry a parameterless overload defaulting to `DefaultPooledObjectPolicy<T>`.
- `ObjectPoolProvider.Create<T>` returns a disposal-aware pool for a `T : IDisposable`, disposing every instance it declines to retain.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Provider policy caps retained instances, never total allocation.
- A returned instance re-pools only when its policy `Return` accepts it, folding `IResettable.TryReset` first; a rejected instance is discarded.
- A pool of `IDisposable` instances disposes each instance it declines to retain.

[STACKING]:
- `Runtime/resources.md`: `PoolPolicy<T> : PooledObjectPolicy<T>` mints each row's pool once through `ObjectPool.Create<T>` and folds `IResettable.TryReset` in `Return`; `Pools` composes the `StringBuilderPooledObjectPolicy` text pool and `DefaultObjectPoolProvider.MaximumRetained` bounded pools.

[LOCAL_ADMISSION]:
- Pools are injected policy values on allocation-hot AppHost lanes.
- Returned instances reset through `IResettable` or policy-owned cleanup.
- Pooled instances carry no request, document, host, or user state across returns.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.ObjectPool`
- Owns: bounded instance reuse on allocation-hot lanes
- Accept: pools as injected runtime policy values
- Reject: ad hoc static pools and per-site `StringBuilder` churn
