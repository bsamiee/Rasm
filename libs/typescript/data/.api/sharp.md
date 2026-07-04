# [sharp] — the libvips image-derivative codec behind the `object/presign` fan-out: one chained `Duplex` pipeline decoding any input, transforming polymorphically, and encoding to any format via `toFormat`

`sharp` is the high-performance libvips-backed image processor the `object` plane composes for content-addressed derivative generation. The `sharp(input?, options?)` factory ingests any source — Buffer, path, an array of inputs, raw pixels, a generated solid/noise, or Pango text — through one `SharpOptions` ingress, and returns a chainable `Sharp` that is itself a Node `Duplex` stream (so it pipes) and terminates in `toBuffer`/`toFile`. The transform grammar is a polymorphic fold: every operation (`resize`/`rotate`/`sharpen`/`composite`/`extractChannel`/`toColourspace`/…) returns `Sharp`, and the output codec is `toFormat(format, options)` — one parameterized dispatch over the codec space, not a hand-written `jpeg()`/`png()`/`webp()` ladder — so the `presign` derivative fan-out is `clone()` + `toFormat(row.format, row.options)` over a derivative-spec row roster. sharp is native (bundled libvips prebuilds), server-only, and NOT Effect-aware — its Promise/stream terminals are wrapped at the `object/presign` boundary in `Effect.tryPromise` with a tagged fault, and untrusted uploads are gated by `sharp.block`/`failOn`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp`
- version: `0.35.3`
- license: `Apache-2.0` (the npm artifact; the bundled `.d.ts` retains its original MIT DefinitelyTyped notice)
- engine: `node >= 20.9.0`
- native: libvips, bundled as prebuilt platform binaries `@img/sharp-<platform>` (`darwin-arm64`/`darwin-x64`, `linux-arm64`/`x64`/`arm`/`ppc64`/`riscv64`/`s390x`, `linuxmusl-arm64`/`x64`, `wasm32`); `sharp.versions` exposes the linked codec versions (mozjpeg, aom, heif, webp, tiff, png, rsvg, …)
- module format: `export = sharp` (CJS default export; `import sharp from "sharp"`); `main: dist/index.cjs`, `module: dist/index.mjs`, `types: dist/index.d.mts` (content in `lib/index.d.ts`)
- runtime: server only (node/bun) — native libvips; NOT a browser/`lane/wasm` dependency. The `object` derivative fan-out is a server-plane concern
- colour dep: `@img/colour` is a direct runtime dependency (not a peer — `peerDependencies` is null); its `ColorLike` (aliased `Colour`/`Color` in the sharp types) parses every `background`/`tint`/overlay colour value
- rail: the `store` `object/presign` codec fan-out — content-addressed image derivatives, wrapped in the `Effect` rail at the boundary
- not-Effect: sharp is Promise/stream-native, not `Effect`-native; every terminal is lifted at the seam, never leaked into domain code

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: input options, result records, and introspection shapes
- rail: shapes
- One `SharpOptions` governs every ingress modality; `OutputInfo` is the receipt every terminal returns; `Metadata`/`Stats` are the pre-decode and pixel-analysis reads that inform the derivative decision and the content-address key.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `SharpInput` (`Buffer` \| path \| typed-array \| …) | input union        | the object bytes fetched from `@aws-sdk/client-s3` feed the factory |
|  [02]   | `SharpOptions`                                      | ingress config     | `failOn`/`limitInputPixels`/`limitInputChannels`/`unlimited`/`sequentialRead`/`density`/`ignoreIcc`/`pages`/`page`/`animated`/`autoOrient` + `raw`/`create`/`text`/`join` |
|  [03]   | `Create` / `CreateText` / `CreateRaw` / `Join` / `Noise` | generated input  | solid/noise canvas, Pango-markup text render, raw pixel buffer, tiled image join — non-fetched sources |
|  [04]   | `OutputInfo`                                        | terminal receipt   | `{ format, size, width, height, channels, premultiplied, hasAlpha, cropOffset*, trimOffset*, attentionX/Y, pages }` — the derivative provenance |
|  [05]   | `Metadata`                                          | pre-decode read    | `format`/`width`/`height`/`space`/`channels`/`depth`/`density`/`hasAlpha`/`hasProfile`/`orientation`/`exif`/`icc`/`xmp`/`pages`/`background`/`gainMap` — drives derivative + key decisions |
|  [06]   | `Stats` / `ChannelStats`                            | pixel analysis     | per-channel `min`/`max`/`mean`/`stdev`, `entropy`, `sharpness`, `dominant` colour, `isOpaque` — placeholder colour, blur-hash, content dedup |
|  [07]   | `TimeoutOptions` / `CacheOptions` / `CacheResult` / `SharpCounters` | governance record | `timeout({seconds})`, libvips operation-cache limits + telemetry, task counters |

[PUBLIC_TYPE_SCOPE]: format output options and the transform-option families
- rail: shapes
- The per-codec option interfaces are the parameter space `toFormat` dispatches over; a derivative-spec row carries a `keyof FormatEnum` plus the matching options object. The transform-option families (`ResizeOptions`, `OverlayOptions`, …) parameterize the chained pipeline.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `OutputOptions` + `JpegOptions`/`PngOptions`/`WebpOptions`/`AvifOptions`/`HeifOptions`/`GifOptions`/`TiffOptions`/`Jp2Options`/`JxlOptions` | codec params | the options half of a derivative-spec row: `quality`/`effort`/`lossless`/`progressive`/`chromaSubsampling`/`mozjpeg`/`palette`/`bitdepth`/… |
|  [02]   | `ResizeOptions`                                     | resize params      | `width`/`height`/`fit`/`position`/`background`/`kernel`/`withoutEnlargement`/`fastShrinkOnLoad` — the derivative geometry |
|  [03]   | `OverlayOptions` (extends `SharpOptions`)           | composite params   | `input`/`blend`/`gravity`/`top`/`left`/`tile`/`premultiplied` — watermark/overlay layer for `composite` |
|  [04]   | `Region` / `ExtendOptions` / `TrimOptions` / `Kernel` / `AffineOptions` | geometry params  | crop region, edge extension (`extendWith`), auto-trim, custom convolution kernel, affine matrix |
|  [05]   | `SharpenOptions` / `BlurOptions` / `FlattenOptions` / `NegateOptions` / `NormaliseOptions` / `ClaheOptions` / `ThresholdOptions` | operation params | the parameter objects for the operation family |
|  [06]   | `WriteableMetadata` / `Exif` / `WithIccProfileOptions` | metadata write   | `withMetadata`/`withExif`/`withIccProfile` — provenance the derivative keeps or strips |
|  [07]   | `TileOptions`                                       | pyramid params     | `tile()` deep-zoom output — `layout` (`dz`/`iiif`/`iiif3`/`zoomify`/`google`), `size`/`overlap`/`container` |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies (the closed dispatch enums)
- rail: types
- These are the closed value sets the pipeline discriminates on; a derivative-spec row references them by value, never by a hardcoded method name.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `FormatEnum` / `AvailableFormatInfo`                | codec set          | the `keyof FormatEnum` a derivative row selects; `sharp.format` reports input/output capability per codec |
|  [02]   | `FitEnum` (`contain`/`cover`/`fill`/`inside`/`outside`) | resize fit       | `ResizeOptions.fit` discriminant |
|  [03]   | `KernelEnum` / `Interpolators`                     | resample set       | `lanczos3`/`mitchell`/`mks2021`/… reduction kernel; `sharp.interpolators` enlargement set |
|  [04]   | `GravityEnum` / `StrategyEnum` (`entropy`/`attention`) | crop anchor      | `ResizeOptions.position` — fixed gravity or smart-crop strategy |
|  [05]   | `Blend` (27 modes) / `BoolEnum` (`and`/`or`/`eor`)  | compositing set    | `OverlayOptions.blend` layer math; `bandbool`/`boolean` channel logic |
|  [06]   | `FailOnOptions` (`none`/`truncated`/`error`/`warning`) | safety level     | `SharpOptions.failOn` — the untrusted-input abort threshold |
|  [07]   | `ColourspaceEnum` / `DepthEnum` / `Channels`        | pixel taxonomy     | `toColourspace`/`pipelineColourspace` target, raw depth, channel count `1\|2\|3\|4` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the factory and the chained transform grammar
- rail: surfaces-and-dispatch
- `sharp(input, options)` opens the pipeline; every transform returns `Sharp`, so the whole pipeline is one polymorphic fold. `clone()` snapshots a decoded pipeline to fan one input into many derivatives without re-decoding — the fan-out efficiency lever.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `sharp(input?, options?)` → `Sharp` (extends node `Duplex`)                                         | open pipeline  | `object/presign` opens on the object `Buffer` (`GetObject` `Body.transformToByteArray()`) so one decode `clone()`s to N derivatives; the `Duplex` stream form pipes a download only for a single-derivative pass — an `SdkStream` `Body` is single-consume |
|  [02]   | `.resize(opts)` / `.extend(opts)` / `.extract(region)` / `.trim(opts)` / `.affine(matrix, opts)`   | geometry fold  | derivative sizing — `fit`/`position`/`kernel` parameterized per spec row |
|  [03]   | `.rotate(a?, opts?)` / `.autoOrient()` / `.flip()` / `.flop()` / `.sharpen(opts?)` / `.blur(s?)` / `.median(n?)` / `.gamma()` / `.negate()` / `.normalise()` / `.clahe(opts)` / `.convolve(kernel)` / `.threshold(n?)` / `.linear(a,b)` / `.recomb(m)` / `.modulate(opts)` / `.flatten()` / `.unflatten()` | operation fold | EXIF-orient, thumbnail sharpen, tone/contrast normalisation for the derivative |
|  [04]   | `.removeAlpha()` / `.ensureAlpha(a?)` / `.extractChannel(c)` / `.joinChannel(imgs)` / `.bandbool(op)` | channel fold   | alpha flattening for JPEG derivatives, channel split/merge |
|  [05]   | `.tint(colour)` / `.greyscale(b?)` / `.pipelineColourspace(cs?)` / `.toColourspace(cs?)`            | colour fold    | colour-managed derivative output (sRGB), greyscale variants |
|  [06]   | `.composite(overlays: OverlayOptions[])`                                                            | layer fold     | watermark / badge overlay with `blend` mode + `gravity`; one call layers many overlays |
|  [07]   | `.clone()`                                                                                          | fan-out split  | snapshot the decoded pipeline → N parallel derivative branches from one decode |

[ENTRYPOINT_SCOPE]: output codecs, terminals, and metadata preservation
- rail: surfaces-and-dispatch
- `toFormat(format, options)` is the one parameterized codec dispatch; `toBuffer`/`toFile` are the terminals returning `OutputInfo`. The metadata-keep family controls what provenance the derivative carries — strip by default, keep ICC for colour fidelity.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `.toFormat(format, options?)` — `format: keyof FormatEnum` \| `AvailableFormatInfo`                | codec dispatch | THE fan-out encoder — `toFormat(row.format, row.options)` over a derivative-spec row roster |
|  [02]   | `.jpeg(o?)` / `.png(o?)` / `.webp(o?)` / `.avif(o?)` / `.heif(o?)` / `.gif(o?)` / `.tiff(o?)` / `.jp2(o?)` / `.jxl(o?)` / `.raw(o?)` | codec alias  | the explicit-codec forms `toFormat` generalizes; used only when a spec fixes one format |
|  [03]   | `.toBuffer({ resolveWithObject: true })` → `Promise<{ data: Buffer, info: OutputInfo }>` / `.toBuffer()` → `Promise<Buffer>` | terminal | the derivative bytes + receipt — wrapped in `Effect.tryPromise` at the boundary |
|  [04]   | `.toFile(path)` → `Promise<OutputInfo>`                                                             | terminal       | write to a filesystem sink (staging) — also `Effect`-lifted at the seam |
|  [05]   | `.tile(TileOptions)`                                                                                | pyramid output | deep-zoom / IIIF pyramid for large-image viewers |
|  [06]   | `.withMetadata(o?)` / `.keepExif()` / `.withExif(exif)` / `.keepIccProfile()` / `.withIccProfile(name, o?)` / `.keepXmp()` / `.keepMetadata()` | metadata keep | provenance control — strip for public derivatives, keep ICC for colour-managed masters |
|  [07]   | `.metadata()` → `Promise<Metadata>` / `.stats()` → `Promise<Stats>`                                 | introspect     | pre-decode format/size read + pixel analysis (`dominant`, `entropy`) for the derivative + key decision |

[ENTRYPOINT_SCOPE]: process governance and module statics
- rail: system-apis
- Per-pipeline `timeout`/`limitInputPixels` and the module-level statics bound resource use and enforce the untrusted-input security posture — the object plane processes user uploads.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `.timeout({ seconds })` / `.limitInputPixels(px)`                                                   | per-op bound   | processing deadline + pixel cap — ride `kernel/fault` degradation budgets |
|  [02]   | `sharp.block({ operation })` / `sharp.unblock({ operation })`                                       | loader gate    | block untrusted libvips loaders (e.g. all input but JPEG/PNG) — the untrusted-upload security posture |
|  [03]   | `sharp.cache(options?)` → `CacheResult` / `sharp.concurrency(n?)` / `sharp.simd(enable?)` / `sharp.counters()` | tune runtime | libvips operation-cache limits, libuv threadpool sizing, SIMD toggle, in-flight task telemetry |
|  [04]   | `sharp.format` / `sharp.versions` / `sharp.interpolators` / `sharp.queue` (EventEmitter)            | capability read | codec capability table, linked native versions, queue-change events |
|  [05]   | `sharp.gravity` / `sharp.strategy` / `sharp.kernel` / `sharp.fit` / `sharp.bool`                    | enum values    | the runtime enum objects the option types reference by value |

## [04]-[IMPLEMENTATION_LAW]

[SHARP_TOPOLOGY]:
- one factory, polymorphic ingress: `sharp(input, options)` is the single entry for every source — fetched bytes, path, raw pixels, generated canvas/text, or a joined array — discriminated by `SharpOptions` (`raw`/`create`/`text`/`join`), never a per-source factory family. The returned `Sharp` is a Node `Duplex`, so it both pipes (stream in/out) and terminates (`toBuffer`/`toFile`).
- the pipeline is a fold: every transform (`resize`/`rotate`/`composite`/`extractChannel`/`toColourspace`/…) returns `Sharp`, so a derivative is a chain built from a spec, and `toFormat(format, options)` is the one parameterized codec dispatch that owns the output space — the `jpeg()`/`png()`/`webp()` methods are aliases `toFormat` generalizes. The `object/presign` fan-out is `clone()` + `resize(row.resize)` + `toFormat(row.format, row.options)` iterated over a derivative-spec ROW roster, never a hardcoded per-format ladder.
- native + server-bound: sharp is libvips through prebuilt N-API binaries (`@img/sharp-<platform>`), synchronous under the hood with an async job queue on libuv. It is a server-plane dependency only — the browser `lane/wasm` never imports it; the derivative fan-out runs where the object bytes and libvips live.
- not Effect-native — lifted at the seam: sharp terminals are Promises (`toBuffer`/`toFile`/`metadata`/`stats`) or streams. `object/presign` wraps each in `Effect.tryPromise`/`Effect.async` with a tagged fault so a decode failure is a typed error-channel member, never a leaked rejection; `timeout`/`limitInputPixels` bound the work and `sharp.block`/`failOn` gate untrusted uploads before decode.

[INTEGRATION_LAW]:
- Stack on `effect` (`.api/effect.md`): the boundary law — `Effect.tryPromise({ try: () => pipeline.toBuffer({ resolveWithObject: true }), catch: e => new ImageError({ cause: e }) })` lifts every terminal; `metadata`/`stats` become `Effect`s that feed a `Match` over `Metadata.format`; `timeout` composes `kernel/fault` degradation budgets. sharp never appears in domain code untyped.
- Stack across `store/object`: `object/presign` collects the `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`) `GetObjectCommand` response `Body` (`StreamingBlobPayloadOutputTypes` = `SdkStream<Readable>`) once via `Body.transformToByteArray()` into a `Buffer`, opens `sharp(buffer)` for one decode, then `clone()`s per derivative-spec row — the `Body` is single-consume, so buffer-then-`clone()`, never a re-piped stream per derivative — and each derivative's content-key is minted through `object/key` = kernel `ContentKey`, so every derivative is itself content-addressed and its upload is conditional-put idempotent (If-None-Match; 412 = idempotent noop). Each derivative row is then delivered to the edge as a presigned `GetObject` URL minted by `@aws-sdk/s3-request-presigner` (`.api/aws-sdk-s3-request-presigner.md`), TTL-bounded like the source. `stats().dominant` seeds a placeholder colour; `metadata()` decides the target format/geometry per row.
- Stack on the derivative-spec roster: the fan-out is driven by a closed roster of `{ resize: ResizeOptions, format: keyof FormatEnum, options }` rows (thumbnail/preview/master), and a new derivative is one roster row — not a new method or code path. Format capability is gated by `sharp.format`; native codec presence by `sharp.versions`.
- Security posture for untrusted input: `SharpOptions.failOn` and `sharp.block({ operation })` restrict libvips loaders to the admitted set (e.g. block SVG/PDF/magick for user avatars); `limitInputPixels`/`limitInputChannels`/`unlimited: false` bound decompression-bomb exposure — the object plane treats every upload as hostile.

[LOCAL_ADMISSION]:
- Use `sharp` only in the server-plane `object/presign` fan-out; never import it on a browser/`lane/wasm` path — it is native and server-bound.
- Wrap every terminal (`toBuffer`/`toFile`/`metadata`/`stats`) in `Effect.tryPromise` with a tagged fault at the `object` boundary; never return a raw Promise or let a sharp throw cross into domain code.
- Drive the derivative fan-out with `toFormat(row.format, row.options)` over a spec-row roster and `clone()` a single decode; never hand-write a per-format `jpeg()`/`png()` ladder or re-decode per derivative.
- Gate untrusted uploads with `failOn` + `sharp.block` + `limitInputPixels` before decode; never process user bytes with `unlimited: true` or an unbounded loader set.
- Mint each derivative's content key through `object/key` (kernel `ContentKey`); never let sharp own addressing or idempotency — the conditional-put lives in `object/presign`.

[RAIL_LAW]:
- Package: `sharp`
- Owns: libvips image decode/transform/encode — the polymorphic `sharp(input, options)` ingress, the chained `Sharp` `Duplex` transform grammar (resize/operation/channel/colour/composite folds), `toFormat` + the codec terminals + `tile`, `metadata`/`stats` introspection, metadata-keep controls, and process governance (`timeout`/`limitInputPixels` + module `cache`/`concurrency`/`simd`/`block`/`format`/`versions`)
- Accept: server-plane use in `object/presign`, `Effect`-lifted terminals with tagged faults, `toFormat`-over-spec-rows fan-out with `clone()`, untrusted-input gating (`failOn`/`block`/`limitInputPixels`), content keys minted by `object/key`
- Reject: a browser/`lane/wasm` import, a raw Promise/throw crossing into domain code, a hardcoded per-format encoder ladder, unbounded/untrusted decode (`unlimited: true`, no loader gate), and sharp owning content addressing or upload idempotency
