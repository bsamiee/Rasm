# [PY_ARTIFACTS_API_PYVIPS]

`pyvips` owns the fused streaming-raster engine the `graphic/raster/io#IO` `RasterEngine.LIBVIPS` arm and the `graphic/color/managed#MANAGED` `_icc_apply` arm compose: one lazily-evaluated `Image` whose factory family loads a source, whose generated libvips-operation methods transform it demand-driven, and whose `write_to_*`/`numpy` egress computes the whole pass exactly once. Every arm crosses the `execution/lanes#LANE` process seam through its `lane.offload(Kernel.of(_worker, KernelTrait.HOSTILE), ...)` worker body — pyvips binds a Forge-provisioned native `libvips` off the runtime loader path, so it rides `WORKER_BAND` like the other host-native packages — captures `pyvips.Error` into `RasterFault.engine` and the dlopen `OSError` of an unprovisioned `libvips` into `RasterFault.provision`, folds one `msgspec` `RasterFact`/`ManagedFact`, and projects `core/receipt#RECEIPT` `ArtifactReceipt.Preview`. It is the fused-pipeline / large-image / managed-ICC-egress owner where `pillow` (`.api/pillow.md`) is the in-process pure-Python working surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvips`
- package: `pyvips` (MIT)
- import: `pyvips` (native `libvips` `cffi` binding; ABI-independent pure-Python wheel)
- owner: `artifacts`
- rail: image (the `RasterEngine.LIBVIPS` fused decode/downscale/ICC/smartcrop streaming pipeline; the `_icc_apply` managed device→device ICC egress; the `Probe` header-read ICC/page introspection)
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image, connection, region, interpolator roots

`Image` is the lazily-evaluated pipeline node — transforms return a new `Image` and nothing computes until a `write_to_*`/`numpy`/`tolist` egress pulls pixels. `Source`/`Target` are the streaming IO endpoints (descriptor/file/memory); `SourceCustom`/`TargetCustom` route reads/writes/seeks through Python callbacks (any file-like, S3 object, in-flight stream). `Region` fetches random-access pixel windows over a computed image; `Interpolate` names the sub-pixel interpolator for warp/affine ops; `Error` carries the libvips error buffer on any failed call.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]     | [CAPABILITY]                                                                         |
| :-----: | :------------- | :---------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Image`        | pipeline node     | lazily-evaluated raster pipeline; factory + generated-operation + egress surface     |
|  [02]   | `Source`       | input connection  | streaming read endpoint (`new_from_descriptor`/`new_from_file`/`new_from_memory`)    |
|  [03]   | `SourceCustom` | input connection  | Python-callback read endpoint (`on_read`/`on_seek`)                                  |
|  [04]   | `Target`       | output connection | streaming write endpoint (`new_to_descriptor`/`new_to_file`/`new_to_memory`)         |
|  [05]   | `TargetCustom` | output connection | Python-callback write endpoint (`on_write`/`on_read`/`on_seek`/`on_end`/`on_finish`) |
|  [06]   | `Region`       | pixel window      | random-access pixel `fetch(x, y, w, h)` over a computed image                        |
|  [07]   | `Interpolate`  | interpolator      | named sub-pixel interpolator (`new("bicubic")`) for affine/warp ops                  |
|  [08]   | `Error`        | engine fault      | a libvips call failed; carries the libvips error buffer message                      |

[PUBLIC_TYPE_SCOPE]: operation-selector enums (string-constant vocabularies, `pyvips.enums`)

