# [DATA_STORE]

`Digest.Key<"content">` is object identity. `ObjectStore` owns conditional writes, verified reads, grants, lifecycle, and reference-ledger GC.

## [01]-[INDEX]

- [02]-[CLIENT_SEAM]: scoped client, one typed send, fault fold, config, engine table.
- [03]-[CONDITIONAL]: put algebra — 412-noop, CAS, multipart-at-complete, streaming, verified reads.
- [04]-[REFERENCE_GC]: reference ledger, owner vocabulary with its mint and ingress admission, derived retention tag and the held posture, If-Match CAS sweep with the transitive derivative cascade, archive ladder and lifecycle, multipart reap, restore deferral.
- [05]-[INSTRUMENT_ROWS]: Convention projections — dedup outcome, bytes written, GC reclaim off the receipts.
- [06]-[GRANT_MINT]: one presign entry, TTL narrowing, header policy, typed grant.
- [07]-[EVENT_DATAREF]: confined CloudEvents claim-check externalize/resolve over the content-addressed store.
- [08]-[CUSTODY_CONTRACT]: object-plane half of the backend generation — custody descriptor artifact, capability rows, realized-state observation.

## [02]-[CLIENT_SEAM]

- Owner: the `ObjectStore` service construction — `Effect.acquireRelease` around the client with `destroy` on release, the abort-bridged send idiom every operation repeats verbatim, the `_folded` fault fold with its read-shaped `_foldedRead` projection, one `_Setting` config owner, the `_shielded` resilience bracket every operation rides — and the `_engines` conformance table that rules which providers host the plane.
- Packages: `@aws-sdk/client-s3`, `effect`, and core `Digest` with `Fault` supply the client, rails, identity, and resilience owners.
- Entry: every S3 operation in the unit is one abort-bridged `client.send(command)` — commands are values discriminating the operation; a per-verb method family and the flat client are the rejected forms; `read/batch.md`'s `presence` lane settles its HEAD windows through the `head` member.
- Growth: a new operation is a command value; a new provider is a `Config` change validated against the conformance table; a new engine is one table row with its conditional verdict filled.
- Law: the abort bridge is mandatory — `Effect.tryPromise({ try: (signal) => client.send(command, { abortSignal: signal }), catch: _folded(key) })` — fiber interruption aborts the in-flight request; an un-abortable send leaks past interruption.
- Law: `ObjectFault` closes missing, archived, refused-owner, integrity, engine-conformance, and I/O reasons through `Fault.Class.family`; HTTP 412 returns replay success.
- Law: the fold reads the SDK's TAGGED classes before it reads transport status, because a status is the coarsest evidence a reply carries and the archive verdict is the case that proves it — `InvalidObjectState` is neither 404 nor 412, so a status-only ladder drops it on the `io` arm and re-drives the whole `lease` curve against a condition no attempt can change, on the plane whose own `_lifecycle` writes the rules that archived the object.
- Law: the command's own conditional selects the leg's fold — `_folded` mints `_Replay` and serves exactly the legs that send a conditional header (the three put legs, the copy, the `If-Match` sweep delete, each folding the replay into a receipt or a retained mark), while every unconditional command takes `_foldedRead`: the presign mint, the lifecycle push, the multipart open, the part uploads, and the reads. `_folded` on an unconditional leg widens that leg's error channel with a private tag no caller folds even though the 412 can never arrive, and `_shielded` then hands its retry gate a value carrying no `class`, which `Fault.Class.of` reads as `defect` — the classification a store fault must never reach by construction.
- Law: `_shielded` composes the `Fault.Budget` lease schedule, attempt ceiling, total ceiling, and `Fault.Class.retryable` gate.
- Law: `Fault.Budget` owns the WHOLE curve and the SDK-native retry pins to a single attempt — `maxAttempts: 1` on the client is the pin, and no `Config` row exposes it, because a provider schedule nested inside each attempt of the lease curve makes effective attempts the PRODUCT of two schedules and every budget on this page then measures a span it never fixed.
- Boundary: streaming, waiting, walking, and reference members stand outside the bracket by construction — `putKeyed` cannot replay a one-shot body, `settled` owns its waiter budget, `sweep` and `reap` settle faults inside their folds, and `refer`/`release` ride the relational rail; every other member rides `_shielded`.
- Law: transport checksums never replace `Digest.Key<"content">`; verified reads remint identity from bytes.
- Law: the conformance table is the admission gate a boot READS — `_Setting.engine` names the row and construction refuses any engine whose `conditional` column is not `"yes"`, so the gate is a startup verdict rather than a comment: the managed rows (S3, R2, Tigris) and the self-host rows (Ceph RGW, the maintained MinIO continuation, SeaweedFS) host the plane, while the CRDT-metadata engine, the B2 row, and the GCS row cannot and stay as data so the argument is never re-had.
- Law: no refusal on this table waits on a release, which is why the column carries two values and not three — the CRDT-metadata engine forecloses conditional writes in its own design, having declined the consensus algorithm they need; GCS answers create-if-absent through a generation precondition its S3 interoperability surface never spells, so the one guarantee this plane admits on cannot cross at all; and the B2 row rests on an observed refusal of the header rather than a published stance, so that row alone re-probes at admission. SeaweedFS moved into the hosting set on its own compare-and-create landing for the unversioned buckets this plane exclusively writes.
- Law: archive depth reads off the ladder POSITION, so a cell admits every rung beneath it and a rung an engine's own guidance warns off caps the cell BENEATH that rung rather than carving it out — Ceph RGW reaches deep archive through its cloud-transition tier type yet reads `cool`, because the `cold` rung's own literal is the GLACIER-prefixed name that engine tells clients to avoid, and a cell of `cold` admits it.
- Law: self-host rows name their transition classes by administrator declaration while the SDK's `TransitionStorageClass` union is closed over six AWS literals, so a class spelled anything else cannot cross this command at all and a class the cluster never minted takes a 200 that transitions nothing — each self-host cell therefore states the deepest rung whose `_STORAGE` literal an operator can both mint and the engine advises, and provisioning that engine mints those classes under exactly those names. MinIO's continuation caps at `cold` because its tiering refuses by design any remote demanding rehydration, and Tigris caps there because its accepted class set stops one literal short of deep archive.
- Law: the engine row answers the whole descriptor a deployment selects on, and two of those coordinates this plane decides NOWHERE — `posture` with `archive` names what a row FITS, `conditional` is its ADMISSION gate (boot refuses any cell short of `yes`), and DEGRADE reads off both: an `archive: "none"` row gives up every retention transition and prices one tier for an object's whole life, a `"cool"` row gives up the restore-bearing rungs beneath it, and a refused row gives up atomic create-if-absent, which is the guarantee rather than the gap; TENANCY no engine row decides, because the key IS the content and one byte-identical object serves every tenant referencing it, so attribution lives on `object_ref` owner coordinates under the closed grammar and an engine-level answer states a guess; LIFETIME no engine row decides either, since the last reference release makes an object reclaimable and the CAS sweep with the native expiry rules ends it, so neither a caller nor a provider ends an object's life.
- Law: archive depth is a conformance CELL, never a fork — `archive` names the deepest `Retain.depths` rung an engine honours (`frozen` the whole restore-bearing ladder, `cool` the reduced-access class alone, `none` a single-tier store), `_lifecycle` filters each retention row's transition rungs against it by depth POSITION, and an engine at `none` finds no index and receives expiry rules alone; a rung an engine cannot honour is a rule its API accepts and silently never applies, which reads on the bill as archived pricing that never arrived. R2 conforms on the conditional header and stores reduced-access alone, so its cell states that rather than inheriting the managed default, and GCS reads `none` because the same interoperability layer that refuses the conditional also takes Google's own lifecycle document in place of the S3 `Transition` element — a product-level archive tier the S3 path reaches through no rule this page can write.

