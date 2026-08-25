# [PERSISTENCE_QUERY_CACHE]

Rasm.Persistence owns one content-addressed artifact index, model-result recency owner, durable solver-memo band, buffer-contract L2 contribution, codec factory, and optional wide-column projection. `ArtifactKind`, composed here off `Version/retention#RETENTION_CLASSES`, carries `CacheTier` into the AppHost runtime lane. `ModelResultIndex` closes `RecencyHorizon` and `IClock`; callers cannot replace freshness policy. `CacheL2Store` persists capped deadlines and tenant-partitioned keys. `IndexResidency` selects `MartenPg | ScyllaWideColumn` without forking admission, identity, retention, or horizon policy, and both rows admit through one `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` cache-lane gate. `CacheProfile` closes the execution-profile roster the cluster declaration and every call site bind by name from, and each wide-column write carries that horizon as a bound `USING TTL` value.

## [01]-[INDEX]

- [02]-[ARTIFACT_BLOB_INDEX]: `ArtifactIndexRow` admits content-keyed under the composed `Version/retention` asset-class axis, and `Project` folds the source-keyed family.
- [03]-[MODEL_RESULT_INDEX]: `ModelResultKey` keys each call, `ModelResultIndex` owns the content-addressed recency/dedup horizon with its gate folded into the lookup, and the lookup/publish seam carries reuse.
- [04]-[BENCHMARK_INDEX]: `BenchmarkFamily` rosters the standing corpus, `BenchmarkRow` carries the durable claim, and `Claim` resolves fingerprint-gated and recency-bounded.
- [05]-[SOLVER_MEMO]: `SolverMemoKind` rosters the content-exact solver producers by key prefix, `SolverMemoRow` persists each memo deadline-free under the `cache` class, and `SolverMemo` owns the band read/write.
- [06]-[L2_CONTRIBUTION]: `IBufferDistributedCache` stores the `Store`-keyed buffer contract, one `IHybridCacheSerializerFactory` mints the MessagePack codec, `TenantId` partitions the content-address key the AppHost cache port resolves over, `CacheLane.Store` gates the Redis invalidation backplane beside it, and the whole leg rides the `#INDEX_RESIDENCY` cache-lane gate.
- [07]-[INDEX_RESIDENCY]: `IndexResidency` axes deployment (`marten-pg` default · `scylla-widecolumn` scale-out), `Admit` gates the whole cache lane at profile selection, `CacheProfile` closes the execution-profile roster, `WideColumnLane` binds the residence once and dials its two verbs across the re-drive seam, LWT claims gate admission under a horizon-derived `CacheTtl`, the `PagingState` sweep pages one partition, and `CacheFault` folds `DriverException` inside the carried attempt.

## [02]-[ARTIFACT_BLOB_INDEX]

