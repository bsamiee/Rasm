# [PY_ARTIFACTS_API_PILLOW]

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, draw, font, palette, and color-management types
- concern: image

`Image.Image` is the one mutable pixel-buffer owner: mode is a string row (`RGB`/`RGBA`/`L`/`P`/`CMYK`/`LA`/`I;16`/`F`/`1`) and format a registry key, never a parallel reader/writer type, and every operation module returns or mutates an `Image.Image`. `ImageFile.ImageFile` is the lazy-decode subtype `open` returns.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Image.Image`                | image object  | the one mutable pixel buffer with transform/encode/band/stat methods       |
|  [02]   | `ImageFile.ImageFile`        | lazy image    | `open`-returned image with deferred decode + `draft` + `get_child_images`  |
|  [03]   | `ImageDraw.ImageDraw`        | draw surface  | 2D primitives, multiline/anchored text, measured text, shape paths         |
|  [04]   | `ImageFont.FreeTypeFont`     | font handle   | FreeType TrueType/OpenType with variation axes (`set_variation_by_*`)      |
|  [05]   | `ImageFont.TransposedFont`   | rotated font  | 90/180/270 + transverse text orientation wrapper                           |
|  [06]   | `ImagePalette.ImagePalette`  | palette       | indexed (`P`-mode) color palette + `sepia`/`wedge`/`make_*_lut`            |
|  [07]   | `Image.Exif`                 | metadata map  | mutable EXIF `MutableMapping` (`getexif`); `get_ifd` nested-IFD access     |
|  [08]   | `PngImagePlugin.PngInfo`     | metadata map  | `add_itxt`/`add_text` chunks supplied through `Image.save(pnginfo=)`       |
|  [09]   | `ImageStat.Stat`             | statistics    | per-band `mean`/`median`/`stddev`/`var`/`rms`/`extrema`/`sum`/`count`      |
|  [10]   | `ImageFilter.Filter`         | filter base   | convolution/rank/multiband filter base (`MultibandFilter`/`BuiltinFilter`) |
|  [11]   | `ImageFilter.Color3DLUT`     | 3D LUT        | trilinear color-grade table (`Color3DLUT.generate(size, callback)`)        |
|  [12]   | `ImageMorph.MorphOp`         | morphology op | binary L-mode hit-or-miss morphology over a `LutBuilder` pattern LUT       |
|  [13]   | `ImageCms.ImageCmsProfile`   | color profile | ICC profile handle (`tobytes`); the profile-header read owner              |
|  [14]   | `ImageCms.ImageCmsTransform` | cms transform | prebuilt profile-to-profile transform (`apply`/`apply_in_place`/`point`)   |

[PUBLIC_TYPE_SCOPE]: codec-plugin extension surface
- concern: image

`open`/`save` are single polymorphic factories: format selection is a registered row keyed by header bytes, and a bespoke codec is a `PyDecoder`/`PyEncoder` subclass with a `register_open`/`register_save` row — `register_*` rows are `Image` module functions.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `ImageFile.PyDecoder`                  | decoder base  | pure-Python decoder (`decode`/`set_as_raw`/`pulls_fd`)        |
|  [02]   | `ImageFile.PyEncoder`                  | encoder base  | pure-Python encoder (`encode`/`encode_to_pyfd`/`pushes_fd`)   |
|  [03]   | `ImageFile.Parser`                     | incremental   | feed-driven progressive decode (`feed`/`image`/`finished`)    |
|  [04]   | `register_open` family                 | registry rows | bind a plugin's accept/factory/save to a format (keyed below) |
|  [05]   | `register_decoder` family              | registry rows | bind a codec/extension/MIME to a format (keyed below)         |
|  [06]   | `ImageMode.getmode` / `ModeDescriptor` | mode registry | resolve a mode string to its band/type/base descriptor        |

- [04]-[REGISTER_OPEN_FAMILY]: `Image.register_open` / `register_save` / `register_save_all` bind a plugin's accept/factory/save to a format key.
- [05]-[REGISTER_DECODER_FAMILY]: `Image.register_decoder` / `register_encoder` / `register_extension` / `register_mime` bind a codec/extension/MIME to a format.

[PUBLIC_TYPE_SCOPE]: mode/transform enums, profile intents, and the fault family
- concern: image

Each arm keys against a bounded vocabulary — a resample selects a `Resampling`, a quantizer a `Quantize`, an orientation a `Transpose`, a rendering intent an `Intent`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `Image.Resampling`             | resample enum  | `NEAREST`/`BOX`/`BILINEAR`/`HAMMING`/`BICUBIC`/`LANCZOS`                  |
|  [02]   | `Image.Transpose`              | transpose enum | 7 members (keyed row below)                                               |
|  [03]   | `Image.Dither`                 | dither enum    | `NONE`/`ORDERED`/`RASTERIZE`/`FLOYDSTEINBERG`                             |
|  [04]   | `Image.Quantize`               | quantize enum  | `MEDIANCUT`/`MAXCOVERAGE`/`FASTOCTREE`/`LIBIMAGEQUANT`                    |
|  [05]   | `Image.Transform`              | transform enum | `AFFINE`/`EXTENT`/`PERSPECTIVE`/`QUAD`/`MESH`                             |
|  [06]   | `Image.Palette`                | palette enum   | `WEB`/`ADAPTIVE` palette-source selector for `convert`                    |
|  [07]   | `ImageCms.Intent`              | cms intent     | `PERCEPTUAL`/`RELATIVE_COLORIMETRIC`/`SATURATION`/`ABSOLUTE_COLORIMETRIC` |
|  [08]   | `ImageCms.Flags`               | cms flags      | `IntFlag` (keyed row below)                                               |
|  [09]   | `ImageCms.Direction`           | cms direction  | `INPUT`/`OUTPUT`/`PROOF` (the `isIntentSupported` axis)                   |
|  [10]   | `ImageFont.Layout`             | shaping engine | `BASIC` / `RAQM` (HarfBuzz/FriBidi complex-script layout)                 |
|  [11]   | `Image.UnidentifiedImageError` | decode fault   | unrecognized image data → the owner's `RasterFault.decode`                |
|  [12]   | `Image.DecompressionBombError` | safety fault   | pixel count exceeds the bomb limit → `RasterFault.bomb`                   |
|  [13]   | `ImageCms.PyCMSError`          | cms fault      | malformed profile / unsupported transform → `ManagedFault`                |

- [02]-[TRANSPOSE]: `FLIP_LEFT_RIGHT`/`FLIP_TOP_BOTTOM`/`ROTATE_90`/`ROTATE_180`/`ROTATE_270`/`TRANSPOSE`/`TRANSVERSE` — the diagonal `TRANSPOSE`/`TRANSVERSE` included.
- [08]-[FLAGS]: `SOFTPROOFING`/`GAMUTCHECK`/`BLACKPOINTCOMPENSATION`/`NOWHITEONWHITEFIXUP`/`HIGHRESPRECALC`/… — the lcms2 `IntFlag` transform flags.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image open, create, encode, and interop
- concern: image

`open` is the single lazy decode factory across every plugin codec; `new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` cover blank/raw/zero-copy-buffer/NumPy/Arrow sources and `save` keys the codec by extension or explicit `format`. `fromarrow`/`SupportsArrowArrayInterface` is the Arrow zero-copy boundary.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                    | [CAPABILITY]                                  |
| :-----: | :------------------------------------ | :---------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Image.open`                          | `open(fp, mode='r', formats=None) -> ImageFile` | open path/stream (lazy; `formats` restricts)  |
|  [02]   | `Image.new`                           | `new(mode, size, color=0)`                      | create a blank image                          |
|  [03]   | `Image.frombytes` / `frombuffer`      | `frombytes(mode, size, data, …)`                | raw bytes / zero-copy buffer view             |
|  [04]   | `Image.fromarray` / `fromarrow`       | `fromarray(obj, mode=None)`                     | NumPy `array_interface` / Arrow source        |
|  [05]   | `Image.Image.save`                    | `save(fp, format=None, **params)`               | encode to a codec format (kwargs keyed below) |
|  [06]   | `Image.Image.tobytes` / `frombytes`   | `tobytes(encoder_name='raw', *args)`            | encode/decode raw planes in place             |
|  [07]   | `Image.Image.draft`                   | `draft(mode, size)`                             | JPEG decode-scale hint (fast lossy load)      |
|  [08]   | `ImageFile.get_child_images`          | `get_child_images() -> list[ImageFile]`         | embedded thumbnails / sub-images              |
|  [09]   | `Image.merge` / `composite` / `blend` | `merge(mode, bands)`                            | merge / composite / alpha blend (keyed below) |

