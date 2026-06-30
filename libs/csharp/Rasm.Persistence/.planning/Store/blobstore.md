# [PERSISTENCE_STORE_BLOBSTORE]

Rasm.Persistence stores geometry and coverage raster bytes as a content-keyed object store keyed by the seam `ContentAddress` (the Persistence `Element/codec#CONTENT_ADDRESS` wraps the seam's raw `UInt128` content keys through `ContentAddress.Of(UInt128)` over the kernel seed-zero `XxHash128`, never a second identity) over the seam `Graph/element#NODE_MODEL` `RepresentationContentHash` keyed map the `Object` node carries: an authored display GLB by its `Body` key, the lossless heavy representation by its `Box` key, the lightweight ANALYTICAL geometry by its `Axis` (the idealized structural line) and `FootPrint` (the space-boundary surface ring) keys — every value an `Option<UInt128>` the store names through `ContentAddress.Of`, the same `file-raw` content-keyed lane, so an above-seam `Rasm.Compute` analysis runner's app-wired `Graph/element#NODE_MODEL` `GeometrySource` port FETCHES and decodes those analytical blobs by content key one-hop (the seam owns the decode CONTRACT, this store the bytes) — and a `Geospatial/coverage` raster/field grid by the `Coverage` node's `CoverageGrid.RasterKey` `UInt128` likewise wrapped through `ContentAddress.Of`, written WRITE-BLOB-FIRST (content-address the blob, write it, then reference the immutable hash from the Marten event — NOT in the same PostgreSQL transaction as the event), so a crash leaves a collectible orphan blob, never a dangling event reference. One `ObjectStore` provider axis behind the `BlobRemote` placement contract carries four S3-compatible providers (`S3`/`Azure`/`GCS`/`Minio`) on the `ObjectClient` union, the write-once `ConditionalWrite` seal making a racing same-content-key writer a benign `412`-noop (the seal IS the concurrency primitive, no read-before-write), the per-row `ObjectChecksum`/`ObjectEncryption`/`ObjectLock` write stances SET on the wire (the content key supplied as the whole-object `XxHash128` checksum, the SSE stance, and the WORM/object-lock retention-until that makes `RemoteStoreFault.Locked` reachable), and the content-defined `MultipartTransfer` packing whole `Element/codec#CONTENT_CHUNKING` chunks into provider parts so a re-store transfers only the changed bytes. Every blob carries the same content-lineage and retention-catalog row the snapshot spine has (`H10`), registered in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — a blob a historical AS-OF cut references is never collected. The geometry blob plus the relational identity row plus the Marten event have ONE transaction owner for identity+event (the Marten document in the one `IDocumentSession`, `Element/graph#STORE_RAIL`); the blob is write-first, referenced-after, never a two-ORM atomicity dance. `ContentAddress` arrives from `Rasm.Element` and is composed locally by `Element/codec` (its `Of(UInt128)` wrap is the ONE entry that turns the seam's raw `RepresentationContentHash` representation hashes and the `CoverageGrid.RasterKey` `UInt128` into the object name, never a second hasher); `ChunkManifest`/`ContentChunker` from `Element/codec`; `RetentionClass`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability`/`RetentionSweep` (the ONE deletion executor the blob GC routes through) from `Version/retention`; the `ObjectEncryption` SSE key material is a server-side-encryption key-id STRING this lane only stamps on the wire (the `ManagedKey.KeyId` the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` or the host KMS minted out-of-band) — the DEK-wrapping envelope lifecycle and both cloud-KMS SDK keyrings (signing AND envelope) are the `Element/identity#AUTHORITY` owner's, never a blob-lane-local KMS envelope; `ClockPolicy`, `ReceiptSinkPort`, `CommunityToolkit.HighPerformance` from the substrate.

## [01]-[INDEX]

- [01]-[OBJECT_STORE]: the four-provider axis projecting `BlobRemote`, the write-once seal, the SSE + WORM/object-lock write stances, the content-lineage catalog, and the closed fault rail.
- [02]-[MULTIPART_TRANSFER]: the content-defined-chunk upload packing whole chunks into provider parts, the content-addressed novelty skip, and the resume edge.
- [03]-[BLOB_GC]: the content-lineage retention row (with its WORM `WormUntil` window) projecting to the `Version/retention` `RetentionFact`, the write-blob-first protocol + the in-flight + WORM fence, and the reclaim routed through the ONE `RetentionSweep` deletion executor with a typed `WormEvict` arrow (never a blob-lane-local sweeper).

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore` the `[SmartEnum<string>]` provider axis under the `ComparerAccessors.StringOrdinal` accessor — each row carries the `PartSize` part floor, the `ChunkPolicy` content-defined window, the `ObjectChecksum` integrity stance, the `ConditionalWrite` write-once flag, the `StorageTier` cold-storage column, the `ObjectEncryption` SSE policy, and the `ObjectLock` WORM/object-lock retention stance, and builds the row's `BlobRemote` from the resolved `ObjectClient`; `ObjectClient` the resolved-SDK `[Union]` whose `Map` owns per-leg dispatch; `ObjectChecksum`/`StorageTier`/`ObjectEncryption`/`ObjectLock` the closed write-policy vocabularies; `RemoteStoreFault` the closed boundary fault family.
- Cases: `s3`, `azure-blob`, `gcs`, `minio` — the provider sweep closes here, PostgreSQL/SQLite/DuckDB never appearing because the object store is the durable home for geometry and coverage raster bytes behind `BlobRemote`, never a relational engine row; a fifth provider is one row.
- Entry: `public BlobRemote Placement(ObjectClient client, ChunkMembership index)` projects the provider's `BlobRemote` from the resolved client; `public IO<BlobResidence> Put(ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Option<StorageTier> at = default)` drains the source once, partitions it through `ContentChunker.Chunk`, and rides the placement at the row `Tier` (or the `at` override the `#BLOB_GC` cold-tier re-PUT supplies); `public IO<Stream> Fetch(...)` and `public IO<Option<BlobResidence>> Head(...)` are the read legs.
- Auto: the upload partitions the source into content-defined chunks and packs whole chunks into provider parts of at least `PartSize`, so the `ContentChunker.Novel` probe folds the manifest against the artifact-blob index and an EMPTY novel projection proves the whole content-keyed blob already resident (the upload short-circuits to a `Dedup` fact plus a `Head`-confirm, zero bytes transferred); the `ConditionalWrite` column seals write-once so a re-put of an existing content-key `412`s to `RemoteStoreFault.Conflict` that one `@catch` arm resolves to a benign no-op (the content is identical by hash); the `Integrity` column threads `ChecksumAlgorithm.XXHASH128` onto the S3 multipart initiate (`ChecksumType.FULL_OBJECT`) so S3 verifies the sealed object against the SAME 128-bit digest the content key already is (the strongest integrity stance — no provider but S3 verifies XxHash128 server-side, so the `XxHash128` row is the S3-only end-to-end check while the Azure row's `Crc64` and the GCS/Minio rows fall back to the providers' SDK-native transfer integrity, CRC32C/CRC64, which the SDK applies automatically); the `Tier` column projects the cold-storage class; and the `Encryption` column is APPLIED on EVERY provider's wire — `ObjectEncryption.ApplyS3`/`ApplyGcs`/`ApplyMinio` stamp the SSE stance on the S3 multipart-initiate, the GCS upload options, and the Minio put respectively, while the Azure SSE (`CustomerProvidedKey`/`EncryptionScope`) is baked into the dialed `ObjectClient.Azure` container by the host — so `ProviderManaged` rides the account/bucket-default SSE (no request member), `ManagedKey` stamps the SSE-KMS key id, `CustomerKey` the SSE-C key, and a provider leg silently dropping the column is the deleted form (the column is honored on request or client at all four), never a decorative column promised only in prose; and the `Lock` column is APPLIED the same way — `ObjectLock.ApplyS3` stamps the `ObjectLockMode`/`ObjectLockRetainUntilDate` on the S3 multipart-initiate and `ApplyMinio` the `ObjectRetentionConfiguration` on the Minio put, while Azure container-immutability and GCS bucket-retention are host-dialed-client facts (no per-request member, the `ObjectEncryption` Azure/GCS split), so `Off` writes no lock member and `Governance`/`Compliance` make the bytes provider-immutable for `Retain` and record the window through `Lock.Until` onto the `#BLOB_GC` `BlobCatalogRow.WormUntil`, which is what makes `RemoteStoreFault.Locked` reachable (the GC `evict` arrow refuses a still-locked key).
- Receipt: a `BlobTransferFact` rides the receipt envelope under `store.blob.*` — one `part` fact per uploaded window, one `resume` per skipped-committed window, one `dedup` per content-addressed novel-skip, one `conflict-noop` per benign `412`, one `abort` per torn ceremony; the envelope stamps the HLC, so the fact carries no `Instant` of its own.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Minio, CommunityToolkit.HighPerformance, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: one `ObjectStore` row absorbs a new provider with zero new surface; a new storage class is one `StorageTier` row, a new SSE stance one `ObjectEncryption` case, a new WORM/object-lock stance one `ObjectLock` case, a new checksum posture one `ObjectChecksum` row, a tighter content window one `ChunkPolicy` row at `#CONTENT_CHUNKING`, a new boundary failure one `RemoteStoreFault` case; zero new surface — a per-provider upload service, a row delegate re-discriminating the union, or a `client is ObjectClient.S3 ? …` guard is the deleted form because the union case IS the dispatch.
- Boundary: the content-key object name derives from the `Element/codec#CONTENT_ADDRESS` `XxHash128` identity the kernel mints — `ContentAddress.Of(UInt128)` wraps the seam's raw `UInt128` content keys (the `Object` node's `RepresentationContentHash` `Body`/`Box`/`Axis`/`FootPrint` representation hashes, the `Coverage` node's `CoverageGrid.RasterKey` raster/field grid), so the object store never mints a second identity and the M2-neutral representation map carries NO IFC-name leak (the `Bim` projector owns the IFC representation mapping behind those neutral keys); per-leg dispatch is `ObjectClient.Map` — a per-provider service class and a mismatch guard are the deleted forms because the union case is the dispatch and a mismatch is unrepresentable; the write-once seal is the optimistic-concurrency edge each provider exposes (S3/Minio `IfNoneMatch:*`, Azure `IfNoneMatch:ETag.All`, GCS `IfGenerationMatch:0`) so a content-address store needs no read-before-write and a `412` is a benign no-op folded to `RemoteStoreFault.Conflict` and treated as success; every SDK exception lifts once into `RemoteStoreFault` at this edge and `Transport.IsTransient` is the sole `Schedule`-retry gate so a throttle/`5xx` re-drives while a `Conflict`/`NotFound`/`Locked`/`IntegrityBreach`/`Denied`/`Oversize` is deterministic and never retried; credential acquisition, endpoint, and region are host-resolved connection inputs, never fence members.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `RemoteStoreFault` derives from the KERNEL federation base, so the bare `Expected` names `Rasm.Domain.Expected`
// (parameterless protected ctor + `Category` virtual) and NEVER the `LanguageExt.Common.Expected` whose
// `(string,int,Option)` ctor is the deleted form.
using Expected = Rasm.Domain.Expected;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectChecksum {
    public static readonly ObjectChecksum XxHash128 = new("xxh128", ChecksumAlgorithm.XXHASH128);
    public static readonly ObjectChecksum Crc64 = new("crc64", ChecksumAlgorithm.CRC64NVME);
    public static readonly ObjectChecksum None = new("none", null);
    public ChecksumAlgorithm? S3Algorithm { get; }
    private ObjectChecksum(string key, ChecksumAlgorithm? s3Algorithm) : this(key) => S3Algorithm = s3Algorithm;
    // The content key IS a 128-bit `XxHash128`, so the `XXHASH128` row hands S3 the SAME digest base64-encoded as the
    // PRECOMPUTED whole-object checksum — never a second hash. `Wire` is the `x-amz-checksum-xxh128` value the S3 `Seal`
    // SUPPLIES on `CompleteMultipartUploadRequest.ChecksumXXHASH128` (paired with the `Initiate`'s `ChecksumType.FULL_OBJECT`
    // stance) so the provider verifies the sealed object against the content key with ZERO server-side re-hash — the content
    // key IS the supplied digest. A non-`XxHash128` row supplies `None` and falls back to the provider's SDK-native transfer
    // integrity (`Crc64` on Azure, CRC32C/CRC64 on GCS/Minio). `Seal` consumes this as its live whole-object checksum, so
    // `Wire` is a load-bearing upload member, never a decorative single-purpose projection.
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
    public static readonly StorageTier Standard = new("standard", S3StorageClass.Standard);
    public static readonly StorageTier Infrequent = new("infrequent", S3StorageClass.StandardInfrequentAccess);
    public static readonly StorageTier Cold = new("cold", S3StorageClass.GlacierInstantRetrieval);
    public static readonly StorageTier Archive = new("archive", S3StorageClass.DeepArchive);
    public S3StorageClass S3Class { get; }
    private StorageTier(string key, S3StorageClass s3Class) : this(key) => S3Class = s3Class;
}

