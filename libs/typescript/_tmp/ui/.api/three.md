# [API_CATALOGUE] three

`three` supplies the scene graph, renderers, math primitives, materials, geometries, lights, cameras, textures, loaders, animation, and the modern WebGPU/TSL node-material surface the `ui` render leaves compose. The `three` entry exports the classic WebGL pipeline; `three/webgpu` re-exports the core plus `WebGPURenderer` (which auto-detects WebGPU and falls back to WebGL with zero config), the `WebGPUBackend`/`WebGLBackend`, the node-material family, and `PostProcessing`; `three/tsl` exposes the Three Shading Language node-function catalog for authoring shaders and GPU compute in TypeScript; `three/addons/*` carries the optional controls/loaders/exporters/post nodes. In `ui` this is one `RendererBackend` row of the GLB viewport: `render/glb.md#GLB_VIEWPORT` acquires the `WebGPURenderer` as an `effect` `Effect.acquireRelease` scoped resource, folds the proto-decoded `GeometryView` onto `BufferGeometry`, and disposes the context, buffers, and render-loop fiber LIFO on scope exit — the WebGL fallback is the renderer's own auto-detect, never a hand-rolled `navigator.gpu` degrade branch.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package / version: `three` @ `0.185.1`
- license: `MIT`
- module: ESM `build/three.module.js` + CJS `build/three.cjs`; `type: "module"`
- exports: `.` → classic WebGL pipeline; `./webgpu` → `build/three.webgpu.js` (`WebGPURenderer` + node materials + `PostProcessing`); `./tsl` → `build/three.tsl.js` (TSL node functions); `./addons` → `examples/jsm/Addons.js` barrel; `./addons/*` and `./examples/jsm/*` → the JSM tree; `./src/*` → source
- types: NO bundled `.d.ts` (build/ ships JS only, no `types` export condition) — the TypeScript surface is supplied by `@types/three` (DefinitelyTyped, matching minor), which covers the classic surface AND the `three/webgpu`/`three/tsl` node system; admit `@types/three` beside `three` in the workspace
- peer / dependency: none
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph root objects
- rail: render

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [NOTE]                               |
| :-----: | :-------------- | :--------------- | :----------------------------------- |
|  [01]   | `Object3D`      | scene node base  | position, rotation, scale, hierarchy |
|  [02]   | `Scene`         | scene root       | background, fog, environment; `backgroundNode`/`environmentNode` under WebGPU |
|  [03]   | `Group`         | container node   | logical grouping of objects          |
|  [04]   | `Mesh`          | renderable node  | geometry + material pair             |
|  [05]   | `InstancedMesh` | GPU-instanced    | high-count instancing via `instanceMatrix`/`setMatrixAt` |
|  [06]   | `BatchedMesh`   | batched draw     | multi-geometry batching in one draw call |
|  [07]   | `SkinnedMesh` / `Skeleton` | skinned mesh / rig | skeletal-animation target and its joint set |
|  [08]   | `Sprite` / `LOD` | billboard / level-of-detail | always-faces-camera quad; distance-switched geometry |
|  [09]   | `Line` / `Points` | line / point cloud | continuous strip; discrete point geometry |

[PUBLIC_TYPE_SCOPE]: cameras and lights
- rail: render

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [NOTE]                           |
| :-----: | :------------------- | :----------------- | :------------------------------- |
|  [01]   | `Camera`             | abstract base      | view matrix, projection matrix   |
|  [02]   | `PerspectiveCamera`  | perspective cam    | `fov`, `aspect`, `near`, `far`   |
|  [03]   | `OrthographicCamera` | orthographic cam   | `left`, `right`, `top`, `bottom` |
|  [04]   | `ArrayCamera` / `CubeCamera` / `StereoCamera` | multi/cube/stereo | VR sub-camera array; 6-face reflection render; anaglyph pair |
|  [05]   | `AmbientLight` / `DirectionalLight` / `PointLight` / `SpotLight` | scene light | uniform; infinite parallel; omnidirectional; cone-constrained |
|  [06]   | `HemisphereLight` / `RectAreaLight` / `LightProbe` | scene light | sky/ground gradient; rectangular emitter; SH-encoded indirect |

