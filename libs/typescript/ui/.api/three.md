# [API_CATALOGUE] three

`three` supplies the core 3D scene graph, renderers, math primitives, materials, geometries, lights, cameras, loaders, animation, and WebGPU/TSL node-material surfaces consumed by the `ui` stack. The main entry (`three`) exports the full classic WebGL pipeline; `three/webgpu` re-exports the core plus `WebGPURenderer`, `WebGPUBackend`, node materials, and post-processing; `three/tsl` exposes the Three Shading Language (TSL) node function catalog.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package: `three`
- module: `three` + `three/webgpu` + `three/tsl` + `three/addons/*`
- asset: scene graph, renderers, math, materials, geometries, lights, cameras, loaders, animation, WebGPU backend
- rail: render

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph root objects
- rail: render

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :-------------- | :--------------- | :----------------------------------- |
|   [1]   | `Object3D`      | scene node base  | position, rotation, scale, hierarchy |
|   [2]   | `Scene`         | scene root       | background, fog, environment         |
|   [3]   | `Group`         | container node   | logical grouping of objects          |
|   [4]   | `Mesh`          | renderable node  | geometry + material pair             |
|   [5]   | `InstancedMesh` | GPU-instanced    | high-count object instancing         |
|   [6]   | `BatchedMesh`   | batched draw     | multi-geometry batching              |
|   [7]   | `SkinnedMesh`   | skinned mesh     | skeletal animation target            |
|   [8]   | `Skeleton`      | bone rig         | joints for skinned mesh              |
|   [9]   | `Sprite`        | billboard object | always-faces-camera quad             |
|  [10]   | `LOD`           | level-of-detail  | distance-switched geometry           |
|  [11]   | `Line`          | line primitive   | continuous line strip                |
|  [12]   | `Points`        | point cloud      | discrete point geometry              |

[PUBLIC_TYPE_SCOPE]: cameras
- rail: render

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                           |
| :-----: | :------------------- | :----------------- | :------------------------------- |
|   [1]   | `Camera`             | abstract base      | view matrix, projection matrix   |
|   [2]   | `PerspectiveCamera`  | perspective cam    | `fov`, `aspect`, `near`, `far`   |
|   [3]   | `OrthographicCamera` | orthographic cam   | `left`, `right`, `top`, `bottom` |
|   [4]   | `ArrayCamera`        | multi-viewport cam | sub-camera array for VR          |
|   [5]   | `CubeCamera`         | cube-map cam       | 6-face render for reflections    |
|   [6]   | `StereoCamera`       | stereo pair        | anaglyph / side-by-side stereo   |

[PUBLIC_TYPE_SCOPE]: lights
- rail: render

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :----------------- | :------------ | :---------------------------- |
|   [1]   | `AmbientLight`     | ambient       | uniform scene illumination    |
|   [2]   | `DirectionalLight` | directional   | infinite parallel rays        |
|   [3]   | `PointLight`       | point         | omnidirectional point source  |
|   [4]   | `SpotLight`        | spot          | cone-constrained point source |
|   [5]   | `HemisphereLight`  | hemisphere    | sky/ground gradient light     |
|   [6]   | `RectAreaLight`    | area          | rectangular emitter           |
|   [7]   | `LightProbe`       | probe         | SH-encoded indirect light     |

