# [@aws-sdk/client-s3] — the S3-compatible object client behind the content-addressed object store and its conditional-put idempotency

`@aws-sdk/client-s3` is the S3 protocol client the `object` plane wraps: one `S3Client` whose single polymorphic `send(command)` discriminates over ~113 command values, tuned to any S3-compatible endpoint (AWS, MinIO, Cloudflare R2, Tigris, Ceph) by `endpoint` + `forcePathStyle`, never a per-verb method family. The store composes it under Effect — the client is `acquireRelease`d, each `send` is an `Effect.tryPromise` with the fiber's `AbortSignal` bridging interruption to SDK cancellation, and the `S3ServiceException` hierarchy maps to a tagged error rail. Content-addressed idempotency is one parameterized pattern: `PutObjectCommand{ Key: contentKey, IfNoneMatch: "*", ChecksumSHA256 }` — the key IS the kernel `ContentKey` digest, the checksum verifies integrity server-side, and a caught `S3ServiceException` with `$metadata.httpStatusCode === 412` is the idempotent noop (there is no `PreconditionFailed` class). The client carries its own `requestHandler` (`@smithy/node-http-handler`), so it does NOT ride the `@effect/platform` `HttpClient` Tag; the Effect seam is the wrap, not the transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-s3`
- package: `@aws-sdk/client-s3`
- version: `3.1078.0`
- license: `Apache-2.0`
- peer: none — self-contained; `@smithy/*` and `@aws-sdk/*` submodules bundled
- transport: own `requestHandler` — `@smithy/node-http-handler` (Node `https.Agent` pool / HTTP2) or `@smithy/fetch-http-handler` (browser); NOT the `@effect/platform` `HttpClient`
- runtime: `runtime:node` (server object plane) and browser (presigned direct-to-S3 uploads); `credentialDefaultProvider` (`@aws-sdk/credential-provider-node`) is node-only
- module format: ESM/CJS dual, `sideEffects: false`, tree-shakeable command imports; deep-import subpaths per command
- admitted companion: `@aws-sdk/lib-storage` (managed `Upload`; `.api/aws-sdk-lib-storage.md`) owns streaming/unknown-length bodies — bounded-bytes multipart stays hand-composed from the low-level command family under an Effect scope

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, its config, and the polymorphic send
- rail: store/object
- One `S3Client` owns the config, credential chain, retry, checksum, and endpoint resolution; `send` is the single dispatch. `S3` is the flat-method convenience subclass (`s3.putObject(input)`) — the command form is the tree-shakeable, Effect-wrappable target.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `S3Client` / `S3Client.send(command, options?)` / `.destroy()` | client          | `object/key`/`presign` — `acquireRelease`d; `send` is the one dispatch     |
|  [02]   | `S3ClientConfig` (`region`/`credentials`/`endpoint`)           | config          | `host/config` — `credentials` `Redacted`; `endpoint`+`forcePathStyle` = S3-compat |
|  [03]   | `S3ClientConfig` (`requestHandler`/`maxAttempts`/`retryMode`)  | transport/retry | `@smithy/node-http-handler` pool tuning; adaptive retry budget             |
|  [04]   | `S3ClientConfig` (`requestChecksumCalculation`/`responseChecksumValidation`/`useDualstackEndpoint`/`useFipsEndpoint`/`useArnRegion`) | integrity/endpoint | default checksum policy (`WHEN_SUPPORTED`/`WHEN_REQUIRED`); dualstack/FIPS/ARN routing |
|  [05]   | `S3` (extends `S3Client`)                                      | flat client     | convenience only; prefer the command form under Effect for tree-shaking    |
|  [06]   | `RuntimeExtension` / `S3ClientConfig.extensions`              | extension       | credential/handler/checksum extension hooks at construction               |

