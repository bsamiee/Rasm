# [STORE_KEY]

The content-addressed object plane: the object key IS the kernel `ContentKey` — this page is a delegating mint site, never a second hash — and the S3 `Key` string is its `:x32` spelling, so two writers producing the same bytes produce the same object and the conditional put makes the second writer a proven noop. One `ObjectStore` service owns the scoped `S3Client`, the single `send` wrap with abort-bridged interruption, the tagged fault fold, the size-discriminated put (simple or hand-composed multipart under one entry), the checksum-verified get, and the lifecycle rows — a SQL reference ledger driving reference-sweep GC by retention class, batch deletes, and the bucket lifecycle configuration as data. Endpoint, credentials, and provider are `Config` facts (`forcePathStyle` targets MinIO/R2/Tigris unchanged); the SDK carries its own transport, so the Effect seam is the resource wrap and the abort bridge, never the wire.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                        |
| :-----: | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `CLIENT_SEAM`   | the scoped client, the one typed `send`, the fault fold, the config owner        |
|  [02]   | `CONTENT_STORE` | put/get/head/delete/list — conditional-put idempotency, integrity, multipart     |
|  [03]   | `LIFECYCLE`     | the reference ledger, reference-sweep GC by retention class, lifecycle rules     |

## [2]-[CLIENT_SEAM]

- Owner: the `ObjectStore` service construction — `Effect.acquireRelease` around `new S3Client(config)` with `destroy` on release, one `_send` wrap, one `_folded` error fold, one `_Setting` config owner.
- Packages: `@aws-sdk/client-s3` (`S3Client`, `S3ServiceException`); `effect` (`Config`, `Redacted`, `Match`, `Data`).
- Entry: every S3 operation in the folder is `_send(command)` — commands are values discriminating the operation; a per-verb method family and the flat `S3` client are the rejected forms.
- Growth: a new object operation is a new command value through the same `send`; a new provider is a `Config` change, never a code path.
- Law: the abort bridge is mandatory — `Effect.tryPromise({ try: (signal) => client.send(command, { abortSignal: signal }), catch: _folded })` — fiber interruption aborts the in-flight request; an un-abortable send leaks past interruption.
- Law: the fault family is `ObjectFault` with reasons `missing` (404/`NoSuchKey`), `integrity` (identity or checksum disagreement), `io` (everything else), and a policy table carrying retry verdicts — 412 is NOT a fault: the conditional-put path folds it to the idempotent-noop receipt before this family is consulted, and there is no `PreconditionFailed` class to catch (the status rides `$metadata.httpStatusCode` on the base exception).
- Law: checksums are transport integrity, `ContentKey` is identity — the client sets `requestChecksumCalculation`/`responseChecksumValidation` to `"WHEN_SUPPORTED"` and stamps `ChecksumAlgorithm: "SHA256"` per put so the provider verifies bytes end-to-end; identity verification is the kernel re-mint at read, and the two never substitute for each other.

