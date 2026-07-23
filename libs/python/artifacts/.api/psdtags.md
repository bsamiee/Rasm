# [PY_ARTIFACTS_API_PSDTAGS]

`psdtags` owns the Photoshop-compatible layered-TIFF tag structure — the `ImageSourceData` (tag `37724`) and `ImageResources` (tag `34377`) object graph a flat RGB TIFF carries so Photoshop/Affinity/Krita read it as a separable, re-orderable layer stack. It authors the graph and emits each tag as a `tifftag(...)` extratag `tifffile` lays into the IFD, never compressing a channel byte (`imagecodecs`) nor writing the TIFF directory (`tifffile`). It is `export/layered`'s `TIFF`-arm structure author for the layered-TIFF container only — native `.psd`/`.psb` authority is `PhotoshopAPI`/`psd-tools`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psdtags`
- package: `psdtags` (BSD-3-Clause)
- module: `psdtags`
- rail: layered — the `ImageSourceData`/`ImageResources` tag-structure authority for the `export/layered` `TIFF` arm
- abi: pure-Python `py3-none-any`, no native extension, no cp gate
- depends: optional `imagecodecs` (channel codecs), `tifffile` (container IO), `matplotlib` (the `python -m psdtags <file.tif>` layer/mask/resource viewer) — imported only when the dependent path runs
- target: a layered TIFF path or `BinaryIO`/`bytes` buffer at the boundary; contiguous 2-D `numpy` channel arrays and the typed `Psd*` graph in memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the layered-TIFF tag owners, the layer/channel/mask graph, and the tagged-block/resource-block families

Both tag owners `TiffImageSourceData`/`TiffImageResources` are the only constructed entrypoints; every other node the author builds inside them. Each `@dataclass` carries a symmetric quartet — `fromtiff`/`frombytes`/`read` parse, `tifftag`/`tobytes`/`write` serialize — so the type round-trips a layered TIFF byte-identically.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [CAPABILITY]                                                               |
| :-----: | :-------------------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `TiffImageSourceData` | tag-37724 owner  | `psdformat`/`layers`/`usermask`/`info`; props `byteorder`/`has_unknowns`   |
|  [02]   | `TiffImageResources`  | tag-34377 owner  | `psdformat`/`blocks`; `thumbnail()` decodes the embedded JPEG              |
|  [03]   | `PsdLayers`           | layer-stack node | `key`/`layers`/`has_transparency`; props `dtype`/`shape`                   |
|  [04]   | `PsdLayer`            | layer node       | editable named layer over channels, bbox, mask, blend/opacity/clip/flags   |
|  [05]   | `PsdChannel`          | channel node     | `channelid`/`compression`/`data`; `tobytes`/`read_image` via `imagecodecs` |
|  [06]   | `PsdLayerMask`        | per-layer mask   | raster+vector density/feather, combined `real_*` trio, prop `param_flags`  |
|  [07]   | `PsdUserMask`         | document mask    | `colorspace`/`components`/`opacity`/`flag`; a `PsdKeyABC` block subtype    |
|  [08]   | `PsdFilterMask`       | document mask    | `colorspace`/`components`/`opacity`; a `PsdKeyABC` block subtype           |

- `PsdLayer`: fields `name`/`channels`/`rectangle`/`mask=None`/`opacity=255`/`blendmode`/`blending_ranges`/`clipping`/`flags`/`info`; props `offset`/`shape`/`title`.
- `PsdKeyABC`: base of `PsdSectionDividerSetting`/`PsdMetadataSettings`/`PsdReferencePoint`/`PsdSheetColorSetting`/`PsdTextEngineData`/`PsdString`/`PsdInteger`/`PsdWord`/`PsdBoolean`/`PsdUnknown`, each implementing its own `key`/`read`/`write`; `PsdSectionDividerSetting(kind, blendmode=None, subtype=None)` is the group-folder block the `TIFF` arm mints on `PsdLayer.info`.
- `PsdResourceBlockABC`: base of `PsdThumbnailBlock`/`PsdVersionBlock`/`PsdColorBlock`/`PsdStringBlock`/`PsdStringsBlock`/`PsdPascalStringBlock`/`PsdPascalStringsBlock`/`PsdBytesBlock`.

