# [PY_ARTIFACTS_API_TIFFFILE]

`tifffile` owns the TIFF/BigTIFF/OME-TIFF container — the IFD directory, strip/tile layout, byte-order and word-size selection, the standard and private tag set, and the `numpy` array<->container round-trip. `export/layered`'s `TIFF` arm composes it beneath the layer author, laying one merged RGBA composite and the two `psdtags`-emitted extratags into a TIFF that Photoshop/Affinity/Krita read as a separable layer stack; it never compresses a channel (`imagecodecs`), authors a PSD layer record (`psdtags`), or authors a native `.psd`/`.psb` (`PhotoshopAPI`/`psd-tools`). Beyond layered egress it is the artifacts TIFF spine — `imwrite`/`imread` for any single- or multi-page TIFF, `memmap`/`aszarr` lazy-tiled access, the GeoTIFF/C2PA tag surface, and `validate_jhove` preflight.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tifffile`
- package: `tifffile` (BSD-3-Clause)
- module: `tifffile`
- rail: layered — the TIFF/BigTIFF/OME-TIFF container IO and extratag authoring the `export/layered` `TIFF` arm composes, and the artifacts TIFF array<->container spine
- abi: pure-Python `py3-none-any`, no native extension; strip codecs delegate to `imagecodecs`
- depends: hard `numpy`; optional `imagecodecs` (strip codecs), `lxml` (OME-XML), `zarr`/`fsspec`/`kerchunk` (chunked-store egress), `matplotlib` (viewer/plot) — imported only when the dependent path runs
- target: a TIFF path or any `os.PathLike`/`FileHandle`/`IO[bytes]` (incl. a `BytesIO` sink); contiguous `numpy` `NDArray` image data in/out
- entry points: console scripts `tifffile` (viewer/info dumper), `tiffcomment` (read/write ImageDescription), `tiff2fsspec` (emit an fsspec/kerchunk chunk reference), `lsm2bin` (Zeiss LSM -> binary); library use imports the module, no plugin entry-point group
- capability: the polymorphic `imwrite`/`imread` array<->TIFF face, the `TiffWriter` incremental multi-page/pyramid/SubIFD writer, the `TiffFile`/`TiffPage`/`TiffPageSeries` reader and tag tree, `memmap`/`aszarr` -> `ZarrTiffStore.write_fsspec` chunked access, `OmeXml` metadata, the closed IFD-field enum vocabularies, the `is_*` detection flags, and `validate_jhove`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the write/read container owners, the page/series/tag graph, the IO/sequence/chunked-store endpoints

Container capability splits a WRITE owner (`TiffWriter`, incremental IFD author) from a READ owner (`TiffFile`/`TiffReader`, directory parser) — both context managers whose native handle the `TIFF` arm brackets in a `with`, both reachable through the one-shot `imwrite`/`imread` faces. Reads walk `TiffFile -> TiffPages -> TiffPage` (one IFD per page, `TiffFrame` the keyframe-sharing lightweight page) and `TiffFile.series -> TiffPageSeries` (the logical N-D image); `TiffTag`/`TiffTags`/`TiffTagRegistry` own the tag value and the standard/private code map, `FileHandle` the unified seekable IO, `TiffSequence`/`FileSequence` the multi-file virtual stack, `ZarrTiffStore` the `aszarr` chunked-store view. Constructors: `TiffWriter(file, /, *, mode, bigtiff, byteorder, append, kind, imagej, ome, shaped)` and `TiffFile(file, /, *, mode, name, offset, size, omexml, **is_flags)`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]        | [CAPABILITY]                                                                 |
| :-----: | :------------------------------ | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `TiffWriter`                    | IFD write owner      | incremental writer (ctx mgr); `write`/`overwrite_description`/`close`        |
|  [02]   | `TiffFile`                      | IFD read owner       | reader (ctx mgr); `pages`/`series`/`asarray`/`aszarr` + metadata props       |
|  [03]   | `TiffReader`                    | read-intent alias    | alias of `TiffFile`                                                          |
|  [04]   | `TiffPage`                      | one-IFD node         | one IFD; `asarray`/`aszarr`, shape/dtype/axes/tags + `is_*` flags            |
|  [05]   | `TiffFrame`                     | lightweight page     | keyframe-sharing page for memory-bounded multi-page reads                    |
|  [06]   | `TiffPageSeries`                | logical N-D image    | image spanning pages/SubIFDs; `asarray(level=)`, `is_pyramidal`              |
|  [07]   | `TiffPages`                     | IFD page list        | the lazy ordered IFD list under a `TiffFile`                                 |
|  [08]   | `TiffTag`                       | one tag value        | one IFD tag; `value`/`code`/`name`/`dtype`; `overwrite(...)` in-file edit    |
|  [09]   | `TiffTags`                      | tag map              | `TiffPage.tags`; `get`/`getall`/`valueof(code)`                              |
|  [10]   | `TiffTagRegistry`               | code<->name table    | standard + private tag-code registry; `add`/`get`/`getall`                   |
|  [11]   | `FileHandle`                    | seekable IO endpoint | unified IO over path/`FileHandle`/`IO[bytes]`; `read`/`write`/`memmap_array` |
|  [12]   | `TiffSequence` / `FileSequence` | multi-file stack     | stack many files into one virtual N-D array by pattern                       |
|  [13]   | `ZarrTiffStore`                 | chunked-store view   | `zarr`-3 `Store` over TIFF (`aszarr` return); `write_fsspec`                 |
|  [14]   | `OmeXml`                        | OME-XML metadata     | `addimage`/`tostring`/`validate` for the OME-TIFF metadata block             |
|  [15]   | `TiffFormat` / `StoredShape`    | format + shape       | byte-order/offset-size descriptor; on-disk shape tuple                       |
|  [16]   | `TiffFileError` / `OmeXmlError` | container faults     | corrupt-TIFF / malformed-OME-XML typed faults                                |

