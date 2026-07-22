# [RASM_PERSISTENCE_API_MINIO]

`Minio` dials any S3-protocol endpoint — a self-hosted MinIO cluster, Ceph RadosGW, R2, B2, Wasabi — through one fluent `IMinioClient` built once and reused, so the self-hosted object plane rides the same provider axis as the cloud SDK rows behind one placement contract. Its whole operation surface is the `*Args` request algebra, and the conditional builder tier with `CopyConditions` supplies the optimistic-concurrency edge a content-addressed write-once store rides. `ObjectStat.ETag` with `ObjectStat.MetaData` is the content-hash descriptor a reader binds back to the content key.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Minio`
- package: `Minio` (Apache-2.0)
- assembly: `Minio`
- namespace: `Minio`, `Minio.ApiEndpoints`, `Minio.Credentials`, `Minio.DataModel`, `Minio.DataModel.Args`, `Minio.DataModel.Encryption`, `Minio.DataModel.ILM`, `Minio.DataModel.Notification`, `Minio.DataModel.ObjectLock`, `Minio.DataModel.Replication`, `Minio.DataModel.Response`, `Minio.DataModel.Result`, `Minio.DataModel.Select`, `Minio.DataModel.Tags`, `Minio.DataModel.Tracing`, `Minio.Exceptions`, `Minio.Handlers`
- target: `net8.0`
- depends: `System.Reactive`, `System.IO.Hashing`, `CommunityToolkit.HighPerformance`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection.Abstractions`
- asset: managed AnyCPU; S3 wire protocol over `HttpClient`
- rail: object-store, self-hosted S3

## [02]-[PUBLIC_TYPES]

[CLIENT_TYPES]: the client contract, its state carrier, and the pluggable credential and handler seams.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `IMinioClient`             | interface     | bucket + object op surface over `IDisposable`     |
|  [02]   | `MinioClient`              | class         | concrete client the fluent extensions build       |
|  [03]   | `MinioConfig`              | class         | endpoint, region, credential, TLS, timeout, proxy |
|  [04]   | `IMinioClientFactory`      | interface     | DI-resolved client construction                   |
|  [05]   | `MinioClientFactory`       | class         | factory implementation                            |
|  [06]   | `IClientProvider`          | interface     | credential acquisition contract                   |
|  [07]   | `AccessCredentials`        | class         | resolved key, secret, session token, expiry       |
|  [08]   | `IApiResponseErrorHandler` | interface     | S3 error response to exception mapping            |
|  [09]   | `IRetryPolicyHandler`      | interface     | request-level retry wrapper                       |
|  [10]   | `IRequestLogger`           | interface     | per-request trace hook                            |

[CREDENTIAL_PROVIDERS]: `AssumeRoleProvider` `AWSEnvironmentProvider` `CertificateIdentityProvider` `ChainedProvider` `IAMAWSProvider` `MinioEnvironmentProvider` `WebIdentityProvider` `WebIdentityClientGrantsProvider`
[HANDLER_DEFAULTS]: `DefaultErrorHandler` `DefaultRequestLogger` `DefaultRetryPolicyHandler`

[ARGS_ALGEBRA]: every op binds its matching builder — `<Name>Async` takes `<Name>Args`, a streaming `<Name>EnumAsync` drops the infix, each `With*` setter returns the builder for chaining, and no positional-parameter form exists. Seven abstract tiers stack the inherited setters, `RequestArgs` -> `BucketArgs<T>` -> `ObjectArgs<T>` -> `EncryptionArgs<T>` -> `ObjectVersionArgs<T>` -> `ObjectConditionalQueryArgs<T>` -> `ObjectWriteArgs<T>`, so a concrete builder reaches every setter of every tier above it.

