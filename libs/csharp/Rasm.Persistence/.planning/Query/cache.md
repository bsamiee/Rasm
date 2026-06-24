# [PERSISTENCE_CACHE_INDEXES]

Rasm.Persistence contributes exactly one registration row to the suite cache port and owns three durable index contracts over it. `CacheContribution` is the sole cache capsule: it is the L2 `IBufferDistributedCache` over the key-value lane and the `IHybridCacheSerializerFactory` riding the MessagePackBinary codec row, and it selects its L2 residence over the closed `CacheResidence` tier axis whose redis arm carries the optional `RedisFabric` live-coordination delegate binding the at-least-once `XREADGROUP` invalidation cursor of record plus the best-effort RESP3/keyevent push hints that race it. The three indexes — `ModelResultKey` (model-result and projection reuse), `ArtifactIndexRow` (content-addressed blob catalog with the chunk-dedup pre-filter), and `BenchmarkRow` (fingerprint-gated route evidence) — all key on one content-address identity tenant-partitioned at the cache-key seam, carry one `ArtifactClass` retention reference, and emit one `CacheIndexFact` evidence stream whose `CacheKind` discriminant is the closed `[SmartEnum<string>]` vocabulary every fact dispatches over. Cache mechanics stay at the AppHost port where `CacheLane` and `CacheSurface` own stampede, tags, and entry options; the owned surfaces here are key-shape value objects, catalog rows, the closed `CacheKind`/`ArtifactKind` evidence-and-family vocabularies, the residence registration axis, the closed `CacheFault` family, and the one fact stream over `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `StackExchange.Redis`, `MessagePack`, `System.IO.Hashing`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, and `NodaTime`, stamped through `ClockPolicy`, partitioned by `TenantId`, and emitted through `ReceiptSinkPort`.

## [01]-[INDEX]

- [01]-[L2_CONTRIBUTION]: one registration row for the L2 store, the residence tier axis, the live-fabric delegate (the at-least-once stream invalidation cursor plus the best-effort push hints), the serializer factory, and the closed cache-fault family.
- [02]-[MODEL_RESULT_INDEX]: the `ModelResultKey` cross-process result identity covering model-result and projection-lane reuse, the tenant-partitioned read-through entry that mints the `Hit`/`Miss` evidence the fact stream asserts, fabric-driven invalidation, and the `CacheKind`-keyed fact stream.
- [03]-[ARTIFACT_BLOB_INDEX]: content-addressed catalog rows for warm-start and profiling artifacts carrying plain and stored byte evidence under a `CompressionPosture`, the cross-projection `SourceKey` join, and the chunk-dedup `ShortTag` membership pre-filter.
- [04]-[BENCHMARK_INDEX]: persisted BenchmarkDotNet rows with latency, GC, and throughput profiles; the fingerprint-gated recency claim and the cost-ranked `Select` that picks the winning route by tail latency, GC pressure, and throughput.

## [02]-[L2_CONTRIBUTION]

The contribution is one storage row and one codec, never a second cache owner. AppHost owns the port, the stampede policy, the tag vocabulary, and the entry options at `Runtime/resources#CACHE_PORT`; this capsule reads and writes the bytes and the residence axis selects which `IDistributedCache` it binds. The redis arm refines L1 coherence from TTL-bounded to invalidation-driven where a multiplexer is present — the at-least-once `XREADGROUP` consumer-group cursor is the coherence guarantee that survives a disconnect, the RESP3/keyevent push only races it to a faster hint — and is bit-identical to the TTL baseline where it is absent; the live fabric is a `Register` delegate row carrying behavior, never a flag-branch in the capsule body.