`pyvips.enums` holds string-constant vocabularies feeding the generated operations as keyword rows — pass the constant (`Interesting.ATTENTION`, `Access.SEQUENTIAL`), never a raw libvips int and never a parallel per-strategy method. `Access` decides one-pass streaming, `Interesting` the smartcrop/`thumbnail crop=` focus, `Intent`+`PCS` the ICC transform, `Kernel` the resampler, `BlendMode` the `composite2` join; the `Foreign*` family carries the per-encoder option vocabularies so a codec reaches its native option set through a typed constant, and the operation-algebra enums select the op for the generic `math`/`relational`/`boolean`/`complex`/`round`/`morph` ops.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]         | [CAPABILITY]                                                                      |
| :-----: | :----------------------- | :-------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Access`                 | streaming hint        | `RANDOM`/`SEQUENTIAL`/`SEQUENTIAL_UNBUFFERED` — `SEQUENTIAL` admits one-pass      |
|  [02]   | `Interesting`            | smartcrop strategy    | `NONE`/`CENTRE`/`ENTROPY`/`ATTENTION`/`LOW`/`HIGH`/`ALL` — crop focus model       |
|  [03]   | `Intent`                 | ICC intent            | `PERCEPTUAL`/`RELATIVE`/`SATURATION`/`ABSOLUTE`/`AUTO`                            |
|  [04]   | `PCS`                    | ICC connection        | `LAB`/`XYZ` — `icc_transform` profile-connection space                            |
|  [05]   | `Kernel`                 | resampler             | `NEAREST`/`LINEAR`/`CUBIC`/`MITCHELL`/`LANCZOS2`/`LANCZOS3`/`MKS2013`/`MKS2021`   |
|  [06]   | `BlendMode`              | compositing           | Porter-Duff + separable modes driving `composite2`                                |
|  [07]   | `BandFormat`             | pixel format          | the `cast` numeric target format                                                  |
|  [08]   | `Interpretation`         | colour space          | the pixel colour-space meaning                                                    |
|  [09]   | `Coding`                 | pixel coding          | `NONE`/`LABQ`/`RAD` — packed-pixel coding                                         |
|  [10]   | `Size`                   | resize clamp          | `BOTH`/`UP`/`DOWN`/`FORCE` — `thumbnail` upsize/downsize policy                   |
|  [11]   | `Extend`                 | edge fill             | `BLACK`/`COPY`/`REPEAT`/`MIRROR`/`WHITE`/`BACKGROUND` — `embed`/`gravity` fill    |
|  [12]   | `FailOn`                 | decode strictness     | `NONE`/`TRUNCATED`/`ERROR`/`WARNING` — load abort threshold                       |
|  [13]   | `Align`                  | grid alignment        | `LOW`/`CENTRE`/`HIGH` — `arrayjoin` halign/valign, `join` placement               |
|  [14]   | `Direction`              | flip axis             | `HORIZONTAL`/`VERTICAL` — `flip` axis                                             |
|  [15]   | `CompassDirection`       | placement gravity     | the `gravity` compass direction                                                   |
|  [16]   | `Angle` / `Angle45`      | rotate step           | `Angle` `D0`/`D90`/`D180`/`D270` (`rot`); `Angle45` adds the 45° steps            |
|  [17]   | `RegionShrink`           | pyramid downsample    | `MEAN`/`MEDIAN`/`MODE`/`MAX`/`MIN`/`NEAREST` — `dzsave` tile-shrink statistic     |
|  [18]   | `Combine`                | mosaic/morph join     | `MAX`/`SUM`/`MIN` — `compass`/morphology band combine                             |
|  [19]   | `Precision`              | conv precision        | `INTEGER`/`FLOAT`/`APPROXIMATE` — `conv`/`sharpen` accumulator                    |
|  [20]   | `TextWrap`               | text wrap             | `text()` wrap `WORD`/`CHAR`/`WORD_CHAR`/`NONE`                                    |
|  [21]   | `SdfShape`               | sdf shape             | `sdf()` shape `CIRCLE`/`BOX`/`ROUNDED_BOX`/`LINE`                                 |
|  [22]   | `CombineMode`            | draw merge            | `SET`/`ADD` — `draw_image`/`mosaic` merge mode                                    |
|  [23]   | `OperationMath` family   | math algebra          | `math`/`math2`/`round`/`complex`/`complex2`/`complexget` selectors                |
|  [24]   | `OperationBoolean`       | boolean algebra       | `AND`/`OR`/`EOR`/`LSHIFT`/`RSHIFT`                                                |
|  [25]   | `OperationRelational`    | relational algebra    | `EQUAL`/`NOTEQ`/`LESS`/`LESSEQ`/`MORE`/`MOREEQ`                                   |
|  [26]   | `OperationMorphology`    | morphology            | `ERODE`/`DILATE` — `morph` structuring-element op                                 |
|  [27]   | `ForeignKeep`            | metadata retention    | `NONE`/`EXIF`/`XMP`/`IPTC`/`ICC`/`OTHER`/`GAINMAP`/`ALL` `keep=` (`IntFlag`-OR'd) |
|  [28]   | `ForeignTiffCompression` | TIFF compression      | `NONE`/`JPEG`/`DEFLATE`/`PACKBITS`/`LZW`/`WEBP`/`ZSTD`                            |
|  [29]   | `ForeignTiffPredictor`   | TIFF predictor        | `NONE`/`HORIZONTAL`/`FLOAT`                                                       |
|  [30]   | `ForeignTiffResunit`     | TIFF resolution-unit  | `CM`/`INCH`                                                                       |
|  [31]   | `ForeignWebpPreset`      | WebP preset           | `DEFAULT`/`PICTURE`/`PHOTO`/`DRAWING`/`ICON`/`TEXT`                               |
|  [32]   | `ForeignSubsample`       | JPEG chroma subsample | `AUTO`/`ON`/`OFF`                                                                 |
|  [33]   | `ForeignHeifCompression` | HEIF/AVIF codec       | `HEVC`/`AVC`/`JPEG`/`AV1`                                                         |
|  [34]   | `ForeignHeifEncoder`     | HEIF encoder          | `AUTO`/`AOM`/`RAV1E`/`SVT`/`X265`                                                 |
|  [35]   | `ForeignPngFilter`       | PNG row filter        | `NONE`/`SUB`/`UP`/`AVG`/`PAETH`/`ALL`                                             |
|  [36]   | `ForeignDzLayout`        | DeepZoom layout       | `DZ`/`ZOOMIFY`/`GOOGLE`/`IIIF`/`IIIF3`                                            |
|  [37]   | `ForeignDzDepth`         | DeepZoom depth        | `ONEPIXEL`/`ONETILE`/`ONE`                                                        |
|  [38]   | `ForeignDzContainer`     | DeepZoom container    | `FS`/`ZIP`/`SZI`                                                                  |
|  [39]   | `ForeignPdfPageBox`      | PDF page box          | `MEDIA`/`CROP`/`TRIM`/`BLEED`/`ART`                                               |
|  [40]   | `ForeignPpmFormat`       | PPM format            | `PBM`/`PGM`/`PPM`/`PFM`/`PNM`                                                     |

- [06]-[BLENDMODE]: `CLEAR`/`SOURCE`/`OVER`/`IN`/`OUT`/`ATOP`/`DEST`/`DEST_OVER`/`DEST_IN`/`DEST_OUT`/`DEST_ATOP`/`XOR`/`ADD`/`SATURATE`/`MULTIPLY`/`SCREEN`/`OVERLAY`/`DARKEN`/`LIGHTEN`/`COLOUR_DODGE`/`COLOUR_BURN`/`HARD_LIGHT`/`SOFT_LIGHT`/`DIFFERENCE`/`EXCLUSION`.
- [07]-[BANDFORMAT]: `NOTSET`/`UCHAR`/`CHAR`/`USHORT`/`SHORT`/`UINT`/`INT`/`FLOAT`/`COMPLEX`/`DOUBLE`/`DPCOMPLEX`.
- [08]-[INTERPRETATION]: `MULTIBAND`/`B_W`/`HISTOGRAM`/`XYZ`/`LAB`/`CMYK`/`LABQ`/`RGB`/`CMC`/`LCH`/`LABS`/`SRGB`/`YXY`/`FOURIER`/`RGB16`/`GREY16`/`MATRIX`/`SCRGB`/`HSV`/`OKLAB`/`OKLCH`.
- [15]-[COMPASSDIRECTION]: `CENTRE`/`NORTH`/`EAST`/`SOUTH`/`WEST`/`NORTH_EAST`/`SOUTH_EAST`/`SOUTH_WEST`/`NORTH_WEST`.
- [23]-[MATH_ALGEBRA]: `OperationMath`/`OperationMath2`/`OperationRound`/`OperationComplex`/`OperationComplex2`/`OperationComplexget` select `math`/`math2`/`round`/`complex`/`complex2`/`complexget` (`SIN`/`COS`/`LOG`/`EXP`/`POW`/`ATAN2`/`CEIL`/`FLOOR`/`RINT`/`POLAR`/`RECT`/`CONJ`/`REAL`/`IMAG`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image construction (one factory family, source-discriminated)

`new_from_file`/`new_from_buffer`/`new_from_source` are the codec-decoding factories (loader chosen from source bytes/extension, per-loader options on `**kwargs` or a trailing `"img.jpg[shrink=2]"` string); `new_from_memory` wraps a raw buffer, `new_from_array`/`new_from_list` lift host arrays, `new_from_image` mints a geometry-matched constant, and `black`/`new_temp_file`/`text`/`sdf` mint blanks and procedural rasters.

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                                                             |
| :-----: | :-------------------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Image.new_from_file`                         | decode from a path; loader inferred; options as kwargs/`[...]`                           |
|  [02]   | `Image.new_from_buffer`                       | decode from in-memory bytes (`options` = trailing option string)                         |
|  [03]   | `Image.new_from_source`                       | decode from a `Source`/`SourceCustom` stream                                             |
|  [04]   | `Image.new_from_memory`                       | wrap an already-decoded raw pixel buffer (no codec)                                      |
|  [05]   | `Image.new_from_array`                        | lift a NumPy/array-interface buffer to an image                                          |
|  [06]   | `Image.new_from_list`                         | build a (convolution-mask) image from a Python list                                      |
|  [07]   | `Image.new_from_image`                        | constant image matching this image's size/format                                         |
|  [08]   | `Image.new_temp_file`                         | mint a temp-backed image for a multi-pass sink                                           |
|  [09]   | `Image.copy_memory`                           | force the lazy pipeline to a RAM buffer (break a re-evaluation dependency before reuse)  |
|  [10]   | `Image.black` / `text` / `sdf` / `gaussnoise` | mint a blank / FreeType-rendered text raster / signed-distance-field shape / noise field |

