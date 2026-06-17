# [APPHOST_RESOURCE_LANES]

Bounded runtime resource lanes for the Rasm.AppHost spine: the HybridCache read-through port with per-lane keyed L2 topology and lane-keyed tag invalidation, delegate-row object pools that rent and recycle, and drainable queue rows that complete under the lifecycle conductor. The page owns the CacheLane axis, the PoolPolicy row shape with its concrete pool rows, and the DrainSpec/DrainQueue family; DeadlineClass and DrainBand bind lifetimes and rank as settled vocabulary, each lane's keyed L2 store and the serializer factory arrive as the single Persistence contribution, and lane counts leave as telemetry consequence.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                    |
| :-----: | :----------- | :------------------------------------------------------------------------ |
|   [1]   | CACHE_PORT   | One read-through entry; lane rows bind tags, lifetimes, options, keyed L2 |
|   [2]   | OBJECT_POOLS | Delegate-row pool policy; concrete text pool; rent, reset, leak tracking  |
|   [3]   | DRAIN_QUEUES | DrainSpec frozen rows; DrainKind topology; fan-out, join, coalesce blocks |

## [2]-[CACHE_PORT]

- Owner: `CacheLane` `[SmartEnum<string>]` under the `LaneKeyPolicy` ordinal accessor; `CacheSurface` attaches the dispatch to `HybridCache` as one extension block and resolves each lane's keyed cache by its `Store` column.
- Cases: `ModelResult`, `Projection`, `ArtifactBlob`.
- Entry: `ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Option<Seq<string>> tags = default, CancellationToken token = default)`.
- Auto: `GetOrCreateAsync` owns stampede single-flight; local and distributed hit, miss, and write counts, stampede joins, and tag invalidations ride the package `Microsoft-Extensions-HybridCache` event source as polling counters with zero call-site metric code; `Cache` resolves the lane's keyed `HybridCache` from the provider by the lane key so each lane reads its own L2 topology.
- Packages: Microsoft.Extensions.Caching.Hybrid; NodaTime; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one lane row on `CacheLane`; a lifetime or flag change is one policy value; a new L2 topology is one `Store` value on the lane row; a payload-guard retune is one `MaxPayloadBytes` value; zero new surface.
- Boundary: the L2 `IDistributedCache` registered under the lane's `Store` key and the `IHybridCacheSerializerFactory` arrive as the single Persistence contribution — `Register` admits that one factory through `AddSerializerFactory` on every keyed builder, never a per-type `AddSerializer<T>` scatter; `Register` composes one `AddKeyedHybridCache(lane.Key)` per lane row whose `Store` is set, binding `DistributedCacheServiceKey` to that store key and `MaximumPayloadBytes` from the lane's `MaxPayloadBytes` so each such lane reads its own keyed L2 under its own payload guard, while a lane with no `Store` resolves the default `AddHybridCache` service — one cache owner across both paths, never a second; registration composes after the DI `TimeProvider` registration so the test row's `FakeTimeProvider` drives creation stamps and tag cuts; this port deletes hand-rolled double-checked caches, `ICacheService` wrappers, and every second cache owner in the suite.

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
    public static readonly CacheLane ModelResult = new("model-result", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None, store: "durable-l2", maxPayloadBytes: 1 << 20);
    public static readonly CacheLane Projection = new("projection", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.None, store: "durable-l2", maxPayloadBytes: 1 << 20);
    public static readonly CacheLane ArtifactBlob = new("artifact-blob", ttl: DeadlineClass.CacheTtl, l1Ttl: DeadlineClass.CacheTtl, flags: HybridCacheEntryFlags.DisableLocalCache, store: default, maxPayloadBytes: 1 << 26);

    public DeadlineClass Ttl { get; }

    public DeadlineClass L1Ttl { get; }

    public HybridCacheEntryFlags Flags { get; }

    public Option<string> Store { get; }

    public long MaxPayloadBytes { get; }

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

    public static IServiceCollection Register(IServiceCollection services, IHybridCacheSerializerFactory contributed) =>
        CacheLane.Items.Fold(services, (current, lane) =>
            lane.Store.Case is string store
                ? (current.AddKeyedHybridCache(lane.Key, options => {
                        options.DefaultEntryOptions = lane.Entry;
                        options.MaximumPayloadBytes = lane.MaxPayloadBytes;
                        options.DistributedCacheServiceKey = store;
                    }).AddSerializerFactory(contributed), current).Item2
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
- Guards: `MaximumPayloadBytes` is the lane's `MaxPayloadBytes` column — 1 MiB for `ModelResult`/`Projection` at the package default and 64 MiB for the `ArtifactBlob` lane whose blobs exceed the default — and `MaximumKeyLength` stays the 1024 default; the package clamps `LocalCacheExpiration` to `Expiration` when the L1 row exceeds the L2 row; `ReportTagMetrics` is enabled because the lane tag vocabulary is closed and low-cardinality.
- Test double: no fake cache type exists or gets hand-rolled; `SetAsync` preloads spec state through the real implementation.

## [3]-[OBJECT_POOLS]

- Owner: `PoolPolicy<T>` — one delegate-row `PooledObjectPolicy<T>` with the `Pool` accessor that mints and owns its package pool; providers and pools stay package surfaces, never wrapped.
- Entry: `T Get()` leases an instance through `Pool.Get`; `void Recycle(T pooled)` returns it through `Pool.Return`, where the package re-invokes `Return` to decide re-pooling.
- Auto: `Return` folds `IResettable.TryReset` before the row's sanity predicate, so a false return discards the instance instead of re-pooling it; `Pool` mints once through `ObjectPool.Create<T>` over the policy and caches the `ObjectPool<T>` for the row's lifetime.
- Packages: Microsoft.Extensions.ObjectPool.
- Growth: one pool policy row per pooled type; a capacity change is one policy value; zero new surface.
- Boundary: pooled instances never carry request, document, or host state across returns; `ObjectPool.Create<T>` mints the default-bounded pool, `Bounded<T>` mints through `DefaultObjectPoolProvider` whose `MaximumRetained` overrides the twice-processor-count default, and the text pool rides the package's own `StringBuilderPooledObjectPolicy` with its `InitialCapacity` and `MaximumRetainedCapacity` knobs — the hand-rolled clear-on-return reset is the deleted form because the package policy owns the reset; `LeakTrackingObjectPoolProvider` wraps the provider on the test-host row only; this cluster deletes ad hoc static pools, per-site `StringBuilder` churn, and any wrapper re-deriving the package's `IResettable` contract.

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
    public static readonly ObjectPool<StringBuilder> Text =
        ObjectPool.Create(new StringBuilderPooledObjectPolicy { InitialCapacity = 100, MaximumRetainedCapacity = 4096 });

    public static ObjectPool<T> Bounded<T>(int maximumRetained) where T : class, new() =>
        new DefaultObjectPoolProvider { MaximumRetained = maximumRetained }.Create(new DefaultPooledObjectPolicy<T>());

    public static ObjectPool<T> Default<T>() where T : class, new() =>
        ObjectPool.Create<T>();
}
```

## [4]-[DRAIN_QUEUES]

- Owner: `DrainSpec` frozen rows carrying the `DrainKind` `[SmartEnum<string>]` topology discriminant, materialized through the `DrainQueue<T>` union; `DrainSurface` carries options projection, open, drain, and the fan-out/join/coalesce block builders as one extension surface.
- Cases: `Pipe(DrainSpec Spec, Channel<T> Channel)` for simple producer-consumer seams; `Network(DrainSpec Spec, ITargetBlock<T> Intake, IDataflowBlock Tail)` for every completion-propagating block graph — single-stage batch, `BroadcastBlock` fan-out, `JoinBlock` correlated-join, and `BatchedJoinBlock` dual-stream coalesce all land as `Network` whose `Row.Kind` names the topology.
- Entry: `Task Drained(CancellationToken token)`.
- Receipt: `DropOldest` rows surface every lost item through the open-time `onDrop` delegate; a faulted `Completion` projects typed evidence onto the lifecycle fault rail; the `Network` tail's `Completion` carries the join-failure and coalesce-flush evidence.
- Packages: BCL inbox; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one `DrainSpec` row per queue carrying its `DrainKind`; a fan-out clone, a correlated-join arity, or a dual-stream coalesce batch is one row column, never a new owner; `Greedy`, `MaxGroups`, and `PropagateCompletion` are policy columns on the row; zero new surface.
- Boundary: `System.Threading.Tasks.Dataflow` rides the shared framework and its central pin stays a transitive floor, never a direct project asset; `DrainQueue` names process-level drainable queues while `WorkLane` stays the Compute solve-path name; `BroadcastBlock` fans the receipt stream to multiple sinks, `JoinBlock` correlates the watchdog heartbeat against the health snapshot, and `BatchedJoinBlock` coalesces the support artifact stream against the error stream — each is a `DrainSurface` builder over the same union, never a hand-rolled fan-out loop, correlation buffer, or dual-queue zip; completion awaits land at the row's `DrainBand` under the conductor's cancellation scope — this family deletes per-lane queue classes and free-floating background loops.

```csharp signature
[SmartEnum<string>]
public sealed partial class DrainKind {
    public static readonly DrainKind Pipe = new("pipe");
    public static readonly DrainKind Network = new("network");
    public static readonly DrainKind FanOut = new("fan-out");
    public static readonly DrainKind CorrelatedJoin = new("correlated-join");
    public static readonly DrainKind DualCoalesce = new("dual-coalesce");
}

