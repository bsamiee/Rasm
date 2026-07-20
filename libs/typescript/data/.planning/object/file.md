# [DATA_FILE]

The filesystem plane and the derivative codec, both over the one content identity: files enter and leave the process through the platform `FileSystem` capability — content-addressed intake streams a file through the incremental digest fold into a conditional put, temp staging is scoped acquisition, and a watched directory is a `Stream` of admission events — and the image derivative fan-out decodes a stored object exactly once, `clone()`s per derivative-spec row, encodes through the one `toFormat` dispatch, mints each derivative's own `ContentKey` through the core digest, re-puts under the same conditional-put idempotency (412 = noop), and hands back one presigned grant per row. Untrusted input is gated before decode — `failOn`, pixel limits, blocked loaders, a per-pipeline deadline — because every upload is hostile; sharp is native and server-plane only, and its Promise terminals exist solely inside this page's boundary seams. A new rendition is a roster row; a new intake source is a lift; no per-format method ladder and no second addressing vocabulary exist anywhere on the plane.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                  |
| :-----: | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `FILE_PLANE`      | content-addressed intake, scoped temp staging, the watch stream, egress |
|  [02]   | `CODEC_GATE`      | the untrusted-input posture and the module governance rows              |
|  [03]   | `DERIVATIVE_ROWS` | the spec roster, decode-once/clone-N, the `toFormat` dispatch, receipts |
|  [04]   | `FANOUT`          | the fetch → gate → encode → mint → conditional re-put → grant fold      |

## [02]-[FILE_PLANE]

- Owner: `Disk` — the file-side verbs over the platform capability Tags: `intake` (file → identity fold → conditional put → reference row, owner parameterized), `stage` (scoped temp file whose teardown rides the `Scope`), `watch` (a settle-guarded drop directory as a `Stream` of intake admissions), `egress` (content object → file sink, streamed).
- Packages: `chokidar` (`watch`, the `all` listener, `awaitWriteFinish`, `atomic`, `ignored` matcher rows, awaited `close`) — the intake watch owner; `@effect/platform` (`FileSystem.FileSystem` — `stream`, `sink`, `watch`, `makeTempFileScoped`, `stat`; `Path.Path`); `effect` (`Stream`, `Effect`); `object/stream.md` (`Rail.bytes`, `Rail.identity` — the one identity fold), `object/store.md` (`ObjectStore` — the conditional legs); `journal/append.md` (`Hook` — the `objectAdmit` admission taps).
- Entry: an artifact producer lands its output through `Disk.intake(path, retention)`; a peer-runtime handoff directory rides `Disk.watch(dir)` feeding the same intake; a served export streams out through `Disk.egress(key, path)` — every verb yields the platform Tags on `R` and the runtime binding stays a root row.
- Receipt: intake answers the object receipt plus the file's `stat` evidence — `{ key, bytes, written, path }` — so a producer logs one row tying the filesystem coordinate to the durable key.
- Growth: a new intake posture (move-after-intake, verify-only) is an options field; a new source (an archive member walk) is one more lift into the same fold.
- Law: intake never buffers the file — identity is content-addressed, so the key cannot exist before the last byte is hashed, and intake is therefore TWO bounded streaming passes over the seekable file: `fs.stream(path)` feeds the `Rail` identity fold, then a fresh `fs.stream(path)` feeds the streaming conditional put — constant memory at any size; a `readFile` single-pass intake is the memory defect the rail already bans.
- Law: temp staging is scoped — `makeTempFileScoped` ties the temp file's deletion to the `Scope`, so an interrupted derivative pass or a failed intake leaks nothing; a hand-managed temp path is the rejected spelling.
- Law: the watch stream is admission, not truth — a watched drop directory emits candidate paths, each admitted through the same gated intake, and every candidate settles as an `Either` element on the success channel: `Either.right` the intake receipt (412 dedup included), `Either.left` the candidate's own `ObjectFault` — so one malformed file never ends the long-lived source, and only watcher transport failure fails the stream itself.
- Law: intake watching rides chokidar with the settle guard MANDATORY — `awaitWriteFinish` holds `add`/`change` until size stabilizes so a half-written file is never digested into a wrong content key, `atomic` absorbs editor rename-swap artifacts, selection is `ignored` predicate rows (never glob strings), the `all` listener lifts through `Stream.asyncPush`, and release AWAITS `close()`; `poll` and `depth` ride the options row for network mounts and bounded trees, and platform `FileSystem.watch` survives only for non-intake observation where a raw event suffices.
- Law: direct `node:fs` imports are banned on this plane — capability rides the Tag, the tracing and error rail come with it; chokidar and the platform binding are the only places a filesystem module name exists.
- Law: the gated intake carries the `rasm.data.object.admit` hook point — the veto runs on the path candidate with its `stat` size before a byte is hashed, so app policy refuses at the admission seam, and the observe fan runs on the landed receipt after the reference row commits; both compose the optional registry, so a composition without hooks pays nothing.

