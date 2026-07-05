# [PERSISTENCE_QUERY_CACHE]

Rasm.Persistence owns the durable reuse substrate every higher compute lane reads by reference AND the L2 backing the `Rasm.AppHost` `Runtime/resources#CACHE_PORT` cache port resolves: ONE content-addressed artifact-blob index mapping a logical artifact (kind + key) onto its `ContentAddress` residence, ONE model-result recency/dedup index that is the single cross-process result-reuse horizon owner, ONE fingerprint-gated benchmark-claim index that decides every performance-motivated route, and ONE keyed-by-`CacheLane` distributed-cache contribution — the `IBufferDistributedCache` buffer-contract L2 store (the read writes straight into the runtime's pooled writer, sparing the array-`IDistributedCache` intermediate copy; one `byte[]` materializes at the Marten document seam) plus the `IHybridCacheSerializerFactory` MessagePack codec the AppHost `CacheSurface.Register` admits, partitioned by `TenantId` so no tenant reads another's cache bucket. The three index documents are append-substrate rows content-keyed by the suite `XxHash128`, registered in the `Version/retention#RETENTION_CLASSES` `cache`/`blob` classes so the ONE full-history reachability GC governs them; every reuse/dedup/claim read routes through the synchronous `Query/lane#READ_ROUTING` lane because a reuse decision is strong-consistency by construction — a stale dedup serves a wrong result, a daemon-lagged claim wins on the wrong host — and the recency horizon is gated INSIDE the lookup fold, never a separate bool the caller can forget. The lane crosses UP only by reference: `Rasm.Compute` composes `ArtifactIndexRow` (its ONNX EP-context, profile, interchange, and chunk artifacts), `ModelResultIndex` (its inference cache horizon and its distributed sub-block reuse seam), and `BenchmarkRow` (its provider/SIMD/partition claim gate), and never re-mints a second horizon, artifact owner, or benchmark store; the AppHost composes the L2 contribution at the cache port and never a second cache owner. `ContentAddress` arrives from `Element/codec#CONTENT_ADDRESS` (the seam `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`); the MessagePack `SnapshotCodec.Binary` options arrive from `Element/codec#CODEC_AXIS`; `CacheLane`/`HybridCacheEntryFlags`-bearing lane rows arrive from AppHost `Runtime/resources`; `DataClassification`/`TenantContext`/`ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive from AppHost; `RetentionClass` arrives from `Version/retention`; the blob residence the rows reference is the `Store/blobstore#OBJECT_STORE` content-keyed object; Marten is the append substrate and `System.IO.Hashing` `XxHash128` the one content-key hash.

## [01]-[INDEX]

- [01]-[ARTIFACT_BLOB_INDEX]: the `ArtifactKind` taxonomy axis, the content-keyed `ArtifactIndexRow` admission, and the source-keyed projection fold.
- [02]-[MODEL_RESULT_INDEX]: the per-call `ModelResultKey`, the content-addressed `ModelResultIndex` recency/dedup horizon owner with the horizon gate folded into the lookup, and the lookup/publish reuse seam.
- [03]-[BENCHMARK_INDEX]: the `BenchmarkRow` durable claim row and the fingerprint-gated, recency-bounded `Claim` resolution.
- [04]-[L2_CONTRIBUTION]: the `Store`-keyed `IBufferDistributedCache` buffer-contract L2 store, the one `IHybridCacheSerializerFactory` MessagePack codec, and the `TenantId`-partitioned content-address key the AppHost cache port resolves over.

## [02]-[ARTIFACT_BLOB_INDEX]

- Owner: `ArtifactKind` the `[SmartEnum<string>]` artifact taxonomy axis carrying each kind's default `RetentionClass` and `CacheLane`; `ArtifactIndexRow` the content-keyed index row — kind, logical key, the `ContentAddress` over the bytes, the byte size, the classification stamp, the optional source-artifact key, and the stamp — with `Admit` the one content-addressing factory and `Project` the source-keyed projection fold; the row is the artifact-residence index, never the blob bytes (the `Store/blobstore#OBJECT_STORE` object owns residence) and never a second identity.
- Cases: `ArtifactKind` rows `interchange` (tessellated GLB / chunked field / tile content / re-exported glTF, `blob`-class, `ArtifactBlob` lane) · `ep-context` (compiled ONNX EP-context blob, `cache`-class, `ModelResult` lane) · `onnx-profile` (chrome-trace profiling export, `cache`-class, `ModelResult` lane) · `ifc-semantic` (the Bim IFC semantic graph, `blob`-class, `ArtifactBlob` lane) · `chunk-content` (a content-defined chunk body, `blob`-class, `ArtifactBlob` lane); a new artifact family is one row carrying its retention class and its lane, never a per-kind row type.
- Entry: `public static ArtifactIndexRow Admit(ArtifactKind kind, string key, ReadOnlySpan<byte> bytes, DataClassification classification, Instant at, Option<UInt128> sourceKey = default)` content-addresses the bytes through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` (the kernel seed-zero `XxHash128`, no second hasher) and stamps the row with the kind's own typed `RetentionClass` (read from `ArtifactKind.Retention`, never a parallel `string`); `public static HashMap<UInt128, Seq<ArtifactIndexRow>> Project(Seq<ArtifactIndexRow> rows)` folds the index into the source-keyed projection family so a tessellated GLB and the IFC-semantic graph of one source IFC return under one `SourceKey`.
- Auto: `Admit` is the single content-addressing path — the `ContentAddress` is the seam `ContentAddress.Of(bytes)` over the artifact bytes (the suite hash law, never a path- or filename-keyed identity and never a second hasher), the byte size records from the admitted span's length (never a later filesystem stat), and a self-keyed artifact carries `None` source while a derived artifact (a GLB tessellated from a source IFC) threads the source IFC's content key as `Some` so the two-projection family stays joined; the source key is the KERNEL seed-zero key over the source bytes (the `Bim/Exchange/tessellation#TESSELLATION_BRIDGE` mints it tolerance-independently), NEVER a policy-seeded interchange-cache key, so the GLB and the semantic graph share one origin even across tessellation settings; `Project` groups by `SourceKey.IfNone(Content)` so a self-keyed row projects under its own content and a source-keyed family under its shared origin; the retention class and lane arrive settled from the `ArtifactKind` row so the artifact admits into the `Version/retention#RETENTION_CLASSES` class without a second taxonomy and reads its cache lane without a second routing axis.
- Receipt: an artifact admission rides `store.cache.artifact` carrying the kind, content key, and byte size; the actual blob write rides the `Store/blobstore#OBJECT_STORE` `store.blob.write` and the index row references that residence by content key, never duplicating the byte transfer.
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), AppHost (`CacheLane`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact family is one `ArtifactKind` row carrying its retention class and lane; a new index column is one field on `ArtifactIndexRow`; zero new surface — a per-kind row type, a second content-key hash, a path-keyed identity, a `string` retention column beside the typed `RetentionClass`, or a managed copy of the blob bytes beside the index is the deleted form because the kind axis is the discriminant and the object store owns residence.
- Boundary: the index row is content-keyed by the same `XxHash128` the kernel mints and the `Store/blobstore#OBJECT_STORE` object name derives from, so the artifact index, the blob residence, and the retention catalog share ONE identity scheme and the index never mints a second; the row references the blob by content key and the `Store/blobstore` lane writes the bytes write-blob-first, so a crash leaves a collectible orphan blob the `Version/retention#SWEEP_AND_GC` reachability mark reaps, never a dangling index row; the `blob`-class kinds (`interchange`/`ifc-semantic`/`chunk-content`) register full-history-reachable so an artifact a historical AS-OF cut references survives, while the `cache`-class kinds (`ep-context`/`onnx-profile`) are receipted-evict and re-derivable (a cold companion recompiles the EP-context blob); each kind carries its `CacheLane` so the large-payload `interchange`/`ifc-semantic`/`chunk-content` rows ride the `ArtifactBlob` lane whose `HybridCacheEntryFlags.DisableLocalCache` keeps an oversized blob from pinning the in-process L1 while the small `ep-context`/`onnx-profile` rows ride the `ModelResult` lane — the L1/L2 routing is a closed lane value on the kind row, never a per-call branch (`#L2_CONTRIBUTION`); the upstream `Rasm.Compute` lanes compose the `ArtifactKind` constants as settled vocabulary (`onnx-profile` from the inference profiling run, `ep-context` from the session warm-start/fleet compile, `interchange` from the codec content-addressing through `ArtifactIndexRow.Admit`) and a Compute-side artifact owner beside this index is the named drift defect; classification arrives settled so an unstamped artifact rejects at retention admission identically to an over-ceiling one because absence of evidence is not clearance.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind Interchange = new("interchange", RetentionClass.Blob, CacheLane.ArtifactBlob);
    public static readonly ArtifactKind EpContext = new("ep-context", RetentionClass.Cache, CacheLane.ModelResult);
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile", RetentionClass.Cache, CacheLane.ModelResult);
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic", RetentionClass.Blob, CacheLane.ArtifactBlob);
    public static readonly ArtifactKind ChunkContent = new("chunk-content", RetentionClass.Blob, CacheLane.ArtifactBlob);

    public RetentionClass Retention { get; }
    public CacheLane Lane { get; }
    private ArtifactKind(string key, RetentionClass retention, CacheLane lane) : this(key) => (Retention, Lane) = (retention, lane);
}

