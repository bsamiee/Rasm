# [PERSISTENCE_QUERY_CACHE]

Rasm.Persistence owns one content-addressed artifact index, one model-result and benchmark recency owner, one buffer-contract L2 contribution, one codec factory, and one optional wide-column projection. `ArtifactKind` carries Persistence-owned `CacheTier`; the AppHost maps that tier onto its runtime cache lane at composition. `ModelResultIndex` owns `RecencyHorizon` and `Now`, so lookup and benchmark claims cannot accept caller freshness knobs. `CacheL2Store` persists absolute and sliding deadlines, rejects expired reads, refreshes sliding rows beneath their absolute cap, and partitions content keys by injected tenant. `IndexResidency` selects `MartenPg | ScyllaWideColumn` without duplicating admission, identity, retention, or horizon policy.

## [01]-[INDEX]

- [01]-[ARTIFACT_BLOB_INDEX]: the `ArtifactKind` taxonomy axis, the content-keyed `ArtifactIndexRow` admission, and the source-keyed projection fold.
- [02]-[MODEL_RESULT_INDEX]: the per-call `ModelResultKey`, the content-addressed `ModelResultIndex` recency/dedup horizon owner with the horizon gate folded into the lookup, and the lookup/publish reuse seam.
- [03]-[BENCHMARK_INDEX]: the `BenchmarkRow` durable claim row and the fingerprint-gated, recency-bounded `Claim` resolution.
- [04]-[L2_CONTRIBUTION]: the `Store`-keyed `IBufferDistributedCache` buffer-contract L2 store, the one `IHybridCacheSerializerFactory` MessagePack codec, the `TenantId`-partitioned content-address key the AppHost cache port resolves over, and the `CacheLane.Store`-gated Redis invalidation backplane beside it.
- [05]-[INDEX_RESIDENCY]: the `IndexResidency` deployment axis (`marten-pg` default · `scylla-widecolumn` scale-out), the LWT claim-gated wide-column admission and `PagingState` sweep, and the `WideColumnFault` one-boundary `DriverException` fold.

## [02]-[ARTIFACT_BLOB_INDEX]

