# [PY_ARTIFACTS_API_PSD_TOOLS]

`psd-tools` owns in-process Adobe PSD/PSB parse, inspect, blend-faithful composite, structural authoring, and round-trip for the artifacts layered-raster rail. `PSDImage` is the document root and top-of-tree `GroupMixin`; every node discriminates over the closed `Layer` family, and `composite`/`topil`/`numpy` egress its pixels, masks, effects, and text across the Pillow/NumPy seam. `export/layered` composes it as the `.psd`-native channel-stack reader and inspect/extract path, never re-implementing the record parser, compositor, RLE/ZIP codec, or PIL/NumPy bridge it owns, nor re-authoring the IFC/semantic models the C# boundary holds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `psd-tools`
- package: `psd-tools` (`MIT`, Kota Yamaguchi)
- module: `psd_tools`
- asset: pure Python but for one `abi3` stable-ABI extension (`compression/_rle.abi3.so`) backing RLE; the optional `composite` extra (`aggdraw`/`scipy`/`scikit-image`) rasterizes vector `ShapeLayer` fills during `composite`, never for pixel authoring or round-trip
- rail: layered
- target: `.psd`/`.psb` path/stream/bytes in, `PIL.Image.Image` and `numpy.ndarray` band arrays across the raster seam
- capability: `open` a PSD/PSB from path/stream/bytes, `new`/`frompil` a blank canvas, walk the tree through the `GroupMixin` algebra, discriminate each node over the closed `Layer` family, read per-node pixels/masks/effects/text/shapes/smart-objects with native blend and ICC, author layers/groups/masks and relocate/lock them, and `save` back with a per-channel `Compression`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root + the closed layer family

`PSDImage` is the document root and a `GroupMixin`, so the tree iterates and indexes directly on it. Every node is a `Layer` — the polymorphic root carrying geometry/blend/opacity/mask/effects/clipping and the `topil`/`numpy`/`composite` egress — its concrete kind read from `Layer.kind` over the seven subclasses, never a parallel reader. `Group` is the only container subclass, and `Mask`/`Effects`/`SmartObject` are the side-channel owners reached off a layer.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                               |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `PSDImage`                | document root      | `GroupMixin` over top-level layers + factory/egress/save surface           |
|  [02]   | `Layer`                   | layer root         | geometry/blend/opacity/mask/effects/clipping + `topil`/`numpy`/`composite` |
|  [03]   | `GroupMixin`              | container algebra  | traversal + mutation on `PSDImage`/`Group` (`descendants`/`find`/`append`) |
|  [04]   | `Group`                   | group layer        | contains layers; `Group.new`/`group_layers`; `open_folder` state           |
|  [05]   | `PixelLayer`              | raster layer       | bitmap channels; `PixelLayer.frompil` author; `has_pixels`                 |
|  [06]   | `TypeLayer`               | text layer         | `text`/`engine_dict`/`font_names`/`text_type`/`transform`/`warp`           |
|  [07]   | `ShapeLayer`              | vector layer       | vector-mask shape + `stroke`/`origination`; geometry-derived bbox          |
|  [08]   | `SmartObjectLayer`        | smart object layer | wraps a `SmartObject` (embedded/linked external asset)                     |
|  [09]   | `AdjustmentLayer`         | adjustment layer   | non-destructive edit (`Curves`/`Levels`/`HueSaturation`/... subclasses)    |
|  [10]   | `FillLayer`               | fill layer         | `SolidColorFill`/`GradientFill`/`PatternFill`                              |
|  [11]   | `Mask`                    | layer mask         | raster/vector mask; `topil`/`data`/`bbox`/`disabled`/`real_flags`          |
|  [12]   | `Effects`                 | layer styles       | `DropShadow`/`OuterGlow`/`ColorOverlay`/`Stroke`/`BevelEmboss`/`Satin`     |
|  [13]   | `SmartObject`             | smart payload      | embedded/linked bytes + `filetype`/`resolution`/`warp`/`open`/`save`       |
|  [14]   | `PSDDecompressionWarning` | warning            | non-fatal decode-size signal (`UserWarning`)                               |
|  [15]   | `PSDLargeImageWarning`    | warning            | non-fatal large-image signal (`UserWarning`)                               |

[PUBLIC_TYPE_SCOPE]: constant vocabularies (closed Enum/IntEnum tables)

