# [API_CATALOGUE] three

`three` supplies the core 3D scene graph, renderers, math primitives, materials, geometries, lights, cameras, loaders, animation, and WebGPU/TSL node-material surfaces consumed by the `ui` stack. The main entry (`three`) exports the full classic WebGL pipeline; `three/webgpu` re-exports the core plus `WebGPURenderer`, `WebGPUBackend`, node materials, and post-processing; `three/tsl` exposes the Three Shading Language (TSL) node function catalog.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package: `three`
- module: `three` + `three/webgpu` + `three/tsl` + `three/addons/*`
- asset: scene graph, renderers, math, materials, geometries, lights, cameras, loaders, animation, WebGPU backend
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph root objects
- rail: render

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :-------------- | :--------------- | :----------------------------------- |
|  [01]   | `Object3D`      | scene node base  | position, rotation, scale, hierarchy |
|  [02]   | `Scene`         | scene root       | background, fog, environment         |
|  [03]   | `Group`         | container node   | logical grouping of objects          |
|  [04]   | `Mesh`          | renderable node  | geometry + material pair             |
|  [05]   | `InstancedMesh` | GPU-instanced    | high-count object instancing         |
|  [06]   | `BatchedMesh`   | batched draw     | multi-geometry batching              |
|  [07]   | `SkinnedMesh`   | skinned mesh     | skeletal animation target            |
|  [08]   | `Skeleton`      | bone rig         | joints for skinned mesh              |
|  [09]   | `Sprite`        | billboard object | always-faces-camera quad             |
|  [10]   | `LOD`           | level-of-detail  | distance-switched geometry           |
|  [11]   | `Line`          | line primitive   | continuous line strip                |
|  [12]   | `Points`        | point cloud      | discrete point geometry              |

[PUBLIC_TYPE_SCOPE]: cameras
- rail: render

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                           |
| :-----: | :------------------- | :----------------- | :------------------------------- |
|  [01]   | `Camera`             | abstract base      | view matrix, projection matrix   |
|  [02]   | `PerspectiveCamera`  | perspective cam    | `fov`, `aspect`, `near`, `far`   |
|  [03]   | `OrthographicCamera` | orthographic cam   | `left`, `right`, `top`, `bottom` |
|  [04]   | `ArrayCamera`        | multi-viewport cam | sub-camera array for VR          |
|  [05]   | `CubeCamera`         | cube-map cam       | 6-face render for reflections    |
|  [06]   | `StereoCamera`       | stereo pair        | anaglyph / side-by-side stereo   |

[PUBLIC_TYPE_SCOPE]: lights
- rail: render

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :----------------- | :------------ | :---------------------------- |
|  [01]   | `AmbientLight`     | ambient       | uniform scene illumination    |
|  [02]   | `DirectionalLight` | directional   | infinite parallel rays        |
|  [03]   | `PointLight`       | point         | omnidirectional point source  |
|  [04]   | `SpotLight`        | spot          | cone-constrained point source |
|  [05]   | `HemisphereLight`  | hemisphere    | sky/ground gradient light     |
|  [06]   | `RectAreaLight`    | area          | rectangular emitter           |
|  [07]   | `LightProbe`       | probe         | SH-encoded indirect light     |

[PUBLIC_TYPE_SCOPE]: materials
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                           |
| :-----: | :--------------------- | :--------------- | :------------------------------- |
|  [01]   | `Material`             | material base    | blend, side, opacity, depthTest  |
|  [02]   | `MeshBasicMaterial`    | unlit flat       | color / map, no lighting         |
|  [03]   | `MeshLambertMaterial`  | diffuse          | Lambertian reflectance           |
|  [04]   | `MeshPhongMaterial`    | Phong            | specular highlight               |
|  [05]   | `MeshStandardMaterial` | PBR standard     | metalness, roughness             |
|  [06]   | `MeshPhysicalMaterial` | PBR extended     | clearcoat, sheen, transmission   |
|  [07]   | `MeshToonMaterial`     | toon / cel       | gradient map shading             |
|  [08]   | `ShaderMaterial`       | custom GLSL      | `vertexShader`, `fragmentShader` |
|  [09]   | `RawShaderMaterial`    | raw custom GLSL  | no built-in uniforms injected    |
|  [10]   | `MeshNormalMaterial`   | normal vis       | RGB-encoded normals              |
|  [11]   | `PointsMaterial`       | point size/color | point cloud rendering            |
|  [12]   | `SpriteMaterial`       | sprite           | billboard texture                |

