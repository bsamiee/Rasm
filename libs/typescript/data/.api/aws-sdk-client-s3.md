# [TS_DATA_API_AWS_SDK_CLIENT_S3]

`@aws-sdk/client-s3` drives every S3-compatible endpoint through one `S3Client` whose single polymorphic `send(command)` discriminates the whole command space; `endpoint` + `forcePathStyle` retarget MinIO, R2, Tigris, or Ceph, never a per-verb method. It carries its own `requestHandler`, so under Effect the client is `acquireRelease`d and each `send` is a `tryPromise` bridging the fiber `AbortSignal` to SDK cancellation — the Effect seam is the wrap, never the transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-s3`
- package: `@aws-sdk/client-s3` (Apache-2.0)
- module: ESM/CJS dual, `sideEffects: false`, per-command deep-import subpaths, tree-shakeable
- runtime: `runtime:node` server object plane, browser presigned-direct; `credentialDefaultProvider` is node-only
- transport: own `requestHandler` — `@smithy/node-http-handler` (Node `https.Agent`/HTTP2) or `@smithy/fetch-http-handler`; never the `@effect/platform` `HttpClient`
- rail: store/object — one `send` over every command value

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, its config, and the polymorphic send

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `S3Client` / `.send(command, options?)` / `.destroy()`                       | client        | the one dispatch; scoped resource       |
|  [02]   | `S3ClientConfig` (`region`/`credentials`/`endpoint`)                         | config        | `endpoint`+`forcePathStyle` = S3-compat |
|  [03]   | `S3ClientConfig` (`requestHandler`/`maxAttempts`/`retryMode`)                | transport     | pooled handler; adaptive retry budget   |
|  [04]   | `S3ClientConfig` (`requestChecksumCalculation`/`responseChecksumValidation`) | integrity     | default checksum policy                 |
|  [05]   | `S3ClientConfig` (`useDualstackEndpoint`/`useFipsEndpoint`/`useArnRegion`)   | endpoint      | dualstack/FIPS/ARN routing              |
|  [06]   | `S3` (extends `S3Client`)                                                    | flat client   | convenience; command form tree-shakes   |
|  [07]   | `RuntimeExtension` / `S3ClientConfig.extensions`                             | extension     | credential/handler/checksum hooks       |

[PUBLIC_TYPE_SCOPE]: the object command rows
- Seed rows of the content-addressed plane; one `send` owns the full command space, each `*CommandInput` carrying the conditional, checksum, encryption, and range members the store composes.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `PutObjectCommand`                                           | write         | conditional-put idempotency; checksum content-verify     |
|  [02]   | `GetObjectCommand` → `Body: StreamingBlobPayloadOutputTypes` | read          | ranged/part reads; `ChecksumMode: "ENABLED"` re-verifies |
|  [03]   | `HeadObjectCommand` / `GetObjectAttributesCommand`           | metadata      | `ETag`/`Checksum`/`ObjectParts` probe, no body           |
|  [04]   | `DeleteObjectCommand` / `DeleteObjectsCommand` (batch ≤1000) | delete        | reference-sweep GC; batch retention sweeps               |
|  [05]   | `CopyObjectCommand`                                          | copy          | server-side rekey; `CopySourceIf*` conditional           |
|  [06]   | `ListObjectsV2Command` / `ListObjectVersionsCommand`         | list          | prefix walk for GC/audit; version enumeration            |
|  [07]   | `PutObjectTaggingCommand` / `GetObjectTaggingCommand`        | tagging       | retention-class + reference-count tags                   |
|  [08]   | `PutBucketLifecycleConfigurationCommand`                     | lifecycle     | retention-class GC as a bucket rule set                  |
|  [09]   | `RestoreObjectCommand` / `SelectObjectContentCommand`        | archive/query | Glacier restore; server-side S3 Select                   |

- `PutObjectCommand` input: `IfNoneMatch` `IfMatch` `ChecksumSHA256` `ChecksumAlgorithm` `ContentMD5` `StorageClass` `ServerSideEncryption` `SSECustomerKey` `Metadata` `Tagging`.
- `GetObjectCommand` input: `Range` `PartNumber` `ChecksumMode` `IfNoneMatch` `IfModifiedSince` `ResponseContentType`.

[PUBLIC_TYPE_SCOPE]: multipart, pagination, waiters, and the error rail
- Multipart is the low-level command family composed under an Effect scope; paginators and waiters are `AsyncIterable`/promise helpers the wrap lifts; the `S3ServiceException` hierarchy seeds the tagged rail.
- [MULTIPART]: `CreateMultipartUploadCommand` `UploadPartCommand` `UploadPartCopyCommand` `CompleteMultipartUploadCommand` `AbortMultipartUploadCommand` `ListPartsCommand` `ListMultipartUploadsCommand` — hand-composed large-blob ingest, `Abort` on scope interrupt.
- [PAGINATOR]: `paginateListObjectsV2` `paginateListParts` `paginateListBuckets` `paginateListDirectoryBuckets` `paginateListObjectAnnotations` — `AsyncIterable` → `Stream.fromAsyncIterable`.
- [WAITER]: `waitUntilObjectExists` `waitUntilObjectNotExists` `waitUntilBucketExists` `waitUntilBucketNotExists` — poll-to-consistency after a write/delete; `{ client, maxWaitTime }` bounds the wait.
- [ERROR_BASE]: `S3ServiceException` — the tagged-error mapping source; `$metadata.httpStatusCode` carries 412/404.
- [TAGGED_FAULT]: `NoSuchKey` `NoSuchBucket` `NoSuchUpload` `NotFound` `InvalidObjectState` `InvalidWriteOffset` `EncryptionTypeMismatch` `TooManyParts` `BucketAlreadyOwnedByYou` — miss, archive-state, append-offset, SSE, and multipart fault classification.
- [ENUM]: `StorageClass` `ChecksumAlgorithm` `ChecksumMode` `ServerSideEncryption` `ObjectCannedACL` — bounded policy values on command inputs.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the Effect wrap — client lifecycle and typed send

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Effect.acquireRelease(new S3Client, c => c.destroy())`              | client layer   | `S3Client` as a scoped `Layer.scoped` service   |
|  [02]   | `Effect.tryPromise(client.send(command, { abortSignal }))`           | typed send     | the one dispatch wrap; interruption → SDK abort |
|  [03]   | `mapS3Error(e)` → `Match` on name / `$metadata.httpStatusCode`       | error fold     | `Data.TaggedError`; 412 ⇒ noop, 404 ⇒ miss      |
|  [04]   | `Stream.fromAsyncIterable(paginateListObjectsV2({ client }, in))`    | paginated read | GC prefix walk as an Effect `Stream`            |
|  [05]   | `Config.redacted(...)` / `Config.string(...)` → credentials/endpoint | config         | secrets `Redacted`, provider parameterized      |