`psd-tools` keys the layer records on these closed enums — pass the member, never a raw PSD int or byte.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [CAPABILITY]                                                                        |
| :-----: | :--------------- | :----------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `BlendMode`      | blend vocabulary   | `PASS_THROUGH`/`NORMAL`/`MULTIPLY`/`SCREEN`/`OVERLAY`/…/`LUMINOSITY`, 4-byte tokens |
|  [02]   | `Compression`    | channel codec      | `RAW`/`RLE`/`ZIP`/`ZIP_WITH_PREDICTION` — per-channel save codec                    |
|  [03]   | `ColorMode`      | color model        | `BITMAP`/`GRAYSCALE`/`INDEXED`/`RGB`/`CMYK`/`MULTICHANNEL`/`DUOTONE`/`LAB`          |
|  [04]   | `ChannelID`      | channel index      | `CHANNEL_0..9`/`TRANSPARENCY_MASK`/`USER_LAYER_MASK`/`REAL_USER_LAYER_MASK`         |
|  [05]   | `Clipping`       | clip role          | `BASE`/`NON_BASE` — clipping-layer membership                                       |
|  [06]   | `SectionDivider` | group marker       | `OTHER`/`OPEN_FOLDER`/`CLOSED_FOLDER`/`BOUNDING_SECTION_DIVIDER`                    |
|  [07]   | `Tag`            | tagged-block key   | `TYPE_TOOL_OBJECT_SETTING`/`VECTOR_MASK_SETTING1`/`SMART_OBJECT_LAYER_DATA1`/…      |
|  [08]   | `Resource`       | image-resource key | `ICC_PROFILE`/`RESOLUTION_INFO`/`XMP_METADATA`/`THUMBNAIL_RESOURCE`/…               |
|  [09]   | `ProtectedFlags` | lock vocabulary    | `COMPLETE`/transparency/composite/position — `Layer.lock(flags)` argument           |
|  [10]   | `TextType`       | text layout        | `POINT`/`PARAGRAPH` — `TypeLayer.text_type`                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction (one factory family, source-discriminated)

`PSDImage.open` is the single decode factory across path/stream/bytes/`PathLike`, PSD and PSB discriminated internally and surfaced on `version`. `max_alloc_bytes` caps the decompression-bomb allocation behind the two warnings. `new` mints a blank document keyed by a `ColorMode`-mapped mode string, size, `depth` (8/16/32), and background `color`; `frompil` lifts one PIL image to a one-layer document. Authoring carries no public version knob — `new` derives `FileHeader.version` from canvas size: version 2 (PSB) above 30000 px on either axis, version 1 at or below, a hard 300000 px `ValueError` cap.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `PSDImage.open(fp, max_alloc_bytes=None, **kwargs) -> PSDImage`                | factory  | decode path/stream/bytes/`PathLike`     |
|  [02]   | `PSDImage.new(mode, size, color=0, depth=8, **kwargs) -> PSDImage`             | factory  | blank doc; `RGB`/`RGBA`/`CMYK`/`L`/`LA` |
|  [03]   | `PSDImage.frompil(image, compression=Compression.RLE, color=None) -> PSDImage` | factory  | lift one PIL image to a 1-layer doc     |
|  [04]   | `PSDImage.save(fp, mode='wb', **kwargs) -> None`                               | instance | encode the tree to PSD/PSB bytes        |

[ENTRYPOINT_SCOPE]: tree traversal + structural authoring (the `GroupMixin` algebra + layer factories/moves)

