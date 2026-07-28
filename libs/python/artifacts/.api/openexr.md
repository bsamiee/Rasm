# [PY_ARTIFACTS_API_OPENEXR]

`openexr` owns the NAMED-CHANNEL EXR document — the one surface in the estate that authors and reads an OpenEXR file as a header beside a channel dictionary rather than an anonymous component array. It carries the arbitrary-channel namespace (`diffuse.R`, `Z`, `mask.A`), multi-part files, per-channel sub-sampling and `pLinear`, tiled storage, the `envmap` latlong and cube attribute, and every header attribute the format defines — `TimeCode`, `KeyCode`, `PreviewImage`, `Rational`, chromaticities, and opaque pass-through. Pixels cross as `numpy` arrays of `uint32`, `float16`, or `float32`; the module is the ASWF reference implementation bound through pybind11, so its vocabulary IS the file format's.

Flat codec work stays with `imagecodecs`(`.api/imagecodecs.md`), which encodes anonymous single-part planes at every compression row and reads a part by index while DISCARDING names. Ownership splits by NAMES, never by capability overlap: a per-channel egress plane takes `imagecodecs`, and a role-bearing, multi-part, sub-sampled, or environment-map file takes this owner. Container-level tiling exists here; a mip or rip PYRAMID does not survive the write, so a pyramid ships as per-level files.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openexr`
- package: `openexr` (BSD-3-Clause, ASWF)
- module: `OpenEXR`
- asset: sdist built at the Forge floor through `scikit-build-core`; `Imath`, `libdeflate`, and `OpenJPH` are VENDORED in-tree, so no native library row is required
- rail: raster (scene-linear HDR document IO)
- target: `numpy` arrays of `uint32`/`float16`/`float32`, one per channel, with `dict` headers
- capability: the `File`/`Part`/`Channel` document model over arbitrary channel names, multi-part files, scanline and tiled storage, twelve compression methods including `DWAA`/`DWAB`/`HTJ2K32`/`HTJ2K256`, `Envmap` latlong and cube tagging, sub-sampled channels, and the full header-attribute type set

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the document model
- rail: raster

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `File(filename, separate_channels=False, header_only=False)` | document root | read; a context manager over `parts`/`channels()`      |
|  [02]   | `File(header: dict, channels: dict)`                         | document root | author a SINGLE-part file from two dicts               |
|  [03]   | `File(parts: list)`                                          | document root | author a MULTI-part file from `Part` objects           |
|  [04]   | `Part(header: dict, channels: dict, name='')`                | part          | `name()`/`type()`/`width()`/`height()`/`compression()` |
|  [05]   | `Channel(name, pixels, xSampling, ySampling, pLinear=False)` | channel       | eight ctor overloads; `pixels`, `type()`, `name()`     |
|  [06]   | `Header(width, height) -> dict`                              | header seed   | the eight-key default header a write starts from       |
|  [07]   | `TileDescription`                                            | tiling        | `xSize`/`ySize`/`mode`/`roundingMode` set by ATTRIBUTE |
|  [08]   | `PreviewImage` `TimeCode` `KeyCode` `Rational`               | attributes    | thumbnail, SMPTE time and film codes, ratio values     |
|  [09]   | `OpaqueAttribute` `Bytes`                                    | attributes    | unknown attributes survive a read-modify-write         |
|  [10]   | `error`                                                      | fault         | the module exception every malformed read raises       |

[PUBLIC_TYPE_SCOPE]: the closed header vocabularies
- rail: raster

Every enum is exported twice — as a class with members and as bare module constants, so `OpenEXR.ZIP_COMPRESSION` is `Compression.ZIP_COMPRESSION`.

| [INDEX] | [ENUM]              | [ROWS]                                                                                |
| :-----: | :------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Compression`       | `NO` `RLE` `ZIPS` `ZIP` `PIZ` `PXR24` `B44` `B44A` `DWAA` `DWAB` `HTJ2K256` `HTJ2K32` |
|  [02]   | `PixelType`         | `UINT` (32-bit int), `HALF`, `FLOAT` — the per-channel storage type                   |
|  [03]   | `Storage`           | `scanlineimage` `tiledimage` `deepscanline` `deeptile` — the header `type`            |
|  [04]   | `LevelMode`         | `ONE_LEVEL` `MIPMAP_LEVELS` `RIPMAP_LEVELS`                                           |
|  [05]   | `LevelRoundingMode` | `ROUND_DOWN` `ROUND_UP`                                                               |
|  [06]   | `LineOrder`         | `INCREASING_Y` `DECREASING_Y` `RANDOM_Y`                                              |
|  [07]   | `Envmap`            | `ENVMAP_LATLONG` `ENVMAP_CUBE` — the environment-map header tag                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document read and write
- rail: raster
- [SHAPE]: instance (`File` is the one root; `Part` and `Channel` are its members)

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                             |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `File(path, separate_channels=True)`                   | read each channel as its own 2-D array under its own name                |
|  [02]   | `File(path)`                                           | read the fused `RGB` or `RGBA` array of shape `(h, w, 3)` or `(h, w, 4)` |
|  [03]   | `File(path, header_only=True)`                         | header and part metadata alone; pixel data is never touched              |
|  [04]   | `File.write(path) -> None`                             | serialize the whole document; the only egress                            |
|  [05]   | `File.channels() -> dict[str, Channel]`                | the channel view of part 0                                               |
|  [06]   | `File.header() -> dict` and `File.parts -> list[Part]` | header of part 0, and the part list                                      |
|  [07]   | `Part.header -> dict`                                  | the per-part attributes: `tiles`, `type`, `envmap`, `name`               |
|  [08]   | `Channel.pixels -> NDArray`                            | the channel array; sampling and `pLinear` carry beside it                |
|  [09]   | `isOpenExrFile(path) -> bool`                          | magic sniff without opening the document                                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Documents are a HEADER DICT beside a CHANNEL DICT; no builder and no writer object exists. Writing is one `File(header, channels).write(path)` or `File([Part, …]).write(path)`, and every authored attribute is a key in that dict — `compression`, `type`, `tiles`, `envmap`, `chromaticities`, `name`. `Header(w, h)` seeds the eight required keys (`channels`, `compression`, `dataWindow`, `displayWindow`, `lineOrder`, `pixelAspectRatio`, `screenWindowCenter`, `screenWindowWidth`).
- `File(header, channels)` MUTATES the channels dict it is handed — every value is replaced in place by a `Channel` object. Verify passes keep an independent expected-array dict, because reading the caller's own dict after construction reads the binding's objects, never the source arrays.
- `Part.name`, `Part.type`, `Part.width`, `Part.height`, `Part.compression`, `Channel.type`, and `Channel.name` are METHODS, not properties; reading them without the call yields a bound method that formats as text and compares equal to nothing.
- `TileDescription()` takes NO constructor arguments — `xSize`, `ySize`, `mode`, and `roundingMode` are assigned after construction. Positional construction raises `TypeError` naming the zero-argument overload.
- MIP AND RIP PYRAMIDS DO NOT SURVIVE THE WRITE: a part whose `tiles.mode` is `MIPMAP_LEVELS` or `RIPMAP_LEVELS` writes only level 0 and leaves a chunk table the reader rejects — the re-read warns `corrupt chunk table` and reports ZERO parts. `ONE_LEVEL` tiled and scanline parts round-trip whole. Pyramids therefore ship as one file per level under the estate's per-level egress grammar, never as one mip-tiled EXR.
- `header_only=True` reports `width()` and `height()` as `0` on a tiled part; extent reads from `dataWindow` on the header, or from a full read.
- Channel storage type is the array dtype: `float16` writes `HALF`, `float32` writes `FLOAT`, `uint32` writes `UINT`. There is no cast at the boundary, so the plane's declared depth is chosen before the channel dict is built.
- Compression splits lossless from lossy on the row: `NO`/`RLE`/`ZIPS`/`ZIP`/`PIZ`/`HTJ2K32`/`HTJ2K256` round-trip byte-exact, and `DWAA`/`DWAB`/`B44`/`B44A`/`PXR24` do not. Content-keyed planes admit a lossy row only when the key is minted over the ENCODED bytes.
- Channel NAMES are the interchange contract, and a reader sorts them alphabetically. Layered files spell `<layer>.<component>` such as `diffuse.R`, the estate's own channel roster stays canonical, and a name resolved from a guess rather than the roster is the silent role-swap this owner exists to foreclose.

