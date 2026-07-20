# [TS_UI_API_THREE]

`three` is the render substrate of the `ui/viewer` Nx project (`scope:viewer`, compile-time excluded from the non-spatial majority): a single scene-graph + renderer + loader engine the `viewer/scene` plane composes into GLB residency and OpenPBR appearance. Two renderer backends live behind one contract — `WebGLRenderer` (core `three`) and `WebGPURenderer` (`three/webgpu`, an async-init unified `Renderer` carrying node-material + compute + post-processing surfaces) — so `viewer/scene/glb` selects a backend at boot without re-authoring the scene. GLB ingestion is a plugin-configured `GLTFLoader`: the DRACO, KTX2/Basis, and Meshopt codecs attach through `.setDRACOLoader`/`.setKTX2Loader`/`.setMeshoptDecoder` (the decode work handed to the `browser` decode-worker port, never run on the render thread), and `viewer/scene/appearance` binds the C# OpenPBR algebra field-for-field onto `MeshPhysicalMaterial`'s coat/sheen/transmission/iridescence/anisotropy lobes. three ships no first-party `.d.ts`; `@types/three` (catalog-bound) is the admitted compile-time declaration source, and every symbol here is verified against the shipped runtime exports (`three.module.js`/`three.webgpu.js`/`three.tsl.js`) and the `examples/jsm/**` addon source — DefinitelyTyped declarations never stand as member truth because they drift from the shipped surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package: `three` (MIT, © three.js authors)
- module format: ESM (`type: module`), `sideEffects: ["./src/nodes/**/*"]` — node-material TSL registration is the one retained side-effect root; four export roots: `three` (core), `three/webgpu` (WebGPU renderer + node materials + compute), `three/tsl` (node-shader language), `three/addons/*` ⇒ `three/examples/jsm/*` (loaders, controls, environments, codecs)
- runtime target: browser DOM + WebGL2/WebGPU context; no Node binding — the viewer runs it in the app render thread while codec transcoders run in the `browser` worker
- peer: none declared; the React seam is the app's own `<canvas>` ref + `useEffect` teardown, not a bundled binding
- asset: pure-JS runtime library shipping NO `.d.ts` (no `types` field, no bundled declarations); `@types/three` (catalog) is the admitted declaration companion for compile only. Members below are verified against the shipped `build/three.cjs` (441 core exports), `build/three.webgpu.js` (629 exports), `build/three.tsl.js` (638 exports), and the `examples/jsm/**` addon source on disk — never against DefinitelyTyped recall
- rail: viewer scene tier — the renderer/scene/loader/material engine `viewer/scene/glb` (residency) and `viewer/scene/appearance` (OpenPBR binding) compose; `scope:viewer` project-local

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph, cameras, and the transform spine
- rail: scene

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]   | [CONSUMER]                                     |
| :-----: | :------------------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `Scene` / `Object3D` / `Group`                                       | scene node      | `viewer/scene/glb` — residency root, GLB graft |
|  [02]   | `Mesh` / `InstancedMesh` / `BatchedMesh` / `SkinnedMesh`             | drawable        | `viewer/scene/glb` — repeats to one draw call  |
|  [03]   | `PerspectiveCamera` / `OrthographicCamera` / `ArrayCamera`           | camera          | `viewer/geo/project` — camera sync, bounds     |
|  [04]   | `Raycaster` / `Layers`                                               | pick / mask     | `viewer/mark/selection` — `GlobalId` hit-test  |
|  [05]   | `BufferGeometry` / `BufferAttribute` / `InterleavedBufferAttribute`  | geometry buffer | `viewer/scene/glb` — typed-array attributes    |
|  [06]   | `Box3` / `Sphere` / `Frustum` / `Vector3` / `Matrix4` / `Quaternion` | math value      | `viewer/geo`, `viewer/scene/glb` — transform   |

[PUBLIC_TYPE_SCOPE]: materials, textures, and color management — the OpenPBR binding target
- rail: scene

The OpenPBR lobe fields carried by `MeshPhysicalMaterial` are catalogued in the [03] appearance-binding rows.

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
- rail: scene

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]    | [CONSUMER]                                     |
| :-----: | :---------------------------------------------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `Renderer` / `WebGPUBackend` / `WebGLBackend`               | unified renderer | `viewer/scene/glb` — WebGPU/WebGL2 backend     |
|  [02]   | `MeshPhysicalNodeMaterial` / `MeshStandardNodeMaterial`     | node material    | `viewer/scene/appearance` — WebGPU OpenPBR TSL |
|  [03]   | `PostProcessing` / `RendererUtils`                          | post pipeline    | `viewer/probe/receipt` — `RenderReceipt` chain |
|  [04]   | `ComputeNode` / `StorageBufferNode`                         | GPU compute      | `viewer/scene/glb` — compute-pass transforms   |
|  [05]   | `StorageBufferAttribute` / `IndirectStorageBufferAttribute` | compute buffer   | `viewer/scene/glb` — storage/indirect attrs    |