[PUBLIC_TYPE_SCOPE]: materials (classic WebGL)
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [NOTE]                           |
| :-----: | :--------------------- | :--------------- | :------------------------------- |
|  [01]   | `Material`             | material base    | blend, side, opacity, depthTest  |
|  [02]   | `MeshBasicMaterial` / `MeshLambertMaterial` / `MeshPhongMaterial` | unlit / diffuse / Phong | flat color/map; Lambertian; specular highlight |
|  [03]   | `MeshStandardMaterial` / `MeshPhysicalMaterial` | PBR / PBR extended | metalness+roughness; clearcoat, sheen, transmission |
|  [04]   | `MeshToonMaterial` / `MeshNormalMaterial` | toon / normal-vis | gradient-map cel shading; RGB-encoded normals |
|  [05]   | `ShaderMaterial` / `RawShaderMaterial` | custom GLSL | built-in uniforms injected / raw, no injection |
|  [06]   | `PointsMaterial` / `SpriteMaterial` | point / sprite | point-cloud size+color; billboard texture |

[PUBLIC_TYPE_SCOPE]: WebGPU node materials, backends, and post (`three/webgpu`)
- rail: render

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]    | [NOTE]                           |
| :-----: | :------------------------------- | :--------------- | :------------------------------- |
|  [01]   | `WebGPURenderer`                 | WebGPU renderer  | the async node-based renderer; auto-detects WebGPU, falls back to WebGL |
|  [02]   | `WebGPUBackend` / `WebGLBackend` | render backend   | native WebGPU device / WebGL fallback; `renderer.backend.isWebGLBackend` discriminates |
|  [03]   | `NodeMaterial`                   | node material base | the TSL-driven material base carrying `colorNode`/`roughnessNode`/`normalNode`/`positionNode` slots |
|  [04]   | `MeshStandardNodeMaterial` / `MeshBasicNodeMaterial` / `MeshPhysicalNodeMaterial` | node material | the TSL twins of the classic PBR/unlit materials; each `*Node` slot takes a TSL node graph |
|  [05]   | `LineBasicNodeMaterial` / `PointsNodeMaterial` / `SpriteNodeMaterial` | node material | node twins for line/point/sprite geometry |
|  [06]   | `PostProcessing`                 | post pipeline    | `new PostProcessing(renderer)` with `.outputNode` = a TSL `pass(scene,camera)` graph; `.renderAsync()` |
|  [07]   | `StorageBufferAttribute` / `StorageInstancedBufferAttribute` | compute buffer | GPU read-write buffers a `three/tsl` `storage()` compute kernel binds |

[PUBLIC_TYPE_SCOPE]: geometries
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [NOTE]                          |
| :-----: | :------------------ | :--------------- | :------------------------------ |
|  [01]   | `BufferGeometry`    | geometry base    | attributes, indices, draw range, groups |
|  [02]   | `BoxGeometry` / `SphereGeometry` / `PlaneGeometry` | primitive | segment-parameterized box / sphere / plane |
|  [03]   | `CylinderGeometry` / `TorusGeometry` / `ExtrudeGeometry` | primitive / extrusion | radii+height; radius+tube; `Shape` + extrude options |
|  [04]   | `EdgesGeometry` / `WireframeGeometry` | derived | edge-line and wireframe extraction from a source geometry |
|  [05]   | `BufferAttribute` / `InstancedBufferAttribute` | typed-array attr | one vertex/instance attribute; the typed-array upload target |