- Owner: `ArtifactIndexRow` is the content-keyed residence index, and `Admit` is its sole factory; the asset-class axis it keys on is the `Version/retention#RETENTION_CLASSES` `ArtifactKind` this index COMPOSES, seated there beside the `RetentionClass` it derives because the object-plane catalog reads the same axis one stratum below and a taxonomy two peers reach seats at the lowest stratum either reaches — an index-local kind roster is the deleted fork.
- Cases: each artifact family is one composed `ArtifactKind` row carrying its retention class and lane; a family whose retention DERIVES from provenance uses one selector that reads the producer's discriminant and returns the row beside the origin key `Admit` records — `Texture(Option<UInt128> planKey)` resolves `TextureSet` (press-baked, `Cache`, rebuildable from its recorded graph/plan/seed triple) or `TextureAcquired` (neural-acquired, `Blob`, durable because a retired model card and a drifted execution provider make the bytes unreproducible), while `Representation(RepresentationSlot slot, Option<UInt128> bodyKey)` sends only proven body-derived forms carrying that source to cache and sends every source-less or unreconstructible form to durable preservation with no invented origin.
- Entry: `Admit(ArtifactKind, string, ReadOnlySpan<byte>, DataClassification, Instant, Option<UInt128>)` requires an explicit source-key decision, content-addresses the admitted bytes through `ContentAddress.Of`, and derives `RetentionClass` from `ArtifactKind`; `Project` folds rows into source-keyed families.
- Auto: `Admit` is the single content-addressing path — the `ContentAddress` is the seam `ContentAddress.Of(bytes)` over the artifact bytes (the suite hash law, never a path- or filename-keyed identity and never a second hasher), the byte size records from the admitted span's length (never a later filesystem stat), and a self-keyed artifact carries `None` source while a derived artifact (a GLB tessellated from a source IFC) threads the source IFC's content key as `Some` so the two-projection family stays joined; the source key is the KERNEL seed-zero key over the source bytes (the `Rasm.Bim/Exchange/tessellation#TESSELLATION_BRIDGE` mints it tolerance-independently), NEVER a policy-seeded interchange-cache key, so the GLB and the semantic graph share one origin even across tessellation settings; `Project` groups by `SourceKey.IfNone(Content)` so a self-keyed row projects under its own content and a source-keyed family under its shared origin; a `cloud-run` row keys by `CloudRunKey.Content` — the `(recipe digest · input-asset content keys · project slug)` preimage streamed through the kernel `Domain/identity#CANONICAL_WRITER` `CanonicalWriter` (`String` length-frames each text, `Rows` count-frames the input run, `U128` fixes each key's width) so the framing law has ONE owner rather than a hand-built buffer kept byte-identical to a sibling by inspection, so a re-submitted byte-identical recipe+inputs resolves the SAME row and the prior run's landed assets serve without a cloud round-trip (the SDK's own reuse — `Helper.CheckCached` path-existence, `Wrapper.LocalDatabase` bare SQLite — is verifiably weaker and SUPERSEDED; the run's output-asset bytes travel the `Store/blobstore` presigned-grant row, lineage the `Version/provenance` PROV rows, and no `PollinationSDK` type crosses into this index); the retention class and lane arrive settled from the `ArtifactKind` row so the artifact admits into the `Version/retention#RETENTION_CLASSES` class without a second taxonomy and reads its cache lane without a second routing axis.
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact family is one `ArtifactKind` row AT ITS OWNER (`Version/retention#RETENTION_CLASSES`) and zero edits here; a new index column is one field on `ArtifactIndexRow`; zero new surface — a per-kind row type, an index-local kind roster, a second content-key hash, a path-keyed identity, a `string` retention column beside the typed `RetentionClass`, an origin flag beside the value that already discriminates, or a managed copy of the blob bytes beside the index is the deleted form because the kind axis is the discriminant, its owner is one stratum below, and the object store owns residence.
- Boundary: the index row is content-keyed by the same `XxHash128` the kernel mints and the `Store/blobstore#OBJECT_STORE` object name derives from, so the artifact index, blob residence, and retention catalog share one identity scheme. Bytes land on the blob lane before the index row; a crash can leave a collectible orphan blob, never a dangling index row. `Blob` rows remain reachable from history, `Cache` rows remain re-derivable, and `CacheTier` projects directly to the AppHost L1/L2 lane. Upstream compute lanes compose the settled `ArtifactKind` vocabulary, and texture families enter through `ArtifactKind.Texture` with the producer's plan key determining both retention and `SourceKey`.

```csharp
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using LanguageExt;
using Marten;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Persistence.Element;
using Rasm.Persistence.Store;
using Rasm.Persistence.Version;
using StackExchange.Redis;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CacheFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Cache;
    private CacheFault() { }

    [FaultCase(0)] public sealed partial record InvalidPolicy(string Policy, string Found) : CacheFault;
    [FaultCase(1)] public sealed partial record Unavailable(Error Cause, ConsistencyLevel Level, int Required, int Alive) : CacheFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(2)] public sealed partial record OperationTimedOut(Error Cause) : CacheFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(3)] public sealed partial record ReadTimedOut(Error Cause) : CacheFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(4)] public sealed partial record WriteTimedOut(Error Cause, string WriteType) : CacheFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(5)] public sealed partial record HostDown(Error Cause) : CacheFault, ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(6)] public sealed partial record InvalidQuery(Error Cause) : CacheFault, ICausedFault;
    [FaultCase(7)] public sealed partial record SchemaExists(Error Cause) : CacheFault, ICausedFault;
    [FaultCase(8)] public sealed partial record Foreign(Error Cause) : CacheFault, ICausedFault;

    public override string Message => Switch(
        invalidPolicy:      static c => $"<cache-policy:{c.Policy}:{c.Found}>",
        unavailable:        static c => $"<cache-unavailable:{c.Level}:{c.Required}>{c.Alive}:{c.Cause.Message}>",
        operationTimedOut:  static c => $"<cache-operation-timeout:{c.Cause.Message}>",
        readTimedOut:       static c => $"<cache-read-timeout:{c.Cause.Message}>",
        writeTimedOut:      static c => $"<cache-write-timeout:{c.WriteType}:{c.Cause.Message}>",
        hostDown:           static c => $"<cache-host-down:{c.Cause.Message}>",
        invalidQuery:       static c => $"<cache-invalid-query:{c.Cause.Message}>",
        schemaExists:       static c => $"<cache-schema-exists:{c.Cause.Message}>",
        foreign:            static c => $"<cache-provider:{c.Cause.Message}>");

    public static Error Lift(Error boundary) => boundary.Exception.Case switch {
        UnavailableException error => new Unavailable(boundary, error.Consistency, error.RequiredReplicas, error.AliveReplicas),
        OperationTimedOutException => new OperationTimedOut(boundary),
        ReadTimeoutException => new ReadTimedOut(boundary),
        WriteTimeoutException error => new WriteTimedOut(boundary, error.WriteType),
        NoHostAvailableException => new HostDown(boundary),
        InvalidQueryException => new InvalidQuery(boundary),
        AlreadyExistsException => new SchemaExists(boundary),
        DriverException => new Foreign(boundary),
        _ => boundary,
    };
}

// --- [TYPES] ---------------------------------------------------------------------------

[ValueObject<string>]
public readonly partial struct CacheToken {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' or ':'))) {
            validationError = new ValidationError($"cache:{"token"}:{value}");
        }
    }
}

[ValueObject<int>]
public readonly partial struct CachePageSize {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 1) validationError = new ValidationError($"cache:{"page-size"}:{value.ToString(CultureInfo.InvariantCulture)}");
    }
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct CloudRunKey(string RecipeDigest, Seq<UInt128> InputKeys, string ProjectSlug) {
    public UInt128 Content => ContentHash.Of(this, static (key, writer) => {
        writer.String(key.RecipeDigest)
            .Rows(key.InputKeys, static (input, w) => { w.U128(input); })
            .String(key.ProjectSlug);
    });
}

public sealed record ArtifactIndexRow(
    ArtifactKind Kind,
    string Key,
    ContentAddress Content,
    long Bytes,
    DataClassification Classification,
    Option<UInt128> SourceKey,
    Instant At) {
    public RetentionClass Retention => Kind.Retention;

    public static ArtifactIndexRow Admit(ArtifactKind kind, string key, ReadOnlySpan<byte> bytes, DataClassification classification, Instant at, Option<UInt128> sourceKey) =>
        new(kind, key, ContentAddress.Of(bytes), bytes.Length, classification, sourceKey, at);

    public static HashMap<UInt128, Seq<ArtifactIndexRow>> Project(Seq<ArtifactIndexRow> rows) =>
        rows.Fold(HashMap<UInt128, Seq<ArtifactIndexRow>>(), static (acc, row) =>
            acc.AddOrUpdate(row.SourceKey.IfNone(row.Content.Value), chain => chain.Add(row), Seq(row)));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                     | [BINDING]                                                 |
| :-----: | :---------------- | :------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | content-key name  | kernel `XxHash128` over the bytes           | shared with blob residence + retention; never a second id |
|  [02]   | kind taxonomy     | composed `Version/retention` `ArtifactKind` | one axis; the store catalog reads it one stratum below    |
|  [03]   | residence owner   | `Store/blobstore#OBJECT_STORE`              | the index references by content key; never the bytes      |
|  [04]   | source projection | `Project` groups by kernel `SourceKey`      | GLB + IFC-semantic of one source stay one family          |
|  [05]   | cloud-run reuse   | `CloudRunKey` length-framed fold            | identical recipe+inputs resolve prior assets              |
|  [06]   | texture retention | `ArtifactKind.Texture(planKey)`             | one key answers the class AND the recorded `SourceKey`    |

## [03]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey` the per-call deterministic cache key (model checksum, input digest, the EP/version/precision result key) with its `Content` fold over the seam `ContentAddress` and stable string form; `ModelResultRow` the indexed residence (content address, the blob `ContentAddress`, the host fingerprint string, the stamp); `ModelResultIndex` the content-addressed recency/dedup index — the SINGLE cross-process result-reuse horizon owner — carrying the `RecencyHorizon`, the clock the horizon gate reads, and the `Resolve`/`Record` ports, with the `Lookup`/`Publish` reuse seam folding the horizon gate INTO the resolve; the index is keyed by the suite `XxHash128` content address and never mints a second horizon.
- Cases: a lookup either resolves a residence that is FRESH within the horizon or misses (a stale residence misses by construction, never a separate caller-applied bool); the reuse seam is content-addressed so an inference cache key and a distributed solve sub-block key both fold to one `UInt128` the index resolves identically.
- Entry: `ModelResultIndex.Of` admits a positive `RecencyHorizon`, `IClock`, and ports; `Lookup(UInt128)` and `Lookup(ModelResultKey)` apply the private freshness predicate against `IClock.GetCurrentInstant`; `Publish` records the residence; `Claim` reuses the same horizon and clock.
- Auto: the per-call `ModelResultKey.Content` streams the kernel `Domain/identity#CANONICAL_WRITER` `CanonicalWriter` (`String` length-frames the model checksum and the result key, `U128` fixes the `InputDigest` between them, so a `(checksum, result-key)` split shift can never collide two distinct calls onto one cached result and the framing law lives at its one owner) so an inference run and its dedup probe address identically AND the suite owns the one `XxHash128` — a second hasher beside the seam, or an unframed concatenation that keys distinct inputs alike, is the deleted form; `ToString` is the stable `HybridCache` lane key the `#L2_CONTRIBUTION` content-address partition scopes; `Lookup` reads through the synchronous lane (a reuse decision is strong-consistency, never a daemon-lagged async read), `Resolve`s the residence, then `Fresh`-gates it against the index clock so a result older than the horizon resolves to `None` and re-computes rather than serving stale — the gate is structural, not a documented obligation; `Publish` records the residence content-addressed so two callers with byte-identical inputs converge on one stored result; the index registers in the `Version/retention#RETENTION_CLASSES` `cache` class so the horizon sweep evicts past the age bound and the one GC governs it.
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new reuse dimension is one field folded into `ModelResultKey.Content`'s canonical preimage; a new residence column is one field on `ModelResultRow`; zero new surface — a second recency horizon, a per-lane dedup owner, a Compute-side result store, a caller-applied freshness bool the lookup does not enforce, or a daemon-lagged reuse read is the deleted form because this is the single horizon owner, the gate is folded in, and the read is synchronous.
- Boundary: this is the ONE cross-process result-reuse recency horizon — the upstream inference cache (`Model/run#RESULT_CACHE`), the distributed solve sub-block reuse (`Tensor/factor#KERNEL_LOWERING` threads it as the `Blocked.Reuse` column and reads RESIDENCE only — `Lookup` resolves the dedup-keyed `ModelResultRow`, the object-store port yields the `SolveResponse` bytes at that residence), the benchmark recency gate (`#BENCHMARK_INDEX` reads `RecencyHorizon`), and the cost-formula reuse (`Symbolic/lowering#LOWERING_CACHE` keyed by its OWN content identity, never a fabricated `ModelResultKey`) all read it by reference and a second `Duration horizon` minted beside it is the named defect; the index is content-addressed by the suite `XxHash128` so a sub-block keyed by the streamed-request hash folded with the provider dedup key and an inference key folded from `ModelResultKey` resolve through the same `Lookup`/`Publish` seam, never two dedup owners, and `Publish` records the RESIDENCE row (the index never holds the payload — a 2-arg `Publish(address, payload)` is the deleted phantom); the reuse read is the synchronous `Query/lane#READ_ROUTING` lane because serving a stale dedup is a correctness fault, never the async columnar lane; the freshness gate lives INSIDE `Lookup` so a consumer cannot reuse a stale row by forgetting a bool — the only correct miss-or-hit is the index's own; the host fingerprint crosses as a string (the AppHost-declared `HostFingerprint.ToString`/`DeterminismTag`) so the index holds no spine or upstream type and the strata dependency stays one-directional; `ModelResultKey` carries ONNX-run identity (model/EP/precision) so a non-ONNX content-keyed reuse (a compiled symbolic formula) keys by its own content identity and never fabricates a `ModelResultKey`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputDigest, string ResultKey) {
    public UInt128 Content => ContentHash.Of(this, static (key, writer) => {
        writer.String(key.ModelChecksum).U128(key.InputDigest).String(key.ResultKey);
    });

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{ModelChecksum}:{InputDigest:x32}:{ResultKey}");
}