// --- [MODELS] -----------------------------------------------------------------------------

public sealed record ArtifactIndexRow(
    ArtifactKind Kind,
    string Key,
    ContentAddress Content,
    long Bytes,
    DataClassification Classification,
    Option<UInt128> SourceKey,
    Instant At) {
    public RetentionClass Retention => Kind.Retention;

    public static ArtifactIndexRow Admit(ArtifactKind kind, string key, ReadOnlySpan<byte> bytes, DataClassification classification, Instant at, Option<UInt128> sourceKey = default) =>
        new(kind, key, ContentAddress.Of(bytes), bytes.Length, classification, sourceKey, at);

    public static HashMap<UInt128, Seq<ArtifactIndexRow>> Project(Seq<ArtifactIndexRow> rows) =>
        rows.Fold(HashMap<UInt128, Seq<ArtifactIndexRow>>(), static (acc, row) =>
            acc.AddOrUpdate(row.SourceKey.IfNone(row.Content.Value), chain => chain.Add(row), Seq(row)));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content-key name    | kernel `XxHash128` over the bytes      | shared with blob residence + retention; never a second id |
|  [02]   | kind taxonomy       | one `ArtifactKind` row per family      | carries the typed `RetentionClass` + `CacheLane`; no per-kind row type |
|  [03]   | residence owner     | `Store/blobstore#OBJECT_STORE`         | the index references by content key; never the bytes      |
|  [04]   | source projection   | `Project` groups by kernel `SourceKey` | GLB + IFC-semantic of one source stay one family          |

## [03]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey` the per-call deterministic cache key (model checksum, input digest, the EP/version/precision result key) with its `Content` fold over the seam `ContentAddress` and stable string form; `ModelResultRow` the indexed residence (content address, the blob `ContentAddress`, the host fingerprint string, the stamp); `ModelResultIndex` the content-addressed recency/dedup index — the SINGLE cross-process result-reuse horizon owner — carrying the `RecencyHorizon`, the clock the horizon gate reads, and the `Resolve`/`Record` ports, with the `Lookup`/`Publish` reuse seam folding the horizon gate INTO the resolve; the index is keyed by the suite `XxHash128` content address and never mints a second horizon.
- Cases: a lookup either resolves a residence that is FRESH within the horizon or misses (a stale residence misses by construction, never a separate caller-applied bool); the reuse seam is content-addressed so an inference cache key and a distributed solve sub-block key both fold to one `UInt128` the index resolves identically.
- Entry: `public IO<Option<ModelResultRow>> Lookup(UInt128 content)` resolves a content-addressed residence AND drops any row older than `RecencyHorizon` against the index clock in the SAME fold and `public IO<Option<ModelResultRow>> Lookup(ModelResultKey key)` folds the key first; `public IO<Unit> Publish(ModelResultRow row)` records a computed residence under its content address; `public bool Fresh(Instant at, Instant now)` is the recency predicate `now - at <= RecencyHorizon` the lookup fold applies and a consumer reads only to explain a miss; the ports are supplied by composition over the Marten lightweight session so the index is read by reference, never embedded.
- Auto: the per-call `ModelResultKey.Content` composes the seam `ContentAddress.Of` over the LENGTH-FRAMED canonical key preimage (each UTF8 string — model checksum, result key — prefixed by its LE `int32` byte count, the fixed-16 `InputDigest` big-endian between them self-delimiting, exactly the `Query/lane#ELEMENT_SET_ALGEBRA` `Canonical` framing law so a `(checksum, result-key)` split shift can never collide two distinct calls onto one cached result) so an inference run and its dedup probe address identically AND the suite owns the one `XxHash128` — a second hasher beside the seam, or an unframed concatenation that keys distinct inputs alike, is the deleted form; `ToString` is the stable `HybridCache` lane key the `#L2_CONTRIBUTION` content-address partition scopes; `Lookup` reads through the synchronous lane (a reuse decision is strong-consistency, never a daemon-lagged async read), `Resolve`s the residence, then `Fresh`-gates it against the index clock so a result older than the horizon resolves to `None` and re-computes rather than serving stale — the gate is structural, not a documented obligation; `Publish` records the residence content-addressed so two callers with byte-identical inputs converge on one stored result; the index registers in the `Version/retention#RETENTION_CLASSES` `cache` class so the horizon sweep evicts past the age bound and the one GC governs it.
- Receipt: a reuse hit rides `store.cache.result.hit` carrying the content key, a stale-skip rides `store.cache.result.stale` carrying the content key and age, a publish rides `store.cache.result.publish` carrying the content key and blob residence; the index emits no compute fact (the `Runtime/receipts` `Cache`/`Factorization` facts are the upstream Compute lane's, read by reference).
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new reuse dimension is one field folded into `ModelResultKey.Content`'s canonical preimage; a new residence column is one field on `ModelResultRow`; zero new surface — a second recency horizon, a per-lane dedup owner, a Compute-side result store, a caller-applied freshness bool the lookup does not enforce, or a daemon-lagged reuse read is the deleted form because this is the single horizon owner, the gate is folded in, and the read is synchronous.
- Boundary: this is the ONE cross-process result-reuse recency horizon — the upstream inference cache (`Model/inference#RESULT_CACHE`), the distributed solve sub-block reuse (`Tensor/factor#SHARD_FANOUT` threads it as the `Blocked.Reuse` column and reads RESIDENCE only — `Lookup` resolves the dedup-keyed `ModelResultRow`, the object-store port yields the `SolveResponse` bytes at that residence), the benchmark recency gate (`#BENCHMARK_INDEX` reads `RecencyHorizon`), and the cost-formula reuse (`Symbolic/lowering#LOWERING_CACHE` keyed by its OWN content identity, never a fabricated `ModelResultKey`) all read it by reference and a second `Duration horizon` minted beside it is the named defect; the index is content-addressed by the suite `XxHash128` so a sub-block keyed by the streamed-request hash folded with the provider dedup key and an inference key folded from `ModelResultKey` resolve through the same `Lookup`/`Publish` seam, never two dedup owners, and `Publish` records the RESIDENCE row (the index never holds the payload — a 2-arg `Publish(address, payload)` is the deleted phantom); the reuse read is the synchronous `Query/lane#READ_ROUTING` lane because serving a stale dedup is a correctness fault, never the async columnar lane; the freshness gate lives INSIDE `Lookup` so a consumer cannot reuse a stale row by forgetting a bool — the only correct miss-or-hit is the index's own; the host fingerprint crosses as a string (the upstream `HostFingerprint.ToString`/`DeterminismTag`) so the index holds no Compute type and the strata dependency stays one-directional; `ModelResultKey` carries ONNX-run identity (model/EP/precision) so a non-ONNX content-keyed reuse (a compiled symbolic formula) keys by its own content identity and never fabricates a `ModelResultKey`.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------

public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputDigest, string ResultKey) {
    // The dedup-key preimage is LENGTH-FRAMED at every variable-width field exactly as the sibling
    // `Query/lane#ELEMENT_SET_ALGEBRA` `Canonical` law mandates: each UTF8 string is prefixed by its LE `int32`
    // byte count so a `(ModelChecksum, ResultKey)` split shift can NEVER collide on an unframed concatenation
    // (`checksum:"ab",rk:"c"` and `checksum:"a",rk:"bc"` are distinct preimages). The `InputDigest` is fixed-16
    // so it self-delimits. An unframed concat here would key two DIFFERENT calls to one cached result — the wrong
    // result a stale-dedup correctness fault serves, so the frame is the same collision-free discipline as the set receipt.
    public UInt128 Content {
        get {
            var preimage = new ArrayBufferWriter<byte>();
            Frame(preimage, ModelChecksum);
            BinaryPrimitives.WriteUInt128BigEndian(preimage.GetSpan(16), InputDigest);
            preimage.Advance(16);
            Frame(preimage, ResultKey);
            return ContentAddress.Of(preimage.WrittenSpan).Value;
        }
    }

    static void Frame(ArrayBufferWriter<byte> preimage, string value) {
        int bytes = Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), bytes);
        preimage.Advance(4);
        preimage.Advance(Encoding.UTF8.GetBytes(value, preimage.GetSpan(bytes)));
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{ModelChecksum}:{InputDigest:x32}:{ResultKey}");
}