[BUCKET_ARGS]: `WithBucket` `WithHeaders`
[OBJECT_ARGS]: `WithObject` `WithRequestBody`
[ENCRYPTION_ARGS]: `WithServerSideEncryption`
[OBJECT_VERSION_ARGS]: `WithVersionId`
[OBJECT_CONDITIONAL_QUERY_ARGS]: `WithMatchETag` `WithNotMatchETag` `WithModifiedSince` `WithUnModifiedSince`
[OBJECT_WRITE_ARGS]: `WithContentType` `WithTagging` `WithRetentionConfiguration` `WithLegalHold`

[MODEL_TYPES]: results, descriptors, and the policy configurations the bucket ops read and write.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `ObjectStat`                   | head result    | size, ETag, metadata, lock state descriptor     |
|  [02]   | `PutObjectResponse`            | write result   | put outcome carrying `Etag`/`ObjectName`/`Size` |
|  [03]   | `Item`                         | list element   | one listing row, versioned or current           |
|  [04]   | `Bucket`                       | list element   | `Name`/`CreationDate`                           |
|  [05]   | `Upload`                       | list element   | dangling multipart `Key`/`UploadId`/`Initiated` |
|  [06]   | `CopyConditions`               | conditions     | server-side copy precondition seal              |
|  [07]   | `DeletedObject`                | bulk result    | per-object success of a batched delete          |
|  [08]   | `DeleteError`                  | bulk result    | per-object failure of a batched delete          |
|  [09]   | `ProgressReport`               | progress       | `Percentage`/`TotalBytesTransferred` callback   |
|  [10]   | `IServerSideEncryption`        | interface      | SSE stance stamped onto a request               |
|  [11]   | `ObjectRetentionConfiguration` | WORM           | retention mode with retain-until date           |
|  [12]   | `ObjectRetentionMode`          | enum           | `GOVERNANCE` / `COMPLIANCE` retention modes     |
|  [13]   | `Tagging`                      | tags           | tag set with bucket/object static factories     |
|  [14]   | `SelectResponseStream`         | select result  | the server-side `SELECT` output stream          |
|  [15]   | `PostPolicy`                   | presign policy | browser-direct upload form conditions           |
|  [16]   | `MinioNotificationRaw`         | notification   | raw event payload on the change feed            |
|  [17]   | `ResponseResult`               | transport      | raw response the handler seams inspect          |

[BUCKET_CONFIGS]: `BucketNotification` `LifecycleConfiguration` `ObjectLockConfiguration` `ReplicationConfiguration` `ServerSideEncryptionConfiguration` `VersioningConfiguration`
[SSE_MODES]: `SSEC` `SSECopy` `SSEKMS` `SSES3`
[SELECT_IO]: `CSVInputOptions` `CSVOutputOptions` `JSONInputOptions` `JSONOutputOptions` `ParquetInputOptions` `QueryExpressionType` `RequestProgress`
[OBJECT_STAT_FIELDS]: `ObjectName` `Size` `ETag` `LastModified` `ContentType` `MetaData` `ExtraHeaders` `VersionId` `DeleteMarker` `TaggingCount` `ArchiveStatus` `ReplicationStatus` `Expires` `ObjectLockMode` `ObjectLockRetainUntilDate` `LegalHoldEnabled`
[ITEM_FIELDS]: `Key` `Size` `ETag` `LastModified` `LastModifiedDateTime` `ContentType` `UserMetadata` `IsDir` `IsLatest` `VersionId` `Expires`
[COPY_CONDITION_SETTERS]: `SetMatchETag` `SetMatchETagNone` `SetModified` `SetUnmodified` `SetByteRange` `SetReplaceMetadataDirective` `HasReplaceMetadataDirective` `Clone`

