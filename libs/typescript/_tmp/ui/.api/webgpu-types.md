# [API_CATALOGUE] @webgpu/types

`@webgpu/types` supplies the ambient TypeScript declarations for the W3C WebGPU API — `GPU`, `GPUAdapter`, `GPUDevice`, the resource objects (`GPUBuffer`/`GPUTexture`/`GPUSampler`/`GPUBindGroup`), the pipeline/shader objects, the command encoders, `GPUQueue`, `GPUCanvasContext`, and the full descriptor/enum/flag vocabulary. It has no runtime: it merges global `GPU*` declarations onto `navigator.gpu`, `Navigator`/`WorkerNavigator`, and `HTMLCanvasElement`/`OffscreenCanvas.getContext('webgpu')`, admitted through the `tsconfig` `types` array. Its command surface is mixin-composed — `GPUObjectBase.label` plus the `GPURenderCommandsMixin`/`GPUBindingCommandsMixin`/`GPUDebugCommandsMixin` vocabularies shared across the render pass and render-bundle encoders — one command algebra, not per-encoder duplication. In the ui stack these types type the `three/webgpu` `WebGPURenderer`/`WebGPUBackend` device (the sanctioned owner, `three.md`), deck.gl's `@luma.gl` device, and any `navigator.gpu` feature probe; the `render/glb.md#GLB_VIEWPORT` leaf never hand-authors a `GPUDevice` path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@webgpu/types`
- package / version: `@webgpu/types` @ `0.1.71`
- license: `BSD-3-Clause`
- module: ambient `.d.ts` only (`dist/index.d.ts`) — NO runtime, NO value exports; global `GPU*` interfaces + ambient flag-constant `declare var`s
- admission: `tsconfig.json` `"types": ["@webgpu/types"]` or `/// <reference types="@webgpu/types" />`; a `devDependency`, never a value import
- rail: type

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: adapter + device acquisition
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPU`                  | entry point | `requestAdapter(options?)`, `getPreferredCanvasFormat()`, `wgslLanguageFeatures` |
|  [02]   | `GPUAdapter`           | adapter handle | `requestDevice()`, `features`, `limits`, `info` |
|  [03]   | `GPUAdapterInfo`       | adapter metadata | `vendor`, `architecture`, `device`, `description`, `subgroupMinSize`, `subgroupMaxSize`, `isFallbackAdapter` |
|  [04]   | `GPUDevice`            | device handle | resource creation, `queue`, `features`, `limits`, `adapterInfo`, `lost: Promise<GPUDeviceLostInfo>`, `onuncapturederror`, `pushErrorScope`/`popErrorScope`, `destroy()` |
|  [05]   | `GPUDeviceLostInfo`    | device-loss detail | `reason: GPUDeviceLostReason` (`'unknown'` \| `'destroyed'`), `message` |
|  [06]   | `NavigatorGPU`         | navigator mixin | `readonly gpu: GPU` on `Navigator` / `WorkerNavigator` |
|  [07]   | `GPUSupportedFeatures` | feature set | `ReadonlySet<GPUFeatureName>` — the adapter/device capability gate |
|  [08]   | `GPUSupportedLimits`   | limits object | `maxBindGroups`, `maxTextureDimension2D`, `maxComputeWorkgroupSizeX`, … |
|  [09]   | `WGSLLanguageFeatures` | WGSL feature set | `ReadonlySet<string>` behind `gpu.wgslLanguageFeatures` |

[PUBLIC_TYPE_SCOPE]: resource objects (every object extends `GPUObjectBase` → carries a writable `label`)
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUBuffer`          | buffer object | `mapAsync`, `getMappedRange`, `unmap`, `destroy`; readonly `size`, `usage`, `mapState: GPUBufferMapState` |
|  [02]   | `GPUTexture`         | texture object | `createView`, `destroy`; readonly `width`/`height`/`depthOrArrayLayers`/`mipLevelCount`/`sampleCount`/`dimension`/`format`/`usage` |
|  [03]   | `GPUTextureView`     | texture view | aspect/mip/layer-scoped view into a `GPUTexture` |
|  [04]   | `GPUSampler`         | sampler object | filter mode, address mode, LOD clamp, compare |
|  [05]   | `GPUExternalTexture` | external texture | wraps `HTMLVideoElement` / `VideoFrame` |
|  [06]   | `GPUBindGroup`       | bind group | resource binding set for a pipeline |
|  [07]   | `GPUBindGroupLayout` | bind group layout | binding slot declarations |
|  [08]   | `GPUPipelineLayout`  | pipeline layout | ordered `GPUBindGroupLayout` list |

