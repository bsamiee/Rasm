# [PY_ARTIFACTS_API_PILLOW]

`pillow` is the in-process working-surface raster owner the `graphic/raster/io#IO` `Raster` page composes as its `RasterEngine.PILLOW` arm: a single mutable `Image` object plus a module-level `open`/`new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` factory family, the pure-function/factory transform modules (`ImageOps`/`ImageFilter`/`ImageEnhance`/`ImageChops`/`ImageMath`/`ImageMorph`/`ImageStat`), a drawing surface (`ImageDraw`), a FreeType-shaped text surface (`ImageFont`), an `ImageCms` lcms2 surface, a multi-frame iterator (`ImageSequence`), and the `register_open`/`register_save`/`PyDecoder`/`PyEncoder` codec-plugin registry that drive decode, encode, geometric transform, resampling, filtering, band/channel algebra, compositing, soft-proofing, drawing, and text rendering across the registered plugins. The package owner composes `Image`, the factory family, the operation modules, the `ImageCms` proof/profile-read surface, and the Arrow/NumPy zero-copy seam into the `graphic/raster/io`/`graphic/color/managed`/`exchange/metadata` arms; every arm crosses the one `execution/lanes#LANE` `WORKER_BAND`-bounded `anyio.to_process.run_sync(..., limiter=WORKER_BAND)` worker seam under a `stamina.AsyncRetryingCaller(...).on(BrokenWorkerProcess)` retry, captures `UnidentifiedImageError`/`DecompressionBombError`/`PyCMSError` into the consuming owner's closed `RasterFault`/`ManagedFault`/`MetaFault` vocabulary, and folds into one `msgspec` receipt projected onto `core/receipt#RECEIPT` `ArtifactReceipt.Preview`. It never re-implements raster decode, the resampling kernels, or the codec plugins Pillow already owns; it is NOT the managed-ICC-conversion engine (pyvips `Image.icc_transform` owns the device→device managed egress) — pillow's surviving color role is the soft-proof/gamut-warning transform (`buildProofTransform`) and the ICC-profile-header read (`ImageCmsProfile` + `getProfile*`) that pyvips does not expose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pillow`
- package: `pillow`
- import: `PIL` (the import package; distribution is `pillow`) — in the consuming pages this is `lazy from PIL import Image, ImageOps, UnidentifiedImageError` / `lazy from PIL import ImageCms` at the worker arm only, never on the core import path, since `PIL` is a host-native worker package riding the `WORKER_BAND`
- owner: `artifacts`
- rail: image (the `graphic/raster/io#IO` `RasterEngine.PILLOW` working surface; the `graphic/color/managed#MANAGED` soft-proof control; the `exchange/metadata#METADATA` ICC-header reader)
- version: `12.2.0`
- build-floor: `Requires-Python >=3.9`; the cp315 wheel is published and the resolver landed it on this interpreter — NOT gated; the `pillow` manifest row carries no `python_version` marker. AVIF/HEIF/WebP/JPEG/TIFF/FreeType/littlecms2/libimagequant/libraqm are compiled into the wheel; `PIL.features.check(feature)` is the runtime build-capability probe (the same native-capability-detection shape the `media/filtergraph#FILTER` filter-registry probe uses) — `check("libjpeg_turbo")`/`check("raqm")`/`check("avif")`/`check_codec("jpg")` gate a build-dependent arm rather than assuming a codec exists
- license: `MIT-CMU` (`License-Expression: MIT-CMU`; permissive, commercial-safe, no copyleft obligation, no Pantone/paid data)
- entry points: none (library only)
- capability: raster decode/encode across the registry codec plugins; geometric transform (resize/thumbnail/reduce/rotate/transpose/affine-perspective-quad-mesh `transform`); resampling under the `Resampling` kernel enum; convolution/rank/multiband filtering + 3D-LUT color grading; tone/recolor enhancement; the full `ImageChops` Porter-Duff/separable blend-mode channel algebra; band split/merge/channel-extract + premultiplied `alpha_composite`; `point`/`ImageMath` per-pixel and multi-band expression LUTs; `ImageStat` masked statistics; `ImageMorph` binary L-mode morphology; ICC soft-proof + gamut-warning (`buildProofTransform`) and profile-header read; ICC-managed `buildTransform`/`applyTransform`/`profileToProfile` (lcms2 — admitted only as the proof engine, NOT the device-egress path pyvips owns); palette quantization (median-cut/octree/libimagequant) + dithering; drawing primitives + measured FreeType text with variation axes; EXIF/XMP metadata maps; multi-frame (GIF/TIFF/APNG/WebP) sequence + embedded-thumbnail access; procedural gradient/noise/Mandelbrot generators; NumPy `__array_interface__` and Arrow `__arrow_c_array__` zero-copy interop; and the `PyDecoder`/`PyEncoder`/`register_*` codec-plugin extension surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, draw, font, palette, and color-management types
- rail: image