```typescript signature
import { Effect, Either, Option, Stream } from "effect"
import { FileSystem, Path } from "@effect/platform"
import { watch } from "chokidar"
import { ContentKey } from "@rasm/ts/core"
import { Hook } from "../journal/append.ts"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Disk {
  type Intake = { readonly key: ContentKey; readonly bytes: number; readonly written: boolean; readonly path: string }
  type Matcher = RegExp | ((path: string) => boolean)
  type WatchOptions = {
    readonly ignored?: ReadonlyArray<Matcher>
    readonly poll?: boolean
    readonly depth?: number
  }
}

const _WATCH = {
  settle: { stabilityThreshold: 2000, pollInterval: 100 },
  flight: 2,
} as const

const _intake = (path: string, retention: Retain.Class, owner?: string) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const store = yield* ObjectStore
    const held = yield* Effect.mapError(fs.stat(path), (fault) => new ObjectFault({ reason: "io", key: path, detail: fault.message }))
    yield* Effect.mapError(
      Hook.gated("objectAdmit", { key: path, owner: owner ?? `disk:${path}`, bytes: Option.some(Number(held.size)) }),
      (veto) => new ObjectFault({ reason: "io", key: path, detail: veto.detail }),
    ) // app policy refuses at the admission seam, before a byte is hashed
    const flow = fs.stream(path).pipe(
      Stream.mapError((fault) => new ObjectFault({ reason: "io", key: path, detail: fault.message })),
    )
    const identity = yield* Rail.identity(Rail.chunked(flow, Rail.cut))
    const landed = yield* store.putKeyed(
      identity.key,
      yield* Stream.toReadableStreamEffect(fs.stream(path)),
      identity.bytes,
    )
    yield* store.refer(identity.key, owner ?? `disk:${path}`, retention) // the derived retention tag lands with the reference row
    yield* Hook.tapped("objectAdmit", { key: identity.key, owner: owner ?? `disk:${path}`, bytes: Option.some(identity.bytes) }) // observe fan on the landed receipt
    return { key: identity.key, bytes: identity.bytes, written: landed.written, path } satisfies Disk.Intake
  })

const _watch = (dir: string, retention: Retain.Class, options?: Disk.WatchOptions) =>
  Stream.asyncPush<string, ObjectFault>((emit) =>
    Effect.acquireRelease(
      Effect.sync(() =>
        watch(dir, {
          atomic: true,
          awaitWriteFinish: _WATCH.settle,
          ignoreInitial: false,
          ...(options?.ignored !== undefined && { ignored: [...options.ignored] }),
          ...(options?.poll !== undefined && { usePolling: options.poll }),
          ...(options?.depth !== undefined && { depth: options.depth }),
        })
          .on("all", (event, path) => {
            if (event === "add" || event === "change") emit.single(path)
          })
          .on("error", (cause) => emit.fail(new ObjectFault({ reason: "io", key: dir, detail: String(cause) })))),
      (watcher) => Effect.orDie(Effect.tryPromise({
        try: () => watcher.close(),
        catch: (cause) => new ObjectFault({ reason: "io", key: dir, detail: String(cause) }),
      })),
    ),
  ).pipe(
    // per-candidate disposition: the intake outcome is an Either element, so one malformed file never ends the watcher
    Stream.mapEffect((path) => Effect.either(_intake(path, retention)), { concurrency: _WATCH.flight }),
  )

const _stage = Effect.flatMap(FileSystem.FileSystem, (fs) => fs.makeTempFileScoped())

const _egress = (key: ContentKey, path: string) =>
  Effect.flatMap(FileSystem.FileSystem, (fs) => Stream.run(Rail.range(key), fs.sink(path)))
```

## [03]-[CODEC_GATE]

