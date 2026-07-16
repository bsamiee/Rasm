# [PERSISTENCE_STORE_BLOBSTORE]

Rasm.Persistence stores geometry and coverage-raster bytes as a content-keyed object store — one `ObjectStore` `[SmartEnum]` provider axis behind the `BlobRemote` placement contract, five rows deep (`S3`/`Azure`/`GCS`/`Minio` credentialed plus the credential-free `Presigned` grant), every write content-addressed, write-once-sealed through the `ConditionalWrite` `412`-noop, and routed through the one `MultipartTransfer.Upload` receipt path. Object names derive from the seam `ContentAddress` the kernel `XxHash128` mints — `ContentAddress.Of(UInt128)` wraps the `Object` node's `RepresentationContentHash` (`Body` display GLB, `Box` lossless heavy, `Axis` structural line, `FootPrint` space-boundary ring, each `Option<UInt128>`) and the `Coverage` node's `CoverageGrid.RasterKey`, so this store holds bytes and never mints a second identity, and PostgreSQL/SQLite/DuckDB never appear because the durable home for geometry and raster bytes is the object plane, never a relational row. Writes land WRITE-BLOB-FIRST — content-address the blob, write it, then reference the immutable hash from a Marten event outside the event's PostgreSQL transaction — so a crash yields a collectible orphan, never a dangling reference, and every blob registers in the `Version/retention#RETENTION_CLASSES` `blob` class where one full-history reachability GC governs it and the snapshot spine alike (`H10`). Identity plus event share the one `IDocumentSession` transaction (`Element/graph#STORE_RAIL`); the blob is write-first, referenced-after, never a two-ORM atomicity dance.

`ContentAddress`, `ChunkManifest`, and `ContentChunker` compose from `Element/codec`; the retention rail (`RetentionClass`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability`/`RetentionSweep`, the one deletion executor the blob GC routes through) from `Version/retention`; `ProjectionContext` (the `Element/graph#STORE_RAIL` [A.1] frame — mark/clock/correlation/tenant as injected values), `ReceiptSinkPort`, and `CommunityToolkit.HighPerformance` from the substrate. An above-seam `Rasm.Compute` analysis runner reaches analytical `Axis`/`FootPrint` blobs one-hop through its app-wired `Graph/element#NODE_MODEL` `GeometrySource` port — the seam owns the decode contract, this store the bytes. SSE key material stays a key-id string this lane only stamps on the wire; the DEK-wrapping envelope lifecycle and both cloud-KMS keyrings (signing and envelope) belong to `Element/identity#AUTHORITY`, never a blob-lane-local KMS envelope.

## [01]-[INDEX]

- [01]-[OBJECT_STORE]: the five-provider axis (four credentialed + the presigned-grant row) projecting `BlobRemote`, the write-once seal, the per-row checksum-honesty stance, the SSE + WORM/object-lock/legal-hold + client-sealed write stances, the issuer-side grant mint, the content-lineage catalog, and the closed fault rail.
- [02]-[MULTIPART_TRANSFER]: the content-defined-chunk upload packing whole chunks into provider parts, exact-object conditional dedup, durable session resume, and explicit abandon.
- [03]-[BLOB_GC]: the content-lineage retention row (with its WORM `WormUntil` window) projecting to the `Version/retention` `RetentionFact`, the write-blob-first protocol + the in-flight + WORM fence, and the reclaim routed through the ONE `RetentionSweep` deletion executor with a typed `WormEvict` arrow (never a blob-lane-local sweeper).

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore` the `[SmartEnum<string>]` provider axis under `ComparerAccessors.StringOrdinal` — each row carries the `PartSize` floor, `ChunkPolicy` window, `ObjectChecksum` integrity, `ConditionalWrite` seal, `StorageTier` class, `ObjectEncryption` SSE, and `ObjectLock` WORM stances and builds its `BlobRemote` from the resolved `ObjectClient` (the `[Union]` whose `Map` owns per-leg dispatch); `ObjectChecksum`/`StorageTier`/`ObjectEncryption`/`ObjectLock` the closed write-policy vocabularies; `RemoteStoreFault` the closed boundary fault family.
- Cases: `s3`, `azure-blob`, `gcs`, `minio`, `presigned` — the sweep closes here, no relational engine appearing because the object plane owns durable geometry and raster bytes behind `BlobRemote`. `presigned` inverts the row: a `GrantMinter`+`Roster` pair and a host-dialed `HttpClient` replace endpoint+credential, reaching domain-cloud planes no credentialed row can (the client-side credential never exists) and single-shot by construction (upstream `FileMeta` carries no checksum/etag, no multipart/resume); Pollination seeds the minter, any other domain one more minter value, a sixth provider one row.
- Entry: `Placement` projects the row's `BlobRemote` with its write arrow routed through `MultipartTransfer.Upload` — the composed receipt path, so the frame's correlation lands on every residence and receipt; `Put` drains the source once, partitions through `ContentChunker.Chunk`, and rides the row `Tier` or the `at` override the `#BLOB_GC` cold-tier re-PUT supplies; `Fetch`/`Head` are the read legs; `Grant(client, demand, frame)` is the ISSUER mint — the inverse of the presigned CONSUMER row — and `GrantDemand` carries operation plus lifetime as one admitted request so no deadline knob travels beside it. A credential-free viewer streams the resulting grant provider-direct after the caller gates the demand through `Element/authority#AUTHORITY` `Admit`.
- Auto: content-defined chunks pack into provider parts of at least `PartSize`, but only the exact object-name seal proves whole-blob residence; chunk membership never short-circuits a provider that cannot assemble an object from foreign parts. A re-put of an existing key `412`s to `RemoteStoreFault.Conflict`, and one `@catch` arm confirms the exact object by `Head` before yielding the benign no-op. Checksum honesty is per row — only S3 verifies the `XxHash128` content key server-side under `ChecksumType.FULL_OBJECT`, so only the S3 row declares it while Azure `Crc64`, GCS `Crc32c`, and Minio `None` fall back to SDK-native transfer integrity. `Encryption` applies on EVERY provider's wire — `ApplyS3`/`ApplyGcs`/`ApplyMinio` stamp the SSE stance at the request, Azure SSE bakes into the host-dialed container — so a leg silently dropping the column is the deleted form. `Lock` applies the same way — `ApplyS3`/`ApplyMinio` stamp the object-lock mode+retention-until, Azure container-immutability and GCS bucket-retention host-dialed — so `Governance`/`Compliance` make the bytes immutable for `Retain` and record the window through `Lock.Until` onto `#BLOB_GC` `BlobCatalogRow.WormUntil`, which is what makes `RemoteStoreFault.Locked` reachable.
- Receipt: a `BlobTransferFact` rides `store.blob.*` — `part` per uploaded window, `resume` per skipped-committed window, `conflict-noop` per exact-object `412`, `abort` per torn ceremony; the envelope stamps the HLC, so the fact carries no `Instant`.
- Packages: AWSSDK.S3 (`GetPreSignedURL`/`GetPreSignedUrlRequest` the issuer leg), Azure.Storage.Blobs (`BlobClient.GenerateSasUri`/`BlobSasPermissions`), Google.Cloud.Storage.V1 (`UrlSigner.SignAsync` the V4 issuer), Minio (`PresignedGetObjectArgs`/`PresignedPutObjectArgs`), CommunityToolkit.HighPerformance, System.IO.Hashing, System.Security.Cryptography inbox (`AesGcm`/`CryptographicOperations.ZeroMemory` the client-seal pair), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: one `ObjectStore` row absorbs a new provider with zero new surface (`presigned` exercised it — one row, one leg, one in-band fault); a new presigned domain is one `GrantMinter` value, a new storage class one `StorageTier` row, a new SSE stance one `ObjectEncryption` case (`ClientSealed` exercised it — one case, one seal/open pair, one catalog column), a new WORM stance one `ObjectLock` case, a new checksum posture one `ObjectChecksum` row, a new grant modality one `GrantRequest` case the leg `Issue` arms dispatch, a tighter window one `ChunkPolicy` row at `#CONTENT_CHUNKING`, a new boundary failure one `RemoteStoreFault` case; a per-provider upload service, a row delegate re-discriminating the union, a second HTTP uploader, or a `client is ObjectClient.S3 ?` guard is the deleted form because the union case IS the dispatch.
- Boundary: the content-key object name derives from the `Element/codec#CONTENT_ADDRESS` identity the kernel mints, so the store never mints a second identity and the M2-neutral representation map leaks no IFC name (the `Bim` projector owns IFC mapping behind those neutral keys); per-leg dispatch is `ObjectClient.Map`, so a per-provider service class and a mismatch guard are the deleted forms. Write-once is the optimistic-concurrency edge each provider exposes (S3/Minio `IfNoneMatch:*`, Azure `ETag.All`, GCS `IfGenerationMatch:0`), so a content-address store needs no read-before-write and a `412` folds to `RemoteStoreFault.Conflict` treated as success; every SDK exception lifts once into `RemoteStoreFault` at this edge and `Transport.IsTransient` is the sole `Schedule`-retry gate, so a throttle/`5xx` re-drives while `Conflict`/`NotFound`/`Locked`/`IntegrityBreach`/`Denied`/`Oversize` is deterministic. Credential, endpoint, and region are host-resolved connection inputs, never fence members; the presigned row inverts the boundary — no client-side credential, the minter closure composed at the app root sees only `GrantRequest → IO<ObjectGrant>`, and only that expiry-aware minter can mint `GrantExpired`; a bare HTTP `403` remains `Denied` because status alone cannot distinguish expiry, signature failure, or policy refusal.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `RemoteStoreFault` derives from the KERNEL federation base, so the bare `Expected` names `Rasm.Domain.Expected`
// (parameterless protected ctor + `Category` virtual) and NEVER the `LanguageExt.Common.Expected` whose
// `(string,int,Option)` ctor is the deleted form. `FaultBand` (the graph#FAULT_TABLES registry) and the
// `EnvelopeKeyring`/`EnvelopeAad`/`WrappedKey` envelope surface arrive from the Element tier.
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectChecksum {
    public static readonly ObjectChecksum XxHash128 = new("xxh128", ChecksumAlgorithm.XXHASH128);
    public static readonly ObjectChecksum Crc64 = new("crc64", ChecksumAlgorithm.CRC64NVME);
    // The GCS SDK-native whole-object stance (`Object.Crc32c` — GCS exposes NO CRC64); the SDK verifies it
    // internally, so the row supplies no S3 wire algorithm and no `Wire` digest.
    public static readonly ObjectChecksum Crc32c = new("crc32c", null);
    public static readonly ObjectChecksum None = new("none", null);
    public ChecksumAlgorithm? S3Algorithm { get; }
    private ObjectChecksum(string key, ChecksumAlgorithm? s3Algorithm) : this(key) => S3Algorithm = s3Algorithm;
    // The content key IS a 128-bit `XxHash128`, so the `XXHASH128` row hands S3 the SAME digest base64-encoded as the
    // PRECOMPUTED whole-object checksum — never a second hash. `Wire` is the `x-amz-checksum-xxh128` value the S3 `Seal`
    // SUPPLIES on `CompleteMultipartUploadRequest.ChecksumXXHASH128` (paired with the `Initiate`'s `ChecksumType.FULL_OBJECT`
    // stance) so the provider verifies the sealed object against the content key with ZERO server-side re-hash — the content
    // key IS the supplied digest. A non-`XxHash128` row supplies `None` and falls back to the provider's SDK-native transfer
    // integrity (`Crc64` on Azure, `Crc32c` on GCS, Minio's transport check under `None`). `Seal` consumes this as its live whole-object checksum, so
    // `Wire` is a load-bearing upload member, never a decorative single-purpose projection.
    public Option<string> Wire(ContentAddress key) {
        byte[] digest = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(digest, key.Value);
        return this == XxHash128 ? Some(Convert.ToBase64String(digest)) : None;
    }

    public static ReadOnlyMemory<byte> Azure(ReadOnlySpan<byte> payload) {
        byte[] digest = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(digest, System.IO.Hashing.Crc64.HashToUInt64(payload));
        return digest;
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
    // The zero-trust residence class no SSE stance reaches: the bytes AES-GCM-seal under a per-blob DEK BEFORE
    // chunking/upload (`SealSource`), so provider-held keys never see plaintext — classified/confidentiality-bound
    // models become admissible on any provider row. The DEK mints through the identity-tier `#KEY_ENVELOPE`
    // `EnvelopeKeyring` under the AAD binding; the `WrappedKey` rides `BlobCatalogRow.Dek`; every provider `Apply*`
    // arm is a no-op and the row pairs with `ObjectChecksum.None`. `Acquire` is the content-key CAS: every writer
    // for one address receives the same wrapped DEK, and the nonce
    // derives from that address. A resume replays identical ciphertext and a race catalogs only one envelope.
    public sealed record ClientSealed(
        EnvelopeKeyring Keyring,
        EnvelopeAad Aad,
        Func<ContentAddress, IO<(ReadOnlyMemory<byte> Dek, WrappedKey Wrapped)>> Acquire) : ObjectEncryption;

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
        clientSealed:    static (r, _) => r,
        state: request);

    // GCS: SSE-KMS rides `UploadObjectOptions.KmsKeyName`; SSE-C (`CustomerKey`) is a CLIENT-construction fact through
    // `StorageClient.CreateAsync(GoogleCredential?, EncryptionKey?)` the host dials, so the upload-options arm carries
    // only the KMS key id and the customer-key case is a client no-op here (the EncryptionKey rode the dialed client).
    public UploadObjectOptions ApplyGcs(UploadObjectOptions options) => Switch(
        providerManaged: static (o, _) => o,
        managedKey:      static (o, k) => (o.KmsKeyName = k.KeyId, o).Item2,
        customerKey:     static (o, _) => o,
        clientSealed:    static (o, _) => o,
        state: options);

    // Minio (S3-compatible): the SSE stance is the `Minio.DataModel.Encryption.IServerSideEncryption` on the put —
    // `SSEKMS` carries the KMS key id, `SSEC` the 32-byte customer key, `SSES3` the server-managed default — applied
    // through the inherited `EncryptionArgs.WithServerSideEncryption`, so the Minio put honors the same column.
    public PutObjectArgs ApplyMinio(PutObjectArgs args) => Switch(
        providerManaged: static (a, _) => a,
        managedKey:      static (a, k) => a.WithServerSideEncryption(new SSEKMS(k.KeyId)),
        customerKey:     static (a, c) => a.WithServerSideEncryption(new SSEC(c.Key.ToArray())),
        clientSealed:    static (a, _) => a,
        state: args);

    // The client-seal pair: `SealSource` resolves one content-key-stable DEK through `Acquire`, AES-GCM-seals the
    // payload (12-byte nonce | 16-byte tag | ciphertext), zeroizes the plaintext DEK, and yields the ciphertext
    // plus the `WrappedKey` the catalog row persists; `OpenSource` unwraps the DEK and opens the frame on the
    // read path. Every non-`ClientSealed` case passes bytes through untouched with no DEK — ONE polymorphic
    // transform on the write/read path, never a parallel encrypting store.
    public IO<(ReadOnlyMemory<byte> Bytes, Option<WrappedKey> Dek)> SealSource(ContentAddress key, ReadOnlyMemory<byte> plain) =>
        this is ClientSealed sealed_
            ? sealed_.Acquire(key).Map(minted => {
                byte[] framed = new byte[12 + 16 + plain.Length];
                BinaryPrimitives.WriteUInt64BigEndian(framed.AsSpan(0, 8), (ulong)(key.Value >> 64));
                BinaryPrimitives.WriteUInt32BigEndian(framed.AsSpan(8, 4), (uint)(key.Value >> 32));
                try {
                    using System.Security.Cryptography.AesGcm aead = new(minted.Dek.Span, tagSizeInBytes: 16);
                    aead.Encrypt(framed.AsSpan(0, 12), plain.Span, framed.AsSpan(28), framed.AsSpan(12, 16));
                }
                finally {
                    System.Security.Cryptography.CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsMemory(minted.Dek).Span);
                }
                return ((ReadOnlyMemory<byte>)framed, Some(minted.Wrapped));
            })
            : IO.pure(((ReadOnlyMemory<byte>)plain, Option<WrappedKey>.None));

    public IO<ReadOnlyMemory<byte>> OpenSource(ContentAddress content, ReadOnlyMemory<byte> framed, Option<WrappedKey> dek) =>
        (this, dek) switch {
            (ClientSealed, { IsNone: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-envelope")),
            (ClientSealed, _) when framed.Length < 28 => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-frame")),
            (ClientSealed sealed_, { IsSome: true }) => sealed_.Keyring.Unwrap(dek.ValueUnsafe(), sealed_.Aad).Map(key => {
                byte[] plain = new byte[framed.Length - 28];
                try {
                    using System.Security.Cryptography.AesGcm aead = new(key.Span, tagSizeInBytes: 16);
                    aead.Decrypt(framed.Span[..12], framed.Span[28..], framed.Span[12..28], plain);
                }
                finally {
                    System.Security.Cryptography.CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsMemory(key).Span);
                }
                return (ReadOnlyMemory<byte>)plain;
            }),
            (_, { IsSome: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "unexpected-envelope")),
            _ => IO.pure(framed),
        };

    public IO<Stream> Read(
        ContentAddress key,
        Option<(long Start, long End)> range,
        Func<ContentAddress, IO<Option<WrappedKey>>> envelope,
        Func<Option<(long Start, long End)>, IO<Stream>> fetch) =>
        this is ClientSealed
            ? from dek in envelope(key)
              from raw in fetch(None)
              from plain in ObjectIo.Drain(raw, framed => OpenSource(key, framed, dek))
              from selected in range.Match(
                  Some: window => window is { Start: >= 0 } && window.End >= window.Start && window.End < plain.Length
                      ? IO.pure(plain.Slice(checked((int)window.Start), checked((int)(window.End - window.Start + 1))))
                      : IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.InvalidRange(key, window.Start, window.End, plain.Length)),
                  None: () => IO.pure(plain))
              select selected.AsStream()
            : fetch(range);
}

