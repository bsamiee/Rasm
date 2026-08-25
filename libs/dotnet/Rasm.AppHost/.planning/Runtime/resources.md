# [APPHOST_RESOURCE_LANES]

Bounded runtime resource lanes for the Rasm.AppHost spine: the HybridCache read-through port with per-lane keyed L2 topology and lane-keyed tag invalidation, delegate-row object pools that rent and recycle, drainable queue rows that complete under the lifecycle conductor, and the TTL-bounded seen-key window every at-least-once consumer folds its duplicates through. The page owns the CacheLane axis, the PoolPolicy row shape with its concrete pool rows, the DrainSpec/DrainQueue family, and the DedupeWindow primitive; DeadlineClass and DrainBand bind lifetimes and rank as settled vocabulary, each lane's keyed L2 store and the serializer factory arrive as the single Persistence contribution, and lane counts leave as telemetry consequence.

## [01]-[INDEX]

- [02]-[CACHE_PORT]: One read-through entry; lane rows bind tags, lifetimes, options, and keyed L2.
- [03]-[OBJECT_POOLS]: Delegate-row pool policy, concrete text pool, and rent/reset/leak tracking.
- [04]-[DRAIN_QUEUES]: `DrainSpec` frozen rows, `DrainKind` topology, and fan-out/join/coalesce blocks.
- [05]-[DEDUPE_WINDOW]: The one TTL-bounded, capacity-bounded seen-key window every dedupe consumer composes.
- [06]-[AMBIENT_SLOT]: The one LIFO-scoped ambient carrier with a declared nesting bound and a typed refusal past it.

## [02]-[CACHE_PORT]

