# [APPHOST_RESOURCE_LANES]

Bounded runtime resource lanes for the Rasm.AppHost spine: the HybridCache read-through port with per-lane keyed L2 topology and lane-keyed tag invalidation, delegate-row object pools that rent and recycle, and drainable queue rows that complete under the lifecycle conductor. The page owns the CacheLane axis, the PoolPolicy row shape with its concrete pool rows, and the DrainSpec/DrainQueue family; DeadlineClass and DrainBand bind lifetimes and rank as settled vocabulary, each lane's keyed L2 store and the serializer factory arrive as the single Persistence contribution, and lane counts leave as telemetry consequence.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                    |
| :-----: | :----------- | :------------------------------------------------------------------------ |
|   [1]   | CACHE_PORT   | One read-through entry; lane rows bind tags, lifetimes, options, keyed L2 |
|   [2]   | OBJECT_POOLS | Delegate-row pool policy; concrete text pool; rent, reset, leak tracking  |
|   [3]   | DRAIN_QUEUES | DrainSpec frozen rows; pipe-versus-network split; conductor completion    |

## [2]-[CACHE_PORT]

- Owner: `CacheLane` `[SmartEnum<string>]` under the `LaneKeyPolicy` ordinal accessor; `CacheSurface` attaches the dispatch to `HybridCache` as one extension block and resolves each lane's keyed cache by its `Store` column.
- Cases: `ModelResult`, `Projection`, `ArtifactBlob`.
- Entry: `ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Option<Seq<string>> tags = default, CancellationToken token = default)`.
- Auto: `GetOrCreateAsync` owns stampede single-flight; local and distributed hit, miss, and write counts, stampede joins, and tag invalidations ride the package `Microsoft-Extensions-HybridCache` event source as polling counters with zero call-site metric code; `Cache` resolves the lane's keyed `HybridCache` from the provider by the lane key so each lane reads its own L2 topology.
- Packages: Microsoft.Extensions.Caching.Hybrid; NodaTime; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one lane row on `CacheLane`; a lifetime or flag change is one policy value; a new L2 topology is one `Store` value on the lane row; zero new surface.
- Boundary: the L2 `IDistributedCache` registered under the lane's `Store` key and the `IHybridCacheSerializerFactory` registration arrive as the single Persistence row; `Register` composes one `AddKeyedHybridCache(lane.Key)` per lane row whose `Store` is set, binding `DistributedCacheServiceKey` to that store key so each such lane reads its own keyed L2, while a lane with no `Store` resolves the default `AddHybridCache` service — one cache owner across both paths, never a second; registration composes after the DI `TimeProvider` registration so the test row's `FakeTimeProvider` drives creation stamps and tag cuts; this port deletes hand-rolled double-checked caches, `ICacheService` wrappers, and every second cache owner in the suite.

```csharp signature
public sealed class LaneKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<LaneKeyPolicy, string>]
[KeyMemberComparer<LaneKeyPolicy, string>]
public sealed partial class CacheLane {
    public static readonly CacheLane ModelResult = new("model-result", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None, store: "durable-l2");
    public static readonly CacheLane Projection = new("projection", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None, store: "durable-l2");
    public static readonly CacheLane ArtifactBlob = new("artifact-blob", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None, store: default);

    public DeadlineClass Ttl { get; }

    public DeadlineClass L1Ttl { get; }

    public HybridCacheEntryFlags Flags { get; }

    public Option<string> Store { get; }

    public HybridCacheEntryOptions Entry => new() { Expiration = Ttl.Allotted.ToTimeSpan(), LocalCacheExpiration = L1Ttl.Allotted.ToTimeSpan(), Flags = Flags };

    public string Scoped(string key) => $"{Key}:{key}";
}
```

