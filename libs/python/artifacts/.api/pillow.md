# [PY_ARTIFACTS_API_PILLOW]

`pillow` is the raster image owner for the artifacts image rail. It exposes an `Image` object plus a module-level `open`/`new`/`frombytes`/`fromarray`/`frombuffer`/`fromarrow` factory family, a drawing surface (`ImageDraw`), a FreeType font surface (`ImageFont`), an ICC color-management pipeline (`ImageCms`), a multi-frame sequence iterator (`ImageSequence`), and pure-function transform/operation modules (`ImageOps`, `ImageFilter`, `ImageEnhance`, `ImageChops`, `ImageColor`) that drive decode, encode, geometric transform, resampling, filtering, compositing, color-grade, and annotation across the registered codec plugins. The package owner composes `Image`, the factory family, the operation modules, and the Arrow zero-copy seam into the image owner; it never re-implements raster decode, the resampling kernels, or the codec plugins Pillow already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pillow`
- package: `pillow`
- import: `PIL`
- owner: `artifacts`
- rail: image
- version: `12.2.0`
- license: `MIT-CMU` (permissive; no copyleft obligation)
- abi: per-interpreter CPython wheels; `Requires-Python >=3.10`; manifest-gated `python_version<'3.15'` (no cp315 wheel exists yet, so this rail is dark on the 3.15 dev band and live on the supported cpXXX bands)
- codec features (`PIL.features.check`): `jpg`, `jpg_2000`, `zlib`, `libtiff`, `webp`, `avif`, `freetype2`, `littlecms2`, `xcb` present; `raqm` (complex-script text shaping) absent
- entry points: none (library only)
- capability: raster decode/encode across plugin codecs, geometric transform, resampling, filtering, enhancement, channel/band ops, compositing, 3D-LUT color grading, ICC color management, drawing, FreeType text rendering, EXIF/XMP metadata, multi-frame sequence access, NumPy/Arrow array interop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, draw, font, and color-management types
- rail: image

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]                                       |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Image.Image`               | image object   | pixel buffer with transform/encode/band methods    |
|  [02]   | `ImageFile.ImageFile`       | lazy image     | open-returned image with deferred decode + `draft`  |
|  [03]   | `ImageDraw.ImageDraw`       | draw surface   | 2D primitives, multiline text, shape paths         |
|  [04]   | `ImageFont.FreeTypeFont`    | font handle    | TrueType/OpenType font with variation axes         |
|  [05]   | `ImagePalette.ImagePalette` | palette        | indexed-mode color palette                         |
|  [06]   | `Image.Exif`                | metadata map   | mutable EXIF tag mapping (`getexif`)               |
|  [07]   | `ImageFilter.Filter`        | filter base    | convolution/rank/multiband filter base type        |
|  [08]   | `ImageFilter.Color3DLUT`    | 3D LUT         | trilinear-interpolated color-grade lookup table    |
|  [09]   | `ImageCms.ImageCmsProfile`  | color profile  | ICC color management profile                       |
|  [10]   | `ImageCms.ImageCmsTransform` | cms transform | prebuilt profile-to-profile transform              |

[PUBLIC_TYPE_SCOPE]: mode enums and fault family
- rail: image

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :----------------------------- | :------------- | :------------------------------------ |
|  [01]   | `Image.Resampling`             | resample enum  | NEAREST/BILINEAR/BICUBIC/LANCZOS/BOX/HAMMING |
|  [02]   | `Image.Transpose`              | transpose enum | flip/rotate-by-90 orientation cases   |
|  [03]   | `Image.Dither`                 | dither enum    | NONE/FLOYDSTEINBERG dithering         |
|  [04]   | `Image.Quantize`               | quantize enum  | MEDIANCUT/MAXCOVERAGE/FASTOCTREE/LIBIMAGEQUANT |
|  [05]   | `Image.Transform`              | transform enum | AFFINE/PERSPECTIVE/QUAD/MESH/EXTENT   |
|  [06]   | `ImageCms.Intent` / `Flags`    | cms enums      | perceptual/relative/saturation intent, BPC flags |
|  [07]   | `Image.UnidentifiedImageError` | decode fault   | unrecognized image data               |
|  [08]   | `Image.DecompressionBombError` | safety fault   | pixel count exceeds the bomb limit    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image open, create, encode, and interop
- rail: image

