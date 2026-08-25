# [TS_DATA_API_GLTF_TRANSFORM_FUNCTIONS]

`@gltf-transform/functions` is the transform roster over the core property graph. Every optimization — prune, dedup, quantize, meshopt, draco, weld, simplify, palette, join, flatten, instance, partition — answers the core `Transform` shape, so a pipeline is an ordered row list folded through the one `document.transform(...)` entry.

Every heavy codec is injected, never imported: `meshopt` and `reorder` take a `MeshoptEncoder`, `simplify` a `MeshoptSimplifier`, `draco` its encoder module, `unwrap` a `watlas` instance. Graph-level readers ride here too — texture channel and slot introspection, vertex counting, bounds, and the whole-document `inspect` report.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: option records — one per transform, each the whole knob surface of its row

| [INDEX] | [SYMBOL]                 | [REQUIRED_SLOT]   | [SHAPE]                                              |
| :-----: | :----------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `MeshoptOptions`         | `encoder`         | `level` over the `QuantizeOptions` set less patterns |
|  [02]   | `ReorderOptions`         | `encoder`         | `target` `cleanup`                                   |
|  [03]   | `SimplifyOptions`        | `simplifier`      | `ratio` `error` `lockBorder`                         |
|  [04]   | `UnwrapOptions`          | `watlas`          | `texcoord` `overwrite` `groupBy`                     |
|  [05]   | `SparseOptions`          | `ratio`           | the only field                                       |
|  [06]   | `ColorSpaceOptions`      | `inputColorSpace` | `srgb` or `srgb-linear`                              |
|  [07]   | `TextureCompressOptions` | none              | `encoder` is OPTIONAL; see the codec caveat below    |

`[PRUNE_FIELD]: `propertyTypes` `keepLeaves` `keepAttributes` `keepIndices` `keepSolidTextures` `keepExtras`` — the exported option type is `PruneOptions`; every row's option record exports under `<Row>Options` (`DedupOptions`, `QuantizeOptions`, …), so a table typing its rows imports the symbol, never a hand-mirrored shape.

`[DEDUP_FIELD]: `propertyTypes` `keepUniqueNames``

`[QUANTIZE_FIELD]: `quantizePosition` `quantizeNormal` `quantizeTexcoord` `quantizeColor` `quantizeWeight` `quantizeGeneric` `quantizationVolume` `pattern` `patternTargets` `normalizeWeights` `cleanup``

`[DRACO_FIELD]: `method` `encodeSpeed` `decodeSpeed` `quantizePosition` `quantizeNormal` `quantizeColor` `quantizeTexcoord` `quantizeGeneric` `quantizationVolume``

`[TEXTURE_COMPRESS_FIELD]: `encoder` `targetFormat` `resize` `resizeFilter` `pattern` `formats` `slots` `quality` `effort` `lossless` `nearLossless` `chromaSubsampling` `limitInputPixels``

`[GEOMETRY_FIELD]: `PaletteOptions` `blockSize` `min` `keepAttributes` `cleanup` · `JoinOptions` `keepMeshes` `keepNamed` `cleanup` `filter` · `CenterOptions` `pivot` · `InstanceOptions` `min` · `ResampleOptions` `ready` `resample` `tolerance` `cleanup` · `PartitionOptions` `animations` `meshes``

`[OVERWRITE_ONLY]: `WeldOptions` `UnweldOptions` `NormalsOptions` `FlattenOptions` `UninstanceOptions` `UnpartitionOptions` `DequantizeOptions` `SequenceOptions` `MetalRoughOptions`` — `overwrite` or `cleanup` alone, with `TangentsOptions` adding a `generateTangents(pos, norm, uv)` callback — REFUSED as the branch's tangent path: the three-argument mikktspace shape carries no index array (forcing an unweld) and admits a second unadmitted wasm lineage, so the branch-owned `createTransform` row over `meshoptimizer/tangents` is the shape.

`[DEFAULTS_CONSTANT]: `PRUNE_DEFAULTS` `QUANTIZE_DEFAULTS` `MESHOPT_DEFAULTS` `DRACO_DEFAULTS` `SIMPLIFY_DEFAULTS` `WELD_DEFAULTS` `JOIN_DEFAULTS` `FLATTEN_DEFAULTS` `INSTANCE_DEFAULTS` `PALETTE_DEFAULTS` `UNWRAP_DEFAULTS` `TEXTURE_COMPRESS_DEFAULTS`` — one exported default record per row, merged by `assignDefaults(defaults, options)`.