[PUBLIC_TYPE_SCOPE]: pipeline + shader objects
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUShaderModule`    | compiled WGSL | `getCompilationInfo(): Promise<GPUCompilationInfo>` for diagnostics |
|  [02]   | `GPURenderPipeline`  | render pipeline | vertex + fragment stages; `getBindGroupLayout(index)` |
|  [03]   | `GPUComputePipeline` | compute pipeline | compute stage; `getBindGroupLayout(index)` |
|  [04]   | `GPURenderBundle`    | recorded bundle | pre-recorded render commands, replayed via `pass.executeBundles` |

[PUBLIC_TYPE_SCOPE]: command encoding objects (composed from the command mixins below)
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUCommandEncoder`      | command encoder | `beginRenderPass`, `beginComputePass`, buffer/texture copies, `clearBuffer`, `resolveQuerySet`, `finish` + debug mixin |
|  [02]   | `GPUCommandBuffer`       | recorded commands | submitted to `GPUQueue.submit()` |
|  [03]   | `GPURenderPassEncoder`   | render pass | raster state + `GPURenderCommandsMixin` + `GPUBindingCommandsMixin` + `GPUDebugCommandsMixin` |
|  [04]   | `GPUComputePassEncoder`  | compute pass | `setPipeline`, `dispatchWorkgroups`, `dispatchWorkgroupsIndirect`, `end` + binding + debug mixins |
|  [05]   | `GPURenderBundleEncoder` | bundle encoder | `finish(): GPURenderBundle` + the same render/binding/debug mixins as the render pass |
|  [06]   | `GPUQueue`               | work queue | `submit`, `writeBuffer`, `writeTexture`, `copyExternalImageToTexture`, `onSubmittedWorkDone` |
|  [07]   | `GPUQuerySet`            | query set | occlusion / timestamp queries; readonly `type`, `count` |

[PUBLIC_TYPE_SCOPE]: command mixins (the shared command vocabulary encoders compose)
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUObjectBase`           | base mixin | writable `label: string` on every GPU object — the debug-name seam |
|  [02]   | `GPURenderCommandsMixin`  | draw commands | `setPipeline`, `setIndexBuffer`, `setVertexBuffer`, `draw`, `drawIndexed`, `drawIndirect`, `drawIndexedIndirect` — shared by render pass + bundle encoder |
|  [03]   | `GPUBindingCommandsMixin` | binding commands | `setBindGroup` (dynamic offsets), `setImmediates` — shared by render pass, compute pass, bundle encoder |
|  [04]   | `GPUDebugCommandsMixin`   | debug markers | `pushDebugGroup`, `popDebugGroup`, `insertDebugMarker` — on all encoders/passes |
|  [05]   | `GPUCommandsMixin`        | marker | the empty base every encoder shares |

[PUBLIC_TYPE_SCOPE]: canvas context
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUCanvasContext`          | canvas context | `configure`, `getCurrentTexture`, `unconfigure`, `getConfiguration` |
|  [02]   | `GPUCanvasConfiguration`    | canvas config | `device`, `format`, `usage`, `viewFormats`, `colorSpace: PredefinedColorSpace`, `toneMapping: GPUCanvasToneMapping`, `alphaMode: GPUCanvasAlphaMode` |
|  [03]   | `GPUCanvasConfigurationOut`  | config readback | result of `getConfiguration()` |
|  [04]   | `GPUCanvasToneMapping`      | tone mapping | `{ mode: GPUCanvasToneMappingMode }` (`'standard'` \| `'extended'`) |

