# [TS_DATA_API_AWS_SDK_CLIENT_S3]

`@aws-sdk/client-s3` drives every S3-compatible endpoint through one `S3Client` whose single polymorphic `send(command)` discriminates the whole command space; `endpoint` + `forcePathStyle` retarget MinIO, R2, Tigris, or Ceph, never a per-verb method. It carries its own `requestHandler`, so under Effect the client is `acquireRelease`d and each `send` is a `tryPromise` bridging the fiber `AbortSignal` to SDK cancellation — the Effect adapter is the wrap, never the transport.

## [01]-[PUBLIC_TYPES]

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
|  [04]   | `DeleteObjectCommand` / `DeleteObjectsCommand` (batch ≤1000) | delete        | per-key `IfMatch` CAS sweep; the batch is REFUSED        |
|  [05]   | `CopyObjectCommand`                                          | copy          | server-side rekey; `CopySourceIf*` conditional           |
|  [06]   | `ListObjectsV2Command` / `ListObjectVersionsCommand`         | list          | prefix walk for GC/audit; version enumeration            |
|  [07]   | `PutObjectTaggingCommand` / `GetObjectTaggingCommand`        | tagging       | retention-class + reference-count tags                   |
|  [08]   | `PutBucketLifecycleConfigurationCommand`                     | lifecycle     | retention-class GC as a bucket rule set                  |
|  [09]   | `RestoreObjectCommand` / `SelectObjectContentCommand`        | archive/query | Glacier restore; the Select row is REFUSED               |

- `PutObjectCommand` input: `IfNoneMatch` `IfMatch` `ChecksumSHA256` `ChecksumAlgorithm` `ContentMD5` `StorageClass` `ServerSideEncryption` `SSECustomerKey` `Metadata` `Tagging`.
- `GetObjectCommand` input: `Range` `PartNumber` `ChecksumMode` `IfNoneMatch` `IfModifiedSince` `ResponseContentType`.
- [REFUSED]: `DeleteObjectsCommand` carries no per-key conditional — one batch expresses no `IfMatch`, so a key whose bytes moved under the listing probe deletes anyway; the CAS law outranks the round-trip saving and every delete is a per-key `DeleteObjectCommand` under the ETag that listing carried.
- [REFUSED]: `SelectObjectContentCommand` cannot read an archived object at all — a restore precedes it, so it answers none of the cold-tier need that alone justifies it, and content-addressed bytes carry no queryable schema for it to project.

[PUBLIC_TYPE_SCOPE]: the bucket-posture and object-lock command rows
- Custody generation reads realized posture off the bucket itself; the lock family is catalogued REFUSED, because every member demands a versioned bucket and this plane writes unversioned by law.

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `GetBucketVersioningCommand` → `Status` / `MFADelete`              | posture       | unversioned proof behind write-once identity        |
|  [02]   | `GetBucketLifecycleConfigurationCommand` → `Rules`                 | posture       | realized retention rules; 404 on a rule-less bucket |
|  [03]   | `GetBucketEncryptionCommand` → `ServerSideEncryptionConfiguration` | posture       | default SSE mode; 404 when none is configured       |
|  [04]   | `PutObjectLockConfigurationCommand` / `Get…`                       | lock REFUSED  | bucket-wide default retention; versioned only       |
|  [05]   | `PutObjectLegalHoldCommand` / `Get…`                               | lock REFUSED  | per-object hold flag; versioned only                |
|  [06]   | `PutObjectRetentionCommand` / `Get…`                               | lock REFUSED  | per-object retain-until; versioned only             |