[PUBLIC_TYPE_SCOPE]: the closed IFD-field enum vocabularies (the `imwrite`/`write` keyword constants)

Each enum is an `IntEnum` feeding an `imwrite`/`write` keyword as the typed value — pass the member or its accepted `int`/`str` alias. `PHOTOMETRIC` selects colour interpretation, `EXTRASAMPLE` the alpha kind, `COMPRESSION`+`PREDICTOR` the `imagecodecs` strip codec and its pre-pass, and `PLANARCONFIG`/`RESUNIT`/`SAMPLEFORMAT`/`DATATYPE`/`FILETYPE`/`CHUNKMODE` the geometry/units/sample/tag/chunk vocabulary.

| [INDEX] | [SYMBOL]       | [SELECTS]                                                        |
| :-----: | :------------- | :--------------------------------------------------------------- |
|  [01]   | `PHOTOMETRIC`  | the `photometric=` colour interpretation                         |
|  [02]   | `EXTRASAMPLE`  | the `extrasamples=` extra-channel kind                           |
|  [03]   | `COMPRESSION`  | the `compression=` strip/tile codec (delegated to `imagecodecs`) |
|  [04]   | `PREDICTOR`    | the `predictor=` codec pre-pass                                  |
|  [05]   | `PLANARCONFIG` | the `planarconfig=` sample layout                                |
|  [06]   | `RESUNIT`      | the `resolutionunit=` resolution unit                            |
|  [07]   | `SAMPLEFORMAT` | the sample numeric kind (from the array dtype)                   |
|  [08]   | `DATATYPE`     | the IFD tag value type                                           |
|  [09]   | `FILETYPE`     | the `subfiletype=` NewSubfileType bitfield                       |
|  [10]   | `CHUNKMODE`    | the `aszarr(chunkmode=)` chunk granularity                       |

