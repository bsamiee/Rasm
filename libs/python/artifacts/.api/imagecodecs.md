# [PY_ARTIFACTS_API_IMAGECODECS]

`imagecodecs` owns the native codec substrate under every raster plane the estate encodes, on two rails one flat surface carries: the DEEP-PIXEL file rail — scene-linear EXR (`none`/`rle`/`zips`/`zip`/`piz`/`pxr24`/`b44`/`b44a`/`dwaa`/`dwab`/`htj2k32`/`htj2k256`), Radiance `rgbe` `.hdr`, 16-bit PNG through three engines, float and half TIFF, JPEG XL at 16-bit and float, 12-bit AVIF, WebP, HTJ2K, JPEG 2000, LERC, UltraHDR, QOI — and the CHANNEL-BYTE rail the `export/layered` PSD/PSB/TIFF egress plane composes beneath its container writers. Beside them ride the `lcms2` ICC transform engine, block-compressed BCn/DDS DECODE, the `meshoptimizer` vertex codecs, and the lossy-float scientific compressors (`zfp`/`sz3`/`sperr`/`pcodec`/`quantize`). Every codec is a `<codec>_encode`/`_decode`/`_check`/`_version` quadruple over contiguous `numpy` buffers; no `Codec` ABC and no instance to construct.

