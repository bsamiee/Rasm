# [RASM_GRASSHOPPER_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` backs the Grasshopper session boundary's document-tagged cache: the composition root supplies one `HybridCache`, and the `SessionCache` boundary adapter reads through it per live document — keying by weak document identity, tagging each value with its document, so one session mutation evicts a single document's slots and never another's.

Substrate canonical members live on `libs/csharp/.api/api-hybrid-cache.md`; this overlay carries only the Grasshopper delta — the session-scoped binding, the `gh:{documentId:N}:{slot}` key and `gh-doc:{documentId:N}` tag scheme, the `SlotPolicy` entry-policy rows, the payload law the `Shell/session` page composes, and the app-root obligations the composition root discharges.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-hybrid-cache.md`
- substrate catalog owns the contract/options/serializer type roster, the operation and registration call-shape tables, and the package/asset facts — this overlay never re-states them
- rail: runtime cache

## [02]-[GRASSHOPPER_BINDINGS]

[GRASSHOPPER_BINDINGS]: `Shell/session.md` seam facts
- `DocumentToken` (`[BoundaryAdapter]`) assigns one stable `Guid` per live `Document` through a `ConditionalWeakTable`; the token dies with the document, and `Document.Hash` stays content identity and never enters cache addressing.
- `CacheSlot` is a `[ValueObject<string>]` trimmed-nonblank concern identity; `SessionCache` keys entries `gh:{documentId:N}:{slot}` and stamps every minted value with the exact `gh-doc:{documentId:N}` tag.
- `SessionCache.Remember<TState, T>(Guid documentId, CacheSlot slot, TState state, Func<TState, CancellationToken, ValueTask<T>> mint, Option<SlotPolicy> policy = default, CancellationToken cancel = default): ValueTask<T>` composes the stateful `HybridCache.GetOrCreateAsync<TState, T>(string, TState, factory, …)` overload with a `static` factory — the `(State, Mint)` tuple threads through `state`, so the delegate captures nothing and stays cached.
- `SlotPolicy` `[SmartEnum<int>]` carries the per-call `HybridCacheEntryOptions?` as policy rows: `Shared` (null — substrate `DefaultEntryOptions` governs), `Resident` (`Flags = HybridCacheEntryFlags.DisableDistributedCache` — process-local Eto-affine payloads stay L1-only), `Recency` (`LocalCacheExpiration` short horizon under a longer absolute `Expiration` — a hot key re-reads L2 without re-minting). An opaque options argument at a call site is the deleted form.
- `SessionCache.Evict(Guid documentId, CancellationToken cancel = default): ValueTask` composes the singular `HybridCache.RemoveByTagAsync(string tag, CancellationToken)` overload against the document tag; one call marks every slot minted for one document stale.
- `SessionCache` (`[BoundaryAdapter]`) takes the `HybridCache` by constructor injection from the composition root; `ValueTask` stays the package carrier, and a kernel consumer bridges at its own seam. It never registers or instantiates a cache.

## [03]-[IMPLEMENTATION_LAW]

[CACHE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `System.Runtime.CompilerServices` (`ConditionalWeakTable` weak document identity)
- contract root: `HybridCache` (abstract, from the `Abstractions` companion), resolved from DI at the composition root
- read root: `SessionCache.Remember` over the stateful `GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions?, IEnumerable<string>? tags, CancellationToken)` root — key `gh:{documentId:N}:{slot}`, tag set `[gh-doc:{documentId:N}]`, closure-free `static` factory
- invalidation root: `SessionCache.Evict` over the singular `RemoveByTagAsync(string tag, CancellationToken)` — document-grouped eviction, one exact tag per document
- entry policy: a `SlotPolicy` row flows through `Remember` and projects its `HybridCacheEntryOptions?` — `Shared` null-defers, `Resident` binds `HybridCacheEntryFlags.DisableDistributedCache`, `Recency` binds `LocalCacheExpiration` under absolute `Expiration`; the substrate default entry policy, stampede control, serializer selection, and maximum-key/maximum-payload bypass behavior stay in force under the read-through
- tag semantics: tag invalidation makes matching entries stale for subsequent reads and never promises physical deletion from L1 or L2

[APP_ROOT_OBLIGATIONS]: composition surfaces this folder cannot own — the GH plugin root's `AddHybridCache`/`IHybridCacheBuilder` registration discharges each row
- raster-byte serializer: the folder's encoded-raster payloads are the `IHybridCacheSerializer<T>` zero-copy case (`ReadOnlySequence<byte>`/`IBufferWriter<byte>`); the default JSON codec round-trips them as base64 waste, so the root admits one buffer serializer through `AddSerializer<T>`/`AddSerializerFactory`
- payload bound: `HybridCacheOptions.MaximumPayloadBytes` returns an over-limit entry uncached with no fault, so the root sizes the bound against the largest admitted canvas raster instead of inheriting the 1 MiB default silently
- tag metrics: `HybridCacheOptions.ReportTagMetrics` surfaces per-tag hit/miss on the `HybridCache` EventSource; `gh-doc:{documentId:N}` is the per-document dimension `Shell/session.md` mints, so the root sets the flag and the observability stack reads the EventSource — this folder emits nothing

[LOCAL_ADMISSION]:
- Hybrid cache is the session boundary's read-through, not a domain repository; `SessionCache` is a `[BoundaryAdapter]`, never a static cache client.
- Cache keys derive from weak document identity (`DocumentToken`) with a `CacheSlot` concern; a hash-keyed, path-keyed, or display-string key is the rejected form.
- Cache payloads are detached serializable values — encoded raster bytes, layout measurements, parse receipts; a `GhScope`, live host object, Eto bitmap, lease, or delegate never becomes a cache value.
- Tag invalidation is an explicit cache capability and never substitutes for durable store integrity; document-grouped eviction is one exact tag value, not a per-entry sweep.
- A new cached concern is one `CacheSlot` value at the call site; a new invalidation axis is one exact tag value — never a new adapter or entrypoint.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid` (+ `Abstractions` contracts)
- Owns: the session-scoped document-tagged cache binding — one composition-root `HybridCache`, the `SessionCache.Remember` read-through, and `SessionCache.Evict` tag invalidation
- Accept: weak document identity with a `CacheSlot` concern as the key, a `SlotPolicy` entry-policy row, a `static` closure-free factory, detached serializable payloads, document-grouped tag eviction
- Reject: a second `HybridCache` registration; a raw `HybridCacheEntryOptions` argument where a `SlotPolicy` row carries the policy; a hash-keyed or display-string cache key; a live host object, scope, bitmap, lease, or delegate as a cache value; a per-entry eviction where a document tag fits