Members:
- [PHOTOMETRIC]: `MINISWHITE` `MINISBLACK` `RGB` `PALETTE` `MASK` `SEPARATED` `YCBCR` `CIELAB` `ICCLAB` `ITULAB` `CFA` `LOGL` `LOGLUV` `LINEAR_RAW` `DEPTH_MAP` `SEMANTIC_MASK` — `RGB` the layered Photoshop contract, `SEPARATED` the CMYK-separations egress.
- [EXTRASAMPLE]: `UNSPECIFIED` `ASSOCALPHA` `UNASSALPHA` — `('unassalpha',)` marks the RGBA 4th sample straight alpha matching `psdtags.PsdChannelId.TRANSPARENCY_MASK`; `ASSOCALPHA` is premultiplied.
- [COMPRESSION]: `NONE` `LZW` `JPEG` `ADOBE_DEFLATE` `PACKBITS` `DEFLATE` `LZMA` `ZSTD` `WEBP` `JPEG2000` `JPEGXL` `LERC` `PIXARLOG` — an absent `imagecodecs` core raises at write, never mid-strip.
- [PREDICTOR]: `NONE` `HORIZONTAL` `FLOATINGPOINT` `HORIZONTALX2` `HORIZONTALX4` `FLOATINGPOINTX2` `FLOATINGPOINTX4` — the differencing pre-pass via `imagecodecs.delta`/`floatpred`.
- [PLANARCONFIG]: `CONTIG` `SEPARATE` — the merged composite is `CONTIG` (interleaved RGBA).
- [RESUNIT]: `NONE` `INCH` `CENTIMETER` `MILLIMETER` `MICROMETER` — paired with `resolution=(x, y)` for print-DPI-correct output.
- [SAMPLEFORMAT]: `UINT` `INT` `IEEEFP` `VOID` `COMPLEXINT` `COMPLEXIEEEFP` — derived from the array dtype; selects the float predictor family.
- [DATATYPE]: `BYTE` `ASCII` `SHORT` `LONG` `RATIONAL` `SBYTE` `UNDEFINED` `SSHORT` `SLONG` `SRATIONAL` `FLOAT` `DOUBLE` `IFD` `UNICODE` `COMPLEX` `LONG8` `SLONG8` `IFD8` — the 2nd element of an `extratags` `TagTuple` and the `TiffTag.overwrite(dtype=)` value.
- [FILETYPE]: `UNDEFINED` `REDUCEDIMAGE` `PAGE` `MASK` `MACRO` `ENHANCED` `DNG` — `REDUCEDIMAGE` marks each non-base pyramid page.
- [CHUNKMODE]: `STRILE` `PLANE` `PAGE` `FILE` — the `aszarr(chunkmode=)`/`imread(chunkmode=)` chunk grain.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the array<->TIFF round-trip the `TIFF` arm composes, the multi-page/pyramid writer, the lazy-tiled read, and the tag/metadata/validation ops
- `imwrite` / `TiffWriter.write` carry: `photometric`, `planarconfig`, `extrasamples`, `compression`, `predictor`, `tile`, `resolution`, `iccprofile`, `colormap`, `description`, `metadata`, `extratags`, `maxworkers`.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------- | :------- | :---------------------------------------------------------------------- |
|  [01]   | `imwrite(file, /, data, *, byteorder, kind)`    | static   | array->TIFF; `bigtiff`/`kind` discriminate, `extratags=` lays `psdtags` |
|  [02]   | `imread(files, *, key, series, level, aszarr)`  | static   | TIFF->array; `key`/`series`/`level` select, `return_as='xarray'`        |
|  [03]   | `TiffWriter.write(data, *, subifds)`            | instance | append one page/plane (multi-page/pyramid/SubIFD)                       |
|  [04]   | `TiffWriter.overwrite_description(text, /)`     | instance | patch page-0 ImageDescription after writing                             |
|  [05]   | `TiffFile.asarray(key, *, series, level, out)`  | instance | decode page/series/level; `out='memmap'` for over-RAM                   |
|  [06]   | `TiffPage.asarray(*, out, squeeze)`             | instance | decode one IFD page to an `NDArray`                                     |
|  [07]   | `aszarr(*, level, chunkmode)`                   | instance | on `TiffFile`/`TiffPage`; view as a `zarr`-3 store, no decode           |
|  [08]   | `memmap(filename, /, *, shape, dtype, series)`  | static   | file-backed `numpy.memmap` over contiguous image data                   |
|  [09]   | `ZarrTiffStore.write_fsspec(jsonfile, url, /)`  | instance | emit an fsspec/kerchunk JSON chunk reference                            |
|  [10]   | `TiffSequence.asarray(*, ioworkers, axestiled)` | instance | virtual N-D array from many files; `axestiled` mosaics tiles            |
|  [11]   | `TiffTag.overwrite(value, /, *, dtype, erase)`  | instance | rewrite one tag's value in place; returns the `TiffTag`                 |
|  [12]   | `tiffcomment(arg, /, comment, tagcode)`         | static   | read/overwrite the ImageDescription tag on disk                         |
|  [13]   | `validate_jhove(filename, /, *, jhove, ignore)` | static   | run external JHOVE and raise; `ignore=` whitelists codes                |
|  [14]   | `OmeXml.addimage / .tostring / .validate`       | instance | author/validate the OME-TIFF metadata block                             |

