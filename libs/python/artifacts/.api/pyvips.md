# [PY_ARTIFACTS_API_PYVIPS]

`pyvips` supplies the libvips-backed streaming raster surface for the artifacts image rail: an `Image` whose `new_from_file`/`new_from_buffer`/`new_from_source`/`new_from_memory`/`new_from_array` factory family loads a source, whose generated libvips-operation methods (`thumbnail`/`resize`/`smartcrop`/`extract_area`/`icc_transform`/`colourspace`/`flatten`/`autorot`/`composite2`) transform it through a lazily-evaluated demand-driven pipeline, and whose `write_to_file`/`write_to_buffer`/`write_to_target`/`write_to_memory` family flushes it — plus `Source`/`Target` (and the `SourceCustom`/`TargetCustom` Python-callback variants) for stream IO and `Region` for random-access pixel fetch. The package owner composes `new_from_source`, `thumbnail`/`resize`, `icc_transform`, `smartcrop`, and `write_to_target` over a single `Access.SEQUENTIAL` pipeline that fuses decode + downscale + ICC + crop in one streamed pass; it removes any decode-then-Pillow-downscale two-step where the whole transform can stay in one libvips pipeline, and it never re-implements the demand-driven region scheduler, the SIMD resamplers, the ICC engine, or the per-format codec loaders libvips already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvips`
- package: `pyvips`
- import: `pyvips`
- owner: `artifacts`
- rail: image
- locked: `3.1.1` (`uv.lock`); surface source-verified from the lock-built sdist tree (`vimage.py`, `vsource.py`, `vsourcecustom.py`, `vtarget.py`, `vtargetcustom.py`, `vconnection.py`, `vregion.py`, `vinterpolate.py`, `voperation.py`, `base.py`, `enums.py`, `error.py`). The distribution is sdist-only (no wheels) — a pure-Python `cffi` binding whose only Python dependency is `cffi`; `assay api resolve pyvips` yields no resolution because the package is absent from the live cp315 `.venv` (native libvips not provisioned on this band), so the surface is read from the lock-built module, not live reflection
- license: `LGPL-2.1-or-later` (binding); the native libvips it binds is `LGPL-2.1-or-later` (permissive-copyleft, dynamic-link safe — no static GPL obligation)
- abi: pure-Python `cffi` binding with no compiled extension in the wheel; at import time `__init__.py` loads the system `libvips` shared object (`import _libvips` / cffi `dlopen`) and raises when absent. The native `libvips` (+ its codec stack: libjpeg-turbo/libpng/libtiff/libwebp/libheif/openjpeg/librsvg/libexif/liblcms2/libimagequant) is a Forge-provisioned host dependency, NOT a wheel — runtime detection is gated until libvips is on the loader path; the Python member surface below is independent of provisioning, but the generated operation methods (`thumbnail`/`resize`/`smartcrop`/...) are dispatched against the live libvips introspection, so their availability tracks the provisioned libvips build
- api mode: `pyvips.API_mode` (`True` = cffi API/compile mode, the default; `False` = ABI/`dlopen` mode). API mode is the build-coupled fast path; both expose the identical Python surface
- entry points: none (library only)
- capability: streaming demand-driven raster decode/transform/encode over libvips — load from path/buffer/descriptor/custom-source/raw-memory/array; one-pass shrink-on-load `thumbnail`/`thumbnail_buffer`/`thumbnail_source`; arbitrary `resize`/`reduce`/`shrink` with a `Kernel` resampler; content-aware `smartcrop` via an `Interesting` strategy; `extract_area`/`crop` and `embed`; ICC `icc_import`/`icc_export`/`icc_transform` under an `Intent`; `colourspace`/`cast` interpretation/format conversion; `flatten`/`premultiply`/`unpremultiply`/`addalpha` alpha algebra; `composite`/`composite2` over a `BlendMode`; `autorot`/`rot`/`flip` orientation; the full libvips arithmetic/relational/morphology/convolution operation algebra; tile/line cache control; `numpy`/`new_from_array` zero-copy NumPy interchange; and encode to file/buffer/custom-target with per-format option strings

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, connection, region, interpolator roots
- rail: image

`Image` is the lazily-evaluated pixel-pipeline node; transforms return a new `Image` and nothing computes until a `write_to_*`/`numpy`/`tolist` egress pulls pixels. `Source`/`Target` are the streaming IO endpoints (descriptor/file/memory); `SourceCustom`/`TargetCustom` route reads/writes/seeks through Python callbacks (any file-like, S3 object, in-flight stream). `Region` exposes random-access pixel fetch over a computed image. `Interpolate` is the named sub-pixel interpolator passed to warp/affine operations. Every failure raises the single typed `Error` carrying the libvips error buffer.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                                                                              |
| :-----: | :-------------- | :---------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Image`         | pipeline node     | lazily-evaluated raster pipeline; factory + generated-operation + egress surface    |
|  [02]   | `Source`        | input connection  | streaming read endpoint (`new_from_descriptor`/`new_from_file`/`new_from_memory`)   |
|  [03]   | `SourceCustom`  | input connection  | Python-callback read endpoint (`on_read`/`on_seek`)                                 |
|  [04]   | `Target`        | output connection | streaming write endpoint (`new_to_descriptor`/`new_to_file`/`new_to_memory`)        |
|  [05]   | `TargetCustom`  | output connection | Python-callback write endpoint (`on_write`/`on_read`/`on_seek`/`on_end`/`on_finish`)|
|  [06]   | `Region`        | pixel window      | random-access pixel `fetch(x, y, w, h)` over a computed image                       |
|  [07]   | `Interpolate`   | interpolator      | named sub-pixel interpolator (`new("bicubic")`) for affine/warp ops                |
|  [08]   | `Error`         | engine fault      | a libvips call failed; carries the libvips error buffer message                    |