[PUBLIC_TYPE_SCOPE]: materials
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                           |
| :-----: | :--------------------- | :--------------- | :------------------------------- |
|   [1]   | `Material`             | material base    | blend, side, opacity, depthTest  |
|   [2]   | `MeshBasicMaterial`    | unlit flat       | color / map, no lighting         |
|   [3]   | `MeshLambertMaterial`  | diffuse          | Lambertian reflectance           |
|   [4]   | `MeshPhongMaterial`    | Phong            | specular highlight               |
|   [5]   | `MeshStandardMaterial` | PBR standard     | metalness, roughness             |
|   [6]   | `MeshPhysicalMaterial` | PBR extended     | clearcoat, sheen, transmission   |
|   [7]   | `MeshToonMaterial`     | toon / cel       | gradient map shading             |
|   [8]   | `ShaderMaterial`       | custom GLSL      | `vertexShader`, `fragmentShader` |
|   [9]   | `RawShaderMaterial`    | raw custom GLSL  | no built-in uniforms injected    |
|  [10]   | `MeshNormalMaterial`   | normal vis       | RGB-encoded normals              |
|  [11]   | `PointsMaterial`       | point size/color | point cloud rendering            |
|  [12]   | `SpriteMaterial`       | sprite           | billboard texture                |

[PUBLIC_TYPE_SCOPE]: geometries
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :------------------ | :--------------- | :------------------------------ |
|   [1]   | `BufferGeometry`    | geometry base    | attributes, indices, draw range |
|   [2]   | `BoxGeometry`       | box primitive    | width/height/depth segments     |
|   [3]   | `SphereGeometry`    | sphere primitive | radius, width/height segments   |
|   [4]   | `PlaneGeometry`     | plane primitive  | width, height, segments         |
|   [5]   | `CylinderGeometry`  | cylinder         | radii, height, segments         |
|   [6]   | `TorusGeometry`     | torus            | radius, tube, segments          |
|   [7]   | `ExtrudeGeometry`   | extrusion        | shape + extrude options         |
|   [8]   | `EdgesGeometry`     | edge lines       | edge detection from geometry    |
|   [9]   | `WireframeGeometry` | wireframe        | wireframe from geometry         |

[PUBLIC_TYPE_SCOPE]: math primitives
- rail: render

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [RAIL]                             |
| :-----: | :----------- | :-------------- | :--------------------------------- |
|   [1]   | `Vector2`    | 2D vector       | `x`, `y`, 2D math operations       |
|   [2]   | `Vector3`    | 3D vector       | `x`, `y`, `z`, 3D math operations  |
|   [3]   | `Vector4`    | 4D vector       | `x`, `y`, `z`, `w`                 |
|   [4]   | `Quaternion` | rotation        | `x`, `y`, `z`, `w`, SLERP          |
|   [5]   | `Matrix3`    | 3x3 matrix      | normal matrix operations           |
|   [6]   | `Matrix4`    | 4x4 matrix      | transform / projection matrix      |
|   [7]   | `Euler`      | rotation angles | `x`, `y`, `z`, `order`             |
|   [8]   | `Color`      | RGB color       | `r`, `g`, `b`, CSS/hex parsing     |
|   [9]   | `Box3`       | AABB            | min/max, intersection, containment |
|  [10]   | `Ray`        | ray             | origin, direction, intersection    |
|  [11]   | `Plane`      | plane           | normal, constant, distance         |
|  [12]   | `Sphere`     | bounding sphere | center, radius                     |
|  [13]   | `Frustum`    | view frustum    | 6-plane containment test           |
|  [14]   | `MathUtils`  | math utilities  | `clamp`, `lerp`, `degToRad`, UUID  |

[PUBLIC_TYPE_SCOPE]: textures
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                      |
| :-----: | :------------------ | :------------- | :-------------------------- |
|   [1]   | `Texture`           | texture base   | image, wrapping, filtering  |
|   [2]   | `CubeTexture`       | cube map       | 6-face environment / skybox |
|   [3]   | `DataTexture`       | buffer texture | typed-array pixel data      |
|   [4]   | `CanvasTexture`     | canvas-backed  | `HTMLCanvasElement` source  |
|   [5]   | `VideoTexture`      | video-backed   | `HTMLVideoElement` source   |
|   [6]   | `CompressedTexture` | GPU-compressed | KTX2 / BCn / ETC2 / ASTC    |
|   [7]   | `DepthTexture`      | depth buffer   | shadow map / depth readback |