- Owner: `CacheLane` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor; `CacheRuntime` the composed posture carrying the graph and the topology both resolution and entry policy read; `CacheSurface` attaches the dispatch to that posture as one extension block.
- Cases: `ModelResult`, `Projection`, `ArtifactBlob`.
- Law: `CacheLane.Store` names the distributed-cache service key its `AddKeyedHybridCache(lane.Key)` registration binds through `DistributedCacheServiceKey`, so `ModelResult` and `Projection` share the `durable-l2` store while `ArtifactBlob` carries no `Store` and resolves the default cache; `Cache(lane)` resolves the keyed `HybridCache` by lane key for a stored lane and the default service otherwise — one cache contract, distinct L2 backings, never a second cache owner; that one column is the only growth axis for L2 topology.
- Law: a consumer holds one `CacheRuntime` and spells the lane ONCE per operation — `runtime.Read(lane, key, …)` resolves the lane's cache and frames its entry policy inside the call, so a stored lane's read hits its keyed L2 and never the default cache; `Read`, `Write`, `Invalidate`, and `Remove` all resolve through that one seat — `Write` is the store-outside-read-through entry taking owner keys exactly as `Read` does, so no direct store hand-frames a tag — and a receiver resolved at one seat then re-spelled with the lane at the next is the drift shape this posture forecloses.
- Law: the capsule gate is ONE decision read at two seats — an `InHost` topology opens no keyed builder and frames every entry through `CacheLane.Capsuled`, so a plugin-ALC process writes no L2 row a collectible load context would have to decode after unload; `Cache` therefore resolves the default service for every lane under that topology and a keyed lookup against a builder the capsule never opened is unreachable rather than guarded.
- Law: `RemoveByTagAsync` records a timestamp cut; pre-cut entries read as misses in both tiers and persist until natural expiry — logical, never physical; `RemoveAsync` is the physical sibling deleting the key from both tiers.
- Law: tags MINT at `CacheLane.Tag`, never at a call site — a read names owner keys and the lane frames each into its own tag space, so a free-string tag has no spelling and no caller reaches another lane's tags; every write also carries the bare lane key, so ONE `Invalidate(lane, owners)` entry cuts the whole lane through `RemoveByTagAsync(lane.Key)` on an empty owner set and exactly those owners otherwise — a lane-scoped cut is the widest invalidation the closed tag vocabulary admits, and a global reset rides provider disposal at host unload, never a write-time pattern tag.
- Law: peer-process L1 staleness is TTL-bounded with no backplane; convergence rides natural expiry or the next tag cut.
- Law: the cache implementation service-locates the DI `TimeProvider` with system fallback, so creation stamps and tag cuts ride the injected clock; absolute L1 expiry is delegated to the memory-cache entry's `AbsoluteExpirationRelativeToNow` under the memory cache's own clock — read-time revalidation checks only tag cuts against the injected clock, so advancing `FakeTimeProvider` never expires an L1 entry by TTL and specs assert via tag cut or `RemoveAsync`.
- Law: `MaximumPayloadBytes` is the lane's `MaxPayloadBytes` column — 1 MiB for `ModelResult`/`Projection` at the package default and 64 MiB for the `ArtifactBlob` lane whose blobs exceed the default — and every lane's column reaches a registration: a `Store`-bearing lane through its own keyed builder, and the `Store`-less set through the ONE default `AddHybridCache` whose ceiling is the widest such lane's, because that default cache is one service every storeless lane resolves; `MaximumKeyLength` stays the 1024 default; the package clamps `LocalCacheExpiration` to `Expiration` when the L1 row exceeds the L2 row; `ReportTagMetrics` is enabled because the lane tag vocabulary is closed and low-cardinality.
- Law: the package ceiling is a SILENT guard — an over-quota payload logs and returns uncached — so a lane refuses over its own ceiling on the rail first, at the one write shape whose size is known before the serializer runs; a `T`-shaped payload's ceiling proves at the contributed serializer, where the encoded bytes exist.
- Law: no fake cache type exists or gets hand-rolled; `SetAsync` preloads spec state through the real implementation.
- Entry: `ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Seq<string> owners = default, CancellationToken token = default)`; `IO<Unit> Write(CacheLane lane, string key, ReadOnlyMemory<byte> payload, Seq<string> owners = default)` — the byte-shaped write, admitted against the lane's ceiling before the store runs.
- Auto: `GetOrCreateAsync` owns stampede single-flight; local and distributed hit, miss, and write counts, stampede joins, and tag invalidations ride the package `Microsoft-Extensions-HybridCache` event source as polling counters with zero call-site metric code; `CacheRuntime.Cache` resolves the lane's keyed `HybridCache` by the lane key so each lane reads its own L2 topology, and `CacheRuntime.Entry` frames the matching entry policy off the same posture so resolution and flags can never disagree about which tier a lane is allowed to reach.
- Packages: Microsoft.Extensions.Caching.Hybrid; NodaTime; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one lane row on `CacheLane`; a lifetime or flag change is one policy value; a new L2 topology is one `Store` value on the lane row; a payload-guard retune is one `MaxPayloadBytes` value; a new deployment posture that must narrow the tiers is one arm on `CacheRuntime`, never a per-call flag; zero new surface.
- Boundary: the L2 `IDistributedCache` registered under the lane's `Store` key and the `IHybridCacheSerializerFactory` arrive as the single Persistence contribution — `Register` admits that one factory through `AddSerializerFactory` on every builder it opens, keyed and default alike, never a per-type `AddSerializer<T>` scatter; `Register` composes one `AddKeyedHybridCache(lane.Key)` per lane row whose `Store` is set, binding `DistributedCacheServiceKey` to that store key and `MaximumPayloadBytes` from the lane's own column, and ONE `AddHybridCache` for the whole `Store`-less set under the widest ceiling in it — a fold that registered only the keyed half left every storeless lane resolving an unregistered service under the package's own 1 MiB default, so a 64 MiB artifact declared a guard nothing bound and every over-size blob missed uncached with nothing raised, which is the deleted form; one cache owner across both paths, never a second; the `InHost` capsule takes the default path for EVERY lane and binds no `DistributedCacheServiceKey` at all, so the plugin-ALC gate is a registration fact rather than a runtime branch, and `CacheLane.Capsuled` is its per-entry half — a cache row surviving a collectible load context is the defect both halves of that one decision close; registration composes after the DI `TimeProvider` registration so the test row's `FakeTimeProvider` drives creation stamps and tag cuts; the ceiling refusal is the lane's own and rides the byte-shaped write, so an over-size artifact is a typed fact and never a key that reads cold forever; this port deletes hand-rolled double-checked caches, `ICacheService` wrappers, and every second cache owner in the suite.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

    public HybridCacheEntryOptions Capsuled => new() {
        Expiration = Ttl.Allotted.ToTimeSpan(),
        LocalCacheExpiration = L1Ttl.Allotted.ToTimeSpan(),
        Flags = Flags | HybridCacheEntryFlags.DisableDistributedCache,
    };

    public string Scoped(string key) => $"{Key}:{TenantContext.Current.Entry}:{key}";

    public string Tag(string owner) => $"{Key}/{owner}";
}
```

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CacheRuntime(IServiceProvider Services, DeploymentTopology Topology) {
    public bool Capsule => Topology == DeploymentTopology.InHost;

    public HybridCache Cache(CacheLane lane) =>
        lane.Store.IsSome && !Capsule
            ? Services.GetRequiredKeyedService<HybridCache>(lane.Key)
            : Services.GetRequiredService<HybridCache>();

    public HybridCacheEntryOptions Entry(CacheLane lane) => Capsule ? lane.Capsuled : lane.Entry;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CacheSurface {
    extension(CacheRuntime runtime) {
        public ValueTask<T> Read<T, TState>(CacheLane lane, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, Seq<string> owners = default, Option<HybridCacheEntryOptions> entry = default, CancellationToken token = default) =>
            runtime.Cache(lane).GetOrCreateAsync(lane.Scoped(key), state, factory, entry.IfNone(runtime.Entry(lane)), owners.Map(lane.Tag).Add(lane.Key), token);

        public ValueTask Write<T>(CacheLane lane, string key, T value, Seq<string> owners = default, Option<HybridCacheEntryOptions> entry = default, CancellationToken token = default) =>
            runtime.Cache(lane).SetAsync(lane.Scoped(key), value, entry.IfNone(runtime.Entry(lane)), owners.Map(lane.Tag).Add(lane.Key), token);

        public IO<Unit> Write(CacheLane lane, string key, ReadOnlyMemory<byte> payload, Seq<string> owners = default) =>
            payload.Length <= lane.MaxPayloadBytes
                ? IO.liftAsync(async env => {
                    await runtime.Cache(lane).SetAsync(lane.Scoped(key), payload, runtime.Entry(lane), owners.Map(lane.Tag).Add(lane.Key), env.Token);
                    return unit;
                })
                : IO.fail<Unit>(new KernelFault.InvalidValue(
                    Label: $"{lane.Key}:{payload.Length}",
                    Requirement: $"a payload at or under the lane's {lane.MaxPayloadBytes} bytes"));

        public ValueTask Invalidate(CacheLane lane, Seq<string> owners = default, CancellationToken token = default) =>
            owners.IsEmpty
                ? runtime.Cache(lane).RemoveByTagAsync(lane.Key, token)
                : runtime.Cache(lane).RemoveByTagAsync(owners.Map(lane.Tag), token);

        public ValueTask Remove(CacheLane lane, string key, CancellationToken token = default) =>
            runtime.Cache(lane).RemoveAsync(lane.Scoped(key), token);
    }

    public static IServiceCollection Register(IServiceCollection services, IHybridCacheSerializerFactory contributed, DeploymentTopology topology) =>
        Defaulted(
            topology == DeploymentTopology.InHost
                ? services
                : toSeq(CacheLane.Items)
                    .Filter(static lane => lane.Store.IsSome)
                    .Fold(services, (current, lane) => Keyed(current, lane, contributed)),
            topology == DeploymentTopology.InHost
                ? toSeq(CacheLane.Items)
                : toSeq(CacheLane.Items).Filter(static lane => lane.Store.IsNone),
            contributed);

    static IServiceCollection Keyed(IServiceCollection services, CacheLane lane, IHybridCacheSerializerFactory contributed) =>
        (services.AddKeyedHybridCache(lane.Key, options => {
            options.DefaultEntryOptions = lane.Entry;
            options.MaximumPayloadBytes = lane.MaxPayloadBytes;
            options.DistributedCacheServiceKey = lane.Store.IfNone(string.Empty);
        }).AddSerializerFactory(contributed), services).Item2;

    static IServiceCollection Defaulted(IServiceCollection services, Seq<CacheLane> local, IHybridCacheSerializerFactory contributed) =>
        local.IsEmpty
            ? services
            : (services.AddHybridCache(options =>
                options.MaximumPayloadBytes = local.Fold(0L, static (widest, lane) => long.Max(widest, lane.MaxPayloadBytes)))
                .AddSerializerFactory(contributed), services).Item2;
}
```