[PUBLIC_TYPE_SCOPE]: math primitives
- rail: render

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [NOTE]                             |
| :-----: | :----------- | :-------------- | :--------------------------------- |
|  [01]   | `Vector2` / `Vector3` / `Vector4` | vector | 2D/3D/4D math; `Vector3` is the workhorse (`x`,`y`,`z`) |
|  [02]   | `Quaternion` / `Euler` | rotation | SLERP-capable rotation; ordered Euler angles (`order`) |
|  [03]   | `Matrix3` / `Matrix4` | matrix | normal matrix; transform/projection matrix |
|  [04]   | `Color`      | RGB color       | `r`,`g`,`b`, CSS/hex parsing; sRGB↔linear under `ColorManagement` |
|  [05]   | `Box3` / `Sphere` / `Plane` / `Ray` / `Frustum` | bounds/geom | AABB; bounding sphere; plane; ray; 6-plane containment |
|  [06]   | `MathUtils`  | math utilities  | `clamp`, `lerp`, `degToRad`, `generateUUID` |

[PUBLIC_TYPE_SCOPE]: textures, animation, renderers, loaders, core utilities
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [NOTE]                      |
| :-----: | :------------------ | :------------- | :-------------------------- |
|  [01]   | `Texture` / `CubeTexture` / `DataTexture` | texture | image/wrap/filter; 6-face env; typed-array pixel data |
|  [02]   | `CanvasTexture` / `VideoTexture` / `CompressedTexture` / `DepthTexture` | texture | canvas/video source; KTX2/BCn/ASTC; depth buffer |
|  [03]   | `AnimationClip` / `AnimationMixer` / `AnimationAction` / `KeyframeTrack` / `AnimationUtils` | animation | clip asset; playback engine; action handle; track; subclip/linearize |
|  [04]   | `WebGLRenderer` / `WebGLRenderTarget` | classic renderer | WebGL 2 raster pipeline; off-screen FBO |
|  [05]   | `Loader` / `TextureLoader` / `FileLoader` / `ObjectLoader` / `BufferGeometryLoader` / `ImageLoader` / `AudioLoader` / `LoadingManager` | loader | base + image/file/scene-JSON/geometry-JSON/image/audio loaders and the progress manager |
|  [06]   | `Raycaster` / `Clock` / `Timer` / `EventDispatcher` / `Layers` | core util | ray picking; delta timing; typed event bus; camera/object visibility bitmask |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Object3D — scene graph operations
- rail: render

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [NOTE]                       |
| :-----: | :------------------------- | :------------- | :--------------------------- |
|  [01]   | `add(...o)` / `remove(...o)` / `attach(o)` | hierarchy | append; detach; reparent preserving world transform |
|  [02]   | `lookAt(x,y,z)` / `applyMatrix4(m)` / `translateX/Y/Z(d)` / `rotateX/Y/Z(a)` | transform | orient; accumulate; local-axis translate/rotate |
|  [03]   | `localToWorld(v)` / `worldToLocal(v)` | space convert | local↔world coordinate mapping |
|  [04]   | `getObjectByName(n)` / `traverse(cb)` / `clone(recursive?)` | query / clone | depth-first name search; visit; shallow/deep clone |

[ENTRYPOINT_SCOPE]: WebGPURenderer — async node render + compute (`three/webgpu`)
- rail: render

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                    |
| :-----: | :------------------------------------- | :------------- | :------------------------ |
|  [01]   | `new WebGPURenderer({ canvas?, antialias?, forceWebGL? })` / `init()` | construct | the ctor is sync; `await init()` selects the backend and MUST resolve before the first render |
|  [02]   | `render(scene, camera)` / `renderAsync(scene, camera)` | render call | sync frame (post-`init`) / awaited frame |
|  [03]   | `compute(computeNode)` / `computeAsync(node)` | GPU compute | dispatch a `three/tsl` compute kernel (`Fn(...).compute(count)`) writing storage buffers |
|  [04]   | `setSize(w, h, updateStyle?)` / `setPixelRatio(v)` / `setAnimationLoop(cb)` | viewport / loop | canvas dimensions; DPR; rAF loop (`null` stops) |
|  [05]   | `setClearColor(c, a?)` / `.toneMapping` / `.toneMappingExposure` / `.backend.isWebGLBackend` | state | clear color; tone-mapping curve+exposure; the resolved-backend discriminant |
|  [06]   | `dispose()`                            | cleanup        | release device + all GPU resources — the `Effect.acquireRelease` finalizer |

