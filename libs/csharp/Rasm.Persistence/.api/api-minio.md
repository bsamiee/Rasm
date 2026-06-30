# [RASM_PERSISTENCE_API_MINIO]

`Minio` is the endpoint-agnostic S3-compatible object client — one client surface dialing MinIO, Cloudflare R2, Backblaze B2, Wasabi, Ceph RadosGW, or any S3 API — the self-hosted lane the cloud-native `AWSSDK.S3`/`Azure.Storage.Blobs`/`Google.Cloud.Storage.V1` rows in `api-objectstore` lack. It composes that catalog directly: Minio is the FOURTH provider on the `ObjectStore` placement / `BlobRemote` contract, supplying the same multipart-upload, head/list/delete, range-read-resume, and conditional-write edges the three cloud SDKs do, behind the same `ObjectClient` union dispatch — so a content-addressed blob lands on a self-hosted MinIO cluster or a cloud bucket by one placement row, never two code paths. The whole surface is the fluent `IMinioClient` builder + the `*Args` request builders; `ObjectStat.ETag`/`MetaData` is the content-hash descriptor and `IfMatch`/`IfNoneMatch` via `CopyConditions` is the optimistic-concurrency seal the write-once placement rides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Minio`
- package: `Minio`
- version: `7.0.0`
- assembly: `Minio`
- namespace: `Minio`, `Minio.ApiEndpoints` (the `IBucketOperations`/`IObjectOperations` op contracts `IMinioClient` derives from), `Minio.Credentials` (`IClientProvider` + the credential-provider family), `Minio.DataModel`, `Minio.DataModel.Args`, `Minio.DataModel.Result`, `Minio.DataModel.Response`, `Minio.DataModel.Encryption`, `Minio.DataModel.Tags`, `Minio.DataModel.Select`, `Minio.DataModel.Notification`, `Minio.DataModel.ILM`, `Minio.DataModel.ObjectLock`, `Minio.DataModel.Replication`, `Minio.Exceptions`, `Minio.Handlers`
- license: Apache-2.0 (`<license type="expression">Apache-2.0</license>`)
- target framework: `net8.0` asset on the `net10.0` floor (package ships `net8.0`/`netstandard2.0`; net8.0 wins NuGet precedence — there is no `net10.0` asset, so the consumer binds the `net8.0` lib)
- dependencies: `System.Reactive` 6.0.1 (the `IObservable<>` bucket-notification stream), `CommunityToolkit.HighPerformance` 8.4.0 (co-consumed substrate), `System.IO.Hashing` 9.0.4 (the content-hash substrate), `Microsoft.Extensions.Logging`/`DependencyInjection.Abstractions` 9.0.4
- asset: runtime library, pure-managed AnyCPU (no native — HTTP/S3 protocol over `HttpClient`)
- rail: object-store (self-hosted S3)

## [02]-[PUBLIC_TYPES]

[CLIENT_TYPES]: the client, its config, and DI wiring
- rail: object-store

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `IMinioClient : IBucketOperations, IObjectOperations, IDisposable` | client contract | the full bucket + object operation surface       |
|  [02]   | `MinioClient : IMinioClient`                              | client          | the concrete client; built via fluent `MinioClientExtensions` |
|  [03]   | `MinioConfig`                                             | config          | endpoint/region/credentials/SSL/timeout/proxy carrier    |
|  [04]   | `IMinioClientFactory` / `MinioClientFactory`             | factory         | DI-resolved client factory (`CreateClient()`)            |
|  [05]   | `IClientProvider`                                         | credentials     | pluggable credential provider (static, IAM, chained, WebIdentity) |
|  [06]   | `Minio.Handlers.IRetryPolicyHandler` / `DefaultRetryPolicyHandler` | retry | request-level retry policy (engine-owned, see RAIL_LAW)  |
|  [07]   | `Minio.Handlers.IRequestLogger` / `DefaultRequestLogger` | tracing         | per-request log hook into the telemetry spine            |

[ARGS_TYPES]: the fluent request builders — `Minio.DataModel.Args`
- rail: object-store

The operation surface is a `*Args` builder hierarchy (`RequestArgs` → `BucketArgs<T>` → `ObjectArgs<T>` → `EncryptionArgs<T>` → `ObjectVersionArgs<T>` → `ObjectConditionalQueryArgs<T>` → `ObjectWriteArgs<T>`), each exposing fluent `With*` setters returning `T`. Every operation takes its matching args object — there is no positional-parameter form.

| [INDEX] | [SYMBOL]                       | [BUILDS]            | [KEY SETTERS / CAPABILITY]                                          |
| :-----: | :----------------------------- | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `MakeBucketArgs`               | bucket create       | `WithBucket`/`WithLocation`/`WithObjectLock`                       |
|  [02]   | `BucketExistsArgs` / `RemoveBucketArgs` | bucket head/drop | `WithBucket`                                                       |
|  [03]   | `ListObjectsArgs`              | object enumeration  | `WithBucket`/`WithPrefix`/`WithRecursive`/`WithVersions`           |
|  [04]   | `PutObjectArgs`                | object write        | own: `WithStreamData`/`WithObjectSize`/`WithFileName`/`WithProgress`; inherited: `WithBucket`/`WithHeaders` (`BucketArgs`), `WithObject` (`ObjectArgs`), `WithContentType`/`WithTagging`/`WithServerSideEncryption`/`WithLegalHold`/`WithRetentionConfiguration` (`ObjectWriteArgs`/`EncryptionArgs`; `WithRetentionConfiguration(ObjectRetentionConfiguration)` writes `x-amz-object-lock-mode`/`x-amz-object-lock-retain-until-date` so a put SETs WORM at write time) — chunked multipart auto-managed |
|  [05]   | `GetObjectArgs`                | object read         | own: `WithOffsetAndLength`/`WithLength` (range read)/`WithCallbackStream`/`WithFile`; inherited: `WithBucket`/`WithObject`/`WithVersionId`/`WithServerSideEncryption` |
|  [06]   | `StatObjectArgs`               | object head         | own: `WithOffsetAndLength`/`WithLength`; inherited (`ObjectConditionalQueryArgs`): `WithMatchETag`/`WithNotMatchETag`/`WithModifiedSince`/`WithUnModifiedSince` (conditional head) |
|  [07]   | `RemoveObjectArgs` / `RemoveObjectsArgs` | object delete | single / bulk delete (`WithObjectsVersions` for versioned batch)   |
|  [08]   | `CopyObjectArgs` / `CopySourceObjectArgs` | server-side copy | `WithCopyObjectSource`/`WithCopyConditions` — S3 server-side copy, no client round-trip |
|  [09]   | `PresignedGetObjectArgs` / `PresignedPutObjectArgs` / `PresignedPostPolicyArgs` | presigned URL | `WithExpiry`/`WithHeaders` — time-bounded delegated access without credentials |
|  [10]   | `SelectObjectContentArgs`      | S3 Select           | `WithExpressionType`/`WithInputSerialization`/`WithOutputSerialization` — server-side SQL over CSV/JSON/Parquet objects |
|  [11]   | `SetObjectTagsArgs` / `GetObjectTagsArgs` / `RemoveObjectTagsArgs` | object tags | `Tagging`/`TagSet` key-value object tags |
|  [12]   | `ListenBucketNotificationsArgs` | event stream       | `WithEvents`/`WithPrefix`/`WithSuffix` — the `IObservable` change feed |
|  [13]   | `SetVersioningArgs` / `GetVersioningArgs` | bucket versioning | enable/suspend object versioning                                  |
|  [14]   | `SetObjectRetentionArgs` / `SetObjectLegalHoldArgs` | object lock | WORM retention mode + legal hold (compliance/governance)          |
|  [15]   | `SetBucketLifecycleArgs` / `SetBucketReplicationArgs` / `SetBucketEncryptionArgs` | bucket policy | ILM lifecycle / cross-region replication / default SSE |

[MODEL_TYPES]: results and descriptors — `Minio.DataModel` / `.DataModel.Result`
- rail: object-store

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ObjectStat`                      | head result   | `ObjectName`/`Size`/`ETag`/`LastModified`/`ContentType`/`MetaData`/`VersionId`/`DeleteMarker`/`TaggingCount`/`Expires`/`ObjectLockMode`/`LegalHoldEnabled` — the `Stat` descriptor |
|  [02]   | `Minio.DataModel.Response.PutObjectResponse` | write result | the put outcome carrying the object `Etag`                  |
|  [03]   | `Item`                            | list element  | `Key`/`Size`/`LastModified`/`ETag`/`IsDir`/`VersionId` — the `List` row |
|  [04]   | `Bucket`                          | bucket        | `Name`/`CreationDate`                                              |
|  [05]   | `CopyConditions`                  | conditions    | `SetMatchETag`/`SetMatchETagNone`/`SetModified`/`SetUnmodified`/`SetReplaceMetadataDirective` — the conditional-copy seal |
|  [06]   | `DeleteError` / `DeletedObject`   | bulk result   | per-object outcome of `RemoveObjectsAsync`                        |
|  [07]   | `Minio.DataModel.Encryption.SSEC` / `SSEKMS` / `SSES3` (`: IServerSideEncryption`) | SSE | customer-key / KMS / S3-managed server-side encryption modes |
|  [07a]  | `Minio.DataModel.ObjectLock.ObjectRetentionConfiguration` / `ObjectRetentionMode` | WORM | `ObjectRetentionConfiguration(DateTime date, ObjectRetentionMode mode)` + `ObjectRetentionMode.GOVERNANCE`/`.COMPLIANCE` — the put-side `WithRetentionConfiguration` retention-until stance |
|  [08]   | `ProgressReport` / `IProgress<ProgressReport>` | progress | per-byte upload/download progress callback                       |
|  [09]   | `Minio.DataModel.Select.*` (`CSVInputOptions`/`JSONInputOptions`/`ParquetInputOptions`/`SelectResponseStream`) | S3 Select IO | input/output serialization for server-side `SELECT` |
|  [10]   | `Minio.DataModel.Notification.*` (`BucketNotification`/`EventType`/`MinioNotificationRaw`) | notification | bucket event configuration + the raw event payload |

