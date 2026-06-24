# [PERSISTENCE_CACHE_INDEXES]

Rasm.Persistence contributes exactly one registration row to the suite cache port and owns three durable index contracts over it. `CacheContribution` is the sole cache capsule: it is the L2 `IBufferDistributedCache` over the key-value lane and the `IHybridCacheSerializerFactory` riding the MessagePackBinary codec row, and it selects its L2 residence over the closed `CacheResidence` tier axis whose redis arm carries the optional `RedisFabric` live-coordination delegate binding the at-least-once `XREADGROUP` invalidation cursor of record plus the best-effort RESP3/keyevent push hints that race it. The three indexes — `ModelResultKey` (model-result and projection reuse), `ArtifactIndexRow` (content-addressed blob catalog with the chunk-dedup pre-filter), and `BenchmarkRow` (fingerprint-gated route evidence) — all key on one content-address identity, carry one `ArtifactClass` retention reference, and emit one `CacheIndexFact` evidence stream. Cache mechanics stay at the AppHost port where `CacheLane` and `CacheSurface` own stampede, tags, and entry options; the owned surfaces here are key-shape value objects, catalog rows, the residence registration axis, the closed `CacheFault` family, and the one fact stream over `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `StackExchange.Redis`, `MessagePack`, `System.IO.Hashing`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, and `NodaTime`, stamped through `ClockPolicy` and emitted through `ReceiptSinkPort`.

## [01]-[INDEX]

- [01]-[L2_CONTRIBUTION]: one registration row for the L2 store, the residence tier axis, the live-fabric delegate (the at-least-once stream invalidation cursor plus the best-effort push hints), the serializer factory, and the closed cache-fault family.
- [02]-[MODEL_RESULT_INDEX]: the `ModelResultKey` cross-process result identity covering model-result and projection-lane reuse, read-through entry, fabric-driven invalidation, and one fact stream.
- [03]-[ARTIFACT_BLOB_INDEX]: content-addressed catalog rows for warm-start and profiling artifacts, the cross-projection `SourceKey` join, and the chunk-dedup `ShortTag` membership pre-filter.
- [04]-[BENCHMARK_INDEX]: persisted BenchmarkDotNet rows with latency, GC, and throughput profiles; fingerprint-gated claim toward route selection.

## [02]-[L2_CONTRIBUTION]

The contribution is one storage row and one codec, never a second cache owner. AppHost owns the port, the stampede policy, the tag vocabulary, and the entry options at `Runtime/resources#CACHE_PORT`; this capsule reads and writes the bytes and the residence axis selects which `IDistributedCache` it binds. The redis arm refines L1 coherence from TTL-bounded to invalidation-driven where a multiplexer is present — the at-least-once `XREADGROUP` consumer-group cursor is the coherence guarantee that survives a disconnect, the RESP3/keyevent push only races it to a faster hint — and is bit-identical to the TTL baseline where it is absent; the live fabric is a `Register` delegate row carrying behavior, never a flag-branch in the capsule body.