[ENTRYPOINT_SCOPE]: WebGLRenderer — classic render pipeline
- rail: render

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                    |
| :-----: | :------------------------------------- | :------------- | :------------------------ |
|  [01]   | `render(scene, camera)` / `setAnimationLoop(cb)` | render / loop | draw one frame; rAF-driven loop (callback gets elapsed ms) |
|  [02]   | `setSize(w,h,updateStyle?)` / `setPixelRatio(v)` / `getSize(target)` | viewport | canvas dimensions, retina DPR, current-size query |
|  [03]   | `setClearColor(...)` / `clear(color?,depth?,stencil?)` / `setRenderTarget(rt,face?,mip?)` | clear / FBO | background clear and off-screen render-target switch |
|  [04]   | `compile(scene,camera,target?)` / `readRenderTargetPixels(...)` / `dispose()` | compile / readback / cleanup | pre-compile materials; GPU→CPU pixel read; release GPU resources |

[ENTRYPOINT_SCOPE]: BufferGeometry — attribute assembly
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [NOTE]                        |
| :-----: | :---------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `setAttribute(name, attr)` / `getAttribute(name)` / `deleteAttribute(name)` | attribute | register/read/drop a named vertex attribute (`position`, `normal`, `uv`) |
|  [02]   | `setIndex(index)` / `addGroup(start,count,matIndex?)` | index / group | triangle indices; multi-material draw ranges |
|  [03]   | `computeBoundingBox()` / `computeBoundingSphere()` / `computeVertexNormals()` | derive | populate bounds; smooth normals from positions |
|  [04]   | `applyMatrix4(m)` / `toNonIndexed()` / `clone()` / `dispose()` | transform / convert / cleanup | bake transform; expand indexed→flat; deep clone; release GPU buffers |

[ENTRYPOINT_SCOPE]: PerspectiveCamera / AnimationMixer / Raycaster
- rail: render

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [NOTE]                    |
| :-----: | :---------------------------------------------- | :------------- | :------------------------ |
|  [01]   | `new PerspectiveCamera(fov,aspect,near,far)` / `updateProjectionMatrix()` / `setFocalLength(f)` / `setViewOffset(...)` | camera | projection; re-apply after changing `fov`/`aspect`/`near`/`far`; 35mm focal length; tiled sub-viewport |
|  [02]   | `new AnimationMixer(root)` / `clipAction(clip, root?)` / `update(delta)` / `stopAllAction()` / `uncacheClip(clip)` | animation | bind mixer; get/create an action; advance per frame; halt; evict clip cache |
|  [03]   | `setFromCamera(coords, camera)` / `intersectObject(o, recursive?, results?)` / `intersectObjects(os, ...)` | raycast | NDC→world ray; near-to-far sorted hits for pointer picking |