- [05]-[SAVE_KWARGS]: `save(fp, format=None, **params)` with `quality`/`optimize`/`compress_level`/`progressive`/`lossless`/`icc_profile`/`exif`/`save_all` — codec keyed by extension or explicit `format`.
- [09]-[MERGE_COMPOSITE_BLEND]: `merge(mode, bands)` / `composite(im1, im2, mask)` / `blend(im1, im2, alpha)` — merge single-band images / mask-composite / constant-alpha blend.

[ENTRYPOINT_SCOPE]: transform, resample, convert, and quantize
- concern: image

One method per geometric concern, each keyed by a `Resampling`/`Transpose`/`Transform` member, never a parallel resizer; `convert` carries a colorspace `matrix` (3×4/4×3, RGB→XYZ) and a `Dither`, `quantize` the `Quantize` method with `kmeans` refinement, and `reducing_gap` is the two-step high-quality downscale shared by `resize`/`thumbnail`.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                      | [CAPABILITY]                              |
| :-----: | :------------------------------ | :------------------------------------------------ | :---------------------------------------- |
|  [01]   | `resize`                        | `resize(size, resample=None, box=None)`           | resample to a new size                    |
|  [02]   | `thumbnail`                     | `thumbnail(size, resample=Resampling.BICUBIC, …)` | shrink to fit a box (in place)            |
|  [03]   | `reduce`                        | `reduce(factor, box=None)`                        | integer-factor box downscale              |
|  [04]   | `rotate`                        | `rotate(angle, resample=…, expand=False, …)`      | rotate by an arbitrary angle              |
|  [05]   | `transpose`                     | `transpose(Transpose member)`                     | flip / 90°-rotate / diagonal transpose    |
|  [06]   | `crop` / `paste`                | `crop(box)` / `paste(im, box=None, mask=None)`    | crop / paste region with optional mask    |
|  [07]   | `transform`                     | `transform(size, method, data, …)`                | AFFINE/PERSPECTIVE/QUAD/MESH/EXTENT       |
|  [08]   | `convert`                       | `convert(mode=None, matrix=None, dither=None, …)` | mode conversion (+ `matrix`/`Dither`)     |
|  [09]   | `quantize`                      | `quantize(colors=256, method=None, kmeans=0, …)`  | palette quantize (median-cut/octree)      |
|  [10]   | `remap_palette` (+ palette I/O) | `remap_palette(dest_map, source_palette=None)`    | reindex / set / read the `P`-mode palette |

