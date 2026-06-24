# [RASM_PERSISTENCE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` is the L1+L2 multi-level cache that builds on
`IDistributedCache`: an in-process memory L1 in front of any registered distributed L2, with
built-in stampede protection, tag invalidation, payload compression, and a pluggable serializer
chain. The `HybridCache` contract, `IHybridCacheSerializer<T>`/`IHybridCacheSerializerFactory`, and
the `IBufferDistributedCache` zero-copy L2 contract live in the companion
`Microsoft.Extensions.Caching.Abstractions` assembly; the implementation assembly ships
`HybridCacheOptions`, `IHybridCacheBuilder`, and the DI/builder extension surface. Every other public
symbol in the implementation DLL lives under `Microsoft.Extensions.Caching.Hybrid.Internal` and is
not a consumption surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.Hybrid`
- package: `Microsoft.Extensions.Caching.Hybrid`
- version: `10.7.0`
- license: MIT
- contract assembly: `Microsoft.Extensions.Caching.Abstractions` (`10.0.9`) — owns `HybridCache`, `HybridCacheEntryOptions`, `HybridCacheEntryFlags`, `IHybridCacheSerializer<T>`, `IHybridCacheSerializerFactory`, and `IBufferDistributedCache`
- implementation assembly: `Microsoft.Extensions.Caching.Hybrid` — owns `HybridCacheOptions`, `IHybridCacheBuilder`, `HybridCacheServiceExtensions`, `HybridCacheBuilderExtensions`
- target: `net10.0` (multi-targets `net462`/`net8.0`/`net9.0`/`net10.0`; consumer binds `net10.0`)
- runtime flags: `IsTrimmable=True`, `IsAotCompatible=True` — serializer admission carries `[DynamicallyAccessedMembers(PublicConstructors)]` trim annotations
- namespace: `Microsoft.Extensions.Caching.Hybrid`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: runtime cache

## [02]-[CACHE_CONTRACT]

[CONTRACT_SCOPE]: `HybridCache` abstract base and entry policy values (`*.Abstractions`)
- rail: runtime cache

`HybridCache` is an `abstract class`, not an interface — the polymorphic `GetOrCreateAsync` family
collapses key shape (`string` / `ReadOnlySpan<char>` / `ref DefaultInterpolatedStringHandler`) and
factory arity (stateful `TState` / stateless `Func<CancellationToken,...>`) onto two abstract roots,
so a single read entrypoint discriminates by overload rather than a method-per-shape proliferation.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [BOUNDARY_NOTE]                                                   |
| :-----: | :------------------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `HybridCache`                   | abstract cache base | read/write/remove/tag root; resolved from DI, never constructed  |
|  [02]   | `HybridCacheEntryOptions`       | entry policy value  | sealed; `init`-only `Expiration`, `LocalCacheExpiration`, `Flags`|
|  [03]   | `HybridCacheEntryFlags`         | `[Flags]` enum      | per-call L1/L2 read/write disable + compression-disable axis     |
|  [04]   | `IHybridCacheSerializer<T>`     | serializer contract | `ReadOnlySequence<byte>` / `IBufferWriter<byte>` zero-copy codec |
|  [05]   | `IHybridCacheSerializerFactory` | serializer factory  | open-generic `TryCreateSerializer<T>` discovery                  |

`HybridCache` operations (the `(string key, ...)` roots are abstract; the span and interpolated-handler
mirrors are non-abstract overloads that funnel into them):

- `GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) : ValueTask<T>` — abstract stampede-protected read-through; the `state` value threads to the factory so a closure-free factory captures nothing.
- `GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) : ValueTask<T>` — stateless convenience mirror.
- `GetOrCreateAsync<TState, T>(ReadOnlySpan<char> key, ...) : ValueTask<T>` and `GetOrCreateAsync<T>(ReadOnlySpan<char> key, ...) : ValueTask<T>` — `virtual` span-key overloads (default impl materializes `key.ToString()`); an allocation-aware L2 can override.
- `GetOrCreateAsync<TState, T>(ref DefaultInterpolatedStringHandler key, ...) : ValueTask<T>` and `GetOrCreateAsync<T>(ref DefaultInterpolatedStringHandler key, ...) : ValueTask<T>` — interpolated-string-handler key overloads that build the key in-place then `Clear()` the handler, so `$"model:{checksum}:{ep}"` cache keys never allocate an intermediate `string`.
- `SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) : ValueTask` — abstract explicit write with optional tag set.
- `RemoveAsync(string key, CancellationToken cancellationToken = default) : ValueTask` — abstract single-key eviction.
- `RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) : ValueTask` — `virtual` batch eviction (singular fast-path on `ICollection` count 0/1).
- `RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) : ValueTask` — abstract single-tag group eviction.
- `RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default) : ValueTask` — `virtual` batch tag eviction (singular fast-path on `ICollection` count 0/1).

`HybridCacheEntryOptions` (sealed; all members `init`-only — an entry options value is immutable):
- `TimeSpan? Expiration { get; init; }` — absolute L1+L2 lifetime (maps to `DistributedCacheEntryOptions.AbsoluteExpirationRelativeToNow`).
- `TimeSpan? LocalCacheExpiration { get; init; }` — shorter L1-only lifetime; an L1 entry can expire ahead of its L2 backing so a hot key re-reads L2 without re-minting.
- `HybridCacheEntryFlags? Flags { get; init; }` — per-entry override of the read/write/compression axis.

`HybridCacheEntryFlags` (`[Flags]`; the per-call lane-control vocabulary that drives the L1/L2 routing the L2 contribution depends on):
- `None = 0`
- `DisableLocalCacheRead = 1`, `DisableLocalCacheWrite = 2`, `DisableLocalCache = 3` (both) — a blob-lane entry sets `DisableLocalCache` so an oversized payload never pins the in-process L1.
- `DisableDistributedCacheRead = 4`, `DisableDistributedCacheWrite = 8`, `DisableDistributedCache = 0xC` (both).
- `DisableUnderlyingData = 0x10` — read cache only; the factory is skipped on miss (cache-peek).
- `DisableCompression = 0x20` — per-entry opt-out of the payload compressor.

`IHybridCacheSerializer<T>` — the zero-copy codec contract a MessagePack/typed serializer row implements:
- `T Deserialize(ReadOnlySequence<byte> source) : T`
- `void Serialize(T value, IBufferWriter<byte> target) : void`

`IHybridCacheSerializerFactory` — open-generic serializer discovery probed at registration:
- `bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer) : bool` — a single factory yields a codec for every payload type with zero per-type registration; first registered factory that succeeds for `T` wins.

## [03]-[L2_BUFFER_CONTRACT]

[BUFFER_SCOPE]: `IBufferDistributedCache` zero-copy distributed backing (`*.Abstractions`, namespace `Microsoft.Extensions.Caching.Distributed`)
- rail: runtime cache

This is the integration seam between Hybrid and the L2 store: the `DefaultHybridCache` runtime sniffs
its registered `IDistributedCache` at construction, and when the implementation also implements
`IBufferDistributedCache` it routes reads through `TryGetAsync(IBufferWriter<byte>)` into its pooled
buffer writer and writes through the `ReadOnlySequence<byte>` form, so payload bytes move L2↔serializer
with no intermediate `byte[]`. An L2 store that implements only `IDistributedCache` still works but
loses the zero-copy path. `RedisCache` (from `Microsoft.Extensions.Caching.StackExchangeRedis`) implements
`IBufferDistributedCache`, so the zero-copy path holds across the redis tier.

`IBufferDistributedCache : IDistributedCache` adds:
- `bool TryGet(string key, IBufferWriter<byte> destination) : bool`
- `ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) : ValueTask<bool>`
- `void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options) : void`
- `ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default) : ValueTask`

The inherited `IDistributedCache` members (`Get`/`GetAsync` → `byte[]?`, `Set`/`SetAsync` over `byte[]`, `Refresh`/`RefreshAsync`, `Remove`/`RemoveAsync`) remain on the contract; an `IBufferDistributedCache` implementer satisfies both the array contract (for non-Hybrid callers) and the buffer contract (for the Hybrid zero-copy path) from one storage row, bridging the synchronous members by blocking and treating `Refresh` as inert when absolute expiry is the only L2 lifetime.

## [04]-[REGISTRATION]

[REGISTRATION_SCOPE]: service and serializer admission (implementation assembly, namespace `Microsoft.Extensions.DependencyInjection`)
- rail: runtime cache

`HybridCacheServiceExtensions` on `IServiceCollection` — registration returns `IHybridCacheBuilder` so the serializer chain is fluently admitted on the same call:
- `AddHybridCache(this IServiceCollection services) : IHybridCacheBuilder`
- `AddHybridCache(this IServiceCollection services, Action<HybridCacheOptions> setupAction) : IHybridCacheBuilder`
- `AddKeyedHybridCache(this IServiceCollection services, object? serviceKey) : IHybridCacheBuilder`
- `AddKeyedHybridCache(this IServiceCollection services, object? serviceKey, Action<HybridCacheOptions> setupAction) : IHybridCacheBuilder`
- `AddKeyedHybridCache(this IServiceCollection services, object? serviceKey, string optionsName) : IHybridCacheBuilder`
- `AddKeyedHybridCache(this IServiceCollection services, object? serviceKey, string optionsName, Action<HybridCacheOptions> setupAction) : IHybridCacheBuilder` — keyed registration yields a distinct `HybridCache` resolvable by `[FromKeyedServices]`/`GetRequiredKeyedService`, so a per-lane cache profile is an admitted keyed row, not an ad-hoc service lookup.

`HybridCacheBuilderExtensions` on `IHybridCacheBuilder` — the serializer chain (probe order is registration order; first match wins):
- `AddSerializer<T>(this IHybridCacheBuilder builder, IHybridCacheSerializer<T> serializer) : IHybridCacheBuilder` — concrete instance for one `T`.
- `AddSerializer<T, TImplementation>(this IHybridCacheBuilder builder) : IHybridCacheBuilder where TImplementation : class, IHybridCacheSerializer<T>` — typed serializer for one `T`.
- `AddSerializerFactory(this IHybridCacheBuilder builder, IHybridCacheSerializerFactory factory) : IHybridCacheBuilder` — open-generic factory instance covering every `T`.
- `AddSerializerFactory<TImplementation>(this IHybridCacheBuilder builder) : IHybridCacheBuilder where TImplementation : class, IHybridCacheSerializerFactory` — open-generic factory by type.

`IHybridCacheBuilder` carries `IServiceCollection Services { get; }` so further DI rows compose on the builder.

`HybridCacheOptions` (the global policy value; the default JSON serializer is the fallback when no factory claims `T`):
- `HybridCacheEntryOptions? DefaultEntryOptions { get; set; }` — default expiration and flags applied when a call passes no options.
- `long MaximumPayloadBytes { get; set; }` — default `1048576` (1 MiB); a larger payload is not cached (returned uncached, not faulted).
- `int MaximumKeyLength { get; set; }` — default `1024`; an over-length key bypasses the cache.
- `bool DisableCompression { get; set; }` — globally disables the payload compressor.
- `bool ReportTagMetrics { get; set; }` — emits per-tag hit/miss metrics on the `HybridCache` EventSource.
- `object? DistributedCacheServiceKey { get; set; }` — selects which keyed `IDistributedCache` backs the L2 tier, so a keyed redis registration is the L2 of a specific `HybridCache` without ambient resolution.

## [05]-[IMPLEMENTATION_LAW]

[CACHE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Caching.Distributed` (the L2 buffer contract)
- contract root: `HybridCache` abstract base from `*.Abstractions` — read/write/remove/tag entrypoints; resolved from DI
- read root: the polymorphic `GetOrCreateAsync` family — one abstract `(string key, TState, factory, ...)` root with span and interpolated-handler key overloads and stateful/stateless factory mirrors; the `state` thread keeps the factory closure-free
- write root: `SetAsync<T>` with optional tag set; `RemoveAsync`/`RemoveByTagAsync` each carry a singular and a batch (`IEnumerable<string>`) overload
- policy root: `HybridCacheOptions.DefaultEntryOptions`, `MaximumPayloadBytes`, `MaximumKeyLength`, `DisableCompression`, `ReportTagMetrics`, `DistributedCacheServiceKey`
- entry policy: `HybridCacheEntryOptions` (`Expiration`, `LocalCacheExpiration`, `Flags`) — absolute expiry plus a shorter L1-only horizon plus the `HybridCacheEntryFlags` lane-disable axis
- serializer root: `IHybridCacheSerializer<T>` (`ReadOnlySequence<byte>`/`IBufferWriter<byte>` zero-copy) discovered via `IHybridCacheSerializerFactory.TryCreateSerializer<T>`
- L2 zero-copy root: `IBufferDistributedCache` — the runtime sniffs the buffer contract at registration; `TryGetAsync(IBufferWriter<byte>)` and `SetAsync(ReadOnlySequence<byte>)` move payload bytes with no intermediate array
- invalidation root: `RemoveAsync` (key) and `RemoveByTagAsync` (tag group), each singular and batched

