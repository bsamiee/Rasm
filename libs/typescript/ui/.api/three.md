# [TS_UI_API_THREE]

`three` is the render substrate of the `ui/viewer` Nx project (`scope:viewer`, compile-time excluded from the non-spatial majority): one scene-graph + renderer + loader engine `viewer/scene` composes into GLB residency and OpenPBR appearance. Two renderer backends satisfy one usage contract — `WebGLRenderer` (core `three`) and `WebGPURenderer` (`three/webgpu`, an async-init unified `Renderer` carrying node-material, compute, and post-processing surfaces) — so `viewer/scene/glb` selects a backend at boot without re-authoring the scene.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package: `three` (MIT)
- module: ESM (`type: module`), four export roots — `three` (core), `three/webgpu` (WebGPU renderer + node materials + compute), `three/tsl` (node-shader language), `three/addons/*` ⇒ `three/examples/jsm/*` (loaders, controls, environments, codecs); `sideEffects: ["./src/nodes/**/*"]` retains node-material TSL registration; ships no `.d.ts`, so `@types/three` is the admitted compile-time declaration companion
- runtime: browser DOM + WebGL2/WebGPU context, no Node binding and no peer binding — the viewer runs it on the app render thread, codec transcoders in the `browser` worker, and the React seam is the app's own `<canvas>` ref + `useEffect` teardown
- rail: viewer scene tier — `viewer/scene/glb` (residency) and `viewer/scene/appearance` (OpenPBR binding) compose the renderer/scene/loader/material engine; `scope:viewer` project-local

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph, cameras, and the transform spine

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]   | [CONSUMER]                                     |
| :-----: | :------------------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `Scene` / `Object3D` / `Group`                                       | scene node      | `viewer/scene/glb` — residency root, GLB graft |
|  [02]   | `Mesh` / `InstancedMesh` / `BatchedMesh` / `SkinnedMesh`             | drawable        | `viewer/scene/glb` — repeats to one draw call  |
|  [03]   | `PerspectiveCamera` / `OrthographicCamera` / `ArrayCamera`           | camera          | `viewer/geo/project` — camera sync, bounds     |
|  [04]   | `Raycaster` / `Layers`                                               | pick / mask     | `viewer/mark/selection` — `GlobalId` hit-test  |
|  [05]   | `BufferGeometry` / `BufferAttribute` / `InterleavedBufferAttribute`  | geometry buffer | `viewer/scene/glb` — typed-array attributes    |
|  [06]   | `Box3` / `Sphere` / `Frustum` / `Vector3` / `Matrix4` / `Quaternion` | math value      | `viewer/geo`, `viewer/scene/glb` — transform   |

[PUBLIC_TYPE_SCOPE]: materials, textures, and color management — the OpenPBR binding target

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY] | [CONSUMER]                                  |
| :-----: | :------------------------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `MeshPhysicalMaterial`                                                     | PBR material  | `viewer/scene/appearance` — OpenPBR target  |
|  [02]   | `MeshStandardMaterial` / `MeshBasicMaterial` / `ShaderMaterial`            | material base | `viewer/scene/appearance` — MR/unlit/shader |
|  [03]   | `Texture` / `CompressedTexture` / `CompressedArrayTexture` / `DataTexture` | texture       | `viewer/scene/glb` — KTX2/Basis + HDR       |
|  [04]   | `Color` / `ColorManagement` / `SRGBColorSpace` / `LinearSRGBColorSpace`    | color space   | `viewer/scene/appearance` — color-space     |
|  [05]   | `ACESFilmicToneMapping` / `AgXToneMapping` / `NeutralToneMapping`          | tone map enum | `viewer/scene/glb` — tone-map; AgX/Neutral  |
|  [06]   | `DirectionalLight` / `AmbientLight` / `HemisphereLight` / `RectAreaLight`  | light         | `viewer/scene/glb` — analytic lights        |
|  [07]   | `LightProbe`                                                               | light probe   | `viewer/scene/glb` — irradiance probe       |

