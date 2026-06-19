# [API_CATALOGUE] @webgpu/types

`@webgpu/types` supplies ambient TypeScript declarations for the WebGPU API — `GPU`, `GPUAdapter`, `GPUDevice`, `GPUBuffer`, `GPUTexture`, `GPUSampler`, `GPUCommandEncoder`, `GPURenderPassEncoder`, `GPUComputePassEncoder`, `GPUQueue`, `GPUCanvasContext`, and the full descriptor/enum vocabulary — consumed by the `ui` stack when authoring WebGPU shaders, pipelines, and GPU compute workloads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@webgpu/types`
- package: `@webgpu/types`
- module: `@webgpu/types` (ambient `.d.ts` — no runtime; types: `dist/index.d.ts`)
- asset: global ambient declarations merged into `navigator.gpu`, `GPU`, all `GPU*` interfaces, descriptor types, enum string unions
- rail: type

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: adapter and device acquisition
- rail: type

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                                                 |
| :-----: | :--------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `GPU`                  | entry point      | `requestAdapter()`, `getPreferredCanvasFormat()`                       |
|  [02]   | `GPUAdapter`           | adapter handle   | `requestDevice()`, `features`, `limits`, `info`                        |
|  [03]   | `GPUAdapterInfo`       | adapter metadata | `vendor`, `architecture`, `device`, `description`, `isFallbackAdapter` |
|  [04]   | `GPUDevice`            | device handle    | resource creation, `queue`, `destroy()`                                |
|  [05]   | `NavigatorGPU`         | navigator mixin  | `readonly gpu: GPU` on `navigator`                                     |
|  [06]   | `GPUSupportedFeatures` | feature set      | `Set<GPUFeatureName>` capabilities                                     |
|  [07]   | `GPUSupportedLimits`   | limits object    | `maxBindGroups`, `maxTextureDimension2D`, etc.                         |

[PUBLIC_TYPE_SCOPE]: resource objects
- rail: type

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `GPUBuffer`          | buffer object     | `mapAsync`, `getMappedRange`, `unmap`, `destroy` |
|  [02]   | `GPUTexture`         | texture object    | `createView`, `destroy`                          |
|  [03]   | `GPUTextureView`     | texture view      | aspect-scoped view into a `GPUTexture`           |
|  [04]   | `GPUSampler`         | sampler object    | filter mode, address mode, LOD clamp             |
|  [05]   | `GPUExternalTexture` | external texture  | wraps `HTMLVideoElement` / `VideoFrame`          |
|  [06]   | `GPUBindGroup`       | bind group        | resource binding set for a pipeline              |
|  [07]   | `GPUBindGroupLayout` | bind group layout | binding slot declarations                        |
|  [08]   | `GPUPipelineLayout`  | pipeline layout   | ordered `GPUBindGroupLayout` list                |

[PUBLIC_TYPE_SCOPE]: pipeline and shader objects
- rail: type

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------- | :--------------- | :------------------------------------- |
|  [01]   | `GPUShaderModule`    | compiled WGSL    | `getCompilationInfo()` for diagnostics |
|  [02]   | `GPURenderPipeline`  | render pipeline  | vertex + fragment stages               |
|  [03]   | `GPUComputePipeline` | compute pipeline | compute stage                          |
|  [04]   | `GPURenderBundle`    | recorded bundle  | pre-recorded render commands           |

[PUBLIC_TYPE_SCOPE]: command encoding objects
- rail: type

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                                                         |
| :-----: | :----------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `GPUCommandEncoder`      | command encoder   | `beginRenderPass`, `beginComputePass`, copy ops                |
|  [02]   | `GPUCommandBuffer`       | recorded commands | submitted to `GPUQueue.submit()`                               |
|  [03]   | `GPURenderPassEncoder`   | render pass       | draw, viewport, scissor, blend, bundle exec                    |
|  [04]   | `GPUComputePassEncoder`  | compute pass      | `dispatchWorkgroups`, pipeline bind, end                       |
|  [05]   | `GPURenderBundleEncoder` | bundle encoder    | pre-record reusable render commands                            |
|  [06]   | `GPUQueue`               | work queue        | `submit`, `writeBuffer`, `writeTexture`, `onSubmittedWorkDone` |
|  [07]   | `GPUQuerySet`            | query set         | occlusion / timestamp queries                                  |

[PUBLIC_TYPE_SCOPE]: canvas context
- rail: type

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [RAIL]                                                 |
| :-----: | :-------------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `GPUCanvasContext`          | canvas context      | `configure`, `getCurrentTexture`, `unconfigure`        |
|  [02]   | `GPUCanvasConfiguration`    | canvas config desc  | `device`, `format`, `usage`, `colorSpace`, `alphaMode` |
|  [03]   | `GPUCanvasConfigurationOut` | config readback     | result of `getConfiguration()`                         |
|  [04]   | `GPUCanvasToneMapping`      | tone mapping config | `mode: GPUCanvasToneMappingMode`                       |

[PUBLIC_TYPE_SCOPE]: descriptor interfaces
- rail: type

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]         | [RAIL]                                                      |
| :-----: | :----------------------------- | :-------------------- | :---------------------------------------------------------- |
|  [01]   | `GPUBufferDescriptor`          | buffer desc           | `size`, `usage`, `mappedAtCreation`                         |
|  [02]   | `GPUTextureDescriptor`         | texture desc          | `size`, `format`, `usage`, `dimension`, `mipLevelCount`     |
|  [03]   | `GPUSamplerDescriptor`         | sampler desc          | `addressMode*`, `magFilter`, `minFilter`, `mipmapFilter`    |
|  [04]   | `GPURenderPipelineDescriptor`  | render pipeline desc  | `layout`, `vertex`, `fragment`, `primitive`, `depthStencil` |
|  [05]   | `GPUComputePipelineDescriptor` | compute pipeline desc | `layout`, `compute`                                         |
|  [06]   | `GPUShaderModuleDescriptor`    | shader module desc    | `code` (WGSL source), `hints`, `sourceMap`                  |
|  [07]   | `GPURenderPassDescriptor`      | render pass desc      | `colorAttachments`, `depthStencilAttachment`                |
|  [08]   | `GPUComputePassDescriptor`     | compute pass desc     | `timestampWrites?`                                          |
|  [09]   | `GPUBindGroupDescriptor`       | bind group desc       | `layout`, `entries`                                         |
|  [10]   | `GPUBindGroupLayoutDescriptor` | layout desc           | `entries: Iterable<GPUBindGroupLayoutEntry>`                |
|  [11]   | `GPUDeviceDescriptor`          | device desc           | `requiredFeatures`, `requiredLimits`                        |

[PUBLIC_TYPE_SCOPE]: primitive type aliases and enums
- rail: type

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                                           |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `GPUTextureFormat`     | format union       | full BCn / ETC2 / ASTC / depth / color formats   |
|  [02]   | `GPUVertexFormat`      | vertex format      | `'float32x3'`, `'uint16x4'`, etc.                |
|  [03]   | `GPUAddressMode`       | address mode       | `'clamp-to-edge' \| 'repeat' \| 'mirror-repeat'` |
|  [04]   | `GPUFilterMode`        | filter mode        | `'nearest' \| 'linear'`                          |
|  [05]   | `GPUBlendFactor`       | blend factor       | src, dst, constant, saturated variants           |
|  [06]   | `GPUBlendOperation`    | blend op           | `'add' \| 'subtract' \| 'min' \| 'max'`          |
|  [07]   | `GPUPrimitiveTopology` | draw topology      | point-list, line-strip, triangle-list, etc.      |
|  [08]   | `GPUCullMode`          | culling            | `'none' \| 'front' \| 'back'`                    |
|  [09]   | `GPUIndexFormat`       | index format       | `'uint16' \| 'uint32'`                           |
|  [10]   | `GPULoadOp`            | load operation     | `'load' \| 'clear'`                              |
|  [11]   | `GPUStoreOp`           | store operation    | `'store' \| 'discard'`                           |
|  [12]   | `GPUFeatureName`       | feature names      | `'depth-clip-control'`, `'shader-f16'`, etc.     |
|  [13]   | `GPUBufferMapState`    | map state          | `'unmapped' \| 'pending' \| 'mapped'`            |
|  [14]   | `GPUErrorFilter`       | error scope filter | `'validation' \| 'out-of-memory' \| 'internal'`  |
|  [15]   | `GPUCompareFunction`   | depth/stencil cmp  | `'never'`, `'less'`, `'equal'`, etc.             |

[PUBLIC_TYPE_SCOPE]: numeric type aliases
- rail: type

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                           |
| :-----: | :--------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `GPUSize64`            | `number`      | byte sizes and offsets                           |
|  [02]   | `GPUSize32`            | `number`      | 32-bit count values                              |
|  [03]   | `GPUIndex32`           | `number`      | bind group slot indices                          |
|  [04]   | `GPUBufferUsageFlags`  | `number`      | `GPUBufferUsage.*` bitmask                       |
|  [05]   | `GPUTextureUsageFlags` | `number`      | `GPUTextureUsage.*` bitmask                      |
|  [06]   | `GPUShaderStageFlags`  | `number`      | `GPUShaderStage.*` bitmask                       |
|  [07]   | `GPUBindingResource`   | union         | sampler \| texture \| view \| buffer \| external |

[PUBLIC_TYPE_SCOPE]: error and event types
- rail: type

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------------ | :--------------- | :------------------------------------- |
|  [01]   | `GPUUncapturedErrorEvent` | error event      | `.error: GPUError`                     |
|  [02]   | `GPUValidationError`      | validation error | device validation failure              |
|  [03]   | `GPUOutOfMemoryError`     | OOM error        | GPU allocation failure                 |
|  [04]   | `GPUInternalError`        | internal error   | driver-level failure                   |
|  [05]   | `GPUPipelineError`        | pipeline error   | `reason: GPUPipelineErrorReason`       |
|  [06]   | `GPUCompilationMessage`   | shader message   | `type`, `message`, `lineNum`, `offset` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GPU — adapter acquisition
- rail: type

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :----------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `navigator.gpu.requestAdapter(options?)`   | adapter request | `Promise<GPUAdapter \| null>`       |
|  [02]   | `navigator.gpu.getPreferredCanvasFormat()` | format query    | optimal `GPUTextureFormat` for swap |
|  [03]   | `navigator.gpu.wgslLanguageFeatures`       | feature set     | supported WGSL extensions           |

[ENTRYPOINT_SCOPE]: GPUAdapter — device request
- rail: type

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :----------------------------------- | :------------- | :--------------------------- |
|  [01]   | `adapter.requestDevice(descriptor?)` | device request | `Promise<GPUDevice>`         |
|  [02]   | `adapter.features`                   | capability     | `GPUSupportedFeatures` set   |
|  [03]   | `adapter.limits`                     | capability     | `GPUSupportedLimits` values  |
|  [04]   | `adapter.info`                       | metadata       | vendor, architecture, device |

[ENTRYPOINT_SCOPE]: GPUDevice — resource creation
- rail: type

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]   | [RAIL]                           |
| :-----: | :-------------------------------------------------- | :--------------- | :------------------------------- |
|  [01]   | `device.createBuffer(descriptor)`                   | buffer factory   | `GPUBuffer`                      |
|  [02]   | `device.createTexture(descriptor)`                  | texture factory  | `GPUTexture`                     |
|  [03]   | `device.createSampler(descriptor?)`                 | sampler factory  | `GPUSampler`                     |
|  [04]   | `device.createShaderModule(descriptor)`             | shader compile   | `GPUShaderModule`                |
|  [05]   | `device.createRenderPipeline(descriptor)`           | pipeline factory | `GPURenderPipeline` (sync)       |
|  [06]   | `device.createComputePipeline(descriptor)`          | pipeline factory | `GPUComputePipeline` (sync)      |
|  [07]   | `device.createRenderPipelineAsync(desc)`            | async pipeline   | `Promise<GPURenderPipeline>`     |
|  [08]   | `device.createBindGroupLayout(descriptor)`          | layout factory   | `GPUBindGroupLayout`             |
|  [09]   | `device.createPipelineLayout(descriptor)`           | layout factory   | `GPUPipelineLayout`              |
|  [10]   | `device.createBindGroup(descriptor)`                | bind group       | `GPUBindGroup`                   |
|  [11]   | `device.createCommandEncoder(descriptor?)`          | encoder factory  | `GPUCommandEncoder`              |
|  [12]   | `device.createRenderBundleEncoder(desc)`            | bundle encoder   | `GPURenderBundleEncoder`         |
|  [13]   | `device.createQuerySet(descriptor)`                 | query set        | `GPUQuerySet` (occlusion/ts)     |
|  [14]   | `device.importExternalTexture(descriptor)`          | texture import   | `GPUExternalTexture`             |
|  [15]   | `device.pushErrorScope(filter)` / `popErrorScope()` | error scope      | structured GPU error capture     |
|  [16]   | `device.destroy()`                                  | cleanup          | release device + outstanding ops |

[ENTRYPOINT_SCOPE]: GPUCommandEncoder — encoding
- rail: type

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `encoder.beginRenderPass(descriptor)`         | pass factory   | `GPURenderPassEncoder`        |
|  [02]   | `encoder.beginComputePass(descriptor?)`       | pass factory   | `GPUComputePassEncoder`       |
|  [03]   | `encoder.copyBufferToBuffer(src, dst, size?)` | copy           | buffer → buffer copy          |
|  [04]   | `encoder.finish()`                            | finalize       | `GPUCommandBuffer` for submit |

[ENTRYPOINT_SCOPE]: GPURenderPassEncoder — draw commands
- rail: type

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `pass.setPipeline(pipeline)`                   | state set      | bind render pipeline          |
|  [02]   | `pass.setBindGroup(index, bindGroup)`          | state set      | bind resource group at slot   |
|  [03]   | `pass.setVertexBuffer(slot, buffer, offset?)`  | state set      | bind vertex attribute buffer  |
|  [04]   | `pass.setIndexBuffer(buffer, format, offset?)` | state set      | bind index buffer             |
|  [05]   | `pass.draw(vertexCount, instanceCount?)`       | draw call      | non-indexed draw              |
|  [06]   | `pass.drawIndexed(indexCount, instanceCount?)` | draw call      | indexed draw                  |
|  [07]   | `pass.setViewport(x, y, w, h, minD, maxD)`     | raster state   | NDC → viewport mapping        |
|  [08]   | `pass.setScissorRect(x, y, w, h)`              | raster state   | scissor rectangle             |
|  [09]   | `pass.executeBundles(bundles)`                 | bundle exec    | replay `GPURenderBundle` list |
|  [10]   | `pass.end()`                                   | finalize       | close render pass             |

[ENTRYPOINT_SCOPE]: GPUQueue — work submission
- rail: type

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `queue.submit(commandBuffers)`                          | submit         | schedule `GPUCommandBuffer` array  |
|  [02]   | `queue.writeBuffer(buffer, offset, data, doff?, size?)` | data upload    | CPU → GPU buffer write             |
|  [03]   | `queue.writeTexture(dst, data, layout, size)`           | data upload    | CPU → GPU texture write            |
|  [04]   | `queue.onSubmittedWorkDone()`                           | sync fence     | `Promise<undefined>` on completion |

[ENTRYPOINT_SCOPE]: GPUCanvasContext — swap chain
- rail: type

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------------------- | :------------- | :---------------------------------- |
|  [01]   | `ctx.configure(configuration)` | setup          | attach device and format to canvas  |
|  [02]   | `ctx.getCurrentTexture()`      | frame acquire  | next presentable `GPUTexture`       |
|  [03]   | `ctx.unconfigure()`            | teardown       | remove config, destroy textures     |
|  [04]   | `ctx.getConfiguration()`       | config query   | `GPUCanvasConfigurationOut \| null` |

## [04]-[IMPLEMENTATION_LAW]

[TYPE_TOPOLOGY]:
- `@webgpu/types` is ambient-only: no runtime exports; install as a `devDependency` and reference via `tsconfig.json` `types` array or `/// <reference types="@webgpu/types" />` directive
- All `GPU*` interfaces are global declarations; no import is needed at use sites once the types are included
- `GPUExtent3DStrict` and `GPUOrigin2DStrict` are stricter alternatives to `GPUExtent3D` / `GPUOrigin2D` that catch the common `depth` vs `depthOrArrayLayers` mistake at the type level
- Deprecated aliases: `GPUImageDataLayout` → `GPUTexelCopyBufferLayout`; `GPUImageCopyBuffer` → `GPUTexelCopyBufferInfo`; `GPUImageCopyTexture` → `GPUTexelCopyTextureInfo`; `GPUImageCopyTextureTagged` → `GPUCopyExternalImageDestInfo`
- `GPUFeatureName` includes `'core-features-and-limits'`, `'shader-f16'`, `'dual-source-blending'`, `'subgroups'`, and `'texture-component-swizzle'`
- `navigator.gpu` is exposed via `NavigatorGPU` mixin on `Navigator` and `WorkerNavigator`
- `HTMLCanvasElement.getContext('webgpu')` and `OffscreenCanvas.getContext('webgpu')` return `GPUCanvasContext | null`

[LOCAL_ADMISSION]:
- Add `"@webgpu/types"` to `tsconfig.json` `"types"` array; do not import individual GPU symbols.
- Use `GPUExtent3DStrict` and `GPUOrigin2DStrict` instead of the bare union types to catch `depth` field misuse at compile time.
- Request device features explicitly in `GPUDeviceDescriptor.requiredFeatures`; unfeatureed `GPUDevice` instances reject operations that depend on missing features with validation errors.
- Error scopes (`pushErrorScope` / `popErrorScope`) wrap async GPU work to capture `GPUValidationError`, `GPUOutOfMemoryError`, and `GPUInternalError` without unhandled promise rejections.

[RAIL_LAW]:
- Package: `@webgpu/types`
- Owns: ambient TypeScript declarations for the W3C WebGPU API
- Accept: global `GPU*` type usage without import, `GPUExtent3DStrict` for dimension descriptors, `GPUTexelCopy*` names over deprecated `GPUImageCopy*` aliases
- Reject: runtime import of `@webgpu/types`, use of deprecated `GPUImageCopy*` and `GPUImageDataLayout` type aliases