- Owner: `CacheContribution` — one sealed boundary capsule implementing both `IBufferDistributedCache` and `IHybridCacheSerializerFactory`, the package's single cache registration row; `CacheResidence` — the registration-row residence axis carrying each tier's `IDistributedCache` registration and, for the redis arm, the `RedisFabric` delegate row binding the RESP3 server-assisted client-side-caching invalidation push, the best-effort keyevent-notification hint, the at-least-once `XREADGROUP` consumer-group invalidation cursor, and the atomic Lua single-flight/lease surface; `RedisFabric` — the live-coordination delegate carrier whose `Inert` value is the Redis-absent identity; `CacheFault` — the closed family deriving from `Expected, IValidationError<CacheFault>` over the live-fabric arm, drain, lease, and replay-gap causes.
- Cases: `CacheResidence` in-memory | sqlite | redis | distributed-pg; `CacheFault` FabricUnarmed | SubscriptionLost | LeaseRejected | ReplayGap | Uncategorized.
- Entry: `bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` — the BCL factory contract shape at the boundary; every other member is the L2 store contract; the constructor's `Func<CacheIndexFact, ValueTask> emit` delegate is the one storage-seam fact channel the composition root binds to the receipt sink, so the capsule stays decoupled from `ReceiptSinkPort`/correlation while still emitting the `L2Promote` transition the read-through cannot see; `static Func<IServiceCollection, IServiceCollection> Register(CacheResidence, string endpoint, string instance, Func<IServiceProvider, IDistributedCache> pgBacked, Option<RedisFabric> fabric)` selects the residence registration row, settles `CacheResidence.Active`, and the redis arm composes the optional `RedisFabric` so an absent multiplexer leaves the path inert.
- Auto: `TryCreateSerializer<T>` yields the MessagePack codec for every cache payload type with zero per-type registration, the `MessagePackSerializerOptions` value arriving settled from the MessagePackBinary codec row; the `DefaultHybridCache` runtime sniffs the buffer contract at registration and routes reads through `TryGetAsync(IBufferWriter<byte>)` into its pooled buffer writer and writes through the `ReadOnlySequence<byte>` form, so payload bytes move with zero intermediate arrays; `CacheResidence.Redis` registers `RedisCache : IBufferDistributedCache` through `AddStackExchangeRedisCache` so the zero-copy path holds across the redis tier, and where a `RedisFabric` is bound the same multiplexer arms `RedisProtocol.Resp3`, creates the `XGROUP` invalidation consumer group, subscribes the `__redis__:invalidate` broadcast push and the `__keyevent@*__:*` keyevent stream as best-effort low-latency hints that drop an L1 entry on the source key change ahead of TTL, and prepares the atomic `ScriptEvaluate` single-flight and writer-lease scripts; the durable invalidation feed is the `XREADGROUP` consumer-group cursor — `Replay` drains the `__cache:invalidate` stream from the committed group cursor with idle-claim takeover, folds each `StreamEntry` into a fact under the `CacheKind.Invalidate` row, and `Commit` `XACK`s the drained ids so a disconnected consumer replays every missed invalidation on reconnect rather than silently serving stale L1 entries, the lossy push only racing the cursor to a faster hint; `Observe` projects each pushed `ChannelMessage` into the same `CacheKind.Invalidate` fact over the queue's own `IAsyncEnumerable<ChannelMessage>` so both the hint lane and the cursor lane are observable and back-pressure-safe, never an opaque side effect or an unbounded accumulator.
- Packages: `Microsoft.Extensions.Caching.Hybrid`; `Microsoft.Extensions.Caching.StackExchangeRedis`; `StackExchange.Redis`; `MessagePack`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project); BCL inbox.
- Growth: a new L2 residence is one `CacheResidence` case carrying its `IDistributedCache` registration row; a new live-fabric coordination — a stream consumer group, a presence keyspace lane, a sharded `SPUBLISH` push — is one delegate slot on `RedisFabric`; a new fabric failure cause is one `CacheFault` case in the closed family; a new residence-aware fact is one `CacheKind` row on the closed evidence vocabulary, so every `Switch` over kinds breaks at compile time rather than admitting an unhandled string; zero new surface — a second live-coordination owner, a fabric capsule per provider, a bare `Error.New` cache fault, or a free-string evidence discriminant is the deleted form.
- Boundary: the BCL cache contracts own the member shapes and the capsule body carries language-owned statement forms; the hybrid runtime confines every L2 touch to `GetAsync`, `TryGetAsync`, `SetAsync`, and `RemoveAsync`, so the synchronous members exist for contract totality and bridge by blocking; the `read` delegate serves the array contract and `readInto` the buffer contract — one storage row behind both; absolute expiry is the only L2 lifetime and `Refresh` is structurally inert; `CacheResidence` is a registration-row axis selecting which `IDistributedCache` the one capsule reads and writes — `InMemory` registers the BCL memory distributed cache, `Sqlite` the embedded key-value L2 (the default), `Redis` registers `StackExchange.Redis` `ConnectionMultiplexer.Connect`/`GetDatabase` behind `RedisCache` through `AddStackExchangeRedisCache` with `RedisCacheOptions.{ConfigurationOptions, InstanceName, ConnectionMultiplexerFactory, ProfilingSession}`, and `DistributedPg` the pg-backed row; each tier keys on the one content-address identity so an L1 miss promotes from the L2 tier without a re-mint; tag invalidation rides the existing op-log changefeed cursor as the baseline, and where the live fabric is present the at-least-once `XREADGROUP` consumer-group cursor over the `__cache:invalidate` stream is the redis backplane of record — a committed group cursor replays every missed invalidation on reconnect — while the RESP3 `__redis__:invalidate` push and the `__keyevent@*__:*` keyevent stream are best-effort low-latency hints that race the cursor to drop an L1 entry ahead of its TTL, never the coherence guarantee themselves (`.api/api-redis.md` `[KEYSPACE_NOTIFICATION]`: a disconnected subscriber misses pushes, so the push REFINES the stream cursor and never replaces it); both lanes feed the op-log HLC-cursor peer-replay rather than replacing it, so a Redis-absent profile is bit-identical to the TTL-bounded baseline (page invalidation row [5] deliberately lacks a backplane and the fabric is strictly additive over it); the RESP3 server-assisted client-side caching arms through `CLIENT TRACKING ON BCAST` over the raw `IDatabase.Execute` escape hatch where no typed member exists (`.api/api-redis.md` `[RESP3_CLIENT_SIDE_CACHING]`), the keyevent notification arms through the typed `IServer.ConfigSet("notify-keyspace-events", "KEA")` and drains the `ChannelMessageQueue` by `await foreach` over its `IAsyncEnumerable<ChannelMessage>` self, the durable cursor arms through `StreamCreateConsumerGroupAsync`/`StreamReadGroupAsync`/`StreamAcknowledgeAsync` with `claimMinIdleTime` idle-claim takeover, and the stampede single-flight plus the cross-process writer-lease fence ride one atomic `LoadedLuaScript.EvaluateAsync` rather than a managed compare-loop; the Lua scripts, the keyevent subscription, the consumer group, and the tracking arming are connection-instance state on the multiplexer, never persisted, so a process restart re-arms them and resumes the durable cursor from its committed `XACK` position with no durable residue; AppHost still owns the cache port, stampede policy, and tag vocabulary (`AppHost/runtime#cache-pool-lanes`), so the fabric deepens the one contribution row and never reverses the dependency; the `emit` storage-seam channel is a non-blocking submit (the composition root binds it to a bounded channel writer per the boundary handoff law, never a synchronous sink call on the hot read path), and `CacheResidence.Active` is settled exactly once by `Register` because `HybridCache` hides the serving tier from the read-through — the storage seam is the one site that knows an L1-missed read reached the L2 store; the capsule deletes a Persistence-owned cache system, a hand-rolled fake cache type, a residence-per-provider cache class, a managed compare-loop single-flight, a lossy-push-only invalidation that silently strands stale L1 entries, an untyped fabric error, a decorative fault case never produced, a hardcoded-tier fact residence, and every second cache owner in the suite.

```csharp signature
public sealed class CacheContribution(
    MessagePackSerializerOptions wire,
    Func<string, CancellationToken, ValueTask<byte[]?>> read,
    Func<string, IBufferWriter<byte>, CancellationToken, ValueTask<bool>> readInto,
    Func<string, ReadOnlySequence<byte>, Instant, CancellationToken, ValueTask> write,
    Func<string, CancellationToken, ValueTask> evict,
    Func<CacheIndexFact, ValueTask> emit,
    ClockPolicy clocks) : IBufferDistributedCache, IHybridCacheSerializerFactory {
    private const string L2Lane = "l2-store";

    public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default) {
        var bytes = await read(key, token).ConfigureAwait(false);
        // The L2 read fires only after the HybridCache L1 missed, so a non-null payload IS the L1-miss-promoted-from-L2 transition the read-through itself cannot see.
        if (bytes is { } served) { await emit(new CacheIndexFact(L2Lane, CacheKind.L2Promote, key, served.LongLength, CacheResidence.Active)).ConfigureAwait(false); }
        return bytes;
    }

    public bool TryGet(string key, IBufferWriter<byte> destination) => TryGetAsync(key, destination).GetAwaiter().GetResult();

    public async ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) {
        var hit = await readInto(key, destination, token).ConfigureAwait(false);
        if (hit) { await emit(new CacheIndexFact(L2Lane, CacheKind.L2Promote, key, destination is ArrayBufferWriter<byte> w ? w.WrittenCount : 0L, CacheResidence.Active)).ConfigureAwait(false); }
        return hit;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).GetAwaiter().GetResult();

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        SetAsync(key, new ReadOnlySequence<byte>(value), options, token).AsTask();

    public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).GetAwaiter().GetResult();

    public ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        write(key, value, clocks.Now + Duration.FromTimeSpan(options.AbsoluteExpirationRelativeToNow ?? DeadlineClass.CacheTtl.Allotted.ToTimeSpan()), token);

    public void Refresh(string key) { }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

    public Task RemoveAsync(string key, CancellationToken token = default) => evict(key, token).AsTask();

    public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer) => (serializer = new Codec<T>(wire)) is not null;

    private sealed class Codec<T>(MessagePackSerializerOptions wire) : IHybridCacheSerializer<T> {
        public T Deserialize(ReadOnlySequence<byte> source) => MessagePackSerializer.Deserialize<T>(source, wire);

        public void Serialize(T value, IBufferWriter<byte> target) => MessagePackSerializer.Serialize(target, value, wire);
    }
}
```

