# [TS_DATA_API_AWS_SDK_LIB_STORAGE]

`@aws-sdk/lib-storage` ships one class, `Upload`, moving a streaming or unknown-length body against the object plane's `S3Client`: one `PutObject` below the part threshold, an auto-engaged multipart above it fanning `UploadPart` across a `queueSize`-wide queue at `partSize` bytes. `params` spreads into the put, create, and complete legs alike, so one `IfNoneMatch: "*"` rides every leg and the content-addressed 412-noop holds for a body the hand-composed part fold cannot serve without buffering.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/lib-storage`
- package: `@aws-sdk/lib-storage` (Apache-2.0)
- peer: `@aws-sdk/client-s3` (the client and command inputs it composes; `.api/aws-sdk-client-s3.md`)
- module: ESM/CJS dual, `sideEffects: false`
- runtime: node and browser configs ship; the data plane composes it server-side
- rail: the streaming-put arm of `object/store`, the finalize re-home of `object/stream`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Upload` class (extends `EventEmitter`), its options, its body union, and its progress payload

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CONSUMER]                                                      |
| :-----: | :-------------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Upload`                                            | uploader      | one instance per streaming put; interruption-aborted            |
|  [02]   | `Options.client: S3Client` / `Options.params`       | input         | the one client; `params` the spread-everywhere input            |
|  [03]   | `Options.partSize` / `.queueSize` (`Configuration`) | throughput    | 5 MiB floor bytes, parallel width; mem `queueSize*partSize`     |
|  [04]   | `Options.leavePartsOnError`                         | policy        | abort-vs-keep on failure teardown                               |
|  [05]   | `Options.tags`                                      | policy        | post-complete `PutObjectTagging`                                |
|  [06]   | `Options.abortController`                           | policy        | external abort injection                                        |
|  [07]   | `BodyDataTypes` (`PutObjectCommandInput["Body"]`)   | body union    | `Uint8Array`/`Buffer`/string/`Readable`/`ReadableStream`/`Blob` |
|  [08]   | `Progress`                                          | event payload | `{ loaded?, total?, part?, Key?, Bucket? }`                     |
|  [09]   | `Upload.uploadId?`                                  | evidence      | the multipart `UploadId` when the multipart path engaged        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the streaming conditional put under Effect

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new Upload({ client, params, partSize, queueSize })`          | ctor     | the streaming conditional put; 412 either leg noops  |
|  [02]   | `upload.done(): Promise<CompleteMultipartUploadCommandOutput>` | instance | one `Effect.tryPromise`; 412 → `written: false`      |
|  [03]   | `upload.abort(): Promise<void>` / `Options.abortController`    | instance | bridges fiber interruption to `AbortMultipartUpload` |
|  [04]   | `upload.on("httpUploadProgress", (progress) => ...)`           | instance | transfer evidence onto telemetry, never domain state |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one size-adaptive entry: `done()` issues a single `PutObject` below the part threshold and engages multipart above it, so the caller selects nothing and the streaming put has no small/large twin.
- params-spread is the conditional guarantee: the complete call builds as `{ ...params, Body: undefined, UploadId, MultipartUpload }` and the single-shot as `PutObjectCommand(params)`, so `IfNoneMatch`, checksum, content type, and metadata state once and hold on both paths.
- at most `queueSize * partSize` bytes buffer at once, so an unbounded `Readable`/`ReadableStream` body moves in constant memory.
- absent an explicit `ChecksumAlgorithm` the multipart create pins CRC32; a content-band write states `"SHA256"` so write-side verification matches the read-side `ChecksumMode`.
- `abort()` interrupts in-flight parts and issues `AbortMultipartUpload`, leaving no orphaned parts when `leavePartsOnError` stays false.

[STACKING]:
- `@aws-sdk/client-s3`(`.api/aws-sdk-client-s3.md`): the same scoped client, the same 412-by-status detection on the caught `S3ServiceException`; `Upload` is the streaming-body arm beside the client's hand-composed bounded-bytes multipart, not a second client or idempotency vocabulary.
- `effect`(`.api/effect.md`): construct in the put effect, run `done()` through `Effect.tryPromise`, bridge interruption through `Effect.onInterrupt(() => Effect.promise(() => upload.abort()))` or `Options.abortController`; `httpUploadProgress` lifts to span annotations.
- `object/stream`: `S3Store.read(id)` (`.api/tus-s3-store.md`) streams into `new Upload({ params: { Key: contentKey, IfNoneMatch: "*" } })`, the finalize fold moving staging bytes onto the content band without materializing them.

[LOCAL_ADMISSION]:
- `Upload` admits a streaming or unknown-length body only; bounded bytes ride the client's plain conditional put.
- a content-band write states `IfNoneMatch: "*"` and `ChecksumAlgorithm` on `params`.
- interruption bridges to `abort()`, so no fiber dies with parts in flight.
- `partSize`/`queueSize` source from `Config`, never call-site literals.

[RAIL_LAW]:
- Package: `@aws-sdk/lib-storage`
- Owns: the managed streaming upload — single-shot/multipart auto-selection, params spread across every leg, bounded part queueing, abort teardown, progress events
- Accept: streaming conditional puts stating `IfNoneMatch: "*"` + checksum on `params`, interruption-bridged `abort`, `Config`-sourced part policy, progress as telemetry
- Reject: `Upload` for bounded bytes the plain put serves, a content-band write without the conditional, an unbridged abort, part-policy literals, a second client for uploads