- [01]-[DECODE]: `new_from_file(vips_filename, *, access, fail_on, **kwargs)`; `new_from_buffer(data, options, *, access, **kwargs)`; `new_from_source(source, options, *, access, **kwargs) -> Image`.
- [02]-[LIFT]: `new_from_memory(data, width, height, bands, format)`; `new_from_array(obj, scale, offset, interpretation)`; `new_from_list(array, scale, offset)`; `new_from_image(value) -> Image`.
- [03]-[MINT]: `new_temp_file(format)`; `copy_memory()`; `black(width, height, bands)`; `text(text, *, font, dpi, width, wrap)`; `sdf(width, height, *, shape) -> Image`.

[ENTRYPOINT_SCOPE]: pipeline egress and stream IO

`write_to_file`/`write_to_buffer` encode to a codec (format from extension/`format_string`), `write_to_target` streams to a `Target`/`TargetCustom`, and `write_to_memory`/`numpy`/`tolist` extract raw pixels; the `*Custom` connection variants register Python read/write/seek callbacks so any file-like or network stream feeds the pipeline without a temp file, and `Region.fetch` pulls a random-access pixel window.

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `Image.write_to_file`                                              | encode to a path (encoder options as kwargs/`[...]`)            |
|  [02]   | `Image.write_to_buffer`                                            | encode to a bytes buffer (`format_string` = `".webp"` + opts)   |
|  [03]   | `Image.write_to_target`                                            | stream the encode to a `Target`/`TargetCustom`                  |
|  [04]   | `Image.write_to_memory`                                            | extract the raw decoded pixel buffer                            |
|  [05]   | `Image.numpy` / `Image.tolist`                                     | egress to a NumPy array / nested Python list                    |
|  [06]   | `Source.new_from_file` / `new_from_memory` / `new_from_descriptor` | mint a streaming input endpoint                                 |
|  [07]   | `SourceCustom.on_read` / `on_seek`                                 | register Python read/seek callbacks for an arbitrary stream     |
|  [08]   | `Target.new_to_file` / `new_to_memory` / `new_to_descriptor`       | mint a streaming output endpoint                                |
|  [09]   | `TargetCustom.on_*`                                                | register Python write/read/seek/end/finish callbacks for a sink |
|  [10]   | `Region.new` / `Region.fetch`                                      | random-access pixel-window fetch over a computed image          |