[Union]
public abstract partial record ObjectLock {
    public sealed record Off : ObjectLock;
    public sealed record Governance(Duration Retain) : ObjectLock;
    public sealed record Compliance(Duration Retain) : ObjectLock;
    // The third admitted lock modality (`ObjectLockLegalHoldStatus` on the S3 initiate): an INDEFINITE hold
    // with no retention date — released by an operator action, never a lapsing window — so `Until` projects
    // `Instant.MaxValue` and the GC fence holds the blob until the hold row is lifted from the catalog.
    public sealed record LegalHold : ObjectLock;

    // The WORM/object-lock retention stance APPLIED on the write so `RemoteStoreFault.Locked` is genuinely REACHABLE — a
    // compliance-class blob written under an active retention-until cannot be deleted until the window lapses, the SET being
    // the only thing that makes the retention/GC `Locked` surfacing real (the fault `H10` declares is otherwise unmintable).
    // `Off` writes no lock member; `Governance`/`Compliance` stamp the object-lock mode + a retention-until of `now +
    // Retain` where `now` is the ONE injected frame instant `Upload` samples ONCE and threads to every arm AND to the
    // catalog `WormUntil` — a per-arm ambient `DateTime.UtcNow` read is the two-clock split-brain that lets the provider
    // window and the catalog window diverge under skew or retry, the deleted form. S3 carries it on the multipart-INITIATE
    // (`ObjectLockMode`/`ObjectLockRetainUntilDate`), Minio on the put through the inherited
    // `ObjectWriteArgs.WithRetentionConfiguration` (`x-amz-object-lock-mode`/`x-amz-object-lock-retain-until-date`); Azure
    // immutability is a CONTAINER policy the host dials (the `BlobContainerClient` carries the immutability scope, no
    // per-request member) and GCS object retention is a BUCKET policy (no `UploadObjectOptions` member), so those two legs
    // apply nothing and the column is honored at the host-dialed client — exactly the `ObjectEncryption` Azure/GCS split.
    // `Until` is the one deadline derivation the catalog column and both Apply arms read. Two providers carry it on the
    // REQUEST and each gets one `Apply*` arm over the union (the only per-provider variance; the request types are
    // categorically distinct so a single signature is unrepresentable), mirroring `ObjectEncryption` — a decorative lock
    // column promised only in prose is the deleted form.
    public Option<Instant> Until(Instant now) => Map(
        off:        static (_, _) => Option<Instant>.None,
        governance: static (at, c) => Some(at + c.Retain),
        compliance: static (at, c) => Some(at + c.Retain),
        legalHold:  static (_, _) => Some(Instant.MaxValue),
        state: now);

    public InitiateMultipartUploadRequest ApplyS3(InitiateMultipartUploadRequest request, Instant now) => Switch(
        off:        static (s, _) => s.Request,
        governance: static (s, c) => (s.Request.ObjectLockMode = ObjectLockMode.Governance,
                                      s.Request.ObjectLockRetainUntilDate = (s.Now + c.Retain).ToDateTimeUtc(), s.Request).Item3,
        compliance: static (s, c) => (s.Request.ObjectLockMode = ObjectLockMode.Compliance,
                                      s.Request.ObjectLockRetainUntilDate = (s.Now + c.Retain).ToDateTimeUtc(), s.Request).Item3,
        legalHold:  static (s, _) => (s.Request.ObjectLockLegalHoldStatus = ObjectLockLegalHoldStatus.On, s.Request).Item2,
        state: (Request: request, Now: now));

