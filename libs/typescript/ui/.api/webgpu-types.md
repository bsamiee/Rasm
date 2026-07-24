# [TS_UI_API_WEBGPU_TYPES]

`@webgpu/types` declares the ambient WebGPU IDL as TypeScript and augments `navigator.gpu` and `canvas.getContext("webgpu")` globally at compile time, shipping zero runtime bytes. It is the type floor under `three`'s `three/webgpu` renderer, admitted `scope:viewer` by a tsconfig `types` entry, never an `import` — `three` owns the WebGPU runtime, this owns the types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@webgpu/types`
- package: `@webgpu/types` (BSD-3-Clause)
- module: type-only ambient declarations (`types: dist/index.d.ts`), no `main`/`module` and no runtime value; declares `global`-scope interfaces and augments `Navigator`/`WorkerNavigator`/`HTMLCanvasElement`/`OffscreenCanvas`
- runtime: none — the browser's own WebGPU implementation backs the types; the package ships zero bytes
- scope: `scope:viewer`, referenced only by the `ui/viewer` Nx project tsconfig, compile-time absent from non-spatial apps
- rail: type substrate under `three`'s WebGPU backend and any raw-WebGPU compute path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: adapter and device negotiation — the boot capability probe

`navigator.gpu` roots the graph; `requestAdapter` → `GPUAdapter` → `requestDevice` runs once at boot, and the negotiated feature/limit sets are the capability gate backend selection reads.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :------------------------- | :------------ | :------------------------ |
|  [01]   | `NavigatorGPU` / `GPU`     | interface     | graph root + swap format  |
|  [02]   | `GPUAdapter`               | interface     | capability probe          |
|  [03]   | `GPURequestAdapterOptions` | interface     | adapter selection knobs   |
|  [04]   | `GPUDeviceDescriptor`      | interface     | device request            |
|  [05]   | `GPUSupportedFeatures`     | value-object  | supported feature set     |
|  [06]   | `GPUSupportedLimits`       | interface     | device numeric limits     |
|  [07]   | `GPUAdapterInfo`           | interface     | physical adapter identity |
|  [08]   | `GPUFeatureName`           | union         | capability axis vocab     |
|  [09]   | `GPUPowerPreference`       | union         | power hint values         |

- `GPU`: `requestAdapter` `getPreferredCanvasFormat` `wgslLanguageFeatures`
- `GPUAdapter`: `features` `limits` `info` `requestDevice`
- `GPURequestAdapterOptions`: `powerPreference` `forceFallbackAdapter` `featureLevel`
- `GPUDeviceDescriptor`: `requiredFeatures` `requiredLimits` `defaultQueue`
- `GPUFeatureName` carries the load-bearing `texture-compression-bc`/`-etc2`/`-astc` members; `GPUDevice.features` (a `GPUSupportedFeatures`) exposes the negotiated set the transcode-target seam reads.

[PUBLIC_TYPE_SCOPE]: the device object graph — resources, pipelines, encoders

`GPUDevice` is the factory for every resource and pipeline, `GPUQueue` submits command buffers, and the encoders record passes — the graph `three`'s backend drives and a raw compute path composes directly.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------- | :------------ | :-------------------------- |
|  [01]   | `GPUDevice`                                                 | interface     | resource + pipeline factory |
|  [02]   | `GPUQueue`                                                  | interface     | command submission + writes |
|  [03]   | `GPUCommandBuffer`                                          | interface     | recorded command batch      |
|  [04]   | `GPUBuffer`                                                 | interface     | GPU buffer + CPU readback   |
|  [05]   | `GPUTexture` / `GPUTextureView`                             | interface     | GPU image + view            |
|  [06]   | `GPUSampler` / `GPUExternalTexture` / `GPUQuerySet`         | interface     | auxiliary GPU resources     |
|  [07]   | `GPURenderPipeline` / `GPUComputePipeline`                  | interface     | compiled programs           |
|  [08]   | `GPUBindGroup` / `GPUBindGroupLayout` / `GPUPipelineLayout` | interface     | resource binding            |
|  [09]   | `GPUShaderModule`                                           | interface     | compiled WGSL module        |
|  [10]   | `GPUCommandEncoder`                                         | interface     | pass + copy recording       |
|  [11]   | `GPURenderPassEncoder` / `GPUComputePassEncoder`            | interface     | pass command recording      |
|  [12]   | `GPURenderBundleEncoder` / `GPURenderBundle`                | interface     | reusable command bundle     |

