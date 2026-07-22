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

[AZURE_TYPES]: Azure.Storage.Blobs staged-block and parallel upload

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `BlobContainerClient`                           | container     | `GetBlobClient`/`GetBlobs` namespace ops                          |
|  [02]   | `BlockBlobClient : BlobBaseClient`              | client        | staged-block upload                                               |
|  [03]   | `BlobClient : BlobBaseClient`                   | client        | simple/parallel upload                                            |
|  [04]   | `SpecializedBlobExtensions`                     | extension     | `GetBlockBlobClient(this BlobContainerClient, name)`              |
|  [05]   | `BlockBlobStageBlockOptions`                    | options       | per-block conditions/progress                                     |
|  [06]   | `CommitBlockListOptions`                        | options       | headers/metadata/tags/conditions                                  |
|  [07]   | `BlobUploadOptions`                             | options       | carries `TransferOptions`                                         |
|  [08]   | `BlobDownloadOptions`                           | options       | `Range` (`HttpRange`) range-read                                  |
|  [09]   | `StorageTransferOptions`                        | value         | chunk-size and concurrency tuning                                 |
|  [10]   | `HttpRange`                                     | value         | `(offset, length)` range window                                   |
|  [11]   | `BlockListTypes`                                | enum          | `Committed`/`Uncommitted`/`All` filter for resume                 |
|  [12]   | `BlockList`                                     | response      | `CommittedBlocks`/`UncommittedBlocks` for resume skip             |
|  [13]   | `BlockInfo`                                     | response      | block `ContentHash`/`ContentCrc64`                                |
|  [14]   | `BlobContentInfo`                               | response      | object `ETag`/`ContentHash`                                       |
|  [15]   | `BlobProperties`                                | response      | `Metadata`/`ContentLength`/`ETag` for `Stat`                      |
|  [16]   | `BlobItem`                                      | list element  | `Name`/`Properties` for `List`                                    |
|  [17]   | `BlobRequestConditions`                         | value         | `IfMatch`/`IfNoneMatch` ETag gate; `ETag.All` write-once          |
|  [18]   | `BlobDownloadStreamingResult`                   | response      | range-read resumption stream (`.Content`)                         |
|  [19]   | `Azure.Storage.UploadTransferValidationOptions` | value         | `ChecksumAlgorithm` + `PrecalculatedChecksum` (Common)            |
|  [20]   | `Azure.Storage.StorageChecksumAlgorithm`        | enum          | `Auto`/`None`/`MD5`/`StorageCrc64`; `StorageCrc64` = Azure stance |

[GCS_TYPES]: Google.Cloud.Storage.V1 resumable upload

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                   |
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

[AZURE_BLOCKS]: staged-block over `BlockBlobClient`

| [INDEX] | [SURFACE]                | [SHAPE]                            | [CAPABILITY]     |
| :-----: | :----------------------- | :--------------------------------- | :--------------- |
|  [01]   | `StageBlockAsync`        | block id, stream, options          | one block        |
|  [02]   | `CommitBlockListAsync`   | block ids plus options             | seals blob       |
|  [03]   | `BlobClient.UploadAsync` | stream plus upload options         | parallel chunked |
|  [04]   | `DownloadStreamingAsync` | download options plus cancellation | range-read fetch |

[GCS_RESUMABLE]: resumable chunked over `StorageClient`

| [INDEX] | [SURFACE]                      | [SHAPE]                               | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------------------------------ | :----------------------------------------- |
|  [01]   | `UploadObjectAsync`            | object, stream, options               | resumable upload                           |
|  [02]   | `DownloadObjectAsync`          | bucket, object, stream, options       | range-read fetch                           |
|  [03]   | `StorageClient.CreateAsync(…)` | `GoogleCredential?`, `EncryptionKey?` | client factory; app-root credential + CSEK |

- `UploadObjectOptions.ChunkSize`: a positive multiple of 262144 selects resumable chunked upload; null uploads single-request. Generation-match gates (`IfGenerationMatch`) ride the same options.

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

- `BlobClient.GenerateSasUri`: an AAD-dialed client cannot sign — `CanGenerateSasUri` is the probe, so SAS capability is a deployment fact of the host-dialed container. GCS's signer stands separate from `StorageClient`, so the `ObjectClient.Gcs` row carries both.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Content-key naming binds the object name to the `Element/codec#CONTENT_ADDRESS` `XxHash128` identity, supplied AS the whole-object checksum (`ChecksumType.FULL_OBJECT` + `ChecksumAlgorithm.XXHASH128` on S3, precalculated CRC64 on Azure/GCS) so the store never re-hashes server-side.
- Write-once seal (`IfNoneMatch: *`, Azure `BlobRequestConditions.IfNoneMatch = ETag.All`, GCS `UploadObjectOptions.IfGenerationMatch = 0`) is the concurrency primitive letting `Store/blobstore#MULTIPART_TRANSFER` skip a read-before-write.
- `ObjectEncryption` (SSE-KMS/SSE-C) and `ObjectLock` (WORM `Governance`/`Compliance`) write stances set on the INITIATE/upload request.

[STACKING]:
- `Minio`(`.api/api-minio`): the fourth self-hosted `ObjectClient` provider row on the same `BlobRemote` placement contract, supplying the same four legs, range-read, and the `#WRITE_ONCE_SEAL` edge (`ObjectConditionalQueryArgs.WithMatchETag`/`CopyConditions`).
- within-lib: the `ObjectClient` union dispatches one `Store/blobstore#OBJECT_STORE` placement row across S3/Azure/GCS/Minio; the SSE-KMS KEK reference rides the tenant `Element/identity#KEY_ENVELOPE` DEK envelope, and framing is settled at `#ARTIFACT_FRAMES`.

[LOCAL_ADMISSION]:
- Conditional-write conflict — S3 `PreconditionFailed`/412, Azure `ConditionNotMet`/412, GCS 412 on generation-match — folds to `RemoteStoreFault.Conflict`, a benign write-once no-op since the content is already durably present, identical by hash.
- Credential acquisition (AWS credential providers, Azure `TokenCredential`, `GoogleCredential`) is app-root connection input, never a Persistence fence member.

[RAIL_LAW]:
- Package: `AWSSDK.S3`, `Azure.Storage.Blobs`, `Google.Cloud.Storage.V1`
- Owns: the cloud object-store lane — multipart/staged-block/resumable put, `Stat` head, `List`, `Delete`, range-read resume, SSE-KMS/SSE-C, WORM object-lock, presigned-URL issuance.
- Accept: one `BlobRemote` placement row per provider dispatched by the `ObjectClient` union, the content-key as whole-object checksum, `IfNoneMatch: *`/`IfGenerationMatch = 0` as the write-once seal, an injected credential handle.
- Reject: a second `BlobRemote` code path beside the union, a read-before-write guard the seal forecloses, a server-side re-hash the content-key checksum forecloses, credential material as a fence member.
