# [PERSISTENCE_STORE_BLOBSTORE]

Rasm.Persistence stores every admitted artifact class as content-keyed object bytes — one `ObjectStore` `[SmartEnum]` provider axis behind the `BlobRemote` placement contract, five rows deep (`S3`/`Azure`/`GCS`/`Minio` credentialed plus the credential-free `Presigned` grant), every write content-addressed, write-once-sealed through the `ConditionalWrite` `412`-noop, and routed through the one `MultipartTransfer.Upload` receipt path. The plane is asset-AGNOSTIC by algebra and its payload families are `Version/retention#RETENTION_CLASSES` `ArtifactKind` ROWS, so a second consumer admits as one row rather than forking the plane. Object names derive from the seam `ContentAddress` the kernel `XxHash128` mints — `ContentAddress.Of(UInt128)` wraps the `Object` node's `RepresentationContentHash` (`Body` display GLB, `Box` lossless heavy, `Axis` structural line, `FootPrint` space-boundary ring, each `Option<UInt128>`) and the `Coverage` node's `CoverageGrid.RasterKey`, so this store holds bytes and never mints a second identity, and PostgreSQL/SQLite/DuckDB never appear because the durable home for an artifact class's bytes is the object plane, never a relational row. Writes land WRITE-BLOB-FIRST — content-address the blob, write it, then reference the immutable hash from a Marten event outside the event's PostgreSQL transaction — so a crash yields a collectible orphan, never a dangling reference, and every blob registers in the `Version/retention#RETENTION_CLASSES` `blob` class where one full-history reachability GC governs it and the snapshot spine alike (`H10`). Identity plus event share the one `IDocumentSession` transaction (`Element/graph#STORE_RAIL`); the blob is write-first, referenced-after, never a two-ORM atomicity dance.

