# [RASM_API_SILK_WEBGPU]

`Silk.NET.WebGPU` binds the native `wgpu_native` runtime against the canonical `webgpu.h` surface: `WebGPU.GetApi()` roots an `unsafe` function table whose instance methods marshal raw-pointer descriptor structs across the instance/adapter/device/queue lifecycle. One binding serves three planes — the presented viewport, the GPGPU dispatch lane, and the surfaceless bake device — because a surface is a `SurfaceDescriptor` a plane either chains or omits, never a second binding.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and device-lifecycle owners

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [CAPABILITY]                  |
| :-----: | :--------- | :-------------- | :---------------------------- |
|  [01]   | `WebGPU`   | static API root | global entry, function table  |
|  [02]   | `Instance` | native handle   | adapter request root          |
|  [03]   | `Adapter`  | native handle   | device request, feature query |
|  [04]   | `Device`   | native handle   | resource factory              |
|  [05]   | `Queue`    | native handle   | command submission, upload    |
|  [06]   | `Surface`  | native handle   | swapchain present target      |

[PUBLIC_TYPE_SCOPE]: resource, pipeline, and recording owners

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------- | :------------ | :---------------------------- |
|  [01]   | `Buffer`             | native handle | storage/uniform/staging alloc |
|  [02]   | `Texture`            | native handle | GPU texture allocation        |
|  [03]   | `TextureView`        | native handle | texture view                  |
|  [04]   | `Sampler`            | native handle | texture sampler               |
|  [05]   | `ShaderModule`       | native handle | WGSL/SPIR-V shader            |
|  [06]   | `BindGroupLayout`    | native handle | binding layout                |
|  [07]   | `BindGroup`          | native handle | bound resource group          |
|  [08]   | `PipelineLayout`     | native handle | pipeline layout               |
|  [09]   | `RenderPipeline`     | native handle | raster pipeline               |
|  [10]   | `ComputePipeline`    | native handle | compute pipeline              |
|  [11]   | `CommandEncoder`     | native handle | command recorder              |
|  [12]   | `RenderPassEncoder`  | native handle | raster pass recorder          |
|  [13]   | `ComputePassEncoder` | native handle | compute pass recorder         |
|  [14]   | `CommandBuffer`      | native handle | finished command buffer       |
|  [15]   | `QuerySet`           | native handle | timestamp/occlusion pool      |
|  [16]   | `SurfaceTexture`     | struct        | per-frame swapchain image     |

[PUBLIC_TYPE_SCOPE]: lifecycle, surface, and transfer descriptors

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `InstanceDescriptor`                       | descriptor    | instance options                                |
|  [02]   | `RequestAdapterOptions`                    | descriptor    | adapter request (`CompatibleSurface` nullable)  |
|  [03]   | `DeviceDescriptor`                         | descriptor    | device request, required features and limits    |
|  [04]   | `SupportedLimits` / `Limits`               | struct        | `AdapterGetLimits`/`DeviceGetLimits` out-struct |
|  [05]   | `AdapterProperties`                        | struct        | vendor, architecture, backend identity          |
|  [06]   | `SurfaceDescriptor`                        | descriptor    | surface source chain                            |
|  [07]   | `SurfaceDescriptorFromMetalLayer`          | descriptor    | Metal surface source                            |
|  [08]   | `SurfaceDescriptorFromWindowsHWND`         | descriptor    | Windows surface source                          |
|  [09]   | `SurfaceDescriptorFromXlibWindow`          | descriptor    | Xlib surface source                             |
|  [10]   | `SurfaceDescriptorFromWaylandSurface`      | descriptor    | Wayland surface source                          |
|  [11]   | `SurfaceDescriptorFromXcbWindow`           | descriptor    | XCB surface source                              |
|  [12]   | `SurfaceDescriptorFromAndroidNativeWindow` | descriptor    | Android surface source                          |
|  [13]   | `SurfaceConfiguration`                     | descriptor    | swapchain configuration                         |
|  [14]   | `SurfaceCapabilities`                      | struct        | surface format and mode set                     |
|  [15]   | `ImageCopyTexture`                         | descriptor    | texture copy endpoint (`MipLevel`, `Origin3D`)  |
|  [16]   | `ImageCopyBuffer`                          | descriptor    | buffer copy endpoint (`Layout`, `Buffer`)       |
|  [17]   | `TextureDataLayout`                        | descriptor    | row pitch (`Offset`, `BytesPerRow`, `RowsPer…`) |
|  [18]   | `Extent3D`                                 | struct        | copy and allocation extent                      |
|  [19]   | `ChainedStruct` / `ChainedStructOut`       | struct        | input and output extension chains               |

