# [TS_DATA_API_GLTF_TRANSFORM_EXTENSIONS]

`@gltf-transform/extensions` implements the Khronos and multi-vendor glTF extensions as `Extension` subclasses over the core property graph. Each one reads its JSON into typed `ExtensionProperty` values on read and re-emits them on write, so a clearcoat weight or a KTX2 image source is a property member rather than an `extensions` object literal.

Admission is a roster on the IO: `registerExtensions` states which extensions this branch honors, and an extension outside the roster survives only as opaque JSON. Both compression extensions carry no codec — `EXT_meshopt_compression` and `KHR_draco_mesh_compression` declare keyed dependencies the caller installs.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the roster constants — the ONLY admission surface

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `KHRONOS_EXTENSIONS` | closed roster | every ratified `KHR_*` extension class              |
|  [02]   | `ALL_EXTENSIONS`     | open roster   | the Khronos set plus every `EXT_*` vendor extension |

[PUBLIC_TYPE_SCOPE]: texture and compression extensions — the asset-pipeline set

| [INDEX] | [SYMBOL]                  | [EXTENSION_NAME]             | [CAPABILITY]                               |
| :-----: | :------------------------ | :--------------------------- | :----------------------------------------- |
|  [01]   | `KHRTextureBasisu`        | `KHR_texture_basisu`         | KTX2 Basis image source on a `Texture`     |
|  [02]   | `KHRTextureTransform`     | `KHR_texture_transform`      | per-`TextureInfo` UV offset/rotation/scale |
|  [03]   | `EXTTextureWebP`          | `EXT_texture_webp`           | WebP image source                          |
|  [04]   | `EXTTextureAVIF`          | `EXT_texture_avif`           | AVIF image source                          |
|  [05]   | `EXTMeshoptCompression`   | `EXT_meshopt_compression`    | meshopt-encoded buffer views               |
|  [06]   | `KHRDracoMeshCompression` | `KHR_draco_mesh_compression` | Draco-encoded primitives                   |
|  [07]   | `KHRMeshQuantization`     | `KHR_mesh_quantization`      | declares quantized attribute types legal   |

[PUBLIC_TYPE_SCOPE]: material extensions and their property classes

| [INDEX] | [EXTENSION]                         | [PROPERTY]                            | [TEXTURE_SLOTS]                                            |
| :-----: | :---------------------------------- | :------------------------------------ | :--------------------------------------------------------- |
|  [01]   | `KHRMaterialsClearcoat`             | `Clearcoat`                           | clearcoat · clearcoatRoughness · clearcoatNormal           |
|  [02]   | `KHRMaterialsSheen`                 | `Sheen`                               | sheenColor · sheenRoughness                                |
|  [03]   | `KHRMaterialsSpecular`              | `Specular`                            | specular · specularColor                                   |
|  [04]   | `KHRMaterialsTransmission`          | `Transmission`                        | transmission                                               |
|  [05]   | `KHRMaterialsVolume`                | `Volume`                              | thickness                                                  |
|  [06]   | `KHRMaterialsIridescence`           | `Iridescence`                         | iridescence · iridescenceThickness                         |
|  [07]   | `KHRMaterialsAnisotropy`            | `Anisotropy`                          | anisotropy                                                 |
|  [08]   | `KHRMaterialsDiffuseTransmission`   | `DiffuseTransmission`                 | diffuseTransmission · diffuseTransmissionColor             |
|  [09]   | `KHRMaterialsIOR`                   | `IOR`                                 | none — scalar only                                         |
|  [10]   | `KHRMaterialsDispersion`            | `Dispersion`                          | none — scalar only                                         |
|  [11]   | `KHRMaterialsEmissiveStrength`      | `EmissiveStrength`                    | none — scalar only                                         |
|  [12]   | `KHRMaterialsUnlit`                 | `Unlit`                               | none — marker only                                         |
|  [13]   | `KHRMaterialsVariants`              | `Variant` / `Mapping` / `MappingList` | none — material swap sets                                  |
|  [14]   | `KHRMaterialsPBRSpecularGlossiness` | `PBRSpecularGlossiness`               | diffuse · specularGlossiness — RETIRED upstream vocabulary |

`[SCENE_EXTENSION]: `KHRLightsPunctual`/`Light` `KHRNodeVisibility`/`Visibility` `EXTMeshGPUInstancing`/`InstancedMesh` `KHRMeshPrimitiveRestart` `KHRXMP`/`Packet` `KHRAccessorFloat16` `KHRAccessorFloat64``

`[METADATA_EXTENSION]: `EXTStructuralMetadata` `EXTMeshFeatures`` — with `Schema` `Class` `ClassProperty` `Enum` `EnumValue` `PropertyTable` `PropertyTableProperty` `PropertyTexture` `PropertyTextureProperty` `PropertyAttribute` `PropertyAttributeProperty` `FeatureID` `FeatureIDTexture` `Features` `MeshPrimitiveStructuralMetadata` `NodeStructuralMetadata`, plus the `ClassPropertyType` `ClassPropertyComponentType` `EnumValueType` `PropertyTablePropertyOffsetType` vocabularies.

