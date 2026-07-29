# [TS_DATA_API_KTX_PARSE]

`ktx-parse` reads and writes the KTX 2.0 container as plain data: `read(bytes)` unpacks a `KTX2Container` of mip levels, a Khronos Data Format Descriptor, and key/value metadata, and `write(container, options?)` repacks it. It transcodes nothing, decodes no texel, and touches no GPU — the payload class, transfer function, alpha association, and mip depth of a delivered `.ktx2` all read out of the header on a plain byte plane.

Encoding stays outside: the provisioned `ktx` CLI mints every KTX2 the branch serves, and this package gates what that CLI produced — classifying and validating it before the object store admits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ktx-parse`
- package: `ktx-parse` (MIT)
- module: `exports` condition-selects `dist/ktx-parse.cjs` for `require` against the `dist/ktx-parse.modern.js` default; types at `dist/index.d.ts`
- runtime: both lanes — pure TypeScript over `Uint8Array`, no native binding and no wasm, so the browser lane parses the same bytes the server admits
- rail: `object` byte-plane classification, lifted into the `Effect` rail at the boundary that admits a delivered texture container
- boundary: read and write ONLY; `KHR_SUPERCOMPRESSION_ZSTD` payloads stay compressed in `levels[].levelData`, and no supercompression codec ships here

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the container and its nested records

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]    | [CAPABILITY]                                 |
| :-----: | :------------------------------------ | :--------------- | :------------------------------------------- |
|  [01]   | `KTX2Container`                       | container record | the whole unpacked file as a plain object    |
|  [02]   | `KTX2Level`                           | level record     | `levelData` bytes + `uncompressedByteLength` |
|  [03]   | `KTX2DataFormatDescriptorBasicFormat` | DFD record       | payload class, transfer, primaries, alpha    |
|  [04]   | `KTX2BasicFormatSample`               | DFD sample       | per-channel bit layout inside a texel block  |
|  [05]   | `KTX2GlobalDataBasisLZ`               | BasisLZ tables   | endpoint/selector/table blobs for ETC1S      |
|  [06]   | `KTX2GlobalDataBasisLZImageDesc`      | BasisLZ slice    | per-image RGB/alpha slice offsets            |

- `KTX2Container` carries `vkFormat`, `typeSize`, `pixelWidth`, `pixelHeight`, `pixelDepth`, `layerCount`, `faceCount`, `levelCount`, `supercompressionScheme`, `levels`, `dataFormatDescriptor`, `keyValue`, and `globalData`.
- `pixelDepth` and `layerCount` are ZERO on a real 2D non-array file, never one — `ktx create` writes 0 for both, and only `faceCount` uses 1 as its non-cube value. Reading either as a ≥1 count multiplies level arithmetic by zero.
- `KTX2DataFormatDescriptorBasicFormat.texelBlockDimension` stores each extent MINUS ONE: a 4×4 block reads `[3, 3, 0, 0]`.
- `globalData` is populated ONLY under `KHR_SUPERCOMPRESSION_BASISLZ` and is `null` for every other scheme.

[PUBLIC_TYPE_SCOPE]: bounded vocabularies, each a union over exported numeric constants

| [INDEX] | [SYMBOL]           | [CONSTANT_FAMILY]                                       | [DISCRIMINATES]                          |
| :-----: | :----------------- | :------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `Supercompression` | `KHR_SUPERCOMPRESSION_{NONE,BASISLZ,ZSTD,ZLIB}`         | how `levelData` is packed                |
|  [02]   | `VKFormat`         | `VK_FORMAT_*`                                           | the uncompressed or block storage format |
|  [03]   | `Transfer`         | `KHR_DF_TRANSFER_*`                                     | the encoded transfer function            |
|  [04]   | `Primaries`        | `KHR_DF_PRIMARIES_*`                                    | the color primaries                      |
|  [05]   | `Channel`          | `KHR_DF_CHANNEL_RGBSDA_*`                               | which channel a DFD sample describes     |
|  [06]   | `SampleDatatype`   | `KHR_DF_SAMPLE_DATATYPE_{FLOAT,SIGNED,EXPONENT,LINEAR}` | sample numeric interpretation            |

`[PAYLOAD_MODEL]: `KHR_DF_MODEL_UASTC` `KHR_DF_MODEL_ETC1S` `KHR_DF_MODEL_ASTC` `KHR_DF_MODEL_ETC1` `KHR_DF_MODEL_ETC2` `KHR_DF_MODEL_RGBSDA` `KHR_DF_MODEL_UNSPECIFIED`` — the `dataFormatDescriptor[0].colorModel` roster; UASTC and ETC1S are the two Basis-transcodable classes.

`[ALPHA_FLAG]: `KHR_DF_FLAG_ALPHA_STRAIGHT` `KHR_DF_FLAG_ALPHA_PREMULTIPLIED`` — the `flags` field carrying the container's alpha association.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the whole surface — three functions

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                         |
| :-----: | :------------------------------------------ | :------ | :----------------------------------- |
|  [01]   | `read(data: Uint8Array) -> KTX2Container`   | static  | unpack a delivered file for classify |
|  [02]   | `write(container, options?) -> Uint8Array`  | static  | repack an edited container           |
|  [03]   | `createDefaultContainer() -> KTX2Container` | factory | an empty container to populate       |

- `write` copies every binary region into the returned array, so the source container is free to mutate or drop afterwards.
- `WriteOptions.keepWriter` is the ONLY option. Left false, `write` overwrites the `KTXwriter` key/value with its own version string; set true, it emits the container's `KTXwriter` verbatim.
- `createDefaultContainer()` returns `levels: []`, and `write` divides by the level-0 block count — a container written before a level lands throws on `levels[0].levelData`.
- `dist/util.js` declares block-geometry and text helpers, and `dist/index.d.ts` re-exports NONE of them: an import of `getBlockDimensionsByVKFormat`, `getBlockCount`, `getBlockByteLength`, `encodeText`, `decodeText`, `concat`, `leastCommonMultiple`, or `getPadding` fails the ESM link with "does not provide an export named". Block geometry derives from `texelBlockDimension` on the DFD instead.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One container value, one read, one write — there is no reader class, no stream, and no partial parse, so a whole `.ktx2` is a bounded buffer by construction and an unbounded object never reaches this package.
- `vkFormat` is NOT the payload discriminant. Every Basis-class file — the only class a web consumer transcodes — reports `VK_FORMAT_UNDEFINED`, so the classifying read is `dataFormatDescriptor[0].colorModel` for the payload class and `supercompressionScheme` for the packing; branching on `vkFormat` classes every wire-legal payload as malformed.
- DFD fields carry the color science every plane declaration must agree with: `transferFunction` against the channel's transfer tag, `flags` against its alpha association, `colorPrimaries` against the working space. Disagreement refuses at admission and never re-tags silently.

[STACKING]:
- `@gltf-transform/core`(`.api/gltf-transform-core.md`): a `Texture` whose `getMimeType()` is `image/ktx2` yields its bytes through `getImage()`, and `read` classifies them without a transcoder; `@gltf-transform/extensions` `KHRTextureBasisu` already depends on this package for the same read, so the gate and the container writer share ONE parser. Core's own `ImageUtils` sniffs KTX2 only after the `KHRTextureBasisu.register()` static installs its impl, while `pixelWidth`/`pixelHeight` read here unconditionally.
- `sharp`(`.api/sharp.md`): `sharp.format` carries `heif` and `jxl` yet no `ktx2` key, so sharp never encodes or decodes this container — the two split cleanly at the file plane, sharp owning the raster codecs and this package the GPU-texture container.
- `effect`(`.api/effect.md`): `read` throws `Missing KTX 2.0 identifier.` on a buffer whose magic fails, so it lifts through `Effect.try` with a tagged fault; classification after that is total over the constant unions and needs no error channel of its own.
- `object/store.md`: classified containers admit under the same `ContentKey` conditional put as every other byte plane, and classification reads bytes already fetched rather than issuing a second GET.
- `object/file.md` `[03]-[CODEC_GATE]`: the sharp `_GATE` refuses a row whose terminal libvips lacks, and this package is the same refusal shape for the container class libvips does not carry — capability proven by the header, never assumed from a suffix.

[LOCAL_ADMISSION]:
- Encode is NOT this package: the provisioned `ktx` CLI mints every KTX2 the branch serves, and `write` exists to repack a container this branch already owns — a KTX2 assembled level-by-level from raw block data here forks the encoder.
- Repacking a content-addressed container demands `keepWriter: true`. Default `write` rewrites `KTXwriter` and shortens the file, so `write(read(bytes))` answers a DIFFERENT digest than the CLI produced and silently re-keys an immutable object.
- Wire legality is a payload-class read, not a suffix read: `colorModel` UASTC or ETC1S admits to a web consumer, and a container reporting a BC block `vkFormat` is a desktop-native payload the branch's own transcoder path cannot consume.
- Mip depth is `levelCount`, and `levelCount === 0` declares a base-level-only file whose pyramid the loader generates — distinct from `levelCount === 1`, which declares that no other level is meant to exist.

[RAIL_LAW]:
- Package: `ktx-parse`
- Owns: KTX 2.0 container read and write as plain data — the `KTX2Container` record, its Data Format Descriptor, BasisLZ global data, key/value metadata, and the Khronos constant vocabularies that classify payload, transfer, primaries, and alpha
- Accept: header classification of a delivered container before admission, DFD validation against the plane's declared transfer and alpha, `keepWriter` repacking of a content-addressed file, `Effect.try`-lifted `read` with a tagged fault, browser and server parsing of one byte plane
- Reject: a `vkFormat` branch standing in for the payload class, a default `write` over content-addressed bytes, `dist/util.js` members imported as public surface, level bytes decoded or transcoded here, a KTX2 encoded here instead of by the provisioned CLI, `layerCount` or `pixelDepth` read as a ≥1 count
