# [DATA_STORE]

The content-addressed object plane: the object key IS the core `ContentKey` — this page is a delegating mint site, never a second hash — and the S3 `Key` string is its `:x32` spelling, so two writers producing the same bytes produce the same object and the conditional put makes the second writer a proven noop. One `ObjectStore` service owns the scoped client, the single abort-bridged `send`, the tagged fault fold with 412 pre-folded to the idempotent receipt, the size-and-shape-discriminated put (plain conditional, hand-composed conditional multipart for bounded bytes, `lib-storage` `Upload` for streaming bodies — the conditional spreads onto every leg), the verified get with ranged reads, the presigned capability mint, and the lifecycle plane: a SQL reference ledger driving reference-sweep GC whose deletes are `If-Match`-guarded CAS so a sweep can never race a re-mint. Provider facts are `Config` data against the engine conformance table — an engine that cannot honor `If-None-Match: *` cannot host this plane, ruled as rows, never re-litigated.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                            |
| :-----: | :------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CLIENT_SEAM`  | the scoped client, the one typed send, the fault fold, config, the engine table   |
|  [02]   | `CONDITIONAL`  | the put algebra — 412-noop, CAS, multipart-at-complete, streaming, verified reads |
|  [03]   | `REFERENCE_GC` | the reference ledger, the derived retention tag, If-Match CAS sweep, lifecycle, multipart reap |
|  [04]   | `GRANT_MINT`   | the one presign entry, TTL narrowing, header policy, the typed grant              |

## [02]-[CLIENT_SEAM]

- Owner: the `ObjectStore` service construction — `Effect.acquireRelease` around the client with `destroy` on release, the abort-bridged send idiom every operation repeats verbatim, the `_folded` fault fold with its read-shaped `_foldedRead` projection, one `_Setting` config owner, the `_shielded` resilience bracket every operation rides — and the `_engines` conformance table that rules which providers host the plane.
- Packages: `@aws-sdk/client-s3` (`S3Client`, `S3ServiceException`); `effect` (`Config`, `Redacted`, `Match`, `Data`, `Duration`, `Schedule`).
- Entry: every S3 operation in the unit is one abort-bridged `client.send(command)` — commands are values discriminating the operation; a per-verb method family and the flat client are the rejected forms; `read/batch.md`'s `presence` lane settles its HEAD windows through the `head` member.
- Growth: a new operation is a command value; a new provider is a `Config` change validated against the conformance table; a new engine is one table row with its conditional verdict filled.
- Law: the abort bridge is mandatory — `Effect.tryPromise({ try: (signal) => client.send(command, { abortSignal: signal }), catch: _folded(key) })` — fiber interruption aborts the in-flight request; an un-abortable send leaks past interruption.
- Law: the fault family is `ObjectFault` with reasons `missing` (404), `integrity` (identity or checksum disagreement), `io` (everything else) and a policy row per reason; 412 is NOT a fault — the fold returns the `_Replay` receipt before the family engages, and there is no `PreconditionFailed` class to catch (the status rides `$metadata.httpStatusCode` on the base exception).
- Law: resilience is owner-internal and the policy table is load-bearing — `_shielded` brackets every bounded operation with the op timeout, the bulkhead semaphore, and the jittered bounded retry whose `Schedule.whileInput` reads `fault.policy.retry`, so `io` retries while `missing`/`integrity` fail fast and a consumer composes capability, never plumbing; the AWS client's `maxAttempts` covers transport-level retry beneath it, and the circuit-breaker tier rides the core fault owner's degradation budget when that upstream row lands.
- Boundary: five members stand outside the bracket by construction — `putKeyed` (a one-shot streaming body cannot replay into a retry and outlives the op budget; its resilience is `Upload`'s part-level retry plus the abort bridge), `settled` (the waiter owns the construction-time `settleSeconds` budget), `sweep` and `reap` (walking folds whose per-key faults settle inside their own accumulation), and the reference verbs `refer`/`release` with their derived retag (the relational rail owns the ledger retry posture, and the retag is idempotent by derivation so its next invocation self-heals a missed stamp); every other member rides `_shielded`.
- Law: checksums are transport integrity, `ContentKey` is identity — `requestChecksumCalculation`/`responseChecksumValidation` pin `"WHEN_SUPPORTED"` against AWS-grade engines and `"WHEN_REQUIRED"` against S3-compatibles that predate default checksums, a `Config` fact per provider; identity verification is the core mint re-run at read, and the two never substitute.
- Law: the conformance table is the admission gate — an engine hosts this plane only with `conditional: "yes"`: the managed rows (S3, R2, Tigris) and the self-host rows (Ceph RGW, the maintained MinIO continuation) conform; the CRDT-metadata engine and the B2 row cannot CAS and are refused rows, kept as data so the argument is never re-had; the pending row waits on its concurrency fix.

```typescript signature
import { Config, Data, Duration, Effect, Match, Redacted, Schedule } from "effect"
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
  partFlight: Config.integer("OBJECT_PART_FLIGHT").pipe(Config.withDefault(4)),
  opFlight: Config.integer("OBJECT_OP_FLIGHT").pipe(Config.withDefault(16)),
  opBudget: Config.duration("OBJECT_OP_BUDGET").pipe(Config.withDefault(Duration.seconds(30))),
  presignTtl: Config.duration("OBJECT_PRESIGN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
  settleSeconds: Config.integer("OBJECT_SETTLE_SECONDS").pipe(Config.withDefault(30)),
})