```typescript
import { Config, Data, Duration, Effect, Match, Redacted } from "effect"
import { S3Client, S3ServiceException } from "@aws-sdk/client-s3"

declare namespace ObjectStore {
  type Reason = "missing" | "integrity" | "io"
}

const _Policy = {
  missing: { retry: false },
  integrity: { retry: false },
  io: { retry: true },
} as const

class _Fault extends Data.TaggedError("ObjectFault")<{
  readonly reason: ObjectStore.Reason
  readonly key: string
  readonly detail: string
}> {
  get policy(): (typeof _Policy)[ObjectStore.Reason] {
    return _Policy[this.reason]
  }
}

const _Setting = Config.unwrap({
  endpoint: Config.string("OBJECT_ENDPOINT"),
  region: Config.string("OBJECT_REGION").pipe(Config.withDefault("auto")),
  bucket: Config.string("OBJECT_BUCKET"),
  forcePathStyle: Config.boolean("OBJECT_PATH_STYLE").pipe(Config.withDefault(true)),
  accessKeyId: Config.redacted("OBJECT_ACCESS_KEY_ID"),
  secretAccessKey: Config.redacted("OBJECT_SECRET_ACCESS_KEY"),
  maxAttempts: Config.integer("OBJECT_MAX_ATTEMPTS").pipe(Config.withDefault(3)),
  multipartThreshold: Config.integer("OBJECT_MULTIPART_BYTES").pipe(Config.withDefault(64 * 1024 * 1024)),
  presignTtl: Config.duration("OBJECT_PRESIGN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
})

class _Replay extends Data.TaggedError("ObjectReplay")<{ readonly key: string }> {}

const _folded = (key: string) => (caught: unknown): _Fault | _Replay =>
  Match.value(caught).pipe(
    Match.when(Match.instanceOf(S3ServiceException), (fault) =>
      fault.$metadata.httpStatusCode === 412
        ? new _Replay({ key })
        : fault.$metadata.httpStatusCode === 404
          ? new _Fault({ reason: "missing", key, detail: fault.name })
          : new _Fault({ reason: "io", key, detail: fault.name })),
    Match.orElse((residue) => new _Fault({ reason: "io", key, detail: String(residue) })),
  )
```

## [3]-[CONTENT_STORE]

- Owner: the `ObjectStore` service surface — `conditional` (the ONE conditional-put command mint the server put and the presign page share), `put` discriminating simple versus multipart by size, `get` with identity verification, `head`, `remove` in ≤1000 batches, `list` as a paginator-lifted `Stream`.
- Packages: `@aws-sdk/client-s3` (command classes, `paginateListObjectsV2`); `effect` (`Chunk`, `Stream`, `Option`); `@rasm/ts/kernel` (`ContentKey` — the delegating mint).
- Entry: `ObjectStore.put(bytes, options?)` mints the key from the bytes through the kernel and writes conditionally — the caller never supplies a key, because identity is derived, not asserted; `get(key)` returns verified bytes; the receipt says what happened.
- Receipt: `ObjectStore.Receipt` — `{ key, bytes, written }` — `written: false` is the 412 idempotent noop, a success by law.
- Growth: a new write posture (storage class, SSE) is a field on `put`'s options row flowing into the command value; a new read shape is a command field, never a sibling get.
- Law: the 412 fold is status-read on the caught base exception — `$metadata.httpStatusCode === 412` folds to `{ written: false }` before the fault family engages; treating 412 as a fault is the named defect.
- Law: multipart is composed, not managed — `Effect.acquireRelease(CreateMultipartUpload, abort-on-failure)` with an `UploadPart` fold over `Chunk.chunksOf` windows and a terminal `CompleteMultipartUpload`; `@aws-sdk/lib-storage` stays unadmitted, and part size is a `Config` fact.
- Law: `get` verifies identity — bytes re-mint through the kernel and disagree only as `integrity`; `ChecksumMode: "ENABLED"` rides the read so the provider's transport verification runs too.
- Boundary: the kernel mint is the ONE hash — invariant 2's third delegating site; the presign page consumes `conditional` for browser-direct upload grants so the idempotency headers exist exactly once.