```typescript
import { Config, Data, Duration, Effect, Match, Redacted, Schema, Struct } from "effect"
import { InvalidObjectState, S3Client, S3ServiceException } from "@aws-sdk/client-s3"
import { Fault } from "@rasm/core"

const _engines = {
  s3: { conditional: "yes", posture: "managed", archive: "frozen" },
  r2: { conditional: "yes", posture: "managed", archive: "cool" },
  tigris: { conditional: "yes", posture: "managed", archive: "cold" },
  cephRgw: { conditional: "yes", posture: "selfHost", archive: "cool" },
  minioContinuation: { conditional: "yes", posture: "selfHost", archive: "cold" },
  seaweedfs: { conditional: "yes", posture: "selfHost", archive: "none" },
  gcs: { conditional: "no", posture: "refused", archive: "none" },
  garage: { conditional: "no", posture: "refused", archive: "none" },
  b2: { conditional: "no", posture: "refused", archive: "none" },
} as const

declare namespace ObjectStore {
  type Engine = keyof typeof _engines
  type Archive = (typeof _engines)[Engine]["archive"]
  type Reason = (typeof _family.kinds)[number]
  type _Engines<
    T extends Record<Engine, {
      readonly conditional: "yes" | "no"
      readonly posture: "managed" | "selfHost" | "refused"
      readonly archive: Retain.Depth | "none"
    }> = typeof _engines,
  > = T
}

const _Subject = Schema.Struct({ key: Schema.String, detail: Schema.String })

const _family = Fault.Class.family(["missing", "archived", "owner", "integrity", "engine", "io"] as const, {
  missing: Fault.Class.row({
    class: "absent",
    leg: "store",
    detail: _Subject,
    render: ({ key, detail }) => `${key} names no object — ${detail}`,
  }),
  archived: Fault.Class.row({
    class: "denied",
    leg: "store",
    detail: _Subject,
    render: ({ key, detail }) => `${key} sits at storage class ${detail} and answers no read until a restore lands`,
  }),
  owner: Fault.Class.row({
    class: "denied",
    leg: "custody",
    detail: _Subject,
    render: ({ key, detail }) => `${key} offered a custody coordinate this seam refuses — ${detail}`,
  }),
  integrity: Fault.Class.row({
    class: "breached",
    leg: "identity",
    detail: _Subject,
    render: ({ key, detail }) => `${key} re-minted as ${detail}`,
  }),
  engine: Fault.Class.row({
    class: "invalid",
    leg: "client",
    detail: _Subject,
    render: ({ key, detail }) => `bucket ${key} names a non-conforming engine — ${detail}`,
  }),
  io: Fault.Class.row({
    class: "unavailable",
    leg: "store",
    detail: _Subject,
    render: ({ key, detail }) => `${key} refused at the transport — ${detail}`,
  }),
})

class ObjectFault extends Schema.TaggedError<ObjectFault>()("ObjectFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

class _Replay extends Data.TaggedError("ObjectReplay")<{ readonly key: string }> {}

const _Setting = Config.unwrap({
  engine: Config.literal(...Struct.keys(_engines))("OBJECT_ENGINE").pipe(Config.withDefault("s3" as const)),
  endpoint: Config.string("OBJECT_ENDPOINT"),
  region: Config.string("OBJECT_REGION").pipe(Config.withDefault("auto")),
  bucket: Config.string("OBJECT_BUCKET"),
  forcePathStyle: Config.boolean("OBJECT_PATH_STYLE").pipe(Config.withDefault(true)),
  checksums: Config.literal("WHEN_SUPPORTED", "WHEN_REQUIRED")("OBJECT_CHECKSUMS").pipe(Config.withDefault("WHEN_REQUIRED")),
  accessKeyId: Config.redacted("OBJECT_ACCESS_KEY_ID"),
  secretAccessKey: Config.redacted("OBJECT_SECRET_ACCESS_KEY"),
  multipartThreshold: Config.integer("OBJECT_MULTIPART_BYTES").pipe(Config.withDefault(64 * 1024 * 1024)),
  partBytes: Config.integer("OBJECT_PART_BYTES").pipe(Config.withDefault(8 * 1024 * 1024)),
  partFlight: Config.integer("OBJECT_PART_FLIGHT").pipe(Config.withDefault(4)),
  opFlight: Config.integer("OBJECT_OP_FLIGHT").pipe(Config.withDefault(16)),
  presignTtl: Config.duration("OBJECT_PRESIGN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
  settleSeconds: Config.integer("OBJECT_SETTLE_SECONDS").pipe(Config.withDefault(30)),
})

const _RETRY = Fault.Budget.schedule("lease")

const _shielded = (gate: Effect.Semaphore) =>
  (key: string) =>
    <A, E extends ObjectFault | _Replay>(op: Effect.Effect<A, E>): Effect.Effect<A, E | ObjectFault> =>
      gate.withPermits(1)(
        op.pipe(
          Effect.timeoutFail({
            duration: Fault.Budget.at("lease").attempt,
            onTimeout: () => new ObjectFault({ case: { reason: "io", key, detail: "<attempt-budget>" } }),
          }),
          Effect.retry(_RETRY),
          Effect.timeoutFail({
            duration: Fault.Budget.at("lease").total,
            onTimeout: () => new ObjectFault({ case: { reason: "io", key, detail: "<call-budget>" } }),
          }),
        ),
      )

const _folded = (key: string) => (caught: unknown): ObjectFault | _Replay =>
  Match.value(caught).pipe(
    Match.when(Match.instanceOf(InvalidObjectState), (fault) =>
      new ObjectFault({ case: { reason: "archived", key, detail: fault.StorageClass ?? fault.name } })),
    Match.when(Match.instanceOf(S3ServiceException), (fault) =>
      fault.$metadata.httpStatusCode === 412
        ? new _Replay({ key })
        : fault.$metadata.httpStatusCode === 404
          ? new ObjectFault({ case: { reason: "missing", key, detail: fault.name } })
          : new ObjectFault({ case: { reason: "io", key, detail: fault.name } })),
    Match.orElse((residue) => new ObjectFault({ case: { reason: "io", key, detail: String(residue) } })),
  )

const _foldedRead = (key: string) => (caught: unknown): ObjectFault =>
  Match.value(_folded(key)(caught)).pipe(
    Match.when(Match.instanceOf(_Replay), () => new ObjectFault({ case: { reason: "io", key, detail: "<unconditional-412>" } })),
    Match.orElse((fault) => fault),
  )
```

## [03]-[CONDITIONAL]