- `imwrite`/`TiffWriter.write`: return `(offset, bytecount)` when `returnoffset=True`, else `None`.
- `imread`: returns `NDArray`, a `ZarrTiffStore` (`aszarr=True`), or an axis-labelled `DataArray` (`return_as='xarray'`).

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three disjoint owners meet at typed seams: `tifffile` lays the IFD directory and writes the flat image and the `psdtags`-emitted extratags; `psdtags` authors the `TiffImageSourceData`/`PsdLayers`/`PsdLayer`/`PsdChannel` graph and emits each as a `tifftag(...)` `TagTuple` `(code, dtype, count, value, writeonce)`; `imagecodecs` compresses each strip, selected by `COMPRESSION` and dispatched through `tifffile._imagecodecs`.
- Byte-order, photometric, and metadata are one contract, not three knobs: the layered `imwrite` carries `byteorder=source_data.byteorder` (read off the authored graph, never hardcoded), `photometric='rgb'`, and `metadata=None`; the default shaped-NumPy JSON ImageDescription corrupts the Photoshop read.
- Flat image carries the merged composite, extratags carry the editable layers: one contiguous `numpy` `uint8` `(H, W, 4)` RGBA array and the `37724`/`34377` tags, `extrasamples=('unassalpha',)` declaring straight alpha — never a per-layer page write, since Photoshop reads the `ImageSourceData` tag, not extra pages.
- `imwrite`/`imread` is the polymorphic face and `TiffWriter`/`TiffFile` the incremental owner: `bigtiff=`/`kind=` are keyword discriminants on the one write face and `key`/`series`/`level` discriminate reads, collapsing a `write_tiff`/`write_bigtiff`/`write_ome` or `Get`/`GetPage`/`GetSeries` family into one entry per concern.
- Both handles are `with`-bracketed and finalize on `__exit__`, never GC-reaped; a `BytesIO()` sink yields `sink.getvalue()` once the `with` closes.
- Every `maxworkers` (on `imwrite`/`write` and inside `psdtags.tifftag(...)`) is an `imagecodecs` thread pool bounded inside the one `to_process` crossing.
- Correctness rides two oracles: the `psdtags.TiffImageSourceData.fromtiff(written) == source_data` round-trip and an optional `validate_jhove(path)`; the codecs are lossless, so no precision-loss field is recorded.

[STACKING]:
- `psdtags`(`.api/psdtags.md`): the two halves of layered egress meet at the `TagTuple` and the file path — `psdtags` emits `ImageSourceData`/`ImageResources` as `tifftag(...)` tuples, `tifffile.imwrite(..., extratags=(isd.tifftag(maxworkers=4), res.tifftag()))` lays them into one IFD, and `psdtags.TiffImageSourceData.fromtiff` reads them back through `tifffile.TiffFile`; `byteorder=source_data.byteorder` is the shared contract.
- `imagecodecs`(`.api/imagecodecs.md`): every strip/tile codec is `imagecodecs`' — `tifffile` selects by `COMPRESSION` and dispatches through `tifffile._imagecodecs` (`ADOBE_DEFLATE`/`DEFLATE` -> `deflate_*`, `PACKBITS` -> `packbits_*`, `LZW` -> `lzw_*`, `PREDICTOR` -> `delta_*`/`floatpred_*`); an absent core raises at the write boundary via the `<CODEC>.available` probe, never mid-strip.
- `numpy`(`.api/numpy.md`): the container boundary is `NDArray -> TIFF` (`imwrite`) and `TIFF -> NDArray` (`imread`/`asarray`), dtype and shape derived from `SAMPLEFORMAT`+`bitspersample`; `memmap`/`aszarr` return a `numpy.memmap`/`zarr`-backed view for over-RAM access, never a whole-image load-then-slice.
- `anyio`(`.api/anyio.md`): the `TIFF` arm runs on the `export/layered` `Band.WORKER` `to_process`/`_GATE` lane (reusing the ORA `pyvips` composite path), so the `pyvips` composite, `psdtags` author, `tifffile.imwrite`, and `imagecodecs` strips cross one process seam, never the in-process `Band.CORE` lane the `SVG`/`PDF` arms use.
- `structlog`+`opentelemetry-api`(`.api/structlog.md`): the `TIFF`-arm `LayerFact` records canvas shape, `byteorder`, `PHOTOMETRIC`/`EXTRASAMPLE`/`COMPRESSION`, the strip `maxworkers`, the thumbnail-tag flag, the `returnoffset` `(offset, bytecount)`, and the round-trip result, folded onto the layer-export span as the `core/receipt` `ArtifactReceipt.Preview` case; the `validate_jhove` verdict is the container-integrity signal.
- substrate rails (`msgspec`/`beartype`/`stamina`): the layered-export policy is a typed `msgspec.Struct` whose container options reach `imwrite` as closed-enum members or fixed contract values, never a `compression='deflate'` string; the `beartype`-annotated boundary `Canvas RGBA NDArray + Sequence[TagTuple] -> LayerFact` surfaces a `merged.ndim == 3 and merged.shape[2] == 4 and merged.dtype == uint8` violation as the `export/layered` `ExportFault`, not a raw `TiffFileError` on read-back; the directory write and strip codecs are CPU-pure, so retry belongs only on the artifact-store IO and the `validate_jhove` JRE subprocess, never the container author.
- within-lib boundary: `tifffile` owns the TIFF container for the `TIFF` arm and the artifacts TIFF spine, and the sibling owners stay disjoint — `drawsvg` the named-`<g>` SVG document, `pymupdf`+`pikepdf` the PDF optional-content-group layers, `pyvips`/`lxml`/`stream-zip` the ORA composite/manifest/container, `PhotoshopAPI`/`psd-tools` the native `.psd`/`.psb`; `pyvips` decodes every non-TIFF raster (and the TIFF composite the arm feeds), `Pillow` the in-process edge, and the universal `numcodecs`/`zarr` own array-chunk storage of any non-TIFF array.