`ContentAddress`, `ChunkManifest`, and `ContentChunker` compose from `Element/codec`; the retention rail (`RetentionClass`/`StorageLane`/`LossPolicy`/`RetentionCeiling`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability`/`RetentionSweep`, the one deletion executor the blob GC routes through) from `Version/retention`; `ProjectionContext` (the `Element/graph#STORE_RAIL` [A.1] frame — mark/clock/correlation/tenant as injected values), `ReceiptSinkPort`, and `CommunityToolkit.HighPerformance` from the substrate. An above-seam `Rasm.Compute` analysis runner reaches analytical `Axis`/`FootPrint` blobs one-hop through its app-wired `Graph/element#NODE_MODEL` `GeometrySource` port — the seam owns the decode contract, this store the bytes. SSE key material stays a key-id string this lane only stamps on the wire; the DEK-wrapping envelope lifecycle and both cloud-KMS keyrings (signing and envelope) belong to `Element/authority#AUTHORITY`, never a blob-lane-local KMS envelope.

## [01]-[INDEX]

- [02]-[OBJECT_STORE]: the five-provider axis (four credentialed + the presigned-grant row) projecting `BlobRemote`, the write-once seal, the two-column integrity stance separating transport from identity, the SSE + WORM/object-lock/legal-hold + client-sealed + codec residence forms, the endpoint-parameterized grant mint, the closed fault rail over its verb-bearing operation identity, and the neutral re-drive verdict the bound port carries.
- [03]-[MULTIPART_TRANSFER]: the class-leading object-name projection and its form-bearing `BlobHandle` mint, the content-defined-chunk upload packing whole chunks into provider parts, exact-object conditional dedup, durable session resume, explicit abandon, and the fourteen-slot per-provider leg carrying tier transition, thaw, and page-at-a-time erase beside the transfer ceremony.
- [04]-[BLOB_GC]: the content-lineage retention row (with its WORM `WormUntil` window and its residence form) projecting to the `Version/retention` `RetentionFact`, the write-blob-first protocol + the in-flight + WORM fence, the provider-side lifecycle rules armed on each class prefix, and the reclaim routed through the ONE `RetentionSweep` deletion executor with typed `WormEvict` and metadata-only `Demote` arrows (never a blob-lane-local sweeper).

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore` the `[SmartEnum<string>]` provider axis under `ComparerAccessors.StringOrdinal` — each row carries the `PartSize` floor, `ChunkPolicy` window, `ObjectChecksum` integrity, `ConditionalWrite` seal, `StorageTier` class, `ObjectEncryption` SSE, `ObjectLock` WORM, and `EraseBatch` page and builds its `BlobRemote` from the resolved `ObjectClient` (the `[Union]` whose `Map` owns per-leg dispatch); `ObjectChecksum`/`StorageTier`/`ObjectCodec`/`RetentionMode`/`ObjectEncryption`/`ObjectLock` the closed policy vocabularies; `ObjectVerb` the closed operation vocabulary every leg slot and every lifted fault names; `ThawState` the cold-rung state family, `EraseTally` the partial-failure receipt, `GrantSigner` the one endpoint-parameterized presigner, `RemoteStoreFault` the closed boundary fault family under the `IStoreRetriable` discriminant; `StoreVerdict` the neutral attempt fold and `StoreRedrivePort` the composition-root-bound executor seam its unbound row degrades to a single pass.
- Cases: `s3`, `azure-blob`, `gcs`, `minio`, `presigned`, each carrying the `WormSeat`/`TierSeat` naming where its write stances are ENFORCED and the `Degrade` clause naming what the row gives up — the sweep closes here, no relational engine appearing because the object plane owns every durable artifact class's bytes behind `BlobRemote`. `presigned` inverts the row: a `GrantMinter`+`Roster` pair and a host-dialed `HttpClient` replace endpoint+credential, reaching domain-cloud planes no credentialed row can (the client-side credential never exists) and single-shot by construction (upstream `FileMeta` carries no checksum/etag, no multipart/resume); Pollination seeds the minter, any other domain one more minter value, a sixth provider one row.
- Entry: `Placement` projects the row's `BlobRemote` with its write arrow routed through `MultipartTransfer.Upload` — the composed receipt path, so the frame's correlation lands on every residence and receipt; `Encode`/`Decode` own the residence transform in ONE fixed order, codec then seal, so no caller re-spells which frames first; `Put` drains the source once and partitions through `ContentChunker.Chunk` at the row `Tier` alone, a tier change being `Transition` rather than a second write; `Fetch`/`Head` are the read legs, `Head` reading the realized storage class and the stored form back through `StorageTier.Observed` and `ObjectCodec.Observed`; `Rehydrate` requests a thaw and reports `ThawState` either way; `EraseMany` chunks a group against the row's own `EraseBatch`; `Grant` is the ISSUER mint — the inverse of the presigned CONSUMER row — and `GrantDemand` carries operation plus lifetime as one admitted request so no deadline knob travels beside it. A credential-free viewer streams the resulting grant provider-direct after the caller gates the demand through `Element/authority#AUTHORITY` `Admit`.
- Auto: content-defined chunks pack into provider parts of at least `PartSize`, but only the exact object-name seal proves whole-blob residence; chunk membership never short-circuits a provider that cannot assemble an object from foreign parts. A re-put of an existing key `412`s to `RemoteStoreFault.Conflict`, and one `@catch` arm confirms the exact object by `Head` before yielding the benign no-op. `Encryption`, `Lock`, and the residence form all apply through the ONE `Stamp` fold per request type — SSE first, WORM second, form last — so a leg silently dropping a column is unrepresentable; `Governance`/`Compliance` make the bytes immutable for `Retain` and record the window through `Lock.Until` onto `#BLOB_GC` `BlobCatalogRow.WormUntil`, which is what makes `RemoteStoreFault.Locked` reachable. A `Fetch` against a rung the provider holds offline refuses `Frozen` from the provider's OWN error code, so no read pays a probing `Head` and no thaw-requiring rung reads as a denial.
- Receipt: a `BlobTransferFact` rides `store.blob.*` — `part` per uploaded window, `resume` per skipped-committed window, `conflict-noop` per exact-object `412`, `tier` per realized transition, `lifecycle-noop` per rung a provider rule already moved, `abort` per torn ceremony; `EraseTally` carries accepted and refused as separate columns over one page; the envelope stamps the HLC, so no fact carries an `Instant`.
- Packages: AWSSDK.S3 (`GetPreSignedURLAsync`/`GetPreSignedUrlRequest`/`HttpVerb` the one presigner, `RestoreObjectRequest`/`GlacierJobTier` the thaw, `CopyObjectRequest.StorageClass`+`S3MetadataDirective` the transition, `DeleteObjectsRequest.Objects` over `KeyVersion` the erase page, `ChecksumMode` the read stance), Azure.Storage.Blobs (`GenerateSasUri`/`BlobSasPermissions`; `SetImmutabilityPolicyAsync`/`SetLegalHoldAsync`/`DeleteImmutabilityPolicyAsync` the per-blob WORM rung; `SetAccessTierAsync`+`RehydratePriority` the transition and thaw), Azure.Storage.Blobs.Batch (`BlobBatchClient`/`BlobBatch` the erase page), Azure.Storage.Common (`DownloadTransferValidationOptions`/`StorageChecksumAlgorithm` the read stance), Google.Cloud.Storage.V1 (`UrlSigner.SignAsync` the V4 issuer, `PatchObjectAsync`+`PatchObjectOptions.OverrideUnlockedRetention` the per-object retention, `CopyObjectOptions.ExtraMetadata` the transition, `DownloadValidationMode` the read stance), Minio (`RemoveObjectsArgs` the erase page, `Minio.Exceptions.DeleteError` its refusal row), ZstdSharp.Port and K4os.Compression.LZ4[.Streams] (the codec rows' encoder pairs), CommunityToolkit.HighPerformance, System.IO.Hashing, System.Security.Cryptography inbox (`AesGcm`/`CryptographicOperations.ZeroMemory` the client-seal pair), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: one `ObjectStore` row absorbs a new provider with zero new surface (`presigned` exercised it — one row, one leg, one in-band fault); a new presigned domain is one `GrantMinter` value, a new storage class one `StorageTier` row, a new SSE stance one `ObjectEncryption` case (`ClientSealed` exercised it — one case, one seal/open pair, one catalog column), a new stored form one `ObjectCodec` row (its encoder and decoder columns and its metadata spelling, zero leg edits), a new WORM stance one `ObjectLock` case admitted only where the row's `WormSeat` can hold it, a new checksum posture one `ObjectChecksum` row answering the whole read/write column family, a new grant modality one `GrantRequest` case the collapsed signer already spells a verb for, a tighter window one `ChunkPolicy` row at `#CONTENT_CHUNKING`, a new boundary failure one `RemoteStoreFault` case, a new dialed operation one `ObjectVerb` row every leg slot and every lifted fault then names; a per-provider upload service, a second presigner beside the endpoint-parameterized one, a row delegate re-discriminating the union, a second HTTP uploader, or a `client is ObjectClient.S3 ?` guard is the deleted form because the union case IS the dispatch.
- Boundary: the content-key object name derives from the `Element/codec#CONTENT_ADDRESS` identity the kernel mints, so the store never mints a second identity and the M2-neutral representation map leaks no IFC name (the `Bim` projector owns IFC mapping behind those neutral keys); per-leg dispatch is `ObjectClient.Map`, so a per-provider service class and a mismatch guard are the deleted forms. Write-once is the optimistic-concurrency edge each provider exposes (S3/Minio `IfNoneMatch:*`, Azure `ETag.All`, GCS `IfGenerationMatch:0`), so a content-address store needs no read-before-write and a `412` folds to `RemoteStoreFault.Conflict` treated as success; every SDK exception lifts once into `RemoteStoreFault` at this edge and `IStoreRetriable.IsTransient` is the sole retriability CLASSIFIER — this tier publishes the discriminant and executes no retry, so a `408`/`429`/`5xx` reads re-offerable while `Conflict`/`NotFound`/`Locked`/`Frozen`/`IntegrityBreach`/`Denied`/`Oversize` reads deterministic, and executing a retry here mints a second reliability owner beside the one the caller composes; the object plane crosses a PROCESS SEAM, so `docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` seats the executor at the composition root's hop pipeline and this page's whole contribution is the `StoreVerdict.Of` fold and the `StoreRedrivePort` the root binds — a package referencing `{Rasm, Rasm.Element}` alone can name no pipeline type, so the seam crosses on this package's own currency and an unbound port degrades to one pass with the typed refusal intact; the pipeline is admissible precisely because no dialed op carries a multi-statement transaction — a content-addressed PUT and a conditional seal are each ONE request, so the executor brackets a single unit and replays from the boundary the unit begins at, which is the discriminant `resilience.md` uses to forbid a pipeline around transactional store work; `ObjectVerb` closes the operation identity that re-drive names, so a `Transport` case states which verb met which key and one provider code no longer wears two meanings across a fetch and a restore. The content key covers PLAINTEXT bytes and every residence form — codec, client seal, or both — frames the STORED bytes beneath it, so the two integrity claims separate structurally: the provider's own digest proves transport over what it holds, the domain fold proves identity over what the caller addressed, and `ProvesIdentity` is the one predicate admitting the single form where they coincide — a fence asserting either from the other is the substitution the two columns exist to make unspellable. Object metadata carries the stored form — the writer declares it and the reader observes it, never a caller argument on the read. Credential, endpoint, and region are host-resolved connection inputs, never fence members; the presigned row inverts the boundary — no client-side credential, the minter closure composed at the app root sees only `GrantRequest → IO<ObjectGrant>`, and only that expiry-aware minter can mint `GrantExpired`; a bare HTTP `403` remains `Denied` because status alone cannot distinguish expiry, signature failure, or policy refusal; a write stance admits only where the row's seat ENFORCES it — `ObjectLock.Until` projects unconditionally into the catalog `WormUntil` the GC fence reads, so a stance set on a row seating none stamped a retention window no provider holds and made the blob permanently un-evictable on a fiction, which `Admits` refuses at composition, and a tier ladder against a row seating no storage class is a declared no-op rather than a receipted transition that moved nothing.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `RemoteStoreFault` derives from the KERNEL federation base, so the bare `Expected` names `Rasm.Domain.Expected`
// (parameterless protected ctor + `Category` virtual) and NEVER the `LanguageExt.Common.Expected` whose
// `(string,int,Option)` ctor is the deleted form. `FaultBand` (the graph#FAULT_TABLES registry) and the
// `EnvelopeKeyring`/`EnvelopeAad`/`WrappedKey` envelope surface arrive from the Element tier.
using Rasm.Domain;                                  // CorrelationId/TenantId — the S0 causal pair the frame seats
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectChecksum {
    public static readonly ObjectChecksum XxHash128 = new("xxh128", ChecksumAlgorithm.XXHASH128, StorageChecksumAlgorithm.None, DownloadValidationMode.Never, identity: true);
    public static readonly ObjectChecksum Crc64 = new("crc64", ChecksumAlgorithm.CRC64NVME, StorageChecksumAlgorithm.StorageCrc64, DownloadValidationMode.Never, identity: false);
    // The GCS SDK-native whole-object stance (`Object.Crc32c` — GCS exposes NO CRC64); the SDK verifies it internally, so the
    // row supplies no S3 wire algorithm and no `Wire` digest.
    public static readonly ObjectChecksum Crc32c = new("crc32c", null, StorageChecksumAlgorithm.None, DownloadValidationMode.Always, identity: false);
    public static readonly ObjectChecksum None = new("none", null, StorageChecksumAlgorithm.None, DownloadValidationMode.Never, identity: false);
    public ChecksumAlgorithm? S3Algorithm { get; }
    // Read-side columns, one per provider whose SDK exposes a download-validation knob. Azure's rides
    // `Azure.Storage.DownloadTransferValidationOptions` out of `Azure.Storage.Common` — a DIFFERENT assembly from the one the
    // client comes from — and GCS's is an options enum; a row arming neither states `None`/`Never` rather than omitting the
    // column, so every row answers the whole family (`docs/laws/topology.md` `[BACKEND_ROW_COLUMN]`).
    public StorageChecksumAlgorithm AzureAlgorithm { get; }
    public DownloadValidationMode GcsValidation { get; }
    // TWO claims, never one. `S3Algorithm`/`AzureAlgorithm`/`GcsValidation`/`Wire` name the TRANSPORT digest the
    // provider verifies over STORED bytes; `Identity` declares whether that same digest IS the
    // `Element/codec#CONTENT_ADDRESS` key over PLAINTEXT. Only the `XxHash128` row answers both with one value — the
    // content key IS its supplied whole-object checksum — so every other row proves transport alone and leaves the
    // identity claim to the domain-side fold `ObjectStore.Decode` runs. Asserting the content key from a passing CRC
    // reads a claim nobody made, and the two columns exist so no fence can make that substitution silently.
    public bool Identity { get; }
    private ObjectChecksum(string key, ChecksumAlgorithm? s3Algorithm, StorageChecksumAlgorithm azureAlgorithm, DownloadValidationMode gcsValidation, bool identity) : this(key) =>
        (S3Algorithm, AzureAlgorithm, GcsValidation, Identity) = (s3Algorithm, azureAlgorithm, gcsValidation, identity);
    // The content key IS a 128-bit `XxHash128`, so the `XXHASH128` row hands S3 the SAME digest base64-encoded as the
    // PRECOMPUTED whole-object checksum — never a second hash. `Wire` is the `x-amz-checksum-xxh128` value the S3 `Seal`
    // SUPPLIES on `CompleteMultipartUploadRequest.ChecksumXXHASH128` (paired with the `Initiate`'s
    // `ChecksumType.FULL_OBJECT` stance) so the provider verifies the sealed object against the content key with ZERO
    // server-side re-hash — the content key IS the supplied digest. A non-`XxHash128` row supplies `None` and falls back
    // to the provider's SDK-native transfer integrity (`Crc64` on Azure, `Crc32c` on GCS, Minio's transport check under
    // `None`). `Seal` consumes this as its live whole-object checksum, so `Wire` is a load-bearing upload member, never a
    // decorative single-purpose projection.
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

    // READ-side application, mirroring the write-side stance arm for arm: the write path verified the whole-object
    // digest BEFORE the seal while the fetch validated nothing on the way back, so a wire that corrupted a byte reached a
    // decoder unchallenged at the one edge this column exists to guard. Each provider arms its own knob from its own
    // column — S3 the single-member `ChecksumMode.ENABLED` (armed exactly where the row supplies a write algorithm),
    // Azure the `Azure.Storage.Common` validation options with SDK auto-verify, GCS its download-validation mode — and
    // Minio arms nothing because its whole `*Args` algebra publishes no read-checksum setter, a recorded structural
    // negative rather than a silence. This is the TRANSPORT claim alone and it runs BEFORE any decode, which is what lets
    // `ObjectStore.Decode` gate the codec and the seal behind a proven wire.
    public GetObjectRequest ApplyS3(GetObjectRequest request) =>
        (request.ChecksumMode = S3Algorithm is null ? null : ChecksumMode.ENABLED, request).Item2;

    public BlobDownloadOptions ApplyAzure(BlobDownloadOptions options) =>
        (options.TransferValidation = new DownloadTransferValidationOptions { ChecksumAlgorithm = AzureAlgorithm, AutoValidateChecksum = true }, options).Item2;

    public DownloadObjectOptions ApplyGcs(DownloadObjectOptions options) =>
        (options.DownloadValidationMode = GcsValidation, options).Item2;
}

// Stored-form codec applied BEFORE the seal, a `ChunkPolicy`-peer on the residence axis rather than a knob: the three
// rows close at pass-through beside the low-latency and high-ratio codecs, and a fourth posture is one more row.
// LOAD-BEARING: the `Element/codec#CONTENT_ADDRESS` key covers PLAINTEXT bytes. Keying the stored bytes would fork one
// payload's address per codec AND per codec level, so a cache hit under `docs/laws/patterns.md` `[CONTENT_KEY]` would
// depend on which row wrote it — the same identity fork the raw pass-through row exists to refuse. The row therefore
// travels as object METADATA on the `BlobHandle` the dispatch layer mints, `Head` reads it back through `Observed`, and a
// read decompresses by that observation; a caller-supplied codec on the read path is the deleted form because it lets a
// reader name a form the writer never used.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectCodec {
    // Pass-through writes NO directory and NO frame header, so an uncompressed object's stored bytes ARE its plaintext
    // bytes and every offset identity the placement bundle's ranged fetch depends on survives untouched. Its two
    // delegates are never invoked — every entry short-circuits the row — and stay total so the column has no null hole.
    public static readonly ObjectCodec Identity = new("identity", static sink => sink, static source => source, level: 0);
    public static readonly ObjectCodec Zstd = new("zstd",
        static sink => new ZstdSharp.CompressionStream(sink, level: 3, bufferSize: 0, leaveOpen: true),
        static source => new ZstdSharp.DecompressionStream(source, bufferSize: 0, checkEndOfStream: true, leaveOpen: true), level: 3);
    public static readonly ObjectCodec Lz4 = new("lz4",
        static sink => LZ4Stream.Encode(sink, LZ4Level.L09_HC, extraMemory: 0, leaveOpen: true),
        static source => LZ4Stream.Decode(source, extraMemory: 0, leaveOpen: true, interactive: false), level: (int)LZ4Level.L09_HC);
    // Two provider metadata keys carry the residence form: WHICH row wrote the bytes, and the PLAINTEXT length the
    // frame arithmetic needs. Stored length alone cannot yield it once a codec sits between plaintext and seal, so the
    // writer states it rather than a reader inferring it from a ratio nobody measured.
    public const string CodecKey = "rasm-codec";
    public const string PlainKey = "rasm-plain";
    // TRAP, opposite defaults on one seam: `ZstdSharp.CompressionStream` defaults `leaveOpen: true` — the INVERSE of the
    // BCL convention — while the static `K4os.Compression.LZ4.Streams.LZ4Stream.Encode` defaults it `false`. A frame walk
    // writes every frame into ONE pooled sink, so a row trusting the zstd default and a row trusting the LZ4 default
    // disagree about who closes that sink; both arms SPELL `leaveOpen: true` and neither reads a default. The decode
    // twins invert the same way. `LZ4Level` carries explicit NON-CONTIGUOUS values (`L00_FAST=0`, the HC band 3-9,
    // `L10_OPT` through `L12_MAX`), so the level column is the enum value, never an ordinal the roster renumbers.
    public Func<Stream, Stream> Encoder { get; }
    public Func<Stream, Stream> Decoder { get; }
    public int Level { get; }
    private ObjectCodec(string key, Func<Stream, Stream> encoder, Func<Stream, Stream> decoder, int level) : this(key) =>
        (Encoder, Decoder, Level) = (encoder, decoder, level);

    // Metadata's REVERSE, ONE entry over every row, mirroring `StorageTier.Observed`: a `Head` reads the
    // provider's own metadata dictionary and an unstated or unmapped value falls back to `Identity`, the only safe
    // fallback because it is also the only row whose stored bytes need no decode. An object written before a row was
    // admitted therefore reads correctly with no migration pass and no stored version column.
    public static ObjectCodec Observed(string? stated) =>
        toSeq(Items).Find(row => row.Key.Equals(stated, StringComparison.OrdinalIgnoreCase)).IfNone(Identity);

    // Frame the codec at the SAME stride the seal frames at, so ONE stride serves both stages and a window resolves
    // through both with no second policy. The directory is the whole index — an 8-byte plaintext length then one 4-byte
    // entry per frame, its prefix sums the stored offsets — self-describing, so no sidecar rides the catalog and
    // nothing drifts from the bytes. The entry's HIGH BIT marks a frame stored PLAIN: an incompressible frame keeps its
    // own bytes and the directory records what the encoder DID rather than what the row asked for, which is also what
    // stops a compressed frame coincidentally equal to its plaintext length from being misread as stored plain.
    public readonly record struct CodecFrame(long Stride, long Plain) {
        public const int Entry = sizeof(uint);
        public const uint Stored = 0x8000_0000u;
        public static CodecFrame Of(ChunkPolicy policy, long plain) => new(policy.Max, plain);
        public long Count => (Plain + Stride - 1) / Stride;
        public long Directory => sizeof(ulong) + (Count * Entry);
        public long Span(long ordinal) => long.Min(Stride, Plain - (ordinal * Stride));
        public long Length(ReadOnlySpan<byte> directory, long ordinal) =>
            BinaryPrimitives.ReadUInt32BigEndian(directory[(int)(sizeof(ulong) + (ordinal * Entry))..]) & ~Stored;
        public bool Verbatim(ReadOnlySpan<byte> directory, long ordinal) =>
            (BinaryPrimitives.ReadUInt32BigEndian(directory[(int)(sizeof(ulong) + (ordinal * Entry))..]) & Stored) != 0;
        // Stored byte run covering every frame the plaintext window touches, the SAME shape `SealFrame.Window`
        // returns so the two stages compose as one arithmetic rather than two conventions.
        public (long Start, long End, long Skip) Window(ReadOnlySpan<byte> directory, long plainStart, long plainEnd) {
            long first = plainStart / Stride, last = plainEnd / Stride, start = Directory, end = Directory;
            for (long ordinal = 0; ordinal <= last; ordinal++) {                // Exemption: the prefix sum is the platform-forced statement seam
                if (ordinal < first) start += Length(directory, ordinal);
                end += Length(directory, ordinal);
            }
            return (start, end - 1, plainStart - (first * Stride));
        }
    }

    // ONE owner carries both directions — a sibling owner split by direction is the rejected form. `Pack` writes the
    // directory then walks the frames through the row's own streaming encoder over one pooled scratch, choosing the
    // smaller of encoded and verbatim per frame; `Unpack` reverses a CONTIGUOUS RUN so the decode cost tracks the
    // window rather than the object, exactly as the seal's own run-scoped open does.
    public IO<ReadOnlySequence<byte>> Pack(ChunkPolicy policy, ReadOnlySequence<byte> plain) =>
        this == Identity
            ? IO.pure(plain)
            : IO.lift(() => {                                                  // Exemption: the frame walk is the platform-forced statement seam
                CodecFrame frame = CodecFrame.Of(policy, plain.Length);
                byte[] packed = new byte[frame.Directory + plain.Length];      // A verbatim frame bounds the body by the plaintext, so this is exact
                BinaryPrimitives.WriteUInt64BigEndian(packed, (ulong)plain.Length);
                using ArrayPoolBufferWriter<byte> scratch = new();
                long at = frame.Directory;
                for (long ordinal = 0; ordinal < frame.Count; ordinal++) {
                    int span = (int)frame.Span(ordinal);
                    ReadOnlySequence<byte> slice = plain.Slice(ordinal * frame.Stride, span);
                    scratch.Clear();
                    using (Stream encoder = Encoder(scratch.AsStream())) slice.AsStream().CopyTo(encoder);
                    bool verbatim = scratch.WrittenCount >= span;
                    int wrote = verbatim ? span : scratch.WrittenCount;
                    if (verbatim) slice.CopyTo(packed.AsSpan((int)at, span)); else scratch.WrittenSpan.CopyTo(packed.AsSpan((int)at));
                    BinaryPrimitives.WriteUInt32BigEndian(packed.AsSpan((int)(sizeof(ulong) + (ordinal * CodecFrame.Entry))), (uint)wrote | (verbatim ? CodecFrame.Stored : 0u));
                    at += wrote;
                }
                return new ReadOnlySequence<byte>(packed.AsMemory(0, (int)at));
            });

    public IO<ReadOnlyMemory<byte>> Unpack(ChunkPolicy policy, long plain, ReadOnlyMemory<byte> directory, long ordinal, ReadOnlySequence<byte> run) =>
        this == Identity
            ? IO.pure(run.IsSingleSegment ? run.First : run.ToArray())
            : IO.lift(() => {                                                  // Exemption: the frame walk is the platform-forced statement seam
                CodecFrame frame = CodecFrame.Of(policy, plain);
                long last = ordinal, at = 0L;
                for (; at < run.Length && last < frame.Count; last++) at += frame.Length(directory.Span, last);
                byte[] opened = new byte[(last - ordinal) * frame.Stride];
                long read = 0L, wrote = 0L;
                for (long index = ordinal; index < last; index++) {
                    long span = frame.Length(directory.Span, index);
                    ReadOnlySequence<byte> slot = run.Slice(read, span);
                    if (frame.Verbatim(directory.Span, index)) slot.CopyTo(opened.AsSpan((int)wrote));
                    else using (Stream decoder = Decoder(slot.AsStream())) decoder.ReadExactly(opened.AsSpan((int)wrote, (int)frame.Span(index)));
                    (read, wrote) = (read + span, wrote + frame.Span(index));
                }
                return (ReadOnlyMemory<byte>)opened.AsMemory(0, (int)wrote);
            });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
// Every tiering provider carries its own residence column, so a Demote re-PUT changes residence on EVERY provider
// that has tiers — an S3-class-only row made the ladder a paid no-op on four of five provider rows. Minio exposes
// no server tier, so its column is None and the Demote arrow no-ops THERE BY DECLARATION, never by accident.
public sealed partial class StorageTier {
    public static readonly StorageTier Standard = new("standard", S3StorageClass.Standard, AccessTier.Hot, "STANDARD");
    public static readonly StorageTier Infrequent = new("infrequent", S3StorageClass.StandardInfrequentAccess, AccessTier.Cool, "NEARLINE");
    public static readonly StorageTier Cold = new("cold", S3StorageClass.GlacierInstantRetrieval, AccessTier.Cold, "COLDLINE");
    public static readonly StorageTier Archive = new("archive", S3StorageClass.DeepArchive, AccessTier.Archive, "ARCHIVE");
    public S3StorageClass S3Class { get; }
    public AccessTier AzureTier { get; }
    // GCS storage classes are protocol strings on the object resource, never an SDK enum.
    public string GcsClass { get; }
    private StorageTier(string key, S3StorageClass s3Class, AccessTier azureTier, string gcsClass) : this(key) =>
        (S3Class, AzureTier, GcsClass) = (s3Class, azureTier, gcsClass);

    // The REVERSE of the three columns, ONE entry over all of them: every tiering provider states the REALIZED class as a
    // string on its head response (S3 `GetObjectMetadataResponse.StorageClass`, Azure `BlobProperties.AccessTier`, GCS
    // `Object.StorageClass`) and the three vocabularies are disjoint, so one lookup answers every row where three
    // per-provider parsers would drift. `None` on an unstated or unmapped class, which is what lets a `Head` fall back to the
    // provider row's declared `Tier` rather than asserting a residence nobody observed. This is the fact the `#BLOB_GC`
    // `Demote` observation gate reads: a rung a provider lifecycle rule already realized costs no re-PUT.
    public static Option<StorageTier> Observed(string? stated) =>
        string.IsNullOrEmpty(stated)
            ? None
            : toSeq(Items).Find(row => stated.Equals(row.S3Class.Value, StringComparison.OrdinalIgnoreCase)
                                    || stated.Equals(row.AzureTier.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || stated.Equals(row.GcsClass, StringComparison.OrdinalIgnoreCase));
}

[Union]
public abstract partial record ObjectEncryption {
    public sealed record ProviderManaged : ObjectEncryption;
    public sealed record ManagedKey(string KeyId, FrozenDictionary<string, string> Aad) : ObjectEncryption;
    public sealed record CustomerKey(ReadOnlyMemory<byte> Key, string KeyMd5) : ObjectEncryption;
    // The zero-trust residence class no SSE stance reaches: the bytes AES-GCM-seal under a per-blob DEK BEFORE
    // chunking/upload (`SealSource`), so provider-held keys never see plaintext — classified/confidentiality-bound models
    // become admissible on any provider row. The DEK mints through the identity-tier `Element/identity#KMS_CUSTODY`
    // `EnvelopeKeyring` under the AAD binding; the `WrappedKey` rides `BlobCatalogRow.Dek`; every provider `Apply*` arm
    // is a no-op and the row pairs with `ObjectChecksum.None`. `Acquire` is the content-key CAS: every writer for one
    // address receives the same wrapped DEK, and the nonce derives from that address. A resume replays identical
    // ciphertext and a race catalogs only one envelope.
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
    // `IServerSideEncryption` on the put. Azure SSE is a CLIENT-construction fact —
    // `CustomerProvidedKey`/`EncryptionScope` are baked into the `ObjectClient.Azure` container by the host at dial, NOT a
    // per-request member — so the Azure leg applies nothing and the column is honored at the client the host hands in. So the
    // declared `ObjectStore.Encryption` row is honored on EVERY provider's wire (request or client) — a decorative column
    // promised only in prose is the deleted form, and a missing leg arm silently dropping the column is the gap this
    // dispatch closes.
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

    // The client-seal pair: `SealSource` resolves one content-key-stable DEK through `Acquire`, AES-GCM-seals the payload
    // (12-byte nonce | 16-byte tag | ciphertext), zeroizes the plaintext DEK, and yields the ciphertext plus the
    // `WrappedKey` the catalog row persists; `OpenSource` unwraps the DEK and opens the frame on the read path. Every
    // non-`ClientSealed` case passes bytes through untouched with no DEK — ONE polymorphic transform on the write/read
    // path, never a parallel encrypting store. The client seal frames PER FRAME, never per object: a ranged read against a
    // whole-object seal had to fetch and AES-GCM-open the entire blob before slicing, so the one-hop partial fetch the
    // placement bundle exists for (a mesh LOD, a BREP byte window) degraded to a full transfer on exactly the rows most
    // likely to be heavy. The stride DERIVES from the row's own `ChunkPolicy.Max` rather than arriving as a knob, and it
    // is a FIXED stride where the content-defined cut is variable — the two serve different questions: the content cut
    // buys cross-payload dedup, the fixed frame buys constant-time offset arithmetic, so a window resolves to a frame
    // span with no per-object manifest to carry and no index to keep in step with the bytes.
    public readonly record struct SealFrame(long Stride) {
        public const int Overhead = 12 + 16;
        public static SealFrame Of(ChunkPolicy policy) => new(policy.Max);
        public long Sealed(long plain) => plain + (Count(plain) * Overhead);
        public long Plain(long sealedLength) => sealedLength - (Frames(sealedLength) * Overhead);
        public long Count(long plain) => (plain + Stride - 1) / Stride;
        long Frames(long sealedLength) => (sealedLength + Stride + Overhead - 1) / (Stride + Overhead);
        // The sealed byte window covering every frame the plaintext window touches — the ONE arithmetic both the fetch
        // range and the post-open slice read, so a ranged read transfers the frames it needs and nothing else.
        public (long Start, long End, long Skip) Window(long plainStart, long plainEnd) {
            long first = plainStart / Stride;
            long last = plainEnd / Stride;
            return (first * (Stride + Overhead), ((last + 1) * (Stride + Overhead)) - 1, plainStart - (first * Stride));
        }
    }

    // Seal every frame under ONE content-key-stable DEK, the frame ORDINAL folded into the nonce's low word so no
    // two frames of one object reuse a nonce — the AES-GCM contract's one non-negotiable. The nonce's key half
    // composes the kernel `ContentHash.Half` high lane; its big-endian byte WRITE is this sealed-frame format's
    // own frozen projection per the owner's boundary law — stored layout, never a second lane convention. The DEK zeroizes once
    // after the whole walk rather than per frame, because `Acquire` is the CAS and re-acquiring per frame would
    // multiply the keyring round trip by the object's frame count.
    public IO<(ReadOnlySequence<byte> Bytes, Option<WrappedKey> Dek)> SealSource(ContentAddress key, ChunkPolicy policy, ReadOnlySequence<byte> plain) =>
        this is ClientSealed sealed_
            ? sealed_.Acquire(key).Map(minted => {                            // Exemption: the AEAD frame walk is the platform-forced statement seam
                SealFrame frame = SealFrame.Of(policy);
                byte[] framed = new byte[frame.Sealed(plain.Length)];
                try {
                    using System.Security.Cryptography.AesGcm aead = new(minted.Dek.Span, tagSizeInBytes: 16);
                    for (long ordinal = 0; ordinal < frame.Count(plain.Length); ordinal++) {
                        long at = ordinal * frame.Stride;
                        int span = (int)long.Min(frame.Stride, plain.Length - at);
                        Span<byte> slot = framed.AsSpan((int)(ordinal * (frame.Stride + SealFrame.Overhead)));
                        BinaryPrimitives.WriteUInt64BigEndian(slot[..8], ContentHash.Half(key.Value, 1));
                        BinaryPrimitives.WriteUInt32BigEndian(slot.Slice(8, 4), (uint)ordinal);
                        plain.Slice(at, span).CopyTo(slot.Slice(SealFrame.Overhead, span));
                        aead.Encrypt(slot[..12], slot.Slice(SealFrame.Overhead, span), slot.Slice(SealFrame.Overhead, span), slot.Slice(12, 16));
                    }
                }
                finally {
                    System.Security.Cryptography.CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsMemory(minted.Dek).Span);
                }
                return (new ReadOnlySequence<byte>(framed), Some(minted.Wrapped));
            })
            : IO.pure((plain, Option<WrappedKey>.None));

    // Open a CONTIGUOUS RUN of frames — the whole object when the caller took no window, the spanned run when it
    // did — so the decrypt cost tracks the window rather than the object. `ordinal` seeds the nonce reconstruction
    // from the run's first frame, which is what lets a mid-object run open without the frames before it.
    public IO<ReadOnlyMemory<byte>> OpenSource(ContentAddress content, ChunkPolicy policy, long ordinal, ReadOnlySequence<byte> framed, Option<WrappedKey> dek) =>
        (this, dek) switch {
            (ClientSealed, { IsNone: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-envelope")),
            (ClientSealed, _) when framed.Length < SealFrame.Overhead => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-frame")),
            (ClientSealed sealed_, { IsSome: true }) => sealed_.Keyring.Unwrap(dek.ValueUnsafe(), sealed_.Aad).Map(key => {
                SealFrame frame = SealFrame.Of(policy);
                byte[] run = framed.ToArray();
                byte[] plain = new byte[frame.Plain(run.LongLength)];
                try {                                                          // Exemption: the AEAD frame walk is the platform-forced statement seam
                    using System.Security.Cryptography.AesGcm aead = new(key.Span, tagSizeInBytes: 16);
                    for (long index = 0; index * (frame.Stride + SealFrame.Overhead) < run.LongLength; index++) {
                        int at = (int)(index * (frame.Stride + SealFrame.Overhead));
                        int span = (int)long.Min(frame.Stride, run.LongLength - at - SealFrame.Overhead);
                        Span<byte> slot = run.AsSpan(at);
                        BinaryPrimitives.WriteUInt32BigEndian(slot.Slice(8, 4), (uint)(ordinal + index));
                        aead.Decrypt(slot[..12], slot.Slice(SealFrame.Overhead, span), slot.Slice(12, 16), plain.AsSpan((int)(index * frame.Stride), span));
                    }
                }
                finally {
                    System.Security.Cryptography.CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsMemory(key).Span);
                }
                return (ReadOnlyMemory<byte>)plain;
            }),
            (_, { IsSome: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "unexpected-envelope")),
            _ => IO.pure(framed.IsSingleSegment ? framed.First : framed.ToArray()),
        };

    // `stat` is the sealed length the frame arithmetic needs and the ONLY fact a ranged sealed read cannot derive
    // from its own window; the placement bundle already holds that leg, so the read composes it rather than
    // carrying a length knob. An unranged sealed read fetches the whole object exactly as before.
    public IO<Stream> Read(
        ContentAddress key,
        ChunkPolicy policy,
        Option<(long Start, long End)> range,
        Func<ContentAddress, IO<Option<WrappedKey>>> envelope,
        Func<ContentAddress, IO<Option<BlobResidence>>> stat,
        Func<Option<(long Start, long End)>, IO<Stream>> fetch) =>
        (this, range) switch {
            (ClientSealed, { IsSome: true, Case: (long Start, long End) window }) =>
                from present in stat(key)
                from resident in present.Match(Some: IO.pure, None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(key)))
                let frame = SealFrame.Of(policy)
                let plainLength = frame.Plain(resident.Length)
                from bounded in window is { Start: >= 0 } && window.End >= window.Start && window.End < plainLength
                    ? IO.pure(frame.Window(window.Start, window.End))
                    : IO.fail<(long Start, long End, long Skip)>(new RemoteStoreFault.InvalidRange(key, window.Start, window.End, plainLength))
                from dek in envelope(key)
                from raw in fetch(Some((bounded.Start, long.Min(bounded.End, resident.Length - 1))))
                from opened in ObjectIo.Drain(raw, run => OpenSource(key, policy, bounded.Start / (frame.Stride + SealFrame.Overhead), run, dek))
                select opened.Slice(checked((int)bounded.Skip), checked((int)(window.End - window.Start + 1))).AsStream(),
            (ClientSealed, _) =>
                from dek in envelope(key)
                from raw in fetch(None)
                from plain in ObjectIo.Drain(raw, run => OpenSource(key, policy, 0L, run, dek))
                select plain.AsStream(),
            _ => fetch(range),
        };
}

// The GCS per-object retention vocabulary, closed at the two modes `Object.RetentionData.Mode` admits. `Unlocked`
// releases and shortens under `OverrideUnlockedRetention`; `Locked` is irreversible and the override does not reach it,
// which is exactly the governance/compliance split every other provider spells, so the mapping is a column rather than a
// per-arm literal and a raw mode string never reaches a call site.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetentionMode {
    public static readonly RetentionMode Unlocked = new("Unlocked", releasable: true);
    public static readonly RetentionMode Locked = new("Locked", releasable: false);
    public bool Releasable { get; }
    private RetentionMode(string key, bool releasable) : this(key) => Releasable = releasable;
}

[Union]
public abstract partial record ObjectLock {
    public sealed record Off : ObjectLock;
    public sealed record Governance(Duration Retain) : ObjectLock;
    public sealed record Compliance(Duration Retain) : ObjectLock;
    // The third admitted lock modality (`ObjectLockLegalHoldStatus` on the S3 initiate): an INDEFINITE hold with no
    // retention date — released by an operator action, never a lapsing window — so `Until` projects `Instant.MaxValue`
    // and the GC fence holds the blob until the hold row is lifted from the catalog.
    public sealed record LegalHold : ObjectLock;

    // The WORM/object-lock retention stance APPLIED on the write so `RemoteStoreFault.Locked` is genuinely REACHABLE — a
    // compliance-class blob written under an active retention-until cannot be deleted until the window lapses, the SET
    // being the only thing that makes the retention/GC `Locked` surfacing real (the fault `H10` declares is otherwise
    // unmintable). `Off` writes no lock member; `Governance`/`Compliance` stamp the object-lock mode + a retention-until
    // of `now + Retain` where `now` is the ONE injected frame instant `Upload` samples ONCE and threads to every arm AND
    // to the catalog `WormUntil` — a per-arm ambient `DateTime.UtcNow` read is the two-clock split-brain that lets the
    // provider window and the catalog window diverge under skew or retry, the deleted form. S3 carries it on the
    // multipart-INITIATE (`ObjectLockMode`/`ObjectLockRetainUntilDate`), Minio on the put through the inherited
    // `ObjectWriteArgs.WithRetentionConfiguration` (`x-amz-object-lock-mode`/`x-amz-object-lock-retain-until-date`);
    // Azure and GCS bind PER BLOB after the seal (`ApplyAzure`/`ApplyGcs` on the `WormSeat.Followup` rung), so those legs
    // enforce the column they declare rather than surrendering it to a container or bucket dial. `Until` is the one
    // deadline derivation the catalog column and every Apply arm reads. Two providers carry it on the REQUEST and two on a
    // follow-up call, each getting one `Apply*` arm over the union (the only per-provider variance; the request types are
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
    // `x-amz-object-lock-legal-hold` on the write, so the `legalHold` arm applies it here; a no-op arm silently dropping the
    // column is exactly the gap this dispatch closes. Azure binds immutability PER BLOB after the object exists, so this
    // arm runs on the `WormSeat.Followup` rung the seal schedules rather than on a request member no Azure write carries.
    // `Off` makes no call at all, so a row without a stance costs no round trip, and retention mode maps to the same
    // governance/compliance pair every other provider spells. Both writers return a PAYLOAD response, not a bare one —
    // `SetImmutabilityPolicyAsync` yields `Task<Response<BlobImmutabilityPolicy>>` and `SetLegalHoldAsync` yields
    // `Task<Response<BlobLegalHoldResult>>` — so the carrier is the non-generic `Task` base and an arm typed to
    // `Task<Response>` fails to bind. `BlobImmutabilityPolicy` carries only `ExpiresOn` and `PolicyMode`, both NULLABLE,
    // and `DeleteImmutabilityPolicyAsync` is its release counterpart the `Governance`/`Unlocked` window releases through; a
    // `Locked` policy has no release verb at all.
    public Option<Func<BlobBaseClient, Instant, Task>> ApplyAzure(Instant now) => Map(
        off:        static (_, _) => Option<Func<BlobBaseClient, Instant, Task>>.None,
        governance: static (at, c) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetImmutabilityPolicyAsync(
            new BlobImmutabilityPolicy { ExpiresOn = (at + c.Retain).ToDateTimeOffset(), PolicyMode = BlobImmutabilityPolicyMode.Unlocked })),
        compliance: static (at, c) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetImmutabilityPolicyAsync(
            new BlobImmutabilityPolicy { ExpiresOn = (at + c.Retain).ToDateTimeOffset(), PolicyMode = BlobImmutabilityPolicyMode.Locked })),
        legalHold:  static (_, _) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetLegalHoldAsync(true)),
        state: now);

    // GCS carries PER-OBJECT retention, so this row seats `WormSeat.Followup` beside Azure rather than surrendering the
    // column to a bucket dial no leg can prove. The window rides `Object.Retention` — an `Object.RetentionData` carrying
    // `Mode` from the closed `RetentionMode` roster and `RetainUntilTimeDateTimeOffset` — applied through
    // `PatchObjectAsync`, never `Object.RetentionExpirationTimeRaw`, which is the read-only BUCKET-policy expiry and a
    // different field entirely. `OverrideUnlockedRetention` arms only on the releasable mode: a re-drive re-applies an
    // idempotent policy against an already-sealed key, and without the override a clock-skewed re-apply that shortens an
    // existing unlocked window is refused, while a `Locked` window admits no override at all and needs none.
    // `Object.RetentionData.RetainUntilTime` is `[Obsolete]`; the `DateTimeOffset?` member is the live one.
    public Option<Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task>> ApplyGcs(Instant now) => Map(
        off:        static (_, _) => Option<Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task>>.None,
        governance: static (at, c) => Some(Patch(RetentionMode.Unlocked, at + c.Retain)),
        compliance: static (at, c) => Some(Patch(RetentionMode.Locked, at + c.Retain)),
        // GCS publishes no indefinite per-object hold on the retention field, so an unbounded hold projects as the
        // farthest representable retain-until — the same `Instant.MaxValue` the catalog column already carries.
        legalHold:  static (_, _) => Some(Patch(RetentionMode.Locked, Instant.MaxValue)),
        state: now);

    static Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task> Patch(RetentionMode mode, Instant until) =>
        (client, resource, _) => client.PatchObjectAsync(
            (resource.Retention = new Google.Apis.Storage.v1.Data.Object.RetentionData { Mode = mode.Key, RetainUntilTimeDateTimeOffset = until.ToDateTimeOffset() }, resource).Item2,
            new PatchObjectOptions { OverrideUnlockedRetention = mode.Releasable });

    public PutObjectArgs ApplyMinio(PutObjectArgs args, Instant now) => Switch(
        off:        static (s, _) => s.Args,
        governance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.GOVERNANCE)),
        compliance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.COMPLIANCE)),
        legalHold:  static (s, _) => s.Args.WithLegalHold(true),
        state: (Args: args, Now: now));
}