[DISPATCH_CEILING]: `Limits.MaxComputeWorkgroupsPerDimension` (`uint`) `Limits.MaxStorageBufferBindingSize` (`ulong`) `Limits.MaxComputeInvocationsPerWorkgroup` (`uint`) — a compute gate reads these three; no timestamp-period member exists on any tier, and resolved `QueryType.Timestamp` values are nanoseconds directly.

[LIMIT_NEGOTIATION]: `RequiredLimits { NextInChain, Limits }` seats on `DeviceDescriptor.RequiredLimits` — an unset requirement grants the specification's conservative defaults, never the adapter's headroom, so a device wanting the hardware ceiling chains the adapter's own `SupportedLimits.Limits` back; the dispatch gate then reads the DEVICE's `DeviceGetLimits` block, because a device grants at or below what it was asked. Both `Bool32` answers refuse — a false answer leaves the out-struct zeroed and a zero ceiling refuses every dispatch.

[HANDLE_RELEASE]: every core handle releases through its own void pointer-taking `XxxRelease` — `InstanceRelease` `AdapterRelease` `DeviceRelease` `QueueRelease` beside the resource roster (`Buffer`/`Texture`/`Sampler`/`ShaderModule`/`BindGroupLayout`/`BindGroup`/`ComputePipeline`/`ComputePassEncoder`/`CommandEncoder`/`CommandBuffer`/`QuerySet`).

[PUBLIC_TYPE_SCOPE]: resource and pipeline descriptors

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `BufferDescriptor`            | descriptor    | buffer alloc (`Usage`, `Size`, `MappedAtCreation`)             |
|  [02]   | `TextureDescriptor`           | descriptor    | texture alloc (`Usage`, `Format`, `Size`, `MipLevelCount`)     |
|  [03]   | `TextureViewDescriptor`       | descriptor    | texture-view allocation                                        |
|  [04]   | `SamplerDescriptor`           | descriptor    | filter, address mode, LOD clamp, compare                       |
|  [05]   | `ShaderModuleDescriptor`      | descriptor    | shader module create (`NextInChain` source chain)              |
|  [06]   | `ShaderModuleWGSLDescriptor`  | descriptor    | WGSL source chain (`SType.ShaderModuleWgslDescriptor`)         |
|  [07]   | `ShaderModuleSPIRVDescriptor` | descriptor    | SPIR-V source chain (`Code`, `CodeSize`)                       |
|  [08]   | `BindGroupLayoutDescriptor`   | descriptor    | layout entries (`Entries`, `EntryCount`)                       |
|  [09]   | `BindGroupLayoutEntry`        | descriptor    | one binding slot (`Binding`, `Visibility`, `Buffer`/`Texture`) |
|  [10]   | `BufferBindingLayout`         | descriptor    | binding kind (`Type`, `HasDynamicOffset`, `MinBindingSize`)    |
|  [11]   | `BindGroupDescriptor`         | descriptor    | bound entries (`Layout`, `Entries`)                            |
|  [12]   | `BindGroupEntry`              | descriptor    | one bound resource (`Binding`, `Buffer`, `Offset`, `Size`)     |
|  [13]   | `PipelineLayoutDescriptor`    | descriptor    | layout set (`BindGroupLayouts`)                                |
|  [14]   | `RenderPipelineDescriptor`    | descriptor    | raster pipeline (vertex, primitive, fragment state)            |
|  [15]   | `ComputePipelineDescriptor`   | descriptor    | compute pipeline (`Layout`, `Compute` stage)                   |
|  [16]   | `ProgrammableStageDescriptor` | descriptor    | `Module`, `EntryPoint`, `ConstantCount`, `Constants`           |
|  [17]   | `ConstantEntry`               | descriptor    | one WGSL `override` constant (`Key`, `Value` double)           |
|  [18]   | `RenderPassDescriptor`        | descriptor    | color/depth attachments, timestamp writes                      |
|  [19]   | `ComputePassDescriptor`       | descriptor    | pass begin (`Label`, `TimestampWrites`)                        |
|  [20]   | `RenderPassTimestampWrites`   | descriptor    | render-pass timestamp slots                                    |
|  [21]   | `ComputePassTimestampWrites`  | descriptor    | `QuerySet`, beginning-of-pass and end-of-pass write indexes    |
|  [22]   | `QuerySetDescriptor`          | descriptor    | query-set alloc (`Type`, `Count`)                              |

