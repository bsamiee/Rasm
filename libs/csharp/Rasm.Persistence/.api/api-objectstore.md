# [RASM_PERSISTENCE_API_OBJECTSTORE]

Cloud object-store SDK surfaces for the `remote-stores` cluster: `AWSSDK.S3` low-level multipart plus `TransferUtility`, `Azure.Storage.Blobs` staged-block plus parallel upload, and `Google.Cloud.Storage.V1` resumable chunked upload with conditional-write concurrency. Each SDK supplies the chunked/resumable transfer members, the content-hash/ETag descriptor evidence, and the conditional-write optimistic-concurrency edge the `ObjectStore` placement rows consume.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[S3_TYPES]: AWSSDK.S3 multipart and transfer
- rail: object-store

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------ |
|   [1]   | `AmazonS3Client : IAmazonS3`      | client              | low-level multipart and object ops    |
|   [2]   | `InitiateMultipartUploadRequest`  | request             | begins multipart, carries key/bucket  |
|   [3]   | `InitiateMultipartUploadResponse` | response            | yields `UploadId`                     |
|   [4]   | `UploadPartRequest`               | request             | one part stream + `PartNumber`        |
|   [5]   | `UploadPartResponse`              | response            | yields part `ETag`                    |
|   [6]   | `CompleteMultipartUploadRequest`  | request             | carries `PartETags` list              |
|   [7]   | `CompleteMultipartUploadResponse` | response            | yields object `ETag`/`Location`       |
|   [8]   | `AbortMultipartUploadRequest`     | request             | abandons an in-flight upload          |
|   [9]   | `PartETag`                        | value               | `(PartNumber, ETag)` pair             |
|  [10]   | `TransferUtility`                 | high-level surface  | managed multipart upload              |
|  [11]   | `TransferUtilityUploadRequest`    | request             | `PartSize`/stream high-level config   |
|  [12]   | `GetObjectRequest`                | request             | range-read resumption (`ByteRange`)   |
|  [13]   | `S3StorageClass`                  | enum                | storage-class column                  |

[AZURE_TYPES]: Azure.Storage.Blobs staged-block and parallel upload
- rail: object-store

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]     | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :----------------- | :------------------------------------ |
|   [1]   | `BlockBlobClient : BlobBaseClient`    | client             | staged-block upload                   |
|   [2]   | `BlobClient : BlobBaseClient`         | client             | simple/parallel upload                |
|   [3]   | `BlockBlobStageBlockOptions`          | options            | per-block conditions/progress         |
|   [4]   | `CommitBlockListOptions`              | options            | headers/metadata/tags/conditions      |
|   [5]   | `BlobUploadOptions`                   | options            | carries `TransferOptions`             |
|   [6]   | `StorageTransferOptions`              | value              | chunk-size and concurrency tuning     |
|   [7]   | `BlockInfo`                           | response           | block `ContentHash`/`ContentCrc64`    |
|   [8]   | `BlobContentInfo`                     | response           | object `ETag`/`ContentHash`           |
|   [9]   | `BlobRequestConditions`               | value              | `IfMatch`/`IfNoneMatch` ETag gate     |
|  [10]   | `BlobDownloadStreamingResult`         | response           | range-read resumption stream          |

[GCS_TYPES]: Google.Cloud.Storage.V1 resumable upload
- rail: object-store

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]     | [CAPABILITY]                                  |
| :-----: | :------------------------ | :----------------- | :-------------------------------------------- |
|   [1]   | `StorageClient`           | client (abstract)  | upload/download/list object ops               |
|   [2]   | `UploadObjectOptions`     | options            | `ChunkSize` resumable + generation-match gate |
|   [3]   | `DownloadObjectOptions`   | options            | range-read resumption                         |
|   [4]   | `Object`                  | value              | destination descriptor + `Generation`/`Crc32c`|
|   [5]   | `IUploadProgress`         | progress           | per-chunk byte progress                       |
|   [6]   | `PredefinedObjectAcl`     | enum               | predefined-acl column                         |

## [3]-[ENTRYPOINTS]

[S3_MULTIPART]: low-level multipart over `AmazonS3Client`
- rail: object-store

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                                  | [CAPABILITY]            |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------------------- | :---------------------- |
|   [1]   | `InitiateMultipartUploadAsync`  | `Task<InitiateMultipartUploadResponse>(InitiateMultipartUploadRequest, CancellationToken = default)`         | begins upload           |
|   [2]   | `UploadPartAsync`               | `Task<UploadPartResponse>(UploadPartRequest, CancellationToken = default)`                                   | one part                |
|   [3]   | `CompleteMultipartUploadAsync`  | `Task<CompleteMultipartUploadResponse>(CompleteMultipartUploadRequest, CancellationToken = default)`         | seals object            |
|   [4]   | `AbortMultipartUploadAsync`     | `Task<AbortMultipartUploadResponse>(AbortMultipartUploadRequest, CancellationToken = default)`               | abandons upload         |
|   [5]   | `GetObjectAsync`                | `Task<GetObjectResponse>(GetObjectRequest, CancellationToken = default)`                                     | range-read fetch        |
|   [6]   | `TransferUtility.UploadAsync`   | `Task(TransferUtilityUploadRequest, CancellationToken = default)`                                            | managed multipart       |