[Union]
public abstract partial record ObjectEncryption {
    public sealed record ProviderManaged : ObjectEncryption;
    public sealed record ManagedKey(string KeyId, FrozenDictionary<string, string> Aad) : ObjectEncryption;
    public sealed record CustomerKey(ReadOnlyMemory<byte> Key, string KeyMd5) : ObjectEncryption;

    // The SSE stance is APPLIED on the wire at every request-keyed provider, never a decorative column: `ProviderManaged`
    // rides the account/bucket default SSE (no request member — the server applies it), `ManagedKey` stamps the SSE-KMS
    // key id, `CustomerKey` the SSE-C customer key + MD5. Three providers carry SSE on the REQUEST and each gets its own
    // `Apply*` arm over the union (the only per-provider variance; one method per request TYPE, not a sibling-factory
    // family — the request types are categorically distinct so a single signature is unrepresentable): S3 on the
    // multipart-initiate, GCS the KMS key id on the upload options (CSEK rides client construction), Minio the
    // `IServerSideEncryption` on the put. Azure SSE is a CLIENT-construction fact — `CustomerProvidedKey`/`EncryptionScope`
    // are baked into the `ObjectClient.Azure` container by the host at dial, NOT a per-request member — so the Azure leg
    // applies nothing and the column is honored at the client the host hands in. So the declared `ObjectStore.Encryption`
    // row is honored on EVERY provider's wire (request or client) — a decorative column promised only in prose is the
    // deleted form, and a missing leg arm silently dropping the column is the gap this dispatch closes.
    public InitiateMultipartUploadRequest ApplyS3(InitiateMultipartUploadRequest request) => Switch(
        providerManaged: static (r, _) => r,
        managedKey:      static (r, k) => (r.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                                           r.ServerSideEncryptionKeyManagementServiceKeyId = k.KeyId, r).Item3,
        customerKey:     static (r, c) => (r.ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.AES256,
                                           r.ServerSideEncryptionCustomerProvidedKey = Convert.ToBase64String(c.Key.Span),
                                           r.ServerSideEncryptionCustomerProvidedKeyMD5 = c.KeyMd5, r).Item4,
        state: request);

    // GCS: SSE-KMS rides `UploadObjectOptions.KmsKeyName`; SSE-C (`CustomerKey`) is a CLIENT-construction fact through
    // `StorageClient.CreateAsync(GoogleCredential?, EncryptionKey?)` the host dials, so the upload-options arm carries
    // only the KMS key id and the customer-key case is a client no-op here (the EncryptionKey rode the dialed client).
    public UploadObjectOptions ApplyGcs(UploadObjectOptions options) => Switch(
        providerManaged: static (o, _) => o,
        managedKey:      static (o, k) => (o.KmsKeyName = k.KeyId, o).Item2,
        customerKey:     static (o, _) => o,
        state: options);

    // Minio (S3-compatible): the SSE stance is the `Minio.DataModel.Encryption.IServerSideEncryption` on the put —
    // `SSEKMS` carries the KMS key id, `SSEC` the 32-byte customer key, `SSES3` the server-managed default — applied
    // through the inherited `EncryptionArgs.WithServerSideEncryption`, so the Minio put honors the same column.
    public PutObjectArgs ApplyMinio(PutObjectArgs args) => Switch(
        providerManaged: static (a, _) => a,
        managedKey:      static (a, k) => a.WithServerSideEncryption(new SSEKMS(k.KeyId)),
        customerKey:     static (a, c) => a.WithServerSideEncryption(new SSEC(c.Key.ToArray())),
        state: args);
}

[Union]
public abstract partial record ObjectLock {
    public sealed record Off : ObjectLock;
    public sealed record Governance(Duration Retain) : ObjectLock;
    public sealed record Compliance(Duration Retain) : ObjectLock;

    // The WORM/object-lock retention stance APPLIED on the write so `RemoteStoreFault.Locked` is genuinely REACHABLE — a
    // compliance-class blob written under an active retention-until cannot be deleted until the window lapses, the SET being
    // the only thing that makes the retention/GC `Locked` surfacing real (the fault `H10` declares is otherwise unmintable).
    // `Off` writes no lock member; `Governance`/`Compliance` stamp the object-lock mode + a retention-until of `clocks.Now +
    // Retain` so the bytes are immutable for the window. S3 carries it on the multipart-INITIATE (`ObjectLockMode`/
    // `ObjectLockRetainUntilDate`), Minio on the put through the inherited `ObjectWriteArgs.WithRetentionConfiguration`
    // (`x-amz-object-lock-mode`/`x-amz-object-lock-retain-until-date`); Azure immutability is a CONTAINER policy the host
    // dials (the `BlobContainerClient` carries the immutability scope, no per-request member) and GCS object retention is a
    // BUCKET policy (no `UploadObjectOptions` member), so those two legs apply nothing and the column is honored at the
    // host-dialed client — exactly the `ObjectEncryption` Azure/GCS split. `Until` is the catalog WORM column the write path
    // commits and the retention sweep reads. Two providers carry it on the REQUEST and each gets one `Apply*` arm over the
    // union (the only per-provider variance; the request types are categorically distinct so a single signature is
    // unrepresentable), mirroring `ObjectEncryption` — a decorative lock column promised only in prose is the deleted form.
    public Option<Instant> Until(Instant now) => Map(
        off:        static _ => Option<Instant>.None,
        governance: c => Some(now + c.Retain),
        compliance: c => Some(now + c.Retain));