- Owner: `CacheContribution` — one sealed boundary capsule implementing both `IBufferDistributedCache` and `IHybridCacheSerializerFactory`, the package's single cache registration row; `CacheResidence` — the registration-row residence axis carrying each tier's `IDistributedCache` registration and, for the redis arm, the `RedisFabric` delegate row binding the RESP3 server-assisted client-side-caching invalidation push, the best-effort keyevent-notification hint, the at-least-once `XREADGROUP` consumer-group invalidation cursor, and the atomic Lua single-flight/lease surface; `RedisFabric` — the live-coordination delegate carrier whose `Inert` value is the Redis-absent identity; `CacheFault` — the closed family deriving from `Expected, IValidationError<CacheFault>` over the live-fabric arm, drain, lease, and replay-gap causes.
- Cases: `CacheResidence` in-memory | sqlite | redis | distributed-pg; `CacheFault` FabricUnarmed | SubscriptionLost | LeaseRejected | ReplayGap | Uncategorized.
- Entry: `bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` — the BCL factory contract shape at the boundary; every other member is the L2 store contract; `static Func<IServiceCollection, IServiceCollection> Register(CacheResidence, string endpoint, string instance, Func<IServiceProvider, IDistributedCache> pgBacked, Option<RedisFabric> fabric)` selects the residence registration row, the redis arm composing the optional `RedisFabric` so an absent multiplexer leaves the path inert.
- Auto: `TryCreateSerializer<T>` yields the MessagePack codec for every cache payload type with zero per-type registration, the `MessagePackSerializerOptions` value arriving settled from the MessagePackBinary codec row; the `DefaultHybridCache` runtime sniffs the buffer contract at registration and routes reads through `TryGetAsync(IBufferWriter<byte>)` into its pooled buffer writer and writes through the `ReadOnlySequence<byte>` form, so payload bytes move with zero intermediate arrays; `CacheResidence.Redis` registers `RedisCache : IBufferDistributedCache` through `AddStackExchangeRedisCache` so the zero-copy path holds across the redis tier, and where a `RedisFabric` is bound the same multiplexer arms `RedisProtocol.Resp3`, creates the `XGROUP` invalidation consumer group, subscribes the `__redis__:invalidate` broadcast push and the `__keyevent@*__:*` keyevent stream as best-effort low-latency hints that drop an L1 entry on the source key change ahead of TTL, and prepares the atomic `ScriptEvaluate` single-flight and writer-lease scripts; the durable invalidation feed is the `XREADGROUP` consumer-group cursor — `Replay` drains the `__cache:invalidate` stream from the committed group cursor with idle-claim takeover, folds each `StreamEntry` into a `CacheIndexFact.Invalidate`, and `Commit` `XACK`s the drained ids so a disconnected consumer replays every missed invalidation on reconnect rather than silently serving stale L1 entries, the lossy push only racing the cursor to a faster hint; `Observe` projects each pushed `ChannelMessage` into the same `CacheIndexFact.Invalidate` over the queue's own `IAsyncEnumerable<ChannelMessage>` so both the hint lane and the cursor lane are observable and back-pressure-safe, never an opaque side effect or an unbounded accumulator.
- Packages: `Microsoft.Extensions.Caching.Hybrid`; `Microsoft.Extensions.Caching.StackExchangeRedis`; `StackExchange.Redis`; `MessagePack`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project); BCL inbox.
- Growth: a new L2 residence is one `CacheResidence` case carrying its `IDistributedCache` registration row; a new live-fabric coordination — a stream consumer group, a presence keyspace lane, a sharded `SPUBLISH` push — is one delegate slot on `RedisFabric`; a new fabric failure cause is one `CacheFault` case in the closed family; a new residence-aware fact is one kind constant on `CacheIndexFact`; zero new surface — a second live-coordination owner, a fabric capsule per provider, or a bare `Error.New` cache fault is the deleted form.
- Boundary: the BCL cache contracts own the member shapes and the capsule body carries language-owned statement forms; the hybrid runtime confines every L2 touch to `GetAsync`, `TryGetAsync`, `SetAsync`, and `RemoveAsync`, so the synchronous members exist for contract totality and bridge by blocking; the `read` delegate serves the array contract and `readInto` the buffer contract — one storage row behind both; absolute expiry is the only L2 lifetime and `Refresh` is structurally inert; `CacheResidence` is a registration-row axis selecting which `IDistributedCache` the one capsule reads and writes — `InMemory` registers the BCL memory distributed cache, `Sqlite` the embedded key-value L2 (the default), `Redis` registers `StackExchange.Redis` `ConnectionMultiplexer.Connect`/`GetDatabase` behind `RedisCache` through `AddStackExchangeRedisCache` with `RedisCacheOptions.{ConfigurationOptions, InstanceName, ConnectionMultiplexerFactory, ProfilingSession}`, and `DistributedPg` the pg-backed row; each tier keys on the one content-address identity so an L1 miss promotes from the L2 tier without a re-mint; tag invalidation rides the existing op-log changefeed cursor as the baseline, and where the live fabric is present the at-least-once `XREADGROUP` consumer-group cursor over the `__cache:invalidate` stream is the redis backplane of record — a committed group cursor replays every missed invalidation on reconnect — while the RESP3 `__redis__:invalidate` push and the `__keyevent@*__:*` keyevent stream are best-effort low-latency hints that race the cursor to drop an L1 entry ahead of its TTL, never the coherence guarantee themselves (`.api/api-redis.md` `[KEYSPACE_NOTIFICATION]`: a disconnected subscriber misses pushes, so the push REFINES the stream cursor and never replaces it); both lanes feed the op-log HLC-cursor peer-replay rather than replacing it, so a Redis-absent profile is bit-identical to the TTL-bounded baseline (page invalidation row [5] deliberately lacks a backplane and the fabric is strictly additive over it); the RESP3 server-assisted client-side caching arms through `CLIENT TRACKING ON BCAST` over the raw `IDatabase.Execute` escape hatch where no typed member exists (`.api/api-redis.md` `[RESP3_CLIENT_SIDE_CACHING]`), the keyevent notification arms through the typed `IServer.ConfigSet("notify-keyspace-events", "KEA")` and drains the `ChannelMessageQueue` by `await foreach` over its `IAsyncEnumerable<ChannelMessage>` self, the durable cursor arms through `StreamCreateConsumerGroupAsync`/`StreamReadGroupAsync`/`StreamAcknowledgeAsync` with `claimMinIdleTime` idle-claim takeover, and the stampede single-flight plus the cross-process writer-lease fence ride one atomic `LoadedLuaScript.EvaluateAsync` rather than a managed compare-loop; the Lua scripts, the keyevent subscription, the consumer group, and the tracking arming are connection-instance state on the multiplexer, never persisted, so a process restart re-arms them and resumes the durable cursor from its committed `XACK` position with no durable residue; AppHost still owns the cache port, stampede policy, and tag vocabulary (`AppHost/runtime#cache-pool-lanes`), so the fabric deepens the one contribution row and never reverses the dependency; the capsule deletes a Persistence-owned cache system, a hand-rolled fake cache type, a residence-per-provider cache class, a managed compare-loop single-flight, a lossy-push-only invalidation that silently strands stale L1 entries, an untyped fabric error, and every second cache owner in the suite.

