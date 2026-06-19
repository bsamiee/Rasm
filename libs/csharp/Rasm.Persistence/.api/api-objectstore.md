# [RASM_PERSISTENCE_API_OBJECTSTORE]

Cloud object-store SDK surfaces for the `remote-stores` cluster: `AWSSDK.S3` low-level multipart plus `TransferUtility`, `Azure.Storage.Blobs` staged-block plus parallel upload, and `Google.Cloud.Storage.V1` resumable chunked upload with conditional-write concurrency. Each SDK supplies the chunked/resumable transfer members, the content-hash/ETag descriptor evidence, and the conditional-write optimistic-concurrency edge the `ObjectStore` placement rows consume.

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

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [CAPABILITY]                         |
| :-----: | :-------------------------------- | :----------------- | :----------------------------------- |
|  [01]   | `AmazonS3Client : IAmazonS3`      | client             | low-level multipart and object ops   |
|  [02]   | `InitiateMultipartUploadRequest`  | request            | begins multipart, carries key/bucket |
|  [03]   | `InitiateMultipartUploadResponse` | response           | yields `UploadId`                    |
|  [04]   | `UploadPartRequest`               | request            | one part stream + `PartNumber`       |
|  [05]   | `UploadPartResponse`              | response           | yields part `ETag`                   |
|  [06]   | `CompleteMultipartUploadRequest`  | request            | carries `PartETags` list             |
|  [07]   | `CompleteMultipartUploadResponse` | response           | yields object `ETag`/`Location`      |
|  [08]   | `AbortMultipartUploadRequest`     | request            | abandons an in-flight upload         |
|  [09]   | `PartETag`                        | value              | `(PartNumber, ETag)` pair            |
|  [10]   | `TransferUtility`                 | high-level surface | managed multipart upload             |
|  [11]   | `TransferUtilityUploadRequest`    | request            | `PartSize`/stream high-level config  |
|  [12]   | `GetObjectRequest`                | request            | range-read resumption (`ByteRange`)  |
|  [13]   | `S3StorageClass`                  | enum               | storage-class column                 |

[AZURE_TYPES]: Azure.Storage.Blobs staged-block and parallel upload
- rail: object-store

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]                       |
| :-----: | :--------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `BlockBlobClient : BlobBaseClient` | client         | staged-block upload                |
|  [02]   | `BlobClient : BlobBaseClient`      | client         | simple/parallel upload             |
|  [03]   | `BlockBlobStageBlockOptions`       | options        | per-block conditions/progress      |
|  [04]   | `CommitBlockListOptions`           | options        | headers/metadata/tags/conditions   |
|  [05]   | `BlobUploadOptions`                | options        | carries `TransferOptions`          |
|  [06]   | `StorageTransferOptions`           | value          | chunk-size and concurrency tuning  |
|  [07]   | `BlockInfo`                        | response       | block `ContentHash`/`ContentCrc64` |
|  [08]   | `BlobContentInfo`                  | response       | object `ETag`/`ContentHash`        |
|  [09]   | `BlobRequestConditions`            | value          | `IfMatch`/`IfNoneMatch` ETag gate  |
|  [10]   | `BlobDownloadStreamingResult`      | response       | range-read resumption stream       |

[GCS_TYPES]: Google.Cloud.Storage.V1 resumable upload
- rail: object-store

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]                                   |
| :-----: | :---------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `StorageClient`         | client (abstract) | upload/download/list object ops                |
|  [02]   | `UploadObjectOptions`   | options           | `ChunkSize` resumable + generation-match gate  |
|  [03]   | `DownloadObjectOptions` | options           | range-read resumption                          |
|  [04]   | `Object`                | value             | destination descriptor + `Generation`/`Crc32c` |
|  [05]   | `IUploadProgress`       | progress          | per-chunk byte progress                        |
|  [06]   | `PredefinedObjectAcl`   | enum              | predefined-acl column                          |

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