`Image.Image` is the ONE mutable pixel-buffer owner — there is no per-mode or per-format image subtype; the mode is a string row (`RGB`/`RGBA`/`L`/`P`/`CMYK`/`LA`/`I;16`/`F`/`1`) and the format is a registry key, never a parallel reader/writer type. `ImageFile.ImageFile` is the lazy-decode subtype `open` returns (deferred decode + the JPEG `draft` override); every operation module returns or mutates an `Image.Image`.

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE] | [CAPABILITY]                                                               |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `Image.Image`                | image object   | the one mutable pixel buffer with transform/encode/band/stat methods       |
|  [02]   | `ImageFile.ImageFile`        | lazy image     | `open`-returned image with deferred decode + `draft` + `get_child_images`  |
|  [03]   | `ImageDraw.ImageDraw`        | draw surface   | 2D primitives, multiline/anchored text, measured text, shape paths         |
|  [04]   | `ImageFont.FreeTypeFont`     | font handle    | FreeType TrueType/OpenType with variation axes (`set_variation_by_*`)      |
|  [05]   | `ImageFont.TransposedFont`   | rotated font   | 90/180/270 + transverse text orientation wrapper                           |
|  [06]   | `ImagePalette.ImagePalette`  | palette        | indexed (`P`-mode) color palette + `sepia`/`wedge`/`make_*_lut`            |
|  [07]   | `Image.Exif`                 | metadata map   | mutable EXIF `MutableMapping` (`getexif`); `get_ifd` nested-IFD access     |
|  [08]   | `ImageStat.Stat`             | statistics     | per-band `mean`/`median`/`stddev`/`var`/`rms`/`extrema`/`sum`/`count`      |
|  [09]   | `ImageFilter.Filter`         | filter base    | convolution/rank/multiband filter base (`MultibandFilter`/`BuiltinFilter`) |
|  [10]   | `ImageFilter.Color3DLUT`     | 3D LUT         | trilinear color-grade table (`Color3DLUT.generate(size, callback)`)        |
|  [11]   | `ImageMorph.MorphOp`         | morphology op  | binary L-mode hit-or-miss morphology over a `LutBuilder` pattern LUT       |
|  [12]   | `ImageCms.ImageCmsProfile`   | color profile  | ICC profile handle (`tobytes`); the profile-header read owner              |
|  [13]   | `ImageCms.ImageCmsTransform` | cms transform  | prebuilt profile-to-profile transform (`apply`/`apply_in_place`/`point`)   |

[PUBLIC_TYPE_SCOPE]: codec-plugin extension surface
- rail: image

The plugin registry is the reason `open`/`save` are single polymorphic factories: format selection is a registered row keyed by header bytes, never a per-format reader type. A new codec is a `PyDecoder`/`PyEncoder` subclass plus a `register_open`/`register_save` row — the artifacts owner composes the built-in plugins and never re-implements decode, but this is the seam a bespoke codec rides. The `register_*` rows are `Image` module functions.

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `ImageFile.PyDecoder`                  | decoder base   | pure-Python decoder (`decode`/`set_as_raw`/`pulls_fd`)        |
|  [02]   | `ImageFile.PyEncoder`                  | encoder base   | pure-Python encoder (`encode`/`encode_to_pyfd`/`pushes_fd`)   |
|  [03]   | `ImageFile.Parser`                     | incremental    | feed-driven progressive decode (`feed`/`image`/`finished`)    |
|  [04]   | `register_open` family                 | registry rows  | bind a plugin's accept/factory/save to a format (keyed below) |
|  [05]   | `register_decoder` family              | registry rows  | bind a codec/extension/MIME to a format (keyed below)         |
|  [06]   | `ImageMode.getmode` / `ModeDescriptor` | mode registry  | resolve a mode string to its band/type/base descriptor        |

- [04]-[REGISTER_OPEN_FAMILY]: `Image.register_open` / `register_save` / `register_save_all` — bind a plugin's accept/factory/save to a format key.
- [05]-[REGISTER_DECODER_FAMILY]: `Image.register_decoder` / `register_encoder` / `register_extension` / `register_mime` — bind a codec/extension/MIME to a format.

[PUBLIC_TYPE_SCOPE]: mode/transform enums, profile intents, and the fault family
- rail: image

