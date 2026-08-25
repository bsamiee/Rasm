# [DATA_FILE]

Filesystem and derivative planes share `Digest.Key<"content">`. One spine opens, admits, emits, mints, conditionally stores, and refers.

Codec table correlates `format` and `options` through one generated union, so each rendition row carries its codec's own option record: a `quality`, `effort`, or `lossless` the roster states is spellable, and a row naming a codec the build cannot emit refuses at construction. `raw` produces the deep store the `ktx` headerless leg consumes, the channel-assembly row gathers component bands into the frozen packing orders, and the source's pixel analysis lifts once per fan-out to seed placeholder colour, retire a redundant alpha channel, and pick each row's grade off a declared entropy ladder.

Gates bound hostile input before any native decode. Renditions are roster rows, engines are plane rows, intake sources are lifts — no per-format ladder, second fan-out, or second address exists.

## [01]-[INDEX]

- [02]-[FILE_PLANE]: content-addressed intake, scoped temp staging, the watch stream, egress.
- [03]-[CODEC_GATE]: untrusted-input posture, module governance rows.
- [04]-[DERIVATIVE_ROWS]: plane contract, raster spec roster, per-row receipt.
- [05]-[FANOUT]: bind → open → facts → admit → emit → mint → re-put → grant spine.

## [02]-[FILE_PLANE]