[ENTRYPOINT_SCOPE]: content-address idempotency and multipart composition

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `PutObjectCommand{ Key: contentKey, IfNoneMatch: "*", ChecksumSHA256 }` | conditional put | first-writer wins; digest = key            |
|  [02]   | `catch $metadata.httpStatusCode === 412` ⇒ `Effect.void`                | idempotency     | the re-put is a proven noop, not a fault   |
|  [03]   | `Effect.acquireRelease(CreateMultipartUpload, AbortMultipartUpload)`    | multipart       | bounded-bytes ingest; abort on interrupt   |
|  [04]   | `UploadPart` fold ⇒ `CompleteMultipartUpload{ IfNoneMatch: "*" }`       | multipart       | conditional at completion, first-writer    |
|  [05]   | `GetObjectCommand{ ChecksumMode: "ENABLED" }`                           | read verify     | verify `ChecksumSHA256` against the key    |
|  [06]   | `PutBucketLifecycleConfigurationCommand{ Rules }` + `PutObjectTagging`  | retention GC    | reference-sweep GC by retention class      |
|  [07]   | `GetObjectCommandOutput.Body`                                           | body read       | node `SdkStream<Readable>`, single-consume |

- `GetObjectCommandOutput.Body`: `transformToByteArray()` / `transformToWebStream()` / `transformToString(enc?)`; single-consume, so buffer once then `sharp(buffer).clone()` per derivative, never a re-piped stream.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one polymorphic `send`, commands as rows: the store wraps `client.send(command)` once, so a new object operation is a new command row, never a method — the `S3` flat client is convenience only.
- content-address idempotency, not a lock: the object `Key` IS the kernel `ContentKey` digest, so identical bytes produce identical keys; `IfNoneMatch: "*"` makes the first `PutObject` win and every re-put fail HTTP 412, caught on the `S3ServiceException` at `$metadata.httpStatusCode === 412` and returned a noop. No `PreconditionFailed` class exists — reading 412 as a named class is a phantom.
- checksums are the integrity proof: `ChecksumSHA256` + `ChecksumAlgorithm: "SHA256"` on write and `ChecksumMode: "ENABLED"` on read verify the digest end to end; `requestChecksumCalculation`/`responseChecksumValidation` set the client default policy.
- own transport, Effect is the wrap: interruption of the fiber aborts the in-flight request through the `AbortSignal`, and the client `destroy`s on scope close; no `@effect/platform` `HttpClient` rides the S3 wire.
- multipart splits by body shape: bounded bytes ride `CreateMultipartUpload`/`UploadPart`/`CompleteMultipartUpload` under `Effect.acquireRelease` that `AbortMultipartUpload`s on interrupt, `IfNoneMatch` riding COMPLETE since `CreateMultipartUploadRequest` carries no conditional member; part size is a parameter, and a streaming or unknown-length body rides `@aws-sdk/lib-storage`'s `Upload`.