- `ServerSideEncryptionConfiguration.Rules: ServerSideEncryptionRule[]`; `ServerSideEncryptionRule.ApplyServerSideEncryptionByDefault` / `.BucketKeyEnabled` / `.BlockedEncryptionTypes`; `ServerSideEncryptionByDefault.SSEAlgorithm: ServerSideEncryption` (required) / `.KMSMasterKeyID` — the key id and the bucket-key flag are observation-side alone.
- [ENUM]: `ServerSideEncryption` = `AES256` `aws:kms` `aws:kms:dsse` `aws:backup` `aws:fsx`; `BucketVersioningStatus`; `MFADeleteStatus`.
- [LOCK_SHAPE]: `ObjectLockConfiguration.ObjectLockEnabled` (`ObjectLockEnabled` = `Enabled`) / `.Rule`; `ObjectLockRule.DefaultRetention`; `DefaultRetention.Mode` / `.Days` / `.Years`; `ObjectLockLegalHold.Status` (`ObjectLockLegalHoldStatus` = `ON` `OFF`); `ObjectLockRetention.Mode` (`ObjectLockRetentionMode` = `COMPLIANCE` `GOVERNANCE`) / `.RetainUntilDate: Date`.
- [LOCK_MEMBER]: `PutObjectRequest.ObjectLockMode` (`ObjectLockMode` = `COMPLIANCE` `GOVERNANCE`) / `.ObjectLockRetainUntilDate: Date` / `.ObjectLockLegalHoldStatus` — the write-time lock members, unspellable here for the same versioning reason.
- [REFUSED]: object lock in every form rests on `ObjectLockEnabled`, which a bucket takes only at creation and only with versioning on; the content-addressed plane refuses versioning because the key IS the content, so the whole surface is catalogued and never composed — litigation preservation rides the folder's own hold ledger and its frozen tag posture instead.

[PUBLIC_TYPE_SCOPE]: multipart, pagination, waiters, and the error channel
- Multipart is the low-level command family composed under an Effect scope; paginators and waiters are `AsyncIterable`/promise helpers the wrap lifts; the `S3ServiceException` hierarchy seeds the tagged error channel.
- [MULTIPART]: `CreateMultipartUploadCommand` `UploadPartCommand` `UploadPartCopyCommand` `CompleteMultipartUploadCommand` `AbortMultipartUploadCommand` `ListPartsCommand` `ListMultipartUploadsCommand` — hand-composed large-blob ingest, `Abort` on scope interrupt.
- [MULTIPART_MEMBER]: `CreateMultipartUploadRequest.ChecksumAlgorithm`/`.ChecksumType`; `UploadPartRequest.ChecksumAlgorithm`/`.ChecksumSHA256`; `UploadPartOutput.ChecksumSHA256`; `CompletedPart.ETag`/`.PartNumber`/`.ChecksumSHA256`; `CompleteMultipartUploadRequest.IfNoneMatch`/`.IfMatch`/`.ChecksumType`/`.MpuObjectSize` — `Create` carries no conditional member.
- [PAGINATOR]: `paginateListObjectsV2` `paginateListParts` `paginateListBuckets` `paginateListDirectoryBuckets` `paginateListObjectAnnotations` — `AsyncIterable` → `Stream.fromAsyncIterable`.
- [WAITER]: `waitUntilObjectExists` `waitUntilObjectNotExists` `waitUntilBucketExists` `waitUntilBucketNotExists` — poll-to-consistency after a write/delete; `{ client, maxWaitTime }` bounds the wait.
- [ERROR_BASE]: `S3ServiceException` — the tagged-error mapping source; `$metadata.httpStatusCode` carries 412/404.
- [TAGGED_FAULT]: `NoSuchKey` `NoSuchBucket` `NoSuchUpload` `NotFound` `InvalidObjectState` `InvalidWriteOffset` `EncryptionTypeMismatch` `TooManyParts` `BucketAlreadyOwnedByYou` — miss, archive-state, append-offset, SSE, and multipart fault classification.
- [ENUM]: `StorageClass` `ChecksumAlgorithm` `ChecksumMode` `ServerSideEncryption` `ObjectCannedACL` — bounded policy values on command inputs.

## [02]-[ENTRYPOINTS]

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
|  [04]   | `UploadPart` fold ⇒ `CompleteMultipartUpload{ IfNoneMatch: "*" }`       | multipart       | conditional at complete; per-part digests  |
|  [05]   | `GetObjectCommand{ ChecksumMode: "ENABLED" }`                           | read verify     | verify `ChecksumSHA256` against the key    |
|  [06]   | `PutBucketLifecycleConfigurationCommand{ Rules }` + `PutObjectTagging`  | retention GC    | reference-sweep GC by retention class      |
|  [07]   | `GetObjectCommandOutput.Body`                                           | body read       | node `SdkStream<Readable>`, single-consume |

