# [PERSISTENCE_CACHE_INDEXES]

Rasm.Persistence contributes exactly one registration row to the suite cache port — the `CacheContribution` capsule serving as the L2 `IDistributedCache` over the key-value lane and as the `IHybridCacheSerializerFactory` riding the MessagePackBinary codec row — selects its L2 residence over the closed `CacheResidence` tier axis, and owns three durable index contracts: model-result, artifact-blob, and benchmark. Cache mechanics stay at the AppHost port where `CacheLane` and `CacheSurface` own stampede, tags, and entry options; the owned surfaces here are key-shape records, catalog rows, the residence registration axis, and one cache-evidence fact stream over Microsoft.Extensions.Caching.Hybrid, Microsoft.Extensions.Caching.StackExchangeRedis, StackExchange.Redis, MessagePack, System.IO.Hashing, and NodaTime, stamped through `ClockPolicy` and emitted through `ReceiptSinkPort`.

## [1]-[INDEX]

- [1]-[L2_CONTRIBUTION]: one registration row for the L2 store, residence tier axis, and serializer factory.
- [2]-[MODEL_RESULT_INDEX]: deterministic result identity, read-through entry, and one fact stream.
- [3]-[ARTIFACT_BLOB_INDEX]: content-addressed catalog rows for warm-start and profiling artifacts.
- [4]-[BENCHMARK_INDEX]: persisted benchmark rows; fingerprint-gated claim toward route selection.

## [2]-[L2_CONTRIBUTION]