## [03]-[OBJECT_POOLS]

- Owner: `PoolPolicy<T>` — one delegate-row `PooledObjectPolicy<T>` with the `Pool` accessor that mints and owns its package pool; providers and pools stay package surfaces, never wrapped.
- Entry: `T Get()` leases an instance through `Pool.Get`; `void Recycle(T pooled)` returns it through `Pool.Return`, where the package re-invokes `Return` to decide re-pooling.
- Auto: `Return` folds `IResettable.TryReset` before the row's sanity predicate, so a false return discards the instance instead of re-pooling it; `Pool` mints once through `ObjectPool.Create<T>` over the policy and caches the `ObjectPool<T>` for the row's lifetime.
- Packages: Microsoft.Extensions.ObjectPool.
- Growth: one pool policy row per pooled type; a capacity change is one policy value; zero new surface.
- Boundary: pooled instances never carry request, document, or host state across returns; `ObjectPool.Create<T>` mints the default-bounded pool, `Bounded<T>` mints through `DefaultObjectPoolProvider` whose `MaximumRetained` overrides the twice-processor-count default, and the text pool rides the package's own `StringBuilderPooledObjectPolicy` with its `InitialCapacity` and `MaximumRetainedCapacity` knobs — the hand-rolled clear-on-return reset is the deleted form because the package policy owns the reset; `LeakTrackingObjectPoolProvider` wraps the provider on the test-host row only; this cluster deletes ad hoc static pools, per-site `StringBuilder` churn, and any wrapper re-deriving the package's `IResettable` contract.

