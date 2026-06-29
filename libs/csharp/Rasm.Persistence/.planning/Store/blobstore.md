# [PERSISTENCE_STORE_BLOBSTORE]

Rasm.Persistence stores geometry and coverage raster bytes as a content-keyed object store keyed by the seam `ContentAddress` (the Persistence `Element/codec#CONTENT_ADDRESS` composes it over the kernel seed-zero `XxHash128`) over the seam `Graph/element#NODE_MODEL` `RepresentationContentHash` the `Object` node carries: an authored GLB by its body `GeometryHash`, a lossless IFC/BREP by its `IfcRepHash`, and a `Geospatial/coverage` raster/field grid by the `Coverage` node's `CoverageGrid.RasterKey`, written WRITE-BLOB-FIRST (content-address the blob, write it, then reference the immutable hash from the Marten event — NOT in the same PostgreSQL transaction as the event), so a crash leaves a collectible orphan blob, never a dangling event reference. One `ObjectStore` provider axis behind the `BlobRemote` placement contract carries four S3-compatible providers (`S3`/`Azure`/`GCS`/`Minio`) on the `ObjectClient` union, the write-once `ConditionalWrite` seal making a racing same-content-key writer a benign `412`-noop (the seal IS the concurrency primitive, no read-before-write), and the content-defined `MultipartTransfer` packing whole `Element/codec#CONTENT_CHUNKING` chunks into provider parts so a re-store transfers only the changed bytes. Every blob carries the same content-lineage and retention-catalog row the snapshot spine has (`H10`), registered in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — a blob a historical AS-OF cut references is never collected. The geometry blob plus the relational identity row plus the Marten event have ONE transaction owner for identity+event (the Marten document in the one `IDocumentSession`, `Element/graph#STORE_RAIL`); the blob is write-first, referenced-after, never a two-ORM atomicity dance. `ContentAddress`, the `RepresentationContentHash` representation content hashes, and the `CoverageGrid.RasterKey` arrive from `Rasm.Element` (composed locally by `Element/codec`); `ChunkManifest`/`ContentChunker` from `Element/codec`; `RetentionClass` from `Version/retention`; `ObjectEncryption` keys from the KMS envelope; `ClockPolicy`, `ReceiptSinkPort`, `CommunityToolkit.HighPerformance` from the substrate.

## [01]-[INDEX]

