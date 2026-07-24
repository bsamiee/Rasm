# [TS_UI_API_DECK_GL_MESH_LAYERS]

`@deck.gl/mesh-layers` instances a 3D asset over N anchors through two primitive `Layer` subclasses — `SimpleMeshLayer` for one arbitrary mesh, `ScenegraphLayer` for a full glTF scenegraph — on one shared placement axis, so a 3D-asset-over-map need is one parameterized choice, not two prop vocabularies. `@deck.gl/geo-layers` `Tile3DLayer` renders its 3D-tile content through these, so this tier resolves or mesh tiles fail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@deck.gl/mesh-layers`
- package: `@deck.gl/mesh-layers` (MIT)
- abi: browser WebGL2/WebGPU via `@deck.gl/core`'s luma.gl `Device`; glTF/mesh parse worker-backed via loaders.gl
- runtime: `scope:viewer` project-local; layer instances are declarative values in `Deck.layers`
- modules: `SimpleMeshLayer`, `ScenegraphLayer`

## [02]-[INSTANCED_MESH_ROWS]

[TYPE_SCOPE]: two `Layer<DataT>` subclasses instancing one asset over N anchors — an arbitrary mesh or a full glTF scenegraph.

| [INDEX] | [SYMBOL]                 | [CONSUMER_BOUNDARY]                                                               |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `SimpleMeshLayer<DataT>` | one arbitrary mesh instanced N ways; BIM extrusions, `Tile3DLayer` mesh tiles     |
|  [02]   | `ScenegraphLayer<DataT>` | full glTF scene instanced N ways; `Tile3DLayer` scenegraph tiles, animated assets |

- `ScenegraphLayer.onFirstDraw` fires once after first render — the visibility signal `Tile3DLayer` reads to safely deselect a parent tile mid-transition.

[DISTINCTIVE_SURFACE] by row:
- [01]-[SIMPLEMESHLAYER]: `mesh: string|Mesh|Promise<Mesh>|null` (`Mesh = Geometry | {attributes: MeshAttributes, indices?} | MeshAttributes`), `texture?`, `textureParameters?: SamplerProps`, `wireframe`, `material`, `_instanced`.
- [02]-[SCENEGRAPHLAYER]: `scenegraph` (glTF url/parsed), `getScene?`, `getAnimator?`, `_animations`, `_lighting: 'flat'|'pbr'`, `_imageBasedLightingEnvironment: PBREnvironment|fn`, `sizeMinPixels`/`sizeMaxPixels`.

## [03]-[INSTANCE_TRANSFORM_FAMILY]

[TYPE_SCOPE]: shared placement axis both layers inherit — one anchor accessor, the orientation/scale/translation triple or a matrix override, and a size multiplier; identical semantics across both layers.

| [INDEX] | [SYMBOL]                      | [TYPE]                                         | [ROLE]                                         |
| :-----: | :---------------------------- | :--------------------------------------------- | :--------------------------------------------- |
|  [01]   | `getPosition`                 | `Accessor<DataT, Position>`                    | the world anchor each instance is placed at    |
|  [02]   | `getOrientation`              | `Accessor<DataT, [pitch,yaw,roll]>` (°, Euler) | per-instance rotation                          |
|  [03]   | `getScale` / `getTranslation` | `Accessor<DataT, [x,y,z]>`                     | per-axis scale factor + offset (m) from anchor |
|  [04]   | `getTransformMatrix`          | `Accessor<DataT, number[]>` (4×4)              | full transform; overrides the o/s/t triple     |
|  [05]   | `sizeScale`                   | `number` (default 1)                           | global multiplier over every instance          |
|  [06]   | `getColor`                    | `Accessor<DataT, Color>`                       | per-instance tint; ignored when `texture` set  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one placement pattern, two payloads: both layers share the `getPosition` + orientation/scale/translation/matrix + `sizeScale` axis; `SimpleMeshLayer` fixes the payload to one mesh, `ScenegraphLayer` to one glTF scene, and a new instanced-asset need chooses between the two.
- matrix XOR triple: `getTransformMatrix` and the orientation/scale/translation triple are one axis — supply one, and the matrix silently overrides the triple.
- worker-backed parse: `scenegraph` and mesh URLs decode through loaders.gl worker pools (Draco/meshopt/KTX2), keeping CPU-heavy decode off the main thread by construction.
- overlay knobs underscore-prefixed: `_instanced`, `_animations`, `_lighting`, `_imageBasedLightingEnvironment` are advanced; instance the plain transform accessors first.

[STACKING]:
- `@deck.gl/core`(`.api/deck.gl-core.md`): both are primitive `Layer` subclasses — transform accessors are core `Accessor`s, positions `Position`, colors `Color`, `material` a core `Material` lit by `LightingEffect`, `data` a `LayerDataSource`; a pick bubbles the core `PickingInfo` (`info.index`/`info.object`).
- `@deck.gl/geo-layers`(`.api/deck.gl-geo-layers.md`): `Tile3DLayer` renders its 3D-tile content through these — `SimpleMeshLayer` for mesh/pnts tiles, `ScenegraphLayer` for scenegraph tiles — reading `ScenegraphLayer.onFirstDraw` as its transition-safety signal.
- `@google/model-viewer`(`.api/google-model-viewer.md`): the `viewer/scene/glb` plane owns single-model GLB display, and mesh-layers owns the deck-composited path for many georeferenced glTF instances, sharing meshopt/Draco decode via loaders.gl.
- `@effect-atom` + `Scope`: layer instances are atom-derived values pushed at `Deck.setProps`; `_animations` runs under `Deck._animate` (or `MapboxOverlay._animate`) from an rAF-fed atom clock, like `TripsLayer.currentTime`.
- picking → selection: an instance pick flows `PickingInfo.index`/`object` → a `mark/selection` `GlobalId`; `pickable`/`autoHighlight` are the core toggles.

[LOCAL_ADMISSION]:
- imported only inside `ui/viewer` (`scope:viewer`); heavy (glTF parse + PBR shading), compile-time excluded from the non-spatial core.
- required peer of `@deck.gl/geo-layers` `Tile3DLayer`; central admission resolves 3D-tile mesh rendering concretely.

[RAIL_LAW]:
- Package: `@deck.gl/mesh-layers`
- Owns: the instanced 3D-mesh (`SimpleMeshLayer`) and glTF-scenegraph (`ScenegraphLayer`) layers, the shared `getPosition`+orientation/scale/translation/matrix+`sizeScale` transform axis, PBR `material` + glTF animation/IBL, and the `Tile3DLayer` mesh/scenegraph render seam
- Accept: one of the two layers per instanced-asset need over the shared axis, `getTransformMatrix` or the orientation/scale/translation triple, worker-backed glTF parse, `_animations` under an atom clock + `_animate`, instance pick → `GlobalId`
- Reject: a third transform vocabulary, supplying both `getTransformMatrix` and the triple, re-deriving placement props per layer, importing luma.gl/loaders.gl directly instead of through deck props, hand-parsing glTF off the worker pool
