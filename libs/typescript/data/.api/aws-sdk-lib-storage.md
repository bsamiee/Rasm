# [TS_DATA_API_AWS_SDK_LIB_STORAGE]

`@aws-sdk/lib-storage` ships one class: `Upload` takes the object plane's live `S3Client` plus a `params` record and moves a body of unknown or streaming length — below the part threshold it issues one `PutObject`, above it it creates a multipart upload, fans `UploadPart` calls across a `queueSize`-wide queue at `partSize` bytes each, and completes atomically. The load-bearing verified fact: `params` is typed `PutObjectCommandInput & Partial<CreateMultipartUploadCommandInput & UploadPartCommandInput & CompleteMultipartUploadCommandInput>` and the implementation spreads `...params` into the `PutObjectCommand`, the `CreateMultipartUploadCommand`, and the `CompleteMultipartUploadCommand` alike — so `IfNoneMatch: "*"` on `params` rides the single-shot put AND the multipart complete, and the content-addressed 412-noop algebra holds unchanged for a streaming body the hand-composed part fold cannot serve without buffering. `abort()` (or an injected `abortController`) tears down in flight and issues `AbortMultipartUpload`; `httpUploadProgress` events carry per-part progress.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/lib-storage`
- package: `@aws-sdk/lib-storage`
- license: `Apache-2.0`
- peer: `@aws-sdk/client-s catalog` (the client and command inputs it composes; `.api/aws-sdk-client-s3.md`)
- module format: ESM/CJS dual (`dist-es`/`dist-cjs`/`dist-types`), `sideEffects: false`
- runtime: node and browser runtime configs ship; the data plane composes it server-side
- rail: the streaming-put arm of `object/store` and the finalize re-home of `object/stream`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the upload, its options, and progress
- rail: object/store

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:------------- |:------------------------------------------------------------------------- |
| [01] | `Upload` (`extends EventEmitter`, `constructor(options: Options)`) | uploader | one instance per streaming put; scoped, aborted on interruption |
| [02] | `Options.client: S3Client` / `Options.params` | input | the object plane's one client; `params` spreads into every leg — `Bucket`/`Key`/`Body`/`IfNoneMatch`/`ChecksumAlgorithm`/`ContentType`/`Metadata` ride here |
| [03] | `Options.partSize` / `.queueSize` (`Configuration`) | throughput | part bytes (5 MiB floor) and parallel-part width; memory ceiling ≈ `queueSize * partSize` |
| [04] | `Options.leavePartsOnError` / `.tags` / `.abortController` | policy | abort-versus-keep on failure; post-complete `PutObjectTagging`; external abort injection |
| [05] | `BodyDataTypes` (`PutObjectCommandInput["Body"]`) | body union | `Uint8Array`/`Buffer`/string/`Readable`/`ReadableStream`/`Blob` — the streaming ingress shapes |
| [06] | `Progress` (`{ loaded?, total?, part?, Key?, Bucket? }`) | event payload | `httpUploadProgress` — per-part transfer evidence |
| [07] | `Upload.uploadId?` | evidence | the multipart `UploadId` when the multipart path engaged |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the streaming conditional put under Effect
- rail: object/store

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `new Upload({ client, params: { Bucket, Key: contentKey, Body, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" }, partSize, queueSize })` | construct | the streaming conditional put — 412 on either leg is the idempotent noop |
| [02] | `upload.done(): Promise<CompleteMultipartUploadCommandOutput>` | run | one `Effect.tryPromise` per upload; the caught 412 folds to `written: false` |
| [03] | `upload.abort(): Promise<void>` / `Options.abortController` | teardown | `Effect.acquireRelease`/`onInterrupt` bridges fiber interruption to `AbortMultipartUpload` |
| [04] | `upload.on("httpUploadProgress", (progress) => ...)` | progress | transfer evidence lifted onto the rail's telemetry, never domain state |

## [04]-[IMPLEMENTATION_LAW]

[UPLOAD_TOPOLOGY]:
- one entry, size-adaptive: below the part threshold `done()` issues a single `PutObject`; above it the multipart path engages — the caller never selects, so the streaming put has no small/large twin.
- params spread everywhere is the conditional guarantee: the implementation builds the complete call as `{ ...params, Body: undefined, UploadId, MultipartUpload }` and the single-shot as `PutObjectCommand(params)` — `IfNoneMatch`, checksum members, content type, and metadata state once and hold on both paths.
- memory is bounded by policy: at most `queueSize * partSize` bytes buffer at once, so an unbounded `Readable`/`ReadableStream` body moves in constant memory.
- checksum defaults hold: absent an explicit `ChecksumAlgorithm` the multipart create pins CRC32; the content-addressed rows state `"SHA256"` explicitly so write-side verification matches the read-side `ChecksumMode` posture.
- abort is transactional: `abort()` (or the injected controller) interrupts in-flight parts and issues `AbortMultipartUpload`, so an interrupted streaming put leaves no orphaned parts when `leavePartsOnError` stays false.

[INTEGRATION_LAW]:
- Stack with `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`): the same scoped client, the same provider facts, the same 412-by-status detection on the caught `S3ServiceException` — `Upload` is the streaming-body arm beside the client's hand-composed bounded-bytes multipart, not a second client or a second idempotency vocabulary.
- Stack with `effect`: construct inside the put effect, run `done()` through `Effect.tryPromise`, bridge interruption through `Effect.onInterrupt(() => Effect.promise(() => upload.abort()))` or the injected `abortController`; progress events lift to span annotations.
- Stack with `object/stream`: the finalize re-home streams the staged tus object (`S3Store.read`) into `Upload` with `Key: contentKey` and `IfNoneMatch: "*"` — the one leg that moves staging bytes onto the content band without materializing them.
- Stack with `sharp`/derivatives: derivative buffers are bounded and ride the plain conditional `PutObjectCommand`; `Upload` is earned only by streaming or unknown-length bodies.

[LOCAL_ADMISSION]:
- reach for `Upload` only when the body streams or its length is unknown; bounded bytes ride the client's plain conditional put.
- always state `IfNoneMatch: "*"` and `ChecksumAlgorithm` on `params` for content-band writes; never a bare streaming put onto a content key.
- always bridge interruption to `abort()`; never let a fiber die with parts in flight.
- size `partSize`/`queueSize` from `Config` facts; never literals at call sites.

[RAIL_LAW]:
- Package: `@aws-sdk/lib-storage`
- Owns: the managed streaming upload — single-shot/multipart auto-selection, params spread across every leg, bounded part queueing, abort teardown, progress events
- Accept: streaming conditional puts with `IfNoneMatch: "*"` + checksum on `params`, interruption-bridged `abort`, `Config`-sourced part policy, progress as telemetry
- Reject: `Upload` for bounded bytes the plain put serves, a content-band write without the conditional, an unbridged abort, part policy literals, a second client for uploads