[PUBLIC_TYPE_SCOPE]: enum vocabularies and callback delegate pointers

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `TextureFormat`             | enum          | pixel format, including the `*Float`/`*Uint` storage rows         |
|  [02]   | `TextureUsage`              | flags enum    | copy/binding/storage/attachment usage mask                        |
|  [03]   | `TextureDimension`          | enum          | 1D/2D/3D allocation shape                                         |
|  [04]   | `TextureViewDimension`      | enum          | 2D, 2D-array, cube, cube-array, 3D view shape                     |
|  [05]   | `BufferUsage`               | flags enum    | storage/uniform/copy/map/indirect/queryresolve                    |
|  [06]   | `BufferBindingType`         | enum          | `Uniform`/`Storage`/`ReadOnlyStorage` binding kind                |
|  [07]   | `BufferMapState`            | enum          | unmapped/pending/mapped poll                                      |
|  [08]   | `MapMode`                   | enum          | readback/upload map direction                                     |
|  [09]   | `BufferMapAsyncStatus`      | enum          | map-callback verdict                                              |
|  [10]   | `ShaderStage`               | flags enum    | vertex/fragment/compute visibility                                |
|  [11]   | `PresentMode`               | enum          | present mode                                                      |
|  [12]   | `BackendType`               | enum          | graphics backend                                                  |
|  [13]   | `PowerPreference`           | enum          | adapter power class for `RequestAdapterOptions`                   |
|  [14]   | `FeatureName`               | enum          | device feature, gating `TimestampQuery` and the BC/ASTC/ETC2 rows |
|  [15]   | `WGSLFeatureName`           | enum          | WGSL language feature                                             |
|  [16]   | `QueryType`                 | enum          | timestamp/occlusion query kind                                    |
|  [17]   | `ErrorFilter`               | enum          | `Validation`/`OutOfMemory`/`Internal` capture-scope selector      |
|  [18]   | `ErrorType`                 | enum          | NoError/Validation/OutOfMemory/Internal/Unknown/DeviceLost        |
|  [19]   | `PfnRequestAdapterCallback` | delegate ptr  | adapter result                                                    |
|  [20]   | `PfnRequestDeviceCallback`  | delegate ptr  | device result                                                     |
|  [21]   | `PfnBufferMapCallback`      | delegate ptr  | buffer-map result                                                 |
|  [22]   | `PfnQueueWorkDoneCallback`  | delegate ptr  | queue-work result                                                 |
|  [23]   | `PfnErrorCallback`          | delegate ptr  | validation error                                                  |
|  [24]   | `PfnDeviceLostCallback`     | delegate ptr  | device-lost result                                                |

[BUFFER_MAP_STATUS]: `Success` = 0 `ValidationError` `Unknown` `DeviceLost` `DestroyedBeforeCallback` `UnmappedBeforeCallback` `MappingAlreadyPending` `OffsetOutOfRange` `SizeOutOfRange`

## [02]-[ENTRYPOINTS]