[ENTRYPOINT_SCOPE]: band, channel, per-pixel transform, and statistics
- concern: image

`split`/`merge`/`getchannel`/`alpha_composite`/`putalpha` own channel composition, `point` the per-pixel LUT, `ImageMath.lambda_eval`/`unsafe_eval` the multi-band pixel expression, and `ImageStat.Stat` (an image + optional mask, not an `Image` method) masked statistics; `histogram`/`getextrema`/`entropy`/`getcolors` are the in-image stats.

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                             | [CAPABILITY]                                  |
| :-----: | :-------------------------------------- | :--------------------------------------- | :-------------------------------------------- |
|  [01]   | `split` / `getchannel` / `getbands`     | `split()` / `getchannel(channel)`        | band split / extract / band-name tuple        |
|  [02]   | `alpha_composite` / `putalpha`          | `alpha_composite(im, dest, source)`      | premultiplied over-composite / set alpha band |
|  [03]   | `point`                                 | `point(lut, mode=None)`                  | per-pixel LUT / callable map                  |
|  [04]   | `ImageMath.lambda_eval` / `unsafe_eval` | `lambda_eval(expr_fn, **bands)`          | multi-band pixel-expression evaluator         |
|  [05]   | `Image.eval`                            | `eval(image, *fns)`                      | apply a function over every pixel             |
|  [06]   | `ImageStat.Stat`                        | `Stat(image, mask=None)`                 | masked per-band statistics (keyed below)      |
|  [07]   | `histogram` / `getcolors`               | `histogram(mask=None)` / `getcolors(…)`  | per-band histogram / color-count census       |
|  [08]   | `getextrema` / `entropy`                | `getextrema()` / `entropy(mask=None)`    | min/max per band / Shannon entropy            |
|  [09]   | `seek` / `tell`                         | `seek(frame)` / `all_frames(im)`         | multi-frame (GIF/TIFF/APNG/WebP) cursor       |
|  [10]   | `getexif` / `getxmp` / `info`           | `getexif() -> Exif` / `getxmp() -> dict` | EXIF / XMP dict / `info['icc_profile']`       |