[PUBLIC_TYPE_SCOPE]: descriptor interfaces
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPURequestAdapterOptions`     | adapter opts | `powerPreference: GPUPowerPreference`, `forceFallbackAdapter`, `featureLevel` |
|  [02]   | `GPUDeviceDescriptor`          | device desc | `requiredFeatures: Iterable<GPUFeatureName>`, `requiredLimits`, `defaultQueue` |
|  [03]   | `GPUBufferDescriptor`          | buffer desc | `size`, `usage`, `mappedAtCreation` |
|  [04]   | `GPUTextureDescriptor`         | texture desc | `size`, `format`, `usage`, `dimension`, `mipLevelCount`, `sampleCount`, `viewFormats` |
|  [05]   | `GPUSamplerDescriptor`         | sampler desc | `addressMode{U,V,W}`, `magFilter`, `minFilter`, `mipmapFilter`, `lodMinClamp`/`lodMaxClamp`, `compare` |
|  [06]   | `GPURenderPipelineDescriptor`  | render pipeline desc | `layout: GPUPipelineLayout \| GPUAutoLayoutMode`, `vertex`, `fragment`, `primitive`, `depthStencil`, `multisample` |
|  [07]   | `GPUVertexState` / `GPUFragmentState` / `GPUPrimitiveState` / `GPUDepthStencilState` / `GPUMultisampleState` | pipeline stage descs | the render-pipeline stage/state fields; `GPUFragmentState.targets: GPUColorTargetState[]` |
|  [08]   | `GPUColorTargetState` / `GPUBlendState` / `GPUBlendComponent` | color target | `format`, `blend: { color, alpha }`, `writeMask: GPUColorWriteFlags` |
|  [09]   | `GPUVertexBufferLayout` / `GPUVertexAttribute` | vertex layout | `arrayStride`, `stepMode`, `attributes: { format, offset, shaderLocation }` |
|  [10]   | `GPUComputePipelineDescriptor` / `GPUProgrammableStage` | compute pipeline desc | `layout`, `compute: { module, entryPoint?, constants? }` |
|  [11]   | `GPUShaderModuleDescriptor`    | shader module desc | `code` (WGSL source), `compilationHints`, `sourceMap` |
|  [12]   | `GPURenderPassDescriptor`      | render pass desc | `colorAttachments: GPURenderPassColorAttachment[]`, `depthStencilAttachment`, `occlusionQuerySet`, `timestampWrites` |
|  [13]   | `GPURenderPassColorAttachment` / `GPURenderPassDepthStencilAttachment` | pass attachments | `view`, `loadOp`, `storeOp`, `clearValue: GPUColor`, `resolveTarget` |
|  [14]   | `GPUComputePassDescriptor`     | compute pass desc | `timestampWrites` |
|  [15]   | `GPUBindGroupDescriptor` / `GPUBindGroupEntry` | bind group desc | `layout`, `entries: { binding, resource }` |
|  [16]   | `GPUBindGroupLayoutDescriptor` / `GPUBindGroupLayoutEntry` | layout desc | `entries` each with `binding`, `visibility: GPUShaderStageFlags`, and ONE of the binding-layout family below |
|  [17]   | `GPUBufferBindingLayout` / `GPUSamplerBindingLayout` / `GPUTextureBindingLayout` / `GPUStorageTextureBindingLayout` / `GPUExternalTextureBindingLayout` | binding layouts | the five binding-slot kinds a `GPUBindGroupLayoutEntry` selects among |

[PUBLIC_TYPE_SCOPE]: flag-constant namespaces (ambient `declare var` — bitmask holders; the `*Flags` aliases are `number`)
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUBufferUsage`   | buffer usage flags | `MAP_READ`/`MAP_WRITE`/`COPY_SRC`/`COPY_DST`/`INDEX`/`VERTEX`/`UNIFORM`/`STORAGE`/`INDIRECT`/`QUERY_RESOLVE` → `GPUBufferUsageFlags` |
|  [02]   | `GPUTextureUsage`  | texture usage flags | `COPY_SRC`/`COPY_DST`/`TEXTURE_BINDING`/`STORAGE_BINDING`/`RENDER_ATTACHMENT` → `GPUTextureUsageFlags` |
|  [03]   | `GPUShaderStage`   | shader stage flags | `VERTEX`/`FRAGMENT`/`COMPUTE` → `GPUShaderStageFlags` (bind-group `visibility`) |
|  [04]   | `GPUColorWrite`    | color write mask | `RED`/`GREEN`/`BLUE`/`ALPHA`/`ALL` → `GPUColorWriteFlags` (`GPUColorTargetState.writeMask`) |
|  [05]   | `GPUMapMode`       | buffer map mode | `READ`/`WRITE` → `GPUMapModeFlags` (`GPUBuffer.mapAsync`) |