```csharp signature
public static class CacheSurface {
    extension(HybridCache cache) {
        public ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Option<Seq<string>> tags = default, CancellationToken token = default) =>
            cache.GetOrCreateAsync(lane.Scoped(key), state, factory, lane.Entry, tags.IfNone(Seq<string>()).Add(lane.Key), token);

        public ValueTask Invalidate(CacheLane lane, CancellationToken token = default) =>
            cache.RemoveByTagAsync(lane.Key, token);

        public ValueTask Invalidate(Seq<string> tags, CancellationToken token = default) =>
            cache.RemoveByTagAsync(tags, token);

        public ValueTask Remove(CacheLane lane, string key, CancellationToken token = default) =>
            cache.RemoveAsync(lane.Scoped(key), token);
    }

    extension(IServiceProvider provider) {
        public HybridCache Cache(CacheLane lane) =>
            lane.Store.IsSome
                ? provider.GetRequiredKeyedService<HybridCache>(lane.Key)
                : provider.GetRequiredService<HybridCache>();
    }

    public static IServiceCollection Register(IServiceCollection services) =>
        CacheLane.Items.Fold(services, static (current, lane) =>
            lane.Store.Case is string store
                ? (current.AddKeyedHybridCache(lane.Key, options => {
                        options.DefaultEntryOptions = lane.Entry;
                        options.DistributedCacheServiceKey = store;
                    }), current).Item2
                : current);
}
```

Cache semantics ride these rulings:

- Keyed L2 topology: a lane's `Store` value is the distributed-cache service key its `AddKeyedHybridCache(lane.Key)` registration binds through `DistributedCacheServiceKey`, so `ModelResult` and `Projection` share the `durable-l2` store while `ArtifactBlob` carries no `Store` and resolves the default cache; `Cache(lane)` resolves the keyed `HybridCache` by lane key for a stored lane and the default service otherwise — one cache contract, distinct L2 backings, never a second cache owner. A lane row's `Store` is the only growth axis for L2 topology.
- Read routing: a consumer obtains the lane's cache as `provider.Cache(lane)` and reads through it as `provider.Cache(lane).Read(lane, key, …)`, so a stored lane's read hits its keyed L2 and never the default cache; `Read`, `Invalidate`, and `Remove` ride the same `Cache(lane)`-resolved receiver as the registration, so the keyed-store routing is one resolution per consumer, never a default-cache read against a `Store`-keyed lane.
- Tag cut: `RemoveByTagAsync` records a timestamp cut; pre-cut entries read as misses in both tiers and persist until natural expiry — logical, never physical; `RemoveAsync` is the physical sibling deleting the key from both tiers.
- Tag vocabulary: tags derive from `CacheLane` keys and admitted owner keys; a free-string tag is the rejected form; every write carries its lane key tag, so `Invalidate(lane)` cuts the whole lane through `RemoveByTagAsync(lane.Key)` and `Invalidate(tags)` cuts a tag set — a lane-scoped cut is the widest invalidation the closed tag vocabulary admits, and a global reset rides provider disposal at host unload, never a write-time pattern tag.
- Cross-process L1: peer-process L1 staleness is TTL-bounded with no backplane; convergence rides natural expiry or the next tag cut.
- Clock seam: the cache implementation service-locates the DI `TimeProvider` with system fallback, so creation stamps and tag cuts ride the injected clock; absolute L1 expiry is delegated to the memory-cache entry's `AbsoluteExpirationRelativeToNow` under the memory cache's own clock — read-time revalidation checks only tag cuts against the injected clock, so advancing `FakeTimeProvider` never expires an L1 entry by TTL and specs assert via tag cut or `RemoveAsync`.
- Guards: `MaximumPayloadBytes` stays the 1 MiB package default and `MaximumKeyLength` the 1024 default; the package clamps `LocalCacheExpiration` to `Expiration` when the L1 row exceeds the L2 row; `ReportTagMetrics` is enabled because the lane tag vocabulary is closed and low-cardinality.
- Test double: no fake cache type exists or gets hand-rolled; `SetAsync` preloads spec state through the real implementation.

