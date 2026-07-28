# [TS_DATA_API_GLTF_TRANSFORM_CORE]

`@gltf-transform/core` lifts a glTF 2.0 asset out of its JSON-and-index encoding into a mutable property graph: `Document` owns the graph, `Root` lists every property family, and each property holds typed references instead of integer indices. Indices are re-derived at write, so an edit never renumbers anything by hand.

`PlatformIO` is the one ingress/egress with three platform subclasses; a `Transform` is a plain `(doc, context?) => void` the document folds. Nothing here encodes an image, compresses a buffer, or knows an extension — those ride the extension and function packages over this graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@gltf-transform/core`
- package: `@gltf-transform/core` (MIT)
- module: `exports["."]` condition-selects `dist/index.cjs` for `require` against the `dist/index.js` default; types at `dist/index.d.ts`
- runtime: both lanes — `NodeIO` binds `node:fs` on the server, `WebIO` binds `fetch` in the browser, `DenoIO` the third host; the graph itself is host-free
- depends: `property-graph` — the reference-counted graph substrate `Property` extends
- rail: `object` container surgery, lifted into the `Effect` rail at the boundary where a `.glb` byte plane is read or written
- boundary: the graph and its IO alone; codecs, compression, and extension vocabulary land in the sibling packages

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the document, its root, and the IO shapes

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Document`                     | graph owner    | the whole asset; every property mints here  |
|  [02]   | `Root`                         | property index | lists every property family in the asset    |
|  [03]   | `JSONDocument`                 | IO record      | `{ json, resources }` — glTF plus its files |
|  [04]   | `PlatformIO`                   | abstract IO    | read/write over an injected host            |
|  [05]   | `NodeIO` / `WebIO` / `DenoIO`  | IO hosts       | fs, fetch, and Deno bindings                |
|  [06]   | `Transform` / `TransformContext` | fold shape   | `(doc, context?) => void`                   |
|  [07]   | `Extension` / `ExtensionProperty` | extension seam | the base every extension package extends |