- Owner: `Disk` — the file-side verbs over the platform capability Tags: `intake` (host file → app admission veto → identity fold → conditional put → reference row, owner parameterized), `seal` (the veto-free two-pass fold every internally minted file lands through), `stage` (scoped temp file whose teardown rides the `Scope`), `watch` (a settle-guarded drop directory as a `Stream` of intake admissions), `egress` (content object → file sink, streamed).
- Packages: `chokidar` (`watch`, the `all` listener, `awaitWriteFinish`, `atomic`, `ignored` matcher rows, awaited `close`) — the intake watch owner; `@effect/platform` (`FileSystem.FileSystem` — `stream`, `sink`, `watch`, `makeTempFileScoped`, `stat`; `Path.Path`); `effect` (`Stream`, `Effect`); `object/stream.md` (`Rail.bytes`, `Rail.identity` — the one identity fold), `object/store.md` (`ObjectStore` — the conditional legs); `journal/append.md` (`Hook` — the `objectAdmit` admission taps; `HookVeto` — the `denied`-class refusal intake keeps typed).
- Entry: an artifact producer lands its output through `Disk.intake(path, retention)`; a peer-runtime handoff directory rides `Disk.watch(dir)` feeding the same intake; a served export streams out through `Disk.egress(key, path)` — every verb yields the platform Tags on `R` and the runtime binding stays a root row.
- Receipt: intake answers the object receipt with file `stat` evidence — `{ key, bytes, written, path }` — tying filesystem coordinate to durable key.
- Growth: a new intake posture (move-after-intake, verify-only) is an options field; a new source (an archive member walk) is one more lift into the same fold.
- Boundary: this plane DECIDES NO TENANCY — the key is the content, so two tenants landing identical bytes land one object, and the reference row's owner is custody attribution rather than isolation; the tenancy axis stays `lane/tenant`'s session pinning, and a caller needing tenant-private bytes seals them before intake sees them.
- Law: intake FORFEITS the non-seekable source — identity and storage are two bounded passes over one path, so a FIFO, a socket, and a single-shot pipe carry no second read and cannot intake; a producer holding such a source stages it through `Disk.stage` first, whose lifetime ends with the `Scope` rather than with a retention class.
- Law: intake never buffers the file — identity is content-addressed, so the key cannot exist before the last byte is hashed, and intake is therefore TWO bounded streaming passes over the seekable file: `fs.stream(path)` feeds the `Rail` identity fold, then a fresh `fs.stream(path)` feeds the streaming conditional put — constant memory at any size; a `readFile` single-pass intake is the memory defect the rail already bans.
- Law: `Disk.seal` is that two-pass fold ALONE and `Disk.intake` is the fold under the host-file postures — the `stat` read, the app admission veto, the reference row, the observe fan; an internally minted file (a staged tile pyramid, a spawned encoder's product) lands through `seal` and takes its reference row from the derivative spine's own tail, so a veto written to refuse untrusted uploads can never refuse this branch's own product and no `(key, owner)` pair is referred twice.
- Law: every file-side verb carries its own span — `data.seal` on the two-pass identity fold every landing shares, `data.intake` on the host-file admission wrapping it, `data.watch` on the watcher acquire, `data.egress` on the streamed export — so a stalled drop directory, a refused candidate, and an internally minted product all read as evidence beside `data.fanout`; the span rides the FOLD rather than the veto-bearing wrapper, because the derivative spine lands through `seal` alone and a span placed on `intake` alone leaves every internally minted product untraced.
- Law: temp staging is scoped — `makeTempFileScoped` ties the temp file's deletion to the `Scope`, so an interrupted derivative pass or a failed intake leaks nothing; a hand-managed temp path is the rejected spelling.
- Law: the watch stream is admission, not truth — a watched drop directory emits candidate paths, each admitted through the same gated intake, and every candidate settles as an `Either` element on the success channel: `Either.right` the intake receipt (412 dedup included), `Either.left` the candidate's own refusal, `ObjectFault` from the filesystem and store legs or `HookVeto` from an app policy — so one malformed file never ends the long-lived source, and only watcher transport failure fails the stream itself.
- Law: intake watching rides chokidar with the settle guard MANDATORY — `awaitWriteFinish` holds `add`/`change` until size stabilizes so a half-written file is never digested into a wrong content key, `atomic` absorbs editor rename-swap artifacts, selection is `ignored` predicate rows (never glob strings), the `all` listener lifts through the SCOPED bridge (`Stream.asyncScoped` — the registration acquires the watcher, and a candidate shed by the push family's dropping or sliding shelves is an admission loss no re-emission ever repairs, because a dropped `add` never fires again until the file changes), and release AWAITS `close()`; `poll` and `depth` ride the options row for network mounts and bounded trees, and platform `FileSystem.watch` survives only for non-intake observation where a raw event suffices.
- Law: direct `node:fs` imports are banned on this plane — capability rides the Tag, the tracing and error rail come with it; chokidar and the platform binding are the only places a filesystem module name exists.
- Law: the gated intake carries the `rasm.data.object.admit` hook point — the veto runs on the path candidate with its `stat` size before a byte is hashed, so app policy refuses at the admission seam, and the observe fan runs on the landed receipt after the reference row commits; both compose the optional registry, so a composition without hooks pays nothing.
- Law: the intake owner is INGRESS — a caller-declared one admits through `object/store.md`'s owner namespace and its minted-below prefixes refuse, so a host file can never be attributed to a subject's custody scan, and an undeclared one takes the file plane's own row mint rather than an interpolated string a path bearing `:` re-splits.
- Law: `HookVeto` rides the intake channel WHOLE and never folds into `ObjectFault` — the veto's `denied` class is caller-blamed and non-retryable, the only `ObjectFault` reason a policy refusal fits is `io`, and `io` classes `unavailable`, the arm the recovery rail re-drives; re-spelling hands an armed app policy to that retry loop, so intake states `ObjectFault | HookVeto` and each fault keeps its own family's class.

```typescript
import { Effect, Either, Option, Stream } from "effect"
import { FileSystem, Path } from "@effect/platform"
import { watch } from "chokidar"
import { Digest } from "@rasm/core"
import { Hook } from "../journal/append.ts"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Disk {
  type Intake = { readonly key: Digest.Key<"content">; readonly bytes: number; readonly written: boolean; readonly path: string }
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

const _sealed = (path: string) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const store = yield* ObjectStore
    const flow = fs.stream(path).pipe(
      Stream.mapError((fault) => new ObjectFault({ case: { reason: "io", key: path, detail: fault.message } })),
    )
    const identity = yield* Rail.identity(Rail.chunked(flow, Rail.cut))
    const landed = yield* store.putKeyed(
      identity.key,
      yield* Stream.toReadableStreamEffect(fs.stream(path)),
      identity.bytes,
    )
    return { key: identity.key, bytes: identity.bytes, written: landed.written, path } satisfies Disk.Intake
  }).pipe(Effect.withSpan("data.seal", { attributes: { path } }))

const _intake = (path: string, retention: Retain.Class, owner?: string) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const store = yield* ObjectStore
    const held = yield* Effect.mapError(fs.stat(path), (fault) => new ObjectFault({ case: { reason: "io", key: path, detail: fault.message } }))
    const custodian = yield* Option.match(Option.fromNullable(owner), {
      onNone: () => Effect.succeed(ObjectStore.owner("disk", path)),
      onSome: ObjectStore.admit(path),
    })
    yield* Hook.gated("objectAdmit", { key: path, owner: custodian, bytes: Option.some(Number(held.size)) })
    const receipt = yield* _sealed(path)
    yield* store.refer(receipt.key, custodian, retention)
    yield* Hook.tapped("objectAdmit", { key: receipt.key, owner: custodian, bytes: Option.some(receipt.bytes) })
    return receipt
  }).pipe(Effect.withSpan("data.intake", { attributes: { path } }))

const _watch = (dir: string, retention: Retain.Class, options?: Disk.WatchOptions) =>
  Stream.asyncScoped<string, ObjectFault>((emit) =>
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
          .on("error", (cause) => emit.fail(new ObjectFault({ case: { reason: "io", key: dir, detail: String(cause) } })))),
      (watcher) => Effect.orDie(Effect.tryPromise({
        try: () => watcher.close(),
        catch: (cause) => new ObjectFault({ case: { reason: "io", key: dir, detail: String(cause) } }),
      })),
    ).pipe(Effect.withSpan("data.watch", { attributes: { dir } })),
  ).pipe(
    Stream.mapEffect((path) => Effect.either(_intake(path, retention)), { concurrency: _WATCH.flight }),
  )

const _stage = Effect.flatMap(FileSystem.FileSystem, (fs) => fs.makeTempFileScoped())

const _egress = (key: Digest.Key<"content">, path: string) =>
  Effect.flatMap(FileSystem.FileSystem, (fs) => Stream.run(Rail.range(key), fs.sink(path))).pipe(
    Effect.withSpan("data.egress", { attributes: { key, path } }),
  )
```

## [03]-[CODEC_GATE]

- Owner: the untrusted-input posture — the `_GATE` ingress options, the blocked-loader roster applied once at module admission, the per-pipeline deadline — and the module governance rows (`cache`, `concurrency`, `simd`, the `sharp.format` capability read, the `_PYRAMID` tile-codec roster) that bound the native runtime and refuse an unbuildable rendition roster at boot.
- Packages: `sharp` (`SharpOptions` — `failOn`, `limitInputPixels`, `unlimited`, `autoOrient`; `sharp.block`, `sharp.cache`, `sharp.concurrency`, `sharp.simd`, `sharp.format`, `sharp.versions`, `timeout`; `RawOptions.depth` over `keyof DepthEnum`).
- Entry: every decode on this plane opens through `_GATE`; `_governed(options, roster)` runs once at service construction — the loader block lands, the runtime tunes, every roster row proves its codec against `sharp.format`, and every tile row proves it again against `_PYRAMID`, so no ungated call site and no per-request format refusal can exist afterward.
- Growth: an admitted loader is a roster edit (an empty roster blocks nothing); a workload class with its own pixel ceiling is a second gate row selected by the fan-out's caller, never an inline override.
- Law: the gate precedes the decode — `failOn: "warning"` aborts on suspect input, `limitInputPixels` bounds decompression exposure, `unlimited` stays false, `autoOrient` normalizes EXIF rotation, and a `timeout` rides every pipeline; user bytes never reach an ungated loader.
- Law: governance is process policy — the libvips operation cache, the threadpool width, and the SIMD toggle are service-construction facts from configuration, because the derivative plane shares its process with the serving plane and unbounded native concurrency starves it; a roster row whose format the build cannot emit (buffer terminal, or file terminal under pyramid membership for tile rows) fails construction as a `gate` fault, never a request.
- Law: pyramid legality is `_PYRAMID`, never the codec's own file-output column — `dzsave` reads the pending format and admits `jpeg`, `png`, and `webp` ALONE, so `gif` and `tiff` pass an `output.file` read and refuse at the encode, which is exactly the per-request refusal class this gate exists to delete; the roster is the type-level closure on `[4]`'s tile arm AND this gate's second column, so a declared roster and a decoded one meet one refusal.
- Law: the `raw` row is the DEEP-STORE PRODUCER — `raw({ depth })` over `keyof DepthEnum` emits headerless pixels at `ushort`, `float`, or any other libvips band width, which is exactly the headerless posture `object/asset.md`'s `--raw --width --height` leg already classifies and had no producer for; seven of ten `_STORES` rows are deep, sharp encodes no EXR, and without this row the only deep route is an externally authored file that skips the derivative spine outright. `DepthEnum` is an INTERFACE keyed by band-width name (`char`..`ushort` beside `complex`/`dpcomplex`), so `keyof DepthEnum` is the sound spelling and a widened `string` admits a depth libvips refuses. `sharp.format.raw` reads like every other row in the boot roster proof and settles the FORMAT alone — `sharp.format` carries output capability per codec and no per-depth column exists — so the declared `keyof DepthEnum` column is the only depth gate that fires before a request and a band width this libvips build lacks refuses at the terminal as an `encode` fault.
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

const _PYRAMID = ["jpeg", "png", "webp"] as const

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
      const emits = spec.terminal === undefined
        ? capability?.output.buffer
        : capability?.output.file === true && Array.contains(_PYRAMID, spec.format)
      return emits === true
        ? Effect.void
        : Effect.fail(new DeriveFault({ case: { reason: "gate", key: spec.name, detail: spec.format } }))
    }, { discard: true }),
  )