Request members consumed: `InitiateMultipartUploadRequest` — `BucketName`, `Key`, `ContentType`, `StorageClass` (`S3StorageClass`); `UploadPartRequest` — `BucketName`, `Key`, `UploadId`, `PartNumber` (int, 1-10000), `InputStream`, `PartSize` (long); `UploadPartResponse` — `ETag`, `PartNumber`; `CompleteMultipartUploadRequest` — `BucketName`, `Key`, `UploadId`, `PartETags` (`List<PartETag>`); `PartETag` — `PartNumber`, `ETag`; `CompleteMultipartUploadResponse` — `Location`, `BucketName`, `Key`, `ETag`; `GetObjectRequest` — `BucketName`, `Key`, `ByteRange` (`ByteRange(long start, long end)`); `TransferUtilityUploadRequest` — `BucketName`, `Key`, `InputStream`, `PartSize` (long, min 5 MB), `AutoCloseStream`.

[AZURE_BLOCKS]: staged-block over `BlockBlobClient`
- rail: object-store

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                                                            | [CAPABILITY]      |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------------------------- | :---------------- |
|   [1]   | `StageBlockAsync`        | `Task<Response<BlockInfo>>(string base64BlockId, Stream content, BlockBlobStageBlockOptions? = null, CancellationToken = default)` | one block         |
|   [2]   | `CommitBlockListAsync`   | `Task<Response<BlobContentInfo>>(IEnumerable<string> base64BlockIds, CommitBlockListOptions? = null, CancellationToken = default)` | seals blob        |
|   [3]   | `BlobClient.UploadAsync` | `Task<Response<BlobContentInfo>>(Stream content, BlobUploadOptions options, CancellationToken = default)`             | parallel chunked  |
|   [4]   | `DownloadStreamingAsync` | `Task<Response<BlobDownloadStreamingResult>>(BlobDownloadOptions? = null, CancellationToken = default)`              | range-read fetch  |

Type members consumed: `BlockInfo` — `ContentHash` (byte[]), `ContentCrc64` (byte[]); `BlobContentInfo` — `ETag`, `LastModified`, `ContentHash` (byte[]), `VersionId`; `BlobUploadOptions` — `HttpHeaders`, `Metadata`, `Tags`, `AccessTier?`, `TransferOptions` (`StorageTransferOptions`), `Conditions` (`BlobRequestConditions?`); `StorageTransferOptions` — `InitialTransferSize` (long?), `MaximumTransferSize` (long?), `MaximumConcurrency` (int?); `BlobRequestConditions` — `IfMatch` (`ETag?`), `IfNoneMatch` (`ETag?`).

[GCS_RESUMABLE]: resumable chunked over `StorageClient`
- rail: object-store

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                              | [CAPABILITY]        |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------- | :------------------ |
|   [1]   | `UploadObjectAsync`        | `Task<Object>(Object destination, Stream source, UploadObjectOptions? = null, CancellationToken = default, IProgress<IUploadProgress>? = null)` | resumable upload    |
|   [2]   | `DownloadObjectAsync`      | `Task(string bucket, string objectName, Stream destination, DownloadObjectOptions? = null, CancellationToken = default, IProgress<IDownloadProgress>? = null)` | range-read fetch    |
|   [3]   | `StorageClient.CreateAsync`| `Task<StorageClient>(GoogleCredential? credential = null)`                                                                              | client factory      |

Type members consumed: `UploadObjectOptions` — `ChunkSize` (int?, positive multiple of 262144 enables resumable chunked; null = single-request), `IfGenerationMatch` (long?), `IfGenerationNotMatch` (long?), `IfMetagenerationMatch` (long?), `PredefinedAcl` (`PredefinedObjectAcl?`), `KmsKeyName` (string?); `Object` — `Name`, `Bucket`, `Generation` (long?), `Crc32c` (string), `Md5Hash` (string), `Size` (ulong?), `ContentType`; `DownloadObjectOptions` — `Range` (`System.Net.Http.Headers.RangeHeaderValue?`).

## [4]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: SDK exception surfaces lifted at the object-store edge
- rail: object-store

| [INDEX] | [SDK]                     | [THROWN]                            | [DISCRIMINANT]                                  |
| :-----: | :------------------------ | :---------------------------------- | :--------------------------------------------- |
|   [1]   | `AWSSDK.S3`               | `AmazonS3Exception`                 | `StatusCode` (HttpStatusCode) + `ErrorCode`    |
|   [2]   | `Azure.Storage.Blobs`     | `RequestFailedException`            | `Status` (int) + `ErrorCode` (string)          |
|   [3]   | `Google.Cloud.Storage.V1` | `GoogleApiException`                | `HttpStatusCode` + `Error.Code`                |

Conditional-write conflict surfaces: S3 `PreconditionFailed`/`412`, Azure `ConditionNotMet`/`412`, GCS `412` on generation-match — each routes to the `RemoteStoreFault.Conflict` case.

## [5]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- Package pages carry external package API facts; the `ObjectStore` placement rows and the multipart transfer algebra are owned at `remote-stores`.
- The frame-law constants (64-KiB frame, Crc32-per-frame, XxHash128 whole-artifact identity) are owned at `Compute/remote-lane#ARTIFACT_FRAMES` and consumed as settled vocabulary.
- Auth credential acquisition (`GoogleCredential`, AWS credential providers, Azure `TokenCredential`) is connection input handed over by app roots, not a Persistence fence member.
