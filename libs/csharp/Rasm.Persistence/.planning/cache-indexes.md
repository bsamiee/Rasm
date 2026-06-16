# [PERSISTENCE_CACHE_INDEXES]

Rasm.Persistence contributes exactly one registration row to the suite cache port — the `CacheContribution` capsule serving as the L2 `IDistributedCache` over the key-value lane and as the `IHybridCacheSerializerFactory` riding the MessagePackBinary codec row — selects its L2 residence over the closed `CacheResidence` tier axis, and owns three durable index contracts: model-result, artifact-blob, and benchmark. Cache mechanics stay at the AppHost port where `CacheLane` and `CacheSurface` own stampede, tags, and entry options; the owned surfaces here are key-shape records, catalog rows, the residence registration axis, and one cache-evidence fact stream over Microsoft.Extensions.Caching.Hybrid, Microsoft.Extensions.Caching.StackExchangeRedis, StackExchange.Redis, MessagePack, System.IO.Hashing, and NodaTime, stamped through `ClockPolicy` and emitted through `ReceiptSinkPort`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                   |
| :-----: | :------------------ | :----------------------------------------------------------------------- |
|   [1]   | L2_CONTRIBUTION     | One registration row: L2 store, residence tier axis, serializer factory   |
|   [2]   | MODEL_RESULT_INDEX  | Deterministic result identity, read-through entry, one fact stream       |
|   [3]   | ARTIFACT_BLOB_INDEX | Content-addressed catalog rows for warm-start and profiling artifacts    |
|   [4]   | BENCHMARK_INDEX     | Persisted benchmark rows; fingerprint-gated claim toward route selection |

## [2]-[L2_CONTRIBUTION]

