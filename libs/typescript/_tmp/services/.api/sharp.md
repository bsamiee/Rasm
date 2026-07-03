# [API_CATALOGUE] sharp

`sharp` is the libvips-backed image pipeline that rides the `persistence/object#OBJECT_STORE` upload path as the in-pipeline image-transform codec (not a fifth `AssetCodec` export literal): the default export is one callable/constructable `SharpConstructor` returning a `Sharp` — a `node:stream.Duplex` that lazily chains resize/rotate/extract/composite/colour/filter/morphology operations and terminates in exactly one output call (`toFile`/`toBuffer`/`toUint8Array`/pipe). Operations are order-semantic and deferred until a terminal, so the whole recipe is a pure description a boundary lifts once. In `services` it is never called bare: the `persistence/object#OBJECT_STORE` `transformImage` seam composes `sharp(input).resize(width).toFormat(format).toBuffer()` inside one `Effect.tryPromise` (rejection → `ObjectFault { reason: "encode_failed" }` — the transform's own rail, distinct from the four export codecs' `AssetTransferFault`), the resulting `Uint8Array` streams straight into `ObjectStore.put` under the `XxHash128` content-address, and `toUint8Array()`/`toBuffer({ resolveWithObject: true })` surfaces the `OutputInfo` receipt (`format`/`size`/`width`/`height`/`channels`) for the stored object's metadata — no disk round-trip. `format` is discriminated by `keyof FormatEnum`, so one `toFormat` entry owns every encoder, never a per-format branch. For a large source the `Sharp` `Duplex` bridges Effect `Stream` through `@effect/platform-node` `NodeStream.pipeThroughDuplex` rather than buffering, matching the store's size-branched `put`. The static libvips controls (`cache`/`concurrency`/`simd`/`block`/`unblock`) are process-global startup config a `Layer` sets once. Hand-rolled pixel loops or a spawned ImageMagick/`gm` shell is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp` (0.35.3, Apache-2.0; bundled `.d.mts` MIT — DefinitelyTyped lineage; © François Nguyen and others)
- module format: ESM types `dist/index.d.mts` (`types`), `default` export `sharp: SharpConstructor` (call `sharp(input, opts)` or `new sharp(...)`) plus named type/enum exports; self-typed, no `@types/sharp`
- reflected: TSDECL — `node_modules/sharp/dist/index.d.mts`
- runtime target: `node` (`engines.node >=20.9.0`); native — prebuilt libvips ships in per-platform optional deps (`@img/sharp-<platform>`, `@img/sharp-libvips-*`); `ColorLike` from `@img/colour`
- ABI: `cache`/`concurrency`/`counters`/`simd`/`block`/`unblock`/`versions` are process-global libvips runtime state, not per-instance; `UV_THREADPOOL_SIZE` (set pre-boot) caps parallel pipelines; `block`/`VIPS_BLOCK_UNTRUSTED` gate untrusted decoders; JXL/HEIC/GIF-magick need a custom-compiled libvips absent from prebuilt binaries
- consumer: `persistence/object#OBJECT_STORE` — the in-pipeline image-transform codec on the upload path (not a fifth `AssetCodec` export literal): `sharp(input).resize().toFormat(keyof FormatEnum).toBuffer()` → `ObjectStore.put` under `XxHash128`, plus the `NodeStream.pipeThroughDuplex` streaming branch for oversized sources
- rail: image-transform / image-processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline, input, and output carriers
The `Sharp` `Duplex` is the one chainable owner; `SharpConstructor` is the callable default export, `SharpInput`/`SharpOptions` the ingress vocabulary, and `OutputInfo`/`Metadata`/`Stats` the terminal and probe receipts.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]        | [CAPABILITY]                                                                                    |
| :-----: | :--------------- | :------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Sharp`          | interface (`Duplex`) | the chainable pipeline; every op returns `Sharp`; terminates in one output call                 |
|  [02]   | `SharpConstructor` | callable interface | the default export — `(input?, opts?) => Sharp` + `new`, carrying the static-control namespace   |
|  [03]   | `SharpInput`     | union                | `Buffer \| ArrayBuffer \| typed-array \| string`; array form = multi-frame join                 |
|  [04]   | `SharpOptions`   | interface            | constructor options: `failOn`/`limitInputPixels`/`density`/`pages`/`animated` + `raw`/`create`/`text`/`join` input modes + per-format input opts |
|  [05]   | `OverlayOptions` | interface            | `extends SharpOptions` — one `composite` layer: `input`/`blend: Blend`/`gravity`/`top`/`left`/`tile` |
|  [06]   | `Metadata`       | interface            | pre-decode probe — `format`/`width`/`height`/`space`/`channels`/`depth`/`hasAlpha`/`orientation`/`exif`/`icc`/`xmp`/`pages`/`autoOrient` |
|  [07]   | `Stats`          | interface            | per-channel `ChannelStats[]` + `entropy`/`sharpness`/`dominant`/`isOpaque`                       |
|  [08]   | `OutputInfo`     | interface            | terminal receipt — `format`/`size`/`width`/`height`/`channels`/`hasAlpha` + crop/trim/attention offsets |
|  [09]   | `Region` / `ExtendOptions` / `TrimOptions` | interface | `extract`/`extend`/`trim` geometry inputs                                              |
|  [10]   | `Create` / `CreateText` / `CreateRaw` / `Join` / `Noise` | interface | generative + raw input descriptors under `SharpOptions.{create,text,raw,join}`         |
|  [11]   | `Exif` / `ExifDir` / `WriteableMetadata` | interface | IFD-keyed EXIF map and the `withMetadata` writeable-channel record                             |
|  [12]   | `Matrix2x2` / `Matrix3x3` / `Matrix4x4` / `Kernel` | type/interface | `affine`/`recomb`/`convolve` operator matrices                                         |

[PUBLIC_TYPE_SCOPE]: format-encoder option family (keyed by `FormatEnum`)
Every encoder-option interface extends `OutputOptions`; `FormatEnum`/`AvailableFormatInfo` is the token vocabulary `toFormat` discriminates over, so one entry owns every encoder.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `OutputOptions`                         | interface     | shared base — `force` (all encoder options extend this)                             |
|  [02]   | `JpegOptions` / `Jp2Options` / `JxlOptions` | interface | quality/progressive/lossless/mozjpeg (JPEG); distance/effort (JXL); tile (JP2)      |
|  [03]   | `PngOptions` / `WebpOptions` / `GifOptions` | interface | palette/effort/dither (PNG/GIF); lossless/nearLossless/smartSubsample/`PresetEnum` (WebP) |
|  [04]   | `AvifOptions` / `HeifOptions`           | interface     | quality/effort/bitdepth/`HeifCompression`(`av1`\|`hevc`)/`HeifTune`                 |
|  [05]   | `TiffOptions` / `RawOptions`            | interface     | compression/predictor/pyramid/bitdepth (TIFF); `DepthEnum` (raw)                    |
|  [06]   | `AnimationOptions`                      | interface     | `loop`/`delay` mixed into WebP/GIF encoders for multi-frame output                  |
|  [07]   | `TileOptions`                           | interface     | deep-zoom pyramid — `size`/`overlap`/`layout: TileLayout`/`container: TileContainer` |
|  [08]   | `AvailableFormatInfo` / `FormatEnum`    | interface     | the format-token vocabulary; `toFormat`'s discriminant and `format` static's shape  |

[PUBLIC_TYPE_SCOPE]: operation-option and enum vocabulary
The resize/filter/morphology operations read these option interfaces and `*Enum` token maps; `Blend` is the 28-mode composite axis and `CacheOptions`/`SharpCounters` the libvips runtime-control payloads.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------------------------- |
|  [01]   | `ResizeOptions`                                         | interface     | `width`/`height`/`fit: FitEnum`/`position`/`kernel: KernelEnum`/`background`/`withoutEnlargement` |
|  [02]   | `SharpenOptions` / `BlurOptions` / `NormaliseOptions` / `ClaheOptions` | interface | filter sigmas, flat/jagged levels, percentile range, CLAHE window     |
|  [03]   | `FlattenOptions` / `NegateOptions` / `ThresholdOptions` / `RotateOptions` / `AffineOptions` | interface | background/alpha/greyscale/interpolator operation knobs |
|  [04]   | `FitEnum` / `KernelEnum` / `PresetEnum` / `BoolEnum`    | object type   | resize-fit, interpolation-kernel, WebP-preset, bitwise-op token maps  |
|  [05]   | `GravityEnum` / `StrategyEnum` / `Interpolators`        | object type   | anchor positions, entropy/attention crop, affine interpolators        |
|  [06]   | `ColourspaceEnum` / `DepthEnum`                         | object type   | pipeline/output colour-space and pixel-depth token maps               |
|  [07]   | `Blend`                                                 | union         | 28 Porter-Duff/PDF composite modes for `OverlayOptions.blend`         |
|  [08]   | `Channels` / `FailOnOptions` / `Precision` / `ExtendWith` / `MediaType` | union | 1-4 band count, decode-abort level, blur precision, extend strategy, MIME |
|  [09]   | `CacheOptions` / `CacheResult` / `SharpCounters` / `TimeoutOptions` | interface | libvips runtime-control payloads/receipts                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and libvips runtime controls (static namespace on the default export)
`sharp(input)` constructs the pipeline; the static namespace on the default export carries the process-global libvips controls a composition-root `Layer` sets once, never per request.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `sharp(input?, options?)` / `new sharp(...)`        | constructor     | pipeline from path/`Buffer`/typed-array/stream/`SharpInput[]`; bare `sharp(options?)` for piped input |
|  [02]   | `sharp.cache(options?)`                             | runtime control | get/set libvips op-cache limits → `CacheResult`                    |
|  [03]   | `sharp.concurrency(n?)` / `sharp.simd(enable?)`     | runtime control | thread count; SIMD (highway) toggle for resize/blur/sharpen        |
|  [04]   | `sharp.block({operation})` / `sharp.unblock({operation})` | runtime control | allow/deny libvips low-level ops (untrusted-input hardening)  |
|  [05]   | `sharp.counters()`                                  | runtime query   | `SharpCounters` queue/process task counts                          |
|  [06]   | `sharp.format` / `versions` / `interpolators` / `queue` | static field | format capability map; libvips dep versions; interpolator map; `EventEmitter` |
|  [07]   | `sharp.gravity` / `strategy` / `kernel` / `fit` / `bool` | static field | the anchor/crop/kernel/fit/bitwise token objects for option values |

[ENTRYPOINT_SCOPE]: geometry — resize, rotate, extract, composite
Order-semantic geometry ops; each returns `Sharp` for chaining before the single terminal fires the deferred recipe.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `resize(widthOrOptions?, height?, options?)` | resize         | scale under `fit`/`kernel`/`position`/`background`; `resize(ResizeOptions)` shorthand |
|  [02]   | `extend(number \| ExtendOptions)` / `extract(Region)` / `trim(options?)` | geometry | pad/extrude edges; crop rectangle; auto-trim to background bbox   |
|  [03]   | `rotate(angle?, options?)` / `autoOrient()`  | transform      | explicit or EXIF-driven rotation; `autoOrient` is the EXIF-only alias |
|  [04]   | `flip(flip?)` / `flop(flop?)` / `affine(matrix, options?)` | transform | Y/X mirror; 2×2 affine under a chosen interpolator                 |
|  [05]   | `composite(OverlayOptions[])`                | composite      | overlay layers with per-layer `Blend`/`gravity`/`top`/`left`/`tile` |

[ENTRYPOINT_SCOPE]: colour, channels, filters, morphology
Colour-space, channel, filter, and morphology ops — all lazy, chainable, and interchangeable in position before the terminal.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `greyscale(b?)`/`grayscale(b?)` / `tint(ColorLike)` / `negate(b\|NegateOptions?)` | colour | monochrome, tint, invert                              |
|  [02]   | `toColourspace(cs?)` / `pipelineColourspace(cs?)` (+`Color`-spelling aliases) | colour | output and pipeline `ColourspaceEnum` selection        |
|  [03]   | `removeAlpha()` / `ensureAlpha(alpha?)` / `extractChannel(0\|1\|2\|3\|'red'..)` / `joinChannel(images, opts?)` / `bandbool(op)` | channel | alpha add/drop, band split/merge, bitwise band fold |
|  [04]   | `sharpen(SharpenOptions?)` / `blur(sigma?\|BlurOptions)` / `median(size?)` / `clahe(ClaheOptions)` | filter | edge sharpen, gaussian blur, median denoise, adaptive-histogram |
|  [05]   | `normalise(opts?)`/`normalize(opts?)` / `gamma(g?, gOut?)` / `threshold(t?, opts?)` / `modulate({brightness,saturation,hue,lightness})` | filter | contrast/tone/binarise/HSL modulation |
|  [06]   | `linear(a?, b?)` / `recomb(Matrix3x3\|Matrix4x4)` / `convolve(Kernel)` / `boolean(operand, op, {raw})` | filter | levels, colour recombination, custom convolution, pixelwise boolean |
|  [07]   | `dilate(width?)` / `erode(width?)` / `flatten(b\|FlattenOptions?)` / `unflatten()` | morphology | grow/shrink foreground; flatten alpha to background; white→transparent |

[ENTRYPOINT_SCOPE]: metadata channel — keep-from-input / set-on-output
The `keep*`/`with*` family is opt-in metadata retention and authorship; default output strips every channel to sRGB.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `keepMetadata()` / `keepExif()` / `keepIccProfile()` / `keepXmp()` | metadata     | preserve input EXIF/ICC/XMP/IPTC (default strips all → sRGB)   |
|  [02]   | `withMetadata(WriteableMetadata?)` / `withDensity(dpi)`          | metadata       | write density/orientation + bulk EXIF/ICC/XMP                  |
|  [03]   | `withExif(Exif)` / `withExifMerge(Exif)` / `withIccProfile(icc, opts?)` / `withXmp(xmp)` | metadata | set/merge EXIF, attach ICC (`srgb`/`p3`/`cmyk` or path), embed XMP |

[ENTRYPOINT_SCOPE]: metadata query and terminal output
`metadata`/`stats` probe pre-decode; `toFormat` encodes; `toBuffer`/`toUint8Array`/`toFile` are the single terminals that fire the lazy chain and yield the `OutputInfo` receipt.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `metadata()` / `metadata(cb)`                                              | query          | `Promise<Metadata>` (or callback) — pre-decode probe          |
|  [02]   | `stats()` / `stats(cb)`                                                    | query          | `Promise<Stats>` per-channel statistics                       |
|  [03]   | `clone()`                                                                  | pipeline       | branch a shared input into independent downstream pipelines    |
|  [04]   | `toFormat(keyof FormatEnum \| AvailableFormatInfo, options?)`             | encode         | the one polymorphic encoder entry; option type resolves by format |
|  [05]   | `jpeg`/`jp2`/`jxl`/`png`/`webp`/`gif`/`avif`/`heif`/`tiff`/`raw`(options?) | encode         | per-format sugar over `toFormat`, each taking its `*Options`   |
|  [06]   | `tile(TileOptions?)` / `timeout({seconds})`                               | encode         | deep-zoom pyramid output; per-image processing deadline        |
|  [07]   | `toFile(path)` / `toFile(path, cb)`                                        | output         | `Promise<OutputInfo>` — format inferred from extension         |
|  [08]   | `toBuffer(options?)` / `toBuffer(cb)`                                      | output         | `Promise<Buffer>`; `{resolveWithObject:true}` → `{data, info}` |
|  [09]   | `toUint8Array()`                                                           | output         | `Promise<{data: Uint8Array, info: OutputInfo}>` — transferable buffer |

## [04]-[IMPLEMENTATION_LAW]

[SHARP_TOPOLOGY]:
- `sharp()` returns a `Sharp extends Duplex`; operations are lazy and never execute until a terminal (`toFile`/`toBuffer`/`toUint8Array`/pipe) fires — the chain is a pure description, so one `Effect.tryPromise` lifts the whole recipe at its single terminal.
- Method order is semantic: `rotate().extract()` ≠ `extract().rotate()`; `resize` default `fit` is `"cover"`; only one rotation applies per pipeline (later `rotate` calls are ignored aside from an initial arg-less EXIF orient).
- `clone()` shares one input stream across independent downstream pipelines — the branch primitive for multi-derivative fan-out (e.g. thumbnail + full-size from one decode).
- Default output strips all metadata and converts to sRGB; the `keep*`/`with*` family is opt-in metadata retention/authorship on the metadata channel {EXIF, ICC, XMP, IPTC, density}.

[LOCAL_ADMISSION]:
- The libvips runtime controls (`concurrency`/`cache`/`simd`/`block`/`unblock`) are process-global and idempotent — set them once in a composition-root `Layer` (`Layer.effect` running `Effect.sync`), never per request.
- Harden untrusted-upload decoding with `sharp.block([...])`/`VIPS_BLOCK_UNTRUSTED` alongside `SharpOptions.failOn`/`limitInputPixels` before the first terminal.
- Method order is semantic (`rotate().extract()` ≠ `extract().rotate()`), one rotation applies per pipeline, and `resize` default `fit` is `"cover"`; `clone()` before a terminal fans one decode into independent derivatives.
- At the single terminal prefer `toUint8Array()` (transferable `{ data, info }`) or `toBuffer({ resolveWithObject: true })` for the `Uint8Array` + `OutputInfo` pair the store's metadata reads; never read the `Duplex` output without a terminal.

[STACKING]:
- The `persistence/object#OBJECT_STORE` `transformImage` seam is canonical: `Effect.tryPromise({ try: () => sharp(input).resize(width).toFormat(format).toBuffer().then((b) => new Uint8Array(b)), catch: (cause) => new ObjectFault({ reason: "encode_failed", key: "", cause }) })` where `format: keyof FormatEnum` — one `toFormat` entry owns every encoder, so a new output format is a token, never a code branch, and the transform fails onto `ObjectFault`, not the export codecs' `AssetTransferFault`.
- The `Uint8Array` (or `{ data, info }` when `resolveWithObject`) feeds `ObjectStore.put`; `OutputInfo.{format,size,width,height,channels}` becomes the object's stored metadata, and `hash-wasm` `XxHash128` (`createXXHash128(0, 0)`, the `interchange#CONTENT_KEY_PARITY` seed regime) over the bytes yields the content-addressed `ObjectKey` — the derivative is idempotent by content, so re-encoding the same source is a cache hit.
- For a body over the streaming threshold, the source `Stream` pipes through the `Sharp` `Duplex` via `@effect/platform-node` `NodeStream.pipeThroughDuplex` (`.api/effect-platform-node.md`; Sharp is both writable sink and readable source) instead of `toBuffer`, matching the store's size-branched `put` (small → buffer, large → `Stream`).
- Sibling `AssetCodec` export codecs — `papaparse` (`csv`, `.api/papaparse.md` `unparse`), `exceljs` (`xlsx`, `.api/exceljs.md` `xlsx.writeBuffer()`), `jspdf` (`pdf`, `.api/jspdf.md` `output("arraybuffer")`), `jszip` (`archive`, `.api/jszip.md` `generateAsync`/`generateNodeStream`) — fold onto the one `Schema.Literal("csv","xlsx","pdf","archive")` axis dispatched by `Match.exhaustive` and fail onto `AssetTransferFault { stage: "encode" }`, while `sharp` is the upstream image transform on that same `put` failing onto its own `ObjectFault { reason: "encode_failed" }`; a `sharp` derivative buffer entering `jszip.file(path, bytes)` bundles image + tabular + document outputs into one streamed archive under one content-addressed `ObjectStore.put`.

[RAIL_LAW]:
- Package: `sharp`
- Owns: image decode, the lazy transform/composite/filter/morphology pipeline, format encode keyed by `FormatEnum`, and libvips runtime control
- Accept: one `Sharp` per input; chain operations before a single terminal; `Effect.tryPromise` at the terminal; `keyof FormatEnum` as the encoder discriminant; `clone()` to fan out derivatives; process-global controls set once in a `Layer`
- Reject: treating `sharp` as a fifth `AssetCodec` export literal instead of the in-pipeline image transform; reading `Sharp` stream output without a terminal call; per-format encoder branches where `toFormat` discriminates; hand-rolled pixel loops or a spawned ImageMagick/`gm` shell; mutating `sharp.versions`/`sharp.queue`; per-request `concurrency`/`cache` mutation
