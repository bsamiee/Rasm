# [PERSISTENCE_QUERY_CACHE]

Rasm.Persistence owns the durable reuse index every higher compute lane reads by reference: ONE content-addressed artifact-blob index that maps a logical artifact (kind + key) onto its `ContentAddress` residence, ONE model-result recency/dedup index that is the single cross-process result-reuse horizon owner, and ONE fingerprint-gated benchmark-claim index that decides every performance-motivated route. All three are append-substrate documents content-keyed by the suite `XxHash128`, registered in the `Version/retention#RETENTION_CLASSES` `cache`/`blob` classes so the ONE full-history reachability GC governs them, and read through the synchronous `Query/lane#READ_ROUTING` lane because a reuse/dedup decision is strong-consistency by construction — a stale dedup serves a wrong result and a daemon-lagged claim wins on the wrong host. The lane crosses UP only by reference: `Rasm.Compute` composes `ArtifactIndexRow` (its ONNX EP-context, profile, interchange, and chunk artifacts), `ModelResultIndex` (its inference cache horizon and its distributed sub-block reuse seam), and `BenchmarkRow` (its provider/SIMD/partition claim gate), and never re-mints a second horizon, a second artifact owner, or a second benchmark store. `ContentAddress` arrives from `Element/codec` (the seam `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`); `DataClassification` and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive from AppHost; `RetentionClass` arrives from `Version/retention`; the blob residence the rows reference is the `Store/blobstore#OBJECT_STORE` content-keyed object; Marten is the append substrate and `System.IO.Hashing` `XxHash128` the one content-key hash.

## [01]-[INDEX]

- [01]-[ARTIFACT_BLOB_INDEX]: the `ArtifactKind` taxonomy axis, the content-keyed `ArtifactIndexRow` admission, and the source-keyed projection fold.
- [02]-[MODEL_RESULT_INDEX]: the per-call `ModelResultKey`, the content-addressed `ModelResultIndex` recency/dedup horizon owner, and the lookup/publish reuse seam.
- [03]-[BENCHMARK_INDEX]: the `BenchmarkRow` durable claim row and the fingerprint-gated, recency-bounded `Claim` resolution.

## [02]-[ARTIFACT_BLOB_INDEX]