Every surface below is an `unsafe` instance method on the `WebGPU.GetApi()` function-table root. Each descriptor-taking call ships paired overloads — a raw `<Descriptor>*` form and a `ref readonly <Descriptor>` `in`-reference form binding the same native call — and `<T0>(… ref T0)` generic overloads thread typed `unmanaged` userdata without a `void*` cast. `callback, userdata` abbreviates the member's own `Pfn…Callback` delegate beside its `void*` state slot.

[ENTRYPOINT_SCOPE]: instance, adapter, device, and queue acquisition

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------- |
|  [01]   | `WebGPU.GetApi()`                                                               | static   | API root load        |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                           | instance | instance create      |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, callback, userdata)` | instance | adapter request      |
|  [04]   | `AdapterGetProperties(Adapter*, AdapterProperties*)`                            | instance | adapter identity     |
|  [05]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, callback, userdata)`         | instance | device request       |
|  [06]   | `DeviceGetQueue(Device*)`                                                       | instance | queue handle         |
|  [07]   | `InstanceProcessEvents(Instance*)`                                              | instance | standard event pump  |
|  [08]   | `IsDeviceExtensionPresent(Device*, string) -> bool`                             | instance | wgpu-native probe    |
|  [09]   | `TryGetDeviceExtension<Wgpu>(Device*, out Wgpu) -> bool`                        | instance | extension table load |

[ENTRYPOINT_SCOPE]: feature and limit negotiation

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `AdapterEnumerateFeatures(Adapter*, FeatureName*) -> nuint` | instance | adapter feature list  |
|  [02]   | `AdapterHasFeature(Adapter*, FeatureName) -> Bool32`        | instance | adapter feature probe |
|  [03]   | `AdapterGetLimits(Adapter*, SupportedLimits*) -> Bool32`    | instance | adapter limits        |
|  [04]   | `DeviceEnumerateFeatures(Device*, FeatureName*) -> nuint`   | instance | device feature list   |
|  [05]   | `DeviceHasFeature(Device*, FeatureName) -> Bool32`          | instance | device feature probe  |
|  [06]   | `DeviceGetLimits(Device*, SupportedLimits*) -> Bool32`      | instance | device limits         |

[ENTRYPOINT_SCOPE]: surface, swapchain, and present

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]        |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `InstanceCreateSurface(Instance*, SurfaceDescriptor*)`             | instance | surface from window |
|  [02]   | `SurfaceGetCapabilities(Surface*, Adapter*, SurfaceCapabilities*)` | instance | format/mode query   |
|  [03]   | `SurfaceConfigure(Surface*, SurfaceConfiguration*)`                | instance | swapchain configure |
|  [04]   | `SurfaceGetCurrentTexture(Surface*, SurfaceTexture*)`              | instance | acquire frame image |
|  [05]   | `SurfacePresent(Surface*)`                                         | instance | present frame       |

[ENTRYPOINT_SCOPE]: resource, sampler, and pipeline creation

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :--------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                                            | instance | storage/staging alloc  |
|  [02]   | `DeviceCreateTexture(Device*, TextureDescriptor*)`                                          | instance | texture alloc          |
|  [03]   | `TextureCreateView(Texture*, TextureViewDescriptor*)`                                       | instance | texture view           |
|  [04]   | `DeviceCreateSampler(Device*, SamplerDescriptor*)`                                          | instance | sampler create         |
|  [05]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`                                | instance | shader compile         |
|  [06]   | `DeviceCreateBindGroupLayout(Device*, BindGroupLayoutDescriptor*)`                          | instance | binding layout         |
|  [07]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`                                      | instance | bound resource group   |
|  [08]   | `DeviceCreatePipelineLayout(Device*, PipelineLayoutDescriptor*)`                            | instance | pipeline layout        |
|  [09]   | `DeviceCreateRenderPipeline(Device*, RenderPipelineDescriptor*)`                            | instance | raster pipeline        |
|  [10]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)`                          | instance | compute pipeline       |
|  [11]   | `DeviceCreateComputePipelineAsync(Device*, ComputePipelineDescriptor*, callback, userdata)` | instance | async pipeline compile |
|  [12]   | `ComputePipelineGetBindGroupLayout(ComputePipeline*, uint) -> BindGroupLayout*`             | instance | auto-derive layout     |
|  [13]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`                            | instance | encoder create         |
|  [14]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                                        | instance | query-set allocation   |

