# [PY_ARTIFACTS_API_IMAGECODECS]

`imagecodecs` owns the native per-channel byte-codec substrate the `export/layered` PSD/PSB/TIFF egress plane composes beneath its container writers — one flat family of `<codec>_encode`/`_decode`/`_check`/`_version` quadruples over `numpy`-shaped contiguous buffers. It authors exactly the per-channel byte transforms the Photoshop and TIFF formats define, while `psdtags` owns the PSD layer/channel graph, `tifffile` the TIFF directory, and `PhotoshopAPI`/`psd-tools` the native PSD/PSB document: the container owners author structure, `imagecodecs` the compressed channel bytes. A codec selects by name through one capability-discriminated boundary, each backend a `<CODEC>` object whose `.available` routes an absent core to a substitute or a `DelayedImportError`, never mid-write.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `imagecodecs`
- package: `imagecodecs` (BSD-3-Clause)
- module: `imagecodecs`
- asset: `abi3` wheel; vendored native cores (libdeflate, zlib-ng, the `imcd` PackBits/LZW/delta kernels, libtiff predictors, libjpeg-turbo, OpenJPEG, libjxl, LERC, c-blosc2) statically linked, no system libs required
- rail: compression (raster-channel codec substrate for the `export/layered` PSD/PSB/TIFF egress plane)
- target: `numpy`-shaped contiguous buffers and `bytes`/`bytearray`/`memoryview` byte streams
- capability: ~87 `<codec>_encode`/`_decode`/`_check`/`_version` quadruples, each backend a `<CODEC>` object carrying `.available`, a per-codec `<Codec>Error`, and (deflate family) a `.COMPRESSION` IntEnum; the polymorphic `imread`/`imwrite`/`imagefileext` codec-name dispatch face; `version()` reporting every linked native core

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the per-codec backend capability objects, the codec quadruple, and the fault family
- rail: compression

`imagecodecs` exposes a FLAT surface — no `Codec` ABC and no instance to construct; each logical codec is four module-level functions, one `<CODEC>` capability object, and one `<Codec>Error`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]      | [CAPABILITY]                                                |
| :-----: | :---------------------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `<codec>_encode(data, /, [level], *, out=None)` | encode arm         | compress/repack a buffer to `bytes` or an encoded `NDArray` |
|  [02]   | `<codec>_decode(data, /, *, out=None)`          | decode arm         | decompress/unpack a buffer to `bytes` or an `NDArray`       |
|  [03]   | `<codec>_check(data) -> bool \| None`           | sniff arm          | codec sniff; `None` = cannot decide                         |
|  [04]   | `<codec>_version() -> str`                      | core anchor        | linked native core version                                  |
|  [05]   | `<CODEC>`                                       | capability object  | backend module exposing the `.available: bool` build probe  |
|  [06]   | `ZLIB.COMPRESSION` / `ZLIBNG.COMPRESSION`       | level enum         | `IntEnum` deflate `NO`/`SPEED`/`DEFAULT`/`BEST` or `int`    |
|  [07]   | `<Codec>Error`                                  | codec fault        | corrupt stream, bad param, undersized `out`                 |
|  [08]   | `DelayedImportError(ImportError)`               | absent-codec fault | `<codec>_*` called without a compiled core                  |
|  [09]   | `NONE` / `none_encode` / `none_decode`          | pass-through codec | identity codec (PSD method 0): store raw bytes              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the PSD/PSB/TIFF channel codecs the layered-egress owner composes, and the generic dispatch face
- rail: compression
- [SHAPE]: static (module-level functions)

