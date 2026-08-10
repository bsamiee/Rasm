# [RASM_PERSISTENCE_API_OBJECTSTORE]

`AWSSDK.S3`, `Azure.Storage.Blobs`, and `Google.Cloud.Storage.V1` are the three cloud object-store SDKs the `Store/blobstore#OBJECT_STORE` placement rows dispatch, each supplying the chunked/resumable transfer, content-hash/ETag descriptor, conditional-write optimistic-concurrency edge, SSE-KMS/SSE-C encryption stance, and WORM object-lock retention members a content-addressed blob write consumes. `Minio` (`api-minio`) is the fourth, self-hosted provider row on the same `ObjectClient` union.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AWSSDK.S3`
- package: `AWSSDK.S3`
- assembly: `AWSSDK.S3`
- namespace: `Amazon.S3`, `Amazon.S3.Transfer`
- asset: runtime library
- rail: object-store

[PACKAGE_SURFACE]: `Azure.Storage.Blobs`
- package: `Azure.Storage.Blobs`
- assembly: `Azure.Storage.Blobs`
- namespace: `Azure.Storage.Blobs`, `Azure.Storage.Blobs.Specialized`, `Azure.Storage.Blobs.Models`
- asset: runtime library
- rail: object-store

[PACKAGE_SURFACE]: `Azure.Storage.Blobs.Batch`
- package: `Azure.Storage.Blobs.Batch`
- assembly: `Azure.Storage.Blobs.Batch`
- namespace: `Azure.Storage.Blobs.Specialized`, `Azure.Storage.Blobs.Batch`
- asset: runtime library; separate distribution from `Azure.Storage.Blobs`, whose own assembly carries no batch type
- rail: object-store

[PACKAGE_SURFACE]: `Google.Cloud.Storage.V1`
- package: `Google.Cloud.Storage.V1`
- assembly: `Google.Cloud.Storage.V1`
- namespace: `Google.Cloud.Storage.V1`
- asset: runtime library
- rail: object-store

## [02]-[PUBLIC_TYPES]

[S3_TYPES]: AWSSDK.S3 multipart and transfer

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]      | [CAPABILITY]                                                            |
| :-----: | :----------------------------------- | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `AmazonS3Client : IAmazonS3`         | client             | low-level multipart and object ops                                      |
|  [02]   | `InitiateMultipartUploadRequest`     | request            | begins multipart, carries key/bucket                                    |
|  [03]   | `InitiateMultipartUploadResponse`    | response           | yields `UploadId`                                                       |
|  [04]   | `UploadPartRequest`                  | request            | one part stream + `PartNumber`                                          |
|  [05]   | `UploadPartResponse`                 | response           | yields part `ETag`                                                      |
|  [06]   | `CompleteMultipartUploadRequest`     | request            | carries `PartETags` list                                                |
|  [07]   | `CompleteMultipartUploadResponse`    | response           | yields object `ETag`/`Location`                                         |
|  [08]   | `AbortMultipartUploadRequest`        | request            | abandons an in-flight upload                                            |
|  [09]   | `ListPartsRequest`                   | request            | lists committed parts for resume                                        |
|  [10]   | `ListPartsResponse`                  | response           | `Parts` (`PartDetail` `PartNumber`/`ETag`) for resume skip              |
|  [11]   | `PartETag`                           | value              | `(PartNumber, ETag)` pair                                               |
|  [12]   | `TransferUtility`                    | high-level surface | managed multipart upload                                                |
|  [13]   | `TransferUtilityUploadRequest`       | request            | `PartSize`/stream high-level config                                     |
|  [14]   | `GetObjectRequest`                   | request            | range-read resumption (`ByteRange`)                                     |
|  [15]   | `GetObjectResponse`                  | response           | `ResponseStream` range-read body                                        |
|  [16]   | `GetObjectMetadataResponse`          | response           | `Metadata`/`ContentLength`/`ETag` for `Stat`                            |
|  [17]   | `ListObjectsV2Request`               | request            | content-key namespace enumeration                                       |
|  [18]   | `ListObjectsV2Response`              | response           | `S3Objects` (`S3Object` `Key`/`Size`/`LastModified`)                    |
|  [19]   | `DeleteObjectRequest`                | request            | removes object by content-key name                                      |
|  [20]   | `S3StorageClass`                     | enum               | storage-class column                                                    |
|  [21]   | `ChecksumAlgorithm`                  | `ConstantClass`    | `XXHASH128` (content-key stance), `CRC64NVME`/`SHA256`/`CRC32`/`CRC32C` |
|  [22]   | `ChecksumType`                       | `ConstantClass`    | `FULL_OBJECT` (whole-object) vs. `COMPOSITE` (per-part roll-up)         |
|  [23]   | `ServerSideEncryptionMethod`         | `ConstantClass`    | `AWSKMS` (`aws:kms`, SSE-KMS), `AES256` (SSE-S3), `AWSKMSDSSE`, `None`  |
|  [24]   | `ServerSideEncryptionCustomerMethod` | `ConstantClass`    | SSE-C: `AES256`, `None`                                                 |
|  [25]   | `ObjectLockMode`                     | `ConstantClass`    | WORM: `Governance` (`GOVERNANCE`), `Compliance` (`COMPLIANCE`)          |
|  [26]   | `ObjectLockLegalHoldStatus`          | `ConstantClass`    | legal-hold `On`/`Off`                                                   |
|  [27]   | `ChecksumMode`                       | `ConstantClass`    | read-side validation; exactly ONE member, `ENABLED`                     |
|  [28]   | `HttpVerb`                           | enum               | presign verb: `GET`, `HEAD`, `PUT`, `DELETE`                            |
|  [29]   | `GetPreSignedUrlRequest`             | request            | `Verb`/`Expires` (`DateTime?`)/`Protocol`/`Parameters`/`Headers`        |
|  [30]   | `CreatePresignedPostRequest`         | request            | browser-direct form policy, the POST counterpart of the URL presign     |
|  [31]   | `RestoreObjectRequest`               | request            | `Days`/`Tier`/`RetrievalTier`/`OutputLocation`/`RestoreRequestType`     |
|  [32]   | `RestoreObjectResponse`              | response           | `RequestCharged` + `RestoreOutputPath`, the whole surface               |
|  [33]   | `GlacierJobTier`                     | `ConstantClass`    | `Bulk`, `Expedited`, `Standard` — each value its own name               |
|  [34]   | `CopyObjectRequest`                  | request            | `StorageClass`/`MetadataDirective`/`TaggingDirective`/`ETagToMatch`     |
|  [35]   | `S3MetadataDirective`                | enum               | `COPY`/`REPLACE`; a true enum where `TaggingDirective` is a class       |
|  [36]   | `TaggingDirective`                   | `ConstantClass`    | `COPY` (`"COPY"`), `REPLACE` (`"REPLACE"`)                              |
|  [37]   | `DeleteObjectsRequest`               | request            | `List<KeyVersion> Objects`, `bool? Quiet`, `BypassGovernanceRetention`  |
|  [38]   | `KeyVersion`                         | value              | `Key`/`VersionId`/`ETag`/`Size`; NO constructor, initializer only       |
|  [39]   | `DeletedObject`                      | bulk result        | `Key`/`VersionId`/`DeleteMarker`/`DeleteMarkerVersionId`                |
|  [40]   | `Amazon.S3.Model.DeleteError`        | bulk result        | `Code`/`Key`/`Message`/`VersionId` per refused key                      |
|  [41]   | `DeleteObjectsResponse`              | response           | `DeletedObjects` + `DeleteErrors` (plural) + `RequestCharged`           |

[AZURE_TYPES]: Azure.Storage.Blobs staged-block and parallel upload

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------ | :------------ | :---------------------------------------------------------------- |
|  [01]   | `BlobContainerClient`                             | container     | `GetBlobClient`/`GetBlobs` namespace ops                          |
|  [02]   | `BlockBlobClient : BlobBaseClient`                | client        | staged-block upload                                               |
|  [03]   | `BlobClient : BlobBaseClient`                     | client        | simple/parallel upload                                            |
|  [04]   | `SpecializedBlobExtensions`                       | extension     | `GetBlockBlobClient(this BlobContainerClient, name)`              |
|  [05]   | `BlockBlobStageBlockOptions`                      | options       | per-block conditions/progress                                     |
|  [06]   | `CommitBlockListOptions`                          | options       | headers/metadata/tags/conditions                                  |
|  [07]   | `BlobUploadOptions`                               | options       | carries `TransferOptions`                                         |
|  [08]   | `BlobDownloadOptions`                             | options       | `Range` (`HttpRange`) range-read                                  |
|  [09]   | `StorageTransferOptions`                          | value         | chunk-size and concurrency tuning                                 |
|  [10]   | `HttpRange`                                       | value         | `(offset, length)` range window                                   |
|  [11]   | `BlockListTypes`                                  | enum          | `Committed`/`Uncommitted`/`All` filter for resume                 |
|  [12]   | `BlockList`                                       | response      | `CommittedBlocks`/`UncommittedBlocks` for resume skip             |
|  [13]   | `BlockInfo`                                       | response      | block `ContentHash`/`ContentCrc64`                                |
|  [14]   | `BlobContentInfo`                                 | response      | object `ETag`/`ContentHash`                                       |
|  [15]   | `BlobProperties`                                  | response      | `Metadata`/`ContentLength`/`ETag` for `Stat`                      |
|  [16]   | `BlobItem`                                        | list element  | `Name`/`Properties` for `List`                                    |
|  [17]   | `BlobRequestConditions`                           | value         | `IfMatch`/`IfNoneMatch` ETag gate; `ETag.All` write-once          |
|  [18]   | `BlobDownloadStreamingResult`                     | response      | range-read resumption stream (`.Content`)                         |
|  [19]   | `Azure.Storage.UploadTransferValidationOptions`   | value         | `ChecksumAlgorithm` + `PrecalculatedChecksum` (Common)            |
|  [20]   | `Azure.Storage.StorageChecksumAlgorithm`          | enum          | `Auto`/`None`/`MD5`/`StorageCrc64`; `StorageCrc64` = Azure stance |
|  [21]   | `Azure.Storage.DownloadTransferValidationOptions` | value         | `ChecksumAlgorithm` + `AutoValidateChecksum` (Common)             |
|  [22]   | `AccessTier`                                      | struct        | `P4`-`P80`, `Hot`, `Cool`, `Archive`, `Premium`, `Cold`, `Smart`  |
|  [23]   | `RehydratePriority`                               | enum          | `High`/`Standard`                                                 |
|  [24]   | `ArchiveStatus`                                   | enum          | `RehydratePendingTo{Hot,Cool,Cold,Smart}`                         |
|  [25]   | `BlobImmutabilityPolicy`                          | value         | `ExpiresOn` + `PolicyMode`, both nullable                         |
|  [26]   | `BlobImmutabilityPolicyMode`                      | enum          | `Mutable`/`Unlocked`/`Locked`                                     |
|  [27]   | `BlobLegalHoldResult`                             | response      | payload `SetLegalHoldAsync` returns inside its `Response<T>`      |
|  [28]   | `DeleteSnapshotsOption`                           | enum          | `None`/`IncludeSnapshots`/`OnlySnapshots`                         |
|  [29]   | `BlobBatchClient`                                 | client        | batch client, container- or service-scoped                        |
|  [30]   | `BlobBatch`                                       | batch         | `IDisposable`; `RequestCount`, delayed per-op `Response`          |
|  [31]   | `BatchDeleteBlobOptions`                          | options       | `SnapshotsOption`/`Conditions`/`VersionId`                        |

[GCS_TYPES]: Google.Cloud.Storage.V1 resumable upload

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                        |
| :-----: | :----------------------- | :---------------- | :-------------------------------------------------- |
|  [01]   | `StorageClient`          | client (abstract) | upload/download/list/delete object ops              |
|  [02]   | `UploadObjectOptions`    | options           | `ChunkSize` resumable + generation-match gate       |
|  [03]   | `DownloadObjectOptions`  | options           | `Range` (`RangeHeaderValue`) range-read             |
|  [04]   | `GetObjectOptions`       | options           | generation/projection on `Stat`                     |
|  [05]   | `DeleteObjectOptions`    | options           | generation-match on delete                          |
|  [06]   | `ListObjectsOptions`     | options           | prefix/delimiter list tuning                        |
|  [07]   | `Object`                 | value             | destination descriptor + `Generation`/`Crc32c`      |
|  [08]   | `Objects`                | list page         | `Items` page over `PagedEnumerable`                 |
|  [09]   | `IUploadProgress`        | progress          | per-chunk byte progress                             |
|  [10]   | `EncryptionKey`          | value             | CSEK customer-supplied encryption key               |
|  [11]   | `PredefinedObjectAcl`    | enum              | predefined-acl column                               |
|  [12]   | `Object.RetentionData`   | value             | per-object `Mode` + `RetainUntilTimeDateTimeOffset` |
|  [13]   | `PatchObjectOptions`     | options           | `bool? OverrideUnlockedRetention`                   |
|  [14]   | `UpdateObjectOptions`    | options           | same override column on the full-replace verb       |
|  [15]   | `CopyObjectOptions`      | options           | 17 members, NO storage class; `ExtraMetadata`       |
|  [16]   | `DownloadValidationMode` | enum              | `Always`/`Never`/`Automatic`                        |
|  [17]   | `RestoreObjectOptions`   | options           | SOFT-DELETE restore over a generation               |

[FAULT_TYPES]: SDK exception surfaces lifted at the object-store edge

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `AmazonS3Exception`      | exception     | `StatusCode` (HttpStatusCode) + `ErrorCode` discriminant |
|  [02]   | `RequestFailedException` | exception     | `Status` (int) + `ErrorCode` (string) discriminant       |
|  [03]   | `GoogleApiException`     | exception     | `HttpStatusCode` + `Error.Code` discriminant             |

## [03]-[ENTRYPOINTS]

[S3_MULTIPART]: low-level multipart over `AmazonS3Client`

| [INDEX] | [SURFACE]                      | [SHAPE]                   | [CAPABILITY]      |
| :-----: | :----------------------------- | :------------------------ | :---------------- |
|  [01]   | `InitiateMultipartUploadAsync` | request plus cancellation | begins upload     |
|  [02]   | `UploadPartAsync`              | request plus cancellation | uploads one part  |
|  [03]   | `CompleteMultipartUploadAsync` | request plus cancellation | seals object      |
|  [04]   | `AbortMultipartUploadAsync`    | request plus cancellation | abandons upload   |
|  [05]   | `GetObjectAsync`               | request plus cancellation | range-read fetch  |
|  [06]   | `TransferUtility.UploadAsync`  | request plus cancellation | managed multipart |

- `InitiateMultipartUploadRequest`: SSE (`ServerSideEncryptionMethod`/`ServerSideEncryptionCustomerMethod`), WORM (`ObjectLockMode` + `ObjectLockRetainUntilDate`), and the whole-object checksum stance (`ChecksumType.FULL_OBJECT` + `ChecksumAlgorithm.XXHASH128`) all ride the INITIATE; `UploadPartRequest` carries the per-part `Checksum*` digest and the precomputed whole-object digest rides `CompleteMultipartUploadRequest.Checksum*`.

[S3_LIFECYCLE]: restore, storage-class copy, and batch delete over `IAmazonS3`

| [INDEX] | [SURFACE]                                                      | [SHAPE]                   | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------- | :------------------------ | :---------------------------- |
|  [01]   | `RestoreObjectAsync`                                           | request plus cancellation | archive thaw request          |
|  [02]   | `CopyObjectAsync`                                              | request plus cancellation | server-side copy and re-class |
|  [03]   | `DeleteObjectsAsync`                                           | request plus cancellation | one page of keys, single verb |
|  [04]   | `GetPreSignedURL(GetPreSignedUrlRequest) -> string`            | instance                  | V4 signed URL, any verb       |
|  [05]   | `GetPreSignedURLAsync(GetPreSignedUrlRequest) -> Task<string>` | instance                  | the async twin, NO token      |

- `RestoreObjectRequest` declares `BucketName`, `ChecksumAlgorithm`, `int? Days`, `Description`, `ExpectedBucketOwner`, `Key`, `OutputLocation`, `RequestPayer`, `RestoreRequestType`, `GlacierJobTier RetrievalTier`, `SelectParameters`, `GlacierJobTier Tier`, `VersionId`. Progress reads back on `GetObjectResponse.RestoreInProgress` (`bool?`) and `RestoreExpiration` (`DateTime?`); `GetObjectResponse.StorageClass` is `S3StorageClass`, not a string.
- `CopyObjectRequest` re-classes in place on a same-key self-copy. `MetadataDirective`/`TaggingDirective` default to replacing nothing only when set to `COPY` — a `REPLACE` on either drops user metadata or tags the copy did not restate, so a metadata-bearing object states both. `Expires` on this request is a `string`, unlike the `DateTime?` on the presign request.
- `DeleteObjectsRequest.Objects` types as `List<KeyVersion>`, NOT `List<string>`, and `KeyVersion` declares no constructor. `Quiet` types as `bool?` and its quiet form suppresses `DeletedObjects`, so a caller needing both halves sets it FALSE. Its response spells the refusal half `DeleteErrors`, plural.
- `GetPreSignedUrlRequest` carries NO checksum member, NO `ExpectedBucketOwner`, and no duration alternative to `Expires`. `HttpVerb` declares as a plain enum, so `DELETE` presigns like any other verb and needs no SDK-specific surface.
- `AmazonS3Config.ForcePathStyle` declares on the S3 config; `ServiceURL`, `UseHttp`, and `AuthenticationRegion` declare on `Amazon.Runtime.ClientConfig` in AWSSDK.Core. TRAP: setting `ServiceURL` NULLS `RegionEndpoint` and the two are mutually exclusive with last-write-wins, so an endpoint dial that also pins a region silently drops one. There is no `SignatureVersion` config knob; `SignatureMethod` is the only settable signing column.

[AZURE_BLOCKS]: staged-block over `BlockBlobClient`

| [INDEX] | [SURFACE]                | [SHAPE]                            | [CAPABILITY]     |
| :-----: | :----------------------- | :--------------------------------- | :--------------- |
|  [01]   | `StageBlockAsync`        | block id, stream, options          | one block        |
|  [02]   | `CommitBlockListAsync`   | block ids plus options             | seals blob       |
|  [03]   | `BlobClient.UploadAsync` | stream plus upload options         | parallel chunked |
|  [04]   | `DownloadStreamingAsync` | download options plus cancellation | range-read fetch |

[AZURE_LIFECYCLE]: tier, immutability, and batch over `BlobBaseClient` and `BlobBatchClient`

| [INDEX] | [SURFACE]                                                            | [SHAPE]   | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------- | :-------- | :---------------------------- |
|  [01]   | `SetAccessTierAsync(AccessTier, conditions, RehydratePriority?, ct)` | instance  | metadata-only tier and thaw   |
|  [02]   | `StartCopyFromUriAsync(…, AccessTier?, RehydratePriority?, ct)`      | instance  | tier-on-copy, cross-scope     |
|  [03]   | `SetImmutabilityPolicyAsync -> Response<BlobImmutabilityPolicy>`     | instance  | per-blob WORM window          |
|  [04]   | `DeleteImmutabilityPolicyAsync -> Response`                          | instance  | release an unlocked window    |
|  [05]   | `SetLegalHoldAsync(bool, ct) -> Response<BlobLegalHoldResult>`       | instance  | indefinite hold on or off     |
|  [06]   | `SpecializedBlobExtensions.GetBlobBatchClient`                       | extension | batch client from either root |
|  [07]   | `BlobBatchClient.CreateBatch() -> BlobBatch`                         | instance  | one homogeneous batch         |
|  [08]   | `SubmitBatchAsync(BlobBatch, bool throwOnAnyFailure, ct)`            | instance  | submit, keep per-op status    |
|  [09]   | `DeleteBlobsAsync(IEnumerable<Uri>, DeleteSnapshotsOption, ct)`      | instance  | convenience; always throws    |
|  [10]   | `SetBlobsAccessTierAsync(IEnumerable<Uri>, AccessTier, …, ct)`       | instance  | batched re-class or thaw      |
|  [11]   | `BlobBatch.DeleteBlob(container, blob, BatchDeleteBlobOptions?)`     | instance  | delayed per-op response       |
|  [12]   | `BlobBatch.SetBlobAccessTier(container, blob, AccessTier, …)`        | instance  | delayed per-op response       |

- TRAP: `DeleteBlobs`/`DeleteBlobsAsync` and `SetBlobsAccessTier`/`SetBlobsAccessTierAsync` submit with `throwOnAnyFailure: true` internally, raising an `AggregateException` on ANY sub-failure, so a typed partial-failure tally is unreachable through them. Both halves need `CreateBatch()`, `SubmitBatchAsync(batch, throwOnAnyFailure: false)`, and a read of each delayed `Response.Status`.
- TRAP: batches admit ONE operation type and refuses a mixed set, refuses an empty set, and refuses resubmission. No page ceiling is enforced client-side: the package declares no maximum-subrequest constant and validates none, so the service rejects an oversized submission and the consumer states its own page bound.
- TRAP: `BlobItemProperties.AccessTier`/`ArchiveStatus`/`RehydratePriority` are strongly typed nullables while `BlobProperties`'s three are get-only STRINGS — the listing surface types what the properties surface stringifies, so a head-based read parses text where a list-based read does not.
- `BlobDownloadOptions` carries exactly `Range`, `Conditions`, `ProgressHandler`, `TransferValidation`; `BlobDownloadToOptions` swaps `Range` for `TransferOptions`. `DownloadTransactionalHashingOptions` is the retired preview spelling and does not exist. `BlobDownloadDetails.ContentHash`/`ContentCrc`/`BlobContentHash` are all `internal set`.

[GCS_RESUMABLE]: resumable chunked over `StorageClient`

| [INDEX] | [SURFACE]                      | [SHAPE]                               | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------------------------------ | :----------------------------------------- |
|  [01]   | `UploadObjectAsync`            | object, stream, options               | resumable upload                           |
|  [02]   | `DownloadObjectAsync`          | bucket, object, stream, options       | range-read fetch                           |
|  [03]   | `StorageClient.CreateAsync(…)` | `GoogleCredential?`, `EncryptionKey?` | client factory; app-root credential + CSEK |

- `UploadObjectOptions.ChunkSize`: selects resumable chunked upload at a positive multiple of 262144 and single-request upload at null; generation-match gates (`IfGenerationMatch`) ride the same options.

[GCS_LIFECYCLE]: per-object retention and storage-class rewrite over `StorageClient`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `PatchObjectAsync(Object, PatchObjectOptions?, ct)`         | instance | partial update; retention write |
|  [02]   | `UpdateObjectAsync(Object, UpdateObjectOptions?, ct)`       | instance | full replace; same override     |
|  [03]   | `CopyObjectAsync(bucket, name, bucket, name, options, ct)`  | instance | rewrite; re-class in place      |
|  [04]   | `RestoreObjectAsync(bucket, name, generation, options, ct)` | instance | SOFT-DELETE restore, not thaw   |

- `Object.Retention` types as `Object.RetentionData`, carrying `string Mode` (`"Unlocked"` or `"Locked"` alone) and `DateTimeOffset? RetainUntilTimeDateTimeOffset`; its `DateTime?` `RetainUntilTime` twin is `[Obsolete]`. `PatchObjectOptions`/`UpdateObjectOptions` both carry `bool? OverrideUnlockedRetention`, which reaches an unlocked window only.
- TRAP: `Object.RetentionExpirationTimeRaw` is the read-only BUCKET-policy expiry, a different field on the same resource — a per-object window never writes there and never reads back from there.
- TRAP: `CopyObjectOptions` has NO storage-class member across its whole property set; the class rides `ExtraMetadata`, typed as `Object`, whose own `StorageClass` string carries the value. Destination bucket and name are optional in the signature yet FAIL when null, an upstream mistake the member's own documentation names, so both spell explicitly. No public `Rewrite*` member exists.
- STRUCTURAL NEGATIVE, archive thaw: this client's object verbs are exactly copy, download, get, move, patch, restore, update, upload, delete, list, `CreateObjectUploader`, and `InitiateUploadSessionAsync`. No thaw verb exists because the GCS archive class is instantly readable and needs none; `RestoreObjectAsync` restores a soft-deleted generation and is not a thaw.
- STRUCTURAL NEGATIVE, batch delete: that same enumeration carries no batched delete, so a page-at-a-time erase degrades to the per-object verb.

[OBJECT_CRUD]: head / list / delete / multipart-resume across the four providers

One unified leg dispatches on the `ObjectClient` union: each leg takes bucket + content-key with its provider args object and returns the provider descriptor. `Minio` (`api-minio`) owns its `*Args`/`IMinioClient` member facts; this grid reproduces the Minio column only as the union-dispatch contract.

| [INDEX] | [LEG]        | [S3]                     | [AZURE]                  | [GCS]                 | [MINIO]                          |
| :-----: | :----------- | :----------------------- | :----------------------- | :-------------------- | :------------------------------- |
|  [01]   | `Stat`       | `GetObjectMetadataAsync` | `GetPropertiesAsync`     | `GetObjectAsync`      | `StatObjectAsync`                |
|  [02]   | `List`       | `ListObjectsV2Async`     | `GetBlobs`               | `ListObjects`         | `ListObjectsEnumAsync`           |
|  [03]   | `Delete`     | `DeleteObjectAsync`      | `DeleteIfExistsAsync`    | `DeleteObjectAsync`   | `RemoveObjectAsync`              |
|  [04]   | `Get`        | `GetObjectAsync`         | `DownloadStreamingAsync` | `DownloadObjectAsync` | `GetObjectAsync`                 |
|  [05]   | resume       | `ListPartsAsync`         | `GetBlockListAsync`      | server-side           | `ListIncompleteUploadsEnumAsync` |
|  [06]   | block client | n/a                      | `GetBlockBlobClient`     | n/a                   | n/a                              |

- resume: `MultipartTransfer` skips already-committed windows — S3 reads prior `PartETag`s by `UploadId`, Azure reads prior uncommitted block ids (`BlockListTypes.Uncommitted`), GCS resumes its session server-side, Minio enumerates dangling uploads.
- `GetBlockBlobClient` is a `SpecializedBlobExtensions` extension on `BlobContainerClient`, not an instance member.

[PRESIGN]: TTL-boxed grant issuance the `ObjectLeg.Issue` leg mints, one signing surface per provider row

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `AmazonS3Client.GetPreSignedURL(GetPreSignedUrlRequest) -> string`     | instance | V4 signed URL from the client credential       |
|  [02]   | `BlobClient.GenerateSasUri(BlobSasPermissions, DateTimeOffset) -> Uri` | instance | service SAS; needs a shared-key-dialed client  |
|  [03]   | `UrlSigner.FromCredential(GoogleCredential) -> UrlSigner`              | factory  | credential-bound signer for GCS V4 signed URLs |

- `AmazonS3Client.GetPreSignedURL`: custom `Parameters` with an expiry past 7 days throws `InvalidOperationException` — SigV2 leaves custom parameters unsigned, so the `ObjectLeg.Issue` TTL stays inside the SigV4 7-day ceiling.
- `BlobClient.GenerateSasUri`: refuses an AAD-dialed client — `CanGenerateSasUri` probes it, so SAS capability is a deployment fact of the host-dialed container. GCS's signer stands separate from `StorageClient`, so the `ObjectClient.Gcs` row carries both.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Content-key naming binds the object name to the `Element/codec#CONTENT_ADDRESS` `XxHash128` identity, supplied AS the whole-object checksum (`ChecksumType.FULL_OBJECT` + `ChecksumAlgorithm.XXHASH128` on S3, precalculated CRC64 on Azure/GCS) so the store never re-hashes server-side.
- Write-once seal (`IfNoneMatch: *`, Azure `BlobRequestConditions.IfNoneMatch = ETag.All`, GCS `UploadObjectOptions.IfGenerationMatch = 0`) is the concurrency primitive letting `Store/blobstore#MULTIPART_TRANSFER` skip a read-before-write.
- `ObjectEncryption` (SSE-KMS/SSE-C) and `ObjectLock` (WORM `Governance`/`Compliance`) write stances set on the INITIATE/upload request; Azure and GCS bind their per-object retention AFTER the seal, so both seat a follow-up rung rather than a container or bucket dial.
- Read-side validation is per provider and arms at the request — S3 `ChecksumMode.ENABLED`, Azure `DownloadTransferValidationOptions`, GCS `DownloadValidationMode` — and proves TRANSPORT over stored bytes alone. Where a codec or a client seal frames those bytes, the provider digest no longer describes the content key, so identity stays a separate domain-side claim and neither substitutes for the other.
- Storage-class changes are metadata alone: S3 self-copies with `StorageClass` under `S3MetadataDirective.COPY`, Azure rewrites the header through `SetAccessTierAsync`, GCS rewrites through `CopyObjectOptions.ExtraMetadata`. Payload re-PUT to change a class is the rejected form on every row.