- Owner: the untrusted-input posture — the `_GATE` ingress options, the blocked-loader roster applied once at module admission, the per-pipeline deadline — and the module governance rows (`cache`, `concurrency`, `simd`, the `sharp.format` capability read) that bound the native runtime and refuse an unbuildable rendition roster at boot.
- Packages: `sharp` (`SharpOptions` — `failOn`, `limitInputPixels`, `unlimited`, `autoOrient`; `sharp.block`, `sharp.cache`, `sharp.concurrency`, `sharp.simd`, `sharp.format`, `sharp.versions`, `timeout`).
- Entry: every decode on this plane opens through `_GATE`; `_governed(options, roster)` runs once at service construction — the loader block lands, the runtime tunes, and every roster row proves its codec against `sharp.format` so no ungated call site and no per-request format refusal can exist afterward.
- Growth: an admitted loader is a roster edit (an empty roster blocks nothing); a workload class with its own pixel ceiling is a second gate row selected by the fan-out's caller, never an inline override.
- Law: the gate precedes the decode — `failOn: "warning"` aborts on suspect input, `limitInputPixels` bounds decompression exposure, `unlimited` stays false, `autoOrient` normalizes EXIF rotation, and a `timeout` rides every pipeline; user bytes never reach an ungated loader.
- Law: governance is process policy — the libvips operation cache, the threadpool width, and the SIMD toggle are service-construction facts from configuration, because the derivative plane shares its process with the serving plane and unbounded native concurrency starves it; a roster row whose format the build cannot emit (buffer terminal, or file terminal for tile rows) fails construction as a `gate` fault, never a request.
- Law: sharp is server-plane native — no browser or wasm path imports it; the browser consumes grants, never the codec.

```typescript signature
import sharp, { type SharpOptions } from "sharp"

const _GATE = {
  failOn: "warning",
  limitInputPixels: 268_402_689,
  unlimited: false,
  autoOrient: true,
} as const satisfies SharpOptions

const _DEADLINE = { seconds: 20 } as const

const _governed = (
  options: { readonly blockedLoaders: ReadonlyArray<string>; readonly cacheMb: number; readonly threads: number; readonly simd: boolean },
  roster: ReadonlyArray<Derive.Spec>,
) =>
  Effect.zipRight(
    Effect.sync(() => {
      if (options.blockedLoaders.length > 0) sharp.block({ operation: [...options.blockedLoaders] })
      sharp.cache({ memory: options.cacheMb })
      sharp.concurrency(options.threads)
      sharp.simd(options.simd)
    }),
    Effect.forEach(roster, (spec) => {
      const capability = sharp.format[spec.format]
      const emits = spec.terminal === undefined ? capability?.output.buffer : capability?.output.file
      return emits === true
        ? Effect.void
        : Effect.fail(new DeriveFault({ stage: "gate", key: spec.name, detail: spec.format }))
    }, { discard: true }),
  )
```

## [04]-[DERIVATIVE_ROWS]

- Owner: the `Derive.Spec` roster row shape and the per-row receipt — a rendition is `{ name, resize, format, options, admit, composite, terminal, keep, placeholder, retention, grant }`, the codec dispatch is `toFormat(row.format, row.options)` with the tile pyramid as the one alternate terminal arm, and the receipt carries sharp's `OutputInfo` provenance plus the dominant color when the row asks.
- Packages: `sharp` (`clone`, `resize`, `composite`, `toFormat`, `tile`, `toBuffer({ resolveWithObject: true })`, `toFile`, `metadata`, `stats`, `keepIccProfile`, `keepMetadata`, `FormatEnum`, `Metadata`, `OverlayOptions`, `TileOptions`, `ResizeOptions`, `OutputOptions`, `OutputInfo`).
- Entry: an app declares its rendition roster once (`thumbnail`/`preview`/`master`/`deepzoom` rows) and hands it to the fan-out; format capability gates through `_governed`'s `sharp.format` read at construction so an unbuildable row refuses at boot, never per request.
- Receipt: `Derive.Receipt` — `{ name, key, grant, info, dominant }` — the row name, the derivative's own content key, its presigned `ObjectStore.Grant`, the codec provenance, and the optional placeholder color seeded from `stats().dominant`.
- Growth: a new rendition is one roster row; a new decision input is an `admit` predicate over the pre-decode `Metadata`, a new overlay is a `composite` row, a new pyramid layout is a `TileOptions` value, and retention plus grant posture travel on that same row — never a code path per format.
- Law: `toFormat` is the one codec dispatch — the per-format methods are aliases it generalizes, and a `jpeg()`/`png()`/`webp()` ladder is the named defect; row options carry quality/effort/lossless per codec; the tile row's `terminal` selects the pyramid arm (`layout: dz | iiif | iiif3 | zoomify | google`) whose container lands through the same content-addressed intake.
- Law: metadata preservation is a roster column — `keep: "icc"` re-attaches the color profile through `keepIccProfile` (the master row), `keep: "all"` carries the full block through `keepMetadata`, and the default strips everything, the public-derivative privacy posture — never a call-site toggle.
- Law: `metadata()` and `stats()` are the decision reads and each lifts ONCE per fan-out — `metadata()` feeds every row's `admit` vote (an SVG source never reaches a raster row unless its row admits it), `stats()` runs once when any admitted row asks for a placeholder and its `dominant` serves every asking row — a per-row pixel analysis is the named waste.
- Law: `composite` is a row-driven step — watermarks and badges are `OverlayOptions` rows on the spec chained before the terminal, so branding is roster data, never a second pipeline.