[ENTRYPOINT_SCOPE]: pass recording and submission

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :-------------------- |
|  [01]   | `CommandEncoderBeginRenderPass(CommandEncoder*, RenderPassDescriptor*)`               | instance | render pass begin     |
|  [02]   | `RenderPassEncoderSetPipeline(RenderPassEncoder*, RenderPipeline*)`                   | instance | bind raster pipeline  |
|  [03]   | `RenderPassEncoderSetBindGroup(RenderPassEncoder*, uint, BindGroup*, nuint, uint*)`   | instance | bind resource group   |
|  [04]   | `RenderPassEncoderDraw(RenderPassEncoder*, uint, uint, uint, uint)`                   | instance | non-indexed draw      |
|  [05]   | `RenderPassEncoderDrawIndexed(RenderPassEncoder*, uint, uint, uint, int, uint)`       | instance | indexed draw          |
|  [06]   | `RenderPassEncoderEnd(RenderPassEncoder*)`                                            | instance | close render pass     |
|  [07]   | `CommandEncoderBeginComputePass(CommandEncoder*, ComputePassDescriptor*)`             | instance | compute pass begin    |
|  [08]   | `ComputePassEncoderSetPipeline(ComputePassEncoder*, ComputePipeline*)`                | instance | bind compute pipeline |
|  [09]   | `ComputePassEncoderSetBindGroup(ComputePassEncoder*, uint, BindGroup*, nuint, uint*)` | instance | bind resource group   |
|  [10]   | `ComputePassEncoderDispatchWorkgroups(ComputePassEncoder*, uint, uint, uint)`         | instance | workgroup dispatch    |
|  [11]   | `ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder*, Buffer*, ulong)`   | instance | indirect dispatch     |
|  [12]   | `ComputePassEncoderEnd(ComputePassEncoder*)`                                          | instance | close compute pass    |
|  [13]   | `CommandEncoderFinish(CommandEncoder*, CommandBufferDescriptor*)`                     | instance | finish command buffer |
|  [14]   | `QueueSubmit(Queue*, nuint, CommandBuffer**)`                                         | instance | submit to GPU         |

[ENTRYPOINT_SCOPE]: host transfer, texture copy, and readback

| [INDEX] | [SURFACE]                                                                                            | [SHAPE]  | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `QueueWriteBuffer(Queue*, Buffer*, ulong, void*, nuint)`                                             | instance | host-to-GPU upload    |
|  [02]   | `QueueWriteTexture(Queue*, ImageCopyTexture*, void*, nuint, TextureDataLayout*, Extent3D*)`          | instance | texture upload        |
|  [03]   | `CommandEncoderCopyBufferToBuffer(CommandEncoder*, Buffer*, ulong, Buffer*, ulong, ulong)`           | instance | device buffer copy    |
|  [04]   | `CommandEncoderCopyTextureToBuffer(CommandEncoder*, ImageCopyTexture*, ImageCopyBuffer*, Extent3D*)` | instance | texture to buffer     |
|  [05]   | `CommandEncoderCopyBufferToTexture(CommandEncoder*, ImageCopyBuffer*, ImageCopyTexture*, Extent3D*)` | instance | buffer to texture     |
|  [06]   | `BufferMapAsync(Buffer*, MapMode, nuint, nuint, callback, userdata)`                                 | instance | request mapping       |
|  [07]   | `BufferGetMappedRange(Buffer*, nuint, nuint) -> void*`                                               | instance | mapped read pointer   |
|  [08]   | `BufferGetMapState(Buffer*) -> BufferMapState`                                                       | instance | poll the map state    |
|  [09]   | `BufferUnmap(Buffer*)`                                                                               | instance | release mapping       |
|  [10]   | `QueueOnSubmittedWorkDone(Queue*, callback, userdata)`                                               | instance | submission completion |

