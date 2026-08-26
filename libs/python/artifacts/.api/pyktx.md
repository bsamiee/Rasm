# [PY_ARTIFACTS_API_PYKTX]

`pyktx` owns the IN-PROCESS KTX2 container — the GPU-texture file the repo's texture egress ships to a web or desktop consumer. It authors a `KtxTexture2` from a create-info record, fills each mip level, array layer, and cubemap face from raw bytes, encodes the payload to UASTC or ETC1S through Basis Universal, supercompresses with Zstd (`deflate_zstd` is the ONE deflate member — `ZLIB` exists only as a readable `KtxSupercmpScheme` row on ingest), transcodes a Basis payload down to BC1/BC3/BC4/BC5/BC7, ETC2, ASTC, PVRTC, or an uncompressed row, and writes to a file or to `bytes`. It is a `cffi` binding over the SAME `libktx` the provisioned `ktx` CLI ships, so the two legs agree byte-for-byte on the container.

CLI encode is the FLOOR and this binding the acceleration row: a plane that must be encoded where no toolchain is provisioned refuses at the tool probe, while a plane encoded here saves the process spawn and the intermediate file. Neither leg reads or writes pixel FORMATS — a plane arrives as raw bytes already at its declared `VkFormat`, produced by `imagecodecs` or `pyvips`, and block-decode read-back for verification is the `imagecodecs` `bcn_decode`/`dds_decode` pair.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the texture roots and their parameter records
- concern: raster

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `KtxTexture2`             | container root | create, fill, encode, supercompress, transcode, write    |
|  [02]   | `KtxTexture1`             | container root | the legacy KTX1 document keyed by `GlInternalformat`     |
|  [03]   | `KtxTextureCreateInfo`    | create record  | format, extent, dimensions, levels, layers, faces        |
|  [04]   | `KtxBasisParams`          | encode policy  | 24 fields: `uastc`, quality, RDO, `normal_map`, swizzle  |
|  [05]   | `KtxAstcParams`           | encode policy  | block dimension, mode, quality, `normal_map`, perceptual |
|  [06]   | `KtxHashList`             | metadata       | the key-value block, read as `kv_data` or `kv_data_raw`  |
|  [07]   | `KtxError`                | fault          | carries the failing entry point and its `KtxErrorCode`   |
|  [08]   | `KtxVersionMismatchError` | fault          | declared; no import-time `libktx` version check runs     |

[PUBLIC_TYPE_SCOPE]: the closed policy vocabularies
- concern: raster

