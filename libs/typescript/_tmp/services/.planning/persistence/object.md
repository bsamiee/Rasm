# [SERVICES_OBJECT]

The first-class blob/object tier collapsing the scattered S3 presign and the asset-export codec fan-out into one owner — `ObjectStore`, the single `Effect.Service` over the `@effect-aws/client-s3` `S3Service` layer-as-service binding, never a raw `@aws-sdk/client-s3` command-style call in domain code and never a sibling uploader per asset kind. Every read, write, multipart streaming upload, presign, lifecycle sweep, and encoded-asset emission rides this one service: `S3Service.getObject`/`putObject` (including the `{ presigned: true }` typed-URL overload), the full multipart surface (`createMultipartUpload`/`uploadPart`/`completeMultipartUpload`/`abortMultipartUpload`), and the paginated `S3Service$.listObjectsV2Stream` the `LifecyclePolicy` expiry sweep walks. An object is addressed by an `ObjectKey` — a content-addressed `[ValueObject]` brand minted by the same `XxHash128` seed-zero regime the `interchange:interchange/Codec/parity#CONTENT_KEY_PARITY` golden-fixture corpus is keyed by (`folder:interchange/Codec/parity#CONTENT_KEY_PARITY` is the cross-runtime byte-identity owner; this page mints the same digest over upload bytes so a stored object and a wire fixture that share bytes share an address, never a second content-address notion). A presign is one `PresignGrant` `Data.TaggedEnum` row over the verb axis (`Get`/`Put`/`Multipart`), each carrying its own TTL and verb scope, gated by the `tenancy#TENANCY` `app.current_tenant` GUC and folded by one `Match.tagsExhaustive` so a new presign verb breaks every grant site at compile time. The asset-export codec fan-out the `work#WORK_AND_SIGNALS` page once carried migrates here: the four export codecs (`papaparse`/`exceljs`/`jspdf`/`jszip`) collapse into one `AssetCodec` `Schema.Literal("csv","xlsx","pdf","archive")` axis encoding into one streaming `ObjectStore.put`, with `sharp` the in-pipeline image-transform codec (not a fifth export literal); a `work#WORK_AND_SIGNALS` `Job` whose payload is an asset export enqueues the work and this owner streams the encoded bytes. This surface crosses no .NET wire and is node-only — the browser receives only a `PresignGrant` URL and performs a plain HTTP GET/PUT against it, never reaching the credentialed service. This is the net-new `object-store/` sub-domain's one page.

## [01]-[INDEX]

- [01]-[OBJECT_STORE]: owns the `ObjectStore` `Effect.Service` over `S3Service`, the `ObjectKey` content-addressed brand, the `PresignGrant` verb axis, the `AssetCodec` streaming fan-out, and the `LifecyclePolicy` rows over `listObjectsV2Stream`.

## [02]-[OBJECT_STORE]

