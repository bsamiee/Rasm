# [APPUI]

`Rasm.AppUi` is the APP-PLATFORM product-UI engine: one Avalonia shell that mounts host-neutrally onto any admitted substrate — Rhino panel and modal, GH2 companion window, standalone desktop, sidecar shell, headless proof surface — through one abstract `SurfaceHost` axis, consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary and never referencing Rhino or GH directly. This README routes the `.planning/` design pages and registers every external package the folder draws on; the domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- hosts: [surface-hosts](.planning/hosts/surface-hosts.md)
- shell: [shell-navigation](.planning/shell/shell-navigation.md)
- screens: [screens-activation](.planning/screens/screens-activation.md)
- commands: [commands-availability](.planning/commands/commands-availability.md)
- livedata: [live-data](.planning/livedata/live-data.md)
- tables: [tables-hierarchy](.planning/tables/tables-hierarchy.md)
- inspector: [inspector-editing](.planning/inspector/inspector-editing.md)
- charts: [charts-dashboards](.planning/charts/charts-dashboards.md), [custom-visuals](.planning/charts/custom-visuals.md)
- visuals: [visuals-offscreen](.planning/visuals/visuals-offscreen.md)
- theme: [theme-tokens](.planning/theme/theme-tokens.md)
- typography: [typography-shaping](.planning/typography/typography-shaping.md)
- assets: [icons-assets](.planning/assets/icons-assets.md)
- dialogs: [dialogs-notifications](.planning/dialogs/dialogs-notifications.md)
- input: [input-interaction](.planning/input/input-interaction.md)
- motion: [motion-tokens](.planning/motion/motion-tokens.md)
- access: [accessibility](.planning/access/accessibility.md)
- localization: [localization-culture](.planning/localization/localization-culture.md)
- evidence: [diagnostics-evidence](.planning/evidence/diagnostics-evidence.md)
- viewport: [viewport-pipeline](.planning/viewport/viewport-pipeline.md)
- shading: [shader-pipeline](.planning/shading/shader-pipeline.md)
- immersive: [xr-review](.planning/immersive/xr-review.md)
- presentation: [review-tour](.planning/presentation/review-tour.md)
- drafting: [drafting-sheets](.planning/drafting/drafting-sheets.md)
- notebook: [notebook-document](.planning/notebook/notebook-document.md)
- animation: [animation-timeline](.planning/animation/animation-timeline.md)
- realitycapture: [reality-capture](.planning/realitycapture/reality-capture.md)
- coordination: [issue-board](.planning/coordination/issue-board.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as one flat registry grouped by domain concern. Versions are centralized in the one C# manifest and never pinned here.

[AVALONIA]:
- Avalonia
- Avalonia.Desktop
- Avalonia.Skia
- Avalonia.Headless
- Avalonia.Headless.XUnit
- Avalonia.Themes.Fluent
- Avalonia.Fonts.Inter
- Avalonia.Controls.DataGrid
- Avalonia.Controls.ColorPicker
- Avalonia.AvaloniaEdit
- AvaloniaEdit.TextMate

[REACTIVE]:
- ReactiveUI
- ReactiveUI.Avalonia
- ReactiveUI.Validation
- Xaml.Behaviors.Avalonia
- System.Reactive
- DynamicData

[DOCKING_DIALOGS]:
- Dock.Avalonia
- Dock.Model.ReactiveUI
- Dock.Serializer.SystemTextJson
- DialogHost.Avalonia

[INSPECTION_CHARTING]:
- bodong.Avalonia.PropertyGrid
- bodong.PropertyModels
- LiveChartsCore.SkiaSharpView.Avalonia

[RENDER_TEXT]:
- SkiaSharp
- SkiaSharp.HarfBuzz
- SkiaSharp.NativeAssets.macOS
- SkiaSharp.NativeAssets.Linux
- SkiaSharp.NativeAssets.Linux.NoDependencies
- HarfBuzzSharp.NativeAssets.macOS
- HarfBuzzSharp.NativeAssets.Linux
- Svg.Controls.Skia.Avalonia
- Svg.Skia

[GPU_BACKEND]:
- Silk.NET.WebGPU
- Silk.NET.WebGPU.Native.WGPU
- Silk.NET.WebGPU.Extensions.WGPU
- Silk.NET.OpenXR
- Silk.NET.OpenXR.Extensions.KHR
- Silk.NET.OpenXR.Extensions.EXT
- Silk.NET.OpenXR.Extensions.FB

> [!NOTE]
> The `Wgpu` GPU family is owned by the .NET Foundation `Silk.NET.WebGPU` binding (stable, MIT) over the bundled `Silk.NET.WebGPU.Native.WGPU` wgpu/Dawn runtime, presenting into the Avalonia 12 compositor through `ICompositionGpuInterop` texture import (`.api/api-avalonia-gpu-interop.md`). `Silk.NET.WebGPU.Extensions.WGPU` adds the wgpu-native vendor surface the standard `webgpu.h` binding omits — non-blocking `DevicePoll`, native log routing, full-adapter enumeration for the compositor-LUID match, and indirect multi-draw for GPU-driven meshlet rendering (`.api/api-silk-webgpu-wgpu.md`). `Silk.NET.OpenXR` owns the immersive design-review surface — the stereo swapchain, the predicted-display-time frame loop, and the action-set controller model share the one `Wgpu` device through the OpenXR graphics binding, folding to the flat viewport where the host OpenXR loader is absent (`.api/api-silk-openxr.md`); `Silk.NET.OpenXR.Extensions.FB` adds the `XR_FB_passthrough` environment-blend layer the on-site mixed-reality review composites under the rendered scene against the one session (`.api/api-silk-openxr-fb.md`), riding the same `2.23.0` line as the core families since Silk.NET publishes its whole core-plus-extension set from one monorepo release. `Avalonia.Skia` (Ganesh) is the shippable `Software`/`Metal`/`Vulkan`/`OpenGl` floor behind the `SurfaceHost` render seam. The archived `VelloSharp`/`VelloSharp.Avalonia.Vello` rows are fully retired — no Vello identity survives in the manifest, this registry, or `.api/`; SkiaSharp Graphite carries no pinnable identity yet (targeted `4.150.0-preview.2`, unshipped), so no Graphite row is admitted until it ships.

[ASSETS_CONTENT]:
- AsyncImageLoader.Avalonia
- FluentIcons.Avalonia
- FluentIcons.Common
- Markdig
- PanAndZoom
- Wacton.Unicolour

[DOMAIN_RUNTIME]:
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- LanguageExt.Core
- NodaTime
- UnitsNet
- System.IO.Hashing

[DRAFTING_EXPORT]:
- ACadSharp
- netDxf
- DocumentFormat.OpenXml

[DEV_TEST]:
- HotAvalonia
- Avalonia.Markup.Xaml.Loader
- Verify.XunitV3