```csharp signature
// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PoolPolicy<T> : PooledObjectPolicy<T> where T : class {
    readonly Func<T> create;
    readonly Func<T, bool> sane;
    readonly Lazy<ObjectPool<T>> pool;

    public PoolPolicy(Func<T> create, Func<T, bool> sane) {
        (this.create, this.sane) = (create, sane);
        pool = new Lazy<ObjectPool<T>>(() => ObjectPool.Create(this), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ObjectPool<T> Pool => pool.Value;

    public T Get() => Pool.Get();

    public void Recycle(T pooled) => Pool.Return(pooled);

    public override T Create() => create();

    public override bool Return(T pooled) => (pooled is not IResettable resettable || resettable.TryReset()) && sane(pooled);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class Pools {
    public static readonly ObjectPool<StringBuilder> Text =
        ObjectPool.Create(new StringBuilderPooledObjectPolicy { InitialCapacity = 100, MaximumRetainedCapacity = 4096 });

    public static ObjectPool<T> Bounded<T>(int maximumRetained) where T : class, new() =>
        new DefaultObjectPoolProvider { MaximumRetained = maximumRetained }.Create(new DefaultPooledObjectPolicy<T>());

    public static ObjectPool<T> Default<T>() where T : class, new() =>
        ObjectPool.Create<T>();
}
```

## [04]-[DRAIN_QUEUES]