- Owner: `ObjectStore`, the one `Effect.Service` wrapping the `@effect-aws/client-s3` `S3Service` tag for every object operation; `ObjectKey`, the content-addressed `[ValueObject]` brand minted by `XxHash128` seed-zero over the upload bytes (the `interchange:interchange/Codec/parity#CONTENT_KEY_PARITY` seed regime); `PresignGrant`, the closed `Data.TaggedEnum` over the presign verb axis (`Get`/`Put`/`Multipart`) folded by one `Match.tagsExhaustive`; `AssetCodec`, the one `Schema.Literal("csv","xlsx","pdf","archive")` export-format axis fanning into one streaming `put`, with `sharp` the in-pipeline image-transform codec; `LifecyclePolicy`, the `as const satisfies Record` storage-class vocabulary keyed by tier carrying the expiry/transition discipline the `listObjectsV2Stream` sweep reads; `ObjectFault`, the one `Schema.TaggedError` policy projection over the typed `@effect-aws/client-s3` error rail.
- Cases: `ObjectStore.put` is the one write entrypoint — small objects go through one `S3Service.putObject({ Bucket, Key, Body })`, and a stream whose length crosses the multipart threshold drives the full multipart sequence as one `Effect.acquireUseRelease`: `createMultipartUpload` captures the `UploadId`, each `Stream` chunk uploads through `uploadPart({ UploadId, PartNumber, Body })` accumulating its `{ PartNumber, ETag }` into the `CompletedPart` list, `completeMultipartUpload({ UploadId, MultipartUpload: { Parts } })` finalizes, and any failure in `use` re-routes to `abortMultipartUpload({ UploadId })` in `release` so a half-uploaded object never strands its parts — the multipart lifecycle is one bracketed resource, never a parallel timer or an orphaned upload. `ObjectStore.get` is one `S3Service.getObject({ Bucket, Key })` returning the body stream, or one `S3Service$.headObject` for a metadata-only existence probe. `ObjectStore.delete` is one `S3Service$.deleteObject`. The `ObjectKey` brand is content-addressed: `objectKeyOf(hasher)(bytes)` feeds the upload bytes through `createXXHash128(0, 0)` `init`/`update`/`digest("binary")`, normalizes the 16-byte little-endian digest to the canonical big-endian key with `.reverse()` (the `hash-wasm` `[ENDIANNESS_LAW]` the `interchange` corpus pins), and lifts the result into the branded slot — so two uploads of identical bytes resolve to one stored object and the store is naturally deduplicated, and an object that shares bytes with a wire fixture shares the fixture's content address. A presign is one `PresignGrant` row dispatched by `Match.tagsExhaustive`: `Get` drives `S3Service.getObject({ Bucket, Key }, { presigned: true, expiresIn })` returning the typed `Effect<string>` download URL, `Put` drives `S3Service.putObject({ Bucket, Key }, { presigned: true, expiresIn })` returning the upload URL, and `Multipart` returns the per-part presigned URLs by first calling `createMultipartUpload` for the `UploadId`, then presigning each `UploadPartCommand` through `@aws-sdk/s3-request-presigner` `getSignedUrl` over the raw `S3ClientInstance` client — `uploadPart` carries NO `{ presigned: true }` overload, so the overload covers only the single-object `Get`/`Put` verbs and the `Multipart` per-part presign is the sibling presigner's job. Each verb carries its own TTL. Every grant resolves inside the `tenancy#TENANCY` `withTenant` GUC scope so the bucket/key prefix is RLS-scoped to `app.current_tenant` and a tenant never signs another tenant's prefix. The `AssetCodec` fan-out is one `encode(format)(rows)` dispatched by `Match` over the closed `Schema.Literal` axis — `csv` through `Papa.unparse` (with `escapeFormulae` set so a cell starting with `=`/`+`/`-`/`@` cannot inject into a spreadsheet), `xlsx` through an `exceljs` `Workbook` whose `xlsx.writeBuffer()` serializes in-memory, `pdf` through a `jsPDF` instance whose `output("arraybuffer")` emits the document bytes, and `archive` through a `JSZip` instance whose `generateNodeStream({ type: "nodebuffer" })` streams the ZIP — each encoder produces a byte source that flows straight into one `ObjectStore.put`, never four parallel exporters and never a buffered round-trip through disk; `sharp` is the in-pipeline transform an image asset passes through before `put` (one `sharp(input).resize(...).toFormat(...).toBuffer()` chain), a transform codec on the upload path, not a fifth export literal. The `LifecyclePolicy` sweep walks `S3Service$.listObjectsV2Stream({ Bucket, Prefix })` as one paginated `Stream`, and for each page folds the `LifecyclePolicy` row for the object's tier — an object past its `expiresAfter` is deleted, an object past its `transitionAfter` is re-storage-classed — so the expiry/transition discipline is data on the policy table the sweep reads, never a sibling lifecycle service.
- Auto: `LifecyclePolicy` is the one `as const satisfies Record` vocabulary keyed by storage tier (`hot`/`warm`/`cold`/`ephemeral`), each row carrying the `expiresAfter` retention `Duration`, the `transitionAfter` cold-storage `Duration` (or `undefined` for tiers that never transition), the S3 `StorageClass` token the transition targets, and the `multipartThreshold` byte cap above which `put` switches to the multipart path — read by `keyof typeof` indexed access so a `Match` chain re-deriving per-tier retention is the deleted form; the sweep reads `policy.expiresAfter`/`policy.transitionAfter` straight off the row keyed by the object's tier, so the policy row IS the lifecycle decision, never a second constant.
- Entry: `ObjectStore` is one `Effect.Service` whose layer carries the `S3Service` tag and the raw `S3ClientInstance` (both provided once at the composition root by `S3Service.layer(config)` / `S3ClientInstance.layer` with the explicit region/endpoint/credentials, or `S3Service.defaultLayer` under the standard credential chain — `S3ClientInstance` is the raw `S3Client` the `Multipart` per-part `getSignedUrl` presign signs over) and the resolved `IHasher` (`createXXHash128(0, 0)` resolved once at the composition root and threaded through the service, never re-resolved per `put` — the `interchange` `[LOCAL_ADMISSION]` mint discipline); the S3 credentials themselves resolve through the `security/secret#SECRET_STORE` `SecretStore` (`SecretStore.resolve(SecretRef.Doppler(...))` for the access keys), never a bare `process.env` read. The asset-export path composes the durable tier: a `work#WORK_AND_SIGNALS` `Job` whose payload is an export request drains off the `WorkQueue`, the worker calls `AssetCodec.encode(format)(rows)` and streams the result through `ObjectStore.put`, and the resulting `ObjectKey` is written back onto the job row — this page owns the codec and the streamed upload, that page owns the work enqueue. The `LifecyclePolicy` sweep registers as a `execution/backplane#RUNNER_AND_SCHEDULING` `ScheduledWork.singleton` so exactly one runner walks each tenant's prefix through `listObjectsV2Stream` and applies the expiry/transition rows; an `AssetTransferFault` an export job surfaces is keyed by format and `encode`/`object-store` stage so the `work#WORK_AND_SIGNALS` page that enqueues the job can route the failure without re-owning the codec.
- Wire: the owner crosses no .NET wire and carries no wire type — `ObjectKey` is this folder's content-address brand (the same `XxHash128` digest the C# `interchange:interchange/Codec/parity#CONTENT_KEY_PARITY` seed mints, reproduced over upload bytes, never a second key scheme), the `GetObjectCommandOutput`/`PutObjectCommandOutput`/`CompletedPart` shapes are the `@effect-aws/client-s3` provider types (not a .NET wire), and the C# branch owns no object-store seam this page decodes; the browser receives only the `PresignGrant` URL string and performs a plain credential-less HTTP GET/PUT against it, never reaching the credentialed `S3Service`. The `@effect/platform` `Multipart` streaming-upload boundary (the public-edge ingress parser that decodes a browser multipart/form-data upload into a `Stream` of parts before `ObjectStore.put`) is an orchestrator-owned branch-`.api` dependency this page references but does not catalogue: the `S3Service` multipart wrapper already owns the S3-side multipart sequence, so the only branch-`.api` gap is the ingress-side `@effect/platform` `Multipart` parser the public edge would feed this owner, which the public-edge ingress concern owns, not this page.
- Packages: `@effect-aws/client-s3` for the `S3Service` tag (`S3Service.layer`/`defaultLayer` at the composition root, `getObject`/`putObject` including the `{ presigned: true }` overload, the full multipart `createMultipartUpload`/`uploadPart`/`completeMultipartUpload`/`abortMultipartUpload`, `S3Service$.headObject`/`deleteObject`, and `S3Service$.listObjectsV2Stream` for the lifecycle sweep — `S3ClientInstance` is the raw-client interop the `Multipart` per-part presign signs over); `@aws-sdk/s3-request-presigner` `getSignedUrl` for the `Multipart` per-part `UploadPartCommand` presign the `{ presigned: true }` overload does not reach (the overload covers only single-object `getObject`/`putObject`), never the primary path; `hash-wasm` `createXXHash128(0, 0)` for the content-addressed `ObjectKey` mint (the same seed-zero regime the `interchange` corpus is keyed by); `papaparse`/`exceljs`/`jspdf`/`jszip` as the four `AssetCodec` export codecs and `sharp` as the in-pipeline image transform; `@effect/sql`/`@effect/sql-pg` for the object-metadata `Model.Class` row through `store#STORE_BOUNDARY` and the tenancy GUC; `@effect/cluster` for the `ScheduledWork.singleton` the sweep registers on; `effect` for the `S3Service` composition, the `PresignGrant` `Data.TaggedEnum` dispatch, the `AssetCodec`/`LifecyclePolicy` vocabularies, the multipart `Stream`/`acquireUseRelease` bracket, and the `ObjectFault` policy projection.
- Growth: a new presign verb lands as one `PresignGrant` `Data.TaggedEnum` variant breaking the `Match.tagsExhaustive` fold at compile time, never a parallel presign method; a new export format lands as one `AssetCodec` `Schema.Literal` literal plus one `Match` encode arm, never a sibling exporter; a new storage tier lands as one `LifecyclePolicy` row keyed by tier, never a second lifecycle service; a new object operation lands as one `S3Service` method call on the one service, never a sibling uploader; a new content-address consumer reads the same `ObjectKey` brand, never a second digest function.
- Boundary: the named defects — a raw `@aws-sdk/client-s3` command-style call in domain code instead of the `S3Service` tag (the wrapper owns the typed-error rail and the `*Stream` pagination); a hand-rolled `getSignedUrl` flow where the `{ presigned: true }` overload covers the verb; a multipart upload whose failed parts strand instead of routing to `abortMultipartUpload` in the bracket `release`; four parallel asset exporters instead of the one `AssetCodec` `Schema.Literal` fan-out into one streaming `put`; a fifth `image` export literal instead of `sharp` as the in-pipeline transform; a second content-address scheme instead of the shared `XxHash128` seed-zero brand; a raw little-endian digest hex compared against the big-endian C# key instead of the `[ENDIANNESS_LAW]` byte-order normalize at the brand boundary; a presign that skips the tenancy-GUC prefix scope, letting a tenant sign another tenant's key; a sibling lifecycle service instead of the `LifecyclePolicy` rows walked over `listObjectsV2Stream`; a buffered codec round-trip through disk instead of the streamed `put`. This is a node-only surface, never browser-reachable beyond the issued presign URL.