```csharp signature
public sealed class CacheKeyPolicy : IEqualityComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CacheKeyPolicy, string>]
public sealed partial class CacheResidence {
    public static readonly CacheResidence InMemory = new("in-memory");
    public static readonly CacheResidence Sqlite = new("sqlite");
    public static readonly CacheResidence Redis = new("redis");
    public static readonly CacheResidence DistributedPg = new("distributed-pg");

    // The bound L2 residence, settled once at composition by `Register`; the read-through seam reads it because `HybridCache` hides which tier served a value, so a fact attributes the configured residence, never a hardcoded tier.
    public static CacheResidence Active { get; private set; } = Sqlite;

    public static Func<IServiceCollection, IServiceCollection> Register(
        CacheResidence residence, string endpoint, string instance,
        Func<IServiceProvider, IDistributedCache> pgBacked, Option<RedisFabric> fabric) =>
        (Active = residence).Switch(
            state: (Endpoint: endpoint, Instance: instance, PgBacked: pgBacked, Fabric: fabric),
            inMemory: static _ => static services => services.AddDistributedMemoryCache(),
            sqlite: static _ => static services => services,
            redis: static s => services => services
                .AddStackExchangeRedisCache(o => {
                    o.ConfigurationOptions = Resp3(s.Endpoint);
                    o.InstanceName = s.Instance;
                })
                .AddSingleton(_ => s.Fabric.Match(
                    Some: live => live.Arm(ConnectionMultiplexer.Connect(Resp3(s.Endpoint)), s.Instance),
                    None: () => RedisFabric.Inert)),
            distributedPg: static s => services => services.AddSingleton<IDistributedCache>(s.PgBacked));

    private static ConfigurationOptions Resp3(string endpoint) {
        var options = ConfigurationOptions.Parse(endpoint);
        options.Protocol = RedisProtocol.Resp3;
        return options;
    }
}

[Union]
public abstract partial record CacheFault : Expected, IValidationError<CacheFault> {
    private CacheFault(string detail, int code) : base(detail, code, None) { }

    public static CacheFault Create(string message) => new Uncategorized(message);

    public sealed record FabricUnarmed : CacheFault {
        public FabricUnarmed(string detail = "redis fabric absent or unarmed") : base(detail, 8290) { }
    }
    public sealed record SubscriptionLost : CacheFault {
        public SubscriptionLost(string channel) : base($"keyevent subscription lost: {channel}", 8291) => Channel = channel;
        public string Channel { get; }
    }
    public sealed record LeaseRejected : CacheFault {
        public LeaseRejected(string key) : base($"single-flight lease not granted: {key}", 8292) => Key = key;
        public string Key { get; }
    }
    public sealed record ReplayGap : CacheFault {
        public ReplayGap(string group, string consumer) : base($"invalidation consumer-group drain faulted: {group}/{consumer}", 8293) => (Group, Consumer) = (group, consumer);
        public string Group { get; }
        public string Consumer { get; }
    }
    public sealed record Uncategorized : CacheFault {
        public Uncategorized(string detail) : base(detail, 8299) { }
    }
}

public sealed record RedisFabric(
    Func<IConnectionMultiplexer, IO<ChannelMessageQueue>> ServerAssistedInvalidate,
    Func<IConnectionMultiplexer, IO<ChannelMessageQueue>> KeyeventStream,
    Func<IDatabase, (string Consumer, RedisValue From), IO<StreamEntry[]>> StreamCursor,
    Func<IDatabase, RedisValue[], IO<long>> StreamAck,
    Func<IDatabase, RedisKey, RedisValue, IO<bool>> SingleFlightLease,
    LoadedLuaScript LeaseScript) {
    public const string InvalidateStream = "__cache:invalidate";
    public const string Group = "rasm-cache-l1";

    public static readonly RedisFabric Inert = new(
        static _ => IO.fail<ChannelMessageQueue>(new CacheFault.FabricUnarmed()),
        static _ => IO.fail<ChannelMessageQueue>(new CacheFault.FabricUnarmed()),
        static (_, _) => IO.fail<StreamEntry[]>(new CacheFault.FabricUnarmed()),
        static (_, _) => IO.fail<long>(new CacheFault.FabricUnarmed()),
        static (_, key, _) => IO.fail<bool>(new CacheFault.LeaseRejected(key.ToString())),
        default!);

    public RedisFabric Arm(IConnectionMultiplexer multiplexer, string consumer) {
        var server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);
        server.ConfigSet("notify-keyspace-events", "KEA");
        var database = multiplexer.GetDatabase();
        if (!database.KeyExists(InvalidateStream) || (database.StreamGroupInfo(InvalidateStream).All(g => g.Name != Group))) {
            database.StreamCreateConsumerGroup(InvalidateStream, Group, StreamPosition.NewMessages, createStream: true);
        }
        var lease = LuaScript.Prepare(
            "if redis.call('GET', @key) == false then redis.call('SET', @key, @owner, 'PX', @ttlMs) return 1 else return 0 end").Load(server);
        return this with {
            LeaseScript = lease,
            ServerAssistedInvalidate = mux => IO.liftAsync(async () => {
                ((IDatabase)mux.GetDatabase()).Execute("CLIENT", "TRACKING", "ON", "BCAST");
                return await mux.GetSubscriber().SubscribeAsync(RedisChannel.Literal("__redis__:invalidate")).ConfigureAwait(false);
            }),
            KeyeventStream = mux => IO.liftAsync(async () =>
                await mux.GetSubscriber().SubscribeAsync(RedisChannel.Pattern("__keyevent@*__:*")).ConfigureAwait(false)),
            StreamCursor = (db, position) => IO.liftAsync(async () =>
                await db.StreamReadGroupAsync(InvalidateStream, Group, position.Consumer, position.From, count: 256, noAck: false,
                    claimMinIdleTime: DeadlineClass.CacheTtl.Allotted.ToTimeSpan()).ConfigureAwait(false)),
            StreamAck = (db, ids) => IO.liftAsync(async () =>
                await db.StreamAcknowledgeAsync(InvalidateStream, Group, ids).ConfigureAwait(false)),
            SingleFlightLease = (db, key, owner) => IO.liftAsync(async () =>
                (bool)await lease.EvaluateAsync(db, new { key, owner, ttlMs = (long)DeadlineClass.CacheTtl.Allotted.TotalMilliseconds }).ConfigureAwait(false)),
        };
    }

    // Pending-list (`0`) drain replays this consumer's un-`XACK`'d entries on reconnect before `>` serves live messages: the at-least-once guarantee a disconnect cannot strand. The async-stream loop is the named statement exemption; a drained cursor fault converts once into a terminal `ReplayGap` element so the durable cursor resumes from its committed `XACK` on the next `Replay`, never strands a raw exception.
    public async IAsyncEnumerable<Fin<CacheIndexFact>> Replay(IDatabase database, string consumer, [EnumeratorCancellation] CancellationToken token = default) {
        var env = EnvIO.New(token: token);
        var from = StreamPosition.Beginning;
        while (!token.IsCancellationRequested) {
            var batch = (await StreamCursor(database, (consumer, from)).Try().RunAsync(env).ConfigureAwait(false))
                .Match(Succ: static entries => entries, Fail: static _ => default(StreamEntry[]?));
            if (batch is null) { yield return Fin.Fail<CacheIndexFact>(new CacheFault.ReplayGap(Group, consumer)); yield break; }
            from = batch is { Length: 0 } && from == StreamPosition.Beginning ? StreamPosition.NewMessages : from;
            foreach (var entry in batch) {
                yield return Fin.Succ(new CacheIndexFact(InvalidateStream, CacheKind.Invalidate, entry["key"].ToString(), 0L, CacheResidence.Redis));
            }
            if (batch is { Length: > 0 }) {
                _ = await StreamAck(database, [.. batch.Select(static e => e.Id)]).Try().RunAsync(env).ConfigureAwait(false);
            }
        }
    }

    // The keyevent/RESP3 hint drain is best-effort; a dropped subscription converts once into a terminal `SubscriptionLost` element naming the lane, never a swallowed callback fault.
    public static async IAsyncEnumerable<Fin<CacheIndexFact>> Observe(ChannelMessageQueue queue, string lane, [EnumeratorCancellation] CancellationToken token = default) {
        await foreach (var message in queue.WithCancellation(token).ConfigureAwait(false)) {
            yield return Fin.Succ(new CacheIndexFact(lane, CacheKind.Invalidate, message.Message.ToString(), 0L, CacheResidence.Redis));
        }
        if (!token.IsCancellationRequested) { yield return Fin.Fail<CacheIndexFact>(new CacheFault.SubscriptionLost(lane)); }
    }
}
```