- `GPUDevice`: `createBuffer` `createTexture` `createSampler` `createBindGroup` `createBindGroupLayout` `createPipelineLayout` `createShaderModule` `createRenderPipeline` `createComputePipeline` `createRenderPipelineAsync` `createComputePipelineAsync` `createCommandEncoder` `createRenderBundleEncoder` `createQuerySet` `importExternalTexture` `queue` `features` `limits` `lost` `pushErrorScope` `popErrorScope` `destroy`
- `GPUQueue`: `submit` `writeBuffer` `writeTexture` `copyExternalImageToTexture` `onSubmittedWorkDone`
- `GPUBuffer`: `mapAsync` `getMappedRange` `unmap` `mapState` `size` `usage` `destroy`
- `GPUTexture`: `createView` `format` `usage` `dimension` `mipLevelCount` `sampleCount` `destroy`
- `GPUCommandEncoder`: `beginRenderPass` `beginComputePass` `copyBufferToBuffer` `copyBufferToTexture` `copyTextureToBuffer` `copyTextureToTexture` `clearBuffer` `resolveQuerySet` `finish`
- `GPURenderPassEncoder`: `setPipeline` `setBindGroup` `setVertexBuffer` `setIndexBuffer` `draw` `drawIndexed` `drawIndirect` `setViewport` `setScissorRect` `executeBundles` `end`
- `GPUComputePassEncoder`: `setPipeline` `setBindGroup` `dispatchWorkgroups` `dispatchWorkgroupsIndirect` `end`
- `GPUComputePipeline` / `GPURenderPipeline`: `getBindGroupLayout`; `GPUShaderModule`: `getCompilationInfo`

[PUBLIC_TYPE_SCOPE]: descriptors — the create-time config per resource and pipeline

One descriptor interface per `create*` call, keyed by the enum vocabularies and flag namespaces below.

| [INDEX] | [SYMBOL]                                                                              | [TYPE_FAMILY] | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------ | :------------ | :---------------------- |
|  [01]   | `GPUBufferDescriptor` / `GPUTextureDescriptor`                                        | interface     | resource create config  |
|  [02]   | `GPUSamplerDescriptor` / `GPUTextureViewDescriptor`                                   | interface     | sampler + view config   |
|  [03]   | `GPURenderPipelineDescriptor` / `GPUComputePipelineDescriptor`                        | interface     | pipeline spec           |
|  [04]   | `GPUVertexState` / `GPUFragmentState` / `GPUProgrammableStage`                        | interface     | shader stage config     |
|  [05]   | `GPUVertexBufferLayout` / `GPUVertexAttribute`                                        | interface     | vertex stream layout    |
|  [06]   | `GPUBindGroupDescriptor` / `GPUBindGroupLayoutDescriptor` / `GPUBindGroupLayoutEntry` | interface     | binding declaration     |
|  [07]   | `GPUPipelineLayoutDescriptor`                                                         | interface     | pipeline layout         |
|  [08]   | `GPURenderPassDescriptor`                                                             | interface     | render-pass attachments |
|  [09]   | `GPURenderPassColorAttachment` / `GPURenderPassDepthStencilAttachment`                | interface     | pass attachment         |
|  [10]   | `GPUColorTargetState` / `GPUBlendState` / `GPUDepthStencilState`                      | interface     | blend + depth state     |
|  [11]   | `GPUPrimitiveState` / `GPUMultisampleState`                                           | interface     | raster + MSAA state     |