| [INDEX] | [ENUM]                      | [ROWS]                                                                                     |
| :-----: | :-------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `VkFormat`                  | 230 Vulkan formats — the ONE format vocabulary a KTX2 texture declares                     |
|  [02]   | `GlInternalformat`          | the KTX1 format vocabulary; KTX2 takes `gl_internal_format=None`                           |
|  [03]   | `KtxTextureCreateStorage`   | `NO` and `ALLOC` — whether `create` allocates the image store                              |
|  [04]   | `KtxTextureCreateFlagBits`  | `NO_FLAGS` `LOAD_IMAGE_DATA_BIT` `RAW_KVDATA_BIT` `SKIP_KVDATA_BIT`                        |
|  [05]   | `KtxSupercmpScheme`         | `NONE` `BASIS_LZ` `ZSTD` `ZLIB`                                                            |
|  [06]   | `KtxTranscodeFmt`           | the BC1/BC3/BC4/BC5/BC7 set, ETC1/ETC2/EAC, `ASTC_4x4_RGBA`, PVRTC, uncompressed fallbacks |
|  [07]   | `KtxTranscodeFlagBits`      | PVRTC power-of-two decode, opaque-alpha transcode, `HIGH_QUALITY`                          |
|  [08]   | `KtxPackUastcFlagBits`      | `FASTEST` through `VERY_SLOW`, two error-favor bits, ETC1 hints                            |
|  [09]   | `KtxPackAstcQualityLevels`  | `FASTEST` `FAST` `MEDIUM` `THOROUGH` `EXHAUSTIVE`                                          |
|  [10]   | `KtxPackAstcBlockDimension` | 24 block shapes from `4x4` through the 3-D rows                                            |
|  [11]   | `KtxPackAstcEncoderMode`    | `DEFAULT` `LDR` `HDR`                                                                      |
|  [12]   | `KtxErrorCode`              | 21 rows including `TRANSCODE_FAILED` and `LIBRARY_NOT_LINKED`                              |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container lifecycle
- concern: raster
- [SHAPE]: instance (`KtxTexture2`; `create` and `create_from_named_file` are its statics)

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `KtxTexture2.create(create_info, storage_allocation)`            | author an empty texture; `ALLOC` reserves the whole store  |
|  [02]   | `KtxTexture2.create_from_named_file(filename, create_flags=…)`   | read; the flag decides whether pixel and raw KV data load  |
|  [03]   | `set_image_from_memory(level, layer, face_slice, data: bytes)`   | place one image; the three coordinates address the payload |
|  [04]   | `write_to_named_file(dst_name) -> None`                          | serialize to disk                                          |
|  [05]   | `write_to_memory() -> bytes` and `write_to_native_memory()`      | serialize to a buffer                                      |
|  [06]   | `image_offset(level, layer, face_slice)` and `image_size(level)` | the payload geometry a reader slices with                  |
|  [07]   | `data() -> buffer` and `row_pitch(level) -> int`                 | the raw store and its stride — METHODS, not properties     |
|  [08]   | `data_size`, `data_size_uncompressed`                            | the store's measures — properties, unlike `data` itself    |

- `data` AND `row_pitch` ARE METHODS while `data_size`/`data_size_uncompressed` are properties (measured, both declared on the `KtxTexture` base): `texture.data` answers the bound method itself and `bytes(texture.data)` raises `TypeError: cannot convert 'method' object to bytes`, so the store reads `bytes(texture.data())`. The two shapes sit adjacent in one geometry block and neither spelling transfers to the other.
- `num_layers` does NOT exist as a read-back member — it is a `KtxTextureCreateInfo` field alone, and an attribute read raises `AttributeError`.

[ENTRYPOINT_SCOPE]: encode, supercompress, and transcode
- concern: raster
- [SHAPE]: instance (`KtxTexture2`, mutating in place)

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `compress_basis(params) -> None`                    | Basis encode; `uastc=True` is UASTC, `False` is ETC1S/BasisLZ |
|  [02]   | `compress_astc(params) -> None`                     | ASTC encode at the chosen block dimension and quality level   |
|  [03]   | `deflate_zstd(compression_level: int) -> None`      | Zstd supercompression over the encoded payload                |
|  [04]   | `transcode_basis(output_format, transcode_flags=0)` | Basis payload down to a block or uncompressed target format   |
|  [05]   | `needs_transcoding -> bool`                         | the ONLY correct transcode predicate                          |
|  [06]   | `supercompression_scheme -> KtxSupercmpScheme`      | the scheme in force on the in-memory texture                  |
|  [07]   | `vk_format`, `element_size`, `oetf`                 | format, texel size, and transfer-function tag                 |

[ENTRYPOINT_SCOPE]: geometry and metadata
- concern: raster
- [SHAPE]: instance (`KtxTexture2` properties)

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `base_width`, `base_height`, `base_depth`, `num_dimensions` | level-0 extent and dimensionality                             |
|  [02]   | `num_levels`, `num_faces`, `is_array`, `is_cubemap`         | pyramid depth and the layer and face layout                   |
|  [03]   | `is_compressed`, `premultipled_alpha`                       | block-compression state and the alpha association flag        |
|  [04]   | `kv_data -> KtxHashList` and `kv_data_raw`                  | the key-value metadata block; `kv_data_raw` reads `None`      |
|  [05]   | `generate_mipmaps`                                          | the create-info flag as read back; an upload hint, not a fold |

