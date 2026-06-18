# [API_CATALOGUE] @aws-sdk/client-s3

`@aws-sdk/client-s3` provides the tree-shakeable command-style S3 client for Node.js: `S3Client` holds credentials and region configuration, and each operation is a separate `Command` class sent via `client.send()`. The package exports all command input/output types, service exception classes, paginators, and waiters. In domain code the Effect wrapper `@effect-aws/client-s3` is preferred; this package surfaces as the composition-root provider and as the type source for command inputs.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-s3`
- package: `@aws-sdk/client-s3`
- module: `@aws-sdk/client-s3`
- asset: `S3Client`, command classes, input/output types, service exception classes, paginators, waiters
- rail: object-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and config family
- rail: object-store

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------- |
|   [1]   | `S3Client`               | class         | credential + region holder; dispatches commands via `send` |
|   [2]   | `S3ClientConfig`         | interface     | `region`, `credentials`, `endpoint`, retry, logger, …      |
|   [3]   | `S3ClientResolvedConfig` | interface     | fully resolved config after construction                   |
|   [4]   | `ServiceInputTypes`      | union type    | union of all `*CommandInput` types                         |
|   [5]   | `ServiceOutputTypes`     | union type    | union of all `*CommandOutput` types                        |
|   [6]   | `S3ServiceException`     | base class    | base for all S3 service exceptions                         |

[PUBLIC_TYPE_SCOPE]: service exception family
- rail: object-store

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------------------------- | :-------------- | :--------------------------------------- |
|   [1]   | `AccessDenied`                   | exception class | IAM / ACL access denial                  |
|   [2]   | `BucketAlreadyExists`            | exception class | bucket namespace collision               |
|   [3]   | `BucketAlreadyOwnedByYou`        | exception class | re-create own bucket                     |
|   [4]   | `EncryptionTypeMismatch`         | exception class | wrong encryption type on write           |
|   [5]   | `IdempotencyParameterMismatch`   | exception class | inconsistent idempotent request params   |
|   [6]   | `InvalidObjectState`             | exception class | archived / Glacier object access         |
|   [7]   | `InvalidRequest`                 | exception class | bad request parameter or header          |
|   [8]   | `InvalidWriteOffset`             | exception class | write offset does not match object size  |
|   [9]   | `NoSuchBucket`                   | exception class | bucket does not exist                    |
|  [10]   | `NoSuchKey`                      | exception class | object key does not exist                |
|  [11]   | `NoSuchUpload`                   | exception class | multipart upload ID not found            |
|  [12]   | `NotFound`                       | exception class | head-bucket / head-object 404            |
|  [13]   | `ObjectAlreadyInActiveTierError` | exception class | restore conflict: already in active tier |
|  [14]   | `ObjectNotInActiveTierError`     | exception class | copy source in Glacier                   |
|  [15]   | `TooManyParts`                   | exception class | multipart part count exceeds 10000       |

[PUBLIC_TYPE_SCOPE]: core command input/output family
- rail: object-store

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------- |
|   [1]   | `GetObjectCommand`          | command class | object download                                        |
|   [2]   | `GetObjectCommandInput`     | input type    | `{ Bucket, Key, VersionId?, Range?, … }`               |
|   [3]   | `GetObjectCommandOutput`    | output type   | `{ Body: SdkStream, ContentType?, ETag?, … }`          |
|   [4]   | `PutObjectCommand`          | command class | object upload                                          |
|   [5]   | `PutObjectCommandInput`     | input type    | `{ Bucket, Key, Body?, ContentType?, Metadata?, … }`   |
|   [6]   | `PutObjectCommandOutput`    | output type   | `{ ETag?, VersionId?, … }`                             |
|   [7]   | `DeleteObjectCommand`       | command class | single object deletion                                 |
|   [8]   | `DeleteObjectsCommand`      | command class | batch object deletion                                  |
|   [9]   | `HeadObjectCommand`         | command class | object metadata without body                           |
|  [10]   | `HeadBucketCommand`         | command class | bucket existence and access check                      |
|  [11]   | `ListObjectsV2Command`      | command class | key enumeration (page-based)                           |
|  [12]   | `ListObjectsV2CommandInput` | input type    | `{ Bucket, Prefix?, ContinuationToken?, MaxKeys?, … }` |
|  [13]   | `CreateBucketCommand`       | command class | bucket creation                                        |
|  [14]   | `DeleteBucketCommand`       | command class | bucket deletion                                        |
|  [15]   | `CopyObjectCommand`         | command class | server-side object copy                                |

[PUBLIC_TYPE_SCOPE]: multipart upload command family
- rail: object-store

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :------------------------------- | :------------ | :-------------------------- |
|   [1]   | `CreateMultipartUploadCommand`   | command class | initiate multipart upload   |
|   [2]   | `UploadPartCommand`              | command class | upload one part             |
|   [3]   | `UploadPartCopyCommand`          | command class | copy source range as a part |
|   [4]   | `CompleteMultipartUploadCommand` | command class | assemble parts into object  |
|   [5]   | `AbortMultipartUploadCommand`    | command class | cancel and clean up parts   |
|   [6]   | `ListPartsCommand`               | command class | enumerate uploaded parts    |
|   [7]   | `ListMultipartUploadsCommand`    | command class | enumerate open uploads      |
|   [8]   | `RenameObjectCommand`            | command class | atomic object rename        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- rail: object-store

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------ |
|   [1]   | `new S3Client(config: S3ClientConfig)`        | constructor    | credential + region holder                              |
|   [2]   | `client.send(command, options?)`              | dispatch       | sends any `Command` instance; returns `Promise<Output>` |
|   [3]   | `client.destroy()`                            | cleanup        | shuts down underlying sockets                           |
|   [4]   | `paginateListObjectsV2(config, input)`        | paginator      | async generator over `ListObjectsV2` pages              |
|   [5]   | `paginateListBuckets(config, input)`          | paginator      | async generator over `ListBuckets` pages                |
|   [6]   | `paginateListParts(config, input)`            | paginator      | async generator over `ListParts` pages                  |
|   [7]   | `paginateListMultipartUploads(config, input)` | paginator      | async generator over multipart upload pages             |
|   [8]   | `waitUntilObjectExists(params, input)`        | waiter         | polls until object exists                               |
|   [9]   | `waitUntilObjectNotExists(params, input)`     | waiter         | polls until object absent                               |
|  [10]   | `waitUntilBucketExists(params, input)`        | waiter         | polls until bucket exists                               |

## [4]-[IMPLEMENTATION_LAW]

[S3_CLIENT_TOPOLOGY]:
- One `S3Client` instance per deployment scope; the instance is reusable and thread-safe.
- `client.send()` accepts any command implementing `Command<Input, Output>` and returns `Promise<Output>`; command classes are individually importable for tree-shaking.
- `GetObjectCommandOutput.Body` is an `SdkStream<Readable>` on Node.js; consume it with `streamCollector` or pipe before the HTTP response completes.
- `PutObjectCommandInput.Body` accepts `string | Buffer | Readable | ReadableStream | Blob`.
- Paginators (`paginateListObjectsV2`, etc.) take a `PaginatorConfig` (`{ client, pageSize? }`) as the first argument and return an `AsyncGenerator` over page outputs.

[LOCAL_ADMISSION]:
- `S3ClientConfig` key fields: `region` (string), `credentials` (`AwsCredentialIdentityProvider`), `endpoint` (string or object for S3-compatible stores), `maxAttempts`, `retryMode`, `logger`.
- `S3ClientConfig` extends a composition of Smithy, retry, region, auth, and S3-specific input configs; domain code uses only the documented public fields.
- For Effect-native code, `@effect-aws/client-s3` wraps this client; `@aws-sdk/client-s3` is a direct dependency only at the composition root or for raw interop.

[RAIL_LAW]:
- Package: `@aws-sdk/client-s3`
- Owns: S3 command dispatch, client construction, exception types, paginators, waiters
- Accept: one `S3Client` instance per scope; command-per-operation pattern; paginator for page iteration
- Reject: `S3` fluent high-level client (`./S3`) in new code; hand-rolled pagination bypassing paginators
