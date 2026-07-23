# [TS_UI_API_WEBGPU_TYPES]

`@webgpu/types` is the WebGPU spec surface as TypeScript: a type-only, zero-runtime package that ambiently augments `navigator.gpu`, `HTMLCanvasElement.getContext("webgpu")`, and `OffscreenCanvas.getContext("webgpu")`, and declares the entire GPU object graph — `GPU` → `GPUAdapter` → `GPUDevice` → the queue/resource/pipeline/encoder tree — plus every descriptor interface, the string-literal enum vocabularies (`GPUTextureFormat`, `GPUFeatureName`, `GPUVertexFormat`, …), the flag namespaces (`GPUBufferUsage`/`GPUTextureUsage`/`GPUShaderStage`/`GPUColorWrite`/`GPUMapMode`), and the error/device-loss surface (`GPUValidationError`, `GPUDeviceLostInfo`, `GPUUncapturedErrorEvent`). It is admitted by a tsconfig `compilerOptions.types` entry (or a `/// <reference types="@webgpu/types" />`), NEVER an `import` — it types global runtime objects the code already calls, and ships zero bytes. It is `scope:viewer` project-local: only the `ui/viewer` Nx project's tsconfig lists it, so the WebGPU types are compile-time absent from the non-spatial app majority. Its sole purpose is to make `three`'s `three/webgpu` path type-check — the `WebGPURenderer` requests an adapter through `navigator.gpu`, TSL `ComputeNode`s compile to WGSL running on a `GPUDevice`, the viewer negotiates features (`GPUFeatureName`, `requiredLimits`) and handles device loss, and `KTX2Loader.detectSupport` reads `GPUDevice.features` for the `texture-compression-*` `GPUFeatureName` members to choose the GLB transcode target — none of which compiles without these globals present. It tracks the living WebGPU IDL, so the version moves as the spec does; it carries no other admitted-lib seam because it is types under the renderer, not a runtime peer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@webgpu/types`
- package: `@webgpu/types` (BSD-3-Clause)
- module format: type-only ambient declarations (`types: dist/index.d.ts`); NO `main`/`module`, NO runtime code, NO exported values — it declares `global`-scope interfaces and augments `Navigator`/`WorkerNavigator`/`HTMLCanvasElement`/`OffscreenCanvas`
- deps: none; peer: none — a leaf type package
- admission: a tsconfig `compilerOptions.types: ["@webgpu/types"]` entry OR a `/// <reference types="@webgpu/types" />` directive; NEVER `import` — the augmentation is global, resolving `navigator.gpu` and `canvas.getContext("webgpu")` wherever they are called
- runtime target: none (types only); the runtime is the browser's own WebGPU implementation. Ships zero bytes
- scope: `scope:viewer` project-local — listed only in the `ui/viewer` Nx project tsconfig, compile-time excluded from non-spatial apps
- rail: type substrate (types `viewer/scene/glb` under `three`'s WebGPU backend and any raw-WebGPU compute path)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: entry + device negotiation — the adapter/device request chain and its capability vocabulary
- rail: type substrate
- `navigator.gpu` (`GPU`) is the root; `requestAdapter` → `GPUAdapter` → `requestDevice` is the async negotiation the viewer runs once at boot. Features and limits are the capability gate — the WebGL2-recovery decision reads them.

- [01]-[ENTRY]: `NavigatorGPU` (`readonly gpu: GPU`) / `GPU` (`requestAdapter(options?)` `getPreferredCanvasFormat(): GPUTextureFormat` `wgslLanguageFeatures`) — `navigator.gpu.requestAdapter()`; `getPreferredCanvasFormat()` picks the swap-chain format.
- [02]-[ADAPTER]: `GPUAdapter` (`features: GPUSupportedFeatures` `limits: GPUSupportedLimits` `info: GPUAdapterInfo` `requestDevice(descriptor?)`) / `GPURequestAdapterOptions` (`powerPreference` `forceFallbackAdapter` `featureLevel`) — the capability probe; `powerPreference: 'high-performance'` selects the discrete GPU on a multi-GPU laptop.
- [03]-[DEVICE_REQUEST]: `GPUDeviceDescriptor` (`requiredFeatures` `requiredLimits` `defaultQueue`) / `GPUSupportedFeatures` (`ReadonlySet<string>`) / `GPUSupportedLimits` / `GPUAdapterInfo` — `requiredFeatures: GPUFeatureName[]` negotiates capabilities up front; an unsupported feature rejects the request and the viewer falls back.
- [04]-[CAPABILITY_VOCAB]: `GPUFeatureName` (`'depth-clip-control'`/`'timestamp-query'`/`'float32-filterable'`/`'texture-compression-bc'`/`'texture-compression-etc2'`/`'texture-compression-astc'`/…) / `GPUPowerPreference` (`'low-power'`/`'high-performance'`) — one union owns the whole capability axis; the `texture-compression-*` members are what `KTX2Loader.detectSupport` probes through `GPUDevice.features` to pick the GLB transcode target.

[PUBLIC_TYPE_SCOPE]: the device object graph — resources, pipelines, encoders (the render/compute machine)
- rail: type substrate
- `GPUDevice` is the factory for every resource and pipeline; `GPUQueue` submits command buffers; the encoders record passes. This is the graph `three`'s backend drives and a raw compute path composes directly.

- [01]-[DEVICE]: `GPUDevice` (`createBuffer`/`createTexture`/`createSampler`/`createBindGroup`/`createShaderModule`/`createRenderPipeline`/`createComputePipeline`/`createCommandEncoder`, `createRenderPipelineAsync`/`createComputePipelineAsync`, `queue`, `features`, `limits`, `lost`, `pushErrorScope`/`popErrorScope`, `destroy`) — the resource + pipeline factory; `features: GPUSupportedFeatures` is the set `renderer.hasFeature(...)` and `KTX2Loader.detectSupport` read for `texture-compression-*`; `lost: Promise<GPUDeviceLostInfo>` is the device-loss re-init seam.
- [02]-[SUBMISSION]: `GPUQueue` (`submit`, `writeBuffer`, `writeTexture`, `copyExternalImageToTexture`, `onSubmittedWorkDone`) / `GPUCommandBuffer` — `queue.submit([encoder.finish()])` per frame; `writeBuffer` uploads geometry/uniforms.
- [03]-[RESOURCES]: `GPUBuffer` (`mapAsync`, `getMappedRange`, `unmap`, `size`, `usage`, `mapState`) / `GPUTexture` (`createView`, `destroy`) / `GPUTextureView` / `GPUSampler` / `GPUExternalTexture` / `GPUQuerySet` — GPU-resident mesh/texture data; `mapAsync`+`getMappedRange` is the CPU↔GPU readback (GPU picking, compute results).
- [04]-[PIPELINES]: `GPURenderPipeline` / `GPUComputePipeline` (`getBindGroupLayout`) / `GPUBindGroup` / `GPUBindGroupLayout` / `GPUPipelineLayout` / `GPUShaderModule` (`getCompilationInfo`) — the compiled render/compute programs and their binding layouts; `getCompilationInfo` surfaces WGSL diagnostics.
- [05]-[ENCODERS]: `GPUCommandEncoder` (`beginRenderPass`/`beginComputePass`/`copyBufferToBuffer`/`copyTextureToBuffer`/`finish`) / `GPURenderPassEncoder` (`setPipeline`/`setBindGroup`/`setVertexBuffer`/`draw`/`drawIndexed`/`drawIndirect`/`end`) / `GPUComputePassEncoder` (`dispatchWorkgroups`/`dispatchWorkgroupsIndirect`/`end`) / `GPURenderBundleEncoder` / `GPURenderBundle` — the per-frame command recording; `dispatchWorkgroups` runs a compute shader (meshopt decode, GPU picking, particle sim).

[PUBLIC_TYPE_SCOPE]: descriptors — the create-time config for every resource and pipeline
- rail: type substrate
- one descriptor interface per `create*` call; these are the parameterized config objects, keyed by the enum vocabularies below.

- [01]-[RESOURCE_CONFIG]: `GPUBufferDescriptor` (`size`, `usage`, `mappedAtCreation`) / `GPUTextureDescriptor` (`size`, `format`, `usage`, `dimension`, `mipLevelCount`, `sampleCount`) / `GPUSamplerDescriptor` / `GPUTextureViewDescriptor` — `usage` is a `GPUBufferUsage`/`GPUTextureUsage` flag OR; `format` a `GPUTextureFormat`.
- [02]-[PIPELINE_CONFIG]: `GPURenderPipelineDescriptor` (`vertex`, `fragment`, `primitive`, `depthStencil`, `multisample`, `layout`) / `GPUComputePipelineDescriptor` (`compute`, `layout`) / `GPUVertexState` / `GPUFragmentState` / `GPUProgrammableStage` — the full pipeline spec; `GPUVertexBufferLayout`/`GPUVertexAttribute` describe the vertex stream.
- [03]-[BINDING_CONFIG]: `GPUBindGroupDescriptor` / `GPUBindGroupLayoutDescriptor` / `GPUBindGroupLayoutEntry` (`buffer`/`sampler`/`texture`/`storageTexture`/`externalTexture` binding layouts) / `GPUPipelineLayoutDescriptor` — the shader-resource binding declaration; the layout entry is a discriminated binding-kind union.
- [04]-[PASS_CONFIG]: `GPURenderPassDescriptor` / `GPURenderPassColorAttachment` (`loadOp`, `storeOp`, `clearValue`, `view`, `resolveTarget`) / `GPURenderPassDepthStencilAttachment` / `GPUColorTargetState` / `GPUBlendState` / `GPUDepthStencilState` / `GPUPrimitiveState` / `GPUMultisampleState` — the render-pass attachment + blend/depth spec; `loadOp`/`storeOp` are `GPULoadOp`/`GPUStoreOp`.

[PUBLIC_TYPE_SCOPE]: the enum vocabularies + flag namespaces — the parameterized axes every descriptor keys on
- rail: type substrate
- these are the bounded string-literal unions (formats, topologies, ops) and the bitwise flag namespaces (usage, stage, write mask); a descriptor field names one, so the whole configuration space is data, not a switch.

- [01]-[FORMAT_VOCAB]: `GPUTextureFormat` / `GPUVertexFormat` / `GPUIndexFormat` / `GPUTextureDimension` / `GPUTextureViewDimension` / `GPUTextureAspect` — the pixel/vertex/index/texture-shape unions, the format axis of every resource.
- [02]-[PIPELINE_STATE_VOCAB]: `GPUPrimitiveTopology` / `GPUCullMode` / `GPUFrontFace` / `GPUBlendFactor` / `GPUBlendOperation` / `GPUCompareFunction` / `GPUStencilOperation` / `GPULoadOp` / `GPUStoreOp` — the rasterizer/blend/depth-op unions the pipeline descriptor keys on.
- [03]-[SAMPLER_BINDING_VOCAB]: `GPUAddressMode` / `GPUFilterMode` / `GPUMipmapFilterMode` / `GPUBufferBindingType` / `GPUSamplerBindingType` / `GPUTextureSampleType` / `GPUStorageTextureAccess` / `GPUQueryType` — the sampler + binding-layout unions.
- [04]-[FLAG_NAMESPACES]: `GPUBufferUsage` / `GPUTextureUsage` / `GPUShaderStage` / `GPUColorWrite` / `GPUMapMode` (`declare var` flag namespaces) — the bitwise usage/stage/write/map flags OR-combined into a descriptor `usage`/`visibility`, `GPUBufferUsage.VERTEX | GPUBufferUsage.COPY_DST`.

[PUBLIC_TYPE_SCOPE]: canvas presentation + error/device-loss surface
- rail: type substrate
- `GPUCanvasContext` is the swap-chain the renderer presents into; the error hierarchy + `lost` promise are the resilience surface the viewer must handle.

- [01]-[PRESENTATION]: `GPUCanvasContext` (`configure(config)`, `unconfigure()`, `getConfiguration()`, `getCurrentTexture()`, `canvas`) / `GPUCanvasConfiguration` (`device`, `format`, `usage`, `viewFormats`, `colorSpace`, `toneMapping`, `alphaMode`) / `GPUCanvasAlphaMode` / `GPUCanvasToneMappingMode` — `canvas.getContext("webgpu")` configures with the device + format; `getCurrentTexture()` is the per-frame render target.
- [02]-[ERROR_SURFACE]: `GPUError` / `GPUValidationError` / `GPUOutOfMemoryError` / `GPUInternalError` / `GPUPipelineError` / `GPUErrorFilter` — `pushErrorScope('validation')`/`popErrorScope()` captures pipeline/resource errors, the WGSL/binding validation the viewer logs.
- [03]-[LOSS_DIAGNOSTICS]: `GPUDeviceLostInfo` (`reason`, `message`) / `GPUDeviceLostReason` / `GPUUncapturedErrorEvent` / `GPUCompilationInfo` / `GPUCompilationMessage` / `GPUCompilationMessageType` — `device.lost` resolves on loss (GPU reset, tab background) so the viewer re-initializes; `getCompilationInfo` carries WGSL line diagnostics.

## [03]-[IMPLEMENTATION_LAW]

[WEBGPU_ADMISSION]:
- Admission is a tsconfig `types` entry, never an `import`. The package declares `global`-scope interfaces and augments `Navigator`/`WorkerNavigator`/`HTMLCanvasElement`/`OffscreenCanvas`, so listing `"@webgpu/types"` in the `ui/viewer` tsconfig `compilerOptions.types` (or a single `/// <reference types="@webgpu/types" />`) makes `navigator.gpu` and `canvas.getContext("webgpu")` resolve everywhere in the project. There is no value to import; importing it is the named error.
- `scope:viewer` project-local: only the `ui/viewer` Nx project tsconfig references it. The WebGPU globals are compile-time absent from the `ui` core and non-spatial apps, so a non-viewer module referencing `GPUDevice` fails to compile — the intended boundary, matching `three`'s own `scope:viewer` exclusion.
- The types track the living WebGPU IDL — `getCurrentTexture`, `toneMapping`, `featureLevel`, and the feature/limit set shift as the spec matures, so the version moves with the standard and is the SOURCE OF TRUTH for what the browser exposes; never hand-declare a `GPU*` interface to patch a perceived gap — update the package.
- The configuration space is parameterized, not enumerated: a descriptor field names one member of a bounded union (`GPUTextureFormat`, `GPUPrimitiveTopology`) or OR-combines flag-namespace constants (`GPUBufferUsage.VERTEX | GPUBufferUsage.COPY_DST`). Feature negotiation is `requiredFeatures: GPUFeatureName[]` — the whole capability axis is data the device request folds, never a branch per feature.

[STACKS_WITH]:
- `three` / `three/webgpu` (`viewer/scene/glb`, `viewer/scene/appearance`, `libs/typescript/ui/.api/three.md`): the sole consumer package, on TWO reference sites. (1) The renderer — `three`'s `WebGPURenderer`/`WebGPUBackend` request an adapter through `navigator.gpu`, its TSL `ComputeNode`/`StorageBufferNode` compile to WGSL running on a `GPUDevice`, and `renderer.hasFeature(name)` (post-`init()`; `hasFeatureAsync` is deprecated) resolves through `WebGPUBackend.hasFeature` to `GPUDevice.features.has(name)` over a `GPUFeatureName`. (2) The codec — `KTX2Loader.detectSupport(renderer)` calls that same `renderer.hasFeature('texture-compression-astc'|'-etc2'|'-bc')`, i.e. reads `GPUDevice.features` (a `GPUSupportedFeatures`) to pick the GLB texture transcode target on the WebGPU path (the WebGL path reads `WEBGL_compressed_texture_*` extensions instead, off these globals). Neither site type-checks without these globals in the viewer tsconfig. `@webgpu/types` is the type floor `three`'s WebGPU path stands on: `three` owns the runtime, this owns the types — the two are the WebGPU-path type + runtime halves.
- `browser` decode-worker port (`ui/viewer` ↔ `browser`, permitted edge): a raw-WebGPU compute path (meshopt decode, GPU picking) runs in a worker whose tsconfig also lists `@webgpu/types` — the augmentation covers `WorkerNavigator.gpu`/`OffscreenCanvas.getContext("webgpu")`, so the worker's `GPUDevice` code type-checks off the same package. The `GlbViewport` residency port carries the decoded buffers the compute pass consumes.
- device-loss + Effect resilience (`viewer`): `GPUDevice.lost` is a `Promise<GPUDeviceLostInfo>` — the viewer wraps it in the folder's Effect rail so a device loss (GPU reset, backgrounded tab) folds to a typed re-initialization, not an unhandled rejection; `pushErrorScope`/`popErrorScope` capture validation errors as values.
- no runtime peer: unlike the other `viewer` libs this admits NO admitted-lib runtime seam — it is types under `three`, so its only "stack" is the compile-time floor beneath the renderer and the worker.

[LOCAL_ADMISSION]:
- Add `@webgpu/types` to the `ui/viewer` tsconfig `compilerOptions.types` (or one triple-slash reference); never `import` it, never add it outside the viewer project scope.
- Consume the globals `three` exposes — the viewer rarely touches raw `GPUDevice` directly; when it does (compute path in a worker), the device graph, descriptors, enum vocabularies, and flag namespaces are all here, keyed by the parameterized unions.
- Handle `device.lost` and error scopes through the folder's Effect rail; treat `GPUValidationError`/`GPUDeviceLostInfo` as typed values.
- Never hand-declare a `GPU*` interface — the package is the IDL source of truth; a missing member is a version bump.

[RAIL_LAW]:
- Package: `@webgpu/types`
- Owns: the ambient WebGPU IDL — the `navigator.gpu`/`getContext("webgpu")` global augmentation, the `GPU`→`GPUAdapter`→`GPUDevice` object graph, every resource/pipeline/encoder interface, the descriptor family, the enum vocabularies + flag namespaces, and the error/device-loss surface
- Accept: a tsconfig `types` entry (or triple-slash reference) in the `ui/viewer` project only, consumption through `three`'s WebGPU backend, direct use on a raw-compute worker path, `GPUFeatureName`/`requiredLimits` capability negotiation, and `device.lost`/error-scope handling through the Effect rail
- Reject: an `import` of the package (it is ambient), admission outside `scope:viewer`, a hand-declared `GPU*` interface patching a spec gap (bump the version), and a per-feature branch where the `GPUFeatureName[]` negotiation is data