[PUBLIC_TYPE_SCOPE]: enum string unions
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUTextureFormat`     | format union | full BCn / ETC2 / ASTC / depth / color format set |
|  [02]   | `GPUVertexFormat`      | vertex format | `'float32x3'`, `'uint16x4'`, `'unorm8x4'`, … |
|  [03]   | `GPUAddressMode` / `GPUFilterMode` / `GPUMipmapFilterMode` | sampler enums | `'clamp-to-edge'\|'repeat'\|'mirror-repeat'`; `'nearest'\|'linear'` |
|  [04]   | `GPUBlendFactor` / `GPUBlendOperation` | blend enums | src/dst/constant/saturated factors; `'add'\|'subtract'\|'reverse-subtract'\|'min'\|'max'` |
|  [05]   | `GPUPrimitiveTopology` / `GPUCullMode` / `GPUFrontFace` / `GPUIndexFormat` | raster enums | point/line/triangle topologies; `'none'\|'front'\|'back'`; `'ccw'\|'cw'`; `'uint16'\|'uint32'` |
|  [06]   | `GPULoadOp` / `GPUStoreOp` | attachment ops | `'load'\|'clear'`; `'store'\|'discard'` |
|  [07]   | `GPUCompareFunction` / `GPUStencilOperation` | depth/stencil enums | `'never'`/`'less'`/`'equal'`/… ; `'keep'`/`'replace'`/`'increment-clamp'`/… |
|  [08]   | `GPUFeatureName`       | feature names | incl. `'core-features-and-limits'`, `'depth-clip-control'`, `'timestamp-query'`, `'shader-f16'`, `'subgroups'`, `'dual-source-blending'`, `'clip-distances'`, `'texture-component-swizzle'`, `'float32-blendable'`, the `texture-compression-*` set |
|  [09]   | `GPUBufferMapState` / `GPUErrorFilter` / `GPUPipelineErrorReason` | state/error enums | `'unmapped'\|'pending'\|'mapped'`; `'validation'\|'out-of-memory'\|'internal'`; `'validation'\|'internal'` |
|  [10]   | `GPUCanvasAlphaMode` / `GPUCanvasToneMappingMode` / `GPUDeviceLostReason` / `GPUAutoLayoutMode` / `GPUPowerPreference` | context/device enums | `'opaque'\|'premultiplied'`; `'standard'\|'extended'`; `'unknown'\|'destroyed'`; `'auto'`; `'low-power'\|'high-performance'` |
|  [11]   | `GPUCompilationMessageType` | shader message | `'error'\|'warning'\|'info'` |

[PUBLIC_TYPE_SCOPE]: numeric aliases, dimension types, deprecated aliases
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUSize64` / `GPUSize32` / `GPUIndex32` / `GPUFlagsConstant` | numeric aliases | byte sizes/offsets; 32-bit counts; slot indices; the flag-constant type |
|  [02]   | `GPUBufferUsageFlags` / `GPUTextureUsageFlags` / `GPUShaderStageFlags` / `GPUColorWriteFlags` / `GPUMapModeFlags` | flag aliases | `number` bitmasks from the flag namespaces above |
|  [03]   | `GPUBindingResource` | union | sampler \| texture view \| buffer binding \| external texture |
|  [04]   | `GPUExtent3DStrict` / `GPUOrigin2DStrict` / `GPUOrigin3D` / `GPUExtent3D` | dimension descs | the `Strict` forms catch `depth` vs `depthOrArrayLayers` misuse at the type level |
|  [05]   | `GPUTexelCopyBufferLayout` / `GPUTexelCopyBufferInfo` / `GPUTexelCopyTextureInfo` / `GPUCopyExternalImageDestInfo` / `GPUCopyExternalImageSourceInfo` / `GPUCopyExternalImageSource` | canonical copy types | the current names for `writeTexture`/`copyExternalImageToTexture` operands |
|  [06]   | `GPUImageDataLayout` → `GPUTexelCopyBufferLayout`; `GPUImageCopyBuffer` → `GPUTexelCopyBufferInfo`; `GPUImageCopyTexture` → `GPUTexelCopyTextureInfo`; `GPUImageCopyTextureTagged` → `GPUCopyExternalImageDestInfo`; `GPUImageCopyExternalImage` → `GPUCopyExternalImageSourceInfo`; `GPUImageCopyExternalImageSource` → `GPUCopyExternalImageSource` | deprecated aliases | `@deprecated` — use the right-hand canonical name |