    // Minio legal hold IS a put-time member — the inherited `ObjectWriteArgs<T>.WithLegalHold(bool?)` stamps
    // `x-amz-object-lock-legal-hold` on the write, so the `legalHold` arm applies it here; a no-op arm silently
    // dropping the column is exactly the gap this dispatch closes.
    public PutObjectArgs ApplyMinio(PutObjectArgs args, Instant now) => Switch(
        off:        static (s, _) => s.Args,
        governance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.GOVERNANCE)),
        compliance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.COMPLIANCE)),
        legalHold:  static (s, _) => s.Args.WithLegalHold(true),
        state: (Args: args, Now: now));
}

// A grant request names the operation the minter authorizes; the minted `ObjectGrant` is the executable
// wire shape — `FormPost` the presigned multipart/form-data POST (the upstream `S3UploadRequest { Url, Fields }`
// DTO decompile-verified on PollinationSDK 1.10.0), `SignedUrl` the bare GET/HEAD/DELETE URL. GENERIC
// parameterization: any presigned-grant cloud domain is one `GrantMinter` value — deployment DATA, zero
// central edits; Pollination is the SEED minter (`ArtifactsApi.CreateArtifactAsync → S3UploadRequest` mints
// writes, `DownloadArtifactAsync`/`JobsApi.DownloadJobArtifact` mint reads, `ListArtifactsAsync → FileMetaList`
// fills the roster on `FileMeta { Key, FileType, FileName, LastModified, Size }`).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GrantRequest {
    private GrantRequest() { }
    public sealed record Write(ContentAddress Key, long Length) : GrantRequest;
    public sealed record Read(ContentAddress Key) : GrantRequest;
    public sealed record Erase(ContentAddress Key) : GrantRequest;
}