| [INDEX] | [LAW]         | [RULING]                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | ownership     | port, stampede protection, tag vocabulary, and entry options stay at the AppHost cache port; the contribution is storage and codec only                                                                                                                                                                                                                                                                                                                                                                                                                             |
|  [02]   | payload       | every L2 payload lands as one key-value row whose codec-id and content-hash columns are settled row law — codec-tagged bytes through `IHybridCacheSerializer<T>`, never bare blobs                                                                                                                                                                                                                                                                                                                                                                                   |
|  [03]   | expiry        | only the write's absolute relative expiry crosses into `ExpiresAt`, stamped through `ClockPolicy`; an absent value traces to the `DeadlineClass.CacheTtl` deadline row                                                                                                                                                                                                                                                                                                                                                                                               |
|  [04]   | sweep         | expired rows leave on the persistence-maintenance `ScheduleEntry` row under the maintenance lease; each sweep deletion emits one `CacheKind.Evict` fact, and the read-through seam mints exactly one `CacheKind.Hit` or `CacheKind.Miss` per `Result` from the factory-invocation observation, so the asserted fact stream is produced rather than declared                                                                                                                                                                                                            |
|  [05]   | invalidation  | `RemoveByTagAsync` is logical; peers replay entity-kind transitions from the op-log HLC cursor and L1 staleness stays TTL-bounded with no backplane as the baseline; where the `RedisFabric` is bound the at-least-once `XREADGROUP` consumer-group cursor over `__cache:invalidate` is the redis backplane of record that collapses L1 staleness from TTL-bounded to invalidation-driven and replays every missed invalidation on reconnect, while the RESP3 `__redis__:invalidate` push and the keyevent stream are best-effort low-latency hints that race the cursor, never the guarantee — both feed the op-log HLC cursor, refining the replay path, never replacing it, so a Redis-absent profile is bit-identical; each cursor entry, push, or key-transition folds through `Replay`/`Observe` into an `Invalidate` fact and the drained ids `XACK` the group cursor |
|  [06]   | residence     | `CacheResidence.Sqlite` is the default L2; each tier is one `Register` row binding the same `IDistributedCache` slot, content-address-keyed so an L1 miss promotes from any tier without a re-mint; `Redis` rides `AddStackExchangeRedisCache` + `RedisCache : IBufferDistributedCache` and arms `RedisProtocol.Resp3` plus the optional `RedisFabric`, `DistributedPg` the pg-backed row; zero change to any owner declared here                                                                                                                                          |
|  [07]   | tier evidence | every `CacheIndexFact` carries a typed `CacheKind Kind` and `CacheResidence Residence`, never free strings; the tier transition is observable only at the storage seam where it fires — `CacheContribution.TryGetAsync`/`GetAsync` emits `CacheKind.L2Promote` exactly when its L2 fetch serves a hit that the `HybridCache` L1 missed, because the `HybridCache` read-through hides L1-versus-L2 and so attributes the configured `CacheResidence.Active` rather than a tier; a durable `XREADGROUP` cursor entry, a RESP3 invalidation push, or a keyevent key-transition emits `CacheKind.Invalidate` through `Replay`/`Observe`; both members cross the wire as their `.Key` through the Thinktecture converter, and Redis `ProfilingSession` rides the existing telemetry contribution                                                                                                          |
|  [08]   | single-flight | the stampede single-flight and the cross-process writer-lease fence ride one atomic `LoadedLuaScript.EvaluateAsync` (`RedisFabric.SingleFlightLease`) rather than a managed compare-loop, the lease keyed under the `DeadlineClass.CacheTtl` allotment, a non-grant routing to `CacheFault.LeaseRejected`; absent the fabric the AppHost-owned managed stampede policy is the unchanged baseline                                                                                                                                                                          |
|  [09]   | fabric fault  | the live-fabric arm, the keyevent drain, the durable cursor replay, and the lease eval fold their causes into the closed `CacheFault` family deriving from `Expected, IValidationError<CacheFault>` — `FabricUnarmed`, `SubscriptionLost`, `LeaseRejected`, `ReplayGap`, `Uncategorized` in the 8290 band; a bare `Error.New` over a multi-cause domain is the deleted form                                                                                                                                                                                                                                                            |

## [03]-[MODEL_RESULT_INDEX]

`ModelResultKey` is the deterministic cross-process result-reuse identity Compute composes — `Compute/Model/inference#RESULT_CACHE` constructs it as `(model checksum, input hash, ep key)` and reads `ModelChecksum`, so the name, the positional shape, and `ModelChecksum` are a cross-package contract this owner holds frozen. The `Query/federation#FUSION_RANK` rule result, the `Query/federation#ENTITY_GRAPH` selection result, and the projection-lane read all reuse through this one key, and `ModelResultKey.RecencyHorizon` is the suite's sole cross-process result-reuse recency horizon every reuse surface — Compute's `LinearProvider.Select` (`Compute/Tensor/blas`), the benchmark claim gate, and the projection lane — traces to by reference.