## [3]-[OBJECT_POOLS]

- Owner: `PoolPolicy<T>` — one delegate-row `PooledObjectPolicy<T>` with the `Pool` accessor that mints and owns its package pool; providers and pools stay package surfaces, never wrapped.
- Entry: `T Get()` leases an instance through `Pool.Get`; `void Recycle(T pooled)` returns it through `Pool.Return`, where the package re-invokes `Return` to decide re-pooling.
- Auto: `Return` folds `IResettable.TryReset` before the row's sanity predicate, so a false return discards the instance instead of re-pooling it; `Pool` mints once through `ObjectPool.Create<T>` over the policy and caches the `ObjectPool<T>` for the row's lifetime.
- Packages: Microsoft.Extensions.ObjectPool.
- Growth: one pool policy row per pooled type; a capacity change is one policy value; zero new surface.
- Boundary: pooled instances never carry request, document, or host state across returns; `ObjectPool.Create<T>` mints the default-bounded pool and `ObjectPoolProvider.Create<T>` mints a row whose retention overrides the `DefaultObjectPoolProvider` `MaximumRetained` default of twice the processor count; the text pool is the `Text` row — a `StringBuilder` policy reset clear-on-return with the `InitialCapacity` 100 and `MaximumRetainedCapacity` 4096 package defaults, and a typed `DefaultPooledObjectPolicy<T>` row covers a parameterless reference type whose default construction and reset suffice — and `LeakTrackingObjectPoolProvider` wraps the provider on the test-host row only; this cluster deletes ad hoc static pools and per-site `StringBuilder` churn.

```csharp signature
public sealed class PoolPolicy<T>(Func<T> create, Func<T, bool> sane) : PooledObjectPolicy<T> where T : class {
    private ObjectPool<T>? pool;

    public ObjectPool<T> Pool => pool ??= ObjectPool.Create(this);

    public T Get() => Pool.Get();

    public void Recycle(T pooled) => Pool.Return(pooled);

    public override T Create() => create();

    public override bool Return(T pooled) => (pooled is not IResettable resettable || resettable.TryReset()) && sane(pooled);
}

public static class Pools {
    public static readonly PoolPolicy<StringBuilder> Text = new(
        create: static () => new StringBuilder(capacity: 100),
        sane: static builder => builder.Capacity <= 4096 && (builder.Clear(), true).Item2);

    public static PoolPolicy<T> Default<T>() where T : class, new() =>
        new(create: static () => new DefaultPooledObjectPolicy<T>().Create(),
            sane: static pooled => new DefaultPooledObjectPolicy<T>().Return(pooled));
}
```

## [4]-[DRAIN_QUEUES]

- Owner: `DrainSpec` frozen rows materialized through the `DrainQueue<T>` union; `DrainSurface` carries options projection, open, and drain as one extension surface.
- Cases: `Pipe(DrainSpec Spec, Channel<T> Channel)` for simple producer-consumer seams; `Network(DrainSpec Spec, ITargetBlock<T> Intake, IDataflowBlock Tail)` for completion-propagating block graphs.
- Entry: `Task Drained(CancellationToken token)`.
- Receipt: `DropOldest` rows surface every lost item through the open-time `onDrop` delegate; a faulted `Completion` projects typed evidence onto the lifecycle fault rail.
- Packages: BCL inbox; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one `DrainSpec` row per queue; `JoinBlock` or `WriteOnceBlock` admission is one row when join or write-once semantics are receipt material; zero new surface.
- Boundary: `System.Threading.Tasks.Dataflow` rides the shared framework and its central pin stays a transitive floor, never a direct project asset; `DrainQueue` names process-level drainable queues while `WorkLane` stays the Compute solve-path name; completion awaits land at the row's `DrainBand` under the conductor's cancellation scope — this family deletes per-lane queue classes and free-floating background loops.

