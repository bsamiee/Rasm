# [RASM_PERSISTENCE_API_OBJECTSTORE]

Cloud object-store SDK surfaces for the `Store/blobstore#OBJECT_STORE` cluster: `AWSSDK.S3` low-level multipart plus `TransferUtility`, `Azure.Storage.Blobs` staged-block plus parallel upload, and `Google.Cloud.Storage.V1` resumable chunked upload with conditional-write concurrency. Each SDK supplies the chunked/resumable transfer members, the content-hash/ETag descriptor evidence, the conditional-write optimistic-concurrency edge, the server-side-encryption (SSE-KMS / SSE-C) request stance, and the WORM object-lock retention members the `ObjectStore` placement rows consume (the `ObjectEncryption` and `ObjectLock` write stances SET on the wire). The self-hosted / S3-compatible FOURTH provider on the same `ObjectClient` union — `Minio` (MinIO, R2, B2, Wasabi, Ceph) — is catalogued at `api-minio`; its `*Args` builder legs map onto the same `BlobRemote` placement contract and the same `#WRITE_ONCE_SEAL` law as these three cloud SDKs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AWSSDK.S3`
- package: `AWSSDK.S3`
- assembly: `AWSSDK.S3`
- companion: `AWSSDK.Core` (transitive; no separate pin)
- namespace: `Amazon.S3`
- transfer namespace: `Amazon.S3.Transfer`
- asset: runtime library
- rail: object-store

[PACKAGE_SURFACE]: `Azure.Storage.Blobs`
- package: `Azure.Storage.Blobs`
- assembly: `Azure.Storage.Blobs`
- namespace: `Azure.Storage.Blobs`, `Azure.Storage.Blobs.Specialized`, `Azure.Storage.Blobs.Models`
- asset: runtime library
- rail: object-store

[PACKAGE_SURFACE]: `Google.Cloud.Storage.V1`
- package: `Google.Cloud.Storage.V1`
- assembly: `Google.Cloud.Storage.V1`
- auth companion: `Google.Apis.Auth` (transitive; `GoogleCredential` factory, not a fence member)
- namespace: `Google.Cloud.Storage.V1`
- asset: runtime library
- rail: object-store

## [02]-[PUBLIC_TYPES]

[S3_TYPES]: AWSSDK.S3 multipart and transfer
- rail: object-store

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]     | [CAPABILITY]                                                            |
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

[AZURE_TYPES]: Azure.Storage.Blobs staged-block and parallel upload
- rail: object-store

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `BlobContainerClient`                           | container      | `GetBlobClient`/`GetBlobs` namespace ops                          |
|  [02]   | `BlockBlobClient : BlobBaseClient`              | client         | staged-block upload                                               |
|  [03]   | `BlobClient : BlobBaseClient`                   | client         | simple/parallel upload                                            |
|  [04]   | `SpecializedBlobExtensions`                     | extension      | `GetBlockBlobClient(this BlobContainerClient, name)`              |
|  [05]   | `BlockBlobStageBlockOptions`                    | options        | per-block conditions/progress                                     |
|  [06]   | `CommitBlockListOptions`                        | options        | headers/metadata/tags/conditions                                  |
|  [07]   | `BlobUploadOptions`                             | options        | carries `TransferOptions`                                         |
|  [08]   | `BlobDownloadOptions`                           | options        | `Range` (`HttpRange`) range-read                                  |
|  [09]   | `StorageTransferOptions`                        | value          | chunk-size and concurrency tuning                                 |
|  [10]   | `HttpRange`                                     | value          | `(offset, length)` range window                                   |
|  [11]   | `BlockListTypes`                                | enum           | `Committed`/`Uncommitted`/`All` filter for resume                 |
|  [12]   | `BlockList`                                     | response       | `CommittedBlocks`/`UncommittedBlocks` for resume skip             |
|  [13]   | `BlockInfo`                                     | response       | block `ContentHash`/`ContentCrc64`                                |
|  [14]   | `BlobContentInfo`                               | response       | object `ETag`/`ContentHash`                                       |
|  [15]   | `BlobProperties`                                | response       | `Metadata`/`ContentLength`/`ETag` for `Stat`                      |
|  [16]   | `BlobItem`                                      | list element   | `Name`/`Properties` for `List`                                    |
|  [17]   | `BlobRequestConditions`                         | value          | `IfMatch`/`IfNoneMatch` ETag gate; `ETag.All` write-once          |
|  [18]   | `BlobDownloadStreamingResult`                   | response       | range-read resumption stream (`.Content`)                         |
|  [19]   | `Azure.Storage.UploadTransferValidationOptions` | value          | `ChecksumAlgorithm` + `PrecalculatedChecksum` (Common)            |
|  [20]   | `Azure.Storage.StorageChecksumAlgorithm`        | enum           | `Auto`/`None`/`MD5`/`StorageCrc64`; `StorageCrc64` = Azure stance |

[GCS_TYPES]: Google.Cloud.Storage.V1 resumable upload
- rail: object-store

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]                                   |
| :-----: | :---------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `StorageClient`         | client (abstract) | upload/download/list/delete object ops         |
|  [02]   | `UploadObjectOptions`   | options           | `ChunkSize` resumable + generation-match gate  |
|  [03]   | `DownloadObjectOptions` | options           | `Range` (`RangeHeaderValue`) range-read        |
|  [04]   | `GetObjectOptions`      | options           | generation/projection on `Stat`                |
|  [05]   | `DeleteObjectOptions`   | options           | generation-match on delete                     |
|  [06]   | `ListObjectsOptions`    | options           | prefix/delimiter list tuning                   |
|  [07]   | `Object`                | value             | destination descriptor + `Generation`/`Crc32c` |
|  [08]   | `Objects`               | list page         | `Items` page over `PagedEnumerable`            |
|  [09]   | `IUploadProgress`       | progress          | per-chunk byte progress                        |
|  [10]   | `EncryptionKey`         | value             | CSEK customer-supplied encryption key          |
|  [11]   | `PredefinedObjectAcl`   | enum              | predefined-acl column                          |

## [03]-[ENTRYPOINTS]

[S3_MULTIPART]: low-level multipart over `AmazonS3Client`
- rail: object-store

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]              | [CAPABILITY]      |
| :-----: | :----------------------------- | :------------------------ | :---------------- |
|  [01]   | `InitiateMultipartUploadAsync` | request plus cancellation | begins upload     |
|  [02]   | `UploadPartAsync`              | request plus cancellation | uploads one part  |
|  [03]   | `CompleteMultipartUploadAsync` | request plus cancellation | seals object      |
|  [04]   | `AbortMultipartUploadAsync`    | request plus cancellation | abandons upload   |
|  [05]   | `GetObjectAsync`               | request plus cancellation | range-read fetch  |
|  [06]   | `TransferUtility.UploadAsync`  | request plus cancellation | managed multipart |

Request members consumed: `InitiateMultipartUploadRequest` — `BucketName`, `Key`, `ContentType`, `StorageClass` (`S3StorageClass`); `UploadPartRequest` — `BucketName`, `Key`, `UploadId`, `PartNumber` (int, 1-10000), `InputStream`, `PartSize` (long); `UploadPartResponse` — `ETag`, `PartNumber`; `CompleteMultipartUploadRequest` — `BucketName`, `Key`, `UploadId`, `PartETags` (`List<PartETag>`); `PartETag` — `PartNumber`, `ETag`; `CompleteMultipartUploadResponse` — `Location`, `BucketName`, `Key`, `ETag`; `GetObjectRequest` — `BucketName`, `Key`, `ByteRange` (`ByteRange(long start, long end)`); `TransferUtilityUploadRequest` — `BucketName`, `Key`, `InputStream`, `PartSize` (long, min 5 MB), `AutoCloseStream`. Content-integrity is the checksum stance: `InitiateMultipartUploadRequest.ChecksumAlgorithm` (`ChecksumAlgorithm`, e.g. `XXHASH128`) + `.ChecksumType` (`ChecksumType.FULL_OBJECT`) DECLARE the whole-object stance, `UploadPartRequest.ChecksumAlgorithm` is per part, and the PRECOMPUTED digest rides `CompleteMultipartUploadRequest.ChecksumXXHASH128` (string, base64 — also `.ChecksumCRC64NVME`/`.ChecksumSHA256`/`.ChecksumCRC32`/`.ChecksumCRC32C`; `UploadPartRequest` carries the same per-algorithm `Checksum*` string members per part) so a content-addressed store supplies the content key AS the whole-object checksum with zero server-side re-hash. SSE rides the INITIATE: `InitiateMultipartUploadRequest.ServerSideEncryptionMethod` (`ServerSideEncryptionMethod`, e.g. `AWSKMS`) + `.ServerSideEncryptionKeyManagementServiceKeyId` (string) for SSE-KMS, `.ServerSideEncryptionCustomerMethod` (`ServerSideEncryptionCustomerMethod.AES256`) + `.ServerSideEncryptionCustomerProvidedKey` (string, base64) + `.ServerSideEncryptionCustomerProvidedKeyMD5` (string) for SSE-C, plus `.ServerSideEncryptionKeyManagementServiceEncryptionContext` (string). WORM object-lock rides the INITIATE: `InitiateMultipartUploadRequest.ObjectLockMode` (`ObjectLockMode.Governance`/`.Compliance`) + `.ObjectLockRetainUntilDate` (`DateTime?`) + `.ObjectLockLegalHoldStatus` (`ObjectLockLegalHoldStatus`) set the retention-until window the `ObjectStore` `ObjectLock` stance projects.

[AZURE_BLOCKS]: staged-block over `BlockBlobClient`
- rail: object-store

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                       | [CAPABILITY]     |
| :-----: | :----------------------- | :--------------------------------- | :--------------- |
|  [01]   | `StageBlockAsync`        | block id, stream, options          | one block        |
|  [02]   | `CommitBlockListAsync`   | block ids plus options             | seals blob       |
|  [03]   | `BlobClient.UploadAsync` | stream plus upload options         | parallel chunked |
|  [04]   | `DownloadStreamingAsync` | download options plus cancellation | range-read fetch |

Type members consumed: `BlockInfo` — `ContentHash` (byte[]), `ContentCrc64` (byte[]); `BlobContentInfo` — `ETag`, `LastModified`, `ContentHash` (byte[]), `VersionId`; `BlobUploadOptions` — `HttpHeaders`, `Metadata`, `Tags`, `AccessTier?`, `TransferOptions` (`StorageTransferOptions`), `Conditions` (`BlobRequestConditions?`); `StorageTransferOptions` — `InitialTransferSize` (long?), `MaximumTransferSize` (long?), `MaximumConcurrency` (int?); `BlobRequestConditions` — `IfMatch` (`ETag?`), `IfNoneMatch` (`ETag?`); `BlockBlobStageBlockOptions` — `Conditions` (`BlobRequestConditions`), `ProgressHandler`, `TransferValidation` (`UploadTransferValidationOptions`); `UploadTransferValidationOptions` — `ChecksumAlgorithm` (`StorageChecksumAlgorithm.StorageCrc64`), `PrecalculatedChecksum` (`ReadOnlyMemory<byte>`).

[GCS_RESUMABLE]: resumable chunked over `StorageClient`
- rail: object-store

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                    | [CAPABILITY]                                               |
| :-----: | :----------------------------- | :------------------------------ | :--------------------------------------------------------- |
|  [01]   | `UploadObjectAsync`            | object, stream, options         | resumable upload                                           |
|  [02]   | `DownloadObjectAsync`          | bucket, object, stream, options | range-read fetch                                           |
|  [03]   | `StorageClient.CreateAsync(…)` | optional credential + CSEK      | `Create`/`CreateAsync` client factory; app-root credential |

`StorageClient.CreateAsync(GoogleCredential?, EncryptionKey?)` is the `Create`/`CreateAsync` client factory (app-root credential plus optional CSEK). Type members consumed: `UploadObjectOptions` — `ChunkSize` (int?, positive multiple of 262144 enables resumable chunked; null = single-request), `IfGenerationMatch` (long?), `IfGenerationNotMatch` (long?), `IfMetagenerationMatch` (long?), `PredefinedAcl` (`PredefinedObjectAcl?`), `KmsKeyName` (string?); `Object` — `Name`, `Bucket`, `Generation` (long?), `Crc32c` (string), `Md5Hash` (string), `Size` (ulong?), `ContentType`; `DownloadObjectOptions` — `Range` (`System.Net.Http.Headers.RangeHeaderValue?`).

[OBJECT_CRUD]: head / list / delete / multipart-resume across the four providers
- rail: object-store

The `BlobRemote.Stat`/`Delete`/`List` legs and the multipart resume skip are the second half of the placement contract the multipart-upload rows above do not cover. One unified entry per leg dispatches on the `ObjectClient` union — a four-row set: `S3` (`AmazonS3Client`), `Azure` (`BlobContainerClient`/`BlockBlobClient`), `GCS` (`StorageClient`), and `Minio` (`IMinioClient`, the self-hosted / S3-compatible row catalogued at `api-minio`). The Minio column maps each leg onto the `*Args` builder surface; its members are owned by `api-minio` and reproduced here only as the union-dispatch contract.

The grid carries the per-provider member name; each leg's full call and return signatures are keyed below.

| [INDEX] | [LEG]        | [S3]                     | [AZURE]                  | [GCS]                 | [MINIO]                          |
| :-----: | :----------- | :----------------------- | :----------------------- | :-------------------- | :------------------------------- |
|  [01]   | `Stat`       | `GetObjectMetadataAsync` | `GetPropertiesAsync`     | `GetObjectAsync`      | `StatObjectAsync`                |
|  [02]   | `List`       | `ListObjectsV2Async`     | `GetBlobs`               | `ListObjects`         | `ListObjectsEnumAsync`           |
|  [03]   | `Delete`     | `DeleteObjectAsync`      | `DeleteIfExistsAsync`    | `DeleteObjectAsync`   | `RemoveObjectAsync`              |
|  [04]   | `Get`        | `GetObjectAsync`         | `DownloadStreamingAsync` | `DownloadObjectAsync` | `GetObjectAsync`                 |
|  [05]   | resume       | `ListPartsAsync`         | `GetBlockListAsync`      | server-side           | `ListIncompleteUploadsEnumAsync` |
|  [06]   | block client | n/a                      | `GetBlockBlobClient`     | n/a                   | n/a                              |

- [01]: `Stat`: S3 `GetObjectMetadataAsync(bucket, key)` -> `Metadata`/`ETag`/`ContentLength`; Azure `BlobBaseClient.GetPropertiesAsync()` -> `BlobProperties`; GCS `GetObjectAsync(bucket, name)` -> `Object`; Minio `StatObjectAsync(StatObjectArgs)` -> `ObjectStat` (`ETag`/`MetaData`/`Size`).
- [02]: `List`: S3 `ListObjectsV2Async(ListObjectsV2Request)` -> `S3Objects`; Azure `BlobContainerClient.GetBlobs()` -> `Pageable<BlobItem>`; GCS `StorageClient.ListObjects(bucket)` -> `PagedEnumerable<Objects, Object>`; Minio `ListObjectsEnumAsync(ListObjectsArgs)` -> `IAsyncEnumerable<Item>`.
- [03]: `Delete`: S3 `DeleteObjectAsync(bucket, key)`; Azure `BlobClient.DeleteIfExistsAsync()`; GCS `DeleteObjectAsync(bucket, name)`; Minio `RemoveObjectAsync(RemoveObjectArgs)`.
- [04]: `Get`: S3 `GetObjectAsync(GetObjectRequest{ByteRange})` -> `ResponseStream`; Azure `BlobBaseClient.DownloadStreamingAsync(BlobDownloadOptions{Range=HttpRange})` -> `.Content`; GCS `DownloadObjectAsync(bucket, name, sink, DownloadObjectOptions{Range=RangeHeaderValue})`; Minio `GetObjectAsync(GetObjectArgs.WithOffsetAndLength)` -> `ObjectStat` (range to stream/file/callback).
- [05]: resume: S3 `ListPartsAsync(ListPartsRequest{UploadId})` -> `Parts` (`PartDetail.PartNumber`/`ETag`); Azure `BlockBlobClient.GetBlockListAsync(BlockListTypes.Uncommitted)` -> `UncommittedBlocks`; GCS resumable session is provider-internal, chunked `UploadObjectAsync` resumes server-side; Minio `ListIncompleteUploadsEnumAsync(args)` -> `IAsyncEnumerable<Upload>` / `RemoveIncompleteUploadAsync`.
- [06]: block client: S3 n/a (multipart on `AmazonS3Client`); Azure `SpecializedBlobExtensions.GetBlockBlobClient(container, name)` (extension); GCS n/a (object-level on `StorageClient`); Minio n/a (multipart auto-managed inside `PutObjectAsync`).

The resume row is the resumable-upload edge `MultipartTransfer` skips already-committed windows on: S3 reads prior `PartETag`s by `UploadId`, Azure reads prior uncommitted block ids, GCS resumes its session server-side, and Minio enumerates dangling multipart uploads through `ListIncompleteUploadsEnumAsync`. `GetBlockBlobClient` is an extension on `BlobContainerClient` from `SpecializedBlobExtensions`, not an instance member.

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: SDK exception surfaces lifted at the object-store edge
- rail: object-store

| [INDEX] | [SDK]                     | [THROWN]                 | [DISCRIMINANT]                              |
| :-----: | :------------------------ | :----------------------- | :------------------------------------------ |
|  [01]   | `AWSSDK.S3`               | `AmazonS3Exception`      | `StatusCode` (HttpStatusCode) + `ErrorCode` |
|  [02]   | `Azure.Storage.Blobs`     | `RequestFailedException` | `Status` (int) + `ErrorCode` (string)       |
|  [03]   | `Google.Cloud.Storage.V1` | `GoogleApiException`     | `HttpStatusCode` + `Error.Code`             |

Conditional-write conflict surfaces: S3 `PreconditionFailed`/`412`, Azure `ConditionNotMet`/`412`, GCS `412` on generation-match, Minio `PreconditionFailedException`/`412` (`api-minio`) — each routes to the `RemoteStoreFault.Conflict` case.

[WRITE_ONCE_SEAL]: the content-address write-once gate is the optimistic-concurrency edge each provider exposes through a distinct member, all collapsed into the one `ConditionalWrite` row column:
- S3 / Azure: `CompleteMultipartUploadRequest`/`CommitBlockListAsync` carry `IfNoneMatch: *` (Azure `BlobRequestConditions.IfNoneMatch = ETag.All`) so a second writer racing the same content-key `412`s.
- GCS: `UploadObjectOptions.IfGenerationMatch = 0` is the create-if-absent precondition; a racing overwrite of an existing generation `412`s.
- Minio (`api-minio`): the inherited `ObjectConditionalQueryArgs<T>.WithMatchETag`/`WithNotMatchETag` and `CopyConditions.SetMatchETag`/`SetMatchETagNone` are the same `If-None-Match: *` edge; a racing second writer to the same content-key `412`s (`PreconditionFailedException`).
- This is why the content-address store needs no read-before-write — the seal itself is the concurrency primitive, and a `412` on any of the four providers is a benign no-op (the content is already durably present, identical by hash), folded to `RemoteStoreFault.Conflict` and treated as success by the write-once placement.

## [05]-[STACKING_AND_LAW]

[STACKING]:
- placement rows: the three cloud SDKs are provider rows on the one `Store/blobstore#OBJECT_STORE` `BlobRemote` placement contract — each supplies the four legs (multipart/staged-block/resumable put, `Stat` head, `List`, `Delete`) plus range-read the `ObjectClient` union dispatches, so a content-keyed blob lands on S3/Azure/GCS by one placement row, never a parallel client surface, with `Minio` (`api-minio`) the fourth self-hosted row.
- content-key object name: the object name IS the `Element/codec#CONTENT_ADDRESS` `XxHash128` identity the kernel mints, supplied AS the whole-object checksum (`ChecksumAlgorithm.XXHASH128` + `ChecksumType.FULL_OBJECT` on S3, precalculated CRC64 on Azure/GCS) so the store needs no server-side re-hash — the settled `#ARTIFACT_FRAMES` frame law owns framing.
- write-once seal: the `#WRITE_ONCE_SEAL` optimistic-concurrency edge (`IfNoneMatch: *`, GCS `IfGenerationMatch = 0`) is the concurrency primitive that lets `Store/blobstore#MULTIPART_TRANSFER` skip a read-before-write — a racing second writer `412`s, folded to `RemoteStoreFault.Conflict` and treated as success since the content is identical by hash.
- SSE + WORM at the tier: the `Store/blobstore#OBJECT_STORE` `ObjectEncryption` (SSE-KMS/SSE-C) and `ObjectLock` (WORM `Governance`/`Compliance`) write stances SET on the INITIATE/upload request, the SSE-KMS KEK reference riding the tenant `Element/identity#KEY_ENVELOPE` DEK envelope, never a fence-member credential.

[PACKAGE_SCOPE]:
- Package pages carry external package API facts; the `ObjectStore` placement rows and the multipart transfer algebra are owned at `Store/blobstore#OBJECT_STORE`.
- The frame-law constants (64-KiB frame, Crc32-per-frame, XxHash128 whole-artifact identity) are owned at `#ARTIFACT_FRAMES` and consumed as settled vocabulary.
- Auth credential acquisition (`GoogleCredential`, AWS credential providers, Azure `TokenCredential`, Minio `IClientProvider`/access-secret keys) is connection input handed over by app roots, not a Persistence fence member.
- The `ObjectClient` union is a four-provider set: the three cloud SDKs catalogued here (`AWSSDK.S3`, `Azure.Storage.Blobs`, `Google.Cloud.Storage.V1`) plus the self-hosted / S3-compatible row `Minio` catalogued at `api-minio`. `api-minio` owns the Minio `*Args`/`IMinioClient` member facts and cross-references `#WRITE_ONCE_SEAL`; this page enumerates Minio only as the fourth union-dispatch row, so the two object-store catalogs cross-reference symmetrically.