- [06]-[IMAGESTAT_STAT]: `Stat(image, mask=None)` → `.mean`/`.median`/`.stddev`/`.var`/`.rms`/`.extrema`/`.sum`/`.count` masked per-band statistics.

[ENTRYPOINT_SCOPE]: draw, font, operation, filter, and channel-algebra modules
- concern: image

`ImageDraw`/`ImageFont` own annotation and measured FreeType text (variation axes, complex-script `RAQM` layout, anchored/stroked text); `ImageOps` is the fit/contain/cover/pad/tone/recolor family, `ImageFilter` the convolution/rank/3D-LUT family, `ImageEnhance` the tone-adjust factories, `ImageChops` the full blend-mode channel algebra, `ImageMorph` binary morphology.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                | [CAPABILITY]                                     |
| :-----: | :------------------------------------- | :------------------------------------------ | :----------------------------------------------- |
|  [01]   | `ImageDraw.Draw`                       | `Draw(im, mode=None) -> ImageDraw`          | bind a draw surface to an image                  |
|  [02]   | `ImageDraw.text` / `multiline_text`    | `text(xy, text, fill, font, anchor, …)`     | anchored/stroked/shaped text                     |
|  [03]   | `ImageDraw.textbbox` / `textlength`    | `textbbox(xy, text, font, anchor, …)`       | measure text extent / advance width              |
|  [04]   | `ImageDraw.rounded_rectangle`          | `rounded_rectangle(xy, radius, fill, …)`    | rounded rect + shape family (keyed below)        |
|  [05]   | `ImageDraw.floodfill`                  | `floodfill(image, xy, value, border=None)`  | seed-fill a connected region                     |
|  [06]   | `ImageFont.truetype` / `load_default`  | `truetype(font, size=10, index=0, …)`       | load FreeType / built-in font                    |
|  [07]   | `ImageFont.set_variation_by_axes`      | `set_variation_by_axes([w0, w1, …])`        | variable-font axes / shape text to a mask        |
|  [08]   | `ImageOps.fit`/`contain`/`cover`/`pad` | `fit(image, size, method=…, bleed=0.0, …)`  | fit/contain/cover/pad to a box (`FitMode`)       |
|  [09]   | `ImageOps` op family                   | `exif_transpose(image, in_place=False)`     | EXIF-orient + op family (keyed below)            |
|  [10]   | `ImageEnhance.Contrast`                | `Contrast(im).enhance(factor)`              | contrast (also `Color`/`Brightness`/`Sharpness`) |
|  [11]   | `ImageFilter` family                   | `Image.filter(GaussianBlur(radius))`        | blur/sharpen/convolve/rank/3D-LUT (keyed below)  |
|  [12]   | `ImageChops` blend algebra             | `(im1, im2)`                                | blend-mode + channel algebra (keyed below)       |
|  [13]   | `ImageMorph.MorphOp` / `LutBuilder`    | `MorphOp(op_name='dilation4').apply(image)` | binary L-mode morphology over a pattern LUT      |

- [04]-[SHAPE_FAMILY]: `rounded_rectangle(xy, radius, fill, outline, width, corners)`, also `rectangle`/`ellipse`/`circle`/`line`/`polygon`/`arc`/`chord`/`pieslice`/`regular_polygon`/`bitmap`.
- [09]-[IMAGEOPS_OP_FAMILY]: `exif_transpose`/`autocontrast`/`equalize`/`colorize`/`grayscale`/`invert`/`mirror`/`flip`/`posterize`/`solarize`/`expand`/`scale`/`deform` — EXIF-orient + the tone/recolor/geometric ops.
- [11]-[IMAGEFILTER]: `GaussianBlur`/`UnsharpMask`/`Kernel`/`RankFilter`/`MedianFilter`/`Color3DLUT` (`Color3DLUT.generate(size, callback)`) — blur/sharpen/NxN-convolve/rank/3D-LUT grade.
- [12]-[IMAGECHOPS]: `multiply`/`screen`/`overlay`/`soft_light`/`hard_light`/`difference`/`add`/`subtract`/`add_modulo`/`darker`/`lighter`/`logical_and` — full Porter-Duff/separable blend-mode + binary channel algebra, each `(im1, im2)`.