[PUBLIC_TYPE_SCOPE]: the object command rows — the space `send` discriminates
- rail: store/object
- These are the seed rows of the content-addressed plane; the full ~113-command surface is the space one `send` owns. Each command's `*CommandInput` carries the conditional, checksum, encryption, and range fields the store composes.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `PutObjectCommand` (`IfNoneMatch`/`IfMatch`/`ChecksumSHA256`/`ChecksumAlgorithm`/`ContentMD5`/`StorageClass`/`ServerSideEncryption`/`SSECustomerKey`/`Metadata`/`Tagging`) | write | `object/key` conditional-put idempotency; checksum = content-address verify |
|  [02]   | `GetObjectCommand` (`Range`/`PartNumber`/`ChecksumMode`/`IfNoneMatch`/`IfModifiedSince`/`ResponseContentType`) → `GetObjectCommandOutput.Body: StreamingBlobPayloadOutputTypes` | read | ranged/part reads, `ChecksumMode: "ENABLED"` re-verifies on read; the response `Body` (node `SdkStream<Readable>`) is the `sharp` derivative-source stream |
|  [03]   | `HeadObjectCommand` / `GetObjectAttributesCommand`            | metadata        | `object/key` existence + `ETag`/`Checksum`/`ObjectParts` probe without body |
|  [04]   | `DeleteObjectCommand` / `DeleteObjectsCommand` (batch ≤1000)  | delete          | `object/key` reference-sweep GC; batch delete for retention-class sweeps    |
|  [05]   | `CopyObjectCommand` (`CopySourceIfMatch`/`CopySourceIfNoneMatch`) | copy         | server-side content-key rename/rekey; conditional copy                     |
|  [06]   | `ListObjectsV2Command` / `ListObjectVersionsCommand`         | list            | prefix walk for GC/audit; version enumeration                              |
|  [07]   | `PutObjectTaggingCommand` / `GetObjectTaggingCommand`        | tagging         | retention-class + reference-count tags on the object                       |
|  [08]   | `PutBucketLifecycleConfigurationCommand`                     | lifecycle       | `object/key` retention-class GC as a bucket rule set                       |
|  [09]   | `RestoreObjectCommand` / `SelectObjectContentCommand`        | archive/query   | Glacier restore; server-side S3 Select over stored objects                 |