- Owner: `CacheContribution` — one sealed boundary capsule implementing both `IBufferDistributedCache` and `IHybridCacheSerializerFactory`; the package's single cache registration row; `CacheResidence` — the registration-row residence axis carrying each tier's `IDistributedCache` registration and, for the redis arm, the live-fabric delegate row binding the RESP3 server-assisted client-side-caching invalidation push, the keyspace-notification stream, and the atomic Lua single-flight/lease surface.
- Cases: `CacheResidence` in-memory | sqlite | redis | distributed-pg.
- Entry: `bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` — the BCL factory contract shape at the boundary; every other member is the L2 store contract; `static Func<IServiceCollection, IServiceCollection> Register(CacheResidence, ...)` selects the residence registration row, and the redis arm carries the optional `RedisFabric` live-coordination delegate so an absent multiplexer leaves the path inert and bit-identical to the TTL baseline.
- Auto: `TryCreateSerializer<T>` yields the MessagePack codec for every cache payload type with zero per-type registration; the `MessagePackSerializerOptions` value arrives settled from the MessagePackBinary codec row; the hybrid runtime sniffs the buffer contract at registration and routes reads through `TryGetAsync` into its pooled buffer writer and writes through the `ReadOnlySequence<byte>` form, so payload bytes move with zero intermediate arrays; `CacheResidence.Redis` registers `RedisCache` (which implements `IBufferDistributedCache`) through `AddStackExchangeRedisCache` so the buffer-contract zero-copy path holds across the redis tier too, and where a `RedisFabric` is bound the same multiplexer arms `RedisProtocol.Resp3` and subscribes the `__redis__:invalidate` broadcast push so an L1 entry drops on the source key change rather than at TTL, subscribes the `__keyevent@*__:*` keyspace-notification stream feeding the op-log HLC cursor as the cross-process tag-transition backplane, and prepares the atomic `ScriptEvaluate` single-flight and writer-lease scripts; the live fabric is a `Register` delegate row carrying behavior, never a flag-branch in the capsule body, so a Redis-absent profile never reaches the fabric path.
- Packages: Microsoft.Extensions.Caching.Hybrid; Microsoft.Extensions.Caching.StackExchangeRedis; StackExchange.Redis; MessagePack; NodaTime; Rasm.AppHost (project); BCL inbox.
- Growth: a new L2 residence is one `CacheResidence` case carrying its `IDistributedCache` registration row; a new live-fabric coordination is one `RedisFabric` delegate slot on the redis arm; a new residence-aware fact is one kind constant on `CacheIndexFact`; zero new surface — a second live-coordination owner or a fabric capsule per provider is the deleted form.
- Boundary: boundary capsule — the BCL cache contracts own these member shapes and the capsule body carries language-owned statement forms; the hybrid runtime confines every L2 touch to `GetAsync`, `TryGetAsync`, `SetAsync`, and `RemoveAsync`, so the synchronous members exist for contract totality and bridge by blocking; the `read` delegate serves the array contract and `readInto` serves the buffer contract — one storage row behind both; absolute expiry is the only L2 lifetime and `Refresh` is structurally inert; `CacheResidence` is a registration-row axis selecting which `IDistributedCache` the one `CacheContribution` capsule reads and writes, never a second cache owner — `InMemory` registers the BCL memory distributed cache, `Sqlite` the embedded key-value L2 (the default), `Redis` registers `StackExchange.Redis` `ConnectionMultiplexer.Connect`/`GetDatabase` behind `RedisCache : IDistributedCache, IBufferDistributedCache` through `AddStackExchangeRedisCache` with `RedisCacheOptions.{Configuration, ConfigurationOptions, InstanceName, ConnectionMultiplexerFactory, ProfilingSession}`, and `DistributedPg` the pg-backed `IDistributedCache` row; each tier keys on the one content-address identity so an L1 miss promotes from the L2 tier without a re-mint; tag invalidation rides the existing op-log changefeed cursor as the baseline, and where the live fabric is present the RESP3 `__redis__:invalidate` push and the keyspace-notification stream REFINE that path — feeding a push lane to the op-log HLC-cursor peer-replay rather than replacing it, so a Redis-absent profile is bit-identical to today's TTL-bounded baseline (the page invalidation law row [5] deliberately lacks a backplane and the fabric is strictly additive over it); the RESP3 server-assisted client-side caching arms through `CLIENT TRACKING ON BCAST PREFIX` over the raw `Execute` escape hatch where no typed member exists (`.api/api-redis.md` RESP3 scope), the keyspace notification arms through `IServer.ConfigSet("notify-keyspace-events", "KEA")` and drains the `ChannelMessageQueue` by `await foreach` over its `IAsyncEnumerable<ChannelMessage>` self, and the stampede single-flight plus the cross-process writer-lease fence ride one atomic `LoadedLuaScript` `ScriptEvaluate` rather than a managed compare-loop; the Lua scripts and the keyspace subscription are connection-instance state on the multiplexer, never persisted, so a process restart re-arms them with no durable residue; AppHost still owns the cache port, stampede policy, and tag vocabulary (`AppHost/runtime#cache-pool-lanes`), so the fabric deepens the one contribution row and never reverses the dependency; the capsule deletes a Persistence-owned cache system, a hand-rolled fake cache type, a residence-per-provider cache class, a managed compare-loop single-flight, and every second cache owner in the suite.