```typescript signature
import type { FormatEnum, Metadata, OutputInfo, OutputOptions, OverlayOptions, ResizeOptions, TileOptions } from "sharp"
import { DateTime, Option } from "effect"

declare namespace Derive {
  type Spec = {
    readonly name: string
    readonly resize: ResizeOptions
    readonly format: keyof FormatEnum
    readonly options?: OutputOptions
    readonly admit?: (source: Metadata) => boolean
    readonly composite?: ReadonlyArray<OverlayOptions>
    readonly terminal?: { readonly tile: TileOptions }
    readonly keep?: "icc" | "all"
    readonly placeholder?: boolean
    readonly retention: Retain.Class
    readonly grant?: ObjectStore.GrantPolicy
  }
  type Receipt = {
    readonly name: string
    readonly key: ContentKey
    readonly grant: ObjectStore.Grant
    readonly info: OutputInfo
    readonly dominant: Option.Option<{ readonly r: number; readonly g: number; readonly b: number }>
  }
}
```

## [05]-[FANOUT]

- Owner: `Derive.fanout(sourceKey, specs)` — the whole fold: verified fetch, gated single decode, clone-per-row encode, per-derivative key mint, conditional re-put, reference row owned by the source, presigned grant — plus `DeriveFault`, the plane's stage-discriminated fault.
- Packages: `sharp`; `object/store.md` (`ObjectStore.get`/`put`/`grant`, the reference verbs); `@rasm/ts/core` (`ContentKey` — every derivative is itself content-addressed).
- Entry: `Derive.fanout(sourceKey, roster)` after an image lands (an intake receipt, an upload finalize); re-running is a proven noop end to end because every re-put lands 412 and every grant re-mints against the same keys.
- Receipt: one `Derive.Receipt` per row; the batch's span carries source key, row count, and total encode span.
- Growth: watermarking is a `composite` step on the row's chain read from the spec; a tile-pyramid rendition is a row whose terminal is `tile` — both land inside the fold as row-driven steps.
- Law: decode once, clone N — the verified source bytes buffer once (`get` already re-minted identity), `metadata()` lifts once and vetoes rows through their `admit` predicates, `sharp(buffer, _GATE)` decodes once, and `clone()` snapshots the decoded pipeline per row; a re-decode, a re-piped stream, or a per-row metadata read is the named waste.
- Law: derivative identity is the core mint over the ENCODED bytes — each derivative is a first-class object with its own key, its own reference row owned by the source key (so source release cascades), and its own grant; the tile arm stages its pyramid container to a scoped temp path and lands it through the same content-addressed intake; sharp owns codec work only, never addressing or idempotency.
- Law: the fold is total over `DeriveFault` — stages `gate | fetch | decode | encode | persist | grant` carry the failing coordinate, and every `ObjectFault` crosses through `DeriveFault.at` at its owning stage; a single row's failure aborts this entrypoint, while an accumulating caller composes `Effect.validate` or `Effect.partition` explicitly.
- Law: `Derive.pressure` is the plane's saturation read — `sharp.counters()` as one typed effect (`{ queue, process }` in-flight telemetry) the maintenance and doctor surfaces sample, because the derivative fan-out is the process's native-saturation hotspot; its series names ride the core observability convention rows, so the emit plane exports them like every other metric.

