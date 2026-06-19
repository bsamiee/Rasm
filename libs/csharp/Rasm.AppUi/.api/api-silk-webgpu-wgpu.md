# [RASM_APPUI_API_SILK_WEBGPU_WGPU]

`Silk.NET.WebGPU.Extensions.WGPU` is the wgpu-native vendor-extension binding layered over the canonical `Silk.NET.WebGPU` core: the upstream `webgpu.h` standard surface omits the non-blocking submission-completion poll and the native log/error callbacks a desktop render loop demands, and `wgpu_native` (the runtime `Silk.NET.WebGPU.Native.WGPU` carries) adds them through `wgpu.h`. The extension's `Wgpu` class is the generated function-table root for those native-only entrypoints — `DevicePoll` advances the wgpu device queue without a blocking fence, `SetLogCallback`/`SetLogLevel` route the native diagnostic stream into the receipt sink, and the instance-enumerate-adapters and surface-capability native calls complete the adapter/format negotiation the standard `RequestAdapter` callback alone does not expose. The extension is loaded against the same `WebGPU.GetApi()` instance the `api-silk-webgpu.md` core owns, so the `Wgpu` `GpuBackend` render loop polls and logs through this surface without a second binding.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU.Extensions.WGPU`
- package: `Silk.NET.WebGPU.Extensions.WGPU`
- assembly: `Silk.NET.WebGPU.Extensions.WGPU`
- namespace: `Silk.NET.WebGPU.Extensions.WGPU`
- asset: managed binding over the `wgpu_native` runtime `Silk.NET.WebGPU.Native.WGPU` already carries
- rail: viewport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension root and native-only carriers
- rail: viewport

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                               |
| :-----: | :--------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Wgpu`                       | extension root | wgpu-native function table           |
|   [2]   | `NativeSType`                | enum           | wgpu-native `next`-chain struct type |
|   [3]   | `LogLevel`                   | enum           | off/error/warn/info/debug/trace      |
|   [4]   | `LogCallback`                | delegate       | native log-message sink              |
|   [5]   | `InstanceBackend`            | flags enum     | vulkan/metal/d3d12/gl backend mask   |
|   [6]   | `InstanceExtras`             | struct         | instance `next` backend selection    |
|   [7]   | `DeviceExtras`               | struct         | device `next` trace path             |
|   [8]   | `RequiredLimitsExtras`       | struct         | wgpu-native limit extensions         |
|   [9]   | `SurfaceConfigurationExtras` | struct         | desired maximum frame latency        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: device poll, log, and native adapter enumeration
- rail: viewport

| [INDEX] | [SURFACE]                                                                           | [SURFACE_ROOT] | [RAIL]                  |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :---------------------- |
|   [1]   | `Wgpu.GetApi(WebGPU)`                                                               | `Wgpu`         | extension load          |
|   [2]   | `DevicePoll(Device*, wait, WrappedSubmissionIndex*)`                                | `Wgpu`         | non-blocking queue poll |
|   [3]   | `SetLogCallback(LogCallback, userdata)`                                             | `Wgpu`         | native log sink         |
|   [4]   | `SetLogLevel(LogLevel)`                                                             | `Wgpu`         | log verbosity           |
|   [5]   | `InstanceEnumerateAdapters(Instance*, InstanceEnumerateAdapterOptions*, Adapter**)` | `Wgpu`         | enumerate all adapters  |
|   [6]   | `GetVersion()`                                                                      | `Wgpu`         | wgpu-native version     |

[ENTRYPOINT_SCOPE]: native resource introspection and submission index
- rail: viewport

| [INDEX] | [SURFACE]                                                                               | [SURFACE_ROOT] | [RAIL]                      |
| :-----: | :-------------------------------------------------------------------------------------- | :------------- | :-------------------------- |
|   [1]   | `QueueSubmitForIndex(Queue*, count, CommandBuffer**)`                                   | `Wgpu`         | submit + return index       |
|   [2]   | `RenderPassEncoderSetPushConstants(RenderPassEncoder*, stages, offset, size, data*)`    | `Wgpu`         | push-constant set           |
|   [3]   | `RenderPassEncoderMultiDrawIndirect(RenderPassEncoder*, buffer*, offset, count)`        | `Wgpu`         | multi-draw indirect         |
|   [4]   | `RenderPassEncoderMultiDrawIndexedIndirect(RenderPassEncoder*, buffer*, offset, count)` | `Wgpu`         | indexed multi-draw indirect |
|   [5]   | `BufferDestroy(Buffer*)` / `TextureDestroy(Texture*)`                                   | `Wgpu`         | explicit native destroy     |

## [4]-[IMPLEMENTATION_LAW]

[WGPU_EXTENSION_TOPOLOGY]:
- `Wgpu.GetApi(webgpu)` loads the wgpu-native vendor function table against the existing `WebGPU.GetApi()` instance — the extension is a second function-table view over the one loaded `wgpu_native` runtime, never a second binding; a call site holds one `WebGPU` core and one `Wgpu` extension.
- `DevicePoll(device, wait: false, submissionIndex)` is the non-blocking submission-completion advance the standard `webgpu.h` lacks: the desktop render loop polls the wgpu queue once per frame to retire completed command buffers and run mapped-buffer callbacks without blocking the UI thread on a fence — `wait: true` is the blocking drain the teardown capsule uses, and a busy-spin on a standard fence is the rejected form.
- `SetLogCallback`/`SetLogLevel` route the native wgpu diagnostic stream (adapter selection, validation errors, device-lost) into the AppUi receipt sink as evidence rows, so a wgpu validation error is a counted `ViewportFault` on the telemetry spine rather than a swallowed native print.
- `InstanceEnumerateAdapters` enumerates every physical adapter (not just the power-preference single adapter the standard `RequestAdapter` returns), so the compositor-adapter-LUID/UUID match the `ICompositionGpuInterop` seam demands selects the exact adapter the Avalonia compositor renders on — a power-preference single adapter ignoring the compositor LUID is the cross-adapter copy penalty.
- `MultiDrawIndirect`/`MultiDrawIndexedIndirect` and `SetPushConstants` are the GPU-driven-rendering native entrypoints the meshlet cluster draw uses to issue one indirect multi-draw over the visible meshlet set rather than N per-meshlet draws, and `QueueSubmitForIndex` returns the submission index `DevicePoll` waits on — the GPU-driven culling and bindless residency the `GEOMETRY_VIRTUAL` cluster owns bind to these wgpu-native indirect draws.

[LOCAL_ADMISSION]:
- The extension carries no native runtime of its own — it P/Invokes the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` already restores, so admitting the extension adds the vendor function table at zero additional native-asset cost.
- The `InstanceExtras`/`DeviceExtras` native `next`-chain structs select the wgpu backend (`InstanceBackend.Vulkan`/`Metal`/`D3D12`) and the device trace path at create, threaded through the standard `InstanceDescriptor.NextInChain`/`DeviceDescriptor.NextInChain` pointer — a parallel non-standard create path is the rejected form, the extras ride the standard descriptor `next` chain.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU.Extensions.WGPU`
- Owns: the wgpu-native vendor surface over the standard binding — non-blocking `DevicePoll` queue advance, native log callback routing, full-adapter enumeration for the compositor-LUID match, indirect multi-draw and push-constants for GPU-driven meshlet rendering, and the submission-index handshake the poll waits on.
- Accept: `Wgpu.GetApi(webgpu)` second function-table view over the one core; `DevicePoll` per-frame non-blocking advance; native log into the receipt sink; `InstanceEnumerateAdapters` LUID-matched adapter select; the `Extras` native `next`-chain on the standard descriptors.
- Reject: a second wgpu binding beside the core `WebGPU`; a blocking fence busy-spin where `DevicePoll` advances; a power-preference single-adapter select ignoring the compositor LUID; N per-meshlet draws where one indirect multi-draw issues.