[PUBLIC_TYPE_SCOPE]: the closed enum + flag vocabularies and the geometry value tuples

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                                                        |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `PsdKey`                    | block-tag vocab   | `BytesEnum` block-key sigs — `LAYER`/`SECTION_DIVIDER_SETTING`/`METADATA_SETTING`/… |
|  [02]   | `PsdKeyABC`                 | block base        | base of the additional-layer-information block subtypes                             |
|  [03]   | `PsdResourceId`             | resource-id vocab | `IntEnum` of image-resource ids                                                     |
|  [04]   | `PsdResourceBlockABC`       | resource base     | base of the resource-block subtypes                                                 |
|  [05]   | `PsdFormat`                 | format sentinel   | `BE32BIT`(`8BIM`)/`LE32BIT`(`MIB8`)/`BE64BIT`(`8B64`, PSB)/`LE64BIT`(`46B8`)        |
|  [06]   | `PsdCompressionType`        | channel codec     | `UNKNOWN=-1`/`RAW=0`/`RLE=1`/`ZIP=2`/`ZIP_PREDICTED=3`                              |
|  [07]   | `PsdImageMode`              | color model       | `Bitmap`/`Grayscale`/`Indexed`/`RGB`/`CMYK`/`Multichannel`/`Duotone`/`Lab`          |
|  [08]   | `PsdColorSpaceType`         | color space       | RGB/CMYK/Lab + spot `Pantone`/`Focoltone`/`Trumatch`/`Toyo`/`HKS`/`DIC`             |
|  [09]   | `PsdChannelId`              | channel index     | color + `TRANSPARENCY_MASK=-1`/`USER_LAYER_MASK=-2`/`REAL_USER_LAYER_MASK=-3`       |
|  [10]   | `PsdClippingType`           | clip role         | `BASE`/`NON_BASE`                                                                   |
|  [11]   | `PsdSectionDividerType`     | group marker      | `OPEN_FOLDER`/`CLOSED_FOLDER`/`BOUNDING_SECTION_DIVIDER`                            |
|  [12]   | `PsdColorType`              | sheet-color label | `RED`/`ORANGE`/`YELLOW`/`GREEN`/`BLUE`/`VIOLET`/`GRAY`                              |
|  [13]   | `PsdBlendMode`              | blend vocab       | `BytesEnum` sigs — `NORMAL`/`MULTIPLY`/`SCREEN`/`OVERLAY`/…/`PASS_THROUGH`          |
|  [14]   | `PsdLayerFlag`              | layer bitset      | `TRANSPARENCY_PROTECTED`/`VISIBLE`/`OBSOLETE`/`PHOTOSHOP5`/`IRRELEVANT`             |
|  [15]   | `PsdLayerMaskFlag`          | mask bitset       | `RELATIVE`/`DISABLED`/`INVERT`/`RENDERED`/`APPLIED`                                 |
|  [16]   | `PsdLayerMaskParameterFlag` | mask-param flags  | `USER_DENSITY`/`USER_FEATHER`/`VECTOR_DENSITY`/`VECTOR_FEATHER`                     |
|  [17]   | `PsdRectangle`              | geometry tuple    | `PsdRectangle(top, left, bottom, right)` layer/mask bbox in canvas space            |
|  [18]   | `PsdPoint`                  | geometry tuple    | `PsdPoint(y, x)` coordinate                                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the layered-TIFF author/parse round-trip, the channel-codec bridge, and the low-level stream primitives

`export/layered`'s `TIFF` arm authors a `TiffImageSourceData`, derives each tag through `tifftag(...)`, and hands the extratag tuple to `tifffile.imwrite` (the container owner) with `byteorder` read off the authored graph (`byteorder=source_data.byteorder`, fixed to `'>'` by the `PsdFormat.BE32BIT` it authors).

