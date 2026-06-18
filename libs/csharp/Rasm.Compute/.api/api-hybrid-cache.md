# [RASM_COMPUTE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` admits hybrid cache implementation,
service registration, keyed cache registration, serializer registration, cache
options, and tag-aware invalidation into the runtime rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.Hybrid`
- package: `Microsoft.Extensions.Caching.Hybrid`
- assembly: `Microsoft.Extensions.Caching.Hybrid`
- companion assembly: `Microsoft.Extensions.Caching.Abstractions`
- namespace: `Microsoft.Extensions.Caching.Hybrid`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: runtime cache

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache contract and entry options
- rail: runtime cache

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                        |
| :-----: | :------------------------------ | :------------------ | :---------------------------- |
|   [1]   | `HybridCache`                   | cache contract      | get/set/remove operations     |
|   [2]   | `HybridCacheEntryOptions`       | entry policy value  | expiration and cache flags    |
|   [3]   | `HybridCacheEntryFlags`         | entry flag value    | local/distributed/cache flags |
|   [4]   | `IHybridCacheSerializer<T>`     | serializer contract | payload codec                 |
|   [5]   | `IHybridCacheSerializerFactory` | serializer factory  | serializer discovery          |

[PUBLIC_TYPE_SCOPE]: registration and implementation options
- rail: runtime cache

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                  |
| :-----: | :----------------------------- | :----------------- | :---------------------- |
|   [1]   | `HybridCacheOptions`           | cache option value | default cache policy    |
|   [2]   | `IHybridCacheBuilder`          | builder contract   | serializer admission    |
|   [3]   | `HybridCacheServiceExtensions` | service extension  | cache registration      |
|   [4]   | `HybridCacheBuilderExtensions` | builder extension  | serializer registration |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cache operations
- rail: runtime cache

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY]   | [RAIL]                    |
| :-----: | :----------------- | :--------------- | :------------------------ |
|   [1]   | `GetOrCreateAsync` | cache read/write | stampede-aware population |
|   [2]   | `SetAsync`         | cache write      | explicit value storage    |
|   [3]   | `RemoveAsync`      | key invalidation | key eviction              |
|   [4]   | `RemoveByTagAsync` | tag invalidation | grouped eviction          |

[ENTRYPOINT_SCOPE]: registration and policy
- rail: runtime cache

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]       | [RAIL]                        |
| :-----: | :--------------------------- | :------------------- | :---------------------------- |
|   [1]   | `AddHybridCache`             | service registration | default cache service         |
|   [2]   | `AddKeyedHybridCache`        | service registration | keyed cache service           |
|   [3]   | `AddSerializer<T>`           | builder extension    | concrete serializer admission |
|   [4]   | `AddSerializer<T,TImpl>`     | builder extension    | typed serializer admission    |
|   [5]   | `AddSerializerFactory`       | builder extension    | serializer factory admission  |
|   [6]   | `DefaultEntryOptions`        | option value         | default expiration and flags  |
|   [7]   | `MaximumPayloadBytes`        | option value         | payload size guard            |
|   [8]   | `MaximumKeyLength`           | option value         | key size guard                |
|   [9]   | `DistributedCacheServiceKey` | option value         | distributed cache selection   |

## [4]-[IMPLEMENTATION_LAW]

[CACHE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`
- contract root: `HybridCache` from caching abstractions
- implementation root: `Microsoft.Extensions.Caching.Hybrid`
- registration surface: default and keyed hybrid cache service registration
- policy surface: default entry options, payload guard, key guard, compression flag, tag metrics
- entry policy: expiration, local cache expiration, and local/distributed disable flags
- serializer surface: per-type serializers and serializer factories
- invalidation surface: key removal and tag removal

[LOCAL_ADMISSION]:
- Hybrid cache is a runtime policy surface, not a domain repository.
- Cache keys, tags, entry options, and serializer policy enter as values.
- Keyed cache registration represents an admitted cache profile, not ad hoc service lookup.
- Stampede control stays behind `GetOrCreateAsync`; callers do not duplicate population locks.
- Tag invalidation is an explicit cache capability and never substitutes for durable store integrity.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid`
- Owns: hybrid cache registration and implementation policy
- Accept: cache policy as runtime data
- Reject: static cache clients and hidden invalidation channels
