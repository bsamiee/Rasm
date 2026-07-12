# [TS_UI_API_DECK_GL_MESH_LAYERS]

`@deck.gl/mesh-layers` is the 3D-geometry render tier the `ui/viewer/geo` plane composes over `@deck.gl/core`: two primitive `Layer` subclasses that place N georeferenced instances of a 3D asset at anchor positions — `SimpleMeshLayer` instances ONE arbitrary mesh (a luma.gl `Geometry`, a loaders.gl mesh, or raw `MeshAttributes`) with an optional texture + PBR `material`, and `ScenegraphLayer` instances a COMPLETE glTF scenegraph with per-node animation and image-based lighting. Both layers share one instance-transform axis — `getPosition` anchor plus the `getOrientation`([pitch,yaw,roll]°)/`getScale`/`getTranslation` triple (or a single `getTransformMatrix` that overrides all three) and a `sizeScale` multiplier — so "place a 3D asset over the map" is one parameterized pattern, not two prop vocabularies. This is the peer `@deck.gl/geo-layers` `Tile3DLayer` renders its b3dm/glTF/i3s tile content through (`SimpleMeshLayer` for mesh tiles, `ScenegraphLayer` for full scenegraphs), so it must resolve or 3D-tile meshes fail. This catalog documents the two layers and the shared transform family, deferring the base `Layer`/`Accessor`/`Position`/`Color`/`Material` surface to `.api/deck.gl-core.md`. `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/mesh-layers`
- package: `@deck.gl/mesh-layers`
- license: `MIT`
- abi: browser WebGL2/WebGPU via `@deck.gl/core`'s luma.gl `Device`; glTF parse worker-backed via loaders.gl
- peer (`~catalog`): `@deck.gl/core` (`Layer`, `Material`, `Accessor`, `Position`, `Color`, `LayerProps`, `LayerDataSource`), `@luma.gl/core` (`Texture`/`SamplerProps`/`Device`), `@luma.gl/engine` (`Model`/`Geometry`/`GroupNode`), `@luma.gl/gltf` (`ParsedPBRMaterial`/`GLTFAnimator`/`PBREnvironment`), `@luma.gl/shadertools`; deps `@loaders.gl/gltf` (scenegraph loader), `@loaders.gl/schema` (`MeshAttributes`/`MeshAttribute`)
- catalog-verdict: KEEP — the required `@deck.gl/geo-layers` `Tile3DLayer` mesh peer + the instanced-glTF path; admitted centrally, no lower-level substitute
- runtime: `scope:viewer` project-local; layer instances are declarative values in `Deck.layers`, mesh/glTF parse is async + worker-backed
- modules: `SimpleMeshLayer`, `ScenegraphLayer`

## [02]-[INSTANCED_MESH_ROWS]

[TYPE_SCOPE]: two primitive `Layer` subclasses instancing a 3D asset — one arbitrary mesh (`SimpleMeshLayer`) or a full glTF scenegraph (`ScenegraphLayer`) — over N anchor positions; both are generic over the row type `DataT`.
- `SimpleMeshLayer` uploads one `mesh` and draws it per data object, mixing a per-object `getColor` with vertex colors (or a `texture`), with `wireframe` + a PBR `material`; `ScenegraphLayer` resolves a `scenegraph` glTF, walks its node hierarchy, and drives per-node `_animations` under a `_lighting: 'flat'|'pbr'` path with optional `_imageBasedLightingEnvironment`. `ScenegraphLayer.onFirstDraw` is the visibility signal `Tile3DLayer` reads to safely deselect a parent tile mid-transition.

| [INDEX] | [SYMBOL]                 | [DISTINCTIVE_SURFACE]                                                                                                                                                                                                               | [CONSUMER_BOUNDARY]                                                                            |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `SimpleMeshLayer<DataT>` | `mesh: string\|Mesh\|Promise<Mesh>\|null` (`Mesh = Geometry\|{attributes:MeshAttributes,indices?}\|MeshAttributes`), `texture?`, `textureParameters?: SamplerProps`, `getColor`, `wireframe`, `material`, `sizeScale`, `_instanced` | one arbitrary mesh instanced N ways; BIM element extrusions, markers, `Tile3DLayer` mesh tiles |
|  [02]   | `ScenegraphLayer<DataT>` | `scenegraph` (glTF url/parsed), `getScene?`, `getAnimator?`, `_animations`, `_lighting:'flat'\|'pbr'`, `_imageBasedLightingEnvironment: PBREnvironment\|fn`, `onFirstDraw`, `sizeScale`, `sizeMinPixels`/`sizeMaxPixels`            | a full glTF scene instanced N ways; `Tile3DLayer` scenegraph tiles, animated 3D assets         |

## [03]-[INSTANCE_TRANSFORM_FAMILY]

[TYPE_SCOPE]: the shared placement axis both layers inherit — one anchor accessor plus an orientation/scale/translation triple (or a single matrix override) and a size multiplier; identical semantics across both layers, never re-derived per layer.
- `getTransformMatrix` is mutually exclusive with the triple: when supplied it overrides `getOrientation`/`getScale`/`getTranslation`. Each accessor is a core `Accessor<In,Out>` (uniform value or per-object function) memoized by `updateTriggers` exactly like any layer accessor.

