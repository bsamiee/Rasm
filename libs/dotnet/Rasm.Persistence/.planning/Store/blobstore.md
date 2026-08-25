# [PERSISTENCE_STORE_BLOBSTORE]

Rasm.Persistence stores every admitted artifact class as content-keyed object bytes — one `ObjectStore` `[SmartEnum]` provider axis behind the `BlobRemote` placement contract, five rows deep (`S3`/`Azure`/`GCS`/`Minio` credentialed, the credential-free `Presigned` grant), every write content-addressed, write-once-sealed through the conditional-write `412`-noop, and routed through the one `MultipartTransfer.Upload` receipt path onto the one write-blob-first protocol. Algebra keeps the plane asset-AGNOSTIC and seats its payload families as retention-class ROWS, so a second consumer admits as one row rather than forking the plane. Object names derive from the seam `ContentAddress` the kernel `XxHash128` mints, so this store holds bytes and never mints a second identity, and no relational engine appears because the durable home for an artifact class's bytes is the object plane. `ObjectClient` IS the dispatch: its union case resolves the fourteen-delegate `ObjectLeg` row that carries every per-provider variance, and one packing fold drives both the staged-multipart and whole-object transfer models through one seal without a mode knob.

`ContentAddress`, `ChunkPolicy`, `ChunkManifest`, and `ContentChunker` compose from `Element/codec#CONTENT_ADDRESS` and `#CONTENT_CHUNKING`; `ObjectChecksum`, `ObjectCodec`, `StorageTier`, `Rung`, `Extent`, `ObjectEncryption`, `ObjectLock`, and `StanceSeat` from `Store/residence#RESIDENCE_FORM` and `#WRITE_STANCE`; `RemoteStoreFault`, `ObjectVerb`, `StoreHop`, `StoreVerdict`, and `StoreRedrivePort` from `Store/redrive#FAULT_BAND` and `#REDRIVE_SEAM`; `BlobGc.WriteBlobFirst`, `BlobLedger`, `PendingWrite`, and `BlobCatalogRow` from `Store/blobgc#BLOB_GC`, which every placement write routes through; `RetentionClass` and `ArtifactKind` from `Version/retention#RETENTION_CLASSES`; `ProjectionContext`, `StoreSlot`, `TenantId`, `CorrelationId`, `DataClassification`, and `WrappedKey` from the `Element/graph#STORE_RAIL` frame and the kernel. `StoreCapability` is the store-plane capability roster `Store/provisioning#STORE_PROFILE` owns and this page composes for its own row columns; the roster lands with that page's own rebuild, and until it does the rows here name the capabilities they hold without a second roster beside it.

## [01]-[INDEX]

