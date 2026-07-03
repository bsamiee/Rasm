# [STORE_PRESIGN]

Presigned capability tokens and the derivative fan-out, both content-addressed end to end: `Presign.grant` is ONE mint over any command value — the command discriminates upload, download, part, or probe, never a per-operation family — inheriting the object plane's resolved client so provider facts exist once; the derivative fan-out decodes a stored image exactly once (`sharp(buffer)` over the single-consume body), `clone()`s per derivative-spec row, encodes through the one `toFormat(row.format, row.options)` dispatch, mints each derivative's own `ContentKey` through the kernel, re-puts it under the same conditional-put idempotency (If-None-Match; 412 = idempotent noop), and hands back one presigned GetObject grant per row. Untrusted input is gated before decode — `failOn`, `limitInputPixels`, blocked loaders — because every upload is hostile; sharp is native and server-plane only, and its Promise terminals exist solely inside this page's `tryPromise` seams.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                  |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `GRANT_MINT`         | the one presign entry, the TTL policy, the typed `{url, expiresAt}` carrier   |
|  [02]   | `DERIVATIVE_FANOUT`  | the spec-row roster, the gated decode, the clone/encode/re-put/grant fold     |

## [2]-[GRANT_MINT]

- Owner: `Presign.grant(key, command, ttl?)` — one polymorphic mint over `PutObjectCommand | GetObjectCommand | UploadPartCommand | HeadObjectCommand` values; the command discriminates the operation, the key rides into the receipt, the TTL rides `ObjectStore`'s config owner, and every grant returns the typed pair, never a bare string.
- Packages: `@aws-sdk/s3-request-presigner` (`getSignedUrl`); `@aws-sdk/client-s3` (the command values); `effect` (`DateTime`, `Duration`).
- Entry: browser-direct upload is `Presign.grant(key, store.conditional(key))` — the SAME conditional-put command mint the server put sends, so the idempotency and checksum headers survive into the browser path by construction; download grants hoist `Response*` overrides into the query through `hoistableHeaders`.
- Receipt: `Presign.Grant` — `{ url, expiresAt, key }` — a bounded bearer-equivalent capability; `security` reasons about it as a token, `edge` returns it as the object-access grant, and the mint is span-annotated because grants are auditable facts.
- Growth: a new presigned operation is a new command value through the same entry; a new signing posture (SSE-C pinning) is a `signableHeaders` policy row on the options, never a second mint.
- Law: the TTL is `Config`-bounded — `ObjectStore`'s `presignTtl` is the default and a per-grant ttl may only narrow it (`Duration.min`); an unbounded or widened grant is unrepresentable at this surface.
- Law: config is inherited, never re-declared — `getSignedUrl` reads the live client's resolved `credentials`/`region`/`endpoint`/`forcePathStyle`, so MinIO/R2/Tigris presigns are the same call and no second client exists.
- Law: `_signed` narrows the command union through `instanceof` control flow before the SDK call — `getSignedUrl`'s generic binds one concrete command input, so the union collapses per arm; `Match.instanceOf` deliberately subtracts nothing and cannot type the terminal arm here, which is why the chain is the checker-proven spelling.

```typescript
import { Data, DateTime, Duration, Effect, Option } from "effect"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import { GetObjectCommand, HeadObjectCommand, PutObjectCommand, type S3Client, UploadPartCommand } from "@aws-sdk/client-s3"
import type { ContentKey } from "@rasm/ts/kernel"
import { ObjectStore } from "./key.ts"

class _PresignFault extends Data.TaggedError("PresignFault")<{
  readonly stage: "gate" | "decode" | "encode" | "grant"
  readonly key: ContentKey | string
  readonly detail: string
}> {}

declare namespace Presign {
  type Command = PutObjectCommand | GetObjectCommand | UploadPartCommand | HeadObjectCommand
  type Grant = {
    readonly url: string
    readonly expiresAt: DateTime.Utc
    readonly key: ContentKey
  }
}

const _signed = (client: S3Client, command: Presign.Command, expiresIn: number): Promise<string> =>
  command instanceof PutObjectCommand
    ? getSignedUrl(client, command, { expiresIn })
    : command instanceof GetObjectCommand
      ? getSignedUrl(client, command, { expiresIn })
      : command instanceof UploadPartCommand
        ? getSignedUrl(client, command, { expiresIn })
        : getSignedUrl(client, command, { expiresIn })

const _grant = (key: ContentKey, command: Presign.Command, ttl?: Duration.Duration) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const bounded = ttl === undefined ? store.presignTtl : Duration.min(ttl, store.presignTtl)
    const url = yield* Effect.tryPromise({
      try: () => _signed(store.client, command, Math.trunc(Duration.toMillis(bounded) / 1000)),
      catch: (defect) => new _PresignFault({ stage: "grant", key, detail: String(defect) }),
    })
    const minted = yield* DateTime.now
    return {
      url,
      expiresAt: DateTime.addDuration(minted, bounded),
      key,
    } satisfies Presign.Grant
  }).pipe(Effect.withSpan("store.presign", { attributes: { key } }))
```