`tifftag(maxworkers=…)` bounds the `imagecodecs` channel-compression thread fan, never an `anyio` worker; `compress`/`decompress` are the explicit channel-codec bridge for a raw-channel path bypassing the per-`PsdChannel.tobytes` default. `tifftag`/`tobytes`/`write` share `psdformat=None, *, compression=None, unknown=True, maxworkers=1`.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]     | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------- | :---------- | :------------------------------------- |
|  [01]   | `TiffImageSourceData.fromtiff(filename, /, *, pageindex=0, unknown=True)`  | classmethod | parse the `ImageSourceData` tag        |
|  [02]   | `TiffImageSourceData.frombytes(data, /, *, name=None, unknown=True)`       | classmethod | parse the tag from a `bytes` buffer    |
|  [03]   | `TiffImageSourceData.tifftag(…) -> tuple`                                  | instance    | emit the `(37724, …)` extratag 5-tuple |
|  [04]   | `TiffImageSourceData.tobytes(…) -> bytes`                                  | instance    | serialize the graph to a tag buffer    |
|  [05]   | `TiffImageSourceData.write(fh, …) -> int`                                  | instance    | serialize the graph to an open file    |
|  [06]   | `TiffImageResources.fromtiff(filename, /, pageindex=0)`                    | classmethod | parse the `ImageResources` tag         |
|  [07]   | `TiffImageResources.tifftag() -> tuple`                                    | instance    | emit the `(34377, …)` extratag         |
|  [08]   | `TiffImageResources.thumbnail() -> NDArray`                                | instance    | decode the embedded JPEG preview       |
|  [09]   | `PsdLayer.asarray(*, channelid=None, planar=False) -> NDArray`             | instance    | `None`=RGBA; `planar`=`(C,H,W)`        |
|  [10]   | `PsdChannel.read_image(fh, psdformat, /, shape, dtype)`                    | instance    | lazily decode a channel into `.data`   |
|  [11]   | `PsdChannel.tobytes(psdformat, /, compression=None)`                       | instance    | compress the channel via `imagecodecs` |
|  [12]   | `compress(data, compression, rlecountfmt) -> bytes`                        | function    | encode a channel by its codec          |
|  [13]   | `decompress(data, compression, shape, dtype, rlecountfmt) -> NDArray`      | function    | inverse of `compress`                  |
|  [14]   | `overlay(*layers, shape=None, vmax=None) -> NDArray`                       | function    | over-composite unassoc-alpha layers    |
|  [15]   | `read_psdtags(fh, psdformat, /, length, *, unknown=True, align=2) -> list` | function    | parse the `info` block list            |
|  [16]   | `write_psdtags(fh, psdformat, /, compression, maxworkers, align, *, tags)` | function    | serialize the `info` block list        |
|  [17]   | `read_psdblocks(fh, /, length) -> list`                                    | function    | parse a resource-block list            |
|  [18]   | `write_psdblocks(fh, /, *blocks) -> int`                                   | function    | serialize a resource-block list        |
|  [19]   | `read_tifftag(filename, tag, /, pageindex=0) -> Any`                       | function    | read one raw TIFF tag via `tifffile`   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- three disjoint owners meet at typed seams: `psdtags` authors the `TiffImageSourceData`/`PsdLayers`/`PsdLayer`/`PsdChannel` graph and emits the `tifftag(...)` extratag; `imagecodecs` compresses each `PsdChannel.data` (`RAW`→none, `RLE`→packbits, `ZIP`→deflate, `ZIP_PREDICTED`→delta+deflate, via `compress`/`decompress`); `tifffile` lays the extratag into the IFD; the `_psd_layer` arm builds the graph, `compress` the channel bytes, `tifftag` the IFD value — none re-implements another's concern.
- byte-order is sourced from the graph, never hardcoded: the authored `PsdFormat.BE32BIT` fixes `byteorder` to `'>'`, so the arm reads `source_data.byteorder` and passes it to `tifffile.imwrite`, and a mismatch produces a TIFF Photoshop reads as flat or rejects.
- flat image and layer channels are two distinct arrays: the arm composites the visible layers to ONE `merged` RGBA canvas as the flat TIFF image and builds each `PsdLayer` from the per-layer RGBA as `CHANNEL0/1/2`+`TRANSPARENCY_MASK` `PsdChannel`s over a `PsdRectangle(top, left, bottom, right)` bbox; each channel is a contiguous big-endian `uint8` 2-D slice `.copy()`-d if non-contiguous before `PsdChannel.data`/`compress`, and `TRANSPARENCY_MASK` carries straight (unassociated) alpha.
- group folders are `PsdSectionDividerSetting` blocks on `PsdLayer.info`, not a tree: the format is a FLAT ordered layer list where an `OPEN_FOLDER`/`CLOSED_FOLDER` block opens a group and a `BOUNDING_SECTION_DIVIDER` closes it, both `PsdKeyABC` entries around the member layers in list order.
- blend derives by shared member name: the pipeline `BlendMode` names are a name-aligned subset of `PsdBlendMode`, so the arm indexes `PsdBlendMode[layer.blend.name]` — total over every pipeline mode, no fallback arm, no `getattr` default, no raw 4-char literal; an out-of-vocabulary mode breaks loudly at the index.
- round-trip equality is the correctness oracle: `TiffImageSourceData.fromtiff(written) == source_data` with `unknown=True` on every parse preserving unrecognized blocks as `PsdUnknown`; the receipt records authored-layer count, canvas shape, per-channel `PsdCompressionType`, and the round-trip result — the codecs are lossless, so no precision-loss field — projected onto the `ArtifactReceipt.Preview` case.