[PUBLIC_TYPE_SCOPE]: animation
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                         |
| :-----: | :---------------- | :-------------- | :----------------------------- |
|   [1]   | `AnimationClip`   | clip asset      | named track collection         |
|   [2]   | `AnimationMixer`  | playback engine | clip actions on scene root     |
|   [3]   | `AnimationAction` | action handle   | play, pause, weight, timeScale |
|   [4]   | `KeyframeTrack`   | track base      | timed value channel            |
|   [5]   | `AnimationUtils`  | clip utilities  | subclip, linearize             |

[PUBLIC_TYPE_SCOPE]: renderers
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                  |
| :-----: | :------------------ | :--------------- | :-------------------------------------- |
|   [1]   | `WebGLRenderer`     | WebGL 2 renderer | classic raster pipeline                 |
|   [2]   | `WebGPURenderer`    | WebGPU renderer  | `three/webgpu` — modern GPU pipeline    |
|   [3]   | `WebGPUBackend`     | WebGPU backend   | `three/webgpu` — native WebGPU device   |
|   [4]   | `WebGLBackend`      | WebGL fallback   | `three/webgpu` — WebGL fallback backend |
|   [5]   | `WebGLRenderTarget` | off-screen RT    | classic FBO                             |
|   [6]   | `PostProcessing`    | post pipeline    | `three/webgpu` — effect chain           |

[PUBLIC_TYPE_SCOPE]: loaders
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :--------------------- | :--------------- | :----------------------------- |
|   [1]   | `Loader`               | loader base      | `load`, `loadAsync`, `setPath` |
|   [2]   | `TextureLoader`        | image loader     | loads PNG/JPG to `Texture`     |
|   [3]   | `FileLoader`           | raw file loader  | text / binary fetch            |
|   [4]   | `ObjectLoader`         | scene JSON       | three.js JSON scene format     |
|   [5]   | `BufferGeometryLoader` | geometry JSON    | buffer geometry JSON           |
|   [6]   | `ImageLoader`          | image element    | raw `HTMLImageElement` load    |
|   [7]   | `AudioLoader`          | audio buffer     | `AudioBuffer` load             |
|   [8]   | `LoadingManager`       | progress manager | multi-loader progress tracking |

[PUBLIC_TYPE_SCOPE]: core utilities
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                             |
| :-----: | :---------------- | :--------------- | :--------------------------------- |
|   [1]   | `Raycaster`       | ray picker       | `setFromCamera`, `intersectObject` |
|   [2]   | `Clock`           | delta timer      | `getDelta`, `getElapsedTime`       |
|   [3]   | `EventDispatcher` | event bus        | typed event emit/listen on objects |
|   [4]   | `Layers`          | layer bitmask    | visibility filtering by camera/obj |
|   [5]   | `BufferAttribute` | typed array attr | geometry vertex attribute          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Object3D — scene graph operations
- rail: render

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------- | :------------- | :--------------------------- |
|   [1]   | `add(...objects)`          | hierarchy      | appends children             |
|   [2]   | `remove(...objects)`       | hierarchy      | detaches children            |
|   [3]   | `attach(object)`           | hierarchy      | reparent preserving world xf |
|   [4]   | `lookAt(x, y, z)`          | transform      | orient toward point          |
|   [5]   | `applyMatrix4(matrix)`     | transform      | accumulate transform         |
|   [6]   | `translateX/Y/Z(distance)` | transform      | local-axis translation       |
|   [7]   | `rotateX/Y/Z(angle)`       | transform      | local-axis rotation          |
|   [8]   | `localToWorld(vector)`     | space convert  | local -> world coordinates   |
|   [9]   | `worldToLocal(vector)`     | space convert  | world -> local coordinates   |
|  [10]   | `getObjectByName(name)`    | query          | depth-first name search      |
|  [11]   | `traverse(callback)`       | traversal      | depth-first node visit       |
|  [12]   | `clone(recursive?)`        | duplication    | shallow or deep clone        |