[FAULT_TYPES]: `DefaultErrorHandler` maps every S3 response onto `Minio.Exceptions`; each row drives its own boundary projection, and each roster below folds to one band.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `MinioException`                          | base          | `ServerMessage`/`ServerResponse`/`Response`   |
|  [02]   | `BucketNotFoundException`                 | absence       | bucket absent                                 |
|  [03]   | `ObjectNotFoundException`                 | absence       | object absent                                 |
|  [04]   | `PreconditionFailedException`             | precondition  | `412` conditional-write conflict              |
|  [05]   | `VersionDeletedException`                 | versioning    | version already deleted                       |
|  [06]   | `MissingObjectLockConfigurationException` | lock state    | bucket carries no object-lock configuration   |
|  [07]   | `ErrorResponseException`                  | protocol      | generic `ErrorResponse` with `Code`/`Message` |

[AUTHZ_FAULTS]: `AccessDeniedException` `AuthorizationException` `ForbiddenException`
[TRANSPORT_FAULTS]: `ConnectionException` `RedirectionException` `InternalServerException` `InternalClientException` `UnexpectedMinioException`
[VALIDATION_FAULTS]: `InvalidBucketNameException` `InvalidObjectNameException` `InvalidObjectPrefixException` `InvalidEndpointException` `InvalidExpiryRangeException` `MalFormedXMLException`
[INTEGRITY_FAULTS]: `EntityTooLargeException` `InvalidContentLengthException` `UnexpectedShortReadException` `PartialContentException`
[OPERATION_FAULTS]: `DeleteObjectException` `SelectObjectContentException` `MissingBucketReplicationConfigurationException` `CredentialsProviderException`

## [03]-[ENTRYPOINTS]

[BUILDER]: `MinioClientExtensions` chains off `new MinioClient()`, and `Build()` validates the config and yields the ready client.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `.WithEndpoint(string)` / `(string, int)` / `(Uri)`                        | static   | dial any S3 endpoint                      |
|  [02]   | `.WithCredentials(string, string)`                                         | static   | static access and secret keys             |
|  [03]   | `.WithSessionToken(string)`                                                | static   | STS session token                         |
|  [04]   | `.WithCredentialsProvider(IClientProvider)`                                | static   | IAM, WebIdentity, or chained acquisition  |
|  [05]   | `.WithRegion(string)` / `.WithRegion()`                                    | static   | explicit or auto-detected bucket region   |
|  [06]   | `.WithSSL(bool)`                                                           | static   | TLS toggle                                |
|  [07]   | `.WithHttpClient(HttpClient, bool)`                                        | static   | inject a pooled client, own its disposal  |
|  [08]   | `.WithTimeout(int)` / `.WithProxy(IWebProxy)`                              | static   | request timeout and outbound proxy        |
|  [09]   | `.WithRetryPolicy(IRetryPolicyHandler)`                                    | static   | retry handler as a contract               |
|  [10]   | `.WithRetryPolicy(Func<Func<Task<ResponseResult>>, Task<ResponseResult>>)` | static   | retry handler as a delegate               |
|  [11]   | `.SetAppInfo(string, string)`                                              | static   | stamp the outbound user agent             |
|  [12]   | `.Build() -> IMinioClient`                                                 | static   | validate the config, yield the client     |
|  [13]   | `services.AddMinio(string, string, ServiceLifetime)`                       | static   | DI registration from key and secret       |
|  [14]   | `services.AddMinio(Action<IMinioClient>, ServiceLifetime)`                 | static   | DI registration from a configure delegate |
|  [15]   | `IMinioClientFactory.CreateClient()`                                       | factory  | resolve a client from the DI factory      |
|  [16]   | `IMinioClient.SetTraceOn(IRequestLogger)` / `SetTraceOff`                  | instance | toggle per-request wire tracing           |