```

## [04]-[DERIVATIVE_ROWS]

- Owner: the plane contract and the raster row family — `Derive.Plane` is the engine row `{ name, open, admit, emit }` the spine folds (raster here, container and `ktx` at `object/asset.md`), `Derive.Row` the envelope every engine's rows share (`name`, `retention`, `grant`), `Derive.Spec` the raster row over it, discriminated `kind: "raster"` so it enters the one category-general entry beside its siblings — rendition policy is one row, `toFormat` is the codec dispatch, tile is the alternate terminal, channel assembly is the alternate chain head, and `OutputInfo` with the source's own measures is evidence.
- Packages: `sharp` (`clone`, `resize`, `composite`, `extractChannel`, `joinChannel`, `removeAlpha`, `toColourspace`, `toFormat`, `raw`, `tile`, `toUint8Array`, `toBuffer`, `toFile`, `metadata`, `stats`, `keepIccProfile`, `keepMetadata`, `Channels`, `DepthEnum`, `FormatEnum`, `Metadata`, `OverlayOptions`, `TileOptions`, `ResizeOptions`, `OutputInfo`, and the per-codec `JpegOptions`/`PngOptions`/`WebpOptions`/`HeifOptions`/`JxlOptions`/`GifOptions`/`Jp2Options`/`RawOptions`/`TiffOptions` records); `@rasm/core` (`Wire.Texture` — the frozen `Pack` vocabulary the assembly row's tag closes against).
- Entry: an app declares its rendition roster once (`thumbnail`/`preview`/`master`/`deepzoom`/`orm` rows) and hands it to `Asset.pipe` beside its container and `ktx` rows; format capability gates through `_governed`'s `sharp.format` read at construction so an unbuildable row refuses at boot, never per request.
- Receipt: `Derive.Receipt` — `{ name, key, grant, info, dominant, measure }` — the row name, the derivative's own content key, the presigned `ObjectStore.Grant` its row's policy asked for, the codec provenance, the optional placeholder color seeded from `stats().dominant`, and the source measures the encode consumed.
- Growth: a rendition is one roster row; admission, overlay, channel assembly, pyramid layout, alpha posture, grade ladder, retention, and grant posture are fields on that row, never format paths.
- Law: `format` and `options` correlate at the DECLARATION — one generated arm per output codec off the interior codec table, so a `webp` row cannot carry a jpeg knob and the roster's stated `quality`/`effort`/`lossless` policy is spellable; `OutputOptions` declares `force?: boolean` and nothing else, so typing the column to it rejected every option the page's own law promised while the call site still compiled through the union `toFormat` accepts. `FormatEnum` closes the table at the type level in one direction and the boot gate closes it in the other, because output capability is a build fact `sharp.format[key].output` answers and no type can carry; AVIF has no `FormatEnum` key at all and rides the `heif` row under `compression: "av1"`, so the capability read stays total.
- Law: `heif` is the ONE arm whose options are MANDATORY — the codec selects `av1` or `hevc` through `compression` and carries no default, so an omitted record refuses inside libvips with a bare option-shape message carrying no row name; the arm states `options` required while every sibling keeps it optional, and the encode's grade substitution therefore never widens a required record to `undefined`.
- Law: `toFormat` is the one codec dispatch — the per-format methods are aliases it generalizes, and a `jpeg()`/`png()`/`webp()` ladder is the named defect; the tile row's `terminal` selects the pyramid arm (`layout: dz | iiif | iiif3 | zoomify | google`) whose container lands through `Disk.seal` under the spine's own reference tail.
- Law: the `terminal` column exists on the PYRAMID arms alone — `Derive.Pyramid` is `_PYRAMID`'s own type, the generated union grants the column to those three keys and types it `never` on every other, so a `gif` or `tiff` row carrying a pyramid refuses where it is written rather than at `_governed`; the gate's own second column then catches the same row when the roster arrives decoded.
- Law: the pyramid terminal PINS `container: "zip"` and the row cannot spell it — `TileOptions.container` defaults to `fs`, which writes a tile DIRECTORY, and every landing on this plane is a single-file two-pass stream, so a row left holding that knob stages a directory the seal cannot open and fails at the filesystem with no row name on the refusal; the column is `Omit<TileOptions, "container">` and one pyramid is one content-addressed archive by construction.
- Law: metadata preservation is a roster column — `keep: "icc"` re-attaches the color profile through `keepIccProfile` (the master row), `keep: "all"` carries the full block through `keepMetadata`, and the default strips everything, the public-derivative privacy posture — never a call-site toggle.
- Law: `metadata()` and `stats()` are the decision reads and each lifts ONCE per fan-out — `metadata()` feeds every row's `admit` vote (an SVG source never reaches a raster row unless its row admits it), `stats()` runs once when any admitted row declares a `placeholder`, an `alpha` posture, or a `grade` ladder, and its whole record then serves every asking row — a per-row pixel analysis is the named waste, and a lift gated on the placeholder column alone paid for one field and discarded three.
- Law: the analysis record drives three decisions and rides the receipt as evidence — `dominant` seeds the placeholder color, `isOpaque` retires a redundant alpha channel under `alpha: "opaque"`, and `entropy` selects the row's quality off a declared descending ladder so a flat plane and a photographic plane stop sharing one hardcoded number; `sharpness` is the third measure and rides the receipt beside them, so a consumer reads WHY a row took its grade without re-running the analysis. Ladder read folds through `Option` — no rung met leaves the row's declared options untouched, so a partial ladder forges no quality number.
- Law: alpha retirement is a RASTER-TERMINAL posture and the texture route takes the same quarter at EGRESS — a retired plane carries three channels, the frozen roster declares no three-channel plane store, and the ktx channel floor refuses a container declaring more than its bytes prove, so a row feeding a texture leaves `alpha` unset and the container holds four; `object/asset.md`'s `transcode` target spells `rgb8` where three channels ARE legal, so the saving lands at delivery rather than in the declaration, and only the declared plane store is refused.
- Law: `composite` is a row-driven step and `assemble` is a row-driven chain HEAD — watermarks and badges are `OverlayOptions` rows chained before the terminal, while the assembly row replaces the decoded source as the pipeline's input by gathering one band per pack position; both are roster data, never a second pipeline.
- Law: the assembly row names its pack and its bands POSITIONALLY — `Wire.Texture.Pack` is the frozen order vocabulary the tag closes against and the position IS the slot, so the row spells no role and holds no copy of the order's role triple. `Wire.Texture.Pack`'s core anchor exports the pack tuple ALONE; a pack's role columns are the interchange owner's own legality, read where a set document is proved, so a role table minted on this plane is exactly the second vocabulary that carve forecloses.
- Law: a band states its own origin — `source` reads the fan-out's own decoded plane, `plane` fetches a sibling by content key, `level` writes a declared constant — so the arm set is total and a component the source lacks writes the constant its ROW declares rather than a neutral read out of a foreign column. `Wire.Texture.Pack` freezes GPU read orders over the eight-bit block stores, so every band materializes as a lossless one-channel image before the join: `joinChannel` admits buffers and file paths and never a live pipeline, and a raw round trip narrows a deep source to eight bits because `CreateRaw` declares no depth.
- Law: the band extent is the AUTO-ORIENTED extent — the ingress pins `autoOrient`, so a rotated source decodes transposed and `Metadata.width`/`height` still report the pre-orientation pair while `Metadata.autoOrient` reports what the pipeline holds; a constant band sized from the raw pair packs a transposed plane against its siblings and `joinChannel` refuses on the extent mismatch.
- Law: the grant is minted only where the row's policy asks — `Derive.Receipt.grant` is `Option<ObjectStore.Grant>` folded on the row's own `grant` column, because a presign every derivative pays for is a signature nothing reads until a serving seam consumes one; the row that declares a policy gets its URL, every other row carries `Option.none()` and its key.

```typescript
import type {
  Channels, DepthEnum, FormatEnum, GifOptions, HeifOptions, Jp2Options, JpegOptions, JxlOptions, Metadata,
  OutputInfo, OverlayOptions, PngOptions, RawOptions, ResizeOptions, TiffOptions, TileOptions, WebpOptions,
} from "sharp"
import { Option } from "effect"
import { Wire } from "@rasm/core"