    public InitiateMultipartUploadRequest ApplyS3(InitiateMultipartUploadRequest request) => Switch(
        off:        static (r, _) => r,
        governance: static (r, c) => (r.ObjectLockMode = ObjectLockMode.Governance,
                                      r.ObjectLockRetainUntilDate = DateTime.UtcNow + c.Retain.ToTimeSpan(), r).Item3,
        compliance: static (r, c) => (r.ObjectLockMode = ObjectLockMode.Compliance,
                                      r.ObjectLockRetainUntilDate = DateTime.UtcNow + c.Retain.ToTimeSpan(), r).Item3,
        state: request);

    public PutObjectArgs ApplyMinio(PutObjectArgs args) => Switch(
        off:        static (a, _) => a,
        governance: static (a, c) => a.WithRetentionConfiguration(new ObjectRetentionConfiguration(DateTime.UtcNow + c.Retain.ToTimeSpan(), ObjectRetentionMode.GOVERNANCE)),
        compliance: static (a, c) => a.WithRetentionConfiguration(new ObjectRetentionConfiguration(DateTime.UtcNow + c.Retain.ToTimeSpan(), ObjectRetentionMode.COMPLIANCE)),
        state: args);
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

public readonly record struct BlobTransferFact(string Kind, ContentAddress Key, long Bytes, int Part = 0);

// The remote-store boundary fault band (540x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the
// Persistence siblings `Version/retention#RETENTION_CLASSES` `RetentionFault` (828x), `Version/recovery#RECOVERY_ROUTES`
// `RecoveryFault` (829x), and `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) realize — NOT `LanguageExt.Common.Expected`,
// whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to override) is the deleted form that
// resolves to the wrong base and falls outside the kernel federation a telemetry reader bands by code. Band membership
// is a per-case `Code => 540x` override, `Message`/`Category` projecting through the generated `Switch`, so a typed case
// lifts BARE onto `Fin<T>`/`IO<T>` with no `.ToError()` hop and a recovery reads `error.IsType<RemoteStoreFault.Conflict>()`
// / `error.HasCode(5402)` / `error.Category()`, never a message substring. `[SkipUnionOps]` is the canonical fault-band
// annotation. `IsTransient` stays an `abstract` discriminant with one override per case (orthogonal to the base-ctor
// change) so `Transport.IsTransient` remains the sole `Schedule`-retry gate. `Create` is the IValidationError admission
// the generated converter bridge calls on a deserialization reject.
[SkipUnionOps]
[Union]
public abstract partial record RemoteStoreFault : Expected, IValidationError<RemoteStoreFault> {
    private RemoteStoreFault() : base() { }
    public abstract bool IsTransient { get; }
    public sealed record Text(string Detail) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record NotFound(ContentAddress Key) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Conflict(ContentAddress Key, string Condition) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Aborted(ContentAddress Key, int Parts, string Reason) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Transport(string Provider, int Status, string Code) : RemoteStoreFault { public override bool IsTransient => Status is 0 or 429 or >= 500; }
    public sealed record IntegrityBreach(ContentAddress Key, string Provider) : RemoteStoreFault { public override bool IsTransient => false; }
    // The WORM/object-lock retention fault — minted by the retention/GC `Execute` evict arrow when a blob under an active
    // object-lock retention-until cannot be deleted (the catalog row carries the lock window), NOT by the SDK `Lift`
    // (a provider 403 retention-block is not reliably distinguishable from an auth denial by status, so the SDK arm folds
    // to `Denied` with the code preserved). The object-store evict surfaces `Locked` from the domain-side retention check.
    public sealed record Locked(ContentAddress Key, string Mode, Instant Until) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Denied(ContentAddress Key, string Provider, string Code) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Oversize(ContentAddress Key, string Provider, string Code) : RemoteStoreFault { public override bool IsTransient => false; }

    public override int Code => Switch(
        text:           static _ => 5400,
        notFound:       static _ => 5401,
        conflict:       static _ => 5402,
        aborted:        static _ => 5403,
        transport:      static _ => 5404,
        integrityBreach: static _ => 5405,
        locked:         static _ => 5406,
        denied:         static _ => 5407,
        oversize:       static _ => 5408);

    public override string Message => Switch(
        text:           static c => c.Detail,
        notFound:       static c => $"blob {c.Key.Value:x32} absent",
        conflict:       static c => $"blob {c.Key.Value:x32} {c.Condition}",
        aborted:        static c => $"blob {c.Key.Value:x32} aborted@{c.Parts}: {c.Reason}",
        transport:      static c => $"{c.Provider} {c.Status}:{c.Code}",
        integrityBreach: static c => $"blob {c.Key.Value:x32} {c.Provider} checksum mismatch",
        locked:         static c => $"blob {c.Key.Value:x32} WORM {c.Mode}",
        denied:         static c => $"blob {c.Key.Value:x32} {c.Provider} denied: {c.Code}",
        oversize:       static c => $"blob {c.Key.Value:x32} {c.Provider} oversize: {c.Code}");

    public override string Category => Switch(
        text:           static _ => "Text",
        notFound:       static _ => "NotFound",
        conflict:       static _ => "Conflict",
        aborted:        static _ => "Aborted",
        transport:      static _ => "Transport",
        integrityBreach: static _ => "Integrity",
        locked:         static _ => "Locked",
        denied:         static _ => "Denied",
        oversize:       static _ => "Oversize");