```csharp signature
public sealed class CacheContribution(
    ClockPolicy clocks,
    MessagePackSerializerOptions wire,
    Func<string, CancellationToken, ValueTask<byte[]?>> read,
    Func<string, IBufferWriter<byte>, CancellationToken, ValueTask<bool>> readInto,
    Func<string, ReadOnlySequence<byte>, Instant, CancellationToken, ValueTask> write,
    Func<string, CancellationToken, ValueTask> evict) : IBufferDistributedCache, IHybridCacheSerializerFactory {
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

    public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer) => (serializer = new Codec<T>(wire)) is not null;

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
                    Some: live => live.Arm(ConnectionMultiplexer.Connect(Resp3(s.Endpoint))),
                    None: () => RedisFabric.Inert)),
            distributedPg: static s => services => services.AddSingleton<IDistributedCache>(s.PgBacked));

    private static ConfigurationOptions Resp3(string endpoint) {
        var options = ConfigurationOptions.Parse(endpoint);
        options.Protocol = RedisProtocol.Resp3;
        return options;
    }
}

public sealed record RedisFabric(
    Func<IConnectionMultiplexer, IO<ChannelMessageQueue>> ServerAssistedInvalidate,
    Func<IConnectionMultiplexer, IO<ChannelMessageQueue>> KeyspaceStream,
    Func<IDatabase, RedisKey, RedisValue, IO<bool>> SingleFlightLease,
    LoadedLuaScript LeaseScript) {
    public static readonly RedisFabric Inert = new(
        static _ => IO.fail<ChannelMessageQueue>(Error.New("<redis-fabric-absent>")),
        static _ => IO.fail<ChannelMessageQueue>(Error.New("<redis-fabric-absent>")),
        static (_, _, _) => IO.pure(false),
        default!);

    public RedisFabric Arm(IConnectionMultiplexer multiplexer) {
        var server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);
        server.Execute("CONFIG", "SET", "notify-keyspace-events", "KEA");
        var lease = LuaScript.Prepare(
            "if redis.call('GET', @key) == false then redis.call('SET', @key, @owner, 'PX', @ttlMs) return 1 else return 0 end").Load(server);
        return this with {
            LeaseScript = lease,
            ServerAssistedInvalidate = mux => IO.liftAsync(async () => {
                var sub = mux.GetSubscriber();
                ((IDatabase)mux.GetDatabase()).Execute("CLIENT", "TRACKING", "ON", "BCAST");
                return await sub.SubscribeAsync(RedisChannel.Literal("__redis__:invalidate")).ConfigureAwait(false);
            }),
            KeyspaceStream = mux => IO.liftAsync(async () =>
                await mux.GetSubscriber().SubscribeAsync(RedisChannel.Pattern("__keyevent@*__:*")).ConfigureAwait(false)),
            SingleFlightLease = (db, key, owner) => IO.lift(() =>
                (bool)db.ScriptEvaluate(lease, new { key, owner, ttlMs = (long)DeadlineClass.CacheTtl.Allotted.TotalMilliseconds })),
        };
    }
}
```

| [INDEX] | [LAW]          | [RULING]                                                                                                                                                                                                                 |
| :-----: | :------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | ownership      | port, stampede protection, tag vocabulary, and entry options stay at the AppHost cache port; the contribution is storage and codec only                                                                                  |
|   [2]   | payload        | every L2 payload lands as one key-value row whose codec-id and content-hash columns are settled row law — codec-tagged bytes, never bare blobs                                                                           |
|   [3]   | expiry         | only the write's absolute relative expiry crosses into `ExpiresAt`, stamped through `ClockPolicy`; an absent value traces to the `CacheTtl` deadline row                                                                 |
|   [4]   | sweep          | expired rows leave on the persistence-maintenance `ScheduleEntry` row under the maintenance lease; each sweep deletion emits one evict fact                                                                              |
|   [5]   | invalidation   | `RemoveByTagAsync` is logical; the bulk path and peer processes drive tag transitions themselves — peers replay entity-kind transitions from the op-log HLC cursor, and L1 staleness stays TTL-bounded with no backplane as the baseline; where the `RedisFabric` is bound the RESP3 `__redis__:invalidate` broadcast push collapses L1 staleness from TTL-bounded to invalidation-driven and the keyspace-notification stream feeds the op-log HLC cursor a push lane, both REFINING the replay path, never replacing it, so a Redis-absent profile is bit-identical |
|   [6]   | residence      | `CacheResidence.Sqlite` is the default L2; each tier is one `CacheResidence.Register` row binding the same `IDistributedCache` slot, content-address-keyed so an L1 miss promotes from any tier without a re-mint; `Redis` rides `AddStackExchangeRedisCache` + `RedisCache : IBufferDistributedCache` and arms `RedisProtocol.Resp3` plus the optional `RedisFabric` live-coordination row, `DistributedPg` the pg-backed row; zero change to any owner declared here |
|   [7]   | tier evidence  | every `CacheIndexFact` carries a `Residence` tag and an L1-miss-promotes-from-L2 transition emits the `L2Promote` kind, so the tiered-cache fabric's tier transition is observable; a RESP3 invalidation push or a keyspace key-transition emits the `Invalidate` kind; Redis `ProfilingSession` rides the existing telemetry contribution |
|   [8]   | single-flight  | the stampede single-flight and the cross-process writer-lease fence ride one atomic `LoadedLuaScript` `ScriptEvaluate` (`RedisFabric.SingleFlightLease`) rather than a managed compare-loop, the lease keyed under the `DeadlineClass.CacheTtl` allotment; absent the fabric the AppHost-owned managed stampede policy is the unchanged baseline |