declare namespace Derive {
  type Reason = (typeof _family.kinds)[number]
  type Row = { readonly name: string; readonly retention: Retain.Class; readonly grant?: ObjectStore.GrantPolicy }
  type Product<R extends Row, I> = { readonly row: R; readonly key: Digest.Key<"content">; readonly evidence: I }
  type Plane<R extends Row, Facts, Handle, Evidence, E, Env> = {
    readonly name: string
    readonly open: (bytes: Uint8Array, source: Digest.Key<"content">) => Effect.Effect<{ readonly facts: Facts; readonly handle: Handle }, E, Env>
    readonly admit: (row: R, facts: Facts) => boolean
    readonly emit: (
      handle: Handle,
      rows: ReadonlyArray<R>,
      facts: Facts,
      source: Digest.Key<"content">,
    ) => Effect.Effect<ReadonlyArray<Product<R, Evidence>>, E, Env>
  }
  type Rgb = { readonly r: number; readonly g: number; readonly b: number }
  type Codec = {
    readonly gif: GifOptions
    readonly heif: HeifOptions
    readonly jp2: Jp2Options
    readonly jpeg: JpegOptions
    readonly jxl: JxlOptions
    readonly png: PngOptions
    readonly raw: RawOptions
    readonly tiff: TiffOptions
    readonly webp: WebpOptions
  }
  type _Coded<K extends keyof FormatEnum = keyof Codec> = K
  type Grade = ReadonlyArray<{ readonly above: number; readonly quality: number }>
  type Band =
    | { readonly from: "source"; readonly channel: 0 | 1 | 2 | 3 }
    | { readonly from: "plane"; readonly key: Digest.Key<"content">; readonly channel: 0 | 1 | 2 | 3 }
    | { readonly from: "level"; readonly value: number }
  type Assembly = { readonly pack: Wire.Texture.Pack; readonly bands: readonly [Band, Band, Band] }
  type Probe = {
    readonly format: keyof FormatEnum
    readonly width: number
    readonly height: number
    readonly channels: Channels
    readonly depth: keyof DepthEnum
  }
  type Measure = { readonly opaque: boolean; readonly entropy: number; readonly sharpness: number }
  type Rendition = Row & {
    readonly kind: "raster"
    readonly resize: ResizeOptions
    readonly admit?: (source: Metadata) => boolean
    readonly assemble?: Assembly
    readonly composite?: ReadonlyArray<OverlayOptions>
    readonly keep?: "icc" | "all"
    readonly alpha?: "opaque"
    readonly grade?: Grade
    readonly placeholder?: boolean
  }
  type Pyramid = (typeof _PYRAMID)[number]
  type Terminal = { readonly tile: Omit<TileOptions, "container"> }
  type _Arm<K extends keyof Codec> =
    & Rendition
    & { readonly format: K }
    & (K extends "heif" ? { readonly options: Codec[K] } : { readonly options?: Codec[K] })
    & (K extends Pyramid ? { readonly terminal?: Terminal } : { readonly terminal?: never })
  type Spec = { readonly [K in keyof Codec]: _Arm<K> }[keyof Codec]
  type Tiled = Extract<Spec, { readonly format: Pyramid }> & { readonly terminal: Terminal }
  type Receipt<
    I = {
      readonly info: OutputInfo
      readonly dominant: Option.Option<Derive.Rgb>
      readonly measure: Option.Option<Derive.Measure>
    },
  > = {
    readonly name: string
    readonly key: Digest.Key<"content">
    readonly grant: Option.Option<ObjectStore.Grant>
  } & I
}
```

## [05]-[FANOUT]

- Owner: `Derive.fanout(plane, sourceKey, rows)` — the ONE spine: verified fetch, plane-owned open and facts, row admission, plane-owned emit, then per-product source-owned reference and policy-gated grant; `Derive.raster` is this page's category plane, packaging gated decode, row-cloned encode, channel assembly, derivative mint, and conditional re-put behind the plane contract with reason-discriminated `DeriveFault`, and `Derive.probe` is the same decode read as the census the category plane's raster admission arm votes on.
- Packages: `sharp`, `effect`, `ObjectStore`, and core `Digest`, `Fault`, and `Convention` supply the derivative plane.
- Entry: `Asset.pipe(sourceKey, rows)` after an image lands (an intake receipt, an upload finalize) — the raster rows travel the same array as the container and `ktx` rows, so a caller mixing a thumbnail rendition with a KTX2 encode states one row array and reads one receipt array; re-running is a proven noop end to end because every re-put lands 412 and every grant re-mints against the same keys.
- Receipt: one `Derive.Receipt` per row; the batch's span carries source key, row count, and total encode span.
- Growth: watermarking is a `composite` step on the row's chain read from the spec; a tile-pyramid rendition is a row whose terminal is `tile`; a packed plane is a row whose `assemble` names a frozen order — all three land inside the fold as row-driven steps.
- Law: the engine is a plane row, never a fork of the spine — `open` yields the engine's facts and handle, `admit` votes rows against those facts, `emit` encodes and persists its products; the reference-and-grant tail is engine-blind, so a container pipeline, a spawned encoder, and this raster plane all inherit idempotency, cascade, and grant posture by construction and a second fanout is unrepresentable. `Derive.fanout` is the spine ALONE — `Asset.pipe` is its one entry, and a direct spine call beside that entry is the second entrypoint the category vocabulary exists to delete.
- Law: decode once, clone N — the verified source bytes buffer once (`get` already re-minted identity), `metadata()` lifts once and vetoes rows through their `admit` predicates, `sharp(buffer, _GATE)` decodes once, and `clone()` snapshots the decoded pipeline per row; a re-decode, a re-piped stream, or a per-row metadata read is the named waste.
- Law: an assembly row's siblings fetch inside EMIT, never through a plural-source open — the spine opens exactly one verified source and every single-source engine otherwise carries a plural signature it never uses, so the multi-source cost lands on the one row that needs it, exactly as the `ktx` engine fetches its extra level and face inputs. Lead plane's own AUTO-ORIENTED extent sizes every constant band, and the one decode already settled it — `Metadata` states extent, codec, channel count, and band depth as total fields, so the census carries no absence arm, and sizing a constant from the pre-orientation pair the same record still carries packs a transposed plane its siblings refuse.
- Law: the delivered-plane census is this page's read, never the category plane's — `Derive.probe` opens the gated decode and projects `Metadata` into `Derive.Probe`, so `object/asset.md`'s raster admission arm proves a declared codec and extent through the ONE libvips composer and imports no image library; probe and fan-out share the same decode and metadata legs, so a category gate and a derivative run can never drift on ingress options or deadline.
- Law: derivative identity is the core mint over the ENCODED bytes — each derivative is a first-class object with its own key, its own reference row minted at the store's `derivative` owner row (whose `cascade` role is what the sweep executes when the source reclaims), and the grant its row's policy asked for; the tile arm stages its pyramid container to a scoped temp path and lands it through `Disk.seal`, taking its reference row from this tail alone; sharp owns codec work only, never addressing or idempotency.
- Law: the row correlates its codec at the DECLARATION and the encode seam takes sharp's own union — one annotated projection folds the grade substitution into `Parameters<Sharp["toFormat"]>[1]`, so a row's stated options stay type-correlated where a caller writes them and the seam needs no cast where it consumes them.
- Law: `DeriveFault` closes gate, fetch, decode, encode, persist, and grant through `Fault.Class.family`, each reason declaring its own subject and rendering its own sentence; the raise carries ONE `case` payload, so a free `detail` field and a hand-written message template both delete at the class.
- Law: legs partition the census by the surface that DECIDES — the codec gate, the native engine, and the engine-blind tail — so a refusal names its seam without re-deriving it from the stage.
- Law: recovery derives from `Fault.Class`; invalid or malformed work quarantines, while unavailable boundary work re-drives.
- Law: `Derive.pressure` SETS the plane's saturation gauges — one `sharp.counters()` read writes `derivativeQueued` from `queue` and `derivativeActive` from `process` through the mounted convention rows, so the maintenance and doctor surfaces sample one owner instead of re-projecting a raw record into series names of their own; the derivative fan-out is the process's native-saturation hotspot, and a producer-side spelling that returns the record leaves both declared rows minted nowhere while a board query already reads them.

```typescript
import { Array, Match, Metric, Record, Schema } from "effect"
import { Convention, Fault } from "@rasm/core"
import { GetObjectCommand } from "@aws-sdk/client-s3"
import type { Sharp, Stats } from "sharp"

