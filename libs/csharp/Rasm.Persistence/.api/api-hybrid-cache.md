# [RASM_PERSISTENCE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` admits hybrid cache implementation,
service registration, keyed cache registration, serializer registration, cache
options, and tag-aware invalidation into the runtime rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.Hybrid`
- package: `Microsoft.Extensions.Caching.Hybrid`
- assembly: `Microsoft.Extensions.Caching.Hybrid`
- companion assembly: `Microsoft.Extensions.Caching.Abstractions`
- namespace: `Microsoft.Extensions.Caching.Hybrid`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: runtime cache

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache contract and entry options
- rail: runtime cache

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                        |
| :-----: | :------------------------------ | :------------------ | :---------------------------- |
|  [01]   | `HybridCache`                   | cache contract      | get/set/remove operations     |
|  [02]   | `HybridCacheEntryOptions`       | entry policy value  | expiration and cache flags    |
|  [03]   | `HybridCacheEntryFlags`         | entry flag value    | local/distributed/cache flags |
|  [04]   | `IHybridCacheSerializer<T>`     | serializer contract | payload codec                 |
|  [05]   | `IHybridCacheSerializerFactory` | serializer factory  | serializer discovery          |

[PUBLIC_TYPE_SCOPE]: registration and implementation options
- rail: runtime cache

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                  |
| :-----: | :----------------------------- | :----------------- | :---------------------- |
|  [01]   | `HybridCacheOptions`           | cache option value | default cache policy    |
|  [02]   | `IHybridCacheBuilder`          | builder contract   | serializer admission    |
|  [03]   | `HybridCacheServiceExtensions` | service extension  | cache registration      |
|  [04]   | `HybridCacheBuilderExtensions` | builder extension  | serializer registration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cache operations
- rail: runtime cache

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY]   | [RAIL]                    |
| :-----: | :----------------- | :--------------- | :------------------------ |
|  [01]   | `GetOrCreateAsync` | cache read/write | stampede-aware population |
|  [02]   | `SetAsync`         | cache write      | explicit value storage    |
|  [03]   | `RemoveAsync`      | key invalidation | key eviction              |
|  [04]   | `RemoveByTagAsync` | tag invalidation | grouped eviction          |

[ENTRYPOINT_SCOPE]: registration and policy
- rail: runtime cache

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]       | [RAIL]                        |
| :-----: | :--------------------------- | :------------------- | :---------------------------- |
|  [01]   | `AddHybridCache`             | service registration | default cache service         |
|  [02]   | `AddKeyedHybridCache`        | service registration | keyed cache service           |
|  [03]   | `AddSerializer<T>`           | builder extension    | concrete serializer admission |
|  [04]   | `AddSerializer<T,TImpl>`     | builder extension    | typed serializer admission    |
|  [05]   | `AddSerializerFactory`       | builder extension    | serializer factory admission  |
|  [06]   | `DefaultEntryOptions`        | option value         | default expiration and flags  |
|  [07]   | `MaximumPayloadBytes`        | option value         | payload size guard            |
|  [08]   | `MaximumKeyLength`           | option value         | key size guard                |
|  [09]   | `DistributedCacheServiceKey` | option value         | distributed cache selection   |

## [04]-[IMPLEMENTATION_LAW]

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
