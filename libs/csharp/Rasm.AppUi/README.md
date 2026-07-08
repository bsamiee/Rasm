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
- [14]-[SHADING](.planning/Render/shading.md)
- [15]-[IMMERSIVE](.planning/Render/immersive.md)
- [16]-[REALITY](.planning/Render/reality.md)
- [17]-[CAPTURE](.planning/Render/capture.md)
- [18]-[DRAFTING](.planning/Render/drafting.md)
- [19]-[ANIMATION](.planning/Render/animation.md)
- [20]-[DASHBOARDS](.planning/Charts/dashboards.md)
- [21]-[CUSTOM](.planning/Charts/custom.md)
- [22]-[BASEMAP](.planning/Charts/basemap.md)
- [23]-[INSPECTOR](.planning/Editing/inspector.md)
- [24]-[TABLES](.planning/Editing/tables.md)
- [25]-[FORMS](.planning/Editing/forms.md)
- [26]-[HISTORY](.planning/Editing/history.md)
- [27]-[LIVEDATA](.planning/Editing/livedata.md)
- [28]-[GRAPH](.planning/Editing/graph.md)
- [29]-[NOTEBOOK](.planning/Document/notebook.md)
- [30]-[MEDIA](.planning/Document/media.md)
- [31]-[EXPORT](.planning/Document/export.md)
- [32]-[SYNC](.planning/Collab/sync.md)
- [33]-[ISSUES](.planning/Collab/issues.md)
- [34]-[TOUR](.planning/Collab/tour.md)
- [35]-[EVIDENCE](.planning/Diagnostics/evidence.md)
- [36]-[PROOF](.planning/Diagnostics/proof.md)
- [37]-[DEVLOOP](.planning/Diagnostics/devloop.md)
- [38]-[GOVERNOR](.planning/Diagnostics/governor.md)
- [39]-[TOKENS](.planning/Theme/tokens.md)
- [40]-[TYPOGRAPHY](.planning/Theme/typography.md)
- [41]-[MOTION](.planning/Theme/motion.md)
- [42]-[ASSETS](.planning/Theme/assets.md)
- [43]-[LOCALE](.planning/Theme/locale.md)

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
- `SkiaSharp.NativeAssets.Linux` — distribution-closure floor (central pin; the csproj carries the macOS natives)
- `SkiaSharp.NativeAssets.Linux.NoDependencies` — distribution-closure floor (central pin)
- `HarfBuzzSharp.NativeAssets.macOS`
- `HarfBuzzSharp.NativeAssets.Linux` — distribution-closure floor (central pin)
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
> `Wgpu` owns the WebGPU viewport over `Silk.NET.WebGPU`, `Silk.NET.WebGPU.Native.WGPU`, and `Silk.NET.WebGPU.Extensions.WGPU`; OpenXR composes the same device for immersive review, and `Avalonia.Skia` remains the viewport fallback floor.

[ASSETS_CONTENT]:
- `AsyncImageLoader.Avalonia`
- `FluentIcons.Avalonia`
- `FluentIcons.Common`
- `Markdig`
- `PanAndZoom`
- `Wacton.Unicolour`

[LAYOUT]:
- `Kiwi`

[MEDIA]:
- `FFmpeg.AutoGen`
- `HanumanInstitute.LibMpv`
- `HanumanInstitute.LibMpv.Avalonia`

[INPUT_FABRIC]:
- `HidSharp`
- `Silk.NET.Input`
- `Silk.NET.SDL`
- `Melanchall.DryWetMidi`

[DRAFTING_EXPORT]:
- `ACadSharp`
- `DocumentFormat.OpenXml`
- `lcmsNET`
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
- `QuikGraph`

[COLLABORATION_LOCALE]:
- `LoroCs`
- `MessageFormat`
- `Whisper.net`

[DEV_LOOP]:
- `ProDiagnostics`
- `HotAvalonia`
- `Avalonia.Markup.Xaml.Loader`

> [!NOTE]
> The Dev Loop family binds `Debug`-only with `PrivateAssets="all"`. `ProDiagnostics` mounts runtime tree, property, style, event, and layout inspection behind the `HotAvalonia` closure; `HotAvalonia` and `Avalonia.Markup.Xaml.Loader` own XAML reload and runtime inflation.

> [!NOTE]
> The theme and extended-control families bind Semi tokens, Ursa controls, NodeEditor graph editing, Mapsui overlays, Loro collaboration, ICU message formatting, and PDFsharp/MigraDoc export through their package rows.

> [!NOTE]
> Media and export engines bind through native-backed rows: FFmpeg encodes compositor frames, LibMpv decodes playback, lcmsNET manages print color, and Whisper.net produces offline captions.

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