Every mint factory shares the tail `(name, top=0, left=0, compression=Compression.RLE, opacity=255, blend_mode=BlendMode.NORMAL)`; `create_group`/`Group.new`/`group_layers` default `blend_mode=BlendMode.PASS_THROUGH` with `open_folder=True`, and `extract_bbox` takes `include_invisible=False`/`include_clipping=False` — the SURFACE cells below carry only the distinguishing arguments.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `GroupMixin.descendants(include_clip=True) -> Iterator[Layer]`               | instance | recursive depth-first subtree flatten |
|  [02]   | `GroupMixin.find(name) -> Layer \| None`; `findall(name) -> Iterator[Layer]` | instance | first / all matches by name           |
|  [03]   | `GroupMixin.insert(index, layer)`; `pop(index=-1) -> Layer`                  | instance | list-protocol child mutation          |
|  [04]   | `PSDImage.create_pixel_layer(image, ...) -> PixelLayer`                      | instance | mint + attach a raster layer          |
|  [05]   | `PSDImage.create_group(layer_list=None, ...) -> Group`                       | instance | mint + attach a group, re-parenting   |
|  [06]   | `PixelLayer.frompil(image, parent, ...) -> PixelLayer`                       | factory  | mint a detached raster layer          |
|  [07]   | `Group.new(parent, ...)`; `group_layers(parent, layers, ...)`                | factory  | mint empty / layers-wrapping group    |
|  [08]   | `Layer.move_to_group(group) -> Self`                                         | instance | relocate under a group                |
|  [09]   | `Layer.move_up(offset=1)`; `move_down(offset=1) -> Self`                     | instance | re-order within the parent            |
|  [10]   | `Layer.delete_layer() -> Self`                                               | instance | remove from the tree                  |
|  [11]   | `Layer.lock(lock_flags=ProtectedFlags.COMPLETE) -> None`                     | instance | set the protected/locked flags        |
|  [12]   | `Layer.unlock() -> None`                                                     | instance | clear the protected/locked flags      |
|  [13]   | `Group.extract_bbox(layers, ...) -> tuple[int,int,int,int]`                  | instance | union bbox over a layer set           |

[ENTRYPOINT_SCOPE]: pixel/mask/effects/text egress + the PIL/NumPy seam