[PUBLIC_TYPE_SCOPE]: geometries
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :------------------ | :--------------- | :------------------------------ |
|  [01]   | `BufferGeometry`    | geometry base    | attributes, indices, draw range |
|  [02]   | `BoxGeometry`       | box primitive    | width/height/depth segments     |
|  [03]   | `SphereGeometry`    | sphere primitive | radius, width/height segments   |
|  [04]   | `PlaneGeometry`     | plane primitive  | width, height, segments         |
|  [05]   | `CylinderGeometry`  | cylinder         | radii, height, segments         |
|  [06]   | `TorusGeometry`     | torus            | radius, tube, segments          |
|  [07]   | `ExtrudeGeometry`   | extrusion        | shape + extrude options         |
|  [08]   | `EdgesGeometry`     | edge lines       | edge detection from geometry    |
|  [09]   | `WireframeGeometry` | wireframe        | wireframe from geometry         |

[PUBLIC_TYPE_SCOPE]: math primitives
- rail: render

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [RAIL]                             |
| :-----: | :----------- | :-------------- | :--------------------------------- |
|  [01]   | `Vector2`    | 2D vector       | `x`, `y`, 2D math operations       |
|  [02]   | `Vector3`    | 3D vector       | `x`, `y`, `z`, 3D math operations  |
|  [03]   | `Vector4`    | 4D vector       | `x`, `y`, `z`, `w`                 |
|  [04]   | `Quaternion` | rotation        | `x`, `y`, `z`, `w`, SLERP          |
|  [05]   | `Matrix3`    | 3x3 matrix      | normal matrix operations           |
|  [06]   | `Matrix4`    | 4x4 matrix      | transform / projection matrix      |
|  [07]   | `Euler`      | rotation angles | `x`, `y`, `z`, `order`             |
|  [08]   | `Color`      | RGB color       | `r`, `g`, `b`, CSS/hex parsing     |
|  [09]   | `Box3`       | AABB            | min/max, intersection, containment |
|  [10]   | `Ray`        | ray             | origin, direction, intersection    |
|  [11]   | `Plane`      | plane           | normal, constant, distance         |
|  [12]   | `Sphere`     | bounding sphere | center, radius                     |
|  [13]   | `Frustum`    | view frustum    | 6-plane containment test           |
|  [14]   | `MathUtils`  | math utilities  | `clamp`, `lerp`, `degToRad`, UUID  |

[PUBLIC_TYPE_SCOPE]: textures
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                      |
| :-----: | :------------------ | :------------- | :-------------------------- |
|  [01]   | `Texture`           | texture base   | image, wrapping, filtering  |
|  [02]   | `CubeTexture`       | cube map       | 6-face environment / skybox |
|  [03]   | `DataTexture`       | buffer texture | typed-array pixel data      |
|  [04]   | `CanvasTexture`     | canvas-backed  | `HTMLCanvasElement` source  |
|  [05]   | `VideoTexture`      | video-backed   | `HTMLVideoElement` source   |
|  [06]   | `CompressedTexture` | GPU-compressed | KTX2 / BCn / ETC2 / ASTC    |
|  [07]   | `DepthTexture`      | depth buffer   | shadow map / depth readback |

[PUBLIC_TYPE_SCOPE]: animation
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                         |
| :-----: | :---------------- | :-------------- | :----------------------------- |
|  [01]   | `AnimationClip`   | clip asset      | named track collection         |
|  [02]   | `AnimationMixer`  | playback engine | clip actions on scene root     |
|  [03]   | `AnimationAction` | action handle   | play, pause, weight, timeScale |
|  [04]   | `KeyframeTrack`   | track base      | timed value channel            |
|  [05]   | `AnimationUtils`  | clip utilities  | subclip, linearize             |