[PUBLIC_TYPE_SCOPE]: bounded vocabularies and the census report

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `VertexCountMethod`                  | count policy  | `RENDER` `RENDER_CACHED` `UPLOAD` `UPLOAD_NAIVE` |
|  [02]   | `TextureResizeFilter`                | resample set  | `LANCZOS3` `LANCZOS2`                            |
|  [03]   | `TEXTURE_COMPRESS_SUPPORTED_FORMATS` | codec set     | `jpeg` `png` `webp` `avif` — NO KTX2 row         |
|  [04]   | `InspectReport`                      | census        | one report per property family                   |

- `VertexCountMethod.UNUSED` completes the roster and counts vertices no primitive reaches.
- `InspectReport` composes `InspectSceneReport`, `InspectMeshReport`, `InspectMaterialReport`, `InspectTextureReport`, and `InspectAnimationReport`, each built from `InspectPropertyReport` rows.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the transform rows — every one folds through `document.transform(...)`

| [INDEX] | [SURFACE]                  | [DEPENDENCY]        | [CAPABILITY]                                         |
| :-----: | :------------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `prune(options?)`          | none                | drop unused properties and solid-color textures      |
|  [02]   | `dedup(options?)`          | none                | merge identical accessors, textures, materials       |
|  [03]   | `quantize(options?)`       | none                | narrow attribute component types                     |
|  [04]   | `meshopt(options)`         | `MeshoptEncoder`    | reorder + quantize + `EXT_meshopt_compression`       |
|  [05]   | `reorder(options)`         | `MeshoptEncoder`    | vertex-cache and overdraw ordering alone             |
|  [06]   | `simplify(options)`        | `MeshoptSimplifier` | decimate to a ratio or error bound                   |
|  [07]   | `draco(options?)`          | `draco3d` encoder   | `KHR_draco_mesh_compression`                         |
|  [08]   | `textureCompress(options)` | `sharp` (optional)  | re-encode textures across `jpeg`/`png`/`webp`/`avif` |
|  [09]   | `palette(options?)`        | none                | fold solid-color materials into one palette texture  |
|  [10]   | `unwrap(options)`          | `watlas`            | generate a UV atlas                                  |

`[GEOMETRY_ROW]: `weld` `unweld` `join` `flatten` `dequantize` `normals` `tangents` `sparse` `partition` `unpartition` `instance` `uninstance` `center` `resample` `sequence` `metalRough` `unlit` `vertexColorSpace`` — the remaining rows, each `(options?) => Transform`.

[ENTRYPOINT_SCOPE]: graph readers, called directly rather than folded

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                           |
| :-----: | :------------------------------------------------- | :------------------------------------- |
|  [01]   | `inspect(doc) -> InspectReport`                    | the whole-document census              |
|  [02]   | `listTextureChannels(texture) -> TextureChannel[]` | channels a texture is actually read on |
|  [03]   | `getTextureChannelMask(texture) -> number`         | the same fact as an R/G/B/A bitmask    |
|  [04]   | `listTextureSlots(texture) -> string[]`            | material slot names binding it         |
|  [05]   | `listTextureInfo(texture) -> TextureInfo[]`        | sampler records bound to it            |
|  [06]   | `listTextureInfoByMaterial(material)`              | the same, scoped to one material       |
|  [07]   | `getTextureColorSpace(texture) -> 'srgb' \| null`  | color space its slots imply            |
|  [08]   | `getBounds(node \| scene) -> bbox`                 | world-space extent                     |
|  [09]   | `getSceneVertexCount(scene, method) -> number`     | counts under a `VertexCountMethod`     |
|  [10]   | `compressTexture(texture, options)`                | single-texture `textureCompress`       |
|  [11]   | `createTransform(name, fn) -> Transform`           | mint a branch-owned row                |

`[VERTEX_COUNT]: `getSceneVertexCount` `getNodeVertexCount` `getMeshVertexCount` `getPrimitiveVertexCount`` — one per grain, each taking the same `VertexCountMethod`.

`[DOCUMENT_OP]: `cloneDocument` `mergeDocuments` `moveToDocument` `copyToDocument` `createDefaultPropertyResolver`` — whole-document composition, each answering the property map it built.

`[PRIMITIVE_OP]: `weldPrimitive` `unwrapPrimitives` `simplifyPrimitive` `joinPrimitives` `compactPrimitive` `dequantizePrimitive` `transformPrimitive` `transformMesh` `convertPrimitiveToLines` `convertPrimitiveToTriangles` `sortPrimitiveWeights` `getGLPrimitiveCount``

`[SIZE_HELPER]: `fitWithin(size, limit)` `fitPowerOfTwo(size, method)` `assignDefaults(defaults, options)` `isTransformPending(context, initial, pending)` `listNodeScenes(node)``

