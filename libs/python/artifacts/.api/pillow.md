# [PY_ARTIFACTS_API_PILLOW]

`pillow` supplies the raster image surface for the artifacts image rail: an `Image` object plus a module-level `open`/`new` factory family, a drawing surface (`ImageDraw`), a font surface (`ImageFont`), and pure-function transform/operation modules (`ImageOps`, `ImageFilter`, `ImageEnhance`, `ImageChops`, `ImageColor`) that drive decode, encode, geometric transform, filtering, compositing, and annotation across the supported codec plugins. The package owner composes `Image`, the `open`/`new` factory, and the operation modules into the image owner; it never re-implements raster decode or the codec plugins Pillow already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pillow`
- package: `pillow`
- import: `PIL`
- owner: `artifacts`
- rail: image
- installed: `12.2.0` reflected via `python -c "import PIL"` on the gated `python_version<'3.15'` band (cp313)
- entry points: none (library only)
- capability: raster image decode/encode across plugin codecs, geometric transform, resampling, filtering, enhancement, channel/band ops, compositing, drawing, text rendering, EXIF/XMP metadata, multi-frame sequence access

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, draw, and font types
- rail: image

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]                               |
| :-----: | :-------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Image.Image`               | image object   | pixel buffer with transform/encode methods |
|  [02]   | `ImageFile.ImageFile`       | lazy image     | open-returned image with deferred decode   |
|  [03]   | `ImageDraw.ImageDraw`       | draw surface   | 2D primitives and text onto an image       |
|  [04]   | `ImageFont.FreeTypeFont`    | font handle    | TrueType/OpenType font for text rendering  |
|  [05]   | `ImagePalette.ImagePalette` | palette        | indexed-mode color palette                 |
|  [06]   | `Image.Exif`                | metadata map   | mutable EXIF tag mapping                   |
|  [07]   | `ImageFilter.Filter`        | filter base    | convolution/rank filter base type          |
|  [08]   | `ImageCms.ImageCmsProfile`  | color profile  | ICC color management profile               |

[PUBLIC_TYPE_SCOPE]: mode enums and fault family
- rail: image

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                        |
| :-----: | :----------------------------- | :------------- | :---------------------------------- |
|  [01]   | `Image.Resampling`             | resample enum  | NEAREST/BILINEAR/BICUBIC/LANCZOS    |
|  [02]   | `Image.Transpose`              | transpose enum | flip/rotate-by-90 orientation cases |
|  [03]   | `Image.Dither`                 | dither enum    | NONE/FLOYDSTEINBERG dithering       |
|  [04]   | `Image.Quantize`               | quantize enum  | MEDIANCUT/MAXCOVERAGE/FASTOCTREE    |
|  [05]   | `Image.Transform`              | transform enum | AFFINE/PERSPECTIVE/QUAD/MESH        |
|  [06]   | `Image.UnidentifiedImageError` | decode fault   | unrecognized image data             |
|  [07]   | `Image.DecompressionBombError` | safety fault   | pixel count exceeds the bomb limit  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image open, create, and encode
- rail: image

Factory rows take path/stream/buffer/array sources; the `Image` method rows return a new image or write bytes.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                 | [CAPABILITY]                           |
| :-----: | :-------------------- | :------------------------------------------- | :------------------------------------- |
|  [01]   | `Image.open`          | `open(fp, mode='r', formats=None)`           | open from path or stream (lazy decode) |
|  [02]   | `Image.new`           | `new(mode, size, color=0)`                   | create a blank image                   |
|  [03]   | `Image.frombytes`     | `frombytes(mode, size, data, decoder='raw')` | build image from raw bytes             |
|  [04]   | `Image.fromarray`     | `fromarray(obj, mode=None)`                  | build image from an array interface    |
|  [05]   | `Image.Image.save`    | path/stream plus format/quality kwargs       | encode to a codec format               |
|  [06]   | `Image.Image.tobytes` | `tobytes(encoder_name='raw', *args)`         | encode to raw bytes                    |
|  [07]   | `Image.merge`         | `merge(mode, bands)`                         | merge single-band images into one      |
|  [08]   | `Image.composite`     | `composite(image1, image2, mask)`            | mask-composite two images              |