const _RETRY = Schedule.exponential("100 millis").pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(3)),
  Schedule.whileInput((fault: ObjectFault | _Replay) => fault._tag === "ObjectFault" && fault.policy.retry),
)

const _shielded = (gate: Effect.Semaphore, budget: Duration.Duration) =>
  (key: string) =>
    <A, E extends ObjectFault | _Replay>(op: Effect.Effect<A, E>): Effect.Effect<A, E | ObjectFault> =>
      gate.withPermits(1)(
        op.pipe(
          Effect.timeoutFail({
            duration: budget,
            onTimeout: () => new ObjectFault({ reason: "io", key, detail: "<budget>" }),
          }),
          Effect.retry(_RETRY),
        ),
      )

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

## [03]-[CONDITIONAL]

- Owner: the conditional-put algebra and the read family — `conditional` (the ONE conditional command mint the server put, the presign grant, and the stream rail's finalize all share), `put` discriminating plain versus multipart versus streaming on the body shape and size, `get` with identity verification, `head` settling presence and descriptor evidence, and the consistency waiters; the ranged streaming read is `object/stream.md`'s `Rail.range` — one owner per read geometry, never both pages.
- Packages: `@aws-sdk/client-s3` (`PutObjectCommand`, `GetObjectCommand`, `HeadObjectCommand`, `GetObjectAttributesCommand`, `CopyObjectCommand`, `CreateMultipartUploadCommand`, `UploadPartCommand`, `CompleteMultipartUploadCommand`, `AbortMultipartUploadCommand`, `waitUntilObjectExists`); `@aws-sdk/lib-storage` (`Upload` — the streaming leg); `effect` (`Array`, `Stream`, `Exit`, `Option`); `@rasm/ts/core` (`ContentKey`, `Digest` — the delegating mint).
- Entry: `store.put(bytes)` mints the key from the bytes through the core digest and writes conditionally — the caller never supplies a key because identity is derived, not asserted; a streaming body whose key is already proven (the stream rail's finalize) enters through `store.putKeyed(key, body)` on the same conditional legs.
- Receipt: `ObjectStore.Receipt` — `{ key, bytes, written }` — `written: false` is the 412 idempotent noop, a success by law; the multipart and streaming legs land the same receipt because the conditional evaluates atomically at completion.
- Growth: a write posture (storage class, SSE) is a field threaded into the command mints, arriving as one policy row on the service construction; a read shape (range, part, attributes) is a command field, never a sibling get.
- Law: the conditional rides every leg — `IfNoneMatch: "*"` on the plain put, on the hand-composed `CompleteMultipartUpload`, and on the `Upload` params whose spread carries it onto both its paths; first-writer-wins lands at the moment the object materializes, and a 409 concurrent race retries into the 412 noop under the `io` policy row.
- Law: body shape selects the leg — bounded bytes below the threshold ride the plain put, bounded bytes above it ride the hand-composed part fold under `Effect.acquireRelease` with `AbortMultipartUpload` on failure, and a streaming or unknown-length body rides `Upload` with the abort bridged to fiber interruption; the caller sees one `put`.
- Law: `get` verifies identity — the returned bytes re-mint through `Digest.mint("content", bytes)` and disagreement is `integrity`; `ChecksumMode: "ENABLED"` rides the read so the provider's transport verification runs too; `head` answers the `Descriptor` request family through one `HeadObjectCommand` send as `ObjectStore.Stat` — the schema-owned evidence row whose `etag`, `contentType`, and `modified` fields are `Option`-carried with encodable twins, so the batch engine's durable band persists the same row `head` mints, a reply without `ContentLength` is the `io` fault, never a sentinel-zero forgery, and the HEAD windows and a singular probe share one member; `attributes` is the deep-evidence twin — `GetObjectAttributesCommand` yields `ObjectParts` and `Checksum` for multipart integrity audits a plain HEAD cannot carry.
- Law: every receipt is honest — `putKeyed` takes the proven span from the caller's identity fold, while `rekey(source, target)` probes the source once and carries its `Stat.bytes` into either copy outcome; the server-side copy derives `CopySourceIfMatch` from the same typed probe, so neither caller-provided ETags nor zero-byte receipt guesses are spellable.
- Boundary: `_putStreaming` is the one lib-storage seam — the `Upload` construction and the abort-signal listener are statement flow inside the `tryPromise` lambda, and fiber interruption reaches the in-flight multipart through `upload.abort()`.
- Law: consistency after a sweep race is a waiter, never a sleep — `settled(key)` runs `waitUntilObjectExists({ client, maxWaitTime: setting.settleSeconds, abortSignal }, { Bucket, Key })` to close the write-then-serve window where an engine's read-after-write posture demands it; the budget is construction policy shared with delete settlement, never a call-site knob.

```typescript signature
import { Array, DateTime, Exit, Option, Schema, Stream } from "effect"
import {
  AbortMultipartUploadCommand, CompleteMultipartUploadCommand, CopyObjectCommand, CreateMultipartUploadCommand,
  GetObjectAttributesCommand, GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand,
  waitUntilObjectExists,
} from "@aws-sdk/client-s3"
import { Upload } from "@aws-sdk/lib-storage"
import { Digest } from "@rasm/ts/core"

class _Stat extends Schema.Class<_Stat>("ObjectStore.Stat")({
  // encodable option fields: the batch engine's durable band persists this row through its own schema, so OptionFromSelf has no spelling here
  key: ContentKey,
  bytes: Schema.NonNegativeInt,
  etag: Schema.OptionFromNullOr(Schema.String),
  contentType: Schema.OptionFromNullOr(Schema.String),
  modified: Schema.OptionFromNullOr(Schema.DateTimeUtc),
}) {}

declare namespace ObjectStore {
  type Receipt = { readonly key: ContentKey; readonly bytes: number; readonly written: boolean }
  type Stat = _Stat
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

const _putMultipart = (client: S3Client, bucket: string, key: ContentKey, bytes: Uint8Array, partBytes: number, partFlight: number) =>
  Effect.scoped(
    Effect.gen(function* () {
      const opened = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: (signal) => client.send(new CreateMultipartUploadCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }),
          catch: _foldedRead(key),
        }),
        (held, exit) =>
          Exit.isFailure(exit)
              ? Effect.ignore(Effect.tryPromise({
                try: () => client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: held.UploadId })),
                catch: _foldedRead(key),
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
            catch: _foldedRead(key),
          }),
          (reply) => ({ ETag: reply.ETag, PartNumber: index + 1 }),
        ), { concurrency: partFlight })
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

const _putStreaming = (client: S3Client, bucket: string, key: ContentKey, body: ReadableStream<Uint8Array>, partBytes: number, partFlight: number, span: number, step?: (loaded: number) => void) =>
  Effect.matchEffect(
    Effect.tryPromise({
      try: (signal) => {
        const upload = new Upload({
          client,
          params: { Bucket: bucket, Key: key, Body: body, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" },
          partSize: partBytes,
          queueSize: partFlight,
        })
        signal.addEventListener("abort", () => { void upload.abort() }, { once: true })
        upload.on("httpUploadProgress", (progress) => {
          if (progress.loaded !== undefined) step?.(progress.loaded)
        })
        return upload.done()
      },
      catch: _folded(key),
    }),
    {
      onFailure: (fault) =>
        fault._tag === "ObjectReplay"
          ? Effect.succeed<ObjectStore.Receipt>({ key, bytes: span, written: false })
          : Effect.fail(fault),
      onSuccess: () => Effect.succeed<ObjectStore.Receipt>({ key, bytes: span, written: true }),
    },
  )

const _attributes = (client: S3Client, bucket: string, key: ContentKey) =>
  Effect.tryPromise({
    try: (signal) =>
      client.send(new GetObjectAttributesCommand({
        Bucket: bucket, Key: key,
        ObjectAttributes: ["ETag", "Checksum", "ObjectParts", "ObjectSize", "StorageClass"],
      }), { abortSignal: signal }),
    catch: _foldedRead(key),
  })

const _rekey = (client: S3Client, bucket: string, source: ContentKey, target: ContentKey) =>
  Effect.gen(function* () {
    const stat = yield* _headed(client, bucket, source)
    const sourceEtag = yield* Option.match(stat.etag, {
      onNone: () => Effect.fail(new ObjectFault({ reason: "io", key: source, detail: "<copy-source-etag>" })),
      onSome: Effect.succeed,
    })
    return yield* Effect.matchEffect(
      Effect.tryPromise({
        try: (signal) =>
          client.send(new CopyObjectCommand({
            Bucket: bucket, Key: target,
            CopySource: `${bucket}/${source}`,
            CopySourceIfMatch: sourceEtag,
          }), { abortSignal: signal }),
        catch: _folded(target),
      }),
      {
        onFailure: (fault) =>
          fault._tag === "ObjectReplay"
            ? Effect.succeed<ObjectStore.Receipt>({ key: target, bytes: stat.bytes, written: false })
            : Effect.fail(fault),
        onSuccess: () => Effect.succeed<ObjectStore.Receipt>({ key: target, bytes: stat.bytes, written: true }),
      },
    )
  })

const _got = (client: S3Client, bucket: string, key: ContentKey) =>
  Effect.gen(function* () {
    const reply = yield* Effect.tryPromise({
      try: (signal) =>
        client.send(new GetObjectCommand({ Bucket: bucket, Key: key, ChecksumMode: "ENABLED" }), { abortSignal: signal }),
      catch: _foldedRead(key),
    })
    const bytes = yield* Effect.tryPromise({
      try: () => reply.Body === undefined ? Promise.reject(new Error("<empty>")) : reply.Body.transformToByteArray(),
      catch: _foldedRead(key),
    })
    const minted = yield* Digest.mint("content", bytes)
    return yield* minted === key
      ? Effect.succeed(bytes)
      : Effect.fail(new ObjectFault({ reason: "integrity", key, detail: minted }))
  })

const _headed = (client: S3Client, bucket: string, key: ContentKey) =>
  Effect.flatMap(
    Effect.tryPromise({
      try: (signal) => client.send(new HeadObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }),
      catch: _foldedRead(key),
    }),
    (reply) =>
      // absence is Option, never a sentinel: a headless reply is the io fault, not a zero-byte forgery
      reply.ContentLength === undefined
        ? Effect.fail(new ObjectFault({ reason: "io", key, detail: "<headless>" }))
        : Effect.succeed(new _Stat({
            key,
            bytes: reply.ContentLength,
            etag: Option.fromNullable(reply.ETag),
            contentType: Option.fromNullable(reply.ContentType),
            modified: Option.flatMap(Option.fromNullable(reply.LastModified), DateTime.make),
          })),
  )

const _settled = (client: S3Client, bucket: string, key: ContentKey, maxWaitTime: number) =>
  Effect.asVoid(Effect.tryPromise({
    try: (signal) => waitUntilObjectExists({ client, maxWaitTime, abortSignal: signal }, { Bucket: bucket, Key: key }),
    catch: _foldedRead(key),
  }))
```

## [04]-[REFERENCE_GC]

- Owner: the `object_ref` ensure row, the reference verbs whose every ledger write re-derives the object's retention tag, the sweep, the two-layer native GC, and the multipart reap — orphan detection walks the bucket through the shipped paginator, joins each entry against the ledger, and every delete is a per-key `If-Match`-guarded CAS against the ETag the listing just carried; `DeleteObjectsCommand` is the refused spelling here because the 1000-key batch cannot carry a per-key conditional, and the CAS law outranks the round-trip saving; `lifecycle` pushes the retention-class windows as native bucket rules.
- Packages: `@aws-sdk/client-s3` (`DeleteObjectCommand`, `paginateListObjectsV2`, `ListMultipartUploadsCommand`, `AbortMultipartUploadCommand`, `PutBucketLifecycleConfigurationCommand`, `PutObjectTaggingCommand`, `waitUntilObjectNotExists`); `@effect/sql` (`SqlSchema`, `sql.insert`, `sql.in`); `journal/retain.md` (`Retain.Class`, `Retain.Policy` — the one retention vocabulary, and the shredded-subject law arriving as data); `effect` (`Order`, `Duration.Order` — the dominance fold).
- Entry: every producer that lands an object records `{ key, owner, retention }` through `store.refer` inside its own unit of work; `store.release(key, owner)` drops a reference; both verbs re-derive and re-stamp the object's retention tag from the surviving reference set, so no caller ever stamps a tag; the sweep and the reap run on the maintenance cadence (`read/fold.md`'s cron row where granted, the host schedule otherwise); `lifecycle` applies once at provision and on any `Retain.Policy` change.
- Receipt: the sweep's mark — `{ probed, swept, retained }` — rides the span and the fact stream; a swept key names its retention class so evidence and billing reconcile; the reap's mark — `{ probed, reaped }` — is the same evidence over abandoned multipart uploads.
- Growth: a new owner kind is a ledger row value; a new retention posture is a `Retain.Class` row arriving from the one vocabulary — the lifecycle rule set and the dominance fold regenerate from the table, zero edits here.
- Law: the object's retention tag is DERIVED, never asserted — `_retag` folds the dominant class over the key's unreleased references (`Array.max` under `Order.mapInput(Duration.Order, ...)` over `Retain.Policy` windows, so `permanent`'s infinite window dominates every finite one), and `refer`/`release` compose it after their ledger write; an object holding any live longer-retention reference therefore can never carry a shorter tag, and no native expiry rule can match an object a live reference still protects.
- Law: GC is two-layer — the CAS reference sweep owns referential correctness (an object is reclaimable only when its last ledger reference released), and the S3-native lifecycle rules own time-windowed expiry by the derived retention tag, the belt-and-suspenders arm that survives total SQL-ledger loss; `permanent` emits no rule, and the rules generate from `Retain.Policy` so no window literal exists here.
- Law: the sweep is CAS end to end — probe the candidate's ETag, re-check the ledger inside the same pass, delete with `IfMatch: etag`, then settle the delete through `waitUntilObjectNotExists` so a lagging-consistency engine cannot re-list a half-dead key into the next pass — a re-mint racing the sweep wins structurally: the re-put lands 412-noop against the still-present object or recreates it after the delete, and the guarded delete refuses when bytes changed under the probe.
- Law: the reap closes the crash window the bracket cannot — a process death between `CreateMultipartUpload` and its abort leaves invisible parts billing forever; `reap(age)` walks `ListMultipartUploadsCommand` pages by `KeyMarker`/`UploadIdMarker`, aborts every upload whose `Initiated` predates the age floor, and the tus staging band's own groom (its ids ride the staging prefix) stays untouched because age, not prefix, selects.
- Law: derivative references are owned rows — a derivative's ledger row names its source key as `owner`, so sweeping a source cascades through ordinary reference release, never a prefix guess.
- Law: retention classes gate the sweep — a `permanent` reference never sweeps, windowed classes sweep past `Retain.Policy[class].window`, and subject-sealed payload objects fall to crypto-shredding upstream (key destruction makes the bytes unreadable; the sweep merely reclaims them).

```typescript signature
import {
  AbortMultipartUploadCommand, DeleteObjectCommand, ListMultipartUploadsCommand, paginateListObjectsV2,
  PutBucketLifecycleConfigurationCommand, PutObjectTaggingCommand, waitUntilObjectNotExists,
} from "@aws-sdk/client-s3"
import { SqlClient, SqlSchema } from "@effect/sql"
import { Chunk, Order, Schema } from "effect"
import type { Capability } from "../lane/capability.ts"
import { Journal } from "../journal/append.ts"
import { Retain } from "../journal/retain.ts"

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

const _sweepDelete = (client: S3Client, bucket: string, settleSeconds: number, key: string, etag: string) =>
  Effect.zipRight(
    Effect.tryPromise({
      try: (signal) =>
        client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key, IfMatch: etag }), { abortSignal: signal }),
      catch: _folded(key),
    }),
    Effect.asVoid(Effect.tryPromise({
      try: (signal) =>
        waitUntilObjectNotExists({ client, maxWaitTime: settleSeconds, abortSignal: signal }, { Bucket: bucket, Key: key }),
      catch: _foldedRead(key),
    })),
  )

const _byWindow: Order.Order<Retain.Class> = Order.mapInput(Duration.Order, (clazz: Retain.Class) => Retain.Policy[clazz].window)

const _classify = (client: S3Client, bucket: string) =>
  (key: ContentKey, retention: Retain.Class) =>
    Effect.asVoid(Effect.tryPromise({
      try: (signal) =>
        client.send(new PutObjectTaggingCommand({
          Bucket: bucket, Key: key,
          Tagging: { TagSet: [{ Key: "retention", Value: retention }] },
        }), { abortSignal: signal }),
      catch: _foldedRead(key),
    }))

const _retag = (client: S3Client, bucket: string) =>
  (key: ContentKey) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const rows = yield* SqlSchema.findAll({
        Request: Schema.String,
        Result: Schema.Struct({ retention: Retain.Class }),
        execute: (who) => sql`SELECT DISTINCT retention FROM object_ref WHERE key = ${who} AND released_at IS NULL`,
      })(key)
      yield* Array.isNonEmptyReadonlyArray(rows)
        ? _classify(client, bucket)(key, Array.max(Array.map(rows, (row) => row.retention), _byWindow))
        : Effect.void
    })

const _lifecycle = (client: S3Client, bucket: string, policy: typeof Retain.Policy) =>
  Effect.asVoid(Effect.tryPromise({
    try: (signal) =>
      client.send(new PutBucketLifecycleConfigurationCommand({
        Bucket: bucket,
        LifecycleConfiguration: {
          Rules: Object.entries(policy).flatMap(([clazz, row]) =>
            Duration.isFinite(row.window)
              ? [{
                  ID: `retain-${clazz}`,
                  Status: "Enabled" as const,
                  Filter: { Tag: { Key: "retention", Value: clazz } },
                  Expiration: { Days: Math.max(1, Math.trunc(Duration.toDays(row.window))) },
                }]
              : []),
        },
      }), { abortSignal: signal }),
    catch: _folded(bucket),
  }))

const _refer = (key: ContentKey, owner: string, retention: Retain.Class) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`INSERT INTO object_ref ${sql.insert([{ key, owner, retention }])}
        ON CONFLICT (key, owner) DO UPDATE SET released_at = NULL, retention = excluded.retention`)

const _release = (key: ContentKey, owner: string) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`UPDATE object_ref SET released_at = ${Journal.now(sql)} WHERE key = ${key} AND owner = ${owner}`)

type _UploadMarker = { readonly keyMarker: string | undefined; readonly idMarker: string | undefined }

const _reap = (client: S3Client, bucket: string) =>
  (age: Duration.Duration) =>
    Effect.gen(function* () {
      const floor = DateTime.subtractDuration(yield* DateTime.now, age)
      const opened: _UploadMarker = { keyMarker: undefined, idMarker: undefined }
      return yield* Stream.runFoldEffect(
        Stream.paginateChunkEffect(opened, (marker) =>
          Effect.map(
            Effect.tryPromise({
              try: (signal) =>
                client.send(new ListMultipartUploadsCommand({
                  Bucket: bucket, KeyMarker: marker.keyMarker, UploadIdMarker: marker.idMarker,
                }), { abortSignal: signal }),
              catch: _foldedRead(bucket),
            }),
            (page) => [
              Chunk.fromIterable(page.Uploads ?? []),
              page.IsTruncated === true
                ? Option.some<_UploadMarker>({ keyMarker: page.NextKeyMarker, idMarker: page.NextUploadIdMarker })
                : Option.none<_UploadMarker>(),
            ] as const,
          )),
        { probed: 0, reaped: 0 },
        (mark, upload) =>
          Option.match(
            Option.filter(
              Option.flatMap(Option.fromNullable(upload.Initiated), DateTime.make),
              (initiated) => DateTime.lessThan(initiated, floor) && upload.Key !== undefined && upload.UploadId !== undefined,
            ),
            {
              onNone: () => Effect.succeed({ probed: mark.probed + 1, reaped: mark.reaped }),
              onSome: () =>
                Effect.as(
                  Effect.tryPromise({
                    try: (signal) =>
                      client.send(new AbortMultipartUploadCommand({
                        Bucket: bucket, Key: upload.Key, UploadId: upload.UploadId,
                      }), { abortSignal: signal }),
                    catch: _foldedRead(upload.Key ?? bucket),
                  }),
                  { probed: mark.probed + 1, reaped: mark.reaped + 1 },
                ),
            },
          ),
      ).pipe(Effect.withSpan("data.reap", { attributes: { bucket } }))
    })

const _sweep = (client: S3Client, bucket: string, settleSeconds: number) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) => {
    const live = SqlSchema.single({
      Request: Schema.String,
      Result: Schema.Struct({ live: Journal.Version }),
      execute: (key) => sql`SELECT count(*) AS live FROM object_ref WHERE key = ${key} AND released_at IS NULL`,
    })
    return Stream.runFoldEffect(
      Stream.fromAsyncIterable(
        paginateListObjectsV2({ client }, { Bucket: bucket }),
        (cause) => new ObjectFault({ reason: "io", key: bucket, detail: String(cause) }),
      ),
      { probed: 0, swept: 0, retained: 0 },
      (mark, page) =>
        Effect.reduce(page.Contents ?? [], mark, (held, entry) =>
          Effect.flatMap(
            live(entry.Key ?? ""),
            (count) =>
              count.live > 0 || entry.Key === undefined || entry.ETag === undefined
                ? Effect.succeed({ probed: held.probed + 1, swept: held.swept, retained: held.retained + 1 })
                : _sweepDelete(client, bucket, settleSeconds, entry.Key, entry.ETag).pipe(
                    Effect.as({ probed: held.probed + 1, swept: held.swept + 1, retained: held.retained }),
                    Effect.catchTag("ObjectReplay", () =>
                      Effect.succeed({ probed: held.probed + 1, swept: held.swept, retained: held.retained + 1 })),
                  ),
          )),
    ).pipe(Effect.withSpan("data.sweep", { attributes: { bucket } }))
  })
```

## [05]-[GRANT_MINT]

- Owner: `store.grant(key, command, policy?)` — one polymorphic mint over any command value: the command discriminates upload, download, part, or probe; the policy row narrows the TTL and carries the signed and hoisted header sets; the reply is the typed `{ url, expiresAt, key }` capability, never a bare string.
- Packages: `@aws-sdk/s3-request-presigner` (`getSignedUrl`, `signableHeaders`, `hoistableHeaders`); `effect` (`DateTime`, `Duration`).
- Entry: a browser-direct upload grant presigns the SAME conditional command `[3]` mints — the idempotency and checksum headers survive into the browser path by construction; a download grant hoists `Response*` overrides through the policy's `hoistableHeaders` set; the stream rail grants part uploads against its staging band.
- Receipt: the grant is a bounded bearer-equivalent capability — the mint is span-annotated because grants are auditable facts, and the serving plane returns it as the object-access token.
- Growth: a new presigned operation is a command value through the same entry; a signing posture (SSE-C pinning into `signableHeaders`, `Response*` hoisting into `hoistableHeaders`) is a `GrantPolicy` field, never a second mint.
- Law: config is inherited, never re-declared — the presigner reads the live client's resolved credentials, region, endpoint, and path style, so grants against any conforming engine are the same call and no second client exists; the published `provider` record carries its credential fields SEALED as `Redacted` values, and the one sanctioned unwrap is the staging store's own construction seam in `object/stream.md`.
- Law: `expiresIn` derives from `Duration.min(ttl, setting.presignTtl)` — a grant narrows policy and an unbounded or widened grant is unrepresentable at this surface.
- Law: the mint rides the `_shielded` bracket like every operation — bounded flight, budget, policy-gated retry; caller-keyed grant QUOTA composes the store-backed `RateLimiter` at the serving seam, because per-principal identity exists there and this page mints capability, never authorization.

```typescript signature
import { DateTime } from "effect"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import type { GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand } from "@aws-sdk/client-s3"

declare namespace ObjectStore {
  type Command = PutObjectCommand | GetObjectCommand | UploadPartCommand | HeadObjectCommand
  type Grant = { readonly url: string; readonly expiresAt: DateTime.Utc; readonly key: ContentKey }
  type GrantPolicy = {
    readonly ttl?: Duration.Duration
    readonly signableHeaders?: Set<string>
    readonly hoistableHeaders?: Set<string>
  }
}

const _grant = (client: S3Client, presignTtl: Duration.Duration) =>
  (key: ContentKey, command: ObjectStore.Command, policy?: ObjectStore.GrantPolicy) =>
    Effect.gen(function* () {
      const bounded = policy?.ttl === undefined ? presignTtl : Duration.min(policy.ttl, presignTtl)
      const url = yield* Effect.tryPromise({
        try: () =>
          getSignedUrl(client, command, {
            expiresIn: Math.trunc(Duration.toSeconds(bounded)),
            ...(policy?.signableHeaders !== undefined && { signableHeaders: policy.signableHeaders }),
            ...(policy?.hoistableHeaders !== undefined && { hoistableHeaders: policy.hoistableHeaders }),
          }),
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
    const gate = yield* Effect.makeSemaphore(setting.opFlight)
    const shield = _shielded(gate, setting.opBudget)
    const retag = _retag(client, setting.bucket)
    return {
      client,
      bucket: setting.bucket,
      folded: _foldedRead,
      presignTtl: setting.presignTtl,
      partBytes: setting.partBytes,
      partFlight: setting.partFlight,
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
          shield(key)(
            bytes.byteLength <= setting.multipartThreshold
              ? _putPlain(client, setting.bucket, key, bytes)
              : _putMultipart(client, setting.bucket, key, bytes, setting.partBytes, setting.partFlight),
          )),
      putKeyed: (key: ContentKey, body: ReadableStream<Uint8Array>, span: number, step?: (loaded: number) => void) =>
        _putStreaming(client, setting.bucket, key, body, setting.partBytes, setting.partFlight, span, step),
      get: (key: ContentKey) => shield(key)(_got(client, setting.bucket, key)),
      head: (key: ContentKey) => shield(key)(_headed(client, setting.bucket, key)),
      attributes: (key: ContentKey) => shield(key)(_attributes(client, setting.bucket, key)),
      rekey: (source: ContentKey, target: ContentKey) =>
        shield(target)(_rekey(client, setting.bucket, source, target)),
      settled: (key: ContentKey) => _settled(client, setting.bucket, key, setting.settleSeconds),
      sweep: _sweep(client, setting.bucket, setting.settleSeconds),
      reap: _reap(client, setting.bucket),
      grant: (key: ContentKey, command: ObjectStore.Command, policy?: ObjectStore.GrantPolicy) =>
        shield(key)(_grant(client, setting.presignTtl)(key, command, policy)),
      lifecycle: _lifecycle(client, setting.bucket, Retain.Policy),
      // the tag follows the ledger: both reference verbs re-derive the object's retention tag from the surviving reference set
      refer: (key: ContentKey, owner: string, retention: Retain.Class) =>
        Effect.zipRight(_refer(key, owner, retention), retag(key)),
      release: (key: ContentKey, owner: string) =>
        Effect.zipRight(_release(key, owner), retag(key)),
    }
  }),
}) {
  static readonly Stat = _Stat
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ObjectFault, ObjectStore }
```