[LOCAL_ADMISSION]:
- Reify `tifffile` through the boundary-scope `lazy import` proxy on first `TIFF`-arm use, as with `psdtags`/`pyvips`/`numpy`; the codec/OME-XML/zarr extras load only on the dependent path, and the arm runs on the `Band.WORKER` `to_process` lane, so `tifffile` never loads on the runtime loader path.
- Route by format: a Photoshop-compatible layered TIFF through `psdtags`+`tifffile`, a generic single/multi-page TIFF through `tifffile.imwrite`/`imread`, a native `.psd` through `PhotoshopAPI`/`psd-tools`, an editable PDF through `pymupdf`/`pikepdf`, a GIMP/Krita stack through the `ORA` arm.

[RAIL_LAW]:
- Package: `tifffile`
- Owns: the TIFF/BigTIFF/OME-TIFF container for the `export/layered` `TIFF` arm and the artifacts TIFF spine — the `imwrite`/`imread` face, the `TiffWriter` writer, the `TiffFile`/`TiffPage`/`TiffPageSeries` reader and tag tree (`TiffTag.overwrite`/`TiffTagRegistry`), `memmap`/`aszarr` -> `ZarrTiffStore.write_fsspec` access, `OmeXml` metadata, the IFD-field enum vocabularies, and `validate_jhove`
- Accept: the layered `imwrite(..., photometric='rgb', extrasamples=('unassalpha',), metadata=None, byteorder=isd.byteorder, extratags=(isd.tifftag(maxworkers=4), res.tifftag()))`; `imagecodecs`-backed strip compression selected by `COMPRESSION`+`PREDICTOR`; `with`-bracketed handles; the whole arm on the `_offloaded` `to_process`/`_GATE` lane; `memmap`/`aszarr` over-RAM reads; boundary-scope `lazy import`
- Reject: re-implementing a strip codec (`imagecodecs`), a PSD layer record (`psdtags`), or a canvas composite (`pyvips`); authoring a native `.psd`/`.psb` with `tifffile`+`psdtags` where `PhotoshopAPI`/`psd-tools` own PSD; decoding a non-TIFF raster through `tifffile`; hardcoding `byteorder` at the `imwrite` site; the default shaped-JSON `metadata` on a Photoshop-read TIFF; a per-layer-page TIFF where the editable structure is the `37724`/`34377` extratags; a `write_tiff`/`write_bigtiff`/`write_ome` or `Get`/`GetPage`/`GetSeries` family where keyword discriminants suffice; a raw enum `int` where the member exists; `@retry` around the pure directory write; a second `anyio` worker on the `maxworkers` strip pool; module-level import