[PUBLIC_TYPE_SCOPE]: operation-selector enums (string-constant vocabularies)
- rail: image

The enums are string-constant holders feeding the generated operations as keyword rows — pass the constant, never a raw libvips int. `Access` is the load-time streaming hint that decides whether the whole pipeline can run single-pass; `Interesting` is the `smartcrop` strategy; `Intent` is the ICC rendering intent; `Kernel` is the resampler; `BlendMode` drives `composite`. `BandFormat`/`Interpretation`/`Size`/`Extend`/`FailOn` complete the format/geometry/robustness vocabulary.

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]   | [MEMBERS / EFFECT]                                                                        |
| :-----: | :---------------- | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Access`          | streaming hint   | `RANDOM` / `SEQUENTIAL` / `SEQUENTIAL_UNBUFFERED` — `SEQUENTIAL` admits one-pass decode→write |
|  [02]   | `Interesting`     | smartcrop strategy | `NONE` / `CENTRE` / `ENTROPY` / `ATTENTION` / `LOW` / `HIGH` / `ALL` — crop focus model     |
|  [03]   | `Intent`          | ICC intent       | `PERCEPTUAL` / `RELATIVE` / `SATURATION` / `ABSOLUTE` / `AUTO`                            |
|  [04]   | `Kernel`          | resampler        | `NEAREST` / `LINEAR` / `CUBIC` / `MITCHELL` / `LANCZOS2` / `LANCZOS3` / `MKS2013` / `MKS2021` |
|  [05]   | `BlendMode`       | compositing      | Porter-Duff + separable blend modes (`OVER` / `MULTIPLY` / `SCREEN` / `OVERLAY` / ... 25 cases) |
|  [06]   | `BandFormat`      | pixel format     | `uchar` / `ushort` / `float` / ... numeric band layout for `cast`                        |
|  [07]   | `Interpretation`  | colour space     | `srgb` / `rgb16` / `cmyk` / `lab` / `scrgb` / `grey16` / `b-w` / ... pixel colour meaning |
|  [08]   | `Size`            | resize clamp     | `BOTH` / `UP` / `DOWN` / `FORCE` — `thumbnail` upsize/downsize policy                      |
|  [09]   | `Extend`          | edge fill        | `BLACK` / `COPY` / `REPEAT` / `MIRROR` / `WHITE` / `BACKGROUND` — `embed`/border fill      |
|  [10]   | `FailOn`          | decode strictness| `NONE` / `TRUNCATED` / `ERROR` / `WARNING` — load abort threshold                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image construction (one factory family, source-discriminated)
- rail: image

`new_from_file`/`new_from_buffer`/`new_from_source` are the codec-decoding factories; the loader is chosen from the source bytes/extension, and per-loader options (`access`, `fail_on`, `page`, `n`, `dpi`, `shrink`, `unlimited`) ride `**kwargs` or the trailing option string (`"img.jpg[shrink=2]"`). `new_from_memory` wraps an already-decoded raw buffer (no codec), `new_from_array`/`new_from_list` lift host arrays, `new_from_image` builds a constant image matching an existing one's geometry, and `black`/`new_temp_file` mint blanks. Pass `access=Access.SEQUENTIAL` at load when the whole pipeline is one-pass so libvips streams instead of buffering the full raster.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                  | [CAPABILITY]                                              |
| :-----: | :---------------------- | :---------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Image.new_from_file`   | `new_from_file(vips_filename, *, access=..., fail_on=..., **kwargs) -> Image` | decode from a path; loader inferred; options as kwargs/`[...]` |
|  [02]   | `Image.new_from_buffer` | `new_from_buffer(data, options, *, access=..., **kwargs) -> Image`            | decode from in-memory bytes (`options` = trailing option string) |
|  [03]   | `Image.new_from_source` | `new_from_source(source, options, *, access=..., **kwargs) -> Image`          | decode from a `Source`/`SourceCustom` stream             |
|  [04]   | `Image.new_from_memory` | `new_from_memory(data, width, height, bands, format) -> Image`               | wrap an already-decoded raw pixel buffer (no codec)      |
|  [05]   | `Image.new_from_array`  | `new_from_array(obj, scale=1.0, offset=0.0, interpretation=None) -> Image`    | lift a NumPy/array-interface buffer to an image          |
|  [06]   | `Image.new_from_list`   | `new_from_list(array, scale=1.0, offset=0.0) -> Image`                        | build a (convolution-mask) image from a Python list      |
|  [07]   | `Image.new_from_image`  | `new_from_image(value) -> Image`                                             | constant image matching this image's size/format         |
|  [08]   | `Image.new_temp_file`   | `new_temp_file(format) -> Image`                                            | mint a temp-backed image for a multi-pass sink           |