[PUBLIC_TYPE_SCOPE]: WebGPU backend, node materials, and compute (`three/webgpu`)

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]    | [CONSUMER]                                     |
| :-----: | :---------------------------------------------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `Renderer` / `WebGPUBackend` / `WebGLBackend`               | unified renderer | `viewer/scene/glb` — WebGPU/WebGL2 backend     |
|  [02]   | `MeshPhysicalNodeMaterial` / `MeshStandardNodeMaterial`     | node material    | `viewer/scene/appearance` — WebGPU OpenPBR TSL |
|  [03]   | `PointsNodeMaterial` / `MeshBasicNodeMaterial`              | node material    | `viewer/scene/glb` — point/unlit node graph    |
|  [04]   | `PostProcessing` / `RendererUtils`                          | post pipeline    | `viewer/probe/receipt` — `RenderReceipt` chain |
|  [05]   | `ComputeNode` / `StorageBufferNode`                         | GPU compute      | `viewer/scene/glb` — compute-pass transforms   |
|  [06]   | `StorageBufferAttribute` / `IndirectStorageBufferAttribute` | compute buffer   | `viewer/scene/glb` — storage/indirect attrs    |

[PUBLIC_TYPE_SCOPE]: loader and animation contracts

Every row is consumed by `viewer/scene/glb`.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CONSUMER]                                            |
| :-----: | :--------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Loader` / `LoadingManager`                                      | loader base   | codec base; `LoadingManager` folds GLB progress/error |
|  [02]   | `GLTFLoader` (`three/addons`) / `GLTF` result                    | GLB loader    | parses to `{ scene, animations, cameras, asset }`     |
|  [03]   | `PMREMGenerator` / `WebGLCubeRenderTarget`                       | IBL prefilter | prefilters equirect/scene into the IBL mip-chain      |
|  [04]   | `AnimationMixer` / `AnimationClip` / `AnimationAction` / `Clock` | animation     | GLB tracks on a per-frame mixer + `Clock`             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: renderer construction and the frame loop
- Both renderers take `{ canvas, antialias, alpha?, powerPreference? }`; every row serves `viewer/scene/glb`.

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]       | [CONSUMER]            |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------- | :-------------------- |
|  [01]   | `new WebGLRenderer(opts)` / `.setPixelRatio` / `.setSize`                                | build renderer       | WebGL2 canvas backend |
|  [02]   | `new WebGPURenderer(opts)` / `await renderer.init()` / `.hasFeature(name)`               | build (async)        | WebGPU async backend  |
|  [03]   | `renderer.setAnimationLoop(fn)` / `.render(s, c)` / `.renderAsync(s, c)`                 | frame loop           | RAF loop, submit      |
|  [04]   | `renderer.outputColorSpace` / `.toneMapping` / `.toneMappingExposure` / `.setClearColor` | output policy        | display contract      |
|  [05]   | `renderer.compileAsync(s, c)` / `renderer.dispose()` / `computeAsync(node)`              | precompile / dispose | pre-warm, dispose     |
|  [06]   | `renderer.info` — `.render` / `.compute` / `.memory` / `.autoReset` / `.reset()`         | counter surface      | probe metric sink     |

- [02]-[WEBGPU_UPGRADE]: `hasFeature` (post-`init()`; `hasFeatureAsync` deprecated) gates the upgrade, resolving through `WebGPUBackend.hasFeature` to `GPUDevice.features.has(name)` (`.api/webgpu-types.md` owns the `GPUFeatureName` vocabulary); the `null` loop pauses under `<Activity>` hidden.
- [06]-[COUNTER_SURFACE]: `Renderer` (unified base, `WebGPURenderer` included) carries `info.render` `{calls, frameCalls, drawCalls, triangles, points, lines, timestamp}`, `info.compute` `{calls, frameCalls, timestamp}`, and `info.memory` `{geometries, textures, texturesSize, attributes, attributesSize, programs, renderTargets, readbackBuffers, uniformBuffers, total}`; an app-managed loop sets `info.autoReset = false` and calls `info.reset()` once per frame.

[ENTRYPOINT_SCOPE]: GLB ingestion — the plugin-configured decode pipeline
- Every row serves `viewer/scene/glb`; codec wasm runs in the `browser` decode-worker, never the render thread.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [CONSUMER]             |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `new GLTFLoader(manager).setDRACOLoader(d).setKTX2Loader(k).setMeshoptDecoder(m)`             | codec setup    | injection, [01]        |
|  [02]   | `loader.register((parser) => plugin)` / `loader.parseAsync(buffer, path)` / `.loadAsync(url)` | parse          | buffer `parseAsync`    |
|  [03]   | `new DRACOLoader().setDecoderPath(p).setDecoderConfig({ type })` / `.preload()`               | draco codec    | geometry codec         |
|  [04]   | `new KTX2Loader().setTranscoderPath(p).detectSupport(renderer)`                               | basis codec    | transcode target, [04] |
|  [05]   | `KTX2Loader` / `RGBELoader` / `TextureLoader` → `renderer.initTexture(tex)`                   | texture ingest | eager `initTexture`    |

- [01]-[MESHOPT_GATE]: `MeshoptDecoder` wires only when the asset flags `EXT_meshopt_compression`.
- [04]-[TRANSCODE_TARGET]: `detectSupport(renderer)` reads the renderer's compressed-format capabilities through `renderer.hasFeature` — `GPUDevice.features` for the `texture-compression-bc`/`-etc2`/`-astc` `GPUFeatureName` members on WebGPU, `WEBGL_compressed_texture_*` extensions on WebGL — and selects the KTX2/Basis transcode target once per asset.

[ENTRYPOINT_SCOPE]: OpenPBR appearance binding on `MeshPhysicalMaterial`
- Every row binds `MeshPhysicalMaterial`/`MeshPhysicalNodeMaterial` fields decoded from `wire#vocab`; consumer `viewer/scene/appearance` unless noted.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY]      | [CONSUMER]             |
| :-----: | :------------------------------------------------------------------------------------- | :------------------ | :--------------------- |
|  [01]   | `material.clearcoat` / `.clearcoatRoughness` / `.clearcoatMap` / `.clearcoatNormalMap` | coat lobe           | coat weight/rough/maps |
|  [02]   | `.sheen` / `.sheenColor` / `.sheenRoughness`                                           | sheen lobe          | fuzz layer             |
|  [03]   | `.transmission` / `.thickness` / `.attenuationColor` / `.attenuationDistance` / `.ior` | transmission        | refraction; glass      |
|  [04]   | `.iridescence` / `.iridescenceIOR` / `.iridescenceThicknessRange`                      | thin-film           | interference + maps    |
|  [05]   | `.anisotropy` / `.anisotropyRotation`                                                  | anisotropy          | aniso highlight + maps |
|  [06]   | `.specularIntensity` / `.specularColor`                                                | specular            | specular tint          |
|  [07]   | `.metalness` / `.roughness` / `.envMapIntensity`                                       | metallic base + IBL | MR base + IBL scale    |
|  [08]   | `.emissive` / `.emissiveIntensity` / `.opacity` / `.transparent` / `.side`             | emission + geometry | emission, [08]         |
|  [09]   | `pmrem.fromScene(env, 0.04)` / `.fromEquirectangular(hdr)` → `scene.environment`       | IBL bind            | `viewer/scene/glb` env |