## [03]-[ENTRYPOINTS]

[BUILDER]: fluent client construction — `MinioClientExtensions`
- rail: object-store

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `new MinioClient().WithEndpoint(string)` / `WithEndpoint(host, port)` / `WithEndpoint(Uri)` | endpoint | dial ANY S3 endpoint — MinIO/R2/B2/Wasabi/Ceph |
|  [02]   | `.WithCredentials(accessKey, secretKey)` / `.WithSessionToken(st)` | credentials    | static creds / STS session token                      |
|  [03]   | `.WithCredentialsProvider(IClientProvider)`                       | credentials    | IAM/WebIdentity/chained credential provider           |
|  [04]   | `.WithRegion(string)` / `.WithRegion()`                           | region         | explicit / auto-detected bucket region                |
|  [05]   | `.WithSSL(secure = true)`                                         | transport      | TLS toggle                                            |
|  [06]   | `.WithHttpClient(HttpClient, disposeHttpClient = false)`         | transport      | inject a pooled/configured `HttpClient`               |
|  [07]   | `.WithTimeout(int)` / `.WithProxy(IWebProxy)`                     | transport      | request timeout / outbound proxy                      |
|  [08]   | `.WithRetryPolicy(IRetryPolicyHandler)` / `.WithRetryPolicy(Func<…>)` | retry      | request-level retry handler                           |
|  [09]   | `.Build()`                                                        | finalize       | validates config, returns the ready `IMinioClient`    |
|  [10]   | `services.AddMinio(accessKey, secretKey, lifetime)` / `AddMinio(Action<IMinioClient>, lifetime)` | DI | `ServiceCollectionExtensions` DI registration (`IMinioClientFactory`) |