const _Subject = Schema.Struct({ key: Schema.String, detail: Schema.String })

const _family = Fault.Class.family(["gate", "fetch", "decode", "encode", "persist", "grant"] as const, {
  gate: Fault.Class.row({
    class: "invalid",
    leg: "gate",
    detail: _Subject,
    render: ({ key, detail }) => `rendition ${key} names codec ${detail}, which this build cannot emit`,
  }),
  fetch: Fault.Class.row({
    class: "unavailable",
    leg: "spine",
    detail: _Subject,
    render: ({ key, detail }) => `${key} did not reach the source bytes — ${detail}`,
  }),
  decode: Fault.Class.row({
    class: "malformed",
    leg: "engine",
    detail: _Subject,
    render: ({ key, detail }) => `${key} refused the gated decode — ${detail}`,
  }),
  encode: Fault.Class.row({
    class: "unavailable",
    leg: "engine",
    detail: _Subject,
    render: ({ key, detail }) => `${key} refused the encode — ${detail}`,
  }),
  persist: Fault.Class.row({
    class: "unavailable",
    leg: "spine",
    detail: _Subject,
    render: ({ key, detail }) => `${key} encoded but did not land — ${detail}`,
  }),
  grant: Fault.Class.row({
    class: "unavailable",
    leg: "spine",
    detail: _Subject,
    render: ({ key, detail }) => `${key} landed but minted no grant — ${detail}`,
  }),
})