## [3]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey`, `CacheIndexFact`, `IndexSurface` — deterministic result identity plus the one cache-evidence fact stream shared by all three indexes; `ModelResultKey.RecencyHorizon` is the suite's sole cross-process result-reuse recency horizon every reuse surface traces to.
- Cases: `Hit`, `Miss`, `Evict`, `L2Promote`, `Invalidate` kind rows on the fact stream — the `Invalidate` kind stamps a RESP3 `__redis__:invalidate` push or a keyspace-notification key-transition where the live fabric is present.
- Entry: `ValueTask<T> Result<T, TState>(ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token = default)` — the cache port's carrier; `Fact` rides `IO<ReceiptEnvelope>` for the sink effect.
- Auto: `Result` composes the port read — lane scoping, stampede single-flight, the lane tag, and the model-checksum tag all ride one call with zero call-site ceremony; `Of` derives input identity via `XxHash128.HashToUInt128` so no caller hashes by hand.
- Receipt: `CacheIndexFact` — miss on produce invocation, hit on completion without produce, evict from the sweep, l2-promote on an L1 miss served from the L2 tier without a re-mint — each carrying the `Residence` tier tag, HLC-stamped through `ReceiptSinkPort` and consumed by compute-side substrate selection.
- Packages: System.IO.Hashing; LanguageExt.Core; NodaTime; Rasm.AppHost (project); BCL inbox.
- Growth: a new evidence kind is one kind row on `CacheIndexFact`; a new result family is one tag value on the same shape; zero new surface.
- Boundary: keys cross as canonical ordinal strings through `ISpanFormattable` and `IUtf8SpanFormattable`; `ModelChecksum` and `EpKey` arrive as opaque wire strings so no compute vocabulary leaks into the key shape; eviction eligibility is retention-axis law and the index contributes only its class value; the fact record lands as one `[JsonSerializable]` row on `PersistenceWireContext`; the cluster deletes per-index repository classes and a generic receipt abstraction over hit/miss evidence; `RecencyHorizon` is the one owned recency bound — the benchmark claim gate and the compute-side result-reuse and route-selection consumers reference this value rather than minting a second `Duration horizon`, so a stale cross-process reuse is impossible by construction and a floating per-call horizon parameter is the deleted form.

```csharp signature
public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputHash, string EpKey) : ISpanFormattable, IUtf8SpanFormattable {
    public static readonly Duration RecencyHorizon = Duration.FromHours(24);

    public static ModelResultKey Of(string modelChecksum, ReadOnlySpan<byte> input, string epKey) =>
        new(modelChecksum, XxHash128.HashToUInt128(input), epKey);

    public override string ToString() => $"{ModelChecksum}:{InputHash:x32}:{EpKey}";

    public string ToString(string? format, IFormatProvider? provider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        destination.TryWrite($"{ModelChecksum}:{InputHash:x32}:{EpKey}", out charsWritten);

    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
        Utf8.TryWrite(utf8Destination, $"{ModelChecksum}:{InputHash:x32}:{EpKey}", out bytesWritten);
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
        public ValueTask<T> Result<T, TState>(ModelResultKey key, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, CancellationToken token = default) =>
            cache.Read(CacheLane.ModelResult, key.ToString(), state, produce, Some(Seq(key.ModelChecksum)), token);
    }

    extension(ReceiptSinkPort sink) {
        public IO<ReceiptEnvelope> Fact(CorrelationId correlation, CacheIndexFact fact) =>
            sink.Send(correlation, TenantContext.Current, "Rasm.Persistence", fact.Kind, JsonSerializer.SerializeToElement(fact, PersistenceWireContext.Default.CacheIndexFact));
    }
}
```