```csharp signature
public sealed class CacheContribution(
    MessagePackSerializerOptions wire,
    Func<string, CancellationToken, ValueTask<byte[]?>> read,
    Func<string, IBufferWriter<byte>, CancellationToken, ValueTask<bool>> readInto,
    Func<string, ReadOnlySequence<byte>, Instant, CancellationToken, ValueTask> write,
    Func<string, CancellationToken, ValueTask> evict,
    ClockPolicy clocks) : IBufferDistributedCache, IHybridCacheSerializerFactory {
    public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => read(key, token).AsTask();

    public bool TryGet(string key, IBufferWriter<byte> destination) => TryGetAsync(key, destination).GetAwaiter().GetResult();

    public ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) => readInto(key, destination, token);

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
public sealed class CacheResidenceKeyPolicy : IEqualityComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CacheResidenceKeyPolicy, string>]
public sealed partial class CacheResidence {
    public static readonly CacheResidence InMemory = new("in-memory");
    public static readonly CacheResidence Sqlite = new("sqlite");
    public static readonly CacheResidence Redis = new("redis");
    public static readonly CacheResidence DistributedPg = new("distributed-pg");

    public static Func<IServiceCollection, IServiceCollection> Register(
        CacheResidence residence, string endpoint, string instance,
        Func<IServiceProvider, IDistributedCache> pgBacked, Option<RedisFabric> fabric) =>
        residence.Switch(
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

    // Pending-list (`0`) drain replays this consumer's un-`XACK`'d entries on reconnect before `>` serves live messages: the at-least-once guarantee a disconnect cannot strand.
    public async IAsyncEnumerable<CacheIndexFact> Replay(IDatabase database, string consumer, [EnumeratorCancellation] CancellationToken token = default) {
        var env = EnvIO.New(token: token);
        var from = StreamPosition.Beginning;
        while (!token.IsCancellationRequested) {
            var batch = await StreamCursor(database, (consumer, from)).RunAsync(env).ConfigureAwait(false);
            from = batch is { Length: 0 } && from == StreamPosition.Beginning ? StreamPosition.NewMessages : from;
            foreach (var entry in batch) {
                yield return new CacheIndexFact(InvalidateStream, CacheIndexFact.Invalidate, entry["key"].ToString(), 0L, CacheResidence.Redis.Key);
            }
            if (batch is { Length: > 0 }) {
                _ = await StreamAck(database, [.. batch.Select(static e => e.Id)]).RunAsync(env).ConfigureAwait(false);
            }
        }
    }

    public static async IAsyncEnumerable<CacheIndexFact> Observe(ChannelMessageQueue queue, string lane, [EnumeratorCancellation] CancellationToken token = default) {
        await foreach (var message in queue.WithCancellation(token).ConfigureAwait(false)) {
            yield return new CacheIndexFact(lane, CacheIndexFact.Invalidate, message.Message.ToString(), 0L, CacheResidence.Redis.Key);
        }
    }
}
```

