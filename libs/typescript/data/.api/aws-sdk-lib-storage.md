# [TS_DATA_API_AWS_SDK_LIB_STORAGE]

`@aws-sdk/lib-storage` ships one class, `Upload`, moving a streaming or unknown-length body against the object plane's `S3Client`: one `PutObject` below the part threshold, an auto-engaged multipart above it fanning `UploadPart` across a `queueSize`-wide queue at `partSize` bytes. `params` spreads into all four legs — put, create, part, complete — so `IfNoneMatch: "*"` and `ChecksumAlgorithm` state once and hold everywhere, and the content-addressed 412-noop holds for a body the hand-composed part fold cannot serve without buffering.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Upload` class (extends `EventEmitter`), its options, its body union, and its progress payload

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CONSUMER]                                                      |
| :-----: | :-------------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Upload`                                            | uploader      | one instance per streaming put; interruption-aborted            |
|  [02]   | `Options.client: S3Client` / `Options.params`       | input         | the one client; `params` the spread-everywhere input            |
|  [03]   | `Options.partSize` / `.queueSize` (`Configuration`) | throughput    | 5 MiB floor bytes, parallel width; mem `queueSize*partSize`     |
|  [04]   | `Options.leavePartsOnError`                         | policy        | abort-vs-keep on failure teardown                               |
|  [05]   | `Options.tags`                                      | policy        | post-complete `PutObjectTagging`                                |
|  [06]   | `Options.abortController`                           | policy        | adopted interrupt bridge; `.abort()` returns void               |
|  [07]   | `BodyDataTypes` (`PutObjectCommandInput["Body"]`)   | body union    | `Uint8Array`/`Buffer`/string/`Readable`/`ReadableStream`/`Blob` |
|  [08]   | `Progress`                                          | event payload | `{ loaded?, total?, part?, Key?, Bucket? }`                     |
|  [09]   | `Upload.uploadId?`                                  | evidence      | the multipart `UploadId` when the multipart path engaged        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the streaming conditional put under Effect

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new Upload({ client, params, partSize, queueSize })`          | ctor     | the streaming conditional put; 412 either leg noops  |
|  [02]   | `upload.done(): Promise<CompleteMultipartUploadCommandOutput>` | instance | one `Effect.tryPromise`; 412 → `written: false`      |
|  [03]   | `Options.abortController` / `upload.abort(): Promise<void>`    | ctor     | interrupt trips the signal; `done()` sends the abort |
|  [04]   | `upload.on("httpUploadProgress", (progress) => ...)`           | instance | transfer evidence onto telemetry, never domain state |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one size-adaptive entry: `done()` issues a single `PutObject` below the part threshold and engages multipart above it, so the caller selects nothing and the streaming put has no small/large twin.
- params-spread is the conditional guarantee: the complete call builds as `{ ...params, Body: undefined, UploadId, MultipartUpload }` and the single-shot as `PutObjectCommand(params)`, so `IfNoneMatch`, checksum, content type, and metadata state once and hold on both paths.
- at most `queueSize * partSize` bytes buffer at once, so an unbounded `Readable`/`ReadableStream` body moves in constant memory.
- `ChecksumAlgorithm` left absent pins CRC32 at the multipart create under `requestChecksumCalculation: "WHEN_SUPPORTED"` alone, so a client at `"WHEN_REQUIRED"` sends none; stating `"SHA256"` on `params` reaches create and every part, and each part reply's `ChecksumSHA256` lands in its own `CompletedPart` so the engine re-verifies the assembly.
- interruption is signal-only: `abort()` trips the `AbortController` and resolves, while `done()` observes the aborted signal, issues `AbortMultipartUpload` where `leavePartsOnError` stays false, and rejects `AbortError` — the teardown call and its rejection both ride the `done()` promise.

[STACKING]:
- `@aws-sdk/client-s3`(`.api/aws-sdk-client-s3.md`): the same scoped client, the same 412-by-status detection on the caught `S3ServiceException`; `Upload` is the streaming-body arm beside the client's hand-composed bounded-bytes multipart, not a second client or idempotency vocabulary.
- `effect`(`.api/effect.md`): construct in the put effect and run `done()` through `Effect.tryPromise`; interruption forwards that call's `AbortSignal` onto an injected `Options.abortController` whose `.abort()` returns void, so no second promise leaves the fiber; `httpUploadProgress` lifts to span annotations.
- `object/stream`: `S3Store.read(id)` (`.api/tus-s3-store.md`) streams into `new Upload({ params: { Key: contentKey, IfNoneMatch: "*" } })`, the finalize fold moving staging bytes onto the content band without materializing them.

[LOCAL_ADMISSION]:
- `Upload` admits a streaming or unknown-length body only; bounded bytes ride the client's plain conditional put.
- content-band writes state `IfNoneMatch: "*"` and `ChecksumAlgorithm` on `params`.
- interruption bridges through `Options.abortController` at `object/store`'s `_putStreaming`, so no fiber dies with parts in flight.
- `Effect.promise` over any `Upload` promise is refused: a rejection it converts becomes a defect carrying no `class`, which `Fault.Class.of` grades `defect` and `Fault.Budget.schedule`'s default `retryable` gate then reads as non-retryable.
- `partSize`/`queueSize` source from `Config`, never call-site literals.