[PUBLIC_TYPE_SCOPE]: loader and animation contracts
- rail: scene

Every row is consumed by `viewer/scene/glb`.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CONSUMER]                                            |
| :-----: | :--------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Loader` / `LoadingManager`                                      | loader base   | codec base; `LoadingManager` folds GLB progress/error |
|  [02]   | `GLTFLoader` (`three/addons`) / `GLTF` result                    | GLB loader    | parses to `{ scene, animations, cameras, asset }`     |
|  [03]   | `PMREMGenerator` / `WebGLCubeRenderTarget`                       | IBL prefilter | prefilters equirect/scene into the IBL mip-chain      |
|  [04]   | `AnimationMixer` / `AnimationClip` / `AnimationAction` / `Clock` | animation     | GLB tracks on a per-frame mixer + `Clock`             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: renderer construction and the frame loop
- rail: scene
- Both renderers take `{ canvas, antialias, alpha?, powerPreference? }`; every row serves `viewer/scene/glb`.

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]       | [CONSUMER]            |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------- | :-------------------- |
|  [01]   | `new WebGLRenderer(opts)` / `.setPixelRatio` / `.setSize`                                | build renderer       | WebGL2 canvas backend |
|  [02]   | `new WebGPURenderer(opts)` / `await renderer.init()` / `.hasFeature(name)`               | build (async)        | WebGPU async, [02]    |
|  [03]   | `renderer.setAnimationLoop(fn)` / `.render(s, c)` / `.renderAsync(s, c)`                 | frame loop           | RAF loop, submit      |
|  [04]   | `renderer.outputColorSpace` / `.toneMapping` / `.toneMappingExposure` / `.setClearColor` | output policy        | display contract      |
|  [05]   | `renderer.compileAsync(s, c)` / `renderer.dispose()` / `computeAsync(node)`              | precompile / dispose | pre-warm, dispose     |
|  [06]   | `renderer.info` — `.render` / `.compute` / `.memory` / `.autoReset` / `.reset()`         | counter surface      | probe metric sink, [06] |

- [02]-[WEBGPU_UPGRADE]: `hasFeature` (post-`init()`; `hasFeatureAsync` deprecated) gates the upgrade, resolving to `GPUDevice.features.has(name)` (`.api/webgpu-types.md`); the `null` loop pauses under `<Activity>` hidden.
- [06]-[COUNTER_SURFACE]: the unified `Renderer` base (so `WebGPURenderer` included) carries `info.render` `{calls, frameCalls, drawCalls, triangles, points, lines, timestamp}`, `info.compute` `{calls, frameCalls, timestamp}`, and `info.memory` `{geometries, textures, texturesSize, attributes, attributesSize, programs, renderTargets, readbackBuffers, uniformBuffers, total, …}` — the legacy `render.calls`/`render.triangles`/`memory.geometries`/`memory.textures` spellings hold, widened by per-frame and byte-grade members; an app-managed loop sets `info.autoReset = false` and calls `info.reset()` once per frame.

[ENTRYPOINT_SCOPE]: GLB ingestion — the plugin-configured decode pipeline
- rail: scene
- Every row serves `viewer/scene/glb`; codec wasm runs in the `browser` decode-worker, never the render thread.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [CONSUMER]             |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `new GLTFLoader(manager).setDRACOLoader(d).setKTX2Loader(k).setMeshoptDecoder(m)`             | codec setup    | injection, [01]        |
|  [02]   | `loader.register((parser) => plugin)` / `loader.parseAsync(buffer, path)` / `.loadAsync(url)` | parse          | buffer `parseAsync`    |
|  [03]   | `new DRACOLoader().setDecoderPath(p).setDecoderConfig({ type })` / `.preload()`               | draco codec    | geometry codec         |
|  [04]   | `new KTX2Loader().setTranscoderPath(p).detectSupport(renderer)`                               | basis codec    | transcode target, [04] |
|  [05]   | `KTX2Loader` / `RGBELoader` / `TextureLoader` → `renderer.initTexture(tex)`                   | texture ingest | eager `initTexture`    |

- [01]-[MESHOPT_GATE]: `MeshoptDecoder` wires only when the asset flags `EXT_meshopt_compression`.
- [04]-[TRANSCODE_TARGET]: `detectSupport` reads the renderer's compressed-format capabilities — `GPUDevice.features` for the `texture-compression-bc`/`-etc2`/`-astc` `GPUFeatureName` members (`.api/webgpu-types.md`) on WebGPU, `WEBGL_compressed_texture_*` extensions on WebGL.

[ENTRYPOINT_SCOPE]: OpenPBR appearance binding on `MeshPhysicalMaterial`
- rail: scene
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

[ENTRYPOINT_SCOPE]: interaction, node-shader authoring, and evidence
- rail: scene
- Node-shader verbs are `three/tsl`; `BufferGeometryUtils` is `three/addons`; `OrbitControls extends Controls extends EventDispatcher`.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]    | [CONSUMER]                              |
| :-----: | :------------------------------------------------------------------- | :---------------- | :-------------------------------------- |
|  [01]   | `new OrbitControls(cam, dom)` / `ArcballControls` / `MapControls`    | camera control    | `viewer/geo/project` — camera nav       |
|  [02]   | `controls.update()` / `controls.dispose()`                           | control lifecycle | `viewer/geo` — update, `Scope` dispose  |
|  [03]   | `Fn(...)` / `uniform` / `texture` / `vec3` / `mix` / `mrt`           | node shader       | `viewer/scene/appearance` — TSL lobes   |
|  [04]   | `renderer.getPixelRatio()` / `.readRenderTargetPixelsAsync(rt, ...)` | frame readback    | `viewer/probe/receipt` — pixel readback |
|  [05]   | `BufferGeometryUtils.mergeGeometries(list)` / `.mergeVertices(geo)`  | geometry fold     | `viewer/scene/glb` — merge submeshes    |
|  [06]   | `controls.addEventListener("change", fn)` / `controls.target`        | control events    | `viewer/geo` — `change` folds camera    |

[ENTRYPOINT_SCOPE]: animation drive and draw-call collapse
- rail: scene
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
- GLB decode is dependency injection, not a monolith: `GLTFLoader` holds a DRACO codec, a KTX2 codec, and a Meshopt decoder as *attached* collaborators. The residency row wires each codec's wasm transcoder to run in the `browser` decode-worker (`DRACOLoader.setWorkerLimit`, `KTX2Loader` worker pool, `MeshoptDecoder`), so heavy transcode never blocks the render thread. The meshopt row is gated: attach `MeshoptDecoder` only when the asset declares `EXT_meshopt_compression`.
- `MeshPhysicalMaterial` is the single OpenPBR bind target: its coat/sheen/transmission/iridescence/anisotropy/specular fields are a superset of the glTF PBR extensions and map field-for-field onto the C# OpenPBR algebra projected through `wire#vocab` appearance. The WebGPU path swaps in `MeshPhysicalNodeMaterial` (same lobes as TSL node graphs) with no change to the appearance-decode row.
- Image-based lighting is a prefilter step, not a runtime cost: `PMREMGenerator.fromScene`/`fromEquirectangular` bakes the environment into a roughness-mip cube once; `scene.environment` then serves every physical material's IBL sample. The render loop stays a pure `setAnimationLoop(() => renderer.render(scene, camera))`.

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): the renderer, controls, loaders, geometries, and prefilter targets are all GPU-resource owners — each construction is acquired inside an `Effect.acquireRelease`/`Scope` whose finalizer calls `.dispose()`, so a viewport unmount releases every GPU handle deterministically. The frame loop is an interruptible `Effect` fiber, paused when `<Activity>` marks the viewport hidden. `Match`/`Schema` classify the decoded `wire` appearance before it reaches `MeshPhysicalMaterial`.
- `@webgpu/types` (`.api/webgpu-types.md`): supplies the `GPUDevice`/`GPUAdapter`/`GPUTextureFormat` ambient types the `three/webgpu` backend and the `KTX2Loader.detectSupport` capability probe reference — the backend requests an adapter/device through `navigator.gpu` and gates the upgrade on `renderer.hasFeature` (→ `GPUDevice.features.has`), while `detectSupport` reads that same `GPUDevice.features` for the `texture-compression-bc`/`-etc2`/`-astc` `GPUFeatureName` members to pick the transcode target; the two are the WebGPU-path type + runtime halves (types-only ambient package, admitted by a viewer-tsconfig `types` entry, never imported).
- `@google/model-viewer` (`.api/google-model-viewer.md`): the declarative `<model-viewer>` custom-element row for the simple embed case; three is the imperative full-control path when the viewer needs custom materials, compute, selection ray-casting, or the render receipt. The two are siblings, not layers, over ONE shared three module: `<model-viewer>` peers `three@^catalog`, filled by this same workspace `three@catalog` (pnpm dedupes it onto the single physical copy — NOT a bundled second three), so the element re-exports these exact `catalog` classes yet owns its own internal renderer instance + `<canvas>` + GL context. The `RendererBackend` `Schema.Literal("three", "model-viewer")` literal picks which path draws a given GLB; they meet at the GLB wire.
- `browser` decode-worker port (declared by `viewer/scene/glb` as `GlbViewport` residency, Layer-provided by `browser`): three's codec transcoders (DRACO wasm, KTX2 Basis, Meshopt) run behind this port, never on the render thread; `ui` declares the port, `browser` provides the worker Layer at app composition.
- `wire#vocab` appearance (`csharp:Rasm.Materials/Appearance/interchange` seam): `viewer/scene/appearance` decodes `MaterialWire`/`OpenPbrGroupsWire`/`AppearanceSummary` once at `wire` and binds the decoded values onto `MeshPhysicalMaterial`; a peer re-mint of the OpenPBR algebra in TypeScript is the `CROSS_LANGUAGE_WIRE` drift defect.
- `colorjs.io` (`.api/colorjs.io.md`) + `three` `ColorManagement`: theme/appearance colors are authored in OKLCH and gamut-fit in `colorjs.io`, then `to("srgb"/"srgb-linear")`-converted into `coords` whose ids are the exact string values of three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants; `Color.setRGB(r, g, b, colorSpace)` ingests them and `ColorManagement` transforms source→working, so token color and rendered color share one color-space contract.

