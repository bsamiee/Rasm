# [APPUI]

`Rasm.AppUi` is the APP-PLATFORM product-UI engine: one Avalonia shell that mounts onto any admitted substrate — Rhino panel and modal, GH2 companion window, standalone desktop, sidecar shell, headless proof surface — through one abstract `SurfaceHost` axis. It consumes AppHost ports, Persistence queries, and Compute receipts as settled vocabulary and never references Rhino or GH directly. The domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[NAVIGATION](.planning/Shell/navigation.md)
- [02]-[SCREENS](.planning/Shell/screens.md)
- [03]-[HOSTS](.planning/Shell/hosts.md)
- [04]-[COMMANDS](.planning/Shell/commands.md)
- [05]-[DIALOGS](.planning/Shell/dialogs.md)
- [06]-[INPUT](.planning/Shell/input.md)
- [07]-[ACCESSIBILITY](.planning/Shell/accessibility.md)
- [08]-[VIEWPORT](.planning/Render/viewport.md)
- [09]-[CAPTURE](.planning/Render/capture.md)
- [10]-[DRAFTING](.planning/Render/drafting.md)
- [11]-[REALITY](.planning/Render/reality.md)
- [12]-[EVIDENCE](.planning/Render/evidence.md)
- [13]-[DASHBOARDS](.planning/Charts/dashboards.md)
- [14]-[CUSTOM](.planning/Charts/custom.md)
- [15]-[INSPECTOR](.planning/Editing/inspector.md)
- [16]-[TABLES](.planning/Editing/tables.md)
- [17]-[NOTEBOOK](.planning/Editing/notebook.md)
- [18]-[LIVEDATA](.planning/Editing/livedata.md)
- [19]-[ISSUES](.planning/Editing/issues.md)
- [20]-[TOUR](.planning/Editing/tour.md)
- [21]-[TOKENS](.planning/Theme/tokens.md)
- [22]-[TYPOGRAPHY](.planning/Theme/typography.md)
- [23]-[MOTION](.planning/Theme/motion.md)
- [24]-[ANIMATION](.planning/Theme/animation.md)
- [25]-[ASSETS](.planning/Theme/assets.md)
- [26]-[LOCALE](.planning/Theme/locale.md)

## [02]-[DOMAIN_PACKAGES]

Domain libraries that implement UI framework, rendering, GPU backends, charts, content, and CAD exchange — entirely distinct from the C# substrate registry.

[UI_FRAMEWORK]:
- `Avalonia`
- `Avalonia.Desktop`
- `Avalonia.Headless`
- `Avalonia.Themes.Fluent`
- `Avalonia.Fonts.Inter`
- `Avalonia.Controls.DataGrid`
- `Avalonia.Controls.ColorPicker`
- `Avalonia.AvaloniaEdit`
- `AvaloniaEdit.TextMate`
- `Avalonia.Markup.Xaml.Loader`

[REACTIVE]:
- `ReactiveUI`
- `ReactiveUI.Avalonia`
- `ReactiveUI.Validation`
- `Xaml.Behaviors.Avalonia`
- `System.Reactive`
- `DynamicData`

[DOCKING_DIALOGS]:
- `Dock.Avalonia`
- `Dock.Model.ReactiveUI`
- `Dock.Serializer.SystemTextJson`
- `DialogHost.Avalonia`

[INSPECTION_CHARTING]:
- `bodong.Avalonia.PropertyGrid`
- `bodong.PropertyModels`
- `LiveChartsCore.SkiaSharpView.Avalonia`

[RENDER_TEXT]:
- `Avalonia.Skia`
- `SkiaSharp`
- `SkiaSharp.HarfBuzz`
- `SkiaSharp.NativeAssets.macOS`
- `SkiaSharp.NativeAssets.Linux`
- `SkiaSharp.NativeAssets.Linux.NoDependencies`
- `HarfBuzzSharp.NativeAssets.macOS`
- `HarfBuzzSharp.NativeAssets.Linux`
- `Svg.Controls.Skia.Avalonia`
- `Svg.Skia`

[GPU_BACKEND]:
- `Silk.NET.WebGPU`
- `Silk.NET.WebGPU.Native.WGPU`
- `Silk.NET.WebGPU.Extensions.WGPU`
- `Silk.NET.OpenXR`
- `Silk.NET.OpenXR.Extensions.KHR`
- `Silk.NET.OpenXR.Extensions.EXT`
- `Silk.NET.OpenXR.Extensions.FB`

> [!NOTE]
> The `Wgpu` GPU family is owned by the .NET Foundation `Silk.NET.WebGPU` binding (stable, MIT) over the bundled `Silk.NET.WebGPU.Native.WGPU` wgpu/Dawn runtime, presenting into the Avalonia 12 compositor through `ICompositionGpuInterop` texture import (`.api/api-avalonia-gpu-interop.md`). `Silk.NET.WebGPU.Extensions.WGPU` adds the wgpu-native vendor surface the standard `webgpu.h` binding omits — non-blocking `DevicePoll`, native log routing, full-adapter enumeration for the compositor-LUID match, and indirect multi-draw for GPU-driven meshlet rendering (`.api/api-silk-webgpu-wgpu.md`). `Silk.NET.OpenXR` owns the immersive design-review surface — the stereo swapchain, the predicted-display-time frame loop, and the action-set controller model share the one `Wgpu` device through the OpenXR graphics binding, folding to the flat viewport where the host OpenXR loader is absent (`.api/api-silk-openxr.md`); `Silk.NET.OpenXR.Extensions.FB` adds the `XR_FB_passthrough` environment-blend layer the on-site mixed-reality review composites under the rendered scene against the one session (`.api/api-silk-openxr-fb.md`), riding the same `2.23.0` line as the core families since Silk.NET publishes its whole core-plus-extension set from one monorepo release. `Avalonia.Skia` (Ganesh) is the shippable `Software`/`Metal`/`Vulkan`/`OpenGl` floor behind the `SurfaceHost` render seam. The archived `VelloSharp`/`VelloSharp.Avalonia.Vello` rows are fully retired — no Vello identity survives in the manifest, this registry, or `.api/`; SkiaSharp Graphite carries no pinnable identity yet (targeted `4.150.0-preview.2`, unshipped), so no Graphite row is admitted until it ships.

[ASSETS_CONTENT]:
- `AsyncImageLoader.Avalonia`
- `FluentIcons.Avalonia`
- `FluentIcons.Common`
- `HotAvalonia`
- `Markdig`
- `PanAndZoom`
- `Wacton.Unicolour`

[DOMAIN_RUNTIME]:
- `UnitsNet`

[DRAFTING_EXPORT]:
- `ACadSharp`
- `netDxf`
- `DocumentFormat.OpenXml`

## [03]-[SUBSTRATE_PACKAGES]

Substrate libraries from the C# registry that this folder consumes directly. Full substrate API evidence is in `.api/` and the registry rationale is in [`libs/csharp/.planning/README.md`](../.planning/README.md).

[FUNCTIONAL_CORE]:
- `Thinktecture.Runtime.Extensions.Json`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