- [08]-[SIDE]: `.side` takes `FrontSide`/`DoubleSide`; `dispersion` rides the [03] glass rows.
- [09]-[IBL_DEFAULT]: `RoomEnvironment()` is the neutral studio default the prefilter bakes; every `MeshPhysicalMaterial` samples the result.

[ENTRYPOINT_SCOPE]: TSL node-shader and GPU-compute authoring (`three/tsl`)
- `Fn(cb)` builds a node function; `Fn(cb)().compute(count)` yields a compute node the unified `Renderer` dispatches through `renderer.compute(node)`. Node-material fields (`material.colorNode`/`.roughnessNode`/`.positionNode`) bind TSL graphs onto `MeshStandardNodeMaterial`/`MeshPhysicalNodeMaterial`/`PointsNodeMaterial`.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]   | [CONSUMER]                    |
| :-----: | :------------------------------------------------------------------------- | :--------------- | :---------------------------- |
|  [01]   | `Fn(cb)` / `If` / `Loop` / `Break` / `Continue`                            | control flow     | shader graph body             |
|  [02]   | `float` / `int` / `vec3` / `vec4` / `color` / `mat4`                       | value node       | typed shader value            |
|  [03]   | `uniform(v)` / `texture(t)` / `uv()` / `attribute` / `varying`             | binding node     | CPU→shader input              |
|  [04]   | `mix` / `hash` / `oscSine` / `time` / `deltaTime`                          | math / time      | animated lobe                 |
|  [05]   | `vertexColor()` / `vertexIndex` / `instanceIndex`                          | attribute index  | per-vertex/instance read      |
|  [06]   | `instancedArray(count, type)` / `.element(i)` / `.compute(count)`          | storage buffer   | `viewer/scene/glb` — compute  |
|  [07]   | `globalId` / `localId` / `workgroupId` / `numWorkgroups`                   | compute built-in | per-invocation index          |
|  [08]   | `workgroupBarrier()` / `storageBarrier()` / `textureBarrier()`             | sync barrier     | workgroup synchronization     |
|  [09]   | `atomicAdd` / `atomicSub` / `atomicMax` / `atomicMin` / `atomicOr`         | atomic op        | thread-safe accumulate        |
|  [10]   | `pass(scene, cam)` / `.setMRT(mrt(...))` / `.getTextureNode(n)` / `output` | post / MRT       | `viewer/probe/receipt` — pass |
|  [11]   | `wgslFn(src)`                                                              | raw WGSL         | escape-hatch kernel           |