[PUBLIC_TYPE_SCOPE]: error + event types
- rail: type

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `GPUError`                | error base | `readonly message` — base of the error hierarchy |
|  [02]   | `GPUValidationError` / `GPUOutOfMemoryError` / `GPUInternalError` | error scopes | the three `pushErrorScope`/`popErrorScope` filter outcomes |
|  [03]   | `GPUUncapturedErrorEvent` | error event | `.error: GPUError` on `device.onuncapturederror` |
|  [04]   | `GPUPipelineError`        | pipeline error | `reason: GPUPipelineErrorReason` from async pipeline creation |
|  [05]   | `GPUCompilationInfo` / `GPUCompilationMessage` | shader diagnostics | `messages[]` with `type`, `message`, `lineNum`, `linePos`, `offset`, `length` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GPU — adapter acquisition
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `navigator.gpu.requestAdapter(options?: GPURequestAdapterOptions)` | adapter request | `Promise<GPUAdapter \| null>`; `powerPreference`/`forceFallbackAdapter` |
|  [02]   | `navigator.gpu.getPreferredCanvasFormat()` | format query | optimal `GPUTextureFormat` for the swap chain |
|  [03]   | `navigator.gpu.wgslLanguageFeatures` | feature set | supported WGSL extensions (`WGSLLanguageFeatures`) |

[ENTRYPOINT_SCOPE]: GPUAdapter — device request + capability
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `adapter.requestDevice(descriptor?: GPUDeviceDescriptor)` | device request | `Promise<GPUDevice>`; opt features via `requiredFeatures`/`requiredLimits` |
|  [02]   | `adapter.features` / `adapter.limits` / `adapter.info` | capability | `GPUSupportedFeatures` / `GPUSupportedLimits` / `GPUAdapterInfo` |