- Owner: `ArtifactKind` is the artifact taxonomy carrying each kind's `RetentionClass` and Persistence-owned `CacheTier`; `ArtifactIndexRow` is the content-keyed residence index, and `Admit` is its sole factory.
- Cases: `ArtifactKind` rows `interchange` (tessellated GLB / chunked field / tile content / re-exported glTF, `blob`-class, `ArtifactBlob` lane) · `ep-context` (compiled ONNX EP-context blob, `cache`-class, `ModelResult` lane) · `onnx-profile` (chrome-trace profiling export, `cache`-class, `ModelResult` lane) · `ifc-semantic` (the Bim IFC semantic graph, `blob`-class, `ArtifactBlob` lane) · `chunk-content` (a content-defined chunk body, `blob`-class, `ArtifactBlob` lane) · `cloud-run` (a completed cloud-run receipt content-keyed by `CloudRunKey`, `cache`-class, `ModelResult` lane — the run's OUTPUT assets land as `interchange` rows through the ONE `Admit` path, the receipt row alone carrying the reuse identity) · `assessment` (the Rasm.Compute discipline-assessment heavy result artifact — an eplusout.sql, an FEA result set — content-keyed by the assessment `(subgraph, route, policy)` input key, `cache`-class, `ModelResult` lane: re-derivable by re-solve, so receipted-evict, never full-history retention) · the THIRTEEN `Rasm.Fabrication` egress families federated at the content-key boundary from the Fabrication-local `EgressKind` discriminant (never a type reference): `cutprogram` · `placement` · `nc1` · `flat-pattern` · `bend-program` · `weld-plan` · `plan` (small machine-consumable receipts, `cache`-class, `ModelResult` lane — re-derivable by re-post/re-nest/re-derive) · `cli` · `threemf` · `scan-vectors` (large layer-stack/mesh payloads, `cache`-class, `ArtifactBlob` lane) · `remnant` · `stock-snapshot` · `traveler` (physical-state and issued-document lineage — an offcut in inventory, the op-N machined state run N+1 admits against, the issued shop traveler — `blob`-class, `ArtifactBlob` lane: the physical world already consumed the artifact, re-derivation is no substitute); a new artifact family is one row carrying its retention class and its lane, never a per-kind row type.
- Entry: `Admit(ArtifactKind, string, ReadOnlySpan<byte>, DataClassification, Instant, Option<UInt128>)` requires an explicit source-key decision, content-addresses the admitted bytes through `ContentAddress.Of`, and derives `RetentionClass` from `ArtifactKind`; `Project` folds rows into source-keyed families.
- Auto: `Admit` is the single content-addressing path — the `ContentAddress` is the seam `ContentAddress.Of(bytes)` over the artifact bytes (the suite hash law, never a path- or filename-keyed identity and never a second hasher), the byte size records from the admitted span's length (never a later filesystem stat), and a self-keyed artifact carries `None` source while a derived artifact (a GLB tessellated from a source IFC) threads the source IFC's content key as `Some` so the two-projection family stays joined; the source key is the KERNEL seed-zero key over the source bytes (the `Bim/Exchange/tessellation#TESSELLATION_BRIDGE` mints it tolerance-independently), NEVER a policy-seeded interchange-cache key, so the GLB and the semantic graph share one origin even across tessellation settings; `Project` groups by `SourceKey.IfNone(Content)` so a self-keyed row projects under its own content and a source-keyed family under its shared origin; a `cloud-run` row keys by `CloudRunKey.Content` — the LENGTH-FRAMED `(recipe digest · input-asset content keys · project slug)` preimage folded through the seam `ContentAddress.Of`, exactly the `Query/lane#ELEMENT_SET_ALGEBRA` `Canonical` framing law, so a re-submitted byte-identical recipe+inputs resolves the SAME row and the prior run's landed assets serve without a cloud round-trip (the SDK's own reuse — `Helper.CheckCached` path-existence, `Wrapper.LocalDatabase` bare SQLite — is verifiably weaker and SUPERSEDED; the run's output-asset bytes travel the `Store/blobstore` presigned-grant row, lineage the `Version/provenance` PROV rows, and no `PollinationSDK` type crosses into this index); the retention class and lane arrive settled from the `ArtifactKind` row so the artifact admits into the `Version/retention#RETENTION_CLASSES` class without a second taxonomy and reads its cache lane without a second routing axis.
- Receipt: an artifact admission rides `store.cache.artifact` carrying the kind, content key, and byte size; the actual blob write rides the `Store/blobstore#OBJECT_STORE` `store.blob.write` and the index row references that residence by content key, never duplicating the byte transfer.
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact family is one `ArtifactKind` row carrying its retention class and lane; a new index column is one field on `ArtifactIndexRow`; zero new surface — a per-kind row type, a second content-key hash, a path-keyed identity, a `string` retention column beside the typed `RetentionClass`, or a managed copy of the blob bytes beside the index is the deleted form because the kind axis is the discriminant and the object store owns residence.
- Boundary: the index row is content-keyed by the same `XxHash128` the kernel mints and the `Store/blobstore#OBJECT_STORE` object name derives from, so the artifact index, the blob residence, and the retention catalog share ONE identity scheme and the index never mints a second; the row references the blob by content key and the `Store/blobstore` lane writes the bytes write-blob-first, so a crash leaves a collectible orphan blob the `Version/retention#SWEEP_AND_GC` reachability mark reaps, never a dangling index row; the `blob`-class kinds (`interchange`/`ifc-semantic`/`chunk-content`) register full-history-reachable so an artifact a historical AS-OF cut references survives, while the `cache`-class kinds (`ep-context`/`onnx-profile`/`cloud-run`/`assessment`) are receipted-evict and re-derivable (a cold companion recompiles the EP-context blob; an evicted cloud-run receipt re-submits at cloud cost; an evicted assessment artifact re-solves through the Compute `AssessmentSink` — cost, never correctness); each kind carries its `CacheLane` so the large-payload `interchange`/`ifc-semantic`/`chunk-content` rows ride the `ArtifactBlob` lane whose `HybridCacheEntryFlags.DisableLocalCache` keeps an oversized blob from pinning the in-process L1 while the small `ep-context`/`onnx-profile`/`cloud-run`/`assessment` rows ride the `ModelResult` lane — the L1/L2 routing is a closed lane value on the kind row, never a per-call branch (`#L2_CONTRIBUTION`); the upstream `Rasm.Compute` lanes compose the `ArtifactKind` constants as settled vocabulary (`onnx-profile` from the inference profiling run, `ep-context` from the session warm-start/fleet compile, `interchange` from the codec content-addressing through `ArtifactIndexRow.Admit`) and a Compute-side artifact owner beside this index is the named drift defect; classification arrives settled so an unstamped artifact rejects at retention admission identically to an over-ceiling one because absence of evidence is not clearance.

```csharp signature
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
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
using Rasm.Persistence.Element;
using Rasm.Persistence.Version;
using StackExchange.Redis;
using Thinktecture;
using Expected = Rasm.Domain.Expected;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------

[Union]
public abstract partial record CacheFault : Expected, IValidationError<CacheFault> {
    private CacheFault() : base() { }
    public sealed record InvalidPolicy(string Policy, string Found) : CacheFault;

    public override int Code => FaultBand.WideColumn;
    public override string Message => Switch(invalidPolicy: static fault => $"<cache-policy:{fault.Policy}:{fault.Found}>");
    public override string Category => "Policy";
    public static CacheFault Create(string message) => new InvalidPolicy("value", message);
}

[ValueObject<string>]
[ValidationError<CacheFault>]
public readonly partial struct CacheToken {
    static partial void ValidateFactoryArguments(ref CacheFault? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' or ':'))) {
            validationError = new CacheFault.InvalidPolicy("token", value);
        }
    }
}

[ValueObject<int>]
[ValidationError<CacheFault>]
public readonly partial struct CachePageSize {
    static partial void ValidateFactoryArguments(ref CacheFault? validationError, ref int value) {
        if (value < 1) validationError = new CacheFault.InvalidPolicy("page-size", value.ToString(CultureInfo.InvariantCulture));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CacheTier {
    public static readonly CacheTier ModelResult = new("model-result");
    public static readonly CacheTier ArtifactBlob = new("artifact-blob");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactKind {
    public static readonly ArtifactKind Interchange = new("interchange", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind EpContext = new("ep-context", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind OnnxProfile = new("onnx-profile", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind IfcSemantic = new("ifc-semantic", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ChunkContent = new("chunk-content", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind CloudRun = new("cloud-run", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Assessment = new("assessment", RetentionClass.Cache, CacheTier.ModelResult);
    // Fabrication egress families: keys mirror the Rasm.Fabrication EgressKind rows verbatim; federation is
    // content-key-only — no Fabrication type crosses this page.
    public static readonly ArtifactKind CutProgram = new("cutprogram", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Placement = new("placement", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind Remnant = new("remnant", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Cli = new("cli", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind ThreeMf = new("threemf", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Nc1 = new("nc1", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind StockSnapshot = new("stock-snapshot", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Traveler = new("traveler", RetentionClass.Blob, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind FlatPattern = new("flat-pattern", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind BendProgram = new("bend-program", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind WeldPlan = new("weld-plan", RetentionClass.Cache, CacheTier.ModelResult);
    public static readonly ArtifactKind ScanVectors = new("scan-vectors", RetentionClass.Cache, CacheTier.ArtifactBlob);
    public static readonly ArtifactKind Plan = new("plan", RetentionClass.Cache, CacheTier.ModelResult);

    public RetentionClass Retention { get; }
    public CacheTier Tier { get; }
    private ArtifactKind(string key, RetentionClass retention, CacheTier tier) : this(key) => (Retention, Tier) = (retention, tier);
}

// --- [MODELS] -----------------------------------------------------------------------------

// `ResultFingerprint` length-frames recipe strings, input count, and fixed-width kernel keys.
// Identical recipes and inputs resolve prior assets without importing a Pollination type.
public readonly record struct CloudRunKey(string RecipeDigest, Seq<UInt128> InputKeys, string ProjectSlug) {
    public UInt128 Content {
        get {
            ArrayBufferWriter<byte> preimage = new();
            Frame(preimage, RecipeDigest);
            BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), InputKeys.Count);
            preimage.Advance(4);
            InputKeys.Iter(key => {
                BinaryPrimitives.WriteUInt128BigEndian(preimage.GetSpan(16), key);
                preimage.Advance(16);
            });
            Frame(preimage, ProjectSlug);
            return ContentAddress.Of(preimage.WrittenSpan).Value;
        }
    }

    static void Frame(ArrayBufferWriter<byte> preimage, string value) {
        int bytes = Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), bytes);
        preimage.Advance(4);
        preimage.Advance(Encoding.UTF8.GetBytes(value, preimage.GetSpan(bytes)));
    }
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

| [INDEX] | [POLICY]          | [VALUE]                                | [BINDING]                                                 |
| :-----: | :---------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content-key name  | kernel `XxHash128` over the bytes      | shared with blob residence + retention; never a second id |
|  [02]   | kind taxonomy     | one `ArtifactKind` row per family      | carries typed `RetentionClass` + `CacheTier`              |
|  [03]   | residence owner   | `Store/blobstore#OBJECT_STORE`         | the index references by content key; never the bytes      |
|  [04]   | source projection | `Project` groups by kernel `SourceKey` | GLB + IFC-semantic of one source stay one family          |
|  [05]   | cloud-run reuse   | `CloudRunKey` length-framed fold       | identical recipe+inputs resolve prior assets              |

## [03]-[MODEL_RESULT_INDEX]

- Owner: `ModelResultKey` the per-call deterministic cache key (model checksum, input digest, the EP/version/precision result key) with its `Content` fold over the seam `ContentAddress` and stable string form; `ModelResultRow` the indexed residence (content address, the blob `ContentAddress`, the host fingerprint string, the stamp); `ModelResultIndex` the content-addressed recency/dedup index — the SINGLE cross-process result-reuse horizon owner — carrying the `RecencyHorizon`, the clock the horizon gate reads, and the `Resolve`/`Record` ports, with the `Lookup`/`Publish` reuse seam folding the horizon gate INTO the resolve; the index is keyed by the suite `XxHash128` content address and never mints a second horizon.
- Cases: a lookup either resolves a residence that is FRESH within the horizon or misses (a stale residence misses by construction, never a separate caller-applied bool); the reuse seam is content-addressed so an inference cache key and a distributed solve sub-block key both fold to one `UInt128` the index resolves identically.
- Entry: `ModelResultIndex.Of` admits a positive `RecencyHorizon` and closes the clock and ports; `Lookup(UInt128)` and `Lookup(ModelResultKey)` apply the private freshness predicate against that clock; `Publish` records the residence; `Claim` reuses the same horizon and clock.
- Auto: the per-call `ModelResultKey.Content` composes the seam `ContentAddress.Of` over the LENGTH-FRAMED canonical key preimage (each UTF8 string — model checksum, result key — prefixed by its LE `int32` byte count, the fixed-16 `InputDigest` big-endian between them self-delimiting, exactly the `Query/lane#ELEMENT_SET_ALGEBRA` `Canonical` framing law so a `(checksum, result-key)` split shift can never collide two distinct calls onto one cached result) so an inference run and its dedup probe address identically AND the suite owns the one `XxHash128` — a second hasher beside the seam, or an unframed concatenation that keys distinct inputs alike, is the deleted form; `ToString` is the stable `HybridCache` lane key the `#L2_CONTRIBUTION` content-address partition scopes; `Lookup` reads through the synchronous lane (a reuse decision is strong-consistency, never a daemon-lagged async read), `Resolve`s the residence, then `Fresh`-gates it against the index clock so a result older than the horizon resolves to `None` and re-computes rather than serving stale — the gate is structural, not a documented obligation; `Publish` records the residence content-addressed so two callers with byte-identical inputs converge on one stored result; the index registers in the `Version/retention#RETENTION_CLASSES` `cache` class so the horizon sweep evicts past the age bound and the one GC governs it.
- Receipt: a reuse hit rides `store.cache.result.hit` carrying the content key, a stale-skip rides `store.cache.result.stale` carrying the content key and age, a publish rides `store.cache.result.publish` carrying the content key and blob residence; the index emits no compute fact (the `Runtime/receipts` `Cache`/`Factorization` facts are the upstream Compute lane's, read by reference).
- Packages: Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`), NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new reuse dimension is one field folded into `ModelResultKey.Content`'s canonical preimage; a new residence column is one field on `ModelResultRow`; zero new surface — a second recency horizon, a per-lane dedup owner, a Compute-side result store, a caller-applied freshness bool the lookup does not enforce, or a daemon-lagged reuse read is the deleted form because this is the single horizon owner, the gate is folded in, and the read is synchronous.
- Boundary: this is the ONE cross-process result-reuse recency horizon — the upstream inference cache (`Model/inference#RESULT_CACHE`), the distributed solve sub-block reuse (`Tensor/factor#SHARD_FANOUT` threads it as the `Blocked.Reuse` column and reads RESIDENCE only — `Lookup` resolves the dedup-keyed `ModelResultRow`, the object-store port yields the `SolveResponse` bytes at that residence), the benchmark recency gate (`#BENCHMARK_INDEX` reads `RecencyHorizon`), and the cost-formula reuse (`Symbolic/lowering#LOWERING_CACHE` keyed by its OWN content identity, never a fabricated `ModelResultKey`) all read it by reference and a second `Duration horizon` minted beside it is the named defect; the index is content-addressed by the suite `XxHash128` so a sub-block keyed by the streamed-request hash folded with the provider dedup key and an inference key folded from `ModelResultKey` resolve through the same `Lookup`/`Publish` seam, never two dedup owners, and `Publish` records the RESIDENCE row (the index never holds the payload — a 2-arg `Publish(address, payload)` is the deleted phantom); the reuse read is the synchronous `Query/lane#READ_ROUTING` lane because serving a stale dedup is a correctness fault, never the async columnar lane; the freshness gate lives INSIDE `Lookup` so a consumer cannot reuse a stale row by forgetting a bool — the only correct miss-or-hit is the index's own; the host fingerprint crosses as a string (the upstream `HostFingerprint.ToString`/`DeterminismTag`) so the index holds no Compute type and the strata dependency stays one-directional; `ModelResultKey` carries ONNX-run identity (model/EP/precision) so a non-ONNX content-keyed reuse (a compiled symbolic formula) keys by its own content identity and never fabricates a `ModelResultKey`.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------

public readonly record struct ModelResultKey(string ModelChecksum, UInt128 InputDigest, string ResultKey) {
    // Dedup preimages frame both UTF-8 strings and retain the fixed-width input digest.
    // Framing prevents field-boundary shifts from aliasing distinct model calls.
    public UInt128 Content {
        get {
            ArrayBufferWriter<byte> preimage = new();
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

public sealed class ModelResultIndex {
    readonly Func<Instant> now;
    readonly Func<UInt128, IO<Option<ModelResultRow>>> resolve;
    readonly Func<ModelResultRow, IO<Unit>> record;

    public Duration RecencyHorizon { get; }

    ModelResultIndex(Duration recencyHorizon, Func<Instant> now, Func<UInt128, IO<Option<ModelResultRow>>> resolve, Func<ModelResultRow, IO<Unit>> record) =>
        (RecencyHorizon, this.now, this.resolve, this.record) = (recencyHorizon, now, resolve, record);

    public static Fin<ModelResultIndex> Of(Duration recencyHorizon, Func<Instant> now, Func<UInt128, IO<Option<ModelResultRow>>> resolve, Func<ModelResultRow, IO<Unit>> record) =>
        recencyHorizon > Duration.Zero
            ? Fin.Succ(new ModelResultIndex(recencyHorizon, now, resolve, record))
            : Fin.Fail<ModelResultIndex>(new CacheFault.InvalidPolicy("recency-horizon", recencyHorizon.ToString()));

    public IO<Option<ModelResultRow>> Lookup(UInt128 content) =>
        resolve(content).Map(found => found.Filter(row => Fresh(row.At)));

    public IO<Option<ModelResultRow>> Lookup(ModelResultKey key) => Lookup(key.Content);
    public IO<Unit> Publish(ModelResultRow row) => record(row);
    public Option<BenchmarkRow> Claim(Seq<BenchmarkRow> rows, string fingerprint) =>
        rows.Filter(row => StringComparer.Ordinal.Equals(row.Fingerprint, fingerprint) && Fresh(row.At))
            .Fold(Option<BenchmarkRow>.None, static (best, row) => best.Case is BenchmarkRow held && held.At >= row.At ? best : row);

    bool Fresh(Instant at) => now() - at <= RecencyHorizon;
}
```

| [INDEX] | [POLICY]          | [VALUE]                                | [BINDING]                                                          |
| :-----: | :---------------- | :------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | reuse horizon     | the ONE `RecencyHorizon` owner         | inference + solve + benchmark + formula read by reference          |
|  [02]   | horizon gate      | folded INTO `Lookup` against the clock | a stale row misses; never a caller-applied bool                    |
|  [03]   | dedup key         | seam `ContentAddress.Of` (`XxHash128`) | one seam for inference keys and solve sub-blocks; no second hasher |
|  [04]   | read consistency  | synchronous `Query/lane` lane          | a stale dedup is a correctness fault; never async                  |
|  [05]   | residence-only    | `Publish` records the row, not bytes   | the object store owns the payload; no 2-arg phantom                |
|  [06]   | fingerprint cross | `HostFingerprint.ToString` string      | no Compute type; strata stays one-directional                      |

## [04]-[BENCHMARK_INDEX]

- Owner: `BenchmarkRow` carries a durable benchmark observation and derives `RetentionClass.Cache`; `ModelResultIndex.Claim` owns fingerprint and recency admission through the closed horizon and clock.
- Cases: `ModelResultIndex.Claim(rows, fingerprint)` returns the newest matching live row or `None`; no call shape can omit or replace the index horizon and clock.
- Entry: `ModelResultIndex.Claim(Seq<BenchmarkRow>, string)` filters and folds once; `BenchmarkRow.Retention` supplies `RetentionClass.Cache`.
- Auto: `Claim` filters the rows to the exact running fingerprint (so a benchmark claimed under managed never wins on a host that resolved native-MKL because the `DeterminismTag` drifts the fingerprint string) and the horizon bound in one pass, then folds to the latest-`At` survivor through one `MostRecent` reduction (never a full `OrderByDescending` materialization, the recency horizon read by reference from `ModelResultIndex` gating the speed claim exactly as it gates result reuse — an optional bound whose absence retained every row was the horizon bypass the mandatory pair deletes); the row registers in the `Version/retention#RETENTION_CLASSES` `cache` class so a re-derivable claim evicts past the age bound and the sweep governs it.
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
    Instant At) {
    public RetentionClass Retention => RetentionClass.Cache;
}
```

| [INDEX] | [POLICY]        | [VALUE]                             | [BINDING]                                      |
| :-----: | :-------------- | :---------------------------------- | :--------------------------------------------- |
|  [01]   | claim gate      | fingerprint-match + latest survivor | a wrong-host or stale claim never wins a route |
|  [02]   | recency bound   | closed index horizon and clock      | `ModelResultIndex.Of`; no bypass shape         |
|  [03]   | head fold       | one `MostRecent` reduction          | no full `OrderByDescending` materialization    |
|  [04]   | retention class | `cache` (re-derivable by re-sweep)  | the sweep governs eviction; never never-evict  |

## [05]-[L2_CONTRIBUTION]

- Owner: `CacheL2Store` contributes the Marten-backed `IBufferDistributedCache`; `CacheCodecFactory` contributes one MessagePack serializer factory; `CachePartition` derives tenant-scoped keys; `CacheBackplane` owns Redis beat and RESP3 tracking invalidation through one `InvalidationMode` drain; AppHost retains the `HybridCache` L1, stampede, and tag owner.
- Cases: the L2 store backs every lane whose `CacheLane.Store` is set (`ModelResult`, `Projection` on `durable-l2`) while a lane with no `Store` (`ArtifactBlob`) resolves the default `HybridCache` with no L2 leg; the codec factory yields a `CacheCodec<T>` for every payload `T` from one MessagePack pass, so a `ModelResultRow`, a `Cached<Fin<T>>` envelope, and a projection document round-trip under one registered factory.
- Entry: `TryGetAsync` rejects expired rows and refreshes a sliding deadline beneath its absolute cap before writing the supplied buffer; `SetAsync` converts nullable platform options into internal `Option` state; `RefreshAsync` advances the same deadline; `CachePartition.Scoped` derives the tenant partition; `Configure` selects RESP3, `EnableTracking` issues `CLIENT TRACKING ON BCAST PREFIX`, and `Drain` folds beat tags or `__redis__:invalidate` keys into the matching `HybridCache` invalidation operation.
- Auto: `IBufferDistributedCache` routes payloads through pooled writers and `ReadOnlySequence<byte>`; `CacheBlob` materializes one durable `byte[]` and stores deadline absence as `Option`. `TryGetAsync` rejects `ExpiresAt <= now()` and renews sliding rows. `SetAsync` derives the earliest absolute, relative, or sliding deadline. `CacheCodec<T>` uses `SnapshotCodec.Binary`. `Scoped` digests the injected tenant through `ContentAddress.Of`.
- Receipt: the L2 contribution emits no cache fact of its own — hit/miss/evict are the AppHost `HybridCacheOptions.ReportTagMetrics` consequences metered by lane tag, and the durable row lifecycle is the `Version/retention` `cache`/`blob` sweep's; the contribution is a storage + codec leg, never a second receipt stream.
- Packages: Microsoft.Extensions.Caching.Hybrid (`IBufferDistributedCache`/`IHybridCacheSerializer<T>`/`IHybridCacheSerializerFactory`/`HybridCache.RemoveAsync`/`RemoveByTagAsync`), Marten (`IDocumentStore`), MessagePack (`MessagePackSerializer`), StackExchange.Redis (`ConfigurationOptions.Protocol`/`RedisProtocol.Resp3`/`IDatabase.ExecuteAsync`/`ISubscriber.SubscribeAsync`/`ChannelMessageQueue`/`RedisChannel`), Rasm.Element (`ContentAddress`), LanguageExt.Core, BCL inbox.
- Growth: a new L2 topology is one composition row; a new payload type uses the existing factory; a new invalidation posture is one `InvalidationMode` case. Redis deployment composes `Configure`, `EnableTracking`, and `Drain(Tracking, token)`; beat deployments use `Drain(Beat, token)`.
- Boundary: Persistence contributes exactly ONE L2 store row (the `IBufferDistributedCache` buffer-contract storage that spares the cache-runtime intermediate-array copy, persisting one `byte[]` at the Marten document seam) plus ONE `IHybridCacheSerializerFactory` (the MessagePack codec for every payload `T`), registered through the AppHost `CacheSurface.Register(services, contributed)` `AddSerializerFactory` on every keyed builder, never a per-type `AddSerializer<T>`; the AppHost `HybridCache` runtime composes ON TOP — `GetOrCreateAsync` drives stampede-protected single-flight population, `RemoveByTagAsync` cuts a lane by its key tag, and the `HybridCacheEntryFlags` lane axis (`DisableLocalCache` on the `ArtifactBlob` lane so an oversized GLB never pins L1, `None` on the `ModelResult` lane) is the per-lane L1/L2 routing — so the L1+stampede+tag-invalidation half is the AppHost port's and the L2-store+serializer half is this contribution, one cache owner across both and never a second; the L2 wire is the `messagepack` `SnapshotCodec.Binary` row so the durable cache bytes and the snapshot/event bytes share one codec and one `Instant` formatter, never a cache-local serializer; the content-address key partitions by `TenantId` through `Scoped` so the `#MODEL_RESULT_INDEX` `ModelResultKey.ToString` lane key and the `#ARTIFACT_BLOB_INDEX` content key both read one tenant-scoped identity exactly as `Element/identity#ELEMENT_IDENTITY` scopes the durable row by `current_setting('rasm.tenant')::uuid`; tag invalidation is an explicit cache capability and never substitutes for durable store integrity — a tag cut is a logical miss-until-expiry, the `RemoveAsync` physical delete its sibling, and the durable reuse rows live on the retention sweep, not the cache TTL; the backplane is LOSSY BY DESIGN — a missed beat is a TTL-bounded stale read, never corruption (the presence-lane precedent), because correctness lives in the durable index rows and the runtime's self-describing expiry envelope, so the beat channel is a latency optimization the deployment composes only where the Redis `Store` row is live; hardening the backplane into a delivery guarantee creates a second reliability owner beside `Version/egress`, the deleted form.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------

public sealed class CacheL2Store(IDocumentStore store, CacheToken storeKey, Func<DateTimeOffset> now) : IBufferDistributedCache {
    public bool TryGet(string key, IBufferWriter<byte> destination) => TryGetAsync(key, destination).AsTask().GetAwaiter().GetResult();

    public async ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default) {
        await using IDocumentSession session = store.LightweightSession();
        CacheBlob? row = await session.LoadAsync<CacheBlob>($"{storeKey}:{key}", token).ConfigureAwait(false);
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
            $"{storeKey}:{key}",
            value.ToArray(),
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
        CacheBlob? row = await session.LoadAsync<CacheBlob>($"{storeKey}:{key}", token).ConfigureAwait(false);
        if (row is null || row.SlidingExpiration.IsNone) { return; }
        session.Store(row with { ExpiresAt = Deadline(now(), row.AbsoluteExpiration, None, row.SlidingExpiration) });
        await session.SaveChangesAsync(token).ConfigureAwait(false);
    }
    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();
    public async Task RemoveAsync(string key, CancellationToken token = default) { await using IDocumentSession session = store.LightweightSession(); session.Delete<CacheBlob>($"{storeKey}:{key}"); await session.SaveChangesAsync(token).ConfigureAwait(false); }

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

public sealed record CacheBlob(string Id, byte[] Payload, Option<DateTimeOffset> AbsoluteExpiration, Option<DateTimeOffset> ExpiresAt, Option<TimeSpan> SlidingExpiration);

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

// --- [OPERATIONS] -------------------------------------------------------------------------

public static class CachePartition {
    // `frame.Tenant` supplies the injected tenant; no ambient tenant context exists.
    public static string Scoped(CacheTier tier, UInt128 tenant, UInt128 content) {
        Span<byte> partition = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(partition, tenant);
        return string.Create(CultureInfo.InvariantCulture, $"{tier.Key}:{ContentAddress.Of(partition).Value:x32}:{content:x32}");
    }
}

// `InvalidationBackplane` carries one lossy channel per store and tenant; TTL bounds missed beats.
// RESP3 tracking converts server invalidations into matching `HybridCache` removals.
public sealed class CacheBackplane(IDatabase database, ISubscriber subscriber, HybridCache cache, CacheToken storeKey, UInt128 tenant) {
    // Composition supplies one injected tenant source for channel and key partitioning.
    public RedisChannel Channel =>
        RedisChannel.Literal(string.Create(CultureInfo.InvariantCulture, $"rasm.cache.{storeKey}:{tenant:x32}"));

    public static ConfigurationOptions Configure(ConfigurationOptions options) {
        options.Protocol = RedisProtocol.Resp3;
        return options;
    }

    public IO<Unit> EnableTracking() =>
        IO.liftAsync(async () => {
            _ = await database.ExecuteAsync("CLIENT", "TRACKING", "ON", "BCAST", "PREFIX", $"{storeKey}:").ConfigureAwait(false);
            return unit;
        });

    public IO<Unit> Publish(CacheToken laneTag) =>
        IO.liftAsync(async () => { _ = await subscriber.PublishAsync(Channel, (string)laneTag).ConfigureAwait(false); return unit; });

    public IO<Unit> Drain(InvalidationMode mode, CancellationToken token) =>
        IO.liftAsync(async () => {
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

    string LogicalKey(string physical) {
        string prefix = $"{storeKey}:";
        return physical.StartsWith(prefix, StringComparison.Ordinal) ? physical[prefix.Length..] : physical;
    }
}
```

| [INDEX] | [POLICY]               | [VALUE]                                         | [BINDING]                                                     |
| :-----: | :--------------------- | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | one L2 store           | `IBufferDistributedCache` buffer contract       | bare `IDistributedCache` forces an extra runtime-copy         |
|  [02]   | one serializer         | `IHybridCacheSerializerFactory`                 | the `messagepack` `SnapshotCodec.Binary` row                  |
|  [03]   | tenant partition       | `Scoped` over `TenantId` digest                 | a cross-tenant L2 bucket collision is unrepresentable         |
|  [04]   | lane L1/L2 routing     | the AppHost `HybridCacheEntryFlags`             | `DisableLocalCache` on the blob lane                          |
|  [05]   | one cache owner        | L2+codec here, L1+stampede+tag AppHost          | composed at `CacheSurface.Register`; never a second owner     |
|  [06]   | invalidation backplane | `Beat` tags / RESP3 tracking keys               | `RemoveByTagAsync` / `RemoveAsync`; null tracking flushes tag |

## [06]-[INDEX_RESIDENCY]

- Owner: `IndexResidency` is the closed `MartenPg | ScyllaWideColumn` deployment family. `WideColumnRow` mirrors the content index. `WideColumnIndex` owns mapping, conditional admission, and cursor paging without forking identity or horizon policy.
- Cases: `IndexResidency` rows are `MartenPg` and `ScyllaWideColumn`; `ClaimMode` is `Idempotent | Required`; `ClaimVerdict` is `Inserted | Duplicate`; `WideColumnFault` closes availability, operation/read/write timeout, LWT refusal, host, invalid-query, and schema-exists causes across `8451`–`8458`.
- Entry: `Claim(Mapper, WideColumnRow, ClaimMode)` maps `AppliedInfo<T>` to `ClaimVerdict` and rails a required duplicate as `LwtRefused`; `Sweep(Mapper, Guid, ArtifactKind, CachePageSize, Option<byte[]>)` pages one partition through `IPage<T>.PagingState`; `Register` declares the single `Map<WideColumnRow>` correspondence.
- Auto: the residence is a projection — the `#ARTIFACT_BLOB_INDEX` `Admit` and `#MODEL_RESULT_INDEX` `Publish` paths stay THE admission owners and the scylla residence receives the SAME rows through `Claim`, so identity, retention, and the recency horizon never fork by residence; consistency/retry/timeout variance is named `IExecutionProfile` rows declared ONCE on the `Builder` and selected per query by name (never per-call branching), routing is `TokenAwarePolicy` over the shard-aware default so a point lookup reaches the owning replica's owning shard, statements are PREPARED only, and the `Cluster`/`ISession` is a composition-root singleton — connection input, never a fence member; `DriverException` lifts ONCE at this boundary through `WideColumnFault.Lift` discriminated on the exception family, never message substrings.
- Receipt: a claim rides `store.cache.residency.claim` carrying the kind, content key, and the `Applied` verdict; a sweep page rides `store.cache.residency.sweep` carrying the partition and row count; the provisioning health posture is the DEPLOYMENT-CONDITIONAL AppHost probe row that lands only where this residency row is composed.
- Packages: ScyllaDBCSharpDriver (`Cluster`/`Builder`/`ISession`/`Cassandra.Mapping` `Mapper`/`Cql`/`CqlQueryOptions`/`MappingConfiguration`/`Map<T>`/`AppliedInfo<T>`/`IPage<T>`/`TokenAwarePolicy`/`DefaultLoadBalancingPolicy`/`IExecutionProfile`/`ConsistencyLevel`/`DriverException` family — assembly `ScyllaDB`, namespace `Cassandra.*`, netstandard2.0 floor: `Task`-based rows, `IPage<T>`+`byte[]` paging, no span/`IAsyncEnumerable` row API to pretend at), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new residence is one `IndexResidency` row carrying its provider surface behind the same `Claim`/`Sweep` verbs; a new claim verdict is one `WideColumnFault` case inside the registry decade; zero new surface — a second admission path beside `Admit`/`Publish`, a scylla-side recency horizon, a scylla event stream, an unprepared inline-CQL statement, a per-call consistency branch beside the named profiles, or a `LOGGED`-vs-`UNLOGGED` batch conflation is the deleted form because the residence is a projection of the one index, the profiles are policy rows, and the claim gate is the one write-once admission.
- Boundary: the wide-column row is a PROJECTION residence — the DECISION seals the SoR spine SINGULAR (one event store, one materializer, one identity, one changefeed), so the scylla residence holds index ROWS keyed by the SAME kernel content identity and can always be rebuilt from the Marten substrate; the LWT claim gate (`InsertIfNotExistsAsync → AppliedInfo<T>.Applied`) is the distributed write-once admission at federation scale — `Serial`/`LocalSerial` is the LWT consistency, distinct from the quorum levels the reads ride; `CqlVector<T>` is recorded embedding-next-to-row ONLY (the corpus ANN owners stay `Query/retrieval`'s pgvector/pgvectorscale rows — never a fifth vector row); the driver's transitive `Newtonsoft.Json` stays driver-internal and the wire codec transits nothing of the STJ rails; `Unavailable`/`WriteTimeout`/`HostDown` are retry-relevant availability faults a recovery predicate may re-drive under the named profile's retry policy, while `LwtRefused` is structurally unretriable — the guard the CAS required was refused by a concurrent winner, the same honesty the coordination lease fence carries.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `IndexResidency` is stable deployment identity; admission, content identity, and horizon remain shared.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IndexResidency {
    public static readonly IndexResidency MartenPg = new("marten-pg");
    public static readonly IndexResidency ScyllaWideColumn = new("scylla-widecolumn");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClaimMode {
    private ClaimMode() { }
    public sealed record Idempotent : ClaimMode;
    public sealed record Required : ClaimMode;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClaimVerdict {
    public static readonly ClaimVerdict Inserted = new("inserted");
    public static readonly ClaimVerdict Duplicate = new("duplicate");
}

// --- [ERRORS] -----------------------------------------------------------------------------
// `WideColumnFold` maps driver exceptions into the closed wide-column band.
// Duplicate claims return `Applied=false`; only refused required guards rail `LwtRefused`.
[Union]
public abstract partial record WideColumnFault : Expected, IValidationError<WideColumnFault> {
    private WideColumnFault() : base() { }
    public sealed record Unavailable(string Detail) : WideColumnFault;
    public sealed record OperationTimedOut(string Detail) : WideColumnFault;
    public sealed record ReadTimedOut(string Detail) : WideColumnFault;
    public sealed record WriteTimedOut(string Detail) : WideColumnFault;
    public sealed record LwtRefused(string Detail) : WideColumnFault;
    public sealed record HostDown(string Detail) : WideColumnFault;
    public sealed record InvalidQuery(string Detail) : WideColumnFault;
    public sealed record SchemaExists(string Detail) : WideColumnFault;

    public override int Code => FaultBand.WideColumn + Switch(
        unavailable:       static _ => 1,
        operationTimedOut: static _ => 2,
        readTimedOut:      static _ => 3,
        writeTimedOut:     static _ => 4,
        lwtRefused:        static _ => 5,
        hostDown:          static _ => 6,
        invalidQuery:      static _ => 7,
        schemaExists:      static _ => 8);

    public override string Message => Switch(
        unavailable:       static c => $"<widecolumn-unavailable:{c.Detail}>",
        operationTimedOut: static c => $"<widecolumn-operation-timeout:{c.Detail}>",
        readTimedOut:      static c => $"<widecolumn-read-timeout:{c.Detail}>",
        writeTimedOut:     static c => $"<widecolumn-write-timeout:{c.Detail}>",
        lwtRefused:        static c => $"<widecolumn-lwt-refused:{c.Detail}>",
        hostDown:          static c => $"<widecolumn-host-down:{c.Detail}>",
        invalidQuery:      static c => $"<widecolumn-invalid-query:{c.Detail}>",
        schemaExists:      static c => $"<widecolumn-schema-exists:{c.Detail}>");

    public override string Category => Switch(
        unavailable:       static _ => "Availability",
        operationTimedOut: static _ => "Timeout",
        readTimedOut:      static _ => "Timeout",
        writeTimedOut:     static _ => "Timeout",
        lwtRefused:        static _ => "Claim",
        hostDown:          static _ => "Host",
        invalidQuery:      static _ => "Query",
        schemaExists:      static _ => "Schema");

    public static WideColumnFault Create(string message) => new Unavailable(message);

    public static WideColumnFault Lift(Exception boundary) => boundary switch {
        UnavailableException u => new Unavailable(u.Message),
        OperationTimedOutException o => new OperationTimedOut(o.Message),
        ReadTimeoutException r => new ReadTimedOut(r.Message),
        WriteTimeoutException w => new WriteTimedOut(w.Message),
        NoHostAvailableException n => new HostDown(n.Message),
        InvalidQueryException i => new InvalidQuery(i.Message),
        AlreadyExistsException a => new SchemaExists(a.Message),
        _ => new Unavailable(boundary.Message),
    };
}

// --- [MODELS] -----------------------------------------------------------------------------
// CQL partitions index mirrors by tenant and kind, then clusters by descending stamp and content.
// Content keys cross as sixteen big-endian bytes; classification and source key remain columns.
public sealed record WideColumnRow(Guid Tenant, string Kind, Instant At, byte[] Content, string Key, long Bytes, string Classification, byte[]? SourceKey);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class WideColumnIndex {
    // ONE fluent mapping declaration (Cassandra.Mapping Map<T>): stated once on the composition root's
    // MappingConfiguration — never attribute scatter beside it, never a second mapping per call site.
    public static void Register(MappingConfiguration mapping) =>
        mapping.Define(new Map<WideColumnRow>()
            .TableName("artifact_index")
            .PartitionKey(static r => r.Tenant, static r => r.Kind)
            .ClusteringKey(static r => r.At)
            .ClusteringKey(static r => r.Content)
            .Column(static r => r.Bytes)
            .Column(static r => r.Classification)
            .Column(static r => r.SourceKey));

    // `InsertIfNotExistsAsync` owns the write-once claim gate; `Applied=false` is idempotent replay.
    // duplicate (the blobstore 412-noop analog) — a verdict, never an error.
    public static IO<Fin<ClaimVerdict>> Claim(Mapper mapper, WideColumnRow row, ClaimMode mode) =>
        IO.liftAsync(async () => {
            AppliedInfo<WideColumnRow> verdict = await mapper.InsertIfNotExistsAsync(row).ConfigureAwait(false);
            return verdict.Applied
                ? Fin.Succ(ClaimVerdict.Inserted)
                : mode is ClaimMode.Idempotent
                    ? Fin.Succ(ClaimVerdict.Duplicate)
                    : Fin.Fail<ClaimVerdict>(new WideColumnFault.LwtRefused(row.Key));
        }) | @catch<IO, Fin<ClaimVerdict>>(static error => error.IsExceptional, static e => IO.pure(Fin<ClaimVerdict>.Fail(WideColumnFault.Lift(e.ToException()))));

    // `FetchPageAsync` and `PagingState` own stateless retention scans by partition.
    // never a full-table read, and the cursor is a byte[] the caller re-presents.
    public static IO<Fin<(Seq<WideColumnRow> Rows, Option<byte[]> Cursor)>> Sweep(Mapper mapper, Guid tenant, ArtifactKind kind, CachePageSize pageSize, Option<byte[]> cursor) =>
        IO.liftAsync(async () => {
            IPage<WideColumnRow> page = await mapper.FetchPageAsync<WideColumnRow>(
                Cql.New("WHERE tenant = ? AND kind = ?", tenant, kind.Key).WithOptions(options => {
                    _ = options.SetPageSize((int)pageSize);
                    cursor.IfSome(held => options.SetPagingState(held));
                })).ConfigureAwait(false);
            return Fin.Succ((toSeq(page), Optional(page.PagingState)));
        }) | @catch<IO, Fin<(Seq<WideColumnRow>, Option<byte[]>)>>(static error => error.IsExceptional, static e => IO.pure(Fin<(Seq<WideColumnRow>, Option<byte[]>)>.Fail(WideColumnFault.Lift(e.ToException()))));
}
```

| [INDEX] | [POLICY]          | [VALUE]                                             | [BINDING]                                               |
| :-----: | :---------------- | :-------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | residency         | `IndexResidency` deployment row                     | a projection residence; not a second SoR/horizon        |
|  [02]   | write-once claim  | `InsertIfNotExistsAsync → AppliedInfo`              | duplicate = `Applied=false`, the 412-noop analog        |
|  [03]   | sweep scan        | `FetchPageAsync` + `PagingState` cursor             | partition-paged; never a full-table read                |
|  [04]   | consistency/retry | named `IExecutionProfile` rows + `TokenAwarePolicy` | policy declared once; never per-call branching          |
|  [05]   | fault fold        | `WideColumnFault.Lift` at ONE boundary              | `FaultBand.WideColumn + n`; no driver exception crosses |