```csharp signature
public sealed record DrainSpec(
    string Name,
    int Capacity,
    int MaxDegree,
    bool Ordered,
    BoundedChannelFullMode FullMode,
    DrainBand Band,
    DeadlineClass Deadline,
    Option<int> Batch = default,
    Option<TaskScheduler> Scheduler = default) {
    public static readonly DrainSpec ReceiptFanIn = new(nameof(ReceiptFanIn), Capacity: 1024, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative, Batch: 64);

    public static readonly DrainSpec SupportCapture = new(nameof(SupportCapture), Capacity: 512, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.DropOldest, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DrainQueue<T> {
    private DrainQueue() { }

    public sealed record Pipe(DrainSpec Spec, Channel<T> Channel) : DrainQueue<T>;

    public sealed record Network(DrainSpec Spec, ITargetBlock<T> Intake, IDataflowBlock Tail) : DrainQueue<T>;
}

public static class DrainSurface {
    extension(DrainSpec spec) {
        public BoundedChannelOptions PipeOptions() => new(spec.Capacity) {
            FullMode = spec.FullMode,
            SingleReader = spec.MaxDegree is 1,
            SingleWriter = false,
        };

        public ExecutionDataflowBlockOptions NetworkOptions(CancellationToken token) => new() {
            BoundedCapacity = spec.Capacity,
            MaxDegreeOfParallelism = spec.MaxDegree,
            EnsureOrdered = spec.Ordered,
            CancellationToken = token,
            TaskScheduler = spec.Scheduler.IfNone(TaskScheduler.Default),
        };

        public Fin<DrainQueue<T>> Open<T>(Option<Action<T>> onDrop = default) =>
            spec.FullMode is BoundedChannelFullMode.Wait || onDrop.IsSome
                ? Fin.Succ<DrainQueue<T>>(new DrainQueue<T>.Pipe(spec, onDrop is { IsSome: true, Case: Action<T> drop }
                    ? Channel.CreateBounded<T>(spec.PipeOptions(), drop)
                    : Channel.CreateBounded<T>(spec.PipeOptions())))
                : Fin.Fail<DrainQueue<T>>(Error.New($"unreceipted-loss:{spec.Name}"));

        public DrainQueue<T> Open<T>(ITargetBlock<T> intake, IDataflowBlock tail) =>
            new DrainQueue<T>.Network(spec, intake, tail);
    }

    extension<T>(DrainQueue<T> queue) {
        public DrainSpec Row => queue.Switch(
            pipe: static p => p.Spec,
            network: static n => n.Spec);

        public Task Drained(CancellationToken token) => queue.Switch(
            state: token,
            pipe: static (s, p) => (p.Channel.Writer.TryComplete(), p.Channel.Reader.Completion.WaitAsync(s)).Item2,
            network: static (s, n) => (fun(n.Intake.Complete)(), n.Tail.Completion.WaitAsync(s)).Item2);
    }
}
```

Queue semantics ride these rulings:

- Kind split: `Channel.CreateBounded` owns simple pipe seams; Dataflow blocks own completion-propagating networks — the row's topology fixes the kind, never the call site.
- Backpressure: `WriteAsync` and `SendAsync` await fullness on `Wait` rows; `TryWrite` and `Post` are legal only on receipted-loss rows; `NullTarget` absorption is spelled at the link site.
- Loss: a `DropOldest` row opens only with an `onDrop` receipt delegate; `Open` rejects an unreceipted-loss row on the `Fin` rail.
- Grouping: `BatchBlock` carries receipt-grade batched hand-off, with `TriggerBatch` flushing a partial batch at drain; the reservation rail, `Encapsulate`, `AsObservable`, and `AsObserver` stay out.
- Drain and fault: `Drained` completes intake then awaits `Completion` under the conductor token at the row's band; evidence rows complete inside the final band before exporter flush; a faulted block or channel fails `Completion`, and the conductor folds the failure into the unload receipt instead of swallowing it.