- [01]-[WRITE]: `write_to_file(vips_filename, **kwargs) -> None`; `write_to_buffer(format_string, **kwargs) -> bytes`; `write_to_target(target, format_string, **kwargs) -> None`; `write_to_memory() -> bytes`; `numpy(dtype) -> ndarray`; `tolist() -> list`.
- [02]-[STREAM]: `Source.new_from_file(filename)`/`new_from_memory(data)`/`new_from_descriptor(fd)`; `SourceCustom.on_read`/`on_seek`; `Target.new_to_file`/`new_to_memory`/`new_to_descriptor`; `TargetCustom.on_write`/`on_read`/`on_seek`/`on_end`/`on_finish`; `Region.new(image)`, `fetch(x, y, w, h) -> bytes`.

[ENTRYPOINT_SCOPE]: generated libvips operations (one `__getattr__` dispatch, not a per-op type)

Most operations are methods generated on `Image` via `Image.__getattr__` → `Operation.call`, the method set introspected from the live libvips build — the names below are canonical libvips nicknames, and `pyvips.base.type_find("VipsOperation", nick)`/`Image.get_suffixes()` are the capability-detection probes before a build-dependent op. A small set are explicit convenience defs (`bandjoin`/`bandsplit`/`composite`/`ifthenelse`/`maxpos`/`get_n_pages`/the band-arithmetic overloads). `Image.get`/`set`/`get_typeof`/`get_fields`/`remove` read/write metadata (EXIF/ICC/orientation/page), and `get_typeof("icc-profile-data") != 0` is the canonical embedded-ICC probe (NOT a `get` that raises on absence).