[ENTRYPOINT_SCOPE]: timestamp queries and pass observability

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, uint)`                        | instance | per-pass timestamp     |
|  [02]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, uint, uint, Buffer*, ulong)` | instance | resolve to read buffer |
|  [03]   | `QuerySetGetCount(QuerySet*) -> uint`                                                   | instance | resolved query count   |
|  [04]   | `QuerySetGetType(QuerySet*) -> QueryType`                                               | instance | query-set kind         |
|  [05]   | `ComputePassEncoderSetLabel(ComputePassEncoder*, byte*)`                                | instance | pass label for capture |
|  [06]   | `ComputePassEncoderInsertDebugMarker(ComputePassEncoder*, byte*)`                       | instance | inline debug marker    |
|  [07]   | `ComputePassEncoderPushDebugGroup(ComputePassEncoder*, byte*)`                          | instance | open debug group       |
|  [08]   | `ComputePassEncoderPopDebugGroup(ComputePassEncoder*)`                                  | instance | close debug group      |
|  [09]   | `ComputePipelineSetLabel(ComputePipeline*, byte*)`                                      | instance | pipeline label         |

[ENTRYPOINT_SCOPE]: validation error scope

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]         |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `DeviceSetUncapturedErrorCallback(Device*, callback, userdata)` | instance | unscoped error sink  |
|  [02]   | `DevicePushErrorScope(Device*, ErrorFilter)`                    | instance | open a capture scope |
|  [03]   | `DevicePopErrorScope(Device*, callback, userdata)`              | instance | drain the open scope |