Factory rows take path/stream/buffer/array/Arrow sources; the `Image` method rows return a new image or write bytes.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                            | [CAPABILITY]                            |
| :-----: | :-------------------- | :------------------------------------------------------ | :-------------------------------------- |
|  [01]   | `Image.open`          | `open(fp, mode='r', formats=None) -> ImageFile`         | open from path or stream (lazy decode)  |
|  [02]   | `Image.new`           | `new(mode, size, color=0)`                              | create a blank image                    |
|  [03]   | `Image.frombytes` / `frombuffer` | `frombytes(mode, size, data, decoder_name='raw')` | build image from raw bytes / zero-copy buffer |
|  [04]   | `Image.fromarray` / `fromarrow` | `fromarray(obj, mode=None)` / `fromarrow(obj, mode, size)` | build from NumPy / Arrow array interface |
|  [05]   | `Image.Image.save`    | `save(fp, format=None, **params)`                       | encode to a codec format (quality/optimize/compression kwargs) |
|  [06]   | `Image.Image.tobytes` | `tobytes(encoder_name='raw', *args)`                    | encode to raw bytes                     |
|  [07]   | `ImageFile.ImageFile.draft` | `draft(mode, size)`                               | JPEG-only fast lossy decode-scale hint  |
|  [08]   | `Image.merge` / `Image.composite` | `merge(mode, bands)` / `composite(im1, im2, mask)` | merge single-band images / mask-composite |

[ENTRYPOINT_SCOPE]: transform, resample, and band operations
- rail: image

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                  | [CAPABILITY]                            |
| :-----: | :------------------------ | :-------------------------------------------- | :-------------------------------------- |
|  [01]   | `Image.Image.resize`      | `resize(size, resample=Resampling.BICUBIC, box=None, reducing_gap=None)` | resample to a new size      |
|  [02]   | `Image.Image.thumbnail`   | `thumbnail(size, resample, reducing_gap=2.0)` | shrink to fit a bounding box (in place) |
|  [03]   | `Image.Image.reduce`      | `reduce(factor, box=None)`                    | integer-factor box downscale            |
|  [04]   | `Image.Image.rotate`      | `rotate(angle, resample, expand=False, center=None, fillcolor=None)` | rotate by an arbitrary angle |
|  [05]   | `Image.Image.transpose`   | `transpose(Transpose case)`                   | flip or 90-degree rotate                |
|  [06]   | `Image.Image.crop` / `paste` | `crop(box)` / `paste(im, box=None, mask=None)` | crop to a rectangle / paste region   |
|  [07]   | `Image.Image.transform`   | `transform(size, Transform, data, resample)`  | affine/perspective/quad/mesh transform  |
|  [08]   | `Image.Image.convert` / `quantize` | `convert(mode, palette, dither)` / `quantize(colors, method, dither)` | mode conversion / palette quantize |
|  [09]   | `Image.Image.alpha_composite` / `putalpha` / `getchannel` / `split` | band ops | premultiplied alpha composite, band split/merge |
|  [10]   | `Image.Image.point` / `histogram` / `getextrema` / `entropy` | per-pixel LUT / stats | pixel transform map and image statistics |
|  [11]   | `Image.Image.seek` / `tell` / `ImageSequence.Iterator` | frame cursor | multi-frame (GIF/TIFF/APNG/WebP) access |