public readonly record struct ModelResultRow(UInt128 Content, ContentAddress Residence, string Fingerprint, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ModelResultIndex {
    readonly IClock clock;
    readonly Func<UInt128, IO<Option<ModelResultRow>>> resolve;
    readonly Func<ModelResultRow, IO<Unit>> record;

    public Duration RecencyHorizon { get; }

    ModelResultIndex(Duration recencyHorizon, IClock clock, Func<UInt128, IO<Option<ModelResultRow>>> resolve, Func<ModelResultRow, IO<Unit>> record) =>
        (RecencyHorizon, this.clock, this.resolve, this.record) = (recencyHorizon, clock, resolve, record);

    public static Fin<ModelResultIndex> Of(Duration recencyHorizon, IClock clock, Func<UInt128, IO<Option<ModelResultRow>>> resolve, Func<ModelResultRow, IO<Unit>> record) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(recencyHorizon > Duration.Zero, "recency-horizon", recencyHorizon, CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(clock is not null, "clock", "<null>", CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(resolve is not null, "resolve", "<null>", CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(record is not null, "record", "<null>", CacheFault.InvalidPolicy)))
        .Map(_ => new ModelResultIndex(recencyHorizon, clock, resolve, record))
        .ToFin();

    public IO<Option<ModelResultRow>> Lookup(UInt128 content) =>
        resolve(content).Map(found => found.Filter(row => Fresh(row.At)));

    public IO<Option<ModelResultRow>> Lookup(ModelResultKey key) => Lookup(key.Content);
    public IO<Unit> Publish(ModelResultRow row) => record(row);
    public Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string fingerprint) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.Fingerprint, fingerprint) && Fresh(row.At))
            .Fold(Option<BenchmarkRow>.None, static (best, row) => best.Case is BenchmarkRow held && held.At >= row.At ? best : row);

    bool Fresh(Instant at) => clock.GetCurrentInstant() - at <= RecencyHorizon;
}
```

| [INDEX] | [POLICY]          | [VALUE]                                | [BINDING]                                                          |
| :-----: | :---------------- | :------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | reuse horizon     | the ONE `RecencyHorizon` owner         | inference + solve + benchmark + formula read by reference          |
|  [02]   | horizon gate      | folded INTO `Lookup` against the clock | a stale row misses; never a caller-applied bool                    |
|  [03]   | dedup key         | seam `ContentAddress.Of` (`XxHash128`) | one seam for inference keys and solve sub-blocks; no second hasher |
|  [04]   | read consistency  | synchronous `Query/lane` lane          | a stale dedup is a correctness fault; never async                  |
|  [05]   | residence-only    | `Publish` records the row, not bytes   | the object store owns the payload; no 2-arg phantom                |
|  [06]   | fingerprint cross | `HostFingerprint.ToString` string      | decode-only wire; no spine type; strata stays one-directional      |

## [04]-[BENCHMARK_INDEX]

- Owner: `BenchmarkRow` carries a durable benchmark observation and derives `RetentionClass.Cache`; `BenchmarkFamily` the standing corpus roster — one row per hot-path family naming its subject owner and its claim-key prefix, so the folder's performance claims are a closed vocabulary the index admits, never review intuition; `ModelResultIndex.Claim` owns fingerprint and recency admission through the closed horizon and clock.
- Cases: `BenchmarkFamily` rows are `Codec` (subject `SnapshotCodec` — chunk, compress, hash), `StoreAppend` (subject `GraphStoreOp` — append and AS-OF fold), `Merge` (subject `StructuralMerge` — three-way structural merge), `Columnar` (subject `ColumnarLane` — analytical aggregate), `VectorRoute` (subject `VectorCodebook` — ANN route), `Multipart` (subject `MultipartTransfer` — blob multipart transfer); `ModelResultIndex.Claim(rows, fingerprint)` returns the newest matching live row or `None`; no call shape can omit or replace the index horizon and clock.
- Entry: `ModelResultIndex.Claim(Seq<BenchmarkRow>, string)` filters and folds once; `BenchmarkFamily.Claim(CacheToken, …)` returns `Fin<BenchmarkRow>` from the sole row mint and derives `BenchmarkRow.Key`, so every key is family-owned; `BenchmarkRow.Retention` supplies `RetentionClass.Cache`.
- Auto: the mint admits nonnegative median, allocation, and operation measurements, orders P95 at or above median, and requires a nonblank case token, route, and fingerprint before construction — the zero-init struct case ghost and a blank route both refuse before the `{Suite}/{Case}/{Route}` identity forms; `Claim` filters admitted rows to the exact running fingerprint (so a benchmark claimed under managed never wins on a host that resolved native-MKL because the `DeterminismTag` drifts the fingerprint string) and the horizon bound in one pass, then folds to the latest-`At` survivor through one `MostRecent` reduction (never a full `OrderByDescending` materialization, the recency horizon read by reference from `ModelResultIndex` gating the speed claim exactly as it gates result reuse — an optional bound whose absence retained every row was the horizon bypass the mandatory pair deletes); the row registers in the `Version/retention#RETENTION_CLASSES` `cache` class so a re-derivable claim evicts past the age bound and the sweep governs it.
- Packages: NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new claim dimension is one column on `BenchmarkRow`; a new claim key shape is one folded into the upstream `BenchmarkClaim.Key`; zero new surface — a second benchmark store, a profiler add-on owner, or prose performance claims are the deleted form because the claim is a row and the gate is one `Claim` resolution.
- Boundary: `BenchmarkRow` is the persisted claim minted from the upstream `BenchmarkClaim`: measurement and identity columns persist (`Median`, `P95`, `AllocatedBytes`, `Operations`, `Corpus`, `ArtifactKey`), while transient judgment and correlation do not. `BenchmarkRow` holds the AppHost-declared fingerprint render as a string, keeping the dependency one-directional. Fingerprint and recency gates prevent stale or wrong-host claims from selecting a route, and `RetentionClass.Cache` leaves eviction to the standing sweep.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchmarkFamily {
    public static readonly BenchmarkFamily Codec       = new("codec", nameof(SnapshotCodec), "chunk/compress/hash over the canonical snapshot bytes");
    public static readonly BenchmarkFamily StoreAppend = new("store-append", nameof(GraphStoreOp), "delta append and AS-OF reconstruction fold");
    public static readonly BenchmarkFamily Merge       = new("merge", nameof(StructuralMerge), "three-way structural merge over graph structure");
    public static readonly BenchmarkFamily Columnar    = new("columnar", nameof(ColumnarLane), "analytical aggregate over the in-process engine");
    public static readonly BenchmarkFamily VectorRoute = new("vector-route", "VectorCodebook", "PQ/ADC ANN routing over the retrieval codebook");
    public static readonly BenchmarkFamily Multipart   = new("multipart", nameof(MultipartTransfer), "resumable multipart blob transfer");

    public string Subject { get; }
    public string Measures { get; }
    private BenchmarkFamily(string key, string subject, string measures) : this(key) => (Subject, Measures) = (subject, measures);

    public Fin<BenchmarkRow> Claim(CacheToken @case, string route, Duration median, Duration p95, long allocatedBytes,
        long operations, Option<UInt128> corpus, Option<string> artifactKey, string fingerprint, Instant at) =>
        BenchmarkRow.Mint(this, @case, route, median, p95, allocatedBytes, operations, corpus, artifactKey, fingerprint, at);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record BenchmarkRow {
    BenchmarkRow(string key, string route, Duration median, Duration p95, long allocatedBytes, long operations,
        Option<UInt128> corpus, Option<string> artifactKey, string fingerprint, Instant at) =>
        (Key, Route, Median, P95, AllocatedBytes, Operations, Corpus, ArtifactKey, Fingerprint, At) =
        (key, route, median, p95, allocatedBytes, operations, corpus, artifactKey, fingerprint, at);

    public string Key { get; }
    public string Route { get; }
    public Duration Median { get; }
    public Duration P95 { get; }
    public long AllocatedBytes { get; }
    public long Operations { get; }
    public Option<UInt128> Corpus { get; }
    public Option<string> ArtifactKey { get; }
    public string Fingerprint { get; }
    public Instant At { get; }
    public RetentionClass Retention => RetentionClass.Cache;

    internal static Fin<BenchmarkRow> Mint(BenchmarkFamily family, CacheToken @case, string route, Duration median, Duration p95,
        long allocatedBytes, long operations, Option<UInt128> corpus, Option<string> artifactKey, string fingerprint, Instant at) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(!string.IsNullOrWhiteSpace((string)@case), "case", "<default>", CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(!string.IsNullOrWhiteSpace(route), "route", route ?? "<null>", CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(median >= Duration.Zero, "median", median, CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(median >= Duration.Zero && p95 >= median, "p95", p95, CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(allocatedBytes >= 0, "allocated-bytes", allocatedBytes, CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(operations >= 0, "operations", operations, CacheFault.InvalidPolicy),
            AdmissionSlots.Gate(!string.IsNullOrWhiteSpace(fingerprint), "fingerprint", fingerprint ?? "<null>", CacheFault.InvalidPolicy)))
        .Map(_ => new BenchmarkRow(
            string.Create(CultureInfo.InvariantCulture, $"{family.Key}/{(string)@case}/{route}"),
            route, median, p95, allocatedBytes, operations, corpus, artifactKey, fingerprint, at))
        .ToFin();
}
```

| [INDEX] | [POLICY]        | [VALUE]                              | [BINDING]                                      |
| :-----: | :-------------- | :----------------------------------- | :--------------------------------------------- |
|  [01]   | claim gate      | fingerprint-match + latest survivor  | a wrong-host or stale claim never wins a route |
|  [02]   | recency bound   | closed index horizon and clock       | `ModelResultIndex.Of`; no bypass shape         |
|  [03]   | head fold       | one `MostRecent` reduction           | no full `OrderByDescending` materialization    |
|  [04]   | retention class | `cache` (re-derivable by re-sweep)   | the sweep governs eviction; never never-evict  |
|  [05]   | corpus roster   | one `BenchmarkFamily` row per family | typed case mint derives the complete row key   |

## [05]-[SOLVER_MEMO]

- Owner: `SolverMemoKind` the closed producer roster carrying each lane's key prefix and producing identity; `SolverMemoRow` the durable content-keyed memo row; `SolverMemo` the band read/write.
- Cases: `Nfp` — the Fabrication pair-matrix polygons keyed by `PairTable.Key`/`InnerKey` over pair geometry, tolerance, rotation, clearance, kerf, and chord error; `Icp` — the registration fits keyed by `ProbeMemo.Key` over both point sets, the align kind, the policy columns, and the context tolerances; each row names its producer identity, so the band stores foreign solver truth without re-deriving it.
- Entry: `SolverMemo.Get(SolverMemoKind kind, UInt128 identity)` is the synchronous-lane read a warm start blocks on; `Put(SolverMemoRow row)` upserts by identity, so a byte-identical re-publish is a no-op.
- Auto: federation is the LANDED `HybridCache` path — the Fabrication memo lanes ride the branch `HybridCache` whose L2 is this folder's `#L2_CONTRIBUTION` `IBufferDistributedCache`, so the band opens NO cross-package seam: the L2 store dispatches on the key prefix through `SolverMemoKind.Of`, a solver-memo key persists as a `SolverMemoRow` with no deadline while every other key keeps the capped-deadline row, and a cross-run or cross-process warm start is an ordinary L2 hit; solver truth is content-exact — the producer key folds EVERY input that shifts the result, so an input change IS a new key, staleness is unspellable, and no recency horizon applies.
- Packages: Marten (`IDocumentStore`), MessagePack (`MessagePackSerializer`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new solver memo lane is one `SolverMemoKind` row — prefix and producer identity — beside the producer-side content key and cache ride; zero new surface here.
- Boundary: identity is the PRODUCER's content key — this band mints no key and decodes no payload, so solver truth round-trips opaque and only its producer reads it; content-exactness is what deletes the recency horizon — the `#MODEL_RESULT_INDEX` horizon governs results whose identity is not total over inputs, and importing it here expires truth that cannot stale; rows derive `RetentionClass.Cache`, so the `Version/retention#SWEEP_AND_GC` sweep alone evicts by budget; the S2-peer law holds whole — Fabrication never references this package, the composition root binds the `HybridCache` L2, and the prefix dispatch is the entire federation seam; tenant scoping rides the same `CachePartition.Scoped` derivation every L2 row takes.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolverMemoKind {
    public static readonly SolverMemoKind Nfp = new("nfp", "PairTable.Key/InnerKey");
    public static readonly SolverMemoKind Icp = new("icp", "ProbeMemo.Key");

    public string Producer { get; }
    public string Prefix => $"{Key}:";
    private SolverMemoKind(string key, string producer) : this(key) => Producer = producer;

    public static Option<SolverMemoKind> Of(string cacheKey) =>
        toSeq(Items).Find(kind => cacheKey.StartsWith(kind.Prefix, StringComparison.Ordinal));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SolverMemoRow(SolverMemoKind Kind, UInt128 Identity, ReadOnlyMemory<byte> Payload, Instant At) {
    public RetentionClass Retention => RetentionClass.Cache;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolverMemo {
    public static IO<Option<SolverMemoRow>> Get(SolverMemoKind kind, UInt128 identity, Func<SolverMemoKind, UInt128, IO<Option<SolverMemoRow>>> resolve) =>
        resolve(kind, identity);

    public static IO<Unit> Put(SolverMemoRow row, Func<SolverMemoRow, IO<Unit>> record) => record(row);
}
```

| [INDEX] | [POLICY]        | [VALUE]                             | [BINDING]                                                |
| :-----: | :-------------- | :---------------------------------- | :------------------------------------------------------- |
|  [01]   | identity        | the producer's content key          | the band mints none; payload stays producer-opaque       |
|  [02]   | freshness       | content-exact, no horizon           | an input change IS a new key; staleness unspellable      |
|  [03]   | federation      | L2 prefix dispatch                  | zero cross-package seam; composition binds the L2        |
|  [04]   | retention class | `cache` — budget-swept, no deadline | the budget sweep governs; a TTL deletes replayable truth |

## [06]-[L2_CONTRIBUTION]

- Owner: `CacheL2Store` contributes the Marten-backed `IBufferDistributedCache`; `CacheCodecFactory` contributes one MessagePack serializer factory; `CachePartition` derives tenant-scoped keys; `CacheBackplane` owns Redis beat and RESP3 tracking invalidation through one `InvalidationMode` drain; AppHost retains the `HybridCache` L1, stampede, and tag owner.
- Cases: the L2 store backs every lane whose `CacheLane.Store` is set (`ModelResult`, `Projection` on `durable-l2`) while a lane with no `Store` (`ArtifactBlob`) resolves the default `HybridCache` with no L2 leg; the codec factory yields a `CacheCodec<T>` for every payload `T` from one MessagePack pass, so a `ModelResultRow`, a `Cached<Fin<T>>` typed envelope, and a projection document round-trip under one registered factory.
- Entry: `CacheL2Store.Partition(StoreOptions)` publishes the `cache-blob` `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` mapping contribution the composition root folds over the spine seat; `TryGetAsync` rejects expired rows and refreshes a sliding deadline beneath its absolute cap before writing the supplied buffer; `SetAsync` converts nullable platform options into internal `Option` state; `RefreshAsync` advances the same deadline; `CachePartition.Scoped` derives the tenant partition; `Configure` selects RESP3, `EnableTracking` issues `CLIENT TRACKING ON BCAST PREFIX`, and `Drain` folds beat tags or `__redis__:invalidate` keys into the matching `HybridCache` invalidation operation.
- Auto: `IBufferDistributedCache` routes payloads through pooled writers and `ReadOnlySequence<byte>`; `CacheBlob` materializes one durable `byte[]` and stores deadline absence as `Option`. `TryGetAsync` rejects `ExpiresAt <= now()` and renews sliding rows. `SetAsync` derives the earliest absolute, relative, or sliding deadline; both verbs FIRST route the key through `#SOLVER_MEMO` `SolverMemoKind.Of` — a solver-memo prefix reads and writes the durable band's `SolverMemoRow` (deadline-free, the caller's expiration options discarded because content-exact truth cannot stale) while every other key keeps the capped-deadline row. `CacheCodec<T>` uses `SnapshotCodec.Binary`. `Scoped` digests the injected tenant through `ContentAddress.Of`.
- Packages: Microsoft.Extensions.Caching.Hybrid (`IBufferDistributedCache`/`IHybridCacheSerializer<T>`/`IHybridCacheSerializerFactory`/`HybridCache.RemoveAsync`/`RemoveByTagAsync`), Marten (`IDocumentStore`; `StoreOptions.Schema.For<T>().PartitionOn` through `RollingWindow.Declare`), MessagePack (`MessagePackSerializer`), StackExchange.Redis (`ConfigurationOptions.Protocol`/`RedisProtocol.Resp3`/`IConnectionMultiplexer.GetDatabase`/`GetSubscriber`/`IDatabase.ExecuteAsync`/`ISubscriber.SubscribeAsync`/`ChannelMessageQueue`/`RedisChannel`), Rasm.Element (`ContentAddress`), LanguageExt.Core, BCL inbox.
- Growth: a new L2 topology is one composition row; a new payload type uses the existing factory; a new invalidation posture is one `InvalidationMode` case. Redis deployment composes `Configure`, `EnableTracking`, and `Drain(Tracking, token)`; beat deployments use `Drain(Beat, token)`.
- Boundary: Persistence contributes exactly ONE L2 store row (the `IBufferDistributedCache` buffer-contract storage that spares the cache-runtime intermediate-array copy, persisting one `byte[]` at the Marten document seam) and ONE `IHybridCacheSerializerFactory` (the MessagePack codec for every payload `T`), registered through the AppHost `CacheSurface.Register(services, contributed)` `AddSerializerFactory` on every keyed builder, never a per-type `AddSerializer<T>`; the AppHost `HybridCache` runtime composes ON TOP — `GetOrCreateAsync` drives stampede-protected single-flight population, `RemoveByTagAsync` cuts a lane by its key tag, and the `HybridCacheEntryFlags` lane axis (`DisableLocalCache` on the `ArtifactBlob` lane so an oversized GLB never pins L1, `None` on the `ModelResult` lane) is the per-lane L1/L2 routing — so the L1+stampede+tag-invalidation half is the AppHost port's and the L2-store+serializer half is this contribution, one cache owner across both and never a second; the L2 wire is the `messagepack` `SnapshotCodec.Binary` row so the durable cache bytes and the snapshot/event bytes share one codec and one `Instant` formatter, never a cache-local serializer; the content-address key partitions by `TenantId` through `Scoped` so the `#MODEL_RESULT_INDEX` `ModelResultKey.ToString` lane key and the `#ARTIFACT_BLOB_INDEX` content key both read one tenant-scoped identity exactly as `Element/identity#ELEMENT_IDENTITY` scopes the durable row by `current_setting('rasm.tenant', true)` over the kernel's canonical tenant text; tag invalidation is an explicit cache capability and never substitutes for durable store integrity — a tag cut is a logical miss-until-expiry, the `RemoveAsync` physical delete its sibling, and the durable reuse rows live on the retention sweep, not the cache TTL; the backplane is LOSSY BY DESIGN — a missed beat is a TTL-bounded stale read, never corruption (the presence-lane precedent), because correctness lives in the durable index rows and the runtime's self-describing expiry message envelope, so the beat channel is a latency optimization the deployment composes only where the Redis `Store` row is live; hardening the backplane into a delivery guarantee creates a second reliability owner beside `Version/egress`, the deleted form; the durable rows here are Marten documents and the backplane needs a live Redis row, so neither realizes on a single-process embedded store — `#INDEX_RESIDENCY` `Admit` is the ONE cache-lane gate covering both halves, refusing at profile selection rather than at the first `TryGetAsync`.

```csharp
// --- [SERVICES] ------------------------------------------------------------------------

public sealed class CacheL2Store(IDocumentStore store, CacheToken storeKey, Func<DateTimeOffset> now) : IBufferDistributedCache {
    public static StoreOptions Partition(StoreOptions opts) =>
        RollingWindow.CacheBlob.Declare<CacheBlob>(opts, static row => row.Window);

    public bool TryGet(string key, IBufferWriter<byte> destination) => TryGetAsync(key, destination).AsTask().GetAwaiter().GetResult();

    public async ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) {
        await using IDocumentSession session = store.LightweightSession();
        CacheBlob? row = await session.LoadAsync<CacheBlob>(Physical(key), token).ConfigureAwait(false);
        DateTimeOffset stamp = now();
        if (row is null || row.ExpiresAt.Match(Some: expiresAt => expiresAt <= stamp, None: static () => false)) { return false; }
        if (row.SlidingExpiration.IsSome) {
            CacheBlob refreshed = row with { ExpiresAt = Deadline(stamp, row.AbsoluteExpiration, None, row.SlidingExpiration) };
            session.Store(refreshed);
            await session.SaveChangesAsync(token).ConfigureAwait(false);
        }
        destination.Write(row.Payload.Span);
        return true;
    }

    public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).AsTask().GetAwaiter().GetResult();

    public async ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default) {
        await using IDocumentSession session = store.LightweightSession();
        DateTimeOffset stamp = now();
        session.Store(new CacheBlob(
            Physical(key),
            value.ToArray(),
            stamp,
            Optional(options.AbsoluteExpiration),
            Deadline(stamp, Optional(options.AbsoluteExpiration), Optional(options.AbsoluteExpirationRelativeToNow), Optional(options.SlidingExpiration)),
            Optional(options.SlidingExpiration)));
        await session.SaveChangesAsync(token).ConfigureAwait(false);
    }

    public byte[]? Get(string key) { ArrayBufferWriter<byte> writer = new(); return TryGet(key, writer) ? writer.WrittenSpan.ToArray() : null; }
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default) { ArrayBufferWriter<byte> writer = new(); return await TryGetAsync(key, writer, token).ConfigureAwait(false) ? writer.WrittenSpan.ToArray() : null; }
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => Set(key, new ReadOnlySequence<byte>(value), options);
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => SetAsync(key, new ReadOnlySequence<byte>(value), options, token).AsTask();
    public void Refresh(string key) => RefreshAsync(key).GetAwaiter().GetResult();
    public async Task RefreshAsync(string key, CancellationToken token = default) {
        await using IDocumentSession session = store.LightweightSession();
        CacheBlob? row = await session.LoadAsync<CacheBlob>(Physical(key), token).ConfigureAwait(false);
        if (row is null || row.SlidingExpiration.IsNone) { return; }
        session.Store(row with { ExpiresAt = Deadline(now(), row.AbsoluteExpiration, None, row.SlidingExpiration) });
        await session.SaveChangesAsync(token).ConfigureAwait(false);
    }
    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();
    public async Task RemoveAsync(string key, CancellationToken token = default) { await using IDocumentSession session = store.LightweightSession(); session.Delete<CacheBlob>(Physical(key)); await session.SaveChangesAsync(token).ConfigureAwait(false); }

    string Physical(string key) => $"{storeKey}:{key}";

    static Option<DateTimeOffset> Deadline(DateTimeOffset stamp, Option<DateTimeOffset> absolute, Option<TimeSpan> relative, Option<TimeSpan> sliding) {
        Option<DateTimeOffset> rolling = relative.Match(
            Some: window => Some(stamp + window),
            None: () => sliding.Map(window => stamp + window));
        return (absolute.Case, rolling.Case) switch {
            (DateTimeOffset cap, DateTimeOffset candidate) => Some(cap <= candidate ? cap : candidate),
            (DateTimeOffset cap, _) => Some(cap),
            (_, DateTimeOffset candidate) => Some(candidate),
            _ => None,
        };
    }
}

public sealed record CacheBlob(string Id, byte[] Payload, DateTimeOffset Window, Option<DateTimeOffset> AbsoluteExpiration, Option<DateTimeOffset> ExpiresAt, Option<TimeSpan> SlidingExpiration);

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InvalidationMode {
    private InvalidationMode() { }
    public sealed record Beat : InvalidationMode;
    public sealed record Tracking : InvalidationMode;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CachePartition {
    public static string Scoped(CacheTier tier, TenantId tenant, UInt128 content) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{tier.Key}:{ContentHash.Of(tenant, static (id, w) => { w.U128(id.Value); }):x32}:{content:x32}");
}

public sealed class CacheBackplane(IConnectionMultiplexer connection, HybridCache cache, CacheToken storeKey, TenantId tenant) {
    public RedisChannel Channel =>
        RedisChannel.Literal(string.Create(CultureInfo.InvariantCulture, $"rasm.cache.{storeKey}:{tenant.Text}"));

    public static ConfigurationOptions Configure(ConfigurationOptions options) {
        options.Protocol = RedisProtocol.Resp3;
        return options;
    }

    public IO<Unit> EnableTracking() =>
        Captured(async () => {
            _ = await connection.GetDatabase().ExecuteAsync(
                "CLIENT", "TRACKING", "ON", "BCAST", "PREFIX", $"{storeKey}:").ConfigureAwait(false);
            return unit;
        });

    public IO<Unit> Publish(CacheToken laneTag) =>
        Captured(async () => { _ = await connection.GetSubscriber().PublishAsync(Channel, (string)laneTag).ConfigureAwait(false); return unit; });

    public IO<Unit> Drain(InvalidationMode mode, CancellationToken token) =>
        Captured(async () => {
            ISubscriber subscriber = connection.GetSubscriber();
            RedisChannel channel = mode.Switch(
                beat: _ => Channel,
                tracking: _ => RedisChannel.Literal("__redis__:invalidate"));
            ChannelMessageQueue queue = await subscriber.SubscribeAsync(channel).ConfigureAwait(false);
            await foreach (ChannelMessage beat in queue.WithCancellation(token).ConfigureAwait(false)) {
                await mode.Switch(
                    beat: _ => beat.Message.HasValue
                        ? cache.RemoveByTagAsync((string)beat.Message).AsTask()
                        : Task.CompletedTask,
                    tracking: _ => beat.Message.HasValue
                        ? cache.RemoveAsync(LogicalKey((string)beat.Message)).AsTask()
                        : cache.RemoveByTagAsync((string)storeKey).AsTask()).ConfigureAwait(false);
            }
            return unit;
        });

    static IO<T> Captured<T>(Func<Task<T>> crossing) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => Fin<T>.Succ(await crossing().ConfigureAwait(false))).ConfigureAwait(false))
            .Bind(IO.liftFin);

    string LogicalKey(string physical) {
        string prefix = $"{storeKey}:";
        return physical.StartsWith(prefix, StringComparison.Ordinal) ? physical[prefix.Length..] : physical;
    }
}
```

| [INDEX] | [POLICY]               | [VALUE]                                        | [BINDING]                                                     |
| :-----: | :--------------------- | :--------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | one L2 store           | `IBufferDistributedCache` buffer contract      | bare `IDistributedCache` forces an extra runtime-copy         |
|  [02]   | one serializer         | `IHybridCacheSerializerFactory`                | the `messagepack` `SnapshotCodec.Binary` row                  |
|  [03]   | tenant partition       | `Scoped` over `TenantId` digest                | a cross-tenant L2 bucket collision is unrepresentable         |
|  [04]   | lane L1/L2 routing     | the AppHost `HybridCacheEntryFlags`            | `DisableLocalCache` on the blob lane                          |
|  [05]   | one cache owner        | L2+codec here, L1+stampede+tag AppHost         | composed at `CacheSurface.Register`; never a second owner     |
|  [06]   | invalidation backplane | `Beat` tags / RESP3 tracking keys              | `RemoveByTagAsync` / `RemoveAsync`; null tracking flushes tag |
|  [07]   | GH-plugin root profile | raster-byte `IHybridCacheSerializer` admission | `MaximumPayloadBytes` sized to the largest canvas raster      |
|  [08]   | GH-plugin tag metering | `ReportTagMetrics = true`                      | `gh-doc:{documentId:N}` the per-document dimension            |

## [07]-[INDEX_RESIDENCY]

- Owner: `IndexResidency` is the closed `MartenPg | ScyllaWideColumn` deployment family and `Admit` its profile gate. `CacheProfile` closes the execution-profile roster the `Builder` declaration and every call site share; `CacheTtl` carries the horizon as CQL seconds. `WideColumnRow` mirrors the content index. `WideColumnIndex` owns the composition-time lane gate and mapping declaration; `WideColumnLane` binds the mapper, re-drive port, cluster instance, and horizon TTL once.
- Cases: `IndexResidency` rows are `MartenPg` and `ScyllaWideColumn`, each carrying its `Degrade` clause; `CacheProfile` rows are `Root`, `Claim`, and `Sweep`; `CacheFault` closes policy admission, availability, operation/read/write timeout, host, invalid-query, schema-exists, and the cause-bearing provider tail on offsets `0`–`8`.
- Entry: `Admit(StoreProfile, IndexResidency)` gates the cache lane at profile selection; `Declare(IExecutionProfileOptions)` folds the roster onto the `Builder`; `Register` declares the `Map<WideColumnRow>` correspondence; `WideColumnLane.Claim(WideColumnRow)` returns the driver's `AppliedInfo<WideColumnRow>`; `WideColumnLane.Sweep(TenantId, ArtifactKind, CachePageSize, Option<byte[]>)` returns the driver's `IPage<WideColumnRow>` with its `PagingState`. Both cross through `Dialed`, which lifts provider faults inside the carried attempt.
- Auto: the residence is a projection — the `#ARTIFACT_BLOB_INDEX` `Admit` and `#MODEL_RESULT_INDEX` `Publish` paths stay THE admission owners and the scylla residence receives the SAME rows through `Claim`, so identity, retention, and the recency horizon never fork by residence; the recency horizon RIDES THE WRITE as `USING TTL ?`, a bound parameter the mapper's own `int? ttl` slot carries, derived once through `CacheTtl.Of(index.RecencyHorizon)` at composition so the residence expires exactly where the substrate's freshness gate already misses and no call site spells a second number; the claim binds `insertNulls: false`, so an absent `SourceKey` writes no column rather than a tombstone the sweep must later read past; consistency, retry, speculation, and timeout variance is `CacheProfile` row DATA declared ONCE and selected per query by name, routing is `TokenAwarePolicy` over the shard-aware default so a point lookup reaches the owning replica's owning shard, statements are PREPARED only, and the `Cluster`/`ISession` is a composition-root singleton — connection input, never a fence member; `DriverException` lifts ONCE at this boundary through `CacheFault.Lift` discriminated on the exception family, never message substrings, and it lifts INSIDE the carried attempt so the executor above reads a typed posture rather than a bare exception every policy must call terminal.
- Law: the execution profile and row TTL are not call-site arguments. `Claim` selects `cache-claim`, `Sweep` selects `cache-sweep`, and the TTL derives once from the substrate horizon.
- Law: this lane's whole fault estate rides one direct `CacheFault` union over `FaultBand.Cache`; generated identity proves all nine offsets unique and in-span.
- Packages: ScyllaDBCSharpDriver (`Cluster`/`Builder.WithExecutionProfiles`/`ISession`/`Cassandra.Mapping` `Mapper.InsertIfNotExistsAsync(poco, executionProfile, insertNulls, ttl, CqlQueryOptions)`/`FetchPageAsync`/`Cql.New`/`WithExecutionProfile`/`WithOptions`/`CqlQueryOptions.SetPageSize`/`SetPagingState`/`MappingConfiguration`/`Map<T>`/`AppliedInfo<T>`/`IPage<T>`/`IExecutionProfileOptions.WithProfile`/`WithDerivedProfile`/`IExecutionProfileBuilder` six members/`FallthroughRetryPolicy.Instance`/`NoSpeculativeExecutionPolicy.Instance`/`TokenAwarePolicy`/`DefaultLoadBalancingPolicy`/`ConsistencyLevel`/`UnavailableException.Consistency`/`RequiredReplicas`/`AliveReplicas`/`WriteTimeoutException.WriteType`/`DriverException` family — assembly `ScyllaDB`, namespace `Cassandra.*`, netstandard2.0 floor: `Task`-based rows, `IPage<T>`+`byte[]` paging, no span/`IAsyncEnumerable` row API to pretend at), Rasm (`Rasm/Domain/rails#FAULT_BAND` `FaultBand`/`[FaultCase]`/`Fault`/`Retriability`), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits`, `Store/redrive#REDRIVE_SEAM` `StoreHop`/`ColumnVerb`/`StoreRedrivePort`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new residence is one `IndexResidency` row carrying its provider surface behind the same `Claim`/`Sweep` verbs; a new consistency or timeout posture is one `CacheProfile` row naming its base, declared and bound from the same value; a new provider refusal is one `CacheFault` case; zero new surface — a second admission path beside `Admit`/`Publish`, a scylla-side recency horizon, a scylla event stream, an unprepared inline-CQL statement, a profile name cast from free text at a call site, a TTL literal beside the horizon derivation, a per-call consistency branch beside the named profiles, or a `LOGGED`-vs-`UNLOGGED` batch conflation is the deleted form because the residence is a projection of the one index, the profiles are policy rows, and the claim gate is the one write-once admission.
- Boundary: the wide-column row is a rebuildable projection of the Marten substrate under the same kernel content identity. `InsertIfNotExistsAsync` returns `AppliedInfo<T>` so callers can distinguish an applied claim from the existing row without a second result family; `FetchPageAsync` returns `IPage<T>` so the provider paging state remains intact. `Serial` and `LocalSerial` remain LWT consistency, separate from read quorum. `StoreRedrivePort` keeps re-drive behind the pinned fallthrough retry and no-speculation policies, while `CacheFault.Lift` preserves typed availability failures. Profile selection refuses the entire lane when the store profile does not admit it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IndexResidency {
    public static readonly IndexResidency MartenPg = new("marten-pg",
        "none — the substrate answers every predicate the index declares, at single-cluster reach");
    public static readonly IndexResidency ScyllaWideColumn = new("scylla-widecolumn",
        "no cross-partition predicate, no ad-hoc filter, no server-side fold: a read names (tenant, kind) then narrows on the clustering prefix, and every other question re-enters through the Marten substrate the residence projects");
    public string Degrade { get; }
    private IndexResidency(string key, string degrade) : this(key) => Degrade = degrade;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheProfile {
    public static readonly CacheProfile Root = new("default");
    public static readonly CacheProfile Claim = new("cache-claim", Root, ConsistencyLevel.LocalQuorum, ConsistencyLevel.LocalSerial, readTimeoutMillis: 12_000);
    public static readonly CacheProfile Sweep = new("cache-sweep", Root, ConsistencyLevel.LocalQuorum, ConsistencyLevel.LocalSerial, readTimeoutMillis: 30_000);

    public Option<CacheProfile> Base { get; }
    public Option<ConsistencyLevel> Consistency { get; }
    public Option<ConsistencyLevel> Serial { get; }
    public Option<int> ReadTimeoutMillis { get; }

    public IExtendedRetryPolicy Retry => FallthroughRetryPolicy.Instance;
    public ISpeculativeExecutionPolicy Speculative => NoSpeculativeExecutionPolicy.Instance;

    private CacheProfile(string key, CacheProfile seat, ConsistencyLevel consistency, ConsistencyLevel serial, int readTimeoutMillis) : this(key) =>
        (Base, Consistency, Serial, ReadTimeoutMillis) = (Some(seat), Some(consistency), Some(serial), Some(readTimeoutMillis));

    public static IExecutionProfileOptions Declare(IExecutionProfileOptions options) =>
        toSeq(Items).Fold(options, static (acc, row) => row.Base.Match(
            Some: seat => acc.WithDerivedProfile(row.Key, seat.Key, row.Compose),
            None: () => acc));

    void Compose(IExecutionProfileBuilder builder) {
        _ = builder.WithRetryPolicy(Retry).WithSpeculativeExecutionPolicy(Speculative);
        Consistency.IfSome(level => _ = builder.WithConsistencyLevel(level));
        Serial.IfSome(level => _ = builder.WithSerialConsistencyLevel(level));
        ReadTimeoutMillis.IfSome(millis => _ = builder.WithReadTimeoutMillis(millis));
    }
}

[ValueObject<int>]
public readonly partial struct CacheTtl {
    public static Fin<CacheTtl> Of(Duration horizon) =>
        horizon.TotalSeconds is > 0d and <= int.MaxValue
            ? Fin.Succ(Create((int)Math.Ceiling(horizon.TotalSeconds)))
            : Fin.Fail<CacheTtl>(new CacheFault.InvalidPolicy("recency-horizon", horizon.ToString()));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 1) validationError = new ValidationError($"cache:{"ttl-seconds"}:{value.ToString(CultureInfo.InvariantCulture)}");
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class WideColumnIndex {
    const string Lane = "cache";

    public static Fin<IndexResidency> Admit(StoreProfile profile, IndexResidency residency) =>
        profile.Admits(Lane)
            ? Fin.Succ(residency)
            : Fin.Fail<IndexResidency>(new CacheFault.InvalidPolicy(Lane, profile.Key));

    public static void Register(MappingConfiguration mapping) =>
        mapping.Define(new Map<WideColumnRow>()
            .TableName("artifact_index")
            .PartitionKey(static r => r.Tenant, static r => r.Kind)
            .ClusteringKey(static r => r.At)
            .ClusteringKey(static r => r.Content)
            .Column(static r => r.Bytes)
            .Column(static r => r.Classification)
            .Column(static r => r.SourceKey));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record WideColumnLane(Mapper Mapper, StoreRedrivePort Redrive, CacheToken Cluster, CacheTtl Ttl) {
    IO<Fin<T>> Dialed<T>(ColumnVerb verb, Func<Task<T>> call) =>
        Redrive.Carry(new StoreHop.WideColumn(verb), (string)Cluster,
            IO.liftAsync(async () => (await Op.Of().Catch(async _ => Fin<T>.Succ(await call().ConfigureAwait(false))).ConfigureAwait(false))
                .MapFail(CacheFault.Lift))
            .Bind(IO.liftFin))
        .Map(Fin.Succ)
        | @catch<IO, Fin<T>>(static _ => true, static error => IO.pure(Fin<T>.Fail(error)));

    public IO<Fin<AppliedInfo<WideColumnRow>>> Claim(WideColumnRow row) =>
        Dialed(ColumnVerb.Claim, () => Mapper.InsertIfNotExistsAsync(row, CacheProfile.Claim.Key, insertNulls: false, (int)Ttl));

    public IO<Fin<IPage<WideColumnRow>>> Sweep(
        TenantId tenant, ArtifactKind kind, CachePageSize pageSize, Option<byte[]> cursor) =>
        Dialed(ColumnVerb.Sweep, () => Mapper.FetchPageAsync<WideColumnRow>(
                Cql.New("WHERE tenant = ? AND kind = ?", tenant.Text, kind.Key)
                .WithExecutionProfile(CacheProfile.Sweep.Key)
                .WithOptions(options => {
                    _ = options.SetPageSize((int)pageSize);
                    cursor.IfSome(held => options.SetPagingState(held));
                })));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                         | [BINDING]                                                |
| :-----: | :---------------- | :---------------------------------------------- | :------------------------------------------------------- |
|  [01]   | residency         | `IndexResidency` deployment row                 | a projection residence; not a second SoR/horizon         |
|  [02]   | lane gate         | `Admit` against `StoreProfile.Admits`           | embedded refuses at selection; Marten backs both rows    |
|  [03]   | write-once claim  | `InsertIfNotExistsAsync → AppliedInfo`          | duplicate = `Applied=false`, the 412-noop analog         |
|  [04]   | row expiry        | `CacheTtl.Of(RecencyHorizon)` bound `USING TTL` | one derivation site; the sweep still owns deletion       |
|  [05]   | null posture      | `insertNulls: false` on the claim               | an absent `SourceKey` writes no tombstone                |
|  [06]   | sweep scan        | `FetchPageAsync` + `PagingState` cursor         | partition-paged; never a full-table read                 |
|  [07]   | profile roster    | `CacheProfile` rows + `Declare`                 | declaration and call site share it; no free-text name    |
|  [08]   | consistency claim | the bound row's DECLARED level                  | the POCO rail discards `ExecutionInfo`; no `Any` default |
|  [09]   | fault fold        | `CacheFault.Lift` at ONE boundary               | inside the attempt; no driver exception crosses the rail |
|  [10]   | retriability      | `CacheFault.Retriability`                       | availability faults re-drive                             |
|  [11]   | LWT posture       | pinned fallthrough retry, no speculation        | a re-issued or speculated CAS refuses its own winner     |
|  [12]   | honest degrade    | `IndexResidency.Degrade` per row                | the query shapes the ceiling is bought with              |
|  [13]   | re-drive seam     | kernel `Retriability` → `StoreVerdict.Of`       | `Store/redrive#REDRIVE_SEAM`; root binds the executor    |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