[ENTRYPOINT_SCOPE]: pipeline egress and stream IO
- rail: image

Egress is where the lazy pipeline computes. `write_to_file`/`write_to_buffer` encode to a codec (format from extension/`format_string`; per-encoder options `Q`/`strip`/`compression`/`lossless`/`effort`/`keep` ride `**kwargs` or `".jpg[Q=82]"`); `write_to_target` streams the encode to a `Target`/`TargetCustom`; `write_to_memory`/`numpy`/`tolist` extract raw pixels. `Source`/`Target` mint the streaming endpoints; the `*Custom` variants register Python read/write/seek callbacks so any file-like or network stream feeds the pipeline without a temp file. `Region.fetch` pulls a pixel window for random-access readers.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                       | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :---------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Image.write_to_file`           | `write_to_file(vips_filename, **kwargs) -> None`                  | encode to a path (encoder options as kwargs/`[...]`)     |
|  [02]   | `Image.write_to_buffer`         | `write_to_buffer(format_string, **kwargs) -> bytes`              | encode to a bytes buffer (`format_string` = `".webp"` + opts) |
|  [03]   | `Image.write_to_target`         | `write_to_target(target, format_string, **kwargs) -> None`       | stream the encode to a `Target`/`TargetCustom`           |
|  [04]   | `Image.write_to_memory`         | `write_to_memory() -> bytes`                                     | extract the raw decoded pixel buffer                     |
|  [05]   | `Image.numpy` / `Image.tolist`  | `numpy(dtype=None) -> ndarray`; `tolist() -> list`              | egress to a NumPy array / nested Python list             |
|  [06]   | `Source.new_from_file` / `new_from_memory` / `new_from_descriptor` | `Source.new_from_file(filename)` / `new_from_memory(data)` / `new_from_descriptor(fd)` | mint a streaming input endpoint |
|  [07]   | `SourceCustom.on_read` / `on_seek` | `on_read(handler)` / `on_seek(handler)`                       | register Python read/seek callbacks for an arbitrary stream |
|  [08]   | `Target.new_to_file` / `new_to_memory` / `new_to_descriptor` | `Target.new_to_file(filename)` / `new_to_memory()` / `new_to_descriptor(fd)` | mint a streaming output endpoint |
|  [09]   | `TargetCustom.on_write` / `on_read` / `on_seek` / `on_end` / `on_finish` | `on_write(handler)` / ... | register Python write/seek callbacks for an arbitrary sink |
|  [10]   | `Region.new` / `Region.fetch`   | `Region.new(image) -> Region`; `fetch(x, y, w, h) -> bytes`     | random-access pixel-window fetch over a computed image    |

[ENTRYPOINT_SCOPE]: generated libvips operations (one `__getattr__` dispatch, not a per-op type)
- rail: image

Every libvips operation is a method generated on `Image` via `Image.__getattr__` -> `Operation.call(name, *args, **kwargs)`; there is no per-operation type and no explicit `def` in the binding source — the method set is introspected from the live libvips build, so the names below are the canonical libvips operation nicknames. They chain into one lazy pipeline (each returns a new `Image`); positional args are the required inputs and keyword rows are the optional libvips arguments. `Image.get`/`set`/`get_typeof`/`get_fields` read/write image metadata (EXIF/ICC/orientation) and `Image.get_value("icc-profile-data")` recovers an embedded ICC blob.

| [INDEX] | [OPERATION]                          | [CALL_SHAPE]                                                                   | [CAPABILITY]                                              |
| :-----: | :----------------------------------- | :---------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Image.thumbnail` / `thumbnail_buffer` / `thumbnail_source` | `Image.thumbnail(filename, width, *, height=..., size=Size.DOWN, crop=Interesting.NONE, ...) -> Image` | one-pass shrink-on-load thumbnail (decode+downscale fused) |
|  [02]   | `Image.resize`                       | `image.resize(scale, *, vscale=..., kernel=Kernel.LANCZOS3, gap=...) -> Image` | high-quality arbitrary resample                          |
|  [03]   | `Image.reduce` / `shrink`            | `image.shrink(hshrink, vshrink) -> Image`                                     | integer/area downscale (block average)                   |
|  [04]   | `Image.smartcrop` / `crop` / `extract_area` | `image.smartcrop(width, height, *, interesting=Interesting.ATTENTION) -> Image`; `image.crop(left, top, width, height) -> Image` | content-aware crop / rectangular extract (`crop` aliases `extract_area`) |
|  [05]   | `Image.embed`                        | `image.embed(x, y, width, height, *, extend=Extend.BACKGROUND, background=...) -> Image` | pad/position onto a larger canvas                        |
|  [06]   | `Image.icc_import` / `icc_export` / `icc_transform` | `image.icc_transform(output_profile, *, input_profile=..., intent=Intent.RELATIVE, ...) -> Image` | ICC-managed colour conversion via liblcms2               |
|  [07]   | `Image.colourspace` / `cast`         | `image.colourspace(Interpretation.SRGB) -> Image`; `image.cast(BandFormat) -> Image` | colour-space / numeric-format conversion                 |
|  [08]   | `Image.flatten` / `premultiply` / `unpremultiply` / `addalpha` / `hasalpha` | `image.flatten(*, background=...) -> Image` | alpha composite-to-background / premultiply algebra       |
|  [09]   | `Image.composite` / `composite2`     | `image.composite(overlay, mode, *, x=..., y=...) -> Image`; `image.composite2(overlay, BlendMode.OVER) -> Image` | layer a stack / a pair over a `BlendMode`                |
|  [10]   | `Image.autorot` / `rot` / `fliphor` / `flipver` | `image.autorot() -> Image`                                          | apply/strip EXIF orientation; 90° rotate; flip           |
|  [11]   | `Image.gaussblur` / `sharpen` / `conv` / `median` | `image.gaussblur(sigma) -> Image`; `image.sharpen(*, sigma=...) -> Image` | separable blur / unsharp / convolution / rank filter     |
|  [12]   | `Image.tilecache` / `linecache` / `copy` | `image.tilecache(*, tile_width=..., tile_height=..., max_tiles=...) -> Image` | insert a cache to break a random-access bottleneck       |

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_STREAMING_RASTER]:
- import: `import pyvips` at boundary scope only; module-level import is banned by the manifest import policy. Wrap the import in the libvips-absent guard — a load failure at import time means the host has no `libvips`, which the image owner must surface as a provisioning fault, not a content fault (mirrors the `python-magic`/libmagic gate).
- load axis: `new_from_file`/`new_from_buffer`/`new_from_source` is the single decode factory across every libvips loader; loader selection is introspected from the source bytes/extension, never a per-format reader type. The streaming endpoint (`Source`/`SourceCustom`) is the canonical ingest because admission already holds the stream — `new_from_buffer` only when the payload is fully in memory. `access=Access.SEQUENTIAL` at load is load-bearing: it tells libvips the pipeline is one-pass so it streams instead of buffering the full raster, which is the property that makes the fused decode+downscale+ICC pass O(scanline) not O(image).
- transform axis: every transform is a generated libvips operation method on `Image` dispatched through `__getattr__` -> `Operation.call`, returning a new lazy `Image`; `thumbnail`/`resize`/`smartcrop`/`extract_area`/`icc_transform`/`colourspace`/`flatten`/`composite2`/`autorot` chain into one pipeline that computes only at egress. `Kernel` (resampler), `Interesting` (smartcrop strategy), `Intent` (ICC), `BlendMode` (composite), `Extend` (embed), and `Size` (thumbnail clamp) are keyword-row enums, never raw libvips ints and never a parallel per-strategy method.
- shrink-on-load axis: `thumbnail`/`thumbnail_buffer`/`thumbnail_source` is the fused decode+downscale row — it asks the codec to shrink during decode (JPEG DCT scaling, libvips `shrink-on-load`) so the full-resolution raster is never materialized; a `new_from_file` + separate `resize` two-step is rejected where `thumbnail` fuses them, and a Pillow round-trip for downscale is rejected where the source is already a libvips pipeline.
- colour axis: `icc_import`/`icc_export`/`icc_transform` run liblcms2-backed ICC conversion under an `Intent`; the embedded profile is read with `Image.get_value("icc-profile-data")` and fed to the transform, never a naive `colourspace('srgb')` that discards the source profile. `cast`/`colourspace` own numeric-format and colour-space conversion as enum rows.
- egress axis: `write_to_file`/`write_to_buffer` keys the encoder by extension/`format_string`; encoder options (`Q`/`compression`/`lossless`/`effort`/`strip`/`keep`/`subsample_mode`) ride `**kwargs` or the `[...]` option string, never a parallel encoder type. `write_to_target` streams the encode to a `Target`/`TargetCustom` for a network/segmented sink; `numpy`/`write_to_memory` extract raw pixels for the host array edge. The pipeline computes exactly once at the chosen egress.
- random-access axis: a consumer that needs scattered pixel windows (tiling, deep-zoom) takes a `Region` over the computed image and `fetch(x, y, w, h)`s; a `tilecache`/`linecache` is inserted to break a re-computation bottleneck when an `Access.RANDOM` pipeline is read non-sequentially, never a manual full-`write_to_memory` then slice.
- fault axis: a failed libvips call raises the single typed `Error` carrying the libvips error buffer; lift it to the image fault rail once at the boundary, never let it escape as a bare exception into domain logic.
- evidence: each image op captures source kind, loader/encoder name, the `Access` mode, output width/height/bands/`Interpretation`, ICC presence, the chained operation list, the libvips version (`pyvips.base.version`), and output byte length as an image receipt.
- boundary: pyvips owns streaming raster decode/transform/encode over native libvips; it is the fused-pipeline and large-image owner, while `pillow` owns the in-process pure-Python raster edge (drawing, FreeType text, per-pixel Python LUTs); scientific filtering/morphology routes to `scikit-image`; the rendered raster bytes feed the document/figure owners directly; live UI stays outside this package.

