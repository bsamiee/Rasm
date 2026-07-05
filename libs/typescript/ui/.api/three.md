# [three] — WebGL/WebGPU scene graph rendering the GLB residency rows; scope:viewer project-local

`three` is the render substrate of the `ui/viewer` Nx project (`scope:viewer`, compile-time excluded from the non-spatial majority): a single scene-graph + renderer + loader engine the `viewer/scene` plane composes into GLB residency and OpenPBR appearance. Two renderer backends live behind one contract — `WebGLRenderer` (core `three`) and `WebGPURenderer` (`three/webgpu`, an async-init unified `Renderer` carrying node-material + compute + post-processing surfaces) — so `viewer/scene/glb` selects a backend at boot without re-authoring the scene. GLB ingestion is a plugin-configured `GLTFLoader`: the DRACO, KTX2/Basis, and Meshopt codecs attach through `.setDRACOLoader`/`.setKTX2Loader`/`.setMeshoptDecoder` (the decode work handed to the `browser` decode-worker port, never run on the render thread), and `viewer/scene/appearance` binds the C# OpenPBR algebra field-for-field onto `MeshPhysicalMaterial`'s coat/sheen/transmission/iridescence/anisotropy lobes. three ships no first-party `.d.ts`; `@types/three` (0.185.0) is the admitted compile-time declaration source, and every symbol here is verified against the shipped runtime exports (`three.module.js`/`three.webgpu.js`/`three.tsl.js`) and the `examples/jsm/**` addon source — DefinitelyTyped declarations never stand as member truth because they drift from the shipped surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three`
- package: `three` (0.185.1, MIT, © three.js authors)
- module format: ESM (`type: module`), `sideEffects: ["./src/nodes/**/*"]` — node-material TSL registration is the one retained side-effect root; four export roots: `three` (core), `three/webgpu` (WebGPU renderer + node materials + compute), `three/tsl` (node-shader language), `three/addons/*` ⇒ `three/examples/jsm/*` (loaders, controls, environments, codecs)
- runtime target: browser DOM + WebGL2/WebGPU context; no Node binding — the viewer runs it in the app render thread while codec transcoders run in the `browser` worker
- peer: none declared; the React seam is the app's own `<canvas>` ref + `useEffect` teardown, not a bundled binding
- asset: pure-JS runtime library shipping NO `.d.ts` (no `types` field, no bundled declarations); `@types/three` (0.185.0) is the admitted declaration companion for compile only. Members below are verified against the shipped `build/three.cjs` (441 core exports), `build/three.webgpu.js` (629 exports), `build/three.tsl.js` (638 exports), and the `examples/jsm/**` addon source on disk — never against DefinitelyTyped recall
- rail: viewer scene tier — the renderer/scene/loader/material engine `viewer/scene/glb` (residency) and `viewer/scene/appearance` (OpenPBR binding) compose; `scope:viewer` project-local

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene graph, cameras, and the transform spine
- rail: scene

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Scene` / `Object3D` / `Group`                                 | scene node        | `viewer/scene/glb` — the residency root; every loaded GLB node is an `Object3D` subtree grafted under one `Scene` |
|  [02]   | `Mesh` / `InstancedMesh` / `BatchedMesh` / `SkinnedMesh`       | drawable          | `viewer/scene/glb` — `BatchedMesh`/`InstancedMesh` collapse repeated element geometry into one draw call for large models |
|  [03]   | `PerspectiveCamera` / `OrthographicCamera` / `ArrayCamera`     | camera            | `viewer/geo/project` — projection/camera sync; `viewer/scene/glb` frames the loaded bounds |
|  [04]   | `Raycaster` / `Layers`                                         | pick / mask       | `viewer/mark/selection` — pointer→`GlobalId` element hit-test over the residency graph |
|  [05]   | `BufferGeometry` / `BufferAttribute` / `InterleavedBufferAttribute` | geometry buffer | `viewer/scene/glb` — decoded vertex/index buffers land as typed-array attributes, never per-vertex objects |
|  [06]   | `Box3` / `Sphere` / `Frustum` / `Vector3` / `Matrix4` / `Quaternion` | math value  | `viewer/geo/project`, `viewer/scene/glb` — bounds, framing, frustum-cull, and the transform algebra |

[PUBLIC_TYPE_SCOPE]: materials, textures, and color management — the OpenPBR binding target
- rail: scene

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `MeshPhysicalMaterial`                                         | PBR material      | `viewer/scene/appearance` — the OpenPBR bind target; carries `clearcoat*`, `sheen*`, `transmission`/`thickness`/`attenuation*`, `ior`, `iridescence*`, `specularIntensity`/`specularColor`, `anisotropy*`, `dispersion` |
|  [02]   | `MeshStandardMaterial` / `MeshBasicMaterial` / `ShaderMaterial` | material base    | `viewer/scene/appearance` — metallic-roughness base, unlit fallback, and the raw-shader escape hatch |
|  [03]   | `Texture` / `CompressedTexture` / `CompressedArrayTexture` / `DataTexture` | texture   | `viewer/scene/glb` — KTX2/Basis transcode lands as a `CompressedTexture`; HDR env as `DataTexture` |
|  [04]   | `Color` / `ColorManagement` / `SRGBColorSpace` / `LinearSRGBColorSpace` | color space | `viewer/scene/appearance` — the working/output color-space contract the OpenPBR albedo decode honors |
|  [05]   | `ACESFilmicToneMapping` / `AgXToneMapping` / `NeutralToneMapping` | tone map enum   | `viewer/scene/glb` — the renderer tone-map policy row; AgX/Neutral for physically-faithful HDR |
|  [06]   | `DirectionalLight` / `AmbientLight` / `HemisphereLight` / `RectAreaLight` / `LightProbe` | light | `viewer/scene/glb` — analytic light rows beside the IBL environment |

[PUBLIC_TYPE_SCOPE]: WebGPU backend, node materials, and compute (`three/webgpu`)
- rail: scene

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Renderer` / `WebGPUBackend` / `WebGLBackend`                  | unified renderer  | `viewer/scene/glb` — one `Renderer` abstract over a WebGPU or WebGL2 backend chosen by the `navigator.gpu` + `renderer.hasFeature` capability probe (post-`init()`) |
|  [02]   | `MeshPhysicalNodeMaterial` / `MeshStandardNodeMaterial`        | node material     | `viewer/scene/appearance` — the WebGPU-path OpenPBR material whose lobes are TSL node graphs, not fixed uniforms |
|  [03]   | `PostProcessing` / `RendererUtils`                             | post pipeline     | `viewer/probe/receipt` — the post chain whose framebuffer feeds the `RenderReceipt` frame-hash |
|  [04]   | `ComputeNode` / `StorageBufferNode` / `StorageBufferAttribute` / `IndirectStorageBufferAttribute` | GPU compute | `viewer/scene/glb` — GPU-side buffer transforms (skinning, culling) as compute passes, WebGPU-gated |

