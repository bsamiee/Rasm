# [PY_ARTIFACTS_API_PHOTOSHOPAPI]

`PhotoshopAPI` (import `photoshopapi`, conventional alias `psapi`) supplies the native PSD/PSB read+write surface for the artifacts `export/layered` rail: a bit-depth-templated `LayeredFile` document holding a real layer hierarchy, `ImageLayer`/`GroupLayer`/`SmartObjectLayer`/`TextLayer` nodes carrying per-channel `numpy` pixel data plus the full editor-panel attribute axis (blend mode, opacity, fill, visibility, lock, clipping mask, layer mask with feather/density/position, display color), and a `LayeredFile.write` close that emits a Photoshop-faithful `.psd`/`.psb` with implicit PSD↔PSB promotion. It is the categorical-best owner of PSD/PSB *authoring* the brief admits to supersede the layered-TIFF approximation: the `export/layered#LAYERED` owner adds a `PSD` (and `PSB`) `ExportTarget` whose arm constructs one `psapi.ImageLayer_<bit>` per `Layer` row over a `ChannelID`-keyed channel dict and writes a real channel-stack document, where the retained `psdtags`/`tifffile` arm can only graft Photoshop layer records onto a TIFF container. The owner composes `LayeredFile`, the layer-node family, and the `psapi.enum` vocabularies into the layered arm; it never re-implements the PSD/PSB binary container, the per-channel RLE/ZIP/ZipPrediction codec, the tagged-block descriptor model, or the smart-object warp algebra the C++20 core already owns — downstream receives only `LayerFact` bytes, a `ContentKey`, and the shared `ArtifactReceipt.Preview` dimensions/layer count.

