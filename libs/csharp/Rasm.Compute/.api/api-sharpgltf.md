# [RASM_COMPUTE_API_SHARPGLTF]

The SharpGLTF distribution is Bim-owned across this branch and Compute registers it rather than re-tabling it: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, and `SharpGLTF.Runtime` carry the glTF 2.0 schema, the typed scene/mesh/material builders, and the runtime scene-decode surface at `libs/csharp/Rasm.Bim/.api/api-sharpgltf.md`, and `SharpGLTF.Ext.3DTiles` carries the `EXT_structural_metadata`/`EXT_mesh_features` emitter surface at `libs/csharp/Rasm.Bim/.api/api-sharpgltf-3dtiles.md`. Compute's partition is the `Runtime/codecs#TILE_PARTITION` composition-root admission alone — the extension registration the octree's leaf-content contract depends on, with no leaf body emitted here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: SharpGLTF Compute partition
- packages: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, `SharpGLTF.Ext.3DTiles` (direct `PackageReference`); `SharpGLTF.Runtime` transitive through `SharpGLTF.Toolkit`
- assembly/namespace: as catalogued at the two Bim owners; Compute names `SharpGLTF.Schema2.Tiles3D` alone and holds no member roster of its own
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; the net10.0 consumer binds `lib/net10.0`
- rail: geometry

- Registers the SharpGLTF glTF authoring core(`libs/csharp/Rasm.Bim/.api/api-sharpgltf.md`): `ModelRoot`, the read/write contexts and their `ValidationMode` policy, the Schema2 logical-resource graph, the Memory typed array views, the Toolkit `SceneBuilder`/`MeshBuilder`/`MaterialBuilder` author path, and the Runtime `SceneTemplate`/`MeshDecoder` decode surface all resolve there — a member verified against that catalogue is verified for this partition, and re-tabling one here forks the branch's glTF truth.
- Registers the Tiles3D emitter surface(`libs/csharp/Rasm.Bim/.api/api-sharpgltf-3dtiles.md`): the `EXTStructuralMetadataRoot` schema model, the property table/texture/attribute storage builders, the `MeshExtMeshFeatures`/`MeshExtInstanceFeatures` feature-ID family, and every `Tiles3DExtensions` binding static resolve there; `Rasm.Bim/Exchange/export#TILE_METADATA` is the one authoring fence.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the one surface this partition adds — composition-root extension admission

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `Tiles3DExtensions.RegisterExtensions()` | static  | seats the Tiles3D extension types on Core's `ExtensionsFactory` |

- Idempotent at the factory and run ONCE at composition, ahead of any read or write touching a Tiles3D extension; a per-tile or per-call registration is the deleted form.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The octree at `Runtime/codecs#TILE_PARTITION` owns the tile pyramid, the geometric-error ladder, the content keys, and the tileset.json manifest it writes through its own pooled `Utf8JsonWriter`; it yields one typed `LeafContent` per leaf naming a `{contentKey:x32}.glb` URI and emits no glTF body, so no `ModelRoot`, `SceneBuilder`, or `MeshDecoder` member is reachable from this partition.
- `TileMetadata`, `PropertyTable`, and `MetadataProperty` at that page are Rasm records over the seam graph, never the same-named `SharpGLTF.Schema2.Tiles3D` types — the Rasm columns lower onto the package types at the Bim authoring fence alone.

[STACKING]:
- `api-alimer-meshoptimizer`(`libs/csharp/.api/api-alimer-meshoptimizer.md`): the leaf-tile LOD and `EXT_meshopt_compression` encode run over the authored buffer views — SharpGLTF owns the schema, the sibling owns the codec, and neither crosses into the other's partition.

[LOCAL_ADMISSION]:
- Every SharpGLTF member a Compute fence spells verifies against one of the two Bim catalogues named above; an unverifiable member is a RESEARCH row on the composing page, never a row minted here.
- The Tiles3D registration is the whole admission: it enters at composition, never inside the partition fold.

[RAIL_LAW]:
- Packages: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, `SharpGLTF.Ext.3DTiles` (direct); `SharpGLTF.Runtime` (transitive)
- Owns: the composition-root `Tiles3DExtensions.RegisterExtensions()` admission for the tile-partition lane
- Accept: that one registration, and member verification delegated to the two Bim owners
- Reject: a Compute-side member roster for any SharpGLTF package, a leaf-tile glTF body emitted here, a hand-authored `JsonSerializable` extension over the raw registration, and a second glTF or Tiles3D rail beside the Bim owners