```ts contract
import type { CompletedPart, GetObjectCommandOutput, PutObjectCommandOutput } from "@aws-sdk/client-s3"
import { UploadPartCommand } from "@aws-sdk/client-s3"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import { S3ClientInstance, S3Service } from "@effect-aws/client-s3"
import { createXXHash128, type IHasher } from "hash-wasm"
import { Model, SqlClient, SqlError } from "@effect/sql"
import Papa from "papaparse"
import ExcelJS from "exceljs"
import { jsPDF } from "jspdf"
import JSZip from "jszip"
import sharp from "sharp"
import { Brand, Chunk, Data, Duration, Effect, Match, Option, Schema as S, Stream } from "effect"

// --- [TYPES] -----------------------------------------------------------------------------

const ASSET_FORMATS = ["csv", "xlsx", "pdf", "archive"] as const
type AssetFormat = (typeof ASSET_FORMATS)[number]
const AssetFormat = S.Literal(...ASSET_FORMATS)

type StorageTier = "hot" | "warm" | "cold" | "ephemeral"

type PresignGrant = Data.TaggedEnum<{
  readonly Get:       { readonly bucket: string; readonly key: ObjectKey; readonly ttl: Duration.Duration }
  readonly Put:       { readonly bucket: string; readonly key: ObjectKey; readonly ttl: Duration.Duration; readonly contentType: string }
  readonly Multipart: { readonly bucket: string; readonly key: ObjectKey; readonly ttl: Duration.Duration; readonly parts: number }
}>
const PresignGrant = Data.taggedEnum<PresignGrant>()

// --- [CONSTANTS] -------------------------------------------------------------------------

const ObjectStorePolicy = {
  hot:       { expiresAfter: Duration.days(365), transitionAfter: Duration.days(90),  storageClass: "STANDARD_IA", multipartThreshold: 8 * 1024 * 1024 },
  warm:      { expiresAfter: Duration.days(180), transitionAfter: Duration.days(30),  storageClass: "GLACIER_IR",  multipartThreshold: 8 * 1024 * 1024 },
  cold:      { expiresAfter: Duration.days(90),  transitionAfter: undefined,          storageClass: "DEEP_ARCHIVE", multipartThreshold: 16 * 1024 * 1024 },
  ephemeral: { expiresAfter: Duration.days(7),   transitionAfter: undefined,          storageClass: "STANDARD",    multipartThreshold: 8 * 1024 * 1024 },
} as const satisfies Record<StorageTier, {
  readonly expiresAfter: Duration.Duration
  readonly transitionAfter: Duration.Duration | undefined
  readonly storageClass: string
  readonly multipartThreshold: number
}>

const ObjectFaultPolicy = {
  upload_failed:    { status: 502, retryable: true,  ord: 0 },
  multipart_abort:  { status: 502, retryable: true,  ord: 1 },
  presign_denied:   { status: 403, retryable: false, ord: 2 },
  encode_failed:    { status: 500, retryable: false, ord: 3 },
  object_missing:   { status: 404, retryable: false, ord: 4 },
  lifecycle_failed: { status: 502, retryable: true,  ord: 5 },
} as const satisfies Record<string, { status: number; retryable: boolean; ord: number }>

type ObjectFaultReason = keyof typeof ObjectFaultPolicy

// --- [MODELS] ----------------------------------------------------------------------------

type ObjectKey = string & Brand.Brand<"ObjectKey">
const ObjectKey = Brand.refined<ObjectKey>(
  (s) => /^[0-9a-f]{32}$/.test(s),
  (s) => Brand.error(`ObjectKey must be a 32-char hex XxHash128 digest, got ${s}`),
)

class StoredObject extends Model.Class<StoredObject>("StoredObject")({
  key: S.String,
  appId: S.UUID,
  bucket: S.String,
  tier: S.Literal("hot", "warm", "cold", "ephemeral"),
  contentType: S.String,
  byteLength: S.Number,
  format: S.optionalWith(AssetFormat, { as: "Option" }),
  storedAt: Model.DateTimeInsert,
}) {}

// --- [ERRORS] ----------------------------------------------------------------------------

class ObjectFault extends S.TaggedError<ObjectFault>()("ObjectFault", {
  reason: S.Literal(...(Object.keys(ObjectFaultPolicy) as [ObjectFaultReason, ...Array<ObjectFaultReason>])),
  key: S.String,
  cause: S.Unknown,
}) {
  get status()    { return ObjectFaultPolicy[this.reason].status }
  get retryable() { return ObjectFaultPolicy[this.reason].retryable }
}

class AssetTransferFault extends S.TaggedError<AssetTransferFault>()("AssetTransferFault", {
  format: AssetFormat,
  stage: S.Literal("encode", "object-store"),
  cause: S.Unknown,
}) {}

// --- [SERVICES] --------------------------------------------------------------------------

interface ObjectStore {
  readonly put: (input: { readonly bucket: string; readonly tier: StorageTier; readonly contentType: string; readonly body: Stream.Stream<Uint8Array, never> }) => Effect.Effect<ObjectKey, ObjectFault, S3Service>
  readonly get: (bucket: string, key: ObjectKey) => Effect.Effect<GetObjectCommandOutput, ObjectFault, S3Service>
  readonly head: (bucket: string, key: ObjectKey) => Effect.Effect<boolean, ObjectFault, S3Service>
  readonly delete: (bucket: string, key: ObjectKey) => Effect.Effect<void, ObjectFault, S3Service>
  readonly presign: (grant: PresignGrant) => Effect.Effect<string | ReadonlyArray<string>, ObjectFault, S3Service>
  readonly encode: (format: AssetFormat) => (rows: ReadonlyArray<Record<string, unknown>>) => Effect.Effect<Uint8Array, AssetTransferFault>
  readonly sweep: (bucket: string, prefix: string, tier: StorageTier) => Effect.Effect<number, ObjectFault | SqlError.SqlError, S3Service>
}

class ObjectStoreService extends Effect.Service<ObjectStoreService>()("services/ObjectStore", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const hasher = yield* Effect.promise(() => createXXHash128(0, 0))
    const s3Client = yield* S3ClientInstance

    const put: ObjectStore["put"] = ({ bucket, tier, contentType, body }) =>
      Effect.gen(function* () {
        const bytes = yield* Stream.runFold(body, new Uint8Array(0), concatBytes)
        const key = objectKeyOf(hasher)(bytes)
        const policy = ObjectStorePolicy[tier]
        yield* bytes.byteLength > policy.multipartThreshold
          ? multipartPut(bucket, key, contentType, bytes)
          : S3Service.putObject({ Bucket: bucket, Key: key, Body: bytes, ContentType: contentType, StorageClass: policy.storageClass }).pipe(
              Effect.mapError((cause) => new ObjectFault({ reason: "upload_failed", key, cause })),
            )
        yield* sql`INSERT INTO stored_object (key, app_id, bucket, tier, content_type, byte_length) VALUES (${key}, current_setting('app.current_tenant')::uuid, ${bucket}, ${tier}, ${contentType}, ${bytes.byteLength})`.pipe(
          Effect.mapError((cause) => new ObjectFault({ reason: "upload_failed", key, cause })),
        )
        return key
      })

    const get: ObjectStore["get"] = (bucket, key) =>
      S3Service.getObject({ Bucket: bucket, Key: key }).pipe(
        Effect.mapError((cause) => new ObjectFault({ reason: "object_missing", key, cause })),
      )

    const head: ObjectStore["head"] = (bucket, key) =>
      S3Service.headObject({ Bucket: bucket, Key: key }).pipe(
        Effect.as(true),
        Effect.catchTag("NotFoundError", () => Effect.succeed(false)),
        Effect.mapError((cause) => new ObjectFault({ reason: "object_missing", key, cause })),
      )

    const del: ObjectStore["delete"] = (bucket, key) =>
      S3Service.deleteObject({ Bucket: bucket, Key: key }).pipe(
        Effect.asVoid,
        Effect.mapError((cause) => new ObjectFault({ reason: "object_missing", key, cause })),
      )

    const presign: ObjectStore["presign"] = (grant) =>
      Match.value(grant).pipe(
        Match.tagsExhaustive({
          Get: ({ bucket, key, ttl }) =>
            S3Service.getObject({ Bucket: bucket, Key: key }, { presigned: true, expiresIn: Duration.toSeconds(ttl) }).pipe(
              Effect.mapError((cause) => new ObjectFault({ reason: "presign_denied", key, cause })),
            ),
          Put: ({ bucket, key, ttl, contentType }) =>
            S3Service.putObject({ Bucket: bucket, Key: key, ContentType: contentType }, { presigned: true, expiresIn: Duration.toSeconds(ttl) }).pipe(
              Effect.mapError((cause) => new ObjectFault({ reason: "presign_denied", key, cause })),
            ),
          Multipart: ({ bucket, key, ttl, parts }) =>
            S3Service.createMultipartUpload({ Bucket: bucket, Key: key }).pipe(
              Effect.mapError((cause) => new ObjectFault({ reason: "presign_denied", key, cause })),
              Effect.flatMap((started) =>
                Effect.forEach(rangeOf(parts), (PartNumber) =>
                  Effect.tryPromise({
                    try: () => getSignedUrl(s3Client, new UploadPartCommand({ Bucket: bucket, Key: key, UploadId: started.UploadId!, PartNumber }), { expiresIn: Duration.toSeconds(ttl) }),
                    catch: (cause) => new ObjectFault({ reason: "presign_denied", key, cause }),
                  }),
                ),
              ),
            ),
        }),
      )

    const encode: ObjectStore["encode"] = (format) => (rows) =>
      Match.value(format).pipe(
        Match.when("csv", () =>
          Effect.try({
            try: () => new TextEncoder().encode(Papa.unparse(rows as ReadonlyArray<Record<string, unknown>>, { escapeFormulae: true })),
            catch: (cause) => new AssetTransferFault({ format, stage: "encode", cause }),
          }),
        ),
        Match.when("xlsx", () =>
          Effect.tryPromise({
            try: async () => {
              const wb = new ExcelJS.Workbook()
              const ws = wb.addWorksheet("export")
              ws.columns = Object.keys(rows[0] ?? {}).map((header) => ({ header, key: header }))
              ws.addRows(rows)
              return new Uint8Array(await wb.xlsx.writeBuffer())
            },
            catch: (cause) => new AssetTransferFault({ format, stage: "encode", cause }),
          }),
        ),
        Match.when("pdf", () =>
          Effect.try({
            try: () => {
              const doc = new jsPDF()
              const headers = Object.keys(rows[0] ?? {})
              doc.table(10, 10, rows as Array<Record<string, string>>, headers, { autoSize: true })
              return new Uint8Array(doc.output("arraybuffer"))
            },
            catch: (cause) => new AssetTransferFault({ format, stage: "encode", cause }),
          }),
        ),
        Match.when("archive", () =>
          Effect.tryPromise({
            try: async () => {
              const zip = new JSZip()
              rows.forEach((row, i) => zip.file(`row-${i}.json`, JSON.stringify(row)))
              return await zip.generateAsync({ type: "uint8array", compression: "DEFLATE" })
            },
            catch: (cause) => new AssetTransferFault({ format, stage: "encode", cause }),
          }),
        ),
        Match.exhaustive,
      )

    const sweep: ObjectStore["sweep"] = (bucket, prefix, tier) =>
      S3Service.listObjectsV2Stream({ Bucket: bucket, Prefix: prefix }).pipe(
        Stream.mapError((cause) => new ObjectFault({ reason: "lifecycle_failed", key: prefix, cause })),
        Stream.flatMap((page) => Stream.fromIterable(page.Contents ?? [])),
        Stream.filterMapEffect((entry) => expireOrTransition(bucket, tier, entry.Key!, entry.LastModified)),
        Stream.runCount,
      )

    return { put, get, head, delete: del, presign, encode, sweep } as const
  }),
}) {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const objectKeyOf = (hasher: IHasher) => (bytes: Uint8Array): ObjectKey =>
  ObjectKey(hasher.init().update(bytes).digest("binary").reverse().reduce((hex, b) => hex + b.toString(16).padStart(2, "0"), ""))

const concatBytes = (acc: Uint8Array, next: Uint8Array): Uint8Array => {
  const merged = new Uint8Array(acc.byteLength + next.byteLength)
  merged.set(acc, 0)
  merged.set(next, acc.byteLength)
  return merged
}

const rangeOf = (n: number): ReadonlyArray<number> => Array.from({ length: n }, (_, i) => i + 1)

const PART_SIZE = 8 * 1024 * 1024

const multipartPut = (bucket: string, key: ObjectKey, contentType: string, bytes: Uint8Array): Effect.Effect<void, ObjectFault, S3Service> =>
  Effect.acquireUseRelease(
    S3Service.createMultipartUpload({ Bucket: bucket, Key: key, ContentType: contentType }).pipe(
      Effect.mapError((cause) => new ObjectFault({ reason: "upload_failed", key, cause })),
    ),
    (started) =>
      Effect.forEach(chunkBy(bytes, PART_SIZE), ([partNumber, slice]) =>
        S3Service.uploadPart({ Bucket: bucket, Key: key, UploadId: started.UploadId!, PartNumber: partNumber, Body: slice }).pipe(
          Effect.map((out): CompletedPart => ({ PartNumber: partNumber, ETag: out.ETag })),
        ),
      ).pipe(
        Effect.flatMap((parts) =>
          S3Service.completeMultipartUpload({ Bucket: bucket, Key: key, UploadId: started.UploadId!, MultipartUpload: { Parts: parts } }),
        ),
        Effect.asVoid,
        Effect.mapError((cause) => new ObjectFault({ reason: "upload_failed", key, cause })),
      ),
    (started, exit) =>
      exit._tag === "Failure"
        ? S3Service.abortMultipartUpload({ Bucket: bucket, Key: key, UploadId: started.UploadId! }).pipe(Effect.ignore)
        : Effect.void,
  )

const chunkBy = (bytes: Uint8Array, size: number): ReadonlyArray<readonly [number, Uint8Array]> =>
  Chunk.toReadonlyArray(
    Chunk.map(
      Chunk.range(0, Math.ceil(bytes.byteLength / size) - 1),
      (i) => [i + 1, bytes.subarray(i * size, (i + 1) * size)] as const,
    ),
  )

const expireOrTransition = (bucket: string, tier: StorageTier, key: string, lastModified: Date | undefined): Effect.Effect<Option.Option<void>, ObjectFault, S3Service> => {
  const policy = ObjectStorePolicy[tier]
  const age = Date.now() - (lastModified?.getTime() ?? Date.now())
  return age >= Duration.toMillis(policy.expiresAfter)
    ? S3Service.deleteObject({ Bucket: bucket, Key: key }).pipe(
        Effect.as(Option.some<void>(undefined)),
        Effect.mapError((cause) => new ObjectFault({ reason: "lifecycle_failed", key, cause })),
      )
    : policy.transitionAfter !== undefined && age >= Duration.toMillis(policy.transitionAfter)
      ? S3Service.copyObject({ Bucket: bucket, Key: key, CopySource: `${bucket}/${key}`, StorageClass: policy.storageClass }).pipe(
          Effect.as(Option.some<void>(undefined)),
          Effect.mapError((cause) => new ObjectFault({ reason: "lifecycle_failed", key, cause })),
        )
      : Effect.succeed(Option.none<void>())
}

const transformImage = (input: Uint8Array, format: keyof sharp.FormatEnum, width: number): Effect.Effect<Stream.Stream<Uint8Array, never>, ObjectFault> =>
  Effect.tryPromise({
    try: () => sharp(input).resize(width).toFormat(format).toBuffer().then((b) => new Uint8Array(b)),
    catch: (cause) => new ObjectFault({ reason: "encode_failed", key: "", cause }),
  }).pipe(Effect.map((bytes) => Stream.make(bytes)))
```