[PUBLIC_TYPE_SCOPE]: renderers
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                  |
| :-----: | :------------------ | :--------------- | :-------------------------------------- |
|  [01]   | `WebGLRenderer`     | WebGL 2 renderer | classic raster pipeline                 |
|  [02]   | `WebGPURenderer`    | WebGPU renderer  | `three/webgpu` — modern GPU pipeline    |
|  [03]   | `WebGPUBackend`     | WebGPU backend   | `three/webgpu` — native WebGPU device   |
|  [04]   | `WebGLBackend`      | WebGL fallback   | `three/webgpu` — WebGL fallback backend |
|  [05]   | `WebGLRenderTarget` | off-screen RT    | classic FBO                             |
|  [06]   | `PostProcessing`    | post pipeline    | `three/webgpu` — effect chain           |

[PUBLIC_TYPE_SCOPE]: loaders
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :--------------------- | :--------------- | :----------------------------- |
|  [01]   | `Loader`               | loader base      | `load`, `loadAsync`, `setPath` |
|  [02]   | `TextureLoader`        | image loader     | loads PNG/JPG to `Texture`     |
|  [03]   | `FileLoader`           | raw file loader  | text / binary fetch            |
|  [04]   | `ObjectLoader`         | scene JSON       | three.js JSON scene format     |
|  [05]   | `BufferGeometryLoader` | geometry JSON    | buffer geometry JSON           |
|  [06]   | `ImageLoader`          | image element    | raw `HTMLImageElement` load    |
|  [07]   | `AudioLoader`          | audio buffer     | `AudioBuffer` load             |
|  [08]   | `LoadingManager`       | progress manager | multi-loader progress tracking |

[PUBLIC_TYPE_SCOPE]: core utilities
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                             |
| :-----: | :---------------- | :--------------- | :--------------------------------- |
|  [01]   | `Raycaster`       | ray picker       | `setFromCamera`, `intersectObject` |
|  [02]   | `Clock`           | delta timer      | `getDelta`, `getElapsedTime`       |
|  [03]   | `EventDispatcher` | event bus        | typed event emit/listen on objects |
|  [04]   | `Layers`          | layer bitmask    | visibility filtering by camera/obj |
|  [05]   | `BufferAttribute` | typed array attr | geometry vertex attribute          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Object3D — scene graph operations
- rail: render

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------- | :------------- | :--------------------------- |
|  [01]   | `add(...objects)`          | hierarchy      | appends children             |
|  [02]   | `remove(...objects)`       | hierarchy      | detaches children            |
|  [03]   | `attach(object)`           | hierarchy      | reparent preserving world xf |
|  [04]   | `lookAt(x, y, z)`          | transform      | orient toward point          |
|  [05]   | `applyMatrix4(matrix)`     | transform      | accumulate transform         |
|  [06]   | `translateX/Y/Z(distance)` | transform      | local-axis translation       |
|  [07]   | `rotateX/Y/Z(angle)`       | transform      | local-axis rotation          |
|  [08]   | `localToWorld(vector)`     | space convert  | local -> world coordinates   |
|  [09]   | `worldToLocal(vector)`     | space convert  | world -> local coordinates   |
|  [10]   | `getObjectByName(name)`    | query          | depth-first name search      |
|  [11]   | `traverse(callback)`       | traversal      | depth-first node visit       |
|  [12]   | `clone(recursive?)`        | duplication    | shallow or deep clone        |

[ENTRYPOINT_SCOPE]: WebGLRenderer — render pipeline
- rail: render

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------- | :------------- | :------------------------ |
|  [01]   | `render(scene, camera)`                | render call    | draws one frame           |
|  [02]   | `setSize(width, height, updateStyle?)` | viewport       | canvas dimensions         |
|  [03]   | `setPixelRatio(value)`                 | viewport       | DPR for retina            |
|  [04]   | `getSize(target)`                      | viewport query | current canvas size       |
|  [05]   | `setClearColor(...)`                   | clear state    | background color + alpha  |
|  [06]   | `clear(color?, depth?, stencil?)`      | clear          | selective buffer clear    |
|  [07]   | `setAnimationLoop(callback)`           | loop control   | rAF-driven render loop    |
|  [08]   | `setRenderTarget(rt, face?, mip?)`     | FBO switch     | off-screen render target  |
|  [09]   | `compile(scene, camera, targetScene?)` | shader compile | pre-compile materials     |
|  [10]   | `readRenderTargetPixels(...)`          | readback       | GPU -> CPU pixel read     |
|  [11]   | `dispose()`                            | cleanup        | release all GPU resources |