- `KtxHashList` IS NOT A MAPPING: it carries `add_kv_pair(key: str, value: bytes) -> None`, `find_value(key: str)` answering the buffer or `None`, `delete_kv_pair(key: str) -> None`, and `copy() -> Dict[str, bytes]`, and nothing else. Subscript assignment raises `TypeError: 'KtxHashList' object does not support item assignment` and no `__getitem__`/`__setitem__`/`__iter__` exists, so a mapping-shaped write against the property is a runtime fault the property's own name invites. A pair written with `add_kv_pair` round-trips a `write_to_named_file`/`create_from_named_file` crossing and reads back through `find_value`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Containers are authored, never converted: `KtxTexture2.create(KtxTextureCreateInfo(...), ALLOC)` reserves the whole store from `num_levels` times `num_layers` times `num_faces`, then one `set_image_from_memory(level, layer, face_slice, data)` per image fills it. KTX2 create-info carries `gl_internal_format=None` — `vk_format` is the format vocabulary, and supplying a GL enum instead is the KTX1 shape.
- MIP LEVELS ARE CALLER-BUILT. `generate_mipmaps` is a create-info flag recorded on the file for the upload path; this binding folds no pyramid. Each level's plane is resampled upstream in the `pyvips` float lane under the channel's own mip policy and placed with its own `set_image_from_memory` call, level extents halving and clamping at 1.
- `transcode_basis` REFUSES on a supercompressed IN-MEMORY texture: `compress_basis` then `deflate_zstd` then `transcode_basis` raises `KtxError(TRANSCODE_FAILED)`. That deflated payload must round-trip through `write_to_named_file` and `create_from_named_file`, whose load inflates it — the reloaded texture reports `supercompression_scheme` back at `NONE`, `needs_transcoding` still `True`, and transcodes clean. Encoding and transcoding in one process therefore cross the file, or skip `deflate_zstd` entirely when the transcode is the next step.
- Branch on `needs_transcoding`, NEVER on `vk_format`: a Basis-encoded texture reads `vk_format` as `0` (`VK_FORMAT_UNDEFINED`) until transcode, so a reader keyed on the format classes every wire-legal supercompressed payload as malformed. After `transcode_basis(BC7_RGBA)` the format resolves to `VK_FORMAT_BC7_SRGB_BLOCK` and `needs_transcoding` clears.
- `compress_basis`, `compress_astc`, `deflate_zstd`, and `transcode_basis` MUTATE the texture in place and return `None`; each is a one-way state move, and a second encode over an encoded payload is not a re-encode.
- BLOCK ENCODE IS 8-BIT-INPUT-ONLY (measured, both legs): `compress_basis` raises `KtxError(INVALID_OPERATION)` and `compress_astc` raises `KtxError(UNSUPPORTED_FEATURE)` — `ktxTexture2_compressAstcEx returned with 17` — on a u16/f16/f32 store, and `KtxPackAstcEncoderMode.HDR` is accepted-and-inert at 8-bit input, so the enum member proves a parameter and never a code path. The store depth is the gate, not the block dimension or the quality level: a caller resolves the payload class from the depth BEFORE the encode, because every one of the 24 block shapes refuses the same deep texture.
- THE DEEP ROUTE IS THE UNCOMPRESSED `VkFormat` UNDER `deflate_zstd`, and it is settled rather than a stand-in for a float block encoder that does not exist. Proven on the FILE BYTES: an `R16G16B16A16_SFLOAT` texture created with `ALLOC`, filled by `set_image_from_memory`, deflated at level 9 and written writes a header reading `vkFormat` 97 and `supercompressionScheme` 2 (`KtxSupercmpScheme.ZSTD`); `ktx validate` passes; and `create_from_named_file` recovers the float16 payload EXACTLY. Zstd supercompression therefore carries the deep store where block compression carries the 8-bit one, and the two are not alternatives at one depth. The reloaded texture reads `supercompression_scheme` back at `NONE` because the load INFLATES — the in-memory property answers the live state, the header answers what the file holds, and only the header is the container's claim.
- BC6H IS NOT AN ESCAPE from the deep route: no BCn encoder ships here or at the read-back leg, where `imagecodecs` declares `bcn_encode` and raises `NotImplementedError` from it. A float block payload is unreachable across the whole repo, so the uncompressed deep store is the only HDR container either leg writes.
- Encode policy is a RECORD, never call arguments: `KtxBasisParams` carries 24 fields and `KtxAstcParams` eight, so a per-channel quality policy is a stored parameter row selected by channel role — `normal_map=True` with RDO disabled for vector channels, the default quality for color. Bare `int` is accepted in place of either record and is the untyped form.
- Every fault is `KtxError` carrying the failing `libktx` entry point and a `KtxErrorCode`; the boundary adapter maps that one family onto the repo's typed fault. `KtxVersionMismatchError` exists but never fires — `LIBKTX_VERSION` is consumed at BUILD time and no import-time version check runs, so the linked library version is proven from the store path, never from the module.
- This extension links `libktx` by ABSOLUTE store path, so the `forge-scientific-env` wrapper is a build-time input alone; a runtime process needs neither the wrapper nor the `LIBKTX_*` variables in its environment.