[ENTRYPOINT_SCOPE]: ICC color management (soft-proof + profile read) and procedural generators
- concern: image

`buildProofTransform` is the soft-proof/gamut-warning transform pyvips has no member for, `ImageCmsProfile` + `getProfile*` the ICC profile-header read; `buildTransform`/`applyTransform`/`profileToProfile` are lcms2 managed-conversion primitives admitted only as the proof/device-link path, never the device→device egress pyvips owns. `effect_*`/`*_gradient` seeds procedural rasters for the media-synthesis/test path.

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                                      | [CAPABILITY]                          |
| :-----: | :------------------------------------------ | :------------------------------------------------ | :------------------------------------ |
|  [01]   | `buildProofTransform`                       | `buildProofTransform(in, out, proof, …)`          | soft-proof simulate + gamut-warning   |
|  [02]   | `buildTransform` / `applyTransform`         | `buildTransform(…)` / `applyTransform(im, …)`     | profile transform (device-link)       |
|  [03]   | `profileToProfile`                          | `profileToProfile(im, in, out, …)`                | one-shot managed conversion           |
|  [04]   | `ImageCmsProfile` / `getOpenProfile`        | `ImageCmsProfile(BytesIO(blob))`                  | parse an ICC profile from bytes/path  |
|  [05]   | `getProfile*`                               | `getProfileDescription(profile) -> str`           | ICC header fields (keyed below)       |
|  [06]   | `getDefaultIntent` / `isIntentSupported`    | `getDefaultIntent(profile)` / `isIntentSupported` | read default intent / probe an intent |
|  [07]   | `createProfile` / `ImageCmsTransform.apply` | `createProfile('sRGB'\|'LAB'\|'XYZ')`             | built-in profile / apply transform    |
|  [08]   | `*_gradient` / `effect_*`                   | `effect_noise(size, sigma)`                       | procedural raster seeds (keyed below) |