[STACKING]:
- Persistence contributes ONE L2 store row implementing `IBufferDistributedCache` (zero-copy storage) plus `IHybridCacheSerializerFactory` (a MessagePack codec for every payload `T`), registered through `AddSerializerFactory` so the suite never registers a per-type serializer.
- The `HybridCache` runtime composes ON TOP of that contribution: `GetOrCreateAsync` drives stampede-protected population, the serializer factory's `IHybridCacheSerializer<T>` round-trips payload through `ReadOnlySequence<byte>`/`IBufferWriter<byte>` with zero intermediate arrays, and the `IBufferDistributedCache` sniff routes the L2 read/write through the pooled buffer path — so MessagePack + the buffer contract + the redis L2 + the stampede gate compose into one zero-copy read-through rail.
- The redis tier (`Microsoft.Extensions.Caching.StackExchangeRedis` `RedisCache`) implements `IBufferDistributedCache`, so the zero-copy seam survives an L2 swap from the embedded key-value store to redis; `DistributedCacheServiceKey` selects the keyed L2 per cache profile.
- `HybridCacheEntryFlags` is the per-lane routing the AppHost `CacheLane` rows carry (`DisableLocalCache` on the blob lane, `None` on the model-result lane), so the L1/L2 disable axis is a closed flag value on the lane row, not a branch.
- `RemoveByTagAsync` is the lane-scoped invalidation the AppHost cache surface drives by lane-key tag; the batch `IEnumerable<string>` overloads cut a tag set in one call.