[STACKING]:
- `imagecodecs`(`.api/imagecodecs.md`): the anonymous-plane codec. `exr_encode`/`exr_decode` cover every compression row at half, float, and uint32 for one unnamed component array; this owner covers names, parts, tiles, sub-sampling, and header attributes. `exr_decode(data, index=)` reads a part authored here and drops the names, which is the correct read exactly when the consumer wants components.
- `numpy`(`.api/numpy.md`): every channel is a contiguous 2-D array of `uint32`/`float16`/`float32`, and the dtype IS the `PixelType`. Planes shaped `(h, w, c)` split per component with `np.ascontiguousarray` before it enters the channel dict, because a strided view of the component axis is not a channel.
- `opencolorio`(`.api/opencolorio.md`): EXR carries scene-linear values and NO color space of its own beyond an optional `chromaticities` attribute, so the working space is the OCIO config's `scene_linear` role and the transform runs before the write or after the read, never inside this owner.
- `pyvips`(`.api/pyvips.md`): resampling, mip folding, and every pixel transform stay upstream; this owner serializes the planes it is handed and reads them back, and holds no image operation.
- `msgspec`(`.api/msgspec.md`): the header dict is authored from a typed policy struct — compression row, storage type, envmap tag, channel roster — so a header key is a validated tag rather than a free-form string reaching the attribute dict.
- `beartype`(`.api/beartype.md`): the boundary contract is `dict[str, NDArray] -> None` on write and `str -> dict[str, NDArray]` on read; a wrong-dtype channel rejects at the contract naming the channel, not inside pybind11.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each document write stamps a receipt — part count, channel roster, compression row, storage type, level mode, byte length — on the owning span.
- `anyio`(`.api/anyio.md`): reads and writes are blocking native file IO that releases the GIL, so a multi-part or many-channel document crosses the runtime `RELEASING` thread arm rather than blocking the loop.

