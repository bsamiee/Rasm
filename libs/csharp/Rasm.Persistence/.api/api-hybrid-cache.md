# [RASM_PERSISTENCE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` splits its two tiers across the seam this folder owns half of: Persistence contributes the durable L2 store, the one serializer factory, the tenant-partitioned key scope, and the Redis invalidation backplane, while the AppHost `HybridCache` runtime composes L1, stampede collapse, and tag invalidation on top. Substrate canonical members live at `libs/csharp/.api/api-hybrid-cache.md`; this overlay carries only the Persistence delta — the `Query/cache` L2-contribution bindings the cache lanes compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-hybrid-cache.md`
- contract/options/serializer type roster, operation and registration call-shape tables, and package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: runtime cache

## [02]-[PERSISTENCE_BINDINGS]

- `CacheL2Store` contributes exactly one Marten-backed `IBufferDistributedCache` row — the buffer contract spares the cache-runtime intermediate-array copy, persisting one `byte[]` at the Marten document seam; `TryGetAsync` rejects expired rows and refreshes a sliding deadline beneath its absolute cap, `RefreshAsync` advances the same deadline.
- `CacheCodecFactory` contributes the one `IHybridCacheSerializerFactory` — a MessagePack `CacheCodec<T>` for every payload `T`, registered through the AppHost `CacheSurface.Register(services, contributed)` `AddSerializerFactory` seam on every keyed builder, never a per-type `AddSerializer<T>`.
- `CachePartition.Scoped` derives the `TenantId`-partitioned content-address key, so the L1/L2 lane key and the durable row read one tenant-scoped identity.
- `CacheBackplane` folds Redis RESP3 `CLIENT TRACKING ON BCAST PREFIX` tracking or beat invalidation through one `InvalidationMode` drain into the matching `HybridCache` removal; the backplane binds only where the Redis `CacheLane.Store` row is live, and it is lossy by design — a missed beat is a TTL-bounded stale read, never corruption.

## [03]-[IMPLEMENTATION_LAW]

[L2_CONTRIBUTION]:
- lane routing: the L2 store backs every lane whose `CacheLane.Store` is set (`ModelResult`, `Projection` on `durable-l2`); a lane with no `Store` (`ArtifactBlob`) resolves the default `HybridCache` with no L2 leg
- L1/L2 partition per lane: the AppHost `HybridCacheEntryFlags` lane axis — `DisableLocalCache` on the blob lane so an oversized payload never pins L1, `None` on the model-result lane
- codec: the L2 wire is the `messagepack` `SnapshotCodec.Binary` row, so durable cache bytes and snapshot/event bytes share one codec and one `Instant` formatter
- metering: the contribution emits no cache fact of its own — hit/miss/evict ride the AppHost `HybridCacheOptions.ReportTagMetrics` by lane tag, and the durable row lifecycle rides the retention sweep

[LOCAL_ADMISSION]:
- Persistence owns the L2-store and serializer half; the AppHost port owns L1, stampede single-flight, and tag invalidation — one cache owner across both halves, never a second registration.
- Tag invalidation is a cache capability and never substitutes for durable store integrity: a tag cut is a logical miss-until-expiry, and the durable reuse rows live on the retention sweep, not the cache TTL.
- Cache keys derive from the content-address seam through the tenant scope; a path-keyed or display-string key is rejected.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid` (+ `Abstractions`; L2 via the Marten `IBufferDistributedCache` contribution, backplane via `StackExchange.Redis`)
- Owns: the Persistence L2 store row, the one serializer factory, the tenant key scope, and the invalidation backplane drain
- Accept: buffer-contract L2 storage at the Marten seam, one factory registration through `CacheSurface.Register`, `InvalidationMode`-selected backplane composition per deployment
- Reject: a second `HybridCache` registration beside the AppHost root; a cache-local serializer beside the snapshot codec row; the backplane hardened into a delivery guarantee beside `Version/egress`