| [INDEX] | [LAW]         | [RULING]                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | ownership     | port, stampede protection, tag vocabulary, and entry options stay at the AppHost cache port; the contribution is storage and codec only                                                                                                                                                                                                                                                                                                                                                                                                                             |
|  [02]   | payload       | every L2 payload lands as one key-value row whose codec-id and content-hash columns are settled row law — codec-tagged bytes through `IHybridCacheSerializer<T>`, never bare blobs                                                                                                                                                                                                                                                                                                                                                                                   |
|  [03]   | expiry        | only the write's absolute relative expiry crosses into `ExpiresAt`, stamped through `ClockPolicy`; an absent value traces to the `DeadlineClass.CacheTtl` deadline row                                                                                                                                                                                                                                                                                                                                                                                               |
|  [04]   | sweep         | expired rows leave on the persistence-maintenance `ScheduleEntry` row under the maintenance lease; each sweep deletion emits one `Evict` fact                                                                                                                                                                                                                                                                                                                                                                                                                       |
|  [05]   | invalidation  | `RemoveByTagAsync` is logical; peers replay entity-kind transitions from the op-log HLC cursor and L1 staleness stays TTL-bounded with no backplane as the baseline; where the `RedisFabric` is bound the at-least-once `XREADGROUP` consumer-group cursor over `__cache:invalidate` is the redis backplane of record that collapses L1 staleness from TTL-bounded to invalidation-driven and replays every missed invalidation on reconnect, while the RESP3 `__redis__:invalidate` push and the keyevent stream are best-effort low-latency hints that race the cursor, never the guarantee — both feed the op-log HLC cursor, refining the replay path, never replacing it, so a Redis-absent profile is bit-identical; each cursor entry, push, or key-transition folds through `Replay`/`Observe` into an `Invalidate` fact and the drained ids `XACK` the group cursor |
|  [06]   | residence     | `CacheResidence.Sqlite` is the default L2; each tier is one `Register` row binding the same `IDistributedCache` slot, content-address-keyed so an L1 miss promotes from any tier without a re-mint; `Redis` rides `AddStackExchangeRedisCache` + `RedisCache : IBufferDistributedCache` and arms `RedisProtocol.Resp3` plus the optional `RedisFabric`, `DistributedPg` the pg-backed row; zero change to any owner declared here                                                                                                                                          |
|  [07]   | tier evidence | every `CacheIndexFact` carries a `Residence` tag and an L1-miss-promotes-from-L2 transition emits the `L2Promote` kind, so the tiered-cache fabric's tier transition is observable; a durable `XREADGROUP` cursor entry, a RESP3 invalidation push, or a keyevent key-transition emits the `Invalidate` kind through `Replay`/`Observe`; Redis `ProfilingSession` rides the existing telemetry contribution                                                                                                                                                                                                              |
|  [08]   | single-flight | the stampede single-flight and the cross-process writer-lease fence ride one atomic `LoadedLuaScript.EvaluateAsync` (`RedisFabric.SingleFlightLease`) rather than a managed compare-loop, the lease keyed under the `DeadlineClass.CacheTtl` allotment, a non-grant routing to `CacheFault.LeaseRejected`; absent the fabric the AppHost-owned managed stampede policy is the unchanged baseline                                                                                                                                                                          |
|  [09]   | fabric fault  | the live-fabric arm, the keyevent drain, the durable cursor replay, and the lease eval fold their causes into the closed `CacheFault` family deriving from `Expected, IValidationError<CacheFault>` — `FabricUnarmed`, `SubscriptionLost`, `LeaseRejected`, `ReplayGap`, `Uncategorized` in the 8290 band; a bare `Error.New` over a multi-cause domain is the deleted form                                                                                                                                                                                                                                                            |

## [03]-[MODEL_RESULT_INDEX]

`ModelResultKey` is the deterministic cross-process result-reuse identity Compute composes — `Compute/Model/inference#RESULT_CACHE` constructs it as `(model checksum, input hash, ep key)` and reads `ModelChecksum`, so the name, the positional shape, and `ModelChecksum` are a cross-package contract this owner holds frozen. The `Query/federation#FUSION_RANK` rule result, the `Query/federation#ENTITY_GRAPH` selection result, and the projection-lane read all reuse through this one key, and `ModelResultKey.RecencyHorizon` is the suite's sole cross-process result-reuse recency horizon every reuse surface — Compute's `LinearProvider.Select` (`Compute/Tensor/blas`), the benchmark claim gate, and the projection lane — traces to by reference.

