# [DATA_FILE]

The filesystem plane and the derivative codec, both over the one content identity: files enter and leave the process through the platform `FileSystem` capability — content-addressed intake streams a file through the incremental digest fold into a conditional put, temp staging is scoped acquisition, and a watched directory is a `Stream` of admission events — and the image derivative fan-out decodes a stored object exactly once, `clone()`s per derivative-spec row, encodes through the one `toFormat` dispatch, mints each derivative's own `ContentKey` through the core digest, re-puts under the same conditional-put idempotency (412 = noop), and hands back one presigned grant per row. Untrusted input is gated before decode — `failOn`, pixel limits, blocked loaders, a per-pipeline deadline — because every upload is hostile; sharp is native and server-plane only, and its Promise terminals exist solely inside this page's boundary seams. A new rendition is a roster row; a new intake source is a lift; no per-format method ladder and no second addressing vocabulary exist anywhere on the plane.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                        |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `FILE_PLANE`      | content-addressed intake, scoped temp staging, the watch stream, egress            |
|  [02]   | `CODEC_GATE`      | the untrusted-input posture and the module governance rows                         |
|  [03]   | `DERIVATIVE_ROWS` | the spec roster, decode-once/clone-N, the `toFormat` dispatch, receipts            |
|  [04]   | `FANOUT`          | the fetch → gate → encode → mint → conditional re-put → grant fold                 |

## [2]-[FILE_PLANE]

- Owner: `Disk` — the file-side verbs over the platform capability Tags: `intake` (file → identity fold → conditional put → reference row), `stage` (scoped temp file whose teardown rides the `Scope`), `watch` (a directory as a `Stream` of intake admissions), `egress` (content object → file sink, streamed).
- Packages: `@effect/platform` (`FileSystem.FileSystem` — `stream`, `sink`, `watch`, `makeTempFileScoped`, `stat`; `Path.Path`); `effect` (`Stream`, `Effect`); `object/stream.md` (`Rail.bytes`, `Rail.identity` — the one identity fold), `object/store.md` (`ObjectStore` — the conditional legs).
- Entry: an artifact producer lands its output through `Disk.intake(path, retention)`; a peer-runtime handoff directory rides `Disk.watch(dir)` feeding the same intake; a served export streams out through `Disk.egress(key, path)` — every verb yields the platform Tags on `R` and the runtime binding stays a root row.
- Receipt: intake answers the object receipt plus the file's `stat` evidence — `{ key, bytes, written, path }` — so a producer logs one row tying the filesystem coordinate to the durable key.
- Growth: a new intake posture (move-after-intake, verify-only) is an options field; a new source (an archive member walk) is one more lift into the same fold.
- Law: intake never buffers the file — identity is content-addressed, so the key cannot exist before the last byte is hashed, and intake is therefore TWO bounded streaming passes over the seekable file: `fs.stream(path)` feeds the `Rail` identity fold, then a fresh `fs.stream(path)` feeds the streaming conditional put — constant memory at any size; a `readFile` single-pass intake is the memory defect the rail already bans.
- Law: temp staging is scoped — `makeTempFileScoped` ties the temp file's deletion to the `Scope`, so an interrupted derivative pass or a failed intake leaks nothing; a hand-managed temp path is the rejected spelling.
- Law: the watch stream is admission, not truth — a watched drop directory emits candidate paths, each admitted through the same gated intake, and a malformed or duplicate file settles as its intake receipt (412 dedup included) rather than a watcher error.
- Law: direct `node:fs` imports are banned on this plane — capability rides the Tag, the tracing and error rail come with it, and the binding package is the only place a platform module name exists.