public readonly record struct ModelResultRow(UInt128 Content, ContentAddress Residence, string Fingerprint, Instant At);

// --- [OPERATIONS] -------------------------------------------------------------------------

public sealed record ModelResultIndex(
    Duration RecencyHorizon,
    ClockPolicy Clocks,
    Func<UInt128, IO<Option<ModelResultRow>>> Resolve,
    Func<ModelResultRow, IO<Unit>> Record) {
    public IO<Option<ModelResultRow>> Lookup(UInt128 content) =>
        Resolve(content).Map(found => found.Filter(row => Fresh(row.At, Clocks.Now)));

    public IO<Option<ModelResultRow>> Lookup(ModelResultKey key) => Lookup(key.Content);
    public IO<Unit> Publish(ModelResultRow row) => Record(row);
    public bool Fresh(Instant at, Instant now) => now - at <= RecencyHorizon;
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | reuse horizon       | the ONE `RecencyHorizon` owner         | inference + solve + benchmark + formula read by reference |
|  [02]   | horizon gate        | folded INTO `Lookup` against the clock | a stale row misses; never a caller-applied bool           |
|  [03]   | dedup key           | seam `ContentAddress.Of` (`XxHash128`) | one seam for inference keys and solve sub-blocks; no second hasher |
|  [04]   | read consistency    | synchronous `Query/lane` lane          | a stale dedup is a correctness fault; never async         |
|  [05]   | residence-only      | `Publish` records the row, not bytes   | the object store owns the payload; no 2-arg phantom       |
|  [06]   | fingerprint cross   | `HostFingerprint.ToString` string      | no Compute type; strata stays one-directional             |

## [04]-[BENCHMARK_INDEX]

- Owner: `BenchmarkRow` the durable benchmark-claim index row — the claim key, the winning route, the median/p95 durations, the allocated bytes, the host fingerprint string, the retention-class key, and the stamp — with `Claim` the fingerprint-gated, recency-bounded resolution and `BenchmarkRowClass` the row's default retention class; a claim is a row, never prose, and the row carries no Compute type.
- Cases: a claim either resolves the most-recent fingerprint-matching row within the recency horizon or falls through to `None` (the upstream lane's static cost-rank fallback); a `within` bound gates the resolution against the `ModelResultIndex.RecencyHorizon` so a stale claim never wins.
- Entry: `public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string fingerprint, Option<(Duration Horizon, Instant Now)> within = default)` resolves the most-recent row whose fingerprint matches the running host and (when `within` is supplied) whose stamp falls inside the recency horizon, folding to the head without materializing an ordered copy; `public static readonly string BenchmarkRowClass` is the row's retention-class key.
- Auto: `Claim` filters the rows to the exact running fingerprint (so a benchmark claimed under managed never wins on a host that resolved native-MKL because the `DeterminismTag` drifts the fingerprint string) and within the optional horizon bound, then folds to the latest-`At` survivor through one `MostRecent` reduction (never a full `OrderByDescending` materialization, the recency horizon read by reference from `ModelResultIndex` gating the speed claim exactly as it gates result reuse); the row registers in the `Version/retention#RETENTION_CLASSES` `cache` class so a re-derivable claim evicts past the age bound and the sweep governs it.
- Receipt: a claim admission rides `store.cache.benchmark` carrying the claim key and fingerprint; the sweep run that produces the claim rows rides the upstream Compute lane's own `TensorRun`/`ModelRun` facts, read by reference, never re-emitted here.
- Packages: NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new claim dimension is one column on `BenchmarkRow`; a new claim key shape is one folded into the upstream `BenchmarkClaim.Key`; zero new surface — a second benchmark store, a profiler add-on owner, or prose performance claims are the deleted form because the claim is a row and the gate is one `Claim` resolution.
- Boundary: the row holds the fingerprint as a STRING (the upstream `HostFingerprint.ToString`/`DeterminismTag`) so the benchmark index carries no Compute type and the strata dependency stays one-directional — the upstream `Rasm.Compute` numeric and SIMD lanes compose `Claim` by reference (`Tensor/blas#PROVIDER_CLAIMS` resolves the winner against the running fingerprint and `ModelResultIndex.RecencyHorizon` then hands it to `LinearProvider.Select`) and a second benchmark store beside this index is the named defect; the claim is fingerprint-gated and recency-bounded so a stale or wrong-host benchmark never wins a route, and the recency horizon is the `ModelResultIndex` owner's, never a second `Duration` minted here; the retention class is the `cache` row because a benchmark claim is re-derivable by re-running the equivalence sweep, so the sweep governs eviction and a never-evict benchmark store is the named defect.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------

public sealed record BenchmarkRow(
    string Key,
    string Route,
    Duration Median,
    Duration P95,
    long AllocatedBytes,
    string Fingerprint,
    string Retention,
    Instant At) {
    public static readonly string BenchmarkRowClass = RetentionClass.Cache.Key;

    public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string fingerprint, Option<(Duration Horizon, Instant Now)> within = default) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.Fingerprint, fingerprint))
            .Filter(row => within.Case is (Duration horizon, Instant now) ? now - row.At <= horizon : true)
            .Fold(Option<BenchmarkRow>.None, static (best, row) => best.Case is BenchmarkRow held && held.At >= row.At ? best : row);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | claim gate          | fingerprint-match + latest survivor    | a wrong-host or stale claim never wins a route            |