Structure owners stay outside: `psdtags` owns the PSD layer/channel graph, `tifffile` the TIFF directory, `PhotoshopAPI`/`psd-tools` the native PSD/PSB document, `openexr` every named-channel, multi-part, tiled, and environment-map EXR this flat surface cannot address, `pyktx` and the provisioned `ktx` CLI the whole KTX2 container, `pyvips` resampling and the fused decode path. Codec selection runs by name through one capability-discriminated boundary, each backend a `<CODEC>` object whose `.available` routes an absent core to a substitute or a `DelayedImportError`, never mid-write.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `imagecodecs`
- package: `imagecodecs` (BSD-3-Clause)
- module: `imagecodecs`
- asset: `abi3` wheel; every native core statically linked and no system library required — `openexr`, `rgbe`, `libpng`/`libpng_apng`/`libspng`, `libtiff`, `libjxl`, `libavif`, `libwebp`, `lcms2`, `bcdec`, `meshoptimizer`, `openjpeg`, `openjph`, `libultrahdr`, `lerc`, `qoi`, `zfp`, `sz3`, `sperr`, `pcodec`, `libjpeg_turbo`, `jxrlib`, `charls`, `libdeflate`, `zlib`/`zlib_ng`/`zopfli`, `zstd`, `lz4`, `brotli`, `c-blosc2`, and the `imcd` PackBits/LZW/delta/float24 kernels
- rail: compression (deep-pixel file codecs, ICC transforms, block-texture decode, and the layered-egress channel codecs)
- target: `numpy`-shaped contiguous buffers and `bytes`/`bytearray`/`memoryview` byte streams
- capability: 87 `<codec>_encode`/`_decode`/`_check`/`_version` quadruples, each backend a `<CODEC>` object carrying `.available`, a per-codec `<Codec>Error`, and a policy `IntEnum` family; the polymorphic `imread`/`imwrite`/`imagefileext` codec-name dispatch face; `version()` reporting every linked core, `n/a` for the eight this build ships unbuilt (`heif`, `jpegxs`, `mozjpeg`, `brunsli`, `jetraw`, `lzham`, `openzl`, `wic`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec quadruple, the per-codec capability object, and the fault family
- rail: compression

`imagecodecs` exposes a FLAT surface — each logical codec is four module-level functions, one `<CODEC>` capability object, and one `<Codec>Error`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `<codec>_encode(data, /, [level], *, out=None)` | encode arm         | compress or repack a buffer to `bytes` or an encoded `NDArray`   |
|  [02]   | `<codec>_decode(data, /, *, out=None)`          | decode arm         | decompress or unpack a buffer to `bytes` or an `NDArray`         |
|  [03]   | `<codec>_check(data)`                           | sniff arm          | returns `True`, `False`, or `None` where the codec cannot decide |
|  [04]   | `<codec>_version() -> str`                      | core anchor        | linked core version, `"<core> n/a"` when the core is out         |
|  [05]   | `<CODEC>`                                       | capability object  | backend carrying `.available: bool` and its policy enums         |
|  [06]   | `<Codec>Error`                                  | codec fault        | corrupt stream, bad parameter, undersized `out`                  |
|  [07]   | `DelayedImportError(ImportError)`               | absent-codec fault | any call or attribute past `.available` on an unbuilt core       |
|  [08]   | `NONE` / `none_encode` / `none_decode`          | pass-through codec | identity codec (PSD method 0): store raw bytes                   |

[PUBLIC_TYPE_SCOPE]: the policy `IntEnum` families each capability object carries
- rail: compression

Every encoder knob that is not a scalar takes its member, its `int`, or its lowercase name string; the enum is the settled spelling and a free-form string is a validated tag, never a caller literal.

| [INDEX] | [OWNER]    | [ENUM]                                         | [ROWS]                                                                |
| :-----: | :--------- | :--------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `EXR`      | `COMPRESSION`                                  | twelve rows; `zip`/`piz` exact, `htj2k*`/`dwa*`/`b44`/`pxr24` lossy   |
|  [02]   | `PNG`      | `COMPRESSION` `STRATEGY` `FILTER` `COLOR_TYPE` | deflate level, zlib strategy, row filter, gray or RGB with alpha      |
|  [03]   | `APNG`     | the same four as `PNG`                         | animated PNG over the identical libpng policy set                     |
|  [04]   | `SPNG`     | `FMT` `FILTER`                                 | `RGBA8` `RGBA16` `RGB8` `GA8` `GA16` `G8` decode-format request       |
|  [05]   | `TIFF`     | `COMPRESSION` `PREDICTOR` `PHOTOMETRIC` + six  | `LZW`/`DEFLATE`/`ZSTD`/`WEBP`/`LERC`; `FLOATINGPOINT`; `EXTRASAMPLE`  |
|  [06]   | `JPEGXL`   | `COLOR_SPACE` `PRIMARIES` `TRANSFER_FUNCTION`  | `RGB`/`GRAY`/`XYB`; `SRGB`/`BT2100`/`P3`; `LINEAR`/`PQ`/`HLG`/`GAMMA` |
|  [07]   | `AVIF`     | `QUALITY` `SPEED` `PIXEL_FORMAT` + color tags  | `LOSSLESS`; `YUV444`/`YUV420`; `COLOR_PRIMARIES`; `PQ`/`HLG`/`LINEAR` |
|  [08]   | `BCN`      | `FORMAT`                                       | `BC1`-`BC5`, `BC6HU`, `BC6HS`, `BC7` — the decode block vocabulary    |
|  [09]   | `CMS`      | `INTENT` `FLAGS` `PT`                          | four intents; black-point, gamut-check, soft-proof, copy-alpha flags  |
|  [10]   | `HTJ2K`    | `TILEPART`                                     | an `IntFlag` over `RESOLUTIONS` and `COMPONENTS`                      |
|  [11]   | `ULTRAHDR` | `CG` `CT` `CR` `CODEC` `USAGE`                 | `BT_2100`/`DISPLAY_P3`; `LINEAR`/`HLG`/`PQ`; `CODEC` JPEG base alone  |
|  [12]   | `ZLIB`     | `COMPRESSION`                                  | `NO`/`SPEED`/`DEFAULT`/`BEST`, mirrored on `ZLIBNG`                   |
|  [13]   | `QUANTIZE` | `MODE`                                         | `bitgroom` `granularbr` `gbr` `bitround` `scale`                      |
|  [14]   | `FLOAT24`  | `ROUND`                                        | rounding for the 24-bit repack, mirrored on `BFLOAT16`                |

- `AVIF.COLOR_PRIMARIES`: `BT709 BT601 BT470M BT470BG SMPTE240 GENERIC_FILM BT2020 XYZ SMPTE431 SMPTE432 EBU3213`, `SRGB` aliasing `BT709`.
- `ULTRAHDR.CODEC`: lists JPEG, HEIF, and AVIF, yet only JPEG encodes on this build — HEIF and AVIF raise `UltrahdrError: invalid output format`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the file image codecs, deep-pixel and display alike
- rail: compression
- [SHAPE]: static (module-level functions)

`data` is any contiguous `Buffer` on decode and any `ArrayLike` on encode; the array's dtype IS the stored sample format, so depth is chosen by the caller's array and never by a knob. Every arm below also takes `out=None` — `out=<int>` preallocates a bounded output of that many bytes, `out=<bytearray|memoryview|NDArray>` writes into a caller-owned destination — and every encode arm returns `bytes`, every decode arm an `NDArray`.

| [INDEX] | [SURFACE]                                                                        | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `exr_encode(data, /, level=None, *, compression=None, planar=None, frames=None)` | half, float, or uint32 EXR; `level` reads PER-FAMILY |
|  [02]   | `exr_decode(data, /, *, index=0, planar=None)`                                   | `index` picks the part; channel names are dropped    |
|  [03]   | `rgbe_encode(data, /, *, header=None, rle=None)`                                 | Radiance `.hdr` from float32 `(H, W, 3)`             |
|  [04]   | `rgbe_decode(data, /, *, header=None, rle=None)`                                 | inverse to float32; headerless needs shape via `out` |
|  [05]   | `png_encode(data, /, level=None, *, strategy=None, filter=None)`                 | libpng 8-bit and 16-bit PNG, with `png_check`        |
|  [06]   | `spng_encode(data, /, level=None, *, filter=None)`                               | libspng 8-bit and 16-bit PNG, the fast engine        |
|  [07]   | `apng_encode(data, /, level=None, *, photometric=None, delay=None)`              | animated PNG; `apng_decode(data, index)` reads one   |
|  [08]   | `tiff_encode(data, /, level=None, *, compression, predictor, tile, …)`           | libtiff writer at float and half depth, tiled, ICC   |
|  [09]   | `tiff_decode(data, /, index=0, *, asrgb=False)`                                  | `index=None` reads the WHOLE image; see `[TOPOLOGY]` |
|  [10]   | `jpegxl_encode(data, /, level=None, *, effort, lossless, distance, …)`           | JXL at uint8/uint16/float16/float32; L/LA/RGB/RGBA   |
|  [11]   | `jpegxl_decode(data, /, index=None, *, keeporientation, numthreads)`             | `jpegxl_encode_jpeg` re-wraps a JPEG losslessly      |
|  [12]   | `avif_encode(data, /, level=None, *, bitspersample, pixelformat, …)`             | AVIF at 8, 10, or 12-bit; `LOSSLESS` needs `YUV444`  |
|  [13]   | `webp_encode(data, /, level=None, *, lossless=None, method=None)`                | WebP, 8-bit only; decode takes `hasalpha`            |
|  [14]   | `htj2k_encode(data, /, level=None, *, reversible, tile, resolutions, …)`         | HTJ2K over openjph; `reversible=True` is lossless    |
|  [15]   | `jpeg2k_encode` and `jpeg2k_decode`                                              | JPEG 2000 over openjpeg, beside the HTJ2K engine     |
|  [16]   | `lerc_encode(data, /, level=None, *, masks, version, compression, …)`            | bounded-error raster; `level=0.0` is lossless        |
|  [17]   | `lerc_decode(data, /, *, masks=None)`                                            | `masks=True` returns `(values, masks)`               |
|  [18]   | `ultrahdr_encode(data, /, level=None, *, sdr, gamut, transfer, nits, …)`         | gain-map HDR from float16 or uint32; JPEG base alone |
|  [19]   | `qoi_encode(data, /)` and `qoi_decode`                                           | QOI 8-bit lossless RGB/RGBA ONLY; no quality knob    |
|  [20]   | `bmp_encode(data, /, *, ppm=None)`                                               | BMP 8-bit gray/RGB/RGBA; the libvips saver gap       |

- `exr_encode(level=)`: ZIP rows take the 0..9 band and raise `EXR_ERR_INVALID_ARGUMENT` outside it, DWA rows read the same argument as quality.
- `ultrahdr_decode`: returns linear RGBA float16 `(H, W, 4)`, and `ultrahdr_check` sniffs — both live on this build.


[ENTRYPOINT_SCOPE]: block-compressed texture DECODE
- rail: compression
- [SHAPE]: static (module-level functions)

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                           |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `bcn_decode(data, /, format, *, shape=None, out=None)` | bcdec BC1-BC7 and BC6H decode; `format` plus `shape` or `out` required |
|  [02]   | `dds_decode(data, /, *, mipmap=0, out=None)`           | DDS container decode; `mipmap` selects the pyramid level               |
|  [03]   | `bcn_check(data)` and `dds_check(data)`                | `dds_check` reads the `DDS ` magic; `bcn_check` always answers `None`  |
|  [04]   | `bcn_encode` and `dds_encode`                          | declared and NOT implemented — both raise `NotImplementedError`        |

[ENTRYPOINT_SCOPE]: ICC color management over lcms2
- rail: color
- [SHAPE]: static (module-level functions)

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `cms_profile(profile, /, *, whitepoint, primaries, transferfunction, gamma)` | mint a profile from a name or an ICC blob       |
|  [02]   | `cms_profile_validate(profile, /, *, verbose=None) -> None`                  | raises `CmsError` on an invalid ICC blob        |
|  [03]   | `cms_transform(data, /, profile, outprofile, *, intent, flags, outdtype, …)` | transform, retype, and re-planarize in one call |

- `cms_profile`: names the `srgb`/`rgb`/`gray`/`adobergb`/`xyz`/`null` set, yet only `srgb`/`adobergb`/`xyz` build a transform — `rgb`/`gray`/`null` construct a profile that then fails `cmsCreateTransform`, and every built transform is 3-channel.
- `cms_transform`: profiles are ICC BLOBS and a name string raises `CmsError`; `intent` speaks `CMS.INTENT` member names; a 4-band input DROPS its alpha band, so the caller splits alpha before the call and rejoins after.


[ENTRYPOINT_SCOPE]: vertex, array, and numeric-precision codecs
- rail: compression
- [SHAPE]: static (module-level functions)

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `meshopt_encode(data, /, level=None, *, items=None, out=None)`   | vertex-buffer codec; the glTF `EXT_meshopt_compression` payload |
|  [02]   | `meshopt_decode(data, /, shape=None, dtype=None, *, items=None)` | inverse; shape and dtype travel out of band                     |
|  [03]   | `numpy_encode` and `numpy_decode`                                | NPY and NPZ as a codec, on the same dispatch face               |
|  [04]   | `float24_encode(data, /, *, byteorder=None, rounding=None)`      | 24-bit float repack; `bfloat16_encode` mirrors it               |
|  [05]   | `quantize_encode(data, /, mode, nsd, *, out=None)`               | reduce to `nsd` significant digits before a lossless codec      |
|  [06]   | `zfp_encode` `sz3_encode` `sperr_encode` `pcodec_encode`         | error-bounded lossy compressors for solver-grade fields         |


[ENTRYPOINT_SCOPE]: the PSD/PSB/TIFF channel codecs the layered-egress owner composes
- rail: compression
- [SHAPE]: static (module-level functions)

`out=<int>` sizes the decode from the channel's `height * rowbytes` worst case so a decode never reallocates; the predictor codecs return a typed `NDArray` whose dtype and shape match the input but whose values are an encoded byte sequence, not meaningful numbers.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | `packbits_encode(data, /, *, axis=None, out=None) -> bytes`   | PackBits RLE (PSD method 1); `axis` packs each scanline           |
|  [02]   | `packbits_decode(data, /, *, out=None) -> bytes`              | inverse PackBits RLE — unpack a channel or strip to raw bytes     |
|  [03]   | `zlib_encode(data, /, level=None, *, out=None) -> bytes`      | ZIP zlib stream (PSD method 2); `level` takes `ZLIB.COMPRESSION`  |
|  [04]   | `zlib_decode(data, /, *, out=None) -> bytes`                  | inverse ZIP — inflate a zlib-wrapped channel                      |
|  [05]   | `deflate_encode(data, /, level=None, *, raw=False, out=None)` | libdeflate ZIP; `raw=True` is the headerless TIFF variant         |
|  [06]   | `deflate_decode(data, /, *, raw=False, out=None) -> bytes`    | inverse libdeflate deflate; `raw=` must match the encode          |
|  [07]   | `zlibng_encode(data, /, level=None, *, out=None) -> bytes`    | zlib-ng SIMD ZIP with `zlibng_decode` and `ZLIBNG.COMPRESSION`    |
|  [08]   | `delta_encode(data, /, *, axis=-1, dist=1, out=None)`         | horizontal-difference predictor, the method-3 pre-pass            |
|  [09]   | `delta_decode(data, /, *, axis=-1, dist=1, out=None)`         | inverse horizontal-differencing predictor                         |
|  [10]   | `floatpred_encode(data, /, *, axis=-1, dist=1, out=None)`     | TIFF float predictor (predictor 3); deinterleaves floats          |
|  [11]   | `floatpred_decode(data, /, *, axis=-1, dist=1, out=None)`     | inverse floating-point predictor                                  |
|  [12]   | `bitorder_encode(data, /, *, out=None)`                       | reverse bit order per byte with `bitorder_decode` (`FillOrder` 2) |
|  [13]   | `packints_decode(data, dtype, bitspersample, /, *, out=None)` | unpack 1, 2, 4, or 12-bit samples to a `dtype` array              |
|  [14]   | `lzw_decode(data, /, *, out=None) -> bytes`                   | TIFF LZW; `lzw_encode` writes the stream, `lzw_decode` reads it   |
|  [15]   | `none_encode(data, *args, **kwargs)`                          | pass-through identity (PSD method 0) with `none_decode`           |

[ENTRYPOINT_SCOPE]: the generic dispatch face and the build census
- rail: compression
- [SHAPE]: static (module-level functions)

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `imread(fileobj, /, codec=None, *, memmap=False, return_codec=False, **kw)` | `codec` takes a name, a callable, or a fallback list |
|  [02]   | `imwrite(fileobj, data, /, codec=None, **kwargs) -> None`                   | encode by extension or by explicit `codec` name      |
|  [03]   | `imagefileext() -> list[str]`                                               | the 66 extensions `imread` and `imwrite` dispatch on |
|  [04]   | `version(astype=None, /)`                                                   | linked-core census; `astype=dict` keys by core name  |


## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One codec is one quadruple — `<name>_encode`/`_decode`/`_check`/`_version`, never a constructed `Codec` object or per-codec wrapper; the selecting vocabulary (PSD method code, plane format, egress extension) drives a `frozendict[<Enum>, str]` discriminant to a codec NAME, never an `if/elif` chain at the call site.
- Depth is the ARRAY, never a knob: `float32` in yields float EXR/TIFF/JXL samples, `uint16` yields 16-bit PNG/TIFF/JXL/AVIF, and a codec refuses a depth it cannot carry — `png_encode(float32)` raises `ValueError: sample format not supported`, `webp_encode(uint16)` raises on item size. Planes therefore cast to their declared depth BEFORE the codec, so the refusal is a caller-side admission and never a mid-write fault.
- `tiff_decode` DEFAULTS to `index=0`, which reads ONE plane of the sample layout and silently returns a reshaped array — a `(16, 16, 4)` float32 plane decodes as `(16, 4)`, passing every dtype check and raising nothing. Whole-image reads pass `index=None`; `index` also takes a sequence or a `slice` for a multi-page document.
- `jpegxl_encode` reads `photometric=JPEGXL.COLOR_SPACE.RGB` as ABSENT — the RGB member is `0` and the codec rejects it with `ValueError: photometric 0 not supported by codec`. RGB is the default; pass `photometric` for `GRAY` alone, on the one-component width.
- EXR channel NAMES are not addressable through this surface. `exr_encode` writes fixed names by component count (`1 -> Y`, `2 -> Y`+`A`, `3 -> RGB`, `4 -> RGBA`), and `exr_decode` returns a component array in the file's own ALPHABETICAL channel order with the names discarded — a named-AOV file whose channels are `diffuse.R`/`diffuse.G`/`Z` decodes with `Z` in slot 0. Role-bearing multi-channel EXR crosses through `openexr`(`.api/openexr.md`), never here, and a per-channel FILE is the form this surface encodes without ambiguity.
- EXR compression splits lossless from lossy on the row, not on a flag: `none`/`rle`/`zips`/`zip`/`piz` round-trip byte-exact, while `htj2k32`/`htj2k256`/`dwaa`/`dwab`/`b44`/`b44a`/`pxr24` do not — `dwaa` at `level=45` carries roughly 2e-2 absolute error on unit-range float, and the HTJ2K rows INSIDE EXR are measured broken across the extent range a mip ladder spans (all-NaN at 16×16 and 2×512 float32, inexact at ≤8) even though the standalone `htj2k_encode(reversible=True)` codec is lossless. Content-keyed planes admit a lossy row only when the KEY is minted over the encoded bytes, never over the source array.
- Alpha association is the codec's, never the caller's: EXR is associated (premultiplied), PNG/WebP/TIFF/KTX2 are straight, and `rgbe` carries no alpha at all. Encode converts into the format's canonical association and decode normalizes back out; a straight-to-associated conversion at 8-bit quantizes catastrophically at low alpha, so a plane whose declared association differs from its codec's admits at 16-bit or float depth alone.
- Capability-detection at the boundary, not assumption: read `<CODEC>.available` before routing to a codec a build may lack, and read `<codec>_version()` — which returns `"<core> n/a"` rather than raising — when a receipt wants the core string. ANY OTHER attribute on an unavailable backend raises `DelayedImportError`, so `.available` and the version function are the only two probes safe on an absent core; the substitute arm chosen is recorded on the receipt.
- `out=<nbytes>` bounds the decode allocation, so a compression bomb cannot exhaust memory; the lossless codecs (`packbits`/`zlib`/`deflate`/`lzw`/`delta`/`floatpred`/`bitorder`/`packints`/`none`) round-trip byte-identical, the PSD/TIFF channel contract.
- Predictor-then-compressor is the method-3 codec topology, one rail: `delta_encode` (integer channels) or `floatpred_encode` (float channels) raises compressibility, then `deflate_encode(raw=True)` compresses the predicted bytes; decode inverts as `deflate_decode(raw=True)` then `delta_decode`. That same pairing rides `tiff_encode(predictor=TIFF.PREDICTOR.FLOATINGPOINT)` for a float TIFF plane, where libtiff owns the pass internally.
- Buffer in, buffer out: the byte codecs take and return contiguous `bytes`/`bytearray`/`memoryview`, the image and predictor codecs `numpy`-shaped contiguous `NDArray`; a discontiguous view is `.copy()`-d at the boundary before any codec sees it.
- `bcn_decode` demands `format` AND one of `shape`/`out` — the block stream carries neither extent nor format, so both arrive out of band from the container owner. Its declared return type is a byte buffer and its runtime return is a shaped `NDArray` (`uint8`, or `float16` for `BC6HU`/`BC6HS`); the runtime shape is the truth a consumer binds, and the component axis is part of `shape` (`BC7` reads `(h, w, 4)`, `BC5` `(h, w, 2)`, `BC4` `(h, w)`).

[STACKING]:
- `openexr`(`.api/openexr.md`): the two EXR owners split on NAMES — this surface encodes and decodes anonymous component planes at every compression row including `dwaa`/`dwab`/`htj2k`, `openexr` owns named channels, multi-part, tiled, and `ENVMAP_LATLONG`/`ENVMAP_CUBE` headers. Per-channel egress files take this surface; a role-bearing or environment-map EXR takes `openexr`; `exr_decode(data, index=)` still reads a part `openexr` wrote, dropping the names.
- `pyktx`(`.api/pyktx.md`): KTX2 is a CONTAINER this surface never writes — `bcn_encode`/`dds_encode` raise `NotImplementedError` and no KTX2 member exists. `bcn_decode`/`dds_decode` are the READ-BACK leg over a transcoded payload `pyktx` or the provisioned `ktx` CLI produced, so a verify pass proves block bytes without a second encoder.
- `pyvips`(`.api/pyvips.md`) / `pillow`(`.api/pillow.md`): resampling, thumbnailing, and the fused decode path stay with libvips, the in-process working surface with pillow; this surface owns the codecs neither linked build carries. Two legs, one law — at DEPTH it owns EXR, `rgbe`, JXL at float, 12-bit AVIF, and LERC, and planes resample through `pyvips` in the float lane then encode here, never the inverse; at DISPLAY depth it is the array-writer column on a `graphic/raster/io#IO` `CODEC` row, taking the 8-bit `Frame` whichever working surface produced it (`np.asarray(image)` from pillow, `Image.cast(BandFormat.UCHAR).numpy()` from libvips) and serving the containers those builds refuse — saver capability is a PER-BUILD probe, never a remembered roster: the current API-mode libvips registers `.bmp` and `.jxl` savers yet `jxlsave` refuses 2- and 4-band images mid-write, and `.qoi` has no saver at any build, so the writer column degrades to `jpegxl_encode(frame, level=quality, effort=effort)`, `bmp_encode(frame)`, or `qoi_encode(frame)` when the trial-probe refuses. Every array leg admits exactly the bands its container carries and answers a violation as a codec `ValueError`, not a promotion — `qoi_encode` refuses a one-band plane with `photometric 1 not supported` and `bmp_encode` takes gray, so the `CODEC` row resolves the mode before the call and the array writer holds no admission literal of its own.
- `psdtags`(`.api/psdtags.md`) / `tifffile`(`.api/tifffile.md`) / `photoshopapi`(`.api/photoshopapi.md`) / `psd-tools`(`.api/psd-tools.md`): the container owner builds the layer/channel structure, then the `_psd`/`_tiff` arm runs each `PsdChannel.data` 2-D array through `packbits_encode(channel, axis=0)` (method 1) or `delta_encode(channel, axis=-1)` + `deflate_encode(raw=True)` (method 3) before serialization, inverting on read; `tifffile` already calls `imagecodecs` internally for its strip and tile codecs.
- `numpy`(`.api/numpy.md`): the codec boundary is `NDArray -> bytes` on encode and `bytes -> NDArray` on decode over contiguous planes; the array dtype IS the sample format, `out=` is sized from `plane.nbytes`, and `quantize_encode`/`float24_encode` are the precision-reduction pre-passes a lossless codec then compresses.
- `colour-science`(`.api/colour-science.md`) / `opencolorio`(`.api/opencolorio.md`): color-science transforms and OCIO config-driven processors own the SPACE, `cms_transform` owns the ICC-profile boundary. Embedded ICC profiles decode through `cms_profile_validate` + `cms_transform` at ingest, and a scene-linear working space is reached through the OCIO processor, never by synthesizing an ICC profile with `gamma=1.0` where a config row already names the transform.
- `msgspec`(`.api/msgspec.md`): every codec selection is a typed member on a policy struct — a `PsdCompression` `IntEnum` (`RAW=0`/`RLE=1`/`ZIP=2`/`ZIP_PREDICTION=3`) or a plane-format tag — and a `frozendict` maps each member to the codec name, so `msgspec` decodes the policy and the name is a validated tag, never a free-form string reaching `imread(codec=...)`.
- `beartype`(`.api/beartype.md`): annotate the rail `bytes | numpy.ndarray -> bytes | numpy.ndarray` so a non-buffer input rejects at the contract, not deep in the Cython extension; `<CODEC>.available` is the boundary predicate the contract reads, failing an unbuildable route with the codec name rather than a `DelayedImportError` stack.
- `stamina`(`libs/python/runtime/.api/stamina.md`): a codec `encode`/`decode` is CPU-pure and deterministic — never `@retry`; a `<Codec>Error` or `DelayedImportError` is a terminal fault, and retry belongs only on the IO around the produced container bytes owned by `core/plan`.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each codec call stamps a receipt — codec name, compression row, predictor, `level`, input and output byte lengths, achieved ratio, `<codec>_version()` core version — on the owning span, and the `version()` build census rides the startup span once.
- `anyio`(`.api/anyio.md`): the codec passes are CPU-bound native calls that release the GIL, so a many-channel or many-plane batch crosses the runtime `RELEASING` thread arm; a whole-document PSD/PSB author rides its channel-codec passes alongside the container author inside the one `HOSTILE` process crossing the libvips composite already runs.

[LOCAL_ADMISSION]:
- `import imagecodecs` at boundary scope only; the `export/layered` `lazy import` proxy reifies the native core on first channel-codec use, as it does `psdtags`/`tifffile`.
- Live UI and re-rasterization stay outside this package.

[RAIL_LAW]:
- Package: `imagecodecs`
- Owns: the native codec substrate for every raster plane the estate encodes — the deep-pixel file codecs (`exr` across all twelve compression rows, `rgbe`, `png`/`spng`/`apng` at 16-bit, float and half `tiff`, `jpegxl`, `avif` to 12-bit, `webp`, `htj2k`, `jpeg2k`, `lerc`, `ultrahdr`, `qoi`), the `lcms2` ICC profile and transform surface, block-compressed `bcn`/`dds` DECODE, the `meshopt` vertex codecs, the error-bounded float compressors, the `none`/`packbits`/`zlib`/`deflate`/`zlibng`/`lzw` channel compressors with the `delta`/`floatpred`/`bitorder`/`packints` predictors and sample-repackers, the `<CODEC>.available` build probes, and the polymorphic `imread`/`imwrite`/`imagefileext` dispatch face
- Accept: codec selection by name through a typed-enum `frozendict` discriminant; depth carried by the caller's array dtype with the cast landing before the codec; the predictor-then-`deflate(raw=True)` chain for method 3; `<CODEC>.available` and `<codec>_version()` as the only probes safe on an absent core; `out=<nbytes>` bounded preallocation sized from `plane.nbytes`; per-plane receipts carrying codec, compression row, predictor, level, ratio, and core version; planes passed as contiguous `numpy` arrays; the `graphic/raster/io#IO` `CODEC` array-writer column at DISPLAY depth, where an 8-bit `Frame` is the container's own referent rather than a quantized intermediate
- Reject: a constructed `Codec` instance where the quadruple suffices; an `if/elif` codec chain where the enum `frozendict` dispatches; a `<codec>_*` call whose `<CODEC>.available` was not asserted in a non-fixed build; any attribute past `.available` on an unavailable backend; a role-bearing, multi-part, or tiled EXR authored here rather than at `openexr`; a KTX2 container or any block ENCODE claimed here, where `bcn_encode`/`dds_encode` raise `NotImplementedError`; a lossy EXR/AVIF/JXL row on a plane whose content key is minted over the SOURCE array; an 8-bit intermediate on a texture or measurement path, where quantization is silent; re-authoring a PSD layer record, TIFF directory, or container header (`psdtags`/`tifffile`/`PhotoshopAPI`/`psd-tools` own those); `@retry` around a pure `encode`/`decode`; a `tiff_decode` at its `index=0` default where the whole image is wanted; a discontiguous plane reaching a codec without `.copy()`; a duplicate `deflate`/`PackBits` owner where the general compression band (`package/bundle#BUNDLE` `CompressionAlgo`) and the universal array-chunk store own their payloads; routing an OpenRaster `ORA` PNG member here, where the container-ZIP path is `stream-zip`, not channel compression
