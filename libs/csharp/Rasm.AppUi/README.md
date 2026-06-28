# [APPUI]

`Rasm.AppUi` is the APP-PLATFORM product-UI engine: one Avalonia shell that mounts onto any admitted substrate — Rhino panel and modal, GH2 companion window, standalone desktop, sidecar shell, headless proof surface — through one abstract `SurfaceHost` axis. It consumes AppHost ports, Persistence queries, and Compute receipts as settled vocabulary and never references Rhino or GH directly. The domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[NAVIGATION](.planning/Shell/navigation.md)
- [02]-[SCREENS](.planning/Shell/screens.md)
- [03]-[HOSTS](.planning/Shell/hosts.md)
- [04]-[COMMANDS](.planning/Shell/commands.md)
- [05]-[CONTROLS](.planning/Shell/controls.md)
- [06]-[SOLVER](.planning/Shell/solver.md)
- [07]-[VIRTUALIZATION](.planning/Shell/virtualization.md)
- [08]-[DIALOGS](.planning/Shell/dialogs.md)
- [09]-[INPUT](.planning/Shell/input.md)
- [10]-[ACCESSIBILITY](.planning/Shell/accessibility.md)
- [11]-[PIPELINE](.planning/Render/pipeline.md)
- [12]-[MESHLETS](.planning/Render/meshlets.md)
- [13]-[PATHTRACE](.planning/Render/pathtrace.md)
- [14]-[CAPTURE](.planning/Render/capture.md)
- [15]-[DRAFTING](.planning/Render/drafting.md)
- [16]-[REALITY](.planning/Render/reality.md)
- [17]-[EVIDENCE](.planning/Render/evidence.md)
- [18]-[SHADING](.planning/Render/shading.md)
- [19]-[IMMERSIVE](.planning/Render/immersive.md)
- [20]-[DASHBOARDS](.planning/Charts/dashboards.md)
- [21]-[CUSTOM](.planning/Charts/custom.md)
- [22]-[INSPECTOR](.planning/Editing/inspector.md)
- [23]-[TABLES](.planning/Editing/tables.md)
- [24]-[NOTEBOOK](.planning/Editing/notebook.md)
- [25]-[LIVEDATA](.planning/Editing/livedata.md)
- [26]-[FORMS](.planning/Editing/forms.md)
- [27]-[HISTORY](.planning/Editing/history.md)
- [28]-[MEDIA](.planning/Editing/media.md)
- [29]-[ISSUES](.planning/Editing/issues.md)
- [30]-[TOUR](.planning/Editing/tour.md)
- [31]-[TOKENS](.planning/Theme/tokens.md)
- [32]-[TYPOGRAPHY](.planning/Theme/typography.md)
- [33]-[MOTION](.planning/Theme/motion.md)
- [34]-[ANIMATION](.planning/Theme/animation.md)
- [35]-[ASSETS](.planning/Theme/assets.md)
- [36]-[LOCALE](.planning/Theme/locale.md)

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

[LAYOUT]:
- `Kiwi`

[MEDIA]:
- `HanumanInstitute.LibMpv`
- `HanumanInstitute.LibMpv.Avalonia`

[INPUT_FABRIC]:
- `HidSharp`
- `Silk.NET.Input`
- `Silk.NET.SDL`
- `Melanchall.DryWetMidi`

[DRAFTING_EXPORT]:
- `ACadSharp`
- `netDxf`
- `DocumentFormat.OpenXml`
- `PDFsharp`
- `PDFsharp-MigraDoc`

[THEME_SUITE]:
- `Semi.Avalonia`
- `Semi.Avalonia.DataGrid`
- `Semi.Avalonia.ColorPicker`
- `Semi.Avalonia.Dock`
- `Semi.Avalonia.AvaloniaEdit`

[EXTENDED_CONTROLS]:
- `Irihi.Ursa`
- `Irihi.Ursa.Themes.Semi`
- `Irihi.Ursa.ReactiveUIExtension`

[GRAPH_MAP_SURFACES]:
- `NodeEditorAvalonia`
- `Mapsui.Avalonia12`

[COLLABORATION_LOCALE]:
- `LoroCs`
- `MessageFormat`

> [!NOTE]
> The `Semi.Avalonia` design-token theme is the active layer over the retained `Avalonia.Themes.Fluent` floor — its base `SemiTheme` plus the `DataGrid`/`ColorPicker`/`Dock`/`AvaloniaEdit` skins restyle the admitted control roster to one token system the `Wacton.Unicolour` OKLCH pipeline materializes into the `ControlIntent` + `Theme/tokens` vocabulary, never displacing the Fluent-templated `bodong.PropertyGrid`/`DialogHost`. `Irihi.Ursa` adds the extended-control families the curated set lacks — `NavMenu`, `Timeline`, `Toast`/`Notification`, `Loading`/`Skeleton`, `Banner`, `Form`, `Drawer` — themed by `Irihi.Ursa.Themes.Semi` and bridged to the admitted ReactiveUI MVVM rail through `Irihi.Ursa.ReactiveUIExtension`. `NodeEditorAvalonia` owns the node/pin/connector graph-editing canvas inside a `PanAndZoom` `ZoomBorder` for the `Shell/Editing` parametric and dependency-graph surfaces (ReactiveUI view-models over the framework-agnostic `INode`/`IConnector` model). `Mapsui.Avalonia12` is the interactive slippy-map / basemap viewport rendering through the admitted `SkiaSharp` + `Avalonia.Skia` and binding the Bim-owned `NetTopologySuite` so GDAL/OGR features draw as overlays beside the `Wgpu` 3D viewport. `LoroCs` is the Eg-walker/Fugue sequence+map+text+movable-list+tree CRDT engine backing the notebook/annotation/table collaboration op-log and presence (`runtimes/osx-arm64/native/loro.dylib`), retiring the bespoke `NotebookCrdt` LWW algebra; `MessageFormat` is the managed ICU MessageFormat engine (CLDR cardinal+ordinal `plural`/`selectordinal`/`select`) materializing `ResolvedLocale.Plural` over the resx pattern vocabulary. `PDFsharp` + `PDFsharp-MigraDoc` add the structured vector-PDF page model and the auto-paginated flow-report DOM the OOXML/DXF/raster export set lacked.

## [03]-[SUBSTRATE_PACKAGES]

Substrate libraries from the C# registry that this folder consumes directly. Full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../.planning/README.md), with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