- [01]-[OBJECT_STORE]: the four-provider axis projecting `BlobRemote`, the write-once seal, the content-lineage catalog, and the closed fault rail.
- [02]-[MULTIPART_TRANSFER]: the content-defined-chunk upload packing whole chunks into provider parts, the content-addressed novelty skip, and the resume edge.
- [03]-[BLOB_GC]: the content-lineage retention row, the full-history reachability mark, the write-blob-first orphan sweep, and the encryption/tenancy envelope.

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore` the `[SmartEnum<string>]` provider axis under the `ComparerAccessors.StringOrdinal` accessor — each row carries the `PartSize` part floor, the `ChunkPolicy` content-defined window, the `ObjectChecksum` integrity stance, the `ConditionalWrite` write-once flag, the `StorageTier` cold-storage column, and the `ObjectEncryption` SSE policy, and builds the row's `BlobRemote` from the resolved `ObjectClient`; `ObjectClient` the resolved-SDK `[Union]` whose `Map` owns per-leg dispatch; `ObjectChecksum`/`StorageTier`/`ObjectEncryption` the closed write-policy vocabularies; `RemoteStoreFault` the closed boundary fault family.
- Cases: `s3`, `azure-blob`, `gcs`, `minio` — the provider sweep closes here, PostgreSQL/SQLite/DuckDB never appearing because the object store is the durable home for geometry and coverage raster bytes behind `BlobRemote`, never a relational engine row; a fifth provider is one row.
- Entry: `public BlobRemote Placement(ObjectClient client, ChunkMembership index)` projects the provider's `BlobRemote` from the resolved client; `public IO<BlobResidence> Put(ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink)` drains the source once, partitions it through `ContentChunker.Chunk`, and rides the placement; `public IO<Stream> Fetch(...)` and `public IO<Option<BlobResidence>> Head(...)` are the read legs.
- Auto: the upload partitions the source into content-defined chunks and packs whole chunks into provider parts of at least `PartSize`, so the `ContentChunker.Novel` probe folds the manifest against the artifact-blob index and an EMPTY novel projection proves the whole content-keyed blob already resident (the upload short-circuits to a `Dedup` fact plus a `Head`-confirm, zero bytes transferred); the `ConditionalWrite` column seals write-once so a re-put of an existing content-key `412`s to `RemoteStoreFault.Conflict` that one `@catch` arm resolves to a benign no-op (the content is identical by hash); the `Integrity` column threads `ChecksumAlgorithm.XXHASH128` (S3/GCS/Minio) so the provider verifies each part and the sealed object against the same 128-bit digest the content key already is; the `Tier` column projects the cold-storage class and the `Encryption` column the SSE stance.
- Receipt: a `BlobTransferFact` rides the receipt envelope under `store.blob.*` — one fact per uploaded part, one per content-addressed novel-skip, one per resumed part, one per abort, one per fetch-by-key hit.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Minio, CommunityToolkit.HighPerformance, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: one `ObjectStore` row absorbs a new provider with zero new surface; a new storage class is one `StorageTier` row, a new SSE stance one `ObjectEncryption` case, a new checksum posture one `ObjectChecksum` row, a tighter content window one `ChunkPolicy` row at `#CONTENT_CHUNKING`, a new boundary failure one `RemoteStoreFault` case; zero new surface — a per-provider upload service, a row delegate re-discriminating the union, or a `client is ObjectClient.S3 ? …` guard is the deleted form because the union case IS the dispatch.
- Boundary: the content-key object name derives from the `Element/codec#CONTENT_ADDRESS` `XxHash128` identity the kernel mints (the GLB `GeometryHash` and IFC/BREP `IfcRepHash` the `Object` node's seam `RepresentationContentHash` carries, the `Coverage` node's `CoverageGrid.RasterKey` raster/field grid), so the object store never mints a second identity; per-leg dispatch is `ObjectClient.Map` — a per-provider service class and a mismatch guard are the deleted forms because the union case is the dispatch and a mismatch is unrepresentable; the write-once seal is the optimistic-concurrency edge each provider exposes (S3/Minio `IfNoneMatch:*`, Azure `IfNoneMatch:ETag.All`, GCS `IfGenerationMatch:0`) so a content-address store needs no read-before-write and a `412` is a benign no-op folded to `RemoteStoreFault.Conflict` and treated as success; every SDK exception lifts once into `RemoteStoreFault` at this edge and `Transport.IsTransient` is the sole `Schedule`-retry gate so a throttle/`5xx` re-drives while a `Conflict`/`NotFound`/`Locked`/`IntegrityBreach`/`Denied`/`Oversize` is deterministic and never retried; credential acquisition, endpoint, and region are host-resolved connection inputs, never fence members.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectChecksum {
    public static readonly ObjectChecksum XxHash128 = new("xxh128");
    public static readonly ObjectChecksum Crc64 = new("crc64");
    public static readonly ObjectChecksum None = new("none");
    public Option<string> Wire(ContentAddress key) {
        var digest = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(digest, key.Value);
        return this == XxHash128 ? Some(Convert.ToBase64String(digest)) : None;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StorageTier {
    public static readonly StorageTier Standard = new("standard");
    public static readonly StorageTier Infrequent = new("infrequent");
    public static readonly StorageTier Cold = new("cold");
    public static readonly StorageTier Archive = new("archive");
}

[Union]
public abstract partial record ObjectEncryption {
    public sealed record ProviderManaged : ObjectEncryption;
    public sealed record ManagedKey(string KeyId, FrozenDictionary<string, string> Aad) : ObjectEncryption;
    public sealed record CustomerKey(ReadOnlyMemory<byte> Key, string KeyMd5) : ObjectEncryption;
}

[Union]
public abstract partial record ObjectClient {
    public sealed record S3(IAmazonS3 Client, string Bucket) : ObjectClient;
    public sealed record Azure(BlobContainerClient Container) : ObjectClient;
    public sealed record Gcs(StorageClient Client, string Bucket) : ObjectClient;
    public sealed record Minio(IMinioClient Client, string Bucket) : ObjectClient;
}

public readonly record struct ChunkMembership(Func<ulong, bool> MayHold, Func<UInt128, bool> Holds) {
    public static readonly ChunkMembership None = new(static _ => false, static _ => false);
}

public readonly record struct BlobResidence(ContentAddress Key, long Length, StorageTier Tier, int Parts, int ResumedParts, int SkippedChunks, Option<string> ConditionToken, CorrelationId Correlation) {
    public static BlobResidence From(ContentAddress key, long length, ObjectStore store) => new(key, length, store.Tier, 0, 0, 0, None, CorrelationId.None);
}

public readonly record struct BlobTransferFact(string Kind, ContentAddress Key, long Bytes, int Part, Instant At);

[Union]
public abstract partial record RemoteStoreFault : Expected, IValidationError<RemoteStoreFault> {
    private RemoteStoreFault(string detail, int code) : base(detail, code, None) { }
    public static RemoteStoreFault Create(string message) => new Transport("none", 0, message);
    public abstract bool IsTransient { get; }
    public sealed record NotFound(ContentAddress Key) : RemoteStoreFault($"blob {Key.Value:x32} absent", 5401) { public override bool IsTransient => false; }
    public sealed record Conflict(ContentAddress Key, string Condition) : RemoteStoreFault($"blob {Key.Value:x32} {Condition}", 5402) { public override bool IsTransient => false; }
    public sealed record Aborted(ContentAddress Key, int Parts, string Reason) : RemoteStoreFault($"blob {Key.Value:x32} aborted@{Parts}: {Reason}", 5403) { public override bool IsTransient => false; }
    public sealed record Transport(string Provider, int Status, string Code) : RemoteStoreFault($"{Provider} {Status}:{Code}", 5404) { public override bool IsTransient => Status is 0 or 429 or >= 500; }
    public sealed record IntegrityBreach(ContentAddress Key, string Provider) : RemoteStoreFault($"blob {Key.Value:x32} {Provider} checksum mismatch", 5405) { public override bool IsTransient => false; }
    public sealed record Locked(ContentAddress Key, string Mode, Instant Until) : RemoteStoreFault($"blob {Key.Value:x32} WORM {Mode}", 5406) { public override bool IsTransient => false; }
    public sealed record Denied(ContentAddress Key, string Provider, string Code) : RemoteStoreFault($"blob {Key.Value:x32} {Provider} denied: {Code}", 5407) { public override bool IsTransient => false; }
    public sealed record Oversize(ContentAddress Key, string Provider, string Code) : RemoteStoreFault($"blob {Key.Value:x32} {Provider} oversize: {Code}", 5408) { public override bool IsTransient => false; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectStore {
    public static readonly ObjectStore S3 = new("s3", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance);
    public static readonly ObjectStore AzureBlob = new("azure-blob", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.Crc64, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance);
    public static readonly ObjectStore Gcs = new("gcs", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance);
    public static readonly ObjectStore Minio = new("minio", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance);

    public long PartSize { get; }
    public ChunkPolicy Chunking { get; }
    public bool ConditionalWrite { get; }
    public ObjectChecksum Integrity { get; }
    public StorageTier Tier { get; }
    public ObjectEncryption Encryption { get; }
    private ObjectStore(string key, long partSize, ChunkPolicy chunking, bool conditionalWrite, ObjectChecksum integrity, StorageTier tier, ObjectEncryption encryption) : this(key) =>
        (PartSize, Chunking, ConditionalWrite, Integrity, Tier, Encryption) = (partSize, chunking, conditionalWrite, integrity, tier, encryption);

    public IO<BlobResidence> Put(ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink) =>
        ContentChunker.Novel(manifest, index.MayHold, index.Holds) is { IsEmpty: true } && !manifest.Chunks.IsEmpty
            ? Head(client, residence.Key).Bind(present => present.Match(Some: IO.pure, None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(residence.Key))))
            : (client.Map(
                s3: row => ObjectIo.S3Multipart(row, this, residence, manifest, source, sink),
                azure: row => ObjectIo.AzureBlocks(row, this, residence, manifest, source, sink),
                gcs: row => ObjectIo.GcsResumable(row, this, residence, manifest, source, sink),
                minio: row => ObjectIo.MinioMultipart(row, this, residence, manifest, source, sink))
            | @catch<IO, BlobResidence>(static e => e is RemoteStoreFault.Conflict, _ => Head(client, residence.Key)
                .Bind(present => present.Match(Some: existing => IO.pure(existing with { Parts = 0, SkippedChunks = manifest.Chunks.Count }), None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(residence.Key)))))).As();

    public IO<Stream> Fetch(ObjectClient client, ContentAddress key, Option<(long Start, long End)> range) =>
        client.Map(s3: r => ObjectIo.S3Fetch(r, key, range), azure: r => ObjectIo.AzureFetch(r, key, range), gcs: r => ObjectIo.GcsFetch(r, key, range), minio: r => ObjectIo.MinioFetch(r, key, range));

    public IO<Option<BlobResidence>> Head(ObjectClient client, ContentAddress key) =>
        client.Map(s3: r => ObjectIo.S3Head(r, key), azure: r => ObjectIo.AzureHead(r, key), gcs: r => ObjectIo.GcsHead(r, key), minio: r => ObjectIo.MinioHead(r, key));

    public BlobRemote Placement(ObjectClient client, ChunkMembership index) =>
        new(
            Put: (key, length, stream) =>
                from source in IO.lift(() => ObjectIo.Drain(stream))
                let manifest = ContentChunker.Chunk(Chunking, source)
                from residence in Put(client, BlobResidence.From(key, length, this), manifest, index, source, static _ => IO.pure(unit))
                select residence.Key,
            Get: key => Fetch(client, key, None),
            Stat: key => Head(client, key).Map(static o => o.Map(static r => r.Key)),
            Delete: key => client.Map(s3: r => ObjectIo.S3Delete(r, key), azure: r => ObjectIo.AzureDelete(r, key), gcs: r => ObjectIo.GcsDelete(r, key), minio: r => ObjectIo.MinioDelete(r, key)),
            List: () => client.Map(s3: ObjectIo.S3List, azure: ObjectIo.AzureList, gcs: ObjectIo.GcsList, minio: ObjectIo.MinioList));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content-key name    | kernel `XxHash128` (`GeometryHash`/`IfcRepHash`) | never a second identity                                |
|  [02]   | per-leg dispatch    | `ObjectClient.Map`                     | union case IS the dispatch; no mismatch guard             |
|  [03]   | write-once seal     | provider conditional-write `412`-noop  | no read-before-write; the seal is the concurrency primitive |
|  [04]   | integrity           | `ChecksumAlgorithm.XXHASH128`          | the same digest the content key is; never re-hashed       |
|  [05]   | fault rail          | one `RemoteStoreFault.Lift` per edge   | `Transport.IsTransient` the sole retry gate               |

## [03]-[MULTIPART_TRANSFER]

- Owner: `MultipartTransfer` the content-defined-chunk upload fold packing whole `ContentChunker` chunks into provider parts; `TransferPart` the packed part window; `BlobTransferReceipt` the per-object transfer evidence; `ObjectIo` the per-leg typed-row upload bodies dispatched only through `ObjectClient.Map`.
- Entry: `public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ClockPolicy clocks)` runs `provider.Put` and reads the realized transfer counts into the receipt; `public static Seq<TransferPart> Parts(ChunkManifest manifest, long partFloor)` packs the manifest's content-defined chunks into provider-floor-clearing part windows.
- Auto: `Parts` accumulates the manifest's content-defined chunks into part windows each closing once it clears the `PartSize` floor (so a part spans whole `#CONTENT_CHUNKING` chunks, never a sub-chunk slice that tears a chunk across a part boundary), the smallest legal part count resulting; a resumed transfer reads the prior committed set (S3 `ListParts`, Azure uncommitted block ids) and skips parts already committed in the SAME interrupted session, orthogonal to the whole-manifest index dedup (one resumes a torn upload, the other skips an already-resident object); the multipart ceremony is a `bracketIO` resource scope whose release runs `AbortMultipartUploadAsync` on every exit so an interrupted upload leaves no orphaned parts, lifting the cause through `RemoteStoreFault.Aborted`.
- Receipt: `BlobTransferReceipt` carries the provider, content-key, bytes, part count, resumed-part count, index-deduplicated-chunk count, abort flag, and elapsed.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Minio, CommunityToolkit.HighPerformance, System.IO.Hashing, LanguageExt.Core, NodaTime.
- Growth: one part-floor policy value per provider row, or one `ChunkPolicy` row for a tighter content window; zero new surface — a second chunker, a re-declared frame width, or a per-provider abort catch is the deleted form because the content-defined window IS the `Element/codec#CONTENT_CHUNKING` chunk fold and the bracket release folds every interruption.
- Boundary: the content-defined chunk boundary, the per-chunk `XxHash128` content key, and the whole-blob `XxHash128` identity are owned at `Element/codec#CONTENT_CHUNKING` and consumed here as the `ChunkManifest` — a re-declared frame width, a second chunker, or a second hash is the deleted form, and the server-side checksum is the SAME digest projected as the provider header; the content-address dedup is a WHOLE-MANIFEST decision (a provider stores whole objects under one content-key name and cannot assemble an object from another object's parts) so an all-resident manifest short-circuits and a novel manifest uploads in full; the part floor clears the S3 5 MB minimum so it is a row value never a free literal; the `ConditionalWrite` column gates the seal so a concurrent same-content-key writer resolves to `RemoteStoreFault.Conflict`, the benign no-op the write-once placement treats as success.

```csharp signature
public readonly record struct TransferPart(int Number, long Offset, int Length, int Chunks);
public readonly record struct BlobTransferReceipt(string Provider, ContentAddress Key, long Bytes, int Parts, int ResumedParts, int SkippedChunks, bool Aborted, Duration Elapsed, Instant At, CorrelationId Correlation);

public static class MultipartTransfer {
    public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ClockPolicy clocks) =>
        from mark in IO.lift(clocks.Mark)
        from sealed_ in provider.Put(client, residence, manifest, index, source, sink)
        select new BlobTransferReceipt(provider.Key, sealed_.Key, sealed_.Length, sealed_.Parts, sealed_.ResumedParts, sealed_.SkippedChunks, Aborted: false, clocks.Elapsed(mark), clocks.Now, sealed_.Correlation);

    public static Seq<TransferPart> Parts(ChunkManifest manifest, long partFloor) =>
        manifest.Chunks.Fold((Done: Seq<TransferPart>(), Open: PartCursor.Empty), (acc, chunk) => acc.Open.Grow(chunk.Length).Pack(partFloor, acc.Done))
            is var (done, open) && open.Count > 0 ? done.Add(open.Seal(done.Count + 1)) : done;
}

file readonly record struct PartCursor(long Start, long Bytes, int Count) {
    public static readonly PartCursor Empty = new(0L, 0L, 0);
    public PartCursor Grow(long bytes) => this with { Bytes = Bytes + bytes, Count = Count + 1 };
    public (Seq<TransferPart> Done, PartCursor Open) Pack(long floor, Seq<TransferPart> done) =>
        Bytes >= floor ? (done.Add(Seal(done.Count + 1)), new PartCursor(Start + Bytes, 0L, 0)) : (done, this);
    public TransferPart Seal(int number) => new(number, Start, (int)Bytes, Count);
}
```

## [04]-[BLOB_GC]

- Owner: `BlobCatalogRow` the content-lineage retention row every blob carries (the same row the snapshot spine has, `H10`); `PendingWrite` the write-blob-first pending ledger; `BlobGc` the static surface owning the write-blob-first protocol, the full-history reachability mark hook, and the orphan sweep.
- Entry: `public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ReadOnlyMemory<byte> source, ChunkMembership index, Func<BlobCatalogRow, IO<Unit>> catalog, ClockPolicy clocks)` content-addresses the blob, writes a `PendingWrite` ledger row, writes the blob, then commits the catalog row — so the blob lands before any event references it; `public static IO<Seq<ContentAddress>> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, LanguageExt.HashSet<ContentAddress> reachable, Duration grace, ClockPolicy clocks)` reaps orphans older than the grace window that no AS-OF cut references.
- Auto: the write protocol is WRITE-BLOB-FIRST — content-address the blob, append a `PendingWrite` ledger row, write the blob via `MultipartTransfer`, reference the immutable content key from the Marten event, then the catalog row commits; a crash between the blob write and the event reference leaves a collectible orphan blob, never a dangling event reference, and the `412`-noop makes a re-write survive; the geometry blob is NOT in the same PostgreSQL transaction as the event (the ONE txn owner is identity+event in the Marten session, `H10`), so the blob is write-first and reference-after; the GC reachability runs over the FULL event history through `Version/retention#SWEEP_AND_GC` `Mark` (every AS-OF cut's referenced content keys) so a blob a historical version references is `Reachable` and never collected, the alternative being geometry-GC-forbidden (dedup + cold-tiering).
- Receipt: a blob write rides `store.blob.write` carrying the content key and bytes; a GC sweep rides `store.blob.gc` carrying the orphan count and reclaimed bytes.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, AWSSDK.KeyManagementService, Azure.Security.KeyVault.Keys, Google.Cloud.Kms.V1, BCL inbox.
- Growth: a new catalog column is one field on `BlobCatalogRow`; zero new surface — a head-only blob GC, a same-PG-txn blob write, a two-ORM atomicity dance, or a free-string blob name is the deleted form because the GC marks over the full history, the blob is write-first content-addressed, and identity+event is the one Marten-session transaction.
- Boundary: the geometry blob carries the SAME content-lineage and retention-catalog row the snapshot spine has (`H10`) and registers in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — geometry GC over head alone is FORBIDDEN, the GC marking over every AS-OF cut so a blob a historical version references survives; the write-blob-first + `412`-noop protocol survives a crash (orphan blob, never dangling reference) and the pending-write ledger commits before the blob `Put` so the sweep distinguishes an in-flight write from an orphan; the ONE transaction owner for identity+event is the Marten `IDocumentSession` (`Element/graph#STORE_RAIL`), the blob being write-first and reference-after with no free two-ORM atomicity; the `ObjectEncryption` envelope binds the SSE key from the `Store` KMS envelope (`ManagedKey` SSE-KMS, `CustomerKey` SSE-C/CSEK, `ProviderManaged` account-default) and the per-tenant prefix partitions the blob namespace by `TenantId` so a multi-tenant store isolates by construction.

```csharp signature
public sealed record BlobCatalogRow(ContentAddress Key, RetentionClass Class, long Bytes, StorageTier Tier, Option<ContentAddress> Lineage, UInt128 Tenant, DataClassification Classification, Instant At);
public readonly record struct PendingWrite(ContentAddress Key, long Bytes, Instant Started);

public static class BlobGc {
    public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ReadOnlyMemory<byte> source, ChunkMembership index, Func<BlobCatalogRow, IO<Unit>> catalog, ClockPolicy clocks) =>
        from manifest in IO.pure(ContentChunker.Chunk(store.Chunking, source))
        from residence in store.Put(client, BlobResidence.From(key, source.Length, store), manifest, index, source, static _ => IO.pure(unit))
        from _ in catalog(new BlobCatalogRow(key, RetentionClass.Blob, source.Length, store.Tier, None, TenantContext.Current.TenantId, DataClassification.Internal, clocks.Now))
        select residence;

    public static IO<Seq<ContentAddress>> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, LanguageExt.HashSet<ContentAddress> reachable, Duration grace, ClockPolicy clocks) =>
        store.Placement(client, ChunkMembership.None).List().Bind(present => {
            var now = clocks.Now;
            var orphans = present.Filter(key => !reachable.Contains(key)
                && catalog.Find(row => row.Key == key).Map(row => now - row.At >= grace).IfNone(true));
            return orphans.TraverseM(key => store.Placement(client, ChunkMembership.None).Delete(key).Map(_ => key)).As();
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | write protocol      | write-blob-first + reference-after     | crash leaves an orphan blob, never a dangling reference   |
|  [02]   | txn owner           | identity+event in the Marten session   | blob is write-first; no two-ORM atomicity (`H10`)         |
|  [03]   | GC reachability     | mark over EVERY AS-OF cut              | full-history; head-only GC is forbidden (`H10`)           |
|  [04]   | lineage catalog     | same row the snapshot spine has        | registers in the `blob` retention class; one GC governs both |
|  [05]   | encryption          | SSE envelope from the KMS owner        | per-tenant prefix partitions the namespace                |