`composite` returns a flattened `PIL.Image.Image` — the whole document on `PSDImage`, that node on `Layer`/`Group`. Four signatures hoist here: `composite(viewport=None, force=False, color=1.0, alpha=0.0, layer_filter=None, ignore_preview=False, apply_icc=True)`, the module form `composite_pil(layer, color, alpha, viewport, layer_filter, force, as_layer=False, apply_icc=True)`, `Mask.topil(real=True, layer_sized=False, **kwargs)`, and `create_mask(image, top=None, left=None, compression=Compression.RLE)`. `topil`/`numpy` extract one node's stored channels with no blend, `channel`-selectable (`'color'`/`'shape'`/`'alpha'`/`'mask'` or a `ChannelID`) and `None` when absent; `pil_io`/`numpy_io` are the lower seam the methods sit on, callable for batch conversion.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `PSDImage.composite(...)`; `Layer.composite(...) -> Image.Image`   | instance | blend-faithful document/node flatten |
|  [02]   | `composite.composite_pil(...) -> Image.Image \| None`              | static   | module-level compositor              |
|  [03]   | `PSDImage.topil(channel=None, apply_icc=True)`; `Layer.topil(...)` | instance | stored channels to PIL (no blend)    |
|  [04]   | `PSDImage.numpy(channel=None, real_mask=True)`; `Layer.numpy(...)` | instance | channels to a NumPy band array       |
|  [05]   | `PSDImage.thumbnail() -> Image.Image \| None`                      | instance | the embedded merged preview          |
|  [06]   | `PSDImage.has_thumbnail()`; `has_preview() -> bool`                | instance | merged-preview presence probes       |
|  [07]   | `Mask.topil(...) -> Image.Image \| None`                           | instance | layer mask to grayscale PIL          |
|  [08]   | `Layer.create_mask(...) -> Mask`                                   | instance | author a raster layer mask           |
|  [09]   | `Layer.update_mask(image)`; `remove_mask()`                        | instance | replace / drop a raster mask         |
|  [10]   | `Effects.find(name, enabled=True) -> Iterator[_Effect]`            | instance | enumerate enabled styles             |
|  [11]   | `Effects.items -> list`; `enabled -> bool`                         | property | style list + any-enabled flag        |
|  [12]   | `TypeLayer.text -> str`; `engine_dict`                             | property | live text + type-engine dict         |
|  [13]   | `TypeLayer.transform`; `warp`                                      | property | text transform + warp descriptors    |
|  [14]   | `SmartObject.open(external_dir=None) -> IO[bytes]`; `save(...)`    | instance | extract / re-emit the asset          |
|  [15]   | `SmartObject.filetype`; `resolution`                               | property | asset format + DPI evidence          |
|  [16]   | `pil_io.convert_layer_to_pil(...) -> Image.Image \| None`          | static   | lower PIL seam (ICC/mode)            |
|  [17]   | `pil_io.convert_image_data_to_pil(...)`                            | static   | image-data plane to PIL              |
|  [18]   | `pil_io.get_pil_mode(color_mode, alpha=False) -> str`              | static   | PSD mode -> PIL mode bridge          |
|  [19]   | `numpy_io.get_array(layer_or_psd, channel, ...)`                   | static   | lower NumPy band seam (batch)        |
|  [20]   | `numpy_io.get_layer_data(...)`                                     | static   | per-layer band extraction            |
|  [21]   | `numpy_io.get_image_data(...)`                                     | static   | merged-image band extraction         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PSDImage.open` is the single decode factory across path/stream/bytes/`PathLike`; PSD versus PSB discriminates internally and surfaces on `version`. `max_alloc_bytes` bounds the decompression-bomb allocation for an untrusted blob, and the `PSDDecompressionWarning`/`PSDLargeImageWarning` pair rides as a structured evidence signal.
- `GroupMixin` is the layer-tree algebra on `PSDImage`/`Group` — iterate/index for direct children, `descendants(include_clip=...)` for the recursive flatten, `find`/`findall` for name lookup; each node's kind reads from `Layer.kind` and the seven subclasses. That same container is the mutation surface (`append`/`insert`/`extend`/`remove`/`pop`/`clear`), so re-parenting and re-ordering stay one algebra.
- `create_pixel_layer`/`create_group` mint-and-attach, `PixelLayer.frompil`/`Group.new`/`group_layers` mint detached then attach, and `move_to_group`/`move_up`/`move_down`/`delete_layer`/`lock`/`unlock` own relocation and lifecycle; `opacity`/`blend_mode`/`compression`/`top`/`left` are constructor rows carrying `BlendMode`/`Compression` members.
- `composite` is the blend-faithful flatten honoring each node's `BlendMode`, `opacity`/`fill_opacity`, `clipping`/`clip_layers`, `mask`, and `effects`, with `viewport`/`layer_filter`/`apply_icc` knobs; rasterizing a vector `ShapeLayer` fill pulls the optional `composite` extra, a pixel-only document composites without it.
- `topil(channel=...)`/`numpy(channel=...)` extract one node's stored channels keyed by `'color'`/`'shape'`/`'alpha'`/`'mask'` or a `ChannelID`, and `apply_icc` runs the embedded `ICC_PROFILE` post-process; `thumbnail`/`has_thumbnail` recover the embedded merged preview as a cheap proxy when a full `composite` is not needed.
- `Mask`, `Effects`, `TypeLayer`, and `SmartObject` are each reached off the owning layer, never a re-parse of the raw tagged block, and `SmartObject.open` is a context manager over the embedded/linked asset.
- `save` re-emits the whole tree with a per-channel `Compression`; the `ZIP`/`ZIP_WITH_PREDICTION` arm is the deflate seam onto the universal compression substrate, RLE riding the bundled `_rle.abi3.so`. `ColorMode` is the document color model, so a CMYK or LAB PSD round-trips and composites in its native space through the `pil_io.get_pil_mode(color_mode, alpha)` bridge.
- Parse/IO failure raises ordinary exceptions; the two `UserWarning` subclasses are non-fatal size signals lifted to the layered-export fault rail once at the boundary.

[STACKING]:
- `numpy`(`.api/numpy.md`): `Layer.numpy(channel=...)`/`PSDImage.numpy()` out and `Image.fromarray(arr)` -> `create_pixel_layer` in; `numpy_io.get_array`/`get_layer_data` return `uint8`/`uint16`/`float32` band arrays, so a `pyvips`/`scikit-image` raster becomes a layer and an extracted layer leaves to the array tier — `numpy()` over `topil().__array__()` for band math.
- `pillow`(`.api/pillow.md`): `topil`/`composite` egress and `frompil`/`create_pixel_layer`/`create_mask` ingress are `PIL.Image.Image`, the exchange currency to every raster owner — a `pyvips` result (`write_to_memory` -> `Image.frombytes`) becomes a layer and a composited PSD becomes a Pillow image, without a re-encoded file round-trip.
- compression (`data/.api/numcodecs.md`, `.api/zlib-ng.md`, `.api/imagecodecs.md`): `Compression.ZIP`/`ZIP_WITH_PREDICTION` is the deflate seam — the managed compression substrate backs the channel ZIP path, so the PSD write codec and the TIFF/ORA container codec share one owner.
- `expression`(`.api/expression.md`): `LayeredExport.of` returns `Result[LayeredExport, ExportFault]`; a readback failure converts to the closed boundary fault and structural evidence maps to the artifact receipt.
- `msgspec`(`.api/msgspec.md`): `Layer` is the frozen typed authoring row `PhotoshopAPI` consumes, and `PSDImage.open` verifies the finished tree by layer name — no parallel DTO or runtime type decorator.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each op stamps `version` (PSD/PSB), `color_mode`, `depth`, `size`, layer count + per-layer kind/name/bbox/blend/opacity/visibility, `ICC_PROFILE`/thumbnail presence, per-channel `Compression`, and output byte length as the event/span payload.
- `pyexiftool`(`.api/pyexiftool.md`): reads `ICC_PROFILE`/`XMP_METADATA`/`RESOLUTION_INFO` off the resource blocks for the metadata owners.
- within-lib: `export/layered` is the primary consumer — reopens the `PhotoshopAPI` output with `PSDImage.open(max_alloc_bytes=...)` and proves every authored layer addressable through `find`; the readback crosses one `LanePolicy.offload(Kernel.of(..., KernelTrait.HOSTILE), ...)` seam, only the finished bytes-and-evidence `LayerFact` returning to the async caller.

[LOCAL_ADMISSION]:
- `import` at boundary scope only — `from psd_tools import PSDImage`, `from psd_tools.constants import BlendMode, Compression, ColorMode, ChannelID`, `from psd_tools.api.layers import Layer, Group, PixelLayer, TypeLayer, ShapeLayer, SmartObjectLayer`; the `export/layered` lazy-import proxy reifies the module on first use. No native or JRE gate binds — the one abi3 RLE extension loads on any supported interpreter, so a missing import is a packaging fault, never a host-capability gate.
- `composite`'s optional extra binds only where a vector `ShapeLayer` fill must rasterize; a pixel-only document needs none.

[RAIL_LAW]:
- Package: `psd-tools`
- Owns: in-process PSD/PSB parse, inspect, blend-faithful composite, structural authoring, and round-trip — `open`/`new`/`frompil` construction; the `GroupMixin` tree algebra; the closed `Layer` family discrimination (pixel/group/type/shape/smart-object/adjustment/fill); per-node/per-document `composite`/`topil`/`numpy` egress with native blend and ICC; layer masks (`Mask`), styles (`Effects`), live text (`TypeLayer`), and smart objects (`SmartObject`); `create_pixel_layer`/`create_group`/`frompil`/`Group.new` authoring, `move_*`/`lock`/`create_mask` lifecycle, and `save` with a per-channel `Compression`
- Accept: structural readback of the `PhotoshopAPI` PSD/PSB output in `export/layered`; reading an incoming `.psd`/`.psb` layer tree, text, smart-object payloads, masks, and effects into the document model; PIL rasters from `pyvips`/`pillow` via `frompil`/`create_pixel_layer`; NumPy band arrays via `numpy()`/`get_array`; `BlendMode`/`Compression`/`ColorMode` typed rows from editor workflows; the `ICC_PROFILE`/`XMP_METADATA`/`RESOLUTION_INFO` resource blocks for the metadata owners
- Reject: a wrapper-rename of `open`/`save`/`composite`/`topil`; a manual `LayerRecord`/`ChannelDataList` build where `create_pixel_layer`/`PixelLayer.frompil`/`create_group` exist; an `isinstance` ladder re-deriving layer kind where `Layer.kind` and the subclass family discriminate; a layer-by-layer `Image.paste` discarding `BlendMode`/`clipping`/`mask`/`effects` where `composite` renders them; a `topil().convert('RGB')` dropping the embedded ICC where `apply_icc=True` applies it; a raw PSD int/byte where a `BlendMode`/`Compression`/`ColorMode`/`ChannelID` member exists; an unbounded `max_alloc_bytes`-free decode of an untrusted blob; a private zlib for the channel ZIP codec where the universal compression substrate backs it; re-authoring the layered-TIFF container (`psdtags`/`tifffile`), the high-throughput native PSD writer (`PhotoshopAPI`), the raster pixel edge (`pyvips`/`pillow`), or the IFC/semantic model (`Rasm.Bim`)
