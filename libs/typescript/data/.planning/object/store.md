# [DATA_STORE]

The content-addressed object plane: the object key IS the core `ContentKey` — this page is a delegating mint site, never a second hash — and the S3 `Key` string is its `:x32` spelling, so two writers producing the same bytes produce the same object and the conditional put makes the second writer a proven noop. One `ObjectStore` service owns the scoped client, the single abort-bridged `send`, the tagged fault fold with 412 pre-folded to the idempotent receipt, the size-and-shape-discriminated put (plain conditional, hand-composed conditional multipart for bounded bytes, `lib-storage` `Upload` for streaming bodies — the conditional spreads onto every leg), the verified get with ranged reads, the presigned capability mint, and the lifecycle plane: a SQL reference ledger driving reference-sweep GC whose deletes are `If-Match`-guarded CAS so a sweep can never race a re-mint. Provider facts are `Config` data against the engine conformance table — an engine that cannot honor `If-None-Match: *` cannot host this plane, ruled as rows, never re-litigated.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                          |
| :-----: | :--------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `CLIENT_SEAM`    | the scoped client, the one typed send, the fault fold, config, the engine table      |
|  [02]   | `CONDITIONAL`    | the put algebra — 412-noop, CAS, multipart-at-complete, streaming, verified reads    |
|  [03]   | `REFERENCE_GC`   | the reference ledger, If-Match-guarded sweep, batch deletes, lifecycle rules         |
|  [04]   | `GRANT_MINT`     | the one presign entry, TTL narrowing, header policy, the typed grant                 |

## [2]-[CLIENT_SEAM]

- Owner: the `ObjectStore` service construction — `Effect.acquireRelease` around the client with `destroy` on release, the abort-bridged send idiom every operation repeats verbatim, the `_folded` fault fold with its read-shaped `_foldedRead` projection, one `_Setting` config owner — and the `_engines` conformance table that rules which providers host the plane.
- Packages: `@aws-sdk/client-s3` (`S3Client`, `S3ServiceException`); `effect` (`Config`, `Redacted`, `Match`, `Data`, `Duration`).
- Entry: every S3 operation in the unit is one abort-bridged `client.send(command)` — commands are values discriminating the operation; a per-verb method family and the flat client are the rejected forms; `read/batch.md`'s `presence` lane settles its HEAD windows through the `head` member.
- Growth: a new operation is a command value; a new provider is a `Config` change validated against the conformance table; a new engine is one table row with its conditional verdict filled.
- Law: the abort bridge is mandatory — `Effect.tryPromise({ try: (signal) => client.send(command, { abortSignal: signal }), catch: _folded(key) })` — fiber interruption aborts the in-flight request; an un-abortable send leaks past interruption.
- Law: the fault family is `ObjectFault` with reasons `missing` (404), `integrity` (identity or checksum disagreement), `io` (everything else) and a policy row per reason; 412 is NOT a fault — the fold returns the `_Replay` receipt before the family engages, and there is no `PreconditionFailed` class to catch (the status rides `$metadata.httpStatusCode` on the base exception).
- Law: checksums are transport integrity, `ContentKey` is identity — `requestChecksumCalculation`/`responseChecksumValidation` pin `"WHEN_SUPPORTED"` against AWS-grade engines and `"WHEN_REQUIRED"` against S3-compatibles that predate default checksums, a `Config` fact per provider; identity verification is the core mint re-run at read, and the two never substitute.
- Law: the conformance table is the admission gate — an engine hosts this plane only with `conditional: "yes"`: the managed rows (S3, R2, Tigris) and the self-host rows (Ceph RGW, the maintained MinIO continuation) conform; the CRDT-metadata engine and the B2 row cannot CAS and are refused rows, kept as data so the argument is never re-had; the pending row waits on its concurrency fix.

