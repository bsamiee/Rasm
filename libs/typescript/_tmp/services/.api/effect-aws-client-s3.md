# [API_CATALOGUE] @effect-aws/client-s3

`@effect-aws/client-s3` is the code-generated Effect wrapper over `@aws-sdk/client-s3`: every AWS SDK `*Command` is projected to one `S3Service` method returning `Effect<…CommandOutput, TimeoutException | SdkError | …typed, S3Service>`, with three orthogonal modality axes layered on the same generated surface — a `{ presigned: true }` overload on the presignable pair, a `*Stream` companion on the pageable list ops, and a per-op typed-error subset of the closed `AllServiceErrors` tuple. Consuming owners import the `S3Service` tag for domain operations, provide `S3Service.layer` / `defaultLayer` once at the composition root, and reach `S3ClientInstance` only for raw-client interop. `ObjectStore` (`persistence/object#OBJECT_STORE`) is the single owner-symbol that internalizes this package — one `Effect.Service` that wraps the tag behind `ObjectKey`/`PresignGrant`/`StorageTier` so no domain code composes an S3 command shape directly.

Members verified against the published `@effect-aws/client-s3@1.11.0` generated source (`S3Service` / `Errors` / `S3ServiceConfig`) and its registry manifest (MIT, `effect >=3.0.4 <4.0.0` peer, the three `@aws-sdk/*` + `@effect-aws/commons ^0.4.0` deps). The package is not yet installed in the workspace; admit it to the central catalog and re-ground via `assay api resolve` before the design realizes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-aws/client-s3`
- package: `@effect-aws/client-s3` (1.11.0, MIT, © Sergey Lukin / floydspace)
- effect-peer: `effect >=3.0.4 <4.0.0` (universal-tier substrate; `.api/effect.md`)
- deps: `@aws-sdk/client-s3 ^3` (command/input/output types; sibling `.api/aws-sdk-client-s3.md`), `@aws-sdk/s3-request-presigner ^3` (the presign overload covers only single-object `getObject`/`putObject`; the `Multipart` per-part presign imports this sibling's `getSignedUrl` directly; `.api/aws-sdk-s3-request-presigner.md`), `@aws-sdk/types ^3`, `@effect-aws/commons ^0.4.0` (the `TaggedException` rail + `HttpHandlerOptions`)
- module format: ESM, per-op re-export subpaths `@effect-aws/client-s3` · `/Errors` · `/S3ClientInstance` · `/S3Service` · `/S3ServiceConfig`
- runtime target: node/bun (the AWS SDK is host-bound); never enters a browser bundle
- asset: generated Effect S3 service tag, generated `TaggedException` error rail, presign overload, pageable `Stream` companions
- rail: object-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: service tag and config family — rail: object-store

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `S3Service`        | Effect service class | primary tag + static method owner; `ObjectStore` wraps it        |
|  [02]   | `S3Service.Config` | interface         | `Omit<S3ClientConfig,"logger">` + `logger?: ServiceLoggerConstructorProps \| true` |
|  [03]   | `S3Service.Type`   | type alias        | `S3Service$` — the structural service interface (methods below)  |
|  [04]   | `S3`               | namespace + const | index alias for `S3Service`; `S3.Config` / `S3.Type`             |
|  [05]   | `S3ClientInstance` | Effect Tag class  | wraps the raw `S3Client`; interop / composition-root only        |
|  [06]   | `makeS3Service`    | `Effect<S3Service, never, S3ClientInstance>` | constructs the service from a client instance     |

[PUBLIC_TYPE_SCOPE]: generated typed-error family — rail: object-store

Every modeled AWS S3 exception `X` is generated as `XError = TaggedException<X>` from `@effect-aws/commons/Errors`; `AllServiceErrors` is the closed 15-tuple of their `_tag`/`name` strings, and two catch-alls sit above the modeled set. Discriminate by `._tag` in `Effect.catchTag` / `catchTags`; the per-op error channel (section [03]) is the subset a given command can raise.

| [INDEX] | [SYMBOL]                            | [WRAPS]                                    | [MODALITY]                          |
| :-----: | :---------------------------------- | :----------------------------------------- | :---------------------------------- |
|  [01]   | `SdkError`                          | `Error & { name: "SdkError" }`             | catch-all transport / unmodeled     |
|  [02]   | `S3ServiceError`                    | `S3ServiceException & { name: "S3ServiceError" }` | catch-all S3 service            |
|  [03]   | `NoSuchKeyError`                    | `NoSuchKey`                                | missing object key                  |
|  [04]   | `NoSuchBucketError`                 | `NoSuchBucket`                             | missing bucket                      |
|  [05]   | `NoSuchUploadError`                 | `NoSuchUpload`                             | missing multipart upload            |
|  [06]   | `NotFoundError`                     | `NotFound`                                 | head-bucket / head-object 404       |
|  [07]   | `AccessDeniedError`                 | `AccessDenied`                             | IAM or ACL denial                   |
|  [08]   | `BucketAlreadyExistsError`          | `BucketAlreadyExists`                      | namespace collision                 |
|  [09]   | `BucketAlreadyOwnedByYouError`      | `BucketAlreadyOwnedByYou`                  | re-create own bucket                |
|  [10]   | `EncryptionTypeMismatchError`       | `EncryptionTypeMismatch`                   | wrong encryption type               |
|  [11]   | `InvalidObjectStateError`           | `InvalidObjectState`                       | archived object access              |
|  [12]   | `InvalidRequestError`               | `InvalidRequest`                           | bad request parameter               |
|  [13]   | `InvalidWriteOffsetError`           | `InvalidWriteOffset`                       | append write-offset mismatch        |
|  [14]   | `TooManyPartsError`                 | `TooManyParts`                             | >10000 multipart parts              |
|  [15]   | `IdempotencyParameterMismatchError` | `IdempotencyParameterMismatch`             | rename idempotency conflict         |
|  [16]   | `ObjectAlreadyInActiveTierError`    | `ObjectAlreadyInActiveTierError`           | restore conflict                    |
|  [17]   | `ObjectNotInActiveTierError`        | `ObjectNotInActiveTierError`               | Glacier copy source                 |
|  [18]   | `AllServiceErrors`                  | `readonly [...15 tag strings]`             | closed tuple; the interop error set |

Rows [16]/[17]: the AWS S3 shapes already end in `Error` (`@aws-sdk/client-s3` exports `ObjectAlreadyInActiveTierError`/`ObjectNotInActiveTierError`; no `...Exception` class exists), so the generated `TaggedException` type name coincides with the wrapped SDK class — `@effect-aws` import-aliases each to a local `...Exception` identifier, and the `AllServiceErrors` `_tag`/`name` string stays the `...Error` form.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: layer and config construction — rail: object-store

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]                   | [SIGNATURE / NOTE]                                             |
| :-----: | :-------------------------------------------- | :------------------------------- | :------------------------------------------------------------- |
|  [01]   | `S3Service.defaultLayer`                      | `Layer<S3Service, never, never>` | default credential-chain client                                |
|  [02]   | `S3Service.layer(config)`                     | `(config: S3Service.Config) => Layer<S3Service>` | explicit region/endpoint/credentials/logger  |
|  [03]   | `S3Service.baseLayer(evaluate)`               | `(evaluate: (defaultConfig: S3ClientConfig) => S3Client) => Layer<S3Service>` | custom `S3Client` factory |
|  [04]   | `S3ClientInstance.layer` / `.make`            | `Layer<S3ClientInstance>` / `Effect<S3Client, never, Scope>` | raw client layer / scoped client |
|  [05]   | `S3ServiceConfig.withS3ServiceConfig(config)` | dual `{ (config): (effect) => Effect; (effect, config): Effect }` | scoped per-effect config override |
|  [06]   | `S3ServiceConfig.setS3ServiceConfig(config)`  | `(config: S3Service.Config) => Layer<never, never, never>` | set config without providing the service |
|  [07]   | `S3ServiceConfig.toS3ClientConfig`            | `Effect<S3ClientConfig>`         | resolve the current config to raw SDK config                   |

[ENTRYPOINT_SCOPE]: the generated operation surface — rail: object-store

One generation pattern owns all 110+ operations; the AWS command roster is the discriminant seed, never a hand-authored method list. For every `@aws-sdk/client-s3` `<Op>Command` (input `<Op>CommandInput`, output `<Op>CommandOutput`):

```ts
// generated method shape (base modality)
<op>(
  args: <Op>CommandInput,
  options?: { readonly presigned?: false } & HttpHandlerOptions // HttpHandlerOptions from @effect-aws/commons/Types
): Effect.Effect<<Op>CommandOutput, Cause.TimeoutException | SdkError | ErrorsFor<op>, S3Service>
```

`ErrorsFor<op>` is the per-op subset of the modeled `AllServiceErrors` the command can raise (e.g. `getObject → NoSuchKeyError | InvalidObjectStateError`, `createBucket → BucketAlreadyExistsError | BucketAlreadyOwnedByYouError`, `putObject → EncryptionTypeMismatchError | InvalidRequestError | InvalidWriteOffsetError | TooManyPartsError`, `renameObject → IdempotencyParameterMismatchError`, `restoreObject → ObjectAlreadyInActiveTierError`, `copyObject → ObjectNotInActiveTierError`, `updateObjectEncryption → AccessDeniedError | InvalidRequestError | NoSuchKeyError`, `headObject`/`headBucket → NotFoundError`, `abortMultipartUpload → NoSuchUploadError`). Three modality axes layer on the base without new method names:

```ts
// PRESIGN axis — getObject and putObject ONLY carry the second overload
getObject(args: GetObjectCommandInput, options: { readonly presigned: true } & RequestPresigningArguments):
  Effect.Effect<string, SdkError | S3ServiceError, S3Service>   // options.expiresIn is the URL TTL in seconds