The bounded vocabularies the consuming owner keys its arms against — a resample quality selects a `Resampling` member, a quantizer selects a `Quantize` member, an orientation selects a `Transpose` member, a rendering intent selects an `Intent` member. `Dither` carries four members (not two) and `Transpose` seven (the diagonal `TRANSPOSE`/`TRANSVERSE` included).

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [MEMBERS_CAPABILITY]                                                      |
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

- [02]-[TRANSPOSE]: `FLIP_LEFT_RIGHT`/`FLIP_TOP_BOTTOM`/`ROTATE_90`/`ROTATE_180`/`ROTATE_270`/`TRANSPOSE`/`TRANSVERSE` — seven members, the diagonal `TRANSPOSE`/`TRANSVERSE` included.
- [08]-[FLAGS]: `SOFTPROOFING`/`GAMUTCHECK`/`BLACKPOINTCOMPENSATION`/`NOWHITEONWHITEFIXUP`/`HIGHRESPRECALC`/… — the lcms2 `IntFlag` transform flags.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image open, create, encode, and interop
- rail: image

`open` is the single decode factory across every plugin codec (lazy — `load`/`draft`/first pixel access forces decode); `new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` cover blank/raw/zero-copy-buffer/NumPy/Arrow sources; `save` keys the codec by extension or explicit `format`. `fromarrow`/`SupportsArrowArrayInterface` is the zero-copy seam from the Arrow-backed data tier; `tobytes`/`frombytes` round-trip raw planes.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                    | [CAPABILITY]                                  |
| :-----: | :------------------------------------ | :---------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Image.open`                          | `open(fp, mode='r', formats=None) -> ImageFile` | open path/stream (lazy; `formats` restricts)  |
|  [02]   | `Image.new`                           | `new(mode, size, color=0)`                      | create a blank image                          |
|  [03]   | `Image.frombytes` / `frombuffer`      | `frombytes(mode, size, data, …)`                | raw bytes / zero-copy buffer view             |
|  [04]   | `Image.fromarray` / `fromarrow`       | `fromarray(obj, mode=None)`                     | NumPy `array_interface` / Arrow source        |
|  [05]   | `Image.Image.save`                    | `save(fp, format=None, **params)`               | encode to a codec format (kwargs keyed below) |
|  [06]   | `Image.Image.tobytes` / `frombytes`   | `tobytes(encoder_name='raw', *args)`            | encode/decode raw planes in place             |
|  [07]   | `Image.Image.draft`                   | `draft(mode, size)`                             | JPEG decode-scale hint (fast lossy load)      |
|  [08]   | `Image.Image.get_child_images`        | `get_child_images() -> list[ImageFile]`         | embedded thumbnails / sub-images              |
|  [09]   | `Image.merge` / `composite` / `blend` | `merge(mode, bands)`                            | merge / composite / alpha blend (keyed below) |

- [05]-[SAVE_KWARGS]: `save(fp, format=None, **params)` with `quality`/`optimize`/`compress_level`/`progressive`/`lossless`/`icc_profile`/`exif`/`save_all` — codec keyed by extension or explicit `format`.
- [09]-[merge/composite/blend]: `merge(mode, bands)` / `composite(im1, im2, mask)` / `blend(im1, im2, alpha)` — merge single-band images / mask-composite / constant-alpha blend.

- [05]-[SAVE_KWARGS]: `save(fp, format=None, **params)` with `quality`/`optimize`/`compress_level`/`progressive`/`lossless`/`icc_profile`/`exif`/`save_all` — encode to a codec format keyed by extension or explicit `format`.

[ENTRYPOINT_SCOPE]: transform, resample, convert, and quantize
- rail: image

`rotate(angle, resample=Resampling.NEAREST, expand=False, center=None, translate=None, fillcolor=None)` is the full rotate signature. One method per geometric concern, each keyed by a `Resampling`/`Transpose`/`Transform` member — never a parallel resizer. `convert` carries a `matrix` (3×4/4×3 colorspace matrix, e.g. RGB→XYZ) and a `Dither`; `quantize` carries the `Quantize` method + `kmeans` refinement; `reducing_gap` is the two-step high-quality downscale knob shared by `resize`/`thumbnail`.

The transform surfaces are `Image.Image` methods; `rotate(angle, resample=Resampling.NEAREST, expand=False, center=None, translate=None, fillcolor=None)` is the full rotate signature.

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
- rail: image

The channel/stat surface the owner keys instead of a NumPy round-trip when a Pillow band op suffices. `split`/`merge`/`getchannel`/`alpha_composite`/`putalpha` own channel composition; `point` is the per-pixel LUT and `ImageMath.lambda_eval`/`unsafe_eval` the multi-band pixel-expression evaluator; `ImageStat.Stat` is the masked statistics owner (NOT an `Image` method — it wraps an image + optional mask); `histogram`/`getextrema`/`entropy`/`getcolors` are the in-image stats.

The band/stat surfaces are on `Image.Image` unless namespaced (`ImageMath`/`ImageStat`/`ImageSequence`).

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

- [06]-[ImageStat.Stat]: `Stat(image, mask=None)` → `.mean`/`.median`/`.stddev`/`.var`/`.rms`/`.extrema`/`.sum`/`.count` — masked per-band image statistics.

[ENTRYPOINT_SCOPE]: draw, font, operation, filter, and channel-algebra modules
- rail: image

`ImageDraw.ImageDraw.text(xy, text, fill, font, anchor, spacing, align, direction, features, language, stroke_width, stroke_fill, embedded_color)` renders anchored/stroked/shaped text. `ImageDraw`/`ImageFont` own annotation + measured FreeType text (variation axes, complex-script `RAQM` layout, anchored/stroked text); `ImageOps` is the fit/contain/cover/pad/tone/recolor operation family; `ImageFilter` the convolution/rank/3D-LUT family; `ImageEnhance` the tone-adjust factories; `ImageChops` the full blend-mode channel algebra; `ImageMorph` the binary morphology.

The `ImageDraw.ImageDraw.text(xy, text, fill, font, anchor, spacing, align, direction, features, language, stroke_width, stroke_fill, embedded_color)` full signature renders anchored/stroked/shaped text.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                | [CAPABILITY]                                     |
| :-----: | :------------------------------------- | :------------------------------------------ | :----------------------------------------------- |
|  [01]   | `ImageDraw.Draw`                       | `Draw(im, mode=None) -> ImageDraw`          | bind a draw surface to an image                  |
|  [02]   | `ImageDraw.text` / `multiline_text`    | `text(xy, text, fill, font, anchor, …)`     | anchored/stroked/shaped text (sig in lead)       |
|  [03]   | `ImageDraw.textbbox` / `textlength`    | `textbbox(xy, text, font, anchor, …)`       | measure text extent / advance width              |
|  [04]   | `ImageDraw.rounded_rectangle`          | `rounded_rectangle(xy, radius, fill, …)`    | rounded rect + shape family (keyed below)        |
|  [05]   | `ImageDraw.floodfill`                  | `floodfill(image, xy, value, border=None)`  | seed-fill a connected region                     |
|  [06]   | `ImageFont.truetype` / `load_default`  | `truetype(font, size=10, index=0, …)`       | load FreeType / built-in font                    |
|  [07]   | `ImageFont.set_variation_by_axes`      | `set_variation_by_axes([w0, w1, …])`        | variable-font axes / shape text to a mask        |
|  [08]   | `ImageOps.fit`/`contain`/`cover`/`pad` | `fit(image, size, method=…, bleed=0.0, …)`  | fit/contain/cover/pad to a box (`FitMode`)       |
|  [09]   | `ImageOps` op family                   | `exif_transpose(image, in_place=False)`     | EXIF-orient + op family (keyed below)            |
|  [10]   | `ImageEnhance.Contrast`                | `Contrast(im).enhance(factor)`              | contrast (also `Color`/`Brightness`/`Sharpness`) |
|  [11]   | `ImageFilter` family                   | `Image.filter(GaussianBlur(radius))`        | blur/sharpen/convolve/rank/3D-LUT (keyed below)  |
|  [12]   | `ImageChops` blend algebra             | `(im1, im2)`                                | blend-mode + channel algebra (keyed)             |
|  [13]   | `ImageMorph.MorphOp` / `LutBuilder`    | `MorphOp(op_name='dilation4').apply(image)` | binary L-mode morphology over a pattern LUT      |

- [04]-[SHAPE_FAMILY]: `rounded_rectangle(xy, radius, fill, outline, width, corners)`, also `rectangle`/`ellipse`/`circle`/`line`/`polygon`/`arc`/`chord`/`pieslice`/`regular_polygon`/`bitmap`.
- [09]-[IMAGEOPS_OP_FAMILY]: `exif_transpose`/`autocontrast`/`equalize`/`colorize`/`grayscale`/`invert`/`mirror`/`flip`/`posterize`/`solarize`/`expand`/`scale`/`deform`.
- [11]-[IMAGEFILTER]: `GaussianBlur`/`UnsharpMask`/`Kernel`/`RankFilter`/`MedianFilter`/`Color3DLUT` (`Color3DLUT.generate(size, callback)`) — blur/sharpen/NxN-convolve/rank/3D-LUT grade.
- [12]-[IMAGECHOPS]: `multiply`/`screen`/`overlay`/`soft_light`/`hard_light`/`difference`/`add`/`subtract`/`add_modulo`/`darker`/`lighter`/`logical_and` — full Porter-Duff/separable blend-mode + binary channel algebra, each `(im1, im2)`.

- [04]-[SHAPE_FAMILY]: `rounded_rectangle(xy, radius, fill, outline, width, corners)`, also `rectangle`/`ellipse`/`circle`/`line`/`polygon`/`arc`/`chord`/`pieslice`/`regular_polygon`/`bitmap`.
- [09]-[IMAGEOPS_OP_FAMILY]: `exif_transpose`/`autocontrast`/`equalize`/`colorize`/`grayscale`/`invert`/`mirror`/`flip`/`posterize`/`solarize`/`expand`/`scale`/`deform` — EXIF-orient + the tone/recolor/geometric ops.
- [12]-[IMAGECHOPS]: `multiply`/`screen`/`overlay`/`soft_light`/`hard_light`/`difference`/`add`/`subtract`/`add_modulo`/`darker`/`lighter`/`logical_and` — full Porter-Duff/separable blend-mode + binary channel algebra, each `(im1, im2)`.

[ENTRYPOINT_SCOPE]: ICC color management (soft-proof + profile read) and procedural generators
- rail: image

`ImageCms.buildProofTransform(inputProfile, outputProfile, proofProfile, inMode, outMode, renderingIntent=PERCEPTUAL, proofRenderingIntent=ABSOLUTE_COLORIMETRIC, flags=Flags.SOFTPROOFING)` is the soft-proof/gamut-warning transform. `buildTransform`/`applyTransform`/`profileToProfile` are the lcms2 managed-conversion primitives — admitted to the artifacts owner ONLY as the proof/device-link path; the standard device→device managed egress (sRGB↔CMYK with intent + black-point) is pyvips `Image.icc_transform`'s, not this. `buildProofTransform` + `Flags.SOFTPROOFING`/`Flags.GAMUTCHECK` is the soft-proof / gamut-warning transform pyvips has no member for; `ImageCmsProfile` + the `getProfile*` readers are the `exchange/metadata` profile-header read. The `effect_*`/`*_gradient` family seeds procedural rasters for the media-synthesis/test path.

The `ImageCms.*` members carry the `ImageCms` prefix; `buildProofTransform(inputProfile, outputProfile, proofProfile, inMode, outMode, renderingIntent=PERCEPTUAL, proofRenderingIntent=ABSOLUTE_COLORIMETRIC, flags=Flags.SOFTPROOFING)` is the full soft-proof signature.

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

- [05]-[getProfile*]: `getProfileDescription` / `getProfileManufacturer` / `getProfileModel` / `getProfileCopyright` / `getProfileInfo` — read the ICC profile header fields (the `exchange/metadata` `_icc` reader).
- [08]-[PROCEDURAL]: `Image.linear_gradient` / `radial_gradient` / `effect_noise` / `effect_mandelbrot` / `Image.Image.effect_spread` — the procedural raster seeds for the media-synthesis/test path.

- [05]-[getProfile*]: `getProfileDescription` / `getProfileManufacturer` / `getProfileModel` / `getProfileCopyright` / `getProfileInfo` — read the ICC profile header fields (the `exchange/metadata` `_icc` reader).
- [08]-[PROCEDURAL]: `Image.linear_gradient` / `radial_gradient` / `effect_noise` / `effect_mandelbrot` / `Image.Image.effect_spread` — the `effect_*`/`*_gradient` family seeds procedural rasters for the media-synthesis/test path.

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_RASTER]:
- import: `lazy from PIL import Image, ImageOps, UnidentifiedImageError` (the `graphic/raster/io#IO` worker arm) / `lazy from PIL import ImageCms` (the `exchange/metadata#METADATA` and `graphic/color/managed#MANAGED` arms) at the worker-arm body only — `PIL` is a host-native worker package, so the import lands inside the `to_process` worker function that needs it, never on the core page import path. An absent `PIL` (`ImportError`) folds to the host-readiness fault (`RasterFault.provision`), distinct from a content fault.
- worker-seam axis: every Pillow op is synchronous native CPU work; it crosses ONE `anyio.to_process.run_sync(_worker, op, limiter=WORKER_BAND)` subprocess seam bounded by the shared `execution/lanes#LANE` `WORKER_BAND` `CapacityLimiter` the `exchange/detect`/`exchange/metadata`/`graphic/raster/*`/`graphic/color/managed` worker lane shares — never a per-owner `CapacityLimiter(slots)` knob that oversubscribes the host, never the unbounded per-loop default, never the inline event loop, and never a `to_interpreter` subinterpreter (which shares the host interpreter version and cannot host the native stack). The crossing wraps `stamina.AsyncRetryingCaller(...).on(BrokenWorkerProcess)` so a transient OOM/signal worker death recovers before the slot faults.
- open/probe axis: `Image.open` is the single decode factory across every plugin codec — format selection is a registry row keyed by header bytes, never a per-format reader type. `open` is lazy; `Probe` reads `format`/`mode`/`n_frames`/`info["icc_profile"]` off the unloaded image with NO transcode, while `load`/`draft`/first pixel access forces the decode. The JPEG `draft(mode, size)` decode-scale hint is the fast lossy downscale-on-load; `get_child_images` extracts embedded thumbnails.
- create/interop axis: `Image.new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` cover blank/raw/zero-copy-buffer/NumPy/Arrow sources; mode is a string row (`RGB`/`RGBA`/`L`/`P`/`CMYK`/`LA`/`I;16`/`F`/`1`), never a per-mode image type. `fromarrow`/`SupportsArrowArrayInterface` is the zero-copy seam from the Arrow-backed data tier and `np.asarray(image)`/`fromarray` the NumPy `__array_interface__` seam — never a manual buffer marshal; a numpy hand-off into a worker uses `ascontiguousarray` for a C-contiguous plane.
- transform axis: `resize`/`thumbnail`/`reduce`/`rotate`/`transpose`/`transform` accept a `Resampling`/`Transpose`/`Transform` member; resample quality and the `reducing_gap` two-step downscale are enum/kwarg rows, never a parallel resizer. `transform` covers AFFINE/PERSPECTIVE/QUAD/MESH/EXTENT under one method; the `FitMode` arm (`CONTAIN`/`COVER`/`STRETCH`/`PAD`) maps to `ImageOps.contain`/`fit`/`resize`/`pad`.
- convert/quantize axis: `convert(mode, matrix=…, dither=…)` carries the colorspace `matrix` (RGB↔XYZ/Lab) and a `Dither`; `quantize(colors, method=Quantize.<M>, kmeans=…)` selects median-cut/max-coverage/octree/libimagequant; `Dither` is `NONE`/`ORDERED`/`RASTERIZE`/`FLOYDSTEINBERG` and `Transpose` carries the diagonal `TRANSPOSE`/`TRANSVERSE`, not a two-member subset. Alpha flatten for a no-alpha codec is `Image.convert("RGB")`.
- band/stat axis: `split`/`merge`/`getchannel`/`alpha_composite`/`putalpha` own channel composition; `point` is the per-pixel LUT and `ImageMath.lambda_eval`/`unsafe_eval` the multi-band pixel-expression evaluator (band algebra `point` cannot express); `ImageStat.Stat(image, mask)` owns masked per-band statistics; `histogram`/`getextrema`/`entropy`/`getcolors` own in-image stats — never a NumPy round-trip where a Pillow band op suffices.
- operation axis: `ImageOps` (`fit`/`contain`/`cover`/`pad`/`exif_transpose`/`autocontrast`/`equalize`/`colorize`/`grayscale`/`invert`/`mirror`/`posterize`/`solarize`/`expand`/`scale`/`deform`), `ImageFilter` (`GaussianBlur`/`UnsharpMask`/`Kernel`/`Color3DLUT`/`RankFilter`/`MedianFilter`), `ImageEnhance` (`Contrast`/`Color`/`Brightness`/`Sharpness`), `ImageChops` (full blend-mode algebra), and `ImageMorph` (binary L-mode morphology) are pure-function/factory surfaces returning a NEW image; enhancement, filtering, grading, and morphology compose, never mutate the source.
- text axis: `ImageDraw.text`/`multiline_text` with `anchor`/`stroke_width`/`embedded_color`/`features`/`language`/`direction` render annotation; `ImageFont.truetype(..., layout_engine=Layout.RAQM)` selects HarfBuzz/FriBidi complex-script shaping; `FreeTypeFont.set_variation_by_axes`/`get_variation_axes` drive variable-font axes; `textbbox`/`textlength`/`getmask2` measure. (Pillow text serves drawing-on-raster annotation; the typography PLANE — shaping QA, font merge/STAT/freeze, text-on-path — stays `uharfbuzz`/`fonttools`/`vharfbuzz`.)
- color-management axis: pyvips `Image.icc_transform(output_profile, *, input_profile, intent, black_point_compensation, pcs, depth)` is the standard device→device managed egress (the `graphic/color/managed#MANAGED` engine). Pillow `ImageCms` is admitted for exactly two surviving roles: (a) the soft-proof / gamut-warning transform — `buildProofTransform(in, out, proof, inMode, outMode, …, flags=Flags.SOFTPROOFING)` (optionally `| Flags.GAMUTCHECK`), which pyvips cannot express; and (b) the ICC profile-header READ — `ImageCmsProfile(BytesIO(info["icc_profile"]))` then `getProfileDescription`/`getProfileManufacturer`/`getProfileModel`/`getProfileCopyright` + `getDefaultIntent` (mapped through the metadata `_INTENT_NAME` table), each accessor's `PyCMSError` folding to `""` so one missing tag never sinks the rest. A second pillow ICC device-egress engine beside pyvips is the deleted divergence — never `applyTransform`/`profileToProfile` where the managed device conversion belongs to pyvips.
- frame axis: `seek`/`tell` plus `ImageSequence.Iterator`/`all_frames` walk GIF/TIFF/APNG/WebP multi-frame containers; frame count (`n_frames`) and per-frame `duration`/`loop` ride `info`, never a manual offset scan.
- encode axis: `Image.save` keys the codec by the target extension or explicit `format`; `quality`/`optimize`/`compress_level`/`progressive`/`lossless`/`icc_profile`/`exif`/`save_all`/`append_images` ride save kwargs, never a parallel encoder; native AVIF/WebP/HEIF rides the built-in `AvifImagePlugin`/`WebPImagePlugin`/`HeifImagePlugin` when `features.check` confirms the codec.
- plugin axis: a bespoke codec is an `ImageFile.PyDecoder`/`PyEncoder` subclass plus a `register_open`/`register_save`/`register_extension`/`register_mime` row — the registry seam, not a forked decode loop; the artifacts owner composes the built-in plugins and adds none.
- capability-detection axis: `PIL.features.check(feature)`/`check_codec(codec)`/`features.pilinfo()` is the native-build probe (`raqm`/`libjpeg_turbo`/`avif`/`webp_anim`/`freetype2`); a build-dependent arm routes on it (RAQM shaping, AVIF save) rather than assuming the feature — the same capability-detection shape `imagecodecs.<CODEC>.available` and the `media/filtergraph` filter registry use.
- evidence: each op captures mode, size, frame count, color bands, ICC presence/description, soft-proof gamut-warning count, and output byte length as a `msgspec.Struct` field folded into the consuming owner's `RasterFact`/`ManagedFact`/`MetaFacts` and projected onto `ArtifactReceipt.Preview` — Pillow mints no receipt of its own.
- boundary: Pillow owns in-process raster decode/encode/transform/resample/filter/grade/band-algebra/morphology/draw/measured-text + ICC soft-proof + ICC profile-header read. The libvips fused decode/downscale/ICC-managed-egress/smartcrop streaming pipeline is pyvips; scientific filtering/segmentation/registration is `scikit-image`; the typography shaping/merge/freeze plane is `uharfbuzz`/`fonttools`/`vharfbuzz`; vector boolean/offset is `skia-pathops`; layered PSD/PSB authoring is `PhotoshopAPI`/`psd-tools` (channel codecs `imagecodecs`); embedded-PDF image extraction arrives via `pikepdf.models.PdfImage.as_pil_image`; CMYK/spectral math is `colour-science`; live UI (`ImageQt`/`ImageTk`/`ImageGrab`/`ImageShow`) stays outside this package.