[STACKING]:
- `effect`(`.api/effect.md`): `Layer.scoped` holds the `S3Client` via `acquireRelease`; `Effect.tryPromise` with `{ abortSignal }` lifts each `send`; the `S3ServiceException` hierarchy maps through `Match` to `Data.TaggedError` (`ObjectMissing`/`ObjectConflict`/`ObjectFault`); `Stream.fromAsyncIterable` lifts a paginator; `Config.redacted` supplies credentials; `Schedule` composes with `maxAttempts`/`retryMode`.
- `@aws-sdk/lib-storage`(`.api/aws-sdk-lib-storage.md`): a streaming or unknown-length body rides `Upload`, which spreads the same `IfNoneMatch: "*"` + checksum members across its put and multipart-complete legs; bounded bytes stay on this client's hand-composed multipart.
- `@aws-sdk/s3-request-presigner`(`.api/aws-sdk-s3-request-presigner.md`): `getSignedUrl(client, command, { expiresIn })` mints a presigned URL from the SAME client + command, inheriting `credentials`/`region`/`endpoint`/`forcePathStyle` — the `object/presign` browser-direct rows.
- `sharp`(`.api/sharp.md`): the codec fan-out reads the `GetObjectCommand` `Body` once via `transformToByteArray()` into a `Buffer`, `sharp(buffer).clone()` per derivative-spec row, and writes each back through this client's conditional-put row (`PutObjectCommand{ Key: derivativeContentKey, IfNoneMatch: "*", ChecksumSHA256 }`, 412 ⇒ noop) — every derivative content-addressed like its source.
- `@effect/opentelemetry`: `Effect.withSpan("s3.putObject", { attributes: { bucket, key } })` wraps each `send` on the same exporter Layer as the SQL spans, correlating an object write to its journal event.
- `kernel`/`security`: `ObjectKey` = kernel `ContentKey` (the digest that is the S3 Key); `credentials` and `SSECustomerKey` are `Redacted` from `host/config`; a presigned URL is a bounded-TTL capability token `security` reasons about.

[LOCAL_ADMISSION]:
- target any S3-compatible provider by `endpoint` + `forcePathStyle` as `Config` facts; `credentials` and SSE-C keys stay `Redacted`, never a hardcoded AWS region.
- size `partSize`/`queueSize` from `Config`, never call-site literals; release hand-composed multipart through `AbortMultipartUpload` on interrupt.
- wrap every `send` once under `Effect.tryPromise` with `{ abortSignal }`; an un-abortable request that leaks past fiber interruption is the rejected form.

[RAIL_LAW]:
- Package: `@aws-sdk/client-s3`
- Owns: the `S3Client` + `send` dispatch, the object/multipart/list command family, `paginate*` iterables, `waitUntil*` pollers, the `S3ServiceException` hierarchy, the bounded enum vocabularies, and `S3ClientConfig` (endpoint/credentials/checksum/retry/transport)
- Accept: one `Effect`-wrapped `send` per command value, `IfNoneMatch: "*"` + checksum as the content-address idempotency pattern, 412-by-status noop, `endpoint`+`forcePathStyle` S3-compat, hand-composed multipart under a scope, `Redacted` credentials, `Effect.withSpan` spans, paginators lifted to `Stream`
- Reject: imperative SDK use or a missing `abortSignal`, a per-verb method family, a `PreconditionFailed` class or a 412-as-fault, hardcoded AWS endpoints, the `@effect/platform` `HttpClient` for S3 transport, `@aws-sdk/lib-storage` for bounded bytes, the `S3` flat client where the command form fits