[ENTRYPOINT_SCOPE]: GPUDevice — resource creation + lifecycle rails
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `device.createBuffer` / `createTexture` / `createSampler` | resource factory | `GPUBuffer` / `GPUTexture` / `GPUSampler` |
|  [02]   | `device.createShaderModule` | shader compile | `GPUShaderModule` (`getCompilationInfo()` for diagnostics) |
|  [03]   | `device.createRenderPipeline` / `createComputePipeline` | pipeline factory | sync `GPURenderPipeline` / `GPUComputePipeline` |
|  [04]   | `device.createRenderPipelineAsync` / `createComputePipelineAsync` | async pipeline | `Promise<…>`; rejects with `GPUPipelineError` |
|  [05]   | `device.createBindGroupLayout` / `createPipelineLayout` / `createBindGroup` | layout/binding | `GPUBindGroupLayout` / `GPUPipelineLayout` / `GPUBindGroup` |
|  [06]   | `device.createCommandEncoder` / `createRenderBundleEncoder` | encoder factory | `GPUCommandEncoder` / `GPURenderBundleEncoder` |
|  [07]   | `device.createQuerySet` / `importExternalTexture` | query/import | `GPUQuerySet` (occlusion/timestamp) / `GPUExternalTexture` |
|  [08]   | `device.queue` / `device.features` / `device.limits` / `device.adapterInfo` | handles | the default `GPUQueue`; the granted feature/limit/adapter facts |
|  [09]   | `device.lost` / `device.onuncapturederror` | loss/error rails | `Promise<GPUDeviceLostInfo>`; the `GPUUncapturedErrorEvent` handler |
|  [10]   | `device.pushErrorScope(filter)` / `popErrorScope()` | error scope | `Promise<GPUError \| null>` — structured GPU error capture |
|  [11]   | `device.destroy()` | cleanup | release device + outstanding ops |

[ENTRYPOINT_SCOPE]: GPUCommandEncoder — encoding + copies + debug
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `encoder.beginRenderPass(descriptor)` / `beginComputePass(descriptor?)` | pass factory | `GPURenderPassEncoder` / `GPUComputePassEncoder` |
|  [02]   | `encoder.copyBufferToBuffer` / `copyBufferToTexture` / `copyTextureToBuffer` / `copyTextureToTexture` | copies | buffer/texture resource copies |
|  [03]   | `encoder.clearBuffer(buffer, offset?, size?)` / `resolveQuerySet(set, first, count, dst, offset)` | clear/resolve | zero a buffer range; resolve query results into a buffer |
|  [04]   | `encoder.pushDebugGroup` / `popDebugGroup` / `insertDebugMarker` | debug | `GPUDebugCommandsMixin` markers |
|  [05]   | `encoder.finish(descriptor?)` | finalize | `GPUCommandBuffer` for submit |

[ENTRYPOINT_SCOPE]: GPURenderPassEncoder — raster + draw (draw/bind/debug from the shared mixins)
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `pass.setPipeline` / `setBindGroup` / `setVertexBuffer` / `setIndexBuffer` | state set | `GPURenderCommandsMixin` + `GPUBindingCommandsMixin` |
|  [02]   | `pass.draw` / `drawIndexed` / `drawIndirect` / `drawIndexedIndirect` | draw call | direct + GPU-driven indirect draws |
|  [03]   | `pass.setViewport` / `setScissorRect` / `setBlendConstant` / `setStencilReference` | raster state | viewport, scissor, blend constant, stencil ref |
|  [04]   | `pass.beginOcclusionQuery(index)` / `endOcclusionQuery()` | occlusion query | occlusion visibility counts into the pass `occlusionQuerySet` |
|  [05]   | `pass.executeBundles(bundles)` / `pass.end()` | bundle/finalize | replay a `GPURenderBundle[]`; close the pass |