- `GetObjectCommandOutput.Body`: `transformToByteArray()` / `transformToWebStream()` / `transformToString(enc?)`; single-consume, so buffer once then `sharp(buffer).clone()` per derivative, never a re-piped stream.
- Multipart checksum chain — the producer behind the end-to-end integrity claim: `CreateMultipartUpload{ ChecksumAlgorithm: "SHA256" }` declares the algorithm, every `UploadPart` asserts the same value, each `UploadPartOutput.ChecksumSHA256` crosses into its `CompletedPart.ChecksumSHA256`, and `CompleteMultipartUpload` re-verifies the assembly; `ChecksumType` (`COMPOSITE`/`FULL_OBJECT`) names which shape the assembled digest carries.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one polymorphic `send`, commands as rows: the store wraps `client.send(command)` once, so a new object operation is a new command row, never a method — the `S3` flat client is convenience only.
- content-address idempotency, not a lock: the object `Key` IS the kernel `ContentKey` digest, so identical bytes produce identical keys; `IfNoneMatch: "*"` makes the first `PutObject` win and every re-put fail HTTP 412, caught on the `S3ServiceException` at `$metadata.httpStatusCode === 412` and returned a noop. No `PreconditionFailed` class exists — reading 412 as a named class is a phantom.
- checksums are the integrity proof: `ChecksumSHA256` + `ChecksumAlgorithm: "SHA256"` on write and `ChecksumMode: "ENABLED"` on read verify the digest end to end; `requestChecksumCalculation`/`responseChecksumValidation` set the client default policy.
- own transport, Effect is the wrap: interruption of the fiber aborts the in-flight request through the `AbortSignal`, and the client `destroy`s on scope close; no `@effect/platform` `HttpClient` rides the S3 wire.
- multipart splits by body shape: bounded bytes ride `CreateMultipartUpload`/`UploadPart`/`CompleteMultipartUpload` under `Effect.acquireRelease` that `AbortMultipartUpload`s on interrupt, `IfNoneMatch` riding COMPLETE since `CreateMultipartUploadRequest` carries no conditional member; part size is a parameter, and a streaming or unknown-length body rides `@aws-sdk/lib-storage`'s `Upload`.

[STACKING]:
- `effect`(`.api/effect.md`): `Layer.scoped` holds the `S3Client` via `acquireRelease`; `Effect.tryPromise` with `{ abortSignal }` lifts each `send`; the `S3ServiceException` hierarchy maps through `Match` to `Data.TaggedError` (`ObjectMissing`/`ObjectConflict`/`ObjectFault`); `Stream.fromAsyncIterable` lifts a paginator; `Config.redacted` supplies credentials; `Schedule` composes with `maxAttempts`/`retryMode`.
- `@aws-sdk/lib-storage`(`.api/aws-sdk-lib-storage.md`): a streaming or unknown-length body rides `Upload`, which spreads the same `IfNoneMatch: "*"` + checksum members across its put and multipart-complete legs; bounded bytes stay on this client's hand-composed multipart.
- `@aws-sdk/s3-request-presigner`(`.api/aws-sdk-s3-request-presigner.md`): `getSignedUrl(client, command, { expiresIn })` mints a presigned URL from the SAME client + command, inheriting `credentials`/`region`/`endpoint`/`forcePathStyle` — the `object/store` `[06]-[GRANT_MINT]` browser-direct rows.
- `sharp`(`.api/sharp.md`): the codec fan-out reads the `GetObjectCommand` `Body` once via `transformToByteArray()` into a `Buffer`, `sharp(buffer).clone()` per derivative-spec row, and writes each back through this client's conditional-put row (`PutObjectCommand{ Key: derivativeContentKey, IfNoneMatch: "*", ChecksumSHA256 }`, 412 ⇒ noop) — every derivative content-addressed like its source.
- `@effect/opentelemetry`: `Effect.withSpan("s3.putObject", { attributes: { bucket, key } })` wraps each `send` on the same exporter Layer as the SQL spans, correlating an object write to its journal event.
- `kernel`/`security`: `ObjectKey` = kernel `ContentKey` (the digest that is the S3 Key); `credentials` and `SSECustomerKey` are `Redacted` from the composition root's `Config`; a presigned URL is a bounded-TTL capability token `security` reasons about.

[LOCAL_ADMISSION]:
- target any S3-compatible provider by `endpoint` + `forcePathStyle` as `Config` facts; `credentials` and SSE-C keys stay `Redacted`, never a hardcoded AWS region.
- size `partSize`/`queueSize` from `Config`, never call-site literals; release hand-composed multipart through `AbortMultipartUpload` on interrupt.
- wrap every `send` once under `Effect.tryPromise` with `{ abortSignal }`; an un-abortable request that leaks past fiber interruption is the rejected form.
