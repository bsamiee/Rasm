# [PY_ARTIFACTS_API_PHOTOSHOPAPI]

`PhotoshopAPI` owns native PSD/PSB read and write for the `export/layered` rail: a bit-depth-templated `LayeredFile` holding a nested layer hierarchy over per-channel `numpy` planes, closed on a `write` emitting a faithful `.psd`/`.psb`. Its layered arm folds each placed `Layer` into one `psapi.ImageLayer_<bit>` over a `ChannelID`-keyed channel dict, writing a real channel-stack document. Four packages partition the concern at RGBA planes and container bytes — `PhotoshopAPI` authors, `psd-tools` reads, `imagecodecs` the channel codec, `psdtags`+`tifffile` the layered-TIFF — downstream receiving only `LayerFact` bytes, a `ContentKey`, and the shared `ArtifactReceipt.Preview`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `photoshopapi`
- package: `PhotoshopAPI` (`BSD-3-Clause`, Emil Dohne)
- module: `photoshopapi` (alias `psapi`)
- namespaces: `psapi.enum`
- abi: `pybind11` C++20 native extension, off the runtime loader path
- rail: `export/layered`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and bit-depth specializations

`LayeredFile.read` deduces on-disk depth and returns the matching `LayeredFile_<bit>`; the three roots fix channel dtype through the template parameter `T` and share one document surface — hierarchy, ICC/DPI/dims, layer `add`/`move`/`remove`/`find`, `read`/`write`. Construction picks the specialization off the source `Layer` channel dtype, never a caller `bit_depth` knob, and never re-templates afterward.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                           |
| :-----: | :------------------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `LayeredFile`       | read dispatcher | `read(path)` deduces depth → `_8bit`/`_16bit`/`_32bit` |
|  [02]   | `LayeredFile_8bit`  | document root   | `uint8` channels                                       |
|  [03]   | `LayeredFile_16bit` | document root   | `uint16` channels                                      |
|  [04]   | `LayeredFile_32bit` | document root   | `float32` channels                                     |

[PUBLIC_TYPE_SCOPE]: layer node family