|  [02]   | recency bound       | `ModelResultIndex.RecencyHorizon`      | one horizon owner; never a second `Duration`              |
|  [03]   | head fold           | one `MostRecent` reduction             | no full `OrderByDescending` materialization               |
|  [04]   | retention class     | `cache` (re-derivable by re-sweep)     | the sweep governs eviction; never never-evict             |

## [05]-[L2_CONTRIBUTION]

- Owner: `CacheL2Store` the `IBufferDistributedCache` over the Marten document store the AppHost `Runtime/resources#CACHE_PORT` registers under a lane's `Store` key (one store per key — `durable-l2` shared by the `ModelResult` and `Projection` lanes, the blob lane carrying no `Store`) — the buffer-contract distributed backing whose `TryGetAsync(IBufferWriter<byte>)` writes the persisted bytes straight into the runtime's pooled writer (no returned-`byte[]` the runtime must then re-copy, the saved allocation the array `IDistributedCache` path cannot avoid) and whose `SetAsync(ReadOnlySequence<byte>)` accepts the serializer's segmented sequence without forcing the caller to flatten first — the document column itself materializes one `byte[]` at the Marten persistence seam (a relational column is not a `ReadOnlySequence`), so the buffer contract removes the cache-runtime intermediate copy, never the one durable row; `CacheCodecFactory` the one `IHybridCacheSerializerFactory` Persistence hands `CacheSurface.Register(services, contributed)` so the suite never registers a per-type `AddSerializer<T>`; `CachePartition` the static surface deriving the `TenantId`-scoped content-address key the lane keys read; the contribution is the L2 half of the cache, the AppHost `HybridCache` port the L1+stampede+tag half — one cache owner across both, never a second.
- Cases: the L2 store backs every lane whose `CacheLane.Store` is set (`ModelResult`, `Projection` on `durable-l2`) while a lane with no `Store` (`ArtifactBlob`) resolves the default `HybridCache` with no L2 leg; the codec factory yields a `CacheCodec<T>` for every payload `T` from one MessagePack pass, so a `ModelResultRow`, a `Cached<Fin<T>>` envelope, and a projection document round-trip under one registered factory.
- Entry: `public bool TryGet(string key, IBufferWriter<byte> destination)` / `public ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default)` read L2 into the pooled buffer; `public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options)` / `public ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default)` write the sequence; `public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)` yields the MessagePack codec for every `T`; `public static string Scoped(CacheLane lane, UInt128 content)` derives the `TenantId`-partitioned lane key over the content address.
- Auto: the L2 store implements `IBufferDistributedCache` so the `HybridCache` runtime sniffs the buffer contract at registration and routes reads through `TryGetAsync(IBufferWriter<byte>)` into its pooled writer and writes through the `ReadOnlySequence<byte>` form — the cache-runtime↔store hop carries no intermediate `byte[]` allocation the array `IDistributedCache` path forces, the single `byte[]` the `CacheBlob` document column holds being the Marten persistence seam's materialization, not a runtime copy; the inherited array `IDistributedCache` members bridge by blocking on the buffer forms (one storage row satisfies both contracts, never a second store), and `Refresh` is inert and `TryGetAsync` does NOT gate row expiry because the `HybridCache` runtime stamps a self-describing expiry envelope into the L2 payload (`HybridCachePayload.Write` records creation-time + duration) and re-validates it on every read (`HybridCachePayload.TryParse` recomputes `creation + duration - now` and rejects a lapsed entry as `ExpiredByEntry`/`ExpiredByTag`/`ExpiredByWildcard` BEFORE deserializing), so a Marten L2 row whose `AbsoluteExpiration` has lapsed but the retention sweep has not yet collected can NEVER be served stale through the runtime — a read-time expiry gate inside the store would duplicate the envelope check the runtime already owns, so absolute expiry plus the retention sweep is the only L2-row lifetime and the store stays a pure storage leg; the codec factory's `CacheCodec<T>` serializes through `MessagePackSerializer.Serialize<T>(IBufferWriter<byte>, value, SnapshotCodec.Binary)` and `Deserialize<T>(in ReadOnlySequence<byte>, SnapshotCodec.Binary)` so the L2 wire is the SAME `Element/codec#CODEC_AXIS` `messagepack` row (`Lz4BlockArray` in-codec) the snapshot/event wire rides — a second cache serializer is the deleted form; `Scoped` prefixes the lane key with the `TenantContext.Current.TenantId.Value` `UInt128` written big-endian and digested through `XxHash128` (the suite big-endian transcription law, never the platform-default little-endian export) so a multi-tenant store partitions the content-address key by tenant and a cross-tenant L2 bucket collision is unrepresentable.
- Receipt: the L2 contribution emits no cache fact of its own — hit/miss/evict are the AppHost `HybridCacheOptions.ReportTagMetrics` consequences metered by lane tag, and the durable row lifecycle is the `Version/retention` `cache`/`blob` sweep's; the contribution is a storage + codec leg, never a second receipt stream.
- Packages: Microsoft.Extensions.Caching.Hybrid (`IBufferDistributedCache`/`IHybridCacheSerializer<T>`/`IHybridCacheSerializerFactory`), Marten (`IDocumentStore`), MessagePack (`MessagePackSerializer`), System.IO.Hashing (`XxHash128`), AppHost (`CacheLane`/`TenantContext`), Rasm.Element (`ContentAddress`), LanguageExt.Core, BCL inbox.
- Growth: a new L2 topology is one `CacheLane.Store` value the AppHost `Register` binds through `DistributedCacheServiceKey`; a new payload type round-trips through the one factory with zero registration; a redis swap is the `Microsoft.Extensions.Caching.StackExchangeRedis` `RedisCache` (itself an `IBufferDistributedCache`, so the buffer-contract seam survives the swap) selected by the lane's `Store` key; zero new surface — a bare `IDistributedCache` store that forces the extra runtime-copy the buffer path spares, a per-type `AddSerializer<T>` scatter, a second cache owner, or an un-partitioned cross-tenant key is the deleted form because the buffer contract is the one storage row, the factory is the one serializer, and `Scoped` is the one tenant partition.
- Boundary: Persistence contributes exactly ONE L2 store row (the `IBufferDistributedCache` buffer-contract storage that spares the cache-runtime intermediate-array copy, persisting one `byte[]` at the Marten document seam) plus ONE `IHybridCacheSerializerFactory` (the MessagePack codec for every payload `T`), registered through the AppHost `CacheSurface.Register(services, contributed)` `AddSerializerFactory` on every keyed builder, never a per-type `AddSerializer<T>`; the AppHost `HybridCache` runtime composes ON TOP — `GetOrCreateAsync` drives stampede-protected single-flight population, `RemoveByTagAsync` cuts a lane by its key tag, and the `HybridCacheEntryFlags` lane axis (`DisableLocalCache` on the `ArtifactBlob` lane so an oversized GLB never pins L1, `None` on the `ModelResult` lane) is the per-lane L1/L2 routing — so the L1+stampede+tag-invalidation half is the AppHost port's and the L2-store+serializer half is this contribution, one cache owner across both and never a second; the L2 wire is the `messagepack` `SnapshotCodec.Binary` row so the durable cache bytes and the snapshot/event bytes share one codec and one `Instant` formatter, never a cache-local serializer; the content-address key partitions by `TenantId` through `Scoped` so the `#MODEL_RESULT_INDEX` `ModelResultKey.ToString` lane key and the `#ARTIFACT_BLOB_INDEX` content key both read one tenant-scoped identity exactly as `Element/identity#ELEMENT_IDENTITY` scopes the durable row by `current_setting('rasm.tenant')::uuid`; tag invalidation is an explicit cache capability and never substitutes for durable store integrity — a tag cut is a logical miss-until-expiry, the `RemoveAsync` physical delete its sibling, and the durable reuse rows live on the retention sweep, not the cache TTL.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------