- Owner: `ModelResultKey` — the zero-alloc `readonly record struct` deterministic result identity (`ModelChecksum`, `InputHash`, `EpKey`); `CacheIndexFact`, `IndexSurface` — the one cache-evidence fact stream and the read-through dispatch shared by all three indexes; `ModelResultKey.RecencyHorizon` — the one owned recency bound.
- Cases: `Hit`, `Miss`, `Evict`, `L2Promote`, `Invalidate` kind rows on the fact stream — `Invalidate` stamps a RESP3 `__redis__:invalidate` push or a keyspace-notification key-transition where the live fabric is present, `L2Promote` an L1 miss served from the L2 tier without a re-mint.
- Entry: `ValueTask<T> Result<T, TState>(ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CacheLane? lane = null, CancellationToken token = default)` on `HybridCache` — the cache port's carrier over the model-result and projection lanes, the optional `lane` selecting the L2 backing and defaulting to `CacheLane.ModelResult`; `Fact` rides `IO<ReceiptEnvelope>` for the sink effect; `Of` derives input identity via `XxHash128.HashToUInt128` so no caller hashes by hand.
- Auto: `Result` composes the port read — lane scoping, stampede single-flight, the lane tag, and the model-checksum tag all ride one `CacheSurface.Read` call with zero call-site ceremony; a model-inference reuse passes `CacheLane.ModelResult` and a federated-projection reuse passes `CacheLane.Projection` from the one entrypoint, so the projection lane gains a reuse spelling without a second key shape; `Of` folds the `ReadOnlySpan<byte>` input through `XxHash128.HashToUInt128` matching the `Query/federation#ELEMENT_SET_ALGEBRA` set receipt and the `Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, so a federated selection and its cached result share one address.
- Receipt: `CacheIndexFact` — miss on produce invocation, hit on completion without produce, evict from the sweep, l2-promote on an L1 miss served from L2 without a re-mint, invalidate from a fabric push folded through `Observe` — each carrying the `Residence` tier tag, HLC-stamped through `ReceiptSinkPort` and consumed by compute-side substrate selection.
- Packages: `System.IO.Hashing`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project); BCL inbox.
- Growth: a new reuse lane is one `CacheLane` argument on the one `Result` entrypoint; a new evidence kind is one kind row on `CacheIndexFact`; a new result family is one tag value on the same key shape; zero new surface — a parallel projection-key record, a per-lane result-reuse entrypoint, or a floating per-call horizon parameter is the deleted form.
- Boundary: keys cross as canonical ordinal strings through `ISpanFormattable` and `IUtf8SpanFormattable`; `ModelChecksum` and `EpKey` arrive as opaque wire strings so no compute vocabulary leaks into the key shape, and the positional `(ModelChecksum, InputHash, EpKey)` order plus the `ModelChecksum` accessor stay frozen because `Compute/Model/inference#RESULT_CACHE` `CacheOps.Key`/`Validated` construct and read them by that exact shape; the projection-lane reuse rides the same key through the `lane` argument rather than a second key type, because the `CacheLane.Projection` L2 backing (`AppHost/Runtime/resources#CACHE_PORT`) differs only in store routing, never in identity; eviction eligibility is retention-axis law and the index contributes only its `ArtifactClass.KvRow` reference; the fact record lands as one `[JsonSerializable]` row on the `Version/snapshots#CODEC_AXIS` `PersistenceWireContext`; the cluster deletes per-index repository classes, a parallel projection-key record, and a generic receipt abstraction over hit/miss evidence; `RecencyHorizon` is the one owned recency bound — the benchmark claim gate and the compute-side result-reuse and route-selection consumers reference this value rather than minting a second `Duration horizon`, so a stale cross-process reuse is impossible by construction and a floating per-call horizon parameter is the deleted form.

```csharp signature
public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputHash, string EpKey) : ISpanFormattable, IUtf8SpanFormattable {
    public static readonly Duration RecencyHorizon = Duration.FromHours(24);

    public static ModelResultKey Of(string modelChecksum, ReadOnlySpan<byte> input, string epKey) =>
        new(modelChecksum, XxHash128.HashToUInt128(input), epKey);

    public static ModelResultKey OfSelection(UInt128 setReceipt) => new("", setReceipt, "");

    public bool IsContentAddressed => ModelChecksum.Length == 0 && EpKey.Length == 0;

    public override string ToString() => IsContentAddressed ? $"{InputHash:x32}" : $"{ModelChecksum}:{InputHash:x32}:{EpKey}";

    public string ToString(string? format, IFormatProvider? provider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        IsContentAddressed
            ? destination.TryWrite($"{InputHash:x32}", out charsWritten)
            : destination.TryWrite($"{ModelChecksum}:{InputHash:x32}:{EpKey}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        IsContentAddressed
            ? Utf8.TryWrite(utf8Destination, $"{InputHash:x32}", out bytesWritten)
            : Utf8.TryWrite(utf8Destination, $"{ModelChecksum}:{InputHash:x32}:{EpKey}", out bytesWritten);
}

public readonly record struct CacheIndexFact(string Lane, string Kind, string Key, long Bytes, string Residence) {
    public const string Hit = "cache-hit";
    public const string Miss = "cache-miss";
    public const string Evict = "cache-evict";
    public const string L2Promote = "cache-l2-promote";
    public const string Invalidate = "cache-invalidate";
}

public static class IndexSurface {
    extension(HybridCache cache) {
        public ValueTask<T> Result<T, TState>(ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CacheLane? lane = null, CancellationToken token = default) =>
            cache.Read(lane ?? CacheLane.ModelResult, key.ToString(), state, produce, Some(Seq(key.ModelChecksum)), token);
    }

    extension(ReceiptSinkPort sink) {
        public IO<ReceiptEnvelope> Fact(CorrelationId correlation, CacheIndexFact fact) =>
            sink.Send(correlation, TenantContext.Current, "Rasm.Persistence", fact.Kind, JsonSerializer.SerializeToElement(fact, PersistenceWireContext.Default.CacheIndexFact));
    }
}
```

## [04]-[ARTIFACT_BLOB_INDEX]