`Layer_<bit>` is the abstract node base carrying every editor-panel attribute; the four leaf and group subclasses derive from it. A `layers` walk (root or group-scoped) yields `Layer_<bit>` references the consumer narrows with `isinstance`; the layered arm constructs the leaf subclass directly.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                                                    |
| :-----: | :----------------------- | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Layer_<bit>`            | node base         | name/blend/opacity/fill/visibility/lock/clipping/display-color + full mask axis |
|  [02]   | `ImageLayer_<bit>`       | raster leaf       | per-channel `numpy` author/extract; the layered arm's primary node              |
|  [03]   | `GroupLayer_<bit>`       | hierarchy node    | nested `layers`, `is_collapsed`, group-scoped `add_layer`/`remove_layer`        |
|  [04]   | `SmartObjectLayer_<bit>` | smart-object leaf | embedded/external linked image + affine transform, warp, `replace`              |
|  [05]   | `TextLayer_<bit>`        | text leaf         | live editable text layer, re-rendered via `invalidate_text_cache`               |

[PUBLIC_TYPE_SCOPE]: `psapi.enum` bounded vocabularies (all `enum`)

`psapi.enum` owns the closed discriminant vocabularies the layered arm derives onto by member value or name — `BlendMode` by hyphen-stripped value, `Compression` by underscore-stripped lowered name — never re-minted as local enums. `ChannelID.mask` is the user-supplied layer-mask slot (logical index `-2`), distinct from `ChannelID.alpha` (`-1`). Each family's members carry below the grid.

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                                            |
| :-----: | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `BlendMode`       | `Layer.blend_mode` and layer constructor `blend_mode=`                  |
|  [02]   | `ColorMode`       | `LayeredFile_<bit>(color_mode, …)` and layer `color_mode=`              |
|  [03]   | `BitDepth`        | `LayeredFile.bit_depth`, read-only, fixed by `T`                        |
|  [04]   | `ChannelID`       | image-layer channel dict keys; `get_channel_by_id`                      |
|  [05]   | `Compression`     | layer `compression=`, `LayeredFile.compression`, `set_mask_compression` |
|  [06]   | `LayerColor`      | `Layer.display_color` layer-panel swatch                                |
|  [07]   | `LinkedLayerType` | `SmartObjectLayer.linkage`, smart-object `link_type=`                   |

- [01]-[BLENDMODE]: `passthrough`(groups only)/`normal`/`dissolve`/`darken`/`multiply`/`colorburn`/`linearburn`/`darkercolor`/`lighten`/`screen`/`colordodge`/`lineardodge`/`lightercolor`/`overlay`/`softlight`/`hardlight`/`vividlight`/`linearlight`/`pinlight`/`hardmix`/`difference`/`exclusion`/`subtract`/`divide`/`hue`/`saturation`/`color`/`luminosity`.
- [02]-[COLORMODE]: `rgb` (R,G,B,A) / `cmyk` (C,M,Y,K,A) / `grayscale` (Gray,A).
- [03]-[BITDEPTH]: `bd_8` (`uint8`) / `bd_16` (`uint16`) / `bd_32` (`float32`).
- [04]-[CHANNELID]: `red`/`green`/`blue`/`cyan`/`magenta`/`yellow`/`black`/`gray`/`custom`/`alpha`(−1)/`mask`(−2).
- [05]-[COMPRESSION]: `raw` (none) / `rle` (fast, low ratio) / `zip` (deflate) / `zipprediction` (deflate + per-scanline delta, best ratio).
- [06]-[LAYERCOLOR]: `none`/`red`/`orange`/`yellow`/`green`/`blue`/`violet`/`gray`/`seafoam`/`indigo`/`magenta`/`fuschia`.
- [07]-[LINKEDLAYERTYPE]: `data` (image stored inside the PSD) / `external` (image shipped alongside on disk).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construct, read, write

`LayeredFile_<bit>` constructs empty from `(color_mode, width, height)` or via the deducing static `LayeredFile.read`; `write` consumes the document by move semantics — the instance is dead afterward — and emits `.psd`/`.psb` with implicit cross-format promotion driven by the path suffix. Dimensions cap at 30,000 (PSD) / 300,000 (PSB).

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `LayeredFile.read(path) -> LayeredFile_<bit>` | static   | open, deduce bit depth                          |
|  [02]   | `LayeredFile_8bit.read(path)`                 | static   | open a known-depth document                     |
|  [03]   | `LayeredFile_8bit(color_mode, width, height)` | ctor     | construct empty                                 |
|  [04]   | `write(path, force_overwrite=True)`           | instance | serialize `.psd`/`.psb`; instance dead after    |
|  [05]   | `invalidate_text_cache() -> None`             | instance | strip `Txt2` + dirty text for re-render on open |

[ENTRYPOINT_SCOPE]: layer placement and lookup

A layer places at root via `LayeredFile.add_layer` or inside a group via `GroupLayer.add_layer(layered_file, layer)`, the document arg driving the duplicate-placement check. Lookup discriminates on input shape: `__getitem__` for a single root-level name (chainable through nested groups), `find_layer` for a `"Group/Sub/Leaf"` path; both raise `KeyError`/`ValueError` on miss.

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `add_layer(layer) -> None`            | instance | place a layer at scene root                        |
|  [02]   | `GroupLayer.add_layer(file, layer)`   | instance | place inside a group; doc arg drives no-double-add |
|  [03]   | `file[name] -> Layer_<bit>`           | instance | root-name lookup, chainable `file["G"]["Img"]`     |
|  [04]   | `find_layer(path) -> Layer_<bit>`     | instance | path lookup `"Group/Nested/Leaf"`                  |
|  [05]   | `move_layer(child, parent=None)`      | instance | reparent (`None` → root); object or path pair      |
|  [06]   | `remove_layer(layer)`                 | instance | drop from root by object or name                   |
|  [07]   | `is_layer_in_document(layer) -> bool` | instance | membership probe at any nesting depth              |

[ENTRYPOINT_SCOPE]: `ImageLayer` construction and channel I/O

`ImageLayer_<bit>` is the layered arm's primary node; its constructor overloads on `image_data` shape — a `numpy.ndarray` (planar `(C,H,W)` or interleaved `(C,H*W)`), an `int`-keyed channel dict (logical `0/1/2`=R/G/B, `-1`=alpha, `-2`=mask), or a `ChannelID`-keyed dict (the explicit, lossless form the arm uses). `width`/`height` are required when constructing from data, and opacity is `0.0–1.0` (mapped to on-disk `0–255`).

- carry: `layer_name`, `layer_mask=None`, `width`, `height`, `blend_mode`, `pos_x=0`, `pos_y=0`, `opacity=1.0`, `compression`, `color_mode`, `is_visible=True`, `is_locked=False` — the shared attribute tail; rows [01]-[03] differ only in `image_data`.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `(image_data: ndarray, …)`                      | ctor     | construct from a planar/interleaved array   |
|  [02]   | `(image_data: dict[int], …)`                    | ctor     | index-keyed channel planes                  |
|  [03]   | `(image_data: dict[ChannelID], …)`              | ctor     | explicit `ChannelID`-keyed planes, lossless |
|  [04]   | `get_image_data() -> dict[int, ndarray]`        | instance | extract all channels incl. mask             |
|  [05]   | `set_image_data(data, width=None, height=None)` | instance | replace channel data in place               |
|  [06]   | `get_channel_by_id(key: ChannelID)`             | instance | one channel by semantic id (`mask` → mask)  |
|  [07]   | `get_channel_by_index(key)`                     | instance | one channel by logical index (also `[i]`)   |
|  [08]   | `channel_indices() -> list[int]`                | instance | the present logical channel indices         |

[ENTRYPOINT_SCOPE]: `GroupLayer` and `SmartObjectLayer` construction

`GroupLayer_<bit>` nests a layer collection, its `width`/`height`/mask kwargs mattering only when the group carries a mask. `SmartObjectLayer_<bit>` embeds or externally links an image as a non-destructive smart object with an affine transform + warp, taking the owning `LayeredFile` and source `path`. Both share the `(…, blend_mode, opacity=1.0, compression, color_mode, is_visible=True, is_locked=False)` attribute tail; the smart-object transform and source families carry as keyed rows below.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `GroupLayer_8bit(layer_name, width, height)` | ctor     | empty group; default blend `passthrough`          |
|  [02]   | `is_collapsed`                               | property | layer-panel folded state                          |
|  [03]   | `remove_layer(index \| layer \| name)`       | instance | drop a child, polymorphic on input                |
|  [04]   | `SmartObjectLayer_8bit(file, path, name)`    | ctor     | embed/link a smart object (`link_type=`)          |
|  [05]   | `replace(path, link_externally=False)`       | instance | swap the backing image, preserving warp/transform |

- [06]-[SMARTOBJECTLAYER_TRANSFORM]: `move`/`rotate`/`scale`/`transform(matrix)`/`reset_transform`/`reset_warp` — non-destructive affine + warp.
- [07]-[SMARTOBJECTLAYER_SOURCE]: `get_original_image_data`/`original_width`/`original_height`/`filename`/`filepath`/`hash` — inspect the pre-transform original.

[ENTRYPOINT_SCOPE]: document and layer properties

Document-level properties carry in the grid (all on `LayeredFile_<bit>`); the `Layer_<bit>` editor-panel attribute axis and the layer-mask parameter set carry as keyed rows below.

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :---------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `layers -> list[Layer_<bit>]` | property | root-level layer list (read-only)                   |
|  [02]   | `flat_layers`                 | property | flatten of every layer for one-shot iteration       |
|  [03]   | `icc -> numpy.ndarray`        | property | attach ICC interpretation hint; setter ndarray/path |
|  [04]   | `compression`                 | property | write-only; set every layer's write compression     |
|  [05]   | `dpi` / `width` / `height`    | property | resolution + canvas dims (PSD ≤30k, PSB ≤300k)      |
|  [06]   | `num_channels` / `bit_depth`  | property | channel count (mask excluded) + fixed depth         |

- [07]-[LAYER_ATTRIBUTES]: `name`/`blend_mode`/`opacity`/`fill`/`is_visible`/`is_locked`/`clipping_mask`/`display_color`/`center_x`/`center_y`/`width`/`height` — the editor-panel attribute axis, read/write.
- [08]-[LAYER_MASK]: `mask`/`has_mask`/`mask_disabled`/`mask_default_color`/`mask_density`/`mask_feather`/`mask_position`/`mask_relative_to_layer`/`mask_width()`/`mask_height()`/`set_mask_compression` — the mask channel and its parameter set.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One polymorphic `_psd` arm selects the `LayeredFile_<bit>`/`ImageLayer_<bit>`/`GroupLayer_<bit>` specialization off the decoded plane dtype (`uint8`→8, `uint16`→16, `float32`→32), never a caller `bit_depth` knob and never a per-depth `_psd_8`/`_16`/`_32` arm family.
- `ChannelID`-keyed channel dicts are the construction form, every color-mode channel present (R,G,B,A for RGB; C,M,Y,K,A for CMYK; Gray,A for grayscale); the bare int-index dict is the lossy fallback.
- Every channel plane and the optional mask is exactly `width * height`; `ImageLayer` raises `ValueError` on a size mismatch, a name over 255 chars, a negative dimension, or an opacity out of range.
- `write` consumes the document by move semantics and targets a filesystem path, not a buffer — the arm writes to a worker-local scratch path, reads the bytes back, and discards; the written instance is never reused.
- Blend and compression derive by shared member correspondence, never a parallel table: the page's CSS `BlendMode` maps onto `psapi.enum.BlendMode` by hyphen-stripped value, the `PsdCompression` policy onto `psapi.enum.Compression` by underscore-stripped lowered name, so each native member grows reachable the moment the page's vocabulary adds it.

[STACKING]:
- `numpy`(`.api/numpy.md`): channel planes are `numpy.ndarray`; the arm slices the shared `(H,W,4)` RGBA buffer into per-`ChannelID` views, and `psapi` constructors + `get_image_data` round-trip the same array protocol, so planar↔channel-dict stays one reshape with no provider value object crossing the boundary.
- `psd-tools`(`.api/psd-tools.md`): the arm reopens the `PhotoshopAPI` output through `PSDImage.open` and proves every authored layer stays addressable via `find` — structural readback evidence, never a second author.
- `imagecodecs`(`.api/imagecodecs.md`) / `psdtags`+`tifffile`(`.api/psdtags.md`, `.api/tifffile.md`): the layered-TIFF arm's owners; `PhotoshopAPI` uses its own native `Compression` (`raw`/`rle`/`zip`/`zipprediction`) and never routes channel bytes through them.
- `anyio`(`.api/anyio.md`): `psapi` is off the loader path, so the `_psd` arm rides the page's lane seam as a `KernelTrait.HOSTILE` kernel; the GIL-releasing native write parallelizes across the bounded subprocess pool, never the event loop.
- `expression`(`.api/expression.md`): the lane boundary converts a `psapi`-raised `ValueError`/`KeyError` into the runtime `BoundaryFault` rail, returning a typed `RuntimeRail[ArtifactReceipt]`, never an exception escaping the interior.
- `msgspec`(`.api/msgspec.md`): `Layer`/`LayerPolicy` are frozen `Struct` rows and `LayerFact` the closed preview `tagged_union`; the arm reads `Layer` fields and returns one `LayerFact.preview`, never mutating a seed.
- `structlog`+`opentelemetry-api`(`.api/structlog.md`, `.api/opentelemetry-api.md`): the lane's boundary span owns emission and tracing; the arm contributes the shared `ArtifactReceipt.Preview` case with the authored-layer count and target, adding no telemetry of its own.
- `export/layered`: `PSD` and `PSB` `ExportTarget` members bind to the `_psd` `LayerEngine`, folding the page's placed `tuple[Layer, ...]` — decoded to RGBA planes, the same raster the `ORA`/`TIFF` arms consume — into one native `psapi.ImageLayer_<bit>` per row and one `GroupLayer` per distinct `group`.

[LOCAL_ADMISSION]:
- `psapi` is a native `pybind11` extension off the runtime loader path — imported at boundary scope only, reified on first `_psd` use in the process worker.
- `PhotoshopAPI` is the sole PSD/PSB author on the interpreter; where its native core is absent, `psd-tools` authors as the fallback path, never a second parallel writer beside it.

[RAIL_LAW]:
- Package: `PhotoshopAPI`
- Owns: native PSD/PSB author and full-fidelity read with bit-depth deduction for the `export/layered` rail — the `LayeredFile_<bit>` document, the layer-node family, per-channel `numpy` I/O keyed by `ChannelID`, the full editor-panel attribute and layer-mask axis, native `Compression`, and the read-then-re-author template flow.
- Accept: one polymorphic arm selecting the bit-depth specialization off the plane dtype; `ChannelID`-keyed channel dicts with every color-mode channel present; blend and compression derived by shared member correspondence; a `HOSTILE` process crossing for the native write; structural readback through `psd-tools`; downstream receiving only `LayerFact`/`ContentKey`/`ArtifactReceipt`.
- Reject: a second PSD writer beside this one on the interpreter; a `bit_depth=` knob or a per-depth arm family where the plane dtype discriminates; a raw `LayeredFile`/`ImageLayer`/`ChannelID` schema name crossing the owner boundary; an in-process `psapi` call on the event loop; an ICC color conversion through `LayeredFile.icc`, an interpretation hint only where device-link and proof conversion stay `graphic/color/managed`; local `BlendMode`/`Compression` twins where the native enums own the vocabulary.