[PUBLIC_TYPE_SCOPE]: multipart, pagination, waiters, and the error rail
- rail: store/object
- Multipart is the low-level command family (no `lib-storage`), composed under an Effect scope that `Abort`s on interrupt. Paginators and waiters are `AsyncIterable`/promise helpers the Effect wrap lifts. The error hierarchy is the tagged rail's source.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `CreateMultipartUploadCommand` / `UploadPartCommand` / `UploadPartCopyCommand` / `CompleteMultipartUploadCommand` (`IfNoneMatch` — the conditional rides COMPLETE; `CreateMultipartUploadRequest` has no conditional member) / `AbortMultipartUploadCommand` / `ListPartsCommand` / `ListMultipartUploadsCommand` | multipart | large-blob ingest, hand-composed; `Abort` on scope interrupt; first-writer-wins lands at completion |
|  [02]   | `paginateListObjectsV2` / `paginateListParts` / `paginateListBuckets` / `paginateListDirectoryBuckets` / `paginateListObjectAnnotations` | paginator | `AsyncIterable` prefix/part walk → `Stream.fromAsyncIterable`             |
|  [03]   | `waitUntilObjectExists` / `waitUntilObjectNotExists` / `waitUntilBucketExists` / `waitUntilBucketNotExists` (`WaiterConfiguration{ client, maxWaitTime }`) | waiter | poll-to-consistency after a write/delete |
|  [04]   | `S3ServiceException` (base)                                    | error base      | the tagged-error mapping source; `$metadata.httpStatusCode` carries 412/404 |
|  [05]   | `NoSuchKey` / `NoSuchBucket` / `NoSuchUpload` / `NotFound`     | tagged fault    | miss classification for `object/key` reads and probes                      |
|  [06]   | `InvalidObjectState` / `InvalidWriteOffset` / `EncryptionTypeMismatch` / `TooManyParts` / `BucketAlreadyOwnedByYou` | tagged fault | archive-state, append-offset, SSE, and multipart faults |
|  [07]   | `StorageClass` / `ChecksumAlgorithm` / `ChecksumMode` / `ServerSideEncryption` / `ObjectCannedACL` | enum vocabulary | bounded policy values on the command inputs                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the Effect wrap — client lifecycle and typed send
- rail: store/object
- The store never touches the SDK imperatively. The client is a scoped resource; every command is a typed `Effect`; the `AbortSignal` bridges Effect interruption; the exception hierarchy folds to a tagged rail.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Effect.acquireRelease(Effect.sync(() => new S3Client(config)), (c) => Effect.sync(() => c.destroy()))` | client layer | `object` `S3Client` as a scoped `Layer.scoped` service   |
|  [02]   | `Effect.tryPromise({ try: (signal) => client.send(command, { abortSignal: signal }), catch: mapS3Error })` | typed send | the one dispatch wrap; interruption → SDK abort          |
|  [03]   | `mapS3Error(e)` → `Match` on `S3ServiceException` name / `$metadata.httpStatusCode`                 | error fold     | `Data.TaggedError` rail; 412 ⇒ idempotent noop, 404 ⇒ miss |
|  [04]   | `Stream.fromAsyncIterable(paginateListObjectsV2({ client }, input), mapS3Error)`                    | paginated read | `object` GC prefix walk as an Effect `Stream`            |
|  [05]   | `Config.redacted("S3_SECRET_ACCESS_KEY")` → `credentials` / `Config.string("S3_ENDPOINT")` → `endpoint` | config | `host/config` — secrets `Redacted`, endpoint/provider parameterized |

[ENTRYPOINT_SCOPE]: content-address idempotency and multipart composition
- rail: store/object
- Conditional put is the idempotency mechanism; multipart is a scoped fold over the low-level commands.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `PutObjectCommand{ Key: contentKey, IfNoneMatch: "*", ChecksumSHA256, ChecksumAlgorithm: "SHA256" }` | conditional put | `object/key` — first-writer wins; digest = key, checksum = integrity |
|  [02]   | `catch` → `$metadata.httpStatusCode === 412` ⇒ `Effect.void` (idempotent noop)                     | idempotency     | the content-address re-put is a proven noop, not a fault |
|  [03]   | `Effect.acquireRelease(CreateMultipartUpload, ({ UploadId }) ⇒ AbortMultipartUpload)` then `UploadPart` fold ⇒ `CompleteMultipartUpload{ IfNoneMatch: "*" }` | multipart | bounded-bytes large-blob ingest; abort on interrupt, conditional at completion; streaming bodies ride `lib-storage` `Upload` |
|  [04]   | `GetObjectCommand{ ChecksumMode: "ENABLED" }` → verify `ChecksumSHA256` against the key             | read verify     | `object/key` end-to-end content-address verification     |
|  [05]   | `PutBucketLifecycleConfigurationCommand{ Rules }` + `PutObjectTaggingCommand`                       | retention GC    | `object/key` reference-sweep GC by retention class       |
|  [06]   | `GetObjectCommandOutput.Body` — node `SdkStream<IncomingMessage \| Readable>`; one-shot `transformToByteArray(): Promise<Uint8Array>` / `transformToWebStream(): ReadableStream` / `transformToString(enc?)` | body read | the `sharp` fan-out source read: `Body.transformToByteArray()` once → `Buffer` → `sharp(buffer).clone()` per derivative; the `Body` is single-consume, so buffer-then-clone, never a re-piped stream per derivative |

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_CLIENT_TOPOLOGY]:
- one polymorphic `send`, commands as rows: the store wraps `client.send(command)` exactly once; the ~113 commands are values that discriminate the operation, so a new object operation is a new command row, never a new method. The `S3` flat client is convenience only.
- content-address idempotency, not a lock: the object `Key` IS the kernel `ContentKey` digest, so two writers producing the same bytes produce the same key. `IfNoneMatch: "*"` makes the first `PutObject` win and every re-put fail with HTTP 412; the store catches the `S3ServiceException`, reads `$metadata.httpStatusCode === 412`, and returns a noop. There is no `PreconditionFailed` error class — 412 is a status on the base exception, and mistaking it for a named class is a phantom.
- checksums are the integrity proof: `ChecksumSHA256` + `ChecksumAlgorithm: "SHA256"` on write and `ChecksumMode: "ENABLED"` on read make S3 verify the content-address digest end to end; `requestChecksumCalculation`/`responseChecksumValidation` set the default policy at the client.
- own transport, Effect is the wrap: the SDK uses its own `requestHandler` (`@smithy/node-http-handler` pool), NOT the `@effect/platform` `HttpClient` — so the Effect seam is the resource wrap and the `AbortSignal` bridge, not the wire. Interruption of the fiber aborts the in-flight request; the client is `destroy`ed on scope close.
- S3-compatible by parameterization: `endpoint` + `forcePathStyle: true` target MinIO/R2/Tigris/Ceph; `credentials` are static keys or a provider; nothing is hardcoded to AWS. The provider is a `Config` fact.
- multipart splits by body shape: bounded bytes ride the low-level `CreateMultipartUpload`/`UploadPart`/`CompleteMultipartUpload` family under `Effect.acquireRelease` that `AbortMultipartUpload`s on interrupt — part size is a parameter, not a library default; a streaming or unknown-length body rides `@aws-sdk/lib-storage`'s `Upload` (`.api/aws-sdk-lib-storage.md`), whose params spread carries the same conditional and checksum members onto both its legs.

[INTEGRATION_LAW]:
- Stack with `effect` (`.api/effect.md`): `Layer.scoped` holds the `S3Client` via `acquireRelease`; `Effect.tryPromise` with `{ abortSignal }` lifts each `send`; the `S3ServiceException` hierarchy maps through `Match` to `Data.TaggedError` (`ObjectMissing`/`ObjectConflict`/`ObjectFault`); `Stream.fromAsyncIterable` lifts a paginator; `Config.redacted` supplies credentials; `Schedule` policy composes with `maxAttempts`/`retryMode`. The client adds no rail — Effect owns lifecycle, cancellation, and error.
- Stack with `@aws-sdk/s3-request-presigner` (`.api/aws-sdk-s3-request-presigner.md`): `getSignedUrl(client, command, { expiresIn })` mints a presigned URL from the SAME `S3Client` + command values, inheriting `credentials`/`region`/`endpoint`/`forcePathStyle` — the `object/presign` browser-direct upload/download rows.
- Stack with `sharp` (`.api/sharp.md`): the `object/presign` codec fan-out collects the `GetObjectCommand` response `Body` (`SdkStream<Readable>`) once via `Body.transformToByteArray()` into a `Buffer`, decodes it in `sharp(buffer)`, then `clone()` + `toFormat(row.format, row.options)` per derivative-spec row (the `Body` is single-consume, so buffer-then-clone, never a re-piped stream per derivative), and writes each derivative back through the SAME conditional-put idempotency row this client owns — `PutObjectCommand{ Key: derivativeContentKey, IfNoneMatch: "*", ChecksumSHA256 }`, 412-by-status ⇒ idempotent noop — so every derivative is content-addressed and re-put-safe exactly like its source. This client owns the fetch and the idempotent write; `sharp` owns the codec.
- Stack with `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): wrap each `send` in `Effect.withSpan("s3.putObject", { attributes: { bucket, key } })`; the span rides the same exporter Layer as the SQL spans, correlating an object write to its journal event.
- Stack with `kernel`/`security`: `object/key` `ObjectKey` = kernel `ContentKey` (the digest that is the S3 Key); `credentials` and `SSECustomerKey` are `Redacted` from `host/config`; a presigned URL is a bounded-TTL capability token `security` reasons about.