[ENTRYPOINT_SCOPE]: draw, font, operation, and color-management modules
- rail: image

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                          | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :---------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `ImageDraw.Draw`                | `Draw(im, mode=None) -> ImageDraw`                    | bind a draw surface to an image                   |
|  [02]   | `ImageDraw.ImageDraw.text`      | `text(xy, text, font, fill, anchor, align)` / `multiline_text` / `textbbox` | render and measure text         |
|  [03]   | `ImageDraw.ImageDraw.rounded_rectangle` | `rounded_rectangle(xy, radius, fill, outline, width)` | rounded rect (also rectangle/ellipse/line/polygon/arc/pieslice) |
|  [04]   | `ImageFont.truetype` / `load_default` | `truetype(font, size, index, encoding, layout_engine)` / `load_default(size)` | load FreeType / built-in font |
|  [05]   | `ImageOps.fit` / `contain` / `cover` / `pad` | `fit(image, size, method=Resampling.BICUBIC, centering)` | fit/contain/cover/pad to a target box |
|  [06]   | `ImageOps.exif_transpose` / `autocontrast` / `equalize` / `colorize` | image | EXIF-orient, tone, and recolor ops |
|  [07]   | `ImageEnhance.Contrast`         | `Contrast(im).enhance(factor)`                        | adjust contrast (also Color/Brightness/Sharpness) |
|  [08]   | `ImageFilter.GaussianBlur` / `UnsharpMask` / `Kernel` / `Color3DLUT` | `Image.filter(GaussianBlur(radius))` | blur/sharpen/convolve/3D-LUT grade |
|  [09]   | `ImageChops.multiply` / `screen` / `overlay` / `difference` / `add` | `(im1, im2)` | full blend-mode channel algebra        |
|  [10]   | `ImageCms.buildTransform` / `applyTransform` / `profileToProfile` | `(in_profile, out_profile, in_mode, out_mode, intent)` | ICC color-managed conversion |

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_RASTER]:
- import: `from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageFilter, ImageEnhance, ImageChops, ImageCms, ImageSequence` at boundary scope only; the distribution is `pillow`, the import package is `PIL`.
- open axis: `Image.open` is the single decode factory across every plugin codec; format selection is a registry row keyed by header bytes, never a per-format reader type. `open` is lazy; `load`, `draft` (JPEG decode-scale), or first pixel access forces the decode.
- create/interop axis: `Image.new`/`frombytes`/`frombuffer`/`fromarray`/`fromarrow` cover blank/raw/zero-copy-buffer/NumPy/Arrow sources; mode is a string row (`RGB`/`RGBA`/`L`/`P`/`CMYK`/`LA`/`I;16`/`F`), never a per-mode image type; `fromarrow`/`SupportsArrowArrayInterface` is the zero-copy seam into the Arrow-backed data tier, never a manual buffer marshal.
- transform axis: `resize`/`thumbnail`/`reduce`/`rotate`/`transpose`/`transform` accept a `Resampling`/`Transpose`/`Transform` enum case; resampling quality and `reducing_gap` two-step downscale are enum/kwarg rows, never a parallel resizer; `transform` covers AFFINE/PERSPECTIVE/QUAD/MESH/EXTENT under one method.
- band/stat axis: `split`/`merge`/`getchannel`/`alpha_composite`/`putalpha` own channel composition; `point`/`histogram`/`getextrema`/`entropy`/`getcolors` own per-pixel transform and statistics, never a NumPy round-trip where a Pillow band op suffices.
- operation axis: `ImageOps` (`fit`/`contain`/`cover`/`pad`/`exif_transpose`/`autocontrast`/`equalize`/`colorize`), `ImageFilter` (`GaussianBlur`/`UnsharpMask`/`Kernel`/`Color3DLUT`/`Rank`/`Median`), `ImageEnhance`, and `ImageChops` (blend-mode algebra) are pure-function/factory surfaces returning a new image; enhancement, filtering, and 3D-LUT grading compose, never mutate the source.
- color-management axis: `ImageCms.buildTransform`/`applyTransform`/`profileToProfile` run ICC-profile color conversion under an `Intent`/`Flags` row through littlecms2; embedded ICC bytes recovered from `info['icc_profile']` (or `pikepdf.models.PdfImage.icc`) feed a managed transform, never a naive `convert('RGB')` that drops the profile.
- text axis: `ImageDraw.text`/`multiline_text`/`textbbox`/`textlength` render and measure with an `ImageFont.FreeTypeFont` (variation axes via `set_variation_by_axes`); complex-script shaping needs the absent `raqm` feature, so RTL/ligature text degrades to per-glyph layout on this build.
- frame axis: `seek`/`tell` plus `ImageSequence.Iterator`/`all_frames` walk GIF/TIFF/APNG/WebP multi-frame containers; frame count and per-frame duration ride `info`, never a manual offset scan.
- encode axis: `Image.save` keys the codec by the target extension or explicit `format`; quality/optimize/compress_level/progressive/lossless/icc_profile/exif ride save kwargs, never a parallel encoder.
- evidence: each image op captures mode, size, frame count, color bands, ICC presence, and output byte length as an image receipt.
- boundary: Pillow owns raster decode/encode/transform/grade/text; scientific filtering and morphology route to `scikit-image`; vector PDF authoring routes to the pdf owner; embedded-PDF image extraction arrives via `pikepdf.models.PdfImage.as_pil_image`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pillow`
- Owns: raster decode/encode, geometric transform, resampling, filtering, enhancement, 3D-LUT grading, ICC color management, compositing, channel/band ops, drawing, FreeType text, EXIF/XMP metadata, multi-frame access, NumPy/Arrow interop
- Accept: image decode/transform/encode feeding the visuals, document, and export-bundle owners; `pikepdf` extracted images via `as_pil_image`; Arrow/NumPy arrays via `fromarrow`/`fromarray`
- Reject: wrapper-renames of `open`/`save`; a hand-rolled resampler where `Resampling` exists; a per-mode or per-format image type where one `Image` and a registry row suffice; a naive `convert` that discards an ICC profile where `ImageCms` exists; raster decode the codec plugins already own