[LOCAL_ADMISSION]:
- `HybridCache` is a runtime policy surface, not a domain repository; it is resolved from DI, never constructed (`DefaultHybridCache` is the internal implementation).
- Cache keys, tags, entry options, flags, and serializer policy enter as values; the interpolated-handler key overloads keep key construction allocation-free.
- Keyed cache registration (`AddKeyedHybridCache`) plus `DistributedCacheServiceKey` represent an admitted cache profile selecting its own L2, not ad-hoc service lookup.
- Stampede control stays behind `GetOrCreateAsync`; callers never duplicate population locks. The `state` parameter keeps the factory closure-free.
- The L2 contribution implements `IBufferDistributedCache` (not bare `IDistributedCache`) so the Hybrid zero-copy path is reached; bridging the array contract and the synchronous members is contract totality, not a second storage row.
- `MaximumPayloadBytes`/`MaximumKeyLength` are silent bypass guards (an over-limit entry is returned uncached, never faulted); the blob lane sets `DisableLocalCache` rather than relying on the L1 size bound.
- Tag invalidation is an explicit cache capability and never substitutes for durable store integrity; the singular and batch tag overloads are one capability, not two owners.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid` (+ `Microsoft.Extensions.Caching.Abstractions` contracts)
- Owns: L1+L2 hybrid cache registration, the abstract `HybridCache` read/write/tag contract, the zero-copy serializer chain, and the `IBufferDistributedCache` L2 seam
- Accept: cache policy as runtime data — entry options, flags, tags, keyed L2 selection, and one serializer factory
- Reject: static cache clients, hidden invalidation channels, per-type serializer proliferation, and an L2 store that implements only `IDistributedCache` where the zero-copy buffer contract is admitted