Every blob is one content-addressed catalog row carrying two keys — a per-row `Path` lookup address and a cross-projection `UInt128 SourceKey` join — plus the `ShortTag` chunk-dedup pre-filter and the typed retention reference. The `Project` fold joins every projection of one source artifact by `SourceKey`; the `Holds`/`MayHold` membership projections serve the `Version/snapshots#CONTENT_CHUNKING` `ContentChunker.Novel` dedup probe so a re-store transfers only the chunks the index lacks. Compute owns each content-key derivation and this owner owns the blob residence, neither re-declaring the other.

- Owner: `ArtifactIndexRow` — content-addressed catalog row for execution-provider warm-start contexts, ONNX profiling traces, IFC semantic-ingest model graphs, content-defined chunks, Compute interchange artifacts (tessellated GLB, chunked field, tile content, re-exported glTF), and graduated offline-science ONNX surrogate assets on the blob lane; the static `Project` fold is the source-artifact join, `Novel` the chunk-dedup pre-filter projection over the `ShortTag` and `ContentHash` columns.
- Cases: `EpContext`, `OnnxProfile`, `IfcSemantic`, `ChunkContent`, `Interchange`, `GraduationAsset` kind rows; the `SourceKey` column is the source-artifact content key (the Compute `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` of the source IFC) that the `IfcSemantic` graph projection and the `Interchange` GLB projection both carry, so the two rows join on one source identity while each keeps its own payload `ContentHash`.
- Entry: `static ArtifactIndexRow Admit(string kind, string path, ReadOnlySpan<byte> payload, DataClassification classification, ArtifactClass retention, Instant at, Option<UInt128> sourceKey = default, Option<UInt128> providerFingerprint = default)` — pure value; the payload `ContentHash` derives from the bytes through `XxHash128.HashToUInt128`, the `ShortTag` from the same bytes through `XxHash3.HashToUInt64` (the `Version/snapshots#COMPRESSION_HASHING` `HashPolicy.Content` 64-bit pre-filter tag), the `SourceKey` carries the joining source-artifact identity, never the caller's path; an absent `sourceKey` self-keys (`SourceKey := ContentHash`) so a single-projection artifact forms a degenerate one-member group, while a multi-projection family passes `Some(sourceIfcKey)`; `providerFingerprint` carries the `GraduationAsset` `(checksum, OrtEpDevice)` digest the `Version/provenance#ATTESTED_LEDGER` chains, folding `UInt128.Zero` for every non-graduation kind.
- Auto: `Admit` stamps the payload content hash, the chunk short tag, the joining source key, byte length, classification, retention class, instant, and the optional provider fingerprint in one call; reads ride the `CacheLane.ArtifactBlob` lane through the port with the same stampede and tag law as every cache read; the row carries two distinct keys — the string `Path` (the per-row lookup address) and the `UInt128 SourceKey` (the cross-projection join) — and the Bim tessellation cache and wire snapshot read each by the matching key; `Project` resolves every projection of one source artifact by its `SourceKey`, and `Novel` projects the chunks a peer or the index lacks by probing the cheap 64-bit `ShortTag` membership ahead of the authoritative 128-bit `ContentHash` compare so a tag-miss skips the full lookup on a hot re-store path.
- Receipt: a graduated surrogate re-admission re-verifies its `providerFingerprint` against the prior row and routes a divergence to the `Version/provenance#ATTESTED_LEDGER` fingerprint check before inference; a chunked store rides the snapshot owner's `store.chunk.split`/`store.chunk.dedup` receipts over the `Novel` projection.
- Packages: `System.IO.Hashing`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `NodaTime`; `Rasm.AppHost` (project).
- Growth: a new artifact family is one kind row on `ArtifactIndexRow`; a content-defined chunk lands as one `ChunkContent` kind row keyed by its `XxHash128` chunk address with its `XxHash3` `ShortTag` (`Version/snapshots#CONTENT_CHUNKING`) so an identical chunk across snapshots and peers dedups on the one content key through the pre-filter, never a second chunk store; a graduated offline-science surrogate lands as one `GraduationAsset` kind row content-addressed identically over the post-`ModelAssetManifest`-validated ONNX bytes carrying its provider fingerprint, so a re-graduated model dedups on its content key and a sibling graduation-artifact record is the deleted form; zero new surface.
- Boundary: every artifact lands on the blob lane under its `ContentHash` and receipts name index rows by path — loose temp files are the deleted form; `Classification` enters typed at admission as a `DataClassification` and the unclassified-write rejection is store-side column law through `Version/retention#CLASSIFICATION_ENFORCEMENT` `ClassificationGuard.Admit`; index rows carry their `ArtifactClass` retention reference (the typed `BlobIndex` row, never a free string) while eligibility folds belong to the retention axis; the `SourceKey` join is the durable multi-projection home — the `IfcSemantic` graph row, the `Interchange` GLB row, and the wire-snapshot row of one source IFC carry the same source-artifact `SourceKey` (the Compute `InterchangeIdentity`, equal to the Bim `IfcContentKey`) so a content-addressed source artifact is one source identity with several payload projections joined by `Project`, each projection landing under its own string `Path`; the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` `TessellationRequest.Resolve` cache leg looks up the prior GLB by its string lookup address (the Bim `ArtifactKey` string `$"{IfcContentKey:x32}:glb"`, the GLB row's `Path`) so a re-tessellation of an unchanged IFC reads the prior GLB by reference, and that GLB row's `SourceKey` is the value `Project` joins on; the `Exchange/wire#WIRE_PROJECTION` `BimWire` snapshot row admits under its own `$"{ContentKey:x32}:bim-wire"` address while carrying the same source-IFC `SourceKey`, and the `Exchange/import#IMPORT_RAIL` incremental-delta reimport resolves the prior `IfcSemantic` graph by `Project` over that source key so a re-import diffs against the prior graph and re-fetches only changed `BimWire` element rows; the Bim side mints only the string `ArtifactKey` address and the `IfcContentKey` it folds and holds no Persistence reference — neither `ArtifactIndexRow`, the `SourceKey` column, nor `Project` crosses into Bim, the artifact index being the Persistence owner's concern the app-platform caller reads at the seam; the `IfcSemantic` kind admits the Compute interchange IFC model graph — the `DatabaseIfc`-extracted property sets, spatial hierarchy, quantities, materials, and type objects serialized to the canonical bytes the `Bim/Exchange/import#IMPORT_RAIL` produces — content-addressed identically through `XxHash128.HashToUInt128`, so the same model graph re-ingested under the same tolerance dedupes on its content key, and the admission carries the model graph only, never tessellated BRep geometry: the two-hop tessellation rail (`Ifc -> IfcOpenShell -> GLB`) is a Compute companion concern whose GLB rides the `Interchange` kind that `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Admit` content-addresses, so this kind owns semantic residence and the geometry residence stays a distinct `Interchange` blob row; the `Interchange` kind admits every Compute interchange artifact — the tessellated GLB, the chunked field artifact, the 3D-Tiles leaf content, and the re-exported glTF — each content-addressed through `XxHash128.HashToUInt128` over its canonical bytes under one identity scheme, so Compute owns the content-key derivation and this owner owns the blob residence; the model-graph residence projection onto the document and search lanes is owned at `Query/lanes#DOCUMENT_LANE` and `Query/lanes#SEARCH_LANES` and consumed here as the index identity only; the `GraduationAsset` kind admits the `ONE_GRADUATION_EVIDENCE` `HandoffAxis` surrogate — the content-keyed ONNX artifact a graduated Python offline result produces (post-`ModelAssetManifest` validation) — content-addressed identically through `XxHash128.HashToUInt128`, so the C# side runs inference over a durable content key the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` chains and the `AppHost/Runtime/determinism#EVENT_LOG` determinism closure references, never an in-process training loop; the asset's `providerFingerprint` is the same `(checksum, OrtEpDevice)` digest the attested ledger folds into its chain, so a re-import re-verifies it at admission and a provider-divergent surrogate fit is caught before inference; the asset rides the existing object-store residence and the `Version/retention.md#RETENTION_SWEEPS` `ClosureGc` reachability sweep so a stale surrogate is collected by the one GC, and Python (`compute/graduation#GRADUATION`) holds the rail singular and this owner adds only the durable blob residence; the `ShortTag` column is the `Version/snapshots#CONTENT_CHUNKING` dedup pre-filter face only — `Novel` probes `MayHold(shortTag)` before `Holds(contentHash)` so a tag-miss proves a chunk novel without the full content-key compare and a `ShortTag` false positive only costs one fall-through `Holds`, never a wrong dedup — so the 128-bit `ContentHash` stays the authoritative dedup identity while the 64-bit tag culls the lookup.