public sealed class CacheL2Store(IDocumentStore store, string storeKey) : IBufferDistributedCache {
    public bool TryGet(string key, IBufferWriter<byte> destination) => TryGetAsync(key, destination).AsTask().GetAwaiter().GetResult();

    public async ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) {
        await using var session = store.QuerySession();
        var row = await session.LoadAsync<CacheBlob>($"{storeKey}:{key}", token).ConfigureAwait(false);
        if (row is null) { return false; }
        destination.Write(row.Payload.Span);
        return true;
    }

    public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).AsTask().GetAwaiter().GetResult();

    public async ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default) {
        await using var session = store.LightweightSession();
        session.Store(new CacheBlob($"{storeKey}:{key}", value.ToArray(), options.AbsoluteExpiration));
        await session.SaveChangesAsync(token).ConfigureAwait(false);
    }

    public byte[]? Get(string key) { var w = new ArrayBufferWriter<byte>(); return TryGet(key, w) ? w.WrittenSpan.ToArray() : null; }
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default) { var w = new ArrayBufferWriter<byte>(); return await TryGetAsync(key, w, token).ConfigureAwait(false) ? w.WrittenSpan.ToArray() : null; }
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => Set(key, new ReadOnlySequence<byte>(value), options);
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => SetAsync(key, new ReadOnlySequence<byte>(value), options, token).AsTask();
    public void Refresh(string key) { }
    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();
    public async Task RemoveAsync(string key, CancellationToken token = default) { await using var s = store.LightweightSession(); s.Delete<CacheBlob>($"{storeKey}:{key}"); await s.SaveChangesAsync(token).ConfigureAwait(false); }
}

