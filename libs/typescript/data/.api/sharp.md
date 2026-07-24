# [TS_DATA_API_SHARP]

`sharp` is the libvips image processor the `object` plane folds for content-addressed derivative generation: one `sharp(input?, options?)` factory ingests every source through a `SharpOptions` ingress, returns a chainable `Sharp` `Duplex`, and dispatches the whole codec space through `toFormat(format, options)`.

Native and server-bound, its Promise and stream terminals lift into the `Effect` rail at the `object/presign` boundary, where `failOn` and `sharp.block` gate untrusted uploads before decode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp` (Apache-2.0)
- module: `export = sharp` CJS default (`import sharp from "sharp"`), ESM `dist/index.mjs`, types `dist/index.d.mts`
- runtime: server-only node/bun native libvips; a browser `lane/wasm` path never imports the binding
- native: libvips through prebuilt N-API binaries `@img/sharp-<platform>`; `sharp.versions` reports the linked codec versions
- depends: `@img/colour` runtime dependency — its `ColorLike` parses every `background`/`tint`/overlay colour value
- rail: the `store` `object/presign` codec fan-out, lifted into the `Effect` rail at the boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: input, result, and introspection shapes

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]    | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `SharpInput`                      | input union      | every decode source shape             |
|  [02]   | `SharpOptions`                    | ingress config   | input gates and source select         |
|  [03]   | `OutputInfo`                      | terminal receipt | provenance every terminal returns     |
|  [04]   | `Metadata`                        | pre-decode read  | drives the derivative and content-key |
|  [05]   | `Stats` / `ChannelStats`          | pixel analysis   | dominant colour, entropy, sharpness   |
|  [06]   | `TimeoutOptions` / `CacheOptions` | governance       | timeout deadline, libvips cache limit |
|  [07]   | `CacheResult` / `SharpCounters`   | telemetry        | cache stats, in-flight task counters  |

`[GENERATED_INPUT]: `Create` `CreateText` `CreateRaw` `Join` `Noise`` — a solid or noise canvas, Pango text, raw pixels, or a tiled join under `SharpOptions`.

[PUBLIC_TYPE_SCOPE]: format-output and transform-option families

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]    | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `OutputOptions` + codec `*Options`                     | codec params     | the options half of a spec row  |
|  [02]   | `ResizeOptions`                                        | resize params    | derivative geometry and fit     |
|  [03]   | `OverlayOptions`                                       | composite params | watermark layer for `composite` |
|  [04]   | `Region`                                               | geometry params  | crop region for `extract`       |
|  [05]   | `ExtendOptions`                                        | geometry params  | edge extension via `extendWith` |
|  [06]   | `TrimOptions`                                          | geometry params  | auto-trim borders               |
|  [07]   | `Kernel`                                               | geometry params  | custom convolution kernel       |
|  [08]   | `AffineOptions`                                        | geometry params  | affine transform matrix         |
|  [09]   | `WriteableMetadata` / `Exif` / `WithIccProfileOptions` | metadata write   | metadata-write provenance       |
|  [10]   | `TileOptions`                                          | pyramid params   | `tile` deep-zoom output         |

`[OPERATION_OPTIONS]: `SharpenOptions` `BlurOptions` `FlattenOptions` `NegateOptions` `NormaliseOptions` `ClaheOptions` `ThresholdOptions` `RotateOptions`` — the per-operation parameter objects each fold reads.

[PUBLIC_TYPE_SCOPE]: bounded vocabularies, the closed dispatch enums a spec row references by value

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `FormatEnum` / `AvailableFormatInfo`         | codec set       | `keyof FormatEnum` per-codec         |
|  [02]   | `FitEnum`                                    | resize fit      | `ResizeOptions.fit` discriminant     |
|  [03]   | `KernelEnum` / `Interpolators`               | resample set    | reduction kernel, enlargement kernel |
|  [04]   | `GravityEnum` / `StrategyEnum`               | crop anchor     | fixed gravity or smart-crop          |
|  [05]   | `Blend` / `BoolEnum`                         | compositing set | overlay blend math, pixel boolean    |
|  [06]   | `FailOnOptions`                              | safety level    | untrusted-input abort threshold      |
|  [07]   | `ColourspaceEnum` / `DepthEnum` / `Channels` | pixel taxonomy  | colourspace, depth, channel count    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the factory and the chained transform fold

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `sharp(input?, options?) -> Sharp`     | factory  | one decode opening the pipeline     |
|  [02]   | `clone() -> Sharp`                     | instance | snapshot the decode into N branches |
|  [03]   | `composite(OverlayOptions[]) -> Sharp` | fold     | overlay layers by blend and gravity |

Every fold member returns `Sharp`, so the chain is one polymorphic fold.

`[GEOMETRY_FOLD]: `resize` `extend` `extract` `trim` `affine``

`[OPERATION_FOLD]: `rotate` `autoOrient` `flip` `flop` `sharpen` `median` `blur` `dilate` `erode` `gamma` `negate` `normalise` `clahe` `convolve` `threshold` `boolean` `linear` `recomb` `modulate` `flatten` `unflatten``

`[CHANNEL_FOLD]: `removeAlpha` `ensureAlpha` `extractChannel` `joinChannel` `bandbool``

`[COLOUR_FOLD]: `tint` `greyscale` `pipelineColourspace` `toColourspace``

[ENTRYPOINT_SCOPE]: output codecs, terminals, and introspection

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------ | :------- | :------------------------------------------------ |
|  [01]   | `toFormat(format, options?) -> Sharp` | fold     | THE fan-out encoder — `toFormat(row.format, ...)` |
|  [02]   | `toBuffer(opts?) -> Promise`          | instance | derivative bytes and `OutputInfo` receipt         |
|  [03]   | `toFile(path) -> Promise<OutputInfo>` | instance | write to a filesystem sink                        |
|  [04]   | `toUint8Array() -> Promise`           | instance | transferable `Uint8Array` and receipt             |
|  [05]   | `tile(TileOptions) -> Sharp`          | fold     | deep-zoom / IIIF pyramid output                   |
|  [06]   | `metadata() -> Promise<Metadata>`     | instance | pre-decode read                                   |
|  [07]   | `stats() -> Promise<Stats>`           | instance | pixel analysis — `dominant`, `entropy`            |

- `toBuffer`: `{ resolveWithObject: true }` returns `{ data: Buffer, info: OutputInfo }`, otherwise `Promise<Buffer>`.

`[CODEC_ALIAS]: `jpeg` `png` `webp` `avif` `heif` `gif` `tiff` `jp2` `jxl` `raw`` — explicit-codec forms `toFormat` generalizes.

`[METADATA_KEEP]: `withMetadata` `keepMetadata` `keepExif` `withExif` `withExifMerge` `keepIccProfile` `withIccProfile` `keepXmp` `withXmp` `withDensity``

[ENTRYPOINT_SCOPE]: process governance and module statics

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------- | :------ | :--------------------------------- |
|  [01]   | `timeout(TimeoutOptions) -> Sharp`  | fold    | per-pipeline processing deadline   |
|  [02]   | `sharp.block(opts)`                 | static  | restrict untrusted libvips loaders |
|  [03]   | `sharp.cache(opts?) -> CacheResult` | static  | libvips operation-cache limits     |

`[CAPABILITY_STATIC]: `sharp.format` `sharp.versions` `sharp.interpolators` `sharp.queue` `sharp.counters``

`[TUNE_STATIC]: `sharp.concurrency` `sharp.simd` `sharp.unblock``

`[ENUM_STATIC]: `sharp.gravity` `sharp.strategy` `sharp.kernel` `sharp.fit` `sharp.bool``

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `sharp(input, options)` is the single entry for every source, discriminated by `SharpOptions` (`raw`/`create`/`text`/`join`), never a per-source factory family; the returned `Sharp` is a `Duplex`, so it both pipes and terminates.
- Every transform returns `Sharp`, so a derivative is a chain built from a spec row, and `toFormat(format, options)` owns the output space — the codec-named methods (`jpeg`/`png`/`webp`) are aliases it generalizes.
- `sharp` is libvips through prebuilt N-API binaries with an async libuv job queue; the fan-out runs server-side where the object bytes and libvips live.
- `toBuffer`/`toFile`/`metadata`/`stats` are Promises wrapped in `Effect.tryPromise` with a tagged fault, so a decode failure is a typed error-channel member.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts each terminal — `pipeline.toBuffer({ resolveWithObject: true })` returns `{ data, info }` on the success channel, and `metadata`/`stats` become `Effect`s feeding a `Match` on `Metadata.format`.
- `@aws-sdk/client-s3`(`.api/aws-sdk-client-s3.md`): the `GetObjectCommand` response `Body` (`SdkStream<Readable>`) reads once via `Body.transformToByteArray()` into a `Buffer` that opens `sharp(buffer)`, then `clone()` fans per derivative-spec row; each derivative writes back through `PutObjectCommand{ IfNoneMatch: "*", ChecksumSHA256 }`, a 412 status resolving an idempotent noop.
- `@aws-sdk/s3-request-presigner`(`.api/aws-sdk-s3-request-presigner.md`): each derivative row mints one `getSignedUrl` `GetObject` URL, TTL-bounded like the source, for browser-direct delivery.
- `object/presign` + `object/key`: the fan-out is `clone()` + `resize(row.resize)` + `toFormat(row.format, row.options)` over a derivative-spec ROW roster, each derivative content-keyed through kernel `ContentKey` so its conditional-put stays idempotent; `stats().dominant` seeds a placeholder colour and `metadata()` decides the per-row format and geometry.

[LOCAL_ADMISSION]:
- Every terminal wraps in `Effect.tryPromise` with a tagged fault at the `object` boundary, so no raw Promise or sharp throw reaches domain code.
- `object/presign` runs the fan-out as `toFormat(row.format, row.options)` over a derivative-spec roster on one `clone()`d decode; a new derivative is a roster row keyed through `object/key` (kernel `ContentKey`).
- `SharpOptions.failOn` + `sharp.block` + `SharpOptions.limitInputPixels` gate untrusted uploads before decode, and `unlimited: false` bounds decompression-bomb exposure.

[RAIL_LAW]:
- Package: `sharp`
- Owns: libvips image decode, transform, and encode — the polymorphic `sharp(input, options)` ingress, the chained `Sharp` `Duplex` fold grammar (resize, operation, channel, colour, composite), `toFormat` with the codec terminals and `tile`, `metadata`/`stats` introspection, metadata-keep controls, and process governance
- Accept: server-plane use in `object/presign`, `Effect`-lifted terminals with tagged faults, `toFormat`-over-spec-rows fan-out on one `clone()`d decode, untrusted-input gating (`failOn`/`block`/`limitInputPixels`), content keys minted by `object/key`
- Reject: a browser/`lane/wasm` import, a raw Promise or throw crossing into domain code, a hardcoded per-format encoder ladder, unbounded or untrusted decode, sharp owning content addressing or upload idempotency