- Owner: the conditional-put algebra and the read family — `conditional` (the ONE conditional command mint the server put, the presign grant, and the stream rail's finalize all share), `put` discriminating plain versus multipart versus streaming on the body shape and size, `get` with identity verification, `head` settling presence and descriptor evidence, and the consistency waiters; the ranged streaming read is `object/stream.md`'s `Rail.range` — one owner per read geometry, never both pages.
- Packages: AWS S3 clients, `effect`, and core `Digest` supply conditional transport, streaming rails, and content identity.
- Entry: `store.put(bytes)` mints the key from the bytes through the core digest and writes conditionally — the caller never supplies a key because identity is derived, not asserted; a streaming body whose key is already proven (the stream rail's finalize) enters through `store.putKeyed(key, body)` on the same conditional legs.
- Receipt: `ObjectStore.Receipt` — `{ key, bytes, written }` — `written: false` is the 412 idempotent noop, a success by law; the multipart and streaming legs land the same receipt because the conditional evaluates atomically at completion.
- Growth: a write posture is a field threaded into the command mints, arriving as one policy row on the service construction; a read shape (range, part, attributes) is a command field, never a sibling get; a new producer is one `_OWNERS` row `[04]` and its own reference row, never a leg here.
- Law: every leg writes at the engine's DEFAULT class and the retention ladder owns depth alone — a put-time archive class is refused because this plane verifies identity by re-minting the bytes it reads back, and a class that cannot be read without a restore makes that verification impossible for the object's whole cold life; the dedup leg proves the same point from the other side, since a 412 against a live object leaves whatever class that object already carries, so a put-time class holds on first write and silently vanishes on every replay. Depth is therefore an AGE decision the lifecycle rules apply, never a write-time assertion a replay can lose.
- Law: the conditional rides every leg — `IfNoneMatch: "*"` on the plain put, on the hand-composed `CompleteMultipartUpload`, and on the `Upload` params whose spread carries it onto both its paths; first-writer-wins lands at the moment the object materializes, and a 409 concurrent race retries into the 412 noop under the `io` reason's retryable class.
- Law: body shape selects the leg — bounded bytes below the threshold ride the plain put, bounded bytes above it ride the hand-composed part fold under `Effect.acquireRelease` with `AbortMultipartUpload` on failure, and a streaming or unknown-length body rides `Upload` with the abort bridged to fiber interruption; the caller sees one `put`.
- Law: the SHA-256 transport checksum rides ALL THREE write legs, never the plain and streaming pair alone — the multipart leg declares the algorithm at `CreateMultipartUpload`, asserts it on every `UploadPart`, and carries each part reply's `ChecksumSHA256` into its `CompletedPart` so the engine re-verifies the assembled object at completion; the middle size band is otherwise the one class whose wire corruption reaches the verified read as an `integrity` fault with no transport evidence naming the part that carried it.
- Law: `get` verifies identity — the returned bytes re-mint through `Digest.mint("content", bytes)` and disagreement is `integrity`; `ChecksumMode: "ENABLED"` rides the read so the provider's transport verification runs too; `head` answers the `Descriptor` request family through one `HeadObjectCommand` send as `ObjectStore.Stat` — the schema-owned evidence row whose `etag`, `contentType`, and `modified` fields are `Option`-carried with encodable twins, so the batch engine's durable band persists the same row `head` mints, a reply without `ContentLength` is the `io` fault, never a sentinel-zero forgery, and the HEAD windows and a singular probe share one member; `attributes` is the deep-evidence twin — `GetObjectAttributesCommand` yields `ObjectParts` and `Checksum` for multipart integrity audits a plain HEAD cannot carry.
- Law: every receipt is honest — `putKeyed` takes the proven span from the caller's identity fold, while `rekey(source, target)` probes the source once and carries its `Stat.bytes` into either copy outcome; the server-side copy derives `CopySourceIfMatch` from the same typed probe, so neither caller-provided ETags nor zero-byte receipt guesses are spellable.
- Boundary: `_putStreaming` is the one lib-storage seam — the `Upload` construction and the abort-signal listener are statement flow inside the `tryPromise` lambda, and fiber interruption reaches the in-flight multipart through the injected `Options.abortController`, whose `abort()` returns void so the teardown call and its rejection stay on the `done()` promise the fold already owns.
- Law: consistency after a sweep race is a waiter, never a sleep — `settled(key)` runs `waitUntilObjectExists({ client, maxWaitTime: setting.settleSeconds, abortSignal }, { Bucket, Key })` to close the write-then-serve window where an engine's read-after-write posture demands it; the budget is construction policy shared with delete settlement, never a call-site knob.
- Law: producers reach ONE of the two put legs and mint no byte plane of their own — `object/file.md`'s derivative persist and `lane/olap.md#ARROW_WIRE`'s `Olap.lake.write`/`.sink` Parquet egress hand bounded bytes to `put` (a row-group window is bytes in hand, so identity derives and no caller asserts a key), while `object/file.md`'s disk seal, `object/stream.md`'s tus finalize and its custody preservation landing, and `object/remote.md`'s remote ingest carry a proven span into `putKeyed`; each spends the owner mint `[04]` closes and records its reference row in the same unit of work, so the cold-tail residence and every other landed object share one identity, one conditional, and one GC ledger rather than a second addressing scheme per producer.

```typescript
import { Array, DateTime, Exit, Option, Schema, Stream } from "effect"
import {
  AbortMultipartUploadCommand, CompleteMultipartUploadCommand, CopyObjectCommand, CreateMultipartUploadCommand,
  GetObjectAttributesCommand, GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand,
  waitUntilObjectExists,
} from "@aws-sdk/client-s3"
import { Upload } from "@aws-sdk/lib-storage"
import { Digest } from "@rasm/core"

class _Stat extends Schema.Class<_Stat>("ObjectStore.Stat")({
  key: Digest.Key.content,
  bytes: Schema.NonNegativeInt,
  etag: Schema.OptionFromNullOr(Schema.String),
  contentType: Schema.OptionFromNullOr(Schema.String),
  modified: Schema.OptionFromNullOr(Schema.DateTimeUtc),
  storage: Schema.OptionFromNullOr(Schema.String),
  archive: Schema.OptionFromNullOr(Schema.String),
  restore: Schema.OptionFromNullOr(Schema.String),
}) {
  get restoring(): boolean {
    return Option.match(this.restore, { onNone: () => false, onSome: (held) => held.includes(`ongoing-request="true"`) })
  }
}

declare namespace ObjectStore {
  type Receipt = { readonly key: Digest.Key<"content">; readonly bytes: number; readonly written: boolean }
  type Stat = _Stat
  type RestorePolicy = { readonly days: number; readonly tier: "Bulk" | "Expedited" | "Standard" }
}

const _putPlain = (client: S3Client, bucket: string, key: Digest.Key<"content">, bytes: Uint8Array) =>
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

const _putMultipart = (client: S3Client, bucket: string, key: Digest.Key<"content">, bytes: Uint8Array, partBytes: number, partFlight: number) =>
  Effect.scoped(
    Effect.gen(function* () {
      const opened = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: (signal) =>
            client.send(new CreateMultipartUploadCommand({
              Bucket: bucket, Key: key, ChecksumAlgorithm: "SHA256",
            }), { abortSignal: signal }),
          catch: _foldedRead(key),
        }),
        (held, exit) =>
          Exit.isFailure(exit)
              ? Effect.catchAll(
                  Effect.tryPromise({
                    try: () => client.send(new AbortMultipartUploadCommand({ Bucket: bucket, Key: key, UploadId: held.UploadId })),
                    catch: _foldedRead(key),
                  }),
                  () => Effect.void,
                )
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
                ChecksumAlgorithm: "SHA256",
              }), { abortSignal: signal }),
            catch: _foldedRead(key),
          }),
          (reply) => ({ ETag: reply.ETag, PartNumber: index + 1, ChecksumSHA256: reply.ChecksumSHA256 }),
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

const _putStreaming = (client: S3Client, bucket: string, key: Digest.Key<"content">, body: ReadableStream<Uint8Array>, partBytes: number, partFlight: number, span: number, step?: (loaded: number) => void) =>
  Effect.matchEffect(
    Effect.tryPromise({
      try: (signal) => {
        const abortController = new AbortController()
        signal.addEventListener("abort", () => abortController.abort(), { once: true })
        const upload = new Upload({
          client,
          abortController,
          params: { Bucket: bucket, Key: key, Body: body, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" },
          partSize: partBytes,
          queueSize: partFlight,
        })
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

const _attributes = (client: S3Client, bucket: string, key: Digest.Key<"content">) =>
  Effect.tryPromise({
    try: (signal) =>
      client.send(new GetObjectAttributesCommand({
        Bucket: bucket, Key: key,
        ObjectAttributes: ["ETag", "Checksum", "ObjectParts", "ObjectSize", "StorageClass"],
      }), { abortSignal: signal }),
    catch: _foldedRead(key),
  })

const _rekey = (client: S3Client, bucket: string, source: Digest.Key<"content">, target: Digest.Key<"content">) =>
  Effect.gen(function* () {
    const stat = yield* _headed(client, bucket, source)
    const sourceEtag = yield* Option.match(stat.etag, {
      onNone: () => Effect.fail(new ObjectFault({ case: { reason: "io", key: source, detail: "<copy-source-etag>" } })),
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

const _got = (client: S3Client, bucket: string, key: Digest.Key<"content">) =>
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
    return yield* (minted === key
      ? Effect.succeed(bytes)
      : Effect.fail(new ObjectFault({ case: { reason: "integrity", key, detail: minted } })))
  })

const _headed = (client: S3Client, bucket: string, key: Digest.Key<"content">) =>
  Effect.flatMap(
    Effect.tryPromise({
      try: (signal) => client.send(new HeadObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }),
      catch: _foldedRead(key),
    }),
    (reply) =>
      reply.ContentLength === undefined
        ? Effect.fail(new ObjectFault({ case: { reason: "io", key, detail: "<headless>" } }))
        : Effect.succeed(new _Stat({
            key,
            bytes: reply.ContentLength,
            etag: Option.fromNullable(reply.ETag),
            contentType: Option.fromNullable(reply.ContentType),
            modified: Option.flatMap(Option.fromNullable(reply.LastModified), DateTime.make),
            storage: Option.fromNullable(reply.StorageClass),
            archive: Option.fromNullable(reply.ArchiveStatus),
            restore: Option.fromNullable(reply.Restore),
          })),
  )

const _settled = (client: S3Client, bucket: string, key: Digest.Key<"content">, maxWaitTime: number) =>
  Effect.asVoid(Effect.tryPromise({
    try: (signal) => waitUntilObjectExists({ client, maxWaitTime, abortSignal: signal }, { Bucket: bucket, Key: key }),
    catch: _foldedRead(key),
  }))
```

## [04]-[REFERENCE_GC]

- Owner: the `object_ref` ensure row, the reference verbs whose every ledger write re-derives the object's retention tag, the sweep, the transitive `derivative:` reach, the two-layer native GC, and the multipart reap — orphan detection walks the bucket through the shipped paginator, joins each entry against the ledger, and every delete is a per-key `If-Match`-guarded CAS against the ETag the listing just carried; `DeleteObjectsCommand` is the refused spelling here because the 1000-key batch cannot carry a per-key conditional, and the CAS law outranks the round-trip saving; `lifecycle` pushes the retention-class windows as native bucket rules.
- Packages: `@aws-sdk/client-s3` (`DeleteObjectCommand`, `paginateListObjectsV2`, `ListMultipartUploadsCommand`, `AbortMultipartUploadCommand`, `PutBucketLifecycleConfigurationCommand`, `PutObjectTaggingCommand`, `RestoreObjectCommand`, `TransitionStorageClass`, `waitUntilObjectNotExists`); `@effect/sql` (`SqlSchema`, `sql.insert`, `sql.in`, `sql.withTransaction`); `journal/retain.md` (`Retain.Class`, `Retain.Policy`, `Retain.depths` — the one retention vocabulary with its cost ladder, and the shredded-subject law arriving as data); `@rasm/core` (`Shape.Bound` — the walk budget the cascade's convergence is stated in); `effect` (`Order`, `Duration.Order` — the dominance fold; `Array`, `HashMap`, `Record`, `Option`, `pipe` — the rule fold and the reach join).
- Entry: every producer that lands an object records `{ key, owner, retention }` through `store.refer` inside its own unit of work; `store.release(key, owner)` drops a reference; both verbs re-derive and re-stamp the object's retention tag from the surviving reference set on the post-commit drain, so no caller ever stamps a tag and the re-derivation never rides the caller's pin; the sweep and the reap run on the maintenance cadence (`read/fold.md`'s cron row where granted, the host schedule otherwise); `lifecycle` applies once at provision and on any `Retain.Policy` change.
- Receipt: the sweep's mark — `{ probed, swept, cascaded, reclaimed, retained }` — rides the span and the fact stream, `swept` the key census, `cascaded` the reference rows the transitive reach released beneath those keys, and `reclaimed` the byte total the listing entries' own `Size` already carried at the fold, so the byte-coded reclaim instrument reads bytes and evidence reconciles against billing in the unit billing is denominated in; the reap's mark — `{ probed, reaped }` — is the same evidence over abandoned multipart uploads.
- Growth: a new owner kind is one `_OWNERS` row carrying its grammar, its role, and its coining page; a new retention posture is a `Retain.Class` row arriving from the one vocabulary and a new cost depth one `Retain.depths` entry with its `_STORAGE` answer — the lifecycle rule set, the ladder filter, and the dominance fold regenerate from those tables, zero edits here.
- Law: the lifecycle rule set carries BOTH halves each retention row prices — the transition ladder its engine honours and the expiry its window names — so one rule per class states the whole cost curve and a `permanent` class survives with transitions alone rather than dropping out with its infinite window; depth maps to the engine's own `TransitionStorageClass` through `_STORAGE`, total over the retention roster so a new depth breaks at that declaration instead of emitting a class the API accepts and ignores.
- Law: the reap's crash window closes TWICE by the same floor — the process arm walks live uploads past `_REAP_FLOOR` and the `AbortIncompleteMultipartUpload` rule enforces the identical age at the engine, so a runtime that dies between `CreateMultipartUpload` and its abort and never boots again still stops billing; the two read one value rather than two windows a reader reconciles, and the rule carries no tag filter because an abandoned upload has no object to tag.
- Law: restore is a typed DEFERRAL with a poll coordinate, never a blocking read — `restore(key, policy)` arms the retrieval and answers immediately, `head`'s own `Restore` header and `ArchiveStatus` are the coordinate a caller re-probes through `Stat.restoring`, and the retrieval tier rides the caller's policy because a DSAR deadline and a batch rehydration price the same bytes differently; a verb that waits holds a session permit for hours against a budget measured in seconds.
- Law: server-side projection over archived objects is REFUSED — `SelectObjectContentCommand` ships unmarked and stays uncomposed, because it cannot read an archived object at all (a restore is required first, so it answers none of the cold-tier need that alone justifies it), the plane's objects are opaque content-addressed bytes carrying no queryable schema, and the columnar projection this branch does own is `lane/olap.md`'s range-read Parquet scan at strictly better economics; the row is kept as data so the argument is not re-had.
- Law: the object's retention tag is DERIVED, never asserted — `_retag` folds the dominant class over the key's unreleased references (`Array.max` under `Order.mapInput(Duration.Order, ...)` over `Retain.Policy` windows, so `permanent`'s infinite window dominates every finite one), and `refer`/`release` compose it after their ledger write; an object holding any live longer-retention reference therefore can never carry a shorter tag, and no native expiry rule can match an object a live reference still protects.
- Law: the tag re-derivation runs in its OWN maintenance-plane transaction on the post-commit drain — `Retain.holding` joins FORCE-registered custody relations, so a session answers holds only under `Tenancy.sweep`: pinned, a foreign tenant's matter cannot freeze the shared object it holds and the re-derivation DOWNGRADES the `held` tag that tenant's declaration stamped; unpinned, every hold vanishes; and inline composition is unspellable anyway, because a plane `set_config` inside the caller's savepoint outlives the savepoint and widens the remaining transaction's visibility while the uncommitted reference stays invisible to any second session — post-commit is the one placement where the reference is durable and the posture is safe. `object_ref` itself registers no `Tenancy.rls` — it carries no tenant column, one content-keyed object serves every tenant, and attribution rides the owner grammar — so the sweep, the orphan census, and the reference read stay posture-free by design and the posture is owed to the hold JOIN alone.
- Law: GC is two-layer — the CAS reference sweep owns referential correctness (an object is reclaimable only when its last ledger reference released), and the S3-native lifecycle rules own time-windowed expiry by the derived retention tag, the belt-and-suspenders arm that survives total SQL-ledger loss; `permanent` emits no rule, and the rules generate from `Retain.Policy` so no window literal exists here.
- Law: the sweep is CAS end to end — probe the candidate's ETag, re-check the ledger inside the same pass, delete with `IfMatch: etag`, then settle the delete through `waitUntilObjectNotExists` so a lagging-consistency engine cannot re-list a half-dead key into the next pass — a re-mint racing the sweep wins structurally: the re-put lands 412-noop against the still-present object or recreates it after the delete, and the guarded delete refuses when bytes changed under the probe.
- Law: the reap closes the crash window the bracket cannot — a process death between `CreateMultipartUpload` and its abort leaves invisible parts billing forever; `reap(age)` walks `ListMultipartUploadsCommand` pages by `KeyMarker`/`UploadIdMarker` and aborts every upload whose `Initiated` predates the age floor, and the floor is the staging band's BACKSTOP rather than its exemption — a staged tus upload older than the floor aborts here exactly as the engine rule aborts it, so the stream rail's staging expiry sits at or below `_REAP_FLOOR` by law and a paused resumable upload dies by its own expiry before either reap can reach it.
- Law: `object_ref.owner` is a CLOSED vocabulary rather than a stated convention — `_OWNERS` seats one row per producer prefix carrying its coordinate grammar, its executed role, and the page that coins it; the prefix union and the `Owner` template-literal type derive from that table, `ObjectStore.owner` is the ONE mint every producer spends, and a prefix outside the roster is unspellable at the mint and refuses at the decode, so a sweep-and-DSAR hole cannot scan as clean. Every fresh producer seats its row HERE — naming its cascade and erasure answers — before its first reference row.
- Law: the role column is what the ledger EXECUTES — `cascade` (the `derivative:` row alone) drives the sweep's release of every reference a reclaimed key's own encoding owns, `dsar` (the `subject:` row alone) drives the portability scan and the hold join, and `custody` is plain attribution; a producer picking a role picks a mechanism, so a role read only by a reader is unrepresentable.
- Law: every coordinate segment percent-encodes through the ONE mint — a `:` inside a path, a `://` inside a remote origin, and a tenant carrying either otherwise re-split the owner a scan parses back and alias one producer's coordinate onto another's; the decode's pattern is that encoder's own alphabet, so a hand-built owner refuses rather than landing a row no join reaches. `journal/retain.md`'s custody projection mints the `subject:` row one stratum below, and its hold ledger writes the identical spelling into its own join column, so the pattern is the check both spellings meet.
- Law: a caller-supplied owner is INGRESS — `ObjectStore.admit` decodes it through the `Owner` schema and REFUSES the `subject:` prefix on the `owner` reason's caller-blamed class, because the custody projection is that prefix's sole mint and an upload declaring one forges a DSAR export and a hold join no subject authorized; `object/stream.md`'s tus create seam and `object/file.md`'s intake owner both compose it, and each falls back to its own row's mint rather than to a hand-built string.
- Law: the cascade is EXECUTED SQL and TRANSITIVE, never one hop — a swept key's reclaim walks the whole `derivative:` closure in one recursive term and releases every reference row inside it, the `released_at IS NULL` guard re-evaluated per row by the engine so a concurrent release is never overwritten off a value read before the write; a one-hop release left a grandchild holding a live reference to a parent already released, which is exactly the debt the orphan census then had to re-open on every later pass.
- Law: the cascade walks to CONVERGENCE and states it as a value — `Shape.Bound.fixpoint("hops")` names the case, the fixpoint arm is the walk's whole TYPE so a ceiling is unspellable here, and the recursive term carries no hop predicate because the `derivative:` graph is content-addressed: a product's owner names the digest of bytes that existed before the product's own, so every edge points strictly backward in mint order and no cycle can form. An adjacency without that proof declares the `finite` arm and refuses on `Fault.Class.spent` instead, which is the election `read/query#ORGANIZATION_ROWS` makes on its foreign-produced tree.
- Law: the reach answers the DISTANCE it computed — every released row carries the hop it sat at, so the sweep's `cascaded` census is evidence a multi-deep chain fell whole rather than a claim, and the walk pairs ONE recursive read with ONE guarded release inside one transaction because a data-modifying CTE would fuse them into a pg-only statement and this ledger carries a sqlite arm; the engine still walks once and the client re-walks nothing.
- Law: the cascade seat and the census offset DERIVE from the `_OWNERS` role column, exactly as the refused ingress seats do — the prefix a statement concatenates and the position the orphan census decodes from both move with that row, so the hand-counted character offset that once justified `substr(owner, 12)` in prose is gone and an unseated cascade role answers no statement at all; concatenating the seat in SQL and spending the mint in TypeScript agree because a content key is hex, the one segment the percent-encoding mint passes through unchanged.
- Law: the cascade's crash window heals on the LEDGER side — a death between the settled delete and its cascade leaves live `derivative:` references for a source key no listing revisits, so each sweep pass closes with one orphan census: live derivative references whose source key itself holds no live reference re-probe that source at the engine, an ABSENT source re-runs its cascade, and a present one — awaiting its own sweep, or re-minted and re-referred — keeps its derivatives whole; the census stays ONE hop precisely because the re-run is transitive, so it names the orphan roots and the walk closes everything beneath them in the same pass, and the re-run is the same per-row-guarded statement, so a double heal releases nothing twice.
- Law: the reference relation has ONE reader surface — `ObjectStore.references` is the published owner-keyed read of live references, satisfying `journal/retain.md`'s `RefRead` port for the DSAR objects leg and serving the maintenance seam's hold-lift walk, so no sibling plane spells `object_ref` SQL and a schema change here ripples through one contract; the handed owner decodes at this seam on the port's own `ParseError` channel, because a string arriving from the journal stratum is ingress like any other.
- Law: retention classes gate the sweep — a `permanent` reference never sweeps, windowed classes sweep past `Retain.Policy[class].lifetime.bound`, and subject-sealed payload objects fall to crypto-shredding upstream (key destruction makes the bytes unreadable; the sweep merely reclaims them).
- Law: the object tag vocabulary is the retention roster WITH one store-plane posture, and a live HOLD takes it — `held` is a TAG and never a `Retain.Class`, so the retention vocabulary stays closed at its own owner and no window prices a suspension carrying no clock; the retag fold composes `Retain.holding.owner` beside its dominance read and writes `held` for any key a held subject still references, no `_lifecycle` rule filters on that value, so the object freezes at whatever depth it already reached and neither transitions nor expires while the matter lives, and `Retain.lift` answers the lifted owners the maintenance seam walks back through the exposed `retag` onto the surviving classification. Re-tagging `permanent` is the deleted spelling: that class runs its own ladder to `frozen`, so the tag meant to protect litigation evidence put it hours behind a restore verb.

```typescript
import {
  AbortMultipartUploadCommand, DeleteObjectCommand, ListMultipartUploadsCommand, paginateListObjectsV2,
  PutBucketLifecycleConfigurationCommand, PutObjectTaggingCommand, RestoreObjectCommand, type TransitionStorageClass,
  waitUntilObjectNotExists,
} from "@aws-sdk/client-s3"
import { SqlClient, SqlSchema } from "@effect/sql"
import { Array, Chunk, HashMap, Option, Order, pipe, Record, Schema, Struct } from "effect"
import { Shape } from "@rasm/core"
import type { Capability } from "../lane/capability.ts"
import { Tenancy, Tenant } from "../lane/tenant.ts"
import { Journal } from "../journal/append.ts"
import { Retain } from "../journal/retain.ts"

const _REAP_FLOOR = Duration.days(1)

const _OWNERS = {
  derivative: { grammar: "derivative:<sourceKey>", role: "cascade", coined: "object/file#FANOUT" },
  disk: { grammar: "disk:<path>", role: "custody", coined: "object/file#FILE_PLANE" },
  event: { grammar: "event:<source>:<id>", role: "custody", coined: "object/store#EVENT_DATAREF" },
  lake: { grammar: "lake:<catalog>", role: "custody", coined: "lane/olap#ARROW_WIRE" },
  remote: { grammar: "remote:<scheme>:<host>:<path>", role: "custody", coined: "object/remote#OP_SURFACE" },
  subject: { grammar: "subject:<app>:<tenant>:<subject>", role: "dsar", coined: "journal/retain#SHREDDER" },
  tus: { grammar: "tus:<staging>", role: "custody", coined: "object/stream#RESUME_RAIL" },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly coined: string
  readonly grammar: string
  readonly role: "cascade" | "custody" | "dsar"
}>

const _SEGMENT = "[A-Za-z0-9\\-_.!~*'()%]+"

const _Owner = Schema.TemplateLiteral(Schema.Literal(...Struct.keys(_OWNERS)), ":", Schema.String).pipe(
  Schema.pattern(new RegExp(`^(?:${Array.join(Struct.keys(_OWNERS), "|")}):${_SEGMENT}(?::${_SEGMENT})*$`)),
  Schema.brand("ObjectOwner"),
)

const _MINTED = Array.filterMap(
  Record.toEntries(_OWNERS),
  ([prefix, row]) => row.role === "dsar" ? Option.some(`${prefix}:`) : Option.none(),
)

const _CASCADE = Option.map(
  Array.head(Array.filterMap(
    Record.toEntries(_OWNERS),
    ([prefix, row]) => row.role === "cascade" ? Option.some(prefix) : Option.none(),
  )),
  (prefix) => ({ seat: `${prefix}:`, offset: prefix.length + 2 }) as const,
)

const _CASCADE_BOUND: Shape.BoundFixpoint<"hops"> = Shape.Bound.fixpoint("hops")

declare namespace ObjectStore {
  type Cascaded = { readonly key: Digest.Key<"content">; readonly hops: number }
  type Owner = typeof _Owner.Type
  type Prefix = keyof typeof _OWNERS
  type Tag = Retain.Class | typeof _HELD
  type _Owners<
    T extends Record<Prefix, {
      readonly coined: string
      readonly grammar: string
      readonly role: "cascade" | "custody" | "dsar"
    }> = typeof _OWNERS,
  > = T
}

const _owner = <P extends ObjectStore.Prefix>(prefix: P, ...coordinate: Array.NonEmptyReadonlyArray<string>): ObjectStore.Owner =>
  _Owner.make(`${prefix}:${Array.join(Array.map(coordinate, encodeURIComponent), ":")}`)

const _admitted = (key: string) => (supplied: string): Effect.Effect<ObjectStore.Owner, ObjectFault> =>
  Effect.flatMap(
    Effect.mapError(
      Schema.decodeUnknown(_Owner)(supplied),
      () => new ObjectFault({ case: { reason: "owner", key, detail: "<owner:grammar>" } }),
    ),
    (custodian) =>
      Array.some(_MINTED, (seat) => custodian.startsWith(seat))
        ? Effect.fail(new ObjectFault({ case: { reason: "owner", key, detail: "<owner:minted-below>" } }))
        : Effect.succeed(custodian),
  )

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

const _byWindow: Order.Order<Retain.Class> = Order.mapInput(Duration.Order, (clazz: Retain.Class) => Retain.Policy[clazz].lifetime.bound)

const _STORAGE: { readonly [D in Retain.Depth]: TransitionStorageClass } = {
  cool: "STANDARD_IA",
  cold: "GLACIER_IR",
  frozen: "DEEP_ARCHIVE",
}

const _honours = (engine: ObjectStore.Engine) => {
  const cell: ObjectStore.Archive = _engines[engine].archive
  const ceiling = Array.findFirstIndex(Retain.depths, (depth) => depth === cell)
  return (rung: Retain.Rung): boolean =>
    Option.match(ceiling, { onNone: () => false, onSome: (at) => Retain.depths.indexOf(rung.depth) <= at })
}

const _HELD = "held" as const

const _classify = (client: S3Client, bucket: string) =>
  (key: Digest.Key<"content">, tag: ObjectStore.Tag) =>
    Effect.asVoid(Effect.tryPromise({
      try: (signal) =>
        client.send(new PutObjectTaggingCommand({
          Bucket: bucket, Key: key,
          Tagging: { TagSet: [{ Key: "retention", Value: tag }] },
        }), { abortSignal: signal }),
      catch: _foldedRead(key),
    }))

const _retag = (client: S3Client, bucket: string) =>
  (key: Digest.Key<"content">) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const evidence = yield* Tenancy.sweep(sql)(Effect.gen(function* () {
        const rows = yield* SqlSchema.findAll({
          Request: Schema.String,
          Result: Schema.Struct({ retention: Retain.Class }),
          execute: (who) => sql`SELECT DISTINCT retention FROM object_ref WHERE key = ${who} AND released_at IS NULL`,
        })(key)
        const held = yield* SqlSchema.single({
          Request: Schema.String,
          Result: Schema.Struct({ held: Journal.Version }),
          execute: (who) =>
            sql`SELECT count(*) AS held FROM object_ref r
                WHERE r.key = ${who} AND r.released_at IS NULL AND ${sql.literal(Retain.holding.owner("r"))}`,
        })(key)
        return { rows, held }
      }))
      yield* (Array.isNonEmptyReadonlyArray(evidence.rows)
        ? _classify(client, bucket)(key, evidence.held.held > 0 ? _HELD : Array.max(Array.map(evidence.rows, (row) => row.retention), _byWindow))
        : Effect.void)
    })

const _days = (span: Duration.Duration): number => Math.max(1, Math.trunc(Duration.toDays(span)))

const _lifecycle = (client: S3Client, bucket: string, engine: ObjectStore.Engine, policy: typeof Retain.Policy) =>
  Effect.asVoid(Effect.tryPromise({
    try: (signal) =>
      client.send(new PutBucketLifecycleConfigurationCommand({
        Bucket: bucket,
        LifecycleConfiguration: {
          Rules: [
            ...Array.filterMap(Record.toEntries(policy), ([clazz, row]) =>
              pipe(
                Array.filter(row.transitions, _honours(engine)),
                (rungs) =>
                  rungs.length === 0 && !Duration.isFinite(row.lifetime.bound)
                    ? Option.none()
                    : Option.some({
                        ID: `retain-${clazz}`,
                        Status: "Enabled" as const,
                        Filter: { Tag: { Key: "retention", Value: clazz } },
                        ...(rungs.length > 0 && {
                          Transitions: Array.map(rungs, (rung) => ({
                            Days: _days(rung.after),
                            StorageClass: _STORAGE[rung.depth],
                          })),
                        }),
                        ...(Duration.isFinite(row.lifetime.bound) && { Expiration: { Days: _days(row.lifetime.bound) } }),
                      }),
              )),
            {
              ID: "abort-incomplete",
              Status: "Enabled" as const,
              Filter: { Prefix: "" },
              AbortIncompleteMultipartUpload: { DaysAfterInitiation: _days(_REAP_FLOOR) },
            },
          ],
        },
      }), { abortSignal: signal }),
    catch: _foldedRead(bucket),
  }))

const _references: Retain.RefRead = (owner) =>
  Effect.flatMap(Schema.decodeUnknown(_Owner)(owner), (custodian) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      SqlSchema.findAll({
        Request: _Owner,
        Result: Schema.Struct({ key: Digest.Key.content, retention: Retain.Class }),
        execute: (who) => sql`SELECT key, retention FROM object_ref WHERE owner = ${who} AND released_at IS NULL`,
      })(custodian)))

const _refer = (key: Digest.Key<"content">, owner: ObjectStore.Owner, retention: Retain.Class) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`INSERT INTO object_ref ${sql.insert([{ key, owner, retention }])}
        ON CONFLICT (key, owner) DO UPDATE SET released_at = NULL, retention = excluded.retention`)

const _release = (key: Digest.Key<"content">, owner: ObjectStore.Owner) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`UPDATE object_ref SET released_at = ${Journal.now(sql)} WHERE key = ${key} AND owner = ${owner}`)

const _cascade = (sql: SqlClient.SqlClient, key: string, bound: Shape.BoundFixpoint<"hops">) =>
  Option.match(_CASCADE, {
    onNone: () => Effect.succeed<ReadonlyArray<ObjectStore.Cascaded>>([]),
    onSome: ({ seat }) =>
      sql.withTransaction(Effect.gen(function* () {
        const reach = yield* SqlSchema.findAll({
          Request: Schema.String,
          Result: Schema.Struct({ key: Digest.Key.content, hops: Schema.Int.pipe(Schema.nonNegative()) }),
          execute: (root) =>
            sql`WITH RECURSIVE reach(key, hops) AS (
                  SELECT CAST(${root} AS TEXT), 0
                  UNION
                  SELECT d.key, r.hops + 1 FROM object_ref d JOIN reach r ON d.owner = ${seat} || r.key
                   WHERE d.released_at IS NULL
                )
                SELECT key, min(hops) AS hops FROM reach GROUP BY key`,
        })(key)
        const released = yield* Array.match(reach, {
          onEmpty: () => Effect.succeed<ReadonlyArray<{ readonly key: Digest.Key<"content"> }>>([]),
          onNonEmpty: (rows) =>
            SqlSchema.findAll({
              Request: Schema.Void,
              Result: Schema.Struct({ key: Digest.Key.content }),
              execute: () =>
                sql`UPDATE object_ref SET released_at = ${Journal.now(sql)}
                     WHERE ${sql.in("owner", Array.map(rows, (row) => `${seat}${row.key}`))} AND released_at IS NULL
                    RETURNING key`,
            })(undefined),
        })
        const depth = HashMap.fromIterable(Array.map(reach, (row) => [row.key, row.hops] as const))
        return Array.filterMap(released, (row) =>
          Option.map(HashMap.get(depth, row.key), (hops): ObjectStore.Cascaded => ({ key: row.key, hops })))
      })).pipe(Effect.withSpan("data.cascade", { attributes: { key, bound: `${bound._tag}:${bound.unit}` } })),
  })

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

const _healed = (client: S3Client, bucket: string, sql: SqlClient.SqlClient) =>
  Option.match(_CASCADE, {
    onNone: () => Effect.void,
    onSome: ({ offset, seat }) =>
      Effect.flatMap(
        SqlSchema.findAll({
          Request: Schema.Void,
          Result: Schema.Struct({ source: Digest.Key.content }),
          execute: () =>
            sql`SELECT DISTINCT substr(d.owner, ${offset}) AS source FROM object_ref d
                WHERE d.owner LIKE ${`${seat}%`} AND d.released_at IS NULL
                  AND NOT EXISTS (SELECT 1 FROM object_ref s
                                   WHERE s.key = substr(d.owner, ${offset}) AND s.released_at IS NULL)`,
        })(undefined),
        (orphans) =>
          Effect.forEach(orphans, (row) =>
            Effect.matchEffect(_headed(client, bucket, row.source), {
              onFailure: (fault) =>
                fault.reason === "missing"
                  ? Effect.asVoid(_cascade(sql, row.source, _CASCADE_BOUND))
                  : Effect.fail(fault),
              onSuccess: () => Effect.void,
            }), { concurrency: 1, discard: true }),
      ),
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
        (cause) => new ObjectFault({ case: { reason: "io", key: bucket, detail: String(cause) } }),
      ),
      { probed: 0, swept: 0, cascaded: 0, reclaimed: 0, retained: 0 },
      (mark, page) =>
        Effect.reduce(page.Contents ?? [], mark, (held, entry) =>
          Effect.flatMap(
            live(entry.Key ?? ""),
            (count) =>
              count.live > 0 || entry.Key === undefined || entry.ETag === undefined
                ? Effect.succeed({ ...held, probed: held.probed + 1, retained: held.retained + 1 })
                : Effect.zipRight(
                    _sweepDelete(client, bucket, settleSeconds, entry.Key, entry.ETag),
                    _cascade(sql, entry.Key, _CASCADE_BOUND),
                  ).pipe(
                    Effect.map((released) => ({
                      probed: held.probed + 1,
                      swept: held.swept + 1,
                      cascaded: held.cascaded + released.length,
                      reclaimed: held.reclaimed + (entry.Size ?? 0),
                      retained: held.retained,
                    })),
                    Effect.catchTag("ObjectReplay", () =>
                      Effect.succeed({ ...held, probed: held.probed + 1, retained: held.retained + 1 })),
                  ),
          )),
    ).pipe(
      Effect.tap(() => _healed(client, bucket, sql)),
      Effect.withSpan("data.sweep", { attributes: { bucket } }),
    )
  })

const _restore = (client: S3Client, bucket: string) =>
  (key: Digest.Key<"content">, policy: ObjectStore.RestorePolicy) =>
    Effect.asVoid(Effect.tryPromise({
      try: (signal) =>
        client.send(new RestoreObjectCommand({
          Bucket: bucket, Key: key,
          RestoreRequest: { Days: policy.days, GlacierJobParameters: { Tier: policy.tier } },
        }), { abortSignal: signal }),
      catch: _foldedRead(key),
    }))
```

## [05]-[INSTRUMENT_ROWS]

- Owner: the object plane's Convention projections — `_measured`, the one receipt fold every write leg taps, and `_reclaimed`, the sweep-mark projection — instruments the runtime meter bridge exports like every other series while the receipts stay the billing and evidence truth.
- Packages: `effect` (`Metric`); `@rasm/core` (`Convention` — the instrument and tag rows).
- Entry: the service construction composes `_measured` as an `Effect.tap` on `put`, `putKeyed`, and `rekey`, and the sweep tail taps `_reclaimed` — zero call-site wiring, and no consumer can write an object the instruments miss.
- Growth: a receipt axis is one `Convention.instrument` row and one tap on the owning leg.
- Law: dedup rate DERIVES on the dashboard — the write counter tags each receipt's outcome (`written` versus `dedup`) from the bounded two-value vocabulary, so the rate is a ratio query over one series and no page computes it; bytes count only on `written: true` because a 412 noop moved no bytes, and reclaim counts the sweep mark's `reclaimed` BYTE total, never its key census — the convention row codes `By`, so a key count exported under that code reports objects in a series a reader spends as bytes.
- Law: instrument name, description, and tag key read off the `Convention` rows — no signal-site literal exists on the object plane, and identifier-grade context (the content key) rides span attributes on `data.sweep`/`data.grant`, never a metric tag.

```typescript
import { Metric } from "effect"
import { Convention } from "@rasm/core"

const _reclaimed = Convention.mount(Convention.metric.objectReclaimed)
const _weight = Convention.mount(Convention.metric.objectSize)
const _written = Convention.mount(Convention.metric.objectWritten)

const _measured = (receipt: ObjectStore.Receipt): Effect.Effect<void> =>
  Effect.zipRight(
    Metric.increment(Metric.tagged(_written, Convention.rasm.objectOutcome, receipt.written ? "written" : "dedup")),
    receipt.written ? Metric.incrementBy(_weight, receipt.bytes) : Effect.void,
  )
```

## [06]-[GRANT_MINT]

- Owner: `store.grant(key, command, policy?)` — one polymorphic mint over any command value: the command discriminates upload, download, part, or probe; the policy row narrows the TTL and carries the signed and hoisted header sets; the reply is the typed `{ url, expiresAt, key }` capability, never a bare string.
- Packages: `@aws-sdk/s3-request-presigner` (`getSignedUrl`, `signableHeaders`, `hoistableHeaders`); `effect` (`DateTime`, `Duration`).
- Entry: the grant's live consumers are IN-BRANCH reads — `lane/olap.md`'s `Olap.lakeSource` registers a presigned URL on a worker session so the browser scans cold-tail Parquet by HTTP range, and `object/file.md`'s derivative fan mints one per row whose own `grant` policy asks, every other row carrying `Option.none()` and its key; a browser-direct upload grant presigns the SAME conditional command `[3]` mints, so the idempotency and checksum headers survive into a browser path by construction, and the stream rail grants part uploads against its staging band.
- Receipt: the grant is a bounded bearer-equivalent capability — the mint is span-annotated because grants are auditable facts, and the value is `{ url, expiresAt, key }` its consumer spends directly.
- Law: this page mints capability and names NO serving seam — no surface outside `data` accepts or returns a presigned URL today, so a law promising one describes a consumer that does not exist, and a receipt minting one unconditionally pays for a signature nothing reads; the grant is therefore an in-branch capability its two readers spend, and per-principal quota is priced wherever a future serving edge lands, because that is where principal identity exists.
- Law: the capability keeps `ObjectStore.Grant` while it stays namespace-qualified and in-branch — `runtime/serve/live.md`'s `Admission.Grant` (a resolved live-channel rule) and the ui geolocation permission port spell the same word for unrelated concepts, which is harmless only because no consumer holds two; the moment a serving edge returns this value into a folder that spells bare `Grant` for authorization, the object capability takes the distinct name `ObjectStore.Ticket` BEFORE it crosses, so the rename rides the seam's arrival rather than a later collision nobody traces.
- Growth: a new presigned operation is a command value through the same entry; a signing posture (SSE-C pinning into `signableHeaders`, `Response*` hoisting into `hoistableHeaders`) is a `GrantPolicy` field, never a second mint.
- Law: config is inherited, never re-declared — the presigner reads the live client's resolved credentials, region, endpoint, and path style, so grants against any conforming engine are the same call and no second client exists; the published `provider` record carries its credential fields SEALED as `Redacted` values, and the one sanctioned unwrap is the staging store's own construction seam in `object/stream.md`.
- Law: `expiresIn` derives from `Duration.min(ttl, setting.presignTtl)` — a grant narrows policy and an unbounded or widened grant is unrepresentable at this surface.
- Law: the mint rides the `_shielded` bracket like every operation — bounded flight, the `lease` row's two deadlines, class-gated retry; caller-keyed grant QUOTA is not this page's, because per-principal identity exists only where a request does and this page mints capability, never authorization.

```typescript
import { DateTime } from "effect"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import type { GetObjectCommand, HeadObjectCommand, PutObjectCommand, UploadPartCommand } from "@aws-sdk/client-s3"

declare namespace ObjectStore {
  type Command = PutObjectCommand | GetObjectCommand | UploadPartCommand | HeadObjectCommand
  type Grant = { readonly url: string; readonly expiresAt: DateTime.Utc; readonly key: Digest.Key<"content"> }
  type GrantPolicy = {
    readonly ttl?: Duration.Duration
    readonly signableHeaders?: Set<string>
    readonly hoistableHeaders?: Set<string>
  }
}

const _grant = (client: S3Client, presignTtl: Duration.Duration) =>
  (key: Digest.Key<"content">, command: ObjectStore.Command, policy?: ObjectStore.GrantPolicy) =>
    Effect.gen(function* () {
      const bounded = policy?.ttl === undefined ? presignTtl : Duration.min(policy.ttl, presignTtl)
      const url = yield* Effect.tryPromise({
        try: () =>
          getSignedUrl(client, command, {
            expiresIn: Math.trunc(Duration.toSeconds(bounded)),
            ...(policy?.signableHeaders !== undefined && { signableHeaders: policy.signableHeaders }),
            ...(policy?.hoistableHeaders !== undefined && { hoistableHeaders: policy.hoistableHeaders }),
          }),
        catch: _foldedRead(key),
      })
      const minted = yield* DateTime.now
      return { url, expiresAt: DateTime.addDuration(minted, bounded), key } satisfies ObjectStore.Grant
    }).pipe(Effect.withSpan("data.grant", { attributes: { key } }))

class ObjectStore extends Effect.Service<ObjectStore>()("data/ObjectStore", {
  scoped: Effect.gen(function* () {
    const setting = yield* _Setting
    yield* Effect.filterOrFail(
      Effect.succeed(setting.engine),
      (engine) => _engines[engine].conditional === "yes",
      (engine) =>
        new ObjectFault({ case: { reason: "engine", key: setting.bucket, detail: `<engine:${engine}:${_engines[engine].conditional}>` } }),
    )
    const client = yield* Effect.acquireRelease(
      Effect.sync(() =>
        new S3Client({
          endpoint: setting.endpoint,
          region: setting.region,
          forcePathStyle: setting.forcePathStyle,
          maxAttempts: 1,
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
    const shield = _shielded(gate)
    const retag = _retag(client, setting.bucket)
    return {
      client,
      bucket: setting.bucket,
      engine: setting.engine,
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
      conditional: (key: Digest.Key<"content">) =>
        new PutObjectCommand({ Bucket: setting.bucket, Key: key, IfNoneMatch: "*", ChecksumAlgorithm: "SHA256" }),
      put: (bytes: Uint8Array) =>
        Effect.flatMap(Digest.mint("content", bytes), (key) =>
          shield(key)(
            bytes.byteLength <= setting.multipartThreshold
              ? _putPlain(client, setting.bucket, key, bytes)
              : _putMultipart(client, setting.bucket, key, bytes, setting.partBytes, setting.partFlight),
          )).pipe(Effect.tap(_measured)),
      putKeyed: (key: Digest.Key<"content">, body: ReadableStream<Uint8Array>, span: number, step?: (loaded: number) => void) =>
        _putStreaming(client, setting.bucket, key, body, setting.partBytes, setting.partFlight, span, step).pipe(Effect.tap(_measured)),
      get: (key: Digest.Key<"content">) => shield(key)(_got(client, setting.bucket, key)),
      head: (key: Digest.Key<"content">) => shield(key)(_headed(client, setting.bucket, key)),
      attributes: (key: Digest.Key<"content">) => shield(key)(_attributes(client, setting.bucket, key)),
      rekey: (source: Digest.Key<"content">, target: Digest.Key<"content">) =>
        shield(target)(_rekey(client, setting.bucket, source, target)).pipe(Effect.tap(_measured)),
      settled: (key: Digest.Key<"content">) => _settled(client, setting.bucket, key, setting.settleSeconds),
      sweep: _sweep(client, setting.bucket, setting.settleSeconds).pipe(
        Effect.tap((mark) => Metric.incrementBy(_reclaimed, mark.reclaimed)),
      ),
      reap: _reap(client, setting.bucket),
      restore: (key: Digest.Key<"content">, policy: ObjectStore.RestorePolicy) => shield(key)(_restore(client, setting.bucket)(key, policy)),
      grant: (key: Digest.Key<"content">, command: ObjectStore.Command, policy?: ObjectStore.GrantPolicy) =>
        shield(key)(_grant(client, setting.presignTtl)(key, command, policy)),
      lifecycle: _lifecycle(client, setting.bucket, setting.engine, Retain.Policy),
      refer: (key: Digest.Key<"content">, owner: ObjectStore.Owner, retention: Retain.Class) =>
        Effect.zipRight(
          _refer(key, owner, retention),
          Effect.flatMap(Tenant, (tenant) => tenant.afterCommit(Effect.ignoreLogged(retag(key)))),
        ),
      release: (key: Digest.Key<"content">, owner: ObjectStore.Owner) =>
        Effect.zipRight(
          _release(key, owner),
          Effect.flatMap(Tenant, (tenant) => tenant.afterCommit(Effect.ignoreLogged(retag(key)))),
        ),
      references: _references,
      retag,
      custody: _custody,
      observed: _observed(client, setting.bucket, setting.engine),
    }
  }),
}) {
  static readonly Owner = _Owner
  static readonly Stat = _Stat
  static readonly admit = _admitted
  static readonly owner = _owner
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectFault, ObjectStore }
```

## [07]-[EVENT_DATAREF]

- Owner: `Dataref` is the future-app claim-check port over `ObjectStore`: one construction policy fixes an HTTPS root and retention class; `externalize` proves the offered bytes mint the event's subject before conditionally landing and referring them, while `resolve` accepts only the exact canonical reference this root would mint, reads through the store's verified `get`, and proves an inline twin byte-equal when one was carried.
- Law: the reference is a receiver-resolvable URI-reference and never an object key alias: the canonical HTTPS URL derives from the configured root plus the lower-case subject, while the store key remains `Digest.Key<"content">`. Resolution performs no caller-directed fetch; a foreign origin, credentials, query, fragment, path escape, or reference whose terminal coordinate differs from `subject` refuses before the object plane reads.
- Law: `dataref` permits reference-only and dual carriage. Reference-only returns the verified resident bytes; dual carriage additionally remints the inline content and compares the complete octet sequence, so a digest collision cannot certify unequal information. The receipt states `reference | dual`; absence of both is unrepresentable.
- Law: `externalize` derives the ledger owner from `(source,id)` through the closed `event:` owner row and reads the retention posture from construction. Callers can neither invent an object-owner prefix nor shorten custody after publishing a reference. The application serving the configured HTTPS root authorizes retrieval; this data owner supplies custody and resolution only and never widens a principal.
- Growth: another residence engine satisfies `Dataref` behind the same port; another URI scheme is a policy case only when it preserves confinement and receiver resolution, never a branch inside a webhook.
- Packages: existing `effect`, core `Digest`, `ObjectStore`, and `Retain`; no new package or codec is admitted.

```typescript
import { Context, Effect, Layer, Option, type ParseResult, Schema } from "effect"
import { Digest, Fault } from "@rasm/core"

const _datarefFamily = Fault.Class.family(["address", "integrity"] as const, {
  address: Fault.Class.row({
    class: "denied",
    leg: "admission",
    detail: Schema.Struct({ reference: Schema.String }),
    render: ({ reference }) => `data reference refused — ${reference}`,
  }),
  integrity: Fault.Class.row({
    class: "breached",
    leg: "admission",
    detail: Schema.Struct({ reference: Schema.String }),
    render: ({ reference }) => `data reference content diverged — ${reference}`,
  }),
})

class DatarefFault extends Schema.TaggedError<DatarefFault>()("DatarefFault", {
  case: _datarefFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _datarefFamily.classOf(this.case.reason)
  }
  override get message(): string {
    return _datarefFamily.render(this.case)
  }
}

const _DatarefRoot = Schema.URL.pipe(Schema.filter((root) => (
  root.protocol === "https:"
  && root.username.length === 0
  && root.password.length === 0
  && root.search.length === 0
  && root.hash.length === 0
  && root.pathname.endsWith("/")
) || "<dataref-root-must-be-clean-https-directory>"))

class _DatarefPolicy extends Schema.Class<_DatarefPolicy>("Dataref.Policy")({
  root: _DatarefRoot,
  retention: Retain.Class,
}) {}

declare namespace Dataref {
  type Address = { readonly source: string; readonly id: string }
  type Externalize = Address & { readonly subject: Digest.Key<"content">; readonly bytes: Uint8Array }
  type Resolve = Address & {
    readonly subject: Digest.Key<"content">
    readonly reference: string
    readonly inline: Option.Option<Uint8Array>
  }
  type Receipt = {
    readonly subject: Digest.Key<"content">
    readonly reference: string
    readonly bytes: Uint8Array
    readonly carriage: "reference" | "dual"
  }
  type Externalized = ObjectStore.Receipt & { readonly reference: string }
  type Policy = typeof _DatarefPolicy.Type
  type Shape = {
    readonly externalize: (offered: Externalize) => Effect.Effect<Externalized, DatarefFault | ObjectFault>
    readonly resolve: (offered: Resolve) => Effect.Effect<Receipt, DatarefFault | ObjectFault>
  }
}

const _datarefFault = (reason: "address" | "integrity", reference: string): DatarefFault =>
  new DatarefFault({ case: { reason, reference } })

const _reference = (root: URL, subject: Digest.Key<"content">): string =>
  new URL(subject.toLowerCase(), root).href

const _sameBytes = (left: Uint8Array, right: Uint8Array): boolean =>
  left.byteLength === right.byteLength && left.every((byte, at) => right[at] === byte)

const _dataref = (store: ObjectStore, policy: Dataref.Policy): Dataref.Shape => ({
  externalize: (offered) => Effect.gen(function* () {
    const minted = yield* Digest.mint("content", offered.bytes)
    if (minted !== offered.subject) return yield* Effect.fail(_datarefFault("integrity", offered.subject))
    const landed = yield* store.put(offered.bytes)
    if (landed.key !== offered.subject) return yield* Effect.fail(_datarefFault("integrity", landed.key))
    yield* store.refer(
      offered.subject,
      ObjectStore.owner("event", offered.source, offered.id),
      policy.retention,
    )
    return { ...landed, reference: _reference(policy.root, offered.subject) }
  }),
  resolve: (offered) => Effect.gen(function* () {
    const expected = _reference(policy.root, offered.subject)
    if (offered.reference !== expected) return yield* Effect.fail(_datarefFault("address", offered.reference))
    const bytes = yield* store.get(offered.subject)
    yield* Option.match(offered.inline, {
      onNone: () => Effect.void,
      onSome: (inline) => Effect.flatMap(
        Digest.mint("content", inline),
        (minted) => minted === offered.subject && _sameBytes(inline, bytes)
          ? Effect.void
          : Effect.fail(_datarefFault("integrity", offered.reference)),
      ),
    })
    return {
      subject: offered.subject,
      reference: offered.reference,
      bytes,
      carriage: Option.isSome(offered.inline) ? "dual" : "reference",
    }
  }),
})

class Dataref extends Context.Tag("data/Dataref")<Dataref, Dataref.Shape>() {
  static readonly Policy = _DatarefPolicy
  static readonly live = (offered: unknown): Layer.Layer<Dataref, ParseResult.ParseError, ObjectStore> =>
    Layer.effect(
      Dataref,
      Effect.flatMap(Schema.decodeUnknown(_DatarefPolicy)(offered), (policy) =>
        Effect.map(ObjectStore, (store) => _dataref(store, policy))),
    )
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dataref, DatarefFault }
```

## [08]-[CUSTODY_CONTRACT]

- Owner: the object plane's half of the branch backend generation — `_descriptor`, the declared custody document whose stable bytes derive from the settled retention and conformance tables alone; `ObjectStore.custody`, the generated `Artifact` and `Capability` messages the branch composition folds into `Backend.compose`; and `ObjectStore.observed`, the realized-state read answering the membership a `Backend.Reading` unions.
- Packages: `@aws-sdk/client-s3` (`GetBucketVersioningCommand`, `GetBucketLifecycleConfigurationCommand`, `GetBucketEncryptionCommand`); `@rasm\/contracts/rasm/contracts/parity/parity_pb` (`ArtifactRole.OBJECT_CUSTODY`, `Provider.OBJECT_STORE`, `ArtifactSchema`, `CapabilitySchema`, `FailureRank`, `RestartClass`); `@rasm/core` (`Format.proto`); `lane/capability.md` (`Backend.Artifact`, `Backend.Capability` — contract authority stays there whole); `journal/retain.md` (`Retain.Policy`, `Retain.depths`).
- Entry: the branch composition root appends `ObjectStore.custody().artifact` to `Backend.Sources.artifacts` and its capabilities to `Backend.Sources.capabilities`, then folds `ObjectStore.observed` into the one `Backend.Reading` it hands `Backend.observe` — one admission verdict covers relational and object state together.
- Law: the descriptor is DECLARED custody, never provider state — conditional-put demand, unversioned posture, the reap floor, and one lifecycle row per retention class (transitions off the ladder, expiry off the bound) derive from `Retain.Policy`, `_STORAGE`'s ladder, and `_REAP_FLOOR`; operator coordinates — endpoint, bucket, region, credentials, and the engine row a deployment selects — never enter the preimage, so the generation moves only when the custody contract moves and a redeployment re-keys nothing.
- Law: the descriptor rows sort by class at encode, because the artifact content preimage reads a published order, never a container's own enumeration.
- Law: the engine is a REALIZATION fact — `observed` reads the conformance cell, the bucket's versioning status, and the realized lifecycle rule set, then answers granted canonical keys (`object.conditional`, `object.archive`, `object.lifecycle`) beside the custody artifact's presence; a bucket provisioned outside the generation therefore stops reading as compliant, which is the whole point of seating custody inside it.
- Law: a missing lifecycle configuration reads as EMPTY rules, never a fault — the engine answers 404 on a bucket that has none, and an absent rule set is exactly the unrealized state observation exists to report; a bucket carrying no default encryption answers 404 on the same shape and folds to the `none` posture, which is absence SPELLED rather than a mode fabricated for a read no probe took; every other fault stays on the rail.
- Law: the encryption axis is a MODE and never a key — the descriptor demands one of `none`, `AES256`, or `aws:kms`, because a KMS master key id is an operator coordinate exactly like the endpoint and the bucket, and admitting one into the preimage re-keys the whole generation on every rotation; `KMSMasterKeyID` and `BucketKeyEnabled` ride the OBSERVATION side alone, where realized state is read rather than declared, and the grant compares the observed default against the declared mode rather than narrowing a foreign algorithm into this vocabulary.
- Law: `object.lock` is REFUSED and the descriptor states the refusal as a cell — S3 Object Lock is configurable only on a VERSIONED bucket and this plane refuses versioning outright, since the key IS the content and a second version of one key is unrepresentable; the lock surface can therefore never be granted here whatever an operator enables, per-object litigation preservation rides `journal/retain.md`'s `legal_hold` ledger with the `held` tag posture `[04]` instead, and the cell keeps the argument settled the way the `archive: "none"` engine cells do.
- Law: recency stays the relational frontier's — every object reference lives in the ledger and an object absent for a landed reference surfaces on the sweep and the verified read, so the object plane contributes no `frontier` or `restoredIn` stamp and the one recovery verdict grades the store that carries them.
- Growth: a custody axis is one descriptor field beside one capability row and one observation read — the write-posture policy row `[03]` names lands here the same pass it lands on the command mints.
- Boundary: this cluster mints rows and reads state; contract authority, merge, collision, and admission stay `lane/capability.md`'s, and no schema mutation runs here.

```typescript
import { GetBucketEncryptionCommand, GetBucketLifecycleConfigurationCommand, GetBucketVersioningCommand } from "@aws-sdk/client-s3"
import {
  ArtifactRole,
  ArtifactSchema,
  CapabilitySchema,
  FailureRank,
  Provider,
  RestartClass,
} from "@rasm\/contracts/rasm/contracts/parity/parity_pb"
import { Format } from "@rasm/core"
import type { Backend } from "../lane/capability.ts"

const _CUSTODY_KEY = "object/custody"

declare namespace ObjectStore {
  type Sse = "none" | "AES256" | "aws:kms"
}

const _SSE: ObjectStore.Sse = "AES256"

const _DEEPEST = Array.last(
  Array.filter(Retain.depths, (depth) =>
    Array.some(Record.values(Retain.Policy), (row) => Array.some(row.transitions, (rung) => rung.depth === depth))),
)

const _capability = (
  key: string,
  requirement: string,
  requirementValue: string,
  failureRank: Backend.FailureRank,
  restartClass: Backend.RestartClass,
): Backend.Capability => Format.proto.create(CapabilitySchema, {
  key,
  lane: "object",
  requirement,
  requirementValue,
  failureRank,
  restartClass,
})

const _CUSTODY_CAPABILITIES: ReadonlyArray<Backend.Capability> = [
  _capability("object.conditional", "conditional-put", "if-none-match-*", FailureRank.REQUIRED, RestartClass.RESTART),
  _capability(
    "object.archive",
    "archive-depth",
    Option.getOrElse(_DEEPEST, () => "none"),
    FailureRank.DEGRADABLE,
    RestartClass.RESTART,
  ),
  _capability("object.encryption", "sse-mode", _SSE, FailureRank.DEGRADABLE, RestartClass.RESTART),
  _capability("object.lifecycle", "lifecycle-rules", "retain-classes", FailureRank.DEGRADABLE, RestartClass.SESSION),
]

const _descriptor = () => ({
  conditional: "if-none-match-*",
  versioning: "unversioned",
  encryption: _SSE,
  lock: "refused",
  reapDays: _days(_REAP_FLOOR),
  lifecycle: Array.map(
    Array.sort(Record.toEntries(Retain.Policy), Order.mapInput(Order.string, ([clazz]: readonly [string, Retain.Row]) => clazz)),
    ([clazz, row]) => ({
      clazz,
      transitions: Array.map(row.transitions, (rung) => ({ afterDays: _days(rung.after), depth: rung.depth })),
      expireDays: Duration.isFinite(row.lifetime.bound) ? _days(row.lifetime.bound) : null,
    }),
  ),
})

const _custody = (): {
  readonly artifact: Backend.Artifact
  readonly capabilities: ReadonlyArray<Backend.Capability>
} => ({
  artifact: Format.proto.create(ArtifactSchema, {
    key: _CUSTODY_KEY,
    role: ArtifactRole.OBJECT_CUSTODY,
    content: new TextEncoder().encode(JSON.stringify(_descriptor())),
    providers: [Provider.OBJECT_STORE],
    dependsOn: [],
  }),
  capabilities: _CUSTODY_CAPABILITIES,
})

const _observed = (client: S3Client, bucket: string, engine: ObjectStore.Engine) =>
  Effect.gen(function* () {
    const versioning = yield* Effect.tryPromise({
      try: (signal) => client.send(new GetBucketVersioningCommand({ Bucket: bucket }), { abortSignal: signal }),
      catch: _foldedRead(bucket),
    })
    const rules = yield* Effect.tryPromise({
      try: (signal) => client.send(new GetBucketLifecycleConfigurationCommand({ Bucket: bucket }), { abortSignal: signal }),
      catch: _foldedRead(bucket),
    }).pipe(
      Effect.map((reply) => reply.Rules ?? []),
      Effect.catchAll((fault) => fault.reason === "missing" ? Effect.succeed([]) : Effect.fail(fault)),
    )
    const encryption = yield* Effect.tryPromise({
      try: (signal) => client.send(new GetBucketEncryptionCommand({ Bucket: bucket }), { abortSignal: signal }),
      catch: _foldedRead(bucket),
    }).pipe(
      Effect.map((reply) =>
        Option.flatMapNullable(
          Array.head(reply.ServerSideEncryptionConfiguration?.Rules ?? []),
          (rule) => rule.ApplyServerSideEncryptionByDefault?.SSEAlgorithm,
        )),
      Effect.catchAll((fault) => fault.reason === "missing" ? Effect.succeedNone : Effect.fail(fault)),
    )
    const ids = HashSet.fromIterable(Array.filterMap(rules, (rule) => Option.fromNullable(rule.ID)))
    const expected = [
      ...Array.filterMap(Record.toEntries(Retain.Policy), ([clazz, row]) =>
        Array.filter(row.transitions, _honours(engine)).length > 0 || Duration.isFinite(row.lifetime.bound)
          ? Option.some(`retain-${clazz}`)
          : Option.none()),
      "abort-incomplete",
    ]
    const unversioned = versioning.Status === undefined
    const cell = Array.findFirstIndex(Retain.depths, (depth) => depth === _engines[engine].archive)
    const holds: ReadonlyArray<readonly [key: string, held: boolean]> = [
      ["object.conditional", _engines[engine].conditional === "yes"],
      ["object.archive", Option.match(_DEEPEST, {
        onNone: () => true,
        onSome: (deepest) => Option.match(cell, { onNone: () => false, onSome: (at) => at >= Retain.depths.indexOf(deepest) }),
      })],
      ["object.encryption", Option.match(encryption, { onNone: () => _SSE === "none", onSome: (held) => held === _SSE })],
      ["object.lifecycle", Array.every(expected, (id) => HashSet.has(ids, id))],
    ]
    return {
      granted: HashSet.fromIterable(Array.filterMap(holds, ([key, held]) => held ? Option.some(key) : Option.none())),
      artifacts: unversioned ? HashSet.make(_CUSTODY_KEY) : HashSet.empty<string>(),
    }
  })
```

## [09]-[RESEARCH]

(none)