[STACK_INTEGRATION]:
- universal `numpy` tier (`libs/python/.api/numpy.md`): the host pixel seam is `Image.numpy(dtype=...)` out / `Image.new_from_array(arr, interpretation=...)` (or `new_from_memory(buf, w, h, bands, format)`) in — a canonical `numpy` `uint8`/`float32` band-interleaved buffer is the one host raster surface, so a `scikit-image`/`matplotlib`/`pillow`-produced array enters the libvips pipeline and a libvips result leaves to the array tier without a bespoke pixel struct; `numpy` is preferred over `tolist` for any real raster (it is the buffer-protocol zero-copy path).
- universal `anyio` tier (`libs/python/.api/anyio.md`): the `write_to_*`/egress pull is CPU-bound native work, so the boundary owner drives one pipeline per `anyio.to_thread.run_sync` worker (or a `CapacityLimiter`-bounded fan over many inputs); a batch resize/thumbnail farm is N bounded worker tasks each holding one `Access.SEQUENTIAL` pipeline, never an unbounded thread pool, and only the finished `write_to_buffer` bytes cross back to the async caller. libvips has its own internal thread pool (`pyvips.concurrency_set`) for intra-op parallelism — bound the outer `anyio` fan against it so the two pools do not oversubscribe the host.
- universal `expression` tier (`libs/python/.api/expression.md`): the typed `Error` maps at the boundary to a `Result[ImageReceipt, ImageError]` — a loader/encoder-not-found or truncated-decode `Error` is the typed failure arm, a successful `write_to_*` yields the `Ok` receipt; the `try/except pyvips.Error` lives only in the boundary adapter, never in the domain pipeline.
- universal `structlog`/`opentelemetry` tier: the per-op evidence (loader, `Access` mode, dimensions, ICC presence, chained operation list, `pyvips.base.version`, byte length) is the structured event/span payload; the libvips version and feature set are read once at boundary init so they ride the receipt as deployment facts.
- sibling artifacts libs: `python-magic` (`.api/python-magic.md`) is the upstream gate — its `ContentIdentity` MIME branch routes an image payload here vs `pillow`; `pillow` (`.api/pillow.md`) is the still-image/draw/text edge, exchanged as a `numpy` array (libvips `numpy()` out → `Image.fromarray` in, or the reverse) when one engine already holds the buffer, never a re-encoded blob round-trip; the SVG raster pipeline (`resvg-py`/`vl-convert` PNG bytes, `svgelements`-conditioned, `segno` QR) routes its post-raster fused downscale here (`new_from_buffer(png_bytes, access=Access.SEQUENTIAL).thumbnail_image(width)`); the document owners (`docxtpl` `InlineImage`, `pymupdf`/`reportlab`) consume a pyvips-prepared `write_to_buffer` raster as the image descriptor.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyvips`
- Owns: streaming demand-driven raster decode/transform/encode over native libvips — shrink-on-load thumbnailing, arbitrary resample, content-aware smartcrop, extract/embed, ICC colour management, colour-space/format conversion, alpha algebra, compositing, orientation, the libvips operation algebra, tile/line caching, custom-source/target stream IO, and NumPy interchange
- Accept: fused decode+downscale+ICC+crop pipelines and large-image/streaming raster work feeding the document, figure, and export-bundle owners; the `python-magic` MIME image branch; `numpy` arrays via `numpy()`/`new_from_array`; PNG bytes from the SVG raster siblings via `new_from_buffer`
- Reject: wrapper-renames of `new_from_*`/`write_to_*`; a `new_from_file` + separate `resize` two-step where `thumbnail` fuses decode and downscale; a Pillow downscale round-trip where the source is already a libvips pipeline; a naive `colourspace` that discards the source ICC profile where `icc_transform` exists; a per-format reader/writer type where one `Image` and the introspected loader/encoder suffice; raw libvips ints where an `Access`/`Interesting`/`Intent`/`Kernel`/`BlendMode` enum row exists; a full `write_to_memory`-then-slice where a `Region`/`tilecache` covers random access; the demand-driven scheduler, SIMD resamplers, ICC engine, or codec loaders libvips already owns