This catalog drives the layered-export rail where `PhotoshopAPI` owns native PSD/PSB author+read, `psd-tools` owns pure-Python/abi3 PSD *read/inspect* and the cp315-present fallback authoring path, `imagecodecs` owns the PackBits/ZIP channel codec for the layered-TIFF container, and `psdtags`+`tifffile` own Photoshop-compatible layered-TIFF tags — every owner meeting the others at decoded RGBA `numpy` planes or finished container bytes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `photoshopapi`
- package: `PhotoshopAPI`
- import: `photoshopapi` (conventional alias `psapi`)
- owner: `artifacts`
- rail: `export/layered`
- build-floor: `>=3.8`; published wheels `cp38`–`cp313` (no `cp314`/`cp315` wheel, no sdist build-on-install) — the root manifest gates `PhotoshopAPI; python_version<'3.15'`; on this `cp315` interpreter the dist is ABSENT, so this catalog is sourced from the official binding surface (the `pybind11` declarations + maintained `.pyi` stubs at `EmilDohne/PhotoshopAPI@055cad5`), not `assay api` reflection; re-resolve via `uv run --frozen python -m tools.assay api resolve photoshopapi` once a `cp315` wheel lands or a source build is provisioned
- license: `BSD-3-Clause` (permissive; commercial-safe, no copyleft, no Pantone-licensed data — unlike the AGPL `pymupdf`/`pymupdf` PDF arm this imposes no source-disclosure obligation on the layered close)
- binding: C++20 core, `pybind11` extension (`scikit-build-core` build backend; the brief's "native PSD/PSB writer, gate `python_version<'3.15'` or source-build")
- entry points: none (library only)
- capability: PSD/PSB read with automatic bit-depth deduction (`LayeredFile.read`); 8/16/32-bit document author (`LayeredFile_8bit`/`_16bit`/`_32bit`); RGB/CMYK/Grayscale color modes; arbitrarily-nested layer hierarchy (group → image/group/smart-object/text); per-channel `numpy` pixel author and extract keyed by logical index or `ChannelID`; full editor-panel layer attributes (28-member `BlendMode`, opacity, fill, visibility, lock, clipping mask, 12-member display `LayerColor`); layer masks with default-color/density/feather/position/disabled/relative; smart objects with affine transform + warp + external/embedded linkage; per-layer or document-wide write `Compression` (Raw/RLE/ZIP/ZipPrediction); ICC profile attach (interpretation only, no color conversion); DPI; text-cache invalidation for template re-render; advertised faster + smaller-file PSD/PSB IO than Photoshop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and bit-depth specializations
- rail: `export/layered`

`LayeredFile` is the read dispatcher (`LayeredFile.read(path)` deduces the file's bit depth and returns the matching specialization); the three `LayeredFile_<bit>` classes are the authored document roots, the template parameter `T` (`bpp8_t`/`bpp16_t`/`bpp32_t`) fixing channel dtype to `uint8`/`uint16`/`float32`. One specialization is selected at construction and never re-templated; the layered arm picks it off the source `Layer` channel dtype, never a `bit_depth` knob the caller toggles. The three roots share one document surface — layer hierarchy, ICC/DPI/dims, layer `add`/`move`/`remove`/`find`, `read`/`write` — differing only in channel dtype.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]      | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------------ | :------------------------------------------------------------- |
|  [01]   | `LayeredFile`       | read dispatcher     | `read(path)` deduces on-disk depth → `_8bit`/`_16bit`/`_32bit` |
|  [02]   | `LayeredFile_8bit`  | document root (u8)  | `uint8` channels                                               |
|  [03]   | `LayeredFile_16bit` | document root (u16) | `uint16` channels                                              |
|  [04]   | `LayeredFile_32bit` | document root (f32) | `float32` channels                                             |

[PUBLIC_TYPE_SCOPE]: layer node family
- rail: `export/layered`

`Layer_<bit>` is the abstract node base carrying every editor-panel attribute; `ImageLayer_<bit>`, `GroupLayer_<bit>`, `SmartObjectLayer_<bit>`, and `TextLayer_<bit>` derive from it. A `LayeredFile_<bit>.layers` (root) or `GroupLayer_<bit>.layers` (nested) walk yields `Layer_<bit>` references the consumer narrows with `isinstance(layer, psapi.GroupLayer_8bit)`; the layered arm constructs the leaf subclass directly and never re-derives a parallel node type.

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]    | [CAPABILITY]                                                                             |
| :-----: | :----------------------- | :---------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Layer_<bit>`            | node base         | name/blend_mode/opacity/fill/visibility/lock/clipping/display_color + the full mask axis |
|  [02]   | `ImageLayer_<bit>`       | raster leaf       | per-channel `numpy` pixel author/extract; the layered arm's primary node                 |
|  [03]   | `GroupLayer_<bit>`       | hierarchy node    | nested `layers` collection, `is_collapsed`, group-scoped `add_layer`/`remove_layer`      |
|  [04]   | `SmartObjectLayer_<bit>` | smart-object leaf | embedded/external linked image with affine transform + warp + `replace`                  |
|  [05]   | `TextLayer_<bit>`        | text leaf         | live editable text layer (paired with `LayeredFile.invalidate_text_cache` for re-render) |

[PUBLIC_TYPE_SCOPE]: `psapi.enum` bounded vocabularies
- rail: `export/layered`

The closed discriminant vocabularies under the `psapi.enum` submodule. These are the native owners the layered arm's `_PSD_BLEND`/color-mode/compression derivation tables map the page's `BlendMode`/`ExportTarget` rows onto — never re-minted local enums. `ChannelID.mask` is the user-supplied-layer-mask slot (logical channel index `-2`), distinct from `ChannelID.alpha` (index `-1`). Each family's members carry below the grid.

| [INDEX] | [FAMILY]          | [DRIVES]                                                                         |
| :-----: | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `BlendMode`       | `Layer.blend_mode` / layer constructor `blend_mode=`                             |
|  [02]   | `ColorMode`       | `LayeredFile_<bit>(color_mode, w, h)`, layer `color_mode=`                       |
|  [03]   | `BitDepth`        | `LayeredFile.bit_depth` (read-only; fixed by `T`)                                |
|  [04]   | `ChannelID`       | image-layer channel dict keys; `get_channel_by_id`                               |
|  [05]   | `Compression`     | layer `compression=` / `LayeredFile.compression` setter / `set_mask_compression` |
|  [06]   | `LayerColor`      | `Layer.display_color` (Photoshop layer-panel swatch)                             |
|  [07]   | `LinkedLayerType` | `SmartObjectLayer.linkage`, smart-object `link_type=`                            |

- [01]-[BLENDMODE]: `passthrough`(groups only)/`normal`/`dissolve`/`darken`/`multiply`/`colorburn`/`linearburn`/`darkercolor`/`lighten`/`screen`/`colordodge`/`lineardodge`/`lightercolor`/`overlay`/`softlight`/`hardlight`/`vividlight`/`linearlight`/`pinlight`/`hardmix`/`difference`/`exclusion`/`subtract`/`divide`/`hue`/`saturation`/`color`/`luminosity` (28).
- [02]-[COLORMODE]: `rgb` (R,G,B,A) / `cmyk` (C,M,Y,K,A) / `grayscale` (Gray,A).
- [03]-[BITDEPTH]: `bd_8` (`uint8`) / `bd_16` (`uint16`) / `bd_32` (`float32`).
- [04]-[CHANNELID]: `red`/`green`/`blue`/`cyan`/`magenta`/`yellow`/`black`/`gray`/`custom`/`alpha`(−1)/`mask`(−2, user-supplied layer mask).
- [05]-[COMPRESSION]: `raw` (none) / `rle` (fast, low ratio) / `zip` (deflate) / `zipprediction` (deflate + per-scanline delta; best ratio).
- [06]-[LAYERCOLOR]: `none`/`red`/`orange`/`yellow`/`green`/`blue`/`violet`/`gray`/`seafoam`/`indigo`/`magenta`/`fuschia` (12).
- [07]-[LINKEDLAYERTYPE]: `data` (image stored inside the PSD) / `external` (image shipped alongside on disk).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construct, read, write
- rail: `export/layered`

`LayeredFile_<bit>` constructs empty (`color_mode`, `width`, `height`) or via the deducing `LayeredFile.read`; `write` consumes the document (move semantics — the instance is invalid afterward, undefined behavior to reuse) and emits `.psd`/`.psb` with implicit cross-format promotion driven by the path extension. Dimensions cap at 30,000 (PSD) / 300,000 (PSB). The [SURFACE] members are on `LayeredFile_<bit>`, save the static `LayeredFile.read` dispatcher.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                  | [CAPABILITY]                                              |
| :-----: | :----------------------- | :-------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `LayeredFile.read`       | `read(path) -> LayeredFile_<bit>` (static)    | open + deduce bit depth (depth unknown ahead)             |
|  [02]   | `.read`                  | `LayeredFile_8bit.read(path)` (static)        | open a known-depth document                               |
|  [03]   | `(...)` constructor      | `LayeredFile_8bit(color_mode, width, height)` | construct empty (or `()` no-arg)                          |
|  [04]   | `.write`                 | `write(path, force_overwrite=True)`           | serialize `.psd`/`.psb`; instance dead after              |
|  [05]   | `.invalidate_text_cache` | `invalidate_text_cache() -> None`             | strip `Txt2` + dirty text so Photoshop re-renders on open |

[ENTRYPOINT_SCOPE]: layer placement and lookup
- rail: `export/layered`

A layer is constructed, then placed at root (`LayeredFile.add_layer`) or inside a group (`GroupLayer.add_layer(layered_file, layer)` — the document is passed for the duplicate-placement runtime check). Lookup is polymorphic on input shape: `__getitem__` for a single root-level name (chainable through nested groups, since `GroupLayer` also implements `__getitem__`), `find_layer` for a `"Group/Sub/Leaf"` path string. The [SURFACE] members are on `LayeredFile_<bit>`, save the group-scoped `GroupLayer_<bit>.add_layer`. `__getitem__`/`find_layer` raise `KeyError`/`ValueError` on miss; group-scoped removal is `GroupLayer.remove_layer` (int/object/name).

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                  | [CAPABILITY]                                             |
| :-----: | :---------------------- | :-------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `.add_layer`            | `add_layer(layer) -> None`                    | place a layer at scene root                              |
|  [02]   | `GroupLayer.add_layer`  | `add_layer(layered_file, layer)`              | place inside a group (document arg drives no-double-add) |
|  [03]   | `.getitem`              | `file["Red"] -> Layer_<bit>`                  | root-name lookup, chainable `file["G"]["Img"]`           |
|  [04]   | `.find_layer`           | `find_layer(path)`                            | path lookup `"Group/NestedGroup/Leaf"`                   |
|  [05]   | `.move_layer`           | `move_layer(child, parent=None)`              | reparent (`None` parent → root); object or path pair     |
|  [06]   | `.remove_layer`         | `remove_layer(layer)` — `Layer_<bit>` or name | drop from root by object or name                         |
|  [07]   | `.is_layer_in_document` | `is_layer_in_document(layer) -> bool`         | membership probe at any nesting depth                    |

[ENTRYPOINT_SCOPE]: `ImageLayer` construction and channel I/O
- rail: `export/layered`

`ImageLayer_<bit>` is the layered arm's primary node. The constructor is overloaded on the `image_data` shape — a single planar/interleaved `numpy.ndarray`, an `int`-keyed channel dict (logical indices: `0/1/2` = R/G/B, `-1` = alpha), or a `ChannelID`-keyed dict (explicit, the preferred form). For a `(C, H, W)` or `(C, H*W)` array the first axis is channel count; `width`/`height` are required when constructing from data. Opacity is `0.0–1.0` (mapped to the on-disk `0–255`). Every `ImageLayer_<bit>(...)` constructor takes `(image_data, layer_name, layer_mask=None, width, height, blend_mode, pos_x=0, pos_y=0, opacity=1.0, compression, color_mode, is_visible=True, is_locked=False)`; rows [01]-[03] differ only in the `image_data` shape. The [SURFACE] members are on `ImageLayer_<bit>`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                     | [CAPABILITY]                                         |
| :-----: | :---------------------- | :----------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `(ndarray, …)`          | `image_data: numpy.ndarray`                      | construct from a planar/interleaved array            |
|  [02]   | `(dict[int], …)`        | `image_data: dict[int, numpy.ndarray]`           | index-keyed channel planes (logical indices)         |
|  [03]   | `(dict[ChannelID], …)`  | `image_data: dict[ChannelID, numpy.ndarray]`     | explicit `ChannelID`-keyed planes (preferred)        |
|  [04]   | `.get_image_data`       | `get_image_data() -> dict[int, numpy.ndarray]`   | extract all channels incl. mask (index dict)         |
|  [05]   | `.set_image_data`       | `set_image_data(data, width=None, height=None)`  | replace channel data in place                        |
|  [06]   | `.get_channel_by_id`    | `get_channel_by_id(key: ChannelID)`              | one channel by semantic id (`ChannelID.mask` → mask) |
|  [07]   | `.get_channel_by_index` | `get_channel_by_index(key)`; `[0]`/`[-2]`/`[-1]` | one channel by logical index (also `getitem`)        |
|  [08]   | `.channel_indices`      | `channel_indices() -> list[int]`                 | the present logical channel indices                  |

[ENTRYPOINT_SCOPE]: `GroupLayer` / `SmartObjectLayer` construction
- rail: `export/layered`

`GroupLayer_<bit>` nests a layer collection; its `width`/`height`/mask kwargs matter only when the group carries a mask. `SmartObjectLayer_<bit>` embeds (or externally links) an image file as a non-destructive smart object with an affine transform + warp; it takes the owning `LayeredFile` and source `path` and supports live `replace`, with an affine transform + warp. Both constructors share the `(…, blend_mode, opacity=1.0, compression, color_mode, is_visible=True, is_locked=False)` attribute tail. The [SURFACE] rows are on `GroupLayer_<bit>` ([01]-[03]) then `SmartObjectLayer_<bit>` ([04]-[05]); the smart-object transform and source method families carry as keyed rows below.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                      | [CAPABILITY]                                          |
| :-----: | :-------------------- | :------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | `GroupLayer(…)`       | `GroupLayer_8bit(layer_name, width, height)`      | empty group; default blend `passthrough`              |
|  [02]   | `.is_collapsed`       | property `bool`                                   | the Photoshop layer-panel folded state                |
|  [03]   | `.remove_layer`       | `remove_layer(index \| layer \| layer_name)`      | drop a child by index, object, or name (polymorphic)  |
|  [04]   | `SmartObjectLayer(…)` | `SmartObjectLayer_8bit(layered_file, path, name)` | embed/link a smart object (`link_type=…`) from a file |
|  [05]   | `.replace`            | `replace(path, link_externally=False) -> None`    | swap the backing image, preserving warp/transform     |

- [06]-[SMARTOBJECTLAYER_TRANSFORM]: `move`/`rotate`/`scale`/`transform(matrix)`/`reset_transform`/`reset_warp` — non-destructive affine + warp manipulation.
- [07]-[SMARTOBJECTLAYER_SOURCE]: `get_original_image_data`/`original_width`/`original_height`/`filename`/`filepath`/`hash` — inspect the linked original (pre-transform).

[ENTRYPOINT_SCOPE]: document and layer properties
- rail: `export/layered`

The document-level properties carry in the grid (all on `LayeredFile_<bit>`); the `Layer_<bit>` editor-panel attribute axis and the layer-mask parameter set — both full property enumerations — carry as keyed rows below it.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                    | [CAPABILITY]                                     |
| :-----: | :----------------------------- | :---------------------------------------------- | :----------------------------------------------- |
|  [01]   | `.layers`                      | property `list[Layer_<bit>]` (read-only)        | the root-level layer list                        |
|  [02]   | `.flat_layers`                 | property `list[Layer_<bit>]` (read-only)        | flatten of every layer for one-shot iteration    |
|  [03]   | `.icc`                         | property `numpy.ndarray`; setter `ndarray`/path | attach ICC (hint only; → Pillow `ImageCms`)      |
|  [04]   | `.compression`                 | write-only setter `(Compression) -> None`       | set the write compression of every layer at once |
|  [05]   | `.dpi` / `.width` / `.height`  | float / int properties                          | resolution + canvas dims (PSD ≤30k, PSB ≤300k)   |
|  [06]   | `.num_channels` / `.bit_depth` | read-only `int` / `BitDepth`                    | channel count (mask excluded) + fixed depth      |

- [07]-[LAYER_ATTRIBUTES]: `name`/`blend_mode`/`opacity`/`fill`/`is_visible`/`is_locked`/`clipping_mask`/`display_color`/`center_x`/`center_y`/`width`/`height` — the full editor-panel attribute axis (all read/write).
- [08]-[LAYER_MASK]: `mask` (`numpy.ndarray`)/`has_mask`/`mask_disabled`/`mask_default_color`/`mask_density`/`mask_feather`/`mask_position`/`mask_relative_to_layer`/`mask_width()`/`mask_height()`/`set_mask_compression` — the layer-mask channel + its full parameter set.

## [04]-[INTEGRATION]

[INTEGRATION_SCOPE]: the `export/layered#LAYERED` PSD/PSB arm — stacking onto the unified `LayeredExport` owner
- rail: `export/layered`

`export/layered.md` admits a `PSD` (and `PSB`) member on the closed `ExportTarget` `StrEnum` plus one `LayerEngine` row binding a `_psd` arm to `Band.WORKER` (libphotoshopapi is off the runtime loader path exactly as libvips is, so the arm crosses the module-level `_GATE`-bounded `to_process.run_sync` worker seam, never the event loop). The arm folds the page's `tuple[Layer, ...]` rows — already placed by the visual producers and decoded to RGBA `numpy` planes (the same pre-rendered, `ContentKey`-keyed raster the `ORA`/`TIFF` arms consume from `graphic/raster/io#RASTER`) — into one `psapi.ImageLayer_<bit>` per row, keyed by `ChannelID`-keyed channel dicts, attaches the editor-panel axis through native setters, and writes the channel-stack document. This is the categorical-best supersession the brief mandates: where the retained `psdtags`/`tifffile` arm grafts `PsdLayer` records onto a *TIFF* container, this arm authors a real `.psd`/`.psb` with native compression and the full attribute axis, so PSD authority moves off the layered-TIFF approximation.

```python conceptual
# export/layered#LAYERED — the PSD/PSB arm, run on the `Band.WORKER` `to_process` seam under `_GATE`.
# `psapi` is off the loader path (like `pyvips`/`lxml`), so it stays the crash-isolated subprocess worker;
# the bit-depth specialization is picked off the source channel dtype, never a caller-set knob.
def _psd(export: LayeredExport) -> LayerFact:
    width, height = (int(extent) for extent in _viewport(export.layers))
    document = psapi.LayeredFile_8bit(psapi.enum.ColorMode.rgb, width, height)  # 8-bit RGB document
    folders: dict[
        str, psapi.GroupLayer_8bit
    ] = {}  # one native group per distinct `group` label — the PSD counterpart to the SVG `<g>` / OCG `/Order` / ORA `<stack>` folder
    for layer in export.layers:
        rgba = _rgba_array(pyvips.Image.new_from_buffer(layer.source, ""))  # the shared decode the ORA/TIFF arms use
        channels = {  # the ChannelID-keyed dict is the preferred construction form (logical indices are the lossy fallback)
            psapi.enum.ChannelID.red: rgba[:, :, 0],
            psapi.enum.ChannelID.green: rgba[:, :, 1],
            psapi.enum.ChannelID.blue: rgba[:, :, 2],
            psapi.enum.ChannelID.alpha: rgba[:, :, 3],
        }
        node = psapi.ImageLayer_8bit(
            channels,
            layer_name=layer.name,
            width=width,
            height=height,
            blend_mode=_PSD_BLEND[layer.blend],  # the page's BlendMode -> psapi.enum.BlendMode derivation table
            opacity=layer.opacity,
            is_visible=layer.visible,
            is_locked=layer.locked,
            compression=psapi.enum.Compression.zipprediction,  # best ratio; the page's LayerPolicy carries the knob
            color_mode=psapi.enum.ColorMode.rgb,
        )
        node.clipping_mask = layer.clip  # native clipping-mask toggle (the layered-TIFF arm cannot express it)
        (
            folders.setdefault(layer.group, _group(document, folders, layer.group)).add_layer(document, node)
            if layer.group
            else document.add_layer(node)
        )
    sink = Path(_scratch(export)) / f"{export.target}.{export.target.value}"  # psapi writes to a path, not a buffer
    document.write(sink, force_overwrite=True)  # consumes the document; reuse is undefined behavior
    return LayerFact(sink.read_bytes(), width=width, height=height, layers=len(export.layers))
```

`_PSD_BLEND` is the page's settled `frozendict[BlendMode, object]` derivation, retargeted from `psdtags.PsdBlendMode` to `psapi.enum.BlendMode` (28 native members vs the page's 16-mode CSS vocabulary — every page mode has a native correspondent, and the extra natives `vividlight`/`linearlight`/`pinlight`/`hardmix`/`subtract`/`divide` extend the table when the page's `BlendMode` grows). The arm consumes `Layer.name`/`source`/`bbox` plus the full `visible`/`locked`/`opacity`/`blend`/`group`/`clip` editor axis the page already models — no new `Layer` field. The bit-depth specialization is selected off the decoded plane dtype (`uint8`→`_8bit`, `uint16`→`_16bit`, `float32`→`_32bit`), never a caller knob, matching the page's "one polymorphic owner discriminates on input value" law.

[INTEGRATION_SCOPE]: cross-tier rails — the shared `libs/python/.api` substrate beneath the layered arm
- rail: `export/layered`

The layered arm composes the universal substrate tier ON TOP OF this folder package, never a folder-only subset:
- `numpy` (`libs/python/.api/numpy.md`) — the channel planes are `numpy.ndarray`. The arm slices the shared `_rgba_array` `(H, W, 4)` `uint8` buffer (the ORA/TIFF arms' own decode) into per-`ChannelID` views; `psapi` constructors and `get_image_data`/`get_channel_by_id` round-trip the same array protocol, so the planar↔channel-dict conversion stays one `numpy` reshape with no provider value object crossing the owner boundary.
- `anyio` (`libs/python/.api/anyio.md`) — `psapi` is off the loader path, so the `_psd` arm rides the page's existing `to_process.run_sync(arm, export, limiter=_GATE)` worker seam (the same `CapacityLimiter`-bounded crossing the `ORA`/`TIFF` arms and `export/indesign#INDESIGN`'s IDML worker use); the GIL-releasing native PSD write parallelizes across the bounded subprocess pool, never on the event loop.
- `expression` (`libs/python/.api/expression.md`) — the arm returns a `LayerFact`; the page's `exported(...)` entry wraps `_exported` in one `async_boundary`, so a `psapi`-raised error (a `ValueError` on a name >255 chars, a mask-size mismatch, an opacity out of range, or a native write failure) folds into the runtime `BoundaryFault` rail through the boundary's `CLASSIFY` table, never an exception escaping the interior. `psapi`'s `ValueError`/`KeyError` surface (documented per constructor and per lookup) is the boundary input, the typed `RuntimeRail[Block[ContentKey]]` the egress.
- `msgspec` (`libs/python/.api/msgspec.md`) — `LayerFact`/`Layer`/`LayerPolicy` are frozen `msgspec.Struct` rows; the arm reads `Layer` fields and threads the produced `LayerFact` onto the frozen owner through `copy.replace`, never mutating a seed, exactly as the sibling arms do.
- `structlog` + `opentelemetry-api` (`libs/python/.api/structlog.md`, `opentelemetry-api.md`) — the page's `@receipted` definition-time weave and the runtime `async_boundary` span own emission and tracing; the `_psd` arm adds no telemetry of its own, contributing the shared `ArtifactReceipt.Preview(key, fact.width, fact.height)` case (the named-document facts — the PSD is a layered preview deliverable, the same shape the `SVG`/`ORA`/`TIFF` arms contribute), the authored-layer count riding the receipt fact.

[INTEGRATION_SCOPE]: folder-tier no-overlap boundary — PSD authority vs the read/codec/TIFF siblings
- rail: `export/layered`

The brief's categorical-best, zero-overlap mandate partitions the PSD/layered concern across four folder packages meeting at decoded RGBA planes or container bytes — `PhotoshopAPI` owns exactly the *author* slice:
- `PhotoshopAPI` — native PSD/PSB author (and full-fidelity read with bit-depth deduction). The `export/layered` `PSD`/`PSB` arms, and any read-then-re-author template flow (read a `.psd`, mutate specific layers/text via the live setters, `invalidate_text_cache`, `write`). This is the system-of-record PSD writer.
- `psd-tools` (`libs/python/artifacts/.api/psd-tools.md`) — pure-Python/abi3 PSD read/inspect + composite, and the `cp315`-present fallback authoring path while `PhotoshopAPI` has no `cp315` wheel (`psd-tools` ships an abi3 + pure-Python wheel importable on this interpreter). On `cp315` the layered arm's `PSD` member binds the `psd-tools` author path; once a `PhotoshopAPI` `cp315` wheel or source build lands, the native writer is the categorical-best owner and the `psd-tools` arm reverts to read/inspect. No two packages own the *same* slice on the *same* interpreter.
- `imagecodecs` (`libs/python/artifacts/.api/imagecodecs.md`) — the PackBits/ZIP channel codec for the *layered-TIFF* container path (`psdtags` writes compressed channel bytes through it); `PhotoshopAPI` owns its own native PSD compression (`Compression.raw`/`rle`/`zip`/`zipprediction`) and never routes channel bytes through `imagecodecs`.
- `psdtags` + `tifffile` (`libs/python/artifacts/.api/psdtags.md`, `tifffile.md`) — Photoshop-compatible layered-TIFF tags + container, the `export/layered` `TIFF` arm. Retained for the TIFF container only; PSD/PSB authority moves to `PhotoshopAPI` per the brief, so the `TIFF` arm is the right owner when the deliverable must be a layered `.tif` (a TIFF-consuming pipeline), the `PSD`/`PSB` arms when it must be a native Photoshop document.

The deleted forms: a second PSD writer admitted beside this one on the same interpreter (the `psd-tools` author path is the `cp315` fallback, not a parallel owner); a `psapi`-`numpy`-array passed straight to `tifffile` (the TIFF arm owns its own `psdtags.PsdLayer` lowering); a raw `LayeredFile`/`ImageLayer`/`ChannelID` schema name crossing the `export/layered` owner boundary (downstream receives only `LayerFact`, `ContentKey`, and `ArtifactReceipt`); an in-process `psapi` call on the event loop (the arm is a `Band.WORKER` `to_process` crossing); a per-bit-depth arm family (`_psd_8`/`_psd_16`/`_psd_32`) where one arm selects the specialization off the plane dtype; a `bit_depth=` knob on `exported` where the input value discriminates; and an ICC color *conversion* attempted through `LayeredFile.icc` (it is an interpretation hint only — real device-link/proof conversion stays Pillow `ImageCms`/lcms2, the `graphic/color/managed` egress owner).

## [05]-[NOTES]

- The `.pyi` stub at `_layered_file.pyi` names the flat-layers property `layers_flat`, but the runtime `pybind11` declaration (`DeclareLayeredFile.h`) and the class docstring bind and document it as `flat_layers` — the stub has drifted; the bound runtime name is `flat_layers`. Re-confirm against the installed dist once a `cp315` wheel is provisioned (`uv run --frozen python -m tools.assay api resolve photoshopapi` → reflect the `LayeredFile_8bit` symbol).
- `write` consumes the document via move semantics; the instance is invalid afterward and reusing it is undefined behavior. The `_psd` arm constructs one `LayeredFile` per `_emit`, writes once, reads the bytes back, and discards — never re-uses a written document.
- `LayeredFile.write` targets a filesystem path, not an in-memory buffer (unlike the page's other arms that return bytes directly). The `_psd` arm writes to a scratch path on the `to_process` worker and reads the bytes back into the `LayerFact`; the scratch file is worker-local and reaped with the subprocess.
- The fixed-`width`/`height` per-channel-plane contract: every channel plane (and the optional mask) must be exactly `width * height`; `ImageLayer` raises `ValueError` on a size mismatch, a name >255 chars, a negative dimension, or an opacity outside range — all caught by the page's `async_boundary` `CLASSIFY` table, so the page's `ExportFault` vocabulary (`empty`/`duplicate`/`payload`) stays the admission-side faults and the native raises stay boundary-classified.
- The `ChannelID`-keyed constructor is the preferred (explicit, lossless) form; the docs flag the bare logical-index dict and the enum-dict as having had construction edge cases historically, so the arm uses the `ChannelID`-keyed dict with all color-mode channels present (R,G,B,A for RGB; C,M,Y,K,A for CMYK; Gray,A for grayscale).