```csharp signature
public readonly record struct ArtifactIndexRow(
    UInt128 ContentHash,
    ulong ShortTag,
    UInt128 SourceKey,
    string Kind,
    string Path,
    long Bytes,
    DataClassification Classification,
    ArtifactClass Retention,
    Instant At,
    UInt128 ProviderFingerprint) {
    public const string EpContext = "ep-context";
    public const string OnnxProfile = "onnx-profile";
    public const string IfcSemantic = "ifc-semantic";
    public const string ChunkContent = "chunk-content";
    public const string Interchange = "interchange";
    public const string GraduationAsset = "graduation-asset";

    public static ArtifactIndexRow Admit(
        string kind, string path, ReadOnlySpan<byte> payload, DataClassification classification,
        ArtifactClass retention, Instant at, Option<UInt128> sourceKey = default, Option<UInt128> providerFingerprint = default) {
        var contentHash = XxHash128.HashToUInt128(payload);
        return new(contentHash, XxHash3.HashToUInt64(payload), sourceKey.IfNone(contentHash), kind, path,
            payload.Length, classification, retention, at, providerFingerprint.IfNone(UInt128.Zero));
    }

    public static Seq<ArtifactIndexRow> Project(Seq<ArtifactIndexRow> rows, UInt128 sourceKey) =>
        rows.Filter(row => row.SourceKey == sourceKey);

    public static Seq<ArtifactIndexRow> Novel(Seq<ArtifactIndexRow> candidates, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        candidates.Filter(row => !mayHold(row.ShortTag) || !holds(row.ContentHash));
}
```