- Owner: `ModelResultKey` — the zero-alloc `readonly record struct` deterministic result identity (`ModelChecksum`, `InputHash`, `EpKey`); `CacheKind` `[SmartEnum<string>]` — the closed cross-process cache-evidence vocabulary every fact and every dispatch reads, its `.Key` the persisted wire string; `CacheIndexFact`, `IndexSurface` — the one fact stream typed on `CacheKind`/`CacheResidence` and the read-through dispatch shared by all three indexes; `ModelResultKey.RecencyHorizon` — the one owned recency bound.
- Cases: `CacheKind` `Hit` | `Miss` | `Evict` | `L2Promote` | `Invalidate` — `Hit` and `Miss` mint at the `HybridCache` read seam from the factory-invocation observation, `Evict` from the maintenance sweep, `L2Promote` at the `CacheContribution` storage seam when its L2 `TryGetAsync` serves a hit the `HybridCache` L1 missed (the one tier transition the read-through itself cannot see), `Invalidate` a RESP3 `__redis__:invalidate` push, a keyspace-notification key-transition, or a durable `XREADGROUP` cursor entry where the live fabric is present; a `Switch` over the closed vocabulary breaks at compile time when a kind is added, never a silent string `_` arm.
- Entry: `ValueTask<T> Result<T, TState>(ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token = default, ReceiptSinkPort? sink = null, CorrelationId correlation = default, CacheLane? lane = null)` on `HybridCache` — one tenant-partitioned read-through carrier over the model-result and projection lanes, the optional `lane` selecting the L2 backing and defaulting to `CacheLane.ModelResult`; it keys on `key.Scoped(TenantContext.Current.TenantId)` so a multi-tenant host never collides two tenants' entries, threads `produce` through a miss-observing wrapper so where a `sink` is supplied the read mints exactly one `CacheKind.Hit` or `CacheKind.Miss` fact through `sink.Fact` (a Compute caller owning its own fact stream passes no sink and opts out of the duplicate emission), and `Of` derives input identity via `XxHash128.HashToUInt128` so no caller hashes by hand.
- Auto: `Result` composes the port read — lane scoping, stampede single-flight, the lane tag, and the model-checksum tag all ride one `CacheSurface.Read` call with zero call-site ceremony; the factory wrapper sets a captured miss flag only when `HybridCache` invokes the producer on an L1+L2 miss, so the post-read fact is `Miss` exactly when the value was minted and `Hit` exactly when both tiers served it — the evidence the prose asserts is produced, never merely declared; a model-inference reuse passes `CacheLane.ModelResult` and a federated-projection reuse passes `CacheLane.Projection` from the one entrypoint, so the projection lane gains a reuse spelling without a second key shape; `Of` folds the `ReadOnlySpan<byte>` input through the same `XxHash128.HashToUInt128` law the `Compute/Runtime/codecs#CONTENT_ADDRESSING` content key and the `Query/federation#ELEMENT_SET_ALGEBRA` `EntityIdentity.Key` ride, so the result identity and the content identity it caches are minted by one hash, never two.
- Receipt: `CacheIndexFact` carries `CacheKind Kind` and `CacheResidence Residence` typed, never free strings — `Miss` on producer invocation, `Hit` on completion without it (both attributing `CacheResidence.Active` since the read-through hides the serving tier), `Evict` from the sweep, `L2Promote` from the `CacheContribution.GetAsync`/`TryGetAsync` storage seam where the L2 fetch fires on a `HybridCache` L1 miss, `Invalidate` from a fabric push folded through `Observe`/`Replay` — each HLC-stamped through `ReceiptSinkPort` and consumed by compute-side substrate selection; `Bytes` is the served payload length where the emitting seam knows it — `GetAsync` stamps the `byte[].LongLength` and `TryGetAsync` the `ArrayBufferWriter<byte>.WrittenCount`, the blob index stamps `StoredBytes` — and is `0L` at the `HybridCache` read-through seam where `GetOrCreateAsync` hides the serialized length and on a pushed `Invalidate` that carries no payload, so a `Bytes`-bearing `Hit`/`Miss` never originates from the read-through.
- Packages: `System.IO.Hashing`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project); BCL inbox.
- Growth: a new reuse lane is one `CacheLane` argument on the one `Result` entrypoint; a new evidence kind is one `CacheKind` row, breaking every `Switch` over the vocabulary at compile time; a new result family is one tag value on the same key shape; zero new surface — a parallel projection-key record, a per-lane result-reuse entrypoint, a free-string `Kind` discriminant, or a floating per-call horizon parameter is the deleted form.
- Boundary: keys cross as canonical ordinal strings through `ISpanFormattable` and `IUtf8SpanFormattable`; `ModelChecksum` and `EpKey` arrive as opaque wire strings so no compute vocabulary leaks into the key shape, and the positional `(ModelChecksum, InputHash, EpKey)` order plus the `ModelChecksum` accessor stay frozen because `Compute/Model/inference#RESULT_CACHE` `CacheOps.Key`/`Validated` construct and read them by that exact shape — tenant partitioning therefore lives on `Scoped`, the one owned cache-KEY projection, never on the record's positional fields, so the `AppHost/Runtime/ports#PORT_VOCABULARY` `TenantId` cache-key partition contract is honored without breaking the Compute constructor — `Scoped(TenantId)` is the single cache-key spelling, and every `ModelResultKey` consumer that reads, stores, or cuts an entry MUST key on `Scoped(TenantContext.Current.TenantId)` so a hit served and an entry cut target the same tenant-partitioned key (the `Compute/Model/inference#RESULT_CACHE` `CacheOps.Fresh` store/cut path keys on `CacheLane.ModelResult.Scoped(key.ToString())`, a bare un-tenant-salted key that DIVERGES from this read-through's `Scoped(tenant)` key — the cross-package alignment that lifts `CacheOps.Fresh` onto `ModelResultKey.Scoped(tenant)` is the live residual, since an un-tenant-salted store under a tenant-salted read leaves a multi-tenant store/cut invisible to the read-through); the `TenantId` arrives as the `[ValueObject<UInt128>]` whose `Value:x32` salts the key, so the uuid-cast RLS predicate and the cache-key partition read one identity; the projection-lane reuse rides the same key through the `lane` argument rather than a second key type, because the `CacheLane.Projection` L2 backing (`AppHost/Runtime/resources#CACHE_PORT`) differs only in store routing, never in identity; eviction eligibility is retention-axis law and the index contributes only its `ArtifactClass.KvRow` reference; the fact record lands as one `[JsonSerializable]` row on the `Version/snapshots#CODEC_AXIS` `PersistenceWireContext` and its `CacheKind`/`CacheResidence` members cross through the `Schema/converters#CONVERTER_RAIL` Thinktecture converter as bare key strings; the cluster deletes per-index repository classes, a parallel projection-key record, a free-string evidence discriminant, and a generic receipt abstraction over hit/miss evidence; `RecencyHorizon` is the one owned recency bound — the benchmark claim gate and the compute-side result-reuse and route-selection consumers reference this value rather than minting a second `Duration horizon`, so a stale cross-process reuse is impossible by construction and a floating per-call horizon parameter is the deleted form.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CacheKeyPolicy, string>]
public sealed partial class CacheKind {
    public static readonly CacheKind Hit = new("cache-hit");
    public static readonly CacheKind Miss = new("cache-miss");
    public static readonly CacheKind Evict = new("cache-evict");
    public static readonly CacheKind L2Promote = new("cache-l2-promote");
    public static readonly CacheKind Invalidate = new("cache-invalidate");
}

