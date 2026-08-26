# [RASM_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` owns the two-tier read-through cache: an in-process L1 over a distributed L2, collapsed behind one entrypoint that serves every concurrent miss on a key from a single factory invocation. Its boundary is the payload codec — an L2 value crosses as `ReadOnlySequence<byte>` and `IBufferWriter<byte>`, so a caller's codec writes straight into the transport buffer.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache contract, policy values, and the codec and L2 interfaces

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `HybridCache`                   | abstract class | L1+L2 read, write, and tag eviction root    |
|  [02]   | `HybridCacheEntryOptions`       | class          | sealed `init`-only per-call entry policy    |
|  [03]   | `HybridCacheEntryFlags`         | enum           | per-call lane and compression bypass        |
|  [04]   | `IHybridCacheSerializer<T>`     | interface      | zero-copy payload codec for one type        |
|  [05]   | `IHybridCacheSerializerFactory` | interface      | open-generic codec discovery                |
|  [06]   | `IBufferDistributedCache`       | interface      | zero-copy L2 storage contract               |
|  [07]   | `HybridCacheOptions`            | class          | instance-wide policy and size guards        |
|  [08]   | `IHybridCacheBuilder`           | interface      | codec admission over its `Services`         |
|  [09]   | `HybridCacheServiceExtensions`  | static class   | cache registration on `IServiceCollection`  |
|  [10]   | `HybridCacheBuilderExtensions`  | static class   | codec registration on `IHybridCacheBuilder` |

- `HybridCacheEntryFlags`: `None` `DisableLocalCacheRead` `DisableLocalCacheWrite` `DisableLocalCache` `DisableDistributedCacheRead` `DisableDistributedCacheWrite` `DisableDistributedCache` `DisableUnderlyingData` `DisableCompression`.
- `DisableUnderlyingData` skips the factory on a miss, turning the read into a cache peek; each read/write pair ORs to its combined tier member.
- `Flags` set at the call site replaces the instance default outright and ORs only with the runtime's forced flags, so an override states every lane it wants disabled.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `HybridCache` operations, each closing on `(HybridCacheEntryOptions? = null, IEnumerable<string>? tags = null, CancellationToken = default)`
- Each key shape carries the stateful `(key, TState, Func<TState, CancellationToken, ValueTask<T>>)` row below and a stateless `(key, Func<CancellationToken, ValueTask<T>>)` mirror; threading caller state through `TState` keeps the factory delegate static and closure-free.
- Subclasses override the `string`-key read, `SetAsync`, and the singular `RemoveAsync`/`RemoveByTagAsync`; the span-key read and both batch forms stay virtual defaults folding onto them.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `HybridCache.GetOrCreateAsync<TState,T>(string, …) -> ValueTask<T>`               | instance | stampede-collapsed read-through root   |
|  [02]   | `HybridCache.GetOrCreateAsync<TState,T>(ReadOnlySpan<char>, …)`                   | instance | materializes the key unless overridden |
|  [03]   | `HybridCache.GetOrCreateAsync<TState,T>(ref DefaultInterpolatedStringHandler, …)` | instance | builds the key in place, then clears   |
|  [04]   | `HybridCache.SetAsync<T>(string, T, …) -> ValueTask`                              | instance | writes one value with its tag set      |
|  [05]   | `HybridCache.RemoveAsync(string \| IEnumerable<string>, …)`                       | instance | evicts one key or a batch              |
|  [06]   | `HybridCache.RemoveByTagAsync(string \| IEnumerable<string>, …)`                  | instance | evicts one tag group or a batch        |

- `HybridCache.RemoveByTagAsync`: `"*"` is the reserved wildcard evicting every entry, and a write carrying `"*"` in its tag set throws `ArgumentException`.

