# [API_CATALOGUE] @effect-aws/client-s3

`@effect-aws/client-s3` wraps `@aws-sdk/client-s3` as an Effect `Tag`-backed service, mapping every S3 command to an `Effect.Effect` with typed domain errors, a `Stream`-based pagination variant for pageable operations, and built-in presigned-URL support via the `presigned: true` option overload. Consuming owners import `S3Service` (or the `S3` namespace alias) for domain operations and `S3ClientInstance` for raw-client access when interop requires it.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-aws/client-s3`
- package: `@effect-aws/client-s3`
- module: `@effect-aws/client-s3`, `@effect-aws/client-s3/Errors`, `@effect-aws/client-s3/S3ClientInstance`, `@effect-aws/client-s3/S3Service`, `@effect-aws/client-s3/S3ServiceConfig`
- asset: Effect S3 service tag, typed error rail, presigned-URL overload, pageable stream variants
- rail: object-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: service tag and config family — rail: object-store

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [DESCRIPTION]                                        |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------- |
|   [1]   | `S3Service`        | Effect Tag class  | primary service accessor and static method owner     |
|   [2]   | `S3Service.Config` | interface         | extends `S3ClientConfig` with optional `logger`      |
|   [3]   | `S3Service.Type`   | type alias        | `S3Service$` — the structural service interface      |
|   [4]   | `S3`               | namespace + const | alias for `S3Service`; `S3.Config` / `S3.Type`       |
|   [5]   | `S3ClientInstance` | Effect Tag class  | wraps raw `S3Client` from `@aws-sdk/client-s3`       |
|   [6]   | `makeS3Service`    | `Effect.Effect`   | constructs `S3Service$` requiring `S3ClientInstance` |

[PUBLIC_TYPE_SCOPE]: typed error family — rail: object-store

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]                                         | [DESCRIPTION]                    |
| :-----: | :---------------------------------- | :---------------------------------------------------- | :------------------------------- |
|   [1]   | `SdkError`                          | `TaggedException<Error>`                              | unclassified SDK transport error |
|   [2]   | `S3ServiceError`                    | `TaggedException<…>`                                  | unclassified S3 service error    |
|   [3]   | `NoSuchKeyError`                    | `TaggedException<NoSuchKey>`                          | missing object key               |
|   [4]   | `NoSuchBucketError`                 | `TaggedException<NoSuchBucket>`                       | missing bucket                   |
|   [5]   | `NoSuchUploadError`                 | `TaggedException<NoSuchUpload>`                       | missing multipart upload         |
|   [6]   | `NotFoundError`                     | `TaggedException<NotFound>`                           | head-bucket / head-object 404    |
|   [7]   | `AccessDeniedError`                 | `TaggedException<AccessDenied>`                       | IAM or ACL denial                |
|   [8]   | `BucketAlreadyExistsError`          | `TaggedException<BucketAlreadyExists>`                | namespace collision              |
|   [9]   | `BucketAlreadyOwnedByYouError`      | `TaggedException<BucketAlreadyOwnedByYou>`            | re-create own bucket             |
|  [10]   | `EncryptionTypeMismatchError`       | `TaggedException<EncryptionTypeMismatch>`             | wrong encryption type            |
|  [11]   | `InvalidObjectStateError`           | `TaggedException<InvalidObjectState>`                 | archived object access           |
|  [12]   | `InvalidRequestError`               | `TaggedException<InvalidRequest>`                     | bad request parameter            |
|  [13]   | `InvalidWriteOffsetError`           | `TaggedException<InvalidWriteOffset>`                 | write offset mismatch            |
|  [14]   | `TooManyPartsError`                 | `TaggedException<TooManyParts>`                       | >10000 multipart parts           |
|  [15]   | `IdempotencyParameterMismatchError` | `TaggedException<IdempotencyParameterMismatch>`       | idempotency conflict             |
|  [16]   | `ObjectAlreadyInActiveTierError`    | `TaggedException<ObjectAlreadyInActiveTierException>` | restore conflict                 |
|  [17]   | `ObjectNotInActiveTierError`        | `TaggedException<ObjectNotInActiveTierException>`     | Glacier copy source              |
|  [18]   | `AllServiceErrors`                  | `readonly string[]`                                   | closed error name array          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: layer and config construction — rail: object-store

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]                   | [DESCRIPTION]                                    |
| :-----: | :-------------------------------------------- | :------------------------------- | :----------------------------------------------- |
|   [1]   | `S3Service.defaultLayer`                      | `Layer<S3Service>`               | layer with default `S3Client` config             |
|   [2]   | `S3Service.layer(config)`                     | `Layer<S3Service>`               | layer from explicit `S3Service.Config`           |
|   [3]   | `S3Service.baseLayer(evaluate)`               | `Layer<S3Service>`               | layer from raw `S3Client` factory                |
|   [4]   | `S3ClientInstance.layer`                      | `Layer<S3ClientInstance>`        | raw client layer, default config                 |
|   [5]   | `S3ClientInstance.make`                       | `Effect<S3Client, never, Scope>` | scoped raw client                                |
|   [6]   | `S3ServiceConfig.withS3ServiceConfig(config)` | middleware                       | injects config into an effect scope              |
|   [7]   | `S3ServiceConfig.setS3ServiceConfig(config)`  | `Layer<never>`                   | layer that sets config without providing service |
|   [8]   | `S3ServiceConfig.toS3ClientConfig`            | `Effect<S3ClientConfig>`         | resolves current config to SDK config            |

[ENTRYPOINT_SCOPE]: core object operations — rail: object-store

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RETURN]                                                                                                                                                                        |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `S3Service.getObject(args)`                   | object read    | `Effect<GetObjectCommandOutput, TimeoutException \| SdkError \| InvalidObjectStateError \| NoSuchKeyError, S3Service>`                                                          |
|   [2]   | `S3Service.getObject(args, {presigned:true})` | presigned URL  | `Effect<string, SdkError \| S3ServiceError, S3Service>`                                                                                                                         |
|   [3]   | `S3Service.putObject(args)`                   | object write   | `Effect<PutObjectCommandOutput, TimeoutException \| SdkError \| EncryptionTypeMismatchError \| InvalidRequestError \| InvalidWriteOffsetError \| TooManyPartsError, S3Service>` |
|   [4]   | `S3Service.putObject(args, {presigned:true})` | presigned URL  | `Effect<string, SdkError \| S3ServiceError, S3Service>`                                                                                                                         |
|   [5]   | `S3Service.use(body)`                         | service use    | injects service instance into an effect                                                                                                                                         |

[ENTRYPOINT_SCOPE]: bucket and object management (representative) — rail: object-store

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RETURN]                                                                                                                                  |
| :-----: | :----------------------------------------- | :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `S3Service$.deleteObject(args)`            | object delete   | `Effect<DeleteObjectCommandOutput, TimeoutException \| SdkError \| S3ServiceError>`                                                       |
|   [2]   | `S3Service$.listObjectsV2(args)`           | object list     | `Effect<ListObjectsV2CommandOutput, TimeoutException \| SdkError \| NoSuchBucketError>`                                                   |
|   [3]   | `S3Service$.listObjectsV2Stream(args)`     | paginated list  | `Stream<ListObjectsV2CommandOutput, TimeoutException \| SdkError \| NoSuchBucketError>`                                                   |
|   [4]   | `S3Service$.headObject(args)`              | metadata check  | `Effect<HeadObjectCommandOutput, TimeoutException \| SdkError \| NotFoundError>`                                                          |
|   [5]   | `S3Service$.headBucket(args)`              | bucket check    | `Effect<HeadBucketCommandOutput, TimeoutException \| SdkError \| NotFoundError>`                                                          |
|   [6]   | `S3Service$.createBucket(args)`            | bucket create   | `Effect<CreateBucketCommandOutput, TimeoutException \| SdkError \| BucketAlreadyExistsError \| BucketAlreadyOwnedByYouError>`             |
|   [7]   | `S3Service$.deleteBucket(args)`            | bucket delete   | `Effect<DeleteBucketCommandOutput, TimeoutException \| SdkError \| S3ServiceError>`                                                       |
|   [8]   | `S3Service$.copyObject(args)`              | object copy     | `Effect<CopyObjectCommandOutput, TimeoutException \| SdkError \| ObjectNotInActiveTierError>`                                             |
|   [9]   | `S3Service$.createMultipartUpload(args)`   | multipart start | `Effect<CreateMultipartUploadCommandOutput, TimeoutException \| SdkError \| S3ServiceError>`                                              |
|  [10]   | `S3Service$.uploadPart(args)`              | multipart part  | `Effect<UploadPartCommandOutput, TimeoutException \| SdkError \| S3ServiceError>`                                                         |
|  [11]   | `S3Service$.completeMultipartUpload(args)` | multipart end   | `Effect<CompleteMultipartUploadCommandOutput, TimeoutException \| SdkError>`                                                              |
|  [12]   | `S3Service$.abortMultipartUpload(args)`    | multipart abort | `Effect<AbortMultipartUploadCommandOutput, TimeoutException \| SdkError \| NoSuchUploadError>`                                            |
|  [13]   | `S3Service$.listBucketsStream(args)`       | paginated list  | `Stream<ListBucketsCommandOutput, TimeoutException \| SdkError>`                                                                          |
|  [14]   | `S3Service$.renameObject(args)`            | object rename   | `Effect<RenameObjectCommandOutput, TimeoutException \| SdkError \| IdempotencyParameterMismatchError>`                                    |
|  [15]   | `S3Service$.restoreObject(args)`           | Glacier restore | `Effect<RestoreObjectCommandOutput, TimeoutException \| SdkError \| ObjectAlreadyInActiveTierError>`                                      |
|  [16]   | `S3Service$.updateObjectEncryption(args)`  | encryption      | `Effect<UpdateObjectEncryptionCommandOutput, TimeoutException \| SdkError \| AccessDeniedError \| InvalidRequestError \| NoSuchKeyError>` |

## [4]-[IMPLEMENTATION_LAW]

[S3_SERVICE_TOPOLOGY]:
- `S3Service` is an Effect `Tag` class extending `Context.TagClass`; the structural interface it wraps is `S3Service$` (not exported directly — use `S3Service.Type`).
- `S3ClientInstance` is a separate `Tag` wrapping the raw `S3Client`; `S3Service.defaultLayer` and `S3Service.layer` wire both tags automatically.
- Pageable operations expose a `*Stream` companion returning `Stream.Stream` over pages; `listObjectsV2Stream`, `listBucketsStream`, `listPartsStream`, `listDirectoryBucketsStream` confirmed in `.d.ts`.
- Presigned URL operations use an overloaded signature on `getObject` and `putObject`; pass `{ presigned: true }` plus `RequestPresigningArguments` to receive `Effect<string, …>` without a raw SDK presigner import.
- All `options?: HttpHandlerOptions` fields are from `@effect-aws/commons/Types`.

[LOCAL_ADMISSION]:
- Domain modules depend on `S3Service` tag; `S3ClientInstance` is for composition roots and interop only.
- Layer selection: `S3Service.defaultLayer` for standard credential chain; `S3Service.layer(config)` when explicit region/endpoint/credentials apply; `S3Service.baseLayer(fn)` for custom `S3Client` construction.
- Error types are `TaggedException<AWS_SDK_Exception>` from `@effect-aws/commons/Errors`; discriminate by `._tag` in `Effect.catchTag` / `Effect.catchTags`.

[RAIL_LAW]:
- Package: `@effect-aws/client-s3`
- Owns: Effect-native S3 operations with typed error rail
- Accept: `S3Service` tag methods; `S3Service.layer` / `S3Service.defaultLayer` at composition root
- Reject: raw `@aws-sdk/client-s3` commands in domain logic; hand-rolled presigned URL flows when the overload covers the case