[ENTRYPOINT_SCOPE]: interaction, geometry folding, and evidence
- `BufferGeometryUtils` is `three/addons`; `OrbitControls extends Controls extends EventDispatcher`.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]    | [CONSUMER]                              |
| :-----: | :------------------------------------------------------------------- | :---------------- | :-------------------------------------- |
|  [01]   | `new OrbitControls(cam, dom)` / `ArcballControls` / `MapControls`    | camera control    | `viewer/geo/project` — camera nav       |
|  [02]   | `controls.update()` / `controls.dispose()`                           | control lifecycle | `viewer/geo` — update, `Scope` dispose  |
|  [03]   | `renderer.getPixelRatio()` / `.readRenderTargetPixelsAsync(rt, ...)` | frame readback    | `viewer/probe/receipt` — pixel readback |
|  [04]   | `BufferGeometryUtils.mergeGeometries(list)` / `.mergeVertices(geo)`  | geometry fold     | `viewer/scene/glb` — merge submeshes    |
|  [05]   | `controls.addEventListener("change", fn)` / `controls.target`        | control events    | `viewer/geo` — `change` folds camera    |

[ENTRYPOINT_SCOPE]: animation drive and draw-call collapse
- Every row serves `viewer/scene`; loop-mode constants are core `three`.

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY]  | [CONSUMER]                    |
| :-----: | :------------------------------------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `new AnimationMixer(root)` / `mixer.clipAction(clip)` / `mixer.timeScale`        | mixer build     | one mixer per animated graft  |
|  [02]   | `mixer.update(deltaTime)` / `mixer.stopAllAction()` / `mixer.uncacheRoot(root)`  | mixer drive     | advance + teardown in loop    |
|  [03]   | `action.play()` / `.stop()` / `.reset()` / `.paused` / `.enabled`                | action control  | clip playback state           |
|  [04]   | `action.setLoop(mode, repetitions)` — `LoopOnce` / `LoopRepeat` / `LoopPingPong` | loop mode       | loop-mode policy constants    |
|  [05]   | `new Clock()` / `clock.getDelta()` / `clock.getElapsedTime()`                    | time source     | one delta source per mixer    |
|  [06]   | `new InstancedMesh(geometry, material, count)` / `.instanceMatrix`               | instanced build | repeated geometry, one draw   |
|  [07]   | `.setMatrixAt(i, matrix)` / `.computeBoundingSphere()`                           | instanced slot  | per-instance matrix + bounds  |
|  [08]   | `new BatchedMesh(maxInstances, maxVertices, maxIndices, material)`               | batched build   | distinct geoms, one draw call |
|  [09]   | `.addGeometry(geo)` / `.addInstance(geoId)`                                      | batched add     | geometry + instance ids       |
|  [10]   | `.setMatrixAt(i, matrix)` / `.setVisibleAt(i, flag)`                             | batched slot    | per-instance matrix + visible |
|  [11]   | `renderer.initTexture(texture)`                                                  | eager upload    | upload ahead of first frame   |