| [INDEX] | [SURFACE]             | [CAPABILITY]                                                                                            |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Image.thumbnail`     | one-pass shrink-on-load thumbnail (decode+downscale fused; `thumbnail_image` shrinks a loaded pipeline) |
|  [02]   | `Image.resize`        | high-quality arbitrary resample under a `Kernel`                                                        |
|  [03]   | `Image.reduce`        | integer/area block-average downscale / sub-pixel reduce                                                 |
|  [04]   | `Image.smartcrop`     | content-aware crop / rectangular `extract_area` / gravity-placed canvas                                 |
|  [05]   | `Image.embed`         | pad/position onto a larger canvas                                                                       |
|  [06]   | `Image.icc_transform` | ICC-managed device→device colour conversion via liblcms2                                                |
|  [07]   | `Image.colourspace`   | colour-space (`Interpretation`) / numeric-format (`BandFormat`) conversion                              |
|  [08]   | `Image.flatten`       | alpha composite-to-background / premultiply algebra / alpha presence                                    |
|  [09]   | `Image.composite2`    | layer a stack / a pair over a `BlendMode` / `arrayjoin` grid montage                                    |
|  [10]   | `Image.autorot`       | apply/strip EXIF orientation; `Angle`/`Angle45` rotate; `Direction` flip                                |
|  [11]   | `Image.gaussblur`     | separable blur / print-tuned unsharp / convolution / rank / binary morphology                           |
|  [12]   | `Image.bandjoin`      | band stack/split/extract/median; operator overloads; conditional select                                 |
|  [13]   | `Image.linear`        | affine band scale, the math algebra, histogram, scalar/position reductions                              |
|  [14]   | `Image.dzsave`        | DeepZoom / Zoomify / Google / IIIF pyramid tiling for tiled-viewer export                               |
|  [15]   | `Image.get_n_pages`   | animated/multi-page page count/height/split/join; HDR gain-map recovery                                 |
|  [16]   | `Image.get`           | read/write/probe/remove a metadata field; progress; cooperative kill; drop cache                        |
|  [17]   | `Image.tilecache`     | insert a tile/line cache to break a recompute bottleneck; force to RAM                                  |

- [01]-[THUMBNAIL]: `thumbnail_source(source, width, *, height, size, crop, no_rotate, import_profile, export_profile, intent, linear, fail_on) -> Image`; `thumbnail_image(width, *, height, size, crop) -> Image`.
- [02]-[RESAMPLE]: `resize(scale, *, vscale, kernel, gap) -> Image`.
- [03]-[REDUCE]: `shrink(hshrink, vshrink)`; `reduce(hshrink, vshrink, *, kernel) -> Image`.
- [04]-[CROP]: `smartcrop(width, height, *, interesting) -> Image`; `crop(left, top, width, height)` aliases `extract_area`; `gravity(direction, width, height, *, extend, background) -> Image`.
- [05]-[EMBED]: `embed(x, y, width, height, *, extend, background) -> Image`.
- [06]-[ICC]: `icc_transform(output_profile, *, pcs, intent, black_point_compensation, embedded, input_profile, depth) -> Image` — `output_profile`/`input_profile` take a device name (`"srgb"`/`"p3"`/`"cmyk"`) or a profile filename; `black_point_compensation` prevents shadow-crush; `embedded=True` reads the source's own profile.
- [07]-[CONVERT]: `colourspace(space, *, source_space) -> Image`; `cast(format, *, shift) -> Image`.
- [08]-[ALPHA]: `flatten(*, background, max_alpha) -> Image`; `premultiply`/`unpremultiply`/`addalpha`; `hasalpha() -> bool`.
- [09]-[COMPOSITE]: `composite([overlay, ...], [mode, ...], *, x, y) -> Image`; `composite2(overlay, mode, *, x, y) -> Image`; `arrayjoin([img, ...], *, across, shim, halign, valign, background) -> Image`.
- [10]-[ORIENT]: `autorot() -> Image`; `rot(angle)`; `rot45`; `flip(direction)`; `fliphor`/`flipver`.
- [11]-[FILTER]: `gaussblur(sigma, *, min_ampl) -> Image`; `sharpen(*, sigma, x1, y2, y3, m1, m2) -> Image`; `conv(mask, *, precision) -> Image`; `convsep`/`median`/`rank`/`erode`/`dilate`.
- [12]-[BAND]: `bandjoin(other) -> Image`; `image[1]`/`extract_band(1, *, n) -> Image`; per-band `+`/`-`/`*`/`/`/`**`/`%`/`<<`/`>>`/`&`/`|`/`^` + relational `>`/`<` operator overloads; `cond.ifthenelse(a, b) -> Image`.
- [13]-[MATH_STAT]: `linear(a, b) -> Image` (a*x+b); `maxpos() -> (v, x, y)`; `minpos`; `avg() -> float`; `math`/`pow`/`abs`/`sign`/`stats`/`hist_find`/`deviate`.
- [14]-[DZSAVE]: `dzsave(filename, *, layout, tile_size, overlap, depth, container, suffix, region_shrink, keep) -> None`; `dzsave_buffer`.
- [15]-[MULTIPAGE]: `get_n_pages() -> int`; `get_page_height() -> int`; `pagesplit() -> list[Image]`; `pagejoin`; `get_gainmap` (GIF/WebP/TIFF/HEIF/PDF + HDR gain-map).
- [16]-[METADATA]: `get_typeof(name) -> int`; `get("icc-profile-data") -> bytes`; `set`/`get_fields`/`remove`; `set_progress(True)`; `set_kill(True)`; `invalidate()`.
- [17]-[CACHE]: `tilecache(*, tile_width, tile_height, max_tiles, access) -> Image`; `linecache`; `copy`; `copy_memory() -> Image`.

[ENTRYPOINT_SCOPE]: process-wide runtime control (module-level functions, `pyvips.base`)

`pyvips.base` module-level functions tune the one shared libvips runtime the whole process binds — boundary-init calls read once at worker startup, never per-op knobs. `concurrency_set` bounds libvips's own internal intra-op thread pool, which the worker owner sets DOWN so the outer `WORKER_BAND` process fan and the inner libvips pool do not oversubscribe the host; `cache_set_max*` and the untrusted-loader block (`block_untrusted_set`/`operation_block_set`) are the throughput and security controls, and `get_suffixes`/`type_find`/`at_least_libvips`/`version` the build-capability probes.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                                                                        |
| :-----: | :------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `pyvips.concurrency_set` / `concurrency_get` | bound libvips's internal intra-op thread pool (set low under `WORKER_BAND`)         |
|  [02]   | `pyvips.cache_set_max*`                      | bound the operation-result cache (count / memory / open-files) and trace cache hits |
|  [03]   | `pyvips.cache_get_*`                         | read the current cache caps and live size                                           |
|  [04]   | `pyvips.base.block_untrusted_set`            | disable untrusted/named-operation loaders for a hostile-input boundary              |
|  [05]   | `pyvips.base.version` / `get_suffixes`       | native version / minimum probe / build-supported loader+saver suffixes              |
|  [06]   | `pyvips.base.enum_dict`                      | introspect a libvips enum's members (the enum-vocabulary source)                    |
|  [07]   | `pyvips.leak_set` / `pyvips.base.shutdown`   | memory-leak reporting in test; release the libvips runtime at process teardown      |

- [01]-[CONCURRENCY]: `concurrency_set(n) -> None`; `concurrency_get() -> int`.
- [02]-[CACHE]: `cache_set_max(n)`; `cache_set_max_mem(bytes)`; `cache_set_max_files(...)`; `cache_set_trace(...)`; `cache_get_max`; `cache_get_size() -> int`.
- [03]-[BLOCK]: `block_untrusted_set(True)`; `operation_block_set("VipsForeignLoad", True)`.
- [04]-[PROBE]: `version(part) -> int`; `at_least_libvips(x, y) -> bool`; `get_suffixes() -> list[str]`; `type_find`; `nickname_find`; `enum_dict("VipsIntent") -> dict[str, int]`.
- [05]-[LIFECYCLE]: `leak_set(True)`; `base.shutdown()`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `lazy import pyvips` inside the `_worker_raster`/`_icc_apply` worker-arm body only — `cffi.dlopen('libvips.42')` runs at import time, so an unprovisioned native `libvips` raises a dlopen `OSError`/`ImportError` the worker's outer `try` converts to `RasterFault.provision` (host-readiness, distinct from a content fault, the gate `python-magic`/libmagic + `skimage` share); a core-page module import crashes the page load on a libvips-less host.
- load: `new_from_file`/`new_from_buffer`/`new_from_source` is the single decode factory across every libvips loader, the loader introspected from source bytes/extension; the raster arms hold `bytes` so the ingest is `new_from_buffer(payload, "", access=Access.SEQUENTIAL)` (the `""` is the empty option string), a live stream/descriptor taking the `Source`/`SourceCustom` endpoint. `access=Access.SEQUENTIAL` tells libvips the pipeline is one-pass so it streams instead of buffering — the property making the fused decode+downscale+ICC pass O(scanline) not O(image).
- transform: every transform is a generated op method on `Image` (`__getattr__` → `Operation.call`) returning a new lazy `Image`; `autorot`/`thumbnail_image`/`resize`/`smartcrop`/`extract_area`/`gravity`/`icc_transform`/`colourspace`/`flatten`/`composite2`/`arrayjoin` chain into one pipeline computing only at egress. `Kernel`/`Interesting`/`Intent`+`PCS`/`BlendMode`/`Extend`/`Align`/`Size` are keyword-row enums, `BlendMode` passed by `mode.value` nickname. `autorot()` precedes a crop/resize so the box is in display orientation; the `FitMode` (`CONTAIN`/`COVER`/`STRETCH`/`PAD`) maps to `crop=Interesting.NONE`/`crop=Interesting.ATTENTION`/`size=Size.FORCE`/`embed(extend=Extend.BACKGROUND)`, the same geometry the `RasterEngine.PILLOW` arm resolves so one `Thumbnail` op never diverges by engine.
- shrink-on-load: `thumbnail`/`thumbnail_buffer`/`thumbnail_source` fuses decode+downscale (JPEG DCT scaling, libvips `shrink-on-load`) so the full-resolution raster is never materialized; `thumbnail_image(width, height=, size=, crop=)` shrinks an already-opened `Access.SEQUENTIAL` pipeline (`new_from_buffer(...).thumbnail_image(...)`).
- colour: `icc_transform(dst, *, input_profile=src, intent=Intent.RELATIVE, black_point_compensation=, pcs=PCS.LAB, depth=)` runs liblcms2 device→device conversion — the `_icc_apply` managed egress AND the `Convert`-arm sRGB normalization. `Image.get_typeof("icc-profile-data") != 0` is the embedded-profile gate (returns 0 for absent, not a `get` that raises); only a proven profile runs `icc_transform`. `embedded=True` reads the source's own profile; a `new_from_array` numpy source carries none, so the managed arm passes an explicit `input_profile`. pyvips owns device→device managed egress; `pillow` `ImageCms` owns only the soft-proof/gamut-warning transform and the profile-header read.
- egress: `write_to_buffer(format_string, **kwargs)` keys the encoder by `format_string` (`".png"`/`".jpg"`/`".webp"`/`".tif"`, the `_VIPS_SUFFIX`/`_CODEC_OPTS` row); encoder options (`Q`/`compression`/`lossless`/`effort`/`subsample_mode`/`bitdepth`, `keep=ForeignKeep` for EXIF/ICC/XMP) ride `**kwargs` per the `_VIPS_KWARGS[codec](quality, effort)` builder. `write_to_target` streams to a `Target`/`TargetCustom`; `dzsave`/`dzsave_buffer` emits a DeepZoom pyramid; `numpy(dtype=)`/`write_to_memory` extract raw pixels. Egress computes the pipeline exactly once, so the whole fused decode+orient+ICC+flatten+encode runs in one streamed pass.
- random-access: a consumer needing scattered pixel windows takes `Region.new(image)` and `fetch(x, y, w, h)`; a `tilecache`/`linecache` breaks a re-computation bottleneck on a non-sequentially-read `Access.RANDOM` pipeline, and `copy_memory()` forces a reused intermediate to RAM.
- runtime: `concurrency_set(n)` bounds libvips's own internal thread pool DOWN at worker init so it does not oversubscribe the host against the outer `WORKER_BAND` `to_process` fan (the two-pool guard the `graphic/raster/io#IO` page names); `cache_set_max_mem`/`cache_set_max` bound the operation cache; `block_untrusted_set(True)`/`operation_block_set(...)` harden the loader set for hostile uploads; `set_progress`/`set_kill` give a long evaluation a cancellation hook.
- fault: a failed libvips call raises the single typed `pyvips.Error` carrying the error buffer; the `_vips_guarded` capture maps `pyvips.Error → RasterFault.engine` and the dlopen `OSError`/`ImportError → RasterFault.provision`, lifting once at the worker boundary (`@beartype` violation → `.contract`, exhausted worker death → `.worker`).
- evidence: each op captures source kind, loader/encoder name, `Access` mode, output width/height/bands/`Interpretation`, ICC presence (`get_typeof("icc-profile-data") != 0`) + applied intent/BPC/PCS/depth, page count (`get_n_pages`), the chained operation list, native `libvips` version (`pyvips.base.version`), and output byte length as the `msgspec.Struct` `RasterFact`/`ManagedFact` field the consuming owner folds — pyvips mints no receipt of its own.

