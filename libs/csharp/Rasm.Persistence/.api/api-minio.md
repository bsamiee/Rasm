# [RASM_PERSISTENCE_API_MINIO]

`Minio` is the endpoint-agnostic S3-compatible object client — one client surface dialing MinIO, Cloudflare R2, Backblaze B2, Wasabi, Ceph RadosGW, or any S3 API — the self-hosted lane the cloud-native `AWSSDK.S3`/`Azure.Storage.Blobs`/`Google.Cloud.Storage.V1` rows in `api-objectstore` lack. It composes that catalog directly: Minio is the FOURTH provider on the `ObjectStore` placement / `BlobRemote` contract, supplying the same multipart-upload, head/list/delete, range-read-resume, and conditional-write edges the three cloud SDKs do, behind the same `ObjectClient` union dispatch — so a content-addressed blob lands on a self-hosted MinIO cluster or a cloud bucket by one placement row, never two code paths. The whole surface is the fluent `IMinioClient` builder + the `*Args` request builders; `ObjectStat.ETag`/`MetaData` is the content-hash descriptor and `IfMatch`/`IfNoneMatch` via `CopyConditions` is the optimistic-concurrency seal the write-once placement rides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Minio`
- package: `Minio`
- assembly: `Minio`
- namespace: `Minio`, `Minio.ApiEndpoints` (the `IBucketOperations`/`IObjectOperations` op contracts `IMinioClient` derives from), `Minio.Credentials` (`IClientProvider` + the credential-provider family), `Minio.DataModel`, `Minio.DataModel.Args`, `Minio.DataModel.Result`, `Minio.DataModel.Response`, `Minio.DataModel.Encryption`, `Minio.DataModel.Tags`, `Minio.DataModel.Select`, `Minio.DataModel.Notification`, `Minio.DataModel.ILM`, `Minio.DataModel.ObjectLock`, `Minio.DataModel.Replication`, `Minio.Exceptions`, `Minio.Handlers`
- license: Apache-2.0 (`<license type="expression">Apache-2.0</license>`)
- target framework: `net8.0` asset on the `net10.0` floor (package ships `net8.0`/`netstandard2.0`; net8.0 wins NuGet precedence — there is no `net10.0` asset, so the consumer binds the `net8.0` lib)
- dependencies: `System.Reactive` (the `IObservable<>` bucket-notification stream), `CommunityToolkit.HighPerformance` (co-consumed substrate), `System.IO.Hashing` (the content-hash substrate), `Microsoft.Extensions.Logging`/`DependencyInjection.Abstractions`
- asset: runtime library, pure-managed AnyCPU (no native — HTTP/S3 protocol over `HttpClient`)
- rail: object-store (self-hosted S3)

## [02]-[PUBLIC_TYPES]

[CLIENT_TYPES]: the client, its config, and DI wiring
- rail: object-store
- `IMinioClient : IBucketOperations, IObjectOperations, IDisposable`; retry/tracing handlers under `Minio.Handlers`

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `IMinioClient`                                      | client contract | the full bucket + object operation surface              |
|  [02]   | `MinioClient : IMinioClient`                        | client          | concrete client; fluent `MinioClientExtensions` build   |
|  [03]   | `MinioConfig`                                       | config          | endpoint/region/credentials/SSL/timeout/proxy carrier   |
|  [04]   | `IMinioClientFactory` / `MinioClientFactory`        | factory         | DI-resolved client factory (`CreateClient()`)           |
|  [05]   | `IClientProvider`                                   | credentials     | pluggable provider — static/IAM/chained/WebIdentity     |
|  [06]   | `IRetryPolicyHandler` / `DefaultRetryPolicyHandler` | retry           | request-level retry policy (engine-owned, see RAIL_LAW) |
|  [07]   | `IRequestLogger` / `DefaultRequestLogger`           | tracing         | per-request log hook into the telemetry spine           |

[ARGS_TYPES]: the fluent request builders — `Minio.DataModel.Args`
- rail: object-store

The operation surface is a `*Args` builder hierarchy (`RequestArgs` → `BucketArgs<T>` → `ObjectArgs<T>` → `EncryptionArgs<T>` → `ObjectVersionArgs<T>` → `ObjectConditionalQueryArgs<T>` → `ObjectWriteArgs<T>`), each exposing fluent `With*` setters returning `T`. Every operation takes its matching args object — there is no positional-parameter form.

| [INDEX] | [SYMBOL]                                            | [BUILDS]           | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `MakeBucketArgs`                                    | bucket create      | bucket name / region / object-lock                 |
|  [02]   | `BucketExistsArgs` / `RemoveBucketArgs`             | bucket head/drop   | by bucket                                          |
|  [03]   | `ListObjectsArgs`                                   | object enumeration | prefix / recursive / versioned listing             |
|  [04]   | `PutObjectArgs`                                     | object write       | multipart-auto stream/file upload, WORM-at-write   |
|  [05]   | `GetObjectArgs`                                     | object read        | full or range read to stream/file/callback         |
|  [06]   | `StatObjectArgs`                                    | object head        | conditional head (ETag/modified-since)             |
|  [07]   | `RemoveObjectArgs` / `RemoveObjectsArgs`            | object delete      | single / bulk (versioned) delete                   |
|  [08]   | `CopyObjectArgs` / `CopySourceObjectArgs`           | server-side copy   | conditional server-side copy, no client round-trip |
|  [09]   | `PresignedGetObjectArgs`                            | presigned URL      | time-bounded delegated GET without credentials     |
|  [10]   | `PresignedPutObjectArgs`                            | presigned URL      | time-bounded delegated PUT without credentials     |
|  [11]   | `PresignedPostPolicyArgs`                           | presigned POST     | browser-direct upload form policy                  |
|  [12]   | `SelectObjectContentArgs`                           | S3 Select          | server-side SQL over CSV/JSON/Parquet objects      |
|  [13]   | `SetObjectTagsArgs`                                 | object tags        | write `Tagging`/`TagSet` object tags               |
|  [14]   | `GetObjectTagsArgs`                                 | object tags        | read object tags                                   |
|  [15]   | `RemoveObjectTagsArgs`                              | object tags        | drop object tags                                   |
|  [16]   | `ListenBucketNotificationsArgs`                     | event stream       | the `IObservable` change feed                      |
|  [17]   | `SetVersioningArgs` / `GetVersioningArgs`           | bucket versioning  | enable/suspend object versioning                   |
|  [18]   | `SetObjectRetentionArgs` / `SetObjectLegalHoldArgs` | object lock        | WORM retention mode + legal hold                   |
|  [19]   | `SetBucketLifecycleArgs`                            | bucket policy      | ILM object-lifecycle/expiry rules                  |
|  [20]   | `SetBucketReplicationArgs`                          | bucket policy      | cross-region replication config                    |
|  [21]   | `SetBucketEncryptionArgs`                           | bucket policy      | default bucket SSE                                 |

[SETTERS]: the fluent `With*` setters each args builder exposes.
- [01]-[MAKEBUCKETARGS]: `WithBucket`/`WithLocation`/`WithObjectLock`
- [02]-[BucketExistsArgs/RemoveBucketArgs]: `WithBucket`
- [03]-[LISTOBJECTSARGS]: `WithBucket`/`WithPrefix`/`WithRecursive`/`WithVersions`
- [04]-[PUTOBJECTARGS]: own `WithStreamData`/`WithObjectSize`/`WithFileName`/`WithProgress`; inherited `WithBucket`/`WithHeaders` (`BucketArgs`), `WithObject` (`ObjectArgs`), `WithContentType`/`WithTagging`/`WithServerSideEncryption`/`WithLegalHold`/`WithRetentionConfiguration` (`WithRetentionConfiguration(ObjectRetentionConfiguration)` writes `x-amz-object-lock-mode`/`x-amz-object-lock-retain-until-date`, SETting WORM at write time)
- [05]-[GETOBJECTARGS]: own `WithOffsetAndLength`/`WithLength` (range read)/`WithCallbackStream`/`WithFile`; inherited `WithBucket`/`WithObject`/`WithVersionId`/`WithServerSideEncryption`
- [06]-[STATOBJECTARGS]: own `WithOffsetAndLength`/`WithLength`; inherited (`ObjectConditionalQueryArgs`) `WithMatchETag`/`WithNotMatchETag`/`WithModifiedSince`/`WithUnModifiedSince`
- [07]-[RemoveObjectArgs/RemoveObjectsArgs]: `WithObjectsVersions` (versioned batch)
- [08]-[CopyObjectArgs/CopySourceObjectArgs]: `WithCopyObjectSource`/`WithCopyConditions`
- [09]-[Presigned*Args]: `WithExpiry`/`WithHeaders`
- [10]-[SELECTOBJECTCONTENTARGS]: `WithExpressionType`/`WithInputSerialization`/`WithOutputSerialization`
- [11]-[*ObjectTagsArgs]: `Tagging`/`TagSet`
- [12]-[LISTENBUCKETNOTIFICATIONSARGS]: `WithEvents`/`WithPrefix`/`WithSuffix`

[MODEL_TYPES]: results and descriptors — `Minio.DataModel` / `.Result` / `.Response` / `.Encryption` / `.ObjectLock` / `.Select` / `.Notification`; SSE modes are `: IServerSideEncryption`
- rail: object-store

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `ObjectStat`                                                | head result   | the `Stat` descriptor (fields in `[FIELDS]`)          |
|  [02]   | `PutObjectResponse`                                         | write result  | put outcome carrying the object `Etag`                |
|  [03]   | `Item`                                                      | list element  | the `List` row (fields in `[FIELDS]`)                 |
|  [04]   | `Bucket`                                                    | bucket        | `Name`/`CreationDate`                                 |
|  [05]   | `CopyConditions`                                            | conditions    | the conditional-copy seal (fields in `[FIELDS]`)      |
|  [06]   | `DeleteError` / `DeletedObject`                             | bulk result   | per-object outcome of `RemoveObjectsAsync`            |
|  [07]   | `SSEC` / `SSEKMS` / `SSES3`                                 | SSE           | customer-key / KMS / S3-managed encryption modes      |
|  [08]   | `ObjectRetentionConfiguration` / `ObjectRetentionMode`      | WORM          | retention-until mode for `WithRetentionConfiguration` |
|  [09]   | `ProgressReport` / `IProgress<ProgressReport>`              | progress      | per-byte upload/download callback                     |
|  [10]   | `CSVInputOptions`                                           | S3 Select IO  | CSV input serialization for server-side `SELECT`      |
|  [11]   | `JSONInputOptions`                                          | S3 Select IO  | JSON input serialization                              |
|  [12]   | `ParquetInputOptions`                                       | S3 Select IO  | Parquet input serialization                           |
|  [13]   | `SelectResponseStream`                                      | S3 Select IO  | the `SELECT` output stream                            |
|  [14]   | `BucketNotification` / `EventType` / `MinioNotificationRaw` | notification  | bucket event config + the raw event payload           |

[FIELDS]: the descriptor field rosters.
- [01]-[OBJECTSTAT]: `ObjectName`/`Size`/`ETag`/`LastModified`/`ContentType`/`MetaData`/`VersionId`/`DeleteMarker`/`TaggingCount`/`Expires`/`ObjectLockMode`/`LegalHoldEnabled`
- [03]-[ITEM]: `Key`/`Size`/`LastModified`/`ETag`/`IsDir`/`VersionId`
- [05]-[COPYCONDITIONS]: `SetMatchETag`/`SetMatchETagNone`/`SetModified`/`SetUnmodified`/`SetReplaceMetadataDirective`

## [03]-[ENTRYPOINTS]

[BUILDER]: fluent client construction — `MinioClientExtensions`; each `.With*` chains off `new MinioClient()` and `.Build()` finalizes
- rail: object-store

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `.WithEndpoint(string)` / `(host, port)` / `(Uri)`       | endpoint       | dial ANY S3 endpoint — MinIO/R2/B2/Wasabi/Ceph     |
|  [02]   | `.WithCredentials(accessKey, secretKey)`                 | credentials    | static access/secret creds                         |
|  [03]   | `.WithSessionToken(st)`                                  | credentials    | STS session token                                  |
|  [04]   | `.WithCredentialsProvider(IClientProvider)`              | credentials    | IAM/WebIdentity/chained provider                   |
|  [05]   | `.WithRegion(string)` / `.WithRegion()`                  | region         | explicit / auto-detected bucket region             |
|  [06]   | `.WithSSL(secure = true)`                                | transport      | TLS toggle                                         |
|  [07]   | `.WithHttpClient(HttpClient, disposeHttpClient = false)` | transport      | inject a pooled/configured `HttpClient`            |
|  [08]   | `.WithTimeout(int)` / `.WithProxy(IWebProxy)`            | transport      | request timeout / outbound proxy                   |
|  [09]   | `.WithRetryPolicy(IRetryPolicyHandler)`                  | retry          | request-level retry handler                        |
|  [10]   | `.WithRetryPolicy(Func<…>)`                              | retry          | retry handler as a delegate                        |
|  [11]   | `.Build()`                                               | finalize       | validates config, returns the ready `IMinioClient` |
|  [12]   | `services.AddMinio(accessKey, secretKey, lifetime)`      | DI             | `ServiceCollectionExtensions` DI registration      |
|  [13]   | `services.AddMinio(Action<IMinioClient>, lifetime)`      | DI             | DI registration via configure delegate             |

[BUCKET_OPS]: bucket lifecycle and policy — `IBucketOperations`; each op is async and takes its matching `*Args` + `CancellationToken`
- rail: object-store

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `MakeBucketAsync`                                                      | create         | create a bucket (region/object-lock aware) |
|  [02]   | `BucketExistsAsync` → `bool`                                           | head           | existence probe                            |
|  [03]   | `RemoveBucketAsync`                                                    | drop           | delete an empty bucket                     |
|  [04]   | `ListBucketsAsync(ct)` → `ListAllMyBucketsResult`                      | list           | all buckets for the credential             |
|  [05]   | `ListObjectsEnumAsync` → `IAsyncEnumerable<Item>`                      | enumerate      | streamed prefix/recursive object listing   |
|  [06]   | `SetVersioningAsync` / `GetVersioningAsync`                            | versioning     | enable/suspend bucket versioning           |
|  [07]   | `SetBucketEncryptionAsync` / `GetBucketEncryptionAsync`                | SSE policy     | get/set default bucket server-side SSE     |
|  [08]   | `RemoveBucketEncryptionAsync`                                          | SSE policy     | drop default bucket SSE                    |
|  [09]   | `SetBucketLifecycleAsync` / `GetBucketLifecycleAsync`                  | ILM            | object lifecycle/expiry rules              |
|  [10]   | `SetBucketReplicationAsync` / `GetBucketReplicationAsync`              | replication    | cross-bucket/region replication config     |
|  [11]   | `ListenBucketNotificationsAsync` → `IObservable<MinioNotificationRaw>` | event stream   | the Reactive bucket-event change feed      |

[OBJECT_OPS]: object put / get / stat / remove / copy — `IObjectOperations`; each op is async and takes its matching `*Args` + `CancellationToken`
- rail: object-store

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `PutObjectAsync` → `PutObjectResponse`                           | write          | stream upload, multipart auto-chunked               |
|  [02]   | `GetObjectAsync` → `ObjectStat`                                  | read           | full or range read (`WithOffsetAndLength`)          |
|  [03]   | `StatObjectAsync` → `ObjectStat`                                 | head           | size/ETag/metadata/lock head (`WithMatchETag`)      |
|  [04]   | `RemoveObjectAsync`                                              | delete         | single object (or version) delete                   |
|  [05]   | `RemoveObjectsAsync` → `IList<DeleteError>`                      | bulk delete    | batched multi-object delete                         |
|  [06]   | `CopyObjectAsync`                                                | copy           | server-side copy with `CopyConditions`              |
|  [07]   | `SelectObjectContentAsync` → `SelectResponseStream`              | S3 Select      | server-side `SELECT` over a CSV/JSON/Parquet object |
|  [08]   | `PresignedGetObjectAsync` → `string`                             | presigned URL  | time-bounded delegated GET URL                      |
|  [09]   | `PresignedPutObjectAsync` → `string`                             | presigned URL  | time-bounded delegated PUT URL                      |
|  [10]   | `PresignedPostPolicyAsync` → `(Uri, IDictionary<string,string>)` | presigned POST | browser-direct upload form policy                   |
|  [11]   | `SetObjectTagsAsync` / `GetObjectTagsAsync`                      | tags           | write/read object tag key-value set                 |
|  [12]   | `RemoveObjectTagsAsync`                                          | tags           | drop object tags                                    |
|  [13]   | `SetObjectRetentionAsync` / `SetObjectLegalHoldAsync`            | object lock    | WORM retention + legal hold                         |
|  [14]   | `ListIncompleteUploadsEnumAsync` → `IAsyncEnumerable<Upload>`    | multipart      | list dangling multipart uploads                     |
|  [15]   | `RemoveIncompleteUploadAsync`                                    | multipart      | abort a dangling multipart upload                   |

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the `Minio.Exceptions` family lifted at the object-store edge
- rail: object-store

| [INDEX] | [THROWN]                                  | [DISCRIMINANT_CASE]                                                       |
| :-----: | :---------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `MinioException`                          | the base — `ServerMessage`/`ServerResponse`/`Response` (`ResponseResult`) |
|  [02]   | `BucketNotFoundException`                 | the absent-bucket → `Option.None` boundary projection                     |
|  [03]   | `ObjectNotFoundException`                 | the absent-object → `Option.None` boundary projection                     |
|  [04]   | `PreconditionFailedException`             | the `412` conditional-write conflict → `RemoteStoreFault.Conflict`        |
|  [05]   | `AccessDeniedException`                   | authz refusal (4xx)                                                       |
|  [06]   | `ForbiddenException`                      | authz refusal (4xx)                                                       |
|  [07]   | `ConnectionException`                     | endpoint unreachable / transport failure                                  |
|  [08]   | `InvalidBucketNameException`              | client-side validation reject                                             |
|  [09]   | `InvalidObjectNameException`              | client-side validation reject                                             |
|  [10]   | `InvalidEndpointException`                | client-side validation reject                                             |
|  [11]   | `EntityTooLargeException`                 | size/length integrity fault                                               |
|  [12]   | `InvalidContentLengthException`           | size/length integrity fault                                               |
|  [13]   | `UnexpectedShortReadException`            | size/length integrity fault                                               |
|  [14]   | `VersionDeletedException`                 | versioning fault                                                          |
|  [15]   | `MissingObjectLockConfigurationException` | lock-state fault                                                          |
|  [16]   | `ErrorResponseException`                  | a generic S3 `ErrorResponse` (carries `ErrorResponse.Code`/`Message`)     |

`Minio.Handlers.IApiResponseErrorHandler` / `DefaultErrorHandler` is the seam that maps an S3 HTTP error to the exception family; the `412` on a `WithMatchETag(*)`/conditional `CopyObject` is the optimistic-concurrency conflict, folded to `RemoteStoreFault.Conflict` exactly as the cloud-SDK `412`s in `api-objectstore` — and treated as a benign write-once no-op (the content is already durably present, identical by hash).

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- the fourth `ObjectStore` provider: Minio is the self-hosted/S3-compatible row on the same `Store/blobstore` `BlobRemote` placement contract as the `api-objectstore` `AWSSDK.S3`/`Azure.Storage.Blobs`/`Google.Cloud.Storage.V1` rows — it supplies the same four legs (`PutObjectAsync`/multipart, `StatObjectAsync` head, `ListObjectsEnumAsync` list, `RemoveObjectAsync` delete) and range-read (`GetObjectArgs.WithOffsetAndLength`) the `ObjectClient` union dispatches, so a content-keyed blob lands on a MinIO/R2/Ceph endpoint by one placement row, never a parallel client surface.
- write-once seal: the inherited `ObjectConditionalQueryArgs<T>.WithMatchETag`/`WithNotMatchETag` (on `StatObjectArgs`/`GetObjectArgs`/`PutObjectArgs`) and `CopyConditions.SetMatchETag`/`SetMatchETagNone` are the optimistic-concurrency edge — a racing second writer to the same content-key `412`s (`PreconditionFailedException`), folded to `RemoteStoreFault.Conflict` and treated as success since the content is identical by hash, the same write-once law `api-objectstore#WRITE_ONCE_SEAL` states for the cloud providers.
- content-hash descriptor: `ObjectStat.ETag` + `ObjectStat.MetaData` is the content-hash/identity evidence the `Element/codec` content-addressed spine and `Query/cache` index read, the `XxHash128` (`System.IO.Hashing`) content key being the object name — Minio stores the bytes, the settled `#ARTIFACT_FRAMES` frame law owns framing.
- self-provisioned tier residence: a self-hosted S3 engine (pgsty/minio continuation image, Ceph RGW) is the server the `Store/provisioning` tier reaches, the on-prem counterpart to the cloud buckets — the `Store ← Rasm.AppHost/Observability # [HEALTH_PROBE]` reachability probe folds the S3 endpoint health into the same `HealthContributorRow` as Npgsql/Redis.
- S3 Select pushdown: `SelectObjectContentAsync` over a CSV/JSON/Parquet object pushes a server-side `SELECT` into the store, the object-store analogue of the `Query/columnar` columnar pushdown — a stored `ParquetSharp`-written object is queried in place without a full GET.
- Reactive change feed: `ListenBucketNotificationsAsync` → `IObservable<MinioNotificationRaw>` (`System.Reactive`) is an external object-mutation stream a `Version/ledger` ingress can fold, distinct from the internal op-log changefeed.
- credential acquisition is connection input handed over by app roots (`IClientProvider`, access/secret keys) per the `api-objectstore` law, never a Persistence fence member.

[RAIL_LAW]:
- Packages: `Minio` (composes `System.Reactive`, `System.IO.Hashing`)
- Owns: the endpoint-agnostic S3-compatible self-hosted object lane — bucket lifecycle, object put/get/stat/remove/copy, multipart + resume, presigned URLs, SSE, versioning, object-lock/WORM, lifecycle/replication, S3 Select, bucket notifications
- Accept: the fluent `IMinioClient` builder built once and reused (a singleton via `AddMinio`/`IMinioClientFactory`), the `*Args` request builders, `ObjectStat.ETag` as the content descriptor, `WithMatchETag`/`CopyConditions` as the write-once seal, an injected pooled `HttpClient`
- Reject: a second `BlobRemote` code path beside the `ObjectStore` union (Minio is one provider row), per-operation client construction, a hand-rolled S3 signing/multipart loop the `*Args` builders own, the `IRetryPolicyHandler` as a second retry owner where the AppHost `OutboundHop` already owns hop retry, credential material as a fence member, an `ObjectNotFoundException` crossing the boundary unwrapped (→ `Option.None`)