- `GPUTextureDescriptor`: `size` `format` `usage` `dimension` `mipLevelCount` `sampleCount`
- `GPUBufferDescriptor`: `size` `usage` `mappedAtCreation`
- `GPURenderPassColorAttachment`: `view` `resolveTarget` `loadOp` `storeOp` `clearValue`

[PUBLIC_TYPE_SCOPE]: enum vocabularies and flag namespaces — the parameterized axes every descriptor keys on

Bounded string-literal unions carry the format, topology, and op axes; `declare var` flag namespaces carry the bitwise usage, stage, write, and map flags OR-combined into a descriptor.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `GPUTextureFormat` / `GPUVertexFormat` / `GPUIndexFormat`              | union         | pixel/vertex/index formats   |
|  [02]   | `GPUTextureDimension` / `GPUTextureViewDimension` / `GPUTextureAspect` | union         | texture shape axes           |
|  [03]   | `GPUPrimitiveTopology` / `GPUCullMode` / `GPUFrontFace`                | union         | rasterizer state             |
|  [04]   | `GPUBlendFactor` / `GPUBlendOperation`                                 | union         | blend ops                    |
|  [05]   | `GPUCompareFunction` / `GPUStencilOperation`                           | union         | depth/stencil ops            |
|  [06]   | `GPULoadOp` / `GPUStoreOp`                                             | union         | attachment load/store        |
|  [07]   | `GPUAddressMode` / `GPUFilterMode` / `GPUMipmapFilterMode`             | union         | sampler modes                |
|  [08]   | `GPUBufferBindingType` / `GPUSamplerBindingType`                       | union         | buffer/sampler binding kinds |
|  [09]   | `GPUTextureSampleType` / `GPUStorageTextureAccess`                     | union         | texture binding kinds        |
|  [10]   | `GPUQueryType`                                                         | union         | query set kind               |
|  [11]   | `GPUBufferUsage` / `GPUTextureUsage`                                   | flag-ns       | resource usage flags         |
|  [12]   | `GPUShaderStage` / `GPUColorWrite` / `GPUMapMode`                      | flag-ns       | stage / write / map flags    |

[PUBLIC_TYPE_SCOPE]: canvas presentation and the error/device-loss surface

`GPUCanvasContext` is the swap-chain the renderer presents into; the error hierarchy and the `lost` promise are the resilience surface the viewer folds through the Effect rail.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------------------- | :------------ | :---------------------------- |
|  [01]   | `GPUCanvasContext`                                                     | interface     | swap-chain present target     |
|  [02]   | `GPUCanvasConfiguration`                                               | interface     | context configure config      |
|  [03]   | `GPUCanvasAlphaMode` / `GPUCanvasToneMappingMode`                      | union         | canvas alpha + tone map       |
|  [04]   | `GPUError` / `GPUValidationError` / `GPUPipelineError`                 | interface     | error hierarchy               |
|  [05]   | `GPUOutOfMemoryError` / `GPUInternalError`                             | interface     | oom + internal errors         |
|  [06]   | `GPUDeviceLostInfo` / `GPUUncapturedErrorEvent`                        | interface     | loss + uncaught-error surface |
|  [07]   | `GPUCompilationInfo` / `GPUCompilationMessage`                         | interface     | WGSL diagnostics              |
|  [08]   | `GPUErrorFilter` / `GPUDeviceLostReason` / `GPUCompilationMessageType` | union         | error/loss/message vocab      |