- [02]-[OBJECT_STORE]: `ObjectStore` the five-provider axis projecting `BlobRemote`, its `CapabilitySet` columns and their honest-degrade complement, the `ResidenceClaim` fold naming the one form where transport and identity coincide, the residence transform in one fixed order, the endpoint-parameterized grant mint, the closed grant and thaw vocabularies, and the `BlobFactKind` fact vocabulary the slot roster derives from.
- [03]-[TRANSFER]: `ObjectIo` the one generic transfer engine over the fourteen-delegate `ObjectLeg` row, `MultipartTransfer` the receipt-emitting write with its content-defined part packer, `BlobName` the class-leading object-name projection minting the form-bearing `BlobHandle`, the five per-provider legs, and the durable-session resume and explicit abandon the torn ceremony survives on.

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore` the `[SmartEnum<string>]` provider axis under ordinal comparison — each row carrying the part floor, per-part ceiling, part count, chunking window, integrity stance, storage class, SSE stance, WORM stance, stance seats, erase page, and the `CapabilitySet<StoreCapability>` naming what it holds — building its `BlobRemote` from the resolved `ObjectClient`, the `[Union]` whose `Map` owns per-leg dispatch; `ResidenceClaim` the three-case integrity fold; `GrantRequest`/`GrantDemand`/`ObjectGrant`/`GrantSigner` the grant plane; `ThawState` the cold-rung state family; `EraseTally` the partial-failure receipt; `BlobResidence` the realized residence report; `BlobFactKind` and `BlobTransferFact` the closed fact vocabulary; `BlobAdmission` the admitted write request; `BlobRemote` the placement-delegate bundle the app wires; `ContentBlobPort` the key-minting byte seam DERIVED off `BlobRemote` — the two-slot put/get pair a composition root binds onto an up-stack consumer's own port slots (the model-session key-minting store and the artifact-index byte read are its standing consumers), so a loose bytes-to-key delegate pair at a consumer is the deleted form.
- Cases: `s3`, `azure-blob`, `gcs`, `minio`, `presigned`, each naming where its write stances are ENFORCED and holding the capability set that decides what it can do at all. `presigned` inverts the row — a grant minter and roster pair with a host-dialed `HttpClient` replace endpoint and credential, reaching domain-cloud planes no credentialed row can because the client-side credential never exists, and single-shot by construction since the upstream roster carries no checksum, no multipart, and no resume. `ResidenceClaim` is `Framed` (a codec or seal sits between plaintext and provider), `Plain` (stored bytes ARE plaintext but the row's digest is not the key), and `Proven` (they coincide, so the digest IS the content key). `GrantRequest` is `Write`/`Read`/`Erase`; `ObjectGrant` is `FormPost`/`SignedUrl`; `ThawState` is `Resident`/`Frozen`/`Thawing`.
- Law: absence is a capability the row does not HOLD, not six spellings of the same fact; a conditional-write bool, a part count of one, an erase page of one, a seat of none, and a checksum row of none all said "this row lacks capability C" in five vocabularies no reader folds; the numeric columns keep their MAGNITUDE alone and `Degrade` is the complement of what the row holds, so the honest clause a reader selects on is derived rather than restated.
- Law: the two integrity claims coincide ONLY where stored bytes ARE the plaintext bytes, so `ResidenceClaim` admits `Proven` on exactly that form. Supplying the content key as a provider digest over framed bytes makes the provider reject a correct upload; reading a passing provider digest as proof of identity asserts a claim nobody made. Three cases make both substitutions unspellable where four independent predicates only discouraged them.
- Law: every write routes through the one composed receipt path and the one write-blob-first protocol, so a placement write opens its pending fence, emits its transfer facts under the frame's correlation, and commits its catalog row as one composition — a bare put beside the receipt engine was the orphaned-surface defect this routing deletes.
- Law: the fact vocabulary is CLOSED and the slot roster DERIVES from it, so a new kind is one row, a projection arm keyed on rows breaks loud, and no free string reaches the fact stream; the fault fact carries the SETTLED verdict itself, so the evidence surface and the re-drive decision read one value. `Store/observability#STORE_INSTRUMENTS` still keys its projection on five of the nine rows, so `session`, `conflict-noop`, `tier`, and `lifecycle-noop` reach no arm until that page mounts the roster this owner now derives.
- Entry: `Placement` projects the row's `BlobRemote`, its write arrow routed through `Store/blobgc#BLOB_GC` `WriteBlobFirst`; `Encode`/`Decode` own the residence transform in ONE fixed order, codec then seal, so no caller re-spells which frames first; `Put` drains the source once and partitions through the content chunker at the row tier alone, a tier change being `Transition` rather than a second write; `Fetch`/`Head` are the read legs, `Head` reading the realized storage class and the stored form back through their own observation entries; `Rehydrate` requests a thaw and reports the state either way; `EraseMany` chunks a group against the row's own erase page; `Grant` is the ISSUER mint, the inverse of the presigned CONSUMER row.
- Auto: content-defined chunks pack into provider parts of at least the part floor, but only the exact object-name seal proves whole-blob residence — chunk membership never short-circuits a provider that cannot assemble an object from foreign parts. Re-putting an existing key `412`s to a conflict, and one catch arm confirms the exact object by `Head` before yielding the benign no-op. Encryption, lock, and residence form all apply through the ONE stamp fold per request type — SSE first, WORM second, form last — so a leg silently dropping a column is unrepresentable. Fetching a rung the provider holds offline refuses from the provider's OWN error code, so no read pays a probing head and no thaw-requiring rung reads as a denial.
- Receipt: a `BlobTransferFact` rides its kind's own slot — a part per uploaded window, a resume per skipped-committed window, a session per opened token, a conflict no-op per exact-object `412`, a tier per realized transition, a lifecycle no-op per rung a provider rule already moved, an abort per torn ceremony, a write per admitted object, and a fault carrying the settled verdict; the erase tally carries accepted and refused as separate columns over one page; the message envelope stamps the HLC, so no fact carries an instant.
- Boundary: the content-key object name derives from the kernel identity, so the store never mints a second identity and the neutral representation map leaks no foreign schema name. Write-once is the optimistic-concurrency edge each provider exposes (S3 and Minio `IfNoneMatch:*`, Azure `ETag.All`, GCS `IfGenerationMatch:0`), so a content-address store needs no read-before-write and a `412` folds to a conflict treated as success. Every SDK exception lifts once at `#TRANSFER` `Bound`, which is also where the root-bound re-drive port wraps the crossing — this tier publishes the discriminant and executes nothing itself. Credential, endpoint, and region are host-resolved connection inputs, never fence members; the presigned row inverts the boundary, its minter closure seeing only a grant request, and only that expiry-aware minter can mint an expired-grant refusal because a bare `403` cannot distinguish expiry from signature failure or policy refusal; a write stance admits only where the row's seat ENFORCES it, so a stance set on a row seating none stamps a retention window no provider holds and makes the blob permanently un-evictable on a fiction.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Azure.Storage.Blobs.Batch, Azure.Storage.Common, Google.Cloud.Storage.V1, Minio, CommunityToolkit.HighPerformance, System.IO.Hashing, System.Collections.Frozen, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox (`HttpClient` and the multipart form content for the presigned leg's granted HTTP).
- Growth: one `ObjectStore` row absorbs a new provider with zero new surface — one row, one leg, one capability set (`presigned` exercised it); a new presigned domain is one minter value, a new grant modality one `GrantRequest` case the collapsed signer already spells a verb for, a new capability one `StoreCapability` row every row then answers; a per-provider upload service, a second presigner beside the endpoint-parameterized one, a row delegate re-discriminating the union, a second HTTP uploader, a client-type guard, or a prose degrade clause beside the capability set is the deleted form because the union case IS the dispatch.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GrantRequest {
    private GrantRequest() { }
    public sealed record Write(ContentAddress Key, long Length) : GrantRequest;
    public sealed record Read(ContentAddress Key) : GrantRequest;
    public sealed record Erase(ContentAddress Key) : GrantRequest;

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
    public Uri Url => Switch(formPost: static p => p.Url, signedUrl: static s => s.Url);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThawState {
    private ThawState() { }
    public sealed record Resident : ThawState;
    public sealed record Frozen : ThawState;
    public sealed record Thawing(Option<Instant> Ready) : ThawState;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceClaim {
    private ResidenceClaim() { }
    public sealed record Framed : ResidenceClaim;
    public sealed record Plain : ResidenceClaim;
    public sealed record Proven(ChecksumAlgorithm Algorithm, string Wire) : ResidenceClaim;
    public ChecksumAlgorithm? Supplied => this is Proven proven ? proven.Algorithm : null;
    public Option<string> Digest => this is Proven proven ? Some(proven.Wire) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BlobFactKind {
    public static readonly BlobFactKind Part = new("part", StoreSlot.Create("store.blob.part"));
    public static readonly BlobFactKind Resume = new("resume", StoreSlot.Create("store.blob.resume"));
    public static readonly BlobFactKind Abort = new("abort", StoreSlot.Create("store.blob.abort"));
    public static readonly BlobFactKind Write = new("write", StoreSlot.Create("store.blob.write"));
    public static readonly BlobFactKind Session = new("session", StoreSlot.Create("store.blob.session"));
    public static readonly BlobFactKind ConflictNoop = new("conflict-noop", StoreSlot.Create("store.blob.conflict"));
    public static readonly BlobFactKind Tier = new("tier", StoreSlot.Create("store.blob.tier"));
    public static readonly BlobFactKind LifecycleNoop = new("lifecycle-noop", StoreSlot.Create("store.blob.lifecycle"));
    public static readonly BlobFactKind Fault = new("fault", StoreSlot.Create("store.blob.fault"));
    public StoreSlot Slot { get; }
    private BlobFactKind(string key, StoreSlot slot) : this(key) => Slot = slot;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BlobStat(ContentAddress Key, long Length);

public readonly record struct EraseTally(int Requested, Seq<(ContentAddress Key, string Code)> Refused) {
    public static readonly EraseTally Empty = new(0, Seq<(ContentAddress Key, string Code)>());
    public int Erased => Requested - Refused.Count;
    public static EraseTally operator +(EraseTally left, EraseTally right) => new(left.Requested + right.Requested, left.Refused + right.Refused);
}

public readonly record struct BlobAdmission(ContentAddress Key, long Length, DataClassification Classification, Option<ContentAddress> Lineage, Option<string> Session);

public readonly record struct BlobResidence(ContentAddress Key, Extent Extent, Rung Tier, ObjectCodec Codec, int Parts, int ResumedParts, Option<ContentAddress> Verified, Option<string> ConditionToken, CorrelationId Correlation) {
    public static BlobResidence From(ContentAddress key, Extent extent, Rung tier, ObjectCodec codec) =>
        new(key, extent, tier, codec, 0, 0, None, None, CorrelationId.None);
}

public readonly record struct BlobTransferFact(ObjectStore Provider, BlobFactKind Kind, ContentAddress Key, long Bytes, int Part, Option<string> Session, Option<StoreVerdict> Settled = default);

public readonly record struct GrantSigner(IAmazonS3 Client, string Bucket) {
    public async Task<ObjectGrant> Sign(GrantDemand demand, BlobHandle handle, Instant now) =>
        new ObjectGrant.SignedUrl(new Uri(await Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest {
            BucketName = Bucket, Key = handle.Name, Verb = Verb(demand.Request), Expires = (now + demand.Lifetime).ToDateTimeUtc(),
        }).ConfigureAwait(false)));

    static HttpVerb Verb(GrantRequest request) => request.Switch(
        write: static _ => HttpVerb.PUT, read: static _ => HttpVerb.GET, erase: static _ => HttpVerb.DELETE);
}

// --- [SERVICES] ------------------------------------------------------------------------
[Union]
public abstract partial record ObjectClient {
    public required TenantId Tenant { get; init; }
    public required StoreRedrivePort Redrive { get; init; }
    public sealed record S3(IAmazonS3 Client, string Bucket, GrantSigner Signer) : ObjectClient;
    public sealed record Azure(BlobContainerClient Container) : ObjectClient;
    public sealed record Gcs(StorageClient Client, string Bucket, UrlSigner Signer) : ObjectClient;
    public sealed record Minio(IMinioClient Client, string Bucket, GrantSigner Signer) : ObjectClient;
    public sealed record Presigned(Func<GrantRequest, IO<ObjectGrant>> Minter, Func<Option<ContentAddress>, IO<Seq<BlobStat>>> Roster, HttpClient Http) : ObjectClient;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectStore {
    public static readonly ObjectStore S3 = new("s3", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 10_000, ChunkPolicy.Artifact,
        ObjectChecksum.XxHash128, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: StanceSeat.Request, tierAt: StanceSeat.Request, eraseBatch: 1_000,
        holds: CapabilitySet<StoreCapability>.Of(StoreCapability.Multipart, StoreCapability.Resume, StoreCapability.BatchErase,
            StoreCapability.Tiering, StoreCapability.Thaw, StoreCapability.PerObjectWorm, StoreCapability.Presign,
            StoreCapability.ReadChecksum, StoreCapability.ConditionalWrite));
    public static readonly ObjectStore AzureBlob = new("azure-blob", 8L * 1024 * 1024, 4000L * 1024 * 1024, 50_000, ChunkPolicy.Artifact,
        ObjectChecksum.Crc64, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: StanceSeat.Followup, tierAt: StanceSeat.Request, eraseBatch: 256,
        holds: CapabilitySet<StoreCapability>.Of(StoreCapability.Multipart, StoreCapability.Resume, StoreCapability.BatchErase,
            StoreCapability.Tiering, StoreCapability.Thaw, StoreCapability.PerObjectWorm, StoreCapability.Presign,
            StoreCapability.ReadChecksum, StoreCapability.ConditionalWrite));
    public static readonly ObjectStore Gcs = new("gcs", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 1, ChunkPolicy.Artifact,
        ObjectChecksum.Crc32c, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: StanceSeat.Followup, tierAt: StanceSeat.Request, eraseBatch: 1,
        holds: CapabilitySet<StoreCapability>.Of(StoreCapability.Tiering, StoreCapability.PerObjectWorm,
            StoreCapability.Presign, StoreCapability.ReadChecksum, StoreCapability.ConditionalWrite));
    public static readonly ObjectStore Minio = new("minio", 8L * 1024 * 1024, 5L * 1024 * 1024 * 1024, 10_000, ChunkPolicy.Artifact,
        ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: StanceSeat.Request, tierAt: StanceSeat.None, eraseBatch: 1_000,
        holds: CapabilitySet<StoreCapability>.Of(StoreCapability.BatchErase, StoreCapability.PerObjectWorm,
            StoreCapability.Presign, StoreCapability.ConditionalWrite));
    public static readonly ObjectStore Presigned = new("presigned", long.MaxValue, long.MaxValue, 1, ChunkPolicy.Artifact,
        ObjectChecksum.None, StorageTier.Standard, ObjectEncryption.ProviderManaged.Instance, ObjectLock.Off.Instance,
        worm: StanceSeat.None, tierAt: StanceSeat.None, eraseBatch: 1,
        holds: CapabilitySet<StoreCapability>.None);

    public long PartSize { get; }
    public long PartCeiling { get; }
    public int PartCount { get; }
    public ChunkPolicy Chunking { get; }
    public ObjectChecksum Integrity { get; }
    public StorageTier Tier { get; }
    public ObjectEncryption Encryption { get; }
    public ObjectLock Lock { get; }
    public StanceSeat Worm { get; }
    public StanceSeat TierAt { get; }
    public int EraseBatch { get; }
    public CapabilitySet<StoreCapability> Holds { get; }
    private ObjectStore(string key, long partSize, long partCeiling, int partCount, ChunkPolicy chunking, ObjectChecksum integrity, StorageTier tier, ObjectEncryption encryption, ObjectLock @lock, StanceSeat worm, StanceSeat tierAt, int eraseBatch, CapabilitySet<StoreCapability> holds) : this(key) =>
        (PartSize, PartCeiling, PartCount, Chunking, Integrity, Tier, Encryption, Lock, Worm, TierAt, EraseBatch, Holds) =
            (partSize, partCeiling, partCount, chunking, integrity, tier, encryption, @lock, worm, tierAt, eraseBatch, holds);

    public CapabilitySet<StoreCapability> Degrade =>
        CapabilitySet<StoreCapability>.Of(toSeq(StoreCapability.Items).Filter(row => !Holds.Admits(row)).ToArray());

    public bool Admits(ObjectLock stance) => stance is ObjectLock.Off || Worm.Holds;

    public ResidenceClaim Claim(BlobHandle handle) =>
        handle.Codec != ObjectCodec.Identity || Encryption is ObjectEncryption.ClientSealed
            ? new ResidenceClaim.Framed()
            : (Integrity.Identity, Integrity.S3Algorithm, Integrity.Wire(handle.Key).Case) is (true, ChecksumAlgorithm algorithm, string wire)
                ? new ResidenceClaim.Proven(algorithm, wire)
                : new ResidenceClaim.Plain();

    public InitiateMultipartUploadRequest Stamp(InitiateMultipartUploadRequest request, BlobHandle handle, Instant now) =>
        (Form(handle).Iter(pair => request.Metadata[pair.Key] = pair.Value), Lock.ApplyS3(Encryption.ApplyS3(request), now)).Item2;
    public PutObjectArgs Stamp(PutObjectArgs request, BlobHandle handle, Instant now) =>
        Lock.ApplyMinio(Encryption.ApplyMinio(request.WithHeaders(Form(handle).ToDictionary())), now);
    public UploadObjectOptions Stamp(UploadObjectOptions options) => Encryption.ApplyGcs(options);
    public Google.Apis.Storage.v1.Data.Object Stamp(Google.Apis.Storage.v1.Data.Object resource, BlobHandle handle) =>
        (resource.Metadata = Form(handle).ToDictionary(), resource).Item2;
    public CommitBlockListOptions Stamp(CommitBlockListOptions options, BlobHandle handle) =>
        (Form(handle).Iter(pair => options.Metadata[pair.Key] = pair.Value), options).Item2;

    static Seq<(string Key, string Value)> Form(BlobHandle handle) => Seq(
        (ObjectCodec.CodecKey, handle.Codec.Key),
        (ObjectCodec.PlainKey, handle.Plain.ToString(CultureInfo.InvariantCulture)));

    public long ObjectCeiling => PartCeiling > long.MaxValue / PartCount ? long.MaxValue : PartCeiling * PartCount;

    public IO<(ReadOnlySequence<byte> Bytes, Option<WrappedKey> Dek)> Encode(ObjectCodec codec, ContentAddress key, ReadOnlySequence<byte> plain) =>
        codec.Pack(Chunking, plain).Bind(packed => Encryption.SealSource(key, Chunking, packed));

    public IO<Stream> Decode(ObjectClient client, BlobHandle handle, Option<(long Start, long End)> range, Func<ContentAddress, IO<Option<WrappedKey>>> envelope) {
        IO<Stream> Opened(Option<(long Start, long End)> window) =>
            Encryption.Read(handle.Key, Chunking, window, envelope, resident => Head(client, handle), inner => Fetch(client, handle, inner));
        return handle.Codec == ObjectCodec.Identity
            ? Opened(range)
            : from present in Head(client, handle)
              from resident in present.Match(Some: IO.pure, None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(handle.Key)))
              let frame = ObjectCodec.CodecFrame.Of(Chunking, resident.Extent.Plain)
              let window = range.IfNone((Start: 0L, End: resident.Extent.Plain - 1))
              from bounded in window is { Start: >= 0 } && window.End >= window.Start && window.End < resident.Extent.Plain
                  ? IO.pure(unit)
                  : IO.fail<Unit>(new RemoteStoreFault.InvalidRange(handle.Key, window.Start, window.End, resident.Extent.Plain))
              from head in Opened(Some((0L, frame.Directory - 1)))
              from directory in ObjectIo.Drain(head, static run => IO.pure(run.ToArray()))
              let span = frame.Window(directory, window.Start, window.End)
              from body in Opened(Some((span.Start, span.End)))
              from plain in ObjectIo.Drain(body, run => handle.Codec.Unpack(Chunking, resident.Extent.Plain, directory, window.Start / frame.Stride, run))
              from proven in Proven(handle, range, plain.Slice(checked((int)span.Skip), checked((int)(window.End - window.Start + 1))))
              select proven.AsStream();
    }

    static IO<ReadOnlyMemory<byte>> Proven(BlobHandle handle, Option<(long Start, long End)> range, ReadOnlyMemory<byte> plain) =>
        range.IsSome || ContentAddress.Of(ContentHash.Of(plain, static (bytes, hash) => hash.Append(bytes.Span))) == handle.Key
            ? IO.pure(plain)
            : IO.fail<ReadOnlyMemory<byte>>(new RemoteStoreFault.IntegrityBreach(handle.Key, "content-key"));

    public IO<BlobResidence> Put(ObjectClient client, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now) =>
        (ObjectIo.For(client).Multipart(this, Tier, handle, residence, manifest, source, sink, now)
            | @catch<IO, BlobResidence>(static e => e is RemoteStoreFault.Conflict or RemoteStoreFault.ProviderConflict, _ => Head(client, handle)
                .Bind(present => present.Match(
                    Some: existing => sink(new BlobTransferFact(this, BlobFactKind.ConflictNoop, handle.Key, 0L, 0, None)).Map(_ => existing with { Parts = 0 }),
                    None: () => IO.fail<BlobResidence>(new RemoteStoreFault.NotFound(handle.Key)))))).As();

    public IO<Stream> Fetch(ObjectClient client, BlobHandle handle, Option<(long Start, long End)> range) =>
        ObjectIo.For(client).Fetch(this, handle, range);
    public IO<Option<BlobResidence>> Head(ObjectClient client, BlobHandle handle) => ObjectIo.For(client).Head(this, handle);
    public IO<Unit> Delete(ObjectClient client, BlobHandle handle) => ObjectIo.For(client).Erase(handle);
    public IO<Seq<ContentAddress>> List(ObjectClient client) => ObjectIo.For(client).Enumerate();
    public IO<Unit> Abandon(ObjectClient client, BlobHandle handle, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        ObjectIo.For(client).Abandon(this, handle, session, sink);

    public IO<Unit> Transition(ObjectClient client, BlobHandle handle, StorageTier tier, Instant now) =>
        ObjectIo.For(client).Transition(this, handle, tier, now);

    public IO<ThawState> Rehydrate(ObjectClient client, BlobHandle handle, Duration window) =>
        ObjectIo.For(client).Rehydrate(this, handle, window);

    public IO<EraseTally> EraseMany(ObjectClient client, Seq<BlobHandle> handles) =>
        toSeq(handles.Chunk(EraseBatch)).TraverseM(page => ObjectIo.For(client).EraseMany(toSeq(page))).As()
            .Map(static pages => pages.Fold(EraseTally.Empty, static (tally, page) => tally + page));

    public IO<ObjectGrant> Grant(ObjectClient client, RetentionClass cls, GrantDemand demand, ProjectionContext frame) =>
        ObjectIo.For(client).Issue(demand, BlobName.Handle(demand.Request.Addressed, client.Tenant, cls, ObjectCodec.Identity, 0L), frame.Now());

    // --- [COMPOSITION]
    public BlobRemote Placement(ObjectClient client, ArtifactKind kind, ObjectCodec codec, Func<ContentAddress, IO<Option<WrappedKey>>> envelope, BlobLedger ledger, ProjectionContext frame, Func<BlobTransferFact, IO<Unit>> sink) {
        RetentionClass cls = kind.Retention;
        BlobHandle Named(ContentAddress key, long plain) => BlobName.Handle(key, client.Tenant, cls, codec, plain);
        IO<BlobHandle> Resolved(ContentAddress key) => Head(client, Named(key, 0L))
            .Bind(present => present.Match(Some: r => IO.pure(Named(key, r.Extent.Plain) with { Codec = r.Codec }), None: () => IO.fail<BlobHandle>(new RemoteStoreFault.NotFound(key))));
        return new(
            Put: (admitted, stream) => ObjectIo.Drain(stream, source =>
                BlobGc.WriteBlobFirst(this, client, admitted, kind, codec, source, ledger, sink, frame).Map(static residence => residence.Key)),
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

public readonly record struct BlobRemote(
    Func<BlobAdmission, Stream, IO<ContentAddress>> Put,
    Func<ContentAddress, Option<(long Start, long End)>, IO<Stream>> Get,
    Func<ContentAddress, IO<Option<BlobResidence>>> Stat,
    Func<ContentAddress, Duration, IO<ThawState>> Thaw,
    Func<ContentAddress, IO<Unit>> Delete,
    Func<Seq<ContentAddress>, IO<EraseTally>> Sweep,
    Func<IO<Seq<ContentAddress>>> List,
    Func<ContentAddress, string, IO<Unit>> Abandon,
    Func<GrantDemand, IO<ObjectGrant>> Issue);

public readonly record struct ContentBlobPort(
    Func<ReadOnlyMemory<byte>, IO<ContentAddress>> Put,
    Func<ContentAddress, IO<ReadOnlyMemory<byte>>> Get) {
    public static ContentBlobPort Of(BlobRemote remote, DataClassification classification) => new(
        Put: bytes => {
            ContentAddress key = ContentAddress.Of(ContentHash.Of(bytes, static (held, hash) => hash.Append(held.Span)));
            return remote.Put(new BlobAdmission(key, bytes.Length, classification, None, None), new MemoryStream(bytes.ToArray()));
        },
        Get: key => remote.Get(key, None)
            .Bind(static stream => ObjectIo.Drain(stream, static source => IO.pure((ReadOnlyMemory<byte>)source.ToArray()))));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                      | [BINDING]                                                           |
| :-----: | :--------------- | :------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | content-key name | `{class}/{tenant}/{key:x32}` via `BlobName`  | one `BlobHandle` mint at dispatch; never a second identity          |
|  [02]   | per-leg dispatch | `ObjectClient.Map`                           | union case IS the dispatch; no mismatch guard                       |
|  [03]   | write-once seal  | provider conditional-write `412`-noop        | no read-before-write; the seal is the concurrency primitive         |
|  [04]   | integrity        | `ResidenceClaim.Proven` supplies the digest  | the content key IS the whole-object checksum; never re-hashed       |
|  [05]   | integrity claims | `Framed`/`Plain`/`Proven`, one fold          | the coincidence is a case; neither substitution is spellable        |
|  [06]   | capability set   | `Holds` per row, `Degrade` its complement    | one absence vocabulary; the honest clause derives, never restated   |
|  [07]   | fault rail       | one `Lift` per edge, one `Granted` per grant | the band classifies; the root-bound port executes                   |
|  [08]   | re-drive seam    | `ObjectClient.Redrive` wraps every crossing  | one pass unbound, typed refusal intact                              |
|  [09]   | fact vocabulary  | `BlobFactKind` closes it; slots DERIVE       | a new kind is one row; no free string reaches the stream            |
|  [10]   | receipt path     | every write via write-blob-first             | pending fence, transfer facts, catalog row as one composition       |
|  [11]   | presigned grants | minter to `ObjectGrant` per op               | minter-attested expiry; a bare `403` is `Denied`                    |
|  [12]   | issuer grants    | `Grant` via the leg per row                  | TTL-boxed, admission-gated, frame-instant expiry                    |
|  [13]   | one WORM clock   | `Upload` samples the frame instant once      | provider retention date and catalog window derive from it           |
|  [14]   | object ceiling   | `PartCeiling * PartCount` per row            | domain-side refusal at admission; never learned from a provider 4xx |
|  [15]   | transfer window  | `ReadOnlySequence<byte>` end to end          | no `int` length on the write path; no truncating narrowing          |
|  [16]   | stance seat      | one `StanceSeat` per stance column           | every rung but `None` enforces; only the grant plane holds none     |
|  [17]   | residence form   | codec row, then the seal, on metadata        | the key covers plaintext; `Head` observes what the writer declared  |
|  [18]   | tier ladder      | `Transition` metadata rewrite                | bytes never move for a storage class; the re-PUT is deleted         |
|  [19]   | cold rung        | `ThawState` plus the provider's own code     | thaw is a verb, not a refusal; no probing head per read             |
|  [20]   | erase page       | `EraseBatch` per row, one tally per page     | accepted and refused are two columns; a page of one degrades        |
|  [21]   | one presigner    | `GrantSigner` keyed by dialed endpoint       | every grant case reaches a verb; no per-provider signer             |
|  [22]   | verb identity    | `ObjectVerb` on every crossing               | one code reads per verb; a re-drive names what it re-drives         |

## [03]-[TRANSFER]

- Owner: `ObjectIo` the one generic transfer engine — a per-provider `ObjectLeg` delegate row (initiate, stage, seal, abort, list-committed, fetch, head, erase, erase-many, enumerate, issue, retain, transition, rehydrate) the five providers each fill once, over which a single packing fold packs the manifest's content-defined chunks into provider parts and seals; `ObjectLeg` the closed fourteen-delegate carrier the union case resolves; `MultipartTransfer` the receipt-emitting `Upload` and the `Parts` packer; `TransferPart`/`CommittedPart`/`PartCursor` the part-packing shapes; `BlobName` the class-leading name projection minting the form-bearing `BlobHandle`; `BlobTransferReceipt` the per-object evidence carrying the frame correlation.
- Law: the transfer window is `ReadOnlySequence<byte>` on every write surface, because a payload past `int` range reaches no `ReadOnlySpan<byte>` at all — the kernel rules the one-shot span unrepresentable there rather than merely slow, so an `int`-shaped length or offset anywhere on this path is the byte ceiling wearing a field type.
- Law: per-provider variance rides ONE leg carrier; a residence axis widens the handle, never fourteen delegate arities, so a new stored form is a column on one value rather than a parameter on every slot.
- Law: staged parts SURVIVE a fault or cancel by design; the durable session token rides the pending ledger, so a re-drive resumes the committed windows instead of restarting, and the explicit abandon is the one reap — an auto-abort release deletes the parts resume exists to keep.
- Law: the presigned leg rails like every other. Its grant execution folds status through the band's own grant admission, so no throw crosses a domain body and the closed grant union dispatches through its generated total switch rather than a catch-all that turns a future case into a silent failure.
- Exemption: the granted HTTP execution and its form-post assembly are the platform-forced statement seams; the minted fields precede the payload part because the upstream form policy requires that order.
- Entry: `Upload` is the receipt-emitting write every op composes — the frame supplies mark, elapsed, and now, stamping the correlation onto residence and receipt, and the row's derived object ceiling admits the payload BEFORE the first part stages; `Multipart` runs the one bracket-scoped packing fold over the resolved leg at the row tier; `Drain` stages a fetch stream into a pooled buffer writer; `Parts` packs the chunks into windows clearing the provider floor and bounded by its per-part ceiling; `Formed` reads the stored residence form back out of whichever metadata dictionary its provider owns; `Bound` is the ONE crossing, lifting every SDK call into the band and carrying it through the root-bound re-drive port.
- Auto: `Parts` accumulates content-defined chunks into part windows each closing once it clears the floor or reaches the ceiling, so a part spans whole chunks at the smallest legal part count and the open tail seals last; `Multipart` reads the prior committed set so an interrupted transfer SKIPS windows already committed in the same session — orthogonal to whole-manifest dedup, one resuming a torn upload and the other skipping a resident object — then folds the residual windows, counting resumed versus fresh into the residence; a fault or cancel folds to the band's torn case and leaves the staged parts in place.
- Receipt: the transfer facts ride their own slots per uploaded part, skipped window, opened session, and torn ceremony, and the fault fact carries the settled re-drive verdict so a refusal states once whether its durable session is worth resuming; the residence carries the realized part and resumed-part counts the receipt reads.
- Boundary: the object name is `{class}/{tenant}/{key:x32}` and projects ONCE at the dispatch layer, so every naming slot takes the resolved handle and a leg composing a prefix is unrepresentable; the class segment LEADS so a provider lifecycle rule governs one class across every tenant, and per-tenant listing folds the closed retention-class roster into one prefixed page per stem rather than scanning a bucket the tenant partition exists to fence; the content-defined chunk boundary, the per-chunk key, and the whole-blob identity are the codec owner's, consumed here as the manifest, so a re-declared frame width, a second chunker, or a second hash is the deleted form. Provider placement deduplicates only at the whole-object seal because no row can synthesize one object from another object's resident chunks; the part floor clears the S3 minimum as a row value, never a free literal; a torn upload leaves resumable staged parts under its durable session, with the provider's incomplete-upload lifecycle rule the backstop.
- Packages: AWSSDK.S3, Azure.Storage.Blobs, Azure.Storage.Blobs.Batch, Google.Cloud.Storage.V1, Minio, CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter<byte>`), System.IO.Hashing, System.Collections.Frozen, LanguageExt.Core, NodaTime, BCL inbox (`HttpClient`, `MultipartFormDataContent`, `ReadOnlyMemoryContent`).
- Growth: one part-floor, per-part-ceiling, part-count, and erase-page quadruple per provider row, or one chunking row for a tighter window; a sixth provider fills one leg row — its retain, transition, and rehydrate slots the declared no-ops wherever its seats hold nothing — and contributes its exception family to the one lift fold; a second chunker, a re-declared frame width, a hand-written object-size or page literal beside the row columns, a per-provider transfer or read body, a second HTTP uploader, a per-leg page-chunking loop, or a per-provider abort catch is the deleted form.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TransferPart(int Number, long Offset, long Length, int Chunks);
public readonly record struct CommittedPart(int Number, string ETag);

public readonly record struct BlobTransferReceipt(ObjectStore Provider, ContentAddress Key, Extent Extent, int Parts, int ResumedParts, Option<ContentAddress> Verified, Option<Instant> WormUntil, Duration Elapsed, Instant At, CorrelationId Correlation);

public readonly record struct BlobHandle(ContentAddress Key, string Name, ObjectCodec Codec, long Plain);

public readonly record struct ObjectLeg(
    Func<ObjectStore, StorageTier, BlobHandle, Instant, Option<string>, IO<string>> Initiate,
    Func<string, BlobHandle, TransferPart, ReadOnlySequence<byte>, IO<CommittedPart>> Stage,
    Func<ObjectStore, StorageTier, string, BlobHandle, Seq<CommittedPart>, ReadOnlySequence<byte>, Instant, IO<Unit>> Seal,
    Func<string, BlobHandle, IO<Unit>> Abort,
    Func<string, BlobHandle, IO<Seq<CommittedPart>>> Committed,
    Func<ObjectStore, BlobHandle, Option<(long Start, long End)>, IO<Stream>> Fetch,
    Func<ObjectStore, BlobHandle, IO<Option<BlobResidence>>> Head,
    Func<BlobHandle, IO<Unit>> Erase,
    Func<Seq<BlobHandle>, IO<EraseTally>> EraseMany,
    Func<IO<Seq<ContentAddress>>> Enumerate,
    Func<GrantDemand, BlobHandle, Instant, IO<ObjectGrant>> Issue,
    Func<ObjectStore, BlobHandle, Instant, IO<Unit>> Retain,
    Func<ObjectStore, BlobHandle, StorageTier, Instant, IO<Unit>> Transition,
    Func<ObjectStore, BlobHandle, Duration, IO<ThawState>> Rehydrate);

// --- [OPERATIONS] ----------------------------------------------------------------------
static class BlobName {
    public static BlobHandle Handle(ContentAddress key, TenantId tenant, RetentionClass cls, ObjectCodec codec, long plain) =>
        new(key, $"{Prefix(tenant, cls)}{key.Value:x32}", codec, plain);
    public static string Prefix(TenantId tenant, RetentionClass cls) => $"{cls.Key}/{tenant.Text}/";
    public static ContentAddress OfName(string name) => ContentAddress.Of(UInt128.Parse(name.AsSpan(name.LastIndexOf('/') + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
}

public static class MultipartTransfer {
    public static IO<BlobTransferReceipt> Upload(ObjectStore provider, ObjectClient client, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, ProjectionContext frame) =>
        from mark in IO.lift(frame.Mark)
        from now in IO.lift(frame.Now)
        from _ in Admitted(provider, handle.Key, source.Length)
        from sealed_ in (provider.Put(client, handle, residence with { Correlation = frame.Correlation }, manifest, source, sink, now)
            | @catch<IO, BlobResidence>(static _ => true, error => sink(new BlobTransferFact(provider, BlobFactKind.Fault, handle.Key, source.Length, 0, residence.ConditionToken,
                Some(client.Redrive.Settle(new StoreHop.Object(ObjectVerb.Write), provider.Key, residence.ResumedParts, Fin<BlobResidence>.Fail(error)))))
                    .Bind(_ => IO.fail<BlobResidence>(error)))).As()
        select new BlobTransferReceipt(provider, sealed_.Key, sealed_.Extent, sealed_.Parts, sealed_.ResumedParts, sealed_.Verified, provider.Lock.Until(now), frame.Elapsed(mark), now, frame.Correlation);

    static IO<Unit> Admitted(ObjectStore provider, ContentAddress key, long length) =>
        length <= provider.ObjectCeiling
            ? IO.pure(unit)
            : IO.fail<Unit>(new RemoteStoreFault.Oversize(key, provider.Key, "object-ceiling"));

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
    public TransferPart Seal(int number) => new(number, Start, Bytes, Chunks);
}

public static class ObjectIo {
    public static readonly Seq<StoreSlot> Slots = toSeq(BlobFactKind.Items).Map(static row => row.Slot);

    public static ObjectLeg For(ObjectClient client) => client.Map(
        s3: static r => S3Leg(r), azure: static r => AzureLeg(r), gcs: static r => GcsLeg(r), minio: static r => MinioLeg(r), presigned: static r => PresignedLeg(r));

    public static IO<BlobResidence> Multipart(this ObjectLeg leg, ObjectStore provider, StorageTier tier, BlobHandle handle, BlobResidence residence, ChunkManifest manifest, ReadOnlySequence<byte> source, Func<BlobTransferFact, IO<Unit>> sink, Instant now) {
        Seq<TransferPart> windows = MultipartTransfer.Parts(manifest, provider);
        return (leg.Initiate(provider, tier, handle, now, residence.ConditionToken).Bind(token =>
                from _session in sink(new BlobTransferFact(provider, BlobFactKind.Session, handle.Key, 0L, 0, Some(token)))
                from prior in leg.Committed(token, handle)
                let resumed = prior.Map(static p => p.Number).ToFrozenSet()
                from staged in windows.Filter(w => !resumed.Contains(w.Number)).TraverseM(w =>
                    from committed in leg.Stage(token, handle, w, source.Slice(w.Offset, w.Length))
                    from _ in sink(new BlobTransferFact(provider, BlobFactKind.Part, handle.Key, w.Length, w.Number, None))
                    select committed).As()
                from _ in prior.TraverseM(p => sink(new BlobTransferFact(provider, BlobFactKind.Resume, handle.Key, 0L, p.Number, None))).As()
                let verified = Verified(provider, handle, source, windows, prior.Count)
                from _integrity in verified.Match(
                    Some: minted => minted == handle.Key
                        ? IO.pure(unit)
                        : IO.fail<Unit>(new RemoteStoreFault.IntegrityBreach(handle.Key, provider.Key)),
                    None: static () => IO.pure(unit))
                from _ in leg.Seal(provider, tier, token, handle, toSeq((prior + staged).OrderBy(static p => p.Number)), source, now)
                from _retain in leg.Retain(provider, handle, now)
                select residence with { Extent = residence.Extent with { Stored = source.Length }, Parts = windows.Count, ResumedParts = prior.Count, Verified = verified, ConditionToken = None })
            | @catch<IO, BlobResidence>(static _ => true, error => IO.fail<BlobResidence>(error is RemoteStoreFault ? error : new RemoteStoreFault.Aborted(handle.Key, windows.Count, error)))).As();
    }

    static Option<ContentAddress> Verified(ObjectStore provider, BlobHandle handle, ReadOnlySequence<byte> source, Seq<TransferPart> windows, int resumed) =>
        resumed > 0 || provider.Claim(handle) is ResidenceClaim.Framed
            ? None
            : Some(ContentAddress.Of(ContentHash.Of(
                (Source: source, Windows: windows),
                static (state, hash) => state.Windows.Iter(row => hash.Append(state.Source.Slice(row.Offset, row.Length).AsStream())))));

    public static IO<Unit> Abandon(this ObjectLeg leg, ObjectStore provider, BlobHandle handle, string session, Func<BlobTransferFact, IO<Unit>> sink) =>
        from _ in leg.Abort(session, handle)
        from _fact in sink(new BlobTransferFact(provider, BlobFactKind.Abort, handle.Key, 0L, 0, Some(session)))
        select unit;

    static BlobResidence Formed(ContentAddress key, long stored, Rung tier, Func<string, string?> stated) =>
        BlobResidence.From(key,
            long.TryParse(stated(ObjectCodec.PlainKey), NumberStyles.None, CultureInfo.InvariantCulture, out long plain)
                ? new Extent(stored, plain)
                : Extent.Passthrough(stored),
            tier, ObjectCodec.Observed(stated(ObjectCodec.CodecKey)));

    static IO<Seq<ContentAddress>> Listed(TenantId tenant, Func<string, IO<Seq<ContentAddress>>> under) =>
        toSeq(RetentionClass.Items).TraverseM(cls => under(BlobName.Prefix(tenant, cls))).As().Map(static pages => pages.Flatten());

    public static IO<T> Drain<T>(Stream source, Func<ReadOnlySequence<byte>, IO<T>> use) =>
        IO.lift(static () => new ArrayPoolBufferWriter<byte>()).Bracket(
            Use: writer => IO.liftAsync(async () => await Op.Of().Catch(async _ => {
                await source.CopyToAsync(writer.AsStream()).ConfigureAwait(false);
                return Fin<ReadOnlySequence<byte>>.Succ(new ReadOnlySequence<byte>(writer.WrittenMemory));
            }).ConfigureAwait(false)).Bind(IO.liftFin).Bind(use),
            Fin: writer => IO.lift(() => Op.Of().Catch(() => {
                writer.Dispose();
                source.Dispose();
                return Fin<Unit>.Succ(unit);
            })).Bind(IO.liftFin));

    internal static IO<T> Bound<T>(ObjectClient client, string provider, ObjectVerb verb, ContentAddress key, Func<Task<T>> call) =>
        client.Redrive.Carry(new StoreHop.Object(verb), provider,
            IO.liftAsync(async () => (await Op.Of().Catch(async _ => Fin<T>.Succ(await call().ConfigureAwait(false))).ConfigureAwait(false))
                .MapFail(error => RemoteStoreFault.Lift(provider, verb, key, error)))
            .Bind(IO.liftFin));

    static IO<HttpResponseMessage> Sent(ObjectClient client, ObjectVerb verb, ContentAddress key, Func<Task<HttpResponseMessage>> call) =>
        Bound(client, "presigned", verb, key, call).Bind(response => response.IsSuccessStatusCode
            ? IO.pure(response)
            : IO.fail<HttpResponseMessage>(RemoteStoreFault.Granted(verb, key, response)));

    // --- [LEGS]
    static ObjectLeg S3Leg(ObjectClient.S3 r) => new(
        Initiate: (store, tier, key, now, resume) => resume.Match(
            Some: IO.pure,
            None: () => Bound(r, "s3", ObjectVerb.Write, key.Key, () => r.Client.InitiateMultipartUploadAsync(store.Stamp(new InitiateMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, StorageClass = tier.S3Class, ChecksumAlgorithm = store.Claim(key).Supplied, ChecksumType = store.Claim(key).Supplied is null ? null : ChecksumType.FULL_OBJECT }, key, now))).Map(static x => x.UploadId)),
        Stage: (token, key, part, bytes) => Bound(r, "s3", ObjectVerb.Write, key.Key, () => r.Client.UploadPartAsync(new UploadPartRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token, PartNumber = part.Number, PartSize = part.Length, InputStream = bytes.AsStream() })).Map(x => new CommittedPart(part.Number, x.ETag)),
        Seal: (store, _, token, key, parts, _, _) => Bound(r, "s3", ObjectVerb.Write, key.Key, () => r.Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token, IfNoneMatch = "*", ChecksumXXHASH128 = store.Claim(key).Digest.ValueUnsafe(), PartETags = parts.Map(static p => new PartETag(p.Number, p.ETag)).ToList() })).Map(static _ => unit),
        Abort: (token, key) => string.IsNullOrEmpty(token) ? IO.pure(unit) : Bound(r, "s3", ObjectVerb.Write, key.Key, () => r.Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token })).Map(static _ => unit),
        Committed: (token, key) => Bound(r, "s3", ObjectVerb.Write, key.Key, () => r.Client.ListPartsAsync(new ListPartsRequest { BucketName = r.Bucket, Key = key.Name, UploadId = token })).Map(static x => toSeq(x.Parts).Map(static p => new CommittedPart(p.PartNumber, p.ETag))),
        Fetch: (store, key, range) => Bound(r, "s3", ObjectVerb.Read, key.Key, () => r.Client.GetObjectAsync(store.Integrity.ApplyS3(new GetObjectRequest { BucketName = r.Bucket, Key = key.Name, ByteRange = range.Match(Some: static w => new ByteRange(w.Start, w.End), None: static () => null) }))).Map(static x => x.ResponseStream),
        Head: (store, key) => Bound(r, "s3", ObjectVerb.Read, key.Key, () => r.Client.GetObjectMetadataAsync(r.Bucket, key.Name)).Map(x => Optional(Formed(key.Key, x.ContentLength, Rung.Of(StorageTier.Observed(x.StorageClass?.Value), store.Tier), slot => x.Metadata[slot]))),
        Erase: key => Bound(r, "s3", ObjectVerb.Erase, key.Key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name)).Map(static _ => unit),
        EraseMany: page => Bound(r, "s3", ObjectVerb.Erase, default, () => r.Client.DeleteObjectsAsync(new DeleteObjectsRequest {
                BucketName = r.Bucket, Quiet = false, Objects = page.Map(static h => new KeyVersion { Key = h.Name }).ToList(),
            })).Map(x => new EraseTally(page.Count, toSeq(x.DeleteErrors).Map(static e => (BlobName.OfName(e.Key), e.Code)))),
        Enumerate: () => Listed(r.Tenant, prefix => Bound(r, "s3", ObjectVerb.List, default, () => r.Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = r.Bucket, Prefix = prefix })).Map(static x => toSeq(x.S3Objects).Map(static o => BlobName.OfName(o.Key)))),
        Issue: (demand, handle, now) => Bound(r, "s3", ObjectVerb.Grant, handle.Key, () => r.Signer.Sign(demand, handle, now)),
        Retain: static (_, _, _) => IO.pure(unit),
        Transition: (_, key, tier, _) => Bound(r, "s3", ObjectVerb.Transition, key.Key, () => r.Client.CopyObjectAsync(new CopyObjectRequest {
                SourceBucket = r.Bucket, SourceKey = key.Name, DestinationBucket = r.Bucket, DestinationKey = key.Name,
                StorageClass = tier.S3Class, MetadataDirective = S3MetadataDirective.COPY, TaggingDirective = TaggingDirective.COPY,
            })).Map(static _ => unit),
        Rehydrate: (_, key, window) => (Bound(r, "s3", ObjectVerb.Restore, key.Key, () => r.Client.RestoreObjectAsync(new RestoreObjectRequest {
                    BucketName = r.Bucket, Key = key.Name, Days = int.Max(1, (int)window.TotalDays), Tier = GlacierJobTier.Standard,
                })).Map(static _ => (ThawState)new ThawState.Thawing(None))
            | @catch<IO, ThawState>(static e => e is RemoteStoreFault.Conflict or RemoteStoreFault.ProviderConflict, static _ => IO.pure<ThawState>(new ThawState.Thawing(None)))
            | @catch<IO, ThawState>(static e => e is RemoteStoreFault.Frozen { Verb.ColdRefuses: false } or RemoteStoreFault.ProviderFrozen { Verb.ColdRefuses: false }, static _ => IO.pure<ThawState>(new ThawState.Resident()))).As());

    static ObjectLeg AzureLeg(ObjectClient.Azure r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: (token, _, part, bytes) => Bound(r, "azure", ObjectVerb.Write, default, async () => {
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
        Seal: (store, tier, token, key, parts, _, _) => Bound(r, "azure", ObjectVerb.Write, key.Key, () => r.Container.GetBlockBlobClient(token).CommitBlockListAsync(parts.Map(static p => p.ETag).ToList(), store.Stamp(new CommitBlockListOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }, AccessTier = tier.AzureTier }, key))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: (token, key) => Bound(r, "azure", ObjectVerb.Write, key.Key, () => r.Container.GetBlockBlobClient(token).GetBlockListAsync(BlockListTypes.Uncommitted)).Map(static x => toSeq(x.Value.UncommittedBlocks).Map(static b => new CommittedPart(BitConverter.ToInt32(Convert.FromBase64String(b.Name)), b.Name))),
        Fetch: (store, key, range) => Bound(r, "azure", ObjectVerb.Read, key.Key, () => r.Container.GetBlobClient(key.Name).DownloadStreamingAsync(store.Integrity.ApplyAzure(new BlobDownloadOptions { Range = range.Map(static w => new HttpRange(w.Start, w.End - w.Start + 1)).IfNone(default(HttpRange)) }))).Map(static x => x.Value.Content),
        Head: (store, key) => Bound(r, "azure", ObjectVerb.Read, key.Key, () => r.Container.GetBlobClient(key.Name).GetPropertiesAsync()).Map(x => Optional(Formed(key.Key, x.Value.ContentLength, Rung.Of(StorageTier.Observed(x.Value.AccessTier), store.Tier), slot => x.Value.Metadata.TryGetValue(slot, out string? stated) ? stated : null))),
        Erase: key => Bound(r, "azure", ObjectVerb.Erase, key.Key, () => r.Container.GetBlobClient(key.Name).DeleteIfExistsAsync()).Map(static _ => unit),
        EraseMany: page => Bound(r, "azure", ObjectVerb.Erase, default, async () => {
            BlobBatchClient batches = r.Container.GetBlobBatchClient();
            using BlobBatch batch = batches.CreateBatch();
            Seq<(BlobHandle Handle, Response Delayed)> issued = page.Map(handle => (handle, batch.DeleteBlob(r.Container.Name, handle.Name)));
            _ = await batches.SubmitBatchAsync(batch, throwOnAnyFailure: false).ConfigureAwait(false);
            return new EraseTally(page.Count, issued.Filter(static row => row.Delayed.Status >= 400)
                .Map(static row => (row.Handle.Key, row.Delayed.Status.ToString(CultureInfo.InvariantCulture))));
        }),
        Enumerate: () => Listed(r.Tenant, prefix => Bound(r, "azure", ObjectVerb.List, default, async () => {
            List<ContentAddress> keys = [];
            await foreach (BlobItem blob in r.Container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None).ConfigureAwait(false))
                keys.Add(BlobName.OfName(blob.Name));
            return toSeq(keys);
        })),
        Issue: (demand, handle, now) => Bound(r, "azure", ObjectVerb.Grant, handle.Key, () => Task.FromResult<ObjectGrant>(new ObjectGrant.SignedUrl(r.Container.GetBlobClient(handle.Name).GenerateSasUri(demand.Request is GrantRequest.Write ? BlobSasPermissions.Write | BlobSasPermissions.Create : demand.Request is GrantRequest.Erase ? BlobSasPermissions.Delete : BlobSasPermissions.Read, (now + demand.Lifetime).ToDateTimeOffset())))),
        Retain: (store, key, now) => store.Lock.ApplyAzure(now).Match(
            Some: apply => Bound(r, "azure", ObjectVerb.Write, key.Key, () => apply(r.Container.GetBlockBlobClient(key.Name), now)).Map(static _ => unit),
            None: static () => IO.pure(unit)),
        Transition: (_, key, tier, _) => Bound(r, "azure", ObjectVerb.Transition, key.Key, () => r.Container.GetBlobClient(key.Name).SetAccessTierAsync(tier.AzureTier)).Map(static _ => unit),
        Rehydrate: (_, key, _) => Bound(r, "azure", ObjectVerb.Restore, key.Key, () => r.Container.GetBlobClient(key.Name).GetPropertiesAsync()).Bind(head =>
            !string.IsNullOrEmpty(head.Value.ArchiveStatus)
                ? IO.pure<ThawState>(new ThawState.Thawing(None))
                : StorageTier.Observed(head.Value.AccessTier) == Some(StorageTier.Archive)
                    ? Bound(r, "azure", ObjectVerb.Restore, key.Key, () => r.Container.GetBlobClient(key.Name).SetAccessTierAsync(AccessTier.Hot, conditions: null, rehydratePriority: RehydratePriority.Standard)).Map(static _ => (ThawState)new ThawState.Thawing(None))
                    : IO.pure<ThawState>(new ThawState.Resident())));

    static ObjectLeg GcsLeg(ObjectClient.Gcs r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, tier, token, key, _, source, _) => Bound(r, "gcs", ObjectVerb.Write, key.Key, () => r.Client.UploadObjectAsync(store.Stamp(new Google.Apis.Storage.v1.Data.Object { Bucket = r.Bucket, Name = token, ContentType = "application/octet-stream", StorageClass = tier.GcsClass }, key), source.AsStream(), store.Stamp(new UploadObjectOptions { IfGenerationMatch = 0, ChunkSize = 8 * 1024 * 1024 }))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (store, key, range) => Bound(r, "gcs", ObjectVerb.Read, key.Key, async () => {
            MemoryStream sink = new();
            await r.Client.DownloadObjectAsync(r.Bucket, key.Name, sink, store.Integrity.ApplyGcs(new DownloadObjectOptions { Range = range.Match(Some: static w => new RangeHeaderValue(w.Start, w.End), None: static () => null) })).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound(r, "gcs", ObjectVerb.Read, key.Key, () => r.Client.GetObjectAsync(r.Bucket, key.Name)).Map(x => Optional(Formed(key.Key, (long)(x.Size ?? 0), Rung.Of(StorageTier.Observed(x.StorageClass), store.Tier), slot => x.Metadata?.GetValueOrDefault(slot)))),
        Erase: key => Bound(r, "gcs", ObjectVerb.Erase, key.Key, () => r.Client.DeleteObjectAsync(r.Bucket, key.Name)).Map(static _ => unit),
        EraseMany: page => page.TraverseM(handle => Bound(r, "gcs", ObjectVerb.Erase, handle.Key, () => r.Client.DeleteObjectAsync(r.Bucket, handle.Name)).Map(static _ => unit)).As()
            .Map(_ => new EraseTally(page.Count, Seq<(ContentAddress Key, string Code)>())),
        Enumerate: () => Listed(r.Tenant, prefix => Bound(r, "gcs", ObjectVerb.List, default,
            () => Task.FromResult(toSeq(r.Client.ListObjects(r.Bucket, prefix).Select(static o => BlobName.OfName(o.Name)))))),
        Issue: (demand, handle, _) => Bound(r, "gcs", ObjectVerb.Grant, handle.Key, () => r.Signer.SignAsync(r.Bucket, handle.Name, demand.Lifetime.ToTimeSpan(), demand.Request is GrantRequest.Write ? HttpMethod.Put : demand.Request is GrantRequest.Erase ? HttpMethod.Delete : HttpMethod.Get)).Map(static url => (ObjectGrant)new ObjectGrant.SignedUrl(new Uri(url))),
        Retain: (store, key, now) => store.Lock.ApplyGcs(now).Match(
            Some: apply => Bound(r, "gcs", ObjectVerb.Write, key.Key, () => apply(r.Client, new Google.Apis.Storage.v1.Data.Object { Bucket = r.Bucket, Name = key.Name }, now)).Map(static _ => unit),
            None: static () => IO.pure(unit)),
        Transition: (_, key, tier, _) => Bound(r, "gcs", ObjectVerb.Transition, key.Key, () => r.Client.CopyObjectAsync(r.Bucket, key.Name, r.Bucket, key.Name,
            new CopyObjectOptions { ExtraMetadata = new Google.Apis.Storage.v1.Data.Object { StorageClass = tier.GcsClass } })).Map(static _ => unit),
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    static ObjectLeg MinioLeg(ObjectClient.Minio r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (store, _, token, key, _, source, now) => Bound(r, "minio", ObjectVerb.Write, key.Key, async () => {
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
        Abort: (_, key) => Bound(r, "minio", ObjectVerb.Write, key.Key, async () => {
            await foreach (Upload upload in r.Client.ListIncompleteUploadsEnumAsync(new ListIncompleteUploadsArgs().WithBucket(r.Bucket).WithPrefix(key.Name)).ConfigureAwait(false))
                await r.Client.RemoveIncompleteUploadAsync(new RemoveIncompleteUploadArgs().WithBucket(r.Bucket).WithObject(upload.Key)).ConfigureAwait(false);
            return unit;
        }),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (_, key, range) => Bound(r, "minio", ObjectVerb.Read, key.Key, async () => {
            MemoryStream sink = new();
            GetObjectArgs request = new GetObjectArgs().WithBucket(r.Bucket).WithObject(key.Name).WithCallbackStream(stream => stream.CopyTo(sink));
            await r.Client.GetObjectAsync(range.Match(Some: window => request.WithOffsetAndLength(window.Start, window.End - window.Start + 1), None: () => request)).ConfigureAwait(false);
            sink.Position = 0;
            return (Stream)sink;
        }),
        Head: (store, key) => Bound(r, "minio", ObjectVerb.Read, key.Key, () => r.Client.StatObjectAsync(new StatObjectArgs().WithBucket(r.Bucket).WithObject(key.Name))).Map(x => Optional(Formed(key.Key, x.Size, new Rung.Assumed(store.Tier), slot => x.MetaData.GetValueOrDefault(slot)))),
        Erase: key => Bound(r, "minio", ObjectVerb.Erase, key.Key, () => r.Client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(r.Bucket).WithObject(key.Name))).Map(static _ => unit),
        EraseMany: page => Bound(r, "minio", ObjectVerb.Erase, default, () => r.Client.RemoveObjectsAsync(new RemoveObjectsArgs()
                .WithBucket(r.Bucket)
                .WithObjects(page.Map(static h => h.Name).ToList())))
            .Map(refused => new EraseTally(page.Count, toSeq(refused).Map(static e => (BlobName.OfName(e.Key), e.Code)))),
        Enumerate: () => Listed(r.Tenant, prefix => Bound(r, "minio", ObjectVerb.List, default, async () => {
            List<ContentAddress> keys = [];
            await foreach (Item item in r.Client.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(r.Bucket).WithPrefix(prefix).WithRecursive(true)).ConfigureAwait(false))
                keys.Add(BlobName.OfName(item.Key));
            return toSeq(keys);
        })),
        Issue: (demand, handle, now) => Bound(r, "minio", ObjectVerb.Grant, handle.Key, () => r.Signer.Sign(demand, handle, now)),
        Retain: static (_, _, _) => IO.pure(unit),
        Transition: static (_, _, _, _) => IO.pure(unit),
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    static ObjectLeg PresignedLeg(ObjectClient.Presigned r) => new(
        Initiate: static (_, _, key, _, _) => IO.pure(key.Name),
        Stage: static (_, _, part, _) => IO.pure(new CommittedPart(part.Number, "")),
        Seal: (_, _, _, key, _, source, _) => r.Minter(new GrantRequest.Write(key.Key, source.Length)).Bind(grant =>
            Sent(r, ObjectVerb.Write, key.Key, () => grant.Switch(
                formPost:  post => Posted(r.Http, post, key, source),
                signedUrl: url => r.Http.PutAsync(url.Url, new ReadOnlyMemoryContent(source))))).Map(static _ => unit),
        Abort: static (_, _) => IO.pure(unit),
        Committed: static (_, _) => IO.pure(Seq<CommittedPart>()),
        Fetch: (_, key, range) => r.Minter(new GrantRequest.Read(key.Key)).Bind(grant =>
            Sent(r, ObjectVerb.Read, key.Key, () => {
                using HttpRequestMessage request = new(HttpMethod.Get, grant.Url);
                _ = range.Map(w => request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(w.Start, w.End));
                return r.Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            })).Bind(static response => IO.liftAsync(async () => await Op.Of().Catch(async _ =>
                Fin<Stream>.Succ(await response.Content.ReadAsStreamAsync().ConfigureAwait(false))).ConfigureAwait(false)).Bind(IO.liftFin)),
        Head: (store, key) => r.Roster(Some(key.Key)).Map(rows =>
            rows.Find(s => s.Key == key.Key).Map(s => BlobResidence.From(key.Key, Extent.Passthrough(s.Length), new Rung.Assumed(store.Tier), ObjectCodec.Identity))),
        Erase: key => r.Minter(new GrantRequest.Erase(key.Key)).Bind(grant =>
            Sent(r, ObjectVerb.Erase, key.Key, () => r.Http.DeleteAsync(grant.Url))).Map(static _ => unit),
        EraseMany: page => page.TraverseM(handle => r.Minter(new GrantRequest.Erase(handle.Key)).Bind(grant =>
                Sent(r, ObjectVerb.Erase, handle.Key, () => r.Http.DeleteAsync(grant.Url)))).As()
            .Map(_ => new EraseTally(page.Count, Seq<(ContentAddress Key, string Code)>())),
        Enumerate: () => r.Roster(None).Map(static rows => rows.Map(static s => s.Key)),
        Issue: (demand, _, _) => r.Minter(demand.Request),
        Retain: static (_, _, _) => IO.pure(unit),
        Transition: static (_, _, _, _) => IO.pure(unit),
        Rehydrate: static (_, _, _) => IO.pure<ThawState>(new ThawState.Resident()));

    static async Task<HttpResponseMessage> Posted(HttpClient http, ObjectGrant.FormPost post, BlobHandle key, ReadOnlyMemory<byte> source) {
        using MultipartFormDataContent form = new();
        foreach ((string field, string value) in post.Fields)
            form.Add(new StringContent(value), field);
        form.Add(new ReadOnlyMemoryContent(source), "file", key.Name);
        return await http.PostAsync(post.Url, form).ConfigureAwait(false);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