[ENTRYPOINT_SCOPE]: GPUComputePassEncoder / GPURenderBundleEncoder
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `computePass.setPipeline` / `setBindGroup` | state set | pipeline + binding mixin |
|  [02]   | `computePass.dispatchWorkgroups(x, y?, z?)` / `dispatchWorkgroupsIndirect(buffer, offset)` | dispatch | direct + GPU-driven indirect compute |
|  [03]   | `computePass.end()` | finalize | close the compute pass |
|  [04]   | `bundleEncoder.finish(descriptor?)` | finalize | `GPURenderBundle`; the encoder shares the render/binding/debug mixins for pre-recording |

[ENTRYPOINT_SCOPE]: GPUQueue — work submission + uploads
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `queue.submit(commandBuffers)` | submit | schedule a `GPUCommandBuffer[]` |
|  [02]   | `queue.writeBuffer(buffer, offset, data, dataOffset?, size?)` | data upload | CPU → GPU buffer write |
|  [03]   | `queue.writeTexture(destination, data, dataLayout, size)` | data upload | CPU → GPU texture write |
|  [04]   | `queue.copyExternalImageToTexture(source, destination, copySize)` | image upload | `ImageBitmap`/`HTMLCanvasElement`/`VideoFrame` → texture |
|  [05]   | `queue.onSubmittedWorkDone()` | sync fence | `Promise<undefined>` on completion |

[ENTRYPOINT_SCOPE]: GPUCanvasContext — swap chain
- rail: type

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `ctx.configure(configuration)` | setup | attach device/format/tone-mapping/alpha-mode to the canvas |
|  [02]   | `ctx.getCurrentTexture()` | frame acquire | next presentable `GPUTexture` |
|  [03]   | `ctx.unconfigure()` / `ctx.getConfiguration()` | teardown/query | remove config; read back `GPUCanvasConfigurationOut \| null` |

## [04]-[IMPLEMENTATION_LAW]

[TYPE_TOPOLOGY]:
- ambient-only, no runtime: include via the `tsconfig` `types` array or a `/// <reference types="@webgpu/types" />` directive; all `GPU*` interfaces are global, so no import is needed at use sites, and `HTMLCanvasElement.getContext('webgpu')` / `OffscreenCanvas.getContext('webgpu')` return `GPUCanvasContext | null` once included.
- the command surface is mixin-composed: `GPUObjectBase` (writable `label`) + `GPUCommandsMixin` + `GPUDebugCommandsMixin` + `GPURenderCommandsMixin` + `GPUBindingCommandsMixin`. `GPURenderPassEncoder` AND `GPURenderBundleEncoder` share the render/binding/debug vocabulary; `GPUComputePassEncoder` shares binding/debug — one draw/bind/debug algebra, never a per-encoder re-declaration.
- flag constants are ambient `declare var` namespaces (`GPUBufferUsage.MAP_READ`, `GPUTextureUsage.RENDER_ATTACHMENT`, `GPUShaderStage.FRAGMENT`, `GPUColorWrite.ALL`, `GPUMapMode.READ`) of type `GPUFlagsConstant`; the `*Flags` aliases are `number` bitmasks.
- `GPUExtent3DStrict`/`GPUOrigin2DStrict` catch `depth` vs `depthOrArrayLayers` at the type level; the six `GPUImageCopy*`/`GPUImageDataLayout` aliases are `@deprecated` → the `GPUTexelCopy*`/`GPUCopyExternalImage*` canonical names.
- device-loss + error rails: `device.lost` (`Promise<GPUDeviceLostInfo>` with `reason`/`message`), `device.onuncapturederror` (`GPUUncapturedErrorEvent`), and `pushErrorScope(filter)`/`popErrorScope()` (`Promise<GPUError | null>`) surround async GPU work; the scoped errors are `GPUValidationError`/`GPUOutOfMemoryError`/`GPUInternalError`, async pipeline creation rejects with `GPUPipelineError`.
- capability gating: read `adapter.features`/`device.features` (`GPUSupportedFeatures`, a `ReadonlySet<GPUFeatureName>`) and `.limits` before enabling a path; request them in `GPUDeviceDescriptor.requiredFeatures`/`requiredLimits` — an unrequested feature makes the dependent operation fail validation. `layout: 'auto'` (`GPUAutoLayoutMode`) skips an explicit pipeline layout.