[ENTRYPOINT_SCOPE]: TSL node-function catalog (`three/tsl`)
- rail: render

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [NOTE]                    |
| :-----: | :---------------------------------------------- | :------------- | :------------------------ |
|  [01]   | `Fn((args) => node)` / `If(cond, cb)` / `Loop(count, cb)` | control | define a reusable node function; branch; GPU loop — the shader/compute authoring core |
|  [02]   | `float` / `int` / `bool` / `vec2` / `vec3` / `vec4` / `color` | constructor | typed node literals and swizzle-capable vectors; method-chained math (`.mul`,`.add`,`.mix`,`.clamp`) |
|  [03]   | `uniform(v)` / `attribute(name)` / `reference(key,type,obj)` / `texture(tex, uv?)` | binding | live uniform (`.value` mutable), vertex attribute, object-field reference, sampled texture |
|  [04]   | `storage(attr, type, count)` / `instanceIndex` / `instancedArray` | compute | storage-buffer binding, the compute invocation index, per-instance storage — the GPU-compute inputs |
|  [05]   | `uv()` / `positionLocal` / `positionWorld` / `normalLocal` / `normalWorld` / `reflectVector` | geometry node | the built-in per-fragment geometry inputs |
|  [06]   | `pass(scene, camera)` / `mrt({...})` / `mix` / `time` / `range(a,b)` / `oscSine` | post / util | render-pass node for `PostProcessing`; multi-render-target; blend; frame time; per-instance random range; oscillator |
|  [07]   | `mx_noise_vec3` / `mx_worley_noise_vec3` / `mx_fractal_noise_vec3` / `hash` / `normalMap` / `parallaxUV` / `hue` / `saturation` / `pmremTexture` | procedural | MaterialX noise family, hashing, normal-map decode, parallax UV, HSV adjust, prefiltered env sampling |

[ENTRYPOINT_SCOPE]: addons (`three/addons/*` → `three/examples/jsm/*`)
- rail: render

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [NOTE]                    |
| :-----: | :---------------------------------------------- | :------------- | :------------------------ |
|  [01]   | `three/addons/controls/OrbitControls.js` `OrbitControls` | control | orbit/pan/zoom camera controls (`enableDamping`, `autoRotate`, `.update()`) |
|  [02]   | `three/addons/loaders/GLTFLoader.js` `GLTFLoader` / `DRACOLoader` / `KTX2Loader` | loader | GLB/glTF parse (`.load`/`.loadAsync`, `.setDRACOLoader`); mesh + texture decompression |
|  [03]   | `three/addons/loaders/HDRLoader.js` `HDRLoader` / `RGBELoader` | loader | equirectangular HDR environment maps for `scene.environment` |
|  [04]   | `three/addons/tsl/display/*` (e.g. `GaussianBlurNode.js` `gaussianBlur`) / `three/addons` `Addons.js` | post node / barrel | TSL post-processing display nodes; the addon barrel re-export |

## [04]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:
- `three` (main): `WebGLRenderer` + the full classic scene graph, math, material, geometry, loader, and animation catalog
- `three/webgpu`: re-exports the core PLUS `WebGPURenderer`, `WebGPUBackend`, `WebGLBackend`, the node-material family, `PostProcessing`, and the storage-buffer attributes — the r171+ split (imports for `WebGPURenderer`/`NodeMaterial` moved here, TSL to `three/tsl`)
- `three/tsl`: the TSL node-function catalog (`Fn`/`If`/`Loop`/`uniform`/`texture`/`storage`/`vec3`/…) for authoring materials AND GPU compute in TypeScript
- `three/addons/*` (= `three/examples/jsm/*`): controls, loaders, exporters, post nodes — NOT bundled in the main entry, imported by subpath
- `WebGPURenderer.init()` is async and selects the backend: the ctor is sync, `await init()` resolves the device, `.backend.isWebGLBackend` reveals the fallback; `render`/`renderAsync` must follow `init`
- `ColorManagement` is enabled by default (r150+): sRGB input textures set `colorSpace = SRGBColorSpace`; `PerspectiveCamera.updateProjectionMatrix()` follows any `fov`/`aspect`/`near`/`far` change; `Texture`/`BufferGeometry`/`WebGLRenderTarget`/renderer all `dispose()` at end of life

