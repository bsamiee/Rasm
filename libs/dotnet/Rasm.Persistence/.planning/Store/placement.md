# [PERSISTENCE_STORE_PLACEMENT]

Rasm.Persistence models the FORM stored bytes take between a caller's plaintext and a provider's object, and the write STANCE that form is sealed under. `ObjectChecksum` separates the transport digest a provider verifies over what it holds from the identity digest the domain folds over what the caller addressed — two columns, never one, so no fence asserts either from the other. `ObjectCodec` and `ObjectEncryption` are the two storage stages in one fixed order, codec then seal, both framing at the SAME `ChunkPolicy.Max` stride so one window resolves through both with no second policy and a ranged read transfers the frames it spans rather than the whole object. `StorageTier` is the storage-class axis, `Rung` the observed-versus-declared carrier that keeps a realized class distinguishable from an assumed one, and `Extent` the stored-versus-plaintext length pair the two stages drive apart the moment either frames. `ObjectLock` is the WORM stance and `StanceSeat` names where a provider ENFORCES one, so a column a row cannot hold is refused at composition rather than stamped as a window nothing keeps.

`ChunkPolicy` composes from `Element/codec#CONTENT_CHUNKING` and is the ONE stride both storage stages read; `ContentAddress` and `ContentHash.Halves` from `Element/codec#CONTENT_ADDRESS` and the kernel identity surface; `EnvelopeKeyring`/`EnvelopeAad`/`WrappedKey` from `Element/identity#KMS_CUSTODY`, the DEK-wrapping lifecycle staying that owner's. `RemoteStoreFault` and its band come from `Store/redrive#FAULT_BAND`; `BlobPlacement`, `ObjectStore`, and `ObjectIo.Drain` from `Store/blobstore#OBJECT_STORE` and `#TRANSFER`, which compose every owner here. Content keys cover PLAINTEXT bytes: keying either stage's output forks one payload's address per codec row and per level, which is the identity fork the pass-through row exists to refuse.

## [01]-[INDEX]

- [02]-[STORAGE_FORM]: `FrameWindow` and `Extent` the shared frame arithmetic and length pair, `ObjectChecksum` the two-column integrity stance, `ObjectCodec` with its self-describing `CodecFrame` directory, `StorageTier` with the `Rung` observation carrier, and `ObjectEncryption` with its per-frame AEAD `SealFrame` seal and its window-resolving read.
- [03]-[WRITE_STANCE]: `StanceSeat` naming where a provider row ENFORCES a stance, and `ObjectLock` the WORM/object-lock/legal-hold family with its one deadline projection, its absorbed GCS retention vocabulary, and its four per-provider apply arms.

## [02]-[STORAGE_FORM]

