# [RASM_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` admits hybrid cache implementation,
service registration, keyed cache registration, serializer registration, cache
options, and tag-aware invalidation into the runtime rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.Hybrid`
- package: `Microsoft.Extensions.Caching.Hybrid`
- contract assembly: `Microsoft.Extensions.Caching.Abstractions` — owns `HybridCache`, `HybridCacheEntryOptions`, `HybridCacheEntryFlags`, `IHybridCacheSerializer<T>`, `IHybridCacheSerializerFactory`, `IBufferDistributedCache`
- implementation assembly: `Microsoft.Extensions.Caching.Hybrid` — owns `HybridCacheOptions`, `IHybridCacheBuilder`, `HybridCacheServiceExtensions`, `HybridCacheBuilderExtensions`
- namespace: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Caching.Distributed` (the L2 buffer contract)
- runtime flags: `IsTrimmable=True`, `IsAotCompatible=True` — serializer admission carries `[DynamicallyAccessedMembers(PublicConstructors)]` trim annotations
- asset: runtime library
- rail: runtime cache

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache contract and entry options
- rail: runtime cache

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:------------------------------ |:------------------ |:---------------------------- |
| [01] | `HybridCache` | abstract cache base | read/write/remove/tag root; resolved from DI, never constructed |
| [02] | `HybridCacheEntryOptions` | entry policy value | sealed; `init`-only `Expiration`/`LocalCacheExpiration`/`Flags` |
| [03] | `HybridCacheEntryFlags` | `[Flags]` enum | per-call L1/L2 read/write disable + compression-disable axis |
| [04] | `IHybridCacheSerializer<T>` | serializer contract | `ReadOnlySequence<byte>`/`IBufferWriter<byte>` zero-copy codec |
| [05] | `IHybridCacheSerializerFactory` | serializer factory | open-generic `TryCreateSerializer<T>` discovery |

`HybridCache` is an `abstract class`, not an interface — the polymorphic `GetOrCreateAsync` family collapses key shape (`string` / `ReadOnlySpan<char>` / `ref DefaultInterpolatedStringHandler`) and factory arity (stateful `TState` / stateless `Func<CancellationToken,...>`) onto two abstract roots, so one read entrypoint discriminates by overload:
- `GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken ct = default): ValueTask<T>` — abstract stampede-protected read-through; `state` threads to the factory so it captures nothing.
- `GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,...): ValueTask<T>` — stateless mirror.
- `GetOrCreateAsync<…>(ReadOnlySpan<char> key, …)` — `virtual` span-key overloads (default materializes `key.ToString()`); an allocation-aware L2 overrides.
- `GetOrCreateAsync<…>(ref DefaultInterpolatedStringHandler key, …)` — interpolated-handler overloads that build the key in-place then `Clear()`, so `$"model:{checksum}:{ep}"` keys never allocate an intermediate `string`.
- `SetAsync<T>(string key, T value, HybridCacheEntryOptions? = null, IEnumerable<string>? tags = null, CancellationToken = default): ValueTask` — abstract explicit write with optional tag set.
- `RemoveAsync(string key \| IEnumerable<string> keys, CancellationToken = default): ValueTask` — abstract single-key + `virtual` batch (singular fast-path on `ICollection` count 0/1).
- `RemoveByTagAsync(string tag \| IEnumerable<string> tags, CancellationToken = default): ValueTask` — abstract single-tag + `virtual` batch group eviction.

`HybridCacheEntryOptions` (sealed; all members `init`-only): `TimeSpan? Expiration` (absolute L1+L2 lifetime → `DistributedCacheEntryOptions.AbsoluteExpirationRelativeToNow`), `TimeSpan? LocalCacheExpiration` (shorter L1-only horizon; a hot key re-reads L2 without re-minting), `HybridCacheEntryFlags? Flags`.