[LOCAL_ADMISSION]:
- Import core `three` for the scene/camera/material/geometry spine, `three/webgpu` only inside the WebGPU-backend construction row, `three/tsl` only inside node-material authoring, and `three/addons/*` for `GLTFLoader`/controls/codecs/`RoomEnvironment` — never deep-import `three/examples/jsm/...` by raw path when the `three/addons/*` export alias resolves it.
- Attach codecs to `GLTFLoader` by injection (`setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder`); never fork the loader per codec or decode DRACO/KTX2/Meshopt by hand — the codec wasm belongs in the `browser` decode-worker behind the port.
- Bind OpenPBR appearance onto `MeshPhysicalMaterial`/`MeshPhysicalNodeMaterial` fields decoded from `wire#vocab`; never re-derive the OpenPBR→glTF-PBR mapping in `ui` (that algebra is owned in C# and mirrored once at `wire`).
- Acquire every renderer/geometry/material/texture/control/prefilter under an Effect `Scope` and dispose in the finalizer; never let a GPU-resource owner outlive its viewport — three has no GC for GPU memory.
- Prefilter environments with `PMREMGenerator` once and serve via `scene.environment`; never sample a raw equirect texture per-fragment or rebuild the IBL mip-chain per frame.
- Verify three member existence against the shipped runtime exports (three ships no `.d.ts`); never trust a `three` signature from `@types/three` recall — the DefinitelyTyped declarations compile the code but drift from the shipped catalog-bound surface, so runtime exports decide member truth.

[RAIL_LAW]:
- Package: `three` (`scope:viewer` project-local)
- Owns: the WebGL2/WebGPU renderer backends, the `Scene`/`Object3D`/`Mesh` residency graph, the material family (`MeshPhysicalMaterial` OpenPBR lobes + node-material mirror), the loader family (`GLTFLoader` + injected DRACO/KTX2/Meshopt codecs), IBL prefilter (`PMREMGenerator`), animation, camera controls, TSL node-shader authoring, and GPU compute
- Accept: one `Scene` residency root keyed by content-key, backend selection at boot behind one usage contract, codec injection into `GLTFLoader`, OpenPBR bind onto `MeshPhysicalMaterial` from `wire#vocab`, `Scope`-scoped GPU-resource disposal, `PMREMGenerator` IBL prefilter, codec transcode behind the `browser` decode-worker port
- Reject: `@types/three` as the member-truth source, per-codec loader forks or hand-rolled DRACO/KTX2/Meshopt decode, OpenPBR-mapping re-mint in `ui`, GPU-resource owners outliving their viewport, per-frame IBL rebuild, deep raw `examples/jsm/...` imports where `three/addons/*` resolves, render-thread codec transcode
