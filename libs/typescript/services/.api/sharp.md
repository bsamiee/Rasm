# [API_CATALOGUE] sharp

`sharp` supplies high-performance Node.js image processing via libvips: the `sharp()` constructor returns a `Sharp` instance (a `Duplex` stream) that chains resize, crop, rotate, flip, color conversion, compositing, filter, and format-encode operations before writing to file, buffer, or stream. Static utilities (`cache`, `concurrency`, `counters`, `simd`, `block`, `unblock`) control the libvips runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp`
- module: `sharp`
- asset: `sharp` constructor function, `Sharp` interface, option/metadata interfaces, libvips runtime controls
- rail: image-processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instance and config family
- rail: image-processing

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :--------------- | :------------ | :-------------------------------------------- |
|  [01]   | `Sharp`          | interface     | extends `Duplex`; all image ops chain on this |
|  [02]   | `SharpOptions`   | interface     | constructor input options                     |
|  [03]   | `ResizeOptions`  | interface     | resize width/height/fit/position/background   |
|  [04]   | `OverlayOptions` | interface     | composite overlay parameters                  |
|  [05]   | `Metadata`       | interface     | image metadata (format, width, height, space) |
|  [06]   | `OutputInfo`     | interface     | write result (format, size, width, height)    |
|  [07]   | `Stats`          | interface     | per-channel pixel statistics                  |

[PUBLIC_TYPE_SCOPE]: format option family
- rail: image-processing

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------ | :------------ | :----------------------------- |
|  [01]   | `JpegOptions` | interface     | JPEG output options            |
|  [02]   | `PngOptions`  | interface     | PNG output options             |
|  [03]   | `WebpOptions` | interface     | WebP output options            |
|  [04]   | `AvifOptions` | interface     | AVIF output options            |
|  [05]   | `GifOptions`  | interface     | GIF output options             |
|  [06]   | `TiffOptions` | interface     | TIFF output options            |
|  [07]   | `FormatEnum`  | object type   | available format tokens        |
|  [08]   | `FitEnum`     | object type   | resize fit strategy tokens     |
|  [09]   | `KernelEnum`  | object type   | interpolation kernel tokens    |
|  [10]   | `GravityEnum` | object type   | gravity/anchor position tokens |

[PUBLIC_TYPE_SCOPE]: operation option family
- rail: image-processing

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|  [01]   | `SharpenOptions`   | interface     | sigma and flat/jagged levels             |
|  [02]   | `BlurOptions`      | interface     | Gaussian blur options                    |
|  [03]   | `NormaliseOptions` | interface     | histogram-based normalise lower/upper    |
|  [04]   | `ClaheOptions`     | interface     | CLAHE width/height/maxSlope              |
|  [05]   | `FlattenOptions`   | interface     | flatten background colour                |
|  [06]   | `NegateOptions`    | interface     | negate alpha flag                        |
|  [07]   | `ThresholdOptions` | interface     | greyscale threshold flag                 |
|  [08]   | `RotateOptions`    | interface     | rotation background colour               |
|  [09]   | `AffineOptions`    | interface     | affine transform background/interpolator |
|  [10]   | `Kernel`           | interface     | convolution kernel matrix                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and static controls
- rail: image-processing

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `sharp(options?)`                 | constructor    | creates pipeline from scratch / stream input |
|  [02]   | `sharp(input?, options?)`         | constructor    | creates pipeline from file path or Buffer    |
|  [03]   | `sharp.cache(options?)`           | static control | gets/sets libvips op cache limits            |
|  [04]   | `sharp.concurrency(concurrency?)` | static control | gets/sets libvips thread count               |
|  [05]   | `sharp.counters()`                | static query   | internal task counter object                 |
|  [06]   | `sharp.simd(enable?)`             | static control | enables/disables SIMD via highway            |
|  [07]   | `sharp.block({ operation })`      | static control | blocks libvips operations by name            |
|  [08]   | `sharp.unblock({ operation })`    | static control | unblocks libvips operations by name          |

[ENTRYPOINT_SCOPE]: resize, transform, and crop
- rail: image-processing

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `resize(widthOrOptions?, height?, options?)` | resize         | scale with fit, kernel, and background |
|  [02]   | `rotate(angle?, options?)`                   | transform      | explicit or EXIF-driven rotation       |
|  [03]   | `autoOrient()`                               | transform      | alias for EXIF-based `rotate()`        |
|  [04]   | `flip(flip?)`                                | transform      | vertical flip about Y axis             |
|  [05]   | `flop(flop?)`                                | transform      | horizontal flip about X axis           |
|  [06]   | `affine(matrix, options?)`                   | transform      | 2×2 affine matrix transform            |
|  [07]   | `composite(images)`                          | composite      | overlay images with blend modes        |
|  [08]   | `extract(region)`                            | crop           | extract rectangular region             |

[ENTRYPOINT_SCOPE]: color, channel, and filter
- rail: image-processing

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `greyscale(greyscale?)` / `grayscale()` | color          | converts to 8-bit greyscale             |
|  [02]   | `tint(tint)`                            | color          | tints using a `ColorLike` value         |
|  [03]   | `toColourspace(colourspace?)`           | color          | converts output color space             |
|  [04]   | `ensureAlpha(alpha?)`                   | channel        | adds alpha channel if absent            |
|  [05]   | `removeAlpha()`                         | channel        | removes alpha channel                   |
|  [06]   | `extractChannel(channel)`               | channel        | extracts one channel as greyscale       |
|  [07]   | `joinChannel(images, options?)`         | channel        | merges additional channels              |
|  [08]   | `sharpen(options?)`                     | filter         | fast mild or sigma-based sharpen        |
|  [09]   | `blur(sigma?)`                          | filter         | fast mild or Gaussian blur              |
|  [10]   | `median(size?)`                         | filter         | median filter; default 3×3              |
|  [11]   | `normalise(options?)` / `normalize()`   | filter         | histogram-based contrast normalise      |
|  [12]   | `clahe(options)`                        | filter         | contrast-limited adaptive histogram eq. |
|  [13]   | `threshold(threshold?, options?)`       | filter         | binary threshold                        |
|  [14]   | `gamma(gamma?, gammaOut?)`              | filter         | gamma encode/decode correction          |
|  [15]   | `negate(negate?)`                       | filter         | image negative                          |

[ENTRYPOINT_SCOPE]: metadata and output
- rail: image-processing

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `metadata()`                                                               | query          | `Promise<Metadata>`                           |
|  [02]   | `stats()`                                                                  | query          | `Promise<Stats>`                              |
|  [03]   | `toFile(fileOut)`                                                          | output         | `Promise<OutputInfo>` — writes to file        |
|  [04]   | `toBuffer(options?)`                                                       | output         | `Promise<Buffer>` or `{ data, info }`         |
|  [05]   | `toUint8Array()`                                                           | output         | `Promise<{ data: Uint8Array, info }>`         |
|  [06]   | `jpeg(options?)` / `png(…)` / `webp(…)` / `avif(…)` / `gif(…)` / `tiff(…)` | format         | sets output format and options                |
|  [07]   | `toFormat(format, options?)`                                               | format         | sets output format by string or format object |
|  [08]   | `clone()`                                                                  | pipeline       | copies pipeline for multi-output branching    |

## [04]-[IMPLEMENTATION_LAW]

[SHARP_TOPOLOGY]:
- `sharp()` returns a `Sharp` which extends `Duplex`; it accepts piped readable streams or explicit `input` (path, Buffer, or Array of inputs for multi-frame).
- Operations are lazy; processing does not begin until an output method (`toFile`, `toBuffer`, `toUint8Array`, pipe) is called.
- Method order is semantic: `rotate` then `extract` differs from `extract` then `rotate`.
- `resize` default fit is `"cover"`; available fit values are `"cover"`, `"contain"`, `"fill"`, `"inside"`, `"outside"`.
- `clone()` inherits the parent input; all downstream operations on the clone are independent.
- `sharp.queue` is a `NodeJS.EventEmitter` emitting `"change"` events when task queue state changes.

[LOCAL_ADMISSION]:
- `SharpOptions.raw` supplies format metadata for raw pixel input: `{ width, height, channels }`.
- `OverlayOptions` extends `SharpOptions`; `top`/`left` override `gravity` when both are set.
- `sharp.versions` exposes `sharp` and `vips` version strings plus optional libvips dependency versions.
- UV_THREADPOOL_SIZE limits the maximum parallel image pipelines; set before process start.

[RAIL_LAW]:
- Package: `sharp`
- Owns: image decode, transform pipeline, format encode, libvips runtime control
- Accept: one `Sharp` pipeline per input; chain operations before a single terminal output call
- Reject: reading `Sharp` stream output without calling a terminal output method; mutating `sharp.versions` or `sharp.queue` directly