[LOCAL_ADMISSION]:
- wrap `send` once under `Effect.tryPromise` with `{ abortSignal }`; never call the SDK imperatively or omit the abort bridge — an un-abortable request leaks past fiber interruption.
- compose commands as values through the one `send`; never build a `get`/`put`/`list` method family, and never reach for the `S3` flat client where the command form tree-shakes.
- detect the conditional-put noop by `$metadata.httpStatusCode === 412` on the caught `S3ServiceException`; never catch a `PreconditionFailed` class (it does not exist) and never treat the 412 as a fault.
- target S3-compatible providers by `endpoint` + `forcePathStyle` as `Config` facts; never hardcode an AWS region/endpoint. Credentials and SSE-C keys are `Redacted`.
- hand-compose bounded-bytes multipart under `Effect.acquireRelease` with `AbortMultipartUpload` on release; route streaming bodies through `@aws-sdk/lib-storage`'s `Upload` with the conditional stated on `params`.

[RAIL_LAW]:
- Package: `@aws-sdk/client-s3`
- Owns: the `S3Client` + `send` dispatch, the object/multipart/list command family, `paginate*` iterables, `waitUntil*` pollers, the `S3ServiceException` hierarchy, the bounded enum vocabularies, and `S3ClientConfig` (endpoint/credentials/checksum/retry/transport)
- Accept: one `Effect`-wrapped `send` discriminated by command value, `IfNoneMatch: "*"` + checksum as the content-address idempotency pattern, 412-by-status noop detection, `endpoint`+`forcePathStyle` S3-compat parameterization, hand-composed multipart under a scope, `Redacted` credentials, spans via `Effect.withSpan`, paginators lifted to `Stream`
- Reject: imperative SDK use or a missing `abortSignal`, a per-verb method family, a `PreconditionFailed` class or a 412-as-fault, hardcoded AWS endpoints/credentials, riding the `@effect/platform` `HttpClient` for S3 transport, `@aws-sdk/lib-storage` reached for bounded bytes, the `S3` flat client where the command form fits