## [04]-[IMPLEMENTATION_LAW]

[SCENE_TOPOLOGY]:
- One `Scene` owns the residency graph; each loaded GLB grafts its `gltf.scene` `Object3D` subtree under it, keyed by the content-key from `viewer/scene/glb`'s `ResidencyManifest`. Removal disposes geometry/material/texture GPU handles — three does not garbage-collect GPU memory, so every `remove` pairs with `.dispose()` under an Effect `Scope` finalizer.
- Two renderer backends satisfy one usage contract: `WebGLRenderer` (synchronous construct) and `WebGPURenderer` (async `.init()`). `viewer/scene/glb` probes `navigator.gpu` + `renderer.hasFeature` (post-`init()`), selects the backend once at boot, and codes the scene/camera/loop identically after — the WebGPU upgrade is a construction-site swap, never a scene rewrite.
- GLB decode is dependency injection: `GLTFLoader` holds a DRACO codec, a KTX2 codec, and a Meshopt decoder as *attached* collaborators, and `viewer/scene/glb` wires each codec's wasm transcoder into the `browser` decode-worker (`DRACOLoader.setWorkerLimit`, the `KTX2Loader` worker pool, `MeshoptDecoder`), so heavy transcode never blocks the render thread; `MeshoptDecoder` attaches only when the asset declares `EXT_meshopt_compression`.
- `MeshPhysicalMaterial` is the single OpenPBR bind target: its coat/sheen/transmission/iridescence/anisotropy/specular fields are a superset of the glTF PBR extensions and map field-for-field onto the C# OpenPBR algebra projected through `wire#vocab` appearance. `MeshPhysicalNodeMaterial` swaps in on the WebGPU path (same lobes as TSL node graphs) with no change to the appearance-decode row.
- Image-based lighting is a prefilter step: `PMREMGenerator.fromScene`/`fromEquirectangular` bakes the environment into a roughness-mip cube once, and `scene.environment` then serves every physical material's IBL sample, so the render loop stays a pure `setAnimationLoop(() => renderer.render(scene, camera))`.

[STACKING]:
- `effect` (`libs/typescript/.api/effect.md`): `Effect.acquireRelease`/`Scope` wraps every GPU-resource owner — renderer, controls, loaders, geometries, prefilter targets — its finalizer calling `.dispose()`, so a viewport unmount releases every GPU handle deterministically. `viewer/scene/glb` drives the frame loop on an interruptible `Effect` fiber, paused when `<Activity>` marks the viewport hidden. `Match`/`Schema` classify the decoded `wire` appearance before it reaches `MeshPhysicalMaterial`.
- `@webgpu/types` (`.api/webgpu-types.md`): supplies the ambient `GPUDevice`/`GPUAdapter`/`GPUTextureFormat` types the `three/webgpu` backend and the `KTX2Loader.detectSupport` probe read. three owns the runtime mechanism: the backend requests an adapter/device through `navigator.gpu`, gates the upgrade on `renderer.hasFeature` → `WebGPUBackend.hasFeature` → `GPUDevice.features.has`, and `detectSupport` reads that same `GPUDevice.features` for the `texture-compression-bc`/`-etc2`/`-astc` `GPUFeatureName` members to pick the transcode target; `@webgpu/types` is the type floor beneath that runtime, admitted by a viewer-tsconfig `types` entry, never imported.
- `@google/model-viewer` (`.api/google-model-viewer.md`): `<model-viewer>` is the declarative custom-element row for the simple embed case; three is the imperative full-control path for custom materials, compute, selection ray-casting, or the render receipt. Both are siblings over ONE shared three module — `<model-viewer>` peers `three@catalog`, filled by this same workspace copy (pnpm dedupes onto one physical three), re-exports these exact classes, yet owns its own renderer instance + `<canvas>` + GL context. `RendererBackend`'s `Schema.Literal("three", "model-viewer")` picks which path draws a given GLB; they meet at the GLB wire.
- `browser` decode-worker port (declared by `viewer/scene/glb` as `GlbViewport` residency, Layer-provided by `browser`): three's codec transcoders (DRACO wasm, KTX2 Basis, Meshopt) run behind this port, never on the render thread; `ui` declares the port and `browser` binds the worker Layer at app composition.
- `wire#vocab` appearance (`csharp:Rasm.Materials/Appearance/interchange` seam): `viewer/scene/appearance` decodes `MaterialWire`/`OpenPbrGroupsWire`/`AppearanceSummary` once at `wire` and binds the decoded values onto `MeshPhysicalMaterial`; a peer re-mint of the OpenPBR algebra in TypeScript is the `CROSS_LANGUAGE_WIRE` drift defect.
- `colorjs.io` (`.api/colorjs.io.md`) + three `ColorManagement`: theme/appearance colors author in OKLCH and gamut-fit in `colorjs.io`, then `to("srgb"/"srgb-linear")`-convert into `coords` whose ids are the exact string values of three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants; `Color.setRGB(r, g, b, colorSpace)` ingests them and `ColorManagement` transforms source→working, so token color and rendered color share one color-space contract.