[PUBLIC_TYPE_SCOPE]: loader and animation contracts
- rail: scene

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Loader` / `LoadingManager`                                    | loader base       | `viewer/scene/glb` — the one loader base every codec extends; `LoadingManager` folds progress/error across the GLB dependency set |
|  [02]   | `GLTFLoader` (`three/addons`) / `GLTF` result                 | GLB loader        | `viewer/scene/glb` — parses the GLB to a `{ scene, animations, cameras, asset }` result; plugin-configured for DRACO/KTX2/Meshopt |
|  [03]   | `PMREMGenerator` / `WebGLCubeRenderTarget`                     | IBL prefilter     | `viewer/scene/glb` — prefilters an equirect/scene environment into the mip-chain `MeshPhysicalMaterial` samples for image-based lighting |
|  [04]   | `AnimationMixer` / `AnimationClip` / `AnimationAction` / `Clock` | animation       | `viewer/scene/glb` — GLB animation tracks bound to a per-frame mixer under one `Clock` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: renderer construction and the frame loop
- rail: scene

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new WebGLRenderer({ canvas, antialias, alpha, powerPreference })` / `.setPixelRatio` / `.setSize` | build renderer | `viewer/scene/glb` — the WebGL2 backend bound to the viewer `<canvas>` ref |
|  [02]   | `new WebGPURenderer({ canvas, antialias })` / `await renderer.init()` / `.hasFeature(name)` | build renderer (async) | `viewer/scene/glb` — the WebGPU backend; `init()` awaits adapter/device, `hasFeature` (post-`init()`; `hasFeatureAsync` is deprecated) gates the upgrade, resolving to `GPUDevice.features.has(name)` (`.api/webgpu-types.md`) |
|  [03]   | `renderer.setAnimationLoop(fn)` / `.render(scene, camera)` / `.renderAsync(scene, camera)`      | frame loop     | `viewer/scene/glb` — the RAF-driven loop; `renderAsync` is the WebGPU submit; `null` loop pauses under `<Activity>` hidden |
|  [04]   | `renderer.outputColorSpace` / `.toneMapping` / `.toneMappingExposure` / `.setClearColor`        | output policy  | `viewer/scene/glb` — output color-space + tone-map + exposure; the physically-faithful display contract |
|  [05]   | `renderer.compileAsync(scene, camera)` / `renderer.dispose()` / `computeAsync(node)`            | precompile / dispose | `viewer/scene/glb` — pre-warm shaders before first frame; `dispose()` under the Effect `Scope` finalizer releases GPU memory |