[STACKING]:
- sanctioned device owner: `three/webgpu` `WebGPURenderer`/`WebGPUBackend` (`three.md`) owns the `GPUDevice`; `@webgpu/types` provides the ambient `GPU*` globals that type that path and its `renderer.init()` auto-detect / `WebGLBackend` fallback — the `render/glb.md#GLB_VIEWPORT` leaf composes it, never a hand-authored `GPUDevice` or `navigator.gpu` degrade branch.
- deck.gl device substrate: `@luma.gl/*` (deck.gl's WebGL2/WebGPU device — `deck.gl-core.md`) is typed by these ambient globals, so the geo overlay stack reads the same `GPUDevice`/`GPUCanvasContext` surface.
- capability probe (NOT backend selection): a direct `navigator.gpu.requestAdapter()` → `adapter.features` (`GPUSupportedFeatures`) read gates OPTIONAL feature arms — admit a `shader-f16`/`timestamp-query`/`subgroups` compute path (the `render/glb.md` cluster-LOD TSL ambition) only when the set carries it, and pass the granted names into `GPUDeviceDescriptor.requiredFeatures`. The `three/webgpu`-vs-WebGL decision is NOT this probe's job — that is `WebGPURenderer.init()`'s own auto-detect read back through `.backend.isWebGLBackend` (`three.md`), never a hand-rolled `navigator.gpu` degrade branch.
- Effect resource lifecycle: wrap the device/pipeline/context in `Effect.acquireRelease` (as `render/glb.md` wraps the renderer + `renderer.dispose()`); fold `device.lost` (a `Promise`) into an `Effect` fiber for device-loss recovery, `createRenderPipelineAsync`/`queue.onSubmittedWorkDone()` into `Effect.promise`, and map `pushErrorScope`/`popErrorScope` verdicts onto the `interchange` `FaultDetail` typed rail.
- ambient admission: add `"@webgpu/types"` to the branch `tsconfig` `types` once; every viewport/compute owner reads the globals with no import.

[LOCAL_ADMISSION]:
- Add `"@webgpu/types"` to `tsconfig.json` `"types"`; never import GPU symbols at runtime.
- Use `GPUExtent3DStrict`/`GPUOrigin2DStrict` over the bare unions, and the `GPUTexelCopy*`/`GPUCopyExternalImage*` names over the deprecated `GPUImageCopy*`/`GPUImageDataLayout` aliases.
- Request device features explicitly in `GPUDeviceDescriptor.requiredFeatures`; wrap async GPU work in error scopes to capture validation/OOM/internal errors without unhandled rejections.
- Read the shared mixin command vocabulary (`setBindGroup`/`draw`/`pushDebugGroup`) rather than treating each encoder's commands as distinct; set an object `label` for debuggable resources.
- Let `three/webgpu` own the device; use these types to gate/probe/type, not to hand-roll a renderer.

[RAIL_LAW]:
- package: `@webgpu/types`
- owns: the ambient TypeScript declarations for the W3C WebGPU API — device/queue/encoder/pass/pipeline/resource/canvas objects, the mixin command vocabulary, the flag-constant namespaces, the strict dimension types, and the descriptor/enum vocabulary
- accept: global `GPU*` usage without import, `GPUExtent3DStrict` dimension descriptors, the `GPUTexelCopy*`/`GPUCopyExternalImage*` names, explicit `requiredFeatures`/`requiredLimits` gating, error-scope + `device.lost` rails
- reject: a runtime import of `@webgpu/types`; the deprecated `GPUImageCopy*`/`GPUImageDataLayout` aliases; a hand-authored `GPUDevice`/`navigator.gpu` renderer path beside the `three/webgpu` owner