## [4]-[ARTIFACT_BLOB_INDEX]

- Owner: `ArtifactIndexRow` — content-addressed catalog row for execution-provider warm-start contexts, ONNX profiling traces, IFC semantic-ingest model graphs, content-defined chunks, Compute interchange artifacts (tessellated GLB, chunked field, tile content, re-exported glTF), and graduated offline-science ONNX surrogate assets on the blob lane.
- Cases: `EpContext`, `OnnxProfile`, `IfcSemantic`, `ChunkContent`, `Interchange`, `GraduationAsset` kind rows.
- Entry: `static ArtifactIndexRow Admit(string kind, string path, ReadOnlySpan<byte> payload, DataClassification classification, string retentionClass, Instant at)` — pure value; identity derives from the payload, never from the caller.
- Auto: `Admit` stamps content hash, byte length, classification, retention class, and instant in one call; reads ride the `ArtifactBlob` lane through the port with the same stampede and tag law as every cache read.
- Packages: System.IO.Hashing; NodaTime; Rasm.AppHost (project).
- Growth: a new artifact family is one kind row on `ArtifactIndexRow`; a content-defined chunk lands as one `ChunkContent` kind row keyed by its `XxHash128` chunk address (`Version/snapshots#CONTENT_CHUNKING`) so an identical chunk across snapshots and peers dedups on the one content key, never a second chunk store; a graduated offline-science surrogate lands as one `GraduationAsset` kind row content-addressed identically to every other kind over the post-`ModelAssetManifest`-validated ONNX bytes, so a re-graduated model dedups on its content key and a sibling graduation-artifact record is the deleted form; zero new surface.
- Boundary: every artifact lands on the blob lane under its `ContentHash` and receipts name index rows by path — loose temp files are the deleted form; `Classification` enters typed at admission and the unclassified-write rejection is store-side column law; index rows carry their retention class value while eligibility folds belong to the retention axis; the `IfcSemantic` kind admits the Compute interchange IFC model graph — the `DatabaseIfc`-extracted property sets, spatial hierarchy, quantities, materials, and type objects serialized to the canonical bytes the `Bim/Exchange/import#IMPORT_RAIL` produces — content-addressed identically to every other kind through `XxHash128.HashToUInt128` over those bytes, so the same model graph re-ingested under the same tolerance dedupes on its content key, and the admission carries the model graph only, never tessellated BRep geometry: the two-hop tessellation rail (`Ifc -> IfcOpenShell -> GLB`) is a Compute companion concern whose GLB artifact rides the `Interchange` kind row that `Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Admit` content-addresses, so this kind owns semantic residence and the geometry residence stays a distinct `Interchange` blob row; the `Interchange` kind admits every Compute interchange artifact — the tessellated GLB, the chunked field artifact, the 3D-Tiles leaf content, and the re-exported glTF — each content-addressed through `XxHash128.HashToUInt128` over its canonical bytes under one identity scheme, so Compute owns the content-key derivation and this owner owns the blob residence, neither re-declaring the other; the model-graph residence projection onto the document and search lanes is owned at `Query/lanes#DOCUMENT_LANE` and `Query/lanes#SEARCH_LANES` and consumed here as the index identity only; the `GraduationAsset` kind admits the `ONE_GRADUATION_EVIDENCE` `HandoffAxis` surrogate — the content-keyed ONNX artifact a graduated Python offline result produces (post-`ModelAssetManifest` validation) — content-addressed identically through `XxHash128.HashToUInt128` over those bytes, so the C# side runs inference over a durable content key the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` chains and the `AppHost/Runtime/determinism#EVENT_LOG` determinism closure references, never an in-process training loop; the asset rides the existing object-store residence and the `Version/retention.md#RETENTION_SWEEPS` `ClosureGc` reachability sweep so a stale surrogate is collected by the one GC, and a re-import re-verifies the `(checksum, OrtEpDevice)` provider fingerprint at admission so a provider-divergent surrogate fit is caught before inference — Python (`compute/graduation#GRADUATION`) holds the rail singular and this owner adds only the durable blob residence.