public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputHash, string EpKey) : ISpanFormattable, IUtf8SpanFormattable {
    public static readonly Duration RecencyHorizon = Duration.FromHours(24);

    public static ModelResultKey Of(string modelChecksum, ReadOnlySpan<byte> input, string epKey) =>
        new(modelChecksum, XxHash128.HashToUInt128(input), epKey);

    public string Scoped(TenantId tenant) => $"{tenant.Value:x32}:{ModelChecksum}:{InputHash:x32}:{EpKey}";

    public override string ToString() => $"{ModelChecksum}:{InputHash:x32}:{EpKey}";

    public string ToString(string? format, IFormatProvider? provider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{ModelChecksum}:{InputHash:x32}:{EpKey}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{ModelChecksum}:{InputHash:x32}:{EpKey}", out bytesWritten);
}

public readonly record struct CacheIndexFact(string Lane, CacheKind Kind, string Key, long Bytes, CacheResidence Residence);

public static class IndexSurface {
    // Receiver invariant: `Result` is invoked on the lane-resolved keyed cache (`provider.Cache(lane).Result(...)`), so the receiver and `lane` name one L2 backing; `HybridCache` hides tier and serialized length, so the read-through `Hit`/`Miss` carries the configured `CacheResidence.Active` and `0L` bytes — `L2Promote` is the storage-seam transition, never spelled here.
    extension(HybridCache cache) {
        public async ValueTask<T> Result<T, TState>(
            ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce,
            CancellationToken token = default, ReceiptSinkPort? sink = null, CorrelationId correlation = default, CacheLane? lane = null) {
            var (resolved, scoped, minted) = (lane ?? CacheLane.ModelResult, key.Scoped(TenantContext.Current.TenantId), new StrongBox<bool>(false));
            var value = await cache.Read(resolved, scoped, (State: state, Produce: produce, Minted: minted),
                static async (carrier, ct) => { carrier.Minted.Value = true; return await carrier.Produce(carrier.State, ct).ConfigureAwait(false); },
                Some(Seq(key.ModelChecksum)), token).ConfigureAwait(false);
            if (sink is { } observed) {
                _ = await observed.Fact(correlation, new CacheIndexFact(resolved.Key, minted.Value ? CacheKind.Miss : CacheKind.Hit, scoped, 0L, CacheResidence.Active))
                    .RunAsync(EnvIO.New(token: token)).ConfigureAwait(false);
            }
            return value;
        }
    }

    extension(ReceiptSinkPort sink) {
        public IO<ReceiptEnvelope> Fact(CorrelationId correlation, CacheIndexFact fact) =>
            sink.Send(correlation, TenantContext.Current, "Rasm.Persistence", fact.Kind.Key, JsonSerializer.SerializeToElement(fact, PersistenceWireContext.Default.CacheIndexFact));
    }
}
```

## [04]-[ARTIFACT_BLOB_INDEX]

Every blob is one content-addressed catalog row carrying two keys — a per-row `Path` lookup address and a cross-projection `UInt128 SourceKey` join — plus the `ShortTag` chunk-dedup pre-filter and the typed retention reference. The `Project` fold joins every projection of one source artifact by `SourceKey`; the `Holds`/`MayHold` membership projections serve the `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel` dedup probe so a re-store transfers only the chunks the index lacks. Compute owns each content-key derivation and this owner owns the blob residence, neither re-declaring the other.

- Owner: `ArtifactIndexRow` — content-addressed catalog row for execution-provider warm-start contexts, ONNX profiling traces, IFC semantic-ingest model graphs, content-defined chunks, Compute interchange artifacts (tessellated GLB, chunked field, tile content, re-exported glTF), and graduated offline-science ONNX surrogate assets on the blob lane; `ArtifactKind` `[SmartEnum<string>]` the closed family discriminant and `CompressionPosture` `[SmartEnum<string>]` the observed-frame discriminant; the static `Project` fold is the source-artifact join, `Novel` the chunk-dedup pre-filter projection over the `ShortTag` and `ContentHash` columns.
- Cases: `ArtifactKind` `EpContext` | `OnnxProfile` | `IfcSemantic` | `ChunkContent` | `Interchange` | `GraduationAsset`, and `CompressionPosture` `None` | `Lz4BlockArray`; the `SourceKey` column is the source-artifact content key (the Compute `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` of the source IFC) that the `IfcSemantic` graph projection and the `Interchange` GLB projection both carry, so the two rows join on one source identity while each keeps its own payload `ContentHash`.
- Entry: `static ArtifactIndexRow Admit(ArtifactKind kind, string path, ReadOnlySpan<byte> stored, DataClassification classification, ArtifactClass retention, Instant at, Option<UInt128> sourceKey = default, Option<UInt128> providerFingerprint = default, long plainBytes = -1, CompressionPosture? compression = null)` — pure value; the `stored` span is the post-compression representation, so the `ContentHash` derives from it through `XxHash128.HashToUInt128` and the `ShortTag` from the same bytes through `XxHash3.HashToUInt64` (the `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Content` 64-bit pre-filter tag), exactly the `api-hashing#INTEGRATION_LAW` law that the content key keys the stored bytes; `StoredBytes := stored.Length` is the on-disk length the retention sweep bounds and `plainBytes` the pre-compression length (defaulting to `stored.Length` for an uncompressed `CompressionPosture.None` artifact), the two folding `Ratio`; the `SourceKey` carries the joining source-artifact identity, never the caller's path; an absent `sourceKey` self-keys (`SourceKey := ContentHash`) so a single-projection artifact forms a degenerate one-member group, while a multi-projection family passes `Some(sourceIfcKey)`; `providerFingerprint` carries the `GraduationAsset` `(checksum, OrtEpDevice)` digest the `Version/provenance#ATTESTED_LEDGER` chains, folding `UInt128.Zero` for every non-graduation kind.
- Auto: `Admit` stamps the stored content hash, the chunk short tag, the joining source key, stored and plain byte lengths, the compression posture, classification, retention class, instant, and the optional provider fingerprint in one call; reads ride the `CacheLane.ArtifactBlob` lane through the port with the same stampede and tag law as every cache read; the row carries two distinct keys — the string `Path` (the per-row lookup address) and the `UInt128 SourceKey` (the cross-projection join) — and the Bim tessellation cache and wire snapshot read each by the matching key; `Project` resolves every projection of one source artifact by its `SourceKey`, and `Novel` projects the chunks a peer or the index lacks by probing the cheap 64-bit `ShortTag` membership ahead of the authoritative 128-bit `ContentHash` compare so a tag-miss skips the full lookup on a hot re-store path.
- Receipt: a graduated surrogate re-admission re-verifies its `providerFingerprint` against the prior row and routes a divergence to the `Version/provenance#ATTESTED_LEDGER` fingerprint check before inference; a chunked store rides the snapshot owner's `store.chunk.split`/`store.chunk.dedup` receipts over the `Novel` projection.
- Packages: `System.IO.Hashing`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project).
- Growth: a new artifact family is one `ArtifactKind` row breaking every `Switch` over the family at compile time; a new compression frame is one `CompressionPosture` row; a content-defined chunk lands as one `ArtifactKind.ChunkContent` row keyed by its `XxHash128` chunk address with its `XxHash3` `ShortTag` (`Version/snapshots#CONTENT_CHUNKING`) so an identical chunk across snapshots and peers dedups on the one content key through the pre-filter, never a second chunk store; a graduated offline-science surrogate lands as one `ArtifactKind.GraduationAsset` row content-addressed identically over the post-`ModelAssetManifest`-validated ONNX bytes carrying its provider fingerprint, so a re-graduated model dedups on its content key and a sibling graduation-artifact record is the deleted form; zero new surface — a free-string kind or a free-string compression flag is the deleted form.
- Boundary: every artifact lands on the blob lane under its `ContentHash` and receipts name index rows by path — loose temp files are the deleted form; `Classification` enters typed at admission as a `DataClassification` and the unclassified-write rejection is store-side column law through `Version/retention#CLASSIFICATION_ENFORCEMENT` `ClassificationGuard.Admit`; index rows carry their `ArtifactClass` retention reference (the typed `BlobIndex` row, never a free string) while eligibility folds belong to the retention axis, and the `SizeBound`-2GiB `BlobIndex` sweep bounds on `StoredBytes` — the on-disk length the row records from its own sealed frame per `durability#SEAL_LAW`, never a later filesystem stat — so the plain length and the `Ratio` are dashboard evidence while the stored length is the budget axis; the `SourceKey` join is the durable multi-projection home — the `IfcSemantic` graph row, the `Interchange` GLB row, and the wire-snapshot row of one source IFC carry the same source-artifact `SourceKey` (the Compute `InterchangeIdentity`, equal to the Bim `IfcContentKey`) so a content-addressed source artifact is one source identity with several payload projections joined by `Project`, each projection landing under its own string `Path`; the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` `TessellationRequest.Resolve` cache leg looks up the prior GLB by its string lookup address (the Bim `ArtifactKey` string `$"{IfcContentKey:x32}:glb"`, the GLB row's `Path`) so a re-tessellation of an unchanged IFC reads the prior GLB by reference, and that GLB row's `SourceKey` is the value `Project` joins on; the `Exchange/wire#WIRE_PROJECTION` `BimWire` snapshot row admits under its own `$"{ContentKey:x32}:bim-wire"` address while carrying the same source-IFC `SourceKey`, and the `Exchange/import#IMPORT_RAIL` incremental-delta reimport resolves the prior `IfcSemantic` graph by `Project` over that source key so a re-import diffs against the prior graph and re-fetches only changed `BimWire` element rows; the Bim side mints only the string `ArtifactKey` address and the `IfcContentKey` it folds and holds no Persistence reference — neither `ArtifactIndexRow`, the `SourceKey` column, nor `Project` crosses into Bim, the artifact index being the Persistence owner's concern the app-platform caller reads at the seam; the `IfcSemantic` kind admits the Compute interchange IFC model graph — the `DatabaseIfc`-extracted property sets, spatial hierarchy, quantities, materials, and type objects serialized to the canonical bytes the `Bim/Exchange/import#IMPORT_RAIL` produces — content-addressed identically through `XxHash128.HashToUInt128`, so the same model graph re-ingested under the same tolerance dedupes on its content key, and the admission carries the model graph only, never tessellated BRep geometry: the two-hop tessellation rail (`Ifc -> IfcOpenShell -> GLB`) is a Compute companion concern whose GLB rides the `Interchange` kind that `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Admit` content-addresses, so this kind owns semantic residence and the geometry residence stays a distinct `Interchange` blob row; the `Interchange` kind admits every Compute interchange artifact — the tessellated GLB, the chunked field artifact, the 3D-Tiles leaf content, and the re-exported glTF — each content-addressed through `XxHash128.HashToUInt128` over its canonical bytes under one identity scheme, so Compute owns the content-key derivation and this owner owns the blob residence; the model-graph residence projection onto the document and search lanes is owned at `Query/lanes#DOCUMENT_LANE` and `Query/lanes#SEARCH_LANES` and consumed here as the index identity only; the `GraduationAsset` kind admits the `ONE_GRADUATION_EVIDENCE` `HandoffAxis` surrogate — the content-keyed ONNX artifact a graduated Python offline result produces (post-`ModelAssetManifest` validation) — content-addressed identically through `XxHash128.HashToUInt128`, so the C# side runs inference over a durable content key the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` chains and the `AppHost/Runtime/determinism#EVENT_LOG` determinism closure references, never an in-process training loop; the asset's `providerFingerprint` is the same `(checksum, OrtEpDevice)` digest the attested ledger folds into its chain, so a re-import re-verifies it at admission and a provider-divergent surrogate fit is caught before inference; the asset rides the existing object-store residence and the `Version/retention.md#RETENTION_SWEEPS` `ClosureGc` reachability sweep so a stale surrogate is collected by the one GC, and Python (`compute/graduation#GRADUATION`) holds the rail singular and this owner adds only the durable blob residence; the `ShortTag` column is the `Version/snapshots#CONTENT_CHUNKING` dedup pre-filter face only — `Novel` probes `MayHold(shortTag)` before `Holds(contentHash)` so a tag-miss proves a chunk novel without the full content-key compare and a `ShortTag` false positive only costs one fall-through `Holds`, never a wrong dedup — so the 128-bit `ContentHash` stays the authoritative dedup identity while the 64-bit tag culls the lookup.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CacheKeyPolicy, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind EpContext = new("ep-context");
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile");
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic");
    public static readonly ArtifactKind ChunkContent = new("chunk-content");
    public static readonly ArtifactKind Interchange = new("interchange");
    public static readonly ArtifactKind GraduationAsset = new("graduation-asset");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CacheKeyPolicy, string>]