[BUCKET_OPS]: bucket lifecycle and policy — `IBucketOperations`
- rail: object-store

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `MakeBucketAsync(MakeBucketArgs, ct)`                      | create         | create a bucket (region/object-lock aware)         |
|  [02]   | `BucketExistsAsync(BucketExistsArgs, ct)` → `bool`         | head           | existence probe                                    |
|  [03]   | `RemoveBucketAsync(RemoveBucketArgs, ct)`                  | drop           | delete an empty bucket                             |
|  [04]   | `ListBucketsAsync(ct)` → `ListAllMyBucketsResult`          | list           | all buckets for the credential                     |
|  [05]   | `ListObjectsEnumAsync(ListObjectsArgs, ct)` → `IAsyncEnumerable<Item>` | enumerate | streamed prefix/recursive object listing |
|  [06]   | `SetVersioningAsync` / `GetVersioningAsync`                | versioning     | enable/suspend bucket versioning                   |
|  [07]   | `SetBucketEncryptionAsync` / `GetBucketEncryptionAsync` / `RemoveBucketEncryptionAsync` | SSE policy | default bucket server-side encryption |
|  [08]   | `SetBucketLifecycleAsync` / `GetBucketLifecycleAsync`      | ILM            | object lifecycle/expiry rules                       |
|  [09]   | `SetBucketReplicationAsync` / `GetBucketReplicationAsync`  | replication    | cross-bucket/region replication config             |
|  [10]   | `ListenBucketNotificationsAsync(args, ct)` → `IObservable<MinioNotificationRaw>` | event stream | the Reactive bucket-event change feed |