[ENTRYPOINT_SCOPE]: GLB ingestion — the plugin-configured decode pipeline
- rail: scene

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new GLTFLoader(manager).setDRACOLoader(d).setKTX2Loader(k).setMeshoptDecoder(MeshoptDecoder)`  | configure codecs | `viewer/scene/glb` — one loader, codecs attached by injection; the meshopt-gated row wires `MeshoptDecoder` only when the asset flags `EXT_meshopt_compression` |
|  [02]   | `loader.register((parser) => plugin)` / `loader.parseAsync(buffer, path)` / `.loadAsync(url)`   | parse          | `viewer/scene/glb` — `parseAsync` over the residency `ArrayBuffer` (already content-key-verified at `wire`); `register` adds an extension plugin |
|  [03]   | `new DRACOLoader().setDecoderPath(p).setDecoderConfig({ type })` / `.preload()`                 | draco codec    | `viewer/scene/glb` — the geometry-decompression codec; its wasm runs in the `browser` decode-worker, not the render thread |
|  [04]   | `new KTX2Loader().setTranscoderPath(p).detectSupport(renderer)`                                 | basis codec    | `viewer/scene/glb` — GPU-texture transcode; `detectSupport` reads the renderer's compressed-format capabilities to pick the target: `GPUDevice.features` for the `texture-compression-bc`/`-etc2`/`-astc` `GPUFeatureName` members (`.api/webgpu-types.md`) on the WebGPU backend, `WEBGL_compressed_texture_*` extensions on WebGL |
|  [05]   | `KTX2Loader` / `RGBELoader` / `TextureLoader` → `renderer.initTexture(tex)`                     | texture ingest | `viewer/scene/glb` — compressed/HDR/8-bit texture rows; `initTexture` uploads eagerly to avoid first-frame hitching |

[ENTRYPOINT_SCOPE]: OpenPBR appearance binding on `MeshPhysicalMaterial`
- rail: scene

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `material.clearcoat` / `.clearcoatRoughness` / `.clearcoatMap` / `.clearcoatNormalMap`          | coat lobe      | `viewer/scene/appearance` — the OpenPBR coat weight/roughness/maps decoded from `wire#vocab` appearance |
|  [02]   | `.sheen` / `.sheenColor` / `.sheenRoughness` — `.transmission` / `.thickness` / `.attenuationColor` / `.attenuationDistance` / `.ior` | sheen / transmission | `viewer/scene/appearance` — fuzz and volumetric/refraction lobes; `ior`+`dispersion` the glass rows |
|  [03]   | `.iridescence` / `.iridescenceIOR` / `.iridescenceThicknessRange` — `.anisotropy` / `.anisotropyRotation` | thin-film / aniso | `viewer/scene/appearance` — thin-film interference and anisotropic-highlight lobes with their maps |
|  [04]   | `.specularIntensity` / `.specularColor` / `.metalness` / `.roughness` / `.envMapIntensity` — `.emissive` / `.emissiveIntensity` / `.opacity` / `.transparent` / `.side` (`FrontSide`/`DoubleSide`) | base + emission + geometry + IBL | `viewer/scene/appearance` — the metallic-roughness base, the emission pair, the opacity/side geometry rows, and the IBL contribution scale |
|  [05]   | `pmrem.fromScene(RoomEnvironment(), 0.04)` / `pmrem.fromEquirectangular(hdr)` → `scene.environment` | IBL bind    | `viewer/scene/glb` — the prefiltered environment map every `MeshPhysicalMaterial` samples; `RoomEnvironment` is the neutral studio default |