[ENTRYPOINT_SCOPE]: registration and codec admission — `HybridCacheServiceExtensions` extends `IServiceCollection`, `HybridCacheBuilderExtensions` extends `IHybridCacheBuilder`, and every row below returns `IHybridCacheBuilder`, so a registration and its codec chain land on one call

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `AddHybridCache([Action<HybridCacheOptions>])`                         | static   | registers the default cache singleton      |
|  [02]   | `AddKeyedHybridCache(object?[, string][, Action<HybridCacheOptions>])` | static   | registers a keyed cache over named options |
|  [03]   | `AddSerializer<T>(IHybridCacheSerializer<T>)`                          | static   | admits a codec instance for one type       |
|  [04]   | `AddSerializer<T,TImpl>()`                                             | static   | admits a DI-built codec for one type       |
|  [05]   | `AddSerializerFactory(IHybridCacheSerializerFactory)`                  | static   | admits a factory instance for many types   |
|  [06]   | `AddSerializerFactory<TImpl>()`                                        | static   | admits a DI-built factory                  |
|  [07]   | `IHybridCacheBuilder.Services`                                         | property | collection the codec chain writes into     |

- `AddKeyedHybridCache` names its options `serviceKey?.ToString()` where the name is omitted, so the keyed cache and a same-named `HybridCacheOptions` registration bind together.

[ENTRYPOINT_SCOPE]: `HybridCacheOptions` instance policy, set at registration
- Properties: `DefaultEntryOptions` `MaximumPayloadBytes` `MaximumKeyLength` `DisableCompression` `ReportTagMetrics` `DistributedCacheServiceKey`; `DefaultEntryOptions.Expiration` seeds every entry's absolute lifetime and falls back to five minutes.
- `MaximumPayloadBytes` and `MaximumKeyLength` are silent guards: an over-quota payload logs and returns uncached, and an empty, over-length, or control-character key routes straight to the factory.
- `DistributedCacheServiceKey` selects the keyed `IDistributedCache` backing L2, so one process runs several cache profiles over distinct stores.