```typescript
import { Chunk, Effect, Exit, Option, Stream } from "effect"
import {
  AbortMultipartUploadCommand, CompleteMultipartUploadCommand, CreateMultipartUploadCommand,
  DeleteObjectsCommand, GetObjectCommand, HeadObjectCommand, ListObjectsV2Command,
  PutObjectCommand, UploadPartCommand, paginateListObjectsV2,
} from "@aws-sdk/client-s3"
import { ContentKey } from "@rasm/ts/kernel"
import type { Retain } from "../journal/retain.ts"

const _multipart = (
  client: S3Client,
  bucket: string,
  key: ContentKey,
  bytes: Uint8Array,
  partBytes: number,
): Effect.Effect<void, _Fault | _Replay> =>
  Effect.scoped(
    Effect.gen(function* () {
      const opened = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: (signal) =>
            client.send(new CreateMultipartUploadCommand({ Bucket: bucket, Key: key, IfNoneMatch: "*" }), {
              abortSignal: signal,
            }),
          catch: _folded(key),
        }),
        (held, exit) =>
          Exit.isFailure(exit)
            ? Effect.ignore(Effect.tryPromise({
                try: () =>
                  client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: held.UploadId })),
                catch: _folded(key),
              }))
            : Effect.void,
      )
      const windows = Chunk.toReadonlyArray(Chunk.chunksOf(Chunk.fromIterable(bytes), partBytes))
      const parts = yield* Effect.forEach(windows, (window, index) =>
        Effect.map(
          Effect.tryPromise({
            try: (signal) =>
              client.send(new UploadPartCommand({
                Bucket: bucket,
                Key: key,
                UploadId: opened.UploadId,
                PartNumber: index + 1,
                Body: Uint8Array.from(Chunk.toReadonlyArray(window)),
              }), { abortSignal: signal }),
            catch: _folded(key),
          }),
          (reply) => ({ ETag: reply.ETag, PartNumber: index + 1 }),
        ), { concurrency: 4 })
      yield* Effect.tryPromise({
        try: (signal) =>
          client.send(new CompleteMultipartUploadCommand({
            Bucket: bucket,
            Key: key,
            UploadId: opened.UploadId,
            MultipartUpload: { Parts: parts },
          }), { abortSignal: signal }),
        catch: _folded(key),
      })
    }),
  )

declare namespace ObjectStore {
  type PutOptions = {
    readonly contentType?: string
    readonly retention?: Retain.Class
  }
  type Receipt = {
    readonly key: ContentKey
    readonly bytes: number
    readonly written: boolean
  }
  type Head = {
    readonly key: ContentKey
    readonly bytes: number
    readonly etag: string
  }
}

class ObjectStore extends Effect.Service<ObjectStore>()("store/ObjectStore", {
  scoped: Effect.gen(function* () {
    const setting = yield* _Setting
    const client = yield* Effect.acquireRelease(
      Effect.sync(() =>
        new S3Client({
          endpoint: setting.endpoint,
          region: setting.region,
          forcePathStyle: setting.forcePathStyle,
          credentials: {
            accessKeyId: Redacted.value(setting.accessKeyId),
            secretAccessKey: Redacted.value(setting.secretAccessKey),
          },
          maxAttempts: setting.maxAttempts,
          requestChecksumCalculation: "WHEN_SUPPORTED",
          responseChecksumValidation: "WHEN_SUPPORTED",
        })),
      (held) => Effect.sync(() => held.destroy()),
    )
    const send = <A>(key: string, run: (signal: AbortSignal) => Promise<A>): Effect.Effect<A, _Fault | _Replay> =>
      Effect.tryPromise({ try: run, catch: _folded(key) })
    const conditional = (key: ContentKey, options?: ObjectStore.PutOptions, body?: Uint8Array) =>
      new PutObjectCommand({
        Bucket: setting.bucket,
        Key: key,
        IfNoneMatch: "*",
        ChecksumAlgorithm: "SHA256",
        ContentType: options?.contentType,
        Tagging: options?.retention === undefined ? undefined : `retention=${options.retention}`,
        Body: body,
      })
    const put = (bytes: Uint8Array, options?: ObjectStore.PutOptions) =>
      Effect.gen(function* () {
        const key = yield* ContentKey.mint(bytes)
        const write = bytes.byteLength > setting.multipartThreshold
          ? _multipart(client, setting.bucket, key, bytes, setting.multipartThreshold)
          : send(key, (signal) => client.send(conditional(key, options, bytes), { abortSignal: signal }))
        return yield* write.pipe(
          Effect.as({ key, bytes: bytes.byteLength, written: true } satisfies ObjectStore.Receipt),
          Effect.catchTag("ObjectReplay", () =>
            Effect.succeed({ key, bytes: bytes.byteLength, written: false } satisfies ObjectStore.Receipt)),
        )
      })
    const get = (key: ContentKey) =>
      Effect.gen(function* () {
        const reply = yield* send(key, (signal) =>
          client.send(new GetObjectCommand({ Bucket: setting.bucket, Key: key, ChecksumMode: "ENABLED" }), {
            abortSignal: signal,
          }))
        const bytes = yield* Effect.tryPromise({
          try: () => reply.Body!.transformToByteArray(),
          catch: _folded(key),
        })
        const minted = yield* ContentKey.mint(bytes)
        return yield* minted === key
          ? Effect.succeed(bytes)
          : Effect.fail(new _Fault({ reason: "integrity", key, detail: minted }))
      })
    return {
      client,
      bucket: setting.bucket,
      presignTtl: setting.presignTtl,
      conditional,
      put,
      get,
      head: (key: ContentKey) =>
        send(key, (signal) =>
          client.send(new HeadObjectCommand({ Bucket: setting.bucket, Key: key }), { abortSignal: signal }),
        ).pipe(
          Effect.map((reply) =>
            Option.some({ key, bytes: Number(reply.ContentLength ?? 0), etag: String(reply.ETag ?? "") })),
          Effect.catchIf((fault): fault is _Fault => fault.reason === "missing", () => Effect.succeed(Option.none())),
        ),
      remove: (keys: ReadonlyArray<ContentKey>) =>
        Effect.forEach(Chunk.chunksOf(Chunk.fromIterable(keys), 1000), (window) =>
          send("<batch>", (signal) =>
            client.send(new DeleteObjectsCommand({
              Bucket: setting.bucket,
              Delete: { Objects: Chunk.toReadonlyArray(window).map((key) => ({ Key: key })) },
            }), { abortSignal: signal })), { discard: true }),
      list: (prefix: string) =>
        Stream.fromAsyncIterable(
          paginateListObjectsV2({ client }, { Bucket: setting.bucket, Prefix: prefix }),
          _folded(prefix),
        ),
    }
  }),
}) {}
```