// A grant request names the operation the minter authorizes; the minted `ObjectGrant` is the executable wire shape —
// `FormPost` the presigned multipart/form-data POST (the upstream `S3UploadRequest { Url, Fields }` DTO
// decompile-verified on PollinationSDK 1.10.0), `SignedUrl` the bare GET/HEAD/DELETE URL. GENERIC parameterization: any
// presigned-grant cloud domain is one `GrantMinter` value — deployment DATA, zero central edits; Pollination is the SEED
// minter (`ArtifactsApi.CreateArtifactAsync → S3UploadRequest` mints writes,
// `DownloadArtifactAsync`/`JobsApi.DownloadJobArtifact` mint reads, `ListArtifactsAsync → FileMetaList` fills the roster
// on `FileMeta { Key, FileType, FileName, LastModified, Size }`).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GrantRequest {
    private GrantRequest() { }
    public sealed record Write(ContentAddress Key, long Length) : GrantRequest;
    public sealed record Read(ContentAddress Key) : GrantRequest;
    public sealed record Erase(ContentAddress Key) : GrantRequest;

    // The addressed content key over every case, seated on the union that owns the cases rather than as a free fold
    // beside it — a demand names one object whatever operation it authorizes. The leg `Issue` reads this for its fault
    // slot while the projected `BlobHandle` the dispatch layer threads supplies the wire name; the projection carries its
    // own name because a case positional member already spells `Key` and a shadowing base member is the deleted form.
    public ContentAddress Addressed => Switch(
        write: static w => w.Key, read: static g => g.Key, erase: static e => e.Key);
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

// ONE presigner over both S3-protocol rows, parameterized by the endpoint its client was dialed at rather than split per
// provider: `AmazonS3Config.ForcePathStyle` plus the inherited `ServiceURL` reach a self-hosted MinIO cluster with the
// same SigV4 machinery the cloud row uses, so the self-hosted lane gains a presigned DELETE its own SDK cannot mint.
// TRAP: `ForcePathStyle` declares on `AmazonS3Config` while `ServiceURL`, `UseHttp`, and `AuthenticationRegion` declare
// on `Amazon.Runtime.ClientConfig` in a DIFFERENT assembly, and setting `ServiceURL` NULLS `RegionEndpoint` — the two are
// mutually exclusive and the last write wins, so an endpoint dial that also pins a region silently drops one.
// `SignatureVersion` is not a config knob at all; `SignatureMethod` is the only settable signing column.
// `GetPreSignedURLAsync` takes NO `CancellationToken`, and `Expires` is a `DateTime?` with no duration alternative, so the
// deadline anchors on the injected frame instant here and never an ambient clock.
public readonly record struct GrantSigner(IAmazonS3 Client, string Bucket) {
    public async Task<ObjectGrant> Sign(GrantDemand demand, BlobHandle handle, Instant now) =>
        new ObjectGrant.SignedUrl(new Uri(await Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest {
            BucketName = Bucket, Key = handle.Name, Verb = Verb(demand.Request), Expires = (now + demand.Lifetime).ToDateTimeUtc(),
        }).ConfigureAwait(false)));

    // `Amazon.S3.HttpVerb` is a true enum carrying exactly `GET`/`HEAD`/`PUT`/`DELETE`, so every `GrantRequest` case
    // reaches a real verb and the grant family answers every case it declares.
    static HttpVerb Verb(GrantRequest request) => request.Switch(
        write: static _ => HttpVerb.PUT, read: static _ => HttpVerb.GET, erase: static _ => HttpVerb.DELETE);
}

// Thaw axis: cause-bearing foreign state is a closed family, never an `Option` — a caller holding `None` cannot tell
// "readable now" from "must ask" from "already asked", and those three decide three different next actions. `Frozen`
// means the rung holds the bytes offline and no restore is in flight; `Thawing` carries the provider's own published ETA
// where one exists and `None` where it publishes none, never a fabricated deadline.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThawState {
    private ThawState() { }
    public sealed record Resident : ThawState;
    public sealed record Frozen : ThawState;
    public sealed record Thawing(Option<Instant> Ready) : ThawState;
}

// Page-at-a-time erase receipt. BOTH halves report as separate columns because a merged tally cannot tell a page that
// erased everything from a page that refused everything — the conservation identity `Requested = Erased + Refused` holds
// by construction, so `Erased` is derived and can never disagree with the refusal roster it is computed from. The
// refusals carry the provider's own per-key code, which is the only evidence naming WHICH key survived and why.
public readonly record struct EraseTally(int Requested, Seq<(ContentAddress Key, string Code)> Refused) {
    public static readonly EraseTally Empty = new(0, Seq<(ContentAddress Key, string Code)>());
    public int Erased => Requested - Refused.Count;
    public static EraseTally operator +(EraseTally left, EraseTally right) => new(left.Requested + right.Requested, left.Refused + right.Refused);
}

[Union]
public abstract partial record ObjectClient {
    // The tenant partition this dialed client serves — minted at the composition root FROM the injected [A.1] frame
    // tenant, so the object name's tenant segment and the `BlobCatalogRow.Tenant` column trace to ONE source;
    // `BlobGc.WriteBlobFirst` refuses a frame/client mismatch, never a silent name/row split-brain.
    public required TenantId Tenant { get; init; }
    public sealed record S3(IAmazonS3 Client, string Bucket) : ObjectClient;
    public sealed record Azure(BlobContainerClient Container) : ObjectClient;
    // `Signer` is the credential-bound V4 `UrlSigner` the host dials beside the client — `StorageClient`
    // carries no signing surface, so issuer grants on the GCS row need the second host-dialed handle.
    public sealed record Gcs(StorageClient Client, string Bucket, UrlSigner Signer) : ObjectClient;
    // `Signer` is the S3-protocol presigner the host dials at the SAME endpoint beside the Minio client — the second
    // host-dialed handle pattern the `Gcs` row already takes. The Minio SDK publishes only `PresignedGetObjectArgs`,
    // `PresignedPutObjectArgs`, and `PresignedPostPolicyArgs`, so a presigned DELETE is unmintable there; the endpoint
    // is S3-compatible, so the collapsed `GrantSigner` mints every verb over the one signing owner.
    public sealed record Minio(IMinioClient Client, string Bucket, GrantSigner Signer) : ObjectClient;
    // The credential-free fifth row: no endpoint, no credential — a `GrantMinter` mints an `ObjectGrant` per operation, the
    // `Roster` delegate fills `Head`/`Enumerate` (the upstream list surface), and the host-dialed `HttpClient` is the
    // connection input the grants execute over.
    public sealed record Presigned(Func<GrantRequest, IO<ObjectGrant>> Minter, Func<Option<ContentAddress>, IO<Seq<BlobStat>>> Roster, HttpClient Http) : ObjectClient;
}

// `Correlation` is THREADED from the write op's `ProjectionContext.Correlation` by the one receipt path
// (`MultipartTransfer.Upload`) — a read-leg `From` mints the kernel `CorrelationId.None` and the write path stamps the
// frame's correlation, so the residence a write yields is traceable to its causing op. `None` is the kernel's declared
// absent row, never a hand-spelled empty key the conversion policy refuses anyway. `Tier` arrives as the OBSERVED rung
// rather than the provider row's declared default: a `Head` reads the realized storage class back through
// `StorageTier.Observed` and falls back to the row `Tier` only where the provider states none, so the residence reports
// where the bytes ARE and the `#BLOB_GC` `Demote` gate can tell a realized rung from an assumed one. A factory re-reading
// `store.Tier` unconditionally is the form that made every residence claim the entry rung forever. `Length` is the STORED
// length and `Plain` the PLAINTEXT length the residence form decodes back to; the two coincide on an `Identity` row under
// no client seal and diverge the moment either stage frames, so a reader deriving one from the other measured a ratio
// nobody took. `Codec` reads back from the object's own metadata through `ObjectCodec. Observed`, so the stored form is the
// WRITER's declaration rather than a caller's assertion, and a fetch decodes by it.
public readonly record struct BlobResidence(ContentAddress Key, long Length, long Plain, StorageTier Tier, ObjectCodec Codec, int Parts, int ResumedParts, Option<ContentAddress> Verified, Option<string> ConditionToken, CorrelationId Correlation) {
    public static BlobResidence From(ContentAddress key, long length, long plain, StorageTier tier, ObjectCodec codec) => new(key, length, plain, tier, codec, 0, 0, None, None, CorrelationId.None);
}

// `Provider` is the attribution axis the `Store/observability#STORE_INSTRUMENTS` object-plane rows tag on: a fact that
// cannot name the provider that produced it is unattributable the moment a deployment carries two rows, and the receipt
// already carried the column the fact stream dropped.
public readonly record struct BlobTransferFact(string Provider, string Kind, ContentAddress Key, long Bytes, int Part, Option<string> Session);

// Closed operation vocabulary every leg slot names and every lifted fault carries, mirroring the
// `Store/provisioning#SERVER_EXTENSIONS` `Lane` shape — one owner, both sides drawn from it. `ColdRefuses` closes
// per-VERB reading of one provider code: `InvalidObjectState` means an object IS archived under a fetch and means
// its thaw is already in flight under a restore, so one code folds to `Frozen` on a refusing verb and to a benign
// in-flight state on `Restore`. Without this column, lifting is per-provider-exception and structurally cannot
// separate them, which is why every leg slot rides its verb into `Bound` rather than a caller argument on the fault.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectVerb {
    public static readonly ObjectVerb Write = new("write", coldRefuses: true);
    public static readonly ObjectVerb Read = new("read", coldRefuses: true);
    public static readonly ObjectVerb Erase = new("erase", coldRefuses: true);
    public static readonly ObjectVerb List = new("list", coldRefuses: true);
    public static readonly ObjectVerb Grant = new("grant", coldRefuses: true);
    public static readonly ObjectVerb Transition = new("transition", coldRefuses: true);
    public static readonly ObjectVerb Restore = new("restore", coldRefuses: false);
    public static readonly ObjectVerb Lifecycle = new("lifecycle", coldRefuses: true);
    public bool ColdRefuses { get; }
    private ObjectVerb(string key, bool coldRefuses) : this(key) => ColdRefuses = coldRefuses;
}

// Retriability discriminant BOTH process-seam store bands publish, so one re-drive fold reads one member and a
// third band joins with no fold edit. `RemoteStoreFault` and `Query/cache#INDEX_RESIDENCY` `WideColumnFault` realize
// it; a band whose re-drive owner is its callee's own transaction (`Store/coordination#COORDINATION_OP`) or its
// caller's in-process rail (`Store/provisioning#ENGINE_OPERATIONS`) does not, since seating it there would offer a
// pipeline to a seam that forbids one.
public interface IStoreRetriable {
    bool IsTransient { get; }
}

// The remote-store boundary fault band (540x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the Persistence
// siblings `Version/retention#RETENTION_CLASSES` `RetentionFault` (828x), `Version/recovery#RECOVERY_ROUTES`
// `RecoveryFault` (829x), and `Element/codec#SNAPSHOT_SPINE` `CodecFault` (83xx) realize — NOT
// `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no `Category` to override)
// is the deleted form that resolves to the wrong base and falls outside the kernel federation a telemetry reader bands by
// code. Band membership derives `Code => FaultBand.RemoteStore + n` through the registry pointer (a bare 540x literal
// beside the registry row is the decoupled form the siblings already reject), `Message`/`Category` projecting through the
// generated `Switch`, so a typed case lifts BARE onto `Fin<T>`/`IO<T>` with no `.ToError()` hop and a recovery reads
// `error.IsType<RemoteStoreFault.Conflict>()` / `error.HasCode(5402)` / `error.Category`, never a message substring. No
// `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in, so the band carries no generated per-case
// `SelfOp`. `IsTransient` stays an `abstract` discriminant with one override per case (orthogonal to the base-ctor
// change), and it CLASSIFIES alone — this tier publishes retriability and executes no retry, so the discriminant answers a
// caller's re-offer decision while execution seats at whichever rail the leg rides. `Create` is the IValidationError
// admission the generated converter bridge calls on a deserialization reject.
[Union]
public abstract partial record RemoteStoreFault : Expected, IValidationError<RemoteStoreFault>, IStoreRetriable {
    private RemoteStoreFault() : base() { }
    public abstract bool IsTransient { get; }
    public sealed record Text(string Detail) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record NotFound(ContentAddress Key) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Conflict(ContentAddress Key, string Condition) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Aborted(ContentAddress Key, int Parts, string Reason) : RemoteStoreFault { public override bool IsTransient => false; }
    // 408 joins 429 and the 5xx band as the closed transient set the branch's one transient definition already spells
    // (`docs/stacks/csharp/domain/resilience#STANDARD_POSTURE`); status 0 is the no-response connection failure. Being
    // this band's ONE re-drivable case, it alone carries the OPERATION IDENTITY a re-drive names — its verb and the
    // content address its attempt addressed. `Text` omits both and needs neither, being the non-transient open tail:
    // no executor re-offers it, so it has nothing to identify.
    public sealed record Transport(string Provider, ObjectVerb Verb, ContentAddress Key, int Status, string Code) : RemoteStoreFault { public override bool IsTransient => Status is 0 or 408 or 429 or >= 500; }
    public sealed record IntegrityBreach(ContentAddress Key, string Provider) : RemoteStoreFault { public override bool IsTransient => false; }
    // The WORM/object-lock retention fault — minted by the retention/GC `Execute` evict arrow when a blob under an active
    // object-lock retention-until cannot be deleted (the catalog row carries the lock window), NOT by the SDK `Lift` (a
    // provider 403 retention-block is not reliably distinguishable from an auth denial by status, so the SDK arm folds to
    // `Denied` with the code preserved). The object-store evict surfaces `Locked` from the domain-side retention check.
    public sealed record Locked(ContentAddress Key, string Mode, Instant Until) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Denied(ContentAddress Key, string Provider, string Code) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record Oversize(ContentAddress Key, string Provider, string Code) : RemoteStoreFault { public override bool IsTransient => false; }
    // The presigned row's expiry-aware minter emits this case only from its signed expiry evidence. A bare provider `403`
    // remains `Denied` because status cannot distinguish expiry from policy or signature refusal. No observed-instant
    // field — the receipt envelope stamps the HLC and a leg-local wall-clock read is the named inversion.
    public sealed record GrantExpired(ContentAddress Key) : RemoteStoreFault { public override bool IsTransient => false; }
    public sealed record InvalidRange(ContentAddress Key, long Start, long End, long Length) : RemoteStoreFault { public override bool IsTransient => false; }
    // Cold-rung read refusal, minted from the provider's OWN error code on the fetch rather than a `Head` before
    // every read — S3 states `InvalidObjectState`, Azure `BlobArchived`, and both arrive as a status the generic arm
    // would otherwise fold to `Denied`, hiding a rung the caller can act on behind an auth verdict it cannot. NOT
    // transient: re-offering the same fetch never thaws anything, so the discriminant correctly refuses a re-drive and the
    // caller routes to `Rehydrate` instead. It carries no ETA because the refusal publishes none; `ThawState` is the
    // surface that states one, from evidence rather than construction. Its `Verb` column makes one provider code
    // readable under two meanings: `ColdRefuses` true is a genuine cold-rung refusal, false is a restore verb meeting
    // that same code on an object never archived at all.
    public sealed record Frozen(ContentAddress Key, string Provider, ObjectVerb Verb) : RemoteStoreFault { public override bool IsTransient => false; }

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
        invalidRange:   static _ => 10,
        frozen:         static _ => 11);

    public override string Message => Switch(
        text:           static c => c.Detail,
        notFound:       static c => $"blob {c.Key.Value:x32} absent",
        conflict:       static c => $"blob {c.Key.Value:x32} {c.Condition}",
        aborted:        static c => $"blob {c.Key.Value:x32} aborted@{c.Parts}: {c.Reason}",
        transport:      static c => $"{c.Provider} {c.Verb.Key} {c.Key.Value:x32} {c.Status}:{c.Code}",
        integrityBreach: static c => $"blob {c.Key.Value:x32} {c.Provider} checksum mismatch",
        locked:         static c => $"blob {c.Key.Value:x32} WORM {c.Mode}",
        denied:         static c => $"blob {c.Key.Value:x32} {c.Provider} denied: {c.Code}",
        oversize:       static c => $"blob {c.Key.Value:x32} {c.Provider} oversize: {c.Code}",
        grantExpired:   static c => $"blob {c.Key.Value:x32} grant expired",
        invalidRange:   static c => $"blob {c.Key.Value:x32} range {c.Start}-{c.End}/{c.Length}",
        frozen:         static c => $"blob {c.Key.Value:x32} {c.Provider} {c.Verb.Key} frozen");

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
        invalidRange:   static _ => "InvalidRange",
        frozen:         static _ => "Frozen");

    public static RemoteStoreFault Create(string message) => new Text(message);
}

// --- [SERVICES] ---------------------------------------------------------------------------

// Neutral attempt verdict a process-seam store band publishes to whatever executor the composition root bound. Its
// three arms are that executor's OWN vocabulary read back in this package's terms — one delivered attempt, one
// re-offerable fault, one deterministic refusal — because `Rasm.Persistence` references `{Rasm, Rasm.Element}` alone
// and can name no pipeline type. `Of` is the ONE fold both bands reach, where classification meets execution, and it
// takes nothing beyond the discriminant each band already publishes: this tier still executes no retry, it states its
// refusal in whichever currency its executor reads. A band-local fold, or a `bool` handed over in place of its fault,
// is the deleted form — an executor's refusal arm must carry the typed error its caller rails on.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreVerdict {
    private StoreVerdict() { }
    public sealed record Delivered : StoreVerdict;
    public sealed record Faulted(Error Reason) : StoreVerdict;
    public sealed record Refused(Error Reason) : StoreVerdict;

    public static StoreVerdict Of<T>(Fin<T> outcome) => outcome.Match(
        Succ: static _ => (StoreVerdict)new Delivered(),
        Fail: static error => error is IStoreRetriable { IsTransient: true } ? new Faulted(error) : new Refused(error));
}

// Re-drive PORT the composition root binds (`libs/.planning/ARCHITECTURE.md` `[10]-[CONSUMPTION_MODEL]`). ONE shape
// serves both process-seam store bands: `hop` plus `instance` is operation identity the root resolves to its own
// pipeline row (an object plane's provider, a wide-column keyspace), `attempt` is that effect re-offered per pass,
// and `verdict` is each band's own fold its executor's predicate reads. `Carry` is a generic METHOD rather than a
// delegate column, so the port is not a record of delegates.
public interface StoreRedrivePort {
    IO<T> Carry<T>(ObjectVerb hop, string instance, IO<T> attempt, Func<Fin<T>, StoreVerdict> verdict);
}

// Refused-capability degrade an UNBOUND port takes: one pass, its typed refusal intact on the rail. A null port
// reaching a leg is the deleted form — this row binds wherever the root composes no pipeline, so an unbound
// capability reads as a single attempt rather than a crash, and never as a silent success no caller can tell from a
// re-driven one.
public sealed class UnboundRedrive : StoreRedrivePort {
    public static readonly StoreRedrivePort Instance = new UnboundRedrive();
    private UnboundRedrive() { }
    public IO<T> Carry<T>(ObjectVerb hop, string instance, IO<T> attempt, Func<Fin<T>, StoreVerdict> verdict) => attempt;
}