- `meshopt(options)` throws at construction when `encoder` is absent, and throws during the fold when the document already carries `KHR_mesh_primitive_restart`.
- `meshopt` attaches `EXTMeshoptCompression` with `setRequired(true)` and picks `EncoderMethod.QUANTIZE` at `level: "medium"` against `EncoderMethod.FILTER` at the default `level: "high"`; it attaches `KHRMeshQuantization` as required whenever a quantized primitive survives.
- `TextureCompressOptions.encoder` is OPTIONAL — omitted, the row falls back to a platform implementation that ignores most quality and compression options.
- `fitPowerOfTwo(size, method)` takes `'nearest-pot' | 'ceil-pot' | 'floor-pot'`, the same preset union `TextureCompressOptions.resize` accepts beside an explicit `vec2`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every row is `(options?) => Transform` and every `Transform` is `(doc, context?) => void`, so an optimization pipeline is an ordered ROW LIST through one fold — there is no builder, no per-row entry, and a branch-owned step is `createTransform(name, fn)` on the same shape.
- Codecs are INJECTED, never imported: `encoder`, `simplifier`, and `watlas` are `unknown`-typed instance slots, so the row states which implementation ran and a codec swap never edits the pipeline.
- Each option record is the whole knob surface of its row and merges over an exported defaults constant, so a stated pipeline row is a data value rather than a call-site argument spray.
- Graph readers answer FACTS about a document — channel masks, slot names, vertex counts, bounds, the inspect census — so policy reads the graph rather than guessing from a filename.

[STACKING]:
- `@gltf-transform/core`(`.api/gltf-transform-core.md`): every row answers core's `Transform` and folds through `Document.transform(...)`; `listTextureChannels` returns core's `TextureChannel` bitmask values and `getBounds` returns core's `bbox`.
- `@gltf-transform/extensions`(`.api/gltf-transform-extensions.md`): `meshopt` and `draco` attach their extensions and set them required, so the IO roster must carry `EXTMeshoptCompression` + `KHRMeshQuantization` or `KHRDracoMeshCompression` before a pipeline running either row writes.
- `meshoptimizer`(`.api/meshoptimizer.md`): `MeshoptEncoder` satisfies `meshopt.encoder` and `reorder.encoder`, `MeshoptSimplifier` satisfies `simplify.simplifier`; both await `ready` before the fold, and the encoder passed here is the SAME instance registered on the IO.
- `ktx-parse`(`.api/ktx-parse.md`): `TEXTURE_COMPRESS_SUPPORTED_FORMATS` carries no `ktx2` row, so KTX2 arrives as bytes the provisioned `ktx` CLI produced, lands on a `Texture` under `KHR_texture_basisu`, and classifies here through `listTextureSlots` + `getTextureColorSpace` rather than a re-encode.
- `sharp`(`.api/sharp.md`): the object plane's ONE libvips owner is `object/file.md`; `textureCompress` is a second encoder over the same native and is refused at the owner, so raster re-encoding stays on the fanout spine and this package's texture rows contribute classification alone.
- `object/file.md` `[05]-[FANOUT]`: the derivative spine is decode-once, clone-N over a row roster, and a container pipeline is the SAME spine parameterized by a container-row engine — the rows here are that engine's vocabulary, never a second fanout.
- `effect`(`.api/effect.md`): `document.transform(...)` is one Promise lifted through `Effect.tryPromise` with a tagged fault; a row that throws at construction (`meshopt` without its encoder) fails before the effect and is caught by proving the codec `ready` at layer build.

[LOCAL_ADMISSION]:
- `textureCompress` and `compressTexture` are REJECTED at this owner. They are a second libvips composer beside `object/file.md`, and their fallback path silently ignores quality and compression options — raster re-encoding rides the one sharp owner and KTX2 rides the provisioned CLI.
- Admission states a roster: the container-surgery rows (`prune`, `dedup`, `quantize`, `meshopt`) and the geometry rows a pipeline names. Every other row admits by adding its roster entry, never by a call-site option.
- `ndarray-pixels` binds `sharp` at module load, so this package is server-lane only and its version resolution is pinned to the catalog sharp — an unpinned resolution loads a SECOND libvips native beside the object plane's.
- Rows requiring a codec run only after that codec's `ready` resolves, proven at layer construction rather than inside the fold, so a pipeline never fails halfway through a document mutation.
- `draco` is NOT admitted: its `draco3d.encoder` slot demands a further unadmitted npm distribution, `meshopt` covers the same geometry-compression concern, and the viewer already serves one decoder path.