`HybridCacheEntryFlags` (`[Flags]`): `None = 0`; `DisableLocalCacheRead = 1`, `DisableLocalCacheWrite = 2`, `DisableLocalCache = 3`; `DisableDistributedCacheRead = 4`, `DisableDistributedCacheWrite = 8`, `DisableDistributedCache = 0xC`; `DisableUnderlyingData = 0x10` (cache-peek, factory skipped on miss); `DisableCompression = 0x20`.

`IHybridCacheSerializer<T>` — `T Deserialize(ReadOnlySequence<byte> source)` / `void Serialize(T value, IBufferWriter<byte> target)`. `IHybridCacheSerializerFactory` — `bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` (one factory yields a codec for every payload type; first registered factory that succeeds for `T` wins).

[PUBLIC_TYPE_SCOPE]: registration and implementation options
- rail: runtime cache

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:----------------------------- |:----------------- |:---------------------- |
| [01] | `HybridCacheOptions` | cache option value | default cache policy |
| [02] | `IHybridCacheBuilder` | builder contract | serializer admission |
| [03] | `HybridCacheServiceExtensions` | service extension | cache registration |
| [04] | `HybridCacheBuilderExtensions` | builder extension | serializer registration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cache operations
- rail: runtime cache

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:----------------- |:--------------- |:------------------------ |
| [01] | `GetOrCreateAsync` | cache read/write | stampede-aware population |
| [02] | `SetAsync` | cache write | explicit value storage |
| [03] | `RemoveAsync` | key invalidation | key eviction |
| [04] | `RemoveByTagAsync` | tag invalidation | grouped eviction |

[ENTRYPOINT_SCOPE]: registration and policy
- rail: runtime cache

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:--------------------------- |:------------------- |:---------------------------- |
| [01] | `AddHybridCache` | service registration | default cache service |
| [02] | `AddKeyedHybridCache` | service registration | keyed cache service |
| [03] | `AddSerializer<T>` | builder extension | concrete serializer admission |
| [04] | `AddSerializer<T,TImpl>` | builder extension | typed serializer admission |
| [05] | `AddSerializerFactory` | builder extension | serializer factory admission |
| [06] | `DefaultEntryOptions` | option value | default expiration and flags |
| [07] | `MaximumPayloadBytes` | option value | payload size guard |
| [08] | `MaximumKeyLength` | option value | key size guard |
| [09] | `DistributedCacheServiceKey` | option value | distributed cache selection |

`HybridCacheServiceExtensions` on `IServiceCollection` returns `IHybridCacheBuilder` so the serializer chain is admitted on the same call: `AddHybridCache([Action<HybridCacheOptions>])`, `AddKeyedHybridCache(object? serviceKey[, string optionsName][, Action<HybridCacheOptions>])` (keyed registration yields a distinct `HybridCache` resolvable by `[FromKeyedServices]`). `HybridCacheBuilderExtensions` on `IHybridCacheBuilder` (probe order is registration order, first match wins): `AddSerializer<T>(IHybridCacheSerializer<T>)`, `AddSerializer<T, TImplementation>()`, `AddSerializerFactory(IHybridCacheSerializerFactory)`, `AddSerializerFactory<TImplementation>()`. `IHybridCacheBuilder` carries `IServiceCollection Services { get; }`.

`HybridCacheOptions`: `HybridCacheEntryOptions? DefaultEntryOptions`; `long MaximumPayloadBytes` (default `1048576` = 1 MiB; a larger payload is returned uncached, not faulted); `int MaximumKeyLength` (default `1024`; an over-length key bypasses the cache); `bool DisableCompression`; `bool ReportTagMetrics` (per-tag hit/miss on the `HybridCache` EventSource); `object? DistributedCacheServiceKey` (selects which keyed `IDistributedCache` backs L2).

