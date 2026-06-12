# [APPHOST_RESOURCE_LANES]

Bounded runtime resource lanes for the Rasm.AppHost spine: the HybridCache read-through port with lane-keyed tag invalidation, delegate-row object pools, and drainable queue rows that complete under the lifecycle conductor. The page owns the CacheLane axis, the PoolPolicy row shape, and the DrainSpec/DrainQueue family; DeadlineClass and DrainBand bind lifetimes and rank as settled vocabulary, the L2 store and serializer factory arrive as the single Persistence contribution, and lane counts leave as telemetry consequence.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                 |
| :-----: | :----------- | :--------------------------------------------------------------------- |
|   [1]   | CACHE_PORT   | One read-through cache entry; lane rows bind tags, lifetimes, options  |
|   [2]   | OBJECT_POOLS | Delegate-row pool policy; reset law; leak tracking on the test row     |
|   [3]   | DRAIN_QUEUES | DrainSpec frozen rows; pipe-versus-network split; conductor completion |

## [2]-[CACHE_PORT]

- Owner: `CacheLane` `[SmartEnum<string>]` under the `LaneKeyPolicy` ordinal accessor; `CacheSurface` attaches the dispatch to `HybridCache` as one extension block.
- Cases: `ModelResult`, `Projection`, `ArtifactBlob`.
- Entry: `ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Option<Seq<string>> tags = default, CancellationToken token = default)`.
- Auto: `GetOrCreateAsync` owns stampede single-flight; local and distributed hit, miss, and write counts, stampede joins, and tag invalidations ride the package `Microsoft-Extensions-HybridCache` event source as polling counters with zero call-site metric code.
- Packages: Microsoft.Extensions.Caching.Hybrid; NodaTime; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one lane row on `CacheLane`; a lifetime or flag change is one policy value; zero new surface.
- Boundary: the L2 `IDistributedCache` and the `IHybridCacheSerializerFactory` registration arrive as the single Persistence row; `AddHybridCache` composes after the DI `TimeProvider` registration so the test row's `FakeTimeProvider` drives creation stamps and tag cuts; this port deletes hand-rolled double-checked caches, `ICacheService` wrappers, and every second cache owner in the suite.

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
    public static readonly CacheLane ModelResult = new("model-result", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None);
    public static readonly CacheLane Projection = new("projection", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None);
    public static readonly CacheLane ArtifactBlob = new("artifact-blob", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None);

    public DeadlineClass Ttl { get; }

    public DeadlineClass L1Ttl { get; }

    public HybridCacheEntryFlags Flags { get; }

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

        public ValueTask Purge(CancellationToken token = default) =>
            cache.RemoveByTagAsync("*", token);
    }
}
```

| [INDEX] | [LAW]            | [RULING]                                                                                                                                                                                                                                                                                                                               |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | tag cut          | `RemoveByTagAsync` records a timestamp cut; pre-cut entries read as misses in both tiers and persist until natural expiry — logical, never physical                                                                                                                                                                                    |
|   [2]   | wildcard         | `*` is reserved and illegal as a write-time tag; the wildcard cut invalidates everything including untagged entries                                                                                                                                                                                                                    |
|   [3]   | glob             | no pattern matching exists; only the bare `*` is special                                                                                                                                                                                                                                                                               |
|   [4]   | physical removal | `RemoveAsync` is the physical sibling — it deletes the key from both tiers                                                                                                                                                                                                                                                             |
|   [5]   | tag vocabulary   | tags derive from `CacheLane` keys and admitted owner keys; a free-string tag is the rejected form                                                                                                                                                                                                                                      |
|   [6]   | cross-process L1 | peer-process L1 staleness is TTL-bounded with no backplane; convergence rides natural expiry or the next tag cut                                                                                                                                                                                                                       |
|   [7]   | clock seam       | the cache implementation service-locates the DI `TimeProvider` with system fallback; creation stamps and tag cuts ride the injected clock                                                                                                                                                                                              |
|   [8]   | L1 TTL split     | absolute L1 expiry is delegated to the memory-cache entry's `AbsoluteExpirationRelativeToNow` under the memory cache's own clock — read-time revalidation checks only wildcard and tag cuts against the injected clock, so advancing `FakeTimeProvider` never expires an L1 entry by TTL and specs assert via tag cut or `RemoveAsync` |
|   [9]   | guards           | `MaximumPayloadBytes` stays the 1 MiB package default and `MaximumKeyLength` the 1024 default; the package clamps `LocalCacheExpiration` to `Expiration` when the L1 row exceeds the L2 row; `ReportTagMetrics` is enabled because the lane tag vocabulary is closed and low-cardinality                                               |
|  [10]   | test double      | no fake cache type exists or gets hand-rolled; `SetAsync` preloads spec state through the real implementation                                                                                                                                                                                                                          |

## [3]-[OBJECT_POOLS]

- Owner: `PoolPolicy<T>` — one delegate-row `PooledObjectPolicy<T>`; providers and pools stay package surfaces, never wrapped.
- Entry: `bool Return(T pooled)`.
- Auto: return-time cleanup folds `IResettable.TryReset` before the row's sanity predicate; a false return discards the instance instead of re-pooling it.
- Packages: Microsoft.Extensions.ObjectPool.
- Growth: one pool policy row per pooled type; a capacity change is one policy value; zero new surface.
- Boundary: pooled instances never carry request, document, or host state across returns; `DefaultObjectPoolProvider` mints every pool with `MaximumRetained` at the package default of twice the processor count, the text pool rides `CreateStringBuilderPool` with the `InitialCapacity` 100 and `MaximumRetainedCapacity` 4096 package defaults, and `LeakTrackingObjectPoolProvider` wraps the provider on the test-host row only — this cluster deletes ad hoc static pools and per-site `StringBuilder` churn.

```csharp signature
public sealed class PoolPolicy<T>(Func<T> create, Func<T, bool> sane) : PooledObjectPolicy<T> where T : class {
    public override T Create() => create();

    public override bool Return(T pooled) => (pooled is not IResettable resettable || resettable.TryReset()) && sane(pooled);
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

| [INDEX] | [LAW]        | [RULING]                                                                                                                                                                                   |
| :-----: | :----------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | kind split   | `Channel.CreateBounded` owns simple pipe seams; Dataflow blocks own completion-propagating networks — the row's topology fixes the kind, never the call site                               |
|   [2]   | backpressure | `WriteAsync` and `SendAsync` await fullness on `Wait` rows; `TryWrite` and `Post` are legal only on receipted-loss rows; `NullTarget` absorption is spelled at the link site               |
|   [3]   | loss         | a `DropOldest` row opens only with an `onDrop` receipt delegate; `Open` rejects an unreceipted-loss row on the `Fin` rail                                                                  |
|   [4]   | grouping     | `BatchBlock` carries receipt-grade batched hand-off, with `TriggerBatch` flushing a partial batch at drain; the reservation rail, `Encapsulate`, `AsObservable`, and `AsObserver` stay out |
|   [5]   | drain        | `Drained` completes intake then awaits `Completion` under the conductor token at the row's band; evidence rows complete inside the final band before exporter flush                        |
|   [6]   | fault        | a faulted block or channel fails `Completion`; the conductor folds the failure into the unload receipt instead of swallowing it                                                            |
|   [7]   | homonym      | `DrainQueue` is the AppHost name for process-level drainable queues; `WorkLane` belongs to Compute — one altitude per name                                                                 |