[OBJECT_OPS]: object put / get / stat / remove / copy — `IObjectOperations`
- rail: object-store

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `PutObjectAsync(PutObjectArgs, ct)` → `PutObjectResponse`  | write          | stream upload, multipart auto-chunked above the part threshold |
|  [02]   | `GetObjectAsync(GetObjectArgs, ct)` → `ObjectStat`         | read           | full or **range** read (`WithOffsetAndLength`) to a stream/file/callback |
|  [03]   | `StatObjectAsync(StatObjectArgs, ct)` → `ObjectStat`       | head           | size/ETag/metadata/lock-state — conditional via `WithMatchETag` |
|  [04]   | `RemoveObjectAsync(RemoveObjectArgs, ct)`                  | delete         | single object (or version) delete                            |
|  [05]   | `RemoveObjectsAsync(RemoveObjectsArgs, ct)` → `IList<DeleteError>` | bulk delete | batched multi-object delete                              |
|  [06]   | `CopyObjectAsync(CopyObjectArgs, ct)`                      | copy           | server-side copy with `CopyConditions` (no client transit)    |
|  [07]   | `SelectObjectContentAsync(SelectObjectContentArgs, ct)` → `SelectResponseStream` | S3 Select | server-side `SELECT` over a CSV/JSON/Parquet object |
|  [08]   | `PresignedGetObjectAsync(args)` → `string` / `PresignedPutObjectAsync(args)` → `string` | presigned URL | time-bounded delegated GET/PUT URL |
|  [09]   | `PresignedPostPolicyAsync(args)` → `(Uri, IDictionary<string,string>)` | presigned POST | browser-direct upload form policy           |
|  [10]   | `SetObjectTagsAsync` / `GetObjectTagsAsync` / `RemoveObjectTagsAsync` | tags  | object tag key-value set                                     |
|  [11]   | `SetObjectRetentionAsync` / `SetObjectLegalHoldAsync`      | object lock    | WORM retention + legal hold                                  |
|  [12]   | `ListIncompleteUploadsEnumAsync(args, ct)` → `IAsyncEnumerable<Upload>` / `RemoveIncompleteUploadAsync` | multipart resume | list/abort dangling multipart uploads |

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: the `Minio.Exceptions` family lifted at the object-store edge
- rail: object-store

| [INDEX] | [THROWN]                          | [DISCRIMINANT / CASE]                                          |
| :-----: | :-------------------------------- | :------------------------------------------------------------ |
|  [01]   | `MinioException`                  | the base — `ServerMessage`/`ServerResponse`/`Response` (`ResponseResult`) |
|  [02]   | `BucketNotFoundException` / `ObjectNotFoundException` | the absent → `Option.None` boundary projection |
|  [03]   | `PreconditionFailedException`     | the `412` conditional-write conflict → `RemoteStoreFault.Conflict` |
|  [04]   | `AccessDeniedException` / `ForbiddenException` | authz refusal (4xx)                              |
|  [05]   | `ConnectionException`             | endpoint unreachable / transport failure                      |
|  [06]   | `InvalidBucketNameException` / `InvalidObjectNameException` / `InvalidEndpointException` | client-side validation reject |
|  [07]   | `EntityTooLargeException` / `InvalidContentLengthException` / `UnexpectedShortReadException` | size/length integrity faults |
|  [08]   | `VersionDeletedException` / `MissingObjectLockConfigurationException` | versioning / lock-state faults |
|  [09]   | `ErrorResponseException`          | a generic S3 `ErrorResponse` (carries `ErrorResponse.Code`/`Message`) |