class DeriveFault extends Schema.TaggedError<DeriveFault>()("DeriveFault", {
  case: _family.payload,
}) {
  static at(reason: Derive.Reason, key: string): (fault: unknown) => DeriveFault {
    return (fault) => new DeriveFault({ case: { reason, key, detail: String(fault) } })
  }
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const _FAN = { flight: 4 } as const

const _queued = Convention.mount(Convention.metric.derivativeQueued)
const _active = Convention.mount(Convention.metric.derivativeActive)

const _decoded = (bytes: Uint8Array, source: Digest.Key<"content">) =>
  Effect.try({ try: () => sharp(Buffer.from(bytes), _GATE).timeout(_DEADLINE), catch: DeriveFault.at("decode", source) })

const _facts = (handle: Sharp, source: Digest.Key<"content">) =>
  Effect.tryPromise({ try: () => handle.metadata(), catch: DeriveFault.at("decode", source) })

const _probed = (bytes: Uint8Array, source: Digest.Key<"content">) =>
  Effect.flatMap(_decoded(bytes, source), (handle) =>
    Effect.map(_facts(handle, source), (facts) => ({
      format: facts.format,
      width: facts.autoOrient.width,
      height: facts.autoOrient.height,
      channels: facts.channels,
      depth: facts.depth,
    } satisfies Derive.Probe)))

const _band = (lane: Sharp, key: string) =>
  Effect.tryPromise({
    try: () => lane.toColourspace("b-w").toFormat("png", { compressionLevel: 9 }).toBuffer(),
    catch: DeriveFault.at("encode", key),
  })

const _banded = (held: { readonly decoded: Sharp; readonly extent: Metadata["autoOrient"]; readonly source: Digest.Key<"content"> }) =>
  Match.type<Derive.Band>().pipe(
    Match.withReturnType<Effect.Effect<Buffer<ArrayBuffer>, DeriveFault, ObjectStore>>(),
    Match.discriminatorsExhaustive("from")({
      source: ({ channel }) => _band(held.decoded.clone().extractChannel(channel), held.source),
      plane: ({ channel, key }) =>
        Effect.flatMap(
          Effect.mapError(Effect.flatMap(ObjectStore, (store) => store.get(key)), DeriveFault.at("fetch", key)),
          (bytes) => Effect.flatMap(_decoded(bytes, key), (lane) => _band(lane.extractChannel(channel), key)),
        ),
      level: ({ value }) =>
        _band(
          sharp({ create: { ...held.extent, channels: 3, background: { r: value, g: value, b: value } } }).extractChannel(0),
          held.source,
        ),
    }),
  )

const _assembled = (decoded: Sharp, row: Derive.Assembly, facts: Metadata, source: Digest.Key<"content">) => {
  const band = _banded({ decoded, extent: facts.autoOrient, source })
  return Effect.zipWith(
    band(row.bands[0]),
    Effect.forEach(Array.drop(row.bands, 1), band, { concurrency: _FAN.flight }),
    (lead, rest) => sharp(lead, _GATE).joinChannel(rest),
    { concurrent: true },
  )
}

const _KEEP = {
  icc: (lane: Sharp) => lane.keepIccProfile(),
  all: (lane: Sharp) => lane.keepMetadata(),
} as const satisfies Record.ReadonlyRecord<NonNullable<Derive.Rendition["keep"]>, (lane: Sharp) => Sharp>

const _graded = (spec: Derive.Spec, measure: Option.Option<Derive.Measure>): Parameters<Sharp["toFormat"]>[1] =>
  Option.match(
    Option.flatMap(Option.all([Option.fromNullable(spec.grade), measure]), ([ladder, held]) =>
      Array.findFirst(ladder, (rung) => held.entropy > rung.above)),
    { onNone: () => spec.options, onSome: (rung) => ({ ...spec.options, quality: rung.quality }) },
  )

const _chain = (head: Sharp, spec: Derive.Spec, measure: Option.Option<Derive.Measure>) => {
  const shaped = head.clone().resize(spec.resize)
  const layered = spec.composite === undefined ? shaped : shaped.composite([...spec.composite])
  const opaque = spec.alpha === "opaque" && Option.match(measure, { onNone: () => false, onSome: (held) => held.opaque })
  const flattened = opaque ? layered.removeAlpha() : layered
  return spec.keep === undefined ? flattened : _KEEP[spec.keep](flattened)
}

const _encodeBuffer = (head: Sharp, spec: Derive.Spec, measure: Option.Option<Derive.Measure>, sourceKey: Digest.Key<"content">) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const encoded = yield* Effect.tryPromise({
      try: () => _chain(head, spec, measure).toFormat(spec.format, _graded(spec, measure)).toUint8Array(),
      catch: DeriveFault.at("encode", sourceKey),
    })
    const landed = yield* Effect.mapError(store.put(encoded.data), DeriveFault.at("persist", sourceKey))
    return { key: landed.key, info: encoded.info }
  })

