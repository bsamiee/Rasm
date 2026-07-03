# [API_CATALOGUE] @aws-sdk/client-s3

`@aws-sdk/client-s3` is the tree-shakeable command-style S3 client: `S3Client` holds region + credentials + retry config, every one of the 112 operations is a `Command<Input, Output>` sent through the single polymorphic `client.send(cmd)`, and the package exports every `*CommandInput`/`*CommandOutput` type, the `S3ServiceException` hierarchy, the `paginate*` async generators, and the `waitUntil*` pollers. In `services` this is NOT the domain surface — `persistence/object` operates S3 through the Effect-native `@effect-aws/client-s3` `S3Service` (typed-error rail, `*Stream` pagination, `{ presigned: true }` overload). This package appears only as the composition-root provider the `S3Service` layer wraps and as the type source the owner imports (`GetObjectCommandOutput`, `PutObjectCommandOutput`, `CompletedPart`, `StorageClass`); `@aws-sdk/s3-request-presigner` `getSignedUrl` is the interop presign for the rare verb the overload does not reach.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-s3`
- package: `@aws-sdk/client-s3` (3.1078.0, Apache-2.0, © Amazon.com)
- module format: dual ESM (`dist-es`) / CJS (`dist-cjs`), `.d.ts` at `dist-types/index.d.ts`; one flat barrel — command classes are individually importable for tree-shaking
- runtime target: Node `>=20.0.0`; the credentialed client is node-only — the browser receives only an issued presigned URL and does plain HTTP against it
- surface: 112 `*Command` classes, `S3Client` + the `S3` aggregated client, the `S3ServiceException` hierarchy, `paginate*` generators, `waitUntil*` pollers; `Body` is the declared `StreamingBlobTypes` (from `@smithy/types`), node-augmented to an `SdkStream` on the output side
- consumer: the provider under `@effect-aws/client-s3` (`effect-aws-client-s3.md`) + `@aws-sdk/s3-request-presigner` (`aws-sdk-s3-request-presigner.md`); domain code imports types here, never dispatches commands here
- rail: object-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, config, and the command-union carriers
- rail: object-store

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `S3Client`               | class         | credential + region holder; `send(cmd)` dispatches any command |
|  [02]   | `S3`                     | class         | aggregated client (`extends S3Client`) with a method per operation — rejected in new code |
|  [03]   | `S3ClientConfig`         | interface     | `region`, `credentials`, `endpoint`, `maxAttempts`, `retryMode`, `logger`, `requestHandler`, … |
|  [04]   | `S3ClientResolvedConfig` | interface     | the fully resolved config after construction               |
|  [05]   | `ServiceInputTypes`      | union type    | union of all `*CommandInput` — the polymorphic `send` input carrier |
|  [06]   | `ServiceOutputTypes`     | union type    | union of all `*CommandOutput` — the polymorphic `send` output carrier |
|  [07]   | `S3ServiceException`     | base class    | base of every S3 service exception (`extends ServiceException`) |

[PUBLIC_TYPE_SCOPE]: service exception hierarchy (representative — narrow by `instanceof` or `error.name`)
- rail: object-store

| [INDEX] | [SYMBOL]                                                  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `NoSuchBucket` / `NoSuchKey` / `NoSuchUpload`             | missing bucket / object key / multipart upload |
|  [02]   | `NotFound`                                                | head-bucket / head-object 404            |
|  [03]   | `AccessDenied`                                            | IAM / ACL access denial                  |
|  [04]   | `BucketAlreadyExists` / `BucketAlreadyOwnedByYou`         | namespace collision / re-create own bucket |
|  [05]   | `InvalidObjectState`                                      | archived / Glacier object access         |
|  [06]   | `InvalidRequest` / `InvalidPrefix` / `InvalidWriteOffset` | bad parameter / bad prefix / offset ≠ object size |
|  [07]   | `EncryptionTypeMismatch`                                  | wrong encryption type on write           |
|  [08]   | `TooManyParts`                                            | multipart part count exceeds 10000       |
|  [09]   | `IdempotencyParameterMismatch`                            | inconsistent idempotent request params (rename) |
|  [10]   | `ObjectAlreadyInActiveTierError` / `ObjectNotInActiveTierError` | restore/copy tier conflicts        |
|  [11]   | `UnsupportedMediaType`                                    | unsupported content type                 |
|  [12]   | annotation family: `NoSuchAnnotation` / `AnnotationLimitExceeded` / `AnnotationNameTooLong` / `InvalidAnnotationName` | object-annotation errors (SDK ≥ 3.107x) |

[PUBLIC_TYPE_SCOPE]: core command families — each `*Command` carries its `*CommandInput`/`*CommandOutput`
- rail: object-store

| [INDEX] | [SYMBOL]                                                         | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `GetObjectCommand` → `{ Body: SdkStream, ContentType?, ETag?, … }` | object download (Body is a `SdkStream<Readable>` on node) |
|  [02]   | `PutObjectCommand` → `{ ETag?, VersionId?, … }`                  | object upload; input `Body: string \| Buffer \| Readable \| ReadableStream \| Blob` |
|  [03]   | `DeleteObjectCommand` / `DeleteObjectsCommand`                   | single / batch object deletion                        |
|  [04]   | `HeadObjectCommand` / `HeadBucketCommand`                        | metadata-only existence + access checks               |
|  [05]   | `CopyObjectCommand` / `RenameObjectCommand`                      | server-side copy / atomic rename                      |
|  [06]   | `ListObjectsV2Command` → `{ Contents?, IsTruncated, NextContinuationToken?, … }` | page-based key enumeration            |
|  [07]   | `CreateBucketCommand` / `DeleteBucketCommand`                    | bucket lifecycle                                      |
|  [08]   | `CompletedPart`                                                  | `{ PartNumber, ETag }` — the multipart part model the owner accumulates |

[PUBLIC_TYPE_SCOPE]: multipart command family
- rail: object-store

| [INDEX] | [SYMBOL]                                                          | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------- | :-------------------------- |
|  [01]   | `CreateMultipartUploadCommand` → `{ UploadId, … }`               | initiate multipart upload   |
|  [02]   | `UploadPartCommand` / `UploadPartCopyCommand`                    | upload a part / copy a source range as a part |
|  [03]   | `CompleteMultipartUploadCommand`                                 | assemble `{ Parts: CompletedPart[] }` into the object |
|  [04]   | `AbortMultipartUploadCommand`                                    | cancel and reclaim staged parts |
|  [05]   | `ListPartsCommand` / `ListMultipartUploadsCommand`               | enumerate parts / open uploads (see the paginator caveat) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction, the polymorphic dispatch, and lifecycle
- rail: object-store

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `new S3Client(config: S3ClientConfig)`        | constructor    | one instance per deployment scope; reusable, thread-safe |
|  [02]   | `client.send(command, options?)`              | dispatch       | the single entry — any `Command<Input, Output>` → `Promise<Output>` |
|  [03]   | `client.destroy()`                            | cleanup        | shuts down the underlying sockets                       |

[ENTRYPOINT_SCOPE]: paginators (real set — `S3PaginationConfiguration` = `{ client, pageSize? }`)
- rail: object-store

| [INDEX] | [SURFACE]                                | [CAPABILITY]                                     |
| :-----: | :--------------------------------------- | :----------------------------------------------- |
|  [01]   | `paginateListObjectsV2(config, input)`   | `AsyncGenerator<ListObjectsV2CommandOutput>` — the sweep the `LifecyclePolicy` walks |
|  [02]   | `paginateListParts(config, input)`       | async generator over multipart part pages        |
|  [03]   | `paginateListBuckets(config, input)`     | async generator over bucket pages                |
|  [04]   | `paginateListDirectoryBuckets(config, input)` | async generator over S3 Express directory buckets |
|  [05]   | `paginateListObjectAnnotations(config, input)` | async generator over object-annotation pages |

[ENTRYPOINT_SCOPE]: waiters (`WaiterConfiguration<S3Client>` + input → `Promise<WaiterResult>`)
- rail: object-store

| [INDEX] | [SURFACE]                                 | [CAPABILITY]                    |
| :-----: | :---------------------------------------- | :------------------------------ |
|  [01]   | `waitUntilObjectExists(params, input)`    | polls until the object exists   |
|  [02]   | `waitUntilObjectNotExists(params, input)` | polls until the object is absent |
|  [03]   | `waitUntilBucketExists(params, input)`    | polls until the bucket exists   |
|  [04]   | `waitUntilBucketNotExists(params, input)` | polls until the bucket is absent |

## [04]-[IMPLEMENTATION_LAW]

[S3_CLIENT_TOPOLOGY]:
- one `S3Client` per scope; `client.send(cmd)` is the only dispatch verb — the 112 commands are data over one `Command<Input, Output>` shape, so tree-shaking keeps only the commands actually sent. `ServiceInputTypes`/`ServiceOutputTypes` are the union carriers `send` is typed against.
- `GetObjectCommandOutput.Body` and `PutObjectCommandInput.Body` are both the declared `StreamingBlobTypes` (from `@smithy/types`); on node the OUTPUT `Body` is runtime-augmented by the `sdkStreamMixin` to an `SdkStream<Readable>` carrying `transformToByteArray`/`transformToWebStream`/`transformToString` — consume it before the HTTP response completes. The INPUT `Body` accepts `string | Uint8Array | Buffer | Readable | ReadableStream | Blob`.
- paginators take `S3PaginationConfiguration` (`{ client, pageSize? }`) as the first argument and yield an `AsyncGenerator` over page outputs; there is NO `paginateListMultipartUploads` — enumerate open uploads with `ListMultipartUploadsCommand` + its `NextKeyMarker`/`NextUploadIdMarker` cursors, or the `@effect-aws/client-s3` `*Stream` variant.
- `S3ClientConfig` extends a composition of Smithy, retry, region, auth, and S3-specific input configs; domain code sets only the documented public fields (`region`, `credentials`, `endpoint`, `maxAttempts`, `retryMode`, `logger`).

[STACKING]:
- provider under `@effect-aws/client-s3`: the `S3Service` layer wraps this `S3Client` and maps every command to an `Effect` with a typed-error rail; `persistence/object` `ObjectStore` calls `S3Service.getObject`/`putObject`/`createMultipartUpload`/`uploadPart`/`completeMultipartUpload`/`abortMultipartUpload`/`listObjectsV2Stream`, never `client.send` directly. This package is imported by the owner ONLY for provider types — `GetObjectCommandOutput`, `PutObjectCommandOutput`, `CompletedPart`, and the `StorageClass` value-map (`"STANDARD" | "STANDARD_IA" | "GLACIER_IR" | "DEEP_ARCHIVE" | …`) the `LifecyclePolicy`/`ObjectStorePolicy` tier rows key on — that the `S3Service` methods return/consume. `S3Service.layer(config)` / `S3Service.defaultLayer` construct the client at the composition root.
- interop presign: `@aws-sdk/s3-request-presigner` `getSignedUrl(client, command)` is the presign for the verb the `S3Service` `{ presigned: true }` overload does not cover (e.g. per-part multipart URLs); the overload is preferred for `Get`/`Put`, the raw presigner is interop-only.
- credential ingress: the `S3ClientConfig.credentials` resolve through `security/secret` `SecretStore` (`SecretRef.Doppler(...)`), never a bare `process.env` read; the tenancy GUC scopes the bucket/key prefix.

[RAIL_LAW]:
- package: `@aws-sdk/client-s3`
- owns: S3 command dispatch, client construction, the `*CommandInput`/`*CommandOutput` type source, the `S3ServiceException` hierarchy, paginators, waiters
- accept: one `S3Client` per scope; the `client.send(new XxxCommand(input))` pattern at composition roots and interop; type imports (`GetObjectCommandOutput`, `PutObjectCommandOutput`, `CompletedPart`, `StorageClass`) into the `S3Service` owner
- reject: the `S3` aggregated fluent client in new code; `client.send` in domain logic where the `@effect-aws/client-s3` `S3Service` tag owns the operation; a hand-rolled pagination loop where `paginate*` (or the `*Stream` variant) applies; a cited `paginateListMultipartUploads`