[ENTRYPOINT_SCOPE]: interaction, node-shader authoring, and evidence
- rail: scene

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new OrbitControls(camera, dom)` / `ArcballControls` / `MapControls` — `.update()` / `.dispose()` | camera control | `viewer/geo/project` — orbit/arcball/map navigation rows; disposed under the Effect `Scope` finalizer |
|  [02]   | `Fn(([...]) => node)` / `uniform(v)` / `texture(t, uv)` / `vec3` / `mix` / `mrt({...})` (`three/tsl`) | node shader   | `viewer/scene/appearance` — TSL node graphs authoring the WebGPU-path material lobes; `mrt` for the G-buffer receipt targets |
|  [03]   | `renderer.getPixelRatio()` / `renderer.readRenderTargetPixelsAsync(rt, ...)`                    | frame readback | `viewer/probe/receipt` — the pixel readback the `RenderReceipt` frame-hash consumes for deterministic render evidence |
|  [04]   | `BufferGeometryUtils.mergeGeometries(list)` / `.mergeVertices(geo)` (`three/addons`)            | geometry fold  | `viewer/scene/glb` — merges same-material submeshes into one buffer, cutting draw calls on large residency sets |
|  [05]   | `controls.addEventListener("change", fn)` / `controls.target`                                  | control events | `viewer/geo` — `OrbitControls extends Controls extends EventDispatcher`; the `change` dispatch folds the settled camera into the atom |

[ENTRYPOINT_SCOPE]: animation drive and draw-call collapse
- rail: scene

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new AnimationMixer(root)` / `mixer.clipAction(clip)` / `mixer.update(deltaTime)` / `mixer.timeScale` | mixer drive    | `viewer/scene` — one mixer per animated graft; `update` advances inside the one frame loop |
|  [02]   | `action.play()` / `.stop()` / `.reset()` / `.setLoop(mode, repetitions)` / `.paused` / `.enabled` — `LoopOnce` / `LoopRepeat` / `LoopPingPong` | action control | `viewer/scene` — clip playback policy; loop-mode constants from core `three` |
|  [03]   | `mixer.stopAllAction()` / `mixer.uncacheRoot(root)`                                             | mixer teardown | `viewer/scene` — eviction releases every action binding before the subtree disposes |
|  [04]   | `new Clock()` / `clock.getDelta()` / `clock.getElapsedTime()`                                   | time source    | `viewer/scene` — the one delta source the frame loop feeds every mixer |
|  [05]   | `new InstancedMesh(geometry, material, count)` / `.setMatrixAt(i, matrix)` / `.instanceMatrix` / `.computeBoundingSphere()` | instanced draw | `viewer/scene` — repeated identical geometry as one draw call |
|  [06]   | `new BatchedMesh(maxInstances, maxVertices, maxIndices, material)` / `.addGeometry(geo)` / `.addInstance(geoId)` / `.setMatrixAt(i, matrix)` / `.setVisibleAt(i, flag)` | batched draw   | `viewer/scene` — distinct same-material geometries batched into one draw call with per-instance visibility |
|  [07]   | `renderer.initTexture(texture)`                                                                 | eager upload   | `viewer/scene` — uploads decoded textures ahead of first use so the first frame never hitches |

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
- `@google/model-viewer` (`.api/google-model-viewer.md`): the declarative `<model-viewer>` custom-element row for the simple embed case; three is the imperative full-control path when the viewer needs custom materials, compute, selection ray-casting, or the render receipt. The two are siblings, not layers, over ONE shared three module: `<model-viewer>` peers `three@^0.183.0`, filled by this same workspace `three@0.185.1` (pnpm dedupes it onto the single physical copy — NOT a bundled second three), so the element re-exports these exact `0.185.1` classes yet owns its own internal renderer instance + `<canvas>` + GL context. The `RendererBackend` `Schema.Literal("three", "model-viewer")` literal picks which path draws a given GLB; they meet at the GLB wire.
- `browser` decode-worker port (declared by `viewer/scene/glb` as `GlbViewport` residency, Layer-provided by `browser`): three's codec transcoders (DRACO wasm, KTX2 Basis, Meshopt) run behind this port, never on the render thread; `ui` declares the port, `browser` provides the worker Layer at app composition.
- `wire#vocab` appearance (`csharp:Rasm.Materials/Appearance/interchange` seam): `viewer/scene/appearance` decodes `MaterialWire`/`OpenPbrGroupsWire`/`AppearanceSummary` once at `wire` and binds the decoded values onto `MeshPhysicalMaterial`; a peer re-mint of the OpenPBR algebra in TypeScript is the `CROSS_LANGUAGE_WIRE` drift defect.
- `colorjs.io` (`.api/colorjs.io.md`) + `three` `ColorManagement`: theme/appearance colors are authored in OKLCH and gamut-fit in `colorjs.io`, then `to("srgb"/"srgb-linear")`-converted into `coords` whose ids are the exact string values of three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants; `Color.setRGB(r, g, b, colorSpace)` ingests them and `ColorManagement` transforms source→working, so token color and rendered color share one color-space contract.