[ComplexValueObject]
public sealed partial class GrantDemand {
    public GrantRequest Request { get; }
    public Duration Lifetime { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectGrant {
    private ObjectGrant() { }
    public sealed record FormPost(Uri Url, HashMap<string, string> Fields) : ObjectGrant;
    public sealed record SignedUrl(Uri Url) : ObjectGrant;
}

public readonly record struct BlobStat(ContentAddress Key, long Length);

[Union]
public abstract partial record ObjectClient {
    // The tenant partition this dialed client serves — minted at the composition root FROM the injected [A.1]
    // frame tenant, so the per-tenant object-name prefix and the `BlobCatalogRow.Tenant` column trace to ONE
    // source; `BlobGc.WriteBlobFirst` refuses a frame/client mismatch, never a silent name/row split-brain.
    public required UInt128 Tenant { get; init; }
    public sealed record S3(IAmazonS3 Client, string Bucket) : ObjectClient;
    public sealed record Azure(BlobContainerClient Container) : ObjectClient;
    // `Signer` is the credential-bound V4 `UrlSigner` the host dials beside the client — `StorageClient`
    // carries no signing surface, so issuer grants on the GCS row need the second host-dialed handle.
    public sealed record Gcs(StorageClient Client, string Bucket, UrlSigner Signer) : ObjectClient;
    public sealed record Minio(IMinioClient Client, string Bucket) : ObjectClient;
    // The credential-free fifth row: no endpoint, no credential — a `GrantMinter` mints an `ObjectGrant`
    // per operation, the `Roster` delegate fills `Head`/`Enumerate` (the upstream list surface), and the
    // host-dialed `HttpClient` is the connection input the grants execute over.
    public sealed record Presigned(Func<GrantRequest, IO<ObjectGrant>> Minter, Func<Option<ContentAddress>, IO<Seq<BlobStat>>> Roster, HttpClient Http) : ObjectClient;
}

// `Correlation` is THREADED from the write op's `ProjectionContext.Correlation` by the one receipt path
// (`MultipartTransfer.Upload`) — a read-leg `From` mints `Guid.Empty` and the write path stamps the frame's
// correlation, so the residence a write yields is traceable to its causing op, never a permanent `None`.
public readonly record struct BlobResidence(ContentAddress Key, long Length, StorageTier Tier, int Parts, int ResumedParts, Option<string> ConditionToken, Guid Correlation) {
    public static BlobResidence From(ContentAddress key, long length, ObjectStore store) => new(key, length, store.Tier, 0, 0, None, Guid.Empty);
}

public readonly record struct BlobTransferFact(string Kind, ContentAddress Key, long Bytes, int Part, Option<string> Session);

// The remote-store boundary fault band (540x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the
// Persistence siblings `Version/retention#RETENTION_CLASSES` `RetentionFault` (828x), `Version/recovery#RECOVERY_ROUTES`
// `RecoveryFault` (829x), and `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) realize — NOT `LanguageExt.Common.Expected`,
// whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to override) is the deleted form that
// resolves to the wrong base and falls outside the kernel federation a telemetry reader bands by code. Band membership
// derives `Code => FaultBand.RemoteStore + n` through the registry pointer (a bare 540x literal beside the registry
// row is the decoupled form the siblings already reject), `Message`/`Category` projecting through the generated `Switch`, so a typed case
// lifts BARE onto `Fin<T>`/`IO<T>` with no `.ToError()` hop and a recovery reads `error.IsType<RemoteStoreFault.Conflict>()`
// / `error.HasCode(5402)` / `error.Category()`, never a message substring. No `[GenerateUnionOps]` — the kernel
// union-ops generator is strictly opt-in, so the band carries no generated per-case `SelfOp`. `IsTransient` stays an
// `abstract` discriminant with one override per case (orthogonal to the base-ctor change) so `Transport.IsTransient`
// remains the sole `Schedule`-retry gate. `Create` is the IValidationError admission the generated converter bridge
// calls on a deserialization reject.
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
    // The presigned row's expiry-aware minter emits this case only from its signed expiry evidence. A bare
    // provider `403` remains `Denied` because status cannot distinguish expiry from policy or signature refusal.
    // No observed-instant field — the receipt envelope stamps the HLC and a leg-local wall-clock read is the
    // named inversion.
    public sealed record GrantExpired(ContentAddress Key) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record InvalidRange(ContentAddress Key, long Start, long End, long Length) : RemoteStoreFault { public override bool IsTransient => false; }

    public override int Code => FaultBand.RemoteStore + Switch(
        text:           static _ => 0,
        notFound:       static _ => 1,
        conflict:       static _ => 2,
        aborted:        static _ => 3,
        transport:      static _ => 4,
        integrityBreach: static _ => 5,
        locked:         static _ => 6,
        denied:         static _ => 7,
        oversize:       static _ => 8,
        grantExpired:   static _ => 9,
        invalidRange:   static _ => 10);

    public override string Message => Switch(
        text:           static c => c.Detail,
        notFound:       static c => $"blob {c.Key.Value:x32} absent",
        conflict:       static c => $"blob {c.Key.Value:x32} {c.Condition}",
        aborted:        static c => $"blob {c.Key.Value:x32} aborted@{c.Parts}: {c.Reason}",
        transport:      static c => $"{c.Provider} {c.Status}:{c.Code}",
        integrityBreach: static c => $"blob {c.Key.Value:x32} {c.Provider} checksum mismatch",
        locked:         static c => $"blob {c.Key.Value:x32} WORM {c.Mode}",
        denied:         static c => $"blob {c.Key.Value:x32} {c.Provider} denied: {c.Code}",
        oversize:       static c => $"blob {c.Key.Value:x32} {c.Provider} oversize: {c.Code}",
        grantExpired:   static c => $"blob {c.Key.Value:x32} grant expired",
        invalidRange:   static c => $"blob {c.Key.Value:x32} range {c.Start}-{c.End}/{c.Length}");

    public override string Category => Switch(
        text:           static _ => "Text",
        notFound:       static _ => "NotFound",
        conflict:       static _ => "Conflict",
        aborted:        static _ => "Aborted",
        transport:      static _ => "Transport",
        integrityBreach: static _ => "Integrity",
        locked:         static _ => "Locked",
        denied:         static _ => "Denied",
        oversize:       static _ => "Oversize",
        grantExpired:   static _ => "GrantExpired",
        invalidRange:   static _ => "InvalidRange");

    public static RemoteStoreFault Create(string message) => new Text(message);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectStore {
    // E9 CHECKSUM HONESTY: only `S3Leg.Seal` supplies the XxHash128 digest server-side (`Integrity.Wire` on
    // `ChecksumXXHASH128`), so ONLY the S3 row declares `XxHash128`; Azure reads its SDK-native `Crc64`, GCS its
    // SDK-native `Crc32c` (the GCS whole-object checksum is CRC32C — the SDK exposes no CRC64), and Minio `None`
    // (the SDK's own transport check) — a row declaring a stance no leg supplies is the decorative form this
    // table deletes. The Presigned row: upstream `FileMeta` carries NO
    // checksum/etag → `ObjectChecksum.None`; no multipart/resume upstream → the unreachable part floor makes
    // `Parts` yield ONE window (single-shot by construction) and `conditionalWrite: false` (no precondition
    // member on a form POST — the content-address invariant is the write-once law, the Minio precedent).
    public static readonly ObjectStore S3 = new("s3", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore AzureBlob = new("azure-blob", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.Crc64, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore Gcs = new("gcs", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.Crc32c, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore Minio = new("minio", 8L * 1024 * 1024, ChunkPolicy.Artifact, true, ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);
    public static readonly ObjectStore Presigned = new("presigned", long.MaxValue, ChunkPolicy.Artifact, false, ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance);

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
    // same-key re-PUT a benign `412`-noop where the bytes already reside. `now` is the ONE frame instant `Upload`
    // sampled — every WORM arm and the catalog `WormUntil` derive from it, never an ambient clock read.
    public IO<BlobResidence> Put(ObjectClient client, BlobResidence residence, ChunkManifest manifest, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now, Option<StorageTier> at) =>
        (ObjectIo.For(client).Multipart(this, at.IfNone(Tier), residence, manifest, source, sink, now)
            | @catch<IO, BlobResidence>(static e => e is RemoteStoreFault.Conflict, _ => Head(client, residence.Key)
                .Bind(present => present.Match(
                    Some: existing => sink(new BlobTransferFact("conflict-noop", residence.Key, 0L, 0, None)).Map(_ => existing with { Parts = 0 }),
                    None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(residence.Key)))))).As();

    public IO<Stream> Fetch(ObjectClient client, ContentAddress key, Option<(long Start, long End)> range) => ObjectIo.For(client).Fetch(key, range);
    public IO<Option<BlobResidence>> Head(ObjectClient client, ContentAddress key) => ObjectIo.For(client).Head(this, key);
    public IO<Unit> Delete(ObjectClient client, ContentAddress key) => ObjectIo.For(client).Erase(key);
    public IO<Seq<ContentAddress>> List(ObjectClient client) => ObjectIo.For(client).Enumerate();
    public IO<Unit> Abandon(ObjectClient client, ContentAddress key, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        ObjectIo.For(client).Abandon(key, session, sink);

    // The ISSUER inverse of the presigned CONSUMER row: mint a time-boxed grant against THIS store's own
    // residence so a credential-free reader (the AppUi `SnapshotAccelerator` viewer, a field tablet) streams
    // bytes directly from the provider — the caller gates the demand through `Element/authority#AUTHORITY`
    // `Admit` BEFORE minting, the TTL boxes the exposure, and the expiry anchors on the injected frame instant
    // ([A.1] — never an ambient clock). One entry over every row via the leg's `Issue`.
    public IO<ObjectGrant> Grant(ObjectClient client, GrantDemand demand, ProjectionContext frame) => ObjectIo.For(client).Issue(demand, frame.Now());

    // The Persistence-local placement-delegate bundle the app composes the seam `Graph/element#NODE_MODEL`
    // `GeometrySource` resolver over: `Get(key, range)` is the range-capable one-hop fetch a `Rasm.Compute` runner
    // pulls an analytical `Axis`/`FootPrint` slice through (a mesh LOD or a BREP byte-window, never the whole blob),
    // `Put` content-addresses and write-once-seals THROUGH `MultipartTransfer.Upload` — the ONE composed receipt
    // path, so every placement write yields a `BlobTransferReceipt` on the fact stream carrying the frame's
    // correlation (a `store.Put` with a no-op sink beside the receipt engine was the orphaned-surface V10 defect
    // this routing deletes) — and `Stat`/`Delete`/`List` close the lifecycle. ONE record over the five legs —
    // a `GeometrySource` over a phantom node field, or a parallel `GetRange` sibling, is the deleted form.
    public BlobRemote Placement(ObjectClient client, Func<ContentAddress, IO<Option<WrappedKey>>> envelope, ProjectionContext frame, Func<BlobTransferFact, IO<Unit>> sink) =>
        new(
            Put: (key, length, stream, session) => ObjectIo.Drain(
                stream,
                source => MultipartTransfer.Upload(this, client, BlobResidence.From(key, length, this) with { ConditionToken = session }, ContentChunker.Chunk(Chunking, source), source, sink, frame, None).Map(static receipt => receipt.Key)),
            Get: (key, range) => Encryption.Read(key, range, envelope, window => Fetch(client, key, window)),
            Stat: key => Head(client, key),
            Delete: key => Delete(client, key),
            List: () => List(client),
            Abandon: (key, session) => Abandon(client, key, session, sink),
            Issue: demand => Grant(client, demand, frame));
}

// The placement-delegate bundle the app wires the seam `Graph/element#NODE_MODEL` `GeometrySource` decoder over
// (`Get(repHash.Axis.Value, None)` -> decode -> `AxisCurve`); the range modality rides `Get`, never a `GetRange`
// sibling, so a partial analytical fetch and a whole-blob fetch are one entry discriminating on the `Option`
// range; `Issue` mints the credential-free viewer grant (Authority-gated at the caller, TTL-boxed).
public readonly record struct BlobRemote(
    Func<ContentAddress, long, Stream, Option<string>, IO<ContentAddress>> Put,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Get,
    Func<ContentAddress, IO<Option<BlobResidence>>> Stat,
    Func<ContentAddress, IO<Unit>> Delete,
    Func<IO<Seq<ContentAddress>>> List,
    Func<ContentAddress, string, IO<Unit>> Abandon,
    Func<GrantDemand, IO<ObjectGrant>> Issue);
```

| [INDEX] | [POLICY]         | [VALUE]                                     | [BINDING]                                                           |
| :-----: | :--------------- | :------------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | content-key name | `ContentAddress.Of` over kernel `XxHash128` | wraps the seam raw keys; never a second identity                    |
|  [02]   | per-leg dispatch | `ObjectClient.Map`                          | union case IS the dispatch; no mismatch guard                       |
|  [03]   | write-once seal  | provider conditional-write `412`-noop       | no read-before-write; the seal is the concurrency primitive         |
|  [04]   | integrity        | `ChecksumAlgorithm.XXHASH128` + `Wire`      | the content key IS the whole-object checksum; never re-hashed       |
|  [05]   | WORM/object-lock | `ObjectLock` SET on the write               | `Governance`/`Compliance` immutable; `LegalHold` indefinite         |
|  [06]   | fault rail       | one `RemoteStoreFault.Lift` per edge        | `Transport.IsTransient` the sole retry gate                         |
|  [07]   | checksum honesty | per-row SDK-native stance                   | S3 `XxHash128`; Azure `Crc64`; GCS `Crc32c`; Minio/Presigned `None` |
|  [08]   | presigned grants | `GrantMinter` → `ObjectGrant` per op        | minter-attested `GrantExpired`; bare `403` is `Denied`              |
|  [09]   | receipt path     | every write via `MultipartTransfer.Upload`  | `BlobTransferReceipt` carries the frame correlation                 |
|  [10]   | issuer grants    | `Grant` via the leg `Issue` per row         | TTL-boxed, `Admit`-gated, frame-instant expiry; viewer streams direct |
|  [11]   | client seal      | `ClientSealed` + `SealSource`/`OpenSource`  | AES-GCM under an envelope DEK; `WrappedKey` on the catalog row      |
|  [12]   | one WORM clock   | `Upload` samples `frame.Now()` once         | provider retention date and catalog `WormUntil` derive from it      |

## [03]-[MULTIPART_TRANSFER]

- Owner: `ObjectIo` the one generic transfer engine — a per-provider `ObjectLeg` delegate row (initiate, stage, seal, abort, list-committed plus fetch/head/erase/enumerate/issue) the five providers each fill once, over which a single packing fold packs the manifest's content-defined chunks into provider parts and seals; `ObjectLeg` the closed ten-delegate carrier `ObjectClient.Map` resolves; `MultipartTransfer` the receipt-emitting `Upload` (the composed receipt path every write op routes through) plus the `Parts` packer; `TransferPart`/`CommittedPart`/`PartCursor` the part-packing shapes; `BlobName` the Persistence-local content-key-to-object-name projection; `BlobTransferReceipt` the per-object evidence carrying the frame correlation.
- Entry: `Upload` is the receipt-emitting write every op composes — the frame supplies mark/elapsed/now and stamps `Correlation` onto residence and receipt, `at` the cold-tier override the `#BLOB_GC` `Demote` re-PUT threads; `Multipart` runs the one bracket-scoped packing fold over the resolved leg at the effective tier; `Drain` stages a fetch stream into a pooled `ArrayPoolBufferWriter<byte>`; `Parts` packs the chunks into provider-floor-clearing windows.
- Auto: `Parts` accumulates content-defined chunks into part windows each closing once it clears the `PartSize` floor, so a part spans whole `#CONTENT_CHUNKING` chunks (never a sub-chunk slice tearing a chunk across a boundary) at the smallest legal part count; `Multipart` reads the prior committed set through `leg.ListCommitted` (S3 `ListParts`, Azure uncommitted block ids) so an interrupted transfer SKIPS windows already committed in the same session — orthogonal to whole-manifest dedup: one resumes a torn upload, the other skips a resident object — then `TraverseM`-folds the residual windows, counting resumed-versus-fresh into the residence; a fault or cancel folds to `RemoteStoreFault.Aborted` and LEAVES the staged parts in place — the durable session token rides `PendingWrite.Session`, so a re-drive resumes the committed windows instead of restarting, and `Abandon` is the one explicit reap (an auto-abort release would delete the very parts resume exists to keep); `Drain` rents a pooled `ArrayPoolBufferWriter<byte>` so the fetch-to-bytes hop never allocates a throwaway array.
- Receipt: `BlobTransferFact` rides `store.blob.part` per uploaded part, `store.blob.resume` per skipped-committed window, `store.blob.abort` per aborted ceremony; `BlobResidence` carries the realized part/resumed-part/skipped-chunk counts the `Upload` receipt reads.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Minio (`Minio.Exceptions` the fourth provider's lifted fault family), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox (`HttpClient`/`MultipartFormDataContent`/`ReadOnlyMemoryContent` for the presigned leg's granted HTTP; the seed minter's SDK lives in the app-root closure).
- Growth: one part-floor value per provider row, or one `ChunkPolicy` row for a tighter window; a sixth provider fills one `ObjectLeg` row in `For` and contributes its exception family to the `Lift` fold (the presigned fifth exercised it: one leg, one status-map `Granted`, one in-band fault); a second chunker, a re-declared frame width, a per-provider multipart or read/head/delete/list body, a second HTTP uploader, or a per-provider abort catch is the deleted form because the content-defined window IS the `Element/codec#CONTENT_CHUNKING` fold, the ten-delegate leg row IS the per-provider variance, and the one `Aborted` fold owns every interruption.
- Boundary: the content-defined chunk boundary, the per-chunk `XxHash128` key, and the whole-blob `XxHash128` identity are owned at `Element/codec#CONTENT_CHUNKING` and consumed here as the `ChunkManifest`, so a re-declared frame width, a second chunker, or a second hash is the deleted form and the server-side checksum is that same digest projected as the provider header. Provider placement deduplicates only at the whole-object seal because no row can synthesize one object from another object's resident chunks; the part floor clears the S3 5 MB minimum as a row value, never a free literal; a torn upload leaves resumable staged parts under its durable session — `Abandon` is the one explicit reap and the provider's incomplete-upload lifecycle rule the backstop; `ConditionalWrite` gates the seal at `complete`, so a concurrent same-key writer resolves to `RemoteStoreFault.Conflict`, the benign no-op write-once placement treats as success.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TransferPart(int Number, long Offset, int Length, int Chunks);
public readonly record struct CommittedPart(int Number, string ETag);
// `WormUntil` carries the ONE lock deadline the upload's single sampled instant derived — the catalog column
// commits THIS value, so provider window and catalog window can never diverge (the two-clock split-brain form).
public readonly record struct BlobTransferReceipt(string Provider, ContentAddress Key, long Bytes, int Parts, int ResumedParts, bool Aborted, Option<Instant> WormUntil, Duration Elapsed, Instant At, Guid Correlation);

// The per-provider leg carrier: each provider fills these TEN delegates ONCE (the only per-provider variance), so the
// transfer ceremony AND the read/head/delete/list bodies are written ONCE over the leg row, never four parallel
// `S3Multipart`/`AzureBlocks`/... bodies. `Initiate` yields the provider upload token (S3 `UploadId`, Azure a block-id
// stem, whole-object providers the object name), seals the whole-object checksum + the effective `StorageTier` the
// caller threads (the row `Tier` on a fresh write, the colder rung on a `#BLOB_GC` `Demote` re-PUT), and takes the ONE
// frame instant the WORM arms stamp; `Stage` uploads one packed window (a no-op for whole-object providers
// whose SDK auto-chunks); `Seal` FINALIZES the object carrying the `ObjectStore` row and the same instant (so a
// single-PUT provider stamps the row's SSE + WORM + write-once stance at the one finalize point) — for staged providers
// it commits the part list under the
// write-once `IfNoneMatch:*` precondition (S3) / `ETag.All` (Azure), for whole-object providers it runs the single PUT of
// the WHOLE `source`: GCS under the genuine `IfGenerationMatch=0` create-if-absent precondition, Minio as an idempotent
// content-addressed create under `PutObjectArgs.WithNotMatchETag("*")`; a racing writer therefore reaches the same
// typed `412` conflict as every cloud row — so the
// one engine drives both transfer models through ONE seal without a mode knob; `Abort` is the bracket release;
// `Committed` lists the prior session's windows for resume;
// `Fetch`/`Head`/`Erase`/`Enumerate` are the read/lifecycle legs the placement dispatches; `Issue` is the
// issuer-side grant mint (each credentialed SDK's presign entry; the presigned row forwards to its own minter).
public readonly record struct ObjectLeg(
    Func<ObjectStore, StorageTier, ContentAddress, Instant, Option<string>, IO<string>> Initiate,
    Func<string, ContentAddress, TransferPart, ReadOnlyMemory<byte>, IO<CommittedPart>> Stage,
    Func<ObjectStore, string, ContentAddress, Seq<CommittedPart>, ReadOnlyMemory<byte>, Instant, IO<Unit>> Seal,
    Func<string, ContentAddress, IO<Unit>> Abort,
    Func<string, ContentAddress, IO<Seq<CommittedPart>>> Committed,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Fetch,
    Func<ObjectStore, ContentAddress, IO<Option<BlobResidence>>> Head,
    Func<ContentAddress, IO<Unit>> Erase,
    Func<IO<Seq<ContentAddress>>> Enumerate,
    Func<GrantDemand, Instant, IO<ObjectGrant>> Issue);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The Persistence-local object-name projection of the seam content key — the `x32` lowercase hex of the `UInt128` under
// the per-tenant prefix the `#BLOB_GC` envelope partitions by, so two tenants never collide on one content key and the
// object store mints NO second identity; the tenant is the INJECTED `ObjectClient.Tenant` value (the [A.1] frame law —
// an ambient `TenantContext.Current` read here is the named inversion, the deleted form), so name prefix and catalog
// row trace to one source; `OfName` is the inverse a `List` row reads back. NOT a seam member (the seam owns the
// `ContentAddress` value, this page owns its provider-name spelling), so a free-string blob name is the deleted form.
file static class BlobName {
    public static string Name(this ContentAddress key, UInt128 tenant) => $"{tenant:x32}/{key.Value:x32}";
    public static ContentAddress OfName(string name) => ContentAddress.Of(UInt128.Parse(name.AsSpan(name.LastIndexOf('/') + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
}

public static class MultipartTransfer {
    // THE composed receipt path: every write op routes here (`Placement.Put`, `BlobGc.WriteBlobFirst`, the
    // `BlobGc.Demote` tier-transition re-PUT), so the receipt is never an orphaned sibling of a bare `store.Put`;
    // the [A.1] frame supplies mark/elapsed/now AND the correlation the residence + receipt both carry —
    // `BlobResidence.Correlation` threads from the causing op, never a permanent none. `at` is the same egress
    // tier override `Put` carries, so the cold-tier demotion rides the one receipt path too.
    public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobResidence residence, ChunkManifest manifest, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame, Option<StorageTier> at) =>
        from mark in IO.lift(frame.Mark)
        from now in IO.lift(frame.Now)
        from sealed_ in provider.Put(client, residence with { Correlation = frame.Correlation }, manifest, source, sink, now, at)
        select new BlobTransferReceipt(provider.Key, sealed_.Key, sealed_.Length, sealed_.Parts, sealed_.ResumedParts, Aborted: false, provider.Lock.Until(now), frame.Elapsed(mark), now, frame.Correlation);

    // Pack whole content-defined chunks into part windows, each closing once it clears the floor — a part spans whole
    // chunks (never a sub-chunk slice), the smallest legal part count resulting, and the open tail seals last.
    public static Seq<TransferPart> Parts(ChunkManifest manifest, long partFloor) {
        (Seq<TransferPart> Done, PartCursor Open) packed = manifest.Chunks.Fold(
            (Done: Seq<TransferPart>(), Open: PartCursor.Empty),
            static (acc, chunk) => acc.Open.Grow(chunk).Pack(partFloor, acc.Done));
        return packed.Open.Chunks > 0 ? packed.Done.Add(packed.Open.Seal(packed.Done.Count + 1)) : packed.Done;
    }
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
        s3: static r => S3Leg(r), azure: static r => AzureLeg(r), gcs: static r => GcsLeg(r), minio: static r => MinioLeg(r), presigned: static r => PresignedLeg(r));

    // The one packing fold. `Initiate` acquires (or resumes) the upload token; `Committed` lists the prior session's
    // committed windows (empty on a fresh upload), only unresumed windows upload through `leg.Stage`, and `Seal` commits
    // under the write-once precondition. Staged parts SURVIVE a fault/cancel by design — the session token rides
    // `PendingWrite.Session` durably, so a re-drive resumes the committed windows instead of restarting; `leg.Abort`
    // runs only through the explicit `Abandon` verb (an auto-abort release would delete the very parts resume keeps).
    // Every non-rail error folds to `RemoteStoreFault.Aborted` carrying the window count.
    public static IO<BlobResidence> Multipart(this ObjectLeg leg, ObjectStore provider, StorageTier tier, BlobResidence residence, ChunkManifest manifest, ReadOnlyMemory<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now) {
        Seq<TransferPart> windows = MultipartTransfer.Parts(manifest, provider.PartSize);
        return (leg.Initiate(provider, tier, residence.Key, now, residence.ConditionToken).Bind(token =>
                from _session in sink(new BlobTransferFact("session", residence.Key, 0L, 0, Some(token)))
                from prior in leg.Committed(token, residence.Key)
                let resumed = prior.Map(static p => p.Number).ToFrozenSet()
                from staged in windows.Filter(w => !resumed.Contains(w.Number)).TraverseM(w =>
                    from committed in leg.Stage(token, residence.Key, w, source.Slice((int)w.Offset, w.Length))
                    from _ in sink(new BlobTransferFact("part", residence.Key, w.Length, w.Number, None))
                    select committed).As()
                from _ in prior.TraverseM(p => sink(new BlobTransferFact("resume", residence.Key, 0L, p.Number, None))).As()
                from _ in leg.Seal(provider, token, residence.Key, toSeq((prior + staged).OrderBy(static p => p.Number)), source, now)
                select residence with { Length = source.Length, Parts = windows.Count, ResumedParts = prior.Count, ConditionToken = None })
            | @catch<IO, BlobResidence>(static _ => true, error => IO.fail<BlobResidence>(error is RemoteStoreFault ? error : new RemoteStoreFault.Aborted(residence.Key, windows.Count, error.Message)))).As();
    }

    public static IO<Unit> Abandon(this ObjectLeg leg, ContentAddress key, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        from _ in leg.Abort(session, key)
        from _fact in sink(new BlobTransferFact("abort", key, 0L, 0, Some(session)))
        select unit;

    // Drain a fetch stream into pooled storage — `ArrayPoolBufferWriter<byte>` rents from the shared pool, the copy fills
    // it, and `WrittenMemory` reads back with zero throwaway array; the writer disposes back to the pool on bracket exit.
    public static IO<T> Drain<T>(Stream source, Func<ReadOnlyMemory<byte>, IO<T>> use) =>
        IO.lift(static () => new ArrayPoolBufferWriter<byte>()).Bracket(
            Use: writer => IO.liftAsync(async () => {
                await source.CopyToAsync(writer.AsStream()).ConfigureAwait(false);
                return writer.WrittenMemory;
            }).Bind(use),
            Fin: writer => IO.lift(() => {
                writer.Dispose();
                source.Dispose();
                return unit;
            }));

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
            int status => new RemoteStoreFault.Transport(provider, status, gcs.Error?.Message ?? "gcs"),
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
        Initiate: (store, tier, key, now, resume) => resume.Match(
            Some: IO.pure,
            None: () => Bound("s3", key, () => r.Client.InitiateMultipartUploadAsync(store.Lock.ApplyS3(store.Encryption.ApplyS3(new InitiateMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), StorageClass = tier.S3Class, ChecksumAlgorithm = store.Integrity.S3Algorithm, ChecksumType = ChecksumType.FULL_OBJECT }), now))).Map(static x => x.UploadId)),
        Stage: (token, key, part, bytes) => Bound("s3", key, () => r.Client.UploadPartAsync(new UploadPartRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), UploadId = token, PartNumber = part.Number, PartSize = part.Length, InputStream = bytes.AsStream() })).Map(x => new CommittedPart(part.Number, x.ETag)),
        Seal: (store, token, key, parts, _, _) => Bound("s3", key, () => r.Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), UploadId = token, IfNoneMatch = "*", ChecksumXXHASH128 = store.Integrity.Wire(key).IfNoneUnsafe(static () => null), PartETags = parts.Map(static p => new PartETag(p.Number, p.ETag)).ToList() })).Map(static _ => unit),
        Abort: (token, key) => string.IsNullOrEmpty(token) ? IO.pure(unit) : Bound("s3", key, () => r.Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), UploadId = token })).Map(static _ => unit),
        Committed: (token, key) => Bound("s3", key, () => r.Client.ListPartsAsync(new ListPartsRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), UploadId = token })).Map(static x => toSeq(x.Parts).Map(static p => new CommittedPart(p.PartNumber, p.ETag))),
        Fetch: (key, range) => Bound("s3", key, () => r.Client.GetObjectAsync(new GetObjectRequest { BucketName = r.Bucket, Key = key.Name(r.Tenant), ByteRange = range.Match(Some: static w => new ByteRange(w.Start, w.End), None: static () => null) })).Map(static x => x.ResponseStream),
        Head: (store, key) => Bound("s3", key, () => r.Client.GetObjectMetadataAsync(r.Bucket, key.Name(r.Tenant))).Map(x => Optional(BlobResidence.From(key, x.ContentLength, store))),
        Erase: key => Bound("s3", key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name(r.Tenant))).Map(static _ => unit),
        Enumerate: () => Bound("s3", default, () => r.Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = r.Bucket })).Map(static x => toSeq(x.S3Objects).Map(static o => BlobName.OfName(o.Key))),
        Issue: (demand, now) => Bound("s3", KeyOf(demand.Request), () => Task.FromResult<ObjectGrant>(new ObjectGrant.SignedUrl(new Uri(r.Client.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = r.Bucket, Key = KeyOf(demand.Request).Name(r.Tenant), Verb = VerbOf(demand.Request), Expires = (now + demand.Lifetime).ToDateTimeUtc() }))))));

    // Azure: staged-block over `BlockBlobClient` (`api-objectstore` AZURE_BLOCKS). The block id is the part number; `Seal`
    // commits with `IfNoneMatch = ETag.All`; range read is `BlobDownloadOptions.Range = HttpRange`.
    static ObjectLeg AzureLeg(ObjectClient.Azure r) => new(
        Initiate: (_, _, key, _, _) => IO.pure(key.Name(r.Tenant)),
        Stage: (token, _, part, bytes) => Bound("azure", default, async () => {
            BlockBlobClient block = r.Container.GetBlockBlobClient(token);
            string id = Convert.ToBase64String(BitConverter.GetBytes(part.Number));
            await block.StageBlockAsync(id, bytes.AsStream(), new BlockBlobStageBlockOptions {
                TransferValidation = new UploadTransferValidationOptions {
                    ChecksumAlgorithm = StorageChecksumAlgorithm.StorageCrc64,
                    PrecalculatedChecksum = ObjectChecksum.Azure(bytes.Span),
                },
            }).ConfigureAwait(false);
            return new CommittedPart(part.Number, id);
        }),
        Seal: (_, token, key, parts, _, _) => Bound("azure", key, () => r.Container.GetBlockBlobClient(token).CommitBlockListAsync(parts.Map(static p => p.ETag).ToList(), new CommitBlockListOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All } })).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: (token, key) => Bound("azure", key, () => r.Container.GetBlockBlobClient(token).GetBlockListAsync(BlockListTypes.Uncommitted)).Map(static x => toSeq(x.Value.UncommittedBlocks).Map(static b => new CommittedPart(BitConverter.ToInt32(Convert.FromBase64String(b.Name)), b.Name))),
        Fetch: (key, range) => Bound("azure", key, () => r.Container.GetBlobClient(key.Name(r.Tenant)).DownloadStreamingAsync(new BlobDownloadOptions { Range = range.Map(static w => new HttpRange(w.Start, w.End - w.Start + 1)).IfNone(default(HttpRange)) })).Map(static x => x.Value.Content),
        Head: (store, key) => Bound("azure", key, () => r.Container.GetBlobClient(key.Name(r.Tenant)).GetPropertiesAsync()).Map(x => Optional(BlobResidence.From(key, x.Value.ContentLength, store))),
        Erase: key => Bound("azure", key, () => r.Container.GetBlobClient(key.Name(r.Tenant)).DeleteIfExistsAsync()).Map(static _ => unit),
        Enumerate: () => Bound("azure", default, async () => {
            List<ContentAddress> keys = [];
            await foreach (BlobItem blob in r.Container.GetBlobsAsync().ConfigureAwait(false))
                keys.Add(BlobName.OfName(blob.Name));
            return toSeq(keys);
        }),
        // SAS issuance needs the container dialed with a shared-key credential (an AAD-dialed client cannot
        // `GenerateSasUri`) — a deployment fact of the host-dialed `BlobContainerClient`, not a leg branch.
        Issue: (demand, now) => Bound("azure", KeyOf(demand.Request), () => Task.FromResult<ObjectGrant>(new ObjectGrant.SignedUrl(r.Container.GetBlobClient(KeyOf(demand.Request).Name(r.Tenant)).GenerateSasUri(demand.Request is GrantRequest.Write ? BlobSasPermissions.Write | BlobSasPermissions.Create : demand.Request is GrantRequest.Erase ? BlobSasPermissions.Delete : BlobSasPermissions.Read, (now + demand.Lifetime).ToDateTimeOffset())))));

    // GCS: single resumable session over `StorageClient` (`api-objectstore` GCS_RESUMABLE) — the provider resumes the
    // session server-side, so the multipart legs collapse to a single chunked `UploadObjectAsync` with the genuine
    // write-once `IfGenerationMatch=0` create-if-absent precondition and the row's SSE-KMS key id through `ApplyGcs`
    // (SSE-C rides the dialed `StorageClient`'s `EncryptionKey`).
    static ObjectLeg GcsLeg(ObjectClient.Gcs r) => new(
        Initiate: (_, _, key, _, _) => IO.pure(key.Name(r.Tenant)),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, token, key, _, source, _) => Bound("gcs", key, () => r.Client.UploadObjectAsync(r.Bucket, token, "application/octet-stream", source.AsStream(), store.Encryption.ApplyGcs(new UploadObjectOptions { IfGenerationMatch = 0, ChunkSize = 8 * 1024 * 1024 }))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (key, range) => Bound("gcs", key, async () => {
            MemoryStream sink = new();
            await r.Client.DownloadObjectAsync(r.Bucket, key.Name(r.Tenant), sink, new DownloadObjectOptions { Range = range.Match(Some: static w => new RangeHeaderValue(w.Start, w.End), None: static () => null) }).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound("gcs", key, () => r.Client.GetObjectAsync(r.Bucket, key.Name(r.Tenant))).Map(x => Optional(BlobResidence.From(key, (long)(x.Size ?? 0), store))),
        Erase: key => Bound("gcs", key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name(r.Tenant))).Map(static _ => unit),
        Enumerate: () => IO.liftAsync(() => Task.FromResult(toSeq(r.Client.ListObjects(r.Bucket).Select(static o => BlobName.OfName(o.Name))))),
        // GCS V4 signing rides the credential-bound `UrlSigner` the host dials onto the row beside the client
        // (`StorageClient` itself carries no signer) — the TTL is a from-now duration by V4 construction.
        Issue: (demand, _) => Bound("gcs", KeyOf(demand.Request), () => r.Signer.SignAsync(r.Bucket, KeyOf(demand.Request).Name(r.Tenant), demand.Lifetime.ToTimeSpan(), demand.Request is GrantRequest.Write ? HttpMethod.Put : demand.Request is GrantRequest.Erase ? HttpMethod.Delete : HttpMethod.Get)).Map(static url => (ObjectGrant)new ObjectGrant.SignedUrl(new Uri(url))));

    // Minio: S3-compatible, multipart auto-managed inside `PutObjectAsync` (`api-minio`) — one `PutObjectArgs` write per
    // blob carrying the row's SSE through `ApplyMinio`; inherited `WithNotMatchETag("*")` supplies the same
    // create-if-absent gate every cloud row carries; range read is `GetObjectArgs.WithOffsetAndLength`.
    static ObjectLeg MinioLeg(ObjectClient.Minio r) => new(
        Initiate: (_, _, key, _, _) => IO.pure(key.Name(r.Tenant)),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, token, key, _, source, now) => Bound("minio", key, async () => {
            using Stream stream = source.AsStream();
            PutObjectArgs request = new PutObjectArgs()
                .WithBucket(r.Bucket)
                .WithObject(token)
                .WithStreamData(stream)
                .WithObjectSize(source.Length)
                .WithContentType("application/octet-stream")
                .WithNotMatchETag("*");
            await r.Client.PutObjectAsync(store.Lock.ApplyMinio(store.Encryption.ApplyMinio(request), now)).ConfigureAwait(false);
            return unit;
        }),
        Abort: (_, key) => Bound("minio", key, async () => {
            await foreach (Upload upload in r.Client.ListIncompleteUploadsEnumAsync(new ListIncompleteUploadsArgs().WithBucket(r.Bucket).WithPrefix(key.Name(r.Tenant))).ConfigureAwait(false))
                await r.Client.RemoveIncompleteUploadAsync(new RemoveIncompleteUploadArgs().WithBucket(r.Bucket).WithObject(upload.Key)).ConfigureAwait(false);
            return unit;
        }),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (key, range) => Bound("minio", key, async () => {
            MemoryStream sink = new();
            GetObjectArgs request = new GetObjectArgs().WithBucket(r.Bucket).WithObject(key.Name(r.Tenant)).WithCallbackStream(stream => stream.CopyTo(sink));
            await r.Client.GetObjectAsync(range.Match(Some: window => request.WithOffsetAndLength(window.Start, window.End - window.Start + 1), None: () => request)).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound("minio", key, () => r.Client.StatObjectAsync(new StatObjectArgs().WithBucket(r.Bucket).WithObject(key.Name(r.Tenant)))).Map(x => Optional(BlobResidence.From(key, x.Size, store))),
        Erase: key => Bound("minio", key, () => r.Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(r.Bucket).WithObject(key.Name(r.Tenant)))).Map(static _ => unit),
        Enumerate: () => Bound("minio", default, async () => {
            List<ContentAddress> keys = [];
            await foreach (Item item in r.Client.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(r.Bucket).WithRecursive(true)).ConfigureAwait(false))
                keys.Add(BlobName.OfName(item.Key));
            return toSeq(keys);
        }),
        // Minio presigns GET and PUT only (the catalog carries no presigned-DELETE member), so an Erase demand is
        // the typed Denied refusal — a GET grant answering a DELETE demand authorized the wrong verb, the deleted form.
        Issue: (demand, _) => demand.Request is GrantRequest.Erase
            ? IO.fail<ObjectGrant>(new RemoteStoreFault.Denied(KeyOf(demand.Request), "minio", "presigned-delete-unsupported"))
            : Bound("minio", KeyOf(demand.Request), async () => (ObjectGrant)new ObjectGrant.SignedUrl(new Uri(demand.Request is GrantRequest.Write
                ? await r.Client.PresignedPutObjectAsync(new PresignedPutObjectArgs().WithBucket(r.Bucket).WithObject(KeyOf(demand.Request).Name(r.Tenant)).WithExpiry((int)demand.Lifetime.TotalSeconds)).ConfigureAwait(false)
                : await r.Client.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(r.Bucket).WithObject(KeyOf(demand.Request).Name(r.Tenant)).WithExpiry((int)demand.Lifetime.TotalSeconds)).ConfigureAwait(false)))));

    // Presigned-grant: the leg holds NO credential — the client-side credential never exists, which is the
    // reach no credentialed row has. Every transfer op mints an `ObjectGrant` per operation through the case's
    // `GrantMinter` and EXECUTES it over the same engine: `FormPost` is ONE `multipart/form-data` POST carrying
    // every minted field plus the payload (the `S3UploadRequest { Url, Fields }` execution — `Helper.
    // UploadArtifactAsync` the decompile-verified upstream precedent), `SignedUrl` a bare GET/HEAD/DELETE.
    // Whole-object single-shot (Stage no-op, the unreachable part floor packs ONE window); `Head`/`Enumerate`
    // fill from the case's `Roster` delegate (upstream `ListArtifacts → FileMetaList` — no head verb exists on
    // a grant plane); only the expiry-aware minter emits `GrantExpired`, while a bare `403` is `Denied`.
    static ObjectLeg PresignedLeg(ObjectClient.Presigned r) => new(
        Initiate: (_, _, key, _, _) => IO.pure(key.Name(r.Tenant)),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (_, _, key, _, source, _) => r.Minter(new GrantRequest.Write(key, source.Length)).Bind(grant =>
            Bound("presigned", key, async () => {                                          // Exemption: the granted HTTP execution is the platform-forced statement seam
                using HttpResponseMessage response = grant switch {
                    ObjectGrant.FormPost post => await Posted(r.Http, post, key, r.Tenant, source).ConfigureAwait(false),
                    ObjectGrant.SignedUrl url => await r.Http.PutAsync(url.Url, new ReadOnlyMemoryContent(source)).ConfigureAwait(false),
                    _ => throw new InvalidOperationException(nameof(ObjectGrant)),
                };
                return response.IsSuccessStatusCode ? unit : throw Granted(key, response.StatusCode).ToException();
            })),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (key, range) => r.Minter(new GrantRequest.Read(key)).Bind(grant =>
            Bound("presigned", key, async () => {
                using HttpRequestMessage request = new(HttpMethod.Get, Url(grant));
                _ = range.Map(w => request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(w.Start, w.End));
                HttpResponseMessage response = await r.Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStreamAsync().ConfigureAwait(false) : throw Granted(key, response.StatusCode).ToException();
            })),
        Head: (store, key) => r.Roster(Some(key)).Map(rows =>
            rows.Find(s => s.Key == key).Map(s => BlobResidence.From(key, s.Length, store))),
        Erase: key => r.Minter(new GrantRequest.Erase(key)).Bind(grant =>
            Bound("presigned", key, async () => {
                using HttpResponseMessage response = await r.Http.DeleteAsync(Url(grant)).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? unit : throw Granted(key, response.StatusCode).ToException();
            })),
        Enumerate: () => r.Roster(None).Map(static rows => rows.Map(static s => s.Key)),
        // The presigned row is already a grant plane — issuance forwards to ITS minter (the domain cloud signs).
        Issue: (demand, _) => r.Minter(demand.Request));

    static ContentAddress KeyOf(GrantRequest request) => request.Switch(
        write: static w => w.Key, read: static g => g.Key, erase: static e => e.Key);

    static HttpVerb VerbOf(GrantRequest request) => request.Switch(
        write: static _ => HttpVerb.PUT, read: static _ => HttpVerb.GET, erase: static _ => HttpVerb.DELETE);

    static async Task<HttpResponseMessage> Posted(HttpClient http, ObjectGrant.FormPost post, ContentAddress key, UInt128 tenant, ReadOnlyMemory<byte> source) {
        using MultipartFormDataContent form = new();
        foreach ((string field, string value) in post.Fields)
            form.Add(new StringContent(value), field); // Exemption: minted fields precede the S3 form payload
        form.Add(new ReadOnlyMemoryContent(source), "file", key.Name(tenant));
        return await http.PostAsync(post.Url, form).ConfigureAwait(false);
    }

    static Uri Url(ObjectGrant grant) => grant switch {
        ObjectGrant.SignedUrl s => s.Url,
        ObjectGrant.FormPost p => p.Url,
        _ => throw new InvalidOperationException(nameof(ObjectGrant)),
    };

    static RemoteStoreFault Granted(ContentAddress key, HttpStatusCode status) => status switch {
        HttpStatusCode.Forbidden => new RemoteStoreFault.Denied(key, "presigned", "forbidden"),
        HttpStatusCode.NotFound => new RemoteStoreFault.NotFound(key),
        HttpStatusCode.PreconditionFailed => new RemoteStoreFault.Conflict(key, "if-none-match"),
        HttpStatusCode refused => new RemoteStoreFault.Transport("presigned", (int)refused, refused.ToString()),
    };
}
```

## [04]-[BLOB_GC]

- Owner: `BlobCatalogRow` the content-lineage retention row every blob carries (the same row the snapshot spine has, `H10`), carrying the `WormUntil` object-lock window beside the `Tenant`/`Bytes`/`Tier` columns and projecting to the `Version/retention#SWEEP_AND_GC` `RetentionFact` the ONE deletion executor consumes; `PendingWrite` the write-blob-first pending ledger; `BlobGc` the static surface owning ONLY the write-blob-first protocol, the in-flight-fence eligibility predicate it contributes to the retention sweep, and the WORM-aware `evict` arrow surfacing `RemoteStoreFault.Locked` — it does NOT own a second deletion executor.
- Entry: `WriteBlobFirst` opens the `PendingWrite` ledger row, write-once-seals the blob through `MultipartTransfer.Upload`, commits the catalog row, then clears the pending row; `InFlightFence(pending, now)` derives its age from `RetentionClass.Blob.Schedule.OrphanAge`, so no grace knob can diverge from the retention verdict; `Sweep` projects the catalog to `RetentionFact`s and routes the whole decision through `RetentionSweep.Run` and `RetentionSweep.Execute` with the `WormEvict` and `Demote` arrows.
- Auto: write-first carries `open → blob → catalog → close`; a crash before catalog commit leaves a pending-fenced orphan until `RetentionClass.Blob.Schedule.OrphanAge`, never a dangling event reference. `BlobGc.Sweep` projects each row to `RetentionFact` and delegates every verdict and mutation to the one retention executor over full-history reachability.
- Receipt: a blob write rides `store.blob.write` carrying the content key and bytes; the GC reclaim rides the `Version/retention#SWEEP_AND_GC` `SweepReceipt` on the retention sweep's own fact stream (the orphan count and reclaimed bytes are the retention executor's receipt, never a parallel `store.blob.gc` stream the blob lane re-mints).
- Packages: System.IO.Hashing, NodaTime (`Instant`/`Duration` the `WormUntil` window), LanguageExt.Core (`Seq`/`Choose`/`IO.fail`), System.Collections.Frozen (`FrozenDictionary` the WORM index), Thinktecture.Runtime.Extensions (`RetentionClass.Blob`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability` the `Version/retention#SWEEP_AND_GC` surface this owner composes), BCL inbox. (The WORM/object-lock SET is the `#OBJECT_STORE` `ObjectLock` write-leg's concern, never a KMS or SDK call here — the blob lane only READS the catalog `WormUntil` and mints `RemoteStoreFault.Locked`; SSE-KMS needs only the key-id STRING, the KMS signing SDKs being the `Version/provenance#ATTESTED_LEDGER` owner's, never composed at the blob lane.)
- Growth: a new catalog column is one field on `BlobCatalogRow` (as `WormUntil` is); a new WORM/object-lock stance is one `ObjectLock` case the `WriteBlobFirst` `Lock.Until` and the `WormEvict` arrow both read with zero new surface; zero new surface — a head-only blob GC, a `BlobGc`-local `List`-then-`Filter` sweep parallel to the retention executor, a same-PG-txn blob write, a two-ORM atomicity dance, a blob-lane-local retention executor that re-decides eviction beside `RetentionSweep`, or a free-string blob name is the deleted form because the GC routes through the ONE `RetentionSweep` over the full history (the WORM fence riding the injected `eligible` predicate and the typed `WormEvict` arrow, never a second sweeper), the blob is write-first content-addressed, and identity+event is the one Marten-session transaction.
- Boundary: the geometry blob carries the SAME content-lineage and retention-catalog row the snapshot spine has (`H10`) and registers in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — a blob-lane-local deletion executor is the deleted form, the blob lane contributing only its `RetentionFact` projection and its `InFlightFence` predicate to the `RetentionSweep` every lane routes through, and geometry GC over head alone is FORBIDDEN (the retention `Mark` folds every AS-OF cut so a blob a historical version references survives); the write-blob-first + `412`-noop protocol survives a crash (orphan blob, never dangling reference) and the `PendingWrite` ledger row OPENS before the blob `Put` and CLOSES after the catalog commit, so the in-flight fence distinguishes an in-flight write from an orphan even before the catalog row exists (the fail-safe never reaps an un-ageable present write); the ONE transaction owner for identity+event is the Marten `IDocumentSession` (`Element/graph#STORE_RAIL`), the blob being write-first and reference-after with no free two-ORM atomicity; the SSE key MATERIAL is a key-id STRING carried on the `ObjectEncryption` case (`ManagedKey` the SSE-KMS key id the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` or the host KMS minted out-of-band, `CustomerKey` the SSE-C key + MD5, `ProviderManaged` the account/bucket-default) and `ObjectEncryption.ApplyS3`/`ApplyGcs`/`ApplyMinio` stamp it on the wire at each provider's request (Azure at the dialed client) — a blob-lane-local KMS envelope is the deleted form (the DEK-wrapping envelope lifecycle and both cloud-KMS keyrings are the `Element/identity#AUTHORITY` owner's), key acquisition being a host connection input never a fence member; the WORM/object-lock retention-until is a write-policy stance carried on the `ObjectLock` case (`Off` the default, `Governance`/`Compliance` a `Retain` window) — `ObjectLock.ApplyS3`/`ApplyMinio` SET it on the wire from the ONE `Upload`-sampled instant and `WriteBlobFirst` records the SAME derivation (`receipt.WormUntil`) onto `BlobCatalogRow.WormUntil`, so the GC's `WormEvict` arrow refuses a key under an active window with `RemoteStoreFault.Locked` (the eligibility fence already holds it out of selection, the arrow the defense-in-depth second gate), making `Locked` a genuinely reachable domain fault rather than an opaque provider `403` the `Lift` would mis-fold to `Denied`; the per-tenant prefix (`BlobName` `{tenant:x32}/{key:x32}` over the injected `ObjectClient.Tenant`) partitions the blob namespace by tenant so a multi-tenant store isolates by construction, the `BlobCatalogRow.Tenant` column stamps the [A.1] `frame.Tenant` the `WriteBlobFirst` first-step guard proves equal to `client.Tenant` (one injected source — an ambient `TenantContext.Current` read is the named inversion, deleted), and the retention catalog `Seq<BlobCatalogRow>` the caller hands `BlobGc.Sweep` is filtered by the canonical x32-text tenant RLS partition at the catalog query so a cross-tenant reclaim is unrepresentable end to end — the sweep never sees another tenant's rows, and the per-tenant object-name prefix means even a mis-supplied catalog row resolves to a name under the wrong tenant's prefix that the provider does not hold.

```csharp signature
// The content-lineage retention row. `WormUntil` is the WORM/object-lock retention-until the `Store/blobstore#OBJECT_STORE`
// `ObjectLock` write stance SET on the blob (`ObjectLock.Until(now)`) — `None` for the default `ObjectLock.Off` rows, an
// `Instant` for a compliance/governance-class blob whose bytes are provider-immutable until the window lapses; it is the ONE
// column that makes `RemoteStoreFault.Locked` genuinely reachable, the sweep's `evict` arrow refusing a key still under it.
// `Dek` is the `ClientSealed` residence's persisted envelope (`Element/identity#KEY_ENVELOPE` `WrappedKey`) —
// `None` on every SSE/provider-managed row; the read path unwraps it through `ObjectEncryption.OpenSource`.
public sealed record BlobCatalogRow(ContentAddress Key, RetentionClass Class, long Bytes, StorageTier Tier, Option<ContentAddress> Lineage, UInt128 Tenant, DataClassification Classification, Option<Instant> WormUntil, Option<WrappedKey> Dek, Instant At);
public readonly record struct PendingWrite(ContentAddress Key, long Bytes, Instant Started, Option<string> Session);

public static class BlobGc {
    // WRITE-BLOB-FIRST: `open` appends the `PendingWrite` ledger row, `SealSource` applies the row's residence
    // transform ONCE (a `ClientSealed` row AES-GCM-seals under a freshly-minted DEK, every other row passes the
    // bytes through), the ONE receipt path (`MultipartTransfer.Upload`) write-once-seals the blob (the row's
    // `ObjectLock` stance SET on the wire) and emits the `BlobTransferReceipt`
    // facts with the frame's correlation threaded onto the residence, the catalog row commits carrying the WORM
    // window AND the wrapped DEK, then `close` clears the pending row — THREE durable marks, the sweep reading the
    // open pending set as the
    // in-flight fence. A crash before the catalog commit leaves a present blob with an OPEN pending row (protected
    // until the blob retention schedule's orphan age) and no event reference — a collectible orphan, never a dangling reference; the `412`-noop makes a
    // re-drive survive. The blob is NOT in the event's PG txn (`H10`). `WormUntil = receipt.WormUntil` — the ONE
    // instant `Upload` sampled derived the provider retention date AND this column, so catalog and provider agree by
    // construction (a second `frame.Now()` sample here was the two-clock divergence, deleted); `frame.Tenant` stamps
    // the catalog row's RLS partition — the [A.1] frame, never an AppHost type crossing down — and the FIRST step
    // refuses a `frame.Tenant`/`client.Tenant` mismatch (`Denied`, "tenant-mismatch"), so the name prefix the legs
    // write and the catalog row the sweep reads can never diverge: one injected tenant, structurally.
    public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ReadOnlyMemory<byte> source, Option<string> session, Func<PendingWrite, IO<Unit>> open, Func<BlobCatalogRow, IO<Unit>> catalog, Func<ContentAddress, IO<Unit>> close, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) =>
        from _t in frame.Tenant == client.Tenant ? IO.pure(unit) : IO.fail<Unit>(new RemoteStoreFault.Denied(key, store.Key, "tenant-mismatch"))
        from _o in open(new PendingWrite(key, source.Length, frame.Now(), session))
        from sealed_ in store.Encryption.SealSource(key, source)
        from receipt in MultipartTransfer.Upload(store, client, BlobResidence.From(key, sealed_.Bytes.Length, store) with { ConditionToken = session }, ContentChunker.Chunk(store.Chunking, sealed_.Bytes), sealed_.Bytes, sink, frame, None)
        from _c in catalog(new BlobCatalogRow(key, RetentionClass.Blob, sealed_.Bytes.Length, store.Tier, None, frame.Tenant, DataClassification.Internal, receipt.WormUntil, sealed_.Dek, frame.Now()))
        from _x in close(key)
        select new BlobResidence(receipt.Key, receipt.Bytes, store.Tier, receipt.Parts, receipt.ResumedParts, None, receipt.Correlation);

    // The in-flight fence THIS owner contributes to the retention sweep's `eligible` predicate: a key under an OPEN
    // `PendingWrite` younger than `RetentionClass.Blob.Schedule.OrphanAge` is in-flight, not an orphan — every
    // other key is eligible (reachability + holds + age are the retention sweep's own stages, never re-decided here).
    // This is the ONE crash-window fence the catalog alone cannot express (a present blob whose catalog row has not yet
    // committed has no inventory row, so the pending ledger is the only evidence it is mid-write).
    public static Func<ContentAddress, bool> InFlightFence(Seq<PendingWrite> pending, Instant now) =>
        key => pending.Find(w => w.Key == key).Match(Some: w => now - w.Started >= RetentionClass.Blob.Schedule.OrphanAge, None: () => true);

    // The blob catalog row IS a `RetentionFact` for the `blob` class — the sealed `Bytes` field is the byte figure the
    // retention sweep budgets on (never a later filesystem stat), the content `Key` the identity, `Tier` the CURRENT
    // `StorageTier` the never-evict cold-tiering verdict reads (the `blob` class is `LossPolicy.NeverEvict`, so an aged
    // reachable blob `Cool`s one rung down the `RetentionCeiling.Demote` ladder rather than collecting), the `At` the age stamp.
    static RetentionFact ToFact(BlobCatalogRow row) => new(RetentionClass.Blob, row.Key, row.Bytes, row.Tier, row.At);

    // The cold-tier demotion the `blob` class's `LossPolicy.NeverEvict` cooling verdict re-PUTs at: the existing object's
    // bytes drained once through the read leg and re-sealed at the COLDER `StorageTier` (the `Version/retention#SWEEP_AND_GC`
    // `RetentionCeiling.Demote` ladder's next rung) — `Cool` keeps the blob resident, just one storage-class rung down, so
    // the demotion is a fetch → `Drain` → content-addressed re-PUT against the same content key UNDER the colder tier the
    // `Upload` `at` override threads onto the leg `Initiate` `S3StorageClass` (the same egress parameterization the row tier
    // defaults; the write-once seal makes the same-key re-PUT a benign `412`-noop at providers that already hold the
    // bytes, the storage-class change taking effect server-side). The re-PUT routes through `MultipartTransfer.Upload` —
    // the ONE receipt path — so a tier transition emits its `BlobTransferReceipt` facts under the sweep frame's
    // correlation; a no-op-sink `store.Put` beside the receipt engine is the table-[09] deleted form, never re-minted here.
    static IO<Unit> Demote(ObjectStore store, ObjectClient client, ContentAddress key, StorageTier colder, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) =>
        from stream in store.Fetch(client, key, None)
        from _ in ObjectIo.Drain(
            stream,
            source => MultipartTransfer.Upload(store, client, BlobResidence.From(key, source.Length, store) with { Tier = colder }, ContentChunker.Chunk(store.Chunking, source), source, sink, frame, Some(colder)).Map(static _ => unit))
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
        key => worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until
            ? IO.fail<Unit>(new RemoteStoreFault.Locked(key, w.Mode, w.Until))
            : store.Delete(client, key);

    // The reclaim is NOT a second sweeper: the catalog IS the authoritative inventory (a `store.List()`-then-`Filter` is
    // the deleted parallel executor), each row projects to a `RetentionFact`, and the WHOLE decision routes through the
    // `Version/retention#SWEEP_AND_GC` `RetentionSweep` — `Run` over the `blob` class with the full-history `reachable`
    // mark (a blob a historical AS-OF cut references exits first), the `holds`, `InFlightFence` AND the WORM-active keys as
    // the injected `eligible` predicate (a locked blob is ineligible like an in-flight write), then `Execute` with the
    // `WormEvict` arrow (refusing a still-locked key with `RemoteStoreFault.Locked`) AND `Demote` as the cold-tier re-PUT arrow
    // (the `blob` class is `NeverEvict`, so an aged reachable blob `Cool`s one rung instead of collecting) — so the orphan
    // reclaim, the cold-tiering, and the snapshot-spine reclaim share ONE executor and ONE receipt ledger, the blob lane
    // owning only its fact projection, its in-flight + WORM fence, and its tier-transition re-PUT (the injected `sink`
    // carries the demotion's transfer facts through the one receipt path).
    public static IO<SweepReceipt> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Reachability reachable, Seq<Hold> holds, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) {
        FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm = catalog.Choose(static r => r.WormUntil.Map(u => (r.Key, (Mode: "worm", Until: u)))).ToFrozenDictionary(static t => t.Key, static t => t.Item2);
        Instant now = frame.Now();
        Func<ContentAddress, bool> fence = InFlightFence(pending, now);
        (Seq<SweepVerdict> verdicts, _) = RetentionSweep.Run(
            RetentionClass.Blob, catalog.Map(ToFact), holds, reachable,
            key => fence(key) && !(worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until), now, frame.Correlation);
        return RetentionSweep.Execute(RetentionClass.Blob, verdicts, WormEvict(store, client, worm, now), (key, tier) => Demote(store, client, key, tier, sink, frame), frame);
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :--------------- | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | write protocol   | open-pending -> blob -> catalog -> close-pending | crash leaves a pending-fenced orphan, never a dangling reference   |
|  [02]   | txn owner        | identity+event in the Marten session             | blob is write-first; no two-ORM atomicity (`H10`)                  |
|  [03]   | GC executor      | the ONE `Version/retention` `RetentionSweep`     | projects `RetentionFact` + `InFlightFence`; no parallel sweeper    |
|  [04]   | GC reachability  | mark over EVERY AS-OF cut                        | full-history; head-only GC is forbidden (`H10`)                    |
|  [05]   | lineage catalog  | same row the snapshot spine has                  | registers in the `blob` retention class; one GC governs both       |
|  [06]   | encryption       | `ApplyS3`/`ApplyGcs`/`ApplyMinio`                | applied on every wire, Azure host-dialed; SSE key id host-supplied |
|  [07]   | WORM/object-lock | `WormUntil` column + `WormEvict` arrow           | eligibility fence + typed evict; `Locked` reachable, no 403 leak   |
|  [08]   | tenancy          | `Tenant` column + RLS-filtered catalog           | per-tenant name prefix; cross-tenant reclaim unrepresentable       |