- Owner: `ObjectChecksum` the `[SmartEnum<string>]` integrity stance carrying the per-provider transport columns beside the one identity claim; `ObjectCodec` the `[SmartEnum<string>]` stored-form roster owning both directions and its `CodecFrame` directory arithmetic; `StorageTier` the `[SmartEnum<string>]` storage-class axis with its per-provider columns and its `Observed` reverse; `Rung` the observed-versus-declared class carrier; `Extent` the stored-versus-plaintext length pair; `FrameWindow` the one window shape both stages return; `ObjectEncryption` the `[Union]` SSE stance and client seal with its `SealFrame` fixed-stride AEAD arithmetic.
- Cases: `ObjectChecksum` is `XxHash128`/`Crc64`/`Crc32c`/`None`, only the first answering the identity claim with one value; `ObjectCodec` is `Identity`/`Zstd`/`Lz4`, closing at pass-through beside the low-latency and high-ratio rows; `StorageTier` is `Standard`/`Infrequent`/`Cold`/`Archive`; `Rung` is `Realized` (the provider stated a class) and `Assumed` (it stated none, so the row's declared default stands); `ObjectEncryption` is `ProviderManaged`/`ManagedKey`/`CustomerKey`/`ClientSealed`, the last reaching the zero-trust storage no SSE stance does.
- Law: the content key covers PLAINTEXT bytes and every storage form frames the STORED bytes beneath, so the two integrity claims separate structurally — the provider's own digest proves transport over what it holds, the domain fold proves identity over what the caller addressed. Asserting either from the other is the substitution the two columns exist to make unspellable.
- Law: `Rung` names WHOSE claim a storage class is; a `Head` that folds an observed class and a declared default into one `StorageTier` cannot tell a realized rung from an assumed one, and the `Store/blobgc#BLOB_GC` `Demote` gate reads exactly that difference — a provider stating no class returns the row's own entry rung, so an assumed rung equal to the target suppresses a transition that never happened.
- Law: `Extent` states the STORED length and the PLAINTEXT length as two columns. They coincide only on a pass-through row under no client seal and diverge the moment either stage frames, so a reader deriving one from the other measured a ratio nobody took; `Passthrough` is the named case an object written before the form existed reads back as, and it is true for exactly that pair.
- Law: ONE stride serves both stages. `CodecFrame` and `SealFrame` both derive from the row's own `ChunkPolicy.Max` and both return `FrameWindow`, so a window resolves through the codec directory and the seal arithmetic as one composition rather than two conventions; the seal frames PER FRAME and never per object: a whole-object seal forced a ranged read to fetch and open the entire blob before slicing, degrading the one-hop partial fetch on exactly the heaviest payloads.
- Law: the fixed seal stride and the content-defined chunk cut serve different questions and never substitute — the content cut buys cross-payload dedup, the fixed frame buys constant-time offset arithmetic, so a window resolves to a frame span with no per-object manifest to carry.
- Exemption: the `CodecFrame.Window` prefix sum is an inherently sequential accumulation and the platform-forced statement boundary; the `Pack`/`Unpack` frame walks and the `SealSource`/`OpenSource` AEAD walks are likewise, the AEAD nonce ordinal and the pooled scratch reuse being inexpressible as a fold.
- Entry: `ObjectCodec.Pack`/`Unpack` own both directions over one pooled scratch, `Pack` choosing the smaller of encoded and verbatim per frame and recording what the encoder DID rather than what the row asked for; `Observed` is metadata's reverse over every row, an unstated or unmapped value falling back to `Identity` — the only safe fallback, being the only row whose stored bytes need no decode; `StorageTier.Observed` reverses the three per-provider columns through ONE entry and `Rung.Of` seats its verdict; `ObjectEncryption.SealSource`/`OpenSource` seal and open a CONTIGUOUS RUN so decrypt cost tracks the window; `Read` resolves a plaintext window to a sealed byte window and slices after the open.
- Auto: the AEAD nonce folds the frame ORDINAL into its low word so no two frames of one object reuse a nonce, its key half composing the kernel `ContentHash.Halves` `High` word; the DEK zeroizes once after the whole walk rather than per frame, because `Acquire` is the content-key CAS and re-acquiring per frame multiplies the keyring round trip by the object's frame count. Every writer for one address receives the same wrapped DEK, so a resume replays identical ciphertext and a race catalogs only one wrapped DEK.
- Boundary: the storage form rides the provider's own user-metadata dictionary — the ONE place a stored fact survives every copy, tier change, and lifecycle transition — so the writer declares it and `Head` observes it, and a caller-supplied codec on the read path is the deleted form because it lets a reader name a form the writer never used. SSE key MATERIAL is a key-id string this lane only stamps on the wire; both cloud-KMS keyrings and the DEK-wrapping lifecycle belong to `Element/authority#AUTHORITY`, never a blob-lane-local KMS wrap.
- Packages: ZstdSharp.Port and K4os.Compression.LZ4[.Streams] (the codec rows' encoder pairs), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.IO.Hashing, System.Security.Cryptography inbox (`AesGcm`/`CryptographicOperations.ZeroMemory`), AWSSDK.S3 (`ChecksumAlgorithm`/`ChecksumMode`/`S3StorageClass`), Azure.Storage.Blobs + Azure.Storage.Common (`AccessTier`, `DownloadTransferValidationOptions`/`StorageChecksumAlgorithm`), Google.Cloud.Storage.V1 (`DownloadValidationMode`, `UploadObjectOptions`), Minio (`PutObjectArgs`, `IServerSideEncryption`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new stored form is one `ObjectCodec` row carrying its encoder, decoder, level, and metadata spelling, with zero leg edits; a new storage class is one `StorageTier` row; a new SSE stance is one `ObjectEncryption` case (`ClientSealed` exercised it — one case, one seal/open pair, one catalog column); a new checksum posture is one `ObjectChecksum` row answering the whole read and write column family; a tighter window is one `ChunkPolicy` row at its own owner; a second chunker, a re-declared frame width, a direction-split codec sibling, a per-provider decoder, or a stored version column beside the self-describing directory is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct FrameWindow(long Start, long End, long Skip);

public readonly record struct Extent(long Stored, long Plain) {
    public static Extent Passthrough(long both) => new(both, both);
    public bool Coincides => Stored == Plain;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectChecksum {
    public static readonly ObjectChecksum XxHash128 = new("xxh128", ChecksumAlgorithm.XXHASH128, StorageChecksumAlgorithm.None, DownloadValidationMode.Never, identity: true);
    public static readonly ObjectChecksum Crc64 = new("crc64", ChecksumAlgorithm.CRC64NVME, StorageChecksumAlgorithm.StorageCrc64, DownloadValidationMode.Never, identity: false);
    public static readonly ObjectChecksum Crc32c = new("crc32c", null, StorageChecksumAlgorithm.None, DownloadValidationMode.Always, identity: false);
    public static readonly ObjectChecksum None = new("none", null, StorageChecksumAlgorithm.None, DownloadValidationMode.Never, identity: false);
    public ChecksumAlgorithm? S3Algorithm { get; }
    public StorageChecksumAlgorithm AzureAlgorithm { get; }
    public DownloadValidationMode GcsValidation { get; }
    public bool Identity { get; }
    private ObjectChecksum(string key, ChecksumAlgorithm? s3Algorithm, StorageChecksumAlgorithm azureAlgorithm, DownloadValidationMode gcsValidation, bool identity) : this(key) =>
        (S3Algorithm, AzureAlgorithm, GcsValidation, Identity) = (s3Algorithm, azureAlgorithm, gcsValidation, identity);

    public Option<string> Wire(ContentAddress key) {
        byte[] digest = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(digest, key.ToValue());
        return this == XxHash128 ? Some(Convert.ToBase64String(digest)) : None;
    }

    public static ReadOnlyMemory<byte> Azure(ReadOnlySpan<byte> payload) {
        byte[] digest = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(digest, System.IO.Hashing.Crc64.HashToUInt64(payload));
        return digest;
    }

    public GetObjectRequest ApplyS3(GetObjectRequest request) =>
        (request.ChecksumMode = S3Algorithm is null ? null : ChecksumMode.ENABLED, request).Item2;

    public BlobDownloadOptions ApplyAzure(BlobDownloadOptions options) =>
        (options.TransferValidation = new DownloadTransferValidationOptions { ChecksumAlgorithm = AzureAlgorithm, AutoValidateChecksum = true }, options).Item2;

    public DownloadObjectOptions ApplyGcs(DownloadObjectOptions options) =>
        (options.DownloadValidationMode = GcsValidation, options).Item2;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectCodec {
    public static readonly ObjectCodec Identity = new("identity", static sink => sink, static source => source, level: 0);
    public static readonly ObjectCodec Zstd = new("zstd",
        static sink => new ZstdSharp.CompressionStream(sink, level: 3, bufferSize: 0, leaveOpen: true),
        static source => new ZstdSharp.DecompressionStream(source, bufferSize: 0, checkEndOfStream: true, leaveOpen: true), level: 3);
    public static readonly ObjectCodec Lz4 = new("lz4",
        static sink => LZ4Stream.Encode(sink, LZ4Level.L09_HC, extraMemory: 0, leaveOpen: true),
        static source => LZ4Stream.Decode(source, extraMemory: 0, leaveOpen: true, interactive: false), level: (int)LZ4Level.L09_HC);
    public const string CodecKey = "rasm-codec";
    public const string PlainKey = "rasm-plain";
    public Func<Stream, Stream> Encoder { get; }
    public Func<Stream, Stream> Decoder { get; }
    public int Level { get; }
    private ObjectCodec(string key, Func<Stream, Stream> encoder, Func<Stream, Stream> decoder, int level) : this(key) =>
        (Encoder, Decoder, Level) = (encoder, decoder, level);

    public static ObjectCodec Observed(string? stated) =>
        toSeq(Items).Find(row => row.Key.Equals(stated, StringComparison.OrdinalIgnoreCase)).IfNone(Identity);

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
        public FrameWindow Window(ReadOnlySpan<byte> directory, long plainStart, long plainEnd) {
            long first = plainStart / Stride, last = plainEnd / Stride, start = Directory, end = Directory;
            for (long ordinal = 0; ordinal <= last; ordinal++) {
                if (ordinal < first) start += Length(directory, ordinal);
                end += Length(directory, ordinal);
            }
            return new FrameWindow(start, end - 1, plainStart - (first * Stride));
        }
    }

    public IO<ReadOnlySequence<byte>> Pack(ChunkPolicy policy, ReadOnlySequence<byte> plain) =>
        this == Identity
            ? IO.pure(plain)
            : IO.lift(() => Try.lift(() => {
                CodecFrame frame = CodecFrame.Of(policy, plain.Length);
                byte[] packed = new byte[frame.Directory + plain.Length];
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
                return Fin<ReadOnlySequence<byte>>.Succ(new ReadOnlySequence<byte>(packed.AsMemory(0, (int)at)));
            }).Run().Bind(static inner => inner));

    public IO<ReadOnlyMemory<byte>> Unpack(ChunkPolicy policy, long plain, ReadOnlyMemory<byte> directory, long ordinal, ReadOnlySequence<byte> run) =>
        this == Identity
            ? IO.pure(run.IsSingleSegment ? run.First : run.ToArray())
            : IO.lift(() => Try.lift(() => {
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
                return Fin<ReadOnlyMemory<byte>>.Succ(opened.AsMemory(0, (int)wrote));
            }).Run().Bind(static inner => inner));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StorageTier {
    public static readonly StorageTier Standard = new("standard", S3StorageClass.Standard, AccessTier.Hot, "STANDARD");
    public static readonly StorageTier Infrequent = new("infrequent", S3StorageClass.StandardInfrequentAccess, AccessTier.Cool, "NEARLINE");
    public static readonly StorageTier Cold = new("cold", S3StorageClass.GlacierInstantRetrieval, AccessTier.Cold, "COLDLINE");
    public static readonly StorageTier Archive = new("archive", S3StorageClass.DeepArchive, AccessTier.Archive, "ARCHIVE");
    public S3StorageClass S3Class { get; }
    public AccessTier AzureTier { get; }
    public string GcsClass { get; }
    private StorageTier(string key, S3StorageClass s3Class, AccessTier azureTier, string gcsClass) : this(key) =>
        (S3Class, AzureTier, GcsClass) = (s3Class, azureTier, gcsClass);

    public static Option<StorageTier> Observed(string? stated) =>
        string.IsNullOrEmpty(stated)
            ? None
            : toSeq(Items).Find(row => stated.Equals(row.S3Class.Value, StringComparison.OrdinalIgnoreCase)
                                    || stated.Equals(row.AzureTier.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || stated.Equals(row.GcsClass, StringComparison.OrdinalIgnoreCase));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Rung {
    private Rung() { }
    public sealed record Realized(StorageTier Tier) : Rung;
    public sealed record Assumed(StorageTier Tier) : Rung;
    public StorageTier Class => Switch(realized: static r => r.Tier, assumed: static a => a.Tier);
    public static Rung Of(Option<StorageTier> observed, StorageTier declared) =>
        observed.Match(Some: static tier => (Rung)new Realized(tier), None: () => new Assumed(declared));
}

[Union]
public abstract partial record ObjectEncryption {
    public sealed record ProviderManaged : ObjectEncryption;
    public sealed record ManagedKey(string KeyId, FrozenDictionary<string, string> Aad) : ObjectEncryption;
    public sealed record CustomerKey(ReadOnlyMemory<byte> Key, string KeyMd5) : ObjectEncryption;
    public sealed record ClientSealed(
        EnvelopeKeyring Keyring,
        EnvelopeAad Aad,
        Func<ContentAddress, IO<(ReadOnlyMemory<byte> Dek, WrappedKey Wrapped)>> Acquire) : ObjectEncryption;

    public InitiateMultipartUploadRequest ApplyS3(InitiateMultipartUploadRequest request) => Switch(
        providerManaged: static (r, _) => r,
        managedKey:      static (r, k) => (r.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                                           r.ServerSideEncryptionKeyManagementServiceKeyId = k.KeyId, r).Item3,
        customerKey:     static (r, c) => (r.ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.AES256,
                                           r.ServerSideEncryptionCustomerProvidedKey = Convert.ToBase64String(c.Key.Span),
                                           r.ServerSideEncryptionCustomerProvidedKeyMD5 = c.KeyMd5, r).Item4,
        clientSealed:    static (r, _) => r,
        state: request);

    public UploadObjectOptions ApplyGcs(UploadObjectOptions options) => Switch(
        providerManaged: static (o, _) => o,
        managedKey:      static (o, k) => (o.KmsKeyName = k.KeyId, o).Item2,
        customerKey:     static (o, _) => o,
        clientSealed:    static (o, _) => o,
        state: options);

    public PutObjectArgs ApplyMinio(PutObjectArgs args) => Switch(
        providerManaged: static (a, _) => a,
        managedKey:      static (a, k) => a.WithServerSideEncryption(new SSEKMS(k.KeyId)),
        customerKey:     static (a, c) => a.WithServerSideEncryption(new SSEC(c.Key.ToArray())),
        clientSealed:    static (a, _) => a,
        state: args);

    public readonly record struct SealFrame(long Stride) {
        public const int Overhead = 12 + 16;
        public static SealFrame Of(ChunkPolicy policy) => new(policy.Max);
        public long Sealed(long plain) => plain + (Count(plain) * Overhead);
        public long Plain(long sealedLength) => sealedLength - (Frames(sealedLength) * Overhead);
        public long Count(long plain) => (plain + Stride - 1) / Stride;
        long Frames(long sealedLength) => (sealedLength + Stride + Overhead - 1) / (Stride + Overhead);
        public FrameWindow Window(long plainStart, long plainEnd) {
            long first = plainStart / Stride;
            long last = plainEnd / Stride;
            return new FrameWindow(first * (Stride + Overhead), ((last + 1) * (Stride + Overhead)) - 1, plainStart - (first * Stride));
        }
    }

    public IO<(ReadOnlySequence<byte> Bytes, Option<WrappedKey> Dek)> SealSource(ContentAddress key, ChunkPolicy policy, ReadOnlySequence<byte> plain) =>
        this is ClientSealed sealed_
            ? sealed_.Acquire(key).Map(minted => {
                SealFrame frame = SealFrame.Of(policy);
                byte[] framed = new byte[frame.Sealed(plain.Length)];
                try {
                    using System.Security.Cryptography.AesGcm aead = new(minted.Dek.Span, tagSizeInBytes: 16);
                    for (long ordinal = 0; ordinal < frame.Count(plain.Length); ordinal++) {
                        long at = ordinal * frame.Stride;
                        int span = (int)long.Min(frame.Stride, plain.Length - at);
                        Span<byte> slot = framed.AsSpan((int)(ordinal * (frame.Stride + SealFrame.Overhead)));
                        BinaryPrimitives.WriteUInt64BigEndian(slot[..8], ContentHash.Halves(key.ToValue()).High);
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

    public IO<ReadOnlyMemory<byte>> OpenSource(ContentAddress content, ChunkPolicy policy, long ordinal, ReadOnlySequence<byte> framed, Option<WrappedKey> dek) =>
        (this, dek) switch {
            (ClientSealed, { IsNone: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-envelope")),
            (ClientSealed, _) when framed.Length < SealFrame.Overhead => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "client-seal-frame")),
            (ClientSealed sealed_, { IsSome: true, Case: WrappedKey key }) => sealed_.Keyring.Unwrap(sealed_.Aad).Map(opened => {
                SealFrame frame = SealFrame.Of(policy);
                byte[] run = framed.ToArray();
                byte[] plain = new byte[frame.Plain(run.LongLength)];
                try {
                    using System.Security.Cryptography.AesGcm aead = new(opened.Span, tagSizeInBytes: 16);
                    for (long index = 0; index * (frame.Stride + SealFrame.Overhead) < run.LongLength; index++) {
                        int at = (int)(index * (frame.Stride + SealFrame.Overhead));
                        int span = (int)long.Min(frame.Stride, run.LongLength - at - SealFrame.Overhead);
                        Span<byte> slot = run.AsSpan(at);
                        BinaryPrimitives.WriteUInt32BigEndian(slot.Slice(8, 4), (uint)(ordinal + index));
                        aead.Decrypt(slot[..12], slot.Slice(SealFrame.Overhead, span), slot.Slice(12, 16), plain.AsSpan((int)(index * frame.Stride), span));
                    }
                }
                finally {
                    System.Security.Cryptography.CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsMemory(opened).Span);
                }
                return (ReadOnlyMemory<byte>)plain;
            }),
            (_, { IsSome: true }) => IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(content, "unexpected-envelope")),
            _ => IO.pure(framed.IsSingleSegment ? framed.First : framed.ToArray()),
        };

    public IO<Stream> Read(
        ContentAddress key,
        ChunkPolicy policy,
        Option<(long Start, long End)> range,
        Func<ContentAddress, IO<Option<WrappedKey>>> envelope,
        Func<ContentAddress, IO<Option<BlobPlacement>>> stat,
        Func<Option<(long Start, long End)>, IO<Stream>> fetch) =>
        (this, range) switch {
            (ClientSealed, { IsSome: true, Case: (long Start, long End) window }) =>
                from present in stat(key)
                from resident in IO.lift(present.ToFin(new RemoteStoreFault.NotFound(key)))
                let frame = SealFrame.Of(policy)
                let plainLength = frame.Plain(resident.Extent.Stored)
                from bounded in window is { Start: >= 0 } && window.End >= window.Start && window.End < plainLength
                    ? IO.pure(frame.Window(window.Start, window.End))
                    : IO.fail<FrameWindow>(new RemoteStoreFault.InvalidRange(key, window.Start, window.End, plainLength))
                from dek in envelope(key)
                from raw in fetch(Some((bounded.Start, long.Min(bounded.End, resident.Extent.Stored - 1))))
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
```

## [03]-[WRITE_STANCE]

- Owner: `StanceSeat` the `[SmartEnum<string>]` rung naming where a provider row ENFORCES a write stance, one roster answering both the WORM and the storage-class columns; `ObjectLock` the `[Union]` WORM stance owning its one deadline projection, its releasability column, and its four per-provider apply arms.
- Cases: `StanceSeat` is `Request` (the initiate or put the leg already composes), `Followup` (a per-object call the leg makes AFTER the seal, the object having to exist before its policy binds), `Container` (the container or bucket the host dialed), and `None`; `ObjectLock` is `Off`, `Governance`/`Compliance` carrying a `Retain` window, and `LegalHold` the indefinite hold an operator action releases rather than a lapsing window.
- Law: `Holds` separates a stance a row can enforce from a column it can only claim. `Container` is honored where the host dialed it and unprovable from a leg, so it satisfies admission while a `Followup` row proves its own application; no provider row seats `Container` today — every provider that can hold a lock proves it per object — and the rung stays as the seat a host-dialed-only provider takes, never a rung the roster outgrew.
- Law: `Until` is the ONE deadline derivation the catalog column and every apply arm read, and it derives from the ONE frame instant the upload sampled; a per-arm ambient clock read is the two-clock split-brain that lets the provider window and the catalog window diverge under skew or retry.
- Law: a stance admits only where the row's seat ENFORCES it. `Until` projects unconditionally into the catalog WORM column the GC fence reads, so a stance set on a row seating none stamped a retention window no provider holds and made the blob permanently un-evictable on a fiction.
- Entry: `ApplyS3` and `ApplyMinio` stamp the stance on the REQUEST the leg already composes; `ApplyAzure` and `ApplyGcs` yield an `Option` of the per-object call the `Followup` rung schedules after the seal, so an unstanced write pays no round trip and a stanced one lands its policy on the sealed object.
- Boundary: `Object.RetentionData.Mode` closes the GCS per-object retention vocabulary at exactly the governance/compliance split every other provider spells, so it rides this union's own projection rather than a two-row roster with one caller; the GCS window rides `Object.Retention` applied through `PatchObjectAsync`, never `Object.RetentionExpirationTimeRaw`, which is the read-only BUCKET-policy expiry and a different field entirely; `OverrideUnlockedRetention` arms only on the releasable mode, so a clock-skewed re-apply that shortens an existing unlocked window is refused while a locked window admits no override and needs none.
- Packages: AWSSDK.S3 (`ObjectLockMode`/`ObjectLockRetainUntilDate`/`ObjectLockLegalHoldStatus`), Azure.Storage.Blobs (`SetImmutabilityPolicyAsync`/`SetLegalHoldAsync`/`DeleteImmutabilityPolicyAsync`, `BlobImmutabilityPolicy`), Google.Cloud.Storage.V1 (`PatchObjectAsync` + `PatchObjectOptions.OverrideUnlockedRetention`), Minio (`ObjectWriteArgs.WithRetentionConfiguration`/`WithLegalHold`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new WORM stance is one `ObjectLock` case admitted only where the row's seat can hold it, read by the deadline projection and the GC evict arrow alike with zero new surface; a new enforcement rung is one `StanceSeat` row; a decorative lock column promised only in prose, a per-provider seat roster, or a no-op arm silently dropping a declared column is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StanceSeat {
    public static readonly StanceSeat Request = new("request", holds: true);
    public static readonly StanceSeat Followup = new("followup", holds: true);
    public static readonly StanceSeat Container = new("container", holds: true);
    public static readonly StanceSeat None = new("none", holds: false);
    public bool Holds { get; }
    private StanceSeat(string key, bool holds) : this(key) => Holds = holds;
}

[Union]
public abstract partial record ObjectLock {
    public sealed record Off : ObjectLock;
    public sealed record Governance(Duration Retain) : ObjectLock;
    public sealed record Compliance(Duration Retain) : ObjectLock;
    public sealed record LegalHold : ObjectLock;

    public Option<Instant> Until(Instant now) => Map(
        off:        static (_, _) => Option<Instant>.None,
        governance: static (at, c) => Some(at + c.Retain),
        compliance: static (at, c) => Some(at + c.Retain),
        legalHold:  static (_, _) => Some(Instant.MaxValue),
        state: now);

    public bool Releasable => this is Off or Governance;
    public string GcsMode => Releasable ? "Unlocked" : "Locked";

    public InitiateMultipartUploadRequest ApplyS3(InitiateMultipartUploadRequest request, Instant now) => Switch(
        off:        static (s, _) => s.Request,
        governance: static (s, c) => (s.Request.ObjectLockMode = ObjectLockMode.Governance,
                                      s.Request.ObjectLockRetainUntilDate = (s.Now + c.Retain).ToDateTimeUtc(), s.Request).Item3,
        compliance: static (s, c) => (s.Request.ObjectLockMode = ObjectLockMode.Compliance,
                                      s.Request.ObjectLockRetainUntilDate = (s.Now + c.Retain).ToDateTimeUtc(), s.Request).Item3,
        legalHold:  static (s, _) => (s.Request.ObjectLockLegalHoldStatus = ObjectLockLegalHoldStatus.On, s.Request).Item2,
        state: (Request: request, Now: now));

    public Option<Func<BlobBaseClient, Instant, Task>> ApplyAzure(Instant now) => Map(
        off:        static (_, _) => Option<Func<BlobBaseClient, Instant, Task>>.None,
        governance: static (at, c) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetImmutabilityPolicyAsync(
            new BlobImmutabilityPolicy { ExpiresOn = (at + c.Retain).ToDateTimeOffset(), PolicyMode = BlobImmutabilityPolicyMode.Unlocked })),
        compliance: static (at, c) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetImmutabilityPolicyAsync(
            new BlobImmutabilityPolicy { ExpiresOn = (at + c.Retain).ToDateTimeOffset(), PolicyMode = BlobImmutabilityPolicyMode.Locked })),
        legalHold:  static (_, _) => Some<Func<BlobBaseClient, Instant, Task>>((blob, _) => blob.SetLegalHoldAsync(true)),
        state: now);

    public Option<Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task>> ApplyGcs(Instant now) => Map(
        off:        static (_, _) => Option<Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task>>.None,
        governance: (at, c) => Some(Patch(this, at + c.Retain)),
        compliance: (at, c) => Some(Patch(this, at + c.Retain)),
        legalHold:  (_, _) => Some(Patch(this, Instant.MaxValue)),
        state: now);

    static Func<StorageClient, Google.Apis.Storage.v1.Data.Object, Instant, Task> Patch(ObjectLock stance, Instant until) =>
        (client, resource, _) => client.PatchObjectAsync(
            (resource.Retention = new Google.Apis.Storage.v1.Data.Object.RetentionData { Mode = stance.GcsMode, RetainUntilTimeDateTimeOffset = until.ToDateTimeOffset() }, resource).Item2,
            new PatchObjectOptions { OverrideUnlockedRetention = stance.Releasable });

    public PutObjectArgs ApplyMinio(PutObjectArgs args, Instant now) => Switch(
        off:        static (s, _) => s.Args,
        governance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.GOVERNANCE)),
        compliance: static (s, c) => s.Args.WithRetentionConfiguration(new ObjectRetentionConfiguration((s.Now + c.Retain).ToDateTimeUtc(), ObjectRetentionMode.COMPLIANCE)),
        legalHold:  static (s, _) => s.Args.WithLegalHold(true),
        state: (Args: args, Now: now));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                       | [BINDING]                                                            |
| :-----: | :--------------- | :-------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | storage form     | `ObjectCodec` row, then the seal              | the key covers plaintext; the form rides object metadata             |
|  [02]   | integrity claims | transport column beside `Identity` column     | no fence asserts either claim from the other                         |
|  [03]   | one stride       | `ChunkPolicy.Max` frames codec and seal alike | one `FrameWindow` resolves both stages; no second policy             |
|  [04]   | framed seal      | one AES-GCM frame per stride, ordinal nonce   | a ranged sealed read opens the frames it spans, never the whole blob |
|  [05]   | self-describing  | directory prefix sums inside the object       | no sidecar rides the catalog; nothing drifts from the bytes          |
|  [06]   | verbatim frame   | high bit records what the encoder DID         | an incompressible frame keeps its own bytes and says so              |
|  [07]   | observed rung    | `Rung.Realized` versus `Rung.Assumed`         | a demote gate tells a realized class from the row's declared default |
|  [08]   | length pair      | `Extent(Stored, Plain)`                       | they coincide on pass-through alone; `Passthrough` is the named case |
|  [09]   | stance seat      | one `StanceSeat` per stance column            | every rung but `None` enforces; only the grant plane holds none      |
|  [10]   | one WORM clock   | `Until` off the ONE sampled frame instant     | provider retention date and catalog window derive from it            |
|  [11]   | GCS retention    | `Releasable`/`GcsMode` off the stance itself  | the override arms on the releasable mode alone                       |
|  [12]   | DEK custody      | keyring mint, zeroize once per walk           | provider-held keys never see plaintext; the wrap is Authority's      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