[LOCAL_ADMISSION]:
- Import core `three` for the scene/camera/material/geometry spine, `three/webgpu` only inside the WebGPU-backend construction row, `three/tsl` only inside node-material authoring, and `three/addons/*` for `GLTFLoader`/controls/codecs/`RoomEnvironment` — never deep-import `three/examples/jsm/...` by raw path when the `three/addons/*` export alias resolves it.
- Attach codecs to `GLTFLoader` by injection (`setDRACOLoader`/`setKTX2Loader`/`setMeshoptDecoder`); never fork the loader per codec or decode DRACO/KTX2/Meshopt by hand — the codec wasm belongs in the `browser` decode-worker behind the port.
- Bind OpenPBR appearance onto `MeshPhysicalMaterial`/`MeshPhysicalNodeMaterial` fields decoded from `wire#vocab`; never re-derive the OpenPBR→glTF-PBR mapping in `ui` (that algebra is owned in C# and mirrored once at `wire`).
- Acquire every renderer/geometry/material/texture/control/prefilter under an Effect `Scope` and dispose in the finalizer; never let a GPU-resource owner outlive its viewport — three has no GC for GPU memory.
- Prefilter environments with `PMREMGenerator` once and serve via `scene.environment`; never sample a raw equirect texture per-fragment or rebuild the IBL mip-chain per frame.
- Verify three member existence against the shipped runtime exports (three ships no `.d.ts`); never trust a `three` signature from `@types/three` recall — the DefinitelyTyped declarations compile the code but drift from the shipped 0.185.1 surface, so runtime exports decide member truth.

[RAIL_LAW]:
- Package: `three` (`scope:viewer` project-local)
- Owns: the WebGL2/WebGPU renderer backends, the `Scene`/`Object3D`/`Mesh` residency graph, the material family (`MeshPhysicalMaterial` OpenPBR lobes + node-material mirror), the loader family (`GLTFLoader` + injected DRACO/KTX2/Meshopt codecs), IBL prefilter (`PMREMGenerator`), animation, camera controls, TSL node-shader authoring, and GPU compute
- Accept: one `Scene` residency root keyed by content-key, backend selection at boot behind one usage contract, codec injection into `GLTFLoader`, OpenPBR bind onto `MeshPhysicalMaterial` from `wire#vocab`, `Scope`-scoped GPU-resource disposal, `PMREMGenerator` IBL prefilter, codec transcode behind the `browser` decode-worker port
- Reject: `@types/three` as the member-truth source, per-codec loader forks or hand-rolled DRACO/KTX2/Meshopt decode, OpenPBR-mapping re-mint in `ui`, GPU-resource owners outliving their viewport, per-frame IBL rebuild, deep raw `examples/jsm/...` imports where `three/addons/*` resolves, render-thread codec transcode