```typescript
import { Effect, Stream } from "effect"
import { FileSystem, Path } from "@effect/platform"
import { ContentKey } from "@rasm/ts/core"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Disk {
  type Intake = { readonly key: ContentKey; readonly bytes: number; readonly written: boolean; readonly path: string }
}

const _intake = (path: string, retention: Retain.Class) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const store = yield* ObjectStore
    const flow = fs.stream(path).pipe(
      Stream.mapError((fault) => new ObjectFault({ reason: "io", key: path, detail: fault.message })),
    )
    const identity = yield* Rail.identity(Rail.chunked(flow, Rail.cut))
    const landed = yield* store.putKeyed(
      identity.key,
      yield* Stream.toReadableStreamEffect(fs.stream(path)),
    )
    yield* store.refer(identity.key, `disk:${path}`, retention)
    return { key: identity.key, bytes: identity.bytes, written: landed.written, path } satisfies Disk.Intake
  })

const _watch = (dir: string, retention: Retain.Class) =>
  Stream.unwrap(
    Effect.map(FileSystem.FileSystem, (fs) =>
      fs.watch(dir).pipe(
        Stream.filter((event) => event._tag === "Create"),
        Stream.mapEffect((event) => _intake(event.path, retention), { concurrency: 2 }),
      )),
  )

const _stage = Effect.flatMap(FileSystem.FileSystem, (fs) => fs.makeTempFileScoped())

const _egress = (key: ContentKey, path: string) =>
  Effect.flatMap(FileSystem.FileSystem, (fs) => Stream.run(Rail.range(key), fs.sink(path)))
```

## [3]-[CODEC_GATE]

- Owner: the untrusted-input posture — the `_GATE` ingress options, the blocked-loader roster applied once at module admission, the per-pipeline deadline — and the module governance rows (`cache`, `concurrency`) that bound the native runtime.
- Packages: `sharp` (`SharpOptions` — `failOn`, `limitInputPixels`, `unlimited`, `autoOrient`; `sharp.block`, `sharp.cache`, `sharp.concurrency`, `timeout`).
- Entry: every decode on this plane opens through `_GATE`; the loader block runs once at service construction so no ungated call site can exist afterward.
- Growth: an admitted loader is a roster edit (an empty roster blocks nothing); a workload class with its own pixel ceiling is a second gate row selected by the fan-out's caller, never an inline override.
- Law: the gate precedes the decode — `failOn: "warning"` aborts on suspect input, `limitInputPixels` bounds decompression exposure, `unlimited` stays false, `autoOrient` normalizes EXIF rotation, and a `timeout` rides every pipeline; user bytes never reach an ungated loader.
- Law: governance is process policy — the libvips operation cache and the threadpool width are service-construction facts from configuration, because the derivative plane shares its process with the serving plane and unbounded native concurrency starves it.
- Law: sharp is server-plane native — no browser or wasm path imports it; the browser consumes grants, never the codec.

```typescript
import sharp, { type SharpOptions } from "sharp"

const _GATE = {
  failOn: "warning",
  limitInputPixels: 268_402_689,
  unlimited: false,
  autoOrient: true,
} as const satisfies SharpOptions

const _DEADLINE = { seconds: 20 } as const

const _governed = (options: { readonly blockedLoaders: ReadonlyArray<string>; readonly cacheMb: number; readonly threads: number }) =>
  Effect.sync(() => {
    if (options.blockedLoaders.length > 0) sharp.block({ operation: [...options.blockedLoaders] })
    sharp.cache({ memory: options.cacheMb })
    sharp.concurrency(options.threads)
  })
```

## [4]-[DERIVATIVE_ROWS]

- Owner: the `Derive.Spec` roster row shape and the per-row receipt — a rendition is `{ name, resize, format, options, placeholder }`, the codec dispatch is `toFormat(row.format, row.options)`, and the receipt carries sharp's `OutputInfo` provenance plus the dominant color when the row asks.
- Packages: `sharp` (`clone`, `resize`, `toFormat`, `toBuffer({ resolveWithObject: true })`, `metadata`, `stats`, `FormatEnum`, `ResizeOptions`, `OutputOptions`, `OutputInfo`).
- Entry: an app declares its rendition roster once (`thumbnail`/`preview`/`master` rows) and hands it to the fan-out; format capability gates through `sharp.format` at construction so an unbuildable row refuses at boot, never per request.
- Receipt: `Derive.Receipt` — `{ name, key, grant, info, dominant }` — the row name, the derivative's own content key, its presigned grant, the codec provenance, and the optional placeholder color seeded from `stats().dominant`.
- Growth: a new rendition is one roster row; a new decision input (per-row format selection from `metadata()`) is a row predicate reading the pre-decode metadata, never a code path per format.
- Law: `toFormat` is the one codec dispatch — the per-format methods are aliases it generalizes, and a `jpeg()`/`png()`/`webp()` ladder is the named defect; row options carry quality/effort/lossless per codec.
- Law: `metadata()` and `stats()` are the decision reads — pre-decode format/geometry select or veto rows (an SVG source never reaches a raster row unless admitted), and pixel analysis feeds the placeholder — both lifted once per fan-out, never per row.