const _encodeTile = (
  head: Sharp,
  spec: Derive.Tiled,
  measure: Option.Option<Derive.Measure>,
  sourceKey: Digest.Key<"content">,
) =>
  Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem
    const path = yield* Path.Path
    const staged = path.join(yield* fs.makeTempDirectoryScoped(), `${spec.name}.zip`)
    const info = yield* Effect.tryPromise({
      try: () =>
        _chain(head, spec, measure)
          .toFormat(spec.format, _graded(spec, measure))
          .tile({ ...spec.terminal.tile, container: "zip" })
          .toFile(staged),
      catch: DeriveFault.at("encode", sourceKey),
    })
    const landed = yield* Effect.mapError(_sealed(staged), DeriveFault.at("persist", sourceKey))
    return { key: landed.key, info }
  })

const _RASTER: Derive.Plane<
  Derive.Spec,
  Metadata,
  Sharp,
  { readonly info: OutputInfo; readonly dominant: Option.Option<Derive.Rgb>; readonly measure: Option.Option<Derive.Measure> },
  DeriveFault,
  ObjectStore | FileSystem.FileSystem | Path.Path
> = {
  name: "raster",
  open: (bytes, source) =>
    Effect.flatMap(_decoded(bytes, source), (handle) => Effect.map(_facts(handle, source), (facts) => ({ facts, handle }))),
  admit: (spec, facts) => spec.admit === undefined || spec.admit(facts),
  emit: (decoded, specs, facts, source) =>
    Effect.gen(function* () {
      const analysis = Array.some(specs, (spec) => spec.placeholder === true || spec.alpha !== undefined || spec.grade !== undefined)
        ? Option.some(yield* Effect.tryPromise({ try: () => decoded.clone().stats(), catch: DeriveFault.at("decode", source) }))
        : Option.none<Stats>()
      const dominant = Option.map(analysis, (held) => held.dominant)
      const measure = Option.map(
        analysis,
        (held) => ({ opaque: held.isOpaque, entropy: held.entropy, sharpness: held.sharpness }) satisfies Derive.Measure,
      )
      return yield* Effect.forEach(specs, (spec) =>
        Effect.gen(function* () {
          const head = spec.assemble === undefined ? decoded : yield* _assembled(decoded, spec.assemble, facts, source)
          const encoded = spec.terminal === undefined
            ? yield* _encodeBuffer(head, spec, measure, source)
            : yield* Effect.scoped(_encodeTile(head, { ...spec, terminal: spec.terminal }, measure, source))
          return {
            row: spec,
            key: encoded.key,
            evidence: { info: encoded.info, dominant: spec.placeholder === true ? dominant : Option.none<Derive.Rgb>(), measure },
          }
        }), { concurrency: _FAN.flight })
    }),
}