[BUCKET_OPS]: `IBucketOperations` — bucket lifecycle, policy, and the event feed; each op takes its builder with a `CancellationToken`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `MakeBucketAsync`                                                     | instance | create a bucket, region and lock aware |
|  [02]   | `BucketExistsAsync -> bool`                                           | instance | existence probe                        |
|  [03]   | `RemoveBucketAsync`                                                   | instance | drop an empty bucket                   |
|  [04]   | `ListBucketsAsync(CancellationToken) -> ListAllMyBucketsResult`       | instance | every bucket for the credential        |
|  [05]   | `ListObjectsEnumAsync -> IAsyncEnumerable<Item>`                      | instance | streamed prefix or recursive listing   |
|  [06]   | `SetVersioningAsync` / `GetVersioningAsync`                           | instance | enable, suspend, or read versioning    |
|  [07]   | `SetBucketEncryptionAsync` / `GetBucketEncryptionAsync`               | instance | default bucket SSE rule set            |
|  [08]   | `RemoveBucketEncryptionAsync`                                         | instance | drop the default bucket SSE            |
|  [09]   | `SetBucketLifecycleAsync` / `GetBucketLifecycleAsync`                 | instance | ILM expiry and transition rules        |
|  [10]   | `RemoveBucketLifecycleAsync`                                          | instance | drop the ILM rules                     |
|  [11]   | `SetBucketReplicationAsync` / `GetBucketReplicationAsync`             | instance | cross-bucket replication config        |
|  [12]   | `RemoveBucketReplicationAsync`                                        | instance | drop the replication config            |
|  [13]   | `SetObjectLockConfigurationAsync` / `GetObjectLockConfigurationAsync` | instance | bucket-default WORM retention rule     |
|  [14]   | `RemoveObjectLockConfigurationAsync`                                  | instance | drop the bucket lock configuration     |
|  [15]   | `SetBucketTagsAsync` / `GetBucketTagsAsync` / `RemoveBucketTagsAsync` | instance | bucket tag set                         |
|  [16]   | `SetPolicyAsync` / `GetPolicyAsync` / `RemovePolicyAsync`             | instance | bucket access policy document          |
|  [17]   | `SetBucketNotificationsAsync` / `GetBucketNotificationsAsync`         | instance | bucket event configuration             |
|  [18]   | `RemoveAllBucketNotificationsAsync`                                   | instance | drop every event configuration         |
|  [19]   | `ListenBucketNotificationsAsync -> IObservable<MinioNotificationRaw>` | instance | live Reactive bucket-event feed        |

[OBJECT_OPS]: `IObjectOperations` — put, get, head, remove, copy, presign, and the WORM legs; each op takes its builder with a `CancellationToken`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `PutObjectAsync -> PutObjectResponse`                            | instance | stream or file upload, multipart chunked  |
|  [02]   | `GetObjectAsync -> ObjectStat`                                   | instance | full or range read to stream, file, sink  |
|  [03]   | `StatObjectAsync -> ObjectStat`                                  | instance | conditional head                          |
|  [04]   | `RemoveObjectAsync`                                              | instance | delete one object or version              |
|  [05]   | `RemoveObjectsAsync -> IList<DeleteError>`                       | instance | batched multi-object delete               |
|  [06]   | `CopyObjectAsync`                                                | instance | server-side copy, no client round-trip    |
|  [07]   | `SelectObjectContentAsync -> SelectResponseStream`               | instance | server-side `SELECT` over a stored object |
|  [08]   | `PresignedGetObjectAsync -> string`                              | instance | time-bounded delegated GET URL            |
|  [09]   | `PresignedPutObjectAsync -> string`                              | instance | time-bounded delegated PUT URL            |
|  [10]   | `PresignedPostPolicyAsync -> (Uri, IDictionary<string, string>)` | instance | browser-direct upload form policy         |
|  [11]   | `SetObjectTagsAsync` / `GetObjectTagsAsync -> Tagging`           | instance | object tag set                            |
|  [12]   | `RemoveObjectTagsAsync`                                          | instance | drop object tags                          |
|  [13]   | `SetObjectRetentionAsync`                                        | instance | stamp WORM retention on an object         |
|  [14]   | `GetObjectRetentionAsync -> ObjectRetentionConfiguration`        | instance | read the retention stance                 |
|  [15]   | `ClearObjectRetentionAsync`                                      | instance | clear retention under governance bypass   |
|  [16]   | `SetObjectLegalHoldAsync` / `GetObjectLegalHoldAsync -> bool`    | instance | legal hold on or off                      |
|  [17]   | `ListIncompleteUploadsEnumAsync -> IAsyncEnumerable<Upload>`     | instance | dangling multipart uploads                |
|  [18]   | `RemoveIncompleteUploadAsync`                                    | instance | abort a dangling multipart upload         |