`Minio.Handlers.IApiResponseErrorHandler` / `DefaultErrorHandler` is the seam that maps an S3 HTTP error to the exception family; the `412` on a `WithMatchETag(*)`/conditional `CopyObject` is the optimistic-concurrency conflict, folded to `RemoteStoreFault.Conflict` exactly as the cloud-SDK `412`s in `api-objectstore` — and treated as a benign write-once no-op (the content is already durably present, identical by hash).

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- the fourth `ObjectStore` provider: Minio is the self-hosted/S3-compatible row on the same `Store/remote` `BlobRemote` placement contract as the `api-objectstore` `AWSSDK.S3`/`Azure.Storage.Blobs`/`Google.Cloud.Storage.V1` rows — it supplies the same four legs (`PutObjectAsync`/multipart, `StatObjectAsync` head, `ListObjectsEnumAsync` list, `RemoveObjectAsync` delete) and range-read (`GetObjectArgs.WithOffsetAndLength`) the `ObjectClient` union dispatches, so a content-keyed blob lands on a MinIO/R2/Ceph endpoint by one placement row, never a parallel client surface.
- write-once seal: the inherited `ObjectConditionalQueryArgs<T>.WithMatchETag`/`WithNotMatchETag` (on `StatObjectArgs`/`GetObjectArgs`/`PutObjectArgs`) and `CopyConditions.SetMatchETag`/`SetMatchETagNone` are the optimistic-concurrency edge — a racing second writer to the same content-key `412`s (`PreconditionFailedException`), folded to `RemoteStoreFault.Conflict` and treated as success since the content is identical by hash, the same write-once law `api-objectstore#WRITE_ONCE_SEAL` states for the cloud providers.
- content-hash descriptor: `ObjectStat.ETag` + `ObjectStat.MetaData` is the content-hash/identity evidence the `Version/snapshots` content-addressed spine and `Query/cache` index read, the `XxHash128` (`System.IO.Hashing`) content key being the object name — Minio stores the bytes, the settled `#ARTIFACT_FRAMES` frame law owns framing.
- self-provisioned tier residence: a MinIO cluster is a self-hosted server the `Store/provisioning` tier reaches, the on-prem counterpart to the cloud buckets — the `Store ← Rasm.AppHost/Observability # [HEALTH_PROBE]` reachability probe folds the MinIO endpoint health into the same `HealthContributorRow` as Npgsql/Redis.
- S3 Select pushdown: `SelectObjectContentAsync` over a CSV/JSON/**Parquet** object pushes a server-side `SELECT` into the store, the object-store analogue of the `Query/lanes` columnar pushdown — a stored `ParquetSharp`-written object is queried in place without a full GET.
- Reactive change feed: `ListenBucketNotificationsAsync` → `IObservable<MinioNotificationRaw>` (`System.Reactive`) is an external object-mutation stream a `Sync` ingress can fold, distinct from the internal op-log changefeed.
- credential acquisition is connection input handed over by app roots (`IClientProvider`, access/secret keys) per the `api-objectstore` law, never a Persistence fence member.

[RAIL_LAW]:
- Packages: `Minio` (composes `System.Reactive`, `System.IO.Hashing`)
- Owns: the endpoint-agnostic S3-compatible self-hosted object lane — bucket lifecycle, object put/get/stat/remove/copy, multipart + resume, presigned URLs, SSE, versioning, object-lock/WORM, lifecycle/replication, S3 Select, bucket notifications
- Accept: the fluent `IMinioClient` builder built once and reused (a singleton via `AddMinio`/`IMinioClientFactory`), the `*Args` request builders, `ObjectStat.ETag` as the content descriptor, `WithMatchETag`/`CopyConditions` as the write-once seal, an injected pooled `HttpClient`
- Reject: a second `BlobRemote` code path beside the `ObjectStore` union (Minio is one provider row), per-operation client construction, a hand-rolled S3 signing/multipart loop the `*Args` builders own, the `IRetryPolicyHandler` as a second retry owner where the AppHost `OutboundHop` already owns hop retry, credential material as a fence member, an `ObjectNotFoundException` crossing the boundary unwrapped (→ `Option.None`)