- [05]-[GETPROFILE]: `getProfileDescription` / `getProfileManufacturer` / `getProfileModel` / `getProfileCopyright` / `getProfileInfo` read the ICC profile-header fields (the `exchange/metadata` `_icc` reader).
- [08]-[PROCEDURAL]: `Image.linear_gradient` / `radial_gradient` / `effect_noise` / `effect_mandelbrot` / `Image.Image.effect_spread` seed procedural rasters.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `lazy from PIL import Image, ImageOps, UnidentifiedImageError` (the `graphic/raster/io#IO` worker arm) / `lazy from PIL import ImageCms` (the `exchange/metadata#METADATA` + `graphic/color/managed#MANAGED` arms) inside the worker-arm body only — `PIL` is a host-native worker package, so the import never rides the core page path, and an absent `PIL` `ImportError` folds to `RasterFault.provision`, distinct from a content fault.
- worker boundary: every Pillow op is synchronous native CPU work crossing ONE caller-threaded `lane.offload(Kernel.of(_worker, KernelTrait.HOSTILE), op)` boundary onto the warm loky pool owning the shared `WORKER_BAND` bound the `exchange/detect`/`exchange/metadata`/`graphic/raster/*`/`graphic/color/managed` crossings share; the trait-row `WORKER` retry recovers a transient OOM/signal worker death and the terminal raise crosses the runtime `async_boundary`, never a per-owner `CapacityLimiter(slots)`, the unbounded per-loop default, the inline event loop, or a `to_interpreter` subinterpreter that cannot host the native stack.
- open/probe: `Image.open` is the single lazy decode factory keyed by header bytes; `Probe` reads `format`/`mode`/`n_frames`/`info["icc_profile"]` off the unloaded image with no transcode, `load`/`draft`/first pixel access forces the decode, and JPEG `draft(mode, size)` is the fast lossy downscale-on-load.
- create/interop: `Image.new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` cover blank/raw/NumPy/Arrow sources; `fromarrow`/`SupportsArrowArrayInterface` is the Arrow zero-copy boundary and `np.asarray`/`fromarray` the NumPy `__array_interface__` boundary, a worker hand-off passing a C-contiguous plane through `ascontiguousarray`, never a manual buffer marshal.
- transform: `resize`/`thumbnail`/`reduce`/`rotate`/`transpose`/`transform` key a `Resampling`/`Transpose`/`Transform` member with `reducing_gap` two-step downscale as a kwarg row, never a parallel resizer; `transform` covers AFFINE/PERSPECTIVE/QUAD/MESH/EXTENT under one method and the `FitMode` (`CONTAIN`/`COVER`/`STRETCH`/`PAD`) maps to `ImageOps.contain`/`fit`/`resize`/`pad`.
- convert/quantize: `convert(mode, matrix=…, dither=…)` carries the colorspace `matrix` (RGB↔XYZ/Lab) and a `Dither`; `quantize(colors, method=Quantize.<M>, kmeans=…)` selects median-cut/max-coverage/octree/libimagequant; alpha flatten for a no-alpha codec is `Image.convert("RGB")`.
- band/stat: `split`/`merge`/`getchannel`/`alpha_composite`/`putalpha` own channel composition, `point` the per-pixel LUT, `ImageMath.lambda_eval`/`unsafe_eval` the multi-band expression `point` cannot express, `ImageStat.Stat(image, mask)` masked per-band statistics, and `histogram`/`getextrema`/`entropy`/`getcolors` in-image stats — never a NumPy round-trip where a Pillow band op suffices.
- operation: `ImageOps`/`ImageFilter`/`ImageEnhance`/`ImageChops`/`ImageMorph` are pure-function/factory surfaces returning a NEW image; enhancement, filtering, grading, and morphology compose, never mutate the source.
- text: `ImageDraw.text`/`multiline_text` render `anchor`/`stroke_width`/`embedded_color`/`features`/`language`/`direction` annotation, `ImageFont.truetype(..., layout_engine=Layout.RAQM)` selects HarfBuzz/FriBidi shaping, and `FreeTypeFont.set_variation_by_axes`/`get_variation_axes` drive variable-font axes; the typography shaping/merge/freeze PLANE stays `uharfbuzz`/`fonttools`/`vharfbuzz`.
- color management: pyvips `Image.icc_transform` owns the device→device managed egress; Pillow `ImageCms` is admitted for exactly two roles — the soft-proof/gamut-warning `buildProofTransform(in, out, proof, inMode, outMode, …, flags=Flags.SOFTPROOFING)` (optionally `| Flags.GAMUTCHECK`) pyvips cannot express, and the ICC profile-header READ (`ImageCmsProfile(BytesIO(info["icc_profile"]))` then `getProfileDescription`/`getProfileManufacturer`/`getProfileModel`/`getProfileCopyright` + `getDefaultIntent`, each accessor's `PyCMSError` folding to `""` so one missing tag never sinks the rest); a second pillow ICC device-egress engine (`buildTransform`/`profileToProfile`) is the deleted divergence.
- frame: `seek`/`tell` and `ImageSequence.Iterator`/`all_frames` walk GIF/TIFF/APNG/WebP containers; `n_frames` and per-frame `duration`/`loop` ride `info`, never a manual offset scan.
- encode: `Image.save` keys the codec by the target extension or explicit `format` with `quality`/`optimize`/`compress_level`/`progressive`/`lossless`/`icc_profile`/`exif`/`save_all`/`append_images` as save kwargs; native AVIF and WebP ride the built-in `AvifImagePlugin`/`WebPImagePlugin` when `features.check` confirms the codec, and HEIF ships in NO plugin — `features.check("heif")` warns Unknown feature and answers False, so a HEIC/HEIF target belongs to the libvips writer column or nowhere.
- multi-frame encode: `Image.SAVE` and `Image.SAVE_ALL` (populated by `Image.init()`) are the authoritative single-frame and multi-frame writer registries, and `save_all=True` indexes `SAVE_ALL` DIRECTLY — a format registered in `SAVE` alone (`JPEG`, `BMP`, `QOI`, `JPEG2000`) raises a bare `KeyError(format)` rather than degrading, so the consumer gates multi-frame on its own row and never on the save call. `SAVE_ALL` carries `AVIF`/`GIF`/`MPO`/`PDF`/`PNG`/`TIFF`/`WEBP`, and its per-frame kwargs are per plugin, not universal: `PngImagePlugin` and `GifImagePlugin` read `duration`/`loop`/`disposal`, `WebPImagePlugin` `duration`/`loop`, `AvifImagePlugin` `duration` alone, and `TiffImagePlugin` none at all — a key its plugin never reads attests a timing the file does not carry.
- mode admission: `save` refuses a mode its container cannot hold and the refusal class differs per plugin — `OSError: cannot write mode LA as JPEG`/`as BMP`, `ValueError: Unsupported QOI image mode` for `L`/`LA`/`P` — while `WEBP`, `AVIF`, and `GIF` silently promote or palettize instead. Every consumer therefore resolves the container's admitted mode set BEFORE `save`, since half the roster answers a capability gap as a content exception and the other half answers it as a silent conversion.
- plugin: a bespoke codec is an `ImageFile.PyDecoder`/`PyEncoder` subclass with a `register_open`/`register_save`/`register_extension`/`register_mime` row — the registry boundary, never a forked decode loop.
- capability detection: `PIL.features.check(feature)`/`check_codec(codec)`/`features.pilinfo()` probes the native build (`raqm`/`libjpeg_turbo`/`avif`/`webp`/`jpg_2000`/`freetype2`); a build-dependent arm routes on it, the same shape `imagecodecs.<CODEC>.available` and the `media/filtergraph` registry use. `check` spans `features`, `modules`, and `codecs` and answers only the OPTIONAL surface — a name none of the three carries emits a `UserWarning` and returns False rather than raising, so probing a CORE plugin (`"gif"`, `"bmp"`) reads as an absent encoder and refuses a container every build writes; a core-plugin row therefore carries no feature name and its probe is constant.

[STACKING]:
- `graphic/raster/io#IO` `Raster` admits Pillow as the `RasterEngine.PILLOW` member of the `_ENGINE` `EngineOps` bundle — `Thumbnail`/`Convert`/`Crop`/`Probe`/`Montage` arms run under one `_pillow_guarded` capture (`UnidentifiedImageError` → `RasterFault.decode`, `DecompressionBombError` → `RasterFault.bomb`, `OSError`/`ValueError`/`KeyError` → `RasterFault.encode`) inside the `WORKER_BAND` `to_process` boundary; a new pillow-side raster operation is one `RasterOp` case, one `EngineOps` field, and one pillow arm. Every producing arm reaches `Image.save` through that page's single `_pillow_bytes` egress off its `CODEC` row — format name, save kwargs, the row's `BandLaw` mode resolution, and the `save_all`/`append_images` composition all read the row and the payload arity, so a bare `save(format=…)` inside an arm is the deleted spelling; a container pillow links no encoder for is a missing writer column, a mode it refuses is a `BandLaw` flatten or promotion resolved before the call, and a multi-frame request against a `SAVE`-only format is a `FrameLaw.SINGLE` capability refusal — never a `KeyError` mid-save.
- `exchange/metadata#METADATA` `_icc` composes Pillow as the ICC profile-header substrate beneath the pyvips byte carrier: `ImageCms.ImageCmsProfile(BytesIO(blob))` + `getProfile*` + `getDefaultIntent` fold description/manufacturer/model/copyright/intent into one `MetaFacts.from_logical(...)` `msgspec.convert(strict=False)` materialization, each accessor wrapped so a `PyCMSError` yields `""` and the raw `ImageCmsProfile` never crosses the owner boundary.
- `graphic/color/managed#MANAGED` admits Pillow only as a soft-proof control on its `IccTransform` bundle: a `GAMUTCHECK`/soft-proof field selects `buildProofTransform(..., flags=Flags.SOFTPROOFING | Flags.GAMUTCHECK)` + `applyTransform` to simulate the CMYK proof and flag out-of-gamut pixels for the PDF/X preflight, the device→device egress staying pyvips `icc_transform`.

[LOCAL_ADMISSION]:
- Pillow owns in-process raster decode/encode/transform/resample/filter/grade/band-algebra/morphology/draw/measured-text, ICC soft-proof, and ICC profile-header read; the fused decode/downscale/ICC-managed-egress pipeline is `pyvips` (`.api/pyvips.md`), scientific filtering/segmentation/registration `scikit-image`, the typography shaping/merge/freeze plane `uharfbuzz`/`fonttools`/`vharfbuzz`, vector boolean/offset `skia-pathops`, layered PSD/PSB authoring `psd-tools`, CMYK/spectral math `colour-science`, and live UI `ImageQt`/`ImageTk`/`ImageGrab`/`ImageShow`.
- Embedded-PDF images enter via `pikepdf.models.PdfImage.as_pil_image`; Arrow/NumPy arrays via `fromarrow`/`fromarray`; a build-dependent arm gates on `features.check` before assuming a codec exists.