    public static RemoteStoreFault Create(string message) => new Text(message);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectStore {
    public static readonly ObjectStore S3 = new("s3", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore AzureBlob = new("azure-blob", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.Crc64, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore Gcs = new("gcs", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore Minio = new("minio", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);

    public long PartSize { get; }
    public ChunkPolicy Chunking { get; }
    public bool ConditionalWrite { get; }
    public ObjectChecksum Integrity { get; }
    public StorageTier Tier { get; }
    public ObjectEncryption Encryption { get; }
    public ObjectLock Lock { get; }
    private ObjectStore(string key, long partSize, ChunkPolicy chunking, bool conditionalWrite, ObjectChecksum integrity, StorageTier tier, ObjectEncryption encryption, ObjectLock @lock) : this(key) =>
        (PartSize, Chunking, ConditionalWrite, Integrity, Tier, Encryption, Lock) = (partSize, chunking, conditionalWrite, integrity, tier, encryption, @lock);

    // `at` is the egress tier override the cold-tier re-PUT supplies (`#BLOB_GC` `Demote`): `None` rides the provider
    // row's own `Tier`, `Some(colder)` stamps the colder `S3StorageClass` on the leg `Initiate` so the SAME content-addressed
    // write engine effects a storage-class demotion with no parallel transition path, the write-once seal making the
    // same-key re-PUT a benign `412`-noop where the bytes already reside.
    public IO<BlobResidence> Put(ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Option<StorageTier> at = default) =>
        ContentChunker.Novel(manifest, index.MayHold, index.Holds) is { IsEmpty: true } && !manifest.Chunks.IsEmpty
            ? Head(client, residence.Key).Bind(present => present.Match(
                Some: existing => sink(new BlobTransferFact("dedup", residence.Key, 0L)).Map(_ => existing with { Parts = 0, SkippedChunks = manifest.Chunks.Count }),
                None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(residence.Key))))
            : (ObjectIo.For(client).Multipart(this, at.IfNone(Tier), residence, manifest, source, sink)
                | @catch<IO, BlobResidence>(static e => e is RemoteStoreFault.Conflict, _ => Head(client, residence.Key)
                    .Bind(present => present.Match(
                        Some: existing => sink(new BlobTransferFact("conflict-noop", residence.Key, 0L)).Map(_ => existing with { Parts = 0, SkippedChunks = manifest.Chunks.Count }),
                        None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(residence.Key)))))).As();

    public IO<Stream> Fetch(ObjectClient client, ContentAddress key, Option<(long Start, long End)> range) => ObjectIo.For(client).Fetch(key, range);
    public IO<Option<BlobResidence>> Head(ObjectClient client, ContentAddress key) => ObjectIo.For(client).Head(this, key);
    public IO<Unit> Delete(ObjectClient client, ContentAddress key) => ObjectIo.For(client).Erase(key);
    public IO<Seq<ContentAddress>> List(ObjectClient client) => ObjectIo.For(client).Enumerate();

    // The Persistence-local placement-delegate bundle the app composes the seam `Graph/element#NODE_MODEL`
    // `GeometrySource` resolver over: `Get(key, range)` is the range-capable one-hop fetch a `Rasm.Compute` runner
    // pulls an analytical `Axis`/`FootPrint` slice through (a mesh LOD or a BREP byte-window, never the whole blob),
    // `Put` content-addresses and write-once-seals, `Stat`/`Delete`/`List` close the lifecycle. ONE record over the
    // five legs — a `GeometrySource` over a phantom node field, or a parallel `GetRange` sibling, is the deleted form.
    public BlobRemote Placement(ObjectClient client, ChunkMembership index) =>
        new(
            Put: (key, length, stream) =>
                from source in ObjectIo.Drain(stream)
                from residence in Put(client, BlobResidence.From(key, length, this), ContentChunker.Chunk(Chunking, source), index, source, static _ => IO.pure(unit))
                select residence.Key,
            Get: (key, range) => Fetch(client, key, range),
            Stat: key => Head(client, key),
            Delete: key => Delete(client, key),
            List: () => List(client));
}

// The placement-delegate bundle the app wires the seam `Graph/element#NODE_MODEL` `GeometrySource` decoder over
// (`Get(repHash.Axis.Value, None)` -> decode -> `AxisCurve`); the range modality rides `Get`, never a `GetRange`
// sibling, so a partial analytical fetch and a whole-blob fetch are one entry discriminating on the `Option` range.
public readonly record struct BlobRemote(
    Func<ContentAddress, long, Stream, IO<ContentAddress>> Put,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Get,
    Func<ContentAddress, IO<Option<BlobResidence>>> Stat,
    Func<ContentAddress, IO<Unit>> Delete,
    Func<IO<Seq<ContentAddress>>> List);
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content-key name    | `ContentAddress.Of(UInt128)` over kernel `XxHash128` | wraps the seam raw `UInt128` keys; never a second identity |
|  [02]   | per-leg dispatch    | `ObjectClient.Map`                     | union case IS the dispatch; no mismatch guard             |
|  [03]   | write-once seal     | provider conditional-write `412`-noop  | no read-before-write; the seal is the concurrency primitive |
|  [04]   | integrity           | `ChecksumAlgorithm.XXHASH128` + `Wire` | the content key supplied as the whole-object checksum; never re-hashed |
|  [05]   | WORM/object-lock    | `ObjectLock` SET on the write          | `Compliance`/`Governance` immutable for `Retain`; `Locked` reachable via the catalog `WormUntil` |
|  [06]   | fault rail          | one `RemoteStoreFault.Lift` per edge   | `Transport.IsTransient` the sole retry gate               |

## [03]-[MULTIPART_TRANSFER]

- Owner: `ObjectIo` the ONE generic transfer engine — a per-provider `ObjectLeg` delegate row (initiate, stage-one-part, seal, abort, list-committed PLUS fetch/head/erase/enumerate) the four providers each fill ONCE, over which a SINGLE `bracketIO`-scoped fold packs the manifest's content-defined chunks into provider parts and seals; `ObjectLeg` the closed nine-delegate per-provider leg carrier `ObjectClient.Map` resolves; `MultipartTransfer` the receipt-emitting `Upload` plus the `Parts` packer; `TransferPart`/`CommittedPart` the packed/committed part windows; `PartCursor` the part-packing fold state; `BlobName` the Persistence-local content-key-to-object-name projection; `BlobTransferReceipt` the per-object transfer evidence.
- Entry: `public static ObjectLeg For(ObjectClient client)` resolves the per-provider nine-delegate leg through `ObjectClient.Map`; `public IO<BlobResidence> Multipart(ObjectStore provider, StorageTier tier, BlobResidence residence, ChunkManifest manifest, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink)` runs the one bracket-scoped packing fold over the resolved leg at the effective `tier` (the row `Tier`, or the colder rung a cold-tier re-PUT threads); `public static IO<ReadOnlyMemory<byte>> Drain(Stream source)` stages a fetch stream into a pooled `ArrayPoolBufferWriter<byte>`; `public static Seq<TransferPart> Parts(ChunkManifest manifest, long partFloor)` packs the chunks into provider-floor-clearing windows the fold uploads.
- Auto: `Parts` accumulates the manifest's content-defined chunks into part windows each closing once it clears the `PartSize` floor (so a part spans whole `#CONTENT_CHUNKING` chunks, never a sub-chunk slice that tears a chunk across a part boundary), the smallest legal part count resulting; `Multipart` initiates the upload, reads the prior committed set through `leg.ListCommitted` (S3 `ListParts`, Azure uncommitted block ids — a fresh upload yields the empty set) so an interrupted transfer SKIPS the windows already committed in the SAME session (orthogonal to the whole-manifest index dedup: one resumes a torn upload, the other skips an already-resident object), then `TraverseM`-folds the residual windows through `leg.UploadPart`, counting resumed-versus-fresh into the residence; the whole ceremony is a `bracketIO` resource scope whose `Catch`/`Fin` release runs `leg.Abort` on EVERY non-completing exit (cancellation, fault, conflict) so an interrupted upload leaves no orphaned parts, lifting the cause through `RemoteStoreFault.Aborted`; `Drain` rents a pooled `ArrayPoolBufferWriter<byte>` and copies the source stream into it so the fetch-to-bytes hop never allocates a throwaway array (`CommunityToolkit.HighPerformance` substrate).
- Receipt: `BlobTransferFact` rides `store.blob.part` per uploaded part, `store.blob.resume` per skipped-committed window, `store.blob.abort` per aborted ceremony; `BlobResidence` carries the realized part / resumed-part / skipped-chunk counts the `Upload` receipt reads.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Minio (`Minio.Exceptions` the fourth provider's lifted fault family), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.IO.Hashing, LanguageExt.Core, NodaTime.
- Growth: one part-floor policy value per provider row, or one `ChunkPolicy` row for a tighter content window; a fifth provider fills ONE `ObjectLeg` row in `For` and contributes ITS exception family to the `Lift` fold; zero new surface — a second chunker, a re-declared frame width, a per-provider multipart body, a per-provider read/head/delete/list body, or a per-provider abort catch is the deleted form because the content-defined window IS the `Element/codec#CONTENT_CHUNKING` chunk fold, the nine-delegate leg row IS the per-provider variance, and the one bracket release folds every interruption.
- Boundary: the content-defined chunk boundary, the per-chunk `XxHash128` content key, and the whole-blob `XxHash128` identity are owned at `Element/codec#CONTENT_CHUNKING` and consumed here as the `ChunkManifest` — a re-declared frame width, a second chunker, or a second hash is the deleted form, and the server-side checksum is the SAME digest projected as the provider header; the content-address dedup is a WHOLE-MANIFEST decision (a provider stores whole objects under one content-key name and cannot assemble an object from another object's parts) so an all-resident manifest short-circuits and a novel manifest uploads in full; the part floor clears the S3 5 MB minimum so it is a row value never a free literal; the abort is the bracket's release arm under a non-cancellable token (a long token-aware drain never rides a disposer), so a torn upload self-cleans without a separate sweep; the `ConditionalWrite` column gates the seal at `complete` so a concurrent same-content-key writer resolves to `RemoteStoreFault.Conflict`, the benign no-op the write-once placement treats as success.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TransferPart(int Number, long Offset, int Length, int Chunks);
public readonly record struct CommittedPart(int Number, string ETag);
public readonly record struct BlobTransferReceipt(string Provider, ContentAddress Key, long Bytes, int Parts, int ResumedParts, int SkippedChunks, bool Aborted, Duration Elapsed, Instant At, CorrelationId Correlation);

// The per-provider leg carrier: each provider fills these NINE delegates ONCE (the only per-provider variance), so the
// transfer ceremony AND the read/head/delete/list bodies are written ONCE over the leg row, never four parallel
// `S3Multipart`/`AzureBlocks`/... bodies. `Initiate` yields the provider upload token (S3 `UploadId`, Azure a block-id
// stem, whole-object providers the object name) and seals the whole-object checksum + the effective `StorageTier` the
// caller threads (the row `Tier` on a fresh write, the colder rung on a `#BLOB_GC` `Demote` re-PUT); `Stage` uploads one packed window (a no-op for whole-object providers
// whose SDK auto-chunks); `Seal` FINALIZES the object carrying the `ObjectStore` row (so a single-PUT provider stamps the
// row's SSE + write-once stance at the one finalize point) — for staged providers it commits the part list under the
// write-once `IfNoneMatch:*` precondition (S3) / `ETag.All` (Azure), for whole-object providers it runs the single PUT of
// the WHOLE `source`: GCS under the genuine `IfGenerationMatch=0` create-if-absent precondition, Minio as an idempotent
// content-addressed overwrite (a plain `PutObjectArgs` cannot carry a conditional ETag — but the object NAME is the
// content hash, so a racing second writer necessarily writes byte-identical bytes and the overwrite is benign by
// construction, the write-once INVARIANT preserved without the conditional the SDK does not expose on a put) — so the
// one engine drives both transfer models through ONE seal without a mode knob; `Abort` is the bracket release;
// `Committed` lists the prior session's windows for resume;
// `Fetch`/`Head`/`Erase`/`Enumerate` are the read/lifecycle legs the placement dispatches.
public readonly record struct ObjectLeg(
    Func<ObjectStore, StorageTier, ContentAddress, IO<string>> Initiate,
    Func<string, TransferPart, ReadOnlyMemory<byte>, IO<CommittedPart>> Stage,
    Func<ObjectStore, string, ContentAddress, Seq<CommittedPart>, ReadOnlyMemory<byte>, IO<Unit>> Seal,
    Func<string, ContentAddress, IO<Unit>> Abort,
    Func<string, ContentAddress, IO<Seq<CommittedPart>>> Committed,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Fetch,
    Func<ObjectStore, ContentAddress, IO<Option<BlobResidence>>> Head,
    Func<ContentAddress, IO<Unit>> Erase,
    Func<IO<Seq<ContentAddress>>> Enumerate);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The Persistence-local object-name projection of the seam content key — the `x32` lowercase hex of the `UInt128` under
// the per-tenant prefix the `#BLOB_GC` envelope partitions by, so two tenants never collide on one content key and the
// object store mints NO second identity; `OfName` is the inverse a `List` row reads back. NOT a seam member (the seam owns
// the `ContentAddress` value, this page owns its provider-name spelling), so a free-string blob name is the deleted form.
file static class BlobName {
    public static string Name(this ContentAddress key) => $"{TenantContext.Current.TenantId:x32}/{key.Value:x32}";
    public static ContentAddress OfName(string name) => ContentAddress.Of(UInt128.Parse(name.AsSpan(name.LastIndexOf('/') + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
}

public static class MultipartTransfer {
    public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobResidence residence, ChunkManifest manifest, ChunkMembership index, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ClockPolicy clocks) =>
        from mark in IO.lift(clocks.Mark)
        from sealed_ in provider.Put(client, residence, manifest, index, source, sink)
        select new BlobTransferReceipt(provider.Key, sealed_.Key, sealed_.Length, sealed_.Parts, sealed_.ResumedParts, sealed_.SkippedChunks, Aborted: false, clocks.Elapsed(mark), clocks.Now, sealed_.Correlation);

    // Pack whole content-defined chunks into part windows, each closing once it clears the floor — a part spans whole
    // chunks (never a sub-chunk slice), the smallest legal part count resulting, and the open tail seals last.
    public static Seq<TransferPart> Parts(ChunkManifest manifest, long partFloor) =>
        manifest.Chunks.Fold((Done: Seq<TransferPart>(), Open: PartCursor.Empty), static (acc, chunk) => acc.Open.Grow(chunk).Pack(partFloor, acc.Done))
            is var (done, open) && open.Chunks > 0 ? done.Add(open.Seal(done.Count + 1)) : done;
}

file readonly record struct PartCursor(long Start, long Bytes, int Chunks) {
    public static readonly PartCursor Empty = new(0L, 0L, 0);
    public PartCursor Grow(ContentChunk chunk) => this with { Bytes = Bytes + chunk.Length, Chunks = Chunks + 1 };
    public (Seq<TransferPart> Done, PartCursor Open) Pack(long floor, Seq<TransferPart> done) =>
        Bytes >= floor ? (done.Add(Seal(done.Count + 1)), new PartCursor(Start + Bytes, 0L, 0)) : (done, this);
    public TransferPart Seal(int number) => new(number, Start, (int)Bytes, Chunks);
}

// The ONE generic transfer engine. `For` resolves the per-provider `ObjectLeg`; `Multipart` runs the single
// bracket-scoped packing fold over the multipart legs; `Drain` stages a fetch stream into pooled storage. The four
// `*Leg` constructors fill the `ObjectLeg` delegates over the typed SDK client — the ONLY per-provider code; every
// SDK exception inside a leg lifts once to `RemoteStoreFault` at the `Bound` boundary, so the engine sees only rails.
public static class ObjectIo {
    public static ObjectLeg For(ObjectClient client) => client.Map(
        s3: static r => S3Leg(r), azure: static r => AzureLeg(r), gcs: static r => GcsLeg(r), minio: static r => MinioLeg(r));

    // The one bracket-scoped packing fold. `Initiate` acquires the upload token; `Use` lists the prior session's committed
    // windows (empty on a fresh upload), uploads only the unresumed windows through `leg.Stage`, and `Seal`s under the
    // write-once precondition; `Fin` runs `leg.Abort` on EVERY exit — on a torn/cancelled upload it reaps the staged parts,
    // on the success path the already-completed upload's abort is a benign provider no-op the `@catch` swallows. So a
    // fault/cancel leaves no orphaned parts and a normal seal pays one idempotent abort, never a leaked multipart.
    public static IO<BlobResidence> Multipart(this ObjectLeg leg, ObjectStore provider, StorageTier tier, BlobResidence residence, ChunkManifest manifest, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink) {
        var windows = MultipartTransfer.Parts(manifest, provider.PartSize);
        return leg.Initiate(provider, tier, residence.Key).Bracket(
            Use: token =>
                from prior in leg.Committed(token, residence.Key)
                let resumed = prior.Map(static p => p.Number).ToFrozenSet()
                from staged in windows.Filter(w => !resumed.Contains(w.Number)).TraverseM(w =>
                    from committed in leg.Stage(token, w, source.Slice((int)w.Offset, w.Length))
                    from _ in sink(new BlobTransferFact("part", residence.Key, w.Length, w.Number))
                    select committed).As()
                from _ in prior.TraverseM(p => sink(new BlobTransferFact("resume", residence.Key, 0L, p.Number))).As()
                from _ in leg.Seal(provider, token, residence.Key, toSeq((prior + staged).OrderBy(static p => p.Number)), source)
                select residence with { Length = source.Length, Parts = windows.Count, ResumedParts = prior.Count, SkippedChunks = 0 },
            Catch: error => IO.fail<BlobResidence>(error is RemoteStoreFault ? error : new RemoteStoreFault.Aborted(residence.Key, windows.Count, error.Message)),
            Fin: token => (leg.Abort(token, residence.Key) | @catch<IO, Unit>(static _ => true, static _ => IO.pure(unit))).As());
    }

    // Drain a fetch stream into pooled storage — `ArrayPoolBufferWriter<byte>` rents from the shared pool, the copy fills
    // it, and `WrittenMemory` reads back with zero throwaway array; the writer disposes back to the pool on bracket exit.
    public static IO<ReadOnlyMemory<byte>> Drain(Stream source) =>
        IO.lift(static () => new ArrayPoolBufferWriter<byte>()).Bracket(
            Use: writer => IO.liftAsync(async () => { await source.CopyToAsync(writer.AsStream()).ConfigureAwait(false); return writer.WrittenMemory; }),
            Fin: static writer => IO.lift(() => fun(writer.Dispose)()));

    // Lift every SDK call once into `RemoteStoreFault` at the leg boundary so the engine interior is total over rails
    // (boundaries.md `[EXCEPTION_CAPTURE]`): each provider's status/exception family folds structurally — the `412`
    // precondition is `Conflict`, `404` is `NotFound`, `401`/`403` is `Denied`, `413` is `Oversize`, a no-response
    // connection failure is the only transient `Transport` (status 0), every OTHER status is a typed `Transport`, and an
    // unrecognized exception is a NON-transient `Text` (a generic exception is NEVER retried — the deterministic default).
    static IO<T> Bound<T>(string provider, ContentAddress key, Func<Task<T>> call) =>
        IO.liftAsync(call) | @catch<IO, T>(static _ => true, e => IO.fail<T>(Lift(provider, key, e)));

    static RemoteStoreFault Lift(string provider, ContentAddress key, Error error) => error switch {
        RemoteStoreFault fault => fault,
        { Exception.Case: AmazonS3Exception s3 } => s3.StatusCode switch {
            HttpStatusCode.PreconditionFailed => new RemoteStoreFault.Conflict(key, "if-none-match"),
            HttpStatusCode.NotFound => new RemoteStoreFault.NotFound(key),
            HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized => new RemoteStoreFault.Denied(key, provider, s3.ErrorCode),
            HttpStatusCode.RequestEntityTooLarge => new RemoteStoreFault.Oversize(key, provider, s3.ErrorCode),
            _ => new RemoteStoreFault.Transport(provider, (int)s3.StatusCode, s3.ErrorCode),
        },
        { Exception.Case: RequestFailedException az } => az.Status switch {
            412 => new RemoteStoreFault.Conflict(key, "if-none-match"),
            404 => new RemoteStoreFault.NotFound(key),
            401 or 403 => new RemoteStoreFault.Denied(key, provider, az.ErrorCode ?? "azure"),
            413 => new RemoteStoreFault.Oversize(key, provider, az.ErrorCode ?? "azure"),
            _ => new RemoteStoreFault.Transport(provider, az.Status, az.ErrorCode ?? "azure"),
        },
        { Exception.Case: GoogleApiException gcs } => (int)gcs.HttpStatusCode switch {
            412 => new RemoteStoreFault.Conflict(key, "if-generation-match"),
            404 => new RemoteStoreFault.NotFound(key),
            401 or 403 => new RemoteStoreFault.Denied(key, provider, gcs.Error?.Message ?? "gcs"),
            413 => new RemoteStoreFault.Oversize(key, provider, gcs.Error?.Message ?? "gcs"),
            var status => new RemoteStoreFault.Transport(provider, status, gcs.Error?.Message ?? "gcs"),
        },
        { Exception.Case: PreconditionFailedException } => new RemoteStoreFault.Conflict(key, "if-none-match"),
        { Exception.Case: ObjectNotFoundException or BucketNotFoundException } => new RemoteStoreFault.NotFound(key),
        { Exception.Case: (AccessDeniedException or ForbiddenException) and { } denied } => new RemoteStoreFault.Denied(key, provider, denied.GetType().Name),
        { Exception.Case: (EntityTooLargeException or InvalidContentLengthException) and { } oversize } => new RemoteStoreFault.Oversize(key, provider, oversize.GetType().Name),
        { Exception.Case: ConnectionException } => new RemoteStoreFault.Transport(provider, 0, "connection"),
        { Exception.Case: { } ex } => new RemoteStoreFault.Text($"{provider}:{ex.GetType().Name}:{ex.Message}"),
        _ => new RemoteStoreFault.Text($"{provider}:{error.Message}"),
    };

    // --- [LEGS] -------------------------------------------------------------------------------
    // S3: low-level multipart over `IAmazonS3` (`api-objectstore` S3_MULTIPART). `Initiate` declares the whole-object
    // `XXHASH128`+`FULL_OBJECT` checksum stance, the storage class, the row's SSE (`ObjectEncryption.ApplyS3`), and the WORM
    // object-lock (`ObjectLock.ApplyS3`); `Seal` SUPPLIES the precomputed `ChecksumXXHASH128` (the content key, via `Integrity.Wire`) and carries
    // the write-once `IfNoneMatch="*"`; range read is `GetObjectRequest.ByteRange`.
    static ObjectLeg S3Leg(ObjectClient.S3 r) => new(
        Initiate: (store, tier, key) => Bound("s3", key, () => r.Client.InitiateMultipartUploadAsync(store.Lock.ApplyS3(store.Encryption.ApplyS3(new InitiateMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(), StorageClass = tier.S3Class, ChecksumAlgorithm = store.Integrity.S3Algorithm, ChecksumType = ChecksumType.FULL_OBJECT })))).Map(static x => x.UploadId),
        Stage: (token, part, bytes) => Bound("s3", default, () => r.Client.UploadPartAsync(new UploadPartRequest { BucketName = r.Bucket, UploadId = token, PartNumber = part.Number, PartSize = part.Length, InputStream = bytes.AsStream() })).Map(x => new CommittedPart(part.Number, x.ETag)),
        Seal: (store, token, key, parts, _) => Bound("s3", key, () => r.Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(), UploadId = token, IfNoneMatch = "*", ChecksumXXHASH128 = store.Integrity.Wire(key).IfNoneUnsafe(static () => null), PartETags = parts.Map(static p => new PartETag(p.Number, p.ETag)).ToList() })).Map(static _ => unit),
        Abort: (token, key) => string.IsNullOrEmpty(token) ? IO.pure(unit) : Bound("s3", key, () => r.Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(), UploadId = token })).Map(static _ => unit),
        Committed: (token, key) => Bound("s3", key, () => r.Client.ListPartsAsync(new ListPartsRequest { BucketName = r.Bucket, Key = key.Name(), UploadId = token })).Map(static x => toSeq(x.Parts).Map(static p => new CommittedPart(p.PartNumber, p.ETag))),
        Fetch: (key, range) => Bound("s3", key, () => r.Client.GetObjectAsync(new GetObjectRequest { BucketName = r.Bucket, Key = key.Name(), ByteRange = range.Match(Some: static w => new ByteRange(w.Start, w.End), None: static () => null) })).Map(static x => x.ResponseStream),
        Head: (store, key) => Bound("s3", key, () => r.Client.GetObjectMetadataAsync(r.Bucket, key.Name())).Map(x => Optional(BlobResidence.From(key, x.ContentLength, store))),
        Erase: key => Bound("s3", key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name())).Map(static _ => unit),
        Enumerate: () => Bound("s3", default, () => r.Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = r.Bucket })).Map(static x => toSeq(x.S3Objects).Map(static o => BlobName.OfName(o.Key))));

    // Azure: staged-block over `BlockBlobClient` (`api-objectstore` AZURE_BLOCKS). The block id is the part number; `Seal`
    // commits with `IfNoneMatch = ETag.All`; range read is `BlobDownloadOptions.Range = HttpRange`.
    static ObjectLeg AzureLeg(ObjectClient.Azure r) => new(
        Initiate: static (_, _, key) => IO.pure(key.Name()),
        Stage: (token, part, bytes) => Bound("azure", default, async () => { var blk = r.Container.GetBlockBlobClient(token); var id = Convert.ToBase64String(BitConverter.GetBytes(part.Number)); await blk.StageBlockAsync(id, bytes.AsStream()); return new CommittedPart(part.Number, id); }),
        Seal: (_, token, key, parts, _) => Bound("azure", key, () => r.Container.GetBlockBlobClient(token).CommitBlockListAsync(parts.Map(static p => p.ETag).ToList(), new CommitBlockListOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All } })).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: (token, key) => Bound("azure", key, () => r.Container.GetBlockBlobClient(token).GetBlockListAsync(BlockListTypes.Uncommitted)).Map(static x => toSeq(x.Value.UncommittedBlocks).Map(static b => new CommittedPart(BitConverter.ToInt32(Convert.FromBase64String(b.Name)), b.Name))),
        Fetch: (key, range) => Bound("azure", key, () => r.Container.GetBlobClient(key.Name()).DownloadStreamingAsync(new BlobDownloadOptions { Range = range.Map(static w => new HttpRange(w.Start, w.End - w.Start + 1)).IfNone(default(HttpRange)) })).Map(static x => x.Value.Content),
        Head: (store, key) => Bound("azure", key, () => r.Container.GetBlobClient(key.Name()).GetPropertiesAsync()).Map(x => Optional(BlobResidence.From(key, x.Value.ContentLength, store))),
        Erase: key => Bound("azure", key, () => r.Container.GetBlobClient(key.Name()).DeleteIfExistsAsync()).Map(static _ => unit),
        Enumerate: () => Bound("azure", default, async () => { var keys = new List<ContentAddress>(); await foreach (var b in r.Container.GetBlobsAsync()) keys.Add(BlobName.OfName(b.Name)); return toSeq(keys); }));

    // GCS: single resumable session over `StorageClient` (`api-objectstore` GCS_RESUMABLE) — the provider resumes the
    // session server-side, so the multipart legs collapse to a single chunked `UploadObjectAsync` with the genuine
    // write-once `IfGenerationMatch=0` create-if-absent precondition and the row's SSE-KMS key id through `ApplyGcs`
    // (SSE-C rides the dialed `StorageClient`'s `EncryptionKey`).
    static ObjectLeg GcsLeg(ObjectClient.Gcs r) => new(
        Initiate: static (_, _, key) => IO.pure(key.Name()),
        Stage: static (_, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, token, key, _, source) => Bound("gcs", key, () => r.Client.UploadObjectAsync(r.Bucket, token, "application/octet-stream", source.AsStream(), store.Encryption.ApplyGcs(new UploadObjectOptions { IfGenerationMatch = 0, ChunkSize = 8 * 1024 * 1024 }))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (key, range) => Bound("gcs", key, async () => { var sink = new MemoryStream(); await r.Client.DownloadObjectAsync(r.Bucket, key.Name(), sink, new DownloadObjectOptions { Range = range.Match(Some: static w => new RangeHeaderValue(w.Start, w.End), None: static () => null) }); sink.Position = 0; return (Stream)sink; }),
        Head: (store, key) => Bound("gcs", key, () => r.Client.GetObjectAsync(r.Bucket, key.Name())).Map(x => Optional(BlobResidence.From(key, (long)(x.Size ?? 0), store))),
        Erase: key => Bound("gcs", key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name())).Map(static _ => unit),
        Enumerate: () => IO.liftAsync(() => Task.FromResult(toSeq(r.Client.ListObjects(r.Bucket).Select(static o => BlobName.OfName(o.Name))))));

    // Minio: S3-compatible, multipart auto-managed inside `PutObjectAsync` (`api-minio`) — one `PutObjectArgs` write per
    // blob carrying the row's SSE through `ApplyMinio`; `PutObjectArgs` (an `ObjectWriteArgs`, NOT an
    // `ObjectConditionalQueryArgs`) cannot carry a conditional ETag, so the write-once seal is the content-address
    // INVARIANT itself — the object name IS the content hash, so a racing same-key writer writes byte-identical bytes and
    // the overwrite is benign (the conditional `412` the cloud SDKs raise is unobservable here but unnecessary, since two
    // writers to one content-key cannot disagree); range read is `GetObjectArgs.WithOffsetAndLength`.
    static ObjectLeg MinioLeg(ObjectClient.Minio r) => new(
        Initiate: static (_, _, key) => IO.pure(key.Name()),
        Stage: static (_, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, token, key, _, source) => Bound("minio", key, async () => { using var ms = source.AsStream(); await r.Client.PutObjectAsync(store.Lock.ApplyMinio(store.Encryption.ApplyMinio(new PutObjectArgs().WithBucket(r.Bucket).WithObject(token).WithStreamData(ms).WithObjectSize(source.Length).WithContentType("application/octet-stream")))); return unit; }),
        Abort: (_, key) => Bound("minio", key, async () => { await foreach (var u in r.Client.ListIncompleteUploadsEnumAsync(new ListIncompleteUploadsArgs().WithBucket(r.Bucket).WithPrefix(key.Name()))) await r.Client.RemoveIncompleteUploadAsync(new RemoveIncompleteUploadArgs().WithBucket(r.Bucket).WithObject(u.Key)); return unit; }),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (key, range) => Bound("minio", key, async () => { var sink = new MemoryStream(); var args = new GetObjectArgs().WithBucket(r.Bucket).WithObject(key.Name()).WithCallbackStream(s => s.CopyTo(sink)); await r.Client.GetObjectAsync(range.Match(Some: w => args.WithOffsetAndLength(w.Start, w.End - w.Start + 1), None: () => args)); sink.Position = 0; return (Stream)sink; }),
        Head: (store, key) => Bound("minio", key, () => r.Client.StatObjectAsync(new StatObjectArgs().WithBucket(r.Bucket).WithObject(key.Name()))).Map(x => Optional(BlobResidence.From(key, x.Size, store))),
        Erase: key => Bound("minio", key, () => r.Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(r.Bucket).WithObject(key.Name()))).Map(static _ => unit),
        Enumerate: () => Bound("minio", default, async () => { var keys = new List<ContentAddress>(); await foreach (var i in r.Client.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(r.Bucket).WithRecursive(true))) keys.Add(BlobName.OfName(i.Key)); return toSeq(keys); }));
}
```

## [04]-[BLOB_GC]

- Owner: `BlobCatalogRow` the content-lineage retention row every blob carries (the same row the snapshot spine has, `H10`), carrying the `WormUntil` object-lock window beside the `Tenant`/`Bytes`/`Tier` columns and projecting to the `Version/retention#SWEEP_AND_GC` `RetentionFact` the ONE deletion executor consumes; `PendingWrite` the write-blob-first pending ledger; `BlobGc` the static surface owning ONLY the write-blob-first protocol, the in-flight-fence eligibility predicate it contributes to the retention sweep, and the WORM-aware `evict` arrow surfacing `RemoteStoreFault.Locked` — it does NOT own a second deletion executor.
- Entry: `public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ReadOnlyMemory<byte> source, ChunkMembership index, Func<PendingWrite, IO<Unit>> open, Func<BlobCatalogRow, IO<Unit>> catalog, Func<ContentAddress, IO<Unit>> close, ClockPolicy clocks)` opens the `PendingWrite` ledger row, write-once-seals the blob through `store.Put`, commits the catalog row, then clears the pending row — so the blob lands before any event references it; `public static Func<ContentAddress, bool> InFlightFence(Seq<PendingWrite> pending, Instant now, Duration grace)` is the eligibility predicate THIS owner contributes to the `Version/retention#SWEEP_AND_GC` `RetentionSweep.Sweep` — a key under an open `PendingWrite` younger than grace is in-flight, NOT an orphan; `public static IO<SweepReceipt> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Reachability reachable, Seq<Hold> holds, Duration grace, ClockPolicy clocks, CorrelationId correlation)` projects the catalog to `RetentionFact`s and routes the WHOLE sweep through `RetentionSweep.Sweep` (the `blob` class, the `reachable` mark, the `holds`, and `InFlightFence` as the injected eligibility) then `RetentionSweep.Execute` with `key => store.Delete(client, key)` as the evict arrow AND `(key, tier) => Demote(store, client, key, tier)` as the cold-tier re-PUT arrow (the `blob` class is `NeverEvict`, so an aged reachable blob `Cool`s one rung down the `RetentionCeiling.Demote` ladder instead of collecting) — never a hand-rolled `List`-then-`Filter` parallel sweeper.
- Auto: the write protocol is WRITE-BLOB-FIRST with THREE durable marks — `open` the `PendingWrite` ledger row, write the blob through `store.Put` (the one generic transfer engine), commit the catalog row, `close` the pending row; the Marten event references the immutable content key AFTER (a separate axis), so a crash between the blob write and the event reference leaves a present blob with an open pending row (sweep-protected until grace) and no event reference — a collectible orphan once grace lapses, never a dangling event reference — and the `412`-noop makes a re-drive survive; the geometry blob is NOT in the same PostgreSQL transaction as the event (the ONE txn owner is identity+event in the Marten session, `H10`), so the blob is write-first and reference-after; the orphan reclaim is NOT a second sweeper — `BlobGc.Sweep` projects each `BlobCatalogRow` to a `RetentionFact` and hands the WHOLE decision to the `Version/retention#SWEEP_AND_GC` `RetentionSweep`, the system's ONE deletion executor, so the receipt stream is one ledger: the `reachable` mark (the `Reachability` carrier whose membership probe is the owner's value-object, never a raw `LanguageExt.HashSet`) folds every AS-OF cut so a blob a historical version references is never collected (`live.Reachable(key)` exits first in the retention fold), and the in-flight fence rides the injected `eligible` predicate so an un-aged pending write is held, never reaped — the alternative to full-history reachability being geometry-GC-forbidden (dedup + cold-tiering).
- Receipt: a blob write rides `store.blob.write` carrying the content key and bytes; the GC reclaim rides the `Version/retention#SWEEP_AND_GC` `SweepReceipt` on the retention sweep's own fact stream (the orphan count and reclaimed bytes are the retention executor's receipt, never a parallel `store.blob.gc` stream the blob lane re-mints).
- Packages: System.IO.Hashing, NodaTime (`Instant`/`Duration` the `WormUntil` window), LanguageExt.Core (`Seq`/`Choose`/`IO.fail`), System.Collections.Frozen (`FrozenDictionary` the WORM index), Thinktecture.Runtime.Extensions (`RetentionClass.Blob`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability` the `Version/retention#SWEEP_AND_GC` surface this owner composes), BCL inbox. (The WORM/object-lock SET is the `#OBJECT_STORE` `ObjectLock` write-leg's concern, never a KMS or SDK call here — the blob lane only READS the catalog `WormUntil` and mints `RemoteStoreFault.Locked`; SSE-KMS needs only the key-id STRING, the KMS signing SDKs being the `Version/provenance#ATTESTED_LEDGER` owner's, never composed at the blob lane.)
- Growth: a new catalog column is one field on `BlobCatalogRow` (as `WormUntil` is); a new WORM/object-lock stance is one `ObjectLock` case the `WriteBlobFirst` `Lock.Until` and the `WormEvict` arrow both read with zero new surface; zero new surface — a head-only blob GC, a `BlobGc`-local `List`-then-`Filter` sweep parallel to the retention executor, a same-PG-txn blob write, a two-ORM atomicity dance, a blob-lane-local retention executor that re-decides eviction beside `RetentionSweep`, or a free-string blob name is the deleted form because the GC routes through the ONE `RetentionSweep` over the full history (the WORM fence riding the injected `eligible` predicate and the typed `WormEvict` arrow, never a second sweeper), the blob is write-first content-addressed, and identity+event is the one Marten-session transaction.
- Boundary: the geometry blob carries the SAME content-lineage and retention-catalog row the snapshot spine has (`H10`) and registers in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — a blob-lane-local deletion executor is the deleted form, the blob lane contributing only its `RetentionFact` projection and its `InFlightFence` predicate to the `RetentionSweep` every lane routes through, and geometry GC over head alone is FORBIDDEN (the retention `Mark` folds every AS-OF cut so a blob a historical version references survives); the write-blob-first + `412`-noop protocol survives a crash (orphan blob, never dangling reference) and the `PendingWrite` ledger row OPENS before the blob `Put` and CLOSES after the catalog commit, so the in-flight fence distinguishes an in-flight write from an orphan even before the catalog row exists (the fail-safe never reaps an un-ageable present write); the ONE transaction owner for identity+event is the Marten `IDocumentSession` (`Element/graph#STORE_RAIL`), the blob being write-first and reference-after with no free two-ORM atomicity; the SSE key MATERIAL is a key-id STRING carried on the `ObjectEncryption` case (`ManagedKey` the SSE-KMS key id the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` or the host KMS minted out-of-band, `CustomerKey` the SSE-C key + MD5, `ProviderManaged` the account/bucket-default) and `ObjectEncryption.ApplyS3`/`ApplyGcs`/`ApplyMinio` stamp it on the wire at each provider's request (Azure at the dialed client) — a blob-lane-local KMS envelope is the deleted form (the DEK-wrapping envelope lifecycle and both cloud-KMS keyrings are the `Element/identity#AUTHORITY` owner's), key acquisition being a host connection input never a fence member; the WORM/object-lock retention-until is a write-policy stance carried on the `ObjectLock` case (`Off` the default, `Governance`/`Compliance` a `Retain` window) — `ObjectLock.ApplyS3`/`ApplyMinio` SET it on the wire and `WriteBlobFirst` records `Lock.Until(now)` onto `BlobCatalogRow.WormUntil`, so the GC's `WormEvict` arrow refuses a key under an active window with `RemoteStoreFault.Locked` (the eligibility fence already holds it out of selection, the arrow the defense-in-depth second gate), making `Locked` a genuinely reachable domain fault rather than an opaque provider 403 the `Lift` would mis-fold to `Denied`; the per-tenant prefix (`BlobName` `{TenantId:x32}/{key:x32}`) partitions the blob namespace by `TenantId` so a multi-tenant store isolates by construction, the `BlobCatalogRow.Tenant` column stamps the same `TenantContext.Current.TenantId`, and the retention catalog `Seq<BlobCatalogRow>` the caller hands `BlobGc.Sweep` is filtered by the `Element/identity#ELEMENT_IDENTITY` `Tenant` RLS partition (`current_setting('rasm.tenant')::uuid`) at the catalog query so a cross-tenant reclaim is unrepresentable end to end — the sweep never sees another tenant's rows, and the per-tenant object-name prefix means even a mis-supplied catalog row resolves to a name under the wrong tenant's prefix that the provider does not hold.

```csharp signature
// The content-lineage retention row. `WormUntil` is the WORM/object-lock retention-until the `Store/blobstore#OBJECT_STORE`
// `ObjectLock` write stance SET on the blob (`ObjectLock.Until(now)`) — `None` for the default `ObjectLock.Off` rows, an
// `Instant` for a compliance/governance-class blob whose bytes are provider-immutable until the window lapses; it is the ONE
// column that makes `RemoteStoreFault.Locked` genuinely reachable, the sweep's `evict` arrow refusing a key still under it.
public sealed record BlobCatalogRow(ContentAddress Key, RetentionClass Class, long Bytes, StorageTier Tier, Option<ContentAddress> Lineage, UInt128 Tenant, DataClassification Classification, Option<Instant> WormUntil, Instant At);
public readonly record struct PendingWrite(ContentAddress Key, long Bytes, Instant Started);

public static class BlobGc {
    // WRITE-BLOB-FIRST: `open` appends the `PendingWrite` ledger row, `Put` write-once-seals the blob (the row's `ObjectLock`
    // stance SET on the wire), the catalog row commits carrying the WORM `Until` window, then `close` clears the pending row —
    // so the protocol has THREE durable marks and the sweep reads the open pending set as the in-flight fence. A crash before
    // the catalog commit leaves a present blob with an OPEN pending row (protected from the sweep until grace) and no event
    // reference (a collectible orphan once grace lapses, never a dangling reference); the `412`-noop makes a re-drive of `Put`
    // survive. The blob is NOT in the event's PG txn (`H10`). `WormUntil = store.Lock.Until(clocks.Now)` so a compliance-class
    // write records the immutability window the SDK SET on the object, the catalog and the provider agreeing on the lock.
    public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ReadOnlyMemory<byte> source, ChunkMembership index, Func<PendingWrite, IO<Unit>> open, Func<BlobCatalogRow, IO<Unit>> catalog, Func<ContentAddress, IO<Unit>> close, ClockPolicy clocks) =>
        from _o in open(new PendingWrite(key, source.Length, clocks.Now))
        from residence in store.Put(client, BlobResidence.From(key, source.Length, store), ContentChunker.Chunk(store.Chunking, source), index, source, static _ => IO.pure(unit))
        from _c in catalog(new BlobCatalogRow(key, RetentionClass.Blob, source.Length, store.Tier, None, TenantContext.Current.TenantId, DataClassification.Internal, store.Lock.Until(clocks.Now), clocks.Now))
        from _x in close(key)
        select residence;

    // The in-flight fence THIS owner contributes to the retention sweep's `eligible` predicate: a key under an OPEN
    // `PendingWrite` younger than grace is an in-flight write, NOT an orphan, so it is INELIGIBLE for collection — every
    // other key is eligible (reachability + holds + age are the retention sweep's own stages, never re-decided here).
    // This is the ONE crash-window fence the catalog alone cannot express (a present blob whose catalog row has not yet
    // committed has no inventory row, so the pending ledger is the only evidence it is mid-write).
    public static Func<ContentAddress, bool> InFlightFence(Seq<PendingWrite> pending, Instant now, Duration grace) =>
        key => pending.Find(w => w.Key == key).Match(Some: w => now - w.Started >= grace, None: () => true);

    // The blob catalog row IS a `RetentionFact` for the `blob` class — the sealed `Bytes` field is the byte figure the
    // retention sweep budgets on (never a later filesystem stat), the content `Key` the identity, `Tier` the CURRENT
    // `StorageTier` the never-evict cold-tiering verdict reads (the `blob` class is `LossPolicy.NeverEvict`, so an aged
    // reachable blob `Cool`s one rung down the `RetentionCeiling.Demote` ladder rather than collecting), the `At` the age stamp.
    static RetentionFact ToFact(BlobCatalogRow row) => new(RetentionClass.Blob, row.Key, row.Bytes, row.Tier, row.At);

    // The cold-tier demotion the `blob` class's `LossPolicy.NeverEvict` cooling verdict re-PUTs at: the existing object's
    // bytes drained once through the read leg and re-sealed at the COLDER `StorageTier` (the `Version/retention#SWEEP_AND_GC`
    // `RetentionCeiling.Demote` ladder's next rung) — `Cool` keeps the blob resident, just one storage-class rung down, so
    // the demotion is a fetch → `Drain` → content-addressed re-`Put` against the same content key UNDER the colder tier the
    // `Put` `at` override threads onto the leg `Initiate` `S3StorageClass` (the same egress parameterization the row tier
    // defaults; the write-once seal makes the same-key re-PUT a benign `412`-noop at providers that already hold the
    // bytes, the storage-class change taking effect server-side). The tier rides the PUT call, never a cloned smart-enum
    // row or a parallel tier-transition side channel beside the one executor.
    static IO<Unit> Demote(ObjectStore store, ObjectClient client, ContentAddress key, StorageTier colder) =>
        from stream in store.Fetch(client, key, None)
        from source in ObjectIo.Drain(stream)
        from _ in store.Put(client, BlobResidence.From(key, source.Length, store) with { Tier = colder }, ContentChunker.Chunk(store.Chunking, source), ChunkMembership.None, source, static _ => IO.pure(unit), colder)
        select unit;

    // The WORM-aware evict arrow the blob lane injects into the retention `Execute`: a key whose catalog row carries an
    // ACTIVE `WormUntil` (`now < until`) is provider-immutable, so the arrow FAILS `RemoteStoreFault.Locked` (carrying the
    // mode and window) BEFORE calling `store.Delete` — surfacing the lock as a typed domain fault rather than letting the
    // provider raise an opaque 403 the `Lift` would mis-fold to `Denied` (a retention-block and an auth-denial are
    // indistinguishable by status). A lapsed or absent lock deletes normally. The retention `eligible` predicate ALSO holds
    // a WORM-locked key (it never reaches an evict verdict in steady state), so this arrow is the defense-in-depth second
    // gate: the eligibility fence keeps the sweep from SELECTING a locked blob, this arrow refuses to EXECUTE one if a
    // compliance window was set after the verdict was computed — the catalog WORM column is the single source both read.
    static Func<ContentAddress, IO<Unit>> WormEvict(ObjectStore store, ObjectClient client, FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm, Instant now) =>
        key => worm.TryGetValue(key, out var w) && now < w.Until
            ? IO.fail<Unit>(new RemoteStoreFault.Locked(key, w.Mode, w.Until))
            : store.Delete(client, key);

    // The reclaim is NOT a second sweeper: the catalog IS the authoritative inventory (a `store.List()`-then-`Filter` is
    // the deleted parallel executor), each row projects to a `RetentionFact`, and the WHOLE decision routes through the
    // `Version/retention#SWEEP_AND_GC` `RetentionSweep` — `Sweep` over the `blob` class with the full-history `reachable`
    // mark (a blob a historical AS-OF cut references exits first), the `holds`, `InFlightFence` AND the WORM-active keys as
    // the injected `eligible` predicate (a locked blob is ineligible like an in-flight write), then `Execute` with the
    // `WormEvict` arrow (refusing a still-locked key with `RemoteStoreFault.Locked`) AND `Demote` as the cold-tier re-PUT arrow
    // (the `blob` class is `NeverEvict`, so an aged reachable blob `Cool`s one rung instead of collecting) — so the orphan
    // reclaim, the cold-tiering, and the snapshot-spine reclaim share ONE executor and ONE receipt ledger, the blob lane
    // owning only its fact projection, its in-flight + WORM fence, and its tier-transition re-PUT.
    public static IO<SweepReceipt> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Reachability reachable, Seq<Hold> holds, Duration grace, ClockPolicy clocks, CorrelationId correlation) {
        var worm = catalog.Choose(static r => r.WormUntil.Map(u => (r.Key, (Mode: "worm", Until: u)))).ToFrozenDictionary(static t => t.Key, static t => t.Item2);
        var fence = InFlightFence(pending, clocks.Now, grace);
        var (verdicts, _) = RetentionSweep.Sweep(
            RetentionClass.Blob, catalog.Map(ToFact), holds, reachable,
            key => fence(key) && !(worm.TryGetValue(key, out var w) && clocks.Now < w.Until), clocks.Now, correlation);
        return RetentionSweep.Execute(RetentionClass.Blob, verdicts, WormEvict(store, client, worm, clocks.Now), (key, tier) => Demote(store, client, key, tier), correlation, clocks);
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | write protocol      | open-pending -> blob -> catalog -> close-pending | crash leaves a pending-fenced orphan, never a dangling reference |
|  [02]   | txn owner           | identity+event in the Marten session   | blob is write-first; no two-ORM atomicity (`H10`)         |
|  [03]   | GC executor         | the ONE `Version/retention` `RetentionSweep` | blob lane projects `RetentionFact` + `InFlightFence`; no parallel sweeper |
|  [04]   | GC reachability     | mark over EVERY AS-OF cut              | full-history; head-only GC is forbidden (`H10`)           |
|  [05]   | lineage catalog     | same row the snapshot spine has        | registers in the `blob` retention class; one GC governs both |
|  [06]   | encryption          | `ApplyS3`/`ApplyGcs`/`ApplyMinio`; Azure at the client | applied on every provider's wire, not decorative; SSE key id host-supplied |
|  [07]   | WORM/object-lock    | `WormUntil` column + `WormEvict` arrow | `Locked` reachable; eligibility fence + typed evict refusal, no provider 403 leak |
|  [08]   | tenancy             | `Tenant` column + RLS-filtered catalog | per-tenant name prefix; cross-tenant reclaim unrepresentable |