// Where a provider ENFORCES a write stance. `Request` seats it on the initiate or put the leg already composes,
// `Followup` on a per-object call the leg makes AFTER the seal (the object must exist before its policy binds),
// `Container` on the container or bucket the host dialed, `None` on nothing at all. `Holds` is what separates a
// stance a row can enforce from a column it can only claim: `Container` is honored where the host dialed it and
// unprovable from a leg, so it satisfies `Admits` while a `Followup` row proves its own application. No provider row
// seats `Container` now — every provider that can hold a lock proves it per object — and the rung stays because it is
// the seat a host-dialed-only provider takes, not a rung the roster outgrew.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WormSeat {
    public static readonly WormSeat Request = new("request", holds: true);
    public static readonly WormSeat Followup = new("followup", holds: true);
    public static readonly WormSeat Container = new("container", holds: true);
    public static readonly WormSeat None = new("none", holds: false);
    public bool Holds { get; }
    private WormSeat(string key, bool holds) : this(key) => Holds = holds;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TierSeat {
    public static readonly TierSeat Request = new("request");
    public static readonly TierSeat None = new("none");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectStore {
    // CHECKSUM HONESTY: only `S3Leg.Seal` can supply the XxHash128 digest server-side (`Wire` on `ChecksumXXHASH128`),
    // so ONLY the S3 row declares `XxHash128`; Azure reads its SDK-native `Crc64`, GCS its SDK-native `Crc32c` (the GCS
    // whole-object checksum is CRC32C — the SDK exposes no CRC64), and Minio `None` (the SDK's own transport check) —
    // a row declaring a stance no leg supplies is the decorative form this table deletes. Even the S3 row supplies it
    // only where the residence form leaves stored bytes equal to plaintext, so the declaration is the row's ceiling and
    // `ProvesIdentity` is what decides per write. The Presigned row: upstream `FileMeta` carries NO
    // checksum/etag → `ObjectChecksum.None`; no multipart/resume upstream → the unreachable part floor makes
    // `Parts` yield ONE window (single-shot by construction) and `conditionalWrite: false` (no precondition
    // member on a form POST — the content-address invariant is the write-once law, the Minio precedent).
    public static readonly ObjectStore S3 = new("s3", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 10_000, ChunkPolicy.Artifact, true, ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: WormSeat.Request, tierAt: TierSeat.Request, eraseBatch: 1_000,
        degrade: "a DeepArchive-rung object needs a restore this row DOES compose, but the thaw itself takes hours and a fetch inside that window refuses Frozen rather than blocking; the tier transition is metadata-only, so an object under an active object-lock retention cannot be re-written in place and its demotion refuses Locked");
    public static readonly ObjectStore AzureBlob = new("azure-blob", 8L * 1024 * 1024, 4000L * 1024 * 1024, 50_000, ChunkPolicy.Artifact, true, ObjectChecksum.Crc64, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: WormSeat.Followup, tierAt: TierSeat.Request, eraseBatch: 256,
        degrade: "the immutability policy binds AFTER the seal rather than on it, so a crash between the two leaves the object briefly mutable and the resume re-applies it; the batch page ceiling is a SERVICE limit the SDK enforces nowhere, so this row states it and the fold chunks against it");
    public static readonly ObjectStore Gcs = new("gcs", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 1, ChunkPolicy.Artifact, true, ObjectChecksum.Crc32c, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: WormSeat.Followup, tierAt: TierSeat.Request, eraseBatch: 1,
        degrade: "no batch delete exists on the client at all, so the erase page degrades to the per-object verb by declaring a page of one; SSE-C rides client construction, so the per-request arm carries the KMS key id alone");
    public static readonly ObjectStore Minio = new("minio", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 10_000, ChunkPolicy.Artifact, true, ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: WormSeat.Request, tierAt: TierSeat.None, eraseBatch: 1_000,
        degrade: "the copy builder's storage-class setter is internal and no tier vocabulary exists on the client, so the transition is a declared no-op and a cold rung buys nothing; whole-object transfer with no read-checksum setter anywhere in the args algebra, so integrity on the way back is the SDK transport check alone");
    public static readonly ObjectStore Presigned = new("presigned", long.MaxValue, long.MaxValue, 1, ChunkPolicy.Artifact, false, ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: WormSeat.None, tierAt: TierSeat.None, eraseBatch: 1,
        degrade: "the grant plane publishes no object-lock, no storage class, no SSE member, no checksum, no multipart, no batch verb, and no object ceiling: every write-stance column is unenforceable here, so the row's WORM and tier columns must stay Off/Standard and a stance set on this row would be a catalog window no provider holds");

    public long PartSize { get; }
    // The provider's own per-part ceiling and part-count bound — the two halves of the single-object maximum the protocol
    // already fixes, so the ceiling DERIVES rather than standing as a fourth asserted magnitude. `PartCeiling` also
    // bounds one packed window: `Parts` closes a window at the floor and would otherwise grow an unbounded tail on a
    // payload whose content-defined cuts never reach the next boundary. A whole-object provider (`Gcs`, `Presigned`)
    // declares `PartCount: 1`, so its ceiling IS its per-object limit and the packer's one-window shape is a row
    // consequence rather than a leg branch.
    public long PartCeiling { get; }
    public int PartCount { get; }
    public ChunkPolicy Chunking { get; }
    public bool ConditionalWrite { get; }
    public ObjectChecksum Integrity { get; }
    public StorageTier Tier { get; }
    public ObjectEncryption Encryption { get; }
    public ObjectLock Lock { get; }
    // Where each write stance is ENFORCED on this provider, and the honest clause naming what the row gives up.
    // `Worm` is what makes `ObjectLock.Until` truthful: the catalog `WormUntil` and the GC fence read that
    // projection unconditionally, so a row whose provider enforces no lock stamped a window nothing holds and the
    // blob became permanently un-evictable on a fiction. Every row a provider CAN enforce on seats a real rung —
    // `Followup` exists because Azure binds immutability per blob after the seal rather than on it, so that row
    // enforces the column it declares instead of surrendering it to a container dial. `Admits` refuses only the
    // grant plane, which publishes no lock member at any seat.
    // `Tier` names where the storage class lands, so a Demote against a row that seats none is a declared no-op
    // rather than a receipted transition that moved nothing.
    public WormSeat Worm { get; }
    public TierSeat TierAt { get; }
    // How many keys ONE erase round trip carries on this provider. The bound is the provider's own protocol limit and
    // never an SDK-enforced one — the Azure batch client validates no ceiling at all and the service rejects the
    // oversized submission, so the estate STATES its page here exactly as `ObjectCeiling` states the object bound rather
    // than learning either from a provider 4xx. A row with no batch verb declares a page of ONE, which makes the fold
    // degrade to the per-object leg with zero branch, the same trick `PartCount: 1` already plays.
    public int EraseBatch { get; }
    public string Degrade { get; }
    public bool Admits(ObjectLock stance) => stance is ObjectLock.Off || Worm.Holds;
    private ObjectStore(string key, long partSize, long partCeiling, int partCount, ChunkPolicy chunking, bool conditionalWrite, ObjectChecksum integrity, StorageTier tier, ObjectEncryption encryption, ObjectLock @lock, WormSeat worm, TierSeat tierAt, int eraseBatch, string degrade) : this(key) =>
        (PartSize, PartCeiling, PartCount, Chunking, ConditionalWrite, Integrity, Tier, Encryption, Lock, Worm, TierAt, EraseBatch, Degrade) =
            (partSize, partCeiling, partCount, chunking, conditionalWrite, integrity, tier, encryption, @lock, worm, tierAt, eraseBatch, degrade);

    // ONE write-stance fold — SSE first, WORM second, residence form last, an ordering owned HERE so no leg re-spells the
    // nested applies or silently drops a stance member; provider request type discriminates the overload, and a provider
    // binding its lock after the seal stamps the first two by construction. The residence form rides the provider's own
    // user-metadata dictionary, which is the ONE place a stored fact survives every copy, tier change, and lifecycle
    // transition — a sidecar column in the catalog would go stale the first time a provider rule moved the bytes without
    // telling the catalog. TRAP: each SDK owns the `x-amz-meta-` prefixing, and Minio additionally LOWER-CASES any header
    // outside its own supported roster before prefixing it, so the two keys are declared lower-case and every read goes
    // back through the SDK's own stripped view rather than a hand-spelled header name.
    public InitiateMultipartUploadRequest Stamp(InitiateMultipartUploadRequest request, BlobHandle handle, Instant now) =>
        (Form(handle).Iter(pair => request.Metadata[pair.Key] = pair.Value), Lock.ApplyS3(Encryption.ApplyS3(request), now)).Item2;
    public PutObjectArgs Stamp(PutObjectArgs request, BlobHandle handle, Instant now) =>
        Lock.ApplyMinio(Encryption.ApplyMinio(request.WithHeaders(Form(handle).ToDictionary())), now);
    public UploadObjectOptions Stamp(UploadObjectOptions options) => Encryption.ApplyGcs(options);
    public Google.Apis.Storage.v1.Data.Object Stamp(Google.Apis.Storage.v1.Data.Object resource, BlobHandle handle) =>
        (resource.Metadata = Form(handle).ToDictionary(), resource).Item2;
    public CommitBlockListOptions Stamp(CommitBlockListOptions options, BlobHandle handle) =>
        (Form(handle).Iter(pair => options.Metadata[pair.Key] = pair.Value), options).Item2;

    // Residence form as stored metadata: WHICH codec framed the bytes and what plaintext length its directory covers.
    // Both are writer facts a reader cannot derive — a stored length yields neither once a codec or a seal sits between
    // plaintext and provider — so the writer states them and `Head` reads them back.
    static Seq<(string Key, string Value)> Form(BlobHandle handle) => Seq(
        (ObjectCodec.CodecKey, handle.Codec.Key),
        (ObjectCodec.PlainKey, handle.Plain.ToString(CultureInfo.InvariantCulture)));

    // The single-object maximum THIS provider admits, derived from the two protocol bounds the row already carries so the
    // estate STATES its own ceiling instead of learning it from a provider 4xx. `Presigned` saturates because an upstream
    // grant plane publishes no bound; the saturating multiply is the honest spelling of "no bound this row can prove"
    // rather than a wrapped negative.
    public long ObjectCeiling => PartCeiling > long.MaxValue / PartCount ? long.MaxValue : PartCeiling * PartCount;

    // Two integrity claims coincide ONLY where the stored bytes ARE the plaintext bytes. A codec frame or a client
    // seal sits between them, so the provider's whole-object digest then covers bytes the content key never described —
    // supplying the content key AS that digest would make the provider reject a correct upload, and reading a passing
    // provider digest as proof of identity would assert a claim nobody made. Both entries therefore gate on the WHOLE
    // residence form rather than the checksum row alone, and a non-coinciding form falls back to the SDK's own transfer
    // integrity for transport while the domain-side fold in `Decode` keeps the identity claim.
    public bool Passthrough(ObjectCodec codec) => codec == ObjectCodec.Identity && Encryption is not ObjectEncryption.ClientSealed;
    public bool ProvesIdentity(ObjectCodec codec) => Integrity.Identity && Passthrough(codec);
    public ChecksumAlgorithm? WireAlgorithm(BlobHandle handle) => ProvesIdentity(handle.Codec) ? Integrity.S3Algorithm : null;
    public Option<string> Wire(BlobHandle handle) => ProvesIdentity(handle.Codec) ? Integrity.Wire(handle.Key) : None;

    // Residence transform, ONE owner over both directions and ONE fixed ordering: the content key addresses the
    // PLAINTEXT, the codec frames it, the seal wraps the codec output. Sealing before framing would hand the codec
    // ciphertext and gain nothing, and keying either stage's output would fork one payload's address per row.
    public IO<(ReadOnlySequence<byte> Bytes, Option<WrappedKey> Dek)> Encode(ObjectCodec codec, ContentAddress key, ReadOnlySequence<byte> plain) =>
        codec.Pack(Chunking, plain).Bind(packed => Encryption.SealSource(key, Chunking, packed));

    // The inverse, resolving a PLAINTEXT window through both stages. The seal's own arithmetic maps a codec-output window
    // onto stored bytes; the codec's directory maps the caller's window onto codec output. A framed object therefore
    // costs one bounded directory read plus one payload read — never the whole blob — so the one-hop partial fetch the
    // placement bundle exists for survives compression exactly as it survived the seal.
    public IO<Stream> Decode(ObjectClient client, BlobHandle handle, Option<(long Start, long End)> range, Func<ContentAddress, IO<Option<WrappedKey>>> envelope) {
        IO<Stream> Opened(Option<(long Start, long End)> window) =>
            Encryption.Read(handle.Key, Chunking, window, envelope, resident => Head(client, handle), inner => Fetch(client, handle, inner));
        return handle.Codec == ObjectCodec.Identity
            ? Opened(range)
            : from present in Head(client, handle)
              from resident in present.Match(Some: IO.pure, None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(handle.Key)))
              let frame = ObjectCodec.CodecFrame.Of(Chunking, resident.Plain)
              let window = range.IfNone((Start: 0L, End: resident.Plain - 1))
              from bounded in window is { Start: >= 0 } && window.End >= window.Start && window.End < resident.Plain
                  ? IO.pure(unit)
                  : IO.fail<Unit>(new RemoteStoreFault.InvalidRange(handle.Key, window.Start, window.End, resident.Plain))
              from head in Opened(Some((0L, frame.Directory - 1)))
              from directory in ObjectIo.Drain(head, static run => IO.pure(run.ToArray()))
              let span = frame.Window(directory, window.Start, window.End)
              from body in Opened(Some((span.Start, span.End)))
              from plain in ObjectIo.Drain(body, run => handle.Codec.Unpack(Chunking, resident.Plain, directory, window.Start / frame.Stride, run))
              from proven in Proven(handle, range, plain.Slice(checked((int)span.Skip), checked((int)(window.End - window.Start + 1))))
              select proven.AsStream();
    }

    // IDENTITY claim, separate from the transport claim the fetch already armed at the request. A WHOLE-object read
    // folds the recovered plaintext through the one kernel streaming entry and refuses where it does not equal the key the
    // caller addressed — the read-side twin of the pre-seal verify. A RANGED read makes NO identity claim: a window is
    // not the object, and a digest over part of it would be a measurement nobody took, so the window returns on the
    // transport claim alone and the row states that rather than manufacturing a partial address.
    static IO<ReadOnlyMemory<byte>> Proven(BlobHandle handle, Option<(long Start, long End)> range, ReadOnlyMemory<byte> plain) =>
        range.IsSome || ContentAddress.Of(ContentHash.Of(plain, static (bytes, hash) => hash.Append(bytes.Span))) == handle.Key
            ? IO.pure(plain)
            : IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(handle.Key, "content-key"));

    // `now` is the ONE frame instant `Upload` sampled — every WORM arm and the catalog `WormUntil` derive from it, never an
    // ambient clock read. Every leg-facing entry takes the PROJECTED `BlobHandle` the dispatch layer minted, never a bare
    // content address: the class-leading name, the residence form, and the plaintext length resolve once from the
    // injected `ObjectClient.Tenant` and the admitted row, so no entry re-derives a prefix or a stored form. The
    // egress-tier override is GONE with the re-PUT that needed it — a tier change is `Transition`, so the write path
    // carries one tier, the row's own, and a second tier parameter would be a knob nothing reads.
    public IO<BlobResidence> Put(ObjectClient client, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now) =>
        (ObjectIo.For(client).Multipart(this, Tier, handle, residence, manifest, source, sink, now)
            | @catch<IO, BlobResidence>(static e => e is RemoteStoreFault.Conflict, _ => Head(client, handle)
                .Bind(present => present.Match(
                    Some: existing => sink(new BlobTransferFact(Key, "conflict-noop", handle.Key, 0L, 0, None)).Map(_ => existing with { Parts = 0 }),
                    None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(handle.Key)))))).As();

    public IO<Stream> Fetch(ObjectClient client, BlobHandle handle, Option<(long Start, long End)> range) =>
        ObjectIo.For(client).Fetch(this, handle, range);
    public IO<Option<BlobResidence>> Head(ObjectClient client, BlobHandle handle) => ObjectIo.For(client).Head(this, handle);
    public IO<Unit> Delete(ObjectClient client, BlobHandle handle) => ObjectIo.For(client).Erase(handle);
    public IO<Seq<ContentAddress>> List(ObjectClient client) => ObjectIo.For(client).Enumerate();
    public IO<Unit> Abandon(ObjectClient client, BlobHandle handle, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        ObjectIo.For(client).Abandon(this, handle, session, sink);

    // METADATA-ONLY storage-class move: a tier change rewrites a header, never bytes, so the ladder is one leg slot
    // and the fetch-drain-re-PUT that moved a whole payload to change a class is the deleted form. A row seating no tier
    // fills the slot with a declared no-op, so the ladder's reach is a row consequence and never a leg branch.
    public IO<Unit> Transition(ObjectClient client, BlobHandle handle, StorageTier tier, Instant now) =>
        ObjectIo.For(client).Transition(this, handle, tier, now);

    // The thaw axis: `Rehydrate` requests a restore and reports the state either way, so a caller that finds a rung
    // already thawing pays no second request and one that finds it resident pays none at all. `window` is how long the
    // readable copy must persist — an admitted request exactly as `GrantDemand.Lifetime` is, never a deadline knob
    // travelling beside the verb, because only the caller knows how long it needs the bytes.
    public IO<ThawState> Rehydrate(ObjectClient client, BlobHandle handle, Duration window) =>
        ObjectIo.For(client).Rehydrate(this, handle, window);

    // Page-at-a-time erase: the group chunks against the ROW's own `EraseBatch` here, so no leg re-derives a page
    // bound and a provider with no batch verb reaches its per-object leg through a page of one. Pages sequence with
    // `TraverseM` because a transport failure kills the sweep, while per-key refusals INSIDE a page accumulate onto the
    // tally — the two failure grains are different questions and one operator cannot answer both.
    public IO<EraseTally> EraseMany(ObjectClient client, Seq<BlobHandle> handles) =>
        toSeq(handles.Chunk(EraseBatch)).TraverseM(page => ObjectIo.For(client).EraseMany(toSeq(page))).As()
            .Map(static pages => pages.Fold(EraseTally.Empty, static (tally, page) => tally + page));

    // The ISSUER inverse of the presigned CONSUMER row: mint a time-boxed grant against THIS store's own residence so a
    // credential-free reader (the AppUi `SnapshotAccelerator` viewer, a field tablet) streams bytes directly from the
    // provider — the caller gates the demand through `Element/authority#AUTHORITY` `Admit` BEFORE minting, the TTL boxes the
    // exposure, and the expiry anchors on the injected frame instant ([A.1] — never an ambient clock). One entry over
    // every row via the leg's `Issue`, the demand naming the content key and THIS entry projecting the class-leading
    // handle the presign signs, so a grant and a transfer address one object name by construction and no caller composes a
    // wire name to demand a grant for it.
    public IO<ObjectGrant> Grant(ObjectClient client, RetentionClass cls, GrantDemand demand, ProjectionContext frame) =>
        ObjectIo.For(client).Issue(demand, BlobName.Handle(demand.Request.Addressed, client.Tenant, cls, ObjectCodec.Identity, 0L), frame.Now());

    // The Persistence-local placement-delegate bundle the app composes the seam `Graph/element#NODE_MODEL`
    // `GeometrySource` resolver over: `Get(key, range)` is the range-capable one-hop fetch a `Rasm.Compute` runner pulls an
    // analytical `Axis`/`FootPrint` slice through (a mesh LOD or a BREP byte-window, never the whole blob), `Put`
    // content-addresses and write-once-seals THROUGH `MultipartTransfer.Upload` — the ONE composed receipt path, so every
    // placement write yields a `BlobTransferReceipt` on the fact stream carrying the frame's correlation (a `store.Put`
    // with a no-op sink beside the receipt engine was the orphaned-surface V10 defect this routing deletes) — `Thaw`
    // drives the cold rung back to readable, `Sweep` erases a page at a time, and `Stat`/`Delete`/`List` close the
    // lifecycle. ONE record over the legs — a `GeometrySource` over a phantom node field, or a parallel `GetRange`
    // sibling, is the deleted form. The bundle is CLASS-AND-FORM-SCOPED because the object name LEADS with its retention
    // class and the write's stored form is a per-payload-family admission, not a per-call argument: `Named` is the one
    // mint every delegate shares, an app reaching two classes or two codecs composes two bundles, and a bundle spanning
    // either could not name or decode its own objects.
    public BlobRemote Placement(ObjectClient client, RetentionClass cls, ObjectCodec codec, Func<ContentAddress, IO<Option<WrappedKey>>> envelope, ProjectionContext frame, Func<BlobTransferFact, IO<Unit>> sink) {
        BlobHandle Named(ContentAddress key, long plain) => BlobName.Handle(key, client.Tenant, cls, codec, plain);
        // A read names the object from the key alone and learns its stored form from the object itself, so the WRITER's
        // declaration decides the decode and a reader can never name a form the writer never used.
        IO<BlobHandle> Resolved(ContentAddress key) => Head(client, Named(key, 0L))
            .Bind(present => present.Match(Some: r => IO.pure(Named(key, r.Plain) with { Codec = r.Codec }), None: () => IO.fail<BlobHandle>(new RemoteStoreFault.NotFound(key))));
        return new(
            Put: (key, length, stream, session) => ObjectIo.Drain(stream, source =>
                from formed in Encode(codec, key, source)
                from receipt in MultipartTransfer.Upload(this, client, Named(key, length), BlobResidence.From(key, formed.Bytes.Length, length, Tier, codec) with { ConditionToken = session }, ContentChunker.Chunk(Chunking, formed.Bytes), formed.Bytes, sink, frame)
                select receipt.Key),
            Get: (key, range) => Resolved(key).Bind(handle => Decode(client, handle, range, envelope)),
            Stat: key => Head(client, Named(key, 0L)),
            Thaw: (key, window) => Rehydrate(client, Named(key, 0L), window),
            Delete: key => Delete(client, Named(key, 0L)),
            Sweep: keys => EraseMany(client, keys.Map(key => Named(key, 0L))),
            List: () => List(client),
            Abandon: (key, session) => Abandon(client, Named(key, 0L), session, sink),
            Issue: demand => Grant(client, cls, demand, frame));
    }
}

// The placement-delegate bundle the app wires the seam `Graph/element#NODE_MODEL` `GeometrySource` decoder over
// (`Get(repHash.Axis.Value, None)` -> decode -> `AxisCurve`); the range modality rides `Get`, never a `GetRange` sibling,
// so a partial analytical fetch and a whole-blob fetch are one entry discriminating on the `Option` range; `Thaw` is the
// cold-rung inverse of a `Frozen` refusal, carrying the window the caller needs the bytes readable for; `Sweep` is the
// plural-reducing arity of `Delete` over one typed tally rather than a batch flag beside it; `Issue` mints the
// credential-free viewer grant (Authority-gated at the caller, TTL-boxed).
public readonly record struct BlobRemote(
    Func<ContentAddress, long, Stream, Option<string>, IO<ContentAddress>> Put,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Get,
    Func<ContentAddress, IO<Option<BlobResidence>>> Stat,
    Func<ContentAddress, Duration, IO<ThawState>> Thaw,
    Func<ContentAddress, IO<Unit>> Delete,
    Func<Seq<ContentAddress>, IO<EraseTally>> Sweep,
    Func<IO<Seq<ContentAddress>>> List,
    Func<ContentAddress, string, IO<Unit>> Abandon,
    Func<GrantDemand, IO<ObjectGrant>> Issue);