Each builder adds these setters above its inherited tiers; `Tagging.GetObjectTags(IDictionary)` and `Tagging.GetBucketTags(IDictionary)` mint the set every `WithTagging` takes.

[MAKE_BUCKET_ARGS]: `WithLocation` `WithObjectLock`
[LIST_OBJECTS_ARGS]: `WithPrefix` `WithRecursive` `WithVersions` `WithIncludeUserMetadata` `WithListObjectsV1`
[PUT_OBJECT_ARGS]: `WithStreamData` `WithFileName` `WithObjectSize` `WithProgress`
[GET_OBJECT_ARGS]: `WithOffsetAndLength` `WithLength` `WithCallbackStream` `WithFile`
[STAT_OBJECT_ARGS]: `WithOffsetAndLength` `WithLength`
[REMOVE_OBJECT_ARGS]: `WithVersionId` `WithBypassGovernanceMode`
[REMOVE_OBJECTS_ARGS]: `WithObjects` `WithObjectAndVersions` `WithObjectsVersions`
[COPY_OBJECT_ARGS]: `WithCopyObjectSource` `WithReplaceMetadataDirective` `WithReplaceTagsDirective` `WithObjectLockMode` `WithObjectLockRetentionDate` `WithRetentionUntilDate`
[COPY_SOURCE_OBJECT_ARGS]: `WithCopyConditions`
[SELECT_OBJECT_CONTENT_ARGS]: `WithExpressionType` `WithQueryExpression` `WithInputSerialization` `WithOutputSerialization` `WithRequestProgress`
[PRESIGNED_GET_OBJECT_ARGS]: `WithExpiry` `WithRequestDate`
[PRESIGNED_PUT_OBJECT_ARGS]: `WithExpiry`
[PRESIGNED_POST_POLICY_ARGS]: `WithExpiration` `WithPolicy`
[SET_OBJECT_RETENTION_ARGS]: `WithRetentionMode` `WithRetentionUntilDate` `WithBypassGovernanceMode`
[SET_OBJECT_LEGAL_HOLD_ARGS]: `WithLegalHold`
[SET_VERSIONING_ARGS]: `WithVersioningEnabled` `WithVersioningSuspended`
[SET_BUCKET_ENCRYPTION_ARGS]: `WithEncryptionConfig` `WithAESConfig` `WithKMSConfig`
[SET_BUCKET_LIFECYCLE_ARGS]: `WithLifecycleConfiguration`
[SET_BUCKET_REPLICATION_ARGS]: `WithConfiguration`
[SET_OBJECT_LOCK_CONFIGURATION_ARGS]: `WithLockConfiguration`
[SET_TAGS_ARGS]: `WithTagging`
[SET_POLICY_ARGS]: `WithPolicy`
[LIST_INCOMPLETE_UPLOADS_ARGS]: `WithPrefix` `WithDelimiter` `WithRecursive`
[LISTEN_BUCKET_NOTIFICATIONS_ARGS]: `WithEvents` `WithPrefix` `WithSuffix` `WithNotificationObserver`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One client is built once and reused for the process; the builder validates endpoint, credential, and region at `Build()`, so a per-operation construction re-pays region discovery and abandons the pooled `HttpClient`.
- Every request is a builder instance, so a policy — SSE stance, WORM retention, precondition ETag, tag set — is a setter on the args object rather than an argument on the op, and an unsupported combination fails at `Validate()` before the wire.
- `WithMatchETag`/`WithNotMatchETag` on the conditional tier and `CopyConditions` on a server-side copy are the optimistic-concurrency edge: a racing write to an existing content key returns `412` as `PreconditionFailedException`, which a content-addressed store reads as durable presence rather than failure.
- `ObjectStat.ETag` with `ObjectStat.MetaData` carries the stored identity evidence, so a reader binds an object back to its content key without re-reading the bytes.