```typescript
import { Config, Data, Duration, Effect, Match, Redacted } from "effect"
import { S3Client, S3ServiceException } from "@aws-sdk/client-s3"
import { ContentKey } from "@rasm/ts/core"

const _engines = {
  s3: { conditional: "yes", posture: "managed" },
  r2: { conditional: "yes", posture: "managed" },
  tigris: { conditional: "yes", posture: "managed" },
  cephRgw: { conditional: "yes", posture: "selfHost" },
  minioContinuation: { conditional: "yes", posture: "selfHost" },
  seaweedfs: { conditional: "pending", posture: "selfHost" },
  garage: { conditional: "no", posture: "refused" },
  b2: { conditional: "no", posture: "refused" },
} as const

declare namespace ObjectStore {
  type Engine = keyof typeof _engines
  type Reason = "missing" | "integrity" | "io"
  type _Engines<T extends Record<Engine, { readonly conditional: "yes" | "no" | "pending"; readonly posture: string }> = typeof _engines> = T
}

const _Policy = {
  missing: { retry: false },
  integrity: { retry: false },
  io: { retry: true },
} as const

class ObjectFault extends Data.TaggedError("ObjectFault")<{
  readonly reason: ObjectStore.Reason
  readonly key: string
  readonly detail: string
}> {
  get policy(): (typeof _Policy)[ObjectStore.Reason] {
    return _Policy[this.reason]
  }
}

class _Replay extends Data.TaggedError("ObjectReplay")<{ readonly key: string }> {}

const _Setting = Config.unwrap({
  endpoint: Config.string("OBJECT_ENDPOINT"),
  region: Config.string("OBJECT_REGION").pipe(Config.withDefault("auto")),
  bucket: Config.string("OBJECT_BUCKET"),
  forcePathStyle: Config.boolean("OBJECT_PATH_STYLE").pipe(Config.withDefault(true)),
  checksums: Config.literal("WHEN_SUPPORTED", "WHEN_REQUIRED")("OBJECT_CHECKSUMS").pipe(Config.withDefault("WHEN_REQUIRED")),
  accessKeyId: Config.redacted("OBJECT_ACCESS_KEY_ID"),
  secretAccessKey: Config.redacted("OBJECT_SECRET_ACCESS_KEY"),
  maxAttempts: Config.integer("OBJECT_MAX_ATTEMPTS").pipe(Config.withDefault(3)),
  multipartThreshold: Config.integer("OBJECT_MULTIPART_BYTES").pipe(Config.withDefault(64 * 1024 * 1024)),
  partBytes: Config.integer("OBJECT_PART_BYTES").pipe(Config.withDefault(8 * 1024 * 1024)),
  presignTtl: Config.duration("OBJECT_PRESIGN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
})

const _folded = (key: string) => (caught: unknown): ObjectFault | _Replay =>
  Match.value(caught).pipe(
    Match.when(Match.instanceOf(S3ServiceException), (fault) =>
      fault.$metadata.httpStatusCode === 412
        ? new _Replay({ key })
        : fault.$metadata.httpStatusCode === 404
          ? new ObjectFault({ reason: "missing", key, detail: fault.name })
          : new ObjectFault({ reason: "io", key, detail: fault.name })),
    Match.orElse((residue) => new ObjectFault({ reason: "io", key, detail: String(residue) })),
  )

const _foldedRead = (key: string) => (caught: unknown): ObjectFault => {
  const fault = _folded(key)(caught)
  return fault._tag === "ObjectReplay" ? new ObjectFault({ reason: "io", key, detail: "<unconditional-412>" }) : fault
}
```

## [3]-[CONDITIONAL]