public sealed partial class CompressionPosture {
    public static readonly CompressionPosture None = new("none");
    public static readonly CompressionPosture Lz4BlockArray = new("lz4-block-array");
}

public readonly record struct ArtifactIndexRow(
    UInt128 ContentHash,
    ulong ShortTag,
    UInt128 SourceKey,
    ArtifactKind Kind,
    string Path,
    long StoredBytes,
    long PlainBytes,
    CompressionPosture Compression,
    DataClassification Classification,
    ArtifactClass Retention,
    Instant At,
    UInt128 ProviderFingerprint) {
    public double Ratio => PlainBytes == 0L ? 1d : (double)StoredBytes / PlainBytes;

    public static ArtifactIndexRow Admit(
        ArtifactKind kind, string path, ReadOnlySpan<byte> stored, DataClassification classification, ArtifactClass retention, Instant at,
        Option<UInt128> sourceKey = default, Option<UInt128> providerFingerprint = default, long plainBytes = -1L, CompressionPosture? compression = null) {
        var contentHash = XxHash128.HashToUInt128(stored);
        return new(contentHash, XxHash3.HashToUInt64(stored), sourceKey.IfNone(contentHash), kind, path,
            stored.Length, plainBytes < 0L ? stored.Length : plainBytes, compression ?? CompressionPosture.None, classification, retention, at, providerFingerprint.IfNone(UInt128.Zero));
    }

    public static Seq<ArtifactIndexRow> Project(Seq<ArtifactIndexRow> rows, UInt128 sourceKey) =>
        rows.Filter(row => row.SourceKey == sourceKey);

    public static Seq<ArtifactIndexRow> Novel(Seq<ArtifactIndexRow> candidates, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        candidates.Filter(row => !mayHold(row.ShortTag) || !holds(row.ContentHash));
}
```

## [05]-[BENCHMARK_INDEX]

Persisted BenchmarkDotNet evidence with the fingerprint-match, recency-bounded claim gate AND the cost-ranked route selector toward route selection. One row carries the full statistical, allocation, and throughput profile a route decision reads — `LatencyProfile` folds the extremes, percentile, dispersion, and confidence-interval summary, `GcProfile` folds the per-generation collection counts, allocated bytes, and per-operation allocation — so a stale or host-foreign benchmark never selects a route and, among the admitted set, the winning route is the one minimizing the `RouteWeight` cost over tail latency, worst-case, allocation, and throughput whose edge clears the measurement noise, not merely the most recent measurement nor a win inside the confidence interval. The contract is read-only toward route selection; the producer mints the rows on the bulk lane under its self-emitted changefeed.

- Owner: `BenchmarkRow` — persisted benchmark evidence with the fingerprint-match, recency-bounded claim gate and the per-case significance-gated cost-ranked winner; `LatencyProfile` — the `[ComplexValueObject]` extremes-percentile-and-dispersion summary (`Min`, `Mean`, `Median`, `Max`, `StdDev`, `ConfidenceHalfWidth`, `P50`, `P90`, `P95`, `P99`) carrying the worst-case bound and the measurement-noise interval a route decision needs, not the central tendency alone; `GcProfile` — the `[ComplexValueObject]` allocation summary (`Gen0`, `Gen1`, `Gen2`, `AllocatedBytes`, `BytesPerOp`); `RouteWeight` — the `[ComplexValueObject]` cost-weight policy whose `Cost` folds a row's `P99`, `Max`, `BytesPerOp`, and `ThroughputOps` into one comparable scalar and whose `Beats` gates a displacement on the combined confidence half-width so a win inside the interval never flips the route.
- Entry: `Claim`/`RouteOf` carry the recency winner (fingerprint admission gated by the `horizon` defaulting to the `ModelResultKey.RecencyHorizon` owned bound, `None` the fall-through toward the caller's static cost rank), and `Select(Seq<BenchmarkRow> rows, string @case, string hostFingerprint, RouteWeight weight, Instant now, Duration horizon = default)`/`WinnerOf` carry the cost winner — the cheapest admitted route for one workload `@case` under `weight.Beats` (the `Cost`-difference gated by the combined `ConfidenceHalfWidth`), taking each route's freshest measurement before ranking so a stale duplicate never out-votes a current one and a noise-level cost edge never flips the route; `Admitted` is the shared fingerprint-and-recency gate both folds compose, never two filters.
- Auto: ingest rides the bulk lane under its self-emitted changefeed and tag-transition law; every row carries the `ArtifactClass.BenchmarkRow` reference so the `CountBound 1024` retention sweep folds over a traceable class; trend history exports to the analytical lane as parquet with zero second pipeline; the latency and GC profiles arrive composed at admission so a dashboard cut reads the dispersion and per-generation collection counts without a second join; `Select` folds the admitted set into a `HashMap<string, BenchmarkRow>` keyed by `Route` keeping each route's freshest measurement, then folds its `Values` through `RouteWeight.Beats` so the winner is the current cheapest route whose cost edge clears the combined confidence interval, not the latest-written row nor a noise-level win.
- Packages: `NodaTime`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `linq2db.EntityFrameworkCore`; `DuckDB.NET.Data.Full`.
- Growth: a new benchmark dimension is one column on `BenchmarkRow` or one member on the `LatencyProfile`/`GcProfile` value object; a new selection objective is one member on `RouteWeight` plus its `Cost` term, never a parallel selector; a new dashboard cut is one analytical export row; zero new surface.
- Boundary: the contract is read-only toward route selection — `Admitted` admits only rows whose `HostFingerprint` matches the calling host with ordinal equality and whose `At` stamp falls inside the staleness `horizon` measured from `now`; over that admitted set `Claim` folds to the most-recent `At` and `RouteOf` projects its `Route` (the recency answer), while `Select` folds each `Route` to its freshest admitted row and folds `RouteWeight.Beats` (the `Cost` edge gated by the combined `ConfidenceHalfWidth`) to the winning row and `WinnerOf` projects its `Route` (the performance answer) — so a stale benchmark never selects a route, a slower route never wins on freshness alone, and a noise-level cost edge never flips the route; the `horizon` argument defaults to the `ModelResultKey.RecencyHorizon` owned bound so this index consumes the one suite recency horizon rather than minting a second `Duration`, and a caller may narrow it but never re-owns it; the recency and cost folds replace the first-ordinal `Find`, an `OrderByDescending` LINQ chain, and a counter loop; the latency summary is a `LatencyProfile` `[ComplexValueObject]` carrying the BenchmarkDotNet `Min`/`Mean`/`Median`/`Max`/`StdDev` distribution, the `ConfidenceHalfWidth` 99.9% interval, and the `P50`/`P90`/`P95`/`P99` percentile band so the cost reads the tail and the worst-case bound and `Select` gates a route flip on statistical significance through `RouteWeight.Beats`, not the median alone, and the allocation summary is a `GcProfile` carrying the `Gen0`/`Gen1`/`Gen2` per-generation collection counts, the `AllocatedBytes` total, and the per-operation `BytesPerOp` an allocation-sensitive route weighs; `RouteWeight.Balanced` is the default policy weighting `P99` and `Max` milliseconds against `BytesPerOp` (at `1e-7`) and `log2` throughput, and a caller passes its own `RouteWeight` to bias latency-critical versus allocation-critical lanes without a second selector; `ThroughputOps` carries the operations-per-second the workload sustained and `WorkloadN` the operation count the measurement folded; the fingerprint value arrives as an opaque string minted by the benchmark producer over the runtime, JIT, and hardware moniker; the `Retention` column carries the typed `ArtifactClass.BenchmarkRow` reference so the retention sweep keys on the same class the redaction axis bounds; the cluster deletes a benchmarks repository service, a second dashboard export path, a flat percentile-field scatter, a parallel route-ranking surface, and a free-string artifact-class column.

```csharp signature
[ComplexValueObject]
public sealed partial class LatencyProfile {
    public Duration Min { get; }
    public Duration Mean { get; }
    public Duration Median { get; }
    public Duration Max { get; }
    public Duration StdDev { get; }
    public Duration ConfidenceHalfWidth { get; }
    public Duration P50 { get; }
    public Duration P90 { get; }
    public Duration P95 { get; }
    public Duration P99 { get; }
}

[ComplexValueObject]
public sealed partial class GcProfile {
    public long Gen0 { get; }
    public long Gen1 { get; }
    public long Gen2 { get; }
    public long AllocatedBytes { get; }
    public double BytesPerOp { get; }
}

[ComplexValueObject]
public sealed partial class RouteWeight {
    public double TailLatency { get; }
    public double WorstCase { get; }
    public double Allocation { get; }
    public double Throughput { get; }

    public static readonly RouteWeight Balanced = Create(tailLatency: 1d, worstCase: 0.25d, allocation: 1e-7d, throughput: 1d);

    public double Cost(BenchmarkRow row) =>
        TailLatency * row.Latency.P99.TotalMilliseconds
        + WorstCase * row.Latency.Max.TotalMilliseconds
        + Allocation * row.Gc.BytesPerOp
        - Throughput * Math.Log2(double.Max(row.ThroughputOps, 1d));

    // A challenger displaces the incumbent only when its cost edge clears the combined measurement noise — the sum of the two confidence half-widths — so a route never flips on a statistically insignificant win inside the interval.
    public bool Beats(BenchmarkRow challenger, BenchmarkRow incumbent) =>
        Cost(incumbent) - Cost(challenger) >
        (challenger.Latency.ConfidenceHalfWidth + incumbent.Latency.ConfidenceHalfWidth).TotalMilliseconds * TailLatency;
}

public readonly record struct BenchmarkRow(
    string Case,
    string Route,
    LatencyProfile Latency,
    GcProfile Gc,
    double ThroughputOps,
    long WorkloadN,
    string HostFingerprint,
    ArtifactClass Retention,
    Instant At) {
    public static Seq<BenchmarkRow> Admitted(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.HostFingerprint, hostFingerprint) && now - row.At <= (horizon == default ? ModelResultKey.RecencyHorizon : horizon));

    public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        Admitted(rows, hostFingerprint, now, horizon).Fold(
            Option<BenchmarkRow>.None,
            static (held, row) => held.Match(prior => row.At > prior.At ? Some(row) : held, () => Some(row)));

    public static Option<string> RouteOf(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        Claim(rows, hostFingerprint, now, horizon).Map(static row => row.Route);

    // Per-case winner: fold each route to its freshest admitted row, then fold the significance-gated cost so a stale duplicate never out-votes a current one, a slower route never wins on freshness alone, and a challenger inside the confidence interval never flips the route on measurement noise.
    public static Option<BenchmarkRow> Select(Seq<BenchmarkRow> rows, string @case, string hostFingerprint, RouteWeight weight, Instant now, Duration horizon = default) =>
        Admitted(rows.Filter(row => StringComparer.Ordinal.Equals(row.Case, @case)), hostFingerprint, now, horizon)
            .Fold(HashMap<string, BenchmarkRow>(), static (freshest, row) => freshest.AddOrUpdate(row.Route, prior => prior.At >= row.At ? prior : row, row))
            .Values.Fold(
                Option<BenchmarkRow>.None,
                (held, row) => held.Match(prior => weight.Beats(row, prior) ? Some(row) : held, () => Some(row)));

    public static Option<string> WinnerOf(Seq<BenchmarkRow> rows, string @case, string hostFingerprint, RouteWeight weight, Instant now, Duration horizon = default) =>
        Select(rows, @case, hostFingerprint, weight, now, horizon).Map(static row => row.Route);
}
```