```csharp signature
public readonly record struct ArtifactIndexRow(
    UInt128 ContentHash,
    string Kind,
    string Path,
    long Bytes,
    DataClassification Classification,
    string RetentionClass,
    Instant At) {
    public const string EpContext = "ep-context";

    public const string OnnxProfile = "onnx-profile";

    public const string IfcSemantic = "ifc-semantic";

    public const string ChunkContent = "chunk-content";

    public const string Interchange = "interchange";

    public const string GraduationAsset = "graduation-asset";

    public static ArtifactIndexRow Admit(string kind, string path, ReadOnlySpan<byte> payload, DataClassification classification, string retentionClass, Instant at) =>
        new(XxHash128.HashToUInt128(payload), kind, path, payload.Length, classification, retentionClass, at);
}
```

## [5]-[BENCHMARK_INDEX]

- Owner: `BenchmarkRow` — persisted benchmark evidence with the fingerprint-match, recency-bounded claim gate.
- Entry: `static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default)` — `Option` carries fingerprint admission gated by the staleness horizon defaulting to the `ModelResultKey.RecencyHorizon` owned bound; `None` is the fall-through signal toward the caller's static cost rank; `RouteOf` projects the same claim to its route string.
- Auto: ingest rides the bulk lane under its self-emitted changefeed and tag-transition law; every row carries the `benchmark-row` artifact-class value so the `CountBound 1024` retention sweep folds over a traceable class; trend history exports to the analytical lane as parquet with zero second pipeline.
- Packages: NodaTime; LanguageExt.Core; linq2db.EntityFrameworkCore; DuckDB.NET.Data.Full.
- Growth: a new benchmark dimension is one column on `BenchmarkRow`; a new dashboard cut is one analytical export row; zero new surface.
- Boundary: the contract is read-only toward route selection — `Claim` admits only rows whose `HostFingerprint` matches the calling host with ordinal equality and whose `At` stamp falls inside the staleness `horizon` measured from `now`, then folds the matching set to the most-recent `At` so a stale benchmark never selects a route, while the companion `RouteOf` projects that same claim to its `Route` string so one horizon-bounded recency fold serves both; the `horizon` argument defaults to the `ModelResultKey.RecencyHorizon` owned bound so this index consumes the one suite recency horizon rather than minting a second `Duration`, and a caller may narrow it but never re-owns it; the recency fold replaces the first-ordinal `Find`, an `OrderByDescending` LINQ chain, and a counter loop, the fingerprint value arrives as an opaque string minted by the benchmark producer, and the `ArtifactClass` column carries the constant `benchmark-row` class so the retention sweep keys on the same class value the redaction axis bounds; the cluster deletes a benchmarks repository service and a second dashboard export path.

```csharp signature
public readonly record struct BenchmarkRow(
    string Case,
    string Route,
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    string HostFingerprint,
    string ArtifactClass,
    Instant At) {
    public const string BenchmarkRowClass = "benchmark-row";

    public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.HostFingerprint, hostFingerprint) && now - row.At <= (horizon == default ? ModelResultKey.RecencyHorizon : horizon))
            .Fold(
                Option<BenchmarkRow>.None,
                static (held, row) => held.Match(prior => row.At > prior.At ? Some(row) : held, () => Some(row)));

    public static Option<string> RouteOf(Seq<BenchmarkRow> rows, string hostFingerprint, Instant now, Duration horizon = default) =>
        Claim(rows, hostFingerprint, now, horizon).Map(static row => row.Route);
}
```