## [3]-[DERIVATIVE_FANOUT]

- Owner: the `Presign.Spec` roster row shape, the `_GATE` untrusted-input policy, and `Presign.fanout(key, specs)` — the whole decode-once/clone-per-row/encode/re-put/grant fold with its receipt per row.
- Packages: `sharp` (the factory, `clone`, `resize`, `autoOrient`, `toFormat`, `toBuffer({ resolveWithObject: true })`, `metadata`, `stats`, `timeout`, `sharp.block`); `object/key.md` (`ObjectStore.get`/`put`); `@rasm/ts/kernel` (`ContentKey` — every derivative is itself content-addressed).
- Entry: `Presign.fanout(sourceKey, specs)` — the object plane fetches the source bytes once (`get` already verified identity), and each spec row yields `{ name, grant, info }`; re-running the fanout is a proven noop end to end because every derivative re-put lands 412.
- Receipt: per-row `Presign.Derivative` — the spec name, the derivative's own grant, and sharp's `OutputInfo` (format, size, dimensions) as codec provenance; `stats().dominant` rides the receipt as the placeholder color when the spec asks.
- Growth: a new rendition is one roster row `{ name, resize, format, options }` — thumbnail, preview, master are rows, and no per-format method ladder exists because `toFormat` is the one codec dispatch.
- Law: decode once, clone N — the source `Body` is single-consume, so bytes buffer once, `sharp(buffer, _GATE)` decodes once, and `clone()` snapshots the decoded pipeline per row; a re-piped stream per derivative is the named waste.
- Law: the gate precedes the decode — `failOn: "warning"`, `limitInputPixels`, `unlimited: false`, `autoOrient: true`, a `timeout` per pipeline, and `sharp.block({ operation })` over the blocked-loader roster applied once at module admission; user bytes never reach an ungated loader.
- Law: derivative identity is the kernel mint over the ENCODED bytes — a derivative is a first-class object with its own key, its own `object_ref` row (owned by the source key), and its own grant; sharp owns codec work only, never addressing or idempotency.
- Boundary: sharp is server-plane native — the `./server` subpath owns this cluster; the browser consumes grants, never the codec; delivery beyond the grant (CDN, cache headers) is `edge` material.

```typescript
import sharp, { type FormatEnum, type OutputInfo, type OutputOptions, type ResizeOptions, type SharpOptions } from "sharp"

declare namespace Presign {
  type Spec = {
    readonly name: string
    readonly resize: ResizeOptions
    readonly format: keyof FormatEnum
    readonly options?: OutputOptions
    readonly placeholder?: boolean
  }
  type Derivative = {
    readonly name: string
    readonly grant: Grant
    readonly info: OutputInfo
    readonly dominant: Option.Option<{ readonly r: number; readonly g: number; readonly b: number }>
  }
}

const _GATE = {
  failOn: "warning",
  limitInputPixels: 268_402_689,
  unlimited: false,
  autoOrient: true,
} as const satisfies SharpOptions

const _DEADLINE = { seconds: 20 } as const

const _blockedLoaders: Array<string> = []

const _fanout = (sourceKey: ContentKey, specs: ReadonlyArray<Presign.Spec>) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    yield* Effect.when(
      Effect.sync(() => sharp.block({ operation: _blockedLoaders })),
      () => _blockedLoaders.length > 0,
    )
    const bytes = yield* store.get(sourceKey)
    const decoded = yield* Effect.try({
      try: () => sharp(Buffer.from(bytes), _GATE).timeout(_DEADLINE),
      catch: (defect) => new _PresignFault({ stage: "decode", key: sourceKey, detail: String(defect) }),
    })
    return yield* Effect.forEach(specs, (spec) =>
      Effect.gen(function* () {
        const encoded = yield* Effect.tryPromise({
          try: () =>
            decoded
              .clone()
              .resize(spec.resize)
              .toFormat(spec.format, spec.options)
              .toBuffer({ resolveWithObject: true }),
          catch: (defect) => new _PresignFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
        })
        const dominant = spec.placeholder === true
          ? Option.some((yield* Effect.tryPromise({
              try: () => decoded.clone().stats(),
              catch: (defect) => new _PresignFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
            })).dominant)
          : Option.none<{ readonly r: number; readonly g: number; readonly b: number }>()
        const receipt = yield* store.put(new Uint8Array(encoded.data), {
          contentType: `image/${spec.format}`,
          retention: "operational",
        })
        const grant = yield* _grant(
          receipt.key,
          new GetObjectCommand({ Bucket: store.bucket, Key: receipt.key }),
        )
        return { name: spec.name, grant, info: encoded.info, dominant } satisfies Presign.Derivative
      }), { concurrency: 4 })
  }).pipe(Effect.withSpan("store.fanout", { attributes: { source: sourceKey } }))

const Presign = {
  grant: _grant,
  fanout: _fanout,
  gate: _GATE,
  blocked: _blockedLoaders,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Presign }
```