[ENTRYPOINT_SCOPE]: native-handle release

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]        |
| :-----: | :----------------------------------------------- | :------- | :------------------ |
|  [01]   | `BufferRelease(Buffer*)`                         | instance | buffer release      |
|  [02]   | `TextureDestroy(Texture*)`                       | instance | texture memory free |
|  [03]   | `TextureRelease(Texture*)`                       | instance | texture release     |
|  [04]   | `SamplerRelease(Sampler*)`                       | instance | sampler release     |
|  [05]   | `ShaderModuleRelease(ShaderModule*)`             | instance | shader release      |
|  [06]   | `BindGroupLayoutRelease(BindGroupLayout*)`       | instance | layout release      |
|  [07]   | `BindGroupRelease(BindGroup*)`                   | instance | group release       |
|  [08]   | `ComputePipelineRelease(ComputePipeline*)`       | instance | pipeline release    |
|  [09]   | `ComputePassEncoderRelease(ComputePassEncoder*)` | instance | pass release        |
|  [10]   | `CommandEncoderRelease(CommandEncoder*)`         | instance | encoder release     |
|  [11]   | `CommandBufferRelease(CommandBuffer*)`           | instance | commands release    |
|  [12]   | `QuerySetRelease(QuerySet*)`                     | instance | query-set release   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; a call site marshals `Span<T>`/`stackalloc` descriptor structs and passes raw pointers, never a managed wrapper object, since Silk.NET binds the C `webgpu.h` surface directly. Composers bind the `ref readonly` descriptor overload (no manual pin) and reserve the `byte*` form for hot WGSL and label paths they own as fixed buffers.
- Lifecycle threads `Instance` -> `Adapter` -> `Device` + `Queue` -> resources; the adapter and device requests complete through native callbacks Silk.NET surfaces as `PfnRequestAdapterCallback`/`PfnRequestDeviceCallback` delegate pointers, retired by `InstanceProcessEvents` or the extension `DevicePoll`.
- Surfaces are optional: `RequestAdapterOptions.CompatibleSurface` null requests a SURFACELESS adapter, so the same lifecycle yields a headless device with no window, no `SurfaceConfigure`, and no present — the bake plane renders into a `TextureUsage.RenderAttachment | CopySrc` texture or writes a `Storage` buffer and never acquires a `SurfaceTexture`.
- Presented planes chain a platform source into `SurfaceDescriptor.NextInChain` — `SurfaceDescriptorFromMetalLayer` (a `CAMetalLayer` from `Platforms.MacOS.NSWindow`), `…FromWindowsHWND`, `…FromXlibWindow`/`…FromWaylandSurface`/`…FromXcbWindow`, or `…FromAndroidNativeWindow`; `SurfaceConfigure` sets format, present mode, usage, and extent, and each frame `SurfaceGetCurrentTexture` yields a `SurfaceTexture` viewed, rendered through a `RenderPassEncoder`, and presented through `SurfacePresent`.
- `BackendType` auto-negotiates D3D12 (Windows), Metal (macOS), Vulkan (Linux), or the in-browser WebGPU target over the `wgpu_native` runtime; `AdapterEnumerateFeatures`/`AdapterHasFeature`/`AdapterGetLimits` negotiate the feature and limit set — `FeatureName.TimestampQuery` for timing, the BC/ASTC/ETC2 rows for compressed-texture sampling — before `AdapterRequestDevice` requires them in `DeviceDescriptor`.
- WGSL is the shader source for `DeviceCreateShaderModule` through the `ShaderModuleWGSLDescriptor` chain (SPIR-V through `ShaderModuleSPIRVDescriptor`); `ComputePipelineDescriptor.Compute` names the WGSL `EntryPoint` and a `ConstantEntry` `override`-constant block, so one kernel module serves many specializations without a recompile per literal.
- Binding admits both shapes: explicit `DeviceCreatePipelineLayout` over authored `BindGroupLayout`s, or auto layout (null `Layout`, then `ComputePipelineGetBindGroupLayout` reads what the WGSL `@group` declarations imply). `BufferDescriptor.Usage` maps role to flags — `Storage` read/write, `Uniform` constant blocks, `CopySrc`/`CopyDst` device copy, `MapRead`/`MapWrite` host mapping, `Indirect` for `DispatchWorkgroupsIndirect`, `QueryResolve` for resolved timestamps.
- Readback is two-phase and identical on every plane: a device-side copy (`CommandEncoderCopyBufferToBuffer` for a compute result, `CommandEncoderCopyTextureToBuffer` for a rendered or storage-written texture) lands the bytes in a `MapRead | CopyDst` staging buffer, then `BufferMapAsync(MapMode.Read)` requests the map, `BufferGetMapState` polls `BufferMapState.Mapped`, `BufferGetMappedRange` yields the host pointer, and `BufferUnmap` releases it.
- `CommandEncoderCopyTextureToBuffer` binds `ImageCopyBuffer.Layout.BytesPerRow` to a 256-byte alignment the caller pads; a plane whose natural row pitch is unaligned reads back into a padded staging buffer and un-pads per row on the host, never a direct span cast over the mapped range.
- Timestamp timing folds through the query surface: `DeviceCreateQuerySet` mints a `QueryType.Timestamp` set, `CommandEncoderWriteTimestamp` or the `RenderPassTimestampWrites`/`ComputePassTimestampWrites` begin-descriptor entries record GPU wall time at pass boundaries, and `CommandEncoderResolveQuerySet` copies the nanosecond ticks into a `QueryResolve | CopySrc` buffer.
- Validation rides the error-scope path: `DeviceSetUncapturedErrorCallback` installs the global `PfnErrorCallback` and `DevicePushErrorScope`/`DevicePopErrorScope` bracket a suspect operation, so a validation or out-of-memory error is a counted fault row on the composing folder's telemetry spine, never a swallowed native abort.