- Owner: the conditional-put algebra and the read family — `conditional` (the ONE conditional command mint the server put, the presign grant, and the stream rail's finalize all share), `put` discriminating plain versus multipart versus streaming on the body shape and size, `get` with identity verification, `head` settling presence and descriptor evidence, and the consistency waiters; the ranged streaming read is `object/stream.md`'s `Rail.range` — one owner per read geometry, never both pages.
- Packages: `@aws-sdk/client-s3` (`PutObjectCommand`, `GetObjectCommand`, `HeadObjectCommand`, `GetObjectAttributesCommand`, `CopyObjectCommand`, `CreateMultipartUploadCommand`, `UploadPartCommand`, `CompleteMultipartUploadCommand`, `AbortMultipartUploadCommand`, `waitUntilObjectExists`); `@aws-sdk/lib-storage` (`Upload` — the streaming leg); `effect` (`Array`, `Stream`, `Exit`, `Option`); `@rasm/ts/core` (`ContentKey`, `Digest` — the delegating mint).
- Entry: `store.put(body, options?)` mints the key from the bytes through the core digest and writes conditionally — the caller never supplies a key because identity is derived, not asserted; a streaming body whose key is already proven (the stream rail's finalize) enters through `store.putKeyed(key, body)` on the same conditional legs.
- Receipt: `ObjectStore.Receipt` — `{ key, bytes, written }` — `written: false` is the 412 idempotent noop, a success by law; the multipart and streaming legs land the same receipt because the conditional evaluates atomically at completion.
- Growth: a write posture (storage class, SSE) is a field on the options row flowing into the command value; a read shape (range, part, attributes) is a command field, never a sibling get.
- Law: the conditional rides every leg — `IfNoneMatch: "*"` on the plain put, on the hand-composed `CompleteMultipartUpload`, and on the `Upload` params whose spread carries it onto both its paths; first-writer-wins lands at the moment the object materializes, and a 409 concurrent race retries into the 412 noop under the `io` policy row.
- Law: body shape selects the leg — bounded bytes below the threshold ride the plain put, bounded bytes above it ride the hand-composed part fold under `Effect.acquireRelease` with `AbortMultipartUpload` on failure, and a streaming or unknown-length body rides `Upload` with the abort bridged to fiber interruption; the caller sees one `put`.
- Law: `get` verifies identity — the returned bytes re-mint through `Digest.mint("content", bytes)` and disagreement is `integrity`; `ChecksumMode: "ENABLED"` rides the read so the provider's transport verification runs too; `head` answers the `Presence`/`Descriptor` request families through one `HeadObjectCommand` send, so the batch engine's HEAD windows and a singular probe share one member.
- Law: consistency after a sweep race is a waiter, never a sleep — `settled(key, maxWaitTime)` runs `waitUntilObjectExists({ client, maxWaitTime, abortSignal }, { Bucket, Key })` to close the write-then-serve window where an engine's read-after-write posture demands it, a provider fact from the conformance row.

```typescript
import { Array, Exit, Option, Stream } from "effect"
import {
  AbortMultipartUploadCommand, CompleteMultipartUploadCommand, CreateMultipartUploadCommand,
  GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand, waitUntilObjectExists,
} from "@aws-sdk/client-s3"
import { Upload } from "@aws-sdk/lib-storage"
import { Digest } from "@rasm/ts/core"

declare namespace ObjectStore {
  type Receipt = { readonly key: ContentKey; readonly bytes: number; readonly written: boolean }
}

const _putPlain = (client: S3Client, bucket: string, key: ContentKey, bytes: Uint8Array) =>
  Effect.matchEffect(
    Effect.tryPromise({
      try: (signal) =>
        client.send(new PutObjectCommand({
          Bucket: bucket, Key: key, Body: bytes, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256",
        }), { abortSignal: signal }),
      catch: _folded(key),
    }),
    {
      onFailure: (fault) =>
        fault._tag === "ObjectReplay"
          ? Effect.succeed<ObjectStore.Receipt>({ key, bytes: bytes.byteLength, written: false })
          : Effect.fail(fault),
      onSuccess: () => Effect.succeed<ObjectStore.Receipt>({ key, bytes: bytes.byteLength, written: true }),
    },
  )

const _putMultipart = (client: S3Client, bucket: string, key: ContentKey, bytes: Uint8Array, partBytes: number) =>
  Effect.scoped(
    Effect.gen(function* () {
      const opened = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: (signal) => client.send(new CreateMultipartUploadCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }),
          catch: _folded(key),
        }),
        (held, exit) =>
          Exit.isFailure(exit)
            ? Effect.ignore(Effect.tryPromise({
                try: () => client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: held.UploadId })),
                catch: _folded(key),
              }))
            : Effect.void,
      )
      const windows = Array.makeBy(Math.ceil(bytes.byteLength / partBytes), (index) =>
        bytes.subarray(index * partBytes, (index + 1) * partBytes))
      const parts = yield* Effect.forEach(windows, (window, index) =>
        Effect.map(
          Effect.tryPromise({
            try: (signal) =>
              client.send(new UploadPartCommand({
                Bucket: bucket, Key: key, UploadId: opened.UploadId,
                PartNumber: index + 1,
                Body: window,
              }), { abortSignal: signal }),
            catch: _folded(key),
          }),
          (reply) => ({ ETag: reply.ETag, PartNumber: index + 1 }),
        ), { concurrency: 4 })
      yield* Effect.tryPromise({
        try: (signal) =>
          client.send(new CompleteMultipartUploadCommand({
            Bucket: bucket, Key: key, UploadId: opened.UploadId,
            MultipartUpload: { Parts: parts },
            IfNoneMatch: "*",
          }), { abortSignal: signal }),
        catch: _folded(key),
      })
      return { key, bytes: bytes.byteLength, written: true } satisfies ObjectStore.Receipt
    }),
  ).pipe(
    Effect.catchTag("ObjectReplay", () =>
      Effect.succeed<ObjectStore.Receipt>({ key, bytes: bytes.byteLength, written: false })),
  )

const _putStreaming = (client: S3Client, bucket: string, key: ContentKey, body: ReadableStream<Uint8Array>, partBytes: number) =>
  Effect.matchEffect(
    Effect.async<ObjectStore.Receipt, ObjectFault | _Replay>((resume) => {
      const upload = new Upload({
        client,
        params: { Bucket: bucket, Key: key, Body: body, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" },
        partSize: partBytes,
        queueSize: 4,
      })
      upload.done().then(
        () => resume(Effect.succeed<ObjectStore.Receipt>({ key, bytes: 0, written: true })),
        (caught) => resume(Effect.fail(_folded(key)(caught))),
      )
      return Effect.promise(() => upload.abort())
    }),
    {
      onFailure: (fault) =>
        fault._tag === "ObjectReplay"
          ? Effect.succeed<ObjectStore.Receipt>({ key, bytes: 0, written: false })
          : Effect.fail(fault),
      onSuccess: Effect.succeed,
    },
  )

const _got = (client: S3Client, bucket: string, key: ContentKey) =>
  Effect.gen(function* () {
    const reply = yield* Effect.tryPromise({
      try: (signal) =>
        client.send(new GetObjectCommand({ Bucket: bucket, Key: key, ChecksumMode: "ENABLED" }), { abortSignal: signal }),
      catch: _folded(key),
    })
    const bytes = yield* Effect.tryPromise({
      try: () => reply.Body === undefined ? Promise.reject(new Error("<empty>")) : reply.Body.transformToByteArray(),
      catch: _folded(key),
    })
    const minted = yield* Digest.mint("content", bytes)
    return yield* minted === key
      ? Effect.succeed(bytes)
      : Effect.fail(new ObjectFault({ reason: "integrity", key, detail: minted }))
  })

const _headed = (client: S3Client, bucket: string, key: ContentKey) =>
  Effect.map(
    Effect.tryPromise({
      try: (signal) => client.send(new HeadObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }),
      catch: _folded(key),
    }),
    (reply) => ({ key, bytes: reply.ContentLength ?? 0, etag: reply.ETag ?? "", contentType: reply.ContentType ?? "" }),
  )

const _settled = (client: S3Client, bucket: string, key: ContentKey, maxWaitTime: number) =>
  Effect.asVoid(Effect.tryPromise({
    try: (signal) => waitUntilObjectExists({ client, maxWaitTime, abortSignal: signal }, { Bucket: bucket, Key: key }),
    catch: _foldedRead(key),
  }))
```

## [4]-[REFERENCE_GC]

- Owner: the `object_ref` ensure row, the reference verbs, and the sweep — orphan detection walks the bucket through the shipped paginator, joins each entry against the ledger, and every delete is a per-key `If-Match`-guarded CAS against the ETag the listing just carried; `DeleteObjectsCommand` is the refused spelling here because the 1000-key batch cannot carry a per-key conditional, and the CAS law outranks the round-trip saving; bucket lifecycle rules land as data.
- Packages: `@aws-sdk/client-s3` (`DeleteObjectCommand`, `paginateListObjectsV2`, `PutBucketLifecycleConfigurationCommand`); `@effect/sql` (`sql.insert`, `sql.in`); `journal/retain.md` (`Retain.Class` — the one retention vocabulary, and the shredded-subject law arriving as data).
- Entry: every producer that lands an object records `{ key, owner, retention }` in the ledger inside its own unit of work; `store.release(key, owner)` drops a reference; the sweep runs on the maintenance cadence (`read/fold.md`'s cron row where granted, the host schedule otherwise).
- Receipt: the sweep's mark — `{ probed, swept, retained }` — rides the span and the fact stream; a swept key names its retention class so evidence and billing reconcile.
- Growth: a new owner kind is a ledger row value; a new retention posture is a `Retain.Class` row arriving from the one vocabulary; a lifecycle rule is a data row in the configuration command.
- Law: the sweep is CAS end to end — probe the candidate's ETag, re-check the ledger inside the same pass, delete with `IfMatch: etag` — so a re-mint racing the sweep wins structurally: the re-put lands 412-noop against the still-present object or recreates it after the delete, and the guarded delete refuses when bytes changed under the probe.
- Law: derivative references are owned rows — a derivative's ledger row names its source key as `owner`, so sweeping a source cascades through ordinary reference release, never a prefix guess.
- Law: retention classes gate the sweep — a `permanent` reference never sweeps, windowed classes sweep past `Retain.Policy[class].window`, and subject-sealed payload objects fall to crypto-shredding upstream (key destruction makes the bytes unreadable; the sweep merely reclaims them).

```typescript
import { DeleteObjectCommand, paginateListObjectsV2 } from "@aws-sdk/client-s3"
import { SqlClient } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal } from "../journal/append.ts"
import type { Retain } from "../journal/retain.ts"

const _refDdl: Capability.Ensure = {
  relation: "object_ref",
  pg: `CREATE TABLE IF NOT EXISTS object_ref (
    key TEXT NOT NULL, owner TEXT NOT NULL,
    retention TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    released_at TIMESTAMPTZ,
    PRIMARY KEY (key, owner));`,
  sqlite: `CREATE TABLE IF NOT EXISTS object_ref (
    key TEXT NOT NULL, owner TEXT NOT NULL,
    retention TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    released_at TEXT,
    PRIMARY KEY (key, owner));`,
}

const _sweepDelete = (client: S3Client, bucket: string, key: string, etag: string) =>
  Effect.tryPromise({
    try: (signal) =>
      client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key, IfMatch: etag }), { abortSignal: signal }),
    catch: _folded(key),
  })

const _refer = (key: ContentKey, owner: string, retention: Retain.Class) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`INSERT INTO object_ref ${sql.insert([{ key, owner, retention }])}
        ON CONFLICT (key, owner) DO UPDATE SET released_at = NULL`)

const _release = (key: ContentKey, owner: string) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`UPDATE object_ref SET released_at = ${Journal.now(sql)} WHERE key = ${key} AND owner = ${owner}`)

const _sweep = (client: S3Client, bucket: string) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Stream.runFoldEffect(
      Stream.fromAsyncIterable(
        paginateListObjectsV2({ client }, { Bucket: bucket }),
        (cause) => new ObjectFault({ reason: "io", key: bucket, detail: String(cause) }),
      ),
      { probed: 0, swept: 0, retained: 0 },
      (mark, page) =>
        Effect.reduce(page.Contents ?? [], mark, (held, entry) =>
          Effect.flatMap(
            sql`SELECT count(*) AS live FROM object_ref WHERE key = ${entry.Key ?? ""} AND released_at IS NULL`.values,
            (cells) =>
              Number(cells[0]?.[0] ?? 0) > 0 || entry.Key === undefined || entry.ETag === undefined
                ? Effect.succeed({ probed: held.probed + 1, swept: held.swept, retained: held.retained + 1 })
                : _sweepDelete(client, bucket, entry.Key, entry.ETag).pipe(
                    Effect.as({ probed: held.probed + 1, swept: held.swept + 1, retained: held.retained }),
                    Effect.catchTag("ObjectReplay", () =>
                      Effect.succeed({ probed: held.probed + 1, swept: held.swept, retained: held.retained + 1 })),
                  ),
          )),
    ).pipe(Effect.withSpan("data.sweep", { attributes: { bucket } })))
```

## [5]-[GRANT_MINT]

- Owner: `store.grant(key, command, ttl?)` — one polymorphic mint over any command value: the command discriminates upload, download, part, or probe; the TTL narrows the config default and never widens it; the reply is the typed `{ url, expiresAt, key }` capability, never a bare string.
- Packages: `@aws-sdk/s3-request-presigner` (`getSignedUrl`); `effect` (`DateTime`, `Duration`).
- Entry: a browser-direct upload grant presigns the SAME conditional command `[3]` mints — the idempotency and checksum headers survive into the browser path by construction; a download grant hoists `Response*` overrides through `hoistableHeaders`; the stream rail grants part uploads against its staging band.
- Receipt: the grant is a bounded bearer-equivalent capability — the mint is span-annotated because grants are auditable facts, and the serving plane returns it as the object-access token.
- Growth: a new presigned operation is a command value through the same entry; a signing posture (SSE-C pinning) is a `signableHeaders` policy row, never a second mint.
- Law: config is inherited, never re-declared — the presigner reads the live client's resolved credentials, region, endpoint, and path style, so grants against any conforming engine are the same call and no second client exists; the published `provider` record carries its credential fields SEALED as `Redacted` values, and the one sanctioned unwrap is the staging store's own construction seam in `object/stream.md`.
- Law: `expiresIn` derives from `Duration.min(ttl, setting.presignTtl)` — a grant narrows policy and an unbounded or widened grant is unrepresentable at this surface.

```typescript
import { DateTime } from "effect"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import type { GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand } from "@aws-sdk/client-s3"

declare namespace ObjectStore {
  type Command = PutObjectCommand | GetObjectCommand | UploadPartCommand | HeadObjectCommand
  type Grant = { readonly url: string; readonly expiresAt: DateTime.Utc; readonly key: ContentKey }
}

const _grant = (client: S3Client, presignTtl: Duration.Duration) =>
  (key: ContentKey, command: ObjectStore.Command, ttl?: Duration.Duration) =>
    Effect.gen(function* () {
      const bounded = ttl === undefined ? presignTtl : Duration.min(ttl, presignTtl)
      const url = yield* Effect.tryPromise({
        try: () => getSignedUrl(client, command, { expiresIn: Math.trunc(Duration.toSeconds(bounded)) }),
        catch: _folded(key),
      })
      const minted = yield* DateTime.now
      return { url, expiresAt: DateTime.addDuration(minted, bounded), key } satisfies ObjectStore.Grant
    }).pipe(Effect.withSpan("data.grant", { attributes: { key } }))

class ObjectStore extends Effect.Service<ObjectStore>()("data/ObjectStore", {
  scoped: Effect.gen(function* () {
    const setting = yield* _Setting
    const client = yield* Effect.acquireRelease(
      Effect.sync(() =>
        new S3Client({
          endpoint: setting.endpoint,
          region: setting.region,
          forcePathStyle: setting.forcePathStyle,
          maxAttempts: setting.maxAttempts,
          requestChecksumCalculation: setting.checksums,
          responseChecksumValidation: setting.checksums,
          credentials: {
            accessKeyId: Redacted.value(setting.accessKeyId),
            secretAccessKey: Redacted.value(setting.secretAccessKey),
          },
        })),
      (held) => Effect.sync(() => held.destroy()),
    )
    return {
      client,
      bucket: setting.bucket,
      folded: _foldedRead,
      presignTtl: setting.presignTtl,
      partBytes: setting.partBytes,
      provider: {
        endpoint: setting.endpoint,
        region: setting.region,
        forcePathStyle: setting.forcePathStyle,
        accessKeyId: setting.accessKeyId,
        secretAccessKey: setting.secretAccessKey,
      },
      conditional: (key: ContentKey) =>
        new PutObjectCommand({ Bucket: setting.bucket, Key: key, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" }),
      put: (bytes: Uint8Array) =>
        Effect.flatMap(Digest.mint("content", bytes), (key) =>
          bytes.byteLength <= setting.multipartThreshold
            ? _putPlain(client, setting.bucket, key, bytes)
            : _putMultipart(client, setting.bucket, key, bytes, setting.partBytes)),
      putKeyed: (key: ContentKey, body: ReadableStream<Uint8Array>) =>
        _putStreaming(client, setting.bucket, key, body, setting.partBytes),
      get: (key: ContentKey) => _got(client, setting.bucket, key),
      head: (key: ContentKey) => _headed(client, setting.bucket, key),
      settled: (key: ContentKey, maxWaitTime: number) => _settled(client, setting.bucket, key, maxWaitTime),
      sweep: _sweep(client, setting.bucket),
      grant: _grant(client, setting.presignTtl),
      refer: _refer,
      release: _release,
    }
  }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ObjectFault, ObjectStore }
```