[STACKING]:
- `numpy` (`libs/python/.api/numpy.md`): the host pixel seam is `Image.numpy(dtype)` out / `new_from_array(arr)` / `new_from_memory(buf, w, h, bands, format)` in — a band-interleaved `numpy` buffer is the one host raster surface, so a `scikit-image`/`colour-science`/`pillow` array enters the libvips pipeline and a libvips result leaves without a bespoke pixel struct; `numpy` over `tolist` for any real raster (the buffer-protocol zero-copy path).
- `anyio` (`libs/python/.api/anyio.md`): egress is native CPU work and pyvips binds an off-loader-path native `libvips`, so every arm crosses `lane.offload(Kernel.of(..., KernelTrait.HOSTILE))` — `_worker_raster` under the trait-row `WORKER` retry, `_icc_apply` under `Some(RetryClass.ENGINE)` — onto the warm loky pool owning the shared `WORKER_BAND` capacity the `exchange/detect`/`metadata`/`graphic/raster/measure`/`process`/`color/managed` lanes share; `concurrency_set` is bound DOWN against the outer fan, and only the finished `write_to_buffer` bytes cross back. Worker-death recovery is the lane's retry on `BrokenWorkerProcess`, never on a `pyvips.Error` content fault — a corrupt image never retries.
- `expression` (`libs/python/.api/expression.md`): the typed `pyvips.Error` maps at the worker boundary into the owner's closed `RasterFault`/`ManagedFault` `@tagged_union` (`pyvips.Error → .engine`, dlopen `OSError → .provision`) and returns through `Result[RasterFact, RasterFault]` (`bind`/`map`, batch `Block`-collected as `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]`) so one corrupt input faults its own slot without aborting the farm; the `except pyvips.Error` lives only in the `_vips_guarded` adapter.
- `structlog`/`opentelemetry` (`libs/python/.api/structlog.md`, `opentelemetry-api.md`): the per-op evidence is the structured event/span payload folded into the `RasterFact`/`ManagedFact` `msgspec.Struct`; the native `libvips` version and `get_suffixes()` feature set read once at boundary init ride the receipt as deployment facts.
- sibling artifacts: `python-magic` (`.api/python-magic.md`) `Detect` MIME branch routes an image payload to the `LIBVIPS`/`PILLOW` arm; `pillow` (`.api/pillow.md`) exchanges as a `numpy` array (`numpy()` out ↔ `Image.fromarray` in) when one engine holds the buffer, never a re-encoded round-trip — pillow owns ICC soft-proof + profile-header read, pyvips the device-egress and profile-byte carrier (`get("icc-profile-data")`); `colour-science` (`.api/colour-science.md`) folds its `GradeStep` CCTF/transfer/primary/CCM/LUT chain inside the same `_icc_apply` worker, handing the toned array to `new_from_array` for the `icc_transform` egress; the SVG raster siblings (`resvg-py`/`vl-convert-python` PNG bytes, `segno`/`python-barcode` codes) route post-raster downscale through `new_from_buffer(png, "", access=Access.SEQUENTIAL).thumbnail_image(width)`; the document owners (`docxtpl` `InlineImage`, `pymupdf`/`reportlab`/`weasyprint`) consume a `write_to_buffer` raster projected onto `core/receipt#RECEIPT` `ArtifactReceipt.Preview`.

