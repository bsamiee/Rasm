# [TS_UI_API_LOADERS_GL_3D_TILES]

`@loaders.gl/3d-tiles` decodes the OGC 3D Tiles / Cesium tile formats — the `cmpt`/`pnts`/`b3dm`/`i3dm` binary tiles and the `tileset.json` manifest — that `Tile3DLayer` resolves through `@loaders.gl/core`. It owns ONLY the content-decoder family and the `Tiles3DLoader` registration deck defaults to; runtime tileset traversal (`Tileset3D`/`Tile3D` LOD, screen-space error, tile lifecycle) is `@loaders.gl/tiles`, deck-owned transitive. `scope:viewer` project-local, compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@loaders.gl/3d-tiles`
- package: `@loaders.gl/3d-tiles` (MIT)
- abi: isomorphic browser/node; binary tile parse runs Web Worker-backed through core's parse engine (`options.worker`); embedded glTF, Draco geometry, and texture decode delegate to the transitive `@loaders.gl/gltf`/`@loaders.gl/draco`/`@loaders.gl/images` decoders
- deps (transitive loaders.gl substrate): `@loaders.gl/loader-utils` (`StrictLoaderOptions`/`LoaderContext`), `@loaders.gl/gltf` (`GLTFPostprocessed`/`FeatureTableJson`, the embedded-glTF decode batched/instanced tiles delegate to), `@loaders.gl/draco`, `@loaders.gl/images`, `@loaders.gl/schema`
- runtime: `scope:viewer` project-local, peer `@loaders.gl/core` (`.api/loaders.gl-core.md`) — stateless decoders whose only mutable surface is the core loader registry, registered app-scoped, never a global two apps contend over
- modules: `Tiles3DLoader`, `CesiumIonLoader`, `Tile3DSubtreeLoader`, `Tiles3DArchiveFileLoader`, `Tiles3DArchive`, `Tile3DWriter`, `Tile3DFeatureTable`, `Tile3DBatchTable`, `TILE3D_TYPE`, `_getIonTilesetMetadata`; types `Tiles3DLoaderOptions`, `Tiles3DTileContent`, `Tiles3DTilesetJSONPostprocessed`, `Tiles3DTileJSONPostprocessed`, `Tile3DBoundingVolume`, `B3DMContent`, `Subtree`, `FeatureTableJson`
- absent (not admitted): `@loaders.gl/tiles` (the `Tileset3D`/`Tile3D` traversal engine reached through deck), `@loaders.gl/i3s` (the ESRI I3S variant `Tile3DLayer` also accepts, its own admission); the sibling core registrations `@loaders.gl/las`/`mvt`/`terrain` and core's `parse`/`load`/`registerLoaders` surface stay their own owners

## [02]-[DECODER_FAMILY]

[TYPE_SCOPE]: the decoder descriptors — one `Loader` value each, registered into `@loaders.gl/core` or passed per `parse`/`load` call `(data, options?, context?)`. Content magic (`cmpt`/`pnts`/`b3dm`/`i3dm`), transport (open href vs Ion token vs `.3tz` archive vs `.subtree` bitstream), and manifest-vs-tile shape are the discriminants; `isTileset: 'auto'` sniffs manifest from tile, never a `parseTileset`/`parseTile` split.
- each content descriptor shares the tile format identity (`id: '3d-tiles'`, `extensions`/`tests`: `['cmpt','pnts','b3dm','i3dm']`, `binary: true`), so `selectLoader` resolves the decoder from the four-byte magic; `Tiles3DLoader.parse` returns a tileset-or-tile whole and `batchType` is `never` — a large tileset streams through the `@loaders.gl/tiles` traversal fetch fanout, not `parseInBatches`.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `Tiles3DLoader -> Promise<Content\|Tileset>`                 | value   | deck `Tile3DLayer` default decoder; `isTileset` manifest sniff |
|  [02]   | `CesiumIonLoader.preload(url, opts?)`                        | value   | Cesium Ion tilesets; `preload` resolves the auth token         |
|  [03]   | `Tile3DSubtreeLoader -> Subtree`                             | value   | `.subtree` implicit-tiling availability bitstreams             |
|  [04]   | `Tiles3DArchiveFileLoader -> Promise<ArrayBuffer>`           | value   | `.3tz` archive member extraction before content decode         |
|  [05]   | `Tiles3DArchive.getFile(path) -> Promise<ArrayBuffer>`       | class   | random-access member lookup over an opened `.3tz`              |
|  [06]   | `Tile3DWriter.encode/encodeSync(tile, opts?) -> ArrayBuffer` | value   | inverse egress — a decoded tile back to tile bytes             |
|  [07]   | `Tile3DFeatureTable`                                         | class   | `featureTable` per-feature property accessor                   |
|  [08]   | `Tile3DBatchTable`                                           | class   | `batchTable` per-batch metadata accessor                       |

## [03]-[OPTIONS_AND_TYPES]

[TYPE_SCOPE]: the `'3d-tiles'` sub-options bag on `Tiles3DLoaderOptions`, the `TILE3D_TYPE` content vocabulary, and the decoded output shapes the loaders return — the vocabulary `Tile3DLayer` traversal and mesh graft consume.
- `Tiles3DLoaderOptions` — `StrictLoaderOptions & DracoLoaderOptions & ImageLoaderOptions & { '3d-tiles'?: {...} }`; the base bag (`worker`/`fetch`/`CDN`) lives in `.api/loaders.gl-core.md`, the Draco/image sub-bags decode compressed geometry and embedded textures.
- `'3d-tiles'` knobs — `loadGLTF` (default true) parses embedded glTF in-loader versus extracting `gltfArrayBuffer` for an independent glTF pass; `decodeQuantizedPositions` (default false) CPU-decodes quantized positions when the renderer declines them; `isTileset` (`'auto'` default) sniffs manifest from tile; `assetGltfUpAxis` (`'x'|'y'|'z'|null`) reconciles the tile Z-up frame with the model Y-up.
- `TILE3D_TYPE` — discriminates a decoded `Tiles3DTileContent.type` across `COMPOSITE`/`POINT_CLOUD`/`BATCHED_3D_MODEL`/`INSTANCED_3D_MODEL`/`GEOMETRY`/`VECTOR`/`GLTF`, driving the mesh-vs-point-vs-instance render branch; the four binary magics are `cmpt`/`pnts`/`b3dm`/`i3dm`.
- `Tiles3DTileContent` — `{ shape:'tile3d', type?, magic?, version?, byteLength?, header?, featureTable*?, batchTable*?, rtcCenter?, gltf?/gltfArrayBuffer?, gltfUpAxis?, tiles? (composite), featuresLength?/attributes? (points/instances) }`; the decoded per-tile payload.
- `Tiles3DTilesetJSONPostprocessed` — `{ url, type: TILESET_TYPE.TILES3D, lodMetricValue, geometricError, asset, root: Tiles3DTileJSONPostprocessed }`; the decoded manifest with the root pre-resolved and content URLs absolutized; `Tiles3DTileJSONPostprocessed` is one traversal node (`boundingVolume`, `geometricError`, `refine?`, `content?`/`contentUrl?`, `children`, `implicitTiling?`).
- `Tile3DBoundingVolume` — `{ box?: number[12], sphere?: number[4], region?: number[6] }`; the tile spatial bound — oriented box, sphere, or EPSG:4979 `[west,south,east,north,minH,maxH]` region — driving the cull frustum test.
- `B3DMContent` — `{ batchTableJson?, featureTableJson?/featureTableBinary?, gltf?: GLTFPostprocessed, gltfUpAxis, rtcCenter: [number,number,number], type }`; the specialized `b3dm` decode shape.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- register INTO core, app-scoped: `registerLoaders([Tiles3DLoader])` mutates core's process-wide roster, so `viewer` registers inside an Effect `Scope` and `_unregisterLoaders` at release; passing `Tiles3DLoader` per `Tile3DLayer` `loaders` prop is the collision-free alternative. Binary tile decode runs in the core worker pool by default, embedded glTF/Draco/texture decode through the transitive decoders — no tile parse blocks the main thread.

[STACKING]:
- `@loaders.gl/core`(`.api/loaders.gl-core.md`): `load(url, Tiles3DLoader)` fetches-then-decodes a `tileset.json` or tile href and `selectLoader` sniffs the four-byte magic — no decode vocabulary added atop core's polymorphic `parse`/`load`.
- `@deck.gl/geo-layers`(`.api/deck.gl-geo-layers.md`): `Tile3DLayer` defaults `loaders`/`loader` to `Tiles3DLoader` and drives the `@loaders.gl/tiles` `Tileset3D`/`Tile3D` traversal — LOD by screen-space error, fetch fanout, lifecycle — surfacing each tile on `onTilesetLoad(Tileset3D)`/`onTileLoad(Tile3D)`; batched/instanced `gltf` renders through `@deck.gl/mesh-layers`, `pnts` feeds the columnar point path.
- `@loaders.gl/las`(`.api/loaders.gl-las.md`): the `pnts` point content complements the LAS scan decoder on the same `PointCloudLayer` binary-attribute seam, both registered into the one core roster.
- `viewer/geo`: `Tile3DFeatureTable`/`Tile3DBatchTable` decode the `featureTableBinary`/`batchTableJson` a `Tile3DLayer` `onClick` resolves picked-feature metadata against.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it, keeping heavy tile-decoder/worker deps compile-time excluded from non-spatial apps.
- pick the decoder by transport: `Tiles3DLoader` for open hrefs, `CesiumIonLoader` for Ion tokens, `Tiles3DArchiveFileLoader` for `.3tz` archives, `Tile3DSubtreeLoader` for implicit-tiling availability; `@loaders.gl/tiles` traversal and the `@loaders.gl/i3s` variant are their own admissions.

[RAIL_LAW]:
- Package: `@loaders.gl/3d-tiles`
- Owns: the 3D-tiles/Cesium content-decoder family — the `Tiles3DLoader`/`CesiumIonLoader`/`Tile3DSubtreeLoader`/`Tiles3DArchiveFileLoader` transports, `Tile3DWriter` inverse encode, `Tile3DFeatureTable`/`Tile3DBatchTable` metadata accessors, the `TILE3D_TYPE` vocabulary, the `'3d-tiles'` option bag, and the decoded output shapes
- Accept: registering into `@loaders.gl/core` inside an Effect `Scope`, decoder selection by transport and `isTileset` sniff, `Tile3DLayer` driving the `@loaders.gl/tiles` traversal over this decoder, embedded glTF/Draco/texture decode delegated to the transitive decoders
- Reject: hand-parsing tile bytes off the worker pool, a `parseTileset`/`parseIonTile` family instead of core's polymorphic `parse`/`load`, importing the `@loaders.gl/tiles` traversal engine directly, a global loader roster two apps contend over