```typescript
import type { FormatEnum, OutputInfo, OutputOptions, ResizeOptions } from "sharp"
import { DateTime, Option } from "effect"

declare namespace Derive {
  type Spec = {
    readonly name: string
    readonly resize: ResizeOptions
    readonly format: keyof FormatEnum
    readonly options?: OutputOptions
    readonly placeholder?: boolean
  }
  type Receipt = {
    readonly name: string
    readonly key: ContentKey
    readonly grant: { readonly url: string; readonly expiresAt: DateTime.Utc; readonly key: ContentKey }
    readonly info: OutputInfo
    readonly dominant: Option.Option<{ readonly r: number; readonly g: number; readonly b: number }>
  }
}
```

## [5]-[FANOUT]

- Owner: `Derive.fanout(sourceKey, specs)` — the whole fold: verified fetch, gated single decode, clone-per-row encode, per-derivative key mint, conditional re-put, reference row owned by the source, presigned grant — plus `DeriveFault`, the plane's stage-discriminated fault.
- Packages: `sharp`; `object/store.md` (`ObjectStore.get`/`put`/`grant`, the reference verbs); `@rasm/ts/core` (`ContentKey` — every derivative is itself content-addressed).
- Entry: `Derive.fanout(sourceKey, roster)` after an image lands (an intake receipt, an upload finalize); re-running is a proven noop end to end because every re-put lands 412 and every grant re-mints against the same keys.
- Receipt: one `Derive.Receipt` per row; the batch's span carries source key, row count, and total encode span.
- Growth: watermarking is a `composite` step on the row's chain read from the spec; a tile-pyramid rendition is a row whose terminal is `tile` — both land inside the fold as row-driven steps.
- Law: decode once, clone N — the verified source bytes buffer once (`get` already re-minted identity), `sharp(buffer, _GATE)` decodes once, and `clone()` snapshots the decoded pipeline per row; a re-decode or re-piped stream per derivative is the named waste.
- Law: derivative identity is the core mint over the ENCODED bytes — each derivative is a first-class object with its own key, its own reference row owned by the source key (so source release cascades), and its own grant; sharp owns codec work only, never addressing or idempotency.
- Law: the fold is total over `DeriveFault` — stages `gate | decode | encode | grant` carry the failing coordinate; a single row's encode fault fails that row's receipt and the batch policy (abort versus partition) is the caller's accumulation decision, stated at the call.

```typescript
import { Data } from "effect"
import { GetObjectCommand } from "@aws-sdk/client-s3"

class DeriveFault extends Data.TaggedError("DeriveFault")<{
  readonly stage: "gate" | "decode" | "encode" | "grant"
  readonly key: string
  readonly detail: string
}> {}

const _fanout = (sourceKey: ContentKey, specs: ReadonlyArray<Derive.Spec>) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const bytes = yield* store.get(sourceKey)
    const decoded = yield* Effect.try({
      try: () => sharp(Buffer.from(bytes), _GATE).timeout(_DEADLINE),
      catch: (defect) => new DeriveFault({ stage: "decode", key: sourceKey, detail: String(defect) }),
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
          catch: (defect) => new DeriveFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
        })
        const dominant = spec.placeholder === true
          ? Option.some((yield* Effect.tryPromise({
              try: () => decoded.clone().stats(),
              catch: (defect) => new DeriveFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
            })).dominant)
          : Option.none<{ readonly r: number; readonly g: number; readonly b: number }>()
        const landed = yield* store.put(new Uint8Array(encoded.data))
        yield* store.refer(landed.key, `derivative:${sourceKey}`, "operational")
        const grant = yield* store.grant(landed.key, new GetObjectCommand({ Bucket: store.bucket, Key: landed.key }))
        return { name: spec.name, key: landed.key, grant, info: encoded.info, dominant } satisfies Derive.Receipt
      }), { concurrency: 4 })
  }).pipe(Effect.withSpan("data.fanout", { attributes: { source: sourceKey } }))

const Disk = {
  intake: _intake,
  watch: _watch,
  stage: _stage,
  egress: _egress,
} as const

const Derive = {
  gate: _GATE,
  governed: _governed,
  fanout: _fanout,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Derive, DeriveFault, Disk }
```