const _fanout = <R extends Derive.Row, F, H, I, E, Env>(
  plane: Derive.Plane<R, F, H, I, E, Env>,
  sourceKey: Digest.Key<"content">,
  rows: ReadonlyArray<R>,
): Effect.Effect<ReadonlyArray<Derive.Receipt<I>>, DeriveFault | E, Env | ObjectStore> =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const bytes = yield* Effect.mapError(store.get(sourceKey), DeriveFault.at("fetch", sourceKey))
    const opened = yield* plane.open(bytes, sourceKey)
    const products = yield* plane.emit(
      opened.handle,
      Array.filter(rows, (row) => plane.admit(row, opened.facts)),
      opened.facts,
      sourceKey,
    )
    return yield* Effect.forEach(products, (product) =>
      Effect.gen(function* () {
        yield* Effect.mapError(
          store.refer(product.key, ObjectStore.owner("derivative", sourceKey), product.row.retention),
          DeriveFault.at("persist", product.key),
        )
        const grant = yield* Effect.mapError(
          store.grant(product.key, new GetObjectCommand({ Bucket: store.bucket, Key: product.key }), product.row.grant),
          DeriveFault.at("grant", product.key),
        )
        return { name: product.row.name, key: product.key, grant, ...product.evidence }
      }), { concurrency: _FAN.flight })
  }).pipe(Effect.withSpan("data.fanout", { attributes: { source: sourceKey, plane: plane.name } }))

const _pressure = Effect.flatMap(
  Effect.sync(() => sharp.counters()),
  (held) => Effect.zipRight(Metric.set(_queued, held.queue), Metric.set(_active, held.process)),
)

const Disk = {
  intake: _intake,
  seal: _sealed,
  watch: _watch,
  stage: _stage,
  egress: _egress,
} as const

const Derive = {
  gate: _GATE,
  governed: _governed,
  probe: _probed,
  raster: _RASTER,
  fanout: _fanout,
  pressure: _pressure,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Derive, DeriveFault, Disk }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