[ENTRYPOINT_SCOPE]: transform and resample
- rail: image

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                         | [CAPABILITY]                           |
| :-----: | :---------------------- | :----------------------------------- | :------------------------------------- |
|  [01]   | `Image.Image.resize`    | size plus `Resampling` filter        | resample to a new size                 |
|  [02]   | `Image.Image.thumbnail` | bounding size plus filter (in place) | shrink to fit a bounding box           |
|  [03]   | `Image.Image.rotate`    | angle plus resample/expand policy    | rotate by an arbitrary angle           |
|  [04]   | `Image.Image.transpose` | `Transpose` case                     | flip or 90-degree rotate               |
|  [05]   | `Image.Image.crop`      | box tuple                            | crop to a rectangle                    |
|  [06]   | `Image.Image.transform` | size plus `Transform` and data       | affine/perspective/quad/mesh transform |
|  [07]   | `Image.Image.convert`   | target mode plus palette/dither      | convert color mode                     |
|  [08]   | `Image.Image.paste`     | source plus box and mask             | paste a region (in place)              |

[ENTRYPOINT_SCOPE]: draw, font, and operation modules
- rail: image

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                         | [CAPABILITY]                                      |
| :-----: | :------------------------------ | :----------------------------------- | :------------------------------------------------ |
|  [01]   | `ImageDraw.Draw`                | `Draw(im, mode=None) -> ImageDraw`   | bind a draw surface to an image                   |
|  [02]   | `ImageDraw.ImageDraw.text`      | xy plus text/font/fill/anchor        | render text                                       |
|  [03]   | `ImageDraw.ImageDraw.rectangle` | box plus fill/outline/width          | draw a rectangle (also ellipse/line/polygon)      |
|  [04]   | `ImageFont.truetype`            | font path plus size                  | load a TrueType/OpenType font                     |
|  [05]   | `ImageFont.load_default`        | optional size                        | load the built-in default font                    |
|  [06]   | `ImageOps.fit`                  | image plus size and `Resampling`     | crop-and-resize to fill                           |
|  [07]   | `ImageOps.exif_transpose`       | image (honors EXIF orientation)      | apply EXIF orientation                            |
|  [08]   | `ImageEnhance.Contrast`         | `Contrast(im).enhance(factor)`       | adjust contrast (also Color/Brightness/Sharpness) |
|  [09]   | `ImageFilter.GaussianBlur`      | `Image.filter(GaussianBlur(radius))` | Gaussian blur (also UnsharpMask/Kernel)           |

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_RASTER]:
- import: `from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageFilter, ImageEnhance` at boundary scope only; the distribution is `pillow`, the import package is `PIL`.
- open axis: `Image.open` is the single decode factory across every plugin codec; format selection is a registry row keyed by header bytes, never a per-format reader type. `open` is lazy; `load` or first pixel access forces the decode.
- create axis: `Image.new`/`frombytes`/`fromarray` cover blank/raw/array sources; mode is a string row (`RGB`/`RGBA`/`L`/`P`/`CMYK`/`I;16`), never a per-mode image type.
- transform axis: `resize`/`thumbnail`/`rotate`/`transpose`/`transform` accept a `Resampling`/`Transpose`/`Transform` enum case; resampling quality is an enum row, never a parallel resizer.
- operation axis: `ImageOps`/`ImageFilter`/`ImageEnhance`/`ImageChops` are pure-function/factory surfaces returning a new image; enhancement and filtering compose, never mutate the source.
- encode axis: `Image.save` keys the codec by the target extension or explicit `format`; quality/optimize/compression ride save kwargs, never a parallel encoder.
- evidence: each image op captures mode, size, frame count, color channels, and output byte length as an image receipt.
- boundary: Pillow owns raster decode/encode/transform; scientific filtering routes to `scikit-image`; vector PDF routes to the pdf owner; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pillow`
- Owns: raster image decode/encode, geometric transform, resampling, filtering, enhancement, compositing, drawing, text rendering, metadata, multi-frame access
- Accept: image decode/transform/encode feeding the visuals, document, and export-bundle owners
- Reject: wrapper-renames of `open`/`save`; a hand-rolled resampler where `Resampling` exists; a per-mode or per-format image type where one `Image` and a registry row suffice; raster decode the codec plugins already own