putObject(args: PutObjectCommandInput, options: { readonly presigned: true } & RequestPresigningArguments):
  Effect.Effect<string, SdkError | S3ServiceError, S3Service>

// STREAM axis — the four pageable list ops gain a <op>Stream companion that auto-follows the continuation token
listObjectsV2Stream(args):        Stream.Stream<ListObjectsV2CommandOutput,        Cause.TimeoutException | SdkError | NoSuchBucketError, S3Service>
listBucketsStream(args):          Stream.Stream<ListBucketsCommandOutput,          Cause.TimeoutException | SdkError, S3Service>
listPartsStream(args):            Stream.Stream<ListPartsCommandOutput,            Cause.TimeoutException | SdkError, S3Service>
listDirectoryBucketsStream(args): Stream.Stream<ListDirectoryBucketsCommandOutput, Cause.TimeoutException | SdkError, S3Service>
```

Command roster (seed data for the generation — each row is one generated method, `<op>` = camelCased command):

| [GROUP]                | [OPERATIONS]                                                                                                                                     |
| :--------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
| object I/O             | `getObject`◆, `putObject`◆, `deleteObject`, `deleteObjects`, `copyObject`, `renameObject`, `restoreObject`, `writeGetObjectResponse`             |
| object metadata        | `headObject`, `getObjectAttributes`, `getObjectAcl`/`putObjectAcl`, `getObjectTagging`/`putObjectTagging`/`deleteObjectTagging`                  |
| object lock/retention  | `getObjectLegalHold`/`putObjectLegalHold`, `getObjectRetention`/`putObjectRetention`, `getObjectLockConfiguration`/`putObjectLockConfiguration`, `updateObjectEncryption`, `getObjectTorrent` |
| multipart              | `createMultipartUpload`, `uploadPart`, `uploadPartCopy`, `completeMultipartUpload`, `abortMultipartUpload`, `listMultipartUploads`, `listParts`✱ |
| list family            | `listObjects`, `listObjectsV2`✱, `listObjectVersions`, `listBuckets`✱, `listDirectoryBuckets`✱, `createSession`, `selectObjectContent`          |
| bucket lifecycle       | `createBucket`, `deleteBucket`, `headBucket`, `getBucketLocation`, `getBucketVersioning`/`putBucketVersioning`, `getBucketLifecycleConfiguration`/`putBucketLifecycleConfiguration`/`deleteBucketLifecycle` |
| bucket policy/security | `getBucketPolicy`/`putBucketPolicy`/`deleteBucketPolicy`/`getBucketPolicyStatus`, `getBucketAcl`/`putBucketAcl`, `getBucketCors`/`putBucketCors`/`deleteBucketCors`, `getBucketEncryption`/`putBucketEncryption`/`deleteBucketEncryption`, `getBucketAbac`/`putBucketAbac`, `getPublicAccessBlock`/`putPublicAccessBlock`/`deletePublicAccessBlock`, `getBucketOwnershipControls`/`putBucketOwnershipControls`/`deleteBucketOwnershipControls` |
| bucket config          | `*BucketAccelerateConfiguration`, `*BucketLogging`, `*BucketNotificationConfiguration`, `*BucketReplication`, `*BucketRequestPayment`, `*BucketTagging`, `*BucketWebsite`, `*Bucket{Analytics,Inventory,Metrics,IntelligentTiering}Configuration` (get/put/delete/list), `*BucketMetadata{,Table}Configuration`, `updateBucketMetadata{Inventory,Journal}TableConfiguration` |

◆ = carries the presign overload · ✱ = carries a `*Stream` companion.

## [04]-[IMPLEMENTATION_LAW]

[S3_SERVICE_TOPOLOGY]:
- `S3Service` is generated once from the AWS SDK command set; `S3Service.Type` (`S3Service$`) is the structural interface, reached as `S3Service.<op>(...)` statics or via a resolved instance. `S3ClientInstance` is a separate tag wrapping the raw `S3Client`; `defaultLayer` / `layer` / `baseLayer` wire both automatically, so a composition root provides exactly one layer.
- Presign is an overload on EXACTLY `getObject`/`putObject`, not a parallel method: `{ presigned: true } & RequestPresigningArguments` swaps the return to `Effect<string, SdkError | S3ServiceError>` and folds `@aws-sdk/s3-request-presigner` in — a raw `getSignedUrl` import for those two single-object verbs is the deleted form. Every other presign (the `Multipart` per-part `UploadPartCommand`, `HeadObject`, `GetBucket*`) has no overload and uses `getSignedUrl` directly (`aws-sdk-s3-request-presigner.md`).
- The `*Stream` companion is the pagination owner: it follows the continuation token internally and emits one page per pull as an `effect/Stream`, so a manual `ContinuationToken` loop over `listObjectsV2` is the deleted form.
- Config precedence: `S3Service.layer(config)` at the root, `S3ServiceConfig.setS3ServiceConfig` to seed config without the service, `withS3ServiceConfig` for a scoped per-effect override.

[OBJECT_STORE_STACKING]:
- `ObjectStore` (`persistence/object#OBJECT_STORE`) is the one `Effect.Service` over `S3Service`; the tag stays in its `R` channel (`Effect<…, ObjectFault, S3Service>`) and is satisfied once at the composition root by `S3Service.layer(config)`. `put`/`get`/`head`/`delete`/`sweep`/`presign` each compose exactly one generated method — `putObject`/`getObject`/`headObject`/`deleteObject`/`copyObject`, the multipart quartet (`createMultipartUpload`→`uploadPart`→`completeMultipartUpload`, `abortMultipartUpload` on interrupt), and `listObjectsV2Stream` for the lifecycle sweep — under `.pipe(Effect.catchTag(...))` mapping the typed rail into the folder `ObjectFault`.
- Presign stacks under the `PresignGrant` `Data.TaggedEnum` (`Get`/`Put`/`Multipart`) folded by `Match.tagsExhaustive`: `Get`/`Put` route to `getObject`/`putObject` with the `{ presigned: true, expiresIn: Duration.toSeconds(ttl) }` overload (the only two methods that carry it), while `Multipart` — `uploadPart` has NO presign overload — routes each `UploadPartCommand` through sibling `@aws-sdk/s3-request-presigner` `getSignedUrl` over the raw `S3ClientInstance` client after `createMultipartUpload` yields the `UploadId` (`aws-sdk-s3-request-presigner.md`). A new grant verb is one `Data.TaggedEnum` row breaking the fold at compile time, never a new presign function.
- Stack with the universal `effect` rail: the `*Stream` output feeds `Stream.mapEffect`/`Stream.runFold` for the sweep; `Cause.TimeoutException` in every op channel composes the `kernel/fault` degradation budget; `ObjectKey` is the `Brand.refined` 32-char `XxHash128` digest content address, and `Body` accepts an `effect/Stream<Uint8Array>` collapsed to bytes before the SDK call. Sibling `@aws-sdk/client-s3` supplies the `*CommandInput`/`*CommandOutput`, `StorageClass`, and the `UploadPartCommand` the `Multipart` presign builds; sibling `@aws-sdk/s3-request-presigner` `getSignedUrl` is imported directly for that `Multipart` per-part presign (the overload owns only single-object `getObject`/`putObject`).

[RAIL_LAW]:
- Package: `@effect-aws/client-s3`
- Owns: generated Effect-native S3 operations, the `TaggedException` typed-error rail, the presign overload, and the pageable `*Stream` companions
- Accept: `S3Service` tag methods in domain code (behind `ObjectStore`); `S3Service.layer` / `defaultLayer` / `baseLayer` at the composition root; `{ presigned: true }` for the single-object `getObject`/`putObject` URL grants (the `Multipart` per-part presign via sibling `getSignedUrl` over `S3ClientInstance`); `*Stream` for pagination; `_tag` discrimination over the modeled error subset
- Reject: raw `@aws-sdk/client-s3` command-`send` calls or a bare `getSignedUrl` for the single-object `getObject`/`putObject` verbs the overload already covers; a hand-rolled continuation-token loop; a parallel presign helper when the overload covers the case; importing the SDK error classes instead of the generated `TaggedException` rail
