# [API_CATALOGUE] @webgpu/types

`@webgpu/types` supplies ambient TypeScript declarations for the WebGPU API — `GPU`, `GPUAdapter`, `GPUDevice`, `GPUBuffer`, `GPUTexture`, `GPUSampler`, `GPUCommandEncoder`, `GPURenderPassEncoder`, `GPUComputePassEncoder`, `GPUQueue`, `GPUCanvasContext`, and the full descriptor/enum vocabulary — consumed by the `ui` stack when authoring WebGPU shaders, pipelines, and GPU compute workloads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@webgpu/types`
- package: `@webgpu/types`
- module: `@webgpu/types` (ambient `.d.ts` — no runtime; types: `dist/index.d.ts`)
- asset: global ambient declarations merged into `navigator.gpu`, `GPU`, all `GPU*` interfaces, descriptor types, enum string unions
- rail: type

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: adapter and device acquisition
- rail: type

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                                                 |
| :-----: | :--------------------- | :--------------- | :--------------------------------------------------------------------- |
|   [1]   | `GPU`                  | entry point      | `requestAdapter()`, `getPreferredCanvasFormat()`                       |
|   [2]   | `GPUAdapter`           | adapter handle   | `requestDevice()`, `features`, `limits`, `info`                        |
|   [3]   | `GPUAdapterInfo`       | adapter metadata | `vendor`, `architecture`, `device`, `description`, `isFallbackAdapter` |
|   [4]   | `GPUDevice`            | device handle    | resource creation, `queue`, `destroy()`                                |
|   [5]   | `NavigatorGPU`         | navigator mixin  | `readonly gpu: GPU` on `navigator`                                     |
|   [6]   | `GPUSupportedFeatures` | feature set      | `Set<GPUFeatureName>` capabilities                                     |
|   [7]   | `GPUSupportedLimits`   | limits object    | `maxBindGroups`, `maxTextureDimension2D`, etc.                         |

[PUBLIC_TYPE_SCOPE]: resource objects
- rail: type

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :------------------- | :---------------- | :----------------------------------------------- |
|   [1]   | `GPUBuffer`          | buffer object     | `mapAsync`, `getMappedRange`, `unmap`, `destroy` |
|   [2]   | `GPUTexture`         | texture object    | `createView`, `destroy`                          |
|   [3]   | `GPUTextureView`     | texture view      | aspect-scoped view into a `GPUTexture`           |
|   [4]   | `GPUSampler`         | sampler object    | filter mode, address mode, LOD clamp             |
|   [5]   | `GPUExternalTexture` | external texture  | wraps `HTMLVideoElement` / `VideoFrame`          |
|   [6]   | `GPUBindGroup`       | bind group        | resource binding set for a pipeline              |
|   [7]   | `GPUBindGroupLayout` | bind group layout | binding slot declarations                        |
|   [8]   | `GPUPipelineLayout`  | pipeline layout   | ordered `GPUBindGroupLayout` list                |

[PUBLIC_TYPE_SCOPE]: pipeline and shader objects
- rail: type

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------- | :--------------- | :------------------------------------- |
|   [1]   | `GPUShaderModule`    | compiled WGSL    | `getCompilationInfo()` for diagnostics |
|   [2]   | `GPURenderPipeline`  | render pipeline  | vertex + fragment stages               |
|   [3]   | `GPUComputePipeline` | compute pipeline | compute stage                          |
|   [4]   | `GPURenderBundle`    | recorded bundle  | pre-recorded render commands           |

[PUBLIC_TYPE_SCOPE]: command encoding objects
- rail: type

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                                                         |
| :-----: | :----------------------- | :---------------- | :------------------------------------------------------------- |
|   [1]   | `GPUCommandEncoder`      | command encoder   | `beginRenderPass`, `beginComputePass`, copy ops                |
|   [2]   | `GPUCommandBuffer`       | recorded commands | submitted to `GPUQueue.submit()`                               |
|   [3]   | `GPURenderPassEncoder`   | render pass       | draw, viewport, scissor, blend, bundle exec                    |
|   [4]   | `GPUComputePassEncoder`  | compute pass      | `dispatchWorkgroups`, pipeline bind, end                       |
|   [5]   | `GPURenderBundleEncoder` | bundle encoder    | pre-record reusable render commands                            |
|   [6]   | `GPUQueue`               | work queue        | `submit`, `writeBuffer`, `writeTexture`, `onSubmittedWorkDone` |
|   [7]   | `GPUQuerySet`            | query set         | occlusion / timestamp queries                                  |

[PUBLIC_TYPE_SCOPE]: canvas context
- rail: type

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [RAIL]                                                 |
| :-----: | :-------------------------- | :------------------ | :----------------------------------------------------- |
|   [1]   | `GPUCanvasContext`          | canvas context      | `configure`, `getCurrentTexture`, `unconfigure`        |
|   [2]   | `GPUCanvasConfiguration`    | canvas config desc  | `device`, `format`, `usage`, `colorSpace`, `alphaMode` |
|   [3]   | `GPUCanvasConfigurationOut` | config readback     | result of `getConfiguration()`                         |
|   [4]   | `GPUCanvasToneMapping`      | tone mapping config | `mode: GPUCanvasToneMappingMode`                       |

[PUBLIC_TYPE_SCOPE]: descriptor interfaces
- rail: type

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]         | [RAIL]                                                      |
| :-----: | :----------------------------- | :-------------------- | :---------------------------------------------------------- |
|   [1]   | `GPUBufferDescriptor`          | buffer desc           | `size`, `usage`, `mappedAtCreation`                         |
|   [2]   | `GPUTextureDescriptor`         | texture desc          | `size`, `format`, `usage`, `dimension`, `mipLevelCount`     |
|   [3]   | `GPUSamplerDescriptor`         | sampler desc          | `addressMode*`, `magFilter`, `minFilter`, `mipmapFilter`    |
|   [4]   | `GPURenderPipelineDescriptor`  | render pipeline desc  | `layout`, `vertex`, `fragment`, `primitive`, `depthStencil` |
|   [5]   | `GPUComputePipelineDescriptor` | compute pipeline desc | `layout`, `compute`                                         |
|   [6]   | `GPUShaderModuleDescriptor`    | shader module desc    | `code` (WGSL source), `hints`, `sourceMap`                  |
|   [7]   | `GPURenderPassDescriptor`      | render pass desc      | `colorAttachments`, `depthStencilAttachment`                |
|   [8]   | `GPUComputePassDescriptor`     | compute pass desc     | `timestampWrites?`                                          |
|   [9]   | `GPUBindGroupDescriptor`       | bind group desc       | `layout`, `entries`                                         |
|  [10]   | `GPUBindGroupLayoutDescriptor` | layout desc           | `entries: Iterable<GPUBindGroupLayoutEntry>`                |
|  [11]   | `GPUDeviceDescriptor`          | device desc           | `requiredFeatures`, `requiredLimits`                        |

[PUBLIC_TYPE_SCOPE]: primitive type aliases and enums
- rail: type

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                                           |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------- |
|   [1]   | `GPUTextureFormat`     | format union       | full BCn / ETC2 / ASTC / depth / color formats   |
|   [2]   | `GPUVertexFormat`      | vertex format      | `'float32x3'`, `'uint16x4'`, etc.                |
|   [3]   | `GPUAddressMode`       | address mode       | `'clamp-to-edge' \| 'repeat' \| 'mirror-repeat'` |
|   [4]   | `GPUFilterMode`        | filter mode        | `'nearest' \| 'linear'`                          |
|   [5]   | `GPUBlendFactor`       | blend factor       | src, dst, constant, saturated variants           |
|   [6]   | `GPUBlendOperation`    | blend op           | `'add' \| 'subtract' \| 'min' \| 'max'`          |
|   [7]   | `GPUPrimitiveTopology` | draw topology      | point-list, line-strip, triangle-list, etc.      |
|   [8]   | `GPUCullMode`          | culling            | `'none' \| 'front' \| 'back'`                    |
|   [9]   | `GPUIndexFormat`       | index format       | `'uint16' \| 'uint32'`                           |
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
|   [1]   | `GPUSize64`            | `number`      | byte sizes and offsets                           |
|   [2]   | `GPUSize32`            | `number`      | 32-bit count values                              |
|   [3]   | `GPUIndex32`           | `number`      | bind group slot indices                          |
|   [4]   | `GPUBufferUsageFlags`  | `number`      | `GPUBufferUsage.*` bitmask                       |
|   [5]   | `GPUTextureUsageFlags` | `number`      | `GPUTextureUsage.*` bitmask                      |
|   [6]   | `GPUShaderStageFlags`  | `number`      | `GPUShaderStage.*` bitmask                       |
|   [7]   | `GPUBindingResource`   | union         | sampler \| texture \| view \| buffer \| external |

[PUBLIC_TYPE_SCOPE]: error and event types
- rail: type

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------------------ | :--------------- | :------------------------------------- |
|   [1]   | `GPUUncapturedErrorEvent` | error event      | `.error: GPUError`                     |
|   [2]   | `GPUValidationError`      | validation error | device validation failure              |
|   [3]   | `GPUOutOfMemoryError`     | OOM error        | GPU allocation failure                 |
|   [4]   | `GPUInternalError`        | internal error   | driver-level failure                   |
|   [5]   | `GPUPipelineError`        | pipeline error   | `reason: GPUPipelineErrorReason`       |
|   [6]   | `GPUCompilationMessage`   | shader message   | `type`, `message`, `lineNum`, `offset` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GPU — adapter acquisition
- rail: type

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :----------------------------------------- | :-------------- | :---------------------------------- |
|   [1]   | `navigator.gpu.requestAdapter(options?)`   | adapter request | `Promise<GPUAdapter \| null>`       |
|   [2]   | `navigator.gpu.getPreferredCanvasFormat()` | format query    | optimal `GPUTextureFormat` for swap |
|   [3]   | `navigator.gpu.wgslLanguageFeatures`       | feature set     | supported WGSL extensions           |

[ENTRYPOINT_SCOPE]: GPUAdapter — device request
- rail: type

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :----------------------------------- | :------------- | :--------------------------- |
|   [1]   | `adapter.requestDevice(descriptor?)` | device request | `Promise<GPUDevice>`         |
|   [2]   | `adapter.features`                   | capability     | `GPUSupportedFeatures` set   |
|   [3]   | `adapter.limits`                     | capability     | `GPUSupportedLimits` values  |
|   [4]   | `adapter.info`                       | metadata       | vendor, architecture, device |

[ENTRYPOINT_SCOPE]: GPUDevice — resource creation
- rail: type

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]   | [RAIL]                           |
| :-----: | :-------------------------------------------------- | :--------------- | :------------------------------- |
|   [1]   | `device.createBuffer(descriptor)`                   | buffer factory   | `GPUBuffer`                      |
|   [2]   | `device.createTexture(descriptor)`                  | texture factory  | `GPUTexture`                     |
|   [3]   | `device.createSampler(descriptor?)`                 | sampler factory  | `GPUSampler`                     |
|   [4]   | `device.createShaderModule(descriptor)`             | shader compile   | `GPUShaderModule`                |
|   [5]   | `device.createRenderPipeline(descriptor)`           | pipeline factory | `GPURenderPipeline` (sync)       |
|   [6]   | `device.createComputePipeline(descriptor)`          | pipeline factory | `GPUComputePipeline` (sync)      |
|   [7]   | `device.createRenderPipelineAsync(desc)`            | async pipeline   | `Promise<GPURenderPipeline>`     |
|   [8]   | `device.createBindGroupLayout(descriptor)`          | layout factory   | `GPUBindGroupLayout`             |
|   [9]   | `device.createPipelineLayout(descriptor)`           | layout factory   | `GPUPipelineLayout`              |
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
|   [1]   | `encoder.beginRenderPass(descriptor)`         | pass factory   | `GPURenderPassEncoder`        |
|   [2]   | `encoder.beginComputePass(descriptor?)`       | pass factory   | `GPUComputePassEncoder`       |
|   [3]   | `encoder.copyBufferToBuffer(src, dst, size?)` | copy           | buffer → buffer copy          |
|   [4]   | `encoder.finish()`                            | finalize       | `GPUCommandBuffer` for submit |

[ENTRYPOINT_SCOPE]: GPURenderPassEncoder — draw commands
- rail: type

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `pass.setPipeline(pipeline)`                   | state set      | bind render pipeline          |
|   [2]   | `pass.setBindGroup(index, bindGroup)`          | state set      | bind resource group at slot   |
|   [3]   | `pass.setVertexBuffer(slot, buffer, offset?)`  | state set      | bind vertex attribute buffer  |
|   [4]   | `pass.setIndexBuffer(buffer, format, offset?)` | state set      | bind index buffer             |
|   [5]   | `pass.draw(vertexCount, instanceCount?)`       | draw call      | non-indexed draw              |
|   [6]   | `pass.drawIndexed(indexCount, instanceCount?)` | draw call      | indexed draw                  |
|   [7]   | `pass.setViewport(x, y, w, h, minD, maxD)`     | raster state   | NDC → viewport mapping        |
|   [8]   | `pass.setScissorRect(x, y, w, h)`              | raster state   | scissor rectangle             |
|   [9]   | `pass.executeBundles(bundles)`                 | bundle exec    | replay `GPURenderBundle` list |
|  [10]   | `pass.end()`                                   | finalize       | close render pass             |

[ENTRYPOINT_SCOPE]: GPUQueue — work submission
- rail: type

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------------------------ | :------------- | :--------------------------------- |
|   [1]   | `queue.submit(commandBuffers)`                          | submit         | schedule `GPUCommandBuffer` array  |
|   [2]   | `queue.writeBuffer(buffer, offset, data, doff?, size?)` | data upload    | CPU → GPU buffer write             |
|   [3]   | `queue.writeTexture(dst, data, layout, size)`           | data upload    | CPU → GPU texture write            |
|   [4]   | `queue.onSubmittedWorkDone()`                           | sync fence     | `Promise<undefined>` on completion |

[ENTRYPOINT_SCOPE]: GPUCanvasContext — swap chain
- rail: type

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------------------- | :------------- | :---------------------------------- |
|   [1]   | `ctx.configure(configuration)` | setup          | attach device and format to canvas  |
|   [2]   | `ctx.getCurrentTexture()`      | frame acquire  | next presentable `GPUTexture`       |
|   [3]   | `ctx.unconfigure()`            | teardown       | remove config, destroy textures     |
|   [4]   | `ctx.getConfiguration()`       | config query   | `GPUCanvasConfigurationOut \| null` |

## [4]-[IMPLEMENTATION_LAW]

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