[STACKING]:
- The `graphic/raster/io#IO` `Raster` page admits Pillow as the `RasterEngine.PILLOW` member of the `_ENGINE` `EngineOps` bundle: `Thumbnail` runs `ImageOps.exif_transpose(Image.open(BytesIO(payload)))` then the `FitMode`-keyed `ImageOps.contain`/`fit`/`resize`/`pad`; `Convert` runs `Image.save` keyed by the typed `ConvertFormat` with the alpha `Image.convert("RGB")` flatten when the codec carries no alpha; `Crop` runs `Image.crop`; `Probe` runs the lazy `Image.open` header read; `Montage` runs the `Image.new`/`thumbnail`/`paste` grid composite — all under one `_pillow_guarded` capture (`UnidentifiedImageError` → `RasterFault.decode`, `DecompressionBombError` → `RasterFault.bomb`, `OSError`/`ValueError`/`KeyError` → `RasterFault.encode`) inside the `WORKER_BAND` `to_process` seam, never a sibling op per engine. A new pillow-side raster operation is one `RasterOp` case plus one `EngineOps` field plus one pillow arm.
- The `exchange/metadata#METADATA` `_icc` reader composes Pillow as the ICC profile-header substrate beneath the pyvips byte carrier: `pyvips` reads the `icc-profile-data` bytes off the libvips metadata namespace, then `ImageCms.ImageCmsProfile(BytesIO(blob))` + the `getProfile*` accessors + `getDefaultIntent` fold the description/manufacturer/model/copyright/intent into the one `MetaFacts.from_logical(...)` `msgspec.convert(strict=False)` materialization — each accessor wrapped so a `PyCMSError` yields `""`, the raw `ImageCmsProfile` never crossing the owner boundary. Pillow is the ICC HEADER reader, pyvips the profile-byte carrier and re-encode boundary — one role each, no overlap.
- The `graphic/color/managed#MANAGED` page admits Pillow ONLY as a soft-proof control on its `IccTransform` bundle: a `GAMUTCHECK`/soft-proof field selects `buildProofTransform(in, out, proof, inMode, outMode, …, flags=Flags.SOFTPROOFING | Flags.GAMUTCHECK)` + `applyTransform` to simulate the CMYK proof profile and flag out-of-gamut pixels for the PDF/X separations preflight — the device→device managed egress stays pyvips `icc_transform`; pillow's `buildTransform`/`profileToProfile` as a second managed engine is the deleted divergence (a growth row is "a new ICC control (gamut-warning flag, soft-proof profile)" on the existing bundle, never a parallel image writer).
- Every Pillow worker arm rails its provider raise into the consuming owner's closed `@tagged_union` fault (`RasterFault`/`ManagedFault`/`MetaFault`) at the incurring arm and returns through the `expression` `Result[T, Fault]` the rail already speaks (`Ok`/`Error`, `bind`/`map`, `Block`-collected over a per-input batch as `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`) so one corrupt input faults its own slot without aborting the farm — the outcome a structurally addressable typed `RasterFact`/`ManagedFact` case, never an erased `bytes` a consumer re-parses or a bare `except Exception` swallowing an unclassified Pillow raise.
- The op evidence (mode, size, bands, `n_frames`, ICC presence + `getProfileDescription`, soft-proof out-of-gamut count, output byte length) feeds the same `msgspec.Struct` `RasterFact`/`ManagedFact` family projected onto `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, scores)` every other raster/color op contributes — one receipt family, the Pillow working-surface/soft-proof arms as cases, never seven sibling receipt shapes; `numpy.frombuffer(image.tobytes(), dtype).reshape(...)` is the zero-copy bridge when an arm hands a plane to the numeric tier and `np.asarray(image)` the intake from it.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pillow`
- Owns: in-process raster decode/encode, geometric transform, resampling under the `Resampling` kernel, convolution/rank/multiband filtering + 3D-LUT grading, tone/recolor enhancement, the full `ImageChops` blend-mode + binary channel algebra, band split/merge/channel-extract + premultiplied `alpha_composite`, `point`/`ImageMath` per-pixel + multi-band expression LUTs, `ImageStat` masked statistics, `ImageMorph` binary L-mode morphology, palette quantization + dithering, drawing + measured FreeType text (variation axes, `RAQM` shaping), ICC soft-proof + gamut-warning (`buildProofTransform`) and ICC profile-header read (`ImageCmsProfile` + `getProfile*`), EXIF/XMP metadata maps, multi-frame sequence + embedded-thumbnail access, procedural generators, NumPy/Arrow zero-copy interop, and the `PyDecoder`/`PyEncoder`/`register_*` codec-plugin extension surface
- Accept: the `graphic/raster/io#IO` `RasterEngine.PILLOW` working surface (decode/thumbnail/convert/crop/probe/montage on the `WORKER_BAND`); the `exchange/metadata#METADATA` ICC profile-header read; the `graphic/color/managed#MANAGED` soft-proof / gamut-warning control; `pikepdf` extracted images via `as_pil_image`; Arrow/NumPy arrays via `fromarrow`/`fromarray`; build-dependent arms gated on `features.check`
- Reject: a wrapper-rename of `open`/`save`; a hand-rolled resampler where `Resampling` exists; a per-mode or per-format image type where one `Image` and a registry row suffice; a NumPy round-trip where a Pillow band/stat/`ImageMath` op suffices; a second pillow ICC device-egress engine (`buildTransform`/`profileToProfile`) beside pyvips `icc_transform` (only `buildProofTransform` soft-proof + `getProfile*` read survive); a naive `convert` that discards an ICC profile where the soft-proof/header path applies; a two-member `Dither`/`Transpose` enum where four/seven members exist; the inline-event-loop or per-owner-`CapacityLimiter` crossing where the shared `WORKER_BAND` `to_process` seam owns it; a bare `except Exception` where `UnidentifiedImageError`/`DecompressionBombError`/`PyCMSError` map to a closed fault case; raster decode the codec plugins already own; the typography shaping/merge/freeze plane `uharfbuzz`/`fonttools` owns; the live-UI surfaces (`ImageQt`/`ImageTk`/`ImageGrab`/`ImageShow`)