## [05]-[BENCHMARK_INDEX]

Persisted BenchmarkDotNet evidence with the fingerprint-match, recency-bounded claim gate toward route selection. One row carries the full statistical, allocation, and throughput profile a route decision reads — `LatencyProfile` folds the percentile and dispersion summary, `GcProfile` folds the per-generation collection counts and allocated bytes — so a stale or host-foreign benchmark never selects a route. The contract is read-only toward route selection; the producer mints the rows on the bulk lane under its self-emitted changefeed.

- Owner: `BenchmarkRow` — persisted benchmark evidence with the fingerprint-match, recency-bounded claim gate; `LatencyProfile` — the `[ComplexValueObject]` percentile-and-dispersion summary (`Mean`, `Median`, `StdDev`, `P50`, `P90`, `P95`, `P99`); `GcProfile` — the `[ComplexValueObject]` allocation summary (`Gen0`, `Gen1`, `Gen2`, `AllocatedBytes`).
- Entry: `static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default)` — `Option` carries fingerprint admission gated by the staleness horizon defaulting to the `ModelResultKey.RecencyHorizon` owned bound; `None` is the fall-through signal toward the caller's static cost rank; `RouteOf` projects the same claim to its route string.
- Auto: ingest rides the bulk lane under its self-emitted changefeed and tag-transition law; every row carries the `ArtifactClass.BenchmarkRow` reference so the `CountBound 1024` retention sweep folds over a traceable class; trend history exports to the analytical lane as parquet with zero second pipeline; the latency and GC profiles arrive composed at admission so a dashboard cut reads the dispersion and per-generation collection counts without a second join.
- Packages: `NodaTime`; `Thinktecture.Runtime.Extensions`; `LanguageExt.Core`; `linq2db.EntityFrameworkCore`; `DuckDB.NET.Data.Full`.
- Growth: a new benchmark dimension is one column on `BenchmarkRow` or one member on the `LatencyProfile`/`GcProfile` value object; a new dashboard cut is one analytical export row; zero new surface.
- Boundary: the contract is read-only toward route selection — `Claim` admits only rows whose `HostFingerprint` matches the calling host with ordinal equality and whose `At` stamp falls inside the staleness `horizon` measured from `now`, then folds the matching set to the most-recent `At` so a stale benchmark never selects a route, while the companion `RouteOf` projects that same claim to its `Route` string so one horizon-bounded recency fold serves both; the `horizon` argument defaults to the `ModelResultKey.RecencyHorizon` owned bound so this index consumes the one suite recency horizon rather than minting a second `Duration`, and a caller may narrow it but never re-owns it; the recency fold replaces the first-ordinal `Find`, an `OrderByDescending` LINQ chain, and a counter loop; the latency summary is a `LatencyProfile` `[ComplexValueObject]` carrying the BenchmarkDotNet `Mean`/`Median`/`StdDev` central tendency and the `P50`/`P90`/`P95`/`P99` percentile band so a route decision reads the tail latency, not the median alone, and the allocation summary is a `GcProfile` carrying the `Gen0`/`Gen1`/`Gen2` per-generation collection counts and the `AllocatedBytes` total so an allocation-sensitive route weighs GC pressure; `ThroughputOps` carries the operations-per-second the workload sustained and `WorkloadN` the operation count the measurement folded; the fingerprint value arrives as an opaque string minted by the benchmark producer over the runtime, JIT, and hardware moniker; the `Retention` column carries the typed `ArtifactClass.BenchmarkRow` reference so the retention sweep keys on the same class the redaction axis bounds; the cluster deletes a benchmarks repository service, a second dashboard export path, a flat percentile-field scatter, and a free-string artifact-class column.

```csharp signature
[ComplexValueObject]
public sealed partial class LatencyProfile {
    public Duration Mean { get; }
    public Duration Median { get; }
    public Duration StdDev { get; }
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
    public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.HostFingerprint, hostFingerprint) && now - row.At <= (horizon == default ? ModelResultKey.RecencyHorizon : horizon))
            .Fold(
                Option<BenchmarkRow>.None,
                static (held, row) => held.Match(prior => row.At > prior.At ? Some(row) : held, () => Some(row)));

    public static Option<string> RouteOf(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        Claim(rows, hostFingerprint, now, horizon).Map(static row => row.Route);
}
```