```

| [INDEX] | [POLICY]         | [VALUE]                                      | [BINDING]                                                              |
| :-----: | :--------------- | :------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | content-key name | `{class}/{tenant}/{key:x32}` via `BlobName`  | one `BlobHandle` mint at dispatch; never a second identity             |
|  [02]   | per-leg dispatch | `ObjectClient.Map`                           | union case IS the dispatch; no mismatch guard                          |
|  [03]   | write-once seal  | provider conditional-write `412`-noop        | no read-before-write; the seal is the concurrency primitive            |
|  [04]   | integrity        | `ChecksumAlgorithm.XXHASH128` + `Wire`       | the content key IS the whole-object checksum; never re-hashed          |
|  [05]   | WORM/object-lock | `ObjectLock` SET on write or `Retain`        | `Governance`/`Compliance` immutable; `LegalHold` indefinite            |
|  [06]   | fault rail       | one `RemoteStoreFault.Lift` per edge         | `IStoreRetriable` classifies; `StoreRedrivePort` executes at the root  |
|  [07]   | checksum honesty | per-row SDK-native stance                    | S3 `XxHash128`; Azure `Crc64`; GCS `Crc32c`; Minio/Presigned `None`    |
|  [08]   | presigned grants | `GrantMinter` → `ObjectGrant` per op         | minter-attested `GrantExpired`; bare `403` is `Denied`                 |
|  [09]   | receipt path     | every write via `MultipartTransfer.Upload`   | `BlobTransferReceipt` carries the frame correlation                    |
|  [10]   | issuer grants    | `Grant` via the leg `Issue` per row          | TTL-boxed, `Admit`-gated, frame-instant expiry; viewer streams direct  |
|  [11]   | client seal      | `ClientSealed` + `SealSource`/`OpenSource`   | AES-GCM under an envelope DEK; `WrappedKey` on the catalog row         |
|  [12]   | one WORM clock   | `Upload` samples `frame.Now()` once          | provider retention date and catalog `WormUntil` derive from it         |
|  [13]   | object ceiling   | `PartCeiling * PartCount` per row            | domain-side `Oversize` at admission; never learned from a provider 4xx |
|  [14]   | transfer window  | `ReadOnlySequence<byte>` end to end          | no `int` length on the write path; no truncating narrowing             |
|  [15]   | framed seal      | one AES-GCM frame per `ChunkPolicy.Max`      | a ranged sealed read opens the frames it spans, never the whole blob   |
|  [16]   | stance seat      | `WormSeat` / `TierSeat` per row              | every rung but `None` enforces; only the grant plane holds none        |
|  [17]   | honest degrade   | one `Degrade` clause per provider row        | what the row gives up, stated where a reader selects it                |
|  [18]   | residence form   | `ObjectCodec` row, then the seal             | the key covers plaintext; the form rides object metadata               |
|  [19]   | integrity claims | transport column beside `Identity` column    | `ProvesIdentity` names the one form where the two coincide             |
|  [20]   | tier ladder      | `Transition` metadata rewrite                | bytes never move for a storage class; the re-PUT is deleted            |
|  [21]   | cold rung        | `ThawState` + `Frozen` on the provider code  | thaw is a verb, not a refusal; no probing `Head` per read              |
|  [22]   | erase page       | `EraseBatch` per row, `EraseTally` per page  | accepted and refused are two columns; a page of one degrades           |
|  [23]   | one presigner    | `GrantSigner` keyed by dialed endpoint       | every `GrantRequest` case reaches a verb; no per-provider signer       |
|  [24]   | verb identity    | `ObjectVerb` on `Bound`/`Transport`/`Frozen` | one code reads per verb; a re-drive names what it re-drives            |
|  [25]   | re-drive seam    | `StoreVerdict.Of` + `StoreRedrivePort`       | root binds the executor; unbound is one pass, refusal intact           |

## [03]-[MULTIPART_TRANSFER]

- Owner: `ObjectIo` the one generic transfer engine — a per-provider `ObjectLeg` delegate row (initiate, stage, seal, retain, abort, list-committed, transition, rehydrate, plus fetch/head/erase/erase-many/enumerate/issue) the five providers each fill once, over which a single packing fold packs the manifest's content-defined chunks into provider parts and seals; `ObjectLeg` the closed fourteen-delegate carrier `ObjectClient.Map` resolves; `MultipartTransfer` the receipt-emitting `Upload` (the composed receipt path every write op routes through) plus the `Parts` packer; `TransferPart`/`CommittedPart`/`PartCursor` the part-packing shapes; `BlobName` the Persistence-local class-leading name projection minting the form-bearing `BlobHandle` every leg names through; `BlobTransferReceipt` the per-object evidence carrying the frame correlation.
- Entry: `Upload` is the receipt-emitting write every op composes — the frame supplies mark/elapsed/now and stamps `Correlation` onto residence and receipt, and the row's derived `ObjectCeiling` admits the payload BEFORE the first part stages; `Multipart` runs the one bracket-scoped packing fold over the resolved leg at the row tier and proves the staged windows address the key they seal under wherever the residence form leaves them able to; `Drain` stages a fetch stream into a pooled `ArrayPoolBufferWriter<byte>`; `Parts` packs the chunks into windows clearing the provider floor and bounded by its per-part ceiling; `Formed` reads the stored residence form back out of whichever metadata dictionary its provider owns.
- Law: the transfer window is `ReadOnlySequence<byte>` on every write surface, because a payload past `int` range reaches no `ReadOnlySpan<byte>` at all — the kernel rules the one-shot span unrepresentable there rather than merely slow, so an `int`-shaped length or offset anywhere on this path is the byte ceiling wearing a field type and the `(int)` narrowings that produced a negative offset and a mis-diagnosed part-count abort delete with it.
- Auto: `Parts` accumulates content-defined chunks into part windows each closing once it clears the `PartSize` floor, so a part spans whole `#CONTENT_CHUNKING` chunks (never a sub-chunk slice tearing a chunk across a boundary) at the smallest legal part count; `Multipart` reads the prior committed set through `leg.ListCommitted` (S3 `ListParts`, Azure uncommitted block ids) so an interrupted transfer SKIPS windows already committed in the same session — orthogonal to whole-manifest dedup: one resumes a torn upload, the other skips a resident object — then `TraverseM`-folds the residual windows, counting resumed-versus-fresh into the residence; a fault or cancel folds to `RemoteStoreFault.Aborted` and LEAVES the staged parts in place — the durable session token rides `PendingWrite.Session`, so a re-drive resumes the committed windows instead of restarting, and `Abandon` is the one explicit reap — an auto-abort release deletes the parts resume exists to keep; `Drain` rents a pooled `ArrayPoolBufferWriter<byte>` so the fetch-to-bytes hop never allocates a throwaway array.
- Receipt: `BlobTransferFact` rides `store.blob.part` per uploaded part, `store.blob.resume` per skipped-committed window, `store.blob.abort` per aborted ceremony; `BlobResidence` carries the realized part/resumed-part/skipped-chunk counts the `Upload` receipt reads.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Azure.Storage.Blobs.Batch, Google.Cloud.Storage.V1, Minio (`Minio.Exceptions` the fourth provider's lifted fault family and its batch refusal row alike), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox (`HttpClient`/`MultipartFormDataContent`/`ReadOnlyMemoryContent` for the presigned leg's granted HTTP; the seed minter's SDK lives in the app-root closure).
- Growth: one part-floor, per-part-ceiling, part-count, and erase-page quadruple per provider row (the object ceiling DERIVES from the middle pair, so a raised provider limit is one row value and never an asserted magnitude beside it), or one `ChunkPolicy` row for a tighter window; a sixth provider fills one `ObjectLeg` row in `For` — its `Retain`, `Transition`, and `Rehydrate` slots the declared no-ops wherever its seats hold nothing — and contributes its exception family to the `Lift` fold (the presigned fifth exercised it: one leg, one status-map `Granted`, one in-band fault); a second chunker, a re-declared frame width, a hand-written object-size or page literal beside the row columns, a per-provider multipart or read/head/delete/list body, a second HTTP uploader, a per-leg page-chunking loop, or a per-provider abort catch is the deleted form because the content-defined window IS the `Element/codec#CONTENT_CHUNKING` fold, the fourteen-delegate leg row IS the per-provider variance, and the one `Aborted` fold owns every interruption.
- Boundary: the object name is `{class}/{tenant}/{key:x32}` and projects ONCE at the dispatch layer, so every leg slot that names takes the resolved `BlobHandle` and a leg composing a prefix is unrepresentable; the class segment LEADS so a provider lifecycle rule governs one class across every tenant, and per-tenant listing folds the closed `RetentionClass.Items` roster into one prefixed page per stem rather than scanning a bucket the tenant partition exists to fence. The content-defined chunk boundary, the per-chunk `XxHash128` key, and the whole-blob `XxHash128` identity are owned at `Element/codec#CONTENT_CHUNKING` and consumed here as the `ChunkManifest`, so a re-declared frame width, a second chunker, or a second hash is the deleted form and the server-side checksum is that same digest projected as the provider header. Provider placement deduplicates only at the whole-object seal because no row can synthesize one object from another object's resident chunks; the part floor clears the S3 5 MB minimum as a row value, never a free literal; a torn upload leaves resumable staged parts under its durable session — `Abandon` is the one explicit reap and the provider's incomplete-upload lifecycle rule the backstop; `ConditionalWrite` gates the seal at `complete`, so a concurrent same-key writer resolves to `RemoteStoreFault.Conflict`, the benign no-op write-once placement treats as success.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// `Offset`/`Length` are both `long`: the transfer window is a `ReadOnlySequence<byte>`, so a part addresses a
// payload no `ReadOnlySpan<byte>` spans and the former `int` `Length` (with its unchecked `(int)Bytes` narrowing)
// was the 2 GiB ceiling wearing a field type. The packer bounds a window by the provider's own `PartCeiling`.
public readonly record struct TransferPart(int Number, long Offset, long Length, int Chunks);
public readonly record struct CommittedPart(int Number, string ETag);
// `WormUntil` carries the ONE lock deadline the upload's single sampled instant derived — the catalog column
// commits THIS value, so provider window and catalog window can never diverge (the two-clock split-brain form).
// `Verified` is the WHOLE-OBJECT digest the transfer's own streaming accumulator folded over every staged
// window — `Some` when this session walked the object end to end, `None` on a RESUMED session that skipped
// windows a prior session committed, because an `XxHash128` accumulator carries no serializable state across a
// process boundary and a digest reconstructed from a partial walk would be a measurement nobody took.
public readonly record struct BlobTransferReceipt(string Provider, ContentAddress Key, long Bytes, int Parts, int ResumedParts, bool Aborted, Option<ContentAddress> Verified, Option<Instant> WormUntil, Duration Elapsed, Instant At, CorrelationId Correlation);

// The projected object name paired with the content key it addresses and the STORED FORM that key decodes through — ONE
// value the dispatch layer mints and every leg consumes, so the wire name, the domain identity, and the residence form
// travel together and a leg holding any one without the others cannot exist. Seating the form here is what keeps every
// leg slot's arity fixed while the residence axis grows: a new stored form is a column on the handle, never a parameter
// on fourteen delegates. The name is class-leading (`BlobName`); the key stays the residence, receipt, and fault
// identity; `Plain` is the plaintext length the codec directory covers, zero on a read whose form resolves from the
// object's own metadata instead.
public readonly record struct BlobHandle(ContentAddress Key, string Name, ObjectCodec Codec, long Plain);

// The per-provider leg carrier: each provider fills these FOURTEEN delegates ONCE (the only per-provider variance), so the
// transfer ceremony AND the read/head/delete/list bodies are written ONCE over the leg row, never four parallel
// `S3Multipart`/`AzureBlocks`/... bodies. Every naming slot takes the PROJECTED `BlobHandle` rather than a bare content
// address, so the class-leading name mints once at the dispatch layer and a leg re-spelling a prefix is unrepresentable.
// `Initiate` yields the provider upload token (S3 `UploadId`, Azure a block-id stem, whole-object providers the handle's
// own name), seals the whole-object checksum + the effective `StorageTier` the caller threads (the row `Tier` on a fresh
// write, the colder rung on a `#BLOB_GC` `Demote` re-PUT), and takes the ONE frame instant the WORM arms stamp; `Stage`
// uploads one packed window (a no-op for whole-object providers whose SDK auto-chunks); `Seal` FINALIZES the object
// carrying the `ObjectStore` row and the same instant (so a single-PUT provider stamps the row's SSE + WORM + write-once
// stance at the one finalize point) — for staged providers it commits the part list under the write-once `IfNoneMatch:*`
// precondition (S3) / `ETag.All` (Azure), for whole-object providers it runs the single PUT of the WHOLE `source`: GCS
// under the genuine `IfGenerationMatch=0` create-if-absent precondition, Minio as an idempotent content-addressed create
// under `PutObjectArgs.WithNotMatchETag("*")`; a racing writer therefore reaches the same typed `412` conflict as every
// cloud row — so the one engine drives both transfer models through ONE seal without a mode knob; `Abort` is the bracket
// release; `Committed` lists the prior session's windows for resume; `Fetch`/`Head`/`Erase`/`Enumerate` are the
// read/lifecycle legs the placement dispatches, `Head` reading the provider's REALIZED storage class back through
// `StorageTier.Observed` and `Enumerate` alone keeping its `ContentAddress` yield — it folds the CLOSED
// `RetentionClass.Items` roster into one prefixed listing per `{class}/{tenant}/` stem and reads each key back through
// `BlobName.OfName`, so a listing partitions by the same two axes the name carries; `Issue` is the issuer-side grant mint
// over the demand plus its projected handle (each credentialed SDK's presign entry; the presigned row forwards to its own
// minter).
public readonly record struct ObjectLeg(
    Func<ObjectStore, StorageTier, BlobHandle, Instant, Option<string>, IO<string>> Initiate,
    Func<string, BlobHandle, TransferPart, ReadOnlySequence<byte>, IO<CommittedPart>> Stage,
    Func<ObjectStore, StorageTier, string, BlobHandle, Seq<CommittedPart>, ReadOnlySequence<byte>, Instant, IO<Unit>> Seal,
    Func<string, BlobHandle, IO<Unit>> Abort,
    Func<string, BlobHandle, IO<Seq<CommittedPart>>> Committed,
    // `Fetch` carries the row so the read-side checksum stance arms at the REQUEST — the transport claim runs before
    // any decoder touches the bytes, which is what lets the residence transform trust what it opens.
    Func<ObjectStore, BlobHandle, Option<(long Start, long End)>, IO<Stream>> Fetch,
    Func<ObjectStore, BlobHandle, IO<Option<BlobResidence>>> Head,
    Func<BlobHandle, IO<Unit>> Erase,
    // The page-at-a-time erase over an ALREADY-CHUNKED page: the row's `EraseBatch` bounded it upstream, so a leg never
    // re-derives a page limit and a row declaring a page of one reaches its per-object verb through this same slot with
    // no branch. Both halves of the outcome return, because a provider that answers only the failures (Minio's request
    // hardcodes the quiet flag) still lets the tally derive its success count from the page it was handed, while one that
    // answers both (S3 under a non-quiet request) proves the derivation rather than replacing it.
    Func<Seq<BlobHandle>, IO<EraseTally>> EraseMany,
    Func<IO<Seq<ContentAddress>>> Enumerate,
    Func<GrantDemand, BlobHandle, Instant, IO<ObjectGrant>> Issue,
    // `Retain` is the `WormSeat.Followup` rung — a per-object lock apply a provider binds only once its object
    // exists. Rows seating `Request` or `Container` fill it with a no-op, so the ceremony calls it unconditionally
    // and the stance lands on every provider that can hold one — the seat decides, never a leg branch.
    Func<ObjectStore, BlobHandle, Instant, IO<Unit>> Retain,
    // The METADATA-ONLY storage-class move. Rows seating `TierSeat.None` fill it with a declared no-op, so the ladder's
    // reach is a row consequence; the bytes never move on any row, which is the whole point of the slot.
    Func<ObjectStore, BlobHandle, StorageTier, Instant, IO<Unit>> Transition,
    // The thaw request-and-report. Every row fills it: a provider whose cold rungs are instantly readable answers
    // `Resident` unconditionally, which is a RECORDED negative rather than an unfilled slot, so a caller reads one
    // vocabulary across the axis and no consumer branches on which provider it holds.
    Func<ObjectStore, BlobHandle, Duration, IO<ThawState>> Rehydrate);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The Persistence-local object-name projection of the seam content key, three segments deep and CLASS-LEADING. The
// retention-class segment LEADS because a provider lifecycle rule targets a key PREFIX: one rule per class then governs
// every tenant's objects in that class (`#BLOB_GC` `LifecycleRules`), where a tenant-leading name would demand one rule
// per tenant per class and re-write the whole rule set on every tenant admission. The tenant segment sits INSIDE so two
// tenants never collide on one content key, and its render is the kernel `TenantId.Text` spelling, so the object prefix, the
// catalog row's RLS text, and the meter tag stay byte-identical. Class membership is IMMUTABLE — reclassification is
// export-then-readmit at its retention owner, never a rename — so the name is stable for the object's whole life and a
// lifecycle rule never governs bytes that moved out from under it. `OfName` parses from the LAST separator, so the
// inverse a `List` row reads back is unchanged by the added segment, and `Prefix` is the ONE stem both `Enumerate` and the
// lifecycle rules address under. The name projects ONCE at the dispatch layer from the INJECTED `ObjectClient.Tenant` and the
// admitted row's own class (the [A.1] frame law — an ambient `TenantContext.Current` read here is the named inversion, the
// deleted form) and the legs receive the projected handle, so no leg re-spells a prefix. NOT a seam member (the seam owns the
// `ContentAddress` value, this page owns its provider-name spelling) and Persistence-internal rather than file-scoped
// because `#BLOB_GC` mints through the same entry, so a free-string blob name is the deleted form.
static class BlobName {
    public static BlobHandle Handle(ContentAddress key, TenantId tenant, RetentionClass cls, ObjectCodec codec, long plain) =>
        new(key, $"{Prefix(tenant, cls)}{key.Value:x32}", codec, plain);
    public static string Prefix(TenantId tenant, RetentionClass cls) => $"{cls.Key}/{tenant.Text}/";
    public static ContentAddress OfName(string name) => ContentAddress.Of(UInt128.Parse(name.AsSpan(name.LastIndexOf('/') + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
}

public static class MultipartTransfer {
    // THE composed receipt path: every write op routes here (`Placement.Put`, `BlobGc.WriteBlobFirst`), so the receipt is
    // never an orphaned sibling of a bare `store.Put`; the [A.1] frame supplies mark/elapsed/now AND the correlation the
    // residence + receipt both carry — `BlobResidence.Correlation` threads from the causing op, never a permanent none.
    // `handle` is the projected name the caller minted from its own class and stored form — the ONE naming hop, so this
    // path never re-derives a prefix. A tier transition is NOT a write and never reaches here: it moves a header through
    // `Transition`, so the egress-tier override this signature once carried is gone with the re-PUT.
    public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from now in IO.lift(frame.Now)
        from _ in Admitted(provider, handle.Key, source.Length)
        // Every refusal this plane mints lands on the fact stream before it re-raises, carrying the union's own
        // `Category` as the fact kind — the rail is what EXPLAINS the failure and a rail whose refusals reach no
        // evidence surface leaves the whole fault band unobservable at exactly the edge it exists to name.
        from sealed_ in (provider.Put(client, handle, residence with { Correlation = frame.Correlation }, manifest, source, sink, now)
            | @catch<IO, BlobResidence>(static _ => true, error => sink(new BlobTransferFact(provider.Key,
                error is RemoteStoreFault fault ? fault.Category : "Text", handle.Key, source.Length, 0, residence.ConditionToken))
                    .Bind(_ => IO.fail<BlobResidence>(error)))).As()
        select new BlobTransferReceipt(provider.Key, sealed_.Key, sealed_.Length, sealed_.Parts, sealed_.ResumedParts, Aborted: false, sealed_.Verified, provider.Lock.Until(now), frame.Elapsed(mark), now, frame.Correlation);

    // The DOMAIN-side ceiling admission, and `RemoteStoreFault.Oversize`'s first mint from evidence the estate owns: the
    // row's derived `ObjectCeiling` is the provider's stated single-object maximum, so an over-ceiling payload refuses
    // BEFORE the first part stages rather than after a full transfer earns a provider 4xx whose `Code` names someone
    // else's vocabulary. The `Code` slot carries the estate's own reason here.
    static IO<Unit> Admitted(ObjectStore provider, ContentAddress key, long length) =>
        length <= provider.ObjectCeiling
            ? IO.pure(unit)
            : IO.fail<Unit>(new RemoteStoreFault.Oversize(key, provider.Key, "object-ceiling"));

    // Pack whole content-defined chunks into part windows, each closing once it clears the floor OR would pass the
    // provider's own per-part ceiling — a part spans whole chunks (never a sub-chunk slice), the smallest legal part
    // count resulting, and the open tail seals last. The ceiling arm is what keeps a payload whose content-defined
    // cuts run long from growing one window past a bound the provider rejects mid-transfer.
    public static Seq<TransferPart> Parts(ChunkManifest manifest, ObjectStore provider) {
        (Seq<TransferPart> Done, PartCursor Open) packed = manifest.Chunks.Fold(
            (Done: Seq<TransferPart>(), Open: PartCursor.Empty),
            (acc, chunk) => acc.Open.Grow(chunk).Pack(provider.PartSize, provider.PartCeiling, acc.Done));
        return packed.Open.Chunks > 0 ? packed.Done.Add(packed.Open.Seal(packed.Done.Count + 1)) : packed.Done;
    }
}

file readonly record struct PartCursor(long Start, long Bytes, int Chunks) {
    public static readonly PartCursor Empty = new(0L, 0L, 0);
    public PartCursor Grow(ContentChunk chunk) => this with { Bytes = Bytes + chunk.Length, Chunks = Chunks + 1 };
    public (Seq<TransferPart> Done, PartCursor Open) Pack(long floor, long ceiling, Seq<TransferPart> done) =>
        Bytes >= floor || Bytes >= ceiling ? (done.Add(Seal(done.Count + 1)), new PartCursor(Start + Bytes, 0L, 0)) : (done, this);
    // No narrowing: `TransferPart.Length` is `long`, so a window past `int` range seals as itself rather than
    // wrapping to a negative offset the provider abort then mis-reports as a part-count failure.
    public TransferPart Seal(int number) => new(number, Start, Bytes, Chunks);
}

// The ONE generic transfer engine. `For` resolves the per-provider `ObjectLeg`; `Multipart` runs the single
// bracket-scoped packing fold over the multipart legs; `Drain` stages a fetch stream into pooled storage. The four `*Leg`
// constructors fill the `ObjectLeg` delegates over the typed SDK client — the ONLY per-provider code; every SDK exception
// inside a leg lifts once to `RemoteStoreFault` at the `Bound` boundary, so the engine sees only rails.
public static class ObjectIo {
    // Four NAMED slots rather than one anonymous quartet: `Store/observability#STORE_INSTRUMENTS` keys its
    // projection arms by slot, so an arm that cannot name the slot it folds cannot mount — the roster spread
    // stays the registry's census input while each static is the arm's key, matching every sibling page's shape.
    public static readonly StoreSlot PartSlot = StoreSlot.Create("store.blob.part");
    public static readonly StoreSlot ResumeSlot = StoreSlot.Create("store.blob.resume");
    public static readonly StoreSlot AbortSlot = StoreSlot.Create("store.blob.abort");
    public static readonly StoreSlot WriteSlot = StoreSlot.Create("store.blob.write");
    // The refusal slot: every `RemoteStoreFault` this plane mints was silent, so a `Locked`, an `IntegrityBreach`, an
    // `Oversize`, a `GrantExpired`, and a `Conflict` all read as one absence on every board. The fact's `Kind` carries the
    // union's OWN `Category` projection, so the fault vocabulary is the union's and no second roster drifts.
    public static readonly StoreSlot FaultSlot = StoreSlot.Create("store.blob.fault");
    public static readonly Seq<StoreSlot> Slots = Seq(PartSlot, ResumeSlot, AbortSlot, WriteSlot, FaultSlot);

    public static ObjectLeg For(ObjectClient client) => client.Map(
        s3: static r => S3Leg(r), azure: static r => AzureLeg(r), gcs: static r => GcsLeg(r), minio: static r => MinioLeg(r), presigned: static r => PresignedLeg(r));

    // The one packing fold. `Initiate` acquires (or resumes) the upload token; `Committed` lists the prior session's
    // committed windows (empty on a fresh upload), only unresumed windows upload through `leg.Stage`, and `Seal` commits
    // under the write-once precondition. Staged parts SURVIVE a fault/cancel by design — the session token rides
    // `PendingWrite.Session` durably, so a re-drive resumes the committed windows instead of restarting; `leg.Abort` runs
    // only through the explicit `Abandon` verb (an auto-abort release would delete the very parts resume keeps). Every
    // non-rail error folds to `RemoteStoreFault.Aborted` carrying the window count.
    public static IO<BlobResidence> Multipart(this ObjectLeg leg, ObjectStore provider, StorageTier tier, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now) {
        Seq<TransferPart> windows = MultipartTransfer.Parts(manifest, provider);
        return (leg.Initiate(provider, tier, handle, now, residence.ConditionToken).Bind(token =>
                from _session in sink(new BlobTransferFact(provider.Key, "session", handle.Key, 0L, 0, Some(token)))
                from prior in leg.Committed(token, handle)
                let resumed = prior.Map(static p => p.Number).ToFrozenSet()
                from staged in windows.Filter(w => !resumed.Contains(w.Number)).TraverseM(w =>
                    from committed in leg.Stage(token, handle, w, source.Slice(w.Offset, w.Length))
                    from _ in sink(new BlobTransferFact(provider.Key, "part", handle.Key, w.Length, w.Number, None))
                    select committed).As()
                from _ in prior.TraverseM(p => sink(new BlobTransferFact(provider.Key, "resume", handle.Key, 0L, p.Number, None))).As()
                let verified = Verified(provider, handle, source, windows, prior.Count)
                from _integrity in verified.Match(
                    Some: minted => minted == handle.Key
                        ? IO.pure(unit)
                        : IO.fail<Unit>(new RemoteStoreFault.IntegrityBreach(handle.Key, provider.Key)),
                    None: static () => IO.pure(unit))
                from _ in leg.Seal(provider, tier, token, handle, toSeq((prior + staged).OrderBy(static p => p.Number)), source, now)
                // `Retain` runs HERE, after the seal and inside the same bracket, because a provider binding its
                // lock per object needs that object to exist first. Rows seating `Request` or `Container` fill the
                // slot with a no-op, so the call is unconditional and no leg branches on its own provider; a fault
                // between seal and retain folds `Aborted` like any other, and the re-drive re-applies an
                // idempotent policy against the already-sealed key.
                from _retain in leg.Retain(provider, handle, now)
                select residence with { Length = source.Length, Parts = windows.Count, ResumedParts = prior.Count, Verified = verified, ConditionToken = None })
            | @catch<IO, BlobResidence>(static _ => true, error => IO.fail<BlobResidence>(error is RemoteStoreFault ? error : new RemoteStoreFault.Aborted(handle.Key, windows.Count, error.Message)))).As();
    }

    // The DOMAIN-side whole-object verify, and `RemoteStoreFault.IntegrityBreach`'s first write-path mint: the ONE kernel
    // streaming entry folds the staged windows IN PART ORDER through a seed-zero accumulator, and part order IS the
    // canonical projection that entry's own law fixes, so the digest equals the one-shot key over the same bytes and a
    // transfer that moved anything else refuses BEFORE the seal makes it write-once. `Append(Stream)` takes the segmented
    // window whole, so verification costs no contiguity the payload could not afford. A RESUMED session walked only its
    // own windows, so it yields `None` — a digest folded over a partial walk would read as a whole-object measurement no
    // session took, and only a fabricated one could compare equal. A NON-PASSTHROUGH form yields `None` for the same
    // reason from the other side: the staged windows carry codec frames or sealed frames, so their digest describes the
    // STORED bytes and the key describes the plaintext, and comparing them would refuse every correct framed write.
    // Identity on a framed write rests on the seam contract that minted the key from the same plaintext handed in,
    // re-proved on the way back by the read-side fold.
    static Option<ContentAddress> Verified(ObjectStore provider, BlobHandle handle, ReadOnlySequence<byte> source, Seq<TransferPart> windows, int resumed) =>
        resumed > 0 || !provider.Passthrough(handle.Codec)
            ? None
            : Some(ContentAddress.Of(ContentHash.Of(
                (Source: source, Windows: windows),
                static (state, hash) => state.Windows.Iter(row => hash.Append(state.Source.Slice(row.Offset, row.Length).AsStream())))));

    public static IO<Unit> Abandon(this ObjectLeg leg, ObjectStore provider, BlobHandle handle, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        from _ in leg.Abort(session, handle)
        from _fact in sink(new BlobTransferFact(provider.Key, "abort", handle.Key, 0L, 0, Some(session)))
        select unit;

    // Per-tenant listing folds the CLOSED `RetentionClass.Items` roster into one prefixed listing per `{class}/{tenant}/`
    // stem, because a class-leading name carries no single prefix covering every class and an unprefixed bucket scan
    // would cross the tenant partition the name exists to enforce. The roster is closed, so the request count is a policy
    // constant rather than a growth axis, and a new class costs one more prefixed page. ONE residence-form read over
    // every provider: each SDK owns its own metadata dictionary shape, so the leg supplies the lookup and this fold owns the
    // vocabulary. An object written before the form existed states neither key and falls back to the pass-through row
    // with its plaintext length equal to its stored length — the only pair for which that fallback is true, which is what
    // makes an unstated form safe rather than a guess.
    static BlobResidence Formed(ContentAddress key, long stored, StorageTier tier, Func<string, string?> stated) =>
        BlobResidence.From(key, stored,
            long.TryParse(stated(ObjectCodec.PlainKey), NumberStyles.None, CultureInfo.InvariantCulture, out long plain) ? plain : stored,
            tier, ObjectCodec.Observed(stated(ObjectCodec.CodecKey)));

    static IO<Seq<ContentAddress>> Listed(TenantId tenant, Func<string, IO<Seq<ContentAddress>>> under) =>
        toSeq(RetentionClass.Items).TraverseM(cls => under(BlobName.Prefix(tenant, cls))).As().Map(static pages => pages.Flatten());

    // Drain a fetch stream into pooled storage — `ArrayPoolBufferWriter<byte>` rents from the shared pool, the copy fills
    // it, and `WrittenMemory` reads back with zero throwaway array; the writer disposes back to the pool on bracket exit. The
    // pooling role is the whole of it: the write path's hashing no longer demands a contiguous span, so this hop is the
    // fetch-to-bytes stager alone and hands the one `ReadOnlySequence<byte>` window every transfer surface takes.
    public static IO<T> Drain<T>(Stream source, Func<ReadOnlySequence<byte>, IO<T>> use) =>
        IO.lift(static () => new ArrayPoolBufferWriter<byte>()).Bracket(
            Use: writer => IO.liftAsync(async () => {
                await source.CopyToAsync(writer.AsStream()).ConfigureAwait(false);
                return new ReadOnlySequence<byte>(writer.WrittenMemory);
            }).Bind(use),
            Fin: writer => IO.lift(() => {
                writer.Dispose();
                source.Dispose();
                return unit;
            }));

    // Lift every SDK call once into `RemoteStoreFault` at the leg boundary so the engine interior is total over rails
    // (`docs/stacks/csharp/rails-and-effects#EXCEPTION_CAPTURE`): each provider's status/exception family folds
    // structurally — the `412` precondition is `Conflict`, `404` is `NotFound`, `401`/`403` is `Denied`, `413` is
    // `Oversize`, a no-response connection failure is the only transient `Transport` (status 0), every OTHER
    // status is a typed `Transport`, and an unrecognized exception is a NON-transient `Text` (a generic exception
    // is NEVER retried — the deterministic default). Persistence-internal rather than private because the `#BLOB_GC`
    // `LifecycleRules` arms are SDK calls too and lift at THIS one boundary, never through a second fold. The VERB
    // rides every crossing beside the provider and the key, so the one re-drivable case names the operation a
    // re-drive would re-offer and the cold-rung arms carry the verb that decides whether their code is a refusal.
    internal static IO<T> Bound<T>(string provider, ObjectVerb verb, ContentAddress key, Func<Task<T>> call) =>
        IO.liftAsync(call) | @catch<IO, T>(static _ => true, e => IO.fail<T>(Lift(provider, verb, key, e)));

    static RemoteStoreFault Lift(string provider, ObjectVerb verb, ContentAddress key, Error error) => error switch {
        RemoteStoreFault fault => fault,
        // The cold-rung arms read the provider's OWN error CODE ahead of its status, because both providers raise the
        // frozen refusal at a status the generic arm folds elsewhere — S3 at a 403 the auth arm would claim, Azure at a
        // 409 the transport arm would. Code-before-status is what keeps a rung the caller can thaw from reading as a
        // denial it cannot, and it is the one place on this fold where the discriminant is not the status. The verb
        // rides the case rather than gating the arm: one code carries opposite meanings across two verbs, so the fold
        // states WHICH verb met it and the restore leg reads `Verb.ColdRefuses` off the vocabulary instead of a
        // hand-placed per-leg exception.
        { Exception.Case: AmazonS3Exception { ErrorCode: "InvalidObjectState" } } => new RemoteStoreFault.Frozen(key, provider, verb),
        { Exception.Case: AmazonS3Exception s3 } => s3.StatusCode switch {
            HttpStatusCode.PreconditionFailed => new RemoteStoreFault.Conflict(key, "if-none-match"),
            HttpStatusCode.NotFound => new RemoteStoreFault.NotFound(key),
            HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized => new RemoteStoreFault.Denied(key, provider, s3.ErrorCode),
            HttpStatusCode.RequestEntityTooLarge => new RemoteStoreFault.Oversize(key, provider, s3.ErrorCode),
            _ => new RemoteStoreFault.Transport(provider, verb, key, (int)s3.StatusCode, s3.ErrorCode),
        },
        { Exception.Case: RequestFailedException { ErrorCode: "BlobArchived" } } => new RemoteStoreFault.Frozen(key, provider, verb),
        { Exception.Case: RequestFailedException az } => az.Status switch {
            412 => new RemoteStoreFault.Conflict(key, "if-none-match"),
            404 => new RemoteStoreFault.NotFound(key),
            401 or 403 => new RemoteStoreFault.Denied(key, provider, az.ErrorCode ?? "azure"),
            413 => new RemoteStoreFault.Oversize(key, provider, az.ErrorCode ?? "azure"),
            _ => new RemoteStoreFault.Transport(provider, verb, key, az.Status, az.ErrorCode ?? "azure"),
        },
        { Exception.Case: GoogleApiException gcs } => (int)gcs.HttpStatusCode switch {
            412 => new RemoteStoreFault.Conflict(key, "if-generation-match"),
            404 => new RemoteStoreFault.NotFound(key),
            401 or 403 => new RemoteStoreFault.Denied(key, provider, gcs.Error?.Message ?? "gcs"),
            413 => new RemoteStoreFault.Oversize(key, provider, gcs.Error?.Message ?? "gcs"),
            int status => new RemoteStoreFault.Transport(provider, verb, key, status, gcs.Error?.Message ?? "gcs"),
        },
        { Exception.Case: PreconditionFailedException } => new RemoteStoreFault.Conflict(key, "if-none-match"),
        { Exception.Case: ObjectNotFoundException or BucketNotFoundException } => new RemoteStoreFault.NotFound(key),
        { Exception.Case: (AccessDeniedException or ForbiddenException) and { } denied } => new RemoteStoreFault.Denied(key, provider, denied.GetType().Name),
        { Exception.Case: (EntityTooLargeException or InvalidContentLengthException) and { } oversize } => new RemoteStoreFault.Oversize(key, provider, oversize.GetType().Name),
        { Exception.Case: ConnectionException } => new RemoteStoreFault.Transport(provider, verb, key, 0, "connection"),
        { Exception.Case: { } ex } => new RemoteStoreFault.Text($"{provider}:{ex.GetType().Name}:{ex.Message}"),
        _ => new RemoteStoreFault.Text($"{provider}:{error.Message}"),
    };

    // --- [LEGS] -------------------------------------------------------------------------------
    // S3: low-level multipart over `IAmazonS3` (`api-objectstore` S3_MULTIPART). `Initiate` declares the whole-object
    // `XXHASH128`+`FULL_OBJECT` checksum stance, the storage class, the row's SSE (`ObjectEncryption.ApplyS3`), and the
    // WORM object-lock (`ObjectLock.ApplyS3`); `Seal` SUPPLIES the precomputed `ChecksumXXHASH128` (the content key, via
    // `Wire`) and carries the write-once `IfNoneMatch="*"`; range read is `GetObjectRequest.ByteRange` under the armed
    // `ChecksumMode`.
    static ObjectLeg S3Leg(ObjectClient.S3 r) => new(
        Initiate: (store, tier, key, now, resume) => resume.Match(
            Some: IO.pure,
            None: () => Bound("s3", ObjectVerb.Write, key.Key, () => r.Client.InitiateMultipartUploadAsync(store.Stamp(new InitiateMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, StorageClass = tier.S3Class, ChecksumAlgorithm = store.WireAlgorithm(key), ChecksumType = store.WireAlgorithm(key) is null ? null : ChecksumType.FULL_OBJECT }, key, now))).Map(static x => x.UploadId)),
        Stage: (token, key, part, bytes) => Bound("s3", ObjectVerb.Write, key.Key, () => r.Client.UploadPartAsync(new UploadPartRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token, PartNumber = part.Number, PartSize = part.Length, InputStream = bytes.AsStream() })).Map(x => new CommittedPart(part.Number, x.ETag)),
        // The precomputed whole-object digest rides ONLY where the residence form leaves stored bytes equal to plaintext;
        // `Wire` yields `None` otherwise and the unset member falls back to the SDK's transfer integrity, because
        // supplying the content key over framed bytes would make the provider reject a correct upload.
        Seal: (store, _, token, key, parts, _, _) => Bound("s3", ObjectVerb.Write, key.Key, () => r.Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token, IfNoneMatch = "*", ChecksumXXHASH128 = store.Wire(key).ValueUnsafe(), PartETags = parts.Map(static p => new PartETag(p.Number, p.ETag)).ToList() })).Map(static _ => unit),
        Abort: (token, key) => string.IsNullOrEmpty(token) ? IO.pure(unit) : Bound("s3", ObjectVerb.Write, key.Key, () => r.Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token })).Map(static _ => unit),
        Committed: (token, key) => Bound("s3", ObjectVerb.Write, key.Key, () => r.Client.ListPartsAsync(new ListPartsRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token })).Map(static x => toSeq(x.Parts).Map(static p => new CommittedPart(p.PartNumber, p.ETag))),
        Fetch: (store, key, range) => Bound("s3", ObjectVerb.Read, key.Key, () => r.Client.GetObjectAsync(store.Integrity.ApplyS3(new GetObjectRequest { BucketName = r.Bucket, Key = key.Name, ByteRange = range.Match(Some: static w => new ByteRange(w.Start, w.End), None: static () => null) }))).Map(static x => x.ResponseStream),
        Head: (store, key) => Bound("s3", ObjectVerb.Read, key.Key, () => r.Client.GetObjectMetadataAsync(r.Bucket, key.Name)).Map(x => Optional(Formed(key.Key, x.ContentLength, StorageTier.Observed(x.StorageClass?.Value).IfNone(store.Tier), slot => x.Metadata[slot]))),
        Erase: key => Bound("s3", ObjectVerb.Erase, key.Key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name)).Map(static _ => unit),
        // `DeleteObjectsRequest.Objects` takes `KeyVersion` values, not strings, and `KeyVersion` declares NO
        // constructor, so each element is an object initializer over the projected name. `Quiet` is a NULLABLE bool set
        // FALSE on purpose: the quiet form suppresses the deleted list, and a tally that cannot see its successes could
        // not tell an empty page from a wholly refused one. Both response halves fold — the plural property is
        // `DeleteErrors`, not `DeleteError` — so accepted and refused stay separate columns.
        EraseMany: page => Bound("s3", ObjectVerb.Erase, default, () => r.Client.DeleteObjectsAsync(new DeleteObjectsRequest {
                BucketName = r.Bucket, Quiet = false, Objects = page.Map(static h => new KeyVersion { Key = h.Name }).ToList(),
            })).Map(x => new EraseTally(page.Count, toSeq(x.DeleteErrors).Map(static e => (BlobName.OfName(e.Key), e.Code)))),
        Enumerate: () => Listed(r.Tenant, prefix => Bound("s3", ObjectVerb.List, default, () => r.Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = r.Bucket, Prefix = prefix })).Map(static x => toSeq(x.S3Objects).Map(static o => BlobName.OfName(o.Key)))),
        Issue: (demand, handle, now) => Bound("s3", ObjectVerb.Grant, handle.Key, () => new GrantSigner(r.Client, r.Bucket).Sign(demand, handle, now)),
        Retain: static (_, _, _) => IO.pure(unit),
        // Same key, same bucket, new storage class: a self-copy rewrites the object's header server-side and moves no
        // bytes. `MetadataDirective`/`TaggingDirective` stay COPY so the residence-form metadata and the tag set survive the
        // move — REPLACE would drop the codec declaration and orphan every stored frame. No write-once precondition rides a
        // self-copy: `IfNoneMatch:*` would refuse against the very object being re-classed.
        Transition: (_, key, tier, _) => Bound("s3", ObjectVerb.Transition, key.Key, () => r.Client.CopyObjectAsync(new CopyObjectRequest {
                SourceBucket = r.Bucket, SourceKey = key.Name, DestinationBucket = r.Bucket, DestinationKey = key.Name,
                StorageClass = tier.S3Class, MetadataDirective = S3MetadataDirective.COPY, TaggingDirective = TaggingDirective.COPY,
            })).Map(static _ => unit),
        // Restore is idempotent by protocol, so the state derives from the request's OWN outcome and no extra head runs:
        // acceptance means a thaw is now in flight, a conflict means one already was, and `InvalidObjectState` — the same
        // code a frozen GET raises, read here under the opposite verb — means the rung was never archived. The catch
        // predicate READS that inversion off the vocabulary (`Verb.ColdRefuses` false is the restore verb alone), so the
        // arm is a consequence of the `ObjectVerb` row rather than a per-leg exception a sixth provider would re-spell.
        // `Days` is a whole-day count the protocol floors at one, so a sub-day window states one day rather than a
        // provider-rejected zero. `Standard` and `Bulk` are the only priorities the deep-archive rung admits, and
        // `Standard` is the faster.
        Rehydrate: (_, key, window) => (Bound("s3", ObjectVerb.Restore, key.Key, () => r.Client.RestoreObjectAsync(new RestoreObjectRequest {
                    BucketName = r.Bucket, Key = key.Name, Days = int.Max(1, (int)window.TotalDays), Tier = GlacierJobTier.Standard,
                })).Map(static _ => (ThawState)new ThawState.Thawing(None))
            | @catch<IO, ThawState>(static e => e is RemoteStoreFault.Conflict, static _ => IO.pure<ThawState>(new ThawState.Thawing(None)))
            | @catch<IO, ThawState>(static e => e is RemoteStoreFault.Frozen { Verb.ColdRefuses: false }, static _ => IO.pure<ThawState>(new ThawState.Resident()))).As());

    // Azure: staged-block over `BlockBlobClient` (`api-objectstore` AZURE_BLOCKS). The block id is the part number; `Seal`
    // commits with `IfNoneMatch = ETag.All`; range read is `BlobDownloadOptions.Range = HttpRange`.
    static ObjectLeg AzureLeg(ObjectClient.Azure r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: (token, _, part, bytes) => Bound("azure", ObjectVerb.Write, default, async () => {
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
        // Commit stamps the EFFECTIVE tier's Azure access tier, so a fresh write lands the row's residence and
        // a Demote re-PUT lands the colder rung — the tier ladder is real on this provider, never an S3-only fact.
        Seal: (store, tier, token, key, parts, _, _) => Bound("azure", ObjectVerb.Write, key.Key, () => r.Container.GetBlockBlobClient(token).CommitBlockListAsync(parts.Map(static p => p.ETag).ToList(), store.Stamp(new CommitBlockListOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }, AccessTier = tier.AzureTier }, key))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: (token, key) => Bound("azure", ObjectVerb.Write, key.Key, () => r.Container.GetBlockBlobClient(token).GetBlockListAsync(BlockListTypes.Uncommitted)).Map(static x => toSeq(x.Value.UncommittedBlocks).Map(static b => new CommittedPart(BitConverter.ToInt32(Convert.FromBase64String(b.Name)), b.Name))),
        // `BlobDownloadOptions` carries exactly four members and `TransferValidation` is the read-checksum one; its
        // type ships in `Azure.Storage.Common`, a DIFFERENT assembly from the client's, and the preview-era
        // `DownloadTransactionalHashingOptions` spelling no longer exists.
        Fetch: (store, key, range) => Bound("azure", ObjectVerb.Read, key.Key, () => r.Container.GetBlobClient(key.Name).DownloadStreamingAsync(store.Integrity.ApplyAzure(new BlobDownloadOptions { Range = range.Map(static w => new HttpRange(w.Start, w.End - w.Start + 1)).IfNone(default(HttpRange)) }))).Map(static x => x.Value.Content),
        // TRAP: the LISTING surface types these facts and the PROPERTIES surface stringifies them —
        // `BlobItemProperties.AccessTier`/`ArchiveStatus`/`RehydratePriority` are strongly typed nullables while
        // `BlobProperties`'s three are get-only STRINGS. A head therefore reads strings and folds them through the same
        // `Observed` entry every other provider's stated class goes through, which is exactly why that entry takes a
        // string rather than a per-provider enum three parsers would drift apart.
        Head: (store, key) => Bound("azure", ObjectVerb.Read, key.Key, () => r.Container.GetBlobClient(key.Name).GetPropertiesAsync()).Map(x => Optional(Formed(key.Key, x.Value.ContentLength, StorageTier.Observed(x.Value.AccessTier).IfNone(store.Tier), slot => x.Value.Metadata.TryGetValue(slot, out string? stated) ? stated : null))),
        Erase: key => Bound("azure", ObjectVerb.Erase, key.Key, () => r.Container.GetBlobClient(key.Name).DeleteIfExistsAsync()).Map(static _ => unit),
        // The blob batch ships in a SEPARATE package from the client, reached through the `SpecializedBlobExtensions`
        // extension on the container. TRAP: the convenience verbs `DeleteBlobs`/`DeleteBlobsAsync` submit with
        // `throwOnAnyFailure: true` internally and raise an `AggregateException` on ANY sub-failure, so a typed partial
        // tally is unreachable through them — the batch is built by hand and submitted with the flag FALSE, then each
        // delayed sub-`Response` is read for its own status. A batch admits ONE operation type and refuses a mixed set,
        // refuses an empty set, and refuses resubmission, so the page is materialized before submit and never reused; the
        // SDK validates no page ceiling at all, which is why the row states one.
        EraseMany: page => Bound("azure", ObjectVerb.Erase, default, async () => {
            BlobBatchClient batches = r.Container.GetBlobBatchClient();
            using BlobBatch batch = batches.CreateBatch();
            Seq<(BlobHandle Handle, Response Delayed)> issued = page.Map(handle => (handle, batch.DeleteBlob(r.Container.Name, handle.Name)));
            _ = await batches.SubmitBatchAsync(batch, throwOnAnyFailure: false).ConfigureAwait(false);
            return new EraseTally(page.Count, issued.Filter(static row => row.Delayed.Status >= 400)
                .Map(static row => (row.Handle.Key, row.Delayed.Status.ToString(CultureInfo.InvariantCulture))));
        }),
        Enumerate: () => Listed(r.Tenant, prefix => Bound("azure", ObjectVerb.List, default, async () => {
            List<ContentAddress> keys = [];
            await foreach (BlobItem blob in r.Container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None).ConfigureAwait(false))
                keys.Add(BlobName.OfName(blob.Name));
            return toSeq(keys);
        })),
        // SAS issuance needs the container dialed with a shared-key credential (an AAD-dialed client cannot
        // `GenerateSasUri`) — a deployment fact of the host-dialed `BlobContainerClient`, not a leg branch.
        Issue: (demand, handle, now) => Bound("azure", ObjectVerb.Grant, handle.Key, () => Task.FromResult<ObjectGrant>(new ObjectGrant.SignedUrl(r.Container.GetBlobClient(handle.Name).GenerateSasUri(demand.Request is GrantRequest.Write ? BlobSasPermissions.Write | BlobSasPermissions.Create : demand.Request is GrantRequest.Erase ? BlobSasPermissions.Delete : BlobSasPermissions.Read, (now + demand.Lifetime).ToDateTimeOffset())))),
        // Azure alone binds its lock after the seal: `ApplyAzure` yields `None` when the row states `ObjectLock.Off`,
        // so an unstanced write pays no extra round trip and a stanced one lands its policy on the sealed blob.
        Retain: (store, key, now) => store.Lock.ApplyAzure(now).Match(
            Some: apply => Bound("azure", ObjectVerb.Write, key.Key, () => apply(r.Container.GetBlockBlobClient(key.Name), now)).Map(static _ => unit),
            None: static () => IO.pure(unit)),
        // Azure moves NO bytes for a tier change at all — the access tier is a header the service rewrites in place, so
        // this row's ladder is the cheapest of the five and needs no copy verb. `StartCopyFromUriAsync` carries its own
        // `accessTier` parameter for the cross-container case, which this plane never takes because the object name is
        // content-keyed and its container is a host dial.
        Transition: (_, key, tier, _) => Bound("azure", ObjectVerb.Transition, key.Key, () => r.Container.GetBlobClient(key.Name).SetAccessTierAsync(tier.AzureTier)).Map(static _ => unit),
        // A non-empty archive status means a rehydration is already running, so the state reads before any request and a
        // second `SetAccessTier` never fires against an in-flight thaw. `window` goes unread here by provider design: a
        // rehydrated Azure blob stays on its new tier until something re-tiers it, so there is no readable-until deadline
        // to state and a fabricated one would be a window the provider never held.
        Rehydrate: (_, key, _) => Bound("azure", ObjectVerb.Restore, key.Key, () => r.Container.GetBlobClient(key.Name).GetPropertiesAsync()).Bind(head =>
            !string.IsNullOrEmpty(head.Value.ArchiveStatus)
                ? IO.pure<ThawState>(new ThawState.Thawing(None))
                : StorageTier.Observed(head.Value.AccessTier) == Some(StorageTier.Archive)
                    ? Bound("azure", ObjectVerb.Restore, key.Key, () => r.Container.GetBlobClient(key.Name).SetAccessTierAsync(AccessTier.Hot, conditions: null, rehydratePriority: RehydratePriority.Standard)).Map(static _ => (ThawState)new ThawState.Thawing(None))
                    : IO.pure<ThawState>(new ThawState.Resident())));

    // GCS: single resumable session over `StorageClient` (`api-objectstore` GCS_RESUMABLE) — the provider resumes the
    // session server-side, so the multipart legs collapse to a single chunked `UploadObjectAsync` with the genuine
    // write-once `IfGenerationMatch=0` create-if-absent precondition and the row's SSE-KMS key id through `ApplyGcs`
    // (SSE-C rides the dialed `StorageClient`'s `EncryptionKey`).
    static ObjectLeg GcsLeg(ObjectClient.Gcs r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        // Destination Object resource carries the EFFECTIVE tier's storage class (a protocol string on the
        // resource, not an options knob), so the Demote re-PUT changes GCS residence too.
        Seal: (store, tier, token, key, _, source, _) => Bound("gcs", ObjectVerb.Write, key.Key, () => r.Client.UploadObjectAsync(store.Stamp(new Google.Apis.Storage.v1.Data.Object { Bucket = r.Bucket, Name = token, ContentType = "application/octet-stream", StorageClass = tier.GcsClass }, key), source.AsStream(), store.Stamp(new UploadObjectOptions { IfGenerationMatch = 0, ChunkSize = 8 * 1024 * 1024 }))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (store, key, range) => Bound("gcs", ObjectVerb.Read, key.Key, async () => {
            MemoryStream sink = new();
            await r.Client.DownloadObjectAsync(r.Bucket, key.Name, sink, store.Integrity.ApplyGcs(new DownloadObjectOptions { Range = range.Match(Some: static w => new RangeHeaderValue(w.Start, w.End), None: static () => null) })).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound("gcs", ObjectVerb.Read, key.Key, () => r.Client.GetObjectAsync(r.Bucket, key.Name)).Map(x => Optional(Formed(key.Key, (long)(x.Size ?? 0), StorageTier.Observed(x.StorageClass).IfNone(store.Tier), slot => x.Metadata?.GetValueOrDefault(slot)))),
        Erase: key => Bound("gcs", ObjectVerb.Erase, key.Key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name)).Map(static _ => unit),
        // RECORDED STRUCTURAL NEGATIVE: no batch delete exists on this client. Its object verbs are exactly copy,
        // download, get, move, patch, restore, update, upload, delete, list, plus the two uploader factories — the
        // enumeration is the whole public surface, not a sample. The row therefore declares a page of ONE and this slot
        // folds the per-object verb, so the sweep's shape is identical on every provider and only its round-trip count
        // differs. The fold sequences: a transport failure ends the page rather than pushing a per-key refusal.
        EraseMany: page => page.TraverseM(handle => Bound("gcs", ObjectVerb.Erase, handle.Key, () => r.Client.DeleteObjectAsync(r.Bucket, handle.Name)).Map(static _ => unit)).As()
            .Map(_ => new EraseTally(page.Count, Seq<(ContentAddress Key, string Code)>())),
        Enumerate: () => Listed(r.Tenant, prefix => IO.liftAsync(() => Task.FromResult(toSeq(r.Client.ListObjects(r.Bucket, prefix).Select(static o => BlobName.OfName(o.Name)))))),
        // GCS V4 signing rides the credential-bound `UrlSigner` the host dials onto the row beside the client
        // (`StorageClient` itself carries no signer) — the TTL is a from-now duration by V4 construction.
        Issue: (demand, handle, _) => Bound("gcs", ObjectVerb.Grant, handle.Key, () => r.Signer.SignAsync(r.Bucket, handle.Name, demand.Lifetime.ToTimeSpan(), demand.Request is GrantRequest.Write ? HttpMethod.Put : demand.Request is GrantRequest.Erase ? HttpMethod.Delete : HttpMethod.Get)).Map(static url => (ObjectGrant)new ObjectGrant.SignedUrl(new Uri(url))),
        // The per-object retention rung this row now seats: the upload carries no retention member, so the window binds
        // after the seal exactly as Azure's does, and the bucket policy stops being the only place the column could land.
        Retain: (store, key, now) => store.Lock.ApplyGcs(now).Match(
            Some: apply => Bound("gcs", ObjectVerb.Write, key.Key, () => apply(r.Client, new Google.Apis.Storage.v1.Data.Object { Bucket = r.Bucket, Name = key.Name }, now)).Map(static _ => unit),
            None: static () => IO.pure(unit)),
        // TRAP: `CopyObjectOptions` carries NO storage-class member — the class rides `ExtraMetadata`, typed as the
        // object resource, whose own `StorageClass` holds the value. The destination bucket and name parameters are
        // OPTIONAL in the signature and FAIL when null, an upstream mistake the SDK's own documentation names, so a
        // same-place transition spells both explicitly. The call is a rewrite server-side: no bytes cross the wire.
        Transition: (_, key, tier, _) => Bound("gcs", ObjectVerb.Transition, key.Key, () => r.Client.CopyObjectAsync(r.Bucket, key.Name, r.Bucket, key.Name,
            new CopyObjectOptions { ExtraMetadata = new Google.Apis.Storage.v1.Data.Object { StorageClass = tier.GcsClass } })).Map(static _ => unit),
        // RECORDED STRUCTURAL NEGATIVE, and the CORRECT one: the GCS archive class is instantly readable, so no thaw
        // verb exists because none is needed and every rung answers `Resident`. `StorageClient.RestoreObjectAsync` is
        // SOFT-DELETE restore over a generation — a different axis entirely — and is NOT this slot; reading it as one
        // would restore a deleted object where the caller asked to read a cold one.
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    // Minio: S3-compatible, multipart auto-managed inside `PutObjectAsync` (`api-minio`) — one `PutObjectArgs` write per
    // blob carrying the row's SSE through `ApplyMinio`; inherited `WithNotMatchETag("*")` supplies the same
    // create-if-absent gate every cloud row carries; range read is `GetObjectArgs.WithOffsetAndLength`.
    static ObjectLeg MinioLeg(ObjectClient.Minio r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        // Minio exposes no server tier — the tier slot discards BY DECLARATION, so the Demote ladder's no-op on
        // this row is a stated fact rather than a silent one, and its `ObjectStat` states no storage class for
        // `StorageTier.Observed` to read back, so the head reports the row's own declared `Tier`.
        Seal: (store, _, token, key, _, source, now) => Bound("minio", ObjectVerb.Write, key.Key, async () => {
            using Stream stream = source.AsStream();
            PutObjectArgs request = new PutObjectArgs()
                .WithBucket(r.Bucket)
                .WithObject(token)
                .WithStreamData(stream)
                .WithObjectSize(source.Length)
                .WithContentType("application/octet-stream")
                .WithNotMatchETag("*");
            await r.Client.PutObjectAsync(store.Stamp(request, key, now)).ConfigureAwait(false);
            return unit;
        }),
        Abort: (_, key) => Bound("minio", ObjectVerb.Write, key.Key, async () => {
            await foreach (Upload upload in r.Client.ListIncompleteUploadsEnumAsync(new ListIncompleteUploadsArgs().WithBucket(r.Bucket).WithPrefix(key.Name)).ConfigureAwait(false))
                await r.Client.RemoveIncompleteUploadAsync(new RemoveIncompleteUploadArgs().WithBucket(r.Bucket).WithObject(upload.Key)).ConfigureAwait(false);
            return unit;
        }),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        // The read stance arms nothing here: no `With*` setter anywhere in the args algebra takes a checksum, and
        // `ObjectStat` publishes no digest beyond `ETag`, so this row's transport claim is the SDK's own check alone and the
        // identity claim rests entirely on the domain-side fold. That is a recorded negative over the whole assembly, not an
        // omission at this call.
        Fetch: (_, key, range) => Bound("minio", ObjectVerb.Read, key.Key, async () => {
            MemoryStream sink = new();
            GetObjectArgs request = new GetObjectArgs().WithBucket(r.Bucket).WithObject(key.Name).WithCallbackStream(stream => stream.CopyTo(sink));
            await r.Client.GetObjectAsync(range.Match(Some: window => request.WithOffsetAndLength(window.Start, window.End - window.Start + 1), None: () => request)).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound("minio", ObjectVerb.Read, key.Key, () => r.Client.StatObjectAsync(new StatObjectArgs().WithBucket(r.Bucket).WithObject(key.Name))).Map(x => Optional(Formed(key.Key, x.Size, store.Tier, slot => x.MetaData.GetValueOrDefault(slot)))),
        Erase: key => Bound("minio", ObjectVerb.Erase, key.Key, () => r.Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(r.Bucket).WithObject(key.Name))).Map(static _ => unit),
        // TRAP: this request hardcodes the quiet flag, so the response carries ONLY failures — `RemoveObjectsAsync`
        // returns the refusal list and never a deleted list, which is exactly why the tally derives its success count
        // from the page it was handed rather than from a response half that does not exist. `DeleteError` lives in
        // `Minio.Exceptions` despite being a plain result row, it declares no members of its own, and every field it
        // carries — `Code`, `Key`, `Message` — comes from its `ErrorResponse` base. `RemoveObjectsArgs` keeps its
        // backing collections `internal`, so the `With*` builders are the ONLY ingress.
        EraseMany: page => Bound("minio", ObjectVerb.Erase, default, () => r.Client.RemoveObjectsAsync(new RemoveObjectsArgs()
                .WithBucket(r.Bucket)
                .WithObjects(page.Map(static h => h.Name).ToList())))
            .Map(refused => new EraseTally(page.Count, toSeq(refused).Map(static e => (BlobName.OfName(e.Key), e.Code)))),
        Enumerate: () => Listed(r.Tenant, prefix => Bound("minio", ObjectVerb.List, default, async () => {
            List<ContentAddress> keys = [];
            await foreach (Item item in r.Client.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(r.Bucket).WithPrefix(prefix).WithRecursive(true)).ConfigureAwait(false))
                keys.Add(BlobName.OfName(item.Key));
            return toSeq(keys);
        })),
        // The Minio SDK mints only three presign shapes — get, put, and a POST policy — so a presigned DELETE is
        // unmintable there and its own presign entries additionally take no cancellation token. The CAPABILITY is not
        // absent, only that SDK's surface is: a presigned DELETE is an ordinary SigV4 URL and this endpoint speaks S3, so the
        // row's host-dialed `GrantSigner` mints every verb over the ONE collapsed signing owner the cloud row also uses.
        // Two presigners parameterized by provider became one parameterized by endpoint, and the refusal that once
        // answered an `Erase` demand is deleted rather than re-justified.
        Issue: (demand, handle, now) => Bound("minio", ObjectVerb.Grant, handle.Key, () => r.Signer.Sign(demand, handle, now)),
        Retain: static (_, _, _) => IO.pure(unit),
        // RECORDED STRUCTURAL NEGATIVE: the copy builder's storage-class setter is `internal` and the fluent builders are the
        // only ingress, so no public path stamps a class on this client; no type in the whole assembly carries a tier,
        // restore, or archive vocabulary, and `ObjectStat.ArchiveStatus` is a bare string the server may never fill. The
        // row seats `TierSeat.None` for exactly that reason and the slot is a DECLARED no-op, so the ladder moves nothing
        // here by statement rather than by silence.
        Transition: static (_, _, _, _) => IO.pure(unit),
        // No server tier means no cold rung and nothing to thaw; every rung is readable, so this row answers `Resident`
        // for the same structural reason the GCS row does, arrived at from the opposite direction.
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    // Presigned-grant: the leg holds NO credential — the client-side credential never exists, which is the
    // reach no credentialed row has. Every transfer op mints an `ObjectGrant` per operation through the case's
    // `GrantMinter` and EXECUTES it over the same engine: `FormPost` is ONE `multipart/form-data` POST carrying
    // every minted field plus the payload (the `S3UploadRequest { Url, Fields }` execution — `Helper.
    // UploadArtifactAsync` the decompile-verified upstream precedent), `SignedUrl` a bare GET/HEAD/DELETE.
    // Whole-object single-shot (Stage no-op, the unreachable part floor packs ONE window); `Head`/`Enumerate`
    // fill from the case's `Roster` delegate (upstream `ListArtifacts → FileMetaList` — no head verb exists on
    // a grant plane); only the expiry-aware minter emits `GrantExpired`, while a bare `403` is `Denied`.
    static ObjectLeg PresignedLeg(ObjectClient.Presigned r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (_, _, _, key, _, source, _) => r.Minter(new GrantRequest.Write(key.Key, source.Length)).Bind(grant =>
            Bound("presigned", ObjectVerb.Write, key.Key, async () => {                                      // Exemption: the granted HTTP execution is the platform-forced statement seam
                using HttpResponseMessage response = grant switch {
                    ObjectGrant.FormPost post => await Posted(r.Http, post, key, source).ConfigureAwait(false),
                    ObjectGrant.SignedUrl url => await r.Http.PutAsync(url.Url, new ReadOnlyMemoryContent(source)).ConfigureAwait(false),
                    _ => throw new InvalidOperationException(nameof(ObjectGrant)),
                };
                return response.IsSuccessStatusCode ? unit : throw Granted(ObjectVerb.Write, key.Key, response.StatusCode).ToException();
            })),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (_, key, range) => r.Minter(new GrantRequest.Read(key.Key)).Bind(grant =>
            Bound("presigned", ObjectVerb.Read, key.Key, async () => {
                using HttpRequestMessage request = new(HttpMethod.Get, Url(grant));
                _ = range.Map(w => request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(w.Start, w.End));
                HttpResponseMessage response = await r.Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStreamAsync().ConfigureAwait(false) : throw Granted(ObjectVerb.Read, key.Key, response.StatusCode).ToException();
            })),
        // The upstream roster keys on the content address and states no storage class, so the residence carries the row's
        // declared `Tier` — a grant plane publishes no tiering vocabulary to observe. The upstream roster keys on the
        // content address and states no storage class, no metadata, and no plaintext length, so the residence carries the
        // row's declared `Tier` and the pass-through form — a grant plane publishes no tiering or residence vocabulary to
        // observe, and a form asserted where the roster states none would be a stored fact nobody wrote.
        Head: (store, key) => r.Roster(Some(key.Key)).Map(rows =>
            rows.Find(s => s.Key == key.Key).Map(s => BlobResidence.From(key.Key, s.Length, s.Length, store.Tier, ObjectCodec.Identity))),
        Erase: key => r.Minter(new GrantRequest.Erase(key.Key)).Bind(grant =>
            Bound("presigned", ObjectVerb.Erase, key.Key, async () => {
                using HttpResponseMessage response = await r.Http.DeleteAsync(Url(grant)).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? unit : throw Granted(ObjectVerb.Erase, key.Key, response.StatusCode).ToException();
            })),
        // A grant plane mints one URL per operation and publishes no batch verb, so the row declares a page of ONE and
        // this slot folds the same per-object erase every other page-of-one row folds.
        EraseMany: page => page.TraverseM(handle => r.Minter(new GrantRequest.Erase(handle.Key)).Bind(grant =>
                Bound("presigned", ObjectVerb.Erase, handle.Key, async () => {
                    using HttpResponseMessage response = await r.Http.DeleteAsync(Url(grant)).ConfigureAwait(false);
                    return response.IsSuccessStatusCode ? unit : throw Granted(ObjectVerb.Erase, handle.Key, response.StatusCode).ToException();
                }))).As()
            .Map(_ => new EraseTally(page.Count, Seq<(ContentAddress Key, string Code)>())),
        // The upstream roster IS the listing — a grant plane exposes no prefix query, so the class fold has nothing to
        // partition here and the roster's own rows answer whole.
        Enumerate: () => r.Roster(None).Map(static rows => rows.Map(static s => s.Key)),
        // The presigned row is already a grant plane — issuance forwards to ITS minter (the domain cloud signs the
        // upstream name), so the projected handle names nothing this leg sends.
        Issue: (demand, _, _) => r.Minter(demand.Request),
        Retain: static (_, _, _) => IO.pure(unit),
        // The grant plane publishes no storage class and no cold rung, so both residence-ladder slots are declared no-ops
        // here for the one reason the row's whole `Degrade` clause already states.
        Transition: static (_, _, _, _) => IO.pure(unit),
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    static async Task<HttpResponseMessage> Posted(HttpClient http, ObjectGrant.FormPost post, BlobHandle key, ReadOnlyMemory<byte> source) {
        using MultipartFormDataContent form = new();
        foreach ((string field, string value) in post.Fields)
            form.Add(new StringContent(value), field); // Exemption: minted fields precede the S3 form payload
        form.Add(new ReadOnlyMemoryContent(source), "file", key.Name);
        return await http.PostAsync(post.Url, form).ConfigureAwait(false);
    }

    static Uri Url(ObjectGrant grant) => grant switch {
        ObjectGrant.SignedUrl s => s.Url,
        ObjectGrant.FormPost p => p.Url,
        _ => throw new InvalidOperationException(nameof(ObjectGrant)),
    };

    // Grant-plane status fold, this fifth row's `Lift` equivalent: no SDK exception family exists on a bare HTTP
    // grant, so status IS the discriminant and its VERB rides in from whichever slot sent it, exactly as it rides
    // `Bound` on every credentialed row. A status-only fold would leave this band's one re-drivable case unable to
    // name what it re-drives.
    static RemoteStoreFault Granted(ObjectVerb verb, ContentAddress key, HttpStatusCode status) => status switch {
        HttpStatusCode.Forbidden => new RemoteStoreFault.Denied(key, "presigned", "forbidden"),
        HttpStatusCode.NotFound => new RemoteStoreFault.NotFound(key),
        HttpStatusCode.PreconditionFailed => new RemoteStoreFault.Conflict(key, "if-none-match"),
        HttpStatusCode refused => new RemoteStoreFault.Transport("presigned", verb, key, (int)refused, refused.ToString()),
    };
}
```

## [04]-[BLOB_GC]

- Owner: `BlobCatalogRow` the content-lineage retention row every blob carries (the same row the snapshot spine has, `H10`), keyed on the `Version/retention#RETENTION_CLASSES` `ArtifactKind` its `Class` DERIVES from and carrying the `WormUntil` object-lock window and the `Codec`/`Plain` residence form beside the `Tenant`/`Bytes`/`Tier`/`Lineage`/`Classification` columns, projecting to the `Version/retention#SWEEP_AND_GC` `RetentionFact` the ONE deletion executor consumes; `PendingWrite` the write-blob-first pending ledger, kind-bearing so its fence ages against its own class; `LifecycleRule`/`LifecycleRules` the provider-side half — the declared-schedule projection of expiry and cold-tier rungs onto per-class key prefixes plus its per-provider `Arm` column; `BlobGc` the static surface owning ONLY the write-blob-first protocol, the in-flight-fence eligibility predicate it contributes to the retention sweep, and the SET-shaped WORM-aware `evict` arrow answering in `EraseTally` — it does NOT own a second deletion executor.
- Law: one sweep pass per retention class PRESENT in the catalog, because `RetentionSweep.Run` budgets against ONE `RetentionSchedule` and `SweepReceipt.Conserves` closes over one class's inventory — a mixed inventory folded under one class silently budgets every row at the founding class's ceiling, which is a `Cache`-class artifact carried at thirty-two times its declared budget and never evicted at all.
- Entry: `WriteBlobFirst` takes the four facts only the admitting caller holds — the `ArtifactKind`, the payload family's `ObjectCodec`, the settled `DataClassification`, and the `Lineage` — opens the `PendingWrite` ledger row, applies the residence transform through the store's one `Encode` fold, write-once-seals the blob through `MultipartTransfer.Upload`, commits the catalog row, then clears the pending row; `InFlightFence(pending, now)` derives each key's grace from that key's OWN `Kind.Retention.Schedule.OrphanAge`, so no grace knob can diverge from the retention verdict and no class inherits the lane's founding window; `Sweep` groups the catalog by derived class, projects each group to `RetentionFact`s, and routes every group through `RetentionSweep.Run` and `RetentionSweep.Execute` with the SET-shaped `WormEvict` and the per-key `Demote` arrow, returning the receipt SEQUENCE the per-class signatures oblige; `LifecycleRules.Arm(client, classes)` installs the provider-side rules once per bucket, `Project` deriving them from declared schedule values alone.
- Auto: write-first carries `open → blob → catalog → close`; a crash before catalog commit leaves a pending-fenced orphan until the pending row's own class orphan age, never a dangling event reference. `BlobGc.Sweep` partitions the catalog by the class each row's kind derives, projects each row to `RetentionFact`, and delegates every verdict and mutation to the one retention executor over full-history reachability — the entry tier stays the PROVIDER's (`store.Tier`) and only the `RetentionCeiling.Demote` ladder moves it, so the tier column reads as the residence fact it is rather than an asset decision the caller never made. `Demote` rewrites a header rather than re-writing a payload; where a provider lifecycle rule already realized a rung it observes that on the `Head` and receipts without paying even the metadata round trip, and where an active WORM window holds the key it refuses `Locked` rather than eating a provider status the lift folds to a denial. Eviction leaves as a SET: the WORM index is a pure predicate, so it partitions the verdict group with no request sent and the residual goes out through the row's own `EraseBatch` paging, held keys landing on the tally as per-key refusals the sweep receipt accounts rather than a rail failure costing the whole pass one compliance window.
- Receipt: a blob write rides `store.blob.write` carrying the content key and bytes; the GC reclaim rides the `Version/retention#SWEEP_AND_GC` `SweepReceipt` on the retention sweep's own fact stream (the orphan count and reclaimed bytes are the retention executor's receipt, never a parallel `store.blob.gc` stream the blob lane re-mints).
- Packages: System.IO.Hashing, NodaTime (`Instant`/`Duration` the `WormUntil` window), LanguageExt.Core (`Seq`/`Choose`/`IO.fail`), System.Collections.Frozen (`FrozenDictionary` the WORM index), Thinktecture.Runtime.Extensions (`RetentionClass.Items`/`StorageLane`/`LossPolicy`/`RetentionCeiling.Demote`/`RetentionFact`/`SweepReceipt`/`Hold`/`Reachability` the `Version/retention#SWEEP_AND_GC` surface this owner composes), AWSSDK.S3 (`PutLifecycleConfigurationAsync` + the `LifecycleConfiguration`/`LifecycleRule`/`LifecycleFilter`/`LifecycleRuleExpiration`/`LifecycleTransition` rule shapes), Google.Cloud.Storage.V1 (`PatchBucketAsync` over the `Bucket.LifecycleData` resource), Minio (`SetBucketLifecycleAsync` + `Minio.DataModel.ILM`), BCL inbox. (The WORM/object-lock SET is the `#OBJECT_STORE` `ObjectLock` write-leg's concern — the blob lane only READS the catalog `WormUntil` and mints `RemoteStoreFault.Locked`; the SDK reach here is the bucket-lifecycle arming alone, lifting at the one `ObjectIo.Bound` boundary. SSE-KMS needs only the key-id STRING, the KMS signing SDKs being the `Version/provenance#ATTESTED_LEDGER` owner's, never composed at the blob lane.)
- Growth: a new catalog column is one field on `BlobCatalogRow` (as `WormUntil` and the residence form are); a new WORM/object-lock stance is one `ObjectLock` case the `WriteBlobFirst` `Lock.Until` and the `WormEvict` arrow both read with zero new surface; a new retention class arms its own prefix rule through `LifecycleRules.Project` with zero edits (the schedule columns already carry the deadline), a new cold rung re-roots the ladder from the `RetentionCeiling` table alone, and a provider gaining a lifecycle surface is one `Arm` case; zero new surface — a head-only blob GC, a `BlobGc`-local `List`-then-`Filter` sweep parallel to the retention executor, a payload re-PUT standing in for a storage-class change, a same-PG-txn blob write, a two-ORM atomicity dance, a blob-lane-local retention executor that re-decides eviction beside `RetentionSweep`, or a free-string blob name is the deleted form because the GC routes through the ONE `RetentionSweep` over the full history (the WORM fence riding the injected `eligible` predicate and the typed `WormEvict` arrow, never a second sweeper), the blob is write-first content-addressed, and identity+event is the one Marten-session transaction.
- Boundary: the artifact blob carries the SAME content-lineage and retention-catalog row the snapshot spine has (`H10`) and registers in the `Version/retention#RETENTION_CLASSES` `blob` class so the ONE full-history reachability GC governs both — a blob-lane-local deletion executor is the deleted form, the blob lane contributing only its `RetentionFact` projection and its `InFlightFence` predicate to the `RetentionSweep` every lane routes through, and artifact GC over head alone is FORBIDDEN (the retention `Mark` folds every AS-OF cut so a blob a historical version references survives); the write-blob-first + `412`-noop protocol survives a crash (orphan blob, never dangling reference) and the `PendingWrite` ledger row OPENS before the blob `Put` and CLOSES after the catalog commit, so the in-flight fence distinguishes an in-flight write from an orphan even before the catalog row exists (the fail-safe never reaps an un-ageable present write); the ONE transaction owner for identity+event is the Marten `IDocumentSession` (`Element/graph#STORE_RAIL`), the blob being write-first and reference-after with no free two-ORM atomicity; the SSE key MATERIAL is a key-id STRING carried on the `ObjectEncryption` case (`ManagedKey` the SSE-KMS key id the `Element/identity#KMS_CUSTODY` `EnvelopeKeyring` or the host KMS minted out-of-band, `CustomerKey` the SSE-C key + MD5, `ProviderManaged` the account/bucket-default) and `ObjectEncryption.ApplyS3`/`ApplyGcs`/`ApplyMinio` stamp it on the wire at each provider's request (Azure at the dialed client) — a blob-lane-local KMS envelope is the deleted form (the DEK-wrapping envelope lifecycle and both cloud-KMS keyrings are the `Element/authority#AUTHORITY` owner's), key acquisition being a host connection input never a fence member; the WORM/object-lock retention-until is a write-policy stance carried on the `ObjectLock` case (`Off` the default, `Governance`/`Compliance` a `Retain` window) — `ObjectLock.ApplyS3`/`ApplyMinio` SET it on the wire from the ONE `Upload`-sampled instant and `WriteBlobFirst` records the SAME derivation (`receipt.WormUntil`) onto `BlobCatalogRow.WormUntil`, so the GC's set-shaped `WormEvict` arrow partitions a key under an active window out of the batch and refuses it on the tally under `RemoteStoreFault.Locked`'s own name (the eligibility fence already holds it out of selection, the arrow the defense-in-depth second gate) while `Demote` mints the typed fault outright, making the lock a named refusal on the sweep receipt rather than an opaque provider `403` `Lift` mis-folds to `Denied` — and a compliance window costs one key rather than the whole pass; the object name (`BlobName` `{class}/{tenant:Text}/{key:x32}` over the injected `ObjectClient.Tenant` and the admitted row's class) partitions the blob namespace by class then tenant, so a multi-tenant store isolates by construction and a provider lifecycle rule targets one class's prefix across every tenant — `LifecycleRules.Project` arming expiry and cold-tier rungs from declared schedule values alone, the count and size stages staying the sweep's because a prefix rule can read neither; a provider-expired object settles through the sweep's OBSERVATION — a catalog row whose provider head reads absent under a lifecycle-armed class receipts its eviction at the sweep, so no deletion goes unreceipted and the provider half never opens a silent gap in the ledger; the `BlobCatalogRow.Tenant` column stamps the [A.1] `frame.Tenant` the `WriteBlobFirst` first-step guard proves equal to `client.Tenant` (one injected source — an ambient `TenantContext.Current` read is the named inversion, deleted), and the retention catalog `Seq<BlobCatalogRow>` the caller hands `BlobGc.Sweep` is filtered by the canonical x32-text tenant RLS partition at the catalog query so a cross-tenant reclaim is unrepresentable end to end — the sweep never sees another tenant's rows, and the per-tenant object-name prefix means even a mis-supplied catalog row resolves to a name under the wrong tenant's prefix that the provider does not hold.

```csharp signature
// The content-lineage retention row. `WormUntil` is the WORM/object-lock retention-until the
// `Store/blobstore#OBJECT_STORE` `ObjectLock` write stance SET on the blob (`ObjectLock.Until(now)`) — `None` for the
// default `ObjectLock.Off` rows, an `Instant` for a compliance/governance-class blob whose bytes are provider-immutable
// until the window lapses; it is the ONE column that makes `RemoteStoreFault.Locked` genuinely reachable, the sweep's
// `evict` arrow refusing a key still under it. `Dek` is the `ClientSealed` residence's persisted envelope
// (`Element/identity#KMS_CUSTODY` `WrappedKey`) — `None` on every SSE/provider-managed row; the read path unwraps it
// through `ObjectEncryption.OpenSource`. `Kind` REPLACES the former stored `RetentionClass` column: retention DERIVES
// from the asset class exactly as it does at `Query/cache#ARTIFACT_BLOB_INDEX`, so one column carries both facts, a
// stored class contradicting its kind is unrepresentable, and the plane's two catalogs finally share one axis.
// `Classification` and `Lineage` arrive from the admitting caller — the only holder of either — because a manufactured
// `Internal` stamp passes a ceiling it never faced and a `None` lineage empties the very column this row is named for.
// `Codec` and `Plain` record the residence form the write chose beside the `Bytes` it actually stored, so the sweep
// budgets on stored bytes — the figure the provider bills — while a reader still knows what plaintext extent those bytes
// cover. Both also mirror the object's own metadata, which is what lets a catalog rebuild from a listing pass rather than
// from a form nobody wrote down.
public sealed record BlobCatalogRow(ContentAddress Key, ArtifactKind Kind, long Bytes, long Plain, StorageTier Tier, ObjectCodec Codec, Option<ContentAddress> Lineage, TenantId Tenant, DataClassification Classification, Option<Instant> WormUntil, Option<WrappedKey> Dek, Instant At) {
    public RetentionClass Class => Kind.Retention;
}

// `Kind` rides the pending row for the same reason it rides the catalog row: the in-flight fence ages a crashed
// write against ITS OWN class's declared orphan window, and a cache-class crash held for the blob class's seven
// days is a fence measuring the founding class rather than the artifact.
public readonly record struct PendingWrite(ContentAddress Key, ArtifactKind Kind, long Bytes, Instant Started, Option<string> Session);

// --- [LIFECYCLE_RULES] ----------------------------------------------------------------------
// The PROVIDER-side half of retention, unlocked by the class-leading object name: a lifecycle rule targets a key PREFIX,
// so one rule per `{class}/{tenant}/` stem hands the provider's own engine the expiries and tier transitions the sweep
// would otherwise pay one request per object to effect. Rows derive from DECLARED schedule values alone — a class that
// expires (declared-expiry, or receipted-evict whose declared order LEADS with the age stage) expires at its own
// `AgeBound`, and a never-evict class demotes down the `Version/retention#SWEEP_AND_GC` `RetentionCeiling` ladder at
// cumulative multiples of that SAME bound, rung k at k x `AgeBound` — one declared value, zero new knobs. The count and
// size stages NEVER project: a prefix rule reads neither a live count nor a running byte total, so those two stages stay the
// sweep's and the provider half can never contradict a verdict it could not compute. A class whose age bound never lapses
// arms nothing, because a rule with no reachable deadline only pretends to govern.
public readonly record struct LifecycleRule(RetentionClass Class, Option<Duration> Expire, Seq<(StorageTier To, Duration After)> Transitions);

public static class LifecycleRules {
    // The ladder ROOT derives rather than asserts: the entry rung is the one `StorageTier` no other rung demotes into, so
    // inserting a rung at the retention ladder re-roots this projection with zero edits here, and `Rungs` is every rung
    // below it in ladder order — the roster a transition schedule spaces out.
    static readonly Seq<StorageTier> Rungs = toSeq(StorageTier.Items)
        .Find(static tier => toSeq(StorageTier.Items).ForAll(other => RetentionCeiling.Demote(other) != Some(tier)))
        .Match(Some: Descent, None: static () => Seq<StorageTier>());

    public static Seq<LifecycleRule> Project(Seq<RetentionClass> classes) =>
        classes.Filter(static cls => cls.Lane == StorageLane.ObjectStore && cls.Schedule.AgeBound < Duration.MaxValue)
            .Map(static cls => cls.Loss.Expires || cls.Loss.Evicts
                ? new LifecycleRule(cls, Some(cls.Schedule.AgeBound), Seq<(StorageTier To, Duration After)>())
                : new LifecycleRule(cls, None, toSeq(Rungs.Select((rung, index) => (To: rung, After: cls.Schedule.AgeBound * (index + 1))))));

    static Seq<StorageTier> Descent(StorageTier from) =>
        RetentionCeiling.Demote(from).Match(Some: colder => colder.Cons(Descent(colder)), None: static () => Seq<StorageTier>());

    // Per-provider application, the union case IS the dispatch exactly as `ObjectClient.Map` owns every other
    // per-provider variance, and the arms mirror the `StorageTier` per-provider columns row for row. Azure's lifecycle
    // policy is an ARM MANAGEMENT-plane resource outside the admitted data-plane package, so its arm declares NO
    // lifecycle surface and the `BlobGc.Demote` fetch-and-re-PUT stays its whole mechanism — the same declared-None
    // form the `StorageTier` Minio column already takes; the presigned row holds no bucket to arm. Minio's ILM rule
    // carries ONE transition and its row states no storage-class column, so it arms the expiries alone and its rung
    // ladder stays the re-PUT's. Every SDK call lifts at the ONE `ObjectIo.Bound` boundary, never a second fold, and
    // `LifecycleRule`/`LifecycleConfiguration` name three different types across this owner and the two SDKs, so each
    // arm spells its provider type whole.
    public static IO<Unit> Arm(ObjectClient client, Seq<RetentionClass> classes) {
        Seq<LifecycleRule> rules = Project(classes);
        return rules.IsEmpty ? IO.pure(unit) : client.Map(
            // `Amazon.S3.Model.LifecycleRule.Prefix` is obsolete in favour of the filter, so the stem rides
            // `LifecycleFilter.Prefix` and the rule id IS that same stem — one string, no second naming.
            s3: r => ObjectIo.Bound("s3", ObjectVerb.Lifecycle, default, () => r.Client.PutLifecycleConfigurationAsync(r.Bucket, new Amazon.S3.Model.LifecycleConfiguration {
                Rules = rules.Map(rule => new Amazon.S3.Model.LifecycleRule {
                    Id = BlobName.Prefix(r.Tenant, rule.Class),
                    Status = LifecycleRuleStatus.Enabled,
                    Filter = new LifecycleFilter { Prefix = BlobName.Prefix(r.Tenant, rule.Class) },
                    Expiration = rule.Expire.Match(Some: static after => new LifecycleRuleExpiration { Days = (int)after.TotalDays }, None: static () => null),
                    Transitions = rule.Transitions.Map(static t => new LifecycleTransition { Days = (int)t.After.TotalDays, StorageClass = t.To.S3Class }).ToList(),
                }).ToList(),
            })).Map(static _ => unit),
            azure: static _ => IO.pure(unit),
            gcs: r => ObjectIo.Bound("gcs", ObjectVerb.Lifecycle, default, () => r.Client.PatchBucketAsync(new Google.Apis.Storage.v1.Data.Bucket {
                Name = r.Bucket,
                Lifecycle = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData { Rule = rules.Bind(rule => Gcs(rule, r.Tenant)).ToList() },
            })).Map(static _ => unit),
            minio: r => rules.Choose(static rule => rule.Expire.Map(after => (rule.Class, After: after))).Match(
                Empty: static () => IO.pure(unit),                                         // `SetBucketLifecycleAsync` refuses an empty rule set
                More: expiring => ObjectIo.Bound("minio", ObjectVerb.Lifecycle, default, async () => {
                    await r.Client.SetBucketLifecycleAsync(new SetBucketLifecycleArgs()
                        .WithBucket(r.Bucket)
                        .WithLifecycleConfiguration(new Minio.DataModel.ILM.LifecycleConfiguration(expiring.Map(row => new Minio.DataModel.ILM.LifecycleRule {
                            ID = BlobName.Prefix(r.Tenant, row.Class),
                            Status = Minio.DataModel.ILM.LifecycleRule.LifecycleRuleStatusEnabled,
                            Filter = new RuleFilter { Prefix = BlobName.Prefix(r.Tenant, row.Class) },
                            Expiration = new Expiration { Days = row.After.TotalDays },
                        }).ToList()))).ConfigureAwait(false);
                    return unit;
                })),
            presigned: static _ => IO.pure(unit));
    }

    // GCS carries ONE action per rule, so the expiry and each transition rung are separate rules under the same prefix
    // condition — the provider's own shape, never a second policy this owner invents.
    static Seq<Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData> Gcs(LifecycleRule rule, TenantId tenant) =>
        rule.Expire.Map(after => Gcs("Delete", null, after, rule.Class, tenant)).ToSeq() +
        rule.Transitions.Map(t => Gcs("SetStorageClass", t.To.GcsClass, t.After, rule.Class, tenant));

    static Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData Gcs(string action, string? storageClass, Duration after, RetentionClass cls, TenantId tenant) => new() {
        Action = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData.ActionData { Type = action, StorageClass = storageClass },
        Condition = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData.ConditionData { Age = (int)after.TotalDays, MatchesPrefix = [BlobName.Prefix(tenant, cls)] },
    };
}

public static class BlobGc {
    // WRITE-BLOB-FIRST: `open` appends the `PendingWrite` ledger row, `SealSource` applies the row's residence transform
    // ONCE (a `ClientSealed` row AES-GCM-seals under a freshly-minted DEK, every other row passes the bytes through), the
    // ONE receipt path (`MultipartTransfer.Upload`) write-once-seals the blob (the row's `ObjectLock` stance SET on the
    // wire) and emits the `BlobTransferReceipt` facts with the frame's correlation threaded onto the residence, the
    // catalog row commits carrying the WORM window AND the wrapped DEK, then `close` clears the pending row — THREE
    // durable marks, the sweep reading the open pending set as the in-flight fence. A crash before the catalog commit
    // leaves a present blob with an OPEN pending row (protected until the blob retention schedule's orphan age) and no
    // event reference — a collectible orphan, never a dangling reference; the `412`-noop makes a re-drive survive. The
    // blob is NOT in the event's PG txn (`H10`). `WormUntil = receipt.WormUntil` — the ONE instant `Upload` sampled
    // derived the provider retention date AND this column, so catalog and provider agree by construction (a second
    // `frame.Now()` sample here was the two-clock divergence, deleted); the admitted `kind` is the ONE source of the
    // object's class, so `handle` projects the class-leading name here and the catalog row, the provider lifecycle rule
    // governing its prefix, and the sweep's own partition all read one class; `frame.Tenant` stamps the catalog row's RLS
    // partition — the [A.1] frame, never an AppHost type crossing down — and the FIRST step refuses a
    // `frame.Tenant.TenantId`/`client.Tenant` mismatch (`Denied`, "tenant-mismatch"), so the name prefix the legs write
    // and the catalog row the sweep reads can never diverge: one injected tenant, structurally.
    public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, ContentAddress key, ArtifactKind kind, ObjectCodec codec, DataClassification classification, Option<ContentAddress> lineage, ReadOnlySequence<byte> source, Option<string> session, Func<PendingWrite, IO<Unit>> open, Func<BlobCatalogRow, IO<Unit>> catalog, Func<ContentAddress, IO<Unit>> close, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) =>
        from _t in frame.Tenant.TenantId == client.Tenant ? IO.pure(unit) : IO.fail<Unit>(new RemoteStoreFault.Denied(key, store.Key, "tenant-mismatch"))
        let handle = BlobName.Handle(key, client.Tenant, kind.Retention, codec, source.Length)
        from _o in open(new PendingWrite(key, kind, source.Length, frame.Now(), session))
        // ONE residence transform over both stages in the ONE order the store owns — codec then seal — so this entry
        // never re-spells which frames first and a new stored form costs no edit here at all.
        from formed in store.Encode(codec, key, source)
        from receipt in MultipartTransfer.Upload(store, client, handle, BlobResidence.From(key, formed.Bytes.Length, source.Length, store.Tier, codec) with { ConditionToken = session }, ContentChunker.Chunk(store.Chunking, formed.Bytes), formed.Bytes, sink, frame)
        from _c in catalog(new BlobCatalogRow(key, kind, formed.Bytes.Length, source.Length, store.Tier, codec, lineage, frame.Tenant.TenantId, classification, receipt.WormUntil, formed.Dek, frame.Now()))
        // The `store.blob.write` fact the slot roster declares and the transfer legs cannot emit: the part, resume, and
        // abort facts are TRANSFER events while this one is the object's admission, so it fires once here after the
        // catalog row commits — a declared slot no owner emits is a projection arm that can never fire.
        from _w in sink(new BlobTransferFact(store.Key, "write", key, receipt.Bytes, receipt.Parts, session))
        from _x in close(key)
        select new BlobResidence(receipt.Key, receipt.Bytes, source.Length, store.Tier, codec, receipt.Parts, receipt.ResumedParts, receipt.Verified, None, receipt.Correlation);

    // The in-flight fence THIS owner contributes to the retention sweep's `eligible` predicate: a key under an OPEN
    // `PendingWrite` younger than ITS OWN class's declared `OrphanAge` is in-flight, not an orphan — every other key is
    // eligible (reachability + holds + age are the retention sweep's own stages, never re-decided here). The window reads
    // off the pending row's `Kind`, so a cache-class crash is fenced for its declared twenty-four hours instead of the
    // never-evict class's seven days — a lane-founding literal here governed every class the lane carries, which is the
    // same drift the catalog row's derived `Class` deletes on the inventory side. This is the ONE crash-window fence the
    // catalog alone cannot express (a present blob whose catalog row has not yet committed has no inventory row, so the
    // pending ledger is the only evidence it is mid-write).
    public static Func<ContentAddress, bool> InFlightFence(Seq<PendingWrite> pending, Instant now) =>
        key => pending.Find(w => w.Key == key).Match(Some: w => now - w.Started >= w.Kind.Retention.Schedule.OrphanAge, None: () => true);

    // The blob catalog row IS a `RetentionFact` for the `blob` class — the sealed `Bytes` field is the byte figure the
    // retention sweep budgets on (never a later filesystem stat), the content `Key` the identity, `Tier` the CURRENT
    // `StorageTier` the never-evict cold-tiering verdict reads (the `blob` class is `LossPolicy.NeverEvict`, so an aged
    // reachable blob `Cool`s one rung down the `RetentionCeiling.Demote` ladder rather than collecting), the `At` the age
    // stamp. `Class` reads off the row's own `ArtifactKind`, so a `Cache`-class texture set is budgeted, aged, and swept
    // under the cache schedule that admitted it rather than under the lane's founding class — the projection that
    // re-minted the literal was the one column of five that decided the policy and the only one not read.
    static RetentionFact ToFact(BlobCatalogRow row) => new(row.Class, row.Key, row.Bytes, row.Tier, row.At);

    // The cold-tier demotion the `blob` class's `LossPolicy.NeverEvict` cooling verdict lands: `Cool` keeps the blob
    // resident one storage-class rung down the `Version/retention#SWEEP_AND_GC` `RetentionCeiling.Demote` ladder, and a
    // rung is a HEADER. The transition therefore rewrites that header through the leg slot and moves ZERO bytes. The
    // fetch → `Drain` → content-addressed re-PUT this once ran is DELETED as the rejected form: it paid a full egress
    // plus a full ingress to change a field the provider rewrites in place, on exactly the payloads big enough to have
    // been demoted, and it routed a non-write through the write path's receipt, session, and conditional-seal machinery. The
    // OBSERVATION GATE still fires first: a provider lifecycle rule armed on this class's prefix demotes natively, so a
    // `Head` reading the target rung back through `StorageTier.Observed` means the transition ALREADY happened and even a
    // metadata round trip is waste. The WORM GATE fires beside it, for the same reason `WormEvict` exists: a provider
    // refuses to re-class an object under an active retention window and raises a status the `Lift` folds to a denial, so the
    // domain check surfaces the typed `Locked` the catalog can already prove instead. Both outcomes receipt — observed,
    // refused, or transitioned — so the sweep's `Cool` verdict reconciles against evidence either way.
    static IO<Unit> Demote(ObjectStore store, ObjectClient client, RetentionClass cls, ContentAddress key, StorageTier colder, FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm, Instant now, Func<BlobTransferFact, IO<Unit>> sink) {
        BlobHandle handle = BlobName.Handle(key, client.Tenant, cls, ObjectCodec.Identity, 0L);
        return worm.TryGetValue(key, out (string Mode, Instant Until) held) && now < held.Until
            ? IO.fail<Unit>(new RemoteStoreFault.Locked(key, held.Mode, held.Until))
            : store.Head(client, handle).Bind(present => present.Match(Some: resident => resident.Tier == colder, None: static () => false)
                ? sink(new BlobTransferFact(store.Key, "lifecycle-noop", key, 0L, 0, None))
                : from _ in store.Transition(client, handle, colder, now)
                  from _fact in sink(new BlobTransferFact(store.Key, "tier", key, 0L, 0, None))
                  select unit);
    }

    // WORM-aware evict arrow the blob lane injects into the retention `Execute`, SET-shaped so the verdict group leaves
    // through the row's own `EraseMany` paging instead of one `Delete` per key. Holding is a PURE predicate over the
    // catalog's WORM index, so it partitions the group BEFORE any call: a key whose row carries an ACTIVE `WormUntil`
    // (`now < until`) is provider-immutable and refuses on the tally under `RemoteStoreFault.Locked`'s own name with no
    // request sent, while everything else goes out in one batched erase. Refusing per key rather than failing the rail
    // is what keeps the sweep's two failure grains separate — a transport failure still kills the pass, a compliance
    // window costs one key. A retention-block and an auth-denial are indistinguishable by provider status, which is why
    // this domain check runs at all rather than eating an opaque 403 the `Lift` folds to `Denied`. Retention's
    // `eligible` predicate ALSO holds a locked key, so this arrow is the defense-in-depth second gate: eligibility keeps
    // the sweep from SELECTING a locked blob, this arrow refuses to EXECUTE one whose compliance window landed after the
    // verdict was computed, and one catalog column is the single source both read.
    static Func<Seq<ContentAddress>, IO<EraseTally>> WormEvict(ObjectStore store, ObjectClient client, RetentionClass cls, FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm, Instant now) =>
        keys => {
            bool Locked(ContentAddress key) => worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until;
            Seq<ContentAddress> held = keys.Filter(Locked);
            Seq<ContentAddress> free = keys.Filter(key => !Locked(key));
            EraseTally refused = new(held.Count, held.Map(static key => (Key: key, Code: nameof(RemoteStoreFault.Locked))));
            return free.IsEmpty
                ? IO.pure(refused)
                : store.EraseMany(client, free.Map(key => BlobName.Handle(key, client.Tenant, cls, ObjectCodec.Identity, 0L))).Map(page => refused + page);
        };

    // The reclaim is NOT a second sweeper: the catalog IS the authoritative inventory (a `store.List()`-then-`Filter` is the
    // deleted parallel executor), each row projects to a `RetentionFact`, and the WHOLE decision routes through the
    // `Version/retention#SWEEP_AND_GC` `RetentionSweep` — `Run` over the `blob` class with the full-history `reachable`
    // mark (a blob a historical AS-OF cut references exits first), the `holds`, `InFlightFence` AND the WORM-active keys
    // as the injected `eligible` predicate (a locked blob is ineligible like an in-flight write), then `Execute` with the
    // set-shaped `WormEvict` arrow (one batched erase per class group, still-locked keys refused on its tally) AND
    // `Demote` as the cold-tier header rewrite (`blob` is `NeverEvict`, so an aged reachable blob `Cool`s one rung
    // instead of collecting) — so orphan reclaim, cold-tiering, and snapshot-spine reclaim share ONE executor and ONE
    // receipt ledger, this lane owning only its fact projection, its in-flight + WORM fence, and its tier transition
    // (the injected `sink` carries each demotion's transfer facts through the one receipt path).
    public static IO<Seq<SweepReceipt>> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Reachability reachable, Seq<Hold> holds, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) {
        FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm = catalog.Choose(static r => r.WormUntil.Map(u => (r.Key, (Mode: "worm", Until: u)))).ToFrozenDictionary(static t => t.Key, static t => t.Item2);
        Instant now = frame.Now();
        Func<ContentAddress, bool> fence = InFlightFence(pending, now);
        Func<ContentAddress, bool> eligible = key => fence(key) && !(worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until);
        return toSeq(catalog.GroupBy(static row => row.Class)).TraverseM(group =>
            RetentionSweep.Execute(
                group.Key,
                RetentionSweep.Run(group.Key, toSeq(group).Map(ToFact), holds, reachable, eligible, now, frame.Correlation).Verdicts,
                WormEvict(store, client, group.Key, worm, now),
                (key, tier) => Demote(store, client, group.Key, key, tier, worm, now, sink),
                frame)).As();
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
|  [08]   | tenancy          | `Tenant` column + RLS-filtered catalog           | tenant name segment; cross-tenant reclaim unrepresentable          |
|  [09]   | asset class      | `ArtifactKind` column; `Class => Kind.Retention` | one axis both catalogs share; a stored class cannot contradict it  |
|  [10]   | sweep partition  | one pass per class present, receipt per pass     | per-class budgets; a mixed inventory never rides one ceiling       |
|  [11]   | admitted stamps  | kind, classification, lineage from the caller    | absence of evidence is not clearance; the lineage column is filled |
|  [12]   | class segment    | name LEADS with `RetentionClass`, tenant inside  | one prefix rule per class over every tenant; membership immutable  |
|  [13]   | lifecycle rules  | `LifecycleRules.Project` over declared schedule  | expiry + rungs from `AgeBound`; count and size stay the sweep's    |
|  [14]   | cold-tier move   | `Demote` through the `Transition` leg slot       | a rung is a header rewrite; the payload re-PUT is deleted          |
|  [15]   | demote fence     | WORM gate beside the observation gate            | a locked key refuses `Locked`, never a mis-folded provider denial  |
|  [16]   | residence form   | `Codec`/`Plain` beside `Bytes` on the row        | budget on stored bytes; a reader still knows the plaintext extent  |
|  [17]   | evict grain      | set-shaped `WormEvict` over the row `EraseBatch` | held keys refuse on the tally; a transport failure kills the pass  |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
