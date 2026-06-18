# [API_CATALOGUE] sharp

`sharp` supplies high-performance Node.js image processing via libvips: the `sharp()` constructor returns a `Sharp` instance (a `Duplex` stream) that chains resize, crop, rotate, flip, color conversion, compositing, filter, and format-encode operations before writing to file, buffer, or stream. Static utilities (`cache`, `concurrency`, `counters`, `simd`, `block`, `unblock`) control the libvips runtime.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sharp`
- package: `sharp`
- module: `sharp`
- asset: `sharp` constructor function, `Sharp` interface, option/metadata interfaces, libvips runtime controls
- rail: image-processing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instance and config family
- rail: image-processing

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :--------------- | :------------ | :-------------------------------------------- |
|   [1]   | `Sharp`          | interface     | extends `Duplex`; all image ops chain on this |
|   [2]   | `SharpOptions`   | interface     | constructor input options                     |
|   [3]   | `ResizeOptions`  | interface     | resize width/height/fit/position/background   |
|   [4]   | `OverlayOptions` | interface     | composite overlay parameters                  |
|   [5]   | `Metadata`       | interface     | image metadata (format, width, height, space) |
|   [6]   | `OutputInfo`     | interface     | write result (format, size, width, height)    |
|   [7]   | `Stats`          | interface     | per-channel pixel statistics                  |

[PUBLIC_TYPE_SCOPE]: format option family
- rail: image-processing

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------ | :------------ | :----------------------------- |
|   [1]   | `JpegOptions` | interface     | JPEG output options            |
|   [2]   | `PngOptions`  | interface     | PNG output options             |
|   [3]   | `WebpOptions` | interface     | WebP output options            |
|   [4]   | `AvifOptions` | interface     | AVIF output options            |
|   [5]   | `GifOptions`  | interface     | GIF output options             |
|   [6]   | `TiffOptions` | interface     | TIFF output options            |
|   [7]   | `FormatEnum`  | object type   | available format tokens        |
|   [8]   | `FitEnum`     | object type   | resize fit strategy tokens     |
|   [9]   | `KernelEnum`  | object type   | interpolation kernel tokens    |
|  [10]   | `GravityEnum` | object type   | gravity/anchor position tokens |

[PUBLIC_TYPE_SCOPE]: operation option family
- rail: image-processing

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------- |
|   [1]   | `SharpenOptions`   | interface     | sigma and flat/jagged levels             |
|   [2]   | `BlurOptions`      | interface     | Gaussian blur options                    |
|   [3]   | `NormaliseOptions` | interface     | histogram-based normalise lower/upper    |
|   [4]   | `ClaheOptions`     | interface     | CLAHE width/height/maxSlope              |
|   [5]   | `FlattenOptions`   | interface     | flatten background colour                |
|   [6]   | `NegateOptions`    | interface     | negate alpha flag                        |
|   [7]   | `ThresholdOptions` | interface     | greyscale threshold flag                 |
|   [8]   | `RotateOptions`    | interface     | rotation background colour               |
|   [9]   | `AffineOptions`    | interface     | affine transform background/interpolator |
|  [10]   | `Kernel`           | interface     | convolution kernel matrix                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and static controls
- rail: image-processing

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `sharp(options?)`                 | constructor    | creates pipeline from scratch / stream input |
|   [2]   | `sharp(input?, options?)`         | constructor    | creates pipeline from file path or Buffer    |
|   [3]   | `sharp.cache(options?)`           | static control | gets/sets libvips op cache limits            |
|   [4]   | `sharp.concurrency(concurrency?)` | static control | gets/sets libvips thread count               |
|   [5]   | `sharp.counters()`                | static query   | internal task counter object                 |
|   [6]   | `sharp.simd(enable?)`             | static control | enables/disables SIMD via highway            |
|   [7]   | `sharp.block({ operation })`      | static control | blocks libvips operations by name            |
|   [8]   | `sharp.unblock({ operation })`    | static control | unblocks libvips operations by name          |

[ENTRYPOINT_SCOPE]: resize, transform, and crop
- rail: image-processing

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `resize(widthOrOptions?, height?, options?)` | resize         | scale with fit, kernel, and background |
|   [2]   | `rotate(angle?, options?)`                   | transform      | explicit or EXIF-driven rotation       |
|   [3]   | `autoOrient()`                               | transform      | alias for EXIF-based `rotate()`        |
|   [4]   | `flip(flip?)`                                | transform      | vertical flip about Y axis             |
|   [5]   | `flop(flop?)`                                | transform      | horizontal flip about X axis           |
|   [6]   | `affine(matrix, options?)`                   | transform      | 2×2 affine matrix transform            |
|   [7]   | `composite(images)`                          | composite      | overlay images with blend modes        |
|   [8]   | `extract(region)`                            | crop           | extract rectangular region             |

[ENTRYPOINT_SCOPE]: color, channel, and filter
- rail: image-processing

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `greyscale(greyscale?)` / `grayscale()` | color          | converts to 8-bit greyscale             |
|   [2]   | `tint(tint)`                            | color          | tints using a `ColorLike` value         |
|   [3]   | `toColourspace(colourspace?)`           | color          | converts output color space             |
|   [4]   | `ensureAlpha(alpha?)`                   | channel        | adds alpha channel if absent            |
|   [5]   | `removeAlpha()`                         | channel        | removes alpha channel                   |
|   [6]   | `extractChannel(channel)`               | channel        | extracts one channel as greyscale       |
|   [7]   | `joinChannel(images, options?)`         | channel        | merges additional channels              |
|   [8]   | `sharpen(options?)`                     | filter         | fast mild or sigma-based sharpen        |
|   [9]   | `blur(sigma?)`                          | filter         | fast mild or Gaussian blur              |
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
|   [1]   | `metadata()`                                                               | query          | `Promise<Metadata>`                           |
|   [2]   | `stats()`                                                                  | query          | `Promise<Stats>`                              |
|   [3]   | `toFile(fileOut)`                                                          | output         | `Promise<OutputInfo>` — writes to file        |
|   [4]   | `toBuffer(options?)`                                                       | output         | `Promise<Buffer>` or `{ data, info }`         |
|   [5]   | `toUint8Array()`                                                           | output         | `Promise<{ data: Uint8Array, info }>`         |
|   [6]   | `jpeg(options?)` / `png(…)` / `webp(…)` / `avif(…)` / `gif(…)` / `tiff(…)` | format         | sets output format and options                |
|   [7]   | `toFormat(format, options?)`                                               | format         | sets output format by string or format object |
|   [8]   | `clone()`                                                                  | pipeline       | copies pipeline for multi-output branching    |

## [4]-[IMPLEMENTATION_LAW]

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