| [INDEX] | [SYMBOL]                      | [TYPE]                                               | [ROLE]                                                                  |
| :-----: | :---------------------------- | :--------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `getPosition`                 | `Accessor<DataT, Position>`                          | the world anchor each instance is placed at                             |
|  [02]   | `getOrientation`              | `Accessor<DataT, [pitch,yaw,roll]>` (degrees, Euler) | per-instance rotation                                                   |
|  [03]   | `getScale` / `getTranslation` | `Accessor<DataT, [x,y,z]>` (translation in meters)   | per-instance scale + offset from anchor                                 |
|  [04]   | `getTransformMatrix`          | `Accessor<DataT, number[]>` (4×4)                    | full transform; OVERRIDES orientation/scale/translation                 |
|  [05]   | `sizeScale`                   | `number` (default 1)                                 | global multiplier over every instance                                   |
|  [06]   | `getColor`                    | `Accessor<DataT, Color>`                             | per-instance tint mixed with vertex colors (ignored when `texture` set) |

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- one placement pattern, two payloads: both layers share the `getPosition` + `getOrientation`/`getScale`/`getTranslation`/`getTransformMatrix` + `sizeScale` transform axis; `SimpleMeshLayer` fixes payload = one mesh, `ScenegraphLayer` fixes payload = one glTF scene. A new instanced-asset need is a choice between these two, never a third transform vocabulary.
- matrix wins: `getTransformMatrix` and the orientation/scale/translation triple are one axis — supply the matrix OR the triple, never both; the matrix silently overrides.
- glTF parse is worker-backed: `ScenegraphLayer.scenegraph` and mesh URLs parse through loaders.gl worker pools (Draco/meshopt/KTX2), keeping CPU-heavy decode off the main thread by construction — mirrors the `browser` GLB decode-worker port.
- overlay knobs are underscored: `_instanced`, `_animations`, `_lighting`, `_imageBasedLightingEnvironment` are advanced — instance the plain transform accessors first.

[INTEGRATION_LAW]:
- Stack on `@deck.gl/core` (`.api/deck.gl-core.md`): both are primitive `Layer` subclasses — the transform accessors are core `Accessor`s, positions core `Position`, colors core `Color`, `material` a core `Material` lit by the core `LightingEffect`, and `data` a core `LayerDataSource`. Picking bubbles through the core `PickingInfo` (`info.index`/`info.object` → a BIM element instance).
- Stack into `@deck.gl/geo-layers` (`.api/deck.gl-geo-layers.md`): `Tile3DLayer` renders its b3dm/glTF/i3s tile content THROUGH these — `SimpleMeshLayer` for mesh/pnts tiles, `ScenegraphLayer` for scenegraph tiles; `ScenegraphLayer.onFirstDraw` is the transition-safety signal `Tile3DLayer` reads. This is the peer that "must resolve or `Tile3DLayer` fails".
- Stack beside `@google/model-viewer` (`.api/google-model-viewer.md`): the `viewer/scene/glb` plane owns single-model GLB display via `@google/model-viewer`; mesh-layers is the deck-composited path for MANY georeferenced glTF instances (BIM elements as instanced meshes over the basemap), sharing meshopt/Draco decode via loaders.gl.
- Stack with `@effect-atom` + a `Scope`: layer instances are pure atom-derived values pushed at `Deck.setProps`; `_animations` under `Deck._animate` (or `MapboxOverlay._animate`) drives glTF animation from an rAF-fed atom clock, exactly like `TripsLayer.currentTime`.
- Stack with picking→selection: an instance pick → `PickingInfo.index`/`object` → `mark/selection` `GlobalId` (a BIM element/asset instance); `pickable`/`autoHighlight` are the core toggles.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); heavy (glTF parse + PBR shading), compile-time excluded from the non-spatial core.
- treat the transform family as one shared axis — `SimpleMeshLayer` for one mesh instanced N ways, `ScenegraphLayer` for a full glTF scene instanced N ways; never fork the transform props per layer.
- `getTransformMatrix` overrides `getOrientation`/`getScale`/`getTranslation` — pick the matrix OR the triple.
- `_instanced`/`_animations`/`_lighting`/`_imageBasedLightingEnvironment` are overlay (underscore); reach for them only for delta-coordinate meshes, animated glTF, or PBR IBL.
- required peer of `@deck.gl/geo-layers` `Tile3DLayer`; admitting it centrally in `pnpm-workspace.yaml` is what lets 3D-tile mesh rendering resolve concretely.

[RAIL_LAW]:
- Package: `@deck.gl/mesh-layers`
- Owns: the instanced 3D-mesh (`SimpleMeshLayer`) and glTF-scenegraph (`ScenegraphLayer`) layers, the shared `getPosition`+orientation/scale/translation/matrix+`sizeScale` instance-transform axis, PBR `material` + glTF animation/IBL, and the `Tile3DLayer` mesh/scenegraph render seam
- Accept: one of the two layers per instanced-asset need over the shared transform axis, `getTransformMatrix` OR the orientation/scale/translation triple, worker-backed glTF parse, `_animations` under an atom clock + `_animate`, instance pick → `GlobalId`
- Reject: a third transform vocabulary beside the shared axis, supplying both `getTransformMatrix` and the triple, re-deriving placement props per layer, importing luma.gl/loaders.gl directly instead of through deck props, hand-parsing glTF off the worker pool
