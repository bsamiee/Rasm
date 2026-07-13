# [TS_DATA_API_SHARP]

`sharp` is the high-performance libvips-backed image processor the `object` plane composes for content-addressed derivative generation. The `sharp(input?, options?)` factory ingests any source — Buffer, path, an array of inputs, raw pixels, a generated solid/noise, or Pango text — through one `SharpOptions` ingress, and returns a chainable `Sharp` that is itself a Node `Duplex` stream (so it pipes) and terminates in `toBuffer`/`toFile`. The transform grammar is a polymorphic fold: every operation (`resize`/`rotate`/`sharpen`/`composite`/`extractChannel`/`toColourspace`/…) returns `Sharp`, and the output codec is `toFormat(format, options)` — one parameterized dispatch over the codec space, not a hand-written `jpeg()`/`png()`/`webp()` ladder — so the `presign` derivative fan-out is `clone()` + `toFormat(row.format, row.options)` over a derivative-spec row roster. sharp is native (bundled libvips prebuilds), server-only, and NOT Effect-aware — its Promise/stream terminals are wrapped at the `object/presign` boundary in `Effect.tryPromise` with a tagged fault, and untrusted uploads are gated by `sharp.block`/`failOn`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp`
- license: `Apache-2.0` (the npm artifact; the bundled `.d.ts` retains its original MIT DefinitelyTyped notice)
- engine: `node >= catalog`
- native: libvips, bundled as prebuilt platform binaries `@img/sharp-<platform>` (`darwin-arm64`/`darwin-x64`, `linux-arm64`/`x64`/`arm`/`ppc64`/`riscv64`/`s390x`, `linuxmusl-arm64`/`x64`, `wasm32`); `sharp.versions` exposes the linked codec versions (mozjpeg, aom, heif, webp, tiff, png, rsvg, …)
- module format: `export = sharp` (CJS default export; `import sharp from "sharp"`); `main: dist/index.cjs`, `module: dist/index.mjs`, `types: dist/index.d.mts` (content in `lib/index.d.ts`)
- runtime: server only (node/bun) — native libvips; NOT a browser/`lane/wasm` dependency. The `object` derivative fan-out is a server-plane concern
- colour dep: `@img/colour` is a direct runtime dependency (not a peer — `peerDependencies` is null); its `ColorLike` (aliased `Colour`/`Color` in the sharp types) parses every `background`/`tint`/overlay colour value
- rail: the `store` `object/presign` codec fan-out — content-addressed image derivatives, wrapped in the `Effect` rail at the boundary
- not-Effect: sharp is Promise/stream-native, not `Effect`-native; every terminal is lifted at the seam, never leaked into domain code

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: input options, result records, and introspection shapes
- rail: shapes
- One `SharpOptions` governs every ingress modality — `failOn`/`limitInputPixels`/`limitInputChannels`/`unlimited`/`sequentialRead`/`density`/`ignoreIcc`/`pages`/`page`/`animated`/`autoOrient` plus the `raw`/`create`/`text`/`join` source select. `OutputInfo` is the terminal receipt `{ format, size, width, height, channels, premultiplied, hasAlpha, cropOffset*, trimOffset*, attentionX/Y, pages }`. `Metadata` reads `format`/`width`/`height`/`space`/`channels`/`depth`/`density`/`hasAlpha`/`hasProfile`/`orientation`/`exif`/`icc`/`xmp`/`pages`/`background`/`gainMap`; `Stats`/`ChannelStats` carry per-channel `min`/`max`/`mean`/`stdev` plus `entropy`/`sharpness`/`dominant` colour/`isOpaque`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `SharpInput` (`Buffer` \| path \| typed-array \| …)      | input union      | the object bytes fetched from `@aws-sdk/client-s3`     |
|  [02]   | `SharpOptions`                                           | ingress config   | one ingress config; input gates + source select        |
|  [03]   | `Create` / `CreateText` / `CreateRaw` / `Join` / `Noise` | generated input  | solid/noise canvas, Pango text, raw pixels, tiled join |
|  [04]   | `OutputInfo`                                             | terminal receipt | the derivative provenance every terminal returns       |
|  [05]   | `Metadata`                                               | pre-decode read  | drives the derivative + content-key decision           |
|  [06]   | `Stats` / `ChannelStats`                                 | pixel analysis   | placeholder colour, blur-hash, content dedup           |
|  [07]   | `TimeoutOptions` / `CacheOptions`                        | governance       | `timeout({seconds})`; libvips operation-cache limits   |
|  [08]   | `CacheResult` / `SharpCounters`                          | governance       | cache telemetry + in-flight task counters              |

[PUBLIC_TYPE_SCOPE]: format output options and the transform-option families
- rail: shapes
- The per-codec option interfaces `OutputOptions`/`JpegOptions`/`PngOptions`/`WebpOptions`/`AvifOptions`/`HeifOptions`/`GifOptions`/`TiffOptions`/`Jp2Options`/`JxlOptions` are the parameter space `toFormat` dispatches over (`quality`/`effort`/`lossless`/`progressive`/`chromaSubsampling`/`mozjpeg`/`palette`/`bitdepth`/…); a derivative-spec row carries a `keyof FormatEnum` plus the matching options object. `ResizeOptions` carries `width`/`height`/`fit`/`position`/`background`/`kernel`/`withoutEnlargement`/`fastShrinkOnLoad`; `OverlayOptions` (extends `SharpOptions`) carries `input`/`blend`/`gravity`/`top`/`left`/`tile`/`premultiplied`; `TileOptions` carries `layout` (`dz`/`iiif`/`iiif3`/`zoomify`/`google`)/`size`/`overlap`/`container`. The operation-option types are `SharpenOptions`/`BlurOptions`/`FlattenOptions`/`NegateOptions`/`NormaliseOptions`/`ClaheOptions`/`ThresholdOptions`.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                   |
| :-----: | :----------------------------------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `OutputOptions` + codec `*Options`                     | codec params     | the options half of a derivative-spec row             |
|  [02]   | `ResizeOptions`                                        | resize params    | the derivative geometry                               |
|  [03]   | `OverlayOptions` (extends `SharpOptions`)              | composite params | watermark/overlay layer for `composite`               |
|  [04]   | `Region`                                               | geometry params  | crop region for `.extract`                            |
|  [05]   | `ExtendOptions`                                        | geometry params  | edge extension (`extendWith`)                         |
|  [06]   | `TrimOptions`                                          | geometry params  | auto-trim borders                                     |
|  [07]   | `Kernel`                                               | geometry params  | custom convolution kernel                             |
|  [08]   | `AffineOptions`                                        | geometry params  | affine transform matrix                               |
|  [09]   | operation `*Options` family                            | operation params | per-operation parameter objects (roster in lead)      |
|  [10]   | `WriteableMetadata` / `Exif` / `WithIccProfileOptions` | metadata write   | `withMetadata`/`withExif`/`withIccProfile` provenance |
|  [11]   | `TileOptions`                                          | pyramid params   | `tile()` deep-zoom output                             |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies (the closed dispatch enums)
- rail: types
- These are the closed value sets the pipeline discriminates on; a derivative-spec row references them by value, never by a hardcoded method name. The `KernelEnum` reduction kernels are `lanczos3`/`mitchell`/`mks2021`/….

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                     |
| :-----: | :------------------------------------------------------ | :-------------- | :------------------------------------------------------ |
|  [01]   | `FormatEnum` / `AvailableFormatInfo`                    | codec set       | `keyof FormatEnum`; `sharp.format` per-codec capability |
|  [02]   | `FitEnum` (`contain`/`cover`/`fill`/`inside`/`outside`) | resize fit      | `ResizeOptions.fit` discriminant                        |
|  [03]   | `KernelEnum` / `Interpolators`                          | resample set    | reduction kernel; `sharp.interpolators` enlargement set |
|  [04]   | `GravityEnum` / `StrategyEnum` (`entropy`/`attention`)  | crop anchor     | `ResizeOptions.position` — fixed gravity or smart-crop  |
|  [05]   | `Blend` (27 modes) / `BoolEnum` (`and`/`or`/`eor`)      | compositing set | `OverlayOptions.blend` math; `bandbool`/`boolean` logic |
|  [06]   | `FailOnOptions` (`none`/`truncated`/`error`/`warning`)  | safety level    | `SharpOptions.failOn` — untrusted-input abort threshold |
|  [07]   | `ColourspaceEnum` / `DepthEnum` / `Channels`            | pixel taxonomy  | `toColourspace` target, depth, channels `1\|2\|3\|4`    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the factory and the chained transform grammar
- rail: surfaces-and-dispatch
- `sharp(input, options)` opens the pipeline; every transform returns `Sharp`, so the whole pipeline is one polymorphic fold. `clone()` snapshots a decoded pipeline to fan one input into many derivatives without re-decoding — the fan-out efficiency lever. The operation fold chains `.rotate(a?, opts?)`/`.autoOrient()`/`.flip()`/`.flop()`/`.sharpen(opts?)`/`.blur(s?)`/`.median(n?)`/`.gamma()`/`.negate()`/`.normalise()`/`.clahe(opts)`/`.convolve(kernel)`/`.threshold(n?)`/`.linear(a,b)`/`.recomb(m)`/`.modulate(opts)`/`.flatten()`/`.unflatten()`, the channel fold `.removeAlpha()`/`.ensureAlpha(a?)`/`.extractChannel(c)`/`.joinChannel(imgs)`/`.bandbool(op)`, the colour fold `.tint(colour)`/`.greyscale(b?)`/`.pipelineColourspace(cs?)`/`.toColourspace(cs?)` — each returning `Sharp`.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                        |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `sharp(input?, options?)` → `Sharp`                      | open pipeline  | opens on the object `Buffer`; one decode → N derivatives   |
|  [02]   | `.resize` / `.extend` / `.extract` / `.trim` / `.affine` | geometry fold  | derivative sizing — `fit`/`position`/`kernel` per spec row |
|  [03]   | `.rotate` … `.unflatten` (18 methods, roster in lead)    | operation fold | EXIF-orient, sharpen, tone/contrast normalisation          |
|  [04]   | `.removeAlpha` … `.bandbool` (channel roster in lead)    | channel fold   | alpha flattening for JPEG derivatives, channel split/merge |
|  [05]   | `.tint` … `.toColourspace` (colour roster in lead)       | colour fold    | colour-managed output (sRGB), greyscale variants           |
|  [06]   | `.composite(overlays: OverlayOptions[])`                 | layer fold     | watermark/badge — `blend` + `gravity`, many per call       |
|  [07]   | `.clone()`                                               | fan-out split  | snapshot the decoded pipeline → N derivative branches      |

[ENTRYPOINT_SCOPE]: output codecs, terminals, and metadata preservation
- rail: surfaces-and-dispatch
- `toFormat(format, options)` is the one parameterized codec dispatch (`format: keyof FormatEnum | AvailableFormatInfo`); `toBuffer`/`toFile` are the terminals returning `OutputInfo`. The explicit-codec aliases are `.jpeg`/`.png`/`.webp`/`.avif`/`.heif`/`.gif`/`.tiff`/`.jp2`/`.jxl`/`.raw`; the metadata-keep family is `.withMetadata`/`.keepExif`/`.withExif`/`.keepIccProfile`/`.withIccProfile`/`.keepXmp`/`.keepMetadata`. `toBuffer({ resolveWithObject: true })` returns `Promise<{ data: Buffer, info: OutputInfo }>`, `toBuffer()` returns `Promise<Buffer>`, `toFile(path)` returns `Promise<OutputInfo>`, `metadata()`/`stats()` return `Promise<Metadata>`/`Promise<Stats>`.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                              |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `.toFormat(format, options?)`                    | codec dispatch | THE fan-out encoder — `toFormat(row.format, row.options)`        |
|  [02]   | codec aliases `.jpeg`/`.png`/… (roster in lead)  | codec alias    | explicit-codec forms `toFormat` generalizes                      |
|  [03]   | `.toBuffer(opts?)`                               | terminal       | derivative bytes + receipt — `Effect.tryPromise` at the boundary |
|  [04]   | `.toFile(path)`                                  | terminal       | write to a filesystem sink (staging) — `Effect`-lifted           |
|  [05]   | `.tile(TileOptions)`                             | pyramid output | deep-zoom / IIIF pyramid for large-image viewers                 |
|  [06]   | metadata-keep `.withMetadata`/… (roster in lead) | metadata keep  | strip for public derivatives, keep ICC for masters               |
|  [07]   | `.metadata()` / `.stats()`                       | introspect     | pre-decode read + pixel analysis (`dominant`, `entropy`)         |

[ENTRYPOINT_SCOPE]: process governance and module statics
- rail: system-apis
- Per-pipeline `timeout`/`limitInputPixels` and the module-level statics bound resource use and enforce the untrusted-input security posture — the object plane processes user uploads. `sharp.cache(options?)` returns `CacheResult`; `sharp.queue` is an `EventEmitter`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                     |
| :-----: | :----------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `.timeout({ seconds })` / `.limitInputPixels(px)`            | per-op bound    | deadline + pixel cap — `core/value/fault` budgets       |
|  [02]   | `sharp.block` / `sharp.unblock` (`{ operation }`)            | loader gate     | block untrusted libvips loaders (e.g. all but JPEG/PNG) |
|  [03]   | `sharp.cache` / `.concurrency` / `.simd` / `.counters`       | tune runtime    | libvips cache, threadpool sizing, SIMD, task telemetry  |
|  [04]   | `sharp.format` / `.versions` / `.interpolators` / `.queue`   | capability read | codec table, native versions, queue-change events       |
|  [05]   | `sharp.gravity` / `.strategy` / `.kernel` / `.fit` / `.bool` | enum values     | runtime enum objects the option types reference         |

## [04]-[IMPLEMENTATION_LAW]

[SHARP_TOPOLOGY]:
- one factory, polymorphic ingress: `sharp(input, options)` is the single entry for every source — fetched bytes, path, raw pixels, generated canvas/text, or a joined array — discriminated by `SharpOptions` (`raw`/`create`/`text`/`join`), never a per-source factory family. The returned `Sharp` is a Node `Duplex`, so it both pipes (stream in/out) and terminates (`toBuffer`/`toFile`).
- the pipeline is a fold: every transform (`resize`/`rotate`/`composite`/`extractChannel`/`toColourspace`/…) returns `Sharp`, so a derivative is a chain built from a spec, and `toFormat(format, options)` is the one parameterized codec dispatch that owns the output space — the `jpeg()`/`png()`/`webp()` methods are aliases `toFormat` generalizes. The `object/presign` fan-out is `clone()` + `resize(row.resize)` + `toFormat(row.format, row.options)` iterated over a derivative-spec ROW roster, never a hardcoded per-format ladder.
- native + server-bound: sharp is libvips through prebuilt N-API binaries (`@img/sharp-<platform>`), synchronous under the hood with an async job queue on libuv. It is a server-plane dependency only — the browser `lane/wasm` never imports it; the derivative fan-out runs where the object bytes and libvips live.
- not Effect-native — lifted at the seam: sharp terminals are Promises (`toBuffer`/`toFile`/`metadata`/`stats`) or streams. `object/presign` wraps each in `Effect.tryPromise`/`Effect.async` with a tagged fault so a decode failure is a typed error-channel member, never a leaked rejection; `timeout`/`limitInputPixels` bound the work and `sharp.block`/`failOn` gate untrusted uploads before decode.

[INTEGRATION_LAW]:
- Stack on `effect` (`.api/effect.md`): the boundary law — `Effect.tryPromise({ try: () => pipeline.toBuffer({ resolveWithObject: true }), catch: e => new ImageError({ cause: e }) })` lifts every terminal; `metadata`/`stats` become `Effect`s that feed a `Match` over `Metadata.format`; `timeout` composes `core/value/fault` degradation budgets. sharp never appears in domain code untyped.
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