public sealed record DrainSpec(
    string Name,
    DrainKind Kind,
    int Capacity,
    int MaxDegree,
    bool Ordered,
    BoundedChannelFullMode FullMode,
    DrainBand Band,
    DeadlineClass Deadline,
    Option<int> Batch = default,
    Option<TaskScheduler> Scheduler = default,
    bool Greedy = true,
    long MaxGroups = -1,
    bool PropagateCompletion = true) {
    public static readonly DrainSpec ReceiptFanIn = new(nameof(ReceiptFanIn), DrainKind.Pipe, Capacity: 1024, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative, Batch: 64);

    public static readonly DrainSpec SupportCapture = new(nameof(SupportCapture), DrainKind.Pipe, Capacity: 512, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.DropOldest, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative);

    public static readonly DrainSpec ReceiptFanOut = new(nameof(ReceiptFanOut), DrainKind.FanOut, Capacity: 1024, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative);

    public static readonly DrainSpec WatchdogJoin = new(nameof(WatchdogJoin), DrainKind.CorrelatedJoin, Capacity: 256, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative, Greedy: false, MaxGroups: -1);

    public static readonly DrainSpec SupportCoalesce = new(nameof(SupportCoalesce), DrainKind.DualCoalesce, Capacity: 512, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Telemetry, Deadline: DeadlineClass.DrainCooperative, Batch: 32, Greedy: true, MaxGroups: -1);
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

        public DataflowBlockOptions BroadcastOptions(CancellationToken token) => new() {
            BoundedCapacity = spec.Capacity,
            EnsureOrdered = spec.Ordered,
            CancellationToken = token,
            TaskScheduler = spec.Scheduler.IfNone(TaskScheduler.Default),
        };

        public GroupingDataflowBlockOptions GroupingOptions(CancellationToken token) => new() {
            BoundedCapacity = spec.Capacity,
            EnsureOrdered = spec.Ordered,
            CancellationToken = token,
            TaskScheduler = spec.Scheduler.IfNone(TaskScheduler.Default),
            Greedy = spec.Greedy,
            MaxNumberOfGroups = spec.MaxGroups,
        };

        public DataflowLinkOptions LinkOptions() => new() {
            PropagateCompletion = spec.PropagateCompletion,
        };

        public Fin<DrainQueue<T>> Open<T>(Option<Action<T>> onDrop = default) =>
            spec.FullMode is BoundedChannelFullMode.Wait || onDrop.IsSome
                ? Fin.Succ<DrainQueue<T>>(new DrainQueue<T>.Pipe(spec, onDrop is { IsSome: true, Case: Action<T> drop }
                    ? Channel.CreateBounded<T>(spec.PipeOptions(), drop)
                    : Channel.CreateBounded<T>(spec.PipeOptions())))
                : Fin.Fail<DrainQueue<T>>(Error.New($"unreceipted-loss:{spec.Name}"));

        public DrainQueue<T> Open<T>(ITargetBlock<T> intake, IDataflowBlock tail) =>
            new DrainQueue<T>.Network(spec, intake, tail);

        public DrainQueue<T> Broadcast<T>(Func<T, T> clone, Seq<ITargetBlock<T>> sinks, CancellationToken token) =>
            new BroadcastBlock<T>(clone, spec.BroadcastOptions(token)) is var head
                ? new DrainQueue<T>.Network(spec, head, sinks.Fold(head as IDataflowBlock, (_, sink) =>
                    (head.LinkTo(sink, spec.LinkOptions()), head).Item2))
                : throw new UnreachableException();

        public DrainQueue<Tuple<T1, T2>> Join<T1, T2>(ITargetBlock<Tuple<T1, T2>> sink, CancellationToken token) =>
            new JoinBlock<T1, T2>(spec.GroupingOptions(token)) is var join
                ? (join.LinkTo(sink, spec.LinkOptions()), new DrainQueue<Tuple<T1, T2>>.Network(spec, DataflowBlock.NullTarget<Tuple<T1, T2>>(), join)).Item2
                : throw new UnreachableException();

        public DrainQueue<Tuple<IList<T1>, IList<T2>>> Coalesce<T1, T2>(ITargetBlock<Tuple<IList<T1>, IList<T2>>> sink, CancellationToken token) =>
            new BatchedJoinBlock<T1, T2>(spec.Batch.IfNone(spec.Capacity), spec.GroupingOptions(token)) is var coalesce
                ? (coalesce.LinkTo(sink, spec.LinkOptions()), new DrainQueue<Tuple<IList<T1>, IList<T2>>>.Network(spec, DataflowBlock.NullTarget<Tuple<IList<T1>, IList<T2>>>(), coalesce)).Item2
                : throw new UnreachableException();
    }

    extension<T1, T2>(DrainQueue<Tuple<T1, T2>> queue) {
        public (ITargetBlock<T1> First, ITargetBlock<T2> Second) Arms => queue.Switch(
            pipe: static _ => throw new UnreachableException(),
            network: static n => (JoinBlock<T1, T2>)n.Tail is var join ? (join.Target1, join.Target2) : throw new UnreachableException());
    }

    extension<T1, T2>(DrainQueue<Tuple<IList<T1>, IList<T2>>> queue) {
        public (ITargetBlock<T1> First, ITargetBlock<T2> Second) CoalesceArms => queue.Switch(
            pipe: static _ => throw new UnreachableException(),
            network: static n => (BatchedJoinBlock<T1, T2>)n.Tail is var coalesce ? (coalesce.Target1, coalesce.Target2) : throw new UnreachableException());
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

- Kind split: `DrainKind` fixes the topology on the row — `Pipe` rides `Channel.CreateBounded`; `Network`, `FanOut`, `CorrelatedJoin`, and `DualCoalesce` ride Dataflow blocks; the row's `Kind` selects the `DrainSurface` builder, never the call site.
- Backpressure: `WriteAsync` and `SendAsync` await fullness on `Wait` rows; `TryWrite` and `Post` are legal only on receipted-loss rows; `NullTarget` absorption is spelled at the link site and stands in as the `Network` intake for join and coalesce rows whose live intake is the two arms.
- Loss: a `DropOldest` row opens only with an `onDrop` receipt delegate; `Open` rejects an unreceipted-loss row on the `Fin` rail; fan-out, join, and coalesce rows are `Wait` rows because their completion-propagating tails carry no silent loss.
- Grouping: `BatchBlock` carries receipt-grade batched hand-off, with `TriggerBatch` flushing a partial batch at drain; `GroupingDataflowBlockOptions` projects `Greedy` and `MaxNumberOfGroups` from the row's `Greedy`/`MaxGroups` columns while `BoundedCapacity` rides the base `DataflowBlockOptions` from `Capacity` — the reservation rail, `Encapsulate`, `AsObservable`, and `AsObserver` stay out.
- Fan-out: `Broadcast` mints one `BroadcastBlock<T>(clone)` whose `BroadcastOptions` projection rides the base `DataflowBlockOptions` (`BoundedCapacity` from `Capacity`, no `MaxDegreeOfParallelism`), links the head to every sink under `LinkOptions` carrying the row's `PropagateCompletion`, and exposes the head as both intake and `Tail` so completing the head fans completion to all sinks; the clone delegate is the receipt-fan-out copy guard, never a shared-reference leak across sinks.
- Correlated join: `Join<T1, T2>` mints one non-greedy `JoinBlock<T1, T2>` (`Greedy: false` so the watchdog heartbeat and the health snapshot pair atomically rather than buffering one stream unbounded), links it to the sink under `PropagateCompletion`, exposes `Target1`/`Target2` through `Arms`, and emits `Tuple<T1, T2>`; the producer completes both arms and `Drained` awaits the join `Tail.Completion`, so an unmatched residual on one arm at drain fails `Completion` and folds onto the lifecycle fault rail.
- Dual coalesce: `Coalesce<T1, T2>` mints one greedy `BatchedJoinBlock<T1, T2>(batchSize)` reading `batchSize` from the row's `Batch` column, links it to the sink under `PropagateCompletion`, exposes `Target1`/`Target2` through `CoalesceArms`, and emits `Tuple<IList<T1>, IList<T2>>` so the support artifact stream and error stream coalesce into one batched hand-off; a partial pair flushes when either arm reaches `batchSize` or both arms complete at drain.
- Drain and fault: `Drained` completes intake then awaits `Completion` under the conductor token at the row's band; for join and coalesce the `NullTarget` intake `Complete()` is inert and the producer-completed arms drive the tail; evidence rows complete inside the final band before exporter flush; a faulted block or channel fails `Completion`, and the conductor folds the failure into the unload receipt instead of swallowing it.
- Telemetry: the fan-out, join, and coalesce rows export no new instrument by default — depth observability is a `rasm.apphost.drain.queue.depth` gauge raised only when a consumer reads it, a forward row, never a speculative instrument; queue lane counts leave as telemetry consequence of the registered `DrainSpec` set.