[ENTRYPOINT_SCOPE]: codec and L2 storage contracts — `IBufferDistributedCache` pairs each member with a `…Async` form, and the runtime sniffs the contract on its registered `IDistributedCache` at construction

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :----------------------------- |
|  [01]   | `IHybridCacheSerializer<T>.Serialize(T, IBufferWriter<byte>)`                               | instance | writes a payload to the buffer |
|  [02]   | `IHybridCacheSerializer<T>.Deserialize(ReadOnlySequence<byte>) -> T`                        | instance | reads a payload from segments  |
|  [03]   | `IHybridCacheSerializerFactory.TryCreateSerializer<T>(out IHybridCacheSerializer<T>?)`      | instance | yields a codec per probed type |
|  [04]   | `IBufferDistributedCache.TryGet(string, IBufferWriter<byte>) -> bool`                       | instance | reads L2 bytes to the buffer   |
|  [05]   | `IBufferDistributedCache.Set(string, ReadOnlySequence<byte>, DistributedCacheEntryOptions)` | instance | writes L2 from segments        |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HybridCache` resolves from DI as a singleton the registration mints; a caller binds the abstract contract and never constructs an implementation.
- One `GetOrCreateAsync` call owns stampede collapse for its key, so concurrent misses share one factory invocation and a caller holds no population lock.
- L1 always binds; L2 binds a registered `IDistributedCache`, and the runtime drops a plain `MemoryDistributedCache` sitting behind the default in-process L1, leaving an L1-only cache.
- Tag eviction stamps the tag with a timestamp and an entry minted before its tag's stamp reads expired, so evicting a group costs one write regardless of the entries it covers.
- `HybridCacheEntryOptions` is a sealed `init`-only class, not a record — `with` does not compile, so per-call variation constructs a fresh options object seeded from the lane default.
- `HybridCacheEntryFlags.DisableDistributedCache` carries every non-serializable L1-by-reference value (full L2 bypass, both legs); `DisableDistributedCacheWrite` alone leaves every miss probing a permanently-empty L2.

[STACKING]:
- `CommunityToolkit.HighPerformance`(`.api/api-highperformance.md`): `ArrayPoolBufferWriter<byte>` is the `IHybridCacheSerializer<T>.Serialize` target, and its `WrittenMemory` reads back as the `ReadOnlySequence<byte>` `Deserialize` consumes, so an L2 payload round-trips with no intermediate array.
- `Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`) and `Thinktecture.Runtime.Extensions.MessagePack`(`.api/api-thinktecture-messagepack.md`): one `IHybridCacheSerializerFactory` wraps the generated-owner converter and formatter resolvers, so every Value Object, Smart Enum, and Union caches through the codec its wire path already binds.
- `System.IO.Hashing`(`.api/api-hashing.md`): `ContentHash.Of(ReadOnlySpan<byte>)` mints the content-addressed key, and the `ref DefaultInterpolatedStringHandler` overload composes it into the cache key with no intermediate `string`.
- Within the branch, one `HybridCache` registers at the composition root and each lane binds its own policy per call through `HybridCacheEntryOptions`, a tag set, and flags; a lane needing its own store registers a keyed cache over `DistributedCacheServiceKey`.
- `StackExchange.Redis`: `AddStackExchangeRedisCache(Action<RedisCacheOptions>)` is the Redis `IDistributedCache` backing, and `DistributedCacheServiceKey` selects a keyed store where more than one registers.
- `Rasm.Grasshopper`: RETIRED — the folder's cache module deleted with zero consumers and the manifest row is removed; `Platform/composition.md`'s cache-boundary row records the standing obligations (codec before `AddHybridCache`, `MaximumPayloadBytes` against the largest admitted payload, `ReportTagMetrics`, L1-only unless a real `IDistributedCache` binds) any future cached carrier re-mints under.
- `Rasm.Compute`: model and lowering lanes compose the app-root cache over the `CacheLane` descriptors — the `TState` static-factory `GetOrCreateAsync` is the default population form, a compiled `Delegate` rides an `[ImmutableObject(true)]` carrier under `DisableDistributedCache` so cross-process reuse is deterministic re-derivation off the content-addressed key, never a serialized delegate, and the `CachePolicy` intent is a `[SmartEnum]` row, never a boolean flag.
- `Rasm.Persistence`: contributes the durable half — `CacheL2Store` is the one Marten-backed `IBufferDistributedCache` row (`TryGetAsync` rejecting expired rows and refreshing the sliding deadline beneath its absolute cap, `RefreshAsync` advancing it), `CacheCodecFactory` the one `IHybridCacheSerializerFactory` registering a MessagePack `CacheCodec<T>` through the AppHost `CacheSurface.Register` hook on every keyed builder, `CachePartition.Scoped` deriving the `TenantId`-partitioned content-address key so lane key and durable row read one identity, and `CacheBackplane` folding Redis RESP3 `CLIENT TRACKING ON BCAST PREFIX` tracking or beat invalidation through one `InvalidationMode` drain into the matching removal — lossy by design, a missed beat a TTL-bounded stale read, never corruption; lanes with `CacheLane.Store` set ride the L2 leg while a storeless lane resolves the default cache, and the L2 wire is the `SnapshotCodec.Binary` MessagePack row so durable cache bytes and snapshot bytes share one codec.

[LOCAL_ADMISSION]:
- `HybridCache` enters as a resolved dependency, and cache keys, tags, entry options, and flags enter as call-site values.
- `AddSerializer`/`AddSerializerFactory` admit every codec; the runtime binds a directly registered `IHybridCacheSerializer<T>` first, then probes factories in reverse registration order, so the last factory admitted wins a contested type.
- `AddHybridCache` seeds `TimeProvider.System`, an `IMemoryCache`, a JSON codec factory, and the inbuilt `string`/`byte[]` codecs through try-add, so a registration placed ahead of it displaces the seed.
- `IBufferDistributedCache` is what an L2 contribution implements to reach the zero-copy path, and every admitted codec type keeps a public constructor for the trim and AOT annotation on the generic overloads.
- Persistence owns the L2-store and serializer half while the AppHost runtime owns L1, stampede single-flight, and tag invalidation — one cache owner across both halves, never a second registration.