- Owner: `DrainSpec` frozen rows carrying the `DrainKind` `[SmartEnum<string>]` topology discriminant, materialized through the `DrainQueue<T>` union; `DrainFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Drain`); `DrainSurface` carries options projection, open, drain, and the fan-out/join/coalesce block builders as one extension surface.
- Cases: `Pipe(DrainSpec Spec, Channel<T> Channel)` for simple producer-consumer seams; `Network(DrainSpec Spec, ITargetBlock<T> Intake, IDataflowBlock Tail)` for every completion-propagating block graph — the `ActionBlock` consumer sink, the single-stage batch, the `BroadcastBlock` fan-out, the `JoinBlock` correlated-join, and the `BatchedJoinBlock` dual-stream coalesce all land as `Network` whose `Row.Kind` names the topology; `DrainFault` = UnreceiptedLoss | TopologyMismatch — a lossy row opened without its `onDrop` receipt or its `onMiss` reporter and an arm projection against the wrong topology are typed rail failures, never throws.
- Law: `DrainKind` fixes the topology on the row — `Pipe` rides `Channel.CreateBounded`; `Network`, `FanOut`, `CorrelatedJoin`, and `DualCoalesce` ride Dataflow blocks; the row's `Kind` selects the `DrainSurface` builder, never the call site.
- Law: `ActionSink` is the CONSUMER end of the same union — one `ActionBlock<T>` over the row's own `NetworkOptions`, exposed as both intake and tail so the sink's back-pressure, its degree of parallelism, its ordering, and its completion are all the row's declared policy; a consumer that mints its own `ActionBlock` holds a raw package handle whose bound no row states, which is exactly how a public block field escaped onto a subscription record. The sink takes no loss reporter: an `ActionBlock` DECLINES an offer its bounded capacity cannot take and the fan that offered it accounts that decline by conservation at its two ends, so a reporter here would be a second account of one loss.
- Law: `WriteAsync` and `SendAsync` await fullness on `Wait` rows; `TryWrite` and `Post` are legal only on receipted-loss rows; `NullTarget` absorption is spelled at the link site and stands in as the `Network` intake for join and coalesce rows whose live intake is the two arms.
- Law: a `DropOldest` row opens only with its `onDrop` receipt delegate and a fan-out row only with its `onMiss` reporter, and both refuse an unreceipted-loss row on the `Fin` rail; a fan-out row's per-target loss is latest-value overwrite the source cannot observe, so nothing wires that reporter into the block and the row's owner invokes it at the two conservation ends — a gap in the dense sequence one sink received, and the residue between the head's final ordinal and that sink's last seat; join and coalesce rows are `Wait` rows whose completion-propagating tails carry no silent loss.
- Law: `BatchBlock` carries receipt-grade batched hand-off, with `TriggerBatch` flushing a partial batch at drain; `GroupingDataflowBlockOptions` projects `Greedy` and `MaxNumberOfGroups` from the row's `Greedy`/`MaxGroups` columns while `BoundedCapacity` rides the base `DataflowBlockOptions` from `Capacity` — the reservation rail, `Encapsulate`, `AsObservable`, and `AsObserver` stay out.
- Law: `Broadcast` mints one `BroadcastBlock<T>(clone)` whose `BroadcastOptions` projection rides the base `DataflowBlockOptions` (`BoundedCapacity` from `Capacity`, no `MaxDegreeOfParallelism`), links the head to every sink under `LinkOptions` carrying the row's `PropagateCompletion`, and exposes the head as both intake and `Tail` so completing the head fans completion to all sinks; the clone delegate is the receipt-fan-out copy guard, never a shared-reference leak across sinks; `Broadcast` returns `Fin<DrainQueue<T>>` and admits on the `onMiss` reporter, whose `FanMiss` names the row, the sink, and the inclusive span of deliveries lost to it — one payload-agnostic shape each fan-out owner projects into its own receipt vocabulary.
- Law: `Join<T1, T2>` mints one non-greedy `JoinBlock<T1, T2>` (`Greedy: false` so the watchdog heartbeat and the health snapshot pair atomically rather than buffering one stream unbounded), links it to the sink under `PropagateCompletion`, exposes `Target1`/`Target2` through `Arms`, and emits `Tuple<T1, T2>`; the producer completes both arms and `Drained` awaits the join `Tail.Completion`, so an unmatched residual on one arm at drain fails `Completion` and folds onto the lifecycle fault rail.
- Law: `Coalesce<T1, T2>` mints one greedy `BatchedJoinBlock<T1, T2>(batchSize)` reading `batchSize` from the row's `Batch` column, links it to the sink under `PropagateCompletion`, exposes `Target1`/`Target2` through `CoalesceArms`, and emits `Tuple<IList<T1>, IList<T2>>` so the support artifact stream and error stream coalesce into one batched hand-off; a partial pair flushes when either arm reaches `batchSize` or both arms complete at drain.
- Law: `Drained` completes intake then awaits `Completion` under the conductor token at the row's band; for join and coalesce the `NullTarget` intake `Complete()` is inert and the producer-completed arms drive the tail; evidence rows complete inside the final band before exporter flush; a faulted block or channel fails `Completion`, and the conductor folds the failure into the unload receipt instead of swallowing it.
- Law: the fan-out, join, and coalesce rows export no new instrument by default — depth observability is a `rasm.apphost.drain.queue.depth` gauge raised only when a consumer reads it, a forward row, never a speculative instrument; queue lane counts leave as telemetry consequence of the registered `DrainSpec` set.
- Entry: `Task Drained(CancellationToken token)`; `ActionSink<T>(Func<T, ValueTask> consume, CancellationToken token)` returns `DrainQueue<T>` — the consumer-sink builder every at-least-once subscriber opens instead of a raw `ActionBlock`.
- Receipt: `DropOldest` rows surface every lost item through the open-time `onDrop` delegate; fan-out rows surface loss as `FanMiss` spans their owner folds at the two conservation ends, because the broadcast head reports no decline to any source; a faulted `Completion` projects typed evidence onto the lifecycle fault rail; the `Network` tail's `Completion` carries the join-failure and coalesce-flush evidence.
- Packages: BCL inbox; Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: one `DrainSpec` row per queue carrying its `DrainKind`; a consumer sink, a fan-out clone, a correlated-join arity, or a dual-stream coalesce batch is one row column, never a new owner; `Greedy`, `MaxGroups`, and `PropagateCompletion` are policy columns on the row; a fan-out row's loss reporter is one open-time argument rather than a row column, because the projection belongs to the owner holding the ordinal vocabulary its deliveries are dense in; zero new surface.
- Boundary: `System.Threading.Tasks.Dataflow` resolves from the `net10.0` shared framework, so it carries no manifest row and no direct package reference, and every consumer reaches Dataflow through these builders — a manifest row minted for it is the deleted form; `DrainQueue` names process-level drainable queues while `WorkLane` stays the Compute solve-path name; the consumer sink lands here rather than at its caller because a subscription holding a public `ActionBlock<T>` field published a package handle whose capacity, degree, ordering, and completion no row stated — `Wire/topics#SUBSCRIPTION_FABRIC` opens `DrainSpec.SubscriptionSink.ActionSink(consume, token)` and holds the `DrainQueue<DomainEvent>` instead; `BroadcastBlock` fans the receipt stream to multiple sinks, `JoinBlock` correlates the watchdog heartbeat against the health snapshot, and `BatchedJoinBlock` coalesces the support artifact stream against the error stream — each is a `DrainSurface` builder over the same union, never a hand-rolled fan-out loop, correlation buffer, or dual-queue zip; a fan-out row accounts its loss by CONSERVATION at its two ends rather than by interception, so a hand-written receipting `ITargetBlock<T>` between head and sink is the deleted form — it re-implements the `consumeToAccept`, `ConsumeMessage`, and postponement protocol this owner declines to own, against an admission rail admitting blocks on four named capabilities and never on a hand-written target; every dispatch over the union is TOTAL — the arm projections return `Fin` with `DrainFault.TopologyMismatch` on the pipe arm and the builders bind their block graphs through total helpers, so a `throw new UnreachableException()` inside an expression fold is the deleted form; completion awaits land at the row's `DrainBand` under the conductor's cancellation scope — this family deletes per-lane queue classes and free-floating background loops.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DrainKind {
    public static readonly DrainKind Pipe = new("pipe");
    public static readonly DrainKind Network = new("network");
    public static readonly DrainKind FanOut = new("fan-out");
    public static readonly DrainKind CorrelatedJoin = new("correlated-join");
    public static readonly DrainKind DualCoalesce = new("dual-coalesce");
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DrainFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Drain;
    private DrainFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record UnreceiptedLoss : DrainFault { public UnreceiptedLoss(string queue) : base(queue) { } }
    [FaultCase(1)]
    public sealed partial record TopologyMismatch : DrainFault { public TopologyMismatch(string queue, string expected) : base($"{queue}!={expected}") { } }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FanMiss(string Queue, string Sink, ulong First, ulong Last);

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

    public static readonly DrainSpec WireInbound = new(nameof(WireInbound), DrainKind.Pipe, Capacity: 1024, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.DropOldest, Band: DrainBand.Interaction, Deadline: DeadlineClass.DrainCooperative);

    public static readonly DrainSpec SubscriptionSink = new(nameof(SubscriptionSink), DrainKind.Network, Capacity: 512, MaxDegree: 1, Ordered: true, FullMode: BoundedChannelFullMode.Wait, Band: DrainBand.Interaction, Deadline: DeadlineClass.DrainCooperative);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DrainQueue<T> {
    private DrainQueue() { }

    public sealed record Pipe(DrainSpec Spec, Channel<T> Channel) : DrainQueue<T>;

    public sealed record Network(DrainSpec Spec, ITargetBlock<T> Intake, IDataflowBlock Tail) : DrainQueue<T>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
                : Fin.Fail<DrainQueue<T>>(new DrainFault.UnreceiptedLoss(spec.Name));

        public DrainQueue<T> Open<T>(ITargetBlock<T> intake, IDataflowBlock tail) =>
            new DrainQueue<T>.Network(spec, intake, tail);

        public DrainQueue<T> ActionSink<T>(Func<T, ValueTask> consume, CancellationToken token) =>
            Sinked(spec, new ActionBlock<T>(consume, spec.NetworkOptions(token)));

        public Fin<DrainQueue<T>> Broadcast<T>(Func<T, T> clone, Seq<ITargetBlock<T>> sinks, Option<Action<FanMiss>> onMiss, CancellationToken token) =>
            onMiss.IsSome
                ? Fin.Succ<DrainQueue<T>>(Fanned(spec, new BroadcastBlock<T>(clone, spec.BroadcastOptions(token)), sinks))
                : Fin.Fail<DrainQueue<T>>(new DrainFault.UnreceiptedLoss(spec.Name));

        public DrainQueue<Tuple<T1, T2>> Join<T1, T2>(ITargetBlock<Tuple<T1, T2>> sink, CancellationToken token) =>
            Tailed<Tuple<T1, T2>, JoinBlock<T1, T2>>(spec, new JoinBlock<T1, T2>(spec.GroupingOptions(token)), sink);

        public DrainQueue<Tuple<IList<T1>, IList<T2>>> Coalesce<T1, T2>(ITargetBlock<Tuple<IList<T1>, IList<T2>>> sink, CancellationToken token) =>
            Tailed<Tuple<IList<T1>, IList<T2>>, BatchedJoinBlock<T1, T2>>(spec, new BatchedJoinBlock<T1, T2>(spec.Batch.IfNone(spec.Capacity), spec.GroupingOptions(token)), sink);
    }

    static DrainQueue<T> Sinked<T>(DrainSpec spec, ActionBlock<T> sink) =>
        new DrainQueue<T>.Network(spec, sink, sink);

    static DrainQueue<T> Fanned<T>(DrainSpec spec, BroadcastBlock<T> head, Seq<ITargetBlock<T>> sinks) =>
        new DrainQueue<T>.Network(spec, head, sinks.Fold((IDataflowBlock)head, (tail, sink) =>
            (head.LinkTo(sink, spec.LinkOptions()), tail).Item2));

    static DrainQueue<T> Tailed<T, TBlock>(DrainSpec spec, TBlock tail, ITargetBlock<T> sink) where TBlock : ISourceBlock<T> =>
        (tail.LinkTo(sink, spec.LinkOptions()), new DrainQueue<T>.Network(spec, DataflowBlock.NullTarget<T>(), tail)).Item2;

    extension<T1, T2>(DrainQueue<Tuple<T1, T2>> queue) {
        public Fin<(ITargetBlock<T1> First, ITargetBlock<T2> Second)> Arms => queue.Switch(
            pipe: static p => Fin.Fail<(ITargetBlock<T1>, ITargetBlock<T2>)>(new DrainFault.TopologyMismatch(p.Spec.Name, DrainKind.CorrelatedJoin.Key)),
            network: static n => n.Tail is JoinBlock<T1, T2> join
                ? Fin.Succ<(ITargetBlock<T1>, ITargetBlock<T2>)>((join.Target1, join.Target2))
                : Fin.Fail<(ITargetBlock<T1>, ITargetBlock<T2>)>(new DrainFault.TopologyMismatch(n.Spec.Name, DrainKind.CorrelatedJoin.Key)));
    }

    extension<T1, T2>(DrainQueue<Tuple<IList<T1>, IList<T2>>> queue) {
        public Fin<(ITargetBlock<T1> First, ITargetBlock<T2> Second)> CoalesceArms => queue.Switch(
            pipe: static p => Fin.Fail<(ITargetBlock<T1>, ITargetBlock<T2>)>(new DrainFault.TopologyMismatch(p.Spec.Name, DrainKind.DualCoalesce.Key)),
            network: static n => n.Tail is BatchedJoinBlock<T1, T2> coalesce
                ? Fin.Succ<(ITargetBlock<T1>, ITargetBlock<T2>)>((coalesce.Target1, coalesce.Target2))
                : Fin.Fail<(ITargetBlock<T1>, ITargetBlock<T2>)>(new DrainFault.TopologyMismatch(n.Spec.Name, DrainKind.DualCoalesce.Key)));
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

## [05]-[DEDUPE_WINDOW]

- Owner: `DedupeWindow` — the one TTL-bounded, capacity-bounded seen-key window the whole suite's at-least-once consumers admit through.
- Entry: `Of(Duration ttl, int cap)` returns `DedupeWindow` — the two bounds are the whole construction, so a window is a value a composition hands its consumer rather than a class each consumer configures; `Admit(string key, Instant now)` returns `bool` — TRUE is the first admission inside the window and a key still holding an unexpired deadline refuses.
- Auto: expiry is INTERIOR — the admission prunes every elapsed row before it decides, so no consumer sweeps, no timer runs, and a window that goes quiet holds nothing; the verdict rides the same compare-and-swap that records the key, so two threads racing one key admit exactly one of them and no read-then-write pair can interleave; the clock arrives as the caller's own `Instant`, so the window reads whatever `ClockPolicy` the composition threaded and a spec advancing a fake clock expires rows deterministically; capacity is the second bound — past `cap` the nearest-to-expiry rows leave, so a burst degrades to a shorter effective window instead of unbounded memory.
- Receipt: the window mints none — a refusal is the consumer's own duplicate fact, recorded at the consumer's receipt where the message identity lives.
- Packages: NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new dedupe consumer is one `DedupeWindow` value at its composition, never a second window type; a different retention or ceiling is one construction argument; zero new surface.
- Boundary: this is the suite's only duplicate-suppression primitive — the `Wire/outbound#DELIVERY_FANOUT` fan and the `Wire/topics#SUBSCRIPTION_FABRIC` consumer each hold one window value and neither declares a map of its own, so a per-consumer `Dictionary<string, DateTime>` with its own expiry rule and no ceiling is the deleted form that grew until the process did; the key is the consumer's own message identity and this owner never derives one, so a window never decides what "the same message" means; both bounds are mandatory because either alone fails — a TTL with no ceiling is unbounded under a burst and a ceiling with no TTL never forgets a key the wire will never resend; the window is process-local by construction and makes no cross-process claim, so a delivery deduplicated here and re-delivered to a peer is the at-least-once contract holding, never a defect of this owner — cross-process suppression is the durable store's fenced-write concern.

```csharp signature
// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DedupeWindow {
    readonly Duration ttl;
    readonly int cap;
    readonly Atom<Window> cell = Atom(Window.Empty);

    DedupeWindow(Duration ttl, int cap) => (this.ttl, this.cap) = (ttl, cap);

    public static DedupeWindow Of(Duration ttl, int cap) => new(ttl, cap);

    public bool Admit(string key, Instant now) =>
        cell.Swap(held => Advanced(held.Seen.Filter(deadline => deadline > now), key, now + ttl)).Admitted;

    Window Advanced(HashMap<string, Instant> live, string key, Instant deadline) =>
        live.ContainsKey(key)
            ? new Window(live, Admitted: false)
            : new Window(Bounded(live.Add(key, deadline)), Admitted: true);

    HashMap<string, Instant> Bounded(HashMap<string, Instant> live) =>
        live.Count <= cap
            ? live
            : toSeq(live.AsIterable().OrderBy(static row => row.Value).Take(live.Count - cap))
                .Fold(live, static (held, row) => held.Remove(row.Key));

    readonly record struct Window(HashMap<string, Instant> Seen, bool Admitted) {
        public static readonly Window Empty = new(HashMap<string, Instant>(), Admitted: false);
    }
}
```

## [06]-[AMBIENT_SLOT]

- Owner: `AmbientSlot<T>` — the one ambient carrier in the suite: an `AsyncLocal` frame chain with LIFO restore, a DECLARED nesting bound, and a typed refusal past it.
- Entry: `Of(string name, int depth)` mints a slot bounded at `depth` and `One(string name)` the one-level case; `Current` reads the innermost value as `Option<T>`; `Depth` reads the live nesting; `Enter(T value)` returns `Fin<IDisposable>` — the scope whose disposal restores the frame it displaced, refusing `KernelFault.InvalidValue` when the caller is already at the slot's bound.
- Auto: the frame chain carries its own depth so a bound is checked against live state rather than a counter a caller maintains; disposal restores the PRIOR frame rather than clearing the slot, so a nested scope leaves its parent intact and a slot whose scopes dispose in order returns to absent exactly once; a one-level slot refuses its second `Enter` instead of silently shadowing, which is the whole point of declaring the bound — a session, a principal, or a turn that nests is a defect at the seam that nested it, not a value the reader should have to disambiguate.
- Receipt: the slot mints none — an ambient value is evidence its own owner receipts; a refusal is the caller's own typed fault.
- Packages: LanguageExt.Core, BCL inbox
- Growth: a new ambient concern is one `AmbientSlot<T>` value at its owning composition, never a second `AsyncLocal` beside it; a bound change is one construction argument.
- Boundary: this is the suite's only ambient-scope primitive — the per-page `static readonly AsyncLocal<T?>` beside a hand-written `Scope : IDisposable` is the deleted form it replaces, and the three that existed disagreed on all three axes (one cleared the slot on dispose, one restored a prior, one had no restore at all). NAMED LOSS: a caller can no longer read the raw `AsyncLocal` to write it without a scope — writes are scope-shaped by construction, which is what makes the restore total; the kernel `TenantContext` slot stays the kernel's and this owner never reaches it, because tenancy admission is a boundary decision with its own adoption law rather than a nesting one. Disposal is LIFO by contract: the scope restores the frame it displaced, so an out-of-order disposal restores a frame the flow already left — a `using` scope is the only lawful spelling and a stored scope disposed later is the deleted form. The slot is process-flow local and makes no cross-thread claim: work handed to a pooled thread carries a CAPTURED frame value (the `Observability/telemetry#SIGNAL_GOVERNANCE` correlation capture is that capture) rather than reading a slot the pool never entered.

```csharp signature
// --- [SERVICES] ------------------------------------------------------------------------
public sealed class AmbientSlot<T> where T : class {
    readonly AsyncLocal<Frame?> cell = new();
    readonly string name;
    readonly int bound;

    AmbientSlot(string name, int bound) => (this.name, this.bound) = (name, bound);

    public static AmbientSlot<T> Of(string name, int depth) => new(name, int.Max(depth, 1));

    public static AmbientSlot<T> One(string name) => Of(name, depth: 1);

    public Option<T> Current => Optional(cell.Value).Map(static frame => frame.Value);

    public int Depth => cell.Value?.Depth ?? 0;

    public Fin<IDisposable> Enter(T value) =>
        Depth < bound
            ? Fin.Succ(Held(value))
            : Fin.Fail<IDisposable>(new KernelFault.InvalidValue(
                Label: name, Requirement: $"at most {bound} nested scope(s); held {Depth}"));

    IDisposable Held(T value) {
        Frame? prior = cell.Value;
        cell.Value = new Frame(value, (prior?.Depth ?? 0) + 1, prior);
        return new Scope(this, prior);
    }

    sealed record Frame(T Value, int Depth, Frame? Prior);

    sealed class Scope(AmbientSlot<T> slot, Frame? prior) : IDisposable {
        public void Dispose() => slot.cell.Value = prior;
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