- Owner: `ArtifactKind` the `[SmartEnum<string>]` artifact taxonomy axis carrying each kind's default `RetentionClass`; `ArtifactIndexRow` the content-keyed index row — kind, logical key, the `ContentAddress` over the bytes, the byte size, the classification stamp, the retention-class key, the optional source-artifact key, and the stamp — with `Admit` the one content-addressing factory and `Project` the source-keyed projection fold; the row is the artifact-residence index, never the blob bytes (the `Store/blobstore#OBJECT_STORE` object owns residence) and never a second identity.
- Cases: `ArtifactKind` rows `interchange` (tessellated GLB / chunked field / tile content / re-exported glTF, `blob`-class) · `ep-context` (compiled ONNX EP-context blob, `cache`-class) · `onnx-profile` (chrome-trace profiling export, `cache`-class) · `ifc-semantic` (the Bim IFC semantic graph, `blob`-class) · `chunk-content` (a content-defined chunk body, `blob`-class); a new artifact family is one row carrying its retention class, never a per-kind row type.
- Entry: `public static ArtifactIndexRow Admit(ArtifactKind kind, string key, ReadOnlySpan<byte> bytes, DataClassification classification, string retentionClass, Instant at, Option<UInt128> sourceKey = default)` content-addresses the bytes through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` (the kernel seed-zero `XxHash128`, no second hasher) and stamps the row; `public static HashMap<UInt128, Seq<ArtifactIndexRow>> Project(Seq<ArtifactIndexRow> rows)` folds the index into the source-keyed projection family so a tessellated GLB and the IFC-semantic graph of one source IFC return under one `SourceKey`.
- Auto: `Admit` is the single content-addressing path — the `ContentAddress` is the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of(bytes)` over the artifact bytes (the suite hash law, never a path- or filename-keyed identity and never a second hasher), the byte size records from the admitted span's length (never a later filesystem stat), and a self-keyed artifact carries `None` source while a derived artifact (a GLB tessellated from a source IFC) threads the source IFC's content key as `Some` so the two-projection family stays joined; `Project` groups by `SourceKey.IfNone(Content)` so a self-keyed row projects under its own content and a source-keyed family under its shared origin; the classification stamp and retention-class key arrive settled from the admitting lane so the row admits into the `Version/retention#RETENTION_CLASSES` class without a second taxonomy.
- Receipt: an artifact admission rides `store.cache.artifact` carrying the kind, content key, and byte size; the actual blob write rides the `Store/blobstore#OBJECT_STORE` `store.blob.write` and the index row references that residence by content key, never duplicating the byte transfer.
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact family is one `ArtifactKind` row carrying its retention class; a new index column is one field on `ArtifactIndexRow`; zero new surface — a per-kind row type, a second content-key hash, a path-keyed identity, or a managed copy of the blob bytes beside the index is the deleted form because the kind axis is the discriminant and the object store owns residence.
- Boundary: the index row is content-keyed by the same `XxHash128` the kernel mints and the `Store/blobstore#OBJECT_STORE` object name derives from, so the artifact index, the blob residence, and the retention catalog share ONE identity scheme and the index never mints a second; the row references the blob by content key and the `Store/blobstore` lane writes the bytes write-blob-first, so a crash leaves a collectible orphan blob the `Version/retention#SWEEP_AND_GC` reachability mark reaps, never a dangling index row; the `blob`-class kinds (`interchange`/`ifc-semantic`/`chunk-content`) register full-history-reachable so an artifact a historical AS-OF cut references survives, while the `cache`-class kinds (`ep-context`/`onnx-profile`) are receipted-evict and re-derivable (a cold companion recompiles the EP-context blob); the upstream `Rasm.Compute` lanes compose the `ArtifactKind` constants as settled vocabulary (`onnx-profile` from the inference profiling run, `ep-context` from the session warm-start/fleet compile, `interchange` from the codec content-addressing) and a Compute-side artifact owner beside this index is the named drift defect; classification arrives settled so an unstamped artifact rejects at retention admission identically to an over-ceiling one because absence of evidence is not clearance.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind Interchange = new("interchange", RetentionClass.Blob);
    public static readonly ArtifactKind EpContext = new("ep-context", RetentionClass.Cache);
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile", RetentionClass.Cache);
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic", RetentionClass.Blob);
    public static readonly ArtifactKind ChunkContent = new("chunk-content", RetentionClass.Blob);

    public RetentionClass Retention { get; }
    private ArtifactKind(string key, RetentionClass retention) : this(key) => Retention = retention;
}