Request members consumed: `InitiateMultipartUploadRequest` — `BucketName`, `Key`, `ContentType`, `StorageClass` (`S3StorageClass`); `UploadPartRequest` — `BucketName`, `Key`, `UploadId`, `PartNumber` (int, 1-10000), `InputStream`, `PartSize` (long); `UploadPartResponse` — `ETag`, `PartNumber`; `CompleteMultipartUploadRequest` — `BucketName`, `Key`, `UploadId`, `PartETags` (`List<PartETag>`); `PartETag` — `PartNumber`, `ETag`; `CompleteMultipartUploadResponse` — `Location`, `BucketName`, `Key`, `ETag`; `GetObjectRequest` — `BucketName`, `Key`, `ByteRange` (`ByteRange(long start, long end)`); `TransferUtilityUploadRequest` — `BucketName`, `Key`, `InputStream`, `PartSize` (long, min 5 MB), `AutoCloseStream`.

[AZURE_BLOCKS]: staged-block over `BlockBlobClient`
- rail: object-store

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                       | [CAPABILITY]     |
| :-----: | :----------------------- | :--------------------------------- | :--------------- |
|  [01]   | `StageBlockAsync`        | block id, stream, options          | one block        |
|  [02]   | `CommitBlockListAsync`   | block ids plus options             | seals blob       |
|  [03]   | `BlobClient.UploadAsync` | stream plus upload options         | parallel chunked |
|  [04]   | `DownloadStreamingAsync` | download options plus cancellation | range-read fetch |

Type members consumed: `BlockInfo` — `ContentHash` (byte[]), `ContentCrc64` (byte[]); `BlobContentInfo` — `ETag`, `LastModified`, `ContentHash` (byte[]), `VersionId`; `BlobUploadOptions` — `HttpHeaders`, `Metadata`, `Tags`, `AccessTier?`, `TransferOptions` (`StorageTransferOptions`), `Conditions` (`BlobRequestConditions?`); `StorageTransferOptions` — `InitialTransferSize` (long?), `MaximumTransferSize` (long?), `MaximumConcurrency` (int?); `BlobRequestConditions` — `IfMatch` (`ETag?`), `IfNoneMatch` (`ETag?`).

[GCS_RESUMABLE]: resumable chunked over `StorageClient`
- rail: object-store

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                    | [CAPABILITY]     |
| :-----: | :-------------------------- | :------------------------------ | :--------------- |
|  [01]   | `UploadObjectAsync`         | object, stream, options         | resumable upload |
|  [02]   | `DownloadObjectAsync`       | bucket, object, stream, options | range-read fetch |
|  [03]   | `StorageClient.CreateAsync` | credential option               | client factory   |

Type members consumed: `UploadObjectOptions` — `ChunkSize` (int?, positive multiple of 262144 enables resumable chunked; null = single-request), `IfGenerationMatch` (long?), `IfGenerationNotMatch` (long?), `IfMetagenerationMatch` (long?), `PredefinedAcl` (`PredefinedObjectAcl?`), `KmsKeyName` (string?); `Object` — `Name`, `Bucket`, `Generation` (long?), `Crc32c` (string), `Md5Hash` (string), `Size` (ulong?), `ContentType`; `DownloadObjectOptions` — `Range` (`System.Net.Http.Headers.RangeHeaderValue?`).

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: SDK exception surfaces lifted at the object-store edge
- rail: object-store

| [INDEX] | [SDK]                     | [THROWN]                 | [DISCRIMINANT]                              |
| :-----: | :------------------------ | :----------------------- | :------------------------------------------ |
|  [01]   | `AWSSDK.S3`               | `AmazonS3Exception`      | `StatusCode` (HttpStatusCode) + `ErrorCode` |
|  [02]   | `Azure.Storage.Blobs`     | `RequestFailedException` | `Status` (int) + `ErrorCode` (string)       |
|  [03]   | `Google.Cloud.Storage.V1` | `GoogleApiException`     | `HttpStatusCode` + `Error.Code`             |

Conditional-write conflict surfaces: S3 `PreconditionFailed`/`412`, Azure `ConditionNotMet`/`412`, GCS `412` on generation-match — each routes to the `RemoteStoreFault.Conflict` case.

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- Package pages carry external package API facts; the `ObjectStore` placement rows and the multipart transfer algebra are owned at `remote-stores`.
- The frame-law constants (64-KiB frame, Crc32-per-frame, XxHash128 whole-artifact identity) are owned at `#ARTIFACT_FRAMES` and consumed as settled vocabulary.
- Auth credential acquisition (`GoogleCredential`, AWS credential providers, Azure `TokenCredential`) is connection input handed over by app roots, not a Persistence fence member.