[ENTRYPOINT_SCOPE]: WebGLRenderer — render pipeline
- rail: render

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------- | :------------- | :------------------------ |
|   [1]   | `render(scene, camera)`                | render call    | draws one frame           |
|   [2]   | `setSize(width, height, updateStyle?)` | viewport       | canvas dimensions         |
|   [3]   | `setPixelRatio(value)`                 | viewport       | DPR for retina            |
|   [4]   | `getSize(target)`                      | viewport query | current canvas size       |
|   [5]   | `setClearColor(...)`                   | clear state    | background color + alpha  |
|   [6]   | `clear(color?, depth?, stencil?)`      | clear          | selective buffer clear    |
|   [7]   | `setAnimationLoop(callback)`           | loop control   | rAF-driven render loop    |
|   [8]   | `setRenderTarget(rt, face?, mip?)`     | FBO switch     | off-screen render target  |
|   [9]   | `compile(scene, camera, targetScene?)` | shader compile | pre-compile materials     |
|  [10]   | `readRenderTargetPixels(...)`          | readback       | GPU -> CPU pixel read     |
|  [11]   | `dispose()`                            | cleanup        | release all GPU resources |

[ENTRYPOINT_SCOPE]: PerspectiveCamera
- rail: render

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `new PerspectiveCamera(fov, aspect, near, far)` | constructor    | perspective projection matrix |
|   [2]   | `updateProjectionMatrix()`                      | matrix update  | apply changed fov/aspect      |
|   [3]   | `setFocalLength(focalLength)`                   | film camera    | 35mm equivalent focal length  |
|   [4]   | `setViewOffset(fw, fh, x, y, w, h)`             | tile render    | sub-viewport offset           |

[ENTRYPOINT_SCOPE]: BufferGeometry
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :---------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `setAttribute(name, attribute)`     | attribute write | register vertex attribute     |
|   [2]   | `getAttribute(name)`                | attribute read  | retrieve named attribute      |
|   [3]   | `setIndex(index)`                   | index buffer    | set triangle indices          |
|   [4]   | `addGroup(start, count, matIndex?)` | draw group      | multi-material draw ranges    |
|   [5]   | `computeBoundingBox()`              | bounds          | populate `boundingBox`        |
|   [6]   | `computeBoundingSphere()`           | bounds          | populate `boundingSphere`     |
|   [7]   | `computeVertexNormals()`            | normals         | smooth normals from positions |
|   [8]   | `applyMatrix4(matrix)`              | transform bake  | bake transform into vertices  |
|   [9]   | `toNonIndexed()`                    | conversion      | expand indexed to flat        |
|  [10]   | `clone()`                           | duplication     | deep attribute clone          |
|  [11]   | `dispose()`                         | cleanup         | release GPU buffer objects    |

[ENTRYPOINT_SCOPE]: AnimationMixer
- rail: render

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|   [1]   | `new AnimationMixer(root)`        | constructor    | bind mixer to scene root        |
|   [2]   | `clipAction(clip, optionalRoot?)` | action factory | get or create `AnimationAction` |
|   [3]   | `update(deltaTime)`               | tick           | advance all active actions      |
|   [4]   | `stopAllAction()`                 | playback ctrl  | halt every active action        |
|   [5]   | `uncacheClip(clip)`               | cache evict    | remove clip from action cache   |

[ENTRYPOINT_SCOPE]: Raycaster
- rail: render

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :--------------------------------------------- | :------------- | :----------------------- |
|   [1]   | `setFromCamera(coords, camera)`                | ray setup      | NDC mouse -> world ray   |
|   [2]   | `intersectObject(obj, recursive?, results?)`   | intersection   | near-to-far sorted hits  |
|   [3]   | `intersectObjects(objs, recursive?, results?)` | intersection   | multi-object sorted hits |

## [4]-[IMPLEMENTATION_LAW]

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