- `GPUCanvasContext`: `configure` `unconfigure` `getConfiguration` `getCurrentTexture` `canvas`
- `GPUCanvasConfiguration`: `device` `format` `usage` `viewFormats` `colorSpace` `toneMapping` `alphaMode`
- `GPUDeviceLostInfo`: `reason` `message`

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Admission binds a tsconfig `types` entry, never an `import`: the package declares `global`-scope interfaces and augments `Navigator`/`WorkerNavigator`/`HTMLCanvasElement`/`OffscreenCanvas`, so one `"@webgpu/types"` entry in the `ui/viewer` tsconfig `compilerOptions.types` (or a `/// <reference types="@webgpu/types" />`) resolves `navigator.gpu` and `canvas.getContext("webgpu")` across the project, and no value exists to import.
- `scope:viewer` fixes the boundary: only the `ui/viewer` project tsconfig references the package, so a non-viewer module naming `GPUDevice` fails to compile, matching `three`'s own `scope:viewer` exclusion.
- `@webgpu/types` owns the WebGPU IDL as source of truth: a missing member resolves through a package bump, never a hand-declared `GPU*` interface patching a perceived gap.
- Descriptor fields carry the configuration space as data: each names one member of a bounded union (`GPUTextureFormat`, `GPUPrimitiveTopology`) or OR-combines flag-namespace constants (`GPUBufferUsage.VERTEX | GPUBufferUsage.COPY_DST`), and feature negotiation folds one `requiredFeatures: GPUFeatureName[]` array, never a per-feature branch.

[STACKING]:
- `three` / `three/webgpu` (`.api/three.md`; `viewer/scene/glb`, `viewer/scene/appearance`): the sole consumer. `GPUDevice.features` (a `GPUSupportedFeatures`) is the set `three`'s `renderer.hasFeature` and `KTX2Loader.detectSupport` read over `GPUFeatureName` — three.md owns that renderer/loader runtime flow, this owns the `GPUDevice`/`GPUAdapter`/`GPUTextureFormat` types it stands on. `three`'s `WebGPURenderer` requests an adapter through `navigator.gpu` and its TSL compute compiles to WGSL on a `GPUDevice`, so the two are the WebGPU-path type + runtime halves.
- `browser` decode-worker port (`ui/viewer` ↔ `browser`): a raw-WebGPU compute path (meshopt decode, GPU picking) runs in a worker whose tsconfig also lists `@webgpu/types`, and the augmentation covers `WorkerNavigator.gpu`/`OffscreenCanvas.getContext("webgpu")` so the worker's `GPUDevice` code type-checks off the same package; the `GlbViewport` residency port carries the decoded buffers the compute pass consumes.
- `effect` (`libs/typescript/.api/effect.md`): `GPUDevice.lost` (a `Promise<GPUDeviceLostInfo>`) folds through the folder's Effect rail so a device loss (GPU reset, backgrounded tab) becomes a typed re-initialization, and `pushErrorScope`/`popErrorScope` capture `GPUValidationError` as values rather than an unhandled rejection.

[LOCAL_ADMISSION]:
- Add `@webgpu/types` to the `ui/viewer` tsconfig `compilerOptions.types` (or one triple-slash reference); never `import` it, never admit it outside `scope:viewer`.
- Consume the globals `three` exposes and reach raw `GPUDevice` only on the worker compute path, where the device graph, descriptors, enum vocabularies, and flag namespaces are all keyed by the parameterized unions.
- Handle `device.lost` and error scopes through the folder's Effect rail; treat `GPUValidationError`/`GPUDeviceLostInfo` as typed values.
- Bump the package for a missing member; never hand-declare a `GPU*` interface.

[RAIL_LAW]:
- Package: `@webgpu/types`
- Owns: the ambient WebGPU IDL — the `navigator.gpu`/`getContext("webgpu")` global augmentation, the `GPU`→`GPUAdapter`→`GPUDevice` object graph, every resource/pipeline/encoder interface, the descriptor family, the enum vocabularies and flag namespaces, and the error/device-loss surface
- Accept: a tsconfig `types` entry (or triple-slash reference) in `ui/viewer` only, consumption through `three`'s WebGPU backend, raw use on the worker compute path, `GPUFeatureName`/`requiredLimits` negotiation, and `device.lost`/error-scope handling through the Effect rail
- Reject: an `import` of the package, admission outside `scope:viewer`, a hand-declared `GPU*` interface patching a spec gap, and a per-feature branch where the `GPUFeatureName[]` negotiation is data