[STACKING]:
- `Minio`(`.api/api-minio`): the fourth self-hosted `ObjectClient` provider row on the same `BlobRemote` placement contract, supplying the same four legs, range-read, and the `#WRITE_ONCE_SEAL` edge (`ObjectConditionalQueryArgs.WithMatchETag`/`CopyConditions`).
- within-lib: the `ObjectClient` union dispatches one `Store/blobstore#OBJECT_STORE` placement row across S3/Azure/GCS/Minio; the SSE-KMS KEK reference rides the tenant `Element/identity#KMS_CUSTODY` `EnvelopeKeyring` wrap, and framing is settled at `#ARTIFACT_FRAMES`.

[LOCAL_ADMISSION]:
- Conditional-write conflict — S3 `PreconditionFailed`/412, Azure `ConditionNotMet`/412, GCS 412 on generation-match — folds to `RemoteStoreFault.Conflict`, a benign write-once no-op since the content is already durably present, identical by hash.
- Credential acquisition (AWS credential providers, Azure `TokenCredential`, `GoogleCredential`) is app-root connection input, never a Persistence fence member.

[RAIL_LAW]:
- Package: `AWSSDK.S3`, `Azure.Storage.Blobs`, `Azure.Storage.Blobs.Batch`, `Google.Cloud.Storage.V1`
- Owns: the cloud object-store lane — multipart/staged-block/resumable put, `Stat` head, `List`, `Delete`, page-at-a-time erase, range-read resume and its read-side validation, SSE-KMS/SSE-C, WORM object-lock and per-object retention, metadata-only storage-class transition, archive thaw, presigned-URL issuance.
- Accept: one `BlobRemote` placement row per provider dispatched by the `ObjectClient` union, the content-key as whole-object checksum wherever stored bytes equal plaintext, `IfNoneMatch: *`/`IfGenerationMatch = 0` as the write-once seal, an injected credential handle, a consumer-stated erase page bound.
- Reject: a second `BlobRemote` code path beside the union, a read-before-write guard the seal forecloses, a server-side re-hash the content-key checksum forecloses, a payload re-PUT standing in for a storage-class change, a batch convenience verb where a typed partial-failure tally is needed, credential material as a fence member.