[STACKING]:
- provisioned `ktx` CLI: the two legs are ONE `libktx` and agree on the container. That CLI, carrying `create`, `encode`, `transcode`, `deflate`, `extract`, `info`, `validate`, and `compare`, is the encode floor every branch spawns; this binding is the in-process acceleration row that skips the spawn and the intermediate file. The store-depth bound holds on that leg too: `--encode` admits the `R8*_UNORM`/`R8*_SRGB` rows alone, and `ktx create --format ASTC_6x6_SFLOAT_BLOCK` refuses a non-raw input as an unsupported create format — so neither leg reaches a float ASTC payload and the deep route is the same uncompressed store on both. Provisioning evidence asserts binary presence and the subcommand roster — every `ktx` binary prints `GIT-NOTFOUND` for `--version`, so version text proves nothing and the nixpkgs attribute is the version truth.
- `imagecodecs`(`.api/imagecodecs.md`): the read-back leg. `bcn_decode(data, format, shape=…)` and `dds_decode` decode a transcoded block payload to a `numpy` plane, so a verify pass proves the block bytes without a second encoder. This owner encodes no pixel format and decodes no image; `bcn_encode`/`dds_encode` are unimplemented there for the same reason.
- `pyvips`(`.api/pyvips.md`): every mip level is resampled upstream in the float lane before placement, because this binding folds no pyramid. Each per-level plane then casts to its declared depth and enters as raw bytes.
- `numpy`(`.api/numpy.md`): images cross as `bytes`, so a plane reaches `set_image_from_memory` through `np.ascontiguousarray(plane).tobytes()` at exactly the `VkFormat` texel layout; a strided view or a mismatched dtype writes a silently wrong texture, since no shape check exists at the boundary.
- `msgspec`(`.api/msgspec.md`): the create-info and the encode-parameter rows are typed structs keyed by channel role, so the whole texture policy is content-keyable and a quality knob is a stored row rather than a call argument.
- `beartype`(`.api/beartype.md`): the boundary contract carries the per-image `bytes` shape and the level, layer, and face coordinate bounds, so an out-of-range coordinate rejects at the contract instead of corrupting the store.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each texture write stamps `vk_format`, `num_levels`, `num_faces`, `is_array`, the supercompression scheme, `data_size` against `data_size_uncompressed`, and the encode parameter row on the owning span.
- `anyio`(`.api/anyio.md`): Basis and ASTC encoding are long CPU-bound native calls that release the GIL and carry their own thread count; a set-wide encode crosses the runtime `HOSTILE` worker arm rather than the loop thread.

[LOCAL_ADMISSION]:
- `import pyktx` at boundary scope only, behind the `lazy import` proxy the native container owners use.
- `KtxTexture1` and `GlInternalformat` are the legacy GL-keyed container; the repo authors KTX2 alone, and a KTX1 file admits only for ingest.