[STACKING]:
- `api-objectstore`(`.api/api-objectstore.md`): peer provider row on the same `ObjectClient` union and `BlobRemote` placement contract, supplying the put, head, list, and delete legs, the `GetObjectArgs.WithOffsetAndLength` range read, and the write-once seal the cloud SDK rows supply through `IfNoneMatch`.
- `api-fastcdc`(`.api/api-fastcdc.md`): `FastCdc.GetChunks()` cut boundaries pack into `PutObjectArgs` multipart parts, so an upload window spans whole content-defined chunks and a re-upload skips the chunks the index already holds.
- `api-parquetsharp`(`.api/api-parquetsharp.md`): a `ParquetFileWriter` managed-`Stream` sink writes straight into an upload stream, and `SelectObjectContentAsync` with `ParquetInputOptions` then pushes the predicate into the store, reading the stored object in place instead of a full GET.
- `api-rocksdb`(`.api/api-rocksdb.md`): checkpoint and SST exports land as objects on this lane, and the symmetric bulk-restore reads them back through a range or full read.
- `api-rabbitmq`(`.api/api-rabbitmq.md`): dead-lettered payloads and shovel snapshots take the same object residence as every other egress sink.
- `api-deltalake`(`.api/api-deltalake.md`): delta-rs reaches a self-hosted S3 residence natively, so a table's files and this client's objects share one endpoint and credential.
- `api-pollination-sdk`(`.api/api-pollination-sdk.md`): presigned upload and download requests resolve on the same S3 plane, so a fetched asset lands content-keyed through this transfer rather than a second HTTP uploader.
- Within-lib: the builder tiers compose to full depth in one chain — SSE stance, version id, precondition ETag, and WORM retention stack onto one `PutObjectArgs`, `IProgress<ProgressReport>` streams transfer telemetry, and `IClientProvider` chains credential acquisition without a second client.
- Within-lib: `ListenBucketNotificationsAsync` carries mutations originating outside this process, so it feeds the ledger as an external ingress rather than the op-log the event store projects from its own events.

[LOCAL_ADMISSION]:
- Endpoint, region, and credential material are host-resolved connection inputs handed in at composition, never fence members.
- Every lifted exception folds once at the object-store edge into the closed local fault family, so no provider exception type crosses into domain code.
- Hop retry belongs to the AppHost `OutboundHop` owner, so the client is built with no retry handler and a transient S3 failure re-drives there.
- A self-hosted endpoint is provisioned infrastructure, so its reachability probe joins the same host degradation grade as the database and cache endpoints.

[RAIL_LAW]:
- Package: `Minio`
- Owns: the S3-protocol self-hosted object lane — bucket lifecycle and policy, object put, get, head, remove, copy, multipart resume, presigned URLs and POST policies, SSE, versioning, object-lock and legal hold, ILM and replication, server-side `SELECT`, and the Reactive bucket-event feed.
- Accept: one client built once and shared, the `*Args` builders carrying every per-request policy, `ObjectStat.ETag` as the content descriptor, the conditional tier and `CopyConditions` as the write-once seal, an injected pooled `HttpClient`.
- Reject: a second code path beside the provider union, per-operation client construction, a hand-rolled S3 signing or multipart loop the builders own, a second retry owner beside the outbound hop, credential material as a fence member, an unwrapped provider exception crossing the boundary.