[LOCAL_ADMISSION]:
- pyvips owns the libvips fused streaming decode/transform/encode, the device→device managed-ICC egress, smartcrop, DeepZoom, and the large-image pipeline; `pillow` (`.api/pillow.md`) owns the in-process pure-Python working surface (drawing, FreeType text, per-pixel Python LUTs, ICC soft-proof, profile-header read); scientific filtering/morphology/registration routes to `scikit-image` (the `graphic/raster/process#PROCESS`+`measure#MEASURE` acceptors), CMYK/spectral math to `colour-science`, layered PSD/PSB authoring to `PhotoshopAPI`/`psd-tools`; the rendered raster bytes feed the `document/emit#DOCUMENT`/figure/export-bundle owners directly, and live UI stays outside this package.

[RAIL_LAW]:
- Package: `pyvips`
- Owns: streaming demand-driven raster decode/transform/encode over native libvips — shrink-on-load thumbnailing, `Kernel` resampling, `Interesting` smartcrop, `extract_area`/`crop`/`embed`/`gravity`, the `icc_transform` device→device managed-ICC egress under `Intent`/`PCS`/black-point/`depth`, `colourspace`/`cast` conversion, `flatten`/premultiply alpha algebra, `composite2` over a `BlendMode` + `arrayjoin` montage, `autorot`/`rot`/`flip` orientation, the full arithmetic/relational/boolean/complex/morphology/convolution algebra + band arithmetic + `ifthenelse`, multi-page/animated `get_n_pages`/`pagesplit` + HDR `get_gainmap`, DeepZoom `dzsave` pyramid tiling, tile/line caching + `copy_memory`, `Source`/`Target` custom-callback stream IO, `Region` random-access fetch, the process-wide cache/`concurrency_set`/untrusted-loader-block runtime controls, and `numpy`/`new_from_array` zero-copy interchange
- Accept: the `RasterEngine.LIBVIPS` fused `new_from_buffer(payload, "", access=Access.SEQUENTIAL)` → `autorot`/`thumbnail_image`/`extract_area`/`composite2` → `write_to_buffer` pipeline (`Thumbnail`/`Convert`/`Crop`/`Probe`/`Composite` arms on the `WORKER_BAND` `to_process` seam under `_WORKER_RETRY`); the `_icc_apply` managed device-egress (`new_from_array(toned).icc_transform(...).write_to_buffer(...)` on `_ICC_LANE`); the `python-magic` `Detect` MIME image branch; `colour-science`-toned `numpy` arrays via `new_from_array`; PNG bytes from the SVG raster siblings via `new_from_buffer(..., access=Access.SEQUENTIAL).thumbnail_image(width)`; large-image / DeepZoom / streaming raster work feeding the `document/emit#DOCUMENT`/figure/export-bundle owners
- Reject: a wrapper-rename of `new_from_*`/`write_to_*`; a `new_from_file` + separate `resize` two-step where `thumbnail*` fuses decode and downscale; a Pillow downscale round-trip where the source is already a libvips pipeline; a naive `colourspace('srgb')` that discards the source ICC profile where the `get_typeof("icc-profile-data") != 0` → `icc_transform` gate applies; a `get("icc-profile-data")` that raises on an absent profile where `get_typeof` probes 0; a SECOND ICC device-egress engine (pillow `ImageCms.buildTransform`/`profileToProfile`) beside pyvips `icc_transform`; a per-format reader/writer type where one `Image` and the introspected loader/encoder suffice; a raw libvips int where an `Access`/`Interesting`/`Intent`/`PCS`/`Kernel`/`BlendMode`/`Extend`/`Align`/`Size`/`Foreign*` enum row exists; a full `write_to_memory`-then-slice where a `Region`/`tilecache`/`copy_memory` covers random access; an `anyio.to_thread` slot or inline-loop call where the shared `WORKER_BAND` `to_process` seam owns the native-`libvips` crossing; a per-owner `CapacityLimiter(slots)` that oversubscribes the host against libvips's internal pool; a `stamina` retry on a `pyvips.Error` content fault; a bare `except Exception` where `pyvips.Error → RasterFault.engine` and the dlopen `OSError → RasterFault.provision`; a parallel per-domain receipt where `ArtifactReceipt.Preview(key, width, height, scores)` is the one family; the demand-driven scheduler, SIMD resamplers, liblcms2 ICC engine, or codec loaders libvips already owns