[L2_BUFFER_CONTRACT]: `IBufferDistributedCache` zero-copy distributed backing (`*.Abstractions`, `Microsoft.Extensions.Caching.Distributed`)
- rail: runtime cache
- The runtime sniffs its registered `IDistributedCache` at construction; when it also implements `IBufferDistributedCache` it routes reads through `TryGetAsync(IBufferWriter<byte>)` and writes through the `ReadOnlySequence<byte>` form, so payload bytes move L2↔serializer with no intermediate `byte[]`. `RedisCache` (`Microsoft.Extensions.Caching.StackExchangeRedis`) implements it, so the zero-copy path survives the redis tier; an L2 implementing only `IDistributedCache` still works but loses the zero-copy path.

`IBufferDistributedCache: IDistributedCache` adds `bool TryGet(string, IBufferWriter<byte>)`, `ValueTask<bool> TryGetAsync(string, IBufferWriter<byte>, CancellationToken = default)`, `void Set(string, ReadOnlySequence<byte>, DistributedCacheEntryOptions)`, `ValueTask SetAsync(string, ReadOnlySequence<byte>, DistributedCacheEntryOptions, CancellationToken = default)`. The inherited `IDistributedCache` array members remain, so one storage row satisfies both the array contract and the buffer contract.

## [04]-[IMPLEMENTATION_LAW]

[CACHE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Caching.Distributed`
- contract root: `HybridCache` abstract base from `*.Abstractions`; resolved from DI
- read root: the polymorphic `GetOrCreateAsync` family — one abstract `(string key, TState, factory, …)` root with span and interpolated-handler key overloads and stateful/stateless factory mirrors; the `state` thread keeps the factory closure-free
- write root: `SetAsync<T>` with optional tag set; `RemoveAsync`/`RemoveByTagAsync` each carry a singular and a batch (`IEnumerable<string>`) overload
- policy surface: `HybridCacheOptions` (default entry options, payload guard, key guard, compression flag, tag metrics, keyed L2 selection)
- entry policy: `HybridCacheEntryOptions` (absolute expiry + shorter L1-only horizon + the `HybridCacheEntryFlags` lane-disable axis)
- serializer surface: `IHybridCacheSerializer<T>` (`ReadOnlySequence<byte>`/`IBufferWriter<byte>` zero-copy) discovered via `IHybridCacheSerializerFactory.TryCreateSerializer<T>`
- L2 zero-copy root: `IBufferDistributedCache` — sniffed at registration; `TryGetAsync(IBufferWriter<byte>)` and `SetAsync(ReadOnlySequence<byte>)` move payload bytes with no intermediate array
- invalidation surface: `RemoveAsync` (key) and `RemoveByTagAsync` (tag group), each singular and batched

[LOCAL_ADMISSION]:
- `HybridCache` is a runtime policy surface, not a domain repository; resolved from DI, never constructed (`DefaultHybridCache` is the internal implementation).
- Cache keys, tags, entry options, flags, and serializer policy enter as values; the interpolated-handler key overloads keep key construction allocation-free.
- Keyed cache registration (`AddKeyedHybridCache`) plus `DistributedCacheServiceKey` represent an admitted cache profile selecting its own L2, not ad hoc service lookup.
- Stampede control stays behind `GetOrCreateAsync`; callers never duplicate population locks. The `state` parameter keeps the factory closure-free.
- An L2 contribution implements `IBufferDistributedCache` (not bare `IDistributedCache`) so the zero-copy path is reached; bridging the array contract is contract totality, not a second storage row.
- `MaximumPayloadBytes`/`MaximumKeyLength` are silent bypass guards (an over-limit entry is returned uncached, never faulted).
- Tag invalidation is an explicit cache capability and never substitutes for durable store integrity; the singular and batch tag overloads are one capability, not two owners.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid` (+ `Microsoft.Extensions.Caching.Abstractions` contracts)
- Owns: L1+L2 hybrid cache registration, the abstract `HybridCache` read/write/tag contract, the zero-copy serializer chain, and the `IBufferDistributedCache` L2 seam
- Accept: cache policy as runtime data — entry options, flags, tags, keyed L2 selection, and one serializer factory
- Reject: static cache clients, hidden invalidation channels, per-type serializer proliferation, and an L2 store that implements only `IDistributedCache` where the zero-copy buffer contract is admitted