public sealed record CacheBlob(string Id, byte[] Payload, DateTimeOffset? AbsoluteExpiration);

public sealed class CacheCodecFactory : IHybridCacheSerializerFactory {
    public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer) {
        serializer = new CacheCodec<T>();
        return true;
    }
}

public sealed class CacheCodec<T> : IHybridCacheSerializer<T> {
    public T Deserialize(ReadOnlySequence<byte> source) => MessagePackSerializer.Deserialize<T>(source, SnapshotCodec.Binary);
    public void Serialize(T value, IBufferWriter<byte> target) => MessagePackSerializer.Serialize(target, value, SnapshotCodec.Binary);
}

// --- [OPERATIONS] -------------------------------------------------------------------------

public static class CachePartition {
    public static string Scoped(CacheLane lane, UInt128 content) {
        Span<byte> tenant = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(tenant, TenantContext.Current.TenantId.Value);
        return string.Create(CultureInfo.InvariantCulture, $"{lane.Key}:{XxHash128.HashToUInt128(tenant):x32}:{content:x32}");
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one L2 store        | `IBufferDistributedCache` buffer contract | bare `IDistributedCache` forces the extra runtime-copy; rejected |
|  [02]   | one serializer      | `IHybridCacheSerializerFactory`        | the `messagepack` `SnapshotCodec.Binary` row; no per-type scatter |
|  [03]   | tenant partition    | `Scoped` over `TenantId` digest        | a cross-tenant L2 bucket collision is unrepresentable     |
|  [04]   | lane L1/L2 routing  | the AppHost `HybridCacheEntryFlags`    | `DisableLocalCache` on the blob lane; never a per-call branch |
|  [05]   | one cache owner     | L2+codec here, L1+stampede+tag AppHost | composed at `CacheSurface.Register`; never a second owner |