[LOCAL_ADMISSION]:
- Import core `three` for the scene/camera/material/geometry spine, `three/webgpu` inside the WebGPU-backend construction row, `three/tsl` inside node-material and compute authoring, and `three/addons/*` for `GLTFLoader`/controls/codecs/`RoomEnvironment` — resolve every addon through the `three/addons/*` export alias, never a raw `three/examples/jsm/...` path.
- Attach codecs to `GLTFLoader` by injection (`setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder`); the codec wasm belongs in the `browser` decode-worker behind the port, never a per-codec loader fork or hand-rolled DRACO/KTX2/Meshopt decode.
- Bind OpenPBR appearance onto `MeshPhysicalMaterial`/`MeshPhysicalNodeMaterial` fields decoded from `wire#vocab`; the OpenPBR→glTF-PBR algebra is owned in C# and mirrored once at `wire`, never re-derived in `ui`.
- Acquire every renderer/geometry/material/texture/control/prefilter under an Effect `Scope` and dispose in the finalizer — three has no GC for GPU memory, so a GPU-resource owner outliving its viewport leaks the handle.
- Prefilter environments with `PMREMGenerator` once and serve via `scene.environment`, never a raw per-fragment equirect sample or a per-frame IBL-mip rebuild.
- Verify three member existence against the shipped runtime exports (`three.module.js`/`three.webgpu.js`/`three.tsl.js`) and the `examples/jsm/**` addon source; `@types/three` compiles the code but drifts from the shipped surface, so runtime exports decide member truth.

[RAIL_LAW]:
- Package: `three` (`scope:viewer` project-local)
- Owns: the WebGL2/WebGPU renderer backends, the `Scene`/`Object3D`/`Mesh` residency graph, the material family (`MeshPhysicalMaterial` OpenPBR lobes + node-material mirror), the loader family (`GLTFLoader` + injected DRACO/KTX2/Meshopt codecs), IBL prefilter (`PMREMGenerator`), animation, camera controls, TSL node-shader authoring, and GPU compute
- Accept: one `Scene` residency root keyed by content-key, backend selection at boot behind one usage contract, codec injection into `GLTFLoader`, OpenPBR bind onto `MeshPhysicalMaterial` from `wire#vocab`, `Scope`-scoped GPU-resource disposal, `PMREMGenerator` IBL prefilter, codec transcode behind the `browser` decode-worker port
- Reject: `@types/three` as the member-truth source, per-codec loader forks or hand-rolled DRACO/KTX2/Meshopt decode, OpenPBR-mapping re-mint in `ui`, GPU-resource owners outliving their viewport, per-frame IBL rebuild, deep raw `examples/jsm/...` imports where `three/addons/*` resolves, render-thread codec transcode