## [4]-[LIFECYCLE]

- Owner: the `object_ref` ensure row and the lifecycle operations riding the service — `retain`/`release` reference edges in SQL, `sweep` folding zero-reference keys past their retention window into batch deletes, and the bucket lifecycle rules as data.
- Packages: `@effect/sql` (`SqlClient`); `@aws-sdk/client-s3` (`PutBucketLifecycleConfigurationCommand`).
- Entry: every durable owner of an object records `retain(key, owner, class)` in the same transaction that stores the reference and `release(key, owner)` when it lets go — GC is then pure data: `sweep(clazz)` deletes what nothing references and the window has aged out.
- Growth: a new retention class is a `journal/retain.md` policy row — the sweep reads the window from there; a new GC posture (orphan audit against `list`) is one fold over existing reads.
- Law: the sweep is reference-counted truth, never a guess — candidates are `GROUP BY key HAVING count(active) = 0` with the newest `released_at` older than the class window; the delete batches ≤1000 and the ledger rows drop only after the provider confirms.
- Law: `lifecycleRules` is the provider-side mirror — abort-incomplete-multipart and class-expiry rules applied once by `iac`'s object row from this data, so the bucket agrees with the ledger about aging.

```typescript
import { SqlClient } from "@effect/sql"

const _refDdl: Capability.Ensure = {
  relation: "object_ref",
  pg: `CREATE TABLE IF NOT EXISTS object_ref (
    key TEXT NOT NULL, owner TEXT NOT NULL,
    retention TEXT NOT NULL,
    retained_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    released_at TIMESTAMPTZ,
    PRIMARY KEY (key, owner));
  CREATE INDEX IF NOT EXISTS object_ref_key ON object_ref (key);`,
  sqlite: `CREATE TABLE IF NOT EXISTS object_ref (
    key TEXT NOT NULL, owner TEXT NOT NULL,
    retention TEXT NOT NULL,
    retained_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    released_at TEXT,
    PRIMARY KEY (key, owner));`,
}

const _lifecycleRules = [
  { ID: "abort-incomplete-multipart", Status: "Enabled", AbortIncompleteMultipartUpload: { DaysAfterInitiation: 7 } },
] as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { ObjectStore }
```