[ENTRYPOINT_SCOPE]: PerspectiveCamera
- rail: render

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `new PerspectiveCamera(fov, aspect, near, far)` | constructor    | perspective projection matrix |
|  [02]   | `updateProjectionMatrix()`                      | matrix update  | apply changed fov/aspect      |
|  [03]   | `setFocalLength(focalLength)`                   | film camera    | 35mm equivalent focal length  |
|  [04]   | `setViewOffset(fw, fh, x, y, w, h)`             | tile render    | sub-viewport offset           |

[ENTRYPOINT_SCOPE]: BufferGeometry
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :---------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `setAttribute(name, attribute)`     | attribute write | register vertex attribute     |
|  [02]   | `getAttribute(name)`                | attribute read  | retrieve named attribute      |
|  [03]   | `setIndex(index)`                   | index buffer    | set triangle indices          |
|  [04]   | `addGroup(start, count, matIndex?)` | draw group      | multi-material draw ranges    |
|  [05]   | `computeBoundingBox()`              | bounds          | populate `boundingBox`        |
|  [06]   | `computeBoundingSphere()`           | bounds          | populate `boundingSphere`     |
|  [07]   | `computeVertexNormals()`            | normals         | smooth normals from positions |
|  [08]   | `applyMatrix4(matrix)`              | transform bake  | bake transform into vertices  |
|  [09]   | `toNonIndexed()`                    | conversion      | expand indexed to flat        |
|  [10]   | `clone()`                           | duplication     | deep attribute clone          |
|  [11]   | `dispose()`                         | cleanup         | release GPU buffer objects    |

[ENTRYPOINT_SCOPE]: AnimationMixer
- rail: render

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|  [01]   | `new AnimationMixer(root)`        | constructor    | bind mixer to scene root        |
|  [02]   | `clipAction(clip, optionalRoot?)` | action factory | get or create `AnimationAction` |
|  [03]   | `update(deltaTime)`               | tick           | advance all active actions      |
|  [04]   | `stopAllAction()`                 | playback ctrl  | halt every active action        |
|  [05]   | `uncacheClip(clip)`               | cache evict    | remove clip from action cache   |

[ENTRYPOINT_SCOPE]: Raycaster
- rail: render

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :--------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `setFromCamera(coords, camera)`                | ray setup      | NDC mouse -> world ray   |
|  [02]   | `intersectObject(obj, recursive?, results?)`   | intersection   | near-to-far sorted hits  |
|  [03]   | `intersectObjects(objs, recursive?, results?)` | intersection   | multi-object sorted hits |

## [04]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:
- `three` (main): `WebGLRenderer` + full scene graph, math, material, geometry, loader, animation catalog
- `three/webgpu`: re-exports core plus `WebGPURenderer`, `WebGPUBackend`, `WebGLBackend`, node material family, `PostProcessing`
- `three/tsl`: TSL node function catalog (`Fn`, `If`, `Loop`, `vec3`, `float`, etc.) for shader authoring in JavaScript
- `three/addons/*`: optional extras (controls, loaders, exporters, postprocessing effects) — not bundled in main entry
- `WebGLRenderer` uses WebGL 2 exclusively; the `domElement` canvas is auto-created unless `canvas` is passed in options
- `setAnimationLoop(null)` stops the render loop; callback receives elapsed time in milliseconds

[LOCAL_ADMISSION]:
- Import from `three` for WebGL pipeline; import from `three/webgpu` for `WebGPURenderer` and node materials.
- `Texture`, `BufferGeometry`, and `WebGLRenderTarget` must call `dispose()` when no longer used.
- `AnimationMixer.update(delta)` must be called each frame inside the render loop for animation to advance.
- `BufferAttribute` typed arrays must match the declared component type; mismatched component size causes silent corruption.
- `PerspectiveCamera.updateProjectionMatrix()` must be called after changing `fov`, `aspect`, `near`, or `far`.
- `ColorManagement` is enabled by default in r150+; sRGB input textures must set `colorSpace = SRGBColorSpace`.

[RAIL_LAW]:
- package: `three`
- Owns: scene graph, WebGL/WebGPU render pipelines, math primitives, materials, geometries, lights, cameras, textures, loaders, animation
- Accept: `three`, `three/webgpu`, `three/tsl`, `three/addons/*` subpath imports
- Reject: hand-rolled WebGL state management, custom matrix math duplicating `Matrix4`/`Quaternion`, redundant texture-format utilities