```typescript signature
import { Array, Data } from "effect"
import { GetObjectCommand } from "@aws-sdk/client-s3"
import type { Sharp } from "sharp"

class DeriveFault extends Data.TaggedError("DeriveFault")<{
  readonly stage: "gate" | "fetch" | "decode" | "encode" | "persist" | "grant"
  readonly key: string
  readonly detail: string
}> {
  static at(stage: DeriveFault["stage"], key: string): (fault: unknown) => DeriveFault {
    return (fault) => new DeriveFault({ stage, key, detail: String(fault) })
  }
}

const _FAN = { flight: 4 } as const

const _chain = (decoded: Sharp, spec: Derive.Spec) => {
  const shaped = decoded.clone().resize(spec.resize)
  const layered = spec.composite === undefined ? shaped : shaped.composite([...spec.composite])
  return spec.keep === "icc" ? layered.keepIccProfile() : spec.keep === "all" ? layered.keepMetadata() : layered
}

const _encodeBuffer = (decoded: Sharp, spec: Derive.Spec, sourceKey: ContentKey) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const encoded = yield* Effect.tryPromise({
      try: () => _chain(decoded, spec).toFormat(spec.format, spec.options).toBuffer({ resolveWithObject: true }),
      catch: (defect) => new DeriveFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
    })
    const landed = yield* Effect.mapError(store.put(new Uint8Array(encoded.data)), DeriveFault.at("persist", sourceKey))
    return { key: landed.key, info: encoded.info }
  })

const _encodeTile = (decoded: Sharp, spec: Derive.Spec & { readonly terminal: { readonly tile: TileOptions } }, sourceKey: ContentKey) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const path = yield* Path.Path
    const staged = path.join(yield* fs.makeTempDirectoryScoped(), "pyramid.zip")
    const info = yield* Effect.tryPromise({
      try: () => _chain(decoded, spec).toFormat(spec.format, spec.options).tile(spec.terminal.tile).toFile(staged),
      catch: (defect) => new DeriveFault({ stage: "encode", key: sourceKey, detail: String(defect) }),
    })
    const landed = yield* Effect.mapError(_intake(staged, spec.retention, `derivative:${sourceKey}`), DeriveFault.at("persist", sourceKey))
    return { key: landed.key, info }
  })

const _fanout = (sourceKey: ContentKey, specs: ReadonlyArray<Derive.Spec>) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const bytes = yield* Effect.mapError(store.get(sourceKey), DeriveFault.at("fetch", sourceKey))
    const decoded = yield* Effect.try({
      try: () => sharp(Buffer.from(bytes), _GATE).timeout(_DEADLINE),
      catch: (defect) => new DeriveFault({ stage: "decode", key: sourceKey, detail: String(defect) }),
    })
    const source = yield* Effect.tryPromise({
      try: () => decoded.metadata(),
      catch: (defect) => new DeriveFault({ stage: "decode", key: sourceKey, detail: String(defect) }),
    })
    const admitted = Array.filter(specs, (spec) => spec.admit === undefined || spec.admit(source))
    const dominant = Array.some(admitted, (spec) => spec.placeholder === true)
      ? Option.some((yield* Effect.tryPromise({
          // one pixel analysis serves every asking row: stats lifts once per fan-out, exactly like metadata
          try: () => decoded.clone().stats(),
          catch: (defect) => new DeriveFault({ stage: "decode", key: sourceKey, detail: String(defect) }),
        })).dominant)
      : Option.none<{ readonly r: number; readonly g: number; readonly b: number }>()
    return yield* Effect.forEach(admitted, (spec) =>
      Effect.gen(function* () {
        const encoded = spec.terminal === undefined
          ? yield* _encodeBuffer(decoded, spec, sourceKey)
          : yield* Effect.scoped(_encodeTile(decoded, { ...spec, terminal: spec.terminal }, sourceKey))
        yield* Effect.mapError(
          store.refer(encoded.key, `derivative:${sourceKey}`, spec.retention),
          DeriveFault.at("persist", encoded.key),
        )
        const grant = yield* Effect.mapError(
          store.grant(encoded.key, new GetObjectCommand({ Bucket: store.bucket, Key: encoded.key }), spec.grant),
          DeriveFault.at("grant", encoded.key),
        )
        return {
          name: spec.name,
          key: encoded.key,
          grant,
          info: encoded.info,
          dominant: spec.placeholder === true ? dominant : Option.none(),
        } satisfies Derive.Receipt
      }), { concurrency: _FAN.flight })
  }).pipe(Effect.withSpan("data.fanout", { attributes: { source: sourceKey } }))

const _pressure = Effect.sync(() => sharp.counters())

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
  pressure: _pressure,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Derive, DeriveFault, Disk }
```