[LOCAL_ADMISSION]:
- `import OpenEXR` at boundary scope only, behind the same `lazy import` proxy the other native document owners use.
- Deep-scanline and deep-tile storage (`Storage.deepscanline`, `Storage.deeptile`) are declared by the format and unused by the estate; a deep document admits only with an owner that consumes sample counts.

[RAIL_LAW]:
- Package: `openexr`
- Owns: the named-channel EXR document — arbitrary channel namespaces, multi-part files, `ONE_LEVEL` tiled and scanline storage, all twelve compression rows, per-channel `xSampling`/`ySampling`/`pLinear`, the `envmap` latlong and cube tag, and the full header-attribute type set including opaque pass-through
- Accept: header and channel dicts authored from a typed policy struct; channel arrays contiguous and already at their declared dtype; `separate_channels=True` whenever names carry meaning; `header_only=True` for a metadata probe; lossy compression rows only where the content key is minted over encoded bytes; per-level FILES for a pyramid
- Reject: a mip- or rip-tiled write, which this binding cannot read back; reading `Part.name`/`type`/`width`/`height` or `Channel.type`/`name` as properties; a positionally-constructed `TileDescription`; reuse of a channels dict after `File(header, channels)` mutated it; an anonymous single-part plane authored here where `imagecodecs` is the flat surface; any pixel transform, resample, or mip fold in this owner; a color-space conversion inside the write, which belongs to the OCIO processor; deep storage without a sample-count consumer