public sealed record ArtifactIndexRow(
    ArtifactKind Kind,
    string Key,
    ContentAddress Content,
    long Bytes,
    DataClassification Classification,
    string RetentionClass,
    Option<UInt128> SourceKey,
    Instant At) {
    public static ArtifactIndexRow Admit(ArtifactKind kind, string key, ReadOnlySpan<byte> bytes, DataClassification classification, string retentionClass, Instant at, Option<UInt128> sourceKey = default) =>
        new(kind, key, ContentAddress.Of(bytes), bytes.Length, classification, retentionClass, sourceKey, at);

    public static HashMap<UInt128, Seq<ArtifactIndexRow>> Project(Seq<ArtifactIndexRow> rows) =>
        rows.Fold(HashMap<UInt128, Seq<ArtifactIndexRow>>(), static (acc, row) =>
            acc.AddOrUpdate(row.SourceKey.IfNone(row.Content.Value), chain => chain.Add(row), Seq(row)));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content-key name    | kernel `XxHash128` over the bytes      | shared with blob residence + retention; never a second id |
|  [02]   | kind taxonomy       | one `ArtifactKind` row per family      | carries the retention class; no per-kind row type         |
|  [03]   | residence owner     | `Store/blobstore#OBJECT_STORE`         | the index references by content key; never the bytes      |
|  [04]   | source projection   | `Project` groups by `SourceKey`        | GLB + IFC-semantic of one source stay one family          |

## [03]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey` the per-call deterministic cache key (model checksum, input digest, the EP/version/precision result key) with its `Content` fold and stable string form; `ModelResultRow` the indexed residence (content address, the blob `ContentAddress`, the host fingerprint string, the stamp); `ModelResultIndex` the content-addressed recency/dedup index — the SINGLE cross-process result-reuse horizon owner — carrying the `RecencyHorizon` and the `Lookup`/`Publish` reuse ports, with the `Fresh` recency gate; the index is keyed by the suite `XxHash128` content address and never mints a second horizon.
- Cases: a lookup either resolves a fresh residence (within the horizon) or misses; the reuse seam is content-addressed so an inference cache key and a distributed solve sub-block key both fold to one `UInt128` the index resolves identically.
- Entry: `public IO<Option<ModelResultRow>> Lookup(UInt128 content)` resolves a content-addressed residence and `public IO<Option<ModelResultRow>> Lookup(ModelResultKey key)` folds the key first; `public IO<Unit> Publish(ModelResultRow row)` records a computed residence under its content address; `public bool Fresh(Instant at, Instant now)` is the recency gate `now - at <= RecencyHorizon`; the ports are supplied by composition over the Marten lightweight session so the index is read by reference, never embedded.
- Auto: the per-call `ModelResultKey.Content` folds the model checksum, the `UInt128` input digest, and the result-key string into one `XxHash128` content address so an inference run and its dedup probe address identically, and `ToString` is the stable `HybridCache` lane key the upstream inference cache scopes; `Lookup` reads through the synchronous lane (a reuse decision is strong-consistency, never a daemon-lagged async read) and `Fresh`-gates the residence so a result older than the horizon misses and re-computes rather than serving stale; `Publish` records the residence content-addressed so two callers with byte-identical inputs converge on one stored result; the index registers in the `Version/retention#RETENTION_CLASSES` `cache` class so the horizon sweep evicts past the age bound and the one GC governs it.
- Receipt: a reuse hit rides `store.cache.result.hit` carrying the content key, a publish rides `store.cache.result.publish` carrying the content key and blob residence; the index emits no compute fact (the `Runtime/receipts` `Cache`/`Factorization` facts are the upstream Compute lane's, read by reference).
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new reuse dimension is one field folded into `ModelResultKey.Content`; a new residence column is one field on `ModelResultRow`; zero new surface — a second recency horizon, a per-lane dedup owner, a Compute-side result store, or a daemon-lagged reuse read is the deleted form because this is the single horizon owner read through the synchronous lane.
- Boundary: this is the ONE cross-process result-reuse recency horizon — the upstream inference cache (`Model/inference#RESULT_CACHE`), the distributed solve sub-block reuse (`Tensor/factor#SHARD_FANOUT` threads it as the `Blocked.Reuse` column), and the benchmark recency gate (`#BENCHMARK_INDEX` reads `RecencyHorizon`) all read it by reference and a second `Duration horizon` minted beside it is the named defect; the index is content-addressed by the suite `XxHash128` so a sub-block keyed by the streamed-request hash folded with the provider dedup key and an inference key folded from `ModelResultKey` resolve through the same `Lookup`/`Publish` seam, never two dedup owners; the reuse read is the synchronous `Query/lane#READ_ROUTING` lane because serving a stale dedup is a correctness fault, never the async columnar lane; the host fingerprint crosses as a string (the upstream `HostFingerprint.ToString`) so the index holds no Compute type and the strata dependency stays one-directional; `ModelResultKey` carries ONNX-run identity (model/EP/precision) so a non-ONNX content-keyed reuse (a compiled symbolic formula) keys by its own content identity and never fabricates a `ModelResultKey`.

```csharp signature
public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputDigest, string ResultKey) {
    public UInt128 Content {
        get {
            Span<byte> seed = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128LittleEndian(seed, InputDigest);
            return XxHash128.HashToUInt128(
                Encoding.UTF8.GetBytes($"{ModelChecksum}:{ResultKey}"),
                unchecked((long)XxHash3.HashToUInt64(seed)));
        }
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{ModelChecksum}:{InputDigest:x32}:{ResultKey}");
}

public readonly record struct ModelResultRow(UInt128 Content, ContentAddress Residence, string Fingerprint, Instant At);

public sealed record ModelResultIndex(
    Duration RecencyHorizon,
    Func<UInt128, IO<Option<ModelResultRow>>> Resolve,
    Func<ModelResultRow, IO<Unit>> Record) {
    public IO<Option<ModelResultRow>> Lookup(UInt128 content) => Resolve(content);
    public IO<Option<ModelResultRow>> Lookup(ModelResultKey key) => Resolve(key.Content);
    public IO<Unit> Publish(ModelResultRow row) => Record(row);
    public bool Fresh(Instant at, Instant now) => now - at <= RecencyHorizon;
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | reuse horizon       | the ONE `RecencyHorizon` owner         | inference + solve + benchmark read by reference           |
|  [02]   | dedup key           | content-addressed `XxHash128`          | one seam for inference keys and solve sub-blocks          |
|  [03]   | read consistency    | synchronous `Query/lane` lane          | a stale dedup is a correctness fault; never async         |
|  [04]   | fingerprint cross   | `HostFingerprint.ToString` string      | no Compute type; strata stays one-directional             |

## [04]-[BENCHMARK_INDEX]

- Owner: `BenchmarkRow` the durable benchmark-claim index row — the claim key, the winning route, the median/p95 durations, the allocated bytes, the host fingerprint string, the retention-class key, and the stamp — with `Claim` the fingerprint-gated, recency-bounded resolution and `BenchmarkRowClass` the row's default retention class; a claim is a row, never prose, and the row carries no Compute type.
- Cases: a claim either resolves the most-recent fingerprint-matching row within the recency horizon or falls through to `None` (the upstream lane's static cost-rank fallback); a `within` bound gates the resolution against the `ModelResultIndex.RecencyHorizon` so a stale claim never wins.
- Entry: `public static Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string fingerprint, Option<(Duration Horizon, Instant Now)> within = default)` resolves the most-recent row whose fingerprint matches the running host and (when `within` is supplied) whose stamp falls inside the recency horizon; `public static readonly string BenchmarkRowClass` is the row's retention-class key.
- Auto: `Claim` filters the rows to the exact running fingerprint (so a benchmark claimed under managed never wins on a host that resolved native-MKL because the determinism tag drifts the fingerprint string), orders by stamp descending, and takes the head — the `within` bound additionally drops any row older than `Now - Horizon` so the recency horizon read by reference from `ModelResultIndex` gates the speed claim exactly as it gates result reuse; the row registers in the `Version/retention#RETENTION_CLASSES` `cache` class so a re-derivable claim evicts past the age bound and the sweep governs it.
- Receipt: a claim admission rides `store.cache.benchmark` carrying the claim key and fingerprint; the sweep run that produces the claim rows rides the upstream Compute lane's own `TensorRun`/`ModelRun` facts, read by reference, never re-emitted here.
- Packages: NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new claim dimension is one column on `BenchmarkRow`; a new claim key shape is one folded into the upstream `BenchmarkClaim.Key`; zero new surface — a second benchmark store, a profiler add-on owner, or prose performance claims are the deleted form because the claim is a row and the gate is one `Claim` resolution.
- Boundary: the row holds the fingerprint as a STRING (the upstream `HostFingerprint.ToString`) so the benchmark index carries no Compute type and the strata dependency stays one-directional — the upstream `Rasm.Compute` numeric and SIMD lanes compose `Claim` by reference (`Tensor/blas#PROVIDER_CLAIMS` reads the winner against the running fingerprint and `ModelResultIndex.RecencyHorizon`) and a second benchmark store beside this index is the named defect; the claim is fingerprint-gated and recency-bounded so a stale or wrong-host benchmark never wins a route, and the recency horizon is the `ModelResultIndex` owner's, never a second `Duration` minted here; the retention class is the `cache` row because a benchmark claim is re-derivable by re-running the equivalence sweep, so the sweep governs eviction and a never-evict benchmark store is the named defect.

```csharp signature
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
        toSeq(rows
            .Filter(row => StringComparer.Ordinal.Equals(row.Fingerprint, fingerprint))
            .Filter(row => within.Case is (Duration horizon, Instant now) ? now - row.At <= horizon : true)
            .OrderByDescending(static row => row.At))
            .Head;
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | claim gate          | fingerprint-match + most-recent        | a wrong-host or stale claim never wins a route            |
|  [02]   | recency bound       | `ModelResultIndex.RecencyHorizon`      | one horizon owner; never a second `Duration`              |
|  [03]   | fingerprint cross   | `HostFingerprint.ToString` string      | no Compute type; strata stays one-directional             |
|  [04]   | retention class     | `cache` (re-derivable by re-sweep)     | the sweep governs eviction; never never-evict             |