- Owner: `CacheContribution` — one sealed boundary capsule implementing both `IBufferDistributedCache` and `IHybridCacheSerializerFactory`; the package's single cache registration row; `CacheResidence` — the registration-row residence axis carrying each tier's `IDistributedCache` registration.
- Cases: `CacheResidence` in-memory | sqlite | redis | distributed-pg.
- Entry: `bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` — the BCL factory contract shape at the boundary; every other member is the L2 store contract.
- Auto: `TryCreateSerializer<T>` yields the MessagePack codec for every cache payload type with zero per-type registration; the `MessagePackSerializerOptions` value arrives settled from the MessagePackBinary codec row; the hybrid runtime sniffs the buffer contract at registration and routes reads through `TryGetAsync` into its pooled buffer writer and writes through the `ReadOnlySequence<byte>` form, so payload bytes move with zero intermediate arrays; `CacheResidence.Redis` registers `RedisCache` (which implements `IBufferDistributedCache`) through `AddStackExchangeRedisCache` so the buffer-contract zero-copy path holds across the redis tier too.
- Packages: Microsoft.Extensions.Caching.Hybrid; Microsoft.Extensions.Caching.StackExchangeRedis; StackExchange.Redis; MessagePack; NodaTime; Rasm.AppHost (project); BCL inbox.
- Growth: a new L2 residence is one `CacheResidence` case carrying its `IDistributedCache` registration row; a new residence-aware fact is one kind constant on `CacheIndexFact`; zero new surface.
- Boundary: boundary capsule — the BCL cache contracts own these member shapes and the capsule body carries language-owned statement forms; the hybrid runtime confines every L2 touch to `GetAsync`, `TryGetAsync`, `SetAsync`, and `RemoveAsync`, so the synchronous members exist for contract totality and bridge by blocking; the `read` delegate serves the array contract and `readInto` serves the buffer contract — one storage row behind both; absolute expiry is the only L2 lifetime and `Refresh` is structurally inert; `CacheResidence` is a registration-row axis selecting which `IDistributedCache` the one `CacheContribution` capsule reads and writes, never a second cache owner — `InMemory` registers the BCL memory distributed cache, `Sqlite` the embedded key-value L2 (the default), `Redis` registers `StackExchange.Redis` `ConnectionMultiplexer.Connect`/`GetDatabase` behind `RedisCache : IDistributedCache, IBufferDistributedCache` through `AddStackExchangeRedisCache` with `RedisCacheOptions.{Configuration, ConfigurationOptions, InstanceName, ConnectionMultiplexerFactory, ProfilingSession}`, and `DistributedPg` the pg-backed `IDistributedCache` row; each tier keys on the one content-address identity so an L1 miss promotes from the L2 tier without a re-mint and tag invalidation rides the existing op-log changefeed cursor rather than a backplane; the capsule deletes a Persistence-owned cache system, a hand-rolled fake cache type, a residence-per-provider cache class, and every second cache owner in the suite — AppHost owns the port, stampede, and tag vocabulary.

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

    public static Func<IServiceCollection, IServiceCollection> Register(CacheResidence residence, string endpoint, string instance, Func<IServiceProvider, IDistributedCache> pgBacked) =>
        residence.Switch(
            state: (Endpoint: endpoint, Instance: instance, PgBacked: pgBacked),
            inMemory: static _ => static services => services.AddDistributedMemoryCache(),
            sqlite: static _ => static services => services,
            redis: static s => services => services.AddStackExchangeRedisCache(o => {
                o.Configuration = s.Endpoint;
                o.InstanceName = s.Instance;
            }),
            distributedPg: static s => services => services.AddSingleton<IDistributedCache>(s.PgBacked));
}
```

| [INDEX] | [LAW]          | [RULING]                                                                                                                                                                                                                 |
| :-----: | :------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | ownership      | port, stampede protection, tag vocabulary, and entry options stay at the AppHost cache port; the contribution is storage and codec only                                                                                  |
|   [2]   | payload        | every L2 payload lands as one key-value row whose codec-id and content-hash columns are settled row law — codec-tagged bytes, never bare blobs                                                                           |
|   [3]   | expiry         | only the write's absolute relative expiry crosses into `ExpiresAt`, stamped through `ClockPolicy`; an absent value traces to the `CacheTtl` deadline row                                                                 |
|   [4]   | sweep          | expired rows leave on the persistence-maintenance `ScheduleEntry` row under the maintenance lease; each sweep deletion emits one evict fact                                                                              |
|   [5]   | invalidation   | `RemoveByTagAsync` is logical; the bulk path and peer processes drive tag transitions themselves — peers replay entity-kind transitions from the op-log HLC cursor, and L1 staleness stays TTL-bounded with no backplane |
|   [6]   | residence      | `CacheResidence.Sqlite` is the default L2; each tier is one `CacheResidence.Register` row binding the same `IDistributedCache` slot, content-address-keyed so an L1 miss promotes from any tier without a re-mint; `Redis` rides `AddStackExchangeRedisCache` + `RedisCache : IBufferDistributedCache`, `DistributedPg` the pg-backed row; zero change to any owner declared here |
|   [7]   | tier evidence  | every `CacheIndexFact` carries a `Residence` tag and an L1-miss-promotes-from-L2 transition emits the `L2Promote` kind, so the tiered-cache fabric's tier transition is observable; Redis `ProfilingSession` rides the existing telemetry contribution |

## [3]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey`, `CacheIndexFact`, `IndexSurface` — deterministic result identity plus the one cache-evidence fact stream shared by all three indexes; `ModelResultKey.RecencyHorizon` is the suite's sole cross-process result-reuse recency horizon every reuse surface traces to.
- Cases: `Hit`, `Miss`, `Evict`, `L2Promote` kind rows on the fact stream.
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

- Owner: `ArtifactIndexRow` — content-addressed catalog row for execution-provider warm-start contexts and ONNX profiling traces on the blob lane.
- Cases: `EpContext`, `OnnxProfile` kind rows.
- Entry: `static ArtifactIndexRow Admit(string kind, string path, ReadOnlySpan<byte> payload, DataClassification classification, string retentionClass, Instant at)` — pure value; identity derives from the payload, never from the caller.
- Auto: `Admit` stamps content hash, byte length, classification, retention class, and instant in one call; reads ride the `ArtifactBlob` lane through the port with the same stampede and tag law as every cache read.
- Packages: System.IO.Hashing; NodaTime; Rasm.AppHost (project).
- Growth: a new artifact family is one kind row on `ArtifactIndexRow`; zero new surface.
- Boundary: every artifact lands on the blob lane under its `ContentHash` and receipts name index rows by path — loose temp files are the deleted form; `Classification` enters typed at admission and the unclassified-write rejection is store-side column law; index rows carry their retention class value while eligibility folds belong to the retention axis.

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