- `Transform` exported here is the `KHR_texture_transform` PROPERTY class, and `Transform` exported by `@gltf-transform/core` is the `(doc, context?) => void` fold type — two unrelated symbols under one name, so a file importing both aliases at the import site.
- `INSTANCE_ATTRIBUTE` names the `EXT_mesh_gpu_instancing` attribute semantics; `EXTMeshoptCompression.EncoderMethod` is `{ QUANTIZE: "quantize", FILTER: "filter" }` and `KHRDracoMeshCompression.EncoderMethod` is the Draco `EDGEBREAKER`/`SEQUENTIAL` pair.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission, attachment, and the codec dependency registry

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `io.registerExtensions([...])`                        | fold     | THE closed roster this IO honors     |
|  [02]   | `io.registerDependencies({ key: instance })`          | fold     | keyed codecs an extension demands    |
|  [03]   | `document.createExtension(Ctor) -> Extension`         | instance | attach one extension to a document   |
|  [04]   | `<Extension>.EXTENSION_NAME`                          | static   | the wire name; the roster's identity |
|  [05]   | `setRequired(required) / isRequired()`                | instance | `extensionsRequired` membership      |
|  [06]   | `listProperties() -> ExtensionProperty[]`             | instance | every property this extension minted |
|  [07]   | `EXTMeshoptCompression.setEncoderOptions({ method })` | instance | `QUANTIZE` or `FILTER` encode mode   |

- `readDependencies`/`writeDependencies` declare the keys `registerDependencies` must supply: `EXT_meshopt_compression` reads `meshopt.decoder` and writes `meshopt.encoder`; `KHR_draco_mesh_compression` reads `draco3d.decoder` and writes `draco3d.encoder`. Omitting a key throws at read or write, naming the extension and the key in the message.
- Each image-source extension carries a `static register()` installing its `ImageUtilsFormat` on `ImageUtils.impls`: `KHRTextureBasisu` installs `image/ktx2`, `EXTTextureWebP` `image/webp`, `EXTTextureAVIF` `image/avif`. Calling `document.createExtension(Ctor)` does NOT run it, so a `Texture` of that mime type answers `getSize()` as `null` until the STATIC runs.
- `preread`, `prewrite`, `read`, and `write` take a core `ReaderContext`/`WriterContext` and are the extension's own lifecycle — driven by the IO, never called by a consumer.
- Every extension property mints from its extension instance (`clearcoatExt.createClearcoat()`), attaches through `material.setExtension(name, property)`, and reads back through `material.getExtension(name)`.
- `KHRTextureTransform.createTransform()` mints the per-`TextureInfo` transform carrying `getOffset`/`setOffset`, `getRotation`/`setRotation`, `getScale`/`setScale`, and `getTexCoord`/`setTexCoord`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Extensions are CLASSES on a roster, never flags: `registerExtensions` states the closed set, `EXTENSION_NAME` is each identity, and an unregistered extension round-trips as opaque JSON with no typed access — silent capability loss only when the roster is left implicit.
- Codec-bearing extensions carry no codec. `EXT_meshopt_compression` and `KHR_draco_mesh_compression` declare keyed dependencies and refuse at read or write when the key is absent, so a decoder is injected data rather than a hard import.
- Every material extension is a property on `Material`, so the PBR surface widens by attachment and the core `Material` shape never grows a column.
- `KHR_texture_basisu` swaps the image SOURCE, not the sampler: the `Texture` still answers `getMimeType()`/`getImage()`, so a container-class read is one code path across every image extension.

[STACKING]:
- `@gltf-transform/core`(`.api/gltf-transform-core.md`): `Extension` and `ExtensionProperty` are core base classes; the roster binds through `PlatformIO.registerExtensions` and the dependency map through `PlatformIO.registerDependencies`, so admission is IO state and every extension property still lists from `Root`.
- `ktx-parse`(`.api/ktx-parse.md`): this package already depends on it for `KHR_texture_basisu`, so the branch's container gate and the extension read share ONE parser — payload class, transfer, and alpha classify from the same `KTX2Container` the extension consumed, and `KHRTextureBasisu.register()` is what lets core's `ImageUtils` answer extent for the same bytes.
- `meshoptimizer`(`.api/meshoptimizer.md`): `MeshoptDecoder` installs at `meshopt.decoder` and `MeshoptEncoder` at `meshopt.encoder`; both expose `ready` and are awaited before the IO reads or writes a meshopt-compressed asset.
- `@gltf-transform/functions`(`.api/gltf-transform-functions.md`): `meshopt()` attaches `EXTMeshoptCompression` and `KHRMeshQuantization` itself, so a pipeline that runs it states both in its roster rather than attaching them by hand.
- `object/store.md`: extensions that change the emitted bytes change the `ContentKey`, so a roster change re-addresses every asset it touches, admitted through the same conditional put.

[LOCAL_ADMISSION]:
- Every roster is stated CLOSED. `ALL_EXTENSIONS` widens the honored set to every vendor extension in the package and is the refused form — a delivered asset then decodes vocabulary this branch has no consumer for, and a write re-emits it as required.
- `KHR_materials_pbrSpecularGlossiness` is retired upstream vocabulary admitted for INGEST alone; conversion to metallic-roughness lands at ingest and no emitted asset carries it.
- `setRequired(true)` is stated only for an extension a consumer cannot render without — a required extension the viewer lacks fails the load hard rather than degrading the render.
- Every image-source extension on the roster has its `register()` static called once at layer construction, because the roster alone installs no `ImageUtils` impl and a silent `null` extent reaches policy as a missing texture.
- `EXT_texture_webp` and `EXT_texture_avif` widen the image source for a consumer proven to support them; the KTX2 path stays the transcodable one, because a Basis payload serves every GPU and a WebP source decodes on the CPU.
- `registerDependencies` runs with codecs already `ready`; an IO built before its decoder resolves throws on the first compressed buffer view.