[STACKING]:
- `api-silk-webgpu-wgpu`(`.api/api-silk-webgpu-wgpu.md`): `TryGetDeviceExtension<Wgpu>` gates on `IsDeviceExtensionPresent` and loads `Silk.NET.WebGPU.Extensions.WGPU` as a second function-table view over the one `wgpu_native` runtime — `DevicePoll` retires the `BufferMapAsync` callback that `InstanceProcessEvents` otherwise pumps blocking (the only non-blocking readback advance on a surfaceless device, which has no present to pump the loop), `QueueSubmitForIndex` returns the `WrappedSubmissionIndex` `DevicePoll(wait: true)` drains deterministically, `SetLogCallback` routes the native adapter and device-lost stream into typed diagnostics beside the error-scope path, `InstanceEnumerateAdapters` supplies the compositor-LUID adapter match, and `RenderPassEncoderMultiDraw*`/`SetPushConstants` and pipeline-statistics queries extend the meshlet passes.
- `api-bcl-interop`(`.api/api-bcl-interop.md`): `ShaderModuleWGSLDescriptor.Code` and `ProgrammableStageDescriptor.EntryPoint` bind a native UTF-8 `byte*`, so `Marshal.StringToCoTaskMemUTF8` mints the WGSL source and entry-point block, `Marshal.FreeCoTaskMem` retires both in the compile fold's `finally`, and `Marshal.PtrToStringUTF8` drains the `PfnErrorCallback` message — one interop owner marshals both directions across this boundary.
- `api-highperformance`(`.api/api-highperformance.md`): `BufferGetMappedRange` yields a `void*` the readback fold wraps as `Span2D<T>` over the padded row pitch, so an unaligned `BytesPerRow` un-pads as a strided 2D view and copies row-wise into a `MemoryOwner<T>` plane arena with no intermediate `byte[]`.
- `api-avalonia-gpu-interop`(`Rasm.AppUi/.api/api-avalonia-gpu-interop.md`): the rendered wgpu `Texture` exports as the platform handle `ICompositionGpuInterop.ImportImage` binds into a `CompositionDrawingSurface`, the export-format synchronization primitive selecting the `UpdateWith*Async` refresh, never a second swapchain composited beside Avalonia's.
- `api-onnxruntime`(`Rasm.Compute/.api/api-onnxruntime.md`): a WGPU storage `Buffer` and the model-lane ORT device value resolve ONE residency — the buffer's mapped pointer admits to an `OrtValue` through `OrtValue.CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` and an ORT device output binds back through `OrtIoBinding.BindOutputToDevice(string, OrtMemoryInfo)`, so the compute kernel and ONNX device inference share one physical allocation with no host round-trip.
- within-lib: three composing folders hold three device dispositions over one binding — `Rasm.AppUi` mints the presented device and owns its lifecycle capsule, `Rasm.Compute` binds that same device for GPGPU dispatch and mints none, and `Rasm.Materials/Raster/gpu#PRESS_DEVICE` acquires its own SURFACELESS bake device (a null `CompatibleSurface`) because a bake runs with no viewport open, confining the raw function table to that one internal boundary and dispatching a closed `WgslKernel` module table through it. Each folder folds every native-handle create-and-release pair in a scoped fold and projects validation faults and resolved timestamp durations onto its own telemetry spine.

[LOCAL_ADMISSION]:
- Every native handle (`Instance`, `Adapter`, `Device`, `Queue`, `Surface`, `Buffer`, `Texture`, `Sampler`, pipelines, encoders, query sets) releases through its matching `XxxRelease`/`XxxDestroy` native call rather than `IDisposable`, the owning boundary capsule pairing create-and-release in a `using`-equivalent scoped fold. `TextureDestroy` frees the backing allocation ahead of the handle release the reference count drives.
- `Silk.NET.WebGPU.Native.WGPU` supplies the one native `wgpu_native` asset this binding P/Invokes across the win/linux/osx RIDs; the device-tick and every other wgpu-native entrypoint lives in `Silk.NET.WebGPU.Extensions.WGPU`, never in the native-asset package.
- Silk.NET 2.x is maintenance-mode and 3.x reshapes the binding, so each composing folder confines the raw table to ONE narrow internal boundary — the swap-point is that boundary, never a call site spread across pages.
- Device ownership is single-writer per plane: a folder composing another folder's device never issues `AdapterRequestDevice`, and a folder needing a device where no viewport exists acquires a surfaceless one rather than forcing a window open.
- Buffers mint through `DeviceCreateBuffer` alone: the binding exposes no external-memory import, adopt, or shared-handle entrypoint, so a foreign device allocation reaches a `Buffer*` only by host copy through `QueueWriteBuffer`.