[STACKING]:
- `tifffile`(`.api/tifffile.md`): the container owner — `psdtags` emits each tag as a `tifftag(...)` `TagTuple` `tifffile.imwrite` lays into the IFD, `TiffImageSourceData.fromtiff` reads it back through `tifffile.TiffFile`, and `read_tifftag` pulls the raw `37724`/`34377` bytes when a streamed source bypasses `fromtiff`, with `byteorder=source_data.byteorder` the shared contract.
- `imagecodecs`(`.api/imagecodecs.md`): the per-channel codec — `compress(array, PsdCompressionType.ZIP_PREDICTED, rlecountfmt)` runs `imagecodecs.delta_encode`+`deflate_encode` and `PsdChannel.tobytes` selects the codec by `PsdCompressionType`; an absent channel codec routes to `RAW`/`none` with the achieved method on the receipt.
- `numpy`(`.api/numpy.md`): every `PsdChannel.data` is a contiguous big-endian 2-D array, `PsdLayer.asarray`/`overlay` return `NDArray`, and `decompress` reconstructs `(shape, dtype)`; slice each layer's channels from the placed canvas and `.copy()` a non-contiguous slice before `PsdChannel.data`.
- within-lib concurrency: the `export/layered` `TIFF` arm crosses ONE `HOSTILE` process worker seam (`self.lane.offload(...)`, reusing the ORA `pyvips` composite path), so the `pyvips` composite, the `psdtags` graph author, and the `imagecodecs` compression share that single crossing with `tifftag`/strip `maxworkers` fans bounded inside it, never a second `anyio` worker nor the `RELEASING` thread arm the `SVG`/`PDF` arms use.
- within-lib rails: the layered-export policy is a typed `msgspec.Struct` whose validated `BlendMode`/`PsdCompressionType` values feed the `PsdBlendMode[blend.name]` index, the `beartype`-annotated boundary surfaces an out-of-bounds bbox as the `export/layered` `ExportFault`, and the author/parse/codec are pure and deterministic, never `@retry`-wrapped.

[LOCAL_ADMISSION]:
- boundary-scope `lazy import psdtags` only — module-level import is banned by the manifest import policy, so the `export/layered` page reifies it on first `TIFF`-arm use (as it does `tifffile`/`pyvips`/`numpy`) and `psdtags` never loads on the runtime loader path.
- one layered-format owner per editor family: a Photoshop-compatible layered TIFF routes through `psdtags`+`tifffile`, a native `.psd`/`.psb` through `PhotoshopAPI`/`psd-tools`, an editable PDF through `pymupdf`/`pikepdf`, a GIMP/Krita raster stack through the `ORA` arm.

[RAIL_LAW]:
- Package: `psdtags`
- Owns: the layered-TIFF `ImageSourceData` (tag 37724) + `ImageResources` (tag 34377) structure — the `TiffImageSourceData`/`PsdLayers`/`PsdLayer`/`PsdChannel` graph, the `PsdLayerMask`/`PsdUserMask`/`PsdFilterMask` masks, the `PsdKey`/`PsdKeyABC` block family (incl. `PsdSectionDividerSetting` group folders) and the `PsdResourceId`/`PsdResourceBlockABC` resource-block family, the closed enum vocabularies, the `compress`/`decompress` codec bridge into `imagecodecs`, the `tifftag`/`fromtiff` seam into `tifffile`, and the `overlay` unassociated-alpha compositor
- Accept: authoring a `TiffImageSourceData` from a placed canvas RGBA array and per-layer bboxes; each `PsdLayer` as `CHANNEL0/1/2`+`TRANSPARENCY_MASK` `PsdChannel`s over a `PsdRectangle`; the name-aligned `PsdBlendMode[blend.name]` index + the `PsdLayerFlag` derivation; `tifftag(maxworkers=…)` emitted for `tifffile.imwrite` with `byteorder=isd.byteorder`; group folders as `PsdSectionDividerSetting` dividers on `PsdLayer.info` in list order; `unknown=True` lossless parse + the `fromtiff`-round-trip oracle; the `TIFF` arm crossing the `export/layered` `HOSTILE` process seam with the channel `maxworkers` fan bounded inside it
- Reject: re-implementing a channel codec, a TIFF directory write, or a canvas composite (`imagecodecs`/`tifffile`/`pyvips` own them); authoring a native `.psd`/`.psb` with `psdtags` where `PhotoshopAPI`/`psd-tools` are categorical-best; hardcoding `byteorder='>'` independently of `isd.byteorder`; a `getattr(PsdBlendMode, name, default)` or raw 4-char literal where the total name-aligned index derives; a nesting tree where the format is a flat `PsdSectionDividerSetting`-divided list; a stride/non-contiguous channel view reaching `PsdChannel.data`/`compress` without `.copy()`; `unknown=False` dropping unmodeled blocks; `@retry` around a pure author/parse/codec; a second `anyio` worker on the channel pool; module-level import