[STACKING]:
- GL lifetime under effect (the viewport backend row): `render/glb.md#GLB_VIEWPORT` acquires the `WebGPURenderer` via `effect` `Effect.acquireRelease(Effect.promise(() => new WebGPURenderer({canvas}).init().then(r => r)), r => Effect.sync(() => r.dispose()))`, forks the render loop as `Effect.forkScoped`, and lets the `Scope` tear down context, buffers, and fiber LIFO (`../.api/effect.md` `Effect.acquireRelease`/`Scope`/`forkScoped`) — never a free React ref holding the GL context
- backend + geometry dispatch: `RendererBackend` is a `Schema.Literal("three","model-viewer")` selected under `Match.value(backend).pipe(Match.when("three", …), Match.when("model-viewer", …), Match.exhaustive)`, and the `GeometryView` `Data.taggedEnum` (`mesh`/`pointCloud`/`voxel`) folds total under `GeometryView.$match` onto `BufferGeometry.setAttribute("position", new BufferAttribute(f32, 3))` + `setIndex` / `Points` / instanced voxel (`../.api/effect.md` `Match`/`Data.taggedEnum`, `render/glb.md`)
- WebGPU device typing + compute: the `@webgpu/types` ambient `GPU*` declarations type the device the `WebGPUBackend` and any `three/tsl` `storage`/`instanceIndex`/`compute` kernel touches (`webgpu-types.md`) — the meshlet/cluster-LOD residency path rides TSL compute (`Fn(...).compute(count)` via `renderer.computeAsync`), never a hand-authored `GPUDevice`/`GPURenderPipeline` beside three
- proto decode → GPU upload: the `@bufbuild/protobuf` generated `GeometryPayload` descriptor decodes the C# `MeshTensor` by reference; `decodeMeshView` folds the `kind` oneof into typed-array views (`Float32Array` N×3, `Uint32Array` F×3) that become `THREE.BufferAttribute` — three owns the GPU upload, the proto owns the wire, and the worker-pool sink's `byteOffset` alignment is honored so the view is zero-copy where aligned (`render/glb.md`)
- declarative row: `@google/model-viewer` `<model-viewer>` is the sibling `RendererBackend` arm — a zero-GL-handle custom element taking a GLB object-URL `Blob` (`model/gltf-binary`) on `src` with a `camera-controls` attribute (`google-model-viewer.md`, `render/glb.md`); three is the imperative arm, model-viewer the declarative arm, both literal rows on the one axis
- camera + preservation seams: the viewport camera is one `interaction/role.md` `RoleBehavior` folding the `interaction/gesture.md` `CameraGesture` `Data.taggedEnum` into `PerspectiveCamera` state (not `OrbitControls`, which is reserved for standalone addon use), and a backgrounded viewport stays mounted through the React `<Activity>` surface (`interaction/transition.md`) preserving the context and uploaded buffers across visibility toggles

[LOCAL_ADMISSION]:
- import the WebGPU path from `three/webgpu` and TSL from `three/tsl`; import classic-only surfaces from `three`; addons by explicit `three/addons/<path>` subpath
- admit `@types/three` (matching minor) in the workspace — `three` ships no bundled declarations; without it every `import * as THREE from "three/webgpu"` is untyped
- `await WebGPURenderer.init()` before the first render; call `updateProjectionMatrix()` after camera-frustum changes; call `AnimationMixer.update(delta)` each frame; match `BufferAttribute` typed-array component size to the declared itemSize (a mismatch corrupts silently)
- `dispose()` every `Texture`/`BufferGeometry`/`WebGLRenderTarget`/renderer at end of life — under effect this is the `acquireRelease` finalizer, not a manual teardown

[RAIL_LAW]:
- package: `three` (types: `@types/three`)
- owns: the scene graph, WebGL + WebGPU render pipelines, math primitives, materials (classic + node), geometries, lights, cameras, textures, loaders, animation, and the TSL shader/compute surface
- accept: `three`, `three/webgpu`, `three/tsl`, `three/addons/*` subpath imports; `WebGPURenderer` auto-detect+fallback; TSL node materials and compute kernels; `BufferGeometry` attribute assembly from decoded typed arrays
- reject: hand-rolled WebGL/WebGPU state management; custom matrix math duplicating `Matrix4`/`Quaternion`; a raw `navigator.gpu` degrade branch beside `WebGPURenderer.init()`; a hand-authored `GPUDevice` compute pipeline where a `three/tsl` `compute` kernel serves; a GL context held as a free React ref instead of an `Effect.acquireRelease` resource