`out=<int>` preallocates a bounded output of that many bytes (size it from the channel's `height * rowbytes` worst case so a decode never reallocates); `out=<bytearray|memoryview|NDArray>` writes into a caller-owned destination. `data` is any contiguous `Buffer` (`bytes`, a `numpy` array, a `memoryview`); the predictor codecs return a typed `NDArray` whose dtype and shape match the input but whose values are an encoded byte sequence, not meaningful numbers.

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `packbits_encode(data, /, *, axis=None, out=None) -> bytes`          | PackBits RLE (PSD method 1); `axis` packs each scanline          |
|  [02]   | `packbits_decode(data, /, *, out=None) -> bytes`                     | inverse PackBits RLE — unpack a channel/strip to raw bytes       |
|  [03]   | `zlib_encode(data, /, level=None, *, out=None) -> bytes`             | ZIP zlib stream (PSD method 2); `level` `ZLIB.COMPRESSION`/`int` |
|  [04]   | `zlib_decode(data, /, *, out=None) -> bytes`                         | inverse ZIP — inflate a zlib-wrapped channel                     |
|  [05]   | `deflate_encode(data, /, level=None, *, raw=False, out=None)`        | libdeflate ZIP; `raw=True` headerless raw-deflate (TIFF variant) |
|  [06]   | `deflate_decode(data, /, *, raw=False, out=None) -> bytes`           | inverse libdeflate deflate; `raw=` must match the encode         |
|  [07]   | `zlibng_encode(data, /, level=None, *, out=None) -> bytes`           | zlib-ng SIMD ZIP + `zlibng_decode`; `level` `ZLIBNG.COMPRESSION` |
|  [08]   | `delta_encode(data, /, *, axis=-1, dist=1, out=None)`                | horizontal-diff predictor (method-3 pre-pass); `axis`/`dist`     |
|  [09]   | `delta_decode(data, /, *, axis=-1, dist=1, out=None)`                | inverse horizontal-differencing predictor                        |
|  [10]   | `floatpred_encode(data, /, *, axis=-1, dist=1, out=None)`            | TIFF float predictor (predictor 3); deinterleaves floats         |
|  [11]   | `floatpred_decode(data, /, *, axis=-1, dist=1, out=None)`            | inverse floating-point predictor                                 |
|  [12]   | `bitorder_encode(data, /, *, out=None) -> bytes\|NDArray`            | reverse bit order per byte + `bitorder_decode` (`FillOrder` 2)   |
|  [13]   | `packints_decode(data, dtype, bitspersample, /, *, out=None)`        | unpack 1/2/4/12-bit samples to a `dtype` array; `bitorder`       |
|  [14]   | `lzw_decode(data, /, *, out=None) -> bytes`                          | TIFF LZW; `lzw_encode` writes the stream, `lzw_decode` reads it  |
|  [15]   | `none_encode(data, *args, **kwargs)`                                 | pass-through identity (PSD method 0) + `none_decode`             |
|  [16]   | `tiff_decode(data, /, index=0, *, asrgb=False, out=None) -> NDArray` | whole-TIFF decode via libtiff (not `tifffile`'s directory)       |
|  [17]   | `imread(fileobj, /, codec=None, *, memmap=True) -> NDArray`          | polymorphic decode face; sniff from buffer/path/ext or `codec`   |
|  [18]   | `imwrite(fileobj, data, /, codec=None, **kwargs) -> None`            | polymorphic encode face; codec by ext or explicit `codec` name   |
|  [19]   | `imagefileext() -> list[str]`                                        | every file extension `imread`/`imwrite` dispatch on              |
|  [20]   | `version(astype=None, /) -> str`                                     | linked-core census for the build receipt; `astype=dict`/`tuple`  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One codec is one quadruple — `<name>_encode`/`_decode`/`_check`/`_version`, never a constructed `Codec` object or per-codec wrapper; PSD method code selects the codec name directly (`0 -> none`, `1 -> packbits` with per-row `axis=`, `2 -> zlib`/`deflate`, `3 -> delta`+`deflate(raw=True)`), driven as a `frozendict[PsdCompression, codec]` discriminant, never an `if/elif` chain at the channel-write site.
- Predictor-then-compressor is the method-3 codec topology, one rail: `delta_encode` (integer channels) or `floatpred_encode` (float channels) raises compressibility, then `deflate_encode(raw=True)` compresses the predicted bytes; decode inverts as `deflate_decode(raw=True)` then `delta_decode`.
- Capability-detection at the boundary, not assumption: assert `<CODEC>.available` before routing a channel to a codec a build may lack — a `False` flag or the `DelayedImportError` an absent-core call raises routes to the substitute arm (`none` raw store, or another admitted codec), the choice recorded on the channel receipt.
- `out=<nbytes>` bounds the decode allocation (a PSD channel is `height * rowbytes`), so a compression bomb cannot exhaust memory; the lossless codecs (`packbits`/`zlib`/`deflate`/`lzw`/`delta`/`floatpred`/`bitorder`/`packints`/`none`) round-trip byte-identical, the PSD/TIFF channel contract.
- Buffer in, buffer out: the byte codecs take and return contiguous `bytes`/`bytearray`/`memoryview`, the predictor and image codecs `numpy`-shaped contiguous `NDArray`; a discontiguous view is `.copy()`-d at the boundary before any codec sees it.

[STACKING]:
- `psdtags`(`.api/psdtags.md`) / `tifffile`(`.api/tifffile.md`) / `photoshopapi`(`.api/photoshopapi.md`) / `psd-tools`(`.api/psd-tools.md`): the container owner builds the layer/channel structure, then the `_psd`/`_tiff` arm runs each `PsdChannel.data` 2-D array through `packbits_encode(channel, axis=0)` (method 1) or `delta_encode(channel, axis=-1)` + `deflate_encode(raw=True)` (method 3) before serialization, inverting on read; `tifffile` already calls `imagecodecs` internally for its strip/tile codecs.
- `numpy`(`.api/numpy.md`): the codec boundary is `NDArray[uint8] -> bytes` (channel encode) and `bytes -> NDArray[uint8]` (decode) over a contiguous 2-D channel sliced from the placed RGBA composite; the predictors preserve dtype and shape while reinterpreting values as encoded bytes, and `out=` is sized from `channel.nbytes`.
- `msgspec`(`.api/msgspec.md`): the channel-codec choice is a typed `PsdCompression` `IntEnum` (`RAW=0`/`RLE=1`/`ZIP=2`/`ZIP_PREDICTION=3`) on the layer-policy struct, and a `frozendict[PsdCompression, str]` maps each member to the codec name — `msgspec` decodes the policy and the name is a validated tag, never a free-form string reaching `imread(codec=...)`.
- `beartype`(`.api/beartype.md`): annotate the rail `bytes | numpy.ndarray -> bytes | numpy.ndarray` so a non-buffer input rejects at the contract, not deep in the Cython extension; `<CODEC>.available` is the boundary predicate the contract reads, failing an unbuildable route with the codec name rather than a `DelayedImportError` stack.
- `stamina`(`.api/stamina.md`): a codec `encode`/`decode` is CPU-pure and deterministic — never `@retry`; a `<Codec>Error` or `DelayedImportError` is a terminal fault, and retry belongs only on the IO around the produced container bytes owned by `core/plan`.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each channel-codec call stamps a receipt — codec name, PSD method code, predictor, `level`, input/output byte lengths, achieved ratio, `<codec>_version()` core version — on the layer-export span, and the `version()` build census rides the startup span once.
- `anyio`(`.api/anyio.md`): the channel-codec passes are CPU-bound native calls that release the GIL, so a many-channel batch crosses the runtime `RELEASING` thread arm; a whole-document PSD/PSB author rides its channel-codec passes alongside the container author inside the one `HOSTILE` process crossing the libvips composite already runs.

[LOCAL_ADMISSION]:
- `import imagecodecs` at boundary scope only; the `export/layered` `lazy import` proxy reifies the native core on first channel-codec use, as it does `psdtags`/`tifffile`.
- Live UI and re-rasterization stay outside this package.

[RAIL_LAW]:
- Package: `imagecodecs`
- Owns: the native per-channel byte-codec substrate for the `export/layered` PSD/PSB/TIFF egress plane — the `none`/`packbits`/`zlib`/`deflate`/`zlibng`/`lzw` channel compressors, the `delta`/`floatpred`/`bitorder`/`packints` TIFF/PSD predictors and sample-repackers, the `<CODEC>.available` build probes, and the polymorphic `imread`/`imwrite`/`imagefileext` codec-name dispatch over the full image-format set
- Accept: codec selection by name through a `frozendict[PsdCompression, codec]` method-code discriminant; the predictor-then-`deflate(raw=True)` chain for method 3; `<CODEC>.available` capability-detection at the boundary; `out=<nbytes>` bounded preallocation sized from `channel.nbytes`; per-channel receipts carrying method/predictor/level/ratio/core-version; channels passed as contiguous `numpy` arrays
- Reject: a constructed `Codec` instance where the quadruple suffices; an `if/elif` channel-codec chain where the method-code `frozendict` dispatches; an `<codec>_*` call whose `<CODEC>.available` was not asserted in a non-fixed build; re-authoring a PSD layer record, TIFF directory, or container header (`psdtags`/`tifffile`/`PhotoshopAPI`/`psd-tools` own those); `@retry` around a pure `encode`/`decode`; a discontiguous channel reaching a codec without `.copy()`; a duplicate `deflate`/`PackBits` owner where the general compression band (`package/bundle#BUNDLE` `CompressionAlgo`) and the universal array-chunk store own their payloads; routing an OpenRaster `ORA` PNG member here, where the container-ZIP path is `stream-zip`, not channel compression