[PUBLIC_TYPE_SCOPE]: the property families the graph holds

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :---------------------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Property` / `ExtensibleProperty`               | base          | name, extras, and extension attachment   |
|  [02]   | `Scene` / `Node` / `Mesh` / `Primitive`         | scene graph   | hierarchy and drawable geometry          |
|  [03]   | `Accessor` / `Buffer`                           | binary plane  | typed vertex and index storage           |
|  [04]   | `Material` / `Texture` / `TextureInfo`          | appearance    | PBR factors, image bytes, sampler state  |
|  [05]   | `Animation` / `AnimationChannel` / `AnimationSampler` | motion  | keyframe tracks                          |
|  [06]   | `Skin` / `PrimitiveTarget` / `Camera`           | rig and view  | joints, morph targets, projection        |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies and utility statics

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `PropertyType`                 | family key    | the string tag every property answers                 |
|  [02]   | `Format`                       | file form     | `GLTF` (JSON + sidecars) or `GLB` (one binary)        |
|  [03]   | `TextureChannel`               | channel mask  | `R = 4096` `G = 256` `B = 16` `A = 1`, bitwise-ORed   |
|  [04]   | `VertexLayout`                 | write policy  | `INTERLEAVED` or `SEPARATE` buffer views              |
|  [05]   | `Verbosity` / `Logger` / `ILogger` | diagnostics | `SILENT` `ERROR` `WARN` `INFO` `DEBUG`              |
|  [06]   | `ImageUtils` / `ImageUtilsFormat` | image probe | registerable size/channel/VRAM readers per mime type  |
|  [07]   | `BufferUtils` / `FileUtils` / `MathUtils` / `ColorUtils` | helpers | byte, path, matrix, and color-space statics |

`[GEOMETRY_ALIAS]: `vec2` `vec3` `vec4` `mat3` `mat4` `bbox` `TypedArray` `TypedArrayConstructor`` — the structural aliases every accessor and transform member takes.

`[REFERENCE_SHAPE]: `Ref` `RefList` `RefMap` `RefSet` `Graph` `GraphEdge` `Nullable` `PropertyResolver` `COPY_IDENTITY`` — the `property-graph` reference primitives a property declares its links with.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the document, its property mints, and the transform fold

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `new Document()`                                 | factory  | an empty asset with a `Root`             |
|  [02]   | `getRoot() -> Root`                              | instance | the property index                       |
|  [03]   | `transform(...transforms) -> Promise<this>`      | fold     | THE fold — every function composes here  |
|  [04]   | `createExtension(ctor) -> T`                     | instance | bind one extension to the document       |
|  [05]   | `hasExtension(name)` / `disposeExtension(name)`  | instance | extension presence and removal           |
|  [06]   | `setLogger(logger)` / `getLogger()`              | instance | verbosity for every operation on the doc |
|  [07]   | `Document.fromGraph(graph) -> Document \| null`  | static   | recover the document owning a graph      |

`[PROPERTY_MINT]: `createScene` `createNode` `createMesh` `createPrimitive` `createPrimitiveTarget` `createMaterial` `createTexture` `createAccessor` `createBuffer` `createAnimation` `createAnimationChannel` `createAnimationSampler` `createSkin` `createCamera`` — every property mints from its document and attaches to the `Root`.

`[ROOT_LIST]: `listScenes` `listNodes` `listMeshes` `listMaterials` `listTextures` `listAccessors` `listBuffers` `listAnimations` `listSkins` `listCameras` `listExtensionsUsed` `listExtensionsRequired`` — plus `getAsset`, `getDefaultScene`, and `setDefaultScene`.

[ENTRYPOINT_SCOPE]: IO — the one read/write surface and its host bindings

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `readBinary(glb) -> Promise<Document>`          | instance | decode `.glb` bytes already in hand     |
|  [02]   | `writeBinary(doc) -> Promise<Uint8Array>`       | instance | encode the graph back to `.glb` bytes   |
|  [03]   | `read(uri) -> Promise<Document>`                | instance | host-resolved read with sidecar resolve |
|  [04]   | `NodeIO.write(uri, doc) -> Promise<void>`       | instance | fs sink; `NodeIO` alone declares it     |
|  [05]   | `readJSON(jsonDoc)` / `writeJSON(doc, opts?)`   | instance | the `JSONDocument` round trip           |
|  [06]   | `binaryToJSON(glb) -> Promise<JSONDocument>`    | instance | container split without graph build     |
|  [07]   | `registerExtensions(extensions) -> this`        | fold     | the CLOSED extension roster for this IO |
|  [08]   | `registerDependencies(deps) -> this`            | fold     | keyed codec instances extensions demand |

- `lastReadBytes` and `lastWriteBytes` populate on the URI path ALONE — `readAsJSON`/`read` and `NodeIO.write` set them, while `readBinary`/`writeBinary` leave both at zero, so the object plane measures its own bytes rather than reading these counters.
- `setVertexLayout(layout)` and `setStrictResources(strict)` are write-side policy; `NodeIO.setAllowNetwork(allow)` gates sidecar fetches and `NodeIO.init()` resolves its `node:fs` binding.
- `readURI`, `resolve`, and `dirname` are `protected` — a new host subclasses `PlatformIO` and implements them; they are not callable surface.

`[TEXTURE_MEMBER]: `getMimeType` `setMimeType` `getURI` `setURI` `getImage` `setImage` `getSize`` — `getImage()` answers the encoded bytes verbatim and `getSize()` reads extent from those bytes through `ImageUtils`.

- `ImageUtils.impls` ships `image/jpeg` and `image/png` alone. `KHRTextureBasisu.register()` — the STATIC, never `document.createExtension(KHRTextureBasisu)` — installs the `image/ktx2` impl, and before that call `ImageUtils.getMimeType` fails to sniff a KTX2 buffer and `Texture.getSize()` on a KTX2 texture answers `null`.

`[MATERIAL_TEXTURE_SLOT]: `BaseColor` `MetallicRoughness` `Normal` `Occlusion` `Emissive`` — each slot answers `get<Slot>Texture()`, `get<Slot>TextureInfo()`, and `set<Slot>Texture(texture)`; every other PBR channel is an extension property.

`[ACCESSOR_MEMBER]: `getArray` `setArray` `getType` `setType` `getComponentType` `getElementSize` `getComponentSize` `getCount` `getByteLength` `getScalar` `setScalar` `setElement` `getMin` `getMax` `getMinNormalized` `getMaxNormalized` `getNormalized` `setNormalized` `getSparse` `setSparse` `getBuffer` `setBuffer``

- `Accessor.getElement(index, target)` resolves at runtime and appears throughout the package's own JSDoc, yet `dist/index.d.ts` declares only `setElement` — the read half is untyped surface, so an element read spells `getScalar` per component or reads `getArray()` directly.

`[IMAGE_PROBE]: `ImageUtils.getMimeType` `ImageUtils.getSize` `ImageUtils.getChannels` `ImageUtils.getVRAMByteLength` `ImageUtils.registerFormat` `ImageUtils.mimeTypeToExtension` `ImageUtils.extensionToMimeType`` — sniffing over a registered `ImageUtilsFormat` table, never a suffix read.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Graph values rule and the JSON never does: properties hold typed references and the writer re-derives every index, so an edit never renumbers accessors, buffer views, or texture indices and a dangling index stays unrepresentable.
- `Document.transform(...)` is the one fold, and `Transform` is `(doc, context?) => void` mutating in place, so a pipeline is an ordered row list, never a chained builder or a per-operation entry.
- `PlatformIO` is one abstract ingress with three host bindings; the host reaches the graph only through `readURI`/`resolve`/`dirname`, so a fourth host is a subclass and never a fork of the reader.
- Extensions are DATA on the IO, not a global: `registerExtensions` is the closed roster this IO honors, and an unregistered extension survives a round trip only as opaque JSON.

[STACKING]:
- `@gltf-transform/extensions`(`.api/gltf-transform-extensions.md`): every `Extension` subclass extends this package's `Extension` and mints its properties through `Document.createExtension(ctor)`; `registerExtensions` takes the roster and `registerDependencies` installs the keyed codec instances an extension declares in `readDependencies`/`writeDependencies`.
- `@gltf-transform/functions`(`.api/gltf-transform-functions.md`): every function answers this package's `Transform`, so the whole roster composes through the one `document.transform(...)` fold and needs no second entry.
- `ktx-parse`(`.api/ktx-parse.md`): a `Texture` carrying `image/ktx2` yields container bytes through `getImage()`, which classify without a transcoder — `ktx-parse` reads extent, payload class, transfer, and alpha off the header, and `ImageUtils` answers the same extent only after `KHRTextureBasisu.register()` installs its impl.
- `effect`(`.api/effect.md`): `readBinary`, `writeBinary`, `read`, and `transform` are Promises lifted through `Effect.tryPromise` with a tagged fault at the object boundary; `Logger` binds to the branch logger through the `ILogger` interface rather than a second sink.
- `object/store.md`: `writeBinary` output is content-addressed and admitted under the one `ContentKey` conditional put, so a re-encoded container is idempotent against a concurrent writer of the same bytes.
- `object/stream.md`: `readBinary` takes bytes already in hand, so an unbounded container reads through the BYOB ingress into a bounded buffer before the graph builds — `read(uri)` never becomes the fetch path for an untrusted object.

[LOCAL_ADMISSION]:
- Bytes in, bytes out: `readBinary`/`writeBinary` are the admitted pair, and `read(uri)`/`NodeIO.write(uri, doc)` stay out of the object plane, whose addresses are content keys rather than host paths.
- `NodeIO.setAllowNetwork(false)` holds on every server instance — a sidecar-resolving glTF must not fetch, and the object plane supplies every resource by key.
- `setVertexLayout` is stated per pipeline rather than inherited, so a write states its buffer-view geometry instead of taking a silent default.
- `getImage()` returns the encoded bytes, never a decoded plane; extent and channel count come from `ImageUtils` under a registered impl, and pixel work belongs to the raster owner.
- Round-tripping through an IO whose roster omits an extension the source used drops that extension's properties, so every roster is stated closed and proven against `Root.listExtensionsUsed()` after read.

[RAIL_LAW]:
- Package: `@gltf-transform/core`
- Owns: the glTF 2.0 property graph — `Document` and `Root`, every property family, typed references in place of indices, the `PlatformIO` read/write surface with its three hosts, the `Transform` fold, the extension base class, and the image/buffer/path/math statics
- Accept: `readBinary`/`writeBinary` over bytes already in hand, `Effect`-lifted terminals with tagged faults, a closed `registerExtensions` roster with `registerDependencies` codec instances, `document.transform(...)` over an ordered row list, content-addressed output through the object store
- Reject: JSON index arithmetic done by hand, a network-resolving IO on the server lane, an implicit vertex layout, `getImage()` treated as a decoded plane, a per-operation entry beside the one transform fold, an extension consumed without its roster row
